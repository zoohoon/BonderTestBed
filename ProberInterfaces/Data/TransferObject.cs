using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using LogModule;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProberInterfaces.Param;
using ProberInterfaces.WaferAlign;
using ProberInterfaces.Enum;
using ProberErrorCode;

namespace ProberInterfaces
{
    /// <summary>
    /// Transfer Object를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class TransferObject : INotifyPropertyChanged, ICloneable, IParamNode
    {
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<string> _ID = new Element<string>();
        /// <summary>
        /// ID 를 가져오거나 설정합니다No
        /// </summary>
        [DataMember]
        public Element<string> ID
        {
            get { return _ID; }
            set { _ID = value; RaisePropertyChanged(); }
        }

        private Element<string> _DeviceName = new Element<string>() { Value = "DEFAULTDEVNAME" };
        /// <summary>
        /// DeviceName 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value; RaisePropertyChanged(); }
        }


        private Element<string> _ProbeCardID = new Element<string>();

        public Element<string> ProbeCardID
        {
            get { return _ProbeCardID; }
            set { _ProbeCardID = value; RaisePropertyChanged(); }
        }

        #region => Device Info
        private Element<SubstrateTypeEnum> _Type = new Element<SubstrateTypeEnum>();
        /// <summary>
        /// Type 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateTypeEnum> Type
        {
            get { return _Type; }
            set { _Type = value; RaisePropertyChanged(); }
        }

