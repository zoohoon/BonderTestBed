using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using System.Windows;
    using ProberInterfaces.Param;
    using ProberInterfaces.Proxies;

    public interface IWaferAlignerProxy: IFactoryModule, IProberProxy
    {
        new void InitService();
        bool GetIsRecovery();
        WaferCoordinate MachineIndexConvertToDieLeftCorner(long xindex, long yindex);
        WaferCoordinate MachineIndexConvertToDieLeftCorenr_NonCalcZ(long xindex, long yindex);
        WaferCoordinate MachineIndexConvertToDieCenter(long xindex, long yindex);
        WaferCoordinate MachineIndexConvertToProbingCoord(long xindex, long yindex);
        Point GetLeftCornerPositionForWAFCoord(WaferCoordinate position);
        EventCodeEnum ClearState();
        void SetSetupState();
        void SetIsNewSetup(bool flag);
        void SetIsModifySetup(bool flag);
        Task<bool> CheckPossibleSetup(bool isrecovery = false);
        EventCodeEnum EdgeCheck(ref WaferCoordinate centeroffset, ref double maximum_value_X, ref double maximum_value_Y);
        ModuleStateEnum GetModuleState();
        ModuleStateEnum GetPreModuleState();
        (double, double) GetVerifyCenterLimitXYValue();
        void SetVerifyCenterLimitXYValue(double xLimit, double yLimit);

    }
}
