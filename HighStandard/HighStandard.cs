using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WAHighStandardModule
{
    using LogModule;
    using MetroDialogInterfaces;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Align;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using ProberInterfaces.WaferAligner;
    using ProberInterfaces.WaferAlignEX;
    using RelayCommandBase;
    using SerializerUtil;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using ThetaAlignStandatdModule;
    using WA_HighMagParameter_Standard;

    public class HighStandard : ThetaAlignStandard, ISetup, IRecovery, IParamNode, IProcessingModule, IHasDevParameterizable, ILotReadyAble, IPackagable, IWaferHighProcModule
    {
        enum WAHighSetupFunction
        {
            UNDIFINE = -1,
            REGPATTERN,
            DELETEPATTERN,
            FOCUSING,
            TEMPORARY_REGPATTERN,
            TEMPORARY_DELETEPATTERN
        }

        private IFocusing _WaferHighFocusModel;
        public IFocusing WaferHighFocusModel
        {
            get
            {
                if (_WaferHighFocusModel == null)
                {
                    if (this.FocusManager() != null)
                    {
                        _WaferHighFocusModel = this.FocusManager().GetFocusingModel((HighStandard_IParam as WA_HighMagParam_Standard).FocusingModuleDllInfo);
                    }
                }


                return _WaferHighFocusModel;
            }
        }
        private IFocusParameter FocusParam => (HighStandard_IParam as WA_HighMagParam_Standard).FocusParam;

        public override Guid ScreenGUID { get; } = new Guid("91AF5353-D69F-CAE4-DA09-7AE6BCF9B789");
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


        #region ..//Property

        public override bool Initialized { get; set; } = false;

        private WAHighSetupFunction ModifyCondition;
        //private IFocusing FocusingModule { get; set; }
        //private IFocusParameter FocusingParam { get; set; }

        public WA_HighMagParam_Standard HighStandardParam_Clone { get; set; }
        public List<WaferProcResult> ProcResults;

        private double ShortMiniumumLength;
        private double ShortOptimumLength;
        private double ShortMaximumLength;

        private double MinimumLength;
        private double OptimumLength;
        private double MaximumLength;

        private int temppatternCount = 0;
        private int patternCount = 0;
        private int patternindex = -1;

        public double RotateAngle = 0.0;

        private CancellationTokenSource _OperCancelTokenSource = null;

        //public WA_HighStandard_JumpIndex_ManualParam JumpIndexManualInputParam
        //     = new WA_HighStandard_JumpIndex_ManualParam();
        //public PMParameter DefaultPMParam
        //    = new PMParameter();

        private List<WAHeightPositionParam> _HeightPositions = new List<WAHeightPositionParam>();


        private SubModuleStateBase _AlignModuleState;
        public override SubModuleStateBase SubModuleState
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

        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
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
                    ActiveStepLabelChanged();
                }
            }
        }

        private int _CurTempPatternIndex;
        public int CurTempPatternIndex
        {
            get { return _CurTempPatternIndex; }
            set
            {
                if (value != _CurTempPatternIndex)
                {
                    _CurTempPatternIndex = value;
                    RaisePropertyChanged();
                    ActiveStepLabelChanged();
                }
            }
        }
        private void ActiveStepLabelChanged()
        {
            try
            {
                if (CurTempPatternIndex > 0)
                {
                    StepSecondLabelActive = true;

                    if (MiniViewTarget is ImageBuffer)
                    {
                        ViewPattern();
                    }
                }
                else
                {
                    StepSecondLabelActive = false;
                }

                if (CurPatternIndex > 0)
                {
                    StepLabelActive = true;

                    if (MiniViewTarget is ImageBuffer)
                    {
                        ViewPattern();
                    }
                }
                else
                {
                    StepLabelActive = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private IParam _HighStandard_IParam;
        [ParamIgnore]
        public IParam HighStandard_IParam
        {
            get { return _HighStandard_IParam; }
            set
            {
                if (value != _HighStandard_IParam)
                {
                    _HighStandard_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SubModuleMovingStateBase MovingState { get; set; }
        public VerifyStandardStepState VerifyStepState { get; set; }
        public WaferProcResult VerifyProcResult;
        public List<VerifyInfo> VerifyInofs = new List<VerifyInfo>();
        #endregion

        #region Command        
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Wafer.GetPhysInfo().Thickness.Value != Wafer.GetSubsInfo().ActualThickness)
                {
                    Wafer.GetPhysInfo().Thickness.Value = Math.Round(Wafer.GetSubsInfo().ActualThickness, 3);
                    Wafer.SaveDevParameter();
                }

                UseUserControl = UserControlFucEnum.DEFAULT;
                StepLabelActive = false;
                StepSecondLabelActive = false;

                this.WaferAligner().WaferAlignInfo.HighMeasurementTable.HeightPoint
                        = HighStandardParam_Clone.HeightProfilingPointType.Value;
                //retVal = ApplyHeightProfiling();

                if (parameter is EventCodeEnum)
                {
                    if ((EventCodeEnum)parameter == EventCodeEnum.NONE | IsParameterChanged())
                        await base.Cleanup(parameter);
                    return retVal;
                }

                retVal = await base.Cleanup(parameter);


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }



        private AsyncCommand _RegistePatternCommand;
        public ICommand RegistePatternCommand
        {
            get
            {
                if (null == _RegistePatternCommand) _RegistePatternCommand = new AsyncCommand(
                    CmdRegistePattern);
                return _RegistePatternCommand;
            }
        }

        private async Task CmdRegistePattern()
        {
            try
            {
                patternindex = CurPatternIndex;
                ModifyCondition = WAHighSetupFunction.REGPATTERN;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DeletePatternCommand;
        public ICommand DeletePatternCommand
        {
            get
            {
                if (null == _DeletePatternCommand) _DeletePatternCommand = new AsyncCommand(
                    CmdDeletePattern);
                return _DeletePatternCommand;
            }
        }

        private async Task CmdDeletePattern()
        {
            try
            {
                ModifyCondition = WAHighSetupFunction.DELETEPATTERN;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand _Temporary_RegistePatternCommand;
        public ICommand Temporary_RegistePatternCommand
        {
            get
            {
                if (null == _Temporary_RegistePatternCommand) _Temporary_RegistePatternCommand = new AsyncCommand(
                    Cmd_Temporary_RegistePattern);
                return _Temporary_RegistePatternCommand;
            }
        }

        private async Task Cmd_Temporary_RegistePattern()
        {
            try
            {
                patternindex = CurPatternIndex;
                ModifyCondition = WAHighSetupFunction.TEMPORARY_REGPATTERN;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _Temporary_DeletePatternCommand;
        public ICommand Temporary_DeletePatternCommand
        {
            get
            {
                if (null == _Temporary_DeletePatternCommand) _Temporary_DeletePatternCommand = new AsyncCommand(
                    Cmd_Temporary_DeletePattern);
                return _Temporary_DeletePatternCommand;
            }
        }

        private async Task Cmd_Temporary_DeletePattern()
        {
            try
            {
                ModifyCondition = WAHighSetupFunction.TEMPORARY_DELETEPATTERN;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task FocusingCommand()
        {
            try
            {
                ModifyCondition = WAHighSetupFunction.FOCUSING;

                await Modify();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _FocusingGraphViewCommand;
        public ICommand FocusingGraphViewCommand
        {
            get
            {
                if (null == _FocusingGraphViewCommand) _FocusingGraphViewCommand = new RelayCommand(
                    FocusingGraphView);
                return _FocusingGraphViewCommand;
            }
        }

        public void FocusingGraphView()
        {
            WaferHighFocusModel.ShowFocusGraph();
        }

        private Task Next()
        {
            try
            {
                if (CurPatternIndex + 1 <= HighStandardParam_Clone.Patterns.Value.Count)
                {
                    CurPatternIndex++;

                    WAStandardPTInfomation ptinfo = HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1];

                    StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY(), ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.GetZ());

                    UpdatePatternSize(ptinfo, CurPatternIndex - 1);

                    foreach (var light in HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].LightParams)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
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

        private Task Prev()
        {
            try
            {
                if (CurPatternIndex - 1 > 0)
                {
                    CurPatternIndex--;
                    StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);

                    WAStandardPTInfomation ptinfo = HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1];

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY(), ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.GetZ());

                    UpdatePatternSize(ptinfo, CurPatternIndex - 1);

                    foreach (var light in HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].LightParams)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLockViewControl(this.GetHashCode());
            }
            return Task.CompletedTask;
        }

        private Task RecoverySetup_Next()
        {
            try
            {
                WAStandardPTInfomation obj = null;

                int patidx = 0;

                if (CurTempPatternIndex > 0)
                {
                    if (CurTempPatternIndex < temppatternCount)
                    {
                        CurTempPatternIndex++;

                        patidx = CurTempPatternIndex - 1;
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer[patidx]);
                    }
                    else
                    {
                        CurPatternIndex++;
                        CurTempPatternIndex = 0;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(HighStandardParam_Clone.Patterns.Value[patidx]);
                    }
                }
                else if (CurPatternIndex > 0)
                {
                    if (CurPatternIndex < patternCount)
                    {
                        CurPatternIndex++;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(HighStandardParam_Clone.Patterns.Value[patidx]);
                    }
                    else if (temppatternCount > 0)
                    {
                        CurPatternIndex = 0;
                        CurTempPatternIndex = 1;

                        patidx = CurTempPatternIndex - 1;
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer[patidx]);
                    }
                    else
                    {
                        CurPatternIndex = 1;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(HighStandardParam_Clone.Patterns.Value[patidx]);
                    }
                }
                else if (CurPatternIndex == 0 && CurPatternIndex == 0)
                {
                    if (temppatternCount > 0)
                    {
                        CurTempPatternIndex++;

                        patidx = CurTempPatternIndex - 1;
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer[patidx]);

                    }
                    else
                    {
                        CurPatternIndex++;
                        CurTempPatternIndex = 0;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(HighStandardParam_Clone.Patterns.Value[patidx]);
                    }

                }

                StepLabel = String.Format("REGISTED PATTERN {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);

                if (obj != null)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        obj.GetX() + this.WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        obj.GetY() + this.WaferObject.GetSubsInfo().WaferCenter.GetY(),
                        obj.GetZ() + this.WaferObject.GetSubsInfo().WaferCenter.GetZ());

                    UpdatePatternSize(obj, patidx);

                    foreach (var light in obj.LightParams)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        private Task RecoverySetup_Prev()
        {
            try
            {
                WAStandardPTInfomation obj = null;

                int patidx = 0;

                if (CurTempPatternIndex > 1)
                {
                    CurTempPatternIndex--;

                    patidx = CurTempPatternIndex - 1;
                    obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer[patidx]);
                }
                else if (CurPatternIndex > 1)
                {
                    CurPatternIndex--;

                    patidx = CurPatternIndex - 1;
                    obj = new WAStandardPTInfomation(HighStandardParam_Clone.Patterns.Value[patidx]);

                }
                else if (temppatternCount > 0)
                {
                    if (CurPatternIndex == 1)
                    {
                        CurPatternIndex--;
                        CurTempPatternIndex = temppatternCount;

                        patidx = CurTempPatternIndex - 1;
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer[patidx]);
                    }
                    else if (CurTempPatternIndex == 1)
                    {
                        CurTempPatternIndex = 0;
                        CurPatternIndex = HighStandardParam_Clone.Patterns.Value.Count;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(HighStandardParam_Clone.Patterns.Value[patidx]);
                    }
                }
                else
                {
                    CurPatternIndex = HighStandardParam_Clone.Patterns.Value.Count;

                    patidx = CurPatternIndex - 1;
                    obj = new WAStandardPTInfomation(HighStandardParam_Clone.Patterns.Value[patidx]);
                }

                StepLabel = String.Format("REGISTED PATTERN  {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);

                if (obj != null)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        obj.GetX() + this.WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        obj.GetY() + this.WaferObject.GetSubsInfo().WaferCenter.GetY(),
                        obj.GetZ() + this.WaferObject.GetSubsInfo().WaferCenter.GetZ());

                    UpdatePatternSize(obj, patidx);

                    foreach (var light in obj.LightParams)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }
        private void PatternRegisteAbort()
        {
            if (_OperCancelTokenSource != null)
                _OperCancelTokenSource.Cancel();
            this.VisionManager().StopWaitGrab(CurCam);
        }
        public Task ViewPattern()
        {
            try
            {
                ImageBuffer targetimage = null;
                if (CurTempPatternIndex >= 1)
                {
                    targetimage = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer[CurTempPatternIndex - 1].Imagebuffer;
                    if (targetimage == null)
                    {
                        //minivew에 image unavaliable 이미지를 띄운다 ? 
                        SetMiniViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/ImageNotFound.bmp");
                    }
                    else
                    {
                        MiniViewTarget = targetimage;
                        ImgBuffer = targetimage;
                    }
                }
                else if (CurPatternIndex >= 1)
                {
                    if (HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].Imagebuffer == null)
                    {
                        HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].Imagebuffer = this.VisionManager().LoadImageFile(
                        this.FileManager().GetDeviceParamFullPath(HighStandardParam_Clone.PatternbasePath, HighStandardParam_Clone.PatternName + (CurPatternIndex - 1).ToString() + HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].PMParameter.PatternFileExtension.Value));
                    }
                    targetimage = HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].Imagebuffer;

                    MiniViewTarget = targetimage;
                    ImgBuffer = targetimage;
                }
                else
                {
                    //miniviewtarget set 하지 않음.
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
        public async Task Apply()
        {
            try
            {
                //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "High Align");


                ClearData();
                var ret = Execute();
                SetStepSetupState();
                SetNextStepsNotCompleteState((PnpManager.SelectedPnpStep as ICategoryNodeItem).Header);

                if (this.GetState() == SubModuleStateEnum.DONE)
                {
                    EnableUseBtn();
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.InfoMessageTitle, Properties.Resources.HighSuccessMessage, EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.HighFailMessage, EnumMessageStyle.Affirmative);
                }
                if (!this.WaferAligner().IsNewSetup && this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count > 0)
                {
                    FourButton.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
        }

        #endregion


        public HighStandard()
        {

        }
        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retval = base.InitModule();

                    SubModuleState = new SubModuleIdleState(this);
                    MovingState = new SubModuleStopState(this);
                    SetupState = new NotCompletedState(this);


                    SubModuleState = new SubModuleIdleState(this);
                    MovingState = new SubModuleStopState(this);
                    SetupState = new NotCompletedState(this);

                    LoadPatterImage();

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
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
                throw err;
            }

            return retval;
        }
        public void LoadPatterImage()
        {
            try
            {
                int index = 0;
                foreach (var pattern in HighStandardParam_Clone.Patterns.Value)
                {
                    pattern.Imagebuffer = this.VisionManager().LoadImageFile(
                        this.FileManager().GetDeviceParamFullPath(HighStandardParam_Clone.PatternbasePath, HighStandardParam_Clone.PatternName + index.ToString() + pattern.PMParameter.PatternFileExtension.Value));
                    index++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = Properties.Resources.Header;
                RecoveryHeader = Properties.Resources.RecoveryHeader;

                retVal = InitPnpModuleStage_AdvenceSetting();

                FocusParam.FocusingCam.Value = this.VisionManager().GetCam(HighStandardParam_Clone.CamType).GetChannelType();

                AdvanceSetupView = new HighMagAdvanceSetup.View.HighMagStandardAdvanceSetupView();
                AdvanceSetupViewModel = new HighMagAdvanceSetup.ViewModel.HighMagStandardAdvanceSetupViewModel();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(HighStandard_IParam));

                MiniViewTarget = this.StageSupervisor().WaferObject;
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                this.WaferAligner().ResetHeightPlanePoint();

                if (this.WaferAligner().IsNewSetup)
                {
                    retVal = await InitSetup();
                }
                else
                {
                    retVal = await InitRecovery();

                }

                InitLightJog(this);

                MotionJogEnabled = true;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }


        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CurCam = this.VisionManager().GetCam(HighStandardParam_Clone.CamType);

                if (CurCam != null)
                {
                    FocusParam.FocusingCam.Value = CurCam.GetChannelType();
                }

                retVal = InitLightJog(this, HighStandardParam_Clone.CamType);

                CurCam = this.VisionManager().GetCam(HighStandardParam_Clone.CamType);

                ushort defaultlightvalue = 85;
                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                }

                switch (Wafer.GetPhysInfo().WaferSize_um.Value)
                {
                    case 150000:
                        ShortMiniumumLength = HighStandardParam_Clone.AcceptAlignMimumLength * (1.0 / 2.0);
                        ShortOptimumLength = HighStandardParam_Clone.AcceptAlignOptimumLength * (1.0 / 2.0);
                        ShortMaximumLength = HighStandardParam_Clone.AcceptAlignMaximumLength * (1.0 / 2.0);
                        MinimumLength = HighStandardParam_Clone.AlignMinimumLength * (1.0 / 2.0);
                        OptimumLength = HighStandardParam_Clone.AlignOptimumLength * (1.0 / 2.0);
                        MaximumLength = HighStandardParam_Clone.AlignMaximumLength * (1.0 / 2.0);
                        break;
                    case 200000:
                        ShortMiniumumLength = HighStandardParam_Clone.AcceptAlignMimumLength * (2.0 / 3.0);
                        ShortOptimumLength = HighStandardParam_Clone.AcceptAlignOptimumLength * (2.0 / 3.0);
                        ShortMaximumLength = HighStandardParam_Clone.AcceptAlignMaximumLength * (2.0 / 3.0);
                        MinimumLength = HighStandardParam_Clone.AlignMinimumLength * (2.0 / 3.0);
                        OptimumLength = HighStandardParam_Clone.AlignOptimumLength * (2.0 / 3.0);
                        MaximumLength = HighStandardParam_Clone.AlignMaximumLength * (2.0 / 3.0);
                        break;
                    case 300000:
                        ShortMiniumumLength = HighStandardParam_Clone.AcceptAlignMimumLength;
                        ShortOptimumLength = HighStandardParam_Clone.AcceptAlignOptimumLength;
                        ShortMaximumLength = HighStandardParam_Clone.AcceptAlignMaximumLength;
                        MinimumLength = HighStandardParam_Clone.AlignMinimumLength;
                        OptimumLength = HighStandardParam_Clone.AlignOptimumLength;
                        MaximumLength = HighStandardParam_Clone.AlignMaximumLength;
                        break;
                }

                MainViewTarget = DisplayPort;
                MiniViewTarget = this.StageSupervisor().WaferObject;

                this.WaferAligner().WaferAlignInfo.HighMeasurementTable.HeightPoint = HighStandardParam_Clone.HeightProfilingPointType.Value;

                retVal = InitPNPSetupUI();

                MoveFirstPattern();

                this.VisionManager().StartGrab(HighStandardParam_Clone.CamType, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }


            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void UpdatePatternSize(WAStandardPTInfomation ptinfo, int index)
        {
            try
            {
                if (ptinfo.Imagebuffer == null)
                {
                    ptinfo.Imagebuffer = this.VisionManager().LoadImageFile(this.FileManager().GetDeviceParamFullPath(HighStandardParam_Clone.PatternbasePath, HighStandardParam_Clone.PatternName + index.ToString() + ptinfo.PMParameter.PatternFileExtension.Value));
                }

                ImageBuffer ptimg = ptinfo.Imagebuffer;

                if (ptimg != null)
                {
                    if (ptimg.SizeX != 0 && ptimg.SizeY != 0)
                    {
                        TargetRectangleWidth = ptimg.SizeX;
                        TargetRectangleHeight = ptimg.SizeY;
                    }
                    else // 파일이 아직 저장되지 않은 경우, SizeX와 SizeY의 값은 0
                    {
                        if (ptinfo.Imagebuffer != null)
                        {
                            TargetRectangleWidth = ptinfo.Imagebuffer.SizeX;
                            TargetRectangleHeight = ptinfo.Imagebuffer.SizeY;
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[HighStandard] UpdatePatternSize(), Pattern ImageBuffer null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MoveFirstPattern()
        {
            try
            {
                LoggerManager.Debug($"[HighStandard] MoveFirstPattern(), Wafe Center (X,Y) = ({Wafer.GetSubsInfo().WaferCenter.X.Value:0.00}, {Wafer.GetSubsInfo().WaferCenter.Y.Value:0.00})");

                if (HighStandardParam_Clone.Patterns.Value != null)
                {
                    patternCount = HighStandardParam_Clone.Patterns.Value.Count;

                    if (HighStandardParam_Clone.Patterns.Value.Count != 0)
                    {
                        WAStandardPTInfomation ptinfo = HighStandardParam_Clone.Patterns.Value[0];

                        StageSupervisor.StageModuleState.WaferHighViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.X.Value, ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.Y.Value, ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.Z.Value);

                        UpdatePatternSize(ptinfo, 0);

                        CurPatternIndex = 1;
                    }
                    else
                    {
                        patternCount = 0;
                        CurPatternIndex = 0;

                        if (this.WaferAligner().WaferAlignInfo.LowFirstPatternPosition != null)
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(Wafer.GetSubsInfo().WaferCenter.X.Value + this.WaferAligner().WaferAlignInfo.LowFirstPatternPosition.GetX(), Wafer.GetSubsInfo().WaferCenter.Y.Value + this.WaferAligner().WaferAlignInfo.LowFirstPatternPosition.GetY(), Wafer.GetSubsInfo().ActualThickness);
                        }
                    }
                }
                else
                {
                    patternCount = 0;
                    CurPatternIndex = 0;

                    if (this.WaferAligner().WaferAlignInfo.LowFirstPatternPosition != null)
                    {
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(Wafer.GetSubsInfo().WaferCenter.X.Value + this.WaferAligner().WaferAlignInfo.LowFirstPatternPosition.GetX(), Wafer.GetSubsInfo().WaferCenter.Y.Value + this.WaferAligner().WaferAlignInfo.LowFirstPatternPosition.GetY(), Wafer.GetSubsInfo().ActualThickness);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                patternCount = HighStandardParam_Clone.Patterns.Value.Count;
                StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);
            }
        }
        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer == null)
                {
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer = new ObservableCollection<WAStandardPTInfomation>();
                }
                if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count > 0)
                {
                    SetNodeSetupState(EnumMoudleSetupState.VERIFY);
                }

                CurCam = this.VisionManager().GetCam(HighStandardParam_Clone.CamType);

                if (HighStandardParam_Clone.Patterns?.Value[0].LightParams == null)
                {
                    ushort defaultlightvalue = 85;
                    for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                    {
                        CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                    }
                }

                this.VisionManager().StartGrab(HighStandardParam_Clone.CamType, this);

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                switch (Wafer.GetPhysInfo().WaferSize_um.Value)
                {
                    case 150000:
                        ShortMiniumumLength = HighStandardParam_Clone.AcceptAlignMimumLength * (1.0 / 2.0);
                        ShortOptimumLength = HighStandardParam_Clone.AcceptAlignOptimumLength * (1.0 / 2.0);
                        ShortMaximumLength = HighStandardParam_Clone.AcceptAlignMaximumLength * (1.0 / 2.0);
                        MinimumLength = HighStandardParam_Clone.AlignMinimumLength * (1.0 / 2.0);
                        OptimumLength = HighStandardParam_Clone.AlignOptimumLength * (1.0 / 2.0);
                        MaximumLength = HighStandardParam_Clone.AlignMaximumLength * (1.0 / 2.0);
                        break;
                    case 200000:
                        ShortMiniumumLength = HighStandardParam_Clone.AcceptAlignMimumLength * (2.0 / 3.0);
                        ShortOptimumLength = HighStandardParam_Clone.AcceptAlignOptimumLength * (2.0 / 3.0);
                        ShortMaximumLength = HighStandardParam_Clone.AcceptAlignMaximumLength * (2.0 / 3.0);
                        MinimumLength = HighStandardParam_Clone.AlignMinimumLength * (2.0 / 3.0);
                        OptimumLength = HighStandardParam_Clone.AlignOptimumLength * (2.0 / 3.0);
                        MaximumLength = HighStandardParam_Clone.AlignMaximumLength * (2.0 / 3.0);
                        break;
                    case 300000:
                        ShortMiniumumLength = HighStandardParam_Clone.AcceptAlignMimumLength;
                        ShortOptimumLength = HighStandardParam_Clone.AcceptAlignOptimumLength;
                        ShortMaximumLength = HighStandardParam_Clone.AcceptAlignMaximumLength;
                        MinimumLength = HighStandardParam_Clone.AlignMinimumLength;
                        OptimumLength = HighStandardParam_Clone.AlignOptimumLength;
                        MaximumLength = HighStandardParam_Clone.AlignMaximumLength;
                        break;
                }

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                MainViewTarget = DisplayPort;
                MiniViewTarget = Wafer;

                MoveFirstPattern();

                InitRecoveryPNPSetupUI();
                LoadPatterImage();

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.InitRocovery() : Error occrued.");
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;


                if (HighStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    ProcessingType = EnumSetupProgressState.DONE;
                }
                else
                {
                    ProcessingType = EnumSetupProgressState.SKIP;
                }

                patternCount = HighStandardParam_Clone.Patterns.Value.Count;
                CurPatternIndex = 0;
                StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("");

                PadJogLeftUp.IconCaption = "PATTERN";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "PATTERN";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogSelect.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/feature-search-outline_W.png");

                PadJogLeftUp.Command = new AsyncCommand(Prev);
                PadJogRightUp.Command = new AsyncCommand(Next);
                PadJogLeft.Command = new RelayCommand(UCDisplayRectWidthMinus);
                PadJogRight.Command = new RelayCommand(UCDisplayRectWidthPlus);
                PadJogUp.Command = new RelayCommand(UCDisplayRectHeightPlus);
                PadJogDown.Command = new RelayCommand(UCDisplayRectHeightMinus);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;


                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                OneButton.IconCaption = "ADD";
                OneButton.Command = RegistePatternCommand;

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Delete.png");
                TwoButton.IconCaption = "DELETE";
                TwoButton.Command = DeletePatternCommand;

                PadJogSelect.IconCaption = "View PT";
                PadJogSelect.Command = new AsyncCommand(ViewPattern);

                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                //ThreeButton.MaskingLevel = 3;
                ThreeButton.IconCaption = "FOCUS";
                ThreeButton.Command = new AsyncCommand(FocusingCommand);

                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Apply.png");
                FourButton.IconCaption = "APPLY";
                FourButton.Command = new AsyncCommand(Apply);


                //PadJogRightDown.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //     new Uri("pack://application:,,,/ImageResourcePack;component/Images/PnpAbort.png", UriKind.Absolute));
                //PadJogRightDown.Command = new RelayCommand(PatternRegisteAbort);

                EnableUseBtn();

                //PadJogRightDown.IsEnabled = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum InitRecoveryPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (HighStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    ProcessingType = EnumSetupProgressState.DONE;
                }
                else
                {
                    ProcessingType = EnumSetupProgressState.SKIP;
                }

                patternCount = HighStandardParam_Clone.Patterns.Value.Count;
                temppatternCount = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count;
                CurPatternIndex = 1;
                CurTempPatternIndex = 0;
                StepLabel = String.Format("REGISTED PATTERN {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);
                StepLabelActive = false;
                StepSecondLabelActive = false;
                PadJogLeftUp.IconCaption = "PATTERN";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "PATTERN";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogSelect.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/feature-search-outline_W.png");

                PadJogLeftUp.Command = new AsyncCommand(RecoverySetup_Prev);
                PadJogRightUp.Command = new AsyncCommand(RecoverySetup_Next);
                PadJogLeft.Command = new RelayCommand(UCDisplayRectWidthMinus);
                PadJogRight.Command = new RelayCommand(UCDisplayRectWidthPlus);
                PadJogUp.Command = new RelayCommand(UCDisplayRectHeightPlus);
                PadJogDown.Command = new RelayCommand(UCDisplayRectHeightMinus);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;


                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                OneButton.IconCaption = "ADD";
                OneButton.Command = Temporary_RegistePatternCommand;

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Delete.png");
                TwoButton.IconCaption = "DELETE";
                TwoButton.Command = Temporary_DeletePatternCommand;


                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                //ThreeButton.MaskingLevel = 3;
                ThreeButton.IconCaption = "FOCUS";
                ThreeButton.Command = new AsyncCommand(FocusingCommand);

                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Apply.png");
                FourButton.IconCaption = "APPLY";
                FourButton.Command = new AsyncCommand(Apply);

                PadJogSelect.IconCaption = "View PT";
                PadJogSelect.Command = new AsyncCommand(ViewPattern);

                //PadJogRightDown.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //     new Uri("pack://application:,,,/ImageResourcePack;component/Images/PnpAbort.png", UriKind.Absolute));
                //PadJogRightDown.Command = new RelayCommand(PatternRegisteAbort);

                retVal = EnableUseBtn();
                if (!this.WaferAligner().IsNewSetup && this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count > 0)
                {
                    FourButton.IsEnabled = false;
                }
                ActiveStepLabelChanged();
                //PadJogRightDown.IsEnabled = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new WA_HighMagParam_Standard();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(WA_HighMagParam_Standard));

                if (retval == EventCodeEnum.NONE)
                {
                    HighStandard_IParam = tmpParam;
                    HighStandardParam_Clone = HighStandard_IParam as WA_HighMagParam_Standard;
                }

                FocusingModule = WaferHighFocusModel;
                FocusingParam = FocusParam;

                this.WaferAligner().WaferAlignInfo.HighMeasurementTable.HeightPoint = HighStandardParam_Clone.HeightProfilingPointType.Value;
                if (HighStandardParam_Clone.Patterns.Value.Count != 0)
                {
                    SortMaxJumpIndex(HighStandardParam_Clone.Patterns.Value[0]);
                    this.WaferAligner().WaferAlignInfo.HeightPositions = _HeightPositions;
                }

                SettingFocusingROIWithPatternSizeParam(HighStandardParam_Clone.FocusingROIWithPatternSize.RetryFocusingROIMargin_X.Value, HighStandardParam_Clone.FocusingROIWithPatternSize.RetryFocusingROIMargin_Y.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }


            return retval;
        }
        public override EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (HighStandardParam_Clone != null)
                {
                    int index = 0;

                    foreach (var pattern in HighStandardParam_Clone.Patterns.Value)
                    {
                        if (pattern.Imagebuffer != null)
                        {
                            var filePathPrefix = this.FileManager().GetDeviceParamFullPath();
                            this.VisionManager().SavePattern(pattern, filePathPrefix);
                        }

                        index++;
                    }
                }

                this.WaferAligner().WaferAlignInfo.HighMeasurementTable.HeightPoint = HighStandardParam_Clone.HeightProfilingPointType.Value;

                HighStandard_IParam = HighStandardParam_Clone;

                RetVal = this.SaveParameter(HighStandard_IParam);

                if (RetVal == EventCodeEnum.NONE)
                {
                    IsParamChanged = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.SaveDevParameter() : Error occured.");
                throw err;
            }

            return RetVal;
        }

        public override void SetPackagableParams()
        {
            try
            {
                WA_HighMagParam_Standard param = HighStandardParam_Clone;

                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(param));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(WA_HighMagParam_Standard));
                    if (target != null)
                    {
                        WA_HighMagParam_Standard paramobj = (WA_HighMagParam_Standard)target;
                        paramobj.JumpIndexManualInputParam.CopyTo(HighStandardParam_Clone.JumpIndexManualInputParam);
                        paramobj.DefaultPMParam.CopyTo(HighStandardParam_Clone.DefaultPMParam);
                        paramobj.FocusingROIWithPatternSize.CopyTo(HighStandardParam_Clone.FocusingROIWithPatternSize);
                        HighStandardParam_Clone.LimitValueX.Value = paramobj.LimitValueX.Value;
                        HighStandardParam_Clone.LimitValueY.Value = paramobj.LimitValueY.Value;
                        HighStandardParam_Clone.VerifyLimitAngle.Value = paramobj.VerifyLimitAngle.Value;
                        HighStandardParam_Clone.EnablePostJumpindex = paramobj.EnablePostJumpindex;
                        foreach (var pattern in HighStandardParam_Clone.Patterns.Value)
                        {
                            pattern.EnablePostJumpindex = paramobj.EnablePostJumpindex;
                        }
                        HighStandardParam_Clone.WaferPlanarityLimit.Value = paramobj.WaferPlanarityLimit.Value;
                        HighStandardParam_Clone.HeightProfilingPointType.Value = paramobj.HeightProfilingPointType.Value;
                        HighStandardParam_Clone.HeightPosParams = paramobj.HeightPosParams;
                        paramobj.High_ProcessingPoint.CopyTo(HighStandardParam_Clone.High_ProcessingPoint);
                        //paramobj.Patterns.CopyTo(HighStandardParam_Clone.Patterns);
                        paramobj.FocusParam.CopyTo(HighStandardParam_Clone.FocusParam);
                        HighStandard_IParam = HighStandardParam_Clone;

                        SettingFocusingROIWithPatternSizeParam(paramobj.FocusingROIWithPatternSize.RetryFocusingROIMargin_X.Value, paramobj.FocusingROIWithPatternSize.RetryFocusingROIMargin_Y.Value);
                        //HighStandard_IParam = (WA_HighMagParam_Standard)target;
                        //HighStandardParam_Clone = (WA_HighMagParam_Standard)target;
                        break;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        public EventCodeEnum DoExecute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                WA_HighMagParam_Standard param = (HighStandard_IParam as WA_HighMagParam_Standard);

                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    param = HighStandardParam_Clone;
                }

                switch (param.HeightProfilingPointType.Value)
                {
                    case HeightPointEnum.POINT1:
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_1HeightProfiling_Start);
                        break;
                    case HeightPointEnum.POINT5:
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_5HeightProfiling_Start);
                        break;
                    case HeightPointEnum.POINT9:
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_9HeightProfiling_Start);
                        break;
                }
                
                List<WAStandardPTInfomation> ptinfos = param.Patterns.Value.ToList<WAStandardPTInfomation>();

                if (this.WaferAligner().WaferAlignInfo.RecoveryParam != null)
                {
                    if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count > 0)
                    {
                        List<WAStandardPTInfomation> recoveryptinofs = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.ToList<WAStandardPTInfomation>();

                        foreach (var pt in ptinfos)
                        {
                            recoveryptinofs.Add(pt);
                        }

                        ptinfos = recoveryptinofs;
                    }
                }

                if (CurCam == null)
                {
                    CurCam = this.VisionManager().GetCam(param.CamType);
                }

                if (GetState() != SubModuleStateEnum.DONE)
                {
                    if (ptinfos.Count != 0)
                    {
                        ProcResults = new List<WaferProcResult>();

                        // Update Processing Start
                        if (this.WaferAligner().IsNewSetup == true || this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP)
                        {
                            AllProcessingIndex();
                        }
                        else
                        {
                            //setup이 아닌 경우 processing 
                            UpdateProcessingIndex();
                        }
                        //Emul Lot Test Code
                        if (this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                        {
                            this.VisionManager().LoadImageFromFloder(@"C:\ProberSystem\EmulImages\WaferAlign\HighMag\", CurCam.GetChannelType());
                            ptinfos[0].PMParameter.PMAcceptance.Value = 60;
                        }
                        //======================

                        if (!this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                        {
                            this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                        }

                        //this.WaferAligner().ResetHeightPlanePoint();
                        Wafer.GetSubsInfo().WaferSequareness.Value = 0;

                        SettingLimit(param.AcceptAlignMimumLength, param.AcceptAlignOptimumLength, param.AcceptAlignMaximumLength, param.AlignMinimumLength, param.AlignOptimumLength, param.AlignMaximumLength);

                        if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP)
                        {
                            RetVal = ThetaAlign(ref ptinfos, ref ProcResults, ref RotateAngle, _OperCancelTokenSource, false, false);
                        }
                        else
                        {
                            RetVal = ThetaAlign(ref ptinfos, ref ProcResults, ref RotateAngle, _OperCancelTokenSource, true);
                        }

                        if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count < 0)
                        {
                            for (int index = 0; index < ptinfos.Count; index++)
                            {
                                param.Patterns.Value[index] = ptinfos[index];
                            }
                        }

                        if (this.WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                        {
                            RetVal = EventCodeEnum.NONE;
                            SubModuleState = new SubModuleDoneState(this);
                        }

                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_OK);

                        if (RetVal == EventCodeEnum.NONE)
                        {
                            RetVal = ProcessingHeightProfiling();

                            if (RetVal == EventCodeEnum.NONE)
                            {
                                RetVal = VerifyAlignProcessing();

                                if (RetVal == EventCodeEnum.NONE)
                                {

                                    double curTheta = 0.0;
                                    this.MotionManager().GetRefPos(EnumAxisConstants.C, ref curTheta);
                                    double prevTheta = curTheta;

                                    RetVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(this.MotionManager().GetAxis(EnumAxisConstants.C), (this.WaferAligner().WaferAlignInfo.AlignAngle));
                                    this.MotionManager().GetRefPos(EnumAxisConstants.C, ref curTheta);
                                    LoggerManager.Debug($"WaferAlign Theta beforeMove : {prevTheta}, afterMove : {curTheta}, offset : {curTheta - prevTheta}");

                                    if (RetVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"VerifyAlign Failure while moving theta.");
                                        RetVal = EventCodeEnum.Wafer_Verify_Align_Failure;
                                        //RetVal = EventCodeEnum.SUB_RECOVERY;
                                    }
                                }

                                if (RetVal == EventCodeEnum.NONE)
                                {
                                    RetVal = CheckHeightProfiling(param.WaferPlanarityLimit.Value);
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[HighStandard], DoExecute() : ProcessingHeightProfiling() result = {RetVal}");
                            }
                        }

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            if (RetVal == EventCodeEnum.FOCUS_VALUE_THRESHOLD | RetVal == EventCodeEnum.FOCUS_VALUE_FLAT | RetVal == EventCodeEnum.FOCUS_VALUE_DUALPEAK)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.WAFER_HIGH_FOCUSING_FAIL);
                            }
                            else if (RetVal == EventCodeEnum.VISION_PM_NOT_FOUND | RetVal == EventCodeEnum.VISION_PM_EXCEPTION)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.WAFER_HIGH_PATTERN_NOT_FOUND);
                            }
                            else if (RetVal == EventCodeEnum.VERIFYALIGN_OVERFLOW_LIMIT)
                            {
                                LoggerManager.Error($"[HighStandard], DoExecute() failed, retval = {RetVal}");
                            }
                            else
                            {
                                LoggerManager.Error($"[HighStandard], DoExecute() : Unknown status. retval = {RetVal}");
                            }
                        }
                    }
                }
                else if (GetState() == SubModuleStateEnum.DONE)
                {
                    if (ptinfos.Count != 0)
                    {
                        ICamera cam = this.VisionManager().GetCam(ptinfos[0].CamType.Value);

                        int count = 0;
                        double totalX = 0.0;
                        double totalY = 0.0;

                        for (int index = 0; index < ptinfos.Count; index++)
                        {
                            if (ptinfos[index].PatternState.Value != PatternStateEnum.FAILED)
                            {
                                count++;

                                totalX += ptinfos[index].ProcWaferCenter.GetX();
                                totalY += ptinfos[index].ProcWaferCenter.GetY();
                            }
                        }

                        double xlimit = ((cam.GetGrabSizeWidth() / 4) * cam.GetRatioX());
                        double ylimit = ((cam.GetGrabSizeHeight() / 4) * cam.GetRatioY());

                        if ((Math.Abs(totalX / 4) > xlimit) || (Math.Abs(totalY / 4) > ylimit))
                        {
                            LoggerManager.Debug($"Wafer High Mag Align Cur State Done Fail => count :{count}, totalX :{totalX}, totalY : {totalY}, xlimit : {xlimit}, ylimit : {ylimit}");
                            RetVal = EventCodeEnum.SUB_RECOVERY;
                        }
                        else
                        {
                            RetVal = EventCodeEnum.NONE;
                        }
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    WirteHeightProfilingLog();

                    SubModuleState = new SubModuleDoneState(this);
                    this.Wafer.SetAlignState(AlignStateEnum.IDLE);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_OK);
                }
                else if (RetVal == EventCodeEnum.SUB_RECOVERY)
                {
                    //this.WaferAligner().ReasonOfError.Reason = "High Pattern Not Found";
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "High Pattern Not Found", this.GetType().Name);

                    SubModuleState = new SubModuleRecoveryState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_High_Mag_Failure, RetVal);
                }
                else if (RetVal == EventCodeEnum.SUB_SKIP)
                {
                    SubModuleState = new SubModuleSkipState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_OK);
                }
                else if (RetVal == EventCodeEnum.WAFER_ALIGN_PLANARITY_OVER_TOLERANCE)
                {
                    //RetVal = EventCodeEnum.SUB_RECOVERY;
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "Wafer Align Planarity over tolerance", this.GetType().Name);
                    SubModuleState = new SubModuleRecoveryState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_High_Mag_Failure, RetVal);
                }
                else if (RetVal == EventCodeEnum.VERIFYALIGN_OVERFLOW_LIMIT)
                {
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "Wafer verify failed.", this.GetType().Name);
                    SubModuleState = new SubModuleErrorState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_High_Mag_Failure, RetVal);
                }
                else if (RetVal == EventCodeEnum.WAFER_ALIGN_THETA_COMPENSATION_FAIL)
                {
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "WaferAlign theta compensation failed.", this.GetType().Name);
                    SubModuleState = new SubModuleRecoveryState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_High_Mag_Failure, RetVal);
                }
                else
                {
                    //this.WaferAligner().ReasonOfError.Reason = "High Pattern Not Found";
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "High Pattern Not Found", this.GetType().Name);

                    SubModuleState = new SubModuleErrorState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_High_Mag_Failure, RetVal);
                }

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_High_Mag_Failure);

                throw err;
            }
            finally
            {
                this.VisionManager().ClearGrabberUserImage(CurCam.GetChannelType());
                MovingState.Stop();
            }
            return RetVal;
        }


        private void WirteHeightProfilingLog()
        {
            try
            {
                WA_HighMagParam_Standard param = HighStandard_IParam as WA_HighMagParam_Standard;

                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    param = HighStandardParam_Clone;
                }

                if (param.Patterns.Value.Count <= 0)
                {
                    return;
                }

                var ptval = GetFirstReadyPattern();

                if(ptval != null)
                {
                    switch (param.HeightProfilingPointType.Value)
                    {
                        //TODO : Tilted 값만 따로 표시되도록 한다.
                        case HeightPointEnum.POINT1:
                            LoggerManager.Debug("-- HEIGHT MEASURED 1PT --", isInfo: true);
                            LoggerManager.Debug($"******************", isInfo: true);
                            LoggerManager.Debug($"******{Math.Round(ptval.PostJumpIndex[0].HeightProfilingVal, 3)}******", isInfo: true);
                            LoggerManager.Debug($"******************", isInfo: true);
                            LoggerManager.Debug("-------------------------", isInfo: true);
                            break;
                        case HeightPointEnum.POINT5:
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_5HeightProfiling_Start);
                            LoggerManager.Debug("-- HEIGHT MEASURED 5PT --", isInfo: true);
                            LoggerManager.Debug($"*************{Math.Round(ptval.JumpIndexs[2].HeightProfilingVal, 3)}*************", isInfo: true);
                            LoggerManager.Debug($"***{Math.Round(ptval.JumpIndexs[0].HeightProfilingVal, 3)}***{Math.Round(ptval.PostJumpIndex[0].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[1].HeightProfilingVal, 3)}***", isInfo: true);
                            LoggerManager.Debug($"*************{Math.Round(ptval.JumpIndexs[3].HeightProfilingVal, 3)}*************", isInfo: true);
                            LoggerManager.Debug("-------------------------", isInfo: true);
                            break;
                        case HeightPointEnum.POINT9:
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_9HeightProfiling_Start);
                            LoggerManager.Debug("-- HEIGHT MEASURED 9PT --", isInfo: true);
                            LoggerManager.Debug($"***{Math.Round(param.HeightPosParams[1].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[2].HeightProfilingVal, 3)}***{Math.Round(param.HeightPosParams[0].HeightProfilingVal, 3)}***", isInfo: true);
                            LoggerManager.Debug($"***{Math.Round(ptval.JumpIndexs[0].HeightProfilingVal, 3)}***{Math.Round(ptval.PostJumpIndex[0].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[1].HeightProfilingVal, 3)}**", isInfo: true);
                            LoggerManager.Debug($"***{Math.Round(param.HeightPosParams[2].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[3].HeightProfilingVal, 3)}***{Math.Round(param.HeightPosParams[3].HeightProfilingVal, 3)}***", isInfo: true);
                            LoggerManager.Debug("-------------------------", isInfo: true);
                            break;
                            //TODO Tilted값도 표시해주면 좋을 듯
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], WirteHeightProfilingLog() : ptval is null.");
                }

                //WAStandardPTInfomation ptval = param.Patterns.Value[0];
                //switch (param.HeightProfilingPointType.Value)
                //{
                //    //TODO : Tilted 값만 따로 표시되도록 한다.
                //    case HeightPointEnum.POINT1:
                //        LoggerManager.Debug("-- HEIGHT MEASURED 1PT --", isInfo: true);
                //        LoggerManager.Debug($"******************", isInfo: true);
                //        LoggerManager.Debug($"******{Math.Round(ptval.PostJumpIndex[0].HeightProfilingVal, 3)}******", isInfo: true);
                //        LoggerManager.Debug($"******************", isInfo: true);
                //        LoggerManager.Debug("-------------------------", isInfo: true);
                //        break;
                //    case HeightPointEnum.POINT5:
                //        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_5HeightProfiling_Start);
                //        LoggerManager.Debug("-- HEIGHT MEASURED 5PT --", isInfo: true);
                //        LoggerManager.Debug($"*************{Math.Round(ptval.JumpIndexs[2].HeightProfilingVal, 3)}*************", isInfo: true);
                //        LoggerManager.Debug($"***{Math.Round(ptval.JumpIndexs[0].HeightProfilingVal, 3)}***{Math.Round(ptval.PostJumpIndex[0].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[1].HeightProfilingVal, 3)}***", isInfo: true);
                //        LoggerManager.Debug($"*************{Math.Round(ptval.JumpIndexs[3].HeightProfilingVal, 3)}*************", isInfo: true);
                //        LoggerManager.Debug("-------------------------", isInfo: true);
                //        break;
                //    case HeightPointEnum.POINT9:
                //        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_High_Mag_9HeightProfiling_Start);
                //        LoggerManager.Debug("-- HEIGHT MEASURED 9PT --", isInfo: true);
                //        LoggerManager.Debug($"***{Math.Round(param.HeightPosParams[1].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[2].HeightProfilingVal, 3)}***{Math.Round(param.HeightPosParams[0].HeightProfilingVal, 3)}***", isInfo: true);
                //        LoggerManager.Debug($"***{Math.Round(ptval.JumpIndexs[0].HeightProfilingVal, 3)}***{Math.Round(ptval.PostJumpIndex[0].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[1].HeightProfilingVal, 3)}**", isInfo: true);
                //        LoggerManager.Debug($"***{Math.Round(param.HeightPosParams[2].HeightProfilingVal, 3)}***{Math.Round(ptval.JumpIndexs[3].HeightProfilingVal, 3)}***{Math.Round(param.HeightPosParams[3].HeightProfilingVal, 3)}***", isInfo: true);
                //        LoggerManager.Debug("-------------------------", isInfo: true);
                //        break;
                //        //TODO Tilted값도 표시해주면 좋을 듯
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public async Task<EventCodeEnum> Modify()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                switch (ModifyCondition)
                {
                    case WAHighSetupFunction.REGPATTERN:
                        this.WaferAligner().ResetHeightPlanePoint();
                        retVal = await RegistPattern();
                        if (retVal == EventCodeEnum.NONE)
                            IsParamChanged = true;
                        EnableUseBtn();
                        break;
                    case WAHighSetupFunction.DELETEPATTERN:
                        if (retVal == EventCodeEnum.NONE)
                            IsParamChanged = true;
                        retVal = DeletePattern();
                        break;
                    case WAHighSetupFunction.TEMPORARY_REGPATTERN:
                        this.WaferAligner().ResetHeightPlanePoint();
                        retVal = await RecoverySetup_RegistPattern();
                        //if (retVal == EventCodeEnum.NONE)
                        //    IsParamChanged = true;
                        //EnableUseBtn();
                        break;
                    case WAHighSetupFunction.TEMPORARY_DELETEPATTERN:
                        //if (retVal == EventCodeEnum.NONE)
                        //    IsParamChanged = true;
                        retVal = RecoverySetup_DeletePattern();
                        break;
                    case WAHighSetupFunction.FOCUSING:

                        FocusingParam.FocusingCam.Value = CurCam.GetChannelType();

                        retVal = FocusingModule.Focusing_Retry(FocusingParam, false, true, false, this);

                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                            Wafer.GetSubsInfo().ActualThickness = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetZ();
                        else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                            Wafer.GetSubsInfo().ActualThickness = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert().GetZ();

                        this.WaferAligner().ResetHeightPlanePoint();

                        this.WaferAligner().CreateBaseHeightProfiling(new WaferCoordinate(
                            WaferObject.GetSubsInfo().WaferCenter.GetX(),
                            WaferObject.GetSubsInfo().WaferCenter.GetY(),
                            WaferObject.GetSubsInfo().ActualThickness));

                        break;
                    default:
                        break;
                }

                IsParameterChanged();
                SetStepSetupState();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

            }
            return retVal;
        }

        // 1 : 1.5 = x : 150,0000
        // 1 : 패턴매칭 정밀도
        // 1.5 : 12Inch Wafer 끝면에서 theta 모듈의 레졸루션 스펙으로 인해서 발생할수있는 포지션 에러의 최대치
        // 150,0000 : 12Inch Wafer 의 반지름 
        // x : theta 모듈의 레졸루션 스펙으로 인해서 발생할수있는 포지션 에러를 보상하기 위해 척 중심에서 부터 뛰어야하는 최소거리 
        // x == minlength

        #region ..//RegistePattern

        private int ManualJumpIndexConfirm()
        {
            int retVal = -1;
            try
            {
                if (HighStandardParam_Clone.JumpIndexManualInputParam.Left1Index != 0 && HighStandardParam_Clone.JumpIndexManualInputParam.Left2Index != 0
                    && HighStandardParam_Clone.JumpIndexManualInputParam.Right1Index != 0 && HighStandardParam_Clone.JumpIndexManualInputParam.Right2Index != 0
                     & HighStandardParam_Clone.JumpIndexManualInputParam.UpperIndex != 0 && HighStandardParam_Clone.JumpIndexManualInputParam.BottomIndex != 0)
                {
                    retVal = 1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private ObservableCollection<StandardJumpIndexParam> ManualJumpIndexApply(ObservableCollection<StandardJumpIndexParam> jindexparam)
        {
            jindexparam.Add(new StandardJumpIndexParam(0, 0));
            jindexparam.Add(new StandardJumpIndexParam(HighStandardParam_Clone.JumpIndexManualInputParam.Right1Index, 0));
            jindexparam.Add(new StandardJumpIndexParam(-HighStandardParam_Clone.JumpIndexManualInputParam.Left1Index, 0));
            jindexparam.Add(new StandardJumpIndexParam(-HighStandardParam_Clone.JumpIndexManualInputParam.Left2Index, 0));
            jindexparam.Add(new StandardJumpIndexParam(HighStandardParam_Clone.JumpIndexManualInputParam.Right2Index, 0));
            jindexparam.Add(new StandardJumpIndexParam(0, HighStandardParam_Clone.JumpIndexManualInputParam.UpperIndex));
            jindexparam.Add(new StandardJumpIndexParam(0, -HighStandardParam_Clone.JumpIndexManualInputParam.BottomIndex));

            return jindexparam;
        }

        private WAStandardPTInfomation ManualJumpIndexApply(WAStandardPTInfomation patterninfo)
        {
            patterninfo.PostJumpIndex.Add(new StandardJumpIndexParam(0, 0));
            patterninfo.PostJumpIndex.Add(new StandardJumpIndexParam(HighStandardParam_Clone.JumpIndexManualInputParam.Right1Index, 0));
            patterninfo.PostJumpIndex.Add(new StandardJumpIndexParam(-HighStandardParam_Clone.JumpIndexManualInputParam.Left1Index, 0));
            patterninfo.PostHorDirection.Value = EnumHorDirection.RIGHTLEFT;
            patterninfo.PostProcDirection.Value = EnumWAProcDirection.HORIZONTAL;

            patterninfo.JumpIndexs.Add(new StandardJumpIndexParam(-HighStandardParam_Clone.JumpIndexManualInputParam.Left2Index, 0));
            patterninfo.JumpIndexs.Add(new StandardJumpIndexParam(HighStandardParam_Clone.JumpIndexManualInputParam.Right2Index, 0));
            patterninfo.JumpIndexs.Add(new StandardJumpIndexParam(0, HighStandardParam_Clone.JumpIndexManualInputParam.UpperIndex));
            patterninfo.JumpIndexs.Add(new StandardJumpIndexParam(0, -HighStandardParam_Clone.JumpIndexManualInputParam.BottomIndex));
            //patterninfo.PostHorDirection.Value = EnumHorDirection.LEFTRIGHT;
            //patterninfo.PostVerDirection.Value = EnumVerDirection.UPPERBOTTOM;
            patterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;
            patterninfo.VerDirection.Value = EnumVerDirection.UPPERBOTTOM;
            patterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;

            return patterninfo;
        }



        private async Task<EventCodeEnum> RegistPattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //==========================
                if (CurCam.GetChannelType() != HighStandardParam_Clone.CamType)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Pattern Register Error.",
                        "To register the Low pattern, please view the screen with Low camera and register again.",
                        EnumMessageStyle.Affirmative);
                    return retVal;
                }

                WaferCoordinate wcd = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();


                //Modify 일때만
                //if (!this.WaferAligner().IsNewSetup & this.WaferAligner().GetWAInnerStateEnum() != WaferAlignInnerStateEnum.IDLE)
                //{
                //    retVal = await ValidationWaferAlign();
                //    this.StageSupervisor().StageModuleState.WaferHighViewMove(wcd.GetX(), wcd.GetY());
                //    if (retVal != EventCodeEnum.NONE)
                //    {
                //        return retVal;
                //    }
                //    else
                //    {
                //        await this.MetroDialogManager().ShowMessageDialog("Wafer Align Fail.",
                //            "Align failed.Setup again with New Setup.",
                //            EnumMessageStyle.Affirmative);
                //        return retVal;
                //    }
                //}

                _OperCancelTokenSource = new CancellationTokenSource();

                double rotateangle = 0.0;

                WAStandardPTInfomation patterninfo = new WAStandardPTInfomation();
                RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();

                if (patternparam.Width == 0 || patternparam.Height == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.",
                        "Check the pattern size."
                            , EnumMessageStyle.Affirmative
                            );
                    return EventCodeEnum.PATTERN_SIZE_ERROR;
                }

                patterninfo.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();
                HighStandardParam_Clone.DefaultPMParam.CopyTo(patterninfo.PMParameter);
                //patterninfo.PMParameter = HighStandardParam.DefaultPMParam;
                patterninfo.CamType.Value = CurCam.GetChannelType();
                //patterninfo.FocusingModel = FocusingModule;

                patterninfo.PMParameter.ModelFilePath.Value = HighStandardParam_Clone.PatternbasePath
                     + HighStandardParam_Clone.PatternName + HighStandardParam_Clone.Patterns.Value.Count.ToString();
                patterninfo.PMParameter.PatternFileExtension.Value = ".mmo";

                ImageBuffer curImage = this.VisionManager().SingleGrab(patterninfo.CamType.Value, this);

                patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(
                    curImage, patternparam.LocationX, patternparam.LocationY, patternparam.Width, patternparam.Height);

                this.VisionManager().GetGrayValue(ref curImage);
                patterninfo.GrayLevel = curImage.GrayLevelValue;

                patterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;
                patterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;
                patterninfo.VerDirection.Value = EnumVerDirection.UPPERBOTTOM;

                //=====================================================================================

                bool IsFindJumpIndex = true;

                double registareaXlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) -
                    Math.Pow(OptimumLength, 2)) - HighStandardParam_Clone.DeadZone;

                double registareaYlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) -
                    Math.Pow(OptimumLength, 2));

                if (wcd.GetX() > -registareaXlimit && wcd.GetX() < registareaXlimit
                    && wcd.GetY() > -registareaYlimit && wcd.GetY() < registareaYlimit)
                {
                    //if (this.WaferAligner().IsNewSetup)
                    //{
                    //    if (this.WaferAligner().WaferAlignInfo.PTWaferCenter != null)
                    //    {
                    //        patterninfo.WaferCenter = new WaferCoordinate();
                    //        this.Wafer.GetSubsInfo().WaferCenter.CopyTo(patterninfo.WaferCenter);
                    //        //this.WaferAligner().WaferAlignInfo.PTWaferCenter.CopyTo(patterninfo.WaferCenter);
                    //    }
                    //}
                    //else
                    //{
                    //    patterninfo.WaferCenter = new WaferCoordinate();
                    //    this.WaferAligner().WaferAlignInfo.PTWaferCenter.CopyTo(patterninfo.WaferCenter);
                    //    //this.Wafer.GetSubsInfo().WaferCenter.CopyTo(patterninfo.WaferCenter);
                    //}
                    patterninfo.WaferCenter = new WaferCoordinate();
                    this.Wafer.GetSubsInfo().WaferCenter.CopyTo(patterninfo.WaferCenter);

                    wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                    patterninfo.X.Value = (wcd.X.Value - patterninfo.WaferCenter.GetX());
                    patterninfo.Y.Value = (wcd.Y.Value - patterninfo.WaferCenter.GetY());
                    //patterninfo.Z.Value = (wcd.Z.Value - patterninfo.WaferCenter.GetZ());

                    retVal = ValidationTesting(patterninfo, FocusingModule, FocusingParam);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        if (_OperCancelTokenSource.Token.IsCancellationRequested)
                        {
                            return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                        }

                        await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.",
                            "Pattern validation failed. Please register another pattern.",
                            EnumMessageStyle.AffirmativeAndNegative);
                        if (HighStandardParam_Clone.Patterns.Value.Count == 0)
                            CurPatternIndex = 0;
                        return retVal;
                    }
                    else
                    {
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                        patterninfo.Z.Value = (wcd.Z.Value - patterninfo.WaferCenter.GetZ());

                        List<WAStandardPTInfomation> registPTInfo = new List<WAStandardPTInfomation>();
                        List<StandardJumpIndexParam> storagePTInfos = new List<StandardJumpIndexParam>();

                        ProcResults = new List<WaferProcResult>();

                        for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                        {
                            patterninfo.LightParams.Add(
                                new LightValueParam(CurCam.LightsChannels[lightindex].Type.Value,
                                (ushort)CurCam.GetLight(CurCam.LightsChannels[lightindex].Type.Value)));
                        }

                        //if (this.WaferAligner().GetWAInnerStateEnum() == ProberInterfaces.Align.WaferAlignInnerStateEnum.SETUP)
                        //{
                        if (ManualJumpIndexConfirm() != -1)
                        {
                            if (patterninfo.JumpIndexs == null)
                                patterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();

                            //patterninfo.JumpIndexs =
                            //    ManualJumpIndexApply(patterninfo.JumpIndexs);

                            patterninfo = ManualJumpIndexApply(patterninfo);
                            patterninfo = UpdateHeightProfiling(patterninfo);

                            IsFindJumpIndex = false;
                        }


                        /* auto mode시 2번째 패턴 부터 fail시 주변 die retry를 안하는 이슈로 주석 처리 함
                        else
                        {
                            foreach (var item in HighStandardParam_Clone.Patterns.Value)
                            {
                                int retindex = item.JumpIndexs.ToList<StandardJumpIndexParam>().FindIndex(
                                    index => index.Index.XIndex != -1);

                                if (retindex >= 0)
                                {
                                    if (patterninfo.JumpIndexs == null)
                                        patterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();


                                    for (int index = 0; index < item.PostJumpIndex.Count; index++)
                                    {
                                        patterninfo.PostJumpIndex.Add(item.PostJumpIndex[index]);
                                    }
                                    patterninfo.PostProcDirection = item.PostProcDirection;
                                    for (int index = 0; index < item.JumpIndexs.Count; index++)
                                    {
                                        patterninfo.JumpIndexs.Add(item.JumpIndexs[index]);
                                    }
                                    patterninfo.ProcDirection = item.ProcDirection;
                                    IsFindJumpIndex = false;
                                    break;
                                }
                            }
                        }
                        */
                        //}
                        //else if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.RECOVERING)
                        //{
                        //    for (int index = 0; index < registPTInfo.Count; index++)
                        //    {
                        //        tempPtInfo.Add(new WAStandardPTInfomation(registPTInfo[index]));
                        //    }
                        //}


                        if (IsFindJumpIndex)
                        {
                            patterninfo.PostProcDirection.Value = EnumWAProcDirection.HORIZONTAL;
                            patterninfo.PostHorDirection.Value = EnumHorDirection.RIGHTLEFT;
                            retVal = FindJumpIndex(ref patterninfo, ref rotateangle, patternparam,
                            ShortMiniumumLength, ShortOptimumLength, ShortMaximumLength, true, true);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                RotateAngle += rotateangle;
                                for (int index = 0; index < patterninfo.JumpIndexs.Count; index++)
                                {
                                    storagePTInfos.Add(patterninfo.JumpIndexs[index]);
                                }

                                patterninfo.JumpIndexs = null;
                                patterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;
                                patterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;
                                patterninfo.VerDirection.Value = EnumVerDirection.UPPERBOTTOM;

                                patterninfo.AcceptFocusing.Value = true;
                                //patterninfo.FocusingModel = FocusingModule;

                                retVal = FindJumpIndex(ref patterninfo, ref rotateangle, patternparam,
                                    MinimumLength, OptimumLength, MaximumLength, false, false);

                                if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                                    return retVal;

                                else if (patterninfo.ErrorCode == EventCodeEnum.NONE)
                                {



                                }
                                this.WaferAligner().WaferAlignInfo.AlignProcResult = ProcResults;
                            }
                            else
                            {
                                //await this.MetroDialogManager().ShowMessageDialog("",
                                //    "This pattern is not valid for Align. Please register the pattern in another location.",
                                //    EnumMessageStyle.Affirmative);

                                //this.StageSupervisor().StageModuleState.WaferLowViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());

                                //return retVal;
                            }

                        }
                        else
                        {
                            patterninfo = SortMaxJumpIndex(patterninfo);
                            this.WaferAligner().ResetHeightPlanePoint();
                            retVal = ThetaAlign(ref patterninfo, _OperCancelTokenSource, ref rotateangle, false);
                        }

                        if (patterninfo != null & patterninfo.ErrorCode == EventCodeEnum.NONE & retVal == EventCodeEnum.NONE)
                        {
                            RotateAngle += rotateangle;
                            this.WaferAligner().WaferAlignInfo.VerifyAngle = rotateangle;

                            patterninfo.AcceptFocusing.Value = false;

                            foreach (var jindex in patterninfo.JumpIndexs)
                            {
                                jindex.AcceptFocusing.Value = false;
                            }

                            patterninfo.EnablePostJumpindex = HighStandardParam_Clone.EnablePostJumpindex;

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());

                            PMResult pmresult = this.VisionManager().PatternMatching(patterninfo, this);

                            this.VisionManager().StartGrab(patterninfo.CamType.Value, this);

                            retVal = pmresult.RetValue;

                            if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                            {
                                return retVal;
                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);


                                patterninfo.X.Value = wcoord.GetX() - patterninfo.WaferCenter.GetX();
                                patterninfo.Y.Value = wcoord.GetY() - patterninfo.WaferCenter.GetY();
                                patterninfo.Z.Value = wcoord.GetZ() - patterninfo.WaferCenter.GetZ();

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                    patterninfo.GetX() + patterninfo.WaferCenter.GetX(),
                                    patterninfo.GetY() + patterninfo.WaferCenter.GetY(),
                                    patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());

                            }
                            else
                            {
                                MachineCoordinate centerPoint = new MachineCoordinate(patterninfo.WaferCenter.GetX()
                                                , patterninfo.WaferCenter.GetY());

                                MachineCoordinate pivotPoint = new MachineCoordinate(patterninfo.GetX() + patterninfo.WaferCenter.GetX()
                                    , patterninfo.GetY() + patterninfo.WaferCenter.GetY());

                                MachineCoordinate wcoord = this.CoordinateManager().GetRotatedPoint(pivotPoint, centerPoint, RotateAngle);

                                patterninfo.X.Value = wcoord.GetX() - patterninfo.WaferCenter.GetX();
                                patterninfo.Y.Value = wcoord.GetY() - patterninfo.WaferCenter.GetY();
                                patterninfo.Z.Value = wcoord.GetZ() - patterninfo.WaferCenter.GetZ();

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                    patterninfo.GetX() + patterninfo.WaferCenter.GetX(),
                                    patterninfo.GetY() + patterninfo.WaferCenter.GetY(),
                                    patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());
                            }

                            patterninfo.MIndex = this.CoordinateManager().GetCurMachineIndex
                               (new WaferCoordinate(patterninfo.GetX() + patterninfo.WaferCenter.GetX(),
                                   patterninfo.GetY() + patterninfo.WaferCenter.GetY(),
                                   patterninfo.GetZ() + patterninfo.WaferCenter.GetZ()));


                            //if (this.WaferAligner().GetWAInnerStateEnum() == ProberInterfaces.Align.WaferAlignInnerStateEnum.SETUP)
                            //{
                            ImageBuffer img = this.VisionManager().SingleGrab(patterninfo.CamType.Value, this);

                            patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(
                               img, (img.SizeX / 2 - (patternparam.Width / 2)), (img.SizeY / 2 - (patternparam.Height / 2)),
                               patternparam.Width, patternparam.Height);

                            //}
                            //else if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.RECOVERY)
                            //{
                            //    tempPtInfo.Add(patterninfo);
                            //}



                            retVal = Verify(ref VerifyInofs);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = VerifyWaferCenter(patterninfo);
                            }

                            if (retVal != EventCodeEnum.NONE)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("",
                                      "This pattern is not valid for Align. Please register the pattern in another location."
                                      , EnumMessageStyle.Affirmative);
                            }
                            else
                            {

                                //patterninfo.PatternState.Value = PatternStateEnum.READY;
                                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING
                                    | this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                                    patterninfo.PatternState.Value = PatternStateEnum.READY;
                                else
                                    patterninfo.PatternState.Value = PatternStateEnum.MODIFY;
                                HighStandardParam_Clone.Patterns.Value.Add(patterninfo);
                                CurPatternIndex = HighStandardParam_Clone.Patterns.Value.Count;
                                if (!this.WaferAligner().IsNewSetup)
                                    SubModuleState = new SubModuleDoneState(this);

                                retVal = ApplyHeightProfiling(CurPatternIndex - 1);
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("High Pattern Register Error",
                                            "This pattern is not suitable for operating the set Height Profiling.Do you want to proceed anyway?.",
                                            EnumMessageStyle.AffirmativeAndNegative);
                                    if (ret == EnumMessageDialogResult.NEGATIVE)
                                        HighStandardParam_Clone.Patterns.Value.Remove(patterninfo);
                                }
                            }

                        }
                        else
                        {
                            if (patterninfo.ErrorCode == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                                return patterninfo.ErrorCode;

                            EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("High Pattern Register Error",
                             "This pattern is not valid for Align. Please register the pattern in another location.",
                             EnumMessageStyle.Affirmative);

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());
                        }

                    }


                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("High Pattern Register Error",
                          "This pattern is not valid for Align. Please register the pattern in another location."
                          , EnumMessageStyle.Affirmative);


                    this.StageSupervisor().StageModuleState.WaferHighViewMove
                        (Wafer.GetSubsInfo().WaferCenter.GetX(),
                        Wafer.GetSubsInfo().WaferCenter.GetY(),
                        Wafer.GetSubsInfo().WaferCenter.GetZ());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.RegistPattern() : Error occured.");
            }
            finally
            {
                patternCount = HighStandardParam_Clone.Patterns.Value.Count;
                StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                _OperCancelTokenSource = null;
                //await this.WaitCancelDialogService().CloseDialog();
            }
            return retVal;
        }

        //Recovery Setup 시 임시 데이터로 저장, 등록하기 위한 함수
        private async Task<EventCodeEnum> RecoverySetup_RegistPattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //==========================
                if (CurCam.GetChannelType() != HighStandardParam_Clone.CamType)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Pattern Register Error.",
                        "To register the Low pattern, please view the screen with Low camera and register again.",
                        EnumMessageStyle.Affirmative);
                    return retVal;
                }

                WaferCoordinate wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                _OperCancelTokenSource = new CancellationTokenSource();

                double rotateangle = 0.0;

                WAStandardPTInfomation temppatterninfo = new WAStandardPTInfomation();
                RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();

                if (patternparam.Width == 0 || patternparam.Height == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.",
                        "Check the pattern size."
                            , EnumMessageStyle.Affirmative
                            );
                    return EventCodeEnum.PATTERN_SIZE_ERROR;
                }
                //  await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                temppatterninfo.CamType.Value = CurCam.GetChannelType();
                temppatterninfo.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();

                HighStandardParam_Clone.DefaultPMParam.CopyTo(temppatterninfo.PMParameter);

                int ptindex = (HighStandardParam_Clone.Patterns.Value.Count + this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count);
                temppatterninfo.PMParameter.ModelFilePath.Value = HighStandardParam_Clone.PatternbasePath
                     + HighStandardParam_Clone.PatternName + ptindex.ToString();
                temppatterninfo.PMParameter.PatternFileExtension.Value = ".mmo";

                temppatterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(this.VisionManager().SingleGrab(temppatterninfo.CamType.Value, this), patternparam.LocationX, patternparam.LocationY, patternparam.Width, patternparam.Height);


                temppatterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;
                temppatterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;
                temppatterninfo.VerDirection.Value = EnumVerDirection.UPPERBOTTOM;

                //=====================================================================================

                bool IsFindJumpIndex = true;

                double registareaXlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) -
                    Math.Pow(OptimumLength, 2)) - HighStandardParam_Clone.DeadZone;

                double registareaYlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) -
                    Math.Pow(OptimumLength, 2));

                if (wcd.GetX() > -registareaXlimit && wcd.GetX() < registareaXlimit
                    && wcd.GetY() > -registareaYlimit && wcd.GetY() < registareaYlimit)
                {
                    temppatterninfo.WaferCenter = new WaferCoordinate();
                    this.WaferObject.GetSubsInfo().WaferCenter.CopyTo(temppatterninfo.WaferCenter);

                    wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                    temppatterninfo.X.Value = (wcd.X.Value - temppatterninfo.WaferCenter.GetX());
                    temppatterninfo.Y.Value = (wcd.Y.Value - temppatterninfo.WaferCenter.GetY());

                    retVal = ValidationTesting(temppatterninfo, FocusingModule, FocusingParam);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        if (_OperCancelTokenSource.Token.IsCancellationRequested)
                        {
                            return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                        }

                        await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.",
                            "Pattern validation failed. Please register another pattern.",
                            EnumMessageStyle.AffirmativeAndNegative);
                        if (HighStandardParam_Clone.Patterns.Value.Count == 0)
                            CurPatternIndex = 0;
                        return retVal;
                    }
                    else
                    {
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                        temppatterninfo.Z.Value = (wcd.Z.Value - temppatterninfo.WaferCenter.GetZ());

                        List<WAStandardPTInfomation> registPTInfo = new List<WAStandardPTInfomation>();
                        List<StandardJumpIndexParam> storagePTInfos = new List<StandardJumpIndexParam>();

                        ProcResults = new List<WaferProcResult>();

                        for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                        {
                            temppatterninfo.LightParams.Add(
                                new LightValueParam(CurCam.LightsChannels[lightindex].Type.Value,
                                (ushort)CurCam.GetLight(CurCam.LightsChannels[lightindex].Type.Value)));
                        }

                        if (ManualJumpIndexConfirm() != -1)
                        {
                            if (temppatterninfo.JumpIndexs == null)
                                temppatterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();

                            temppatterninfo = ManualJumpIndexApply(temppatterninfo);
                            temppatterninfo = UpdateHeightProfiling(temppatterninfo);

                            IsFindJumpIndex = false;
                        }

                        /* auto mode시 2번째 패턴 부터 fail시 주변 die retry를 안하는 이슈로 주석 처리 함
                        else
                        {
                            foreach (var item in HighStandardParam_Clone.Patterns.Value)
                            {
                                int retindex = item.JumpIndexs.ToList<StandardJumpIndexParam>().FindIndex(
                                    index => index.Index.XIndex != -1);

                                if (retindex >= 0)
                                {
                                    if (temppatterninfo.JumpIndexs == null)
                                        temppatterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();


                                    for (int index = 0; index < item.PostJumpIndex.Count; index++)
                                    {
                                        temppatterninfo.PostJumpIndex.Add(item.PostJumpIndex[index]);
                                    }
                                    temppatterninfo.PostProcDirection = item.PostProcDirection;
                                    for (int index = 0; index < item.JumpIndexs.Count; index++)
                                    {
                                        temppatterninfo.JumpIndexs.Add(item.JumpIndexs[index]);
                                    }
                                    temppatterninfo.ProcDirection = item.ProcDirection;
                                    IsFindJumpIndex = false;
                                    break;
                                }
                            }
                        }
                        */
                        if (IsFindJumpIndex)
                        {
                            temppatterninfo.PostProcDirection.Value = EnumWAProcDirection.HORIZONTAL;
                            temppatterninfo.PostHorDirection.Value = EnumHorDirection.RIGHTLEFT;
                            retVal = FindJumpIndex(ref temppatterninfo, ref rotateangle, patternparam,
                            ShortMiniumumLength, ShortOptimumLength, ShortMaximumLength, true, true);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                RotateAngle += rotateangle;
                                for (int index = 0; index < temppatterninfo.JumpIndexs.Count; index++)
                                {
                                    storagePTInfos.Add(temppatterninfo.JumpIndexs[index]);
                                }

                                temppatterninfo.JumpIndexs = null;
                                temppatterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;
                                temppatterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;
                                temppatterninfo.VerDirection.Value = EnumVerDirection.UPPERBOTTOM;

                                temppatterninfo.AcceptFocusing.Value = true;

                                retVal = FindJumpIndex(ref temppatterninfo, ref rotateangle, patternparam,
                                    MinimumLength, OptimumLength, MaximumLength, false, false);

                                if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                                    return retVal;

                                else if (temppatterninfo.ErrorCode == EventCodeEnum.NONE)
                                {



                                }
                                this.WaferAligner().WaferAlignInfo.AlignProcResult = ProcResults;
                            }
                            else
                            {
                            }

                        }
                        else
                        {
                            temppatterninfo = SortMaxJumpIndex(temppatterninfo);
                            this.WaferAligner().ResetHeightPlanePoint();
                            retVal = ThetaAlign(ref temppatterninfo, _OperCancelTokenSource, ref rotateangle, false);
                        }

                        if (temppatterninfo != null & temppatterninfo.ErrorCode == EventCodeEnum.NONE & retVal == EventCodeEnum.NONE)
                        {
                            RotateAngle += rotateangle;
                            this.WaferAligner().WaferAlignInfo.VerifyAngle = rotateangle;

                            temppatterninfo.AcceptFocusing.Value = false;
                            foreach (var jindex in temppatterninfo.JumpIndexs)
                            {
                                jindex.AcceptFocusing.Value = false;
                            }
                            temppatterninfo.EnablePostJumpindex = HighStandardParam_Clone.EnablePostJumpindex;
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX(),
                                temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY(),
                                temppatterninfo.GetZ() + temppatterninfo.WaferCenter.GetZ());


                            PMResult pmresult = this.VisionManager().PatternMatching(temppatterninfo, this);

                            this.VisionManager().StartGrab(temppatterninfo.CamType.Value, this);

                            retVal = pmresult.RetValue;

                            if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                            {
                                return retVal;
                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);


                                temppatterninfo.X.Value = wcoord.GetX() - temppatterninfo.WaferCenter.GetX();
                                temppatterninfo.Y.Value = wcoord.GetY() - temppatterninfo.WaferCenter.GetY();
                                temppatterninfo.Z.Value = wcoord.GetZ() - temppatterninfo.WaferCenter.GetZ();

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                    temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX(),
                                    temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY(),
                                    temppatterninfo.GetZ() + temppatterninfo.WaferCenter.GetZ());

                            }
                            else
                            {
                                MachineCoordinate centerPoint = new MachineCoordinate(temppatterninfo.WaferCenter.GetX()
                                                , temppatterninfo.WaferCenter.GetY());

                                MachineCoordinate pivotPoint = new MachineCoordinate(temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX()
                                    , temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY());

                                MachineCoordinate wcoord = this.CoordinateManager().GetRotatedPoint(pivotPoint, centerPoint, RotateAngle);

                                temppatterninfo.X.Value = wcoord.GetX() - temppatterninfo.WaferCenter.GetX();
                                temppatterninfo.Y.Value = wcoord.GetY() - temppatterninfo.WaferCenter.GetY();
                                temppatterninfo.Z.Value = wcoord.GetZ() - temppatterninfo.WaferCenter.GetZ();

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                    temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX(),
                                    temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY(),
                                    temppatterninfo.GetZ() + temppatterninfo.WaferCenter.GetZ());
                            }

                            temppatterninfo.MIndex = this.CoordinateManager().GetCurMachineIndex
                               (new WaferCoordinate(temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX(),
                                   temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY(),
                                   temppatterninfo.GetZ() + temppatterninfo.WaferCenter.GetZ()));

                            ImageBuffer img = this.VisionManager().SingleGrab(temppatterninfo.CamType.Value, this);

                            temppatterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(
                               img, (img.SizeX / 2 - (patternparam.Width / 2)), (img.SizeY / 2 - (patternparam.Height / 2)),
                               patternparam.Width, patternparam.Height);


                            retVal = Verify(ref VerifyInofs);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = RecoverySteup_VerifyWaferCenter(temppatterninfo);
                            }

                            if (retVal != EventCodeEnum.NONE)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("",
                                      "This pattern is not valid for Align. Please register the pattern in another location."
                                      , EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                                    temppatterninfo.PatternState.Value = PatternStateEnum.READY;
                                else
                                    temppatterninfo.PatternState.Value = PatternStateEnum.MODIFY;
                                this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Add(temppatterninfo);
                                CurTempPatternIndex = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count;
                                temppatternCount = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count;

                                SetNodeSetupState(EnumMoudleSetupState.VERIFY);
                                //if (!this.WaferAligner().IsNewSetup)
                                //    SubModuleState = new SubModuleDoneState(this);

                                retVal = ApplyHeightProfiling(CurPatternIndex - 1);
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("High Pattern Register Error",
                                            "This pattern is not suitable for operating the set Height Profiling.Do you want to proceed anyway?.",
                                            EnumMessageStyle.AffirmativeAndNegative);
                                    if (ret == EnumMessageDialogResult.NEGATIVE)
                                        this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Remove(temppatterninfo);
                                }
                            }

                        }
                        else
                        {
                            if (temppatterninfo.ErrorCode == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                                return temppatterninfo.ErrorCode;

                            EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("High Pattern Register Error",
                             "This pattern is not valid for Align. Please register the pattern in another location.",
                             EnumMessageStyle.Affirmative);

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());
                        }

                    }


                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("High Pattern Register Error",
                          "This pattern is not valid for Align. Please register the pattern in another location."
                          , EnumMessageStyle.Affirmative);


                    this.StageSupervisor().StageModuleState.WaferHighViewMove
                        (Wafer.GetSubsInfo().WaferCenter.GetX(),
                        Wafer.GetSubsInfo().WaferCenter.GetY(),
                        Wafer.GetSubsInfo().WaferCenter.GetZ());
                }
                //if (retVal == EventCodeEnum.NONE)
                //{
                //    SubModuleState = new SubModuleSkipState(this);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.RegistPattern() : Error occured.");
            }
            finally
            {
                CurPatternIndex = 0;
                StepLabel = String.Format("REGISTED PATTERN {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                _OperCancelTokenSource = null;

                if (temppatternCount > 0)
                {
                    FourButton.IsEnabled = false;
                }
                else
                {
                    FourButton.IsEnabled = true;
                }
            }
            return retVal;
        }
        #region //..Old RegistPattern

        //private async Task<EventCodeEnum> RegistPattern()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        if (CurCam.GetChannelType() != HighStandardParam.CamType.Value)
        //        {
        //            await this.MetroDialogManager().ShowMessageDialog("Pattern registration Error.",
        //                "To register the Low pattern, please view the screen with Low camera and register again.",
        //                EnumMessageStyle.Affirmative);
        //            return retVal;
        //        }

        //        PadJogRightDown.IsEnabled = true;
        //        PatternRegisteCancelTokenSource = new CancellationTokenSource();
        //        //await this.WaitCancelDialogService().ShowDialog("Registe High Pattern . Please wait for a while. ", null, false);

        //        double rotateangle = 0.0;

        //        //.. PatternInfo 
        //        WAStandardPTInfomation patterninfo = new WAStandardPTInfomation();
        //        RegistePatternParam param = GetPatternRectInfo();
        //        patterninfo.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();
        //        patterninfo.PMParameter = DefaultPMParam;
        //        patterninfo.CamType.Value = CurCam.GetChannelType();
        //        patterninfo.FocusingModel = FocusingModule;
        //        patterninfo.FocusParam = FocusingParam;

        //        string RootPath = this.FileManager().FileManagerParam.DeviceParamRootDirectory +
        //         "\\" + this.FileManager().FileManagerParam.DeviceName;
        //        patterninfo.PMParameter.PMModelPath.Value = RootPath + HighStandardParam.PatternbasePath
        //             + HighStandardParam.PatternName + patternindex.ToString();
        //        patterninfo.PMParameter.Extension.Value = ".mmo";

        //        patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(
        //            this.VisionManager().SingleGrab(patterninfo.CamType.Value),
        //            param.LocationX, param.LocationY, param.Width, param.Height);

        //        //=====================================================================================

        //        WaferCoordinate wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

        //        bool IsFindJumpIndex = true;

        //        double registareaXlimit = Math.Sqrt(Math.Pow((Wafer.PhysInfo.WaferSize.Value), 2) -
        //            Math.Pow(OptimumLength, 2)) - HighStandardParam.DeadZone.Value;

        //        double registareaYlimit = Math.Sqrt(Math.Pow((Wafer.PhysInfo.WaferSize.Value), 2) -
        //            Math.Pow(OptimumLength, 2));

        //        if (wcd.GetX() > -registareaXlimit && wcd.GetX() < registareaXlimit
        //            && wcd.GetY() > -registareaYlimit && wcd.GetY() < registareaYlimit)
        //        {

        //            patterninfo.X.Value = (wcd.X.Value - Wafer.SubsInfo.WaferCenter.GetX());
        //            patterninfo.Y.Value = (wcd.Y.Value - Wafer.SubsInfo.WaferCenter.GetY());
        //            patterninfo.Z.Value = (wcd.Z.Value - Wafer.SubsInfo.WaferCenter.GetZ());

        //            retVal = VaildationTesting(patterninfo);
        //            if (retVal != EventCodeEnum.NONE)
        //            {

        //                 await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.", 
        //                     "Pattern validation failed. Please register another pattern.",
        //                     EnumMessageStyle.AffirmativeAndNegative);
        //                CurPatternIndex = 0;
        //            }
        //            else
        //            {
        //                List<WAStandardPTInfomation> registPTInfo = new List<WAStandardPTInfomation>();
        //                List<StandardJumpIndexParam> storagePTInfos = new List<StandardJumpIndexParam>();


        //                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
        //                {
        //                    patterninfo.LightParams.Add(
        //                        new LightValueParam(CurCam.LightsChannels[lightindex].Type.Value,
        //                        (ushort)CurCam.GetLight(CurCam.LightsChannels[lightindex].Type.Value)));
        //                }


        //                //foreach (var item in HighStandardParam.Patterns.Value)
        //                //{
        //                //    int retindex = item.JumpIndexs.ToList<StandardJumpIndexParam>().FindIndex(
        //                //        index => index.Index.XIndex.Value != -1);

        //                //    if (retindex >= 0)
        //                //    {
        //                //        if (patterninfo.JumpIndexs == null)
        //                //            patterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();

        //                //        for (int index = 0; index < item.JumpIndexs.Count; index++)
        //                //        {
        //                //            patterninfo.JumpIndexs.Add(item.JumpIndexs[index]);
        //                //        }
        //                //        IsFindJumpIndex = false;
        //                //        break;
        //                //    }
        //                //}

        //                //registPTInfo.Add(patterninfo);

        //                //ProcResults = new List<WaferProcResult>();

        //                //if (IsFindJumpIndex == true)
        //                //{
        //                //    patterninfo.ProcDirection.Value = EnumWAProcDirection.VERTICAL;
        //                //    retVal = FindJumpIndex(ref registPTInfo, ref patterninfo, ref rotateangle, false, AcceptMiniumumLength,
        //                //        AcceptOptimumLength, AcceptMaximumLength, false);
        //                //    if (retVal == EventCodeEnum.NONE)
        //                //    {
        //                //        RotateAngle += rotateangle;
        //                //        this.StageSupervisor().PnpMotionJog.IndexMoveX = Wafer.SubsInfo.ActualIndexSize.Width.Value;

        //                //        for (int index = 0; index < registPTInfo[0].JumpIndexs.Count; index++)
        //                //        {
        //                //            storagePTInfos.Add(registPTInfo[0].JumpIndexs[index]);
        //                //        }
        //                //        registPTInfo[0].JumpIndexs = null;
        //                //        registPTInfo[0].AcceptFocusing.Value = true;
        //                //        //registPTInfo[0].FocusParam = HighStandardParam.FocusParam;
        //                //        registPTInfo[0].FocusParam = FocusingParam;

        //                //        patterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;
        //                //        retVal = FindJumpIndex(ref registPTInfo, ref patterninfo, ref rotateangle, false,
        //                //            MinimumLength, OptimumLength, MaximumLength, true);

        //                //    }

        //                //}
        //                //else
        //                //{
        //                //    patterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;

        //                //}
        //                //retVal = FindJumpIndex(ref registPTInfo, ref patterninfo, ref rotateangle, false, MinimumLength, OptimumLength, MaximumLength, true);
        //                //if (retVal == EventCodeEnum.NONE)
        //                //{

        //                //    RotateAngle += rotateangle;

        //                //    this.StageSupervisor().PnpMotionJog.IndexMoveX = Wafer.SubsInfo.ActualIndexSize.Width.Value;
        //                //    this.StageSupervisor().PnpMotionJog.IndexMoveY = Wafer.SubsInfo.ActualIndexSize.Height.Value;

        //                //    //=============Add ProcData

        //                //    foreach (var result in ProcResults)
        //                //    {
        //                //        Wafer.SubsInfo.AlignProcResult.Add(result);
        //                //    }
        //                //    Wafer.SubsInfo.VerifyAngle = rotateangle;

        //                //=============
        //                ObservableCollection<WAStandardPTInfomation> ptinfos =
        //                          new ObservableCollection<WAStandardPTInfomation>();

        //                    patterninfo.PatternState.Value = PatternStateEnum.READY;
        //                    patterninfo.MIndex = this.CoordinateManager().GetCurMachineIndex
        //                        (new WaferCoordinate(patterninfo.GetX(),patterninfo.GetX()));

        //                    if (this.WaferAligner().WaferAlignerGetState() == ProberInterfaces.Align.AlignStateEnum.SETUP)
        //                    {

        //                        if (HighStandardParam.Patterns.Value.Count >= patternindex + 1)
        //                            HighStandardParam.Patterns.Value.RemoveAt(patternindex);


        //                        if (ConfirmExistManualJumpIndex.Confirm(DefaultJumpIndexParam,EnumWAProcDirection.BIDIRECTIONAL) != -1)
        //                        {
        //                            if (patterninfo.JumpIndexs == null)
        //                                patterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();

        //                            patterninfo.JumpIndexs =
        //                                ConfirmExistManualJumpIndex.FindJumpIndex(DefaultJumpIndexParam, EnumWAProcDirection.BIDIRECTIONAL, patterninfo.JumpIndexs);
        //                        }
        //                        else
        //                        {
        //                            if (HighStandardParam.Patterns.Value.Count != 0)
        //                            {
        //                                for (int index = 0; index < HighStandardParam.Patterns.Value.Count; index++)
        //                                {
        //                                    ptinfos.Add(HighStandardParam.Patterns.Value[index]);
        //                                    for (int jndex = 0; jndex < registPTInfo.Count; jndex++)
        //                                    {
        //                                        ptinfos.Add(registPTInfo[jndex]);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                for (int jndex = 0; jndex < registPTInfo.Count; jndex++)
        //                                {
        //                                    for (int lndex = 0; lndex < storagePTInfos.Count; lndex++)
        //                                    {
        //                                        registPTInfo[0].JumpIndexs.Insert(lndex, storagePTInfos[lndex]);
        //                                    }
        //                                    ptinfos.Add(registPTInfo[jndex]);
        //                                }
        //                            }
        //                        }

        //                        HighStandardParam.Patterns.Value = ptinfos;
        //                        this.WaferAligner().HighMeasurementTable.MeasurementInfos.Add(
        //                            new ThetaAlignMeasurementInfo(ptinfos[ptinfos.Count() - 1]));

        //                    }
        //                    else if (this.WaferAligner().WaferAlignerGetState() == AlignStateEnum.RECOVERY)
        //                    {
        //                        for (int index = 0; index < registPTInfo.Count; index++)
        //                        {
        //                            tempPtInfo.Add(new WAStandardPTInfomation(registPTInfo[index]));
        //                        }
        //                    }

        //                    CurPatternIndex = HighStandardParam.Patterns.Value.Count + tempPtInfo.Count;
        //                //}

        //                if (retVal != EventCodeEnum.NONE)
        //                {
        //                    //Message
        //                    //Align을 하기에 유효하지 않은 패턴입니다. 다른 위치에 패턴을 등록해 주시기 바랍니다.
        //                    //This pattern is not valid for Align. Please register the pattern in another location.
        //                        DiaglogResultEnum ret = await DialogService.ShowMessage("",
        //                            "This pattern is not valid for Align. Please register the pattern in another location.");
        //                }
        //            }
        //        }
        //        else
        //        {

        //            this.StageSupervisor().StageModuleState.WaferHighViewMove
        //                (Wafer.SubsInfo.WaferCenter.GetX(),
        //                Wafer.SubsInfo.WaferCenter.GetY(),
        //                Wafer.SubsInfo.WaferCenter.GetZ());

        //            //Message
        //            //Align 하기에 적절하지 않은 위치입니다.다른 위치에 패던을 등록해 주시기 바랍니다
        //            //Align is not a good place to be.. Please register your padan in another location.

        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"{err.ToString()}.RegistPattern() : Error occured.");
        //    }
        //    finally
        //    {
        //        this.VisionManager().StartGrab(CurCam.GetChannelType());
        //        PadJogRightDown.IsEnabled = false;
        //        PatternRegisteCancelTokenSource = null;
        //        //await this.WaitCancelDialogService().CloseDialog();
        //    }
        //    return retVal;
        //}

        #endregion

        //private EventCodeEnum FindJumpIndex(ref List<WAStandardPTInfomation> RegistPTInfo , ref WAStandardPTInfomation patterninfo , 
        //    ref double rotateAngle,bool doAlign,double minimumlength = 0.0, double optimumlength = 0.0, double maximumlength=0.0 , bool originproc = true)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    int count = 0;

        //    retVal =
        //            ThetaAlign(ref RegistPTInfo, ref ProcResults, ref rotateAngle, _OperCancelTokenSource, doAlign, originproc,minimumlength, optimumlength, maximumlength);
        //    if (retVal == EventCodeEnum.NONE)
        //    {

        //        this.StageSupervisor().StageModuleState.WaferHighViewMove(
        //            patterninfo.GetX() + Wafer.SubsInfo.WaferCenter.GetX(),
        //            patterninfo.GetY() + Wafer.SubsInfo.WaferCenter.GetY());

        //        PMResult pmresult = this.VisionManager().PatternMatching(patterninfo);

        //        this.VisionManager().StartGrab(patterninfo.CamType.Value);

        //        WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);

        //        patterninfo.X.Value = wcoord.GetX() - Wafer.SubsInfo.WaferCenter.GetX();
        //        patterninfo.Y.Value = wcoord.GetY() - Wafer.SubsInfo.WaferCenter.GetY();

        //        this.StageSupervisor().StageModuleState.WaferHighViewMove(
        //            patterninfo.GetX() + Wafer.SubsInfo.WaferCenter.GetX(),
        //            patterninfo.GetY() + Wafer.SubsInfo.WaferCenter.GetY());
        //    }
        //    else
        //    {
        //        if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCLE)
        //            return retVal;

        //        if(count >0)
        //        {
        //            return retVal;
        //        }
        //        count++;
        //        foreach (var ptinto in RegistPTInfo)
        //        {
        //            ptinto.JumpIndexs = null;
        //            //foreach (var jindex in ptinto.JumpIndexs)
        //            //{
        //            //    jindex = null;
        //            //}
        //        }

        //        patterninfo.ProcDirection.Value = EnumWAProcDirection.VERTICAL;
        //        retVal = FindJumpIndex(ref RegistPTInfo, ref patterninfo, ref rotateAngle, false, AcceptMiniumumLength,
        //            AcceptOptimumLength, AcceptMaximumLength, false);
        //        if (retVal == EventCodeEnum.NONE)
        //        {
        //            RotateAngle += rotateAngle;
        //            this.StageSupervisor().PnpMotionJog.IndexMoveX = Wafer.SubsInfo.ActualIndexSize.Width.Value;

        //            RegistPTInfo[0].JumpIndexs = null;
        //            RegistPTInfo[0].AcceptFocusing.Value = true;
        //            //registPTInfo[0].FocusParam = HighStandardParam.FocusParam;

        //            patterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;
        //            retVal = FindJumpIndex(ref RegistPTInfo, ref patterninfo, ref rotateAngle, false,
        //                MinimumLength, OptimumLength, MaximumLength, true);

        //        }
        //        if(retVal != EventCodeEnum.NONE)
        //        {
        //            HighStandardParam.Patterns.Value.RemoveAt(patternindex);
        //            System.IO.FileInfo file = new FileInfo(patterninfo.PMParameter.PMModelPath.Value);
        //            file.Delete();
        //        }
        //        else
        //        {
        //            this.StageSupervisor().StageModuleState.WaferHighViewMove(
        //           patterninfo.GetX() + Wafer.SubsInfo.WaferCenter.GetX(),
        //           patterninfo.GetY() + Wafer.SubsInfo.WaferCenter.GetY());

        //            PMResult pmresult = this.VisionManager().PatternMatching(patterninfo);

        //            this.VisionManager().StartGrab(patterninfo.CamType.Value);

        //            WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);

        //            patterninfo.X.Value = wcoord.GetX() - Wafer.SubsInfo.WaferCenter.GetX();
        //            patterninfo.Y.Value = wcoord.GetY() - Wafer.SubsInfo.WaferCenter.GetY();

        //            this.StageSupervisor().StageModuleState.WaferHighViewMove(
        //                patterninfo.GetX() + Wafer.SubsInfo.WaferCenter.GetX(),
        //                patterninfo.GetY() + Wafer.SubsInfo.WaferCenter.GetY());
        //        }

        //    }

        //    return retVal;
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="patterninfo"></param>
        /// <param name="rotateAngle"></param>
        /// <param name="patternparam"></param>
        /// <param name="minimumlength"></param>
        /// <param name="optimumlength"></param>
        /// <param name="maximumlength"></param>
        /// <param name="originproc"></param>
        /// <param name="hojumpindex"></param>
        /// <returns></returns>
        private EventCodeEnum FindJumpIndex(ref WAStandardPTInfomation patterninfo,
            ref double rotateAngle, RegisteImageBufferParam patternparam,
            double minimumlength, double optimumlength, double maximumlength, bool originproc, bool ispost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            patterninfo =
                    ThetaAlign(patterninfo, ref rotateAngle, _OperCancelTokenSource, minimumlength, optimumlength, maximumlength, originproc, ProcResults, ispost, this.WaferAligner().IsNewSetup);
            if (patterninfo != null && patterninfo.ErrorCode == EventCodeEnum.NONE)
            {
                if (patterninfo.ErrorCode == EventCodeEnum.NONE)
                {
                    retVal = patterninfo.ErrorCode;

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());

                    PMResult pmresult = this.VisionManager().PatternMatching(patterninfo, this);

                    this.VisionManager().StartGrab(patterninfo.CamType.Value, this);

                    retVal = pmresult.RetValue;

                    if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                        return retVal;


                    if (retVal == EventCodeEnum.NONE)
                    {
                        WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);

                        patterninfo.X.Value = wcoord.GetX() - patterninfo.WaferCenter.GetX();
                        patterninfo.Y.Value = wcoord.GetY() - patterninfo.WaferCenter.GetY();
                        patterninfo.Z.Value = wcoord.GetZ() - patterninfo.WaferCenter.GetZ();

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            patterninfo.GetX() + patterninfo.WaferCenter.GetX(),
                            patterninfo.GetY() + patterninfo.WaferCenter.GetY(),
                            patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());

                    }
                    else
                    {
                        MachineCoordinate centerPoint = new MachineCoordinate(patterninfo.WaferCenter.GetX()
                                        , patterninfo.WaferCenter.GetY());

                        MachineCoordinate pivotPoint = new MachineCoordinate(patterninfo.GetX() + patterninfo.WaferCenter.GetX()
                            , patterninfo.GetY() + patterninfo.WaferCenter.GetY());

                        MachineCoordinate wcoord = this.CoordinateManager().GetRotatedPoint(pivotPoint, centerPoint, RotateAngle);

                        patterninfo.X.Value = wcoord.GetX() - patterninfo.WaferCenter.GetX();
                        patterninfo.Y.Value = wcoord.GetY() - patterninfo.WaferCenter.GetY();
                        patterninfo.Z.Value = wcoord.GetZ() - patterninfo.WaferCenter.GetZ();

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            patterninfo.GetX() + patterninfo.WaferCenter.GetX(),
                            patterninfo.GetY() + patterninfo.WaferCenter.GetY(),
                            patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());
                    }
                }
                if (retVal == EventCodeEnum.NONE)
                {
                    patterninfo.MIndex = this.CoordinateManager().GetCurMachineIndex
                           (new WaferCoordinate(patterninfo.GetX() + patterninfo.WaferCenter.GetX(),
                               patterninfo.GetY() + patterninfo.WaferCenter.GetY(),
                               patterninfo.GetZ() + patterninfo.WaferCenter.GetZ()));

                    foreach (var info in patterninfo.JumpIndexs)
                    {
                        info.AcceptFocusing.Value = false;
                    }

                    ImageBuffer img = this.VisionManager().SingleGrab(patterninfo.CamType.Value, this);

                    patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(
                       img, (img.SizeX / 2 - (patternparam.Width / 2)), (img.SizeY / 2 - (patternparam.Height / 2)),
                       patternparam.Width, patternparam.Height);
                }
                else
                {
                    retVal = EventCodeEnum.WAFER_JUMPINDEX_NOT_FOUND;
                }
            }

            return retVal;
        }

        #endregion


        private EventCodeEnum DeletePattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (HighStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    int precount = HighStandardParam_Clone.Patterns.Value.Count;
                    HighStandardParam_Clone.Patterns.Value.RemoveAt(CurPatternIndex - 1);
                    if (HighStandardParam_Clone.Patterns.Value.Count == precount - 1 & CurPatternIndex != 1)
                        CurPatternIndex--;
                    if (HighStandardParam_Clone.Patterns.Value.Count == 0)
                        CurPatternIndex = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                patternCount = HighStandardParam_Clone.Patterns.Value.Count;
                StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);
            }

            return retVal;
        }

        private EventCodeEnum RecoverySetup_DeletePattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count > 0)
                {
                    int removeindex = CurTempPatternIndex - 1;
                    if (CurTempPatternIndex == 0)
                    {
                        removeindex = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count - 1;
                    }

                    this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.RemoveAt(removeindex);
                }

                CurTempPatternIndex = 0;
                temppatternCount = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count;

                if (temppatternCount == 0)
                {
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);

                if (temppatternCount > 0)
                {
                    FourButton.IsEnabled = false;
                }
                else
                {
                    FourButton.IsEnabled = true;
                }
            }
            return retVal;
        }
        #region //..HeightProfiling


        private EventCodeEnum ProcessingHeightProfiling()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                WA_HighMagParam_Standard param = (HighStandard_IParam as WA_HighMagParam_Standard);
                ObservableCollection<WAHeightPositionParam> heightparam = param.HeightPosParams;

                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    heightparam = HighStandardParam_Clone.HeightPosParams;
                }

                if (param.HeightProfilingPointType.Value == HeightPointEnum.POINT9)
                {
                    var ptinfo = param.Patterns.Value.FirstOrDefault();

                    if (ptinfo != null)
                    {
                        List<IDeviceObject> devices = this.StageSupervisor.WaferObject.GetDevices();
                        IFocusing focusing = FocusingModule;

                        Point ptleftcorner = this.WaferAligner().GetLeftCornerPosition(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY());

                        double leftcornerptoffsetx = (ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX()) - ptleftcorner.X;
                        double leftcornerptoffsety = (ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY()) - ptleftcorner.Y;

                        long xMaxIndex = Wafer.GetSubsInfo().Devices.Max(xindex => xindex.DieIndexM.XIndex);
                        long xMinIndex = Wafer.GetSubsInfo().Devices.Min(xindex => xindex.DieIndexM.XIndex);
                        long yMaxIndex = Wafer.GetSubsInfo().Devices.Max(yindex => yindex.DieIndexM.YIndex);
                        long yMinIndex = Wafer.GetSubsInfo().Devices.Min(yindex => yindex.DieIndexM.YIndex);

                        long xIndex = 0;
                        long yIndex = 0;

                        //RightUpper
                        xIndex = xMaxIndex;
                        yIndex = yMaxIndex;

                        while (true)
                        {
                            if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety, this.WaferObject.GetSubsInfo().ActualThickness);

                                // focusing.Focusing_Retry
                                retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    if (heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.RIGHTUPPER).FirstOrDefault() == null)
                                    {
                                        heightparam.Add(new WAHeightPositionParam(this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.RIGHTUPPER));
                                    }
                                    else
                                    {
                                        heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.RIGHTUPPER).FirstOrDefault().Position = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                                    }

                                    var heightpos = _HeightPositions.Where(pos => pos.PosEnum == HeightProfilignPosEnum.RIGHTUPPER).FirstOrDefault();

                                    if (heightpos == null)
                                    {
                                        _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                                    }
                                    else
                                    {
                                        heightpos = HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1];
                                    }

                                    this.WaferAligner().AddHeighPlanePoint(HighStandardParam_Clone.HeightPosParams.Last());
                                    heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.RIGHTUPPER).HeightProfilingVal = Wafer.WaferHeightMapping.PlanPoints.Last().Z.Value;

                                    break;
                                }
                                else
                                {
                                    xIndex--;
                                    yIndex--;
                                }
                            }
                            else
                            {
                                xIndex--;
                                yIndex--;
                            }

                            //_delays.DelayFor(1);
                        }

                        //LeftUpper
                        xIndex = xMinIndex;
                        yIndex = yMaxIndex;

                        while (true)
                        {
                            if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety, this.WaferObject.GetSubsInfo().ActualThickness);

                                // focusing.Focusing_Retry
                                retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    if (heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.LEFTUPPER).FirstOrDefault() == null)
                                    {
                                        heightparam.Add(new WAHeightPositionParam(this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.LEFTUPPER));
                                    }
                                    else
                                    {
                                        heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.LEFTUPPER).FirstOrDefault().Position = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                                    }

                                    var heightpos = _HeightPositions.Where(pos => pos.PosEnum == HeightProfilignPosEnum.LEFTUPPER).FirstOrDefault();
                                    if (heightpos == null)
                                    {
                                        _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                                    }
                                    else
                                    {
                                        heightpos = HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1];
                                    }

                                    this.WaferAligner().AddHeighPlanePoint(HighStandardParam_Clone.HeightPosParams.Last());
                                    heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.LEFTUPPER).HeightProfilingVal = Wafer.WaferHeightMapping.PlanPoints.Last().Z.Value;

                                    break;
                                }
                                else
                                {
                                    xIndex++;
                                    yIndex--;
                                }
                            }
                            else
                            {
                                xIndex++;
                                yIndex--;
                            }

                            //_delays.DelayFor(1);
                        }

                        //LeftBottom

                        xIndex = xMinIndex;
                        yIndex = yMinIndex;

                        while (true)
                        {
                            if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety, this.WaferObject.GetSubsInfo().ActualThickness);

                                // focusing.Focusing_Retry
                                retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    if (heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.LEFTBOTTOM).FirstOrDefault() == null)
                                    {
                                        heightparam.Add(new WAHeightPositionParam(this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.LEFTBOTTOM));
                                    }
                                    else
                                    {
                                        heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.LEFTBOTTOM).FirstOrDefault().Position = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                                    }

                                    var heightpos = _HeightPositions.Where(pos => pos.PosEnum == HeightProfilignPosEnum.LEFTBOTTOM).FirstOrDefault();

                                    if (heightpos == null)
                                    {
                                        _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                                    }
                                    else
                                    {
                                        heightpos = HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1];
                                    }

                                    this.WaferAligner().AddHeighPlanePoint(HighStandardParam_Clone.HeightPosParams.Last());
                                    heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.LEFTBOTTOM).HeightProfilingVal = Wafer.WaferHeightMapping.PlanPoints.Last().Z.Value;

                                    break;
                                }
                                else
                                {
                                    xIndex++;
                                    yIndex++;
                                }
                            }
                            else
                            {
                                xIndex++;
                                yIndex++;
                            }

                            //_delays.DelayFor(1);
                        }

                        //RightBottom
                        xIndex = xMaxIndex;
                        yIndex = yMinIndex;

                        while (true)
                        {
                            if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety, this.WaferObject.GetSubsInfo().ActualThickness);

                                // focusing.Focusing_Retry
                                retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    if (heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.RIGHTBOTTOM).FirstOrDefault() == null)
                                    {
                                        heightparam.Add(new WAHeightPositionParam(this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.RIGHTBOTTOM));
                                    }
                                    else
                                    {
                                        heightparam.Where(pos => pos.PosEnum == HeightProfilignPosEnum.RIGHTBOTTOM).FirstOrDefault().Position = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                                    }

                                    var heightpos = _HeightPositions.Where(pos => pos.PosEnum == HeightProfilignPosEnum.RIGHTBOTTOM).FirstOrDefault();
                                    if (heightpos == null)
                                    {
                                        _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                                    }
                                    else
                                    {
                                        heightpos = HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1];
                                    }

                                    this.WaferAligner().AddHeighPlanePoint(HighStandardParam_Clone.HeightPosParams.Last());
                                    heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.RIGHTBOTTOM).HeightProfilingVal = Wafer.WaferHeightMapping.PlanPoints.Last().Z.Value;

                                    break;
                                }
                                else
                                {
                                    xIndex--;
                                    yIndex++;
                                }
                            }
                            else
                            {
                                xIndex--;
                                yIndex++;
                            }

                            //_delays.DelayFor(1);
                        }
                        this.WaferAligner().PlanePointChangetoFocusing9pt(heightparam);
                    }
                    else
                    {
                        LoggerManager.Error($"[HighStandard], ProcessingHeightProfiling() : ptinfo is null.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"Height Profiling Mode = {param.HeightProfilingPointType.Value}");
                    retVal = EventCodeEnum.NONE;
                }

                this.WaferAligner().WaferAlignInfo.HeightPositions = _HeightPositions;
                param.HeightPosParams = heightparam;

                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    HighStandardParam_Clone.HeightPosParams = heightparam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }
        public EventCodeEnum ApplyHeightProfiling(int index = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                HighStandardParam_Clone.HeightPosParams.Clear();
                switch (HighStandardParam_Clone.HeightProfilingPointType.Value)
                {
                    case HeightPointEnum.POINT1:
                        foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                        {
                            foreach (var procpos in ptinfo.JumpIndexs)
                            {
                                if (procpos.Index.XIndex == 0 && procpos.Index.YIndex == 0)
                                {
                                    procpos.AcceptFocusing.Value = true;
                                    //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY()));
                                }
                                else
                                {
                                    procpos.AcceptFocusing.Value = false;
                                }
                            }
                        }
                        retVal = EventCodeEnum.NONE;
                        break;
                    case HeightPointEnum.POINT5:
                        foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                        {
                            SortMaxJumpIndex(ptinfo);
                        }
                        retVal = EventCodeEnum.NONE;
                        break;
                    case HeightPointEnum.POINT9:
                        foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                        {
                            SortMaxJumpIndex(ptinfo);
                            if (index == -1)
                                retVal = HeightProfilingEdge(ptinfo);
                            else
                                retVal = EventCodeEnum.NONE;
                            if (retVal != EventCodeEnum.NONE)
                                break;
                        }

                        retVal = HeightProfilingEdge(HighStandardParam_Clone.Patterns.Value[index]);

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                             HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(),
                             HighStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].GetY() + Wafer.GetSubsInfo().WaferCenter.GetY(),
                             Wafer.GetSubsInfo().ActualThickness);

                        break;
                }

                this.WaferAligner().WaferAlignInfo.HeightPositions = _HeightPositions;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public EventCodeEnum CheckHeightProfiling(double limit)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double minVal = 0.0;
                double maxVal = 0.0;
                double alignPlanarityLimit = limit;

                if (Wafer.WaferHeightMapping.PlanPoints != null)
                {
                    foreach (var point in Wafer.WaferHeightMapping.PlanPoints)
                    {
                        if (minVal == 0.0) minVal = point.GetZ();
                        if (maxVal == 0.0) maxVal = point.GetZ();

                        minVal = (minVal <= point.GetZ()) ? minVal : point.GetZ();
                        maxVal = (maxVal >= point.GetZ()) ? maxVal : point.GetZ();
                    }

                    LoggerManager.Debug($"Wafer Plamarity MinHeight : {minVal}, MaxHeight : {maxVal}");

                    if ((maxVal - minVal) > alignPlanarityLimit)
                    {
                        LoggerManager.Debug($"Wafer Planarity Too *LARGE*! ( {maxVal - minVal} um )");
                        retVal = EventCodeEnum.WAFER_ALIGN_PLANARITY_OVER_TOLERANCE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum UpdateHeightProfiling()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"WaferAligner UpdateHeightProfiling(): High Pattern Index Count : {HighStandardParam_Clone.Patterns.Value}, Point : {HighStandardParam_Clone.HeightProfilingPointType.Value} ");

                if (HighStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    if (HighStandardParam_Clone.HeightProfilingPointType.Value == HeightPointEnum.POINT1)
                    {
                        foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                        {
                            foreach (var postjumpindex in ptinfo.PostJumpIndex)
                            {

                                if (postjumpindex.Index.XIndex == 0 && postjumpindex.Index.YIndex == 0)
                                {
                                    postjumpindex.AcceptFocusing.Value = true;
                                }
                                else
                                {
                                    postjumpindex.AcceptFocusing.Value = false;
                                }
                            }

                            foreach (var jumpindex in ptinfo.JumpIndexs)
                            {
                                jumpindex.AcceptFocusing.Value = false;
                            }
                        }
                    }
                    else
                    {
                        foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                        {
                            SortMaxJumpIndex(ptinfo);
                            foreach (var jumpindex in ptinfo.JumpIndexs)
                            {
                                jumpindex.AcceptFocusing.Value = true;
                            }
                        }

                    }

                }
                this.WaferAligner().WaferAlignInfo.HeightPositions = _HeightPositions;

                this.WaferAligner().WaferAlignInfo.HighMeasurementTable.HeightPoint
                        = HighStandardParam_Clone.HeightProfilingPointType.Value;
                //RetVal = ApplyHeightProfiling();
                HighStandard_IParam = HighStandardParam_Clone;
                retVal = this.SaveParameter(HighStandard_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void UpdateProcessingIndex()
        {
            try
            {
                High_ProcessingPointEnum processing_point = HighStandardParam_Clone.High_ProcessingPoint.Value;
                foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                {
                    if (ptinfo.PostJumpIndex.Count > 0)
                    {
                        foreach (var postjumpindex in ptinfo.PostJumpIndex)
                        {
                            if (postjumpindex.Index.XIndex == 0 && postjumpindex.Index.YIndex == 0)
                            {
                                postjumpindex.AcceptProcessing.Value = true;
                            }
                            else
                            {
                                if (processing_point == High_ProcessingPointEnum.HIGH_5PT)
                                {
                                    postjumpindex.AcceptProcessing.Value = false;
                                }
                                else
                                {
                                    postjumpindex.AcceptProcessing.Value = true;

                                }
                            }
                        }

                    }

                    if (ptinfo.JumpIndexs.Count > 0)
                    {
                        foreach (var jumpindex in ptinfo.JumpIndexs)
                        {
                            jumpindex.AcceptProcessing.Value = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void AllProcessingIndex()
        {
            try
            {
                if (HighStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                    {
                        if (ptinfo.JumpIndexs.Count > 0)
                        {
                            foreach (var jumpindex in ptinfo.JumpIndexs)
                            {
                                jumpindex.AcceptProcessing.Value = true;
                            }
                        }
                        if (ptinfo.PostJumpIndex.Count > 0)
                        {
                            foreach (var postjumpindex in ptinfo.PostJumpIndex)
                            {
                                postjumpindex.AcceptProcessing.Value = true;
                            }
                        }
                    }


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
        }
        private WAStandardPTInfomation UpdateHeightProfiling(WAStandardPTInfomation mptinfo)
        {
            WAStandardPTInfomation ptInfo = null;
            try
            {
                ptInfo = mptinfo;

                if (HighStandardParam_Clone.HeightProfilingPointType.Value == HeightPointEnum.POINT1)
                {
                    foreach (var postjumpindex in mptinfo.PostJumpIndex)
                    {
                        if (postjumpindex.Index.XIndex == 0 && postjumpindex.Index.YIndex == 0)
                        {
                            postjumpindex.AcceptFocusing.Value = true;
                        }
                        else
                        {
                            postjumpindex.AcceptFocusing.Value = false;
                        }
                    }

                    if (mptinfo.JumpIndexs != null)
                    {
                        foreach (var jumpindex in mptinfo.JumpIndexs)
                        {
                            jumpindex.AcceptFocusing.Value = false;
                        }
                    }
                }
                else
                {
                    //SortMaxJumpIndex(mptinfo);

                    foreach (var postjumpindex in mptinfo.PostJumpIndex)
                    {
                        if (postjumpindex.Index.XIndex == 0 && postjumpindex.Index.YIndex == 0)
                        {
                            postjumpindex.AcceptFocusing.Value = true;
                        }
                        else
                        {
                            postjumpindex.AcceptFocusing.Value = false;
                        }
                    }

                    if (mptinfo.JumpIndexs != null)
                    {
                        foreach (var jumpindex in mptinfo.JumpIndexs)
                        {
                            jumpindex.AcceptFocusing.Value = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ptInfo;
        }
        private WAStandardPTInfomation SortMaxJumpIndex(WAStandardPTInfomation mptinfo)
        {
            ObservableCollection<StandardJumpIndexParam> highjumpparam
                                   = new ObservableCollection<StandardJumpIndexParam>();
            List<StandardJumpIndexParam> tmpjumpparam = new List<StandardJumpIndexParam>();

            try
            {
                _HeightPositions.Clear();

                foreach (var info in mptinfo.PostJumpIndex)
                {
                    tmpjumpparam.Add(info);
                }

                foreach (var info in mptinfo.JumpIndexs)
                {
                    tmpjumpparam.Add(info);
                }

                //highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                //    (jumpindex => jumpindex.Index.XIndex.Value == 0
                //    && jumpindex.Index.YIndex.Value == 0));

                StandardJumpIndexParam jiparam = tmpjumpparam.Find
                    (jumpindex => jumpindex.Index.XIndex == 0
                    && jumpindex.Index.YIndex == 0);
                if (jiparam == null)
                {
                    jiparam = mptinfo.PostJumpIndex.ToList<StandardJumpIndexParam>().Find
                    (jumpindex => jumpindex.Index.XIndex == 0
                    && jumpindex.Index.YIndex == 0);
                }

                highjumpparam.Add(jiparam);

                _HeightPositions.Add(new WAHeightPositionParam(
                    mptinfo.GetX() + mptinfo.WaferCenter.GetX(),
                    mptinfo.GetY() + mptinfo.WaferCenter.GetY(),
                    HeightProfilignPosEnum.CENTER));

                tmpjumpparam.Sort(delegate (StandardJumpIndexParam A, StandardJumpIndexParam B)
                {
                    if (A.Index.XIndex > B.Index.XIndex)
                    {
                        return 1;
                    }
                    else if (A.Index.XIndex < B.Index.XIndex)
                    {
                        return -1;
                    }
                    return 0;
                });

                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                    (jumpindex => jumpindex.Index.XIndex
                    == tmpjumpparam[0].Index.XIndex
                    && jumpindex.Index.YIndex
                    == tmpjumpparam[0].Index.YIndex));
                //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.LEFT));
                _HeightPositions.Add(new WAHeightPositionParam(
                    ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                    ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                    HeightProfilignPosEnum.LEFT));


                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                    (jumpindex => jumpindex.Index.XIndex
                    == tmpjumpparam[tmpjumpparam.Count() - 1].Index.XIndex
                    && jumpindex.Index.YIndex
                    == tmpjumpparam[tmpjumpparam.Count() - 1].Index.YIndex));
                _HeightPositions.Add(new WAHeightPositionParam(
                   ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                   ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                   HeightProfilignPosEnum.RIGHT));

                //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.RIGHT));
                tmpjumpparam.Sort(delegate (StandardJumpIndexParam A, StandardJumpIndexParam B)
                {
                    if (A.Index.YIndex > B.Index.YIndex)
                    {
                        return 1;
                    }
                    else if (A.Index.YIndex < B.Index.YIndex)
                    {
                        return -1;
                    }
                    return 0;
                });

                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                    (jumpindex => jumpindex.Index.XIndex
                    == tmpjumpparam[0].Index.XIndex
                    && jumpindex.Index.YIndex
                    == tmpjumpparam[0].Index.YIndex));
                //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.UPPER));
                _HeightPositions.Add(new WAHeightPositionParam(
                   ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                   ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                   HeightProfilignPosEnum.UPPER));

                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                    (jumpindex => jumpindex.Index.XIndex
                    == tmpjumpparam[tmpjumpparam.Count() - 1].Index.XIndex
                    && jumpindex.Index.YIndex
                    == tmpjumpparam[tmpjumpparam.Count() - 1].Index.YIndex));
                //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.BOTTOM));
                _HeightPositions.Add(new WAHeightPositionParam(
                  ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                  ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                  HeightProfilignPosEnum.BOTTOM));

                foreach (var param in highjumpparam)
                {
                    if (param != null)
                    {
                        StandardJumpIndexParam jparam = mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().
                        Find(jindex => jindex.Index.XIndex
                         == param.Index.XIndex
                          && jindex.Index.YIndex
                           == param.Index.YIndex);
                        if (jparam != null)
                        {
                            //jparam.AcceptFocusing.Value = true;
                        }
                        else
                        {
                            jparam = mptinfo.PostJumpIndex.ToList<StandardJumpIndexParam>().
                            Find(jindex => jindex.Index.XIndex
                             == param.Index.XIndex
                              && jindex.Index.YIndex
                               == param.Index.YIndex);
                            if (jparam != null)
                            {
                                //jparam.AcceptFocusing.Value = true;
                            }
                        }
                    }
                    //mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().
                    //    Find(jindex => jindex.Index.XIndex.Value
                    //     == param.Index.XIndex.Value
                    //      && jindex.Index.YIndex.Value
                    //       == param.Index.YIndex.Value).AcceptFocusing.Value = true;



                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, err.Message);
                throw err;
            }

            return mptinfo;
        }

        private EventCodeEnum HeightProfilingEdge(WAStandardPTInfomation ptinfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                List<IDeviceObject> devices = this.StageSupervisor.WaferObject.GetDevices();
                IFocusing focusing = FocusingModule;



                //foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                //{
                //    if (ptinfo.PatternState.Value == PatternStateEnum.READY)
                //    {
                //        Point point = this.WaferAligner().GetLeftCornerPosition(ptinfo.GetX(), ptinfo.GetY());

                //        lcoffsetx = ptinfo.GetX() - point.X;
                //        lcoffsety = ptinfo.GetY() - point.Y;
                //        break;
                //    }

                //}

                //foreach (var ptinfo in this.WaferAligner().WaferAlignInfo.HighMeasurementTable.MeasurementInfos)
                //{
                //    if (ptinfo.PatternInfo.PatternState.Value == PatternStateEnum.READY)
                //    {
                //        Point point = this.WaferAligner().GetLeftCornerPosition(ptinfo.PatternInfo.GetX(), ptinfo.PatternInfo.GetY());

                //        lcoffsetx = ptinfo.PatternInfo.GetX() - point.X;
                //        lcoffsety = ptinfo.PatternInfo.GetY() - point.Y;
                //        break;
                //    }

                //}

                //MachineIndex ptindex = this.WaferAligner().WPosToMIndex(new WaferCoordinate(
                //    ptinfo.GetX()+Wafer.GetSubsInfo().WaferCenter.GetX(),
                //    ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY()));

                Point ptleftcorner = this.WaferAligner().GetLeftCornerPosition(
                    ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(),
                    ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY());

                double leftcornerptoffsetx = (ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX()) - ptleftcorner.X;
                double leftcornerptoffsety = (ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY()) - ptleftcorner.Y;

                long xMaxIndex = Wafer.GetSubsInfo().Devices.Max(xindex => xindex.DieIndexM.XIndex);
                long xMinIndex = Wafer.GetSubsInfo().Devices.Min(xindex => xindex.DieIndexM.XIndex);
                long yMaxIndex = Wafer.GetSubsInfo().Devices.Max(yindex => yindex.DieIndexM.YIndex);
                long yMinIndex = Wafer.GetSubsInfo().Devices.Min(yindex => yindex.DieIndexM.YIndex);

                long xIndex = 0;
                long yIndex = 0;


                //RightUpper
                xIndex = xMaxIndex;
                yIndex = yMaxIndex;

                while (true)
                {
                    if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                    {
                        WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety);

                        // focusing.Focusing_Retry
                        retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                        if (retVal == EventCodeEnum.NONE)
                        {

                            HighStandardParam_Clone.HeightPosParams.Add(new WAHeightPositionParam(
                                this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.RIGHTUPPER));
                            //if(this.WaferAligner().GetWAInnerStateEnum() != WaferAlignInnerStateEnum.SETUP)
                            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                this.WaferAligner().AddHeighPlanePoint();
                            _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                            break;
                        }
                        else
                        {
                            xIndex--;
                            yIndex--;
                        }
                    }
                    else
                    {
                        xIndex--;
                        yIndex--;
                    }

                    //_delays.DelayFor(1);
                }

                //LeftUpper
                xIndex = xMinIndex;
                yIndex = yMaxIndex;

                while (true)
                {
                    if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                    {
                        WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety);

                        // focusing.Focusing_Retry
                        retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, null);

                        if (retVal == EventCodeEnum.NONE)
                        {

                            HighStandardParam_Clone.HeightPosParams.Add(new WAHeightPositionParam(
                                this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.LEFTUPPER));
                            //if (this.WaferAligner().GetWAInnerStateEnum() != WaferAlignInnerStateEnum.SETUP)
                            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                this.WaferAligner().AddHeighPlanePoint();
                            _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                            break;
                        }
                        else
                        {
                            xIndex++;
                            yIndex--;
                        }
                    }
                    else
                    {
                        xIndex++;
                        yIndex--;
                    }

                    //_delays.DelayFor(1);
                }


                //LeftBottom

                xIndex = xMinIndex;
                yIndex = yMinIndex;

                while (true)
                {
                    if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                    {
                        WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety);

                        // focusing.Focusing_Retry
                        retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                        if (retVal == EventCodeEnum.NONE)
                        {

                            HighStandardParam_Clone.HeightPosParams.Add(new WAHeightPositionParam(
                                this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.LEFTBOTTOM));
                            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                this.WaferAligner().AddHeighPlanePoint();
                            _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                            break;
                        }
                        else
                        {
                            xIndex++;
                            yIndex++;
                        }
                    }
                    else
                    {
                        xIndex++;
                        yIndex++;
                    }

                    //_delays.DelayFor(1);
                }
                //RightBottom
                xIndex = xMaxIndex;
                yIndex = yMinIndex;

                while (true)
                {
                    if (devices.Find(mIndex => mIndex.DieIndexM.XIndex == xIndex && mIndex.DieIndexM.YIndex == yIndex).DieType.Value == DieTypeEnum.TEST_DIE)
                    {
                        WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)xIndex, (int)yIndex);

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX() + leftcornerptoffsetx, coordinate.GetY() + leftcornerptoffsety);

                        // focusing.Focusing_Retry
                        retVal = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                        if (retVal == EventCodeEnum.NONE)
                        {

                            HighStandardParam_Clone.HeightPosParams.Add(new WAHeightPositionParam(
                                this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert(), HeightProfilignPosEnum.RIGHTBOTTOM));
                            //if (this.WaferAligner().GetWAInnerStateEnum() != WaferAlignInnerStateEnum.SETUP)
                            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                this.WaferAligner().AddHeighPlanePoint();
                            _HeightPositions.Add(HighStandardParam_Clone.HeightPosParams[HighStandardParam_Clone.HeightPosParams.Count() - 1]);
                            break;
                        }
                        else
                        {
                            xIndex--;
                            yIndex++;
                        }
                    }
                    else
                    {
                        xIndex--;
                        yIndex++;
                    }

                    //_delays.DelayFor(1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"");
                throw err;
            }

            return retVal;
        }

        private EventCodeEnum SetHeightProfilingParams(WAStandardPTInfomation mptinfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ObservableCollection<StandardJumpIndexParam> highjumpparam
                       = new ObservableCollection<StandardJumpIndexParam>();
                List<StandardJumpIndexParam> tmpjumpparam = new List<StandardJumpIndexParam>();

                try
                {
                    _HeightPositions.Clear();

                    foreach (var info in mptinfo.PostJumpIndex)
                    {
                        tmpjumpparam.Add(info);
                    }

                    foreach (var info in mptinfo.JumpIndexs)
                    {
                        tmpjumpparam.Add(info);
                    }

                    StandardJumpIndexParam jiparam = tmpjumpparam.Find
                        (jumpindex => jumpindex.Index.XIndex == 0
                        && jumpindex.Index.YIndex == 0);
                    if (jiparam == null)
                    {
                        jiparam = mptinfo.PostJumpIndex.ToList<StandardJumpIndexParam>().Find
                        (jumpindex => jumpindex.Index.XIndex == 0
                        && jumpindex.Index.YIndex == 0);
                    }

                    highjumpparam.Add(jiparam);

                    _HeightPositions.Add(new WAHeightPositionParam(
                        mptinfo.GetX() + mptinfo.WaferCenter.GetX(),
                        mptinfo.GetY() + mptinfo.WaferCenter.GetY(),
                        HeightProfilignPosEnum.CENTER));

                    tmpjumpparam.Sort(delegate (StandardJumpIndexParam A, StandardJumpIndexParam B)
                    {
                        if (A.Index.XIndex > B.Index.XIndex)
                        {
                            return 1;
                        }
                        else if (A.Index.XIndex < B.Index.XIndex)
                        {
                            return -1;
                        }
                        return 0;
                    });

                    highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                        (jumpindex => jumpindex.Index.XIndex
                        == tmpjumpparam[0].Index.XIndex
                        && jumpindex.Index.YIndex
                        == tmpjumpparam[0].Index.YIndex));
                    //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.LEFT));
                    _HeightPositions.Add(new WAHeightPositionParam(
                        ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                        ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                        HeightProfilignPosEnum.LEFT));


                    highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                        (jumpindex => jumpindex.Index.XIndex
                        == tmpjumpparam[tmpjumpparam.Count() - 1].Index.XIndex
                        && jumpindex.Index.YIndex
                        == tmpjumpparam[tmpjumpparam.Count() - 1].Index.YIndex));
                    _HeightPositions.Add(new WAHeightPositionParam(
                       ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                       ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                       HeightProfilignPosEnum.RIGHT));

                    //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.RIGHT));
                    tmpjumpparam.Sort(delegate (StandardJumpIndexParam A, StandardJumpIndexParam B)
                    {
                        if (A.Index.YIndex > B.Index.YIndex)
                        {
                            return 1;
                        }
                        else if (A.Index.YIndex < B.Index.YIndex)
                        {
                            return -1;
                        }
                        return 0;
                    });

                    highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                        (jumpindex => jumpindex.Index.XIndex
                        == tmpjumpparam[0].Index.XIndex
                        && jumpindex.Index.YIndex
                        == tmpjumpparam[0].Index.YIndex));
                    //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.UPPER));
                    _HeightPositions.Add(new WAHeightPositionParam(
                       ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                       ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                       HeightProfilignPosEnum.UPPER));

                    highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find
                        (jumpindex => jumpindex.Index.XIndex
                        == tmpjumpparam[tmpjumpparam.Count() - 1].Index.XIndex
                        && jumpindex.Index.YIndex
                        == tmpjumpparam[tmpjumpparam.Count() - 1].Index.YIndex));
                    //_HeightPositions.Add(new WAHeightPositionParam(mptinfo.GetX(), mptinfo.GetY(), HeightProfilignPosEnum.BOTTOM));
                    _HeightPositions.Add(new WAHeightPositionParam(
                      ((mptinfo.GetX() + mptinfo.WaferCenter.GetX()) + (highjumpparam[highjumpparam.Count - 1].Index.XIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value)),
                      ((mptinfo.GetY() + mptinfo.WaferCenter.GetY()) + (highjumpparam[highjumpparam.Count - 1].Index.YIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value)),
                      HeightProfilignPosEnum.BOTTOM));

                    foreach (var param in highjumpparam)
                    {
                        if (param != null)
                        {
                            StandardJumpIndexParam jparam = mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().
                            Find(jindex => jindex.Index.XIndex
                             == param.Index.XIndex
                              && jindex.Index.YIndex
                               == param.Index.YIndex);
                            if (jparam != null)
                            {
                                jparam.AcceptFocusing.Value = true;
                            }
                            else
                            {
                                jparam = mptinfo.PostJumpIndex.ToList<StandardJumpIndexParam>().
                                Find(jindex => jindex.Index.XIndex
                                 == param.Index.XIndex
                                  && jindex.Index.YIndex
                                   == param.Index.YIndex);
                                if (jparam != null)
                                {
                                    jparam.AcceptFocusing.Value = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err, err.Message);
                    throw err;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region //..VerifyAlign

        public EventCodeEnum VerifyAlignProcessing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Verify(ref VerifyInofs);
                //if (VerifyInofs.Count != 0)
                if (retVal != EventCodeEnum.NONE)
                {
                    //retVal = DoRetryVerify();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.VERIFYALIGN_OVERFLOW_LIMIT;
                        this.NotifyManager().Notify(retVal);
                    }
                }
                else
                {
                    retVal = VerifyWaferCenter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        //private EventCodeEnum Verify(ref List<VerifyInfo> verifyinfo )
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //     verifyinfo = new List<VerifyInfo>();
        //    try
        //    {
        //        double forecastXpos = 0.0;
        //        double forecastYpos = 0.0;

        //        double deviationX = 0.0;
        //        double deviationY = 0.0;


        //        //.. forecastPos : 패턴의 위치와 JumpIndex로 예측한 패턴위치 결과
        //        //.. retcoord : 실제 High의 결과위치와 보정된 각도로 예상한 패턴위치 결과
        //        //.. deviation : retcoord - forecastPos 값.

        //        ProcResults = ObjectExtensions.DeepClone(this.WaferAligner().WaferAlignInfo.AlignProcResult);
        //        double Angle = this.WaferAligner().WaferAlignInfo.VerifyAngle;

        //        List<WaferProcResult> results = new List<WaferProcResult>();
        //        if (ProcResults.Count != 0)
        //        {
        //            //..  X 정렬
        //            ProcResults.Sort(delegate (WaferProcResult A, WaferProcResult B)
        //            {

        //                if (A.ResultPos.GetX() > B.ResultPos.GetX())
        //                {
        //                    return 1;
        //                }
        //                else
        //                {
        //                    return -1;
        //                }

        //            }
        //            );

        //            results.Add(ProcResults[0]);
        //            results.Add(ProcResults[ProcResults.Count() - 1]);

        //            //.. Y 정렬

        //            ProcResults.Sort(delegate (WaferProcResult A, WaferProcResult B)
        //            {

        //                if (A.ResultPos.GetY() > B.ResultPos.GetY())
        //                {
        //                    return 1;
        //                }
        //                else
        //                {
        //                    return -1;
        //                };

        //            }
        //            );

        //            results.Add(ProcResults[0]);
        //            results.Add(ProcResults[ProcResults.Count() - 1]);


        //            ProcResults.Clear();
        //            ProcResults = results;

        //            List<VerifyInfo> infos = new List<VerifyInfo>();

        //            WAStandardPTInfomation ptinfo = null;

        //            foreach (var result in ProcResults)
        //            {
        //                ptinfo = (WAStandardPTInfomation)result.PatternInfo;

        //                forecastXpos = ptinfo.GetX() + Wafer.Info.WaferCenter.GetX() +
        //                        (result.Index.XIndex.Value * result.AlignIndexSize.Width.Value);
        //                forecastYpos = ptinfo.GetY() + Wafer.Info.WaferCenter.GetY() +
        //                   (result.Index.YIndex.Value * result.AlignIndexSize.Height.Value);

        //                MachineCoordinate pivotPoint = new MachineCoordinate(ptinfo.GetX() + Wafer.Info.WaferCenter.GetX()
        //                    , ptinfo.GetY() + Wafer.Info.WaferCenter.GetY());

        //                MachineCoordinate Point = new MachineCoordinate(result.ResultPos.GetX(),
        //                    result.ResultPos.GetY());

        //                MachineCoordinate retcoord = this.CoordinateManager().GetRotatedPoint(Point, pivotPoint, Angle);


        //                deviationX = Math.Abs(retcoord.GetX() - forecastXpos);
        //                deviationY = Math.Abs(retcoord.GetY() - forecastYpos);

        //                if (deviationX >= HighStandardParam.LimitValueX.Value || deviationY >= HighStandardParam.LimitValueX.Value)
        //                {
        //                    //.. The value of devationX or devationY is higher than MaxValue.
        //                    infos.Add(new VerifyInfo(ptinfo, result.ResultPos, new WaferCoordinate(forecastXpos, forecastYpos)
        //                        , result.Index, new Values(deviationX, deviationY)));
        //                }
        //            }

        //            if (infos.Count == 0)
        //                retVal = EventCodeEnum.NONE;
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw err;
        //    }
        //    return retVal;
        //}

        /// <summary>
        /// vb
        /// </summary>
        /// <param name="verifyinfo"></param>
        /// <returns></returns>
        private EventCodeEnum Verify(ref List<VerifyInfo> verifyinfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            verifyinfo = new List<VerifyInfo>();
            try
            {
                double forecastXpos = 0.0;
                double forecastYpos = 0.0;

                double deviationX = 0.0;
                double deviationY = 0.0;


                //.. forecastPos : 패턴의 위치와 JumpIndex로 예측한 패턴위치 결과
                //.. retcoord : 실제 High의 결과위치와 보정된 각도로 예상한 패턴위치 결과
                //.. deviation : retcoord - forecastPos 값.

                //ProcResults = ObjectExtensions.DeepClone(this.WaferAligner().WaferAlignInfo.AlignProcResult);
                ProcResults = this.WaferAligner().WaferAlignInfo.AlignProcResult;

                //foreach (var result in this.WaferAligner().WaferAlignInfo.AlignProcResult)
                //{
                //    if (ProcResults == null)
                //        ProcResults = new List<WaferProcResult>();
                //    ProcResults.Add(result);
                //}
                WaferProcResult centerresult = ProcResults.Where(result => result.Index.XIndex == 0 && result.Index.YIndex == 0).FirstOrDefault();

                double Angle = this.WaferAligner().WaferAlignInfo.VerifyAngle;

                List<WaferProcResult> results = new List<WaferProcResult>();
                if (ProcResults.Count != 0)
                {
                    //..  X 정렬
                    ProcResults.Sort(delegate (WaferProcResult A, WaferProcResult B)
                    {

                        if (A.ResultPos.GetX() > B.ResultPos.GetX())
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }

                    }
                    );

                    results.Add(ProcResults[0]);
                    results.Add(ProcResults[ProcResults.Count() - 1]);

                    //.. Y 정렬

                    ProcResults.Sort(delegate (WaferProcResult A, WaferProcResult B)
                    {

                        if (A.ResultPos.GetY() > B.ResultPos.GetY())
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        };

                    }
                    );

                    results.Add(ProcResults[0]);
                    results.Add(ProcResults[ProcResults.Count() - 1]);


                    ProcResults.Clear();
                    ProcResults = results;

                    VerifyInofs.Clear();

                    WAStandardPTInfomation ptinfo = null;


                    List<WaferProcResult> verresult = ProcResults.FindAll(result => result.Index.XIndex == 0);
                    List<WaferProcResult> horresult = ProcResults.FindAll(result => result.Index.YIndex == 0);

                    //ProcResults를 아래서에서 GetRotate하기 전에 Pattern Matching된 결과로 Verify - Angle 동작
                    retVal = CompareLeftRightAnglesToCenter(centerresult, horresult);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        return retVal;
                    }

                    double curtheta = 0.0;
                    this.MotionManager().GetRefPos(EnumAxisConstants.C, ref curtheta);
                    foreach (var result in horresult)
                    {
                        double x1 = 0.0;
                        double x2 = 0.0;
                        double y1 = 0.0;
                        double y2 = 0.0;
                        double angle = 0;

                        x1 = result.ResultPos.GetX();
                        y1 = result.ResultPos.GetY();
                        angle = (curtheta - result.ResultPos.GetT()) / 10000.0;
                        GetRotCoord(ref x2, ref y2, x1, y1, angle);
                        //result.ResultPos.X.Value = x1;
                        result.ResultPos.X.Value = x2;
                        result.ResultPos.Y.Value = y2;
                    }

                    foreach (var result in ProcResults)
                    {
                        ptinfo = (WAStandardPTInfomation)result.PatternInfo;
                        result.VerifyPos = new WaferCoordinate();

                        result.VerifyPos = result.ResultPos;
                        //result.VerifyPos.X.Value = result.ResultPos.GetX() - (ptinfo.GetX() + Wafer.Info.WaferCenter.GetX() +
                        //        (result.Index.XIndex.Value * result.AlignIndexSize.Width.Value));
                        //result.VerifyPos.Y.Value = result.ResultPos.GetY() - (ptinfo.GetY() + Wafer.Info.WaferCenter.GetY() +
                        //   (result.Index.YIndex.Value * result.AlignIndexSize.Height.Value));

                        //result.VerifyPos.X.Value = result.ResultPos.GetX() - (ptinfo.GetX() + ptinfo.ProcWaferCenter.GetX() +
                        //     (result.Index.XIndex.Value * result.AlignIndexSize.Width.Value));
                        //result.VerifyPos.Y.Value = result.ResultPos.GetY() - (ptinfo.GetY() + ptinfo.WaferCenter.GetY() +
                        //   (result.Index.YIndex.Value * result.AlignIndexSize.Height.Value));

                    }



                    foreach (var result in verresult)
                    {
                        //result.VerifyPos.X.Value = result.ResultPos.GetX() + Wafer.GetSubsInfo().WaferSquarness.Value * result.Index.YIndex;
                        if (result.Index.YIndex < 0)
                        {
                            result.VerifyPos.X.Value = result.ResultPos.GetX() + Wafer.GetSubsInfo().WaferSequareness.Value * Math.Abs(result.Index.YIndex);
                        }
                        else
                        {
                            result.VerifyPos.X.Value = result.ResultPos.GetX() - Wafer.GetSubsInfo().WaferSequareness.Value * Math.Abs(result.Index.YIndex);
                        }
                        //result.ResultPos.X.Value = result.ResultPos.GetX() - Wafer.GetSubsInfo().WaferSquarness / 10000 + Math.Abs(result.Index.YIndex.Value);
                    }

                    LoggerManager.Debug($"==VerifyAlign Info==", isInfo: true);
                    double maxX = 0.0;
                    double maxY = 0.0;

                    LoggerManager.Debug($"LimitValueX = {HighStandardParam_Clone.LimitValueX.Value}, LimitValueY = {HighStandardParam_Clone.LimitValueY.Value}", isInfo: true);

                    for (int i = 0; i < ProcResults.Count; i++)
                    {
                        for (int j = 0; j < ProcResults.Count; j++)
                        {
                            deviationX = Math.Abs(ProcResults[i].VerifyPos.GetX() - ProcResults[j].VerifyPos.GetX()) -
                                 (Wafer.GetSubsInfo().ActualDieSize.Width.Value * Math.Abs(ProcResults[i].Index.XIndex - ProcResults[j].Index.XIndex));
                            deviationY = Math.Abs(ProcResults[i].VerifyPos.GetY() - ProcResults[j].VerifyPos.GetY()) -
                                 (Wafer.GetSubsInfo().ActualDieSize.Height.Value * Math.Abs(ProcResults[i].Index.YIndex - ProcResults[j].Index.YIndex));

                            if (deviationX > maxX)
                                maxX = deviationX;
                            if (deviationY > maxY)
                                maxY = deviationY;

                            if (deviationX >= HighStandardParam_Clone.LimitValueX.Value || deviationY >= HighStandardParam_Clone.LimitValueY.Value)
                            {
                                //.. The value of devationX or devationY is higher than MaxValue.
                                VerifyInofs.Add(new VerifyInfo(ptinfo, ProcResults[i].ResultPos, new WaferCoordinate(forecastXpos, forecastYpos)
                                    , ProcResults[i].Index, new Values(deviationX, deviationY)));
                            }

                            LoggerManager.Debug($"i : {i}, j : {j} = [deviationX] : {deviationX}, [deviationY] : {deviationY}", isInfo: true);
                        }
                    }
                    LoggerManager.Debug($"Verify Align Result", isInfo: true);
                    LoggerManager.Debug($"[Max Result X Value] : {maxX}", isInfo: true);
                    LoggerManager.Debug($"[Max Result Y Value] : {maxY}", isInfo: true);

                    //foreach (var result in ProcResults)
                    //{
                    //    ptinfo = (WAStandardPTInfomation)result.PatternInfo;

                    //    forecastXpos = ptinfo.GetX() + Wafer.Info.WaferCenter.GetX() +
                    //            (result.Index.XIndex.Value * result.AlignIndexSize.Width.Value);
                    //    forecastYpos = ptinfo.GetY() + Wafer.Info.WaferCenter.GetY() +
                    //       (result.Index.YIndex.Value * result.AlignIndexSize.Height.Value);

                    //    MachineCoordinate pivotPoint = new MachineCoordinate(ptinfo.GetX() + Wafer.Info.WaferCenter.GetX()
                    //        , ptinfo.GetY() + Wafer.Info.WaferCenter.GetY());

                    //    MachineCoordinate Point = new MachineCoordinate(result.ResultPos.GetX(),
                    //        result.ResultPos.GetY());

                    //   MachineCoordinate retcoord = this.CoordinateManager().GetRotatedPoint(Point, pivotPoint, Angle);


                    //    deviationX = Math.Abs(retcoord.GetX() - forecastXpos);
                    //    deviationY = Math.Abs(retcoord.GetY() - forecastYpos);

                    //    if (deviationX >= HighStandardParam.LimitValueX.Value || deviationY >= HighStandardParam.LimitValueX.Value)
                    //    {
                    //        //.. The value of devationX or devationY is higher than MaxValue.
                    //        infos.Add(new VerifyInfo(ptinfo, result.ResultPos, new WaferCoordinate(forecastXpos, forecastYpos)
                    //            , result.Index, new Values(deviationX, deviationY)));
                    //    }
                    //}

                    if (VerifyInofs.Count == 0)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        private EventCodeEnum CompareLeftRightAnglesToCenter(WaferProcResult centerresult, List<WaferProcResult> horresult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"CompareLeftRightAnglesToCenter() : Start Verify Angles");

                WaferProcResult leftPointResult = horresult.Where(result => result.Index.XIndex < 0).FirstOrDefault();
                WaferProcResult rightPointResult = horresult.Where(result => result.Index.XIndex > 0).FirstOrDefault();

                //LEFT TO CENTER
                double leftangle = this.CoordinateManager().CalcP2PAngle(centerresult.ResultPos.GetX(), centerresult.ResultPos.GetY(), leftPointResult.ResultPos.GetX(), leftPointResult.ResultPos.GetY());

                double lefttocenterangle = NormalizeAngleByQuadrant(leftangle);
                if (lefttocenterangle == -999)
                {
                    return EventCodeEnum.EXCEPTION;
                }

                leftangle = Math.Round(leftangle, 6);
                lefttocenterangle = Math.Round(lefttocenterangle, 6);

                LoggerManager.Debug($"[Verify Angle - LEFT TO CENTER] center xpos = {centerresult.ResultPos.GetX():0.00}, center ypos = {centerresult.ResultPos.GetY():0.00}, left xpos = {leftPointResult.ResultPos.GetX():0.00}, left ypos = {leftPointResult.ResultPos.GetY():0.00}" +
                    $" angle = {leftangle}, normalizeangle = {lefttocenterangle}");

                //CENTER TO RIGHT
                double rightangle = this.CoordinateManager().CalcP2PAngle(centerresult.ResultPos.GetX(), centerresult.ResultPos.GetY(), rightPointResult.ResultPos.GetX(), rightPointResult.ResultPos.GetY());
                double righttocenterangle = NormalizeAngleByQuadrant(rightangle);
                if (righttocenterangle == -999)
                {
                    return EventCodeEnum.EXCEPTION;
                }

                rightangle = Math.Round(rightangle, 6);
                righttocenterangle = Math.Round(righttocenterangle, 6);

                LoggerManager.Debug($"[Verify Angle - CENTER TO RIGHT] center xpos = {centerresult.ResultPos.GetX():0.00}, center ypos = {centerresult.ResultPos.GetY():0.00}, right xpos = {rightPointResult.ResultPos.GetX():0.00}, right ypos = {rightPointResult.ResultPos.GetY():0.00}" +
                    $" angle = {rightangle}, normalizeangle = {righttocenterangle}");

                //COMPARE
                double anglediff = Math.Abs(lefttocenterangle + righttocenterangle);

                double anglelimit = HighStandardParam_Clone.VerifyLimitAngle.Value;
                if (anglelimit > HighStandardParam_Clone.VerifyLimitAngle.UpperLimit || anglelimit < HighStandardParam_Clone.VerifyLimitAngle.LowerLimit)
                {
                    LoggerManager.Debug($"[Verify Angle - Parameter Error] VerifyLimitAngle = {anglelimit}");
                    //sin(θ) * 150 um (최외곽 거리) = +-3um(오차 허용 값)
                    double value = 0.003 / 150.0; // 3µm / 150mm = 0.00002
                    anglelimit = Math.Asin(value) * (180.0 / Math.PI);
                    //0.0011459
                    HighStandardParam_Clone.VerifyLimitAngle.Value = Math.Round(anglelimit, 6);
                }

                if (anglediff >= HighStandardParam_Clone.VerifyLimitAngle.Value)
                {
                    retVal = EventCodeEnum.WAFER_ALIGN_HORIZONTAL_ANGLE_VERIFY_FAIL;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

                LoggerManager.Debug($"CompareLeftRightAnglesToCenter() : [Verify Angle - Result] Angle Diff = |rightangle + leftangle| = {Math.Round(anglediff, 6)}, VerifyAngleLimit = {Math.Round(anglelimit, 6)}, result = {retVal}");

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private double NormalizeAngleByQuadrant(double angle)
        {
            double normalizeangle = 0.0;

            try
            {
                // angle은 0 도 기준으로 환산하기 위함.
                //angle 0 ~ 90, 0 ~ -90 까지는 normalizeangle = angle
                //-91 ~ -180 , 3분면
                if (angle < -90 && angle >= -180)
                {
                    normalizeangle = (180 + (angle)) * -1;
                }
                //90 ~ 180, 2분면
                else if (angle > 90 && angle <= 180)
                {
                    normalizeangle = 180 + (-angle);
                }
                //1,4분면
                else
                {
                    normalizeangle = angle;
                }

                if (angle > 180 || angle < -180)
                {
                    LoggerManager.Debug($"NormalizeAngleByQuadrant(), Angle = {angle}");
                    normalizeangle = -999;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                normalizeangle = -999;
            }
            return normalizeangle;
        }

        private EventCodeEnum RecoverySteup_VerifyWaferCenter(WAStandardPTInfomation ptinfo = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ptinfo == null)
                {
                    if (this.WaferAligner().WaferAlignInfo.RecoveryParam != null)
                    {
                        //Reocvery Setup Registrated Pattern
                        if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count > 0)
                        {
                            foreach (var info in this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer)
                            {
                                if (info.PatternState.Value == PatternStateEnum.READY)
                                {
                                    ptinfo = info;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug("Wafer alignment temporary high mag pattern data count is zero.");
                            return retVal;
                        }
                    }
                    else
                    {
                        LoggerManager.Debug("Wafer alignment high mag pattern data is null.");
                        return retVal;
                    }
                }

                retVal = VerifyRevisionWaferCenter(ptinfo);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private WAStandardPTInfomation GetFirstReadyPattern()
        {
            WAStandardPTInfomation retval = null;

            try
            {
                if(HighStandardParam_Clone.Patterns.Value != null)
                {
                    List<WAStandardPTInfomation> ptinfos = HighStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>();

                    if (ptinfos != null)
                    {
                        if (ptinfos.Count != 0)
                        {
                            foreach (var info in ptinfos)
                            {
                                if (info.PatternState.Value == PatternStateEnum.READY)
                                {
                                    retval = info;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug("Wafer alignment high mag pattern data count is zero.");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug("Wafer alignment high mag pattern data is null.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum VerifyWaferCenter(WAStandardPTInfomation ptinfo = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ptinfo == null)
                {
                    ptinfo = GetFirstReadyPattern();

                    if(ptinfo == null)
                    {
                        return retVal;
                    }
                }

                retVal = VerifyRevisionWaferCenter(ptinfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private void GetRotCoord(ref double x2, ref double y2, double x1, double y1, double angle)
        {
            //double theta = (double)(((angle) / 180) * Math.PI); // rad = arg * COEF_TO_RAD '--pi/180#
            double theta = (double)angle * (Math.PI / 180); // rad = arg * COEF_TO_RAD '--pi/180#
            double newx = x1;
            double newy = y1;
            x2 = newx * Math.Cos(theta) - newy * Math.Sin(theta);
            y2 = newx * Math.Sin(theta) + newy * Math.Cos(theta);
        }

        /*
        public EventCodeEnum DoRetryVerify()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                while (true)
                {
                    for (int index = 0; index < VerifyInofs.Count; index++)
                    {
                        retVal = VerifyStepState.Execute(VerifyInofs[index]);
                    }
                    if (VerifyStepState.GetState() == WAVerifyStepEnum.DONE || VerifyStepState.GetState() == WAVerifyStepEnum.FAIL)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(1);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        */


        public EventCodeEnum CorrectVibration(VerifyInfo info, ref WaferProcResult procresult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (info.PtInfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                {

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        info.Targetpos.GetX(), info.Targetpos.GetY(), info.Targetpos.GetZ(), 0.8);
                }

                PMResult result = this.VisionManager().PatternMatching(info.PtInfo, this);

                if (result.RetValue != EventCodeEnum.NONE)
                {
                    return result.RetValue;
                }

                WaferCoordinate wcd = (WaferCoordinate)this.CoordinateManager().PmResultConverToUserCoord(result);

                procresult.ErrorCodeType = result.RetValue;
                procresult.ResultPos = wcd;

                retVal = ReInspect(procresult, info.Targetpos, HighStandardParam_Clone.LimitValueX.Value);
            }
            catch (Exception err)
            {
                procresult = null;
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum CorrectFocusing(VerifyInfo info, ref WaferProcResult procresult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        info.Targetpos.GetX(), info.Targetpos.GetY(), info.Targetpos.GetZ());

                FocusingModule.Focusing_Retry(FocusingParam, false, false, true, this);

                PMResult result = this.VisionManager().PatternMatching(info.PtInfo, this);

                if (result.RetValue != EventCodeEnum.NONE)
                {
                    return result.RetValue;
                }

                WaferCoordinate wcd = (WaferCoordinate)this.CoordinateManager().PmResultConverToUserCoord(result);

                procresult.ErrorCodeType = result.RetValue;
                procresult.ResultPos = wcd;

                retVal = ReInspect(procresult, info.Targetpos, HighStandardParam_Clone.LimitValueX.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum CorrectLight(VerifyInfo info, ref WaferProcResult procresult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        info.Targetpos.GetX(), info.Targetpos.GetY(), info.Targetpos.GetZ());


                PMResult result = this.VisionManager().PatternMatchingRetry(info.PtInfo);

                if (result.RetValue != EventCodeEnum.NONE)
                {
                    return result.RetValue;
                }

                WaferCoordinate wcd = (WaferCoordinate)this.CoordinateManager().PmResultConverToUserCoord(result);

                procresult.ErrorCodeType = result.RetValue;
                procresult.ResultPos = wcd;

                retVal = ReInspect(procresult, info.Targetpos, HighStandardParam_Clone.LimitValueX.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum ReInspect(WaferProcResult procResult, WaferCoordinate forecastcoord, double limit)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double deviationX = 0.0;
                double deviationY = 0.0;

                deviationX = procResult.ResultPos.GetX() - forecastcoord.GetX();
                deviationY = procResult.ResultPos.GetY() - forecastcoord.GetY();

                if (deviationX < limit && deviationY < limit)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.VERIFYALIGN_OVERFLOW_LIMIT;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        #endregion



        public bool IsLotReady(out string msg)
        {
            bool retVal = false;
            try
            {
                msg = null;
                try
                {
                    if (ParamValidation() == EventCodeEnum.NONE)
                        retVal = true;
                    else
                    {
                        if ((HighStandard_IParam as WA_HighMagParam_Standard).Patterns.Value.Count == 0)
                            msg = "Not Exist Pattern(Wafer High Align).";
                        else if ((HighStandard_IParam as WA_HighMagParam_Standard).Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                            item => item.PatternState.Value == PatternStateEnum.FAILED).Count
                            != (HighStandard_IParam as WA_HighMagParam_Standard).Patterns.Value.Count)
                            msg = "Not Exist Pattern that can be Align(WaferHigh  Align).";
                        retVal = false;

                        if (msg != null) LoggerManager.Debug(msg);
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if ((HighStandard_IParam as WA_HighMagParam_Standard) != null)
                {
                    WA_HighMagParam_Standard param = (HighStandard_IParam as WA_HighMagParam_Standard);
                    if (param.Patterns != null)
                    {
                        if (param.Patterns.Value.Count != 0)
                        {
                            foreach (var pattern in param.Patterns.Value)
                            {
                                pattern.PatternState.Value = PatternStateEnum.READY;
                            }
                        }
                    }
                }
                retVal = SubModuleState.ClearData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public override EventCodeEnum ClearSettingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //HighStandardParam_Clone = HighStandard_IParam.Copy() as WA_HighMagParam_Standard;
                HighStandardParam_Clone = HighStandard_IParam as WA_HighMagParam_Standard;
                ParamValidation();

                if (this.WaferAligner().IsNewSetup)
                {
                    string targetDirectory = this.FileManager().GetDeviceParamFullPath() + HighStandardParam_Clone.PatternbasePath;
                    this.FileManager().DeleteFilesInDirectory(targetDirectory);

                    HighStandardParam_Clone.Patterns.Value.Clear();

                    //Clear Manual JumpIndex Setting.
                    HighStandardParam_Clone.JumpIndexManualInputParam.Left1Index = 0;
                    HighStandardParam_Clone.JumpIndexManualInputParam.Left2Index = 0;
                    HighStandardParam_Clone.JumpIndexManualInputParam.Right1Index = 0;
                    HighStandardParam_Clone.JumpIndexManualInputParam.Right2Index = 0;
                    HighStandardParam_Clone.JumpIndexManualInputParam.UpperIndex = 0;
                    HighStandardParam_Clone.JumpIndexManualInputParam.BottomIndex = 0;

                    //Clear Pattern Image
                }
                //(AdvanceSetupView as HighMagStandardInputControl).SettingData(HighStandardParam_Clone);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    if ((HighStandard_IParam as WA_HighMagParam_Standard).Patterns.Value.Count != 0 &&
                        (HighStandard_IParam as WA_HighMagParam_Standard).Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(item => item.PatternState.Value == PatternStateEnum.FAILED).Count == 0)
                    {
                        retVal = Extensions_IParam.ElementStateNeedSetupValidation(HighStandard_IParam);
                    }
                }
                else
                {
                    if ((HighStandard_IParam as WA_HighMagParam_Standard).Patterns.Value.Count != 0)
                    {
                        retVal = Extensions_IParam.ElementStateNeedSetupValidation(HighStandard_IParam);
                    }
                }

                if (Parent != null)
                {
                    Parent.ParamValidation();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }


        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var pt in (HighStandard_IParam as WA_HighMagParam_Standard).Patterns.Value)
                {
                    pt.PatternState.Value = PatternStateEnum.READY;
                }
                SubModuleState = new SubModuleIdleState(this);
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - PreRun() : Error occured.");
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.GetState() == SubModuleStateEnum.IDLE | this.GetState() == SubModuleStateEnum.SKIP)
                    SubModuleState = new SubModuleRecoveryState(this);
                else
                {
                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        this.PnPManager().GetPnpSteps(this.WaferAligner());
                        this.ViewModelManager().ViewTransitionType(this.PnPManager());
                    }
                }
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;
        }

        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum ExitRecovery()
        {
            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var ptinfo in HighStandardParam_Clone.Patterns.Value)
                {
                    if (ptinfo.PatternState.Value == PatternStateEnum.MODIFY)
                    {
                        //ptinfo.X.Value += Wafer.GetSubsInfo().WaferCenterOffset.GetX();
                        //ptinfo.Y.Value += Wafer.GetSubsInfo().WaferCenterOffset.GetY();
                        // ptinfo.X.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX;
                        //ptinfo.Y.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY;
                        ptinfo.PatternState.Value = PatternStateEnum.READY;
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                EventCodeEnum ret = Extensions_IParam.ElementStateDefaultValidation(HighStandard_IParam);
                if (ret == EventCodeEnum.NONE)
                    retVal = false;
                else
                    retVal = true;

                retVal = IsParamChanged || retVal;

                if (Parent != null)
                    Parent.IsParameterChanged();
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
                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    if (this.WaferAligner().IsNewSetup)
                    {
                        if (HighStandardParam_Clone.Patterns.Value.Count != 0
                        && HighStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                            item => item.PatternState.Value == PatternStateEnum.FAILED).Count == 0)
                            SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                        else
                            SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    }
                    else
                    {
                        if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Count > 0)
                        {
                            SetNodeSetupRecoveryState(EnumMoudleSetupState.VERIFY);
                        }
                        else
                        {
                            if (this.GetState() == SubModuleStateEnum.DONE)
                                SetNodeSetupRecoveryState(EnumMoudleSetupState.COMPLETE);
                            else
                                SetNodeSetupRecoveryState(EnumMoudleSetupState.NOTCOMPLETED);
                        }
                    }
                }
                else
                {
                    //WaferAligner Recovery
                    if (this.WaferAligner().ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                    {
                        if (this.GetState() == SubModuleStateEnum.IDLE)
                            SetNodeSetupRecoveryState(EnumMoudleSetupState.NOTCOMPLETED);
                        else if (this.GetState() == SubModuleStateEnum.DONE)
                            SetNodeSetupRecoveryState(EnumMoudleSetupState.COMPLETE);
                        else
                        {
                            if (this.GetState() != SubModuleStateEnum.DONE | this.GetState() == SubModuleStateEnum.IDLE)
                            {
                                if (HighStandardParam_Clone.Patterns.Value.Count != 0
                                && HighStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                                    item => item.PatternState.Value == PatternStateEnum.FAILED).Count != HighStandardParam_Clone.Patterns.Value.Count)
                                    SetNodeSetupRecoveryState(EnumMoudleSetupState.COMPLETE);
                                else
                                    SetNodeSetupRecoveryState(EnumMoudleSetupState.NOTCOMPLETED);
                            }

                        }
                    }
                    else
                    {
                        if (HighStandardParam_Clone.Patterns.Value.Count != 0
                             && HighStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                                 item => item.PatternState.Value == PatternStateEnum.FAILED).Count != HighStandardParam_Clone.Patterns.Value.Count)
                            SetNodeSetupRecoveryState(EnumMoudleSetupState.COMPLETE);
                        else
                            SetNodeSetupRecoveryState(EnumMoudleSetupState.NOTCOMPLETED);

                    }
                }

                if (Parent != null)
                    Parent.SetStepSetupState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetStepSetupCompleteState()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum SetStepSetupNotCompleteState()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (GetModuleSetupState() == EnumMoudleSetupState.VERIFY)
                {
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public void SetRecoveryStepState(EnumMoudleSetupState state, bool isparent = false)
        {
            try
            {
                SetNodeSetupRecoveryState(state);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }



    #region //..StepState

    public enum WAVerifyStepEnum
    {
        UNDIFIND,
        IDLE,
        DONE,
        FAIL,
        VERIFY,
        ADJUSTSPEED,
        ADJUSTSPEEDFAILED,
        FOCUSING,
        FOCUSINGFAILED,
        ADJUSTLIGHT,
    }

    public abstract class VerifyStandardStepState
    {
        protected HighStandard _Module;
        public VerifyStandardStepState(HighStandard module)
        {
            _Module = module;
        }

        public abstract WAVerifyStepEnum GetState();
        public abstract EventCodeEnum Execute(VerifyInfo info);
    }

    public class IdleState : VerifyStandardStepState
    {
        public IdleState(HighStandard module) : base(module)
        {
        }

        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.IDLE;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                _Module.VerifyStepState = new AdjustSpeedState(_Module);
                //retVal = _Module.DoExecute();
                //if (retVal == EventCodeEnum.NONE)
                //{
                //    _Module.StepState = new DoneState(_Module);
                //}
                //else
                //{
                //    _Module.StepState = new AdjustSpeedState(_Module);
                //}
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "VerifyStandardStepState[IdleState] - VerExecuteify() : Error occurred.");
                //LoggerManager.Debug($"{err.ToString() }VerifyStandardStepState[IdleState] - Execute() : Error occurred.");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class VerifyFailState : VerifyStandardStepState
    {
        public VerifyFailState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.VERIFY;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            return EventCodeEnum.WAFERALIGN_MODULE_STATE_CANTRUN;
        }
    }

    public class DoneState : VerifyStandardStepState
    {
        public DoneState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.DONE;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            return EventCodeEnum.NONE;
        }
    }

    public class FailState : VerifyStandardStepState
    {
        public FailState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.FAIL;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            return EventCodeEnum.WAFERALIGN_MODULE_STATE_CANTRUN;
        }

    }

    public class AdjustSpeedState : VerifyStandardStepState
    {
        public AdjustSpeedState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.ADJUSTSPEED;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = _Module.CorrectVibration(info, ref _Module.VerifyProcResult);
                if (retVal == EventCodeEnum.NONE)
                {
                    _Module.VerifyStepState = new DoneState(_Module);
                }
                else
                {
                    _Module.VerifyStepState = new AdjustSpeedFailState(_Module);
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "VerifyStandardStepState[AdjustSpeedState] - VerExecuteify() : Error occurred.");
                //LoggerManager.Debug($"{err.ToString() }VerifyStandardStepState[AdjustSpeedState] - Execute() : Error occurred.");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    public class AdjustSpeedFailState : VerifyStandardStepState
    {
        public AdjustSpeedFailState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.ADJUSTSPEEDFAILED;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                _Module.VerifyStepState = new FocusingState(_Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "VerifyStandardStepState[AdjustSpeedFailState] - VerExecuteify() : Error occurred.");
                //LoggerManager.Debug($"{err.ToString() }VerifyStandardStepState[AdjustSpeedFailState] - Execute() : Error occurred.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    public class FocusingState : VerifyStandardStepState
    {
        public FocusingState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.FOCUSING;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = _Module.CorrectFocusing(info, ref _Module.VerifyProcResult);
                if (retVal == EventCodeEnum.NONE)
                {
                    _Module.VerifyStepState = new DoneState(_Module);
                }
                else
                {
                    _Module.VerifyStepState = new FocusingFailState(_Module);
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "VerifyStandardStepState[FocusingState] - VerExecuteify() : Error occurred.");
                //LoggerManager.Debug($"{err.ToString() }VerifyStandardStepState[FocusingState] - Execute() : Error occurred.");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class FocusingFailState : VerifyStandardStepState
    {
        public FocusingFailState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.FOCUSINGFAILED;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                _Module.VerifyStepState = new AdjustLightState(_Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "VerifyStandardStepState[FocusingFailState] - VerExecuteify() : Error occurred.");
                //LoggerManager.Debug($"{err.ToString() }VerifyStandardStepState[FocusingFailState] - Execute() : Error occurred.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class AdjustLightState : VerifyStandardStepState
    {
        public AdjustLightState(HighStandard module) : base(module)
        {
        }
        public override WAVerifyStepEnum GetState()
        {
            return WAVerifyStepEnum.ADJUSTLIGHT;
        }

        public override EventCodeEnum Execute(VerifyInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = _Module.CorrectLight(info, ref _Module.VerifyProcResult);
                if (retVal == EventCodeEnum.NONE)
                {
                    _Module.VerifyStepState = new DoneState(_Module);
                }
                else
                {
                    _Module.VerifyStepState = new FailState(_Module);
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "VerifyStandardStepState[AdjustLightState] - VerExecuteify() : Error occurred.");
                //LoggerManager.Debug($"{err.ToString() }VerifyStandardStepState[AdjustLightState] - Execute() : Error occurred.");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }


    #endregion

    public class VerifyInfo
    {

        private WAPatternInfomation _PtInfo;

        public WAPatternInfomation PtInfo
        {
            get { return _PtInfo; }
            set { _PtInfo = value; }
        }

        private WaferCoordinate _Pos;

        public WaferCoordinate Pos
        {
            get { return _Pos; }
            set { _Pos = value; }
        }

        private WaferCoordinate _TargetPos;

        public WaferCoordinate Targetpos
        {
            get { return _TargetPos; }
            set { _TargetPos = value; }
        }

        private IndexCoord _JumpIndex;

        public IndexCoord JumpIndex
        {
            get { return _JumpIndex; }
            set { _JumpIndex = value; }
        }


        private Values _DeviationValues = new Values();

        public Values DeviationValues
        {
            get { return _DeviationValues; }
            set { _DeviationValues = value; }
        }


        public VerifyInfo()
        {

        }

        public VerifyInfo(WAPatternInfomation patternInfo, WaferCoordinate pos, WaferCoordinate targetpos)
        {
            try
            {
                this.PtInfo = patternInfo;
                this.Pos = pos;
                this.Targetpos = targetpos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public VerifyInfo(WAPatternInfomation patternInfo, WaferCoordinate pos, WaferCoordinate targetpos, double deviationx, double deviationy)
        {
            try
            {
                this.PtInfo = patternInfo;
                this.Pos = pos;
                this.Targetpos = targetpos;
                DeviationValues.XValue = deviationx;
                DeviationValues.YValue = deviationy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public VerifyInfo(WAPatternInfomation patternInfo, WaferCoordinate pos, WaferCoordinate targetpos, IndexCoord index, Values deviationXY)
        {
            this.PtInfo = patternInfo;
            this.Pos = pos;
            this.Targetpos = targetpos;
            DeviationValues = deviationXY;
        }
    }

}

