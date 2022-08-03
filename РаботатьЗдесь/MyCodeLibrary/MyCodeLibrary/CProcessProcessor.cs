using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace MyCodeLibrary
{
    //Permissions: SecurityPermission 
    //for calling members of Process.Demand value: LinkDemand; Named Permission Sets: FullTrust.

    //24092017 - тестировано успешно.
    //16042019 - добавлены функции запуска процесса просмотра документа и конструктор

    public class CProcessProcessor
    {
        /// <summary>
        /// Процесс просмотра документа
        /// </summary>
        private Process m_docProcess;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CProcessProcessor()
        {
            this.m_docProcess = null;
        }
        #region Функции просмотра документа
        /// <summary>
        /// RT-запустить просмотр файла-документа
        /// </summary>
        /// <param name="filepath">Путь к файлу-документу: полный или относительно текущего каталога приложения.</param>
        /// <exception cref="FileNotFoundException">Выбрасывает исключение, если указанный файл документа не найден.</exception>
        private void OpenDocument(String filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("Ошибка: Файл не найден", filepath);
            else
            {
                //запустить просмотр документа тут
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = false;
                psi.FileName = filepath;
                psi.WindowStyle = ProcessWindowStyle.Normal;

                this.m_docProcess = Process.Start(psi);
            }

            return;
        }
        /// <summary>
        /// NT-если просмотр документа уже запущен, закрыть его.
        /// </summary>
        private void CloseDocument()
        {
            if (this.m_docProcess == null)
                return;
            //else
            //возможно, пользователь уже закрыл окно сам
            if (!this.m_docProcess.HasExited)
            {
                //если процесс еще существует
                this.m_docProcess.CloseMainWindow();
                this.m_docProcess.WaitForExit(3000);
                if (!this.m_docProcess.HasExited)
                    this.m_docProcess.Kill();
            }
            //clear variable
            this.m_docProcess = null;

            return;
        }
        #endregion


        #region Static functions
        /// <summary>
        /// Закрыть приложение
        /// </summary>
        /// <param name="appPath">Путь к исполняемому файлу приложения</param>
        /// <param name="kill">True - убить процесс, False - только попытаться закрыть главное окно приложения.</param>
        /// <param name="timeout">Таймаут ожидания завершения операции в миллисекундах. Используется дважды. По умолчанию = 5000</param>
        /// <returns>Возвращает результат операции: успех или неудача.</returns>
        public static bool CloseApplicationByAppPath(String appPath, bool kill, int timeout)
        {
            bool closed = false;
            try
            {
                Process p = null;
                List<Process> processes = GetProcessesByAppPath(appPath);
                //select app
                if (processes.Count > 1)
                {
                    //Console.WriteLine("Too many active apps!");
                    return false;//error: Not single opened application
                }
                else if (processes.Count == 1)
                    p = processes[0];
                //clear list
                processes = null;

                //closing                
                if (p != null)
                {
                    closed = CloseProcess(p, kill, timeout);
                }
            }
            catch (Exception ex)
            {
                closed = false;
            }
            return closed;
        }


        /// <summary>
        /// Закрыть приложение
        /// </summary>
        /// <param name="titleText">Путь к исполняемому файлу приложения</param>
        /// <param name="kill">True - убить процесс, False - только попытаться закрыть главное окно приложения.</param>
        /// <param name="timeout">Таймаут ожидания завершения операции в миллисекундах. Используется дважды. По умолчанию = 5000</param>
        /// <returns>Возвращает результат операции: успех или неудача.</returns>
        public static bool CloseApplicationByMainWindowTitle(String titleText, bool kill, int timeout)
        {
            bool closed = false;
            try
            {
                Process p = null;
                List<Process> processes = GetProcessesByMainWindowTitle(titleText);
                //select app
                if (processes.Count > 1)
                {
                    //Console.WriteLine("Too many active apps!");
                    return false;//error: Not single opened application
                }
                else if (processes.Count == 1)
                    p = processes[0];
                //clear list
                processes = null;

                //closing                
                if (p != null)
                {
                    closed = CloseProcess(p, kill, timeout);
                }
            }
            catch (Exception ex)
            {
                closed = false;
            }
            return closed;
        }



        /// <summary>
        /// Завершить процесс
        /// </summary>
        /// <param name="process">Объект экземпляра процесса</param>
        /// <param name="kill">True - убить процесс, False - только попытаться закрыть главное окно приложения.</param>
        /// <param name="timeout">Таймаут ожидания завершения операции в миллисекундах. Используется дважды. По умолчанию = 5000</param>
        /// <returns>Возвращает результат операции: успех или неудача.</returns>
        public static bool CloseProcess(Process process, bool kill, int timeout)
        {
            bool closed = false;

            if (process == null)
                throw new ArgumentNullException("process");
            //closing                
            process.CloseMainWindow();
            //а вот тут Блокнот возвращает труе, а окно не закрывает а мессагобоксом предлагает сохранить изменения.
            closed = process.WaitForExit(timeout);
            //а сколько именно надо ждать - зависит от приложения и загрузки компьютера. Может и десять минут закрываться.
            //Можно мониторить свободные ресурсы машины и по ним назначать верхний порог таймаута
            //но это надо опыты ставить, и опять же, это от приложения зависит.
            if ((closed == false) && (kill == true))
            {
                process.Kill();
                //wait for exit
                closed = process.WaitForExit(timeout);
            }
            process.Close();

            return closed;
        }

        /// <summary>
        /// Получить список всех процессов текущей машины
        /// </summary>
        /// <returns></returns>
        public static List<Process> GetAllProcesses()
        {
            Process[] par = Process.GetProcesses();
            List<Process> lip = new List<Process>(par);
            return lip;
        }

        /// <summary>
        /// Получить список запущенных экземпляров указанного приложения
        /// </summary>
        /// <param name="appPath">Путь к исполняемому файлу приложения</param>
        /// <returns>Список запущенных экземпляров указанного приложения</returns>
        public static List<Process> GetProcessesByAppPath(String appPath)
        {
            //find all app objects by name
            Process[] par = Process.GetProcesses();
            List<Process> lip = new List<Process>();
            foreach (Process pr in par)
            {
                //тут так сложно потому, что есть процессы, доступ к которым вызывает исключение.
                String moduleFileName = safeGetModuleFileName(pr);//безопасно получаем путь к файлу приложения
                if (moduleFileName != null)
                {
                    if (String.Compare(moduleFileName, appPath, true) == 0)
                    {
                        lip.Add(pr);
                    }
                }
            }
            //return
            return lip;
        }

        /// <summary>
        /// Безопасно получить путь к исполняемому файлу процесса
        /// </summary>
        /// <remarks>
        /// Некоторые процессы выбрасывают исключение "Недостаточно прав доступа" при попытке чтения имени модуля.
        /// Эта функция блокирует в себе эти исключения, возвращая null вместо имени модуля.
        /// </remarks>
        /// <param name="p">Объект процесса</param>
        /// <returns>Возвращает путь к исполняемому файлу процесса или null при ошибке доступа.</returns>
        public static string safeGetModuleFileName(Process p)
        {
            string result = null;
            try
            {
                result = p.MainModule.FileName;
            }
            catch(Exception ex)
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Получить список запущенных экземпляров указанного приложения
        /// </summary>
        /// <param name="appPath">Текст заголовка главного окна приложения</param>
        /// <returns>Список запущенных экземпляров указанного приложения</returns>
        public static List<Process> GetProcessesByMainWindowTitle(String titleText)
        {
            //find all app objects by name
            Process[] par = Process.GetProcesses();
            List<Process> lip = new List<Process>();
            foreach (Process pr in par)
                if (String.Compare(pr.MainWindowTitle, titleText, true) == 0)
                {
                    lip.Add(pr);
                }
            //return
            return lip;
        }


        #endregion

        #region Запуск программ в консоли

        /// <summary>
        /// Output object that is returned after the CLI application has completed
        /// </summary>
        public class Output
        {

            private List<string> m_Result;

            private List<string> m_Error;

            private int m_ExitCode;

            private String m_Exception;

            /// <summary>
            /// Default constructor
            /// </summary>
            public Output()
            {
                this.m_Error = new List<String>();
                this.m_Exception = String.Empty;
                this.m_ExitCode = 0;
                this.m_Result = new List<String>();
            }

            //Этих коллекций нет в NET Framework 2.0 - они есть в 3.0, и они потокобезопасные.
            //Я их заменил пока простыми списками, авось прокатит.
            /// <summary>
            /// Returns the text result of the command.
            /// </summary>
            public List<string> Result
            {
                get { return m_Result; }
            }

            /// <summary>
            /// Returns the error if an error occurred.
            /// </summary>
            public List<string> Error
            {
                get { return m_Error; }
            }
            /// <summary>
            /// Returns the exit code. Returns 0 if no error occurred.
            /// </summary>
            public int ExitCode
            {
                get { return m_ExitCode; }
                internal set { m_ExitCode = value; }
            }
            /// <summary>
            /// Returns the exception if an exception occurred.
            /// </summary>
            public String Exception
            {
                get { return m_Exception; }
                internal set { m_Exception = value; }
            }

            //заменены из

            ///// <summary>
            ///// Returns the text result of the command.
            ///// </summary>
            //public ObservableCollection<String> Result { get; } = new ObservableCollection<String>();

            ///// <summary>
            ///// Returns the error if an error occurred.
            ///// </summary>
            //public ObservableCollection<String> Error { get; } = new ObservableCollection<String>();

            ///// <summary>
            ///// Returns the exit code. Returns 0 if no error occurred.
            ///// </summary>
            //public int ExitCode { get; internal set; } = 0;

            ///// <summary>
            ///// Returns the exception if an exception occurred.
            ///// </summary>
            //public String Exception { get; internal set; } = String.Empty;
        }

        /// <summary>
        /// NR-Runs a command prompt command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Output Run(object command)
        {
            return Run(command, false, false);
        }

        /// <summary>
        /// NR-Runs a command prompt command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Output RunElevated(object command)
        {
            return Run(command, false, true);
        }

        /// <summary>
        /// NR-Runs a command prompt command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="returnOutput"></param>
        /// <returns></returns>
        public static Output Run(object command, Boolean returnOutput)
        {
            return Run(command, returnOutput, false);
        }
        /// <summary>
        /// NR-
        /// </summary>
        /// <param name="command"></param>
        /// <param name="returnOutput"></param>
        /// <param name="elevate"></param>
        /// <returns></returns>
        private static Output Run(object command, Boolean returnOutput, Boolean elevate)
        {
            var output = new Output();
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
                {

                    // The following commands are needed to redirect the Standard output.
                    // This means that it will be redirected to the Process.StandardOutput StreamReader.
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    // Do not create the black window.
                    CreateNoWindow = true
                };

                if (elevate)
                {
                    procStartInfo.RedirectStandardOutput = false;
                    procStartInfo.RedirectStandardError = false;
                    procStartInfo.UseShellExecute = true;
                    procStartInfo.Verb = "runas";
                    procStartInfo.CreateNoWindow = false;
                }

                // Now we create a process, assign its ProcessStartInfo and start it
                using (Process proc = new System.Diagnostics.Process())
                {
                    proc.StartInfo = procStartInfo;
                    proc.Start();

                    if (returnOutput)
                    {
                        String line;
                        // Get the output into a strings
                        while ((line = proc.StandardOutput.ReadLine()) != null)
                        {
                            output.Result.Add(line);
                        }
                        while ((line = proc.StandardError.ReadLine()) != null)
                        {
                            output.Error.Add(line);
                        }
                        output.ExitCode = proc.ExitCode;
                    }
                }
            }
            catch (Exception objException) { output.Exception = Convert.ToString(objException, CultureInfo.CurrentCulture); }

            return output;
        }
        #endregion

    }
}
