using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyCodeLibrary
{
    /// <summary>
    /// Представляет файл настроек приложения. Позволяет перезаписывать файл настроек, не теряя комментарии и общую структуру файла.
    /// Имена параметров могут повторяться, их можно получить списком или только первый попавшийся.
    /// Можно создавать файл настроек из кода, добавляя строки в конец списка строк.
    /// Поведение для файла настроек конкретного приложения следует доопределить в производных классах.
    /// </summary>
    /// <remarks>
    /// Это излишне затратный класс по памяти и быстродействию.
    /// На больших файлах он будет сильно тормозить, так как перебирать список строк это долго.
    /// Можно потом отобрать объекты строк-параметров в список или словарь.
    /// Тогда доступ будет быстрее. Но это оставим производным классам. А тут общий функционал.
    /// </remarks>
    /// <example>
    /// <code>
    //        //тест файла настроек для моих приложений
    //        String IniPath = "C:\\Temp\\test.ini";
    //        
    //        MyIniFileBase1 ini = new MyIniFileBase1();
    //        //add some example parameters
    //        ini.AddComment("Это файл инициализации");
    //        ini.AddComment("");
    //        ini.AddComment("");
    //        ini.AddRules();//add file format description header!
    //        ini.AddParameter("Казлы", "тут");
    //        ini.AddComment("");
    //        ini.AddComment("Код для самопроверки");
    //        ini.AddParameter("Код", "12345");
    //        ini.AddComment("");
    //        ini.AddComment("");
    //        ini.AddComment("Код для самопроверки");
    //        ini.AddParameter("Код", "54321");
    //        //set file properties
    //        ini.IniFilePathname = IniPath;
    //        ini.Encoding = Encoding.Unicode;
    //        ini.Save();
    //        ini = null;

    //        //load file
    //        MyIniFileBase1 ini2 = new MyIniFileBase1();
    //        ini2.Load("C:\\Temp\\settings.ini", Encoding.GetEncoding(1251));
    //        //get list of parameter names
    //        List<String> parameters = ini2.getParameterNames();
    //        foreach (String s in parameters)
    //        {
    //            //get all values for this parameter name
    //            List<String> valist = ini2.getValues(s, StringComparison.Ordinal);
    //            foreach(String ss in valist)
    //                Console.WriteLine(String.Format("{0}={1}", s, ss));
    //        }
    //
    //        return;
    /// </code>
    /// </example>
    public class MyIniFileBase
    {
        /// <summary>
        /// Путь к текущему файлу настроек
        /// </summary>
        protected String m_iniFilePathname;
        /// <summary>
        /// Список строк файла настроек
        /// </summary>
        protected List<MyIniFileRow> m_lines;
        /// <summary>
        /// Кодировка файла настроек
        /// </summary>
        private Encoding m_Encoding;


        public MyIniFileBase()
        {
            m_iniFilePathname = String.Empty;
            m_lines = new List<MyIniFileRow>();
            m_Encoding = Encoding.Unicode;//default encoding for new files
        }

        /// <summary>
        /// Путь к текущему файлу настроек
        /// </summary>
        public String IniFilePathname
        {
            get { return m_iniFilePathname; }
            set { m_iniFilePathname = value; }
        }

        /// <summary>
        /// Кодировка файла настроек
        /// </summary>
        public Encoding Encoding
        {
            get { return m_Encoding; }
            set { m_Encoding = value; }
        }

        //Функции создания файла настроек из кода
        //- обычно файл настроек создается в текстовом редакторе
        //но с помощью этих функций его можно создать из кода

        /// <summary>
        /// NT-Добавить строку параметра в конец списка строк
        /// </summary>
        /// <param name="paramName">Имя параметра</param>
        /// <param name="value">Значение параметра</param>
        /// <remarks>
        /// Функция добавляет строку параметра в конец списка строк.
        /// Если параметр не имеет значения, передать в value пустую строку.
        /// </remarks>
        public void AddParameter(String paramName, String value)
        {
            if (String.IsNullOrEmpty(paramName))
                throw new ArgumentException("Parameter name must be specified", "paramName");
            if (value == null)
                throw new ArgumentException("Parameter value cannot be null", "value");

            MyIniFileRow row = new MyIniFileRow(paramName, value);
            m_lines.Add(row);

            return;
        }
        /// <summary>
        /// NT-Добавить строку комментария или пустую строку в конец списка строк
        /// </summary>
        /// <param name="value">Значение параметра</param>
        public void AddComment(String value)
        {
            if (value == null)
                throw new ArgumentException("Parameter value cannot be null", "value");

            MyIniFileRow row = new MyIniFileRow(value);
            m_lines.Add(row);

            return;
        }

        /// <summary>
        /// NT-Добавить в файл правила его формата.
        /// Это важная часть файла настроек, рекомендуется всегда выводить ее в создаваемый заново файл в начале, после информации о приложении.
        /// Важно: Кодировка файла уже должна быть установлена!
        /// </summary>
        public void AddRules()
        {
            AddComment(String.Empty);
            AddComment("Формат файла настроек:");
            AddComment("Каждая запись занимает только одну строку.");
            AddComment("символы // или # обозначают комментарий, и они должны располагаться в самом начале строки.");
            AddComment("Нельзя вставлять комментарии в середине строки.");
            AddComment(String.Format("Обязательно убедитесь, что файл настроек в кодировке {0}!", m_Encoding.WebName));
            AddComment(String.Empty);
        }
        /// <summary>
        /// NT-Очистить список строк
        /// </summary>
        public void Clear()
        {
            this.m_lines.Clear();
        }

        /// <summary>
        /// NT-Сохранить данные в ранее указанном файле и указанной кодировке
        /// Если указанный файл существует, он будет перезаписан.
        /// Если указанный файл не существуе
        /// </summary>
        public void Save()
        {
            StreamWriter sw = new StreamWriter(m_iniFilePathname, false, m_Encoding);

            foreach (MyIniFileRow row in m_lines)
                sw.WriteLine(row.ToString());

            sw.Close();

            return;
        }

        /// <summary>
        /// NT-Прочитать файл и вернуть true если файл прочитан успешно
        /// Исключения не выбрасываются.
        /// Список строк не очищается и содержит ранее добавленные строки.
        /// </summary>
        /// <param name="filename">Полный путь к файлу настроек</param>
        /// <param name="enc">Кодировка файла настроек</param>
        /// <returns>
        /// Если функция вернула false, значит произошло исключение.
        /// Ошибка: Файл настроек недоступен или отсутствует или имеет неправильный формат.
        /// </returns>
        public bool Load(String filename, Encoding enc)
        {
            bool result = true;
            StreamReader sr = null;
            bool fileOpened = false;
            try
            {
                //open settings file
                sr = new StreamReader(filename, enc);
                fileOpened = true;
                //читаем данные из файла настроек
                while (!sr.EndOfStream)
                {
                    String s = String.Empty;
                    //read line from file
                    String line = sr.ReadLine();
                    //if line not empty, parse it
                    line = line.Trim();
                    if (String.IsNullOrEmpty(line))
                    {
                        //это пустая строка
                        m_lines.Add(new MyIniFileRow(s));
                    }
                    else if (line[0] == '#')
                    {
                        if (line.Length > 1)
                            s = line.Substring(1);
                        //add as comment
                        m_lines.Add(new MyIniFileRow(s));
                    }
                    else if ((line[0] == '/'))
                    {
                        if (line.Length > 2)
                            s = line.Substring(2);
                        //add as comment
                        m_lines.Add(new MyIniFileRow(s));
                    }
                    else
                    {
                        //parameter, split by =
                        String[] sar = line.Split(new char[] { '=' }, StringSplitOptions.None);
                        if (sar.Length > 2)
                            throw new Exception("Неправильный формат файла настроек приложения");
                        else
                        {
                            String title = sar[0].Trim();
                            String value = sar[1].Trim();
                            //add as parameter
                            m_lines.Add(new MyIniFileRow(title, value));
                        }
                    }
                }
                sr.Close();
            }
            catch (Exception)
            {
                result = false;
                if (fileOpened)
                    sr.Close();
            }
            //сохранить значения для последующего использования 
            this.m_Encoding = enc;
            this.m_iniFilePathname = filename;

            return result;
        }

        /// <summary>
        /// NT-Получить одно значение параметра
        /// </summary>
        /// <param name="parameterName">Название параметра</param>
        /// <param name="comp">Способ сравнения названий</param>
        /// <returns>Возвращается одно значение параметра.
        /// Если параметров с таким значением несколько, возвращается первый из них.
        /// Если ничего не найдено, возвращается null.
        /// </returns>
        /// 
        public String getValue(String parameterName, StringComparison comp)
        {
            String result = null;
            foreach (MyIniFileRow row in m_lines)
            {
                if (row.m_Type == RowType.Parameter)
                    if (String.Equals(row.m_Name, parameterName, comp))
                    {
                        result = row.m_Value;
                        break;
                    }
            }

            return result;
        }
        /// <summary>
        /// NT-Получить все значения параметра
        /// </summary>
        /// <param name="parameterName">Название параметра</param>
        /// <param name="comp">Способ сравнения названий</param>
        /// <returns>
        /// Возвращается список значений параметра.
        /// Если нет такого параметра, возвращается пустой список.
        /// </returns>
        public List<String> getValues(String parameterName, StringComparison comp)
        {
            List<String> result = new List<string>();
            foreach (MyIniFileRow row in m_lines)
            {
                if (row.m_Type == RowType.Parameter)
                    if (String.Equals(row.m_Name, parameterName, comp))
                        result.Add(row.m_Value);
            }

            return result;
        }

        /// <summary>
        /// NT-Получить список названий всех параметров из файла, без самих значений.
        /// </summary>
        /// <returns></returns>
        public List<String> getParameterNames()
        {
            Dictionary<String, int> dic = new Dictionary<string, int>();
            foreach (MyIniFileRow row in m_lines)
            {
                if (row.m_Type == RowType.Parameter)
                    if (!dic.ContainsKey(row.m_Name))
                        dic.Add(row.m_Name, 0);
            }
            //return list of names
            List<String> result = new List<string>(dic.Keys);

            return result;
        }

        /// <summary>
        /// NT-Проверить что параметр с таким названием существует
        /// </summary>
        /// <param name="parameterName">Название параметра</param>
        /// <param name="comp">Способ сравнения названий</param>
        /// <returns></returns>
        public bool isParameterExists(String parameterName, StringComparison comp)
        {
            return (getValue(parameterName, comp) != null);
        }

    }
    /// <summary>
    /// Тип строки: комментарий или данные
    /// </summary>
    public enum RowType
    {
        Unknown = 0,
        Comment = 1,
        Parameter = 2,

    }
    /// <summary>
    /// Представляет строку файла настроек
    /// </summary>
    /// <remarks>
    /// Нужен только для того, чтобы искать требуемый параметр в общем списке строк
    /// </remarks>
    public class MyIniFileRow
    {
        /// <summary>
        /// Название параметра
        /// </summary>
        internal String m_Name;
        /// <summary>
        /// Значение параметра
        /// </summary>
        internal String m_Value;
        /// <summary>
        /// Тип строки файла: комментарий или данные
        /// </summary>
        internal RowType m_Type;
        /// <summary>
        /// Default constructor
        /// </summary>
        public MyIniFileRow()
        {
            m_Name = String.Empty;
            m_Value = String.Empty;
            m_Type = RowType.Unknown;
        }
        /// <summary>
        /// Конструктор для параметра
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public MyIniFileRow(String Name, String Value)
        {
            m_Name = Name.Trim();
            m_Value = Value.Trim();
            m_Type = RowType.Parameter;
        }
        /// <summary>
        /// Конструктор для комментария или пустой строки
        /// </summary>
        /// <param name="Value"></param>
        public MyIniFileRow(String Value)
        {
            m_Name = String.Empty;
            m_Value = Value.Trim();
            m_Type = RowType.Comment;
        }

        public override string ToString()
        {
            if (m_Type == RowType.Parameter)
                return m_Name + "=" + m_Value;
            else
            {
                if (m_Value != String.Empty)
                    return "//" + m_Value;
                else
                    return String.Empty;
            }
        }

    }
}
