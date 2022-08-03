using System;
using System.Collections.Generic;
using System.Text;

namespace MyCodeLibrary.Collections
{
    public class CollectionsUtility
    {
        /// <summary>
        /// RT-разделить список на части по N элементов или менее
        /// </summary>
        /// <remarks>
        /// Применялся для построения запроса SQL, чтобы запрашивать из большой СУБД сразу 5..10 записей за раз.
        /// Для БД на миллион записей - большая экономия времени.
        /// </remarks>
        /// <param name="idList">Исходный список</param>
        /// <param name="n">Размер каждой из частей, больше 0</param>
        /// <returns>Возвращает список списков, каждый из которых содержит части входного списка.</returns>
        public static List<List<int>> SplitListInt32(List<int> idList, int n)
        {
            //проверка аргументов
            if (n <= 0)
                throw new ArgumentException("Argument N must be greather than 0!", "n");

            List<List<Int32>> result = new List<List<int>>();
            int cnt = idList.Count;
            if (cnt == 0) return result;
            //если там меньше N, то весь список возвращаем как единственный элемент 
            if (cnt <= n)
            {
                result.Add(idList);
                return result;
            }
            //иначе
            int c = cnt / n; //полных кусков по n элементов
            int cs = cnt % n; //остаточная длина куска
            //целые куски добавим
            for (int i = 0; i < c; i++)
                result.Add(idList.GetRange(i * n, n));
            //остаток
            if (cs > 0)
                result.Add(idList.GetRange(c * n, cs));

            return result;
        }

        /// <summary>
        /// Breaks a dictionary into a string value
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static String BreakDictionaryToString(Dictionary<String, String> dictionary)
        {
            var sb = new StringBuilder();
            const char KeySeparator = '=';
            const char PairSeparator = '&';
            foreach (var pair in dictionary)
            {
                sb.Append(pair.Key);
                sb.Append(KeySeparator);
                sb.Append(pair.Value);
                sb.Append(PairSeparator);
            }
            return sb.ToString(0, sb.Length - 1);
        }

    }
}
