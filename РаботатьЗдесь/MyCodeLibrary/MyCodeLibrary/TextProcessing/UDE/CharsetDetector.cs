using System;
using System.IO;

using MyCodeLibrary.TextProcessing.UDE.Core;
using System.Text;

namespace MyCodeLibrary.TextProcessing.UDE
{
    /// <summary>
    /// Default implementation of charset detection interface. 
    /// The detector can be fed by a System.IO.Stream:
    /// <example>
    /// <code>
    /// using (FileStream fs = File.OpenRead(filename)) {
    ///    CharsetDetector cdet = new CharsetDetector();
    ///    cdet.Feed(fs);
    ///    cdet.DataEnd();
    ///    Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
    /// </code>
    /// </example>
    /// 
    ///  or by a byte a array:
    /// 
    /// <example>
    /// <code>
    /// byte[] buff = new byte[1024];
    /// int read;
    /// while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
    ///     Feed(buff, 0, read);
    /// cdet.DataEnd();
    /// Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
    /// </code>
    /// </example>
    /// 
    /// detect file encoding from static function:
    /// 
    /// <example>
    /// <code>
    /// Encoding e1 = CharsetDetector.DetectFileEncoding("C:\\Temp\\pitest.xml");
    /// Encoding e2 = CharsetDetector.DetectFileEncoding("C:\\Temp\\test.txt");
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Это вероятностный детектор, и на текстовых файлах он примерно в 10% случаев неправильно определяет кодировку либо вовсе не определяет ее.
    /// </remarks>
    public class CharsetDetector : UniversalDetector, ICharsetDetector
    {
        /// <summary>
        /// Текстовое название кодировки
        /// </summary>
        private string charset;
        /// <summary>
        /// Уверенность определения как число в пределах 0..1
        /// </summary>
        private float confidence;
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public CharsetDetector() : base()
        {
        }

        /// <summary>
        /// Текстовое название кодировки
        /// </summary>
        public string Charset
        {
            get { return charset; }
        }
        /// <summary>
        /// Уверенность определения как число в пределах 0..1
        /// </summary>
        public float Confidence
        {
            get { return confidence; }
        }

        /// <summary>
        /// Отправить в детектор поток текста для обработки
        /// </summary>
        /// <param name="stream">Поток, содержащий исходный текст</param>
        public void Feed(Stream stream)
        { 
            byte[] buff = new byte[1024];
            int read;
            while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
            {
                Feed(buff, 0, read);
            }
        }
        /// <summary>
        /// Узнать, закончено ли определение кодировки
        /// </summary>
        /// <returns></returns>
        public bool IsDone() 
        {
            return done;
        }
        /// <summary>
        /// Сбросить детектор в исходное состояние
        /// </summary>
        public override void Reset()
        {
            this.charset = null;
            this.confidence = 0.0f;
            base.Reset();
        }

        /// <summary>
        /// Переопределите эту функцию для получения результата детектирования
        /// </summary>
        /// <param name="charset">Название кодировки как член класса Charsets</param>
        /// <param name="confidence">Уверенность детектора (0..1)</param>
        protected override void Report(string charset, float confidence)
        {
            this.charset = charset;
            this.confidence = confidence;
        }

        /// <summary>
        /// NT-Определить кодировку текстового файла
        /// </summary>
        /// <param name="filepath">Путь текстового файла</param>
        /// <returns>Функция возвращает название кодировки, либо null если кодировку определить не удалось.</returns>
        public static string DetectFileEncodingString(String filepath)
        {
            String encod = String.Empty;
            
            using (FileStream fs = File.OpenRead(filepath))
            {
                CharsetDetector cdet = new CharsetDetector();
                //включить отладочный вывод проберов в консоль - по умолчанию он выключен.
                cdet.DumpToConsole = false;
                //загрузить текст и детектировать
                cdet.Feed(fs);
                cdet.DataEnd();
                encod = cdet.charset;
            }

            return encod;
        }

        /// <summary>
        /// NT-Определить кодировку текстового файла
        /// </summary>
        /// <param name="filepath">Путь текстового файла</param>
        /// <returns>Функция возвращает название кодировки, либо выбрасывает исключение ApplicationException, если кодировку определить не удалось.</returns>
        /// <exception cref="ApplicationException">Функция выбрасывает исключение, если если кодировку определить не удалось.</exception>
        public static Encoding DetectFileEncoding(String filepath)
        {
            String encod = String.Empty;
            
            using (FileStream fs = File.OpenRead(filepath))
            {
                CharsetDetector cdet = new CharsetDetector();
                //включить отладочный вывод проберов в консоль - по умолчанию он выключен.
                cdet.DumpToConsole = false;
                //загрузить текст и детектировать
                cdet.Feed(fs);
                cdet.DataEnd();
                encod = cdet.charset;
            }
            if (String.IsNullOrEmpty(encod))
                throw new ApplicationException("Error: Encoding detection failed!");
            //else
            Encoding enc = Encoding.GetEncoding(encod);
            
            return enc;
        }

    }//end class
}

