using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringModule
{
    using LoaderController;
    using LoaderController.GPController;
    using LogModule;
    using MetroDialogInterfaces;
    using MonitoringModule.HardwarePartChecker;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Lamp;
    using ProberInterfaces.CardChange;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using ProberInterfaces.Event;
    using ProberInterfaces.Monitoring;
    using Newtonsoft.Json;
    using ProberInterfaces.LoaderController;
    using ProberInterfaces.Command.Internal;

    public class MonitoringManager : IMonitoringManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;
        private static int MonitoringInterValInms = 50;
        private static int StageAxesErrorCount;
        private static int LoaderAxesErrorCount;
        private static int MainAirErrorCount;
        private static int MainVacuumErrorCount;
        private static int EMGErrorCount;
        private static int ChuckVacuumErrorCount;
        private static int PreVacuumErrorCount;
        private static int Arm1ErrorCount;
        private static int Arm2ErrorCount;
        private static int LoaderDoorErrorCount;
        //private static int FrontDoorErrorCount;

        private const int ErrorCountTolerance = 5;
        private const int FIleLimitLength = 50000000;

        private int lastPrintOutSeconds = 0;

        private bool TesterPurgeAirErrorFlag;
        //private void WriteProLog(EventCodeEnum Str, EventCodeEnum errorCodeList = EventCodeEnum.NONE)
        //{
        //    try
        //    {
        //        string EventCode;
        //        long CodeNum;

        //        CodeNum = (long)Str;
        //        EventCode = CodeNum.ToString("D8");

        //        LoggerManager.Prolog(EventCode, Str.ToString(), errorCodeList);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        private string FileFolder;
        private string FileName;
        private IParam _MonitoringSystemParam_IParam;
        public IParam MonitoringSystemParam_IParam
        {
            get { return _MonitoringSystemParam_IParam; }
            set
            {
                if (value != _MonitoringSystemParam_IParam)
                {
                    _MonitoringSystemParam_IParam = value;
                }
            }
        }

        public MonitoringSystemParameter MonitoringSystemParam { get; set; }

        public MonitoringBehaviorParam MonitoringBehaviorParam { get; set; }

        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set { _SysParam = value; }
        //}

        public IIOService IOService { get; set; }
        private List<HWPartChecker> _HWPartCheckList;
        private Stopwatch StageAxesStopWatch;
        private Stopwatch LoaderAxesStopWatch;

        private List<IMonitoringBehavior> _MonitoringBehaviorList;
        public List<IMonitoringBehavior> MonitoringBehaviorList
        {
            get { return _MonitoringBehaviorList; }
            set
            {
                if (value != _MonitoringBehaviorList)
                {
                    _MonitoringBehaviorList = value;
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    IOService = this.IOManager().IOServ;

                    List<HWPartChecker> hwPartCheckerList = new List<HWPartChecker>();

                    //==> 루프를 돌면서 검사할 HW Checker 리스트 생성
                    List<EnumAxisConstants> axisList = null;
                    if (MonitoringSystemParam.HWAxisCheckList.TryGetValue(nameof(AirChecker), out axisList))
                        hwPartCheckerList.Add(new AirChecker(this, axisList));

                    if (MonitoringSystemParam.HWAxisCheckList.TryGetValue(nameof(PowerChecker), out axisList))
                        hwPartCheckerList.Add(new PowerChecker(this, axisList));

                    if (MonitoringSystemParam.HWAxisCheckList.TryGetValue(nameof(AxisChecker), out axisList))
                        hwPartCheckerList.Add(new AxisChecker(this, axisList));

                    if (MonitoringSystemParam.HWAxisCheckList.TryGetValue(nameof(VacuumChecker), out axisList))
                        hwPartCheckerList.Add(new VacuumChecker(this, axisList));

                    if (MonitoringSystemParam.HWAxisCheckList.TryGetValue(nameof(DoorChecker), out axisList))
                        hwPartCheckerList.Add(new DoorChecker(this, axisList));

                    _HWPartCheckList = hwPartCheckerList;
                    //RunCheck();

                    LoadMonitoringBehaviorParams();

                    foreach (var beh in MonitoringBehaviorList)
                    {
                        beh.InitModule();
                    }

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        GPStageMachineMonitoring();
                    }
                    else
                    {
                        MachineMonitoring();
                    }
                    //MachineMonotiring();
                    IsSystemError = false;
                    IsStageSystemError = false;
                    IsLoaderSystemError = false;
                    IsMachineInitDone = false;
                    IsMachinInitOn = true;
                    IsGPEMOButtonOn = false;
                    IsGPLoaderMainAirEmergency = false;
                    IsGPLoaderVacuumEmergency = false;
                    retval = this.EventManager().RegisterEvent(typeof(MachineInitCompletedEvent).FullName, "ProbeEventSubscibers", MachineInitDone);
                    //retval = this.EventManager().RegisterEvent(typeof(MachineInitOnEvent).FullName, "ProbeEventSubscibers", MachineInitEventOn);

                    this.StageSupervisor().MachineInitEvent -= MachineInitEnter;

                    this.StageSupervisor().MachineInitEvent += MachineInitEnter;

                    this.StageSupervisor().MachineInitEndEvent -= MachineInitDone;

                    this.StageSupervisor().MachineInitEndEvent += MachineInitDone;
                    Initialized = true;
                    StageAxesErrorCount = 0;
                    LoaderAxesErrorCount = 0;
                    MainAirErrorCount = 0;
                    MainVacuumErrorCount = 0;
                    EMGErrorCount = 0;
                    ChuckVacuumErrorCount = 0;
                    PreVacuumErrorCount = 0;
                    Arm1ErrorCount = 0;
                    Arm2ErrorCount = 0;
                    LoaderDoorErrorCount = 0;
                    //FrontDoorErrorCount = 0;
                    FileFolder = @"C:\Logs";
                    FileName = @"MonitoringErrorCheck.txt";

                    TesterPurgeAirErrorFlag = false;
                    StageAxesStopWatch = new Stopwatch();
                    LoaderAxesStopWatch = new Stopwatch();

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

        public void MachineInitDone(object sender, EventArgs e)
        {
            try
            {
                IsMachinInitOn = false;

                // TODO : Check 
                IsMachineInitDone = true;

                LoggerManager.Debug("[MonitoringManger] MachineInitDone() OK");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void MachineInitEnter(object sender, EventArgs e)
        {
            try
            {
                IsMachinInitOn = true;
                IsMachineInitDone = false;
                IsSystemError = false;

                IsGPEMOButtonOn = false;
                IsGPLoaderMainAirEmergency = false;
                IsGPLoaderVacuumEmergency = false;

                foreach (var behavior in MonitoringBehaviorList)
                {
                    if (behavior is CheckEMGFromLoader)
                    {
                        (behavior as CheckEMGFromLoader).IsGPEMOButtonOn = IsGPEMOButtonOn;
                    }
                    else if (behavior is CheckMainAirFromLoader)
                    {
                        (behavior as CheckMainAirFromLoader).IsGPLoaderMainAirEmergency = IsGPLoaderMainAirEmergency;
                    }
                    else if (behavior is CheckMainVacFromLoader)
                    {
                        (behavior as CheckMainVacFromLoader).IsGPLoaderVacuumEmergency = IsGPLoaderVacuumEmergency;
                    }
                }

                LoggerManager.Debug("[MonitoringManger] MachineInitEnter() OK");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private bool _CheckRun;
        private Task _CheckRunTask;
        private RequestCombination _LampCombo;
        public RequestCombination LampCombo
        {
            get { return _LampCombo; }
            set
            {
                if (value != _LampCombo)
                {
                    _LampCombo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SkipCheckChuckVacuumFlag;
        public bool SkipCheckChuckVacuumFlag
        {
            get { return _SkipCheckChuckVacuumFlag; }
            set
            {
                if (value != _SkipCheckChuckVacuumFlag)
                {
                    _SkipCheckChuckVacuumFlag = value;

                    if (_SkipCheckChuckVacuumFlag == true)
                    {
                        LoggerManager.Debug($"SkipCheckChuckVacuumFlag on");
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSystemError;
        public bool IsSystemError
        {
            get { return _IsSystemError; }
            set
            {
                if (value != _IsSystemError)
                {
                    _IsSystemError = value;
                    this.LoaderController().BroadcastLotState(value);
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsStageSystemError;

        public bool IsStageSystemError
        {
            get { return _IsStageSystemError; }
            set
            {
                if (value != _IsStageSystemError)
                {
                    _IsStageSystemError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLoaderSystemError;
        public bool IsLoaderSystemError
        {
            get { return _IsLoaderSystemError; }
            set
            {
                if (value != _IsLoaderSystemError)
                {
                    _IsLoaderSystemError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsTesterheadPurgeAirError;

        public bool IsTesterheadPurgeAirError
        {
            get { return _IsTesterheadPurgeAirError; }
            set { _IsTesterheadPurgeAirError = value; }
        }

        private bool _MachinInitOn;

        public bool IsMachinInitOn
        {
            get { return _MachinInitOn; }
            set { _MachinInitOn = value; }
        }

        private bool _IsMachineInit;
        public bool IsMachineInitDone
        {
            get { return _IsMachineInit; }
            set { _IsMachineInit = value; }
        }

        private bool _IsGPEMOButtonOn;
        public bool IsGPEMOButtonOn
        {
            get { return _IsGPEMOButtonOn; }
            set { _IsGPEMOButtonOn = value; }
        }

        private bool _IsGPLoaderMainAirEmergency;
        public bool IsGPLoaderMainAirEmergency
        {
            get { return _IsGPLoaderMainAirEmergency; }
            set { _IsGPLoaderMainAirEmergency = value; }
        }

        private bool _IsGPLoaderVacuumEmergency;
        public bool IsGPLoaderVacuumEmergency
        {
            get { return _IsGPLoaderVacuumEmergency; }
            set { _IsGPLoaderVacuumEmergency = value; }
        }

        public EventCodeEnum RecievedFromLoaderEMG(EnumLoaderEmergency emgtype)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                switch (emgtype)
                {
                    case EnumLoaderEmergency.EMG:
                        IsGPEMOButtonOn = true;
                        break;
                    case EnumLoaderEmergency.AIR:
                        IsGPLoaderMainAirEmergency = true;
                        break;
                    case EnumLoaderEmergency.VACUUM:
                        IsGPLoaderVacuumEmergency = true;
                        break;
                    default:
                        break;
                }
                foreach (var behavior in MonitoringBehaviorList)
                {
                    if(behavior is CheckEMGFromLoader)
                    {
                        (behavior as CheckEMGFromLoader).IsGPEMOButtonOn = IsGPEMOButtonOn;
                    }
                    else if (behavior is CheckMainAirFromLoader)
                    {
                        (behavior as CheckMainAirFromLoader).IsGPLoaderMainAirEmergency = IsGPLoaderMainAirEmergency;
                    }
                    else if (behavior is CheckMainVacFromLoader)
                    {
                        (behavior as CheckMainVacFromLoader).IsGPLoaderVacuumEmergency = IsGPLoaderVacuumEmergency;
                    }
                }
                //IsSystemError = true;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum CheckFileLength()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var file = new FileInfo(Path.Combine(FileFolder, FileName));
                if (file.Exists)
                {
                    if (file.Length >= FIleLimitLength)
                    {
                        file.Delete();
                        ret = EventCodeEnum.NONE;
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
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //WriteProLog(EventCodeEnum.Chuck_Vacuum_Error, ret);
                LoggerManager.Debug($"Error occured in CheckFileLength function");
            }
            return ret;
        }

        //==> HW 부품 체크 루프
        public void RunCheck()
        {
            try
            {
                if (MonitoringSystemParam.MonitoringEnable.Value == false)
                    return;

                if (_CheckRun)
                    return;

                StopCheck();

                _CheckRun = true;
                _CheckRunTask = Task.Run(() =>
                {
                    const int checkInterval = 0;
                    HashSet<HWPartChecker> checkerHashSet = new HashSet<HWPartChecker>(_HWPartCheckList);//==> 일반 상태의 HW Checker 목록
                    HashSet<HWPartChecker> lockCheckerHashSet = new HashSet<HWPartChecker>();//==> Lock 상태가 걸린 Checker 목록
                    do
                    {

                        //==> 
                        foreach (HWPartChecker checker in checkerHashSet.ToList())
                        {
                            EnumHWPartErrorLevel errorLevel = checker.GetCheckResult();//==> HW Checker로 HW 부품 상태 검사
                            switch (errorLevel)
                            {
                                case EnumHWPartErrorLevel.LOCK:
                                    lockCheckerHashSet.Add(checker);//==> Lock 상태의 HW Checker 목록에 추가
                                    checkerHashSet.Remove(checker);//==> 일반 상태의 HW Checker 목록에서 제거
                                    checker.LockAxis();//==> HW checker가 관리하는 축 Lock
                                    break;
                                case EnumHWPartErrorLevel.EMERGENCY:
                                    checkerHashSet.Remove(checker);
                                    checker.SetEmergency();
                                    break;
                            }
                        }

                        foreach (HWPartChecker lockedChecker in lockCheckerHashSet.ToList())
                        {
                            EnumHWPartErrorLevel errorLevel = lockedChecker.GetCheckResult();
                            if (errorLevel == EnumHWPartErrorLevel.NONE)//==> 정상으로 돌아온 HW Checker
                            {
                                checkerHashSet.Add(lockedChecker);
                                lockCheckerHashSet.Remove(lockedChecker);
                                lockedChecker.UnlockAxis();

                                //==> 같은 축을 감시하는 HW Checker가 있는지 검사
                                bool isExistIntersectChecker = false;
                                foreach (HWPartChecker ownerChecker in lockCheckerHashSet)
                                {
                                    if (ownerChecker == lockedChecker)
                                        continue;

                                    isExistIntersectChecker = ownerChecker.AxisList.Intersect(lockedChecker.AxisList).Count() > 0;
                                    if (isExistIntersectChecker)
                                    {
                                        //==> lockedChecker 는 ownerChecker가 ResumAxis를 해야만 축을 해제 할 수 있다.
                                        //==> ownerChecker -> lockedChecker1 -> lockedChecker2 다음과 같은 사슬 관계 구축
                                        ownerChecker.ResumeDepHWPartChecker = lockedChecker;
                                        break;
                                    }
                                }

                                if (isExistIntersectChecker == false)
                                    lockedChecker.ResumeAxis();
                            }
                        }

                        Thread.Sleep(checkInterval);

                    } while (_CheckRun);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected EventCodeEnum ResultValidate(object funcname, EventCodeEnum retcode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (retcode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"FunctionName: {funcname.ToString()} Returncode: {retcode.ToString()} Error occurred");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        protected EventCodeEnum IOResultValidate(object funcname, IORet ioretcode)
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {

                if (ioretcode != IORet.NO_ERR)
                {
                    ret = EventCodeEnum.IO_DEV_LIB_ERROR;
                    LoggerManager.Error($"FunctionName: {funcname.ToString()} Returncode: {ioretcode.ToString()} Error occurred");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        private EventCodeEnum SysTemErrorCheck(ref bool systemerror)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsMachinInitOn == true)
                {
                    systemerror = true;
                    IsMachineInitDone = false;
                    return EventCodeEnum.MONITORING_MACHINEINIT_ON;
                }
                else
                {
                    if (IsSystemError == true)
                    {
                        this.LampManager().RequestSirenLamp();

                        systemerror = true;
                        IsMachineInitDone = false;
                        ret = EventCodeEnum.SYSTEM_ERROR;
                    }
                    else
                    {
                        systemerror = false;
                        ret = EventCodeEnum.NONE;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        private EventCodeEnum GPStageSysTemErrorCheck(ref bool systemerror)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsMachinInitOn == true)
                {
                    systemerror = true;
                    IsMachineInitDone = false;
                    return EventCodeEnum.MONITORING_MACHINEINIT_ON;
                }
                else
                {
                    if (IsSystemError == true)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                        systemerror = true;
                        IsMachineInitDone = false;
                        ret = EventCodeEnum.SYSTEM_ERROR;
                    }
                    else
                    {
                        systemerror = false;
                        ret = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        public Task<EventCodeEnum> MachineMonitoring()
        {

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                EventCodeEnum mRet = EventCodeEnum.UNDEFINED;
                if (MonitoringSystemParam.MonitoringEnable.Value == false)
                    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);

                if (_CheckRun)
                    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);

                StopCheck();
                try
                {
                    _CheckRun = true;
                    _CheckRunTask = Task.Run(() =>
                    {
                        do
                        {
                            try
                            {
                                if (IsStageSystemError == false && IsLoaderSystemError == false &&
                                IsSystemError == false && IsMachinInitOn == false)
                                {
                                    bool issyserror = false;
                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {ret.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckEMGButton();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckMainAir();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckMainVacuum();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckStageAxesState();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckLoaderAxesState();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckChuckVacuum();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckPreAlignVacuum();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckArmsVacuum();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);

                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }
                                    mRet = CheckThreeLeg();
                                    ResultValidate(MethodBase.GetCurrentMethod(), mRet);
                                    ret = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }



                                    //ret = SysTemErrorCheck(ref issyserror);
                                    //if (issyserror)
                                    //{
                                    //    this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {ret.ToString()}", EnumMessageStyle.Affirmative);
                                    //    continue;
                                    //}
                                    //ret = CheckMainPower();
                                    //ResultValidate(MethodBase.GetCurrentMethod(), ret);

                                    //ret = SysTemErrorCheck(ref issyserror);
                                    //if (issyserror)
                                    //{
                                    //    this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {ret.ToString()}", EnumMessageStyle.Affirmative);
                                    //    continue;
                                    //}
                                    //ret = CheckFrontDoor().Result;
                                    //ResultValidate(MethodBase.GetCurrentMethod(), ret);

                                    //ret = SysTemErrorCheck(ref issyserror);
                                    //if (issyserror)
                                    //{
                                    //    this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {ret.ToString()}", EnumMessageStyle.Affirmative);
                                    //    continue;
                                    //}
                                    //ret = CheckLoaderDoor();
                                    //ResultValidate(MethodBase.GetCurrentMethod(), ret);

                                    mRet = SysTemErrorCheck(ref issyserror);
                                    if (issyserror)
                                    {
                                        if (!IsMachinInitOn)
                                        {

                                            this.MetroDialogManager().ShowMessageDialog("SystemError", $"ErrorCode: {mRet.ToString()}", EnumMessageStyle.Affirmative);
                                        }
                                        continue;
                                    }

                                }
                                else
                                {
                                    bool isstagehomeSeted = true;
                                    // 뭔가를 다보고 정상적이 되었을때 다시 시스템 에러를 풀어줘야 함
                                    foreach (var stageaxis in this.MotionManager().StageAxes.ProbeAxisProviders)
                                    {
                                        if (stageaxis.Status.IsHomeSeted == true)
                                        {
                                            isstagehomeSeted &= true;
                                        }
                                        else
                                        {
                                            isstagehomeSeted &= false;
                                        }
                                    }
                                    bool isloaderhomeSeted = true;
                                    foreach (var loaderaxis in this.MotionManager().LoaderAxes.ProbeAxisProviders)
                                    {
                                        if (loaderaxis.Status.IsHomeSeted == true)
                                        {
                                            isloaderhomeSeted &= true;
                                        }
                                        else
                                        {
                                            isloaderhomeSeted &= false;
                                        }
                                    }

                                    if (isstagehomeSeted == true && isloaderhomeSeted == true && IsMachineInitDone == true && IsMachinInitOn == false)
                                    {
                                        IsSystemError = false;
                                        IsLoaderSystemError = false;
                                        IsStageSystemError = false;
                                        ret = EventCodeEnum.MONITORING_START;
                                        this.LampManager().ClearRequestLamp();
                                        //this.MetroDialogManager().ShowMessageDialog("SystemError Cleared", $"ErrorCode: {ret.ToString()}", EnumMessageStyle.Affirmative);

                                        //WriteProLog(EventCodeEnum.Monitoring_ReStart, EventCodeEnum.MONITORING_START);
                                        //LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Monitoring_ReStart, EventCodeEnum.MONITORING_START);

                                    }
                                    else
                                    {
                                        IsSystemError = true;
                                        //LoggerManager.Debug($"[MonitoringManager] isstagehomeSeted = {isstagehomeSeted}, isloaderhomeSeted = {isloaderhomeSeted}, IsMachineInit = {IsMachineInit}, MachinInitOn = {MachinInitOn}");
                                    }
                                }
                                //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                                System.Threading.Thread.Sleep(MonitoringInterValInms);

                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                                IsSystemError = true;
                                //WriteProLog(EventCodeEnum.Monitoring_Error, ret);
                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Monitoring_Error, ret);
                            }
                        } while (_CheckRun);
                    });

                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Monitoring_Error, ret);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(ret);
        }
        private EventCodeEnum GPSetSystemError()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (!IsSystemError)
                {
                    IsSystemError = true;

                    (this.LoaderController() as GP_LoaderController)?.GPLoaderService?.NotifyStageSystemError(
                        this.LoaderController().GetChuckIndex());

                    ret = this.EventManager().RaisingEvent(typeof(ProberErrorEvent).FullName);

                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error("$[MonitoringManager], GPSetSystemError() : ProberErrorEvent is not work properly.");
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
        private EventCodeEnum GPSetClearSystemError()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsSystemError)
                {
                    IsSystemError = false;
                    (this.LoaderController() as GP_LoaderController)?.GPLoaderService?.NotifyClearStageSystemError(
                          this.LoaderController().GetChuckIndex());
                }

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public Task<EventCodeEnum> GPStageMachineMonitoring()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (MonitoringSystemParam.MonitoringEnable.Value == false)
                    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);

                if (_CheckRun)
                    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);

                StopCheck();

                try
                {
                    _CheckRun = true;
                    _CheckRunTask = Task.Run(() =>
                    {
                        do
                        {
                            try
                            {
                                if (IsMachinInitOn == false)
                                {
                                    if (MonitoringBehaviorList.Count > 0)
                                    {
                                        foreach (var behavior in MonitoringBehaviorList)
                                        {
                                            bool issyserror = false;

                                            ret = behavior.Monitoring();
                                            behavior.ErrorCode = ret;
                                            if (ret != EventCodeEnum.NONE)
                                            {
                                                if (behavior.IsError == false)
                                                {
                                                    behavior.ErrorOccurred(ret);
                                                    if (behavior.SystemErrorType == true)
                                                    {
                                                        GPSetSystemError();
                                                        GPStageSysTemErrorCheck(ref issyserror);
                                                        if (issyserror)
                                                        {
                                                            if (!IsMachinInitOn)
                                                            {
                                                                string err_msg = "";
                                                                if (behavior.ErrorDescription == "")
                                                                {
                                                                    err_msg = $"Error Code : {ret} \n\n\nA system initialisation should be performed to clear any system errors.\n- {behavior.RecoveryDescription}";
                                                                }
                                                                else
                                                                {
                                                                    err_msg = $"Error Code : {ret}\n\nError Description: {behavior.ErrorDescription}\n\n\nA system initialisation should be performed to clear any system errors.\n- {behavior.RecoveryDescription}";
                                                                }
                                                                this.MetroDialogManager().ShowMessageDialog("SystemError", err_msg, EnumMessageStyle.Affirmative);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (behavior.ShowMessageDialog)
                                                        {
                                                            string err_msg = "";
                                                            if (behavior.ErrorDescription == "")
                                                            {
                                                                err_msg = $"Error Code : {ret} \n\n\nA system initialization should be performed to clear any system errors.\n- {behavior.RecoveryDescription}";
                                                            }
                                                            else
                                                            {
                                                                err_msg = $"Error Code : {ret}\nError Description: {behavior.ErrorDescription}\n\n\nA system initialization should be performed to clear any system errors.\n- {behavior.RecoveryDescription}";
                                                            }
                                                            this.MetroDialogManager().ShowMessageDialog("SystemError", err_msg, EnumMessageStyle.Affirmative);
                                                        }
                                                    }

                                                    if (behavior.PauseOnError == true)
                                                    {
                                                        this.LoaderController().LotOPPause(behavior.ImmediatePauseOnError);
                                                    }
                                                    this.LoaderController().SetMonitoringBehavior(MonitoringBehaviorList, this.LoaderController().GetChuckIndex());
                                                    this.LoaderController().ChangeTabIndex(TabControlEnum.MONITORING);                   
                                                    LoggerManager.Debug($"[MonitoringManager] {behavior.GetType().FullName} Error Occured");
                                                }
                                            }
                                            else
                                            {
                                                if (behavior.IsError == true)
                                                {
                                                    if (behavior.SystemErrorType == true)
                                                    {
                                                        if (IsMachineInitDone == true && IsMachinInitOn == false)
                                                        {
                                                            GPSetClearSystemError();
                                                            behavior.ClearError();

                                                            this.LoaderController().SetMonitoringBehavior(MonitoringBehaviorList, this.LoaderController().GetChuckIndex());
                                                            this.LoaderController().ChangeTabIndex(TabControlEnum.MONITORING);
                                                            LoggerManager.Debug($"[MonitoringManager] {behavior.GetType().FullName} Error Cleared + System Error Cleared");
                                                        }
                                                        else
                                                        {
                                                            // 실제로 Error 상태 자체는 클리어 되었지만, SystemErrorType의 Monitoring 항목의 경우는 머신이닛을 해야 ErrorClear가 가능함.
                                                            // 이 부분은 머신이닛이 완료되지 않아 들어오는 부분임.
                                                        }
                                                    }
                                                    else
                                                    {
                                                        behavior.ClearError();

                                                        this.LoaderController().SetMonitoringBehavior(MonitoringBehaviorList, this.LoaderController().GetChuckIndex());
                                                        this.LoaderController().ChangeTabIndex(TabControlEnum.MONITORING);
                                                        LoggerManager.Debug($"[MonitoringManager] {behavior.GetType().FullName} Error Cleared");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                                    System.Threading.Thread.Sleep(MonitoringInterValInms);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Monitoring_Error, ret);
                                LoggerManager.Exception(err);
                            }
                        } while (_CheckRun);
                    });

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Monitoring_Error, ret);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(ret);
        }


        public void StopCheck()
        {
            try
            {
                _CheckRun = false;
                if (_CheckRunTask != null)
                    _CheckRunTask.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new MonitoringSystemParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                RetVal = this.LoadParameter(ref tmpParam, typeof(MonitoringSystemParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    MonitoringSystemParam_IParam = tmpParam;
                    MonitoringSystemParam = MonitoringSystemParam_IParam as MonitoringSystemParameter;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadMonitoringBehaviorParams()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new MonitoringBehaviorParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                RetVal = this.LoadParameter(ref tmpParam, typeof(MonitoringBehaviorParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    MonitoringBehaviorParam = tmpParam as MonitoringBehaviorParam;
                    MonitoringBehaviorList = MonitoringBehaviorParam.MonitoringBehaviorList;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(MonitoringSystemParam);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error(String.Format("[Monitoring Manager] Save System Param: Serialize Error"));
                    return RetVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public Object GetHWPartCheckList()
        {
            return _HWPartCheckList;
        }
        public EventCodeEnum StageEmergencyStop()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IsSystemError = true;
                //ret = this.MotionManager().StageEMGStop();
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Stage_Emergency_Stop, ret);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                ret = EventCodeEnum.MONITORING_STAGE_EMG_STOP;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Stage_Emergency_Stop, ret);
            }
            return ret;
        }

        public EventCodeEnum LoaderEmergencyStop()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    this.NotifyManager().Notify(EventCodeEnum.Loader_Emergency_Stop);
                    IsSystemError = true;
                    ret = this.MotionManager().LoaderEMGStop();
                    //WriteProLog(EventCodeEnum.Loader_Emergency_Stop, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Loader_Emergency_Stop, ret);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    ret = EventCodeEnum.MONITORING_LOADER_EMG_STOP;
                    //WriteProLog(EventCodeEnum.Loader_Emergency_Stop, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Loader_Emergency_Stop, ret);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }


        #region Monitoring Function
        #region GP
        #endregion

        private EventCodeEnum CheckEMGButton()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
            List<string> emgErrorList = new List<string>();
            try
            {
                IORet ioreturnValue = IORet.ERROR;
                bool ioValue = false;
                try
                {
                    ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIEMGSTOPSW, out ioValue);
                    IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                    if (ioValue == true)
                    {
                        bErrorOccured = true;
                        if (EMGErrorCount < ErrorCountTolerance)
                        {
                            LoggerManager.Debug($"EMGError ioValue :{ioValue} EMGErrorCount:{EMGErrorCount} in MonitoringManager");

                            string errormsg = $"[{DateTime.Now.ToString()}] EMGError ioValue :{ioValue} EMGErrorCount:{EMGErrorCount} in MonitoringManager";
                            emgErrorList.Add(errormsg);
                        }
                        else
                        {
                            ret = EventCodeEnum.MONITORING_EMERGENCY_BUTTON_ON;
                            //WriteProLog(EventCodeEnum.EMG_Button_On, ret);
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.EMG_Button_On, ret);
                            IsSystemError = true;
                            IsStageSystemError = true;
                        }

                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (bErrorOccured)
                    {
                        EMGErrorCount++;
                    }
                    else
                    {
                        EMGErrorCount = 0;
                    }

                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    ret = EventCodeEnum.MONITORING_EMERGENCY_BUTTON_ON;
                    //WriteProLog(EventCodeEnum.EMG_Button_On, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.EMG_Button_On, ret);
                    IsSystemError = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if (0 < (emgErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in emgErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }

        private EventCodeEnum CheckMainAir()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
            List<string> mainAirErrorList = new List<string>();
            try
            {
                IORet ioreturnValue = IORet.ERROR;
                bool ioValue = false;
                try
                {
                    ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIMAINAIR, out ioValue);
                    IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                    if (ioValue == false)
                    {
                        bErrorOccured = true;
                        if (MainAirErrorCount < ErrorCountTolerance)
                        {
                            LoggerManager.Debug($"MainAirError ioValue :{ioValue} MainErrorCount:{MainAirErrorCount} in MonitoringManager");

                            string errormsg = $"[{DateTime.Now.ToString()}] MainAirError ioValue :{ioValue} MainErrorCount:{MainAirErrorCount} in MonitoringManager";
                            mainAirErrorList.Add(errormsg);
                        }
                        else
                        {
                            ret = StageEmergencyStop();
                            ResultValidate(MethodBase.GetCurrentMethod(), ret);
                            ret = EventCodeEnum.MONITORING_MAIN_AIR_ERROR;
                            //WriteProLog(EventCodeEnum.Main_Air_Error, ret);
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Main_Air_Error, ret);
                            this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                            IsSystemError = true;
                            IsStageSystemError = true;
                        }

                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (bErrorOccured)
                    {
                        MainAirErrorCount++;
                    }
                    else
                    {
                        MainAirErrorCount = 0;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                    IsSystemError = true;
                    ret = EventCodeEnum.MONITORING_MAIN_AIR_ERROR;
                    //WriteProLog(EventCodeEnum.Main_Air_Error, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Main_Air_Error, ret);
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if (0 < (mainAirErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in mainAirErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }

        private EventCodeEnum CheckMainVacuum()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
            List<string> mainVacuumErrorList = new List<string>();
            try
            {
                IORet ioreturnValue = IORet.ERROR;
                bool ioValue = false;
                try
                {
                    ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIMAINVAC, out ioValue);
                    IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                    if (ioValue == false)
                    {
                        bErrorOccured = true;
                        if (MainVacuumErrorCount < ErrorCountTolerance)
                        {
                            LoggerManager.Debug($"MainVacuumError ioValue :{ioValue} MainVacuumErrorCount:{MainVacuumErrorCount} in MonitoringManager");

                            string errormsg = $"[{DateTime.Now.ToString()}] MainVacuumError ioValue :{ioValue} MainVacuumErrorCount:{MainVacuumErrorCount} in MonitoringManager";
                            mainVacuumErrorList.Add(errormsg);
                        }
                        else
                        {
                            ret = StageEmergencyStop();
                            ResultValidate(MethodBase.GetCurrentMethod(), ret);
                            ret = EventCodeEnum.MONITORING_MAIN_VACUUM_ERROR;
                            //WriteProLog(EventCodeEnum.Main_Vacuum_Error, ret);
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Main_Vacuum_Error, ret);
                            this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                            IsSystemError = true;
                            IsStageSystemError = true;
                        }

                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (bErrorOccured)
                    {
                        MainVacuumErrorCount++;
                    }
                    else
                    {
                        MainVacuumErrorCount = 0;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                    IsSystemError = true;
                    ret = EventCodeEnum.MONITORING_MAIN_VACUUM_ERROR;
                    //WriteProLog(EventCodeEnum.Main_Vacuum_Error, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Main_Vacuum_Error, ret);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if (0 < (mainVacuumErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in mainVacuumErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }

        private EventCodeEnum CheckMainPower()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                IORet ioreturnValue = IORet.ERROR;
                bool ioValue = false;
                try
                {
                    ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOWER_DOWN, out ioValue);
                    IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                    if (ioValue == true)
                    {
                        ret = StageEmergencyStop();
                        ResultValidate(MethodBase.GetCurrentMethod(), ret);
                        ret = EventCodeEnum.MONITORING_MAIN_POWER_ERROR;
                        //WriteProLog(EventCodeEnum.Main_Power_Error, ret);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Main_Power_Error, ret);
                        this.NotifyManager().Notify(EventCodeEnum.Main_Power_Error);
                        this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                        IsSystemError = true;
                        IsStageSystemError = true;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    ret = EventCodeEnum.MONITORING_MAIN_POWER_ERROR;
                    //WriteProLog(EventCodeEnum.Main_Power_Error, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Main_Power_Error, ret);
                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                    IsSystemError = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        private EventCodeEnum CheckStageAxesState()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
            List<string> axisErrorList = new List<string>();

            try
            {
                if (IsMachinInitOn == true)
                {
                    ret = EventCodeEnum.MONITORING_MACHINEINIT_ON;
                }
                else
                {
                    foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                    {
                        if (IsMachinInitOn == true)
                        {
                            ret = EventCodeEnum.MONITORING_MACHINEINIT_ON;
                            break;
                        }
                        else
                        {
                            if (axis.Status.State == EnumAxisState.ERROR || axis.Status.State == EnumAxisState.INVALID ||
                            axis.Status.State == EnumAxisState.DISABLED)
                            {

                                bErrorOccured = true;

                                StageAxesStopWatch.Start();
                                if (StageAxesStopWatch.ElapsedMilliseconds < this.MonitoringSystemParam.StageAxesTimeout.Value)
                                {
                                    if (StageAxesStopWatch.Elapsed.Seconds != lastPrintOutSeconds)
                                    {
                                        LoggerManager.Debug($"Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                        $" StageAxesStopWatch: {StageAxesStopWatch.ElapsedMilliseconds} in MonitoringManager");
                                        lastPrintOutSeconds = StageAxesStopWatch.Elapsed.Seconds;
                                    }
                                    string errormsg = $"[{DateTime.Now.ToString()}] Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                       $" StageAxesStopWatch: {StageAxesStopWatch.ElapsedMilliseconds}";
                                    axisErrorList.Add(errormsg);
                                }
                                else
                                {
                                    LoggerManager.Debug($"Axis State Error Axis:{axis.Label.Value} State: {axis.Status.State} in MonitoringManager");
                                    ret = StageEmergencyStop();
                                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                                        //WriteProLog(EventCodeEnum.Stage_Axes_State_Error, ret);
                                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Stage_Axes_State_Error, ret);
                                        this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                        IsSystemError = true;
                                        IsStageSystemError = true;
                                        break;
                                    }
                                }

                                //if (StageAxesErrorCount < ErrorCountTolerance)
                                //{
                                //    LoggerManager.Debug($"Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                //        $" StageErrorCount: {StageAxesErrorCount} in MonitoringManager");

                                //    string errormsg = $"[{DateTime.Now.ToString()}] Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                //       $" StageErrorCount: {StageAxesErrorCount}";
                                //    axisErrorList.Add(errormsg);
                                //}
                                //else
                                //{
                                //    LoggerManager.Debug($"Axis State Error Axis:{axis.Label.Value} State: {axis.Status.State} in MonitoringManager");
                                //    ret = StageEmergencyStop();
                                //    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                //    if (ret != EventCodeEnum.NONE)
                                //    {
                                //        ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                                //        //WriteProLog(EventCodeEnum.Stage_Axes_State_Error, ret);
                                //        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Stage_Axes_State_Error, ret);

                                //        IsSystemError = true;
                                //        IsStageSystemError = true;
                                //        break;
                                //    }
                                //}

                                //}

                            }
                            else
                            {
                                ret = EventCodeEnum.NONE;
                            }
                        }

                    }

                    if (bErrorOccured)
                    {
                        StageAxesErrorCount++;
                    }
                    else
                    {
                        StageAxesErrorCount = 0;
                        StageAxesStopWatch.Reset();
                        StageAxesStopWatch.Stop();
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //GPSetSystemError();
                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                IsSystemError = true;
                ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                //WriteProLog(EventCodeEnum.Stage_Axes_State_Error, ret);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Stage_Axes_State_Error, ret);

            }
            finally
            {
                if (0 < (axisErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in axisErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }

        private EventCodeEnum CheckLoaderAxesState()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
            List<string> axisErrorList = new List<string>();
            try
            {
                if (IsMachinInitOn == true)
                {
                    ret = EventCodeEnum.MONITORING_MACHINEINIT_ON;
                }
                else
                {
                    foreach (var axis in this.MotionManager().LoaderAxes.ProbeAxisProviders)
                    {
                        if (axis.Status.State == EnumAxisState.ERROR || axis.Status.State == EnumAxisState.INVALID ||
                            axis.Status.State == EnumAxisState.DISABLED)
                        {
                            if (IsMachinInitOn == true)
                            {
                                ret = EventCodeEnum.MONITORING_MACHINEINIT_ON;
                                break;
                            }
                            else
                            {
                                LoaderAxesStopWatch.Start();
                                bErrorOccured = true;

                                if (LoaderAxesStopWatch.ElapsedMilliseconds < this.MonitoringSystemParam.StageAxesTimeout.Value)
                                {
                                    if (LoaderAxesStopWatch.Elapsed.Seconds != lastPrintOutSeconds)
                                    {
                                        LoggerManager.Debug($"Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                        $" LoaderAxesStopWatch: {LoaderAxesStopWatch.ElapsedMilliseconds} in MonitoringManager");
                                        lastPrintOutSeconds = LoaderAxesStopWatch.Elapsed.Seconds;
                                    }
                                    string errormsg = $"[{DateTime.Now.ToString()}] Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                       $" LoaderAxesStopWatch: {LoaderAxesStopWatch.ElapsedMilliseconds}";
                                    axisErrorList.Add(errormsg);
                                    ret = EventCodeEnum.NONE;
                                }
                                else
                                {
                                    LoggerManager.Debug($"Axis State Error Axis:{axis.Label.Value} State: {axis.Status.State} in MonitoringManager");
                                    ret = LoaderEmergencyStop();
                                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                                        //WriteProLog(EventCodeEnum.Stage_Axes_State_Error, ret);
                                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Stage_Axes_State_Error, ret);
                                        this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                        IsSystemError = true;
                                        IsStageSystemError = true;
                                        break;
                                    }
                                }

                                //if (LoaderAxesErrorCount < ErrorCountTolerance)
                                //{
                                //    LoggerManager.Debug($"Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                //        $" LoaderErrorCount: {LoaderAxesErrorCount} in MonitoringManager");
                                //    string errormsg = $"[{DateTime.Now.ToString()}] Axis StateError Axis:{axis.Label.Value} State: {axis.Status.State} StatusCode:{axis.Status.StatusCode}  " +
                                //      $" LoaderErrorCount: {LoaderAxesErrorCount}";
                                //    axisErrorList.Add(errormsg);
                                //}
                                //else
                                //{
                                //    LoggerManager.Debug($"Axis State Error Axis:{axis.Label.Value} State: {axis.Status.State} in MonitoringManager");
                                //    ret = LoaderEmergencyStop();
                                //    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                //    if (ret != EventCodeEnum.NONE)
                                //    {
                                //        ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                                //        //WriteProLog(EventCodeEnum.Loader_Axes_State_Error, ret);
                                //        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Loader_Axes_State_Error, ret);

                                //        IsSystemError = true;
                                //        IsLoaderSystemError = true;
                                //        break;
                                //    }
                                //}
                            }

                        }
                        else
                        {
                            ret = EventCodeEnum.NONE;
                        }

                    }
                    if (bErrorOccured)
                    {
                        LoaderAxesErrorCount++;
                    }
                    else
                    {
                        LoaderAxesErrorCount = 0;
                        LoaderAxesStopWatch.Reset();
                        LoaderAxesStopWatch.Stop();
                    }
                }


            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                IsSystemError = true;
                ret = EventCodeEnum.MONITORING_AXIS_STATE_ERROR;
                //WriteProLog(EventCodeEnum.Loader_Axes_State_Error, ret);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Loader_Axes_State_Error, ret);
            }
            finally
            {
                if (0 < (axisErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in axisErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }
            }
            return ret;
        }

        private EventCodeEnum CheckChuckVacuum()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
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
                    if (this.SkipCheckChuckVacuumFlag == false)
                    {
                        if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                            this.WaferTransferModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
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
                                    else
                                    {
                                        LoggerManager.Error($"WaferSizeEnum = {physinfo.WaferSizeEnum}, WaferSize_um = {physinfo.WaferSize_um.Value}");
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
                                        IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                                        if (inch6 == false)
                                        {
                                            ret = EventCodeEnum.NONE;
                                        }
                                        else
                                        {
                                            bErrorOccured = true;
                                            if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                            {
                                                LoggerManager.Debug($"ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");

                                                string errormsg = $"[{DateTime.Now.ToString()}] ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager";
                                                chuckVacuumErrorList.Add(errormsg);
                                            }
                                            else
                                            {
                                                ret = EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR;
                                                //WriteProLog(EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                                this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                IsSystemError = true;
                                                IsStageSystemError = true;
                                            }

                                        }
                                        break;
                                    case EnumWaferSize.INCH8:
                                        ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, out inch6);
                                        IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                                        if (inch6 == false)
                                        {
                                            ret = EventCodeEnum.NONE;
                                            ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, out inch8);
                                            IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                                            if (inch8 == false)
                                            {
                                                ret = EventCodeEnum.NONE;
                                            }
                                            else
                                            {
                                                bErrorOccured = true;
                                                if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                                {
                                                    LoggerManager.Debug($"ChuckVacuumError inch8Value :{inch8} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");

                                                    string errormsg = $"[{DateTime.Now.ToString()}] ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager";
                                                    chuckVacuumErrorList.Add(errormsg);
                                                }
                                                else
                                                {
                                                    ret = EventCodeEnum.MONITORING_CHUCK_8VAC_ERROR;
                                                    //WriteProLog(EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                    this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                                                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                                    IsSystemError = true;
                                                    IsStageSystemError = true;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            bErrorOccured = true;
                                            if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                            {
                                                LoggerManager.Debug($"ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");

                                                string errormsg = $"[{DateTime.Now.ToString()}] ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager";
                                                chuckVacuumErrorList.Add(errormsg);
                                            }
                                            else
                                            {
                                                ret = EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR;
                                                this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                                                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                                IsSystemError = true;
                                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
                                            }
                                        }
                                        break;
                                    case EnumWaferSize.INCH12:
                                        ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, out inch6);
                                        IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                                        if (inch6 == false)
                                        {
                                            ret = EventCodeEnum.NONE;
                                            ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, out inch8);
                                            IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                                            if (inch8 == false)
                                            {
                                                ret = EventCodeEnum.NONE;
                                                ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONCHUCK_12, out inch12);
                                                IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                                                if (inch12 == false)
                                                {
                                                    ret = EventCodeEnum.NONE;
                                                }
                                                else
                                                {
                                                    if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                                    {
                                                        LoggerManager.Debug($"ChuckVacuumError inch12Value :{inch12} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");
                                                        string errormsg = $"[{DateTime.Now.ToString()}] ChuckVacuumError inch12Value :{inch12} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager";
                                                        chuckVacuumErrorList.Add(errormsg);
                                                    }
                                                    else

                                                    {
                                                        this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                                                        this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                                        IsSystemError = true;
                                                        IsStageSystemError = true;
                                                        ret = EventCodeEnum.MONITORING_CHUCK_12VAC_ERROR;
                                                        //WriteProLog(EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bErrorOccured = true;
                                                if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                                {
                                                    LoggerManager.Debug($"ChuckVacuumError inch8Value :{inch8} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");

                                                    string errormsg = $"[{DateTime.Now.ToString()}] ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager";
                                                    chuckVacuumErrorList.Add(errormsg);
                                                }
                                                else
                                                {
                                                    IsSystemError = true;
                                                    IsStageSystemError = true;
                                                    this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                                                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                                    ret = EventCodeEnum.MONITORING_CHUCK_8VAC_ERROR;
                                                    //WriteProLog(EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bErrorOccured = true;
                                            if (ChuckVacuumErrorCount < ErrorCountTolerance)
                                            {
                                                LoggerManager.Debug($"ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager");

                                                string errormsg = $"[{DateTime.Now.ToString()}] ChuckVacuumError inch6Value :{inch6} ChuckVacuumErrorCount:{ChuckVacuumErrorCount} in MonitoringManager";
                                                chuckVacuumErrorList.Add(errormsg);
                                            }
                                            else
                                            {
                                                this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                                                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                                IsSystemError = true;
                                                IsStageSystemError = true;
                                                ret = EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR;
                                                //WriteProLog(EventCodeEnum.Chuck_Vacuum_Error, ret);
                                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
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
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (bErrorOccured)
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
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    //WriteProLog(EventCodeEnum.Chuck_Vacuum_Error, ret);
                    this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Chuck_Vacuum_Error, ret);
                    IsSystemError = true;
                    IsStageSystemError = true;

                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if (0 < (chuckVacuumErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in chuckVacuumErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }

        private EventCodeEnum CheckPreAlignVacuum()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
            List<string> preVacuumErrorList = new List<string>();
            try
            {
                IORet ioreturnValue = IORet.ERROR;
                bool ioValue = false;
                try
                {

                    if ((this.LoaderController() as LoaderController).LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.EXIST &&
                        (this.LoaderController() as LoaderController).LoaderInfo.ModuleInfo.ModuleState != ModuleStateEnum.RUNNING)
                    {
                        if (this.StageSupervisor().StageModuleState.GetState() != StageStateEnum.MANUAL)
                        {
                            ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONSUBCHUCK, out ioValue);
                            IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);

                            if (ioValue == false)
                            {
                                bErrorOccured = true;
                                if (MainAirErrorCount < ErrorCountTolerance)
                                {
                                    LoggerManager.Debug($"PreVacuumError ioValue :{ioValue} PreVacuumErrorCount:{PreVacuumErrorCount} in MonitoringManager");

                                    string errormsg = $"[{DateTime.Now.ToString()}] PreVacuumError ioValue :{ioValue} PreVacuumErrorCount:{PreVacuumErrorCount} in MonitoringManager";
                                    preVacuumErrorList.Add(errormsg);
                                }
                                else
                                {
                                    ret = LoaderEmergencyStop();
                                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                    ret = EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR;
                                    //WriteProLog(EventCodeEnum.PreAlign_Vacuum_Error, ret);
                                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PreAlign_Vacuum_Error, ret);
                                    IsSystemError = true;
                                    IsLoaderSystemError = true;
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
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                    if (bErrorOccured)
                    {
                        PreVacuumErrorCount++;
                    }
                    else
                    {
                        PreVacuumErrorCount = 0;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    ret = EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR;
                    //WriteProLog(EventCodeEnum.PreAlign_Vacuum_Error, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PreAlign_Vacuum_Error, ret);
                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                    IsSystemError = true;
                    IsLoaderSystemError = true;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if (0 < (preVacuumErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in preVacuumErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }

        private EventCodeEnum CheckArmsVacuum()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bAmr1ErrorOccured = false;
            bool bAmr2ErrorOccured = false;
            StreamWriter sw = null;
            List<string> ArmsErrorList = new List<string>();
            try
            {
                IORet ioreturnValue = IORet.ERROR;
                bool ioValue = false;
                try
                {
                    //arm1
                    if ((this.LoaderController() as LoaderController).LoaderInfo.StateMap.ARMModules[0].WaferStatus == EnumSubsStatus.EXIST &&
                        (this.LoaderController() as LoaderController).LoaderInfo.ModuleInfo.ModuleState != ModuleStateEnum.RUNNING)
                    {
                        if (this.StageSupervisor().StageModuleState.GetState() != StageStateEnum.MANUAL)
                        {
                            ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONARM, out ioValue);
                            IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                            if (ioValue == false)
                            {
                                bAmr1ErrorOccured = true;
                                if (Arm1ErrorCount < ErrorCountTolerance)
                                {
                                    LoggerManager.Debug($"Arm1Error ioValue :{ioValue} Arm1ErrorCount:{Arm1ErrorCount} in MonitoringManager");

                                    string errormsg = $"[{DateTime.Now.ToString()}] Arm1Error ioValue :{ioValue} Arm1ErrorCount:{Arm1ErrorCount} in MonitoringManager";
                                    ArmsErrorList.Add(errormsg);
                                }
                                else
                                {
                                    ret = LoaderEmergencyStop();
                                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                    ret = EventCodeEnum.MONITORING_ARM1_VAC_ERROR;
                                    //WriteProLog(EventCodeEnum.Arms_Vacuum_Error, ret);
                                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Arms_Vacuum_Error, ret);
                                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                    IsSystemError = true;
                                    IsLoaderSystemError = true;
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

                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                    if (bAmr1ErrorOccured)
                    {
                        Arm1ErrorCount++;
                    }
                    else
                    {
                        Arm1ErrorCount = 0;
                    }


                    //arm2
                    if ((this.LoaderController() as LoaderController).LoaderInfo.StateMap.ARMModules[1].WaferStatus == EnumSubsStatus.EXIST &&
                        (this.LoaderController() as LoaderController).LoaderInfo.ModuleInfo.ModuleState != ModuleStateEnum.RUNNING)
                    {
                        if (this.StageSupervisor().StageModuleState.GetState() != StageStateEnum.MANUAL)
                        {
                            ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERONARM2, out ioValue);
                            IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);
                            if (ioValue == false)
                            {
                                bAmr1ErrorOccured = true;
                                if (Arm2ErrorCount < ErrorCountTolerance)
                                {
                                    LoggerManager.Debug($"Arm2Error ioValue :{ioValue} Arm2ErrorCount:{Arm2ErrorCount} in MonitoringManager");

                                    string errormsg = $"[{DateTime.Now.ToString()}] Arm2Error ioValue :{ioValue} Arm2ErrorCount:{Arm2ErrorCount} in MonitoringManager";
                                    ArmsErrorList.Add(errormsg);
                                }
                                else
                                {
                                    ret = LoaderEmergencyStop();
                                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                    ret = EventCodeEnum.MONITORING_ARM2_VAC_ERROR;
                                    //WriteProLog(EventCodeEnum.Arms_Vacuum_Error, ret);
                                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Arms_Vacuum_Error, ret);
                                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                    IsSystemError = true;
                                    IsLoaderSystemError = true;
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
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                    if (bAmr2ErrorOccured)
                    {
                        Arm2ErrorCount++;
                    }
                    else
                    {
                        Arm2ErrorCount = 0;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    //WriteProLog(EventCodeEnum.Arms_Vacuum_Error, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Arms_Vacuum_Error, ret);
                    this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                    IsLoaderSystemError = true;
                    IsSystemError = true;
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if (0 < (ArmsErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in ArmsErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }

        private EventCodeEnum CheckTesterHeadPurge()
        {
            EventCodeEnum ret = EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR;
            try
            {
                if(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE.IOOveride.Value == EnumIOOverride.NONE)
                {
                    bool expected_input = this.TempController().IsPurgeAirBackUpValue;

                    bool ditoppurge = false;
                    var ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE, out ditoppurge);//들어오는지 확인
                    IOResultValidate(MethodBase.GetCurrentMethod(), ioreturnValue);

                    if (ditoppurge == expected_input)
                    {                        
                        TesterPurgeAirErrorFlag = false;
                        ret = EventCodeEnum.NONE;
                    }
                    else //Tester Purge Air 유량 떨어진 경우 출력 Input 0
                    {
                        var purgeret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE,
                                                                            expected_input,
                                                                            this.IOManager().IO.Inputs.DITESTERHEAD_PURGE.MaintainTime.Value,
                                                                            this.IOManager().IO.Inputs.DITESTERHEAD_PURGE.TimeOut.Value,
                                                                            writelog: false);//delay 을 줘서 한번 더 기회를 준다.
                        if (purgeret != 0)
                        {
                            ret = EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR;
                            if (TesterPurgeAirErrorFlag == false)
                            {
                                this.NotifyManager().Notify(ret);
                                LoggerManager.Debug($"CheckTesterHeadPurge(), retval: {ret}, IsPurgeAirBackUpValue: {this.TempController().IsPurgeAirBackUpValue}, expected_input:{expected_input},  ditoppurge: {ditoppurge}, purgeret:{purgeret}");
                                TesterPurgeAirErrorFlag = true;
                            }
                        }
                        else
                        {
                            TesterPurgeAirErrorFlag = false;
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
                throw;
            }
            return ret;
        }

        private EventCodeEnum GP_CheckBackSideDoor()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                //IO가 에뮬인지 아닌지 체크
                //에뮬이 아니라면 Readbit를 통해 값을 확인
                //만약 IO가 들어온 경우라면 MonitorForIo로 체크
                //그래도 들어온경우라면 LOT상태와 다른 모듈의 러닝상태를 체크 
                // 모듈이 멈춰 있다 생각하면 Lock 상태로 체인지 ( StageSupervisor 모듈, 무브스테이트) -> 로더에게 전달
                if (this.StageSupervisor().IStageMoveLockParam.DoorInterLockEnable.Value == true)
                {
                    var backsideIO = this.IOManager().IO.Inputs.DI_BACKSIDE_DOOR_OPEN;

                    if (backsideIO.IOOveride.Value == EnumIOOverride.NONE)
                    {
                        int checkInterval = 500;

                        if (this.StageSupervisor().IStageMoveLockStatus.LastStageMoveLockReasonList.Where(x => x.Equals(ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN)).Count() == 0)
                        {
                            bool value = false;
                            var ioRetVal = this.IOManager().IOServ.ReadBit(backsideIO, out value);
                            if (ioRetVal == IORet.NO_ERR && value == true)
                            {
                                var ioRet = this.IOManager().IOServ.MonitorForIO(backsideIO, true, 2000);
                                if (ioRet == 0)
                                {
                                    this.StageSupervisor().SetStageLock(ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN);
                                }
                            }
                        }

                        if (this.StageSupervisor().IStageMoveLockStatus.LastStageMoveLockReasonList.Where(x => x.Equals(ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN)).Count() > 0)
                        {
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
                        Thread.Sleep(checkInterval);
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {


            }
            return ret;
        }

        private EventCodeEnum CheckThreeLeg()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.WaferTransferModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    if (this.StageSupervisor().StageModuleState.GetState() != StageStateEnum.MANUAL)
                    {
                        bool isthreelegDown = false;
                        for (int retryCount = 0; retryCount <= 5; retryCount++)
                        {
                            try
                            {
                                ret = EventCodeEnum.MONITORING_THREELEG_ERROR;
#if DEBUG
                                isthreelegDown = true;
                                ret = EventCodeEnum.NONE;
                                break;
#else
                                ret = this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegDown);
                                if (ret == EventCodeEnum.NONE)
                                {
                                    break;
                                }
                                else
                                {
                                    if (retryCount == 5)
                                    {
                                        ret = EventCodeEnum.MONITORING_THREELEG_ERROR;
                                    }
                                }
#endif


                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                                if (retryCount < 6)
                                {
                                    continue;
                                }
                                else
                                {
                                    ret = EventCodeEnum.MONITORING_THREELEG_ERROR;

                                }
                            }
                        }

                        if (isthreelegDown != true || ret != EventCodeEnum.NONE)
                        {
                            ret = StageEmergencyStop();
                            ResultValidate(MethodBase.GetCurrentMethod(), ret);
                            ret = EventCodeEnum.MONITORING_THREELEG_ERROR;
                            //WriteProLog(EventCodeEnum.Three_Leg_Error, ret);
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Three_Leg_Error, ret);
                            this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                            IsSystemError = true;
                            IsStageSystemError = true;
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
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                ret = EventCodeEnum.MONITORING_THREELEG_ERROR;
                //WriteProLog(EventCodeEnum.Three_Leg_Error, ret);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Three_Leg_Error, ret);
                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                IsStageSystemError = true;
                IsSystemError = true;
            }
            return ret;
        }


        private async Task<EventCodeEnum> CheckFrontDoor()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                bool isfdoorOpen = false;
                ret = this.StageSupervisor().StageModuleState.IsFrontDoorOpen(ref isfdoorOpen);
                if (ret == EventCodeEnum.NONE)
                {
                    if (isfdoorOpen == true)
                    {
                        ret = EventCodeEnum.MONITORING_FRONT_DOOR_ERROR;
                        //WriteProLog(EventCodeEnum.Three_Leg_Error, ret);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Three_Leg_Error, ret);

                        var retIO = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOBUZZERON, true);
                        if (retIO == IORet.NO_ERR)
                        {
                            foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                            {
                                this.MotionManager().SetFeedrate(axis, 0, 0);
                            }
                            this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                            IsSystemError = true;
                            IsStageSystemError = true;

                            await this.MetroDialogManager().ShowMessageDialog($"MONITORING", $"Front door is opend", EnumMessageStyle.Affirmative);

                        }
                        else
                        {
                            //부저에러 
                            ret = EventCodeEnum.MONITORING_BUZZER_ERROR;
                            //WriteProLog(EventCodeEnum.Three_Leg_Error, ret);
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Front_Door_Error, ret);
                            IsStageSystemError = true;
                            this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                            IsSystemError = true;
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    ret = EventCodeEnum.MONITORING_FRONT_DOOR_ERROR;
                    //WriteProLog(EventCodeEnum.Three_Leg_Error, ret);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Front_Door_Error, ret);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                ret = EventCodeEnum.MONITORING_FRONT_DOOR_ERROR;
                //WriteProLog(EventCodeEnum.Three_Leg_Error, ret);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Front_Door_Error, ret);
                IsStageSystemError = true;
                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                IsSystemError = true;
            }
            return ret;
        }

        private EventCodeEnum CheckLoaderDoor()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool bErrorOccured = false;
            StreamWriter sw = null;
            List<string> loaderDoorErrorList = new List<string>();
            try
            {
                bool isldoorOpen = false;
                if (this.WaferTransferModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    ret = this.StageSupervisor().StageModuleState.IsLoaderDoorOpen(ref isldoorOpen);
                    if (ret == EventCodeEnum.NONE)
                    {
                        if (isldoorOpen == true)
                        {
                            bErrorOccured = true;
                            if (MainAirErrorCount < ErrorCountTolerance)
                            {
                                LoggerManager.Debug($"LoaderDoorError ioValue :{isldoorOpen} LoaderdoorErrorCount:{LoaderDoorErrorCount} in MonitoringManager");

                                string errormsg = $"[{DateTime.Now.ToString()}] LoaderDoorError ioValue :{isldoorOpen} LoaderdoorErrorCount:{LoaderDoorErrorCount} in MonitoringManager";
                                loaderDoorErrorList.Add(errormsg);
                            }
                            else
                            {
                                //Error 멈추기 
                                ret = LoaderEmergencyStop();
                                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                                ret = EventCodeEnum.MONITORING_LOADER_DOOR_ERROR;
                                //WriteProLog(EventCodeEnum.Loader_Door_Error, ret);
                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Loader_Door_Error, ret);
                                IsLoaderSystemError = true;
                                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                                IsSystemError = true;
                            }
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
                if (bErrorOccured)
                {
                    LoaderDoorErrorCount++;
                }
                else
                {
                    LoaderDoorErrorCount = 0;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                ret = EventCodeEnum.MONITORING_LOADER_DOOR_ERROR;
                //WriteProLog(EventCodeEnum.Loader_Door_Error, ret);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Loader_Door_Error, ret);
                IsLoaderSystemError = true;
                this.NotifyManager().Notify(EventCodeEnum.SYSTEM_ERROR);
                IsSystemError = true;
            }
            finally
            {
                if (0 < (loaderDoorErrorList?.Count ?? 0))
                {
                    try
                    {
                        CheckFileLength();

                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in loaderDoorErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }

            }
            return ret;
        }


        public void DEBUG_Check()
        {

        }

        public void DeInitModule()
        {
            try
            {
                // TODO: 
                if (_CheckRunTask != null)
                {
                    _CheckRun = false;

                    _CheckRunTask.Wait();
                }


                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion
    }
}
