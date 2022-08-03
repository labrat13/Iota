using System;
using System.Collections.Generic;
using System.IO;
using MyCodeLibrary.FileOperations;
using MyCodeLibrary.TextProcessing;

namespace MyCodeLibrary.FileStorageV2
{
    /* TODO: Описать здесь, как эта штука работает, где ее можно применить.
     * Привести пример использования этого Хранилища.
     * 
     * Это хранилище файлов приложения.
     * Код изначально был взят из википада и немного переработан. А то в википаде он уж совсем утилитарный и кривоватый.
     * Файлы хранятся в папке хранилища после двух уровней подпапок, только в подпапках второго уровня. 
     * В подпапке второго уровня может быть не более 1024 файлов.
     * Поэтому код может работать на томах FAT32.
     * 
     * Основное использование этого Хранилища - добавить файл и получить путь к этому добавленному файлу.
     * Этот путь добавляется в ссылку а ссылка - в текст документа, и все на этом.
     * Доступ к файлу затем производится обычным способом и никак Хранилищем не контролируется.
     * Так же можно добавлять и папки с файлами.
     * 
     * Если в Хранилище добавляется дубликат существующего файла, 
     *  то можно выбрать: использовать уже существующий файл или все-таки добавить файл-дубликат.
     *  А вот папки - хз какв этом случае обрабатываются - этот случай пока недоделан.
     */
    
    
    /// <summary>
    /// Менеджер файлового хранилища для проекта
    /// </summary>
    /// <remarks>
    /// Это менеджер для хранения файлов по подобию Wikidpad. 
    /// Для текстового редактора, использующего ссылки на файлы.
    /// Например, для Инвентарь.
    /// </remarks>
    public class FileStorageManagerV2
    {
        /// <summary>
        /// Предельное число пользовательских папок и файлов в подкаталоге хранилища
        /// </summary>
        private const int FileCountLimit = 1024;

        #region Fields
        /// <summary>
        /// Предельное число каталогов хранилища в корневом каталоге хранилища
        /// </summary>
        private int m_FolderCountLimit;

        /// <summary>
        /// Путь к корневому каталогу хранилища
        /// </summary>
        private String m_StorageRootFolderPath;

        //TODO: Определиться с назначением и использованием имени корневого каталога хранилища.
        ///// <summary>
        ///// Имя корневого каталога хранилища
        ///// </summary>
        //private String m_StorageRootFolderName;

        /// <summary>
        /// Режим read-only для хранилища
        /// </summary>
        private bool m_canWrite;

        /// <summary>
        /// Генератор случайных чисел для функции быстрого добавления файлов
        /// </summary>
        private Random m_Randomizer;

        #endregion

        
        /// <summary>
        /// Создать менеджер файлового хранилища
        /// </summary>
        /// <param name="storageRootFolderPath">Путь к каталогу Хранилища</param>
        public FileStorageManagerV2(String storageRootFolderPath)
        {
            //установить корневой каталог хранилища
            this.m_StorageRootFolderPath = String.Copy(storageRootFolderPath);
            //установить предельное число подкатегорий хранилища для принятой схемы хранилища
            int s = BaseXCoder.AsciiChars.Length;       // =26 символов
            this.m_FolderCountLimit = (s * s);    //2 символа = 26^2 = 676папок
            //Создать хранилище или проверить что хранилище допускает изменения
            IsCanWrite();// требует this.m_StorageRootFolderPath
            //инициализируем генератор случайных чисел на весь сеанс Хранилища
            this.m_Randomizer = new Random();
        }


        #region Properties
        /// <summary>
        /// Установить или получить режим read-only хранилища
        /// </summary>
        public bool CanWrite
        {
            get { return m_canWrite; }
            set { m_canWrite = value; }
        }

        /// <summary>
        /// Путь к корневому каталогу хранилища
        /// </summary>
        public String StorageRootFolderPath
        {
            get { return m_StorageRootFolderPath; }
            set { m_StorageRootFolderPath = value; }
        }

 

        #endregion


        #region *** Функции управления хранилищем ***

