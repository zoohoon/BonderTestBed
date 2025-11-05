using System.Collections.Generic;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;

namespace LoaderBase
{
    /// <summary>
    /// Loader Attached Module을 관리하는 모듈을 정의합니다.
    /// </summary>
    public interface IModuleManager : ILoaderFactoryModule
    {
        /// <summary>
        /// 모든 모듈을 재정의 합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum UpdateDefinitionParameters();

        /// <summary>
        /// 모든 모듈의 디바이스를 재 설정합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum UpdateDeviceParameters();

        /// <summary>
        /// 모든 모듈을 초기화 합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum InitAttachModules(bool IsRefresh=false);

        /// <summary>
        /// Wafer와 연관된 모듈의 정보를 초기화합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ResetWaferLocation();
        
        /// <summary>
        /// ID로 모듈을 검색합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IAttachedModule FindModule(ModuleID id);

        /// <summary>
        /// 모듈타입과 인덱스로 모듈을 검색합니다.
        /// </summary>
        /// <param name="moduleType"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        IAttachedModule FindModule(ModuleTypeEnum moduleType, int index);

        /// <summary>
        /// ID로 모듈을 검색합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T FindModule<T>(ModuleID id) where T : class, IAttachedModule;

        /// <summary>
        /// 모듈타입과 인덱스로 모듈을 검색합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="moduleType"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        T FindModule<T>(ModuleTypeEnum moduleType, int index) where T : class, IAttachedModule;

        /// <summary>
        /// Label로 모듈을 검색합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <returns></returns>
        T FindModule<T>(string label) where T : class, IAttachedModule;

        /// <summary>
        /// 타입과 일치하는 모듈의 리스트를 검색합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> FindModules<T>() where T : class, IAttachedModule;

        /// <summary>
        /// 카세트가 소유하고 있는 슬롯모듈을 검색합니다.
        /// </summary>
        /// <param name="cassette"></param>
        /// <returns></returns>
        List<ISlotModule> FindSlots(ICassetteModule cassette);
        
        /// <summary>
        /// 사용가능한 ARM을 검색합니다.
        /// </summary>
        /// <param name="useType"></param>
        /// <param name="excludeARMs"></param>
        /// <returns></returns>
        IARMModule FindUsableARM(ARMUseTypeEnum useType, params IARMModule[] excludeARMs);

        /// <summary>
        /// 사용가능한 IWaferOwnable 모듈을 검색합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T FindUsableModule<T>() where T : class, IWaferOwnable;

        /// <summary>
        /// 사용가능한 IPreAlignable을 검색합니다.
        /// </summary>
        /// <param name="transferObject"></param>
        /// <returns></returns>
        IPreAlignable FindUsablePreAlignable(TransferObject transferObject);

        /// <summary>
        /// 사용가능한 IWaferOwnable을 검색합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T FindUsableModule<T>(ModuleID id) where T : class, IWaferOwnable;

        #region Find SubstrateObject Methods
        /// <summary>
        /// 몯
        /// </summary>
        /// <returns></returns>
        List<TransferObject> GetTransferObjectAll();
        List<TransferObject> GetUnknownTransferObjectAll();
        List<TransferObject> GetCardObjectAll();
        TransferObject FindTransferObject(string objGuid);
        EventCodeEnum SetCstDevice(int cstIndex, string deviceName, double loadingAngle, double unloadingAngle, SubstrateSizeEnum size, WaferNotchTypeEnum notchType, OCRDevParameter ocrDev, EnumWaferType type=EnumWaferType.STANDARD);
        #endregion
    }
}
