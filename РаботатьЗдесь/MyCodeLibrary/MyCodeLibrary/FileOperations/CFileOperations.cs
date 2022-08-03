using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using MyCodeLibrary.TextProcessing;
using MyCodeLibrary.Hash;

namespace MyCodeLibrary.FileOperations
{
    /// <summary>
    /// Общие операции с файлами
    /// </summary>
    public class CFileOperations
    {
        /// <summary>
        /// Расширение файла-ярлыка
        /// </summary>
        public const string ShortcutFileExtension = ".lnk";

        /// <summary>
        /// NT-Проверить по расширению файла, что файл является ярлыком.
        /// </summary>
        /// <param name="notePath">Путь к файлу-ярлыку</param>
        /// <returns>True если это файл-ярлык</returns>
        public static bool IsShortcutFile(string notePath)
        {
            string ext = Path.GetExtension(notePath);
            return String.Equals(ext, ".lnk", StringComparison.OrdinalIgnoreCase);
        }

        

        /// <summary>
        /// Заменить неправильные символы пути файла в строке на указанный символ
        /// </summary>
        /// <param name="s">Строка пути файла</param>
        /// <param name="rChar">Символ на замену</param>
        /// <returns>Возвращает строку не содержащую неправильных символов имени файла</returns>
        public static string ReplaceInvalidFilenameChars(string s, char rChar)
        {
            //Получаем массив запрещенных символов
            char[] inv = Path.GetInvalidFileNameChars();
            //Создаем билдер для сборки символов
            StringBuilder sb = new StringBuilder();
            foreach (Char c in s)
            {
                //если символ есть в массиве, то вместо него пишем замену, иначе пишем сам символ
                if (Array.IndexOf(inv, c) == -1)
                    sb.Append(c);
                else sb.Append(rChar);
            }
            return sb.ToString();
        }

        /// <summary>
        /// NT-Заменить все символы не подходящие для файловых путей на _
        /// </summary>
        /// <param name="chapTitle"></param>
        /// <returns></returns>
        public static string makeSafeFileTitle(string filetitle)
        {
            StringBuilder sb = new StringBuilder(filetitle.Length);
            List<char> charlist = new List<char>(Path.GetInvalidFileNameChars());
            charlist.Add('.');
            charlist.Add('#');
            foreach (char c in filetitle)
            {
                if (!charlist.Contains(c))
                    sb.Append(c);
                else
                    sb.Append('_');
            }
            //replace restricted file names like con com1 lpt null
            if (isRestrictedFileName(sb.ToString()))
                sb.Append("!");
            //return
            return sb.ToString();
        }

        /// <summary>
        /// NT-Создать безопасное имя без расширения для файла, длиной менее maxLength символов.
        /// </summary>
        /// <param name="title">Название для файла, возможно, содержащее недопустимые для имени файла символы.</param>
        /// <param name="maxLength">Максимально допустимое количество символов названия файла</param>
        /// <returns>Функция возвращаетновое название для файла.</returns>
        public static string makeSafeFileTitle(string title, int maxLength)
        {
            String result = CFileOperations.ReplaceInvalidFilenameChars(title, '_').Trim();
            if(result.Length > maxLength)
                result = result.Substring(0, maxLength).Trim();

            return result;
        }

        /// <summary>
        /// RT-Заменить в названии файла все не подходящие для файловых путей символы на указанные
        /// </summary>
        /// <param name="filetitle">Исходное название файла, без расширения</param>
        /// <param name="wrongSymbols">Добавочные символы, помимо Path.GetInvalidFileNameChars(), нежелательные в названии файла</param>
        /// <param name="replacement">Строка, которой заменяются неправильные символы. Обычно это "_" или "-".</param>
        /// <param name="titleTemplate">Шаблонное название файла, используемое в случае, когда исходное название не содержит никаких символов.</param>
        /// <returns>Функция возвращает исправленное название файла.</returns>
        /// <remarks>
        /// Проверено 04 ноября 2019 г.
        /// Создание файлов функцией File.Create(..):
        /// "" ==> "file" : C:\Temp\.txt Success
        /// " " ==> "file" : C:\Temp\ .txt Success
        /// " . " ==> "." : C:\Temp\ . .txt Success
        /// ".." ==> ".." : C:\Temp\...txt Success
        /// "-." ==> "-." : C:\Temp\-..txt Success
        /// "con" ==> "con-" : C:\Temp\con.txt System.ArgumentException
        /// ".con" ==> ".con" : C:\Temp\.con.txt Success
        /// "con." ==> "con-." : C:\Temp\con..txt System.ArgumentException
        /// "f.con.con" ==> "f.con.con" : C:\Temp\f.con.con.txt Success
        /// "con.f" ==> "con-.f" : C:\Temp\con.f.txt System.ArgumentException
        /// "файл.con" ==> "файл.con" : C:\Temp\файл.con.txt Success
        /// "файл" ==> "файл" : C:\Temp\файл.txt Success
        /// ".файл" ==> ".файл" : C:\Temp\.файл.txt Success
        /// "aux.файл" ==> "aux-.файл" : C:\Temp\aux.файл.txt System.ArgumentException
        /// 
        /// Создание файлов после обработки функцией makeSafeFiletitle(..):
        /// "" ==> "file" : C:\Temp\file.txt Success
        /// " " ==> "file" : C:\Temp\file.txt Success
        /// " . " ==> "." : C:\Temp\..txt Success
        /// ".." ==> ".." : C:\Temp\...txt Success
        /// "-." ==> "-." : C:\Temp\-..txt Success
        /// "con" ==> "con-" : C:\Temp\con-.txt Success
        /// ".con" ==> ".con" : C:\Temp\.con.txt Success
        /// "con." ==> "con-." : C:\Temp\con-..txt Success
        /// "f.con.con" ==> "f.con.con" : C:\Temp\f.con.con.txt Success
        /// "con.f" ==> "con-.f" : C:\Temp\con-.f.txt Success
        /// "файл.con" ==> "файл.con" : C:\Temp\файл.con.txt Success
        /// "файл" ==> "файл" : C:\Temp\файл.txt Success
        /// ".файл" ==> ".файл" : C:\Temp\.файл.txt Success
        /// "aux.файл" ==> "aux-.файл" : C:\Temp\aux-.файл.txt Success
        /// </remarks>
        /// <example>
        /// <code>
        /// static void Main(string[] args)
        /// {
        ///     String[] wrongTitles = new String[] { "", " ", " . ", "..", "-.", "con", ".con", "con.", "f.con.con", "con.f", "файл.con", "файл", ".файл", "aux.файл" };
        ///     foreach (String s in wrongTitles)
        ///     {
        ///         String t = makeSafeFiletitle(s, "#", "-", "file");
        ///         Console.WriteLine("\"{0}\" ==> \"{1}\" : {2}", s, t, tryCreateFile(t));
        ///     }
        ///     Console.ReadLine();
        /// }
        ///
        ///
        /// // Попытаемся создать файл с указанным названием во временном каталоге
        /// private static object tryCreateFile(string s)
        /// {
        ///     String result = "C:\\Temp\\" + s + ".txt";
        ///     try
        ///     {
        ///         FileStream fs = System.IO.File.Create(result);
        ///         fs.Close();
        ///         result = result + " Success";
        ///     }
        ///     catch (Exception ex)
        ///     {
        ///         result = result + " " + ex.GetType().ToString();
        ///     }
        ///     return result;
        /// }
        /// </code> 
        /// </example>
        public static string makeSafeFiletitle(string filetitle, String wrongSymbols, String replacement, string titleTemplate)
        {
            //1. если название файла это нуль или пустая строка, вернуть указанный шаблон
            if(String.IsNullOrEmpty(filetitle))
                return titleTemplate;
            //1.1 если название файла после Trim()  это нуль или пустая строка, вернуть указанный шаблон
            String t = filetitle.Trim();
            if(String.IsNullOrEmpty(t))
                return titleTemplate;
            //2. если название таки есть, надо удалить из него неправильные символы 
            StringBuilder sb = new StringBuilder(t.Length);
            List<char> charlist = new List<char>(Path.GetInvalidFileNameChars());
            //добавим в список нежелательные символы, указанные пользователем 
            charlist.AddRange(wrongSymbols.ToCharArray());
            //заменим нежелательные символы на замену
            foreach(char c in t)
            {
                if(!charlist.Contains(c))
                    sb.Append(c);
                else
                    sb.Append(replacement);
            }
            t = sb.ToString();
            sb.Length = 0;
            //3 replace restricted file names like con com1 lpt null
            //надо разделять строку на части по точкам и потом искать, в первой части только, зарезервированные имена.
            //проверить символы перед первой точкой
            String[] sar = t.Split('.');
            //if (String.IsNullOrEmpty(sar[0]))  файлы ".txt" или "..txt" вполне создаются в NTFS Виндовс
            //    return titleTemplate;
            //else
            if(isRestrictedFileName(sar[0])) //файлы "con.t.txt"  не создаются в NTFS Виндовс, так как работает только первая часть имени до точки
            {
                sar[0] = sar[0] + replacement;
                return String.Join(".", sar);
            }
            else
                return t;
        }


