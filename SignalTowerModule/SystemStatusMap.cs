using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderController.GPController;
using LoaderControllerBase;
using LoaderMapView;
using LoaderParameters;
using LoaderParameters.Data;
using LogModule;
using NotifyEventModule;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SignalTowerModule
{
    public class SystemStatusMap : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private List<CellInfo> _CellInfo = new List<CellInfo>();
        public List<CellInfo> CellInfo
        {
            get { return _CellInfo; }
            set { _CellInfo = value; }
        }

        private List<FoupInfo> _FoupInfo = new List<FoupInfo>();
        public List<FoupInfo> FoupInfo
        {
            get { return _FoupInfo; }
            set { _FoupInfo = value; }
        }

        private SignalTowerManager _Module;
        public SignalTowerManager Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        private ModuleStateEnum _ModuleState;

        public ModuleStateEnum ModuleState
        {
            get { return _ModuleState; }
            set { _ModuleState = value; }
        }



        private Autofac.IContainer _Container => Module.GetLoaderContainer();

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();
        public ILoaderSupervisor LoaderSupervisor => _Container.Resolve<ILoaderSupervisor>();
        public SystemStatusMap(SignalTowerManager module)
        {
            try
            {
                Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void InitData()
        {
            try
            {
                if (Module.Initialized == false)
                {
                    if (Module.FoupOpModule().FoupControllers != null)
                    {
                        foreach (var foup in Module.FoupOpModule().FoupControllers)
                        {
                            FoupInfo.Add(new FoupInfo(foup.FoupModuleInfo.FoupNumber, foup.FoupModuleInfo.State, foup.FoupModuleInfo.FoupPRESENCEState,
                                                      foup.FoupModuleInfo.FoupModeStatus, foup.FoupModuleInfo.Enable));
                        }
                    }

                    if (LoaderCommunicationManager.Cells != null)
                    {
                        foreach (var cell in LoaderCommunicationManager.Cells)
                        {
                            CellInfo.Add(new CellInfo(cell.Index, cell.StageMode, cell.StageState, cell.Reconnecting));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // Map Info 갱신하는 함수
        public void SetPulse()
        {
            try
            {
                if (Module.StageSupervisor() != null)
                {
                    if (Module.LoaderController() != null)
                    {
                        // Foup Info 갱신
                        for (int i = 0; i < FoupInfo.Count; i++)
                        {
                            var foupController = Module.FoupOpModule().GetFoupController(i + 1);

                            FoupInfo[i].Index = foupController.FoupModuleInfo.FoupNumber;
                            FoupInfo[i].FoupState = foupController.FoupModuleInfo.State;
                            FoupInfo[i].FoupPRESENCEState = foupController.FoupModuleInfo.FoupPRESENCEState;
                            FoupInfo[i].FoupModeStatus = foupController.FoupModuleInfo.FoupModeStatus;
                            FoupInfo[i].FoupEnable = foupController.FoupModuleInfo.Enable;
                        }

                        // Cell Info 갱신
                        if (LoaderCommunicationManager != null)
                        {
                            for (int i = 0; i < CellInfo.Count; i++)
                            {
                                CellInfo[i].Index = LoaderCommunicationManager.Cells[i].Index;
                                CellInfo[i].CellMode = LoaderCommunicationManager.Cells[i].StageMode;
                                CellInfo[i].Cellstate = LoaderCommunicationManager.Cells[i].StageState;
                                CellInfo[i].Reconnecting = LoaderCommunicationManager.Cells[i].Reconnecting;
                            }
                        }

                        // machininitdone 변수의 값이 변했을때만 가져오기 // 위에서 가져오는 형태로

                        if (LoaderSupervisor.ModuleState != null)
                        {
                            ModuleState = LoaderSupervisor.ModuleState.GetState();
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

    public class CellInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public CellInfo(int cellindex)
        {
            try
            {
                _Index = cellindex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public CellInfo(int cellindex, GPCellModeEnum gPCellModeEnum, ModuleStateEnum moduleStateEnum, bool reconnecting)
        {
            try
            {
                _Index = cellindex;
                _CellMode = gPCellModeEnum;
                _Cellstate = moduleStateEnum;
                _Reconnecting = reconnecting;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        private GPCellModeEnum _CellMode;
        public GPCellModeEnum CellMode
        {
            get { return _CellMode; }
            set
            {
                if (_CellMode != value)
                {
                    _CellMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleStateEnum _Cellstate;  // 이전 값이랑 달라졌을 때만 
        public ModuleStateEnum Cellstate
        {
            get { return _Cellstate; }
            set
            {
                if (_Cellstate != value)
                {
                    _Cellstate = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsSystemError = false;

        public bool IsSystemError
        {
            get { return _IsSystemError; }
            set { _IsSystemError = value; }
        }
        private bool _Reconnecting;

        public bool Reconnecting
        {
            get { return _Reconnecting; }
            set { _Reconnecting = value; }
        }
    }

    public class FoupInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FoupInfo(int index)
        {
            try
            {
                _Index = index;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public FoupInfo(int index, FoupStateEnum foupStateEnum, FoupPRESENCEStateEnum foupPRESENCEStateEnum, FoupModeStatusEnum foupModeStatusEnum, bool foupEnable)
        {
            try
            {
                _Index = index;
                _FoupState = foupStateEnum;
                _FoupPRESENCEState = foupPRESENCEStateEnum;
                _FoupModeStatus = foupModeStatusEnum;
                _FoupEnable = foupEnable;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        private FoupStateEnum _FoupState;
        public FoupStateEnum FoupState
        {

            get { return _FoupState; }
            set
            {
                if (_FoupState != value)
                {
                    _FoupState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupPRESENCEStateEnum _FoupPRESENCEState;
        public FoupPRESENCEStateEnum FoupPRESENCEState
        {
            get { return _FoupPRESENCEState; }
            set
            {
                if (_FoupPRESENCEState != value)
                {
                    _FoupPRESENCEState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupModeStatusEnum _FoupModeStatus;

        public FoupModeStatusEnum FoupModeStatus
        {
            get { return _FoupModeStatus; }
            set
            {
                if (_FoupModeStatus != value)
                {
                    _FoupModeStatus = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _FoupEnable;

        public bool FoupEnable
        {
            get { return _FoupEnable; }
            set { _FoupEnable = value; }
        }
    }
}
