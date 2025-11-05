using LogModule;
using ProberInterfaces;
using System;
using System.Runtime.Serialization;

namespace LoaderParameters
{
    /// <summary>
    /// COGNEXOCR의 디바이스를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    [KnownType(typeof(IElement))]
   
    public class CognexOCRDevice : OCRDeviceBase
    {
        /// <summary>
        /// 정의하는 모듈의 타입을 가져옵니다.
        /// </summary>
        /// <returns>모듈 타입</returns>
        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.COGNEXOCR;
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            try
            {
            var shallowClone = MemberwiseClone() as CognexOCRDevice;
            return shallowClone;
                //TODO : Deep copy if ref val is exist.

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

}
