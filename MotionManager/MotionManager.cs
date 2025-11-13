using System;
using System.Threading;
using System.ComponentModel;
using System.Timers;
using System.Collections.ObjectModel;
using System.Linq;
using Motion;
using ErrorCompensation;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProberInterfaces;
using Autofac;
using System.Diagnostics;
using System.Reflection;
using ProberErrorCode;
using SystemExceptions.MotionException;
using LogModule;
using ProberInterfaces.Param;
using System.Runtime.CompilerServices;
using System.ServiceModel;

namespace ProbeMotion
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MotionManager : IMotionManager
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region # Monitering Fields
        private int MonitoringInterValInms = 12;
        private DateTime LastUpdateTimeforThreePodCheck = default;

        private object lockObject = new object();
        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        System.Timers.Timer _monitoringTimer;
        bool bStopUpdateThread;
        Thread UpdateThread;
        #endregion

        private bool IsInfo = true;
        public bool Initialized { get; set; } = false;

        private IErrorCompensationManager _ErrorManager;
        public IErrorCompensationManager ErrorManager
        {
            get { return _ErrorManager; }
            set
            {
                if (value != _ErrorManager)
                {
                    _ErrorManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LoaderAxes _LoaderAxes;
        public LoaderAxes LoaderAxes
        {
            get { return _LoaderAxes; }
            set
            {
                if (value != _LoaderAxes)
                {
                    _LoaderAxes = value;
                    RaisePropertyChanged();
                }
            }
        }

        private StageAxes _StageAxes;
        public StageAxes StageAxes
        {
            get { return _StageAxes; }
            set
            {
                if (value != _StageAxes)
                {
                    _StageAxes = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _UpdateProcTime;
        public long UpdateProcTime
        {
            get { return _UpdateProcTime; }
            set
            {
                if (value != _UpdateProcTime)
                {
                    _UpdateProcTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _MaxUpdateProcTime;
        public long MaxUpdateProcTime
        {
            get { return _MaxUpdateProcTime; }
            set
            {
                if (value != _MaxUpdateProcTime)
                {
                    _MaxUpdateProcTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMotionProvider _MotionProvider;
        public IMotionProvider MotionProvider
        {
            get { return _MotionProvider; }
            set
            {
                if (value != _MotionProvider)
                {
                    _MotionProvider = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _XPosForViewer;
        public double XPosForViewer
        {
            get { return _XPosForViewer; }
            set
            {
                if (value != _XPosForViewer)
                {
                    _XPosForViewer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YPosForViewer;
        public double YPosForViewer
        {
            get { return _YPosForViewer; }
            set
            {
                if (value != _YPosForViewer)
                {
                    _YPosForViewer = value;
                    RaisePropertyChanged();
                }
            }
        }
        public IMotionManagerCallback ServiceCallBack { get; private set; }

        ~MotionManager()
        {
            try
            {
                _monitoringTimer?.Stop();
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
                tmpParam = new LoaderAxes();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //RetVal = Extensions_IParam.LoadParameter(ref tmpParam, typeof(LoaderAxes), null, null, Extensions_IParam.FileType.XML);
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderAxes));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    LoaderAxes = tmpParam as LoaderAxes;
                }
                tmpParam = new StageAxes();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //RetVal = Extensions_IParam.LoadParameter(ref tmpParam, typeof(StageAxes), null, null, Extensions_IParam.FileType.XML);
                RetVal = this.LoadParameter(ref tmpParam, typeof(StageAxes));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    StageAxes = tmpParam as StageAxes;
                }

                //SysParam = new IParamEmpty();
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
                RetVal = this.SaveParameter(LoaderAxes);
                RetVal = this.SaveParameter(StageAxes);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        // Unit : ms
        public long MotionDoneTimeOut = 10000;

        private void _monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                areUpdateEvent.Set();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                if (Initialized == false)
                {
                    foreach (ProbeAxisObject axis in LoaderAxes.ProbeAxisProviders)
                    {
                        axis.SettlingTime = (long)axis.Config.SettlingTime.Value;
                    }

                    ErrorManager = new ErrorCompensationManager();
                    MotionProvider = new MotionProvider();

                    retval = ErrorManager.LoadSysParameter();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"ErrorManager.LoadSysParameter() Failed");
                    }

                    retval = ErrorManager.InitModule();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"ErrorManager.InitModule() Failed");
                    }

                    ObservableCollection<AxisObject> allAxes = new ObservableCollection<AxisObject>();

                    if (StageAxes.ProbeAxisProviders != null)
                    {
                        foreach (var item in StageAxes.ProbeAxisProviders)
                        {
                            allAxes.Add(item);
                        }

                        foreach (ProbeAxisObject axis in StageAxes.ProbeAxisProviders)
                        {
                            if (ErrorManager.AssociatedAxisTypeList.Find(Type => axis.AxisType.Value == Type) != EnumAxisConstants.Undefined)
                            {

                                ErrorManager.AssociatedAxes.Add(axis);
                                axis.ErrorModule = ErrorManager;
                            }
                        }
                    }

                    if (LoaderAxes.ProbeAxisProviders != null)
                    {
                        foreach (var item in LoaderAxes.ProbeAxisProviders)
                        {
                            allAxes.Add(item);
                        }
                    }


                    retval = MotionProvider.LoadSysParameter();

                    if (retval != EventCodeEnum.NONE)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.MOTION_CONFIG_FILE_LOADING_FAIL);
                        LoggerManager.Error($"MotionProvider.LoadSysParameter() Failed");
                    }

                    var groupAxes = allAxes.Where(item => (item.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS));

                    foreach (var groupAxis in groupAxes)
                    {
                        groupAxis.GroupMembers = new List<AxisObject>();
                        foreach (var item in allAxes)
                        {
                            if (item.MasterAxis.Value == (groupAxis as ProbeAxisObject).AxisType.Value)
                            {
                                groupAxis.GroupMembers.Add(item);
                            }
                        }
                    }

                    retval = MotionProvider.InitMotionProvider(allAxes);

                    if (retval == EventCodeEnum.NONE)
                    {
                        _monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
                        _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                        //_monitoringTimer.Start();

                        bStopUpdateThread = false;
                        UpdateThread = new Thread(new ThreadStart(UpdateManagerProc));
                        UpdateThread.Name = this.GetType().Name;
                        UpdateThread.Start();
                        //DisableAxes();
                    }
                    else
                    {
                        this.NotifyManager().Notify(EventCodeEnum.MOTION_CONFIG_FILE_LOADING_FAIL);
                        LoggerManager.Error($"MotionProvider.InitMotionProvider(allAxes) Failed");
                    }

                    Initialized = true;

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

                bStopUpdateThread = true;
                UpdateThread?.Join();
                //if (UpdateThread != null) UpdateThread.Join();

                MotionProvider?.DeInitMotionService();

                _monitoringTimer?.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void ValidateMotion(int retVal, string format, params object[] args)
        {
            if (retVal != 0)
            {
                throw new Exception(string.Format(format, args));
            }
        }

        private EventCodeEnum ConvertErrorCode(int retVal)
        {
            try
            {
                switch (retVal)
                {
                    case 0: return EventCodeEnum.NONE;
                    //TODO : 
                    default: return EventCodeEnum.MOTION_MOVING_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #region # Common
        //private int ResultValidate(object funcname, int retcode)
        //{
        //    if ((EnumReturnCodes)retcode != EnumReturnCodes.ReturnCodeOK)
        //    {
        //        throw new MotionException($"Funcname: {funcname.ToString()} ReturnValue: {retcode.ToString()} Error occurred");
        //    }
        //    return retcode;
        //}

        private EventCodeEnum ResultValidate(object funcname, EventCodeEnum errorcode)
        {
            if (errorcode != EventCodeEnum.NONE)
            {
                throw new MotionException($"Funcname: {funcname.ToString()} ReturnValue: {errorcode.ToString()} Error occurred", errorcode);
            }
            return errorcode;
        }
        public ProbeAxisObject GetAxis(EnumAxisConstants axis)
        {
            try
            {
                return MotionProvider?
                    .AxisProviders
                    .AxisObjects
                    .Where(item => item is ProbeAxisObject && (item as ProbeAxisObject).AxisType.Value == axis)
                    .FirstOrDefault() as ProbeAxisObject;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public int GetActualPos(EnumAxisConstants axisType, ref double pos)
        {
            int retVal = -1;
            ProbeAxisObject axis = GetAxis(axisType);
            try
            {
                retVal = MotionProvider.GetActualPosition(axis, ref pos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }
            catch (MotionException ex)
            {
                throw new MotionException("GetActualPos Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetActualPos Error in MotionManager " + ex.Message, ex);
            }

            return retVal;

        }

        public IMotionProvider GetMotionProvider()
        {
            IMotionProvider motionprovider = null;
            try
            {
                motionprovider = this.MotionProvider;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return motionprovider;
        }
        public int GetRefPos(EnumAxisConstants axisType, ref double pos)
        {
            int returnValue = -1;
            try
            {
                ProbeAxisObject axis = GetAxis(axisType);
                pos = axis.Status.Position.Ref;
                returnValue = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return returnValue;
        }
        public int GetRefPos(ref double xpos, ref double ypos, ref double zpos, ref double tpos)
        {
            int retVal = -1;
            try
            {
                ProbeAxisObject axis = GetAxis(EnumAxisConstants.X);

                xpos = axis.Status.Position.Ref;

                axis = GetAxis(EnumAxisConstants.Y);
                ypos = axis.Status.Position.Ref;

                axis = GetAxis(EnumAxisConstants.Z);
                if (axis.Status != null)
                    zpos = axis.Status.Position.Ref;

                axis = GetAxis(EnumAxisConstants.C);
                tpos = axis.Status.Position.Ref;
            }
            catch (MotionException ex)
            {
                throw new MotionException("GetRefPos Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetRefPos Error in MotionManager " + ex.Message, ex);
            }

            return retVal;

        }
        public int GetActualPoss(ref double xpos, ref double ypos, ref double zpos, ref double tpos)
        {
            int retVal = -1;
            try
            {
                ProbeAxisObject axis = GetAxis(EnumAxisConstants.X);
                retVal = MotionProvider.GetActualPosition(axis, ref xpos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                axis = GetAxis(EnumAxisConstants.Y);
                retVal = MotionProvider.GetActualPosition(axis, ref ypos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                axis = GetAxis(EnumAxisConstants.Z);
                retVal = MotionProvider.GetActualPosition(axis, ref zpos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                axis = GetAxis(EnumAxisConstants.C);
                retVal = MotionProvider.GetActualPosition(axis, ref tpos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }
            catch (MotionException ex)
            {
                throw new MotionException("GetActualPoss Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetActualPoss Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int AmpFaultClear(EnumAxisConstants axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.AmpFaultClear(GetAxis(axis));
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("AmpFaultClear Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("AmpFaultClear Error in MotionManager " + ex.Message, ex);
            }

            return retVal;

        }
        public int SetSettlingTime(ProbeAxisObject axis, double settlingTime)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProvider.SetSettlingTime(axis, settlingTime);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetSettlingTime Error " + ex.Message, ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetSettlingTime Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int SetPosition(ProbeAxisObject axis, double val)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProvider.SetPosition(axis, val);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetPosition Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetPosition Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public bool GetIOHome(ProbeAxisObject axis)
        {
            bool value = false;
            try
            {
                value = MotionProvider.GetIOHome(axis);
            }
            catch (Exception ex)
            {
                throw new Exception("GetIOHome Error in MotionManager " + ex.Message, ex);
            }
            return value;
        }
        public int GetCommandPos(ProbeAxisObject axis, ref double pos)
        {
            int retVal = -1;
            double cmdPos = 0;

            try
            {
                retVal = MotionProvider.GetCmdPosition(axis, ref cmdPos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                pos = cmdPos;
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetCommandPos Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetCommandPos Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, long timeout = 0)
        {
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                int ret = WaitForAxisMotionDone(GetAxis(axis), timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(ret));
                rel = EventCodeEnum.NONE;
            }

            catch (MotionException ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                rel = EventCodeEnum.MOTION_MOTIONDONE_ERROR;
                return rel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                rel = EventCodeEnum.MOTION_MOTIONDONE_ERROR;
            }

            return rel;
        }
        public int WaitForAxisVMotionDone(ProbeAxisObject axis, long timeout = 0)
        {
            int retVal = -1;

            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return -1;
                        }
                    }
                }
                retVal = MotionProvider.WaitForVMoveAxisMotionDone(axis, timeout);

                if (retVal != (int)EnumMotionBaseReturnCode.ReturnCodeOK)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_MOTIONDONE_ERROR);
                }

                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("WaitForAxisVMotionDone Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("WaitForAxisVMotionDone Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int WaitForAxisMotionDone(ProbeAxisObject axis, long timeout = 0)
        {
            // 20251030 LJH Elmo 메뉴얼 조그 임시 회피 코드
            // int retVal = -1;
            int retVal = 0;
            return retVal;
            // end

            try
            {

                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return -1;
                        }
                    }
                }
                retVal = MotionProvider.WaitForAxisMotionDone(axis, timeout);

                if (retVal != (int)EnumMotionBaseReturnCode.ReturnCodeOK)
                {
                    this.NotifyManager()?.Notify(EventCodeEnum.MOTION_MOTIONDONE_ERROR);
                }

                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("WaitForAxisMotionDone Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("WaitForAxisMotionDone Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public bool IsEmulMode(ProbeAxisObject axis)
        {
            bool retVal = false;

            try
            {
                retVal = MotionProvider.IsEmulMode(axis);
            }
            catch (Exception ex)
            {
                throw new Exception("IsEmulMode Error in MotionManager " + ex.Message, ex);
            }
            return retVal;
        }

        public int Stop(ProbeAxisObject axis)
        {
            int retVal = -1;

            try
            {


                retVal = MotionProvider.Stop(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                WaitForAxisMotionDone(axis, 1000);
            }

            catch (MotionException ex)
            {
                throw new MotionException("Stop Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("Stop Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int Abort(ProbeAxisObject axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.Abort(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }
            catch (MotionException ex)
            {
                throw new MotionException("Abort Error " + ex.Message, ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("Abort Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public EventCodeEnum ScanJogMove(double xVel, double yVel, EnumTrjType trjtype)
        {
            int retVal = -1;
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }

            double actposX = 0.0;
            double refpulseX = 0.0;
            double refposX = 0.0;

            double actposY = 0.0;
            double refpulseY = 0.0;
            double refposY = 0.0;

            try
            {
                var axisX = GetAxis(EnumAxisConstants.X);
                var axisY = GetAxis(EnumAxisConstants.Y);

                if (xVel == 0 && yVel == 0)
                {
                    MotionProvider.VMoveStop(axisX);
                    MotionProvider.VMoveStop(axisY);

                    retVal = GetActualPos(axisX.AxisType.Value, ref actposX);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    refpulseX = axisX.DtoP(actposX);
                    Math.Truncate(refpulseX - 2);
                    refposX = axisX.PtoD(refpulseX);
                    axisX.Status.RawPosition.Ref = refposX;
                    axisX.Status.Position.Ref = refposX;

                    retVal = GetActualPos(axisY.AxisType.Value, ref actposY);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    refpulseY = axisX.DtoP(actposY);
                    Math.Truncate(refpulseY - 2);
                    refposY = axisX.PtoD(refpulseY);
                    axisY.Status.RawPosition.Ref = refposY;
                    axisY.Status.Position.Ref = refposY;


                    retVal = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                    if (axisX.Status.State == EnumAxisState.MOVING || axisX.Status.AxisBusy == true ||
                        axisY.Status.State == EnumAxisState.MOVING || axisY.Status.AxisBusy == true)
                    {
                        //암것두 안함 
                        rel = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = MotionProvider.WaitForVMoveAxisMotionDone(axisX, axisX.Param.TimeOut.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        retVal = MotionProvider.WaitForVMoveAxisMotionDone(axisY, axisY.Param.TimeOut.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        retVal = MotionProvider.AbsMove(axisX, axisX.Status.RawPosition.Ref, axisX.Param.Speed.Value, axisX.Param.Acceleration.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        retVal = MotionProvider.AbsMove(axisY, axisY.Status.RawPosition.Ref, axisY.Param.Speed.Value, axisY.Param.Acceleration.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                        retVal = MotionProvider.WaitForVMoveAxisMotionDone(axisX, axisX.Param.TimeOut.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        retVal = MotionProvider.WaitForVMoveAxisMotionDone(axisY, axisY.Param.TimeOut.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        rel = ConvertErrorCode(retVal);

                    }

                }

                else
                {
                    if (Math.Abs(xVel) >= 0.1)
                    {
                        retVal = MotionProvider.VMove(axisX, xVel, trjtype);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        rel = ConvertErrorCode(retVal);
                    }
                    else
                    {
                        MotionProvider.VMoveStop(axisX);
                    }
                    if (Math.Abs(yVel) >= 0.1)
                    {
                        retVal = MotionProvider.VMove(axisY, yVel, trjtype);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        rel = ConvertErrorCode(retVal);
                    }
                    else
                    {
                        MotionProvider.VMoveStop(axisY);
                    }
                }
            }
            catch (MotionException ex)
            {
                throw new MotionException("ScanJogMove Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("ScanJogMove Error in MotionManager " + ex.Message, ex);
            }
            return rel;
        }
        public EventCodeEnum VMove(EnumAxisConstants axis, double vel, EnumTrjType trjtype)
        {
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                rel = VMove(GetAxis(axis), vel, trjtype);
                ResultValidate(MethodBase.GetCurrentMethod(), rel);
            }

            catch (MotionException ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                rel = EventCodeEnum.MOTION_VMOVING_ERROR;
                return rel;
                //throw new MotionException($"RelMove Error Axis:{axis.ToString()}" + ex.Message, ex, EventCodeEnum.MOTION_MOVING_ERROR, this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                //throw new Exception("RelMove Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }

        public EventCodeEnum VMove(ProbeAxisObject axis, double vel, EnumTrjType trjtype)
        {
            int retVal = -1;
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            double actpos = 0.0;
            double refpulse = 0.0;
            double refpos = 0.0;
            try
            {
                //lock(axis)
                // {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                lock (axis)
                {
                    if (vel == 0)
                    {
                        MotionProvider.VMoveStop(axis);
                        MotionProvider.WaitForVMoveAxisMotionDone(axis, axis.Param.TimeOut.Value);
                        retVal = GetActualPos(axis.AxisType.Value, ref actpos);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        refpulse = axis.DtoP(actpos);
                        Math.Truncate(refpulse - 2);
                        refpos = axis.PtoD(refpulse);
                        axis.Status.RawPosition.Ref = refpos;
                        axis.Status.Position.Ref = refpos;

                        //retVal = MotionProvider.AbsMove(axis, refpos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                        ////rel = AbsMove(axis.AxisType.Value, refpos);
                        //ResultValidate(MethodBase.GetCurrentMethod(), retVal);


                        if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis))
                        {

                            retVal = ErrorManager.CalcErrorComp();
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                            foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                            {
                                if (compAxis.Status.State == EnumAxisState.MOVING || compAxis.Status.AxisBusy == true)
                                {
                                    //암것두 안함 
                                }
                                else
                                {
                                    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                                    if (axis.AxisType.Value == compAxis.AxisType.Value)
                                    {
                                        retVal = MotionProvider.WaitForVMoveAxisMotionDone(compAxis, compAxis.Param.TimeOut.Value);
                                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                                    }
                                    else
                                    {
                                        retVal = WaitForAxisMotionDone(compAxis, compAxis.Param.TimeOut.Value);
                                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                                    }
                                }

                            }

                        }
                        else
                        {
                            //20170822 JUNE : LOADER MOTION DONE이 안됨.
                            //retVal = WaitForAxisMotionDone(axis);
                            //ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            retVal = MotionProvider.AbsMove(axis, refpos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                            retVal = MotionProvider.WaitForVMoveAxisMotionDone(axis, axis.Param.TimeOut.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                            //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");

                        }

                        rel = EventCodeEnum.NONE;
                    }
                    else
                    {
                        //rel = AssociatedAxesWaitForMotionDone(axis);
                        //ResultValidate(MethodBase.GetCurrentMethod(), rel);

                        retVal = MotionProvider.VMove(axis, vel, trjtype);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        rel = ConvertErrorCode(retVal);

                    }
                    //}
                }
            }

            catch (MotionException ex)
            {
                throw new MotionException("VMove Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("VMove Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }
        //public Task<EventCodeEnum> VMoveAsync(ProbeAxisObject axis, double vel, EnumTrjType trjtype)
        //{
        //    return Task.Run(() => VMoveAsync(axis, vel, trjtype));

        //}


        public EventCodeEnum HomingTaskRun(params EnumAxisConstants[] axes)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                List<Task<EventCodeEnum>> tasklist = new List<Task<EventCodeEnum>>();
                //List<EventCodeEnum> parallellist = new List<EventCodeEnum>();

                //Parallel.For(0, axes.Length, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, i =>
                //{
                //    ret = Homing(axes[i]);
                //    parallellist.Add(ret);
                //});


                foreach (var axistype in axes)
                {
                    var task = Task.Run(() =>
                    {
                        return Homing(axistype);
                    });
                    tasklist.Add(task);
                }

                var retArr = Task.WhenAll(tasklist).Result;
                int errCount = retArr.Count(item => item != EventCodeEnum.NONE);
                if (errCount > 0)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_HOMING_ERROR);
                    ret = EventCodeEnum.MOTION_HOMING_ERROR;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (MotionException ex)
            {
                throw new MotionException("Homming Error " + ex.Message, ex, ex.ErrorCode, -5, this);
            }
            catch (Exception ex)
            {

                throw new Exception("VMove Error in MotionManager " + ex.Message, ex);
            }

            return ret;

        }

        public EventCodeEnum Homing(EnumAxisConstants axis)
        {
            EventCodeEnum rel = EventCodeEnum.MOTION_HOMING_ERROR;
            try
            {
                rel = Homing(GetAxis(axis));
            }

            catch (MotionException ex)
            {
                throw new MotionException($"Homing {axis} Error " + ex.Message, ex, ex.ErrorCode, (int)rel, this);
            }
            catch (Exception ex)
            {
                throw new Exception("Homing Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }

        public EventCodeEnum Homing(ProbeAxisObject axis)
        {
            int retVal = -1;
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = MotionProvider.Homming(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                rel = ConvertErrorCode(retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("Homing Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("Homing Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }

        //public Task<EventCodeEnum> HomingAsync(EnumAxisConstants axis)
        //{
        //    return HomingAsync(GetAxis(axis));
        //}

        public Task<EventCodeEnum> HomingAsync(ProbeAxisObject axis)
        {
            Task<EventCodeEnum> task;
            try
            {
                task = Task.Run(() => Homing(axis));
            }
            catch (MotionException ex)
            {
                throw new MotionException("HomingAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("HomingAsync Error in MotionManager " + ex.Message, ex);
            }

            return task;

        }

        #endregion
        #region Loader Move Methods
        public EventCodeEnum LoaderSystemInit()
        {
            int retVal = -1;
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {

                int failedCount = 0;
                foreach (HomingGroup hominggroup in LoaderAxes.HomingGroups)
                {
                    var axisQuery = LoaderAxes
                        .ProbeAxisProviders
                        .Where(axis => axis.AxisType.Value == hominggroup.Stage.Value.Find(eax => eax == axis.AxisType.Value));

                    var retArr = Task.WhenAll(axisQuery.Select(axis => HomingAsync(axis))).Result;
                    failedCount = retArr.Count(item => item != EventCodeEnum.NONE);
                    if (failedCount > 0)
                        break;
                }

                if (failedCount == 0)
                    rel = EventCodeEnum.NONE;
                else
                    rel = EventCodeEnum.MOTION_MOVING_ERROR;//Homing err.
            }
            catch (MotionException ex)
            {
                throw new MotionException("LoaderSystemInit Error " + ex.Message, ex, EventCodeEnum.MOTION_HOMING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                rel = ConvertErrorCode(retVal);
                throw new Exception("LoaderSystemInit Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }

        //public EventCodeEnum HommingVaxis(ProbeAxisObject axis, double speed, EnumInputLevel inputLevel)
        //{
        //    EventCodeEnum rel = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        var retVal = MotionProvider.SetSwitchAction(axis, axis.Config.InputHome, EnumEventActionType.ActionSTOP, inputLevel);
        //        ValidateMotion(retVal, "SetSwitchAction() : Error occurred. Axis = {0}, retVal = {1}", axis.Label, retVal);
        //        retVal = MotionProvider.VMove(axis, speed, EnumTrjType.Homming);
        //        ValidateMotion(retVal, "VMove() : Error occurred. Axis = {0}, retVal = {1}", axis.Label, retVal);

        //        retVal = MotionProvider.WaitForAxisEvent(axis, EnumAxisState.StateSTOPPED, 0);
        //        ValidateMotion(retVal, "WaitForAxisEvent() : Error occurred. Axis = {0}, retVal = {1}", axis.Label, retVal);

        //        MotionProvider.ClearUserLimit(axis);

        //        retVal = MotionProvider.AlarmReset(axis);
        //        ValidateMotion(retVal, "AlarmReset() : Error occurred. Axis = {0}, retVal = {1}", axis.Label, retVal);

        //        rel = EventCodeEnum.NONE;
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerManager.Error($ex.Message);
        //        rel = EventCodeEnum.MOTION_MOVING_ERROR;
        //    }
        //    return rel;
        //}
        #endregion
        #region Stage Move Methods
        public EventCodeEnum StageSystemInit()
        {
            int retVal = -1;

            EventCodeEnum rel = EventCodeEnum.UNDEFINED;

            try
            {
                //retVal = ForcedZDown();
                //ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //int failedCount = 0;

                //foreach (HomingGroup hominggroup in StageAxes.HomingGroups)
                //{
                //    var axisQuery = StageAxes
                //        .ProbeAxisProviders
                //        .Where(axis => axis.AxisType.Value == hominggroup.Stage.Value.Find(eax => eax == axis.AxisType.Value));

                //    var retArr = Task.WhenAll(axisQuery.Select(axis => HomingAsync(axis))).Result;

                //    failedCount = retArr.Count(item => item != EventCodeEnum.NONE);

                //    if (failedCount > 0)
                //        break;
                //}

                //if (failedCount == 0)
                //    rel = EventCodeEnum.NONE;
                //else
                //    rel = EventCodeEnum.MOTION_MOVING_ERROR;//Homing err.

                rel = this.StageSupervisor().StageModuleState.StageSystemInit();
                ResultValidate(MethodBase.GetCurrentMethod(), rel);

            }

            catch (MotionException ex)
            {
                throw new MotionException("StageSystemInit Error " + ex.Message, ex, EventCodeEnum.MOTION_STAGE_INIT_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                rel = ConvertErrorCode(retVal);
                throw new Exception("StageSystemInit Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }

        public Task<EventCodeEnum> StageSystemInitAsync()
        {
            Task<EventCodeEnum> task;

            try
            {
                task = Task.Run(() => StageSystemInit());
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageSystemInitAsync Error " + ex.Message, ex, EventCodeEnum.MOTION_STAGE_INIT_ERROR, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageSystemInitAsync Error in MotionManager " + ex.Message, ex);
            }
            return task;
        }

        private Task<int> WaitMotionDoneAsync(ProbeAxisObject axis, CancellationToken token)
        {
            int retVal = -1;

            try
            {
                return Task.Run(() =>
                {
                    if (this.MonitoringManager().IsSystemError == true)
                    {
                        if (this.MonitoringManager().IsMachineInitDone == true)
                        {

                        }
                        else
                        {
                            if (this.MonitoringManager().IsMachinInitOn != true)
                            {
                                return -1;
                            }
                        }
                    }
                    retVal = MotionProvider.WaitForAxisMotionDone(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    return retVal;
                });
            }
            catch (MotionException ex)
            {
                throw new MotionException("WaitMotionDoneAsync Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("WaitMotionDoneAsync Error in MotionManager " + ex.Message, ex);
            }

        }

        #region //Motion Edit
        public EventCodeEnum AbsMove(EnumAxisConstants axis, double abspos)
        {
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                rel = AbsMove(GetAxis(axis), abspos);
                ResultValidate(MethodBase.GetCurrentMethod(), rel);
            }

            catch (MotionException ex)
            {
                throw new MotionException("AbsMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("AbsMove Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }
        public EventCodeEnum AbsMove(ProbeAxisObject axis, double abspos)
        {
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                rel = AbsMove(axis, abspos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), rel);
            }
            catch (MotionException ex)
            {
                throw new MotionException("AbsMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                throw new Exception("AbsMove Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }

        public EventCodeEnum AbsMove(ProbeAxisObject axis, double abspos, double vel, double acc)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            int returnValue = -1;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }

                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                if (abspos > axis.Param.PosSWLimit.Value)
                {
                    // targetPos = abspos;
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);

                    throw new MotionException($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {abspos}, Limit = {axis.Param.PosSWLimit.Value}", EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                }
                else if (abspos < axis.Param.NegSWLimit.Value)
                {
                    // targetPos = abspos;
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);

                    throw new MotionException($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {abspos}, Limit = {axis.Param.NegSWLimit.Value}", EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                }
                else
                {
                    axis.Status.RawPosition.Ref = abspos;
                    axis.Status.Position.Ref = abspos;

                    if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis))
                    {
                        returnValue = ErrorManager.CalcErrorComp();
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                        {
                            //compAxis.Status.AxisBusy = true;
                            if (compAxis == axis)
                            {
                                returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc);
                                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                            }
                            else
                            {
                                returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                            }
                        }
                    }
                    else
                    {
                        //20170822 JUNE : LOADER MOTION DONE이 안됨.
                        returnValue = WaitForAxisMotionDone(axis);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        axis.Status.AxisBusy = true;

                        returnValue = MotionProvider.AbsMove(axis, abspos, vel, acc);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                        returnValue = WaitForAxisMotionDone(axis);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");
                    }
                    ret = AssociatedAxesWaitForMotionDone(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                    ret = EventCodeEnum.NONE;
                }
            }

            catch (MotionException ex)
            {
                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new MotionException($"AbsMove Error Axis{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new Exception($"AbsMove Error in MotionManager Axis{axis.AxisType.Value}" + ex.Message, ex);
            }

            return ret;
        }
        public EventCodeEnum AbsMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int returnValue = -1;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);


                if (pos > axis.Param.PosSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return ret;
                }
                else if (pos < axis.Param.NegSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    return ret;
                }

                axis.Status.RawPosition.Ref = pos;
                axis.Status.Position.Ref = pos;


                if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis))
                {
                    returnValue = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        //compAxis.Status.AxisBusy = true;
                        if (compAxis == axis)
                        {
                            returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                        }
                        else
                        {
                            returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        }
                    }

                }
                else
                {
                    //20170822 JUNE : LOADER MOTION DONE이 안됨.
                    returnValue = WaitForAxisMotionDone(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                    axis.Status.AxisBusy = true;
                    returnValue = MotionProvider.AbsMove(axis, pos, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                    returnValue = WaitForAxisMotionDone(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                    //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");

                }
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                ret = EventCodeEnum.NONE;

            }
            catch (MotionException ex)
            {
                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new MotionException($"AbsMove Error Axis{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new Exception("AbsMove Error in MotionManager " + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum AbsMove(ProbeAxisObject axis, double abspos, double vel, double acc, double dcc)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int returnValue = -1;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                //TODO 보상이 꺼져있을때 해당 Axis만 WaitforAxis 변경해야 함 . 
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);


                if (abspos > axis.Param.PosSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return ret;
                }
                else if (abspos < axis.Param.NegSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    return ret;
                }

                axis.Status.RawPosition.Ref = abspos;
                axis.Status.Position.Ref = abspos;


                if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis))
                {
                    returnValue = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        //compAxis.Status.AxisBusy = true;
                        if (compAxis == axis)
                        {
                            returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc, dcc);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        }
                        else
                        {
                            returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        }
                    }

                }
                else
                {
                    //20170822 JUNE : LOADER MOTION DONE이 안됨.
                    returnValue = WaitForAxisMotionDone(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                    axis.Status.AxisBusy = true;
                    returnValue = MotionProvider.AbsMove(axis, abspos, vel, acc);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                    returnValue = WaitForAxisMotionDone(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                    //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");


                }
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                ret = EventCodeEnum.NONE;

            }
            catch (MotionException ex)
            {
                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new MotionException($"AbsMove Error Axis{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new Exception("AbsMove Error in MotionManager " + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum RelMove(EnumAxisConstants axis, double pos)
        {

            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                rel = RelMove(GetAxis(axis), pos);
                ResultValidate(MethodBase.GetCurrentMethod(), rel);
            }

            catch (MotionException ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                rel = EventCodeEnum.MOTION_REL_MOVING_ERROR;
                return rel;
                //throw new MotionException($"RelMove Error Axis:{axis.ToString()}" + ex.Message, ex, EventCodeEnum.MOTION_MOVING_ERROR, this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                //throw new Exception("RelMove Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }
        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos)
        {

            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                rel = RelMove(axis, pos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), rel);
            }

            catch (MotionException ex)
            {
                throw new MotionException($"RelMove Error Axis:{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                throw new Exception("RelMove Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }
        public EventCodeEnum RelMoveZAxis(double pos, double vel, double acc)
        {
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            stw.Restart();
            stw.Start();

            int retVal = -1;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            AxisObject axisZ = GetAxis(EnumAxisConstants.Z);
            try
            {


                timeStamp.Add(new KeyValuePair<string, long>(string.Format("RelMove Start"), stw.ElapsedMilliseconds));
                retVal = MotionProvider.RelMove(axisZ, pos, vel, acc);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                timeStamp.Add(new KeyValuePair<string, long>(string.Format("RelMove End"), stw.ElapsedMilliseconds));
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("WaitFor Start"), stw.ElapsedMilliseconds));
                retVal = MotionProvider.WaitForAxisMotionDone(axisZ);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                timeStamp.Add(new KeyValuePair<string, long>(string.Format("WaitFor End"), stw.ElapsedMilliseconds));

                ret = EventCodeEnum.NONE;

            }
            catch (MotionException ex)
            {
                throw new MotionException("RelMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                throw new Exception("RelMove Error in MotionManager " + ex.Message, ex);
            }

            stw.Stop();
            return ret;

        }
        public EventCodeEnum AssociatedAxesWaitForMotionDone(ProbeAxisObject axis)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }

                if (ErrorManager.AssociatedAxes.Where(item => item.AxisType.Value == axis.AxisType.Value).FirstOrDefault() != null)
                {
                    // Task factory method.
                    var taskList = new List<Task<int>>();
                    foreach (var ass in ErrorManager.AssociatedAxes)
                    {
                        taskList.Add(Task.Factory.StartNew<int>(() => WaitForAxisMotionDone(ass, ass.Param.TimeOut.Value)));
                    }
                    Task.WaitAll(taskList.ToArray());

                    var errorTasks = taskList.FindAll(t => t.Result != 0);

                    if (errorTasks.Count > 0)
                    {
                        retVal = EventCodeEnum.MOTION_MOTIONDONE_ERROR;
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    //int retCode = -1;`
                    //for (int i = 0; i < ErrorManager.AssociatedAxes.Count(); i++)
                    //{
                    //    retCode = WaitForAxisMotionDone(
                    //        ErrorManager.AssociatedAxes[i], 
                    //        ErrorManager.AssociatedAxes[i].Param.TimeOut.Value);
                    //    if(retCode != 0)
                    //    {
                    //        retVal = EventCodeEnum.MOTION_MOTIONDONE_ERROR;
                    //    }
                    //}
                    //if(retVal != EventCodeEnum.MOTION_MOTIONDONE_ERROR)
                    //{
                    //    retVal = EventCodeEnum.NONE;
                    //}
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }



            }
            catch (MotionException ex)
            {
                throw new MotionException("AssociatedAxesWaitForMotionDone Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return retVal;
        }
        public bool IsAssociatedAxes(ProbeAxisObject axis)
        {
            bool bRet = false;
            try
            {
                if (ErrorManager.AssociatedAxes.Where(item => item.AxisType.Value == axis.AxisType.Value).FirstOrDefault() != null)
                {
                    bRet = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return bRet;
        }
        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos, double vel, double acc)
        {

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();
            int retVal = -1;

            stw.Restart();
            stw.Start();
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                //==> 왜 다른 축들을 기다 리는 것인가??
                //timeStamp.Add(new KeyValuePair<string, long>("Pre-WaitForMotion", stw.ElapsedMilliseconds));

                //ret = AssociatedAxesWaitForMotionDone(axis);
                //ResultValidate(MethodBase.GetCurrentMethod(), ret);
                retVal = WaitForAxisMotionDone(axis);
                //timeStamp.Add(new KeyValuePair<string, long>("WaitForMotion", stw.ElapsedMilliseconds));

                double cpos = 0;

                //timeStamp.Add(new KeyValuePair<string, long>("Position update", stw.ElapsedMilliseconds));

                var IsEmul = IsEmulMode(axis);

                double actual;

                if (!IsEmul)
                {
                    actual = axis.Status.Position.Actual;
                }
                else
                {
                    actual = axis.Status.Position.Ref;
                }

                //251106 sebas limit 해제
                //if (pos + actual > axis.Param.PosSWLimit.Value)
                if (false)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);

                    throw new MotionException($"Positive SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {pos}, Limit = {axis.Param.PosSWLimit.Value}", EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);

                    //throw new MotionException(string.Format("Positive SW Limit occurred while Relative moving for Axis {0}, Target = {1}, Limit = {2}",
                    //    axis.Label, pos, axis.Param.PosSWLimit.Value));
                }
                //else if (pos + actual < axis.Param.NegSWLimit.Value)
                else if (false)
                        {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);

                    throw new MotionException($"Negative SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {pos}, Limit = {axis.Param.NegSWLimit.Value}", EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);

                    //throw new MotionException(string.Format("Negative SW Limit occurred while Relative moving for Axis {0}, Target = {1}, Limit = {2}",
                    //    axis.Label, pos, axis.Param.NegSWLimit.Value));
                }
                else
                {
                    retVal = GetCommandPos(axis, ref cpos);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    axis.Status.RawPosition.Ref = cpos + pos;
                    axis.Status.Position.Ref += pos;

                    // 20241031 LJH 메뉴얼조그를 위한 회피
                    //if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis) && axis.AxisType.Value != EnumAxisConstants.Z)
                    //{
                    //    retVal = ErrorManager.CalcErrorComp();
                    //    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    //    //timeStamp.Add(new KeyValuePair<string, long>("Calc. Error", stw.ElapsedMilliseconds));

                    //    //==> 다른 축들도 움직여야 한다.??
                    //    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    //    {
                    //        if (compAxis == axis)
                    //        {
                    //            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc);
                    //            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    //        }
                    //        else
                    //        {
                    //            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                    //            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    //        }

                    //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move {0}axis", compAxis.Label), stw.ElapsedMilliseconds));
                    //    }

                    //    foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                    //    {
                    //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("WaitForAxisMotionDone start  {0}axis", compAxis.Label), stw.ElapsedMilliseconds));
                    //        retVal = MotionProvider.WaitForAxisMotionDone(compAxis, 0);
                    //        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("WaitForAxisMotionDone end {0}axis", compAxis.Label), stw.ElapsedMilliseconds));
                    //    }
                    //}
                    //else
                    // end ㅣjh
                    {
                        //20170926 JUNE : LOADER MOTION DONE이 안됨.
                        //retCode = WaitForAxisMotionDone(axis, 0);

                        //timeStamp.Add(new KeyValuePair<string, long>("RelMove Start", stw.ElapsedMilliseconds));


                        //Log.Debug("[LOADER REL MOVE] START: axis={0}", axis.AxisType);
                        retVal = MotionProvider.RelMove(axis, pos, vel, acc);

                        //Log.Debug("[LOADER REL MOVE] END: axis={0}", axis.AxisType);
                        //retVal = MotionProvider.AbsMove(axis, 0, vel, acc);

                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        //timeStamp.Add(new KeyValuePair<string, long>("RelMove End", stw.ElapsedMilliseconds));

                        retVal = WaitForAxisMotionDone(axis);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");
                    }
                }
                //==> 다른 축들을 기다림
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Entering wait for axes done."), stw.ElapsedMilliseconds));
                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retCode = MotionProvider.WaitForAxisMotionDone(compAxis, 0);
                //    if (retCode != 0)
                //    {
                //        throw new Exception("WaitForAxisMotionDone() error occurred.");
                //    }
                //}
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Motion done"), stw.ElapsedMilliseconds));
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                ret = EventCodeEnum.NONE;
            }

            catch (MotionException ex)
            {

                throw new MotionException($"RelMove Error Axis:{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                throw new Exception("RelMove Error in MotionManager" + ex.Message, ex);
            }
            finally
            {

            }
            stw.Stop();

            return ret;
        }
        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            int retVal = -1;
            stw.Restart();
            stw.Start();

            //==> 왜 다른 축들을 기다 리는 것인가??
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                timeStamp.Add(new KeyValuePair<string, long>("Pre-WaitForMotion", stw.ElapsedMilliseconds));
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                timeStamp.Add(new KeyValuePair<string, long>("WaitForMotion", stw.ElapsedMilliseconds));

                double cpos = 0;
                GetCommandPos(axis, ref cpos);

                if (cpos + pos > axis.Param.PosSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return ret;
                }
                else if (cpos + pos < axis.Param.NegSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    return ret;
                }

                axis.Status.RawPosition.Ref = cpos + pos;
                axis.Status.Position.Ref += pos;

                timeStamp.Add(new KeyValuePair<string, long>("Position update", stw.ElapsedMilliseconds));



                if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis) && axis.AxisType.Value != EnumAxisConstants.Z)
                {
                    retVal = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    timeStamp.Add(new KeyValuePair<string, long>("Calc. Error", stw.ElapsedMilliseconds));


                    //==> 다른 축들도 움직여야 한다.??
                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        if (compAxis == axis)
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        else
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }
                        timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move {0}axis", compAxis.Label), stw.ElapsedMilliseconds));
                    }
                    foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                    {
                        retVal = MotionProvider.WaitForAxisMotionDone(compAxis, 0);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                    }
                }
                else
                {
                    //20170926 JUNE : LOADER MOTION DONE이 안됨.
                    //retCode = WaitForAxisMotionDone(axis, 0);

                    timeStamp.Add(new KeyValuePair<string, long>("RelMove Start", stw.ElapsedMilliseconds));

                    retVal = MotionProvider.RelMove(axis, pos, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    timeStamp.Add(new KeyValuePair<string, long>("RelMove End", stw.ElapsedMilliseconds));

                    retVal = WaitForAxisMotionDone(axis, 0);
                    //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");

                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                }

                //==> 다른 축들을 기다림
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Entering wait for axes done."), stw.ElapsedMilliseconds));
                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retCode = MotionProvider.WaitForAxisMotionDone(compAxis, 0);
                //    if (retCode != 0)
                //    {
                //        throw new Exception("WaitForAxisMotionDone() error occurred.");
                //    }
                //}
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Motion done"), stw.ElapsedMilliseconds));

                ret = EventCodeEnum.NONE;
            }
            catch (MotionException ex)
            {
                throw new MotionException($"RelMove Error Axis{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                throw new Exception("RelMove Error in MotionManager" + ex.Message, ex);
            }

            stw.Stop();

            return ret;
        }
        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos, double vel, double acc, double dcc)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            int retVal = -1;

            stw.Restart();
            stw.Start();
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                //==> 왜 다른 축들을 기다 리는 것인가??
                timeStamp.Add(new KeyValuePair<string, long>("Pre-WaitForMotion", stw.ElapsedMilliseconds));
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                timeStamp.Add(new KeyValuePair<string, long>("WaitForMotion", stw.ElapsedMilliseconds));

                double cpos = 0;
                retVal = GetCommandPos(axis, ref cpos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                if (cpos + pos > axis.Param.PosSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return ret;
                }
                else if (cpos + pos < axis.Param.NegSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    return ret;
                }
                axis.Status.RawPosition.Ref = cpos + pos;
                axis.Status.Position.Ref += pos;
                timeStamp.Add(new KeyValuePair<string, long>("Position update", stw.ElapsedMilliseconds));


                if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis) && axis.AxisType.Value != EnumAxisConstants.Z)
                {
                    retVal = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    timeStamp.Add(new KeyValuePair<string, long>("Calc. Error", stw.ElapsedMilliseconds));


                    //==> 다른 축들도 움직여야 한다.??
                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        if (compAxis == axis)
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc, dcc);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        else
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move {0}axis", compAxis.Label), stw.ElapsedMilliseconds));
                    }
                    foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                    {
                        retVal = MotionProvider.WaitForAxisMotionDone(compAxis, 0);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    }
                }
                else
                {
                    //20170926 JUNE : LOADER MOTION DONE이 안됨.
                    //retCode = WaitForAxisMotionDone(axis, 0);
                    if (retVal != 0)
                    {
                        var msg = string.Format("MotionManger.WaitForAxisMotionDone(axis,abspos,vel,acc) Error relmove");
                        throw new Exception(msg);
                    }
                    timeStamp.Add(new KeyValuePair<string, long>("RelMove Start", stw.ElapsedMilliseconds));

                    retVal = MotionProvider.RelMove(axis, pos, vel, acc);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    timeStamp.Add(new KeyValuePair<string, long>("RelMove End", stw.ElapsedMilliseconds));

                    retVal = WaitForAxisMotionDone(axis, 0);
                    //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");

                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                }

                //==> 다른 축들을 기다림
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Entering wait for axes done."), stw.ElapsedMilliseconds));
                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retCode = MotionProvider.WaitForAxisMotionDone(compAxis, 0);
                //    if (retCode != 0)
                //    {
                //        throw new Exception("WaitForAxisMotionDone() error occurred.");
                //    }
                //}
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Motion done"), stw.ElapsedMilliseconds));

                ret = EventCodeEnum.NONE;
            }
            catch (MotionException ex)
            {
                throw new MotionException($"RelMove Error Axis{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                throw new Exception("RelMove Error in MotionManager" + ex.Message, ex);
            }
            stw.Stop();

            return ret;
        }
        public EventCodeEnum AbsFeedRateMove(ProbeAxisObject axis, double abspos, double vel, double acc)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int retVal = -1;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                if (abspos > axis.Param.PosSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return ret;
                }
                else if (abspos < axis.Param.NegSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                    ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    return ret;
                }
                axis.Status.RawPosition.Ref = abspos;
                axis.Status.Position.Ref = abspos;


                if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis))
                {
                    retVal = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        if (compAxis == axis)
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        else
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                    }

                }
                else
                {
                    retVal = MotionProvider.AbsMove(axis, abspos, vel, acc);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                }
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                ret = EventCodeEnum.NONE;

            }
            catch (MotionException ex)
            {
                throw new MotionException("AbsFeedRateMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                throw new Exception("AbsFeedRateMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum AbsMoveWithSpeedRate(ProbeAxisObject axis, double abspos, double vel, double acc, ProbingSpeedRateList SpeedRateList)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int returnValue = -1;
            var feedRateList = SpeedRateList?.OrderBy(i => i.SectionRate);

            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                ret = AssociatedAxesWaitForMotionDone(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                if (abspos > axis.Param.PosSWLimit.Value)
                {
                    //this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                    // targetPos = abspos;
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);

                    throw new MotionException($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {abspos}, Limit = {axis.Param.PosSWLimit.Value}", EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);

                    //throw new MotionException(string.Format(
                    //    "Positive SW Limit occurred while AbsMove moving for Axis {0}, Target = {1}, Limit = {2}",
                    //    axis.Label, abspos, axis.Param.PosSWLimit.Value));
                }
                else if (abspos < axis.Param.NegSWLimit.Value)
                {
                    //this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);

                    // targetPos = abspos;
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);

                    throw new MotionException($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {abspos}, Limit = {axis.Param.NegSWLimit.Value}", EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);

                    //throw new MotionException(string.Format(
                    //    "Negative SW Limit occurred while AbsMove moving for Axis {0}, Target = {1}, Limit = {2}",
                    //    axis.Label, abspos, axis.Param.NegSWLimit.Value));
                }
                else
                {
                    axis.Status.RawPosition.Ref = abspos;
                    axis.Status.Position.Ref = abspos;

                    if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true && IsAssociatedAxes(axis))
                    {
                        returnValue = ErrorManager.CalcErrorComp();
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                        {
                            double speedVal = 0;
                            double accVal = 0;

                            if (compAxis == axis)
                            {
                                speedVal = vel;
                                accVal = acc;
                            }
                            else
                            {
                                speedVal = compAxis.Param.Speed.Value;
                                accVal = compAxis.Param.Acceleration.Value;
                            }

                            if (feedRateList == null || feedRateList.Count() < 1)
                            {
                                returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, speedVal, accVal);
                                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                            }
                            else
                            {
                                double curPos = 0;
                                GetActualPos(compAxis.AxisType.Value, ref curPos);

                                foreach (ProbingFeedRate feedRate in feedRateList)
                                {
                                    double sectionPos = curPos + ((abspos - curPos) / feedRate.SectionRate);
                                    this.SetFeedrate(compAxis, feedRate.FeedRate, feedRate.FeedRate);
                                    returnValue = MotionProvider.AbsMove(compAxis, sectionPos, speedVal, accVal);
                                }

                                returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, speedVal, accVal);
                                this.SetFeedrate(compAxis, 1, 1);
                            }
                        }
                    }
                    else
                    {
                        //20170822 JUNE : LOADER MOTION DONE이 안됨.
                        returnValue = WaitForAxisMotionDone(axis);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        //20181016 Jake. for soft touch.
                        if (feedRateList == null || feedRateList.Count() < 1)
                        {
                            returnValue = MotionProvider.AbsMove(axis, abspos, vel, acc);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                        }
                        else
                        {
                            double curPos = 0;
                            GetActualPos(axis.AxisType.Value, ref curPos);

                            foreach (ProbingFeedRate feedRate in feedRateList)
                            {
                                double sectionPos = curPos + ((abspos - curPos) / feedRate.SectionRate);
                                this.SetFeedrate(axis, feedRate.FeedRate, feedRate.FeedRate);
                                returnValue = MotionProvider.AbsMove(axis, sectionPos, vel, acc);
                            }

                            returnValue = MotionProvider.AbsMove(axis, abspos, vel, acc);
                            this.SetFeedrate(axis, 1, 1);
                        }


                        returnValue = WaitForAxisMotionDone(axis);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                        //Log.Debug($"MotionManger.WaitForAxisMotionDone() : axis={axis.AxisType.Value}");
                    }
                    ret = AssociatedAxesWaitForMotionDone(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                    ret = EventCodeEnum.NONE;
                }
            }

            catch (MotionException ex)
            {
                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new MotionException($"AbsMove Error Axis{axis.AxisType.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {

                ret = EventCodeEnum.MOTION_MOVING_ERROR;
                throw new Exception($"AbsMove Error in MotionManager Axis{axis.AxisType.Value}" + ex.Message, ex);
            }

            return ret;
        }
        public EventCodeEnum TiltingMove(double tz1pos, double tz2pos, double tz3pos)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            int retVal = -1;
            ProbeAxisObject tz1axis = GetAxis(EnumAxisConstants.TZ1);
            ProbeAxisObject tz2axis = GetAxis(EnumAxisConstants.TZ2);
            ProbeAxisObject tz3axis = GetAxis(EnumAxisConstants.TZ3);
            List<ProbeAxisObject> tiltaxes = new List<ProbeAxisObject>();
            tiltaxes.Add(tz1axis);
            tiltaxes.Add(tz2axis);
            tiltaxes.Add(tz3axis);

            foreach (ProbeAxisObject axis in tiltaxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(tz1axis, tz1pos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(tz2axis, tz2pos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(tz3axis, tz3pos));

            tz1axis.Status.RawPosition.Ref = tz1pos;
            tz1axis.Status.Position.Ref = tz1pos;

            tz2axis.Status.RawPosition.Ref = tz2pos;
            tz2axis.Status.Position.Ref = tz2pos;

            tz3axis.Status.RawPosition.Ref = tz3pos;
            tz3axis.Status.Position.Ref = tz3pos;

            try
            {
                retVal = MotionProvider.AbsMove(tz1axis, tz1axis.Status.RawPosition.Ref, tz1axis.Param.Speed.Value, tz1axis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));



                retVal = MotionProvider.AbsMove(tz2axis, tz2axis.Status.RawPosition.Ref, tz2axis.Param.Speed.Value, tz2axis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(tz3axis, tz3axis.Status.RawPosition.Ref, tz3axis.Param.Speed.Value, tz3axis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                foreach (ProbeAxisObject axis in tiltaxes)
                {
                    retVal = WaitForAxisMotionDone(axis, 0);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                ret = EventCodeEnum.NONE;
                //return EventCodeEnum.MOTION_MOVING_ERROR;

            }
            catch (MotionException ex)
            {
                throw new MotionException("TiltingMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("TiltingMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum ChuckTiltMove(double rpos, double ttpos)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            if (rpos > 360)
            {
                rpos = rpos % 360;
            }

            int retVal = -1;
            double a = 0d;
            double b = 0d;
            double c = 0d;
            double d = 0d;

            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return EventCodeEnum.SYSTEM_ERROR;
                        }
                    }
                }
                CatCoordinates tiltPoint = new CatCoordinates();
                CatCoordinates pivotPointCW = new CatCoordinates();
                CatCoordinates pivotPointCCW = new CatCoordinates();
                var axisz0 = GetAxis(EnumAxisConstants.Z0);
                var axisz1 = GetAxis(EnumAxisConstants.Z1);
                var axisz2 = GetAxis(EnumAxisConstants.Z2);
                var axisTheta = GetAxis(EnumAxisConstants.C);
                var zaxis = GetAxis(EnumAxisConstants.Z);

                tiltPoint.X.Value = (150000 * Math.Cos(Math.PI * rpos / 180.0));
                tiltPoint.Y.Value = (150000 * Math.Sin(Math.PI * rpos / 180.0));
                tiltPoint.Z.Value = ttpos;

                //각도로는 -방향
                pivotPointCW.X.Value = (150000 * Math.Cos(Math.PI * (rpos - 90.0) / 180.0));
                pivotPointCW.Y.Value = (150000 * Math.Sin(Math.PI * (rpos - 90.0) / 180.0));
                pivotPointCW.Z.Value = 0d;


                //각도로는 +방향
                pivotPointCCW.X.Value = (150000 * Math.Cos(Math.PI * (rpos + 90.0) / 180.0));
                pivotPointCCW.Y.Value = (150000 * Math.Sin(Math.PI * (rpos + 90.0) / 180.0));
                pivotPointCCW.Z.Value = 0d;


                a = tiltPoint.Y.Value * (pivotPointCW.Z.Value - pivotPointCCW.Z.Value) +
                    pivotPointCW.Y.Value * (pivotPointCCW.Z.Value - tiltPoint.Z.Value) +
                    pivotPointCCW.Y.Value * (tiltPoint.Z.Value - pivotPointCW.Z.Value);

                b = tiltPoint.Z.Value * (pivotPointCW.X.Value - pivotPointCCW.X.Value) +
                    pivotPointCW.Z.Value * (pivotPointCCW.X.Value - tiltPoint.X.Value) +
                    pivotPointCCW.Z.Value * (tiltPoint.X.Value - pivotPointCW.X.Value);

                c = tiltPoint.X.Value * (pivotPointCW.Y.Value - pivotPointCCW.Y.Value) +
                    pivotPointCW.X.Value * (pivotPointCCW.Y.Value - tiltPoint.Y.Value) +
                    pivotPointCCW.X.Value * (tiltPoint.Y.Value - pivotPointCW.Y.Value);

                d = -(tiltPoint.X.Value * ((pivotPointCW.Y.Value * pivotPointCCW.Z.Value) - (pivotPointCCW.Y.Value * pivotPointCW.Z.Value)) -
                     pivotPointCW.X.Value * ((pivotPointCCW.Y.Value * tiltPoint.Z.Value) - (tiltPoint.Y.Value * pivotPointCCW.Z.Value)) -
                     pivotPointCCW.X.Value * ((tiltPoint.Y.Value * pivotPointCW.Z.Value) - (pivotPointCW.Y.Value * tiltPoint.Z.Value)));

                double z0Radius = 120000;
                //double z1Radius = 120000;
                //double z2Radius = 120000;
                //Z0 offset

                double pillar0X = 0d;
                double pillar1X = 0d;
                double pillar2X = 0d;

                double pillar0Y = 0d;
                double pillar1Y = 0d;
                double pillar2Y = 0d;


                pillar0X = z0Radius * Math.Cos(Math.PI * (270 + (axisTheta.Status.Position.Ref / 10000)) / 180);
                pillar0Y = z0Radius * Math.Sin(Math.PI * (270 + (axisTheta.Status.Position.Ref / 10000)) / 180);
                pillar1X = z0Radius * Math.Cos(Math.PI * (150 + (axisTheta.Status.Position.Ref / 10000)) / 180);
                pillar1Y = z0Radius * Math.Sin(Math.PI * (150 + (axisTheta.Status.Position.Ref / 10000)) / 180);
                pillar2X = z0Radius * Math.Cos(Math.PI * (30 + (axisTheta.Status.Position.Ref / 10000)) / 180);
                pillar2Y = z0Radius * Math.Sin(Math.PI * (30 + (axisTheta.Status.Position.Ref / 10000)) / 180);

                double offsetZ0 = 0.0;
                double offsetZ1 = 0.0;
                double offsetZ2 = 0.0;
                // 틀림! j.flow
                //double offsetZ0 = a * (pillar0X + b * (pillar0Y + d)) / c;
                //double offsetZ1 = a * (pillar1X + b * (pillar1Y + d)) / c;
                //double offsetZ2 = a * (pillar2X + b * (pillar2Y + d)) / c;

                // 맞음!
                offsetZ0 = (-a * pillar0X - b * pillar0Y - d) / c;
                offsetZ1 = (-a * pillar1X - b * pillar1Y - d) / c;
                offsetZ2 = (-a * pillar2X - b * pillar2Y - d) / c;

                double compLimit = 350;

                if (offsetZ0 < compLimit)
                {
                    //axisz0.Status.CompValue = offsetZ0;
                    LoggerManager.Debug($"Z0  Comp. Value is {offsetZ0}", isInfo: IsInfo);

                }
                else
                {
                    //axisz0.Status.CompValue = compLimit;
                    offsetZ0 = compLimit;
                    LoggerManager.Debug($"Comp. Value for Z0 is out of tolerence(Tol:{compLimit}). Comp. Value is {offsetZ0}", isInfo: IsInfo);
                }

                if (offsetZ1 < compLimit)
                {
                    //axisz1.Status.CompValue = offsetZ1;
                    LoggerManager.Debug($"Z1  Comp. Value is {offsetZ1}", isInfo: IsInfo);

                }
                else
                {
                    offsetZ1 = compLimit;
                    LoggerManager.Debug($"Comp. Value for Z1 is out of tolerence(Tol:{compLimit}). Comp. Value is {offsetZ1}", isInfo: IsInfo);
                    //axisz1.Status.CompValue = compLimit;
                }

                if (offsetZ2 < compLimit)
                {
                    //axisz2.Status.CompValue = offsetZ2;
                    LoggerManager.Debug($"Z2  Comp. Value is {offsetZ2}", isInfo: IsInfo);

                }
                else
                {
                    offsetZ2 = compLimit;
                    LoggerManager.Debug($"Comp. Value for Z2 is out of tolerence(Tol:{compLimit}). Comp. Value is {offsetZ2}", isInfo: IsInfo);
                    //axisz2.Status.CompValue = compLimit;
                }

                retVal = MotionProvider.ChuckTiltMove(zaxis, offsetZ0, offsetZ1, offsetZ2, zaxis.Status.RawPosition.Ref, zaxis.Param.Speed.Value * 0.5, zaxis.Param.Acceleration.Value * 0.5);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                ret = EventCodeEnum.NONE;

            }
            catch (MotionException ex)
            {
                throw new MotionException("ChuckTiltMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("ChuckTiltMove Error in MotionManager" + ex.Message, ex);
            }

            return ret;
        }
        public EventCodeEnum StageMove(double xpos, double ypos)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }

            int retVal = -1;

            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00})", isInfo:IsInfo);

            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, ypos));

            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;


            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, xaxis.Param.Speed.Value, xaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, yaxis.Param.Speed.Value, yaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, caxis.Param.Speed.Value, caxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                // return EventCodeEnum.NONE;
                //return EventCodeEnum.MOTION_MOVING_ERROR;

                // }
                // foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                // {
                //   retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                //   ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                // }
                // foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                // {
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //  }
                ret = EventCodeEnum.NONE;
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum StageMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            int retVal = -1;
            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00}): Pre-motion check done.", isInfo: IsInfo);
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);



            if (xpos > xaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (xpos < xaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }

            if (ypos > yaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (ypos < yaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;


            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                ret = EventCodeEnum.NONE;
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum StageMove(double xpos, double ypos, double zpos)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            int retVal = -1;
            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00}, Z: {zpos:0.00})", isInfo: IsInfo);
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);


            if (xpos > xaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (xpos < xaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }

            if (ypos > yaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (ypos < yaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (zpos > zaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (zpos < zaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }

            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;

            zaxis.Status.RawPosition.Ref = zpos;
            zaxis.Status.Position.Ref = zpos;

            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, xaxis.Param.Speed.Value, xaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, yaxis.Param.Speed.Value, yaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, caxis.Param.Speed.Value, caxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                // }


                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                ret = EventCodeEnum.NONE;
            }

            catch (MotionException ex)
            {
                throw new MotionException("StageMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum StageMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            int retVal = -1;
            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00}, Z: {zpos:0.00})", isInfo: IsInfo);
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            if (xpos > xaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (xpos < xaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }

            if (ypos > yaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (ypos < yaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (zpos > zaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (zpos < zaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }

            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;

            zaxis.Status.RawPosition.Ref = zpos;
            zaxis.Status.Position.Ref = zpos;

            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                // return EventCodeEnum.NONE;
                //return EventCodeEnum.MOTION_MOVING_ERROR;

                // }
                //if (retVal != 0)
                //{
                //    return EventCodeEnum.MOTION_MOVING_ERROR;
                //}


                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                ret = EventCodeEnum.NONE;
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum StageMove(double xpos, double xvel, double xacc, double ypos, double yvel, double yacc)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            int retVal = -1;
            LoggerManager.Debug($"MotionManger.StageMove({xpos:0.00}, {ypos:0.00})", isInfo: IsInfo);
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            if (xpos > xaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (xpos < xaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }

            if (ypos > yaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (ypos < yaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }

            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;


            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, xvel, xacc);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, yvel, yacc);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, caxis.Param.Speed.Value, caxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));




                // return EventCodeEnum.NONE;
                //return EventCodeEnum.MOTION_MOVING_ERROR;

                // }
                //if (retVal != 0)
                //{
                //    return EventCodeEnum.MOTION_MOVING_ERROR;
                //}

                //double vel = 0;
                //double acc = 0;
                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    if (compAxis.AxisType.Value == EnumAxisConstants.X)
                //    {
                //        vel = xvel;
                //        acc = xacc;
                //    }
                //    else if (compAxis.AxisType.Value == EnumAxisConstants.Y)
                //    {
                //        vel = yvel;
                //        acc = yacc;
                //    }
                //    else
                //    {
                //        vel = compAxis.Param.Speed.Value;
                //        acc = compAxis.Param.Acceleration.Value;
                //    }
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                ret = EventCodeEnum.NONE;
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum StageMove(double xpos, double ypos, double zpos, double cpos)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            int retVal = -1;

            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00}, Z: {zpos:0.00}, C: {cpos:0.00})", isInfo: IsInfo);
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            if (xpos > xaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (xpos < xaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (ypos > yaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (ypos < yaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (zpos > zaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (zpos < zaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (cpos > caxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (cpos < caxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;

            zaxis.Status.RawPosition.Ref = zpos;
            zaxis.Status.Position.Ref = zpos;

            caxis.Status.RawPosition.Ref = cpos;
            caxis.Status.Position.Ref = cpos;

            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, xaxis.Param.Speed.Value, xaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, yaxis.Param.Speed.Value, yaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, caxis.Param.Speed.Value, caxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));



                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                //}
                ret = EventCodeEnum.NONE;

            }

            catch (MotionException ex)
            {
                throw new MotionException("StageMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public EventCodeEnum StageMove(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return EventCodeEnum.SYSTEM_ERROR;
                    }
                }
            }
            int retVal = -1;

            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00}, Z: {zpos:0.00}, C: {cpos:0.00})", isInfo: IsInfo);
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            if (xpos > xaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (xpos < xaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (ypos > yaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (ypos < yaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (zpos > zaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (zpos < zaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            if (cpos > caxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return ret;
            }
            else if (cpos < caxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return ret;
            }
            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;

            zaxis.Status.RawPosition.Ref = zpos;
            zaxis.Status.Position.Ref = zpos;

            caxis.Status.RawPosition.Ref = cpos;
            caxis.Status.Position.Ref = cpos;

            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));



                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);


                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                ret = EventCodeEnum.NONE;

            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMove Error in MotionManager" + ex.Message, ex);
            }
            return ret;
        }
        public int StageRelMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            LoggerManager.Debug($"MotionManger.StageRelMove(x,y)");
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                WaitForAxisMotionDone(axis, 0);
            }
            int retVal = -1;
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);

            double xCpos = 0;
            double yCpos = 0;

            retVal = GetCommandPos(xaxis, ref xCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            retVal = GetCommandPos(yaxis, ref yCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (xCpos + xpos > xaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return (int)ret;
            }
            else if (xCpos + xpos < xaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return (int)ret;
            }
            if (yCpos + ypos > yaxis.Param.PosSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                return (int)ret;
            }
            else if (yCpos + ypos < yaxis.Param.NegSWLimit.Value)
            {
                this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);
                ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                return (int)ret;
            }

            xaxis.Status.RawPosition.Ref = xCpos + xpos;
            xaxis.Status.Position.Ref += xpos;

            yaxis.Status.RawPosition.Ref = yCpos + ypos;
            yaxis.Status.Position.Ref += ypos;


            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                //}

                retVal = 0;
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageRelMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageRelMove Error in MotionManager" + ex.Message, ex);
            }
            return retVal;
        }
        public int StageRelMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            LoggerManager.Debug($"MotionManger.StageRelMove(x,y,z)");
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                WaitForAxisMotionDone(axis, 0);
            }
            int retVal = -1;
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);

            double xCpos = 0;
            double yCpos = 0;
            double zCpos = 0;
            retVal = GetCommandPos(xaxis, ref xCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            retVal = GetCommandPos(yaxis, ref yCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            retVal = GetCommandPos(zaxis, ref zCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xCpos + xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, yCpos + ypos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(zaxis, zCpos + zpos));

            xaxis.Status.RawPosition.Ref = xCpos + xpos;
            xaxis.Status.Position.Ref += xpos;

            yaxis.Status.RawPosition.Ref = yCpos + ypos;
            yaxis.Status.Position.Ref += ypos;

            zaxis.Status.RawPosition.Ref = zCpos + zpos;
            zaxis.Status.Position.Ref += zpos;

            try
            {
                retVal = ErrorManager.CalcErrorComp();
                //ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                //}
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageRelMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageRelMove Error in MotionManager" + ex.Message, ex);
            }
            return retVal;
        }
        public int StageRelMove(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            LoggerManager.Debug($"MotionManger.StageRelMove(x,y,z)");
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                WaitForAxisMotionDone(axis, 0);
            }
            int retVal = -1;
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            double xCpos = 0;
            double yCpos = 0;
            double zCpos = 0;
            double cCpos = 0;
            retVal = GetCommandPos(xaxis, ref xCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            retVal = GetCommandPos(yaxis, ref yCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            retVal = GetCommandPos(zaxis, ref zCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            retVal = GetCommandPos(caxis, ref cCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xCpos + xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, yCpos + ypos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(zaxis, zCpos + zpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(caxis, cCpos + cpos));

            xaxis.Status.RawPosition.Ref = xCpos + xpos;
            xaxis.Status.Position.Ref = xCpos + xpos;

            yaxis.Status.RawPosition.Ref = yCpos + ypos;
            yaxis.Status.Position.Ref = yCpos + ypos;

            zaxis.Status.RawPosition.Ref = zCpos + zpos;
            zaxis.Status.Position.Ref = zCpos + zpos;

            caxis.Status.RawPosition.Ref = cCpos + cpos;
            caxis.Status.Position.Ref = cCpos + cpos;

            try
            {
                retVal = ErrorManager.CalcErrorComp();
                //ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                retVal = MotionProvider.AbsMove(xaxis, xaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(yaxis, yaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(caxis, caxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(xaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(yaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(caxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = MotionProvider.AbsMove(zaxis, zaxis.Status.RawPosition.Ref, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(zaxis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));



                //foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
                //foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
                //{
                //    retVal = WaitForAxisMotionDone(axis, 0);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                //}
            }
            catch (MotionException ex)
            {
                throw new MotionException("StageRelMove Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageRelMove Error in MotionManager" + ex.Message, ex);
            }
            return retVal;
        }
        public int RelMoveAsync(double xpos, double ypos, double xvel, double xacc, double yvel, double yacc)
        {
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            var axisx = GetAxis(EnumAxisConstants.X);
            var axisy = GetAxis(EnumAxisConstants.Y);
            var axisz = GetAxis(EnumAxisConstants.Z);
            var axisc = GetAxis(EnumAxisConstants.C);
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();

            stw.Restart();
            stw.Start();
            int retVal = -1;
            timeStamp.Add(new KeyValuePair<string, long>("Pre-WaitForMotion", stw.ElapsedMilliseconds));
            foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(compAxis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //WaitForAxisMotionDone(axis,0);
            }
            timeStamp.Add(new KeyValuePair<string, long>("WaitForMotion", stw.ElapsedMilliseconds));

            double xCpos = 0;
            double yCpos = 0;
            retVal = GetCommandPos(axisx, ref xCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            retVal = GetCommandPos(axisy, ref yCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(axisx, xCpos + xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(axisy, yCpos + ypos));

            axisx.Status.RawPosition.Ref = xCpos + xpos;
            axisy.Status.RawPosition.Ref = yCpos + ypos;
            axisx.Status.Position.Ref += xpos;
            axisy.Status.Position.Ref += ypos;
            timeStamp.Add(new KeyValuePair<string, long>("Position update", stw.ElapsedMilliseconds));

            try
            {
                retVal = ErrorManager.CalcErrorComp();

                retVal = MotionProvider.AbsMove(axisx, axisx.Status.RawPosition.Ref, xvel, xacc);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(axisy, axisy.Status.RawPosition.Ref, yvel, yacc);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(axisc, axisc.Status.RawPosition.Ref, axisc.Param.Speed.Value, axisc.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(axisx);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(axisy);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(axisc);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = MotionProvider.AbsMove(axisz, axisz.Status.RawPosition.Ref, axisz.Param.Speed.Value, axisz.Param.Acceleration.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = WaitForAxisMotionDone(axisz);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = 0;
            }
            catch (MotionException ex)
            {
                throw new MotionException("RelMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("RelMoveAsync Error in MotionManager" + ex.Message, ex);
            }

            stw.Stop();

            return retVal;
        }
        public int RelMoveAsync(ProbeAxisObject axis, double pos, double vel, double acc)
        {
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            int retVal = 0;

            stw.Restart();
            stw.Start();

            timeStamp.Add(new KeyValuePair<string, long>("Pre-WaitForMotion", stw.ElapsedMilliseconds));
            foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(compAxis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //WaitForAxisMotionDone(axis,0);
            }
            timeStamp.Add(new KeyValuePair<string, long>("WaitForMotion", stw.ElapsedMilliseconds));

            double cpos = 0;
            retVal = GetCommandPos(axis, ref cpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(axis, cpos + pos));

            axis.Status.RawPosition.Ref = cpos + pos;
            axis.Status.Position.Ref += pos;
            timeStamp.Add(new KeyValuePair<string, long>("Position update", stw.ElapsedMilliseconds));

            try
            {
                if (axis.ErrorModule != null)
                {
                    retVal = ErrorManager.CalcErrorComp();
                    timeStamp.Add(new KeyValuePair<string, long>("Calc. Error", stw.ElapsedMilliseconds));
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        if (compAxis == axis)
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        else
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move {0}axis", compAxis.Label), stw.ElapsedMilliseconds));
                    }
                }
                else
                {
                    retVal = MotionProvider.AbsMove(axis, axis.Status.RawPosition.Ref, vel, acc);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("RelMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("RelMoveAsync Error in MotionManager" + ex.Message, ex);
            }

            stw.Stop();

            return retVal;

        }
        public int RelMoveAsync(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();

            stw.Restart();
            stw.Start();
            int retVal = 0;

            timeStamp.Add(new KeyValuePair<string, long>("Pre-WaitForMotion", stw.ElapsedMilliseconds));
            foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(compAxis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //WaitForAxisMotionDone(axis,0);
            }
            timeStamp.Add(new KeyValuePair<string, long>("WaitForMotion", stw.ElapsedMilliseconds));

            double cpos = 0;
            retVal = GetCommandPos(axis, ref cpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(axis, cpos + pos));

            axis.Status.RawPosition.Ref = cpos + pos;
            axis.Status.Position.Ref += pos;
            timeStamp.Add(new KeyValuePair<string, long>("Position update", stw.ElapsedMilliseconds));

            try
            {
                if (axis.ErrorModule != null)
                {
                    retVal = ErrorManager.CalcErrorComp();
                    timeStamp.Add(new KeyValuePair<string, long>("Calc. Error", stw.ElapsedMilliseconds));
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        if (compAxis == axis)
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        else
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move {0}axis", compAxis.Label), stw.ElapsedMilliseconds));
                    }
                }
                else
                {
                    retVal = MotionProvider.AbsMove(axis, axis.Status.RawPosition.Ref, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("RelMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("RelMoveAsync Error in MotionManager" + ex.Message, ex);
            }

            stw.Stop();

            return retVal;
        }
        public int AbsMoveAsync(ProbeAxisObject axis, double abspos, double vel, double acc)
        {
            int retVal = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(compAxis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(axis, abspos));

            axis.Status.RawPosition.Ref = abspos;
            axis.Status.Position.Ref = abspos;

            try
            {
                if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true)
                {
                    retVal = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        //compAxis.Status.AxisBusy = true;
                        if (compAxis == axis)
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, vel, acc);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }
                        else
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, compAxis.Param.Speed.Value, compAxis.Param.Acceleration.Value);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }
                    }

                }
                else
                {
                    axis.Status.AxisBusy = true;
                    retVal = MotionProvider.AbsMove(axis, abspos, vel, acc);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                }


            }
            catch (MotionException ex)
            {
                throw new MotionException("AbsMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("AbsMoveAsync Error in MotionManager" + ex.Message, ex);
            }
            return retVal;
        }
        public int AbsMoveAsync(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
            {
                retVal = WaitForAxisMotionDone(compAxis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(axis, pos));

            axis.Status.RawPosition.Ref = pos;
            axis.Status.Position.Ref = pos;

            try
            {
                if (axis.ErrorModule != null && axis.ErrorModule.CompensationModule.Enable1D == true)
                {
                    retVal = ErrorManager.CalcErrorComp();
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                    foreach (ProbeAxisObject compAxis in axis.ErrorModule.AssociatedAxes)
                    {
                        //compAxis.Status.AxisBusy = true;
                        if (compAxis == axis)
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                        else
                        {
                            retVal = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                        }
                    }

                }
                else
                {
                    axis.Status.AxisBusy = true;
                    retVal = MotionProvider.AbsMove(axis, pos, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }


            }
            catch (MotionException ex)
            {
                throw new MotionException("AbsMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("AbsMoveAsync Error in MotionManager" + ex.Message, ex);
            }
            return retVal;
        }
        public int StageMoveAync(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int returnValue = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00})", isInfo:IsInfo);

            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                returnValue = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            }
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, xpos));

            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;

            try
            {
                returnValue = ErrorManager.CalcErrorComp();
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                {
                    returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMoveAync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMoveAync Error in MotionManager" + ex.Message, ex);
            }
            return returnValue;
        }
        public int StageMoveAync(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int returnValue = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00}, Z: {zpos:0.00})", isInfo: IsInfo);

            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                returnValue = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            }
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);

            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, ypos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(zaxis, zpos));

            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;

            zaxis.Status.RawPosition.Ref = zpos;
            zaxis.Status.Position.Ref = zpos;

            try
            {
                returnValue = ErrorManager.CalcErrorComp();
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                {
                    returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMoveAync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMoveAync Error in MotionManager" + ex.Message, ex);
            }
            return returnValue;
        }
        public int StageMoveAync(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int returnValue = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            LoggerManager.Debug($"MotionManger.StageMove(X: {xpos:0.00}, Y: {ypos:0.00}, Z: {zpos:0.00}, C: {cpos:0.00})", isInfo: IsInfo);

            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                returnValue = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            }
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, ypos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(zaxis, zpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(caxis, cpos));

            xaxis.Status.RawPosition.Ref = xpos;
            xaxis.Status.Position.Ref = xpos;

            yaxis.Status.RawPosition.Ref = ypos;
            yaxis.Status.Position.Ref = ypos;

            zaxis.Status.RawPosition.Ref = zpos;
            zaxis.Status.Position.Ref = zpos;

            caxis.Status.RawPosition.Ref = cpos;
            caxis.Status.Position.Ref = cpos;

            try
            {
                returnValue = ErrorManager.CalcErrorComp();
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                {
                    returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("StageMoveAync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageMoveAync Error in MotionManager" + ex.Message, ex);
            }

            return returnValue;
        }
        public int StageRelMoveAsync(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int returnValue = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            
            LoggerManager.Debug($"MotionManger.StageRelMove(x,y)");

            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                returnValue = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            }

            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);

            double xCpos = 0;
            double yCpos = 0;
            returnValue = GetCommandPos(xaxis, ref xCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            returnValue = GetCommandPos(yaxis, ref yCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xCpos + xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, yCpos + ypos));
            xaxis.Status.RawPosition.Ref = xCpos + xpos;
            xaxis.Status.Position.Ref += xpos;

            yaxis.Status.RawPosition.Ref = yCpos + ypos;
            yaxis.Status.Position.Ref += ypos;

            try
            {
                returnValue = ErrorManager.CalcErrorComp();
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                {
                    returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("StageRelMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageRelMoveAsync Error in MotionManager" + ex.Message, ex);
            }
            return returnValue;
        }
        public int StageRelMoveAsync(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int returnValue = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            
            LoggerManager.Debug($"MotionManger.StageRelMove(x,y,z)");

            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                returnValue = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            }
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);

            double xCpos = 0;
            double yCpos = 0;
            double zCpos = 0;
            returnValue = GetCommandPos(xaxis, ref xCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            returnValue = GetCommandPos(yaxis, ref yCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            returnValue = GetCommandPos(zaxis, ref zCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xCpos + xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, yCpos + ypos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(zaxis, zCpos + zpos));

            xaxis.Status.RawPosition.Ref = xCpos + xpos;
            xaxis.Status.Position.Ref += xpos;

            yaxis.Status.RawPosition.Ref = yCpos + ypos;
            yaxis.Status.Position.Ref += ypos;

            zaxis.Status.RawPosition.Ref = zCpos + zpos;
            zaxis.Status.Position.Ref += zpos;

            try
            {
                returnValue = ErrorManager.CalcErrorComp();
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                {
                    returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("StageRelMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageRelMoveAsync Error in MotionManager" + ex.Message, ex);
            }
            return returnValue;
        }
        public int StageRelMoveAsync(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int returnValue = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            
            LoggerManager.Debug($"MotionManger.StageRelMove(x,y,z)");

            foreach (ProbeAxisObject axis in ErrorManager.AssociatedAxes)
            {
                returnValue = WaitForAxisMotionDone(axis, 0);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            }
            ProbeAxisObject xaxis = GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yaxis = GetAxis(EnumAxisConstants.Y);
            ProbeAxisObject zaxis = GetAxis(EnumAxisConstants.Z);
            ProbeAxisObject caxis = GetAxis(EnumAxisConstants.C);
            double xCpos = 0;
            double yCpos = 0;
            double zCpos = 0;
            double cCpos = 0;
            returnValue = GetCommandPos(xaxis, ref xCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            returnValue = GetCommandPos(yaxis, ref yCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            returnValue = GetCommandPos(zaxis, ref zCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));

            returnValue = GetCommandPos(caxis, ref cCpos);
            ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(xaxis, xCpos + xpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(yaxis, yCpos + ypos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(zaxis, zCpos + zpos));
            ResultValidate(MethodBase.GetCurrentMethod(), CheckSWLimit(caxis, cCpos + cpos));


            xaxis.Status.RawPosition.Ref = xCpos + xpos;
            xaxis.Status.Position.Ref = xCpos + xpos;

            yaxis.Status.RawPosition.Ref = yCpos + ypos;
            yaxis.Status.Position.Ref = yCpos + ypos;

            zaxis.Status.RawPosition.Ref = zCpos + zpos;
            zaxis.Status.Position.Ref = zCpos + zpos;

            caxis.Status.RawPosition.Ref = cCpos + cpos;
            caxis.Status.Position.Ref = cCpos + cpos;

            try
            {
                returnValue = ErrorManager.CalcErrorComp();
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));


                foreach (ProbeAxisObject compAxis in ErrorManager.AssociatedAxes)
                {
                    returnValue = MotionProvider.AbsMove(compAxis, compAxis.Status.RawPosition.Ref, trjtype, ovrd);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(returnValue));
                }

            }
            catch (MotionException ex)
            {
                throw new MotionException("StageRelMoveAsync Error " + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StageRelMoveAsync Error in MotionManager" + ex.Message, ex);
            }
            return returnValue;
        }

        #endregion
        #endregion
        public Task<int> WaitForMotionDoneAsync()
        {
            try
            {
                Stopwatch stw = new Stopwatch();
                List<KeyValuePair<string, long>> timeStamp;
                timeStamp = new List<KeyValuePair<string, long>>();

                stw.Restart();
                stw.Start();

                timeStamp.Add(new KeyValuePair<string, long>("Start", stw.ElapsedMilliseconds));

                return Task.Run(async () =>
                {
                    if (this.MonitoringManager().IsSystemError == true)
                    {
                        if (this.MonitoringManager().IsMachineInitDone == true)
                        {

                        }
                        else
                        {
                            if (this.MonitoringManager().IsMachinInitOn != true)
                            {
                                return -1;
                            }
                        }
                    }
                    int returnValue = -1;

                    CancellationTokenSource source = new CancellationTokenSource();
                    CancellationToken token = source.Token;
                    // Need to add cancelation token firing method

                    try
                    {
                        timeStamp.Add(new KeyValuePair<string, long>("Start", stw.ElapsedMilliseconds));

                        foreach (HomingGroup hominggroup in StageAxes.HomingGroups)
                        {
                            IEnumerable<ProbeAxisObject> axisQuery =
                                StageAxes.ProbeAxisProviders.Where(
                                    axis => axis.AxisType.Value ==
                                    hominggroup.Stage.Value.Find(eax => eax == axis.AxisType.Value));

                            var relArr = await Task.WhenAll(
                                   axisQuery.Select(
                                       axis => WaitMotionDoneAsync(axis, token))
                            );


                            int failedCount = relArr.Count(item => item != 0);
                            if (failedCount > 0)
                            {
                                throw new Exception("Wait failed.");
                            }
                        }
                        timeStamp.Add(new KeyValuePair<string, long>("end", stw.ElapsedMilliseconds));
                        returnValue = 0;
                    }
                    catch (Exception err)
                    {
                        if (err is TaskCanceledException)
                        {
                            source.Cancel();
                            LoggerManager.Error($"WaitForMotionDoneAsync(): Error occurred. Err = {((TaskCanceledException)err).Message}");

                        }

                        //LoggerManager.Error($"WaitForMotionDoneAsync(): Error occurred. Err = {0}", err.GetType().Name));
                        LoggerManager.Exception(err);

                        returnValue = -1;
                        throw;
                    }

                    return returnValue;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int ForcedZDown()
        {
            int retVal = -1;
            if (this.MonitoringManager().IsSystemError == true)
            {
                if (this.MonitoringManager().IsMachineInitDone == true)
                {

                }
                else
                {
                    if (this.MonitoringManager().IsMachinInitOn != true)
                    {
                        return -1;
                    }
                }
            }
            try
            {
                ProbeAxisObject axisZ = GetAxis(EnumAxisConstants.Z);
                retVal = MotionProvider.ForcedZDown(axisZ);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }
            catch (MotionException ex)
            {
                throw new MotionException("ForcedZDown Error", ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("ForcedZDown Error in MotionManager", ex);
            }


            //EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            //try
            //{
            //    AmpFaultClear(EnumAxisConstants.Z);
            //    retVal = MotionProvider.GetAxisState(axisZ, out state);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    if ((EnumAxisState)state != EnumAxisState.StateIDLE)
            //    {
            //        while (flag)
            //        {
            //            retVal = MotionProvider.GetAxisState(axisZ,
            //                out state);
            //            ResultValidate(MethodBase.GetCurrentMethod(), retVal);


            //            retVal = MotionProvider.AlarmReset(axisZ);
            //            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //            retVal = MotionProvider.SetZeroPosition(axisZ);
            //            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //            if ((EnumAxisState)state == EnumAxisState.StateIDLE)
            //            {
            //                flag = false;
            //            }

            //        }
            //    }

            //    retVal = MotionProvider.SetLimitSWNegAct(axisZ,
            //        EnumEventActionType.ActionNONE);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.SetLimitSWPosAct(axisZ,
            //        EnumEventActionType.ActionNONE);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.AlarmReset(axisZ);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.SetZeroPosition(axisZ);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.EnableAxis(axisZ);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);


            //    retVal = MotionProvider.SetSwitchAction(axisZ,
            //          axisZ.Config.InputHome,
            //            EnumEventActionType.ActionSTOP,
            //           axisZ.Param.HomeInvert
            //            == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);


            //    vel = axisZ.Param.HommingSpeed.Value * -1;
            //    rel = VMove(axisZ, vel, EnumTrjType.Homming);
            //    if (rel != EventCodeEnum.NONE)
            //    {

            //    }

            //    retVal = MotionProvider.WaitForAxisEvent(axisZ,
            //        EnumAxisState.StateSTOPPED,
            //       axisZ.Param.HomeDistLimit, 30000);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.ClearUserLimit(axisZ);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.AlarmReset(axisZ);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.SetZeroPosition(axisZ);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);


            //    retVal = MotionProvider.SetLimitSWNegAct(axisZ,
            //        EnumEventActionType.ActionSTOP);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //    retVal = MotionProvider.SetLimitSWPosAct(axisZ,
            //        EnumEventActionType.ActionSTOP);
            //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //}
            //catch (MotionException ex)
            //{
            //    throw new MotionException("ForcedZDown Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("ForcedZDown Error in MotionManager", ex);
            //}

            return retVal;
        }
        /// <summary>
        /// GetSourceLevel을 통하여 값을 계속 체크하여
        /// Resume과 체크한 값이 같으면 Motion이 Resume.
        /// Resume과 체크한 값이 다르면 Motion이 Stop.
        /// </summary>
        /// <param name="axis">             움직일 축                           </param>
        /// <param name="GetSourceLevel">   계속 값을 체크 하기 위한 Delegate    </param>
        /// <param name="resumeLevel">      Resume이 되는 조건                  </param>
        /// <param name="timeout">          timeout 시간                        </param>
        /// <returns>
        /// success     =  0
        /// error       = -1
        /// timeout     = -2
        /// </returns>
        public int WaitForAxisMotionDone(ProbeAxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            int retVal = -1;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return -1;
                        }
                    }
                }
                retVal = MotionProvider.WaitForAxisMotionDone(axis, GetSourceLevel, resumeLevel, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("WaitForAxisMotionDone Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("WaitForAxisMotionDone Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.MonitorForAxisMotion(axis, pos, allowanceRange, maintainTime, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            catch (MotionException ex)
            {
                throw new MotionException("MonitorForAxisMotion Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("MonitorForAxisMotion Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int SetOverride(ProbeAxisObject axis, double ovrd)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.SetOverride(axis, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            catch (MotionException ex)
            {
                throw new MotionException("SetOverride Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetOverride Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public EventCodeEnum NotchFinding(AxisObject axis, EnumMotorDedicatedIn input)
        {

            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            try
            {
                rel = MotionProvider.NotchFinding(axis, input);
                ResultValidate(MethodBase.GetCurrentMethod(), rel);
            }

            catch (MotionException ex)
            {
                throw new MotionException($"NotchFinding Error Axis:{axis.Label.Value}" + ex.Message, ex, ex.ErrorCode, this);
            }
            catch (Exception ex)
            {
                throw new Exception("NotchFinding Error in MotionManager " + ex.Message, ex);
            }

            return rel;
        }
        private void UpdateManagerProc()
        {
            bool bExceptionState = false;
            try
            {
                ProbeAxisObject axisX, axisY, axisZ, axisC, axisR, axisTT, axisPZ, axisCT, axisCCM, axisCCS, axisCCG, axisROT, CX1;

                CompensationPos CPos = new CompensationPos();
                CompensationValue ResultCompValue;

                axisX = GetAxis(EnumAxisConstants.X);
                axisY = GetAxis(EnumAxisConstants.Y);
                axisZ = GetAxis(EnumAxisConstants.Z);
                axisC = GetAxis(EnumAxisConstants.C);
                axisCT = GetAxis(EnumAxisConstants.CT);
                axisCCM = GetAxis(EnumAxisConstants.CCM);
                axisCCS = GetAxis(EnumAxisConstants.CCS);
                axisCCG = GetAxis(EnumAxisConstants.CCG);
                axisR = GetAxis(EnumAxisConstants.R);
                axisTT = GetAxis(EnumAxisConstants.TT);
                axisPZ = GetAxis(EnumAxisConstants.PZ);
                axisROT = GetAxis(EnumAxisConstants.ROT);
                //251030 yb add
                CX1 = GetAxis(EnumAxisConstants.CX1);

                while (bStopUpdateThread == false)
                {
                    try
                    {
                        Thread.Sleep(MonitoringInterValInms);
                        UpdateProcTime = MotionProvider.UpdateProcTime;
                        if (UpdateProcTime > MaxUpdateProcTime)
                        {
                            MaxUpdateProcTime = UpdateProcTime;
                        }
                        //=> loader update
                        // TODO : ??!!
                        if (LoaderAxes.ProbeAxisProviders != null)
                        {
                            foreach (var axisObj in LoaderAxes.ProbeAxisProviders)
                            {
                                axisObj.Status.Position.Actual = axisObj.Status.RawPosition.Actual;
                                axisObj.Status.Position.Command = axisObj.Status.RawPosition.Command;
                                axisObj.Status.Position.Error = axisObj.Status.RawPosition.Error;
                                //axisObj.Status.Position.Ref = axisObj.Status.RawPosition.Ref;
                            }
                        }

                        //=> Stage update.
                        if (axisX != null && axisY != null && axisZ != null && axisC != null)
                        {
                            CPos.XPos = axisX.Status.RawPosition.Actual;
                            CPos.YPos = axisY.Status.RawPosition.Actual;
                            CPos.ZPos = axisZ.Status.RawPosition.Actual;
                            CPos.CPos = axisC.Status.RawPosition.Actual;

                            ResultCompValue = ErrorManager.GetErrorComp(CPos);
                            if (axisX.Status.Position.Actual - 10 > CPos.XPos + ResultCompValue.XValue ||
                                axisX.Status.Position.Actual + 10 < CPos.XPos + ResultCompValue.XValue ||
                                axisY.Status.Position.Actual - 10 > CPos.YPos + ResultCompValue.YValue ||
                                axisY.Status.Position.Actual + 10 < CPos.YPos + ResultCompValue.YValue)
                            {
                                XPosForViewer = CPos.XPos + ResultCompValue.XValue;
                                YPosForViewer = CPos.YPos + ResultCompValue.YValue;
                            }
                            axisX.Status.Position.Comp = ResultCompValue.XValue;
                            axisX.Status.Position.Actual = CPos.XPos + ResultCompValue.XValue;
                            axisX.Status.Position.Command = axisX.Status.RawPosition.Command;
                            //axisX.Status.Position.Ref = axisX.Status.RawPosition.Command;
                            axisX.Status.Position.Error = axisX.Status.RawPosition.Error;

                            axisY.Status.Position.Comp = ResultCompValue.YValue;
                            axisY.Status.Position.Actual = CPos.YPos + ResultCompValue.YValue;
                            axisY.Status.Position.Command = axisY.Status.RawPosition.Command;
                            //axisY.Status.Position.Ref = axisY.Status.RawPosition.Command;
                            axisY.Status.Position.Error = axisY.Status.RawPosition.Error;

                            axisZ.Status.Position.Actual = CPos.ZPos;
                            axisZ.Status.Position.Command = axisZ.Status.RawPosition.Command;
                            //axisZ.Status.Position.Ref = axisZ.Status.RawPosition.Command;
                            axisZ.Status.Position.Error = axisZ.Status.RawPosition.Error;


                            axisC.Status.Position.Comp = ResultCompValue.CValue;
                            axisC.Status.Position.Actual = CPos.CPos + ResultCompValue.CValue;
                            axisC.Status.Position.Command = axisC.Status.RawPosition.Command;
                            //axisC.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisC.Status.Position.Error = axisC.Status.RawPosition.Error;
                        }

                        if (axisR != null && axisTT != null)
                        {
                            axisR.Status.Position.Actual = axisR.Status.RawPosition.Actual;
                            axisR.Status.Position.Command = axisR.Status.RawPosition.Command;
                            //axisR.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisR.Status.Position.Error = axisR.Status.RawPosition.Error;

                            axisTT.Status.Position.Actual = axisTT.Status.RawPosition.Actual;
                            axisTT.Status.Position.Command = axisTT.Status.RawPosition.Command;
                            //axisTT.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisTT.Status.Position.Error = axisTT.Status.RawPosition.Error;

                        }
                        if (axisPZ != null)
                        {
                            axisPZ.Status.Position.Actual = axisPZ.Status.RawPosition.Actual;
                            axisPZ.Status.Position.Command = axisPZ.Status.RawPosition.Command;
                            //axisR.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisPZ.Status.Position.Error = axisPZ.Status.RawPosition.Error;

                        }
                        if (axisROT != null)
                        {
                            axisROT.Status.Position.Actual = axisROT.Status.RawPosition.Actual;
                            axisROT.Status.Position.Command = axisROT.Status.RawPosition.Command;
                            axisROT.Status.Position.Error = axisROT.Status.RawPosition.Error;
                        }
                        if (axisCT != null)
                        {
                            axisCT.Status.Position.Actual = axisCT.Status.RawPosition.Actual;
                            axisCT.Status.Position.Command = axisCT.Status.RawPosition.Command;
                            //axisCT.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisCT.Status.Position.Error = axisCT.Status.RawPosition.Error;
                        }
                        if (axisCCM != null)
                        {
                            axisCCM.Status.Position.Actual = axisCCM.Status.RawPosition.Actual;
                            axisCCM.Status.Position.Command = axisCCM.Status.RawPosition.Command;
                            //axisCT.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisCCM.Status.Position.Error = axisCCM.Status.RawPosition.Error;
                        }
                        if (axisCCS != null)
                        {
                            axisCCS.Status.Position.Actual = axisCCS.Status.RawPosition.Actual;
                            axisCCS.Status.Position.Command = axisCCS.Status.RawPosition.Command;
                            //axisCT.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisCCS.Status.Position.Error = axisCCS.Status.RawPosition.Error;
                        }
                        if (axisCCG != null)
                        {
                            axisCCG.Status.Position.Actual = axisCCG.Status.RawPosition.Actual;
                            axisCCG.Status.Position.Command = axisCCG.Status.RawPosition.Command;
                            //axisCT.Status.Position.Ref = axisC.Status.RawPosition.Command;
                            axisCCG.Status.Position.Error = axisCCG.Status.RawPosition.Error;
                        }
                        if (bExceptionState == true)
                        {
                            bExceptionState = false;
                            _monitoringTimer.Interval = MonitoringInterValInms;
                        }
                        //areUpdateEvent.WaitOne(8);
                        CheckCurrentofThreePod();
                    }
                    catch (Exception err)
                    {
                        if (bExceptionState == false)
                        {
                            LoggerManager.Error($"UpdateManagerProc(): Exception occurred. Err = {err.Message}");
                            bExceptionState = true;
                        }
                        _monitoringTimer.Interval = MonitoringInterValInms * 10;
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"UpdateIOProc(): Error occurred while update io proc. Err = {0} ", err.Message));
                LoggerManager.Exception(err);

            }
        }
        public int Resume(ProbeAxisObject axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.Resume(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("Resume Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("Resume Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int SetFeedrate(ProbeAxisObject axis, double norm, double pause)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.SetFeedrate(axis, norm, pause);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            catch (MotionException ex)
            {
                throw new MotionException("SetFeedrate Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetFeedrate Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int EnableAxis(ProbeAxisObject axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.EnableAxis(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            catch (MotionException ex)
            {
                throw new MotionException("EnableAxis Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("EnableAxis Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int DisableAxes()
        {
            int retVal = -1;
            try
            {
                foreach (ProbeAxisObject axis in LoaderAxes.ProbeAxisProviders)
                {
                    retVal = DisableAxis(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                }
                foreach (ProbeAxisObject axis in StageAxes.ProbeAxisProviders)
                {
                    retVal = DisableAxis(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                }
            }
            catch (MotionException ex)
            {
                throw new MotionException("DisableAxes Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("DisableAxes Error in MotionManager " + ex.Message, ex);
            }
            return retVal;
        }
        public int DisableAxis(ProbeAxisObject axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.DisableAxis(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("DisableAxis Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("DisableAxis Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum CheckSWLimit(EnumAxisConstants axistype, double position)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (position > GetAxis(axistype).Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"CheckSWLimit(): Axis {axistype.ToString()} Positive SW Limit occurred. Limit = { GetAxis(axistype).Param.PosSWLimit.Value}, Target position = {position}");
                    retVal = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                }
                else if (position < GetAxis(axistype).Param.NegSWLimit.Value)
                {
                    retVal = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    LoggerManager.Error($"CheckSWLimit(): Axis {axistype.ToString()} Negative SW Limit occurred. Limit = { GetAxis(axistype).Param.NegSWLimit.Value}, Target position = {position}");
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CheckSWLimit Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int GetCommandPos(EnumAxisConstants axisType, ref double pos)
        {

            int retVal = -1;
            try
            {
                retVal = GetCommandPos(GetAxis(axisType), ref pos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            catch (MotionException ex)
            {
                throw new MotionException("GetCommandPos Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetCommandPos Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public double GetVel(EnumAxisConstants axistype)
        {
            try
            {
                return GetAxis(axistype).Param.Speed.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GetVel Error in MotionManager " + ex.Message, ex);

            }
        }
        public double GetAccel(EnumAxisConstants axistype)
        {
            try
            {
                return GetAxis(axistype).Param.Acceleration.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GetAccel Error in MotionManager " + ex.Message, ex);
            }
        }
        public EventCodeEnum SetDualLoop(bool dualloop)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int retval = -1;
            try
            {
                var axis = GetAxis(EnumAxisConstants.TT);

                retval = MotionProvider.SetDualLoop(axis, dualloop);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retval));

                if (retval == 0)
                {
                    ret = EventCodeEnum.NONE;
                }

            }
            catch (Exception)
            {

                throw;
            }

            return ret;
        }
        public EventCodeEnum SetLoadCellZero()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int retval = -1;
            try
            {
                var axis = GetAxis(EnumAxisConstants.TT);

                retval = MotionProvider.SetLoadCellZero(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retval));

                if (retval == 0)
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception)
            {

                throw;
            }

            return ret;
        }

        public EventCodeEnum DefaultParameterSet(int Index)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Index == 0)
                {
                    this.LoaderAxes = new LoaderAxes();
                    this.LoaderAxes.HomingGroups = new List<HomingGroup>()
                    {
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.U1 }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.U2 }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.A, EnumAxisConstants.W }),
                    };
                }
                else if (Index == 1)
                {
                    this.StageAxes = new StageAxes();
                    this.StageAxes.HomingGroups = new List<HomingGroup>()
                    {
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.Z, EnumAxisConstants.NC }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.Y, EnumAxisConstants.C }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.X }),
                    };
                }
                else
                {
                    RetVal = EventCodeEnum.NODATA;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        public int StartScanPosCapt(AxisObject axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProvider.StartScanPosCapt(axis, MotorDedicatedIn);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            catch (MotionException ex)
            {
                throw new MotionException("StartScanPosCapt Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StartScanPosCapt Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public int StopScanPosCapt(AxisObject axis)
        {

            int retVal = -1;
            try
            {
                retVal = MotionProvider.StopScanPosCapt(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }

            catch (MotionException ex)
            {
                throw new MotionException("StopScanPosCapt Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StopScanPosCapt Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public EventCodeEnum SetMotorStopCommand(EnumAxisConstants axis, string setevent, EnumMotorDedicatedIn input)
        {
            int retVal = -1;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var commandaxis = GetAxis(axis);
                retVal = MotionProvider.SetMotorStopCommand(commandaxis, setevent, input);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                if (retVal == 0)
                {
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.MOTION_SETTING_ERROR;
                }
            }

            catch (MotionException ex)
            {
                throw new MotionException("StopScanPosCapt Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("StopScanPosCapt Error in MotionManager " + ex.Message, ex);
            }

            return ret;
        }
        public bool StageAxesBusy()
        {
            bool retVal = false;
            try
            {
                var axisX = GetAxis(EnumAxisConstants.X);
                var axisY = GetAxis(EnumAxisConstants.Y);
                var axisZ = GetAxis(EnumAxisConstants.Z);
                var axisT = GetAxis(EnumAxisConstants.C);

                retVal = axisX.Status.AxisBusy || axisX.Status.AxisBusy
                    || axisZ.Status.AxisBusy || axisT.Status.AxisBusy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum StageEMGStop()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //int iRet = -1;
            try
            {
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                var axisPZ = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                bool isthreelegup = false;
                //bool isthreelegdn = false;
                List<ProbeAxisObject> stageaxes = new List<ProbeAxisObject>();
                foreach (var axis in StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.State == EnumAxisState.DISABLED ||
                        axis.Status.State == EnumAxisState.ERROR ||
                        axis.Status.State == EnumAxisState.INVALID)
                    {

                    }
                    else
                    {
                        if (axis.Status.IsHomeSeted == true)
                        {
                            try
                            {
                                Stop(axis);
                            }
                            catch (MotionException err)
                            {
                                LoggerManager.Debug($"Motion exception occurred while stopping {axis.Label.Value} axis. Err = {err.Message}");
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Debug($"Exception occurred while stopping {axis.Label.Value} axis. Err = {err.Message}");
                            }
                        }
                        else
                        {
                            //스탑 하지 않음 
                        }
                    }
                }

                ret = IsThreeLegUp(EnumAxisConstants.TRI, ref isthreelegup);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                //삼발이가 올라와 있지 않으면
                if (isthreelegup == false)
                {
                    int rel = -1;

                    if (axisZ.Status.State != EnumAxisState.ERROR && axisZ.Status.State != EnumAxisState.DISABLED)
                    {
                        LoggerManager.Error($"AxisZ before Absmove ");
                        Stop(axisZ);
                        rel = MotionProvider.AbsMove(axisZ, axisZ.Param.ClearedPosition.Value, EnumTrjType.Emergency, 1);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(rel));
                        LoggerManager.Error($"AxisZ after Absmove ");
                        rel = MotionProvider.WaitForAxisMotionDone(axisZ, axisZ.Param.TimeOut.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(rel));
                    }

                    if (axisPZ.Status.State != EnumAxisState.ERROR && axisPZ.Status.State != EnumAxisState.DISABLED)
                    {
                        Stop(axisPZ);
                        LoggerManager.Error($"AxisPZ before Absmove ");
                        rel = MotionProvider.AbsMove(axisPZ, axisPZ.Param.ClearedPosition.Value, EnumTrjType.Emergency, 1);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(rel));
                        LoggerManager.Error($"AxisPZ after Absmove ");
                        rel = MotionProvider.WaitForAxisMotionDone(axisPZ, axisPZ.Param.TimeOut.Value);
                        ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(rel));
                    }
                }
                else
                {
                    // 삼발이 올라와서 전축들 다 죽어야함 finally 에서 죽임 
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                // stage축은 전부 다 죽이기 
                foreach (var item in StageAxes.ProbeAxisProviders)
                {
                    DisableAxis(item);
                    ret = EventCodeEnum.MONITORING_STAGE_EMG_STOP;
                    LoggerManager.Error($"axis {item.AxisType.Value} disabled");
                }
            }

            return ret;
        }
        public EventCodeEnum LoaderEMGStop()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                List<ProbeAxisObject> loaderaxes = new List<ProbeAxisObject>();

                foreach (var item in LoaderAxes.ProbeAxisProviders)
                {
                    loaderaxes.Add(item);
                    Stop(item);
                }

                // 로더축 다 죽이기 
                foreach (var item in LoaderAxes.ProbeAxisProviders)
                {
                    DisableAxis(item);
                    LoggerManager.Error($"axis {item.AxisType.Value} disabled");
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
            }
            return ret;
        }
        public EventCodeEnum IsThreeLegUp(EnumAxisConstants axis, ref bool isthreelegup)
        {
            int retVal = -1;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var triaxis = GetAxis(axis);
                retVal = MotionProvider.IsThreeLegUp(triaxis, ref isthreelegup);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                if (retVal == 0)
                {
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.MOTION_SETTING_ERROR;
                }
            }

            catch (MotionException ex)
            {
                throw new MotionException("IsThreeLegUp Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("IsThreeLegUp Error in MotionManager " + ex.Message, ex);
            }

            return ret;
        }
        public EventCodeEnum IsThreeLegDown(EnumAxisConstants axis, ref bool isthreelegdn)
        {
            int retVal = -1;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var triaxis = GetAxis(axis);
                retVal = MotionProvider.IsThreeLegDown(triaxis, ref isthreelegdn);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                if (retVal == 0)
                {
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.MOTION_SETTING_ERROR;
                }
            }

            catch (MotionException ex)
            {
                throw new MotionException("IsThreeLegUp Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("IsThreeLegUp Error in MotionManager " + ex.Message, ex);
            }

            return ret;
        }
        public EventCodeEnum IsFls(EnumAxisConstants axis, ref bool isfls)
        {
            int retVal = -1;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var curAxis = GetAxis(axis);

                if (curAxis != null)
                {
                    retVal = MotionProvider.IsFls(curAxis, ref isfls);
                }

                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                if (retVal == 0)
                {
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.MOTION_SETTING_ERROR;
                }
            }

            catch (MotionException ex)
            {
                throw new MotionException($"Axis: {axis} " + "IsFls Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception($"Axis: {axis} " + "IsFls Error in MotionManager " + ex.Message, ex);
            }

            return ret;
        }
        public EventCodeEnum IsRls(EnumAxisConstants axis, ref bool isrls)
        {
            int retVal = -1;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var curAxis = GetAxis(axis);
                retVal = MotionProvider.IsRls(curAxis, ref isrls);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                if (retVal == 0)
                {
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.MOTION_SETTING_ERROR;
                }
            }

            catch (MotionException ex)
            {
                throw new MotionException($"Axis: {axis} " + "IsRls Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception($"Axis: {axis} " + "IsRls Error in MotionManager " + ex.Message, ex);
            }

            return ret;
        }
        public int InitHostService()
        {
            int retVal = -1;
            try
            {
                ServiceCallBack = OperationContext.Current.GetCallbackChannel<IMotionManagerCallback>();
                OperationContext.Current.Channel.Faulted += Channel_Faulted;

                foreach (var axis in StageAxes.ProbeAxisProviders)
                {
                    var probAxis = axis;
                    //probAxis.Status.Position.PosUpdated += OnAxisPropertyChanged;
                    probAxis.Status.Position.Updated += probAxis.OnStatusUpdated;
                    //probAxis.OnAxisStatusUpdated += OnAxisPropertyChanged;
                }

                LoggerManager.Debug($"InitHostService(): Hash = {ServiceCallBack.GetHashCode()}");

                retVal = ServiceCallBack.GetHashCode();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name}] InitService() - Failed ENV Host Callback Channel Init (Exception : {err}");
            }
            return retVal;
        }
        public void DeInitService()
        {
            try
            {
                ServiceCallBack = null;
                LoggerManager.Debug($"DeInit MotionManager Channel.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            try
            {
                //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
                if (ServiceCallBack != null)
                {
                    if ((ServiceCallBack as ICommunicationObject).State == CommunicationState.Faulted || (ServiceCallBack as ICommunicationObject).State == CommunicationState.Closed)
                    {
                        DeInitService();
                        LoggerManager.Debug($"Callback Channel faulted. Sender = {sender}");
                    }
                    else
                    {
                        LoggerManager.Debug($"Ignore Callback Channel faulted. Sender = {sender}, Already Reconnected");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsServiceAvailable()
        {
            bool retVal = false;

            if (ServiceCallBack != null)
            {
                retVal = true;
            }
            return retVal;
        }
        public int GetAuxPulse(ProbeAxisObject axis, ref int pos)
        {
            int retVal = -1;
            try
            {
                if (this.MonitoringManager().IsSystemError == true)
                {
                    if (this.MonitoringManager().IsMachineInitDone == true)
                    {

                    }
                    else
                    {
                        if (this.MonitoringManager().IsMachinInitOn != true)
                        {
                            return -1;
                        }
                    }
                }
                retVal = MotionProvider.GetAuxPulse(axis, ref pos);
                ResultValidate(MethodBase.GetCurrentMethod(), EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetAuxPulse Error " + ex.Message, ex, ex.ErrorCode, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetAuxPulse Error in MotionManager " + ex.Message, ex);
            }

            return retVal;
        }
        public EventCodeEnum StageEMGAmpDisable()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_X_BRAKERELEASE, false);
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_Y_BRAKERELEASE, false);
                // stage축은 전부 다 죽이기 
                foreach (var item in StageAxes.ProbeAxisProviders)
                {
                    DisableAxis(item);
                    ret = EventCodeEnum.MONITORING_STAGE_EMG_STOP;
                    LoggerManager.Error($"axis {item.AxisType.Value} disabled");
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"Error occured while StageEMGAmpDisable");
            }
            finally
            {

            }
            return ret;
        }
        public EventCodeEnum StageEMGZDown()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                var axisPZ = this.MotionManager().GetAxis(EnumAxisConstants.PZ);

                // Home포지션으로 강제 다운 
                LoggerManager.Error($"AxisZ before Absmove ");
                AbsMove(axisZ, axisZ.Param.HomeOffset.Value, axisZ.Param.Speed.Value, axisZ.Param.Acceleration.Value);
                LoggerManager.Error($"AxisZ after Absmove ");

                LoggerManager.Error($"AxisPZ before Absmove ");
                AbsMove(axisPZ, axisPZ.Param.HomeOffset.Value, axisPZ.Param.Speed.Value, axisPZ.Param.Acceleration.Value);
                LoggerManager.Error($"AxisPZ after Absmove ");
                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"Error occurred while StageEMGZDown");
            }
            finally
            {
                // stage축은 전부 다 죽이기 
                foreach (var item in StageAxes.ProbeAxisProviders)
                {
                    DisableAxis(item);
                    ret = EventCodeEnum.MONITORING_STAGE_EMG_STOP;
                    LoggerManager.Error($"axis {item.AxisType.Value} disabled");
                }
            }
            return ret;
        }
        public EventCodeEnum StageAxisLock()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                var axisPZ = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                //bool isthreelegup = false;
                //bool isthreelegdn = false;
                bool is_XY_Error = false;
                List<ProbeAxisObject> stageaxes = new List<ProbeAxisObject>();
                foreach (var axis in StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.State == EnumAxisState.DISABLED ||
                        axis.Status.State == EnumAxisState.ERROR ||
                        axis.Status.State == EnumAxisState.INVALID)
                    {
                        if (axis.AxisType.Value == EnumAxisConstants.X || axis.AxisType.Value == EnumAxisConstants.Y)
                        {
                            is_XY_Error = true;
                        }
                    }

                }

                if (is_XY_Error)
                {
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_X_BRAKERELEASE, false);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_Y_BRAKERELEASE, false);

                    this.MotionManager().DisableAxis(this.MotionManager().GetAxis(EnumAxisConstants.X));
                    this.MotionManager().DisableAxis(this.MotionManager().GetAxis(EnumAxisConstants.Y));
                }
                // cleard포지션으로 absmove 

                LoggerManager.Error($"AxisPZ before Absmove ");
                MotionProvider.AbsMove(axisPZ, axisPZ.Param.HomeOffset.Value, axisPZ.Param.Speed.Value, axisPZ.Param.Acceleration.Value);
                LoggerManager.Error($"AxisPZ after Absmove ");
                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"Error occurred while StageEMGZDown");
            }
            finally
            {

            }
            return ret;
        }
        public EventCodeEnum CheckSWLimit(AxisObject axis, double tpos)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (tpos > axis.Param.PosSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR);

                    LoggerManager.Error($"CheckSWLimit(): Axis {axis.GetType()} Positive SW Limit occurred. Limit = {axis.Param.PosSWLimit.Value}, Target position = {tpos}");

                    ret = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    return ret;
                }
                else if (tpos < axis.Param.NegSWLimit.Value)
                {
                    this.NotifyManager().Notify(EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR);

                    LoggerManager.Error($"CheckSWLimit(): Axis {axis.GetType()} Negative SW Limit occurred. Limit = {axis.Param.NegSWLimit.Value}, Target position = {tpos}");

                    ret = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;

                    return ret;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CheckSWLimit(): Error occurred. Err = {err.Message}");
            }

            return EventCodeEnum.NONE;
        }
        /// <summary>
        /// Three Pod이 받는 전류 값의 부하(Torque)를 확인하여 그 사이의 차이 값을 구하고, 한계치를 넘어가는 차이 값을 보이게 될 시 ErrorCode를 넘기는 함수.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum CheckCurrentofThreePod()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                TimeSpan ElapsedTime = DateTime.Now - LastUpdateTimeforThreePodCheck;

                if (ElapsedTime.TotalSeconds > StageAxes.MotionLoggingInterval.Value)
                {
                    LastUpdateTimeforThreePodCheck = DateTime.Now;
                    retVal = CalcZTorque(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        /// <summary>
        /// ZTorque를 가져와서 Tolerance를 넘는지 계산하는 함수
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum CalcZTorque(bool write_log)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                double torqueTol = this.MotionManager().StageAxes.ThreePodTorqueTolerance.Value;
                double Z0_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.Z0);
                double Z1_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.Z1);
                double Z2_torque = this.MotionManager().GetAxisTorque(EnumAxisConstants.Z2);

                bool diff1 = Math.Abs(Z0_torque - Z1_torque) >= torqueTol ? true : false;
                bool diff2 = Math.Abs(Z1_torque - Z2_torque) >= torqueTol ? true : false;
                bool diff3 = Math.Abs(Z0_torque - Z2_torque) >= torqueTol ? true : false;

                double X_Position = GetAxisPos(EnumAxisConstants.X);
                double Y_Position = GetAxisPos(EnumAxisConstants.Y);
                double Z_Position = GetAxisPos(EnumAxisConstants.Z);
                double C_Position = GetAxisPos(EnumAxisConstants.C);
                double PZ_Position = GetAxisPos(EnumAxisConstants.PZ);

                double X_torque = GetAxisTorque(EnumAxisConstants.X);
                double Y_torque = GetAxisTorque(EnumAxisConstants.Y);
                double C_torque = GetAxisTorque(EnumAxisConstants.C);
                double PZ_torque = GetAxisTorque(EnumAxisConstants.PZ);
                double TRI_torque = GetAxisTorque(EnumAxisConstants.TRI);
                double ROT_torque = GetAxisTorque(EnumAxisConstants.ROT);

                if (diff1 || diff2 || diff3)
                {
                    ret = EventCodeEnum.MOTION_THREE_POD_LOAD_UNBALANCE_ERROR;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }

                if (write_log)
                {
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.MonitoringErrLog($"Z Torque Tol : {torqueTol}, Torque X:{X_torque}, Y:{Y_torque}, C:{C_torque}, PZ:{PZ_torque} ,Z0:{Z0_torque}, Z1:{Z1_torque}, Z2:{Z2_torque}, TRI:{TRI_torque}, ROT:{ROT_torque}" +
                                                       $" Position X:{X_Position:00}, Y:{Y_Position:00}, Z:{Z_Position:00}, C:{C_Position:00}, PZ:{PZ_Position:00}");
                    }
                    else
                    {
                        LoggerManager.MonitoringLog($"Z Torque Tol : {torqueTol}, Torque X:{X_torque}, Y:{Y_torque}, C:{C_torque}, PZ:{PZ_torque} ,Z0:{Z0_torque}, Z1:{Z1_torque}, Z2:{Z2_torque}, TRI:{TRI_torque}, ROT:{ROT_torque}" +
                                                    $" Position X:{X_Position:00}, Y:{Y_Position:00}, Z:{Z_Position:00}, C:{C_Position:00}, PZ:{PZ_Position:00}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        /// <summary>
        /// 매개변수로 받은 축의 Torque값을 Return하는 함수.
        /// </summary>
        /// <param name="axisType"></param>
        /// <returns></returns>
        public double GetAxisTorque(EnumAxisConstants axisType)
        {
            double retVal = -1;
            try
            {
                AxisObject axis = GetAxis(axisType);
                if(axis != null)
                {
                    retVal = axis.Status.Torque;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        /// <summary>
        /// 매개변수로 받은 축의 Pos값을 Return하는 함수.
        /// </summary>
        /// <param name="axisType"></param>
        /// <returns></returns>
        public double GetAxisPos(EnumAxisConstants axisType)
        {
            double retVal = -1;
            try
            {
                AxisObject axis = GetAxis(axisType);
                if (axis != null)
                {
                    retVal = axis.Status.Position.Actual;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}

