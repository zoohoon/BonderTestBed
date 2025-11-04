using LogModule;
using System;
using System.Runtime.Serialization;

namespace LoaderParameters
{
    /// <summary>
    /// UExtensionNone 을 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class UExtensionNone : UExtensionBase
    {
        /// <summary>
        /// 인스턴스를 생성합니다.
        /// </summary>
        public UExtensionNone()
        {
            UExtensionType.Value = UExtensionTypeEnum.NONE;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone();
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

}
