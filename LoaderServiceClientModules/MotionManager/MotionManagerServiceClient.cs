using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderServiceClientModules
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Autofac;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ServiceClient;
    using LoaderBase.LoaderLog;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;

    public delegate void MotionPosDelegate();
    public class MotionManagerServiceClient : IMotionManagerServiceClient
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        public InitPriorityEnum InitPriority { get; set; }


        ManualResetEvent _pauseEvent = new ManualResetEvent(false);

        public void ThreadPuase()
        {
            _pauseEvent.Reset();
        }
        public void ThreadResume()
        {
            _pauseEvent.Set();
        }
        public MotionPosDelegate motionPosDelegate { get; set; }
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

        private StageAxes _StageAxes = new StageAxes();
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

        public IErrorCompensationManager ErrorManager { get; set; }

        public long UpdateProcTime { get; set; }

        public long MaxUpdateProcTime { get; set; }

        public bool Initialized { get; set; }

        ILoaderCommunicationManager _LoaderCommunicationManager { get; set; }
        ILoaderLogManagerModule _LoaderLogManager { get; set; }

        private IMotionAxisProxy _MotionAxisProxy;
        public IMotionAxisProxy MotionAxisProxy
        {
            get { return _MotionAxisProxy; }
            set
            {
                if (value != _MotionAxisProxy)
                {
                    _MotionAxisProxy = value;
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
        #endregion

        public int Abort(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMove(EnumAxisConstants axis, double pos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMove(ProbeAxisObject axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMove(ProbeAxisObject axis, double pos, double vel, double acc, double dcc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMove(ProbeAxisObject axis, double pos)
        {
            throw new NotImplementedException();
        }

        public int AbsMoveAsync(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int AbsMoveAsync(ProbeAxisObject axis, double abspos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMoveWithSpeedRate(ProbeAxisObject axis, double pos, double vel, double acc, ProbingSpeedRateList FeedRateList)
        {
            throw new NotImplementedException();
        }

        public int AmpFaultClear(EnumAxisConstants axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CheckSWLimit(EnumAxisConstants axistype, double position)
        {
            //throw new NotImplementedException();
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ChuckTiltMove(double rpos, double ttpos)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            return;
        }

        public int DisableAxes()
        {
            throw new NotImplementedException();
        }

        public int DisableAxis(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int EnableAxis(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int ForcedZDown()
        {
            throw new NotImplementedException();
        }

        public double GetAccel(EnumAxisConstants axistype)
        {
            throw new NotImplementedException();
        }

        public int GetActualPos(EnumAxisConstants axisType, ref double pos)
        {
            throw new NotImplementedException();
        }

        public int GetActualPoss(ref double xpos, ref double ypos, ref double zpos, ref double tpos)
        {
            throw new NotImplementedException();
        }

        public int GetAuxPulse(ProbeAxisObject axis, ref int pos)
        {
            throw new NotImplementedException();
        }

        public ProbeAxisObject GetAxis(EnumAxisConstants axis)
        {
            ProbeAxisObject retval = null;

            try
            {
                if (StageAxes.ProbeAxisProviders.Count != 0)
                {
                    retval = StageAxes.ProbeAxisProviders.Single(axes => axes?.AxisType.Value == axis);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public int GetCommandPos(ProbeAxisObject axis, ref double pos)
        {
            throw new NotImplementedException();
        }

        public int GetCommandPos(EnumAxisConstants axisType, ref double pos)
        {
            throw new NotImplementedException();
        }

        public bool GetIOHome(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public IMotionProvider GetMotionProvider()
        {
            throw new NotImplementedException();
        }

        public int GetRefPos(EnumAxisConstants axisType, ref double pos)
        {
            throw new NotImplementedException();
        }

        public int GetRefPos(ref double xpos, ref double ypos, ref double zpos, ref double tpos)
        {
            throw new NotImplementedException();
        }

        public double GetVel(EnumAxisConstants axistype)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Homing(EnumAxisConstants axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Homing(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> HomingAsync(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum HomingTaskRun(params EnumAxisConstants[] axes)
        {
            throw new NotImplementedException();
        }

        public int InitHostService()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        //private int MonitoringInterValInms = 3000;
        private object lockObject = new object();
        //AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        //System.Timers.Timer _monitoringTimer;
        bool bStopUpdateThread;
        Thread UpdateThread;


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                _LoaderLogManager = this.GetLoaderContainer().Resolve<ILoaderLogManagerModule>();

                //_monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
                //_monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                //_monitoringTimer.Start();

                bStopUpdateThread = false;

                UpdateThread = new Thread(new ThreadStart(UpdateManagerProc));
                UpdateThread.Name = this.GetType().Name;
                UpdateThread.Start();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #region //..Update Aexs
        //private void _monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        areUpdateEvent.Set();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
        private void UpdateManagerProc()
        {
            try
            {
                while (bStopUpdateThread == false)
                {
                    _pauseEvent.WaitOne(Timeout.Infinite);

                    if (_MotionAxisProxy != null)
                    {
                        for (int index = 0; index < StageAxes.ProbeAxisProviders.Count; index++)
                        {
                            var getaxis = _MotionAxisProxy.GetAxis(StageAxes.ProbeAxisProviders[index].AxisType.Value);
                            if (getaxis != null)
                            {
                                var stageaxes = _MotionAxisProxy.GetAxis(StageAxes.ProbeAxisProviders[index].AxisType.Value);
                                //StageAxes.ProbeAxisProviders[index] = _MotionAxisProxy.GetAxis(StageAxes.ProbeAxisProviders[index].AxisType.Value);

                                if (_MotionAxisProxy != null)
                                {
                                    if (stageaxes != null)
                                    {
                                        var axis = _MotionAxisProxy?.GetAxis(stageaxes.AxisType.Value);
                                        if (axis != null)
                                        {
                                            if (axis.AxisType.Value == EnumAxisConstants.X || axis.AxisType.Value == EnumAxisConstants.Y)
                                            {
                                                if (StageAxes.ProbeAxisProviders[index].Status.RawPosition.Ref - 10 > axis.Status.RawPosition.Ref ||
                                                    StageAxes.ProbeAxisProviders[index].Status.RawPosition.Ref + 10 < axis.Status.RawPosition.Ref)
                                                {
                                                    if (axis.AxisType.Value == EnumAxisConstants.X)
                                                    {
                                                        XPosForViewer = axis.Status.RawPosition.Ref;
                                                    }
                                                    else
                                                    {
                                                        YPosForViewer = axis.Status.RawPosition.Ref;
                                                    }
                                                }
                                            }
                                            StageAxes.ProbeAxisProviders[index].Status.Position.Ref = axis.Status.Position.Ref;
                                            StageAxes.ProbeAxisProviders[index].Status.Position.Actual = axis.Status.Position.Actual;

                                            StageAxes.ProbeAxisProviders[index].Status.RawPosition.Ref = axis.Status.RawPosition.Ref;
                                            StageAxes.ProbeAxisProviders[index].Status.RawPosition.Actual = axis.Status.RawPosition.Actual;

                                            StageAxes.ProbeAxisProviders[index].Status.Pulse.Command = axis.Status.Pulse.Command;
                                            StageAxes.ProbeAxisProviders[index].Status.Pulse.Actual = axis.Status.Pulse.Actual;

                                            // TODO : MotionProxy 사용과 같이 살펴봐야 됨.
                                            //if (motionPosDelegate != null)
                                            //{
                                            //    motionPosDelegate();
                                            //}
                                        }
                                    }
                                }
                            }
                            Thread.Sleep(33);
                        }
                    }
                    Thread.Sleep(500);
                    //areUpdateEvent.WaitOne(3000);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        public bool IsEmulMode(ProbeAxisObject axis)
        {
            return true;
        }

        public bool IsServiceAvailable()
        {
            return false;
        }

        public EventCodeEnum IsThreeLegDown(EnumAxisConstants axis, ref bool isthreelegdn)
        {
            var ret = _LoaderCommunicationManager.GetProxy<IMotionAxisProxy>()?.IsThreeLegDown(axis, ref isthreelegdn)??EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            return ret;
        }

        public EventCodeEnum IsThreeLegUp(EnumAxisConstants axis, ref bool isthreelegup)
        {
            var ret = _LoaderCommunicationManager.GetProxy<IMotionAxisProxy>()?.IsThreeLegUp(axis, ref isthreelegup) ?? EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            return ret;
        }

        public EventCodeEnum IsRls(EnumAxisConstants axis, ref bool isrls)
        {
            var ret = _LoaderCommunicationManager.GetProxy<IMotionAxisProxy>()?.IsRls(axis, ref isrls) ?? EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            return ret;
        }

        public EventCodeEnum IsFls(EnumAxisConstants axis, ref bool isfls)
        {
            var ret = _LoaderCommunicationManager.GetProxy<IMotionAxisProxy>()?.IsFls(axis, ref isfls) ?? EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            return ret;
        }

        public EventCodeEnum LoaderEMGStop()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoaderSystemInit()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum NotchFinding(AxisObject axis, EnumMotorDedicatedIn input)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double pos)
        {
            var ret = _LoaderCommunicationManager.GetProxy<IMotionAxisProxy>()?.RelMove(axis, pos) ?? EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            return ret;
        }

        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos, double vel, double acc, double dcc)
        {
            throw new NotImplementedException();
        }

        public int RelMoveAsync(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int RelMoveAsync(ProbeAxisObject axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public int RelMoveAsync(double xpos, double ypos, double xvel, double xacc, double yvel, double yacc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMoveZAxis(double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public int Resume(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveSysParameter()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ScanJogMove(double xVel, double yVel, EnumTrjType trjtype)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetDualLoop(bool dualloop)
        {
            throw new NotImplementedException();
        }

        public int SetFeedrate(ProbeAxisObject axis, double normfeedrate, double pausefeedrate)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetLoadCellZero()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetMotorStopCommand(EnumAxisConstants axis, string setevent, EnumMotorDedicatedIn input)
        {
            throw new NotImplementedException();
        }

        public int SetOverride(ProbeAxisObject axis, double ovrd)
        {
            throw new NotImplementedException();
        }

        public int SetPosition(ProbeAxisObject probeAxisObject, double pos)
        {
            throw new NotImplementedException();
        }

        public int SetSettlingTime(ProbeAxisObject axis, double settlingTime)
        {
            throw new NotImplementedException();
        }

        public bool StageAxesBusy()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageEMGStop()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMove(double xpos, double ypos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMove(double xpos, double ypos, double zpos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMove(double xpos, double ypos, double zpos, double cpos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMove(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMove(double xpos, double xvel, double xacc, double ypos, double yvel, double yacc)
        {
            throw new NotImplementedException();
        }

        public int StageMoveAync(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageMoveAync(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageMoveAync(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageRelMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageRelMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageRelMove(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageRelMoveAsync(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageRelMoveAsync(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int StageRelMoveAsync(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageSystemInit()
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> StageSystemInitAsync()
        {
            throw new NotImplementedException();
        }

        public int StartScanPosCapt(AxisObject axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            throw new NotImplementedException();
        }

        public int Stop(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int StopScanPosCapt(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum TiltingMove(double tz1pos, double tz2pos, double tz3pos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum VMove(EnumAxisConstants axis, double vel, EnumTrjType trjtype)
        {
            var ret = _LoaderCommunicationManager.GetProxy<IMotionAxisProxy>()?.VMove(axis, vel, trjtype) ?? EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            return ret;
        }

        public EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, long timeout = 0)
        {
            var ret = _LoaderCommunicationManager.GetProxy<IMotionAxisProxy>()?.WaitForAxisMotionDone(axis, timeout) ?? EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            return ret;
        }

        public int WaitForAxisMotionDone(ProbeAxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public Task<int> WaitForMotionDoneAsync()
        {
            throw new NotImplementedException();
        }

        public void DeInitService()
        {
            return;
        }

        public EventCodeEnum StageEMGZDown()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum StageAxisLock()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum StageEMGAmpDisable()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum VMove(ProbeAxisObject axis, double vel, EnumTrjType trjtype)
        {
            throw new NotImplementedException();
        }

        public int WaitForAxisMotionDone(ProbeAxisObject axis, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public int WaitForAxisVMotionDone(ProbeAxisObject axis, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CheckCurrentofThreePod()
        {
            throw new NotImplementedException();
        }

        public double GetAxisTorque(EnumAxisConstants axisType)
        {
            throw new NotImplementedException();
        }

        public double GetAxisPos(EnumAxisConstants axisType)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CalcZTorque(bool writelog)
        {
            throw new NotImplementedException();
        }
    }
}
