using ProberInterfaces.Param;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace ProberInterfaces.PinAlign.ProbeCardData
{
    [ServiceContract]
    public interface IPinData
    {
        //PinCoordinate AbsPos { get; }
        PinCoordinate AbsPos { get; }
        PinCoordinate AbsPosOrg { get; set; }
        PinCoordinate AlignedOffset { get; set; }
        PinCoordinate MarkCumulativeCorrectionValue { get; set; }
        PinCoordinate LowCompensatedOffset { get; set; }
        //PinCoordinate ThetaAbsPos { get; }
        PinCoordinate RelPos { get; set; }
        Element<int> DutNumber { get; set; }
        Element<MachineIndex> DutMacIndex { get; set; }
        Element<PinCoordinate> DutLLconerPos { get; set; }
        //ObservableCollection<UpdatePinPos> UpdatePins { get; set; }
        //IDut DutInfo { get; set; }
        Element<PINALIGNRESULT> Result { get; set; }
        Element<PINALIGNRESULT> PinTipResult { get; set; }
        ObservableCollection<PinCoordinate> UpdatePinsHistory { get; set; }
        PinSearchParameter PinSearchParam { get; set; }
        //Element<String> SerialNum { get; set; }
        Element<int> PinNum { get; set; }
        Element<Guid> PadGuid { get; set; }
        MachineIndex PadMachineIndex { get; set; }
        Element<bool> IsAlignPin { get; set; }
        Element<bool> IsRefPin { get; set; }
        Element<bool> IsRegisteredPin { get; set; }
        Element<PINMODE> PinMode { get; set; }
        Element<int> GroupNum { get; set; }
        MachineCoordinate MachineCoordPos { get; set; }
    }
}
