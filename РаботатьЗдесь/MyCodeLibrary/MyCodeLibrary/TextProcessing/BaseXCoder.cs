using System;
using System.Collections.Generic;
using System.Text;

namespace MyCodeLibrary.TextProcessing
{
    /// <summary>
    /// Класс для кодирования и декодирования чисел в шестнадцатеричной или другой системе
    /// </summary>
    public class BaseXCoder
    {

        /// <summary>
        /// Набор символов кодера по умолчанию
        /// </summary>
        public const string AsciiChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        /// <summary>
        /// Набор символов кодера по умолчанию
        /// </summary>
        public const string HexChars = "0123456789ABCDEF";
        
        private String m_Letters;        
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseXCoder()
        {
            m_Letters = "";
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="letters">Строка символов, представляющих число</param>
        public BaseXCoder(String letters)
        {
            m_Letters = String.Copy(letters);

        }


        /// <summary>
        /// Переводит число в строковое представление
        /// </summary>
        /// <param name="number">Конвертируемое число</param>
        /// <param name="length">Число требуемых символов результата, не считая знака</param>
        /// <returns>Возвращает строковое представление, возможно содержащее знак отрицательного числа</returns>
        public String Encode(Int32 number, Int32 length)
        {
            int count = m_Letters.Length;
            //если строка символов не задана, возвращаем пустую строку. А надо бы выбрасывать исключение.
            if (count == 0) return String.Empty;
            int num = number;
            //для преобразования нужно положительное число
            if (num < 0) num = 0 - num;
            int tmp;
            Char[] carr = new Char[length];
            //encode loop
            int i = length;
            while (i > 0)
            {
                i--;
                tmp = num % count;
                carr[i] = m_Letters[tmp];
                num = num / count;
            }
            String result = new String(carr);
            //add sign
            if (number < 0)
                return "-" + result;
            else return result;
        }

        /// <summary>
        /// Переводит строковое представление в число
        /// </summary>
        /// <param name="text">Строка, буквы должны быть в правильном регистре</param>
        /// <returns>Возвращает число</returns>
        /// <exception cref="ArgumentException">Входная строка имеет неправильный формат.</exception>
        public Int32 Decode(String text)
        {
            Int32 bbase = m_Letters.Length;
            Int32 tmp;
            Int32 result = 0;
            Char let;
            Boolean sign = false;

            for (int i = 0; i < text.Length; i++)
            {
                //читаем самый старший символ
                let = text[i];
                //если это знак, установим флаг
                if (let == '-')
                    sign = true;

                else //иначе проверим что символ есть в списке символов
                {
                    //получим индекс символа
                    tmp = m_Letters.IndexOf(let);
                    if (tmp < 0)    //если символ не найден в образцовой строке
                        throw new ArgumentException("Неправильный формат");
                    else
                    {
                        result *= bbase;//умножим на основание системы счисления
                        result += tmp; //добавим новое значение как порядковый индекс символа в массиве m_Letters
                    }
                }
            }
            //применим флаг знака
            if (sign == true)
                result = 0 - result;

            return result;
        }

        /// <summary>
        /// Переводит число в строковое представление из двух букв
        /// </summary>
        /// <param name="number">Положительное целое число от 0 до ХЗ</param>
        /// <returns>Возвращает строковое представление числа</returns>
        public String Encode2(Int32 number)
        {
            int count = m_Letters.Length;
            //если строка символов не задана, выбрасывать исключение.
            int num = number;
            int tmp;

            Char[] carr = new Char[2];
            int i = 2;
            while (i > 0)
            {
                i--;
                tmp = num % count;
                carr[i] = m_Letters[tmp];
                num = num / count;
            }
            String result = new String(carr);
            //return
            return result;
        } 



        /// <summary>
        /// NT-Проверка, что текст содержит только допустимые символы
        /// </summary>
        /// <param name="text">Строка текста BaseX кодировки</param>
        /// <param name="CodeChars">Строка символов BaseX кодировки</param>
        /// <returns>True если строка содержит только символы кодировки, False если содержит посторонние символы</returns>
        internal static bool isBaseXstring(string text, string CodeChars)
        {
            //если любой символ из строки текста не найден в строке шаблона, то возвращаем ошибку
            for (int i = 0; i < text.Length; i++)
                if (CodeChars.IndexOf(text[i]) == -1)
                    return false;

            return true;
        }

        ///// <summary>
        ///// Переводит число в строковое представление из трех букв
        ///// </summary>
        ///// <param name="number">Положительное целое число от 0 до ХЗ</param>
        ///// <returns>Возвращает строковое представление числа</returns>
        //public String Encode3(Int32 number)
        //{
        //    //TODO: удалить функцию когда она станет не нужна
        //    int count = m_Letters.Length;
        //    //если строка символов не задана, возвращаем пустую строку. А надо бы выбрасывать исключение.
        //    //if (count == 0) return String.Empty;
        //    int num = number;
        //    int tmp;
        //    StringBuilder sb = new StringBuilder();
        //    Char[] carr = new Char[3];
        //    int i = 3;
        //    while (i > 0)
        //    {
        //        i--;
        //        tmp = num % count;
        //        carr[i] = m_Letters[tmp];
        //        num = num / count;
        //    }
        //    String result = new String(carr);
        //    //return
        //    return result;
        //}
        ///// <summary>
        ///// Проверка алгоритмов
        ///// </summary>
        ///// <returns></returns>
        //public static void test()
        //{
        //    //создать объект
        //    BaseXCoder coder = new BaseXCoder(BaseXCoder.AsciiChars);
        //    Int32[] samples = { 0, 1, 22, 444, 16384, -7, -17890 };
        //    foreach (int i in samples)
        //    {
        //        String s = coder.Encode(i, 32);
        //        int t = coder.Decode(s);
        //        if (t != i) throw new Exception();
        //    }
        //    //тест короткой функции
        //    String[] samples2 = new string[] { "AAA", "AAB", "ABC", "ZZZ" };
        //    foreach (string s in samples2)
        //    {
        //        int ii = coder.Decode(s);
        //        string result = coder.Encode3(ii);
        //        if (!String.Equals(s, result))
        //            throw new Exception();
        //    }
        //    return;
        //}


    }
}
