using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ProberErrorCode;
using ProberInterfaces;
using System.Runtime.CompilerServices;
using RelayCommandBase;
using System.Windows.Input;
using LogModule;
using VirtualKeyboardControl;
using System.Windows;
using MetroDialogInterfaces;
using System.Threading;
using LoaderBase.Communication;
using SerializerUtil;
using Autofac;
using SoakingParameters;
using System.Windows.Threading;
using LoaderBase;

namespace ProberViewModel
{
    public class ManualSoakingViewModel : IMainScreenViewModel, IManualSoakingViewModel
    {
        private enum SoakMode
        {
            START,
            STOP,
            UNDEFINED,
        }

        readonly Guid _ViewModelGUID = new Guid("88FC6405-0266-443D-8628-F01BA17A1C45");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private readonly Dispatcher dispatcher;
        public ILoaderCommunicationManager _LoaderCommunicationManager = null;

        private readonly int MinimumSoakingTime = 5;

        public ManualSoakingViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region Property
        private int _ManualSoakingTime;
        public int ManualSoakingTime
        {
            get
            {
                return _ManualSoakingTime;
            }
            set
            {
                _ManualSoakingTime = value;
                RaisePropertyChanged();
            }
        }

        private int _RecommandedSoakingTime;
        public int RecommandedSoakingTime
        {
            get
            {
                return _RecommandedSoakingTime;
            }
            set
            {
                _RecommandedSoakingTime = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingPolishWaferViewModel _ManualSoakingPolishWafer = new UcSoakingPolishWaferViewModel();
        public UcSoakingPolishWaferViewModel ManualSoakingPolishWafer
        {
            get
            {
                return _ManualSoakingPolishWafer;
            }
            set
            {
                _ManualSoakingPolishWafer = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingStepViewModel _ManualSoakingStep = new UcSoakingStepViewModel();
        public UcSoakingStepViewModel ManualSoakingStep
        {
            get
            {
                return _ManualSoakingStep;
            }
            set
            {
                _ManualSoakingStep = value;
                RaisePropertyChanged();
            }
        }

        private string _SoakedTime;
        public string SoakedTime
        {
            get { return _SoakedTime; }
            set
            {
                if (value != _SoakedTime)
                {
                    _SoakedTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _RemainingTime;
        public string RemainingTime
        {
            get { return _RemainingTime; }
            set
            {
                if (value != _RemainingTime)
                {
                    _RemainingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _EditLockMode = Visibility.Hidden;
        public Visibility EditLockMode
        {
            get { return _EditLockMode; }
            set
            {
                if (value != _EditLockMode)
                {
                    _EditLockMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CanStartSoak;
        public bool CanStartSoak
        {
            get { return _CanStartSoak; }
            set
            {
                if (value != _CanStartSoak)
                {
                    _CanStartSoak = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CanStopSoak;
        public bool CanStopSoak
        {
            get { return _CanStopSoak; }
            set
            {
                if (value != _CanStopSoak)
                {
                    _CanStopSoak = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime ManualSoakingStartTime { get; set; } = default;
        private SoakMode SoakingMode { get; set; } = SoakMode.UNDEFINED;

        private SoakingStateEnum SoakingSubState { get; set; } = SoakingStateEnum.UNDEFINED;

        private Autofac.IContainer _Container => this.GetLoaderContainer();
        private ILoaderModule Loader => _Container.Resolve<ILoaderModule>();

        #endregion

        #region IMainScreenViewModel
        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ManualSoakingPolishWafer.SetStatusSoakingState(SoakingStateEnum.MANUAL);
                ManualSoakingStep.SetStatusSoakingState(SoakingStateEnum.MANUAL);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private IStageObject _SelectedStage;
        public IStageObject SelectedStage
        {
            get { return _SelectedStage; }
            set
            {
                if (value != _SelectedStage)
                {
                    _SelectedStage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {

            try
            {
                ReadParameter();                
                ProcessingAccordingToCellState();
                if(SoakingMode != SoakMode.START )
                {
                    SoakedTime = "";
                    RemainingTime = "";
                }

                SelectedStage = _LoaderCommunicationManager.Cells[_LoaderCommunicationManager.SelectedStageIndex - 1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return EventCodeEnum.NONE;
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                SaveParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }


        public bool Initialized { get; set; } = false;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

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
        #endregion

        #region IManualSoakingViewModel

        #endregion

        #region ICommand
        private RelayCommand<Object> textBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == textBoxClickCommand) textBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return textBoxClickCommand;
            }
        }

        public void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        private AsyncCommand<object> startSoakingCommand;
        public IAsyncCommand StartSoakingCommand
        {
            get
            {
                if (null == startSoakingCommand) startSoakingCommand = new AsyncCommand<object>(StartSoakingCommandFunc);
                return startSoakingCommand;
            }
        }

        private async Task StartSoakingCommandFunc(object obj)
        {           
            try
            {                
                if (ManualSoakingTime < MinimumSoakingTime)
                {
                    this.MetroDialogManager().ShowMessageDialog($"Not enough manual soaking time", $"Manual soaking time is at least {ManualSoakingTime} second.", EnumMessageStyle.Affirmative);
                    return;
                }

                // selected cell check
                if (_LoaderCommunicationManager == null)
                {
                    _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                }

                if ((_LoaderCommunicationManager == null) || (_LoaderCommunicationManager.SelectedStage == null))
                {                    
                    return;
                }

                bool isCurTempWithinSetTempRange = this.SoakingModule().IsCurTempWithinSetSoakingTempRange();
                if (false == isCurTempWithinSetTempRange)  //temp가 사용자가 지정한 SV에 도달되지 않은경우 Soaking을 지원하지 않는다.
                {
                    this.MetroDialogManager().ShowMessageDialog($"Manual soaking", $"Temp. Deviation Out of Range.", EnumMessageStyle.Affirmative);
                    return;
                }

                var loaderMap = Loader.GetLoaderInfo().StateMap;
                if (loaderMap.ChuckModules[_LoaderCommunicationManager.SelectedStage.Index - 1].WaferStatus != EnumSubsStatus.EXIST)
                {
                    this.MetroDialogManager().ShowMessageDialog($"Manual soaking", $"Wafer is no exist on cell", EnumMessageStyle.Affirmative);
                    return;
                }

                // Cell이 Idle 상태인 경우만 Manual Soaking을 할 수 있도록 한다.
                // ex) Running 상태에서 Pause 후 Manual Soaking 시도하는 경우 할 수 없도록 한다.
                if (_LoaderCommunicationManager.SelectedStage.StageState != ModuleStateEnum.IDLE)
                {
                    this.MetroDialogManager().ShowMessageDialog($"Manual soaking", $"Manual Soaking can only be started when the Cell is in Idle state.", EnumMessageStyle.Affirmative);
                    return;
                }

                SaveParameter();
                EventCodeEnum ret = this.SoakingModule().StartManualSoakingProc();
                if (EventCodeEnum.NONE != ret)
                {
                    LoggerManager.SoakingErrLog($"Can not start manual soaking(ret:{ret.ToString()})");
                    this.MetroDialogManager().ShowMessageDialog($"Manual soaking", $"Can not start manual soaking({ret.ToString()})", EnumMessageStyle.Affirmative);
                    return;
                }

                //manual soaking이 실제 Running state로 가는지 확인처리
                // cell에서의 manual soaking 동작이 취소되었는지 확인
                int maxWaitCnt = 60 * 5;
                int WaitCnt = 0;// 600; //일단 최장 5분
                while (true)
                {
                    if (WaitCnt > maxWaitCnt)
                    {
                        LoggerManager.SoakingErrLog($"Can not start manual soaking - Timeout");
                        break;
                    }

                    EventCodeEnum retVal = EventCodeEnum.NONE;
                    DateTime SoakingStartTm;
                    SoakingStateEnum Soaking_Status = SoakingStateEnum.UNDEFINED;
                    SoakingStateEnum Soaking_SubState = SoakingStateEnum.UNDEFINED;
                    ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
                    (retVal, SoakingStartTm, Soaking_Status, Soaking_SubState, moduleState) = this.SoakingModule().GetCurrentSoakingInfo();
                    if (ModuleStateEnum.RUNNING == moduleState || ModuleStateEnum.SUSPENDED == moduleState)
                        break;

                    //pin or wafer align 실패시
                    if(EventCodeEnum.NONE != retVal)
                    {
                        NoticeErrMsg(retVal);
                        return;
                    }

                    Thread.Sleep(1000);
                    WaitCnt++;
                }
                
                ManualSoakingStartTime = default;
                SoakedTime = "";
                RemainingTime = "";
                SetSoakingTimer(SoakMode.START);
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
                throw;
            }
        }

        private AsyncCommand<object> stopSoakingCommand;
        public IAsyncCommand StopSoakingCommand
        {
            get
            {
                if (null == stopSoakingCommand) stopSoakingCommand = new AsyncCommand<object>(StopSoakingCommandFunc);
                return stopSoakingCommand;
            }
        }

        //public void StopSoakingCommandFunc()
        private async Task StopSoakingCommandFunc(object obj)
        {
            try
            {
                int maxWaitCnt = 60 * 5;
                EventCodeEnum ret = this.SoakingModule().StopManualSoakingProc();
                if (EventCodeEnum.NONE != ret)
                {
                    LoggerManager.SoakingErrLog($"Can not stop manual soaking(ret:{ret.ToString()})");
                    this.MetroDialogManager().ShowMessageDialog($"Manual soaking", $"Manual soaking is not working", EnumMessageStyle.Affirmative);
                    return;
                }

                // cell에서의 manual soaking 동작이 취소되었는지 확인
                int WaitCnt = 0;// 600; //일단 최장 5분
                while (true)
                {
                    if(WaitCnt > maxWaitCnt)
                    {
                        LoggerManager.SoakingErrLog($"Can not stop manual soaking - Timeout");
                        break;
                    }

                    EventCodeEnum retVal = EventCodeEnum.NONE;
                    DateTime SoakingStartTm;
                    SoakingStateEnum Soaking_Status = SoakingStateEnum.UNDEFINED;
                    SoakingStateEnum Soaking_SubState = SoakingStateEnum.UNDEFINED;
                    ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
                    (retVal, SoakingStartTm, Soaking_Status, Soaking_SubState, moduleState) = this.SoakingModule().GetCurrentSoakingInfo();
                    if (ModuleStateEnum.IDLE == moduleState || ModuleStateEnum.DONE == moduleState)
                        break;

                    Thread.Sleep(1000);
                    WaitCnt++;
                }
                               
                SetSoakingTimer(SoakMode.STOP);
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
                throw;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Error발생시 Error Msg 출력처리
        /// </summary>
        /// <param name="retVal">EventCodeEnum</param>
        private void NoticeErrMsg(EventCodeEnum retVal)
        {
            LoggerManager.SoakingErrLog($"Manual Soaking Alignment is failed.(ret:{retVal.ToString()})");
            if (EventCodeEnum.NONE == retVal)
                return;

            string FailedTarget = "";
            if (EventCodeEnum.SOAKING_ERROR_IDLE_WAFERALIGN == retVal)
                FailedTarget = "(Wafer Alignment has failed.)";
            else if (EventCodeEnum.SOAKING_ERROR_IDLE_PINALIGN == retVal)
                FailedTarget = "(Pin Alignment has failed.)";

            this.MetroDialogManager().ShowMessageDialog($"Manual soaking", $"Manual soaking error{FailedTarget} - ({retVal.ToString()})", EnumMessageStyle.Affirmative);
            return;
        }

        private void ReadParameter()
        {
            ReadSoakingTime();
            ManualSoakingPolishWafer.ReadStatusSoakingParameter();
            ManualSoakingStep.ReadStatusSoakingParameter();
        }

        private void SaveParameter()
        {
            SaveSoakingTime();
            ManualSoakingPolishWafer.SaveStatusSoakingParameter();
            ManualSoakingStep.SaveStatusSoakingParameter();
        }

        private void ReadSoakingTime()
        {
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    if (_LoaderCommunicationManager == null)
                    {
                        _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                    }

                    if ((_LoaderCommunicationManager == null) || (_LoaderCommunicationManager.SelectedStage == null))
                    {                        
                        return;
                    }
                }

                double notExistWaferObj_OD = 0;
                bool enableWaitingPinAlign = false;
                int waitingPinAlignPeriod = 1800;

                var statusSoakingParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                var statusSoakingParamObj = SerializeManager.ByteToObject(statusSoakingParamByte);
                if (null != statusSoakingParamObj)
                {
                    var statusSoakingParam = statusSoakingParamObj as StatusSoakingConfig;
                    if ((statusSoakingParam != null) && (statusSoakingParam.ManualSoakingConfig != null))
                    {
                        ManualSoakingTime = statusSoakingParam.ManualSoakingConfig.SoakingTimeSec.Value;
                        
                        RecommandedSoakingTime = this.SoakingModule().GetStatusSoakingTime();
                        if (RecommandedSoakingTime < 5)
                        {
                            RecommandedSoakingTime = MinimumSoakingTime;
                        }
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Failed to 'ByteToObject'.");
                }
            }
            catch (Exception ex)
            {
                LoggerManager.SoakingErrLog($"{ex.Message}");
            }
        }

        private void SaveSoakingTime()
        {
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    if (_LoaderCommunicationManager == null)
                    {
                        _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                    }

                    if ((_LoaderCommunicationManager == null) || (_LoaderCommunicationManager.SelectedStage == null))
                    {
                        return;
                    }
                }

                var statusSoakingReadParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                var statusSoakingReadParamObj = SerializeManager.ByteToObject(statusSoakingReadParamByte);
                if (null != statusSoakingReadParamObj)
                {
                    var statusSoakingParam = statusSoakingReadParamObj as StatusSoakingConfig;
                    if ((statusSoakingParam != null) && (statusSoakingParam.PrepareStateConfig != null))
                    {
                        statusSoakingParam.ManualSoakingConfig.SoakingTimeSec.Value = ManualSoakingTime;

                        var statusSoakingSaveParamByte = SerializeManager.ObjectToByte(statusSoakingParam);
                        this.SoakingModule().SetStatusSoakingConfigParam(statusSoakingSaveParamByte);
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Failed to 'ByteToObject'.");
                }
            }
            catch (Exception ex)
            {
                LoggerManager.SoakingErrLog($"{ex.Message}");
            }
        }

        private void SetSoakingTimer(SoakMode soakMode)
        {
            switch (soakMode)
            {
                case SoakMode.START:
                    SetSoakMode(soakMode);
                    Task.Run(() => SoakingTimer());
                    break;
                case SoakMode.STOP:
                    SetSoakMode(soakMode);
                    break;
                case SoakMode.UNDEFINED:
                default:
                    SetSoakMode(SoakMode.UNDEFINED);
                    SoakedTime = "00:00:00.000";
                    RemainingTime = "00:00:00.000";
                    break;
            }            
        }

        private async Task SoakingTimer()
        {
            Action UpdateSoakedTime = () =>
            {
                if (ManualSoakingStartTime == default)
                    return;

                var CurTime = DateTime.Now;

                var soakedTime = CurTime - ManualSoakingStartTime;
                SoakedTime = string.Format($"{soakedTime.Hours.ToString("D2")}:{soakedTime.Minutes.ToString("D2")}:{soakedTime.Seconds.ToString("D2")}.{soakedTime.Milliseconds.ToString("D3")}");

                if (ManualSoakingStartTime.AddSeconds(ManualSoakingTime) > CurTime)
                {
                    var remainingTime = ManualSoakingStartTime.AddSeconds(ManualSoakingTime) - CurTime;
                    RemainingTime = string.Format($"{remainingTime.Hours.ToString("D2")}:{remainingTime.Minutes.ToString("D2")}:{remainingTime.Seconds.ToString("D2")}.{remainingTime.Milliseconds.ToString("D3")}");
                }
                else
                {
                    RemainingTime = "00:00:00.000";
                }
            };
           
            try
            {
                int loopCnt = 0;
                int CheckCount = 1;
                EventCodeEnum retVal = EventCodeEnum.NONE;
                while (SoakingMode == SoakMode.START)
                {
                    if (dispatcher.CheckAccess())
                    {
                        UpdateSoakedTime();
                    }
                    else
                    {
                        dispatcher.Invoke(() =>
                        {
                            UpdateSoakedTime();
                        });
                    }

                    Thread.Sleep(1000);
                    
                    //cell로 부터 정보를 가져와 갱신 처리
                    loopCnt++;
                    if (loopCnt > CheckCount)
                    {
                        loopCnt = 0;
                        retVal = ProcessingAccordingToCellState();
                        if (EventCodeEnum.NONE != retVal)
                            break;
                    }

                    if(ManualSoakingStartTime != default)
                    {
                        CheckCount = 6; //정보를 가져왔다면 6초마다 시간 동기화 갱신처리
                    }
                }

                if (EventCodeEnum.NONE != retVal)
                {
                    SetSoakingTimer(SoakMode.STOP);

                    if (dispatcher.CheckAccess())
                    {
                        NoticeErrMsg(retVal);
                    }
                    else
                    {
                        dispatcher.Invoke(() =>
                        {
                            NoticeErrMsg(retVal);
                        });
                    }                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"exception : {err.Message}");
            }
        }

        /// <summary>
        /// Cell이 manual soaking에 대한 상태정보 갱신 및 error 발생 시 반환
        /// </summary>
        /// <returns></returns>
        private EventCodeEnum ProcessingAccordingToCellState()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            DateTime SoakingStartTm = default;
            SoakingStateEnum Soaking_Status = SoakingStateEnum.UNDEFINED;
            SoakingStateEnum Soaking_SubState = SoakingStateEnum.UNDEFINED;
            ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
            (retVal, SoakingStartTm, Soaking_Status, Soaking_SubState, moduleState) = this.SoakingModule().GetCurrentSoakingInfo();
            if (SoakingStateEnum.MANUAL == Soaking_Status)
            {
                SoakingSubState = Soaking_SubState;
                if (SoakingStartTm != default )
                {
                    if(ManualSoakingStartTime == default)
                        ManualSoakingStartTime = SoakingStartTm;
                }

                //pin or wafer align 실패시
                if (EventCodeEnum.NONE != retVal)
                {
                    LoggerManager.SoakingErrLog($"Manual Soaking is failed.(ret:{retVal.ToString()})");
                    return retVal;
                }

                if (SoakingStateEnum.SUSPENDED_FOR_ALIGN == SoakingSubState)
                {
                    CanStartSoak = false;
                    CanStopSoak = false;
                }
                else if (ModuleStateEnum.RUNNING == moduleState || ModuleStateEnum.SUSPENDED == moduleState)
                {
                    CanStopSoak = true;
                    if (SoakingMode != SoakMode.START)
                        SetSoakingTimer(SoakMode.START);
                }

                return retVal;
            }
            else
            {
                SetSoakingTimer(SoakMode.STOP);
                return retVal;
            }
        }
        
        private void SetSoakMode(SoakMode soakMode)
        {
            switch (soakMode)
            {
                case SoakMode.START:
                    EditLockMode = Visibility.Visible;
                    CanStartSoak = false;
                    CanStopSoak = true;
                    break;
                case SoakMode.STOP:
                    EditLockMode = Visibility.Hidden;
                    CanStartSoak = true;
                    CanStopSoak = false;
                    break;
                case SoakMode.UNDEFINED:
                default:
                    EditLockMode = Visibility.Hidden;
                    CanStartSoak = true;
                    CanStopSoak = false;
                    break;
            }

            SoakingMode = soakMode;
        }
        #endregion
    }
}
