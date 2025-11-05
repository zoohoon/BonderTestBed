using LogModule;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace ProberInterfaces
{
    [Serializable(), DataContract]
    public abstract class AxisObject : INotifyPropertyChanged, IParamNode, IRecipeEditorSelectAble
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        #region // Properties

        public List<object> Nodes { get; set; }

        private Element<EnumAxisConstants> _MasterAxis = new Element<EnumAxisConstants>();
        [DataMember]
        public Element<EnumAxisConstants> MasterAxis
        {
            get { return _MasterAxis; }
            set { _MasterAxis = value; }
        }

        [NonSerialized]
        private List<AxisObject> _GroupMembers = new List<AxisObject>();
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public List<AxisObject> GroupMembers
        {
            get { return _GroupMembers; }
            set { _GroupMembers = value; }
        }

        private Element<EnumAxisGroupType> _AxisGroupType = new Element<EnumAxisGroupType>();
        //[XmlAttribute("AxisGroupType")]
        [DataMember]
        public Element<EnumAxisGroupType> AxisGroupType
        {
            get { return _AxisGroupType; }
            set { _AxisGroupType = value; }
        }

        private Element<string> _Label = new Element<string>();
        //  [XmlAttribute(nameof(Label))]
        [DataMember]
        public Element<string> Label
        {
            get { return _Label; }
            set { _Label = value; }
        }

        private Element<int> _PortNum = new Element<int>();
        //   [XmlAttribute(nameof(PortNum))]
        [DataMember]
        public Element<int> PortNum
        {
            get { return _PortNum; }
            set
            {
                _PortNum = value;
                //mMotParams[0].mAxisIndex = mAxisIndex;
            }
        }

        private Element<int> _AxisIndex = new Element<int>();
        // [XmlAttribute("Index")]
        [DataMember]
        public Element<int> AxisIndex
        {
            get { return _AxisIndex; }
            set
            {
                _AxisIndex = value;
                //mMotParams[0].mAxisIndex = mAxisIndex;
            }
        }
        [XmlIgnore, JsonIgnore]
        private IHomingMethods _HomingMethod;
        //[DataMember]
        public IHomingMethods HomingMethod
        {
            get { return _HomingMethod; }
            set
            {
                if (value == _HomingMethod) return;
                _HomingMethod = value;
                NotifyPropertyChanged("HomingMethod");
            }
        }

        private Element<bool> _VerticalWithoutBreak = new Element<bool>();
        [DataMember]
        //  [XmlAttribute("VerticalWithoutBreak")]
        public Element<bool> VerticalWithoutBreak
        {
            get { return _VerticalWithoutBreak; }
            set
            {
                if (value == _VerticalWithoutBreak) return;
                _VerticalWithoutBreak = value;
                NotifyPropertyChanged("VerticalWithoutBreak");
            }
        }


        private Element<HomingMethodType> _HomingType = new Element<HomingMethodType>();
        //   [XmlAttribute("HomingType")]
        [DataMember]
        public Element<HomingMethodType> HomingType
        {
            get { return _HomingType; }
            set
            {
                if (value == _HomingType) return;
                _HomingType = value;
                NotifyPropertyChanged("HomingType");
            }
        }
        [XmlIgnore, JsonIgnore]
        private long _SettlingTime;
        //[XmlIgnore, JsonIgnore]
        [DataMember]
        public long SettlingTime
        {
            get { return _SettlingTime; }
            set
            {
                if (value != _SettlingTime)
                {
                    _SettlingTime = value;
                    NotifyPropertyChanged("SettlingTime");
                }
            }
        }

        private MotionParam mParam = new MotionParam();
        [DataMember]
        public MotionParam Param
        {
            get { return mParam; }
            set { mParam = value; }
        }
        //public ObservableCollection<MotionParam> ParamLists { get; set; }

        private AxisConfig _Config;
        [DataMember]
        public AxisConfig Config
        {
            get { return _Config; }
            set { _Config = value; }
        }
        [NonSerialized]
        private AxisStatus mStatus;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        [DataMember]
        public AxisStatus Status
        {
            get { return mStatus; }
            set
            {
                if (value == mStatus) return;
                mStatus = value;
                NotifyPropertyChanged("Status");
            }
        }

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
        #endregion
        public AxisObject()
        {
            try
            {
                mParam = new MotionParam();
                _Label.Value = "Undefined";
                _AxisIndex.Value = -1;
                _PortNum.Value = -1;
                mStatus = new AxisStatus();
                _Config = new AxisConfig();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public AxisObject(int portNum, int axisIndex)
        {
            try
            {
                mParam = new MotionParam();
                _Label.Value = "Undefined";
                _AxisIndex.Value = axisIndex;
                _PortNum.Value = portNum;
                mStatus = new AxisStatus();
                _Config = new AxisConfig(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public double SubDtoP(double distance)
        {
            double pulses = 0;
            try
            {
                if (Param.SubDtoPRatio.Value <= 0)
                {
                    Param.SubDtoPRatio.Value = 1.0;
                }
                pulses = Math.Round(distance * Param.SubDtoPRatio.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //LoggerManager.Error($"DtoP() Function error: " + err.Message);
            }

            return pulses;
        }
        public double DtoP(double distance)
        {
            double pulses = 0;
            try
            {
                if (Param.DtoPRatio.Value <= 0)
                {
                    Param.DtoPRatio.Value = 1.0;
                }
                pulses = Math.Round(distance * Param.DtoPRatio.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //LoggerManager.Error($"DtoP() Function error: " + err.Message);
            }

            return pulses;
        }

        public double SubPtoD(double pulses)
        {
            double dist = 0;
            try
            {
                if (Param.SubDtoPRatio.Value <= 0)
                {
                    Param.SubDtoPRatio.Value = 1.0;
                }
                dist = (double)pulses / Param.SubDtoPRatio.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"PtoD() Function error: " + err.Message);
            }

            return dist;
        }
        public double PtoD(double pulses)
        {
            double dist = 0;
            try
            {
                if (Param.DtoPRatio.Value <= 0)
                {
                    Param.DtoPRatio.Value = 1.0;
                }
                dist = (double)pulses / Param.DtoPRatio.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"PtoD() Function error: " + err.Message);
            }

            return dist;
        }

        private long _LockCount = 0;
        public void LockAxis()
        {
            Interlocked.Increment(ref _LockCount);
        }
        public void UnLockAxis()
        {
            try
            {
                if (_LockCount > 0)
                    Interlocked.Decrement(ref _LockCount);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public bool IsLock()
        {
            return Interlocked.Read(ref _LockCount) > 0;
        }
        public void ResetLock()
        {
            Interlocked.Exchange(ref _LockCount, 0);
        }

        //public void InitParamSet()
        //{
        //    ParamLists = new ObservableCollection<MotionParam>();
        //    ParamLists.Add(Param);
        //}
    }

    [Serializable(), DataContract]
    public class MotionParam : INotifyPropertyChanged, IParamNode
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<object> Nodes { get; set; }
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private Element<float> _FeedOverride = new Element<float>();
        [DataMember]
        public Element<float> FeedOverride
        {
            get { return _FeedOverride; }
            set
            {
                if (value != _FeedOverride)
                {
                    _FeedOverride = value;
                    NotifyPropertyChanged("FeedOverride");
                }
            }
        }

        private Element<double> mClearedPosition = new Element<double>();
        [DataMember]
        public Element<double> ClearedPosition
        {
            get { return mClearedPosition; }
            set
            {
                if (mClearedPosition == value) return;
                mClearedPosition = value;
                NotifyPropertyChanged("ClearedPosition");
            }
        }

        private Element<double> mHomeOffset = new Element<double>();
        [DataMember]
        public Element<double> HomeOffset
        {
            get { return mHomeOffset; }
            set
            {
                if (mHomeOffset == value) return;
                mHomeOffset = value;
                NotifyPropertyChanged("HomeOffset");
            }
        }

        private Element<double> _IndexSearchingSpeed = new Element<double>();
        [DataMember]
        public Element<double> IndexSearchingSpeed
        {
            get { return _IndexSearchingSpeed; }
            set
            {
                if (_IndexSearchingSpeed == value) return;
                _IndexSearchingSpeed = value;
                NotifyPropertyChanged("IndexSearchingSpeed");
            }
        }

        private Element<double> _HomeShift = new Element<double>();
        [DataMember]
        public Element<double> HomeShift
        {
            get { return _HomeShift; }
            set
            {
                if (_HomeShift == value) return;
                _HomeShift = value;
                NotifyPropertyChanged("HomeShift");
            }
        }

        private Element<bool> _HomeInvert = new Element<bool>();
        [DataMember]
        public Element<bool> HomeInvert
        {
            get { return _HomeInvert; }
            set
            {
                if (_HomeInvert == value) return;
                _HomeInvert = value;
                NotifyPropertyChanged("HomeInvert");
            }
        }
        private Element<bool> _IndexInvert = new Element<bool>();
        [DataMember]
        public Element<bool> IndexInvert
        {
            get { return _IndexInvert; }
            set
            {
                if (_IndexInvert == value) return;
                _IndexInvert = value;
                NotifyPropertyChanged("IndexInvert");
            }
        }




        private Element<double> mHommingSpeed = new Element<double>();
        [DataMember]
        public Element<double> HommingSpeed
        {
            get { return mHommingSpeed; }
            set
            {
                if (mHommingSpeed == value) return;
                mHommingSpeed = value;
                NotifyPropertyChanged("HommingSpeed");
            }
        }

        private Element<double> mHommingAcceleration = new Element<double>();
        [DataMember]
        public Element<double> HommingAcceleration
        {
            get { return mHommingAcceleration; }
            set
            {
                if (mHommingAcceleration == value) return;
                mHommingAcceleration = value;
                NotifyPropertyChanged("HommingAcceleration");
            }
        }
        private Element<double> mHommingDecceleration = new Element<double>();
        [DataMember]
        public Element<double> HommingDecceleration
        {
            get { return mHommingDecceleration; }
            set
            {
                if (mHommingDecceleration == value) return;
                mHommingDecceleration = value;
                NotifyPropertyChanged("HommingDecceleration");
            }
        }

        private Element<double> mFinalVelocity = new Element<double>();
        [DataMember]
        public Element<double> FinalVelociy
        {
            get { return mFinalVelocity; }
            set
            {
                if (mFinalVelocity == value) return;
                mFinalVelocity = value;
                NotifyPropertyChanged("FinalVelocity");
            }
        }

        private Element<double> mSpeed = new Element<double>();
        [DataMember]
        public Element<double> Speed
        {
            get { return mSpeed; }
            set
            {
                if (mSpeed == value) return;
                mSpeed = value;
                NotifyPropertyChanged("Speed");
            }
        }

        private Element<double> mAcceleration = new Element<double>();
        [DataMember]
        public Element<double> Acceleration
        {
            get { return mAcceleration; }
            set
            {
                if (mAcceleration == value) return;
                mAcceleration = value;
                NotifyPropertyChanged("Acceleration");
            }
        }

        private Element<double> mAccelerationJerk = new Element<double>();
        [DataMember]
        public Element<double> AccelerationJerk
        {
            get { return mAccelerationJerk; }
            set
            {
                if (mAccelerationJerk == value) return;
                mAccelerationJerk = value;
                NotifyPropertyChanged("AccelerationJerk");
            }
        }

        private Element<double> mDecceleration = new Element<double>();
        [DataMember]
        public Element<double> Decceleration
        {
            get { return mDecceleration; }
            set
            {
                if (mDecceleration == value) return;
                mDecceleration = value;
                NotifyPropertyChanged("Decceleration");
            }
        }

        private Element<double> mDeccelerationJerk = new Element<double>();
        [DataMember]
        public Element<double> DeccelerationJerk
        {
            get { return mDeccelerationJerk; }
            set
            {
                if (mDeccelerationJerk == value) return;
                mDeccelerationJerk = value;
                NotifyPropertyChanged("DeccelerationJerk");
            }
        }

        private Element<double> mSeqSpeed = new Element<double>();
        [DataMember]
        public Element<double> SeqSpeed
        {
            get { return mSeqSpeed; }
            set
            {
                if (mSeqSpeed == value) return;
                mSeqSpeed = value;
                NotifyPropertyChanged("SeqSpeed");
            }
        }

        private Element<double> mSeqAcc = new Element<double>();
        [DataMember]
        public Element<double> SeqAcc
        {
            get { return mSeqAcc; }
            set
            {
                if (mSeqAcc == value) return;
                mSeqAcc = value;
                NotifyPropertyChanged("SeqAcc");
            }
        }


        private Element<double> mSeqDcc = new Element<double>();
        [DataMember]
        public Element<double> SeqDcc
        {
            get { return mSeqDcc; }
            set
            {
                if (mSeqDcc == value) return;
                mSeqDcc = value;
                NotifyPropertyChanged("SeqDcc");
            }
        }

        private Element<double> mSubDtoPRatio = new Element<double>();
        [DataMember]
        public Element<double> SubDtoPRatio
        {
            get { return mSubDtoPRatio; }
            set
            {
                if (mSubDtoPRatio == value) return;
                mSubDtoPRatio = value;
                NotifyPropertyChanged("SubDtoPRatio");
            }
        }

        private Element<double> mDtoPRatio = new Element<double>();
        [DataMember]
        public Element<double> DtoPRatio
        {
            get { return mDtoPRatio; }
            set
            {
                if (mDtoPRatio == value) return;
                mDtoPRatio = value;
                NotifyPropertyChanged("DtoPRatio");
            }
        }

        private Element<double> mPulsePerRound = new Element<double>();
        [DataMember]
        public Element<double> PulsePerRound
        {
            get { return mPulsePerRound; }
            set
            {
                if (mPulsePerRound == value) return;
                mPulsePerRound = value;
                NotifyPropertyChanged("PulsePerRound");
            }
        }

        private Element<double> mStepDistance = new Element<double>();
        //[XmlIgnoreAttribute]
        [DataMember]
        public Element<double> StepDistance
        {
            get { return mStepDistance; }
            set
            {
                if (mStepDistance == value) return;
                mStepDistance = value;
                NotifyPropertyChanged("StepDistance");
            }
        }


        private Element<double> _NegSWLimit = new Element<double>();
        [DataMember]
        public Element<double> NegSWLimit
        {
            get { return _NegSWLimit; }
            set
            {
                if (_NegSWLimit == value) return;
                _NegSWLimit = value;
                NotifyPropertyChanged("NegSWLimit");
            }
        }

        private Element<double> _PosSWLimit = new Element<double>();
        [DataMember]
        public Element<double> PosSWLimit
        {
            get { return _PosSWLimit; }
            set
            {
                if (_PosSWLimit == value) return;
                _PosSWLimit = value;
                NotifyPropertyChanged("PosSWLimit");
            }
        }

        private Element<int> _POTIndex = new Element<int>();
        [DataMember]
        public Element<int> POTIndex
        {
            get { return _POTIndex; }
            set { _POTIndex = value; }
        }
        private Element<int> _NOTIndex = new Element<int>();
        [DataMember]
        public Element<int> NOTIndex
        {
            get { return _NOTIndex; }
            set { _NOTIndex = value; }
        }
        private Element<int> _HOMEIndex = new Element<int>();
        [DataMember]
        public Element<int> HOMEIndex
        {
            get { return _HOMEIndex; }
            set { _HOMEIndex = value; }
        }

        private Element<int> _TimeOut = new Element<int>();
        [DataMember]
        public Element<int> TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }
        private Element<int> _HomeDistLimit = new Element<int>();
        [DataMember]
        public Element<int> HomeDistLimit
        {
            get { return _HomeDistLimit; }
            set { _HomeDistLimit = value; }
        }
        private Element<int> _IndexDistLimit = new Element<int>();
        [DataMember]
        public Element<int> IndexDistLimit
        {
            get { return _IndexDistLimit; }
            set { _IndexDistLimit = value; }
        }


        [XmlIgnore, JsonIgnore]
        public ObservableCollection<double> AxisParamSet { get; set; }
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

        public MotionParam()
        {
            AxisParamSet = new ObservableCollection<double>();
        }

    }
    [Serializable(), DataContract]
    public class AxisConfig : INotifyPropertyChanged, IParamNode
    {
        //private static short FeedbackChannelCount = 2;
        public event PropertyChangedEventHandler PropertyChanged;
        public List<object> Nodes { get; set; }
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public AxisConfig()
        {
            try
            {
                //_FeedbackFaultAction = new ObservableCollection<EnumEventActionType>();
                //_FeedbackInvert = new ObservableCollection<bool>();

                //for (int fdbkIndex = 0; fdbkIndex < FeedbackChannelCount; fdbkIndex++)
                //{
                //    _FeedbackFaultAction.Add(new EnumEventActionType());
                //    _FeedbackInvert.Add(new bool());
                //}
                //_StepMotor = true;
                //_ClosedLoop = false;
                //_StopRate.Value = 0.05;
                //_EStopRate.Value = 0.025;
                //_Inposition.Value = 10;
                //_PosErrLimit.Value = 1100;
                //_PosErrLimitAction = EnumEventActionType.EventActionABORT;
                //_SWPosLimitAction = EnumEventActionType.EventActionNONE;
                //_SWNegLimitAction = EnumEventActionType.EventActionNONE;
                //_MotProviderType = EnumMotionProviderType.MPI;

                //_DualLoop = false;
                //_FeedbackFaultAction[0] = EnumEventActionType.EventActionABORT;
                //_FeedbackFaultAction[1] = EnumEventActionType.EventActionNONE;
                //_FeedbackInvert[0] = false;
                //_FeedbackInvert[1] = false;


                //_PIntMode.Value = 0;
                //_HWNegLimitAction = EnumEventActionType.EventActionNONE;
                //_HWNegLimitLevel = EnumInputLevel.Normal;
                //_HWPosLimitAction = EnumEventActionType.EventActionNONE;
                //_HWPosLimitLevel = EnumInputLevel.Normal;

                //_AmpFaultLevel = EnumInputLevel.Normal;
                //_AmpFaultAction = EnumEventActionType.EventActionABORT;
                //_AmpFaultDuration.Value = 0.1;
                //_HomeInputLevel = EnumInputLevel.Normal;
                //_HomeInputAction = EnumEventActionType.EventActionNONE;
                //_PulseAType = EnumPulseType.MotorStepperPulseTypeCW;
                //_PulseAInv = false;
                //_PulseBType = EnumPulseType.MotorStepperPulseTypeCCW;
                //_PulseBInv = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public AxisConfig(bool setdefault)
        {
            try
            {
                //_FeedbackFaultAction = new ObservableCollection<EnumEventActionType>();
                //_FeedbackInvert = new ObservableCollection<bool>();
                //if (setdefault == true)
                //{
                //    for (int fdbkIndex = 0; fdbkIndex < FeedbackChannelCount; fdbkIndex++)
                //    {
                //        _FeedbackFaultAction.Add(new EnumEventActionType());
                //        _FeedbackInvert.Add(new bool());
                //    }
                //}
                //_StepMotor = true;
                //_ClosedLoop = false;
                //_HomeConfig = EnumIndexConfig.HOME_ONLY;
                //_StopRate.Value = 0.05;
                //_EStopRate.Value = 0.025;
                //_Inposition.Value = 10;
                //_PosErrLimit.Value = 1100;
                //_PosErrLimitAction = EnumEventActionType.EventActionABORT;
                //_SWPosLimitAction = EnumEventActionType.EventActionNONE;
                //_SWNegLimitAction = EnumEventActionType.EventActionNONE;
                //_MotProviderType = EnumMotionProviderType.MPI;

                //_DualLoop = false;
                //_FeedbackFaultAction[0] = EnumEventActionType.EventActionABORT;
                //_FeedbackFaultAction[1] = EnumEventActionType.EventActionNONE;
                //_FeedbackInvert[0] = false;
                //_FeedbackInvert[1] = false;
                //_PIntMode.Value = 0;
                //_HWNegLimitAction = EnumEventActionType.EventActionNONE;
                //_HWNegLimitLevel = EnumInputLevel.Normal;
                //_HWPosLimitAction = EnumEventActionType.EventActionNONE;
                //_HWPosLimitLevel = EnumInputLevel.Normal;

                //_AmpFaultLevel = EnumInputLevel.Normal;
                //_AmpFaultAction = EnumEventActionType.EventActionABORT;
                ////_AmpFaultAction = EnumEventActionType.EventActionDONE;
                //_AmpFaultDuration.Value = 0.1;
                //_HomeInputLevel = EnumInputLevel.Normal;
                //_HomeInputAction = EnumEventActionType.EventActionNONE;
                //_PulseAType = EnumPulseType.MotorStepperPulseTypeCW;
                //_PulseAInv = false;
                //_PulseBType = EnumPulseType.MotorStepperPulseTypeCCW;
                //_PulseBInv = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private Element<double> _StopRate = new Element<double>();
        [DataMember]
        public Element<double> StopRate
        {
            get { return _StopRate; }
            set
            {
                if (_StopRate == value) return;
                _StopRate = value;
                NotifyPropertyChanged("StopRate");
            }
        }

        private Element<double> _EStopRate = new Element<double>();
        [DataMember]
        public Element<double> EStopRate
        {
            get { return _EStopRate; }
            set
            {
                if (_EStopRate == value) return;
                _EStopRate = value;
                NotifyPropertyChanged("EStopRate");
            }
        }

        private Element<double> _Inposition = new Element<double>();
        [DataMember]
        public Element<double> Inposition
        {
            get { return _Inposition; }
            set
            {
                if (_Inposition == value) return;
                _Inposition = value;
                NotifyPropertyChanged("Inposition");
            }
        }

        private Element<double> _NearTargetDistance = new Element<double>();
        [DataMember]
        public Element<double> NearTargetDistance
        {
            get { return _NearTargetDistance; }
            set
            {
                if (_NearTargetDistance == value) return;
                _NearTargetDistance = value;
                NotifyPropertyChanged("NearTargetDistance");
            }
        }
        private Element<double> _VelocityTolerance = new Element<double>();
        [DataMember]
        public Element<double> VelocityTolerance
        {
            get { return _VelocityTolerance; }
            set
            {
                if (_VelocityTolerance == value) return;
                _VelocityTolerance = value;
                NotifyPropertyChanged("VelocityTolerance");
            }
        }

        private Element<double> _SettlingTime = new Element<double>();
        [DataMember]
        public Element<double> SettlingTime
        {
            get { return _SettlingTime; }
            set
            {
                if (_SettlingTime == value) return;
                _SettlingTime = value;
                NotifyPropertyChanged("SettlingTime");
            }
        }

        private MoterTable _MoterConfig = new MoterTable();
        [DataMember]
        public MoterTable MoterConfig
        {
            get { return _MoterConfig; }
            set
            {
                if (value != _MoterConfig)
                {
                    _MoterConfig = value;
                    NotifyPropertyChanged("MoterConfig");
                }
            }
        }

        private Element<ControlLoopTypeEnum> _ControlType = new Element<ControlLoopTypeEnum>();
        [DataMember]
        public Element<ControlLoopTypeEnum> ControlType
        {
            get { return _ControlType; }
            set
            {
                if (value != _ControlType)
                {
                    _ControlType = value;
                    NotifyPropertyChanged("ControlType");
                }
            }
        }
        private Element<EnumDedicateInputs> _InputHome = new Element<EnumDedicateInputs>();
        [DataMember]
        public Element<EnumDedicateInputs> InputHome
        {
            get { return _InputHome; }
            set
            {
                if (value != _InputHome)
                {
                    _InputHome = value;
                    NotifyPropertyChanged("InputHome");
                }
            }
        }
        private Element<EnumDedicateInputs> _InputIndex = new Element<EnumDedicateInputs>();
        [DataMember]
        public Element<EnumDedicateInputs> InputIndex
        {
            get { return _InputIndex; }
            set
            {
                if (value != _InputIndex)
                {
                    _InputIndex = value;
                    NotifyPropertyChanged("InputIndex");
                }
            }
        }

        private Element<EnumMotorDedicatedIn> _InputMotor = new Element<EnumMotorDedicatedIn>();
        [DataMember]
        public Element<EnumMotorDedicatedIn> InputMotor
        {
            get { return _InputMotor; }
            set
            {
                if (value != _InputMotor)
                {
                    _InputMotor = value;
                    NotifyPropertyChanged("InputMotor");
                }
            }
        }

        private PIDGainTable _PIDCoeff = new PIDGainTable();
        [DataMember]
        public PIDGainTable PIDCoeff
        {
            get { return _PIDCoeff; }
            set
            {
                if (value != _PIDCoeff)
                {
                    _PIDCoeff = value;
                    NotifyPropertyChanged("PIDCoeff");
                }
            }
        }
        private PIVGainTable _PIVCoeff = new PIVGainTable();
        [DataMember]
        public PIVGainTable PIVCoeff
        {
            get { return _PIVCoeff; }
            set
            {
                if (value != _PIVCoeff)
                {
                    _PIVCoeff = value;
                    NotifyPropertyChanged("PIVCoeff");
                }
            }
        }

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
    }
    [Serializable, DataContract]
    public class MoterTable : INotifyPropertyChanged, IParamNode
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<object> Nodes { get; set; }
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public MoterTable()
        {
            try
            {
                //AmpEnable = false;
                //_FeedbackPhaseReverse.Add(false);
                //_FeedbackPhaseReverse.Add(false);
                //_EnableFeedbackFilter.Add(false);
                //_EnableFeedbackFilter.Add(false);
                //_FeedbackType.Add(EnumFeedbackType.QUAD_AB);
                //_FeedbackType.Add(EnumFeedbackType.QUAD_AB);
                FeedbackPhaseReverse.Value = new ObservableCollection<bool>();
                EnableFeedbackFilter.Value = new ObservableCollection<bool>();
                FeedbackType.Value = new ObservableCollection<EnumFeedbackType>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private Element<bool> _AmpEnable = new Element<bool>();
        [DataMember]
        public Element<bool> AmpEnable
        {
            get { return _AmpEnable; }
            set
            {
                if (value != _AmpEnable)
                {
                    _AmpEnable = value;
                    NotifyPropertyChanged("AmpEnable");
                }
            }
        }

        private Element<EnumMoterType> _MotorType = new Element<EnumMoterType>();
        [DataMember]
        public Element<EnumMoterType> MotorType
        {
            get { return _MotorType; }
            set
            {
                if (_MotorType == value) return;
                _MotorType = value;
                NotifyPropertyChanged("MotorType");
            }
        }
        private Element<EnumAmpActionType> _AmpDisableAction = new Element<EnumAmpActionType>();
        [DataMember]
        public Element<EnumAmpActionType> AmpDisableAction
        {
            get { return _AmpDisableAction; }
            set
            {
                if (value != _AmpDisableAction)
                {
                    _AmpDisableAction = value;
                    NotifyPropertyChanged("AmpDisableAction");
                }
            }
        }


        private Element<ObservableCollection<bool>> _FeedbackPhaseReverse = new Element<ObservableCollection<bool>>();
        [DataMember]
        public Element<ObservableCollection<bool>> FeedbackPhaseReverse
        {
            get { return _FeedbackPhaseReverse; }
            set
            {
                if (_FeedbackPhaseReverse == value) return;
                _FeedbackPhaseReverse = value;
                NotifyPropertyChanged("FeedbackPhaseReverse");
            }
        }
        private Element<ObservableCollection<bool>> _EnableFeedbackFilter = new Element<ObservableCollection<bool>>();
        [DataMember]
        public Element<ObservableCollection<bool>> EnableFeedbackFilter
        {
            get { return _EnableFeedbackFilter; }
            set
            {
                if (_EnableFeedbackFilter == value) return;
                _EnableFeedbackFilter = value;
                NotifyPropertyChanged("EnableFeedbackFilter");
            }
        }
        private Element<ObservableCollection<EnumFeedbackType>> _FeedbackType = new Element<ObservableCollection<EnumFeedbackType>>();
        [DataMember]
        public Element<ObservableCollection<EnumFeedbackType>> FeedbackType
        {
            get { return _FeedbackType; }
            set
            {
                if (_FeedbackType == value) return;
                _FeedbackType = value;
                NotifyPropertyChanged("FeedbackType");
            }
        }
        private Element<double> _AmpDisableDelay = new Element<double>();
        [DataMember]
        public Element<double> AmpDisableDelay
        {
            get { return _AmpDisableDelay; }
            set
            {
                if (_AmpDisableDelay == value) return;
                _AmpDisableDelay = value;
                NotifyPropertyChanged("AmpDisableDelay");
            }
        }

        private Element<double> _BrakeApplyDelay = new Element<double>();
        [DataMember]
        public Element<double> BrakeApplyDelay
        {
            get { return _BrakeApplyDelay; }
            set
            {
                if (_BrakeApplyDelay == value) return;
                _BrakeApplyDelay = value;
                NotifyPropertyChanged("BrakeApplyDelay");
            }
        }

        private Element<double> _BrakeReleaseDelay = new Element<double>();
        [DataMember]
        public Element<double> BrakeReleaseDelay
        {
            get { return _BrakeReleaseDelay; }
            set
            {
                if (_BrakeReleaseDelay == value) return;
                _BrakeReleaseDelay = value;
                NotifyPropertyChanged("BrakeReleaseDelay");
            }
        }


        private Element<bool> _EnableStepLoopBack = new Element<bool>();
        [DataMember]
        public Element<bool> EnableStepLoopBack
        {
            get { return _EnableStepLoopBack; }
            set
            {
                if (_EnableStepLoopBack == value) return;
                _EnableStepLoopBack = value;
                NotifyPropertyChanged("EnableStepLoopBack");
            }
        }

        private Element<double> _StepPulseWidth = new Element<double>();
        [DataMember]
        public Element<double> StepPulseWidth
        {
            get { return _StepPulseWidth; }
            set
            {
                if (_StepPulseWidth == value) return;
                _StepPulseWidth = value;
                NotifyPropertyChanged("StepPulseWidth");
            }
        }


        private Element<EnumPulseType> _PulseAType = new Element<EnumPulseType>();
        [DataMember]
        public Element<EnumPulseType> PulseAType
        {
            get { return _PulseAType; }
            set
            {
                if (_PulseAType == value) return;
                _PulseAType = value;
                NotifyPropertyChanged("PulseAType");
            }
        }

        private Element<EnumPulseType> _PulseBType = new Element<EnumPulseType>();
        [DataMember]
        public Element<EnumPulseType> PulseBType
        {
            get { return _PulseBType; }
            set
            {
                if (_PulseBType == value) return;
                _PulseBType = value;
                NotifyPropertyChanged("PulseBType");
            }
        }


        private Element<bool> _PulseAInv = new Element<bool>();
        [DataMember]
        public Element<bool> PulseAInv
        {
            get { return _PulseAInv; }
            set
            {
                if (_PulseAInv == value) return;
                _PulseAInv = value;
                NotifyPropertyChanged("PulseAInv");
            }
        }

        private Element<bool> _PulseBInv = new Element<bool>();
        [DataMember]
        public Element<bool> PulseBInv
        {
            get { return _PulseBInv; }
            set
            {
                if (_PulseBInv == value) return;
                _PulseBInv = value;
                NotifyPropertyChanged("PulseBInv");
            }
        }


        private Element<bool> _AmpFaultTrigHigh = new Element<bool>();
        [DataMember]
        public Element<bool> AmpFaultTrigHigh
        {
            get { return _AmpFaultTrigHigh; }
            set
            {
                if (_AmpFaultTrigHigh == value) return;
                _AmpFaultTrigHigh = value;
                NotifyPropertyChanged("AmpFaultTrigHigh");
            }
        }

        private Element<EnumEventActionType> _AmpFaultAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> AmpFaultAction
        {
            get { return _AmpFaultAction; }
            set
            {
                if (_AmpFaultAction == value) return;
                _AmpFaultAction = value;
                NotifyPropertyChanged("AmpFaultAction");
            }
        }
        private Element<double> _AmpFaultDuration = new Element<double>();
        [DataMember]
        public Element<double> AmpFaultDuration
        {
            get { return _AmpFaultDuration; }
            set
            {
                if (_AmpFaultDuration == value) return;
                _AmpFaultDuration = value;
                NotifyPropertyChanged("AmpFaultDuration");
            }
        }

        private Element<bool> _AmpWarningTrigHigh = new Element<bool>();
        [DataMember]

        public Element<bool> AmpWarningTrigHigh
        {
            get { return _AmpWarningTrigHigh; }
            set
            {
                if (_AmpWarningTrigHigh == value) return;
                _AmpWarningTrigHigh = value;
                NotifyPropertyChanged("AmpWarningTrigHigh");
            }
        }

        private Element<EnumEventActionType> _AmpWarningAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> AmpWarningAction
        {
            get { return _AmpWarningAction; }
            set
            {
                if (_AmpWarningAction == value) return;
                _AmpWarningAction = value;
                NotifyPropertyChanged("AmpWarningAction");
            }
        }

        private Element<double> _AmpWarningDuration = new Element<double>();
        [DataMember]
        public Element<double> AmpWarningDuration
        {
            get { return _AmpWarningDuration; }
            set
            {
                if (_AmpWarningDuration == value) return;
                _AmpWarningDuration = value;
                NotifyPropertyChanged("AmpWarningDuration");
            }
        }

        private Element<int> _ErrorLimitTrigger = new Element<int>();
        [DataMember]
        public Element<int> ErrorLimitTrigger
        {
            get { return _ErrorLimitTrigger; }
            set
            {
                if (_ErrorLimitTrigger == value) return;
                _ErrorLimitTrigger = value;
                NotifyPropertyChanged("ErrorLimitTrigger");
            }
        }



        private Element<EnumEventActionType> _ErrorLimitAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> ErrorLimitAction
        {
            get { return _ErrorLimitAction; }
            set
            {
                if (_ErrorLimitAction == value) return;
                _ErrorLimitAction = value;
                NotifyPropertyChanged("ErrorLimitAction");
            }
        }

        private Element<double> _ErrorLimitDuration = new Element<double>();
        [DataMember]
        public Element<double> ErrorLimitDuration
        {
            get { return _ErrorLimitDuration; }
            set
            {
                if (_ErrorLimitDuration == value) return;
                _ErrorLimitDuration = value;
                NotifyPropertyChanged("ErrorLimitDuration");
            }
        }



        private Element<double> _TorqueLimitTrigger = new Element<double>();
        [DataMember]
        public Element<double> TorqueLimitTrigger
        {
            get { return _TorqueLimitTrigger; }
            set
            {
                if (_TorqueLimitTrigger == value) return;
                _TorqueLimitTrigger = value;
                NotifyPropertyChanged("TorqueLimitTrigger");
            }
        }



        private Element<EnumEventActionType> _TorqueLimitAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> TorqueLimitAction
        {
            get { return _TorqueLimitAction; }
            set
            {
                if (_TorqueLimitAction == value) return;
                _TorqueLimitAction = value;
                NotifyPropertyChanged("TorqueLimitAction");
            }
        }

        private Element<double> _TorqueLimitDuration = new Element<double>();
        [DataMember]
        public Element<double> TorqueLimitDuration
        {
            get { return _TorqueLimitDuration; }
            set
            {
                if (_TorqueLimitDuration == value) return;
                _TorqueLimitDuration = value;
                NotifyPropertyChanged("TorqueLimitDuration");
            }
        }



        private Element<bool> _HWNegLimitTrigHigh = new Element<bool>();
        [DataMember]

        public Element<bool> HWNegLimitTrigHigh
        {
            get { return _HWNegLimitTrigHigh; }
            set
            {
                if (_HWNegLimitTrigHigh == value) return;
                _HWNegLimitTrigHigh = value;
                NotifyPropertyChanged("HWNegLimitTrigHigh");
            }
        }

        private Element<EnumEventActionType> _HWNegLimitAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> HWNegLimitAction
        {
            get { return _HWNegLimitAction; }
            set
            {
                if (_HWNegLimitAction == value) return;
                _HWNegLimitAction = value;
                NotifyPropertyChanged("HWNegLimitAction");
            }
        }

        private Element<double> _HWNegLimitDuration = new Element<double>();
        [DataMember]
        public Element<double> HWNegLimitDuration
        {
            get { return _HWNegLimitDuration; }
            set
            {
                if (_HWNegLimitDuration == value) return;
                _HWNegLimitDuration = value;
                NotifyPropertyChanged("HWNegLimitDuration");
            }
        }


        private Element<bool> _HWPosLimitTrigHigh = new Element<bool>();
        [DataMember]

        public Element<bool> HWPosLimitTrigHigh
        {
            get { return _HWPosLimitTrigHigh; }
            set
            {
                if (_HWPosLimitTrigHigh == value) return;
                _HWPosLimitTrigHigh = value;
                NotifyPropertyChanged("HWPosLimitTrigHigh");
            }
        }

        private Element<EnumEventActionType> _HWPosLimitAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> HWPosLimitAction
        {
            get { return _HWPosLimitAction; }
            set
            {
                if (_HWPosLimitAction == value) return;
                _HWPosLimitAction = value;
                NotifyPropertyChanged("HWPosLimitAction");
            }
        }

        private Element<double> _HWPosLimitDuration = new Element<double>();
        [DataMember]
        public Element<double> HWPosLimitDuration
        {
            get { return _HWPosLimitDuration; }
            set
            {
                if (_HWPosLimitDuration == value) return;
                _HWPosLimitDuration = value;
                NotifyPropertyChanged("HWPosLimitDuration");
            }
        }


        private Element<double> _SWNegLimitTrigger = new Element<double>();
        [DataMember]
        public Element<double> SWNegLimitTrigger
        {
            get { return _SWNegLimitTrigger; }
            set
            {
                if (_SWNegLimitTrigger == value) return;
                _SWNegLimitTrigger = value;
                NotifyPropertyChanged("SWNegLimitTrigger");
            }
        }


        private Element<EnumEventActionType> _SWNegLimitAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> SWNegLimitAction
        {
            get { return _SWNegLimitAction; }
            set
            {
                if (_SWNegLimitAction == value) return;
                _SWNegLimitAction = value;
                NotifyPropertyChanged("SWNegLimitAction");
            }
        }

        private Element<double> _SWPosLimitTrigger = new Element<double>();
        [DataMember]
        public Element<double> SWPosLimitTrigger
        {
            get { return _SWPosLimitTrigger; }
            set
            {
                if (_SWPosLimitTrigger == value) return;
                _SWPosLimitTrigger = value;
                NotifyPropertyChanged("SWPosLimitTrigger");
            }
        }


        private Element<EnumEventActionType> _SWPosLimitAction = new Element<EnumEventActionType>();
        [DataMember]
        public Element<EnumEventActionType> SWPosLimitAction
        {
            get { return _SWPosLimitAction; }
            set
            {
                if (_SWPosLimitAction == value) return;
                _SWPosLimitAction = value;
                NotifyPropertyChanged("SWPosLimitAction");
            }
        }
        private Element<ObservableCollection<double>> _EncRatioA = new Element<ObservableCollection<double>>();
        [DataMember]
        public Element<ObservableCollection<double>> EncRatioA
        {
            get { return _EncRatioA; }
            set
            {
                if (_EncRatioA == value) return;
                _EncRatioA = value;
                NotifyPropertyChanged("EncRatioA");
            }
        }


        private Element<ObservableCollection<double>> _EncRatioB = new Element<ObservableCollection<double>>();
        [DataMember]
        public Element<ObservableCollection<double>> EncRatioB
        {
            get { return _EncRatioB; }
            set
            {
                if (_EncRatioB == value) return;
                _EncRatioB = value;
                NotifyPropertyChanged("EncRatioB");
            }
        }

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
    }
    [Serializable, DataContract]
    public class PIDGainTable : INotifyPropertyChanged, IParamNode
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<object> Nodes { get; set; }
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private Element<double> _GainProportional = new Element<double>();
        [DataMember]
        public Element<double> GainProportional
        {
            get { return _GainProportional; }
            set
            {
                if (value != _GainProportional)
                {
                    _GainProportional = value;
                    NotifyPropertyChanged("GainProportional");
                }
            }
        }
        private Element<double> _GainIntegral = new Element<double>();
        [DataMember]
        public Element<double> GainIntegral
        {
            get { return _GainIntegral; }
            set
            {
                if (value != _GainIntegral)
                {
                    _GainIntegral = value;
                    NotifyPropertyChanged("GainIntegral");
                }
            }
        }
        private Element<double> _GainDerivative = new Element<double>();
        [DataMember]
        public Element<double> GainDerivative
        {
            get { return _GainDerivative; }
            set
            {
                if (value != _GainDerivative)
                {
                    _GainDerivative = value;
                    NotifyPropertyChanged("GainDerivative");
                }
            }
        }

        private Element<double> _FeedForwardPosition = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardPosition
        {
            get { return _FeedForwardPosition; }
            set
            {
                if (value != _FeedForwardPosition)
                {
                    _FeedForwardPosition = value;
                    NotifyPropertyChanged("FeedForwardPosition");
                }
            }
        }
        private Element<double> _FeedForwardVelocity = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardVelocity
        {
            get { return _FeedForwardVelocity; }
            set
            {
                if (value != _FeedForwardVelocity)
                {
                    _FeedForwardVelocity = value;
                    NotifyPropertyChanged("FeedForwardVelocity");
                }
            }
        }

        private Element<double> _FeedForwardAcceleration = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardAcceleration
        {
            get { return _FeedForwardAcceleration; }
            set
            {
                if (value != _FeedForwardAcceleration)
                {
                    _FeedForwardAcceleration = value;
                    NotifyPropertyChanged("FeedForwardAcceleration");
                }
            }
        }
        private Element<double> _FeedForwardFriction = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardFriction
        {
            get { return _FeedForwardFriction; }
            set
            {
                if (value != _FeedForwardFriction)
                {
                    _FeedForwardFriction = value;
                    NotifyPropertyChanged("FeedForwardFriction");
                }
            }
        }
        private Element<double> _DRate = new Element<double>();
        [DataMember]
        public Element<double> DRate
        {
            get { return _DRate; }
            set
            {
                if (value != _DRate)
                {
                    _DRate = value;
                    NotifyPropertyChanged("DRate");
                }
            }
        }
        private Element<double> _IntegrationMaxMoving = new Element<double>();
        [DataMember]
        public Element<double> IntegrationMaxMoving
        {
            get { return _IntegrationMaxMoving; }
            set
            {
                if (value != _IntegrationMaxMoving)
                {
                    _IntegrationMaxMoving = value;
                    NotifyPropertyChanged("IntegrationMaxMoving");
                }
            }
        }
        private Element<double> _IntegrationMaxRest = new Element<double>();
        [DataMember]
        public Element<double> IntegrationMaxRest
        {
            get { return _IntegrationMaxRest; }
            set
            {
                if (value != _IntegrationMaxRest)
                {
                    _IntegrationMaxRest = value;
                    NotifyPropertyChanged("IntegrationMaxRest");
                }
            }
        }
        private Element<double> _OutputLimitHigh = new Element<double>();
        [DataMember]
        public Element<double> OutputLimitHigh
        {
            get { return _OutputLimitHigh; }
            set
            {
                if (value != _OutputLimitHigh)
                {
                    _OutputLimitHigh = value;
                    NotifyPropertyChanged("OutputLimitHigh");
                }
            }
        }
        private Element<double> _OutputLimtLow = new Element<double>();
        [DataMember]
        public Element<double> OutputLimtLow
        {
            get { return _OutputLimtLow; }
            set
            {
                if (value != _OutputLimtLow)
                {
                    _OutputLimtLow = value;
                    NotifyPropertyChanged("OutputLimtLow");
                }
            }
        }
        private Element<double> _OutputVelocityLimitHigh = new Element<double>();
        [DataMember]
        public Element<double> OutputVelocityLimitHigh
        {
            get { return _OutputVelocityLimitHigh; }
            set
            {
                if (value != _OutputVelocityLimitHigh)
                {
                    _OutputVelocityLimitHigh = value;
                    NotifyPropertyChanged("OutputVelocityLimitHigh");
                }
            }
        }
        private Element<double> _OutputVelocityLimitLow = new Element<double>();
        [DataMember]
        public Element<double> OutputVelocityLimitLow
        {
            get { return _OutputVelocityLimitLow; }
            set
            {
                if (value != _OutputVelocityLimitLow)
                {
                    _OutputVelocityLimitLow = value;
                    NotifyPropertyChanged("OutputVelocityLimitLow");
                }
            }
        }
        private Element<double> _OutputOffset = new Element<double>();
        [DataMember]
        public Element<double> OutputOffset
        {
            get { return _OutputOffset; }
            set
            {
                if (value != _OutputOffset)
                {
                    _OutputOffset = value;
                    NotifyPropertyChanged("OutputOffset");
                }
            }
        }
        private Element<double> _NoisePositionFFT = new Element<double>();
        [DataMember]
        public Element<double> NoisePositionFFT
        {
            get { return _NoisePositionFFT; }
            set
            {
                if (value != _NoisePositionFFT)
                {
                    _NoisePositionFFT = value;
                    NotifyPropertyChanged("NoisePositionFFT");
                }
            }
        }

        public string Genealogy { get; set; }
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
    }
    [Serializable, DataContract]
    public class PIVGainTable : INotifyPropertyChanged, IParamNode
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<object> Nodes { get; set; }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
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

        private Element<double> _GainPositionProportional = new Element<double>();
        [DataMember]
        public Element<double> GainPositionProportional
        {
            get { return _GainPositionProportional; }
            set
            {
                if (value != _GainPositionProportional)
                {
                    _GainPositionProportional = value;
                    NotifyPropertyChanged("GainPositionProportional");
                }
            }
        }
        private Element<double> _GainPositionIntegral = new Element<double>();
        [DataMember]
        public Element<double> GainPositionIntegral
        {
            get { return _GainPositionIntegral; }
            set
            {
                if (value != _GainPositionIntegral)
                {
                    _GainPositionIntegral = value;
                    NotifyPropertyChanged("GainPositionIntegral");
                }
            }
        }

        private Element<double> _GainVelocityProportional = new Element<double>();
        [DataMember]
        public Element<double> GainVelocityProportional
        {
            get { return _GainVelocityProportional; }
            set
            {
                if (value != _GainVelocityProportional)
                {
                    _GainVelocityProportional = value;
                    NotifyPropertyChanged("GainVelocityProportional");
                }
            }
        }

        private Element<double> _GainVelocityIntegral = new Element<double>();
        [DataMember]
        public Element<double> GainVelocityIntegral
        {
            get { return _GainVelocityIntegral; }
            set
            {
                if (value != _GainVelocityIntegral)
                {
                    _GainVelocityIntegral = value;
                    NotifyPropertyChanged("GainVelocityIntegral");
                }
            }
        }

        private Element<double> _FeedForwardPosition = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardPosition
        {
            get { return _FeedForwardPosition; }
            set
            {
                if (value != _FeedForwardPosition)
                {
                    _FeedForwardPosition = value;
                    NotifyPropertyChanged("FeedForwardPosition");
                }
            }
        }

        private Element<double> _FeedForwardVelocity = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardVelocity
        {
            get { return _FeedForwardVelocity; }
            set
            {
                if (value != _FeedForwardVelocity)
                {
                    _FeedForwardVelocity = value;
                    NotifyPropertyChanged("FeedForwardVelocity");
                }
            }
        }

        private Element<double> _FeedForwardAcceleration = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardAcceleration
        {
            get { return _FeedForwardAcceleration; }
            set
            {
                if (value != _FeedForwardAcceleration)
                {
                    _FeedForwardAcceleration = value;
                    NotifyPropertyChanged("FeedForwardAcceleration");
                }
            }
        }

        private Element<double> _FeedForwardFriction = new Element<double>();
        [DataMember]
        public Element<double> FeedForwardFriction
        {
            get { return _FeedForwardFriction; }
            set
            {
                if (value != _FeedForwardFriction)
                {
                    _FeedForwardFriction = value;
                    NotifyPropertyChanged("FeedForwardFriction");
                }
            }
        }

        private Element<double> _Smoothing = new Element<double>();
        [DataMember]
        public Element<double> Smoothing
        {
            get { return _Smoothing; }
            set
            {
                if (value != _Smoothing)
                {
                    _Smoothing = value;
                    NotifyPropertyChanged("Smoothing");
                }
            }
        }


        private Element<double> _IntegrationMaxMoving = new Element<double>();
        [DataMember]
        public Element<double> IntegrationMaxMoving
        {
            get { return _IntegrationMaxMoving; }
            set
            {
                if (value != _IntegrationMaxMoving)
                {
                    _IntegrationMaxMoving = value;
                    NotifyPropertyChanged("IntegrationMaxMoving");
                }
            }
        }

        private Element<double> _IntegrationMaxRest = new Element<double>();
        [DataMember]
        public Element<double> IntegrationMaxRest
        {
            get { return _IntegrationMaxRest; }
            set
            {
                if (value != _IntegrationMaxRest)
                {
                    _IntegrationMaxRest = value;
                    NotifyPropertyChanged("IntegrationMaxRest");
                }
            }
        }

        private Element<double> _IntegrationMaxVelocityIntegrationMax = new Element<double>();
        [DataMember]
        public Element<double> IntegrationMaxVelocityIntegrationMax
        {
            get { return _IntegrationMaxVelocityIntegrationMax; }
            set
            {
                if (value != _IntegrationMaxVelocityIntegrationMax)
                {
                    _IntegrationMaxVelocityIntegrationMax = value;
                    NotifyPropertyChanged("IntegrationMaxVelocityIntegrationMax");
                }
            }
        }
        private Element<double> _OutputLimitHigh = new Element<double>();
        [DataMember]
        public Element<double> OutputLimitHigh
        {
            get { return _OutputLimitHigh; }
            set
            {
                if (value != _OutputLimitHigh)
                {
                    _OutputLimitHigh = value;
                    NotifyPropertyChanged("OutputLimitHigh");
                }
            }
        }
        private Element<double> _OutputLimtLow = new Element<double>();
        [DataMember]
        public Element<double> OutputLimtLow
        {
            get { return _OutputLimtLow; }
            set
            {
                if (value != _OutputLimtLow)
                {
                    _OutputLimtLow = value;
                    NotifyPropertyChanged("OutputLimtLow");
                }
            }
        }


        private Element<double> _OutputOffset = new Element<double>();
        [DataMember]
        public Element<double> OutputOffset
        {
            get { return _OutputOffset; }
            set
            {
                if (value != _OutputOffset)
                {
                    _OutputOffset = value;
                    NotifyPropertyChanged("OutputOffset");
                }
            }
        }
        private Element<double> _NoiseFilterFFT = new Element<double>();
        [DataMember]
        public Element<double> NoiseFilterFFT
        {
            get { return _NoiseFilterFFT; }
            set
            {
                if (value != _NoiseFilterFFT)
                {
                    _NoiseFilterFFT = value;
                    NotifyPropertyChanged("NoiseFilterFFT");
                }
            }
        }


    }
    public enum ControlLoopTypeEnum
    {
        UNDEFINED = -1,
        UNUSED = 0,
        PID = 1,
        PIV = 2
    }
    public delegate void PositionUpdatedDelegate();
    [DataContract]
    public class Positions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public PositionUpdatedDelegate Updated;

        private double _Actual;
        [DataMember]
        public double Actual
        {
            get { return _Actual; }
            set
            {
                if (value != this._Actual)
                {
                    _Actual = value;
                    if (Updated != null) Updated();
                    NotifyPropertyChanged("Actual");
                }
            }
        }
        private double _Command;
        [DataMember]
        public double Command
        {
            get { return _Command; }
            set
            {
                if (value != this._Command)
                {
                    _Command = value;
                    NotifyPropertyChanged("Command");
                }
            }
        }
        private double _Error;
        [DataMember]
        public double Error
        {
            get { return _Error; }
            set
            {
                if (value != _Error)
                {
                    _Error = value;
                    NotifyPropertyChanged("Error");
                }
            }
        }
        private double _Velocity;
        [DataMember]
        public double Velocity
        {
            get { return _Velocity; }
            set
            {
                if (value != _Velocity)
                {
                    _Velocity = value;
                    NotifyPropertyChanged("Velocity");
                }
            }
        }
        private double _Ref;
        [DataMember]
        public double Ref
        {
            get { return _Ref; }
            set
            {
                if (value != _Ref)
                {
                    if (value == double.NaN)
                    {

                    }
                    _Ref = value;
                    NotifyPropertyChanged("Ref");
                }
            }
        }

        private double _Comp;
        [DataMember]
        public double Comp
        {
            get { return _Comp; }
            set
            {
                if (value != _Comp)
                {
                    if (value == double.NaN)
                    {

                    }
                    _Comp = value;
                    NotifyPropertyChanged("Comp");
                }
            }
        }


    }
    [DataContract]
    public class CaptureStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private CaptureState _CaptureState;
        [DataMember]
        public CaptureState CaptureState
        {
            get { return _CaptureState; }
            set
            {
                if (value != _CaptureState)
                {
                    _CaptureState = value;
                    NotifyPropertyChanged("CaptureState");
                }
            }
        }
        private double _LatchedValue;
        [DataMember]
        public double LatchedValue
        {
            get { return _LatchedValue; }
            set
            {
                if (value != _LatchedValue)
                {
                    _LatchedValue = value;
                    NotifyPropertyChanged("LatchedValue");
                }
            }
        }


    }
    [DataContract]
    public class AxisStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region //Properties
        public AxisStatus()
        {
            try
            {
                Pulse = new Positions();
                Position = new Positions();
                RawPosition = new Positions();
                MotionCaptureStatus = new CaptureStatus();
                CapturePositions = new List<double>();
                Torque = 0.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private bool _IsHomeSensor = false;
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public bool IsHomeSensor
        {
            get { return _IsHomeSensor; }
            set
            {
                if (value != _IsHomeSensor)
                {
                    _IsHomeSensor = value;
                    NotifyPropertyChanged("IsHomeSensor");
                }
            }
        }

        private bool _IsLimitSensor = false;
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public bool IsLimitSensor
        {
            get { return _IsLimitSensor; }
            set
            {
                if (value != _IsLimitSensor)
                {
                    _IsLimitSensor = value;
                    NotifyPropertyChanged("IsLimitSensor");
                }
            }
        }

        private bool _IsHoming = false;
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public bool IsHoming
        {
            get { return _IsHoming; }
            set
            {
                if (value != _IsHoming)
                {
                    _IsHoming = value;
                    NotifyPropertyChanged("IsHoming");
                }
            }
        }

        private bool _IsHomeSeted = false;
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public bool IsHomeSeted
        {
            get { return _IsHomeSeted; }
            set
            {
                if (value != _IsHomeSeted)
                {
                    _IsHomeSeted = value;
                    NotifyPropertyChanged("IsHomeSeted");
                }
            }
        }
        private EnumAxisState _State;
        [DataMember]
        public EnumAxisState State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }
        private CaptureStatus _MotionCaptureStatus;
        [DataMember]
        public CaptureStatus MotionCaptureStatus
        {
            get { return _MotionCaptureStatus; }
            set
            {
                if (value != _MotionCaptureStatus)
                {
                    _MotionCaptureStatus = value;
                    NotifyPropertyChanged("MotionCaptureStatus");
                }
            }
        }

        private List<Double> _CapturePositions;
        [DataMember]
        [XmlIgnore, JsonIgnore]
        public List<Double> CapturePositions
        {
            get { return _CapturePositions; }
            set
            {
                if (value != _CapturePositions)
                {
                    _CapturePositions = value;
                    NotifyPropertyChanged("CapturePositions");
                }
            }
        }

        /// <summary>
        /// Torque값 산출 식 -> 해당 모터의 (Actual Current/Continuous Current)*1000
        ///     Actual Current -> 축의 Active Current값 (Elmo프로그램에 Motion-Maestro Axes에서 볼 수 있음)
        ///     Continuous Current -> 축의 Peak Current값 (= Continuous Stall Current*1.414), Z0, Z1, Z2축은 2.12임. (이 값도 Elmo프로그램에서 확인 가능)
        ///     1000 -> PerMile단위. 천분율
        /// </summary>
        private double _Torque;
        [DataMember]
        [XmlIgnore, JsonIgnore]
        public double Torque
        {
            get { return _Torque; }
            set
            {
                if (_Torque == value) return;
                _Torque = value;
                NotifyPropertyChanged("Torque");
            }
        }

        private EnumEventActionType _Action;
        [DataMember]
        public EnumEventActionType Action
        {
            get { return _Action; }
            set
            {
                if (value != _Action)
                {
                    _Action = value;
                    NotifyPropertyChanged("Action");
                }
            }
        }

        private long _EventMask;
        [DataMember]
        public long EventMask
        {
            get { return _EventMask; }
            set
            {
                if (value != _EventMask)
                {
                    _EventMask = value;
                    NotifyPropertyChanged("EventMask");
                }
            }
        }

        private int _Settled;
        [DataMember]
        public int Settled
        {
            get { return _Settled; }
            set
            {
                if (value != _Settled)
                {
                    _Settled = value;
                    NotifyPropertyChanged("Settled");
                }
            }
        }

        private int _AtTarget;
        [DataMember]
        public int AtTarget
        {
            get { return _AtTarget; }
            set
            {
                if (value != _AtTarget)
                {
                    _AtTarget = value;
                    NotifyPropertyChanged("AtTarget");
                }
            }
        }

        private EnumAxisActionSource _ActionSource;
        [DataMember]
        public EnumAxisActionSource ActionSource
        {
            get { return _ActionSource; }
            set
            {
                if (value != _ActionSource)
                {
                    _ActionSource = value;
                    NotifyPropertyChanged("ActionSource");
                }
            }
        }

        private Positions _RawPosition;
        [DataMember]
        public Positions RawPosition
        {
            get { return _RawPosition; }
            set
            {
                if (value != _RawPosition)
                {
                    _RawPosition = value;
                    NotifyPropertyChanged("RawPosition");
                }
            }
        }

        private Positions _Position;
        [DataMember]
        public Positions Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                    NotifyPropertyChanged("Position");
                }
            }
        }
        private Positions _Pulse;
        [DataMember]
        public Positions Pulse
        {
            get { return _Pulse; }
            set
            {
                if (value != _Pulse)
                {
                    _Pulse = value;
                    NotifyPropertyChanged("Pulse");
                }
            }
        }
        private UInt32 mErrCode;
        [DataMember]

        public UInt32 ErrCode
        {
            get { return mErrCode; }
            set
            {
                if (value != this.mErrCode)
                {
                    mErrCode = value;
                    NotifyPropertyChanged("ErrCode");
                }
            }
        }

        private int mStatusCode;
        [DataMember]
        public int StatusCode
        {
            get { return mStatusCode; }
            set
            {
                if (value != this.mStatusCode)
                {
                    mStatusCode = value;
                    NotifyPropertyChanged("StatusCode");
                }
            }
        }


        private bool mAxisBusy;
        [DataMember]
        public bool AxisBusy
        {
            get { return mAxisBusy; }
            set
            {
                if (value != this.mAxisBusy)
                {
                    mAxisBusy = value;
                    NotifyPropertyChanged("AxisBusy");
                }

            }
        }

        private bool mAxisEnabled;
        [DataMember]
        public bool AxisEnabled
        {
            get { return mAxisEnabled; }
            set
            {
                if (value != this.mAxisEnabled)
                {
                    mAxisEnabled = value;
                    NotifyPropertyChanged("AxisEnabled");
                }
            }
        }


        private bool _Inposition;
        [DataMember]
        public bool Inposition
        {
            get { return _Inposition; }
            set
            {
                if (value != this._Inposition)
                {
                    _Inposition = value;
                    NotifyPropertyChanged("Inposition");
                }

            }
        }

        private bool _HomeSensor;
        [DataMember]
        public bool HomeSensor
        {
            get { return _HomeSensor; }
            set
            {
                if (_HomeSensor == value) return;
                _HomeSensor = value;
                NotifyPropertyChanged("HomeSensor");
            }
        }

        //private int _HomeSensorIdx;

        //public int HomeSensorIdx
        //{
        //    get { return _HomeSensorIdx; }
        //    set
        //    {
        //        if (_HomeSensorIdx == value) return;
        //        _HomeSensorIdx = value;
        //    }
        //}
        //private int _POTSensorIdx;

        //public int POTSensorIdx
        //{
        //    get { return _POTSensorIdx; }
        //    set
        //    {
        //        if (_POTSensorIdx == value) return;
        //        _POTSensorIdx = value;
        //    }
        //}
        //private int _NOTSensorIdx;

        //public int NOTSensorIdx
        //{
        //    get { return _NOTSensorIdx; }
        //    set
        //    {
        //        if (_NOTSensorIdx == value) return;
        //        _NOTSensorIdx = value;
        //    }
        //}

        private bool _POTSensor;
        [DataMember]
        public bool POTSensor
        {
            get { return _POTSensor; }
            set
            {
                if (_POTSensor == value) return;
                _POTSensor = value;
                NotifyPropertyChanged("POTSensor");
            }
        }

        private bool _NOTSensor;
        [DataMember]
        public bool NOTSensor
        {
            get { return _NOTSensor; }
            set
            {
                if (_NOTSensor == value) return;
                _NOTSensor = value;
                NotifyPropertyChanged("NOTSensor");
            }
        }

        private double _CompValue;
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public double CompValue
        {
            get { return _CompValue; }
            set
            {
                if (_CompValue == value) return;
                _CompValue = value;
                NotifyPropertyChanged("CompValue");
            }
        }
        private double _AuxPosition;
        [XmlIgnore, JsonIgnore]
        [DataMember]
        public double AuxPosition
        {
            get { return _AuxPosition; }
            set
            {
                if (_AuxPosition == value) return;
                _AuxPosition = value;
                NotifyPropertyChanged("AuxPosition");
            }
        }
        #endregion
    }

}
