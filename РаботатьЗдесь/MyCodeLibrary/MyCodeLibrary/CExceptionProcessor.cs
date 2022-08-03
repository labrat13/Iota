using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MyCodeLibrary
{
    public class CExceptionProcessor
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/>
        /// if the the value is null
        /// </summary>
        /// <param name="input">The value to test.</param>
        /// <param name="message">The message to display if the value is null.</param>
        /// <param name="name">The name of the parameter being tested.</param>
        public static object ExceptionIfNull(object input, String message, String name)
        {
            if (input == null)
                throw new ArgumentNullException(name, message);
            return input;
        }
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/>
        /// if the the Collection is null or empty
        /// </summary>
        /// <param name="input">The value to test.</param>
        /// <param name="message">The message to display if the value is null.</param>
        /// <param name="name">The name of the parameter being tested.</param>
        public static ICollection ExceptionIfNullOrEmpty(ICollection input, String message, String name)
        {
            if (input == null)
                throw new ArgumentNullException(name, message);
            if (input.Count == 0)
                throw new ArgumentNullException(name, message);
            return input;
        }
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/>
        /// if the the Dictionary is null or empty
        /// </summary>
        /// <param name="input">The value to test.</param>
        /// <param name="message">The message to display if the value is null.</param>
        /// <param name="name">The name of the parameter being tested.</param>
        public static ICollection ExceptionIfNullOrEmpty(IDictionary input, String message, String name)
        {
            if (input == null)
                throw new ArgumentNullException(name, message);
            if (input.Count == 0)
                throw new ArgumentNullException(name, message);
            return input;
        }
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/>
        /// if the the List is null or empty
        /// </summary>
        /// <param name="input">The value to test.</param>
        /// <param name="message">The message to display if the value is null.</param>
        /// <param name="name">The name of the parameter being tested.</param>
        public static ICollection ExceptionIfNullOrEmpty(IList input, String message, String name)
        {
            if (input == null)
                throw new ArgumentNullException(name, message);
            if (input.Count == 0)
                throw new ArgumentNullException(name, message);
            return input;
        }
        ///// <summary>
        ///// Throws an <see cref="ArgumentNullException"/>
        ///// if the the ReadOnlyCollection is null or empty
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="input"></param>
        ///// <param name="message"></param>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static IReadOnlyCollection<T> ExceptionIfNullOrEmpty<T>(IReadOnlyCollection<T> input, String message, String name)
        //{
        //    if (input == null)
        //        throw new ArgumentNullException(name, message);
        //    if (input.Count == 0)
        //        throw new ArgumentNullException(name, message);
        //    return input;
        //}
        ///// <summary>
        ///// Throws an <see cref="ArgumentNullException"/>
        ///// if the the ReadOnlyDictionary is null or empty
        ///// </summary>
        ///// <typeparam name="T1"></typeparam>
        ///// <typeparam name="T2"></typeparam>
        ///// <param name="input"></param>
        ///// <param name="message"></param>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static IReadOnlyDictionary<T1, T2> ExceptionIfNullOrEmpty<T1, T2>(this IReadOnlyDictionary<T1, T2> input, String message, String name)
        //{
        //    if (input == null)
        //        throw new ArgumentNullException(name, message);
        //    if (input.Count == 0)
        //        throw new ArgumentNullException(name, message);
        //    return input;
        //}
        ///// <summary>
        ///// Throws an <see cref="ArgumentNullException"/>
        ///// if the the ReadOnlyList is null or empty
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="input"></param>
        ///// <param name="message"></param>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static IReadOnlyList<T> ExceptionIfNullOrEmpty<T>(this IReadOnlyList<T> input, String message, String name)
        //{
        //    if (input == null)
        //        throw new ArgumentNullException(name, message);
        //    if (input.Count == 0)
        //        throw new ArgumentNullException(name, message);
        //    return input;
        //}
        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the string value is null or empty.
        /// </summary>
        /// <param name="input">The value to test.</param>
        /// <param name="message">The message to display if the value is null.</param>
        /// <param name="name">The name of the parameter being tested.</param>
        public static String ExceptionIfNullOrEmpty(String input, String message, String name)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException(message, name);
            return input;
        }

    }
}
