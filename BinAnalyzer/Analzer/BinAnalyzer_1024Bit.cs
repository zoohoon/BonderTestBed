using BinAnalyzer.Data;
using ProberInterfaces.BinData;
using ProbingDataInterface;
using System.Linq;

namespace BinAnalyzer.Analzer
{
    internal class BinAnalyzer_1024Bit : BinAnalyzerBase
    {
        protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = true;

            int resultCodeTmpLength = resultCode.Length;
            int division = 0;
            int remain = 0;
            division = resultCode.Length / (BinGlobalValidation.BIN_8BIT + 1);
            remain = resultCode.Length % (BinGlobalValidation.BIN_8BIT + 1);
            if (division != 0 && remain == 0)
            {
                division -= 1;
            }

            resultCodeTmpLength -= division;

            if (resultCodeTmpLength % 2 == 0)
            {
                retVal = false;
                return retVal;
            }

            try
            {
                int loopCount = 0;
                loopCount = (analysisDataArray.Count - 1) >> 2;

                for (int i = 0; i < loopCount + 1; i++)
                {
                    int resultCodeCount = 0;

                    if (i == loopCount)
                    {
                        int DataArrayLoopCount = 0;
                        int resultCodeLoopCount = 0;

                        DataArrayLoopCount = analysisDataArray.Count % 4;
                        DataArrayLoopCount = (DataArrayLoopCount == 0) ? 4 : analysisDataArray.Count % 4;
                        resultCodeLoopCount = (resultCode.Length - (BinGlobalValidation.BIN_1024BIT * i)) / (BinGlobalValidation.BIN_1024BIT >> 2);

                        resultCodeCount = DataArrayLoopCount < resultCodeLoopCount ? DataArrayLoopCount : resultCodeLoopCount;
                    }
                    else
                    {
                        resultCodeCount = 4;
                    }

                    for (int j = 0; j < resultCodeCount; j++)
                    {
                        int idx = i * 4 + j;
                        char Selectedchar;
                        int operationVal = 0;

                        byte loVAl = 0;
                        byte hiVAl = 0;

                        Selectedchar = resultCode.ElementAt(i * (BinGlobalValidation.BIN_1024BIT + 1));
                        operationVal = (byte)(Selectedchar & (0x01 << j));
                        analysisDataArray[idx].pf_Data = 0 == operationVal ? (byte)TestState.MAP_STS_PASS : (byte)TestState.MAP_STS_FAIL;

                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_1024BIT + 1)) + (j << 1) + 1);
                        loVAl = (byte)(Selectedchar & 0x0F);
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_1024BIT + 1)) + (j << 1) + 2);
                        hiVAl = (byte)(Selectedchar & 0x3F);

                        operationVal = (loVAl << 6) + hiVAl;
                        analysisDataArray[idx].cat_Data = (byte)(operationVal % (0x01 << 8));
                        analysisDataArray[idx].cat_Data_Ex = (byte)(operationVal / (0x01 << 8));
                    }
                }
                retVal = true;
            }
            catch
            {
                retVal = false;
            }

            return retVal;
        }

        protected override bool CheckResultCode(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool judgmentValue = false;

            int binSize = BinGlobalValidation.BIN_8BIT + BinGlobalValidation.BIN_PF_CHAR_SIZE;
            int division = resultCode.Length / binSize;
            int remainder = resultCode.Length % binSize;
            int allCountInResultCode = 0;

            allCountInResultCode = GetAllCountInResultCode(division, remainder) / 2;

            if (allCountInResultCode == analysisDataArray.Count)
            {
                judgmentValue = true;
            }

            return judgmentValue;
        }

        private int GetAllCountInResultCode(int division, int remainder)
        {
            int allCountInResultCode = 0;

            if (remainder == 0)
            {
                allCountInResultCode = 0;
            }
            else
            {
                allCountInResultCode = remainder - 1;
            }

            allCountInResultCode += (division * BinGlobalValidation.BIN_1024BIT);

            return allCountInResultCode;
        }
    }
}
