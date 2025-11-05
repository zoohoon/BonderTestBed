using ProberInterfaces.Param;

namespace ProberInterfaces
{
    public interface ICoordinateConvert<T> 
    {
        T Convert(MachineCoordinate machinecoord);
        MachineCoordinate ConvertBack(T coord);
        T CurrentPosConvert();
        
    }
}
