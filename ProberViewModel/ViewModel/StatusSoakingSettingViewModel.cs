using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Soaking;
using RelayCommandBase;
using VirtualKeyboardControl;
using ProberViewModel;
using LoaderBase.Communication;
using SerializerUtil;
using SoakingParameters;

namespace LoaderStatusSoakingSettingVM
{
    public class StatusSoakingSettingViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("C0479A79-DF79-437D-9E06-BE9307ACD696");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public ILoaderCommunicationManager _LoaderCommunicationManager = null;

        public bool Initialized { get; set; } = false;

        private StatusSoakingConfig StatusSoakingParam { get; set; } = null;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region Property
        #region Common
        private bool _UsePolishWafer = false;
        public bool UsePolishWafer
        {
            get
            {
                return _UsePolishWafer;
            }
            set
            {
                _UsePolishWafer = value;
                RaisePropertyChanged();
            }
        }

        private int _PolishWaferUIHeight = 0;
        public int PolishWaferUIHeight
        {
            get
            {
                return _PolishWaferUIHeight;
            }
            set
            {
                _PolishWaferUIHeight = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Prepare
        private int _PrepareSoakingTime;
        public int PrepareSoakingTime
        {
            get
            {
                return _PrepareSoakingTime;
            }
            set
            {
                _PrepareSoakingTime = value;
                RaisePropertyChanged();
            }
        }

        private int _PrepareTriggerDegreeForNormal;
        public int PrepareTriggerDegreeForNormal
        {
            get
            {
                return _PrepareTriggerDegreeForNormal;
            }
            set
            {
                _PrepareTriggerDegreeForNormal = value;
                RaisePropertyChanged();
            }
        }

        private int _PrepareTempDiffSoakingTime;
        public int PrepareTempDiffSoakingTime
        {
            get
            {
                return _PrepareTempDiffSoakingTime;
            }
            set
            {
                _PrepareTempDiffSoakingTime = value;
                RaisePropertyChanged();
            }
        }
        private bool _PrepareSoak_afterDeviceChange;
        public bool PrepareSoak_afterDeviceChange
        {
            get
            {
                return _PrepareSoak_afterDeviceChange;
            }
            set
            {
                _PrepareSoak_afterDeviceChange = value;
                RaisePropertyChanged();
            }
        }
        private int _PrepareTriggerDegreeForTempDiff;
        public int PrepareTriggerDegreeForTempDiff
        {
            get
            {
                return _PrepareTriggerDegreeForTempDiff;
            }
            set
            {
                _PrepareTriggerDegreeForTempDiff = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingPolishWaferViewModel _PrepareSoakingPolishWafer = new UcSoakingPolishWaferViewModel();
        public UcSoakingPolishWaferViewModel PrepareSoakingPolishWafer
        {
            get
            {
                return _PrepareSoakingPolishWafer;
            }
            set
            {
                _PrepareSoakingPolishWafer = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingStepViewModel _PrepareSoakingStep = new UcSoakingStepViewModel();
        public UcSoakingStepViewModel PrepareSoakingStep
        {
            get
            {
                return _PrepareSoakingStep;
            }
            set
            {
                _PrepareSoakingStep = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingODViewModel _PrepareSoakingOD = new UcSoakingODViewModel();
        public UcSoakingODViewModel PrepareSoakingOD
        {
            get
            {
                return _PrepareSoakingOD;
            }
            set
            {
                _PrepareSoakingOD = value;
                RaisePropertyChanged();
            }
        }

        private string _TooltipSoakingTime;
        public string TooltipSoakingTime
        {
            get { return _TooltipSoakingTime; }
            set
            {
                if (value != _TooltipSoakingTime)
                {
                    _TooltipSoakingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TooltipSoakingTriggerDegree;
        public string TooltipSoakingTriggerDegree
        {
            get { return _TooltipSoakingTriggerDegree; }
            set
            {
                if (value != _TooltipSoakingTriggerDegree)
                {
                    _TooltipSoakingTriggerDegree = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Recovery
        private UcSoakingPolishWaferViewModel _RecoverySoakingPolishWafer = new UcSoakingPolishWaferViewModel();
        public UcSoakingPolishWaferViewModel RecoverySoakingPolishWafer
        {
            get
            {
                return _RecoverySoakingPolishWafer;
            }
            set
            {
                _RecoverySoakingPolishWafer = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingStepViewModel _RecoverySoakingStep = new UcSoakingStepViewModel();
        public UcSoakingStepViewModel RecoverySoakingStep
        {
            get
            {
                return _RecoverySoakingStep;
            }
            set
            {
                _RecoverySoakingStep = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingODViewModel _RecoverySoakingOD = new UcSoakingODViewModel();
        public UcSoakingODViewModel RecoverySoakingOD
        {
            get
            {
                return _RecoverySoakingOD;
            }
            set
            {
                _RecoverySoakingOD = value;
                RaisePropertyChanged();
            }
        }
        private UcSoakingChillingTimeTableViewModel _RecoverySoakingChillingTimeTable = new UcSoakingChillingTimeTableViewModel();
        public UcSoakingChillingTimeTableViewModel RecoverySoakingChillingTimeTable
        {
            get
            {
                return _RecoverySoakingChillingTimeTable;
            }
            set
            {
                _RecoverySoakingChillingTimeTable = value;
                RaisePropertyChanged();
            }
        }

        #region Event Soak - Every Wafer Soak
        private UcSoakingEventSoakViewModel _EveryWaferSoak = new UcSoakingEventSoakViewModel();
        public UcSoakingEventSoakViewModel EveryWaferSoak
        {
            get
            {
                return _EveryWaferSoak;
            }
            set
            {
                _EveryWaferSoak = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Event Soak - Before Z Up Soak
        private UcSoakingEventSoakViewModel _BeforeZUpSoak = new UcSoakingEventSoakViewModel();
        public UcSoakingEventSoakViewModel BeforeZUpSoak
        {
            get
            {
                return _BeforeZUpSoak;
            }
            set
            {
                _BeforeZUpSoak = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Maintain
        private int _MaintainChuckAwayElapsedTime;
        public int MaintainChuckAwayElapsedTime
        {
            get
            {
                return _MaintainChuckAwayElapsedTime;
            }
            set
            {
                _MaintainChuckAwayElapsedTime = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingPolishWaferViewModel _MaintainSoakingPolishWafer = new UcSoakingPolishWaferViewModel();
        public UcSoakingPolishWaferViewModel MaintainSoakingPolishWafer
        {
            get
            {
                return _MaintainSoakingPolishWafer;
            }
            set
            {
                _MaintainSoakingPolishWafer = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingStepViewModel _MaintainSoakingStep = new UcSoakingStepViewModel();
        public UcSoakingStepViewModel MaintainSoakingStep
        {
            get
            {
                return _MaintainSoakingStep;
            }
            set
            {
                _MaintainSoakingStep = value;
                RaisePropertyChanged();
            }
        }

        private UcSoakingODViewModel _MaintainSoakingOD = new UcSoakingODViewModel();
        public UcSoakingODViewModel MaintainSoakingOD
        {
            get
            {
                return _MaintainSoakingOD;
            }
            set
            {
                _MaintainSoakingOD = value;
                RaisePropertyChanged();
            }
        }
        #endregion
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
                if (tb.IsReadOnly)
                {
                    return;
                }

                int oldValue = int.Parse(tb.Text);
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 5);

                switch (tb.Name)
                {
                    case "PrepareSoakingTime_tb":
                    case "PrepareTempDiffSoakingTime_tb":                   
                        if (!CheckMinValue(tb.Text, StatusSoakingParam.AdvancedSetting.ChillingSoakingTimeMin.Value))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                    case "PrepareTempDiffTriggerDegree_tb":
                        if (!CheckMinValue(tb.Text, 1))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                }

                if (String.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = "0";
                }

                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        /// <summary>
        /// Advanced 설정용 View+ViewModel
        /// </summary>
        private SoakingSettingView.UcSoakingAdvancedSetting advancedSettingView;
        private UcSoakingAdvancedSettingViewModel advancedSettingViewModel;

        private AsyncCommand _cmdShowAdvancedSettingView;
        public ICommand CmdShowAdvancedSettingView
        {
            get
            {
                if (null == _cmdShowAdvancedSettingView)
                {
                    _cmdShowAdvancedSettingView = new AsyncCommand(async () =>
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

                      var statusSoakingParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                      var statusSoakingParamObj = SerializeManager.ByteToObject(statusSoakingParamByte);
                      if (null != statusSoakingParamObj)
                      {
                          var statusSoakingParam = statusSoakingParamObj as StatusSoakingConfig;
                          if (null != statusSoakingParam)
                          {
                              StatusSoakingParam = statusSoakingParam;

                              advancedSettingViewModel.Setting = statusSoakingParam.AdvancedSetting;
                              advancedSettingViewModel.ChuckAwayToleranceLimitX = this.SoakingModule().GetChuckAwayToleranceLimitX();
                              advancedSettingViewModel.ChuckAwayToleranceLimitY = this.SoakingModule().GetChuckAwayToleranceLimitY();
                              advancedSettingViewModel.ChuckAwayToleranceLimitZ = this.SoakingModule().GetChuckAwayToleranceLimitZ();
                              await this.MetroDialogManager().ShowWindow(advancedSettingView);
                              
                              //APPLY 요청시 충돌안내 까지 표시된 이후 전달됨.  사용자 동의됨.
                              if (advancedSettingViewModel.Result == MessageBoxResult.OK) 
                              {
                                  var statusSoakingSaveParamByte = SerializeManager.ObjectToByte(statusSoakingParam);
                                  this.SoakingModule().SetStatusSoakingConfigParam(statusSoakingSaveParamByte);

                                  var verifyResult = VerifyParameterLimit(advancedSettingViewModel.Setting);
                                  if (verifyResult.Item1 != EventCodeEnum.NONE) // 충돌사항이 있다면
                                  {
                                      VerifyParameterLimit(advancedSettingViewModel.Setting, true); //변경

                                      SavePrepareParameter();
                                      SaveRecoveryParameter();
                                      SaveMaintainParameter();
                                      SaveEventSoakParameter();
                                      SaveSoakingParameter();
                                  }

                                  UpdateSoakingParameter();
                                  UpdateTooltip();
                              }
                          }
                      }
                  });
                }

                return _cmdShowAdvancedSettingView;
            }
        }

        #endregion

        #region IMainScreenViewModel
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

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

            return retval;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            InitPrepareViewModel();
            InitRecoveryViewModel();
            InitMaintainViewModel();
            InitEventSoakViewModel();
            InitAdvancedViewModel();

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            ReadPrepareParameter();
            ReadRecoveryParameter();
            ReadMaintainParameter();
            ReadEvenSoaktParameter();

            // 기타 Soaking Parameter
            ReadSoakingParameter();

            // 1. Cell에서는 Polish Wafer 설정이 불가능하여 화면을 가리도록 한다.
            // 2. Polish Wafer 사용 옵션에 따라 화면에 표시 여부가 달라진다.
            UsePolishWafer = this.SoakingModule().IsUsePolishWafer();
            if (UsePolishWafer &&
                (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote))
            {
                PolishWaferUIHeight = 110;
            }
            else
            {
                PolishWaferUIHeight = 0;
            }

            // Tooltip Setting
            UpdateTooltip();

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retPrepare = SavePrepareParameter();
            EventCodeEnum retRecovery = SaveRecoveryParameter();
            EventCodeEnum retMaintain = SaveMaintainParameter();
            EventCodeEnum retEvent = SaveEventSoakParameter();

            // 기타 Soaking Parameter
            EventCodeEnum retParam = SaveSoakingParameter();

            CheckAvailableStatusSoakingEnable();

            if ((retPrepare == EventCodeEnum.NONE) &&
                (retRecovery == EventCodeEnum.NONE) &&
                (retMaintain == EventCodeEnum.NONE) &&
                (retEvent == EventCodeEnum.NONE) &&
                (retParam == EventCodeEnum.NONE))
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            else
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.UNDEFINED);
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {GetType().Name}");
        }
        #endregion

        #region Method
        private void InitPrepareViewModel()
        {
            PrepareSoakingPolishWafer.SetStatusSoakingState(SoakingStateEnum.PREPARE);
            PrepareSoakingStep.SetStatusSoakingState(SoakingStateEnum.PREPARE);
            PrepareSoakingOD.SetStatusSoakingState(SoakingStateEnum.PREPARE);
        }

        private void InitRecoveryViewModel()
        {
            RecoverySoakingPolishWafer.SetStatusSoakingState(SoakingStateEnum.RECOVERY);
            RecoverySoakingStep.SetStatusSoakingState(SoakingStateEnum.RECOVERY);
            RecoverySoakingOD.SetStatusSoakingState(SoakingStateEnum.RECOVERY);
        }

        private void InitMaintainViewModel()
        {
            MaintainSoakingPolishWafer.SetStatusSoakingState(SoakingStateEnum.MAINTAIN);
            MaintainSoakingStep.SetStatusSoakingState(SoakingStateEnum.MAINTAIN);
            MaintainSoakingOD.SetStatusSoakingState(SoakingStateEnum.MAINTAIN);
            MaintainSoakingOD.ODValueChanged += OnODValueChanged;
        }

        private void InitEventSoakViewModel()
        {
            EveryWaferSoak.EventSoakMode = EventSoakType.EveryWaferSoak;
            BeforeZUpSoak.EventSoakMode = EventSoakType.BeforeZUpSoak;
        }

        private void UpdateAdvancedSetting(Advancedsetting setting)
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

            var statusSoakingParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
            var statusSoakingParamObj = SerializeManager.ByteToObject(statusSoakingParamByte);
            if (null != statusSoakingParamObj)
            {
                var statusSoakingParam = statusSoakingParamObj as StatusSoakingConfig;
                if (null != statusSoakingParam)
                {
                    //StatusSoakingParam = statusSoakingParam;
                    //advancedSettingViewModel.Setting = statusSoakingParam.AdvancedSetting;
                    statusSoakingParam.AdvancedSetting = setting;

                    var statusSoakingSaveParamByte = SerializeManager.ObjectToByte(statusSoakingParam);
                    this.SoakingModule().SetStatusSoakingConfigParam(statusSoakingSaveParamByte);

                }
            }

        }

        private void InitAdvancedViewModel()
        {
            advancedSettingView = new SoakingSettingView.UcSoakingAdvancedSetting();
            advancedSettingViewModel = new UcSoakingAdvancedSettingViewModel(advancedSettingView, advancedSettingView.GetType().Name);
            advancedSettingView.DataContext = advancedSettingViewModel;
            advancedSettingViewModel.Validator = VerifyParameterLimit;
        }
       

        private void ReadPrepareParameter()
        {
            PrepareSoakingPolishWafer.ReadStatusSoakingParameter();
            PrepareSoakingStep.ReadStatusSoakingParameter();
            PrepareSoakingOD.ReadStatusSoakingParameter();
        }

        private void ReadRecoveryParameter()
        {
            RecoverySoakingPolishWafer.ReadStatusSoakingParameter();
            RecoverySoakingStep.ReadStatusSoakingParameter();
            RecoverySoakingOD.ReadStatusSoakingParameter();
            RecoverySoakingChillingTimeTable.ReadStatusSoakingParameter();
        }

        private void ReadMaintainParameter()
        {
            MaintainSoakingPolishWafer.ReadStatusSoakingParameter();
            MaintainSoakingStep.ReadStatusSoakingParameter();
            MaintainSoakingOD.ReadStatusSoakingParameter();
        }

        private void ReadEvenSoaktParameter()
        {
            EveryWaferSoak.ReadStatusSoakingParameter();
            BeforeZUpSoak.ReadStatusSoakingParameter();
        }

        private EventCodeEnum SavePrepareParameter()
        {
            EventCodeEnum retPolishWafer = EventCodeEnum.NONE;
            if (UsePolishWafer && (PolishWaferUIHeight != 0))
            {
                retPolishWafer = PrepareSoakingPolishWafer.SaveStatusSoakingParameter();
            }
            EventCodeEnum retSoaknigStep = PrepareSoakingStep.SaveStatusSoakingParameter();
            EventCodeEnum retSoakingOD = PrepareSoakingOD.SaveStatusSoakingParameter();

            if ((retPolishWafer == EventCodeEnum.NONE) && (retSoaknigStep == EventCodeEnum.NONE) && (retSoakingOD == EventCodeEnum.NONE))
            {
                return EventCodeEnum.NONE;
            }
            else
            {
                return EventCodeEnum.UNDEFINED;
            }
        }

        private EventCodeEnum SaveRecoveryParameter()
        {
            EventCodeEnum retPolishWafer = EventCodeEnum.NONE;
            if (UsePolishWafer && (PolishWaferUIHeight != 0))
            {
                retPolishWafer = RecoverySoakingPolishWafer.SaveStatusSoakingParameter();
            }
            EventCodeEnum retChillingTime = RecoverySoakingChillingTimeTable.SaveStatusSoakingParameter();
            EventCodeEnum retSoaknigStep = RecoverySoakingStep.SaveStatusSoakingParameter();
            EventCodeEnum retSoakingOD = RecoverySoakingOD.SaveStatusSoakingParameter();

            if ((retChillingTime == EventCodeEnum.NONE) && (retPolishWafer == EventCodeEnum.NONE) && (retSoaknigStep == EventCodeEnum.NONE) && (retSoakingOD == EventCodeEnum.NONE))
            {
                return EventCodeEnum.NONE;
            }
            else
            {
                return EventCodeEnum.UNDEFINED;
            }
        }

        private EventCodeEnum SaveMaintainParameter()
        {
            EventCodeEnum retPolishWafer = EventCodeEnum.NONE;
            if (UsePolishWafer && (PolishWaferUIHeight != 0))
            {
                retPolishWafer = MaintainSoakingPolishWafer.SaveStatusSoakingParameter();
            }
            EventCodeEnum retSoaknigStep = MaintainSoakingStep.SaveStatusSoakingParameter();
            EventCodeEnum retSoakingOD = MaintainSoakingOD.SaveStatusSoakingParameter();

            if ((retPolishWafer == EventCodeEnum.NONE) && (retSoaknigStep == EventCodeEnum.NONE) && (retSoakingOD == EventCodeEnum.NONE))
            {
                return EventCodeEnum.NONE;
            }
            else
            {
                return EventCodeEnum.UNDEFINED;
            }
        }

        private EventCodeEnum SaveEventSoakParameter()
        {
            EventCodeEnum retEveryWafer = EveryWaferSoak.SaveStatusSoakingParameter();
            EventCodeEnum retBeforeZUp = BeforeZUpSoak.SaveStatusSoakingParameter();

            if ((retEveryWafer == EventCodeEnum.NONE) && (retBeforeZUp == EventCodeEnum.NONE))
            {
                return EventCodeEnum.NONE;
            }
            else
            {
                return EventCodeEnum.UNDEFINED;
            }
        }

        private void OnODValueChanged(object sender, double newOD)
        {
            StatusSoakingParam = LoadSoakingParameter();
            if (StatusSoakingParam == null)
            {
                return;
            }

            var baseOD = (int)newOD;
            var chuckAwayZ = (int)StatusSoakingParam.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;

            var odMax = StatusSoakingParam.AdvancedSetting.ODLimit.Value;
            var odMin = baseOD - chuckAwayZ;

            UpdateSoakingParameter(StatusSoakingParam);
            UpdateTooltip();

            var ret = VerifyParameterODRange(odMin, odMax, false);
            if (ret.Item1 == EventCodeEnum.NONE)
            {
                return;
            }

            VerifyParameterODRange(odMin, odMax, true);

            string msg = string.Format($"Changed \"OD / Z Clearance\" values in some SoakingStep tables.\r\n\r\n" +
                                       $"Detail :\r\n" +
                                       ret.Item2);

            this.MetroDialogManager().ShowMessageDialog("Warning", msg, EnumMessageStyle.Affirmative);

            return;
        }

        #region Verify Method
        private (EventCodeEnum, string) VerifyParameter()
        {
            StringBuilder retMsg = new StringBuilder();
            List<(EventCodeEnum, string)> results = new List<(EventCodeEnum, string)>();

            // Prepare
            results.Add(PrepareSoakingStep.VerifySoakingStepTable("Pre-Heating"));

            // Recovery
            results.Add(RecoverySoakingChillingTimeTable.VerifyChillingTimeTable("Recovery"));
            results.Add(RecoverySoakingStep.VerifySoakingStepTable("Recovery"));

            // Maintain
            results.Add(MaintainSoakingStep.VerifySoakingStepTable("Maintain"));

            foreach (var ret in results)
            {
                if (ret.Item1 != EventCodeEnum.NONE)
                {
                    retMsg.Append(ret.Item2);
                    retMsg.Append("\r\n");
                }
            }

            var retMsgStr = retMsg.ToString();
            if (string.IsNullOrEmpty(retMsgStr))
            {
                return (EventCodeEnum.NONE, retMsgStr);
            }
            else
            {
                return (EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID, retMsgStr);
            }
        }

        public void CheckAvailableStatusSoakingEnable()
        {
            // VerifyParameter에서 실패하면 Soaking을 Enable : False 상태로 변경한다.
            var verityRet = VerifyParameter();
            if (verityRet.Item1 != EventCodeEnum.NONE)
            {
                StatusSoakingParam = LoadSoakingParameter();
                if ((null != StatusSoakingParam) && StatusSoakingParam.UseStatusSoaking.Value)
                {
                    string msg = string.Format($"Failed to update soaking parameter.\r\n" +
                                               $"Disable status soak.\r\n" + 
                                               $"Please check the soaking parameter.\r\n\r\n" +
                                               $"Detail :\r\n" +
                                               verityRet.Item2);
                    this.MetroDialogManager().ShowMessageDialog("Warning", msg, EnumMessageStyle.Affirmative);

                    StatusSoakingParam.UseStatusSoaking.Value = false;

                    if (LoaderCommunicationCheck() == EventCodeEnum.NONE)
                    {
                        try
                        {
                            var statusSoakingSaveParamByte = SerializeManager.ObjectToByte(StatusSoakingParam);
                            this.SoakingModule().SetStatusSoakingConfigParam(statusSoakingSaveParamByte);

                            List<IElement> _DeviceParamElementList = null;
                            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                            {
                                _DeviceParamElementList = this.ParamManager()?.GetDevElementList();
                            }
                            else
                            {
                                _DeviceParamElementList = this.GetLoaderContainer().Resolve<ILoaderParamManager>().GetDevElementList();
                            }

                            string useStatusSoakingName = nameof(StatusSoakingParam.UseStatusSoaking);
                            var element = _DeviceParamElementList?.FirstOrDefault(x => x.PropertyPath.EndsWith(useStatusSoakingName));
                            if (element != null)
                            {
                                element.SetValue(false);
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.SoakingErrLog($"{err.Message}");
                        }
                    }
                }
            }
        }

        private (EventCodeEnum, string) VerifyParameterODRange(int odMin, int odMax, bool bSetValue = false)
        {
            var ret = EventCodeEnum.NONE;
            StringBuilder retMsg = new StringBuilder();

            if (!VerifyParameterODRangeSub("Pre-Heating", PrepareSoakingStep.SoakingSteps, odMin, odMax, bSetValue))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Pre-Heating SoakingStep Table] OD");
            }

            if (!VerifyParameterODRangeSub("Recovery", RecoverySoakingStep.SoakingSteps, odMin, odMax, bSetValue))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Recovery SoakingStep Table] OD");
            }

            if (!VerifyParameterODRangeSub("Maintain", MaintainSoakingStep.SoakingSteps, odMin, odMax, bSetValue))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Maintain SoakingStep Table] OD");
            }
            
            return (ret, retMsg.ToString());
        }

        private bool VerifyParameterODRangeSub(string objectName, ObservableCollection<SoakingStep> soakingSteps, int odMin, int odMax, bool bSetValue = false)
        {
            bool bVerify = true;
            if (soakingSteps == null)
            {
                return bVerify;
            }

            try
            {
                foreach (var soakingStep in soakingSteps)
                {
                    if (soakingStep.OD < odMin)
                    {
                        bVerify = false;

                        if (bSetValue)
                        {
                            LoggerManager.SoakingLog($"Verify OD & Value changed [{objectName}] [BEFORE] {soakingStep.OD}, [AFTER] {odMin}, [RANGE] {odMin} ~ {odMax}");
                            soakingStep.OD = odMin;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (soakingStep.OD > odMax)
                    {
                        bVerify = false;

                        if (bSetValue)
                        {
                            LoggerManager.SoakingLog($"Verify OD & Value changed [{objectName}] [BEFORE] {soakingStep.OD}, [AFTER] {odMax}, [RANGE] {odMin} ~ {odMax}");
                            soakingStep.OD = odMax;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return bVerify;
        }

        /// <summary>
        /// Advanced Setting에서 Limit 값이 바뀌었을때 Soaking Table에서 Limit을 충족하는지 확인하기 위한 Func.
        /// </summary>
        /// <param name="advancedSettingParam"></param>
        /// <param name="bSetValue"></param>
        /// <returns></returns>
        private (EventCodeEnum, string) VerifyParameterLimit(Advancedsetting advancedSettingParam, bool bSetValue = false)
        {
            var ret = EventCodeEnum.NONE;
            var retMsg = new StringBuilder();

            var chillingTimeMin = advancedSettingParam.ChillingTimeMin.Value;
            var chillingSoakingTimeMin = advancedSettingParam.ChillingSoakingTimeMin.Value;
            var soakingStepTimeMin = advancedSettingParam.SoakingStepTimeMin.Value;
            var odMax = StatusSoakingParam.AdvancedSetting.ODLimit.Value;
            var odMin = StatusSoakingParam.MaintainStateConfig.NotExistWaferObj_OD.Value - StatusSoakingParam.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;

            // Prepare
            if (!VerifySoakingStepLimitFunc("Pre-Heating", PrepareSoakingStep.SoakingSteps, bSetValue, soakingStepTimeMin, (int)odMin, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Pre-Heating SoakingStep Table]");
            }
            
            if (!VerifyODLimitFunc("Pre-Heating", PrepareSoakingOD, bSetValue, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Pre-Heating OD / Z Clearance]");
            }

            // Recovery
            if (!VerifyChillingTableLimitFunc("Recovery", RecoverySoakingChillingTimeTable.ChillingTimeTable, bSetValue, chillingTimeMin, chillingSoakingTimeMin))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Recovery ChillingTime Table]");
            }
            
            if (!VerifySoakingStepLimitFunc("Recovery", RecoverySoakingStep.SoakingSteps, bSetValue, soakingStepTimeMin, (int)odMin, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Recovery SoakingStep Table]");
            }

            if (!VerifyODLimitFunc("Recovery", RecoverySoakingOD, bSetValue, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Recovery OD / Z Clearance]");
            }

            // Maintain
            if (!VerifySoakingStepLimitFunc("Maintain", MaintainSoakingStep.SoakingSteps, bSetValue, soakingStepTimeMin, (int)odMin, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Maintain SoakingStep Table]");
            }

            if (!VerifyODLimitFunc("Maintain", MaintainSoakingOD, bSetValue, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Maintain OD / Z Clearance]");
            }

            // Event Soak
            if (!VerifyEventSoakingLimitFunc("EveryWafer", EveryWaferSoak, bSetValue, soakingStepTimeMin, (int)odMin, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Every Wafer]");
            }

            if (!VerifyEventSoakingLimitFunc("BeforeZUp", BeforeZUpSoak, bSetValue, soakingStepTimeMin, (int)odMin, odMax))
            {
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                retMsg.AppendLine("[Before Z Up]");
            }

            return (ret, retMsg.ToString());
        }
        private bool VerifyParameterLimit(object advancedSetting, out string msg)
        {
            bool ret = false;
            msg = "---FAIL---";

            if (advancedSetting is Advancedsetting s)
            {
                var verifyResult = VerifyParameterLimit(s);

                if (verifyResult.Item1 != EventCodeEnum.NONE)
                {
                    var chillingTimeMin = advancedSettingViewModel.Setting.ChillingTimeMin.Value;
                    var chillingSoakingTimeMin = advancedSettingViewModel.Setting.ChillingSoakingTimeMin.Value;
                    var soakingStepTimeMin = advancedSettingViewModel.Setting.SoakingStepTimeMin.Value;
                    var odLimit = advancedSettingViewModel.Setting.ODLimit.Value;

                    msg = string.Format($"Some item values do not match the limit.\r\n" +
                                              $"Press \"OK\" to change the value to the limit.\r\n\r\n" +
                                              $"Chilling Time(Min) : {chillingTimeMin}\r\n" +
                                              $"Soaking Time(Min) : {chillingSoakingTimeMin}\r\n" +
                                              $"Soaking Step Time(Min) : {soakingStepTimeMin}\r\n" +
                                              $"OD / Z Clearance(Max) : {odLimit}\r\n\r\n" +
                                              $"Detail :\r\n" +
                                              verifyResult.Item2);
                }
                else
                {
                    ret = true;
                }
            }
            return ret;
        }
        private bool VerifyChillingTableLimitFunc(string objectName, ObservableCollection<Chillingtimetable> ChillingTimeTable, bool bSetValue, int chillingTimeMin, int chillingSoakingTimeMin)
        {
            bool bVerify = true;
            if (ChillingTimeTable == null)
            {
                return bVerify;
            }

            try
            {
                foreach (var chillingTime in ChillingTimeTable)
                {
                    // Chilling Time
                    if (chillingTime.ChillingTimeSec.Value < chillingTimeMin)
                    {
                        bVerify = false;
                        LoggerManager.SoakingLog($"VerifyChillingTableLimitFunc check fail. [{objectName}] ChillingTime[{chillingTime.ChillingTimeSec}], ChillingTime Min.[{chillingTimeMin}]");

                        if (bSetValue)
                        {
                            chillingTime.ChillingTimeSec.Value = chillingTimeMin;
                        }
                    }

                    if (!bSetValue && !bVerify)
                    {
                        break;
                    }

                    // Soaking Time
                    if (chillingTime.SoakingTimeSec.Value < chillingSoakingTimeMin)
                    {
                        bVerify = false;
                        LoggerManager.SoakingLog($"VerifyChillingTableLimitFunc check fail. [{objectName}] ChillingSoakingTime[{chillingTime.SoakingTimeSec}], ChillingSoakingTime Min.[{chillingSoakingTimeMin}]");

                        if (bSetValue)
                        {
                            chillingTime.SoakingTimeSec.Value = chillingSoakingTimeMin;
                        }
                    }

                    if (!bSetValue && !bVerify)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return bVerify;
        }

        private bool VerifySoakingStepLimitFunc(string objectName, ObservableCollection<SoakingStep> soakingSteps, bool bSetValue, int soakingStepTimeMin, int odMin, int odMax)
        {
            bool bVerify = true;
            if (soakingSteps == null)
            {
                return bVerify;
            }

            try
            {
                foreach (var soakingStep in soakingSteps)
                {
                    // Soaking Time
                    if (soakingStep.SoakingTimeSec != UcSoakingStepViewModel.RemainTime)
                    {
                        if (int.Parse(soakingStep.SoakingTimeSec) < soakingStepTimeMin)
                        {
                            bVerify = false;
                            LoggerManager.SoakingLog($"VerifySoakingStepLimit check fail. [{objectName}] SoakingTime[{soakingStep.SoakingTimeSec}], SoakingStepTime Min.[{soakingStepTimeMin}]");

                            if (bSetValue)
                            {
                                soakingStep.SoakingTimeSec = soakingStepTimeMin.ToString();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    // OD Min
                    if (soakingStep.OD < odMin)
                    {
                        bVerify = false;
                        LoggerManager.SoakingLog($"VerifySoakingStepLimit check fail. [{objectName}] OD[{soakingStep.OD}], OD Min[{odMin}]");

                        if (bSetValue)
                        {
                            LoggerManager.SoakingLog($"Verify OD & Value changed [{objectName}] [BEFORE] {soakingStep.OD}, [AFTER] {odMin}, [RANGE] {odMin} ~ {odMax}");
                            soakingStep.OD = odMin;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // OD Max
                    if (soakingStep.OD > odMax)
                    {
                        bVerify = false;
                        LoggerManager.SoakingLog($"VerifySoakingStepLimit check fail. [{objectName}] OD[{soakingStep.OD}], OD Max[{odMax}]");

                        if (bSetValue)
                        {
                            LoggerManager.SoakingLog($"Verify OD & Value changed [{objectName}] [BEFORE] {soakingStep.OD}, [AFTER] {odMax}, [RANGE] {odMin} ~ {odMax}");
                            soakingStep.OD = odMax;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return bVerify;
        }

        private bool VerifyODLimitFunc(string objectName, UcSoakingODViewModel soakingOD, bool bSetValue, int odLimit)
        {
            bool bVerify = true;
            if (soakingOD == null)
            {
                return bVerify;
            }

            try
            {
                // OD
                if (soakingOD.NotExistWaferObj_OD > odLimit)
                {
                    bVerify = false;
                    LoggerManager.SoakingLog($"VerifyODLimitFunc check fail. [{objectName}] NotExistWafer OD[{soakingOD.NotExistWaferObj_OD}], OD Limit[{odLimit}]");

                    if (bSetValue)
                    {
                        soakingOD.NotExistWaferObj_OD = odLimit;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return bVerify;
        }

        private bool VerifyEventSoakingLimitFunc(string objectName, UcSoakingEventSoakViewModel eventSoak, bool bSetValue, int soakingTimeMin, int odMin, int odMax)
        {
            bool bVerify = true;

            try
            {
                // Soaking Time
                if (eventSoak.SoakingTime < soakingTimeMin)
                {
                    bVerify = false;
                    LoggerManager.SoakingLog($"VerifyEventSoakingLimitFunc check fail. [{objectName}] SoakingTime[{eventSoak.SoakingTime}], SoakingTime Min.[{soakingTimeMin}]");

                    if (bSetValue)
                    {
                        eventSoak.SoakingTime = soakingTimeMin;
                    }
                }

                // OD Min
                if (eventSoak.OD < odMin)
                {
                    bVerify = false;
                    LoggerManager.SoakingLog($"VerifyEventSoakingLimitFunc check fail. [{objectName}] EventSoak OD[{eventSoak.OD}], OD Min[{odMin}]");

                    if (bSetValue)
                    {
                        eventSoak.OD = odMin;
                    }
                }

                // OD Max
                if (eventSoak.OD > odMax)
                {
                    bVerify = false;
                    LoggerManager.SoakingLog($"VerifyEventSoakingLimitFunc check fail. [{objectName}] EventSoak OD[{eventSoak.OD}], OD Max[{odMax}]");

                    if (bSetValue)
                    {
                        eventSoak.OD = odMax;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return bVerify;
        }

        private EventCodeEnum ReadSoakingParameter()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = LoaderCommunicationCheck();
                if (EventCodeEnum.NONE != ret)
                {
                    return ret;
                }

                double notExistWaferObj_OD = 0;
                bool enableWaitingPinAlign = false;
                int waitingPinAlignPeriod = 1800;

                var statusSoakingParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                var statusSoakingParamObj = SerializeManager.ByteToObject(statusSoakingParamByte);
                if (null != statusSoakingParamObj)
                {
                    var statusSoakingParam = statusSoakingParamObj as StatusSoakingConfig;
                    StatusSoakingParam = statusSoakingParam;

                    // Prepare
                    if ((statusSoakingParam != null) && (statusSoakingParam.PrepareStateConfig != null))
                    {
                        PrepareSoakingTime = statusSoakingParam.PrepareStateConfig.SoakingTimeSec.Value;
                        PrepareTempDiffSoakingTime = statusSoakingParam.PrepareStateConfig.TempDiffSoakingTimeSec.Value;
                        PrepareTriggerDegreeForTempDiff = statusSoakingParam.PrepareStateConfig.TriggerDegreeForTempDiff.Value;
                        PrepareSoak_afterDeviceChange = this.SoakingModule().Get_PrepareStatusSoak_after_DeviceChange();
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Failed to 'ByteToObject'.");
                    ret = EventCodeEnum.SOAKING_ERROR_NULL_DATA;
                }
            }
            catch (Exception ex)
            {
                LoggerManager.SoakingErrLog($"{ex.Message}");
                ret = EventCodeEnum.UNDEFINED;
            }

            return ret;
        }
        #endregion

        /// <summary>
        /// 바뀐 설정을 업데이트. 
        /// 개별 뷰에서 제약조건이 설정되는 Limit 항목들이 주 대상. 
        /// </summary>
        private void UpdateSoakingParameter(StatusSoakingConfig soakingParam = null)
        {
            //적용대상
            List<UcStatusSoakingViewModelBase> statusSoakingViewModels = new List<UcStatusSoakingViewModelBase>()
            {
                PrepareSoakingStep,
                PrepareSoakingOD,
                RecoverySoakingChillingTimeTable,
                RecoverySoakingStep,
                RecoverySoakingOD,
                MaintainSoakingStep,
                MaintainSoakingOD,
                EveryWaferSoak,
                BeforeZUpSoak,
            };

            if (soakingParam == null)
            {
                //1개의 뷰를 대상으로 설정 업데이트
                var baseView = statusSoakingViewModels.Where(x => x != null).FirstOrDefault();

                if (baseView != null)
                {
                    baseView.UpdateStatusSoakingConfig();//설정 최신화
                    statusSoakingViewModels.ForEach(x => x?.UpdateStatusSoakingConfig(baseView.StatusSoakingParam));//설정 전파
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Why all SokingViewModels are NULL?");
                }
            }
            else
            {
                statusSoakingViewModels.ForEach(x => x?.UpdateStatusSoakingConfig(soakingParam));//설정 전파
            }
        }

        private EventCodeEnum SaveSoakingParameter()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = LoaderCommunicationCheck();
                if (EventCodeEnum.NONE != ret)
                {
                    return ret;
                }

                var statusSoakingReadParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                var statusSoakingReadParamObj = SerializeManager.ByteToObject(statusSoakingReadParamByte);
                if (null != statusSoakingReadParamObj)
                {
                    var statusSoakingParam = statusSoakingReadParamObj as StatusSoakingConfig;
                    StatusSoakingParam = statusSoakingParam;
                    
                    if (statusSoakingParam != null)
                    {
                        // Prepare
                        if (statusSoakingParam.PrepareStateConfig != null)
                        {
                            statusSoakingParam.PrepareStateConfig.SoakingTimeSec.Value = PrepareSoakingTime;
                            statusSoakingParam.PrepareStateConfig.TempDiffSoakingTimeSec.Value = PrepareTempDiffSoakingTime;
                            statusSoakingParam.PrepareStateConfig.TriggerDegreeForTempDiff.Value = PrepareTriggerDegreeForTempDiff;
                            this.SoakingModule().Set_PrepareStatusSoak_after_DeviceChange(PrepareSoak_afterDeviceChange);
                        }

                        var statusSoakingSaveParamByte = SerializeManager.ObjectToByte(statusSoakingParam);
                        this.SoakingModule().SetStatusSoakingConfigParam(statusSoakingSaveParamByte);
                        this.SoakingModule().SaveSysParameter();
                    }
                    else
                    {
                        LoggerManager.SoakingErrLog($"StatusSoakingConfig is null");
                        ret = EventCodeEnum.SOAKING_ERROR_NULL_DATA;
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Failed to 'ByteToObject'.");
                    ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                }
            }
            catch (Exception ex)
            {
                LoggerManager.SoakingErrLog($"{ex.Message}");
                ret = EventCodeEnum.UNDEFINED;
            }

            return ret;
        }

        private StatusSoakingConfig LoadSoakingParameter()
        {
            StatusSoakingConfig statusSoakingConfig = null;
            try
            {
                EventCodeEnum ret = LoaderCommunicationCheck();
                if (EventCodeEnum.NONE != ret)
                {
                    return null;
                }

                var statusSoakingReadParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                var statusSoakingReadParamObj = SerializeManager.ByteToObject(statusSoakingReadParamByte);
                if (null != statusSoakingReadParamObj)
                {
                    statusSoakingConfig = statusSoakingReadParamObj as StatusSoakingConfig;
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

            return statusSoakingConfig;
        }
        
        private EventCodeEnum LoaderCommunicationCheck()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;

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
                        ret = EventCodeEnum.UNDEFINED;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return ret;
        }

        private bool CheckMinValue(string src, int minValue)
        {
            bool bRet = true;
            try
            {
                int.TryParse(src, out int value);
                if (value < minValue)
                {
                    bRet = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.Message}");
            }

            return bRet;
        }

        public void UpdateTooltip()
        {
            UpdatePrepareTooltip();
            UpdateRecoveryTooltip();
            UpdateMaintainTooltip();
            UpdateEventSoakTooltip();

            if (StatusSoakingParam == null)
            {
                return;
            }

            TooltipSoakingTime = string.Format($"Minimum Value : {StatusSoakingParam.AdvancedSetting.ChillingSoakingTimeMin.Value}");
            TooltipSoakingTriggerDegree = string.Format($"Minimum Value : 1");
        }

        private void UpdatePrepareTooltip()
        {
            PrepareSoakingStep.UpdateTooltip();
            PrepareSoakingOD.UpdateTooltip();
        }

        private void UpdateRecoveryTooltip()
        {
            RecoverySoakingChillingTimeTable.UpdateTooltip();
            RecoverySoakingStep.UpdateTooltip();
            RecoverySoakingOD.UpdateTooltip();
        }

        private void UpdateMaintainTooltip()
        {
            MaintainSoakingStep.UpdateTooltip();
            MaintainSoakingOD.UpdateTooltip();
        }

        private void UpdateEventSoakTooltip()
        {
            EveryWaferSoak.UpdateTooltip();
            BeforeZUpSoak.UpdateTooltip();
        }
        #endregion
    }
}