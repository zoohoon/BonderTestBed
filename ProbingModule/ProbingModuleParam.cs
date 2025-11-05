using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ProbingModule
{
    [Serializable]
    public class ProbingModuleSysParam : INotifyPropertyChanged, IParamNode, ISystemParameterizable
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
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
        public string FilePath { get; } = "ProbingModule";
        public string FileName { get; } = "ProbingModuleSysParam.Json";

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //if(ProbingShift == null)
                //{
                //    ProbingShift = new CatCoordinates();
                //}

                if (DWellZAxisTime == null)
                {
                    DWellZAxisTime = new Element<double>();
                }

                if (TwistForProbing == null)
                {
                    TwistForProbing = new Element<double>();
                }

                if (SquarenessForProbing == null)
                {
                    SquarenessForProbing = new Element<double>();
                }

                if (DeflectX == null)
                {
                    DeflectX = new Element<double>();
                }

                if (DeflectY == null)
                {
                    DeflectY = new Element<double>();
                }

                if (OverDriveStartPosition == null)
                {
                    OverDriveStartPosition = new Element<OverDriveStartPositionType>();
                }
                if (ProbeMarkShift == null)
                {
                    ProbeMarkShift = new Element<CatCoordinates>();
                }

                if (ProbeMarkShift.Value == null)
                {
                    ProbeMarkShift.Value = new CatCoordinates();
                }

                if (UserProbeMarkShift == null)
                {
                    UserProbeMarkShift = new Element<CatCoordinates>();
                }

                if (UserProbeMarkShift.Value == null)
                {
                    UserProbeMarkShift.Value = new CatCoordinates();
                }

                UserProbeMarkShift.Value.X.UpperLimit = ProbeMarkShift.Value.X.UpperLimit;
                UserProbeMarkShift.Value.X.LowerLimit = ProbeMarkShift.Value.X.LowerLimit;
                UserProbeMarkShift.Value.Y.UpperLimit = ProbeMarkShift.Value.Y.UpperLimit;
                UserProbeMarkShift.Value.Y.LowerLimit = ProbeMarkShift.Value.Y.LowerLimit;

                if (TestingTimeout == null)
                {
                    TestingTimeout = new Element<int>();
                }

                if (TestingTimeoutOnOff == null)
                {
                    TestingTimeoutOnOff = new Element<TestingTimeoutEnum>();
                }

                if (RepeatedAlarmTime == null)
                {
                    RepeatedAlarmTime = new Element<int>();
                }

                if (RepeatedAlarmOnOff == null)
                {
                    RepeatedAlarmOnOff = new Element<RepeatedAlarmEnum>();
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
        public void SetElementMetaData()
        {
            CPC.UpperLimit = 30.0;
            CPC.LowerLimit = -30.0;
            CPC.Description = "Chuck plane compensation parameter";

            DWellZAxisTime.CategoryID = "00010017";
            DWellZAxisTime.ElementName = "Dwell Z Axis Time";

            TwistForProbing.CategoryID = "00010017";
            TwistForProbing.ElementName = "Twist for probing";

            SquarenessForProbing.CategoryID = "00010017";
            SquarenessForProbing.ElementName = "Squareness for probing";

            DeflectX.CategoryID = "00010017";
            DeflectX.ElementName = "Deflect X";

            DeflectY.CategoryID = "00010017";
            DeflectY.ElementName = "Deflect Y";

            InclineZHor.CategoryID = "00010017";
            InclineZHor.ElementName = "Incline Z Horizontal";

            InclineZVer.CategoryID = "00010017";
            InclineZVer.ElementName = "Incline Z Vertical";

            OverDriveStartPosition.CategoryID = "00010017";
            OverDriveStartPosition.ElementName = "Overdrive start position";

            ProbeMarkShift.CategoryID = "00010017";
            ProbeMarkShift.ElementName = "Probe Mark Shift";

            TestingTimeout.CategoryID = "10002";
            TestingTimeout.ElementName = "Testing Time Out(Secs)";

            TestingTimeoutOnOff.CategoryID = "10002";
            TestingTimeoutOnOff.ElementName = "Testing Time Out On Off";

            RepeatedAlarmTime.CategoryID = "10002";
            RepeatedAlarmTime.ElementName = "Repeated Alarm Time(Secs)";

            RepeatedAlarmOnOff.CategoryID = "10002";
            RepeatedAlarmOnOff.ElementName = "Repeated Alarm On Off";

            EnablePTPAEnhancer.CategoryID = "10002";
            EnablePTPAEnhancer.ElementName = "Enable PTPA Enhancement Function";

            //Element<List<ChuckPlaneCompParameter>> CPC
        }

        //private CatCoordinates _ProbingShift;
        //public CatCoordinates ProbingShift
        //{
        //    get { return _ProbingShift; }
        //    set
        //    {
        //        if (value != _ProbingShift)
        //        {
        //            _ProbingShift = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<int> _TestingTimeout
            = new Element<int>() { Value = 3600 }; // 단위는 seconds

        public Element<int> TestingTimeout
        {
            get { return _TestingTimeout; }
            set
            {
                if (value != _TestingTimeout)
                {
                    _TestingTimeout = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _RepeatedAlarmTime
            = new Element<int>() { Value = 3600 }; // 단위는 seconds

        public Element<int> RepeatedAlarmTime
        {
            get { return _RepeatedAlarmTime; }
            set
            {
                if (value != _RepeatedAlarmTime)
                {
                    _RepeatedAlarmTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<TestingTimeoutEnum> _TestingTimeoutOnOff
            = new Element<TestingTimeoutEnum>() { Value = TestingTimeoutEnum.OFF }; // TestingTimeout on, off

        public Element<TestingTimeoutEnum> TestingTimeoutOnOff
        {
            get { return _TestingTimeoutOnOff; }
            set
            {
                if (value != _TestingTimeoutOnOff)
                {
                    _TestingTimeoutOnOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<RepeatedAlarmEnum> _RepeatedAlarmOnOff
            = new Element<RepeatedAlarmEnum>() { Value = RepeatedAlarmEnum.OFF }; // RepeatedAlarm on, off

        public Element<RepeatedAlarmEnum> RepeatedAlarmOnOff
        {
            get { return _RepeatedAlarmOnOff; }
            set
            {
                if (value != _RepeatedAlarmOnOff)
                {
                    _RepeatedAlarmOnOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        //Z_UP DWell Time
        private Element<double> _DWellZAxisTime
            = new Element<double>();// 단위는 ms
        public Element<double> DWellZAxisTime
        {
            get { return _DWellZAxisTime; }
            set
            {
                if (value != _DWellZAxisTime)
                {
                    _DWellZAxisTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        //Z_Down DWell Time
        private Element<double> _DWellZDownZAxisTime
    = new Element<double>();// 단위는 ms
        public Element<double> DWellZDownZAxisTime
        {
            get { return _DWellZDownZAxisTime; }
            set
            {
                if (value != _DWellZDownZAxisTime)
                {
                    _DWellZDownZAxisTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TwistForProbing
            = new Element<double>();// 단위는 ms
        public Element<double> TwistForProbing
        {
            get { return _TwistForProbing; }
            set
            {
                if (value != _TwistForProbing)
                {
                    _TwistForProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SquarenessForProbing
            = new Element<double>();
        public Element<double> SquarenessForProbing
        {
            get { return _SquarenessForProbing; }
            set
            {
                if (value != _SquarenessForProbing)
                {
                    _SquarenessForProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DeflectX
            = new Element<double>();
        public Element<double> DeflectX
        {
            get { return _DeflectX; }
            set
            {
                if (value != _DeflectX)
                {
                    _DeflectX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DeflectY
            = new Element<double>();
        public Element<double> DeflectY
        {
            get { return _DeflectY; }
            set
            {
                if (value != _DeflectY)
                {
                    _DeflectY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _InclineZHor
            = new Element<double>();
        public Element<double> InclineZHor
        {
            get { return _InclineZHor; }
            set
            {
                if (value != _InclineZHor)
                {
                    _InclineZHor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _InclineZVer
            = new Element<double>();
        public Element<double> InclineZVer
        {
            get { return _InclineZVer; }
            set
            {
                if (value != _InclineZVer)
                {
                    _InclineZVer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<OverDriveStartPositionType> _OverDriveStartPosition
    = new Element<OverDriveStartPositionType>();
        public Element<OverDriveStartPositionType> OverDriveStartPosition
        {
            get { return _OverDriveStartPosition; }
            set
            {
                if (value != _OverDriveStartPosition)
                {
                    _OverDriveStartPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<CatCoordinates> _ProbeMarkShift
            = new Element<CatCoordinates>();
        public Element<CatCoordinates> ProbeMarkShift
        {
            get { return _ProbeMarkShift; }
            set
            {
                if (value != _ProbeMarkShift)
                {
                    _ProbeMarkShift = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<CatCoordinates> _UserProbeMarkShift
            = new Element<CatCoordinates>();
        public Element<CatCoordinates> UserProbeMarkShift
        {
            get { return _UserProbeMarkShift; }
            set
            {
                if (value != _UserProbeMarkShift)
                {
                    _UserProbeMarkShift = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Dictionary<double, CatCoordinates>> _ProbeTemperaturePositionTable
            = new Element<Dictionary<double, CatCoordinates>>();
        public Element<Dictionary<double, CatCoordinates>> ProbeTemperaturePositionTable
        {
            get { return _ProbeTemperaturePositionTable; }
            set
            {
                if (value != _ProbeTemperaturePositionTable)
                {
                    _ProbeTemperaturePositionTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<List<ChuckPlaneCompParameter>> _CPC
            = new Element<List<ChuckPlaneCompParameter>>();
        public Element<List<ChuckPlaneCompParameter>> CPC   // position이 높은 곳 부터 순서대로
        {
            get { return _CPC; }
            set
            {
                if (value != _CPC)
                {
                    _CPC = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ProbingCoordCalcType> _ProbingCoordCalcTypeEnum
             = new Element<ProbingCoordCalcType>() { Value = ProbingCoordCalcType.USERCOORD };
        public Element<ProbingCoordCalcType> ProbingCoordCalcTypeEnum
        {
            get { return _ProbingCoordCalcTypeEnum; }
            set
            {
                if (value != _ProbingCoordCalcTypeEnum)
                {
                    _ProbingCoordCalcTypeEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _EnablePTPAEnhancer = new Element<bool>();
        public Element<bool> EnablePTPAEnhancer
        {
            get { return _EnablePTPAEnhancer; }
            set
            {
                if (value != _EnablePTPAEnhancer)
                {
                    _EnablePTPAEnhancer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _WaitProbingStartRspEnable = new Element<bool>() { Value = false };

        public Element<bool> WaitProbingStartRspEnable
        {
            get { return _WaitProbingStartRspEnable; }
            set
            {

                if (value != _WaitProbingStartRspEnable)
                {
                    _WaitProbingStartRspEnable = value;
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
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                OverDriveStartPosition.Value = OverDriveStartPositionType.ALL_CONTACT;
                DWellZAxisTime.Value = 0;
                TwistForProbing.Value = 0;
                SquarenessForProbing.Value = 0;
                DeflectX.Value = 0;
                DeflectY.Value = 0;
                InclineZHor.Value = 0;
                InclineZVer.Value = 0;

                ProbeMarkShift = new Element<CatCoordinates>();
                ProbeMarkShift.Value = new CatCoordinates();
                ProbeMarkShift.Value.X.Value = 0;
                ProbeMarkShift.Value.Y.Value = 0;
                ProbeMarkShift.Value.Z.Value = 0;
                ProbeMarkShift.Value.T.Value = 0;

                UserProbeMarkShift = new Element<CatCoordinates>();
                UserProbeMarkShift.Value = new CatCoordinates();
                UserProbeMarkShift.Value.X.Value = 0;
                UserProbeMarkShift.Value.Y.Value = 0;
                UserProbeMarkShift.Value.Z.Value = 0;
                UserProbeMarkShift.Value.T.Value = 0;
                UserProbeMarkShift.UpperLimit = ProbeMarkShift.UpperLimit;
                UserProbeMarkShift.LowerLimit = ProbeMarkShift.LowerLimit;

                CPC = new Element<List<ChuckPlaneCompParameter>>();
                CPC.Value = new List<ChuckPlaneCompParameter>();
                CPC.Value.Add(new ChuckPlaneCompParameter(0, 0, 0, 0));
                CPC.UpperLimit = 30.0;
                CPC.LowerLimit = -30.0;

                ProbeTemperaturePositionTable = new Element<Dictionary<double, CatCoordinates>>();
                ProbeTemperaturePositionTable.Value = new Dictionary<double, CatCoordinates>();
                CatCoordinates coord1 = new CatCoordinates();
                ProbeTemperaturePositionTable.Value.Add(-10, coord1);

                CatCoordinates coord2 = new CatCoordinates();
                ProbeTemperaturePositionTable.Value.Add(90, coord2);

                TestingTimeout.Value = 3600;
                TestingTimeoutOnOff.Value = TestingTimeoutEnum.OFF;
                RepeatedAlarmTime.Value = 3600;
                RepeatedAlarmOnOff.Value = RepeatedAlarmEnum.OFF;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }



    }


    [Serializable]
    public class ProbingModuleDevParam : INotifyPropertyChanged, IParamNode, IDeviceParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
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
        public string FilePath { get; } = "ProbingModule";
        public string FileName { get; } = "ProbingModuleDevParam.Json";

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbingModuleDevParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public void SetElementMetaData()
        {
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                ZClearence.Value = -1000;
                OverDrive.Value = -100;
                OverdriveLowLimit.Value = -5000;
                OverdriveUpperLimit.Value = 200;
                IsEnableFirstContactToAllContactLimit.Value = false;
                IsEnableAllContactToOverdriveLimit.Value = false;
                IsEnableFirstContactToOverdriveLimit.Value = false;

                BeforeZupSoakingEnable.Value = false;
                BeforeZupSoakingClearanceZ.Value = -70;
                BeforeZupSoakingTime.Value = 60;
                AllowOutOfWaferContact.Value = false;

                IsEnableAutoZup.Value = false;
                BinFuncParam = new BinFunctionParam();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"[ProbingModuleDevParam] SetDefaultParam() : Error");
            }

            return RetVal;
        }

        private Element<double> _OverDrive
            = new Element<double>();
        public Element<double> OverDrive
        {
            get { return _OverDrive; }
            set
            {
                if (value != _OverDrive)
                {
                    _OverDrive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ZClearence
            = new Element<double>();
        public Element<double> ZClearence
        {
            get { return _ZClearence; }
            set
            {
                if (value != _ZClearence)
                {
                    _ZClearence = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsEnableAutoZup
             = new Element<bool>();
        public Element<bool> IsEnableAutoZup
        {
            get { return _IsEnableAutoZup; }
            set
            {
                if (value != _IsEnableAutoZup)
                {
                    _IsEnableAutoZup = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _FirstContactToAllContactLimitRange
            = new Element<double>();
        public Element<double> FirstContactToAllContactLimitRange
        {
            get { return _FirstContactToAllContactLimitRange; }
            set
            {
                if (value != _FirstContactToAllContactLimitRange)
                {
                    _FirstContactToAllContactLimitRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _AllContactToOverdriveLimitRange
            = new Element<double>();
        public Element<double> AllContactToOverdriveLimitRange
        {
            get { return _AllContactToOverdriveLimitRange; }
            set
            {
                if (value != _AllContactToOverdriveLimitRange)
                {
                    _AllContactToOverdriveLimitRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _FirstContactToOverdriveLimitRange
            = new Element<double>();
        public Element<double> FirstContactToOverdriveLimitRange
        {
            get { return _FirstContactToOverdriveLimitRange; }
            set
            {
                if (value != _FirstContactToOverdriveLimitRange)
                {
                    _FirstContactToOverdriveLimitRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OverdriveLowLimit
            = new Element<double>();
        public Element<double> OverdriveLowLimit
        {
            get { return _OverdriveLowLimit; }
            set
            {
                if (value != _OverdriveLowLimit)
                {
                    _OverdriveLowLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OverdriveLimit
           = new Element<double>() { Value = 200.0 };
        public Element<double> OverdriveUpperLimit
        {
            get 
            { return _OverdriveLimit; }
            set
            {
                if (value != _OverdriveLimit)
                {
                    _OverdriveLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ProbingXYOffSetLowLimit
            = new Element<double>() { Value = -150.0 };
        public Element<double> ProbingXYOffSetLowLimit
        {
            get
            { return _ProbingXYOffSetLowLimit; }
            set
            {
                if (value != _ProbingXYOffSetLowLimit)
                {
                    _ProbingXYOffSetLowLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ProbingXYOffSetUpperLimit
         = new Element<double>() { Value = 150.0 };
        public Element<double> ProbingXYOffSetUpperLimit
        {
            get
            { return _ProbingXYOffSetUpperLimit; }
            set
            {
                if (value != _ProbingXYOffSetUpperLimit)
                {
                    _ProbingXYOffSetUpperLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsEnableFirstContactToAllContactLimit
            = new Element<bool>();
        public Element<bool> IsEnableFirstContactToAllContactLimit
        {
            get { return _IsEnableFirstContactToAllContactLimit; }
            set
            {
                if (value != _IsEnableFirstContactToAllContactLimit)
                {
                    _IsEnableFirstContactToAllContactLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsEnableAllContactToOverdriveLimit
            = new Element<bool>();
        public Element<bool> IsEnableAllContactToOverdriveLimit
        {
            get { return _IsEnableAllContactToOverdriveLimit; }
            set
            {
                if (value != _IsEnableAllContactToOverdriveLimit)
                {
                    _IsEnableAllContactToOverdriveLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsEnableFirstContactToOverdriveLimit
            = new Element<bool>();
        public Element<bool> IsEnableFirstContactToOverdriveLimit
        {
            get { return _IsEnableFirstContactToOverdriveLimit; }
            set
            {
                if (value != _IsEnableFirstContactToOverdriveLimit)
                {
                    _IsEnableFirstContactToOverdriveLimit = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<bool> _AllowOutOfWaferContact
            = new Element<bool>();
        public Element<bool> AllowOutOfWaferContact
        {
            get { return _AllowOutOfWaferContact; }
            set
            {
                if (value != _AllowOutOfWaferContact)
                {
                    _AllowOutOfWaferContact = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _BeforeZupSoakingClearanceZ
    = new Element<double>();
        public Element<double> BeforeZupSoakingClearanceZ
        {
            get { return _BeforeZupSoakingClearanceZ; }
            set
            {
                if (value != _BeforeZupSoakingClearanceZ)
                {
                    _BeforeZupSoakingClearanceZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _BeforeZupSoakingTime
            = new Element<int>();
        public Element<int> BeforeZupSoakingTime
        {
            get { return _BeforeZupSoakingTime; }
            set
            {
                if (value != _BeforeZupSoakingTime)
                {
                    _BeforeZupSoakingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _BeforeZupSoakingEnable
            = new Element<bool>();
        public Element<bool> BeforeZupSoakingEnable
        {
            get { return _BeforeZupSoakingEnable; }
            set
            {
                if (value != _BeforeZupSoakingEnable)
                {
                    _BeforeZupSoakingEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BinFunctionParam _BinFuncParam = new BinFunctionParam();
        public BinFunctionParam BinFuncParam
        {
            get { return _BinFuncParam; }
            set
            {
                if (value != _BinFuncParam)
                {
                    _BinFuncParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PinPadMatchedTimeOut = new Element<int>();
        /// <summary>
        /// unit : sec
        /// </summary>
        public Element<int> PinPadMatchedTimeOut
        {
            get { return _PinPadMatchedTimeOut; }
            set
            {
                if (value != _PinPadMatchedTimeOut)
                {
                    _PinPadMatchedTimeOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MultipleContactCount : 컨택을 몇 번 반복하여 할 것인가 (0 ~ 10)
        /// 0: contact 한번
        /// 1: contact 한번 이후 next contact 1회 (총 2회 zup)
        /// 2: contact 한번 이후 next contact 2회 (총 3회 zup)
        /// </summary>
        private Element<int> _MultipleContactCount = new Element<int>();
        [System.Runtime.Serialization.DataMember]
        public Element<int> MultipleContactCount
        {
            get { return _MultipleContactCount; }
            set
            {
                if (value != _MultipleContactCount)
                {
                    _MultipleContactCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MultipleContactBackOD : 컨택 이후 내려올 높이 (~500 ~ 0)
        /// ZClearece 와는 다름, OD가 100이고 BackOD가 -70 이라면 30까지만 내려옴.
        /// </summary>
        private Element<double> _MultipleContactBackOD = new Element<double>();
        [System.Runtime.Serialization.DataMember]
        public Element<double> MultipleContactBackOD
        {
            get { return _MultipleContactBackOD; }
            set
            {
                if (value != _MultipleContactBackOD)
                {
                    _MultipleContactBackOD = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MultipleContactSpeed : Multiple Contact시에 Z축에 속도 (10 ~ 260000)
        /// </summary>
        private Element<double> _MultipleContactSpeed = new Element<double>() { Value = 10000 };
        [System.Runtime.Serialization.DataMember]
        public Element<double> MultipleContactSpeed
        {
            get { return _MultipleContactSpeed; }
            set
            {
                if (value != _MultipleContactSpeed)
                {
                    _MultipleContactSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MultipleContactAccel : Second Contact시에 Z축에 가속도 (10 ~ 2600000)
        /// </summary>
        private Element<double> _MultipleContactAccel = new Element<double>(){ Value = 100000 };
        [System.Runtime.Serialization.DataMember]
        public Element<double> MultipleContactAccel
        {
            get { return _MultipleContactAccel; }
            set
            {
                if (value != _MultipleContactAccel)
                {
                    _MultipleContactAccel = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MultipleContactOD : Second Contact시 반영되는 OD (~30 ~ 30)
        /// 0 : 첫번째 Contact에서 사용되는 높이만큼 똑같이 올라감.
        /// 30 : 첫번째 Contact에서의 OD가 100이라면 두번째 Contact시에는 130까지 올라감.
        /// </summary>
        private Element<double> _MultipleContactOD = new Element<double>();
        [System.Runtime.Serialization.DataMember]
        public Element<double> MultipleContactOD
        {
            get { return _MultipleContactOD; }
            set
            {
                if (value != _MultipleContactOD)
                {
                    _MultipleContactOD = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MultipleContactBackODOption : Second Contact 후 Back OD만큼 내려올 때 어느 포지션까지 내려올지에 대한 Option
        /// 0 : Probing OD 포지션부터 설정된 BackOD 값 만큼 내려옴. ex) OD: 100, BackOD: -70, 내려오는 높이: 30
        /// 1 : All Contact 포지션부터 설정된 BackOD 값 만큼 내려옴. ex) OD: 100, BackOD: -70, All Contact: 70, 내려오는 높이: 0
        /// 2 : First Contact 포지션부터 설정된 BackOD 값 만큼 내려옴. ex) OD: 100, BackOD: -70, First Contact 50, 내려오는 높이: -20
        /// </summary>
        private Element<MultipleContactBackODOptionEnum> _MultipleContactBackODOption = new Element<MultipleContactBackODOptionEnum>();
        [System.Runtime.Serialization.DataMember]
        public Element<MultipleContactBackODOptionEnum> MultipleContactBackODOption
        {
            get { return _MultipleContactBackODOption; }
            set
            {
                if (value != _MultipleContactBackODOption)
                {
                    _MultipleContactBackODOption = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MultipleContactDelayTime : Multiple Contact사이 DelayTime 설정.
        /// </summary>
        private Element<double> _MultipleContactDelayTime = new Element<double>() { Value = 0 };
        [System.Runtime.Serialization.DataMember]
        public Element<double> MultipleContactDelayTime
        {
            get { return _MultipleContactDelayTime; }
            set
            {
                if (value != _MultipleContactDelayTime)
                {
                    _MultipleContactDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class BinFunctionParam : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public BinFunctionParam()
        {
        }

        private Element<int> _ContinuousFailEnable = new Element<int>(0);
        [System.Runtime.Serialization.DataMember]
        public Element<int> ContinuousFailEnable
        {
            get { return _ContinuousFailEnable; }
            set
            {
                if (value != _ContinuousFailEnable)
                {
                    _ContinuousFailEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _BinAnyCountLimit = new Element<int>(0);
        [System.Runtime.Serialization.DataMember]
        public Element<int> BinAnyCountLimit
        {
            get { return _BinAnyCountLimit; }
            set
            {
                if (value != _BinAnyCountLimit)
                {
                    _BinAnyCountLimit = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class ChuckPlaneCompParameter : INotifyPropertyChanged, IChuckPlaneCompParameter
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        public ChuckPlaneCompParameter()
        {

        }
        public ChuckPlaneCompParameter(double position, double z0, double z1, double z2)
        {
            try
            {
                Position = position;
                Z0 = z0;
                Z1 = z1;
                Z2 = z2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private double _Z0;

        public double Z0
        {
            get { return _Z0; }
            set { _Z0 = value; }
        }
        private double _Z1;

        public double Z1
        {
            get { return _Z1; }
            set { _Z1 = value; }
        }
        private double _Z2;

        public double Z2
        {
            get { return _Z2; }
            set { _Z2 = value; }
        }

        private double _Position;

        public double Position
        {
            get { return _Position; }
            set { _Position = value; }
        }

        //public double Position;
        public override bool Equals(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return false;
                }

                ChuckPlaneCompParameter errorParam = obj as ChuckPlaneCompParameter;

                if (errorParam == null)
                {
                    return false;
                }

                return this.Position == errorParam.Position;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override int GetHashCode()
        {
            try
            {
                return this.Position.GetHashCode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

}
