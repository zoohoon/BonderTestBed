using System;
using System.Collections.Generic;

namespace LoaderServiceClientModules.TempController
{
    using Autofac;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.Communication;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.Chiller;
    using ProberInterfaces.Temperature.DewPoint;
    using ProberInterfaces.Temperature.DryAir;
    using ProberInterfaces.Temperature.TempManager;
    using SciChart.Charting.Model.DataSeries;
    using Temperature;
    using System.Threading;
    using System.Timers;
    using LogModule;
    using SerializerUtil;

    public class TempControllerServiceClient : ITempController, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }
        //private IRemoteMediumProxy CurStageObj => LoaderCommunicationManager.GetRemoteMediumService();

        #region // Properties

        bool bStopUpdateThread;
        Thread UpdateThread;
        private bool updateEnable = false;

        private XyDataSeries<DateTime, double> _dataSeries_CurTemp
            = new XyDataSeries<DateTime, double>() { SeriesName = "PV" };
        public XyDataSeries<DateTime, double> dataSeries_CurTemp
        {
            get { return _dataSeries_CurTemp; }
            set
            {
                if (value != _dataSeries_CurTemp)
                {
                    _dataSeries_CurTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private XyDataSeries<DateTime, double> _dataSeries_SetTemp
            = new XyDataSeries<DateTime, double>() { SeriesName = "SV" };
        public XyDataSeries<DateTime, double> dataSeries_SetTemp
        {
            get { return _dataSeries_SetTemp; }
            set
            {
                if (value != _dataSeries_SetTemp)
                {
                    _dataSeries_SetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OverHeatTemp = 0;
        public double OverHeatTemp
        {
            get { return _OverHeatTemp; }
            set
            {
                if (value != _OverHeatTemp)
                {
                    _OverHeatTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TempControllerParam _TempControllerParam;
        public TempControllerParam TempControllerParam
        {
            get { return _TempControllerParam; }
            set { _TempControllerParam = value; }
        }

        private TimeSpan _RunTimeSpan;
        public TimeSpan RunTimeSpan
        {
            get { return _RunTimeSpan; }
            set
            {
                if (value != _RunTimeSpan)
                {
                    _RunTimeSpan = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ITempManager _TempManger;
        public ITempManager TempManager
        {
            get
            {
                return GetTempManager();
            }
            set
            {
                //_TempManger = value;
            }
        }
        private IChillerModule _ChillerModule;
        public IChillerModule ChillerModule
        {
            get
            {
                return _ChillerModule;
            }
            set
            {
                _ChillerModule = value;
            }
        }

        private IDewPointModule _DewPointModule;
        public IDewPointModule DewPointModule
        {
            get
            {
                return _DewPointModule;
            }
            set
            {
                _DewPointModule = value;
            }
        }
        private IDryAirModule _DryAirModule;
        public IDryAirModule DryAirModule
        {
            get
            {
                return _DryAirModule;
            }
            set
            {
                _DryAirModule = value;
            }
        }
        #endregion

        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }
        public EnumCommunicationState CommunicationState
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    return EnumCommunicationState.CONNECTED;
                }
                else
                {
                    return EnumCommunicationState.UNAVAILABLE;
                }
            }
            set { }
        }

        #region // IStateModule Implementation. Does not need on remote system.
        public ReasonOfError ReasonOfError
        {
            get
            {
                return new ReasonOfError(ModuleEnum.Temperature);
            }
            set => throw new NotImplementedException();
        }
        public CommandSlot CommandSendSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvProcSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvDoneSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandTokenSet RunTokenSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandInformation CommandInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ModuleStateBase ModuleState { get; set; }

        public ObservableCollection<TransitionInfo> TransitionInfo { get; set; }

        public EnumModuleForcedState ForcedDone { get; set; }
        public IParam TempSafetyDevParam { get; set; }
        public IParam TempControllerDevParam { get; set; }

        public double CurTemp { get; set; }

        public double SetTemp { get; set; }
        public TemperatureInfo TempInfo { get; set; }
        public bool IsPurgeAirBackUpValue { get; set; }

        public IParam TempSafetySysParam => throw new NotImplementedException();

        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public void StateTransition(ModuleStateBase state)
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Execute()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum End()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {
            throw new NotImplementedException();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }


        public bool IsLotReady(out string msg)
        {
            bool retval = true;
            try
            {
                msg = "";
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                bStopUpdateThread = true;
                UpdateThread?.Join();

            }
            catch (Exception err)
            {
                LoggerManager.Error($"DeInitTempControllerServiceClient() Function error: " + err.Message);
            }
            Initialized = false;
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                if (!Initialized)
                {
                    if (TempInfo == null)
                        TempInfo = new TemperatureInfo();


                    bStopUpdateThread = false;
                    UpdateThread = new Thread(new ThreadStart(UpdateData));
                    UpdateThread.Name = this.GetType().Name;
                    UpdateThread.Start();

                    Initialized = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }

        private void UpdateData()
        {
            bool errorHandled = false;
            int maxTrendHistoryCount = 60 * 60;
            while (bStopUpdateThread == false)
            {
                try
                {
                    if (Initialized == true & updateEnable == true)
                    {
                        if (dataSeries_CurTemp != null & dataSeries_SetTemp != null)
                        {
                            if (dataSeries_CurTemp.Count != dataSeries_SetTemp.Count)
                            {
                                dataSeries_CurTemp = new XyDataSeries<DateTime, double>();
                                dataSeries_SetTemp = new XyDataSeries<DateTime, double>();
                            }
                        }
                        else
                        {
                            dataSeries_CurTemp = new XyDataSeries<DateTime, double>();
                            dataSeries_SetTemp = new XyDataSeries<DateTime, double>();
                        }

                        CurTemp = GetTemperature();
                        SetTemp = GetSetTemp();

                        var updateTime = DateTime.Now;
                        dataSeries_CurTemp.Append(updateTime, CurTemp * 1.0);
                        dataSeries_SetTemp.Append(updateTime, SetTemp * 1.0);
                        if (dataSeries_CurTemp.Count > maxTrendHistoryCount)
                        {
                            dataSeries_CurTemp.RemoveAt(0);
                            dataSeries_SetTemp.RemoveAt(0);
                        }
                        TempInfo.CurTemp.Value = CurTemp;
                        TempInfo.SetTemp.Value = SetTemp;

                    }
                    if (errorHandled == true)
                    {
                        LoggerManager.Debug($"TempControllr.UpdateData(): Error recovered.");
                        errorHandled = false;
                    }

                }
                catch (Exception err)
                {
                    if (errorHandled == false)
                    {
                        LoggerManager.Error($"TempControllr.UpdateData(): Error occurred. Err = {err.Message}");
                        errorHandled = true;
                    }
                }

                Thread.Sleep(2000);
            }
        }

        #endregion


        public double GetSetTempWithOverHeatTemp()
        {
            throw new NotImplementedException();
        }

        public void SetTemperatureFromDevParamSetTemp()
        {
            throw new NotImplementedException();
        }

        public bool IsCurTempWithinSetTempRange(bool checkDevTemp = true)
        {
            bool retVal = false;
            try
            {
                retVal = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().IsCurTempWithinSetTempRange();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsCurTempWithinSetTempRangeDeviation(double deviationval)
        {
            return false;
        }

        public bool IsCurTempUpperThanSetTemp(double setTemp, double margin)
        {
            bool retVal = false;
            try
            {
                retVal = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().IsCurTempUpperThanSetTemp(setTemp, margin);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public double GetTemperatureOffset(double temperature)
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetTemperatureOffset(temperature);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        /// <summary>
        /// TempManager is not available in remote mode. Use ITempControllerProxy.
        /// </summary>
        /// <returns></returns>
        public ITempManager GetTempManager()
        {
            //var tempManager = CurStageObj.GetTempModule();
            //return CurStageObj.GetTempModule().GetTempManager();
            return null;
        }
        public void SetSV(TemperatureChangeSource source, double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false, double overHeating = 0.0, double Hysteresis = 0.0)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetSV(source, changeSetTemp, willYouSaveSetValue, forcedSetValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetAmbientTemp([CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetAmbientTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public int GetHeaterOffsetCount()
        {
            int retval = 0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetHeaterOffsetCount();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Dictionary<double, double> GetHeaterOffsets()
        {
            Dictionary<double, double> retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetHeaterOffset();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public void ClearHeaterOffset()
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().ClearHeaterOffset();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void AddHeaterOffset(double reftemp, double measuredtemp)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().AddHeaterOffset(reftemp, measuredtemp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SaveOffsetParameter()
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SaveOffsetParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public double GetTemperature()
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetTemperature();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void SetTemperature()
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetTemperature();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public double GetMonitoringMVTimeInSec()
        {
            double retVal = 0.0;

            try
            {
                retVal = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetMonitoringMVTimeInSec();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetMonitoringMVTimeInSec(double value)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetMonitoringMVTimeInSec(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetVacuum(bool ison)
        {
            try
            {
                if (LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>() != null)
                {
                    LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().SetVacuum(ison);
                }
            }
            catch (Exception err)
            {

                LoggerManager.Debug($"TempControllerServiceClient.SetVacuum(): Err occurred. Err = {err.Message}");
            }
        }

        public void EnableRemoteUpdate()
        {
            updateEnable = true;
        }

        public void DisableRemoteUpdate()
        {
            updateEnable = false;
        }

        public bool IsServiceAvailable()
        {
            throw new NotImplementedException();
        }

        public void SetLoggingInterval(long seconds)
        {
            try
            {
                ITempControllerProxy tempcontrollerproxy = LoaderCommunicationManager.GetProxy<ITempControllerProxy>();

                if (tempcontrollerproxy != null)
                {
                    tempcontrollerproxy.SetLoggingInterval(seconds);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumTemperatureState GetTempControllerState()
        {
            EnumTemperatureState retval = EnumTemperatureState.IDLE;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetTempControllerState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetDeviaitionValue()
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetDeviaitionValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public TempPauseTypeEnum GetTempPauseType()
        {
            TempPauseTypeEnum retval = TempPauseTypeEnum.NONE;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetTempPauseType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public double GetEmergencyAbortTempTolereance()
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetEmergencyAbortTempTolereance();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void SetDeviaitionValue(double deviation, bool emergencyparam = false)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetDeviaitionValue(deviation, emergencyparam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetTempPauseType(TempPauseTypeEnum pausetype)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetTempPauseType(pausetype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool CheckSetDeviationParamLimit(double deviation, bool emergencyparam = false)
        {
            bool retval = false;
            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().CheckSetDeviationParamLimit(deviation, emergencyparam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void SettingSV(double changeSetTemp, bool willYouSaveSetValue = true)
        {
            return;
        }

        public bool IsUsingDryAir()
        {
            return false;
        }

        public bool IsUsingChiller()
        {
            return false;
        }

        public List<IOPortDescripter<bool>> GetDryAirIoMapInputPorts()
        {
            return null;
        }

        public List<IOPortDescripter<bool>> GetDryAirIoMapOutputPorts()
        {
            return null;
        }


        public double GetCurDewPointValue()
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetCurDewPointValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetMV()
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetMV();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetDewPointTolerance()
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetDewPointTolerance();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetSetTemp()
        {
            double retval = 0.0;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetSetTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetHeaterOutPutState()
        {
            bool retval = false;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetHeaterOutPutState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetTempMonitorInfo(TempMonitoringInfo param)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetTempMonitorInfo(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public TempMonitoringInfo GetTempMonitorInfo()
        {
            TempMonitoringInfo retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetTempMonitorInfo();

                if (retval != null)
                {
                    TempInfo.MonitoringInfo.TempMonitorRange = retval.TempMonitorRange;
                    TempInfo.MonitoringInfo.WaitMonitorTimeSec = retval.WaitMonitorTimeSec;
                    TempInfo.MonitoringInfo.MonitoringEnable = retval.MonitoringEnable;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var devparam = LoaderCommunicationManager.GetProxy<ITempControllerProxy>()?.GetParamByte();

                if (devparam != null)
                {
                    object target = null;
                    var result = SerializeManager.DeserializeFromByte(devparam, out target, typeof(TempControllerDevParam));

                    if (target != null)
                    {
                        TempControllerDevParam = target as TempControllerDevParam;
                        SaveDevParameter();
                    }
                    else
                    {
                        LoggerManager.Error($"Load remote temp dev param function is faild.");
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var param = SerializeManager.SerializeToByte(TempControllerDevParam, typeof(TempControllerDevParam));
                LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetParamByte(param);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public byte[] GetParamByte()
        {
            return null;
        }
        public EventCodeEnum SetParamByte(byte[] devparam)
        {
            return EventCodeEnum.NONE;
        }

        public void SetEndTempEmergencyErrorCommand()
        {
            return;
        }
        public bool GetIsOccurTimeOutError()
        {
            return false;
        }
        public void ClearTimeOutError()
        {
            return;
        }
        public double GetCCActivatableTemp()
        {
            return 0.0;
        }
        public EventCodeEnum CheckSVWithDevice()
        {
            return EventCodeEnum.NONE;
        }

        public bool CheckIfTempIsIncluded()
        {
            return false;
        }

        public bool GetCheckingTCTempTable()
        {
            return false;
        }

        public bool ControlTopPurgeAir()
        {
            return false;
        }

        public double GetDevSetTemp()
        {
            double ret = -1;
            try
            {
                if(LoaderCommunicationManager.GetProxy<ITempControllerProxy>() != null)
                {
                    return LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetDevSetTemp();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum SetDevSetTemp(double setTemp)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetDevSetTemp(setTemp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool GetApplySVChangesBasedOnDeviceValue()
        {
            bool retVal = false;
            try
            {
                if (LoaderCommunicationManager.GetProxy<ITempControllerProxy>() != null)
                {
                    return LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetApplySVChangesBasedOnDeviceValue();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsUsingChillerState()
        {
            return false;
        }

        public long GetLimitRunTimeSeconds()
        {
            return 0;
        }

        public void SetActivatedState(bool forced = false)
        {
            try
            {
                if (LoaderCommunicationManager.GetProxy<ITempControllerProxy>() != null)
                {
                    LoaderCommunicationManager.GetProxy<ITempControllerProxy>().SetActivatedState(forced);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public TempEventInfo GetPreviousTempInfoInHistory()
        {
            TempEventInfo ret = null;
            try
            {
                if (LoaderCommunicationManager.GetProxy<ITempControllerProxy>() != null)
                {
                    ret = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetPreviousTempInfoInHistory();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        
        public TempEventInfo GetPreviousSourceTempInfoInHistory()
        {
            TempEventInfo ret = null;
            try
            {
                if (LoaderCommunicationManager.GetProxy<ITempControllerProxy>() != null)
                {
                    ret = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetPreviousSourceTempInfoInHistory();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public TempEventInfo GetCurrentTempInfoInHistory()
        {
            TempEventInfo ret = null;
            try
            {
                if (LoaderCommunicationManager.GetProxy<ITempControllerProxy>() != null)
                {
                    ret = LoaderCommunicationManager.GetProxy<ITempControllerProxy>().GetCurrentTempInfoInHistory();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public void RestorePrevSetTemp()
        {

        }
    }
}
