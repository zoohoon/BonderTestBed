using BinAnalyzer.Data;
using ProberInterfaces.BinData;
using ProbingDataInterface;
using System.Linq;

namespace BinAnalyzer.Analzer
{
    /// <remarks> 6Bit Type </remarks>
    /// <summary>
    ///     
    /// </summary>
    internal class BinAnalyzer_6Bit : BinAnalyzerBase
    {
        protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = true;

            try
            {
                for (int i = 0; i < ((resultCode.Length - 1) / (BinGlobalValidation.BIN_6BIT + 1)) + 1; i++)
                {
                    int resultCodeCount = 0;

                    /*****************************************************************
                    //
                    //  6Bit Site에서는 4+1개씩 끊어 분석 한다.
                    //  그 이유는 Pass/Fail(1개) + Bin 문자 갯수(4개)이기 때문이다.
                    //
                    *****************************************************************/
                    if (i == ((resultCode.Length - 1) / (BinGlobalValidation.BIN_6BIT + 1)))
                    {
                        int remainder = resultCode.Length % (BinGlobalValidation.BIN_6BIT + 1);
                        if (remainder == 0)
                        {
                            resultCodeCount = (BinGlobalValidation.BIN_6BIT + 1);
                        }
                        else
                        {
                            resultCodeCount = (resultCode.Length % (BinGlobalValidation.BIN_6BIT + 1));
                        }
                    }
                    else
                    {
                        resultCodeCount = (BinGlobalValidation.BIN_6BIT + 1);
                    }

                    /*****************************************************************
                    //
                    //  Category값을 얻는 법!
                    //  Bin값과 0111 1111로 & 연산을 하여 얻는다.
                    //
                    *****************************************************************/
                    for (int j = 1; j < resultCodeCount; j++)
                    {
                        int idx = i * 4 + (j - 1);
                        char Selectedchar;
                        byte operationVal = 0;

                        Selectedchar = resultCode.ElementAt(i * (BinGlobalValidation.BIN_6BIT + 1));
                        operationVal = (byte)(Selectedchar & (0x01 << j - 1));
                        analysisDataArray[idx].pf_Data = 0 == operationVal ? (byte)TestState.MAP_STS_PASS : (byte)TestState.MAP_STS_FAIL;

                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_6BIT + 1)) + j);
                        operationVal = (byte)(Selectedchar & 0x3F);
                        analysisDataArray[idx].cat_Data = operationVal;
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

            int binSize = BinGlobalValidation.BIN_6BIT + BinGlobalValidation.BIN_PF_CHAR_SIZE;
            int division = resultCode.Length / binSize;
            int remainder = resultCode.Length % binSize;
            int allCountInResultCode = 0;

            allCountInResultCode = GetAllCountInResultCode(division, remainder);

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

            allCountInResultCode += (division * BinGlobalValidation.BIN_6BIT);

            return allCountInResultCode;
        }
    }

    //internal class BinAnalyzer_6Bit : BinAnalyzerBase
    //{
    //    private int AnalysisDataArrayRemainCount = 0;

    //    protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
    //    {
    //        bool retVal = true;
    //        AnalysisDataArrayRemainCount = analysisDataArray.Count;

    //        try
    //        {
    //            for (int i = 0; i < ((resultCode.Length - 1) / (BinGlobalValidation.BIN_6BIT + 1)) + 1; i++)
    //            {
    //                int resultCodeCount = 0;

    //                /*****************************************************************
    //                //
    //                //  6Bit Site에서는 4+1개씩 끊어 분석 한다.
    //                //  그 이유는 Pass/Fail(1개) + Bin 문자 갯수(4개)이기 때문이다.
    //                //
    //                *****************************************************************/
    //                resultCodeCount = CalculateResultCodeCount();
    //                AnalysisDataArrayRemainCount -= resultCodeCount;

    //                /*****************************************************************
    //                //
    //                //  Category값을 얻는 법!
    //                //  Bin값과 0111 1111로 & 연산을 하여 얻는다.
    //                //
    //                *****************************************************************/
    //                for (int j = 1; j < resultCodeCount; j++)
    //                {
    //                    int idx = i * 4 + (j - 1);
    //                    char Selectedchar;
    //                    byte operationVal = 0;

    //                    Selectedchar = resultCode.ElementAt(i * (BinGlobalValidation.BIN_6BIT + 1));
    //                    operationVal = (byte)(Selectedchar & (0x01 << j - 1));
    //                    analysisDataArray[idx].pf_Data = 0 < operationVal ? (byte)0 : (byte)1;

    //                    Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_6BIT + 1)) + j);
    //                    operationVal = (byte)(Selectedchar & 0x3F);
    //                    analysisDataArray[idx].cat_Data = operationVal;
    //                }
    //            }
    //            retVal = true;
    //        }
    //        catch
    //        {
    //            retVal = false;
    //        }

    //        return retVal;
    //    }

    //    private int CalculateResultCodeCount()
    //    {
    //        int resultCodeCount;

    //        resultCodeCount = AnalysisDataArrayRemainCount % (BinGlobalValidation.BIN_6BIT + 1);

    //        if (resultCodeCount == 0)
    //        {
    //            resultCodeCount = (BinGlobalValidation.BIN_6BIT + 1);
    //        }

    //        return resultCodeCount;
    //    }
    //}
}
