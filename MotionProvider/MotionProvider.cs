using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Threading;
using ProberInterfaces;
using EmulMotionModule;
using System.ComponentModel;
using ProberErrorCode;
using SystemExceptions.MotionException;
using System.Reflection;

using LogModule;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Motion
{
    public enum MotionType
    {
        MPI,
        SCL,
        Elmo,
        Emul,
        Enet,
        TwinCat,
        ADSRemote,
        UNDEFINED
    }

    [Serializable()]
    public class MotionProvider : IMotionProvider
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static int MonitoringInterValInms = 100;

        bool bStopUpdateThread;
        Thread UpdateThread;

        private MotionBaseConfigurator _MotionConfigurators;
        public MotionBaseConfigurator MotionConfigurators
        {
            get { return _MotionConfigurators; }
            set { _MotionConfigurators = value; }
        }

        private Axes _AxisProviders;

        public Axes AxisProviders
        {
            get { return _AxisProviders; }
            set
            {
                _AxisProviders = value;
            }
        }
        private long _UpdateProcTime;

        public long UpdateProcTime
        {
            get { return _UpdateProcTime; }
            set { _UpdateProcTime = value; }
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new MotionBaseConfigurator();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(MotionBaseConfigurator));

                if (RetVal == EventCodeEnum.NONE)
                {
                    MotionConfigurators = tmpParam as MotionBaseConfigurator;
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
                RetVal = this.SaveParameter(MotionConfigurators);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveAutoTiltSysFile] Faile SaveParameter");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        private ObservableCollection<IMotionBase> _MotionProviders;
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<IMotionBase> MotionProviders
        {
            get { return _MotionProviders; }
            set { _MotionProviders = value; }
        }
        public MotionProvider()
        {
            try
            {
                MotionProviders = new ObservableCollection<IMotionBase>();
                MotionConfigurators = new MotionBaseConfigurator();
                AxisProviders = new Axes();
                //  InitMotionProvider();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum InitMotionProvider(ObservableCollection<AxisObject> axes)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                int state = -1;
                double posError = 0;
                AxisProviders.AxisObjects = axes;

                foreach (MotionBaseDescriptor motionDesc in MotionConfigurators.MotionBaseDescriptors)
                {
                    if (motionDesc.Type.Value == MotionType.Emul)
                    {
                        MotionProviders.Add(new EmulMotionBase(motionDesc.CtrlNum.Value, motionDesc.LimitNum.Value));
                    }
                    else if (motionDesc.Type.Value == MotionType.Elmo)
                    {
                        this.PMASManager().InitModule();
                        MotionProviders.Add(new ElmoMotionBase(this.PMASManager().ConnHndl));
                    }
                    else if (motionDesc.Type.Value == MotionType.TwinCat)
                    {
                        MotionProviders.Add(new TcAdsMotionBase(motionDesc.CtrlNum.Value));
                    }
                    else if (motionDesc.Type.Value == MotionType.ADSRemote)
                    {
                        MotionProviders.Add(new ADSRemoteMotion(motionDesc.CtrlNum.Value, motionDesc.LimitNum.Value));
                    }
                }

                foreach (AxisObject axis in AxisProviders.AxisObjects)
                {
                    double cmdPos = 0;
                    axis.Status = new AxisStatus();
                    axis.Status.Position = new Positions();
                    axis.Status.RawPosition = new Positions();
                    axis.Status.Torque = 0.0;
                    GetCommandPosition(axis, ref cmdPos);
                    axis.Status.RawPosition.Ref = cmdPos;
                    axis.Status.Position.Ref = cmdPos;

                    if (MotionConfigurators.MotionBaseDescriptors[axis.PortNum.Value].Type.Value == MotionType.MPI)
                    {
                        ClearUserLimit(axis);
                        Abort(axis);
                        SetPosition(axis, 0);
                        AmpFaultClear(axis);
                    }

                    Thread.Sleep(100);
                    GetAxisState(axis, ref state);
                    GetPosError(axis, ref posError);

                    if ((EnumAxisState)state != EnumAxisState.IDLE | posError > axis.Config.Inposition.Value)
                    {
                        AmpFaultClear(axis);
                        SetZeroPosition(axis);
                        AmpFaultClear(axis);
                        Thread.Sleep(100);
                    }

                    ApplyAxisConfig(axis);
                }

                bStopUpdateThread = false;
                UpdateThread = new Thread(new ThreadStart(UpdateMotionProc));
                UpdateThread.Name = this.GetType().Name;

                UpdateThread.Start();

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.MOTION_INIT_ERROR;
                LoggerManager.Error($"InitMotionProvider(): Error occurred while init. Err = {err.Message}");
            }

            return RetVal;
        }
        public EventCodeEnum LoadMotionBaseDescriptor(string MotionBaseParamFilePath)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(MotionBaseConfigurator), null, MotionBaseParamFilePath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    _MotionConfigurators = tmpParam as MotionBaseConfigurator;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("LoadMotionParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }
        public int InitMotionProvider(List<AxisObject> axes)
        {
            int retVal = -1;
            try
            {

                //foreach (MPIMotionBase mpiMP in MotionProviders)
                //{
                //mpiMP.InitMotionProvider(mpiAxes);
                //}


                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public bool IsMotionReady
        {
            get
            {
                bool ready = true;
                foreach (var mp in MotionProviders)
                {
                    ready = ready & mp.IsMotionReady;
                }

                return ready;
            }
        }
        public int Address
        {
            get
            {
                return 0;
            }
        }
        public int ResultValidate(object funcname, int retcode)
        {
            try
            {
                if ((EnumMotionBaseReturnCode)retcode != EnumMotionBaseReturnCode.ReturnCodeOK)
                {
                    throw new MotionException($"Funcname: {funcname.ToString()} ReturnValue: {retcode.ToString()} Error occurred", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retcode));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retcode;
        }
        public int ChuckTiltMove(AxisObject axis, double offsetz0, double offsetz1, double offsetz2, double abspos, double vel, double acc)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].ChuckTiltMove(axis, offsetz0, offsetz1, offsetz2, abspos, vel, acc);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }
            catch (MotionException ex)
            {
                throw new MotionException("ChuckTiltMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("ChuckTiltMove Error in MotionProvider", ex);
            }

            return retVal;

        }
        public int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].AbsMove(axis, abspos, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }
            catch (MotionException ex)
            {
                throw new MotionException("AbsMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("AbsMove Error in MotionProvider", ex);
            }

            return retVal;

        }
        public int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double velovrd = 1, double accovrd = 1, double dccovrd = 1)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;
            double vel = 0;
            retVal = GetVelocity(axis, trjtype, ref vel, velovrd);
            ResultValidate(retVal);

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].AbsMove(axis, abspos,
               vel,
               GetAccel(axis, trjtype, accovrd),
               GetDeccel(axis, trjtype, dccovrd));

                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("AbsMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("AbsMove Error in MotionProvider", ex);
            }

            return retVal;

        }
        public int AbsMove(AxisObject axis, double abspos, double vel, double acc)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].AbsMove(axis, abspos, vel, acc);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("AbsMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("AbsMove Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int AbsMove(AxisObject axis, double abspos, double vel, double acc, double dcc)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].AbsMove(axis, abspos, vel, acc, dcc);

                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("AbsMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("AbsMove Error in MotionProvider", ex);
            }

            return retVal;


        }
        public int AlarmReset(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].AlarmReset(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("AlarmReset Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("AlarmReset Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int DeInitMotionService()
        {
            try
            {
                LoggerManager.Debug($"DeInitMotionService() in {this.GetType().Name}");

                bStopUpdateThread = true;
                //if (UpdateThread != null) UpdateThread.Join();
                UpdateThread?.Join();

                foreach (var mp in MotionProviders)
                {
                    mp.DeInitMotionService();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return (int)EnumMotionBaseReturnCode.ReturnCodeOK;
        }
        public int DisableAxis(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].DisableAxis(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("DisableAxis Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("DisableAxis Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int EnableAxis(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].EnableAxis(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("EnableAxis Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("EnableAxis Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int ApplyAxisConfig(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].ApplyAxisConfig(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("ApplyAxisConfig Error", ex, EventCodeEnum.MOTION_APPLYAXISCONFIG_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("ApplyAxisConfig Error in MotionProvider", ex);
            }

            return retVal;
        }
        public double GetAccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            double value = -1;
            try
            {
                value = MotionProviders[axis.PortNum.Value].GetAccel(axis, trjtype, ovrd);
            }
            catch (Exception ex)
            {
                throw new Exception("GetAccel Error in MotionProvider", ex);
            }

            return value;
        }
        public int GetActualPosition(AxisObject axis, ref double pos)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetActualPosition(axis, ref pos);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetActualPosition Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetActualPosition Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetActualPulse(AxisObject axis, ref int pos)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetActualPulse(axis, ref pos);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetActualPulse Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetActualPulse Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetAlarmCode(AxisObject axis, ref ushort alarmcode)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetAlarmCode(axis, ref alarmcode);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetAlarmCode Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetAlarmCode Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetAxisInputs(AxisObject axis, ref uint instatus)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetAxisInputs(axis, ref instatus);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetAxisInputs Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetAxisInputs Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetAxisState(AxisObject axis, ref int state)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetAxisState(axis, ref state);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetAxisState Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetAxisState Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetAxisStatus(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetAxisStatus(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetAxisStatus Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetAxisStatus Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetCaptureStatus(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetCaptureStatus(axis.AxisIndex.Value, axis.Status);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetCaptureStatus Error", ex, EventCodeEnum.MOTION_GETCAPTURESTATUS_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetCaptureStatus Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetCmdPosition(AxisObject axis, ref double cmdpos)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetCmdPosition(axis, ref cmdpos);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetCmdPosition Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetCmdPosition Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetCommandPosition(AxisObject axis, ref double pos)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetCommandPosition(axis, ref pos);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetCommandPosition Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetCommandPosition Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetCommandPulse(AxisObject axis, ref int pos)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetCommandPulse(axis, ref pos);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("GetCommandPulse Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetCommandPulse Error in MotionProvider", ex);
            }

            return retVal;
        }
        public double GetDeccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            double value = -1;
            try
            {
                value = MotionProviders[axis.PortNum.Value].GetDeccel(axis, trjtype, ovrd);
            }
            catch (MotionException ex)
            {
                throw new MotionException("GetCommandPulse Error", ex, EventCodeEnum.MOTION_FATAL_ERROR, -1, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetDeccel Error in MotionProvider", ex);
            }

            return value;
        }
        public bool GetIOAmpFault(AxisObject axis)
        {
            bool value = false;
            try
            {
                value = MotionProviders[axis.PortNum.Value].GetIOAmpFault(axis);
            }
            catch (Exception ex)
            {
                throw new Exception("GetIOAmpFault Error in MotionProvider", ex);
            }
            return value;
        }
        public bool GetIOHome(AxisObject axis)
        {
            bool value = false;
            try
            {
                value = MotionProviders[axis.PortNum.Value].GetIOHome(axis);
            }
            catch (Exception ex)
            {
                throw new Exception("GetIOHome Error in MotionProvider", ex);
            }
            return value;
        }
        public bool GetIONegLim(AxisObject axis)
        {
            bool value = false;
            try
            {
                value = MotionProviders[axis.PortNum.Value].GetIONegLim(axis);
            }
            catch (Exception ex)
            {
                throw new Exception("GetIONegLim Error in MotionProvider", ex);
            }

            return value;
        }
        public bool GetIOPosLim(AxisObject axis)
        {

            bool value = false;
            try
            {
                value = MotionProviders[axis.PortNum.Value].GetIOPosLim(axis);
            }
            catch (Exception ex)
            {
                throw new Exception("GetIOPosLim Error in MotionProvider", ex);
            }

            return value;
        }
        public int GetPosError(AxisObject axis, ref double poserr)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetPosError(axis, ref poserr);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }
            catch (MotionException ex)
            {
                throw new MotionException("GetPosError Error", ex, EventCodeEnum.MOTION_GETPOSITION_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetPosError Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetVelocity(AxisObject axis, EnumTrjType trjtype, ref double vel, double ovrd = 1)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetVelocity(axis, trjtype, ref vel, ovrd);
                ResultValidate(retVal);
            }
            catch (Exception ex)
            {
                throw new Exception("GetVelocity Error in MotionProvider", ex);
            }
            return retVal;
        }
        public int Halt(AxisObject axis, double value)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].Halt(axis, value);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }
            catch (MotionException ex)
            {
                throw new MotionException("Halt Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("Halt Error in MotionProvider", ex);
            }

            return retVal;
        }
        public bool HasAlarm(AxisObject axis)
        {

            bool value = false;
            try
            {
                value = MotionProviders[axis.PortNum.Value].HasAlarm(axis);
            }
            catch (Exception ex)
            {
                throw new Exception("HasAlarm Error in MotionProvider", ex);
            }

            return value;
        }
        public int Homming(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                // return MotionProviders[axis.PortNum.Value].Homming(axis);
                if (MotionProviders[axis.PortNum.Value] is ElmoMotionBase)
                {
                    retVal = MotionProviders[axis.PortNum.Value].Homming(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                    return retVal;
                }
                else if (MotionProviders[axis.PortNum.Value] is EmulMotionBase)
                {
                    retVal = MotionProviders[axis.PortNum.Value].Homming(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                    return retVal;
                }
                //else if (MotionProviders[axis.PortNum.Value] is MPIMotionBase)
                //{
                //    switch (axis.HomingType.Value)
                //    {
                //        case HomingMethodType.NHPI:
                //            axis.HomingMethod = new NHPI();
                //            break;
                //        case HomingMethodType.PHNI:
                //            axis.HomingMethod = new PHNI();
                //            break;
                //        case HomingMethodType.NH:
                //            axis.HomingMethod = new NH();
                //            break;
                //        case HomingMethodType.VH:
                //            axis.HomingMethod = new VH();
                //            break;
                //        default:
                //            break;
                //    }

                //    retVal = axis.HomingMethod.Homing(MotionProviders[axis.PortNum.Value], axis);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                //    return retVal;
                //}
                //else if (MotionProviders[axis.PortNum.Value] is EnetMotionBase)
                //{
                //    retVal = MotionProviders[axis.PortNum.Value].Homming(axis);
                //    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                //    return retVal;
                //}
                else if (MotionProviders[axis.PortNum.Value] is TcAdsMotionBase)
                {
                    retVal = MotionProviders[axis.PortNum.Value].Homming(axis);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                    return retVal;
                }
            }
            catch (MotionException ex)
            {
                throw new MotionException("Homming Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("Homming Error in MotionProvider", ex);
            }
            return axis.HomingMethod.Homing(MotionProviders[axis.PortNum.Value], axis);

        }
        public int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].Homming(axis, reverse, (int)input, homeoffset);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("Homming Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("Homming Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int Homming(AxisObject axis, bool reverse, EnumMCInputs input, double homeoffset)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].Homming(axis, reverse, input, homeoffset);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("Homming Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("Homming Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int IsInposition(AxisObject axis, ref bool val)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].IsInposition(axis, ref val);
            }
            catch (Exception ex)
            {
                throw new Exception("IsInposition Error in MotionProvider", ex);
            }


            return retVal;
        }
        public int IsMotorEnabled(AxisObject axis, ref bool val)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].IsMotorEnabled(axis, ref val);
            }
            catch (Exception ex)
            {
                throw new Exception("IsMotorEnabled Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int IsMoving(AxisObject axis, ref bool val)
        {
            int retval = -1;
            try
            {
                retval = MotionProviders[axis.PortNum.Value].IsMoving(axis, ref val);
            }
            catch (Exception ex)
            {
                throw new Exception("IsMoving Error in MotionProvider", ex);
            }

            return retval;
        }
        public int JogMove(AxisObject axis, double velocity, double accel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].JogMove(axis, velocity, accel, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("JogMove Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("JogMove Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int LinInterpolation(AxisObject[] axes, double[] poss)
        {
            int retVal = -1;

            try
            {
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("LinInterpolation Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("LinInterpolation Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int Pause(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].Pause(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("Pause Error", ex, EventCodeEnum.MOTION_PAUSE_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("Pause Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].RelMove(axis, pos, trjtype, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("RelMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("RelMove Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double velovrd = 1, double accovrd = 1, double dccovrd = 1)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;
            double vel = 0;
            retVal = GetVelocity(axis, trjtype, ref vel, velovrd);
            ResultValidate(retVal);
            try
            {
                retVal = GetVelocity(axis, trjtype, ref vel, velovrd);
                ResultValidate(retVal);

                retVal = MotionProviders[axis.PortNum.Value].AbsMove(axis, pos,
                vel,
                GetAccel(axis, trjtype, accovrd),
                GetDeccel(axis, trjtype, dccovrd));
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }
            catch (MotionException ex)
            {
                throw new MotionException("RelMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("RelMove Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int RelMove(AxisObject axis, double pos, double vel, double acc)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].RelMove(axis, pos, vel, acc);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }
            catch (MotionException ex)
            {
                throw new MotionException("RelMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("RelMove Error in MotionProvider", ex);
            }
            finally
            {

            }


            return retVal;
        }
        public int RelMove(AxisObject axis, double pos, double vel, double acc, double dcc)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].RelMove(axis, pos, vel, acc, dcc);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("RelMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("RelMove Error in MotionProvider", ex);
            }
            finally
            {
                retVal = -3;
            }


            return retVal;
        }
        public int VMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            if (axis.IsLock())
                return 0;

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].VMove(axis, velocity, trjtype);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("VMove Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("VMove Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetLimitSWPosAct(AxisObject axis, EnumEventActionType action)
        {

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetLimitSWPosAct(axis, action);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("SetLimitSWPosAct Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetLimitSWPosAct Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetLimitSWNegAct(AxisObject axis, EnumEventActionType action)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetLimitSWNegAct(axis, action);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetLimitSWNegAct Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetLimitSWNegAct Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int ResultValidate(int retcode)
        {
            if ((EnumMotionBaseReturnCode)retcode != EnumMotionBaseReturnCode.ReturnCodeOK)
            {
                LoggerManager.Error($"ResultValidate(): Err code = {retcode}, Description = {((EnumMotionBaseReturnCode)retcode).ToString()}");
                throw new MotionException($"ResultValidate(): Err code = {retcode}, Description = {((EnumMotionBaseReturnCode)retcode).ToString()}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retcode));
            }
            return retcode;
        }
        public int Resume(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].Resume(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("Resume Error", ex, EventCodeEnum.MOTION_RESUME_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("Resume Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetFeedrate(AxisObject axis, double normfeedrate, double pausefeedrate)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetFeedrate(axis, normfeedrate, pausefeedrate);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("SetFeedrate Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetFeedrate Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetSWNegLimAction(axis, action, polarity); //check
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetNegLimAction Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetNegLimAction Error in MotionProvider", ex);
            }

            return retVal;
        }

        public int SetOverride(AxisObject axis, double ovrd)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetOverride(axis, ovrd);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetOverride Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetOverride Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetPosition(AxisObject axis, double pos)
        {
            int retVal = -1;

            try
            {
                axis.Status.RawPosition.Ref = pos;
                axis.Status.Position.Ref = pos;
                retVal = MotionProviders[axis.PortNum.Value].SetPosition(axis, pos);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetPosition Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetPosition Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetSWPosLimAction(axis, action, polarity); //Check
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetPosLimAction Error", ex, EventCodeEnum.MOTION_SETPOSITION_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("SetPosLimAction Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetPulse(AxisObject axis, int pos)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetPulse(axis, pos);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetPulse Error", ex, EventCodeEnum.MOTION_SETPULSE_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetPulse Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetSwitchAction(AxisObject axis, EnumDedicateInputs input, EnumEventActionType action, EnumInputLevel reverse)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetSwitchAction(axis, input, action, reverse);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("AbsMove Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("GetAccel Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int VMoveStop(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].VMoveStop(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("VMoveStop Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("VMoveStop Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int Stop(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].Stop(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("Stop Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("Stop Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int WaitForAxisEvent(AxisObject axis, EnumAxisState waitfor, double distlimit, long timeout = -1)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].WaitForAxisEvent(axis, waitfor, distlimit, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("WaitForAxisEvent Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("WaitForAxisEvent Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int WaitForVMoveAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            return MotionProviders[axis.PortNum.Value].WaitForVMoveAxisMotionDone(axis, timeout);
        }
        public int WaitForAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            return MotionProviders[axis.PortNum.Value].WaitForAxisMotionDone(axis, timeout);
        }
        public int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].WaitForAxisMotionDone(axis, GetSourceLevel, resumeLevel, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("WaitForAxisMotionDone Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("WaitForAxisMotionDone Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0)
        {

            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].MonitorForAxisMotion(axis, pos, allowanceRange, maintainTime, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("MonitorForAxisMotion Error", ex, EventCodeEnum.MOTION_MONITOR_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("MonitorForAxisMotion Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int ClearUserLimit(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].ClearUserLimit(axis.AxisIndex.Value);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("ClearUserLimit Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("ClearUserLimit Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetZeroPosition(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetZeroPosition(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetZeroPosition Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetZeroPosition Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int AmpFaultClear(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].AmpFaultClear(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("AmpFaultClear Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("AmpFaultClear Error in MotionProvider", ex);
            }

            return retVal;

        }
        public int SetSettlingTime(AxisObject axis, double settlingTime)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetSettlingTime(axis, settlingTime);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetSettlingTime Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetSettlingTime Error in MotionProvider", ex);
            }

            return retVal;

        }
        public int Abort(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].Abort(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("Abort Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("Abort Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int ConfigCapture(AxisObject axis, ProberInterfaces.EnumMotorDedicatedIn input)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].ConfigCapture(axis, input);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("ConfigCapture Error", ex, EventCodeEnum.MOTION_CONFIGCAPTURE_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("ConfigCapture Error in MotionProvider", ex);
            }

            return retVal;

        }
        public int DisableCapture(AxisObject axis)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].DisableCapture(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("DisableCapture Error", ex, EventCodeEnum.MOTION_CAPTURE_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("DisableCapture Error in MotionProvider", ex);
            }

            return retVal;

        }
        private void UpdateMotionProc()
        {
            var emp = MotionProviders.FirstOrDefault(m => m is ElmoMotionBase);
            try
            {
                while (bStopUpdateThread == false)
                {
                    if (emp != null)
                    {
                        UpdateProcTime = emp.UpdateProcTime;
                    }

                    for (int i = 0; i < AxisProviders.AxisObjects.Count; i++)
                    {
                        double actpos = 0.0;
                        double commandpos = 0.0;
                        double erropos = 0.0;
                        double pActPos = 0.0;
                        double pCommandPos = 0.0;
                        double pErrorPos = 0.0;
                        double auxpulse = 0;
                        //bool turnAmp = false;

                        var tPortNum = AxisProviders.AxisObjects[i].PortNum;
                        var tAxisIndex = AxisProviders.AxisObjects[i].AxisIndex;

                        Positions pulsePos = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].Pulse;

                        AxisProviders.AxisObjects[i].Status.State = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].State;
                        AxisProviders.AxisObjects[i].Status.StatusCode = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].StatusCode;
                        AxisProviders.AxisObjects[i].Status.AxisBusy = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].AxisBusy;
                        AxisProviders.AxisObjects[i].Status.Torque = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].Torque;

                        actpos = AxisProviders.AxisObjects[i].PtoD(pulsePos.Actual);
                        commandpos = AxisProviders.AxisObjects[i].PtoD(pulsePos.Command);
                        erropos = AxisProviders.AxisObjects[i].PtoD(pulsePos.Error);

                        pActPos = pulsePos.Actual;
                        pCommandPos = pulsePos.Command;
                        pErrorPos = pulsePos.Error;

                        //actpos = Math.Truncate(actpos);

                        AxisProviders.AxisObjects[i].Status.RawPosition.Actual = actpos;
                        AxisProviders.AxisObjects[i].Status.Pulse.Actual = pActPos;

                        //commandpos = Math.Truncate(commandpos);
                        //AxisProviders.AxisObjects[i].Status.RawPosition.Command = commandpos;
                        //AxisProviders.AxisObjects[i].Status.Pulse.Command = pCommandPos;
                        AxisProviders.AxisObjects[i].Status.RawPosition.Command
                            = AxisProviders.AxisObjects[i].PtoD(AxisProviders.AxisObjects[i].Status.Pulse.Command);

                        //erropos = Math.Truncate(erropos);
                        AxisProviders.AxisObjects[i].Status.RawPosition.Error = erropos;
                        AxisProviders.AxisObjects[i].Status.Pulse.Error = pErrorPos;

                        //AxisProviders.AxisObjects[i].Status.AxisEnabled = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].AxisEnabled;
                        AxisProviders.AxisObjects[i].Status.AxisEnabled = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].AxisEnabled;
                        AxisProviders.AxisObjects[i].Status.CapturePositions = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].CapturePositions;
                        AxisProviders.AxisObjects[i].Status.IsHomeSensor = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].IsHomeSensor;
                        AxisProviders.AxisObjects[i].Status.IsLimitSensor = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].IsLimitSensor;

                        auxpulse = _MotionProviders[tPortNum.Value].AxisStatusList[tAxisIndex.Value].AuxPosition;
                        //double dauxpulse = Convert.ToDouble(auxpulse);
                        //AxisProviders.AxisObjects[i].Status.AuxPosition = Convert.ToInt32(AxisProviders.AxisObjects[i].(dauxpulse));
                        AxisProviders.AxisObjects[i].Status.AuxPosition = auxpulse;
                    }
                    //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                    System.Threading.Thread.Sleep(MonitoringInterValInms);


                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

        }
        int IMotionProvider.LoadMotionBaseDescriptor(string MotionBaseParamFilePath)
        {

            int retVal = -1;

            try
            {
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("LoadMotionBaseDescriptor Error", ex, EventCodeEnum.MOTION_SETTING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {
                throw new Exception("LoadMotionBaseDescriptor Error in MotionProvider", ex);
            }

            return retVal;
        }
        public EventCodeEnum NotchFinding(AxisObject axis, EnumMotorDedicatedIn input)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].NotchFinding(axis, input);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }

            catch (MotionException ex)
            {
                throw new MotionException("NotchFinding Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("NotchFinding Error in MotionProvider", ex);
            }

            EventCodeEnum ret = retVal == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;

            return ret;
        }
        public int StartScanPosCapt(AxisObject axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            int retVal = -1;

            try
            {
                Thread.Sleep(100);
                retVal = MotionProviders[axis.PortNum.Value].StartScanPosCapt(axis, MotorDedicatedIn);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("StartScanPosCapt Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("StartScanPosCapt Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int StopScanPosCapt(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                Thread.Sleep(100);
                retVal = MotionProviders[axis.PortNum.Value].StopScanPosCapt(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }
            catch (MotionException ex)
            {
                throw new MotionException("StopScanPosCapt Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("StopScanPosCapt Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int ForcedZDown(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].ForcedZDown(axis);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("ForcedZDown Error", ex, EventCodeEnum.MOTION_MOVING_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("ForcedZDown Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetDualLoop(AxisObject axis, bool dualloop)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetDualLoop(dualloop);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetDualLoop Error", ex, EventCodeEnum.MOTION_CHUCKTILT_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetDualLoop Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetLoadCellZero(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetLoadCellZero();
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("SetLoadCellZero Error", ex, EventCodeEnum.MOTION_SETPOSITION_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetLoadCellZero Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int SetMotorStopCommand(AxisObject axis, string setevent, EnumMotorDedicatedIn input)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].SetMotorStopCommand(axis, setevent, input);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }
            catch (MotionException ex)
            {
                throw new MotionException("SetMotorStopCommand Error", ex, EventCodeEnum.MOTION_SETPOSITION_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("SetMotorStopCommand Error in MotionProvider", ex);
            }

            return retVal;

        }
        public bool IsEmulMode(ProbeAxisObject axis)
        {
            bool retVal = false;
            try
            {
                retVal = MotionProviders[axis.PortNum.Value] is EmulMotionBase;
            }
            catch (Exception ex)
            {

                throw new Exception("SetMotorStopCommand Error in MotionProvider", ex);
            }
            return retVal;
        }
        public int IsThreeLegUp(AxisObject axis, ref bool isthreelegup)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].IsThreeLegUp(axis, ref isthreelegup);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }
            catch (MotionException ex)
            {
                throw new MotionException("IsThreeLegUp Error", ex, EventCodeEnum.MOTION_THREE_LEG_UP_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("IsThreeLegUp Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int IsThreeLegDown(AxisObject axis, ref bool isthreelegdn)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].IsThreeLegDown(axis, ref isthreelegdn);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }
            catch (MotionException ex)
            {
                throw new MotionException("IsThreeLegDown Error", ex, EventCodeEnum.MOTION_THREE_LEG_DOWN_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("IsThreeLegDown Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int IsFls(AxisObject axis, ref bool isfls)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].IsFls(axis, ref isfls);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }
            catch (MotionException ex)
            {
                throw new MotionException("IsFls Error", ex, EventCodeEnum.MOTION_FATAL_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("IsFls Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int IsRls(AxisObject axis, ref bool isrls)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].IsRls(axis, ref isrls);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            }
            catch (MotionException ex)
            {
                throw new MotionException("IsRls Error", ex, EventCodeEnum.MOTION_FATAL_ERROR, retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("IsRls Error in MotionProvider", ex);
            }

            return retVal;
        }
        public int GetAuxPulse(AxisObject axis, ref int auxpulse)
        {
            int retVal = -1;

            try
            {
                retVal = MotionProviders[axis.PortNum.Value].GetAuxPulse(axis, ref auxpulse);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
            }

            catch (MotionException ex)
            {
                throw new MotionException("GetAuxPulse Error", ex, EnumReturnCodesConverter.ConvertToEventCode(retVal), retVal, this);
            }
            catch (Exception ex)
            {

                throw new Exception("GetAuxPulse Error in MotionProvider", ex);
            }

            return retVal;
        }
    }

    [Serializable]
    public class MotionBaseConfigurator : IParamNode, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        public MotionBaseConfigurator()
        {

        }

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public string FilePath { get; } = "";
        public string FileName { get; } = "MotionBaseParam.json";

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        private ObservableCollection<MotionBaseDescriptor> _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();
        public ObservableCollection<MotionBaseDescriptor> MotionBaseDescriptors
        {
            get { return _MotionBaseDescriptors; }
            set { _MotionBaseDescriptors = value; }
        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // SetDefaultParamEmul();
                //SetDefaultParamEmul();
                // SetDefaultParamEmul();
                //SetDefaultParamEmul();
                SetDefaultParamEmulGP();

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // SetDefaultParamEmul();
                // SetDefaultParamOPUSV();
                SetDefaultParamOPUSV_Machine3();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        private void SetDefaultParamEmul()
        {
            try
            {
                _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();

                MotionBaseDescriptor default1 = new MotionBaseDescriptor(MotionType.Emul, "0", 20);
                //MotionBaseDescriptor default2 = new MotionBaseDescriptor(MotionType.Emul, "1", 5);

                _MotionBaseDescriptors.Add(default1);
                //_MotionBaseDescriptors.Add(default2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultParamEmulGP()
        {
            _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();

            MotionBaseDescriptor default1 = new MotionBaseDescriptor(MotionType.Emul, "0", 20);
            MotionBaseDescriptor default2 = new MotionBaseDescriptor(MotionType.ADSRemote, "5.57.26.84.1.1", 11);
            _MotionBaseDescriptors.Add(default1);
            _MotionBaseDescriptors.Add(default2);
        }
        private void SetDefaultParamMPI()
        {
            try
            {
                _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();
                MotionBaseDescriptor default1 = new MotionBaseDescriptor(MotionType.MPI, "0", 32);
                MotionBaseDescriptor default2 = new MotionBaseDescriptor(MotionType.MPI, "1", 32);

                _MotionBaseDescriptors.Add(default1);
                _MotionBaseDescriptors.Add(default2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        private void SetDefaultParamOPUSV()
        {
            try
            {
                _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();
                MotionBaseDescriptor default1 = new MotionBaseDescriptor(MotionType.Elmo, "20", 32);
                // MotionBaseDescriptor default2 = new MotionBaseDescriptor(MotionType.TwinCat, "5.42.253.8.4.1", 32);
                //MotionBaseDescriptor default3 = new MotionBaseDescriptor(MotionType.Emul, "5", 32);

                _MotionBaseDescriptors.Add(default1);
                //_MotionBaseDescriptors.Add(default2);
                //_MotionBaseDescriptors.Add(default3);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        private void SetDefaultParamOPUSVLoader()
        {
            try
            {
                _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();
                MotionBaseDescriptor default1 = new MotionBaseDescriptor(MotionType.Emul, "5", 32);
                MotionBaseDescriptor default2 = new MotionBaseDescriptor(MotionType.Elmo, "6", 32);

                _MotionBaseDescriptors.Add(default1);
                _MotionBaseDescriptors.Add(default2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        private void SetDefaultParamBSCI1()
        {
            try
            {
                _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();
                MotionBaseDescriptor default1 = new MotionBaseDescriptor(MotionType.Elmo, "15", 32);// Stage + Loader

                _MotionBaseDescriptors.Add(default1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        private void SetDefaultParamOPUSV_Machine3()
        {
            try
            {
                _MotionBaseDescriptors = new ObservableCollection<MotionBaseDescriptor>();
                MotionBaseDescriptor default1 = new MotionBaseDescriptor(MotionType.Elmo, "20", 32);// Stage + Loader

                _MotionBaseDescriptors.Add(default1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
    }

    [Serializable]
    public class MotionBaseDescriptor : IParamNode
    {
        public List<object> Nodes { get; set; }

        private Element<string> _CtrlNum = new Element<string>();
        public Element<string> CtrlNum
        {
            get { return _CtrlNum; }
            set { _CtrlNum = value; }
        }

        private Element<int> _LimitNum = new Element<int>();
        public Element<int> LimitNum
        {
            get { return _LimitNum; }
            set { _LimitNum = value; }
        }

        private Element<MotionType> _Type = new Element<MotionType>();
        public Element<MotionType> Type
        {
            get { return _Type; }
            set
            {
                if (value != this.Type)
                {
                    _Type = value;
                }
            }
        }

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public MotionBaseDescriptor()
        {
            try
            {
                CtrlNum = null;
                Type.Value = MotionType.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public MotionBaseDescriptor(MotionType type, string ctrlNum, int limitNum)
        {
            try
            {
                _Type.Value = type;
                _CtrlNum.Value = ctrlNum;
                _LimitNum.Value = limitNum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}

