namespace ProberInterfaces.PnpSetup.StepInfo
{
    using System.Collections.ObjectModel;

    public interface IModuleInfo 
    {
        ObservableCollection<IProcessingModule> AlignModule { get; set; }
    }

}
