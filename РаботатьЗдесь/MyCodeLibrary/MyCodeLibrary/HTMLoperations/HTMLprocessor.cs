using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using MyCodeLibrary.TextProcessing;

namespace MyCodeLibrary.HTMLoperations
{
    /// <summary>
    /// NT-Функции обработки текста HTML файлов.
    /// </summary>
    public class HTMLprocessor
    {
        private static char[] tagsSeparator = new char[] { '<', '>' };
		
		/// <summary>
        /// Массив строк html-кодов и их текстовых замен.
        /// </summary>
        /// <remarks>
        /// Текстовая замена должна состоять из ASCII символов
        /// </remarks>
        private static string[,] htmlCharReplaces = {
                                                     {"&laquo;", "\""},
                                                     {"&raquo;", "\""},
                                                     {"&mdash;", "-"},
                                                     {"&nbsp;", " "},
                                                     {"&ndash;", "-"},
                                                     {"&ldquo;", "\""},
                                                     {"&rdquo;", "\""},
                                                     {"&hellip;", "..."},
                                                     {"&reg;", "(R)"},
                                                     {"&quot;", "\""},
                                                     {"&rsquo;", "'"},
                                                     {"&lsquo;", "'"},
                                                     {"&copy;", "(C)"},
                                                     {"&sup;", "^"},
                                                     {"&bdquo;", "\""},
                                                     {"&#039;", "'"},
                                                     {"&#39;", "'"},
                                                     //{"&eacute;", "a"},
                                                     {"&lt;", "<"},
                                                     {"&gt;", ">"},

                                                     //{}, добавить новые символы здесь
                                                     //{}, добавить новые символы здесь
                                                     //{"&amp;", "&"} - Заменять отдельным вызовом ДО или ПОСЛЕ остальных тегов , поскольку используется в кодах символов. 
                                                     };


		
		
