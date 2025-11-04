using BinAnalyzer.Data;
using ProberInterfaces.BinData;
using ProbingDataInterface;
using System.Linq;
using System.Text;

namespace BinAnalyzer.Analzer
{
    internal class BinAnalyzer_U32_64Bit : BinAnalyzerBase
    {
        protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = true;                 //리턴 값
            StringBuilder sbResultCode = null;  //하나의 bin 정보의 string을 담기 위한 변수.
            int dataArrayCount = 0;             //현재 가리키고 있는 Array 순번.
            char pfChar = ' ';                  //pass fail 문자 변수.

            sbResultCode = new StringBuilder();

            try
            {
                for (int i = 0; i < resultCode.Length; i++)
                {
                    if (pfChar == ' ')
                    {
                        pfChar = resultCode.ElementAt(i);
                    }
                    else
                    {
                        char CharElement = resultCode.ElementAt(i);

                        sbResultCode.Append(CharElement);

                        if (BinGlobalValidation.BIN_U32_64BIT == sbResultCode.Length)
                        {
                            string resultCodeUnit = null;
                            resultCodeUnit = sbResultCode.ToString();
                            sbResultCode.Clear();

                            ////////////////////////////////////////////////////////////////////
                            byte operationVal = (byte)(pfChar & (0x01 << dataArrayCount % 4));
                            analysisDataArray[dataArrayCount].pf_Data = 0 == operationVal ? (byte)TestState.MAP_STS_PASS : (byte)TestState.MAP_STS_FAIL;
                            ////////////////////////////////////////////////////////////////////

                            if (dataArrayCount != 0 && dataArrayCount % 4 == 3)
                            {
                                pfChar = ' ';
                            }
                            else
                            {
                            }

                            //////////////////////////////////
                            //Todo : parameter로부터 sbResultCode를 찾는 코드를 넣어야 한다.
                            byte findData = 0;
                            if (this.HardBinData.UserBinDictionary.ContainsKey(resultCodeUnit) == false)
                            {
                                HardBinData.UserBinDictionary.Add(resultCodeUnit, 0);
                            }
                            else
                            {

                            }

                            findData = this.HardBinData.UserBinDictionary[resultCodeUnit];
                            //////////////////////////////////

                            analysisDataArray[dataArrayCount].cat_Data = (byte)(findData % (0x1 << 8));

                            dataArrayCount++;
                        }
                        else
                        {
                        }

                        if (analysisDataArray.Count <= dataArrayCount)
                        {
                            break;
                        }
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
            bool judgmentValue = true;

            return judgmentValue;
        }
    }
}
