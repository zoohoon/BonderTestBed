using System;

namespace ProberInterfaces.Wizard
{
    using ProberInterfaces.State;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    public interface IWizardCategoryForm : IWizardCategory
    {
        EnumWizardCategoryState Categorystate { get; set; }
        ObservableCollection<WizardMainStep> WizardStep { get; set; }
    }

    public interface IWizardCategoryInfo
    {

    }


    public class WizardMainStep : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private IWizardStep _AcWizardStep;
        public IWizardStep AcWizardStep
        {
            get { return _AcWizardStep; }
            set
            {
                if (value != _AcWizardStep)
                {
                    _AcWizardStep = value;
                    NotifyPropertyChanged("AcWizardStep");
                }
            }
        }

        private ObservableCollection<IWizardStep> _Steps;
        public ObservableCollection<IWizardStep> Steps
        {
            get { return _Steps; }
            set
            {
                if (value != _Steps)
                {
                    _Steps = value;
                    NotifyPropertyChanged("Steps");
                }
            }
        }

        //private WizardStep _WizardStep;
        //public WizardStep WizardStep
        //{
        //    get { return _WizardStep; }
        //    set
        //    {
        //        if (value != _WizardStep)
        //        {
        //            _WizardStep = value;
        //            NotifyPropertyChanged("WizardStep");
        //        }
        //    }
        //}

        public WizardMainStep()
        {

        }
        public WizardMainStep(IWizardStep acwizardStep)
        {
            AcWizardStep = acwizardStep;
        }

    }
}
