using System;
using System.Linq;
using BinAnalyzer.Data;
using LogModule;
using ProberInterfaces.BinData;
using ProbingDataInterface;

namespace BinAnalyzer.Analzer
{
    /// <remarks> Pass/Fail Type </remarks>
    /// <summary>
    ///     문자 하나를 이용하여 Pass/Fail을 분석을 하며,
    ///     이 분석결과를 이용하여 Category Data를 결정하는 타입이다.
    /// </summary>
    public class BinAnalyzer_PassFail : BinAnalyzerBase
    {
        private BinAnalysisDataArray AnalysisDataArray;
        private int AnalysisDataArrayRemainCount = 0;

        private const string PassFailPattern = "^@ABCDEFGHIJKLMNO";

        protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = true;
            AnalysisDataArrayRemainCount = 0;
            AnalysisDataArray = analysisDataArray;

            AnalysisDataArrayRemainCount = AnalysisDataArray.Count;

            try
            {
                for (int i = 0; i < resultCode.Length; i++)
                {
                    SetAnalysisDataArray(resultCode, i);
                }

                if(AnalysisDataArrayRemainCount > 0)
                {
                    LoggerManager.Error($"[BinAnalyzer_PassFail], BinResultAnalysis() : Bin Analysis is wrong. Remain count is {AnalysisDataArrayRemainCount}, Correct value is 0.");
                }

                retVal = true;
            }
            catch
            {
                retVal = false;
            }
            finally
            {
                AnalysisDataArray = null;
                AnalysisDataArrayRemainCount = 0;
            }

            return retVal;
        }

        private void SetAnalysisDataArray(string resultCode, int i)
        {
            int resultCodeCount = 0;

            resultCodeCount = CalculateResultCodeCount();
            AnalysisDataArrayRemainCount -= resultCodeCount;

            for (int j = 0; j < resultCodeCount; j++)
            {
                int idx = i * 4 + j;
                char Selectedchar;
                byte operationVal = 0;

                if (AnalysisDataArray.Count <= idx)
                    break;

                Selectedchar = resultCode.ElementAt(i);
                operationVal = (byte)(Selectedchar & (0x01 << j));

                SetAnalysisData(operationVal, idx);
            }
        }

        private int CalculateResultCodeCount()
        {
            int resultCodeCount;

            resultCodeCount = AnalysisDataArrayRemainCount % 4;

            if (resultCodeCount == 0)
            {
                resultCodeCount = 4;
            }

            return resultCodeCount;
        }

        private void SetAnalysisData(byte operationVal, int idx)
        {
            AnalysisDataArray[idx].pf_Data = 0 == operationVal ? (byte)TestState.MAP_STS_PASS : (byte)TestState.MAP_STS_FAIL;

            //AnalysisDataArray[idx].cat_Data = AnalysisDataArray[idx].pf_Data == (byte)TestState.MAP_STS_PASS ? (byte)1 : (byte)2;
            AnalysisDataArray[idx].cat_Data = AnalysisDataArray[idx].pf_Data == (byte)TestState.MAP_STS_PASS ? (byte)2 : (byte)0;
        }

        protected override bool CheckResultCode(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            // TODO : 사용될 수 있는 문자 확인
            // Dut 개수에 맞는지 확인

            bool judgmentValue = false;
            int resultCodeLength = 0;
            int dutSize = 0;

            foreach (char c in resultCode)
            {
                judgmentValue = PassFailPattern.Contains(c);

                if (judgmentValue == false)
                {
                    break;
                }
            }

            if(judgmentValue == true)
            {
                // TODO : Dut 개수에 맞는 충분한 데이터가 입력되었는지 확인.
                // Bin Code 하나당, 4개의 DUT까지 가능
                resultCodeLength = resultCode.Length;
                dutSize = analysisDataArray.Count;

                double PossibleNum = Math.Ceiling(((double)dutSize / (double)BinGlobalValidation.BIN_6BIT));

                if (resultCodeLength <= PossibleNum)
                {
                    judgmentValue = true;
                }
                else
                {
                    judgmentValue = false;
                }

                //if (resultCodeLength == dutSize)
                //{
                //    judgmentValue = true;
                //}
                //else
                //{
                //    judgmentValue = false;
                //}
            }

            return judgmentValue;
        }
    }
}
