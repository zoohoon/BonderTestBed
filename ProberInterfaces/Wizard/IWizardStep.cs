using System;


namespace ProberInterfaces.Wizard
{
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;
    using LogModule;

    public enum EnumModuleSetupState
    {
        INVALID = -1,
        UNDEFINED = 0,
        NOTSETYET,
        DONE
    }

    public interface IWizardStep : ICategoryNodeItem
    {
        //Guid PageGUID { get; set; }
        //EnumEnableState StateEnable { get; }
        //EnumSetupState StateSetup { get; }
        //EnumSetupState StateDevSetup { get; }
        UserControl UCDetailSummary { get; }
        //EventCodeEnum ParamValidation();
        void SetHeader(string name);
        ObservableCollection<IWizardStep> GetWizardStep();
    }

    public class SetupSummary : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ObservableCollection<SummaryContent> _SummaryContents;
        public ObservableCollection<SummaryContent> SummaryContents
        {
            get { return _SummaryContents; }
            set
            {
                if (value != _SummaryContents)
                {
                    _SummaryContents = value;
                    NotifyPropertyChanged("SummaryContents");
                }
            }
        }


    }
    public class SummaryContent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private object _Value;
        public object Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        private string _Content;
        public string Content
        {
            get { return _Content; }
            set
            {
                if (value != _Content)
                {
                    _Content = value;
                    NotifyPropertyChanged("Content");
                }
            }
        }

    }
    public static class TemplateToWizardConverter
    {
        public static ObservableCollection<IWizardStep> Converter(ObservableCollection<ITemplateModule> templatemodule)
        {
            ObservableCollection<IWizardStep> wizardSteps = new ObservableCollection<IWizardStep>();
            try
            {
                foreach (var module in templatemodule)
                {
                    //if(module.Categories.Count ==0)
                    //{
                    if (module is IWizardStep)
                    {
                        wizardSteps.Add((IWizardStep)module);
                    }

                    //}
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return wizardSteps;
        }

        private static IWizardStep NodeStep(IWizardStep step)
        {
            //foreach(var nodestep in step.Categories)
            try
            {
            //{

            //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return step;
        }
    }


}
