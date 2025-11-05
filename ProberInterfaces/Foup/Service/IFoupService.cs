
using ProberErrorCode;


namespace ProberInterfaces.Foup
{
    public interface IFoupService : IFactoryModule, IParamNode
    {
        EventCodeEnum Connect();

        EventCodeEnum Disconnect();

        EventCodeEnum InitModule(int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam, CassetteConfigurationParameter CassetteConfigurationParam);

        EventCodeEnum Deinit();

        FoupModuleInfo GetFoupModuleInfo();

        EventCodeEnum SetCommand(FoupCommandBase command);

        void SetLoaderState(ModuleStateEnum state);
        
        void SetLotState(ModuleStateEnum state);
        ICylinderManager GetFoupCylinderManager();
        IFoupProcedureManager GetFoupProcedureManager();

        FoupIOMappings GetFoupIOMap();
        void ChangeState(FoupStateEnum state);
        EventCodeEnum MonitorForWaferOutSensor(bool value);
        EventCodeEnum FoupStateInit();
        EventCodeEnum InitProcedures();
        IFoupModule FoupModule { get; }
        EventCodeEnum SetLock(bool isLock);
        EventCodeEnum CassetteTypeAvailable(CassetteTypeEnum cassetteType);
        CassetteTypeEnum GetCassetteType();
        EventCodeEnum ValidationCassetteAvailable(out string msg , CassetteTypeEnum cassetteType = CassetteTypeEnum.UNDEFINED);
        EventCodeEnum SetDevice(FoupDeviceParam devParam);
    }
    
    public interface IDirectFoupService : IFoupService, IParamNode
    {
        void SetCallback(IFoupServiceCallback callback, FoupServiceTypeEnum servtype);
    }
}
