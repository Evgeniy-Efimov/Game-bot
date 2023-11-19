using EngineProject.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Helpers
{
    //Methods for strings
    public static class StringHelper
    {
        private static string CyrillicLetters = "йцукенгшщзхъфывапролджэячсмитьбюё";
        private static string Numbers = "0123456789";
        private static int MaxDifference = 3; //Max error of comparing string

        //Check string is in list (using MaxDifference)
        public static bool IsTextMatchToList(string text, string[] list)
        {
            if (string.IsNullOrWhiteSpace(text)) return true; //sometimes can't get text from image
            if (!list.Any()) return true;
            foreach (var str in list)
            {
                if (StringHelper.AreCyrillicStringEqual(text, str))
                {
                    return true;
                }
            }
            return false;
        }

        //Get index of first number in string (for weight parsing)
        public static int GetIndexOfFirstNumber(string str)
        {
            var firstNumberIndex = 0;
            for (int i = 0; i < str.Count(); i++)
            {
                if (Numbers.Contains(str[i]))
                {
                    firstNumberIndex = i;
                    i = str.Count();
                }
            }
            return firstNumberIndex;
        }

        //Parse text to int removing incorrect symbols
        public static int GetNumbersFromText(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return 0;
            try
            {
                return int.Parse(string.Join("", str.Where(c => Numbers.Contains(c))));
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #region Utility methods

        private static bool AreCyrillicStringEqual(string str1, string str2)
        {
            return ComputeDifference(GetOnlyCyrillicLetters(str1), GetOnlyCyrillicLetters(str2)) <= MaxDifference;
        }

        private static string GetOnlyCyrillicLetters(string str)
        {
            str = str.ToLower();
            return string.Join("", str.Where(c => CyrillicLetters.Contains(c)));
        }

        private static int ComputeDifference(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

        #endregion

    }
}