        /// <summary>
        /// NT-Создать временный каталог со случайным именем и расширением
        /// </summary>
        /// <param name="tempFolderPath">Родительский каталог для создаваемого временного каталога</param>
        /// <returns>Возвращает путь к созданному временному каталогу.</returns>
        /// <remarks>
        /// Функция перебирает случайные имена каталогов, чтобы получить незанятое имя для нового каталога.
        /// Если 1024 попытки не удались, выдается исключение.
        /// </remarks>
        public static String createTemporaryFolder(String tempFolderPath)
        {
            String name = "";
            //подбираем уникальное имя каталога
            for (int i = 0; i < 1024; i++)
            {
                name = Path.Combine(tempFolderPath, Path.GetRandomFileName());
                if (!Directory.Exists(name))
                    break;
            }
            //создаем каталог
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
            }
            else
                throw new Exception("Error: Cannot create temporary directory");

            return name;
        }

        /// <summary>
        /// Массив запрещенных имен файлов - для коррекции имен файлов
        /// </summary>
        public static String[] RestrictedFileNames = { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM1", "LPT1", "LPT2", "LPT3", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10", "COM11", "COM12", "COM13", "COM14", "COM15", "COM16", "COM17", "COM18", "COM19" };

        /// <summary>
        /// NT-Проверить, что имя файла является неправильным
        /// Это не совсем правильная функция: для имен, содержащих точку, лишь первая часть (до первой точки) вызывает исключение.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool isRestrictedFileName(string p)
        {
            //Это не совсем правильная функция: для имен, содержащих точку, лишь первая часть (до первой точки) вызывает исключение.
            //TODO: переделать так, чтобы проверять только первую часть имени до первой точки, если она есть. См. makeSafeFiletitle().
            //fast check = check length
            if ((p.Length > 6) || (p.Length < 3))
                return false;
            //slow check - check content
            foreach (String s in RestrictedFileNames)
                if (String.Equals(s, p, StringComparison.OrdinalIgnoreCase))
                    return true;
            //no restrictions
            return false;
        }




        /// <summary>
        /// NT- Вычислить CRC32 для файла
        /// </summary>
        /// <param name="filepath">Путь к файлу</param>
        /// <returns>Возвращает CRC32</returns>
        public static UInt32 getFileCrc(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384);
            CRC32st crc = new CRC32st();
            uint val = crc.GetCrc32(fs);

