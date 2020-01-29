using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ODK.Core.Utils
{
    public static class StringUtils
    {
        private static readonly Regex AlphaNumericRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
        private static readonly Regex TokenRegex = new Regex(@"\{(.+?)\}", RegexOptions.Compiled);

        public static string AlphaNumeric(this string text)
        {
            return AlphaNumericRegex.Replace(text, "");
        }

        public static string Interpolate(this string text, IDictionary<string, string> values)
        {
            StringBuilder sb = new StringBuilder(text);

            IEnumerable<string> tokens = text.Tokens();

            foreach (string token in tokens.Where(values.ContainsKey))
            {
                sb.Replace($"{{{token}}}", values[token]);
            }

            return sb.ToString();
        }

        public static IEnumerable<string> Tokens(this string text)
        {
            MatchCollection matches = TokenRegex.Matches(text);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }
    }
}
