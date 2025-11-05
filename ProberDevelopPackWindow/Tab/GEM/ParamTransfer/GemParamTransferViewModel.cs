using System;

namespace ProberDevelopPackWindow.Tab
{
    using Autofac;
    using LoaderBase.Communication;
    using LogModule;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class GemParamTransferViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region <PropertyChanged>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private ILoaderCommunicationManager LoaderCommManager { get; set; }

        private ObservableCollection<IStageObject> _Cells = new ObservableCollection<IStageObject>();
        public ObservableCollection<IStageObject> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }


        public GemParamTransferViewModel()
        {
            InitViewModel();
        }

        public void InitViewModel()
        {
            try
            {
                if (this.GetLoaderContainer() == null)
                {
                    return;
                }

                LoaderCommManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                Cells = LoaderCommManager.GetStages();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

    }
}
