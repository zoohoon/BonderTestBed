using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ProberInterfaces.ViewModel
{
    using ProberErrorCode;
    using ProberInterfaces.Param;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.CardChange;

    [ServiceContract]
    public interface ICardChangeOPVM : INotifyPropertyChanged
    {
        //ICommand SmallRaiseChuckCommand { get; }
        //ICommand SmallDropChuckCommand { get; }
        //ICommand MoveToZClearedCommand { get; }

        //ICommand CardContactSettingZCommand { get; }
        //ICommand SetContactOffsetZValueCommand { get; }

        //IAsyncCommand MoveToLoaderCommand { get; }
        //IAsyncCommand RaisePodCommand { get; }
        //IAsyncCommand DropPodCommand { get; }
        //IAsyncCommand TopPlateSolLockCommand { get; }
        //IAsyncCommand TopPlateSolUnLockCommand { get; }
        //IAsyncCommand PCardPodVacuumOffCommand { get; }
        //IAsyncCommand PCardPodVacuumOnCommand { get; }
        //IAsyncCommand UpPlateTesterCOfftactVacuumOffCommand { get; }
        //IAsyncCommand UpPlateTesterContactVacuumOnCommand { get; }
        //IAsyncCommand DockCardCommand { get; }
        //IAsyncCommand UnDockCardCommand { get; }
        //IAsyncCommand CardAlignCommand { get; }
        //IAsyncCommand PageSwitchCommand { get; }
        //IAsyncCommand CleanUpCommand { get; }
        CardChangeSysparamData GetCardChangeSysParam();
        Task<CardChangeDevparamData> GetCardChangeDevParam();
        CardChangeVacuumAndIOStatus GetCardChangeVacAndIOStatus();
        Task SmallRaiseChuckCommandFunc();
        Task SmallDropChuckCommandFunc();
        void MoveToZClearedCommandFunc();

        void CardContactSettingZCommandFunc();
        Task SetContactOffsetZValueFunc(double offsetz);
        Task SetContactOffsetXValueFunc(double offsetx);
        Task SetContactOffsetYValueFunc(double offsety);
        void CardUndockContactSettingZCommandFunc();
        Task SetAlignRetryCountCommandFunc(int retrycount);
        Task CardFocusRangeSettingCommandFunc(double rangevalue);
        Task ZifToggleCommandFunc();
        Task SetFocusRangeValueCommandFunc(double offsetz);
        Task SetDistanceOffsetCommandFunc(double distanceOffset);
        Task SetCardTopFromChuckPlaneSettingCommandFunc(double distance);
        Task SetUndockContactOffsetZValueCommandFunc(double offstz);

        Task SetCardCenterOffsetX1CommandFunc(double value);
        Task SetCardCenterOffsetX2CommandFunc(double value);
        Task SetCardCenterOffsetY1CommandFunc(double value);
        Task SetCardCenterOffsetY2CommandFunc(double value);

        Task SetCardPodCenterXCommandFunc(double value);
        Task SetCardPodCenterYCommandFunc(double value);

        Task SetCardLoadZLimitCommandFunc(double value);
        Task SetCardLoadZIntervalCommandFunc(double value);
        Task SetCardUnloadZOffsetCommandFunc(double value);



        Task MoveToLoaderCommandFunc();
        Task RaisePodAfterMoveCardAlignPosCommandFunc();
        Task RaisePodCommandFunc();
        Task DropPodCommandFunc();
        Task<EventCodeEnum> ForcedDropPodCommandFunc();
        Task TopPlateSolLockCommandFunc();
        Task TopPlateSolUnLockCommandFunc();
        Task PCardPodVacuumOffCommandFunc();
        Task PCardPodVacuumOnCommandFunc();
        Task UpPlateTesterCOfftactVacuumOffCommandFunc();
        Task UpPlateTesterContactVacuumOnCommandFunc();
        Task UpPlateTesterPurgeAirOffCommandFunc();
        Task UpPlateTesterPurgeAirOnCommandFunc();
        Task PogoVacuumOnCommandFunc();
        Task PogoVacuumOffCommandFunc();
        Task DockCardCommandFunc();
        Task UnDockCardCommandFunc();
        Task CardAlignCommandFunc();
        Task PageSwitchCommandFunc();
        Task CleanUpCommandFunc();
        byte[] GetDockSequences();
        byte[] GetUnDockSequences();
        IAsyncCommand MoveToCenterCommand { get; }
        IAsyncCommand MoveToBackCommand { get; }
        IAsyncCommand MoveToFrontCommand { get; }

        int GetCurBehaviorIdx();

        Task SetWaitForCardPermitEnableFunc(bool toggle);

    }

    [ServiceContract]
    public interface IGPCCObservationMarkPosition
    {
        [DataMember]
        bool ButtonEnable { get; set; }
        [DataMember]
        int Index { get; set; }
        [DataMember]
        String Description { get; set; }
        [OperationContract]
        void MoveToMark();
        [OperationContract]
        Task RegisterPatternCommandFunc();
        [OperationContract]
        Task RegisterPosCommandFunc();

    }
    [ServiceContract]
    public interface ICCObservationVM : INotifyPropertyChanged
    {
        void PatternWidthPlusCommandFunc();
        void PatternWidthMinusCommandFunc();
        void PatternHeightPlusCommandFunc();
        void PatternHeightMinusCommandFunc();

        Task CardSettingCommandFunc();
        Task PogoSettingCommandFunc();
        Task PodSettingCommandFunc();
        Task WaferCamExtendCommandFunc();
        Task WaferCamFoldCommandFunc();
        Task MoveToCenterCommandFunc();
        Task ReadyToGetCardCommandFunc();
        Task RaiseZCommandFunc();
        Task DropZCommandFunc();
        void IncreaseLightIntensityCommandFunc();
        void DecreaseLightIntensityCommandFunc();
        Task RegisterPatternCommandFunc();
        Task PogoAlignPointCommandFunc(EnumPogoAlignPoint point);
        Task RegisterPosCommandFunc();
        Task MoveToMarkCommandFunc();
        //Task SetSelectedMarkPosCommandFunc(IGPCCObservationMarkPosition selectedmarkpos);
        Task SetSelectedMarkPosCommandFunc(int selectedmarkposIdx);
        Task SetSelectLightTypeCommandFunc(EnumLightType lighttype);
        //Task SetSelectLightTypeCommandFunc(EnumLightType lighttype);
        Task SetLightValueCommandFunc(ushort lightvalue);
        Task SetZTickValueCommandFunc(int ztickvalue);
        Task SetZDistanceValueCommandFunc(double zdistance);
        Task SetLightTickValueCommandFunc(int lighttick);
        Task SetMFModelLightsCommandFunc();
        Task SetMFChildLightsCommandFunc();

        Task AlignCommandFunc();
        Task PageSwitchCommandFunc(bool observation);
        Task CleanUpCommandFunc();
        Task<EventCodeEnum> FocusingCommandFunc();
        GPCardChangeVMData GetGPCCData();
        string GetPostion();
        bool CARDSetupBtnEnable
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }
        bool POGOSetupBtnEnable
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }
        EnumLightType SelectedLightType
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }

        double PatternWidth
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }
        double PatternHeight
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }
        int ZTickValue
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }
        double ZDistanceValue
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }
        ushort LightValue
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }
        int LightTickValue
        {
            [OperationContract]
            get;
            [OperationContract]
            set;
        }


    }

    [DataContract]
    public class CtGPCardChangeMainPage
    {
        [DataMember]
        public bool CARDSetupBtnEnable { get; set; }
        [DataMember]
        public bool POGOSetupBtnEnable { get; set; }
        [DataMember]
        public EnumLightType SelectedLightType { get; set; }
        [DataMember]
        public ushort LightValue { get; set; }
        [DataMember]
        public double PatternWidth { get; set; }
        [DataMember]
        public double PatternHeight { get; set; }
        [DataMember]
        public int ZTickValue { get; set; }
        [DataMember]
        public double ZDistanceValue { get; set; }
        [DataMember]
        public int LightTickValue { get; set; }
    }
    [DataContract]
    public class GPCardChangeVMData
    {
        private double _PatternWidth;
        [DataMember]
        public double PatternWidth
        {
            get { return _PatternWidth; }
            set { _PatternWidth = value; }
        }

        private double _PatternHeight;
        [DataMember]
        public double PatternHeight
        {
            get { return _PatternHeight; }
            set { _PatternHeight = value; }
        }

        private ObservableCollection<MarkPosCoord> _MarkPositions;
        [DataMember]
        public ObservableCollection<MarkPosCoord> MarkPositions
        {
            get { return _MarkPositions; }
            set { _MarkPositions = value; }
        }

        private bool _ButtonEnable;
        [DataMember]
        public bool ButtonEnable
        {
            get { return _ButtonEnable; }
            set { _ButtonEnable = value; }
        }

        private EnumProberCam _SelectCam;
        [DataMember]
        public EnumProberCam SelectCam
        {
            get { return _SelectCam; }
            set { _SelectCam = value; }
        }

        private ushort _LightValue;
        [DataMember]
        public ushort LightValue
        {
            get { return _LightValue; }
            set { _LightValue = value; }
        }

        private int _ZTickValue;
        [DataMember]
        public int ZTickValue
        {
            get { return _ZTickValue; }
            set { _ZTickValue = value; }
        }

        private double _ZDistanceValue;
        [DataMember]
        public double ZDistanceValue
        {
            get { return _ZDistanceValue; }
            set { _ZDistanceValue = value; }
        }

        private int _LightTickValue;
        [DataMember]
        public int LightTickValue
        {
            get { return _LightTickValue; }
            set { _LightTickValue = value; }
        }
        private EnumLightType _SelectedLightType;
        [DataMember]
        public EnumLightType SelectedLightType
        {
            get { return _SelectedLightType; }
            set { _SelectedLightType = value; }
        }

        private double _ZActualPos;
        [DataMember]
        public double ZActualPos
        {
            get { return _ZActualPos; }
            set { _ZActualPos = value; }
        }
        private double _PZActualPos;
        [DataMember]
        public double PZActualPos
        {
            get { return _PZActualPos; }
            set { _PZActualPos = value; }
        }
    }
    [DataContract]
    public class MarkPosCoord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private double _XPos;
        [DataMember]
        public double XPos
        {
            get { return _XPos; }
            set
            {
                if (value != _XPos)
                {
                    _XPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YPos;
        [DataMember]
        public double YPos
        {
            get { return _YPos; }
            set
            {
                if (value != _YPos)
                {
                    _YPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ZPos;
        [DataMember]
        public double ZPos
        {
            get { return _ZPos; }
            set
            {
                if (value != _ZPos)
                {
                    _ZPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _TPos;
        [DataMember]
        public double TPos
        {
            get { return _TPos; }
            set
            {
                if (value != _TPos)
                {
                    _TPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index;
        [DataMember]
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    [DataContract]
    public class CardChangeSysparamData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        //private double _GP_CardContactPosZ;
        //[DataMember]
        //public double GP_CardContactPosZ
        //{
        //    get { return _GP_CardContactPosZ; }
        //    set
        //    {
        //        if (value != _GP_CardContactPosZ)
        //        {
        //            _GP_CardContactPosZ = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private double _GP_ContactCorrectionZ;
        [DataMember]
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

        private double _GP_ContactCorrectionX;
        [DataMember]
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

        private double _GP_ContactCorrectionY;
        [DataMember]
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

        private double _GP_UndockCorrectionZ;
        [DataMember]
        public double GP_UndockCorrectionZ
        {
            get { return _GP_UndockCorrectionZ; }
            set
            {
                if (value != _GP_UndockCorrectionZ)
                {
                    _GP_UndockCorrectionZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_DistanceOffset;
        [DataMember]
        public double GP_DistanceOffset
        {
            get { return _GP_DistanceOffset; }
            set
            {
                if (value != _GP_DistanceOffset)
                {
                    _GP_DistanceOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_CardFocusRange;
        [DataMember]
        public double GP_CardFocusRange
        {
            get { return _GP_CardFocusRange; }
            set
            {
                if (value != _GP_CardFocusRange)
                {
                    _GP_CardFocusRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _GP_CardAlignRetryCount;
        [DataMember]
        public int GP_CardAlignRetryCount
        {
            get { return _GP_CardAlignRetryCount; }
            set
            {
                if (value != _GP_CardAlignRetryCount)
                {
                    _GP_CardAlignRetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }






        private double _GP_CardCenterOffsetX1;
        [DataMember]
        public double GP_CardCenterOffsetX1
        {
            get { return _GP_CardCenterOffsetX1; }
            set
            {
                if (value != _GP_CardCenterOffsetX1)
                {
                    _GP_CardCenterOffsetX1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_CardCenterOffsetX2;
        [DataMember]
        public double GP_CardCenterOffsetX2
        {
            get { return _GP_CardCenterOffsetX2; }
            set
            {
                if (value != _GP_CardCenterOffsetX2)
                {
                    _GP_CardCenterOffsetX2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_CardCenterOffsetY1;
        [DataMember]
        public double GP_CardCenterOffsetY1
        {
            get { return _GP_CardCenterOffsetY1; }
            set
            {
                if (value != _GP_CardCenterOffsetY1)
                {
                    _GP_CardCenterOffsetY1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_CardCenterOffsetY2;
        [DataMember]
        public double GP_CardCenterOffsetY2
        {
            get { return _GP_CardCenterOffsetY2; }
            set
            {
                if (value != _GP_CardCenterOffsetY2)
                {
                    _GP_CardCenterOffsetY2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_CardPodCenterX;
        [DataMember]
        public double GP_CardPodCenterX
        {
            get { return _GP_CardPodCenterX; }
            set
            {
                if (value != _GP_CardPodCenterX)
                {
                    _GP_CardPodCenterX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_CardPodCenterY;
        [DataMember]
        public double GP_CardPodCenterY
        {
            get { return _GP_CardPodCenterY; }
            set
            {
                if (value != _GP_CardPodCenterY)
                {
                    _GP_CardPodCenterY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_CardLoadZLimit;
        [DataMember]
        public double GP_CardLoadZLimit
        {
            get { return _GP_CardLoadZLimit; }
            set
            {
                if (value != _GP_CardLoadZLimit)
                {
                    _GP_CardLoadZLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_CardLoadZInterval;
        [DataMember]
        public double GP_CardLoadZInterval
        {
            get { return _GP_CardLoadZInterval; }
            set
            {
                if (value != _GP_CardLoadZInterval)
                {
                    _GP_CardLoadZInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_CardUnloadZOffset;
        [DataMember]
        public double GP_CardUnloadZOffset
        {
            get { return _GP_CardUnloadZOffset; }
            set
            {
                if (value != _GP_CardUnloadZOffset)
                {
                    _GP_CardUnloadZOffset = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _GP_CardAlignPosX;
        [DataMember]
        public double GP_CardAlignPosX
        {
            get { return _GP_CardAlignPosX; }
            set
            {
                if (value != _GP_CardAlignPosX)
                {
                    _GP_CardAlignPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_CardAlignPosY;
        [DataMember]
        public double GP_CardAlignPosY
        {
            get { return _GP_CardAlignPosY; }
            set
            {
                if (value != _GP_CardAlignPosY)
                {
                    _GP_CardAlignPosY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_CardAlignPosT;
        [DataMember]
        public double GP_CardAlignPosT
        {
            get { return _GP_CardAlignPosT; }
            set
            {
                if (value != _GP_CardAlignPosT)
                {
                    _GP_CardAlignPosT = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_DockMatchedPosX;
        [DataMember]
        public double GP_DockMatchedPosX
        {
            get { return _GP_DockMatchedPosX; }
            set
            {
                if (value != _GP_DockMatchedPosX)
                {
                    _GP_DockMatchedPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_DockMatchedPosY;
        [DataMember]
        public double GP_DockMatchedPosY
        {
            get { return _GP_DockMatchedPosY; }
            set
            {
                if (value != _GP_DockMatchedPosY)
                {
                    _GP_DockMatchedPosY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _GP_DockMatchedPosZ;
        [DataMember]
        public double GP_DockMatchedPosZ
        {
            get { return _GP_DockMatchedPosZ; }
            set
            {
                if (value != _GP_DockMatchedPosZ)
                {
                    _GP_DockMatchedPosZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _GP_DockMatchedPosT;
        [DataMember]
        public double GP_DockMatchedPosT
        {
            get { return _GP_DockMatchedPosT; }
            set
            {
                if (value != _GP_DockMatchedPosT)
                {
                    _GP_DockMatchedPosT = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ProberCardListParameter> _ProberCardList;
        [DataMember]
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
        private double _CardTopFromChuckPlane;
        [DataMember]
        public double CardTopFromChuckPlane
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

        private EnumPogoAlignPoint _PogoAlignPoint;
        [DataMember]
        public EnumPogoAlignPoint PogoAlignPoint
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

        private bool _WaitForCardPermitEnable;
        [DataMember]
        public bool WaitForCardPermitEnable
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

        private EnumCardChangeType _CardChangeType;
        [DataMember]
        public EnumCardChangeType CardChangeType
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
    }

    [DataContract]
    public class CardChangeDevparamData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        private double _GP_CardContactPosZ;
        [DataMember]
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
    }

    [DataContract]
    public class CardChangeVacuumAndIOStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private bool _CardOnPogoPod;
        [DataMember]
        public bool CardOnPogoPod
        {
            get { return _CardOnPogoPod; }
            set
            {
                if (value != _CardOnPogoPod)
                {
                    _CardOnPogoPod = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TesterPogoTouched;
        [DataMember]
        public bool TesterPogoTouched
        {
            get { return _TesterPogoTouched; }
            set
            {
                if (value != _TesterPogoTouched)
                {
                    _TesterPogoTouched = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CardPogoTouched;
        [DataMember]
        public bool CardPogoTouched
        {
            get { return _CardPogoTouched; }
            set
            {
                if (value != _CardPogoTouched)
                {
                    _CardPogoTouched = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCardLatchLock;
        [DataMember]
        public bool IsCardLatchLock
        {
            get { return _IsCardLatchLock; }
            set
            {
                if (value != _IsCardLatchLock)
                {
                    _IsCardLatchLock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCardLatchUnLock;
        [DataMember]
        public bool IsCardLatchUnLock
        {
            get { return _IsCardLatchUnLock; }
            set
            {
                if (value != _IsCardLatchUnLock)
                {
                    _IsCardLatchUnLock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLeftUpModuleUp;
        [DataMember]
        public bool IsLeftUpModuleUp
        {
            get { return _IsLeftUpModuleUp; }
            set
            {
                if (value != _IsLeftUpModuleUp)
                {
                    _IsLeftUpModuleUp = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsRightUpModuleUp;
        [DataMember]
        public bool IsRightUpModuleUp
        {
            get { return _IsRightUpModuleUp; }
            set
            {
                if (value != _IsRightUpModuleUp)
                {
                    _IsRightUpModuleUp = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsCardExistOnCardPod;
        [DataMember]
        public bool IsCardExistOnCardPod
        {
            get { return _IsCardExistOnCardPod; }
            set
            {
                if (value != _IsCardExistOnCardPod)
                {
                    _IsCardExistOnCardPod = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsTesterMotherBoardConnected;
        [DataMember]
        public bool IsTesterMotherBoardConnected
        {
            get { return _IsTesterMotherBoardConnected; }
            set
            {
                if (value != _IsTesterMotherBoardConnected)
                {
                    _IsTesterMotherBoardConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsTesterPCBUnlocked;
        [DataMember]
        public bool IsTesterPCBUnlocked
        {
            get { return _IsTesterPCBUnlocked; }
            set
            {
                if (value != _IsTesterPCBUnlocked)
                {
                    _IsTesterPCBUnlocked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsThreeLegDown;
        [DataMember]
        public bool IsThreeLegDown
        {
            get { return _IsThreeLegDown; }
            set
            {
                if (value != _IsThreeLegDown)
                {
                    _IsThreeLegDown = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsThreeLegUp;
        [DataMember]
        public bool IsThreeLegUp
        {
            get { return _IsThreeLegUp; }
            set
            {
                if (value != _IsThreeLegUp)
                {
                    _IsThreeLegUp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _WaferOnChuck;
        [DataMember]
        public bool WaferOnChuck
        {
            get { return _WaferOnChuck; }
            set
            {
                if (value != _WaferOnChuck)
                {
                    _WaferOnChuck = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCamExtended;
        [DataMember]
        public bool IsCamExtended
        {
            get { return _IsCamExtended; }
            set
            {
                if (value != _IsCamExtended)
                {
                    _IsCamExtended = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

}
