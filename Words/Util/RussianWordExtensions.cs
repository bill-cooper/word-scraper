using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RussianWordScraper.Util
{
    public static class RussianWordExtensions
    {
        public static string RemoveStressMarks(this string word)
        {
            return word
                    .Replace("а́", "а")
                    .Replace("е́", "е")
                    .Replace("у́", "у")
                    .Replace("о́", "о")
                    .Replace("ю́", "ю")
                    .Replace("ы́", "ы")
                    .Replace("и́", "и")
                    .Replace("я́", "я")
                    .Replace("э́", "э");
        }

        public static bool HasStressMarks(this string word)
        {
            return word.Contains("а́")  
                   || word.Contains("е́")
                   || word.Contains("у́")
                   || word.Contains("о́")
                   || word.Contains("ю́")
                   || word.Contains("ы́")
                   || word.Contains("и́")
                   || word.Contains("э́")
                   || word.Contains("я́")
                   || word.Contains("ё");
        }
        public static bool IsSameWord(this string word, string compareWord)
        {
            return word.RemoveStressMarks().Replace("ё", "е").ToLower() == compareWord.RemoveStressMarks().Replace("ё", "е").ToLower();
        }
    }
}
