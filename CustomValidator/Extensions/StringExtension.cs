using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CustomValidator.Extensions
{
    /// <summary>
    /// String extensions.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Returns true if string is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// Returns true if string is null, empty or only whitespaces.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// Returns specified value if string is null/empty/whitespace else same string.
        /// </summary>
        public static string Or(this string s, string or)
        {
            if (!s.IsNullOrWhiteSpace())
            {
                return s;
            }
            return or;
        }

        /// <summary>
        /// Returns empty if string is null/empty/whitespace else same string.
        /// </summary>
        public static string OrEmpty(this string s)
        {
            return s.Or("");
        }

        /// <summary>
        /// Returns null if string is null/empty else same string.
        /// </summary>
        public static string NullIfEmpty(this string s)
        {
            return !s.IsNullOrEmpty() ? s : null;
        }

        /// <summary>
        /// Returns null if string is null/empty/whitespace else same string.
        /// </summary>
        public static string NullIfWhiteSpace(this string s)
        {
            return !s.IsNullOrWhiteSpace() ? s : null;
        }

        /// <summary>
        /// Returns html-encoded string.
        /// </summary>
        public static string HtmlEncode(this string s)
        {
            return WebUtility.HtmlEncode(s);
        }

        /// <summary>
        /// Returns html-decoded string.
        /// </summary>
        public static string HtmlDecode(this string s)
        {
            return WebUtility.HtmlDecode(s);
        }

        /// <summary>
        /// Returns url-encoded string.
        /// </summary>
        public static string UrlEncode(this string s)
        {
            return WebUtility.UrlEncode(s);
        }

        /// <summary>
        /// Returns url-decoded string.
        /// </summary>
        public static string UrlDecode(this string s)
        {
            return WebUtility.UrlDecode(s);
        }

        /// <summary>
        /// Formats string as string.Format() but the parameter index is optional and parameter meta is allowed.
        /// </summary>
        /// <example>"{} is a {1}".format("this", "test")</example>
        public static string format(this string format, params object[] args)
        {
            var buffer = new StringBuilder(format);
            var i = 0;
            var param = 0;
            var paramMetaToIndexMap = new Dictionary<string, string>();
            while (i < buffer.Length)
            {
                // close curly before open curly
                if (buffer[i] == '}')
                {
                    // skip escaped curly "}}", else break
                    if (i + 1 < buffer.Length && buffer[i + 1] == '}')
                    {
                        i += 2;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                // stop at '{'
                if (buffer[i] != '{')
                {
                    i++;
                    continue;
                }
                // skip escaped curly "{{"
                if (i + 1 < buffer.Length && buffer[i + 1] == '{')
                {
                    i += 2;
                    continue;
                }
                var start = i;
                while (i < buffer.Length && buffer[i] != '}')
                {
                    i++;
                }
                // open curly is not matched, break
                if (i == buffer.Length)
                {
                    break;
                }
                var end = i;
                //// at this point buffer[start..end] has format field
                //// insert parameter index only if not present
                ////     so insert for:
                ////     se  s  e  s            e  s    e  s      e  s                e
                ////     {}  {:2}  {,10:#,##0.00}  {test}  {test:2}  {test,10:#,##0.00}
                ////     and do not insert for:
                ////     s e  s   e  s             e
                ////     {0}  {0:2}  {0,10:#,##0.00}
                // find the meta substring
                var metastart = start + 1;
                var metaend = metastart;
                while (buffer[metaend] != '}' && buffer[metaend] != ':' && buffer[metaend] != ',')
                {
                    metaend++;
                }
                var paramMeta = buffer.ToString(metastart, metaend - metastart).Trim();
                // insert param only if meta is not int
                int ignore;
                if (!int.TryParse(paramMeta, out ignore))
                {
                    string paramIndex;
                    // do not insert "" into meta->index map
                    if (paramMeta == "")
                    {
                        paramIndex = param.ToString();
                        param++;
                    }
                    else
                    {
                        // remove meta
                        buffer.Remove(metastart, paramMeta.Length);
                        // insert param index into meta->index map if not exists
                        if (!paramMetaToIndexMap.ContainsKey(paramMeta))
                        {
                            paramMetaToIndexMap[paramMeta] = param.ToString();
                            param++;
                        }
                        // do not increment param as param index is reused
                        paramIndex = paramMetaToIndexMap[paramMeta];
                    }
                    // insert index
                    buffer.Insert(metastart, paramIndex);
                    // adjust end as buffer is removed from and inserted into
                    end += -paramMeta.Length + paramIndex.Length;
                }
                else
                {
                    param++;
                }
                i = end + 1; // i++ does not work
            }
            var formatConverted = buffer.ToString();
            return string.Format(formatConverted, args);
        }

        /// <summary>
        /// Try-parses string to bool, else default value.
        /// </summary>
        public static bool ToBool(this string value, bool defaultValue)
        {
            bool result;
            if (bool.TryParse(value, out result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Munges substitutions.
        /// </summary>
        private static List<KeyValuePair<string, string>> mungeUnmungeSubstitutions =
            GetMungeUnmungeSubstitutions();
        private static List<KeyValuePair<string, string>> GetMungeUnmungeSubstitutions()
        {
            var mungeSubstitutions = new List<KeyValuePair<string, string>>();
            new[]
            {
                new[] {"a", "@"},
                new[] {"b", "8"},
                new[] {"c", "("},
                new[] {"d", "6"},
                new[] {"e", "3"},
                new[] {"f", "#"},
                new[] {"g", "9"},
                new[] {"h", "#"},
                new[] {"i", "1"},
                new[] {"i", "!"},
                new[] {"k", "<"},
                new[] {"l", "1"},
                new[] {"l", "i"},
                new[] {"o", "0"},
                new[] {"q", "9"},
                new[] {"s", "$"},
                new[] {"s", "5"},
                new[] {"t", "+"},
                new[] {"v", ">"},
                new[] {"v", "<"},
                new[] {"w", "uu"},
                new[] {"w", "2u"},
                new[] {"x", "%"},
                new[] {"y", "?"},
            }.ToList().ForEach(x => mungeSubstitutions.Add(new KeyValuePair<string, string>(x[0], x[1])));
            return mungeSubstitutions;
        }

        /// <summary>
        /// key->value[] map.
        /// </summary>
        private static IDictionary<string, string[]> mungeMap =
            mungeUnmungeSubstitutions
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Value)
                .ToArray());

        /// <summary>
        /// value->key[] map.
        /// </summary>
        private static IDictionary<string, string[]> unmungeMap =
            mungeUnmungeSubstitutions
                .GroupBy(x => x.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Key)
                .ToArray());

        /// <summary>
        /// Munges or unmunges password as per substitution map.
        /// </summary>
        private static IList<string> MungeUnmunge(this string password,
            IDictionary<string, string[]> muMap, bool isMunging)
        {
            password.ThrowIfArgumentNull(nameof(password));
            var items = new List<string>();
            MungeUnmunge
            (
                password, muMap, isMunging, items,
                0, new StringBuilder(password.Length), new Dictionary<string, string>()
            );
            return items;
        }

        /// <summary>
        /// Munges or unmunges recursively.
        /// </summary>
        /// <param name="password">Password to munge/unmunge.</param>
        /// <param name="muMap">Substitution map.</param>
        /// <param name="isMunging">True if munging, false if unmunging.</param>
        /// <param name="items">List to hold the passwords.</param>
        /// <param name="index">Index (start) of currently processed part.</param>
        /// <param name="buffer">Buffer to hold the munge/unmunged password.</param>
        /// <param name="partReplaceMap">Map to hold part replacements.</param>
        private static void MungeUnmunge(string password,
            IDictionary<string, string[]> muMap, bool isMunging, List<string> items,
            int index, StringBuilder buffer, Dictionary<string, string> partReplaceMap)
        {
            if (index == password.Length)
            {
                items.Add(buffer.ToString());
                return;
            }
            Func<string, int, int, string> substring = (pwd, idx, len) =>
            {
                if (idx + len > pwd.Length)
                {
                    return null;
                }
                return pwd.Substring(idx, len);
            };
            // look ahead 1, 2 chars only
            for (int length = 1; length <= 2; length++)
            {
                var part = substring(password, index, length);
                foreach (var muSubstitute in
                    GetMungeUnmungeParts(part, muMap, isMunging, partReplaceMap))
                {
                    buffer.Insert(index, muSubstitute);
                    partReplaceMap[part] = muSubstitute;
                    MungeUnmunge(password, muMap, isMunging, items, index + length, buffer, partReplaceMap);
                    buffer.Length -= muSubstitute.Length;
                    partReplaceMap.Remove(part);
                }
            }
        }

        /// <summary>
        /// Returns the munged/unmunged parts for part.
        /// </summary>
        /// <example>munge("o"), returns {"o", "0"}.</example>
        /// <example>Unmunge("0"), returns {"o"}.</example>
        private static IEnumerable<string> GetMungeUnmungeParts(string part,
            IDictionary<string, string[]> muMap, bool isMunging,
            Dictionary<string, string> partReplaceMap)
        {
            if (part == null)
            {
                yield break;
            }
            // yield same replacement for part as before
            if (partReplaceMap.ContainsKey(part))
            {
                yield return partReplaceMap[part];
            }
            else if (part.Length == 1)
            {
                // yield same part only if munging and length 1
                if (isMunging)
                {
                    yield return part;
                }
                // yield if in muMap
                string[] muParts;
                if (muMap.TryGetValue(part, out muParts))
                {
                    foreach (var muPart in muParts)
                    {
                        yield return muPart;
                    }
                }
                else
                {
                    // yield only if not munging and not in muMap
                    if (!isMunging)
                    {
                        yield return part;
                    }
                }
            }
            else if (part.Length == 2)
            {
                // yield only if in muMap
                string[] muParts;
                if (muMap.TryGetValue(part, out muParts))
                {
                    foreach (var muPart in muParts)
                    {
                        yield return muPart;
                    }
                }
            }
        }

        /// <summary>
        /// Munges a password (up to two chars).
        /// </summary>
        /// <remarks>http://en.wikipedia.org/wiki/Munged_password</remarks>
        public static IEnumerable<string> Munge(this string password)
        {
            var items = MungeUnmunge(password, mungeMap, true);
            // 1st item is same as password
            if (items.Any() && items[0] == password)
            {
                items.RemoveAt(0);
            }
            return items.AsEnumerable();
        }

        /// <summary>
        /// Unmunges a (munged) password (up to two chars).
        /// </summary>
        /// <remarks>http://en.wikipedia.org/wiki/Munged_password</remarks>
        public static IEnumerable<string> Unmunge(this string password)
        {
            var items = MungeUnmunge(password, unmungeMap, false);
            // remove 1st item if same as password
            if (items.Any() && items[0] == password)
            {
                items.RemoveAt(0);
            }
            return items.AsEnumerable();
        }

        /// <summary>
        /// Returns a new collection with characters scrabbled like the game.
        /// </summary>
        public static IEnumerable<string> Scrabble(this string word)
        {
            word.ThrowIfArgumentNull(nameof(word));
            return word.Scrabble<char>().Select(x => new string(x));
        }

        /// <summary>
        /// Returns a new collection with characters scrabbled like the game for particular r.
        /// </summary>
        public static IEnumerable<string> Scrabble(this string word, int limit)
        {
            word.ThrowIfArgumentNull(nameof(word));
            limit.ThrowIfArgumentOutOfRange(nameof(limit), maxRange: word.Length);
            return word.Scrabble<char>(limit).Select(x => new string(x));
        }

        /// <summary>
        /// Parses a string in UTC format as DateTime.
        /// </summary>
        public static DateTime ParseAsUtc(this string s)
        {
            return DateTimeOffset.Parse(s).UtcDateTime;
        }

        /// <summary>
        /// Converts a string to title case.
        /// </summary>
        /// <example>"war and peace" -> "War And Peace"</example>
        public static string ToTitleCase(this string s)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s);
        }

        /// <summary>
        /// Gets permutations of string for particular r.
        /// </summary>
        /// <remarks>nPr permutations</remarks>
        public static IEnumerable<string> Permutation(this string s, int r)
        {
            return s.Permutation<char>(r).Select(x => new string(x));
        }

        /// <summary>
        /// Gets permutations of string.
        /// </summary>
        /// <remarks>nPn permutations</remarks>
        public static IEnumerable<string> Permutation(this string s)
        {
            return s.Permutation(s.Length);
        }

        /// <summary>
        /// Gets combinations of string for particular r.
        /// </summary>
        /// <remarks>nCr combinations</remarks>
        public static IEnumerable<string> Combination(this string s, int r)
        {
            return s.Combination<char>(r).Select(x => new string(x));
        }

        /// <summary>
        /// Gets combinations of string.
        /// </summary>
        /// <remarks>nCn combinations</remarks>
        public static IEnumerable<string> Combination(this string s)
        {
            return s.Combination(s.Length);
        }

        /// <summary>
        /// Splits csv string using common delimiters.
        /// </summary>
        public static IEnumerable<string> SplitCsv(this string s)
        {
            return s.Split(new[] { ",", ";", "|" }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns substring from start of length <paramref name="n"/>.
        /// </summary>
        public static string SubstringFromStart(this string source, int n)
        {
            source.ThrowIfArgumentNull(nameof(source));
            n.ThrowIfArgumentOutOfRange(nameof(n));
            if (n >= source.Length)
            {
                return source;
            }
            return source.Substring(0, n);
        }

        /// <summary>
        /// Returns substring till end of length <paramref name="n"/>.
        /// </summary>
        public static string SubstringTillEnd(this string source, int n)
        {
            source.ThrowIfArgumentNull(nameof(source));
            n.ThrowIfArgumentOutOfRange(nameof(n));
            if (n >= source.Length)
            {
                return source;
            }
            return source.Substring(source.Length - n);
        }

        /// <summary>
        /// Returns substring by specifying start index and end index.
        /// </summary>
        public static string SubstringByIndex(this string source, int startIndex, int endIndex)
        {
            source.ThrowIfArgumentNull(nameof(source));
            startIndex.ThrowIfArgumentOutOfRange(nameof(startIndex));
            endIndex.ThrowIfArgumentOutOfRange(nameof(endIndex), maxRange: source.Length);
            return source.Substring(startIndex, endIndex - startIndex);
        }

        #region Capitalize

        /// <summary>
        /// Return a capitalized version of given input string i.e. make the first character have upper case and the rest lower case.
        /// </summary>
        /// <param name="input">input string parameter</param>
        /// <returns>String with first letter in upper case</returns>
        public static string Capitalize(this string input)
        {
            if (input.Length > 0)
            {
                char[] array = input.ToLower().ToCharArray();
                array[0] = char.ToUpper(array[0]);
                return new string(array);
            }
            return input;
        }

        #endregion

        #region GetOccurrenceCount

        // Usage: var count = "supercalifragilisticexpealidocious".GetOccurrenceCount("li"); // returns 3
        /// <summary>
        /// Get the occurence count of a substring in a given input string
        /// </summary>
        /// <param name="input">input string parameter in which a occurence needs to be identified</param>
        /// <param name="searchText">input search text</param>
        /// <returns>integer value returning the number of occurences of a serachText in main input string</returns>
        public static int GetOccurrenceCount(this string input, string searchText)
        {
            if (input.Trim() == string.Empty && searchText.Trim() == string.Empty)
                return 1;
            else if (input.Trim() == string.Empty || searchText.Trim() == string.Empty)
                return 0;
            else
                return Regex.Matches(input, searchText).Count;
        }

        #endregion

        #region GetWordCount

        /// <summary>
        /// Get the word count from an input string
        /// </summary>
        /// <param name="input">input string parameter from which words needs to be counted</param>
        /// <returns>integer value indicating number of words</returns>
        public static int GetWordCount(this string input)
        {
            if (input.Trim().Length == 0)
                return 0;

            return Regex.Split(input, @"\W+").Length;
        }

        #endregion

        #region IsLower

        /// <summary>
        /// Find if all the characters from given input string are in lower case or not
        /// </summary>
        /// <param name="input">input string whose individual characters needs to be checked</param>
        /// <returns>Boolen result (true / false) indicating whether all the characters are in lower case or not</returns>
        public static bool IsLower(this string input)
        {
            char[] chars = input.ToCharArray();
            return chars.All(c => (char.IsLower(c) || char.IsWhiteSpace(c)));
        }

        #endregion

        #region IsUpper

        /// <summary>
        /// Find if all the characters from given input string are in upper case or not
        /// </summary>
        /// <param name="input">input string whose individual characters needs to be checked</param>
        /// <returns>Boolen result (true / false) indicating whether all the characters are in upper case or not</returns>
        public static bool IsUpper(this string input)
        {
            char[] chars = input.ToCharArray();
            return chars.All(c => (char.IsUpper(c) || char.IsWhiteSpace(c)));
        }

        #endregion

        #region IsEmail
        /// <summary>
        /// Function to identify whether given input string is a valid email address or not
        /// </summary>
        /// <param name="input">String input parameter to check for email validity</param>
        /// <returns>True if given input string is a valid email address, false otherwise</returns>
        public static bool IsEmail(this string input)
        {
            try
            {
                if (String.IsNullOrEmpty(input.Trim()))
                    throw new ArgumentNullException("Email address cannot be null or empty");

                MailAddress address = new MailAddress(input);
                return true;
            }
            catch (FormatException) // email is not in a recognized format OR email contains non-ASCII characters.
            {
                throw;
            }
        }

        #endregion

        #region IsNumeric

        /// <summary>
        /// Function to identify whether given input string can be converted to a valid numeric value or not
        /// </summary>
        /// <param name="input">Input string parameter</param>
        /// <returns>True if given input string is a valid numeric value, false otherwise</returns>
        public static bool IsNumeric(this string input)
        {
            double n;
            var isNumeric = Double.TryParse(input, out n);
            return isNumeric;
        }

        #endregion

        #region IsPalindrome

        /// <summary>
        /// Function to identify whether given input string is palindrome or not
        /// </summary>
        /// <param name="input">String input parameter</param>
        /// <returns>True if given input string is palindrome, false otherwise</returns>
        public static bool IsPalindrome(this string input)
        {
            var reverse = new string(input.ToCharArray().Reverse().ToArray());
            return input.Equals(reverse);
        }

        #endregion

        #region RemoveFirst

        /// <summary>
        /// Function to remove the given number of characters from input string
        /// </summary>
        /// <param name="input">string input</param>
        /// <param name="numberOfCharacteresToRemove">number of characters to remove from first</param>
        /// <returns>string with characters removed </returns>
        public static string RemoveFirst(this String input, int numberOfCharacteresToRemove)
        {
            if (input == string.Empty)
                throw new ArgumentException("Characters cannot be removed from an empty string.");

            return input.Substring(numberOfCharacteresToRemove);
        }

        #endregion

        #region RemoveFirstCharacter

        /// <summary>
        /// Function to remove the first character from given the string
        /// </summary>
        /// <param name="input">Input string from which first character needs to be removed</param>
        /// <returns>string with removed first character</returns>
        public static string RemoveFirstCharacter(this String input)
        {
            if (input == string.Empty)
                throw new ArgumentException("First character cannot be removed from an empty string.");

            return input.Substring(1);
        }

        #endregion

        #region RemoveLast

        /// <summary>
        /// Function to remove the given number of characters from end of input string
        /// </summary>
        /// <param name="input">string input</param>
        /// <param name="numberOfCharacteresToRemove">number of characters to remove from the end</param>
        /// <returns>string with characters removed </returns>
        public static string RemoveLast(this String input, int numberOfCharacteresToRemove)
        {
            if (input == string.Empty)
                throw new ArgumentException("Characters cannot be removed from an empty string.");

            return input.Substring(0, input.Length - numberOfCharacteresToRemove);
        }

        #endregion

        #region RemoveLastCharacter

        /// <summary>
        /// Function to remove the last character from given the string
        /// </summary>
        /// <param name="input">Input string from which last character needs to be removed</param>
        /// <returns>string with removed last character</returns>
        public static string RemoveLastCharacter(this String input)
        {
            if (input == string.Empty)
                throw new ArgumentException("Last character cannot be removed from an empty string.");

            return input.Substring(0, input.Length - 1);
        }

        #endregion

        #region SwapCase

        /// <summary>
        /// Return a copy of given input string with uppercase characters converted to lowercase and vice versa.
        /// </summary>
        /// <param name="input">Input string whose character case needs to be reversed</param>
        /// <returns>String with reverse characters</returns>
        public static string SwapCase(this string input)
        {
            return new string(input.Select
                                    (c => char.IsLetter(c) ? (char.IsUpper(c) ?
                                          char.ToLower(c) : char.ToUpper(c)) : c)
                                    .ToArray()
                             );
        }

        #endregion

        #region TrimAndReduce

        /// <summary>
        /// Extension method to remove all the repeated blank spaces within the String itself, not only at the end or at the start of it
        /// </summary>
        /// <param name="input">string input parameter with extra spaces</param>
        /// <returns>string with repeated blanks spaces removed</returns>
        public static string TrimAndReduce(this string input)
        {
            return Regex.Replace(input, @"\s+", " ").Trim();
        }

        #endregion

        #region ToInt

        /// <summary>
        /// Convert an input string to integer
        /// Usage: var count = strCount.ToInt();
        /// </summary>
        /// <param name="input">input string parameter which needs to be converted to integer</param>
        /// <returns>integer value of input string. Zero if input string cannot be converted to an integer</returns>
        public static int ToInt(this String input)
        {
            int result;
            if (!int.TryParse(input, out result))
            {
                result = 0;
            }
            return result;
        }

        #endregion
    }
}
