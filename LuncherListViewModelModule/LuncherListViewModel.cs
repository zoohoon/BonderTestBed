using System;
using System.Runtime.CompilerServices;

namespace LuncherListViewModelModule
{
    using Autofac;
    using LoaderBase.Communication;
    using LoaderMapView;
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class LuncherListViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        private ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        public ObservableCollection<ILauncherObject> MultiLaunchers
        {
            get { return _LoaderCommunicationManager.GetMultiLaunchers(); }
        }

   

        private LauncherObject _SelectedLauncherObj;
        public LauncherObject SelectedLauncherObj
        {
            get { return _SelectedLauncherObj; }
            set
            {
                if (value != _SelectedLauncherObj)
                {
                    _SelectedLauncherObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SelectedAllLaunchers;
        public bool SelectedAllLaunchers
        {
            get { return _SelectedAllLaunchers; }
            set
            {
                if (value != _SelectedAllLaunchers)
                {
                    _SelectedAllLaunchers = value;
                    UpdateCheckedState(SelectedAllLaunchers);
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Creator & Init
        public LuncherListViewModel()
        {
        }
        #endregion

        #region //.. Command & Method

        private RelayCommand<object> _ObjectClickCommand;
        public ICommand ObjectClickCommand
        {
            get
            {
                if (null == _ObjectClickCommand) _ObjectClickCommand = new RelayCommand<object>(ObjectClickCommandFunc);
                return _ObjectClickCommand;
            }
        }

        private void ObjectClickCommandFunc(object param)
        {
            try
            {
                object[] _param = param as object[];
                // 0 : ListView
                // 1 : SelectedIndex
                // 2 : SelectedItem
                ListView listView = null;
                int index = -1;
                ListViewItem listViewItem = null;

                listView = (ListView)_param[0];
                index = (int)_param[1];
                if (index == -1)
                    return;
                listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(index);
                object item = _param[2];

                SelectedLauncherObj = (LauncherObject)item;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void UpdateCheckedState(bool ischecked)
        {
            try
            {
                if (MultiLaunchers != null)
                {
                    foreach (var launcher in MultiLaunchers)
                    {
                        launcher.IsChecked = ischecked;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region //..Get Drive Information Command


        private AsyncCommand _GetDriveInfoCommand;
        public ICommand GetDriveInfoCommand
        {
            get
            {
                if (null == _GetDriveInfoCommand) _GetDriveInfoCommand = new AsyncCommand(GetDrive);
                return _GetDriveInfoCommand;
            }
        }

        private Task GetDrive()
        {
            //LoaderCommunication method call -> Method - channel - multiExecuter drive get & messagesend
            //Servicecallback drive info -> alarm UI
            try
            {
                var lunchers = _LoaderCommunicationManager.GetMultiLaunchers();

                foreach (var luncher in lunchers)
                {

                    if (luncher.IsConnected) //연결되어 있는 luncher Drive Info Get
                    {
                        _LoaderCommunicationManager.GetDiskInfo(luncher.Index);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
