using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using MyCodeLibrary.TextProcessing;
using System.Diagnostics;

namespace MyCodeLibrary.FileOperations
{
    // TODO: Класс экспериментальный, состав не оформился пока, функции работают, но их нужно проработать лучше.
    // TODO: добавить части ссылки: домен и машину, что там еще входит в Uri
 
    /// <summary>
    /// Класс представляет Файловую ссылку file:/
    /// </summary>
    public class FileLink
    {
        /// <summary>
        /// Строка зарезервированных для Url символов
        /// </summary>
        private static string UrlReservedChars = ";?:@$&=+,/{}|\\^~[]`\"%";
        
        /// <summary>
        /// Путь к файлу
        /// </summary>
        protected string m_uncPathName;

        public FileLink()
        {
            this.m_uncPathName = String.Empty;
        }

        public FileLink(string uncPath)
        {
            this.m_uncPathName = String.Copy(uncPath);
        }

        #region Properties
        /// <summary>
        /// NT - UNC сетевой путь к файлу
        /// </summary>
        public string UncPath
        {
            get { return m_uncPathName; }
            set { m_uncPathName = value; }
        }

        /// <summary>
        /// NR - Локальный путь к файлу
        /// </summary>
        public string LocalPath
        {
            get { return UNCpathToLocalPath(m_uncPathName); }
            set { m_uncPathName = LocalPathToUNCpath(value); }
        }
        /// <summary>
        /// NT- Существует ли файл?
        /// </summary>
        public bool Exists
        {
            get { return File.Exists(m_uncPathName); }
        }

        #endregion

        /// <summary>
        /// NT-Открыть указанный документ через ShellExecute
        /// </summary>
        /// <returns>Функция возвращает объект процесса.</returns>
        public Process Run()
        {
            return Process.Start(this.m_uncPathName);
        }
        /// <summary>
        /// NT-Открыть указанный документ через ShellExecute
        /// </summary>
        /// <param name="info">Параметры запуска процесса.</param>
        /// <returns>Функция возвращает объект процесса.</returns>
        public Process Run(ProcessStartInfo info)
        {
            return Process.Start(info);
        }

        #region статические функции ссылок

        /// <summary>
        /// NR-Конвертировать сетевой путь в локальный путь
        /// </summary>
        /// <param name="UNCpath">сетевой путь к документу</param>
        /// <returns>Возвращает локальный файловый путь к документу</returns>
        public static string UNCpathToLocalPath(string UNCpath)
        {
            throw new NotImplementedException();//TODO: add code here
        }
        
        /// <summary>
        /// NT- превратить путь к файлу в (нечитаемую) сетевую ссылку на файл, пригодную для использования в оболочке Винды. 
        /// </summary>
        /// <param name="localPath">Абсолютный путь к файлу</param>
        /// <returns>
        /// Возвращает полностью экранированную ссылку вида: 
        /// file://localhost/C:/Documents%20and%20Settings/1/%D0%A0%D0%B0%D0%B1%D0%BE%D1%87%D0%B8%D0%B9%20%D1%81%D1%82%D0%BE%D0%BB/12%D0%81%60~!@%23$%25%5E&()_-+=,.txt
        /// </returns>
        public static string LocalPathToUNCpath2(string localPath)
        {
            UriBuilder u = new UriBuilder();
            u.Scheme = Uri.UriSchemeFile;
            u.Path = localPath;
            return u.ToString();
        }
        
        /// <summary>
        /// NT-Конвертировать локальный путь в сетевой путь.
        /// Возвращает пустую строку при ошибке.
        /// </summary>
        /// <param name="localPath">Локальный файловый путь к документу</param>
        /// <returns>
        /// Возвращает сетевой путь к файлу.
        /// Возвращает пустую строку при ошибке.
        /// </returns>
        public static string LocalPathToUNCpath(string localPath)
        {
            //    Convert a DOS/Windows path name to a file url.
            //            C:\foo\bar\spam.foo
            //                    becomes
            //            file:///C:/foo/bar/spam.foo
            Char[] splitter1 = new char[] { '\\' };
            Char[] splitter2 = new char[] { ':' };
            String[] components = null;

            //    если нет ':' в пути
            if (localPath.IndexOf(':') == -1)//  (!pathname.Contains(":"))
            {
                //Нет буквы диска, просто меняем слеши и экранируем символы
                components = localPath.Split(splitter1); // "\"
                return urlQuote(String.Join("/", components), "/"); //экранирование символов
            }
            //Иначе, должна быть буква диска - делим строку по :
            String[] comp = localPath.Split(splitter2); // ":"
            //проверяем, что есть буква диска
            if ((comp.Length != 2) || (comp[0].Length > 1))
            {
                //TODO: как обрабатывать тут ошибку?
                //String error = "Bad path: " + pathname;
                //throw new System.IO.IOException(error);
                return String.Empty;
            }
            //Экранируем букву диска (зачем? она же буква)
            String drive = urlQuote(comp[0].ToUpper(), "/");
            //делим путь на части
            components = comp[1].Split(splitter1);
            //Каждую часть экранируем отдельно. А можно было же сразу все экранировать, ведь / указано не экранировать. 
            //TODO: Эту функцию еще надо переделывать и тестировать на всех вариантах путей, пилить и пилить.
            String path = "file:///" + drive + ":";
            foreach (String s in components)
                if (s != String.Empty)
                    path = path + "/" + urlQuote(s, "/"); //тут, если в имени папки будет / то он не будет экранирован и испортит путь. Но почему-то это не учитывается здесь.

            return path;
        }
        


