using ProberInterfaces.Param;

namespace ProberInterfaces.Proxies
{
    public interface ICoordinateManagerProxy : IFactoryModule, IProberProxy
    {
        void StageCoordConvertToChuckCoord();
        void StopStageCoordConvertToChuckCoord();
        CatCoordinates StageCoordConvertToUserCoord(EnumProberCam camtype);
        UserIndex GetCurUserIndex(CatCoordinates Pos);
        MachineIndex UserIndexConvertToMachineIndex(UserIndex UI);
        UserIndex MachineIndexConvertToUserIndex(MachineIndex MI);
        MachineIndex WUIndexConvertWMIndex(long uindexX, long uindexY);
        MachineIndex GetCurMachineIndex(WaferCoordinate Pos);
        UserIndex WMIndexConvertWUIndex(long mindexX, long mindexY);
        CatCoordinates PmResultConverToUserCoord(PMResult pmresult);
        MachineCoordinate RelPosToAbsPos(MachineCoordinate RelPos);
    }
}
