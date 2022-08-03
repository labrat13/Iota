using System;
using System.Windows.Forms;

namespace MyCodeLibrary
{
    /// <summary>
    /// Функции управления раскладками клавиатуры
    /// </summary>
    /// <remarks>
    /// Для консольных приложений это не работает. Там надо переключать через комбинации клавиш.
    /// Переключается раскладка для текущего потока, а не для всего процесса.
    /// В GUI приложении обычно один (текущий) поток обслуживает ввод. Поэтому это там работает.
    /// В консольном приложении ввод обслуживается одним потоком, а приложение - другим. Поэтому там не работает.
    /// </remarks>
    public class KeyboardLayouts
    {
        /// <summary>
        /// RT-Получить раскладку клавиатуры  по имени раскладки
        /// </summary>
        /// <param name="langname">
        /// Англоязычное название раскладки клавиатуры.
        /// English или eng, Russian или rus - используется регистронезависимое сравнение начальных символов
        /// </param>
        /// <returns>Возвращает найденный объект раскладки клавиатуры или null если ничего не найдено</returns>
        public static InputLanguage GetInputLanguageByName(string langname)
        {
            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                if (lang.Culture.EnglishName.ToLower().StartsWith(langname.ToLower()))
                    return lang;
            }
            return null;
        }
        /// <summary>
        /// RT-Установить раскладку клавиатуры
        /// </summary>
        /// <param name="lang">Объект раскладки клавиатуры</param>
        public static void SetKeyboardLayout(InputLanguage lang)
        {
            InputLanguage.CurrentInputLanguage = lang;
        }

        /// <summary>
        /// RT-Вовзвращает TRUE если для приложения установлено более одной раскладки клавиатуры
        /// </summary>
        /// <returns></returns>
        public static bool HasTwoMoreLanguages()
        {
            return (InputLanguage.InstalledInputLanguages.Count > 1);
        }

        /// <summary>
        /// RT-Установить раскладку клавиатуры по имени раскладки
        /// </summary>
        /// <param name="langname">
        /// Англоязычное название раскладки клавиатуры.
        /// English или eng, Russian или rus - используется регистронезависимое сравнение начальных символов
        /// </param>
        public static void SetLayoutSimple(String langname)
        {
            SetKeyboardLayout(GetInputLanguageByName(langname));
            return;
        }

        /// <summary>
        /// NT-Получить массив объектов раскладки клавиатуры для выбора в комбобоксе или меню.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Тут проще было бы использовать InputLanguage.InstalledInputLanguages прямо,
        /// но у нас все как всегда. Поэтому передаем ее вызывающему коду без изменений.
        /// </remarks>
        public static InputLanguageCollection GetLayouts()
        {
            return InputLanguage.InstalledInputLanguages;
        }

    }
}
