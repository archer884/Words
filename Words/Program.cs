﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Words
{
    enum Command 
    { 
        /// <summary>
        /// Adds a word to Word's database (by inserting it into an ad-hoc file in the base word db directory).
        /// </summary>
        Add,

        /// <summary>
        /// Generates a puzzle using the provided letters.
        /// </summary>
        Get,

        /// <summary>
        /// Allows output of Words to be piped back into words. Useful for scrabble.
        /// </summary>
        Filter,

        /// <summary>
        /// Gets a list of solutions fitting the current known letters.
        /// </summary>
        Query
    }

    class Program
    {
        static readonly string _wordListPath = ConfigurationManager.AppSettings["WordListPath"];
        static readonly int _minimumLength = Int32.Parse(ConfigurationManager.AppSettings["MinimumLength"]);
        static readonly int _maximumLength = Int32.Parse(ConfigurationManager.AppSettings["MaximumLength"]);
        static readonly WordMapComparer _comparer = new WordMapComparer();

        static IEnumerable<WordMap> WordList 
        {
            get { return _wordList.Value; }
        }
        static Lazy<IList<WordMap>> _wordList = new Lazy<IList<WordMap>>(() => CreateMaps(ReadFiles()));

        static void Main(string[] args)
        {
            if (!args.Any() || args.All(String.IsNullOrWhiteSpace))
                foreach (var word in WordList.Select(w => w.Word))
                    Console.WriteLine(word);

            else
            {
                Command command;
                bool commandIncluded = false;
                if (!(commandIncluded = Enum.TryParse<Command>(args[0], true, out command)))
                    command = args[0][0] == '/' ? Command.Query : Command.Get;

                RunCommand(command, args.Skip(commandIncluded ? 1 : 0));
            }
        }

        static void RunCommand(Command command, IEnumerable<string> args)
        {
            switch (command)
            {
                case Command.Add:
                    Add(args.ToList());
                    return;

                case Command.Get:
                    Get(args.ToList());
                    return;

                case Command.Filter:
                    Filter(args.ToList());
                    return;

                case Command.Query:
                    Query(args.ToList());
                    return;
            }
        }

        static void Add(IList<string> args)
        {
            File.AppendAllLines(Path.Combine(_wordListPath, "ad-hoc.txt"), args);
        }

        static void Get(IList<string> args)
        {
            if (args.Count != 1)
            {
                Console.WriteLine("Just one query, please.");
                return;
            }

            var input = args[0].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var wild = input.Length > 1 ? Int32.Parse(input[0]) : 0;
            var query = input.Length > 1 ? input[1] : input[0];
            var source = WordMap.Build(query);

            var validWords = WordList.AsParallel()
                .Where(target => source.Map.Contains(target.Map, wild))
                .OrderByDescending(target => target, _comparer) // in order of difficulty
                .ThenByDescending(target => target.Word);       // in reverse alphabetical order

            foreach (var word in validWords)
            {
                Console.WriteLine(word);
            }
        }

        static void Filter(IList<string> args)
        {
            _wordList = new Lazy<IList<WordMap>>(() => CreateMaps(ReadStandardIn()));
            Query(args);
        }

        static void Query(IList<string> args)
        {
            var query = args[0].TrimStart('/');
            var excludes = new HashSet<char>((args.Count > 1 ? args[1] : Enumerable.Empty<char>()).Concat(query.Where(Char.IsLetter)).Distinct());

            var validWords = WordList.AsParallel()
                .Where(target => query.SequenceEqual(target.Word, excludes))
                .OrderByDescending(target => target, _comparer) // in order of difficulty
                .ThenBy(target => target.Word);                 // in alphabetical order

            foreach (var word in validWords)
            {
                Console.WriteLine(word);
            }
        }

        static IEnumerable<string> ReadFiles()
        {
            if (Directory.Exists(_wordListPath))
                return Directory.EnumerateFiles(_wordListPath)
                    .SelectMany(file => File.ReadLines(file).Where(line => !String.IsNullOrWhiteSpace(line)).Select(s => s.ToLower()))
                    .Distinct();

            else throw new ApplicationException("Could not read word lists.");
        }

        static IEnumerable<string> ReadStandardIn()
        {
            string line;
            while (!String.IsNullOrWhiteSpace(line = Console.ReadLine()))
                yield return line;
        }

        static IList<WordMap> CreateMaps(IEnumerable<string> words)
        {
            return words.Where(word => word.Length >= _minimumLength && word.Length <= _maximumLength).Select(WordMap.Build).ToList();
        }
    }
}
