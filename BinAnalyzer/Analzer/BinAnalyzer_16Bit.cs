using BinAnalyzer.Data;
using ProberInterfaces.BinData;
using ProbingDataInterface;
using System.Linq;

namespace BinAnalyzer.Analzer
{
    /// <remarks> 16Bit Type </remarks>
    /// <summary>
    ///     
    /// </summary>
    internal class BinAnalyzer_16Bit : BinAnalyzerBase
    {
        protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = true;

            int resultCodeTmpLength = resultCode.Length;
            int division = 0;
            int remain = 0;
            division = resultCode.Length / (BinGlobalValidation.BIN_16BIT + 1);
            remain = resultCode.Length % (BinGlobalValidation.BIN_16BIT + 1);
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
                        resultCodeLoopCount = (resultCode.Length - (BinGlobalValidation.BIN_16BIT * i)) / (BinGlobalValidation.BIN_16BIT >> 2);

                        resultCodeCount = DataArrayLoopCount < resultCodeLoopCount ? DataArrayLoopCount : resultCodeLoopCount;
                    }
                    else
                    {
                        resultCodeCount = 4;
                    }

                    /*****************************************************************
                    //
                    //  Category값을 얻는 법!
                    //  ⓐ 첫번째 Bin문자를 0000 1111로 연산.
                    //  ⓑ 두번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 4칸 움직인 후
                    //     0000 0000 1111 0000로 연산.
                    //  ⓒ 세번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 8칸 움직인 후
                    //     0000 1111 0000로 0000로 연산.
                    //  ⓓ 네번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 12칸 움직인 후
                    //     1111 0000 0000로 0000로 연산.
                    //  ⓐ, ⓑ, ⓒ, ⓓ를 |로 bit연산하고,
                    //  10000으로 나눈 나머지 값을 256으로 나눈 몫을 cat_Data_Ex에
                    //  나머지는 cat_Data에 넣는다.
                    //
                    *****************************************************************/
                    for (int j = 0; j < resultCodeCount; j++)
                    {
                        int idx = i * 4 + j;
                        char Selectedchar;
                        byte operationVal = 0;

                        int Nib1st = 0;
                        int Nib2nd = 0;
                        int Nib3nd = 0;
                        int Nib4nd = 0;
                        int AllNib = 0;

                        Selectedchar = resultCode.ElementAt(i * (BinGlobalValidation.BIN_16BIT + 1));
                        operationVal = (byte)(Selectedchar & (0x01 << j));
                        analysisDataArray[idx].pf_Data = 0 == operationVal ? (byte)TestState.MAP_STS_PASS: (byte)TestState.MAP_STS_FAIL;

                        // @Aaaabbbbccccdddd@EEEEFFFFGGGGHHHH
                        // @Aaaabbbbccccdddd가 4개의 Site 셋트.
                        // i * (BinGlobalValidation.BIN_16BIT + 1)는 bin result Code의 셋트 순번에 따른 위치
                        // j * 4는 한 순번 안에 들어있는 4개의 site중 하나의 bin정보(문자 4개에 담겨 있으므로)의 위치
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_16BIT + 1)) + (j << 2) + 1);
                        Nib1st = (Selectedchar & 0xF);
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_16BIT + 1)) + (j << 2) + 2);
                        Nib2nd = (((Selectedchar & 0xF)) << 4) & 0x00F0;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_16BIT + 1)) + (j << 2) + 3);
                        Nib3nd = (((Selectedchar & 0xF)) << 8) & 0x0F00;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_16BIT + 1)) + (j << 2) + 4);
                        Nib4nd = (((Selectedchar & 0xF)) << 12) & 0xF000;

                        AllNib = Nib1st | Nib2nd | Nib3nd | Nib4nd;
                        AllNib = AllNib % BinGlobalValidation.MAX_SITE_COUNT;
                        analysisDataArray[idx].cat_Data = (byte)(AllNib % (0x01 << 8));
                        analysisDataArray[idx].cat_Data_Ex = (byte)(AllNib / (0x01 << 8));
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

            int binSize = BinGlobalValidation.BIN_16BIT + BinGlobalValidation.BIN_PF_CHAR_SIZE;
            int division = resultCode.Length / binSize;
            int remainder = resultCode.Length % binSize;
            int allCountInResultCode = 0;

            allCountInResultCode = GetAllCountInResultCode(division, remainder) / 4;

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

            allCountInResultCode += (division * BinGlobalValidation.BIN_16BIT);

            return allCountInResultCode;
        }
    }
}
