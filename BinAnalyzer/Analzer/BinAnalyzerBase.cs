using BinAnalyzer.Data;
using ProberInterfaces.BinData;

namespace BinAnalyzer.Analzer
{
    public abstract class BinAnalyzerBase
    {
        #region <remarks> Property                  </remarks>
        private HardBinData _HardBinData;
        public HardBinData HardBinData
        {
            get { return _HardBinData; }
            set { _HardBinData = value; }
        }
        #endregion

        #region <remarks> GetTestResultAnalysis     </remarks>
        /// <summary>
        ///     테스트 결과를 분석하는 메소드
        /// </summary>
        ///     <param name="prefixChar">
        ///         접두사(M, C)
        ///     </param>
        ///     <param name="testResultCode">
        ///         테스트 결과 코드
        ///     </param>
        ///     <param name="siteNum">
        ///         사이트 갯수
        ///     </param>
        ///     <param name="analysisDataArray">
        ///         분석 결과를 반환할 객체
        ///     </param>
        /// <returns>
        ///     분석 성공 유무.
        /// </returns>
        public bool GetTestResultAnalysis(string prefixChar, string testResultCode, int siteNum, ref BinAnalysisDataArray analysisDataArray)
        {
            bool result = false;
            string pureResultCode = null;

            if (string.IsNullOrEmpty(testResultCode) == true || siteNum < 1)
                return false;

            pureResultCode = RemoveCharInStr(testResultCode, prefixChar, BinGlobalValidation.ZERO);

            result = InitBinAnalysisDataArray(siteNum, testResultCode, ref analysisDataArray);

            result = CheckResultCode(pureResultCode, analysisDataArray);

            if (result == true)
            {
                result = BinResultAnalysis(pureResultCode, analysisDataArray);
                analysisDataArray.Valid = true;
            }
            else
            {
                analysisDataArray.Valid = false;
            }

            return result;
        }
        #endregion

        #region <remarks> InitBinAnalysisDataArray  </remarks>
        /// <summary>
        ///     site 갯수에 따라 analysisDataArray 내부 배열을 셋팅하는 메소드.1
        /// </summary>
        ///     <param name="siteNum">
        ///         site 갯수.
        ///     </param>
        /// <returns>
        ///     셋팅 성공 유무에 따른 bool 결과 값.
        /// </returns>
        protected bool InitBinAnalysisDataArray(int siteNum, string binData, ref BinAnalysisDataArray analysisDataArray)
        {
            bool result = false;

            analysisDataArray = new BinAnalysisDataArray(siteNum, binData);
            result = true;

            return result;
        }
        #endregion

        #region <remarks> RemoveCharInStr           </remarks>
        /// <summary>
        ///     문자열에서 특정 위치의 문자를 제거하는 메소드.
        /// </summary>
        ///     <param name="testResultCode">
        ///         Tester로부터 받은 Result Code.
        ///     </param>
        ///     <param name="removeChar">
        ///         지우고 싶은 문자. ex) M, C
        ///     </param>
        ///     <param name="idx">
        ///         지우고 싶은 인덱스.
        ///     </param>
        /// <returns>
        ///     특정 문자를 지운 뒤의 문자열.
        /// </returns>
        private string RemoveCharInStr(string testResultCode, string removeChar, int idx)
        {
            string retStr = null;
            string targetCode = testResultCode.Substring(idx, removeChar.Length);

            if (targetCode == removeChar)
            {
                retStr = testResultCode.Remove(0, removeChar.Length);
            }
            else
            {
                retStr = testResultCode;
            }

            return retStr;
        }
        #endregion

        #region <remarks> Abstract                  </remakrs>
        /// <summary>
        ///     Bin 결과 값을 분석하는 가상 메소드.
        /// </summary>
        ///     <param name="resultCode">
        ///         Bin 결과 값.
        ///     </param>
        /// <returns>
        ///     분석 성공 유무에 따른 bool 결과 값.
        /// </returns>
        protected abstract bool BinResultAnalysis(string resultCode, BinAnalysisDataArray analysisDataArray);

        /// <summary>
        ///     ResultCode의 유효성을 검사하는 메소드.
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="analysisDataArray"></param>
        /// <returns></returns>
        protected abstract bool CheckResultCode(string resultCode, BinAnalysisDataArray analysisDataArray);
        #endregion
    }
}
