using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LoaderMapView;
using LoaderParameters;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Loader;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using StageStateEnum = LoaderBase.Communication.StageStateEnum;
using System.Windows.Input;
using AlarmViewDialog;
using MultiManualContactTransferDiaglog;
using MultiManualContactSettingDialog;
using System.Windows;
using ProberViewModel.View.MultiManualContact;
using VirtualKeyboardControl;
using ProbingModule;

namespace MultiManualContactVM
{



    public class MultiManualContactViewModel : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule, ILoaderMapConvert
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("d940ad3e-b182-48aa-b23e-9384b86aa316");
        public Guid ViewModelGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        public ILoaderViewModelManager LoaderViewModelManager => (ILoaderViewModelManager)this.ViewModelManager();

        public IDeviceManager DeviceManager => _Container.Resolve<IDeviceManager>();

        public ILotOPModule LotOPModule => this.LotOPModule();        
        public IStageSupervisor StageSuperVisior => this.StageSupervisor();        
        private ILoaderModule _LoaderModule;
        public ILoaderModule LoaderModule
        {
            get { return _LoaderModule; }
            set
            {
                if (value != _LoaderModule)
                {

                    _LoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderSupervisor _LoaderMaster;
        public ILoaderSupervisor LoaderMaster
        {
            get { return _LoaderMaster; }
            set
            {
                if (value != _LoaderMaster)
                {

                    _LoaderMaster = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IStageObject> _Stages;
        public ObservableCollection<IStageObject> Stages
        {
            get { return _Stages; }
            set
            {
                if (value != _Stages)
                {
                    _Stages = value;
                    RaisePropertyChanged();
                }
            }
        }




        private int _SelectedOperationTabIndex;
        public int SelectedOperationTabIndex
        {
            get { return _SelectedOperationTabIndex; }
            set
            {
                if (value != _SelectedOperationTabIndex)
                {
                    _SelectedOperationTabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }




        private int _CellRow;
        public int CellRow
        {
            get { return _CellRow; }
            set
            {
                if (value != _CellRow)
                {
                    _CellRow = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CellColumn;
        public int CellColumn
        {
            get { return _CellColumn; }
            set
            {
                if (value != _CellColumn)
                {
                    _CellColumn = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _UpdateTimerInterval;
        public int UpdateTimerInterval
        {
            get { return _UpdateTimerInterval; }
            set
            {
                if (value != _UpdateTimerInterval)
                {
                    _UpdateTimerInterval = value;
                    RaisePropertyChanged();
                }
            }
        }        
        
        public IProbingModule ProbingModule { get; set; }        

        private IParam _ProbingDevCopyParam;
        public IParam ProbingDevCopyParam
        {
            get { return _ProbingDevCopyParam; }
            set
            {
                if (_ProbingDevCopyParam != value)
                {
                    _ProbingDevCopyParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EventCodeEnum InitModule()
        {
            try
            {                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }




        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {




                LoaderMaster = _Container.Resolve<ILoaderSupervisor>();
                LoaderModule = _Container.Resolve<ILoaderModule>();

                Stages = LoaderCommunicationManager.Cells;


                foreach (var stage in Stages)
                {
                    stage.StageInfo.SetTitles = new ObservableCollection<string>();
                }
                // Foups = LoaderModule.Foups;

                LoaderModule.SetLoaderMapConvert(this);
                LoaderModule.BroadcastLoaderInfo();

                SelectedOperationTabIndex = 0;

                UpdateTimerInterval = 30;

                CellColumn = 4;
                CellRow = 3;

                CellIndexSort();

                GetCellsInfo();                                

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                UpdateChanged();
                LoaderModule.SetLoaderMapConvert(this);
                LoaderModule.BroadcastLoaderInfo();                
                //PTPAModule = new PinPadMatchModule.PinPadMatchModule();
                //var ret = PTPAModule.DoPinPadMatch();

                //if (ret != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[{this.GetType().Name}], PageSwitched() : DoPinPadMatch is {ret}");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                foreach (var cell in Stages)
                {
                    if (cell is StageObject)
                    {
                        if ((cell as StageObject).InfoVisibility == Visibility.Hidden)
                            (cell as StageObject).InfoVisibility = Visibility.Visible;
                        
                        if ((cell as StageObject).ParamVisibility == Visibility.Visible)
                            (cell as StageObject).ParamVisibility = Visibility.Hidden;
                           
                        if ((cell as StageObject).IsSelectedParamSettingBtn == true)
                            (cell as StageObject).IsSelectedParamSettingBtn = false;
                    }                        
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private void GetCellsInfo()
        {
            try
            {
                foreach (var stage in Stages)
                {
                    if (stage.StageInfo.IsConnected == true)
                    {
                        GetCellInfo(stage);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetCellInfo(IStageObject stage)
        {
            try
            {
                stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.Index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CellIndexSort()
        {
            try
            {
                int CellCount = Stages.Count;

                if (CellCount == CellRow * CellColumn)
                {


                    // 01 02 03 04
                    // 05 06 07 08
                    // 09 10 11 12

                    List<int> sortindex = new List<int>();

                    for (int i = 1; i <= CellRow; i++)
                    {
                        int StartNo = CellCount - (CellColumn * (CellRow - (i - 1))) + 1;
                        //int FloorNo = (CellRow - i);

                        for (int j = 0; j < CellColumn; j++)
                        {
                            sortindex.Add(StartNo + j);
                        }
                    }

                    Stages = new ObservableCollection<IStageObject>(SortBy(Stages, sortindex, c => c.Index));

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IEnumerable<TResult> SortBy<TResult, TKey>(IEnumerable<TResult> itemsToSort, IEnumerable<TKey> sortKeys, Func<TResult, TKey> matchFunc)
        {
            return sortKeys.Join(itemsToSort, key => key, matchFunc, (key, iitem) => iitem);
        }

        public async Task LoaderMapConvert(LoaderMap map)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                int index = -1;

                for (int i = 0; i < map.ChuckModules.Count(); i++)
                {
                    IStageObject CurStage = Stages.Where(x => x.Index - 1 == i).FirstOrDefault();

                    if (CurStage != null)
                    {
                        index = CurStage.Index - 1;

                        if (this.LoaderMaster.StageStates.Count > i)
                        {
                            CurStage.StageState = this.LoaderMaster.StageStates[i];
                        }
                        if (map.ChuckModules[i].Substrate != null)
                        {
                            CurStage.State = StageStateEnum.Requested;
                            CurStage.TargetName = map.ChuckModules[i].Substrate.PrevPos.ToString();
                            CurStage.Progress = map.ChuckModules[i].Substrate.OriginHolder.Index;
                            CurStage.WaferObj = map.ChuckModules[i].Substrate;
                        }
                        else
                        {
                            CurStage.State = StageStateEnum.Not_Request;
                        }

                        CurStage.WaferStatus = map.ChuckModules[i].WaferStatus;

                        if (map.CCModules[i].Substrate != null)
                        {
                            CurStage.CardObj = map.CCModules[i].Substrate;
                            CurStage.CardStatus = map.CCModules[i].WaferStatus;
                            CurStage.Progress = map.CCModules[i].Substrate.OriginHolder.Index;
                        }

                        CurStage.CardStatus = map.CCModules[i].WaferStatus;
                    }
                }

                int selectedFoupNum = -1;
                bool isExternalLotStart = false;

                if (LoaderMaster.Mode == LoaderMasterMode.External)
                {
                    isExternalLotStart = true;
                }

                for (int i = 0; i < map.CassetteModules.Count(); i++)
                {

                    for (int j = 0; j < map.CassetteModules[i].SlotModules.Count(); j++)
                    {
                        //var slot = new SlotObject(map.CassetteModules[i].SlotModules.Count() - j);
                        if (map.CassetteModules[i].SlotModules[j].Substrate != null)
                        {
                            if (isExternalLotStart && LoaderMaster.ActiveLotInfos[i].State == LotStateEnum.Running) //foupNumber가 같을때
                            {
                                if (!(LoaderMaster.ActiveLotInfos[i].UsingSlotList.Where(idx => idx == LoaderModule.Foups[i].Slots[j].Index).FirstOrDefault() > 0))
                                {
                                    LoaderModule.Foups[i].Slots[j].WaferState = EnumWaferState.SKIPPED;
                                }
                                else
                                {
                                    LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                }

                                if (LoaderMaster.ActiveLotInfos[i].UsingPMIList.Where(idx => idx == LoaderModule.Foups[i].Slots[j].Index).FirstOrDefault() > 0)
                                {
                                    if (LoaderModule.Foups[i].Slots[j].WaferObj != null)
                                    {
                                        //LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMITriggerEnum.WAFER_INTERVAL;
                                        LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMIRemoteTriggerEnum.UNDIFINED;
                                        LoaderModule.Foups[i].Slots[j].WaferObj.DoPMIFlag = true;
                                    }
                                }
                            }
                            else
                            {
                                LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                            }
                        }
                        //foup.Slots.Add(slot);
                    }
                }
            }));
        }



        private RelayCommand<object> _UsedBellDrawerOpenCommand;
        public ICommand UsedBellDrawerOpenCommand
        {
            get
            {
                if (null == _UsedBellDrawerOpenCommand) _UsedBellDrawerOpenCommand = new RelayCommand<object>(UsedBellDrawerOpenCmd);
                return _UsedBellDrawerOpenCommand;
            }
        }

        private void UsedBellDrawerOpenCmd(object obj)
        {
            try
            {
                IStageObject SelecgtedStage = obj as IStageObject;
                //if (SelecgtedStage != null && SelecgtedStage.StageInfo != null && SelecgtedStage.StageInfo.IsConnected == true)
                if (SelecgtedStage != null && SelecgtedStage.StageInfo != null)
                {
                    if (SelecgtedStage.StageInfo.ErrorCodeAlarams == null)
                    {
                        SelecgtedStage.StageInfo.ErrorCodeAlarams = new ObservableCollection<AlarmLogData>();
                    }

                    AlarmViewDialogViewModel.Show(SelecgtedStage.StageInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<int> _StageGetErrorInfoCommand;
        public ICommand StageGetErrorInfoCommand
        {
            get
            {
                if (null == _StageGetErrorInfoCommand)
                {
                    _StageGetErrorInfoCommand = new AsyncCommand<int>(StageGetErrorInfoCommandFunc);
                }
                return _StageGetErrorInfoCommand;
            }
        }
        private async Task StageGetErrorInfoCommandFunc(int index)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                if (stage != null)
                {
                    var message = stage.GetLotErrorMessage();
                    //await this.MetroDialogManager().ShowMessageDialog("Error Message", message, EnumMessageStyle.Affirmative);

                    await this.MetroDialogManager().ShowMessageDialog($"[LOT PAUSE] in [Cell#{index}]", message, EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }




        public void UpdateChanged()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var cell in Stages)
                {
                    if (cell.StageMode == GPCellModeEnum.MAINTENANCE&& cell.IsRecoveryMode==false && (cell.StageState == ModuleStateEnum.IDLE || cell.StageState == ModuleStateEnum.PAUSED))
                    {
                        if (cell is StageObject)
                        {
                            retVal = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(cell.Index).CheckManualZUpState();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                (cell as StageObject).ManualZUPState = ManualZUPStateEnum.Z_UP;
                            }
                            else
                            {
                                if ((cell as StageObject).ManualZUPState != ManualZUPStateEnum.Z_DOWN)
                                {
                                    (cell as StageObject).ManualZUPState = ManualZUPStateEnum.NONE;
                                }
                            }

                            (cell as StageObject).StageInfo.LotData = LoaderMaster.GetStageLotData(cell.Index);
                            (cell as StageObject).ManualZUPEnable = ManualZUPEnableEnum.Enable;
                            (cell as StageObject).CellTransferBtnEnable = true;
                            (cell as StageObject).CellParamSettingBtnEnable = true;
                            (cell as StageObject).CellZUPBtnEnable = true;
                            (cell as StageObject).CellZDownBtnEnable = true;
                            (cell as StageObject).CellInspectionBtnEnable = false;                            

                            if ((cell as StageObject).WaferStatus == EnumSubsStatus.EXIST && (cell as StageObject).WaferObj.WaferType.Value == EnumWaferType.STANDARD)
                            {
                                (cell as StageObject).CellWaferAlignBtnEnable = true;
                            }
                            else
                            {
                                (cell as StageObject).CellWaferAlignBtnEnable = false;
                            }
                            if ((cell as StageObject).CardStatus == EnumSubsStatus.EXIST)
                            {
                                (cell as StageObject).CellPinAlignBtnEnable = true;
                                (cell as StageObject).CellManualSoakingEnable = true;
                            }
                            else
                            {
                                (cell as StageObject).CellManualSoakingEnable = false;
                                (cell as StageObject).CellPinAlignBtnEnable = false;
                            }

                            if ((cell as StageObject).StageInfo.LotData.WaferAlignState.Equals("DONE") && (cell as StageObject).StageInfo.LotData.PinAlignState.Equals("DONE"))
                            {
                                if ((cell as StageObject).ManualZUPState == ManualZUPStateEnum.Z_UP)
                                {
                                    (cell as StageObject).CellZUPBtnEnable = false;
                                    (cell as StageObject).CellZDownBtnEnable = true;
                                    (cell as StageObject).CellPinAlignBtnEnable = false;
                                    (cell as StageObject).CellWaferAlignBtnEnable = false;
                                    (cell as StageObject).CellTransferBtnEnable = false;
                                    (cell as StageObject).CellManualSoakingEnable = false;
                                    (cell as StageObject).CellInspectionBtnEnable = false;
                                }
                                else
                                {
                                    (cell as StageObject).CellZUPBtnEnable = true;
                                    (cell as StageObject).CellZDownBtnEnable = false;
                                    (cell as StageObject).CellInspectionBtnEnable = true;
                                }
                            }
                            else
                            {
                                (cell as StageObject).CellZUPBtnEnable = false;
                                (cell as StageObject).CellZDownBtnEnable = true;
                            }

                            if ((cell as StageObject).IsSelectedParamSettingBtn)
                            {
                                if ((cell as StageObject).ManualZUPState == ManualZUPStateEnum.Z_UP)
                                {
                                    (cell as StageObject).CellZUPBtnEnable = false;
                                    (cell as StageObject).CellZDownBtnEnable = true;
                                }
                                else
                                {
                                    (cell as StageObject).CellZUPBtnEnable = true;
                                    (cell as StageObject).CellZDownBtnEnable = false;
                                }                                    
                            }                            
                        }
                    }
                    else
                    {

                        if (cell is StageObject)
                        {
                            (cell as StageObject).ManualZUPState = ManualZUPStateEnum.UNDEFINED;
                            (cell as StageObject).ManualZUPEnable = ManualZUPEnableEnum.Disable;
                            (cell as StageObject).CellParamSettingBtnEnable = false;
                            (cell as StageObject).CellPinAlignBtnEnable = false;
                            (cell as StageObject).CellTransferBtnEnable = false;
                            (cell as StageObject).CellWaferAlignBtnEnable = false;
                            (cell as StageObject).CellZDownBtnEnable = false;
                            (cell as StageObject).CellZUPBtnEnable = false;
                            (cell as StageObject).CellManualSoakingEnable = false;
                            (cell as StageObject).CellInspectionBtnEnable = false;                     
                        }
                    }
                }        
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CheckZupState()
        {
        
            try
            {
                foreach (var cell in Stages)
                {
                    if (cell.StageMode == GPCellModeEnum.MAINTENANCE && (cell.StageState == ModuleStateEnum.IDLE || cell.StageState == ModuleStateEnum.PAUSED))
                    {
                        
                    }
                }
            }catch(Exception err)
            {

            }
        }

        private AsyncCommand<int> _CellZUPCommand;
        public ICommand CellZUPCommand
        {
            get
            {
                if (null == _CellZUPCommand) _CellZUPCommand = new AsyncCommand<int>(CellZUPCommandFunc);
                return _CellZUPCommand;
            }
        }

        private async Task CellZUPCommandFunc(int param)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                retVal = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(param).DoPinPadMatch_FirstSequence();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(param).DO_ManualZUP();
                    if (retVal == EventCodeEnum.NONE)
                    {
                        (Stages[param - 1] as StageObject).ManualZUPState = ManualZUPStateEnum.Z_UP;
                        (Stages[param - 1] as StageObject).ManualProbingTime = DateTime.Now;
                        (Stages[param - 1] as StageObject).CellZUPBtnEnable = false;
                        (Stages[param - 1] as StageObject).CellZDownBtnEnable = true;                        
                    }
                }
                UpdateChanged();                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }




        private AsyncCommand<int> _CellZDownCommand;
        public ICommand CellDownCommand
        {
            get
            {
                if (null == _CellZDownCommand) _CellZDownCommand = new AsyncCommand<int>(CellZDownCommandFunc);
                return _CellZDownCommand;
            }
        }

        private async Task CellZDownCommandFunc(int param)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(param).DO_ManualZDown();
                (Stages[param - 1] as StageObject).ManualZUPState = ManualZUPStateEnum.Z_DOWN;
                this.ProbingModule().ClearUnderDutDevs();
                UpdateChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }





        private AsyncCommand<int> _CellWaferAlignCommand;
        public ICommand CellWaferAlignCommand
        {
            get
            {
                if (null == _CellWaferAlignCommand)
                {
                    _CellWaferAlignCommand = new AsyncCommand<int>(CellWaferAlignCommandFunc);
                    // _CellWaferAlignCommand.SetJobTask(LoaderCommunicationManager.WaitStageJob);
                }
                return _CellWaferAlignCommand;
            }
        }
        private async Task CellWaferAlignCommandFunc(int param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var ret = this.MetroDialogManager().ShowMessageDialog($"Wafer alignment in Cell{param}", "Are you sure you want to wafer alignment?", EnumMessageStyle.AffirmativeAndNegative).Result;
                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    retval = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(param).DO_ManualWaferAlign();
                    if (retval == EventCodeEnum.NONE)
                    {
                        ret = this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Succeed Wafer alignment", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        ret = this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Fail Wafer alignment", EnumMessageStyle.Affirmative).Result;
                    }
                    UpdateChanged();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //  this.MetroDialogManager().CloseWaitCancelDialaog();
                //LoaderCommunicationManager.SetLoaderWorkingFlag(false);
            }
        }



        private AsyncCommand<int> _CellPinAlignCommand;
        public ICommand CellPinAlignCommand
        {
            get
            {
                if (null == _CellPinAlignCommand)
                {
                    _CellPinAlignCommand = new AsyncCommand<int>(CellPinAlignCommandFunc);
                    //  _CellPinAlignCommand.SetJobTask(LoaderCommunicationManager.WaitStageJob);
                }
                return _CellPinAlignCommand;
            }
        }
        private async Task CellPinAlignCommandFunc(int param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                EnumMessageDialogResult ret;
                ret = this.MetroDialogManager().ShowMessageDialog($"Pin alignment in Cell{param}", "Are you sure you want to pin alignment?", EnumMessageStyle.AffirmativeAndNegative).Result;

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    retval = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(param).DO_ManualPinAlign();

                    UpdateChanged();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<int> _CellTransferCommand;
        public ICommand CellTransferCommand
        {
            get
            {
                if (null == _CellTransferCommand) _CellTransferCommand = new AsyncCommand<int>(CellTransferCommandFunc);
                return _CellTransferCommand;
            }
        }

        private async Task CellTransferCommandFunc(int param)
        {
            try
            {
                MultiManualContactTransferVM multiContactView = new MultiManualContactTransferVM();
                multiContactView.Show(_Container, param);
                UpdateChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<int> _CellParamSettingCommand;
        public ICommand CellParamSettingCommand
        {
            get
            {
                if (null == _CellParamSettingCommand) _CellParamSettingCommand = new AsyncCommand<int>(CellParamSettingCommandFunc);
                return _CellParamSettingCommand;
            }
        }

        private async Task CellParamSettingCommandFunc(int param)
        {
            try
            {
                //MultiManualContactSettingVM multiContactView = new MultiManualContactSettingVM();
                //multiContactView.Show(_Container, param);
                //ProbingDevCopyParam = ProbingModule.GetProbingDevIParam(param);
                if ((Stages[param - 1] as StageObject).IsSelectedParamSettingBtn == false)
                {
                    (Stages[param - 1] as StageObject).InfoVisibility = Visibility.Hidden;
                    (Stages[param - 1] as StageObject).ParamVisibility = Visibility.Visible;
                    (Stages[param - 1] as StageObject).IsSelectedParamSettingBtn = true;                    
                }
                else
                {
                    (Stages[param - 1] as StageObject).InfoVisibility = Visibility.Visible;
                    (Stages[param - 1] as StageObject).ParamVisibility = Visibility.Hidden;
                    (Stages[param - 1] as StageObject).IsSelectedParamSettingBtn = false;                    
                }             
                UpdateChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<int> _CellManualSoakCommand;
        public ICommand CellManualSoakCommand
        {
            get
            {
                if (null == _CellManualSoakCommand) _CellManualSoakCommand = new AsyncCommand<int>(CellManualSoakCommandFunc);
                return _CellManualSoakCommand;
            }
        }

        private async Task CellManualSoakCommandFunc(int param)
        {
            try
            {
                MultiManualContactTransferVM multiContactView = new MultiManualContactTransferVM();
                multiContactView.Show(_Container, param);
                UpdateChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<int> _CellGotoInspectionCommand;
        public ICommand CellGotoInspectionCommand
        {
            get
            {
                if (null == _CellGotoInspectionCommand) _CellGotoInspectionCommand = new AsyncCommand<int>(CellGotoInspectionCommandFunc);
                return _CellGotoInspectionCommand;
            }
        }

        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        private async Task CellGotoInspectionCommandFunc(int param)
        {
            try
            {
                this.LoaderCommunicationManager.SelectedStage = LoaderCommunicationManager.GetStage(param);
                this.StageSupervisor().StageModuleState.ZCLEARED();
                await this.ViewModelManager().ViewTransitionAsync(new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"));
                UpdateChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }        
    }

}
