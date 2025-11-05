using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SorterSystem.VM
{
    public interface ISorterModuleVM
    {
        IDisplayPort DisplayPort { get; set; }

        AxisobjectVM AxisX { get; set; }
        AxisobjectVM AxisY { get; set; }
        AxisobjectVM AxisZ { get; set; }
        AxisobjectVM AxisT { get; set; }
        AxisobjectVM AxisPZ { get; set; }
        EnumProberCam CurrCam { get; set; }

        Task AbsMove();

        ICamera WLCam { get; set; }
        ICamera WHCam { get; set; }
        ICamera PLCam { get; set; }
        ICamera PHCam { get; set; }
    }
}
