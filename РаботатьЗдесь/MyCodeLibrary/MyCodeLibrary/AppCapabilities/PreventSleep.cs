using System;
using System.Runtime.InteropServices;

namespace MyCodeLibrary
{
        
    /// <summary>
    /// Позволяет предотвратить самопроизвольное засыпание компьютера в процессе работы приложения.
    /// </summary>
    /// <remarks>
    /// Проверен, работает на ВиндовсХР
    /// Client: Requires Windows Vista, Windows XP, or Windows 2000 Professional.
    /// Server: Requires Windows Server 2008, Windows Server 2003, or Windows 2000 Server.
    /// Если другое приложение также запретило усыпление, отсюда это отменить нельзя.
    /// </remarks>
    /// <example>
    /// public partial class Form1 : Form
    ///{
    ///    public Form1()
    ///    {
    ///        InitializeComponent();
    ///    }

    ///    private void checkBox_Screen_CheckedChanged(object sender, EventArgs e)
    ///    {
    ///        if (this.checkBox_Screen.Checked == true)
    ///        {
    ///            PreventSleep.PreventDisplayOff();
    ///        }
    ///        else
    ///        {
    ///            PreventSleep.ClearPrevents();
    ///        }
    ///    }

    ///    private void checkBox_Proc_CheckedChanged(object sender, EventArgs e)
    ///    {
    ///        if (this.checkBox_Proc.Checked == true)
    ///        {
    ///            //PreventSleep.PreventSystemOffWinVista();
    ///            PreventSleep.PreventSystemOffWinXP();
    ///        }
    ///        else
    ///        {
    ///            PreventSleep.ClearPrevents();
    ///        }
    ///    }

    ///    private void timer1_Tick(object sender, EventArgs e)
    ///    {
    ///        PreventSleep.KeepSystemAwake();
    ///    }

    ///    private void checkBox_Timer_CheckedChanged(object sender, EventArgs e)
    ///    {
    ///        if (this.checkBox_Timer.Checked == true)
    ///        {
    ///            timer1.Enabled = true;
    ///        }
    ///        else
    ///        {
    ///            timer1.Enabled = false;
    ///        }
    ///    }
    ///}
    /// </example>
    public static class PreventSleep
    {
        //Автоматически регистрируемая активность - это клавиатура, мышь, действия сервера, изменения фокуса окон.
        //Не детектируются автоматически: работа дисков, процессора и видеодисплея

        //Вызов SetThreadExecutionState без флага ES_CONTINUOUS просто сбрасывает соответствующий таймаут.
        // Чтобы сохранить дисплей или систему в рабочем состоянии, поток должен вызывать SetThreadExecutionState периодически.

        //Чтобы правильно использовать: Приложения такие как факс-серверы, бекап-менеджеры и сетевые приложения должны использовать ES_SYSTEM_REQUIRED | ES_CONTINUOUS при обработке своих событий.
        //Мультимедийные приложения, как видеоплееры, должны использовать ES_DISPLAY_REQUIRED когда они показывают длинные видеоролики без пользовательского ввода.
        //Приложения с активным пользовательским вводом - текстовые редакторы, игры, браузеры - не должны использовать SetThreadExecutionState вовсе.

        //Функция SetThreadExecutionState не может быть применена для предотвращения перевода пользователем компьютера в standby mode.
        //Так как приложение должно соответствовать правилам, по которым пользователь ожидает соответственного поведения системы, когда пользователь закрывает крышку ноутбука или нажимает кнопку выключения питания.

        //Эта функция не предотвращает исполнение скринсейвера. 

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        /// <summary>
        /// NT-Предотвратить выключение дисплея по таймеру. И системы соответственно.
        /// </summary>
        /// <returns>Возвращает предыдущее состояние свойства.</returns>
        /// <remarks></remarks>
        public static EXECUTION_STATE PreventDisplayOff()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }
        /// <summary>
        /// NT-Предотвратить засыпание компьютера при бездействии. 
        /// Монитор гасится по таймеру. Версия для Windows2000 и XP.
        /// </summary>
        /// <returns>Возвращает предыдущее состояние свойства.</returns>
        public static EXECUTION_STATE PreventSystemOffWinXP()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }
        /// <summary>
        /// NT-Предотвратить засыпание компьютера при бездействии. 
        /// Монитор гасится по таймеру. Версия для Windows Vista, не работает в WindowsXP.
        /// </summary>
        /// <returns>Возвращает предыдущее состояние свойства.</returns>
        public static EXECUTION_STATE PreventSystemOffWinVista()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_AWAYMODE_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }
        /// <summary>
        /// NT-Отменить запрещения гасить монитор и усыплять систему.
        /// </summary>
        /// <returns>Возвращает предыдущее состояние свойства.</returns>
        public static EXECUTION_STATE ClearPrevents()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }
        /// <summary>
        /// NT-Периодически сбрасывать таймер усыпления системы
        /// </summary>
        /// <returns>Возвращает предыдущее состояние свойства.</returns>
        public static EXECUTION_STATE KeepSystemAwake()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
        }
        /// <summary>
        /// NT-Периодически сбрасывать таймер выключения монитора
        /// </summary>
        /// <returns>Возвращает предыдущее состояние свойства.</returns>
        public static EXECUTION_STATE KeepDisplayAwake()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        }


        //Включил енум в тело класса, чтобы он не торчал наружу, не захламлял подсказки
        /// <summary>
        /// Флаги состояния
        /// </summary>
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            /// <summary>
            /// Флаг состояния AWAYMODE - поддерживается начиная с Windows Vista
            /// </summary>
            /// <remarks>
            /// Although Away Mode is supported on any Windows Vista PC, the mode must be explicitly allowed by the current power policy. The Allow Away Mode power setting enables the user to selectively allow Away Mode on one or more power plans or individually for AC and DC (on battery) power states.
            /// (see more about this at http://msdn.microsoft.com/en-us/windows/hardware/gg463208.aspx )
            /// Description of what the different EXECUTION_STATE does - http://msdn.microsoft.com/en-us/library/aa373208(v=vs.85).aspx
            /// </remarks>
            ES_AWAYMODE_REQUIRED = 0x00000040,
            /// <summary>
            /// Informs the system that the state being set should remain in effect until the next call that uses ES_CONTINUOUS and one of the other state flags is cleared.
            /// </summary>
            ES_CONTINUOUS = 0x80000000,
            /// <summary>
            /// Сохранить Дисплей включенным
            /// </summary>
            ES_DISPLAY_REQUIRED = 0x00000002,
            /// <summary>
            /// Сохранить систему включенной
            /// </summary>
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }
    }


}
