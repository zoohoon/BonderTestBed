using System;
using System.Collections.Generic;

namespace ProberInterfaces.PinAlign.ProbeCardData
{
    using ProberInterfaces.Param;
    using System.Windows;
    using System.ServiceModel;

    [Serializable]
    public class WrapperDutlist
    {
        public AsyncObservableCollection<IDut> DutList { get; set; }
    }

    [ServiceContract]
    public interface IDut
    {
        int DutNumber { get; set; }
        bool DutEnable { get; set; }
        MachineIndex MacIndex { get; set; }
        UserIndex UserIndex { get; set; }
        List<IPinData> PinList { get; set; }
        PinCoordinate RefCorner { get; set; }
        double DutSizeLeft { get; set; }
        double DutSizeTop { get; set; }
        double DutSizeWidth { get; set; }
        double DutSizeHeight { get; set; }
        Visibility DutVisibility { get; set; }
    }
}
