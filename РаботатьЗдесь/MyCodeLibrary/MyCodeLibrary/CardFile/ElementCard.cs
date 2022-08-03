using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MyCodeLibrary.TextProcessing;
using MyCodeLibrary.Hash;

namespace MyCodeLibrary.CardFile
{


    /// <summary>
    /// Класс карточки абстрактного  Элемента Инвентарь
    /// </summary>
    public class ElementCard
    {

        /// <summary>
        /// Заголовок файла
        /// </summary>
        private const UInt64 CardFileSignature = 0x32564e494152454d;  //MERAINV2
        /// <summary>
        /// Код версии формата файла
        /// </summary>
        private const UInt16 CardFileFormatVersion = 0x1111; 
        
        /// <summary>
        /// Словарь разделов карточки
        /// </summary>
        private Dictionary<String, CardFileSection> m_ChapterDictionary;
        
        /// <summary>
        /// Путь к файлу карточки
        /// </summary>
        private string m_FilePath;

        /// <summary>
        /// Тут хранится идентификатор файла для индивидуального ключа файла.
        /// </summary>
        private UInt32 m_fileid;

        /// <summary>
        /// Менеджер ключей
        /// </summary>
        private KeyManager m_keyManager;

        /// <summary>
        /// Хеш содержимого файла для сравнения версий
        /// </summary>
        private UInt32 m_ContentHash;

        /// <summary>
        /// Таймштамп момента записи в файл
        /// </summary>
        private DateTime m_WriteTime;
        /// <summary>
        /// Таймштамп момента создания файла
        /// </summary>
        private DateTime m_CreateTime;

        /// <summary>
        /// Конструктор
        /// </summary>
        public ElementCard()
        {
            this.m_ChapterDictionary = new Dictionary<string, CardFileSection>(8);
            this.m_fileid = 0;
            this.m_FilePath = "";
        }


        /// <summary>
        /// Деструктор
        /// </summary>
        ~ElementCard()
        {
            this.m_ChapterDictionary.Clear();
            this.m_ChapterDictionary = null;
            this.m_fileid = 0;
            this.m_FilePath = null;
        }

        /// <summary>
        /// Получить хеш содержимого файла для сравнения версий
        /// </summary>
        public UInt32 ContentHash
        {
            get { return m_ContentHash; }
            //set { m_ContentHash = value; }
        }
        /// <summary>
        /// Получить таймштамп момента записи в файл
        /// </summary>
        public DateTime WriteTime
        {
            get { return m_WriteTime; }
            set { m_WriteTime = value; }
        }
        /// <summary>
        /// Таймштамп момента создания файла
        /// </summary>
        public DateTime CreateTime
        {
            get { return m_CreateTime; }
            set { m_CreateTime = value; }
        }
        /// <summary>
        /// Уникальный идентификатор файла для индивидуального ключа файла.
        /// </summary>
        public UInt32 FileId
        {
            get { return m_fileid; }
            set { m_fileid = value; }
        }

        /// <summary>
        /// Менеджер ключей
        /// </summary>
        public KeyManager KeyManager
        {
            get { return m_keyManager; }
            set { m_keyManager = value; }
        }

        /// <summary>
        /// Путь к файлу элемента
        /// </summary>
        public string FilePath
        {
            get { return m_FilePath; }
            set { m_FilePath = value; }
        }

        /// <summary>
        /// NT-Возвращает труе если это новый элемент, не записанный в хранилише
        /// Проверяются значения: FileId = 0 или FilePath = null
        /// </summary>
        /// <returns></returns>
        public bool IsNewElement()
        {
            return ((m_fileid == 0) || String.IsNullOrEmpty(m_FilePath)); 
        }
        /// <summary>
        /// NR-Создать имя для нового файла.
        /// Эта операция должна быть реализована в производных классах.
        /// </summary>
        /// <returns></returns>
        public virtual string createFileName()
    {
        return null;
    }

        /// <summary>
        /// NR-Извлечь идентификатор файла
        /// </summary>
        /// <param name="filepath">Путь к файлу</param>
        /// <returns>Возвращает идентификатор файла</returns>
        public static uint GetFileId(String filepath)
        {
            BinaryReader br = new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read));
            //read file header
            UInt64 signature = br.ReadUInt64();
            //read card file format version
            UInt16 version = br.ReadUInt16();
            //read fileid
            UInt32 fileid = br.ReadUInt32();
            br.Close();
            //checking
            if (signature != CardFileSignature)
                throw new Exception("Неправильная сигнатура файла " + filepath);
            if(version != CardFileFormatVersion)
                throw new Exception("Неправильная версия файла " + filepath);

