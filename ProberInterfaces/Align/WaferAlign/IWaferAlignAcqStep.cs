using ProberInterfaces.AlignEX;
using ProberInterfaces.WaferAlignEX;
using System;
using System.Collections.ObjectModel;

namespace ProberInterfaces.WaferAlign
{
    public interface IWaferAlignAcqStep
    {
        ObservableCollection<WaferAlignProcessAcq> Acquisitions { get; set; }
    }
    
    [Serializable]
    public abstract class WaferAlignAcqStepBase : AlignAcqStepBase
    {

        //public abstract ObservableCollection<WaferAlignProcessAcq> Acquisitions { get; set; }
    }
}
