using System;
using System.Collections.Generic;
using System.Text;

namespace MyCodeLibrary
{
    /// <summary>
    /// Класс для функций работы в консольном окне приложения
    /// </summary>
    public class ConsoleDialogs
    {
        /// <summary>
        /// NT-показать прогресс - сколько сделано из всего
        /// </summary>
        /// <param name="all">Сколько всего</param>
        /// <param name="current">Сколько сделано</param>
        /// <param name="step">Размер шага, обычно = 16</param>
        public static void ShowProgressToUser(int all, int current, int step)
        {
            if ((current % step) == 0)
            {
                float ps = ((float)current / (float)all) * 100.0f;
                Console.CursorLeft = 0;
                Console.Write("Обработано: {0:F2}%", ps);
            }
            return;
        }

    }
}
