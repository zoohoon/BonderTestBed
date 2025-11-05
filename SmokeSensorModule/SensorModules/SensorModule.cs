using EnvMonitoring.ProtocolModules;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.EnvControl.Enum;
using ProberInterfaces.State;
using ProberInterfaces.Temperature.EnvMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EnvMonitoring
{
    public class SensorModule : ISensorModule, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
        
        public SensorModule(SensorSysParameter sysparam, SensorInfo sensorInfo)
        {
            SensorSysParameter = sysparam;
            SensorInfo = sensorInfo;
        }

        public event SensorTempChangedEvent TempChangedEvent;
        public event SensorStatusChangedEvent StatusChangedEvent;
        public event SensorRequestDataEvent RequestDataEvent;
        public bool Initialized { get; set; } = false;
        public byte[] InputBuff { get; set; }
        public Boolean bRcvDone { get; set; } = false;        

        private bool _SensorEnable = false;
        public bool SensorEnable
        {
            get { return _SensorEnable; }
            set
            {
                if (value != _SensorEnable)
                {
                    _SensorEnable = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _SensorIndex;
        public int SensorIndex
        {
            get { return _SensorIndex; }
            set
            {
                if (value != _SensorIndex)
                {
                    _SensorIndex = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _SensorAlias;
        public string SensorAlias
        {
            get { return _SensorAlias; }
            set
            {
                if (value != _SensorAlias)
                {
                    _SensorAlias = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private SensorStatusEnum _SensorStatus;
        public SensorStatusEnum SensorStatus
        {
            get { return _SensorStatus; }
            set
            {
                if (value != _SensorStatus)
                {
                    _SensorStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private SensorStatusEnum PrevSensorStatus;
        private double PrevSensorTemp;

        private GEMSensorStatusEnum _GEMSensorStatus;
        public GEMSensorStatusEnum GEMSensorStatus
        {
            get { return _GEMSensorStatus; }
            set
            {
                if (value != _GEMSensorStatus)
                {
                    _GEMSensorStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _bDisconnectNotifyDone = false;        
        public bool bDisconnectNotifyDone
        {
            get { return _bDisconnectNotifyDone; }
            set
            {
                if (value != _bDisconnectNotifyDone)
                {
                    _bDisconnectNotifyDone = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _bAlarmNotifyDone = false;
        public bool bAlarmNotifyDone
        {
            get { return _bAlarmNotifyDone; }
            set
            {
                if (value != _bAlarmNotifyDone)
                {
                    _bAlarmNotifyDone = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private SensorInfo _SensorInfo;
        public SensorInfo SensorInfo
        {
            get { return _SensorInfo; }
            set
            {
                if (value != _SensorInfo)
                {
                    _SensorInfo = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private List<SensorStatusEnum> _ErrorReasons
            = new List<SensorStatusEnum>();        
        public List<SensorStatusEnum> ErrorReasons
        {
            get { return _ErrorReasons; }
            set
            {
                if (value != _ErrorReasons)
                {
                    _ErrorReasons = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private SensorSysParameter _SensorSysParameter;
        public SensorSysParameter SensorSysParameter
        {
            get { return _SensorSysParameter; }
            set
            {
                if (value != _SensorSysParameter)
                {
                    _SensorSysParameter = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private IProtocolModule _ProtocolModule;
        public IProtocolModule ProtocolModule
        {
            get { return _ProtocolModule; }
            private set { _ProtocolModule = value; }
        }       

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;            
            try
            {
                if (Initialized == false)
                {
                    //_EnvState = new EnvMonitoringIdleState(this);
                    if (SensorSysParameter != null)
                    {
                        // Protocol type 비교
                        if (SensorSysParameter.ProtocolType.Value == "ONOFF")
                        {
                            ProtocolModule = new OnOffSystem(this);
                            ProtocolModule.BuffInit();
                        }
                        else if (SensorSysParameter.ProtocolType.Value == "MODBUS")
                        {

                        }
                        SensorEnable = SensorSysParameter.ModuleEnable.Value;
                        SensorIndex = SensorSysParameter.Index.Value;
                        SensorAlias = SensorSysParameter.Sensor.SensorAlias.Value;
                    }
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
            throw new NotImplementedException();
        }        
        /// <summary>
        /// Protocol module 을 거쳐 send data 하기 위한 함수
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="envMonitoringHub"></param>
        public EventCodeEnum RequestData(int idx)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // 프로토콜 방식 선택
                if (ProtocolModule is OnOffSystem)
                {
                    retVal = ProtocolModule.VerifyData(idx, ProtocolModule.Sendbuff);
                    if(retVal == EventCodeEnum.NONE)
                    {
                        RequestDataEvent(ProtocolModule.Sendbuff, 0, ProtocolModule.Sendbuff.Length);                        
                    }                    
                }
                else
                {
                    // Modbus
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        
        /// <summary>
        /// Protocol module 에서 파싱한 데이터를 바탕으로 sensor module 스스로 데이터를 업데이트해주기위한 함수
        /// </summary>
        public void UpdateSensorModule()
        {
            try
            {                
                ProtocolModule.ParseData(InputBuff, this.SensorInfo);
                bRcvDone = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// receiveData 의 index 에 맞는 센서의 데이터를 버퍼에 넣어주기 위한 함수
        /// </summary>
        /// <param name="receiveData"></param>
        /// <returns></returns>
        public EventCodeEnum FillInputBuff(byte[] receiveData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int idx = -1;
                if (receiveData.Length == 0)
                    return retVal;

                idx = ProtocolModule.GetDataIdFunc(receiveData);
                if (idx != -1)
                {
                    if (SensorIndex == idx)
                    {
                        InputBuff = receiveData;
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.INVALID_ACCESS;
                    }
                }                                                     
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// parse data 를 받아서 sensor module 이 가지고 있는 sensor info 에 업데이트 하기
        /// </summary>
        /// <param name="sensorInfo"></param>
        public void UpdateSensorInfo(ISensorInfo sensorInfo)
        {
            try
            {
                if (sensorInfo is SmokeSensorInfo)
                {
                    SmokeSensorInfo smokeSensor = sensorInfo as SmokeSensorInfo;
                    SensorInfo = smokeSensor;
                }
            }
            catch (Exception err )
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// ErrorReasons 보고 현재 센서 상태 업데이트 하기
        /// </summary>
        private void ConfirmSensorStatus()
        {
            try
            {
                SmokeSensorInfo smokeSensor = SensorInfo as SmokeSensorInfo;
                if (smokeSensor.bTemp_Warning.Value)
                {
                    SensorStatus = SensorStatusEnum.TEMP_WARN;
                    if (ErrorReasons.Contains(SensorStatusEnum.TEMP_WARN) == false)
                        ErrorReasons.Add(SensorStatusEnum.TEMP_WARN);
                }
                else
                {
                    if (ErrorReasons.Contains(SensorStatusEnum.TEMP_WARN) == true)
                        ErrorReasons.Remove(SensorStatusEnum.TEMP_WARN);
                }

                if (smokeSensor.bTemp_Alarm.Value)
                {
                    SensorStatus = SensorStatusEnum.TEMP_ALARM;
                    if (ErrorReasons.Contains(SensorStatusEnum.TEMP_ALARM) == false)
                        ErrorReasons.Add(SensorStatusEnum.TEMP_ALARM);
                }
                else
                {
                    if (ErrorReasons.Contains(SensorStatusEnum.TEMP_ALARM) == true)
                        ErrorReasons.Remove(SensorStatusEnum.TEMP_ALARM);
                }

                if (smokeSensor.bSmoke_Detect.Value)
                {
                    SensorStatus = SensorStatusEnum.SMOKE_DETECTED;
                    if (ErrorReasons.Contains(SensorStatusEnum.SMOKE_DETECTED) == false)
                        ErrorReasons.Add(SensorStatusEnum.SMOKE_DETECTED);
                }
                else
                {
                    if (ErrorReasons.Contains(SensorStatusEnum.SMOKE_DETECTED) == true)
                        ErrorReasons.Remove(SensorStatusEnum.SMOKE_DETECTED);
                }

                if (smokeSensor.bDisconnect_Sensor.Value)
                {
                    SensorStatus = SensorStatusEnum.DISCONNECT_INDICATOR;
                    GEMSensorStatus = GEMSensorStatusEnum.DISCONNECTED;
                    if (ErrorReasons.Contains(SensorStatusEnum.DISCONNECT_INDICATOR) == false)
                        ErrorReasons.Add(SensorStatusEnum.DISCONNECT_INDICATOR);
                }
                else
                {
                    if (ErrorReasons.Contains(SensorStatusEnum.DISCONNECT_INDICATOR) == true)
                        ErrorReasons.Remove(SensorStatusEnum.DISCONNECT_INDICATOR);
                }

                if (!smokeSensor.bTemp_Warning.Value && !smokeSensor.bTemp_Alarm.Value && 
                    !smokeSensor.bSmoke_Detect.Value && !smokeSensor.bDisconnect_Sensor.Value)
                {
                    SensorStatus = SensorStatusEnum.NORMAL;
                    SensorEnable = true;
                }

                if (PrevSensorStatus != SensorStatus)
                {
                    if (StatusChangedEvent != null)
                    {
                        if (SensorStatus == SensorStatusEnum.DISCONNECT_INDICATOR
                            || SensorStatus == SensorStatusEnum.DISCONNECT_HUB)
                        {
                            SensorEnable = false;
                            StatusChangedEvent(SensorIndex, GEMSensorStatusEnum.DISCONNECTED);
                        }
                        else if (SensorStatus == SensorStatusEnum.TEMP_ALARM
                            || SensorStatus == SensorStatusEnum.SMOKE_DETECTED)
                        {
                            SensorEnable = true;
                            StatusChangedEvent(SensorIndex, GEMSensorStatusEnum.ALARM);
                        }
                        else
                        {
                            SensorEnable = true;
                            StatusChangedEvent(SensorIndex, GEMSensorStatusEnum.NORMAL);
                            LoggerManager.Debug($"Smoke sensor onoffsystem normal status." +
                                        $"Sensor Index: {SensorIndex}, " +
                                        $"Temp: {smokeSensor.CurTemp.Value,2:0.00}, " +
                                        $"Status: {SensorStatus.ToString()}");
                        }
                    }
                }

                if (PrevSensorTemp != smokeSensor.CurTemp.Value)
                {
                    if (TempChangedEvent != null)
                    {
                        TempChangedEvent(SensorIndex, smokeSensor.CurTemp.Value);
                    }
                }

                PrevSensorStatus = SensorStatus;
                PrevSensorTemp = smokeSensor.CurTemp.Value;
                
                LoggerManager.SmokeSensorLog($"Sensor Index: {SensorIndex}, " +
                                             $"Temp: {smokeSensor.CurTemp.Value,2:0.00}, " +
                                             $"Status: {SensorStatus.ToString()}");
            }            
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #region <remarks> IStateModule Function </remarks>
        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                //EventCodeEnum retVal = InnerState.Execute();
                WaitRequestData();
                ConfirmSensorStatus();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stat;
        }
        #endregion
        
        private EventCodeEnum WaitRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.bRcvDone = false;
                int timeout = 1;

                retVal = RequestData(SensorIndex);

                Stopwatch ts = new Stopwatch();
                ts.Start();     

                while (bRcvDone == false)
                {
                    /// 여기서 무한으로 로그 찍는다. 
                    if (ts.Elapsed.Seconds > timeout)
                    {
                        // 1sec. Time-out"                                
                        //LoggerManager.Debug($"The request for data has timed out.");
                    }
                }
                ts.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }
            return retVal;
        }             
    }
}
