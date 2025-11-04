using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.FDAlign
{
    public enum FDWaferAlignInnerStateEnum
    {
        IDLE = 0,
        SUSPENDED,
        ALIGN,
        READY,
        ERROR,
        SETUP,
        RECOVERY,
        RECOVERING,
        FAILED,
        PAUSED,
        DONE,
        ABORT
    }
}
