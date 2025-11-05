using BinAnalyzer.Analzer;
using BinAnalyzer.Data;
using ProberInterfaces.Enum;
using System;

namespace BinAnalyzer
{
    public class BinAnalyzerObjFactory
    {
        public static BinAnalyzerBase GetBinAnalyzerObj(BinType binType, HardBinData HardBinData = null)
        {
            BinAnalyzerBase retBinAnlayzerObj = null;
            try
            {

                switch (binType)
                {
                    case BinType.BIN_PASSFAIL:
                        retBinAnlayzerObj = new BinAnalyzer_PassFail();
                        break;
                    case BinType.BIN_6BIT:
                        retBinAnlayzerObj = new BinAnalyzer_6Bit();
                        break;
                    case BinType.BIN_8BIT:
                        retBinAnlayzerObj = new BinAnalyzer_8Bit();
                        break;
                    case BinType.BIN_16BIT:
                        retBinAnlayzerObj = new BinAnalyzer_16Bit();
                        break;
                    case BinType.BIN_32BIT:
                        retBinAnlayzerObj = new BinAnalyzer_32Bit();
                        break;
                    case BinType.BIN_32BIT_SPECIAL:
                        retBinAnlayzerObj = new BinAnalyzer_32Bit_Special();
                        break;
                    case BinType.BIN_U32_32BIT:
                        retBinAnlayzerObj = new BinAnalyzer_UserBin();
                        break;
                    case BinType.BIN_U32_64BIT:
                        retBinAnlayzerObj = new BinAnalyzer_UserBin();
                        break;
                    case BinType.BIN_USERBIN:
                        retBinAnlayzerObj = new BinAnalyzer_UserBin();
                        break;
                    case BinType.BIN_1024BIT:
                        retBinAnlayzerObj = new BinAnalyzer_1024Bit();
                        break;
                }

                if (retBinAnlayzerObj != null)
                {
                    retBinAnlayzerObj.HardBinData = HardBinData;
                }

            }
#pragma warning disable 0168
            catch (Exception err)
            {
                throw;
            }
#pragma warning restore 0168
            return retBinAnlayzerObj;
        }
    }
}
