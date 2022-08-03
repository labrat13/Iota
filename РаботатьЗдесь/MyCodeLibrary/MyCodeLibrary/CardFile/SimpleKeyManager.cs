using System;
using System.Collections.Generic;
using System.Text;

namespace MyCodeLibrary.CardFile
{
    /// <summary>
    /// простой шифоровальщик - ничего не шифрует
    /// </summary>
    public class SimpleKeyManager: KeyManager
    {

        /// <summary>
        /// Зашифровать или дешифровать блок данных файла
        /// </summary>
        /// <param name="FileNumber">Номер из файла</param>
        /// <param name="data">Блок данных файла</param>
        /// <returns>Возвращает преобразованный блок данных</returns>
        public override Byte[] CodeFile(UInt32 FileNumber, Byte[] data)
        {
            Byte[] result = (Byte[])data.Clone();

            return result;
        }

    }
}