        /// <summary>
        /// Экранировать символы в строке подобно %20, для их совместимости с Url
        /// </summary>
        /// <param name="path">строка для экранирования</param>
        /// <param name="safe">строка символов, не подлежащих экранированию</param>
        /// <returns></returns>
        private static string urlQuote(string path, string safe)
        {
            //    Modified version of urllib.quote supporting unicode.

            //    Each part of a URL, e.g. the path info, the query, etc., has a
            //    different set of reserved characters that must be quoted.

            //    RFC 2396 Uniform Resource Identifiers (URI): Generic Syntax lists
            //    the following reserved characters.

            //    reserved    = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+" |
            //                  "$" | ","

            //    Each of these characters is reserved in some component of a URL,
            //    but not necessarily in all of them.

            //    The function is intended for quoting the path
            //    section of a URL.  Thus, it will not encode '/'.  This character
            //    is reserved, but in typical usage the quote function is being
            //    called on a path where the existing slash characters are used as
            //    reserved characters.

            //    The characters u"{", u"}", u"|", u"\", u"^", u"~", u"[", u"]", u"`"
            //    are considered unsafe and should be quoted as well.

            StringBuilder result = new StringBuilder();
            //    for c in s:
            for (int i = 0; i < path.Length; i++)
            {
                Char c = path[i];
                //тут надо в строке оставить только символы, не входящие в список запрещенных, или упомянутые в переменной safe
                //а остальное заменить на эквиваленты вроде %20 (=пробел)
                //виндовая функция заменяет все русские буквы тоже, а это не дает читать пути.
                //if c not in safe and (ord(c) < 33 or c in URL_RESERVED):
                if ((safe.IndexOf(c) == -1) && ((Char.ConvertToUtf32(path, i) < 33) || (UrlReservedChars.IndexOf(c) != -1)))
                    result.AppendFormat("%{0:X2}", Char.ConvertToUtf32(path, i));
                else result.Append(c);
            }
            return result.ToString();
        }


        /// <summary>
        /// NT-Собрать сетевой путь к файлу из относительного или абсолютного пути к файлу
        /// </summary>
        /// <param name="rootpath">Начальная часть пути для относительного пути файла</param>
        /// <param name="text">Относительный или абсолютный путь к файлу, или ссылка вида file:///C:/data.dat или file:///data.dat </param>
        /// <returns>
        /// Если файл собран правильно, возвращается сетевая ссылка на файл, пригодная для использования в оболочке Винды.
        /// Если файл не существует, или возникает любая ошибка, возвращается String.Empty.
        /// </returns>
        public static String makeUriFromRelativeFilePath(string rootpath, string text)
        {
            String result = String.Empty;
            try
            {
                String absolutePath = text.Trim();

                //проверить, что путь это уже готовая ссылка
                //и извлечь из нее собственно путь
                // а просто использовать ее не получается - код пропускает относительные пути в ссылках 
                if (Uri.IsWellFormedUriString(absolutePath, UriKind.Absolute))
                {
                    UriBuilder ub = new UriBuilder(absolutePath);
                    absolutePath = ub.Path;
                    ub = null;
                }
                //удалить первые символы / \ из пути файла, если они есть.
                absolutePath = absolutePath.TrimStart(new Char[] { '/', '\\' });
                //проверить что путь относительный и преобразовать в абсолютный
                String root = Path.GetPathRoot(absolutePath);
                if ((String.IsNullOrEmpty(root)))
                {
                    //это относительный путь
                    //его надо переделать в абсолютный
                    absolutePath = Path.Combine(rootpath, absolutePath);
                }
                else if (root.Length < 3)
                {
                    //а тут то же самое для случая, когда root != null и короче 3 символов
                    //поскольку нормальный формат корня:  C:\
                    //absolutePath = Path.Combine(rootpath, absolutePath); - absolute path already comes from ImportMenuManager, and we can ignore this case
                    absolutePath = String.Empty;
                }
                //Если файл по такому пути не существует, возвращаем пустую строку
                //Иначе возвращаем сетевой путь к файлу
                if (File.Exists(absolutePath))
                    result = LocalPathToUNCpath2(absolutePath);//превращаем путь в URI
            }
            catch (Exception)
            {
                result = String.Empty;
            }

            return result;
        }
#endregion
    }//end class
}//end namespace