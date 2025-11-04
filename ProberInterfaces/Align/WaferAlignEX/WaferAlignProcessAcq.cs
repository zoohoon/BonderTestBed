using ProberInterfaces.AlignEX;

namespace ProberInterfaces.WaferAlignEX
{
    public abstract class WaferAlignProcessAcq : AlignProcessAcqBase
    {
        public abstract WaferAlignProcAcqEnum GetAcqType();
    }
}
