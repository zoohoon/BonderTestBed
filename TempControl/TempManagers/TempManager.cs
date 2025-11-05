using System;
using System.Collections.Generic;
using System.Linq;

namespace Temperature.Temp
{
    using global::TempManager;
    using LogModule;
    using Newtonsoft.Json;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Event;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.Chiller;
    using ProberInterfaces.Temperature.TempManager;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Xml.Serialization;
    using TempControllerParameter;

    [DataContract]
    public class TempManager : ITempManager
    {
        public List<object> Nodes { get; set; }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public ITempModule TempModule { get; private set; }

        #region Property

        public bool Initialized { get; set; } = false;

        private double _MV;
        public double MV
        {
            get { return _MV; }
            private set
            {
                if (value != _MV)
                {
                    _MV = value;
                }
            }
        }
        private double _PV;
        public double PV
        {
            get { return _PV; }
            private set
            {
                if (value != _PV)
                {
                    _PV = value;
                }
            }
        }

        private double _SV;
        public double SV
        {
            get { return _SV; }
            private set
            {
                if (value != _SV)
                {
                    _SV = value;
                }
            }
        }

        private TempSysParam _TempSysParam;
        public TempSysParam TempSysParam
        {
            get { return TempSysParam_IParam as TempSysParam; }
            set
            {
                if (value != _TempSysParam)
                {
                    _TempSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        [DataMember]
        public ITempCommInfoParam TempCommInfoParam { get { return TempSysParam.TempCommInfoParam; } }
        [DataMember]
        public Dictionary<double, double> Dic_HeaterOffset { get { return TempSysParam.HeaterOffsetDictionary.Value; } }
        [DataMember]
        public Dictionary<double, ITCParamArgs> Dic_TCParam { get { return TempSysParam.TCParamArgsDictionary.Value; } }
        [DataMember]
        public bool CheckingTCTempTable{ get { return TempSysParam.CheckingTCTempTable.Value; } }


        #endregion



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

        //static


        //delegate
        public delegate void DataHandler(TempUpdateEventArgs args);

        //private
        //private AutoResetEvent areUpdateIntv = new AutoResetEvent(false);
        //private Timer _monitoringTimer;
        private TempManagerState tempManagerState;

        private IParam _TempSysParam_IParam;
        public IParam TempSysParam_IParam
        {
            get { return _TempSysParam_IParam; }
            set
            {
                if (value != _TempSysParam_IParam)
                {
                    _TempSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TempSysParam tempManagerCommInfoParam { get { return TempSysParam_IParam as TempSysParam; } }

        //public
        public bool ChangeEnable { get; set; }

        public TempManager()
        {
            TempModule = new E5ENTempModule();
        }

        public TempManager(TempModuleMode tempMode)
        {
            try
            {
                if (tempMode == TempModuleMode.E5EN)
                {
                    TempModule = new E5ENTempModule();
                }
                else
                {
                    TempModule = new EmulTempModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        ~TempManager()
        {
            Disconnect();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if (Initialized == false)
                    {
                        TempManagerStateTransition(new TempManagerDisConnectedState(this));
                        ChangeEnable = TempCommInfoParam.Init_WriteEnable.Value;

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
                    retval = EventCodeEnum.UNDEFINED;
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public void DeInitModule()
        {
        }

        #region ==> Save & Load


        public EventCodeEnum LoadTempInfoParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new TempSysParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(TempSysParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    TempSysParam_IParam = tmpParam;
                }

                tempManagerCommInfoParam.TempCommInfoParam.Init_WriteEnable.PropertyChanged -= WriteParamChanged;
                tempManagerCommInfoParam.TempCommInfoParam.Init_WriteEnable.PropertyChanged += WriteParamChanged;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        #endregion

        private void WriteParamChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (tempManagerCommInfoParam.TempCommInfoParam.Init_WriteEnable.Value == true)
                {
                    this.SetRemote_ON(null);
                }
                else
                {
                    this.SetRemote_OFF(null);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum StartModule()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool tcIsAttached = tempManagerCommInfoParam?.TempCommInfoParam.IsAttached.Value ?? false;

                if (tcIsAttached)
                {
                    RetVal = this.StartTemp();

                    if (RetVal == EventCodeEnum.UNDEFINED)
                    {
                        TempManagerStateTransition(new TempManagerDisConnectedState(this));
                        RetVal = EventCodeEnum.UNDEFINED;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        private EventCodeEnum StartTemp()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ICommandManager commandManager = this.CommandManager();

                try
                {
                    bool isConnected = Connect(tempManagerCommInfoParam.TempCommInfoParam.SerialPort.Value, tempManagerCommInfoParam.TempCommInfoParam.Unitidentifier.Value);
                    if (isConnected)
                    {
                        //if(this.TempController().GetApplySVChangesBasedOnDeviceValue())
                        //{
                            commandManager.SetCommand<IChangeTemperatureToSetTempAfterConnectTempController>(this);
                        //}
                        StartUpdate();
                        RetVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        RetVal = EventCodeEnum.UNDEFINED;
                        throw new TemperatureConnectException("occur from StartTemp() in TempManager.");
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public void TempManagerStateTransition(TempManagerState state)
        {
            tempManagerState = state;
        }

        Thread UpdateThread;
        bool bStopUpdateThread;
        public void StartUpdate()
        {
            try
            {
                bStopUpdateThread = false;
                UpdateThread = new Thread(new ThreadStart(UpdateProc));
                UpdateThread.Name = this.GetType().Name;
                UpdateThread.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void UpdateProc()
        {
            try
            {
                while (!bStopUpdateThread)
                {
                    if (tempManagerState.GetState() == TempManagerStateEnum.DISCONNECTING
                        || tempManagerState.GetState() == TempManagerStateEnum.DISCONNECTED
                        || tempManagerState.GetState() == TempManagerStateEnum.ERROR)
                    {
                        if (tempManagerState.GetState() == TempManagerStateEnum.DISCONNECTING)
                            tempManagerState?.Disconnect();

                        Thread.Sleep(tempManagerCommInfoParam.TempCommInfoParam.GetCurTempLoopTime.Value);
                        //return;
                        continue;
                    }

                    ITempUpdateEventArgs tmp = tempManagerState?.Get_Cur_Temp(tempManagerCommInfoParam.TempCommInfoParam.GetCurTempLoopTime.Value);

                    if (tmp != null)
                    {
                        PV = tmp.PV;
                        SV = tmp.SV;
                        MV = tmp.MV;
                    }

                    Thread.Sleep(tempManagerCommInfoParam.TempCommInfoParam.GetCurTempLoopTime.Value);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                StopUpdate();
                //throw new TemperatureConnectException("occur from UpdateProc() in TempManager.");
            }
        }

        private void StopUpdate()
        {
            try
            {
                bStopUpdateThread = true;
                if (UpdateThread != null)
                {
                    UpdateThread.Join();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool Connect(string serialPort, byte uintidentifier)
        {
            bool? result = tempManagerState?.Connect(serialPort, uintidentifier);
            return result ?? false;
        }

        public void Disconnect()
        {
            StopUpdate();
        }

        public void Dispose()
        {
            Disconnect();
        }

        public double ReadMV()
        {
            double? result = tempManagerState?.ReadMV();
            return result ?? -1;
        }

        public double ReadPV()
        {
            double? result = tempManagerState?.ReadPV();
            return result ?? -1;
        }

        public double ReadSetV()
        {
            double? result = tempManagerState?.ReadSetV();
            return result ?? -1;
        }

        public long ReadStatus()
        {
            long? result = tempManagerState?.ReadStatus();
            return result ?? -1;
        }

        public long Get_OutPut_State()
        {
            long? result = tempManagerState?.Get_OutPut_State();
            return result ?? -1;
        }
        public void SetRemote_OFF(object notUsed)
        {
            tempManagerState?.SetRemote_OFF(notUsed);
        }

        public void SetRemote_ON(object notUsed)
        {
            tempManagerState?.SetRemote_ON(notUsed);
        }

        public void SetTempWithOption(double value)
        {
            if (tempManagerState.GetState() == TempManagerStateEnum.CONN_WRT_ENABLE)
            {
                try
                {
                    Set_SV(value);

                    double temperature = Math.Truncate((value));
                    double offset = 0.0;
                    offset = GetTempOffset(temperature);

                    Set_Offset(offset);

                    ////////////////////////////////////////////////////////////////////////////////////////////

                    double TCValue = value;
                    ITCParamArgs settingPidValue;
                    if (!this.Dic_TCParam.ContainsKey(TCValue))
                    {
                        var bestMatch = Dic_TCParam.OrderBy(e => Math.Abs(e.Key - TCValue)).FirstOrDefault();
                        settingPidValue = bestMatch.Value;
                    }
                    else
                    {
                        settingPidValue = this.Dic_TCParam[TCValue];
                    }
                    Set_PB(settingPidValue.Pb.Value);
                    Set_IT(settingPidValue.iT.Value);
                    Set_DT(settingPidValue.dE.Value);

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
        }

        /// <summary>
        /// 1/10
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns></returns>
        public double GetTempOffset(double temperature)
        {
            double retOffset = 0.0;
            try
            {
                if (!Dic_HeaterOffset.ContainsKey(temperature))
                {
                    LinkedList<KeyValuePair<double, double>> linkedListData
                        = new LinkedList<KeyValuePair<double, double>>(Dic_HeaterOffset.OrderBy(i => i.Key));

                    bool haveToBreak = false;
                    for (var recentNode = linkedListData.First; recentNode != null; recentNode = recentNode.Next)
                    {
                        if (recentNode.Previous == null && temperature < recentNode.Value.Key)
                        {
                            retOffset = 0;
                            haveToBreak = true;
                        }
                        else if (recentNode.Previous != null && temperature <= recentNode.Value.Key)
                        {
                            //double lowerKey = recentNode.Previous.Value.Key;
                            //double lowerOffset = recentNode.Previous.Value.Value;
                            //double upperKey = recentNode.Value.Key;
                            //double upperOffset = recentNode.Value.Value;

                            double lowerKey = recentNode.Previous.Value.Key;
                            double lowerOffset = recentNode.Previous.Value.Value - lowerKey;
                            double upperKey = recentNode.Value.Key;
                            double upperOffset = recentNode.Value.Value - upperKey;

                            retOffset = (((upperOffset - lowerOffset) / (upperKey - lowerKey)) * (temperature - lowerKey)) + lowerOffset;
                            haveToBreak = true;
                        }
                        else if (recentNode.Next == null && recentNode.Value.Key <= temperature)
                        {
                            retOffset = temperature - recentNode.Value.Value;
                            //retOffset = recentNode.Value.Value;
                            haveToBreak = true;
                        }

                        if (haveToBreak)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    retOffset = temperature - Dic_HeaterOffset[temperature];
                }
                //retOffset *= 10;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retOffset;
        }

        //public AlarmSVChanged NotifySVChanged { get; set; }

        public void Set_SV(double value)
        {
            double prevSV = this.TempController().TempInfo.SetTemp.Value;
            //NotifySVChanged?.Invoke(prevSV, value, PV);

            SemaphoreSlim semaphore = new SemaphoreSlim(0);

            List<double> eventArgs = new List<double>();
            eventArgs.Add(prevSV);
            eventArgs.Add(value);
            eventArgs.Add(PV);
            this.EventManager().RaisingEvent(typeof(AlarmSVChangedEvent).FullName, new ProbeEventArgs(this, semaphore, eventArgs));
            //SaveTempInfoParam();
            LoggerManager.Debug($"Set_SV [{value}]");
            tempManagerState?.SetSV(value);
        }

        public void Set_DT(double value)
        {
            tempManagerState?.Set_DT(value);
        }

        public void Set_IT(double value)
        {
            tempManagerState?.Set_IT(value);
        }

        public void Set_Offset(double value)
        {
            tempManagerState?.Set_Offset(value);
        }

        public void Set_OutPut_OFF(object notUsed)
        {
            try
            {
                tempManagerState?.Set_OutPut_OFF(notUsed);
            }
            catch (Exception err)
            {
                this.NotifyManager().Notify(EventCodeEnum.HEATER_POWER_SUPPLY_FAIL);
                LoggerManager.Exception(err);
            }
        }

        public void Set_OutPut_ON(object notUsed)
        {
            try
            {
                tempManagerState?.Set_OutPut_ON(notUsed);
            }
            catch (Exception err)
            {
                this.NotifyManager().Notify(EventCodeEnum.HEATER_POWER_SUPPLY_FAIL);
                LoggerManager.Exception(err);
            }
        }

        public void Set_PB(double value)
        {
            tempManagerState?.Set_PB(value);
        }

        ////////////////////////////////////////////////////////////////////////////

        private int _SVPb;
        public int SVPb
        {
            get { return _SVPb; }
            set
            {
                _SVPb = value;
                RaisePropertyChanged();
            }
        }
        private int _SViT;
        public int SViT
        {
            get { return _SViT; }
            set
            {
                _SViT = value;
                RaisePropertyChanged();
            }
        }
        private int _SVdE;
        public int SVdE
        {
            get { return _SVdE; }
            set
            {
                _SVdE = value;
                RaisePropertyChanged();
            }
        }

        private int _Pb;
        public int Pb
        {
            get { return _Pb; }
            set
            {
                _Pb = value;
                RaisePropertyChanged();
            }
        }
        private int _iT;
        public int iT
        {
            get { return _iT; }
            set
            {
                _iT = value;
                RaisePropertyChanged();
            }
        }
        private int _dE;
        public int dE
        {
            get { return _dE; }
            set
            {
                _dE = value;
                RaisePropertyChanged();
            }
        }

        private class PID : IPID
        {
            public double SVPb { get; set; }
            public double SViT { get; set; }
            public double SVdE { get; set; }
        }

        public IPID LoadTCGainSVtemp(double tmpsv)
        {
            IPID retVal = new PID();
            try
            {

                if (TempSysParam.TCParamArgsDictionary == null)
                    TempSysParam.TCParamArgsDictionary = new Element<Dictionary<double, ITCParamArgs>>();

                double key = tmpsv;
                if (!TempSysParam.TCParamArgsDictionary.Value.ContainsKey(key))
                {
                    TempSysParam.TCParamArgsDictionary.Value.Add(key,
                    new TCParamArgs()
                    {
                        Pb = new Element<double>() { Value = 0.1 }
                                        ,
                        iT = new Element<double>() { Value = 10 }
                                        ,
                        dE = new Element<double>() { Value = 1 }
                    });
                    SaveSysParameter();
                }
                else
                { }

                retVal.SVPb = (int)TempSysParam.TCParamArgsDictionary.Value[key].Pb.Value;
                retVal.SViT = (int)TempSysParam.TCParamArgsDictionary.Value[key].iT.Value;
                retVal.SVdE = (int)TempSysParam.TCParamArgsDictionary.Value[key].dE.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        //public void TermTempControl()
        //{
        //    //this.StopUpdate();

        //    //while (IsEnterUpdateProc)
        //    //{
        //    //    Thread.Sleep(100);
        //    //}

        //    //Thread.Sleep(300);
        //}

        //public void InitTempControl()
        //{
        //    //if(UpdateThread?.ThreadState != ThreadState.Running)
        //    this.TempInit();
        //}

        public void SaveTCGainCurtemp(double mSVPb, double mSViT, double mSVdE)
        {
            try
            {
                if (TempSysParam.TCParamArgsDictionary == null)
                    LoadSysParameter();

                double key = PV;
                if (!TempSysParam.TCParamArgsDictionary.Value.ContainsKey(key))
                {
                    TempSysParam.TCParamArgsDictionary.Value.Add(key,
                    new TCParamArgs()
                    {
                        Pb = new Element<double>() { Value = mSVPb }
                                        ,
                        iT = new Element<double>() { Value = mSViT }
                                        ,
                        dE = new Element<double>() { Value = mSVdE }
                    });
                }
                else
                {
                    TempSysParam.TCParamArgsDictionary.Value[key].Pb.Value = mSVPb;
                    TempSysParam.TCParamArgsDictionary.Value[key].iT.Value = mSViT;
                    TempSysParam.TCParamArgsDictionary.Value[key].dE.Value = mSVdE;
                }

                SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SaveTCGainSVtemp(int SVTemp, double mSVPb, double mSViT, double mSVdE)
        {
            try
            {
                if (TempSysParam.TCParamArgsDictionary.Value == null)
                    LoadSysParameter();

                double key = SVTemp;
                if (!TempSysParam.TCParamArgsDictionary.Value.ContainsKey(key))
                {
                    TempSysParam.TCParamArgsDictionary.Value.Add(key,
                    new TCParamArgs()
                    {
                        Pb = new Element<double>() { Value = mSVPb }
                        ,
                        iT = new Element<double>() { Value = mSViT }
                        ,
                        dE = new Element<double>() { Value = mSVdE }
                    });
                }
                else
                {
                    TempSysParam.TCParamArgsDictionary.Value[key].Pb.Value = mSVPb;
                    TempSysParam.TCParamArgsDictionary.Value[key].iT.Value = mSViT;
                    TempSysParam.TCParamArgsDictionary.Value[key].dE.Value = mSVdE;
                }
                SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public double mSetPoint { get; set; }

        //public IParam SysParam { get; set; }

        //public int Chuck_SetTemp(int itemp, bool bSavetoSetPoint = true)
        //{
        //    IsEnterUpdateProc = true;
        //    if (TempSysParam.TCParamArgsDictionary.Value == null)
        //        LoadSysParameter();

        //    int retVal = 0;
        //    double dblP;
        //    double dblIt;
        //    double dbldE;
        //    StringBuilder sb = new StringBuilder();

        //    //TermTempControl();

        //    LoggerManager.Debug("Wait mWatHB Object To SetTempOffset ");

        //    tempControlState.SetSV(itemp);

        //    if (tempManagerCommInfoParam.TempCommInfoParam.HCType.Value == 1 || tempManagerCommInfoParam.TempCommInfoParam.HCType.Value == 2)
        //    {
        //        double key = (double)(((int)(itemp / 100)) * 100);
        //        if (TempSysParam.TCParamArgsDictionary.Value.ContainsKey(key))
        //        {
        //            dblP = TempSysParam.TCParamArgsDictionary.Value[key].Pb.Value;
        //            if (dblP < 0.1 || 999.9 < dblP)
        //            {
        //                LoggerManager.Debug("Temp Controller: Propotional band out of range. Set default value = 1.");
        //                dblP = 1;
        //            }

        //            dblIt = TempSysParam.TCParamArgsDictionary.Value[key].iT.Value;
        //            if (dblIt < 0 || 3999 < dblIt)
        //            {
        //                LoggerManager.Debug("Temp Controller: Integral time out of range. Set default value = 100.");
        //                dblIt = 1;
        //            }

        //            dbldE = TempSysParam.TCParamArgsDictionary.Value[key].dE.Value;
        //            if (dbldE < 0 || 3999 < dbldE)
        //            {
        //                LoggerManager.Debug("Temp Controller: Derivative time out of range. Set default value = 10.");
        //                dbldE = 1;
        //            }
        //        }
        //        else
        //        {
        //            TempSysParam.TCParamArgsDictionary.Value.Add(key, 
        //                new TCParamArgs() { Pb = new Element<double>() { Value = 10 }
        //                                    , iT = new Element<double>() { Value = 100 }
        //                                    , dE = new Element<double>() { Value = 10  } });
        //            SaveSysParameter();
        //            dblP = 10;
        //            dblIt = 100;
        //            dbldE = 10;
        //        }

        //        if (tempManagerCommInfoParam.TempCommInfoParam.HCType.Value == 2)
        //        {
        //            tempControlState.Set_PB(dblP);
        //            tempControlState.Set_IT(dblIt);
        //            tempControlState.Set_DT(dbldE);
        //        }
        //        else
        //        {
        //            tempControlState.Set_PB(dblP);
        //            tempControlState.Set_IT((dblIt / 10));
        //            tempControlState.Set_DT((dbldE / 10));
        //        }
        //    }

        //    mSetPoint = itemp;

        //    tempControlState.Set_OutPut_ON(null);
        //    //InitModule();
        //    //LoadSysParameter();

        //    IsEnterUpdateProc = false;

        //    return retVal;
        //}

        //public int Chuck_SetTempOffset(int ioffset)
        //{
        //    int retVal = 0;

        //    //TermTempControl();

        //    tempControlState.Set_Offset(ioffset);

        //    //InitModule();
        //    //LoadSysParameter();

        //    return retVal;
        //}

        public void ClearInputAlarm()
        {
            tempManagerState.WriteSingleRegister(311, 1);
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = LoadTempInfoParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = SaveTempInfoParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum SaveTempInfoParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(TempSysParam_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        /// <summary>
        /// 오직 에뮬에서만 사용!! 절대 호출 하지 말것!!
        /// </summary>
        /// <param name="temp"></param>
        public void ChangeCurTemp(double temp)
        {
            try
            {
                this.PV = (long)temp;
                TempModule.ChangeCurTemp(temp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
