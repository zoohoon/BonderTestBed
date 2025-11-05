using System;
using System.Runtime.Serialization;

namespace LoaderParameters
{
    /// <summary>
    /// LoaderServiceType 을 정의합니다.
    /// </summary>
    [DataContract]
    public enum LoaderServiceTypeEnum
    {
        /// <summary>
        /// 동적으로 어셈블리를 링크합니다.
        /// </summary>
        [EnumMember]
        DynamicLinking,
        /// <summary>
        /// WCF를 사용합니다.
        /// </summary>
        [EnumMember]
        WCF,
        /// <summary>
        /// REMOTE Controller(ADS)를 사용합니다.
        /// </summary>
        [EnumMember]
        REMOTE,
    }
    
    /// <summary>
    /// 응답 결과를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ResponseResult
    {
        /// <summary>
        /// 요청이 성공하였는 지 여부를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public bool IsSucceed { get; set; }

        /// <summary>
        /// 요청이 실패하였을 때 사유를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
