using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using ProberInterfaces.EnvControl.Enum;
using ProberInterfaces.Temperature.EnvMonitoring;
using SequenceService;

namespace EnvMonitoring
{
    public class EnvMonitoringManager : SequenceServiceBase, IEnvMonitoringManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private SensorParameters _SensorParams;

        public SensorParameters SensorParams
        {
            get { return _SensorParams; }
            set
            {
                if (value != _SensorParams)
                {
                    _SensorParams = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<IEnvMonitoringHub> _EnvMonitoringHubs
            = new List<IEnvMonitoringHub>();
        public List<IEnvMonitoringHub> EnvMonitoringHubs
        {
            get { return _EnvMonitoringHubs; }
            set
            {
                if (value != _EnvMonitoringHubs)
                {
                    _EnvMonitoringHubs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ISensorModule> _SensorModules
            = new List<ISensorModule>();
        public List<ISensorModule> SensorModules
        {
            get { return _SensorModules; }
            set
            {
                if (value != _SensorModules)
                {
                    _SensorModules = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AfterErrorBehaviorParam AfterErrorBehaviorParam { get; set; }

        private List<IAfterErrorBehavior> _AfterErrorBehaviorList;
        public List<IAfterErrorBehavior> AfterErrorBehaviorList
        {
            get { return _AfterErrorBehaviorList; }
            set
            {
                if (value != _AfterErrorBehaviorList)
                {
                    _AfterErrorBehaviorList = value;
                }
            }
        }

        private EnumCommunicationState prevCommState = EnumCommunicationState.DISCONNECT;

        #region <remarks> Init & DeInit </remarks>
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    CreateModules();
                    //LoadAferErrorBehaviorParams();
                    retval = EventCodeEnum.NONE;
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Task 가 돌기 전에, sensor & hub 데이터를 만들어주기 위한 함수
        /// </summary>
        public void CreateModules()
        {
            try
            {
                if (SensorParams == null)
                    return;

                for (int i = 0; i < SensorParams.SensorCommParams.Count; i++)
                {                    
                    // 현재 허브에 연결되어 있는 센서들을 만들어주기.
                    List<SensorSysParameter> param = SensorParams.SensorSysParams.FindAll(a => a.Hub.Value == SensorParams.SensorCommParams[i].Hub.Value);
                    if (param.Count != 0)
                    {
                        SensorModules.Clear();
                        foreach (var item in SensorParams.SensorSysParams)
                        {
                            if (item.Sensor is SmokeSensorSettingParam)
                            {
                                if (item.RunMode.Value == "EMUL")
                                {
                                    //SensorModules.Add(new EmulSensorModule());
                                }
                                else
                                {
                                    if (item.ModuleAttached.Value)
                                    {
                                        SensorModules.Add(new SensorModule(item, new SmokeSensorInfo()));
                                    }
                                }
                            }
                        }                        
                    }
                    // hub 개수를 파악해서 hub 개수만큼 hub class 만들기
                    EnvMonitoringHubs.Clear();
                    EnvMonitoringHubs.Add(new EnvMonitoringHub(SensorParams.SensorCommParams[i], SensorModules));
                    EnvMonitoringHubs[i].Commmunication();

                    if (SensorModules.Count != 0)
                    {
                        for (int j = 0; j < SensorModules.Count; j++)
                        {
                            SensorModules[j].InitModule();
                            SensorModules[j].TempChangedEvent += ReportSensorTempSVID;
                            SensorModules[j].StatusChangedEvent += ReportSensorStatusSVID;
                            SensorModules[j].RequestDataEvent += EnvMonitoringHubs[i].Send;
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ReportSensorTempSVID(int idx, double temp)
        {
            try
            {
                if (idx < -1)
                    return;
                this.GEMModule().GetPIVContainer()?.SetSmokeSensorTempState(idx - 1, temp);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }
        }

        private void ReportSensorStatusSVID(int idx, GEMSensorStatusEnum status)
        {
            try
            {
                if (idx < -1)
                    return;
                this.GEMModule().GetPIVContainer()?.SetSmokeSensorStatusState(idx - 1, status);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        
        public override ModuleStateEnum SequenceRun()
        { 
            ModuleStateEnum retVal = ModuleStateEnum.RUNNING;
            try
            {
                if (SensorModules == null || EnvMonitoringHubs == null)
                {
                    retVal = ModuleStateEnum.ERROR;
                    return retVal;
                }

                foreach (var item in EnvMonitoringHubs)
                {
                    List<ISensorModule> sensorModules = SensorModules.FindAll(a => a.SensorSysParameter.Hub.Value == item.CommunicationParam.Hub.Value);
                    var curCommState = item.CommModule.GetCommState();
                    if (prevCommState == EnumCommunicationState.CONNECTED &&
                                curCommState == EnumCommunicationState.DISCONNECT)
                    {
                        //Hub Disconnect처리를 한다. 
                        for (int i = 0; i < sensorModules.Count; i++)
                        {
                            sensorModules[i].SensorEnable = false;
                            sensorModules[i].GEMSensorStatus = GEMSensorStatusEnum.DISCONNECTED;
                            sensorModules[i].SensorStatus = SensorStatusEnum.DISCONNECT_HUB;                            
                        }                        
                    }
                    else if (curCommState == EnumCommunicationState.CONNECTED)
                    {                       
                        for (int i = 0; i < sensorModules.Count; i++)
                        {
                            if (item.CommModule.Port != null)
                            {
                                if (!item.CommModule.Port.IsOpen)
                                {                                    
                                    item.CommModule.DisConnect();                                    
                                    return retVal;
                                }

                                if (sensorModules[i].SensorSysParameter.ModuleAttached.Value && sensorModules[i].SensorSysParameter.ModuleEnable.Value)
                                {
                                    sensorModules[i].Execute();     // request - response 동작이 다 이루어짐.                                                                         
                                    AlarmNotifyStatus(sensorModules[i]);     
                                }
                            }                                                 
                        }
                        // sensor data update interval
                        System.Threading.Thread.Sleep(item.CommunicationParam.IntervalTime.Value);                        
                    }
                    prevCommState = curCommState;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }
        #endregion

        #region <remarks> Parameter Methods </remarks>
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new SensorParameters();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(SensorParameters));

                if (RetVal == EventCodeEnum.NONE)
                {
                    SensorParams = tmpParam as SensorParameters;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        /// <summary>
        /// 추후에 alarm 이 발생한 후에 이어서 할 동작에 대한 내용
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum LoadAferErrorBehaviorParams()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new AfterErrorBehaviorParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                RetVal = this.LoadParameter(ref tmpParam, typeof(AfterErrorBehaviorParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    AfterErrorBehaviorParam = tmpParam as AfterErrorBehaviorParam;
                    AfterErrorBehaviorList = AfterErrorBehaviorParam.AfterErrorBehaviors;
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
                RetVal = this.SaveParameter(_SensorParams);
                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[EnvMonitoring] SaveSysParam(): Serialize Error");
                    return RetVal;
                }
                else
                {
                    RetVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }
            return RetVal;
        }
        #endregion

        #region <remarks> Functions </remarks>                     
        /// <summary>
        /// EAP 에 보고하기 (SVID)
        /// </summary>
        /// <param name="sensorModule"></param>         

        /// <summary>
        /// sensor alarm 발생했는지 확인하는 함수
        /// </summary>
        /// <param name="sensorModule"></param>
        public void AlarmNotifyStatus(ISensorModule sensorModule)
        {
            try
            {
                if (sensorModule == null)
                    return;

                if (sensorModule.SensorIndex < 1)
                    return;

                //Sensor의 ErrorReason을 보고 Notify 한다.               
                if (sensorModule.SensorInfo is SmokeSensorInfo)
                {
                    if (sensorModule.ErrorReasons.Count() != 0)
                    {
                        foreach (var item in sensorModule.ErrorReasons)
                        {
                            if ((SensorStatusEnum)item == SensorStatusEnum.TEMP_ALARM
                                || (SensorStatusEnum)item == SensorStatusEnum.SMOKE_DETECTED)
                            {
                                if (!sensorModule.bAlarmNotifyDone)
                                {
                                    sensorModule.bAlarmNotifyDone = true;
                                    this.NotifyManager().Notify(EventCodeEnum.SENSOR_ALARM_DETECTED);
                                }
                            }
                            else if((SensorStatusEnum)item == SensorStatusEnum.DISCONNECT_INDICATOR
                                || (SensorStatusEnum)item == SensorStatusEnum.DISCONNECT_HUB)
                            {
                                if (!sensorModule.bDisconnectNotifyDone)    // connected -> disconnected(indicator - sensor) 로 바뀌었을때, N개의 센서의 알람이 다 떠야하므로, 센서의 bool 타입으로 가야한다.  
                                {
                                    sensorModule.bDisconnectNotifyDone = true;
                                    this.NotifyManager().Notify(EventCodeEnum.SENSOR_DISCONNECTED,
                                                                message: $"Sensor #{sensorModule.SensorSysParameter.Index.Value} disconnected.\n" +
                                                                        "I lost connection with the sensor. Please check the sensor's connection status.",
                                                                sensorModule.SensorSysParameter.Index.Value);   // gem alarm 할때, index 만큼 + 해서 set 해준다.                            
                                    sensorModule.SensorEnable = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        // error reason clear
                        if(sensorModule.bAlarmNotifyDone)
                        {
                            sensorModule.bAlarmNotifyDone = false;
                            this.NotifyManager().ClearNotify(EventCodeEnum.SENSOR_ALARM_DETECTED);
                        }
                        if(sensorModule.bDisconnectNotifyDone)
                        {
                            sensorModule.bDisconnectNotifyDone = false;
                            this.NotifyManager().ClearNotify(EventCodeEnum.SENSOR_DISCONNECTED);
                        }
                    }                                                       
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int GetSensorMaxCount()
        {
            return SensorParams?.SensorMaxCount?.Value ?? -1;
        }
        #endregion
    }

}
