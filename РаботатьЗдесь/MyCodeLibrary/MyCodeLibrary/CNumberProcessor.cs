using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace MyCodeLibrary
{
    public class CNumberProcessor
    {
        /// <summary>
        /// Converts bytes to Kilobytes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double BytesToKB(double input) { checked { return input / 1024; } }

        /// <summary>
        /// Converts bytes to Megabytes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double BytesToMB(double input) { checked { return (input / (1024*1024)); } }

        /// <summary>
        /// Converts bytes to Gigabytes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double BytesToGB(double input) { checked { return (input / (1024 * 1024*1024)); } }

        /// <summary>
        /// Converts bytes to Terabytes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double BytesToTB(double input) { checked { return (BytesToGB(input) / 1024); } }

        /// <summary>
        /// Returns the conversion from bytes to the correct version. Ex. 1024 bytes = 1 KB
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String ConvertBytes(double input)
        {
            if (input >= 1024)
            {
                input = input / 1024;
                if (input >= 1024)
                {
                    input = input / 1024;
                    if (input >= 1024)
                    {
                        input = input / 1024;
                        if (input >= 1024)
                        {
                            input = input / 1024;
                            return input.ToString("#.##", CultureInfo.CurrentCulture) + " TB";
                        }
                        return input.ToString("#.##", CultureInfo.CurrentCulture) + " GB";
                    }
                    return input.ToString("#.##", CultureInfo.CurrentCulture) + " MB";
                }
                return input.ToString("#.##", CultureInfo.CurrentCulture) + " KB";
            }
            return input.ToString("#.##", CultureInfo.CurrentCulture) + " Bytes";
        }

        /// <summary>
        /// Returns the conversion from KB to the correct version. Ex. 1024 KB = 1 MB
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String ConvertKilobytes(double input)
        {
            if (input >= 1024)
            {
                input = input / 1024;
                if (input >= 1024)
                {
                    input = input / 1024;
                    if (input >= 1024)
                    {
                        input = input / 1024;
                        return input.ToString("#.##", CultureInfo.CurrentCulture) + " TB";
                    }
                    return input.ToString("#.##", CultureInfo.CurrentCulture) + " GB";
                }
                return input.ToString("#.##", CultureInfo.CurrentCulture) + " MB";
            }
            return input.ToString("#.##", CultureInfo.CurrentCulture) + " KB";
        }

        /// <summary>
        /// Converts any type to an int
        /// </summary>
        /// <typeparam name="T">Any object</typeparam>
        /// <param name="input">Value to convert</param>
        /// <returns>The integer, 0 if unsuccessful</returns>
        public static int ToInt<T>(T input)
        {
            int result;
            if (int.TryParse(input.ToString(), out result))
            {
                return result;
            }
            return 0;
        }
        /// <summary>
        /// Converts any type to an int. Returns defaultValue if fails
        /// </summary>
        /// <param name="input">Value to convert</param>
        /// <typeparam name="T">Any object</typeparam>
        /// <param name="defaultValue">Default to use</param>
        /// <returns>The defaultValue if unsuccessful</returns>
        public static int ToInt<T>(T input, int defaultValue)
        {
            int result;
            if (int.TryParse(input.ToString(), out result))
            {
                return result;
            }
            return defaultValue;
        }
        /// <summary>
        /// Checks to see if a string is an int
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsInt(String input)
        {
            int temp;

            return int.TryParse(input, out temp);
        }
        /// <summary>
        /// Converts a string to an int. Returns defaultValue if fails
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(String input, int defaultValue)
        {
            int temp;

            return (int.TryParse(input, out temp)) ? temp : defaultValue;
        }
        /// <summary>
        /// Checks to see if a string is a Decimal
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsDecimal(String input)
        {
            Decimal temp;

            return Decimal.TryParse(input, out temp);
        }
        /// <summary>
        /// Converts a string to a double. Returns defaultValue if fails
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Decimal ToDecimal(String input, Decimal defaultValue)
        {
            Decimal temp;

            return (Decimal.TryParse(input, out temp)) ? temp : defaultValue;
        }
        /// <summary>
        /// Converts a string to a double
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double ToDouble(String input)
        {
            double retNum;
            var result = double.TryParse(input, out retNum);
            return result ? retNum : 0;
        }
        /// <summary>
        /// Checks if a number is prime
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsPrime(int input)
        {
            if ((input % 2) == 0) return input == 2;
            var sqrt = (int)Math.Sqrt(input);
            for (int t = 3; t <= sqrt; t = t + 2)
            {
                if (input % t == 0) return false;
            }
            return input != 1;
        }
        /// <summary>
        /// Checks if a number is even
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsEven(int input) { return (input % 2 == 0); }

        /// <summary>
        /// Checks if a number is odd
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Boolean IsOdd(int input) { return ( input % 2 != 0); }

        /// <summary>
        /// Returns a number squared
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int Squared(int input) { return (input * input); }

        /// <summary>
        /// Checks if a number is in between a range
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Boolean IsInRange(int input, int start, int end) { return ((input >= start) && (input <= end)); }

        /// <summary>
        /// Rounds the supplied decimal to the specified amount of decimal points
        /// </summary>
        /// <param name="input">The decimal to round</param>
        /// <param name="decimalPoints">The number of decimal points to round the output value to</param>
        /// <returns>A rounded decimal</returns>
        public static Decimal RoundDecimalPoints(Decimal input, int decimalPoints) { return Math.Round(input, decimalPoints); }

        /// <summary>
        /// Rounds the supplied decimal value to two decimal points
        /// </summary>
        /// <param name="input">The decimal to round</param>
        /// <returns>A decimal value rounded to two decimal points</returns>
        public static Decimal RoundToTwoDecimalPoints(Decimal input) { return Math.Round(input, 2); }
    }
}
