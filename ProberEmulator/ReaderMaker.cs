using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProberEmulator
{
    public enum FABTYPE
    {
        ST = 0,
        SMIC,
        UMC,
        TSMC,
        HYNIX,
        CHARTERED,
        PHILIPS
    }
    public enum CHECKSUMTYPE
    { 
        SEMI,
        IBM
    }
    
    public static class ReaderMaker
    {
        // TODO : LIST -> specific class
        public static List<string> ST_ReaderPattern { get; set; }
        public static List<string> SMIC_ReaderPattern { get; set; }
        public static List<string> UMC_ReaderPattern { get; set; }
        public static List<string> TSMC_ReaderPattern { get; set; }
        public static List<string> HYNIX_ReaderPattern { get; set; }
        public static List<string> CHARTERED_ReaderPattern { get; set; }
        public static List<string> PHILIPS_ReaderPattern { get; set; }

        private static bool Initialzied { get; set; }

        private static void init()
        {
            try
            {
                // TODO : Default set
                // FAB TYPE 마다 사용할 수 있는 Reader Pattern은 정해져있음.

                if (ST_ReaderPattern == null)
                {
                    ST_ReaderPattern = new List<string>();
                }

                ST_ReaderPattern.Add("LLLLLLL-WWCC");
                ST_ReaderPattern.Add("LLLLLLLL-WWCC");
                ST_ReaderPattern.Add("LLLLLLLLL-WWCC");

                if (SMIC_ReaderPattern == null)
                {
                    SMIC_ReaderPattern = new List<string>();
                }

                SMIC_ReaderPattern.Add("LLLLLLLLWWCC");

                if (UMC_ReaderPattern == null)
                {
                    UMC_ReaderPattern = new List<string>();
                }

                UMC_ReaderPattern.Add("LLLLLLLLWWCC");

                if (TSMC_ReaderPattern == null)
                {
                    TSMC_ReaderPattern = new List<string>();
                }

                TSMC_ReaderPattern.Add("LLLLLL-WWCC");

                if (HYNIX_ReaderPattern == null)
                {
                    HYNIX_ReaderPattern = new List<string>();
                }

                HYNIX_ReaderPattern.Add("LLLLLLLLWWCC");

                if (CHARTERED_ReaderPattern == null)
                {
                    CHARTERED_ReaderPattern = new List<string>();
                }

                CHARTERED_ReaderPattern.Add("LLLLLLLLWWCC");

                if (PHILIPS_ReaderPattern == null)
                {
                    PHILIPS_ReaderPattern = new List<string>();
                }

                PHILIPS_ReaderPattern.Add("LLLLLWWCC");

                Initialzied = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public static string GetReaderPattern(FABTYPE type, string lotname)
        {
            string retval = string.Empty;

            try
            {
                if(Initialzied ==false)
                {
                    init();
                }

                List<string> o = null;

                switch (type)
                {
                    case FABTYPE.ST:
                        o = ST_ReaderPattern;
                        break;
                    case FABTYPE.SMIC:
                        o = SMIC_ReaderPattern;
                        break;
                    case FABTYPE.UMC:
                        o = UMC_ReaderPattern;
                        break;
                    case FABTYPE.TSMC:
                        o = TSMC_ReaderPattern;
                        break;
                    case FABTYPE.HYNIX:
                        o = HYNIX_ReaderPattern;
                        break;
                    case FABTYPE.CHARTERED:
                        o = CHARTERED_ReaderPattern;
                        break;
                    case FABTYPE.PHILIPS:
                        o = PHILIPS_ReaderPattern;
                        break;
                    default:
                        break;
                }

                if (o != null)
                {
                    int lotlength = lotname.Length;

                    retval = o.FirstOrDefault(x => x.Count(L => L == 'L') == lotlength);

                    // TODO : 
                    if (retval != null)
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="lotname"></param>
        /// <param name="slotnumber"></param>
        /// <returns></returns>
        public static string MakeWaferID(string pattern, string lotname, int slotnumber)
        {
            string retval = string.Empty;

            try
            {
                // L - Refers the Marking Lot Number, any alphanumeric character
                // W – Refers the Wafer Number, integer
                // C – Refers the SEMI Checksum, any alphanumeric character

                bool isValid = false;

                // TODO : 입력받은 Pattern과 Lotname의 유효성 확인.

                int PatternLotlength = 0;
                int InputLotlength = lotname.Length;

                char[] idarray = new char[pattern.Length];

                int startindex = pattern.IndexOf('-');

                if(startindex != -1)
                {
                    idarray[startindex] = '-';
                }

                //MatchCollection matches = Regex.Matches(pattern, "L");
                //PatternLotlength = matches.Count;

                PatternLotlength = pattern.Count(L => L == 'L');

                if(PatternLotlength != InputLotlength)
                {
                    isValid = false;
                }
                else
                {
                    isValid = true;
                }

                if(isValid == true)
                {
                    string checksum = string.Empty;

                    int startIndex;

                    // LOT
                    InsertData(idarray, pattern, lotname, 'L');

                    // SLOT

                    startIndex = pattern.IndexOf('W');

                    int wcount = pattern.Count(x => x == 'W');
                    string wformat = string.Empty;

                    for (int i = 0; i < wcount; i++)
                    {
                        wformat += "0";
                    }

                    string convertedSlotNum = slotnumber.ToString(wformat);

                    InsertData(idarray, pattern, convertedSlotNum, 'W');

                    // CHECKSUM

                    string OCR = GetOCR(idarray, pattern);

                    checksum = MakeChecksum(OCR);

                    InsertData(idarray, pattern, checksum, 'C');

                    retval = new string(idarray);
                }
                else
                {
                    // TODO : 
                    LoggerManager.Error($"");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private static void InsertData(char[] idarray, string pattern, string datas, char c)
        {
            try
            {
                int startindex = pattern.IndexOf(c);

                foreach (var data in datas)
                {
                    idarray[startindex] = data;
                    startindex++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private static string GetOCR(char[] idarray, string pattern)
        {
            string retval = string.Empty;

            try
            {
                //int CheckSumStartindex = pattern.IndexOf('C');

                retval = new string(idarray);
                //retval = retval.Substring(0, CheckSumStartindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static string MakeChecksum(string ocr)
        {
            string retval = string.Empty;
            CHECKSUMTYPE type = CHECKSUMTYPE.SEMI;

            try
            {
                if(type == CHECKSUMTYPE.SEMI)
                {
                    retval = GetSEMIChecksum(ocr);
                }
                else if (type == CHECKSUMTYPE.IBM)
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private static string GetSEMIChecksum(string ocr)
        {
            string retval = string.Empty;
            char[] inputCheckSum = new char[2];

            try
            {
                if(ocr.Length >=2)
                {
                    inputCheckSum[0] = ocr[ocr.Length - 2];
                    inputCheckSum[1] = ocr[ocr.Length - 1];

                    StringBuilder stb = new StringBuilder(ocr);
                    stb[ocr.Length - 2] = 'A';
                    stb[ocr.Length - 1] = '0';
                    String strImsiOcrBuf = stb.ToString();

                    int[] chrtmp = new int[18];

                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        chrtmp[i] = strImsiOcrBuf[i];
                    }

                    int seed = 0;

                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        int j = (seed * 8) % 59;

                        j = j + chrtmp[i] - 32;

                        seed = j % 59;
                    }

                    char[] calcCheckSum = new char[2];

                    if (seed == 0)
                    {
                        calcCheckSum[0] = 'A';
                        calcCheckSum[1] = '0';
                    }
                    else
                    {
                        seed = 59 - seed;

                        int j = (seed / 8) & 0x7;
                        int i = seed & 0x7;

                        calcCheckSum[0] = (char)(j + 33 + 32);
                        calcCheckSum[1] = (char)(i + 16 + 32);
                    }

                    retval = $"{calcCheckSum[0]}{calcCheckSum[1]}";
                    //result = inputCheckSum[0] == calcCheckSum[0] && inputCheckSum[1] == calcCheckSum[1];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private static string GetIBMChecksum(string ocr)
        {
            string retval = string.Empty;
            char[] inputCheckSum = new char[2];

            try
            {
                int fe = 0;
                int fo = 0;
                int cd = 0;

                int[] ibmChar = new int[18];

                for (int i = 0; i < ocr.Length; i++)
                {
                    ibmChar[i] = IBM_Checksum_Char_Val(ocr[i]);

                    if (ibmChar[i] < 0)
                    {
                        retval = string.Empty;
                        break;
                    }

                    //==> 번갈아 가며 계산
                    if (i % 2 == 0)//==> 첫 순번인 녀석
                    {
                        fe = fe + ibmChar[i];
                    }
                    else
                    {
                        if (i == 1)//==> CheckSum index
                            fo = fo + 0;
                        else
                            fo = fo + ibmChar[i];
                    }
                }

                cd = (17 * (fe + (2 * fo))) % 35;

                if (cd == ibmChar[1])
                {
                    fo = fo + cd;

                    //result = (17 * (fe + (2 * fo))) % 35 == 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private static bool SEMIChecksum(String ocr)
        {
            char[] inputCheckSum = new char[2];
            bool result = false;

            try
            {
                if (ocr.Length > 2)
                {

                    inputCheckSum[0] = ocr[ocr.Length - 2];
                    inputCheckSum[1] = ocr[ocr.Length - 1];

                    StringBuilder stb = new StringBuilder(ocr);
                    stb[ocr.Length - 2] = 'A';
                    stb[ocr.Length - 1] = '0';
                    String strImsiOcrBuf = stb.ToString();

                    int[] chrtmp = new int[18];
                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        chrtmp[i] = strImsiOcrBuf[i];
                    }

                    int seed = 0;
                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        int j = (seed * 8) % 59;
                        j = j + chrtmp[i] - 32;
                        seed = j % 59;
                    }

                    char[] calcCheckSum = new char[2];
                    if (seed == 0)
                    {
                        calcCheckSum[0] = 'A';
                        calcCheckSum[1] = '0';
                    }
                    else
                    {
                        seed = 59 - seed;
                        int j = (seed / 8) & 0x7;
                        int i = seed & 0x7;
                        calcCheckSum[0] = (char)(j + 33 + 32);
                        calcCheckSum[1] = (char)(i + 16 + 32);
                    }

                    String checksum = $"{calcCheckSum[0]}{calcCheckSum[1]}";
                    result = inputCheckSum[0] == calcCheckSum[0] && inputCheckSum[1] == calcCheckSum[1];
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }

        private static bool IMBChecksum(String ocr)
        {
            bool result = false;
            try
            {
                int fe = 0;
                int fo = 0;
                int cd = 0;

                int[] ibmChar = new int[18];
                for (int i = 0; i < ocr.Length; i++)
                {
                    ibmChar[i] = IBM_Checksum_Char_Val(ocr[i]);
                    if (ibmChar[i] < 0)
                        return false;

                    //==> 번갈아 가며 계산
                    if (i % 2 == 0)//==> 첫 순번인 녀석
                    {
                        fe = fe + ibmChar[i];
                    }
                    else
                    {
                        if (i == 1)//==> CheckSum index
                            fo = fo + 0;
                        else
                            fo = fo + ibmChar[i];
                    }
                }

                //cd = (17 * (fo + (2 * fe))) % 35;

                cd = (17 * (fe + (2 * fo))) % 35;
                if (cd == ibmChar[1])
                {
                    fo = fo + cd;
                    result = (17 * (fe + (2 * fo))) % 35 == 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }

        private static int IBM_Checksum_Char_Val(char chr)
        {
            int ret = -1;
            try
            {
                switch (chr)
                {
                    case '0': ret = 0; break;
                    case '1': ret = 1; break;
                    case '2': ret = 2; break;
                    case '3': ret = 3; break;
                    case '4': ret = 4; break;
                    case '5': ret = 5; break;
                    case '6': ret = 6; break;
                    case '7': ret = 7; break;
                    case '8': ret = 8; break;
                    case '9': ret = 9; break;
                    case 'a': ret = 10; break;
                    case 'b': ret = 11; break;
                    case 'c': ret = 12; break;
                    case 'd': ret = 13; break;
                    case 'e': ret = 14; break;
                    case 'f': ret = 15; break;
                    case 'g': ret = 16; break;
                    case 'h': ret = 17; break;
                    case 'i': ret = 18; break;
                    case 'j': ret = 19; break;
                    case 'k': ret = 20; break;
                    case 'l': ret = 21; break;
                    case 'm': ret = 22; break;
                    case 'n': ret = 23; break;
                    case 'p': ret = 24; break;
                    case 'q': ret = 25; break;
                    case 'r': ret = 26; break;
                    case 's': ret = 27; break;
                    case 't': ret = 28; break;
                    case 'u': ret = 29; break;
                    case 'v': ret = 30; break;
                    case 'w': ret = 31; break;
                    case 'x': ret = 32; break;
                    case 'y': ret = 33; break;
                    case 'z': ret = 34; break;
                    default: ret = -1; break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
