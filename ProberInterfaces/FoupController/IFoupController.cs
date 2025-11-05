using System;
using ProberErrorCode;
using ProberInterfaces.Enum;

namespace ProberInterfaces.Foup
{
    public enum FoupControllerStateEnum
    {
        UNDEFINED,
        IDLE,
        RUNNING,
        SUSPENDED,
        DONE,
    }

    public interface IWaferInfo
    {
        EnumWaferSize WaferSize { get; set; }
        double Notchangle { get; set; }
        double WaferThickness { get; set; }
        OCRTypeEnum OCRType { get; set; }
        OCRDirectionEnum OCRDirection { get; set; }
        OCRModeEnum OCRMode { get; set; }

    }

    public interface IFoupController : IFactoryModule, IParamNode
    {
        FoupModuleInfo FoupModuleInfo { get; }
        int FoupNumber { get; set; }
        void OnChangedFoupInfoFunc(object sender, EventArgs e);
        EventCodeEnum Execute(FoupCommandBase command);

        EventCodeEnum MonitorForWaferOutSensor(bool value);

        void SetLoaderState(ModuleStateEnum state);

        void SetLotState(ModuleStateEnum state);

        FoupModuleInfo GetFoupModuleInfo();

        EventCodeEnum InitController(int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam, CassetteConfigurationParameter CassetteConfigurationParam);

        void DeInit();

        FoupIOMappings GetFoupIOMap();

        void RecoveryIfErrorOccurred();

        ICylinderManager GetFoupCylinderManager();
        IFoupProcedureManager GetFoupProcedureManager();
        void ChangeState(FoupStateEnum state);
        EventCodeEnum FoupStateInit();
        EventCodeEnum InitProcedures();

        IFoupService GetFoupService();
        IFoupService Service { get; set; }
        void ChangeFoupServiceStatus(GEMFoupStateEnum state);
        EventCodeEnum SetLock(bool isLock);
        EventCodeEnum CassetteTypeAvailable(CassetteTypeEnum CassetteType);
        CassetteTypeEnum GetCassetteType();
        EventCodeEnum ValidationCassetteAvailable(out string msg, CassetteTypeEnum cassetteType = CassetteTypeEnum.UNDEFINED);
    }
}