            return fileid;
        }
        
        /// <summary>
        /// NR-Загрузить карточку из файла
        /// </summary>
        /// <param name="cardFilePath">Путь к файлу карточки</param>
        public void LoadCard(string cardFilePath)
        {
            //запомним путь к файлу карточки чтобы потом легко сохранить в тот же файл
            this.FilePath = String.Copy(cardFilePath);
            // тут надо загрузить все секции из файла в словарь как блоки данных, с заголовками и контрольными суммами итд.
            // как в снимках тапп
            BinaryReader br = new BinaryReader(File.Open(cardFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
            //read file header
            UInt64 signature = br.ReadUInt64();
            //read card file format version
            UInt16 version = br.ReadUInt16();
            //read fileid
            m_fileid = br.ReadUInt32();
            //read CRC32 - не пересчитываем содержимое файла, просто читаем из заголовка.
            //Это плохо - сумма должна быть защищена ключом шифрования, так как сейчас она позволяет автоматизировать взлом перебором ключей.
            //Однако если ее оставить незащищенной, то для выявления различий версий не придется расшифровывать файл.
            m_ContentHash = br.ReadUInt32();
            //read WriteTime
            m_WriteTime = DateTime.FromFileTime(br.ReadInt64());//DateTime.Now.ToFileTime();
            //read WriteTime
            m_CreateTime = DateTime.FromFileTime(br.ReadInt64());//DateTime.Now.ToFileTime();
            //отсюда и до конца файла должен идти зашифрованный блок.
            //читаем его в массив для расшифровки
            long len = br.BaseStream.Length - br.BaseStream.Position;
            byte[] bar = br.ReadBytes((int)len);
            //close input file
            br.Close();
            //теперь расшифровать это и распарсить на секции
            byte[] bar2 = this.KeyManager.CodeFile(m_fileid, bar);
            //можно сразу очистить буфер, а можно и не очищать - он содержит данные из файла
            //this.KeyManager.ClearBlock(bar);
            //TODO: тут можно проверить данные в bar2 на соответствие CRC сумме
            //распарсить на секции
            MemoryStream ms = new MemoryStream(bar2);
            BinaryReader brr = new BinaryReader(ms, Encoding.UTF8);
            //читаем секции пока не встретится FileEndTag = 0xAA или конец файла
            Byte sectionTag;
            UInt32 sectionLength;
            Int32 dataLength;
            Byte[] sectionData;
            String sectionTitle;
            while (brr.BaseStream.Position < brr.BaseStream.Length)
            {
                sectionTag = brr.ReadByte();
                if (sectionTag == CardFileSection.FileEndTag)
                    break;
                else if (sectionTag != CardFileSection.SectionStartTag)
                    throw new Exception(String.Format("Неправильный формат файла {0}", cardFilePath));
                sectionLength = brr.ReadUInt32();
                dataLength = brr.ReadInt32();
                sectionData = brr.ReadBytes(dataLength);
                sectionTitle = brr.ReadString();
                //write to section
                this.WriteChapter(sectionTitle, sectionData);
            }
            //закрыть реадер и поток
            brr.Close();
            //очистить буфер
            this.KeyManager.ClearBlock(bar2);
            return;
        }

        /// <summary>
        /// NT-Сохранить карточку в тот же файл
        /// </summary>
        public void SaveCard()
        {
            SaveCard(this.FilePath);
        }

        /// <summary>
        /// NR-Сохранить карточку в другой файл. Ноавый путь не становится текущим для карточки
        /// </summary>
        public void SaveCard(String newCardFilePath)
        {
            // тут надо сохранить все секции из словаря в файл в определенном порядке, как блок данных, с заголовками и контрольными суммами итд.
            // как в снимках тапп
                        
            //this.m_CardFilePath = String.Copy(newCardFilePath);
            //1-создать файл
            BinaryWriter bw = new BinaryWriter(File.Create(newCardFilePath));
            //2-записать шапку файла
            // signature 8 bytes
            bw.Write(CardFileSignature);
            //3 - file struct version 2 bytes
            bw.Write(CardFileFormatVersion);
            //4 - file id 4 bytes
            bw.Write(this.m_fileid);



            //вывести все данные секций во временный массив в памяти для шифрования
            MemoryStream ms = new MemoryStream(65536);
            BinaryWriter bm = new BinaryWriter(ms, Encoding.UTF8);
            UInt32 sectionLength = 0;
            long sectionStartPos = 0;
            long SectionEndPos = 0;
            //6 - store section list
            foreach (CardFileSection cfs in m_ChapterDictionary.Values)
            {
                //TODO: Write section here
                //write section start tag - 1 byte
                bm.Write(CardFileSection.SectionStartTag);
                //write section length - 4 byte
                bm.Write(sectionLength);//now write 0 length
                //save current file pos for length calculation
                sectionStartPos = bm.BaseStream.Position;
                //store data length  - 4 bytes
                bm.Write(cfs.Data.Length);
                //store data values - see prev bytes
                bm.Write(cfs.Data);
                //save title - ненадежно, надо бы длину строки тоже сохранить здесь.
                bm.Write(cfs.Title); //-? bytes
                // store checksum? 
                // store end section tag - не нужен.
                //write section length
                SectionEndPos = bm.BaseStream.Position;
                sectionLength = (UInt32)(SectionEndPos - sectionStartPos);
                bm.BaseStream.Position = sectionStartPos - sizeof(UInt32);
                bm.Write(sectionLength);
                //restore end position
                bm.BaseStream.Position = SectionEndPos;
            }
            //write file end tag here
            bm.Write(CardFileSection.FileEndTag);
            //encode data and write to file
            bm.Flush();
            byte[] bar = ms.ToArray();
            
            //CRC32 посчитать и вывести 4bytes
            this.m_ContentHash = Crc32s.ComputeChecksum(bar);
            bw.Write(m_ContentHash);
            //WriteTime дату записи посчитать и вывести 8bytes
            this.m_WriteTime = DateTime.Now;
            bw.Write(this.m_WriteTime.ToFileTime());
            //CreateTime дату записи вывести 8bytes
            bw.Write(this.m_CreateTime.ToFileTime());

            //encode data 
            byte[] fb = m_keyManager.CodeFile(m_fileid, bar);
            //destroy buffer with non-coded data
            m_keyManager.ClearBlock(bar);
            bar = null;
            //close memory writer and memory stream
            bm.Close();

            //write to file 
            bw.Write(fb);
            //close output file
            bw.Close();
            //clear and destroy buffer
            m_keyManager.ClearBlock(fb);
            fb = null;

            return;
        }


        //Логично было бы получить раздел сразу как текст или картинку, а не сырой блок байт.
        //Но лучше мы эту конверсию будем делать в этом классе, а не в секции файла.

        /// <summary>
        /// NT-Получить данные раздела. Возвращает null если нет такого раздела.
        /// </summary>
        /// <param name="НазваниеРаздела">Название раздела</param>
        public Byte[] GetChapter(string ChapterTitle)
        {
            if (this.m_ChapterDictionary.ContainsKey(ChapterTitle))
                return m_ChapterDictionary[ChapterTitle].Data;
            else return null;

        }

        /// <summary>
        /// NT-Получить данные раздела. Возвращает null если нет такого раздела.
        /// </summary>
        /// <param name="НазваниеРаздела">Название раздела</param>
        public String GetChapterAsText(string ChapterTitle)
        {
            Byte[] bar = GetChapter(ChapterTitle);
            if (bar != null)
            {
                //конвертировать байты в строку и вернуть строку
                return CStringProcessor.BytesToString(bar);
            }
            else return null;
        }

        /// <summary>
        /// NT-Получить данные раздела. Возвращает null если нет такого раздела.
        /// </summary>
        /// <param name="НазваниеРаздела">Название раздела</param>
        public System.Drawing.Image GetChapterAsImage(string ChapterTitle)
        {
            Byte[] bar = GetChapter(ChapterTitle);
            if (bar != null)
            {
                //конвертировать байты в картинку и вернуть картинку
                return CImageProcessor.BytesToImage(bar);
            }
            else return null;
        }

        /// <summary>
        /// NT-Перезаписать данные раздела или добавить новый раздел
        /// </summary>
        /// <param name="НазваниеРаздела">Название раздела</param>
        /// <param name="БлокДанныхРаздела">Блок данных раздела</param>
        public void WriteChapter(String ChapterTitle, Byte[] Data)
        {
            if(this.m_ChapterDictionary.ContainsKey(ChapterTitle))
            {
                m_ChapterDictionary[ChapterTitle].Data = Data;
            }
            else
            {
                m_ChapterDictionary.Add(ChapterTitle, new CardFileSection(ChapterTitle, Data));
            }
        }

        /// <summary>
        /// NT-Перезаписать данные раздела или добавить новый раздел
        /// </summary>
        /// <param name="НазваниеРаздела">Название раздела</param>
        /// <param name="ТекстРаздела">Текст раздела</param>
        public void WriteChapterAsText(String title, String text)
        {
            Byte[] bar = null;
            //Конвертировать тут ТекстРаздела в массив байт
            bar = CStringProcessor.StringToBytes(text);
            WriteChapter(title, bar);

            return;
        }


        /// <summary>
        /// NT-Перезаписать данные раздела или добавить новый раздел
        /// </summary>
        /// <param name="НазваниеРаздела">Название раздела</param>
        /// <param name="Изображение">Изображение</param>
        public void WriteChapterAsImage(String title, System.Drawing.Image picture)
        {

            Byte[] bar = null;
            //Конвертировать тут Изображение в массив байт
            bar = CImageProcessor.ImageToBytes(picture);

            WriteChapter(title, bar);

            return;

        }


        /// <summary>
        /// NT-Удалить раздел
        /// </summary>
        /// <param name="НазваниеРаздела">Название раздела</param>
        public void RemoveChapter(string ChapterTitle)
        {
            if (this.m_ChapterDictionary.ContainsKey(ChapterTitle))
                m_ChapterDictionary.Remove(ChapterTitle);
            return;
        }
    }
}