        private Element<WaferSubstrateTypeEnum> _WaferSubstrateType = new Element<WaferSubstrateTypeEnum>();
        /// <summary>
        /// Type 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<WaferSubstrateTypeEnum> WaferSubstrateType
        {
            get { return _WaferSubstrateType; }
            set { _WaferSubstrateType = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _Size = new Element<SubstrateSizeEnum>();
        /// <summary>
        /// Size 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<SubstrateSizeEnum> Size
        {
            get { return _Size; }
            set { _Size = value; RaisePropertyChanged(); }
        }

        private Element<EnumWaferType> _WaferType = new Element<EnumWaferType>();
        /// <summary>
        /// WaferType 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<EnumWaferType> WaferType
        {
            get { return _WaferType; }
            set { _WaferType = value; RaisePropertyChanged(); }
        }

        private Element<OCRTypeEnum> _OCRType = new Element<OCRTypeEnum>();
        /// <summary>
        /// OCRType 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<OCRTypeEnum> OCRType
        {
            get { return _OCRType; }
            set { _OCRType = value; RaisePropertyChanged(); }
        }

        private Element<OCRModeEnum> _OCRMode = new Element<OCRModeEnum>();
        /// <summary>
        /// OCRMode 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<OCRModeEnum> OCRMode
        {
            get { return _OCRMode; }
            set { _OCRMode = value; RaisePropertyChanged(); }
        }

        private Element<OCRDirectionEnum> _OCRDirection = new Element<OCRDirectionEnum>();
        /// <summary>
        /// OCRDirection 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<OCRDirectionEnum> OCRDirection
        {
            get { return _OCRDirection; }
            set { _OCRDirection = value; RaisePropertyChanged(); }
        }
        #endregion

        #region => Wafer State
        private EnumWaferState _WaferState;
        /// <summary>
        /// WaferState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public EnumWaferState WaferState
        {
            get { return _WaferState; }
            set { _WaferState = value; RaisePropertyChanged(); }
        }
        #endregion

        #region => ProcessCellIndex
        private int _ProcessCellIndex;
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int ProcessCellIndex
        {
            get { return _ProcessCellIndex; }
            set { _ProcessCellIndex = value; RaisePropertyChanged(); }
        }
        #endregion

        #region => Reservation State
        private ReservationStateEnum _ReservationState;
        /// <summary>
        /// Reservation State 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ReservationStateEnum ReservationState
        {
            get { return _ReservationState; }
            set { _ReservationState = value; RaisePropertyChanged(); }
        }
        #endregion

        #region => PreAlign State
        private ModuleID _UsedPA = new ModuleID();
        /// <summary>
        /// UsedPA 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID UsedPA
        {
            get { return _UsedPA; }
            set { _UsedPA = value; RaisePropertyChanged(); }
        }

        private PreAlignStateEnum _PreAlignState;
        /// <summary>
        /// PreAlignState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public PreAlignStateEnum PreAlignState
        {
            get { return _PreAlignState; }
            set { _PreAlignState = value; RaisePropertyChanged(); }
        }

        private Element<double> _NotchAngle = new Element<double>();
        /// <summary>
        /// Current NotchAngle 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> NotchAngle
        {
            get { return _NotchAngle; }
            set { _NotchAngle = value; RaisePropertyChanged(); }
        }


        private Element<double> _SlotNotchAngle = new Element<double>();
        /// <summary>
        /// Slot NotchAngle 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> SlotNotchAngle
        {
            get { return _SlotNotchAngle; }
            set { _SlotNotchAngle = value; RaisePropertyChanged(); }
        }



        private Element<double> _ChuckNotchAngle = new Element<double>();
        /// <summary>
        /// Chuck NotchAngle 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> ChuckNotchAngle
        {
            get { return _ChuckNotchAngle; }
            set { _ChuckNotchAngle = value; RaisePropertyChanged(); }
        }
        #endregion

        #region => OCR State
        private OCRReadStateEnum _OCRReadState;
        /// <summary>
        /// OCRReadState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public OCRReadStateEnum OCRReadState
        {
            get { return _OCRReadState; }
            set { _OCRReadState = value; RaisePropertyChanged(); }
        }

        private Element<string> _OCR = new Element<string>();
        /// <summary>
        /// OCR 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> OCR
        {
            get { return _OCR; }
            set { _OCR = value; RaisePropertyChanged(); }
        }

        private Element<double> _OCRReadScore = new Element<double>();
        /// <summary>
        /// OCRReadScore 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OCRReadScore
        {
            get { return _OCRReadScore; }
            set { _OCRReadScore = value; RaisePropertyChanged(); }
        }
        private Element<double> _OCRAngle = new Element<double>();
        /// <summary>
        /// OCR NotchAngle 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> OCRAngle
        {
            get { return _OCRAngle; }
            set { _OCRAngle = value; RaisePropertyChanged(); }
        }
        #endregion

        #region => Transfer Info

        private ModuleID _OriginHolder = new ModuleID();
        /// <summary>
        /// OriginHolder 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID OriginHolder
        {
            get { return _OriginHolder; }
            set { _OriginHolder = value; RaisePropertyChanged(); }
        }

        private ModuleID _PrevHolder = new ModuleID();
        /// <summary>
        /// PrevHolder 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID PrevHolder
        {
            get { return _PrevHolder; }
            set { _PrevHolder = value; RaisePropertyChanged(); }
        }

        private ModuleID _CurrHolder = new ModuleID();
        /// <summary>
        /// CurrHolder 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID CurrHolder
        {
            get { return _CurrHolder; }
            set { _CurrHolder = value; RaisePropertyChanged(); }
        }

        private ModuleID _PrevPos = new ModuleID();
        /// <summary>
        /// PrevPos 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID PrevPos
        {
            get { return _PrevPos; }
            set { _PrevPos = value; RaisePropertyChanged(); }
        }

        private ModuleID _CurrPos = new ModuleID();
        /// <summary>
        /// CurrPos 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID CurrPos
        {
            get { return _CurrPos; }
            set { _CurrPos = value; RaisePropertyChanged(); }
        }
        private ModuleID _DstPos = new ModuleID();
        /// <summary>
        /// CurrPos 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID DstPos
        {
            get { return _DstPos; }
            set { _DstPos = value; RaisePropertyChanged(); }
        }


        private int _UnloadFoupNumber;
        /// <summary>
        /// CurrPos 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public int UnloadFoupNumber
        {
            get { return _UnloadFoupNumber; }
            set { _UnloadFoupNumber = value; RaisePropertyChanged(); }
        }

        private ModuleID _UnloadHolder = new ModuleID();
        /// <summary>
        /// OriginHolder 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID UnloadHolder
        {
            get { return _UnloadHolder; }
            set { _UnloadHolder = value; RaisePropertyChanged(); }
        }
        #endregion        
       
        #region => Process Options
        private LoadNotchAngleOption _OverrideLoadNotchAngleOption;
        /// <summary>
        /// OverrideLoadNotchAngleOption 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public LoadNotchAngleOption OverrideLoadNotchAngleOption
        {
            get { return _OverrideLoadNotchAngleOption; }
            set { _OverrideLoadNotchAngleOption = value; RaisePropertyChanged(); }
        }

        private OCRDeviceOption _OverrideOCRDeviceOption;
        /// <summary>
        /// OverrideOCRDeviceOption 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public OCRDeviceOption OverrideOCRDeviceOption
        {
            get { return _OverrideOCRDeviceOption; }
            set { _OverrideOCRDeviceOption = value; RaisePropertyChanged(); }
        }

        private OCRPerformOption _OverrideOCROption;
        /// <summary>
        /// OverrideOCROption 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public OCRPerformOption OverrideOCROption
        {
            get { return _OverrideOCROption; }
            set { _OverrideOCROption = value; RaisePropertyChanged(); }
        }


        private PolishWaferInformation _PolishWaferInfo;
        /// <summary>
        /// OverrideOCROption 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public PolishWaferInformation PolishWaferInfo
        {
            get { return _PolishWaferInfo; }
            set { _PolishWaferInfo = value; RaisePropertyChanged(); }
        }

        
        //bool _RemoveCheckSum = false;
        //[XmlIgnore, JsonIgnore]
        //public bool RemoveCheckSum
        //{
        //    get { return _RemoveCheckSum; }
        //    set { _RemoveCheckSum = value; RaisePropertyChanged(); }
        //}

        //bool _ReplaceDashToDot = false;
        //[XmlIgnore, JsonIgnore]
        //public bool ReplaceDashToDot
        //{
        //    get { return _ReplaceDashToDot; }
        //    set { _ReplaceDashToDot = value; RaisePropertyChanged(); }
        //}

        #endregion


        private DateTime _ReservationTime = new DateTime();
        [DataMember]
        public DateTime ReservationTime
        {
            get { return _ReservationTime; }
            set { _ReservationTime = value; RaisePropertyChanged(); }
        }

        private int _Priority;
        [DataMember]
        public int Priority
        {
            get { return _Priority; }
            set { _Priority = value; RaisePropertyChanged(); }
        }
        #region => ProcessingEnableEnum State
        private ProcessingEnableEnum _ProcessingEnable;
        /// <summary>
        /// Reservation State 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ProcessingEnableEnum ProcessingEnable
        {
            get { return _ProcessingEnable; }
            set { _ProcessingEnable = value; RaisePropertyChanged(); }
        }
        #endregion

        #region => Using StageList
        private List<int> _UsingStageList = new List<int>();
        /// <summary>
        /// Reservation State 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public List<int> UsingStageList
        {
            get { return _UsingStageList; }
            set { _UsingStageList = value; RaisePropertyChanged(); }
        }
        #endregion
        //Card Skip Enum (Transfer가 카드 일 경우 스테이지에 올릴때 Docking까지 할것인가 스테이지한테만 줄것인가 보는 Enum)
        private CardSkipEnum _CardSkip;
        /// <summary>
        /// CardSkip 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public CardSkipEnum CardSkip
        {
            get { return _CardSkip; }
            set { _CardSkip = value; RaisePropertyChanged(); }
        }

        private int _LotPriority = 999;
        [DataMember]
        public int LotPriority
        {
            get { return _LotPriority; }
            set { _LotPriority = value; RaisePropertyChanged(); }
        }


        private PMIRemoteTriggerEnum _PMITirgger;
        /// <summary>
        /// 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public PMIRemoteTriggerEnum PMITirgger
        {
            get { return _PMITirgger; }
            set { _PMITirgger = value; RaisePropertyChanged(); }
        }
        private bool _DoPMIFlag;
        /// <summary>
        /// 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public bool DoPMIFlag
        {
            get { return _DoPMIFlag; }
            set { _DoPMIFlag = value; RaisePropertyChanged(); }
        }

        private string _LOTID = "";
        /// <summary>
        /// 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string LOTID
        {
            get { return _LOTID; }
            set { _LOTID = value; RaisePropertyChanged(); }
        }

        private OCRDevParameter _OCRDevParam;//TODO: TransferObject에 PW OCRConfig 값을 어떻게 넣어 줄 지:
                                             //Wafer Change Trigger에 따라서 Rcmd를 받았을 때 
                                             //Target이 Polish wafer Source이면 PolishInfo의 OCR DevParam을 꺼내와서 넣어준다. 
                                             // 만약 Load Target이 Polish wafer Source로 되어있지 않을 경우 Default Device 로 동작한다.
        /// <summary>
        /// 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public OCRDevParameter OCRDevParam
        {
            get { return _OCRDevParam; }
            set { _OCRDevParam = value; RaisePropertyChanged(); }
        }
        private WaferNotchTypeEnum _NotchType;
        /// <summary>
        /// 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public WaferNotchTypeEnum NotchType
        {
            get { return _NotchType; }
            set { _NotchType = value; RaisePropertyChanged(); }
        }


        private bool _WFWaitFlag = true;
        /// <summary>
        /// Wafer Id Comfirm 을 받을때 까지 기다렸다가 false 가 되면 셀에서 웨이퍼를 가져갈수 있음. 
        /// </summary>
        [DataMember]
        public bool WFWaitFlag
        {
            get { return _WFWaitFlag; }
            set { _WFWaitFlag = value; RaisePropertyChanged(); }
        }

        private string _Pre_OCRID;
        /// <summary>
        /// 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string Pre_OCRID
        {
            get { return _Pre_OCRID; }
            set { _Pre_OCRID = value; RaisePropertyChanged(); }
        }

        private string _CST_HashCode;
        /// <summary>
        /// ScanState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string CST_HashCode
        {
            get { return _CST_HashCode; }
            set { _CST_HashCode = value; RaisePropertyChanged(); }
        }

        private List<string> _LOTRunning_CSTHash_List = new List<string>();
        /// <summary>
        /// ScanState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public List<string> LOTRunning_CSTHash_List
        {
            get { return _LOTRunning_CSTHash_List; }
            set { _LOTRunning_CSTHash_List = value; RaisePropertyChanged(); }
        }
        public TransferObject()
        {
            try
            {
                _Type.Value = SubstrateTypeEnum.UNDEFINED;
                _WaferSubstrateType.Value = WaferSubstrateTypeEnum.UNDEFINED;
                _Size.Value = SubstrateSizeEnum.UNDEFINED;
                _WaferType.Value = EnumWaferType.UNDEFINED;
                _WaferState = EnumWaferState.UNDEFINED;
                _UsedPA = ModuleID.UNDEFINED;
                _PreAlignState = PreAlignStateEnum.NONE;
                _OCRReadState = OCRReadStateEnum.NONE;
                _OCR.Value = string.Empty;
                _CST_HashCode = string.Empty;
                _OriginHolder = ModuleID.UNDEFINED;
                _PrevHolder = ModuleID.UNDEFINED;
                _CurrHolder = ModuleID.UNDEFINED;
                _PrevPos = ModuleID.UNDEFINED;
                _CurrPos = ModuleID.UNDEFINED;
                _NotchType = WaferNotchTypeEnum.NOTCH;
                _ReservationTime = new DateTime();
                _ProcessingEnable = ProcessingEnableEnum.DISABLE;
                _Priority = 9999;
                _PMITirgger = PMIRemoteTriggerEnum.UNDIFINED;
                DoPMIFlag = false;
                OCRDevParam = new OCRDevParameter();
                _OverrideLoadNotchAngleOption = new LoadNotchAngleOption();
                _OverrideOCRDeviceOption = new OCRDeviceOption();
                _OverrideOCROption = new OCRPerformOption();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Copy(TransferObject Source)
        {
            try
            {
                _Type.Value = Source.Type.Value;
                _WaferSubstrateType.Value = Source.WaferSubstrateType.Value;
                _Size.Value = Source.Size.Value;
                _WaferType.Value = Source.WaferType.Value;
                _WaferState = Source.WaferState;
                _UsedPA = Source.UsedPA;
                _PreAlignState = Source.PreAlignState;
                _OCRReadState = Source.OCRReadState;
                _OCR.Value = Source.OCR.Value;
                _CST_HashCode = Source._CST_HashCode;
                _OriginHolder = Source.OriginHolder;
                _PrevHolder = Source.PrevHolder;
                _CurrHolder = Source.CurrHolder;
                _PrevPos = Source.PrevPos;
                _CurrPos = Source.CurrPos;
                _ReservationTime = Source.ReservationTime;
                _ProcessingEnable = Source.ProcessingEnable;
                _Priority = Source.Priority;
                _PMITirgger = Source.PMITirgger;
                DoPMIFlag = Source.DoPMIFlag;
                ChuckNotchAngle.Value = Source.ChuckNotchAngle.Value;
                SlotNotchAngle.Value = Source.SlotNotchAngle.Value;
                NotchAngle.Value = Source.NotchAngle.Value;

                _OverrideLoadNotchAngleOption = new LoadNotchAngleOption();
                _OverrideLoadNotchAngleOption.IsEnable = Source.OverrideLoadNotchAngleOption.IsEnable;
                _OverrideLoadNotchAngleOption.Angle = Source.OverrideLoadNotchAngleOption.Angle;

                _OverrideOCRDeviceOption = new OCRDeviceOption();
                _OverrideOCRDeviceOption.IsEnable = Source.OverrideOCRDeviceOption.IsEnable;

                _OverrideOCROption = new OCRPerformOption();
                _OverrideOCROption.IsEnable = Source.OverrideOCROption.IsEnable;
                _OverrideOCROption.IsPerform = Source.OverrideOCROption.IsPerform;

                _PolishWaferInfo = new PolishWaferInformation();

                if (Source.PolishWaferInfo != null)
                {
                    _PolishWaferInfo.Copy(Source.PolishWaferInfo);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// NeedPreAlign
        /// </summary>
        /// <returns></returns>
        public bool NeedPreAlign()
        {
            return PreAlignState != PreAlignStateEnum.DONE && PreAlignState != PreAlignStateEnum.SKIP;
        }
        public bool IsOCRDone()
        {
            bool isDone = false;
            try
            {
                if (WaferType.Value == EnumWaferType.POLISH)
                {
                    isDone = true;
                }
                else if (WaferType.Value == EnumWaferType.STANDARD|| WaferType.Value == EnumWaferType.TCW)
                {
                    isDone = ((OCRReadState == OCRReadStateEnum.ABORT) || OCRReadState == OCRReadStateEnum.DONE);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return isDone;
        }
        /// <summary>
        /// NeedOCR
        /// </summary>
        /// <returns></returns>
        public bool NeedOCR()
        {
            bool isPerformByState = false;
            bool isSkipOCR = false;
            try
            {
                isSkipOCR =
                    OverrideOCROption.IsEnable.Value &&
                    OverrideOCROption.IsPerform.Value == false;
                //isSkipOCR = true;
                //isPerformByState =
                //    OCRMode.Value == OCRModeEnum.READ &&
                //    OCRReadState != OCRReadStateEnum.DONE;

                isPerformByState = (
                                        ((OCRMode.Value == OCRModeEnum.READ) || (OCRMode.Value == OCRModeEnum.MANUAL)) &&
                                        (OCRReadState != OCRReadStateEnum.DONE) &&
                                        (OCRReadState != OCRReadStateEnum.ABORT)
                                   );

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            if (isSkipOCR)
            {
                return false;
            }

            return isPerformByState;
        }

        /// <summary>
        /// SetDeviceInfo
        /// </summary>
        /// <param name="originID"></param>
        /// <param name="devInfo"></param>
        public void SetDeviceInfo(ModuleID originID, TransferObject devInfo)
        {
            try
            {

                if (devInfo != null)
                {

                    OriginHolder = originID;
                    Type.Value = devInfo.Type.Value;
                    WaferSubstrateType.Value = devInfo.WaferSubstrateType.Value;
                    Size.Value = devInfo.Size.Value;
                    WaferType.Value = devInfo.WaferType.Value;
                    OCRType.Value = devInfo.OCRType.Value;
                    OCRMode.Value = devInfo.OCRMode.Value;
                    OCRDirection.Value = devInfo.OCRDirection.Value;
                    ChuckNotchAngle.Value = devInfo.NotchAngle.Value;
                    SlotNotchAngle.Value = devInfo.SlotNotchAngle.Value; //Todo Fixed =>노치앵글이 다를경우 언로딩하다가 버그가 나는경우 때문에 우선 동일하게함.
                    DeviceName.Value = devInfo.DeviceName.Value;
                    OCRDevParam = new OCRDevParameter();
                    OCRDevParam.OCRAngle = devInfo.OCRDevParam.OCRAngle;
                    OCRDevParam.ConfigList = new List<OCRConfig>();
                    CST_HashCode = devInfo.CST_HashCode;
                    foreach (var config in devInfo.OCRDevParam.ConfigList)
                    {
                        OCRDevParam.ConfigList.Add(config);
                    }
                    OCRAngle.Value = devInfo.OCRDevParam.OCRAngle;
                    OCRDevParam.lotIntegrity.LotIntegrityEnable = devInfo.OCRDevParam.lotIntegrity.LotIntegrityEnable;
                    OCRDevParam.lotIntegrity.LotnameDigit = devInfo.OCRDevParam.lotIntegrity.LotnameDigit;
                    OCRDevParam.lotIntegrity.Lotnamelength = devInfo.OCRDevParam.lotIntegrity.Lotnamelength;
                    NotchType = devInfo.NotchType;                    
                    CST_HashCode = devInfo.CST_HashCode;
                    //if(WaferType.Value == EnumWaferType.POLISH&&devInfo.PolishWaferInfo!=null)
                    //{
                    //    PolishWaferInfo = devInfo.PolishWaferInfo;
                    //    PolishWaferInfo.DefineName.Value = devInfo.DeviceName.Value;
                    //}
                    //else if (WaferType.Value == EnumWaferType.POLISH)
                    //{
                    //    PolishWaferInfo = new PolishWaferInformation();
                    //    PolishWaferInfo.DefineName.Value = devInfo.DeviceName.Value;
                    //}
                    if (WaferType.Value == EnumWaferType.POLISH)
                    {
                        PolishWaferInfo = new PolishWaferInformation();
                        PolishWaferInfo.DefineName.Value = devInfo.DeviceName.Value;
                        if (PolishWaferInfo.OCRConfigParam == null)
                        {
                            PolishWaferInfo.OCRConfigParam = new OCRDevParameter();
                        }
                        if (devInfo.PolishWaferInfo?.OCRConfigParam != null)
                        {
                            PolishWaferInfo.OCRConfigParam.Copy(devInfo.PolishWaferInfo.OCRConfigParam);
                        }
                        PolishWaferInfo.Size.Value = devInfo.Size.Value;
                        PolishWaferInfo.CurrentAngle.Value = devInfo.NotchAngle.Value;                    
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// PreAlign 상태를 Done으로 설정합니다.
        /// </summary>
        /// <param name="usedPA">사용된 PreAlign ID</param>
        public void SetPreAlignDone(ModuleID usedPA)
        {
            try
            {
                if (PreAlignState != PreAlignStateEnum.DONE)
                {
                    LoggerManager.Debug($"PA.{usedPA.Index} TransferObject.PreAlignState = DONE");
                }
                PreAlignState = PreAlignStateEnum.DONE;
                UsedPA = usedPA;
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// PreAlign 상태를 초기화합니다.
        /// </summary>
        public void CleanPreAlignState(string reason)
        {
            try
            {
                if (PreAlignState != PreAlignStateEnum.NONE)
                {
                    LoggerManager.Debug($"PA.{UsedPA.Index} TransferObject.PreAlignState = NONE, Reason:{reason}");
                }
                PreAlignState = PreAlignStateEnum.NONE;
                UsedPA = ModuleID.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// OCR 상태를 설정합니다.
        /// </summary>
        /// <param name="readText">읽은 문자열</param>
        /// <param name="score">점수</param>
        /// <param name="state">상태</param>
        public void SetOCRState(string readText, double score, OCRReadStateEnum state)
        {
            try
            {
                if (!(state == OCRReadStateEnum.FAILED && OCRReadState == OCRReadStateEnum.ABORT))//TODO: 이조건 정말 필요한지? 검토할것. 현재 ABORT 상태인데 FAIL로 엎어치려고 하는 것을 막기위한 조건으로 보임. 
                {
                    OCR.Value = readText;
                    OCRReadScore.Value = score;
                    OCRReadState = state;
                    if (state != OCRReadStateEnum.NONE)
                    {
                        LoggerManager.Debug($"[SetOCRState] State={state}, ID: { readText} , Score:{score}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// OCR 상태를 초기화합니다.
        /// </summary>
        public void CleanOCRState()
        {
            try
            {
                OCR.Value = "";
                OCRReadScore.Value = 0;
                OCRReadState = OCRReadStateEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Option을 Disable합니다.
        /// </summary>
        public void DisableOptionAll()
        {
            try
            {
                this.OverrideLoadNotchAngleOption.IsEnable.Value = false;
                this.OverrideOCRDeviceOption.IsEnable.Value = false;
                this.OverrideOCROption.IsEnable.Value = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            //1. this -> serial
            //return dearial ;

            var shallowClone = MemberwiseClone() as TransferObject;
            try
            {
                shallowClone.OverrideLoadNotchAngleOption = OverrideLoadNotchAngleOption.Clone() as LoadNotchAngleOption;
                shallowClone.OverrideOCRDeviceOption = OverrideOCRDeviceOption.Clone() as OCRDeviceOption;
                shallowClone.OverrideOCROption = OverrideOCROption.Clone() as OCRPerformOption;
                shallowClone.NotchAngle = new Element<double>();
                shallowClone.NotchAngle.Value = NotchAngle.Value;
                shallowClone.CST_HashCode = CST_HashCode;
                shallowClone.WaferType = WaferType;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return shallowClone;
        }
        public void SetReservationState(ReservationStateEnum reservationState)
        {
            this.ReservationState = reservationState;
        }

        public void ClearData()
        {
            try
            {
                this.WaferType.Value = EnumWaferType.UNDEFINED;
                this.DeviceName.Value = string.Empty;
                this.Size.Value = SubstrateSizeEnum.UNDEFINED;
                this.WaferType.Value = EnumWaferType.UNDEFINED;
                
                this.PolishWaferInfo = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }

    /// <summary>
    /// PreAlign 상태를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum PreAlignStateEnum
    {
        /// <summary>
        /// NONE
        /// </summary>
        [EnumMember]
        NONE,
        /// <summary>
        /// DONE
        /// </summary>
        [EnumMember]
        DONE,
        /// <summary>
        /// SKIP
        /// </summary>
        [EnumMember]
        SKIP
    }

    [Serializable]
    [DataContract]
    public enum ReservationStateEnum
    {
        /// <summary>
        /// NONE
        /// </summary>
        [EnumMember]
        NONE,
        /// <summary>
        /// RESERVATION
        /// </summary>
        [EnumMember]
        RESERVATION

    }

    [Serializable]
    [DataContract]
    public enum ProcessingEnableEnum
    {
        /// <summary>
        /// NONE
        /// </summary>
        [EnumMember]
        DISABLE,
        /// <summary>
        /// RESERVATION
        /// </summary>
        [EnumMember]
        ENABLE

    }
    /// <summary>
    /// LoadNotchAngleOption을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class LoadNotchAngleOption : INotifyPropertyChanged, ICloneable, IParamNode
    {
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<bool> _IsEnable = new Element<bool>();
        /// <summary>
        /// IsEnable 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> IsEnable
        {
            get { return _IsEnable; }
            set { _IsEnable = value; RaisePropertyChanged(); }
        }

        private Element<double> _Angle = new Element<double>();
        /// <summary>
        /// Angle 을 가져오거나 설정합니다. (단위 degree)
        /// </summary>
        [DataMember]
        public Element<double> Angle
        {
            get { return _Angle; }
            set { _Angle = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone() as LoadNotchAngleOption;
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    /// <summary>
    /// OCROption 을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class OCRPerformOption : INotifyPropertyChanged, ICloneable, IParamNode
    {
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<bool> _IsEnable = new Element<bool>();
        /// <summary>
        /// IsEnable 을 가져오거나 설정합니다. (단위 degree)
        /// </summary>
        [DataMember]
        public Element<bool> IsEnable
        {
            get { return _IsEnable; }
            set { _IsEnable = value; RaisePropertyChanged(); }
        }

        private Element<bool> _IsPerform = new Element<bool>();
        /// <summary>
        /// IsPerform 을 가져오거나 설정합니다. (단위 degree)
        /// </summary>
        [DataMember]
        public Element<bool> IsPerform
        {
            get { return _IsPerform; }
            set { _IsPerform = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone() as OCRPerformOption;
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    /// <summary>
    /// OCRDeviceOption 을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class OCRDeviceOption : INotifyPropertyChanged, ICloneable, IParamNode
    {
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<bool> _IsEnable = new Element<bool>();
        /// <summary>
        /// IsEnable 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> IsEnable
        {
            get { return _IsEnable; }
            set { _IsEnable = value; RaisePropertyChanged(); }
        }

        private OCRDeviceBase _OCRDeviceBase;
        /// <summary>
        /// _OCRDeviceBase 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public OCRDeviceBase OCRDeviceBase
        {
            get { return _OCRDeviceBase; }
            set { _OCRDeviceBase = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {

            try
            {
                var shallowClone = MemberwiseClone() as OCRDeviceOption;
                if (shallowClone.OCRDeviceBase != null)
                {
                    shallowClone.OCRDeviceBase = OCRDeviceBase.Clone() as OCRDeviceBase;
                }
                return shallowClone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }



    /// <summary>
    /// PolishWaferInfo 을 정의합니다.
    /// </summary>

    [DataContract]
    [Serializable]
    public class PolishWaferInformation : INotifyPropertyChanged, ICloneable, IParamNode, IPolishWaferSourceInformation
    {
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private Element<string> _DefineName = new Element<string>();
        /// <summary>
        /// CurrentAngle 을 가져오거나 설정합니다. 
        /// </summary>
        [DataMember]
        public Element<string> DefineName
        {
            get { return _DefineName; }
            set { _DefineName = value; RaisePropertyChanged(); }
        }

        private Element<double> _MaxLimitCount = new Element<double>();
        /// <summary>
        /// TouchCount 을 가져오거나 설정합니다. 
        /// </summary>
        [DataMember]
        public Element<double> MaxLimitCount
        {
            get { return _MaxLimitCount; }
            set { _MaxLimitCount = value; RaisePropertyChanged(); }
        }

        private Element<double> _TouchCount = new Element<double>();
        /// <summary>
        /// TouchCount 을 가져오거나 설정합니다. 
        /// </summary>
        [DataMember]
        public Element<double> TouchCount
        {
            get { return _TouchCount; }
            set { _TouchCount = value; RaisePropertyChanged(); }
        }

        private Element<int> _Priorty = new Element<int>();
        [DataMember]
        public Element<int> Priorty
        {
            get { return _Priorty; }
            set { _Priorty = value; RaisePropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _Size = new Element<SubstrateSizeEnum>();
        [DataMember]
        public Element<SubstrateSizeEnum> Size
        {
            get { return _Size; }
            set { _Size = value; RaisePropertyChanged(); }
        }

        private Element<WaferNotchTypeEnum> _NotchType = new Element<WaferNotchTypeEnum>() { Value = WaferNotchTypeEnum.NOTCH};
        [DataMember]
        public Element<WaferNotchTypeEnum> NotchType
        {
            get { return _NotchType; }
            set { _NotchType = value; RaisePropertyChanged(); }
        }

        private Element<double> _CurrentAngle = new Element<double>();
        /// <summary>
        /// CurrentAngle 을 가져오거나 설정합니다. 
        /// </summary>
        [DataMember]
        public Element<double> CurrentAngle
        {
            get { return _CurrentAngle; }
            set { _CurrentAngle = value; RaisePropertyChanged(); }
        }

        private Element<double> _NotchAngle = new Element<double>();
        [DataMember]
        public Element<double> NotchAngle
        {
            get { return _NotchAngle; }
            set { _NotchAngle = value; }
        }

        private Element<double> _RotateAngle = new Element<double>();
        /// <summary>
        /// RotateAngle 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> RotateAngle
        {
            get { return _RotateAngle; }
            set { _RotateAngle = value; RaisePropertyChanged(); }
        }

        private Element<double> _Margin = new Element<double>();
        /// <summary>
        /// CurrentAngle 을 가져오거나 설정합니다. 
        /// </summary>
        [DataMember]
        public Element<double> Margin
        {
            get { return _Margin; }
            set { _Margin = value; RaisePropertyChanged(); }
        }


        //private FocusParameter _FocusParam;
        ///// <summary>
        ///// Focusing 파라미터를 가져오거나 설정합니다.
        ///// </summary>
        //[DataMember]
        //public FocusParameter FocusParam
        //{
        //    get { return _FocusParam; }
        //    set { _FocusParam = value; RaisePropertyChanged(); }
        //}

        //private ModuleDllInfo _FocusingModuleDllInfo;
        ///// <summary>
        ///// Focusing을 할 때, 사용되는 Module 정보를 가져오거나 설정합니다.
        ///// </summary>
        //[DataMember]
        //public ModuleDllInfo FocusingModuleDllInfo
        //{
        //    get { return _FocusingModuleDllInfo; }
        //    set { _FocusingModuleDllInfo = value; }
        //}

        //private Element<PWFocusingPointMode> _FocusingPointMode = new Element<PWFocusingPointMode>();
        ///// <summary>
        ///// Focusing을 할 때, 몇 포인트를 Focusing 할지 결정하는 파라미터입니다.
        ///// </summary>
        //[DataMember]
        //public Element<PWFocusingPointMode> FocusingPointMode
        //{
        //    get { return _FocusingPointMode; }
        //    set { _FocusingPointMode = value; }
        //}

        //private Element<PWFocusingType> _FocusingType = new Element<PWFocusingType>();
        ///// <summary>
        ///// Focusing을 할 때, 카메라로 할지, 터치 센서로 할지 결정하는 파라미터입니다.
        ///// </summary>
        //[DataMember]
        //public Element<PWFocusingType> FocusingType
        //{
        //    get { return _FocusingType; }
        //    set { _FocusingType = value; }
        //}

        private Element<double> _Thickness = new Element<double>();
        [DataMember]
        public Element<double> Thickness
        {
            get { return _Thickness; }
            set
            {
                if (value != _Thickness)
                {
                    _Thickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _IdentificationColorBrush = new Element<string>();

        [DataMember]
        public Element<string> IdentificationColorBrush
        {
            get
            {
                return _IdentificationColorBrush;
            }
            set
            {
                if (value != _IdentificationColorBrush)
                {
                    _IdentificationColorBrush = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private WaferHeightMapping _WaferHeightMapping = new WaferHeightMapping();
        [JsonIgnore]
        public WaferHeightMapping WaferHeightMapping
        {
            get
            {
                if (_WaferHeightMapping == null)
                {
                    _WaferHeightMapping = new WaferHeightMapping();
                }
                return _WaferHeightMapping;
            }

            set { _WaferHeightMapping = value; }
        }

        [NonSerialized]
        private WaferCoordinate _PolishWaferCenter;
        [JsonIgnore]
        public WaferCoordinate PolishWaferCenter
        {
            get { return _PolishWaferCenter; }
            set
            {
                if (value != _PolishWaferCenter)
                {
                    _PolishWaferCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DeadZone = 500;

        public double DeadZone
        {
            get { return _DeadZone; }
            set { _DeadZone = value; }
        }

        private OCRDevParameter _OCRConfigParam;

        [DataMember]
        public OCRDevParameter OCRConfigParam
        {
            get { return _OCRConfigParam; }
            set { _OCRConfigParam = value; }
        }

        public PolishWaferInformation()
        {

        }
        public PolishWaferInformation(string definename)
        {
            this.DefineName.Value = definename;
        }

        public void Copy(IPolishWaferSourceInformation Source)
        {
            try
            {
                this.DefineName.Value = Source.DefineName.Value;
                this.MaxLimitCount.Value = Source.MaxLimitCount.Value;
                this.TouchCount.Value = Source.TouchCount.Value;//TODO: Type을 Copy할때는 다른 웨이퍼 일 수 있으므로 TouchCount 를 Clear해주고 사용하지 않는다.
                this.Priorty.Value = Source.Priorty.Value;
                this.Size.Value = Source.Size.Value;
                this.NotchType.Value = Source.NotchType.Value;
                this.Margin.Value = Source.Margin.Value;
                this.Thickness.Value = Source.Thickness.Value;
                this.CurrentAngle.Value = Source.CurrentAngle.Value;
                this.NotchAngle.Value = Source.NotchAngle.Value;
                this.RotateAngle.Value = Source.RotateAngle.Value;

                this.IdentificationColorBrush.Value = Source.IdentificationColorBrush.Value;

                if (Source.OCRConfigParam != null)
                {
                    if(this.OCRConfigParam == null)
                    {
                        this.OCRConfigParam = new OCRDevParameter();
                    }

                    this.OCRConfigParam.Copy(Source.OCRConfigParam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ClearData()
        {
            try
            {
                this.DefineName.Value = string.Empty;
                this.MaxLimitCount.Value = 9999;
                this.TouchCount.Value = 0;
                this.Priorty.Value = 0;
                this.Size.Value = SubstrateSizeEnum.UNDEFINED;
                this.NotchType.Value = WaferNotchTypeEnum.UNKNOWN; 
                this.Margin.Value = 0;
                this.Thickness.Value = 0;
                this.CurrentAngle.Value = 0;
                this.NotchAngle.Value = 0;
                this.RotateAngle.Value = 0;

                this.IdentificationColorBrush.Value = "DarkGray";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone() as PolishWaferInformation;
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }


    [Serializable]
    public class PolishWaferInfoParameter : ISystemParameterizable, INotifyPropertyChanged
    {
        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FilePath => "PolishWafer";

        public string FileName => "PolishWaferInfoParameter.json";

        public bool IsParamChanged { get; set; }
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


        private AsyncObservableCollection<IPolishWaferSourceInformation> _PolishWaferTypeParams = new AsyncObservableCollection<IPolishWaferSourceInformation>();
        [DataMember]
        public AsyncObservableCollection<IPolishWaferSourceInformation> PolishWaferTypeParams
        {
            get { return _PolishWaferTypeParams; }
            set
            {
                if (value != _PolishWaferTypeParams)
                {
                    _PolishWaferTypeParams = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            return;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }
    }

}
