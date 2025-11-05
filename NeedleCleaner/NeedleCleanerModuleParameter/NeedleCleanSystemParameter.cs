using Focusing;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;

namespace NeedleCleanerModuleParameter
{
    [Serializable]
    public class NeedleCleanSystemParameter :
        ISystemParameterizable,
        INotifyPropertyChanged,
        INCSysParam, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public NeedleCleanSystemParameter()
        {
        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "NeedleCleanModule";
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "NeedleCleanSysParameter.json";
        private string _Genealogy;
        [XmlIgnore, JsonIgnore]
        public string Genealogy
        {
            get { return _Genealogy; }
            set { _Genealogy = value; }
        }

        private List<NCSheetDefinition> _SheetDefs = new List<NCSheetDefinition>();
        public List<NCSheetDefinition> SheetDefs
        {
            get { return _SheetDefs; }
            set
            {
                if (value != _SheetDefs)
                {
                    _SheetDefs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ManualNcDefinition _ManualNC = new ManualNcDefinition();
        public ManualNcDefinition ManualNC
        {
            get { return _ManualNC; }
            set
            {
                if (value != _ManualNC)
                {
                    _ManualNC = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private NCSheetDefinition _SheetDef = new NCSheetDefinition();
        //public NCSheetDefinition SheetDef
        //{
        //    get { return _SheetDef; }
        //    set
        //    {
        //        if (value != _SheetDef)
        //        {
        //            _SheetDef = value;
        //            NotifyPropertyChanged();
        //        }
        //    }
        //}

        private Element<float> _NeedleCleanPadWidth
            = new Element<float>();
        public Element<float> NeedleCleanPadWidth
        {
            get { return _NeedleCleanPadWidth; }
            set
            {
                if (value != _NeedleCleanPadWidth)
                {
                    _NeedleCleanPadWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<float> _NeedleCleanPadHeight
            = new Element<float>();
        public Element<float> NeedleCleanPadHeight
        {
            get { return _NeedleCleanPadHeight; }
            set
            {
                if (value != _NeedleCleanPadHeight)
                {
                    _NeedleCleanPadHeight = value;
                    RaisePropertyChanged();
                }
            }
        }



        private Element<EnumAxisConstants> _AxisType = new Element<EnumAxisConstants>();
        public Element<EnumAxisConstants> AxisType
        {
            get { return _AxisType; }
            set
            {
                if (value != _AxisType)
                {
                    _AxisType = value;
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

        //  포커싱 후 높이를 비교하여 등록된 높이에 비해 어느 이상 높이 변화가 발생하면 에러 발생. (초기값 150마이크론)
        private Element<double> _CleanSheetFocusThreshold = new Element<double>();
        public Element<double> CleanSheetFocusThreshold
        {
            get { return _CleanSheetFocusThreshold; }
            set
            {
                if (value != _CleanSheetFocusThreshold)
                {
                    _CleanSheetFocusThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        //  평탄도 리미트 (초기값 20마이크론)
        private Element<double> _CleanSheetPlanarityLimit = new Element<double>();
        public Element<double> CleanSheetPlanarityLimit
        {
            get { return _CleanSheetPlanarityLimit; }
            set
            {
                if (value != _CleanSheetPlanarityLimit)
                {
                    _CleanSheetPlanarityLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private Element<bool> _CleanUnitAttached = new Element<bool>();
        public Element<bool> CleanUnitAttached
        {
            get { return _CleanUnitAttached; }
            set
            {
                if (value != _CleanUnitAttached)
                {
                    _CleanUnitAttached = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<NC_MachineType> _NC_TYPE = new Element<NC_MachineType>();
        public Element<NC_MachineType> NC_TYPE
        {
            get { return _NC_TYPE; }
            set
            {
                if (value != _NC_TYPE)
                {
                    _NC_TYPE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CleanUnitDownPos = new Element<double>();
        public Element<double> CleanUnitDownPos
        {   // Decide clean unit down position(safe height, default = -38000)
            get { return _CleanUnitDownPos; }
            set
            {
                if (value != _CleanUnitDownPos)
                {
                    _CleanUnitDownPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 클린 패드 영역 분할 최대 갯수 (기본값 3개)
        private Element<int> _MaxCleanPadNum = new Element<int>();
        public Element<int> MaxCleanPadNum
        {
            get { return _MaxCleanPadNum; }
            set
            {
                if (value != _MaxCleanPadNum)
                {
                    _MaxCleanPadNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 클린 패드 교체 위치
        private Element<double> _CleanPadChangePosX = new Element<double>();
        public Element<double> CleanPadChangePosX
        {
            get { return _CleanPadChangePosX; }
            set
            {
                if (value != _CleanPadChangePosX)
                {
                    _CleanPadChangePosX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CleanPadChangePosY = new Element<double>();
        public Element<double> CleanPadChangePosY
        {
            get { return _CleanPadChangePosY; }
            set
            {
                if (value != _CleanPadChangePosY)
                {
                    _CleanPadChangePosY = value;
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

        // 동일한 OD를 적용시켰을 때 물리적인 프로빙 OD와 클리닝 OD의 편차 옵셋
        // 이 값에 들어가 있는 값 만큼 그대로 클리닝 OD에 더해준다.
        private Element<double> _CleaningOverdriveOffset = new Element<double>();
        public Element<double> CleaningOverdriveOffset
        {
            get { return _CleaningOverdriveOffset; }
            set
            {
                if (value != _CleaningOverdriveOffset)
                {
                    _CleaningOverdriveOffset = value;
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

        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
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

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    CleanUnitAttached.Value = false;
                }

                ManualNC = new ManualNcDefinition();
                ManualNC.EnableCleaning.Value = new List<bool>();
                ManualNC.EnableCleaning.Value.Add(false);
                ManualNC.EnableCleaning.Value.Add(false);
                ManualNC.EnableCleaning.Value.Add(false);

                ManualNC.Overdrive.Value = new List<double>();
                ManualNC.Overdrive.Value.Add(-200);
                ManualNC.Overdrive.Value.Add(-200);
                ManualNC.Overdrive.Value.Add(-200);

                ManualNC.ContactCount.Value = new List<int>();
                ManualNC.ContactCount.Value.Add(10);
                ManualNC.ContactCount.Value.Add(10);
                ManualNC.ContactCount.Value.Add(10);

                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                if (FocusParamForPadBase == null)
                {
                    FocusParamForPadBase = new NormalFocusParameter();
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
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                SheetDefs = new List<NCSheetDefinition>();

                SheetDefs.Add(new NCSheetDefinition());
                SheetDefs.Add(new NCSheetDefinition());
                SheetDefs.Add(new NCSheetDefinition());
                AxisType.Value = EnumAxisConstants.PZ;
                TouchSensorAttached.Value = true;
                CleanUnitAttached.Value = true;
                NC_TYPE.Value = NC_MachineType.MOTOR_NC;
                CleanUnitDownPos.Value = -80000;
                MaxCleanPadNum.Value = 3;
                CleanSheetPlanarityLimit.Value = 9999999;
                CleanSheetFocusThreshold.Value = 9999999;
                NeedleCleanPadWidth.Value = 200000;
                NeedleCleanPadHeight.Value = 100000;
                //SensorPos = new Element<NCCoordinate>();
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

                ManualNC = new ManualNcDefinition();
                ManualNC.EnableCleaning.Value = new List<bool>();
                ManualNC.EnableCleaning.Value.Add(true);
                ManualNC.EnableCleaning.Value.Add(false);
                ManualNC.EnableCleaning.Value.Add(false);

                ManualNC.Overdrive.Value = new List<double>();
                ManualNC.Overdrive.Value.Add(-200);
                ManualNC.Overdrive.Value.Add(-200);
                ManualNC.Overdrive.Value.Add(-200);

                ManualNC.ContactCount.Value = new List<int>();
                ManualNC.ContactCount.Value.Add(10);
                ManualNC.ContactCount.Value.Add(10);
                ManualNC.ContactCount.Value.Add(10);

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
                    FocusParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;
                }

                if (FocusParamForPadBase == null)
                {
                    FocusParamForPadBase = new NormalFocusParameter();
                    FocusParamForPadBase.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }

        public void CopyToSensorReg(NeedleCleanSystemParameter target)
        {
            try
            {
                target.TouchSensorRegistered.Value = this.TouchSensorRegistered.Value;

                target.SensorFocusedPos.Value = new PinCoordinate(
                this.SensorFocusedPos.Value.X.Value,
                this.SensorFocusedPos.Value.Y.Value,
                this.SensorFocusedPos.Value.Z.Value);

                target.LightForFocusSensor = new List<LightValueParam>();
                foreach (var item in this.LightForFocusSensor)
                {
                    target.LightForFocusSensor.Add(item);
                }

            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanSystemParameter - CopyToSensorReg() : Error occured.");
                throw;
            }

        }

        public void CopyToSensorBaseReg(NeedleCleanSystemParameter target)
        {
            try
            {
                target.TouchSensorBaseRegistered.Value = this.TouchSensorBaseRegistered.Value;

                target.SensorBasePos.Value = new PinCoordinate(
                this.SensorBasePos.Value.X.Value,
                this.SensorBasePos.Value.Y.Value,
                this.SensorBasePos.Value.Z.Value);

                target.LightForBaseFocusSensor = new List<LightValueParam>();
                foreach (var item in this.LightForBaseFocusSensor)
                {
                    target.LightForBaseFocusSensor.Add(item);
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanSystemParameter - CopyToSensorBaseReg() : Error occured.");
                throw;
            }

        }

        public void CopyToSensoPadRefReg(NeedleCleanSystemParameter target)
        {
            try
            {
                target.TouchSensorPadBaseRegistered.Value = this.TouchSensorPadBaseRegistered.Value;

                target.SensingPadBasePos.Value = new NCCoordinate(
                this.SensingPadBasePos.Value.X.Value,
                this.SensingPadBasePos.Value.Y.Value,
                this.SensingPadBasePos.Value.Z.Value);

                target.LightForPadBaseFocusSensor = new List<LightValueParam>();
                foreach (var item in this.LightForPadBaseFocusSensor)
                {
                    target.LightForPadBaseFocusSensor.Add(item);
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanSystemParameter - CopyToSensoPadRefReg() : Error occured.");
                throw;
            }

        }

        public void CopyToSensoOffsetReg(NeedleCleanSystemParameter target)
        {
            try
            {
                target.TouchSensorOffsetRegistered.Value = this.TouchSensorOffsetRegistered.Value;

                target.SensorPos.Value = new PinCoordinate(
                this.SensorPos.Value.X.Value,
                this.SensorPos.Value.Y.Value,
                this.SensorPos.Value.Z.Value);

            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanSystemParameter - CopyToSensoOffsetReg() : Error occured.");
                throw;
            }

        }

        public void CopyTo(NeedleCleanSystemParameter target)
        {
            try
            {
                //target = new NeedleCleanSystemParameter();
                if (target == null)
                    return;

                target.SheetDefs = new List<NCSheetDefinition>();

                foreach (var item in this.SheetDefs)
                {
                    NCSheetDefinition sheet = new NCSheetDefinition();
                    item.CopyTo(sheet);
                    target.SheetDefs.Add(sheet);
                }

                target.ManualNC = new ManualNcDefinition();

                target.ManualNC.ContactCount.Value = new List<int>();

                foreach (var item in this.ManualNC.ContactCount.Value)
                {
                    target.ManualNC.ContactCount.Value.Add(item);
                }

                target.ManualNC.EnablePinAlignAfterNC.Value = this.ManualNC.EnablePinAlignAfterNC.Value;
                target.ManualNC.EnablePinAlignBeforeNC.Value = this.ManualNC.EnablePinAlignBeforeNC.Value;

                target.ManualNC.Overdrive.Value = new List<double>();
                foreach (var item in this.ManualNC.Overdrive.Value)
                {
                    target.ManualNC.Overdrive.Value.Add(item);
                }

                target.ManualNC.EnableFocus.Value = this.ManualNC.EnableFocus.Value;

                target.ManualNC.EnableCleaning.Value = new List<bool>();
                foreach (var item in this.ManualNC.EnableCleaning.Value)
                {
                    target.ManualNC.EnableCleaning.Value.Add(item);
                }

                target.SensorPos.Value = new PinCoordinate();
                target.SensorFocusedPos.Value = new PinCoordinate();
                target.SensorBasePos.Value = new PinCoordinate();
                target.SensorBaseOffset.Value = new PinCoordinate();
                target.SensingPadBasePos.Value = new NCCoordinate();

                target._NeedleCleanPadWidth = this.NeedleCleanPadWidth;
                target.NeedleCleanPadHeight = this.NeedleCleanPadHeight;
                target.AxisType.Value = this.AxisType.Value;
                target.TouchSensorAttached.Value = this.TouchSensorAttached.Value;
                target.CleanUnitAttached.Value = this.CleanUnitAttached.Value;
                target.NC_TYPE.Value = this.NC_TYPE.Value;
                target.CleanUnitDownPos.Value = this.CleanUnitDownPos.Value;
                target.MaxCleanPadNum.Value = this.MaxCleanPadNum.Value;
                target.CleanSheetPlanarityLimit.Value = this.CleanSheetPlanarityLimit.Value;
                target.CleanSheetFocusThreshold.Value = this.CleanSheetFocusThreshold.Value;

                target.SensorPos.Value = new PinCoordinate(
                    this.SensorPos.Value.X.Value,
                    this.SensorPos.Value.Y.Value,
                    this.SensorPos.Value.Z.Value);
                target.SensorFocusedPos.Value = new PinCoordinate(
                    this.SensorFocusedPos.Value.X.Value,
                    this.SensorFocusedPos.Value.Y.Value,
                    this.SensorFocusedPos.Value.Z.Value);
                target.SensorBasePos.Value = new PinCoordinate(
                    this.SensorBasePos.Value.X.Value,
                    this.SensorBasePos.Value.Y.Value,
                    this.SensorBasePos.Value.Z.Value);
                target.SensorBaseOffset.Value = new PinCoordinate(
                    this.SensorBaseOffset.Value.X.Value,
                    this.SensorBaseOffset.Value.Y.Value,
                    this.SensorBaseOffset.Value.Z.Value);
                target.SensingPadBasePos.Value = new NCCoordinate(
                    this.SensingPadBasePos.Value.X.Value,
                    this.SensingPadBasePos.Value.Y.Value,
                    this.SensingPadBasePos.Value.Z.Value);
                target.TouchSensorOffset.Value = this._TouchSensorOffset.Value;

                target.LightForFocusSensor = new List<LightValueParam>();
                foreach (var item in this.LightForFocusSensor)
                {
                    target.LightForFocusSensor.Add(item);
                }

                target.LightForBaseFocusSensor = new List<LightValueParam>();
                foreach (var item in this.LightForBaseFocusSensor)
                {
                    target.LightForBaseFocusSensor.Add(item);
                }

                target.LightForPadBaseFocusSensor = new List<LightValueParam>();
                foreach (var item in this.LightForPadBaseFocusSensor)
                {
                    target.LightForPadBaseFocusSensor.Add(item);
                }

                target.CleanPadChangePosX.Value = this.CleanPadChangePosX.Value;
                target.CleanPadChangePosY.Value = this.CleanPadChangePosY.Value;

                target.TouchSensorRegistered.Value = this.TouchSensorRegistered.Value;
                target.TouchSensorBaseRegistered.Value = this.TouchSensorBaseRegistered.Value;
                target.TouchSensorPadBaseRegistered.Value = this.TouchSensorPadBaseRegistered.Value;
                target.TouchSensorOffsetRegistered.Value = this.TouchSensorOffsetRegistered.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                SheetDefs = new List<NCSheetDefinition>();

                SheetDefs.Add(new NCSheetDefinition());
                SheetDefs.Add(new NCSheetDefinition());
                SheetDefs.Add(new NCSheetDefinition());
                AxisType.Value = EnumAxisConstants.PZ;
                TouchSensorAttached.Value = true;
                CleanUnitAttached.Value = true;
                NC_TYPE.Value = NC_MachineType.MOTOR_NC;
                CleanUnitDownPos.Value = -80000;
                MaxCleanPadNum.Value = 3;
                CleanSheetPlanarityLimit.Value = 9999999;
                CleanSheetFocusThreshold.Value = 9999999;
                NeedleCleanPadWidth.Value = 200000;
                NeedleCleanPadHeight.Value = 100000;
                //SensorPos = new Element<NCCoordinate>();
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

                ManualNC = new ManualNcDefinition();
                ManualNC.EnableCleaning.Value = new List<bool>();
                ManualNC.EnableCleaning.Value.Add(true);
                ManualNC.EnableCleaning.Value.Add(false);
                ManualNC.EnableCleaning.Value.Add(false);

                ManualNC.Overdrive.Value = new List<double>();
                ManualNC.Overdrive.Value.Add(-200);
                ManualNC.Overdrive.Value.Add(-200);
                ManualNC.Overdrive.Value.Add(-200);

                ManualNC.ContactCount.Value = new List<int>();
                ManualNC.ContactCount.Value.Add(10);
                ManualNC.ContactCount.Value.Add(10);
                ManualNC.ContactCount.Value.Add(10);

                if (SensorPos.Value.Z.Value >= 0)
                    SensorPos.Value.Z.Value = -63500;

                if (SensorFocusedPos.Value.Z.Value >= 0)
                    SensorFocusedPos.Value.Z.Value = -63500;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }
    }

    public enum NC_MachineType
    {
        AIR_NC = 0,
        MOTOR_NC = 1
    }

    [Serializable]
    public class ManualNcDefinition : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        [NonSerialized]
        private Element<List<double>> _Overdrive = new Element<List<double>>();
        [XmlIgnore, JsonIgnore]
        public Element<List<double>> Overdrive
        {
            get { return _Overdrive; }
            set
            {
                if (value != _Overdrive)
                {
                    _Overdrive = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<List<int>> _ContactCount = new Element<List<int>>();
        [XmlIgnore, JsonIgnore]
        public Element<List<int>> ContactCount
        {
            get { return _ContactCount; }
            set
            {
                if (value != _ContactCount)
                {
                    _ContactCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<bool> _EnablePinAlignBeforeNC = new Element<bool>();
        [XmlIgnore, JsonIgnore]
        public Element<bool> EnablePinAlignBeforeNC
        {
            get { return _EnablePinAlignBeforeNC; }
            set
            {
                if (value != _EnablePinAlignBeforeNC)
                {
                    _EnablePinAlignBeforeNC = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<bool> _EnablePinAlignAfterNC = new Element<bool>();
        [XmlIgnore, JsonIgnore]
        public Element<bool> EnablePinAlignAfterNC
        {
            get { return _EnablePinAlignAfterNC; }
            set
            {
                if (value != _EnablePinAlignAfterNC)
                {
                    _EnablePinAlignAfterNC = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<bool> _EnableFocus = new Element<bool>();
        [XmlIgnore, JsonIgnore]
        public Element<bool> EnableFocus
        {
            get { return _EnableFocus; }
            set
            {
                if (value != _EnableFocus)
                {
                    _EnableFocus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Element<List<bool>> _EnableCleaning = new Element<List<bool>>();
        [XmlIgnore, JsonIgnore]
        public Element<List<bool>> EnableCleaning
        {
            get { return _EnableCleaning; }
            set
            {
                if (value != _EnableCleaning)
                {
                    _EnableCleaning = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ManualNcDefinition()
        {
            try
            {
                EnablePinAlignBeforeNC.Value = false;
                EnablePinAlignAfterNC.Value = false;
                EnableFocus.Value = true;

                EnableCleaning.Value = new List<bool>();
                EnableCleaning.Value.Add(true);
                EnableCleaning.Value.Add(false);
                EnableCleaning.Value.Add(false);

                Overdrive.Value = new List<double>();
                Overdrive.Value.Add(-200);
                Overdrive.Value.Add(-200);
                Overdrive.Value.Add(-200);

                ContactCount.Value = new List<int>();
                ContactCount.Value.Add(10);
                ContactCount.Value.Add(10);
                ContactCount.Value.Add(10);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable]
    public class NCSheetDefinition : INotifyPropertyChanged, IFactoryModule
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public void CopyTo(NCSheetDefinition target)
        {
            try
            {
                target.Offset.Value = new NCCoordinate(
                    this.Offset.Value.X.Value,
                    this.Offset.Value.Y.Value,
                    this.Offset.Value.Z.Value);
                target.Range.Value =
                    new NCCoordinate(
                        this.Range.Value.X.Value,
                        this.Range.Value.Y.Value,
                        this.Range.Value.Z.Value);
                target.Margin.Value = this.Margin.Value;
                target.MarkedDieCountVal = this.MarkedDieCountVal;
                target.MarkedWaferCountVal = this.MarkedWaferCountVal;

                target.LastCleaningPos.Value.X.Value = this.LastCleaningPos.Value.X.Value;
                target.LastCleaningPos.Value.Y.Value = this.LastCleaningPos.Value.Y.Value;
                target.LastCleaningPos.Value.Z.Value = this.LastCleaningPos.Value.Z.Value;
                target.LastStartingPos.Value = this.LastStartingPos.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public NCSheetDefinition()
        {
            try
            {
                Offset.Value = new NCCoordinate(0, 0, 0);
                Range.Value = new NCCoordinate(100000, 50000, 0);

                Margin.Value = 1000;

                LastCleaningPos.Value = new NCCoordinate();
                LastCleaningPos.Value.X.Value = 3233;
                LastCleaningPos.Value.Y.Value = 3233;
                Offset.Value = new NCCoordinate();
                Range.Value = new NCCoordinate(100000, 50000);
                LastCleaningPos.Value = new NCCoordinate();

                //CurCleaningLoc = new NCCoordinate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #region INcSystemParameters

        private Element<NCCoordinate> _Offset = new Element<NCCoordinate>();
        public Element<NCCoordinate> Offset
        {
            get { return _Offset; }
            set
            {
                if (value != _Offset)
                {
                    _Offset = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<NCCoordinate> _Range = new Element<NCCoordinate>();
        public Element<NCCoordinate> Range
        {
            get { return _Range; }
            set
            {
                if (value != _Range)
                {
                    _Range = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<double> _Margin = new Element<double>();
        public Element<double> Margin
        {
            get { return _Margin; }
            set
            {
                if (value != _Margin)
                {
                    _Margin = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion


        // 마지막 클리닝 순간부터 진행된 총 다이 갯수를 계산하기 위해서 사용. 다이인터벌 용
        // 프로빙이 될 때마다 무한히 증가하는 다이 카운트를 읽어서 클리닝이 끝날 때마다 값을 저장해 둔다.
        // 디바이스 변경 후 클리어할 것
        private long _MarkedDieCountVal = new long();
        public long MarkedDieCountVal
        {
            get { return _MarkedDieCountVal; }
            set
            {
                if (value != _MarkedDieCountVal)
                {
                    _MarkedDieCountVal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // 웨이퍼 인터벌 용 카운트.
        // 웨이퍼 한 장이 끝날 때마다 무한히 증가하는 웨이퍼 카운트를 읽어서 클리닝이 끝날 때마다 값을 저장해 둔다.
        // 디바이스 변경 후 클리어 할 것
        private long _MarkedWaferCountVal = new long();
        public long MarkedWaferCountVal
        {
            get { return _MarkedWaferCountVal; }
            set
            {
                if (value != _MarkedWaferCountVal)
                {
                    _MarkedWaferCountVal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<NCCoordinate> _LastCleaningPos = new Element<NCCoordinate>();
        public Element<NCCoordinate> LastCleaningPos
        {
            get { return _LastCleaningPos; }
            set
            {
                if (value != _LastCleaningPos)
                {
                    _LastCleaningPos = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<int> _LastStartingPos = new Element<int>();
        public Element<int> LastStartingPos
        {
            get { return _LastStartingPos; }
            set
            {
                if (value != _LastStartingPos)
                {
                    _LastStartingPos = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

    public class NCSheetVMDefinition : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        public void CopyTo(NCSheetVMDefinition target)
        {
            try
            {
                target.SheetWidth = this.SheetWidth;
                target.SheetHeight = this.SheetHeight;
                target.SheetLeft = this.SheetLeft;
                target.SheetTop = this.SheetTop;
                target.Index = this.Index;
                target.Thickness = this.Thickness;
                target.CurCleaningLoc = this.CurCleaningLoc;
                target.SelectIndex = this.SelectIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public NCSheetVMDefinition()
        {
            try
            {
                CurCleaningLoc.X.Value = 100000;
                CurCleaningLoc.Y.Value = 50000;
                Index = 0;
                Thickness = 0;
                //SheetHeight = 100;
                //SheetWidth = 100;
                SheetLeft = 0;
                SheetTop = 0;
                SelectIndex = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private double _SheetWidth = 100;
        public double SheetWidth
        {
            get { return _SheetWidth; }
            set
            {
                if (value != _SheetWidth)
                {
                    _SheetWidth = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        private double _SheetHeight = 100;
        public double SheetHeight
        {
            get { return _SheetHeight; }
            set
            {
                if (value != _SheetHeight)
                {
                    _SheetHeight = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        private double _SheetLeft;
        public double SheetLeft
        {
            get { return _SheetLeft; }
            set
            {
                if (value != _SheetLeft)
                {
                    _SheetLeft = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        private double _SheetTop;
        public double SheetTop
        {
            get { return _SheetTop; }
            set
            {
                if (value != _SheetTop)
                {
                    _SheetTop = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        private int _Thickness;
        public int Thickness
        {
            get { return _Thickness; }
            set
            {
                if (value != _Thickness)
                {
                    _Thickness = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        private int _SelectIndex;
        public int SelectIndex
        {
            get { return _SelectIndex; }
            set
            {
                if (value != _SelectIndex)
                {
                    _SelectIndex = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        private Visibility _ResultVisibility;
        public Visibility ResultVisibility
        {
            get { return _ResultVisibility; }
            set
            {
                if (value != _ResultVisibility)
                {
                    _ResultVisibility = value;
                    RaisePropertyChanged();
                    this.GetParam_NcObject().NCSheetVMDefsUpdated();
                }
            }
        }

        #region //Flags

        /// <summary>
        /// 여기 항목들은 시스템 파라미터가 아님. 따라서 파일에 저장할 필요가 없음.
        /// 니들 클리닝 동작 중에 사용한 Global 변수들 목록.
        /// </summary>
        private NCCoordinate _CurCleaningLoc = new NCCoordinate();
        public NCCoordinate CurCleaningLoc
        {
            get { return _CurCleaningLoc; }
            set
            {
                if (value != _CurCleaningLoc)
                {
                    _CurCleaningLoc = value;
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// 포커싱 NC1/NC2/NC3 각각에 대해 포커싱 해야 하나 말아야 하나에 대한 명령
        /// 수치는 포커싱을 몇 포인트 해야 하느냐를 나타낸다. (0: 할 필요 없음. 1: 1포인트  5:5포인트)
        /// </summary>
        private int _FlagRequiredFocus = new int();
        public int FlagRequiredFocus
        {
            get { return _FlagRequiredFocus; }
            set
            {
                if (value != _FlagRequiredFocus)
                {
                    _FlagRequiredFocus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<NCCoordinate> _Heights = new ObservableCollection<NCCoordinate>();
        public ObservableCollection<NCCoordinate> Heights
        {
            get { return _Heights; }
            set
            {
                if (value != _Heights)
                {
                    _Heights = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 1회 포커싱 NC1/NC2/NC3 각각에 대한 포커싱 결과 플래그. 클리닝 시작 시 반드시 초기화 할것.        
        /// </summary>
        private bool _FlagFocusDone = new bool();
        public bool FlagFocusDone
        {
            get { return _FlagFocusDone; }
            set
            {
                if (value != _FlagFocusDone)
                {
                    _FlagFocusDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// NC1/NC2/NC3 각각에 대해 클리닝 해야 하나 말아야 하나에 대한 명령
        /// </summary>
        private bool _FlagRequiredCleaning = new bool();
        public bool FlagRequiredCleaning
        {
            get { return _FlagRequiredCleaning; }
            set
            {
                if (value != _FlagRequiredCleaning)
                {
                    _FlagRequiredCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 포커싱 NC1/NC2/NC3 각각에 대한 클리닝 결과 플래그. 클리닝 시작 시 반드시 초기화 할것.        
        /// </summary>
        private bool _FlagCleaningDone = new bool();
        public bool FlagCleaningDone
        {
            get { return _FlagCleaningDone; }
            set
            {
                if (value != _FlagCleaningDone)
                {
                    _FlagCleaningDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 실제로 클리닝해야 할 X/Y 위치. 매번 클리닝할 때마다 재설정된다. (NC좌표계)
        /// </summary>
        private List<NCCoordinate> _CleaningSeq = new List<NCCoordinate>();
        public List<NCCoordinate> CleaningSeq
        {
            get { return _CleaningSeq; }
            set
            {
                if (value != _CleaningSeq)
                {
                    _CleaningSeq = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 장비 초기화 후 NC1/NC2/NC3 각각에 대해 전체 포커싱 한 적이 있냐 플래그. 
        /// 머신 이닛 시 반드시 초기화 할것.        
        /// 이 플래그가 False면, 즉 머신 이닛 후 한 번도 전체 포커싱한 적이 없으면 처음 포커싱에서는
        /// 5포인트를 포커싱한다.
        /// </summary>
        private bool _FlagPlanarityConfirmed = new bool();
        public bool FlagPlanarityConfirmed
        {
            get { return _FlagPlanarityConfirmed; }
            set
            {
                if (value != _FlagPlanarityConfirmed)
                {
                    _FlagPlanarityConfirmed = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 이번 웨이퍼에 대해서 포커싱을 했는지 안 했는지 기억하는 플래그
        /// 랏드 재시작 시, 웨이퍼 언로딩 시 클리어할 것
        /// </summary>
        private bool _FlagFocusedForCurrentWafer = new bool();
        public bool FlagFocusedForCurrentWafer
        {
            get { return _FlagFocusedForCurrentWafer; }
            set
            {
                if (value != _FlagFocusedForCurrentWafer)
                {
                    _FlagFocusedForCurrentWafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 이번 랏드에 대해서 포커싱을 했는지 안 했는지 기억하는 플래그
        /// 랏드 재시작 시 클리어할 것
        /// </summary>
        private bool _FlagFocusedForCurrentLot = new bool();
        public bool FlagFocusedForCurrentLot
        {
            get { return _FlagFocusedForCurrentLot; }
            set
            {
                if (value != _FlagFocusedForCurrentLot)
                {
                    _FlagFocusedForCurrentLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 이번 랏드에 대해서 클리닝을 했는지 안 했는지 기억하는 플래그
        /// 랏드 재시작 시 클리어할 것
        /// </summary>
        private bool _FlagCleaningForCurrentLot = new bool();
        public bool FlagCleaningForCurrentLot
        {
            get { return _FlagCleaningForCurrentLot; }
            set
            {
                if (value != _FlagCleaningForCurrentLot)
                {
                    _FlagCleaningForCurrentLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 엔지니어링 플래그
        // ON되면 Cleaning UP 까지 원래 속도의 10%로 느리게 올라가며, UP 상태로 플래그가 OFF 될 때까지 대기한다.
        private bool _FlagHoldCleaningUpState = new bool();
        public bool FlagHoldCleaningUpState
        {
            get { return _FlagHoldCleaningUpState; }
            set
            {
                if (value != _FlagHoldCleaningUpState)
                {
                    _FlagHoldCleaningUpState = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        // 엔지니어링 클리닝 OD
        // 클리닝 옵셋을 구하기 위해서 임시로 사용되는 OD
        private double _EngrOverdrive = new double();
        public double EngrOverdrive
        {
            get { return _EngrOverdrive; }
            set
            {
                if (value != _EngrOverdrive)
                {
                    _EngrOverdrive = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 엔지니어링 클리닝 속도
        // 클리닝 옵셋을 구하기 위해서 임시로 사용되는 속도
        private double _EngrCleaningSpeed = new double();
        public double EngrCleaningSpeed
        {
            get { return _EngrCleaningSpeed; }
            set
            {
                if (value != _EngrCleaningSpeed)
                {
                    _EngrCleaningSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 엔지니어링 클리닝 가속도
        // 클리닝 옵셋을 구하기 위해서 임시로 사용되는 가속도
        private double _EngrCleaningAccel = new double();
        public double EngrCleaningAccel
        {
            get { return _EngrCleaningAccel; }
            set
            {
                if (value != _EngrCleaningAccel)
                {
                    _EngrCleaningAccel = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

    }
}
