using System;

using ProberInterfaces;
using System.Runtime.Serialization;
using LogModule;

namespace LoaderParameters
{
    /// <summary>
    /// COGNEXOCR의 특성을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class CognexOCRDefinition : OCRDefinitionBase
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
                var shallowClone = MemberwiseClone() as CognexOCRDefinition;
                shallowClone.AccessParams = AccessParams.CloneFrom();
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
