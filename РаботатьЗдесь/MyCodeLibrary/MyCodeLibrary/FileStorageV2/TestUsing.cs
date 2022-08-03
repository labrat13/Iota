using System;
using System.IO;
using System.Text;
using MyCodeLibrary.FileOperations;
using MyCodeLibrary.FileStorageV2;

namespace Test
{
    class TestUsing //Program
    {
        static void Main(string[] args)
        {

            ////test FSM
            //FileStorageManager fsm = new FileStorageManager("D:\\Temp\\FileStorage");
            ////create storage
            //fsm.EnsureStorageExists();
            ////check abs path
            //string absolutePath = "D:\\Temp\\FileStorage\\AAA\\Filename.ext";
            //string relpath = fsm.ConvertAbsoluteToRelativePath(absolutePath);
            //string abspath = fsm.ConvertRelativeToAbsolutePath(relpath);
            //if(!String.Equals(absolutePath, abspath, StringComparison.InvariantCultureIgnoreCase))
            //    throw new Exception();

            ////delete storage
            //fsm.DeleteStorage();

            ////compare two identical files
            //if (!FileStorageManager.IsEqualFileContent("D:\\Temp\\file1.txt", "D:\\Temp\\file2.txt"))
            //    throw new Exception();
            ////compare two not identical files
            //if (FileStorageManager.IsEqualFileContent("D:\\Temp\\file1.txt", "D:\\Temp\\file3.txt"))
            //    throw new Exception();

            ////compare two big identical files
            //DateTime d1 = DateTime.Now;
            ////1.5Gb=0h3m15s слишком долго, без прогрессбара плохо.
            //if (!FileStorageManager.IsEqualFileContent("D:\\Temp\\file1.avi", "D:\\Temp\\file2.avi"))
            //    throw new Exception();
            //TimeSpan d2 = DateTime.Now.Subtract(d1);
            //Console.WriteLine("Длительность сравнения: " + d2.ToString());
            //Console.ReadKey();

            ////проверить удаление пустых каталогов - ок
            ////проверено: удаляются скрытые, архивные и рид-онли, без выдачи исключений.
            //Directory.CreateDirectory("D:\\Temp\\FileStorage\\AAA");
            //Directory.CreateDirectory("D:\\Temp\\FileStorage\\ZZZ");
            //Directory.CreateDirectory("D:\\Temp\\FileStorage\\trtyftyuftgjvyufhv");
            //fsm.RemoveEmptyFolders();

            ////проверить проверку записи - ок
            //fsm = new FileStorageManager("c:\\"); //this path cannot create any file
            //if (!fsm.IsCanWrite())
            //    throw new Exception();

            //Удаляет файлы и папки в корзину
            //ShellFileOperations.DeleteFileOrFolder(fsm.StorageRootFolderPath);

            //тут надо бы тест хранилища целиком сделать, и заносить в файл лога в КСВ формате (строки в кавычках):
            //-исходный путь файла
            //-конечный путь файла
            //-существует ли файл на новом месте после копирования
            //-исключение, если возникло
            //-таймштамп операции
            //- аббревиатуру итоговую: ОК - нормальная операция, EX - было исключение, NO - нет на месте конечного файла, NU - не найден путь для добавляемого файла, итд. 
            //Так отправить в хранилище пару сотен тысяч файлов, и потом посмотреть, как они там поместились, и за какое время.
            //Только как такой лог просматривать, без БД? Поиском.
            //testUsing();

            startBigTest();

            testUsing();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private const String storagePath = "C:\\Temp\\FileStorage";

        private const String sourcePath = "C:\\Temp\\TestContent";

        private static void testUsing()
        {
            try
            {
                //тест обычного использования хранилища
                
                //open log file
                StreamWriter log = new StreamWriter("C:\\Temp\\UsingLog.txt", true, Encoding.Default);
                log.AutoFlush = true;
                
                //open folder with source files
                string[] srcFiles = Directory.GetFileSystemEntries(sourcePath);
                
                //приложение инициализируется
                FileStorageManagerV2 fsm = new FileStorageManagerV2(storagePath);
                if (!fsm.CanWrite) log.WriteLine(DateTime.Now.ToLongTimeString() + "  Storage is Read-Only");
                log.WriteLine(DateTime.Now.ToLongTimeString() + "  Storage test started");
                //получить статистику
                log.WriteLine(DateTime.Now.ToLongTimeString() + " Storage " + fsm.GetStatistics().ToString());
                
                //приложение добавляет файлы, если они отсутствуют
                log.WriteLine(DateTime.Now.ToLongTimeString() + "  Add files if not exists");
                foreach (String s in srcFiles)
                {
                    tryAddFileIfNotExists(fsm, log, s);
                }

                //приложение получает статистику
                log.WriteLine(DateTime.Now.ToLongTimeString() + "  Storage " + fsm.GetStatistics().ToString());

                //приложение добавляет новые файлы
                log.WriteLine(DateTime.Now.ToLongTimeString() + "  Add files new");
                foreach (String s in srcFiles)
                {
                    tryAddFile(fsm, log, s);
                }
                
                //приложение получает статистику
                log.WriteLine(DateTime.Now.ToLongTimeString() + "  Storage " + fsm.GetStatistics().ToString());
                
                //приложение закрывается
                log.WriteLine(DateTime.Now.ToLongTimeString() + "  Application exit");
                log.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Test ended");

            return;
        }



        private static void startBigTest()
        {
            try
            {
                //начальная забивка утилиты для использования
                FileStorageManagerV2 fsm = new FileStorageManagerV2(storagePath);
                //open log file
                StreamWriter log = new StreamWriter("C:\\Temp\\bigTestLog.txt", false, Encoding.Default);
                //open folder with source files
                string[] srcFiles = Directory.GetFileSystemEntries(sourcePath);
                Console.WriteLine("Start big test: " + srcFiles.Length.ToString() + " elements");
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("Stage " + i.ToString());
                    foreach (String s in srcFiles)
                    {
                        tryAddFile(fsm, log, s);
                    }
                }
                log.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Test ended");

            return;
        }

        private static void tryAddFileIfNotExists(FileStorageManagerV2 fsm, StreamWriter log, String oldpath)
        {
            String exmsg = String.Empty;
            String abbr = "OK";
            String newPath = String.Empty;
            String exists = "fail";
            try
            {
                //Ищем существующий файл или папку. Для папок сейчас всегда должен возвращаться null, так как сравнивать папки нечем.
                newPath = fsm.FindPathForFileAnalog(oldpath);
                //если не найдено
                if (newPath == null)
                {
                    //добавляем файл как новый
                    //ищемсвободный путь для создания нового файла
                    newPath = fsm.FindPathForFileNew(oldpath);
                    //если ничего не нашлось
                    if (newPath != null)
                    {
                        //если движок неправильно сработал, и файл по этому пути уже существует
                        if (File.Exists(newPath) || Directory.Exists(newPath))
                            throw new IOException("Файл уже существует!");
                        //Копировать или переместить

                        ShellFileOperations.CopyFile(oldpath, newPath);
                    }
                }
                //возвращаем путь к найденному файлу

            }
            catch (Exception ex)
            {
                exmsg = ex.GetType().ToString() + " : " + ex.Message;
                abbr = "EX";
            }
            //process data for log text 
            if (newPath == null)
            {
                abbr = "NU";
                newPath = "null";
            }
            else
            {
                if (Directory.Exists(newPath) || File.Exists(newPath))
                {
                    exists = "Y";
                    abbr = "OK";
                }
                else
                {
                    abbr = "NO";
                    exists = "N";
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}\t{2}", abbr, DateTime.Now.ToLongTimeString(), oldpath);
            sb.AppendFormat("\t{0}\t{1}\t{2}", newPath, exists, exmsg);
            log.WriteLine(sb.ToString());
            log.Flush();
        }

        private static void tryAddFile(FileStorageManagerV2 fsm, StreamWriter log, String oldpath)
        {
            String exmsg = String.Empty;
            String abbr = "OK";
            String newPath = String.Empty;
            String exists = "fail";
            try
            {
                //newPath = fsm.AddFileAnyway(oldpath, true);
                //ищемсвободный путь для создания нового файла
                newPath = fsm.FindPathForFileNew(oldpath);
                //если ничего не нашлось
                if (newPath != null)
                {
                    //если движок неправильно сработал, и файл по этому пути уже существует
                    if (File.Exists(newPath) || Directory.Exists(newPath))
                        throw new IOException("Файл уже существует!");
                    //Копировать или переместить

                    ShellFileOperations.CopyFile(oldpath, newPath);
                }
            }
            catch (Exception ex)
            {
                exmsg = ex.GetType().ToString() + " : " + ex.Message;
                abbr = "EX";
            }
            //process data for log text 
            if (newPath == null)
            {
                abbr = "NU";
                newPath = "null";
            }
            else
            {
                if (Directory.Exists(newPath) || File.Exists(newPath))
                {
                    exists = "Y";
                    abbr = "OK";
                }
                else
                {
                    abbr = "NO";
                    exists = "N";
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}\t{2}", abbr, DateTime.Now.ToLongTimeString(), oldpath);
            sb.AppendFormat("\t{0}\t{1}\t{2}", newPath, exists, exmsg);
            log.WriteLine(sb.ToString());
            log.Flush();
        }



    }
}
