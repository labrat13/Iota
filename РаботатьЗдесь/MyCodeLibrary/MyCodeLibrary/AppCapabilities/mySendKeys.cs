using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MyCodeLibrary
{
    
    /* Это недостаточно надежный способ имитации нажатия клавиш.
     * Он работает, но нуждается в усовершенствовании.
     * Создан 22 марта 2018 года
     */

    /// <summary>
    /// Симулирует нажатия клавиш для текущего активного окна приложения.
    /// </summary>
    public class mySendKeys
    {
        // Get a handle to an application window.
        [DllImport("USER32.DLL")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //21032018 - test CTRL+C
        [DllImport("USER32.DLL", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        
        //modifier keys
        private const byte VK_LSHIFT = 0xA0; // left shift key
        private const byte VK_CONTROL = 0x11;//control key
        private const byte VK_MENU = 0x12;//ALT key
        //service flags
        private const int KEYEVENTF_KEYUP = 0x02;
        private const int KEYEVENTF_EXTENDEDKEY = 0x01;


        /// <summary>
        /// Сделать окно активным для приема нажатий клавиш
        /// </summary>
        /// <param name="hWnd">HWND окна</param>
        public static void SetInputWindow(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
            return;
        }

        /// <summary>
        /// Найти существующее главное окно приложения по его имени
        /// </summary>
        /// <param name="windowClassName">Название класса окна</param>
        /// <param name="windowCaptionText">Текст заголовка окна. Если null, любой текст заголовка окна подходит.</param>
        /// <returns>Возвращает HWND описатель окна, если окно найдено. Возвращает null, если окно не найдено; следует вызвать GetLastError() для получения кода ошибки.</returns>
        public static IntPtr FindInputWindow(string windowClassName, string windowCaptionText)
        {
            return FindWindow(windowClassName, windowCaptionText);
        }

        /// <summary>
        /// Отправить простой текст в указанное окно.
        /// См. описание класса System.Windows.Forms.SendKeys в МСДН.
        /// Но модификаторы %^+ здесь не работают, хотя строка с ними парсится правильно.
        /// </summary>
        /// <param name="hWnd">HWND окна</param>
        /// <param name="text">Строка текста</param>
        public static void SendWait(IntPtr hWnd, string text)
        {
            SetForegroundWindow(hWnd);
            System.Windows.Forms.SendKeys.SendWait(text);
            return;
        }

        /// <summary>
        /// Отправить последовательность клавиш с модификаторами в текущее активное окно
        /// </summary>
        /// <param name="ks">Массив нажимаемых клавиш</param>
        /// <param name="Ctrl">При нажатой клавише Control</param>
        /// <param name="Shift">При нажатой клавише Shift</param>
        /// <param name="Alt">При нажатой клавише Alt</param>
        /// <remarks>
        /// Если в процессе отправки событий в окно происходят некие помехи,
        /// то могут возникать странные эффекты в ОС. Например, на таскбаре не разворачиваются окна при клике, итп.
        /// Это потому что нажатые программно клавиши не были отпущены. 
        /// Их можно вручную покликать, это должно исправить проблему.
        /// Но все же это недостаточно надежный механизм управления приложением.
        /// Он нуждается в совершенствовании.
        /// </remarks>
        public static void SendKeysWithModifiers(Keys[] ks, bool Ctrl, bool Shift, bool Alt)
        {
            UIntPtr extra = (UIntPtr)0;
            //modifier keys press
            if (Ctrl == true)
            {
                keybd_event(VK_CONTROL, 0, 0, extra);
            }
            if (Shift == true)
            {
                keybd_event(VK_LSHIFT, 0, 0, extra);
            }
            if (Alt == true)
            {
                keybd_event(VK_MENU, 0, 0, extra);
            }
            //text keys press and up
            foreach (Keys k in ks)
            {
                byte code = (byte)k;
                keybd_event(code, 0, 0, extra);
                keybd_event(code, 0, KEYEVENTF_KEYUP, extra);
            }

            //modifier keys up
            if (Ctrl == true)
            {
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, extra);
            }
            if (Shift == true)
            {
                keybd_event(VK_LSHIFT, 0, KEYEVENTF_KEYUP, extra);
            }
            if (Alt == true)
            {
                keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, extra);
            }
            return;
        }

        /// <summary>
        /// Получить массив клавиш из одной клавиши TAB
        /// </summary>
        /// <returns></returns>
        public static Keys[] GetKeysArrayTab()
        {
            return new Keys[] { Keys.Tab };//Tab
        }

        /// <summary>
        /// Получить массив клавиш из одной клавиши ENTER
        /// </summary>
        /// <returns></returns>
        public static Keys[] GetKeysArrayEnter()
        {
            return new Keys[] { Keys.Enter };
        }

        /// <summary>
        /// Получить массив клавиш из одной клавиши SPACE
        /// </summary>
        /// <returns></returns>
        public static Keys[] GetKeysArraySpace()
        {
            return new Keys[] { Keys.Space };
        }

    }
}

/* Пример использования класса

        /// <summary>
        /// Отправить события клавиш в запущенный Блокнот
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //IntPtr hwnd = mySendKeys.FindInputWindow("Notepad", "Безымянный - Блокнот");//Использует окно Блокнот с указанным названием.
            IntPtr hwnd = mySendKeys.FindInputWindow("Notepad", null);//использует первое попавшееся окно Блокнот

            if (hwnd == IntPtr.Zero)
            {
                MessageBox.Show("Notepad is not running.");
                return;
            }

            //mySendKeys.SetInputWindow(hwnd); - уже вызывается ниже
            mySendKeys.SendWait(hwnd, "111");
            mySendKeys.SendWait(hwnd, "*");
            mySendKeys.SendWait(hwnd, "11");
            mySendKeys.SendWait(hwnd, "=");
            //send CTRL+A, CTRL+C
            mySendKeys.SendKeysWithModifiers(new Keys[] { Keys.A, Keys.C }, true, false, false);

            return;
        }

*/

/* Предыдущие исследования

       private void TestЗаменитьВсе()
        {
            //тест сценария Заменить все в Блокноте
            //это работает на коротких текстах, а на длинных не проверялось. Хотя на одностраничном тесте Блокнот завис.
            //Надо бы вставлять вызовы SetForegroundWindow() между вызовами SendKeys*(),
            //чтобы неожиданно всплывшее окно не обламывало процесс и не забирало себе ввод.
            //Но тут лучше проверять и переустанавливать ForegroundWindow если оно неправильное.

            //Это работает и внушает оптимизм. Можно делать в приложении почти все, что может делать пользователь с клавиатуры.
            //Но нужен описатель окна или процесса с окном.

            Keys[] k1 = new Keys[] { Keys.H };//Ctrl+H
            Keys[] k2 = new Keys[] { Keys.D1, Keys.D1, Keys.D1 }; //111
            Keys[] k3 = new Keys[] { Keys.Tab };//Tab
            Keys[] k4 = new Keys[] { Keys.Enter };//Enter
            Keys[] k5 = new Keys[] { Keys.D, Keys.U, Keys.R, Keys.A, Keys.K };
            int delay = 100;
            SendKeys2_1(k1, true, false, false);//Ctrl+H - Диалог Заменить 
            Thread.Sleep(delay);
            SendKeys2_1(k2, false, false, false);//Образец - 111
            Thread.Sleep(delay);
            SendKeys2_1(k3, false, false, false);//Tab
            Thread.Sleep(delay);
            SendKeys2_1(k5, false, false, false);//Замена
            Thread.Sleep(delay);
            SendKeys2_1(k3, false, false, false);//Tab
            Thread.Sleep(delay);
            //тут опционально нажать Пробел - галочка С учетом регистра
            SendKeys2_1(k3, false, false, false);//Tab
            Thread.Sleep(delay);
            //тут опционально нажать Enter - кнопка Найти далее
            SendKeys2_1(k3, false, false, false);//Tab
            Thread.Sleep(delay);
            //тут опционально нажать Enter - кнопка Заменить
            SendKeys2_1(k3, false, false, false);//Tab
            Thread.Sleep(delay);
            //тут опционально нажать Enter - кнопка Заменить все
            SendKeys2_1(k4, false, false, false);//Enter
            Thread.Sleep(delay);

            SendKeys2_1(k3, false, false, false);//Tab
            Thread.Sleep(delay);
            //кнопка Закрыть окно
            SendKeys2_1(k4, false, false, false);//Enter
            Thread.Sleep(delay);

            //тут нажать Ctrl+End чтобы перейти  к концу многострочного текста 
            SendKeys2_1(new Keys[] { Keys.End }, true, false, false);
        }

        public const byte VK_A = 0x41;
        public const byte VK_C = 0x43;
        public const byte VK_TAB = 0x09;
        /// <summary>
        /// Это работает в винХР. Надо улучшить код до большей автоматичности.
        /// </summary>
        private void SENDKEYS2()
        {
            UIntPtr extra = (UIntPtr)0;
            
            //press the shift key
            keybd_event(VK_CONTROL, 0, 0, extra);

            //press the tab key
            keybd_event(VK_A, 0, 0, extra);
            keybd_event(VK_A, 0, KEYEVENTF_KEYUP, extra);

            keybd_event(VK_C, 0, 0, extra);
            keybd_event(VK_C, 0x45, KEYEVENTF_KEYUP, extra);

            //release the shift key
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, extra);

            return;
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            // Send the enter key to the button, which raises the click 
            // event for the button. This works because the tab stop of 
            // the button is 0.
            SendKeys.Send("{ENTER}");

        }
*/