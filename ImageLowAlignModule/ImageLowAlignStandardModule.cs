using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageLowAlignStandardModule
{
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Windows;
    using ProberInterfaces.PnpSetup;
    using System.Collections.ObjectModel;
    using PnPControl;
    using System.IO;
    using ProberInterfaces.PinAlign;
    using System.Threading;
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using ProberInterfaces.State;
    using LogModule;
    using ProbeCardObject;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using MetroDialogInterfaces;

    public enum MultipleEnum
    {
        ONE = 1,
        TWO = 2,
        FIVE = 5,
        TEN = 10
    }

    public static class PinLowExtensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
            }
            else
            {
                T[] Arr = (T[])Enum.GetValues(src.GetType());
                int j = Array.IndexOf<T>(Arr, src) + 1;
                return (Arr.Length == j) ? Arr[0] : Arr[j];
            }
        }
    }

    public class ImageLowAlignStandardModule : PNPSetupBase, IProcessingModule, ISetup, IRecovery
    {
        public override Guid ScreenGUID { get; } = new Guid("BB5EEA0F-AF78-B573-5908-3864AB536C25");
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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

        public string PatternbasePath { get; } = "\\PinAlignParam\\PinLowPattern\\";

        public new List<object> Nodes { get; set; }

        public override bool Initialized { get; set; } = false;

        private ObservableCollection<IDut> BackupData;

        private ICamera Cam;

        private double[,] RETRYPOSTABLE;

        private bool _IsFirstPatternReg = false;
        public bool IsFirstPatternReg
        {
            get { return _IsFirstPatternReg; }
            set
            {
                if (value != _IsFirstPatternReg)
                {
                    _IsFirstPatternReg = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSecondPatternReg = false;
        public bool IsSecondPatternReg
        {
            get { return _IsSecondPatternReg; }
            set
            {
                if (value != _IsSecondPatternReg)
                {
                    _IsSecondPatternReg = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFocusing _PinFocusModel;
        public IFocusing PinFocusModel
        {
            get
            {
                if (_PinFocusModel == null)
                    _PinFocusModel = this.FocusManager().GetFocusingModel((this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.FocusingModuleDllInfo);

                return _PinFocusModel;
            }
        }

        private double PatternWidthChangeUnitValue = 4;
        private double PatternHeightChangeUnitValue = 4;

        private double DisplayWidthRatioUmToPixel;
        private double DisplayHeightRatioUmToPixel;

        private MultipleEnum UnitValueMultiple = MultipleEnum.ONE;

        private IFocusParameter FocusParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.FocusParam;

        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        private IPinAligner _PinAligner;
        public IPinAligner PinAligner
        {
            get { return _PinAligner; }
        }

        public new IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }
        public SubModuleMovingStateBase MovingState { get; set; }
        public AlginParamBase Param { get; set; }

        private PinLowAlignPatternOrderEnum _CurPatternIndexEnum
        {
            get
            {
                if (CurPatternIndex == 0)
                {
                    return PinLowAlignPatternOrderEnum.FIRST;
                }
                else
                {
                    return PinLowAlignPatternOrderEnum.SECOND;
                }
            }
        }

        private int _CurPatternIndex;
        public int CurPatternIndex
        {
            get { return _CurPatternIndex; }
            set
            {
                if (value != _CurPatternIndex)
                {
                    _CurPatternIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region DutViewerProperties
        private double? _ZoomLevel;
        public new double? ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowGrid;
        public new bool? ShowGrid
        {
            get { return _ShowGrid; }
            set
            {
                if (value != _ShowGrid)
                {
                    _ShowGrid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPin;
        public new bool? ShowPin
        {
            get { return _ShowPin; }
            set
            {
                if (value != _ShowPin)
                {
                    _ShowPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPad;
        public new bool? ShowPad
        {
            get { return _ShowPad; }
            set
            {
                if (value != _ShowPad)
                {
                    _ShowPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowCurrentPos;
        public new bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set
            {
                if (value != _ShowCurrentPos)
                {
                    _ShowCurrentPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private EnumProberCam _CamType;
        //public EnumProberCam CamType
        //{
        //    get { return _CamType; }
        //    set
        //    {
        //        if (value != _CamType)
        //        {
        //            _CamType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool? _EnableDragMap;
        public new bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set
            {
                if (value != _EnableDragMap)
                {
                    _EnableDragMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowSelectedDut;
        public new bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set
            {
                if (value != _ShowSelectedDut)
                {
                    _ShowSelectedDut = value;
                    RaisePropertyChanged();
                }
            }
        }

        public new IStageSupervisor StageSupervisor
        {
            get { return this.StageSupervisor(); }
        }

        public new IVisionManager VisionManager
        {
            get { return this.VisionManager(); }
        }
        #endregion

        public new EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {

                    _PinAligner = this.PinAligner();
                    //MotionManager = container.Resolve<IMotionManager>();
                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    ProcessingType = EnumSetupProgressState.SKIP;
                    InitPnpModuleStage();
                    MotionManager = this.MotionManager();

                    //RETRYPOSTABLE = new double[9, 2];

                    //RETRYPOSTABLE[0, 0] = 0;
                    //RETRYPOSTABLE[0, 1] = 0;

                    //RETRYPOSTABLE[1, 0] = PinAlignParam.PinLowAlignParam.RetryStepSize.Value;
                    //RETRYPOSTABLE[1, 1] = 0;

                    //RETRYPOSTABLE[2, 0] = PinAlignParam.PinLowAlignParam.RetryStepSize.Value;
                    //RETRYPOSTABLE[2, 1] = -PinAlignParam.PinLowAlignParam.RetryStepSize.Value;

                    //RETRYPOSTABLE[3, 0] = 0;
                    //RETRYPOSTABLE[3, 1] = -PinAlignParam.PinLowAlignParam.RetryStepSize.Value;

                    //RETRYPOSTABLE[4, 0] = -PinAlignParam.PinLowAlignParam.RetryStepSize.Value;
                    //RETRYPOSTABLE[4, 1] = -PinAlignParam.PinLowAlignParam.RetryStepSize.Value;

                    //RETRYPOSTABLE[5, 0] = -PinAlignParam.PinLowAlignParam.RetryStepSize.Value;
                    //RETRYPOSTABLE[5, 1] = 0;

                    //RETRYPOSTABLE[6, 0] = -PinAlignParam.PinLowAlignParam.RetryStepSize.Value;
                    //RETRYPOSTABLE[6, 1] = PinAlignParam.PinLowAlignParam.RetryStepSize.Value;

                    //RETRYPOSTABLE[7, 0] = 0;
                    //RETRYPOSTABLE[7, 1] = PinAlignParam.PinLowAlignParam.RetryStepSize.Value;

                    //RETRYPOSTABLE[8, 0] = PinAlignParam.PinLowAlignParam.RetryStepSize.Value;
                    //RETRYPOSTABLE[8, 1] = PinAlignParam.PinLowAlignParam.RetryStepSize.Value;

                    //retval = LoadDevParameter();

                    //if (retval != EventCodeEnum.NONE)
                    //{
                    //    LoggerManager.Error($"LoadDevParameter() Failed");
                    //}

                    SubModuleState = new SubModuleIdleState(this);

                    MovingState = new SubModuleStopState(this);

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private SubModuleStateBase _AlignModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _AlignModuleState; }
            set
            {
                if (value != _AlignModuleState)
                {
                    _AlignModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }


        public EventCodeEnum DoExecute()
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.PinAlignParam.PinLowAlignEnable.Value == true)
                {
                    bool NeedLowAlignFlag = false;

                    if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.CARD_CHANGE ||
                       this.PinAligner().PinAlignSource == PINALIGNSOURCE.DEVICE_CHANGE ||
                       this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION ||
                       (SubModuleState.GetState() == SubModuleStateEnum.SKIP && PinAlignParam.PinLowAlignRetryAfterPinHighEnable.Value))
                    {
                        NeedLowAlignFlag = true;
                    }

                    if (NeedLowAlignFlag == true)
                    {
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_Low_Mag_Align_Start, EventCodeEnum.NONE);

                        result = LowAlign();

                        if (result == EventCodeEnum.NONE)
                        {
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_Low_Mag_Align_OK, result);

                            if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING && this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                            {
                                //this.MetroDialogManager().ShowMessageDialog("Information", $"Pin low alignment done successfully.", EnumMessageStyle.Affirmative, "OK");
                            }

                            SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                            SubModuleState = new SubModuleDoneState(this);
                        }
                        else
                        {
                            this.PinAligner().PinAlignInfo.AlignResult.Result = result;
                            this.PinAligner().PinAlignInfo.AlignResult.AlignSource = this.PinAligner().PinAlignSource;

                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_Low_Mag_Align_Fail, result);

                            if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING && this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                            {
                                this.MetroDialogManager().ShowMessageDialog("Information", $"Pin low alignment failed.", EnumMessageStyle.Affirmative, "OK");
                            }

                        }

                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                    }
                    else
                    {
                        SubModuleState = new SubModuleSkipState(this);

                        result = EventCodeEnum.NONE;
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_Low_Mag_Align_Skip, result);
                        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                    }
                }
                else
                {
                    result = EventCodeEnum.NONE;

                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_Low_Mag_Align_Skip, result);

                    if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING && this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                    {
                        //this.MetroDialogManager().ShowMessageDialog("Information", $"Pin low alignment skipped.", EnumMessageStyle.Affirmative, "OK");
                    }

                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "DoAlign() : Error occured.");
                LoggerManager.Exception(err);
            }
            finally
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    LoggerManager.Debug($"[ImageLowAlignStandardModule], DoExecute() : Forced Pass.");
                    result = EventCodeEnum.NONE;
                }
            }

            return result;
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }

        private EventCodeEnum MakeRetryTable(double roi_x, double roi_y, double pat_x, double pat_y)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // 인덱스별 위치
                // 입력받은 ROI와 Pattern Size를 이용해 테이블을 만들자.

                //  ――――――――――――――――――――――――――――――――――――――――――――――――
                // ｜              ｜              ｜              ｜
                // ｜      4       ｜      3       ｜       2      ｜
                // ｜              ｜              ｜              ｜
                //  ――――――――――――――――――――――――――――――――――――――――――――――――
                // ｜              ｜              ｜              ｜
                // ｜      5       ｜      0       ｜       1      ｜
                // ｜              ｜ Current Pos  ｜              ｜
                //  ――――――――――――――――――――――――――――――――――――――――――――――――
                // ｜              ｜              ｜              ｜
                // ｜      6       ｜      7       ｜       8      ｜
                // ｜              ｜              ｜              ｜
                //  ――――――――――――――――――――――――――――――――――――――――――――――――

                double xoffset = roi_x - pat_x;
                double yoffset = roi_y - pat_y;

                RETRYPOSTABLE = new double[9, 2];

                RETRYPOSTABLE[0, 0] = 0;
                RETRYPOSTABLE[0, 1] = 0;

                RETRYPOSTABLE[1, 0] = xoffset;
                RETRYPOSTABLE[1, 1] = 0;

                RETRYPOSTABLE[2, 0] = xoffset;
                RETRYPOSTABLE[2, 1] = -yoffset;

                RETRYPOSTABLE[3, 0] = 0;
                RETRYPOSTABLE[3, 1] = -yoffset;

                RETRYPOSTABLE[4, 0] = -xoffset;
                RETRYPOSTABLE[4, 1] = -yoffset;

                RETRYPOSTABLE[5, 0] = -xoffset;
                RETRYPOSTABLE[5, 1] = 0;

                RETRYPOSTABLE[6, 0] = -xoffset;
                RETRYPOSTABLE[6, 1] = yoffset;

                RETRYPOSTABLE[7, 0] = 0;
                RETRYPOSTABLE[7, 1] = yoffset;

                RETRYPOSTABLE[8, 0] = xoffset;
                RETRYPOSTABLE[8, 1] = yoffset;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LowAlign()
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            PMResult pmresult = null;
            PinCoordinate PatternPos = null;

            PinCoordinate NewFirstPatternPos = null;
            PinCoordinate NewSecondPatternPos = null;
            PinCoordinate FocusingPos = null;

            double[] Diff = new double[2];

            InitBackupData();

            string tmpModelFilePath = string.Empty;

            double firstpatternoffsetX = 0.0;
            double firstpatternoffsetY = 0.0;
            double firstpatternoffsetZ = 0.0;

            string SaveFailFocusingPath = string.Empty;
            string SaveFailOriginImageFullPath = string.Empty;
            string SaveFailPatternImageFullPath = string.Empty;

            try
            {
                if (PinAlignParam.PinLowAlignParam.Patterns.Count == 2)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (result == EventCodeEnum.PIN_LOW_PATTERN_FAILED)
                        {
                            break;
                        }

                        PinLowAlignPatternOrderEnum order = PinLowAlignPatternOrderEnum.FIRST;

                        if (i == 1)
                        {
                            order = PinLowAlignPatternOrderEnum.SECOND;
                        }

                        PinLowAlignPatternInfo currentpattinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == order);

                        if (currentpattinfo != null)
                        {
                            double roi_size_x_um = CurCam.GetGrabSizeWidth() * CurCam.GetRatioX();
                            double roi_size_y_um = CurCam.GetGrabSizeHeight() * CurCam.GetRatioY();

                            double pat_size_x_um = currentpattinfo.PMParameter.PattWidth.Value * CurCam.GetRatioX();
                            double pat_size_y_um = currentpattinfo.PMParameter.PattHeight.Value * CurCam.GetRatioY();

                            MakeRetryTable(roi_size_x_um, roi_size_y_um, pat_size_x_um, pat_size_y_um);

                            if (CurCam == null || CurCam.CameraChannel.Type != currentpattinfo.CamType.Value)
                            {
                                CurCam = this.VisionManager().GetCam(currentpattinfo.CamType.Value);
                            }

                            // 더듬더듬
                            for (int PosIndex = 0; PosIndex < 9; PosIndex++)
                            {
                                // Reset original light
                                if (currentpattinfo.LightParams == null)
                                {
                                    result = EventCodeEnum.UNDEFINED;
                                    break;
                                }
                                else
                                {
                                    foreach (var light in currentpattinfo.LightParams)
                                    {
                                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                                    }
                                }

                                bool IsSuccessPatterMatching = false;
                                IDut refDut = this.StageSupervisor().ProbeCardInfo.GetDutFromPinNum(pinNum: this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value);

                                if (refDut != null)
                                {
                                    double pattXPos = currentpattinfo.GetX() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                                    double pattYPos = currentpattinfo.GetY() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                                    double pattZPos = currentpattinfo.GetZ() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                                    double TargetXpos = pattXPos + RETRYPOSTABLE[PosIndex, 0] - firstpatternoffsetX;
                                    double TargetYpos = pattYPos + RETRYPOSTABLE[PosIndex, 1] - firstpatternoffsetY;
                                    double TargetZpos = pattZPos - firstpatternoffsetZ;

                                    LoggerManager.Debug($"Low Key Align : TargetPos (X = {TargetXpos}, Y = {TargetYpos}, Z = {TargetZpos}) = Pattern (X = {pattXPos}, Y = {pattYPos}, Z = {pattZPos}) " +
                                        $"+ Table (X = {RETRYPOSTABLE[PosIndex, 0]}, Y = {RETRYPOSTABLE[PosIndex, 1]}) + offset (X = {firstpatternoffsetX}, Y = {firstpatternoffsetY}, Z = {firstpatternoffsetZ})");

                                    this.StageSupervisor().StageModuleState.PinLowViewMove(TargetXpos, TargetYpos, TargetZpos);
                                    LoggerManager.Debug($"LowAlign(): dut with pin Num 1 not exist.");

                                    var curmachinepos = this.CoordinateManager().PinLowPinConvert.ConvertBack(this.CoordinateManager().PinLowPinConvert.CurrentPosConvert());
                                    LoggerManager.Debug($"[Pin Image Align({order} Encoder Pos => X : {curmachinepos.GetX()}, Y : {curmachinepos.GetY()}, Z : {curmachinepos.GetZ()}]");

                                    FocusParam.FocusingCam.Value = currentpattinfo.CamType.Value;
                                    FocusParam.FocusingROI.Value = new Rect(0, 0, CurCam.GetGrabSizeWidth(), CurCam.GetGrabSizeHeight());

                                    tmpModelFilePath = currentpattinfo.PMParameter.ModelFilePath.Value;

                                    currentpattinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(currentpattinfo.PMParameter.ModelFilePath.Value);

                                    ImageBuffer PatternImg = VisionManager.GetPatternImageInfo(currentpattinfo);

                                    currentpattinfo.PMParameter.ModelFilePath.Value = tmpModelFilePath;

                                    int patgraylevel = 0;

                                    if (PatternImg != null)
                                    {
                                        patgraylevel = PatternImg.GrayLevelValue;
                                    }

                                    SaveFailFocusingPath = this.FileManager().GetImageSavePath(EnumProberModule.PINALIGNER, true, "\\IMAGELOW\\FOCUSING");
                                    if (FocusParam.FocusRange.Value <= 200)
                                    {
                                        var before_range = FocusParam.FocusRange.Value;
                                        FocusParam.FocusRange.Value = 200;
                                        LoggerManager.Debug($"The Focusing Range value is lower than the reference. Before Focusing Range : {before_range}, After Focusing Range: {FocusParam.FocusRange.Value}");
                                    }
                                    result = PinFocusModel.Focusing_Retry(FocusParam, true, false, false, this, TargetGrayLevel: patgraylevel, ForcedApplyAutolight: false, SaveFailPath: SaveFailFocusingPath);

                                    if (result == EventCodeEnum.NONE)
                                    {
                                        FocusingPos = new PinCoordinate(CurCam.GetCurCoordPos());

                                        tmpModelFilePath = currentpattinfo.PMParameter.ModelFilePath.Value;

                                        currentpattinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(currentpattinfo.PMParameter.ModelFilePath.Value);

                                        pmresult = this.VisionManager().PatternMatching(currentpattinfo, this, retryautolight: true);

                                        currentpattinfo.PMParameter.ModelFilePath.Value = tmpModelFilePath;

                                        if (pmresult != null)
                                        {
                                            result = pmresult.RetValue;
                                        }

                                        if (result == EventCodeEnum.NONE)
                                        {
                                            PatternPos = new PinCoordinate(ConvertPosPixelToPin(pmresult));

                                            LoggerManager.Debug($"Low Key Align : Patter matching is success. Pattern Index = {order}, Score = {pmresult.ResultParam[0].Score}, Acceptance = {currentpattinfo.PMParameter.PMAcceptance.Value}", isInfo: true);

                                            // 해당 카메라의 Ratio가 정밀하게 설정되어 있지 않더라도, 움직인 후, 한번 더 패턴 매칭을 진행함으로써, 정밀도를 향상시킬 수 있다.
                                            this.StageSupervisor().StageModuleState.PinLowViewMove(PatternPos.X.Value, PatternPos.Y.Value, FocusingPos.Z.Value);

                                            // 패턴 매칭을 다시하기 위해, 필요한 정보를 다시 넣어줌.
                                            currentpattinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(currentpattinfo.PMParameter.ModelFilePath.Value);

                                            pmresult = this.VisionManager().PatternMatching(currentpattinfo, this, retryautolight: true);

                                            currentpattinfo.PMParameter.ModelFilePath.Value = tmpModelFilePath;

                                            if (pmresult != null)
                                            {
                                                result = pmresult.RetValue;
                                            }

                                            if (result == EventCodeEnum.NONE)
                                            {
                                                LoggerManager.Debug($"Low Key Align : Second Pattern matching is success. Pattern Index = {order}, Score = {pmresult.ResultParam[0].Score}, Acceptance = {currentpattinfo.PMParameter.PMAcceptance.Value}", isInfo: true);

                                                PatternPos = new PinCoordinate(ConvertPosPixelToPin(pmresult));
                                            }
                                            else
                                            {
                                                SaveFailOriginImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "\\IMAGELOW\\PATTERNMATCHING\\", $"{currentpattinfo.PatternOrder.Value}_ORG_Pos#{PosIndex + 1}");
                                                SaveFailPatternImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "\\IMAGELOW\\PATTERNMATCHING\\", $"{currentpattinfo.PatternOrder.Value}_PATTERN_Pos#{PosIndex + 1}");

                                                this.VisionManager().SaveImageBuffer(pmresult.FailOriginImageBuffer, SaveFailOriginImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.PIN_LOWKEY_FAILED);
                                                this.VisionManager().SaveImageBuffer(pmresult.FailPatternImageBuffer, SaveFailPatternImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.PIN_LOWKEY_FAILED);
                                            }

                                            IsSuccessPatterMatching = true;
                                            double SuccessPosX = PatternPos.GetX();
                                            double SuccessPosY = PatternPos.GetY();
                                            double SuccessPosZ = PatternPos.GetZ();

                                            LoggerManager.Debug($"Low Key Align : Relative value X = {SuccessPosX}, Y = {SuccessPosY}, Z = {SuccessPosZ}, Retry Count = {PosIndex}", isInfo: true);

                                            if (order == PinLowAlignPatternOrderEnum.FIRST)
                                            {
                                                firstpatternoffsetX = pattXPos - SuccessPosX;
                                                firstpatternoffsetY = pattYPos - SuccessPosY;
                                                firstpatternoffsetZ = pattZPos - SuccessPosZ;
                                                LoggerManager.Debug($"Low Key Align : FIRST Pattern Offset value X = {firstpatternoffsetX}, Y = {firstpatternoffsetY}, Z = {firstpatternoffsetZ}", isInfo: true);

                                                NewFirstPatternPos = new PinCoordinate(SuccessPosX, SuccessPosY, SuccessPosZ);
                                            }
                                            else if (order == PinLowAlignPatternOrderEnum.SECOND)
                                            {
                                                NewSecondPatternPos = new PinCoordinate(SuccessPosX, SuccessPosY, SuccessPosZ);

                                                result = UpdatePinPosition(NewFirstPatternPos, NewSecondPatternPos);
                                            }
                                        }
                                        else
                                        {
                                            SaveFailOriginImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "\\IMAGELOW\\PATTERNMATCHING\\", $"{currentpattinfo.PatternOrder.Value}_ORG_Pos#{PosIndex + 1}");
                                            SaveFailPatternImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "\\IMAGELOW\\PATTERNMATCHING\\", $"{currentpattinfo.PatternOrder.Value}_PATTERN_Pos#{PosIndex + 1}");

                                            this.VisionManager().SaveImageBuffer(pmresult.FailOriginImageBuffer, SaveFailOriginImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.PIN_LOWKEY_FAILED);
                                            this.VisionManager().SaveImageBuffer(pmresult.FailPatternImageBuffer, SaveFailPatternImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.PIN_LOWKEY_FAILED);

                                            // TODO : Retry Function 
                                            LoggerManager.Debug($"Low Key Align : Patter matching retry (Histogram equlization) process.", isInfo: true);

                                            // (1) Histogram Equlization
                                            ImageBuffer grabbuffer = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);
                                            ImageBuffer HistoEqualbuffer = this.VisionManager().VisionProcessing.Algorithmes.GetHistogramEqualizationImage(grabbuffer, 4);

                                            tmpModelFilePath = currentpattinfo.PMParameter.ModelFilePath.Value;

                                            currentpattinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(currentpattinfo.PMParameter.ModelFilePath.Value);

                                            pmresult = this.VisionManager().PatternMatching(currentpattinfo, this, img: HistoEqualbuffer);

                                            if (pmresult != null)
                                            {
                                                result = pmresult.RetValue;
                                            }

                                            currentpattinfo.PMParameter.ModelFilePath.Value = tmpModelFilePath;

                                            if (result == EventCodeEnum.NONE)
                                            {
                                                PatternPos = new PinCoordinate(ConvertPosPixelToPin(pmresult));

                                                LoggerManager.Debug($"Low Key Align : Patter matching is success. Pattern Index = {order}, Score = {pmresult.ResultParam[0].Score}, Acceptance = {currentpattinfo.PMParameter.PMAcceptance.Value}", isInfo: true);

                                                // 해당 카메라의 Ratio가 정밀하게 설정되어 있지 않더라도, 움직인 후, 한번 더 패턴 매칭을 진행함으로써, 정밀도를 향상시킬 수 있다.
                                                this.StageSupervisor().StageModuleState.PinLowViewMove(PatternPos.X.Value, PatternPos.Y.Value, FocusingPos.Z.Value);

                                                // 패턴 매칭을 다시하기 위해, 필요한 정보를 다시 넣어줌.
                                                currentpattinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(currentpattinfo.PMParameter.ModelFilePath.Value);

                                                pmresult = this.VisionManager().PatternMatching(currentpattinfo, this, retryautolight: true);

                                                currentpattinfo.PMParameter.ModelFilePath.Value = tmpModelFilePath;

                                                if (pmresult != null && pmresult.ResultParam != null && pmresult.ResultParam.Count > 0)
                                                {
                                                    LoggerManager.Debug($"Low Key Align : Second Pattern matching is success. Pattern Index = {order}, Score = {pmresult.ResultParam[0].Score}, Acceptance = {currentpattinfo.PMParameter.PMAcceptance.Value}", isInfo: true);

                                                    PatternPos = new PinCoordinate(ConvertPosPixelToPin(pmresult));
                                                }

                                                double SuccessPosX = PatternPos.GetX();// - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                                                double SuccessPosY = PatternPos.GetY();// - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                                                double SuccessPosZ = PatternPos.GetZ();

                                                LoggerManager.Debug($"Low Key Align : Relative value X = {SuccessPosX}, Y = {SuccessPosY}, Z = {SuccessPosZ}, Retry Count = {PosIndex}", isInfo: true);

                                                if (order == PinLowAlignPatternOrderEnum.FIRST)
                                                {
                                                    firstpatternoffsetX = pattXPos - SuccessPosX;
                                                    firstpatternoffsetY = pattYPos - SuccessPosY;
                                                    firstpatternoffsetZ = pattZPos - SuccessPosZ;
                                                    LoggerManager.Debug($"Low Key Align : FIRST Pattern Offset value X = {firstpatternoffsetX}, Y = {firstpatternoffsetY}, Z = {firstpatternoffsetZ}", isInfo: true);

                                                    NewFirstPatternPos = new PinCoordinate(SuccessPosX, SuccessPosY, SuccessPosZ);
                                                }
                                                else if (order == PinLowAlignPatternOrderEnum.SECOND)
                                                {
                                                    NewSecondPatternPos = new PinCoordinate(SuccessPosX, SuccessPosY, SuccessPosZ);

                                                    result = UpdatePinPosition(NewFirstPatternPos, NewSecondPatternPos);
                                                }
                                            }
                                            else
                                            {
                                                SaveFailOriginImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "\\IMAGELOW\\PATTERNMATCHING\\", $"{currentpattinfo.PatternOrder.Value}_ORG_Pos#{PosIndex + 1}");
                                                SaveFailPatternImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "\\IMAGELOW\\PATTERNMATCHING\\", $"{currentpattinfo.PatternOrder.Value}_PATTERN_Pos#{PosIndex + 1}");

                                                this.VisionManager().SaveImageBuffer(pmresult.FailOriginImageBuffer, SaveFailOriginImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.PIN_LOWKEY_FAILED);
                                                this.VisionManager().SaveImageBuffer(pmresult.FailPatternImageBuffer, SaveFailPatternImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.PIN_LOWKEY_FAILED);

                                                LoggerManager.Debug($"LowAlign Focusing Fail, Position Index = {PosIndex + 1} - Retry (Histogram)");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"LowAlign Focusing Fail, Position Index = {PosIndex + 1}");
                                    }

                                    //패턴 매칭 성공 시, 다음 패턴을 보러 가야 됨.
                                    if (IsSuccessPatterMatching == true)
                                    {
                                        break;
                                    }

                                    // 더듬더듬을 모두 진행했는데도 불구하고, 패턴을 찾지 못함.
                                    if (PosIndex == 8)
                                    {
                                        result = EventCodeEnum.PIN_LOW_PATTERN_FAILED;
                                        break;
                                    }
                                }
                                else
                                {
                                    result = EventCodeEnum.PIN_INVALID_LIST;
                                }
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"Pattern Information is not enough.");

                    result = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                this.VisionManager().StartGrab(Cam.GetChannelType(), this);

                LoggerManager.Exception(err);
            }

            return result;
        }

        private EventCodeEnum UpdatePinPosition(PinCoordinate FirstPatternPos, PinCoordinate SecondPattenPos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            PinCoordinate OldFirstPatternPos = null;
            PinCoordinate OldSecondPatternPos = null;

            PinCoordinate PosOffset = null;
            PinCoordinate PinPos = null;

            PinCoordinate RotatedPos = null;

            double OldAng = 0;
            double NewAng = 0;
            double DiffAng = 0;

            double X1 = 0;
            double X2 = 0;
            double X3 = 0;

            double Y1 = 0;
            double Y2 = 0;
            double Y3 = 0;

            double px = 0;
            double py = 0;

            double[] Incline = new double[2];
            double[] K = new double[2];

            double CurDist = 0;
            double TotalDist = 0;

            double CurZDiff = 0;
            double TotalZDiff = 0;

            bool UpdateLowImgFlatness = true;

            try
            {
                LoggerManager.Debug($"[ImageLowAlgnStandardModule] UpdatePinPosition() : Pin source = {this.PinAligner().PinAlignSource}");

                PinLowAlignPatternInfo FirstPattInfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.FIRST);
                PinLowAlignPatternInfo SecondPattInfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.SECOND);

                // 원래 갖고 있던 정보
                OldFirstPatternPos = new PinCoordinate(FirstPattInfo.GetX(), FirstPattInfo.GetY(), FirstPattInfo.GetZ());
                OldSecondPatternPos = new PinCoordinate(SecondPattInfo.GetX(), SecondPattInfo.GetY(), SecondPattInfo.GetZ());

                LoggerManager.Debug($"Pin center = {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX:0.00}, {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY:0.00}");
                IDut refDut = this.StageSupervisor().ProbeCardInfo.GetDutFromPinNum(pinNum: this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value);
                if (refDut == null)
                {
                    LoggerManager.Debug($"UpdatePinPosition(): dut with pin num {this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value} is not exist.");
                    retVal = EventCodeEnum.PIN_INVALID_LIST;
                    return retVal;
                }

                OldFirstPatternPos.X.Value = OldFirstPatternPos.X.Value + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                OldFirstPatternPos.Y.Value = OldFirstPatternPos.Y.Value + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                OldFirstPatternPos.Z.Value = OldFirstPatternPos.Z.Value + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                OldSecondPatternPos.X.Value = OldSecondPatternPos.X.Value + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                OldSecondPatternPos.Y.Value = OldSecondPatternPos.Y.Value + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                OldSecondPatternPos.Z.Value = OldSecondPatternPos.Z.Value + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                //PinCoordinate UpdatedFirstPos = new PinCoordinate(FirstPatternPos.GetX() + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX,
                //                                                  FirstPatternPos.GetY() + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY,
                //                                                  FirstPatternPos.GetZ());

                //PosOffset = new PinCoordinate(UpdatedFirstPos.GetX() - OldFirstPatternPos.GetX(), UpdatedFirstPos.GetY() - OldFirstPatternPos.GetY(), UpdatedFirstPos.GetZ() - OldFirstPatternPos.GetZ());
                PosOffset = new PinCoordinate(FirstPatternPos.GetX() - OldFirstPatternPos.GetX(), FirstPatternPos.GetY() - OldFirstPatternPos.GetY(), FirstPatternPos.GetZ() - OldFirstPatternPos.GetZ());
                var prevCenterPosX = (OldFirstPatternPos.X.Value + OldSecondPatternPos.X.Value) / 2;
                var currCenterPosX = (FirstPatternPos.X.Value + SecondPattenPos.X.Value) / 2;
                var cenDiffX = currCenterPosX - prevCenterPosX;
                var prevCenterPosY = (OldFirstPatternPos.Y.Value + OldSecondPatternPos.Y.Value) / 2;
                var currCenterPosY = (FirstPatternPos.Y.Value + SecondPattenPos.Y.Value) / 2;
                var cenDiffY = currCenterPosY - prevCenterPosY;
                LoggerManager.Debug($"Offset value : X = {PosOffset.X.Value:0.0}, Y = { PosOffset.Y.Value:0.0}, Z = { PosOffset.Z.Value:0.0}");
                LoggerManager.Debug($"Center diff. value : X = {cenDiffX:0.0}, Y = {cenDiffY:0.0}, Z = { PosOffset.Z.Value:0.0}");

                OldAng = GetAngle(OldFirstPatternPos, OldSecondPatternPos);
                NewAng = GetAngle(FirstPatternPos, SecondPattenPos);
                DiffAng = NewAng - OldAng;
                LoggerManager.Debug($"Diffrence Angle = {DiffAng:0.00000}");

                bool hasIntersection = false;
                bool segIntersection = false;
                PinCoordinate pattIntersection = new PinCoordinate();
                PinCoordinate closeP1 = new PinCoordinate();
                PinCoordinate closeP2 = new PinCoordinate();

                FindIntersection(FirstPatternPos, SecondPattenPos, OldFirstPatternPos, OldSecondPatternPos,
                    out hasIntersection, out segIntersection, out pattIntersection, out closeP1, out closeP2);
                if (hasIntersection == false)
                {
                    retVal = EventCodeEnum.PIN_LOWKEY_FAILED;
                    //throw new Exception("Pin low align exception. Intersection does not exist.");
                    return retVal;
                }

                /*
                PinCoordinate rotPatt1 = new PinCoordinate();
                PinCoordinate rotPatt2 = new PinCoordinate();
                GetRotCoordEx(ref rotPatt1, FirstPatternPos, pattIntersection, -DiffAng);
                GetRotCoordEx(ref rotPatt2, SecondPattenPos, pattIntersection, -DiffAng);
                currCenterPosX = (rotPatt1.X.Value + rotPatt2.X.Value) / 2d;
                currCenterPosY = (rotPatt1.Y.Value + rotPatt2.Y.Value) / 2d;
                */

                //cenDiffX = currCenterPosX - prevCenterPosX;
                //cenDiffY = currCenterPosY - prevCenterPosY;

                InitBackupData();

                LoggerManager.Debug($"Center diff.(Angled) value : X = {cenDiffX:0.0}, Y = {cenDiffY:0.0}, Z = { PosOffset.Z.Value:0.0}");

                foreach (IDut dutdata in BackupData)
                {
                    foreach (IPinData pin in dutdata.PinList)
                    {
                        PinPos = new PinCoordinate(pin.AbsPosOrg);

                        //pin.AbsPosOrg = new PinCoordinate(PinPos.GetX() + (PosOffset.GetX()), PinPos.GetY() + (PosOffset.GetY()), PinPos.GetZ() + PosOffset.GetZ());
                        //pin.AbsPosOrg = new PinCoordinate(PinPos.GetX(), PinPos.GetY(), PinPos.GetZ());
                        GetRotCoordEx(ref RotatedPos, new PinCoordinate(PinPos.GetX(), PinPos.GetY(), PinPos.GetZ()), new PinCoordinate(currCenterPosX, currCenterPosY), DiffAng);

                        LoggerManager.Debug($"Rotate Position of Pin #{pin.PinNum}: ({PinPos.X.Value:0.0}, {PinPos.Y.Value:0.0}, {PinPos.Z.Value:0.0}) -> ({RotatedPos.X.Value:0.0}, {RotatedPos.Y.Value:0.0}, {PinPos.GetZ():0.0})");

                        pin.AbsPosOrg = new PinCoordinate(RotatedPos.GetX() + cenDiffX, RotatedPos.GetY() + cenDiffY, pin.AbsPosOrg.GetZ());

                        LoggerManager.Debug($"Shifted Position of Pin #{pin.PinNum}: ({RotatedPos.X.Value:0.0}, {RotatedPos.Y.Value:0.0}, {PinPos.Z.Value:0.0}) -> ({pin.AbsPosOrg.X.Value:0.0}, {pin.AbsPosOrg.Y.Value:0.0}, {pin.AbsPosOrg.Z.Value:0.0})");
                    }
                }

                X1 = FirstPatternPos.GetX();
                Y1 = FirstPatternPos.GetY();

                X2 = SecondPattenPos.GetX();
                Y2 = SecondPattenPos.GetY();

                if (X2 - X1 == 0 || Y2 - Y1 == 0)
                {
                    UpdateLowImgFlatness = false;
                    //throw new Exception(String.Empty);

                    retVal = EventCodeEnum.PIN_LOWKEY_FAILED;

                    return retVal;
                }
                else
                {
                    Incline[0] = (Y2 - Y1) / (X2 - X1);
                    Incline[1] = -Incline[0];
                }

                IDut refdut = null;
                foreach (IDut dut in BackupData)
                {
                    var tpin = dut.PinList.Find(pin => pin.PinNum.Value == this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value);
                    if (tpin != null)
                    {
                        refdut = dut;
                        break;
                    }
                }

                X3 = refdut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPos.X.Value;
                Y3 = refdut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPos.Y.Value;

                K[0] = Y1 - (Incline[0] * X1);
                //K[1] = Y3 - (Incline[1] * X3);

                if (Incline[0] == 0 || Incline[1] == 0)
                {
                    UpdateLowImgFlatness = false;
                    throw new Exception(String.Empty);
                }

                TotalDist = GetDistance2D(X1, Y1, X2, Y2);
                //TotalZDiff = (SecondPattenPos.GetZ() - FirstPatternPos.GetZ()) - (OldSecondPatternPos.GetZ() - OldFirstPatternPos.GetZ());
                TotalZDiff = (SecondPattenPos.GetZ() + FirstPatternPos.GetZ()) / 2 - (OldSecondPatternPos.GetZ() + OldFirstPatternPos.GetZ()) / 2;
                LoggerManager.Debug($"[PinLow] TotalZDiff= {TotalZDiff}, FirstPatternPos.Z = {FirstPatternPos.GetZ()}, SecondPattenPos.Z = {SecondPattenPos.GetZ()}, OldFirstPatternPos.Z = {OldFirstPatternPos.GetZ()}, OldSecondPatternPos.Z = {OldSecondPatternPos.GetZ()}");
                if (TotalDist == 0)
                {
                    LoggerManager.Debug($"[PinLow] TotalDist= 0");
                    UpdateLowImgFlatness = false;
                }

                if (TotalZDiff == 0)
                {
                    LoggerManager.Debug($"[PinLow] TotalZDiff = 0, No need to update flatness of LowImg.");
                    UpdateLowImgFlatness = false;
                }

                if (UpdateLowImgFlatness == true)
                {
                    foreach (IDut dutdata in BackupData)
                    {
                        foreach (IPinData pin in dutdata.PinList)
                        {
                            PinPos = new PinCoordinate(pin.AbsPos);
                            X3 = pin.AbsPos.X.Value;
                            Y3 = pin.AbsPos.Y.Value;
                            K[1] = Y3 - (Incline[1] * X3);

                            //Cross point
                            px = (K[1] - K[0]) / (Incline[0] - Incline[1]);
                            py = Incline[0] * px + K[0];

                            CurDist = GetDistance2D(X1, Y1, px, py);
                            CurZDiff = TotalZDiff * (CurDist) / (TotalDist);
                            var interpolatedZ = pin.AbsPosOrg.GetZ() + CurZDiff;
                            pin.AbsPosOrg.Z.Value = pin.AbsPosOrg.GetZ() + TotalZDiff;
                            LoggerManager.Debug($"Updated Z Position of Pin #{pin.PinNum}: Z: {pin.AbsPosOrg.Z.Value:0.0}um, Projected Height = {interpolatedZ:0.00}um)");

                            //pin.AbsPos.Z.Value = pin.AbsPos.Z.Value + CurZDiff;
                            //LoggerManager.Debug($"[PinLow] Update Pin by LowImg Dif" + $"f: Pin# {PinPos.GetX()}, {PinPos.GetY()}, {PinPos.GetZ()} ===> {pin.AbsPos.X.Value}, {pin.AbsPos.Y.Value}, {pin.AbsPos.Z.Value}");

                            //LoggerManager.Debug($"[PinLow] Update Pin by Low Img Diff: Pin #{pin.PinNum} Old : ({PinPos.X.Value}, {PinPos.Y.Value}, {PinPos.Z.Value}), New : ({pin.AbsPosOrg.X.Value}, {pin.AbsPosOrg.Y.Value}, {pin.AbsPosOrg.Z.Value})");

                        }
                    }
                }

                double tmpDiffX = 0;
                double tmpDiffY = 0;
                double tmpDiffZ = 0;

                double tmpOldX = 0;
                double tmpOldY = 0;
                double tmpOldZ = 0;

                if (this.PinAligner().PinAlignSource != PINALIGNSOURCE.PIN_REGISTRATION)
                {
                    for (int m = 0; m < BackupData.Count; m++)
                    {
                        for (int n = 0; n < BackupData[m].PinList.Count; n++)
                        {
                            PinData Old = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[m].PinList[n] as PinData;
                            PinData New = BackupData[m].PinList[n] as PinData;

                            tmpDiffX = 0;
                            tmpDiffY = 0;
                            tmpDiffZ = 0;

                            tmpDiffX = (New.AbsPosOrg.X.Value - Old.AbsPosOrg.X.Value) * 1.0;
                            tmpDiffY = (New.AbsPosOrg.Y.Value - Old.AbsPosOrg.Y.Value) * 1.0;
                            tmpDiffZ = (New.AbsPosOrg.Z.Value - Old.AbsPosOrg.Z.Value) * 1.0;

                            //tmpDiffX = (Old.AbsPosOrg.X.Value - New.AbsPosOrg.X.Value);
                            //tmpDiffY = (Old.AbsPosOrg.Y.Value - New.AbsPosOrg.Y.Value);
                            //tmpDiffZ = (Old.AbsPosOrg.Z.Value - New.AbsPosOrg.Z.Value);

                            tmpOldX = Old.AbsPosOrg.X.Value;
                            tmpOldY = Old.AbsPosOrg.Y.Value;
                            tmpOldZ = Old.AbsPosOrg.Z.Value;

                            // Note
                            // AbsPos has NO Setter! 
                            // If you want update pin position for next pin alignment, adjust aligned offset.
                            //public PinCoordinate AbsPos
                            //{
                            //    get
                            //    {
                            //        PinCoordinate tmpPos = new PinCoordinate();
                            //        tmpPos.X.Value = _AbsPosOrg.X.Value + _AlignedOffset.X.Value;
                            //        tmpPos.Y.Value = _AbsPosOrg.Y.Value + _AlignedOffset.Y.Value;
                            //        tmpPos.Z.Value = _AbsPosOrg.Z.Value + _AlignedOffset.Z.Value;

                            //        return tmpPos;
                            //    }
                            //}

                            //if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.CARD_CHANGE ||
                            //   this.PinAligner().PinAlignSource == PINALIGNSOURCE.DEVICE_CHANGE)
                            //{
                            //    Old.AbsPosOrg.X.Value = Old.AbsPosOrg.X.Value + Old.AlignedOffset.X.Value + tmpDiffX;
                            //    Old.AbsPosOrg.Y.Value = Old.AbsPosOrg.Y.Value + Old.AlignedOffset.Y.Value + tmpDiffY;
                            //    Old.AbsPosOrg.Z.Value = Old.AbsPosOrg.Z.Value + Old.AlignedOffset.Z.Value + tmpDiffZ;

                            //    Old.AlignedOffset.X.Value = 0;
                            //    Old.AlignedOffset.Y.Value = 0;
                            //    Old.AlignedOffset.Z.Value = 0;
                            //}
                            //else
                            //{
                            //    Old.AlignedOffset.X.Value = tmpDiffX;
                            //    Old.AlignedOffset.Y.Value = tmpDiffY;
                            //    Old.AlignedOffset.Z.Value = tmpDiffZ;
                            //}

                            Old.AlignedOffset.X.Value = tmpDiffX;
                            Old.AlignedOffset.Y.Value = tmpDiffY;
                            Old.AlignedOffset.Z.Value = tmpDiffZ;

                            if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.CARD_CHANGE ||
                               this.PinAligner().PinAlignSource == PINALIGNSOURCE.DEVICE_CHANGE)
                            {
                                Old.LowCompensatedOffset.X.Value = tmpDiffX;
                                Old.LowCompensatedOffset.Y.Value = tmpDiffY;
                                Old.LowCompensatedOffset.Z.Value = tmpDiffZ;
                            }
                            else
                            {
                                Old.LowCompensatedOffset.X.Value = 0;
                                Old.LowCompensatedOffset.Y.Value = 0;
                                Old.LowCompensatedOffset.Z.Value = 0;
                            }

                            //Old.AlignedOffset.X.Value += tmpDiffX;
                            //Old.AlignedOffset.Y.Value += tmpDiffY;
                            //Old.AlignedOffset.Z.Value += tmpDiffZ;
                            LoggerManager.Debug($"[PinLow] Shifted Offset = ({tmpDiffX:0.0}, {tmpDiffY:0.0}. {tmpDiffZ:0.0})");
                            LoggerManager.Debug($"[PinLow] Update Pin: Dut#{BackupData[m].DutNumber}, Pin #{Old.PinNum} @({tmpOldX:0.0}, {tmpOldY:0.0}, {tmpOldZ:0.0}) shifted to ({Old.AbsPos.X.Value:0.0}, {Old.AbsPos.Y.Value:0.0}, {Old.AbsPos.Z.Value:0.0})");

                            //if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                            //{
                            //    // PIN_REGISTRATION에서는 업데이트하지 않아야 함.

                            //    //Old.AbsPosOrg.X.Value = Old.AbsPosOrg.X.Value + Old.AlignedOffset.X.Value + tmpDiffX;
                            //    //Old.AbsPosOrg.Y.Value = Old.AbsPosOrg.Y.Value + Old.AlignedOffset.Y.Value + tmpDiffY;
                            //    //Old.AbsPosOrg.Z.Value = Old.AbsPosOrg.Z.Value + Old.AlignedOffset.Z.Value + tmpDiffZ;

                            //    //LoggerManager.Debug($"[PinLow] Update Pin by Low Img Diff: Dut#{BackupData[m].DutNumber} Pin #{Old.PinNum} Old : ({tmpOldX:0.0}, {tmpOldY:0.0}, {tmpOldZ:0.0}), New : ({Old.AbsPosOrg.X.Value:0.0}, {Old.AbsPosOrg.Y.Value:0.0}, {Old.AbsPosOrg.Z.Value:0.0}, Diff : ({tmpDiffX:0.0}, {tmpDiffY:0.0}. {tmpDiffZ:0.0})");
                            //}
                            //else
                            //{

                            //    // Note
                            //    // AbsPos has NO Setter! 
                            //    // If you want update pin position for next pin alignment, adjust aligned offset.
                            //    //public PinCoordinate AbsPos
                            //    //{
                            //    //    get
                            //    //    {
                            //    //        PinCoordinate tmpPos = new PinCoordinate();
                            //    //        tmpPos.X.Value = _AbsPosOrg.X.Value + _AlignedOffset.X.Value;
                            //    //        tmpPos.Y.Value = _AbsPosOrg.Y.Value + _AlignedOffset.Y.Value;
                            //    //        tmpPos.Z.Value = _AbsPosOrg.Z.Value + _AlignedOffset.Z.Value;

                            //    //        return tmpPos;
                            //    //    }
                            //    //}

                            //    Old.AlignedOffset.X.Value = tmpDiffX;
                            //    Old.AlignedOffset.Y.Value = tmpDiffY;
                            //    Old.AlignedOffset.Z.Value = tmpDiffZ;

                            //    //Old.AlignedOffset.X.Value += tmpDiffX;
                            //    //Old.AlignedOffset.Y.Value += tmpDiffY;
                            //    //Old.AlignedOffset.Z.Value += tmpDiffZ;
                            //    LoggerManager.Debug($"[PinLow] Shifted Offset = ({tmpDiffX:0.0}, {tmpDiffY:0.0}. {tmpDiffZ:0.0})");
                            //    LoggerManager.Debug($"[PinLow] Update Pin: Dut#{BackupData[m].DutNumber}, Pin #{Old.PinNum} @({tmpOldX:0.0}, {tmpOldY:0.0}, {tmpOldZ:0.0}) shifted to ({Old.AbsPos.X.Value:0.0}, {Old.AbsPos.Y.Value:0.0}, {Old.AbsPos.Z.Value:0.0})");
                            //}
                        }
                    }
                }
                else
                {
                    FirstPattInfo.X.Value = FirstPatternPos.X.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                    FirstPattInfo.Y.Value = FirstPatternPos.Y.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                    FirstPattInfo.Z.Value = FirstPatternPos.Z.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                    LoggerManager.Debug($"First Pattern Info.: ({FirstPattInfo.X.Value:0.0}, {FirstPattInfo.Y.Value:0.0}, {FirstPattInfo.Z.Value:0.0})");

                    SecondPattInfo.X.Value = SecondPattenPos.X.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                    SecondPattInfo.Y.Value = SecondPattenPos.Y.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                    SecondPattInfo.Z.Value = SecondPattenPos.Z.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                    LoggerManager.Debug($"Second Pattern Info.: ({SecondPattInfo.X.Value:0.0}, {SecondPattInfo.Y.Value:0.0}, {SecondPattInfo.Z.Value:0.0})");
                    LoggerManager.Debug($"[PinLow] Pattern info. updated.");
                }

                retVal = this.StageSupervisor().SaveProberCard();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"Save Probe Card data is failed.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retVal = EventCodeEnum.EXCEPTION;

                //if (err.Message == string.Empty)
                //{
                //    //LoggerManager.Error($err + "UpdatePinPosition() : Over Tolerence.");
                //    LoggerManager.Exception(err);

                //    retVal = EventCodeEnum.PIN_LOW_OVER_TOLERENCE;
                //}
                //else
                //{
                //    //LoggerManager.Error($err + "UpdatePinPosition() : Error occured.");
                //    LoggerManager.Exception(err);

                //    retVal = EventCodeEnum.UNDEFINED;
                //}
            }

            return retVal;
        }

        private PinCoordinate ConvertPosPixelToPin(PMResult pmresult)
        {

            PinCoordinate pcd = (PinCoordinate)this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
            double ptxpos = pmresult.ResultParam[0].XPoss;
            double ptypos = pmresult.ResultParam[0].YPoss;

            double offsetx = 0.0;
            double offsety = 0.0;

            try
            {
                //if (CurCam.GetHorizontalFlip() == FlipEnum.NONE)
                //{
                //    offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
                //}
                //else
                //{
                //    offsetx = ptxpos - (pmresult.ResultBuffer.SizeX / 2);
                //}

                offsetx = ptxpos - (pmresult.ResultBuffer.SizeX / 2);
                offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
                //if (CurCam.GetVerticalFlip() == FlipEnum.NONE)
                //{
                //    offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
                //}
                //else
                //{
                //    offsety = ptypos - (pmresult.ResultBuffer.SizeY / 2);
                //}

                //LoggerManager.Debug($"Image Low Align Pattern Pos => x :{ptxpos}, y : {ptypos}");

                //if (CurCam.GetVerticalFlip() == FlipEnum.NONE)
                //{
                //    offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
                //}
                //else
                //{
                //    offsetx = ptxpos - (pmresult.ResultBuffer.SizeX / 2);
                //}

                //if (CurCam.GetHorizontalFlip() == FlipEnum.NONE)
                //{
                //    offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
                //}
                //else
                //{
                //    offsety = ptypos - (pmresult.ResultBuffer.SizeY / 2);
                //}

                offsetx *= CurCam.GetRatioX();
                offsety *= CurCam.GetRatioY();
                LoggerManager.Debug($"Image Low Align Pattern Pos. on screen x :{ptxpos}, y : {ptypos}, Dist. = ({offsetx:0.00}, {offsety:0.00})");

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "ConvertPosPixelToPin() : Error occured.");
                LoggerManager.Exception(err);

            }


            return new PinCoordinate((pcd.GetX() + offsetx), (pcd.GetY() + offsety), pcd.GetZ(), 1);
        }
        public double GetAngle(PinCoordinate pivot, PinCoordinate point)
        {
            double Degree = 0;
            try
            {
                //==> degree = atan((y2 - cy) / (x2-cx)) - atan((y1 - cy)/(x1-cx)) : 세점사이의 각도 구함
                Degree = Math.Atan2(
                     point.Y.Value - pivot.Y.Value,
                     point.X.Value - pivot.X.Value)
                     * 180 / Math.PI;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetDegree() : Error occured.");
                LoggerManager.Exception(err);

            }
            return Degree;
        }
        private double GetDistance2D(double X1, double Y1, double X2, double Y2)
        {
            double Distance = -1;
            try
            {
                Distance = Math.Sqrt(Math.Pow(X1 - X2, 2) + Math.Pow(Y1 - Y2, 2));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetDistance2D() : Error occured.");
                LoggerManager.Exception(err);

            }


            return Distance;
        }
        private void FindIntersection(
            PinCoordinate p1, PinCoordinate p2, PinCoordinate p3, PinCoordinate p4,
            out bool lines_intersect, out bool segments_intersect,
            out PinCoordinate intersection,
            out PinCoordinate close_p1, out PinCoordinate close_p2)
        {
            // Get the segments' parameters.
            double dx12 = p2.X.Value - p1.X.Value;
            double dy12 = p2.Y.Value - p1.Y.Value;
            double dx34 = p4.X.Value - p3.X.Value;
            double dy34 = p4.Y.Value - p3.Y.Value;

            // Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 =
                ((p1.X.Value - p3.X.Value) * dy34 + (p3.Y.Value - p1.Y.Value) * dx34)
                    / denominator;
            if (double.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PinCoordinate(float.NaN, float.NaN);
                close_p1 = new PinCoordinate(float.NaN, float.NaN);
                close_p2 = new PinCoordinate(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            double t2 =
                ((p3.X.Value - p1.X.Value) * dy12 + (p1.Y.Value - p3.Y.Value) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PinCoordinate(p1.X.Value + dx12 * t1, p1.Y.Value + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PinCoordinate(p1.X.Value + dx12 * t1, p1.Y.Value + dy12 * t1);
            close_p2 = new PinCoordinate(p3.X.Value + dx34 * t2, p3.Y.Value + dy34 * t2);
        }
        private void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            try
            {
                NewPos = new PinCoordinate();

                newx = OriPos.X.Value - RefPos.X.Value;
                newy = OriPos.Y.Value - RefPos.Y.Value;

                NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
                NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetRotCoordEx() : Error occured.");
                LoggerManager.Exception(err);

            }
        }

        public void GetTransformationPin(PinCoordinate crossPin, PinCoordinate orgPin, double degree, out PinCoordinate updatePin)
        {
            if (crossPin == null)
            {
                updatePin = orgPin;
                return;
            }
            //==> 좌표의 회전 변환
            //==> x' = (x-cx)cosθ - (y-cy)sinθ
            //==> y' = (y-cx)sinθ + (y-cy)cosθ
            //double radian = (double)(((degree) / 180) * Math.PI);
            double radian = Math.PI * degree / 180.0;

            //double radian = degree;

            double cosq = Math.Cos(radian);
            double sinq = Math.Sin(radian);
            double sx = orgPin.X.Value - crossPin.X.Value;
            double sy = orgPin.Y.Value - crossPin.Y.Value;
            double rx = (sx * cosq - sy * sinq) + crossPin.X.Value; // 결과 좌표 x
            double ry = (sx * sinq + sy * cosq) + crossPin.Y.Value; // 결과 좌표 y
            updatePin = new PinCoordinate(rx, ry, orgPin.Z.Value);
        }
        private double DegreeToRadian(double angle)
        {
            double degerr = 0;
            try
            {
                degerr = Math.PI * angle / 180.0;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "DegreeToRadian() : Error occured.");
                LoggerManager.Exception(err);
            }

            return degerr;
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum deserialRes = EventCodeEnum.UNDEFINED;

            return Task.FromResult<EventCodeEnum>(deserialRes);
        }
        public override void UpdateLabel()
        {
            try
            {
                if (_IsFirstPatternReg && _IsSecondPatternReg)
                {
                    StepLabel = $"1st : Ok | 2nd : Ok \n Current Pattern Index : {_CurPatternIndexEnum}";
                }
                else if (!_IsFirstPatternReg && _IsSecondPatternReg)
                {
                    StepLabel = $"1st : Not | 2nd : Ok \n Current Pattern Index : {_CurPatternIndexEnum}";
                }
                else if (_IsFirstPatternReg && !_IsSecondPatternReg)
                {
                    StepLabel = $"1st : Ok | 2nd : Not \n Current Pattern Index : {_CurPatternIndexEnum}";
                }
                else if (!_IsFirstPatternReg && !_IsSecondPatternReg)
                {
                    StepLabel = $"1st : Not | 2nd : Not \n Current Pattern Index : {_CurPatternIndexEnum}";
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "UpdateLabel() : Error occured.");
                LoggerManager.Exception(err);

            }
        }

        private EventCodeEnum ChangedData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);

                SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Pin Low Align Pattern Registration";
                retVal = InitPnpModuleStage();
                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                UseUserControl = UserControlFucEnum.PTRECT;

                retVal = CheckPatternExist();

                SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitLightJog(this);
                retVal = await InitSetup();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public async Task<EventCodeEnum> InitSetup()
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitBackupData();

                _CurPatternIndex = 0;

                ushort defaultlightvalue = 85;

                if (Cam == null || Cam.CameraChannel.Type != EnumProberCam.PIN_LOW_CAM)
                {
                    Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                }

                CurCam = Cam;

                DisplayWidthRatioUmToPixel = CurCam.GetGrabSizeWidth() / DisplayPort.StandardOverlayCanvaseWidth;
                DisplayHeightRatioUmToPixel = CurCam.GetGrabSizeHeight() / DisplayPort.StandardOverlayCanvaseHeight;

                // Pin Low Ratio X = 2.7 (x1)
                // Pin Low Ratio Y = 2.7 (x1)

                PatternWidthChangeUnitValue = CurCam.GetRatioX() * DisplayWidthRatioUmToPixel * (int)UnitValueMultiple;
                PatternHeightChangeUnitValue = CurCam.GetRatioY() * DisplayHeightRatioUmToPixel * (int)UnitValueMultiple;

                TargetRectangleWidth = 120;
                TargetRectangleHeight = 120;

                CheckPatternExist();

                //ref pin이 등록되어 있는지 확인 한다.
                var CurDutIndex = 0;
                var CurPinIndex = 0; //ref pin index = 0, number = 1
                var CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);
                IPinData tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                if (tmpPinData != null)
                    CurDutIndex = tmpPinData.DutNumber.Value - 1;
                else
                    CurDutIndex = 0;
                if (CurPinArrayIndex >= 0 && tmpPinData != null && CurDutIndex >= 0)
                {
                    // First Move Position
                    if (IsFirstPatternReg)
                    {
                        PinLowAlignPatternInfo pattinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.FIRST);

                        SetRectInfo(pattinfo.PMParameter.PattWidth.Value, pattinfo.PMParameter.PattHeight.Value);

                        if (pattinfo.LightParams == null)
                        {
                            pattinfo.LightParams = new ObservableCollection<LightValueParam>();
                        }

                        foreach (var light in pattinfo.LightParams)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }


                        double TargetXpos = pattinfo.GetX() + tmpPinData.AbsPosOrg.X.Value;
                        double TargetYpos = pattinfo.GetY() + tmpPinData.AbsPosOrg.Y.Value;
                        double TargetZpos = pattinfo.GetZ() + tmpPinData.AbsPosOrg.Z.Value;

                        if (TargetZpos > this.StageSupervisor().PinMaxRegRange)
                        {
                            await this.MetroDialogManager().ShowMessageDialog($"Pin Low Align", $"The Low pattern position is higher than the PinMaxRegRange value." +
                                $"\nTargetZpos: {Convert.ToInt32(TargetZpos)}, PinMaxRegRange: {this.StageSupervisor().PinMaxRegRange}", EnumMessageStyle.Affirmative);
                        }

                        this.StageSupervisor().StageModuleState.PinLowViewMove(TargetXpos, TargetYpos, TargetZpos);
                    }
                    else if (IsSecondPatternReg)
                    {
                        PinLowAlignPatternInfo pattinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.SECOND);

                        SetRectInfo(pattinfo.PMParameter.PattWidth.Value, pattinfo.PMParameter.PattHeight.Value);

                        if (pattinfo.LightParams == null)
                        {
                            pattinfo.LightParams = new ObservableCollection<LightValueParam>();
                        }

                        foreach (var light in pattinfo.LightParams)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }


                        double TargetXpos = pattinfo.GetX() + tmpPinData.AbsPosOrg.X.Value;
                        double TargetYpos = pattinfo.GetY() + tmpPinData.AbsPosOrg.Y.Value;
                        double TargetZpos = pattinfo.GetZ() + tmpPinData.AbsPosOrg.Z.Value;

                        if (TargetZpos > this.StageSupervisor().PinMaxRegRange)
                        {
                            await this.MetroDialogManager().ShowMessageDialog($"Pin Low Align", $"The Low pattern position is higher than the PinMaxRegRange value." +
                                $"\nTargetZpos: {Convert.ToInt32(TargetZpos)}, PinMaxRegRange: {this.StageSupervisor().PinMaxRegRange}", EnumMessageStyle.Affirmative);
                        }

                        this.StageSupervisor().StageModuleState.PinLowViewMove(TargetXpos, TargetYpos, TargetZpos);
                    }
                    else
                    {
                        LoggerManager.Debug($"[ImageLowAlignStandardModule] InitSetup() : Move To Ref Pin");
                        if (tmpPinData.AbsPos.Z.Value > this.StageSupervisor().PinMaxRegRange)
                        {
                            await this.MetroDialogManager().ShowMessageDialog($"Pin Low Align", $"The Low pattern position is higher than the PinMaxRegRange value." +
                                $"\nTargetZpos: {Convert.ToInt32(tmpPinData.AbsPos.Z.Value)}, PinMaxRegRange: {this.StageSupervisor().PinMaxRegRange}", EnumMessageStyle.Affirmative);
                        }

                        this.StageSupervisor().StageModuleState.PinLowViewMove(tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value, tmpPinData.AbsPos.Z.Value);

                        for (int lightindex = 0; lightindex < Cam.LightsChannels.Count; lightindex++)
                        {
                            Cam.SetLight(Cam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[ImageLowAlignStandardModule] InitSetup() : invalid ref pin position, Move To Center Position");
                    this.StageSupervisor().StageModuleState.PinLowViewMove(0, 0, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinDefaultHeight.Value);
                }

                retVal = InitPNPSetupUI();

                //SetStepSetupState();

                // 현재 랏드 State가 IDLE인 경우, 핀 셋업 모드로 Align이 동작되어야 한다.
                //if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                //{
                this.PinAligner().PinAlignSource = PINALIGNSOURCE.PIN_REGISTRATION;
               // }

                UseUserControl = UserControlFucEnum.PTRECT;

                //CheckPatternExist();
                UpdateLabel();

                MainViewTarget = DisplayPort;
                MiniViewTarget = ProbeCard;

                ShowPad = false;
                ShowPin = true;
                EnableDragMap = true;
                ShowSelectedDut = false;
                ShowGrid = false;
                ZoomLevel = 5;
                ShowCurrentPos = true;

                this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);

                //StageSupervisor.PnpLightJog.InitCameraJog();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
                LoggerManager.Exception(err);

            }


            return retVal;
        }
        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MainViewTarget = DisplayPort;

                PadJogLeftUp.IconSource = null;
                PadJogRightUp.IconSource = null;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";

                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");

                PadJogLeftDown.Caption = "Reg 1st";
                PadJogRightDown.Caption = "Reg 2nd";

                PadJogSelect.IconSource = null;
                PadJogSelect.Caption = $"X {(int)UnitValueMultiple}";
                PadJogSelect.IsEnabled = true;
                PadJogSelect.Command = new AsyncCommand(ChangeUnitValueMultipleCommandFunc);

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;
                PadJogRightDown.IsEnabled = true;
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;
                PadJogLeftDown.IsEnabled = true;

                PadJogLeftUp.Command = MovePatternIndexCommand;
                PadJogRightUp.Command = MovePatternIndexCommand;

                PadJogLeftDown.Command = RegistrationPatternCommand;
                PadJogLeftDown.CommandParameter = PinLowAlignPatternOrderEnum.FIRST;

                PadJogRightDown.Command = RegistrationPatternCommand;
                PadJogRightDown.CommandParameter = PinLowAlignPatternOrderEnum.SECOND;

                PadJogUp.Caption = "+";
                PadJogUp.Command = new RelayCommand(RectHeightSizeUp);

                PadJogDown.Caption = "-";
                PadJogDown.Command = new RelayCommand(RectHeightSizeDown);

                PadJogLeft.Caption = "-";
                PadJogLeft.Command = new RelayCommand(RectWidthSizeDown);

                PadJogRight.Caption = "+";
                PadJogRight.Command = new RelayCommand(RectWidthSizeUp);

                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Visible;
                ThreeButton.Visibility = System.Windows.Visibility.Visible;
                FourButton.Visibility = System.Windows.Visibility.Hidden;
                FiveButton.Visibility = System.Windows.Visibility.Hidden;

                OneButton.Caption = "Align";
                OneButton.Command = AlignCommand;

                TwoButton.IconSource = null;
                TwoButton.Command = DeletePatternCommand;
                TwoButton.Caption = "Delete";

                ThreeButton.IconSource = null;
                ThreeButton.Command = (ICommand)FocusingCommand;
                ThreeButton.Caption = "Focusing";

                UseUserControl = UserControlFucEnum.PTRECT;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private AsyncCommand _AlignCommand;
        public IAsyncCommand AlignCommand
        {
            get
            {
                if (null == _AlignCommand) _AlignCommand = new AsyncCommand(Button1);
                return _AlignCommand;
            }
        }

        private Task Button1()
        {
            try
            {
                DoExecute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _DeletePatternCommand;
        public ICommand DeletePatternCommand
        {
            get
            {
                if (null == _DeletePatternCommand) _DeletePatternCommand = new AsyncCommand(DeletePattern, false);
                return _DeletePatternCommand;
            }
        }

        private async Task DeletePattern()
        {
            try
            {
                PinLowAlignPatternInfo patinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == (PinLowAlignPatternOrderEnum)CurPatternIndex);

                if (patinfo != null)
                {
                    EnumMessageDialogResult answer = EnumMessageDialogResult.UNDEFIND;

                    answer = await this.MetroDialogManager().ShowMessageDialog("Delete", "Do you want to delete the pattern information?", EnumMessageStyle.AffirmativeAndNegative);

                    if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        string ModelPath = this.FileManager().GetDeviceParamFullPath(patinfo.PMParameter.ModelFilePath.Value);

                        if (File.Exists(ModelPath))
                        {
                            File.Delete(ModelPath);
                        }

                        if (File.Exists(ModelPath + ".bmp"))
                        {
                            File.Delete(ModelPath + ".bmp");
                        }

                        PinAlignParam.PinLowAlignParam.Patterns.Remove(patinfo);

                        if (_CurPatternIndexEnum == PinLowAlignPatternOrderEnum.FIRST)
                        {
                            IsFirstPatternReg = false;

                            if (PinAlignParam.PinLowAlignParam.Patterns.Count > 0)
                            {
                                CurPatternIndex = (int)PinLowAlignPatternOrderEnum.SECOND;
                            }

                        }
                        else
                        {
                            IsSecondPatternReg = false;

                            if (PinAlignParam.PinLowAlignParam.Patterns.Count > 0)
                            {
                                CurPatternIndex = (int)PinLowAlignPatternOrderEnum.FIRST;
                            }
                        }

                        UpdateLabel();

                        ChangedData();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _FocusingCommand;
        public IAsyncCommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(CmdFocusing);
                return _FocusingCommand;
            }
        }

        private Task<EventCodeEnum> CmdFocusing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                FocusParam.FocusingCam.Value = CurCam.GetChannelType();
                if (FocusParam.FocusRange.Value <= 200)
                {
                    var before_range = FocusParam.FocusRange.Value;
                    FocusParam.FocusRange.Value = 200;
                    LoggerManager.Debug($"The Focusing Range value is lower than the reference. Before Focusing Range : {before_range}, After Focusing Range: {FocusParam.FocusRange.Value}");
                }
                retVal = PinFocusModel.Focusing_Retry(FocusParam, false, true, false, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void RectHeightSizeUp()
        {
            try
            {
                double ChangeValue = TargetRectangleHeight + PatternHeightChangeUnitValue;

                bool CanChange = (ChangeValue / DisplayHeightRatioUmToPixel) < CurCam.GetGrabSizeHeight() ? true : false;

                if (CanChange == true)
                {
                    TargetRectangleWidth = TargetRectangleWidth;
                    TargetRectangleHeight = ChangeValue;
                }
                else
                {
                    ChangeValue = CurCam.GetGrabSizeWidth() * 0.8;
                    TargetRectangleWidth = TargetRectangleWidth;
                    TargetRectangleHeight = ChangeValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void RectHeightSizeDown()
        {
            try
            {
                double ChangeValue = TargetRectangleHeight - PatternHeightChangeUnitValue;

                bool CanChange = (ChangeValue / DisplayHeightRatioUmToPixel) > 0 ? true : false;

                if (CanChange == true)
                {
                    TargetRectangleWidth = TargetRectangleWidth;
                    TargetRectangleHeight = ChangeValue;
                }
                else
                {
                    ChangeValue = CurCam.GetGrabSizeWidth() * 0.1;
                    TargetRectangleWidth = TargetRectangleWidth;
                    TargetRectangleHeight = ChangeValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void RectWidthSizeUp()
        {
            try
            {
                double ChangeValue = TargetRectangleWidth + PatternWidthChangeUnitValue;

                bool CanChange = (ChangeValue / DisplayWidthRatioUmToPixel) < CurCam.GetGrabSizeWidth() ? true : false;

                if (CanChange == true)
                {
                    TargetRectangleWidth = ChangeValue;
                    TargetRectangleHeight = TargetRectangleHeight;
                }
                else
                {
                    ChangeValue = CurCam.GetGrabSizeWidth() * 0.8;
                    TargetRectangleWidth = ChangeValue;
                    TargetRectangleHeight = TargetRectangleHeight;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void RectWidthSizeDown()
        {
            try
            {
                double ChangeValue = TargetRectangleWidth - PatternWidthChangeUnitValue;

                bool CanChange = (ChangeValue / DisplayWidthRatioUmToPixel) > 0 ? true : false;

                if (CanChange == true)
                {
                    TargetRectangleWidth = ChangeValue;
                    TargetRectangleHeight = TargetRectangleHeight;
                }
                else
                {
                    ChangeValue = CurCam.GetGrabSizeWidth() * 0.1;
                    TargetRectangleWidth = ChangeValue;
                    TargetRectangleHeight = TargetRectangleHeight;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private void SetRectInfo(double PatWidth, double PatHeight)
        {
            try
            {
                TargetRectangleWidth = PatWidth * DisplayWidthRatioUmToPixel;
                TargetRectangleHeight = PatHeight * DisplayHeightRatioUmToPixel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task ChangeUnitValueMultipleCommandFunc()
        {
            try
            {
                MultipleEnum NextMultiple = UnitValueMultiple.Next();

                UnitValueMultiple = NextMultiple;

                PadJogSelect.Caption = $"X {(int)UnitValueMultiple}";

                PatternWidthChangeUnitValue = CurCam.GetRatioX() * DisplayWidthRatioUmToPixel * (int)UnitValueMultiple;
                PatternHeightChangeUnitValue = CurCam.GetRatioY() * DisplayHeightRatioUmToPixel * (int)UnitValueMultiple;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _MovePatternIndexCommand;
        public IAsyncCommand MovePatternIndexCommand
        {
            get
            {
                if (null == _MovePatternIndexCommand) _MovePatternIndexCommand = new AsyncCommand(MovePatternIndexCommandFunc);
                return _MovePatternIndexCommand;
            }
        }

        public Task MovePatternIndexCommandFunc()
        {
            try
            {


                if (PinAlignParam.PinLowAlignParam.Patterns.Count > 0)
                {
                    PinLowAlignPatternInfo currentpatinfo = null;

                    if (PinAlignParam.PinLowAlignParam.Patterns.Count == 1)
                    {
                        currentpatinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == _CurPatternIndexEnum);
                    }
                    else
                    {
                        if (CurPatternIndex == 0)
                        {
                            CurPatternIndex++;
                        }
                        else
                        {
                            CurPatternIndex--;
                        }

                        currentpatinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == _CurPatternIndexEnum);
                    }

                    if (currentpatinfo != null)
                    {
                        if (currentpatinfo.PatternState.Value == PatternStateEnum.READY)
                        {
                            SetRectInfo(currentpatinfo.PMParameter.PattWidth.Value, currentpatinfo.PMParameter.PattHeight.Value);


                            IDut refDut = this.StageSupervisor().ProbeCardInfo.GetDutFromPinNum(pinNum: this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value);
                            if (refDut != null)
                            {
                                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                                {
                                    double TargetXpos = currentpatinfo.GetX() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                                    double TargetYpos = currentpatinfo.GetY() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                                    double TargetZpos = currentpatinfo.GetZ() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                                    this.StageSupervisor().StageModuleState.PinHighViewMove(TargetXpos, TargetYpos, TargetZpos);
                                }
                                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                                {
                                    double TargetXpos = currentpatinfo.GetX() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                                    double TargetYpos = currentpatinfo.GetY() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                                    double TargetZpos = currentpatinfo.GetZ() + refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                                    this.StageSupervisor().StageModuleState.PinLowViewMove(TargetXpos, TargetYpos, TargetZpos);
                                }

                            }

                        }
                    }

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return Task.CompletedTask;
        }

        private AsyncCommand<PinLowAlignPatternOrderEnum> _RegistrationPatternCommand;
        public IAsyncCommand RegistrationPatternCommand
        {
            get
            {
                if (null == _RegistrationPatternCommand) _RegistrationPatternCommand = new AsyncCommand<PinLowAlignPatternOrderEnum>(RegistrationPattern);
                return _RegistrationPatternCommand;
            }
        }

        public async Task RegistrationPattern(PinLowAlignPatternOrderEnum order)
        {
            string msg = String.Empty;
            CancellationTokenSource cs = new CancellationTokenSource();

            try
            {
                EnumMessageDialogResult answer = EnumMessageDialogResult.UNDEFIND;

                if (order == PinLowAlignPatternOrderEnum.FIRST)
                {
                    if (_IsFirstPatternReg)
                    {
                        msg = "Already registration first pattern. Do you want to exchange this pattern?";
                    }
                    else
                    {
                        msg = "Do you want to registration this pattern?";
                    }

                    answer = await this.MetroDialogManager().ShowMessageDialog("First Pattern Registration", msg, EnumMessageStyle.AffirmativeAndNegative);
                }
                else
                {
                    if (_IsSecondPatternReg)
                    {
                        msg = "Already registration second pattern. Do you want to exchange this pattern?";
                    }
                    else
                    {
                        msg = "Do you want to registration this pattern?";
                    }

                    answer = await this.MetroDialogManager().ShowMessageDialog("Second Pattern Registration", msg, EnumMessageStyle.AffirmativeAndNegative);
                }

                if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    CurPatternIndex = (int)order;

                    await RegistPattern();
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "RegistrationSecondPattern() : Error occured.");
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            //});
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //SetNodeSetupState(EnumMoudleSetupState.NONE);
                retVal = await base.Cleanup(parameter);

                if (retVal == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                }

                // 셋업 화면 들어온 뒤, 나가서 메뉴얼로 동작 할 때, PIN REGISTRATION 소스로 동작하는 것을 막음.
                //this.PinAligner().IsChangedSource = false;
                this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Modify()
        {
            EventCodeEnum deserialRes = EventCodeEnum.UNDEFINED;

            return deserialRes;
        }
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = (PinAligner as IHasDevParameterizable).SaveDevParameter();

                if (retval == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"SaveDevParameter() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug($"SaveDevParameter() : Save Fail.");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public EventCodeEnum UpdateData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // Position만 변경할 것.

                for (int i = 0; i < BackupData.Count; i++)
                {
                    for (int j = 0; j < BackupData[i].PinList.Count; j++)
                    {
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPosOrg.X.Value = BackupData[i].PinList[j].AbsPosOrg.X.Value;
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPosOrg.X.Value = BackupData[i].PinList[j].AbsPosOrg.Y.Value;
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[i].PinList[j].AbsPosOrg.X.Value = BackupData[i].PinList[j].AbsPosOrg.Z.Value;
                    }
                }

                retval = this.StageSupervisor().SaveProberCard();

                if (retval == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"SaveDevParameter() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug($"SaveDevParameter() : Save Fail.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ClearData()
        {
            EventCodeEnum Res = EventCodeEnum.NONE;

            try
            {
                if (PinAlignParam.PinLowAlignEnable.Value == true)
                {
                    //this.StateTransition(new ImageLowAlignIdleState(this));
                    CheckPatternExist();
                    if (!IsFirstPatternReg || !IsSecondPatternReg)
                    {
                        Res = EventCodeEnum.NODATA;
                        // Res = SubModuleState.SetErrorState(); 
                        //this.StateTransition(new ImageLowAlignNoDataState(this));
                    }
                    else
                    {
                        Res = EventCodeEnum.NONE;
                        //Res = SubModuleState.SetIdleState();
                        //this.StateTransition(new ImageLowAlignIdleState(this));
                    }

                    if (GetState() == SubModuleStateEnum.SKIP)
                    {
                        ClearState();
                    }
                }
                else
                {
                    Res = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "PreRun() : Error occured.");
                LoggerManager.Exception(err);
            }
            return Res;
        }
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        private async Task RegistPattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                PinCoordinate pos = (PinCoordinate)this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();

                this.StageSupervisor().StageModuleState.PinLowViewMove(pos.GetX(), pos.GetY());

                PinLowAlignPatternInfo patterninfo = new PinLowAlignPatternInfo();
                patterninfo.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();

                patterninfo.PatternOrder.Value = _CurPatternIndexEnum;

                // Pixel
                RegisteImageBufferParam param = GetDisplayPortRectInfo();

                pos = (PinCoordinate)this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                var encoderPos = this.CoordinateManager().PinLowPinConvert.ConvertBack(pos);

                patterninfo.X.Value = pos.X.Value;
                patterninfo.Y.Value = pos.Y.Value;
                patterninfo.Z.Value = pos.Z.Value;

                if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, encoderPos.X.Value) == EventCodeEnum.NONE &&
                    this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, encoderPos.Y.Value) == EventCodeEnum.NONE &&
                    this.MotionManager().CheckSWLimit(this.StageSupervisor().StageModuleState.PinViewAxis, encoderPos.Z.Value) == EventCodeEnum.NONE)
                {
                    param.CamType = this.CurCam.CameraChannel.Type;

                    string RootPath = this.FileManager().GetDeviceParamFullPath();

                    PinAlignParam.PinLowAlignParam.PatternPath = PatternbasePath;

                    string DirPath = RootPath + PinAlignParam.PinLowAlignParam.PatternPath;

                    if (Directory.Exists(System.IO.Path.GetDirectoryName(DirPath)) == false)
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DirPath));
                    }

                    patterninfo.PMParameter = new PMParameter();

                    if (patterninfo.PMParameter.PMCertainty.Value != 100)
                    {
                        patterninfo.PMParameter.PMCertainty.Value = 100;
                    }

                    patterninfo.PMParameter.ModelFilePath.Value = PatternbasePath + "_PLP_" + CurPatternIndex.ToString();
                    patterninfo.PMParameter.PatternFileExtension.Value = ".mmo";
                    //patterninfo.PMParameter.MaskFilePath.Value = 
                    patterninfo.PMParameter.PattWidth.Value = param.Width;
                    patterninfo.PMParameter.PattHeight.Value = param.Height;
                    patterninfo.CamType.Value = param.CamType;

                    if (patterninfo.LightParams == null)
                    {
                        patterninfo.LightParams = new ObservableCollection<LightValueParam>();
                    }

                    patterninfo.LightParams.Clear();

                    foreach (var light in CurCam.LightsChannels)
                    {
                        int val = CurCam.GetLight(light.Type.Value);
                        patterninfo.LightParams.Add(new LightValueParam(light.Type.Value, (ushort)val));
                    }

                    param.PatternPath = RootPath + patterninfo.PMParameter.ModelFilePath.Value + patterninfo.PMParameter.PatternFileExtension.Value;

                    if (File.Exists(param.PatternPath))
                    {
                        File.Delete(param.PatternPath);
                    }

                    // 저장하는데 사용하는 파라미터 전달해주면 됨.
                    this.VisionManager().SavePattern(param);

                    retVal = ValidationTesting(patterninfo);

                    // 포커싱 + 패턴 매칭 성공 후
                    if (retVal != EventCodeEnum.NONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog($"Inappropriate Pattern.", $"Pattern validation failed. Please register another pattern.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        PinLowAlignPatternInfo exist = null;

                        if (CurPatternIndex == 0)
                        {
                            exist = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.FIRST);
                        }
                        else if (CurPatternIndex == 1)
                        {
                            exist = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.SECOND);
                        }
                        else
                        {
                            LoggerManager.Error($"Unknown Error");
                        }

                        if (exist != null)
                        {
                            PinAlignParam.PinLowAlignParam.Patterns.Remove(exist);
                        }

                        patterninfo.PatternState.Value = PatternStateEnum.READY;

                        pos = (PinCoordinate)this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();

                        IDut refDut = this.StageSupervisor().ProbeCardInfo.GetDutFromPinNum(pinNum: this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value);

                        if (refDut == null)
                        {
                            await this.MetroDialogManager().ShowMessageDialog($"Inappropriate Pattern.", $"Pattern validation failed. Please register another pattern.", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            patterninfo.X.Value = pos.X.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.X.Value;
                            patterninfo.Y.Value = pos.Y.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Y.Value;
                            patterninfo.Z.Value = pos.Z.Value - refDut.PinList[this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value - 1].AbsPosOrg.Z.Value;

                            LoggerManager.Debug($"Low Align Key, Pattern Index = {_CurPatternIndexEnum}, Position = ({pos.X.Value}, {pos.Y.Value}, {pos.Z.Value})");
                            LoggerManager.Debug($"Relative value : X = {patterninfo.X.Value}, Y = {patterninfo.Y.Value}, Z = {patterninfo.Y.Value}");

                            PinAlignParam.PinLowAlignParam.Patterns.Add(patterninfo);

                            if (CurPatternIndex == 0)
                            {
                                IsFirstPatternReg = true;
                            }
                            else if (CurPatternIndex == 1)
                            {
                                IsSecondPatternReg = true;
                            }

                            UpdateLabel();

                            ChangedData();
                        }

                        retVal = (PinAligner as IHasDevParameterizable).SaveDevParameter();
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Can not registration.", "Pattern position is out of SW Limit.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

        }

        public EventCodeEnum ValidationTesting(PatternInfomation ptinfo, FocusParameter foucsparam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            PMResult mResult = new PMResult();

            string tmpModuleFilePath = ptinfo.PMParameter.ModelFilePath.Value;

            try
            {
                if (ptinfo.CamType.Value == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(ptinfo.GetX(), ptinfo.GetY(), ptinfo.GetZ());

                }
                else if (ptinfo.CamType.Value == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(ptinfo.GetX(), ptinfo.GetY(), ptinfo.GetZ());
                }

                FocusParam.FocusingCam.Value = CurCam.GetChannelType();

                if (FocusParam.FocusRange.Value <= 200)
                {
                    var before_range = FocusParam.FocusRange.Value;
                    FocusParam.FocusRange.Value = 200;
                    LoggerManager.Debug($"The Focusing Range value is lower than the reference. Before Focusing Range : {before_range}, After Focusing Range: {FocusParam.FocusRange.Value}");
                }

                retVal = PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this);

                if (retVal == EventCodeEnum.NONE)
                {
                    ptinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(ptinfo.PMParameter.ModelFilePath.Value);

                    mResult = this.VisionManager().PatternMatching(ptinfo, this);

                    retVal = mResult.RetValue;

                    if (mResult.RetValue != EventCodeEnum.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog($"Pattern Matching Failed.", $"Score : {mResult.ResultParam[0].Score}", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    LoggerManager.Debug($"Focusing Faild.");
                }

                this.VisionManager().StartGrab(ptinfo.CamType.Value, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "VaildationTesting() : Error occured.");
                LoggerManager.Exception(err);
            }
            finally
            {
                ptinfo.PMParameter.ModelFilePath.Value = tmpModuleFilePath;
            }

            return retVal;
        }

        private EventCodeEnum CheckPatternExist()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                PinLowAlignPatternInfo firstpattinfo = null;
                PinLowAlignPatternInfo secondpattinfo = null;

                firstpattinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.FIRST);
                secondpattinfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.SECOND);

                if (firstpattinfo == null || secondpattinfo == null)
                {
                    LoggerManager.Debug($"[ImageLowAlignStandardModule] CheckPatternExist() : Pattern inforamation is not enought.");

                    if (firstpattinfo != null)
                    {
                        string ModelPath = firstpattinfo.PMParameter.ModelFilePath.Value;

                        if (File.Exists(ModelPath))
                        {
                            File.Delete(ModelPath);
                        }

                        if (File.Exists(ModelPath + ".bmp"))
                        {
                            File.Delete(ModelPath + ".bmp");
                        }

                        PinAlignParam.PinLowAlignParam.Patterns.Remove(firstpattinfo);
                    }

                    if (secondpattinfo != null)
                    {
                        string ModelPath = secondpattinfo.PMParameter.ModelFilePath.Value;

                        if (File.Exists(ModelPath))
                        {
                            File.Delete(ModelPath);
                        }

                        if (File.Exists(ModelPath + ".bmp"))
                        {
                            File.Delete(ModelPath + ".bmp");
                        }

                        PinAlignParam.PinLowAlignParam.Patterns.Remove(secondpattinfo);
                    }

                    _IsFirstPatternReg = false;
                    _IsSecondPatternReg = false;
                }
                else
                {
                    if (firstpattinfo.PatternState.Value != PatternStateEnum.NOTREG &&
                        firstpattinfo.PatternState.Value != PatternStateEnum.FAILED)
                    {
                        _IsFirstPatternReg = true;
                    }
                    else
                    {
                        _IsFirstPatternReg = false;
                    }

                    if (secondpattinfo.PatternState.Value != PatternStateEnum.NOTREG &&
                       secondpattinfo.PatternState.Value != PatternStateEnum.FAILED)
                    {
                        _IsSecondPatternReg = true;
                    }
                    else
                    {
                        _IsSecondPatternReg = false;
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private void InitBackupData()
        {
            try
            {
                if (BackupData != null)
                {
                    BackupData.Clear();
                }
                else
                {
                    BackupData = new ObservableCollection<IDut>();
                }

                foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    Dut tmpdut = new Dut();

                    tmpdut.DutNumber = dut.DutNumber;
                    tmpdut.DutEnable = dut.DutEnable;

                    tmpdut.MacIndex.XIndex = dut.MacIndex.XIndex;
                    tmpdut.MacIndex.YIndex = dut.MacIndex.YIndex;

                    tmpdut.UserIndex.XIndex = dut.UserIndex.XIndex;
                    tmpdut.UserIndex.YIndex = dut.UserIndex.YIndex;

                    tmpdut.RefCorner.X.Value = dut.RefCorner.X.Value;
                    tmpdut.RefCorner.Y.Value = dut.RefCorner.Y.Value;
                    tmpdut.RefCorner.Z.Value = dut.RefCorner.Z.Value;

                    tmpdut.DutSizeLeft = dut.DutSizeLeft;
                    tmpdut.DutSizeTop = dut.DutSizeTop;
                    tmpdut.DutSizeWidth = dut.DutSizeWidth;
                    tmpdut.DutSizeHeight = dut.DutSizeHeight;
                    tmpdut.DutVisibility = dut.DutVisibility;

                    foreach (var pin in dut.PinList)
                    {
                        PinData tmpPin = new PinData(pin);
                        tmpdut.PinList.Add(tmpPin);
                    }

                    BackupData.Add(tmpdut);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public new void DeInitModule()
        {

        }

        public override EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsExecute()
        {
            return true;

            throw new NotImplementedException();
        }
    }

}
