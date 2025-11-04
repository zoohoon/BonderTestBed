using System;

namespace StageListViewModelModule
{
    using Autofac;
    using LogModule;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderMapView;
    using MultiLauncherProxy;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class StageListViewModel : INotifyPropertyChanged, IFactoryModule
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
        public ILoaderSupervisor loaderMaster => this.GetLoaderContainer().Resolve<ILoaderSupervisor>();


        public ObservableCollection<LoaderBase.Communication.IStageObject> Cells
        {
            get { return _LoaderCommunicationManager.GetStages(); }
        }

        private ObservableCollection<LauncherObject> _MultiLaunchers;
        public ObservableCollection<LauncherObject> MultiLaunchers
        {
            get { return _MultiLaunchers; }
            set
            {
                if (value != _MultiLaunchers)
                {
                    _MultiLaunchers = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<MultiExecuterProxy> _MultiLauncherProxies;
        public ObservableCollection<MultiExecuterProxy> MultiLauncherProxies
        {
            get { return _MultiLauncherProxies; }
            set
            {
                if (value != _MultiLauncherProxies)
                {
                    _MultiLauncherProxies = value;
                    RaisePropertyChanged();
                }
            }
        }

        public StageObject SelectedStageObj
        {
            get { return (StageObject)_LoaderCommunicationManager.SelectedStage; }
            set
            {
                if (value != (_LoaderCommunicationManager.SelectedStage))
                {
                    _LoaderCommunicationManager.SelectedStage = (StageObject)value;
                    RaisePropertyChanged();
                }
            }
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

        private string _CurListView = "STAGE";
        public string CurListView
        {
            get { return _CurListView; }
            set
            {
                if (value != _CurListView)
                {
                    _CurListView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _CellListViewVisibility = Visibility.Visible;
        public Visibility CellListViewVisibility
        {
            get { return _CellListViewVisibility; }
            set
            {
                if (value != _CellListViewVisibility)
                {
                    _CellListViewVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _LauncherListViewVisibility = Visibility.Hidden;
        public Visibility LauncherListViewVisibility
        {
            get { return _LauncherListViewVisibility; }
            set
            {
                if (value != _LauncherListViewVisibility)
                {
                    _LauncherListViewVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SelectedAllStage = false;
        public bool SelectedAllStage
        {
            get { return _SelectedAllStage; }
            set
            {
                if (value != _SelectedAllStage)
                {
                    _SelectedAllStage = value;
                    UpdateCheckedState(_SelectedAllStage);
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Creator & Init
        public StageListViewModel()
        {
            //var lunchers = this._LoaderCommunicationManager.GetMultiExecuterProxies();
            //MultiLauncherProxies = new ObservableCollection<MultiExecuterProxy>();
            //MultiLaunchers = new ObservableCollection<LauncherObject>();
            //for (int index = 0; index < lunchers.Count; index++)
            //{
            //    MultiLauncherProxies.Add(lunchers[index] as MultiExecuterProxy);
            //   MultiLaunchers.Add(new LauncherObject(index, lunchers[index]));
            //}
            //ConnectMultiProxys();
        }
        #endregion
        
        #region //..Command & Method

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

                IStageSupervisorProxy preStage = null;
                if (SelectedStageObj != null)
                {
                    preStage = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                }
                if (item is StageObject)
                {
                    SelectedStageObj = (StageObject)item;

                    if (preStage != null)
                    {
                        //이전 선택된 Stage Display update 끊기.
                        preStage.SetAcceptUpdateDisp(false);
                    }
                    //선택된 Stage Display update 연결.
                    _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>()?.SetAcceptUpdateDisp(true);

                    // TODO : REMOVE ?
                    //if (SelectedStageObj.StageInfo.WaferObject != null)
                    //{
                    //    SelectedStageObj.StageInfo.MapIndexX = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value;
                    //    SelectedStageObj.StageInfo.MapIndexY = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value;
                    //}

                    //this.StageSupervisor().WaferObject = SelectedStageObj.StageInfo.WaferObject;
                }
                else if (item is LauncherObject)
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
                if (Cells != null)
                {
                    foreach (var cell in Cells)
                    {
                        StageObject stage = (StageObject)cell;
                        //if (stage.StageInfo.IsExcuteProgram)
                        stage.StageInfo.IsChecked = ischecked;
                    }
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