        /// <summary>
        /// NT-Проверить, что хранилище физически доступно для записи
        /// </summary>
        /// <remarks>
        /// Функция устанавливает флаг CanWrite.
        /// Если хранилище не существует, но запись возможна, то оно создается.
        /// </remarks>
        public bool IsCanWrite()
        {
            //TODO: создавать Хранилище когда надо лишь проверить что запись на диск возможна - неправильное решение.
            //функцию надо переделать, и ее пользователей тоже.
            
            m_canWrite = true;
            try
            {
                //создаем каталог хранилища если его еще нет
                if(!IsStorageExists())
                    CreateStorage();
                //создаем временный тестовый файл
                FileInfo fi = new FileInfo(Path.Combine(this.m_StorageRootFolderPath, "testRO.txt"));
                if (!fi.Exists)
                {
                    StreamWriter sw = fi.CreateText();
                    sw.WriteLine("test read-only mode. If you see this file then test fail.");
                    sw.Close();
                }
                fi.Delete();
            }
            catch (Exception)
            {
                m_canWrite = false;
            }
            return m_canWrite;
        }
        /// <summary>
        /// NT-Проверить, что файловое хранилище существует
        /// </summary>
        public bool IsStorageExists()
        {
            bool result = Directory.Exists(this.m_StorageRootFolderPath);
            return result;
        }
        /// <summary>
        /// NT-Проверить и при необходимости создать файловое хранилище
        /// </summary>
        public void EnsureStorageWritable()
        {
            if (IsStorageExists() == false)
            {
                bool res = this.CreateStorage();
                if (res == false) throw new Exception("Storage cannot be created");
            }

            return;
        }
        /// <summary>
        /// NT-Удалить хранилище
        /// </summary>
        /// <param name="useTrashcan">true  - переместить в Корзину Windows, false - удалять.</param>
        /// <returns>Возвращает True, если хранилище было удалено, False если нет.</returns>
        public bool DeleteStorage(bool useTrashcan)
        {
            //нельзя удалять если хранилище в рид-онли режиме
            if (this.m_canWrite == true)
                return SafeRemoveFile(this.m_StorageRootFolderPath, useTrashcan);
            else return false;
        }
        /// <summary>
        /// NT-Создать новое хранилище
        /// </summary>
        /// <returns>Возвращает True если создание удалось, False если не удалось</returns>
        public bool CreateStorage()
        {
            return CreateStorageFolder(this.m_StorageRootFolderPath);
        }

        #endregion


        #region *** Дополнительные функции хранилища ***

        /// <summary>
        /// NT-Создать папку для структуры хранилища  и установить атрибут неиндексирования
        /// </summary>
        /// <param name="pathname">Абсолютный путь к папке</param>
        /// <returns>Возвращает True если создание удалось, False если не удалось</returns>
        public bool CreateStorageFolder(string pathName)
        {
            if (this.m_canWrite == true)
            {
                //создать папку хранилища
                DirectoryInfo di = new DirectoryInfo(pathName);
                di.Create();
                //установить атрибуты, запрещающие индексацию, архивацию и прочее в том же духе
                di.Attributes = (di.Attributes | FileAttributes.NotContentIndexed);

                return true;
            }
            else return false;
        }
        
        /// <summary>
        /// NT-Проверить наличие файла или папки по относительному пути
        /// </summary>
        /// <param name="relFilePath">Путь к проверяемому файлу относительно корневой папки хранилища</param>
        public bool CheckFileExists(string relFilePath)
        {
            //получим абсолютный путь из относительного
            String absPath = this.ConvertRelativeToAbsolutePath(relFilePath);
            //проверим существование файла
            return File.Exists(absPath);
        }

