using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberErrorCode;

using LogModule;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public delegate void AxisStatusUpdatedDelegate(ProbeAxisObject axis);

    [Serializable]
    public class Axes : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ObservableCollection<AxisObject> _AxisObjects = new ObservableCollection<AxisObject>();

        public ObservableCollection<AxisObject> AxisObjects
        {
            get { return _AxisObjects; }
            set
            {
                _AxisObjects = value;
                NotifyPropertyChanged("AxisObjects");
            }
        }
        public Axes()
        {
            try
            {
                //AxisObject tmp = new AxisObject(0, 0);
                //tmp.Config.MoterConfig.MotorType = EnumMoterType.SERVO;
                //tmp.Config.MoterConfig.AmpEnable = false;
                //tmp.Config.MoterConfig.AmpDisableDelay.Value = 0;

                //tmp.Config.MoterConfig.AmpDisableAction = EnumAmpActionType.CMD_ACT;
                //tmp.Config.MoterConfig.FeedbackPhaseReverse[0] = false;
                //tmp.Config.MoterConfig.FeedbackType[0] = EnumFeedbackType.QUAD_AB;
                //tmp.Config.MoterConfig.EnableFeedbackFilter[0] = false;

                //tmp.Config.MoterConfig.FeedbackPhaseReverse[1] = false;
                //tmp.Config.MoterConfig.FeedbackType[1] = EnumFeedbackType.QUAD_AB;
                //tmp.Config.MoterConfig.EnableFeedbackFilter[1] = false;
                //tmp.Config.MoterConfig.AmpDisableDelay.Value = 0;




                //tmp.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                //tmp.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                //tmp.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;
                //tmp.Config.MoterConfig.EnableStepLoopBack = false;
                //// tmp.Param.DtoPRatio.Value = 279.62026666666666666666666666667;

                //tmp.Config.MoterConfig.AmpFaultTrigHigh = true;
                //tmp.Config.MoterConfig.AmpFaultAction = EnumEventActionType.ActionABORT;
                //tmp.Config.MoterConfig.AmpFaultDuration.Value = 0;
                //tmp.Config.MoterConfig.AmpWarningTrigHigh = true;
                //tmp.Config.MoterConfig.AmpWarningAction = EnumEventActionType.ActionNONE;
                //tmp.Config.MoterConfig.AmpWarningDuration.Value = 0;

                //tmp.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                //tmp.Config.MoterConfig.ErrorLimitAction = EnumEventActionType.ActionABORT;
                //tmp.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                //tmp.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                //tmp.Config.MoterConfig.TorqueLimitAction = EnumEventActionType.ActionNONE;
                //tmp.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                //tmp.Config.MoterConfig.HWNegLimitTrigHigh = false;
                //tmp.Config.MoterConfig.HWNegLimitAction = EnumEventActionType.ActionABORT;
                //tmp.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                //tmp.Config.MoterConfig.HWPosLimitTrigHigh = false;
                //tmp.Config.MoterConfig.HWPosLimitAction = EnumEventActionType.ActionABORT;
                //tmp.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                //tmp.Config.MoterConfig.SWNegLimitTrigger.Value = -1125899890065408;
                //tmp.Config.MoterConfig.SWNegLimitAction = EnumEventActionType.ActionE_STOP;
                //tmp.Config.MoterConfig.SWPosLimitTrigger.Value = 1125899906842624;
                //tmp.Config.MoterConfig.SWPosLimitAction = EnumEventActionType.ActionE_STOP;


                //tmp.Config.ControlType = ControlLoopTypeEnum.PID;

                //tmp.Config.PIDCoeff.GainProportional = 1.25;
                //tmp.Config.PIDCoeff.GainIntegral = 0.01;
                //tmp.Config.PIDCoeff.GainDerivative = 15;
                //tmp.Config.PIDCoeff.FeedForwardPosition = 0;
                //tmp.Config.PIDCoeff.FeedForwardVelocity = 0;
                //tmp.Config.PIDCoeff.FeedForwardAcceleration = 400;
                //tmp.Config.PIDCoeff.FeedForwardFriction = 0;
                //tmp.Config.PIDCoeff.DRate = 0;
                //tmp.Config.PIDCoeff.IntegrationMaxMoving = 1000;
                //tmp.Config.PIDCoeff.IntegrationMaxRest = 30000;
                //tmp.Config.PIDCoeff.OutputLimitHigh = 32767;
                //tmp.Config.PIDCoeff.OutputLimtLow = -32768;
                //tmp.Config.PIDCoeff.OutputVelocityLimitHigh = 32767;
                //tmp.Config.PIDCoeff.OutputVelocityLimitLow = -32768;
                //tmp.Config.PIDCoeff.OutputOffset = 0;

                //tmp.Config.Inposition.Value = 1118;
                //tmp.Config.SettlingTime.Value = 0;
                //tmp.Config.NearTargetDistance.Value = 5590;
                //tmp.Config.VelocityTolerance.Value = 559241;

                //tmp.Config.StopRate.Value = 0.5;
                //tmp.Config.EStopRate.Value = 0.025;


                //_AxisObjects.Add(tmp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public enum EnumAxisGroupType
    {
        SINGEAXIS,
        GROUPAXIS,
    }
    public enum EnumAxisConstants
    {
        Undefined = 0,
        X = Undefined + 1, //베이스 
        Y = 2,              //베이스
        Z = 3,              
        C = 4,              //wafer  chuck rot.
        A = 5,


        U1 = 6,
        U2 = 7,
        W = 8,
        V = 9,
        E = 10,
        CT = 11,
        CCM = 12,
        CCS = 13,
        CCG = 14,
        NC = 15,

        TZ1 = 16,
        TZ2 = 17,
        TZ3 = 18,
        R = 19,
        TT = 20,
        PZ = 21,
        TRI = 22, //3leg
        ROT = 23,
        SC = 24,

        //여기도 새로운거
        Z0 = 25, //wafer chuck base Z1
        Z1 = 26, //wafer chuck base Z2
        Z2 = 27, //wafer chuck base Z3

        // <-- 251017 sebas
        CX1 = 28,
        CY1 = 29,
        CZ1 = 30,
        CX2 = 31,
        CY2 = 32,
        CZ2 = 33,
        CX3 = 34,
        CY3 = 35,
        CZ3 = 36,
        CX4 = 37,
        CY4 = 38,
        CZ4 = 39,
        CZ5 = 40,
        NSZ1 = 41, //nano stage z
        FDZ1 = 42, //FD Z
        EJX1 = 43, //이젝션 X
        EJY1 = 44, //이젝션 Y
        FDT1 = 45, //FD회전
        EJPZ1 = 46, //이젝션 핀Z
        EJZ1 = 47, //이젝션 Z
        NZD1 = 48, //DD
        // -->

        MAX_STAGE_AXIS = Z2,
        
        LX = 100,
        FIRST_GPLOADER_AXIS = LX,
        LZM,
        LZS,
        LW,
        LB,
        LUD,
        LUU,
        LCC,
        FC1,
        FC2,
        FC3,
        FC4,
        LAST_GPLOADER_AXIS = FC4,
    }

    [Serializable()]
    public abstract class ProbeAxes : IParamNode
    {
        ObservableCollection<ProbeAxisObject> _ProbeAxisProviders = new ObservableCollection<ProbeAxisObject>();
        public ObservableCollection<ProbeAxisObject> ProbeAxisProviders
        {
            get { return _ProbeAxisProviders; }
            set { _ProbeAxisProviders = value; }
        }

        private List<HomingGroup> _HomingGroups = new List<HomingGroup>();

        public List<HomingGroup> HomingGroups
        {
            get { return _HomingGroups; }
            set { _HomingGroups = value; }
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


        public List<object> Nodes { get; set; }

        public ProbeAxes()
        {
            try
            {
                ProbeAxisProviders = new ObservableCollection<ProbeAxisObject>();
                #region Axis X,Y
                //for (int i = 0; i < 2; i++)
                //{
                //    ProbeAxisObject tmp = new ProbeAxisObject(0, i, (EnumAxisConstants)i);
                //    tmp.MotorVerticalHorizontalType = MotorVerticalHorizontalType.Horizontal;
                //    tmp.Param.IndexSearchingSpeed.Value = 20000;
                //    if (i == 0)
                //    {
                //        tmp.Param.HomeShift.Value = 4200;
                //        tmp.Param.HomeOffset.Value = -174450;
                //        tmp.HomingType = HomingMethodType.NHPI;
                //        tmp.Param.HomeInvert = false;
                //        tmp.Param.IndexInvert = true;
                //    }
                //    else if (i == 1)
                //    {
                //        tmp.Param.HomeShift.Value = 4200;
                //        tmp.Param.HomeOffset.Value = 587600;
                //        tmp.HomingType = HomingMethodType.PHNI;
                //        tmp.Param.HomeInvert = false;
                //        tmp.Param.IndexInvert = true;
                //    }
                //    tmp.Param.HommingSpeed.Value = 80000;
                //    tmp.Param.HommingAcceleration.Value = 800000;
                //    tmp.Param.HommingDecceleration.Value = 800000;
                //    tmp.Param.FinalVelociy.Value = 0;
                //    tmp.Param.Speed.Value = 1000000;
                //    tmp.Param.Acceleration.Value = 10000000;
                //    tmp.Param.Decceleration.Value = 10000000;
                //    tmp.Param.AccelerationJerk.Value = 0;
                //    tmp.Param.DeccelerationJerk.Value = 0;
                //    tmp.Param.SeqAcc.Value = 0;
                //    tmp.Param.SeqDcc.Value = 0;
                //    tmp.Param.DtoPRatio.Value = 279.62026666666666666666666666667;          //수정 요망


                //    if (i == 0)
                //    {
                //        tmp.Param.NegSWLimit.Value = -191000;
                //        tmp.Param.PosSWLimit.Value = 193000;
                //    }
                //    else if (i == 1)
                //    {
                //        tmp.Param.NegSWLimit.Value = -187000;
                //        tmp.Param.PosSWLimit.Value = 588000;
                //    }

                //    tmp.Param.POTIndex = 0;
                //    tmp.Param.NOTIndex = 0;
                //    tmp.Param.HOMEIndex = 1;

                //    tmp.Config.MoterConfig.MotorType = EnumMoterType.SERVO;
                //    tmp.Config.MoterConfig.AmpEnable = false;
                //    tmp.Config.MoterConfig.AmpDisableDelay.Value = 0;

                //    tmp.Config.InputHome = EnumDedicateInputs.DedicateInputHOME;
                //    tmp.Config.InputIndex = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                //    tmp.Config.InputMotor = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;


                //    tmp.Config.MoterConfig.AmpDisableAction = EnumAmpActionType.CMD_ACT;
                //    tmp.Config.MoterConfig.FeedbackPhaseReverse[0] = false;
                //    tmp.Config.MoterConfig.FeedbackType[0] = EnumFeedbackType.QUAD_AB;
                //    tmp.Config.MoterConfig.EnableFeedbackFilter[0] = false;

                //    tmp.Config.MoterConfig.FeedbackPhaseReverse[1] = false;
                //    tmp.Config.MoterConfig.FeedbackType[1] = EnumFeedbackType.QUAD_AB;
                //    tmp.Config.MoterConfig.EnableFeedbackFilter[1] = false;
                //    tmp.Config.MoterConfig.AmpDisableDelay.Value = 0;



                //    tmp.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                //    tmp.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                //    tmp.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;
                //    tmp.Config.MoterConfig.EnableStepLoopBack = false;

                //    tmp.Config.MoterConfig.AmpFaultTrigHigh = true;
                //    tmp.Config.MoterConfig.AmpFaultAction = EnumEventActionType.ActionABORT;
                //    tmp.Config.MoterConfig.AmpFaultDuration.Value = 0;
                //    tmp.Config.MoterConfig.AmpWarningTrigHigh = true;
                //    tmp.Config.MoterConfig.AmpWarningAction = EnumEventActionType.ActionNONE;
                //    tmp.Config.MoterConfig.AmpWarningDuration.Value = 0;

                //    tmp.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                //    tmp.Config.MoterConfig.ErrorLimitAction = EnumEventActionType.ActionABORT;
                //    tmp.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                //    tmp.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                //    tmp.Config.MoterConfig.TorqueLimitAction = EnumEventActionType.ActionNONE;
                //    tmp.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                //    tmp.Config.MoterConfig.HWNegLimitTrigHigh = false;
                //    tmp.Config.MoterConfig.HWNegLimitAction = EnumEventActionType.ActionABORT;
                //    tmp.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                //    tmp.Config.MoterConfig.HWPosLimitTrigHigh = false;
                //    tmp.Config.MoterConfig.HWPosLimitAction = EnumEventActionType.ActionABORT;
                //    tmp.Config.MoterConfig.HWPosLimitDuration.Value = 0;


                //    if (i == 0)
                //    {
                //        tmp.Config.MoterConfig.SWNegLimitTrigger.Value = -53407470;
                //        tmp.Config.MoterConfig.SWNegLimitAction = EnumEventActionType.ActionE_STOP;
                //        tmp.Config.MoterConfig.SWPosLimitTrigger.Value = 53966711;
                //        tmp.Config.MoterConfig.SWPosLimitAction = EnumEventActionType.ActionE_STOP;
                //    }
                //    else if (i == 1)
                //    {
                //        tmp.Config.MoterConfig.SWNegLimitTrigger.Value = -52173000;
                //        tmp.Config.MoterConfig.SWNegLimitAction = EnumEventActionType.ActionE_STOP;
                //        tmp.Config.MoterConfig.SWPosLimitTrigger.Value = 163215000;
                //        tmp.Config.MoterConfig.SWPosLimitAction = EnumEventActionType.ActionE_STOP;
                //    }

                //    tmp.Config.ControlType = ControlLoopTypeEnum.PID;

                //    tmp.Config.PIDCoeff.GainProportional = 1.25;
                //    tmp.Config.PIDCoeff.GainIntegral = 0.01;
                //    tmp.Config.PIDCoeff.GainDerivative = 15;
                //    tmp.Config.PIDCoeff.FeedForwardPosition = 0;
                //    tmp.Config.PIDCoeff.FeedForwardVelocity = 0;
                //    tmp.Config.PIDCoeff.FeedForwardAcceleration = 400;
                //    tmp.Config.PIDCoeff.FeedForwardFriction = 0;
                //    tmp.Config.PIDCoeff.DRate = 0;
                //    tmp.Config.PIDCoeff.IntegrationMaxMoving = 1000;
                //    tmp.Config.PIDCoeff.IntegrationMaxRest = 30000;
                //    tmp.Config.PIDCoeff.OutputLimitHigh = 32767;
                //    tmp.Config.PIDCoeff.OutputLimtLow = -32768;
                //    tmp.Config.PIDCoeff.OutputVelocityLimitHigh = 32767;
                //    tmp.Config.PIDCoeff.OutputVelocityLimitLow = -32768;
                //    tmp.Config.PIDCoeff.OutputOffset = 0;

                //    tmp.Config.Inposition.Value = 1118;
                //    tmp.Config.SettlingTime.Value = 0;
                //    tmp.Config.NearTargetDistance.Value = 5590;
                //    tmp.Config.VelocityTolerance.Value = 559241;

                //    tmp.Config.StopRate.Value = 0.1;
                //    tmp.Config.EStopRate.Value = 0.025;

                //    _ProbeAxisProviders.Add(tmp);
                //}
                #endregion

                #region Axis Z

                //ProbeAxisObject axisZ = new ProbeAxisObject(0, 2, (EnumAxisConstants)2);
                //axisZ.MotorVerticalHorizontalType = MotorVerticalHorizontalType.Vertical;
                //axisZ.Param.IndexSearchingSpeed.Value = 2000;
                //axisZ.Param.HomeOffset.Value = -31912;
                //axisZ.Param.HomeShift.Value = 0;


                //axisZ.HomingType = HomingMethodType.NHPI;
                //axisZ.Param.HomeInvert = false;
                //axisZ.Param.IndexInvert = false;


                //axisZ.Param.HommingSpeed.Value = 2000;
                //axisZ.Param.HommingAcceleration.Value = 100000;
                //axisZ.Param.HommingDecceleration.Value = 100000;
                //axisZ.Param.FinalVelociy.Value = 0;
                //axisZ.Param.Speed.Value = 8000;
                //axisZ.Param.Acceleration.Value = 1000000;
                //axisZ.Param.Decceleration.Value = 1000000;
                //axisZ.Param.AccelerationJerk.Value = 0;
                //axisZ.Param.DeccelerationJerk.Value = 0;
                //axisZ.Param.SeqAcc.Value = 0;
                //axisZ.Param.SeqDcc.Value = 0;
                //axisZ.Param.DtoPRatio.Value = 22.443835616438356164383561643836;
                //axisZ.Param.NegSWLimit.Value = -32500;
                //axisZ.Param.PosSWLimit.Value = 6000;
                //axisZ.Param.POTIndex = 0;
                //axisZ.Param.NOTIndex = 0;
                //axisZ.Param.HOMEIndex = 1;

                //axisZ.Config.MoterConfig.MotorType = EnumMoterType.STPPER;
                //axisZ.Config.MoterConfig.AmpEnable = false;
                //axisZ.Config.MoterConfig.AmpDisableDelay.Value = 0;

                //axisZ.Config.InputHome = EnumDedicateInputs.DedicateInputHOME;
                //axisZ.Config.InputIndex = EnumDedicateInputs.DedicateInputINDEX;
                //axisZ.Config.InputMotor = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                //axisZ.Config.MoterConfig.AmpDisableAction = EnumAmpActionType.NONE;
                //axisZ.Config.MoterConfig.FeedbackPhaseReverse[0] = true;
                //axisZ.Config.MoterConfig.FeedbackType[0] = EnumFeedbackType.QUAD_AB;
                //axisZ.Config.MoterConfig.EnableFeedbackFilter[0] = false;

                //axisZ.Config.MoterConfig.FeedbackPhaseReverse[1] = true;
                //axisZ.Config.MoterConfig.FeedbackType[1] = EnumFeedbackType.QUAD_AB;
                //axisZ.Config.MoterConfig.EnableFeedbackFilter[1] = false;
                //axisZ.Config.MoterConfig.AmpDisableDelay.Value = 0;

                //axisZ.Config.MoterConfig.PulseAType = EnumPulseType.MotorStepperPulseTypeQUADB;
                //axisZ.Config.MoterConfig.PulseBType = EnumPulseType.MotorStepperPulseTypeQUADA;
                //axisZ.Config.MoterConfig.PulseAInv = false;
                //axisZ.Config.MoterConfig.PulseBInv = false;


                //axisZ.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                //axisZ.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                //axisZ.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;
                //axisZ.Config.MoterConfig.EnableStepLoopBack = false;

                //axisZ.Config.MoterConfig.AmpFaultTrigHigh = true;
                //axisZ.Config.MoterConfig.AmpFaultAction = EnumEventActionType.ActionABORT;
                //axisZ.Config.MoterConfig.AmpFaultDuration.Value = 0;
                //axisZ.Config.MoterConfig.AmpWarningTrigHigh = true;
                //axisZ.Config.MoterConfig.AmpWarningAction = EnumEventActionType.ActionNONE;
                //axisZ.Config.MoterConfig.AmpWarningDuration.Value = 0;

                //axisZ.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                //axisZ.Config.MoterConfig.ErrorLimitAction = EnumEventActionType.ActionABORT;
                //axisZ.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                //axisZ.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                //axisZ.Config.MoterConfig.TorqueLimitAction = EnumEventActionType.ActionNONE;
                //axisZ.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                //axisZ.Config.MoterConfig.HWNegLimitTrigHigh = false;
                //axisZ.Config.MoterConfig.HWNegLimitAction = EnumEventActionType.ActionABORT;
                //axisZ.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                //axisZ.Config.MoterConfig.HWPosLimitTrigHigh = false;
                //axisZ.Config.MoterConfig.HWPosLimitAction = EnumEventActionType.ActionABORT;
                //axisZ.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                //axisZ.Config.MoterConfig.SWNegLimitTrigger.Value = -729424;
                //axisZ.Config.MoterConfig.SWNegLimitAction = EnumEventActionType.ActionE_STOP;
                //axisZ.Config.MoterConfig.SWPosLimitTrigger.Value = -134663;
                //axisZ.Config.MoterConfig.SWPosLimitAction = EnumEventActionType.ActionE_STOP;


                //axisZ.Config.ControlType = ControlLoopTypeEnum.PID;

                //axisZ.Config.PIDCoeff.GainProportional = 1;
                //axisZ.Config.PIDCoeff.GainIntegral = 0.012;
                //axisZ.Config.PIDCoeff.GainDerivative = 40;
                //axisZ.Config.PIDCoeff.FeedForwardPosition = 0;
                //axisZ.Config.PIDCoeff.FeedForwardVelocity = 0;
                //axisZ.Config.PIDCoeff.FeedForwardAcceleration = 1000;
                //axisZ.Config.PIDCoeff.FeedForwardFriction = 0;
                //axisZ.Config.PIDCoeff.DRate = 0;
                //axisZ.Config.PIDCoeff.IntegrationMaxMoving = 30000;
                //axisZ.Config.PIDCoeff.IntegrationMaxRest = 32767;
                //axisZ.Config.PIDCoeff.OutputLimitHigh = 32767;
                //axisZ.Config.PIDCoeff.OutputLimtLow = -32768;
                //axisZ.Config.PIDCoeff.OutputVelocityLimitHigh = 32767;
                //axisZ.Config.PIDCoeff.OutputVelocityLimitLow = -32768;
                //axisZ.Config.PIDCoeff.OutputOffset = 0;

                //axisZ.Config.Inposition.Value = 1118;
                //axisZ.Config.SettlingTime.Value = 0;
                //axisZ.Config.NearTargetDistance.Value = 5590;
                //axisZ.Config.VelocityTolerance.Value = 559241;

                //axisZ.Config.StopRate.Value = 0.05;
                //axisZ.Config.EStopRate.Value = 0.025;

                //_ProbeAxisProviders.Add(axisZ);

                #endregion

                #region AxisC
                //ProbeAxisObject axisC = new ProbeAxisObject(0, 3, (EnumAxisConstants)3);
                //axisC.MotorVerticalHorizontalType = MotorVerticalHorizontalType.Horizontal;
                //axisC.Param.IndexSearchingSpeed.Value = 2000;
                //axisC.Param.HomeOffset.Value = 15000;
                //axisC.Param.HomeShift.Value = 0;


                //axisC.HomingType = HomingMethodType.NH;
                //axisC.Param.HomeInvert = false;
                //axisC.Param.IndexInvert = true;


                //axisC.Param.HommingSpeed.Value = 2000;
                //axisC.Param.HommingAcceleration.Value = 100000;
                //axisC.Param.HommingDecceleration.Value = 100000;
                //axisC.Param.FinalVelociy.Value = 0;
                //axisC.Param.Speed.Value = 8000;
                //axisC.Param.Acceleration.Value = 1000000;
                //axisC.Param.Decceleration.Value = 1000000;
                //axisC.Param.AccelerationJerk.Value = 0;
                //axisC.Param.DeccelerationJerk.Value = 0;
                //axisC.Param.SeqAcc.Value = 0;
                //axisC.Param.SeqDcc.Value = 0;
                //axisC.Param.DtoPRatio.Value = 2.2756;
                //axisC.Param.NegSWLimit.Value = -51000;
                //axisC.Param.PosSWLimit.Value = 51000;
                //axisC.Param.POTIndex = 0;
                //axisC.Param.NOTIndex = 0;
                //axisC.Param.HOMEIndex = 1;

                //axisC.Config.MoterConfig.MotorType = EnumMoterType.SERVO;
                //axisC.Config.MoterConfig.AmpEnable = false;
                //axisC.Config.MoterConfig.AmpDisableDelay.Value = 0;

                //axisC.Config.InputHome = EnumDedicateInputs.DedicateInputHOME;
                //axisC.Config.InputIndex = EnumDedicateInputs.DedicateInputINDEX;
                //axisC.Config.InputMotor = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                //axisC.Config.MoterConfig.AmpDisableAction = EnumAmpActionType.CMD_ACT;
                //axisC.Config.MoterConfig.FeedbackPhaseReverse[0] = false;
                //axisC.Config.MoterConfig.FeedbackType[0] = EnumFeedbackType.QUAD_AB;
                //axisC.Config.MoterConfig.EnableFeedbackFilter[0] = false;

                //axisC.Config.MoterConfig.FeedbackPhaseReverse[1] = false;
                //axisC.Config.MoterConfig.FeedbackType[1] = EnumFeedbackType.QUAD_AB;
                //axisC.Config.MoterConfig.EnableFeedbackFilter[1] = false;
                //axisC.Config.MoterConfig.AmpDisableDelay.Value = 0;

                //axisC.Config.MoterConfig.PulseAType = EnumPulseType.MotorStepperPulseTypeSTEP;
                //axisC.Config.MoterConfig.PulseBType = EnumPulseType.MotorStepperPulseTypeDIR;
                //axisC.Config.MoterConfig.PulseAInv = false;
                //axisC.Config.MoterConfig.PulseBInv = false;


                //axisC.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                //axisC.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                //axisC.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;
                //axisC.Config.MoterConfig.EnableStepLoopBack = false;

                //axisC.Config.MoterConfig.AmpFaultTrigHigh = false;
                //axisC.Config.MoterConfig.AmpFaultAction = EnumEventActionType.ActionABORT;
                //axisC.Config.MoterConfig.AmpFaultDuration.Value = 0;
                //axisC.Config.MoterConfig.AmpWarningTrigHigh = true;
                //axisC.Config.MoterConfig.AmpWarningAction = EnumEventActionType.ActionNONE;
                //axisC.Config.MoterConfig.AmpWarningDuration.Value = 0;

                //axisC.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                //axisC.Config.MoterConfig.ErrorLimitAction = EnumEventActionType.ActionABORT;
                //axisC.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                //axisC.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                //axisC.Config.MoterConfig.TorqueLimitAction = EnumEventActionType.ActionNONE;
                //axisC.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                //axisC.Config.MoterConfig.HWNegLimitTrigHigh = false;
                //axisC.Config.MoterConfig.HWNegLimitAction = EnumEventActionType.ActionABORT;
                //axisC.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                //axisC.Config.MoterConfig.HWPosLimitTrigHigh = false;
                //axisC.Config.MoterConfig.HWPosLimitAction = EnumEventActionType.ActionABORT;
                //axisC.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                //axisC.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                //axisC.Config.MoterConfig.SWNegLimitAction = EnumEventActionType.ActionE_STOP;
                //axisC.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                //axisC.Config.MoterConfig.SWPosLimitAction = EnumEventActionType.ActionE_STOP;

                //axisC.Config.ControlType = ControlLoopTypeEnum.PID;

                //axisC.Config.PIDCoeff.GainProportional = 90;
                //axisC.Config.PIDCoeff.GainIntegral = 0;
                //axisC.Config.PIDCoeff.GainDerivative = -900;
                //axisC.Config.PIDCoeff.FeedForwardPosition = 0;
                //axisC.Config.PIDCoeff.FeedForwardVelocity = 0;
                //axisC.Config.PIDCoeff.FeedForwardAcceleration = 0;
                //axisC.Config.PIDCoeff.FeedForwardFriction = 0;
                //axisC.Config.PIDCoeff.DRate = 0;
                //axisC.Config.PIDCoeff.IntegrationMaxMoving = 0;
                //axisC.Config.PIDCoeff.IntegrationMaxRest = 0;
                //axisC.Config.PIDCoeff.OutputLimitHigh = 15000;
                //axisC.Config.PIDCoeff.OutputLimtLow = -15000;
                //axisC.Config.PIDCoeff.OutputVelocityLimitHigh = 32767;
                //axisC.Config.PIDCoeff.OutputVelocityLimitLow = -32768;
                //axisC.Config.PIDCoeff.OutputOffset = 0;

                //axisC.Config.Inposition.Value = 200;
                //axisC.Config.SettlingTime.Value = 0;
                //axisC.Config.NearTargetDistance.Value = 500;
                //axisC.Config.VelocityTolerance.Value = 40000;

                //axisC.Config.StopRate.Value = 0.05;
                //axisC.Config.EStopRate.Value = 0.025;

                //_ProbeAxisProviders.Add(axisC);

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable()]
    public class LoaderAxes : ProbeAxes, ISystemParameterizable, IParamNode
    {

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);


                    retval = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public string FilePath { get; } = "";
        public string FileName { get; } = "LoaderAxesObjectParam.xml";

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //  LoaderAxisEmulDefualt();
                //LoaderAxisDefualt();
                //LoaderAxisOPUSV();

                // LoaderAxisBSCI1();   // sebas 임시 주석
                LoaderAxisBSCI1_Bonder();   // 251017 sebas 

                HomingGroups = new List<HomingGroup>()
                {
                    new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.U1 }),
                    new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.U2 }),
                    new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.A, EnumAxisConstants.W }),
                };
                
                // LoaderAxisGP();  // sebas 임시 주석

            RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            //LoaderAxisDefualt();
            //LoaderAxis_OPUSV_Machine3();
            //LoaderAxisBSCI1();
            LoaderAxisGP();

                //HomingGroups = new List<HomingGroup>()
                //        {
                //            new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.U1 }),
                //            new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.U2 }),
                //            new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.A, EnumAxisConstants.W }),
                //        };

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

        }
        public EventCodeEnum LoaderAxisDefualt()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region Axis U1

                ProbeAxisObject axisu1 = new ProbeAxisObject(1, 0, EnumAxisConstants.U1);
                axisu1.Param.IndexSearchingSpeed.Value = 10000;
                axisu1.Param.HomeOffset.Value = -227700;
                axisu1.Param.HomeShift.Value = 0;
                axisu1.AxisType.Value = EnumAxisConstants.U1;
                axisu1.Label.Value = "U1";

                axisu1.HomingType.Value = HomingMethodType.NH;
                axisu1.Param.HomeInvert.Value = false;
                axisu1.Param.IndexInvert.Value = true;


                axisu1.Param.HommingSpeed.Value = 15000;
                axisu1.Param.HommingAcceleration.Value = 100000;
                axisu1.Param.HommingDecceleration.Value = 100000;
                axisu1.Param.FinalVelociy.Value = 0;
                axisu1.Param.Speed.Value = 900000;
                axisu1.Param.Acceleration.Value = 900000;
                axisu1.Param.Decceleration.Value = 900000;
                axisu1.Param.AccelerationJerk.Value = 66;
                axisu1.Param.DeccelerationJerk.Value = 66;
                axisu1.Param.SeqAcc.Value = 0;
                axisu1.Param.SeqDcc.Value = 0;
                axisu1.Param.DtoPRatio.Value = 0.111;
                axisu1.Param.NegSWLimit.Value = -229700;
                axisu1.Param.PosSWLimit.Value = 222000;
                axisu1.Param.POTIndex.Value = 0;
                axisu1.Param.NOTIndex.Value = 0;
                axisu1.Param.HOMEIndex.Value = 1;
                axisu1.Param.TimeOut.Value = 60000;
                axisu1.Param.HomeDistLimit.Value = 451700;
                axisu1.Param.IndexDistLimit.Value = 50000;

                axisu1.Config.StopRate.Value = 0.05;
                axisu1.Config.EStopRate.Value = 0.025;
                axisu1.Config.Inposition.Value = 350;
                axisu1.Config.NearTargetDistance.Value = 500;
                axisu1.Config.VelocityTolerance.Value = 10000;
                axisu1.Config.SettlingTime.Value = 0;

                axisu1.Config.MoterConfig.AmpEnable.Value = true;
                axisu1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisu1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisu1.Config.MoterConfig.PulseAInv.Value = true;
                axisu1.Config.MoterConfig.PulseBInv.Value = true;

                axisu1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisu1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisu1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu1.Config.PIDCoeff.GainProportional.Value = 0;
                axisu1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu1.Config.PIDCoeff.DRate.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu1.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu1);

                #endregion

                #region Axis U2

                ProbeAxisObject axisu2 = new ProbeAxisObject(1, 1, EnumAxisConstants.U2);
                axisu2.Param.IndexSearchingSpeed.Value = 10000;
                axisu2.Param.HomeOffset.Value = -227700;
                axisu2.Param.HomeShift.Value = 0;
                axisu2.AxisType.Value = EnumAxisConstants.U2;
                axisu2.Label.Value = "U2";

                axisu2.HomingType.Value = HomingMethodType.NH;
                axisu2.Param.HomeInvert.Value = false;
                axisu2.Param.IndexInvert.Value = true;


                axisu2.Param.HommingSpeed.Value = 15000;
                axisu2.Param.HommingAcceleration.Value = 100000;
                axisu2.Param.HommingDecceleration.Value = 100000;
                axisu2.Param.FinalVelociy.Value = 0;
                axisu2.Param.Speed.Value = 900000;
                axisu2.Param.Acceleration.Value = 900000;
                axisu2.Param.Decceleration.Value = 900000;
                axisu2.Param.AccelerationJerk.Value = 66;
                axisu2.Param.DeccelerationJerk.Value = 66;
                axisu2.Param.SeqAcc.Value = 0;
                axisu2.Param.SeqDcc.Value = 0;
                axisu2.Param.DtoPRatio.Value = 0.111;
                axisu2.Param.NegSWLimit.Value = -229700;
                axisu2.Param.PosSWLimit.Value = 222000;
                axisu2.Param.POTIndex.Value = 0;
                axisu2.Param.NOTIndex.Value = 0;
                axisu2.Param.HOMEIndex.Value = 1;
                axisu2.Param.TimeOut.Value = 60000;
                axisu2.Param.HomeDistLimit.Value = 451700;
                axisu2.Param.IndexDistLimit.Value = 50000;

                axisu2.Config.StopRate.Value = 0.05;
                axisu2.Config.EStopRate.Value = 0.025;
                axisu2.Config.Inposition.Value = 350;
                axisu2.Config.NearTargetDistance.Value = 500;
                axisu2.Config.VelocityTolerance.Value = 10000;
                axisu2.Config.SettlingTime.Value = 0;

                axisu2.Config.MoterConfig.AmpEnable.Value = true;
                axisu2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisu2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisu2.Config.MoterConfig.PulseAInv.Value = true;
                axisu2.Config.MoterConfig.PulseBInv.Value = true;

                axisu2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisu2.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisu2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisu2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu2.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisu2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu2.Config.PIDCoeff.GainProportional.Value = 0;
                axisu2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu2.Config.PIDCoeff.DRate.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu2.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu2);

                #endregion

                #region Axis V
                ProbeAxisObject axisv = new ProbeAxisObject(1, 3, EnumAxisConstants.V);
                axisv.Param.IndexSearchingSpeed.Value = 0;
                axisv.Param.HomeOffset.Value = 0;
                axisv.Param.HomeShift.Value = 0;
                axisv.AxisType.Value = EnumAxisConstants.V;
                axisv.Label.Value = "V";

                axisv.HomingType.Value = HomingMethodType.VH;
                axisv.Param.HomeInvert.Value = false;
                axisv.Param.IndexInvert.Value = false;


                axisv.Param.HommingSpeed.Value = 20000;
                axisv.Param.HommingAcceleration.Value = 100000;
                axisv.Param.HommingDecceleration.Value = 100000;
                axisv.Param.FinalVelociy.Value = 0;
                axisv.Param.Speed.Value = 80000;
                axisv.Param.Acceleration.Value = 200000;
                axisv.Param.Decceleration.Value = 200000;
                axisv.Param.AccelerationJerk.Value = 66;
                axisv.Param.DeccelerationJerk.Value = 66;
                axisv.Param.SeqAcc.Value = 0;
                axisv.Param.SeqDcc.Value = 0;
                axisv.Param.DtoPRatio.Value = 2.222222;
                axisv.Param.NegSWLimit.Value = -227000;
                axisv.Param.PosSWLimit.Value = 226000;
                axisv.Param.POTIndex.Value = 0;
                axisv.Param.NOTIndex.Value = 0;
                axisv.Param.HOMEIndex.Value = 1;
                axisv.Param.TimeOut.Value = 60000;
                axisv.Param.HomeDistLimit.Value = 0;
                axisv.Param.IndexDistLimit.Value = 0;

                axisv.Config.StopRate.Value = 0.05;
                axisv.Config.EStopRate.Value = 0.025;
                axisv.Config.Inposition.Value = 50;
                axisv.Config.NearTargetDistance.Value = 500;
                axisv.Config.VelocityTolerance.Value = 10000;
                axisv.Config.SettlingTime.Value = 0;

                axisv.Config.MoterConfig.AmpEnable.Value = true;
                axisv.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisv.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisv.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisv.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisv.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisv.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisv.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisv.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisv.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisv.Config.MoterConfig.PulseAInv.Value = true;
                axisv.Config.MoterConfig.PulseBInv.Value = true;

                axisv.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisv.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisv.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisv.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisv.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisv.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisv.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisv.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisv.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisv.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisv.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisv.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisv.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisv.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisv.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisv.Config.PIDCoeff.GainProportional.Value = 0;
                axisv.Config.PIDCoeff.GainIntegral.Value = 0;
                axisv.Config.PIDCoeff.GainDerivative.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisv.Config.PIDCoeff.DRate.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisv.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisv.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisv);

                #endregion

                #region Axis W

                ProbeAxisObject axisw = new ProbeAxisObject(1, 2, EnumAxisConstants.W);
                axisw.Param.IndexSearchingSpeed.Value = 500;
                axisw.Param.HomeOffset.Value = -121.31;
                axisw.Param.HomeShift.Value = 0;
                axisw.AxisType.Value = EnumAxisConstants.W;
                axisw.Label.Value = "W";

                axisw.HomingType.Value = HomingMethodType.PHNI;
                axisw.Param.HomeInvert.Value = false;
                axisw.Param.IndexInvert.Value = true;


                axisw.Param.HommingSpeed.Value = 1000;
                axisw.Param.HommingAcceleration.Value = 800;
                axisw.Param.HommingDecceleration.Value = 800;
                axisw.Param.FinalVelociy.Value = 0;
                axisw.Param.Speed.Value = 9000;
                axisw.Param.Acceleration.Value = 9000;
                axisw.Param.Decceleration.Value = 9000;
                axisw.Param.AccelerationJerk.Value = 66;
                axisw.Param.DeccelerationJerk.Value = 66;
                axisw.Param.SeqAcc.Value = 0;
                axisw.Param.SeqDcc.Value = 0;
                axisw.Param.DtoPRatio.Value = 1.666;
                axisw.Param.NegSWLimit.Value = -21000;
                axisw.Param.PosSWLimit.Value = 13000;
                axisw.Param.POTIndex.Value = 0;
                axisw.Param.NOTIndex.Value = 0;
                axisw.Param.HOMEIndex.Value = 1;
                axisw.Param.TimeOut.Value = 60000;
                axisw.Param.HomeDistLimit.Value = 34000;
                axisw.Param.IndexDistLimit.Value = 50000;

                axisw.Config.StopRate.Value = 0.05;
                axisw.Config.EStopRate.Value = 0.025;
                axisw.Config.Inposition.Value = 100;
                axisw.Config.NearTargetDistance.Value = 500;
                axisw.Config.VelocityTolerance.Value = 10000;
                axisw.Config.SettlingTime.Value = 0;

                axisw.Config.MoterConfig.AmpEnable.Value = true;
                axisw.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisw.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisw.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisw.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisw.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisw.Config.MoterConfig.EnableStepLoopBack.Value = true;
                axisw.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisw.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisw.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisw.Config.MoterConfig.PulseAInv.Value = true;
                axisw.Config.MoterConfig.PulseBInv.Value = true;

                axisw.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisw.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisw.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisw.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisw.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisw.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisw.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisw.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisw.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisw.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisw.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisw.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisw.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisw.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisw.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisw.Config.PIDCoeff.GainProportional.Value = 0;
                axisw.Config.PIDCoeff.GainIntegral.Value = 0;
                axisw.Config.PIDCoeff.GainDerivative.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisw.Config.PIDCoeff.DRate.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisw.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisw.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisw);


                #endregion

                #region Axis A

                ProbeAxisObject aixsa = new ProbeAxisObject(1, 4, EnumAxisConstants.A);
                aixsa.Param.IndexSearchingSpeed.Value = 1000;
                aixsa.Param.HomeOffset.Value = -100330;
                aixsa.Param.HomeShift.Value = 0;
                aixsa.AxisType.Value = EnumAxisConstants.A;
                aixsa.Label.Value = "A";

                aixsa.HomingType.Value = HomingMethodType.NHPI;
                aixsa.Param.HomeInvert.Value = false;
                aixsa.Param.IndexInvert.Value = true;


                aixsa.Param.HommingSpeed.Value = 30000;
                aixsa.Param.HommingAcceleration.Value = 15000;
                aixsa.Param.HommingDecceleration.Value = 15000;
                aixsa.Param.FinalVelociy.Value = 0;
                aixsa.Param.Speed.Value = 100000;
                aixsa.Param.Acceleration.Value = 100000;
                aixsa.Param.Decceleration.Value = 100000;
                aixsa.Param.AccelerationJerk.Value = 66;
                aixsa.Param.DeccelerationJerk.Value = 66;
                aixsa.Param.SeqAcc.Value = 0;
                aixsa.Param.SeqDcc.Value = 0;
                aixsa.Param.DtoPRatio.Value = 1.6384;
                aixsa.Param.NegSWLimit.Value = -105130;
                aixsa.Param.PosSWLimit.Value = 330000;
                aixsa.Param.POTIndex.Value = 0;
                aixsa.Param.NOTIndex.Value = 0;
                aixsa.Param.HOMEIndex.Value = 1;
                aixsa.Param.TimeOut.Value = 60000;
                aixsa.Param.HomeDistLimit.Value = 435130;
                aixsa.Param.IndexDistLimit.Value = 50000;

                aixsa.Config.StopRate.Value = 0.05;
                aixsa.Config.EStopRate.Value = 0.025;
                aixsa.Config.Inposition.Value = 50;
                aixsa.Config.NearTargetDistance.Value = 500;
                aixsa.Config.VelocityTolerance.Value = 10000;
                aixsa.Config.SettlingTime.Value = 0;

                aixsa.Config.MoterConfig.AmpEnable.Value = true;
                aixsa.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                aixsa.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                aixsa.Config.MoterConfig.AmpDisableDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                aixsa.Config.MoterConfig.EnableStepLoopBack.Value = false;
                aixsa.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                aixsa.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                aixsa.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                aixsa.Config.MoterConfig.PulseAInv.Value = true;
                aixsa.Config.MoterConfig.PulseBInv.Value = true;

                aixsa.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                aixsa.Config.MoterConfig.AmpFaultDuration.Value = 0;
                aixsa.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.AmpWarningDuration.Value = 0;
                aixsa.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                aixsa.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                aixsa.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                aixsa.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                aixsa.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                aixsa.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                aixsa.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                aixsa.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                aixsa.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                aixsa.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                aixsa.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                aixsa.Config.PIDCoeff.GainProportional.Value = 0;
                aixsa.Config.PIDCoeff.GainIntegral.Value = 0;
                aixsa.Config.PIDCoeff.GainDerivative.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                aixsa.Config.PIDCoeff.DRate.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                aixsa.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputOffset.Value = 16348;
                aixsa.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(aixsa);

                #endregion
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum LoaderAxisEmulDefualt()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region Axis U1

                ProbeAxisObject axisu1 = new ProbeAxisObject(1, 0, EnumAxisConstants.U1);
                axisu1.Param.IndexSearchingSpeed.Value = 10000;
                axisu1.Param.HomeOffset.Value = -227700;
                axisu1.Param.HomeShift.Value = 0;
                axisu1.AxisType.Value = EnumAxisConstants.U1;
                axisu1.Label.Value = "U1";

                axisu1.HomingType.Value = HomingMethodType.NH;
                axisu1.Param.HomeInvert.Value = false;
                axisu1.Param.IndexInvert.Value = true;


                axisu1.Param.HommingSpeed.Value = 15000;
                axisu1.Param.HommingAcceleration.Value = 100000;
                axisu1.Param.HommingDecceleration.Value = 100000;
                axisu1.Param.FinalVelociy.Value = 0;
                axisu1.Param.Speed.Value = 9000000;
                axisu1.Param.Acceleration.Value = 9000000;
                axisu1.Param.Decceleration.Value = 9000000;
                axisu1.Param.AccelerationJerk.Value = 66;
                axisu1.Param.DeccelerationJerk.Value = 66;
                axisu1.Param.SeqAcc.Value = 0;
                axisu1.Param.SeqDcc.Value = 0;
                axisu1.Param.DtoPRatio.Value = 0.111;
                axisu1.Param.NegSWLimit.Value = -229700;
                axisu1.Param.PosSWLimit.Value = 222000;
                axisu1.Param.POTIndex.Value = 0;
                axisu1.Param.NOTIndex.Value = 0;
                axisu1.Param.HOMEIndex.Value = 1;
                axisu1.Param.TimeOut.Value = 60000;
                axisu1.Param.HomeDistLimit.Value = 451700;
                axisu1.Param.IndexDistLimit.Value = 50000;

                axisu1.Config.StopRate.Value = 0.05;
                axisu1.Config.EStopRate.Value = 0.025;
                axisu1.Config.Inposition.Value = 350;
                axisu1.Config.NearTargetDistance.Value = 500;
                axisu1.Config.VelocityTolerance.Value = 10000;
                axisu1.Config.SettlingTime.Value = 0;

                axisu1.Config.MoterConfig.AmpEnable.Value = true;
                axisu1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisu1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisu1.Config.MoterConfig.PulseAInv.Value = true;
                axisu1.Config.MoterConfig.PulseBInv.Value = true;

                axisu1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisu1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisu1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu1.Config.PIDCoeff.GainProportional.Value = 0;
                axisu1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu1.Config.PIDCoeff.DRate.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu1.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu1);

                #endregion

                #region Axis U2

                ProbeAxisObject axisu2 = new ProbeAxisObject(1, 1, EnumAxisConstants.U2);
                axisu2.Param.IndexSearchingSpeed.Value = 10000;
                axisu2.Param.HomeOffset.Value = -227700;
                axisu2.Param.HomeShift.Value = 0;
                axisu2.AxisType.Value = EnumAxisConstants.U2;
                axisu2.Label.Value = "U2";

                axisu2.HomingType.Value = HomingMethodType.NH;
                axisu2.Param.HomeInvert.Value = false;
                axisu2.Param.IndexInvert.Value = true;


                axisu2.Param.HommingSpeed.Value = 15000;
                axisu2.Param.HommingAcceleration.Value = 100000;
                axisu2.Param.HommingDecceleration.Value = 100000;
                axisu2.Param.FinalVelociy.Value = 0;
                axisu2.Param.Speed.Value = 9000000;
                axisu2.Param.Acceleration.Value = 9000000;
                axisu2.Param.Decceleration.Value = 9000000;
                axisu2.Param.AccelerationJerk.Value = 66;
                axisu2.Param.DeccelerationJerk.Value = 66;
                axisu2.Param.SeqAcc.Value = 0;
                axisu2.Param.SeqDcc.Value = 0;
                axisu2.Param.DtoPRatio.Value = 0.111;
                axisu2.Param.NegSWLimit.Value = -229700;
                axisu2.Param.PosSWLimit.Value = 222000;
                axisu2.Param.POTIndex.Value = 0;
                axisu2.Param.NOTIndex.Value = 0;
                axisu2.Param.HOMEIndex.Value = 1;
                axisu2.Param.TimeOut.Value = 60000;
                axisu2.Param.HomeDistLimit.Value = 451700;
                axisu2.Param.IndexDistLimit.Value = 50000;

                axisu2.Config.StopRate.Value = 0.05;
                axisu2.Config.EStopRate.Value = 0.025;
                axisu2.Config.Inposition.Value = 350;
                axisu2.Config.NearTargetDistance.Value = 500;
                axisu2.Config.VelocityTolerance.Value = 10000;
                axisu2.Config.SettlingTime.Value = 0;

                axisu2.Config.MoterConfig.AmpEnable.Value = true;
                axisu2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisu2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisu2.Config.MoterConfig.PulseAInv.Value = true;
                axisu2.Config.MoterConfig.PulseBInv.Value = true;

                axisu2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisu2.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisu2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisu2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu2.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisu2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu2.Config.PIDCoeff.GainProportional.Value = 0;
                axisu2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu2.Config.PIDCoeff.DRate.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu2.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu2);

                #endregion

                #region Axis V
                ProbeAxisObject axisv = new ProbeAxisObject(1, 3, EnumAxisConstants.V);
                axisv.Param.IndexSearchingSpeed.Value = 0;
                axisv.Param.HomeOffset.Value = 0;
                axisv.Param.HomeShift.Value = 0;
                axisv.AxisType.Value = EnumAxisConstants.V;
                axisv.Label.Value = "V";

                axisv.HomingType.Value = HomingMethodType.VH;
                axisv.Param.HomeInvert.Value = false;
                axisv.Param.IndexInvert.Value = false;


                axisv.Param.HommingSpeed.Value = 20000;
                axisv.Param.HommingAcceleration.Value = 100000;
                axisv.Param.HommingDecceleration.Value = 100000;
                axisv.Param.FinalVelociy.Value = 0;
                axisv.Param.Speed.Value = 800000;
                axisv.Param.Acceleration.Value = 2000000;
                axisv.Param.Decceleration.Value = 2000000;
                axisv.Param.AccelerationJerk.Value = 66;
                axisv.Param.DeccelerationJerk.Value = 66;
                axisv.Param.SeqAcc.Value = 0;
                axisv.Param.SeqDcc.Value = 0;
                axisv.Param.DtoPRatio.Value = 2.222222;
                axisv.Param.NegSWLimit.Value = -227000;
                axisv.Param.PosSWLimit.Value = 226000;
                axisv.Param.POTIndex.Value = 0;
                axisv.Param.NOTIndex.Value = 0;
                axisv.Param.HOMEIndex.Value = 1;
                axisv.Param.TimeOut.Value = 60000;
                axisv.Param.HomeDistLimit.Value = 0;
                axisv.Param.IndexDistLimit.Value = 0;

                axisv.Config.StopRate.Value = 0.05;
                axisv.Config.EStopRate.Value = 0.025;
                axisv.Config.Inposition.Value = 50;
                axisv.Config.NearTargetDistance.Value = 500;
                axisv.Config.VelocityTolerance.Value = 10000;
                axisv.Config.SettlingTime.Value = 0;

                axisv.Config.MoterConfig.AmpEnable.Value = true;
                axisv.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisv.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisv.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisv.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisv.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisv.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisv.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisv.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisv.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisv.Config.MoterConfig.PulseAInv.Value = true;
                axisv.Config.MoterConfig.PulseBInv.Value = true;

                axisv.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisv.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisv.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisv.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisv.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisv.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisv.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisv.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisv.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisv.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisv.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisv.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisv.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisv.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisv.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisv.Config.PIDCoeff.GainProportional.Value = 0;
                axisv.Config.PIDCoeff.GainIntegral.Value = 0;
                axisv.Config.PIDCoeff.GainDerivative.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisv.Config.PIDCoeff.DRate.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisv.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisv.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisv);

                #endregion

                #region Axis W

                ProbeAxisObject axisw = new ProbeAxisObject(1, 2, EnumAxisConstants.W);
                axisw.Param.IndexSearchingSpeed.Value = 500;
                axisw.Param.HomeOffset.Value = -121.31;
                axisw.Param.HomeShift.Value = 0;
                axisw.AxisType.Value = EnumAxisConstants.W;
                axisw.Label.Value = "W";

                axisw.HomingType.Value = HomingMethodType.PHNI;
                axisw.Param.HomeInvert.Value = false;
                axisw.Param.IndexInvert.Value = true;


                axisw.Param.HommingSpeed.Value = 1000;
                axisw.Param.HommingAcceleration.Value = 800;
                axisw.Param.HommingDecceleration.Value = 800;
                axisw.Param.FinalVelociy.Value = 0;
                axisw.Param.Speed.Value = 90000;
                axisw.Param.Acceleration.Value = 900000;
                axisw.Param.Decceleration.Value = 900000;
                axisw.Param.AccelerationJerk.Value = 66;
                axisw.Param.DeccelerationJerk.Value = 66;
                axisw.Param.SeqAcc.Value = 0;
                axisw.Param.SeqDcc.Value = 0;
                axisw.Param.DtoPRatio.Value = 1.666;
                axisw.Param.NegSWLimit.Value = -21000;
                axisw.Param.PosSWLimit.Value = 13000;
                axisw.Param.POTIndex.Value = 0;
                axisw.Param.NOTIndex.Value = 0;
                axisw.Param.HOMEIndex.Value = 1;
                axisw.Param.TimeOut.Value = 60000;
                axisw.Param.HomeDistLimit.Value = 34000;
                axisw.Param.IndexDistLimit.Value = 50000;

                axisw.Config.StopRate.Value = 0.05;
                axisw.Config.EStopRate.Value = 0.025;
                axisw.Config.Inposition.Value = 100;
                axisw.Config.NearTargetDistance.Value = 500;
                axisw.Config.VelocityTolerance.Value = 10000;
                axisw.Config.SettlingTime.Value = 0;

                axisw.Config.MoterConfig.AmpEnable.Value = true;
                axisw.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisw.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisw.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisw.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisw.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisw.Config.MoterConfig.EnableStepLoopBack.Value = true;
                axisw.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisw.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisw.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisw.Config.MoterConfig.PulseAInv.Value = true;
                axisw.Config.MoterConfig.PulseBInv.Value = true;

                axisw.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisw.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisw.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisw.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisw.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisw.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisw.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisw.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisw.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisw.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisw.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisw.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisw.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisw.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisw.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisw.Config.PIDCoeff.GainProportional.Value = 0;
                axisw.Config.PIDCoeff.GainIntegral.Value = 0;
                axisw.Config.PIDCoeff.GainDerivative.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisw.Config.PIDCoeff.DRate.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisw.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisw.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisw);


                #endregion

                #region Axis A

                ProbeAxisObject aixsa = new ProbeAxisObject(1, 4, EnumAxisConstants.A);
                aixsa.Param.IndexSearchingSpeed.Value = 1000;
                aixsa.Param.HomeOffset.Value = -100330;
                aixsa.Param.HomeShift.Value = 0;
                aixsa.AxisType.Value = EnumAxisConstants.A;
                aixsa.Label.Value = "A";

                aixsa.HomingType.Value = HomingMethodType.NHPI;
                aixsa.Param.HomeInvert.Value = false;
                aixsa.Param.IndexInvert.Value = true;


                aixsa.Param.HommingSpeed.Value = 30000;
                aixsa.Param.HommingAcceleration.Value = 15000;
                aixsa.Param.HommingDecceleration.Value = 15000;
                aixsa.Param.FinalVelociy.Value = 0;
                aixsa.Param.Speed.Value = 1000000;
                aixsa.Param.Acceleration.Value = 10000000;
                aixsa.Param.Decceleration.Value = 10000000;
                aixsa.Param.AccelerationJerk.Value = 66;
                aixsa.Param.DeccelerationJerk.Value = 66;
                aixsa.Param.SeqAcc.Value = 0;
                aixsa.Param.SeqDcc.Value = 0;
                aixsa.Param.DtoPRatio.Value = 1.6384;
                aixsa.Param.NegSWLimit.Value = -105130;
                aixsa.Param.PosSWLimit.Value = 330000;
                aixsa.Param.POTIndex.Value = 0;
                aixsa.Param.NOTIndex.Value = 0;
                aixsa.Param.HOMEIndex.Value = 1;
                aixsa.Param.TimeOut.Value = 60000;
                aixsa.Param.HomeDistLimit.Value = 435130;
                aixsa.Param.IndexDistLimit.Value = 50000;

                aixsa.Config.StopRate.Value = 0.05;
                aixsa.Config.EStopRate.Value = 0.025;
                aixsa.Config.Inposition.Value = 50;
                aixsa.Config.NearTargetDistance.Value = 500;
                aixsa.Config.VelocityTolerance.Value = 10000;
                aixsa.Config.SettlingTime.Value = 0;

                aixsa.Config.MoterConfig.AmpEnable.Value = true;
                aixsa.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                aixsa.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                aixsa.Config.MoterConfig.AmpDisableDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                aixsa.Config.MoterConfig.EnableStepLoopBack.Value = false;
                aixsa.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                aixsa.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                aixsa.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                aixsa.Config.MoterConfig.PulseAInv.Value = true;
                aixsa.Config.MoterConfig.PulseBInv.Value = true;

                aixsa.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                aixsa.Config.MoterConfig.AmpFaultDuration.Value = 0;
                aixsa.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.AmpWarningDuration.Value = 0;
                aixsa.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                aixsa.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                aixsa.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                aixsa.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                aixsa.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                aixsa.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                aixsa.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                aixsa.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                aixsa.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                aixsa.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                aixsa.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                aixsa.Config.PIDCoeff.GainProportional.Value = 0;
                aixsa.Config.PIDCoeff.GainIntegral.Value = 0;
                aixsa.Config.PIDCoeff.GainDerivative.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                aixsa.Config.PIDCoeff.DRate.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                aixsa.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputOffset.Value = 16348;
                aixsa.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(aixsa);

                #endregion
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum LoaderAxisOPUSV()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region Axis U1

                ProbeAxisObject axisu1 = new ProbeAxisObject(0, 4, EnumAxisConstants.U1);
                axisu1.Param.IndexSearchingSpeed.Value = 10000;
                axisu1.Param.HomeOffset.Value = -98596;
                axisu1.Param.ClearedPosition.Value = -98596;
                axisu1.Param.HomeShift.Value = 0;
                axisu1.AxisType.Value = EnumAxisConstants.U1;
                axisu1.Label.Value = "U1";

                axisu1.HomingType.Value = HomingMethodType.NH;
                axisu1.Param.HomeInvert.Value = false;
                axisu1.Param.IndexInvert.Value = true;
                axisu1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu1.Param.FeedOverride.Value = 1;


                axisu1.Param.HommingSpeed.Value = 50000;
                axisu1.Param.HommingAcceleration.Value = 1500000;
                axisu1.Param.HommingDecceleration.Value = 1500000;
                axisu1.Param.FinalVelociy.Value = 0;
                axisu1.Param.Speed.Value = 540000;
                axisu1.Param.Acceleration.Value = 2880000;
                axisu1.Param.Decceleration.Value = 2880000;
                axisu1.Param.AccelerationJerk.Value = 288000000;
                axisu1.Param.DeccelerationJerk.Value = 288000000;
                axisu1.Param.SeqAcc.Value = 0;
                axisu1.Param.SeqDcc.Value = 0;
                axisu1.Param.DtoPRatio.Value = 0.113777778;
                axisu1.Param.NegSWLimit.Value = -99000;
                axisu1.Param.PosSWLimit.Value = 152028;
                axisu1.Param.POTIndex.Value = 0;
                axisu1.Param.NOTIndex.Value = 0;
                axisu1.Param.HOMEIndex.Value = 1;
                axisu1.Param.TimeOut.Value = 60000;
                axisu1.Param.HomeDistLimit.Value = 451700;
                axisu1.Param.IndexDistLimit.Value = 50000;

                axisu1.Config.StopRate.Value = 0.05;
                axisu1.Config.EStopRate.Value = 0.025;
                axisu1.Config.Inposition.Value = 350;
                axisu1.Config.NearTargetDistance.Value = 500;
                axisu1.Config.VelocityTolerance.Value = 10000;
                axisu1.Config.SettlingTime.Value = 0;

                axisu1.Config.MoterConfig.AmpEnable.Value = true;
                axisu1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisu1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisu1.Config.MoterConfig.PulseAInv.Value = true;
                axisu1.Config.MoterConfig.PulseBInv.Value = true;

                axisu1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisu1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisu1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu1.Config.PIDCoeff.GainProportional.Value = 0;
                axisu1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu1.Config.PIDCoeff.DRate.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu1.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu1);

                #endregion

                #region Axis U2

                ProbeAxisObject axisu2 = new ProbeAxisObject(0, 5, EnumAxisConstants.U2);
                axisu2.Param.IndexSearchingSpeed.Value = 10000;
                axisu2.Param.HomeOffset.Value = -98596;
                axisu2.Param.ClearedPosition.Value = -98596;
                axisu2.Param.HomeShift.Value = 0;
                axisu2.AxisType.Value = EnumAxisConstants.U2;
                axisu2.Label.Value = "U2";

                axisu2.HomingType.Value = HomingMethodType.NH;
                axisu2.Param.HomeInvert.Value = false;
                axisu2.Param.IndexInvert.Value = true;
                axisu2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu2.Param.FeedOverride.Value = 1;


                axisu2.Param.HommingSpeed.Value = 50000;
                axisu2.Param.HommingAcceleration.Value = 1500000;
                axisu2.Param.HommingDecceleration.Value = 1500000;
                axisu2.Param.FinalVelociy.Value = 0;
                axisu2.Param.Speed.Value = 540000;
                axisu2.Param.Acceleration.Value = 2880000;
                axisu2.Param.Decceleration.Value = 2880000;
                axisu2.Param.AccelerationJerk.Value = 288000000;
                axisu2.Param.DeccelerationJerk.Value = 288000000;
                axisu2.Param.SeqAcc.Value = 0;
                axisu2.Param.SeqDcc.Value = 0;
                axisu2.Param.DtoPRatio.Value = 0.113777778;
                axisu2.Param.NegSWLimit.Value = -99000;
                axisu2.Param.PosSWLimit.Value = 152028;
                axisu2.Param.POTIndex.Value = 0;
                axisu2.Param.NOTIndex.Value = 0;
                axisu2.Param.HOMEIndex.Value = 1;
                axisu2.Param.TimeOut.Value = 60000;
                axisu2.Param.HomeDistLimit.Value = 451700;
                axisu2.Param.IndexDistLimit.Value = 50000;

                axisu2.Config.StopRate.Value = 0.05;
                axisu2.Config.EStopRate.Value = 0.025;
                axisu2.Config.Inposition.Value = 350;
                axisu2.Config.NearTargetDistance.Value = 500;
                axisu2.Config.VelocityTolerance.Value = 10000;
                axisu2.Config.SettlingTime.Value = 0;

                axisu2.Config.MoterConfig.AmpEnable.Value = true;
                axisu2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisu2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisu2.Config.MoterConfig.PulseAInv.Value = true;
                axisu2.Config.MoterConfig.PulseBInv.Value = true;

                axisu2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisu2.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisu2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisu2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu2.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisu2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu2.Config.PIDCoeff.GainProportional.Value = 0;
                axisu2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu2.Config.PIDCoeff.DRate.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu2.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu2);

                #endregion

                #region Axis W

                ProbeAxisObject axisw = new ProbeAxisObject(0, 6, EnumAxisConstants.W);
                axisw.Param.IndexSearchingSpeed.Value = 500;
                axisw.Param.HomeOffset.Value = -125;
                axisw.Param.ClearedPosition.Value = 0;
                axisw.Param.HomeShift.Value = 0;
                axisw.AxisType.Value = EnumAxisConstants.W;
                axisw.Label.Value = "W";

                axisw.HomingType.Value = HomingMethodType.PH;
                axisw.Param.HomeInvert.Value = false;
                axisw.Param.IndexInvert.Value = true;
                axisw.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisw.Param.FeedOverride.Value = 1;


                axisw.Param.HommingSpeed.Value = 1500;
                axisw.Param.HommingAcceleration.Value = 15000;
                axisw.Param.HommingDecceleration.Value = 15000;
                axisw.Param.FinalVelociy.Value = 0;
                axisw.Param.Speed.Value = 15000;
                axisw.Param.Acceleration.Value = 50000;
                axisw.Param.Decceleration.Value = 50000;
                axisw.Param.AccelerationJerk.Value = 5000000;
                axisw.Param.DeccelerationJerk.Value = 5000000;
                axisw.Param.SeqAcc.Value = 0;
                axisw.Param.SeqDcc.Value = 0;
                //axisw.Param.DtoPRatio.Value = 1;
                axisw.Param.DtoPRatio.Value = 1.333333;
                axisw.Param.NegSWLimit.Value = -9300;
                axisw.Param.PosSWLimit.Value = 200;
                axisw.Param.POTIndex.Value = 0;
                axisw.Param.NOTIndex.Value = 0;
                axisw.Param.HOMEIndex.Value = 1;
                axisw.Param.TimeOut.Value = 60000;
                axisw.Param.HomeDistLimit.Value = 34000;
                axisw.Param.IndexDistLimit.Value = 50000;

                axisw.Config.StopRate.Value = 0.05;
                axisw.Config.EStopRate.Value = 0.025;
                axisw.Config.Inposition.Value = 100;
                axisw.Config.NearTargetDistance.Value = 500;
                axisw.Config.VelocityTolerance.Value = 10000;
                axisw.Config.SettlingTime.Value = 0;

                axisw.Config.MoterConfig.AmpEnable.Value = true;
                axisw.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisw.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisw.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisw.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisw.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisw.Config.MoterConfig.EnableStepLoopBack.Value = true;
                axisw.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisw.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisw.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisw.Config.MoterConfig.PulseAInv.Value = true;
                axisw.Config.MoterConfig.PulseBInv.Value = true;

                axisw.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisw.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisw.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisw.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisw.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisw.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisw.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisw.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisw.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisw.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisw.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisw.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisw.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisw.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisw.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisw.Config.PIDCoeff.GainProportional.Value = 0;
                axisw.Config.PIDCoeff.GainIntegral.Value = 0;
                axisw.Config.PIDCoeff.GainDerivative.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisw.Config.PIDCoeff.DRate.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisw.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisw.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisw);


                #endregion

                #region Axis A

                ProbeAxisObject aixsa = new ProbeAxisObject(0, 7, EnumAxisConstants.A);
                aixsa.Param.IndexSearchingSpeed.Value = 10000;
                aixsa.Param.HomeOffset.Value = -220301;
                aixsa.Param.ClearedPosition.Value = -220301;
                aixsa.Param.HomeShift.Value = 0;
                aixsa.AxisType.Value = EnumAxisConstants.A;
                aixsa.Label.Value = "A";

                aixsa.HomingType.Value = HomingMethodType.RLSEDGEINDEX;
                aixsa.Param.HomeInvert.Value = false;
                aixsa.Param.IndexInvert.Value = true;
                aixsa.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                aixsa.Param.FeedOverride.Value = 1;


                aixsa.Param.HommingSpeed.Value = 20000;
                aixsa.Param.HommingAcceleration.Value = 200000;
                aixsa.Param.HommingDecceleration.Value = 200000;
                aixsa.Param.FinalVelociy.Value = 0;
                aixsa.Param.Speed.Value = 250000;
                aixsa.Param.Acceleration.Value = 300000;
                aixsa.Param.Decceleration.Value = 300000;
                aixsa.Param.AccelerationJerk.Value = 30000000;
                aixsa.Param.DeccelerationJerk.Value = 30000000;
                aixsa.Param.SeqAcc.Value = 0;
                aixsa.Param.SeqDcc.Value = 0;
                aixsa.Param.DtoPRatio.Value = 209.7152;
                aixsa.Param.NegSWLimit.Value = -230000;
                aixsa.Param.PosSWLimit.Value = 190000;
                aixsa.Param.POTIndex.Value = 0;
                aixsa.Param.NOTIndex.Value = 0;
                aixsa.Param.HOMEIndex.Value = 1;
                aixsa.Param.TimeOut.Value = 60000;
                aixsa.Param.HomeDistLimit.Value = 435130;
                aixsa.Param.IndexDistLimit.Value = 50000;

                aixsa.Config.StopRate.Value = 0.05;
                aixsa.Config.EStopRate.Value = 0.025;
                aixsa.Config.Inposition.Value = 50;
                aixsa.Config.NearTargetDistance.Value = 500;
                aixsa.Config.VelocityTolerance.Value = 10000;
                aixsa.Config.SettlingTime.Value = 0;

                aixsa.Config.MoterConfig.AmpEnable.Value = true;
                aixsa.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                aixsa.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                aixsa.Config.MoterConfig.AmpDisableDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                aixsa.Config.MoterConfig.EnableStepLoopBack.Value = false;
                aixsa.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                aixsa.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                aixsa.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                aixsa.Config.MoterConfig.PulseAInv.Value = true;
                aixsa.Config.MoterConfig.PulseBInv.Value = true;

                aixsa.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                aixsa.Config.MoterConfig.AmpFaultDuration.Value = 0;
                aixsa.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.AmpWarningDuration.Value = 0;
                aixsa.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                aixsa.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                aixsa.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                aixsa.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                aixsa.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                aixsa.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                aixsa.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                aixsa.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                aixsa.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                aixsa.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                aixsa.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                aixsa.Config.PIDCoeff.GainProportional.Value = 0;
                aixsa.Config.PIDCoeff.GainIntegral.Value = 0;
                aixsa.Config.PIDCoeff.GainDerivative.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                aixsa.Config.PIDCoeff.DRate.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                aixsa.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputOffset.Value = 16348;
                aixsa.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(aixsa);

                #endregion

                #region Axis Scan

                ProbeAxisObject axisScan = new ProbeAxisObject(0, 8, EnumAxisConstants.SC);
                axisScan.Param.IndexSearchingSpeed.Value = 30000;
                axisScan.Param.HomeOffset.Value = 39560;
                axisScan.Param.ClearedPosition.Value = 0;
                axisScan.Param.HomeShift.Value = 0;
                axisScan.AxisType.Value = EnumAxisConstants.SC;
                axisScan.Label.Value = "SCAN";

                axisScan.HomingType.Value = HomingMethodType.NHPI;
                axisScan.Param.HomeInvert.Value = false;
                axisScan.Param.IndexInvert.Value = true;
                axisScan.Param.FeedOverride.Value = 1;


                axisScan.Param.HommingSpeed.Value = 50000;
                axisScan.Param.HommingAcceleration.Value = 500000;
                axisScan.Param.HommingDecceleration.Value = 500000;
                axisScan.Param.FinalVelociy.Value = 0;
                axisScan.Param.Speed.Value = 50000;
                axisScan.Param.Acceleration.Value = 500000;
                axisScan.Param.Decceleration.Value = 500000;
                axisScan.Param.AccelerationJerk.Value = 5000000;
                axisScan.Param.DeccelerationJerk.Value = 5000000;
                axisScan.Param.SeqAcc.Value = 0;
                axisScan.Param.SeqDcc.Value = 0;
                axisScan.Param.DtoPRatio.Value = 17.47626667;
                axisScan.Param.NegSWLimit.Value = -3000;
                axisScan.Param.PosSWLimit.Value = 184000;
                axisScan.Param.POTIndex.Value = 0;
                axisScan.Param.NOTIndex.Value = 0;
                axisScan.Param.HOMEIndex.Value = 1;
                axisScan.Param.TimeOut.Value = 60000;
                axisScan.Param.HomeDistLimit.Value = 435130;
                axisScan.Param.IndexDistLimit.Value = 50000;

                axisScan.Config.StopRate.Value = 0.05;
                axisScan.Config.EStopRate.Value = 0.025;
                axisScan.Config.Inposition.Value = 50;
                axisScan.Config.NearTargetDistance.Value = 500;
                axisScan.Config.VelocityTolerance.Value = 10000;
                axisScan.Config.SettlingTime.Value = 0;

                axisScan.Config.MoterConfig.AmpEnable.Value = true;
                axisScan.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisScan.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisScan.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisScan.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisScan.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisScan.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisScan.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisScan.Config.MoterConfig.PulseAInv.Value = true;
                axisScan.Config.MoterConfig.PulseBInv.Value = true;

                axisScan.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisScan.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisScan.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisScan.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisScan.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisScan.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisScan.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisScan.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisScan.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisScan.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisScan.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisScan.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisScan.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisScan.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisScan.Config.PIDCoeff.GainProportional.Value = 0;
                axisScan.Config.PIDCoeff.GainIntegral.Value = 0;
                axisScan.Config.PIDCoeff.GainDerivative.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisScan.Config.PIDCoeff.DRate.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisScan.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisScan.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisScan);

                #endregion

                #region Axis V
                ProbeAxisObject axisv = new ProbeAxisObject(0, 9, EnumAxisConstants.V);
                axisv.Param.IndexSearchingSpeed.Value = 0;
                axisv.Param.HomeOffset.Value = 0;
                axisv.Param.HomeShift.Value = 0;
                axisv.AxisType.Value = EnumAxisConstants.V;
                axisv.Label.Value = "V";

                axisv.HomingType.Value = HomingMethodType.VH;
                axisv.Param.HomeInvert.Value = false;
                axisv.Param.IndexInvert.Value = false;
                axisv.Param.FeedOverride.Value = 1;


                axisv.Param.HommingSpeed.Value = 20000;
                axisv.Param.HommingAcceleration.Value = 100000;
                axisv.Param.HommingDecceleration.Value = 100000;
                axisv.Param.FinalVelociy.Value = 0;
                axisv.Param.Speed.Value = 200000;
                axisv.Param.Acceleration.Value = 200000;
                axisv.Param.Decceleration.Value = 200000;
                axisv.Param.AccelerationJerk.Value = 2000000;
                axisv.Param.DeccelerationJerk.Value = 2000000;
                axisv.Param.SeqSpeed.Value = 60000;
                axisv.Param.SeqAcc.Value = 200000;
                axisv.Param.SeqDcc.Value = 200000;
                axisv.Param.DtoPRatio.Value = 1.42;
                axisv.Param.NegSWLimit.Value = double.MinValue;
                axisv.Param.PosSWLimit.Value = double.MaxValue;
                axisv.Param.POTIndex.Value = 0;
                axisv.Param.NOTIndex.Value = 0;
                axisv.Param.HOMEIndex.Value = 1;
                axisv.Param.TimeOut.Value = 60000;
                axisv.Param.HomeDistLimit.Value = 0;
                axisv.Param.IndexDistLimit.Value = 0;

                axisv.Config.StopRate.Value = 0.05;
                axisv.Config.EStopRate.Value = 0.025;
                axisv.Config.Inposition.Value = 50;
                axisv.Config.NearTargetDistance.Value = 500;
                axisv.Config.VelocityTolerance.Value = 10000;
                axisv.Config.SettlingTime.Value = 0;

                axisv.Config.MoterConfig.AmpEnable.Value = true;
                axisv.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisv.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisv.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisv.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisv.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisv.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisv.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisv.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisv.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisv.Config.MoterConfig.PulseAInv.Value = true;
                axisv.Config.MoterConfig.PulseBInv.Value = true;

                axisv.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisv.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisv.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisv.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisv.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisv.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisv.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisv.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisv.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisv.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisv.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisv.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisv.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisv.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisv.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisv.Config.PIDCoeff.GainProportional.Value = 0;
                axisv.Config.PIDCoeff.GainIntegral.Value = 0;
                axisv.Config.PIDCoeff.GainDerivative.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisv.Config.PIDCoeff.DRate.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisv.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisv.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisv);

                #endregion
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        public EventCodeEnum LoaderAxisBSCI1()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region Axis U1

                ProbeAxisObject axisu1 = new ProbeAxisObject(0, 0, EnumAxisConstants.U1);
                axisu1.Param.IndexSearchingSpeed.Value = 10000;
                axisu1.Param.HomeOffset.Value = -98596;
                axisu1.Param.ClearedPosition.Value = -98596;
                axisu1.Param.HomeShift.Value = 0;
                axisu1.AxisType.Value = EnumAxisConstants.U1;
                axisu1.Label.Value = "U1";

                axisu1.HomingType.Value = HomingMethodType.NH;
                axisu1.Param.HomeInvert.Value = false;
                axisu1.Param.IndexInvert.Value = true;
                axisu1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu1.Param.FeedOverride.Value = 1;


                axisu1.Param.HommingSpeed.Value = 50000;
                axisu1.Param.HommingAcceleration.Value = 1500000;
                axisu1.Param.HommingDecceleration.Value = 1500000;
                axisu1.Param.FinalVelociy.Value = 0;
                axisu1.Param.Speed.Value = 850000;
                axisu1.Param.Acceleration.Value = 4500000;
                axisu1.Param.Decceleration.Value = 4500000;
                axisu1.Param.AccelerationJerk.Value = 450000000;
                axisu1.Param.DeccelerationJerk.Value = 450000000;
                axisu1.Param.SeqAcc.Value = 0;
                axisu1.Param.SeqDcc.Value = 0;
                axisu1.Param.DtoPRatio.Value = 0.113777778;
                //axisu1.Param.DtoPRatio.Value = 111.1126;
                axisu1.Param.NegSWLimit.Value = -99000;
                axisu1.Param.PosSWLimit.Value = 152028;
                axisu1.Param.POTIndex.Value = 0;
                axisu1.Param.NOTIndex.Value = 0;
                axisu1.Param.HOMEIndex.Value = 1;
                axisu1.Param.TimeOut.Value = 60000;
                axisu1.Param.HomeDistLimit.Value = 451700;
                axisu1.Param.IndexDistLimit.Value = 50000;

                axisu1.Config.StopRate.Value = 0.05;
                axisu1.Config.EStopRate.Value = 0.025;
                axisu1.Config.Inposition.Value = 350;
                axisu1.Config.NearTargetDistance.Value = 500;
                axisu1.Config.VelocityTolerance.Value = 10000;
                axisu1.Config.SettlingTime.Value = 0;

                axisu1.Config.MoterConfig.AmpEnable.Value = true;
                axisu1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisu1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisu1.Config.MoterConfig.PulseAInv.Value = true;
                axisu1.Config.MoterConfig.PulseBInv.Value = true;

                axisu1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisu1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisu1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu1.Config.PIDCoeff.GainProportional.Value = 0;
                axisu1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu1.Config.PIDCoeff.DRate.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu1.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu1);

                #endregion

                #region Axis U2

                ProbeAxisObject axisu2 = new ProbeAxisObject(0, 1, EnumAxisConstants.U2);
                axisu2.Param.IndexSearchingSpeed.Value = 10000;
                axisu2.Param.HomeOffset.Value = -98596;
                axisu2.Param.ClearedPosition.Value = -98596;
                axisu2.Param.HomeShift.Value = 0;
                axisu2.AxisType.Value = EnumAxisConstants.U2;
                axisu2.Label.Value = "U2";

                axisu2.HomingType.Value = HomingMethodType.NH;
                axisu2.Param.HomeInvert.Value = false;
                axisu2.Param.IndexInvert.Value = true;
                axisu2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu2.Param.FeedOverride.Value = 1;


                axisu2.Param.HommingSpeed.Value = 50000;
                axisu2.Param.HommingAcceleration.Value = 1500000;
                axisu2.Param.HommingDecceleration.Value = 1500000;
                axisu2.Param.FinalVelociy.Value = 0;
                axisu2.Param.Speed.Value = 850000;
                axisu2.Param.Acceleration.Value = 4500000;
                axisu2.Param.Decceleration.Value = 4500000;
                axisu2.Param.AccelerationJerk.Value = 450000000;
                axisu2.Param.DeccelerationJerk.Value = 450000000;
                axisu2.Param.SeqAcc.Value = 0;
                axisu2.Param.SeqDcc.Value = 0;
                axisu2.Param.DtoPRatio.Value = 0.113777778;
                //axisu2.Param.DtoPRatio.Value = 111.1126;
                axisu2.Param.NegSWLimit.Value = -99000;
                axisu2.Param.PosSWLimit.Value = 152028;
                axisu2.Param.POTIndex.Value = 0;
                axisu2.Param.NOTIndex.Value = 0;
                axisu2.Param.HOMEIndex.Value = 1;
                axisu2.Param.TimeOut.Value = 60000;
                axisu2.Param.HomeDistLimit.Value = 451700;
                axisu2.Param.IndexDistLimit.Value = 50000;

                axisu2.Config.StopRate.Value = 0.05;
                axisu2.Config.EStopRate.Value = 0.025;
                axisu2.Config.Inposition.Value = 350;
                axisu2.Config.NearTargetDistance.Value = 500;
                axisu2.Config.VelocityTolerance.Value = 10000;
                axisu2.Config.SettlingTime.Value = 0;

                axisu2.Config.MoterConfig.AmpEnable.Value = true;
                axisu2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisu2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisu2.Config.MoterConfig.PulseAInv.Value = true;
                axisu2.Config.MoterConfig.PulseBInv.Value = true;

                axisu2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisu2.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisu2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisu2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu2.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisu2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu2.Config.PIDCoeff.GainProportional.Value = 0;
                axisu2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu2.Config.PIDCoeff.DRate.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu2.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu2);

                #endregion

                #region Axis W

                ProbeAxisObject axisw = new ProbeAxisObject(0, 2, EnumAxisConstants.W);
                axisw.Param.IndexSearchingSpeed.Value = 500;
                axisw.Param.HomeOffset.Value = -107;
                axisw.Param.ClearedPosition.Value = 0;
                axisw.Param.HomeShift.Value = 0;
                axisw.AxisType.Value = EnumAxisConstants.W;
                axisw.Label.Value = "W";

                axisw.HomingType.Value = HomingMethodType.PH;
                axisw.Param.HomeInvert.Value = false;
                axisw.Param.IndexInvert.Value = true;
                axisw.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisw.Param.FeedOverride.Value = 1;


                axisw.Param.HommingSpeed.Value = 300;
                axisw.Param.HommingAcceleration.Value = 3000;
                axisw.Param.HommingDecceleration.Value = 3000;
                axisw.Param.FinalVelociy.Value = 0;
                axisw.Param.Speed.Value = 15000;
                axisw.Param.Acceleration.Value = 50000;
                axisw.Param.Decceleration.Value = 50000;
                axisw.Param.AccelerationJerk.Value = 5000000;
                axisw.Param.DeccelerationJerk.Value = 5000000;
                axisw.Param.SeqAcc.Value = 0;
                axisw.Param.SeqDcc.Value = 0;
                axisw.Param.DtoPRatio.Value = 1.33333;
                axisw.Param.NegSWLimit.Value = -9200;
                axisw.Param.PosSWLimit.Value = 200;
                axisw.Param.POTIndex.Value = 0;
                axisw.Param.NOTIndex.Value = 0;
                axisw.Param.HOMEIndex.Value = 1;
                axisw.Param.TimeOut.Value = 60000;
                axisw.Param.HomeDistLimit.Value = 34000;
                axisw.Param.IndexDistLimit.Value = 50000;

                axisw.Config.StopRate.Value = 0.05;
                axisw.Config.EStopRate.Value = 0.025;
                axisw.Config.Inposition.Value = 100;
                axisw.Config.NearTargetDistance.Value = 500;
                axisw.Config.VelocityTolerance.Value = 10000;
                axisw.Config.SettlingTime.Value = 0;

                axisw.Config.MoterConfig.AmpEnable.Value = true;
                axisw.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisw.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisw.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisw.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisw.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisw.Config.MoterConfig.EnableStepLoopBack.Value = true;
                axisw.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisw.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisw.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisw.Config.MoterConfig.PulseAInv.Value = true;
                axisw.Config.MoterConfig.PulseBInv.Value = true;

                axisw.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisw.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisw.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisw.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisw.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisw.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisw.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisw.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisw.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisw.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisw.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisw.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisw.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisw.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisw.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisw.Config.PIDCoeff.GainProportional.Value = 0;
                axisw.Config.PIDCoeff.GainIntegral.Value = 0;
                axisw.Config.PIDCoeff.GainDerivative.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisw.Config.PIDCoeff.DRate.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisw.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisw.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisw);


                #endregion
                #region Axis V
                ProbeAxisObject axisv = new ProbeAxisObject(0, 3, EnumAxisConstants.V);
                axisv.Param.IndexSearchingSpeed.Value = 0;
                axisv.Param.HomeOffset.Value = 0;
                axisv.Param.HomeShift.Value = 0;
                axisv.AxisType.Value = EnumAxisConstants.V;
                axisv.Label.Value = "V";

                axisv.HomingType.Value = HomingMethodType.VH;
                axisv.Param.HomeInvert.Value = false;
                axisv.Param.IndexInvert.Value = false;
                axisv.Param.FeedOverride.Value = 1;


                axisv.Param.HommingSpeed.Value = 20000;
                axisv.Param.HommingAcceleration.Value = 100000;
                axisv.Param.HommingDecceleration.Value = 100000;
                axisv.Param.FinalVelociy.Value = 0;
                axisv.Param.Speed.Value = 200000;
                axisv.Param.Acceleration.Value = 200000;
                axisv.Param.Decceleration.Value = 200000;
                axisv.Param.AccelerationJerk.Value = 2000000;
                axisv.Param.DeccelerationJerk.Value = 2000000;
                axisv.Param.SeqSpeed.Value = 60000;
                axisv.Param.SeqAcc.Value = 200000;
                axisv.Param.SeqDcc.Value = 200000;
                axisv.Param.DtoPRatio.Value = 1.42;
                axisv.Param.NegSWLimit.Value = double.MinValue;
                axisv.Param.PosSWLimit.Value = double.MaxValue;
                axisv.Param.POTIndex.Value = 0;
                axisv.Param.NOTIndex.Value = 0;
                axisv.Param.HOMEIndex.Value = 1;
                axisv.Param.TimeOut.Value = 60000;
                axisv.Param.HomeDistLimit.Value = 0;
                axisv.Param.IndexDistLimit.Value = 0;

                axisv.Config.StopRate.Value = 0.05;
                axisv.Config.EStopRate.Value = 0.025;
                axisv.Config.Inposition.Value = 50;
                axisv.Config.NearTargetDistance.Value = 500;
                axisv.Config.VelocityTolerance.Value = 10000;
                axisv.Config.SettlingTime.Value = 0;

                axisv.Config.MoterConfig.AmpEnable.Value = true;
                axisv.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisv.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisv.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisv.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisv.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisv.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisv.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisv.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisv.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisv.Config.MoterConfig.PulseAInv.Value = true;
                axisv.Config.MoterConfig.PulseBInv.Value = true;

                axisv.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisv.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisv.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisv.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisv.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisv.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisv.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisv.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisv.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisv.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisv.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisv.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisv.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisv.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisv.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisv.Config.PIDCoeff.GainProportional.Value = 0;
                axisv.Config.PIDCoeff.GainIntegral.Value = 0;
                axisv.Config.PIDCoeff.GainDerivative.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisv.Config.PIDCoeff.DRate.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisv.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisv.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisv);

                #endregion
                #region Axis A

                ProbeAxisObject aixsa = new ProbeAxisObject(0, 4, EnumAxisConstants.A);
                aixsa.Param.IndexSearchingSpeed.Value = 10000;
                aixsa.Param.HomeOffset.Value = -216201;
                aixsa.Param.ClearedPosition.Value = -216201;
                aixsa.Param.HomeShift.Value = 0;
                aixsa.AxisType.Value = EnumAxisConstants.A;
                aixsa.Label.Value = "A";

                aixsa.HomingType.Value = HomingMethodType.RLSEDGEINDEX;
                aixsa.Param.HomeInvert.Value = false;
                aixsa.Param.IndexInvert.Value = true;
                aixsa.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                aixsa.Param.FeedOverride.Value = 1;


                aixsa.Param.HommingSpeed.Value = 20000;
                aixsa.Param.HommingAcceleration.Value = 250000;
                aixsa.Param.HommingDecceleration.Value = 250000;
                aixsa.Param.FinalVelociy.Value = 0;
                aixsa.Param.Speed.Value = 250000;
                aixsa.Param.Acceleration.Value = 300000;
                aixsa.Param.Decceleration.Value = 300000;
                aixsa.Param.AccelerationJerk.Value = 30000000;
                aixsa.Param.DeccelerationJerk.Value = 30000000;
                aixsa.Param.SeqAcc.Value = 0;
                aixsa.Param.SeqDcc.Value = 0;
                aixsa.Param.DtoPRatio.Value = 209.7152;
                aixsa.Param.NegSWLimit.Value = -230000;
                aixsa.Param.PosSWLimit.Value = 190000;
                aixsa.Param.POTIndex.Value = 0;
                aixsa.Param.NOTIndex.Value = 0;
                aixsa.Param.HOMEIndex.Value = 1;
                aixsa.Param.TimeOut.Value = 60000;
                aixsa.Param.HomeDistLimit.Value = 435130;
                aixsa.Param.IndexDistLimit.Value = 50000;

                aixsa.Config.StopRate.Value = 0.05;
                aixsa.Config.EStopRate.Value = 0.025;
                aixsa.Config.Inposition.Value = 50;
                aixsa.Config.NearTargetDistance.Value = 500;
                aixsa.Config.VelocityTolerance.Value = 10000;
                aixsa.Config.SettlingTime.Value = 0;

                aixsa.Config.MoterConfig.AmpEnable.Value = true;
                aixsa.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                aixsa.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                aixsa.Config.MoterConfig.AmpDisableDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                aixsa.Config.MoterConfig.EnableStepLoopBack.Value = false;
                aixsa.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                aixsa.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                aixsa.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                aixsa.Config.MoterConfig.PulseAInv.Value = true;
                aixsa.Config.MoterConfig.PulseBInv.Value = true;

                aixsa.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                aixsa.Config.MoterConfig.AmpFaultDuration.Value = 0;
                aixsa.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.AmpWarningDuration.Value = 0;
                aixsa.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                aixsa.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                aixsa.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                aixsa.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                aixsa.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                aixsa.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                aixsa.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                aixsa.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                aixsa.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                aixsa.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                aixsa.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                aixsa.Config.PIDCoeff.GainProportional.Value = 0;
                aixsa.Config.PIDCoeff.GainIntegral.Value = 0;
                aixsa.Config.PIDCoeff.GainDerivative.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                aixsa.Config.PIDCoeff.DRate.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                aixsa.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputOffset.Value = 16348;
                aixsa.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(aixsa);

                #endregion

                #region Axis Scan

                ProbeAxisObject axisScan = new ProbeAxisObject(0, 5, EnumAxisConstants.SC);
                axisScan.Param.IndexSearchingSpeed.Value = 30000;
                axisScan.Param.HomeOffset.Value = 38360;
                axisScan.Param.ClearedPosition.Value = 0;
                axisScan.Param.HomeShift.Value = 0;
                axisScan.AxisType.Value = EnumAxisConstants.SC;
                axisScan.Label.Value = "SCAN";

                axisScan.HomingType.Value = HomingMethodType.NHPI;
                axisScan.Param.HomeInvert.Value = false;
                axisScan.Param.IndexInvert.Value = true;
                axisScan.Param.FeedOverride.Value = 1;


                axisScan.Param.HommingSpeed.Value = 50000;
                axisScan.Param.HommingAcceleration.Value = 500000;
                axisScan.Param.HommingDecceleration.Value = 500000;
                axisScan.Param.FinalVelociy.Value = 0;
                axisScan.Param.Speed.Value = 90000;
                axisScan.Param.Acceleration.Value = 500000;
                axisScan.Param.Decceleration.Value = 500000;
                axisScan.Param.AccelerationJerk.Value = 5000000;
                axisScan.Param.DeccelerationJerk.Value = 5000000;
                axisScan.Param.SeqAcc.Value = 0;
                axisScan.Param.SeqDcc.Value = 0;
                axisScan.Param.DtoPRatio.Value = 17.47626667;
                axisScan.Param.NegSWLimit.Value = -3000;
                axisScan.Param.PosSWLimit.Value = 185000;
                axisScan.Param.POTIndex.Value = 0;
                axisScan.Param.NOTIndex.Value = 0;
                axisScan.Param.HOMEIndex.Value = 1;
                axisScan.Param.TimeOut.Value = 60000;
                axisScan.Param.HomeDistLimit.Value = 435130;
                axisScan.Param.IndexDistLimit.Value = 50000;

                axisScan.Config.StopRate.Value = 0.05;
                axisScan.Config.EStopRate.Value = 0.025;
                axisScan.Config.Inposition.Value = 50;
                axisScan.Config.NearTargetDistance.Value = 500;
                axisScan.Config.VelocityTolerance.Value = 10000;
                axisScan.Config.SettlingTime.Value = 0;

                axisScan.Config.MoterConfig.AmpEnable.Value = true;
                axisScan.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisScan.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisScan.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisScan.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisScan.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisScan.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisScan.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisScan.Config.MoterConfig.PulseAInv.Value = true;
                axisScan.Config.MoterConfig.PulseBInv.Value = true;

                axisScan.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisScan.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisScan.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisScan.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisScan.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisScan.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisScan.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisScan.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisScan.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisScan.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisScan.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisScan.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisScan.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisScan.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisScan.Config.PIDCoeff.GainProportional.Value = 0;
                axisScan.Config.PIDCoeff.GainIntegral.Value = 0;
                axisScan.Config.PIDCoeff.GainDerivative.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisScan.Config.PIDCoeff.DRate.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisScan.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisScan.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisScan);

                #endregion


                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        public EventCodeEnum LoaderAxisBSCI1_Bonder()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region Axis U1

                ProbeAxisObject axisu1 = new ProbeAxisObject(0, 36, EnumAxisConstants.U1);
                axisu1.Param.IndexSearchingSpeed.Value = 10000;
                axisu1.Param.HomeOffset.Value = -98596;
                axisu1.Param.ClearedPosition.Value = -98596;
                axisu1.Param.HomeShift.Value = 0;
                axisu1.AxisType.Value = EnumAxisConstants.U1;
                axisu1.Label.Value = "U1";

                axisu1.HomingType.Value = HomingMethodType.NH;
                axisu1.Param.HomeInvert.Value = false;
                axisu1.Param.IndexInvert.Value = true;
                axisu1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu1.Param.FeedOverride.Value = 1;


                axisu1.Param.HommingSpeed.Value = 50000;
                axisu1.Param.HommingAcceleration.Value = 1500000;
                axisu1.Param.HommingDecceleration.Value = 1500000;
                axisu1.Param.FinalVelociy.Value = 0;
                axisu1.Param.Speed.Value = 850000;
                axisu1.Param.Acceleration.Value = 4500000;
                axisu1.Param.Decceleration.Value = 4500000;
                axisu1.Param.AccelerationJerk.Value = 450000000;
                axisu1.Param.DeccelerationJerk.Value = 450000000;
                axisu1.Param.SeqAcc.Value = 0;
                axisu1.Param.SeqDcc.Value = 0;
                axisu1.Param.DtoPRatio.Value = 0.113777778;
                //axisu1.Param.DtoPRatio.Value = 111.1126;
                axisu1.Param.NegSWLimit.Value = -99000;
                axisu1.Param.PosSWLimit.Value = 152028;
                axisu1.Param.POTIndex.Value = 0;
                axisu1.Param.NOTIndex.Value = 0;
                axisu1.Param.HOMEIndex.Value = 1;
                axisu1.Param.TimeOut.Value = 60000;
                axisu1.Param.HomeDistLimit.Value = 451700;
                axisu1.Param.IndexDistLimit.Value = 50000;

                axisu1.Config.StopRate.Value = 0.05;
                axisu1.Config.EStopRate.Value = 0.025;
                axisu1.Config.Inposition.Value = 350;
                axisu1.Config.NearTargetDistance.Value = 500;
                axisu1.Config.VelocityTolerance.Value = 10000;
                axisu1.Config.SettlingTime.Value = 0;

                axisu1.Config.MoterConfig.AmpEnable.Value = true;
                axisu1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisu1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisu1.Config.MoterConfig.PulseAInv.Value = true;
                axisu1.Config.MoterConfig.PulseBInv.Value = true;

                axisu1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisu1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisu1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu1.Config.PIDCoeff.GainProportional.Value = 0;
                axisu1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu1.Config.PIDCoeff.DRate.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu1.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu1);

                #endregion

                #region Axis U2

                ProbeAxisObject axisu2 = new ProbeAxisObject(0, 37, EnumAxisConstants.U2);
                axisu2.Param.IndexSearchingSpeed.Value = 10000;
                axisu2.Param.HomeOffset.Value = -98596;
                axisu2.Param.ClearedPosition.Value = -98596;
                axisu2.Param.HomeShift.Value = 0;
                axisu2.AxisType.Value = EnumAxisConstants.U2;
                axisu2.Label.Value = "U2";

                axisu2.HomingType.Value = HomingMethodType.NH;
                axisu2.Param.HomeInvert.Value = false;
                axisu2.Param.IndexInvert.Value = true;
                axisu2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu2.Param.FeedOverride.Value = 1;


                axisu2.Param.HommingSpeed.Value = 50000;
                axisu2.Param.HommingAcceleration.Value = 1500000;
                axisu2.Param.HommingDecceleration.Value = 1500000;
                axisu2.Param.FinalVelociy.Value = 0;
                axisu2.Param.Speed.Value = 850000;
                axisu2.Param.Acceleration.Value = 4500000;
                axisu2.Param.Decceleration.Value = 4500000;
                axisu2.Param.AccelerationJerk.Value = 450000000;
                axisu2.Param.DeccelerationJerk.Value = 450000000;
                axisu2.Param.SeqAcc.Value = 0;
                axisu2.Param.SeqDcc.Value = 0;
                axisu2.Param.DtoPRatio.Value = 0.113777778;
                //axisu2.Param.DtoPRatio.Value = 111.1126;
                axisu2.Param.NegSWLimit.Value = -99000;
                axisu2.Param.PosSWLimit.Value = 152028;
                axisu2.Param.POTIndex.Value = 0;
                axisu2.Param.NOTIndex.Value = 0;
                axisu2.Param.HOMEIndex.Value = 1;
                axisu2.Param.TimeOut.Value = 60000;
                axisu2.Param.HomeDistLimit.Value = 451700;
                axisu2.Param.IndexDistLimit.Value = 50000;

                axisu2.Config.StopRate.Value = 0.05;
                axisu2.Config.EStopRate.Value = 0.025;
                axisu2.Config.Inposition.Value = 350;
                axisu2.Config.NearTargetDistance.Value = 500;
                axisu2.Config.VelocityTolerance.Value = 10000;
                axisu2.Config.SettlingTime.Value = 0;

                axisu2.Config.MoterConfig.AmpEnable.Value = true;
                axisu2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisu2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisu2.Config.MoterConfig.PulseAInv.Value = true;
                axisu2.Config.MoterConfig.PulseBInv.Value = true;

                axisu2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisu2.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisu2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisu2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu2.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisu2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu2.Config.PIDCoeff.GainProportional.Value = 0;
                axisu2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu2.Config.PIDCoeff.DRate.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu2.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu2);

                #endregion

                #region Axis W

                ProbeAxisObject axisw = new ProbeAxisObject(0, 38, EnumAxisConstants.W);
                axisw.Param.IndexSearchingSpeed.Value = 500;
                axisw.Param.HomeOffset.Value = -107;
                axisw.Param.ClearedPosition.Value = 0;
                axisw.Param.HomeShift.Value = 0;
                axisw.AxisType.Value = EnumAxisConstants.W;
                axisw.Label.Value = "W";

                axisw.HomingType.Value = HomingMethodType.PH;
                axisw.Param.HomeInvert.Value = false;
                axisw.Param.IndexInvert.Value = true;
                axisw.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisw.Param.FeedOverride.Value = 1;


                axisw.Param.HommingSpeed.Value = 300;
                axisw.Param.HommingAcceleration.Value = 3000;
                axisw.Param.HommingDecceleration.Value = 3000;
                axisw.Param.FinalVelociy.Value = 0;
                axisw.Param.Speed.Value = 15000;
                axisw.Param.Acceleration.Value = 50000;
                axisw.Param.Decceleration.Value = 50000;
                axisw.Param.AccelerationJerk.Value = 5000000;
                axisw.Param.DeccelerationJerk.Value = 5000000;
                axisw.Param.SeqAcc.Value = 0;
                axisw.Param.SeqDcc.Value = 0;
                axisw.Param.DtoPRatio.Value = 1.33333;
                axisw.Param.NegSWLimit.Value = -9200;
                axisw.Param.PosSWLimit.Value = 200;
                axisw.Param.POTIndex.Value = 0;
                axisw.Param.NOTIndex.Value = 0;
                axisw.Param.HOMEIndex.Value = 1;
                axisw.Param.TimeOut.Value = 60000;
                axisw.Param.HomeDistLimit.Value = 34000;
                axisw.Param.IndexDistLimit.Value = 50000;

                axisw.Config.StopRate.Value = 0.05;
                axisw.Config.EStopRate.Value = 0.025;
                axisw.Config.Inposition.Value = 100;
                axisw.Config.NearTargetDistance.Value = 500;
                axisw.Config.VelocityTolerance.Value = 10000;
                axisw.Config.SettlingTime.Value = 0;

                axisw.Config.MoterConfig.AmpEnable.Value = true;
                axisw.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisw.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisw.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisw.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisw.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisw.Config.MoterConfig.EnableStepLoopBack.Value = true;
                axisw.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisw.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisw.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisw.Config.MoterConfig.PulseAInv.Value = true;
                axisw.Config.MoterConfig.PulseBInv.Value = true;

                axisw.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisw.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisw.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisw.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisw.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisw.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisw.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisw.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisw.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisw.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisw.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisw.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisw.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisw.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisw.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisw.Config.PIDCoeff.GainProportional.Value = 0;
                axisw.Config.PIDCoeff.GainIntegral.Value = 0;
                axisw.Config.PIDCoeff.GainDerivative.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisw.Config.PIDCoeff.DRate.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisw.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisw.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisw);


                #endregion
                #region Axis V
                ProbeAxisObject axisv = new ProbeAxisObject(0, 39, EnumAxisConstants.V);
                axisv.Param.IndexSearchingSpeed.Value = 0;
                axisv.Param.HomeOffset.Value = 0;
                axisv.Param.HomeShift.Value = 0;
                axisv.AxisType.Value = EnumAxisConstants.V;
                axisv.Label.Value = "V";

                axisv.HomingType.Value = HomingMethodType.VH;
                axisv.Param.HomeInvert.Value = false;
                axisv.Param.IndexInvert.Value = false;
                axisv.Param.FeedOverride.Value = 1;


                axisv.Param.HommingSpeed.Value = 20000;
                axisv.Param.HommingAcceleration.Value = 100000;
                axisv.Param.HommingDecceleration.Value = 100000;
                axisv.Param.FinalVelociy.Value = 0;
                axisv.Param.Speed.Value = 200000;
                axisv.Param.Acceleration.Value = 200000;
                axisv.Param.Decceleration.Value = 200000;
                axisv.Param.AccelerationJerk.Value = 2000000;
                axisv.Param.DeccelerationJerk.Value = 2000000;
                axisv.Param.SeqSpeed.Value = 60000;
                axisv.Param.SeqAcc.Value = 200000;
                axisv.Param.SeqDcc.Value = 200000;
                axisv.Param.DtoPRatio.Value = 1.42;
                axisv.Param.NegSWLimit.Value = double.MinValue;
                axisv.Param.PosSWLimit.Value = double.MaxValue;
                axisv.Param.POTIndex.Value = 0;
                axisv.Param.NOTIndex.Value = 0;
                axisv.Param.HOMEIndex.Value = 1;
                axisv.Param.TimeOut.Value = 60000;
                axisv.Param.HomeDistLimit.Value = 0;
                axisv.Param.IndexDistLimit.Value = 0;

                axisv.Config.StopRate.Value = 0.05;
                axisv.Config.EStopRate.Value = 0.025;
                axisv.Config.Inposition.Value = 50;
                axisv.Config.NearTargetDistance.Value = 500;
                axisv.Config.VelocityTolerance.Value = 10000;
                axisv.Config.SettlingTime.Value = 0;

                axisv.Config.MoterConfig.AmpEnable.Value = true;
                axisv.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisv.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisv.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisv.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisv.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisv.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisv.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisv.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisv.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisv.Config.MoterConfig.PulseAInv.Value = true;
                axisv.Config.MoterConfig.PulseBInv.Value = true;

                axisv.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisv.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisv.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisv.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisv.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisv.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisv.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisv.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisv.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisv.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisv.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisv.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisv.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisv.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisv.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisv.Config.PIDCoeff.GainProportional.Value = 0;
                axisv.Config.PIDCoeff.GainIntegral.Value = 0;
                axisv.Config.PIDCoeff.GainDerivative.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisv.Config.PIDCoeff.DRate.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisv.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisv.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisv);

                #endregion
                #region Axis A

                ProbeAxisObject aixsa = new ProbeAxisObject(0, 40, EnumAxisConstants.A);
                aixsa.Param.IndexSearchingSpeed.Value = 10000;
                aixsa.Param.HomeOffset.Value = -216201;
                aixsa.Param.ClearedPosition.Value = -216201;
                aixsa.Param.HomeShift.Value = 0;
                aixsa.AxisType.Value = EnumAxisConstants.A;
                aixsa.Label.Value = "A";

                aixsa.HomingType.Value = HomingMethodType.RLSEDGEINDEX;
                aixsa.Param.HomeInvert.Value = false;
                aixsa.Param.IndexInvert.Value = true;
                aixsa.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                aixsa.Param.FeedOverride.Value = 1;


                aixsa.Param.HommingSpeed.Value = 20000;
                aixsa.Param.HommingAcceleration.Value = 250000;
                aixsa.Param.HommingDecceleration.Value = 250000;
                aixsa.Param.FinalVelociy.Value = 0;
                aixsa.Param.Speed.Value = 250000;
                aixsa.Param.Acceleration.Value = 300000;
                aixsa.Param.Decceleration.Value = 300000;
                aixsa.Param.AccelerationJerk.Value = 30000000;
                aixsa.Param.DeccelerationJerk.Value = 30000000;
                aixsa.Param.SeqAcc.Value = 0;
                aixsa.Param.SeqDcc.Value = 0;
                aixsa.Param.DtoPRatio.Value = 209.7152;
                aixsa.Param.NegSWLimit.Value = -230000;
                aixsa.Param.PosSWLimit.Value = 190000;
                aixsa.Param.POTIndex.Value = 0;
                aixsa.Param.NOTIndex.Value = 0;
                aixsa.Param.HOMEIndex.Value = 1;
                aixsa.Param.TimeOut.Value = 60000;
                aixsa.Param.HomeDistLimit.Value = 435130;
                aixsa.Param.IndexDistLimit.Value = 50000;

                aixsa.Config.StopRate.Value = 0.05;
                aixsa.Config.EStopRate.Value = 0.025;
                aixsa.Config.Inposition.Value = 50;
                aixsa.Config.NearTargetDistance.Value = 500;
                aixsa.Config.VelocityTolerance.Value = 10000;
                aixsa.Config.SettlingTime.Value = 0;

                aixsa.Config.MoterConfig.AmpEnable.Value = true;
                aixsa.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                aixsa.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                aixsa.Config.MoterConfig.AmpDisableDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                aixsa.Config.MoterConfig.EnableStepLoopBack.Value = false;
                aixsa.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                aixsa.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                aixsa.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                aixsa.Config.MoterConfig.PulseAInv.Value = true;
                aixsa.Config.MoterConfig.PulseBInv.Value = true;

                aixsa.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                aixsa.Config.MoterConfig.AmpFaultDuration.Value = 0;
                aixsa.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.AmpWarningDuration.Value = 0;
                aixsa.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                aixsa.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                aixsa.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                aixsa.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                aixsa.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                aixsa.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                aixsa.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                aixsa.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                aixsa.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                aixsa.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                aixsa.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                aixsa.Config.PIDCoeff.GainProportional.Value = 0;
                aixsa.Config.PIDCoeff.GainIntegral.Value = 0;
                aixsa.Config.PIDCoeff.GainDerivative.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                aixsa.Config.PIDCoeff.DRate.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                aixsa.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputOffset.Value = 16348;
                aixsa.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(aixsa);

                #endregion

                #region Axis Scan

                ProbeAxisObject axisScan = new ProbeAxisObject(0, 41, EnumAxisConstants.SC);
                axisScan.Param.IndexSearchingSpeed.Value = 30000;
                axisScan.Param.HomeOffset.Value = 38360;
                axisScan.Param.ClearedPosition.Value = 0;
                axisScan.Param.HomeShift.Value = 0;
                axisScan.AxisType.Value = EnumAxisConstants.SC;
                axisScan.Label.Value = "SCAN";

                axisScan.HomingType.Value = HomingMethodType.NHPI;
                axisScan.Param.HomeInvert.Value = false;
                axisScan.Param.IndexInvert.Value = true;
                axisScan.Param.FeedOverride.Value = 1;


                axisScan.Param.HommingSpeed.Value = 50000;
                axisScan.Param.HommingAcceleration.Value = 500000;
                axisScan.Param.HommingDecceleration.Value = 500000;
                axisScan.Param.FinalVelociy.Value = 0;
                axisScan.Param.Speed.Value = 90000;
                axisScan.Param.Acceleration.Value = 500000;
                axisScan.Param.Decceleration.Value = 500000;
                axisScan.Param.AccelerationJerk.Value = 5000000;
                axisScan.Param.DeccelerationJerk.Value = 5000000;
                axisScan.Param.SeqAcc.Value = 0;
                axisScan.Param.SeqDcc.Value = 0;
                axisScan.Param.DtoPRatio.Value = 17.47626667;
                axisScan.Param.NegSWLimit.Value = -3000;
                axisScan.Param.PosSWLimit.Value = 185000;
                axisScan.Param.POTIndex.Value = 0;
                axisScan.Param.NOTIndex.Value = 0;
                axisScan.Param.HOMEIndex.Value = 1;
                axisScan.Param.TimeOut.Value = 60000;
                axisScan.Param.HomeDistLimit.Value = 435130;
                axisScan.Param.IndexDistLimit.Value = 50000;

                axisScan.Config.StopRate.Value = 0.05;
                axisScan.Config.EStopRate.Value = 0.025;
                axisScan.Config.Inposition.Value = 50;
                axisScan.Config.NearTargetDistance.Value = 500;
                axisScan.Config.VelocityTolerance.Value = 10000;
                axisScan.Config.SettlingTime.Value = 0;

                axisScan.Config.MoterConfig.AmpEnable.Value = true;
                axisScan.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisScan.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisScan.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisScan.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisScan.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisScan.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisScan.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisScan.Config.MoterConfig.PulseAInv.Value = true;
                axisScan.Config.MoterConfig.PulseBInv.Value = true;

                axisScan.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisScan.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisScan.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisScan.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisScan.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisScan.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisScan.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisScan.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisScan.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisScan.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisScan.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisScan.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisScan.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisScan.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisScan.Config.PIDCoeff.GainProportional.Value = 0;
                axisScan.Config.PIDCoeff.GainIntegral.Value = 0;
                axisScan.Config.PIDCoeff.GainDerivative.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisScan.Config.PIDCoeff.DRate.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisScan.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisScan.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisScan);

                #endregion


                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum LoaderAxis_OPUSV_Machine3()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region Axis U1

                ProbeAxisObject axisu1 = new ProbeAxisObject(0, 4, EnumAxisConstants.U1);
                axisu1.Param.IndexSearchingSpeed.Value = 10000;
                axisu1.Param.HomeOffset.Value = -98596;
                axisu1.Param.ClearedPosition.Value = -98596;
                axisu1.Param.HomeShift.Value = 0;
                axisu1.AxisType.Value = EnumAxisConstants.U1;
                axisu1.Label.Value = "U1";

                axisu1.HomingType.Value = HomingMethodType.NH;
                axisu1.Param.HomeInvert.Value = false;
                axisu1.Param.IndexInvert.Value = true;
                axisu1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu1.Param.FeedOverride.Value = 1;


                axisu1.Param.HommingSpeed.Value = 50000;
                axisu1.Param.HommingAcceleration.Value = 1500000;
                axisu1.Param.HommingDecceleration.Value = 1500000;
                axisu1.Param.FinalVelociy.Value = 0;
                axisu1.Param.Speed.Value = 850000;
                axisu1.Param.Acceleration.Value = 4500000;
                axisu1.Param.Decceleration.Value = 4500000;
                axisu1.Param.AccelerationJerk.Value = 450000000;
                axisu1.Param.DeccelerationJerk.Value = 450000000;
                axisu1.Param.SeqAcc.Value = 0;
                axisu1.Param.SeqDcc.Value = 0;
                axisu1.Param.DtoPRatio.Value = 0.113777778;
                //axisu1.Param.DtoPRatio.Value = 111.1126;
                axisu1.Param.NegSWLimit.Value = -99000;
                axisu1.Param.PosSWLimit.Value = 152028;
                axisu1.Param.POTIndex.Value = 0;
                axisu1.Param.NOTIndex.Value = 0;
                axisu1.Param.HOMEIndex.Value = 1;
                axisu1.Param.TimeOut.Value = 60000;
                axisu1.Param.HomeDistLimit.Value = 451700;
                axisu1.Param.IndexDistLimit.Value = 50000;

                axisu1.Config.StopRate.Value = 0.05;
                axisu1.Config.EStopRate.Value = 0.025;
                axisu1.Config.Inposition.Value = 350;
                axisu1.Config.NearTargetDistance.Value = 500;
                axisu1.Config.VelocityTolerance.Value = 10000;
                axisu1.Config.SettlingTime.Value = 0;

                axisu1.Config.MoterConfig.AmpEnable.Value = true;
                axisu1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisu1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisu1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisu1.Config.MoterConfig.PulseAInv.Value = true;
                axisu1.Config.MoterConfig.PulseBInv.Value = true;

                axisu1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisu1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisu1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu1.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu1.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu1.Config.PIDCoeff.GainProportional.Value = 0;
                axisu1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu1.Config.PIDCoeff.DRate.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu1.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu1.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu1);

                #endregion

                #region Axis U2

                ProbeAxisObject axisu2 = new ProbeAxisObject(0, 5, EnumAxisConstants.U2);
                axisu2.Param.IndexSearchingSpeed.Value = 10000;
                axisu2.Param.HomeOffset.Value = -98596;
                axisu2.Param.ClearedPosition.Value = -98596;
                axisu2.Param.HomeShift.Value = 0;
                axisu2.AxisType.Value = EnumAxisConstants.U2;
                axisu2.Label.Value = "U2";

                axisu2.HomingType.Value = HomingMethodType.NH;
                axisu2.Param.HomeInvert.Value = false;
                axisu2.Param.IndexInvert.Value = true;
                axisu2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisu2.Param.FeedOverride.Value = 1;


                axisu2.Param.HommingSpeed.Value = 50000;
                axisu2.Param.HommingAcceleration.Value = 1500000;
                axisu2.Param.HommingDecceleration.Value = 1500000;
                axisu2.Param.FinalVelociy.Value = 0;
                axisu2.Param.Speed.Value = 850000;
                axisu2.Param.Acceleration.Value = 4500000;
                axisu2.Param.Decceleration.Value = 4500000;
                axisu2.Param.AccelerationJerk.Value = 450000000;
                axisu2.Param.DeccelerationJerk.Value = 450000000;
                axisu2.Param.SeqAcc.Value = 0;
                axisu2.Param.SeqDcc.Value = 0;
                axisu2.Param.DtoPRatio.Value = 0.113777778;
                //axisu2.Param.DtoPRatio.Value = 111.1126;
                axisu2.Param.NegSWLimit.Value = -99000;
                axisu2.Param.PosSWLimit.Value = 152028;
                axisu2.Param.POTIndex.Value = 0;
                axisu2.Param.NOTIndex.Value = 0;
                axisu2.Param.HOMEIndex.Value = 1;
                axisu2.Param.TimeOut.Value = 60000;
                axisu2.Param.HomeDistLimit.Value = 451700;
                axisu2.Param.IndexDistLimit.Value = 50000;

                axisu2.Config.StopRate.Value = 0.05;
                axisu2.Config.EStopRate.Value = 0.025;
                axisu2.Config.Inposition.Value = 350;
                axisu2.Config.NearTargetDistance.Value = 500;
                axisu2.Config.VelocityTolerance.Value = 10000;
                axisu2.Config.SettlingTime.Value = 0;

                axisu2.Config.MoterConfig.AmpEnable.Value = true;
                axisu2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisu2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisu2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisu2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisu2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisu2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisu2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisu2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisu2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisu2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisu2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisu2.Config.MoterConfig.PulseAInv.Value = true;
                axisu2.Config.MoterConfig.PulseBInv.Value = true;

                axisu2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisu2.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisu2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisu2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisu2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisu2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisu2.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisu2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisu2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisu2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisu2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisu2.Config.MoterConfig.SWNegLimitTrigger.Value = -2069369.3693693693693693693693694;
                axisu2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisu2.Config.MoterConfig.SWPosLimitTrigger.Value = 2000000;
                axisu2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisu2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisu2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisu2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisu2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisu2.Config.PIDCoeff.GainProportional.Value = 0;
                axisu2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisu2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisu2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisu2.Config.PIDCoeff.DRate.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisu2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisu2.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisu2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisu2.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisu2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisu2);

                #endregion

                #region Axis W

                ProbeAxisObject axisw = new ProbeAxisObject(0, 6, EnumAxisConstants.W);
                axisw.Param.IndexSearchingSpeed.Value = 500;
                axisw.Param.HomeOffset.Value = -107;
                axisw.Param.ClearedPosition.Value = 0;
                axisw.Param.HomeShift.Value = 0;
                axisw.AxisType.Value = EnumAxisConstants.W;
                axisw.Label.Value = "W";

                axisw.HomingType.Value = HomingMethodType.PH;
                axisw.Param.HomeInvert.Value = false;
                axisw.Param.IndexInvert.Value = true;
                axisw.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisw.Param.FeedOverride.Value = 1;


                axisw.Param.HommingSpeed.Value = 300;
                axisw.Param.HommingAcceleration.Value = 3000;
                axisw.Param.HommingDecceleration.Value = 3000;
                axisw.Param.FinalVelociy.Value = 0;
                axisw.Param.Speed.Value = 15000;
                axisw.Param.Acceleration.Value = 50000;
                axisw.Param.Decceleration.Value = 50000;
                axisw.Param.AccelerationJerk.Value = 5000000;
                axisw.Param.DeccelerationJerk.Value = 5000000;
                axisw.Param.SeqAcc.Value = 0;
                axisw.Param.SeqDcc.Value = 0;
                axisw.Param.DtoPRatio.Value = 1.33333;
                axisw.Param.NegSWLimit.Value = -9200;
                axisw.Param.PosSWLimit.Value = 200;
                axisw.Param.POTIndex.Value = 0;
                axisw.Param.NOTIndex.Value = 0;
                axisw.Param.HOMEIndex.Value = 1;
                axisw.Param.TimeOut.Value = 60000;
                axisw.Param.HomeDistLimit.Value = 34000;
                axisw.Param.IndexDistLimit.Value = 50000;

                axisw.Config.StopRate.Value = 0.05;
                axisw.Config.EStopRate.Value = 0.025;
                axisw.Config.Inposition.Value = 100;
                axisw.Config.NearTargetDistance.Value = 500;
                axisw.Config.VelocityTolerance.Value = 10000;
                axisw.Config.SettlingTime.Value = 0;

                axisw.Config.MoterConfig.AmpEnable.Value = true;
                axisw.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisw.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisw.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisw.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisw.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisw.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisw.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisw.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisw.Config.MoterConfig.EnableStepLoopBack.Value = true;
                axisw.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisw.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCCW;
                axisw.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCW;

                axisw.Config.MoterConfig.PulseAInv.Value = true;
                axisw.Config.MoterConfig.PulseBInv.Value = true;

                axisw.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisw.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisw.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisw.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisw.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisw.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisw.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisw.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisw.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisw.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisw.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisw.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisw.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisw.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisw.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisw.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisw.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisw.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisw.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisw.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisw.Config.PIDCoeff.GainProportional.Value = 0;
                axisw.Config.PIDCoeff.GainIntegral.Value = 0;
                axisw.Config.PIDCoeff.GainDerivative.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisw.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisw.Config.PIDCoeff.DRate.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisw.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisw.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisw.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisw.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisw.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisw);


                #endregion
                #region Axis V
                ProbeAxisObject axisv = new ProbeAxisObject(0, 7, EnumAxisConstants.V);
                axisv.Param.IndexSearchingSpeed.Value = 0;
                axisv.Param.HomeOffset.Value = 0;
                axisv.Param.HomeShift.Value = 0;
                axisv.AxisType.Value = EnumAxisConstants.V;
                axisv.Label.Value = "V";

                axisv.HomingType.Value = HomingMethodType.VH;
                axisv.Param.HomeInvert.Value = false;
                axisv.Param.IndexInvert.Value = false;
                axisv.Param.FeedOverride.Value = 1;


                axisv.Param.HommingSpeed.Value = 20000;
                axisv.Param.HommingAcceleration.Value = 100000;
                axisv.Param.HommingDecceleration.Value = 100000;
                axisv.Param.FinalVelociy.Value = 0;
                axisv.Param.Speed.Value = 200000;
                axisv.Param.Acceleration.Value = 200000;
                axisv.Param.Decceleration.Value = 200000;
                axisv.Param.AccelerationJerk.Value = 2000000;
                axisv.Param.DeccelerationJerk.Value = 2000000;
                axisv.Param.SeqSpeed.Value = 60000;
                axisv.Param.SeqAcc.Value = 200000;
                axisv.Param.SeqDcc.Value = 200000;
                axisv.Param.DtoPRatio.Value = 1.42;
                axisv.Param.NegSWLimit.Value = double.MinValue;
                axisv.Param.PosSWLimit.Value = double.MaxValue;
                axisv.Param.POTIndex.Value = 0;
                axisv.Param.NOTIndex.Value = 0;
                axisv.Param.HOMEIndex.Value = 1;
                axisv.Param.TimeOut.Value = 60000;
                axisv.Param.HomeDistLimit.Value = 0;
                axisv.Param.IndexDistLimit.Value = 0;

                axisv.Config.StopRate.Value = 0.05;
                axisv.Config.EStopRate.Value = 0.025;
                axisv.Config.Inposition.Value = 50;
                axisv.Config.NearTargetDistance.Value = 500;
                axisv.Config.VelocityTolerance.Value = 10000;
                axisv.Config.SettlingTime.Value = 0;

                axisv.Config.MoterConfig.AmpEnable.Value = true;
                axisv.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisv.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisv.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisv.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisv.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisv.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisv.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisv.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisv.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisv.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisv.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeCW;
                axisv.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeCCW;

                axisv.Config.MoterConfig.PulseAInv.Value = true;
                axisv.Config.MoterConfig.PulseBInv.Value = true;

                axisv.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisv.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisv.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisv.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisv.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisv.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisv.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisv.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisv.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisv.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisv.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisv.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisv.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisv.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisv.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisv.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisv.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisv.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisv.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisv.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisv.Config.PIDCoeff.GainProportional.Value = 0;
                axisv.Config.PIDCoeff.GainIntegral.Value = 0;
                axisv.Config.PIDCoeff.GainDerivative.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisv.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisv.Config.PIDCoeff.DRate.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisv.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisv.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisv.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisv.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisv.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisv);

                #endregion
                #region Axis A

                ProbeAxisObject aixsa = new ProbeAxisObject(0, 8, EnumAxisConstants.A);
                aixsa.Param.IndexSearchingSpeed.Value = 10000;
                aixsa.Param.HomeOffset.Value = -216201;
                aixsa.Param.ClearedPosition.Value = -216201;
                aixsa.Param.HomeShift.Value = 0;
                aixsa.AxisType.Value = EnumAxisConstants.A;
                aixsa.Label.Value = "A";

                aixsa.HomingType.Value = HomingMethodType.RLSEDGEINDEX;
                aixsa.Param.HomeInvert.Value = false;
                aixsa.Param.IndexInvert.Value = true;
                aixsa.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                aixsa.Param.FeedOverride.Value = 1;


                aixsa.Param.HommingSpeed.Value = 20000;
                aixsa.Param.HommingAcceleration.Value = 250000;
                aixsa.Param.HommingDecceleration.Value = 250000;
                aixsa.Param.FinalVelociy.Value = 0;
                aixsa.Param.Speed.Value = 250000;
                aixsa.Param.Acceleration.Value = 300000;
                aixsa.Param.Decceleration.Value = 300000;
                aixsa.Param.AccelerationJerk.Value = 30000000;
                aixsa.Param.DeccelerationJerk.Value = 30000000;
                aixsa.Param.SeqAcc.Value = 0;
                aixsa.Param.SeqDcc.Value = 0;
                aixsa.Param.DtoPRatio.Value = 209.7152;
                aixsa.Param.NegSWLimit.Value = -230000;
                aixsa.Param.PosSWLimit.Value = 190000;
                aixsa.Param.POTIndex.Value = 0;
                aixsa.Param.NOTIndex.Value = 0;
                aixsa.Param.HOMEIndex.Value = 1;
                aixsa.Param.TimeOut.Value = 60000;
                aixsa.Param.HomeDistLimit.Value = 435130;
                aixsa.Param.IndexDistLimit.Value = 50000;

                aixsa.Config.StopRate.Value = 0.05;
                aixsa.Config.EStopRate.Value = 0.025;
                aixsa.Config.Inposition.Value = 50;
                aixsa.Config.NearTargetDistance.Value = 500;
                aixsa.Config.VelocityTolerance.Value = 10000;
                aixsa.Config.SettlingTime.Value = 0;

                aixsa.Config.MoterConfig.AmpEnable.Value = true;
                aixsa.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                aixsa.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                aixsa.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                aixsa.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                aixsa.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                aixsa.Config.MoterConfig.AmpDisableDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                aixsa.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                aixsa.Config.MoterConfig.EnableStepLoopBack.Value = false;
                aixsa.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                aixsa.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                aixsa.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                aixsa.Config.MoterConfig.PulseAInv.Value = true;
                aixsa.Config.MoterConfig.PulseBInv.Value = true;

                aixsa.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                aixsa.Config.MoterConfig.AmpFaultDuration.Value = 0;
                aixsa.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                aixsa.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.AmpWarningDuration.Value = 0;
                aixsa.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                aixsa.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                aixsa.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                aixsa.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                aixsa.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                aixsa.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                aixsa.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                aixsa.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                aixsa.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                aixsa.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                aixsa.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                aixsa.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                aixsa.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                aixsa.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                aixsa.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                aixsa.Config.PIDCoeff.GainProportional.Value = 0;
                aixsa.Config.PIDCoeff.GainIntegral.Value = 0;
                aixsa.Config.PIDCoeff.GainDerivative.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                aixsa.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                aixsa.Config.PIDCoeff.DRate.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                aixsa.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                aixsa.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                aixsa.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                aixsa.Config.PIDCoeff.OutputOffset.Value = 16348;
                aixsa.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(aixsa);

                #endregion

                #region Axis Scan

                ProbeAxisObject axisScan = new ProbeAxisObject(0, 9, EnumAxisConstants.SC);
                axisScan.Param.IndexSearchingSpeed.Value = 30000;
                axisScan.Param.HomeOffset.Value = 38360;
                axisScan.Param.ClearedPosition.Value = 0;
                axisScan.Param.HomeShift.Value = 0;
                axisScan.AxisType.Value = EnumAxisConstants.SC;
                axisScan.Label.Value = "SCAN";

                axisScan.HomingType.Value = HomingMethodType.NHPI;
                axisScan.Param.HomeInvert.Value = false;
                axisScan.Param.IndexInvert.Value = true;
                axisScan.Param.FeedOverride.Value = 1;


                axisScan.Param.HommingSpeed.Value = 50000;
                axisScan.Param.HommingAcceleration.Value = 500000;
                axisScan.Param.HommingDecceleration.Value = 500000;
                axisScan.Param.FinalVelociy.Value = 0;
                axisScan.Param.Speed.Value = 90000;
                axisScan.Param.Acceleration.Value = 500000;
                axisScan.Param.Decceleration.Value = 500000;
                axisScan.Param.AccelerationJerk.Value = 5000000;
                axisScan.Param.DeccelerationJerk.Value = 5000000;
                axisScan.Param.SeqAcc.Value = 0;
                axisScan.Param.SeqDcc.Value = 0;
                axisScan.Param.DtoPRatio.Value = 17.47626667;
                axisScan.Param.NegSWLimit.Value = -3000;
                axisScan.Param.PosSWLimit.Value = 185000;
                axisScan.Param.POTIndex.Value = 0;
                axisScan.Param.NOTIndex.Value = 0;
                axisScan.Param.HOMEIndex.Value = 1;
                axisScan.Param.TimeOut.Value = 60000;
                axisScan.Param.HomeDistLimit.Value = 435130;
                axisScan.Param.IndexDistLimit.Value = 50000;

                axisScan.Config.StopRate.Value = 0.05;
                axisScan.Config.EStopRate.Value = 0.025;
                axisScan.Config.Inposition.Value = 50;
                axisScan.Config.NearTargetDistance.Value = 500;
                axisScan.Config.VelocityTolerance.Value = 10000;
                axisScan.Config.SettlingTime.Value = 0;

                axisScan.Config.MoterConfig.AmpEnable.Value = true;
                axisScan.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisScan.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisScan.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisScan.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisScan.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisScan.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisScan.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisScan.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisScan.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisScan.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisScan.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisScan.Config.MoterConfig.PulseAInv.Value = true;
                axisScan.Config.MoterConfig.PulseBInv.Value = true;

                axisScan.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;

                axisScan.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisScan.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisScan.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisScan.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisScan.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisScan.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisScan.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisScan.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWNegLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.HWPosLimitTrigHigh.Value = true;
                axisScan.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisScan.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisScan.Config.MoterConfig.SWNegLimitTrigger.Value = -252222;
                axisScan.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisScan.Config.MoterConfig.SWPosLimitTrigger.Value = 251111;
                axisScan.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisScan.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisScan.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisScan.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisScan.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisScan.Config.PIDCoeff.GainProportional.Value = 0;
                axisScan.Config.PIDCoeff.GainIntegral.Value = 0;
                axisScan.Config.PIDCoeff.GainDerivative.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisScan.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisScan.Config.PIDCoeff.DRate.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisScan.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisScan.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisScan.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisScan.Config.PIDCoeff.OutputOffset.Value = 16348;
                axisScan.Config.PIDCoeff.NoisePositionFFT.Value = 0;

                ProbeAxisProviders.Add(axisScan);

                #endregion


                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        private EventCodeEnum LoaderAxisGP()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            #region Axis LX
            ProbeAxisObject axisuLX = new ProbeAxisObject(0, 0, EnumAxisConstants.LX);
            axisuLX.Param.HomeOffset.Value = 0;
            axisuLX.Param.ClearedPosition.Value = 0;
            axisuLX.Param.HomeShift.Value = 0;
            axisuLX.AxisType.Value = EnumAxisConstants.LX;
            axisuLX.Label.Value = "LX";
            axisuLX.HomingType.Value = HomingMethodType.NH;
            axisuLX.Param.HomeInvert.Value = false;
            axisuLX.Param.IndexInvert.Value = true;
            axisuLX.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLX.Param.FeedOverride.Value = 1;

            axisuLX.Param.HommingSpeed.Value = 50000;
            axisuLX.Param.HommingAcceleration.Value = 1500000;
            axisuLX.Param.HommingDecceleration.Value = 1500000;
            axisuLX.Param.FinalVelociy.Value = 0;
            axisuLX.Param.Speed.Value = 1100000;
            axisuLX.Param.Acceleration.Value = 3000000;
            axisuLX.Param.Decceleration.Value = 3000000;
            axisuLX.Param.AccelerationJerk.Value = 18000000;
            axisuLX.Param.DeccelerationJerk.Value = 18000000;
            axisuLX.Param.SeqSpeed.Value = 1100000;
            axisuLX.Param.SeqAcc.Value = 3000000;
            axisuLX.Param.SeqDcc.Value = 3000000;
            axisuLX.Param.DtoPRatio.Value = 1;
            axisuLX.Param.NegSWLimit.Value = -10000;
            axisuLX.Param.PosSWLimit.Value = 2100000;
            axisuLX.Param.TimeOut.Value = 60000;
            axisuLX.Param.HomeDistLimit.Value = 2200000;
            axisuLX.Param.IndexDistLimit.Value = 50000;

            axisuLX.Config.StopRate.Value = 0.05;
            axisuLX.Config.EStopRate.Value = 0.025;
            axisuLX.Config.Inposition.Value = 350;
            axisuLX.Config.NearTargetDistance.Value = 500;
            axisuLX.Config.VelocityTolerance.Value = 10000;
            axisuLX.Config.SettlingTime.Value = 0;

            axisuLX.Config.MoterConfig.AmpEnable.Value = true;
            axisuLX.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLX.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLX.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLX.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLX.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLX.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLX.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLX.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLX.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLX);

            #endregion
            #region Axis LZM
            ProbeAxisObject axisuLZ = new ProbeAxisObject(0, 0, EnumAxisConstants.LZM);
            axisuLZ.Param.HomeOffset.Value = 0;
            axisuLZ.Param.ClearedPosition.Value = 0;
            axisuLZ.Param.HomeShift.Value = 0;
            axisuLZ.AxisType.Value = EnumAxisConstants.LZM;
            axisuLZ.Label.Value = "LZM";
            axisuLZ.HomingType.Value = HomingMethodType.NH;
            axisuLZ.Param.HomeInvert.Value = false;
            axisuLZ.Param.IndexInvert.Value = true;
            axisuLZ.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLZ.Param.FeedOverride.Value = 1;

            axisuLZ.Param.HommingSpeed.Value = 50000;
            axisuLZ.Param.HommingAcceleration.Value = 1500000;
            axisuLZ.Param.HommingDecceleration.Value = 1500000;
            axisuLZ.Param.FinalVelociy.Value = 0;
            axisuLZ.Param.Speed.Value = 580000;
            axisuLZ.Param.Acceleration.Value = 1500000;
            axisuLZ.Param.Decceleration.Value = 1500000;
            axisuLZ.Param.AccelerationJerk.Value = 15000000;
            axisuLZ.Param.DeccelerationJerk.Value = 15000000;
            axisuLZ.Param.SeqSpeed.Value = 580000;
            axisuLZ.Param.SeqAcc.Value = 1500000;
            axisuLZ.Param.SeqDcc.Value = 1500000;
            axisuLZ.Param.DtoPRatio.Value = 1;
            axisuLZ.Param.NegSWLimit.Value = -10000;
            axisuLZ.Param.PosSWLimit.Value = 1700000;
            axisuLZ.Param.TimeOut.Value = 60000;
            axisuLZ.Param.HomeDistLimit.Value = 2200000;
            axisuLZ.Param.IndexDistLimit.Value = 50000;

            axisuLZ.Config.StopRate.Value = 0.05;
            axisuLZ.Config.EStopRate.Value = 0.025;
            axisuLZ.Config.Inposition.Value = 350;
            axisuLZ.Config.NearTargetDistance.Value = 500;
            axisuLZ.Config.VelocityTolerance.Value = 10000;
            axisuLZ.Config.SettlingTime.Value = 0;

            axisuLZ.Config.MoterConfig.AmpEnable.Value = true;
            axisuLZ.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLZ.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLZ.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLZ.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLZ.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLZ.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLZ.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLZ.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLZ.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLZ);

            #endregion
            #region Axis LZS
            ProbeAxisObject axisuLZS = new ProbeAxisObject(0, 0, EnumAxisConstants.LZS);
            axisuLZS.Param.HomeOffset.Value = 0;
            axisuLZS.Param.ClearedPosition.Value = 0;
            axisuLZS.Param.HomeShift.Value = 0;
            axisuLZS.AxisType.Value = EnumAxisConstants.LZS;
            axisuLZS.Label.Value = "LZS";
            axisuLZS.HomingType.Value = HomingMethodType.NH;
            axisuLZS.Param.HomeInvert.Value = false;
            axisuLZS.Param.IndexInvert.Value = true;
            axisuLZS.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLZS.Param.FeedOverride.Value = 1;

            axisuLZS.Param.HommingSpeed.Value = 50000;
            axisuLZS.Param.HommingAcceleration.Value = 1500000;
            axisuLZS.Param.HommingDecceleration.Value = 1500000;
            axisuLZS.Param.FinalVelociy.Value = 0;
            axisuLZS.Param.Speed.Value = 580000;
            axisuLZS.Param.Acceleration.Value = 1500000;
            axisuLZS.Param.Decceleration.Value = 1500000;
            axisuLZS.Param.AccelerationJerk.Value = 15000000;
            axisuLZS.Param.DeccelerationJerk.Value = 15000000;
            axisuLZS.Param.SeqSpeed.Value = 580000;
            axisuLZS.Param.SeqAcc.Value = 1500000;
            axisuLZS.Param.SeqDcc.Value = 1500000;
            axisuLZS.Param.DtoPRatio.Value = 1;
            axisuLZS.Param.NegSWLimit.Value = -10000;
            axisuLZS.Param.PosSWLimit.Value = 1700000;
            axisuLZS.Param.TimeOut.Value = 60000;
            axisuLZS.Param.HomeDistLimit.Value = 2200000;
            axisuLZS.Param.IndexDistLimit.Value = 50000;

            axisuLZS.Config.StopRate.Value = 0.05;
            axisuLZS.Config.EStopRate.Value = 0.025;
            axisuLZS.Config.Inposition.Value = 350;
            axisuLZS.Config.NearTargetDistance.Value = 500;
            axisuLZS.Config.VelocityTolerance.Value = 10000;
            axisuLZS.Config.SettlingTime.Value = 0;

            axisuLZS.Config.MoterConfig.AmpEnable.Value = true;
            axisuLZS.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLZS.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLZS.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLZS.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLZS.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLZS.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLZS.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLZS.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLZS.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLZS);

            #endregion
            #region Axis LW
            ProbeAxisObject axisuLW = new ProbeAxisObject(0, 0, EnumAxisConstants.LW);
            axisuLW.Param.HomeOffset.Value = 0;
            axisuLW.Param.ClearedPosition.Value = 0;
            axisuLW.Param.HomeShift.Value = 0;
            axisuLW.AxisType.Value = EnumAxisConstants.LW;
            axisuLW.Label.Value = "LW";
            axisuLW.HomingType.Value = HomingMethodType.NH;
            axisuLW.Param.HomeInvert.Value = false;
            axisuLW.Param.IndexInvert.Value = true;
            axisuLW.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLW.Param.FeedOverride.Value = 1;

            axisuLW.Param.HommingSpeed.Value = 50000;
            axisuLW.Param.HommingAcceleration.Value = 1500000;
            axisuLW.Param.HommingDecceleration.Value = 1500000;
            axisuLW.Param.FinalVelociy.Value = 0;
            axisuLW.Param.Speed.Value = 390000;
            axisuLW.Param.Acceleration.Value = 540000;
            axisuLW.Param.Decceleration.Value = 540000;
            axisuLW.Param.AccelerationJerk.Value = 5400000;
            axisuLW.Param.DeccelerationJerk.Value = 5400000;
            axisuLW.Param.SeqSpeed.Value = 390000;
            axisuLW.Param.SeqAcc.Value = 540000;
            axisuLW.Param.SeqDcc.Value = 540000;
            axisuLW.Param.DtoPRatio.Value = 1;
            axisuLW.Param.NegSWLimit.Value = -10000;
            axisuLW.Param.PosSWLimit.Value = 2100000;
            axisuLW.Param.TimeOut.Value = 60000;
            axisuLW.Param.HomeDistLimit.Value = 2200000;
            axisuLW.Param.IndexDistLimit.Value = 50000;

            axisuLW.Config.StopRate.Value = 0.05;
            axisuLW.Config.EStopRate.Value = 0.025;
            axisuLW.Config.Inposition.Value = 350;
            axisuLW.Config.NearTargetDistance.Value = 500;
            axisuLW.Config.VelocityTolerance.Value = 10000;
            axisuLW.Config.SettlingTime.Value = 0;

            axisuLW.Config.MoterConfig.AmpEnable.Value = true;
            axisuLW.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLW.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLW.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLW.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLW.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLW.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLW.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLW.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLW.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLW);

            #endregion
            #region Axis LB
            ProbeAxisObject axisuLB = new ProbeAxisObject(0, 0, EnumAxisConstants.LB);
            axisuLB.Param.HomeOffset.Value = 0;
            axisuLB.Param.ClearedPosition.Value = 0;
            axisuLB.Param.HomeShift.Value = 0;
            axisuLB.AxisType.Value = EnumAxisConstants.LB;
            axisuLB.Label.Value = "LB";
            axisuLB.HomingType.Value = HomingMethodType.NH;
            axisuLB.Param.HomeInvert.Value = false;
            axisuLB.Param.IndexInvert.Value = true;
            axisuLB.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLB.Param.FeedOverride.Value = 1;

            axisuLB.Param.HommingSpeed.Value = 50000;
            axisuLB.Param.HommingAcceleration.Value = 1500000;
            axisuLB.Param.HommingDecceleration.Value = 1500000;
            axisuLB.Param.FinalVelociy.Value = 0;
            axisuLB.Param.Speed.Value = 550000;
            axisuLB.Param.Acceleration.Value = 550000;
            axisuLB.Param.Decceleration.Value = 550000;
            axisuLB.Param.AccelerationJerk.Value = 5500000;
            axisuLB.Param.DeccelerationJerk.Value = 5500000;
            axisuLB.Param.SeqSpeed.Value = 550000;
            axisuLB.Param.SeqAcc.Value = 550000;
            axisuLB.Param.SeqDcc.Value = 550000;
            axisuLB.Param.DtoPRatio.Value = 1;
            axisuLB.Param.NegSWLimit.Value = -10000;
            axisuLB.Param.PosSWLimit.Value = 2100000;
            axisuLB.Param.TimeOut.Value = 60000;
            axisuLB.Param.HomeDistLimit.Value = 2200000;
            axisuLB.Param.IndexDistLimit.Value = 50000;

            axisuLB.Config.StopRate.Value = 0.05;
            axisuLB.Config.EStopRate.Value = 0.025;
            axisuLB.Config.Inposition.Value = 350;
            axisuLB.Config.NearTargetDistance.Value = 500;
            axisuLB.Config.VelocityTolerance.Value = 10000;
            axisuLB.Config.SettlingTime.Value = 0;

            axisuLB.Config.MoterConfig.AmpEnable.Value = true;
            axisuLB.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLB.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLB.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLB.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLB.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLB.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLB.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLB.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLB.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLB);

            #endregion
            #region Axis LUD
            ProbeAxisObject axisuLUD = new ProbeAxisObject(0, 0, EnumAxisConstants.LUD);
            axisuLUD.Param.HomeOffset.Value = 0;
            axisuLUD.Param.ClearedPosition.Value = 0;
            axisuLUD.Param.HomeShift.Value = 0;
            axisuLUD.AxisType.Value = EnumAxisConstants.LUD;
            axisuLUD.Label.Value = "LUD";
            axisuLUD.HomingType.Value = HomingMethodType.NH;
            axisuLUD.Param.HomeInvert.Value = false;
            axisuLUD.Param.IndexInvert.Value = true;
            axisuLUD.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLUD.Param.FeedOverride.Value = 1;

            axisuLUD.Param.HommingSpeed.Value = 50000;
            axisuLUD.Param.HommingAcceleration.Value = 1500000;
            axisuLUD.Param.HommingDecceleration.Value = 1500000;
            axisuLUD.Param.FinalVelociy.Value = 0;
            axisuLUD.Param.Speed.Value = 2475000;
            axisuLUD.Param.Acceleration.Value = 3500000;
            axisuLUD.Param.Decceleration.Value = 3500000;
            axisuLUD.Param.AccelerationJerk.Value = 35000000;
            axisuLUD.Param.DeccelerationJerk.Value = 35000000;
            axisuLUD.Param.SeqSpeed.Value = 2475000;
            axisuLUD.Param.SeqAcc.Value = 3500000;
            axisuLUD.Param.SeqDcc.Value = 3500000;
            axisuLUD.Param.DtoPRatio.Value = 1;
            axisuLUD.Param.NegSWLimit.Value = -10000;
            axisuLUD.Param.PosSWLimit.Value = 450000;
            axisuLUD.Param.TimeOut.Value = 60000;
            axisuLUD.Param.HomeDistLimit.Value = 2200000;
            axisuLUD.Param.IndexDistLimit.Value = 50000;

            axisuLUD.Config.StopRate.Value = 0.05;
            axisuLUD.Config.EStopRate.Value = 0.025;
            axisuLUD.Config.Inposition.Value = 350;
            axisuLUD.Config.NearTargetDistance.Value = 500;
            axisuLUD.Config.VelocityTolerance.Value = 10000;
            axisuLUD.Config.SettlingTime.Value = 0;

            axisuLUD.Config.MoterConfig.AmpEnable.Value = true;
            axisuLUD.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLUD.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLUD.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLUD.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLUD.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLUD.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLUD.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLUD.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLUD.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLUD);

            #endregion
            #region Axis LUU
            ProbeAxisObject axisuLUU = new ProbeAxisObject(0, 0, EnumAxisConstants.LUU);
            axisuLUU.Param.HomeOffset.Value = 0;
            axisuLUU.Param.ClearedPosition.Value = 0;
            axisuLUU.Param.HomeShift.Value = 0;
            axisuLUU.AxisType.Value = EnumAxisConstants.LUU;
            axisuLUU.Label.Value = "LUU";
            axisuLUU.HomingType.Value = HomingMethodType.NH;
            axisuLUU.Param.HomeInvert.Value = false;
            axisuLUU.Param.IndexInvert.Value = true;
            axisuLUU.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLUU.Param.FeedOverride.Value = 1;

            axisuLUU.Param.HommingSpeed.Value = 50000;
            axisuLUU.Param.HommingAcceleration.Value = 1500000;
            axisuLUU.Param.HommingDecceleration.Value = 1500000;
            axisuLUU.Param.FinalVelociy.Value = 0;
            axisuLUU.Param.Speed.Value = 2475000;
            axisuLUU.Param.Acceleration.Value = 3500000;
            axisuLUU.Param.Decceleration.Value = 3500000;
            axisuLUU.Param.AccelerationJerk.Value = 35000000;
            axisuLUU.Param.DeccelerationJerk.Value = 35000000;
            axisuLUU.Param.SeqSpeed.Value = 2475000;
            axisuLUU.Param.SeqAcc.Value = 3500000;
            axisuLUU.Param.SeqDcc.Value = 3500000;
            axisuLUU.Param.DtoPRatio.Value = 1;
            axisuLUU.Param.NegSWLimit.Value = -10000;
            axisuLUU.Param.PosSWLimit.Value = 450000;
            axisuLUU.Param.TimeOut.Value = 60000;
            axisuLUU.Param.HomeDistLimit.Value = 2200000;
            axisuLUU.Param.IndexDistLimit.Value = 50000;

            axisuLUU.Config.StopRate.Value = 0.05;
            axisuLUU.Config.EStopRate.Value = 0.025;
            axisuLUU.Config.Inposition.Value = 350;
            axisuLUU.Config.NearTargetDistance.Value = 500;
            axisuLUU.Config.VelocityTolerance.Value = 10000;
            axisuLUU.Config.SettlingTime.Value = 0;

            axisuLUU.Config.MoterConfig.AmpEnable.Value = true;
            axisuLUU.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLUU.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLUU.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLUU.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLUU.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLUU.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLUU.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLUU.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLUU.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLUU);

            #endregion
            #region Axis LCC
            ProbeAxisObject axisuLCC = new ProbeAxisObject(0, 0, EnumAxisConstants.LCC);
            axisuLCC.Param.HomeOffset.Value = 0;
            axisuLCC.Param.ClearedPosition.Value = 0;
            axisuLCC.Param.HomeShift.Value = 0;
            axisuLCC.AxisType.Value = EnumAxisConstants.LCC;
            axisuLCC.Label.Value = "LCC";
            axisuLCC.HomingType.Value = HomingMethodType.NH;
            axisuLCC.Param.HomeInvert.Value = false;
            axisuLCC.Param.IndexInvert.Value = true;
            axisuLCC.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuLCC.Param.FeedOverride.Value = 1;

            axisuLCC.Param.HommingSpeed.Value = 50000;
            axisuLCC.Param.HommingAcceleration.Value = 1500000;
            axisuLCC.Param.HommingDecceleration.Value = 1500000;
            axisuLCC.Param.FinalVelociy.Value = 0;
            axisuLCC.Param.Speed.Value = 330000;
            axisuLCC.Param.Acceleration.Value = 1500000;
            axisuLCC.Param.Decceleration.Value = 1500000;
            axisuLCC.Param.AccelerationJerk.Value = 15000000;
            axisuLCC.Param.DeccelerationJerk.Value = 15000000;
            axisuLCC.Param.SeqSpeed.Value = 330000;
            axisuLCC.Param.SeqAcc.Value = 1500000;
            axisuLCC.Param.SeqDcc.Value = 1500000;
            axisuLCC.Param.DtoPRatio.Value = 1;
            axisuLCC.Param.NegSWLimit.Value = -10000;
            axisuLCC.Param.PosSWLimit.Value = 450000;
            axisuLCC.Param.TimeOut.Value = 60000;
            axisuLCC.Param.HomeDistLimit.Value = 2200000;
            axisuLCC.Param.IndexDistLimit.Value = 50000;

            axisuLCC.Config.StopRate.Value = 0.05;
            axisuLCC.Config.EStopRate.Value = 0.025;
            axisuLCC.Config.Inposition.Value = 350;
            axisuLCC.Config.NearTargetDistance.Value = 500;
            axisuLCC.Config.VelocityTolerance.Value = 10000;
            axisuLCC.Config.SettlingTime.Value = 0;

            axisuLCC.Config.MoterConfig.AmpEnable.Value = true;
            axisuLCC.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuLCC.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuLCC.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuLCC.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuLCC.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuLCC.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuLCC.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuLCC.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuLCC.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuLCC);

            #endregion
            #region Axis FC1
            ProbeAxisObject axisuFC1 = new ProbeAxisObject(0, 0, EnumAxisConstants.FC1);
            axisuFC1.Param.HomeOffset.Value = 0;
            axisuFC1.Param.ClearedPosition.Value = 0;
            axisuFC1.Param.HomeShift.Value = 0;
            axisuFC1.AxisType.Value = EnumAxisConstants.FC1;
            axisuFC1.Label.Value = "FC1";
            axisuFC1.HomingType.Value = HomingMethodType.NH;
            axisuFC1.Param.HomeInvert.Value = false;
            axisuFC1.Param.IndexInvert.Value = true;
            axisuFC1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuFC1.Param.FeedOverride.Value = 1;

            axisuFC1.Param.HommingSpeed.Value = 50000;
            axisuFC1.Param.HommingAcceleration.Value = 1500000;
            axisuFC1.Param.HommingDecceleration.Value = 1500000;
            axisuFC1.Param.FinalVelociy.Value = 0;
            axisuFC1.Param.Speed.Value = 2200000;
            axisuFC1.Param.Acceleration.Value = 1500000;
            axisuFC1.Param.Decceleration.Value = 1500000;
            axisuFC1.Param.AccelerationJerk.Value = 15000000;
            axisuFC1.Param.DeccelerationJerk.Value = 15000000;
            axisuFC1.Param.SeqSpeed.Value = 2200000;
            axisuFC1.Param.SeqAcc.Value = 1500000;
            axisuFC1.Param.SeqDcc.Value = 1500000;
            axisuFC1.Param.DtoPRatio.Value = 1;
            axisuFC1.Param.NegSWLimit.Value = -1000;
            axisuFC1.Param.PosSWLimit.Value = 320000;
            axisuFC1.Param.TimeOut.Value = 60000;
            axisuFC1.Param.HomeDistLimit.Value = 2200000;
            axisuFC1.Param.IndexDistLimit.Value = 50000;

            axisuFC1.Config.StopRate.Value = 0.05;
            axisuFC1.Config.EStopRate.Value = 0.025;
            axisuFC1.Config.Inposition.Value = 350;
            axisuFC1.Config.NearTargetDistance.Value = 500;
            axisuFC1.Config.VelocityTolerance.Value = 10000;
            axisuFC1.Config.SettlingTime.Value = 0;

            axisuFC1.Config.MoterConfig.AmpEnable.Value = true;
            axisuFC1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuFC1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuFC1.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuFC1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuFC1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuFC1.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuFC1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuFC1.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuFC1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuFC1);

            #endregion
            #region Axis FC2
            ProbeAxisObject axisuFC2 = new ProbeAxisObject(0, 0, EnumAxisConstants.FC2);
            axisuFC2.Param.HomeOffset.Value = 0;
            axisuFC2.Param.ClearedPosition.Value = 0;
            axisuFC2.Param.HomeShift.Value = 0;
            axisuFC2.AxisType.Value = EnumAxisConstants.FC2;
            axisuFC2.Label.Value = "FC2";
            axisuFC2.HomingType.Value = HomingMethodType.NH;
            axisuFC2.Param.HomeInvert.Value = false;
            axisuFC2.Param.IndexInvert.Value = true;
            axisuFC2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuFC2.Param.FeedOverride.Value = 1;

            axisuFC2.Param.HommingSpeed.Value = 50000;
            axisuFC2.Param.HommingAcceleration.Value = 1500000;
            axisuFC2.Param.HommingDecceleration.Value = 1500000;
            axisuFC2.Param.FinalVelociy.Value = 0;
            axisuFC2.Param.Speed.Value = 2200000;
            axisuFC2.Param.Acceleration.Value = 1500000;
            axisuFC2.Param.Decceleration.Value = 1500000;
            axisuFC2.Param.AccelerationJerk.Value = 15000000;
            axisuFC2.Param.DeccelerationJerk.Value = 15000000;
            axisuFC2.Param.SeqSpeed.Value = 2200000;
            axisuFC2.Param.SeqAcc.Value = 1500000;
            axisuFC2.Param.SeqDcc.Value = 1500000;
            axisuFC2.Param.DtoPRatio.Value = 1;
            axisuFC2.Param.NegSWLimit.Value = -1000;
            axisuFC2.Param.PosSWLimit.Value = 320000;
            axisuFC2.Param.TimeOut.Value = 60000;
            axisuFC2.Param.HomeDistLimit.Value = 2200000;
            axisuFC2.Param.IndexDistLimit.Value = 50000;

            axisuFC2.Config.StopRate.Value = 0.05;
            axisuFC2.Config.EStopRate.Value = 0.025;
            axisuFC2.Config.Inposition.Value = 350;
            axisuFC2.Config.NearTargetDistance.Value = 500;
            axisuFC2.Config.VelocityTolerance.Value = 10000;
            axisuFC2.Config.SettlingTime.Value = 0;

            axisuFC2.Config.MoterConfig.AmpEnable.Value = true;
            axisuFC2.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuFC2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuFC2.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuFC2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuFC2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuFC2.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuFC2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuFC2.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuFC2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuFC2);

            #endregion
            #region Axis FC3
            ProbeAxisObject axisuFC3 = new ProbeAxisObject(0, 0, EnumAxisConstants.FC3);
            axisuFC3.Param.HomeOffset.Value = 0;
            axisuFC3.Param.ClearedPosition.Value = 0;
            axisuFC3.Param.HomeShift.Value = 0;
            axisuFC3.AxisType.Value = EnumAxisConstants.FC3;
            axisuFC3.Label.Value = "FC3";
            axisuFC3.HomingType.Value = HomingMethodType.NH;
            axisuFC3.Param.HomeInvert.Value = false;
            axisuFC3.Param.IndexInvert.Value = true;
            axisuFC3.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
            axisuFC3.Param.FeedOverride.Value = 1;

            axisuFC3.Param.HommingSpeed.Value = 50000;
            axisuFC3.Param.HommingAcceleration.Value = 1500000;
            axisuFC3.Param.HommingDecceleration.Value = 1500000;
            axisuFC3.Param.FinalVelociy.Value = 0;
            axisuFC3.Param.Speed.Value = 2200000;
            axisuFC3.Param.Acceleration.Value = 1500000;
            axisuFC3.Param.Decceleration.Value = 1500000;
            axisuFC3.Param.AccelerationJerk.Value = 15000000;
            axisuFC3.Param.DeccelerationJerk.Value = 15000000;
            axisuFC3.Param.SeqSpeed.Value = 2200000;
            axisuFC3.Param.SeqAcc.Value = 1500000;
            axisuFC3.Param.SeqDcc.Value = 1500000;
            axisuFC3.Param.DtoPRatio.Value = 1;
            axisuFC3.Param.NegSWLimit.Value = -1000;
            axisuFC3.Param.PosSWLimit.Value = 320000;
            axisuFC3.Param.TimeOut.Value = 60000;
            axisuFC3.Param.HomeDistLimit.Value = 2200000;
            axisuFC3.Param.IndexDistLimit.Value = 50000;

            axisuFC3.Config.StopRate.Value = 0.05;
            axisuFC3.Config.EStopRate.Value = 0.025;
            axisuFC3.Config.Inposition.Value = 350;
            axisuFC3.Config.NearTargetDistance.Value = 500;
            axisuFC3.Config.VelocityTolerance.Value = 10000;
            axisuFC3.Config.SettlingTime.Value = 0;

            axisuFC3.Config.MoterConfig.AmpEnable.Value = true;
            axisuFC3.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
            axisuFC3.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

            axisuFC3.Config.MoterConfig.AmpDisableDelay.Value = 0;
            axisuFC3.Config.MoterConfig.BrakeApplyDelay.Value = 0;
            axisuFC3.Config.MoterConfig.BrakeReleaseDelay.Value = 0;

            axisuFC3.Config.MoterConfig.SWNegLimitTrigger.Value = -10000;
            axisuFC3.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
            axisuFC3.Config.MoterConfig.SWPosLimitTrigger.Value = 2100000;
            axisuFC3.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;
            ProbeAxisProviders.Add(axisuFC3);

            #endregion
            ret = EventCodeEnum.NONE;
            return ret;
        }
    }
    [Serializable()]
    public class StageAxes : ProbeAxes, ISystemParameterizable, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
      
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[TemplateEntryInfo] [Method = Init] [Error = {err}]");
                    retval = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public string FilePath { get; } = "";
        public string FileName { get; } = "StageAxesObjectParam.json";

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //StageAxisDefualtEmul();
                //StageAxisDefualt_MachineOPusV();

                //StageAxisDefault_BSCI1();     // sebas 임시 주석처리

                HomingGroups = new List<HomingGroup>()
                    {
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.Z, EnumAxisConstants.NC }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.Y, EnumAxisConstants.C }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.X }),
                    };

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //StageAxisDefualtEmul();
                //StageAxisDefualt();
                //StageAxisDefault_BSCI1();
                // StageAxisDefualt_OPUSV_Machine3();

                StageAxisDefualt_Bonder();  // 251017 sebas

                HomingGroups = new List<HomingGroup>()
                    {
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.Z, EnumAxisConstants.NC }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.Y, EnumAxisConstants.C }),
                        new HomingGroup(new List<EnumAxisConstants> {EnumAxisConstants.X }),
                    };

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

        }
        public EventCodeEnum StageAxisDefualtEmul()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region ==> Default
                #region Axis X

                ProbeAxisObject axisx = new ProbeAxisObject(0, 0, EnumAxisConstants.X);
                axisx.Param.IndexSearchingSpeed.Value = 6000;
                axisx.Param.HomeOffset.Value = -160000;
                axisx.Param.HomeShift.Value = 6500;
                axisx.AxisType.Value = EnumAxisConstants.X;
                axisx.Label.Value = "X";

                axisx.HomingType.Value = HomingMethodType.NHPI;
                axisx.Param.HomeInvert.Value = false;
                axisx.Param.IndexInvert.Value = true;


                axisx.Param.HommingSpeed.Value = 80000;
                axisx.Param.HommingAcceleration.Value = 800000;
                axisx.Param.HommingDecceleration.Value = 800000;
                axisx.Param.FinalVelociy.Value = 0;
                axisx.Param.Speed.Value = 2000000;
                axisx.Param.Acceleration.Value = 20000000;
                axisx.Param.Decceleration.Value = 20000000;
                axisx.Param.AccelerationJerk.Value = 66;
                axisx.Param.DeccelerationJerk.Value = 66;
                axisx.Param.SeqAcc.Value = 0;
                axisx.Param.SeqDcc.Value = 0;
                axisx.Param.DtoPRatio.Value = 559.21753333333333333333333333333;
                //axisx.Param.NegSWLimit.Value = -178000;
                //axisx.Param.PosSWLimit.Value = 188000;
                axisx.Param.NegSWLimit.Value = -195000;
                axisx.Param.PosSWLimit.Value = 193000;
                axisx.Param.POTIndex.Value = 0;
                axisx.Param.NOTIndex.Value = 0;
                axisx.Param.HOMEIndex.Value = 1;
                axisx.Param.TimeOut.Value = 10000;
                axisx.Param.HomeDistLimit.Value = 366000;
                axisx.Param.IndexDistLimit.Value = 50000;

                axisx.Config.StopRate.Value = 0.05;
                axisx.Config.EStopRate.Value = 0.025;
                axisx.Config.Inposition.Value = 118;
                axisx.Config.NearTargetDistance.Value = 5590;
                axisx.Config.VelocityTolerance.Value = 559241;
                axisx.Config.SettlingTime.Value = 0;

                axisx.Config.MoterConfig.AmpEnable.Value = false;
                axisx.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisx.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisx.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisx.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisx.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisx.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisx.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisx.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisx.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisx.Config.MoterConfig.PulseAInv.Value = false;
                axisx.Config.MoterConfig.PulseBInv.Value = false;

                axisx.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisx.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisx.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisx.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisx.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisx.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisx.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisx.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisx.Config.MoterConfig.SWNegLimitTrigger.Value = -99540720.933333333333333333333333;
                axisx.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.SWPosLimitTrigger.Value = 105132896.26666666666666666666667;
                axisx.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisx.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisx.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisx.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisx.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisx.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisx.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisx.Config.PIDCoeff.GainDerivative.Value = 5.5;
                axisx.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardAcceleration.Value = 150;
                axisx.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisx.Config.PIDCoeff.DRate.Value = 0;
                axisx.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisx.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputOffset.Value = 0;
                axisx.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisx);

                #endregion
                #region AxisY

                ProbeAxisObject axisy = new ProbeAxisObject(0, 1, EnumAxisConstants.Y);
                axisy.Param.IndexSearchingSpeed.Value = 10000;
                axisy.Param.HomeOffset.Value = 573500;
                axisy.Param.HomeShift.Value = -10000;
                axisy.AxisType.Value = EnumAxisConstants.Y;
                axisy.Label.Value = "Y";

                axisy.HomingType.Value = HomingMethodType.PHNI;
                axisy.Param.HomeInvert.Value = false;
                axisy.Param.IndexInvert.Value = true;


                axisy.Param.HommingSpeed.Value = 80000;
                axisy.Param.HommingAcceleration.Value = 800000;
                axisy.Param.HommingDecceleration.Value = 800000;
                axisy.Param.FinalVelociy.Value = 0;
                axisy.Param.Speed.Value = 2000000;
                axisy.Param.Acceleration.Value = 18000000;
                axisy.Param.Decceleration.Value = 18000000;
                axisy.Param.AccelerationJerk.Value = 66;
                axisy.Param.DeccelerationJerk.Value = 66;
                axisy.Param.SeqAcc.Value = 0;
                axisy.Param.SeqDcc.Value = 0;
                axisy.Param.DtoPRatio.Value = 559.20053333333333333333333333333;
                axisy.Param.NegSWLimit.Value = -198000;
                axisy.Param.PosSWLimit.Value = 598000;
                axisy.Param.POTIndex.Value = 0;
                axisy.Param.NOTIndex.Value = 0;
                axisy.Param.HOMEIndex.Value = 1;
                axisy.Param.TimeOut.Value = 10000;
                axisy.Param.HomeDistLimit.Value = 796000;
                axisy.Param.IndexDistLimit.Value = 50000;

                axisy.Config.StopRate.Value = 0.05;
                axisy.Config.EStopRate.Value = 0.025;
                axisy.Config.Inposition.Value = 118;
                axisy.Config.NearTargetDistance.Value = 5590;
                axisy.Config.VelocityTolerance.Value = 559241;
                axisy.Config.SettlingTime.Value = 0;

                axisy.Config.MoterConfig.AmpEnable.Value = false;
                axisy.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisy.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisy.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisy.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisy.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisy.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisy.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisy.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;
                axisy.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;

                axisy.Config.MoterConfig.PulseAInv.Value = false;
                axisy.Config.MoterConfig.PulseBInv.Value = false;

                axisy.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisy.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisy.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisy.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisy.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisy.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisy.Config.MoterConfig.SWNegLimitTrigger.Value = -110721705.6;
                axisy.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisy.Config.MoterConfig.SWPosLimitTrigger.Value = 334401918.93333333333333333333333;
                axisy.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisy.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisy.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisy.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisy.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisy.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisy.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisy.Config.PIDCoeff.GainDerivative.Value = 6.5;
                axisy.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardAcceleration.Value = 145;
                axisy.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisy.Config.PIDCoeff.DRate.Value = 0;
                axisy.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisy.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputOffset.Value = 0;
                axisy.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisy);

                #endregion
                #region Axis Z

                ProbeAxisObject axisz = new ProbeAxisObject(0, 2, EnumAxisConstants.Z);
                axisz.Param.IndexSearchingSpeed.Value = 10000;
                axisz.Param.HomeOffset.Value = -36562;
                axisz.Param.HomeShift.Value = 0;
                axisz.AxisType.Value = EnumAxisConstants.Z;
                axisz.Label.Value = "Z";

                axisz.HomingType.Value = HomingMethodType.NHPI;
                axisz.Param.HomeInvert.Value = false;
                axisz.Param.IndexInvert.Value = false;


                axisz.Param.HommingSpeed.Value = 2000;
                axisz.Param.HommingAcceleration.Value = 100000;
                axisz.Param.HommingDecceleration.Value = 100000;
                axisz.Param.FinalVelociy.Value = 0;
                axisz.Param.Speed.Value = 200000;
                axisz.Param.Acceleration.Value = 20000000;
                axisz.Param.Decceleration.Value = 20000000;
                axisz.Param.AccelerationJerk.Value = 66;
                axisz.Param.DeccelerationJerk.Value = 66;
                axisz.Param.SeqAcc.Value = 0;
                axisz.Param.SeqDcc.Value = 0;
                axisz.Param.DtoPRatio.Value = 19.299142580828183215633093937922;
                axisz.Param.NegSWLimit.Value = -88000;
                axisz.Param.PosSWLimit.Value = 0;
                axisz.Param.POTIndex.Value = 0;
                axisz.Param.NOTIndex.Value = 0;
                axisz.Param.HOMEIndex.Value = 1;
                axisz.Param.TimeOut.Value = 10000;
                axisz.Param.HomeDistLimit.Value = 49500;
                axisz.Param.IndexDistLimit.Value = 7000;

                axisz.Config.StopRate.Value = 0.05;
                axisz.Config.EStopRate.Value = 0.025;
                axisz.Config.Inposition.Value = 1118;
                axisz.Config.NearTargetDistance.Value = 5590;
                axisz.Config.VelocityTolerance.Value = 559241;
                axisz.Config.SettlingTime.Value = 0;

                axisz.Config.MoterConfig.AmpEnable.Value = true;
                axisz.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz.Config.MoterConfig.PulseAInv.Value = false;
                axisz.Config.MoterConfig.PulseBInv.Value = false;

                axisz.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz.Config.PIDCoeff.GainProportional.Value = 0;
                axisz.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz.Config.PIDCoeff.DRate.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz);

                #endregion
                #region Axis C

                ProbeAxisObject axist = new ProbeAxisObject(0, 3, EnumAxisConstants.C);
                axist.Param.IndexSearchingSpeed.Value = 1000;
                axist.Param.HomeOffset.Value = 15000;
                axist.Param.HomeShift.Value = 0;
                axist.AxisType.Value = EnumAxisConstants.C;
                axist.Label.Value = "T";

                axist.HomingType.Value = HomingMethodType.NH;
                axist.Param.HomeInvert.Value = false;
                axist.Param.IndexInvert.Value = true;

                axist.Param.HommingSpeed.Value = 1500;
                axist.Param.HommingAcceleration.Value = 1500;
                axist.Param.HommingDecceleration.Value = 1500;
                axist.Param.FinalVelociy.Value = 0;
                axist.Param.Speed.Value = 200000;
                axist.Param.Acceleration.Value = 200000;
                axist.Param.Decceleration.Value = 200000;
                axist.Param.AccelerationJerk.Value = 66;
                axist.Param.DeccelerationJerk.Value = 66;
                axist.Param.SeqAcc.Value = 0;
                axist.Param.SeqDcc.Value = 0;
                axist.Param.DtoPRatio.Value = 2.2727;
                axist.Param.NegSWLimit.Value = -51000;
                axist.Param.PosSWLimit.Value = 51000;
                axist.Param.POTIndex.Value = 0;
                axist.Param.NOTIndex.Value = 0;
                axist.Param.HOMEIndex.Value = 1;
                axist.Param.TimeOut.Value = 30000;
                axist.Param.HomeDistLimit.Value = 102000;
                axist.Param.IndexDistLimit.Value = 0;

                axist.Config.StopRate.Value = 0.05;
                axist.Config.EStopRate.Value = 0.025;
                axist.Config.Inposition.Value = 200;
                axist.Config.NearTargetDistance.Value = 500;
                axist.Config.VelocityTolerance.Value = 40000;
                axist.Config.SettlingTime.Value = 0;

                axist.Config.MoterConfig.AmpEnable.Value = false;
                axist.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axist.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axist.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axist.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axist.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axist.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axist.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axist.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axist.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axist.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axist.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axist.Config.MoterConfig.PulseAInv.Value = false;
                axist.Config.MoterConfig.PulseBInv.Value = false;

                axist.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axist.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axist.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axist.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axist.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axist.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axist.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axist.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axist.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axist.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axist.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axist.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axist.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axist.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axist.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axist.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axist.Config.PIDCoeff.GainProportional.Value = 450;
                axist.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axist.Config.PIDCoeff.GainDerivative.Value = 2500;
                axist.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axist.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axist.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axist.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axist.Config.PIDCoeff.DRate.Value = 0;
                axist.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axist.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axist.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axist.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axist.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axist.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axist.Config.PIDCoeff.OutputOffset.Value = -32768;
                axist.Config.PIDCoeff.NoisePositionFFT.Value = 0;
                #endregion
                #region Axis PZ

                ProbeAxisObject axispz = new ProbeAxisObject(0, 4, EnumAxisConstants.PZ);
                axispz.Param.IndexSearchingSpeed.Value = 1000;
                axispz.Param.HomeOffset.Value = 0;
                axispz.Param.HomeShift.Value = 0;
                axispz.AxisType.Value = EnumAxisConstants.PZ;
                axispz.Label.Value = "PZ";

                axispz.HomingType.Value = HomingMethodType.NH;
                axispz.Param.HomeInvert.Value = false;
                axispz.Param.IndexInvert.Value = true;

                axispz.Param.HommingSpeed.Value = 20000;
                axispz.Param.HommingAcceleration.Value = 200000;
                axispz.Param.HommingDecceleration.Value = 200000;
                axispz.Param.FinalVelociy.Value = 0;
                axispz.Param.Speed.Value = 2000;
                axispz.Param.Acceleration.Value = 2000;
                axispz.Param.Decceleration.Value = 2000;
                axispz.Param.AccelerationJerk.Value = 66;
                axispz.Param.DeccelerationJerk.Value = 66;
                axispz.Param.SeqAcc.Value = 0;
                axispz.Param.SeqDcc.Value = 0;
                axispz.Param.DtoPRatio.Value = 2.2727;
                axispz.Param.NegSWLimit.Value = -88000;
                axispz.Param.PosSWLimit.Value = 0;
                axispz.Param.POTIndex.Value = 0;
                axispz.Param.NOTIndex.Value = 0;
                axispz.Param.HOMEIndex.Value = 1;
                axispz.Param.TimeOut.Value = 30000;
                axispz.Param.HomeDistLimit.Value = 102000;
                axispz.Param.IndexDistLimit.Value = 0;

                axispz.Config.StopRate.Value = 0.05;
                axispz.Config.EStopRate.Value = 0.025;
                axispz.Config.Inposition.Value = 200;
                axispz.Config.NearTargetDistance.Value = 500;
                axispz.Config.VelocityTolerance.Value = 40000;
                axispz.Config.SettlingTime.Value = 0;

                axispz.Config.MoterConfig.AmpEnable.Value = false;
                axispz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axispz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axispz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axispz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axispz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axispz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axispz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axispz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axispz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axispz.Config.MoterConfig.PulseAInv.Value = false;
                axispz.Config.MoterConfig.PulseBInv.Value = false;

                axispz.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axispz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axispz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axispz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axispz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axispz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axispz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axispz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axispz.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axispz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axispz.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axispz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axispz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axispz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axispz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axispz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axispz.Config.PIDCoeff.GainProportional.Value = 450;
                axispz.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axispz.Config.PIDCoeff.GainDerivative.Value = 2500;
                axispz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axispz.Config.PIDCoeff.DRate.Value = 0;
                axispz.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axispz.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axispz.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axispz.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axispz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axispz.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axispz.Config.PIDCoeff.OutputOffset.Value = -32768;
                axispz.Config.PIDCoeff.NoisePositionFFT.Value = 0;
                #endregion
                ProbeAxisProviders.Add(axist);
                ProbeAxisProviders.Add(axispz);
                #endregion

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        public EventCodeEnum StageAxisDefualt()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region ==> Default
                #region Axis X

                ProbeAxisObject axisx = new ProbeAxisObject(0, 0, EnumAxisConstants.X);
                axisx.Param.IndexSearchingSpeed.Value = 6000;
                axisx.Param.HomeOffset.Value = -160000;
                axisx.Param.HomeShift.Value = 6500;
                axisx.AxisType.Value = EnumAxisConstants.X;
                axisx.Label.Value = "X";

                axisx.HomingType.Value = HomingMethodType.NHPI;
                axisx.Param.HomeInvert.Value = false;
                axisx.Param.IndexInvert.Value = true;


                axisx.Param.HommingSpeed.Value = 80000;
                axisx.Param.HommingAcceleration.Value = 800000;
                axisx.Param.HommingDecceleration.Value = 800000;
                axisx.Param.FinalVelociy.Value = 0;
                axisx.Param.Speed.Value = 200000;
                axisx.Param.Acceleration.Value = 2000000;
                axisx.Param.Decceleration.Value = 2000000;
                axisx.Param.AccelerationJerk.Value = 66;
                axisx.Param.DeccelerationJerk.Value = 66;
                axisx.Param.SeqAcc.Value = 0;
                axisx.Param.SeqDcc.Value = 0;
                axisx.Param.DtoPRatio.Value = 559.21753333333333333333333333333;
                axisx.Param.NegSWLimit.Value = -178000;
                axisx.Param.PosSWLimit.Value = 188000;
                axisx.Param.POTIndex.Value = 0;
                axisx.Param.NOTIndex.Value = 0;
                axisx.Param.HOMEIndex.Value = 1;
                axisx.Param.TimeOut.Value = 10000;
                axisx.Param.HomeDistLimit.Value = 366000;
                axisx.Param.IndexDistLimit.Value = 50000;

                axisx.Config.StopRate.Value = 0.05;
                axisx.Config.EStopRate.Value = 0.025;
                axisx.Config.Inposition.Value = 118;
                axisx.Config.NearTargetDistance.Value = 5590;
                axisx.Config.VelocityTolerance.Value = 559241;
                axisx.Config.SettlingTime.Value = 0;

                axisx.Config.MoterConfig.AmpEnable.Value = false;
                axisx.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisx.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisx.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisx.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisx.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisx.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisx.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisx.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisx.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisx.Config.MoterConfig.PulseAInv.Value = false;
                axisx.Config.MoterConfig.PulseBInv.Value = false;

                axisx.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisx.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisx.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisx.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisx.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisx.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisx.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisx.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisx.Config.MoterConfig.SWNegLimitTrigger.Value = -99540720.933333333333333333333333;
                axisx.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.SWPosLimitTrigger.Value = 105132896.26666666666666666666667;
                axisx.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisx.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisx.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisx.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisx.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisx.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisx.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisx.Config.PIDCoeff.GainDerivative.Value = 5.5;
                axisx.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardAcceleration.Value = 150;
                axisx.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisx.Config.PIDCoeff.DRate.Value = 0;
                axisx.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisx.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputOffset.Value = 0;
                axisx.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisx);

                #endregion
                #region AxisY

                ProbeAxisObject axisy = new ProbeAxisObject(0, 1, EnumAxisConstants.Y);
                axisy.Param.IndexSearchingSpeed.Value = 10000;
                axisy.Param.HomeOffset.Value = 573500;
                axisy.Param.HomeShift.Value = -10000;
                axisy.AxisType.Value = EnumAxisConstants.Y;
                axisy.Label.Value = "Y";

                axisy.HomingType.Value = HomingMethodType.PHNI;
                axisy.Param.HomeInvert.Value = false;
                axisy.Param.IndexInvert.Value = true;


                axisy.Param.HommingSpeed.Value = 80000;
                axisy.Param.HommingAcceleration.Value = 800000;
                axisy.Param.HommingDecceleration.Value = 800000;
                axisy.Param.FinalVelociy.Value = 0;
                axisy.Param.Speed.Value = 200000;
                axisy.Param.Acceleration.Value = 1800000;
                axisy.Param.Decceleration.Value = 1800000;
                axisy.Param.AccelerationJerk.Value = 66;
                axisy.Param.DeccelerationJerk.Value = 66;
                axisy.Param.SeqAcc.Value = 0;
                axisy.Param.SeqDcc.Value = 0;
                axisy.Param.DtoPRatio.Value = 559.20053333333333333333333333333;
                axisy.Param.NegSWLimit.Value = -198000;
                axisy.Param.PosSWLimit.Value = 598000;
                axisy.Param.POTIndex.Value = 0;
                axisy.Param.NOTIndex.Value = 0;
                axisy.Param.HOMEIndex.Value = 1;
                axisy.Param.TimeOut.Value = 10000;
                axisy.Param.HomeDistLimit.Value = 796000;
                axisy.Param.IndexDistLimit.Value = 50000;

                axisy.Config.StopRate.Value = 0.05;
                axisy.Config.EStopRate.Value = 0.025;
                axisy.Config.Inposition.Value = 118;
                axisy.Config.NearTargetDistance.Value = 5590;
                axisy.Config.VelocityTolerance.Value = 559241;
                axisy.Config.SettlingTime.Value = 0;

                axisy.Config.MoterConfig.AmpEnable.Value = false;
                axisy.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisy.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisy.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisy.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisy.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisy.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisy.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisy.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;
                axisy.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;

                axisy.Config.MoterConfig.PulseAInv.Value = false;
                axisy.Config.MoterConfig.PulseBInv.Value = false;

                axisy.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisy.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisy.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisy.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisy.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisy.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisy.Config.MoterConfig.SWNegLimitTrigger.Value = -110721705.6;
                axisy.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisy.Config.MoterConfig.SWPosLimitTrigger.Value = 334401918.93333333333333333333333;
                axisy.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisy.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisy.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisy.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisy.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisy.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisy.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisy.Config.PIDCoeff.GainDerivative.Value = 6.5;
                axisy.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardAcceleration.Value = 145;
                axisy.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisy.Config.PIDCoeff.DRate.Value = 0;
                axisy.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisy.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputOffset.Value = 0;
                axisy.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisy);

                #endregion
                #region Axis Z

                ProbeAxisObject axisz = new ProbeAxisObject(0, 2, EnumAxisConstants.Z);
                axisz.Param.IndexSearchingSpeed.Value = 10000;
                axisz.Param.HomeOffset.Value = -36562;
                axisz.Param.HomeShift.Value = 0;
                axisz.AxisType.Value = EnumAxisConstants.Z;
                axisz.Label.Value = "Z";

                axisz.HomingType.Value = HomingMethodType.NHPI;
                axisz.Param.HomeInvert.Value = false;
                axisz.Param.IndexInvert.Value = false;


                axisz.Param.HommingSpeed.Value = 2000;
                axisz.Param.HommingAcceleration.Value = 100000;
                axisz.Param.HommingDecceleration.Value = 100000;
                axisz.Param.FinalVelociy.Value = 0;
                axisz.Param.Speed.Value = 20000;
                axisz.Param.Acceleration.Value = 2000000;
                axisz.Param.Decceleration.Value = 2000000;
                axisz.Param.AccelerationJerk.Value = 66;
                axisz.Param.DeccelerationJerk.Value = 66;
                axisz.Param.SeqAcc.Value = 0;
                axisz.Param.SeqDcc.Value = 0;
                axisz.Param.DtoPRatio.Value = 19.299142580828183215633093937922;
                axisz.Param.NegSWLimit.Value = -39500;
                axisz.Param.PosSWLimit.Value = -5000;
                axisz.Param.POTIndex.Value = 0;
                axisz.Param.NOTIndex.Value = 0;
                axisz.Param.HOMEIndex.Value = 1;
                axisz.Param.TimeOut.Value = 10000;
                axisz.Param.HomeDistLimit.Value = 49500;
                axisz.Param.IndexDistLimit.Value = 7000;

                axisz.Config.StopRate.Value = 0.05;
                axisz.Config.EStopRate.Value = 0.025;
                axisz.Config.Inposition.Value = 1118;
                axisz.Config.NearTargetDistance.Value = 5590;
                axisz.Config.VelocityTolerance.Value = 559241;
                axisz.Config.SettlingTime.Value = 0;

                axisz.Config.MoterConfig.AmpEnable.Value = true;
                axisz.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz.Config.MoterConfig.PulseAInv.Value = false;
                axisz.Config.MoterConfig.PulseBInv.Value = false;

                axisz.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz.Config.PIDCoeff.GainProportional.Value = 0;
                axisz.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz.Config.PIDCoeff.DRate.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz);

                #endregion
                #region Axis C

                ProbeAxisObject axist = new ProbeAxisObject(0, 3, EnumAxisConstants.C);
                axist.Param.IndexSearchingSpeed.Value = 1000;
                axist.Param.HomeOffset.Value = 15000;
                axist.Param.HomeShift.Value = 0;
                axist.AxisType.Value = EnumAxisConstants.C;
                axist.Label.Value = "T";

                axist.HomingType.Value = HomingMethodType.NH;
                axist.Param.HomeInvert.Value = false;
                axist.Param.IndexInvert.Value = true;

                axist.Param.HommingSpeed.Value = 1500;
                axist.Param.HommingAcceleration.Value = 1500;
                axist.Param.HommingDecceleration.Value = 1500;
                axist.Param.FinalVelociy.Value = 0;
                axist.Param.Speed.Value = 2000;
                axist.Param.Acceleration.Value = 2000;
                axist.Param.Decceleration.Value = 2000;
                axist.Param.AccelerationJerk.Value = 66;
                axist.Param.DeccelerationJerk.Value = 66;
                axist.Param.SeqAcc.Value = 0;
                axist.Param.SeqDcc.Value = 0;
                axist.Param.DtoPRatio.Value = 2.2727;
                axist.Param.NegSWLimit.Value = -51000;
                axist.Param.PosSWLimit.Value = 51000;
                axist.Param.POTIndex.Value = 0;
                axist.Param.NOTIndex.Value = 0;
                axist.Param.HOMEIndex.Value = 1;
                axist.Param.TimeOut.Value = 30000;
                axist.Param.HomeDistLimit.Value = 102000;
                axist.Param.IndexDistLimit.Value = 0;

                axist.Config.StopRate.Value = 0.05;
                axist.Config.EStopRate.Value = 0.025;
                axist.Config.Inposition.Value = 200;
                axist.Config.NearTargetDistance.Value = 500;
                axist.Config.VelocityTolerance.Value = 40000;
                axist.Config.SettlingTime.Value = 0;

                axist.Config.MoterConfig.AmpEnable.Value = false;
                axist.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axist.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axist.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axist.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axist.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axist.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axist.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axist.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axist.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axist.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axist.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axist.Config.MoterConfig.PulseAInv.Value = false;
                axist.Config.MoterConfig.PulseBInv.Value = false;

                axist.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axist.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axist.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axist.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axist.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axist.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axist.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axist.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axist.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axist.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axist.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axist.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axist.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axist.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axist.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axist.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axist.Config.PIDCoeff.GainProportional.Value = 450;
                axist.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axist.Config.PIDCoeff.GainDerivative.Value = 2500;
                axist.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axist.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axist.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axist.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axist.Config.PIDCoeff.DRate.Value = 0;
                axist.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axist.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axist.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axist.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axist.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axist.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axist.Config.PIDCoeff.OutputOffset.Value = -32768;
                axist.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axist);

                #endregion
                #region Axis PZ

                ProbeAxisObject axispz = new ProbeAxisObject(0, 4, EnumAxisConstants.PZ);
                axispz.Param.IndexSearchingSpeed.Value = 1000;
                axispz.Param.HomeOffset.Value = 0;
                axispz.Param.HomeShift.Value = 0;
                axispz.AxisType.Value = EnumAxisConstants.PZ;
                axispz.Label.Value = "PZ";

                axispz.HomingType.Value = HomingMethodType.NH;
                axispz.Param.HomeInvert.Value = false;
                axispz.Param.IndexInvert.Value = true;

                axispz.Param.HommingSpeed.Value = 1500;
                axispz.Param.HommingAcceleration.Value = 1500;
                axispz.Param.HommingDecceleration.Value = 1500;
                axispz.Param.FinalVelociy.Value = 0;
                axispz.Param.Speed.Value = 2000;
                axispz.Param.Acceleration.Value = 2000;
                axispz.Param.Decceleration.Value = 2000;
                axispz.Param.AccelerationJerk.Value = 66;
                axispz.Param.DeccelerationJerk.Value = 66;
                axispz.Param.SeqAcc.Value = 0;
                axispz.Param.SeqDcc.Value = 0;
                axispz.Param.DtoPRatio.Value = 2.2727;
                axispz.Param.NegSWLimit.Value = -10000;
                axispz.Param.PosSWLimit.Value = 80000;
                axispz.Param.POTIndex.Value = 0;
                axispz.Param.NOTIndex.Value = 0;
                axispz.Param.HOMEIndex.Value = 1;
                axispz.Param.TimeOut.Value = 30000;
                axispz.Param.HomeDistLimit.Value = 102000;
                axispz.Param.IndexDistLimit.Value = 0;

                axispz.Config.StopRate.Value = 0.05;
                axispz.Config.EStopRate.Value = 0.025;
                axispz.Config.Inposition.Value = 200;
                axispz.Config.NearTargetDistance.Value = 500;
                axispz.Config.VelocityTolerance.Value = 40000;
                axispz.Config.SettlingTime.Value = 0;

                axispz.Config.MoterConfig.AmpEnable.Value = false;
                axispz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axispz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axispz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axispz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axispz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axispz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axispz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axispz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axispz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axispz.Config.MoterConfig.PulseAInv.Value = false;
                axispz.Config.MoterConfig.PulseBInv.Value = false;

                axispz.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axispz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axispz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axispz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axispz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axispz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axispz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axispz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axispz.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axispz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axispz.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axispz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axispz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axispz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axispz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axispz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axispz.Config.PIDCoeff.GainProportional.Value = 450;
                axispz.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axispz.Config.PIDCoeff.GainDerivative.Value = 2500;
                axispz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axispz.Config.PIDCoeff.DRate.Value = 0;
                axispz.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axispz.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axispz.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axispz.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axispz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axispz.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axispz.Config.PIDCoeff.OutputOffset.Value = -32768;
                axispz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axispz);

                #endregion
                #endregion

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum StageAxisDefualt_OPUSV_Machine3()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region ==> Default

                #region Axis CT

                ProbeAxisObject axiscc1 = new ProbeAxisObject(0, 0, EnumAxisConstants.CT);
                axiscc1.Param.IndexSearchingSpeed.Value = 1000;
                axiscc1.Param.HomeOffset.Value = 0;
                axiscc1.Param.HomeShift.Value = 0;
                axiscc1.AxisType.Value = EnumAxisConstants.CT;
                axiscc1.Label.Value = "CT";
                axiscc1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axiscc1.Param.FeedOverride.Value = 1;

                axiscc1.HomingType.Value = HomingMethodType.PH;
                axiscc1.Param.HomeInvert.Value = false;
                axiscc1.Param.IndexInvert.Value = true;

                axiscc1.Param.HommingSpeed.Value = 15000;
                axiscc1.Param.HommingAcceleration.Value = 150000;
                axiscc1.Param.HommingDecceleration.Value = 150000;
                axiscc1.Param.FinalVelociy.Value = 0;
                axiscc1.Param.Speed.Value = 20000;
                axiscc1.Param.Acceleration.Value = 100000;
                axiscc1.Param.Decceleration.Value = 100000;
                axiscc1.Param.AccelerationJerk.Value = 1000000;
                axiscc1.Param.DeccelerationJerk.Value = 1000000;
                axiscc1.Param.SeqAcc.Value = 0;
                axiscc1.Param.SeqDcc.Value = 0;
                axiscc1.Param.DtoPRatio.Value = 0.8;
                axiscc1.Param.NegSWLimit.Value = -500000;
                axiscc1.Param.PosSWLimit.Value = 500000;
                axiscc1.Param.POTIndex.Value = 0;
                axiscc1.Param.NOTIndex.Value = 0;
                axiscc1.Param.HOMEIndex.Value = 1;
                axiscc1.Param.TimeOut.Value = 30000;
                axiscc1.Param.HomeDistLimit.Value = 102000;
                axiscc1.Param.IndexDistLimit.Value = 0;

                axiscc1.Config.StopRate.Value = 0.05;
                axiscc1.Config.EStopRate.Value = 0.025;
                axiscc1.Config.Inposition.Value = 200;
                axiscc1.Config.NearTargetDistance.Value = 500;
                axiscc1.Config.VelocityTolerance.Value = 40000;
                axiscc1.Config.SettlingTime.Value = 0;

                axiscc1.Config.MoterConfig.AmpEnable.Value = false;
                axiscc1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axiscc1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axiscc1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axiscc1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axiscc1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axiscc1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axiscc1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axiscc1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axiscc1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axiscc1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axiscc1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axiscc1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axiscc1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axiscc1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axiscc1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axiscc1.Config.MoterConfig.PulseAInv.Value = false;
                axiscc1.Config.MoterConfig.PulseBInv.Value = false;

                axiscc1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axiscc1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axiscc1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axiscc1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axiscc1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axiscc1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axiscc1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axiscc1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axiscc1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axiscc1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axiscc1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axiscc1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axiscc1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axiscc1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axiscc1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axiscc1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axiscc1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axiscc1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axiscc1.Config.PIDCoeff.GainProportional.Value = 450;
                axiscc1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axiscc1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axiscc1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axiscc1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axiscc1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axiscc1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axiscc1.Config.PIDCoeff.DRate.Value = 0;
                axiscc1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axiscc1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axiscc1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axiscc1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axiscc1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axiscc1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axiscc1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axiscc1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axiscc1);

                #endregion
                #region Axis CCM

                ProbeAxisObject axisccm = new ProbeAxisObject(0, 1, EnumAxisConstants.CCM);
                axisccm.Param.IndexSearchingSpeed.Value = 1000;
                axisccm.Param.HomeOffset.Value = 0;
                axisccm.Param.HomeShift.Value = 0;
                axisccm.AxisType.Value = EnumAxisConstants.CCM;
                axisccm.Label.Value = "CCM";
                axisccm.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisccm.MasterAxis.Value = EnumAxisConstants.CCG;
                axisccm.Param.FeedOverride.Value = 1;

                axisccm.HomingType.Value = HomingMethodType.PH;
                axisccm.Param.HomeInvert.Value = false;
                axisccm.Param.IndexInvert.Value = true;

                axisccm.Param.HommingSpeed.Value = 15000;
                axisccm.Param.HommingAcceleration.Value = 150000;
                axisccm.Param.HommingDecceleration.Value = 150000;
                axisccm.Param.FinalVelociy.Value = 0;
                axisccm.Param.Speed.Value = 20000;
                axisccm.Param.Acceleration.Value = 100000;
                axisccm.Param.Decceleration.Value = 100000;
                axisccm.Param.AccelerationJerk.Value = 1000000;
                axisccm.Param.DeccelerationJerk.Value = 1000000;
                axisccm.Param.SeqAcc.Value = 0;
                axisccm.Param.SeqDcc.Value = 0;
                axisccm.Param.DtoPRatio.Value = 0.69;
                axisccm.Param.NegSWLimit.Value = -500000;
                axisccm.Param.PosSWLimit.Value = 500000;
                axisccm.Param.POTIndex.Value = 0;
                axisccm.Param.NOTIndex.Value = 0;
                axisccm.Param.HOMEIndex.Value = 1;
                axisccm.Param.TimeOut.Value = 30000;
                axisccm.Param.HomeDistLimit.Value = 102000;
                axisccm.Param.IndexDistLimit.Value = 0;

                axisccm.Config.StopRate.Value = 0.05;
                axisccm.Config.EStopRate.Value = 0.025;
                axisccm.Config.Inposition.Value = 200;
                axisccm.Config.NearTargetDistance.Value = 500;
                axisccm.Config.VelocityTolerance.Value = 40000;
                axisccm.Config.SettlingTime.Value = 0;

                axisccm.Config.MoterConfig.AmpEnable.Value = false;
                axisccm.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisccm.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisccm.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisccm.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisccm.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisccm.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisccm.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisccm.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisccm.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisccm.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisccm.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisccm.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisccm.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisccm.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisccm.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisccm.Config.MoterConfig.PulseAInv.Value = false;
                axisccm.Config.MoterConfig.PulseBInv.Value = false;

                axisccm.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisccm.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisccm.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisccm.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisccm.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisccm.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisccm.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisccm.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisccm.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisccm.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisccm.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisccm.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisccm.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisccm.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisccm.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisccm.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisccm.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisccm.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisccm.Config.PIDCoeff.GainProportional.Value = 450;
                axisccm.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisccm.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisccm.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisccm.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisccm.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisccm.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisccm.Config.PIDCoeff.DRate.Value = 0;
                axisccm.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisccm.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisccm.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisccm.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisccm.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisccm.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisccm.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisccm.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisccm);

                #endregion
                #region Axis CCS

                ProbeAxisObject axisccs = new ProbeAxisObject(0, 2, EnumAxisConstants.CCS);
                axisccs.Param.IndexSearchingSpeed.Value = 1000;
                axisccs.Param.HomeOffset.Value = 0;
                axisccs.Param.HomeShift.Value = 0;
                axisccs.AxisType.Value = EnumAxisConstants.CCS;
                axisccs.Label.Value = "CCS";
                axisccs.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisccs.MasterAxis.Value = EnumAxisConstants.CCG;
                axisccs.Param.FeedOverride.Value = 1;


                axisccs.HomingType.Value = HomingMethodType.PH;
                axisccs.Param.HomeInvert.Value = false;
                axisccs.Param.IndexInvert.Value = true;

                axisccs.Param.HommingSpeed.Value = 15000;
                axisccs.Param.HommingAcceleration.Value = 150000;
                axisccs.Param.HommingDecceleration.Value = 150000;
                axisccs.Param.FinalVelociy.Value = 0;
                axisccs.Param.Speed.Value = 20000;
                axisccs.Param.Acceleration.Value = 100000;
                axisccs.Param.Decceleration.Value = 100000;
                axisccs.Param.AccelerationJerk.Value = 1000000;
                axisccs.Param.DeccelerationJerk.Value = 1000000;
                axisccs.Param.SeqAcc.Value = 0;
                axisccs.Param.SeqDcc.Value = 0;
                axisccs.Param.DtoPRatio.Value = 0.69;
                axisccs.Param.NegSWLimit.Value = -500000;
                axisccs.Param.PosSWLimit.Value = 500000;
                axisccs.Param.POTIndex.Value = 0;
                axisccs.Param.NOTIndex.Value = 0;
                axisccs.Param.HOMEIndex.Value = 1;
                axisccs.Param.TimeOut.Value = 30000;
                axisccs.Param.HomeDistLimit.Value = 102000;
                axisccs.Param.IndexDistLimit.Value = 0;

                axisccs.Config.StopRate.Value = 0.05;
                axisccs.Config.EStopRate.Value = 0.025;
                axisccs.Config.Inposition.Value = 200;
                axisccs.Config.NearTargetDistance.Value = 500;
                axisccs.Config.VelocityTolerance.Value = 40000;
                axisccs.Config.SettlingTime.Value = 0;

                axisccs.Config.MoterConfig.AmpEnable.Value = false;
                axisccs.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisccs.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisccs.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisccs.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisccs.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisccs.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisccs.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisccs.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisccs.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisccs.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisccs.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisccs.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisccs.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisccs.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisccs.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisccs.Config.MoterConfig.PulseAInv.Value = false;
                axisccs.Config.MoterConfig.PulseBInv.Value = false;

                axisccs.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisccs.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisccs.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisccs.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisccs.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisccs.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisccs.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisccs.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisccs.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisccs.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisccs.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisccs.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisccs.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisccs.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisccs.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisccs.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisccs.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisccs.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisccs.Config.PIDCoeff.GainProportional.Value = 450;
                axisccs.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisccs.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisccs.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisccs.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisccs.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisccs.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisccs.Config.PIDCoeff.DRate.Value = 0;
                axisccs.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisccs.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisccs.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisccs.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisccs.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisccs.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisccs.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisccs.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisccs);

                #endregion
                #region Axis ROT

                ProbeAxisObject axisROT = new ProbeAxisObject(0, 3, EnumAxisConstants.ROT);
                axisROT.Param.IndexSearchingSpeed.Value = 1000;
                axisROT.Param.HomeOffset.Value = 0;
                axisROT.Param.HomeShift.Value = 0;
                axisROT.AxisType.Value = EnumAxisConstants.ROT;
                axisROT.Label.Value = "ROT";
                axisROT.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisROT.Param.FeedOverride.Value = 1;


                axisROT.HomingType.Value = HomingMethodType.RLSEDGE;
                axisROT.Param.HomeInvert.Value = false;
                axisROT.Param.IndexInvert.Value = true;

                axisROT.Param.HommingSpeed.Value = 100000;
                axisROT.Param.HommingAcceleration.Value = 1000000;
                axisROT.Param.HommingDecceleration.Value = 1000000;
                axisROT.Param.FinalVelociy.Value = 0;
                axisROT.Param.Speed.Value = 150000;
                axisROT.Param.Acceleration.Value = 500000;
                axisROT.Param.Decceleration.Value = 500000;
                axisROT.Param.AccelerationJerk.Value = 5E+6;
                axisROT.Param.DeccelerationJerk.Value = 5E+6;
                axisROT.Param.SeqSpeed.Value = 150000;
                axisROT.Param.SeqAcc.Value = 500000;
                axisROT.Param.SeqDcc.Value = 500000;
                axisROT.Param.DtoPRatio.Value = 1;
                axisROT.Param.NegSWLimit.Value = -100000;
                axisROT.Param.PosSWLimit.Value = 800000;
                axisROT.Param.POTIndex.Value = 0;
                axisROT.Param.NOTIndex.Value = 0;
                axisROT.Param.HOMEIndex.Value = 1;
                axisROT.Param.TimeOut.Value = 30000;
                axisROT.Param.HomeDistLimit.Value = 102000;
                axisROT.Param.IndexDistLimit.Value = 0;

                axisROT.Config.StopRate.Value = 0.05;
                axisROT.Config.EStopRate.Value = 0.025;
                axisROT.Config.Inposition.Value = 200;
                axisROT.Config.NearTargetDistance.Value = 500;
                axisROT.Config.VelocityTolerance.Value = 40000;
                axisROT.Config.SettlingTime.Value = 0;

                axisROT.Config.MoterConfig.AmpEnable.Value = false;
                axisROT.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisROT.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisROT.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisROT.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisROT.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisROT.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisROT.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisROT.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisROT.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisROT.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisROT.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisROT.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisROT.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisROT.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisROT.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisROT.Config.MoterConfig.PulseAInv.Value = false;
                axisROT.Config.MoterConfig.PulseBInv.Value = false;

                axisROT.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisROT.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisROT.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisROT.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisROT.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisROT.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisROT.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisROT.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisROT.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisROT.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisROT.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisROT.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisROT.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisROT.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisROT.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisROT.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisROT.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisROT.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisROT.Config.PIDCoeff.GainProportional.Value = 450;
                axisROT.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisROT.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisROT.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisROT.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisROT.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisROT.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisROT.Config.PIDCoeff.DRate.Value = 0;
                axisROT.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisROT.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisROT.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisROT.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisROT.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisROT.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisROT.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisROT.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisROT);

                #endregion


                #region Axis X

                ProbeAxisObject axisx = new ProbeAxisObject(0, 10, EnumAxisConstants.X);
                axisx.Param.IndexSearchingSpeed.Value = 6000;
                axisx.Param.HomeOffset.Value = -138121;
                axisx.Param.HomeShift.Value = 0;
                axisx.AxisType.Value = EnumAxisConstants.X;
                axisx.Label.Value = "X";
                axisx.SettlingTime = 0;
                axisx.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisx.Param.FeedOverride.Value = 1;


                axisx.HomingType.Value = HomingMethodType.NHPI;
                axisx.Param.HomeInvert.Value = false;
                axisx.Param.IndexInvert.Value = true;


                axisx.Param.HommingSpeed.Value = 40000;   
                axisx.Param.HommingAcceleration.Value = 400000;   
                axisx.Param.HommingDecceleration.Value = 400000;  
                axisx.Param.FinalVelociy.Value = 0;
                axisx.Param.Speed.Value = 150000;    
                axisx.Param.Acceleration.Value = 2325000; 
                axisx.Param.Decceleration.Value = 2325000;
                axisx.Param.AccelerationJerk.Value = 232500000; 
                axisx.Param.DeccelerationJerk.Value = 232500000; 
                axisx.Param.SeqSpeed.Value = 220000;
                axisx.Param.SeqAcc.Value = 2400000;
                axisx.Param.SeqDcc.Value = 2400000;
                axisx.Param.DtoPRatio.Value = 51.2;
                axisx.Param.NegSWLimit.Value = -169923.3203125;   
                axisx.Param.PosSWLimit.Value = 166000;        
                axisx.Param.POTIndex.Value = 0;
                axisx.Param.NOTIndex.Value = 0;
                axisx.Param.HOMEIndex.Value = 1;
                axisx.Param.TimeOut.Value = 30000;            
                axisx.Param.HomeDistLimit.Value = 329747;
                axisx.Param.IndexDistLimit.Value = 50000;

                axisx.Config.StopRate.Value = 0.05;
                axisx.Config.EStopRate.Value = 0.025;
                axisx.Config.Inposition.Value = 118;      
                axisx.Config.NearTargetDistance.Value = 5590;
                axisx.Config.VelocityTolerance.Value = 559241;
                axisx.Config.SettlingTime.Value = 0;

                axisx.Config.MoterConfig.AmpEnable.Value = false;
                axisx.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisx.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisx.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisx.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisx.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisx.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisx.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisx.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisx.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisx.Config.MoterConfig.PulseAInv.Value = false;
                axisx.Config.MoterConfig.PulseBInv.Value = false;

                axisx.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisx.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisx.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisx.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisx.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisx.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisx.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisx.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisx.Config.MoterConfig.SWNegLimitTrigger.Value = -99540720.933333333333333333333333;
                axisx.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.SWPosLimitTrigger.Value = 105132896.26666666666666666666667;
                axisx.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisx.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisx.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisx.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisx.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisx.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisx.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisx.Config.PIDCoeff.GainDerivative.Value = 5.5;
                axisx.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardAcceleration.Value = 150;
                axisx.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisx.Config.PIDCoeff.DRate.Value = 0;
                axisx.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisx.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputOffset.Value = 0;
                axisx.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisx);

                #endregion
                #region AxisY

                ProbeAxisObject axisy = new ProbeAxisObject(0, 11, EnumAxisConstants.Y);
                axisy.Param.IndexSearchingSpeed.Value = 10000;
                axisy.Param.HomeOffset.Value = 311933;
                axisy.Param.HomeShift.Value = 0;
                axisy.AxisType.Value = EnumAxisConstants.Y;
                axisy.Label.Value = "Y";
                axisy.SettlingTime = 0;
                axisy.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisy.Param.FeedOverride.Value = 1;


                axisy.HomingType.Value = HomingMethodType.PHNI;
                axisy.Param.HomeInvert.Value = false;
                axisy.Param.IndexInvert.Value = true;


                axisy.Param.HommingSpeed.Value = 40000;
                axisy.Param.HommingAcceleration.Value = 400000;
                axisy.Param.HommingDecceleration.Value = 400000;
                axisy.Param.FinalVelociy.Value = 0;
                axisy.Param.Speed.Value = 150000;
                axisy.Param.Acceleration.Value = 2325000;
                axisy.Param.Decceleration.Value = 2325000;
                axisy.Param.AccelerationJerk.Value = 232500000;
                axisy.Param.DeccelerationJerk.Value = 232500000;
                axisy.Param.SeqSpeed.Value = 240000;
                axisy.Param.SeqAcc.Value = 2400000;
                axisy.Param.SeqDcc.Value = 2400000;
                axisy.Param.DtoPRatio.Value = 51.2;
                axisy.Param.NegSWLimit.Value = -199678.56046065259117082533589251;
                axisy.Param.PosSWLimit.Value = 353832.43761996161228406909788868;
                axisy.Param.POTIndex.Value = 0;
                axisy.Param.NOTIndex.Value = 0;
                axisy.Param.HOMEIndex.Value = 1;
                axisy.Param.TimeOut.Value = 30000;
                axisy.Param.HomeDistLimit.Value = 553510;
                axisy.Param.IndexDistLimit.Value = 50000;

                axisy.Config.StopRate.Value = 0.05;
                axisy.Config.EStopRate.Value = 0.025;
                axisy.Config.Inposition.Value = 118;
                axisy.Config.NearTargetDistance.Value = 5590;
                axisy.Config.VelocityTolerance.Value = 559241;
                axisy.Config.SettlingTime.Value = 0;

                axisy.Config.MoterConfig.AmpEnable.Value = false;
                axisy.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisy.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisy.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisy.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisy.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisy.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisy.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisy.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;
                axisy.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;

                axisy.Config.MoterConfig.PulseAInv.Value = false;
                axisy.Config.MoterConfig.PulseBInv.Value = false;

                axisy.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisy.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisy.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisy.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisy.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisy.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisy.Config.MoterConfig.SWNegLimitTrigger.Value = -110721705.6;
                axisy.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisy.Config.MoterConfig.SWPosLimitTrigger.Value = 334401918.93333333333333333333333;
                axisy.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisy.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisy.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisy.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisy.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisy.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisy.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisy.Config.PIDCoeff.GainDerivative.Value = 6.5;
                axisy.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardAcceleration.Value = 145;
                axisy.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisy.Config.PIDCoeff.DRate.Value = 0;
                axisy.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisy.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputOffset.Value = 0;
                axisy.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisy);

                #endregion
                #region Axis C

                ProbeAxisObject axist = new ProbeAxisObject(0, 12, EnumAxisConstants.C);
                axist.Param.IndexSearchingSpeed.Value = 1000;
                axist.Param.HomeOffset.Value = 50000;
                axist.Param.HomeShift.Value = 0;
                axist.AxisType.Value = EnumAxisConstants.C;
                axist.Label.Value = "T";
                axist.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axist.Param.FeedOverride.Value = 1;


                axist.HomingType.Value = HomingMethodType.PH;
                axist.Param.HomeInvert.Value = false;
                axist.Param.IndexInvert.Value = true;

                axist.Param.HommingSpeed.Value = 8000;
                axist.Param.HommingAcceleration.Value = 150000;
                axist.Param.HommingDecceleration.Value = 150000;
                axist.Param.FinalVelociy.Value = 0;
                axist.Param.Speed.Value = 15000;
                axist.Param.Acceleration.Value = 50000;
                axist.Param.Decceleration.Value = 50000;
                axist.Param.AccelerationJerk.Value = 10000000;
                axist.Param.DeccelerationJerk.Value = 10000000;
                axist.Param.SeqAcc.Value = 50000;
                axist.Param.SeqDcc.Value = 50000;
                axist.Param.SeqSpeed.Value = 15000;
                axist.Param.DtoPRatio.Value = 2.757620218;
                axist.Param.NegSWLimit.Value = -51000;
                axist.Param.PosSWLimit.Value = 51000;
                axist.Param.POTIndex.Value = 0;
                axist.Param.NOTIndex.Value = 0;
                axist.Param.HOMEIndex.Value = 1;
                axist.Param.TimeOut.Value = 30000;
                axist.Param.HomeDistLimit.Value = 102000;
                axist.Param.IndexDistLimit.Value = 0;

                axist.Config.StopRate.Value = 0.05;
                axist.Config.EStopRate.Value = 0.025;
                axist.Config.Inposition.Value = 200;
                axist.Config.NearTargetDistance.Value = 500;
                axist.Config.VelocityTolerance.Value = 40000;
                axist.Config.SettlingTime.Value = 0;

                axist.Config.MoterConfig.AmpEnable.Value = false;
                axist.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axist.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axist.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axist.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axist.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axist.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axist.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axist.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axist.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axist.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axist.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axist.Config.MoterConfig.PulseAInv.Value = false;
                axist.Config.MoterConfig.PulseBInv.Value = false;

                axist.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axist.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axist.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axist.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axist.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axist.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axist.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axist.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axist.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axist.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axist.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axist.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axist.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axist.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axist.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axist.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axist.Config.PIDCoeff.GainProportional.Value = 450;
                axist.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axist.Config.PIDCoeff.GainDerivative.Value = 2500;
                axist.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axist.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axist.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axist.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axist.Config.PIDCoeff.DRate.Value = 0;
                axist.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axist.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axist.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axist.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axist.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axist.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axist.Config.PIDCoeff.OutputOffset.Value = -32768;
                axist.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axist);

                #endregion
                #region Axis TRI

                ProbeAxisObject axistri = new ProbeAxisObject(0, 13, EnumAxisConstants.TRI);
                axistri.Param.IndexSearchingSpeed.Value = 1000;
                axistri.Param.HomeOffset.Value = 0;
                axistri.Param.HomeShift.Value = 0;
                axistri.AxisType.Value = EnumAxisConstants.TRI;
                axistri.Label.Value = "TRI";
                axistri.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axistri.Param.FeedOverride.Value = 1;


                axistri.HomingType.Value = HomingMethodType.RLSEDGE;
                axistri.Param.HomeInvert.Value = false;
                axistri.Param.IndexInvert.Value = true;

                axistri.Param.HommingSpeed.Value = 20000;
                axistri.Param.HommingAcceleration.Value = 1000000;
                axistri.Param.HommingDecceleration.Value = 1000000;
                axistri.Param.FinalVelociy.Value = 0;
                axistri.Param.Speed.Value = 150000;
                axistri.Param.Acceleration.Value = 500000;
                axistri.Param.Decceleration.Value = 500000;
                axistri.Param.AccelerationJerk.Value = 5E+6;
                axistri.Param.DeccelerationJerk.Value = 5E+6;
                axistri.Param.SeqSpeed.Value = 10000;
                axistri.Param.SeqAcc.Value = 100000;
                axistri.Param.SeqDcc.Value = 100000;
                axistri.Param.DtoPRatio.Value = 1;
                axistri.Param.NegSWLimit.Value = -100000;
                axistri.Param.PosSWLimit.Value = 800000;
                axistri.Param.POTIndex.Value = 0;
                axistri.Param.NOTIndex.Value = 0;
                axistri.Param.HOMEIndex.Value = 1;
                axistri.Param.TimeOut.Value = 30000;
                axistri.Param.HomeDistLimit.Value = 102000;
                axistri.Param.IndexDistLimit.Value = 0;

                axistri.Config.StopRate.Value = 0.05;
                axistri.Config.EStopRate.Value = 0.025;
                axistri.Config.Inposition.Value = 200;
                axistri.Config.NearTargetDistance.Value = 500;
                axistri.Config.VelocityTolerance.Value = 40000;
                axistri.Config.SettlingTime.Value = 0;

                axistri.Config.MoterConfig.AmpEnable.Value = false;
                axistri.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axistri.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axistri.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axistri.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axistri.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axistri.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axistri.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axistri.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axistri.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axistri.Config.MoterConfig.PulseAInv.Value = false;
                axistri.Config.MoterConfig.PulseBInv.Value = false;

                axistri.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axistri.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axistri.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axistri.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axistri.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axistri.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axistri.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axistri.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axistri.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axistri.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axistri.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axistri.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axistri.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axistri.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axistri.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axistri.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axistri.Config.PIDCoeff.GainProportional.Value = 450;
                axistri.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axistri.Config.PIDCoeff.GainDerivative.Value = 2500;
                axistri.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axistri.Config.PIDCoeff.DRate.Value = 0;
                axistri.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axistri.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axistri.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axistri.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axistri.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axistri.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axistri.Config.PIDCoeff.OutputOffset.Value = -32768;
                axistri.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axistri);

                #endregion

                #region Axis PZ

                ProbeAxisObject axispz = new ProbeAxisObject(0, 14, EnumAxisConstants.PZ);
                axispz.Param.IndexSearchingSpeed.Value = 1000;
                axispz.Param.HomeOffset.Value = -89704;
                axispz.Param.ClearedPosition.Value = -89704;
                axispz.Param.HomeShift.Value = 0;
                axispz.AxisType.Value = EnumAxisConstants.PZ;
                axispz.Label.Value = "PZ";
                axispz.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axispz.Param.FeedOverride.Value = 1;


                axispz.HomingType.Value = HomingMethodType.NHPI;
                axispz.Param.HomeInvert.Value = false;
                axispz.Param.IndexInvert.Value = true;

                axispz.Param.HommingSpeed.Value = 5000;
                axispz.Param.HommingAcceleration.Value = 50000;
                axispz.Param.HommingDecceleration.Value = 50000;
                axispz.Param.FinalVelociy.Value = 0;
                axispz.Param.Speed.Value = 80000;
                axispz.Param.Acceleration.Value = 1250000;
                axispz.Param.Decceleration.Value = 1250000;
                axispz.Param.AccelerationJerk.Value = 125000000;
                axispz.Param.DeccelerationJerk.Value = 125000000;
                axispz.Param.SeqAcc.Value = 0;
                axispz.Param.SeqDcc.Value = 0;
                axispz.Param.DtoPRatio.Value = 873.81333333333333333333333333333;
                axispz.Param.NegSWLimit.Value = -90000;
                axispz.Param.PosSWLimit.Value = -5000;
                axispz.Param.POTIndex.Value = 0;
                axispz.Param.NOTIndex.Value = 0;
                axispz.Param.HOMEIndex.Value = 1;
                axispz.Param.TimeOut.Value = 30000;
                axispz.Param.HomeDistLimit.Value = 102000;
                axispz.Param.IndexDistLimit.Value = 0;

                axispz.Config.StopRate.Value = 0.05;
                axispz.Config.EStopRate.Value = 0.025;
                axispz.Config.Inposition.Value = 200;
                axispz.Config.NearTargetDistance.Value = 500;
                axispz.Config.VelocityTolerance.Value = 40000;
                axispz.Config.SettlingTime.Value = 0;

                axispz.Config.MoterConfig.AmpEnable.Value = false;
                axispz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axispz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axispz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axispz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axispz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axispz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axispz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axispz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axispz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axispz.Config.MoterConfig.PulseAInv.Value = false;
                axispz.Config.MoterConfig.PulseBInv.Value = false;

                axispz.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axispz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axispz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axispz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axispz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axispz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axispz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axispz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axispz.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axispz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axispz.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axispz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axispz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axispz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axispz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axispz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axispz.Config.PIDCoeff.GainProportional.Value = 450;
                axispz.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axispz.Config.PIDCoeff.GainDerivative.Value = 2500;
                axispz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axispz.Config.PIDCoeff.DRate.Value = 0;
                axispz.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axispz.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axispz.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axispz.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axispz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axispz.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axispz.Config.PIDCoeff.OutputOffset.Value = -32768;
                axispz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axispz);

                #endregion

                #region Axis Z0

                ProbeAxisObject axisz0 = new ProbeAxisObject(0, 15, EnumAxisConstants.Z0);
                axisz0.Param.IndexSearchingSpeed.Value = 300;
                axisz0.Param.HomeOffset.Value = 0;
                axisz0.Param.HomeShift.Value = 0;
                axisz0.AxisType.Value = EnumAxisConstants.Z0;
                axisz0.Label.Value = "Z0";
                axisz0.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz0.MasterAxis.Value = EnumAxisConstants.Z;

                axisz0.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz0.Param.HomeInvert.Value = false;
                axisz0.Param.IndexInvert.Value = false;
                axisz0.Param.FeedOverride.Value = 1;


                axisz0.Param.HommingSpeed.Value = 5000;
                axisz0.Param.HommingAcceleration.Value = 150000;
                axisz0.Param.HommingDecceleration.Value = 150000;
                axisz0.Param.FinalVelociy.Value = 0;
                axisz0.Param.Speed.Value = 200000;
                axisz0.Param.Acceleration.Value = 2000000;
                axisz0.Param.Decceleration.Value = 2000000;
                axisz0.Param.AccelerationJerk.Value = 20000000;
                axisz0.Param.DeccelerationJerk.Value = 20000000;
                axisz0.Param.SeqAcc.Value = 0;
                axisz0.Param.SeqDcc.Value = 0;
                axisz0.Param.DtoPRatio.Value = 2.5;
                axisz0.Param.NegSWLimit.Value = -89000;
                axisz0.Param.PosSWLimit.Value = -5000;
                axisz0.Param.POTIndex.Value = 0;
                axisz0.Param.NOTIndex.Value = 0;
                axisz0.Param.HOMEIndex.Value = 1;
                axisz0.Param.TimeOut.Value = 30000;
                axisz0.Param.HomeDistLimit.Value = 49500;
                axisz0.Param.IndexDistLimit.Value = 7000;

                axisz0.Config.StopRate.Value = 0.05;
                axisz0.Config.EStopRate.Value = 0.025;
                axisz0.Config.Inposition.Value = 1118;
                axisz0.Config.NearTargetDistance.Value = 5590;
                axisz0.Config.VelocityTolerance.Value = 559241;
                axisz0.Config.SettlingTime.Value = 0;

                axisz0.Config.MoterConfig.AmpEnable.Value = true;
                axisz0.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz0.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz0.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz0.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz0.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz0.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz0.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz0.Config.MoterConfig.PulseAInv.Value = false;
                axisz0.Config.MoterConfig.PulseBInv.Value = false;

                axisz0.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz0.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz0.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz0.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz0.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz0.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz0.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz0.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz0.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz0.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz0.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz0.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz0.Config.PIDCoeff.GainProportional.Value = 0;
                axisz0.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz0.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz0.Config.PIDCoeff.DRate.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz0.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz0);

                #endregion
                #region Axis Z1

                ProbeAxisObject axisz1 = new ProbeAxisObject(0, 16, EnumAxisConstants.Z1);
                axisz1.Param.IndexSearchingSpeed.Value = 300;
                axisz1.Param.HomeOffset.Value = -1753;
                axisz1.Param.HomeShift.Value = 0;
                axisz1.AxisType.Value = EnumAxisConstants.Z1;
                axisz1.Label.Value = "Z1";
                axisz1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz1.MasterAxis.Value = EnumAxisConstants.Z;
                axisz1.Param.FeedOverride.Value = 1;


                axisz1.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz1.Param.HomeInvert.Value = false;
                axisz1.Param.IndexInvert.Value = false;


                axisz1.Param.HommingSpeed.Value = 5000;
                axisz1.Param.HommingAcceleration.Value = 150000;
                axisz1.Param.HommingDecceleration.Value = 150000;
                axisz1.Param.FinalVelociy.Value = 0;
                axisz1.Param.Speed.Value = 200000;
                axisz1.Param.Acceleration.Value = 2000000;
                axisz1.Param.Decceleration.Value = 2000000;
                axisz1.Param.AccelerationJerk.Value = 20000000;
                axisz1.Param.DeccelerationJerk.Value = 20000000;
                axisz1.Param.SeqAcc.Value = 0;
                axisz1.Param.SeqDcc.Value = 0;
                axisz1.Param.DtoPRatio.Value = 2.5;
                axisz1.Param.NegSWLimit.Value = -89000;
                axisz1.Param.PosSWLimit.Value = -5000;
                axisz1.Param.POTIndex.Value = 0;
                axisz1.Param.NOTIndex.Value = 0;
                axisz1.Param.HOMEIndex.Value = 1;
                axisz1.Param.TimeOut.Value = 30000;
                axisz1.Param.HomeDistLimit.Value = 49500;
                axisz1.Param.IndexDistLimit.Value = 7000;

                axisz1.Config.StopRate.Value = 0.05;
                axisz1.Config.EStopRate.Value = 0.025;
                axisz1.Config.Inposition.Value = 1118;
                axisz1.Config.NearTargetDistance.Value = 5590;
                axisz1.Config.VelocityTolerance.Value = 559241;
                axisz1.Config.SettlingTime.Value = 0;

                axisz1.Config.MoterConfig.AmpEnable.Value = true;
                axisz1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz1.Config.MoterConfig.PulseAInv.Value = false;
                axisz1.Config.MoterConfig.PulseBInv.Value = false;

                axisz1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz1.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz1.Config.PIDCoeff.GainProportional.Value = 0;
                axisz1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz1.Config.PIDCoeff.DRate.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz1);

                #endregion
                #region Axis Z2

                ProbeAxisObject axisz2 = new ProbeAxisObject(0, 17, EnumAxisConstants.Z2);
                axisz2.Param.IndexSearchingSpeed.Value = 300;
                axisz2.Param.HomeOffset.Value = 95;
                axisz2.Param.HomeShift.Value = 0;
                axisz2.AxisType.Value = EnumAxisConstants.Z2;
                axisz2.Label.Value = "Z2";
                axisz2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz2.MasterAxis.Value = EnumAxisConstants.Z;
                axisz2.Param.FeedOverride.Value = 1;


                axisz2.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz2.Param.HomeInvert.Value = false;
                axisz2.Param.IndexInvert.Value = false;


                axisz2.Param.HommingSpeed.Value = 5000;
                axisz2.Param.HommingAcceleration.Value = 150000;
                axisz2.Param.HommingDecceleration.Value = 150000;
                axisz2.Param.FinalVelociy.Value = 0;
                axisz2.Param.Speed.Value = 200000;
                axisz2.Param.Acceleration.Value = 2000000;
                axisz2.Param.Decceleration.Value = 2000000;
                axisz2.Param.AccelerationJerk.Value = 20000000;
                axisz2.Param.DeccelerationJerk.Value = 20000000;
                axisz2.Param.SeqAcc.Value = 0;
                axisz2.Param.SeqDcc.Value = 0;
                axisz2.Param.DtoPRatio.Value = 2.5;
                axisz2.Param.NegSWLimit.Value = -89000;
                axisz2.Param.PosSWLimit.Value = -5000;
                axisz2.Param.POTIndex.Value = 0;
                axisz2.Param.NOTIndex.Value = 0;
                axisz2.Param.HOMEIndex.Value = 1;
                axisz2.Param.TimeOut.Value = 30000;
                axisz2.Param.HomeDistLimit.Value = 49500;
                axisz2.Param.IndexDistLimit.Value = 7000;

                axisz2.Config.StopRate.Value = 0.05;
                axisz2.Config.EStopRate.Value = 0.025;
                axisz2.Config.Inposition.Value = 1118;
                axisz2.Config.NearTargetDistance.Value = 5590;
                axisz2.Config.VelocityTolerance.Value = 559241;
                axisz2.Config.SettlingTime.Value = 0;

                axisz2.Config.MoterConfig.AmpEnable.Value = true;
                axisz2.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz2.Config.MoterConfig.PulseAInv.Value = false;
                axisz2.Config.MoterConfig.PulseBInv.Value = false;

                axisz2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz2.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz2.Config.PIDCoeff.GainProportional.Value = 0;
                axisz2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz2.Config.PIDCoeff.DRate.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz2);

                #endregion

                #region Axis Z

                ProbeAxisObject axisz = new ProbeAxisObject(0, 18, EnumAxisConstants.Z);
                axisz.Param.IndexSearchingSpeed.Value = 300;
                axisz.Param.HomeOffset.Value = -82604;
                axisz.Param.ClearedPosition.Value = -82604;
                axisz.Param.HomeShift.Value = 4500;
                axisz.AxisType.Value = EnumAxisConstants.Z;
                axisz.Label.Value = "Z";
                axisz.AxisGroupType.Value = EnumAxisGroupType.GROUPAXIS;
                axisz.MasterAxis.Value = EnumAxisConstants.Undefined;
                axisz.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz.Param.HomeInvert.Value = false;
                axisz.Param.IndexInvert.Value = false;
                axisz.Param.FeedOverride.Value = 1;


                axisz.Param.HommingSpeed.Value = 1500;
                axisz.Param.HommingAcceleration.Value = 150000;
                axisz.Param.HommingDecceleration.Value = 150000;
                axisz.Param.FinalVelociy.Value = 0;
                axisz.Param.Speed.Value = 200000;
                axisz.Param.Acceleration.Value = 1250000;
                axisz.Param.Decceleration.Value = 1250000;
                axisz.Param.AccelerationJerk.Value = 125000000;
                axisz.Param.DeccelerationJerk.Value = 125000000;
                axisz.Param.SeqAcc.Value = 0;
                axisz.Param.SeqDcc.Value = 0;
                axisz.Param.DtoPRatio.Value = 2.5;
                axisz.Param.NegSWLimit.Value = -89000;
                axisz.Param.PosSWLimit.Value = -5000;
                axisz.Param.POTIndex.Value = 0;
                axisz.Param.NOTIndex.Value = 0;
                axisz.Param.HOMEIndex.Value = 1;
                axisz.Param.TimeOut.Value = 30000;
                axisz.Param.HomeDistLimit.Value = 4000;
                axisz.Param.IndexDistLimit.Value = 7000;

                axisz.Config.StopRate.Value = 0.05;
                axisz.Config.EStopRate.Value = 0.025;
                axisz.Config.Inposition.Value = 1118;
                axisz.Config.NearTargetDistance.Value = 5590;
                axisz.Config.VelocityTolerance.Value = 559241;
                axisz.Config.SettlingTime.Value = 0;

                axisz.Config.MoterConfig.AmpEnable.Value = true;
                axisz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz.Config.MoterConfig.PulseAInv.Value = false;
                axisz.Config.MoterConfig.PulseBInv.Value = false;

                axisz.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz.Config.PIDCoeff.GainProportional.Value = 0;
                axisz.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz.Config.PIDCoeff.DRate.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz);

                #endregion
                #region Axis CCG

                ProbeAxisObject axiscc2 = new ProbeAxisObject(0, 19, EnumAxisConstants.CCG);
                axiscc2.Param.IndexSearchingSpeed.Value = 1000;
                axiscc2.Param.HomeOffset.Value = 0;
                axiscc2.Param.HomeShift.Value = 0;
                axiscc2.AxisType.Value = EnumAxisConstants.CCG;
                axiscc2.Label.Value = "CCG";
                axiscc2.AxisGroupType.Value = EnumAxisGroupType.GROUPAXIS;
                axiscc2.Param.FeedOverride.Value = 1;

                axiscc2.HomingType.Value = HomingMethodType.PH;
                axiscc2.Param.HomeInvert.Value = false;
                axiscc2.Param.IndexInvert.Value = true;

                axiscc2.Param.HommingSpeed.Value = 15000;
                axiscc2.Param.HommingAcceleration.Value = 150000;
                axiscc2.Param.HommingDecceleration.Value = 150000;
                axiscc2.Param.FinalVelociy.Value = 0;
                axiscc2.Param.Speed.Value = 20000;
                axiscc2.Param.Acceleration.Value = 100000;
                axiscc2.Param.Decceleration.Value = 100000;
                axiscc2.Param.AccelerationJerk.Value = 1000000;
                axiscc2.Param.DeccelerationJerk.Value = 1000000;
                axiscc2.Param.SeqAcc.Value = 0;
                axiscc2.Param.SeqDcc.Value = 0;
                axiscc2.Param.DtoPRatio.Value = 0.69;
                axiscc2.Param.NegSWLimit.Value = -500000;
                axiscc2.Param.PosSWLimit.Value = 500000;
                axiscc2.Param.POTIndex.Value = 0;
                axiscc2.Param.NOTIndex.Value = 0;
                axiscc2.Param.HOMEIndex.Value = 1;
                axiscc2.Param.TimeOut.Value = 30000;
                axiscc2.Param.HomeDistLimit.Value = 102000;
                axiscc2.Param.IndexDistLimit.Value = 0;

                axiscc2.Config.StopRate.Value = 0.05;
                axiscc2.Config.EStopRate.Value = 0.025;
                axiscc2.Config.Inposition.Value = 200;
                axiscc2.Config.NearTargetDistance.Value = 500;
                axiscc2.Config.VelocityTolerance.Value = 40000;
                axiscc2.Config.SettlingTime.Value = 0;

                axiscc2.Config.MoterConfig.AmpEnable.Value = false;
                axiscc2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axiscc2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axiscc2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axiscc2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axiscc2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axiscc2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axiscc2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axiscc2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axiscc2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axiscc2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axiscc2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axiscc2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axiscc2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axiscc2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axiscc2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axiscc2.Config.MoterConfig.PulseAInv.Value = false;
                axiscc2.Config.MoterConfig.PulseBInv.Value = false;

                axiscc2.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axiscc2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axiscc2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axiscc2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axiscc2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axiscc2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axiscc2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axiscc2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axiscc2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axiscc2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axiscc2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axiscc2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axiscc2.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axiscc2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axiscc2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axiscc2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axiscc2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axiscc2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axiscc2.Config.PIDCoeff.GainProportional.Value = 450;
                axiscc2.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axiscc2.Config.PIDCoeff.GainDerivative.Value = 2500;
                axiscc2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axiscc2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axiscc2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axiscc2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axiscc2.Config.PIDCoeff.DRate.Value = 0;
                axiscc2.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axiscc2.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axiscc2.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axiscc2.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axiscc2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axiscc2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axiscc2.Config.PIDCoeff.OutputOffset.Value = -32768;
                axiscc2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axiscc2);

                #endregion

                #endregion

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        // 251017 sebas
        public EventCodeEnum StageAxisDefualt_Bonder()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                // Axis CT
                // Axis CCM
                // Axis CCS
                // Axis ROT

                #region => 기존 축
                #region Axis X

                ProbeAxisObject axisx = new ProbeAxisObject(0, 27, EnumAxisConstants.X);
                axisx.Param.IndexSearchingSpeed.Value = 6000;
                axisx.Param.HomeOffset.Value = -138121;
                axisx.Param.HomeShift.Value = 0;
                axisx.AxisType.Value = EnumAxisConstants.X;
                axisx.Label.Value = "X";
                axisx.SettlingTime = 0;
                axisx.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisx.Param.FeedOverride.Value = 1;


                axisx.HomingType.Value = HomingMethodType.NHPI;
                axisx.Param.HomeInvert.Value = false;
                axisx.Param.IndexInvert.Value = true;


                axisx.Param.HommingSpeed.Value = 40000;
                axisx.Param.HommingAcceleration.Value = 400000;
                axisx.Param.HommingDecceleration.Value = 400000;
                axisx.Param.FinalVelociy.Value = 0;
                axisx.Param.Speed.Value = 150000;
                axisx.Param.Acceleration.Value = 2325000;
                axisx.Param.Decceleration.Value = 2325000;
                axisx.Param.AccelerationJerk.Value = 232500000;
                axisx.Param.DeccelerationJerk.Value = 232500000;
                axisx.Param.SeqSpeed.Value = 220000;
                axisx.Param.SeqAcc.Value = 2400000;
                axisx.Param.SeqDcc.Value = 2400000;
                axisx.Param.DtoPRatio.Value = 51.2;
                axisx.Param.NegSWLimit.Value = -169923.3203125;
                axisx.Param.PosSWLimit.Value = 166000;
                axisx.Param.POTIndex.Value = 0;
                axisx.Param.NOTIndex.Value = 0;
                axisx.Param.HOMEIndex.Value = 1;
                axisx.Param.TimeOut.Value = 30000;
                axisx.Param.HomeDistLimit.Value = 329747;
                axisx.Param.IndexDistLimit.Value = 50000;

                axisx.Config.StopRate.Value = 0.05;
                axisx.Config.EStopRate.Value = 0.025;
                axisx.Config.Inposition.Value = 118;
                axisx.Config.NearTargetDistance.Value = 5590;
                axisx.Config.VelocityTolerance.Value = 559241;
                axisx.Config.SettlingTime.Value = 0;

                axisx.Config.MoterConfig.AmpEnable.Value = false;
                axisx.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisx.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisx.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisx.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisx.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisx.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisx.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisx.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisx.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisx.Config.MoterConfig.PulseAInv.Value = false;
                axisx.Config.MoterConfig.PulseBInv.Value = false;

                axisx.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisx.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisx.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisx.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisx.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisx.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisx.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisx.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisx.Config.MoterConfig.SWNegLimitTrigger.Value = -99540720.933333333333333333333333;
                axisx.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.SWPosLimitTrigger.Value = 105132896.26666666666666666666667;
                axisx.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisx.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisx.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisx.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisx.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisx.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisx.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisx.Config.PIDCoeff.GainDerivative.Value = 5.5;
                axisx.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardAcceleration.Value = 150;
                axisx.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisx.Config.PIDCoeff.DRate.Value = 0;
                axisx.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisx.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputOffset.Value = 0;
                axisx.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisx);

                #endregion
                #region AxisY

                ProbeAxisObject axisy = new ProbeAxisObject(0, 28, EnumAxisConstants.Y);
                axisy.Param.IndexSearchingSpeed.Value = 10000;
                axisy.Param.HomeOffset.Value = 311933;
                axisy.Param.HomeShift.Value = 0;
                axisy.AxisType.Value = EnumAxisConstants.Y;
                axisy.Label.Value = "Y";
                axisy.SettlingTime = 0;
                axisy.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisy.Param.FeedOverride.Value = 1;


                axisy.HomingType.Value = HomingMethodType.PHNI;
                axisy.Param.HomeInvert.Value = false;
                axisy.Param.IndexInvert.Value = true;


                axisy.Param.HommingSpeed.Value = 40000;
                axisy.Param.HommingAcceleration.Value = 400000;
                axisy.Param.HommingDecceleration.Value = 400000;
                axisy.Param.FinalVelociy.Value = 0;
                axisy.Param.Speed.Value = 150000;
                axisy.Param.Acceleration.Value = 2325000;
                axisy.Param.Decceleration.Value = 2325000;
                axisy.Param.AccelerationJerk.Value = 232500000;
                axisy.Param.DeccelerationJerk.Value = 232500000;
                axisy.Param.SeqSpeed.Value = 240000;
                axisy.Param.SeqAcc.Value = 2400000;
                axisy.Param.SeqDcc.Value = 2400000;
                axisy.Param.DtoPRatio.Value = 51.2;
                axisy.Param.NegSWLimit.Value = -199678.56046065259117082533589251;
                axisy.Param.PosSWLimit.Value = 353832.43761996161228406909788868;
                axisy.Param.POTIndex.Value = 0;
                axisy.Param.NOTIndex.Value = 0;
                axisy.Param.HOMEIndex.Value = 1;
                axisy.Param.TimeOut.Value = 30000;
                axisy.Param.HomeDistLimit.Value = 553510;
                axisy.Param.IndexDistLimit.Value = 50000;

                axisy.Config.StopRate.Value = 0.05;
                axisy.Config.EStopRate.Value = 0.025;
                axisy.Config.Inposition.Value = 118;
                axisy.Config.NearTargetDistance.Value = 5590;
                axisy.Config.VelocityTolerance.Value = 559241;
                axisy.Config.SettlingTime.Value = 0;

                axisy.Config.MoterConfig.AmpEnable.Value = false;
                axisy.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisy.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisy.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisy.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisy.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisy.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisy.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisy.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;
                axisy.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;

                axisy.Config.MoterConfig.PulseAInv.Value = false;
                axisy.Config.MoterConfig.PulseBInv.Value = false;

                axisy.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisy.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisy.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisy.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisy.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisy.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisy.Config.MoterConfig.SWNegLimitTrigger.Value = -110721705.6;
                axisy.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisy.Config.MoterConfig.SWPosLimitTrigger.Value = 334401918.93333333333333333333333;
                axisy.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisy.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisy.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisy.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisy.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisy.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisy.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisy.Config.PIDCoeff.GainDerivative.Value = 6.5;
                axisy.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardAcceleration.Value = 145;
                axisy.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisy.Config.PIDCoeff.DRate.Value = 0;
                axisy.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisy.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputOffset.Value = 0;
                axisy.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisy);

                #endregion
                #region Axis C

                ProbeAxisObject axist = new ProbeAxisObject(0, 29, EnumAxisConstants.C);
                axist.Param.IndexSearchingSpeed.Value = 1000;
                axist.Param.HomeOffset.Value = 50000;
                axist.Param.HomeShift.Value = 0;
                axist.AxisType.Value = EnumAxisConstants.C;
                axist.Label.Value = "T";
                axist.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axist.Param.FeedOverride.Value = 1;


                axist.HomingType.Value = HomingMethodType.PH;
                axist.Param.HomeInvert.Value = false;
                axist.Param.IndexInvert.Value = true;

                axist.Param.HommingSpeed.Value = 8000;
                axist.Param.HommingAcceleration.Value = 150000;
                axist.Param.HommingDecceleration.Value = 150000;
                axist.Param.FinalVelociy.Value = 0;
                axist.Param.Speed.Value = 15000;
                axist.Param.Acceleration.Value = 50000;
                axist.Param.Decceleration.Value = 50000;
                axist.Param.AccelerationJerk.Value = 10000000;
                axist.Param.DeccelerationJerk.Value = 10000000;
                axist.Param.SeqAcc.Value = 50000;
                axist.Param.SeqDcc.Value = 50000;
                axist.Param.SeqSpeed.Value = 15000;
                axist.Param.DtoPRatio.Value = 2.757620218;
                axist.Param.NegSWLimit.Value = -51000;
                axist.Param.PosSWLimit.Value = 51000;
                axist.Param.POTIndex.Value = 0;
                axist.Param.NOTIndex.Value = 0;
                axist.Param.HOMEIndex.Value = 1;
                axist.Param.TimeOut.Value = 30000;
                axist.Param.HomeDistLimit.Value = 102000;
                axist.Param.IndexDistLimit.Value = 0;

                axist.Config.StopRate.Value = 0.05;
                axist.Config.EStopRate.Value = 0.025;
                axist.Config.Inposition.Value = 200;
                axist.Config.NearTargetDistance.Value = 500;
                axist.Config.VelocityTolerance.Value = 40000;
                axist.Config.SettlingTime.Value = 0;

                axist.Config.MoterConfig.AmpEnable.Value = false;
                axist.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axist.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axist.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axist.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axist.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axist.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axist.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axist.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axist.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axist.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axist.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axist.Config.MoterConfig.PulseAInv.Value = false;
                axist.Config.MoterConfig.PulseBInv.Value = false;

                axist.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axist.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axist.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axist.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axist.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axist.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axist.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axist.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axist.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axist.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axist.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axist.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axist.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axist.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axist.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axist.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axist.Config.PIDCoeff.GainProportional.Value = 450;
                axist.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axist.Config.PIDCoeff.GainDerivative.Value = 2500;
                axist.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axist.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axist.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axist.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axist.Config.PIDCoeff.DRate.Value = 0;
                axist.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axist.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axist.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axist.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axist.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axist.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axist.Config.PIDCoeff.OutputOffset.Value = -32768;
                axist.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axist);

                #endregion
                #region Axis TRI

                ProbeAxisObject axistri = new ProbeAxisObject(0, 30, EnumAxisConstants.TRI);
                axistri.Param.IndexSearchingSpeed.Value = 1000;
                axistri.Param.HomeOffset.Value = 0;
                axistri.Param.HomeShift.Value = 0;
                axistri.AxisType.Value = EnumAxisConstants.TRI;
                axistri.Label.Value = "TRI";
                axistri.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axistri.Param.FeedOverride.Value = 1;


                axistri.HomingType.Value = HomingMethodType.RLSEDGE;
                axistri.Param.HomeInvert.Value = false;
                axistri.Param.IndexInvert.Value = true;

                axistri.Param.HommingSpeed.Value = 20000;
                axistri.Param.HommingAcceleration.Value = 1000000;
                axistri.Param.HommingDecceleration.Value = 1000000;
                axistri.Param.FinalVelociy.Value = 0;
                axistri.Param.Speed.Value = 150000;
                axistri.Param.Acceleration.Value = 500000;
                axistri.Param.Decceleration.Value = 500000;
                axistri.Param.AccelerationJerk.Value = 5E+6;
                axistri.Param.DeccelerationJerk.Value = 5E+6;
                axistri.Param.SeqSpeed.Value = 10000;
                axistri.Param.SeqAcc.Value = 100000;
                axistri.Param.SeqDcc.Value = 100000;
                axistri.Param.DtoPRatio.Value = 1;
                axistri.Param.NegSWLimit.Value = -100000;
                axistri.Param.PosSWLimit.Value = 800000;
                axistri.Param.POTIndex.Value = 0;
                axistri.Param.NOTIndex.Value = 0;
                axistri.Param.HOMEIndex.Value = 1;
                axistri.Param.TimeOut.Value = 30000;
                axistri.Param.HomeDistLimit.Value = 102000;
                axistri.Param.IndexDistLimit.Value = 0;

                axistri.Config.StopRate.Value = 0.05;
                axistri.Config.EStopRate.Value = 0.025;
                axistri.Config.Inposition.Value = 200;
                axistri.Config.NearTargetDistance.Value = 500;
                axistri.Config.VelocityTolerance.Value = 40000;
                axistri.Config.SettlingTime.Value = 0;

                axistri.Config.MoterConfig.AmpEnable.Value = false;
                axistri.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axistri.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axistri.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axistri.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axistri.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axistri.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axistri.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axistri.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axistri.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axistri.Config.MoterConfig.PulseAInv.Value = false;
                axistri.Config.MoterConfig.PulseBInv.Value = false;

                axistri.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axistri.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axistri.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axistri.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axistri.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axistri.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axistri.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axistri.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axistri.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axistri.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axistri.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axistri.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axistri.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axistri.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axistri.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axistri.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axistri.Config.PIDCoeff.GainProportional.Value = 450;
                axistri.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axistri.Config.PIDCoeff.GainDerivative.Value = 2500;
                axistri.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axistri.Config.PIDCoeff.DRate.Value = 0;
                axistri.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axistri.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axistri.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axistri.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axistri.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axistri.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axistri.Config.PIDCoeff.OutputOffset.Value = -32768;
                axistri.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axistri);

                #endregion

                #region Axis PZ

                ProbeAxisObject axispz = new ProbeAxisObject(0, 31, EnumAxisConstants.PZ);
                axispz.Param.IndexSearchingSpeed.Value = 1000;
                axispz.Param.HomeOffset.Value = -89704;
                axispz.Param.ClearedPosition.Value = -89704;
                axispz.Param.HomeShift.Value = 0;
                axispz.AxisType.Value = EnumAxisConstants.PZ;
                axispz.Label.Value = "PZ";
                axispz.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axispz.Param.FeedOverride.Value = 1;


                axispz.HomingType.Value = HomingMethodType.NHPI;
                axispz.Param.HomeInvert.Value = false;
                axispz.Param.IndexInvert.Value = true;

                axispz.Param.HommingSpeed.Value = 5000;
                axispz.Param.HommingAcceleration.Value = 50000;
                axispz.Param.HommingDecceleration.Value = 50000;
                axispz.Param.FinalVelociy.Value = 0;
                axispz.Param.Speed.Value = 80000;
                axispz.Param.Acceleration.Value = 1250000;
                axispz.Param.Decceleration.Value = 1250000;
                axispz.Param.AccelerationJerk.Value = 125000000;
                axispz.Param.DeccelerationJerk.Value = 125000000;
                axispz.Param.SeqAcc.Value = 0;
                axispz.Param.SeqDcc.Value = 0;
                axispz.Param.DtoPRatio.Value = 873.81333333333333333333333333333;
                axispz.Param.NegSWLimit.Value = -90000;
                axispz.Param.PosSWLimit.Value = -5000;
                axispz.Param.POTIndex.Value = 0;
                axispz.Param.NOTIndex.Value = 0;
                axispz.Param.HOMEIndex.Value = 1;
                axispz.Param.TimeOut.Value = 30000;
                axispz.Param.HomeDistLimit.Value = 102000;
                axispz.Param.IndexDistLimit.Value = 0;

                axispz.Config.StopRate.Value = 0.05;
                axispz.Config.EStopRate.Value = 0.025;
                axispz.Config.Inposition.Value = 200;
                axispz.Config.NearTargetDistance.Value = 500;
                axispz.Config.VelocityTolerance.Value = 40000;
                axispz.Config.SettlingTime.Value = 0;

                axispz.Config.MoterConfig.AmpEnable.Value = false;
                axispz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axispz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axispz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axispz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axispz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axispz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axispz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axispz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axispz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axispz.Config.MoterConfig.PulseAInv.Value = false;
                axispz.Config.MoterConfig.PulseBInv.Value = false;

                axispz.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axispz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axispz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axispz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axispz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axispz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axispz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axispz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axispz.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axispz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axispz.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axispz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axispz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axispz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axispz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axispz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axispz.Config.PIDCoeff.GainProportional.Value = 450;
                axispz.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axispz.Config.PIDCoeff.GainDerivative.Value = 2500;
                axispz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axispz.Config.PIDCoeff.DRate.Value = 0;
                axispz.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axispz.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axispz.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axispz.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axispz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axispz.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axispz.Config.PIDCoeff.OutputOffset.Value = -32768;
                axispz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axispz);

                #endregion

                #region Axis Z0

                ProbeAxisObject axisz0 = new ProbeAxisObject(0, 32, EnumAxisConstants.Z0);
                axisz0.Param.IndexSearchingSpeed.Value = 300;
                axisz0.Param.HomeOffset.Value = 0;
                axisz0.Param.HomeShift.Value = 0;
                axisz0.AxisType.Value = EnumAxisConstants.Z0;
                axisz0.Label.Value = "Z0";
                axisz0.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz0.MasterAxis.Value = EnumAxisConstants.Z;

                axisz0.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz0.Param.HomeInvert.Value = false;
                axisz0.Param.IndexInvert.Value = false;
                axisz0.Param.FeedOverride.Value = 1;


                axisz0.Param.HommingSpeed.Value = 5000;
                axisz0.Param.HommingAcceleration.Value = 150000;
                axisz0.Param.HommingDecceleration.Value = 150000;
                axisz0.Param.FinalVelociy.Value = 0;
                axisz0.Param.Speed.Value = 200000;
                axisz0.Param.Acceleration.Value = 2000000;
                axisz0.Param.Decceleration.Value = 2000000;
                axisz0.Param.AccelerationJerk.Value = 20000000;
                axisz0.Param.DeccelerationJerk.Value = 20000000;
                axisz0.Param.SeqAcc.Value = 0;
                axisz0.Param.SeqDcc.Value = 0;
                axisz0.Param.DtoPRatio.Value = 2.5;
                axisz0.Param.NegSWLimit.Value = -89000;
                axisz0.Param.PosSWLimit.Value = -5000;
                axisz0.Param.POTIndex.Value = 0;
                axisz0.Param.NOTIndex.Value = 0;
                axisz0.Param.HOMEIndex.Value = 1;
                axisz0.Param.TimeOut.Value = 30000;
                axisz0.Param.HomeDistLimit.Value = 49500;
                axisz0.Param.IndexDistLimit.Value = 7000;

                axisz0.Config.StopRate.Value = 0.05;
                axisz0.Config.EStopRate.Value = 0.025;
                axisz0.Config.Inposition.Value = 1118;
                axisz0.Config.NearTargetDistance.Value = 5590;
                axisz0.Config.VelocityTolerance.Value = 559241;
                axisz0.Config.SettlingTime.Value = 0;

                axisz0.Config.MoterConfig.AmpEnable.Value = true;
                axisz0.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz0.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz0.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz0.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz0.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz0.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz0.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz0.Config.MoterConfig.PulseAInv.Value = false;
                axisz0.Config.MoterConfig.PulseBInv.Value = false;

                axisz0.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz0.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz0.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz0.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz0.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz0.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz0.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz0.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz0.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz0.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz0.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz0.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz0.Config.PIDCoeff.GainProportional.Value = 0;
                axisz0.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz0.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz0.Config.PIDCoeff.DRate.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz0.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz0);

                #endregion
                #region Axis Z1

                ProbeAxisObject axisz1 = new ProbeAxisObject(0, 33, EnumAxisConstants.Z1);
                axisz1.Param.IndexSearchingSpeed.Value = 300;
                axisz1.Param.HomeOffset.Value = -1753;
                axisz1.Param.HomeShift.Value = 0;
                axisz1.AxisType.Value = EnumAxisConstants.Z1;
                axisz1.Label.Value = "Z1";
                axisz1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz1.MasterAxis.Value = EnumAxisConstants.Z;
                axisz1.Param.FeedOverride.Value = 1;


                axisz1.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz1.Param.HomeInvert.Value = false;
                axisz1.Param.IndexInvert.Value = false;


                axisz1.Param.HommingSpeed.Value = 5000;
                axisz1.Param.HommingAcceleration.Value = 150000;
                axisz1.Param.HommingDecceleration.Value = 150000;
                axisz1.Param.FinalVelociy.Value = 0;
                axisz1.Param.Speed.Value = 200000;
                axisz1.Param.Acceleration.Value = 2000000;
                axisz1.Param.Decceleration.Value = 2000000;
                axisz1.Param.AccelerationJerk.Value = 20000000;
                axisz1.Param.DeccelerationJerk.Value = 20000000;
                axisz1.Param.SeqAcc.Value = 0;
                axisz1.Param.SeqDcc.Value = 0;
                axisz1.Param.DtoPRatio.Value = 2.5;
                axisz1.Param.NegSWLimit.Value = -89000;
                axisz1.Param.PosSWLimit.Value = -5000;
                axisz1.Param.POTIndex.Value = 0;
                axisz1.Param.NOTIndex.Value = 0;
                axisz1.Param.HOMEIndex.Value = 1;
                axisz1.Param.TimeOut.Value = 30000;
                axisz1.Param.HomeDistLimit.Value = 49500;
                axisz1.Param.IndexDistLimit.Value = 7000;

                axisz1.Config.StopRate.Value = 0.05;
                axisz1.Config.EStopRate.Value = 0.025;
                axisz1.Config.Inposition.Value = 1118;
                axisz1.Config.NearTargetDistance.Value = 5590;
                axisz1.Config.VelocityTolerance.Value = 559241;
                axisz1.Config.SettlingTime.Value = 0;

                axisz1.Config.MoterConfig.AmpEnable.Value = true;
                axisz1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz1.Config.MoterConfig.PulseAInv.Value = false;
                axisz1.Config.MoterConfig.PulseBInv.Value = false;

                axisz1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz1.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz1.Config.PIDCoeff.GainProportional.Value = 0;
                axisz1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz1.Config.PIDCoeff.DRate.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz1);

                #endregion
                #region Axis Z2

                ProbeAxisObject axisz2 = new ProbeAxisObject(0, 34, EnumAxisConstants.Z2);
                axisz2.Param.IndexSearchingSpeed.Value = 300;
                axisz2.Param.HomeOffset.Value = 95;
                axisz2.Param.HomeShift.Value = 0;
                axisz2.AxisType.Value = EnumAxisConstants.Z2;
                axisz2.Label.Value = "Z2";
                axisz2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz2.MasterAxis.Value = EnumAxisConstants.Z;
                axisz2.Param.FeedOverride.Value = 1;


                axisz2.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz2.Param.HomeInvert.Value = false;
                axisz2.Param.IndexInvert.Value = false;


                axisz2.Param.HommingSpeed.Value = 5000;
                axisz2.Param.HommingAcceleration.Value = 150000;
                axisz2.Param.HommingDecceleration.Value = 150000;
                axisz2.Param.FinalVelociy.Value = 0;
                axisz2.Param.Speed.Value = 200000;
                axisz2.Param.Acceleration.Value = 2000000;
                axisz2.Param.Decceleration.Value = 2000000;
                axisz2.Param.AccelerationJerk.Value = 20000000;
                axisz2.Param.DeccelerationJerk.Value = 20000000;
                axisz2.Param.SeqAcc.Value = 0;
                axisz2.Param.SeqDcc.Value = 0;
                axisz2.Param.DtoPRatio.Value = 2.5;
                axisz2.Param.NegSWLimit.Value = -89000;
                axisz2.Param.PosSWLimit.Value = -5000;
                axisz2.Param.POTIndex.Value = 0;
                axisz2.Param.NOTIndex.Value = 0;
                axisz2.Param.HOMEIndex.Value = 1;
                axisz2.Param.TimeOut.Value = 30000;
                axisz2.Param.HomeDistLimit.Value = 49500;
                axisz2.Param.IndexDistLimit.Value = 7000;

                axisz2.Config.StopRate.Value = 0.05;
                axisz2.Config.EStopRate.Value = 0.025;
                axisz2.Config.Inposition.Value = 1118;
                axisz2.Config.NearTargetDistance.Value = 5590;
                axisz2.Config.VelocityTolerance.Value = 559241;
                axisz2.Config.SettlingTime.Value = 0;

                axisz2.Config.MoterConfig.AmpEnable.Value = true;
                axisz2.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz2.Config.MoterConfig.PulseAInv.Value = false;
                axisz2.Config.MoterConfig.PulseBInv.Value = false;

                axisz2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz2.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz2.Config.PIDCoeff.GainProportional.Value = 0;
                axisz2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz2.Config.PIDCoeff.DRate.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz2);

                #endregion

                #region Axis Z

                ProbeAxisObject axisz = new ProbeAxisObject(0, 35, EnumAxisConstants.Z);
                axisz.Param.IndexSearchingSpeed.Value = 300;
                axisz.Param.HomeOffset.Value = -82604;
                axisz.Param.ClearedPosition.Value = -82604;
                axisz.Param.HomeShift.Value = 4500;
                axisz.AxisType.Value = EnumAxisConstants.Z;
                axisz.Label.Value = "Z";
                axisz.AxisGroupType.Value = EnumAxisGroupType.GROUPAXIS;
                axisz.MasterAxis.Value = EnumAxisConstants.Undefined;
                axisz.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz.Param.HomeInvert.Value = false;
                axisz.Param.IndexInvert.Value = false;
                axisz.Param.FeedOverride.Value = 1;


                axisz.Param.HommingSpeed.Value = 1500;
                axisz.Param.HommingAcceleration.Value = 150000;
                axisz.Param.HommingDecceleration.Value = 150000;
                axisz.Param.FinalVelociy.Value = 0;
                axisz.Param.Speed.Value = 200000;
                axisz.Param.Acceleration.Value = 1250000;
                axisz.Param.Decceleration.Value = 1250000;
                axisz.Param.AccelerationJerk.Value = 125000000;
                axisz.Param.DeccelerationJerk.Value = 125000000;
                axisz.Param.SeqAcc.Value = 0;
                axisz.Param.SeqDcc.Value = 0;
                axisz.Param.DtoPRatio.Value = 2.5;
                axisz.Param.NegSWLimit.Value = -89000;
                axisz.Param.PosSWLimit.Value = -5000;
                axisz.Param.POTIndex.Value = 0;
                axisz.Param.NOTIndex.Value = 0;
                axisz.Param.HOMEIndex.Value = 1;
                axisz.Param.TimeOut.Value = 30000;
                axisz.Param.HomeDistLimit.Value = 4000;
                axisz.Param.IndexDistLimit.Value = 7000;

                axisz.Config.StopRate.Value = 0.05;
                axisz.Config.EStopRate.Value = 0.025;
                axisz.Config.Inposition.Value = 1118;
                axisz.Config.NearTargetDistance.Value = 5590;
                axisz.Config.VelocityTolerance.Value = 559241;
                axisz.Config.SettlingTime.Value = 0;

                axisz.Config.MoterConfig.AmpEnable.Value = true;
                axisz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz.Config.MoterConfig.PulseAInv.Value = false;
                axisz.Config.MoterConfig.PulseBInv.Value = false;

                axisz.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz.Config.PIDCoeff.GainProportional.Value = 0;
                axisz.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz.Config.PIDCoeff.DRate.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz);

                #endregion
                // Axis CCG
                #endregion

                #region => 신규 축
                #region Axis CX1
                ProbeAxisObject axisCX1 = new ProbeAxisObject(0, 4, EnumAxisConstants.CX1);
                axisCX1.Param.IndexSearchingSpeed.Value = 1000;
                axisCX1.Param.HomeOffset.Value = 0;
                axisCX1.Param.HomeShift.Value = 0;
                axisCX1.AxisType.Value = EnumAxisConstants.CX1;
                axisCX1.Label.Value = "CX1";
                axisCX1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCX1.Param.FeedOverride.Value = 1;


                axisCX1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCX1.Param.HomeInvert.Value = false;
                axisCX1.Param.IndexInvert.Value = true;

                axisCX1.Param.HommingSpeed.Value = 2000;
                axisCX1.Param.HommingAcceleration.Value = 20000;
                axisCX1.Param.HommingDecceleration.Value = 20000;
                axisCX1.Param.FinalVelociy.Value = 0;
                axisCX1.Param.Speed.Value = 2000;
                axisCX1.Param.Acceleration.Value = 20000;
                axisCX1.Param.Decceleration.Value = 20000;
                axisCX1.Param.AccelerationJerk.Value = 200000;
                axisCX1.Param.DeccelerationJerk.Value = 200000;
                axisCX1.Param.SeqSpeed.Value = 2000;
                axisCX1.Param.SeqAcc.Value = 20000;
                axisCX1.Param.SeqDcc.Value = 20000;
                axisCX1.Param.DtoPRatio.Value = 1;
                axisCX1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCX1.Param.PosSWLimit.Value = 1000000;
                axisCX1.Param.POTIndex.Value = 0;
                axisCX1.Param.NOTIndex.Value = 0;
                axisCX1.Param.HOMEIndex.Value = 1;
                axisCX1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCX1.Param.HomeDistLimit.Value = 102000;
                axisCX1.Param.IndexDistLimit.Value = 0;

                axisCX1.Config.StopRate.Value = 0.05;
                axisCX1.Config.EStopRate.Value = 0.025;
                axisCX1.Config.Inposition.Value = 200;
                axisCX1.Config.NearTargetDistance.Value = 500;
                axisCX1.Config.VelocityTolerance.Value = 40000;
                axisCX1.Config.SettlingTime.Value = 0;

                axisCX1.Config.MoterConfig.AmpEnable.Value = false;
                axisCX1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCX1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCX1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCX1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCX1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCX1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCX1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCX1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCX1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCX1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCX1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCX1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCX1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCX1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCX1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCX1.Config.MoterConfig.PulseAInv.Value = false;
                axisCX1.Config.MoterConfig.PulseBInv.Value = false;

                axisCX1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCX1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCX1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCX1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCX1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCX1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCX1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCX1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCX1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCX1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCX1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCX1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCX1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCX1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCX1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCX1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCX1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCX1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCX1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCX1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCX1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCX1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCX1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCX1.Config.PIDCoeff.GainProportional.Value = 450;
                axisCX1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCX1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCX1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCX1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCX1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCX1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCX1.Config.PIDCoeff.DRate.Value = 0;
                axisCX1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCX1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCX1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCX1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCX1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCX1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCX1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCX1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCX1);
                #endregion
                #region Axis CY1
                ProbeAxisObject axisCY1 = new ProbeAxisObject(0, 5, EnumAxisConstants.CY1);
                axisCY1.Param.IndexSearchingSpeed.Value = 1000;
                axisCY1.Param.HomeOffset.Value = 0;
                axisCY1.Param.HomeShift.Value = 0;
                axisCY1.AxisType.Value = EnumAxisConstants.CY1;
                axisCY1.Label.Value = "CY1";
                axisCY1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCY1.Param.FeedOverride.Value = 1;


                axisCY1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCY1.Param.HomeInvert.Value = false;
                axisCY1.Param.IndexInvert.Value = true;

                axisCY1.Param.HommingSpeed.Value = 2000;
                axisCY1.Param.HommingAcceleration.Value = 20000;
                axisCY1.Param.HommingDecceleration.Value = 20000;
                axisCY1.Param.FinalVelociy.Value = 0;
                axisCY1.Param.Speed.Value = 2000;
                axisCY1.Param.Acceleration.Value = 20000;
                axisCY1.Param.Decceleration.Value = 20000;
                axisCY1.Param.AccelerationJerk.Value = 200000;
                axisCY1.Param.DeccelerationJerk.Value = 200000;
                axisCY1.Param.SeqSpeed.Value = 2000;
                axisCY1.Param.SeqAcc.Value = 20000;
                axisCY1.Param.SeqDcc.Value = 20000;
                axisCY1.Param.DtoPRatio.Value = 1;
                axisCY1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCY1.Param.PosSWLimit.Value = 1000000;
                axisCY1.Param.POTIndex.Value = 0;
                axisCY1.Param.NOTIndex.Value = 0;
                axisCY1.Param.HOMEIndex.Value = 1;
                axisCY1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCY1.Param.HomeDistLimit.Value = 102000;
                axisCY1.Param.IndexDistLimit.Value = 0;

                axisCY1.Config.StopRate.Value = 0.05;
                axisCY1.Config.EStopRate.Value = 0.025;
                axisCY1.Config.Inposition.Value = 200;
                axisCY1.Config.NearTargetDistance.Value = 500;
                axisCY1.Config.VelocityTolerance.Value = 40000;
                axisCY1.Config.SettlingTime.Value = 0;

                axisCY1.Config.MoterConfig.AmpEnable.Value = false;
                axisCY1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCY1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCY1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCY1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCY1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCY1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCY1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCY1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCY1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCY1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCY1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCY1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCY1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCY1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCY1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCY1.Config.MoterConfig.PulseAInv.Value = false;
                axisCY1.Config.MoterConfig.PulseBInv.Value = false;

                axisCY1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCY1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCY1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCY1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCY1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCY1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCY1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCY1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCY1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCY1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCY1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCY1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCY1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCY1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCY1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCY1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCY1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCY1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCY1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCY1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCY1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCY1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCY1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCY1.Config.PIDCoeff.GainProportional.Value = 450;
                axisCY1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCY1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCY1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCY1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCY1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCY1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCY1.Config.PIDCoeff.DRate.Value = 0;
                axisCY1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCY1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCY1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCY1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCY1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCY1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCY1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCY1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCY1);
                #endregion
                #region Axis CZ1
                ProbeAxisObject axisCZ1 = new ProbeAxisObject(0, 6, EnumAxisConstants.CZ1);
                axisCZ1.Param.IndexSearchingSpeed.Value = 1000;
                axisCZ1.Param.HomeOffset.Value = 0;
                axisCZ1.Param.HomeShift.Value = 0;
                axisCZ1.AxisType.Value = EnumAxisConstants.CZ1;
                axisCZ1.Label.Value = "CZ1";
                axisCZ1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCZ1.Param.FeedOverride.Value = 1;


                axisCZ1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCZ1.Param.HomeInvert.Value = false;
                axisCZ1.Param.IndexInvert.Value = true;

                axisCZ1.Param.HommingSpeed.Value = 2000;
                axisCZ1.Param.HommingAcceleration.Value = 20000;
                axisCZ1.Param.HommingDecceleration.Value = 20000;
                axisCZ1.Param.FinalVelociy.Value = 0;
                axisCZ1.Param.Speed.Value = 2000;
                axisCZ1.Param.Acceleration.Value = 20000;
                axisCZ1.Param.Decceleration.Value = 20000;
                axisCZ1.Param.AccelerationJerk.Value = 200000;
                axisCZ1.Param.DeccelerationJerk.Value = 200000;
                axisCZ1.Param.SeqSpeed.Value = 2000;
                axisCZ1.Param.SeqAcc.Value = 20000;
                axisCZ1.Param.SeqDcc.Value = 20000;
                axisCZ1.Param.DtoPRatio.Value = 1;
                axisCZ1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCZ1.Param.PosSWLimit.Value = 1000000;
                axisCZ1.Param.POTIndex.Value = 0;
                axisCZ1.Param.NOTIndex.Value = 0;
                axisCZ1.Param.HOMEIndex.Value = 1;
                axisCZ1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCZ1.Param.HomeDistLimit.Value = 102000;
                axisCZ1.Param.IndexDistLimit.Value = 0;

                axisCZ1.Config.StopRate.Value = 0.05;
                axisCZ1.Config.EStopRate.Value = 0.025;
                axisCZ1.Config.Inposition.Value = 200;
                axisCZ1.Config.NearTargetDistance.Value = 500;
                axisCZ1.Config.VelocityTolerance.Value = 40000;
                axisCZ1.Config.SettlingTime.Value = 0;

                axisCZ1.Config.MoterConfig.AmpEnable.Value = false;
                axisCZ1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCZ1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCZ1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCZ1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCZ1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCZ1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCZ1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCZ1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCZ1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCZ1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCZ1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCZ1.Config.MoterConfig.PulseAInv.Value = false;
                axisCZ1.Config.MoterConfig.PulseBInv.Value = false;

                axisCZ1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCZ1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCZ1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCZ1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCZ1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCZ1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCZ1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCZ1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCZ1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCZ1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCZ1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCZ1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCZ1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCZ1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCZ1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCZ1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCZ1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCZ1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCZ1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCZ1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCZ1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCZ1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCZ1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCZ1.Config.PIDCoeff.GainProportional.Value = 450;
                axisCZ1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCZ1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCZ1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCZ1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCZ1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCZ1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCZ1.Config.PIDCoeff.DRate.Value = 0;
                axisCZ1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCZ1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCZ1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCZ1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCZ1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCZ1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCZ1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCZ1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCZ1);
                #endregion

                #region Axis CX2
                ProbeAxisObject axisCX2 = new ProbeAxisObject(0, 7, EnumAxisConstants.CX2);
                axisCX2.Param.IndexSearchingSpeed.Value = 1000;
                axisCX2.Param.HomeOffset.Value = 0;
                axisCX2.Param.HomeShift.Value = 0;
                axisCX2.AxisType.Value = EnumAxisConstants.CX2;
                axisCX2.Label.Value = "CX2";
                axisCX2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCX2.Param.FeedOverride.Value = 1;


                axisCX2.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCX2.Param.HomeInvert.Value = false;
                axisCX2.Param.IndexInvert.Value = true;

                axisCX2.Param.HommingSpeed.Value = 2000;
                axisCX2.Param.HommingAcceleration.Value = 20000;
                axisCX2.Param.HommingDecceleration.Value = 20000;
                axisCX2.Param.FinalVelociy.Value = 0;
                axisCX2.Param.Speed.Value = 2000;
                axisCX2.Param.Acceleration.Value = 20000;
                axisCX2.Param.Decceleration.Value = 20000;
                axisCX2.Param.AccelerationJerk.Value = 200000;
                axisCX2.Param.DeccelerationJerk.Value = 200000;
                axisCX2.Param.SeqSpeed.Value = 2000;
                axisCX2.Param.SeqAcc.Value = 20000;
                axisCX2.Param.SeqDcc.Value = 20000;
                axisCX2.Param.DtoPRatio.Value = 1;
                axisCX2.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCX2.Param.PosSWLimit.Value = 1000000;
                axisCX2.Param.POTIndex.Value = 0;
                axisCX2.Param.NOTIndex.Value = 0;
                axisCX2.Param.HOMEIndex.Value = 1;
                axisCX2.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCX2.Param.HomeDistLimit.Value = 102000;
                axisCX2.Param.IndexDistLimit.Value = 0;

                axisCX2.Config.StopRate.Value = 0.05;
                axisCX2.Config.EStopRate.Value = 0.025;
                axisCX2.Config.Inposition.Value = 200;
                axisCX2.Config.NearTargetDistance.Value = 500;
                axisCX2.Config.VelocityTolerance.Value = 40000;
                axisCX2.Config.SettlingTime.Value = 0;

                axisCX2.Config.MoterConfig.AmpEnable.Value = false;
                axisCX2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCX2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCX2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCX2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCX2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCX2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCX2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCX2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCX2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCX2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCX2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCX2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCX2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCX2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCX2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCX2.Config.MoterConfig.PulseAInv.Value = false;
                axisCX2.Config.MoterConfig.PulseBInv.Value = false;

                axisCX2.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCX2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCX2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCX2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCX2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCX2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCX2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCX2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCX2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCX2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCX2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCX2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCX2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCX2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCX2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCX2.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCX2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCX2.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCX2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCX2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCX2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCX2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCX2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCX2.Config.PIDCoeff.GainProportional.Value = 450;
                axisCX2.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCX2.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCX2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCX2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCX2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCX2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCX2.Config.PIDCoeff.DRate.Value = 0;
                axisCX2.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCX2.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCX2.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCX2.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCX2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCX2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCX2.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCX2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCX2);
                #endregion
                #region Axis CY2
                ProbeAxisObject axisCY2 = new ProbeAxisObject(0, 8, EnumAxisConstants.CY2);
                axisCY2.Param.IndexSearchingSpeed.Value = 1000;
                axisCY2.Param.HomeOffset.Value = 0;
                axisCY2.Param.HomeShift.Value = 0;
                axisCY2.AxisType.Value = EnumAxisConstants.CY2;
                axisCY2.Label.Value = "CY2";
                axisCY2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCY2.Param.FeedOverride.Value = 1;


                axisCY2.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCY2.Param.HomeInvert.Value = false;
                axisCY2.Param.IndexInvert.Value = true;

                axisCY2.Param.HommingSpeed.Value = 2000;
                axisCY2.Param.HommingAcceleration.Value = 20000;
                axisCY2.Param.HommingDecceleration.Value = 20000;
                axisCY2.Param.FinalVelociy.Value = 0;
                axisCY2.Param.Speed.Value = 2000;
                axisCY2.Param.Acceleration.Value = 20000;
                axisCY2.Param.Decceleration.Value = 20000;
                axisCY2.Param.AccelerationJerk.Value = 200000;
                axisCY2.Param.DeccelerationJerk.Value = 200000;
                axisCY2.Param.SeqSpeed.Value = 2000;
                axisCY2.Param.SeqAcc.Value = 20000;
                axisCY2.Param.SeqDcc.Value = 20000;
                axisCY2.Param.DtoPRatio.Value = 1;
                axisCY2.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCY2.Param.PosSWLimit.Value = 1000000;
                axisCY2.Param.POTIndex.Value = 0;
                axisCY2.Param.NOTIndex.Value = 0;
                axisCY2.Param.HOMEIndex.Value = 1;
                axisCY2.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCY2.Param.HomeDistLimit.Value = 102000;
                axisCY2.Param.IndexDistLimit.Value = 0;

                axisCY2.Config.StopRate.Value = 0.05;
                axisCY2.Config.EStopRate.Value = 0.025;
                axisCY2.Config.Inposition.Value = 200;
                axisCY2.Config.NearTargetDistance.Value = 500;
                axisCY2.Config.VelocityTolerance.Value = 40000;
                axisCY2.Config.SettlingTime.Value = 0;

                axisCY2.Config.MoterConfig.AmpEnable.Value = false;
                axisCY2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCY2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCY2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCY2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCY2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCY2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCY2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCY2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCY2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCY2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCY2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCY2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCY2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCY2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCY2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCY2.Config.MoterConfig.PulseAInv.Value = false;
                axisCY2.Config.MoterConfig.PulseBInv.Value = false;

                axisCY2.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCY2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCY2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCY2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCY2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCY2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCY2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCY2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCY2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCY2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCY2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCY2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCY2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCY2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCY2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCY2.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCY2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCY2.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCY2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCY2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCY2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCY2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCY2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCY2.Config.PIDCoeff.GainProportional.Value = 450;
                axisCY2.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCY2.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCY2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCY2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCY2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCY2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCY2.Config.PIDCoeff.DRate.Value = 0;
                axisCY2.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCY2.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCY2.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCY2.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCY2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCY2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCY2.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCY2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCY2);
                #endregion
                #region Axis CZ2
                ProbeAxisObject axisCZ2 = new ProbeAxisObject(0, 9, EnumAxisConstants.CZ2);
                axisCZ2.Param.IndexSearchingSpeed.Value = 1000;
                axisCZ2.Param.HomeOffset.Value = 0;
                axisCZ2.Param.HomeShift.Value = 0;
                axisCZ2.AxisType.Value = EnumAxisConstants.CZ2;
                axisCZ2.Label.Value = "CZ2";
                axisCZ2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCZ2.Param.FeedOverride.Value = 1;


                axisCZ2.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCZ2.Param.HomeInvert.Value = false;
                axisCZ2.Param.IndexInvert.Value = true;

                axisCZ2.Param.HommingSpeed.Value = 2000;
                axisCZ2.Param.HommingAcceleration.Value = 20000;
                axisCZ2.Param.HommingDecceleration.Value = 20000;
                axisCZ2.Param.FinalVelociy.Value = 0;
                axisCZ2.Param.Speed.Value = 2000;
                axisCZ2.Param.Acceleration.Value = 20000;
                axisCZ2.Param.Decceleration.Value = 20000;
                axisCZ2.Param.AccelerationJerk.Value = 200000;
                axisCZ2.Param.DeccelerationJerk.Value = 200000;
                axisCZ2.Param.SeqSpeed.Value = 2000;
                axisCZ2.Param.SeqAcc.Value = 20000;
                axisCZ2.Param.SeqDcc.Value = 20000;
                axisCZ2.Param.DtoPRatio.Value = 1;
                axisCZ2.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCZ2.Param.PosSWLimit.Value = 1000000;
                axisCZ2.Param.POTIndex.Value = 0;
                axisCZ2.Param.NOTIndex.Value = 0;
                axisCZ2.Param.HOMEIndex.Value = 1;
                axisCZ2.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCZ2.Param.HomeDistLimit.Value = 102000;
                axisCZ2.Param.IndexDistLimit.Value = 0;

                axisCZ2.Config.StopRate.Value = 0.05;
                axisCZ2.Config.EStopRate.Value = 0.025;
                axisCZ2.Config.Inposition.Value = 200;
                axisCZ2.Config.NearTargetDistance.Value = 500;
                axisCZ2.Config.VelocityTolerance.Value = 40000;
                axisCZ2.Config.SettlingTime.Value = 0;

                axisCZ2.Config.MoterConfig.AmpEnable.Value = false;
                axisCZ2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCZ2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCZ2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCZ2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCZ2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCZ2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCZ2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCZ2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCZ2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCZ2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCZ2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCZ2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCZ2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCZ2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCZ2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCZ2.Config.MoterConfig.PulseAInv.Value = false;
                axisCZ2.Config.MoterConfig.PulseBInv.Value = false;

                axisCZ2.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCZ2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCZ2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCZ2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCZ2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCZ2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCZ2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCZ2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCZ2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCZ2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCZ2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCZ2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCZ2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCZ2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCZ2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCZ2.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCZ2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCZ2.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCZ2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCZ2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCZ2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCZ2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCZ2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCZ2.Config.PIDCoeff.GainProportional.Value = 450;
                axisCZ2.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCZ2.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCZ2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCZ2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCZ2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCZ2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCZ2.Config.PIDCoeff.DRate.Value = 0;
                axisCZ2.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCZ2.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCZ2.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCZ2.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCZ2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCZ2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCZ2.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCZ2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCZ2);
                #endregion

                #region Axis CX3
                ProbeAxisObject axisCX3 = new ProbeAxisObject(0, 10, EnumAxisConstants.CX3);
                axisCX3.Param.IndexSearchingSpeed.Value = 1000;
                axisCX3.Param.HomeOffset.Value = 0;
                axisCX3.Param.HomeShift.Value = 0;
                axisCX3.AxisType.Value = EnumAxisConstants.CX3;
                axisCX3.Label.Value = "CX3";
                axisCX3.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCX3.Param.FeedOverride.Value = 1;


                axisCX3.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCX3.Param.HomeInvert.Value = false;
                axisCX3.Param.IndexInvert.Value = true;

                axisCX3.Param.HommingSpeed.Value = 2000;
                axisCX3.Param.HommingAcceleration.Value = 20000;
                axisCX3.Param.HommingDecceleration.Value = 20000;
                axisCX3.Param.FinalVelociy.Value = 0;
                axisCX3.Param.Speed.Value = 2000;
                axisCX3.Param.Acceleration.Value = 20000;
                axisCX3.Param.Decceleration.Value = 20000;
                axisCX3.Param.AccelerationJerk.Value = 200000;
                axisCX3.Param.DeccelerationJerk.Value = 200000;
                axisCX3.Param.SeqSpeed.Value = 2000;
                axisCX3.Param.SeqAcc.Value = 20000;
                axisCX3.Param.SeqDcc.Value = 20000;
                axisCX3.Param.DtoPRatio.Value = 1;
                axisCX3.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCX3.Param.PosSWLimit.Value = 1000000;
                axisCX3.Param.POTIndex.Value = 0;
                axisCX3.Param.NOTIndex.Value = 0;
                axisCX3.Param.HOMEIndex.Value = 1;
                axisCX3.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCX3.Param.HomeDistLimit.Value = 102000;
                axisCX3.Param.IndexDistLimit.Value = 0;

                axisCX3.Config.StopRate.Value = 0.05;
                axisCX3.Config.EStopRate.Value = 0.025;
                axisCX3.Config.Inposition.Value = 200;
                axisCX3.Config.NearTargetDistance.Value = 500;
                axisCX3.Config.VelocityTolerance.Value = 40000;
                axisCX3.Config.SettlingTime.Value = 0;

                axisCX3.Config.MoterConfig.AmpEnable.Value = false;
                axisCX3.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCX3.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCX3.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCX3.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCX3.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCX3.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCX3.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCX3.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCX3.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCX3.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCX3.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCX3.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCX3.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCX3.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCX3.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCX3.Config.MoterConfig.PulseAInv.Value = false;
                axisCX3.Config.MoterConfig.PulseBInv.Value = false;

                axisCX3.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCX3.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCX3.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCX3.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCX3.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCX3.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCX3.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCX3.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX3.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCX3.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCX3.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCX3.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCX3.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCX3.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX3.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCX3.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCX3.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX3.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCX3.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCX3.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCX3.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCX3.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCX3.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCX3.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCX3.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCX3.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCX3.Config.PIDCoeff.GainProportional.Value = 450;
                axisCX3.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCX3.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCX3.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCX3.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCX3.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCX3.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCX3.Config.PIDCoeff.DRate.Value = 0;
                axisCX3.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCX3.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCX3.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCX3.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCX3.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCX3.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCX3.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCX3.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCX3);
                #endregion
                #region Axis CY3
                ProbeAxisObject axisCY3 = new ProbeAxisObject(0, 11, EnumAxisConstants.CY3);
                axisCY3.Param.IndexSearchingSpeed.Value = 1000;
                axisCY3.Param.HomeOffset.Value = 0;
                axisCY3.Param.HomeShift.Value = 0;
                axisCY3.AxisType.Value = EnumAxisConstants.CY3;
                axisCY3.Label.Value = "CY3";
                axisCY3.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCY3.Param.FeedOverride.Value = 1;


                axisCY3.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCY3.Param.HomeInvert.Value = false;
                axisCY3.Param.IndexInvert.Value = true;

                axisCY3.Param.HommingSpeed.Value = 2000;
                axisCY3.Param.HommingAcceleration.Value = 20000;
                axisCY3.Param.HommingDecceleration.Value = 20000;
                axisCY3.Param.FinalVelociy.Value = 0;
                axisCY3.Param.Speed.Value = 2000;
                axisCY3.Param.Acceleration.Value = 20000;
                axisCY3.Param.Decceleration.Value = 20000;
                axisCY3.Param.AccelerationJerk.Value = 200000;
                axisCY3.Param.DeccelerationJerk.Value = 200000;
                axisCY3.Param.SeqSpeed.Value = 2000;
                axisCY3.Param.SeqAcc.Value = 20000;
                axisCY3.Param.SeqDcc.Value = 20000;
                axisCY3.Param.DtoPRatio.Value = 1;
                axisCY3.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCY3.Param.PosSWLimit.Value = 1000000;
                axisCY3.Param.POTIndex.Value = 0;
                axisCY3.Param.NOTIndex.Value = 0;
                axisCY3.Param.HOMEIndex.Value = 1;
                axisCY3.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCY3.Param.HomeDistLimit.Value = 102000;
                axisCY3.Param.IndexDistLimit.Value = 0;

                axisCY3.Config.StopRate.Value = 0.05;
                axisCY3.Config.EStopRate.Value = 0.025;
                axisCY3.Config.Inposition.Value = 200;
                axisCY3.Config.NearTargetDistance.Value = 500;
                axisCY3.Config.VelocityTolerance.Value = 40000;
                axisCY3.Config.SettlingTime.Value = 0;

                axisCY3.Config.MoterConfig.AmpEnable.Value = false;
                axisCY3.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCY3.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCY3.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCY3.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCY3.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCY3.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCY3.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCY3.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCY3.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCY3.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCY3.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCY3.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCY3.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCY3.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCY3.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCY3.Config.MoterConfig.PulseAInv.Value = false;
                axisCY3.Config.MoterConfig.PulseBInv.Value = false;

                axisCY3.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCY3.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCY3.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCY3.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCY3.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCY3.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCY3.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCY3.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY3.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCY3.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCY3.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCY3.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCY3.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCY3.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY3.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCY3.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCY3.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY3.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCY3.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCY3.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCY3.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCY3.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCY3.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCY3.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCY3.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCY3.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCY3.Config.PIDCoeff.GainProportional.Value = 450;
                axisCY3.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCY3.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCY3.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCY3.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCY3.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCY3.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCY3.Config.PIDCoeff.DRate.Value = 0;
                axisCY3.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCY3.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCY3.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCY3.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCY3.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCY3.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCY3.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCY3.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCY3);
                #endregion
                #region Axis CZ3
                ProbeAxisObject axisCZ3 = new ProbeAxisObject(0, 12, EnumAxisConstants.CZ3);
                axisCZ3.Param.IndexSearchingSpeed.Value = 1000;
                axisCZ3.Param.HomeOffset.Value = 0;
                axisCZ3.Param.HomeShift.Value = 0;
                axisCZ3.AxisType.Value = EnumAxisConstants.CZ3;
                axisCZ3.Label.Value = "CZ3";
                axisCZ3.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCZ3.Param.FeedOverride.Value = 1;


                axisCZ3.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCZ3.Param.HomeInvert.Value = false;
                axisCZ3.Param.IndexInvert.Value = true;

                axisCZ3.Param.HommingSpeed.Value = 2000;
                axisCZ3.Param.HommingAcceleration.Value = 20000;
                axisCZ3.Param.HommingDecceleration.Value = 20000;
                axisCZ3.Param.FinalVelociy.Value = 0;
                axisCZ3.Param.Speed.Value = 2000;
                axisCZ3.Param.Acceleration.Value = 20000;
                axisCZ3.Param.Decceleration.Value = 20000;
                axisCZ3.Param.AccelerationJerk.Value = 200000;
                axisCZ3.Param.DeccelerationJerk.Value = 200000;
                axisCZ3.Param.SeqSpeed.Value = 2000;
                axisCZ3.Param.SeqAcc.Value = 20000;
                axisCZ3.Param.SeqDcc.Value = 20000;
                axisCZ3.Param.DtoPRatio.Value = 1;
                axisCZ3.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCZ3.Param.PosSWLimit.Value = 1000000;
                axisCZ3.Param.POTIndex.Value = 0;
                axisCZ3.Param.NOTIndex.Value = 0;
                axisCZ3.Param.HOMEIndex.Value = 1;
                axisCZ3.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCZ3.Param.HomeDistLimit.Value = 102000;
                axisCZ3.Param.IndexDistLimit.Value = 0;

                axisCZ3.Config.StopRate.Value = 0.05;
                axisCZ3.Config.EStopRate.Value = 0.025;
                axisCZ3.Config.Inposition.Value = 200;
                axisCZ3.Config.NearTargetDistance.Value = 500;
                axisCZ3.Config.VelocityTolerance.Value = 40000;
                axisCZ3.Config.SettlingTime.Value = 0;

                axisCZ3.Config.MoterConfig.AmpEnable.Value = false;
                axisCZ3.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCZ3.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCZ3.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCZ3.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCZ3.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCZ3.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCZ3.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCZ3.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCZ3.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCZ3.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCZ3.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCZ3.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCZ3.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCZ3.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCZ3.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCZ3.Config.MoterConfig.PulseAInv.Value = false;
                axisCZ3.Config.MoterConfig.PulseBInv.Value = false;

                axisCZ3.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCZ3.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCZ3.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCZ3.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCZ3.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCZ3.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCZ3.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCZ3.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ3.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCZ3.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCZ3.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCZ3.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCZ3.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCZ3.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ3.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCZ3.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCZ3.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ3.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCZ3.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCZ3.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCZ3.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCZ3.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCZ3.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCZ3.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCZ3.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCZ3.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCZ3.Config.PIDCoeff.GainProportional.Value = 450;
                axisCZ3.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCZ3.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCZ3.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCZ3.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCZ3.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCZ3.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCZ3.Config.PIDCoeff.DRate.Value = 0;
                axisCZ3.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCZ3.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCZ3.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCZ3.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCZ3.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCZ3.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCZ3.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCZ3.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCZ3);
                #endregion

                #region Axis CX4
                ProbeAxisObject axisCX4 = new ProbeAxisObject(0, 13, EnumAxisConstants.CX4);
                axisCX4.Param.IndexSearchingSpeed.Value = 1000;
                axisCX4.Param.HomeOffset.Value = 0;
                axisCX4.Param.HomeShift.Value = 0;
                axisCX4.AxisType.Value = EnumAxisConstants.CX4;
                axisCX4.Label.Value = "CX4";
                axisCX4.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCX4.Param.FeedOverride.Value = 1;


                axisCX4.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCX4.Param.HomeInvert.Value = false;
                axisCX4.Param.IndexInvert.Value = true;

                axisCX4.Param.HommingSpeed.Value = 2000;
                axisCX4.Param.HommingAcceleration.Value = 20000;
                axisCX4.Param.HommingDecceleration.Value = 20000;
                axisCX4.Param.FinalVelociy.Value = 0;
                axisCX4.Param.Speed.Value = 2000;
                axisCX4.Param.Acceleration.Value = 20000;
                axisCX4.Param.Decceleration.Value = 20000;
                axisCX4.Param.AccelerationJerk.Value = 200000;
                axisCX4.Param.DeccelerationJerk.Value = 200000;
                axisCX4.Param.SeqSpeed.Value = 2000;
                axisCX4.Param.SeqAcc.Value = 20000;
                axisCX4.Param.SeqDcc.Value = 20000;
                axisCX4.Param.DtoPRatio.Value = 1;
                axisCX4.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCX4.Param.PosSWLimit.Value = 1000000;
                axisCX4.Param.POTIndex.Value = 0;
                axisCX4.Param.NOTIndex.Value = 0;
                axisCX4.Param.HOMEIndex.Value = 1;
                axisCX4.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCX4.Param.HomeDistLimit.Value = 102000;
                axisCX4.Param.IndexDistLimit.Value = 0;

                axisCX4.Config.StopRate.Value = 0.05;
                axisCX4.Config.EStopRate.Value = 0.025;
                axisCX4.Config.Inposition.Value = 200;
                axisCX4.Config.NearTargetDistance.Value = 500;
                axisCX4.Config.VelocityTolerance.Value = 40000;
                axisCX4.Config.SettlingTime.Value = 0;

                axisCX4.Config.MoterConfig.AmpEnable.Value = false;
                axisCX4.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCX4.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCX4.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCX4.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCX4.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCX4.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCX4.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCX4.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCX4.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCX4.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCX4.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCX4.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCX4.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCX4.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCX4.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCX4.Config.MoterConfig.PulseAInv.Value = false;
                axisCX4.Config.MoterConfig.PulseBInv.Value = false;

                axisCX4.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCX4.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCX4.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCX4.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCX4.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCX4.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCX4.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCX4.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX4.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCX4.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCX4.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCX4.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCX4.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCX4.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX4.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCX4.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCX4.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCX4.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCX4.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCX4.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCX4.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCX4.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCX4.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCX4.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCX4.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCX4.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCX4.Config.PIDCoeff.GainProportional.Value = 450;
                axisCX4.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCX4.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCX4.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCX4.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCX4.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCX4.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCX4.Config.PIDCoeff.DRate.Value = 0;
                axisCX4.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCX4.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCX4.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCX4.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCX4.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCX4.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCX4.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCX4.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCX4);
                #endregion
                #region Axis CY4
                ProbeAxisObject axisCY4 = new ProbeAxisObject(0, 14, EnumAxisConstants.CY4);
                axisCY4.Param.IndexSearchingSpeed.Value = 1000;
                axisCY4.Param.HomeOffset.Value = 0;
                axisCY4.Param.HomeShift.Value = 0;
                axisCY4.AxisType.Value = EnumAxisConstants.CY4;
                axisCY4.Label.Value = "CY4";
                axisCY4.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCY4.Param.FeedOverride.Value = 1;


                axisCY4.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCY4.Param.HomeInvert.Value = false;
                axisCY4.Param.IndexInvert.Value = true;

                axisCY4.Param.HommingSpeed.Value = 2000;
                axisCY4.Param.HommingAcceleration.Value = 20000;
                axisCY4.Param.HommingDecceleration.Value = 20000;
                axisCY4.Param.FinalVelociy.Value = 0;
                axisCY4.Param.Speed.Value = 2000;
                axisCY4.Param.Acceleration.Value = 20000;
                axisCY4.Param.Decceleration.Value = 20000;
                axisCY4.Param.AccelerationJerk.Value = 200000;
                axisCY4.Param.DeccelerationJerk.Value = 200000;
                axisCY4.Param.SeqSpeed.Value = 2000;
                axisCY4.Param.SeqAcc.Value = 20000;
                axisCY4.Param.SeqDcc.Value = 20000;
                axisCY4.Param.DtoPRatio.Value = 1;
                axisCY4.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCY4.Param.PosSWLimit.Value = 1000000;
                axisCY4.Param.POTIndex.Value = 0;
                axisCY4.Param.NOTIndex.Value = 0;
                axisCY4.Param.HOMEIndex.Value = 1;
                axisCY4.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCY4.Param.HomeDistLimit.Value = 102000;
                axisCY4.Param.IndexDistLimit.Value = 0;

                axisCY4.Config.StopRate.Value = 0.05;
                axisCY4.Config.EStopRate.Value = 0.025;
                axisCY4.Config.Inposition.Value = 200;
                axisCY4.Config.NearTargetDistance.Value = 500;
                axisCY4.Config.VelocityTolerance.Value = 40000;
                axisCY4.Config.SettlingTime.Value = 0;

                axisCY4.Config.MoterConfig.AmpEnable.Value = false;
                axisCY4.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCY4.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCY4.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCY4.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCY4.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCY4.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCY4.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCY4.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCY4.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCY4.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCY4.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCY4.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCY4.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCY4.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCY4.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCY4.Config.MoterConfig.PulseAInv.Value = false;
                axisCY4.Config.MoterConfig.PulseBInv.Value = false;

                axisCY4.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCY4.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCY4.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCY4.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCY4.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCY4.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCY4.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCY4.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY4.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCY4.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCY4.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCY4.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCY4.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCY4.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY4.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCY4.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCY4.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCY4.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCY4.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCY4.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCY4.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCY4.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCY4.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCY4.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCY4.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCY4.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCY4.Config.PIDCoeff.GainProportional.Value = 450;
                axisCY4.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCY4.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCY4.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCY4.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCY4.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCY4.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCY4.Config.PIDCoeff.DRate.Value = 0;
                axisCY4.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCY4.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCY4.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCY4.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCY4.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCY4.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCY4.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCY4.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCY4);
                #endregion
                #region Axis CZ4
                ProbeAxisObject axisCZ4 = new ProbeAxisObject(0, 15, EnumAxisConstants.CZ4);
                axisCZ4.Param.IndexSearchingSpeed.Value = 1000;
                axisCZ4.Param.HomeOffset.Value = 0;
                axisCZ4.Param.HomeShift.Value = 0;
                axisCZ4.AxisType.Value = EnumAxisConstants.CZ4;
                axisCZ4.Label.Value = "CZ4";
                axisCZ4.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCZ4.Param.FeedOverride.Value = 1;


                axisCZ4.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCZ4.Param.HomeInvert.Value = false;
                axisCZ4.Param.IndexInvert.Value = true;

                axisCZ4.Param.HommingSpeed.Value = 2000;
                axisCZ4.Param.HommingAcceleration.Value = 20000;
                axisCZ4.Param.HommingDecceleration.Value = 20000;
                axisCZ4.Param.FinalVelociy.Value = 0;
                axisCZ4.Param.Speed.Value = 2000;
                axisCZ4.Param.Acceleration.Value = 20000;
                axisCZ4.Param.Decceleration.Value = 20000;
                axisCZ4.Param.AccelerationJerk.Value = 200000;
                axisCZ4.Param.DeccelerationJerk.Value = 200000;
                axisCZ4.Param.SeqSpeed.Value = 2000;
                axisCZ4.Param.SeqAcc.Value = 20000;
                axisCZ4.Param.SeqDcc.Value = 20000;
                axisCZ4.Param.DtoPRatio.Value = 1;
                axisCZ4.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCZ4.Param.PosSWLimit.Value = 1000000;
                axisCZ4.Param.POTIndex.Value = 0;
                axisCZ4.Param.NOTIndex.Value = 0;
                axisCZ4.Param.HOMEIndex.Value = 1;
                axisCZ4.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCZ4.Param.HomeDistLimit.Value = 102000;
                axisCZ4.Param.IndexDistLimit.Value = 0;

                axisCZ4.Config.StopRate.Value = 0.05;
                axisCZ4.Config.EStopRate.Value = 0.025;
                axisCZ4.Config.Inposition.Value = 200;
                axisCZ4.Config.NearTargetDistance.Value = 500;
                axisCZ4.Config.VelocityTolerance.Value = 40000;
                axisCZ4.Config.SettlingTime.Value = 0;

                axisCZ4.Config.MoterConfig.AmpEnable.Value = false;
                axisCZ4.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCZ4.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCZ4.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCZ4.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCZ4.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCZ4.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCZ4.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCZ4.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCZ4.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCZ4.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCZ4.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCZ4.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCZ4.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCZ4.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCZ4.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCZ4.Config.MoterConfig.PulseAInv.Value = false;
                axisCZ4.Config.MoterConfig.PulseBInv.Value = false;

                axisCZ4.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCZ4.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCZ4.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCZ4.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCZ4.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCZ4.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCZ4.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCZ4.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ4.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCZ4.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCZ4.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCZ4.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCZ4.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCZ4.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ4.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCZ4.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCZ4.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ4.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCZ4.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCZ4.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCZ4.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCZ4.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCZ4.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCZ4.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCZ4.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCZ4.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCZ4.Config.PIDCoeff.GainProportional.Value = 450;
                axisCZ4.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCZ4.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCZ4.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCZ4.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCZ4.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCZ4.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCZ4.Config.PIDCoeff.DRate.Value = 0;
                axisCZ4.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCZ4.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCZ4.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCZ4.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCZ4.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCZ4.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCZ4.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCZ4.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCZ4);
                #endregion

                #region Axis CZ5
                ProbeAxisObject axisCZ5 = new ProbeAxisObject(0, 16, EnumAxisConstants.CZ5);
                axisCZ5.Param.IndexSearchingSpeed.Value = 1000;
                axisCZ5.Param.HomeOffset.Value = 0;
                axisCZ5.Param.HomeShift.Value = 0;
                axisCZ5.AxisType.Value = EnumAxisConstants.CZ5;
                axisCZ5.Label.Value = "CZ5";
                axisCZ5.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisCZ5.Param.FeedOverride.Value = 1;


                axisCZ5.HomingType.Value = HomingMethodType.RLSEDGE;
                axisCZ5.Param.HomeInvert.Value = false;
                axisCZ5.Param.IndexInvert.Value = true;

                axisCZ5.Param.HommingSpeed.Value = 2000;
                axisCZ5.Param.HommingAcceleration.Value = 20000;
                axisCZ5.Param.HommingDecceleration.Value = 20000;
                axisCZ5.Param.FinalVelociy.Value = 0;
                axisCZ5.Param.Speed.Value = 2000;
                axisCZ5.Param.Acceleration.Value = 20000;
                axisCZ5.Param.Decceleration.Value = 20000;
                axisCZ5.Param.AccelerationJerk.Value = 200000;
                axisCZ5.Param.DeccelerationJerk.Value = 200000;
                axisCZ5.Param.SeqSpeed.Value = 2000;
                axisCZ5.Param.SeqAcc.Value = 20000;
                axisCZ5.Param.SeqDcc.Value = 20000;
                axisCZ5.Param.DtoPRatio.Value = 1;
                axisCZ5.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisCZ5.Param.PosSWLimit.Value = 1000000;
                axisCZ5.Param.POTIndex.Value = 0;
                axisCZ5.Param.NOTIndex.Value = 0;
                axisCZ5.Param.HOMEIndex.Value = 1;
                axisCZ5.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisCZ5.Param.HomeDistLimit.Value = 102000;
                axisCZ5.Param.IndexDistLimit.Value = 0;

                axisCZ5.Config.StopRate.Value = 0.05;
                axisCZ5.Config.EStopRate.Value = 0.025;
                axisCZ5.Config.Inposition.Value = 200;
                axisCZ5.Config.NearTargetDistance.Value = 500;
                axisCZ5.Config.VelocityTolerance.Value = 40000;
                axisCZ5.Config.SettlingTime.Value = 0;

                axisCZ5.Config.MoterConfig.AmpEnable.Value = false;
                axisCZ5.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisCZ5.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisCZ5.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisCZ5.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisCZ5.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisCZ5.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisCZ5.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisCZ5.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisCZ5.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisCZ5.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisCZ5.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisCZ5.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisCZ5.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisCZ5.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisCZ5.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisCZ5.Config.MoterConfig.PulseAInv.Value = false;
                axisCZ5.Config.MoterConfig.PulseBInv.Value = false;

                axisCZ5.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisCZ5.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisCZ5.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisCZ5.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisCZ5.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisCZ5.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisCZ5.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisCZ5.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ5.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisCZ5.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisCZ5.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisCZ5.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisCZ5.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisCZ5.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ5.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisCZ5.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisCZ5.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisCZ5.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisCZ5.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisCZ5.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisCZ5.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisCZ5.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisCZ5.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisCZ5.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisCZ5.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisCZ5.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisCZ5.Config.PIDCoeff.GainProportional.Value = 450;
                axisCZ5.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisCZ5.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisCZ5.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisCZ5.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisCZ5.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisCZ5.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisCZ5.Config.PIDCoeff.DRate.Value = 0;
                axisCZ5.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisCZ5.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisCZ5.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisCZ5.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisCZ5.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisCZ5.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisCZ5.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisCZ5.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisCZ5);
                #endregion

                #region Axis NSZ1

                ProbeAxisObject axisNSZ1 = new ProbeAxisObject(0, 17, EnumAxisConstants.NSZ1);
                axisNSZ1.Param.IndexSearchingSpeed.Value = 300;
                axisNSZ1.Param.HomeOffset.Value = 0;
                axisNSZ1.Param.HomeShift.Value = 0;
                axisNSZ1.AxisType.Value = EnumAxisConstants.NSZ1;
                axisNSZ1.Label.Value = "NSZ1";
                axisNSZ1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisNSZ1.MasterAxis.Value = EnumAxisConstants.Z;

                axisNSZ1.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisNSZ1.Param.HomeInvert.Value = false;
                axisNSZ1.Param.IndexInvert.Value = false;
                axisNSZ1.Param.FeedOverride.Value = 1;


                axisNSZ1.Param.HommingSpeed.Value = 2000;
                axisNSZ1.Param.HommingAcceleration.Value = 20000;
                axisNSZ1.Param.HommingDecceleration.Value = 20000;
                axisNSZ1.Param.FinalVelociy.Value = 0;
                axisNSZ1.Param.Speed.Value = 2000;
                axisNSZ1.Param.Acceleration.Value = 20000;
                axisNSZ1.Param.Decceleration.Value = 20000;
                axisNSZ1.Param.AccelerationJerk.Value = 200000;
                axisNSZ1.Param.DeccelerationJerk.Value = 200000;
                axisNSZ1.Param.SeqAcc.Value = 0;
                axisNSZ1.Param.SeqDcc.Value = 0;
                axisNSZ1.Param.DtoPRatio.Value = 2.5;
                axisNSZ1.Param.NegSWLimit.Value = -1000000;
                axisNSZ1.Param.PosSWLimit.Value = -1000000;
                axisNSZ1.Param.POTIndex.Value = 0;
                axisNSZ1.Param.NOTIndex.Value = 0;
                axisNSZ1.Param.HOMEIndex.Value = 1;
                axisNSZ1.Param.TimeOut.Value = 600000;
                axisNSZ1.Param.HomeDistLimit.Value = 49500;
                axisNSZ1.Param.IndexDistLimit.Value = 7000;

                axisNSZ1.Config.StopRate.Value = 0.05;
                axisNSZ1.Config.EStopRate.Value = 0.025;
                axisNSZ1.Config.Inposition.Value = 1118;
                axisNSZ1.Config.NearTargetDistance.Value = 5590;
                axisNSZ1.Config.VelocityTolerance.Value = 559241;
                axisNSZ1.Config.SettlingTime.Value = 0;

                axisNSZ1.Config.MoterConfig.AmpEnable.Value = true;
                axisNSZ1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisNSZ1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisNSZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisNSZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisNSZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisNSZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisNSZ1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisNSZ1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisNSZ1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisNSZ1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisNSZ1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisNSZ1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisNSZ1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisNSZ1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisNSZ1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisNSZ1.Config.MoterConfig.PulseAInv.Value = false;
                axisNSZ1.Config.MoterConfig.PulseBInv.Value = false;

                axisNSZ1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisNSZ1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisNSZ1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisNSZ1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisNSZ1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisNSZ1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisNSZ1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisNSZ1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisNSZ1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisNSZ1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisNSZ1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisNSZ1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisNSZ1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisNSZ1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisNSZ1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisNSZ1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisNSZ1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisNSZ1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisNSZ1.Config.MoterConfig.SWNegLimitTrigger.Value = -1000000;
                axisNSZ1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisNSZ1.Config.MoterConfig.SWPosLimitTrigger.Value = -1000000;
                axisNSZ1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisNSZ1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisNSZ1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisNSZ1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisNSZ1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisNSZ1.Config.PIDCoeff.GainProportional.Value = 0;
                axisNSZ1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisNSZ1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisNSZ1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisNSZ1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisNSZ1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisNSZ1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisNSZ1.Config.PIDCoeff.DRate.Value = 0;
                axisNSZ1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisNSZ1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisNSZ1.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisNSZ1.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisNSZ1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisNSZ1.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisNSZ1.Config.PIDCoeff.OutputOffset.Value = 0;
                axisNSZ1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisNSZ1);

                #endregion
                #region Axis FDZ1

                ProbeAxisObject axisFDZ1 = new ProbeAxisObject(0, 18, EnumAxisConstants.FDZ1);
                axisFDZ1.Param.IndexSearchingSpeed.Value = 300;
                axisFDZ1.Param.HomeOffset.Value = 0;
                axisFDZ1.Param.HomeShift.Value = 0;
                axisFDZ1.AxisType.Value = EnumAxisConstants.FDZ1;
                axisFDZ1.Label.Value = "FDZ1";
                axisFDZ1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisFDZ1.MasterAxis.Value = EnumAxisConstants.Z;

                axisFDZ1.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisFDZ1.Param.HomeInvert.Value = false;
                axisFDZ1.Param.IndexInvert.Value = false;
                axisFDZ1.Param.FeedOverride.Value = 1;


                axisFDZ1.Param.HommingSpeed.Value = 2000;
                axisFDZ1.Param.HommingAcceleration.Value = 20000;
                axisFDZ1.Param.HommingDecceleration.Value = 20000;
                axisFDZ1.Param.FinalVelociy.Value = 0;
                axisFDZ1.Param.Speed.Value = 2000;
                axisFDZ1.Param.Acceleration.Value = 20000;
                axisFDZ1.Param.Decceleration.Value = 20000;
                axisFDZ1.Param.AccelerationJerk.Value = 200000;
                axisFDZ1.Param.DeccelerationJerk.Value = 200000;
                axisFDZ1.Param.SeqAcc.Value = 0;
                axisFDZ1.Param.SeqDcc.Value = 0;
                axisFDZ1.Param.DtoPRatio.Value = 2.5;
                axisFDZ1.Param.NegSWLimit.Value = -1000000;
                axisFDZ1.Param.PosSWLimit.Value = -1000000;
                axisFDZ1.Param.POTIndex.Value = 0;
                axisFDZ1.Param.NOTIndex.Value = 0;
                axisFDZ1.Param.HOMEIndex.Value = 1;
                axisFDZ1.Param.TimeOut.Value = 600000;
                axisFDZ1.Param.HomeDistLimit.Value = 49500;
                axisFDZ1.Param.IndexDistLimit.Value = 7000;

                axisFDZ1.Config.StopRate.Value = 0.05;
                axisFDZ1.Config.EStopRate.Value = 0.025;
                axisFDZ1.Config.Inposition.Value = 1118;
                axisFDZ1.Config.NearTargetDistance.Value = 5590;
                axisFDZ1.Config.VelocityTolerance.Value = 559241;
                axisFDZ1.Config.SettlingTime.Value = 0;

                axisFDZ1.Config.MoterConfig.AmpEnable.Value = true;
                axisFDZ1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisFDZ1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisFDZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisFDZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisFDZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisFDZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisFDZ1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisFDZ1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisFDZ1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisFDZ1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisFDZ1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisFDZ1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisFDZ1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisFDZ1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisFDZ1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisFDZ1.Config.MoterConfig.PulseAInv.Value = false;
                axisFDZ1.Config.MoterConfig.PulseBInv.Value = false;

                axisFDZ1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisFDZ1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisFDZ1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisFDZ1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisFDZ1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisFDZ1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisFDZ1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisFDZ1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisFDZ1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisFDZ1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisFDZ1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisFDZ1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisFDZ1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisFDZ1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisFDZ1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisFDZ1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisFDZ1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisFDZ1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisFDZ1.Config.MoterConfig.SWNegLimitTrigger.Value = -1000000;
                axisFDZ1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisFDZ1.Config.MoterConfig.SWPosLimitTrigger.Value = -1000000;
                axisFDZ1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisFDZ1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisFDZ1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisFDZ1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisFDZ1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisFDZ1.Config.PIDCoeff.GainProportional.Value = 0;
                axisFDZ1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisFDZ1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisFDZ1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisFDZ1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisFDZ1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisFDZ1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisFDZ1.Config.PIDCoeff.DRate.Value = 0;
                axisFDZ1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisFDZ1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisFDZ1.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisFDZ1.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisFDZ1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisFDZ1.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisFDZ1.Config.PIDCoeff.OutputOffset.Value = 0;
                axisFDZ1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisFDZ1);

                #endregion

                #region Axis EJX1
                ProbeAxisObject axisEJX1 = new ProbeAxisObject(0, 20, EnumAxisConstants.EJX1);
                axisEJX1.Param.IndexSearchingSpeed.Value = 1000;
                axisEJX1.Param.HomeOffset.Value = 0;
                axisEJX1.Param.HomeShift.Value = 0;
                axisEJX1.AxisType.Value = EnumAxisConstants.EJX1;
                axisEJX1.Label.Value = "EJX1";
                axisEJX1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisEJX1.Param.FeedOverride.Value = 1;


                axisEJX1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisEJX1.Param.HomeInvert.Value = false;
                axisEJX1.Param.IndexInvert.Value = true;

                axisEJX1.Param.HommingSpeed.Value = 2000;
                axisEJX1.Param.HommingAcceleration.Value = 20000;
                axisEJX1.Param.HommingDecceleration.Value = 20000;
                axisEJX1.Param.FinalVelociy.Value = 0;
                axisEJX1.Param.Speed.Value = 2000;
                axisEJX1.Param.Acceleration.Value = 20000;
                axisEJX1.Param.Decceleration.Value = 20000;
                axisEJX1.Param.AccelerationJerk.Value = 200000;
                axisEJX1.Param.DeccelerationJerk.Value = 200000;
                axisEJX1.Param.SeqSpeed.Value = 2000;
                axisEJX1.Param.SeqAcc.Value = 20000;
                axisEJX1.Param.SeqDcc.Value = 20000;
                axisEJX1.Param.DtoPRatio.Value = 1;
                axisEJX1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisEJX1.Param.PosSWLimit.Value = 1000000;
                axisEJX1.Param.POTIndex.Value = 0;
                axisEJX1.Param.NOTIndex.Value = 0;
                axisEJX1.Param.HOMEIndex.Value = 1;
                axisEJX1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisEJX1.Param.HomeDistLimit.Value = 102000;
                axisEJX1.Param.IndexDistLimit.Value = 0;

                axisEJX1.Config.StopRate.Value = 0.05;
                axisEJX1.Config.EStopRate.Value = 0.025;
                axisEJX1.Config.Inposition.Value = 200;
                axisEJX1.Config.NearTargetDistance.Value = 500;
                axisEJX1.Config.VelocityTolerance.Value = 40000;
                axisEJX1.Config.SettlingTime.Value = 0;

                axisEJX1.Config.MoterConfig.AmpEnable.Value = false;
                axisEJX1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisEJX1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisEJX1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisEJX1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisEJX1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisEJX1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisEJX1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisEJX1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisEJX1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisEJX1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisEJX1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisEJX1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisEJX1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisEJX1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisEJX1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisEJX1.Config.MoterConfig.PulseAInv.Value = false;
                axisEJX1.Config.MoterConfig.PulseBInv.Value = false;

                axisEJX1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisEJX1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisEJX1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisEJX1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisEJX1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisEJX1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisEJX1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisEJX1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJX1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisEJX1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisEJX1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisEJX1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisEJX1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisEJX1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJX1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisEJX1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisEJX1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJX1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisEJX1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisEJX1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisEJX1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisEJX1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisEJX1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisEJX1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisEJX1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisEJX1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisEJX1.Config.PIDCoeff.GainProportional.Value = 450;
                axisEJX1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisEJX1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisEJX1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisEJX1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisEJX1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisEJX1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisEJX1.Config.PIDCoeff.DRate.Value = 0;
                axisEJX1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisEJX1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisEJX1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisEJX1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisEJX1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisEJX1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisEJX1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisEJX1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisEJX1);
                #endregion
                #region Axis EJY1
                ProbeAxisObject axisEJY1 = new ProbeAxisObject(0, 21, EnumAxisConstants.EJY1);
                axisEJY1.Param.IndexSearchingSpeed.Value = 1000;
                axisEJY1.Param.HomeOffset.Value = 0;
                axisEJY1.Param.HomeShift.Value = 0;
                axisEJY1.AxisType.Value = EnumAxisConstants.EJY1;
                axisEJY1.Label.Value = "EJY1";
                axisEJY1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisEJY1.Param.FeedOverride.Value = 1;


                axisEJY1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisEJY1.Param.HomeInvert.Value = false;
                axisEJY1.Param.IndexInvert.Value = true;

                axisEJY1.Param.HommingSpeed.Value = 2000;
                axisEJY1.Param.HommingAcceleration.Value = 20000;
                axisEJY1.Param.HommingDecceleration.Value = 20000;
                axisEJY1.Param.FinalVelociy.Value = 0;
                axisEJY1.Param.Speed.Value = 2000;
                axisEJY1.Param.Acceleration.Value = 20000;
                axisEJY1.Param.Decceleration.Value = 20000;
                axisEJY1.Param.AccelerationJerk.Value = 200000;
                axisEJY1.Param.DeccelerationJerk.Value = 200000;
                axisEJY1.Param.SeqSpeed.Value = 2000;
                axisEJY1.Param.SeqAcc.Value = 20000;
                axisEJY1.Param.SeqDcc.Value = 20000;
                axisEJY1.Param.DtoPRatio.Value = 1;
                axisEJY1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisEJY1.Param.PosSWLimit.Value = 1000000;
                axisEJY1.Param.POTIndex.Value = 0;
                axisEJY1.Param.NOTIndex.Value = 0;
                axisEJY1.Param.HOMEIndex.Value = 1;
                axisEJY1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisEJY1.Param.HomeDistLimit.Value = 102000;
                axisEJY1.Param.IndexDistLimit.Value = 0;

                axisEJY1.Config.StopRate.Value = 0.05;
                axisEJY1.Config.EStopRate.Value = 0.025;
                axisEJY1.Config.Inposition.Value = 200;
                axisEJY1.Config.NearTargetDistance.Value = 500;
                axisEJY1.Config.VelocityTolerance.Value = 40000;
                axisEJY1.Config.SettlingTime.Value = 0;

                axisEJY1.Config.MoterConfig.AmpEnable.Value = false;
                axisEJY1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisEJY1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisEJY1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisEJY1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisEJY1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisEJY1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisEJY1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisEJY1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisEJY1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisEJY1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisEJY1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisEJY1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisEJY1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisEJY1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisEJY1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisEJY1.Config.MoterConfig.PulseAInv.Value = false;
                axisEJY1.Config.MoterConfig.PulseBInv.Value = false;

                axisEJY1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisEJY1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisEJY1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisEJY1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisEJY1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisEJY1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisEJY1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisEJY1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJY1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisEJY1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisEJY1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisEJY1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisEJY1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisEJY1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJY1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisEJY1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisEJY1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJY1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisEJY1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisEJY1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisEJY1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisEJY1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisEJY1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisEJY1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisEJY1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisEJY1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisEJY1.Config.PIDCoeff.GainProportional.Value = 450;
                axisEJY1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisEJY1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisEJY1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisEJY1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisEJY1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisEJY1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisEJY1.Config.PIDCoeff.DRate.Value = 0;
                axisEJY1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisEJY1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisEJY1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisEJY1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisEJY1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisEJY1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisEJY1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisEJY1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisEJY1);
                #endregion
                #region Axis FDT1
                ProbeAxisObject axisFDT1 = new ProbeAxisObject(0, 22, EnumAxisConstants.FDT1);
                axisFDT1.Param.IndexSearchingSpeed.Value = 1000;
                axisFDT1.Param.HomeOffset.Value = 0;
                axisFDT1.Param.HomeShift.Value = 0;
                axisFDT1.AxisType.Value = EnumAxisConstants.FDT1;
                axisFDT1.Label.Value = "FDT1";
                axisFDT1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisFDT1.Param.FeedOverride.Value = 1;


                axisFDT1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisFDT1.Param.HomeInvert.Value = false;
                axisFDT1.Param.IndexInvert.Value = true;

                axisFDT1.Param.HommingSpeed.Value = 2000;
                axisFDT1.Param.HommingAcceleration.Value = 20000;
                axisFDT1.Param.HommingDecceleration.Value = 20000;
                axisFDT1.Param.FinalVelociy.Value = 0;
                axisFDT1.Param.Speed.Value = 2000;
                axisFDT1.Param.Acceleration.Value = 20000;
                axisFDT1.Param.Decceleration.Value = 20000;
                axisFDT1.Param.AccelerationJerk.Value = 200000;
                axisFDT1.Param.DeccelerationJerk.Value = 200000;
                axisFDT1.Param.SeqSpeed.Value = 2000;
                axisFDT1.Param.SeqAcc.Value = 20000;
                axisFDT1.Param.SeqDcc.Value = 20000;
                axisFDT1.Param.DtoPRatio.Value = 1;
                axisFDT1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisFDT1.Param.PosSWLimit.Value = 1000000;
                axisFDT1.Param.POTIndex.Value = 0;
                axisFDT1.Param.NOTIndex.Value = 0;
                axisFDT1.Param.HOMEIndex.Value = 1;
                axisFDT1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisFDT1.Param.HomeDistLimit.Value = 102000;
                axisFDT1.Param.IndexDistLimit.Value = 0;

                axisFDT1.Config.StopRate.Value = 0.05;
                axisFDT1.Config.EStopRate.Value = 0.025;
                axisFDT1.Config.Inposition.Value = 200;
                axisFDT1.Config.NearTargetDistance.Value = 500;
                axisFDT1.Config.VelocityTolerance.Value = 40000;
                axisFDT1.Config.SettlingTime.Value = 0;

                axisFDT1.Config.MoterConfig.AmpEnable.Value = false;
                axisFDT1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisFDT1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisFDT1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisFDT1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisFDT1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisFDT1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisFDT1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisFDT1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisFDT1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisFDT1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisFDT1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisFDT1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisFDT1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisFDT1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisFDT1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisFDT1.Config.MoterConfig.PulseAInv.Value = false;
                axisFDT1.Config.MoterConfig.PulseBInv.Value = false;

                axisFDT1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisFDT1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisFDT1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisFDT1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisFDT1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisFDT1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisFDT1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisFDT1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisFDT1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisFDT1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisFDT1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisFDT1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisFDT1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisFDT1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisFDT1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisFDT1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisFDT1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisFDT1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisFDT1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisFDT1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisFDT1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisFDT1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisFDT1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisFDT1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisFDT1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisFDT1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisFDT1.Config.PIDCoeff.GainProportional.Value = 450;
                axisFDT1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisFDT1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisFDT1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisFDT1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisFDT1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisFDT1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisFDT1.Config.PIDCoeff.DRate.Value = 0;
                axisFDT1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisFDT1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisFDT1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisFDT1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisFDT1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisFDT1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisFDT1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisFDT1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisFDT1);
                #endregion

                #region Axis EJPZ1
                ProbeAxisObject axisEJPZ1 = new ProbeAxisObject(0, 23, EnumAxisConstants.EJPZ1);
                axisEJPZ1.Param.IndexSearchingSpeed.Value = 1000;
                axisEJPZ1.Param.HomeOffset.Value = 0;
                axisEJPZ1.Param.HomeShift.Value = 0;
                axisEJPZ1.AxisType.Value = EnumAxisConstants.EJPZ1;
                axisEJPZ1.Label.Value = "EJPZ1";
                axisEJPZ1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisEJPZ1.Param.FeedOverride.Value = 1;


                axisEJPZ1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisEJPZ1.Param.HomeInvert.Value = false;
                axisEJPZ1.Param.IndexInvert.Value = true;

                axisEJPZ1.Param.HommingSpeed.Value = 2000;
                axisEJPZ1.Param.HommingAcceleration.Value = 20000;
                axisEJPZ1.Param.HommingDecceleration.Value = 20000;
                axisEJPZ1.Param.FinalVelociy.Value = 0;
                axisEJPZ1.Param.Speed.Value = 2000;
                axisEJPZ1.Param.Acceleration.Value = 20000;
                axisEJPZ1.Param.Decceleration.Value = 20000;
                axisEJPZ1.Param.AccelerationJerk.Value = 200000;
                axisEJPZ1.Param.DeccelerationJerk.Value = 200000;
                axisEJPZ1.Param.SeqSpeed.Value = 2000;
                axisEJPZ1.Param.SeqAcc.Value = 20000;
                axisEJPZ1.Param.SeqDcc.Value = 20000;
                axisEJPZ1.Param.DtoPRatio.Value = 1;
                axisEJPZ1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisEJPZ1.Param.PosSWLimit.Value = 1000000;
                axisEJPZ1.Param.POTIndex.Value = 0;
                axisEJPZ1.Param.NOTIndex.Value = 0;
                axisEJPZ1.Param.HOMEIndex.Value = 1;
                axisEJPZ1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisEJPZ1.Param.HomeDistLimit.Value = 102000;
                axisEJPZ1.Param.IndexDistLimit.Value = 0;

                axisEJPZ1.Config.StopRate.Value = 0.05;
                axisEJPZ1.Config.EStopRate.Value = 0.025;
                axisEJPZ1.Config.Inposition.Value = 200;
                axisEJPZ1.Config.NearTargetDistance.Value = 500;
                axisEJPZ1.Config.VelocityTolerance.Value = 40000;
                axisEJPZ1.Config.SettlingTime.Value = 0;

                axisEJPZ1.Config.MoterConfig.AmpEnable.Value = false;
                axisEJPZ1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisEJPZ1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisEJPZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisEJPZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisEJPZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisEJPZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisEJPZ1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisEJPZ1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisEJPZ1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisEJPZ1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisEJPZ1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisEJPZ1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisEJPZ1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisEJPZ1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisEJPZ1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisEJPZ1.Config.MoterConfig.PulseAInv.Value = false;
                axisEJPZ1.Config.MoterConfig.PulseBInv.Value = false;

                axisEJPZ1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisEJPZ1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisEJPZ1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisEJPZ1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisEJPZ1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisEJPZ1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisEJPZ1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisEJPZ1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJPZ1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisEJPZ1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisEJPZ1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisEJPZ1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisEJPZ1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisEJPZ1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJPZ1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisEJPZ1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisEJPZ1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJPZ1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisEJPZ1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisEJPZ1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisEJPZ1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisEJPZ1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisEJPZ1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisEJPZ1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisEJPZ1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisEJPZ1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisEJPZ1.Config.PIDCoeff.GainProportional.Value = 450;
                axisEJPZ1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisEJPZ1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisEJPZ1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisEJPZ1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisEJPZ1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisEJPZ1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisEJPZ1.Config.PIDCoeff.DRate.Value = 0;
                axisEJPZ1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisEJPZ1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisEJPZ1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisEJPZ1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisEJPZ1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisEJPZ1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisEJPZ1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisEJPZ1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisEJPZ1);
                #endregion
                #region Axis EJZ1
                ProbeAxisObject axisEJZ1 = new ProbeAxisObject(0, 24, EnumAxisConstants.EJZ1);
                axisEJZ1.Param.IndexSearchingSpeed.Value = 1000;
                axisEJZ1.Param.HomeOffset.Value = 0;
                axisEJZ1.Param.HomeShift.Value = 0;
                axisEJZ1.AxisType.Value = EnumAxisConstants.EJZ1;
                axisEJZ1.Label.Value = "EJZ1";
                axisEJZ1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisEJZ1.Param.FeedOverride.Value = 1;


                axisEJZ1.HomingType.Value = HomingMethodType.RLSEDGE;
                axisEJZ1.Param.HomeInvert.Value = false;
                axisEJZ1.Param.IndexInvert.Value = true;

                axisEJZ1.Param.HommingSpeed.Value = 2000;
                axisEJZ1.Param.HommingAcceleration.Value = 20000;
                axisEJZ1.Param.HommingDecceleration.Value = 20000;
                axisEJZ1.Param.FinalVelociy.Value = 0;
                axisEJZ1.Param.Speed.Value = 2000;
                axisEJZ1.Param.Acceleration.Value = 20000;
                axisEJZ1.Param.Decceleration.Value = 20000;
                axisEJZ1.Param.AccelerationJerk.Value = 200000;
                axisEJZ1.Param.DeccelerationJerk.Value = 200000;
                axisEJZ1.Param.SeqSpeed.Value = 2000;
                axisEJZ1.Param.SeqAcc.Value = 20000;
                axisEJZ1.Param.SeqDcc.Value = 20000;
                axisEJZ1.Param.DtoPRatio.Value = 1;
                axisEJZ1.Param.NegSWLimit.Value = -1000000;  // Limit 값 크게 줘서 없는셈
                axisEJZ1.Param.PosSWLimit.Value = 1000000;
                axisEJZ1.Param.POTIndex.Value = 0;
                axisEJZ1.Param.NOTIndex.Value = 0;
                axisEJZ1.Param.HOMEIndex.Value = 1;
                axisEJZ1.Param.TimeOut.Value = 600000;    // TimeOut 값 크게 줘서 없는셈
                axisEJZ1.Param.HomeDistLimit.Value = 102000;
                axisEJZ1.Param.IndexDistLimit.Value = 0;

                axisEJZ1.Config.StopRate.Value = 0.05;
                axisEJZ1.Config.EStopRate.Value = 0.025;
                axisEJZ1.Config.Inposition.Value = 200;
                axisEJZ1.Config.NearTargetDistance.Value = 500;
                axisEJZ1.Config.VelocityTolerance.Value = 40000;
                axisEJZ1.Config.SettlingTime.Value = 0;

                axisEJZ1.Config.MoterConfig.AmpEnable.Value = false;
                axisEJZ1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisEJZ1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisEJZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisEJZ1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisEJZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisEJZ1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisEJZ1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisEJZ1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisEJZ1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisEJZ1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisEJZ1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisEJZ1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisEJZ1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisEJZ1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisEJZ1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisEJZ1.Config.MoterConfig.PulseAInv.Value = false;
                axisEJZ1.Config.MoterConfig.PulseBInv.Value = false;

                axisEJZ1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisEJZ1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisEJZ1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisEJZ1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisEJZ1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisEJZ1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisEJZ1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisEJZ1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJZ1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisEJZ1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisEJZ1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisEJZ1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisEJZ1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisEJZ1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJZ1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisEJZ1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisEJZ1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisEJZ1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisEJZ1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisEJZ1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisEJZ1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisEJZ1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisEJZ1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisEJZ1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisEJZ1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisEJZ1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisEJZ1.Config.PIDCoeff.GainProportional.Value = 450;
                axisEJZ1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisEJZ1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisEJZ1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisEJZ1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisEJZ1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisEJZ1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisEJZ1.Config.PIDCoeff.DRate.Value = 0;
                axisEJZ1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisEJZ1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisEJZ1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisEJZ1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisEJZ1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisEJZ1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisEJZ1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisEJZ1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisEJZ1);
                #endregion

                #region Axis NZD1

                ProbeAxisObject axisNZD1 = new ProbeAxisObject(0, 25, EnumAxisConstants.NZD1);
                axisNZD1.Param.IndexSearchingSpeed.Value = 300;
                axisNZD1.Param.HomeOffset.Value = 0;
                axisNZD1.Param.HomeShift.Value = 0;
                axisNZD1.AxisType.Value = EnumAxisConstants.NZD1;
                axisNZD1.Label.Value = "NZD1";
                axisNZD1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisNZD1.MasterAxis.Value = EnumAxisConstants.Z;

                axisNZD1.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisNZD1.Param.HomeInvert.Value = false;
                axisNZD1.Param.IndexInvert.Value = false;
                axisNZD1.Param.FeedOverride.Value = 1;


                axisNZD1.Param.HommingSpeed.Value = 2000;
                axisNZD1.Param.HommingAcceleration.Value = 20000;
                axisNZD1.Param.HommingDecceleration.Value = 20000;
                axisNZD1.Param.FinalVelociy.Value = 0;
                axisNZD1.Param.Speed.Value = 2000;
                axisNZD1.Param.Acceleration.Value = 20000;
                axisNZD1.Param.Decceleration.Value = 20000;
                axisNZD1.Param.AccelerationJerk.Value = 200000;
                axisNZD1.Param.DeccelerationJerk.Value = 200000;
                axisNZD1.Param.SeqAcc.Value = 0;
                axisNZD1.Param.SeqDcc.Value = 0;
                axisNZD1.Param.DtoPRatio.Value = 2.5;
                axisNZD1.Param.NegSWLimit.Value = -1000000;
                axisNZD1.Param.PosSWLimit.Value = -1000000;
                axisNZD1.Param.POTIndex.Value = 0;
                axisNZD1.Param.NOTIndex.Value = 0;
                axisNZD1.Param.HOMEIndex.Value = 1;
                axisNZD1.Param.TimeOut.Value = 600000;
                axisNZD1.Param.HomeDistLimit.Value = 49500;
                axisNZD1.Param.IndexDistLimit.Value = 7000;

                axisNZD1.Config.StopRate.Value = 0.05;
                axisNZD1.Config.EStopRate.Value = 0.025;
                axisNZD1.Config.Inposition.Value = 1118;
                axisNZD1.Config.NearTargetDistance.Value = 5590;
                axisNZD1.Config.VelocityTolerance.Value = 559241;
                axisNZD1.Config.SettlingTime.Value = 0;

                axisNZD1.Config.MoterConfig.AmpEnable.Value = true;
                axisNZD1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisNZD1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisNZD1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisNZD1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisNZD1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisNZD1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisNZD1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisNZD1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisNZD1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisNZD1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisNZD1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisNZD1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisNZD1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisNZD1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisNZD1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisNZD1.Config.MoterConfig.PulseAInv.Value = false;
                axisNZD1.Config.MoterConfig.PulseBInv.Value = false;

                axisNZD1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisNZD1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisNZD1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisNZD1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisNZD1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisNZD1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisNZD1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisNZD1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisNZD1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisNZD1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisNZD1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisNZD1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisNZD1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisNZD1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisNZD1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisNZD1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisNZD1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisNZD1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisNZD1.Config.MoterConfig.SWNegLimitTrigger.Value = -1000000;
                axisNZD1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisNZD1.Config.MoterConfig.SWPosLimitTrigger.Value = -1000000;
                axisNZD1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisNZD1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisNZD1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisNZD1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisNZD1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisNZD1.Config.PIDCoeff.GainProportional.Value = 0;
                axisNZD1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisNZD1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisNZD1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisNZD1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisNZD1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisNZD1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisNZD1.Config.PIDCoeff.DRate.Value = 0;
                axisNZD1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisNZD1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisNZD1.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisNZD1.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisNZD1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisNZD1.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisNZD1.Config.PIDCoeff.OutputOffset.Value = 0;
                axisNZD1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisNZD1);
                #endregion

                #endregion

            }
            catch
            {

            }
            return ret;
        }

        public EventCodeEnum StageAxisDefualt_MachineOPusV()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region ==> Default

                #region Axis CT

                ProbeAxisObject axiscc1 = new ProbeAxisObject(0, 0, EnumAxisConstants.CT);
                axiscc1.Param.IndexSearchingSpeed.Value = 1000;
                axiscc1.Param.HomeOffset.Value = 0;
                axiscc1.Param.HomeShift.Value = 0;
                axiscc1.AxisType.Value = EnumAxisConstants.CT;
                axiscc1.Label.Value = "CT";
                axiscc1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axiscc1.Param.FeedOverride.Value = 1;

                axiscc1.HomingType.Value = HomingMethodType.PH;
                axiscc1.Param.HomeInvert.Value = false;
                axiscc1.Param.IndexInvert.Value = true;

                axiscc1.Param.HommingSpeed.Value = 15000;
                axiscc1.Param.HommingAcceleration.Value = 150000;
                axiscc1.Param.HommingDecceleration.Value = 150000;
                axiscc1.Param.FinalVelociy.Value = 0;
                axiscc1.Param.Speed.Value = 20000;
                axiscc1.Param.Acceleration.Value = 100000;
                axiscc1.Param.Decceleration.Value = 100000;
                axiscc1.Param.AccelerationJerk.Value = 1000000;
                axiscc1.Param.DeccelerationJerk.Value = 1000000;
                axiscc1.Param.SeqAcc.Value = 0;
                axiscc1.Param.SeqDcc.Value = 0;
                axiscc1.Param.DtoPRatio.Value = 0.8;
                axiscc1.Param.NegSWLimit.Value = -500000;
                axiscc1.Param.PosSWLimit.Value = 500000;
                axiscc1.Param.POTIndex.Value = 0;
                axiscc1.Param.NOTIndex.Value = 0;
                axiscc1.Param.HOMEIndex.Value = 1;
                axiscc1.Param.TimeOut.Value = 30000;
                axiscc1.Param.HomeDistLimit.Value = 102000;
                axiscc1.Param.IndexDistLimit.Value = 0;

                axiscc1.Config.StopRate.Value = 0.05;
                axiscc1.Config.EStopRate.Value = 0.025;
                axiscc1.Config.Inposition.Value = 200;
                axiscc1.Config.NearTargetDistance.Value = 500;
                axiscc1.Config.VelocityTolerance.Value = 40000;
                axiscc1.Config.SettlingTime.Value = 0;

                axiscc1.Config.MoterConfig.AmpEnable.Value = false;
                axiscc1.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axiscc1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axiscc1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axiscc1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axiscc1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axiscc1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axiscc1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axiscc1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axiscc1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axiscc1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axiscc1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axiscc1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axiscc1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axiscc1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axiscc1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axiscc1.Config.MoterConfig.PulseAInv.Value = false;
                axiscc1.Config.MoterConfig.PulseBInv.Value = false;

                axiscc1.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axiscc1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axiscc1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axiscc1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axiscc1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axiscc1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axiscc1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axiscc1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axiscc1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axiscc1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axiscc1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axiscc1.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axiscc1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axiscc1.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axiscc1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axiscc1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axiscc1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axiscc1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axiscc1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axiscc1.Config.PIDCoeff.GainProportional.Value = 450;
                axiscc1.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axiscc1.Config.PIDCoeff.GainDerivative.Value = 2500;
                axiscc1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axiscc1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axiscc1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axiscc1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axiscc1.Config.PIDCoeff.DRate.Value = 0;
                axiscc1.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axiscc1.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axiscc1.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axiscc1.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axiscc1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axiscc1.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axiscc1.Config.PIDCoeff.OutputOffset.Value = -32768;
                axiscc1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axiscc1);

                #endregion
                #region Axis CCM

                ProbeAxisObject axisccm = new ProbeAxisObject(0, 1, EnumAxisConstants.CCM);
                axisccm.Param.IndexSearchingSpeed.Value = 1000;
                axisccm.Param.HomeOffset.Value = 0;
                axisccm.Param.HomeShift.Value = 0;
                axisccm.AxisType.Value = EnumAxisConstants.CCM;
                axisccm.Label.Value = "CCM";
                axisccm.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisccm.MasterAxis.Value = EnumAxisConstants.CCG;
                axisccm.Param.FeedOverride.Value = 1;

                axisccm.HomingType.Value = HomingMethodType.PH;
                axisccm.Param.HomeInvert.Value = false;
                axisccm.Param.IndexInvert.Value = true;

                axisccm.Param.HommingSpeed.Value = 15000;
                axisccm.Param.HommingAcceleration.Value = 150000;
                axisccm.Param.HommingDecceleration.Value = 150000;
                axisccm.Param.FinalVelociy.Value = 0;
                axisccm.Param.Speed.Value = 20000;
                axisccm.Param.Acceleration.Value = 100000;
                axisccm.Param.Decceleration.Value = 100000;
                axisccm.Param.AccelerationJerk.Value = 1000000;
                axisccm.Param.DeccelerationJerk.Value = 1000000;
                axisccm.Param.SeqAcc.Value = 0;
                axisccm.Param.SeqDcc.Value = 0;
                axisccm.Param.DtoPRatio.Value = 0.69;
                axisccm.Param.NegSWLimit.Value = -500000;
                axisccm.Param.PosSWLimit.Value = 500000;
                axisccm.Param.POTIndex.Value = 0;
                axisccm.Param.NOTIndex.Value = 0;
                axisccm.Param.HOMEIndex.Value = 1;
                axisccm.Param.TimeOut.Value = 30000;
                axisccm.Param.HomeDistLimit.Value = 102000;
                axisccm.Param.IndexDistLimit.Value = 0;

                axisccm.Config.StopRate.Value = 0.05;
                axisccm.Config.EStopRate.Value = 0.025;
                axisccm.Config.Inposition.Value = 200;
                axisccm.Config.NearTargetDistance.Value = 500;
                axisccm.Config.VelocityTolerance.Value = 40000;
                axisccm.Config.SettlingTime.Value = 0;

                axisccm.Config.MoterConfig.AmpEnable.Value = false;
                axisccm.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisccm.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisccm.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisccm.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisccm.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisccm.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisccm.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisccm.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisccm.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisccm.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisccm.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisccm.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisccm.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisccm.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisccm.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisccm.Config.MoterConfig.PulseAInv.Value = false;
                axisccm.Config.MoterConfig.PulseBInv.Value = false;

                axisccm.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisccm.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisccm.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisccm.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisccm.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisccm.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisccm.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisccm.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisccm.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisccm.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisccm.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccm.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisccm.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisccm.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisccm.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisccm.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisccm.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisccm.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisccm.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisccm.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisccm.Config.PIDCoeff.GainProportional.Value = 450;
                axisccm.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisccm.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisccm.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisccm.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisccm.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisccm.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisccm.Config.PIDCoeff.DRate.Value = 0;
                axisccm.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisccm.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisccm.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisccm.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisccm.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisccm.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisccm.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisccm.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisccm);

                #endregion
                #region Axis CCS

                ProbeAxisObject axisccs = new ProbeAxisObject(0, 2, EnumAxisConstants.CCS);
                axisccs.Param.IndexSearchingSpeed.Value = 1000;
                axisccs.Param.HomeOffset.Value = 0;
                axisccs.Param.HomeShift.Value = 0;
                axisccs.AxisType.Value = EnumAxisConstants.CCS;
                axisccs.Label.Value = "CCS";
                axisccs.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisccs.MasterAxis.Value = EnumAxisConstants.CCG;
                axisccs.Param.FeedOverride.Value = 1;


                axisccs.HomingType.Value = HomingMethodType.PH;
                axisccs.Param.HomeInvert.Value = false;
                axisccs.Param.IndexInvert.Value = true;

                axisccs.Param.HommingSpeed.Value = 15000;
                axisccs.Param.HommingAcceleration.Value = 150000;
                axisccs.Param.HommingDecceleration.Value = 150000;
                axisccs.Param.FinalVelociy.Value = 0;
                axisccs.Param.Speed.Value = 20000;
                axisccs.Param.Acceleration.Value = 100000;
                axisccs.Param.Decceleration.Value = 100000;
                axisccs.Param.AccelerationJerk.Value = 1000000;
                axisccs.Param.DeccelerationJerk.Value = 1000000;
                axisccs.Param.SeqAcc.Value = 0;
                axisccs.Param.SeqDcc.Value = 0;
                axisccs.Param.DtoPRatio.Value = 0.69;
                axisccs.Param.NegSWLimit.Value = -500000;
                axisccs.Param.PosSWLimit.Value = 500000;
                axisccs.Param.POTIndex.Value = 0;
                axisccs.Param.NOTIndex.Value = 0;
                axisccs.Param.HOMEIndex.Value = 1;
                axisccs.Param.TimeOut.Value = 30000;
                axisccs.Param.HomeDistLimit.Value = 102000;
                axisccs.Param.IndexDistLimit.Value = 0;

                axisccs.Config.StopRate.Value = 0.05;
                axisccs.Config.EStopRate.Value = 0.025;
                axisccs.Config.Inposition.Value = 200;
                axisccs.Config.NearTargetDistance.Value = 500;
                axisccs.Config.VelocityTolerance.Value = 40000;
                axisccs.Config.SettlingTime.Value = 0;

                axisccs.Config.MoterConfig.AmpEnable.Value = false;
                axisccs.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisccs.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisccs.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisccs.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisccs.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisccs.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisccs.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisccs.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisccs.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisccs.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisccs.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisccs.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisccs.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisccs.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisccs.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisccs.Config.MoterConfig.PulseAInv.Value = false;
                axisccs.Config.MoterConfig.PulseBInv.Value = false;

                axisccs.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisccs.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisccs.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisccs.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisccs.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisccs.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisccs.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisccs.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisccs.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisccs.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisccs.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisccs.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisccs.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisccs.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisccs.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisccs.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisccs.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisccs.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisccs.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisccs.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisccs.Config.PIDCoeff.GainProportional.Value = 450;
                axisccs.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisccs.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisccs.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisccs.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisccs.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisccs.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisccs.Config.PIDCoeff.DRate.Value = 0;
                axisccs.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisccs.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisccs.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisccs.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisccs.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisccs.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisccs.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisccs.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisccs);

                #endregion
                #region Axis ROT

                ProbeAxisObject axisROT = new ProbeAxisObject(0, 3, EnumAxisConstants.ROT);
                axisROT.Param.IndexSearchingSpeed.Value = 1000;
                axisROT.Param.HomeOffset.Value = 0;
                axisROT.Param.HomeShift.Value = 0;
                axisROT.AxisType.Value = EnumAxisConstants.ROT;
                axisROT.Label.Value = "ROT";
                axisROT.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisROT.Param.FeedOverride.Value = 1;


                axisROT.HomingType.Value = HomingMethodType.RLSEDGE;
                axisROT.Param.HomeInvert.Value = false;
                axisROT.Param.IndexInvert.Value = true;

                axisROT.Param.HommingSpeed.Value = 100000;
                axisROT.Param.HommingAcceleration.Value = 1000000;
                axisROT.Param.HommingDecceleration.Value = 1000000;
                axisROT.Param.FinalVelociy.Value = 0;
                axisROT.Param.Speed.Value = 150000;
                axisROT.Param.Acceleration.Value = 500000;
                axisROT.Param.Decceleration.Value = 500000;
                axisROT.Param.AccelerationJerk.Value = 5E+6;
                axisROT.Param.DeccelerationJerk.Value = 5E+6;
                axisROT.Param.SeqSpeed.Value = 150000;
                axisROT.Param.SeqAcc.Value = 500000;
                axisROT.Param.SeqDcc.Value = 500000;
                axisROT.Param.DtoPRatio.Value = 1;
                axisROT.Param.NegSWLimit.Value = -100000;
                axisROT.Param.PosSWLimit.Value = 800000;
                axisROT.Param.POTIndex.Value = 0;
                axisROT.Param.NOTIndex.Value = 0;
                axisROT.Param.HOMEIndex.Value = 1;
                axisROT.Param.TimeOut.Value = 30000;
                axisROT.Param.HomeDistLimit.Value = 102000;
                axisROT.Param.IndexDistLimit.Value = 0;

                axisROT.Config.StopRate.Value = 0.05;
                axisROT.Config.EStopRate.Value = 0.025;
                axisROT.Config.Inposition.Value = 200;
                axisROT.Config.NearTargetDistance.Value = 500;
                axisROT.Config.VelocityTolerance.Value = 40000;
                axisROT.Config.SettlingTime.Value = 0;

                axisROT.Config.MoterConfig.AmpEnable.Value = false;
                axisROT.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axisROT.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisROT.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisROT.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisROT.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisROT.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisROT.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisROT.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisROT.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisROT.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisROT.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisROT.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisROT.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisROT.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisROT.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisROT.Config.MoterConfig.PulseAInv.Value = false;
                axisROT.Config.MoterConfig.PulseBInv.Value = false;

                axisROT.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axisROT.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisROT.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisROT.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisROT.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisROT.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisROT.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisROT.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisROT.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisROT.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisROT.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisROT.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisROT.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axisROT.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisROT.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axisROT.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisROT.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisROT.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisROT.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axisROT.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisROT.Config.PIDCoeff.GainProportional.Value = 450;
                axisROT.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axisROT.Config.PIDCoeff.GainDerivative.Value = 2500;
                axisROT.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisROT.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisROT.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisROT.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisROT.Config.PIDCoeff.DRate.Value = 0;
                axisROT.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axisROT.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisROT.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axisROT.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axisROT.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisROT.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axisROT.Config.PIDCoeff.OutputOffset.Value = -32768;
                axisROT.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisROT);

                #endregion


                #region Axis X

                ProbeAxisObject axisx = new ProbeAxisObject(0, 10, EnumAxisConstants.X);
                axisx.Param.IndexSearchingSpeed.Value = 6000; //써야할것같은
                axisx.Param.HomeOffset.Value = -138121;       //써야할것같은
                                                              //axisx.Param.HomeOffset.Value = -150451;
                                                              //axisx.Param.HomeOffset.Value = -125791;
                axisx.Param.HomeShift.Value = 0;            //써야할것같은
                axisx.AxisType.Value = EnumAxisConstants.X;
                axisx.Label.Value = "X";
                axisx.SettlingTime = 0;
                axisx.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisx.Param.FeedOverride.Value = 1;


                axisx.HomingType.Value = HomingMethodType.NHPI;
                axisx.Param.HomeInvert.Value = false;
                axisx.Param.IndexInvert.Value = true;


                axisx.Param.HommingSpeed.Value = 40000;     //써야할것같은
                axisx.Param.HommingAcceleration.Value = 400000;     //써야할것같은
                axisx.Param.HommingDecceleration.Value = 400000;    //써야할것같은
                axisx.Param.FinalVelociy.Value = 0;
                axisx.Param.Speed.Value = 150000;       //써야할것같은
                axisx.Param.Acceleration.Value = 2325000;   //써야할것같은
                axisx.Param.Decceleration.Value = 2325000;  //써야할것같은
                axisx.Param.AccelerationJerk.Value = 232500000;  //써야할것같은
                axisx.Param.DeccelerationJerk.Value = 232500000;  //써야할것같은
                axisx.Param.SeqSpeed.Value = 220000;
                axisx.Param.SeqAcc.Value = 2400000;
                axisx.Param.SeqDcc.Value = 2400000;
                axisx.Param.DtoPRatio.Value = 51.2;
                axisx.Param.NegSWLimit.Value = -169923.3203125;     //써야할것같은
                axisx.Param.PosSWLimit.Value = 166000;          //써야할것같은
                axisx.Param.POTIndex.Value = 0;
                axisx.Param.NOTIndex.Value = 0;
                axisx.Param.HOMEIndex.Value = 1;
                axisx.Param.TimeOut.Value = 30000;              //써야할것같은
                axisx.Param.HomeDistLimit.Value = 329747;
                axisx.Param.IndexDistLimit.Value = 50000;

                axisx.Config.StopRate.Value = 0.05;
                axisx.Config.EStopRate.Value = 0.025;
                axisx.Config.Inposition.Value = 118;        //써야할것같은
                axisx.Config.NearTargetDistance.Value = 5590;
                axisx.Config.VelocityTolerance.Value = 559241;
                axisx.Config.SettlingTime.Value = 0;

                axisx.Config.MoterConfig.AmpEnable.Value = false;
                axisx.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisx.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisx.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisx.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisx.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisx.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisx.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisx.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisx.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisx.Config.MoterConfig.PulseAInv.Value = false;
                axisx.Config.MoterConfig.PulseBInv.Value = false;

                axisx.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisx.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisx.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisx.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisx.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisx.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisx.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisx.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisx.Config.MoterConfig.SWNegLimitTrigger.Value = -99540720.933333333333333333333333;
                axisx.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.SWPosLimitTrigger.Value = 105132896.26666666666666666666667;
                axisx.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisx.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisx.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisx.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisx.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisx.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisx.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisx.Config.PIDCoeff.GainDerivative.Value = 5.5;
                axisx.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardAcceleration.Value = 150;
                axisx.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisx.Config.PIDCoeff.DRate.Value = 0;
                axisx.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisx.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputOffset.Value = 0;
                axisx.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisx);

                #endregion
                #region AxisY

                ProbeAxisObject axisy = new ProbeAxisObject(0, 11, EnumAxisConstants.Y);
                axisy.Param.IndexSearchingSpeed.Value = 10000;
                axisy.Param.HomeOffset.Value = 311933;
                //axisy.Param.HomeOffset.Value = 305833;
                //axisy.Param.HomeOffset.Value = 318033;
                //axisy.Param.HomeOffset.Value = 304133;
                axisy.Param.HomeShift.Value = 0;
                axisy.AxisType.Value = EnumAxisConstants.Y;
                axisy.Label.Value = "Y";
                axisy.SettlingTime = 0;
                axisy.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisy.Param.FeedOverride.Value = 1;


                axisy.HomingType.Value = HomingMethodType.PHNI;
                axisy.Param.HomeInvert.Value = false;
                axisy.Param.IndexInvert.Value = true;


                axisy.Param.HommingSpeed.Value = 40000;
                axisy.Param.HommingAcceleration.Value = 400000;
                axisy.Param.HommingDecceleration.Value = 400000;
                axisy.Param.FinalVelociy.Value = 0;
                axisy.Param.Speed.Value = 150000;
                axisy.Param.Acceleration.Value = 2325000;
                axisy.Param.Decceleration.Value = 2325000;
                axisy.Param.AccelerationJerk.Value = 232500000;
                axisy.Param.DeccelerationJerk.Value = 232500000;
                axisy.Param.SeqSpeed.Value = 240000;
                axisy.Param.SeqAcc.Value = 2400000;
                axisy.Param.SeqDcc.Value = 2400000;
                axisy.Param.DtoPRatio.Value = 51.2;
                axisy.Param.NegSWLimit.Value = -199678.56046065259117082533589251;
                axisy.Param.PosSWLimit.Value = 353832.43761996161228406909788868;
                axisy.Param.POTIndex.Value = 0;
                axisy.Param.NOTIndex.Value = 0;
                axisy.Param.HOMEIndex.Value = 1;
                axisy.Param.TimeOut.Value = 30000;
                axisy.Param.HomeDistLimit.Value = 553510;
                axisy.Param.IndexDistLimit.Value = 50000;

                axisy.Config.StopRate.Value = 0.05;
                axisy.Config.EStopRate.Value = 0.025;
                axisy.Config.Inposition.Value = 118;
                axisy.Config.NearTargetDistance.Value = 5590;
                axisy.Config.VelocityTolerance.Value = 559241;
                axisy.Config.SettlingTime.Value = 0;

                axisy.Config.MoterConfig.AmpEnable.Value = false;
                axisy.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisy.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisy.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisy.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisy.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisy.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisy.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisy.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;
                axisy.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;

                axisy.Config.MoterConfig.PulseAInv.Value = false;
                axisy.Config.MoterConfig.PulseBInv.Value = false;

                axisy.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisy.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisy.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisy.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisy.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisy.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisy.Config.MoterConfig.SWNegLimitTrigger.Value = -110721705.6;
                axisy.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisy.Config.MoterConfig.SWPosLimitTrigger.Value = 334401918.93333333333333333333333;
                axisy.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisy.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisy.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisy.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisy.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisy.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisy.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisy.Config.PIDCoeff.GainDerivative.Value = 6.5;
                axisy.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardAcceleration.Value = 145;
                axisy.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisy.Config.PIDCoeff.DRate.Value = 0;
                axisy.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisy.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputOffset.Value = 0;
                axisy.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisy);

                #endregion
                #region Axis TRI

                ProbeAxisObject axistri = new ProbeAxisObject(0, 12, EnumAxisConstants.TRI);
                axistri.Param.IndexSearchingSpeed.Value = 1000;
                axistri.Param.HomeOffset.Value = 0;
                axistri.Param.HomeShift.Value = 0;
                axistri.AxisType.Value = EnumAxisConstants.TRI;
                axistri.Label.Value = "TRI";
                axistri.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axistri.Param.FeedOverride.Value = 1;


                axistri.HomingType.Value = HomingMethodType.RLSEDGE;
                axistri.Param.HomeInvert.Value = false;
                axistri.Param.IndexInvert.Value = true;

                axistri.Param.HommingSpeed.Value = 20000;
                axistri.Param.HommingAcceleration.Value = 1000000;
                axistri.Param.HommingDecceleration.Value = 1000000;
                axistri.Param.FinalVelociy.Value = 0;
                axistri.Param.Speed.Value = 150000;
                axistri.Param.Acceleration.Value = 500000;
                axistri.Param.Decceleration.Value = 500000;
                axistri.Param.AccelerationJerk.Value = 5E+6;
                axistri.Param.DeccelerationJerk.Value = 5E+6;
                axistri.Param.SeqSpeed.Value = 10000;
                axistri.Param.SeqAcc.Value = 100000;
                axistri.Param.SeqDcc.Value = 100000;
                axistri.Param.DtoPRatio.Value = 1;
                axistri.Param.NegSWLimit.Value = -100000;
                axistri.Param.PosSWLimit.Value = 800000;
                axistri.Param.POTIndex.Value = 0;
                axistri.Param.NOTIndex.Value = 0;
                axistri.Param.HOMEIndex.Value = 1;
                axistri.Param.TimeOut.Value = 30000;
                axistri.Param.HomeDistLimit.Value = 102000;
                axistri.Param.IndexDistLimit.Value = 0;

                axistri.Config.StopRate.Value = 0.05;
                axistri.Config.EStopRate.Value = 0.025;
                axistri.Config.Inposition.Value = 200;
                axistri.Config.NearTargetDistance.Value = 500;
                axistri.Config.VelocityTolerance.Value = 40000;
                axistri.Config.SettlingTime.Value = 0;

                axistri.Config.MoterConfig.AmpEnable.Value = false;
                axistri.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axistri.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axistri.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axistri.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axistri.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axistri.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axistri.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axistri.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axistri.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axistri.Config.MoterConfig.PulseAInv.Value = false;
                axistri.Config.MoterConfig.PulseBInv.Value = false;

                axistri.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axistri.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axistri.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axistri.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axistri.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axistri.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axistri.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axistri.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axistri.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axistri.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axistri.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axistri.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axistri.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axistri.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axistri.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axistri.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axistri.Config.PIDCoeff.GainProportional.Value = 450;
                axistri.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axistri.Config.PIDCoeff.GainDerivative.Value = 2500;
                axistri.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axistri.Config.PIDCoeff.DRate.Value = 0;
                axistri.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axistri.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axistri.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axistri.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axistri.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axistri.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axistri.Config.PIDCoeff.OutputOffset.Value = -32768;
                axistri.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axistri);

                #endregion
                #region Axis C

                ProbeAxisObject axist = new ProbeAxisObject(0, 13, EnumAxisConstants.C);
                axist.Param.IndexSearchingSpeed.Value = 1000;
                axist.Param.HomeOffset.Value = 50000;
                axist.Param.HomeShift.Value = 0;
                axist.AxisType.Value = EnumAxisConstants.C;
                axist.Label.Value = "T";
                axist.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axist.Param.FeedOverride.Value = 1;


                axist.HomingType.Value = HomingMethodType.PH;
                axist.Param.HomeInvert.Value = false;
                axist.Param.IndexInvert.Value = true;

                axist.Param.HommingSpeed.Value = 8000;
                axist.Param.HommingAcceleration.Value = 150000;
                axist.Param.HommingDecceleration.Value = 150000;
                axist.Param.FinalVelociy.Value = 0;
                axist.Param.Speed.Value = 15000;
                axist.Param.Acceleration.Value = 50000;
                axist.Param.Decceleration.Value = 50000;
                axist.Param.AccelerationJerk.Value = 10000000;
                axist.Param.DeccelerationJerk.Value = 10000000;
                axist.Param.SeqAcc.Value = 50000;
                axist.Param.SeqDcc.Value = 50000;
                axist.Param.SeqSpeed.Value = 15000;
                axist.Param.DtoPRatio.Value = 2.757620218;
                axist.Param.NegSWLimit.Value = -51000;
                axist.Param.PosSWLimit.Value = 51000;
                axist.Param.POTIndex.Value = 0;
                axist.Param.NOTIndex.Value = 0;
                axist.Param.HOMEIndex.Value = 1;
                axist.Param.TimeOut.Value = 30000;
                axist.Param.HomeDistLimit.Value = 102000;
                axist.Param.IndexDistLimit.Value = 0;

                axist.Config.StopRate.Value = 0.05;
                axist.Config.EStopRate.Value = 0.025;
                axist.Config.Inposition.Value = 200;
                axist.Config.NearTargetDistance.Value = 500;
                axist.Config.VelocityTolerance.Value = 40000;
                axist.Config.SettlingTime.Value = 0;

                axist.Config.MoterConfig.AmpEnable.Value = false;
                axist.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axist.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axist.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axist.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axist.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axist.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axist.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axist.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axist.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axist.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axist.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axist.Config.MoterConfig.PulseAInv.Value = false;
                axist.Config.MoterConfig.PulseBInv.Value = false;

                axist.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axist.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axist.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axist.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axist.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axist.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axist.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axist.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axist.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axist.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axist.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axist.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axist.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axist.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axist.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axist.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axist.Config.PIDCoeff.GainProportional.Value = 450;
                axist.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axist.Config.PIDCoeff.GainDerivative.Value = 2500;
                axist.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axist.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axist.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axist.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axist.Config.PIDCoeff.DRate.Value = 0;
                axist.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axist.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axist.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axist.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axist.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axist.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axist.Config.PIDCoeff.OutputOffset.Value = -32768;
                axist.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axist);

                #endregion

                #region Axis PZ

                ProbeAxisObject axispz = new ProbeAxisObject(0, 14, EnumAxisConstants.PZ);
                axispz.Param.IndexSearchingSpeed.Value = 1000;
                axispz.Param.HomeOffset.Value = -89704;
                axispz.Param.ClearedPosition.Value = -89704;
                axispz.Param.HomeShift.Value = 0;
                axispz.AxisType.Value = EnumAxisConstants.PZ;
                axispz.Label.Value = "PZ";
                axispz.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axispz.Param.FeedOverride.Value = 1;


                axispz.HomingType.Value = HomingMethodType.NHPI;
                axispz.Param.HomeInvert.Value = false;
                axispz.Param.IndexInvert.Value = true;

                axispz.Param.HommingSpeed.Value = 5000;
                axispz.Param.HommingAcceleration.Value = 50000;
                axispz.Param.HommingDecceleration.Value = 50000;
                axispz.Param.FinalVelociy.Value = 0;
                axispz.Param.Speed.Value = 80000;
                axispz.Param.Acceleration.Value = 1250000;
                axispz.Param.Decceleration.Value = 1250000;
                axispz.Param.AccelerationJerk.Value = 125000000;
                axispz.Param.DeccelerationJerk.Value = 125000000;
                axispz.Param.SeqAcc.Value = 0;
                axispz.Param.SeqDcc.Value = 0;
                axispz.Param.DtoPRatio.Value = 873.81333333333333333333333333333;
                axispz.Param.NegSWLimit.Value = -90000;
                axispz.Param.PosSWLimit.Value = -5000;
                axispz.Param.POTIndex.Value = 0;
                axispz.Param.NOTIndex.Value = 0;
                axispz.Param.HOMEIndex.Value = 1;
                axispz.Param.TimeOut.Value = 30000;
                axispz.Param.HomeDistLimit.Value = 102000;
                axispz.Param.IndexDistLimit.Value = 0;

                axispz.Config.StopRate.Value = 0.05;
                axispz.Config.EStopRate.Value = 0.025;
                axispz.Config.Inposition.Value = 200;
                axispz.Config.NearTargetDistance.Value = 500;
                axispz.Config.VelocityTolerance.Value = 40000;
                axispz.Config.SettlingTime.Value = 0;

                axispz.Config.MoterConfig.AmpEnable.Value = false;
                axispz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axispz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axispz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axispz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axispz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axispz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axispz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axispz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axispz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axispz.Config.MoterConfig.PulseAInv.Value = false;
                axispz.Config.MoterConfig.PulseBInv.Value = false;

                axispz.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axispz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axispz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axispz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axispz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axispz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axispz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axispz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axispz.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axispz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axispz.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axispz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axispz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axispz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axispz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axispz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axispz.Config.PIDCoeff.GainProportional.Value = 450;
                axispz.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axispz.Config.PIDCoeff.GainDerivative.Value = 2500;
                axispz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axispz.Config.PIDCoeff.DRate.Value = 0;
                axispz.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axispz.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axispz.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axispz.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axispz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axispz.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axispz.Config.PIDCoeff.OutputOffset.Value = -32768;
                axispz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axispz);

                #endregion

                #region Axis Z0

                ProbeAxisObject axisz0 = new ProbeAxisObject(0, 15, EnumAxisConstants.Z0);
                axisz0.Param.IndexSearchingSpeed.Value = 300;
                axisz0.Param.HomeOffset.Value = 0;
                axisz0.Param.HomeShift.Value = 0;
                axisz0.AxisType.Value = EnumAxisConstants.Z0;
                axisz0.Label.Value = "Z0";
                axisz0.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz0.MasterAxis.Value = EnumAxisConstants.Z;

                axisz0.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz0.Param.HomeInvert.Value = false;
                axisz0.Param.IndexInvert.Value = false;
                axisz0.Param.FeedOverride.Value = 1;


                axisz0.Param.HommingSpeed.Value = 5000;
                axisz0.Param.HommingAcceleration.Value = 150000;
                axisz0.Param.HommingDecceleration.Value = 150000;
                axisz0.Param.FinalVelociy.Value = 0;
                axisz0.Param.Speed.Value = 200000;
                axisz0.Param.Acceleration.Value = 2000000;
                axisz0.Param.Decceleration.Value = 2000000;
                axisz0.Param.AccelerationJerk.Value = 20000000;
                axisz0.Param.DeccelerationJerk.Value = 20000000;
                axisz0.Param.SeqAcc.Value = 0;
                axisz0.Param.SeqDcc.Value = 0;
                axisz0.Param.DtoPRatio.Value = 262.144;
                axisz0.Param.NegSWLimit.Value = -89000;
                axisz0.Param.PosSWLimit.Value = -5000;
                axisz0.Param.POTIndex.Value = 0;
                axisz0.Param.NOTIndex.Value = 0;
                axisz0.Param.HOMEIndex.Value = 1;
                axisz0.Param.TimeOut.Value = 30000;
                axisz0.Param.HomeDistLimit.Value = 49500;
                axisz0.Param.IndexDistLimit.Value = 7000;

                axisz0.Config.StopRate.Value = 0.05;
                axisz0.Config.EStopRate.Value = 0.025;
                axisz0.Config.Inposition.Value = 1118;
                axisz0.Config.NearTargetDistance.Value = 5590;
                axisz0.Config.VelocityTolerance.Value = 559241;
                axisz0.Config.SettlingTime.Value = 0;

                axisz0.Config.MoterConfig.AmpEnable.Value = true;
                axisz0.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz0.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz0.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz0.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz0.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz0.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz0.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz0.Config.MoterConfig.PulseAInv.Value = false;
                axisz0.Config.MoterConfig.PulseBInv.Value = false;

                axisz0.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz0.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz0.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz0.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz0.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz0.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz0.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz0.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz0.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz0.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz0.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz0.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz0.Config.PIDCoeff.GainProportional.Value = 0;
                axisz0.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz0.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz0.Config.PIDCoeff.DRate.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz0.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz0);

                #endregion
                #region Axis Z1

                ProbeAxisObject axisz1 = new ProbeAxisObject(0, 16, EnumAxisConstants.Z1);
                axisz1.Param.IndexSearchingSpeed.Value = 300;
                axisz1.Param.HomeOffset.Value = -1753;
                axisz1.Param.HomeShift.Value = 0;
                axisz1.AxisType.Value = EnumAxisConstants.Z1;
                axisz1.Label.Value = "Z1";
                axisz1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz1.MasterAxis.Value = EnumAxisConstants.Z;
                axisz1.Param.FeedOverride.Value = 1;


                axisz1.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz1.Param.HomeInvert.Value = false;
                axisz1.Param.IndexInvert.Value = false;


                axisz1.Param.HommingSpeed.Value = 5000;
                axisz1.Param.HommingAcceleration.Value = 150000;
                axisz1.Param.HommingDecceleration.Value = 150000;
                axisz1.Param.FinalVelociy.Value = 0;
                axisz1.Param.Speed.Value = 200000;
                axisz1.Param.Acceleration.Value = 2000000;
                axisz1.Param.Decceleration.Value = 2000000;
                axisz1.Param.AccelerationJerk.Value = 20000000;
                axisz1.Param.DeccelerationJerk.Value = 20000000;
                axisz1.Param.SeqAcc.Value = 0;
                axisz1.Param.SeqDcc.Value = 0;
                axisz1.Param.DtoPRatio.Value = 262.144;
                axisz1.Param.NegSWLimit.Value = -89000;
                axisz1.Param.PosSWLimit.Value = -5000;
                axisz1.Param.POTIndex.Value = 0;
                axisz1.Param.NOTIndex.Value = 0;
                axisz1.Param.HOMEIndex.Value = 1;
                axisz1.Param.TimeOut.Value = 30000;
                axisz1.Param.HomeDistLimit.Value = 49500;
                axisz1.Param.IndexDistLimit.Value = 7000;

                axisz1.Config.StopRate.Value = 0.05;
                axisz1.Config.EStopRate.Value = 0.025;
                axisz1.Config.Inposition.Value = 1118;
                axisz1.Config.NearTargetDistance.Value = 5590;
                axisz1.Config.VelocityTolerance.Value = 559241;
                axisz1.Config.SettlingTime.Value = 0;

                axisz1.Config.MoterConfig.AmpEnable.Value = true;
                axisz1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz1.Config.MoterConfig.PulseAInv.Value = false;
                axisz1.Config.MoterConfig.PulseBInv.Value = false;

                axisz1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz1.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz1.Config.PIDCoeff.GainProportional.Value = 0;
                axisz1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz1.Config.PIDCoeff.DRate.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz1);

                #endregion
                #region Axis Z2

                ProbeAxisObject axisz2 = new ProbeAxisObject(0, 17, EnumAxisConstants.Z2);
                axisz2.Param.IndexSearchingSpeed.Value = 300;
                axisz2.Param.HomeOffset.Value = 95;
                axisz2.Param.HomeShift.Value = 0;
                axisz2.AxisType.Value = EnumAxisConstants.Z2;
                axisz2.Label.Value = "Z2";
                axisz2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz2.MasterAxis.Value = EnumAxisConstants.Z;
                axisz2.Param.FeedOverride.Value = 1;


                axisz2.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz2.Param.HomeInvert.Value = false;
                axisz2.Param.IndexInvert.Value = false;


                axisz2.Param.HommingSpeed.Value = 5000;
                axisz2.Param.HommingAcceleration.Value = 150000;
                axisz2.Param.HommingDecceleration.Value = 150000;
                axisz2.Param.FinalVelociy.Value = 0;
                axisz2.Param.Speed.Value = 200000;
                axisz2.Param.Acceleration.Value = 2000000;
                axisz2.Param.Decceleration.Value = 2000000;
                axisz2.Param.AccelerationJerk.Value = 20000000;
                axisz2.Param.DeccelerationJerk.Value = 20000000;
                axisz2.Param.SeqAcc.Value = 0;
                axisz2.Param.SeqDcc.Value = 0;
                axisz2.Param.DtoPRatio.Value = 262.144;
                axisz2.Param.NegSWLimit.Value = -89000;
                axisz2.Param.PosSWLimit.Value = -5000;
                axisz2.Param.POTIndex.Value = 0;
                axisz2.Param.NOTIndex.Value = 0;
                axisz2.Param.HOMEIndex.Value = 1;
                axisz2.Param.TimeOut.Value = 30000;
                axisz2.Param.HomeDistLimit.Value = 49500;
                axisz2.Param.IndexDistLimit.Value = 7000;

                axisz2.Config.StopRate.Value = 0.05;
                axisz2.Config.EStopRate.Value = 0.025;
                axisz2.Config.Inposition.Value = 1118;
                axisz2.Config.NearTargetDistance.Value = 5590;
                axisz2.Config.VelocityTolerance.Value = 559241;
                axisz2.Config.SettlingTime.Value = 0;

                axisz2.Config.MoterConfig.AmpEnable.Value = true;
                axisz2.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz2.Config.MoterConfig.PulseAInv.Value = false;
                axisz2.Config.MoterConfig.PulseBInv.Value = false;

                axisz2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz2.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz2.Config.PIDCoeff.GainProportional.Value = 0;
                axisz2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz2.Config.PIDCoeff.DRate.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz2);

                #endregion

                #region Axis Z

                ProbeAxisObject axisz = new ProbeAxisObject(0, 18, EnumAxisConstants.Z);
                axisz.Param.IndexSearchingSpeed.Value = 300;
                axisz.Param.HomeOffset.Value = -82604;
                axisz.Param.ClearedPosition.Value = -82604;
                axisz.Param.HomeShift.Value = 4500;
                axisz.AxisType.Value = EnumAxisConstants.Z;
                axisz.Label.Value = "Z";
                axisz.AxisGroupType.Value = EnumAxisGroupType.GROUPAXIS;
                axisz.MasterAxis.Value = EnumAxisConstants.Undefined;
                axisz.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz.Param.HomeInvert.Value = false;
                axisz.Param.IndexInvert.Value = false;
                axisz.Param.FeedOverride.Value = 1;


                axisz.Param.HommingSpeed.Value = 1500;
                axisz.Param.HommingAcceleration.Value = 150000;
                axisz.Param.HommingDecceleration.Value = 150000;
                axisz.Param.FinalVelociy.Value = 0;
                axisz.Param.Speed.Value = 200000;
                axisz.Param.Acceleration.Value = 1250000;
                axisz.Param.Decceleration.Value = 1250000;
                axisz.Param.AccelerationJerk.Value = 125000000;
                axisz.Param.DeccelerationJerk.Value = 125000000;
                axisz.Param.SeqAcc.Value = 0;
                axisz.Param.SeqDcc.Value = 0;
                //axisz.Param.DtoPRatio.Value = 32.768;
                axisz.Param.DtoPRatio.Value = 262.144;
                axisz.Param.NegSWLimit.Value = -89000;
                axisz.Param.PosSWLimit.Value = -5000;
                axisz.Param.POTIndex.Value = 0;
                axisz.Param.NOTIndex.Value = 0;
                axisz.Param.HOMEIndex.Value = 1;
                axisz.Param.TimeOut.Value = 30000;
                axisz.Param.HomeDistLimit.Value = 4000;
                axisz.Param.IndexDistLimit.Value = 7000;

                axisz.Config.StopRate.Value = 0.05;
                axisz.Config.EStopRate.Value = 0.025;
                axisz.Config.Inposition.Value = 1118;
                axisz.Config.NearTargetDistance.Value = 5590;
                axisz.Config.VelocityTolerance.Value = 559241;
                axisz.Config.SettlingTime.Value = 0;

                axisz.Config.MoterConfig.AmpEnable.Value = true;
                axisz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz.Config.MoterConfig.PulseAInv.Value = false;
                axisz.Config.MoterConfig.PulseBInv.Value = false;

                axisz.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz.Config.PIDCoeff.GainProportional.Value = 0;
                axisz.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz.Config.PIDCoeff.DRate.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz);

                #endregion
                #region Axis CCG

                ProbeAxisObject axiscc2 = new ProbeAxisObject(0, 19, EnumAxisConstants.CCG);
                axiscc2.Param.IndexSearchingSpeed.Value = 1000;
                axiscc2.Param.HomeOffset.Value = 0;
                axiscc2.Param.HomeShift.Value = 0;
                axiscc2.AxisType.Value = EnumAxisConstants.CCG;
                axiscc2.Label.Value = "CCG";
                axiscc2.AxisGroupType.Value = EnumAxisGroupType.GROUPAXIS;
                axiscc2.Param.FeedOverride.Value = 1;

                axiscc2.HomingType.Value = HomingMethodType.PH;
                axiscc2.Param.HomeInvert.Value = false;
                axiscc2.Param.IndexInvert.Value = true;

                axiscc2.Param.HommingSpeed.Value = 15000;
                axiscc2.Param.HommingAcceleration.Value = 150000;
                axiscc2.Param.HommingDecceleration.Value = 150000;
                axiscc2.Param.FinalVelociy.Value = 0;
                axiscc2.Param.Speed.Value = 20000;
                axiscc2.Param.Acceleration.Value = 100000;
                axiscc2.Param.Decceleration.Value = 100000;
                axiscc2.Param.AccelerationJerk.Value = 1000000;
                axiscc2.Param.DeccelerationJerk.Value = 1000000;
                axiscc2.Param.SeqAcc.Value = 0;
                axiscc2.Param.SeqDcc.Value = 0;
                axiscc2.Param.DtoPRatio.Value = 0.69;
                axiscc2.Param.NegSWLimit.Value = -500000;
                axiscc2.Param.PosSWLimit.Value = 500000;
                axiscc2.Param.POTIndex.Value = 0;
                axiscc2.Param.NOTIndex.Value = 0;
                axiscc2.Param.HOMEIndex.Value = 1;
                axiscc2.Param.TimeOut.Value = 30000;
                axiscc2.Param.HomeDistLimit.Value = 102000;
                axiscc2.Param.IndexDistLimit.Value = 0;

                axiscc2.Config.StopRate.Value = 0.05;
                axiscc2.Config.EStopRate.Value = 0.025;
                axiscc2.Config.Inposition.Value = 200;
                axiscc2.Config.NearTargetDistance.Value = 500;
                axiscc2.Config.VelocityTolerance.Value = 40000;
                axiscc2.Config.SettlingTime.Value = 0;

                axiscc2.Config.MoterConfig.AmpEnable.Value = false;
                axiscc2.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axiscc2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axiscc2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axiscc2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axiscc2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axiscc2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axiscc2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axiscc2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axiscc2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axiscc2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axiscc2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axiscc2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axiscc2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axiscc2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axiscc2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axiscc2.Config.MoterConfig.PulseAInv.Value = false;
                axiscc2.Config.MoterConfig.PulseBInv.Value = false;

                axiscc2.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axiscc2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axiscc2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axiscc2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axiscc2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axiscc2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axiscc2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axiscc2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axiscc2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axiscc2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axiscc2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axiscc2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axiscc2.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axiscc2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axiscc2.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axiscc2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axiscc2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axiscc2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axiscc2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axiscc2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axiscc2.Config.PIDCoeff.GainProportional.Value = 450;
                axiscc2.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axiscc2.Config.PIDCoeff.GainDerivative.Value = 2500;
                axiscc2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axiscc2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axiscc2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axiscc2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axiscc2.Config.PIDCoeff.DRate.Value = 0;
                axiscc2.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axiscc2.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axiscc2.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axiscc2.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axiscc2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axiscc2.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axiscc2.Config.PIDCoeff.OutputOffset.Value = -32768;
                axiscc2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axiscc2);

                #endregion

                #region 예전에 쓰리팟 때문에 쓰던거 
                //#region Axis R

                //ProbeAxisObject axisr = new ProbeAxisObject(1, 1, EnumAxisConstants.R);
                //axisr.Param.IndexSearchingSpeed.Value = 1000;
                //axisr.Param.HomeOffset.Value = 0;
                //axisr.Param.HomeShift.Value = 0;
                //axisr.AxisType.Value = EnumAxisConstants.R;
                //axisr.Label.Value = "R";

                //axisr.HomingType.Value = HomingMethodType.NHPI;
                //axisr.Param.HomeInvert.Value = false;
                //axisr.Param.IndexInvert.Value = true;
                //axisr.Param.FeedOverride.Value = 1;


                //axisr.Param.HommingSpeed.Value = 2000;
                //axisr.Param.HommingAcceleration.Value = 100000;
                //axisr.Param.HommingDecceleration.Value = 100000;
                //axisr.Param.FinalVelociy.Value = 0;
                //axisr.Param.Speed.Value = 20000;
                //axisr.Param.Acceleration.Value = 250000;
                //axisr.Param.Decceleration.Value = 250000;
                //axisr.Param.AccelerationJerk.Value = 66;
                //axisr.Param.DeccelerationJerk.Value = 66;
                //axisr.Param.SeqAcc.Value = 0;
                //axisr.Param.SeqDcc.Value = 0;
                //axisr.Param.DtoPRatio.Value = 1.0;
                //axisr.Param.NegSWLimit.Value = -10000;
                //axisr.Param.PosSWLimit.Value = 80000;
                //axisr.Param.POTIndex.Value = 0;
                //axisr.Param.NOTIndex.Value = 0;
                //axisr.Param.HOMEIndex.Value = 1;
                //axisr.Param.TimeOut.Value = 30000;
                //axisr.Param.HomeDistLimit.Value = 102000;
                //axisr.Param.IndexDistLimit.Value = 0;

                //axisr.Config.StopRate.Value = 0.05;
                //axisr.Config.EStopRate.Value = 0.025;
                //axisr.Config.Inposition.Value = 200;
                //axisr.Config.NearTargetDistance.Value = 500;
                //axisr.Config.VelocityTolerance.Value = 40000;
                //axisr.Config.SettlingTime.Value = 0;

                //axisr.Config.MoterConfig.AmpEnable.Value = false;
                //axisr.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                //axisr.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                //axisr.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                //axisr.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                //axisr.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                //axisr.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                //axisr.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                //axisr.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                //axisr.Config.MoterConfig.AmpDisableDelay.Value = 0;
                //axisr.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                //axisr.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                //axisr.Config.MoterConfig.EnableStepLoopBack.Value = false;
                //axisr.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                //axisr.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                //axisr.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                //axisr.Config.MoterConfig.PulseAInv.Value = false;
                //axisr.Config.MoterConfig.PulseBInv.Value = false;

                //axisr.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                //axisr.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                //axisr.Config.MoterConfig.AmpFaultDuration.Value = 0;

                //axisr.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                //axisr.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                //axisr.Config.MoterConfig.AmpWarningDuration.Value = 0;

                //axisr.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                //axisr.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                //axisr.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                //axisr.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                //axisr.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                //axisr.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                //axisr.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                //axisr.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                //axisr.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                //axisr.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                //axisr.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                //axisr.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                //axisr.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                //axisr.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                //axisr.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                //axisr.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                //axisr.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                //axisr.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                //axisr.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                //axisr.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                //axisr.Config.PIDCoeff.GainProportional.Value = 450;
                //axisr.Config.PIDCoeff.GainIntegral.Value = 0.4;
                //axisr.Config.PIDCoeff.GainDerivative.Value = 2500;
                //axisr.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                //axisr.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                //axisr.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                //axisr.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                //axisr.Config.PIDCoeff.DRate.Value = 0;
                //axisr.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                //axisr.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                //axisr.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                //axisr.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                //axisr.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                //axisr.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                //axisr.Config.PIDCoeff.OutputOffset.Value = -32768;
                //axisr.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                //ProbeAxisProviders.Add(axisr);

                //#endregion
                //#region Axis TT

                //ProbeAxisObject axistt = new ProbeAxisObject(1, 2, EnumAxisConstants.TT);
                //axistt.Param.IndexSearchingSpeed.Value = 1000;
                //axistt.Param.HomeOffset.Value = 0;
                //axistt.Param.HomeShift.Value = 0;
                //axistt.AxisType.Value = EnumAxisConstants.TT;
                //axistt.Label.Value = "TT";

                //axistt.HomingType.Value = HomingMethodType.NHPI;
                //axistt.Param.HomeInvert.Value = false;
                //axistt.Param.IndexInvert.Value = true;
                //axistt.Param.FeedOverride.Value = 1;


                //axistt.Param.HommingSpeed.Value = 2000;
                //axistt.Param.HommingAcceleration.Value = 100000;
                //axistt.Param.HommingDecceleration.Value = 100000;
                //axistt.Param.FinalVelociy.Value = 0;
                //axistt.Param.Speed.Value = 20000;
                //axistt.Param.Acceleration.Value = 250000;
                //axistt.Param.Decceleration.Value = 250000;
                //axistt.Param.AccelerationJerk.Value = 66;
                //axistt.Param.DeccelerationJerk.Value = 66;
                //axistt.Param.SeqAcc.Value = 0;
                //axistt.Param.SeqDcc.Value = 0;
                //axistt.Param.DtoPRatio.Value = 1.0;
                //axistt.Param.NegSWLimit.Value = -10000;
                //axistt.Param.PosSWLimit.Value = 80000;
                //axistt.Param.POTIndex.Value = 0;
                //axistt.Param.NOTIndex.Value = 0;
                //axistt.Param.HOMEIndex.Value = 1;
                //axistt.Param.TimeOut.Value = 30000;
                //axistt.Param.HomeDistLimit.Value = 102000;
                //axistt.Param.IndexDistLimit.Value = 0;

                //axistt.Config.StopRate.Value = 0.05;
                //axistt.Config.EStopRate.Value = 0.025;
                //axistt.Config.Inposition.Value = 200;
                //axistt.Config.NearTargetDistance.Value = 500;
                //axistt.Config.VelocityTolerance.Value = 40000;
                //axistt.Config.SettlingTime.Value = 0;

                //axistt.Config.MoterConfig.AmpEnable.Value = false;
                //axistt.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                //axistt.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                //axistt.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                //axistt.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                //axistt.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                //axistt.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                //axistt.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                //axistt.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                //axistt.Config.MoterConfig.AmpDisableDelay.Value = 0;
                //axistt.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                //axistt.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                //axistt.Config.MoterConfig.EnableStepLoopBack.Value = false;
                //axistt.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                //axistt.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                //axistt.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                //axistt.Config.MoterConfig.PulseAInv.Value = false;
                //axistt.Config.MoterConfig.PulseBInv.Value = false;

                //axistt.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                //axistt.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                //axistt.Config.MoterConfig.AmpFaultDuration.Value = 0;

                //axistt.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                //axistt.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                //axistt.Config.MoterConfig.AmpWarningDuration.Value = 0;

                //axistt.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                //axistt.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                //axistt.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                //axistt.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                //axistt.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                //axistt.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                //axistt.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                //axistt.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                //axistt.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                //axistt.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                //axistt.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                //axistt.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                //axistt.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                //axistt.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                //axistt.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                //axistt.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                //axistt.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                //axistt.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                //axistt.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                //axistt.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                //axistt.Config.PIDCoeff.GainProportional.Value = 450;
                //axistt.Config.PIDCoeff.GainIntegral.Value = 0.4;
                //axistt.Config.PIDCoeff.GainDerivative.Value = 2500;
                //axistt.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                //axistt.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                //axistt.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                //axistt.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                //axistt.Config.PIDCoeff.DRate.Value = 0;
                //axistt.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                //axistt.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                //axistt.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                //axistt.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                //axistt.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                //axistt.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                //axistt.Config.PIDCoeff.OutputOffset.Value = -32768;
                //axistt.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                //ProbeAxisProviders.Add(axistt);

                //#endregion
                #endregion

                #endregion

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum StageAxisDefault_BSCI1()
        {
            // Emul
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                #region ==> Default

                #region Axis X

                ProbeAxisObject axisx = new ProbeAxisObject(0, 6, EnumAxisConstants.X);
                axisx.Param.IndexSearchingSpeed.Value = 6000;
                axisx.Param.HomeOffset.Value = -149121;
                axisx.Param.ClearedPosition.Value = 0;
                //axisx.Param.HomeOffset.Value = -150451;
                //axisx.Param.HomeOffset.Value = -125791;
                axisx.Param.HomeShift.Value = 0;
                axisx.AxisType.Value = EnumAxisConstants.X;
                axisx.Label.Value = "X";
                axisx.SettlingTime = 0;
                axisx.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisx.MasterAxis.Value = EnumAxisConstants.Undefined;

                axisx.HomingType.Value = HomingMethodType.NHPI;
                axisx.Param.HomeInvert.Value = false;
                axisx.Param.IndexInvert.Value = true;
                axisx.Param.FeedOverride.Value = 1;


                axisx.Param.HommingSpeed.Value = 40000;
                axisx.Param.HommingAcceleration.Value = 400000;
                axisx.Param.HommingDecceleration.Value = 400000;
                axisx.Param.FinalVelociy.Value = 0;
                axisx.Param.Speed.Value = 220000;
                axisx.Param.Acceleration.Value = 2400000;
                axisx.Param.Decceleration.Value = 2400000;
                axisx.Param.AccelerationJerk.Value = 128000000;
                axisx.Param.DeccelerationJerk.Value = 128000000;
                axisx.Param.SeqSpeed.Value = 220000;
                axisx.Param.SeqAcc.Value = 2400000;
                axisx.Param.SeqDcc.Value = 2400000;
                axisx.Param.DtoPRatio.Value = 51.2;
                axisx.Param.NegSWLimit.Value = -165000;
                axisx.Param.PosSWLimit.Value = 166000;
                axisx.Param.POTIndex.Value = 0;
                axisx.Param.NOTIndex.Value = 0;
                axisx.Param.HOMEIndex.Value = 1;
                axisx.Param.TimeOut.Value = 30000;
                axisx.Param.HomeDistLimit.Value = 329747;
                axisx.Param.IndexDistLimit.Value = 50000;

                axisx.Config.StopRate.Value = 0.05;
                axisx.Config.EStopRate.Value = 0.025;
                axisx.Config.Inposition.Value = 118;
                axisx.Config.NearTargetDistance.Value = 5590;
                axisx.Config.VelocityTolerance.Value = 559241;
                axisx.Config.SettlingTime.Value = 0;

                axisx.Config.MoterConfig.AmpEnable.Value = false;
                axisx.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisx.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisx.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisx.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisx.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisx.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisx.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisx.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisx.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisx.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisx.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axisx.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axisx.Config.MoterConfig.PulseAInv.Value = false;
                axisx.Config.MoterConfig.PulseBInv.Value = false;

                axisx.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionNONE;


                axisx.Config.MoterConfig.AmpFaultDuration.Value = 0;
                axisx.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisx.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.AmpWarningDuration.Value = 0;
                axisx.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisx.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisx.Config.MoterConfig.ErrorLimitDuration.Value = 0;
                axisx.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisx.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisx.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisx.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisx.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisx.Config.MoterConfig.SWNegLimitTrigger.Value = -99540720.933333333333333333333333;
                axisx.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisx.Config.MoterConfig.SWPosLimitTrigger.Value = 105132896.26666666666666666666667;
                axisx.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisx.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisx.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisx.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisx.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisx.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisx.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisx.Config.PIDCoeff.GainDerivative.Value = 5.5;
                axisx.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisx.Config.PIDCoeff.FeedForwardAcceleration.Value = 150;
                axisx.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisx.Config.PIDCoeff.DRate.Value = 0;
                axisx.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisx.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisx.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisx.Config.PIDCoeff.OutputOffset.Value = 0;
                axisx.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisx);

                #endregion
                #region AxisY

                ProbeAxisObject axisy = new ProbeAxisObject(0, 7, EnumAxisConstants.Y);
                axisy.Param.IndexSearchingSpeed.Value = 10000;
                axisy.Param.HomeOffset.Value = 257733;
                axisy.Param.ClearedPosition.Value = 0;
                //axisy.Param.HomeOffset.Value = 305833;
                //axisy.Param.HomeOffset.Value = 318033;
                //axisy.Param.HomeOffset.Value = 304133;
                axisy.Param.HomeShift.Value = 0;
                axisy.AxisType.Value = EnumAxisConstants.Y;
                axisy.Label.Value = "Y";
                axisy.SettlingTime = 0;
                axisy.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisy.MasterAxis.Value = EnumAxisConstants.Undefined;

                axisy.HomingType.Value = HomingMethodType.PHNI;
                axisy.Param.HomeInvert.Value = false;
                axisy.Param.IndexInvert.Value = true;
                axisy.Param.FeedOverride.Value = 1;


                axisy.Param.HommingSpeed.Value = 40000;
                axisy.Param.HommingAcceleration.Value = 400000;
                axisy.Param.HommingDecceleration.Value = 400000;
                axisy.Param.FinalVelociy.Value = 0;
                axisy.Param.Speed.Value = 240000;
                axisy.Param.Acceleration.Value = 2400000;
                axisy.Param.Decceleration.Value = 2400000;
                axisy.Param.AccelerationJerk.Value = 128000000;
                axisy.Param.DeccelerationJerk.Value = 128000000;
                axisy.Param.SeqSpeed.Value = 240000;
                axisy.Param.SeqAcc.Value = 2400000;
                axisy.Param.SeqDcc.Value = 2400000;
                axisy.Param.DtoPRatio.Value = 51.2;
                axisy.Param.NegSWLimit.Value = -199678.56046065259117082533589251;
                axisy.Param.PosSWLimit.Value = 275000;
                axisy.Param.POTIndex.Value = 0;
                axisy.Param.NOTIndex.Value = 0;
                axisy.Param.HOMEIndex.Value = 1;
                axisy.Param.TimeOut.Value = 30000;
                axisy.Param.HomeDistLimit.Value = 553510;
                axisy.Param.IndexDistLimit.Value = 50000;

                axisy.Config.StopRate.Value = 0.05;
                axisy.Config.EStopRate.Value = 0.025;
                axisy.Config.Inposition.Value = 118;
                axisy.Config.NearTargetDistance.Value = 5590;
                axisy.Config.VelocityTolerance.Value = 559241;
                axisy.Config.SettlingTime.Value = 0;

                axisy.Config.MoterConfig.AmpEnable.Value = false;
                axisy.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisy.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axisy.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisy.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisy.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisy.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisy.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisy.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisy.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisy.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisy.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;
                axisy.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeSTEP;

                axisy.Config.MoterConfig.PulseAInv.Value = false;
                axisy.Config.MoterConfig.PulseBInv.Value = false;

                axisy.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisy.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisy.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisy.Config.MoterConfig.ErrorLimitTrigger.Value = 32000;
                axisy.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisy.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisy.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisy.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisy.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisy.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionNONE;
                axisy.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisy.Config.MoterConfig.SWNegLimitTrigger.Value = -110721705.6;
                axisy.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisy.Config.MoterConfig.SWPosLimitTrigger.Value = 334401918.93333333333333333333333;
                axisy.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisy.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisy.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX_SECONDARY;
                axisy.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_SECONDARY;

                axisy.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisy.Config.PIDCoeff.GainProportional.Value = 0.25;
                axisy.Config.PIDCoeff.GainIntegral.Value = 0.005;
                axisy.Config.PIDCoeff.GainDerivative.Value = 6.5;
                axisy.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisy.Config.PIDCoeff.FeedForwardAcceleration.Value = 145;
                axisy.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisy.Config.PIDCoeff.DRate.Value = 0;
                axisy.Config.PIDCoeff.IntegrationMaxMoving.Value = 32767;
                axisy.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputLimtLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axisy.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32768;
                axisy.Config.PIDCoeff.OutputOffset.Value = 0;
                axisy.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisy);

                #endregion

                #region Axis C

                ProbeAxisObject axist = new ProbeAxisObject(0, 8, EnumAxisConstants.C);
                axist.Param.IndexSearchingSpeed.Value = 1000;
                axist.Param.HomeOffset.Value = 50000;
                axist.Param.ClearedPosition.Value = 0;
                axist.Param.HomeShift.Value = 0;
                axist.AxisType.Value = EnumAxisConstants.C;
                axist.Label.Value = "T";
                axist.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axist.MasterAxis.Value = EnumAxisConstants.Undefined;


                axist.HomingType.Value = HomingMethodType.PH;
                axist.Param.HomeInvert.Value = false;
                axist.Param.IndexInvert.Value = true;
                axist.Param.FeedOverride.Value = 1;


                axist.Param.HommingSpeed.Value = 8000;
                axist.Param.HommingAcceleration.Value = 150000;
                axist.Param.HommingDecceleration.Value = 150000;
                axist.Param.FinalVelociy.Value = 0;
                axist.Param.Speed.Value = 15000;
                axist.Param.Acceleration.Value = 50000;
                axist.Param.Decceleration.Value = 50000;
                axist.Param.AccelerationJerk.Value = 10000000;
                axist.Param.DeccelerationJerk.Value = 10000000;
                axist.Param.SeqAcc.Value = 50000;
                axist.Param.SeqDcc.Value = 50000;
                axist.Param.SeqSpeed.Value = 15000;
                axist.Param.DtoPRatio.Value = 2.757620218;
                axist.Param.NegSWLimit.Value = -51000;
                axist.Param.PosSWLimit.Value = 51000;
                axist.Param.POTIndex.Value = 0;
                axist.Param.NOTIndex.Value = 0;
                axist.Param.HOMEIndex.Value = 1;
                axist.Param.TimeOut.Value = 30000;
                axist.Param.HomeDistLimit.Value = 102000;
                axist.Param.IndexDistLimit.Value = 0;

                axist.Config.StopRate.Value = 0.05;
                axist.Config.EStopRate.Value = 0.025;
                axist.Config.Inposition.Value = 200;
                axist.Config.NearTargetDistance.Value = 500;
                axist.Config.VelocityTolerance.Value = 40000;
                axist.Config.SettlingTime.Value = 0;

                axist.Config.MoterConfig.AmpEnable.Value = false;
                axist.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axist.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axist.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axist.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axist.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axist.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axist.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axist.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axist.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axist.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axist.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axist.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axist.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axist.Config.MoterConfig.PulseAInv.Value = false;
                axist.Config.MoterConfig.PulseBInv.Value = false;

                axist.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axist.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axist.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axist.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axist.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axist.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axist.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axist.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axist.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axist.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axist.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axist.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axist.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axist.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axist.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axist.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axist.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axist.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axist.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axist.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axist.Config.PIDCoeff.GainProportional.Value = 450;
                axist.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axist.Config.PIDCoeff.GainDerivative.Value = 2500;
                axist.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axist.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axist.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axist.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axist.Config.PIDCoeff.DRate.Value = 0;
                axist.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axist.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axist.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axist.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axist.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axist.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axist.Config.PIDCoeff.OutputOffset.Value = -32768;
                axist.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axist);

                #endregion
                #region Axis TRI

                ProbeAxisObject axistri = new ProbeAxisObject(0, 9, EnumAxisConstants.TRI);
                axistri.Param.IndexSearchingSpeed.Value = 1000;
                axistri.Param.HomeOffset.Value = 0;
                axistri.Param.HomeShift.Value = 0;
                axistri.AxisType.Value = EnumAxisConstants.TRI;
                axistri.Label.Value = "TRI";
                axistri.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axistri.MasterAxis.Value = EnumAxisConstants.Undefined;

                axistri.HomingType.Value = HomingMethodType.RLSEDGE;
                axistri.Param.HomeInvert.Value = false;
                axistri.Param.IndexInvert.Value = true;
                axistri.Param.FeedOverride.Value = 1;


                axistri.Param.HommingSpeed.Value = 20000;
                axistri.Param.HommingAcceleration.Value = 1000000;
                axistri.Param.HommingDecceleration.Value = 1000000;
                axistri.Param.FinalVelociy.Value = 0;
                axistri.Param.Speed.Value = 150000;
                axistri.Param.Acceleration.Value = 500000;
                axistri.Param.Decceleration.Value = 500000;
                axistri.Param.AccelerationJerk.Value = 5000000;
                axistri.Param.DeccelerationJerk.Value = 5000000;
                axistri.Param.SeqSpeed.Value = 150000;
                axistri.Param.SeqAcc.Value = 500000;
                axistri.Param.SeqDcc.Value = 500000;
                axistri.Param.DtoPRatio.Value = 1;
                axistri.Param.NegSWLimit.Value = -100000;
                axistri.Param.PosSWLimit.Value = 800000;
                axistri.Param.POTIndex.Value = 0;
                axistri.Param.NOTIndex.Value = 0;
                axistri.Param.HOMEIndex.Value = 1;
                axistri.Param.TimeOut.Value = 30000;
                axistri.Param.HomeDistLimit.Value = 102000;
                axistri.Param.IndexDistLimit.Value = 0;

                axistri.Config.StopRate.Value = 0.05;
                axistri.Config.EStopRate.Value = 0.025;
                axistri.Config.Inposition.Value = 200;
                axistri.Config.NearTargetDistance.Value = 500;
                axistri.Config.VelocityTolerance.Value = 40000;
                axistri.Config.SettlingTime.Value = 0;

                axistri.Config.MoterConfig.AmpEnable.Value = false;
                axistri.Config.MoterConfig.MotorType.Value = EnumMoterType.STEPPER;
                axistri.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axistri.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axistri.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axistri.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axistri.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axistri.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axistri.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axistri.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axistri.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axistri.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axistri.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axistri.Config.MoterConfig.PulseAInv.Value = false;
                axistri.Config.MoterConfig.PulseBInv.Value = false;

                axistri.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axistri.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axistri.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axistri.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axistri.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axistri.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axistri.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axistri.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axistri.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axistri.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axistri.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axistri.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axistri.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axistri.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axistri.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axistri.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axistri.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axistri.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axistri.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axistri.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axistri.Config.PIDCoeff.GainProportional.Value = 450;
                axistri.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axistri.Config.PIDCoeff.GainDerivative.Value = 2500;
                axistri.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axistri.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axistri.Config.PIDCoeff.DRate.Value = 0;
                axistri.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axistri.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axistri.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axistri.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axistri.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axistri.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axistri.Config.PIDCoeff.OutputOffset.Value = -32768;
                axistri.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axistri);

                #endregion
                #region Axis PZ

                ProbeAxisObject axispz = new ProbeAxisObject(0, 10, EnumAxisConstants.PZ);
                axispz.Param.IndexSearchingSpeed.Value = 1000;
                axispz.Param.HomeOffset.Value = -91260;
                axispz.Param.ClearedPosition.Value = -91260;
                axispz.Param.HomeShift.Value = 0;
                axispz.AxisType.Value = EnumAxisConstants.PZ;
                axispz.Label.Value = "PZ";
                axispz.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axispz.MasterAxis.Value = EnumAxisConstants.Undefined;

                axispz.HomingType.Value = HomingMethodType.RLSEDGEINDEX;
                axispz.Param.HomeInvert.Value = false;
                axispz.Param.IndexInvert.Value = true;
                axispz.Param.FeedOverride.Value = 1;


                axispz.Param.HommingSpeed.Value = 5000;
                axispz.Param.HommingAcceleration.Value = 50000;
                axispz.Param.HommingDecceleration.Value = 50000;
                axispz.Param.FinalVelociy.Value = 0;
                axispz.Param.Speed.Value = 80000;
                axispz.Param.Acceleration.Value = 1250000;
                axispz.Param.Decceleration.Value = 1250000;
                axispz.Param.AccelerationJerk.Value = 125000000;
                axispz.Param.DeccelerationJerk.Value = 125000000;
                axispz.Param.SeqSpeed.Value = 80000;
                axispz.Param.SeqAcc.Value = 1250000;
                axispz.Param.SeqDcc.Value = 1250000;
                axispz.Param.DtoPRatio.Value = 873.81333333333339;
                axispz.Param.NegSWLimit.Value = -92000;
                axispz.Param.PosSWLimit.Value = -1000;
                axispz.Param.POTIndex.Value = 0;
                axispz.Param.NOTIndex.Value = 0;
                axispz.Param.HOMEIndex.Value = 1;
                axispz.Param.TimeOut.Value = 30000;
                axispz.Param.HomeDistLimit.Value = 102000;
                axispz.Param.IndexDistLimit.Value = 0;

                axispz.Config.StopRate.Value = 0.05;
                axispz.Config.EStopRate.Value = 0.025;
                axispz.Config.Inposition.Value = 200;
                axispz.Config.NearTargetDistance.Value = 500;
                axispz.Config.VelocityTolerance.Value = 40000;
                axispz.Config.SettlingTime.Value = 0;

                axispz.Config.MoterConfig.AmpEnable.Value = false;
                axispz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axispz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.CMD_ACT;

                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, false);
                axispz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, false);

                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axispz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axispz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axispz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axispz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axispz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axispz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axispz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axispz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;
                axispz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;

                axispz.Config.MoterConfig.PulseAInv.Value = false;
                axispz.Config.MoterConfig.PulseBInv.Value = false;

                axispz.Config.MoterConfig.AmpFaultTrigHigh.Value = false;
                axispz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axispz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axispz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axispz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axispz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axispz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axispz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axispz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axispz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axispz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axispz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axispz.Config.MoterConfig.SWNegLimitTrigger.Value = -116055;
                axispz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axispz.Config.MoterConfig.SWPosLimitTrigger.Value = 116055;
                axispz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axispz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axispz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axispz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInHOME;

                axispz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axispz.Config.PIDCoeff.GainProportional.Value = 450;
                axispz.Config.PIDCoeff.GainIntegral.Value = 0.4;
                axispz.Config.PIDCoeff.GainDerivative.Value = 2500;
                axispz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axispz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axispz.Config.PIDCoeff.DRate.Value = 0;
                axispz.Config.PIDCoeff.IntegrationMaxMoving.Value = 15000;
                axispz.Config.PIDCoeff.IntegrationMaxRest.Value = 32767;
                axispz.Config.PIDCoeff.OutputLimitHigh.Value = 15384;
                axispz.Config.PIDCoeff.OutputLimtLow.Value = -15384;
                axispz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 32767;
                axispz.Config.PIDCoeff.OutputVelocityLimitLow.Value = -32767;
                axispz.Config.PIDCoeff.OutputOffset.Value = -32768;
                axispz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axispz);

                #endregion

                #region Axis Z0

                ProbeAxisObject axisz0 = new ProbeAxisObject(0, 11, EnumAxisConstants.Z0);
                axisz0.Param.IndexSearchingSpeed.Value = 300;
                axisz0.Param.HomeOffset.Value = 0;
                axisz0.Param.HomeShift.Value = 0;
                axisz0.AxisType.Value = EnumAxisConstants.Z0;
                axisz0.Label.Value = "Z0";
                axisz0.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz0.MasterAxis.Value = EnumAxisConstants.Z;

                axisz0.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz0.Param.HomeInvert.Value = false;
                axisz0.Param.IndexInvert.Value = false;
                axisz0.Param.FeedOverride.Value = 1;


                axisz0.Param.HommingSpeed.Value = 5000;
                axisz0.Param.HommingAcceleration.Value = 50000;
                axisz0.Param.HommingDecceleration.Value = 50000;
                axisz0.Param.FinalVelociy.Value = 0;
                axisz0.Param.Speed.Value = 200000;
                axisz0.Param.Acceleration.Value = 2000000;
                axisz0.Param.Decceleration.Value = 2000000;
                axisz0.Param.AccelerationJerk.Value = 20000000;
                axisz0.Param.DeccelerationJerk.Value = 20000000;
                axisz0.Param.SeqSpeed.Value = 200000;
                axisz0.Param.SeqAcc.Value = 2000000;
                axisz0.Param.SeqDcc.Value = 2000000;

                axisz0.Param.DtoPRatio.Value = 2.5;
                axisz0.Param.NegSWLimit.Value = -89000;
                axisz0.Param.PosSWLimit.Value = -5000;
                axisz0.Param.POTIndex.Value = 0;
                axisz0.Param.NOTIndex.Value = 0;
                axisz0.Param.HOMEIndex.Value = 1;
                axisz0.Param.TimeOut.Value = 30000;
                axisz0.Param.HomeDistLimit.Value = 49500;
                axisz0.Param.IndexDistLimit.Value = 7000;

                axisz0.Config.StopRate.Value = 0.05;
                axisz0.Config.EStopRate.Value = 0.025;
                axisz0.Config.Inposition.Value = 1118;
                axisz0.Config.NearTargetDistance.Value = 5590;
                axisz0.Config.VelocityTolerance.Value = 559241;
                axisz0.Config.SettlingTime.Value = 0;

                axisz0.Config.MoterConfig.AmpEnable.Value = true;
                axisz0.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz0.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz0.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz0.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz0.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz0.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz0.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz0.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz0.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz0.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz0.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz0.Config.MoterConfig.PulseAInv.Value = false;
                axisz0.Config.MoterConfig.PulseBInv.Value = false;

                axisz0.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz0.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz0.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz0.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz0.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz0.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz0.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz0.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz0.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz0.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz0.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz0.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz0.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz0.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz0.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz0.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz0.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz0.Config.PIDCoeff.GainProportional.Value = 0;
                axisz0.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz0.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz0.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz0.Config.PIDCoeff.DRate.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz0.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz0.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz0.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz0.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz0);

                #endregion
                #region Axis Z1

                ProbeAxisObject axisz1 = new ProbeAxisObject(0, 12, EnumAxisConstants.Z1);
                axisz1.Param.IndexSearchingSpeed.Value = 300;
                // axisz1.Param.HomeOffset.Value = -1933.6 - 80;
                axisz1.Param.HomeOffset.Value = -1866;
                axisz1.Param.HomeShift.Value = 0;
                axisz1.AxisType.Value = EnumAxisConstants.Z1;
                axisz1.Label.Value = "Z1";
                axisz1.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz1.MasterAxis.Value = EnumAxisConstants.Z;


                axisz1.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz1.Param.HomeInvert.Value = false;
                axisz1.Param.IndexInvert.Value = false;
                axisz1.Param.FeedOverride.Value = 1;


                axisz1.Param.HommingSpeed.Value = 5000;
                axisz1.Param.HommingAcceleration.Value = 50000;
                axisz1.Param.HommingDecceleration.Value = 50000;
                axisz1.Param.FinalVelociy.Value = 0;
                axisz1.Param.Speed.Value = 200000;
                axisz1.Param.Acceleration.Value = 2000000;
                axisz1.Param.Decceleration.Value = 2000000;
                axisz1.Param.AccelerationJerk.Value = 20000000;
                axisz1.Param.DeccelerationJerk.Value = 20000000;
                axisz1.Param.SeqSpeed.Value = 200000;
                axisz1.Param.SeqAcc.Value = 2000000;
                axisz1.Param.SeqDcc.Value = 2000000;

                axisz1.Param.DtoPRatio.Value = 2.5;
                axisz1.Param.NegSWLimit.Value = -89000;
                axisz1.Param.PosSWLimit.Value = -5000;
                axisz1.Param.POTIndex.Value = 0;
                axisz1.Param.NOTIndex.Value = 0;
                axisz1.Param.HOMEIndex.Value = 1;
                axisz1.Param.TimeOut.Value = 30000;
                axisz1.Param.HomeDistLimit.Value = 49500;
                axisz1.Param.IndexDistLimit.Value = 7000;

                axisz1.Config.StopRate.Value = 0.05;
                axisz1.Config.EStopRate.Value = 0.025;
                axisz1.Config.Inposition.Value = 1118;
                axisz1.Config.NearTargetDistance.Value = 5590;
                axisz1.Config.VelocityTolerance.Value = 559241;
                axisz1.Config.SettlingTime.Value = 0;

                axisz1.Config.MoterConfig.AmpEnable.Value = true;
                axisz1.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz1.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz1.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz1.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz1.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz1.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz1.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz1.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz1.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz1.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz1.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz1.Config.MoterConfig.PulseAInv.Value = false;
                axisz1.Config.MoterConfig.PulseBInv.Value = false;

                axisz1.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz1.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz1.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz1.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz1.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz1.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz1.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz1.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz1.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz1.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz1.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz1.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz1.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz1.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz1.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz1.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz1.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz1.Config.PIDCoeff.GainProportional.Value = 0;
                axisz1.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz1.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz1.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz1.Config.PIDCoeff.DRate.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz1.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz1.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz1.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz1.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz1);

                #endregion
                #region Axis Z2

                ProbeAxisObject axisz2 = new ProbeAxisObject(0, 13, EnumAxisConstants.Z2);
                axisz2.Param.IndexSearchingSpeed.Value = 300;
                //axisz2.Param.HomeOffset.Value = -1634 + 50;
                axisz2.Param.HomeOffset.Value = -1664;
                axisz2.Param.HomeShift.Value = 0;
                axisz2.AxisType.Value = EnumAxisConstants.Z2;
                axisz2.Label.Value = "Z2";
                axisz2.AxisGroupType.Value = EnumAxisGroupType.SINGEAXIS;
                axisz2.MasterAxis.Value = EnumAxisConstants.Z;


                axisz2.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz2.Param.HomeInvert.Value = false;
                axisz2.Param.IndexInvert.Value = false;
                axisz2.Param.FeedOverride.Value = 1;


                axisz2.Param.HommingSpeed.Value = 5000;
                axisz2.Param.HommingAcceleration.Value = 50000;
                axisz2.Param.HommingDecceleration.Value = 50000;
                axisz2.Param.FinalVelociy.Value = 0;
                axisz2.Param.Speed.Value = 200000;
                axisz2.Param.Acceleration.Value = 2000000;
                axisz2.Param.Decceleration.Value = 2000000;
                axisz2.Param.AccelerationJerk.Value = 20000000;
                axisz2.Param.DeccelerationJerk.Value = 20000000;
                axisz2.Param.SeqSpeed.Value = 200000;
                axisz2.Param.SeqAcc.Value = 2000000;
                axisz2.Param.SeqDcc.Value = 2000000;
                axisz2.Param.DtoPRatio.Value = 2.5;
                axisz2.Param.NegSWLimit.Value = -89000;
                axisz2.Param.PosSWLimit.Value = -5000;
                axisz2.Param.POTIndex.Value = 0;
                axisz2.Param.NOTIndex.Value = 0;
                axisz2.Param.HOMEIndex.Value = 1;
                axisz2.Param.TimeOut.Value = 30000;
                axisz2.Param.HomeDistLimit.Value = 49500;
                axisz2.Param.IndexDistLimit.Value = 7000;

                axisz2.Config.StopRate.Value = 0.05;
                axisz2.Config.EStopRate.Value = 0.025;
                axisz2.Config.Inposition.Value = 1118;
                axisz2.Config.NearTargetDistance.Value = 5590;
                axisz2.Config.VelocityTolerance.Value = 559241;
                axisz2.Config.SettlingTime.Value = 0;

                axisz2.Config.MoterConfig.AmpEnable.Value = true;
                axisz2.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz2.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz2.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz2.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz2.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz2.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz2.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz2.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz2.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz2.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz2.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz2.Config.MoterConfig.PulseAInv.Value = false;
                axisz2.Config.MoterConfig.PulseBInv.Value = false;

                axisz2.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz2.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz2.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz2.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz2.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz2.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz2.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz2.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz2.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz2.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz2.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz2.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz2.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz2.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz2.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz2.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz2.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz2.Config.PIDCoeff.GainProportional.Value = 0;
                axisz2.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz2.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz2.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz2.Config.PIDCoeff.DRate.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz2.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz2.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz2.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz2.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz2);

                #endregion

                #region Axis Z

                ProbeAxisObject axisz = new ProbeAxisObject(0, 14, EnumAxisConstants.Z);
                axisz.Param.IndexSearchingSpeed.Value = 300;
                axisz.Param.HomeOffset.Value = -84280;
                axisz.Param.ClearedPosition.Value = -84280;
                axisz.Param.HomeShift.Value = 0;
                axisz.AxisType.Value = EnumAxisConstants.Z;
                axisz.Label.Value = "Z";
                axisz.AxisGroupType.Value = EnumAxisGroupType.GROUPAXIS;

                axisz.HomingType.Value = HomingMethodType.SYNC_NHPI;
                axisz.Param.HomeInvert.Value = false;
                axisz.Param.IndexInvert.Value = false;
                axisz.Param.FeedOverride.Value = 1;


                axisz.Param.HommingSpeed.Value = 3000;
                axisz.Param.HommingAcceleration.Value = 80000;
                axisz.Param.HommingDecceleration.Value = 80000;
                axisz.Param.FinalVelociy.Value = 0;
                axisz.Param.Speed.Value = 200000;
                axisz.Param.Acceleration.Value = 1250000;
                axisz.Param.Decceleration.Value = 1250000;
                axisz.Param.AccelerationJerk.Value = 125000000;
                axisz.Param.DeccelerationJerk.Value = 125000000;
                axisz.Param.SeqSpeed.Value = 200000;
                axisz.Param.SeqAcc.Value = 1250000;
                axisz.Param.SeqDcc.Value = 1250000;
                //axisz.Param.DtoPRatio.Value = 32.768;
                axisz.Param.DtoPRatio.Value = 2.5;
                axisz.Param.NegSWLimit.Value = -89000;
                axisz.Param.PosSWLimit.Value = -500;
                axisz.Param.POTIndex.Value = 0;
                axisz.Param.NOTIndex.Value = 0;
                axisz.Param.HOMEIndex.Value = 1;
                axisz.Param.TimeOut.Value = 30000;
                axisz.Param.HomeDistLimit.Value = 4000;
                axisz.Param.IndexDistLimit.Value = 7000;

                axisz.Config.StopRate.Value = 0.05;
                axisz.Config.EStopRate.Value = 0.025;
                axisz.Config.Inposition.Value = 1118;
                axisz.Config.NearTargetDistance.Value = 5590;
                axisz.Config.VelocityTolerance.Value = 559241;
                axisz.Config.SettlingTime.Value = 0;

                axisz.Config.MoterConfig.AmpEnable.Value = true;
                axisz.Config.MoterConfig.MotorType.Value = EnumMoterType.SERVO;
                axisz.Config.MoterConfig.AmpDisableAction.Value = EnumAmpActionType.NONE;

                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(0, true);
                axisz.Config.MoterConfig.FeedbackPhaseReverse.Value.Insert(1, true);

                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(0, false);
                axisz.Config.MoterConfig.EnableFeedbackFilter.Value.Insert(1, false);

                axisz.Config.MoterConfig.FeedbackType.Value.Insert(0, EnumFeedbackType.QUAD_AB);
                axisz.Config.MoterConfig.FeedbackType.Value.Insert(1, EnumFeedbackType.QUAD_AB);

                axisz.Config.MoterConfig.AmpDisableDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeApplyDelay.Value = 0;
                axisz.Config.MoterConfig.BrakeReleaseDelay.Value = 0;
                axisz.Config.MoterConfig.EnableStepLoopBack.Value = false;
                axisz.Config.MoterConfig.StepPulseWidth.Value = 5.6e-007;

                axisz.Config.MoterConfig.PulseAType.Value = EnumPulseType.MotorStepperPulseTypeQUADB;
                axisz.Config.MoterConfig.PulseBType.Value = EnumPulseType.MotorStepperPulseTypeQUADA;

                axisz.Config.MoterConfig.PulseAInv.Value = false;
                axisz.Config.MoterConfig.PulseBInv.Value = false;

                axisz.Config.MoterConfig.AmpFaultTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpFaultAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.AmpFaultDuration.Value = 0;

                axisz.Config.MoterConfig.AmpWarningTrigHigh.Value = true;
                axisz.Config.MoterConfig.AmpWarningAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.AmpWarningDuration.Value = 0;

                axisz.Config.MoterConfig.ErrorLimitTrigger.Value = 5000;
                axisz.Config.MoterConfig.ErrorLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.ErrorLimitDuration.Value = 0;

                axisz.Config.MoterConfig.TorqueLimitTrigger.Value = 32767;
                axisz.Config.MoterConfig.TorqueLimitAction.Value = EnumEventActionType.ActionNONE;
                axisz.Config.MoterConfig.TorqueLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWNegLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWNegLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWNegLimitDuration.Value = 0;

                axisz.Config.MoterConfig.HWPosLimitTrigHigh.Value = false;
                axisz.Config.MoterConfig.HWPosLimitAction.Value = EnumEventActionType.ActionABORT;
                axisz.Config.MoterConfig.HWPosLimitDuration.Value = 0;

                axisz.Config.MoterConfig.SWNegLimitTrigger.Value = -762316.13194271323701750721054792;
                axisz.Config.MoterConfig.SWNegLimitAction.Value = EnumEventActionType.ActionE_STOP;
                axisz.Config.MoterConfig.SWPosLimitTrigger.Value = -192991.42580828183215633093937922;
                axisz.Config.MoterConfig.SWPosLimitAction.Value = EnumEventActionType.ActionE_STOP;

                axisz.Config.InputHome.Value = EnumDedicateInputs.DedicateInputHOME;
                axisz.Config.InputIndex.Value = EnumDedicateInputs.DedicateInputINDEX;
                axisz.Config.InputMotor.Value = EnumMotorDedicatedIn.MotorDedicatedInINDEX_PRIMARY;

                axisz.Config.ControlType.Value = ControlLoopTypeEnum.PID;
                axisz.Config.PIDCoeff.GainProportional.Value = 0;
                axisz.Config.PIDCoeff.GainIntegral.Value = 0;
                axisz.Config.PIDCoeff.GainDerivative.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardPosition.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardVelocity.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardAcceleration.Value = 0;
                axisz.Config.PIDCoeff.FeedForwardFriction.Value = 0;
                axisz.Config.PIDCoeff.DRate.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxMoving.Value = 0;
                axisz.Config.PIDCoeff.IntegrationMaxRest.Value = 0;
                axisz.Config.PIDCoeff.OutputLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputLimtLow.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitHigh.Value = 0;
                axisz.Config.PIDCoeff.OutputVelocityLimitLow.Value = 0;
                axisz.Config.PIDCoeff.OutputOffset.Value = 0;
                axisz.Config.PIDCoeff.NoisePositionFFT.Value = 0;


                ProbeAxisProviders.Add(axisz);

                #endregion

                #endregion

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        /// <summary>
        /// Three Pod의 Pod간 Torque차가 비정상인지 구분을 위한 Tolerance
        /// Default Value -> Ipeak(Ampare)의 약 70% Current에 대한 Torque
        /// </summary>
        private Element<double> _ThreePodTorqueTolerance = new Element<double> { Value = 700.0 };
        public Element<double> ThreePodTorqueTolerance
        {
            get { return _ThreePodTorqueTolerance; }
            set
            {
                if (value != _ThreePodTorqueTolerance)
                {
                    _ThreePodTorqueTolerance = value;
                    RaisePropertyChanged(nameof(_ThreePodTorqueTolerance));
                }
            }
        }
        /// <summary>
        /// Three Pod의 Torque에 대한 Log의 Time interval
        /// unit : sec
        /// </summary>
        private Element<double> _MotionLoggingInterval = new Element<double> { Value = 60.0 };
        public Element<double> MotionLoggingInterval
        {
            get { return _MotionLoggingInterval; }
            set
            {
                if (value != _MotionLoggingInterval)
                {
                    _MotionLoggingInterval = value;
                    RaisePropertyChanged(nameof(_MotionLoggingInterval));
                }
            }
        }
    }

    [Serializable]
    public class HomingGroup : IParamNode
    {


        private Element<List<EnumAxisConstants>> _Stage = new Element<List<EnumAxisConstants>>();

        public Element<List<EnumAxisConstants>> Stage
        {
            get { return _Stage; }
            set { _Stage = value; }
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
        public List<object> Nodes { get; set; }
        public HomingGroup()
        {

        }
        public HomingGroup(List<EnumAxisConstants> homingaxes)
        {
            try
            {
                EnumAxisConstants[] axes;
                try
                {
                    axes = new EnumAxisConstants[homingaxes.Count];
                    homingaxes.CopyTo(axes);
                    _Stage = new Element<List<EnumAxisConstants>>();
                    _Stage.Value = new List<EnumAxisConstants>();
                    foreach (EnumAxisConstants axis in axes)
                    {
                        _Stage.Value.Add(axis);
                    }
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($string.Format("HomingStage(): Error occurred. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable]
    public class MovingGroup : IParamNode
    {


        private List<EnumAxisConstants> _Stage;

        public List<EnumAxisConstants> Stage
        {
            get { return _Stage; }
            set { _Stage = value; }
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
        public List<object> Nodes { get; set; }
        public MovingGroup()
        {

        }
        public MovingGroup(List<EnumAxisConstants> movingaxes)
        {
            try
            {
                EnumAxisConstants[] axes;
                try
                {
                    axes = new EnumAxisConstants[movingaxes.Count];
                    movingaxes.CopyTo(axes);

                    _Stage = new List<EnumAxisConstants>();
                    foreach (EnumAxisConstants axis in axes)
                    {
                        _Stage.Add(axis);
                    }
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($string.Format("HomingStage(): Error occurred. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable(), DataContract]
    public class ProbeAxisObject : AxisObject, IParamNode
    {
        public event AxisStatusUpdatedDelegate OnAxisStatusUpdated;
        public void OnStatusUpdated()
        {
            OnAxisStatusUpdated?.Invoke(this);
        }
        public ProbeAxisObject() : base()
        {

        }

        public ProbeAxisObject(int portnum, int axisindex, EnumAxisConstants type) : base(portnum, axisindex)
        {
            _AxisType.Value = type;
        }

        private Element<EnumAxisConstants> _AxisType = new Element<EnumAxisConstants>();
        [DataMember]
        public Element<EnumAxisConstants> AxisType
        {
            get { return _AxisType; }
            set { _AxisType = value; }
        }
        [NonSerialized]
        private IErrorCompensationManager _ErrorModule;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public IErrorCompensationManager ErrorModule
        {
            get { return _ErrorModule; }
            set
            {
                if (value != _ErrorModule)
                {
                    _ErrorModule = value;
                }
            }
        }
    }
}
