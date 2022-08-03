using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MyCodeLibrary.TextProcessing
{
    /// <summary>
    /// Различные функции текстовых строк
    /// </summary>
    public class CStringProcessor
    {
        //TODO: перебросить функции отсюда в HTMLprocessor
        
        /// <summary>
        /// RT-Конвертировать блок данных в строку
        /// </summary>
        /// <param name="bar">Конвертируемый блок данных</param>
        /// <returns></returns>
        public static string BytesToString(byte[] bar)
        {
            MemoryStream ms = new MemoryStream(bar);
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8);

            String text = br.ReadString();
            br.Close();//close reader and stream
            return text;
        }

        /// <summary>
        /// RT-Конвертировать строку в блок данных
        /// </summary>
        /// <param name="text">Конвертируемая строка</param>
        /// <returns></returns>
        public static byte[] StringToBytes(string text)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8);
            bw.Write(text);
            bw.Flush();
            byte[] res = ms.ToArray();
            bw.Close();//close writer and stream

            return res;
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToHexString(uint val)
        {
            return val.ToString("X8");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static uint FromHexString(string val)
        {
            return uint.Parse(val, NumberStyles.HexNumber);
        }

		/// <summary>
        /// NT- Удалить из строки двойные пробелы, переносы строк, TAB итп.
        /// </summary>
        /// <param name="text">Входная строка</param>
        /// <returns></returns>
        public static string RemoveWhiteSpaces(string text)
        {
            StringBuilder sb = new StringBuilder(text.Trim());
            sb.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("  ", "");
            
            return sb.ToString();
        }

        /// <summary>
        /// NT-Заменить символы на пробелы и удалить все, что более одного пробела подряд.
        /// </summary>
        /// <param name="text">Обрабатываемый текст.</param>
        /// <param name="vs">Массив символов, заменяемых на пробелы.</param>
        /// <returns>Функция возвращает строку в которой все указанные символы заменены пробелами, а последовательность пробелом заменена одним пробелом.</returns>
        /// <example>
        /// <code>
        /// String result = removeTwoMoreWhitespaces(" A \t bar \t \tB ", new char[]{'b','a','r'});
        /// //result == " A B "
        /// </code>
        /// </example>
        public static string removeTwoMoreWhitespaces(string text, char[] vs)
        {
            StringBuilder sb = new StringBuilder();
            Char lastChar = Char.MinValue;

            foreach(Char c in text)
            {
                //char is whitespace
                bool r1 = Char.IsWhiteSpace(c);
                //char found in vs array
                bool r2 = (Array.IndexOf(vs, c) != -1);
                //TODO: тут надо переработать, но что-то не думается никак.
                Char c1 = c;
                if(r1 || r2)
                {
                    c1 = ' ';
                    if(lastChar != c1)
                    {
                        sb.Append(c1);
                        lastChar = c1;
                    }
                }
                else
                {
                    sb.Append(c1);
                    lastChar = c1;
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// NT- Заменить все найденные подстроки во входной строке на указаное значение.
        /// </summary>
        /// <param name="text">Входная строка</param>
        /// <param name="substrings">Массив заменяемых подстрок</param>
        /// <param name="replace">Строка замены</param>
        /// <returns>Функция возвращает строку, в которой все указанные подстроки заменены указанной строкой замены.</returns>
        /// <example>
        /// <code>
        /// String result = ReplaceSubStrings(htmlText, new String[] {"A", "B", "C" }, "Замена");
        /// </code> 
        /// </example>
        public static string ReplaceSubStrings(String text, String[] substrings, String replace)
        {
            StringBuilder sb = new StringBuilder(text);
            foreach(String s in substrings)
                sb.Replace(s, replace);

            return sb.ToString();
        }

        /// <summary>
        /// NT-Get part of input string between start pattern an end pattern v2.
        /// </summary>
        /// <param name="text">Input string</param>
        /// <param name="v1">Start pattern</param>
        /// <param name="v2">End pattern</param>
        /// <returns>
        /// Function returns substring between start and end patterns.
        /// Function returns null if start or end patterns not founded in input string.
        /// </returns>
        /// <example>
        /// <code>
        /// String text = "AbetweenZ";
        /// String part = getPartBetween(text, "A", "Z");
        /// //part == "between"
        /// </code>
        /// </example>
        public static string GetPartBetween(string text, string v1, string v2)
        {
            int start = text.IndexOf(v1);
            if(start == -1)
                return null;//returns null if start pattern not found 
            //else
            start = start + v1.Length;
            int end = text.IndexOf(v2, start);
            if(end == -1)
                return null;//returns null if end pattern not found
            //else
            string result = text.Substring(start, end - start);

            return result;
        }

        #region отобрать полезные функции

        /// <summary>
        /// 	Checks if a string is null or empty and returns a default value if fails
        /// </summary>
        /// <param name = "value"></param>
        /// <param name = "defaultValue"></param>
        /// <returns>Either the string or the default value.</returns>
        public static String IfNullOrEmpty(String value, String defaultValue)
        {
            return (String.IsNullOrEmpty(value) ? defaultValue : value);
        }
        /// <summary>
        /// It returns true if string is null or empty or just a white space otherwise it returns false
        /// </summary>
        /// <param name = "input">Target reference. Can be null.</param>
        /// <returns></returns>
        public static Boolean IsNullOrEmptyOrWhiteSpace(String input)
        {
            return String.IsNullOrEmpty(input) || String.IsNullOrEmpty(input.Trim());
        }
        /// <summary>
        /// Checks if a string is an valid URL
        /// </summary>
        /// <param name = "input">Target reference.</param>
        /// <returns></returns>
        public static Boolean IsValidUrl(String input)
        {
            return Regex.IsMatch(input, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Compiled);
        }
        /// <summary>
        /// Checks if a string is a valid IPv4 address
        /// </summary>
        /// <param name = "input">Target reference.</param>
        /// <returns></returns>
        public static Boolean IsValidIPAddress(String input)
        {
            return Regex.IsMatch(input,
                    @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", RegexOptions.Compiled);
        }
        /// <summary>
        /// Checks if a string is an valid e-mail address
        /// </summary>
        /// <param name = "input">Target reference.</param>
        /// <returns></returns>
        public static Boolean IsValidEmailAddress(String input)
        {
            return Regex.IsMatch(input, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", RegexOptions.Compiled);
        }


         /// <summary>
        /// 	Ensures that a string starts with a given prefix.
        /// </summary>
        /// <param name = "input"></param>
        /// <param name = "prefix"></param>
        /// <param name = "ignoreCase"></param>
        /// <returns>The string value including the prefix</returns>
        /// <example>
        /// 	<code>
        /// 		var extension = "txt";
        /// 		var fileName = string.Concat(file.Name, extension.EnsureStartsWith(".", true));
        /// 	</code>
        /// </example>
        public static String EnsureStartsWith(String input, String prefix, Boolean ignoreCase)
        {
            return input.StartsWith(prefix, ignoreCase, CultureInfo.CurrentCulture) ? input : String.Concat(prefix, input);
        }
        /// <summary>
        /// 	Ensures that a string ends with a given suffix.
        /// </summary>
        /// <param name = "input"></param>
        /// <param name = "suffix"></param>
        /// <param name = "ignoreCase"></param>
        /// <returns>The string value including the suffix</returns>
        /// <example>
        /// 	<code>
        /// 		var url = "http://www.google.com";
        /// 		url = url.EnsureEndsWith("/", true));
        /// 	</code>
        /// </example>
        public static String EnsureEndsWith(String input, String suffix, Boolean ignoreCase)
        {
            return input.EndsWith(suffix, ignoreCase, CultureInfo.CurrentCulture) ? input : String.Concat(input, suffix);
        }
        /// <summary>
        /// Converts a string to a Boolean. ArgumentException is thrown if fails
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean ToBoolean(String input)
        {
            var value = input.ToLower(CultureInfo.CurrentCulture).Trim();
            switch(value)
            {
                case "true":
                    return true;
                case "false":
                    return false;
                case "t":
                    return true;
                case "f":
                    return false;
                case "yes":
                    return true;
                case "no":
                    return false;
                case "y":
                    return true;
                case "n":
                    return false;
                case "1":
                    return true;
                case "0":
                    return false;
            }
            throw new ArgumentException("Input is not a boolean value.");
        }

        /// <summary>
        /// Removes the last number of characters from a string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static String RemoveLast(String input, int number)
        {
            return input.Substring(0, input.Length - number);
        }

        /// <summary>
        /// Removes the first number of characters from a string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static String RemoveFirst(String input, int number)
        {
            return input.Substring(number);
        }

        /// <summary>
        /// Removes all special characters from the string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The adjusted string.</returns>
        public static String RemoveAllSpecialCharacters(String input)
        {
            StringBuilder sb = new System.Text.StringBuilder(input.Length);
            foreach (char c in input)
                if(Char.IsLetterOrDigit(c))
                    sb.Append(c);

            return sb.ToString();
        }
        /// <summary>
        /// 	Reverses / mirrors a string.
        /// </summary>
        /// <param name = "input"></param>
        /// <returns>The reversed string</returns>
        public static String Reverse(String input)
        {
            if (String.IsNullOrEmpty(input) || (input.Length == 1))
                return input;

            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new String(chars);
        }
        /// <summary>
        /// Returns the first part of the string, up until the character c. If c is not found in the
        /// string the whole string is returned.
        /// </summary>
        /// <param name="input">String to truncate</param>
        /// <param name="c">Separator</param>
        public static String LeftOf(String input, Char c)
        {
            int ndx = input.IndexOf(c);
            if (ndx >= 0)
            {
                return input.Substring(0, ndx);
            }

            return input;
        }
        /// <summary>
        /// Returns right part of the string, after the character c. If c is not found in the
        /// string the whole string is returned.
        /// </summary>
        /// <param name="input">String to truncate</param>
        /// <param name="c">Separator</param>
        public static String RightOf(String input, Char c)
        {
            int ndx = input.IndexOf(c);
            if (ndx == -1)
                return input;
            return input.Substring(ndx + 1);
        }
        /// <summary>
        /// Returns true if string does not start with the pattern, otherwise false. If patern is null or empty, false will be returned.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static Boolean DoesNotStartWith(String input, String pattern)
        {
            return String.IsNullOrEmpty(pattern) ||
                   CStringProcessor.IsNullOrEmptyOrWhiteSpace(input) ||
                   !input.StartsWith(pattern, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Returns true if string does not end with the pattern, otherwise false. If patern is null or empty, false will be returned.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static Boolean DoesNotEndWith(String input, String pattern)
        {
            return String.IsNullOrEmpty(pattern) ||
                   CStringProcessor.IsNullOrEmptyOrWhiteSpace(input) ||
                     !input.EndsWith(pattern, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Returns first character in a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String FirstChar(String input)
        {
            if (!String.IsNullOrEmpty(input))
            {
                return (input.Length >= 1) ? input.Substring(0, 1) : input;
            }
            return null;
        }
        /// <summary>
        /// Returns last character in a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String LastChar(String input)
        {
            if (!String.IsNullOrEmpty(input))
            {
                return (input.Length >= 1) ? input.Substring(input.Length - 1, 1) : input;
            }
            return null;
        }
        /// <summary>
        /// Returns first number of characters in string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static String FirstChars(String input, int number)
        {
            if (String.IsNullOrEmpty(input)) return input;
            return (input.Length < number ? input : input.Substring(0, number));
        }
        /// <summary>
        /// Returns string with first char upercase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String UppercaseFirst(String input)
        {
            // Check for empty string.
            if (String.IsNullOrEmpty(input)) return string.Empty;
            // Return char and concat substring.
            return char.ToUpper(input[0], CultureInfo.CurrentCulture) + input.Substring(1);
        }

        #endregion
    }
}
