using ProberInterfaces.Align;

namespace ProberInterfaces.WaferAlign
{
    public enum HeightMappingStateEnum
    {
        UNKNOWN = 0,
        NOT_PERFORMED,
        POINT_UPDATE,
        MAPPING_FAILED,
        OVER_TOL,
    }
    public abstract class HeightMappingStateBase :     AlignProcessStateBase
    {
        public HeightMappingStateBase(AlignProcessBase module) : base(module)
        {

        }

        public abstract HeightMappingStateEnum GetProfilingState();
    }
}
