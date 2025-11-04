using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.EnvControl.Enum;
using ProberInterfaces.State;
using ProberInterfaces.Temperature.EnvMonitoring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EnvMonitoring
{
    public class EmulSensorModule : INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        public bool Initialized { get; set; } = false;
        public Boolean bRcvDone { get; set; } = false;

        private List<SensorStatusEnum> _ErrorReasons = new List<SensorStatusEnum>();
        public List<SensorStatusEnum> ErrorReasons
        {
            get { return _ErrorReasons; }
            set { _ErrorReasons = value; }
        }

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

        private bool _bDisconnectNotifyDone;
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
            set
            {
                if (value != _ProtocolModule)
                {
                    _ProtocolModule = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public EmulSensorModule()
        {
            
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {                    
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

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                //EventCodeEnum retVal = InnerState.Execute();                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stat;
        }

        public void UpdateSensorModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum FillInputBuff(byte[] receiveData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void UpdateSensorInfo(ISensorInfo sensorInfo)
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
