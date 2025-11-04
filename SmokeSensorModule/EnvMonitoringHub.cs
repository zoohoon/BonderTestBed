using EnvMonitoring.ProtocolModules;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
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
using System.Text;
using System.Threading.Tasks;

namespace EnvMonitoring
{
    public class EnvMonitoringHub : IEnvMonitoringHub, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        private CommunicationParameterBase _CommunicationParam;

        public CommunicationParameterBase CommunicationParam
        {
            get { return _CommunicationParam; }
            set { _CommunicationParam = value; }            
        }

        private IByteCommModule _CommModule;
        public IByteCommModule CommModule
        {
            get { return _CommModule; }
            set { _CommModule = value; }
        }
    
        public EnvMonitoringHub(CommunicationParameterBase communicationParam, List<ISensorModule> sensorModules)
        {
            CommunicationParam = communicationParam;
            SensorModules = sensorModules;
        }       

        public bool Initialized { get; set; } = false;

        #region <remarks> Init & DeInit </remarks>
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            
            try
            {
                if (Initialized == false)
                {                    
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
        #endregion                        

        /// <summary>
        /// Hub 에 통신 연결하기
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum Commmunication()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                // commtype 비교  // 허브에 통신 연결하기
                switch (CommunicationParam.ModuleCommType.Value)
                {
                    case EnumCommmunicationType.EMUL:
                        break;
                    case EnumCommmunicationType.SERIAL:
                        if (CommModule == null)
                        {
                            CommModule = new EnvSerialCommModule();
                            retval = CommModule.InitModule();
                            if (CommModule.CurState != EnumCommunicationState.CONNECTED)
                            {
                                if (retval == EventCodeEnum.NONE)
                                {
                                    retval = CommModule.Connect();
                                    if (CommModule.CurState == EnumCommunicationState.CONNECTED)
                                    {                                        
                                        CommModule.SetDataChangedByte += new setDataHandlerforByte(ReceivedData);
                                        LoggerManager.Debug($"SensorModule.InitModule() Connect Success");
                                    }
                                    else
                                    {
                                        // 프로그램 킬때부터 센서와 연결 안됨.  
                                        CommModule.DisConnect();
                                        LoggerManager.Debug($"SensorModule.InitModule() Connect Fail");
                                    }
                                }
                            }
                        }
                        break;
                    case EnumCommmunicationType.TCPIP:
                        break;
                    case EnumCommmunicationType.RS232:
                        break;
                    case EnumCommmunicationType.USB:
                        break;
                    case EnumCommmunicationType.MODBUS:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        /// <summary>
        /// Received data 를 바탕으로 sensor module 정보 업데이트
        /// </summary>
        /// <param name="receiveData"></param>
        public void ReceivedData(byte[] receiveData)
        {
            try
            {                       
                foreach (var sensor in SensorModules)
                {                    
                    // ※ CR+LF 의 경우 END_CODE 가 USED 설정 시 추가로 송신됨 => 고려해야함.
                    if (receiveData.Length != 24 && receiveData.Length != 26)
                        return;
                    
                    if (sensor.FillInputBuff(receiveData) == EventCodeEnum.NONE)
                    {                        
                        sensor.UpdateSensorModule();
                        break;
                    }                    
                }                     
            }
            catch (Exception err )
            {
                LoggerManager.Exception(err);                
            }
        }

        public  void Send(byte[] buffer, int offset, int count)
        {
            try
            {
                CommModule.Send(buffer, offset, count);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        
    }
}
