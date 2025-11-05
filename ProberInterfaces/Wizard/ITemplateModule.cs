namespace ProberInterfaces.Wizard
{

    using ProberInterfaces.PnpSetup;
    using System.Collections.ObjectModel;

    //public interface IWizardStepTemplateModule : IWizardStep, IUseSetUI
    //{
    //}

    //public interface IWizardTemplateModule : IWizardStepTemplateModule
    //{
    //    //UserControl UCDetailSummary { get; }
    //}

    public interface IWizardPostTemplateModule : IWizardStep, IPnpSetupScreen
    {
        ObservableCollection<int> InitSettingIDList { get; set; }
        //UserControl UCDetailSummary { get; }
    }

    //public class WizardTemplateModule : INotifyPropertyChanged, IWizardTemplateModule
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void NotifyPropertyChanged(String info)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(info));
    //        }
    //    }

    //    public EventCodeEnum ParamValidation()
    //    {
    //        return EventCodeEnum.NONE;
    //    }

    //    public void SetEnableState(EnumEnableState state)
    //    {
    //        return;
    //    }

    //    public void SetParent(string name)
    //    {
    //    }

    //    private string _Label;
    //    public string Label
    //    {
    //        get { return _Label; }
    //        set
    //        {
    //            if (value != _Label)
    //            {
    //                _Label = value;
    //                NotifyPropertyChanged("Label");
    //            }
    //        }
    //    }

    //    public EnumEnableState StateEnable { get; set; }

    //    public EnumSetupState StateSetup { get; set; }

    //    public UserControl UCDetailSummary { get; }

    //    public string Parent { get; }

    //    public Guid PageGUID { get; set; }

    //    public WizardTemplateModule(string label)
    //    {
    //        Label = label;
    //    }

    //}

}
