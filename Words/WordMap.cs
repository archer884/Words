using System;
using System.Collections.Generic;
using System.Linq;

namespace Words
{
    public static class WordMapExtensions
    {
        public static IEnumerable<T> IndistinctIntersect<T>(this IEnumerable<T> collection, IEnumerable<T> other)
        {
            return collection
                .Zip(other, (a, b) => new { a, b })
                .Where(p => p.a.Equals(p.b)).Select(p => p.a);
        }

        public static bool Contains(this IDictionary<char, int> map, IDictionary<char, int> other)
        {
            return other.All(kvOther => map.ContainsKey(kvOther.Key) && map[kvOther.Key] >= kvOther.Value);
        }
    }

    public class WordMap
    {
        public readonly string Word;
        public readonly IDictionary<char, int> Map;

        public IEnumerable<char> Letters 
        {
            get { return Word.Where(Char.IsLetter); }
        }


        public WordMap(string word)
        {
            Word = word;
            Map = BuildMap(word);
        }

        public static WordMap Build(string word)
        {
            return new WordMap(word);
        }

        public static IDictionary<char, int> BuildMap(string word)
        {
            return word.ToLower().Aggregate(new Dictionary<char, int>(), (map, c) =>
            {
                if (map.ContainsKey(c))
                    map[c]++;

                else map.Add(c, 1);

                return map;
            });
        }

        public override string ToString()
        {
            return this.Word;
        }
    }
}
