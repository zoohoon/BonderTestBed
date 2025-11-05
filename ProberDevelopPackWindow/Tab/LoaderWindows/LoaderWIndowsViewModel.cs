using System;

namespace ProberDevelopPackWindow.Tab
{
    using Autofac;
    using BarcordReaderView;
    using LoaderBase;
    using LogModule;
    using ProberInterfaces;
    using ProberViewModel;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class LoaderWIndowsViewModel : IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private IGPLoader _GPLoader;
        public IGPLoader GPLoader
        {
            get { return _GPLoader; }
            set
            {
                if (value != _GPLoader)
                {
                    _GPLoader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderModule _loaderModule;
        public ILoaderModule loaderModule
        {
            get { return _loaderModule; }
            set
            {
                if (value != _loaderModule)
                {
                    _loaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LoaderWIndowsViewModel()
        {
            try
            {
                InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void InitViewModel()
        {
            try
            {
                if (this.GetLoaderContainer() == null)
                {
                    return;
                }

                GPLoader = this.GetLoaderContainer().Resolve<IGPLoader>();
                loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();

                GPLoader?.preAlignerControlItems?.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }


        #region <!-- Command -->

        private RelayCommand _OpenBarcodeReaderWindowCommand;
        public ICommand OpenBarcodeReaderWindowCommand
        {
            get
            {
                if (null == _OpenBarcodeReaderWindowCommand) _OpenBarcodeReaderWindowCommand = new RelayCommand(OpenBarcodeReaderWindowCommandFunc);
                return _OpenBarcodeReaderWindowCommand;
            }
        }
        private void OpenBarcodeReaderWindowCommandFunc()
        {
            try
            {
                BacordReaderVM.Show(GPLoader);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _OpenTransferObjectWindowCommand;
        public ICommand OpenTransferObjectWindowCommand
        {
            get
            {
                if (null == _OpenTransferObjectWindowCommand) _OpenTransferObjectWindowCommand = new RelayCommand(OpenTransferObjectWindowCommandFunc);
                return _OpenTransferObjectWindowCommand;
            }
        }
        private void OpenTransferObjectWindowCommandFunc()
        {
            try
            {
                TemplateTransferObjectVM TransferObjectViewModel = new TemplateTransferObjectVM();
                TransferObjectViewModel.Show();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _OpenFoupSettingWindowCommand;
        public ICommand OpenFoupSettingWindowCommand
        {
            get
            {
                if (null == _OpenFoupSettingWindowCommand) _OpenFoupSettingWindowCommand = new RelayCommand(OpenFoupSettingWindowCommandFunc);
                return _OpenFoupSettingWindowCommand;
            }
        }
        private void OpenFoupSettingWindowCommandFunc()
        {
            try
            {
                GPFoupSettingVM GPFoupSettingViewModel = new GPFoupSettingVM();
                GPFoupSettingViewModel.Show(loaderModule);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _OpenLoaderJobWindowCommand;
        public ICommand OpenLoaderJobWindowCommand
        {
            get
            {
                if (null == _OpenLoaderJobWindowCommand) _OpenLoaderJobWindowCommand = new RelayCommand(OpenLoaderJobWindowCommandFunc);
                return _OpenLoaderJobWindowCommand;
            }
        }
        private void OpenLoaderJobWindowCommandFunc()
        {
            try
            {
                GPLoaderJobVM GPLoaderJobViewModel = new GPLoaderJobVM();
                GPLoaderJobViewModel.Show(loaderModule);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _SetLampStateCommand;
        public ICommand SetLampStateCommand
        {
            get
            {
                if (null == _SetLampStateCommand)
                    _SetLampStateCommand = new RelayCommand<object>(SetLampStateCommandFunc);
                return _SetLampStateCommand;
            }
        }

        private void SetLampStateCommandFunc(object obj)
        {
            try
            {
                ModuleStateEnum param = (ModuleStateEnum)obj;

                GPLoader.LoaderLampSetState(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ClearPreAlignerControlCommand;
        public ICommand ClearPreAlignerControlCommand
        {
            get
            {
                if (null == _ClearPreAlignerControlCommand) _ClearPreAlignerControlCommand = new RelayCommand(ClearPreAlignerControlCommandFunc);
                return _ClearPreAlignerControlCommand;
            }
        }

        private void ClearPreAlignerControlCommandFunc()
        {
            try
            {
                if(GPLoader != null)
                {
                    GPLoader.preAlignerControlItems.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }   
        }

        #endregion
    }
}
