using System.Collections.Generic;
using System.Linq;

namespace Words
{
    class WordMapComparer : IComparer<WordMap>
    {
        static readonly char[] Vowels = new[] { 'A', 'E', 'I', 'O', 'U' };

        public int Compare(WordMap x, WordMap y)
        {
            return WordValue(x).CompareTo(WordValue(y));
        }

        private static double WordValue(WordMap word)
        {
            return word.Map.Sum(kv => (IsVowel(kv.Key) ? 1 : 1.5) / kv.Value);
        }

        private static bool IsVowel(char c)
        {
            return Vowels.Contains(c);
        }
    }
}
