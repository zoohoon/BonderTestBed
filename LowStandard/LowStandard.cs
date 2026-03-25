

namespace WALowStandardModule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.IO;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.WaferAlignEX;
    using RelayCommandBase;
    using System.Windows.Input;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using ProberInterfaces.PnpSetup;
    using ThetaAlignStandatdModule;
    using SubstrateObjects;
    using ProberInterfaces.Align;
    using ProberErrorCode;
    using WA_LowMagParameter_Standard;
    using LogModule;
    using ProberInterfaces.State;
    using System.Threading;
    using ThetaAlignStandardModule;
    using SerializerUtil;
    using MetroDialogInterfaces;
    using ProberInterfaces.WaferAligner;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    //  LowFocusingParam -> LowFocusingParam
    public class LowStandard : ThetaAlignStandard, ISetup, IRecovery, IParamNode, IProcessingModule, IHasDevParameterizable, ILotReadyAble, IPackagable, IWaferLowProcModule, INotifyPropertyChanged
    {

        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("830A6DE1-956A-54AB-407F-3D3222DFBA41");
        public enum WALowSetupFunction
        {
            UNDIFINE = -1,
            REGPATTERN,
            DELETEPATTERN,
            FOCUSING,
            SIXBUTTON,  // 260317 sebas : 6~10 add
            SEVENBUTTON,
            EIGHTBUTTON,
            NINEBUTTON,
            TENBUTTON
        }


        private IFocusing _WaferLowFocusModel;
        public IFocusing WaferLowFocusModel
        {
            get
            {
                if (_WaferLowFocusModel == null)
                {
                    if (this.FocusManager() != null)
                    {
                        _WaferLowFocusModel = this.FocusManager().GetFocusingModel((LowStandard_IParam as WA_LowMagParam_Standard).FocusingModuleDllInfo);
                    }
                }


                return _WaferLowFocusModel;
            }
        }
        private IFocusParameter FocusParam => (LowStandard_IParam as WA_LowMagParam_Standard).FocusParam;

        private IParam _LowStandard_IParam;
        [ParamIgnore]
        public IParam LowStandard_IParam
        {
            get { return _LowStandard_IParam; }
            set
            {
                if (value != _LowStandard_IParam)
                {
                    _LowStandard_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        public SubModuleMovingStateBase MovingState { get; set; }

        #region ..//Property
        private WA_LowMagParam_Standard LowStandardParam_Clone;

        private WALowSetupFunction ModifyCondition;

        private List<WaferProcResult> procresults;
        private CancellationTokenSource _OperCancelTokenSource = null;

        private int temppatternCount = 0;
        private int patternCount = 0;
        private int patternindex = -1;
        private int _CurPatternIndex;


        private double MinimumLength;
        private double OptimumLength;
        private double MaximumLength;

        public double RotateAngle = 0.0;

        private SubModuleStateBase _ModuleState;
        public override SubModuleStateBase SubModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

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
        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }
        }
        #endregion

        #region Command & Method   

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

                if (LowStandardParam_Clone.Patterns.Value.Count != 0)
                {
                    this.WaferAligner().WaferAlignInfo.LowFirstPatternPosition = (CatCoordinates)LowStandardParam_Clone.Patterns.Value[0];
                }

                if (parameter is EventCodeEnum)
                {
                    if ((EventCodeEnum)parameter == EventCodeEnum.NONE)
                        await base.Cleanup(parameter);
                    return retVal;
                }
                //bool ret = IsParameterChanged();

                //if (ret)
                //    retVal = await base.Cleanup(null);
                //else
                retVal = await base.Cleanup(parameter);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }



        private RelayCommand _UIModeChangeCommand;
        public ICommand UIModeChangeCommand6
        {
            get
            {
                if (null == _UIModeChangeCommand) _UIModeChangeCommand = new RelayCommand(
                    UIModeChange, EvaluationPrivilege.Evaluate(
                            CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                             new Action(() => { ShowMessages("UIModeChange"); }));
                return _UIModeChangeCommand;
            }
        }

        private void UIModeChange()
        {

        }

        private async Task RegistePatternCommand()
        {
            try
            {
                patternindex = CurPatternIndex;
                ModifyCondition = WALowSetupFunction.REGPATTERN;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async Task DeletePatternCommand()
        {
            try
            {

                ModifyCondition = WALowSetupFunction.DELETEPATTERN;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async Task FocusingCommand()
        {
            try
            {

                try
                {
                    ModifyCondition = WALowSetupFunction.FOCUSING;
                    await Modify();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                //Task<EventCodeEnum> stateTask;


                //AllBtnDisable();

                //FocusingModule.FocusParameter.FocusingCam.Value = CurCam.GetChannelType();

                //stateTask = Task.Run(() => {
                //    return FocusingModule.Focusing_Retry(
                //        false, true, false);
                //});
                //await stateTask;
                //retVal = stateTask.Result;

                //this.VisionManager().StartGrab(CurCam.GetChannelType());

                //AllBtnEnable();
                //InitCommonUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        private Task Next()
        {
            try
            {
                if (CurPatternIndex + 1 <= LowStandardParam_Clone.Patterns.Value.Count)
                {
                    CurPatternIndex++;

                    WAStandardPTInfomation ptinfo = LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1];

                    StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY(), ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.GetZ());

                    UpdatePatternSize(ptinfo, CurPatternIndex - 1);

                    foreach (var light in ptinfo.LightParams)
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

                    WAStandardPTInfomation ptinfo = LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1];

                    StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY(), ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.GetZ());

                    UpdatePatternSize(ptinfo, CurPatternIndex - 1);

                    foreach (var light in ptinfo.LightParams)
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
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer[CurTempPatternIndex - 1]);
                    }
                    else
                    {
                        CurPatternIndex++;
                        CurTempPatternIndex = 0;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1]);
                    }
                }
                else if (CurPatternIndex > 0)
                { 
                    if (CurPatternIndex < patternCount)
                    {
                        CurPatternIndex++;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1]);
                    }
                    else if (temppatternCount > 0)
                    {
                        CurPatternIndex = 0;
                        CurTempPatternIndex = 1;

                        patidx = CurTempPatternIndex - 1;
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer[CurTempPatternIndex - 1]);
                    }
                    else
                    {
                        CurPatternIndex = 1;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1]);
                    }
                }
                else if (CurPatternIndex == 0 && CurPatternIndex == 0)
                {
                    if (temppatternCount > 0)
                    {
                        CurTempPatternIndex++;

                        patidx = CurTempPatternIndex - 1;
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer[CurTempPatternIndex - 1]);

                    }
                    else
                    {
                        CurPatternIndex++;
                        CurTempPatternIndex = 0;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1]);
                    }

                }


                StepLabel = String.Format("REGISTED PATTERN {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);

                if (obj != null)
                {

                    this.StageSupervisor().StageModuleState.WaferLowViewMove
                       (obj.GetX()
                       + this.WaferObject.GetSubsInfo().WaferCenter.GetX(),
                       obj.GetY()
                       + this.WaferObject.GetSubsInfo().WaferCenter.GetY(),
                      obj.GetZ()
                       + this.WaferObject.GetSubsInfo().WaferCenter.GetZ());

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
            finally
            {
                //this.ViewModelManager().UnLockViewControl(this.GetHashCode());
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
                    obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer[CurTempPatternIndex - 1]);
                }
                else if (CurPatternIndex > 1)
                {
                    CurPatternIndex--;

                    patidx = CurPatternIndex - 1;
                    obj = new WAStandardPTInfomation(LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1]);

                }
                else if (temppatternCount > 0)
                {
                    if (CurPatternIndex == 1)
                    {
                        CurPatternIndex--;
                        CurTempPatternIndex = temppatternCount;

                        patidx = CurTempPatternIndex - 1;
                        obj = new WAStandardPTInfomation(this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer[CurTempPatternIndex - 1]);

                    }
                    else if (CurTempPatternIndex == 1)
                    {
                        CurTempPatternIndex = 0;
                        CurPatternIndex = LowStandardParam_Clone.Patterns.Value.Count;

                        patidx = CurPatternIndex - 1;
                        obj = new WAStandardPTInfomation(LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1]);
                    }
                }
                else
                {
                    CurPatternIndex = LowStandardParam_Clone.Patterns.Value.Count;

                    patidx = CurPatternIndex - 1;
                    obj = new WAStandardPTInfomation(LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1]);
                }

                StepLabel = String.Format("REGISTED PATTERN {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);
                if (obj != null)
                {

                    this.StageSupervisor().StageModuleState.WaferLowViewMove
                       (obj.GetX()
                       + this.WaferObject.GetSubsInfo().WaferCenter.GetX(),
                       obj.GetY()
                       + this.WaferObject.GetSubsInfo().WaferCenter.GetY(),
                      obj.GetZ()
                       + this.WaferObject.GetSubsInfo().WaferCenter.GetZ());

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
            finally
            {
                //this.ViewModelManager().UnLockViewControl(this.GetHashCode());
            }
            return Task.CompletedTask;
        }
        public Task ViewPattern()
        {
            try
            {
                ImageBuffer targetimage = null;
                if (CurTempPatternIndex >= 1)
                {
                    targetimage = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer[CurTempPatternIndex - 1].Imagebuffer;
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
                    if (LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].Imagebuffer == null)
                    {
                        LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].Imagebuffer = this.VisionManager().LoadImageFile(
                        this.FileManager().GetDeviceParamFullPath(LowStandardParam_Clone.PatternbasePath, LowStandardParam_Clone.PatternName + (CurPatternIndex - 1).ToString() + LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].PMParameter.PatternFileExtension.Value));
                    }
                    targetimage = LowStandardParam_Clone.Patterns.Value[CurPatternIndex - 1].Imagebuffer;

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
                //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "Low Align");
                //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "Low Align");


                double curtpos = 0.0;
                ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                curtpos = Math.Abs(curtpos);

                int converttpos = Convert.ToInt32(curtpos);

                if (converttpos != 0)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                    this.MotionManager().WaitForAxisMotionDone(axist, 60000);
                }
                this.WaferAligner().WaferAlignInfo.AlignAngle = 0;

                ClearData();
                var ret = Execute();
                SetStepSetupState();
                SetNextStepsNotCompleteState((PnpManager.SelectedPnpStep as ICategoryNodeItem).Header);
                if (this.GetState() == SubModuleStateEnum.DONE)
                {
                    EnableUseBtn();

                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.InfoMessageTitle, Properties.Resources.LowSuccessMessage, EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.LowFailMessage, EnumMessageStyle.Affirmative);
                }

                if (!this.WaferAligner().IsNewSetup && this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count > 0)
                {
                    FourButton.IsEnabled = false;
                }
                if(PadJogSelect.IconCaption.Contains("Pattern"))
                {
                    PadJogSelect.IconCaption = "WaferMap";
                }else
                {
                    PadJogSelect.IconCaption = "Pattern";
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

        public LowStandard()
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
                foreach (var pattern in LowStandardParam_Clone.Patterns.Value)
                {
                    pattern.Imagebuffer = this.VisionManager().LoadImageFile(
                        this.FileManager().GetDeviceParamFullPath(LowStandardParam_Clone.PatternbasePath, LowStandardParam_Clone.PatternName + index.ToString() + pattern.PMParameter.PatternFileExtension.Value));
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
        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                patternCount = LowStandardParam_Clone.Patterns.Value.Count;
                CurPatternIndex = 0;
                StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);

                //PadJogLeftUp.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/ArrowLeftW.png", UriKind.Absolute));
                //PadJogRightUp.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //    new Uri("pack://application:,,,/ImageResourcePack;component/Images/ArrowRightW.png", UriKind.Absolute));
                PadJogLeftUp.IconCaption = "PATTERN";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "PATTERN";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                StepSecondLabel = String.Format("");

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

                PadJogSelect.IconCaption = "View PT";
                PadJogSelect.Command = new AsyncCommand(ViewPattern);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                OneButton.IconCaption = "ADD";
                OneButton.Command = new AsyncCommand(RegistePatternCommand);

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Delete.png");
                TwoButton.IconCaption = "DELETE";
                TwoButton.Command = new AsyncCommand(DeletePatternCommand);

                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                ThreeButton.IconCaption = "FOCUS";
                ThreeButton.Command = new AsyncCommand(FocusingCommand);

                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Apply.png");
                FourButton.IconCaption = "APPLY";
                FourButton.Command = new AsyncCommand(Apply);

                // 260317 sebas 6~10 button add
                SixButton.IconCaption = "DieReg";
                SixButton.Command = new AsyncCommand(SixButtonCommand);
                SevenButton.IconCaption = "DAlign";
                SevenButton.Command = new AsyncCommand(SevenButtonCommand);
                EightButton.IconCaption = "1stDie";
                EightButton.Command = new AsyncCommand(EightButtonCommand);
                NineButton.IconCaption = "DClear";
                NineButton.Command = new AsyncCommand(NineButtonCommand);
                TenButton.IconCaption = "Button2";
                TenButton.Command = new AsyncCommand(TenButtonCommand);

                EnableUseBtn();
                ActiveStepLabelChanged();
                //PadJogRightDown.IsEnabled = false;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        // <-- 260317 sebas button add
        private async Task SixButtonCommand()
        {
            try
            {
                ModifyCondition = WALowSetupFunction.SIXBUTTON;
                await Modify();
            }
            catch
            {

            }
        }
        private async Task SevenButtonCommand()
        {
            try
            {
                ModifyCondition = WALowSetupFunction.SEVENBUTTON;
                await Modify();
            }
            catch
            {

            }
        }
        private async Task EightButtonCommand()
        {
            try
            {
                ModifyCondition = WALowSetupFunction.EIGHTBUTTON;
                await Modify();
            }
            catch
            {

            }
        }
        private async Task NineButtonCommand()
        {
            try
            {
                ModifyCondition = WALowSetupFunction.NINEBUTTON;
                await Modify();
            }
            catch
            {

            }
        }
        private async Task TenButtonCommand()
        {
            try
            {
                ModifyCondition = WALowSetupFunction.TENBUTTON;
                await Modify();
            }
            catch
            {

            }
        }
        // -->
        private EventCodeEnum InitRecoveryPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                patternCount = LowStandardParam_Clone.Patterns.Value.Count;
                temppatternCount = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count;
                CurPatternIndex = 1;
                CurTempPatternIndex = 0;
                StepLabel = String.Format("REGISTED PATTERN {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);

                //PadJogLeftUp.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/ArrowLeftW.png", UriKind.Absolute));
                //PadJogRightUp.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //    new Uri("pack://application:,,,/ImageResourcePack;component/Images/ArrowRightW.png", UriKind.Absolute));
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

                PadJogSelect.IconCaption = "View PT";
                PadJogSelect.Command = new AsyncCommand(ViewPattern);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                OneButton.IconCaption = "ADD";
                OneButton.Command = new AsyncCommand(RecoverySetup_RegistPattern);

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Delete.png");
                TwoButton.IconCaption = "DELETE";
                TwoButton.Command = new AsyncCommand(RecoverySetup_DeletePattern);

                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                ThreeButton.IconCaption = "FOCUS";
                ThreeButton.Command = new AsyncCommand(FocusingCommand);

                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Apply.png");
                FourButton.IconCaption = "APPLY";
                FourButton.Command = new AsyncCommand(Apply);

                EnableUseBtn();

                if (!this.WaferAligner().IsNewSetup && this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count > 0)
                {
                    FourButton.IsEnabled = false;
                }
                //PadJogRightDown.IsEnabled = false;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }

        #region //.. DevParameter

        public override EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new WA_LowMagParam_Standard();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(WA_LowMagParam_Standard));

                if (retval == EventCodeEnum.NONE)
                {
                    LowStandard_IParam = tmpParam;
                    LowStandardParam_Clone = LowStandard_IParam as WA_LowMagParam_Standard;
                }

                FocusingModule = WaferLowFocusModel;
                FocusingParam = FocusParam;


                if (LowStandardParam_Clone.Patterns.Value != null)
                {
                    if (LowStandardParam_Clone.Patterns.Value.Count != 0)
                    {
                        if (LowStandardParam_Clone.Patterns.Value[0].WaferCenter != null)
                        {
                            if (this.WaferAligner().WaferAlignInfo.PTWaferCenter == null)
                                this.WaferAligner().WaferAlignInfo.PTWaferCenter = new WaferCoordinate();
                            LowStandardParam_Clone.Patterns.Value[0].WaferCenter.CopyTo(this.WaferAligner().WaferAlignInfo.PTWaferCenter);
                        }

                    }
                }
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
                if (LowStandardParam_Clone != null)
                {
                    int index = 0;

                    foreach (var pattern in LowStandardParam_Clone.Patterns.Value)
                    {
                        if (pattern.Imagebuffer != null)
                        {
                            var filePathPrefix = this.FileManager().GetDeviceParamFullPath();
                            this.VisionManager().SavePattern(pattern, filePathPrefix);
                        }

                        index++;
                    }
                }

                LowStandard_IParam = LowStandardParam_Clone;

                RetVal = this.SaveParameter(LowStandard_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.SaveDevParameter() : Error occured.");
                throw err;
            }

            return RetVal;
        }

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(WA_LowMagParam_Standard));
                    if (target != null)
                    {
                        WA_LowMagParam_Standard paramobj = (WA_LowMagParam_Standard)target;
                        paramobj.JumpIndexManualInputParam.CopyTo(LowStandardParam_Clone.JumpIndexManualInputParam);
                        paramobj.DefaultPMParam.CopyTo(LowStandardParam_Clone.DefaultPMParam);
                        paramobj.FocusParam.CopyTo(LowStandardParam_Clone.FocusParam);
                        paramobj.Low_ProcessingPoint.CopyTo(LowStandardParam_Clone.Low_ProcessingPoint);
                        //paramobj.Patterns.CopyTo(LowStandardParam_Clone.Patterns);

                        LowStandard_IParam = LowStandardParam_Clone;

                        //LowStandard_IParam = (WA_LowMagParam_Standard)target;
                        //LowStandardParam_Clone = (WA_LowMagParam_Standard)target;
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

        #endregion

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = Properties.Resources.Header;
                RecoveryHeader = Properties.Resources.RecoveryHeader;
                retVal = InitPnpModuleStage_AdvenceSetting();
                FocusingParam.FocusingCam.Value =
                    this.VisionManager().GetCam(LowStandardParam_Clone.CamType).GetChannelType();

                AdvanceSetupView = new LowMagAdvanceSetup.View.LowMagStandardAdvanceSetupView();
                AdvanceSetupViewModel = new LowMagAdvanceSetup.ViewModel.LowMagStandardAdvanceSetupViewModel();
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
                PackagableParams.Add(SerializeManager.SerializeToByte(LowStandard_IParam));
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                InitStateUI();
                if (this.WaferAligner().IsNewSetup)
                    retVal = await InitSetup();
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
                var SubsInfo = Wafer.GetSubsInfo();
                var PhysInfo = Wafer.GetPhysInfo() as PhysicalInfo;

                SubsInfo.ActualDieSize.Width.Value = PhysInfo.DieSizeX.Value;
                SubsInfo.ActualDieSize.Height.Value = PhysInfo.DieSizeY.Value;

                SubsInfo.ActualDeviceSize.Width.Value = (SubsInfo.ActualDieSize.Width.Value - PhysInfo.DieXClearance.Value);
                SubsInfo.ActualDeviceSize.Height.Value = (SubsInfo.ActualDieSize.Height.Value - PhysInfo.DieYClearance.Value);

                LoggerManager.Debug($"[{this.GetType().Name}], InitSetup() : ActualDieSize = (Width : {SubsInfo.ActualDieSize.Width.Value:0.00}, Height : {SubsInfo.ActualDieSize.Height.Value:0.00})");

                if(WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)   // 260126 sebas : 로우캠 FD/Wafer 구분
                {
                    LoggerManager.PinLog("Wafer low align start");
                    CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                }
                else
                {
                    LoggerManager.PinLog("FD Wafer low align start");
                    CurCam = this.VisionManager().GetCam(LowStandardParam_Clone.CamType);
                }

                ushort defaultlightvalue = 25;  // 260112 sebas : 로우캠 조명값 낮춤 (85 -> 25)
                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                }

                switch (Wafer.GetPhysInfo().WaferSize_um.Value)
                {
                    case 150000:
                        MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (1.0 / 2.0);
                        OptimumLength = LowStandardParam_Clone.AlignOptimumLength * (1.0 / 2.0);
                        MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (1.0 / 2.0);
                        break;
                    case 200000:
                        MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (2.0 / 3.0);
                        OptimumLength = LowStandardParam_Clone.AlignOptimumLength * (2.0 / 3.0);
                        MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (2.0 / 3.0);
                        break;
                    case 300000:
                        MinimumLength = LowStandardParam_Clone.AlignMinimumLength;
                        OptimumLength = LowStandardParam_Clone.AlignOptimumLength;
                        MaximumLength = LowStandardParam_Clone.AlignMaximumLength;
                        break;
                }

                MainViewTarget = DisplayPort;
                MiniViewTarget = Wafer;

                retVal = InitPNPSetupUI();

                MoveFirstPattern();

                if (WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)   // 260126 sebas : 로우캠 FD/Wafer 구분
                {
                    this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);
                }
                else
                {
                    this.VisionManager().StartGrab(LowStandardParam_Clone.CamType, this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.WAFER_SETUP_PROCEDURE_EROOR;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public void UpdateProcessingIndex()
        {
            try
            {
                Low_ProcessingPointEnum low_processing_point = LowStandardParam_Clone.Low_ProcessingPoint.Value;
                if (LowStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    foreach (var ptinfo in LowStandardParam_Clone.Patterns.Value)
                    {
                        if (ptinfo.JumpIndexs.Count > 0)
                        {
                            StandardJumpIndexParam maxXIndexJump = null;
                            foreach (var jumpindex in ptinfo.JumpIndexs)
                            {
                                if (jumpindex.Index.XIndex == 0 && jumpindex.Index.YIndex == 0)
                                {
                                    jumpindex.AcceptProcessing.Value = true;
                                }
                                else
                                {
                                    if (low_processing_point == Low_ProcessingPointEnum.LOW_2PT)
                                    {
                                        jumpindex.AcceptProcessing.Value = false;

                                        if (maxXIndexJump == null || Math.Abs(jumpindex.Index.XIndex) > Math.Abs(maxXIndexJump.Index.XIndex))
                                        {
                                            maxXIndexJump = jumpindex;
                                        }
                                    }
                                    else
                                    {
                                        jumpindex.AcceptProcessing.Value = true;
                                    }
                                }
                            }

                            if (maxXIndexJump != null && low_processing_point == Low_ProcessingPointEnum.LOW_2PT)
                            {
                                maxXIndexJump.AcceptProcessing.Value = true;
                            }
                        }

                        if (ptinfo.PostJumpIndex.Count > 0)
                        {
                            //Not Exist
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
                if (LowStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    foreach (var ptinfo in LowStandardParam_Clone.Patterns.Value)
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
                            //Not Exist
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdatePatternSize(WAStandardPTInfomation ptinfo, int index)
        {
            try
            {
                if (ptinfo.Imagebuffer == null)
                {
                    ptinfo.Imagebuffer = this.VisionManager().LoadImageFile(this.FileManager().GetDeviceParamFullPath(LowStandardParam_Clone.PatternbasePath, LowStandardParam_Clone.PatternName + index.ToString() + ptinfo.PMParameter.PatternFileExtension.Value));
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
                    LoggerManager.Debug($"[LowStandard] UpdatePatternSize(), Pattern ImageBuffer null.");
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
                LoggerManager.Debug($"[LowStandard] MoveFirstPattern(), Wafe Center (X,Y) = ({Wafer.GetSubsInfo().WaferCenter.X.Value:0.00}, {Wafer.GetSubsInfo().WaferCenter.Y.Value:0.00})");
                if (LowStandardParam_Clone.Patterns.Value != null)
                {
                    patternCount = LowStandardParam_Clone.Patterns.Value.Count;

                    if (LowStandardParam_Clone.Patterns.Value.Count != 0)
                    {
                        WAStandardPTInfomation ptinfo = LowStandardParam_Clone.Patterns.Value[0];

                        if (WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)   // 260123 sebas : FD/Wafer 구분
                        {
                            StageSupervisor.StageModuleState.WaferLowViewMove_Wafer( Wafer.GetSubsInfo().WaferCenter_WF.GetX(),Wafer.GetSubsInfo().WaferCenter_WF.GetY(),Wafer.GetSubsInfo().WaferCenter_WF.GetZ());
                        }
                        else
                        {
                            // <-- 260223 sebas : 개조 후 스트로크 문제로 센터가 아닌 다른 위치로 이동
                            //StageSupervisor.StageModuleState.WaferLowViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY(), ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.GetZ());

                            // 하이캠 -> 로우캠 전환시 사용 됨
                            double FDCEnterOffsetY = 0; // -Wafer.GetSubsInfo().ActualDieSize.Height.Value * 6;    // X축은 변경 없음. Y축만 변경
                            StageSupervisor.StageModuleState.WaferLowViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY() + FDCEnterOffsetY, ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.GetZ());
                            LoggerManager.PinLog($"로우얼라인 시작 위치 X = {Wafer.GetSubsInfo().WaferCenter.GetX()}, Y = {Wafer.GetSubsInfo().WaferCenter.GetY() + FDCEnterOffsetY}");
                            // -->
                        }

                        UpdatePatternSize(ptinfo, 0);

                        CurPatternIndex = 1;
                    }
                    else
                    {
                        patternCount = 0;
                        CurPatternIndex = 0;

                        if (WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)   // 260123 sebas : FD/Wafer 구분
                        {
                            LoggerManager.PinLog("Move to Wafer first low pattern");
                            this.StageSupervisor().StageModuleState.WaferLowViewMove_Wafer(Wafer.GetSubsInfo().WaferCenter_WF.GetX(), Wafer.GetSubsInfo().WaferCenter_WF.GetY(), Wafer.GetSubsInfo().ActualThickness);
                        }
                        else
                        {
                            LoggerManager.PinLog("Move to FD Wafer first low pattern");
                            // 위치 주의
                            //this.StageSupervisor().StageModuleState.WaferLowViewMove(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), Wafer.GetSubsInfo().ActualThickness);
                            double FDCEnterOffsetY = -Wafer.GetSubsInfo().ActualDieSize.Height.Value * 0;    // X축은 변경 없음. Y축만 변경
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY() + FDCEnterOffsetY, Wafer.GetSubsInfo().ActualThickness);
                            LoggerManager.PinLog($"로우얼라인 시작 위치 X = {Wafer.GetSubsInfo().WaferCenter.GetX()}, Y = {Wafer.GetSubsInfo().WaferCenter.GetY() + FDCEnterOffsetY}");
                        }
                        
                        double curtpos = 0.0;
                        ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                        this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                        curtpos = Math.Abs(curtpos);

                        int converttpos = Convert.ToInt32(curtpos);

                        if (converttpos != 0)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                            int ret = this.MotionManager().WaitForAxisMotionDone(axist, 60000);
                        }
                        
                        this.WaferAligner().WaferAlignInfo.AlignAngle = 0;
                    }
                }
                else
                {
                    patternCount = 0;
                    CurPatternIndex = 0;

                    this.StageSupervisor().StageModuleState.WaferLowViewMove(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), Wafer.GetSubsInfo().ActualThickness);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                patternCount = LowStandardParam_Clone.Patterns.Value.Count;
                StepLabel = String.Format("PATTERN  {0}/{1}", CurPatternIndex, patternCount);
            }
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            //SeletedStep = this;
            try
            {
                if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer == null)
                {
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer = new ObservableCollection<WAStandardPTInfomation>();
                }
                if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count > 0)
                {
                    SetNodeSetupState(EnumMoudleSetupState.VERIFY);
                }

                CurCam = this.VisionManager().GetCam(LowStandardParam_Clone.CamType);
                
                if (LowStandardParam_Clone.Patterns?.Value[0].LightParams == null)
                {
                    ushort defaultlightvalue = 85;
                    for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                    {
                        CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                    }

                }

                this.VisionManager().StartGrab(LowStandardParam_Clone.CamType, this);

                UseUserControl = UserControlFucEnum.PTRECT;

                switch (Wafer.GetPhysInfo().WaferSize_um.Value)
                {
                    case 150000:
                        MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (1.0 / 2.0);
                        OptimumLength = LowStandardParam_Clone.AlignOptimumLength * (1.0 / 2.0);
                        MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (1.0 / 2.0);
                        break;
                    case 200000:
                        MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (2.0 / 3.0);
                        OptimumLength = LowStandardParam_Clone.AlignOptimumLength * (2.0 / 3.0);
                        MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (2.0 / 3.0);
                        break;
                    case 300000:
                        MinimumLength = LowStandardParam_Clone.AlignMinimumLength;
                        OptimumLength = LowStandardParam_Clone.AlignOptimumLength;
                        MaximumLength = LowStandardParam_Clone.AlignMaximumLength;
                        break;
                }

                //ZoomObject = Wafer;//==> Nick : Zoom IN/OUT할 객체를 설정함, MainViewTarget과 MiniViewTarge 설정 전에 ZoomObject 설정 해야함, InitSetup마다 호출 해야함. 
                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                MainViewTarget = DisplayPort;
                MiniViewTarget = this.StageSupervisor().WaferObject;

                MoveFirstPattern();

                InitRecoveryPNPSetupUI();
                LoadPatterImage();

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }


        public EventCodeEnum DoExecute()

        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Low_Mag_Start);

                //SetNextStepsNotCompleteState((PnpManager.SelectedPnpStep as ICategoryNodeItem).Header);
                WA_LowMagParam_Standard param = LowStandard_IParam as WA_LowMagParam_Standard;
                List<WAStandardPTInfomation> ptinfos = param.Patterns.Value.ToList<WAStandardPTInfomation>();
                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                    ptinfos = (LowStandardParam_Clone as WA_LowMagParam_Standard).Patterns.Value.ToList<WAStandardPTInfomation>();

                if (this.WaferAligner().WaferAlignInfo.RecoveryParam != null)
                {
                    if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count > 0)
                    {
                        List<WAStandardPTInfomation> recoveryptinofs = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.ToList<WAStandardPTInfomation>();
                        foreach (var pt in ptinfos)
                        {
                            recoveryptinofs.Add(pt);
                        }

                        ptinfos = recoveryptinofs;
                    }
                }

                if (CurCam == null)
                    CurCam = this.VisionManager().GetCam((LowStandard_IParam as WA_LowMagParam_Standard).CamType);
                if (GetState() != SubModuleStateEnum.DONE)
                {
                    //우선은 주석 !*****************
                    //if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                    //{
                    //    ptinfos = LowStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>();
                    //}

                    if (ptinfos.Count != 0)
                    {
                        procresults = new List<WaferProcResult>();

                        // Update Processing Start
                        //pad setup에서 isenewsetup == true
                        //wafer new, recovery setup 
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
                            this.VisionManager().StopGrab(CurCam.GetChannelType());
                            this.VisionManager().LoadImageFromFloder(@"C:\ProberSystem\EmulImages\WaferAlign\LowMag\", CurCam.GetChannelType());
                        }
                        this.WaferAligner().WaferAlignInfo.AlignAngle = 0;
                        SettingLimit(param.AlignMinimumLength, param.AlignOptimumLength, param.AlignMaximumLength);
                        if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP)
                        {
                            RetVal = ThetaAlign(ref ptinfos, ref procresults, ref RotateAngle, _OperCancelTokenSource, false, !this.WaferAligner().IsNewSetup); 
                        }
                        else
                        {
                            RetVal = ThetaAlign(ref ptinfos, ref procresults, ref RotateAngle, _OperCancelTokenSource, true, true);
                        }
                            

                        if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count < 0)
                        {
                            for (int index = 0; index < ptinfos.Count; index++)
                            {
                                param.Patterns.Value[index] = ptinfos[index];
                            }
                        }

                        //this.WaferAligner().ReasonOfError.Reason = "";

                        if (this.WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                        {
                            RetVal = EventCodeEnum.NONE;
                            SubModuleState = new SubModuleDoneState(this);
                        }

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            if (RetVal == EventCodeEnum.FOCUS_VALUE_THRESHOLD | RetVal == EventCodeEnum.FOCUS_VALUE_FLAT | RetVal == EventCodeEnum.FOCUS_VALUE_DUALPEAK)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.WAFER_LOW_FOCUSING_FAIL);
                            }
                            else if (RetVal == EventCodeEnum.VISION_PM_NOT_FOUND | RetVal == EventCodeEnum.VISION_PM_EXCEPTION)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.WAFER_LOW_PATTERN_NOT_FOUND);
                            }
                        }
                    }
                }
                else if (GetState() == SubModuleStateEnum.DONE)
                {
                    if (ptinfos.Count != 0)
                    {
                        ICamera cam = this.VisionManager().GetCam(ptinfos[0].CamType.Value);

                        //x , y 중 WaferCenter 가 화면의 1/4 이상 벗어났다.
                        //bool xoffset = ((cam.GetGrabSizeWidth() / 4) * cam.GetRatioX()) >
                        //    Math.Abs(Wafer.SubsInfo.WaferCenter.GetX() - ptinfos[0].ProcWaferCenter.GetX()) ?
                        //    true : false;

                        //bool yoffset = ((cam.GetGrabSizeHeight() / 4) * cam.GetRatioY()) >
                        //    Math.Abs(Wafer.SubsInfo.WaferCenter.GetY() - ptinfos[0].ProcWaferCenter.GetY()) ?
                        //    true : false;


                        //8 보다 오차가 작으면 true, 크면 false
                        bool xoffset = Math.Abs(Wafer.GetSubsInfo().WaferCenter.GetX() - ptinfos[0].ProcWaferCenter.GetX()) < 8 ?
                          true : false;

                        bool yoffset = Math.Abs(Wafer.GetSubsInfo().WaferCenter.GetY() - ptinfos[0].ProcWaferCenter.GetY()) < 8 ?
                            true : false;


                        //x , y 중 WaferCenter 가 달라졌다 (Edge 로 인해 Center가 달라졌다).
                        if (!(xoffset && yoffset))
                        {
                            WAStandardPTInfomation ptinfo = ptinfos.Find(info => info.PatternState.Value != PatternStateEnum.FAILED);
                            if (ptinfo == null)
                                RetVal = EventCodeEnum.SUB_RECOVERY;
                            else
                            {
                                RetVal = ThetaAlign(ref ptinfo, null, ref RotateAngle);
                                if (RetVal != EventCodeEnum.NONE)
                                {
                                    for (int index = 0; index < ptinfos.Count; index++)
                                    {
                                        if (ptinfos[index].PatternState.Value != PatternStateEnum.FAILED)
                                        {
                                            WAStandardPTInfomation into = ptinfos[index];
                                            RetVal = ThetaAlign(ref into, null, ref RotateAngle);
                                        }
                                    }

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
                                        LoggerManager.Debug($"Wafer Low Mag Align Cur State Done Fail => count :{count}, totalX :{totalX}, totalY : {totalY}, xlimit : {xlimit}, ylimit : {ylimit}");
                                        RetVal = EventCodeEnum.SUB_RECOVERY;
                                    }
                                    else
                                        RetVal = EventCodeEnum.NONE;
                                }
                                else
                                    RetVal = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            for (int index = 0; index < ptinfos.Count; index++)
                            {
                                if (ptinfos[index].PatternState.Value != PatternStateEnum.FAILED)
                                {
                                    WAStandardPTInfomation into = ptinfos[index];
                                    RetVal = ThetaAlign(ref into, null, ref RotateAngle);

                                    if (RetVal == EventCodeEnum.NONE)
                                        break;
                                }
                            }

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
                                    break;
                                }
                            }

                            double xlimit = ((cam.GetGrabSizeWidth() / 4) * cam.GetRatioX());
                            double ylimit = ((cam.GetGrabSizeHeight() / 4) * cam.GetRatioY());

                            if ((Math.Abs(totalX / 4) > xlimit) || (Math.Abs(totalY / 4) > ylimit))
                            {
                                LoggerManager.Debug($"Wafer Low Mag Align Cur State Done Fail => count :{count}, totalX :{totalX}, totalY : {totalY}, xlimit : {xlimit}, ylimit : {ylimit}");
                                RetVal = EventCodeEnum.SUB_RECOVERY;
                            }
                            else
                                RetVal = EventCodeEnum.NONE;
                        }
                    }
                }

                else if (GetState() == SubModuleStateEnum.ERROR)
                    RetVal = EventCodeEnum.UNDEFINED;
                else
                    RetVal = EventCodeEnum.SUB_RECOVERY;
                //MovingState.Stop();

                //RetVal = EventCodeEnum.SUB_RECOVERY;
                if (RetVal == EventCodeEnum.NONE)
                {
                    this.Wafer.SetAlignState(AlignStateEnum.IDLE);
                    SubModuleState = new SubModuleDoneState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Low_Mag_OK);
                }
                else if (RetVal == EventCodeEnum.SUB_RECOVERY)
                {
                    //this.WaferAligner().ReasonOfError.Reason = "Low Pattern Not Found";
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "Low Pattern Not Found", this.GetType().Name);

                    SubModuleState = new SubModuleRecoveryState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Low_Mag_Failure, RetVal);
                }
                else if (RetVal == EventCodeEnum.SUB_SKIP)
                {
                    SubModuleState = new SubModuleSkipState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Low_Mag_OK);
                }
                else
                {
                    //this.WaferAligner().ReasonOfError.Reason = "Low Pattern Not Found";
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "Low Pattern Not Found", this.GetType().Name);

                    SubModuleState = new SubModuleErrorState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Low_Mag_Failure, RetVal);
                }

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Low_Mag_Failure, RetVal);

                throw err;
            }
            finally
            {
                this.VisionManager().ClearGrabberUserImage(CurCam.GetChannelType());
                this.VisionManager().StopGrab(CurCam.GetChannelType());
                //MovingState.Stop();
            }
            return RetVal;
        }

        public async Task<EventCodeEnum> Modify()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //MovingState.Moving();
                switch (ModifyCondition)
                {
                    case WALowSetupFunction.REGPATTERN:
                        retVal = await RegistPattern();
                        if (retVal == EventCodeEnum.NONE)
                            IsParamChanged = true;
                        break;
                    case WALowSetupFunction.DELETEPATTERN:
                        retVal = DeletePattern();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            IsParamChanged = true;
                        }
                        break;
                    case WALowSetupFunction.FOCUSING:
                        //FocusingModule.FocusParameter.FocusingCam.Value = CurCam.GetChannelType();
                        FocusingParam.FocusingCam.Value = CurCam.GetChannelType();

                        retVal = FocusingModule.Focusing_Retry(FocusingParam, false, true, false, this);

                        //if (retVal != EventCodeEnum.NONE)
                        //    this.MetroDialogManager().ShowMessageDialog("","",EnumMessageStyle.Affirmative);
                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        {
                            Wafer.GetSubsInfo().ActualThickness = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetZ();
                        }
                        else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        {
                            Wafer.GetSubsInfo().ActualThickness = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert().GetZ();
                        }

                        this.WaferAligner().ResetHeightPlanePoint();

                        this.WaferAligner().CreateBaseHeightProfiling(new WaferCoordinate(WaferObject.GetSubsInfo().WaferCenter.GetX(), WaferObject.GetSubsInfo().WaferCenter.GetY(), WaferObject.GetSubsInfo().ActualThickness));

                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                        break;

                    case WALowSetupFunction.SIXBUTTON:
                        retVal = await DieRegPattern();
                        break;
                    case WALowSetupFunction.SEVENBUTTON:
                        retVal = await DieAlignCommand();
                        break;
                    case WALowSetupFunction.EIGHTBUTTON:
                        retVal = await MoveDie1();
                        break;
                    case WALowSetupFunction.NINEBUTTON:     // Die Regist Pattern Clear
                        break;
                    case WALowSetupFunction.TENBUTTON:
                        break;

                    default:
                        break;
                }

                patternCount = LowStandardParam_Clone.Patterns.Value.Count;
                StepLabel = String.Format("PATTERN  {0}/{1} ", CurPatternIndex, patternCount);

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

        // <-- 260317 sebas func add
        private List<WAStandardPTInfomation> _capturedPatterns = new List<WAStandardPTInfomation>();
        private const int MaxCaptureCount = 3;
        private string _captureSavePath = @"C:\\ProberSystem\\Default\\Parameters\\DeviceParam\\DEFAULTDEVNAME\\WaferAlignParam\\PickCam";

        public async Task<EventCodeEnum> DieRegPattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                WAStandardPTInfomation ptinfo = LowStandardParam_Clone.Patterns.Value[0];

                // Camera check
                if (CurCam.GetChannelType() != EnumProberCam.PIN_HIGH_CAM) // 핀하이카메라를 픽커카메라로 변경 != LowStandardParam_Clone.CamType)
                {
                    CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                    ushort defaultlightvalue = 50;
                    for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                    {
                        CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                    }

                    this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                    // *** PickCAM으로 이동 ***
                    this.StageSupervisor().StageModuleState.WaferLowViewMove_PickCAM(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), ptinfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY(), ptinfo.GetZ() + Wafer.GetSubsInfo().WaferCenter.GetZ());
                    // 아니면 아래거로?
                    //this.StageSupervisor().StageModuleState.WaferLowViewMove_PickCAM(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), Wafer.GetSubsInfo().WaferCenter.GetZ());
                }

                if (_capturedPatterns.Count >= MaxCaptureCount)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Capture", "3 Pictures are already saved.", EnumMessageStyle.Affirmative);

                    //DieCount = 1;
                    return retVal;
                }

                if(!Directory.Exists(_captureSavePath))
                {
                    Directory.CreateDirectory(_captureSavePath);
                }

                // ROI 가져오기
                RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();
                if(patternparam.Width == 0 || patternparam.Height == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Capture Error", "Check the pattern ROI size", EnumMessageStyle.Affirmative);
                    return retVal;
                }

                // Image grab
                ImageBuffer frame = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);
                if(frame == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Capture Error", "Failed to capture frame.", EnumMessageStyle.Affirmative);
                    return retVal;
                }

                // PatternInfo 생성 
                WAStandardPTInfomation patterninfo = new WAStandardPTInfomation();

                patterninfo.CamType.Value = CurCam.GetChannelType();
                patterninfo.LightParams = new ObservableCollection<LightValueParam>();

                // Light 정보 저장
                for(int i = 0; i< CurCam.LightsChannels.Count; i++)
                {
                    patterninfo.LightParams.Add(new LightValueParam(CurCam.LightsChannels[i].Type.Value, (ushort)CurCam.GetLight(CurCam.LightsChannels[i].Type.Value)));
                }

                // GrayLevel 계산
                this.VisionManager().GetGrayValue(ref frame);
                patterninfo.GrayLevel = frame.GrayLevelValue;

                // ROI 기준 이미지 저장
                patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(frame, patternparam.LocationX, patternparam.LocationY, patternparam.Width, patternparam.Height);

                // 리스트 저장
                _capturedPatterns.Add(patterninfo);

                // 파일 저장
                string fileName = $"Picture_{_capturedPatterns.Count}.bmp";
                string fullPath = Path.Combine(_captureSavePath, fileName);

                this.VisionManager().SaveImageBuffer(patterninfo.Imagebuffer, fullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                LoggerManager.PinLog($"Die Pattern captured : {_capturedPatterns.Count}, Saved : {fullPath}");

                await this.MetroDialogManager().ShowMessageDialog("Capture", $"Frame saved {_capturedPatterns.Count}", EnumMessageStyle.Affirmative);
            }
            catch
            {
                throw;
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
            return retVal;
        }
        public async Task<EventCodeEnum> DieAlignCommand()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                int jumpCount = 3;  // 연속 진행할 다이 개수
                double xOffset = -3000; // 부호 방향 주의, 다이 점프 오프셋
                double yOffset = -1000; // 부호 방향 주의, 다이 점프 오프셋

                // Camera check
                if (CurCam.GetChannelType() != EnumProberCam.PIN_HIGH_CAM) // 핀하이카메라를 픽커카메라로 변경 != LowStandardParam_Clone.CamType)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Pattern Register Error", "change to Pick camera", EnumMessageStyle.Affirmative);

                    CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                    ushort defaultlightvalue = 50;  // 260112 sebas : 조명 너무 밝아서 내림 (85 -> 50)
                    for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                    {
                        CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                    }

                    this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                    // *** PickCAM으로 이동 ***
                }

                for (int count = 0; count < jumpCount; count++)
                {
                    retVal = DieAlign();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        DiePosAdd(count);

                        Thread.Sleep(1000);

                        if (count < jumpCount - 1)   // 마지막 루프일 때 제외
                        {
                            retVal = DieJump(xOffset, yOffset);

                            Thread.Sleep(1000);

                            retVal = DieEdge();
                        }
                    }
                }
                // DiePos 값을 WaferSubstrate쪽에 옮김
                ConvertDataToWafer();
            }
            catch
            {

            }
            finally
            {
                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);
            }

            return retVal;
        }
        int saveCount = 1;   // 이미지파일 저장 체크용
        public EventCodeEnum DieAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                ProbeAxisObject xaxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                WAStandardPTInfomation patterninfo = new WAStandardPTInfomation();
                WAStandardPTInfomation patterninfo1 = new WAStandardPTInfomation();

                if (_capturedPatterns.Count > 0)
                {
                    // 1번째 캡처 패턴 사용
                    WAStandardPTInfomation capturedPattern = _capturedPatterns[0];

                    // PMParameter 복사
                    capturedPattern.PMParameter.CopyTo(patterninfo.PMParameter);

                    // PatternMatching
                    PMResult pmresult = this.VisionManager().PatternMatching(capturedPattern, this);

                    // 타겟 다이 이미지 파일 저장
                    string fileName = $"Frame_PatternMaching{saveCount}_1 .bmp";
                    string fullPath = Path.Combine(_captureSavePath, fileName);

                    this.VisionManager().SaveImageBuffer(
                        pmresult.ResultBuffer,
                        fullPath,
                        IMAGE_LOG_TYPE.NORMAL,
                        EventCodeEnum.NONE);


                    this.VisionManager().StartGrab(capturedPattern.CamType.Value, this);

                    retVal = pmresult.RetValue;

                    if (retVal == EventCodeEnum.NONE)
                    {
                        LoggerManager.PinLog($"1st Pattern : X = {pmresult.ResultParam[0].XPoss}, Y = {pmresult.ResultParam[0].YPoss}, Score = {pmresult.ResultParam[0].Score}");

                        WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);

                        patterninfo.X.Value = wcoord.GetX() - xaxis.Status.RawPosition.Ref;
                        patterninfo.Y.Value = wcoord.GetY() - yaxis.Status.RawPosition.Ref;
                        patterninfo.Z.Value = wcoord.GetZ() - zaxis.Status.RawPosition.Ref;

                        this.StageSupervisor().StageModuleState.WaferLowViewMove_PickCAM(
                            patterninfo.GetX() + xaxis.Status.RawPosition.Ref,
                            patterninfo.GetY() + yaxis.Status.RawPosition.Ref);

                        // 2번 위치로 이동
                        this.StageSupervisor().StageModuleState.WaferLowViewMove_PickCAM(patterninfo.GetX() + xaxis.Status.RawPosition.Ref + 10000,
                                                                                            patterninfo.GetY() + yaxis.Status.RawPosition.Ref);

                        // 2번 위치 정렬
                        WAStandardPTInfomation capturedPattern1 = _capturedPatterns[1];
                        capturedPattern1.PMParameter.CopyTo(patterninfo1.PMParameter);

                        // PatternMatching
                        PMResult pmresult1 = this.VisionManager().PatternMatching(capturedPattern1, this);

                        // 타겟 다이 이미지 파일 저장
                        string fileName1 = $"Frame_PatternMaching{saveCount}_2.bmp";
                        string fullPath1 = Path.Combine(_captureSavePath, fileName1);

                        this.VisionManager().SaveImageBuffer(
                            pmresult1.ResultBuffer,
                            fullPath1,
                            IMAGE_LOG_TYPE.NORMAL,
                            EventCodeEnum.NONE);

                        this.VisionManager().StartGrab(capturedPattern1.CamType.Value, this);
                        retVal = pmresult1.RetValue;

                        if (retVal == EventCodeEnum.NONE)
                        {
                            LoggerManager.PinLog($"2nd Pattern X = {pmresult1.ResultParam[0].XPoss}, Y = {pmresult1.ResultParam[0].YPoss}, Score = {pmresult1.ResultParam[0].Score}");

                            // 세타 계산
                            double rotateangle = 0;
                            double curTheta = 0;
                            WaferCoordinate wcoord1 = ChangedLocationFormPT(pmresult1);

                            MachineCoordinate mccoord = new MachineCoordinate();
                            MachineCoordinate mccoord1 = new MachineCoordinate();
                            mccoord = this.CoordinateManager().WaferLowChuckConvert.ConvertBack(wcoord);
                            mccoord1 = this.CoordinateManager().WaferLowChuckConvert.ConvertBack(wcoord1);

                            //double angle = this.CoordinateManager().CalcP2PAngle(patterninfo1.GetX(), patterninfo1.GetY(), patterninfo.GetX(), patterninfo.GetY());
                            //double angle = this.CoordinateManager().CalcP2PAngle(pmresult1.ResultParam[0].XPoss, pmresult1.ResultParam[0].YPoss, pmresult.ResultParam[0].XPoss, pmresult.ResultParam[0].XPoss);
                            double angle = this.CoordinateManager().CalcP2PAngle(wcoord1.X.Value, wcoord1.Y.Value, wcoord.X.Value, wcoord.Y.Value);

                            LoggerManager.PinLog($"Number {saveCount} Die's Left Low Corner : X = {wcoord.X.Value}, Y = {wcoord.Y.Value}");
                            LoggerManager.PinLog($"Number {saveCount} Die's Right Low Corner : X = {wcoord1.X.Value}, Y = {wcoord1.Y.Value}");

                            angle = Math.Round(angle, 6);

                            if (angle <= -1 && angle >= -89)
                            {

                            }
                            else if (angle <= -90 & angle > -180)
                            {
                                rotateangle = (180 + (angle)) * -1;
                            }
                            else if (angle <= -1 && angle >= 90)
                            {

                            }
                            else if (angle >= 90 && angle < 180)
                            {
                                rotateangle = 180 + (-angle);
                            }

                            // 세타 보정
                            rotateangle = Math.Round(rotateangle, 6);
                            var FDT1axis = this.MotionManager().GetAxis(EnumAxisConstants.FDT1);

                            LoggerManager.PinLog($"Number {saveCount} Die's theta angle = {rotateangle}");
                            this.StageSupervisor().StageModuleState.StageRelMove(FDT1axis, (rotateangle * 10000d));

                            this.MotionManager().GetRefPos(EnumAxisConstants.FDT1, ref curTheta);
                            this.WaferAligner().WaferAlignInfo.AlignAngle = curTheta / 10000d;

                            // 세타 이동 후 2번 위치 정렬
                            PMResult pmresult3 = this.VisionManager().PatternMatching(capturedPattern1, this);

                            this.VisionManager().StartGrab(capturedPattern1.CamType.Value, this);

                            retVal = pmresult3.RetValue;

                            if (retVal == EventCodeEnum.NONE)
                            {

                                WaferCoordinate wcoord3 = ChangedLocationFormPT(pmresult3);

                                patterninfo1.X.Value = wcoord3.GetX() - xaxis.Status.RawPosition.Ref;
                                patterninfo1.Y.Value = wcoord3.GetY() - yaxis.Status.RawPosition.Ref;
                                patterninfo1.Z.Value = wcoord3.GetZ() - zaxis.Status.RawPosition.Ref;

                                this.StageSupervisor().StageModuleState.WaferLowViewMove_PickCAM(
                                    patterninfo1.GetX() + xaxis.Status.RawPosition.Ref,
                                    patterninfo1.GetY() + yaxis.Status.RawPosition.Ref);

                            }
                        }
                    }
                }
                saveCount++;
                retVal = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return retVal;
        }
        public EventCodeEnum DieEdge()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                double moveoffset = 100;  // 몇 um 간격으로 이동할 것인지
                int threshold = 120;    // 기준이 되는 조명 수치
                int moveBackoffset = 4; // 영역안으로 다시 들어가는 거리의 배율
                int limitCount = 0;    // 이동 횟수 제한(무한이동 제한)

                ProbeAxisObject xaxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                // 기존 코드와 동일한 ROI 변수명 사용
                RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();

                while (true)    // Y축 이동
                {
                    // Frame Grab (기존 코드와 동일)
                    ImageBuffer frame = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                    if (frame == null)
                        return retVal;

                    // ROI 생성 (기존 코드 동일 함수 사용)
                    ImageBuffer roi = this.VisionManager().ReduceImageSize(
                                            frame,
                                            patternparam.LocationX,
                                            patternparam.LocationY,
                                            patternparam.Width,
                                            patternparam.Height);

                    long sum = 0;
                    int count = roi.SizeX * roi.SizeY;

                    // ROI 내 픽셀 값을 더해서 평균 내는 부분 (평균 밝기)
                    for (int i = 0; i < count; i++)
                    {
                        sum += roi.Buffer[i];
                    }

                    double mean = (double)sum / count;

                    // Edge 도달 (밝은 위치 -> 어두운 위치)
                    if (mean < threshold)
                    {
                        LoggerManager.PinLog("Wafer Y edge detected.");
                        break;
                    }

                    // Y축 이동 (기존 코드 Motion 구조 사용)
                    this.StageSupervisor().StageModuleState.StageRelMove(yaxis, moveoffset);

                    limitCount++;
                    if (limitCount > 20)
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                        limitCount = 0;
                        LoggerManager.PinLog("Die Edge Move Y Fail");
                        return retVal;
                    }
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                // 되돌아가서 다시 영역 안으로 들어가게 한 다음
                this.StageSupervisor().StageModuleState.StageRelMove(yaxis, -(moveoffset * moveBackoffset));

                while (true)    // X축 이동
                {
                    // Frame Grab (기존 코드와 동일)
                    ImageBuffer frame = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                    if (frame == null)
                        return retVal;

                    // ROI 생성 (기존 코드 동일 함수 사용)
                    ImageBuffer roi = this.VisionManager().ReduceImageSize(
                                            frame,
                                            patternparam.LocationX,
                                            patternparam.LocationY,
                                            patternparam.Width,
                                            patternparam.Height);

                    long sum = 0;
                    int count = roi.SizeX * roi.SizeY;

                    // ROI 내 픽셀 값을 더해서 평균 내는 부분 (평균 밝기)
                    for (int i = 0; i < count; i++)
                    {
                        sum += roi.Buffer[i];
                    }

                    double mean = (double)sum / count;

                    // Edge 도달 (밝은 위치 -> 어두운 위치)
                    if (mean < threshold)
                    {
                        LoggerManager.PinLog("Wafer X edge detected.");

                        // 되돌아간 Y 원상복귀
                        this.StageSupervisor().StageModuleState.StageRelMove(yaxis, moveoffset * (moveBackoffset / 2));

                        break;
                    }

                    // Y축 이동 (기존 코드 Motion 구조 사용)
                    this.StageSupervisor().StageModuleState.StageRelMove(xaxis, moveoffset);

                    limitCount++;
                    if (limitCount > 20)
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                        limitCount = 0;
                        LoggerManager.PinLog("Die Edge Move X Fail");
                        return retVal;
                    }
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                retVal = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return retVal;
        }
        public EventCodeEnum DieJump(double xpos, double ypos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ProbeAxisObject xaxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                double curXpos = xaxis.Status.Position.Ref;
                double curYpos = yaxis.Status.Position.Ref;

                retVal = this.MotionManager.StageMove(curXpos + xpos, curYpos + ypos);
            }
            catch
            {

            }

            return retVal;
        }

        class PosSave
        {
            public double xpos;
            public double ypos;
            public double fdzpos;
            public double cpos;
        }
        Dictionary<int, PosSave> data = new Dictionary<int, PosSave>();

        public void DiePosAdd(int key)
        {
            //ProbeAxisObject xaxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
            //ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
            //ProbeAxisObject FDZ1axis = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);
            //ProbeAxisObject FDT1axis = this.MotionManager().GetAxis(EnumAxisConstants.FDT1);

            double xpos = 0;
            double ypos = 0;
            double FDZpos = 0;
            double FDT1pos = 0;

            this.MotionManager().GetRefPos(EnumAxisConstants.X, ref xpos);
            this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref ypos);
            this.MotionManager().GetRefPos(EnumAxisConstants.FDZ1, ref FDZpos);
            this.MotionManager().GetRefPos(EnumAxisConstants.FDT1, ref FDT1pos);

            data[key] = new PosSave { xpos = xpos, ypos = ypos, fdzpos = FDZpos, cpos = FDT1pos };

            // Wafer.GetSubsInfo().~    <- 이쪽에 넣어야할 수 있음
        }

        private async Task<EventCodeEnum> MoveDie1()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Camera 확인.
                if (CurCam.GetChannelType() != LowStandardParam_Clone.CamType)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Pattern Register Error.", "To register the Low pattern, please view the screen with Low camera and register again.", EnumMessageStyle.Affirmative);

                    return retVal;
                }

                ProbeAxisObject FDT1axis = this.MotionManager().GetAxis(EnumAxisConstants.FDT1);
                double cpos = data[0].cpos;

                FDT1axis.Status.RawPosition.Ref = cpos;
                FDT1axis.Status.Position.Ref = cpos;

                // z축과 c축 값전달 주의할 것
                //this.MotionManager.StageMove(data[0].xpos, data[0].ypos);
                //this.MotionManager.StageMove(data[0].xpos, data[0].ypos, data[0].fdzpos);
            }
            catch
            {

            }
            return retVal;
        }
        public void ConvertDataToWafer()
        {
            var target = Wafer.GetSubsInfo();

            if (target.DiePosList == null)
                target.DiePosList = new Dictionary<int, DiePosElement>();

            foreach (var item in data)
            {
                int key = item.Key;
                var src = item.Value;

                var dest = new DiePosElement();
                dest.SetX(src.xpos);
                dest.SetY(src.ypos);
                dest.SetFDZ(src.fdzpos);
                dest.SetC(src.cpos);

                target.DiePosList[key] = dest;
            }
        }
        // -->

        private async Task<EventCodeEnum> RegistPattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool isFindJumpIndex = true;
                _OperCancelTokenSource = new CancellationTokenSource();

                WaferCoordinate wcd;
                if (WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)  // 260126 sebas : FD/Wafer 로우구분
                {
                    wcd = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert_Wafer();
                }
                else
                {
                    wcd = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                }

                //Camera 확인.    // 260121 sebas : 카메라 겸용하기 때문에 체크 제거
                //if (CurCam.GetChannelType() != LowStandardParam_Clone.CamType)
                //{
                //    await this.MetroDialogManager().ShowMessageDialog("Pattern Register Error.", "To register the Low pattern, please view the screen with Low camera and register again.", EnumMessageStyle.Affirmative);

                //    return retVal;
                //}

                ///PatternSize
                RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();
                if (patternparam.Width == 0 || patternparam.Height == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.", "Check the pattern size.", EnumMessageStyle.Affirmative);

                    return EventCodeEnum.PATTERN_SIZE_ERROR;
                }

                ///PatternInfomation
                WAStandardPTInfomation patterninfo = new WAStandardPTInfomation();

                patterninfo.CamType.Value = CurCam.GetChannelType();
                patterninfo.LightParams = new ObservableCollection<LightValueParam>();

                LowStandardParam_Clone.DefaultPMParam.CopyTo(patterninfo.PMParameter);

                int ptindex = patternindex;

                patterninfo.PMParameter.ModelFilePath.Value = LowStandardParam_Clone.PatternbasePath + LowStandardParam_Clone.PatternName + LowStandardParam_Clone.Patterns.Value.Count;
                patterninfo.PMParameter.PatternFileExtension.Value = ".mmo";

                ImageBuffer curImage = this.VisionManager().SingleGrab(patterninfo.CamType.Value, this);
                patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(curImage, patternparam.LocationX, patternparam.LocationY, patternparam.Width, patternparam.Height);

                this.VisionManager().GetGrayValue(ref curImage);
                patterninfo.GrayLevel = curImage.GrayLevelValue;

                patterninfo.ProcDirection.Value = EnumWAProcDirection.HORIZONTAL;
                patterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;

                if (_OperCancelTokenSource.Token.IsCancellationRequested)
                {
                    return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                }

                //..[Trun Theta 0 ] 
                double curtpos = 0.0;
                ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                curtpos = Math.Abs(curtpos);

                int converttpos = Convert.ToInt32(curtpos);

                if (converttpos != 0)
                {
                    if (_OperCancelTokenSource.Token.IsCancellationRequested)
                    {
                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }

                    // 260127 sebas : 시작전 C 초기화 구분
                    if(WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)
                    {
                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0, EnumTrjType.Normal, -1);
                        int ret = this.MotionManager().WaitForAxisMotionDone(axist, 60000);
                    }
                    else
                    {
                        // FD축의 경우 패스
                        //retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                        //int ret = this.MotionManager().WaitForAxisMotionDone(axist, 60000);
                    }
                }

                this.WaferAligner().WaferAlignInfo.AlignAngle = 0;

                //===============================================================================

                double registareaXlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) - Math.Pow(wcd.GetY(), 2));

                double registareaYlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) - Math.Pow(OptimumLength, 2));

                if (wcd.X.Value > -registareaXlimit && wcd.X.Value < registareaXlimit &&
                    wcd.Y.Value > -registareaYlimit && wcd.Y.Value < registareaYlimit)
                {

                    if (_OperCancelTokenSource.Token.IsCancellationRequested)
                    {
                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }

                    patterninfo.WaferCenter = new WaferCoordinate();

                    if (this.WaferAligner().WaferAlignInfo.PTWaferCenter != null)
                    {
                        if (this.GetState() != SubModuleStateEnum.RECOVERY & this.GetState() != SubModuleStateEnum.DONE)
                        {
                            this.WaferAligner().WaferAlignInfo.PTWaferCenter.CopyTo(patterninfo.WaferCenter);
                        }
                        else
                        {
                            this.WaferObject.GetSubsInfo().WaferCenter.CopyTo(patterninfo.WaferCenter);
                        }
                    }
                    else
                    {
                        this.WaferAligner().WaferAlignInfo.PTWaferCenter = new WaferCoordinate();
                        Wafer.GetSubsInfo().WaferCenter.CopyTo(this.WaferAligner().WaferAlignInfo.PTWaferCenter);
                        this.WaferAligner().WaferAlignInfo.PTWaferCenter.CopyTo(patterninfo.WaferCenter);
                    }

                    patterninfo.X.Value = (wcd.X.Value - patterninfo.WaferCenter.GetX());
                    patterninfo.Y.Value = (wcd.Y.Value - patterninfo.WaferCenter.GetY());

                    if (_OperCancelTokenSource.Token.IsCancellationRequested)
                    {
                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }
 
                    retVal = ValidationTesting(patterninfo, FocusingModule, FocusingParam);
                    //VaildationTesting 매개변수 변경할것.

                    if (retVal != EventCodeEnum.NONE)
                    {
                        if (_OperCancelTokenSource.Token.IsCancellationRequested)
                        {
                            return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                        }

                        await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.", "Pattern validation failed. Please register another pattern.", EnumMessageStyle.AffirmativeAndNegative);

                        if (LowStandardParam_Clone.Patterns.Value.Count == 0)
                        {
                            CurPatternIndex = 0;
                        }

                        return retVal;
                    }
                    else
                    {
                        if(WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)
                        {
                            wcd = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert_Wafer();
                        }
                        else
                        {
                            wcd = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                        }

                        patterninfo.Z.Value = (wcd.Z.Value - patterninfo.WaferCenter.GetZ());
                        patterninfo.ProcDirection.Value = EnumWAProcDirection.HORIZONTAL;

                        for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                        {
                            patterninfo.LightParams.Add(new LightValueParam(CurCam.LightsChannels[lightindex].Type.Value, (ushort)CurCam.GetLight(CurCam.LightsChannels[lightindex].Type.Value)));
                        }

                        procresults = new List<WaferProcResult>();

                        if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                        {
                            if (ConfirmExistManualJumpIndex.Confirm(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL) != -1)
                            {
                                if (patterninfo.JumpIndexs == null)
                                {
                                    patterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                                }

                                patterninfo.ProcDirection.Value = EnumWAProcDirection.HORIZONTAL;
                                patterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;

                                patterninfo.JumpIndexs = ConfirmExistManualJumpIndex.FindJumpIndex(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL, patterninfo.JumpIndexs);

                                isFindJumpIndex = false;
                            }
                            else
                            {
                                foreach (var item in LowStandardParam_Clone.Patterns.Value)
                                {
                                    int retindex = item.JumpIndexs.ToList<StandardJumpIndexParam>().FindIndex(index => index.Index.XIndex != -1);

                                    if (retindex >= 0)
                                    {
                                        if (patterninfo.JumpIndexs == null)
                                        {
                                            patterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                                        }

                                        for (int index = 0; index < item.JumpIndexs.Count; index++)
                                        {
                                            patterninfo.JumpIndexs.Add(item.JumpIndexs[index]);
                                        }

                                        isFindJumpIndex = false;

                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ConfirmExistManualJumpIndex.Confirm(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL) != -1)
                            {
                                if (patterninfo.JumpIndexs == null)
                                {
                                    patterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                                }

                                patterninfo.JumpIndexs = ConfirmExistManualJumpIndex.FindJumpIndex(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL, patterninfo.JumpIndexs);

                                retVal = ThetaAlign(ref patterninfo, null, ref RotateAngle);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    isFindJumpIndex = true;
                                    patterninfo.ErrorCode = retVal;
                                }
                                else
                                {
                                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("", Properties.Resources.JumpIndexThetaAlignErrorMessage, EnumMessageStyle.AffirmativeAndNegative);

                                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                                    {
                                        isFindJumpIndex = true;
                                    }
                                    else
                                    {
                                        return retVal;
                                    }
                                }
                            }
                        }

                        if (isFindJumpIndex)
                        {
                            switch (Wafer.GetPhysInfo().WaferSize_um.Value)
                            {
                                case 150000:
                                    MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (1.0 / 2.0);
                                    MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (1.0 / 2.0);
                                    break;
                                case 200000:
                                    MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (2.0 / 3.0);
                                    MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (2.0 / 3.0);
                                    break;
                                case 300000:
                                    MinimumLength = LowStandardParam_Clone.AlignMinimumLength;
                                    MaximumLength = LowStandardParam_Clone.AlignMaximumLength;
                                    break;
                            }

                            patterninfo = ThetaAlign(patterninfo, ref RotateAngle, _OperCancelTokenSource, MinimumLength, OptimumLength, MaximumLength, true, null, false, this.WaferAligner().IsNewSetup);
                        }
                        else
                        {
                            retVal = ThetaAlign(ref patterninfo, null, ref RotateAngle, false);
                        }

                        if (patterninfo != null & patterninfo.ErrorCode == EventCodeEnum.NONE & retVal == EventCodeEnum.NONE)
                        {
                            if (patterninfo.ErrorCode == EventCodeEnum.NONE)
                            {
                                if (patterninfo.PatternState.Value == PatternStateEnum.READY)
                                {
                                    foreach (var result in procresults)
                                    {
                                        this.WaferAligner().WaferAlignInfo.AlignProcResult.Add(result);
                                    }

                                    // 260126 sebas : 마지막 센터로 이동 구분1    => ***** 개조 후 위치 변경 필요
                                    if(WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)
                                    {
                                        //StageSupervisor.StageModuleState.WaferLowViewMove_Wafer(patterninfo.GetX() + Wafer.GetSubsInfo().WaferCenter_WF.GetX(), patterninfo.GetY() + Wafer.GetSubsInfo().WaferCenter_WF.GetY(), patterninfo.GetZ() + Wafer.GetSubsInfo().WaferCenter_WF.GetZ(), false, EnumTrjType.Normal, -1);
                                        StageSupervisor.StageModuleState.WaferLowViewMove_Wafer(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ(), false, EnumTrjType.Normal, -1);
                                    }
                                    else
                                    {
                                        this.StageSupervisor().StageModuleState.WaferLowViewMove(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());
                                    }

                                    PMResult pmresult = this.VisionManager().PatternMatching(patterninfo, this);

                                    this.VisionManager().StartGrab(patterninfo.CamType.Value, this);

                                    retVal = pmresult.RetValue;

                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);

                                        patterninfo.X.Value = wcoord.GetX() - patterninfo.WaferCenter.GetX();
                                        patterninfo.Y.Value = wcoord.GetY() - patterninfo.WaferCenter.GetY();
                                        patterninfo.Z.Value = wcoord.GetZ() - patterninfo.WaferCenter.GetZ();

                                        // 260127 sebas : 마지막 센터로 이동 구분2    => ***** 개조 후 위치 변경 필요
                                        if (WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)
                                        {
                                            StageSupervisor.StageModuleState.WaferLowViewMove_Wafer(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ(), false, EnumTrjType.Normal, -1);
                                        }
                                        else
                                        {
                                            this.StageSupervisor().StageModuleState.WaferLowViewMove(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());
                                        }
                                    }
                                    else
                                    {
                                        // Nothing
                                    }
                                }

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    patterninfo.MIndex = this.CoordinateManager().GetCurMachineIndex(new WaferCoordinate(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ()));

                                    foreach (var info in patterninfo.JumpIndexs)
                                    {
                                        info.AcceptFocusing.Value = false;
                                    }

                                    ImageBuffer img = this.VisionManager().SingleGrab(patterninfo.CamType.Value, this);

                                    patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(img, (img.SizeX / 2 - (patternparam.Width / 2)), (img.SizeY / 2 - (patternparam.Height / 2)), patternparam.Width, patternparam.Height);

                                    if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                                    {
                                        patterninfo.PatternState.Value = PatternStateEnum.READY;
                                    }
                                    else
                                    {
                                        patterninfo.PatternState.Value = PatternStateEnum.MODIFY;
                                    }

                                    LowStandardParam_Clone.Patterns.Value.Add(patterninfo);
                                    CurPatternIndex = LowStandardParam_Clone.Patterns.Value.Count;
                                    if (!this.WaferAligner().IsNewSetup)
                                    {
                                        SubModuleState = new SubModuleDoneState(this);
                                    }
                                }
                                else
                                {
                                    MachineCoordinate centerPoint = new MachineCoordinate(patterninfo.WaferCenter.GetX(), patterninfo.WaferCenter.GetY());
                                    MachineCoordinate pivotPoint = new MachineCoordinate(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY());
                                    MachineCoordinate wcoord = this.CoordinateManager().GetRotatedPoint(pivotPoint, centerPoint, RotateAngle);

                                    patterninfo.X.Value = wcoord.GetX() - patterninfo.WaferCenter.GetX();
                                    patterninfo.Y.Value = wcoord.GetY() - patterninfo.WaferCenter.GetY();

                                    // 260127 sebas : 마지막 센터로 이동 구분3    => ***** 개조 후 위치 변경 필요
                                    if (WAEdgeStadnardModule.EdgeStandard.IsWaferEdge == true)
                                    {
                                        //retVal = StageSupervisor.StageModuleState.WaferLowViewMove_Wafer(patterninfo.GetX() + Wafer.GetSubsInfo().WaferCenter_WF.GetX(), patterninfo.GetY() + Wafer.GetSubsInfo().WaferCenter_WF.GetY(), patterninfo.GetZ() + Wafer.GetSubsInfo().WaferCenter_WF.GetZ(), false, EnumTrjType.Normal, -1);
                                        retVal = StageSupervisor.StageModuleState.WaferLowViewMove_Wafer(Wafer.GetSubsInfo().WaferCenter_WF.GetX(),Wafer.GetSubsInfo().WaferCenter_WF.GetY(), patterninfo.GetZ() + Wafer.GetSubsInfo().WaferCenter_WF.GetZ(), false, EnumTrjType.Normal, -1);
                                    }
                                    else
                                    {
                                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(patterninfo.GetX() + patterninfo.WaferCenter.GetX(), patterninfo.GetY() + patterninfo.WaferCenter.GetY(), patterninfo.GetZ() + patterninfo.WaferCenter.GetZ());
                                    }

                                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Low Pattern Register Error", "This pattern is not valid for Align. Please register the pattern in another location.", EnumMessageStyle.Affirmative);
                                }
                            }

                        }
                        else
                        {
                            if (patterninfo.ErrorCode == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                            {
                                return patterninfo.ErrorCode;
                            }

                            EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Low Pattern Register Error", "This pattern is not valid for Align. Please register the pattern in another location.", EnumMessageStyle.Affirmative);

                            this.StageSupervisor().StageModuleState.WaferLowViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());
                        }
                    }
                }
                else
                {
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Align is not a good place to be..", "Please register your padan in another location.", EnumMessageStyle.Affirmative);

                    this.StageSupervisor().StageModuleState.WaferLowViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.RegistPattern() : Error occured.");
                throw err;
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                _OperCancelTokenSource = null;
            }

            return retVal;

        }

        //Recovery Setup 시 임시 데이터로 저장, 등록하기 위한 함수
        private async Task<EventCodeEnum> RecoverySetup_RegistPattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool isFindJumpIndex = true;
                _OperCancelTokenSource = new CancellationTokenSource();
                WaferCoordinate wcd = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

                ///Camera 확인.
                if (CurCam.GetChannelType() != LowStandardParam_Clone.CamType)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Pattern Register Error.",
                        "To register the Low pattern, please view the screen with Low camera and register again.",
                        EnumMessageStyle.Affirmative);
                    return retVal;
                }

                ///Modify Setup 이라면 WaferAlign Done인지 확인하고 수행.

                ///PatternSize
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
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                ///PatternInfomation
                temppatterninfo.CamType.Value = CurCam.GetChannelType();
                temppatterninfo.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();

                LowStandardParam_Clone.DefaultPMParam.CopyTo(temppatterninfo.PMParameter);
                int ptindex = (LowStandardParam_Clone.Patterns.Value.Count + this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count);
                temppatterninfo.PMParameter.ModelFilePath.Value = LowStandardParam_Clone.PatternbasePath
                    + LowStandardParam_Clone.PatternName + ptindex.ToString();
                temppatterninfo.PMParameter.PatternFileExtension.Value = ".mmo";
                temppatterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(this.VisionManager().SingleGrab(temppatterninfo.CamType.Value, this), patternparam.LocationX, patternparam.LocationY, patternparam.Width, patternparam.Height);

                temppatterninfo.ProcDirection.Value = EnumWAProcDirection.HORIZONTAL;
                temppatterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;

                if (_OperCancelTokenSource.Token.IsCancellationRequested)
                {
                    return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                }

                //..[Trun Theta 0 ] 
                double curtpos = 0.0;
                ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                curtpos = Math.Abs(curtpos);

                int converttpos = Convert.ToInt32(curtpos);

                if (converttpos != 0)
                {
                    if (_OperCancelTokenSource.Token.IsCancellationRequested)
                    {
                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }

                    retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                    int ret = this.MotionManager().WaitForAxisMotionDone(axist, 60000);

                }
                this.WaferAligner().WaferAlignInfo.AlignAngle = 0;


                //===============================================================================

                double registareaXlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) -
                    Math.Pow(wcd.GetY(), 2));

                double registareaYlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value), 2) -
                    Math.Pow(OptimumLength, 2));

                if (wcd.X.Value > -registareaXlimit && wcd.X.Value < registareaXlimit
                    && wcd.Y.Value > -registareaYlimit && wcd.Y.Value < registareaYlimit)
                {

                    if (_OperCancelTokenSource.Token.IsCancellationRequested)
                    {
                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }

                    temppatterninfo.WaferCenter = new WaferCoordinate();

                    //if (this.WaferAligner().WaferAlignInfo.PTWaferCenter != null)
                    //{
                    this.WaferObject.GetSubsInfo().WaferCenter.CopyTo(temppatterninfo.WaferCenter);
                    //}
                    //else
                    //{
                    //    this.WaferAligner().WaferAlignInfo.PTWaferCenter = new WaferCoordinate();
                    //    Wafer.GetSubsInfo().WaferCenter.CopyTo(this.WaferAligner().WaferAlignInfo.PTWaferCenter);
                    //    this.WaferAligner().WaferAlignInfo.PTWaferCenter.CopyTo(temppatterninfo.WaferCenter);
                    //}


                    temppatterninfo.X.Value = (wcd.X.Value - temppatterninfo.WaferCenter.GetX());
                    temppatterninfo.Y.Value = (wcd.Y.Value - temppatterninfo.WaferCenter.GetY());

                    if (_OperCancelTokenSource.Token.IsCancellationRequested)
                    {
                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }

                    retVal = ValidationTesting(temppatterninfo, FocusingModule, FocusingParam);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        if (_OperCancelTokenSource.Token.IsCancellationRequested)
                        {
                            return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                        }

                        await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.",
                            "Pattern validation failed. Please register another pattern."
                             , EnumMessageStyle.AffirmativeAndNegative
                             );
                        if (LowStandardParam_Clone.Patterns.Value.Count == 0)
                            CurPatternIndex = 0;
                        return retVal;
                    }
                    else
                    {
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                        temppatterninfo.Z.Value = (wcd.Z.Value - Wafer.GetSubsInfo().WaferCenter.GetZ());

                        temppatterninfo.ProcDirection.Value = EnumWAProcDirection.HORIZONTAL;
                        for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                        {
                            temppatterninfo.LightParams.Add(
                                new LightValueParam(CurCam.LightsChannels[lightindex].Type.Value,
                                (ushort)CurCam.GetLight(CurCam.LightsChannels[lightindex].Type.Value)));
                        }


                        procresults = new List<WaferProcResult>();


                        if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                        {
                            if (ConfirmExistManualJumpIndex.Confirm(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL) != -1)
                            {
                                if (temppatterninfo.JumpIndexs == null)
                                    temppatterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();


                                temppatterninfo.ProcDirection.Value = EnumWAProcDirection.HORIZONTAL;
                                temppatterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;

                                temppatterninfo.JumpIndexs =
                                    ConfirmExistManualJumpIndex.FindJumpIndex(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL, temppatterninfo.JumpIndexs);

                                isFindJumpIndex = false;
                            }
                            else
                            {
                                foreach (var item in LowStandardParam_Clone.Patterns.Value)
                                {
                                    int retindex = item.JumpIndexs.ToList<StandardJumpIndexParam>().FindIndex(
                                        index => index.Index.XIndex != -1);

                                    if (retindex >= 0)
                                    {
                                        if (temppatterninfo.JumpIndexs == null)
                                            temppatterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();

                                        for (int index = 0; index < item.JumpIndexs.Count; index++)
                                        {
                                            temppatterninfo.JumpIndexs.Add(item.JumpIndexs[index]);
                                        }
                                        isFindJumpIndex = false;
                                        break;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (ConfirmExistManualJumpIndex.Confirm(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL) != -1)
                            {
                                if (temppatterninfo.JumpIndexs == null)
                                    temppatterninfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();

                                temppatterninfo.JumpIndexs =
                                    ConfirmExistManualJumpIndex.FindJumpIndex(LowStandardParam_Clone.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL, temppatterninfo.JumpIndexs);
                                retVal = ThetaAlign(ref temppatterninfo, null, ref RotateAngle);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    isFindJumpIndex = true;
                                    temppatterninfo.ErrorCode = retVal;
                                }
                                else
                                {
                                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                                        "", Properties.Resources.JumpIndexThetaAlignErrorMessage, EnumMessageStyle.AffirmativeAndNegative);
                                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                                        isFindJumpIndex = true;
                                    else
                                        return retVal;
                                }
                            }
                        }

                        if (isFindJumpIndex)
                        {
                            switch (Wafer.GetPhysInfo().WaferSize_um.Value)
                            {
                                case 150000:
                                    MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (1.0 / 2.0);
                                    MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (1.0 / 2.0);
                                    break;
                                case 200000:
                                    MinimumLength = LowStandardParam_Clone.AlignMinimumLength * (2.0 / 3.0);
                                    MaximumLength = LowStandardParam_Clone.AlignMaximumLength * (2.0 / 3.0);
                                    break;
                                case 300000:
                                    MinimumLength = LowStandardParam_Clone.AlignMinimumLength;
                                    MaximumLength = LowStandardParam_Clone.AlignMaximumLength;
                                    break;
                            }


                            temppatterninfo = ThetaAlign(temppatterninfo, ref RotateAngle, _OperCancelTokenSource,
                                 MinimumLength, OptimumLength, MaximumLength, true, null, false, true);
                        }
                        else
                        {
                            retVal = ThetaAlign(ref temppatterninfo, null, ref RotateAngle, true);
                        }

                        if (temppatterninfo != null & temppatterninfo.ErrorCode == EventCodeEnum.NONE & retVal == EventCodeEnum.NONE)
                        {
                            if (temppatterninfo.ErrorCode == EventCodeEnum.NONE)
                            {
                                if (temppatterninfo.PatternState.Value == PatternStateEnum.READY)
                                {

                                    foreach (var result in procresults)
                                    {
                                        this.WaferAligner().WaferAlignInfo.AlignProcResult.Add(result);
                                    }

                                    this.StageSupervisor().StageModuleState.WaferLowViewMove(temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX(), temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY(), temppatterninfo.GetZ() + temppatterninfo.WaferCenter.GetZ());

                                    PMResult pmresult = this.VisionManager().PatternMatching(temppatterninfo, this);

                                    this.VisionManager().StartGrab(temppatterninfo.CamType.Value, this);

                                    retVal = pmresult.RetValue;

                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);

                                        temppatterninfo.X.Value = wcoord.GetX() - temppatterninfo.WaferCenter.GetX();
                                        temppatterninfo.Y.Value = wcoord.GetY() - temppatterninfo.WaferCenter.GetY();
                                        temppatterninfo.Z.Value = wcoord.GetZ() - temppatterninfo.WaferCenter.GetZ();

                                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
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

                                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(
                                            temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX(),
                                            temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY(),
                                            temppatterninfo.GetZ() + temppatterninfo.WaferCenter.GetZ());

                                    }
                                }
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    temppatterninfo.MIndex = this.CoordinateManager().GetCurMachineIndex
                                        (new WaferCoordinate(temppatterninfo.GetX() + temppatterninfo.WaferCenter.GetX(),
                                            temppatterninfo.GetY() + temppatterninfo.WaferCenter.GetY(),
                                            temppatterninfo.GetZ() + temppatterninfo.WaferCenter.GetZ()));

                                    foreach (var info in temppatterninfo.JumpIndexs)
                                    {
                                        info.AcceptFocusing.Value = false;
                                    }

                                    ImageBuffer img = this.VisionManager().SingleGrab(temppatterninfo.CamType.Value, this);

                                    temppatterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(
                                       img, (img.SizeX / 2 - (patternparam.Width / 2)), (img.SizeY / 2 - (patternparam.Height / 2)),
                                       patternparam.Width, patternparam.Height);


                                    //patterninfo.PatternState.Value = PatternStateEnum.READY;
                                    if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                                        temppatterninfo.PatternState.Value = PatternStateEnum.READY;
                                    else
                                        temppatterninfo.PatternState.Value = PatternStateEnum.MODIFY;

                                    this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Add(temppatterninfo);
                                    CurTempPatternIndex = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count;
                                    temppatternCount = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count;

                                    SetNodeSetupRecoveryState(EnumMoudleSetupState.VERIFY);
                                    //if (!this.WaferAligner().IsNewSetup)
                                    //    SubModuleState = new SubModuleDoneState(this);
                                }
                                else
                                {
                                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Low Pattern Register Error",
                                   "This pattern is not valid for Align. Please register the pattern in another location.",
                                   EnumMessageStyle.Affirmative);

                                    this.StageSupervisor().StageModuleState.WaferLowViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());
                                }
                            }

                        }
                        else
                        {
                            if (temppatterninfo.ErrorCode == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                                return temppatterninfo.ErrorCode;

                            EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Low Pattern Register Error",
                              "This pattern is not valid for Align. Please register the pattern in another location.",
                              EnumMessageStyle.Affirmative);

                            this.StageSupervisor().StageModuleState.WaferLowViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());
                        }
                    }

                }
                else
                {
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Align is not a good place to be..",
                      "Please register your padan in another location.",
                      EnumMessageStyle.Affirmative);

                    this.StageSupervisor().StageModuleState.WaferLowViewMove(wcd.GetX(), wcd.GetY(), wcd.GetZ());
                    //Message
                    //Align 하기에 적절하지 않은 위치입니다.다른 위치에 패던을 등록해 주시기 바랍니다
                    //Align is not a good place to be.. Please register your padan in another location.
                }
                //if (retVal == EventCodeEnum.NONE)
                //{
                //    SubModuleState = new SubModuleSkipState(this);
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.RegistPattern() : Error occured.");
                throw err;
            }
            finally
            {
                CurPatternIndex = 0;
                StepLabel = String.Format("REGISTED PATTERN {0}/{1}", CurPatternIndex, patternCount);
                StepSecondLabel = String.Format("TEMPORARY PATTERN {0}/{1}", CurTempPatternIndex, temppatternCount);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

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
        private Task<EventCodeEnum> RecoverySetup_DeletePattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count > 0)
                {
                    int removeindex = CurTempPatternIndex - 1;
                    if (CurTempPatternIndex == 0)
                    {
                        removeindex = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count - 1;
                    }

                    this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.RemoveAt(removeindex);
                }

                CurTempPatternIndex = 0;
                temppatternCount = this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count;

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
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        private EventCodeEnum DeletePattern()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (LowStandardParam_Clone.Patterns.Value.Count > 0)
                {
                    int precount = LowStandardParam_Clone.Patterns.Value.Count;
                    if (CurPatternIndex != 0)
                        LowStandardParam_Clone.Patterns.Value.RemoveAt(CurPatternIndex - 1);
                    else
                        LowStandardParam_Clone.Patterns.Value.RemoveAt(CurPatternIndex);
                    if (LowStandardParam_Clone.Patterns.Value.Count == precount - 1 & CurPatternIndex != 1)
                        CurPatternIndex--;

                    if (LowStandardParam_Clone.Patterns.Value.Count == 0)
                        CurPatternIndex = 0;

                    if (CurPatternIndex == 0)
                    {
                        this.WaferAligner().WaferAlignInfo.PTWaferCenter = null;
                    }
                }
                else
                {
                    CurPatternIndex = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }


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
                        if ((LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value.Count == 0)
                            msg = "Not Exist Pattern(Wafer Low Align).";
                        else if ((LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                            item => item.PatternState.Value == PatternStateEnum.FAILED).Count
                            != (LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value.Count)
                            msg = "Not Exist Pattern that can be Align(Wafer Low Align).";
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
            try
            {
                if ((LowStandard_IParam as WA_LowMagParam_Standard) != null)
                {
                    WA_LowMagParam_Standard param = (LowStandard_IParam as WA_LowMagParam_Standard);
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return SubModuleState.ClearData();
        }


        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    if ((LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value.Count != 0 &&
                    (LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(item => item.PatternState.Value == PatternStateEnum.FAILED).Count != (LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value.Count)
                    {
                        retVal = Extensions_IParam.ElementStateNeedSetupValidation(LowStandard_IParam);
                    }
                }
                else
                {
                    if ((LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value.Count != 0)
                    {
                        retVal = Extensions_IParam.ElementStateNeedSetupValidation(LowStandard_IParam);
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

        public override EventCodeEnum ClearSettingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //LowStandardParam_Clone = LowStandard_IParam.Copy() as WA_LowMagParam_Standard;
                LowStandardParam_Clone = LowStandard_IParam as WA_LowMagParam_Standard;

                ParamValidation();
                if (this.WaferAligner().IsNewSetup)
                {
                    //clear pattern image
                    string targetDirectory = this.FileManager().GetDeviceParamFullPath() + LowStandardParam_Clone.PatternbasePath;
                    this.FileManager().DeleteFilesInDirectory(targetDirectory);

                    LowStandardParam_Clone.Patterns.Value.Clear();
                    this.WaferAligner().WaferAlignInfo.PTWaferCenter = null;

                    //Clear Manual JumpIndex Setting.
                    LowStandardParam_Clone.JumpIndexManualInputParam.LeftIndex = 0;
                    LowStandardParam_Clone.JumpIndexManualInputParam.RightIndex = 0;
                }
                //(AdvanceSetupView as LowMagStandardInputControl).SettingData(LowStandardParam_Clone);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var pat in (LowStandard_IParam as WA_LowMagParam_Standard).Patterns.Value)
                {
                    pat.PatternState.Value = PatternStateEnum.READY;
                }
                //this.WaferAligner().WaferAlignInfo.AlignAngle = 0;
                SubModuleState = new SubModuleIdleState(this);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.DoClearData() : Error occured.");
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
                //this.PnPManager().SetSeletedStep(this);

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
                foreach (var ptinfo in LowStandardParam_Clone.Patterns.Value)
                {
                    if (ptinfo.PatternState.Value == PatternStateEnum.MODIFY)
                    {
                        //ptinfo.X.Value += Wafer.GetSubsInfo().WaferCenterOffset.GetX();
                        //ptinfo.Y.Value += Wafer.GetSubsInfo().WaferCenterOffset.GetY();
                        // ptinfo.X.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX;
                        // ptinfo.Y.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY;
                        ptinfo.PatternState.Value = PatternStateEnum.READY;
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                EventCodeEnum ret = Extensions_IParam.ElementStateDefaultValidation(LowStandardParam_Clone);
                if (ret == EventCodeEnum.NONE)
                    retVal = false;
                else
                    retVal = true;

                retVal = IsParamChanged | retVal;

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
                        if (LowStandardParam_Clone.Patterns.Value.Count != 0
                            && LowStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                        item => item.PatternState.Value == PatternStateEnum.FAILED).Count == 0)
                        {
                            SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                        }
                        else
                        {
                            SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                            //SetNextStepsNotCompleteState();
                            //Next steps change to notcompleted. 
                        }
                    }
                    else
                    {
                        if (this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Count > 0)
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
                                if (LowStandardParam_Clone.Patterns.Value.Count != 0
                                && LowStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                                    item => item.PatternState.Value == PatternStateEnum.FAILED).Count != LowStandardParam_Clone.Patterns.Value.Count)
                                    SetNodeSetupRecoveryState(EnumMoudleSetupState.COMPLETE);
                                else
                                    SetNodeSetupRecoveryState(EnumMoudleSetupState.NOTCOMPLETED);
                            }

                        }
                    }
                    else
                    {
                        if (LowStandardParam_Clone.Patterns.Value.Count != 0
                            && LowStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(
                                item => item.PatternState.Value == PatternStateEnum.FAILED).Count != LowStandardParam_Clone.Patterns.Value.Count)
                            SetNodeSetupRecoveryState(EnumMoudleSetupState.COMPLETE);
                        else
                            SetNodeSetupRecoveryState(EnumMoudleSetupState.NOTCOMPLETED);
                    }

                    //if (this.GetState() != SubModuleStateEnum.RECOVERY)
                    //{
                    //    if (LowStandardParam_Clone.Patterns.Value.Count != 0
                    //    && LowStandardParam_Clone.Patterns.Value.ToList<WAStandardPTInfomation>().FindAll(item => item.PatternState.Value == PatternStateEnum.FAILED).Count == 0)
                    //    {
                    //        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                    //    }
                    //    else
                    //    {
                    //        SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    //    }
                    //}
                    //else
                    //{
                    //    if (this.GetState() == SubModuleStateEnum.DONE)
                    //        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                    //    else
                    //        SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    //}
                }


                if (Parent != null)
                    Parent.SetStepSetupState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(LowStandardParam_Clone));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

//public EventCodeEnum UpdateRecoveryed()
//{
//    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//    try
//    {
//        foreach (var ptinfo in tempPtInfo)
//        {
//            ptinfo.X.Value += Wafer.SubsInfo.WaferCenterOffset.GetX();
//            ptinfo.Y.Value += Wafer.SubsInfo.WaferCenterOffset.GetY();

//            LowStandardParam.Patterns.Value.Add(ptinfo);
//        }
//        tempPtInfo.Clear();
//        retVal = SaveDevParameter();
//        // retVal = SubModuleState.SetRecoveryedSate();
//        //retVal = StateTransition(new LowRecoveryedState(this));
//    }
//    catch (Exception err)
//    {
//        LoggerManager.Debug($"{err.ToString()}.UpdataRecoveryData() : Error occured.");
//    }
//    return retVal;
//}

//private WaferCoordinate ChangedLocationFormPT(PMResult pmresult)
//{
//    WaferCoordinate wcd = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
//    if (pmresult.ResultParam.Count == 0)
//        return null;
//    double ptxpos = pmresult.ResultParam[0].XPoss;
//    double ptypos = pmresult.ResultParam[0].YPoss;

//    double offsetx = 0.0;
//    double offsety = 0.0;


//    if (CurCam.GetVerticalFlip() == FlipEnum.NONE)
//    {
//        offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
//    }
//    else
//    {
//        offsetx = ptxpos - (pmresult.ResultBuffer.SizeX / 2);
//    }

//    if (CurCam.GetHorizontalFlip() == FlipEnum.NONE)
//    {
//        offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
//    }
//    else
//    {
//        offsety = ptypos - (pmresult.ResultBuffer.SizeY / 2);

//    }

//    offsetx *= CurCam.GetRatioX();
//    offsety *= CurCam.GetRatioY();

//    return new WaferCoordinate((wcd.GetX() + offsetx), (wcd.GetY() + offsety), wcd.GetZ(), 1);
//}



