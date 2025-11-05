using BinAnalyzer.Data;
using LogModule;
using ProberInterfaces.Enum;
using System;
using System.Linq;

namespace BinDataMaker
{
    public class BinDataMaker
    {
        private const string PassFailPattern = "^@ABCDEFGHIJKLMNO";

        //private bool IsDemoMode { get; set; }
        Random random = new Random(DateTime.Now.Millisecond);

        private int DutCount { get; set; }

        private string Prefix { get; set; }

        private BinType BinType { get; set; }

        public void SetBinType(BinType type)
        {
            try
            {
                this.BinType = type;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        //public void DemoModeOnOff(bool mode)
        //{
        //    try
        //    {
        //        IsDemoMode = mode;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public void SetDutCount(int dutcount)
        {
            try
            {
                this.DutCount = dutcount;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string MakeBinInfo()
        {
            string retval = string.Empty;
            string BinData = string.Empty;

            try
            {
                if (BinType == BinType.BIN_PASSFAIL)
                {
                    BinData = MakeDummyPassFailInfo();

                    bool judgmentValue = false;

                    foreach (char c in BinData)
                    {
                        judgmentValue = PassFailPattern.Contains(c);

                        if (judgmentValue == false)
                        {
                            break;
                        }
                    }

                    if (judgmentValue == false)
                    {

                    }
                }
                else if (BinType == BinType.BIN_6BIT)
                {
                    BinData = MakeDummy6BITInfo();
                }
                else if (BinType == BinType.BIN_8BIT)
                {
                    BinData = MakeDummy8BITInfo();
                }

                retval = Prefix + BinData;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetPrefix(string prefix)
        {
            try
            {
                this.Prefix = prefix;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string MakeDummyPassFailInfo()
        {
            string retval = "";

            try
            {
                int PossibleBinDataLength = (int)Math.Ceiling(((double)DutCount / (double)BinGlobalValidation.BIN_6BIT));

                // 4 dut
                /// XXXX 0000 : ALL FAIL (@)
                /// XXXX 1111 : ALL PASS (O)
                /// 

                // Dut 개수에 따라, 가능한 Data

                // DUT : 1 : 0000, 0001

                // DUT : 2 : 0000, 0001, 
                //           0010, 0011

                // DUT : 3 : 0000, 0001, 
                //           0010, 0011, 
                //           0100, 0101, 0110, 0111

                // DUT : 4 : 0000, 0001,
                //           0010, 0011, 
                //           0100, 0101, 0110, 0111
                //           1000, 1001, 1010, 1011, 1100, 1101, 1110, 1111

                int[] MaximumRnd = { 2 << 0, 2 << 1, 2 << 2, 2 << 3 };
                int remaindutcount = 0;

                for (int i = 0; i < PossibleBinDataLength; i++)
                {
                    int mod = (DutCount - (i * BinGlobalValidation.BIN_6BIT)) % BinGlobalValidation.BIN_6BIT;

                    if (mod == 0)
                    {
                        remaindutcount = BinGlobalValidation.BIN_6BIT;
                    }
                    else
                    {
                        remaindutcount = mod;
                    }

                    int randData = random.Next(0, MaximumRnd[remaindutcount - 1]);

                    retval += (char)((int)'@' + randData);
                }

                LoggerManager.Debug($"[BinDataMaker], MakeDummyPassFailInfo() : retval = {retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string MakeDummy6BITInfo()
        {
            string retval = "";

            try
            {
                string tmpStr = string.Empty;

                //int passPercent = 35;

                int PassFailCount = (int)Math.Ceiling(((double)DutCount / (double)BinGlobalValidation.BIN_6BIT));

                char[] DutPFResult = new char[PassFailCount];
                char[] DutBinResult = new char[DutCount];

                // Make Pass/Fail

                int[] MaximumRnd = { 2 << 0, 2 << 1, 2 << 2, 2 << 3 };

                int remaindutcount = DutCount;
                int rndindex = 0;

                for (int i = 0; i < PassFailCount; i++)
                {
                    if (remaindutcount - BinGlobalValidation.BIN_6BIT >= 0)
                    {
                        rndindex = BinGlobalValidation.BIN_6BIT;

                        remaindutcount = remaindutcount - BinGlobalValidation.BIN_6BIT;
                    }
                    else
                    {
                        rndindex = remaindutcount;
                    }

                    int randData = random.Next(0, MaximumRnd[rndindex - 1]);

                    DutPFResult[i] = (char)((int)'@' + randData);
                }

                // Make Bin Data

                for (int i = 0; i < DutCount; i++)
                {
                    int randData = random.Next(0, 2 << 5);

                    DutBinResult[i] = (char)((int)'@' + randData);
                }

                // Merege

                int PFIndex = 0;

                for (int i = 0; i < DutCount; i++)
                {
                    if (i % BinGlobalValidation.BIN_6BIT == 0)
                    {
                        retval += DutPFResult[PFIndex];

                        PFIndex++;
                    }

                    retval += DutBinResult[i];
                }

                LoggerManager.Debug($"[BinDataMaker], MakeDummy6BITInfo() : retval = {retval}");

                //for (int i = 0; i < DutCount; i++)
                //{
                //    int randData = random.Next(0, 100);

                //    if (i % 4 == 0)
                //    {
                //        tmpStr = string.Empty;
                //    }

                //    int SecondRandData = random.Next(1, 5);

                //    randData = randData < passPercent ? 0 : SecondRandData;

                //    tmpStr += (char)((int)'@' + randData);

                //    if (DutCount % 4 == 3 || i == (DutCount - 1))
                //    {
                //        char prePix = '@';

                //        for (int j = 0; j < (tmpStr?.Length ?? 0); j++)
                //        {
                //            if (tmpStr[j] != '@')
                //            {
                //                prePix = (char)(((int)prePix) | (1 << j));
                //            }
                //        }

                //        retval = $"{retval}{prePix}{tmpStr}";
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string MakeDummy8BITInfo()
        {
            string retval = "";

            try
            {
                // 8bit의 경우, P/F의 제작은 6bit와 동일하다.
                // Bin code의 경우, Dut 1개당 문자 2개를 사용한다.

                string tmpStr = string.Empty;

                int PassFailCount = (int)Math.Ceiling(((double)DutCount / (double)BinGlobalValidation.BIN_6BIT));

                char[] DutPFResult = new char[PassFailCount];
                char[] DutBinResult = new char[DutCount * 2];

                // Make Pass/Fail

                int[] MaximumRnd = { 2 << 0, 2 << 1, 2 << 2, 2 << 3 };

                int remaindutcount = DutCount;
                int rndindex = 0;

                for (int i = 0; i < PassFailCount; i++)
                {
                    if (remaindutcount - BinGlobalValidation.BIN_6BIT >= 0)
                    {
                        rndindex = BinGlobalValidation.BIN_6BIT;

                        remaindutcount = remaindutcount - BinGlobalValidation.BIN_6BIT;
                    }
                    else
                    {
                        rndindex = remaindutcount;
                    }

                    int randData = random.Next(0, MaximumRnd[rndindex - 1]);

                    DutPFResult[i] = (char)((int)'@' + randData);
                }

                // Make Bin Data

                //string candidate = "@ABCDEFGHIJKLMNO";
                string candidate = "@ABCD";

                // O    => 0100 1111
                // 0x3F => 0011 1111

                // 0x3F와 & 연산 시...

                for (int i = 0; i < DutCount; i++)
                {
                    int randData1 = random.Next(0, candidate.Length);
                    int randData2 = random.Next(0, candidate.Length);

                    char firstChar = candidate[randData1];
                    char secondChar = candidate[randData2];

                    DutBinResult[i] = firstChar;
                    DutBinResult[i + 1] = secondChar;

                    //int randData = random.Next(0, 1 << BinGlobalValidation.BIN_8BIT);
                    //string hexValue = randData.ToString("X");

                    //int intAgain = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

                    //int SecondrandData = random.Next(0, candidate.Length);

                    //int first = candidate[FirstrandData] & 0x3F;
                    //int second = candidate[SecondrandData] & 0x3F;

                    //byte[] ba = new byte[2];
                    //ba[0] = Convert.ToByte(first);
                    //ba[1] = Convert.ToByte(second);

                    //string hexstr = first.ToString() + second.ToString();

                    //var hexstring = BitConverter.ToString(ba);
                }

                // Merege

                int PFIndex = 0;

                for (int i = 0; i < DutCount; i++)
                {
                    if (i % BinGlobalValidation.BIN_6BIT == 0)
                    {
                        retval += DutPFResult[PFIndex];

                        PFIndex++;
                    }

                    retval += DutBinResult[i];
                    retval += DutBinResult[i + 1];
                }

                LoggerManager.Debug($"[BinDataMaker], MakeDummy8BITInfo() : retval = {retval}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
