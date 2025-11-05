using LogModule;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using ProberInterfaces.Vision;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PMISetup
{
    public class PadAutoRegistrationModule : PMIPNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup
    {
        public override bool Initialized { get; set; } = false;
        public override Guid ScreenGUID { get; } = new Guid("467070CE-2C29-6610-9005-728C4DA1658D");

        private PMIDrawingGroup _DrawingGroup;
        public PMIDrawingGroup DrawingGroup
        {
            get { return _DrawingGroup; }
            set
            {
                if (value != _DrawingGroup)
                {
                    _DrawingGroup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }
        }

        public PadAutoRegistrationModule()
        {

        }
        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Parameter 확인한다.

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Modify 상태가 있는지 없는지를 확인해준다.
                EventCodeEnum retVal1 = Extensions_IParam.ElementStateDefaultValidation(this.PMIModule().PMIModuleDevParam_IParam);
                EventCodeEnum retVal2 = this.StageSupervisor().SaveWaferObject();

                if ((retVal1 == EventCodeEnum.NONE) && (retVal2 == EventCodeEnum.NONE))
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Update 상태가 있는지 없는지를 확인해준다.
                ///retVal = Extensions_IParam.ElementStateUpdateValidation(PMIDevParam);

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Apply 상태가 있는지 없는지를 확인해준다.
                ///retVal = Extensions_IParam.ElementStateApplyValidation(Param);

                //모듈의  Setup상태를 변경해준다.

                if (retVal == EventCodeEnum.NONE)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.UNDEFINED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [ParamValidation()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                EventCodeEnum retVal1 = Extensions_IParam.ElementStateDefaultValidation(this.PMIModule().PMIModuleDevParam_IParam);
                EventCodeEnum retVal2 = Extensions_IParam.ElementStateDefaultValidation(this.StageSupervisor().WaferObject.GetSubsInfo().Pads);

                if ((retVal1 == EventCodeEnum.NONE) && (retVal2 == EventCodeEnum.NONE))
                {
                    retVal = false;
                }
                else
                {
                    retVal = true;
                }


                if (retVal == false)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    //모듈의 Setup상태를 변경해준다.
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.UNDEFINED);
                }
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

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    DrawingGroup = new PMIDrawingGroup();
                    DrawingGroup.Template = true;
                    DrawingGroup.RegisterdPad = true;

                    SharpDXLayer = this.PMIModule().InitPMIRenderLayer(this.PMIModule().GetLayerSize(), 0, 0, 0, 0);
                    SharpDXLayer?.Init();

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
            }
            
            return retval;
        }
        
        public new void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [DeInitModule()] : {err}");
            }
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Pnp를 사용한다면 ListView에 띄워줄 이름을 기재해 주어야한다.
                Header = "Pad Auto Registration";

                retVal = InitPnpModuleStage();

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                curMode = 0;
                XDirectionSearch = true;
                FixXSizeFlag = false;
                FixYSizeFlag = false;

                OneButton.IconCaption = "LINEAR";
                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/direction-left.png");
                OneButton.Command = new RelayCommand(SelectOneDirModeCommand);

                TwoButton.IconCaption = "OUTLINE";
                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rotate-rect.png");
                TwoButton.Command = new RelayCommand(SelectFourSideCommand);

                ThreeButton.IconCaption = "ALL DIE";
                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/grid2.png");
                ThreeButton.Command = new RelayCommand(SelectAllDieAreaModeCommand);

                FourButton.IconCaption = "CURRENT";
                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/image-search.png");
                FourButton.Command = new RelayCommand(SelectCurScreenModeCommand);

                PadJogLeftUp.Caption = null;
                PadJogLeftUp.Command = null;
                PadJogLeftUp.IsEnabled = false;

                PadJogRightUp.Caption = null;
                PadJogRightUp.Command = null;
                PadJogRightUp.IsEnabled = false;

                PadJogLeftDown.Caption = "X";
                //PadJogLeftDown.IsSelected....
                PadJogLeftDown.Command = new AsyncCommand(SelectOneDirSearchModeCommand);

                PadJogRightDown.Caption = "Y";
                //PadJogRightDown.IsSelected....
                PadJogRightDown.Command = new AsyncCommand(SelectOneDirSearchModeCommand);

                PadJogSelect.IconCaption = "SCAN";
                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/feature-search_W.png");
                PadJogSelect.Command = new AsyncCommand(ScanPMIPadCommand);

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");

                PadJogLeft.Command = new RelayCommand(UCDisplayRectWidthMinus);
                PadJogRight.Command = new RelayCommand(UCDisplayRectWidthPlus);
                PadJogUp.Command = new RelayCommand(UCDisplayRectHeightPlus);
                PadJogDown.Command = new RelayCommand(UCDisplayRectHeightMinus);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;
                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Debug($"[PadAutoRegistrationModule] [InitViewModel()] : {err}");
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
                retVal = await InitSetup();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                this.StageSupervisor().WaferObject.DrawDieOverlay(CurCam);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Debug($"[PadAutoRegistrationModule] [PageSwitched()] : {err}");
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        /// <summary>
        /// Setup시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);

                InitLightJog(this);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                // MainView 화면에 Camera(Vision) 화면이 나온다.
                MainViewTarget = DisplayPort;
                // MiniView 화면에 WaferMap 화면이 나온다.
                MiniViewTarget = Wafer;
                // MiniView 화면에 Dut 화면이 나온다.
                //MiniViewTarget = ProbeCard;

                UseUserControl = UserControlFucEnum.DEFAULT;

                InitLightJog(this, EnumProberCam.WAFER_HIGH_CAM);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [InitSetup()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        #region ==> Properties..
        private int _curMode;
        public int curMode
        {
            get { return _curMode; }
            set
            {
                if (value != _curMode)
                {
                    _curMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _XDirectionSearch;
        public bool XDirectionSearch
        {
            get { return _XDirectionSearch; }
            set
            {
                if (value != _XDirectionSearch)
                {
                    _XDirectionSearch = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _FixXSizeFlag;
        public bool FixXSizeFlag
        {
            get { return _FixXSizeFlag; }
            set
            {
                if (value != _FixXSizeFlag)
                {
                    _FixXSizeFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _FixYSizeFlag;
        public bool FixYSizeFlag
        {
            get { return _FixYSizeFlag; }
            set
            {
                if (value != _FixYSizeFlag)
                {
                    _FixYSizeFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                this.StageSupervisor().WaferObject.StopDrawDieOberlay(CurCam);


                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PadAutoRegistrationModule] [Cleanup()] : Error");
                }

                //retVal = await base.Cleanup(parameter);

                //if (retVal != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Debug($"[PadAutoRegistrationModule] [Cleanup()] : Error");
                //}

                //if (IsParameterChanged() == true)
                //    retVal = await base.Cleanup(null);
                //else
                //    retVal = await base.Cleanup(parameter);

                //if (retVal == EventCodeEnum.SAVE_PARAM)
                //{
                //    retVal = SaveDevParameter();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [Cleanup()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region //..Command Method
        public void SelectOneDirModeCommand()
        {
            try
            {
                //.. Todo
                curMode = 0;

                PadJogLeftDown.Caption = "X";
                //PadJogLeftDown.IsSelected....
                PadJogLeftDown.Command = new AsyncCommand(SelectOneDirSearchModeCommand);

                PadJogRightDown.Caption = "Y";
                //PadJogRightDown.IsSelected....
                PadJogRightDown.Command = new AsyncCommand(SelectOneDirSearchModeCommand);

                //UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [SelectOneDirModeCommand()] : {err}");
            }
        }
               
        public void SelectFourSideCommand()
        {
            try
            {
                //.. Todo
                curMode = 1;

                PadJogLeftDown.Caption = "Fix X";
                //PadJogLeftDown.IsSelected....
                PadJogLeftDown.Command = new AsyncCommand(FixXSizeCommand);

                PadJogRightDown.Caption = "Fix Y";
                //PadJogRightDown.IsSelected....
                PadJogRightDown.Command = new AsyncCommand(FixYSizeCommand);

                //UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [SelectFourSideCommand()] : {err}");
            }
        }

        public void SelectAllDieAreaModeCommand()
        {
            try
            {
                //.. Todo
                curMode = 2;

                PadJogLeftDown.Caption = null;
                //PadJogLeftDown.IsSelected....
                PadJogLeftDown.Command = null;

                PadJogRightDown.Caption = null;
                //PadJogRightDown.IsSelected....
                PadJogRightDown.Command = null;

                //UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [SelectAllDieAreaModeCommand()] : {err}");
            }
        }
        
        public void SelectCurScreenModeCommand()
        {
            try
            {
                //.. Todo
                curMode = 3;

                PadJogLeftDown.Caption = null;
                //PadJogLeftDown.IsSelected....
                PadJogLeftDown.Command = null;

                PadJogRightDown.Caption = null;
                //PadJogRightDown.IsSelected....
                PadJogRightDown.Command = null;

                //UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [SelectCurScreenModeCommand()] : {err}");
            }
        }

        public Task SelectOneDirSearchModeCommand()
        {
            try
            {
                //.. Todo
                if (XDirectionSearch == true)
                {
                    XDirectionSearch = false;
                }
                else
                {
                    XDirectionSearch = true;
                }

                //UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [SelectOneDirSearchModeCommand()] : {err}");
            }
            return Task.CompletedTask;
        }

        public Task FixXSizeCommand()
        {
            try
            {
                //.. Todo
                FixXSizeFlag = true;
                FixYSizeFlag = false;
                //UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [FixXSizeCommand()] : {err}");
            }
            return Task.CompletedTask;
        }

        public Task FixYSizeCommand()
        {
            try
            {
                //.. Todo
                FixXSizeFlag = false;
                FixYSizeFlag = true;

                //UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [FixYSizeCommand()] : {err}");
            }
            return Task.CompletedTask;
        }

        public Task ScanPMIPadCommand()
        {
            try
            {

                var visionAlgorithmes = this.VisionManager().VisionProcessing.Algorithmes;
                IVisionManager visionManager = this.VisionManager();
                List<ImageBuffer> images = new List<ImageBuffer>();
                Point LLConer = new Point();
                Point LastPos = new Point();

                double widthSize = (CurCam.GetGrabSizeWidth() * CurCam.GetRatioX());
                double heightSize = (CurCam.GetGrabSizeHeight() * CurCam.GetRatioY());
                double widthHalfSize = widthSize / 2;
                double heightHalfSize = heightSize / 2;

                ////1단계 : 스타트 지점으로 이동.
                LLConer = this.WaferAligner().GetLeftCornerPosition(CurCam.CamSystemPos.GetX(), CurCam.CamSystemPos.GetY());
                //임시 높이.
                this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick = 606.2;

                //처음 시작지점으로 이동.
                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    double dieYSize = this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value;
                    double dieYClearance = this.StageSupervisor().WaferObject.GetSubsInfo().DieYClearance.Value;

                    this.StageSupervisor().StageModuleState.WaferLowViewMove(LLConer.X,
                        LLConer.Y + dieYSize - dieYClearance,
                        this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick);
                }
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    double dieYSize = this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value;
                    double dieYClearance = this.StageSupervisor().WaferObject.GetSubsInfo().DieYClearance.Value;
                    LastPos.X = LLConer.X + widthHalfSize;
                    LastPos.Y = LLConer.Y + dieYSize - dieYClearance - heightHalfSize;

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(LastPos.X,
                                                                                LastPos.Y,
                                                                                this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick);
                }

                ////2단계 : Grab

                ImageBuffer CurImg = null;
                CurImg = visionManager.SingleGrab(CurCam.GetChannelType(), this);

                ////3단계 : 이진화
                CurImg = visionAlgorithmes.MilDefaultBinarize(CurImg);

                ////4단계 : blob
                BlobParameter blobParam = new BlobParameter();
                ROIParameter roiParam = new ROIParameter();

                blobParam.BlobMinRadius.Value = 150;
                blobParam.BlobThreshHold.Value = 120;
                blobParam.MinBlobArea.Value = 500;
                blobParam.MaxBlobArea.Value = 90000;
                blobParam.MAX_FERET_X.Value = 300;
                blobParam.MAX_FERET_Y.Value = 300;
                blobParam.MIN_FERET_X.Value = 50;
                blobParam.MIN_FERET_Y.Value = 50;

                roiParam.OffsetX.Value = 0;
                roiParam.OffsetY.Value = 0;
                roiParam.Width.Value = 5000;
                roiParam.Height.Value = 5000;

                BlobResult blobResult = visionAlgorithmes.FindBlobObject(CurImg, blobParam, roiParam);

                ////4단계 : PatternMatching
                //visionManager.PatternMatching()

                ////5단계 : 오른쪽 끝 검사.
                if (blobResult != null)
                {
                    double maxXPos = blobResult.DevicePositions.Max(pos => pos.PosX);
                    GrabDevPosition grabDevPos = blobResult.DevicePositions.First(devPos => devPos.PosX == maxXPos);
                    maxXPos -= (grabDevPos.SizeX / 2);
                    LastPos.X += maxXPos;

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(LastPos.X,
                        LastPos.Y,
                        this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick);
                }

                ////6단계 : 다음 줄 해야하는지 검사.
                bool hasNextCheckLine = false;

                double dieXSize = this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value;
                double dieXClearance = this.StageSupervisor().WaferObject.GetSubsInfo().DieXClearance.Value;
                double nowXPos = 0;
                int isGetActPos = this.MotionManager().GetActualPos(EnumAxisConstants.X, ref nowXPos);

                hasNextCheckLine = nowXPos < LLConer.X + dieXSize - dieXClearance;

                // One Direction
                if (curMode == 0)
                {

                }
                // Four Side
                else if (curMode == 1)
                {

                }
                // All Area
                else if (curMode == 2)
                {

                }
                // Cur Screen
                else if (curMode == 3)
                {

                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadAutoRegistrationModule] [ScanPMIPadCommand()] : {err}");
            }
            return Task.CompletedTask;
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
