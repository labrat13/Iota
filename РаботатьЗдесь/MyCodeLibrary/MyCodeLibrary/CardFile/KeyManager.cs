
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyCodeLibrary.CardFile
{
    /// <summary>
    /// TODO: Незаконченный класс менеджера паролей приложения
    /// </summary>
    public class KeyManager
    {
        
        /// <summary>
        /// Заполнить массив нулями
        /// </summary>
        /// <param name="data">Массив для обработки</param>
        public void ClearBlock(Byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = 0;
        }

        /// <summary>
        /// Кодировать или декодировать блок данных по ранее созданному ключу
        /// </summary>
        /// <param name="data">Блок данных для обработки</param>
        /// <returns></returns>
        private Byte[] EncodeDecode(Byte[] data, Byte[] key)
        {
            Byte[] result = new Byte[data.Length];
            //TODO: сейчас тут неправильно - если ключ короче чем данные, его надо несколько раз применять.
            int keyIndex = 0;
            int index = 0;
            UInt32 a, b;
            while(index < data.Length)
            {
                //ХОРим данные и ключ
                a = (UInt32) data[index];
                b = (UInt32) key[keyIndex];
                result[index] = (Byte)(a ^ b);
                //если длина ключа меньше чем длина блока данных, повторяем ключ с начала 
                keyIndex++;
                index++;
                if (keyIndex >= key.Length)
                    keyIndex = 0;
            }
            return result;
        }

        /// <summary>
        /// NR-Создать первичный ключ из паролей пользователя
        /// </summary>
        /// <param name="userPass">Короткий пароль пользователя</param>
        /// <param name="appPass">Длинный пароль приложения</param>
        /// <returns></returns>
        public static Byte[] CreateMasterKey(String userPass, String appPass)
        {
            //Длина ключа принята в 256 кб.
            //сделаем байтовый массив из паролей
            Byte[] userpas = KeyManager.HashText(userPass + appPass);
            //теперь надо смешать части массива так чтобы пользовательский пароль растворился в пароле приложения
            Int32 halfpas = userpas.Length / 2;
            for (int i = 0; i < halfpas; i++)
            {
                Byte b1 = userpas[i];
                userpas[i] = userpas[i + halfpas];
                userpas[i + halfpas] = b1;
            }
            //теперь надо сделать большой буфер пароля для всех запусков ГСЧ
            Byte[] hash = new Byte[64];
            //копируем пароль в hash и повторяем его если осталось свободное место. 
            //Это позволяет использовать короткие пароли.
            Int32 tmp = 0;
            for (int ii = 0; ii < hash.Length; ii++)
            {
                hash[ii] = userpas[tmp];
                tmp++;
                if (tmp >= userpas.Length) tmp = 0;
            };
            //зачистить и освободить ненужный буфер.
            Array.Clear(userpas, 0, userpas.Length);
            userpas = null;
            //TODO: тут вывести ключ в файл чтобы убедиться в его энтропии
            KeyManager.StoreFile("hash.bin", hash);

            //генерация случайной последовательности
            //для ключа в 256кб нужно 2048 сегментов по 128 байт
            //а не выйдет. Итак, 8 А индексов и 8 С индексов образуют 64 байта.
            //256 кб делим на 64 сегмента, получаем длину сегмента 4 кб
            Byte[] Adata = new Byte[] { 17, 61, 240, 127, 33, 94, 168, 199 };
            Byte[] Cdata = new Byte[] { 91, 116, 3, 45, 218, 254, 141, 48 };
            Byte[] ResultKey = new Byte[256 * 1024];
            //генератор случайных чисел по Кнуту
            //Размер одной страницы 4096 байт
            Int32 bigindex = 0; //счетчик страниц ключа
            Int32 a, c, x;
            for (int Aind = 0; Aind < Adata.Length; Aind++)
            { //4 начальных коэффициента
                for (int Cind = 0; Cind < Cdata.Length; Cind++)
                {//8 вторых коэффициентов, итого 32 комбинации под размер хеша
                    a = (Int32)Adata[Aind];
                    c = (Int32)Cdata[Cind];
                    x = (Int32)hash[(Aind * 8) + Cind]; //делается выборка из массива с хешем пароля
                    for (Int32 index = 0; index < 4096; index++)
                    {//4096 членов псевдослучайной последовательности по каждой выборке из хеша. Итого 4096*64=256кбайт.
                        x = ((a * x) + c) % 256; //собственно генератор последовательности и тоже не рассчитанный толком, тоже от фонаря
                        ResultKey[index + bigindex] = (Byte)x; //вписываем байт в очередной участок ключа
                    };
                    bigindex += 4096; //переходим на следующий 128байтный участок ключа
                };
            };
            //TODO: Теперь размешать ключ и провверить его энтропию.
            //До размешивания энтропия низкая - ключ сжимается до с 256кб до 5кб.
            //по результатам проверок, ключ содержит повторы, необходимо дополнительно размешать
            //тут вроде получше схема чем в прошлом приложении
            for (int idx = 0; idx < (ResultKey.Length - 6); idx++)
            {
                Int32 ii1 = ((Int32)ResultKey[idx / 3] + (int)ResultKey[idx / 2]) & 0xFF; //0..255
                Int32 ii2 = (int)ResultKey[idx + 5]; //0..255
                Int32 ii3 = (((int)ResultKey[idx+3] & 0x03) + 1); //1..4
                ResultKey[idx] = (Byte) ((int)ResultKey[idx] ^ (int)ResultKey[ii1 * ii2 * ii3]);
            }
            KeyManager.StoreFile("key.bin", ResultKey);
            //TODO: теперь зачистить все лишние буферы и вернуть ключ.
            Array.Clear(hash, 0, hash.Length);
            hash = null;
            Array.Clear(Adata, 0, Adata.Length);
            Adata = null;
            Array.Clear(Cdata, 0, Cdata.Length);
            Cdata = null;
            return ResultKey;
        }

        /// <summary>
        /// NT-Вывести массив байт в файл для оценки энтропии
        /// </summary>
        /// <param name="p"></param>
        /// <param name="bytes"></param>
        private static void StoreFile(string p, byte[] bytes)
        {
            BinaryWriter bw = new BinaryWriter(File.Create(p));
            bw.Write(bytes);
            bw.Close();
            return;
        }

        /// <summary>
        /// NR-Сгенерировать ключ файла по номеру файла.
        /// </summary>
        /// <param name="FileNumber">Номер файла</param>
        private Byte[] CreateFileKey(UInt32 FileNumber)
        {
            //для теста только инициализируем ключ файла инвертирующими значениями
            Byte[] res = new Byte[4096];
            for (int i = 0; i < res.Length; i++)
                res[i] = 0xff;
            //окно в 4 байта в файловом ключе. Это место заполнить нулями.
            //окно не нужно, так как вся шапка файла не шифруется - только секции.
            //res[10] = 0; res[11] = 0;
            //res[12] = 0; res[13] = 0;
            
            return res;
        }
        /// <summary>
        /// Зашифровать или дешифровать блок данных файла
        /// </summary>
        /// <param name="FileNumber">Номер из файла</param>
        /// <param name="data">Блок данных файла</param>
        /// <returns>Возвращает преобразованный блок данных</returns>
        public virtual Byte[] CodeFile(UInt32 FileNumber, Byte[] data)
        {
            //create file key from primary key
            Byte[] filekey = CreateFileKey(FileNumber);
            //encode or decode data
            Byte[] result = EncodeDecode(data, filekey);
            //clear file key
            this.ClearBlock(filekey);
            filekey = null;

            return result;
        }

        #region *** Поиск алгоритма преобразования пароля ***
        internal static byte[] HashText(string s)
        {
            //все получилось, но есть проблема- на яве может оказаться другая раскладка символов и формат тоже.
            //и тогда строка будет преобразовываться по-другому, и пароли не совпадут.
            Byte[] bar = new Byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                Int32 b = (int)s[i];
                b = b + b * 8 + (b-20);
                bar[i] = (Byte)b;
            }
            return bar;
        }

        internal static int CalcMinByte(byte[] bar)
        {
            int min = Int32.MaxValue;
            foreach (Byte b in bar)
                if (((int)b) < min) min = ((int)b);
            return min;
        }

        internal static int CalcMaxByte(byte[] bar)
        {
            int max = Int32.MinValue;
            foreach (Byte b in bar)
                if (((int)b) > max) max = ((int)b);
            return max;
        }

        internal static int CalcMinDelta(byte[] bar)
        {
            //sort
            Array.Sort<Byte>(bar);
            //list for all delta
            int min = Int32.MaxValue;
            for (int i = 0; i < bar.Length-1; i++)
            {
                int delta = (int)(bar[i+1]) - (int)(bar[i]);
                if(delta < min) min = delta;
            }

            return min;
        }

        internal static int CalcMaxDelta(byte[] bar)
        {
            //sort
            Array.Sort<Byte>(bar);
            //list for all delta
            int max = Int32.MinValue;
            for (int i = 0; i < bar.Length - 1; i++)
            {
                int delta = (int)(bar[i+1]) - (int)(bar[i ]);
                if (delta > max) max = delta;
            }

            return max;
        }
        #endregion
    }
}
