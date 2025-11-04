namespace ProberViewModel.ViewModel
{
    using LoaderBase;
    using ProberInterfaces;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using LogModule;
    using System.Collections.ObjectModel;

    public class ActiveStageInformationUCViewModel : INotifyPropertyChanged, IFactoryModule
    { 
        #region ==> PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;
            protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        #endregion


        private Window viewWindow { get; set; }
        public ILoaderSupervisor LoaderMaster { get; set; }

        private ObservableCollection<ActiveStageInfoamtion> _ActiveStageInfos;
        public ObservableCollection<ActiveStageInfoamtion> ActiveStageInfos
        {
            get { return _ActiveStageInfos; }
            set
            {
                if (value != _ActiveStageInfos)
                {
                    _ActiveStageInfos = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ActiveStageInformationUCViewModel(ILoaderSupervisor loaderSupervisor, Window window, int foupnumber)
        {
            try
            {
                LoaderMaster = loaderSupervisor;
                viewWindow = window;

                InitViewModel(foupnumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        ~ ActiveStageInformationUCViewModel()
        {

        }

        private void InitViewModel(int foupnumber)
        {
            try
            {
                if(LoaderMaster != null)
                {
                    ActiveStageInfos = new ObservableCollection<ActiveStageInfoamtion>();

                    var ActiveLotInfo = LoaderMaster.ActiveLotInfos.Find(info => info.FoupNumber == foupnumber);
                    if(ActiveLotInfo != null)
                    {
                        int stgcount = SystemModuleCount.ModuleCnt.StageCount;
                        for (int index = 1; index <= stgcount; index++)
                        {
                            int stgIdx = ActiveLotInfo.AssignedUsingStageIdxList.Find(idx => idx == index);
                            StageAssignStateEnum stgState = StageAssignStateEnum.NOT_ASSIGN;
                            if (stgIdx != 0)
                            {
                                var moduleID = ModuleID.Create(ModuleTypeEnum.CHUCK, stgIdx, "");
                                var stgObj =  ActiveLotInfo.UsingStageIdxList.Find(idx => idx == index);
                                if (stgObj != 0)
                                {
                                    stgState = StageAssignStateEnum.ASSIGN;
                                }
                                else
                                {
                                    stgState = StageAssignStateEnum.UNASSIGN;
                                }
                            }
                            ActiveStageInfos.Add(new ActiveStageInfoamtion(index, stgState));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class ActiveStageInfoamtion : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _StageIndex;
        public int StageIndex
        {
            get { return _StageIndex; }
            set
            {
                if (value != _StageIndex)
                {
                    _StageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private StageAssignStateEnum _ActiveStageState;
        public StageAssignStateEnum ActiveStageState
        {
            get { return _ActiveStageState; }
            set
            {
                if (value != _ActiveStageState)
                {
                    _ActiveStageState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ActiveStageInfoamtion(int stgindex, StageAssignStateEnum stageAssignState)
        {
            StageIndex = stgindex;
            ActiveStageState = stageAssignState;
        }

    }
}
