using System;
using System.Collections.Generic;
using System.Text;

namespace MyCodeLibrary.CardFile
{
    /// <summary>
    /// Секция файла карточки
    /// </summary>
    public class CardFileSection
    {
        /// <summary>
        /// Строка названия раздела карточки
        /// </summary>
        private string m_Title;
        /// <summary>
        /// Блок байт содержимого раздела карточки
        /// </summary>
        private Byte[] m_Data;
        /// <summary>
        /// Код тега заголовка секции
        /// </summary>
        public const Byte SectionStartTag = 0x55;
        /// <summary>
        /// Код тега конца файла секций
        /// </summary>
        public const Byte FileEndTag = 0xAA;
        /// <summary>
        /// Конструктор
        /// </summary>
        public CardFileSection()
        {
            this.m_Title = String.Empty;

        }

        /// <summary>
        /// NT-Конструктор с параметрами
        /// </summary>
        /// <param name="НазваниеРаздела"></param>
        /// <param name="БлокДанныхРаздела"></param>
        public CardFileSection(string title, byte[] data)
        {
            this.m_Title = title;
            this.m_Data = data;
        }

        /// <summary>
        /// Название секции файла карточки
        /// </summary>
        public String Title
        {
            get
            {
                return m_Title;
            }

            set
            {
                this.m_Title = value;
            }
        }

        /// <summary>
        /// Название секции файла карточки
        /// </summary>
        public Byte[] Data
        {
            get
            {
                return m_Data;
            }

            set
            {
                this.m_Data = value;
            }
        }


        /// <summary>
        /// NT-Получить строковое представление объекта
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //Формат: названиесекции [размерданных]
            return this.m_Title + " [" + this.m_Data.LongLength.ToString() + "]";
        }
    }
}
