using System;
using System.Collections.Generic;
using System.Linq;

namespace Words
{
    public static class WordMapExtensions
    {
        public static bool SequenceEqual(this string query, string other, ISet<char> exclude = null)
        {
            if (query.Length != other.Length)
                return false;

            exclude = exclude ?? new HashSet<char>(exclude.Concat(query.Where(Char.IsLetter)).Distinct());
            
            for (int i = 0; i < other.Length; i++)
            {
                if (query[i] == other[i])
                    continue;

                if (query[i] == '.' && !exclude.Contains(other[i]))
                    continue;

                return false;
            }
            return true;
        }

        public static bool Contains(this IDictionary<char, int> map, IDictionary<char, int> other, int wild = 0)
        {
            var wildHits = 0;

            return other.All(kvOther => (map.ContainsKey(kvOther.Key) && map[kvOther.Key] >= kvOther.Value) || wildHits++ < wild);
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
