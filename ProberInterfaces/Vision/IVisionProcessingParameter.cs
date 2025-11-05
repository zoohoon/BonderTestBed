namespace ProberInterfaces.Vision
{

    using System.Collections.ObjectModel;

    public interface IVisionProcessingParameter : IParamNode
    {
        Element<EnumVisionProcRaft> ProcRaft { get; set; }
        Element<double> MaxFocusFlatness { get; set; }
        Element<double> FocusFlatnessTriggerValue { get; set; }
        BlobParameter BlobParam { get; set; }
        ObservableCollection<PMParameter> PMParam { get; set; }
    }
}
