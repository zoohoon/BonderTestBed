using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using LogModule;
using Newtonsoft.Json;
using Focusing;

namespace CardChange
{

    // Z Offset과 Theta만 System 파라미터로 남길 것.
    [Serializable]
    public class CardChangeSysParam : INotifyPropertyChanged, IParamNode, ISystemParameterizable, ICardChangeSysParam
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
        public string FileName { get; } = "CardChangeParams.json";
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

        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }
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
        private FocusParameter _PLFocusParam;
        public FocusParameter PLFocusParam
        {
            get { return _PLFocusParam; }
            set
            {
                if (value != _PLFocusParam)
                {
                    _PLFocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> CardChangeType
        private Element<EnumCardChangeType> _CardChangeType = new Element<EnumCardChangeType>();
        public Element<EnumCardChangeType> CardChangeType
        {
            get { return _CardChangeType; }
            set
            {
                if (value != _CardChangeType)
                {
                    _CardChangeType = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private Element<EnumCardDockType> _CardDockType = new Element<EnumCardDockType>();
        public Element<EnumCardDockType> CardDockType
        {
            get { return _CardDockType; }
            set
            {
                if (value != _CardDockType)
                {
                    _CardDockType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumPogoAlignPoint> _PogoAlignPoint = new Element<EnumPogoAlignPoint>();
        public Element<EnumPogoAlignPoint> PogoAlignPoint
        {
            get { return _PogoAlignPoint; }
            set
            {
                if (value != _PogoAlignPoint)
                {
                    _PogoAlignPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> GP_DockUndockTesterVacSequenceSkip
        private bool _GPTesterVacSeqSkip;
        public bool GPTesterVacSeqSkip
        {
            get { return _GPTesterVacSeqSkip; }
            set
            {
                if (value != _GPTesterVacSeqSkip)
                {
                    _GPTesterVacSeqSkip = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_Undock_CardContactPosX
        private double _GP_Undock_CardContactPosX;
        public double GP_Undock_CardContactPosX
        {
            get { return _GP_Undock_CardContactPosX; }
            set
            {
                if (value != _GP_Undock_CardContactPosX)
                {
                    _GP_Undock_CardContactPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_Undock_CardContactPosY
        private double _GP_Undock_CardContactPosY;
        public double GP_Undock_CardContactPosY
        {
            get { return _GP_Undock_CardContactPosY; }
            set
            {
                if (value != _GP_Undock_CardContactPosY)
                {
                    _GP_Undock_CardContactPosY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_Undock_CardContactPosZ
        private double _GP_Undock_CardContactPosZ;
        public double GP_Undock_CardContactPosZ
        {
            get { return _GP_Undock_CardContactPosZ; }
            set
            {
                if (value != _GP_Undock_CardContactPosZ)
                {
                    _GP_Undock_CardContactPosZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_Undock_CardContactPosT
        private double _GP_Undock_CardContactPosT;
        public double GP_Undock_CardContactPosT
        {
            get { return _GP_Undock_CardContactPosT; }
            set
            {
                if (value != _GP_Undock_CardContactPosT)
                {
                    _GP_Undock_CardContactPosT = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_LoadingPosT
        private double _GP_LoadingPosT;
        public double GP_LoadingPosT
        {
            get { return _GP_LoadingPosT; }
            set
            {
                if (value != _GP_LoadingPosT)
                {
                    _GP_LoadingPosT = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_CardCheckOffsetZ
        private double _GP_CardCheckOffsetZ;
        public double GP_CardCheckOffsetZ
        {
            get { return _GP_CardCheckOffsetZ; }
            set
            {
                if (value != _GP_CardCheckOffsetZ)
                {
                    _GP_CardCheckOffsetZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_ContactCorrectionX
        private double _GP_ContactCorrectionX;
        public double GP_ContactCorrectionX
        {
            get { return _GP_ContactCorrectionX; }
            set
            {
                if (value != _GP_ContactCorrectionX)
                {
                    _GP_ContactCorrectionX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_ContactCorrectionY
        private double _GP_ContactCorrectionY;
        public double GP_ContactCorrectionY
        {
            get { return _GP_ContactCorrectionY; }
            set
            {
                if (value != _GP_ContactCorrectionY)
                {
                    _GP_ContactCorrectionY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_ContactCorrectionZ
        private double _GP_ContactCorrectionZ;
        public double GP_ContactCorrectionZ
        {
            get { return _GP_ContactCorrectionZ; }
            set
            {
                if (value != _GP_ContactCorrectionZ)
                {
                    _GP_ContactCorrectionZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_Undock_ContactCorrectionZ
        private double _GP_Undock_ContactCorrectionZ;
        public double GP_Undock_ContactCorrectionZ
        {
            get { return _GP_Undock_ContactCorrectionZ; }
            set
            {
                if (value != _GP_Undock_ContactCorrectionZ)
                {
                    _GP_Undock_ContactCorrectionZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_PogoCenter
        private PinCoordinate _GP_PogoCenter = new PinCoordinate();
        public PinCoordinate GP_PogoCenter
        {
            get { return _GP_PogoCenter; }
            set
            {
                if (value != _GP_PogoCenter)
                {
                    _GP_PogoCenter = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_PogoMarkPosList
        private List<PinCoordinate> _GP_PogoMarkPosList = new List<PinCoordinate>();
        public List<PinCoordinate> GP_PogoMarkPosList
        {
            get { return _GP_PogoMarkPosList; }
            set
            {
                if (value != _GP_PogoMarkPosList)
                {
                    _GP_PogoMarkPosList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_PogoMarkPosList3P
        private List<PinCoordinate> _GP_PogoMarkPosList3P = new List<PinCoordinate>();
        public List<PinCoordinate> GP_PogoMarkPosList3P
        {
            get { return _GP_PogoMarkPosList3P; }
            set
            {
                if (value != _GP_PogoMarkPosList3P)
                {
                    _GP_PogoMarkPosList3P = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> CardPodMarkPosList
        private List<WaferCoordinate> _CardPodMarkPosList = new List<WaferCoordinate>();
        public List<WaferCoordinate> CardPodMarkPosList
        {
            get { return _CardPodMarkPosList; }
            set
            {
                if (value != _CardPodMarkPosList)
                {
                    _CardPodMarkPosList = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Double _CardPodGuideHeight = new Double();
        public Double CardPodGuideHeight
        {
            get { return _CardPodGuideHeight; }
            set
            {
                if (value != _CardPodGuideHeight)
                {
                    _CardPodGuideHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardPodCenterX = new Element<double>();
        public Element<double> CardPodCenterX
        {
            get { return _CardPodCenterX; }
            set
            {
                if (value != _CardPodCenterX)
                {
                    _CardPodCenterX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardPodCenterY = new Element<double>();
        public Element<double> CardPodCenterY
        {
            get { return _CardPodCenterY; }
            set
            {
                if (value != _CardPodCenterY)
                {
                    _CardPodCenterY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardPodCenterZ = new Element<double>();
        public Element<double> CardPodCenterZ
        {
            get { return _CardPodCenterZ; }
            set
            {
                if (value != _CardPodCenterZ)
                {
                    _CardPodCenterZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardTopFromChuckPlane = new Element<double>();
        public Element<double> CardTopFromChuckPlane
        {
            get { return _CardTopFromChuckPlane; }
            set
            {
                if (value != _CardTopFromChuckPlane)
                {
                    _CardTopFromChuckPlane = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardPodDeg = new Element<double>();
        public Element<double> CardPodDeg
        {
            get { return _CardPodDeg; }
            set
            {
                if (value != _CardPodDeg)
                {
                    _CardPodDeg = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> GP_SearchedPogoMarkPosList
        private List<PinCoordinate> _GP_SearchedPogoMarkPosList = new List<PinCoordinate>();
        public List<PinCoordinate> GP_SearchedPogoMarkPosList
        {
            get { return _GP_SearchedPogoMarkPosList; }
            set
            {
                if (value != _GP_SearchedPogoMarkPosList)
                {
                    _GP_SearchedPogoMarkPosList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_SearchedPogoMarkPosList3P
        private List<PinCoordinate> _GP_SearchedPogoMarkPosList3P = new List<PinCoordinate>();
        public List<PinCoordinate> GP_SearchedPogoMarkPosList3P
        {
            get { return _GP_SearchedPogoMarkPosList3P; }
            set
            {
                if (value != _GP_SearchedPogoMarkPosList3P)
                {
                    _GP_SearchedPogoMarkPosList3P = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_CardMarkPosList
        private List<WaferCoordinate> _GP_CardMarkPosList = new List<WaferCoordinate>();
        public List<WaferCoordinate> GP_CardMarkPosList
        {
            get { return _GP_CardMarkPosList; }
            set
            {
                if (value != _GP_CardMarkPosList)
                {
                    _GP_CardMarkPosList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_SearchedCardMarkPosList
        private List<WaferCoordinate> _GP_SearchedCardMarkPosList = new List<WaferCoordinate>();
        public List<WaferCoordinate> GP_SearchedCardMarkPosList
        {
            get { return _GP_SearchedCardMarkPosList; }
            set
            {
                if (value != _GP_SearchedCardMarkPosList)
                {
                    _GP_SearchedCardMarkPosList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private List<PinBaseFiducialMarkParameter> _PinBaseFiducialMarkInfos = new List<PinBaseFiducialMarkParameter>();
        public List<PinBaseFiducialMarkParameter> PinBaseFiducialMarkInfos
        {
            get { return _PinBaseFiducialMarkInfos; }
            set
            {
                if (value != _PinBaseFiducialMarkInfos)
                {
                    _PinBaseFiducialMarkInfos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private WaferCoordinate _PodCenterOffset = new WaferCoordinate();
        public WaferCoordinate PodCenterOffset
        {
            get { return _PodCenterOffset; }
            set
            {
                if (value != _PodCenterOffset)
                {
                    _PodCenterOffset = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region ==> GP_WL_COAXIAL
        private ushort _GP_WL_COAXIAL;
        public ushort GP_WL_COAXIAL
        {
            get { return _GP_WL_COAXIAL; }
            set
            {
                if (value != _GP_WL_COAXIAL)
                {
                    _GP_WL_COAXIAL = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_WL_OBLIQUE
        private ushort _GP_WL_OBLIQUE;
        public ushort GP_WL_OBLIQUE
        {
            get { return _GP_WL_OBLIQUE; }
            set
            {
                if (value != _GP_WL_OBLIQUE)
                {
                    _GP_WL_OBLIQUE = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_PL_COAXIAL
        private ushort _GP_PL_COAXIAL;
        public ushort GP_PL_COAXIAL
        {
            get { return _GP_PL_COAXIAL; }
            set
            {
                if (value != _GP_PL_COAXIAL)
                {
                    _GP_PL_COAXIAL = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> GP_PL_OBLIQUE
        private ushort _GP_PL_OBLIQUE;
        public ushort GP_PL_OBLIQUE
        {
            get { return _GP_PL_OBLIQUE; }
            set
            {
                if (value != _GP_PL_OBLIQUE)
                {
                    _GP_PL_OBLIQUE = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> CardID
        //minskim// ProbeCard ID 값의 Defalut 값이 선언되어 있지 않아 CardChangeParams.json의 CardID 값이 NULL로 Setting 되는 현상을 방지하기 위해 기본값 선언 추가함
        private string _CardID = string.Empty;
        public string CardID
        {
            get { return _CardID; }
            set
            {
                //minskim// CardID  값이 null일 경우 해당 Property를 사용하는 로직에서 exception이 발생할수 있어 이에 대해 예외 처리함, null일 경우 기본값(string.empty)를 return 하도록 함                
                if (value != _CardID)
                {
                    _CardID = value ?? string.Empty;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ProbeCard
        private Element<string> _ProbeCardID = new Element<string>();
        public Element<string> ProbeCardID
        {
            get { return _ProbeCardID; }
            set
            {
                if (value != _ProbeCardID)
                {
                    _ProbeCardID = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PatternMatching Retry Count
        private int _PatternMatchingRetryCount;
        public int PatternMatchingRetryCount
        {
            get { return _PatternMatchingRetryCount; }
            set
            {
                if (value != _PatternMatchingRetryCount)
                {
                    _PatternMatchingRetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> DistanceOffset
        private double _DistanceOffset;
        public double DistanceOffset
        {
            get { return _DistanceOffset; }
            set
            {
                if (value != _DistanceOffset)
                {
                    _DistanceOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private Element<double> _CardDockPosX = new Element<double>();
        public Element<double> CardDockPosX
        {
            get { return _CardDockPosX; }
            set
            {
                if (value != _CardDockPosX)
                {
                    _CardDockPosX = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _CardDockPosY = new Element<double>();
        public Element<double> CardDockPosY
        {
            get { return _CardDockPosY; }
            set
            {
                if (value != _CardDockPosY)
                {
                    _CardDockPosY = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _CardDockPosZ = new Element<double>();
        public Element<double> CardDockPosZ
        {
            get { return _CardDockPosZ; }
            set
            {
                if (value != _CardDockPosZ)
                {
                    _CardDockPosZ = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _CardDockPosT = new Element<double>();
        public Element<double> CardDockPosT
        {
            get { return _CardDockPosT; }
            set
            {
                if (value != _CardDockPosT)
                {
                    _CardDockPosT = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardHolderCheckPosX = new Element<double>();
        public Element<double> CardHolderCheckPosX
        {
            get { return _CardHolderCheckPosX; }
            set
            {
                if (value != _CardHolderCheckPosX)
                {
                    _CardHolderCheckPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardHolderCheckPosY = new Element<double>();
        public Element<double> CardHolderCheckPosY
        {
            get { return _CardHolderCheckPosY; }
            set
            {
                if (value != _CardHolderCheckPosY)
                {
                    _CardHolderCheckPosY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardHolderCheckPosZ = new Element<double>();
        public Element<double> CardHolderCheckPosZ
        {
            get { return _CardHolderCheckPosZ; }
            set
            {
                if (value != _CardHolderCheckPosZ)
                {
                    _CardHolderCheckPosZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardHolderCheckPosT = new Element<double>();
        public Element<double> CardHolderCheckPosT
        {
            get { return _CardHolderCheckPosT; }
            set
            {
                if (value != _CardHolderCheckPosT)
                {
                    _CardHolderCheckPosT = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardPodRadiusMax = new Element<double>();
        public Element<double> CardPodRadiusMax
        {
            get { return _CardPodRadiusMax; }
            set
            {
                if (value != _CardPodRadiusMax)
                {
                    _CardPodRadiusMax = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardPodRadiusMin = new Element<double>();
        public Element<double> CardPodRadiusMin
        {
            get { return _CardPodRadiusMin; }
            set
            {
                if (value != _CardPodRadiusMin)
                {
                    _CardPodRadiusMin = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CardPodMinHeight = new Element<double>();
        public Element<double> CardPodMinHeight
        {
            get { return _CardPodMinHeight; }
            set
            {
                if (value != _CardPodMinHeight)
                {
                    _CardPodMinHeight = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<WaferCoordinate> _CardPodCenterPoint = new Element<WaferCoordinate>();
        public Element<WaferCoordinate> CardPodCenterPoint
        {
            get { return _CardPodCenterPoint; }
            set
            {
                if (value != _CardPodCenterPoint)
                {
                    _CardPodCenterPoint = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<CatCoordinates> _CardTransferPos = new Element<CatCoordinates>();
        public Element<CatCoordinates> CardTransferPos
        {
            get { return _CardTransferPos; }
            set
            {
                if (value != _CardTransferPos)
                {
                    _CardTransferPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardTransferOffsetX = new Element<double>();
        public Element<double> CardTransferOffsetX
        {
            get { return _CardTransferOffsetX; }
            set
            {
                if (value != _CardTransferOffsetX)
                {
                    _CardTransferOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardTransferOffsetY = new Element<double>();
        public Element<double> CardTransferOffsetY
        {
            get { return _CardTransferOffsetY; }
            set
            {
                if (value != _CardTransferOffsetY)
                {
                    _CardTransferOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardTransferOffsetZ = new Element<double>();
        public Element<double> CardTransferOffsetZ
        {
            get { return _CardTransferOffsetZ; }
            set
            {
                if (value != _CardTransferOffsetZ)
                {
                    _CardTransferOffsetZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardTransferOffsetT = new Element<double>();
        public Element<double> CardTransferOffsetT
        {
            get { return _CardTransferOffsetT; }
            set
            {
                if (value != _CardTransferOffsetT)
                {
                    _CardTransferOffsetT = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _CardDoorAttached;
        public bool CardDoorAttached
        {
            get { return _CardDoorAttached; }
            set
            {
                if (value != _CardDoorAttached)
                {
                    _CardDoorAttached = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region ==> GP_ManualPogoAlignMode 
        private bool _GP_ManualPogoAlignMode;
        public bool GP_ManualPogoAlignMode
        {
            get { return _GP_ManualPogoAlignMode; }
            set
            {
                if (value != _GP_ManualPogoAlignMode)
                {
                    _GP_ManualPogoAlignMode = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _IsCardExist; //MPT OPRT 의 경우 카드를 확인 할수 없으므로 카드를 로드 언로드 할 시 시스템 파라미터에 저장한다.
        public bool IsCardExist
        {
            get { return _IsCardExist; }
            set
            {
                if (value != _IsCardExist)
                {
                    _IsCardExist = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// IsDocked ROT Lock이 되었는지 체크하는 Flag
        /// True : 잠김
        /// False : 열림
        /// *이 파라미터 사용 시 주의사항* 
        /// 1.  GOP_TopPlateSolLocked내부에서만 사용함
        /// 2.GOP_TopPlateSolLocked는 Card Change시퀀스 중 ROT Lock/Unlock바로 후에 동작되도록 넣어져 있음
        /// 3. 임의로 값을 바꾸면 안됨. 바꿔도 Set함수가 있으니 Set함수를 사용하자
        /// </summary>
        private bool _IsDocked;
        public bool IsDocked
        {
            get { return _IsDocked; }
            set
            {
                if (value != _IsDocked)
                {
                    _IsDocked = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _MoveToCardHolderPosEnable = false; //MPT OPRT 의 경우 홀더 상태 업데이트 MoveToCardHolderPositionAndCheck 을 수행여부
        public bool MoveToCardHolderPosEnable
        {
            get { return _MoveToCardHolderPosEnable; }
            set
            {
                if (value != _MoveToCardHolderPosEnable)
                {
                    _MoveToCardHolderPosEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DD_CardDockZInterval = new Element<double>();
        public Element<double> DD_CardDockZInterval
        {
            get { return _DD_CardDockZInterval; }
            set
            {
                if (value != _DD_CardDockZInterval)
                {
                    _DD_CardDockZInterval = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _DD_CardDockZMaxValue = new Element<double>();
        public Element<double> DD_CardDockZMaxValue
        {
            get { return _DD_CardDockZMaxValue; }
            set
            {
                if (value != _DD_CardDockZMaxValue)
                {
                    _DD_CardDockZMaxValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _DD_CardUndockZOffeset = new Element<double>();
        public Element<double> DD_CardUndockZOffeset
        {
            get { return _DD_CardUndockZOffeset; }
            set
            {
                if (value != _DD_CardUndockZOffeset)
                {
                    _DD_CardUndockZOffeset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ProberCardListParameter> _ProberCardList;
        public List<ProberCardListParameter> ProberCardList
        {
            get { return _ProberCardList; }
            set
            {
                if (value != _ProberCardList)
                {
                    _ProberCardList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SnailPointsOdd = 3;
        public int SnailPointsOdd
        {
            get { return _SnailPointsOdd; }
            set
            {
                if (value != _SnailPointsOdd)
                {
                    _SnailPointsOdd = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _RestoreSVAfterCardChange = new Element<bool>() { Value = true };
        /// <summary>
        /// TempSource 가 CC 인경우 동작 완료 시, 이전 온도로 되돌릴지 말지 여부 (true : 이전 온도로 변경 , false : 온도 유지)
        /// </summary>
        public Element<bool> RestoreSVAfterCardChange
        {
            get { return _RestoreSVAfterCardChange; }
            set
            {
                if (value != _RestoreSVAfterCardChange)
                {
                    _RestoreSVAfterCardChange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CCActivatableTemp = new Element<double>() { Value = 30.0 };
        public Element<double> CCActivatableTemp
        {
            get { return _CCActivatableTemp; }
            set
            {
                if (value != _CCActivatableTemp)
                {
                    _CCActivatableTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CCActiveTempDeviation = new Element<double>() { Value = 1 };
        public Element<double> CCActiveTempDeviation
        {
            get { return _CCActiveTempDeviation; }
            set
            {
                if (value != _CCActiveTempDeviation)
                {
                    _CCActiveTempDeviation = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _OverHeating = new Element<double>() { Value = 3 };
        public Element<double> OverHeating
        {
            get { return _OverHeating; }
            set
            {
                if (value != _OverHeating)
                {
                    _OverHeating = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OverHeatingHysteresis = new Element<double>() { Value = 3 };
        public Element<double> OverHeatingHysteresis
        {
            get { return _OverHeatingHysteresis; }
            set
            {
                if (value != _OverHeatingHysteresis)
                {
                    _OverHeatingHysteresis = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<bool> _WaitForCardPermitEnable = new Element<bool> { Value = false };
        public Element<bool> WaitForCardPermitEnable
        {
            get { return _WaitForCardPermitEnable; }
            set
            {
                if (value != _WaitForCardPermitEnable)
                {
                    _WaitForCardPermitEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardContactRadiusLimit = new Element<double>();
        public Element<double> CardContactRadiusLimit
        {
            get { return _CardContactRadiusLimit; }
            set
            {
                if (value != _CardContactRadiusLimit)
                {
                    _CardContactRadiusLimit = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<bool> _WaitTesterResponse = new Element<bool>();
        public Element<bool> WaitTesterResponse
        {
            get { return _WaitTesterResponse; }
            set
            {
                if (value != _WaitTesterResponse)
                {
                    _WaitTesterResponse = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<string> _WMBIdentifier = new Element<string> { Value = "NA" };
        /// <summary>
        /// 1101
        /// </summary>
        public Element<string> WMBIdentifier
        {
            get { return _WMBIdentifier; }
            set
            {
                if (value != _WMBIdentifier)
                {
                    _WMBIdentifier = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private AlignStateEnum _CardAlignState = AlignStateEnum.IDLE;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public AlignStateEnum CardAlignState
        {
            get { return _CardAlignState; }
            set
            {
                if (value != _CardAlignState)
                {
                    _CardAlignState = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private Element<int> _PatternDistMargin = new Element<int>() { Value = 100 };   // default 값 어떻게 설정할지 논의 필요
        public Element<int> PatternDistMargin
        {
            get { return _PatternDistMargin; }
            set
            {
                if (value != _PatternDistMargin)
                {
                    _PatternDistMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                if (PLFocusParam == null)
                {
                    PLFocusParam = new NormalFocusParameter();
                }

                retval = this.FocusManager().ValidationFocusParam(FocusParam);

                if (retval != EventCodeEnum.NONE)
                {
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_LOW_CAM, EnumAxisConstants.Z, FocusParam, 2000);
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.PIN_LOW_CAM, EnumAxisConstants.PZ, PLFocusParam, 2000);
                }

                if(this.ProbeCardID.Value == null)
                {
                    if (string.IsNullOrEmpty(this.CardID) == false)
                    {
                        this.ProbeCardID.Value = this.CardID;
                    }
                }

                if(GP_PogoMarkPosList3P.Count == 0)
                {
                    GP_PogoMarkPosList3P.Add(new PinCoordinate(124000, 106000, -45665));//==> Z는 SW Neg Limit으로
                    GP_PogoMarkPosList3P.Add(new PinCoordinate(-163000, 16500, -45665));
                    GP_PogoMarkPosList3P.Add(new PinCoordinate(106000, -125000, -45665));
                }

                if(GP_SearchedPogoMarkPosList3P.Count == 0)
                {
                    foreach (PinCoordinate pinPos in GP_PogoMarkPosList3P)
                    {
                        GP_SearchedPogoMarkPosList3P.Add(new PinCoordinate(pinPos.X.Value, pinPos.Y.Value, pinPos.Z.Value));
                    }
                }

                if(CardContactRadiusLimit.Value == 0)
                {
                    CardContactRadiusLimit.Value = 5000.0;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void SetElementMetaData()
        {
            CardChangeType.CategoryID = "10007";
            CardChangeType.ElementName = "CardChangeType";
            CardChangeType.Description = "CardChangeType";
            
            CardDockType.CategoryID = "10007";
            CardDockType.ElementName = "Card Docking Type";
            CardDockType.Description = "Card Docking Type[POGO, DIRECT]";

            PogoAlignPoint.CategoryID = "10007";
            PogoAlignPoint.ElementName = "PogoAlignPoint";
            PogoAlignPoint.Description = "PogoAlignPoint[4POINT, 3POINT]";


            CardDockPosX.CategoryID = "10007";
            CardDockPosX.Description = "Card Docking/Undocking PosX";
            CardDockPosX.ElementName = "Card Docking/Undocking PosX";
            CardDockPosX.UpperLimit = 20000;
            CardDockPosX.LowerLimit = -20000;
            CardDockPosX.Unit = "um";

            CardDockPosY.CategoryID = "10007";
            CardDockPosY.Description = "Card Docking/Undocking PosY";
            CardDockPosY.ElementName = "Card Docking/Undocking PosY";
            CardDockPosY.UpperLimit = 20000;
            CardDockPosY.LowerLimit = -20000;
            CardDockPosY.Unit = "um";

            CardDockPosZ.CategoryID = "10007";
            CardDockPosZ.Description = "Card Docking/Undocking PosZ";
            CardDockPosZ.ElementName = "Card Docking/Undocking PosZ";
            CardDockPosZ.UpperLimit = 500;
            CardDockPosZ.LowerLimit = -85000;
            CardDockPosZ.Unit = "um";

            CardDockPosT.CategoryID = "10007";
            CardDockPosT.Description = "Card Docking/Undocking PosT";
            CardDockPosT.ElementName = "Card Docking/Undocking PosT";
            CardDockPosT.UpperLimit = 5.0;
            CardDockPosT.LowerLimit = -5.0;
            CardDockPosT.Unit = "deg";

            CardHolderCheckPosX.CategoryID = "10007";
            CardHolderCheckPosX.Description = "Card Holder Check PosX";
            CardHolderCheckPosX.ElementName = "Card Holder Check PosX";
            CardHolderCheckPosX.UpperLimit = 20000;
            CardHolderCheckPosX.LowerLimit = -20000;
            CardHolderCheckPosX.Unit = "um";

            CardHolderCheckPosY.CategoryID = "10007";
            CardHolderCheckPosY.Description = "Card Holder Check PosY";
            CardHolderCheckPosY.ElementName = "Card Holder Check PosY";
            CardHolderCheckPosY.UpperLimit = 20000;
            CardHolderCheckPosY.LowerLimit = -20000;
            CardHolderCheckPosY.Unit = "um";

            CardHolderCheckPosZ.CategoryID = "10007";
            CardHolderCheckPosZ.Description = "Card Holder Check PosZ";
            CardHolderCheckPosZ.ElementName = "Card Holder Check PosZ";
            CardHolderCheckPosZ.UpperLimit = 0;
            CardHolderCheckPosZ.LowerLimit = -85000;
            CardHolderCheckPosZ.Unit = "um";

            CardHolderCheckPosT.CategoryID = "10007";
            CardHolderCheckPosT.Description = "Card Holder Check PosT";
            CardHolderCheckPosT.ElementName = "Card Holder Check PosT";
            CardHolderCheckPosT.UpperLimit = 5.0;
            CardHolderCheckPosT.LowerLimit = -5.0;
            CardHolderCheckPosT.Unit = "deg";



            CardTransferOffsetX.CategoryID = "10007";
            CardTransferOffsetX.Description = "Card Transfer Pos. Offset X";
            CardTransferOffsetX.ElementName = "Card Transfer Pos. Offset X";
            CardTransferOffsetX.UpperLimit = 50000;
            CardTransferOffsetX.LowerLimit = -50000;
            CardTransferOffsetX.Unit = "um";

            CardTransferOffsetY.CategoryID = "10007";
            CardTransferOffsetY.Description = "Card Transfer Pos. Offset Y";
            CardTransferOffsetY.ElementName = "Card Transfer Pos. Offset Y";
            CardTransferOffsetY.UpperLimit = 50000;
            CardTransferOffsetY.LowerLimit = -50000;

            CardTransferOffsetZ.Unit = "um";
            CardTransferOffsetZ.CategoryID = "10007";
            CardTransferOffsetZ.Description = "Card Transfer Pos. Offset Z";
            CardTransferOffsetZ.ElementName = "Card Transfer Pos. Offset Z";
            CardTransferOffsetZ.UpperLimit = 10000;
            CardTransferOffsetZ.LowerLimit = -50000;

            CardTransferOffsetT.Unit = "um";
            CardTransferOffsetT.CategoryID = "10007";
            CardTransferOffsetT.Description = "Card Transfer Pos. Offset T";
            CardTransferOffsetT.ElementName = "Card Transfer Pos. Offset T";
            CardTransferOffsetT.UpperLimit = 10000;
            CardTransferOffsetT.LowerLimit = -10000;
            CardTransferOffsetT.Unit = "um";

            CardPodRadiusMax.CategoryID = "10007";
            CardPodRadiusMax.Description = "Available Card Pod Radius Max. Value";
            CardPodRadiusMax.ElementName = "Available Card Pod Radius Max. Value";
            CardPodRadiusMax.UpperLimit = 400000;
            CardPodRadiusMax.LowerLimit = 300000;
            CardPodRadiusMax.Unit = "um";

            CardPodRadiusMin.CategoryID = "10007";
            CardPodRadiusMin.Description = "Available Card Pod Radius Min. Value";
            CardPodRadiusMin.ElementName = "Available Card Pod Radius Min. Value";
            CardPodRadiusMin.UpperLimit = 400000;
            CardPodRadiusMin.LowerLimit = 300000;
            CardPodRadiusMin.Unit = "um";

            CardPodMinHeight.CategoryID = "10007";
            CardPodMinHeight.Description = "Card Pod Minumum Height(Chuck coord)";
            CardPodMinHeight.ElementName = "Card Pod Minumum Height(Chuck coord)";
            CardPodMinHeight.UpperLimit = 1000;
            CardPodMinHeight.LowerLimit = 100000;
            CardPodMinHeight.Unit = "um";

            CardPodCenterX.CategoryID = "10007";
            CardPodCenterX.Description = "Card Pod Center X(Chuck coord)";
            CardPodCenterX.ElementName = "Card Pod Center X(Chuck coord)";
            CardPodCenterX.UpperLimit = 50000;
            CardPodCenterX.LowerLimit = -50000;
            CardPodCenterX.Unit = "um";

            CardPodCenterY.CategoryID = "10007";
            CardPodCenterY.Description = "Card Pod Center Y(Chuck coord)";
            CardPodCenterY.ElementName = "Card Pod Center Y(Chuck coord)";
            CardPodCenterY.UpperLimit = 50000;
            CardPodCenterY.LowerLimit = -50000;
            CardPodCenterY.Unit = "um";

            CardPodCenterZ.CategoryID = "10007";
            CardPodCenterZ.Description = "Card Pod Center Height(Chuck coord)";
            CardPodCenterZ.ElementName = "Card Pod Center Height(Chuck coord)";
            CardPodCenterZ.UpperLimit = 500;
            CardPodCenterZ.LowerLimit = 50000;
            CardPodCenterZ.Unit = "um";

            CardTopFromChuckPlane.CategoryID = "10007";
            CardTopFromChuckPlane.Description = "Distance CardTop From ChuckPlane(Chuck coord)";
            CardTopFromChuckPlane.ElementName = "Distance CardTop From ChuckPlane(Chuck coord)";
            CardTopFromChuckPlane.UpperLimit = 500;
            CardTopFromChuckPlane.LowerLimit = 50000;
            CardTopFromChuckPlane.Unit = "um";

            DD_CardDockZInterval.Unit = "um";
            DD_CardDockZInterval.CategoryID = "10007";
            DD_CardDockZInterval.Description = "DDType CardDock Z Interval";
            DD_CardDockZInterval.ElementName = "DDType CardDock Z Interval";
            DD_CardDockZInterval.UpperLimit = 1000;
            DD_CardDockZInterval.LowerLimit = 100;
            DD_CardDockZInterval.Unit = "um";

            DD_CardDockZMaxValue.Unit = "um";
            DD_CardDockZMaxValue.CategoryID = "10007";
            DD_CardDockZMaxValue.Description = "DDType CardDock Z MaxValue";
            DD_CardDockZMaxValue.ElementName = "DDType CardDock Z MaxValue";
            DD_CardDockZMaxValue.UpperLimit = -50000;
            DD_CardDockZMaxValue.LowerLimit = -70000;
            DD_CardDockZMaxValue.Unit = "um";

            DD_CardUndockZOffeset.Unit = "um";
            DD_CardUndockZOffeset.CategoryID = "10007";
            DD_CardUndockZOffeset.Description = "DDType CardDock Z MaxValue";
            DD_CardUndockZOffeset.ElementName = "DDType CardDock Z MaxValue";
            DD_CardUndockZOffeset.UpperLimit = 3000;
            DD_CardUndockZOffeset.LowerLimit = 500;
            DD_CardUndockZOffeset.Unit = "um";

            OverHeating.CategoryID = "10007";
            OverHeating.Description = "OverHeating offset of CCActivateTemp";
            OverHeating.ElementName = "OverHeating offset of CCActivateTemp";
            OverHeating.UpperLimit = 10;
            OverHeating.LowerLimit = 0;

            OverHeatingHysteresis.CategoryID = "10007";
            OverHeatingHysteresis.Description = "Hysteresis of CCActivateTemp";
            OverHeatingHysteresis.ElementName = "Hysteresis of CCActivateTemp";
            OverHeatingHysteresis.UpperLimit = 5;
            OverHeatingHysteresis.LowerLimit = 0;
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
                CardChangeType.Value = EnumCardChangeType.UNDEFINED;

                GPTesterVacSeqSkip = false;

                GP_LoadingPosT = 0.0;
                GP_CardCheckOffsetZ = 20000;
                GP_ContactCorrectionX = 0;
                GP_ContactCorrectionY = 0;
                GP_ContactCorrectionZ = 0;

                GP_Undock_ContactCorrectionZ = 400;

                GP_Undock_CardContactPosX = 0;
                GP_Undock_CardContactPosY = 0;
                GP_Undock_CardContactPosZ = -70000;
                GP_Undock_CardContactPosT = 0;

                DistanceOffset = 100000;
                // 4Point(기존)
                GP_PogoMarkPosList.Add(new PinCoordinate(89400, 111500, -45665));//==> Z는 SW Neg Limit으로
                GP_PogoMarkPosList.Add(new PinCoordinate(-89400, 111500, -45665));
                GP_PogoMarkPosList.Add(new PinCoordinate(89400, -111500, -45665));
                GP_PogoMarkPosList.Add(new PinCoordinate(-89400, -111500, -45665));

                // 3Point
                GP_PogoMarkPosList3P.Add(new PinCoordinate(124000, 106000, -45665));//==> Z는 SW Neg Limit으로
                GP_PogoMarkPosList3P.Add(new PinCoordinate(-163000, 16500, -45665));
                GP_PogoMarkPosList3P.Add(new PinCoordinate(106000, -125000, -45665));

                GP_CardMarkPosList.Add(new WaferCoordinate(91000, 111000, 14521));//==> Z는 SW Neg Limit으로
                GP_CardMarkPosList.Add(new WaferCoordinate(91000, -111000, 14521));
                GP_CardMarkPosList.Add(new WaferCoordinate(-91000, 111000, 14521));
                GP_CardMarkPosList.Add(new WaferCoordinate(-91000, -111000, 14521));

                // 4Point(기존)
                foreach (PinCoordinate pinPos in GP_PogoMarkPosList)
                {
                    GP_SearchedPogoMarkPosList.Add(new PinCoordinate(pinPos.X.Value, pinPos.Y.Value, pinPos.Z.Value));
                }
                // 3Point
                foreach (PinCoordinate pinPos in GP_PogoMarkPosList3P)
                {
                    GP_SearchedPogoMarkPosList3P.Add(new PinCoordinate(pinPos.X.Value, pinPos.Y.Value, pinPos.Z.Value));
                }

                foreach (WaferCoordinate waferPos in GP_CardMarkPosList)
                {
                    GP_SearchedCardMarkPosList.Add(new WaferCoordinate(waferPos.X.Value, waferPos.Y.Value, waferPos.Z.Value));
                }

                GP_WL_COAXIAL = 100;
                GP_WL_OBLIQUE = 100;
                GP_PL_COAXIAL = 100;
                GP_PL_OBLIQUE = 100;

                PatternMatchingRetryCount = 3;

                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }
                CardDockPosX = new Element<double>();
                CardDockPosX.Value = 0.0;
                CardDockPosY = new Element<double>();
                CardDockPosY.Value = 0.0;

                CardDockPosZ = new Element<double>();
                CardDockPosZ.Value = -20000.0;
                
                CardDockPosT = new Element<double>();
                CardDockPosT.Value = 0.0;


                CardHolderCheckPosX = new Element<double>();
                CardHolderCheckPosX.Value = 0.0;

                CardHolderCheckPosY = new Element<double>();
                CardHolderCheckPosY.Value = 0.0;

                CardHolderCheckPosZ = new Element<double>();
                CardHolderCheckPosZ.Value = -20000.0;

                CardHolderCheckPosT = new Element<double>();
                CardHolderCheckPosT.Value = 0.0;


                MoveToCardHolderPosEnable = false;

                GP_ManualPogoAlignMode = false;

                PogoAlignPoint.Value = EnumPogoAlignPoint.POINT_4;

                PinBaseFiducialMarkInfos = new List<PinBaseFiducialMarkParameter>();
                PinBaseFiducialMarkParameter fidInfo = new PinBaseFiducialMarkParameter();

                var mpccZ = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value;
                var zNegLim = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Param.NegSWLimit.Value;
                fidInfo.FiducialMarkPos = new PinCoordinate(-42000, -335000, zNegLim + mpccZ);
                fidInfo.CardCenterOffset = new PinCoordinate(50000, -151000, 0);
                PinBaseFiducialMarkInfos.Add(fidInfo);

                PinBaseFiducialMarkParameter fidInfo2 = new PinBaseFiducialMarkParameter();
                fidInfo2.FiducialMarkPos = new PinCoordinate(58000, -335000, zNegLim + mpccZ);
                fidInfo2.CardCenterOffset = new PinCoordinate(-50000, -151000, 0);
                PinBaseFiducialMarkInfos.Add(fidInfo2);

                //GP_PogoMarkPosList.Add(new PinCoordinate(89400, 111500, -45665));//==> Z는 SW Neg Limit으로
                //GP_PogoMarkPosList.Add(new PinCoordinate(-89400, 111500, -45665));
                //GP_PogoMarkPosList.Add(new PinCoordinate(89400, -111500, -45665));
                //GP_PogoMarkPosList.Add(new PinCoordinate(-89400, -111500, -45665));

                CardPodMarkPosList.Add(new WaferCoordinate(-165000, 0, 7000));
                CardPodMarkPosList.Add(new WaferCoordinate(
                    Math.Cos(60 / Math.PI * 180) * 16500, Math.Sin(60 / Math.PI * 180) * 16500, 7000));
                CardPodMarkPosList.Add(new WaferCoordinate(
                    Math.Cos(60 / Math.PI * 180) * 16500, Math.Sin(60 / Math.PI * 180) * -16500, 7000));

                CardPodRadiusMax.Value = 300000 * 1.5; // 300mm의 1.5배;
                CardPodRadiusMin.Value = 300000;
                CardPodMinHeight.Value = 5000;

                DD_CardDockZInterval.Value = 1000;
                DD_CardUndockZOffeset.Value = 1000;
                DD_CardDockZMaxValue.Value = -50000;

                CardTopFromChuckPlane = new Element<double>();
                CardTopFromChuckPlane.Value = 30000;

                CardTransferPos.Value = new CatCoordinates();

                ProberCardList = new List<ProberCardListParameter>();
                ProberCardListParameter DefaultCard = new ProberCardListParameter();
                DefaultCard.FiducialMarInfos = new List<PinBaseFiducialMarkParameter>();
                DefaultCard.FiducialMarInfos.Add(fidInfo);
                DefaultCard.FiducialMarInfos.Add(fidInfo2);
                DefaultCard.CardID = "DefaultCard";
                ProberCardList.Add(DefaultCard);

                SnailPointsOdd = 3;

                CardContactRadiusLimit = new Element<double>();
                CardContactRadiusLimit.Value = 5000.0;

                WaitTesterResponse.Value = false;
                PatternDistMargin.Value = 100;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }

    }
}
