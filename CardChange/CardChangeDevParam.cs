using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.CardChange;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Focusing;
using ProberInterfaces.Vision;

namespace CardChange
{

    [Serializable]
    public class CardChangeDevParam : INotifyPropertyChanged, IParamNode, IDeviceParameterizable, ICardChangeDevParam
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

        public string FilePath { get; } = "CardChange";

        public string FileName { get; } = "CardChangeDeviceParam.json";

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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (CardFocusParam == null)
            {
                CardFocusParam = new NormalFocusParameter();
            }
            retval = this.FocusManager().ValidationFocusParam(CardFocusParam);
            if (retval != EventCodeEnum.NONE)
            {
                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, CardFocusParam, 200);
            }

            if (PogoFocusParam == null)
            {
                PogoFocusParam = new NormalFocusParameter();
            }
            retval = this.FocusManager().ValidationFocusParam(PogoFocusParam);
            if (retval != EventCodeEnum.NONE)
            {
                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.PIN_HIGH_CAM, EnumAxisConstants.PZ, PogoFocusParam, 400);
            }

            retval = EventCodeEnum.NONE;

            return retval;
        }
        public void SetElementMetaData()
        {

        }


        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CardCahngerAttached.Value = false;
                CLAMPLock_Timeout.Value = 60000;
                TPLock_Timeout.Value = 60000;
                ZIFLock_Timeout.Value = 60000;
                FrontDoorOpenSensorAttached.Value = false;
                BlZIFoutputMode.Value = false;
                ZIFSEQTYPE.Value = 0;
                CLPLKSEQTYPE.Value = 0;
                ManipulatorType.Value = EnumManipulatorType.NO_MANIPULATOR;
                AddedPCDetectEXSensorOnPlate.Value = false;
                ControllerType.Value = 0;
                BlTubSynqNetControl.Value = false;
                SACCOffSetY.Value = 0;

                GP_CardContactPosX = 0.0;
                GP_CardContactPosY = 0.0;
                GP_CardContactPosT = 0.0;
                GP_CardContactPosZ = -75000.0;

               

                GP_CardPatternWidth = 106;
                GP_CardPatternHeight = 106;
                GP_PogoPatternWidth = 236;
                GP_PogoPatternHeight = 236;

                GP_PogoCenterDiffX = 0;
                GP_PogoCenterDiffY = 0;
                GP_PogoDegreeDiff = 0;


                ModelInfos = new List<MFParameter>();
                MFParameter mF = new MFParameter();
                mF.ModelTargetType.Value = EnumModelTargetType.Rectangle;
                mF.ModelWidth.Value = 95 * 5.28;
                mF.ModelHeight.Value = 95 * 5.28;
                mF.ForegroundType.Value = EnumForegroundType.ANY;
                mF.ScaleMin.Value = 0.95;
                mF.ScaleMax.Value = 1.05;
                mF.Acceptance.Value = 80;
                mF.Certainty.Value = 95;
                mF.Lights = new List<LightChannelType>();
                mF.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 100));
                mF.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));

                
                mF.Child = new MFParameter();
                mF.Child.ModelTargetType.Value = EnumModelTargetType.Circle;
                mF.Child.ModelWidth.Value = 20 * 5.28; // px * WL ratio
                mF.Child.ModelHeight.Value = 20 * 5.28;
                mF.Child.ForegroundType.Value = EnumForegroundType.ANY;
                mF.Child.ScaleMin.Value = 0.95;
                mF.Child.ScaleMax.Value = 1.05;
                mF.Child.Acceptance.Value = 80;
                mF.Child.Certainty.Value = 95;
                mF.Child.Lights = new List<LightChannelType>();
                mF.Child.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 100));
                mF.Child.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));

                ModelInfos.Add(mF);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }

        private Element<double> _CCSVelScale = new Element<double>();
        public Element<double> CCSVelScale
        {
            get { return _CCSVelScale; }
            set
            {
                if (value != _CCSVelScale)
                {
                    _CCSVelScale = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _CardCahngerAttached = new Element<bool>();
        public Element<bool> CardCahngerAttached
        {
            get { return _CardCahngerAttached; }
            set
            {
                if (value != _CardCahngerAttached)
                {
                    _CardCahngerAttached = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CCSAccScale = new Element<double>();
        public Element<double> CCSAccScale
        {
            get { return _CCSAccScale; }
            set
            {
                if (value != _CCSAccScale)
                {
                    _CCSAccScale = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CCStageBackPos = new CatCoordinates();
        public CatCoordinates CCStageBackPos
        {
            get { return _CCStageBackPos; }
            set
            {
                if (value != _CCStageBackPos)
                {
                    _CCStageBackPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CCStageFrontDoorPos = new CatCoordinates();
        public CatCoordinates CCStageFrontDoor
        {
            get { return _CCStageFrontDoorPos; }
            set
            {
                if (value != _CCStageFrontDoorPos)
                {
                    _CCStageFrontDoorPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CCStageCarrierPos = new CatCoordinates();
        public CatCoordinates CCStageCarrierPos
        {
            get { return _CCStageCarrierPos; }
            set
            {
                if (value != _CCStageCarrierPos)
                {
                    _CCStageCarrierPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _CLAMPLock_Timeout = new Element<long>();
        public Element<long> CLAMPLock_Timeout
        {
            get { return _CLAMPLock_Timeout; }
            set
            {
                if (value != _CLAMPLock_Timeout)
                {
                    _CLAMPLock_Timeout = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _TPLock_Timeout = new Element<long>();
        public Element<long> TPLock_Timeout
        {
            get { return _TPLock_Timeout; }
            set
            {
                if (value != _TPLock_Timeout)
                {
                    _TPLock_Timeout = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<long> _ZIFLock_Timeout = new Element<long>();
        public Element<long> ZIFLock_Timeout
        {
            get { return _ZIFLock_Timeout; }
            set
            {
                if (value != _ZIFLock_Timeout)
                {
                    _ZIFLock_Timeout = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FrontDoorOpenSensorAttached = new Element<bool>();
        public Element<bool> FrontDoorOpenSensorAttached
        {
            get { return _FrontDoorOpenSensorAttached; }
            set
            {
                if (value != _FrontDoorOpenSensorAttached)
                {
                    _FrontDoorOpenSensorAttached = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _BlZIFoutputMode = new Element<bool>();
        public Element<bool> BlZIFoutputMode
        {
            get { return _BlZIFoutputMode; }
            set
            {
                if (value != _BlZIFoutputMode)
                {
                    _BlZIFoutputMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ZIFSEQTYPE = new Element<int>();
        public Element<int> ZIFSEQTYPE
        {
            get { return _ZIFSEQTYPE; }
            set
            {
                if (value != _ZIFSEQTYPE)
                {
                    _ZIFSEQTYPE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _CLPLKSEQTYPE = new Element<int>();
        public Element<int> CLPLKSEQTYPE
        {
            get { return _CLPLKSEQTYPE; }
            set
            {
                if (value != _CLPLKSEQTYPE)
                {
                    _CLPLKSEQTYPE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumManipulatorType> _ManipulatorType = new Element<EnumManipulatorType>();
        public Element<EnumManipulatorType> ManipulatorType
        {
            get { return _ManipulatorType; }
            set
            {
                if (value != _ManipulatorType)
                {
                    _ManipulatorType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _AddedPCDetectEXSensorOnPlate = new Element<bool>();
        public Element<bool> AddedPCDetectEXSensorOnPlate
        {
            get { return _AddedPCDetectEXSensorOnPlate; }
            set
            {
                if (value != _AddedPCDetectEXSensorOnPlate)
                {
                    _AddedPCDetectEXSensorOnPlate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ControllerType = new Element<int>();
        public Element<int> ControllerType
        {
            get { return _ControllerType; }
            set
            {
                if (value != _ControllerType)
                {
                    _ControllerType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _BlTubSynqNetControl = new Element<bool>();
        public Element<bool> BlTubSynqNetControl
        {
            get { return _BlTubSynqNetControl; }
            set
            {
                if (value != _BlTubSynqNetControl)
                {
                    _BlTubSynqNetControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SACCOffSetY = new Element<double>();
        public Element<double> SACCOffSetY
        {
            get { return _SACCOffSetY; }
            set
            {
                if (value != _SACCOffSetY)
                {
                    _SACCOffSetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> Machine

        #region ==> GP_CardContactPosX
        private double _GP_CardContactPosX;
        public double GP_CardContactPosX
        {
            get { return _GP_CardContactPosX; }
            set
            {
                if (value != _GP_CardContactPosX)
                {
                    _GP_CardContactPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_CardContactPosY
        private double _GP_CardContactPosY;
        public double GP_CardContactPosY
        {
            get { return _GP_CardContactPosY; }
            set
            {
                if (value != _GP_CardContactPosY)
                {
                    _GP_CardContactPosY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_CardContactPosT
        private double _GP_CardContactPosT;
        public double GP_CardContactPosT
        {
            get { return _GP_CardContactPosT; }
            set
            {
                if (value != _GP_CardContactPosT)
                {
                    _GP_CardContactPosT = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_CardContactPosZ
        private double _GP_CardContactPosZ;
        public double GP_CardContactPosZ
        {
            get { return _GP_CardContactPosZ; }
            set
            {
                if (value != _GP_CardContactPosZ)
                {
                    _GP_CardContactPosZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        private double _CardPodPatternWidth;
        public double CardPodPatternWidth
        {
            get { return _CardPodPatternWidth; }
            set
            {
                if (value != _CardPodPatternWidth)
                {
                    _CardPodPatternWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CardPodPatternHeight;
        public double CardPodPatternHeight
        {
            get { return _CardPodPatternHeight; }
            set
            {
                if (value != _CardPodPatternHeight)
                {
                    _CardPodPatternHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> GP_CardPatternWidth
        private double _GP_CardPatternWidth;
        public double GP_CardPatternWidth
        {
            get { return _GP_CardPatternWidth; }
            set
            {
                if (value != _GP_CardPatternWidth)
                {
                    _GP_CardPatternWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_CardPatternHeight
        private double _GP_CardPatternHeight;
        public double GP_CardPatternHeight
        {
            get { return _GP_CardPatternHeight; }
            set
            {
                if (value != _GP_CardPatternHeight)
                {
                    _GP_CardPatternHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_PogoPatternWidth
        private double _GP_PogoPatternWidth;
        public double GP_PogoPatternWidth
        {
            get { return _GP_PogoPatternWidth; }
            set
            {
                if (value != _GP_PogoPatternWidth)
                {
                    _GP_PogoPatternWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_PogoPatternHeight
        private double _GP_PogoPatternHeight;
        public double GP_PogoPatternHeight
        {
            get { return _GP_PogoPatternHeight; }
            set
            {
                if (value != _GP_PogoPatternHeight)
                {
                    _GP_PogoPatternHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_PogoCenterDiffX
        private double _GP_PogoCenterDiffX;
        public double GP_PogoCenterDiffX
        {
            get { return _GP_PogoCenterDiffX; }
            set
            {
                if (value != _GP_PogoCenterDiffX)
                {
                    _GP_PogoCenterDiffX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_PogoCenterDiffY
        private double _GP_PogoCenterDiffY;
        public double GP_PogoCenterDiffY
        {
            get { return _GP_PogoCenterDiffY; }
            set
            {
                if (value != _GP_PogoCenterDiffY)
                {
                    _GP_PogoCenterDiffY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_PogoCenterDiffZ
        private double _GP_PogoCenterDiffZ;
        public double GP_PogoDegreeDiff
        {
            get { return _GP_PogoCenterDiffZ; }
            set
            {
                if (value != _GP_PogoCenterDiffZ)
                {
                    _GP_PogoCenterDiffZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

       

        

        

        #region ==> GP_CARD FocusParameter
        private FocusParameter _CardFocusParam ;
        public FocusParameter CardFocusParam
        {
            get { return _CardFocusParam; }
            set
            {
                if (value != _CardFocusParam)
                {
                    _CardFocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> GP_POGO FocusParameter
        private FocusParameter _PogoFocusParam;
        public FocusParameter PogoFocusParam
        {
            get { return _PogoFocusParam; }
            set
            {
                if (value != _PogoFocusParam)
                {
                    _PogoFocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        private List<MFParameter> _ModelInfos;
        public List<MFParameter> ModelInfos
        {
            get { return _ModelInfos; }
            set
            {
                if (value != _ModelInfos)
                {
                    _ModelInfos = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