        /// <summary>
        /// NT-Получить относительный путь по абсолютному
        /// </summary>
        /// <param name="absolutePath">Абсолютный путь к файлу</param>
        /// <returns>Возвращает относительный путь к хранилищу или null, если не удалось создать относительный путь</returns>
        public string ConvertAbsoluteToRelativePath(string absolutePath)
        {
            //как выглядит формат относительного пути 
            // AA\ZZ\FileName.ext или AA\ZZ\FolderName

            //надо удалить из абсолютного пути путь к корневой папке хранилища
            //как это сделать?
            String abs = Path.GetFullPath(absolutePath);
            String store = Path.GetFullPath(this.m_StorageRootFolderPath);
            //разделим строку по абсолютному пути
            String[] sar = new string[1];
            sar[0] = store;
            String[] result = abs.Split(sar, StringSplitOptions.None);
            //первый кусок - должна быть пустая строка
            //второй кусок - остаток с относительным путем
            String res = null;
            if (result.Length > 1)
                res = result[1].TrimStart(new char[] { '\\', '/' });

            return res;
        }
        
        /// <summary>
        /// NT-Получить абсолютный путь по относительному
        /// </summary>
        /// <param name="relativePath">Путь к файлу относительно корневой папки хранилища</param>
        public string ConvertRelativeToAbsolutePath(string relativePath)
        {
            return Path.Combine(this.m_StorageRootFolderPath, relativePath);
        }
        
