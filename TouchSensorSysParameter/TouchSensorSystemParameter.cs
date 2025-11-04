using Focusing;
using LogModule;
using NeedleCleanerModuleParameter;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.TouchSensor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace TouchSensorSystemParameter
{
    [Serializable]
    public class TouchSensorSysParameter : ISystemParameterizable, INotifyPropertyChanged, IParamNode, ITouchSensorSysParam
    {
        public TouchSensorSysParameter()
        {

        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ParamNode
        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "";

        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "TouchSensorSetupSysParameter.json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        private string _Genealogy;
        [XmlIgnore, JsonIgnore]
        public string Genealogy
        {
            get { return _Genealogy; }
            set { _Genealogy = value; }
        }

        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore]
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
        public List<object> Nodes { get; set; }

        #endregion

        #region SystemParameterize
        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.PIN_HIGH_CAM, EnumAxisConstants.PZ, FocusParam, 1000);
                }

                if (FocusParamForPadBase == null)
                {
                    FocusParamForPadBase = new NormalFocusParameter();
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.PZ, FocusParamForPadBase, 1000);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SetDefaultParam();
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }

        public void SetElementMetaData()
        {
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                TouchSensorAttached.Value = false;

                TouchSensorBaseRegistered.Value = false;
                TouchSensorOffsetRegistered.Value = false;
                TouchSensorRegistered.Value = false;
                TouchSensorPadBaseRegistered.Value = false;

                SensorPos.Value = new PinCoordinate();
                SensorPos.Value.X.Value = 0;
                SensorPos.Value.Y.Value = 0;
                SensorPos.Value.Z.Value = -63500;

                SensorFocusedPos.Value = new PinCoordinate();
                SensorBasePos.Value = new PinCoordinate();
                SensorBaseOffset.Value = new PinCoordinate();
                SensingPadBasePos.Value = new NCCoordinate();

                SensorBasePos.Value.X.Value = 0;
                SensorBasePos.Value.Y.Value = 0;
                SensorBasePos.Value.Z.Value = -63500;

                SensorFocusedPos.Value.X.Value = 0;
                SensorFocusedPos.Value.Y.Value = 0;
                SensorFocusedPos.Value.Z.Value = -63500;

                SensorAutoDetectZCleareance.Value = 500;
                TouchSensorRegMax.Value = -45000;
                TouchSensorRegMin.Value = -70000;

                if (SensorPos.Value.Z.Value >= 0)
                    SensorPos.Value.Z.Value = -63500;

                if (SensorFocusedPos.Value.Z.Value >= 0)
                    SensorFocusedPos.Value.Z.Value = -63500;

                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.PIN_HIGH_CAM, EnumAxisConstants.PZ, FocusParam, 500);
                    FocusParam.OutFocusLimit.Value = 20;
                    FocusParam.FlatnessThreshold.Value = 60;
                }

                if (FocusParamForPadBase == null)
                {
                    FocusParamForPadBase = new NormalFocusParameter();
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.PZ, FocusParamForPadBase, 500);
                    FocusParamForPadBase.OutFocusLimit.Value = 20;
                    FocusParamForPadBase.FlatnessThreshold.Value = 60;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }
        #endregion

        private Element<bool> _TouchSensorAttached = new Element<bool>();
        public Element<bool> TouchSensorAttached
        {
            get { return _TouchSensorAttached; }
            set
            {
                if (value != _TouchSensorAttached)
                {
                    _TouchSensorAttached = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Register Touch Sensor
        // 핀 카메라로 확인한 센서의 위치 (실제 감지되는 높이보다 항상 아래에 존재한다. 약 -30 ~ -40 마이크론 아래)
        // 대략적인 센서의 위치를 알기 위한 용도로만 사용되며 실제 클리닝 높이 계산 과정에서는 사용되지 않는다.
        private Element<PinCoordinate> _SensorFocusedPos = new Element<PinCoordinate>();
        public Element<PinCoordinate> SensorFocusedPos
        {
            get { return _SensorFocusedPos; }
            set
            {
                if (value != _SensorFocusedPos)
                {
                    _SensorFocusedPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 터치센서 포커싱을 위한 밝기 값
        private List<LightValueParam> _LightForFocusSensor = new List<LightValueParam>();
        public List<LightValueParam> LightForFocusSensor
        {
            get { return _LightForFocusSensor; }
            set
            {
                if (value != _LightForFocusSensor)
                {
                    _LightForFocusSensor = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 터치센서 등록이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private Element<bool> _TouchSensorRegistered = new Element<bool>();
        public Element<bool> TouchSensorRegistered
        {
            get { return _TouchSensorRegistered; }
            set
            {
                if (value != _TouchSensorRegistered)
                {
                    _TouchSensorRegistered = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 터치센서 포커싱 용 파라미터. 카메라 타입은 핀 하이 고정
        private FocusParameter _FocusParam;
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set
            {
                if (value != _FocusParam)
                {
                    _FocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region Register Sensor Base
        // 핀 카메라로 확인한 센서 베이스의 위치 
        // 센서 끝면의 포커싱 위치는 매번 불규칙적이다. 따라서 센서의 끝점이 아닌 센서 주변에 고정된 부위를 포커싱하여 그 지점으로 부터
        // 센서 끝면 까지의 상대거리를 기억하여 사용한다.
        // 등록 과정이 아닌 상황에서 센서의 위치를 확인하는 경우, 센서를 직접 포커싱 하지 않고 이 지점의 높이를 확인한다.
        private Element<PinCoordinate> _SensorBasePos = new Element<PinCoordinate>();
        public Element<PinCoordinate> SensorBasePos
        {
            get { return _SensorBasePos; }
            set
            {
                if (value != _SensorBasePos)
                {
                    _SensorBasePos = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 터치센서 베이스 포커싱을 위한 밝기 값
        private List<LightValueParam> _LightForBaseFocusSensor = new List<LightValueParam>();
        public List<LightValueParam> LightForBaseFocusSensor
        {
            get { return _LightForBaseFocusSensor; }
            set
            {
                if (value != _LightForBaseFocusSensor)
                {
                    _LightForBaseFocusSensor = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 터치센서 베이스 등록이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private Element<bool> _TouchSensorBaseRegistered = new Element<bool>();
        public Element<bool> TouchSensorBaseRegistered
        {
            get { return _TouchSensorBaseRegistered; }
            set
            {
                if (value != _TouchSensorBaseRegistered)
                {
                    _TouchSensorBaseRegistered = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Register Sensor Trial Pos

        // 센서가 감지되는 원점 높이를 알아내기 위해 확인 차 찍어보는 위치
        // 우선 이 위치를 웨이퍼 카메라로 포커싱 한 후, 이 위치에서 터치 센서를 감지시켜 얼마나 
        // 오차가 발생하는지 확인한 뒤 해당 오차 만큼의 옵셋을 센서 베이스와 센서의 포커싱 결과 높이의 상관 관계에 더해 준다.
        private Element<NCCoordinate> _SensingPadBasePos = new Element<NCCoordinate>();
        public Element<NCCoordinate> SensingPadBasePos
        {
            get { return _SensingPadBasePos; }
            set
            {
                if (value != _SensingPadBasePos)
                {
                    _SensingPadBasePos = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 클린패드 베이스 포커싱을 위한 밝기 값
        private List<LightValueParam> _LightForPadBaseFocusSensor = new List<LightValueParam>();
        public List<LightValueParam> LightForPadBaseFocusSensor
        {
            get { return _LightForPadBaseFocusSensor; }
            set
            {
                if (value != _LightForPadBaseFocusSensor)
                {
                    _LightForPadBaseFocusSensor = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 클린 패드 베이스(터치 센서 옵셋을 구하기 위해 테스트로 찍어 보는 곳) 등록이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private Element<bool> _TouchSensorPadBaseRegistered = new Element<bool>();
        public Element<bool> TouchSensorPadBaseRegistered
        {
            get { return _TouchSensorPadBaseRegistered; }
            set
            {
                if (value != _TouchSensorPadBaseRegistered)
                {
                    _TouchSensorPadBaseRegistered = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 패드 베이스 포커싱용 파라미터. 카메라 타입은 웨이퍼 하이 고정
        private FocusParameter _FocusParamForPadBase;
        public FocusParameter FocusParamForPadBase
        {
            get { return _FocusParamForPadBase; }
            set
            {
                if (value != _FocusParamForPadBase)
                {
                    _FocusParamForPadBase = value;
                    RaisePropertyChanged();
                }
            }

        }

        #endregion

        #region Calculate Sensor Offset
        // 센서 베이스에서 실제 감지되는 위치까지의 상대거리 
        // 센서 베이스를 포커싱 한 후, 이 값을 더해서 최종 센서의 위치를 계산한다.
        private Element<PinCoordinate> _SensorBaseOffset = new Element<PinCoordinate>();
        public Element<PinCoordinate> SensorBaseOffset
        {
            get { return _SensorBaseOffset; }
            set
            {
                if (value != _SensorBaseOffset)
                {
                    _SensorBaseOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 실제로 감지되는 높이 (계산으로 구해지는 값)
        private Element<PinCoordinate> _SensorPos = new Element<PinCoordinate>();
        public Element<PinCoordinate> SensorPos
        {
            get { return _SensorPos; }
            set
            {
                if (value != _SensorPos)
                {
                    _SensorPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        // 포커싱으로 확인한 센서 끝면의 위치와 실제 감지된 센서 높이와의 오차.
        // 즉, 물리적으로 센서 끝면이 패드에 닿은 순간부터 실제로 센서가 동작하기까지 얼마나 센서가 더 눌려야 하느냐를 가리키는 값.
        private Element<double> _TouchSensorOffset = new Element<double>();
        public Element<double> TouchSensorOffset
        {
            get { return _TouchSensorOffset; }
            set
            {
                if (value != _TouchSensorOffset)
                {
                    _TouchSensorOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        // 터치센서 옵셋 계산 과정이 완료 되었나 안 되었나.
        // 클린패드 교체 후 초기화 할 것.
        private Element<bool> _TouchSensorOffsetRegistered = new Element<bool>();
        public Element<bool> TouchSensorOffsetRegistered
        {
            get { return _TouchSensorOffsetRegistered; }
            set
            {
                if (value != _TouchSensorOffsetRegistered)
                {
                    _TouchSensorOffsetRegistered = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        // 머신 이닛 후 1회에 한 해 자동으로 터치센서 베이스의 높이를 확인하여 높이 변화가 있을 경우 보상한다. 안전을 위해 포커싱 레인지는 150으로 고정.
        private bool _FlagTouchSensorBaseConfirmed = new bool();
        public bool FlagTouchSensorBaseConfirmed
        {
            get { return _FlagTouchSensorBaseConfirmed; }
            set
            {
                if (value != _FlagTouchSensorBaseConfirmed)
                {
                    _FlagTouchSensorBaseConfirmed = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SensorAutoDetectZCleareance = new Element<double>();
        public Element<double> SensorAutoDetectZCleareance
        {
            get { return _SensorAutoDetectZCleareance; }
            set
            {
                if (value != _SensorAutoDetectZCleareance)
                {
                    _SensorAutoDetectZCleareance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TouchSensorRegMax
             = new Element<double>();
        public Element<double> TouchSensorRegMax
        {
            get { return _TouchSensorRegMax; }
            set
            {
                if (value != _TouchSensorRegMax)
                {
                    _TouchSensorRegMax = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TouchSensorRegMin
             = new Element<double>();
        public Element<double> TouchSensorRegMin
        {
            get { return _TouchSensorRegMin; }
            set
            {
                if (value != _TouchSensorRegMin)
                {
                    _TouchSensorRegMin = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        public bool IsReadyToTouchSensor()
        {
            try
            {
                if (TouchSensorRegistered.Value != true || TouchSensorBaseRegistered.Value != true ||
                    TouchSensorPadBaseRegistered.Value != true || TouchSensorOffsetRegistered.Value != true)
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

    }
}

