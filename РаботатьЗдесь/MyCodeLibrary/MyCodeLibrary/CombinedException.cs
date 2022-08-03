using System;
using System.Collections.Generic;

namespace MyCodeLibrary
{
    /// <summary>
    /// Исключение, которое комбиирует нескоько вложенных исключений.
    /// Для случая пакетной обработки файлов, когда надо все файлы задания попытаться обработать, несмотря на ошибки.
    /// </summary>
    [Serializable]
    public class CombinedException : Exception
    {
        /// <summary>
        /// 	Initializes a new instance of the <see cref = "CombinedException" /> class.
        /// </summary>
        /// <param name = "message">The message.</param>
        /// <param name = "innerExceptions">The inner exceptions.</param>
        public CombinedException(String message, Exception[] innerExceptions) : base(message)
        {
            InnerExceptions = innerExceptions;
        }

        /// <summary>
        /// 	Gets the inner exceptions.
        /// </summary>
        /// <value>The inner exceptions.</value>
        public Exception[] InnerExceptions { get; protected set; }

        /// <summary>
        /// Combines the specified exceptions.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerExceptions"></param>
        /// <returns></returns>
        public static Exception Combine(string message, params Exception[] innerExceptions)
        {
            if (innerExceptions.Length == 1)
                return innerExceptions[0];

            return new CombinedException(message, innerExceptions);
        }

        ///// <summary>
        ///// Combines the specified exceptions.
        ///// </summary>
        ///// <param name="message">The message.</param>
        ///// <param name="innerExceptions">The inner exceptions.</param>
        ///// <returns></returns>
        //public static Exception Combine(string message, Exception[] innerExceptions)
        //{
        //    return Combine(message, innerExceptions);
        //}
        ///// <summary>
        ///// Combines the specified exceptions.
        ///// </summary>
        ///// <param name="message">The message.</param>
        ///// <param name="innerExceptions">The inner exceptions.</param>
        ///// <returns></returns>
        //public static Exception Combine(string message, IEnumerable<Exception> innerExceptions)
        //{
        //    return Combine(message, innerExceptions.ToArray());
        //}
    }
}
