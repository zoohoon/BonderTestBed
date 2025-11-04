using BinAnalyzer.Data;
using ProberInterfaces.BinData;
using ProbingDataInterface;
using System.Linq;

namespace BinAnalyzer.Analzer
{
    /// <remarks> 32Bit Type </remarks>
    /// <summary>
    ///     
    /// </summary>
    internal class BinAnalyzer_32Bit : BinAnalyzerBase
    {
        protected override bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = true;

            int resultCodeTmpLength = resultCode.Length;
            int division = 0;
            int remain = 0;
            division = resultCode.Length / (BinGlobalValidation.BIN_32BIT + 1);
            remain = resultCode.Length % (BinGlobalValidation.BIN_32BIT + 1);
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
                        resultCodeLoopCount = (resultCode.Length - (BinGlobalValidation.BIN_32BIT * i)) / (BinGlobalValidation.BIN_32BIT >> 2);

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
                    //     10000로 나눈 나머지.
                    //  ⓒ 세번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 8칸 움직인 후
                    //     10000로 나눈 나머지.
                    //  ⓓ 네번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 12칸 움직인 후
                    //     10000로 나눈 나머지.
                    //  ⓔ 다섯번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 16칸 움직인 후
                    //     10000로 나눈 나머지.
                    //  ⓕ 여섯번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 20칸 움직인 후
                    //     10000로 나눈 나머지.
                    //  ⓖ 일곱번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 24칸 움직인 후
                    //     10000로 나눈 나머지.
                    //  ⓗ 여덟번째 Bin문자를 0000 1111로 연산 후, 왼쪽으로 28칸 움직인 후
                    //     10000로 나눈 나머지.
                    //
                    //  ⓐ, ⓑ, ⓒ, ⓓ, ⓔ, ⓕ, ⓖ, ⓗ를 더하고
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
                        int Nib5nd = 0;
                        int Nib6nd = 0;
                        int Nib7nd = 0;
                        int Nib8nd = 0;

                        int AllNib = 0;

                        Selectedchar = resultCode.ElementAt(i * (BinGlobalValidation.BIN_32BIT + 1));
                        operationVal = (byte)(Selectedchar & (0x01 << j));
                        analysisDataArray[idx].pf_Data = 0 == operationVal ? (byte)TestState.MAP_STS_PASS : (byte)TestState.MAP_STS_FAIL;

                        // @AaaaAaaabbbbbbbbccccccccdddddddd@EEEEEEEEFFFFFFFFGGGGGGGGHHHHHHHH
                        // @AaaaAaaabbbbbbbbccccccccdddddddd가 4개의 Site 셋트.
                        // i * (BinGlobalValidation.BIN_32BIT + 1)는 bin result Code의 셋트 순번에 따른 위치
                        // j * 8은 한 순번 안에 들어있는 4개의 site중 하나의 bin정보(문자 8개에 담겨 있으므로)의 위치
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 1);
                        Nib1st = (Selectedchar & 0xF);
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 2);
                        Nib2nd = ((Selectedchar & 0xF) << 4) % BinGlobalValidation.MAX_SITE_COUNT;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 3);
                        Nib3nd = ((Selectedchar & 0xF) << 8) % BinGlobalValidation.MAX_SITE_COUNT;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 4);
                        Nib4nd = ((Selectedchar & 0xF) << 12) % BinGlobalValidation.MAX_SITE_COUNT;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 5);
                        Nib5nd = ((Selectedchar & 0xF) << 16) % BinGlobalValidation.MAX_SITE_COUNT;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 6);
                        Nib6nd = ((Selectedchar & 0xF) << 20) % BinGlobalValidation.MAX_SITE_COUNT;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 7);
                        Nib7nd = ((Selectedchar & 0xF) << 24) % BinGlobalValidation.MAX_SITE_COUNT;
                        Selectedchar = resultCode.ElementAt((i * (BinGlobalValidation.BIN_32BIT + 1)) + (j << 3) + 8);
                        Nib8nd = ((Selectedchar & 0xF) << 28) % BinGlobalValidation.MAX_SITE_COUNT;

                        AllNib = Nib1st + Nib2nd + Nib3nd + Nib4nd + Nib5nd + Nib6nd + Nib7nd + Nib8nd;
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

            int binSize = BinGlobalValidation.BIN_32BIT + BinGlobalValidation.BIN_PF_CHAR_SIZE;
            int division = resultCode.Length / binSize;
            int remainder = resultCode.Length % binSize;
            int allCountInResultCode = 0;

            allCountInResultCode = GetAllCountInResultCode(division, remainder) / 8;

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

            allCountInResultCode += (division * BinGlobalValidation.BIN_32BIT);

            return allCountInResultCode;
        }
    }
}
