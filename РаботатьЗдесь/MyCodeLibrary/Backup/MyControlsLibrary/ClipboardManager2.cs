using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MyControlsLibrary
{
    /// <summary>
    /// Мой класс менеджера клипборда и данных для операций перетаскивания.
    /// Версия без операций с PictureBox Control
    /// </summary>
    public class ClipboardManager2
    {
        internal static void RichTextBoxPaste(System.Windows.Forms.IDataObject daob, RichTextBox rtb)
        {
           
            //Обработать сброс на RichTextBox файлов из Проводника или текста из этого или другого контрола.
            //тут вообще-то надо здорово поработать, корректно обрабатывать файлы, перетащенные с сети по FTP или еще как-то
            //и корректно обрабатывать строки, принесенные из разных форматов вроде RTF или HTML
            //и корректно обрабатывать перетаскивание текста внутри контрола - сейчас текст копируется, а надо переносить.
            //и еще вставка из клипборда тоже - текста и файлов - подобным же образом должна быть сделана.

            String textPasted = makeTextOrLinks(daob);

            ////insert text at position
            ////может лучше сделать это через clipboard и Paste() ?
            ////нет - он тогда вставляет не как ссылку, а как объект. И объект потом не сохраняется в текст.
            ////Так что тут надо сделать прогу для отработки этого кода.
            RichTextBoxPaste(rtb, textPasted);

        }
        /// <summary>
        /// NT-вставить текст в текущую позицию RichTextBox контрола
        /// </summary>
        /// <param name="rtb">Контрол для вставки</param>
        /// <param name="textPasted">Вставляемый в текущую позицию текст</param>
        public static void RichTextBoxPaste(RichTextBox rtb, string textPasted)
        {
            //TODO: 13092015 - Тут надо понять что получилось и внести исправления. Ранее выделенный текст из контрола надо удалить и потом на его место вставить новый
            //получаем начало и конец выделенного текста для его замены
            int i = rtb.SelectionStart;                         //позиция начала выделенного текста
            int j = rtb.SelectionStart + rtb.SelectionLength;   //позиция конца выделенного текста
            //получаем строку от места вставки до конца
            String tail = rtb.Text.Substring(j);
            String head = rtb.Text.Substring(0, i);
            //добавляем вставляемый текст
            head = head + textPasted;
            //добавляем остаток текста
            head = head + tail;
            rtb.Text = head;
            //восстанавливаем позицию вставки
            //rtb.Select(i, textPasted.Length); - это выделит вставленный текст, и повторное нажатие Paste не вставит его еще раз.
            rtb.Select(i + textPasted.Length, 0);
            //вроде все
            return;
        }
        /// <summary>
        /// NT-Извлечь данные в виде строки.
        /// </summary>
        /// <param name="daob"></param>
        /// <returns></returns>
        private static string makeTextOrLinks(IDataObject daob)
        {
            String item = "";
            if (daob.GetDataPresent(DataFormats.FileDrop, true))
            {
                String[] sar = (String[])daob.GetData(DataFormats.FileDrop, true);
                if (sar != null)
                {
                    //13092015 - если ссылок более 2, вставлять переносы строк, чтобы ссылки столбиком вставились.
                    StringBuilder sb = new StringBuilder();
                    //filenames array like C:\test.pdf
                    foreach (String ss in sar)
                    {
                        sb.Append("  ");
                        //makeUriFromAbsoluteFilePath(ss);
                        sb.Append(makeFileUrlFromAbsoluteNTPath(ss));//13092015 - новая, но сырая функция с более понятным путем файла
                        sb.Append("  ");
                        if (sar.Length > 2)
                            sb.AppendLine();
                    }
                    item = sb.ToString();
                }
            }
            else if (daob.GetDataPresent(DataFormats.UnicodeText, true))
            {
                item = daob.GetData(DataFormats.UnicodeText, true).ToString();
            }

            return item;
        }

        //TODO: Эта функция добавлена в MyCodeLibrary.FileOperations.FileLink класс как в основное расположение.
        /// <summary>
        /// NT-14042015 - превратить путь к файлу в сетевую ссылку на файл, пригодную для использования в оболочке Винды. 
        /// /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        public static string makeUriFromAbsoluteFilePath(string ss)
        {
            
            UriBuilder u = new UriBuilder();
            u.Scheme = Uri.UriSchemeFile;
            u.Path = ss;
            return u.ToString();
        }

        //TODO: Эта функция добавлена в MyCodeLibrary.FileOperations.FileLink класс как в основное расположени
        /// <summary>
        /// NT-13092015 - конвертировать путь к файлу в сетевую ссылку на файл, пригодную для использования в оболочке Винды.
        /// Возвращает сетевой путь, или пустую строку если путь не удалось конвертировать.
        /// </summary>
        /// <param name="pathname">Путь к файлу</param>
        /// <returns></returns>
        public static string makeFileUrlFromAbsoluteNTPath(string pathname)
        {
            //    Convert a DOS/Windows path name to a file url.

            //            C:\foo\bar\spam.foo

            //                    becomes

            //            ///C:/foo/bar/spam.foo
            Char[] splitter1 = new char[] { '\\' };
            Char[] splitter2 = new char[] { ':' };
            String[] components = null;

            //    если нет ':' в пути
            if (pathname.IndexOf(':') == -1)//  (!pathname.Contains(":"))
            {
                //Нет буквы диска, просто меняем слеши и экранируем символы
                components = pathname.Split(splitter1); // "\"
                return urlQuote(String.Join("/", components), "/"); //экранирование символов
            }
            //Иначе, должна быть буква диска - делим строку по :
            String[] comp = pathname.Split(splitter2); // ":"
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

        //TODO: Эта функция добавлена в MyCodeLibrary.FileOperations.FileLink класс как в основное расположени
        /// <summary>
        /// Строка зарезервированных для Url символов
        /// </summary>
        private static string UrlReservedChars = ";?:@$&=+,/{}|\\^~[]`\"%";


        //TODO: Эта функция добавлена в MyCodeLibrary.FileOperations.FileLink класс как в основное расположени
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

        //TODO: Эта функция добавлена в MyCodeLibrary.FileOperations.FileLink класс как в основное расположени
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
                    result = makeUriFromAbsoluteFilePath(absolutePath);//превращаем путь в URI
            }
            catch (Exception)
            {
                result = String.Empty;
            }

            return result;
        }





    }
}