            return val;
        }

        /// <summary>
        /// NT-Создать свободное имя файла в том же каталоге
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string createFreeFileName(string filePath)
        {
            //fast return
            if (!File.Exists(filePath)) return filePath;
            //get parts of name
            String folder = Path.GetDirectoryName(filePath);
            String name = Path.GetFileNameWithoutExtension(filePath);
            if (name.Length > 5)
                name = name.Substring(0, 5);
            String ext = Path.GetExtension(filePath);
            //assembly new filename
            int cnt = 0;
            String result = String.Empty;
            while (true)
            {
                result = Path.Combine(folder, name + cnt.ToString() + ext);
                if (!File.Exists(result))
                    break;
                else
                    cnt++;
            }
            return result;
        }

        /// <summary>
        /// NT-Проверить что содержимое потоков одинаковое
        /// </summary>
        /// <param name="zs">Поток 1</param>
        /// <param name="fs">Поток 2</param>
        /// <returns></returns>
        public static bool CompareStreams(Stream zs, Stream fs)
        {
            Byte[] buf = new byte[4096];
            Byte[] buz = new byte[4096];
            int rdf = 0;
            int rdz = 0;
            while (true)
            {
                rdf = fs.Read(buf, 0, 4096);
                rdz = zs.Read(buz, 0, 4096);
                //если длины не равны, то и файлы не равны
                if (rdf != rdz) return false;
                //content
                for (int i = 0; i < rdf; i++)
                    if (buf[i] != buz[i])
                        return false;

                //конец файла
                if (rdf != 4096) break;
            }
            // вернуть результат
            return true;
        }

        /// <summary>
        /// NT- compare two files
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="file2"></param>
        /// <returns></returns>
        public static bool CompareFiles(string filePath, string file2)
        {
            FileStream s1 = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream s2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read);
            bool result = CompareStreams(s1, s2);
            s1.Close();
            s2.Close();
            return result;
        }

        /// <summary>
        /// Массив запрещенных для веб-имен символов - здесь для оптимизации функции проверки веб-имен 
        /// </summary>
        public static Char[] RestrictedWebLinkSymbols = { ' ', '\\', '/', '?', ';', ':', '@', '&',
                                                             '=', '+', '$', ',', '<', '>', '"', '#',
                                                           '{', '}', '|', '^', '[', ']', '‘', '%',
                                                             '\n', '\t', '\r' };


        /// <summary>
        /// NFT-Нормализовать имя файла или каталога
        /// </summary>
        /// <param name="title">имя файла без расширения</param>
        /// <param name="maxLength">Максимальная длина имени, в символах</param>
        /// <returns>Возвращает нормализованное название файла или каталога, без расширения.</returns>
        /// <remarks>
        /// Функция заменяет на подчеркивания _ все символы, кроме букв и цифр.
        /// Если в названии есть пробелы, они удаляются, а следующий символ переводится в верхний регистр.
        /// Если в названии есть символ 'µ', он заменяется на символ 'u'.
        /// Если получившееся название длиннее maxLength, то оно обрезается до maxLength.
        /// Если получившееся название является зарезервированным системным названием (вроде CON), или
        /// если получившееся название короче 3 символов, к нему добавляется случайное число.
        /// </remarks>
        public static string RemoveWrongSymbols(string title, int maxLength)
        {
            //TODO: Optimize - переработать для ускорения работы насколько это возможно
            //надо удалить все запрещенные символы
            //если пробелы, то символ после них перевести в верхний регистр
            //если прочие символы, заменить на подчеркивание
            //если имя длиннее maxLength, то обрезать до maxLength.
            Char[] chars = title.ToCharArray();
            //create string builder
            StringBuilder sb = new StringBuilder(chars.Length);
            //если символ в строке является недопустимым, заменить его на подчеркивание.
            Char c;
            bool toUp = false;//для перевода в верхний регистр
            foreach (char ch in chars)
            {
                c = ch;
                if (ch == ' ')
                {
                    toUp = true;
                    //ничего не записываем в выходной накопитель - так пропускаем все пробелы.
                }
                else
                {
                    //foreach (char ct in RestrictedWebLinkSymbols)
                    //{
                    //    if (ch.Equals(ct))
                    //        c = '_';//замена недопустимого символа на подчеркивание
                    //}
                    //Unicode chars throw exceptions

                    //тут надо пропускать только -_A-Za-zА-Яа-я и все равно будут проблемы с именами файлов архива

                    if (!Char.IsLetterOrDigit(ch))
                        c = '_';//замена недопустимого символа на подчеркивание
                    //перевод в верхний регистр после пробела
                    if (toUp == true)
                    {
                        c = Char.ToUpper(c);
                        toUp = false;
                    }
                    //if c == мю then c = u
                    if (c == 'µ') c = 'u';
                    //добавить в выходной накопитель
                    sb.Append(c);
                }
            }
            //если имя длиннее максимума, обрезать по максимуму
            if (sb.Length > maxLength) sb.Length = maxLength;
            //если имя короче минимума, добавить псевдослучайную последовательность.
            //и проверить, что имя не запрещенное
            if ((sb.Length < 3) || isRestrictedFileName(sb.ToString()))
            {
                sb.Append('_');
                sb.Append(new Random().Next(10, 100).ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        /// <summary>
        /// NFT-Нормализовать имя файла или каталога
        /// </summary>
        /// <param name="title">имя файла без расширения</param>
        /// <returns>Возвращает нормализованное название файла или каталога, без расширения.</returns>
        /// <remarks>
        /// Функция заменяет на подчеркивания _ все символы, кроме букв и цифр.
        /// Если в названии есть пробелы, они удаляются, а следующий символ переводится в верхний регистр.
        /// Если в названии есть символ 'µ', он заменяется на символ 'u'.
        /// Если получившееся название длиннее maxLength, то оно обрезается до maxLength.
        /// Если получившееся название является зарезервированным системным названием (вроде CON), или
        /// если получившееся название короче 3 символов, к нему добавляется случайное число.
        /// </remarks>
        public static string RemoveWrongSymbols(string title)
        {
            //TODO: Optimize - переработать для ускорения работы насколько это возможно
            //надо удалить все запрещенные символы
            //если пробелы, то символ после них перевести в верхний регистр
            //если прочие символы, заменить на подчеркивание
            //если имя длиннее 16, то обрезать до 16.
            Char[] chars = title.ToCharArray();
            //create string builder
            StringBuilder sb = new StringBuilder(chars.Length);
            //если символ в строке является недопустимым, заменить его на подчеркивание.
            Char c;
            bool toUp = false;//для перевода в верхний регистр
            foreach (char ch in chars)
            {
                c = ch;
                if (ch == ' ')
                {
                    toUp = true;
                    //ничего не записываем в выходной накопитель - так пропускаем все пробелы.
                }
                else
                {
                    //foreach (char ct in RestrictedWebLinkSymbols)
                    //{
                    //    if (ch.Equals(ct))
                    //        c = '_';//замена недопустимого символа на подчеркивание
                    //}
                    //Unicode chars throw exceptions

                    //тут надо пропускать только -_A-Za-zА-Яа-я и все равно будут проблемы с именами файлов архива

                    if (!Char.IsLetterOrDigit(ch))
                        c = '_';//замена недопустимого символа на подчеркивание
                    //перевод в верхний регистр после пробела
                    if (toUp == true)
                    {
                        c = Char.ToUpper(c);
                        toUp = false;
                    }
                    //if c == мю then c = u
                    if (c == 'µ') c = 'u';
                    //добавить в выходной накопитель
                    sb.Append(c);
                }
            }
            //если имя длиннее максимума, обрезать по максимуму
            if (sb.Length > 16) sb.Length = 16;
            //если имя короче минимума, добавить псевдослучайную последовательность.
            //и проверить, что имя не запрещенное
            if ((sb.Length < 3) || isRestrictedFileName(sb.ToString()))
            {
                sb.Append('_');
                sb.Append(new Random().Next(10, 100).ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Удалить файл, изменив его атрибуты на нормальные.
        /// Если файл не существует, исключение не выбрасывается.
        /// </summary>
        /// <param name="filepath">Путь к файлу</param>
        /// <remarks>
        /// Обычно, если файл имеет атрибуты Архивный или ReadOnly, функция File.Delete() вместо удаления выдает исключение.
        /// Эта функция сначала снимает атрибуты, а затем выполняет удаление.
        /// Это мелочь, которая упрощает написание кода.
        /// </remarks>
        public void DeleteFile(String filepath)
        {
            if(File.Exists(filepath))
            {
                //set file attributes to normal
                File.SetAttributes(filepath, FileAttributes.Normal);
                //remove file
                File.Delete(filepath);
            }
            return;
        }

        /// <summary>
        /// NT-Создать каталог, в котором запрещено индексирование средствами операционной системы
        /// </summary>
        /// <param name="folderPath">Путь к создаваемому каталогу</param>
        public static void CreateNotIndexedFolder(String folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            //create directory
            if(!di.Exists)
                di.Create();
            //set attribute Not indexed
            di.Attributes = FileAttributes.NotContentIndexed | FileAttributes.Directory;
            di = null;
            return;
        }

        /// <summary>
        /// NT- Удалить и заново создать указанный каталог.
        /// </summary>
        /// <param name="folderPath">Путь к каталогу.</param>
        public static void FolderDeleteAndCreate(String folderPath)
        {
            if(Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);
            Directory.CreateDirectory(folderPath);

            return;
        }

        /// <summary>
        /// RT-Создать новый файл или перезаписать существующий
        /// </summary>
        /// <param name="p">Путь файла</param>
        public static void СоздатьФайлБезИндексирования(string p)
        {
            FileStream fs = File.Create(p, 16 * 1024, FileOptions.WriteThrough);
            fs.Close();
            FileInfo fi = new FileInfo(p);
            fi.Attributes = FileAttributes.Normal | FileAttributes.NotContentIndexed;

            return;
        }

        /// <summary>
        /// RT-Сделать абсолютный путь из относительного, если путь не абсолютный.
        /// </summary>
        /// <param name="refPath">Обрабатываемый путь, абсолютный или относительный</param>
        /// <param name="basePath">Базовый абсолютный путь</param>
        /// <returns>Возвращает абсолютный путь</returns>
        public static string ПолучитьАбсолютныйПуть1(string refPath, string basePath)
        {
            if (IsAbsolutePath(refPath))
                return refPath;//если уже абсолютный, просто вернуть его.
            //иначе
            //1 проверить что базовый путь абсолютный
            if (!IsAbsolutePath(basePath))
                throw new Exception(String.Format(CultureInfo.CurrentCulture, "Путь {0} не является абсолютным", basePath));
            //2 собрать абсолютный путь
            //если первый символ \, то удалить его, иначе пути не склеятся этой функцией
            if (refPath[0] == '\\')
                refPath = refPath.Substring(1);
            String result = Path.Combine(basePath, refPath);

            return result;
        }

        #region *** absolute-relative path ***
        //Это функции из движка Тапп Бар.
        //Они протестированы, вот результаты тестов:

        /*
        
s3 = Bar.Utility.MUtility.makeRelativePath(s1, s2)

makeRelPath(C:\Temp, C:\Absolute) = C:\Absolute
makeRelPath(C:\Temp, C:\Absolute\) = C:\Absolute
makeRelPath(C:\Temp, C:\Temp) =
makeRelPath(C:\Temp, C:\Temp\) =
makeRelPath(C:\Temp, Lokal) = Lokal
makeRelPath(C:\Temp, Lokal\) = Lokal
makeRelPath(C:\Temp, \Lokal\) = Lokal
makeRelPath(C:\Temp, ) =
makeRelPath(C:\Temp\, C:\Absolute) = C:\Absolute
makeRelPath(C:\Temp\, C:\Absolute\) = C:\Absolute
makeRelPath(C:\Temp\, C:\Temp) =
makeRelPath(C:\Temp\, C:\Temp\) =
makeRelPath(C:\Temp\, Lokal) = Lokal
makeRelPath(C:\Temp\, Lokal\) = Lokal
makeRelPath(C:\Temp\, \Lokal\) = Lokal
makeRelPath(C:\Temp\, ) =
makeRelPath(Local, C:\Absolute) = ArgumentException
makeRelPath(Local, C:\Absolute\) = ArgumentException
makeRelPath(Local, C:\Temp) = ArgumentException
makeRelPath(Local, C:\Temp\) = ArgumentException
makeRelPath(Local, Lokal) = ArgumentException
makeRelPath(Local, Lokal\) = ArgumentException
makeRelPath(Local, \Lokal\) = ArgumentException
makeRelPath(Local, ) = ArgumentException
makeRelPath(\Local\, C:\Absolute) = ArgumentException
makeRelPath(\Local\, C:\Absolute\) = ArgumentException
makeRelPath(\Local\, C:\Temp) = ArgumentException
makeRelPath(\Local\, C:\Temp\) = ArgumentException
makeRelPath(\Local\, Lokal) = ArgumentException
makeRelPath(\Local\, Lokal\) = ArgumentException
makeRelPath(\Local\, \Lokal\) = ArgumentException
makeRelPath(\Local\, ) = ArgumentException

s3 = Bar.Utility.MUtility.makeAbsolutePath(s1, s2)

makeAbsPath(C:\Temp, C:\Absolute) = C:\Absolute
makeAbsPath(C:\Temp, C:\Absolute\) = C:\Absolute
makeAbsPath(C:\Temp, C:\Temp) = C:\Temp
makeAbsPath(C:\Temp, C:\Temp\) = C:\Temp
makeAbsPath(C:\Temp, Lokal) = C:\Temp\Lokal
makeAbsPath(C:\Temp, Lokal\) = C:\Temp\Lokal
makeAbsPath(C:\Temp, \Lokal\) = C:\Temp\Lokal
makeAbsPath(C:\Temp, ) = C:\Temp
makeAbsPath(C:\Temp\, C:\Absolute) = C:\Absolute
makeAbsPath(C:\Temp\, C:\Absolute\) = C:\Absolute
makeAbsPath(C:\Temp\, C:\Temp) = C:\Temp
makeAbsPath(C:\Temp\, C:\Temp\) = C:\Temp
makeAbsPath(C:\Temp\, Lokal) = C:\Temp\Lokal
makeAbsPath(C:\Temp\, Lokal\) = C:\Temp\Lokal
makeAbsPath(C:\Temp\, \Lokal\) = C:\Temp\Lokal
makeAbsPath(C:\Temp\, ) = C:\Temp\
makeAbsPath(Local, C:\Absolute) = ArgumentException
makeAbsPath(Local, C:\Absolute\) = ArgumentException
makeAbsPath(Local, C:\Temp) = ArgumentException
makeAbsPath(Local, C:\Temp\) = ArgumentException
makeAbsPath(Local, Lokal) = ArgumentException
makeAbsPath(Local, Lokal\) = ArgumentException
makeAbsPath(Local, \Lokal\) = ArgumentException
makeAbsPath(Local, ) = ArgumentException
makeAbsPath(\Local\, C:\Absolute) = ArgumentException
makeAbsPath(\Local\, C:\Absolute\) = ArgumentException
makeAbsPath(\Local\, C:\Temp) = ArgumentException
makeAbsPath(\Local\, C:\Temp\) = ArgumentException
makeAbsPath(\Local\, Lokal) = ArgumentException
makeAbsPath(\Local\, Lokal\) = ArgumentException
makeAbsPath(\Local\, \Lokal\) = ArgumentException
makeAbsPath(\Local\, ) = ArgumentException


        */

        /// <summary>
        /// RT-Вернуть абсолютный путь к каталогу
        /// </summary>
        /// <param name="basedir">Абсолютный путь основного каталога</param>
        /// <param name="localPath">Относительный или абсолютный путь конечного каталога</param>
        /// <returns>Функция возвращает абсолютный путь к конечному каталогу</returns>
        public static string makeAbsolutePath(string basedir, string localPath)
        {
            //проверяем  аргументы
            if (String.IsNullOrEmpty(basedir))
                throw new ArgumentException("Неправильный путь каталога", "basedir");
            if (localPath == null)
                throw new ArgumentException("Неправильный путь каталога", "localPath");
            if (!IsAbsolutePath(basedir))
                throw new ArgumentException("Путь должен быть абсолютным", "basedir");
            //если первый символ \, то удалить его, иначе пути не склеятся этой функцией
            String locP = localPath.Trim();
            if (locP.StartsWith("\\"))
                locP = locP.Substring(1);
            if (locP.EndsWith("\\"))
                locP = locP.Remove(locP.Length - 1);
            //если localPath не относительный, возвращаем его            
            if (IsAbsolutePath(locP))
                return locP;
            //иначе создаем абсолютный путь из локального и базового 
            String result = Path.Combine(basedir, locP);

            return result;
        }

        /// <summary>
        /// RT-Вернуть относительный (если возможно) или абсолютный путь к каталогу
        /// </summary>
        /// <param name="basedir">Абсолютный путь основного каталога</param>
        /// <param name="absolutePath">Относительный или абсолютный путь конечного каталога</param>
        /// <returns></returns>
        public static string makeRelativePath(string basedir, string absolutePath)
        {
            //проверяем аргументы
            if (String.IsNullOrEmpty(basedir))
                throw new ArgumentException("Неправильный путь каталога", "basedir");
            if (absolutePath == null)
                throw new ArgumentException("Неправильный путь каталога", "absolutePath");
            if (!IsAbsolutePath(basedir))
                throw new ArgumentException("Путь должен быть абсолютным", "basedir");
            //удалить конечный слеш в базовом каталоге если он есть
            String basePath = basedir.Trim();
            if (basePath.EndsWith("\\"))
                basePath = basePath.Remove(basePath.Length - 1);
            //удалить начальный и конечный слеш в пути конечного каталога, если они есть
            String absPath = absolutePath.Trim();
            if (absPath.StartsWith("\\"))
                absPath = absPath.Substring(1);
            if (absPath.EndsWith("\\"))
                absPath = absPath.Remove(absPath.Length - 1);
            //если absolutePath - абсолютный
            if (IsAbsolutePath(absPath))
            {
                //то надо определить, приводится ли он в относительный путь
                //и если не приводится, то вернуть целиком
                //а если приводится, то вернуть относительную часть.
                String p1 = Path.GetFullPath(basePath);
                String p2 = Path.GetFullPath(absPath);
                bool res = p2.StartsWith(p1, StringComparison.InvariantCultureIgnoreCase);
                if (res == true)//если absolutePath начинается с basedir, то он и приводится в относительный путь.
                {
                    string result = p2.Remove(0, p1.Length);
                    if (result.StartsWith("\\"))//TODO: проверить это при отладке и сразу упростить, если возможно.
                        result = result.Substring(1);
                    return result;
                }
                else
                    return absPath;//поскольку не приводится, то вернуть целиком
            }
            else
            {
                //если absolutePath - относительный, просто копируем его.
                return absPath;
            }
        }


        /// <summary>
        /// RT-Убедиться что файловый путь является абсолютным
        /// </summary>
        /// <param name="p">Проверяемый файловый путь, не сетевой.</param>
        /// <returns></returns>
        public static bool IsAbsolutePath(string p)
        {
            if (p == null) throw new ArgumentException("Path is null", "p");
            //если путь - пустая строка то это точно локальный путь.
            if (p == String.Empty)
                return false;
            //проверяем
            String vol = Path.GetPathRoot(p);
            //returns "" or "\" for relative path, and "C:\" for absolute path
            if (vol.Length != 3)
                return false;
            //первый символ должен быть буквой дискового тома
            return (Char.IsLetter(vol, 0));
        }

        #endregion

        /// <summary>
        /// RT-получить размер свободного места на диске.
        /// С учетом дисковой квоты пользователя, итп.
        /// </summary>
        /// <param name="volume">Буква тома</param>
        /// <returns>Размер свободного места на томе</returns>
        public static long ПолучитьРазмерСвободногоМестаНаТоме(string volume)
        {
            DriveInfo di = new DriveInfo(volume);
            return di.AvailableFreeSpace;
        }

        /// <summary>
        /// NT-Получить размер указанного каталога
        /// </summary>
        /// <param name="dirpath">Путь к каталогу</param>
        /// <returns>Возвращает размер указанного каталога в байтах</returns>
        public static long GetDirectorySize(string dirpath)
        {
            DirectoryInfo d = new DirectoryInfo(dirpath);
            return getDirectorySizeRecursive(d);
        }
        /// <summary>
        /// NT-Получить размер указанного каталога рекурсивно
        /// </summary>
        /// <param name="d">Объект каталога</param>
        /// <returns>Возвращает размер указанного каталога в байтах</returns>
        private static long getDirectorySizeRecursive(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += getDirectorySizeRecursive(di);
            }
            return (size);
        }

        #region Get files

        /// <summary>
        /// NT-Получить из каталога файлы, соответствующие допустимым расширениям файлов.
        /// </summary>
        /// <param name="srcFolder">Путь к папке проекта.</param>
        /// <param name="exts">Допустимые расширения файлов, с точкой.</param>
        /// <param name="option">Способ поиска в каталогах.</param>
        /// <returns></returns>
        public string[] getSrcFiles(string srcFolder, string[] exts, SearchOption option)
        {
            //TODO: после отладки функции добавить этот код функции в MyCodeLibrary.FileOperations.CFileOperations 
            //public static string[] getFolderFilesByExts(String folder, string[] fileExtensions, SearchOption option)
            //как более оптимизированный способ выбрать все файлы с заданными расширениями.

            //TODO: выдает ошибку, если в каталоге встречается папка или файл с запрещенным доступом.
            //Например, папка D:\SystemVolumeInformation
            //После нее исполнение останавливается и прочие папки не просматриваются.
            //- Надо обходить каталоги рекурсивно, игнорируя эти исключения доступа.
            //- Замена паттерна * на *.* также выдает исключение доступа.
            //string[] files = Directory.GetFiles(srcFolder, "*", SearchOption.AllDirectories);

            //переделано на рекурсивный обход каталога и перехват исключений
            string[] files = getFiles(srcFolder, option);

            List<string> result = new List<string>();
            foreach (String s in files)
            {
                if (isAllowedExtension(s, exts))
                    result.Add(s);
            }

            return result.ToArray();
        }

        /// <summary>
        /// NT-убедиться, что файл имеет допустимое расширение.
        /// </summary>
        /// <param name="s">Путь к файлу</param>
        /// <param name="exts">Допустимые расширения файлов, с точкой</param>
        /// <returns></returns>
        private static bool isAllowedExtension(string s, string[] exts)
        {
            string ext = Path.GetExtension(s); //'.ext'
            foreach (string t in exts)
                if (String.Equals(ext, t, StringComparison.OrdinalIgnoreCase) == true)
                    return true;

            return false;
        }
        /// <summary>
        /// NT-Получить все файлы из каталога
        /// </summary>
        /// <param name="srcFolder">Путь к папке проекта</param>
        /// <param name="option">Способ поиска в каталогах.</param>
        /// <returns></returns>
        private static string[] getFiles(string srcFolder, SearchOption option)
        {
            List<String> result = new List<string>();
            DirectoryInfo di = new DirectoryInfo(srcFolder);
            recursiveGetFiles(result, di, option);
            //при исключении запрета доступа к папке алгоритм должен осмотреть остальные файлы и папки,
            //чтобы не пропускать файлы и папки из-за отсутствия прав доступа.

            return result.ToArray();
        }
        /// <summary>
        /// NT-Рекурсивная функция обхода каталогов
        /// </summary>
        /// <param name="result">Список найденных путей файлов.</param>
        /// <param name="di"><c>DirectoryInfo</c> просматриваемого каталога.</param>
        /// <param name="option">Способ поиска в каталогах.</param>
        private static void recursiveGetFiles(List<string> result, DirectoryInfo di, SearchOption option)
        {
            //при исключении запрета доступа к папке алгоритм должен осмотреть остальные файлы и папки,
            //чтобы не пропускать файлы и папки из-за отсутствия прав доступа.
            
            try
            {
                //process files
                FileInfo[] fi = di.GetFiles();
                foreach (FileInfo f in fi)
                {
                    result.Add(f.FullName);
                }
                //process directories
                if (option != SearchOption.AllDirectories)
                    return;

                DirectoryInfo[] dir = di.GetDirectories();
                
                foreach (DirectoryInfo d in dir)
                    recursiveGetFiles(result, d, option);//тут вызывает исключения, когда пытается открыть заблокированную папку, и все файлы в ней не попадают в выходной список, и это правильно, это хорошо. 
            }
            catch (Exception ex)
            {
                ;//тут исключение игнорировать или сообщить приложению ?
                //OnMessage(new ApplicationMessageEventArgs(0, String.Format("Ошибка {0} для {1}", ex.GetType().ToString(), di.FullName)));
            }
            return;
        }


        #endregion
        /// <summary>
        /// NT-Получить словарь счетчиков расширений файлов, размещающихся в указанном каталоге.
        /// </summary>
        /// <param name="folder">Путь к каталогу, в котором производится поиск</param>
        /// <param name="option">Искать ли во вложенных каталогах</param>
        /// <returns>Возвращает словарь счетчиков расширений файлов</returns>
        public static Dictionary<String, int> getCounterDictionaryOfFileExtensions(String folder, SearchOption option)
        {
            Dictionary<String, int> dic = new Dictionary<string, int>();
            DirectoryInfo di = new DirectoryInfo(folder);
            recursiveGetExtensions(dic, di, option);

            return dic;
        }

        /// <summary>
        /// NT-Служебная функция рекурсивного обхода каталога
        /// </summary>
        /// <param name="dic">Словарь счетчиков расширений</param>
        /// <param name="di">DirectoryInfo объект просматриваемого каталога</param>
        /// <param name="option">Искать ли во вложенных каталогах</param>
        private static void recursiveGetExtensions(Dictionary<string, int> dic, DirectoryInfo di, SearchOption option)
        {
            //process files
            FileInfo[] fi = di.GetFiles();
            foreach (FileInfo f in fi)
            {
                String ext = f.Extension;//example: .txt
                if (dic.ContainsKey(ext))
                {
                    int t = dic[ext];
                    dic[ext] = t + 1;
                }
                else
                {
                    dic.Add(ext, 1);
                }
            }
            //process directories
            if (option != SearchOption.AllDirectories)
                return;

            DirectoryInfo[] dir = di.GetDirectories();
            foreach (DirectoryInfo d in dir)
                recursiveGetExtensions(dic, d, option);

            return;
        }

        /// <summary>
        /// NT-Возвращает массив путей файлов с указаными расширениями 
        /// </summary>
        /// <param name="extensions">
        /// Строка с расширениями файлов. Пример: ".txt .diz" 
        /// </param>
        /// <param name="folder">Путь к каталогу, в котором производится поиск</param>
        /// <param name="option">Искать ли во вложенных каталогах</param>
        /// <returns>Возвращает массив путей файлов с указаными расширениями </returns>
        public static string[] getFolderFilesByExts(String folder, string extensions, SearchOption option)
        {
            //тут надо предусмотреть случаи, когда строка расширений пустая
            return getFolderFilesByExts(folder, parseFileExtensionsString(extensions), option);
        }

        /// <summary>
        /// NT-Возвращает массив путей файлов с указанными расширениями 
        /// </summary>
        /// <param name="folder">Путь к каталогу, в котором производится поиск</param>
        /// <param name="fileExtensions">Массив с расширениями искомых файлов</param>
        /// <param name="option">Искать ли во вложенных каталогах</param>
        /// <returns>Возвращает массив путей файлов с указанными расширениями</returns>
        public static string[] getFolderFilesByExts(String folder, string[] fileExtensions, SearchOption option)
        {
            //Тут код по каждому расширению выбирает файлы из каталога заново
            //и при помощи словаря проверяет что они не были выбраны ранее
            //Это долго для диска С: , но займет меньше памяти, чем если 
            //выбрать все файлы и потом их отбраковывать по расширениям.
            //Но надо иметь и второй способ отбора, который сейчас реализован в проекте Nado.TodoEngine.GetSrcFiles(..)
            //TODO: реализовать второй способ как getFolderFilesByExts2(..)
            //TODO: Описать, в чем разница и преимущества каждого из способов
			
			Dictionary<String, String> files = new Dictionary<string, string>();
            foreach (String ext in fileExtensions)
            {
                String[] sar = Directory.GetFiles(folder, "*" + ext, option);
                foreach (String sa in sar)
                {
                    if (!files.ContainsKey(sa))
                        files.Add(sa, sa);
                }
            }
            //copy to result array
            String[] result = new String[files.Count];
            int i = 0;
            foreach (KeyValuePair<String, String> kvp in files)
            {
                result[i] = kvp.Key;
                i++;
            }

            return result;
        }

        /// <summary>
        /// NT-Разделяет строку расширений файлов на массив расширений файлов
        /// </summary>
        /// <param name="extensions">
        /// Строка с расширениями файлов. 
        /// Расширение начинается с точки и отделяется пробелом
        /// Пример: ".txt .diz" 
        /// </param>
        /// <returns>Возвращает массив строк расширений файлов</returns>
        public static string[] parseFileExtensionsString(string extensions)
        {
            char[] splitter = new char[] { ' ' };
            String[] result = extensions.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

            return result;
        }

        /// <summary>
        /// NT-убирает точку и делает следующую букву в верхний регистр.
        /// Пример: .pdf -> Pdf
        /// </summary>
        /// <param name="p">расширение файла: .pdf</param>
        /// <returns></returns>
        public static string createDocTypeFromFileExtension(string p)
        {
            //добавлена в движок в класс Utility для использования в подобных случаях.
            StringBuilder sb = new StringBuilder();
            char cc = p[0];
            foreach (char c in p)
            {
                if (c != '.')
                    if (cc == '.')
                        sb.Append(Char.ToUpper(c));
                    else sb.Append(Char.ToLower(c));
                //save to next use
                cc = c;
            }
            return sb.ToString();
        }

        /// <summary>
        /// RT-Получить название для очередной подпапки в указанном каталоге.
        /// </summary>
        /// <remarks>
        /// Эта функция используется в случае, когда в некотором каталоге нужно создать ряд подкаталогов для хранения множества файлов.
        /// Новая папка отличается от существующих порядковым номером, увеличенным на 1.
        /// Функция возвращает название для новой подпапки, но не создает ее.
        /// </remarks>
        /// <param name="rootFolder">Путь к родительской папке коллекции</param>
        /// <param name="folderTitlePart">Шаблон названия подпапки. Пример: "folder-"</param>
        /// <returns>Возвращает название для очередной подпапки в указанном каталоге.</returns>
        /// <example>
        /// <code>
        ///    String rootFolder = "c:\\Temp";
        ///    for (int i = 0; i < 5; i++)
        ///    {
        ///        String newFolderTitle = CFileOperations.getTitleOfNewSubfolder(rootFolder, "folder-");
        ///        Directory.CreateDirectory(Path.Combine(rootFolder, newFolderTitle));
        ///        Console.WriteLine(newFolderTitle);
        ///    }
        ///</code>
        ///    /*  выводит в консоль:
        ///        folder-1
        ///        folder-2
        ///        folder-3
        ///        folder-4
        ///        folder-5
        ///        Press Enter to finish...
        ///     * */
        /// </example>
        public static string getTitleOfNewSubfolder(String rootFolder, String folderTitlePart)
        {
            //1 получить все подпапки в корневой папке
            string[] dirs = Directory.GetDirectories(rootFolder, folderTitlePart + "*");
            //2 найти максимальный номер из них
            int max = 0;
            foreach (string dirpath in dirs)
            {
                String foldername = Path.GetFileName(dirpath);
                String num = foldername.Remove(0, folderTitlePart.Length);
                int number = Int32.Parse(num);
                //set new max value
                if (number > max)
                    max = number;
            }
            //3 собрать новый номер и новое название папки
            max++;
            String result = folderTitlePart + max.ToString();

            return result;
        }
        /// <summary>
        /// NT- Создать путь к файлу, избежать одинаковых имен файлов.
        /// Это решение для организации папки выходных файлов.
        /// </summary>
        /// <param name="newFileName">Название для создаваемого файла, без расширения</param>
        /// <param name="newFileExtension">Расширение для создаваемого файла, с точкой</param>
        /// <param name="baseFolderPath">Базовый каталог для подкаталогов хранения файлов.</param>
        /// <returns>
        /// Функция возвращает путь подкаталога для размещения файла. 
        /// Пример: baseFolderPath\0\
        /// </returns>
        /// <remarks>
        /// Это решение для организации папки выходных файлов.
        /// Это способ размещения файлов в каталоге baseFolderPath. 
        /// Файлы добавляются в подпапки с номерами 0..N.
        /// Имя и расширение файла передаются раздельно, поскольку в исходном примере используется одно название и несколько расширений файлов.
        /// </remarks>
        /// <example>
        /// <code>
        /// //создать название документа
        /// String newName = "document";
        /// //проверить, что документа с таким именем еще нет в выходной папке.
        /// //Иначе назначить следующий подкаталог в выходной папке для файлов.
        /// String newPath = makeNewPath(newName, ".pdf", "C:\\workfiles");
        /// //Переместить - переименовать файл пдф в папку выходных файлов.
        /// String newPdfFilePath = Path.Combine(newPath, newName + ".pdf");
        /// File.Move(srcPdfFolder, newPdfFilePath);
        /// </code> 
        /// </example>
        public static string makeNewPath(String newFileName, String newFileExtension, String baseFolderPath)
        {
            int folderIndex = 0;
            String newFolderPath = null;
            while(true)
            {
                newFolderPath = Path.Combine(baseFolderPath, folderIndex.ToString());
                if(!Directory.Exists(newFolderPath))
                    Directory.CreateDirectory(newFolderPath);
                String newFilePath = Path.Combine(newFolderPath, newFileName + newFileExtension);
                if(!File.Exists(newFilePath))
                    break;
                else
                    folderIndex++;
            }

            return newFolderPath;
        }



        #region FileInfo extensions - наскоро накиданы недоделаны

        /// <summary>
        /// Renames a file.
        /// </summary>
        /// <param name = "file">The file.</param>
        /// <param name = "newName">The new name.</param>
        /// <returns>The renamed file</returns>
        /// <example>
        /// 	<code>
        /// 		var file = new FileInfo(@"c:\test.txt");
        /// 		file.Rename("test2.txt");
        /// 	</code>
        /// </example>
        public static FileInfo Rename(FileInfo file, String newName)
        {
            String filePath = Path.Combine(Path.GetDirectoryName(file.FullName), newName);
            file.MoveTo(filePath);
            return file;
        }
        /// <summary>
        /// Renames a file without changing its extension.
        /// </summary>
        /// <param name = "file">The file.</param>
        /// <param name = "newName">The new name.</param>
        /// <returns>The renamed file</returns>
        /// <example>
        /// 	<code>
        /// 		var file = new FileInfo(@"c:\test.txt");
        /// 		file.RenameFileWithoutExtension("test3");
        /// 	</code>
        /// </example>
        public static FileInfo RenameFileWithoutExtension(FileInfo file, String newName)
        {
            string fileName = String.Concat(newName, file.Extension);
            CFileOperations.Rename(file, fileName);
            return file;
        }
        /// <summary>
        /// Changes the files extension.
        /// </summary>
        /// <param name = "file">The file.</param>
        /// <param name = "newExtension">The new extension.</param>
        /// <returns>The renamed file</returns>
        /// <example>
        /// 	<code>
        /// 		var file = new FileInfo(@"c:\test.txt");
        /// 		file.ChangeExtension("xml");
        /// 	</code>
        /// </example>
        public static FileInfo ChangeExtension(FileInfo file, String newExtension)
        {
            newExtension = CStringProcessor.EnsureStartsWith(newExtension, ".", true);
            string fileName = String.Concat(Path.GetFileNameWithoutExtension(file.FullName), newExtension);
            CFileOperations.Rename(file, fileName);
            return file;
        }
        /// <summary>
        /// Changes the extensions of several files at once.
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "newExtension">The new extension.</param>
        /// <returns>The renamed files</returns>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		files.ChangeExtensions("tmp");
        /// 	</code>
        /// </example>
        public static FileInfo[] ChangeExtensions(FileInfo[] files, String newExtension)
        {
            //files.ForEach(f => f.ChangeExtension(newExtension));
            foreach(FileInfo fi in files)
            {
                CFileOperations.ChangeExtension(fi, newExtension);
            }
            return files;
        }
        /// <summary>
        /// Deletes several files at once and consolidates any exceptions.
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		files.Delete()
        /// 	</code>
        /// </example>
        public static void Delete(FileInfo[] files)
        {
            CFileOperations.Delete(files, true);
        }
        /// <summary>
        /// Deletes several files at once and optionally consolidates any exceptions.
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "consolidateExceptions">if set to <c>true</c> exceptions are consolidated and the processing is not interrupted.</param>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		files.Delete()
        /// 	</code>
        /// </example>
        public static void Delete(FileInfo[] files, Boolean consolidateExceptions)
        {

            if (consolidateExceptions)
            {
                List<Exception> exceptions = new List<Exception>();

                foreach (FileInfo file in files)
                {
                    try { file.Delete(); }
                    catch (Exception e) { exceptions.Add(e); }
                }
                if (exceptions.Count > 0)
                    throw CombinedException.Combine("Error while deleting one or several files, see InnerExceptions array for details.", exceptions.ToArray());
            }
            else 
                foreach (FileInfo file in files) 
                    file.Delete();

            return;
        }
        /// <summary>
        /// Copies several files to a new folder at once and consolidates any exceptions.
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "targetPath">The target path.</param>
        /// <returns>The newly created file copies</returns>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		var copiedFiles = files.CopyTo(@"c:\temp\");
        /// 	</code>
        /// </example>
        public static FileInfo[] CopyTo(FileInfo[] files, String targetPath) 
        {
            return CFileOperations.CopyTo(files, targetPath, true);
        }

        /// <summary>
        /// Copies several files to a new folder at once and optionally consolidates any exceptions.
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "targetPath">The target path.</param>
        /// <param name = "consolidateExceptions">if set to <c>true</c> exceptions are consolidated and the processing is not interrupted.</param>
        /// <returns>The newly created file copies</returns>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		var copiedFiles = files.CopyTo(@"c:\temp\");
        /// 	</code>
        /// </example>
        public static FileInfo[] CopyTo(FileInfo[] files, String targetPath, Boolean consolidateExceptions)
        {
            List<FileInfo> copiedfiles = new List<FileInfo>();
            List<Exception> exceptions = null;

            foreach (FileInfo file in files)
            {
                try
                {
                    string fileName = Path.Combine(targetPath, file.Name);
                    copiedfiles.Add(file.CopyTo(fileName));
                }
                catch (Exception e)
                {
                    if (consolidateExceptions)
                    {
                        if (exceptions == null) exceptions = new List<Exception>();
                        exceptions.Add(e);
                    }
                    else throw;
                }
            }

            if ((exceptions != null) && (exceptions.Count > 0))
                throw new CombinedException("Error while copying one or several files, see InnerExceptions array for details.", exceptions.ToArray());

            return copiedfiles.ToArray();
        }
        /// <summary>
        /// Moves several files to a new folder at once and optionally consolidates any exceptions.
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "targetPath">The target path.</param>
        /// <returns>The moved files</returns>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		files.MoveTo(@"c:\temp\");
        /// 	</code>
        /// </example>
        public static FileInfo[] MoveTo(FileInfo[] files, String targetPath)
        {
            return CFileOperations.MoveTo(files, targetPath, true);
        }

        /// <summary>
        /// Moves several files to a new folder at once and optionally consolidates any exceptions.
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "targetPath">The target path.</param>
        /// <param name = "consolidateExceptions">if set to <c>true</c> exceptions are consolidated and the processing is not interrupted.</param>
        /// <returns>The moved files</returns>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		files.MoveTo(@"c:\temp\");
        /// 	</code>
        /// </example>
        public static FileInfo[] MoveTo(FileInfo[] files, String targetPath, Boolean consolidateExceptions)
        {
            List<Exception> exceptions = null;

            foreach (FileInfo file in files)
            {
                try
                {
                    string fileName = Path.Combine(targetPath, file.Name);
                    file.MoveTo(fileName);
                }
                catch (Exception e)
                {
                    if (consolidateExceptions)
                    {
                        if (exceptions == null) exceptions = new List<Exception>();
                        exceptions.Add(e);
                    }
                    else throw;
                }
            }

            if ((exceptions != null) && (exceptions.Count > 0))
                throw new CombinedException("Error while moving one or several files, see InnerExceptions array for details.", exceptions.ToArray());

            return files;
        }
        /// <summary>
        /// Sets file attributes for several files at once
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "attributes">The attributes to be set.</param>
        /// <returns>The changed files</returns>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		files.SetAttributes(FileAttributes.Archive);
        /// 	</code>
        /// </example>
        public static FileInfo[] SetAttributes(FileInfo[] files, FileAttributes attributes)
        {
            foreach (FileInfo file in files) 
                file.Attributes = attributes;
            return files;
        }
        /// <summary>
        /// Appends file attributes for several files at once (additive to any existing attributes)
        /// </summary>
        /// <param name = "files">The files.</param>
        /// <param name = "attributes">The attributes to be set.</param>
        /// <returns>The changed files</returns>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 		files.SetAttributesAdditive(FileAttributes.Archive);
        /// 	</code>
        /// </example>
        public static FileInfo[] SetAttributesAdditive(FileInfo[] files, FileAttributes attributes)
        {
            foreach (FileInfo file in files) 
                file.Attributes = (file.Attributes | attributes);
            return files;
        }

        #endregion

        #region DirectoryInfo extensions  - наскоро накиданы недоделаны

        /// <summary>
        /// Gets all files in the directory matching one of the several (!) supplied patterns (instead of just one in the regular implementation).
        /// </summary>
        /// <param name = "directory">The directory.</param>
        /// <param name = "patterns">The patterns.</param>
        /// <returns>The matching files</returns>
        /// <remarks>
        /// Выходной массив может содержать повторы путей файлов!	
        /// </remarks>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 	</code>
        /// </example>
        public static FileInfo[] GetFiles(DirectoryInfo directory, params String[] patterns)
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (String pattern in patterns) 
                files.AddRange(directory.GetFiles(pattern));
            return files.ToArray();
        }
        /// <summary>
        /// Searches the provided directory recursively and returns the first file matching the provided pattern.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        /// <param name = "pattern">The pattern.</param>
        /// <returns>The found file</returns>
        /// <example>
        /// 	<code>
        /// 		var directory = new DirectoryInfo(@"c:\");
        /// 		var file = directory.FindFileRecursive("win.ini");
        /// 	</code>
        /// </example>
        public static FileInfo FindFileRecursive(DirectoryInfo directory, String pattern)
        {
            FileInfo[] files = directory.GetFiles(pattern);
            if (files.Length > 0) return files[0];

            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                FileInfo foundFile = CFileOperations.FindFileRecursive(subDirectory, pattern);
                if (foundFile != null) return foundFile;
            }
            return null;
        }
        ///// <summary>
        ///// Searches the provided directory recursively and returns the first file matching to the provided predicate.
        ///// </summary>
        ///// <param name = "directory">The directory.</param>
        ///// <param name = "predicate">The predicate.</param>
        ///// <returns>The found file</returns>
        ///// <example>
        ///// 	<code>
        ///// 		var directory = new DirectoryInfo(@"c:\");
        ///// 		var file = directory.FindFileRecursive(f => f.Extension == ".ini");
        ///// 	</code>
        ///// </example>
        //public static FileInfo FindFileRecursive(DirectoryInfo directory, Func<FileInfo, Boolean> predicate)
        //{
        //    foreach (FileInfo file in directory.GetFiles())
        //    {
        //        if (predicate?.Invoke(file) ?? default(Boolean)) return file;
        //    }

        //    foreach (var subDirectory in directory.GetDirectories())
        //    {
        //        var foundFile = subDirectory.FindFileRecursive(predicate);
        //        if (foundFile != null) return foundFile;
        //    }
        //    return null;
        //}
        /// <summary>
        /// Searches the provided directory recursively and returns the all files matching the provided pattern.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        /// <param name = "pattern">The pattern.</param>
        /// <remarks>
        /// 	This methods is quite perfect to be used in conjunction with the newly created FileInfo-Array extension methods.
        /// </remarks>
        /// <returns>The found files</returns>
        /// <example>
        /// 	<code>
        /// 		var directory = new DirectoryInfo(@"c:\");
        /// 		var files = directory.FindFilesRecursive("*.ini");
        /// 	</code>
        /// </example>
        public static FileInfo[] FindFilesRecursive(DirectoryInfo directory, String pattern)
        {
            List<FileInfo> foundFiles = new List<FileInfo>();
            FindFilesRecursive(directory, pattern, foundFiles);
            return foundFiles.ToArray();
        }
        /// <summary>
        /// Subfunction for FindFilesRecursive
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="pattern"></param>
        /// <param name="foundFiles"></param>
        private static void FindFilesRecursive(DirectoryInfo directory, String pattern, List<FileInfo> foundFiles)
        {
            foundFiles.AddRange(directory.GetFiles(pattern));
            //directory.GetDirectories().ForEach(d => FindFilesRecursive(d, pattern, foundFiles));
            DirectoryInfo[] dis = directory.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                CFileOperations.FindFilesRecursive(di, pattern, foundFiles);
            }
        }
        ///// <summary>
        ///// Searches the provided directory recursively and returns the all files matching to the provided predicate.
        ///// </summary>
        ///// <param name = "directory">The directory.</param>
        ///// <param name = "predicate">The predicate.</param>
        ///// <returns>The found files</returns>
        ///// <remarks>
        ///// 	This methods is quite perfect to be used in conjunction with the newly created FileInfo-Array extension methods.
        ///// </remarks>
        ///// <example>
        ///// 	<code>
        ///// 		var directory = new DirectoryInfo(@"c:\");
        ///// 		var files = directory.FindFilesRecursive(f => f.Extension == ".ini");
        ///// 	</code>
        ///// </example>
        //public static FileInfo[] FindFilesRecursive(DirectoryInfo directory, Func<FileInfo, Boolean> predicate)
        //{
        //    var foundFiles = new List<FileInfo>();
        //    FindFilesRecursive(directory, predicate, foundFiles);
        //    return foundFiles.ToArray();
        //}
        //static void FindFilesRecursive(DirectoryInfo directory, Func<FileInfo, Boolean> predicate, List<FileInfo> foundFiles)
        //{
        //    foundFiles.AddRange(directory.GetFiles().Where(predicate));
        //    directory.GetDirectories().ForEach(d => FindFilesRecursive(d, predicate, foundFiles));
        //}
        /// <summary>
        /// Copies the entire directory to another one
        /// </summary>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="targetDirectoryPath">The target directory path.</param>
        /// <returns></returns>
        public static DirectoryInfo CopyTo(DirectoryInfo sourceDirectory, String targetDirectoryPath)
        {
            DirectoryInfo targetDirectory = new DirectoryInfo(targetDirectoryPath);
            CFileOperations.CopyTo(sourceDirectory, targetDirectory);
            return targetDirectory;
        }
        /// <summary>
        /// Copies the entire directory to another one
        /// </summary>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="targetDirectory">The target directory.</param>
        public static void CopyTo(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
        {
            if (!targetDirectory.Exists) targetDirectory.Create();

            foreach (DirectoryInfo childDirectory in sourceDirectory.GetDirectories())
            {
                DirectoryInfo td = new DirectoryInfo(Path.Combine(targetDirectory.FullName, childDirectory.Name));
                CFileOperations.CopyTo(childDirectory, td);
            }

            foreach (FileInfo file in sourceDirectory.GetFiles())
            {
                file.CopyTo(Path.Combine(targetDirectory.FullName, file.Name));
            }
        }

#endregion

    }
}