        /// <summary>
        /// NT-Разделить строку тегов из Posts на отдельные теги без скобок.
        /// Теги еще надо потом дообработать: .Trim().ToLowerInvariant() для гарантии.
        /// </summary>
        /// <param name="input">Строка тегов</param>
        /// <returns>Возвращает массив строк тегов</returns>
        public static String[] parseTagString(String input)
        {
            return input.Split(tagsSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Собрать строку тегов через запятую и убрать угловые скобки
        /// </summary>
        /// <param name="tags">Строка тегов в угловых скобках</param>
        /// <returns></returns>
        public static string makeTagString(string tags)
        {
            String[] tagar = HTMLprocessor.parseTagString(tags);
            return String.Join(", ", tagar);
        }


        /// <summary>
        /// NT-Экранировать символы &lt;&gt;&quot;&equiv; для показа в HTML-файлах
        /// </summary>
        /// <param name="titleText"></param>
        /// <returns></returns>
        /// <remarks>
        /// Это плохая функция, она мало символов исправляет. 
        /// Лучше использовать более сложную функцию ниже
        /// </remarks>
        public static string getSafeHtmlText(String titleText)
        {
            String t = titleText.Replace("<", "&lt;").Replace(">", "&gt;");
            t = t.Replace("\"", "&quot;").Replace("=", "&equiv;");

            return t;
        }
        /// <summary>
        /// NT-Экранировать символы &lt;&gt;&quot;&equiv; для показа в HTML-файлах
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string getSafeHtmlText2(String text)
        {
            //fast exit
            if (String.IsNullOrEmpty(text))
                return text;
            //
            StringBuilder sb = new StringBuilder(text.Trim());
            //1. replace {"&amp;", "&"}
            sb.Replace("&", "&amp;");
            //2 replace other tags
            int arrSize = htmlCharReplaces.GetLength(0);
            for (int i = 0; i < arrSize; i++)
            {
                sb.Replace(htmlCharReplaces[i, 1], htmlCharReplaces[i, 0]);
            }

            return sb.ToString();
        }

		/// <summary>
        /// NT-Заменить html-последовательности (например, & amp ;) в строке на их текстовые аналоги.
        /// Функция очень поверхностная, нуждается в доработке/развитии.
        /// </summary>
        /// <param name="text">Входная строка</param>
        /// <returns></returns>
        public static string ReplaceHtmlCharCodes(string text)
        {
            /* Это очень медленная функция. Правильнее будет искать в тексте символы & и от них до ; собирать код символа, потом по словарю переводить и заменять в тексте.
             * Такой способ намного быстрее, занимает 2х размера входного текста, и требует лишь одного прохода по тексту.
             * Реализовать его лучше как конечный автомат для парсера.
             * Но это сложно, а обычно в проектах надо быстро получить результат.
             * Так что написать код тут - отдельная работа, но потом проекты будут работать быстрее.
             * TODO: Написать и отладить вышеописанный код.
             */ 
            
            //fast exit
            if(String.IsNullOrEmpty(text))
                return text;
            //2 replace other tags
            StringBuilder sb = new StringBuilder(text.Trim());
            int arrSize = htmlCharReplaces.GetLength(0);
            for (int i = 0; i < arrSize; i++)
            {
                sb.Replace(htmlCharReplaces[i, 0], htmlCharReplaces[i, 1]);
            }
            //2. replace {"&amp;", "&"}
            sb.Replace("&amp;", "&");

            return sb.ToString();
        }
		
        /// <summary>
        /// NT-попробовать собрать ссылку из этого веб-адреса
        /// Вернуть ссылку или исходную строку
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string tryMakeLink(string p)
        {

            if (String.IsNullOrEmpty(p))
                return String.Empty;

            String src = p;
            if (p.StartsWith("ww"))
                src = "http://" + p;

            if (p.StartsWith("http") || p.StartsWith("ftp"))
                return String.Format("<a href=\"{0}\">{1}</a>", src, p);
            else return p;
        }


        /// <summary>
        /// NT- Get href path from "...a href="/path/to/file.ext" ..." string.
        /// </summary>
        /// <param name="text">Input string</param>
        /// <returns>Function returns href path or null if href string has invalid format.</returns>
        public static string getHrefValue(string text)
        {
            String v1 = "href=\"";
            String v2 = "\"";

            return CStringProcessor.GetPartBetween(text, v1, v2);
        }

        /// <summary>
        /// NT-Собрать веб-ссылку
        /// </summary>
        /// <param name="link">веб-ссылка или ссылка на файл</param>
        /// <param name="title">Название ссылки</param>
        /// <returns><![CDATA[Возвращает ссылку вида: <A href="web-path">text</A>A>.]]></returns>
        public static string makeHtmlFileLink(string link, string title)
        {
            return String.Format("<A href=\"{0}\">{1}</A>", link, title);
        }

        /// <summary>
        /// Returns a string of the HTML of the specified URL String.
        /// </summary>
        /// <param name="url"></param>
        public static String GetHtml(String url)
        {
            if ((!String.IsNullOrEmpty(url)) && CStringProcessor.IsValidUrl(url))
            {
                return GetHtml(new Uri(url));
            }
            throw new ArgumentException("URL string is invalid!");
        }

        /// <summary>
        /// Returns a string of the HTML of the specified Uri object.
        /// </summary>
        /// <param name="url"></param>
        public static String GetHtml(Uri url)
        {
            try
            {
                //Create request For given url
                var response = ((HttpWebRequest)WebRequest.Create(url)).GetResponse();

                //Take response stream
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    //Read response stream (html code)
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Returns a string of the JSON request
        /// </summary>
        /// <param name="url">API URL</param>
        /// <param name="CurrentToken">API Security Token</param>
        /// <returns></returns>
        public static String GetJson(String url, String CurrentToken)
        {
            if ((!String.IsNullOrEmpty(url)) && CStringProcessor.IsValidUrl(url))
            {
                return GetJson(new Uri(url), CurrentToken);
            }
            throw new ArgumentException("URL string is invalid!");
        }

        /// <summary>
        /// Returns a string of the JSON request
        /// </summary>
        /// <param name="url">API URL</param>
        /// <param name="currentToken">API Security Token</param>
        /// <returns></returns>
        public static String GetJson(Uri url, String currentToken)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + currentToken);
                request.ContentType = "application/json";
                request.Method = "GET";

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


    }
}
