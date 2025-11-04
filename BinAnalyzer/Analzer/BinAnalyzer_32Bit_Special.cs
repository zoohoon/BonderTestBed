using BinAnalyzer.Data;
using ProberInterfaces.BinData;
using ProbingDataInterface;
using System;
using System.Linq;
using System.Text;

namespace BinAnalyzer.Analzer
{
    /// <remarks> 32Bit Special Type </remarks>
    /// <summary>
    ///     2의 지수를 Category 값으로 가진다.
    ///     예로들어, @ @D@@@@@@ @@B@@@@@의 경우
    ///                 0000 0100 0000 0000  0000 0000 0000 0000 => 6
    ///                 0000 0000 0010 0000  0000 0000 0000 0000 => 9
    ///     읽는순서는   3,2,1,0 | 7,6,5,4, | 11,10,9,8 | 15,14,13,12 | 19,18,17,16 | 23,22,21,20 | 27,26,25,24 | 31,30,29,28
    ///     이다.
    ///     
    ///     하나의 bin정보는 다수의 @와 하나의 문자(A, B, D, H)로만 이루어져 있다.
    ///     이때, 8자리 이내로 문자가 있는경우 8개의 문자를 다 읽고, 다음 bin 정보를 읽는다.
    ///     만약, 8자리 이내에 문자가 없고 @로만 이루어진 경우, 문자를 찾을때까지 bin 정보가 이어진다.
    ///     문자를 찾는다면 그 자리에서 바로 다음 bin 정보를 읽는다.
    ///     
    ///      - 예시  (슬래쉬는 하나의 bin단위를 표시)
    ///     예1) @@@@@@A@@B@@...의 경우 A를 읽은 뒤, 1개의 @를 더 읽고 다음 Bin을 읽는다. => @@@@@@A@/@B@@ 
    ///     예2) @@@A@@@@@B@@...의 경우 A를 읽은 뒤, 4개의 @를 더 읽고 다음 Bin을 읽는다. => @@@A@@@@/@B@@
    ///     예3) @@@@@@@@@@@@@@@@@@A@@@@@...의 경우 A를 읽은 뒤,  다음 Bin을 읽는다. => @@@@@@@@@@@@@@@@@@A/@@@@@...
    ///     예4) @@@@@@@@@@@@@@@@@@@@@@AB...의 경우 A를 읽은 뒤,  다음 Bin을 읽는다. => @@@@@@@@@@@@@@@@@@@@@@A/B...
    ///     
    ///     - sample -
    ///     CC@@@@@@@@@@@@@@@@@@@@@@@@@@@@H@@A@@@@@ 115,8
    ///     CC@@A@@@@@@@A@@@@@ 8, 8
    ///     C@@@@@@@@@B@@@@@@@ 0, 1
    ///     CC@@D@@@@@@@@@@B@@ 10, 21
    ///     C@B@@@@@@@B@@@@@@@ 1, 1
    ///     CBB@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@H 1, 111
    ///     CC@@@@@A@@@@D@@@@@ 20, 10
    ///     CBB@@@@@@@@@@@@B@@ 1, 21
    ///     CA@@@@@B@@B@@@@@@@ 21, 1
    ///     CA@@@@@@@@@@@@@@@@@@@@@@@@@@@@HB@@@@@@@ 115, 1
    ///     CA@@@@@@@@@@@@@@@@@@@@@BB@@@@@@@ 85, 1
    ///     CBB@@@@@@@@@@@@@@@@@@@@@@@@@@@B 1,81
    ///     CA@@@@@@@@@@@@@@@@@@@@BB@@@@@@@ 81,1
    ///     CC@@@@@@@@@@@@B@@@@@@@@@@@@B 49,49
    /// </summary>
    internal class BinAnalyzer_32Bit_Special : BinAnalyzerBase
    {
        private enum AnalysisMode
        {
            ANALYSIS,
            CHECK
        }

        bool haveChar = false;
        StringBuilder sbResultCode;
        int dataArrayCount = 0;
        char pfChar = ' ';
        string ResultCode = null;
        BinAnalysisDataArray AnalysisDataArray = null;


        protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = true;

            InitField(resultCode, analysisDataArray);

            try
            {
                for (int i = 0; i < resultCode.Length; i++)
                {
                    ResultAnalysis(i);
                }
                retVal = true;
            }
            catch
            {
                retVal = false;
            }

            return retVal;
        }

        private void InitField(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            haveChar = false;
            sbResultCode = new StringBuilder();
            dataArrayCount = 0;
            pfChar = ' ';
            this.ResultCode = resultCode;
            this.AnalysisDataArray = analysisDataArray;
        }

        private void ResultAnalysis(int idx, AnalysisMode analysisMode = AnalysisMode.ANALYSIS)
        {
            if (pfChar == ' ')
            {
                pfChar = ResultCode.ElementAt(idx);
            }
            else
            {
                AppendToResultCodeElement(idx);

                if (haveChar == true)
                {
                    ProcessResultCode(analysisMode);
                }
            }
        }

        private void AppendToResultCodeElement(int idx)
        {
            char CharElement = ResultCode.ElementAt(idx);

            if ('@' < CharElement)
            {
                haveChar = true;
            }

            sbResultCode.Append(CharElement);
        }

        private void ProcessResultCode(AnalysisMode analysisMode)
        {
            if (BinGlobalValidation.BIN_32BIT_SPECIAL_MINIMUM_SIZE <= sbResultCode.Length)
            {
                string resultCodeUnit = sbResultCode.ToString();
                byte operationVal = 0;

                haveChar = false;
                sbResultCode.Clear();
                operationVal = (byte)(pfChar & (0x01 << (dataArrayCount % 4)));

                if (dataArrayCount != 0 && dataArrayCount % 4 == 3)
                {
                    pfChar = ' ';
                }

                if (analysisMode == AnalysisMode.ANALYSIS)
                {
                    SetPassFailDataInAnalysisData(operationVal);
                    SetCategoryDataInAnalysisData(resultCodeUnit, operationVal);
                }

                dataArrayCount++;
            }
        }

        private void SetPassFailDataInAnalysisData(byte operationVal)
        {
            AnalysisDataArray[dataArrayCount].pf_Data = 0 == operationVal ? (byte)TestState.MAP_STS_PASS: (byte)TestState.MAP_STS_FAIL;
        }

        private void SetCategoryDataInAnalysisData(string resultCodeUnit, byte operationVal)
        {
            for (int j = 0; j < resultCodeUnit.Length; j++)
            {
                char choiceChar = resultCodeUnit.ElementAt(j);
                if ('@' < choiceChar)
                {
                    operationVal = (byte)(choiceChar & 0x0F);

                    operationVal = (byte)Math.Log(operationVal, 2);
                    AnalysisDataArray[dataArrayCount].cat_Data = (byte)((j << 2) + operationVal);
                    break;
                }
            }
        }

        protected override bool CheckResultCode(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool judgmentValue = false;

            InitField(resultCode, analysisDataArray);

            try
            {
                for (int i = 0; i < resultCode.Length; i++)
                {
                    ResultAnalysis(i, AnalysisMode.CHECK);
                }

                if (dataArrayCount == analysisDataArray.Count
                    && sbResultCode.ToString().Length == 0)
                {
                    judgmentValue = true;
                }
            }
            catch
            {
                judgmentValue = false;
            }

            return judgmentValue;
        }
    }
}
