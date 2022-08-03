
using System;

namespace MyCodeLibrary.TextProcessing.UDE
{
    /// <summary>
    /// Хранит названия поддерживаемых кодировок
    /// </summary>
    public static class Charsets
    {
        //TODO: если нужно изменить название кодировки, следует делать это здесь.
        //пробую изменить названия на принятые для Encoding.Webname

        /// <summary>
        /// US-ASCII
        /// CodePage=20127
        /// </summary>
        public const string ASCII = "us-ascii";
        /// <summary>
        /// Unicode (UTF-8)
        /// CodePage=65001
        /// </summary>
        public const string UTF8 = "utf-8";
        /// <summary>
        /// Unicode
        /// CodePage=1200
        /// </summary>
        public const string UTF16_LE = "utf-16";
        /// <summary>
        /// Unicode (Big-Endian)
        /// CodePage=1201
        /// </summary>
        public const string UTF16_BE = "unicodeFFFE";
        /// <summary>
        /// Unicode (UTF-32 Big-Endian)
        /// CodePage=12001
        /// </summary>
        public const string UTF32_BE = "utf-32BE";
        /// <summary>
        /// Unicode (UTF-32)
        /// CodePage=12000
        /// </summary>
        public const string UTF32_LE = "utf-32";

        /// <summary>
        /// Unusual BOM (3412 order)
        /// CodePage=?
        /// </summary>
        public const string UCS4_3412 = "X-ISO-10646-UCS-4-3412";//нет в моих кодировках
        
        /// <summary>
        /// Unusual BOM (2143 order)
        /// CodePage=?
        /// </summary>
        public const string UCS4_2143 = "X-ISO-10646-UCS-4-2143";//нет в моих кодировках

        /// <summary>
        /// Central European (Windows)
        /// Hungarian?
        /// CodePage=1250
        /// </summary>
        public const string WIN1250 = "windows-1250";

        /// <summary>
        /// Cyrillic (Windows)
        /// Cyrillic (based on bulgarian and russian data)
        /// CodePage=1251
        /// </summary>
        public const string WIN1251 = "windows-1251";
        
        /// <summary>
        /// Western European (Windows)
        /// Latin-1, almost identical to ISO-8859-1
        /// CodePage=1252
        /// </summary>
        public const string WIN1252 = "Windows-1252";
        
        /// <summary>
        /// Greek (Windows)
        /// Greek
        /// CodePage=1253
        /// </summary>
        public const string WIN1253 = "windows-1253";
        
        /// <summary>
        /// Hebrew (Windows)
        /// Logical hebrew (includes ISO-8859-8-I and most of x-mac-hebrew)
        /// CodePage=1255
        /// </summary>
        public const string WIN1255 = "windows-1255";
        
        /// <summary>
        /// Chinese Traditional (Big5)
        /// CodePage=950
        /// </summary>
        public const string BIG5 = "big5";//code=950
        /// <summary>
        /// Korean (EUC)
        /// CodePage=51949
        /// </summary>
        public const string EUCKR = "euc-kr";
        /// <summary>
        /// Japanese (EUC)
        /// CodePage=51932
        /// </summary>
        public const string EUCJP = "euc-jp";
        /// <summary>
        /// 
        /// CodePage=?
        /// </summary>
        public const string EUCTW = "EUC-TW";//нет в моих кодировках

        /// <summary>
        /// Chinese Simplified (GB18030)
        /// Note: gb2312 is a subset of gb18030
        /// CodePage=54936
        /// </summary>
        public const string GB18030 = "GB18030";
        /// <summary>
        /// Japanese (JIS)
        /// CodePage=50220
        /// </summary>
        public const string ISO2022_JP = "iso-2022-jp";
        /// <summary>
        /// 
        /// CodePage=?
        /// </summary>
        public const string ISO2022_CN = "ISO-2022-CN";//нет в моих кодировках
        /// <summary>
        /// Korean (ISO)
        /// CodePage=50225
        /// </summary>
        public const string ISO2022_KR = "iso-2022-kr";
        
        /// <summary>
        /// Chinese Simplified (HZ)
        /// CodePage=52936
        /// </summary>
        public const string HZ_GB_2312 = "hz-gb-2312";
        /// <summary>
        /// Japanese (Shift-JIS)
        /// CodePage=932
        /// </summary>
        public const string SHIFT_JIS = "shift_jis";
        /// <summary>
        /// Cyrillic (Mac)
        /// CodePage=10007
        /// </summary>
        public const string MAC_CYRILLIC = "x-mac-cyrillic";
        /// <summary>
        /// Cyrillic (KOI8-R)
        /// CodePage=20866
        /// </summary>
        public const string KOI8R = "koi8-r";
        /// <summary>
        /// OEM Cyrillic
        /// CodePage=855
        /// </summary>
        public const string IBM855 = "IBM855";
        /// <summary>
        /// Cyrillic (DOS)
        /// CodePage=866
        /// </summary>
        public const string IBM866 = "cp866";

        /// <summary>
        /// Central European (ISO)
        /// East-Europe. Disabled because too similar to windows-1252 
        /// (latin-1). Should use tri-grams models to discriminate between
        /// these two charsets.
        /// CodePage=28592
        /// </summary>
        public const string ISO8859_2 = "iso-8859-2";

        /// <summary>
        /// Cyrillic (ISO) 
        /// CodePage=28595
        /// </summary>
        public const string ISO8859_5 = "iso-8859-5";

        /// <summary>
        /// Greek (ISO)
        /// CodePage=28597
        /// </summary>
        public const string ISO_8859_7 = "iso-8859-7";

        /// <summary>
        /// Hebrew (ISO-Visual)
        /// CodePage=28598
        /// </summary>
        public const string ISO8859_8 = "iso-8859-8";

        /// <summary>
        /// Thai. This recognizer is not enabled yet. 
        /// 
        /// CodePage=?
        /// </summary>
        public const string TIS620 = "TIS620";//нет в моих кодировках
        
    }
}
