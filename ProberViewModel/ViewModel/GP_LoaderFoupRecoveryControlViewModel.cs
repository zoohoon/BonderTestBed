using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AccountModule;
using Autofac;
using FoupProcedureManagerProject;
using LoaderBase;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ControlClass.ViewModel.Foup;
using ProberInterfaces.Foup;
using RelayCommandBase;
using VirtualKeyboardControl;

namespace FoupReoveryViewModel
{
    public class GP_LoaderFoupRecoveryControlViewModel : IGPFoupRecoveryControlVM
    {
        public bool Initialized { get; set; } = false;

        readonly Guid _ViewModelGUID = new Guid("0DA29549-83DA-40A2-9A6A-1D058BDA3D88");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        private ILoaderSupervisor LoaderMaster => this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region

        private ObservableCollection<int> _FoupCount = new ObservableCollection<int>();
        public ObservableCollection<int> FoupCount
        {
            get { return _FoupCount; }
            set
            {
                if (value != _FoupCount)
                {
                    _FoupCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupModule _SelectedFoupModule;
        public IFoupModule SelectedFoupModule
        {
            get { return _SelectedFoupModule; }
            set
            {
                if (value != _SelectedFoupModule)
                {
                    _SelectedFoupModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedFoup = -1;
        public int SelectedFoup
        {
            get { return _SelectedFoup; }
            set
            {
                if (value != _SelectedFoup)
                {
                    _SelectedFoup = value;
                    if (value > -1)
                    {
                        FoupInfoInit();
                    }
                }
                RaisePropertyChanged();
            }
        }

        private FoupModuleInfo _FoupInfo;
        public FoupModuleInfo FoupInfo
        {
            get { return _FoupInfo; }
            set
            {
                if (value != _FoupInfo)
                {
                    _FoupInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupProcedure _CurProcedure;
        public IFoupProcedure CurProcedure
        {
            get { return _CurProcedure; }
            set
            {
                if (value != _CurProcedure)
                {
                    _CurProcedure = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurIndex;
        public int CurIndex
        {
            get { return _CurIndex; }
            set
            {
                if (value != _CurIndex)
                {
                    _CurIndex = value;

                    RaisePropertyChanged();
                }
            }
        }
        private string _SelectedFoupCassetteID;
        public string SelectedFoupCassetteID
        {
            get { return _SelectedFoupCassetteID; }
            set
            {
                if (value != _SelectedFoupCassetteID)
                {
                    _SelectedFoupCassetteID = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private int _SeletedTabIndex;
        public int SeletedTabIndex   // (0 LOAD) (1 UNLOAD)   
        {
            get { return _SeletedTabIndex; }
            set
            {
                if (value != _SeletedTabIndex)
                {
                    _SeletedTabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _OutputVisibility = false;
        public bool OutputVisibility
        {
            get { return _OutputVisibility; }
            set
            {
                if (value != _OutputVisibility)
                {
                    _OutputVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEngineerModeChecked = false;
        public bool IsEngineerModeChecked
        {
            get { return _IsEngineerModeChecked; }
            set
            {
                if (value != _IsEngineerModeChecked)
                {
                    _IsEngineerModeChecked = value;
                    TriggerEngineerModeFunc();
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _FoupRecoveryErrorLogBuffer = new ObservableCollection<string>();
        public ObservableCollection<string> FoupRecoveryErrorLogBuffer
        {
            get { return _FoupRecoveryErrorLogBuffer; }
            set
            {
                if (value != _FoupRecoveryErrorLogBuffer)
                {
                    _FoupRecoveryErrorLogBuffer = value;
                    RaisePropertyChanged();
                }
            }

        }

        #endregion

        #region Event

        private void RecoveryLogBuffer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                // 한번 Log를 출력할 때 Add, Remove 이벤트가 둘다 발생하므로 Add 이벤트인 경우에만 Log를 출력한다.
                if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    string logStr;

                    if(FoupRecoveryErrorLogBuffer.Count == 50)
                    {
                        FoupRecoveryErrorLogBuffer.RemoveAt(FoupRecoveryErrorLogBuffer.Count-1);
                    }

                    string NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    logStr = $"{NowTime} | Cassette#{SelectedFoup} | {LoggerManager.RecoveryLogBuffer.Last()}";

                    FoupRecoveryErrorLogBuffer.Insert(0, logStr);

                    LoggerManager.RecoveryLogBuffer.Clear();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.FoupCount; i++) // total foup count
                {
                    if (FoupCount != null)
                    {
                        if (this.FoupOpModule().GetFoupController(i).FoupModuleInfo.Enable == true)
                        {
                            var foupinfo = this.FoupOpModule().GetFoupController(i).GetFoupService().FoupModule.GetFoupModuleInfo();

                            LoggerManager.Debug($"Exit Cassette Mainteanace Page: FoupIndex = {i}, Foup Mode Changed Flag = {foupinfo.IsChangedFoupMode}");
                            if (foupinfo.IsChangedFoupMode == true)
                            {
                                this.FoupOpModule().TempSetFoupModeStatus(i, false);
                                foupinfo.IsChangedFoupMode = false;
                            }

                            if (this.E84Module().GetE84Controller(i, E84OPModuleTypeEnum.FOUP) != null)
                            {
                                var e84controller = this.E84Module().GetE84Controller(i, E84OPModuleTypeEnum.FOUP);
                                LoggerManager.Debug($"Exit Cassette Mainteanace Page: FoupIndex = {i}, E84 Mode Changed Flag = {e84controller.IsChangedE84Mode}");
                                if (e84controller.IsChangedE84Mode == true)
                                {
                                    this.E84Module().GetE84Controller(i, E84OPModuleTypeEnum.FOUP).TempSetE84ModeStatus(false);
                                    e84controller.IsChangedE84Mode = false;
                                }
                            }
                        }
                    }
                }

                SelectedFoup = -1;
                CurProcedure = null;
                CurIndex = -1;
                OutputVisibility = false;
                IsEngineerModeChecked = false;                

                if (FoupRecoveryErrorLogBuffer.Count > 0)
                {
                    FoupRecoveryErrorLogBuffer.Clear();
                }
                LoggerManager.RecoveryLogBuffer.Clear();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if (_SelectedFoup > -1)
                    {

                        SelectedFoupModule = this.FoupOpModule().GetFoupController(_SelectedFoup).GetFoupService().FoupModule;
                        SelectedFoupModule.UpdateFoupState();
                        FoupInfo = this.FoupOpModule().GetFoupController(_SelectedFoup).GetFoupService().FoupModule.GetFoupModuleInfo();
                    }

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Application.Current.Dispatcher.Invoke((() =>
                {
                    if (FoupCount.Count > 0)
                    {
                        FoupCount.Clear();
                    }
                    for (int i = 1; i <= SystemModuleCount.ModuleCnt.FoupCount; i++) // total foup count
                    {
                        if (FoupCount != null)
                        {
                            if (this.FoupOpModule().GetFoupController(i).FoupModuleInfo.Enable == true)
                            {
                                FoupCount.Add(i);

                                this.FoupOpModule().TempSetFoupModeStatus(i, true);
                                if (this.E84Module().GetE84Controller(i, E84OPModuleTypeEnum.FOUP) != null)
                                {
                                    this.E84Module().GetE84Controller(i, E84OPModuleTypeEnum.FOUP).TempSetE84ModeStatus(true);
                                }                                
                            }
                        }
                    }

                    if (SelectedFoup == -1)
                    {
                        SelectedFoup = FoupCount[0];
                    }
                    else
                    {
                        SelectedFoup = _SelectedFoup;
                    }
                    
                                   
                }));

                Init();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        private void Init()
        {
            try
            {
                LoggerManager.RecoveryLogBuffer = new ObservableCollection<string>();
                LoggerManager.RecoveryLogBuffer.CollectionChanged += RecoveryLogBuffer_CollectionChanged;

                UpdateProcedure();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetSelectedFoup(int foupindex)
        {
            try
            {
                SelectedFoup = FoupCount[foupindex-1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void FoupInfoInit()
        {
            try
            {
                //int index = 0;
                CurIndex = -1;
                CurProcedure = null;
                SelectedFoupCassetteID = "";

                SelectedFoupModule = this.FoupOpModule().GetFoupController(_SelectedFoup).GetFoupService().FoupModule;
                SelectedFoupModule.UpdateFoupState();
                FoupInfo = this.FoupOpModule().GetFoupController(_SelectedFoup).GetFoupService().FoupModule.GetFoupModuleInfo();                
                UpdateProcedure();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Sequences Control
        private AsyncCommand _SequencesPrevCammand;
        public IAsyncCommand SequencesPrevCammand
        {
            get
            {
                if (null == _SequencesPrevCammand) _SequencesPrevCammand = new AsyncCommand(SequencesPrevFunc);
                return _SequencesPrevCammand;
            }
        }

        private async Task SequencesPrevFunc()
        {
            try
            {
                //EventCodeEnum retval = SelectedFoupModule.ProcManager.SetPrevSelectedProcedureStateMapNode();
                SelectedFoupModule.ProcManager.PreviousRun();
                UpdateProcedure();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SequencesContinueCammand;
        public IAsyncCommand SequencesContinueCammand
        {
            get
            {
                if (null == _SequencesContinueCammand) _SequencesContinueCammand = new AsyncCommand(SequencesContinueFunc);
                return _SequencesContinueCammand;
            }
        }

        private async Task SequencesContinueFunc()
        {
            try
            {
                EventCodeEnum retval = SelectedFoupModule.Continue();
                await UpdateProcedure();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SequencesFastPrevCammand;
        public IAsyncCommand SequencesFastPrevCammand
        {
            get
            {
                if (null == _SequencesFastPrevCammand) _SequencesFastPrevCammand = new AsyncCommand(SequencesFastPrevFunc);
                return _SequencesFastPrevCammand;
            }
        }

        private async Task SequencesFastPrevFunc()
        {
            try
            {
                //EventCodeEnum retval = SelectedFoupModule.ProcManager.SetPrevSelectedProcedureStateMapNode();
                while(true)
                {
                    EventCodeEnum retval = SelectedFoupModule.ProcManager.PreviousRun();
                    UpdateProcedure();
                    if (retval != EventCodeEnum.NONE)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SequencesFaseContinueCammand;
        public IAsyncCommand SequencesFaseContinueCammand
        {
            get
            {
                if (null == _SequencesFaseContinueCammand) _SequencesFaseContinueCammand = new AsyncCommand(SequencesFaseContinueFunc);
                return _SequencesFaseContinueCammand;
            }
        }

        private async Task SequencesFaseContinueFunc()
        {
            try
            {
                while (true)
                {
                    //int curindex = SelectedFoupModule.ProcManager.GetSelectedProcedureIndex();
                    EventCodeEnum retval = SelectedFoupModule.Continue();
                    await UpdateProcedure();
                    if (retval != EventCodeEnum.NONE)
                    //curindex + 1 == (SelectedFoupModule.ProcManager.SelectedProcedureStateMaps as FoupProcedureStateMaps).Count)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SequencesRefreshCammand;
        public IAsyncCommand SequencesRefreshCammand
        {
            get
            {
                if (null == _SequencesRefreshCammand) _SequencesRefreshCammand = new AsyncCommand(SequencesRefreshFunc);
                return _SequencesRefreshCammand;
            }
        }

        private async Task SequencesRefreshFunc()
        {
            try
            {
                EventCodeEnum retval = SelectedFoupModule.Refresh();
                await UpdateProcedure();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region Method
        public async Task UpdateProcedure()
        {
            try
            {
                SelectedFoupModule.UpdateFoupState();

                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                ////procedure 정의되어 있지 않음. UNLOAD 시퀀스에서 IO 조합을 보고 찾도록 구현
                if (SelectedFoupModule.ProcManager.GetSelectedProcedureStateMapNode() == null)
                {
                    if(SelectedFoupModule.ModuleState.State == FoupStateEnum.LOAD)
                    {
                        retval = SelectedFoupModule.InitSelectedProcedureStateMapNode(FoupStateEnum.UNLOAD);
                    }
                    else if(SelectedFoupModule.ModuleState.State == FoupStateEnum.UNLOAD)
                    {
                        retval = SelectedFoupModule.InitSelectedProcedureStateMapNode(FoupStateEnum.LOAD);
                    }
                    else if(SelectedFoupModule.ModuleState.State == FoupStateEnum.LOADING)
                    {
                        retval = SelectedFoupModule.InitSelectedProcedureStateMapNode(FoupStateEnum.LOAD);
                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = SelectedFoupModule.Refresh();
                        }
                    }
                    else if(SelectedFoupModule.ModuleState.State == FoupStateEnum.ERROR || SelectedFoupModule.ModuleState.State == FoupStateEnum.UNLOADING)
                    {
                        retval = SelectedFoupModule.InitSelectedProcedureStateMapNode(FoupStateEnum.UNLOAD);

                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = SelectedFoupModule.Refresh();
                        }
                    }


                    this.MetroDialogManager().ShowMessageDialog($"Cassette#{SelectedFoup}", $"Changed State : {SelectedFoupModule.ModuleState.State}", EnumMessageStyle.Affirmative, "OK");
                }

                if (SelectedFoupModule.ModuleState.State == FoupStateEnum.LOAD)
                {
                    SelectedFoupModule.CheckCoverDown();
                }
                else if(SelectedFoupModule.ModuleState.State == FoupStateEnum.LOADING)
                {
                    SelectedFoupModule.CheckDockingPlate();
                }

                CurProcedure = SelectedFoupModule.ProcManager.GetSelectedProcedureStateMapNode();
                CurIndex = SelectedFoupModule.ProcManager.GetSelectedProcedureIndex();

                foreach (var input in SelectedFoupModule.Inputs)
                {
                    input.IsActive.Value = false;
                }
                foreach (var input in CurProcedure.Behavior.Inputs)
                {
                    SelectedFoupModule.Inputs.ElementAt(SelectedFoupModule.Inputs.IndexOf(input)).IsActive.Value = true;
                }
                foreach (var output in SelectedFoupModule.Outputs)
                {
                    output.IsActive.Value = false;
                }
                foreach (var output in CurProcedure.Behavior.Outputs)
                {
                    SelectedFoupModule.Outputs.ElementAt(SelectedFoupModule.Outputs.IndexOf(output)).IsActive.Value = true;
                }



                SeletedTabIndex = SelectedFoupModule.ProcManager.SetSequenceTab(CurProcedure);

                SelectedFoupCassetteID = LoaderMaster.Loader.Foups[SelectedFoup-1].CassetteID;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void TriggerEngineerModeFunc()
        {
            try
            {
                if(IsEngineerModeChecked == true)
                {
                    string text = "";

                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                    string superpassword = AccountManager.MakeSuperAccountPassword();

                    if (text.ToLower().CompareTo(superpassword) == 0)
                    {
                        OutputVisibility = true;
                    }
                    else
                    {
                        OutputVisibility = false;
                        IsEngineerModeChecked = false;
                    }
                }
                else
                {
                    //IsEngineerModeChecked == false
                    OutputVisibility = false;
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
