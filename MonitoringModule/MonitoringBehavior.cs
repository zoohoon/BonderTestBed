namespace MonitoringModule
{
    using LogModule;
    using MetroDialogInterfaces;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Monitoring;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    [Serializable]
    public abstract class MonitoringBehavior : IMonitoringBehavior
    {
        public abstract bool SystemErrorType { get; set; }

        public abstract bool IsError { get; set; }
        public abstract bool PauseOnError { get; set; }
        public abstract bool ImmediatePauseOnError { get; set; }

        public abstract string Name { get; set; }
        public abstract string ErrorDescription { get; set; }

        public abstract EventCodeEnum ErrorCode { get; set; }

        public abstract EventCodeEnum InitModule();

        public abstract EventCodeEnum ErrorOccurred(EventCodeEnum eventCode);

        public abstract void ClearError();

        public abstract EventCodeEnum Monitoring();

        public abstract Element<string> BehaviorClassName { get; set; }

        public abstract bool CanManualRecovery { get; set; }

        public abstract void ManualRecovery();

        public abstract string RecoveryDescription { get; set; }

        public EventCodeEnum IsMainternanceMode()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                GPCellModeEnum Mode = this.StageSupervisor().StageMode;
                if (Mode == GPCellModeEnum.OFFLINE || Mode == GPCellModeEnum.MAINTENANCE)
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog(
                            "Error Message",
                            "You can move pages only when stage is in maintanance mode.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public abstract List<string> PreCheckRecoveryBehaviors { get; set; }

        public abstract bool ShowMessageDialog { get; set; }
    }

    [Serializable]
    public class CheckEMGFromLoader : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckEMGFromLoader" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "EMG";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _IsGPEMOButtonOn;
        [JsonIgnore]
        public bool IsGPEMOButtonOn
        {
            get { return _IsGPEMOButtonOn; }
            set { _IsGPEMOButtonOn = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Please restart the loader and prober program after EMG release.";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                SystemErrorType = true;
                IsGPEMOButtonOn = false;
                CanManualRecovery = false;
                ShowMessageDialog = true;
                ErrorDescription = "";
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                this.MotionManager().StageEMGAmpDisable();
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsGPEMOButtonOn)
                {
                    ret = EventCodeEnum.MONITORING_LOADER_EMG_STOP;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ManualRecovery()
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckMainAirFromLoader : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckMainAirFromLoader" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "MainAIR";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _IsGPLoaderMainAirEmergency;
        [JsonIgnore]
        public bool IsGPLoaderMainAirEmergency
        {
            get { return _IsGPLoaderMainAirEmergency; }
            set { _IsGPLoaderMainAirEmergency = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Machine initialization";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                SystemErrorType = true;
                CanManualRecovery = true;
                IsGPLoaderMainAirEmergency = false;
                ret = EventCodeEnum.NONE;
                ShowMessageDialog = true;
                ErrorDescription = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                this.MotionManager().StageAxisLock();
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsGPLoaderMainAirEmergency)
                {
                    ret = EventCodeEnum.LOADER_MAIN_AIR_ERROR;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ManualRecovery()
        {
            try
            {
                EventCodeEnum ret = IsMainternanceMode();
                if (ret == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().DoSystemInit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckMainVacFromLoader : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckMainVacFromLoader" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "MainVAC";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _IsGPLoaderVacuumEmergency;
        [JsonIgnore]
        public bool IsGPLoaderVacuumEmergency
        {
            get { return _IsGPLoaderVacuumEmergency; }
            set { _IsGPLoaderVacuumEmergency = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Machine initialization";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                SystemErrorType = true;
                CanManualRecovery = true;
                IsGPLoaderVacuumEmergency = false;
                ShowMessageDialog = true;
                ret = EventCodeEnum.NONE;
                ErrorDescription = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                this.MotionManager().StageAxisLock();
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsGPLoaderVacuumEmergency)
                {
                    ret = EventCodeEnum.LOADER_MAIN_VAC_ERROR;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ManualRecovery()
        {
            try
            {
                EventCodeEnum ret = IsMainternanceMode();
                if (ret == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().DoSystemInit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckStageAxesState : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckStageAxesState" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "Axes";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Machine initialization";
                IsError = false;
                PauseOnError = true;
                ImmediatePauseOnError = true;
                CanManualRecovery = true;
                SystemErrorType = true;
                ErrorDescription = "";
                // 먼저 체크해야할 Behavior 등록.
                PreCheckRecoveryBehaviors.Add(new CheckEMGFromLoader().Name);
                ShowMessageDialog = true;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                ret = this.MotionManager().StageAxisLock();
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Stage_Axes_State_Error, eventCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
                ErrorDescription = $"";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.State == EnumAxisState.ERROR || axis.Status.State == EnumAxisState.INVALID ||
                    axis.Status.State == EnumAxisState.DISABLED)
                    {
                        ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                        ErrorDescription = $" Axis State: {axis.Status.State}, Axis '{axis.AxisType.Value}' error.";
                        break;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                        //ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public override void ManualRecovery()
        {
            try
            {
                EventCodeEnum ret = IsMainternanceMode();
                if (ret == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().DoSystemInit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckChuckVacuum : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckChuckVacuum" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "ChuckVAC";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private static int ChuckVacuumErrorCount;
        private const int ErrorCountTolerance = 5;

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Machine initialization";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                CanManualRecovery = true;
                ChuckVacuumErrorCount = 0;
                SystemErrorType = true;
                ShowMessageDialog = true;
                ErrorDescription = "";

                // 먼저 체크해야할 Behavior 등록.
                PreCheckRecoveryBehaviors.Add(new CheckMainVacFromLoader().Name);

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                if (eventCode == EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR);
                }
                else if (eventCode == EventCodeEnum.MONITORING_CHUCK_8VAC_ERROR)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MONITORING_CHUCK_8VAC_ERROR);
                }
                else if (eventCode == EventCodeEnum.MONITORING_CHUCK_12VAC_ERROR)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MONITORING_CHUCK_12VAC_ERROR);
                }
                else
                {
                    LoggerManager.Debug($"{Name}.ErrorOccurred() evnetCode = {eventCode}");
                }
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
                this.MotionManager().StageAxisLock();

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccurred = false;
            List<string> chuckVacuumErrorList = new List<string>();

            try
            {
                var physinfo = this.StageSupervisor().WaferObject.GetPhysInfo();
                bool inch6 = false;
                bool inch8 = false;
                bool inch12 = false;
                IORet ioreturnValue = IORet.ERROR;

                try
                {
                    bool isthreelegdown = false;
                    this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegdown);
                    if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                        this.WaferTransferModule().ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                        isthreelegdown == true)
                    {
                        if (this.StageSupervisor().StageModuleState.GetState() != StageStateEnum.MANUAL)
                        {
                            if (physinfo.WaferSizeEnum == EnumWaferSize.UNDEFINED)
                            {
                                if (physinfo.WaferSize_um.Value == 150000)
                                {
                                    physinfo.WaferSizeEnum = EnumWaferSize.INCH6;
                                }
                                else if (physinfo.WaferSize_um.Value == 200000)
                                {
                                    physinfo.WaferSizeEnum = EnumWaferSize.INCH8;
                                }
                                else if (physinfo.WaferSize_um.Value == 300000)
                                {
                                    physinfo.WaferSizeEnum = EnumWaferSize.INCH12;
                                }
                            }

                            if (this.StageSupervisor().CheckUsingHandler(this.LoaderController().GetChuckIndex()) && this.StageSupervisor().StageModuleState.IsHandlerholdWafer() == true)
                            {
                                Thread.Sleep(2000);
                                return EventCodeEnum.NONE;
                            }

                            switch (physinfo.WaferSizeEnum)
                            {
                                case EnumWaferSize.INVALID:
                                    break;
                                case EnumWaferSize.UNDEFINED:
                                    break;
                                case EnumWaferSize.INCH6:
                                    ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, out inch6);
                                    if (inch6 == false || this.IOManager().IO.Outputs.DOCHUCKAIRON_0.Value == false || this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_0.Value == false)
                                    {
                                        ret = EventCodeEnum.NONE;
                                    }
                                    else
                                    {
                                        bErrorOccurred = true;
                                        if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                        {
                                            ret = EventCodeEnum.NONE;
                                            LoggerManager.Debug($"ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");
                                        }
                                        else
                                        {
                                            ret = EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR;
                                        }
                                    }
                                    break;
                                case EnumWaferSize.INCH8:
                                    ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, out inch6);
                                    if (inch6 == false || this.IOManager().IO.Outputs.DOCHUCKAIRON_0.Value == false)
                                    {
                                        ret = EventCodeEnum.NONE;
                                        ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, out inch8);
                                        if (inch8 == false || this.IOManager().IO.Outputs.DOCHUCKAIRON_1.Value == false || this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_0.Value == false)
                                        {
                                            ret = EventCodeEnum.NONE;
                                        }
                                        else
                                        {
                                            bErrorOccurred = true;
                                            if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                            {
                                                ret = EventCodeEnum.NONE;
                                                LoggerManager.Debug($"ChuckVacuumError inch8Value :{inch8} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");
                                            }
                                            else
                                            {
                                                ret = EventCodeEnum.MONITORING_CHUCK_8VAC_ERROR;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bErrorOccurred = true;
                                        if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                        {
                                            ret = EventCodeEnum.NONE;
                                            LoggerManager.Debug($"ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");
                                        }
                                        else
                                        {
                                            ret = EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR;
                                        }
                                    }
                                    break;
                                case EnumWaferSize.INCH12:
                                    ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, out inch6);
                                    if (inch6 == false || this.IOManager().IO.Outputs.DOCHUCKAIRON_0.Value == false || this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_0.Value == false)
                                    {
                                        ret = EventCodeEnum.NONE;
                                        ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, out inch8);
                                        if (inch8 == false || this.IOManager().IO.Outputs.DOCHUCKAIRON_1.Value == false)
                                        {
                                            ret = EventCodeEnum.NONE;
                                            ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_12, out inch12);
                                            if (inch12 == false || this.IOManager().IO.Outputs.DOCHUCKAIRON_2.Value == false || this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_2.Value == false)
                                            {
                                                ret = EventCodeEnum.NONE;
                                            }
                                            else
                                            {
                                                bErrorOccurred = true;
                                                if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                                {
                                                    ret = EventCodeEnum.NONE;
                                                    LoggerManager.Debug($"ChuckVacuumError inch12Value :{inch12} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");
                                                }
                                                else
                                                {
                                                    ret = EventCodeEnum.MONITORING_CHUCK_12VAC_ERROR;

                                                }
                                            }
                                        }
                                        else
                                        {
                                            bErrorOccurred = true;
                                            if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                            {
                                                ret = EventCodeEnum.NONE;
                                                LoggerManager.Debug($"ChuckVacuumError inch8Value :{inch8} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");
                                            }
                                            else
                                            {
                                                ret = EventCodeEnum.MONITORING_CHUCK_8VAC_ERROR;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bErrorOccurred = true;
                                        if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                        {
                                            ret = EventCodeEnum.NONE;
                                            LoggerManager.Debug($"ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");
                                        }
                                        else
                                        {
                                            ret = EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            ret = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (bErrorOccurred)
                    {
                        ChuckVacuumErrorCount++;
                    }
                    else
                    {
                        ChuckVacuumErrorCount = 0;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ManualRecovery()
        {
            try
            {
                EventCodeEnum ret = IsMainternanceMode();
                if (ret == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().DoSystemInit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckCardStuck : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckCardStuck" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "CardVAC";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Check the card vacuum line after unloading the probe card";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                CanManualRecovery = false;
                SystemErrorType = false;
                ErrorDescription = "";

                // 먼저 체크해야할 Behavior 등록.
                PreCheckRecoveryBehaviors.Add(new CheckMainVacFromLoader().Name);
                ShowMessageDialog = true;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;

                if(eventCode == EventCodeEnum.GP_CardChange_CARD_MAINVAC_ERROR)
                {
                    CardMainVacError();
                }
                else
                {
                    LoggerManager.Debug($"{Name}.ErrorOccurred() evnetCode = {eventCode}");
                }

                RecoveryDescription += $"\nCardDockingStatus : {this.CardChangeModule().GetCardDockingStatus()}";

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public void CardMainVacError()
        {
            try
            {
                this.NotifyManager().Notify(EventCodeEnum.GP_CardChange_CARD_MAINVAC_ERROR); //ALID : 73141

                switch (this.StageSupervisor().CardChangeModule().GetCCType())
                {
                    case EnumCardChangeType.NONE:
                        break;
                    case EnumCardChangeType.DIRECT_CARD:
                        if (this.StageSupervisor().CardChangeModule().GetCCDockType() == EnumCardDockType.NORMAL)
                        {
                            ErrorOccuered_DIRECT_CARD_NORMAL();
                        }
                        else if (this.StageSupervisor().CardChangeModule().GetCCDockType() == EnumCardDockType.DIRECTDOCK)
                        {
                            ErrorOccuered_DIRECT_CARD_DIRECTDOCK();
                        }
                        break;
                    case EnumCardChangeType.CARRIER:
                        ErrorOccuered_CARRIER_NORMAL();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
       
        public override void ClearError()
        {
            try
            {
                IsError = false;
                this.CardChangeModule().IsCardStuck = false;
                RecoveryDescription = "Check the card vacuum line after unloading the probe card";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.CardChangeModule().GetCardDockingStatus() == EnumCardDockingStatus.DOCKED) // 카드 DOCKED상태일 경우만 모니터링.
                {
                    bool dipogocard_vacu_sensor = false;
                    bool ditplate_pclatch_sensor_lock = false;
                    bool ditplate_pclatch_sensor_unlock = false;
                    var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                    if (ioret != IORet.NO_ERR)
                    {
                        ret = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return ret;
                    }
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                    if (ioret != IORet.NO_ERR)
                    {
                        ret = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return ret;

                    }
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                    if (ioret != IORet.NO_ERR)
                    {
                        ret = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return ret;
                    }

                    if (dipogocard_vacu_sensor == false)    // 포고카드 베큠 Off
                    {
                        switch (this.StageSupervisor().CardChangeModule().GetCCType())
                        {
                            case EnumCardChangeType.NONE:
                                break;
                            case EnumCardChangeType.DIRECT_CARD:
                                if (ditplate_pclatch_sensor_lock && ditplate_pclatch_sensor_unlock == false)    // 베큠 off, Latch 정상.
                                {
                                    ret = EventCodeEnum.GP_CardChange_CARD_MAINVAC_ERROR;
                                }
                                else
                                {
                                    ret = EventCodeEnum.GP_CardChange_CARD_MAINVAC_ERROR;
                                }
                                break;
                            case EnumCardChangeType.CARRIER:
                                // Todo: Add card detect sensor for MPT
                                ret = EventCodeEnum.NONE;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else if(this.CardChangeModule().GetCardDockingStatus() == EnumCardDockingStatus.DOCKEDWITHSUBVAC
                        || this.CardChangeModule().GetCardDockingStatus() == EnumCardDockingStatus.STUCKED)
                {
                    // 한번 에러가 발생하여, 상태가 DOCKEDWITHSUBVAC 또는 STUCK상태라면, 카드를 도킹하여 상태 Clear를 해줘야 함.
                    // 이는 카드 언로드 시 에러가 클리어 된다.
                    ret = EventCodeEnum.GP_CARD_STUCK;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public override void ManualRecovery()
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // Card도킹 타입 별로 함수 분리. (카드타입_카드도킹타입)
        public void ErrorOccuered_DIRECT_CARD_NORMAL()
        {
            try
            {
                StageEMGDown();

                var cardSubVac = this.IOManager().IO.Outputs.DOPOGOCARD_VACU_SUB;
                if (cardSubVac.IOOveride.Value == EnumIOOverride.NONE) //CardSubVac을 사용하는 장비인지 체크.
                {
                    var ioRet = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 1000, 3000, false);
                    if (ioRet == 0)
                    {
                        // MainVac이 떨어져서 Error가 발생했는데, MainVac이 다시 잡혔다면 SubVac이 켜졌고 SubVac에 의해 잡힌것으로 보겠다. SubVac Input이 따로 없음.
                        this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKEDWITHSUBVAC);
                    }
                    else
                    {
                        // SubVac을 켰는데 MainVac이 안잡힌 경우에는 Latch에 걸려있는 STUCK 상태다.
                        this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                    }
                }
                else
                {
                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                }

                // 머신이닛 후 리커버리 시퀀스 타기 위한 Flag
                this.CardChangeModule().IsCardStuck = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ErrorOccuered_DIRECT_CARD_DIRECTDOCK()
        {
            try
            {
                StageEMGDown();
                
                var cardSubVac = this.IOManager().IO.Outputs.DOPOGOCARD_VACU_SUB; 
                if (cardSubVac.IOOveride.Value == EnumIOOverride.NONE) //CardSubVac을 사용하는 장비인지 체크.
                {
                    var ioRet = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 1000, 3000, false);
                    if (ioRet == 0)
                    {
                        // MainVac이 떨어져서 Error가 발생했는데, MainVac이 다시 잡혔다면 SubVac이 켜졌고 SubVac에 의해 잡힌것이다. SubVac Input이 따로 없음.
                        this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKEDWITHSUBVAC);
                    }
                    else
                    {
                        // SubVac을 켰는데 MainVac이 안잡힌 경우에는 Latch에 걸려있는 STUCK 상태다.
                        this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                    }
                }
                else
                {
                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ErrorOccuered_CARRIER_NORMAL()
        {
            try
            {
                //TODO:
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StageEMGDown()
        {
            try
            {
                var cardSubVac = this.IOManager().IO.Outputs.DOPOGOCARD_VACU_SUB; //CardSubVac

                if (cardSubVac.IOOveride.Value == EnumIOOverride.NONE)
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_SUB, true);
                }
                
                if (this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUP ||
                    this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPPERFORM ||
                    this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPDWELL ||
                    this.ManualContactModule().IsZUpState == true)
                {
                    // 1. ZUP 중.
                    // 테스트 중이기 때문에 강제로 내릴 수 없음.
                    // Alarm을 이미 보냈기 때문에 Error_End를 받고 자연스럽게 LOT_END되길 기다림.
                    LoggerManager.Debug($"CheckCardStuck.ErrorOccurred() LotState is {this.LotOPModule().ModuleState.GetState()}, ProbingState is {this.ProbingModule().ProbingStateEnum} [Z-UP]");
                }
                else
                {
                    // 2
                    // 테스트 중은 아니지만 소킹이나 핀얼라인 상황에서 메인베큠이 빠지게 된다면,
                    // 서브베큠을 키긴 하지만 카드가 약간 쳐질 수 있기 때문에 즉시 ZDOWN.
                    if (this.WaferTransferModule().ModuleState.State == ModuleStateEnum.RUNNING ||
                        this.WaferTransferModule().ModuleState.State == ModuleStateEnum.PENDING)
                    {
                        // transfer 중에는 즉시 ZDown 하지 않고 Transfer가 끝난 뒤 Zdown 후 축을 죽인다.
                        this.WaferTransferModule().StopAfterTransferDone = true;
                        LoggerManager.Debug($"CheckCardStuck.ErrorOccurred() WaferTransferModule is Running");
                    }
                    else
                    {
                        this.MotionManager().StageEMGZDown(); // 즉시 Z DOWN
                    }
                    LoggerManager.Debug($"CheckCardStuck.ErrorOccurred() LotState is {this.LotOPModule().ModuleState.GetState()}, ProbingState is {this.ProbingModule().ProbingStateEnum}");
                }

                LoggerManager.Debug("Need to check for card vacuum status");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckTesterVac : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckTesterVac" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "TesterVAC";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private ICardChangeSysParam _CardChangeParam;
        [JsonIgnore]
        public ICardChangeSysParam CardChangeParam
        {
            get { return _CardChangeParam; }
            set { _CardChangeParam = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Turn on the tester vacuum.";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                CanManualRecovery = true;
                SystemErrorType = false;
                ShowMessageDialog = true;
                ErrorDescription = "";
                // 먼저 체크해야할 Behavior 등록.
                PreCheckRecoveryBehaviors.Add(new CheckMainVacFromLoader().Name);

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                if (eventCode == EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_OFF_ERROR)
                {
                    this.NotifyManager().Notify(EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_OFF_ERROR); //ALID : 73161

                    if (this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUP ||
                        this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPPERFORM ||
                        this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPDWELL ||
                        this.ManualContactModule().IsZUpState == true)
                    {
                        // 1. LOT Running && Probing ZUp
                        // 테스트 중이기 때문에 강제로 내릴 수 없음.
                        // Alarm을 이미 보냈기 때문에 Error_End를 받고 자연스럽게 LOT_END되길 기다림.
                        LoggerManager.Debug($"CheckTesterVac.ErrorOccurred() LotState is {this.LotOPModule().ModuleState.GetState()}, ProbingState is {this.ProbingModule().ProbingStateEnum} [Z-UP]");
                    }
                    else
                    {
                        // 2
                        // 테스트 중은 아니지만 소킹이나 핀얼라인 상황에서 메인베큠이 빠지게 된다면,
                        // 서브베큠을 키긴 하지만 카드가 약간 쳐질 수 있기 때문에 즉시 ZDOWN.
                        if (this.WaferTransferModule().ModuleState.State == ModuleStateEnum.RUNNING ||
                                this.WaferTransferModule().ModuleState.State == ModuleStateEnum.PENDING)
                        {
                            // transfer 중에는 즉시 ZDown 하지 않고 Transfer가 끝난 뒤 Zdown 후 축을 죽인다. (여기서 기다리진 않고 TransferModule내에서 idle이 되면 StageEMGZDown 불러줌.)
                            this.WaferTransferModule().StopAfterTransferDone = true;
                            LoggerManager.Debug($"CheckTesterVac.ErrorOccurred() WaferTransferModule is Running");
                        }
                        else
                        {
                            this.MotionManager().StageEMGZDown(); // 즉시 Z DOWN
                        }

                        LoggerManager.Debug($"CheckTesterVac.ErrorOccurred() LotState is {this.LotOPModule().ModuleState.GetState()}, ProbingState is {this.ProbingModule().ProbingStateEnum}");
                    }

                    LoggerManager.Debug("Need to check for testers vacuum status");
                }
                else
                {
                    LoggerManager.Debug($"{Name}.ErrorOccurred() evnetCode = {eventCode}");
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                LoggerManager.Debug("Testers vacuum status recovered");
                IsError = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool CardChangeParamInitFlag { get; set; } = false;
        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (CardChangeParamInitFlag == false)
            {
                CardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                CardChangeParamInitFlag = true;
            }
            try
            {
                if (this.CardChangeModule().GetCardDockingStatus() == EnumCardDockingStatus.DOCKED
                    && CardChangeParam.GPTesterVacSeqSkip == false)
                {
                    bool dipogotester_vacu_sensor = false;
                    var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);
                    if (dipogotester_vacu_sensor == false)
                    {
                        ret = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_OFF_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public override void ManualRecovery()
        {
            bool dipogotester_vacu_sensor = false;
            try
            {
                EventCodeEnum ret = IsMainternanceMode();
                if (ret == EventCodeEnum.NONE)
                {
                    if (this.CardChangeModule().GetCardDockingStatus() == EnumCardDockingStatus.DOCKED)
                    {
                        var testerVac = this.IOManager().IO.Outputs.DOPOGOTESTER_VACU; //CardSubVac

                        if (testerVac.IOOveride.Value == EnumIOOverride.NONE)
                        {
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU, true);
                            LoggerManager.Debug($"ManualRecovery() TesterVac On");
                        }
                        Thread.Sleep(2000);

                        var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);
                        if (ioret == IORet.NO_ERR && dipogotester_vacu_sensor == true)
                        {
                            LoggerManager.Debug($"ManualRecovery() TesterVac Recovery Success");
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

    [Serializable]
    public class CheckBackSideDoor : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckBackSideDoor" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "BackDoor";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Close the stage backside door.";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                CanManualRecovery = false;
                SystemErrorType = false;
                ShowMessageDialog = true;
                ret = EventCodeEnum.NONE;
                ErrorDescription = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                if (eventCode == EventCodeEnum.MONITORING_STAGE_BACKSIDEDOOR_OPEN)
                {
                    if (this.StageSupervisor().IStageMoveLockStatus.LastStageMoveLockReasonList.Where(x => x.Equals(ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN)).Count() == 0)
                    {
                        this.StageSupervisor().SetStageLock(ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN);
                    }
                }

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
                if (this.StageSupervisor().IStageMoveLockStatus.LastStageMoveLockReasonList.Where(x => x.Equals(ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN)).Count() > 0)
                {
                    var backsideIO = this.IOManager().IO.Inputs.DI_BACKSIDE_DOOR_OPEN;

                    bool value = false;
                    var ioRetVal = this.IOManager().IOServ.ReadBit(backsideIO, out value);
                    if (ioRetVal == IORet.NO_ERR && value == false)
                    {
                        var ioRet = this.IOManager().IOServ.MonitorForIO(backsideIO, false, 2000);
                        if (ioRet == 0)
                        {
                            this.StageSupervisor().SetStageUnlock(ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.StageSupervisor().IStageMoveLockParam.DoorInterLockEnable.Value == true)
                {
                    var backsideIO = this.IOManager().IO.Inputs.DI_BACKSIDE_DOOR_OPEN;

                    if (backsideIO.IOOveride.Value == EnumIOOverride.NONE)
                    {
                        bool value = false;
                        var ioRetVal = this.IOManager().IOServ.ReadBit(backsideIO, out value);
                        if (ioRetVal == IORet.NO_ERR && value == true)
                        {
                            var ioRet = this.IOManager().IOServ.MonitorForIO(backsideIO, true, 2000, 0, false);
                            if (ioRet == 0)
                            {
                                ret = EventCodeEnum.MONITORING_STAGE_BACKSIDEDOOR_OPEN;
                                return ret;
                            }
                        }
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ManualRecovery()
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckCurrentofThreePod : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckCurrentofThreePod" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "3PodTorq";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }

        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Three Pod Error. Check Axis.";
                IsError = false;
                PauseOnError = false;
                ImmediatePauseOnError = false;
                CanManualRecovery = false;
                SystemErrorType = false;
                ShowMessageDialog = false;
                ret = EventCodeEnum.NONE;
                ErrorDescription = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;
                this.NotifyManager().Notify(EventCodeEnum.MOTION_THREE_POD_LOAD_UNBALANCE_ERROR);

                double torqueTol = this.MotionManager().StageAxes.ThreePodTorqueTolerance.Value;
                double Z0_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.Z0);
                double Z1_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.Z1);
                double Z2_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.Z2);

                double X_Position = this.MotionManager().GetAxisPos(EnumAxisConstants.X);
                double Y_Position = this.MotionManager().GetAxisPos(EnumAxisConstants.Y);
                double Z_Position = this.MotionManager().GetAxisPos(EnumAxisConstants.Z);
                double C_Position = this.MotionManager().GetAxisPos(EnumAxisConstants.C);
                double PZ_Position = this.MotionManager().GetAxisPos(EnumAxisConstants.PZ);

                double X_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.X);
                double Y_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.Y);
                double C_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.C);
                double PZ_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.PZ);
                double TRI_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.TRI);
                double ROT_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.ROT);

                LoggerManager.MonitoringErrLog($"Z Torque Tol : {torqueTol}, Torque X:{X_torque}, Y:{Y_torque}, C:{C_torque}, PZ:{PZ_torque} ,Z0:{Z0_torque}, Z1:{Z1_torque}, Z2:{Z2_torque}, TRI:{TRI_torque}, ROT:{ROT_torque}" +
                                             $" Position X:{X_Position:00}, Y:{Y_Position:00}, Z:{Z_Position:00}, C:{C_Position:00}, PZ:{PZ_Position:00}");

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
                this.NotifyManager().ClearNotify(EventCodeEnum.MOTION_THREE_POD_LOAD_UNBALANCE_ERROR);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            
            try
            {
                AxisObject axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                if (!axis.Status.AxisBusy)
                {
                    ret = this.MotionManager().CalcZTorque(false);
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public override void ManualRecovery()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CheckTesterHeadPurge : MonitoringBehavior
    {
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "CheckTesterHeadPurge" };
        public override Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private EventCodeEnum _ErrorCode;
        [JsonIgnore]
        public override EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private string _Name = "PurgeAIR";
        [JsonIgnore]
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _ErrorDescription = "";
        [JsonIgnore]
        public override string ErrorDescription
        {
            get { return _ErrorDescription; }
            set { _ErrorDescription = value; }
        }

        private bool _IsError;
        [JsonIgnore]
        public override bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private bool _PauseOnError;
        [JsonIgnore]
        public override bool PauseOnError
        {
            get { return _PauseOnError; }
            set { _PauseOnError = value; }
        }

        private bool _ImmediatePauseOnError;
        [JsonIgnore]
        public override bool ImmediatePauseOnError
        {
            get { return _ImmediatePauseOnError; }
            set { _ImmediatePauseOnError = value; }
        }


        private bool _CanManualRecovery;
        [JsonIgnore]
        public override bool CanManualRecovery
        {
            get { return _CanManualRecovery; }
            set { _CanManualRecovery = value; }
        }

        private bool _SystemErrorType;
        [JsonIgnore]
        public override bool SystemErrorType
        {
            get { return _SystemErrorType; }
            set { _SystemErrorType = value; }
        }

        private string _RecoveryDescription;
        [JsonIgnore]
        public override string RecoveryDescription
        {
            get { return _RecoveryDescription; }
            set { _RecoveryDescription = value; }
        }

        private List<string> _PreCheckRecoveryBehaviors = new List<string>();
        [JsonIgnore]
        public override List<string> PreCheckRecoveryBehaviors
        {
            get { return _PreCheckRecoveryBehaviors; }
            set { _PreCheckRecoveryBehaviors = value; }
        }

        private bool _ShowMessageDialog;
        [JsonIgnore]
        public override bool ShowMessageDialog
        {
            get { return _ShowMessageDialog; }
            set { _ShowMessageDialog = value; }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorCode = EventCodeEnum.NONE;
                RecoveryDescription = "Check the tester head purge air I/O";
                IsError = false;
                PauseOnError = true;
                ImmediatePauseOnError = false;
                CanManualRecovery = false;
                SystemErrorType = false;
                ShowMessageDialog = true;
                ret = EventCodeEnum.NONE;
                ErrorDescription = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override EventCodeEnum ErrorOccurred(EventCodeEnum eventCode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsError = true;

                if(eventCode == EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR 
                    | eventCode == EventCodeEnum.TESTERHEAD_PURGE_AIR_LOW_ERROR
                    | eventCode == EventCodeEnum.TESTERHEAD_PURGE_AIR_HI_ERROR)
                {
                    this.NotifyManager().Notify(EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR);
                    LoggerManager.Debug($"CheckTesterHeadPurge.ErrorOccurred() : {eventCode}, IsPurgeAirBackUpValue: {this.TempController().IsPurgeAirBackUpValue}");
               
                    if (this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUP ||
                            this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPPERFORM ||
                            this.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPDWELL ||
                            this.ManualContactModule().IsZUpState == true)
                    {
                        this.MotionManager().StageEMGZDown(); // 즉시 Z DOWN
                        this.TempController().SetAmbientTemp();// 즉시 상온으로 변경
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ClearError()
        {
            try
            {
                IsError = false;
                ClearNotify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ClearNotify()
        {
            try
            {
                this.NotifyManager().ClearNotify(EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ClearNotify(): Error occurred. Err = {err.Message}");
            }
        }

        public override EventCodeEnum Monitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                long timeOut = 1000;
                if (this.IOManager().IO.Inputs.DITESTERHEAD_PURGE.IOOveride.Value == EnumIOOverride.NONE)
                {
                    if (this.TempController().IsPurgeAirBackUpValue)
                    {
                        bool ditoppurge = false;
                        var ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE, out ditoppurge); // 들어오는지 확인

                        if(ditoppurge == true) // air 들어올 때 Input 1
                        {
                            ret = EventCodeEnum.NONE;
                        }
                        else //Tester Purge Air 유량 떨어진 경우 출력 Input 0
                        {
                            if (IsError == false)
                            {
                                timeOut = 5000;
                            }
                            else
                            {
                                timeOut = 300;
                            }
                            var purgeret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE, true, 100, timeOut, false); // 들어 오는지 확인
                            if (purgeret != 0)
                            {
                                if(IsError == false)
                                {
                                    LoggerManager.Debug($"CheckTesterHeadPurge.Monitoring(): TESTERHEAD_PURGE_AIR_LOW_ERROR, Purge air state = {ditoppurge}, Output state = {this.TempController().IsPurgeAirBackUpValue}");
                                }
                                ret = EventCodeEnum.TESTERHEAD_PURGE_AIR_LOW_ERROR;
                            }
                            else
                            {
                                ret = EventCodeEnum.NONE;
                            }
                        }
                    }
                    else
                    {

                        bool ditoppurge = false;
                        var ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE, out ditoppurge); // 안들어오는지 확인
                        if(ditoppurge == true)      // 출력 해제 상태이므로 ditoppurge는 들어오면 안됨
                        {
                            if (IsError == false)
                            {
                                timeOut = 5000; // Error 발생 후 최초 1회는 5초 대기
                            }
                            else
                            {
                                timeOut = 300;  // Error 지속 상태면 Timeout 주기 감소
                            }
                            var purgeret = this.IOManager().IOServ.MonitorForIO(
                                this.IOManager().IO.Inputs.DITESTERHEAD_PURGE, false, 100, timeOut, false); // 안들어 오는지 확인
                            if (purgeret != 0)      // 기대상태 false일 때 타임아웃 발생 시
                            {
                                if (IsError == false)
                                {
                                    LoggerManager.Debug($"CheckTesterHeadPurge.Monitoring(): TESTERHEAD_PURGE_AIR_HI_ERROR, Purge air state = {ditoppurge}, Output state = {this.TempController().IsPurgeAirBackUpValue}");
                                }
                                ret = EventCodeEnum.TESTERHEAD_PURGE_AIR_HI_ERROR;
                            }
                            else
                            {   // Sensor glitch 이후 복구된 것으로 판단
                                ret = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {   // 정상상태
                            ret = EventCodeEnum.NONE;
                        }
                    }
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public override void ManualRecovery()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

}
