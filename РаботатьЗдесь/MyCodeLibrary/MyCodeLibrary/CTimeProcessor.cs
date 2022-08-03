using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace MyCodeLibrary
{
    public class CTimeProcessor
    {
        static readonly DateTime EPOCH = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0, 0), DateTimeKind.Utc);

        /// <summary>
        /// Converts Unix timestamp to a DateTime
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimestamp(long input)
        {
            return EPOCH.AddSeconds(input);
        }
        /// <summary>
        /// Converts Unix timestamp to a miliseconds DateTime
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimestampUltra(long input) 
        { 
            return EPOCH.AddMilliseconds(input);
        }

        /// <summary>
        /// Converts a DateTime to a Unix timestamp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(DateTime input)
        {
            var diff = input.ToUniversalTime() - EPOCH;
            return (long)diff.TotalSeconds;
        }
        /// <summary>
        /// Converts a miliseconds DateTime to a Unix timestamp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long ToUnixTimestampUltra(DateTime input)
        {
            var diff = input.ToUniversalTime() - EPOCH;
            return (long)diff.TotalMilliseconds;
        }
        /// <summary>
        /// Checks if a string is DateTime.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsDate(String input)
        {
            if (!String.IsNullOrEmpty(input))
            {
                DateTime dt;
                return (DateTime.TryParse(input, out dt));
            }
            return false;
        }
        /// <summary>
        /// Checks if a date falls before a date
        /// </summary>
        /// <param name="input"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static Boolean IsBefore(DateTime input, DateTime from) 
        { 
            return input.Date > from.Date; 
        }

        /// <summary>
        /// Checks if a date falls before DateTime.Now
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsBeforeNow(DateTime input) 
        { 
            return  IsBefore(input, DateTime.Now); 
        }

        /// <summary>
        /// Checks if a date falls after a date
        /// </summary>
        /// <param name="input"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static Boolean IsAfter(DateTime input, DateTime from) 
        { 
            return  (input.Date < from.Date); 
        }

        /// <summary>
        /// Checks if a date falls after DateTime.Now
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsAfterNow(DateTime input) 
        { 
            return IsAfter(input, DateTime.Now); 
        }

        /// <summary>
        /// 	Checks if a date is today.
        /// </summary>
        /// <param name = "input"></param>
        /// <returns>
        /// 	<c>true</c> if the specified date is today; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsToday(DateTime input) 
        { 
            return  (input.Date == DateTime.Today);
        }

        /// <summary>
        /// 	Checks if the time only part of two DateTime values are equal.
        /// </summary>
        /// <param name = "input"></param>
        /// <param name = "timeToCompare"></param>
        /// <returns>
        /// 	<c>true</c> if both time values are equal; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsTimeEqual(DateTime input, DateTime timeToCompare)
        {
            return (input.TimeOfDay == timeToCompare.TimeOfDay);
        }

        /// <summary>
        /// NR-Returns current timestamp
        /// </summary>
        public static String CurrentShortTimeStamp
        {
            get { return DateTime.Now.ToString("MM.dd.yy HH:mm:ss tt", CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// NR-Returns current timestamp extended
        /// </summary>
        public static String CurrentFullTimeStamp
        {
            get { return DateTime.Now.ToString("ddd MMMM dd, yyyy hh:mm:ss tt", CultureInfo.CurrentCulture); }
        }


    }
}
