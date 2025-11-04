using System;

namespace TouchSensorObject
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using SubstrateObjects;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using TouchSensorSystemParameter;

    public class TouchSensorObject : INotifyPropertyChanged, IFactoryModule , ITouchSensorObject, IParamNode, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
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

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        //IParam
        //private IParam _TouchSensorParam_IParam;
        [ParamIgnore]
        public IParam TouchSensorParam_IParam
        {
            get  { return (IParam)TouchSensorParam; }
            set
            {
                TouchSensorParam = (TouchSensorSysParameter)value;
            }
        }

        //TouchSensorSetupParameter
        private TouchSensorSysParameter _TouchSensorParam;
        [ParamIgnore]
        public TouchSensorSysParameter TouchSensorParam
        {
            get { return _TouchSensorParam; }
            set
            {
                if (value != _TouchSensorParam)
                {
                    _TouchSensorParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = (this.TouchSensorParam == null) ? new TouchSensorSysParameter() : this.TouchSensorParam;
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(TouchSensorSysParameter)); 

                if (retval == EventCodeEnum.NONE)
                {
                    TouchSensorParam_IParam = tmpParam;
                    this.TouchSensorParam = TouchSensorParam_IParam as TouchSensorSysParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {
                ret = this.SaveParameter(TouchSensorParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public bool IsInitialized { get; set; } = false;
        public EventCodeEnum Init()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {

                if (!IsInitialized)
                {
                    IsInitialized = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        // 터치센서 등록이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private bool _TouchSensorRegistered = new bool();
        public bool TouchSensorRegistered
        {
            get { return TouchSensorParam.TouchSensorRegistered.Value; }
            set
            {
                if (value != _TouchSensorRegistered)
                {
                    TouchSensorParam.TouchSensorRegistered.Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 터치센서 베이스 등록이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private bool _TouchSensorBaseRegistered = new bool();
        public bool TouchSensorBaseRegistered
        {
            get { return TouchSensorParam.TouchSensorBaseRegistered.Value; }
            set
            {
                if (value != _TouchSensorBaseRegistered)
                {
                    TouchSensorParam.TouchSensorBaseRegistered.Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 클린 패드 베이스(터치 센서 옵셋을 구하기 위해 테스트로 찍어 보는 곳) 등록이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private bool _TouchSensorPadBaseRegistered = new bool();
        public bool TouchSensorPadBaseRegistered
        {
            get { return TouchSensorParam.TouchSensorPadBaseRegistered.Value; }
            set
            {
                if (value != _TouchSensorPadBaseRegistered)
                {
                    TouchSensorParam.TouchSensorPadBaseRegistered.Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 터치센서 옵셋 계산 과정이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private bool _TouchSensorOffsetRegistered = new bool();
        public bool TouchSensorOffsetRegistered
        {
            get { return TouchSensorParam.TouchSensorOffsetRegistered.Value; }
            set
            {
                if (value != _TouchSensorOffsetRegistered)
                {
                    TouchSensorParam.TouchSensorOffsetRegistered.Value = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool IsReadyToTouchSensor()
        {
            try
            {
                if (TouchSensorRegistered != true || TouchSensorBaseRegistered != true ||
                    TouchSensorPadBaseRegistered != true || TouchSensorOffsetRegistered != true)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        //PW Module Flag ADD ? 

        public Element<PinCoordinate> SensorPos
        {
            get { return TouchSensorParam.SensorPos; }
        }

        public Element<PinCoordinate> SensorFocusedPos
        {
            get { return TouchSensorParam.SensorFocusedPos; }
        }

        public Element<PinCoordinate> SensorBasePos
        {
            get { return TouchSensorParam.SensorBasePos; }
        }

        public Element<NCCoordinate> SensingPadBasePos
        {
            get { return TouchSensorParam.SensingPadBasePos; }
        }

    }
}
