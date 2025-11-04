namespace ProberInterfaces.PnpSetup
{
    using System.Collections.ObjectModel;

    public interface IPnpSetupScreen 
    {
       // string ParamPath { get; set; }
       // AlignStateEnum GetAlignStateEnum();
        //EventCodeEnum SetNodeSetupState();
        ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps();
    }
}
