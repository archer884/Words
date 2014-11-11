using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
        /// Gets a list of solutions fitting the current known letters.
        /// </summary>
        Query
    }

    class Program
    {
        static readonly string _wordListPath = ConfigurationManager.AppSettings["WordListPath"];
        static readonly int _minimumLength = Int32.Parse(ConfigurationManager.AppSettings["MinimumLength"]);
        static readonly int _maximumLength = Int32.Parse(ConfigurationManager.AppSettings["MaximumLength"]);
        static readonly IList<WordMap> _wordList = GetWords().Where(word => word.Length >= _minimumLength && word.Length <= _maximumLength).Select(WordMap.Build).ToList();
        static readonly WordMapComparer _comparer = new WordMapComparer();

        static void Main(string[] args)
        {
            if (!args.Any() || args.All(String.IsNullOrWhiteSpace))
            {
                foreach (var word in _wordList.Select(w => w.Word))
                {
                    Console.WriteLine(word);
                }
            }

            Command command;
            bool commandIncluded = false;
            if (!(commandIncluded = Enum.TryParse<Command>(args[0], true, out command)))
                command = args[0][0] == '/' ? Command.Query : Command.Get;

            RunCommand(command, args.Skip(commandIncluded ? 1 : 0));
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

            var source = WordMap.Build(args.Single());
            var validWords = _wordList
                .AsParallel()
                .Where(target => source.Map.Contains(target.Map))
                .OrderByDescending(target => target, _comparer) // in order of difficulty
                .ThenByDescending(target => target.Word);       // in reverse alphabetical order

            foreach (var word in validWords)
            {
                Console.WriteLine(word);
            }
        }

        static void Query(IList<string> args)
        {
            var query = args[0].TrimStart('/').ToLower(); // / is used to signal a query without passing a command parameter
            var source = WordMap.Build(query);
            var exclude = String.Format("[^{0}]", String.Concat(args.Count > 1 ? source.Letters.Concat(args[1]) : source.Letters));
            var regex = new Regex(String.Format("^{0}$", query).Replace(".", exclude), RegexOptions.Compiled|RegexOptions.IgnoreCase);
            var validWords = _wordList
                .AsParallel()
                .Where(target => regex.IsMatch(target.Word) && source.Word.IndistinctIntersect(target.Word).SequenceEqual(source.Letters))
                .OrderByDescending(target => target, _comparer) // in order of difficulty
                .ThenBy(target => target.Word);                 // in alphabetical order

            foreach (var word in validWords)
            {
                Console.WriteLine(word);
            }
        }

        static IEnumerable<string> GetWords()
        {
            if (Directory.Exists(_wordListPath))
                return Directory.EnumerateFiles(_wordListPath)
                    .SelectMany(file => File.ReadLines(file).Where(line => !String.IsNullOrWhiteSpace(line)).Select(s => s.ToLower()))
                    .Distinct();

            else throw new ApplicationException("Could not read word lists.");
        }
    }
}
