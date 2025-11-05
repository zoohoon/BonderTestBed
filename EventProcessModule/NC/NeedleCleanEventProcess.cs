using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessModule.NC
{
    [Serializable]
    public abstract class NeedleCleanEventProcessBase : EventProcessBase
    {
    }

    [Serializable]
    public class NCEventProc_LotStart : NeedleCleanEventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
   
        }
    }
}
