using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProberInterfaces.Param
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [Serializable()]
    public class StageCoords : INotifyPropertyChanged, IStageCoords, ISystemParameterizable, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
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

        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "";

        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "StageCoordination.json";

        private PinRegRange _PinReg;
        public PinRegRange PinReg
        {
            get { return _PinReg; }
            set
            {
                if (value != _PinReg)
                {
                    _PinReg = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> Chuck 상판 기준 : Chuck Center -> WH
        private CatCoordinates _WHOffset;
        public CatCoordinates WHOffset
        {
            get { return _WHOffset; }
            set
            {
                if (value != _WHOffset)
                {
                    _WHOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _WLCAMFromWH;
        public CatCoordinates WLCAMFromWH
        {
            get { return _WLCAMFromWH; }
            set
            {
                if (value != _WLCAMFromWH)
                {
                    _WLCAMFromWH = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> MARK -> PH : 상대 거리
        private CatCoordinates _PHOffset;
        public CatCoordinates PHOffset
        {
            get { return _PHOffset; }
            set
            {
                if (value != _PHOffset)
                {
                    _PHOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> PH -> PL : 상대 거리
        private CatCoordinates _PLCAMFromPH;
        public CatCoordinates PLCAMFromPH
        {
            get { return _PLCAMFromPH; }
            set
            {
                if (value != _PLCAMFromPH)
                {
                    _PLCAMFromPH = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==>  Chuck 상판 기준 : WH -> MARK
        private CatCoordinates _MarkEncPos;
        public CatCoordinates MarkEncPos
        {
            get { return _MarkEncPos; }
            set
            {
                if (value != _MarkEncPos)
                {
                    _MarkEncPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _RefMarkPos;
        [XmlIgnore, JsonIgnore]
        public CatCoordinates RefMarkPos
        {
            get { return _RefMarkPos; }
            set
            {
                if (value != _RefMarkPos)
                {
                    _RefMarkPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> Chuck Center -> MARK
        private CatCoordinates _MarkPosInChuckCoord;
        public CatCoordinates MarkPosInChuckCoord
        {
            get { return _MarkPosInChuckCoord; }
            set
            {
                if (value != _MarkPosInChuckCoord)
                {
                    _MarkPosInChuckCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _ChuckCenter;
        public CatCoordinates ChuckCenter
        {
            get { return _ChuckCenter; }
            set
            {
                if (value != _ChuckCenter)
                {
                    _ChuckCenter = value;
                    RaisePropertyChanged();
                }
            }
        }



        //==> Mark -> DiskPos
        private CatCoordinates _MarkPosInDiskPosCoord;
        public CatCoordinates MarkPosInDiskPosCoord
        {
            get { return _MarkPosInDiskPosCoord; }
            set
            {
                if (value != _MarkPosInDiskPosCoord)
                {
                    _MarkPosInDiskPosCoord = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CatCoordinates _ChuckLoadingPosition;
        public CatCoordinates ChuckLoadingPosition
        {
            get { return _ChuckLoadingPosition; }
            set
            {
                if (value != _ChuckLoadingPosition)
                {
                    _ChuckLoadingPosition = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private Element<double> _HandlerholdingPosX
           = new Element<double>();
        public Element<double> HandlerholdingPosX
        {
            get { return _HandlerholdingPosX; }
            set
            {
                if (value != _HandlerholdingPosX)
                {
                    _HandlerholdingPosX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _HandlerholdingPosY
           = new Element<double>();
        public Element<double> HandlerholdingPosY
        {
            get { return _HandlerholdingPosY; }
            set
            {
                if (value != _HandlerholdingPosY)
                {
                    _HandlerholdingPosY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _HandlerholdingPosZ
           = new Element<double>();
        public Element<double> HandlerholdingPosZ
        {
            get { return _HandlerholdingPosZ; }
            set
            {
                if (value != _HandlerholdingPosZ)
                {
                    _HandlerholdingPosZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _HandlerholdingPosT
           = new Element<double>();
        public Element<double> HandlerholdingPosT
        {
            get { return _HandlerholdingPosT; }
            set
            {
                if (value != _HandlerholdingPosT)
                {
                    _HandlerholdingPosT = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _BernoulliTopHandlerAttached
            = new Element<bool>();
        public Element<bool> BernoulliTopHandlerAttached
        {
            get { return _BernoulliTopHandlerAttached; }
            set
            {
                if (value != _BernoulliTopHandlerAttached)
                {
                    _BernoulliTopHandlerAttached = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _ProbingSWLimitPositive;
        public CatCoordinates ProbingSWLimitPositive
        {
            get { return _ProbingSWLimitPositive; }
            set
            {
                if (value != _ProbingSWLimitPositive)
                {
                    _ProbingSWLimitPositive = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CatCoordinates _ProbingSWLimitNegative;
        public CatCoordinates ProbingSWLimitNegative
        {
            get { return _ProbingSWLimitNegative; }
            set
            {
                if (value != _ProbingSWLimitNegative)
                {
                    _ProbingSWLimitNegative = value;
                    RaisePropertyChanged();
                }
            }
        }
        // 그룹 프로버 때문에 만듦. 핀 좌표계 원점(Z = 0)보다 상판 밑면이 3mm 더 내려와 있음. (강성 때문에)
        // 척 중심으로부터 일정 거리만큼 떨어지면 리밋 에러 발생
        private Element<double> _ProbingSWRadiusLimit = new Element<double>();

        public Element<double> ProbingSWRadiusLimit
        {
            get { return _ProbingSWRadiusLimit; }
            set
            {
                _ProbingSWRadiusLimit = value;
                RaisePropertyChanged();
            }
        }

        // 
        private Element<int> _ChuckVacBlowTimeout = new Element<int>();

        public Element<int> ChuckVacBlowTimeout
        {
            get { return _ChuckVacBlowTimeout; }
            set
            {
                _ChuckVacBlowTimeout = value;
                RaisePropertyChanged();
            }
        }

        private Element<int> _DelayTimeBeforeOnlyChuckVacOff = new Element<int>() { Value = 100 };
        /// <summary>
        /// Chuck Vac Generator 를 사용하는 경우 메인베큠에서 오는 척베큠 사용안하고 Vac Generator 로만 웨이퍼를 척에 유지하기 위해서 척베큠 켠 후 끄기 전까지 대기하는 시간, msec
        /// Default 값은 CheckWafer 함수의 sustain time을 사용했음. 
        /// </summary>
        public Element<int> DelayTimeBeforeOnlyChuckVacOff
        {
            get { return _DelayTimeBeforeOnlyChuckVacOff; }
            set
            {
                _DelayTimeBeforeOnlyChuckVacOff = value;
                RaisePropertyChanged();
            }
        }



        private Element<int> _DelayTimeAfterOnlyChuckVacOff = new Element<int>() { Value = 100 };
        /// <summary>
        /// 척베큠만 끈 후에 체크하기 전 DelayTime, msec
        /// Default 값은 CheckWafer 함수의 sustain time을 사용했음. 
        /// </summary>
        public Element<int> DelayTimeAfterOnlyChuckVacOff
        {
            get { return _DelayTimeAfterOnlyChuckVacOff; }
            set
            {
                _DelayTimeAfterOnlyChuckVacOff = value;
                RaisePropertyChanged();
            }
        }

        private Element<int> _ChuckVacBlowMaintainTime_PW = new Element<int>();

        public Element<int> ChuckVacBlowMaintainTime_PW
        {
            get { return _ChuckVacBlowMaintainTime_PW; }
            set
            {
                _ChuckVacBlowMaintainTime_PW = value;
                RaisePropertyChanged();
            }
        }

        private Element<int> _DelayTimeAfterThreeLegAreakVacOff = new Element<int>() { Value = 0 };
        /// <summary>
        /// 베큠 파기가 더 잘되도록 삼발이 영역(6인치) 베큠 끄고 먼저 6Inch Blow 를 한 후 주변영역에 공기가 퍼지도록 기다리는 DelayTime, msec
        /// Defatul 값 : 0
        /// </summary>
        public Element<int> DelayTimeAfterThreeLegAreakVacOff
        {
            get { return _DelayTimeAfterThreeLegAreakVacOff; }
            set
            {
                _DelayTimeAfterThreeLegAreakVacOff = value;
                RaisePropertyChanged();
            }
        }

        private Element<double> _ProbingSWRadiusZLimit = new Element<double>();

        public Element<double> ProbingSWRadiusZLimit
        {
            get { return _ProbingSWRadiusZLimit; }
            set
            {
                _ProbingSWRadiusZLimit = value;
                RaisePropertyChanged();
            }
        }
        private CatCoordinates _CleanPadSWLimitNegative;
        public CatCoordinates CleanPadSWLimitNegative
        {
            get { return _CleanPadSWLimitNegative; }
            set
            {
                if (value != _CleanPadSWLimitNegative)
                {
                    _CleanPadSWLimitNegative = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private double _SafePosZAxis;
        public double SafePosZAxis
        {
            get { return _SafePosZAxis; }
            set
            {
                if (value != _SafePosZAxis)
                {
                    _SafePosZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _SafePosPZAxis;
        public double SafePosPZAxis
        {
            get { return _SafePosPZAxis; }
            set
            {
                if (value != _SafePosPZAxis)
                {
                    _SafePosPZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }

        //TODO : touchsensorsystemparameter 이동, 추후 삭제할 파라미터
        private double? _TouchSensorRegMax;
        public double? TouchSensorRegMax
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

        //TODO : touchsensorsystemparameter 이동, 추후 삭제할 파라미터
        private double? _TouchSensorRegMin;
        public double? TouchSensorRegMin
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

        private Element<double> _MachineSequareness = new Element<double>();
        public Element<double> MachineSequareness
        {
            get { return _MachineSequareness; }
            set
            {
                if (value != _MachineSequareness)
                {
                    _MachineSequareness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _HandlerPrePositionOffset = new Element<double>();
        public Element<double> HandlerPrePositionOffset
        {
            get { return _HandlerPrePositionOffset; }
            set
            {
                if (value != _HandlerPrePositionOffset)
                {
                    _HandlerPrePositionOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _ChuckCenterX = new Element<double>();
        public Element<double> ChuckCenterX
        {
            get { return _ChuckCenterX; }
            set
            {
                if (value != _ChuckCenterX)
                {
                    _ChuckCenterX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _ChuckCenterY = new Element<double>();
        public Element<double> ChuckCenterY
        {
            get { return _ChuckCenterY; }
            set
            {
                if (value != _ChuckCenterY)
                {
                    _ChuckCenterY = value;
                    RaisePropertyChanged();
                }
            }
        }

        
        private Element<bool> _ReverseManualMoveX = new Element<bool> { Value= false };
        // 카드의 방향을 180도 반대로 도킹하게 되어서 만들어진 파라미터, DisplayPort, MapviewControl, Jog 로 이동할때 반대로 움직임. 
        public Element<bool> ReverseManualMoveX
        {
            get { return _ReverseManualMoveX; }
            set
            {
                if (value != _ReverseManualMoveX)
                {
                    _ReverseManualMoveX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _ReverseManualMoveY = new Element<bool> { Value = false };
        // 카드의 방향을 180도 반대로 도킹하게 되어서 만들어진 파라미터, DisplayPort, MapviewControl, Jog 로 이동할때 반대로 움직임. 
        public Element<bool> ReverseManualMoveY
        {
            get { return _ReverseManualMoveY; }
            set
            {
                if (value != _ReverseManualMoveY)
                {
                    _ReverseManualMoveY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _EdgeProcessClampWidth = new Element<int>() { Value = 180 };
        public Element<int> EdgeProcessClampWidth
        {
            get { return _EdgeProcessClampWidth; }
            set
            {
                if (value != _EdgeProcessClampWidth)
                {
                    _EdgeProcessClampWidth = value;
                    RaisePropertyChanged();
                }
            }
        }


        private CatCoordinates _TCW_Position=new CatCoordinates(120000,-400000,-75000);
        public CatCoordinates TCW_Position
        {
            get { return _TCW_Position; }
            set
            {
                if (value != _TCW_Position)
                {
                    _TCW_Position = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<float> _Z0Angle = new Element<float>() { Value = 210 };
        public Element<float> Z0Angle
        {
            get { return _Z0Angle; }
            set
            {
                if (value != _Z0Angle)
                {
                    _Z0Angle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<float> _Z1Angle = new Element<float>() { Value = 90 };
        public Element<float> Z1Angle
        {
            get { return _Z1Angle; }
            set
            {
                if (value != _Z1Angle)
                {
                    _Z1Angle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<float> _Z2Angle = new Element<float>() { Value = 330 };
        public Element<float> Z2Angle
        {
            get { return _Z2Angle; }
            set
            {
                if (value != _Z2Angle)
                {
                    _Z2Angle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<float> _PCD = new Element<float>() { Value = 110000 };
        public Element<float> PCD
        {
            get { return _PCD; }
            set
            {
                if (value != _PCD)
                {
                    _PCD = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _ChuckPlanarityTol = new Element<double>() { Value = 15 };
        public Element<double> ChuckPlanarityTol
        {
            get { return _ChuckPlanarityTol; }
            set
            {
                if (value != _ChuckPlanarityTol)
                {
                    _ChuckPlanarityTol = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }


        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                WHOffset = new CatCoordinates();
                WLCAMFromWH = new CatCoordinates();
                PHOffset = new CatCoordinates();
                PLCAMFromPH = new CatCoordinates();
                MarkEncPos = new CatCoordinates();
                MarkPosInChuckCoord = new CatCoordinates();
                ChuckCenter = new CatCoordinates();
                PinReg = new PinRegRange();
                MarkPosInDiskPosCoord = new CatCoordinates();
                ChuckLoadingPosition = new CatCoordinates();
                HandlerholdingPosX = new Element<double>();
                HandlerholdingPosY = new Element<double>();
                HandlerholdingPosZ = new Element<double>();
                HandlerholdingPosT = new Element<double>();
                ChuckVacBlowTimeout = new Element<int>();
                BernoulliTopHandlerAttached = new Element<bool>();
                ProbingSWLimitPositive = new CatCoordinates();
                ProbingSWLimitNegative = new CatCoordinates();
                CleanPadSWLimitNegative = new CatCoordinates();
                RefMarkPos = new CatCoordinates();
                ChuckVacBlowTimeout.Value = 0;
                ProbingSWRadiusZLimit.Value = -3500;
                ProbingSWRadiusLimit.Value = 20000;
                HandlerPrePositionOffset = new Element<double>();
                ChuckVacBlowMaintainTime_PW.Value = 0;

                //SetDefaultParam_OPUSV();
                SetDefaultParam_BSCI1();

                ChuckCenterX.Value = 0.0;
                ChuckCenterY.Value = 0.0;

                EdgeProcessClampWidth.Value = 180;

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public void SetElementMetaData()
        {
            WHOffset.X.AssociateElementID = "C1,C3,C4,C7,C8";
            WHOffset.Y.AssociateElementID = "C1,C3,C4,C7,C8";
            WHOffset.Z.AssociateElementID = "C1,C3,C4,C7,C8";

            PHOffset.X.AssociateElementID = "C1,C3,C4,C7,C8";
            PHOffset.Y.AssociateElementID = "C1,C3,C4,C7,C8";
            PHOffset.Z.AssociateElementID = "C1,C3,C4,C7,C8";
            
            MachineSequareness.CategoryID = "10015";
            MachineSequareness.ElementName = "Machine Squareness";
            MachineSequareness.Description = "Machine Squareness";
            MachineSequareness.ReadMaskingLevel = 0;
            MachineSequareness.WriteMaskingLevel = 0;


            //ProbingSWRadiusZLimit.Value = -3500;
            //ProbingSWRadiusLimit.Value = 20000;

            // 반경 외 영역에서 올라갈 수 있는 최대 높이
            ProbingSWRadiusZLimit.CategoryID = "10015";
            ProbingSWRadiusZLimit.ElementName = "Probing hieght limit for probing radius";
            ProbingSWRadiusZLimit.Description = "Probing radius limit";
            ProbingSWRadiusZLimit.ReadMaskingLevel = 0;
            ProbingSWRadiusZLimit.WriteMaskingLevel = 0;


            // 조도 좋은 척에서 사용하는 Chuck blow time
            ChuckVacBlowTimeout.CategoryID = "10015";
            ChuckVacBlowTimeout.ElementName = "Chuck Vaccum Blow Timeout";
            ChuckVacBlowTimeout.Description = "Chuck Vaccum Blow Timeout";
            ChuckVacBlowTimeout.ReadMaskingLevel = 0;
            ChuckVacBlowTimeout.WriteMaskingLevel = 0;

            // PolishWafer Chuck Blow 시 사용되는 Maintaintime
            ChuckVacBlowMaintainTime_PW.CategoryID = "10015";
            ChuckVacBlowMaintainTime_PW.ElementName = "Chuck Vaccum Blow Maintaintime (Only Polish Wafer)";
            ChuckVacBlowMaintainTime_PW.Description = "Chuck Vaccum Blow Maintaintime (Only Polish Wafer)";
            ChuckVacBlowMaintainTime_PW.ReadMaskingLevel = 0;
            ChuckVacBlowMaintainTime_PW.WriteMaskingLevel = 0;


            // + 핀 높이에서 프로빙 가능 반경 정의
            ProbingSWRadiusLimit.CategoryID = "10015";
            ProbingSWRadiusLimit.ElementName = "Probing radius limit";
            ProbingSWRadiusLimit.Description = "Probing radius limit";
            ProbingSWRadiusLimit.ReadMaskingLevel = 0;
            ProbingSWRadiusLimit.WriteMaskingLevel = 0;

            BernoulliTopHandlerAttached.CategoryID = "10015";
            BernoulliTopHandlerAttached.ElementName = "Bernoulli Top Handler Attached";
            BernoulliTopHandlerAttached.Description = "Bernoulli Top Handler Attached";
            BernoulliTopHandlerAttached.ReadMaskingLevel = 0;
            BernoulliTopHandlerAttached.WriteMaskingLevel = 0;

            HandlerholdingPosX.CategoryID = "10015";
            HandlerholdingPosX.ElementName = "Bernoulli Top Handler Holding Pos X";
            HandlerholdingPosX.Description = "Bernoulli Top Handler Holding Pos X";
            HandlerholdingPosX.ReadMaskingLevel = 0;
            HandlerholdingPosX.WriteMaskingLevel = 0;

            HandlerholdingPosY.CategoryID = "10015";
            HandlerholdingPosY.ElementName = "Bernoulli Top Handler Holding Pos Y";
            HandlerholdingPosY.Description = "Bernoulli Top Handler Holding Pos Y";
            HandlerholdingPosY.ReadMaskingLevel = 0;
            HandlerholdingPosY.WriteMaskingLevel = 0;

            HandlerholdingPosZ.CategoryID = "10015";
            HandlerholdingPosZ.ElementName = "Bernoulli Top Handler Holding Pos Z";
            HandlerholdingPosZ.Description = "Bernoulli Top Handler Holding Pos Z";
            HandlerholdingPosZ.ReadMaskingLevel = 0;
            HandlerholdingPosZ.WriteMaskingLevel = 0;

            HandlerholdingPosT.CategoryID = "10015";
            HandlerholdingPosT.ElementName = "Bernoulli Top Handler Holding Pos T";
            HandlerholdingPosT.Description = "Bernoulli Top Handler Holding Pos T";
            HandlerholdingPosT.ReadMaskingLevel = 0;
            HandlerholdingPosT.WriteMaskingLevel = 0;

            HandlerPrePositionOffset.CategoryID = "10015";
            HandlerPrePositionOffset.ElementName = "Bernoulli Top Handler PrePosition Offset";
            HandlerPrePositionOffset.Description = "Bernoulli Top Handler PrePosition Offset";
            HandlerPrePositionOffset.ReadMaskingLevel = 0;
            HandlerPrePositionOffset.WriteMaskingLevel = 0;
            HandlerPrePositionOffset.UpperLimit = 50000;
            HandlerPrePositionOffset.LowerLimit = 0;
            
            ChuckCenterX.CategoryID = "10015";
            ChuckCenterX.ElementName = "Chuck Center X position in Wafer Coordinate";
            ChuckCenterX.Description = "Chuck Center X position in Wafer Coordinate";
            ChuckCenterX.UpperLimit = 2500;
            ChuckCenterX.LowerLimit = -2500;
            ChuckCenterX.Unit = "um";

            ChuckCenterY.CategoryID = "10015";
            ChuckCenterY.ElementName = "Chuck Center Y position in Wafer Coordinate";
            ChuckCenterY.Description = "Chuck Center Y position in Wafer Coordinate";
            ChuckCenterY.UpperLimit = 2500;
            ChuckCenterY.LowerLimit = -2500;
            ChuckCenterY.Unit = "um";

            EdgeProcessClampWidth.CategoryID = "10015";
            EdgeProcessClampWidth.ElementName = "Edge Process Clamp Width";
            EdgeProcessClampWidth.Description = "Edge Process Clamp Width";
            EdgeProcessClampWidth.UpperLimit = 300;
            EdgeProcessClampWidth.LowerLimit = 10;
            EdgeProcessClampWidth.Unit = "Pixel";



        }
        private void SetDefaultParam_OPUSV()
        {
            try
            {
                WHOffset.X.Value = 19500;
                WHOffset.Y.Value = 0;
                WHOffset.Z.Value = -67410;  // 상판 (0,0,0)로 부터 웨이퍼 하이 카메라 초점 맞는 위치
                WHOffset.T.Value = 0;

                //WLCAMFromWH.X.Value = -38938;
                //WLCAMFromWH.Y.Value = 160;
                //WLCAMFromWH.Z.Value = 0;
                //WLCAMFromWH.T.Value = 0;
                //WLCAMFromWH.X.Value = -38982;
                //WLCAMFromWH.Y.Value = -70.1;
                //WLCAMFromWH.Z.Value = 0;
                //WLCAMFromWH.T.Value = 0;
                WLCAMFromWH.X.Value = -38989;
                WLCAMFromWH.Y.Value = -56;
                WLCAMFromWH.Z.Value = 185;
                WLCAMFromWH.T.Value = 0;

                // Pin high offset의 정의는 레퍼런스 마크의 위치와 카메라 결상 위치와의 차이
                // Pin high 초점 위치와 레퍼런스 마크 이미지 투사 위치는 동일.
                // 따라서 이론적으로 Pin high offset은 (0, 0, 0)이여야 함. 
                // 추가적인 보정은 Optical Axis Alignment System(OAAS)로 보정 해야 함.
                PHOffset.X.Value = 0;
                PHOffset.Y.Value = 0;
                PHOffset.Z.Value = -70;
                PHOffset.T.Value = 0;

                PLCAMFromPH.X.Value = 32461;
                PLCAMFromPH.Y.Value = 185;
                PLCAMFromPH.Z.Value = 20719;
                PLCAMFromPH.T.Value = 0;

                MarkEncPos.X.Value = 41695.2;
                MarkEncPos.Y.Value = 182153;
                MarkEncPos.Z.Value = -88244;
                MarkEncPos.T.Value = 0;

                MarkPosInChuckCoord.X.Value = -17589.2;
                MarkPosInChuckCoord.Y.Value = -185861.3;
                MarkPosInChuckCoord.Z.Value = 20000;
                MarkPosInChuckCoord.T.Value = 0;

                ChuckCenter.X.Value = -5298668;
                ChuckCenter.Y.Value = 100291789;
                ChuckCenter.Z.Value = -290552;
                ChuckCenter.T.Value = 9134;

                PinReg.PinRegMin.Value = -10000;
                PinReg.PinRegMax.Value = -100;

                MarkPosInDiskPosCoord.X.Value = -17000;
                MarkPosInDiskPosCoord.Y.Value = 67000;
                MarkPosInDiskPosCoord.Z.Value = 16000;
                MarkPosInDiskPosCoord.T.Value = 0;

                ChuckLoadingPosition.X.Value = 162838;
                ChuckLoadingPosition.Y.Value = 47510;
                ChuckLoadingPosition.Z.Value = -77240;
                ChuckLoadingPosition.T.Value = 0;

                HandlerholdingPosX.Value = 162838;
                HandlerholdingPosY.Value = 47510;
                HandlerholdingPosZ.Value = -77240;
                HandlerholdingPosT.Value = 0;

                BernoulliTopHandlerAttached.Value = false;

                ProbingSWLimitPositive.X.Value = 173000;
                ProbingSWLimitPositive.Y.Value = 180000;
                ProbingSWLimitPositive.Z.Value = -1000;

                ProbingSWLimitNegative.X.Value = -173000;
                ProbingSWLimitNegative.Y.Value = -190000;
                ProbingSWLimitNegative.Z.Value = -80000;

                CleanPadSWLimitNegative.X.Value = -173000;
                CleanPadSWLimitNegative.Y.Value = 32000;
                CleanPadSWLimitNegative.Z.Value = -80000;



                TouchSensorRegMax = -58000;
                TouchSensorRegMin = -70000;

                SafePosPZAxis = -65000;
                SafePosZAxis = -55000;
                HandlerPrePositionOffset.Value = 10000;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private void SetDefaultParam_BSCI1()
        {
            try
            {
                WHOffset.X.Value = 18602;
                WHOffset.Y.Value = 27;
                WHOffset.Z.Value = -65823;  // 상판 (0,0,0)로 부터 웨이퍼 하이 카메라 초점 맞는 위치
                WHOffset.T.Value = 0;

                WLCAMFromWH.X.Value = -38703;
                WLCAMFromWH.Y.Value = 451;
                WLCAMFromWH.Z.Value = -138;
                WLCAMFromWH.T.Value = 0;

                // Pin high offset의 정의는 레퍼런스 마크의 위치와 카메라 결상 위치와의 차이
                // Pin high 초점 위치와 레퍼런스 마크 이미지 투사 위치는 동일.
                // 따라서 이론적으로 Pin high offset은 (0, 0, 0)이여야 함. 
                // 추가적인 보정은 Optical Axis Alignment System(OAAS)로 보정 해야 함.
                PHOffset.X.Value = 0;
                PHOffset.Y.Value = 0;
                PHOffset.Z.Value = 0;
                PHOffset.T.Value = 0;

                PLCAMFromPH.X.Value = 32652;
                PLCAMFromPH.Y.Value = -38;
                PLCAMFromPH.Z.Value = 20760;
                PLCAMFromPH.T.Value = 0;

                MarkEncPos.X.Value = 34334.1;
                MarkEncPos.Y.Value = 185172.2;
                MarkEncPos.Z.Value = -89670;
                MarkEncPos.T.Value = 0;

                MarkPosInChuckCoord.X.Value = -16503.7;
                MarkPosInChuckCoord.Y.Value = -185238.1;
                MarkPosInChuckCoord.Z.Value = 21286;
                MarkPosInChuckCoord.T.Value = 0;

                ChuckCenter.X.Value = 0;
                ChuckCenter.Y.Value = 0;
                ChuckCenter.Z.Value = 0;
                ChuckCenter.T.Value = 0;

                PinReg.PinRegMin.Value = -10000;
                PinReg.PinRegMax.Value = -100;

                MarkPosInDiskPosCoord.X.Value = -17000;
                MarkPosInDiskPosCoord.Y.Value = 67000;
                MarkPosInDiskPosCoord.Z.Value = 16000;
                MarkPosInDiskPosCoord.T.Value = 0;

                ChuckLoadingPosition.X.Value = 154750;
                ChuckLoadingPosition.Y.Value = 50247;
                ChuckLoadingPosition.Z.Value = -76280;
                ChuckLoadingPosition.T.Value = 0;

                HandlerholdingPosX.Value = 154750;
                HandlerholdingPosY.Value = 50247;
                HandlerholdingPosZ.Value = -76280;
                HandlerholdingPosT.Value = 0;

                BernoulliTopHandlerAttached.Value = false;

                ProbingSWLimitPositive.X.Value = 173000;
                ProbingSWLimitPositive.Y.Value = 180000;
                ProbingSWLimitPositive.Z.Value = -1000;

                ProbingSWLimitNegative.X.Value = -173000;
                ProbingSWLimitNegative.Y.Value = -190000;
                ProbingSWLimitNegative.Z.Value = -80000;

                CleanPadSWLimitNegative.X.Value = -173000;
                CleanPadSWLimitNegative.Y.Value = 32000;
                CleanPadSWLimitNegative.Z.Value = -80000;

                TouchSensorRegMax = -58000;
                TouchSensorRegMin = -70000;
                MachineSequareness.Value = -0.78675;

                HandlerPrePositionOffset.Value = 10000;

                ReverseManualMoveX.Value = false;
                ReverseManualMoveY.Value = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public StageCoords()
        {

        }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                double markposx = this.MarkEncPos.X.Value;
                double markposy = this.MarkEncPos.Y.Value;
                double markposz = this.MarkEncPos.Z.Value;
                double markpost = this.MarkEncPos.T.Value;

                if(RefMarkPos == null)
                {
                    RefMarkPos = new CatCoordinates();
                }

                this.RefMarkPos.X.Value = markposx;
                this.RefMarkPos.Y.Value = markposy;
                this.RefMarkPos.Z.Value = markposz;
                this.RefMarkPos.T.Value = markpost;

                //Z0,Z1,Z2 Single, Multiple 기준이 다르다.
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Z0Angle.Value = 30;
                    Z1Angle.Value = 270;
                    Z2Angle.Value = 150;
                    PCD.Value = 110000;
                }
                else
                {
                    Z0Angle.Value = 210;
                    Z1Angle.Value = 90;
                    Z2Angle.Value = 330;
                    PCD.Value = 110000;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }
            return retval;
        }
    }
}