        /// <summary>
        /// NT-Удалить файл или каталог с учетом его блокировки службой индексации или сторонней программой
        /// </summary>
        /// <param name="filepath">Абсолютный путь к файлу или каталогу</param>
        /// <param name="useTrashcan">true  - переместить в Корзину Windows, false - удалять.</param>
        /// <returns>Возвращает True если файл удален, False если возникла ошибка или пользователь отменил операцию.</returns>
        public static bool SafeRemoveFile(string filePath, bool useTrashcan)
        {
            bool result = true;
            try
            {
                if (useTrashcan)
                {
                    //удалять файлы в корзину?
                    result = ShellFileOperations.DeleteFile(filePath);
                }
                else
                {
                    //если каталог, удалить рекурсивно
                    if (IsFolder(filePath))
                        Directory.Delete(filePath, true);
                    else
                        File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// NT-Сравнить два файла по содержимому
        /// </summary>
        /// <param name="filepath1">Абсолютный путь к первому сравниваемому  файлу</param>
        /// <param name="filepath2">Абсолютный путь ко второму сравниваемому  файлу</param>
        public static bool IsEqualFileContent(string filePath1, string filePath2)
        {
            //TODO: тут нужно генерировать события для прогрессбара, так как для больших файлов процесс долгий. 1.5Гб=3мин15сек
            FileInfo di1 = new FileInfo(filePath1);
            FileInfo di2 = new FileInfo(filePath2);
            //проверить что это не каталоги, каталоги нельзя сравнивать здесь. Но что тогда вернуть?
            if ((di1.Attributes & FileAttributes.Directory) == FileAttributes.Directory) return false;
            if ((di2.Attributes & FileAttributes.Directory) == FileAttributes.Directory) return false;
            //сравнить по размеру            
            if (di1.Length != di2.Length) return false;

            //сравнить по содержимому
            FileStream fs1 = di1.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream fs2 = di2.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            Byte[] bar1 = new Byte[4096 * 8];
            Byte[] bar2 = new Byte[4096 * 8];
            int readed1 = 0;
            int readed2 = 0;
            bool result = true;
            do
            {
                readed1 = fs1.Read(bar1, 0, 4096 * 8);
                readed2 = fs2.Read(bar2, 0, 4096 * 8);
                //определить конец файла
                if ((readed1 == 0) || (readed2 == 0))
                {
                    //result = true;
                    break;
                }
                //проверить число прочитанных байт - если не равны, то и файлы разной длины - не одинаковы.                
                if (readed1 != readed2)
                {
                    result = false;
                    break;
                }
                //сравнить содержимое массивов
                for (int i = 0; i < readed1; i++)
                {
                    if (bar1[i] != bar2[i])
                    {
                        result = false;
                        break;
                    }
                }
                //прервать цикл, если массивы неодинаковы
                if (result == false)
                    break;
            }
            while (true);

            fs1.Close();
            fs2.Close();
            return result;
        }

        /// <summary>
        /// NR-Сравнить два каталога по содержимому
        /// </summary>
        /// <param name="testFolderPath">Абсолютный путь к первому сравниваемому каталогу</param>
        /// <param name="srcFolderPath">Абсолютный путь ко второму сравниваемому каталогу</param>
        /// <returns>Возвращает True если оба каталога одинаковы, False в противном случае</returns>
        public static bool IsEqualFolderContent(string testFolderPath, string srcFolderPath)
        {
            //TODO: написать сравнение двух каталогов, когда мне нечего будет делать тут.
            //можно проверить состав и размер входящих в каталог файлов и подкаталогов
            //и в том числе по содержимому файлов.
            //И это будет опупенно долго. Но прикольно.
            //А пока что просто возвращаем, что каталоги не равны.
            //Это заставит функцию поиска аналогов вернуть null, и каталог будет добавлен в хранилище.

            //DirectoryInfo di1 = new DirectoryInfo(testFolderPath);
            //DirectoryInfo di2 = new DirectoryInfo(srcFolderPath);
            ////имя не проверяем - нет смысла
            ////проверяем число элементов
            //DirectoryInfo[] diar1 = di1.GetDirectories();
            //DirectoryInfo[] diar2 = di2.GetDirectories();
            //if (diar1.Length != diar2.Length) return false;
            //FileInfo[] fiar1 = di1.GetFiles();
            //FileInfo[] fiar2 = di2.GetFiles();
            //if (fiar1.Length != fiar2.Length) return false;
            ////теперь надо проверить каждую пару файлов и папок - по размеру и контенту. По дате и имени не надо проверять.
            ////Но это мы оставим на потом - это очень долгая и нудная история, и редко встречающаяся.

            return false;
        }

        /// <summary>
        /// NT-Проверить, является ли указанный файл каталогом 
        /// </summary>
        /// <param name="filepath">Путь к файлу или каталогу</param>
        /// <returns>Возвращает True если путь указывает на каталог. False, если это не каталог.</returns>
        public static bool IsFolder(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            return ((fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory);
        }

        #endregion 


        #region *** Основные функции хранилища ***

        /// <summary>
        /// NT-Добавить файл в хранилище
        /// </summary>
        /// <param name="filePathName">Абсолютный путь к добавляемому  файлу или каталогу</param>
        /// <param name="CopyOrMove">True для Копировать,  False для Переместить</param>
        /// <returns>Возвращает абсолютный путь к файлу или null, если путь не удалось создать.</returns>
        public string AddFileAnyway(string filePathName, bool CopyOrMove)
        {
            //автоматически создать хранилище и проверить возможность записи
            this.EnsureStorageWritable();
            //ищем свободный путь для создания нового файла
            string newPath = FindPathForFileNew(filePathName);
            //если ничего не нашлось
            if (newPath != null)
            {
                //если движок неправильно сработал, и файл по этому пути уже существует
                FileSystemInfo fsi = (FileSystemInfo) new FileInfo(filePathName);
                //if (File.Exists(newPath) || Directory.Exists(newPath))  - два запроса к диску - многовато.
                if(fsi.Exists)
                    throw new IOException("Файл уже существует!");
                //Копировать или переместить
                if (CopyOrMove == true)
                {
                    ShellFileOperations.CopyFile(filePathName, newPath);
                }
                else
                {
                    ShellFileOperations.MoveFile(filePathName, newPath);
                }
            }
            //вовращаем новый путь к файлу, уже скопированному 
            return newPath;
        }

        /// <summary>
        /// NT-Добавить файл, если не существует
        /// </summary>
        /// <param name="filePathName">Абсолютный путь к добавляемому  файлу или каталогу</param>
        /// <param name="CopyOrMove">True для Копировать,  False для Переместить</param>
        /// <returns>Возвращает строку пути к файлу или null, если путь не удалось создать.</returns>
        /// <remarks>Если хранилище достаточно заполнено, поиск аналогов может затянуться надолго.</remarks>
        public string AddFileIfNotExists(string filePathName, bool CopyOrMove)
        {
            //автоматически создать хранилище и проверить возможность записи
            this.EnsureStorageWritable();
            
            //Ищем существующий файл или папку. Для папок сейчас всегда должен возвращаться null, так как сравнивать папки нечем.
            string oldPath = this.FindPathForFileAnalog(filePathName);
            //если не найдено
            if (oldPath == null)
            {
                //добавляем файл как новый
                oldPath = AddFileAnyway(filePathName, CopyOrMove);
            }
            //возвращаем путь к найденному файлу
            return oldPath;
        }

        /// <summary>
        /// NT-Удалить пустые подкаталоги. Запускать дважды.
        /// </summary>
        /// <remarks>
        /// Функция работает только если хранилище можно изменять.
        /// Функция просматривает подкаталоги первого и второго уровня, удаляя пустые подкаталоги.
        /// ЕЕ следует запускать дважды: Сначала чтобы удалить пустые каталоги нижнего уровня, 
        /// потом - каталоги верхнего уровня, которые в первый раз содержали пустые каталоги нижнего уровня.
        /// </remarks>
        public void RemoveEmptyFolders()
        {

            //Если хранилище не в рид-онли, то пробуем удалить
            if (this.m_canWrite == true)
            {
                //список для найденных пустых каталогов
                List<DirectoryInfo> lis = new List<DirectoryInfo>();
                //открываем корневой каталог хранилища
                DirectoryInfo di = new DirectoryInfo(this.m_StorageRootFolderPath);
                //получим список подкаталогов 1 уровня
                DirectoryInfo[] dirs1 = di.GetDirectories();
                //в каждом проверим число файлов и каталогов 
                foreach (DirectoryInfo d in dirs1)
                {
                    //тут можно еще проверить имена каталогов, чтобы удостовериться что они соответствуют правилам и являются каталогами хранилища, а не левыми совсем.
                    FileSystemInfo[] fsi1 = d.GetFileSystemInfos();
                    if (fsi1.Length < 1) //если каталог 1 уровня пустой
                        lis.Add(d);         //добавляем его в список для удаления
                    else
                    {
                        //получим список подкаталогов 2 уровня
                        DirectoryInfo[] dirs2 = d.GetDirectories();
                        //в каждом проверим число файлов и каталогов 
                        foreach (DirectoryInfo dd in dirs2)
                        {
                            //тут можно еще проверить имена каталогов, чтобы удостовериться что они соответствуют правилам и являются каталогами хранилища, а не левыми совсем.
                            FileSystemInfo[] fsi2 = dd.GetFileSystemInfos();
                            if (fsi2.Length < 1) //если каталог пустой
                                lis.Add(dd);         //добавляем его в список для удаления
                        }
                    }
                }
                //удалить все каталоги по списку
                foreach (DirectoryInfo ddd in lis)
                    SafeRemoveFile(ddd.FullName, false);
            }
            return;
        }

        /// <summary>
        /// NT-Получить статистику хранилища. Это долгий процесс просмотра всех файлов Хранилища.
        /// </summary>
        /// <remarks>
        /// Реализовано это функцией, чтобы исключить многократное использование в неправильно спланированных циклах
        /// </remarks>
        /// 
        public FileStorageStatistics GetStatistics()
        {
            return new FileStorageStatistics(this.m_StorageRootFolderPath);
        }


        /// <summary>
        /// NT-Вернуть абсолютный путь к файлу полного аналога или null если полный аналог не найден.
        /// </summary>
        /// <param name="srcFile">Абсолютный путь к образцовому файлу или каталогу для сравнения</param>
        /// <returns>Возвращает путь к полному аналогу файла или каталога, или null если полный аналог не найден.</returns>
        /// <remarks>
        /// Если хранилище достаточно заполнено, поиск аналогов может затянуться надолго.
        /// Аналоги каталогов сейчас ищутся только по имени, так как сравнение по контенту очень долгое и сложное и не сделано.
        /// </remarks>
        public string FindPathForFileAnalog(string srcFile)
        {
            //TODO: надо придумать как передавать события для индикатора прогресса и Application.DoEvents.

            //TODO: надо внести в вики, что аналоги могут быть и под другими именами, но с тем же размером и расширением.
            //Надо искать по маске расширения во всех папках рекурсивно. Иначе можно пропустить файл в подпапке ХЗ как глубоко
            //Будем перебирать все существующие папки и в каждой искать файлы по расширению. Если найдем, то все бросаем и вернем найденный путь
            //пока только для файлов.
            //Потом папки попробуем тоже искать. Их только по имени, без размера и содержимого пока что.
            //Сравнение папок вынесено в отдельную функцию, потом надо добавить сравнение по контенту.

            //Функция может вызываться самостоятельно.
            //Функция не изменяет хранилище, поэтому проверять ридонли флаг не нужно.
            //Если корневой папки хранилища не найдено, выбрасываем исключение.
            //Возвращать null тут не следует, так как отсутствие хранилища это явная ошибка. 
            if(!IsStorageExists()) throw new Exception("Storage not exists");

            //проверим что это папка
            bool isFolder = IsFolder(srcFile);
            //открываем корневой каталог Хранилища
            DirectoryInfo root = new DirectoryInfo(this.m_StorageRootFolderPath);
            
            //создаем паттерн для поиска файла или папки
            string SearchPattern = null;
            if (isFolder) SearchPattern = Path.GetFileName(srcFile);
            else
                SearchPattern = "*" + Path.GetExtension(srcFile); //= "*.pdf" or "*" if no extension
            
            //получим существующие папки 1 уровня Хранилища
            DirectoryInfo[] dar = root.GetDirectories();
            foreach (DirectoryInfo d in dar)
            {
                //проверим что это папка Хранилища. Теоретически, тут не должно быть других папок. А практически - все как всегда.
                if ((d.Name.Length == 2) && (BaseXCoder.isBaseXstring(d.Name, BaseXCoder.AsciiChars)))
                {
                    //получим существующие папки 2 уровня Хранилища
                    DirectoryInfo[] dar2 = root.GetDirectories();
                    foreach (DirectoryInfo d2 in dar2)
                    {
                        //проверим что это папка Хранилища. Теоретически, тут не должно быть других папок. А практически - все как всегда.
                        if ((d2.Name.Length == 2) && (BaseXCoder.isBaseXstring(d2.Name, BaseXCoder.AsciiChars)))
                        {
                            //просматриваем пользовательские папки и файлы
                            if (isFolder) //если ищем аналог папки, то обрабатывать не так как файл
                            {
                                //ищем папки во всех подпапках рекурсивно
                                DirectoryInfo[] dar3 = d2.GetDirectories(SearchPattern, SearchOption.AllDirectories);
                                //теперь каждую найденную папку надо сравнить с образцовой
                                //в отдельной функции
                                //и если нашелся аналог, то вернуть его путь
                                foreach (DirectoryInfo dinfo in dar3)
                                {
                                    //тут можно бы сравнить время модификации, но оно не обязательно различает две копии
                                    if (IsEqualFolderContent(dinfo.FullName, srcFile))
                                        return dinfo.FullName;
                                }
                            }
                            else
                            {
                                //ищем файлы по расширению во всех подпапках рекурсивно
                                FileInfo[] far = d2.GetFiles(SearchPattern, SearchOption.AllDirectories);
                                //Теперь каждый найденный файл надо сравнить с образцовым
                                //в отдельной функции
                                //и если нашелся аналог, то вернуть его путь
                                foreach (FileInfo f in far)
                                {
                                    //тут можно бы сравнить время модификации, но оно не обязательно различает две копии
                                    if (IsEqualFileContent(f.FullName, srcFile))
                                        return f.FullName;
                                }
                            }
                        }
                    }
                }
            }
            //не нашлось полного аналога
            return null;
        }
  
        /// <summary>
        /// NT-Вернуть абсолютный путь для заново добавляемого файла или null если такой путь не найден.
        /// </summary>
        /// <param name="srcFile">Абсолютный путь к исходному файлу</param>
        /// <returns>Возвращает путь для заново добавляемого файла (с именем файла) или null если такой путь не найден.</returns>
        /// <remarks>
        /// Возвращаемый путь может содержать несуществующие папки. 
        /// В этом случае надо создать весь путь перед копированием файла.
        /// А то вдруг пользователь передумает, а папка уже будет создана.
        /// </remarks>
        public string FindPathForFileNew(string srcFile)
        {
            //TODO: надо придумать как передавать события для индикатора прогресса и Application.DoEvents.

            //Функция может вызываться самостоятельно.
            //Функция не изменяет хранилище, поэтому проверять ридонли флаг не нужно.
            //Если корневой папки хранилища не найдено, выбрасываем исключение.
            //Возвращать null тут не следует, так как отсутствие хранилища это явная ошибка. 
            if (!IsStorageExists()) throw new Exception("Storage not exists");

            //создаем паттерн для поиска файла или папки
            string SearchName = Path.GetFileName(srcFile);
            //Инициализируем кодер имен папок
            BaseXCoder coder = new BaseXCoder(BaseXCoder.AsciiChars);

            //Используем генератор случайных чисел.
            //Он гарантирует равномерное распределение файлов по папкам
            //Только если хранилище не заполнено наполовину.
            //Поэтому его используем в первую очередь, а если он не нашел ничего, то ищем последовательным перебором, как выше.

            //Оптимизация: можно выявлять заполненные папки 1 и 2 уровня и вносить их в специальный массив битовых флагов менеджера хранилища.
            //Тогда эти папки можно исключать из результатов ГСЧ, это повысит эффективность метода.
            //Но займет дополнительную память.

            //запускаем попытки быстрого создания пути для нового файла
            //Надо измерить, сколько времени это занимает.
            for (int count = 0; count < this.m_FolderCountLimit; count++)
            {
                //создаем имя папки Хранилища
                String level1folderName = coder.Encode2(this.m_Randomizer.Next(this.m_FolderCountLimit));
                //тут надо рандомайзер перезапустить, а то он будет недостаточно случаен
                this.m_Randomizer = new Random(Environment.TickCount + 7360);//наверно, так будет достаточно хорошо
                String level2folderName = coder.Encode2(this.m_Randomizer.Next(this.m_FolderCountLimit));
                //создаем путь папки Хранилища
                String level2folderPath = Path.Combine(this.m_StorageRootFolderPath, Path.Combine(level1folderName, level2folderName));
                //Если папка не существует или существует но (не содержит имя и имеет свободное место), переходим на следующую итерацию
                if (checkFolderAvailable(level2folderPath, SearchName) == true)
                    return Path.Combine(level2folderPath, Path.GetFileName(srcFile));//собираем полный путь к конечному файлу или папке
            }
            //Мы должны были быстро получить папку для помещения файлов.
            //Но раз мы здесь, то подходящей папки не нашлось.
            //Значит надо применить другой способ, медленный но точный

            //последовательно перебираем все возможные папки Хранилища 1 уровня,
            //если папка отсутствует, то собираем ее путь и возвращаем его.
            //если папка присутствует, то:
            //ищем в ней папку 2 уровня, не существующую или существующую и не заполненную и не содержащую указанное имя файла.
            //если находим, то собираем путь и возвращаем его.
            //если не находим, возвращаемся для поиска в следующей папке 1 уровня.
            //если, в итоге, не нашлось свободного пути, возвращаем null.
 
            //Оптимизация: можно менять направление, откуда искать - с начала или с конца. 
            //Это должно позволить более быстро находить новый путь после заполнения хранилища..

            for (int i = 0; i < this.m_FolderCountLimit; i++)
            {
                //Создать имя для папки 1 уровня
                String level1folderName = coder.Encode2(i);
                //Собрать путь для папки 1 уровня
                String level1folderPath = Path.Combine(this.m_StorageRootFolderPath, level1folderName);
                //Проверить, что папка существует и имеет свободное место
                //Если папка 1 уровня не существует, собираем путь и возвращаем его.
                if (!Directory.Exists(level1folderPath))
                {
                    //собираем полный путь к конечному файлу или папке, используя первое же имя папки 2 уровня
                    return Path.Combine(level1folderPath, Path.Combine(coder.Encode2(0), Path.GetFileName(srcFile)));
                }
                else
                {
                    //Если папка 1 уровня существует, ищем в ней папку 2 уровня, не существующую или существующую и не заполненную и не содержащую указанное имя файла.
                    for (int ii = 0; ii < this.m_FolderCountLimit; ii++)
                    {
                        //Создать имя для папки 2 уровня
                        String level2folderName = coder.Encode2(ii);
                        //Собрать путь для папки 1 уровня
                        String level2folderPath = Path.Combine(level1folderPath, level2folderName);
                        //Если папка не существует или существует но (не содержит имя и имеет свободное место), используем ее
                        if (checkFolderAvailable(level2folderPath, SearchName) == true)
                            return Path.Combine(level2folderPath, Path.GetFileName(srcFile));//собираем полный путь к конечному файлу или папке
                    }
                }
            }
            //Тут мы оказались потому, что имеем полный набор папок 2 уровня, и в каждой или нет места, или есть аналог файла.
            //Поэтому теперь мы можем сгенерировать имя папки 3 уровня и пристраивать ее в каждую папку 2 уровня,
            //проверяя и существование и аналог и место в папке 2 уровня

            return null;//ничего не найдено
        }


        #endregion


        #region *** Служебные функции хранилища ***
        /// <summary>
        /// RT-Проверка что каталог Хранилища пригоден для добавления в него файла 
        /// </summary>
        /// <param name="folderPath">Абсолютный путь к проверяемому каталогу</param>
        /// <param name="filename">Имя файла, который планируется добавить в каталог</param>
        /// <returns>Возвращается true, если проверка прошла успешно, false в противном случае.</returns>
        private bool checkFolderAvailable(string folderPath, string filename)
        {
            //если папка не существует, она не может содержать аналог, следовательно она годится
            if (!Directory.Exists(folderPath)) return true;
            //если же папка существует и не содержит имя и имеет свободное место, то она годится
            return IsFolderHavePlaceNotContainsName(folderPath, filename);
        }
        /// <summary>
        /// NT-Проверить, что каталог имеет свободное место и не содержит каталога или файла с указанным именем
        /// </summary>
        /// <param name="folderPath">Путь к проверяемому каталогу</param>
        /// <param name="filename">Имя для проверки</param>
        /// <returns>Функция возвращает true если проверка успешна и false в противном случае.</returns>
        private static bool IsFolderHavePlaceNotContainsName(string folderPath, string filename)
        {
            //Эта функция объединяет код своих предшественников, чтобы делать один запрос к диску вместо двух.
            //return isFolderHaveFreePlace(folderPath) && (!isFolderContainsName(folderPath, filename));

            DirectoryInfo di = new DirectoryInfo(folderPath);
            FileSystemInfo[] fis = di.GetFileSystemInfos();
            //проверить ограничение числа элементов
            if (fis.Length >= FileCountLimit) return false;
            //проверить наличие имени (имя файла или папки, не паттерн)
            foreach (FileSystemInfo f in fis)
            {
                if (String.Equals(f.Name, filename))
                    return false;
            }

            return true;
        }

        ///// <summary>
        ///// RT-Проверка что каталог содержит файл с указанным именем
        ///// </summary>
        ///// <param name="folderPath">Абсолютный путь к проверяемому каталогу</param>
        ///// <param name="filename">Имя файла, который планируется добавить в каталог</param>
        ///// <returns>Возвращается true, если проверка прошла успешно, false в противном случае.</returns>
        //private bool isFolderContainsName(string folderPath, string filename)
        //{
        //    //ищем аналоги
        //    string[] sar = Directory.GetFileSystemEntries(folderPath, filename);
        //    return (sar.Length > 0);

        //}

        ///// <summary>
        ///// RT-Проверка что каталог имеет свободное место для новых файлов
        ///// </summary>
        ///// <param name="folderPath">Абсолютный путь к проверяемому каталогу</param>
        ///// <returns>Возвращается true, если проверка прошла успешно, false в противном случае.</returns>
        //private bool isFolderHaveFreePlace(string folderPath)
        //{
        //    string[] sar = Directory.GetFileSystemEntries(folderPath);
        //    return (sar.Length < FileCountLimit);
        //}

        #endregion

    }
}
