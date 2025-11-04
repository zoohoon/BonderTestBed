using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WABoundaryStandardModule
{
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.WaferAlignEX;
    using RelayCommandBase;
    using System.Windows.Input;
    using ProberInterfaces.Align;
    using ProberInterfaces.Param;
    using System.Windows;
    using ProberInterfaces.PnpSetup;
    using SubstrateObjects;
    using PnPControl;
    using ProberErrorCode;
    using WA_BoundaryParameter_Standard;
    using ProberInterfaces.State;
    using LogModule;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using MetroDialogInterfaces;

    public class BoundaryStandard : PNPSetupBase, ISetup, IRecovery, IParamNode, IProcessingModule, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;
        public override Guid ScreenGUID { get; } = new Guid("FD761F1E-F14C-FEF6-4EE3-FE3E6ADC5458");
        [ParamIgnore]
        public new List<object> Nodes { get; set; }
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

        private WABoundarySetupFunction ModifyCondition { get; set; }
        private ICamera Cam;

        public WaferObject Wafer { get { return this.GetParam_Wafer() as WaferObject; } }

        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        public SubModuleMovingStateBase MovingState { get; set; }

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

        private IParam _BoundaryParam_IParam;
        public IParam BoundaryParam_IParam
        {
            get { return _BoundaryParam_IParam; }
            set
            {
                if (value != _BoundaryParam_IParam)
                {
                    _BoundaryParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WA_BoundaryParam_Standard BoundaryParam_Clone;

        private double bottomleftx = 0;
        private double bottomlefty = 0;
        private double uppertopx = 0;
        private double uppertopy = 0;
        private WaferCoordinate lccoordinate;
        private WaferCoordinate rccoordinate;
        public BoundaryStandard()
        {

        }

        #region ..//Command & CommandMethod

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try 
            {
                UseUserControl = UserControlFucEnum.DEFAULT;
                Wafer.StopDrawDieOberlay(CurCam);

                if (parameter is EventCodeEnum)
                {
                    if ((EventCodeEnum)parameter == EventCodeEnum.NONE)
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


        //private void ShowMessages(string message)
        //{
        //    this.MetroDialogManager().ShowMessageDialog("Access denieded.",
        //        string.Format("Authority level is not sufficient to access this button({0}).", message),EnumMessageStyle.AffirmativeAndNegative);
        //}
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command) _Button1Command
                        = new AsyncCommand(Button1);
                return _Button1Command;
            }
        }

        private async Task Button1()
        {
            try
            {
                //if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING
                //    & this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                if(this.WaferAligner().IsNewSetup & (this.WaferAligner().GetIsModify() == false))
                {
                    ModifyCondition = WABoundarySetupFunction.TEACHBOTTOMLEFT;
                }
                else
                {
                    ModifyCondition = WABoundarySetupFunction.TEACHBOUNDARY;
                }

                
                await Modify();
                //SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }


        private AsyncCommand _Button2Command;
        public ICommand Button2Command
        {
            get
            {
                if (null == _Button2Command) _Button2Command
                        = new AsyncCommand(Button2);
                return _Button2Command;
            }
        }

        private async Task Button2()
        {
            try
            {
                ModifyCondition = WABoundarySetupFunction.TEACHUPPERRIGHT;
                //await Task.Run(() => Modify());
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {
                    if (Initialized == false)
                    {
                        SubModuleState = new SubModuleIdleState(this);
                        MovingState = new SubModuleStopState(this);
                        SetupState = new NotCompletedState(this);

                        Header = Properties.Resources.Header;
                        RecoveryHeader = Properties.Resources.RecoveryHeader;
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public MovingStateEnum GetMovingState()
        {
            try
            {
                return MovingState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InitPnpModuleStage();
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
                MiniViewTarget = this.StageSupervisor().WaferObject;

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                if (this.WaferAligner().IsNewSetup )
                {
                    retVal = await InitSetup();
                }
                else
                {
                    retVal = await InitRecovery();
                }


                InitLightJog(this);
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
                retVal = InitPnpModuleStage();

                if ((BoundaryParam_IParam as WA_BoundaryParam_Standard).CamType != EnumProberCam.UNDEFINED ||
              (BoundaryParam_IParam as WA_BoundaryParam_Standard).CamType != EnumProberCam.INVALID)
                {
                    Cam = this.VisionManager().GetCam((BoundaryParam_IParam as WA_BoundaryParam_Standard).CamType);
                }
                else
                {
                    (BoundaryParam_IParam as WA_BoundaryParam_Standard).CamType = EnumProberCam.WAFER_HIGH_CAM;
                }

                this.VisionManager().StartGrab((BoundaryParam_IParam as WA_BoundaryParam_Standard).CamType, this);

                //if (Wafer.PhysInfo.LowLeftCorner != null)
                //{
                //    Wafer.PhysInfo.LowLeftCorner = null;
                //    Wafer.PhysInfo.LowLeftCorner = new WaferCoordinate();
                //}

                //======================


                Point pt = this.WaferAligner().GetLeftCornerPosition
                    (Wafer.GetSubsInfo().WaferCenter.X.Value, Wafer.GetSubsInfo().WaferCenter.Y.Value);

                this.StageSupervisor().StageModuleState.WaferHighViewMove(pt.X, pt.Y, WaferObject.GetSubsInfo().ActualThickness);

                //======================

                CurCam = Cam;
                MainViewTarget = DisplayPort;
                MiniViewTarget = this.StageSupervisor().WaferObject;


                ModifyCondition = WABoundarySetupFunction.UNDEFINED;
                UseUserControl = UserControlFucEnum.DIELEFTCORNER;

                retVal = InitPNPSetupUI();

                //if (!this.WaferAligner().IsNewSetup)
                Wafer.DrawDieOverlay(CurCam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.WAFER_SETUP_PROCEDURE_EROOR;
                throw err;
            }
            //Simplex Remove.
            //retVal = this.WaferAligner().CheckSetupStep(this);

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                WaferCoordinate coord = null;
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM, this);

                coord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgLCPoint = this.WaferAligner().GetLeftCornerPosition(coord.GetX(), coord.GetY());

                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgLCPoint.X + (Wafer.GetSubsInfo().DieXClearance.Value / 2),
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgLCPoint.Y + (Wafer.GetSubsInfo().DieYClearance.Value / 2),
                    Wafer.GetSubsInfo().ActualThickness);

                MainViewTarget = DisplayPort;
                MiniViewTarget = Wafer;

                InitStateUI();

                //InitPNPSetupUI();
                //SeletedStep = this;

                if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/WABoundaryRCIcon.png");
                }
                else
                {
                    OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/WABoundaryLCIcon.png");
                }
                OneButton.Command = (ICommand)Button1Command;

                ModifyCondition = WABoundarySetupFunction.TEACHBOUNDARY;
                UseUserControl = UserControlFucEnum.DIELEFTCORNER;

                Wafer.DrawDieOverlay(CurCam);

                if (SetupState.GetState() == EnumMoudleSetupState.NOTCOMPLETED)
                {
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.SetBoundaryFalg = false;
                }
                else
                {
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.SetBoundaryFalg = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/WABoundaryLCIcon.png");
                OneButton.Command = (ICommand)Button1Command;

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/WABoundaryRCIcon.png");
                TwoButton.Command = (ICommand)Button2Command;

                if (VisionManager.GetDispHorFlip() == DispFlipEnum.FLIP && VisionManager.GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/WABoundaryLCIcon.png");
                    OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/WABoundaryRCIcon.png");
                }

                PadJogSelect.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/barn.png");
                PadJogSelect.Command = (ICommand)RefreshMapCommand;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new WA_BoundaryParam_Standard();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(WA_BoundaryParam_Standard));

                if (RetVal == EventCodeEnum.NONE)
                {
                    BoundaryParam_IParam = tmpParam;
                    BoundaryParam_Clone = BoundaryParam_IParam as WA_BoundaryParam_Standard;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(BoundaryParam_IParam);
                this.StageSupervisor().SaveWaferObject();
            }
            catch (Exception err)
            {
                throw err;
            }

            return RetVal;
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

        private EventCodeEnum ResetMapInfo()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
                double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();
                double wafercornerFormcornerX = 0.0;
                double wafercornerFormcornerY = 0.0;

                double dieXclearance = Math.Abs(uppertopx - bottomleftx);
                double dieYclearance = Math.Abs(uppertopy - bottomlefty);

                Wafer.GetSubsInfo().DieXClearance.Value = dieXclearance;
                Wafer.GetSubsInfo().DieYClearance.Value = dieYclearance;

                Wafer.GetSubsInfo().DieXClearance.Value = dieXclearance;
                Wafer.GetSubsInfo().DieYClearance.Value = dieYclearance;

                wafercornerFormcornerX = (bottomleftx - (dieXclearance / 2)) - wafercenterx;
                wafercornerFormcornerY = (bottomlefty - (dieYclearance / 2)) - wafercentery;


                Wafer.GetPhysInfo().LowLeftCorner.X.Value = wafercornerFormcornerX;
                Wafer.GetPhysInfo().LowLeftCorner.Y.Value = wafercornerFormcornerY;


                double indexsizewidth = Wafer.GetSubsInfo().ActualDieSize.Width.Value;


                Wafer.GetSubsInfo().ActualDeviceSize.Width.Value =
                    Wafer.GetSubsInfo().ActualDieSize.Width.Value - dieXclearance;

                Wafer.GetSubsInfo().ActualDeviceSize.Height.Value =
                    Wafer.GetSubsInfo().ActualDieSize.Height.Value - dieYclearance;

                //Wafer.WaferDevObject.CalcWaferCenterIndex();
                Wafer.WaferDevObject.UpdateWaferBoundary();
                Wafer.WaferDevObject.CalcMapOffset();

                var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(Wafer.GetPhysInfo().CenM.XIndex.Value, Wafer.GetPhysInfo().CenM.YIndex.Value);
                Wafer.GetPhysInfo().CenU.XIndex.Value = userIndex.XIndex;//Wafer.GetPhysInfo().CenM.XIndex;
                Wafer.GetPhysInfo().CenU.YIndex.Value = userIndex.YIndex;//Wafer.GetPhysInfo().CenM.YIndex;

                Wafer.GetPhysInfo().LowLeftCorner.X.Value = Wafer.GetSubsInfo().RefDieLeftCorner.GetX() - wafercenterx;
                Wafer.GetPhysInfo().LowLeftCorner.Y.Value = Wafer.GetSubsInfo().RefDieLeftCorner.GetY() - wafercentery;

                Wafer.GetPhysInfo().LowLeftCorner.X.DoneState = ElementStateEnum.DONE;
                Wafer.GetPhysInfo().LowLeftCorner.Y.DoneState = ElementStateEnum.DONE;
                Wafer.GetSubsInfo().DieXClearance.DoneState = ElementStateEnum.DONE;
                Wafer.GetSubsInfo().DieYClearance.DoneState = ElementStateEnum.DONE;

                Wafer.DrawDieOverlay(CurCam);
            }
            catch (Exception err)
            {

                throw err;
            }


            return RetVal;

        }

        private AsyncCommand _RefreshMapCommand;
        public ICommand RefreshMapCommand
        {
            get
            {
                if (null == _RefreshMapCommand) _RefreshMapCommand
                        = new AsyncCommand(RefreshMap);
                return _RefreshMapCommand;
            }
        }

        private async Task RefreshMap()
        {
            try
            {
                var mRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message",
                    "Automatically create wafer map. Do you want t to continue?", EnumMessageStyle.AffirmativeAndNegative);
                if (mRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    Wafer.WaferDevObject.UpdateWaferObject(Wafer.WaferDevObject.AutoCalWaferMap(true));
                    Wafer.UpdateWaferObject();
                    if (this.LoaderRemoteMediator()?.GetServiceCallBack() != null)
                    {
                        await this.LoaderRemoteMediator()?.GetServiceCallBack()?.RequestGetWaferObject();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        public async Task<EventCodeEnum> Modify()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                switch (ModifyCondition)
                {
                    case WABoundarySetupFunction.TEACHBOTTOMLEFT:
                        {
                            lccoordinate = GetPosition();
                            if (lccoordinate != null)
                            {
                                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                                {
                                    bottomleftx = lccoordinate.X.Value;
                                    bottomlefty = lccoordinate.Y.Value;

                                    //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/WABoundaryRCIcon.png");
                                    UseUserControl = UserControlFucEnum.DIERIGHTCORNER;
                                }
                                else
                                {
                                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                                    {
                                        WaferCoordinate coord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                                        this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX =
                                            coord.GetX() - this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgLCPoint.X;
                                        this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY =
                                            coord.GetY() - this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgLCPoint.Y;


                                        double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
                                        double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

                                        bottomleftx = lccoordinate.X.Value;
                                        bottomlefty = lccoordinate.Y.Value;

                                        Point pt = this.WaferAligner().GetLeftCornerPosition(coord.GetX(), coord.GetY());

                                        //pt = this.WaferAligner().UserIndexConvertLeftBottomCorner(
                                        //    CurCam.CamSystemUI.XIndex, CurCam.CamSystemUI.YIndex);

                                        //Wafer.SubsInfo.RecoveryParam.LCPointOffsetX =
                                        //    pt.X - bottomleftx;

                                        //Wafer.SubsInfo.RecoveryParam.LCPointOffsetY =
                                        //   pt.Y - bottomlefty;

                                        Point wcpt = this.WaferAligner().GetLeftCornerPosition(wafercenterx, wafercentery);
                                        Wafer.GetSubsInfo().RefDieLeftCorner.X.Value = wcpt.X;
                                        Wafer.GetSubsInfo().RefDieLeftCorner.Y.Value = wcpt.Y;

                                        ProcessingType = EnumSetupProgressState.DONE;


                                    }
                                    else
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog(
                                                "WaferAlignBoundarySetup",
                                                "Please use High-Magnitude Camera to Register Corner",
                                                EnumMessageStyle.Affirmative);
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Debug("LeftBottomCorner can not be positioned to the right of the LeftBottomCorner.");
                                ModifyCondition = WABoundarySetupFunction.UNDEFINED;
                                //var mtask = Task.Run(async () =>
                                //{
                                //    await this.MetroDialogManager().ShowMessageDialog("WaferAlignBoundarySetup", "Please try again with WaferHigh camera.", EnumMessageStyle.Affirmative);
                                //});
                                //mtask.Wait();
                                await this.MetroDialogManager().ShowMessageDialog(
                                    "WaferAlignBoundarySetup", 
                                    "Please use High-Magnitude Camera to Register Corner", 
                                    EnumMessageStyle.Affirmative);
                            }
                            SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                        }
                        break;
                    case WABoundarySetupFunction.TEACHUPPERRIGHT:
                        bool validData = true;
                        rccoordinate = GetPosition();

                        if(lccoordinate != null)
                        {
                            if  (this.VisionManager().DigitizerService[CurCam.GetDigitizerIndex()].CurCamera.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                            {
                                if (rccoordinate != null)
                                {
                                    if (lccoordinate.GetX() <= rccoordinate.GetX() || lccoordinate.GetY() <= rccoordinate.GetY())
                                    {
                                        LoggerManager.Debug("RightUpperCorner can not be positioned to the right of the LeftBottomCorner.");

                                        await this.MetroDialogManager().ShowMessageDialog("WaferAlignBoundarySetup", "The setting range is out of range.Please set it again.", EnumMessageStyle.Affirmative);
                                        validData = false;
                                    }

                                    if (validData != false)
                                    {
                                        uppertopx = rccoordinate.X.Value;
                                        uppertopy = rccoordinate.Y.Value;
                                        ResetMapInfo();
                                        //Wafer.WaferDevObject.UpdateWaferObject(Wafer.WaferDevObject.AutoCalWaferMap(true));
                                        Wafer.UpdateWaferObject();
                                        //await this.LoaderRemoteMediator()?.GetServiceCallBack()?.RequestGetWaferObject();

                                        if(this.LoaderRemoteMediator()?.GetServiceCallBack() != null)
                                        {
                                            await this.LoaderRemoteMediator()?.GetServiceCallBack()?.RequestGetWaferObject();
                                        }
                                        UseUserControl = UserControlFucEnum.DIELEFTCORNER;

                                        Wafer.StopDrawDieOberlay(CurCam);
                                        Wafer.DrawDieOverlay(CurCam);

                                        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);

                                    }
                                    else
                                    {
                                        UseUserControl = UserControlFucEnum.DIELEFTCORNER;
                                        ModifyCondition = WABoundarySetupFunction.TEACHBOTTOMLEFT;
                                        Wafer.StopDrawDieOberlay(CurCam);
                                    }
                                    lccoordinate = null;
                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog(
                                        "Error Message",
                                        "Please use High-Magnitude Camera to Register Corner",
                                        EnumMessageStyle.Affirmative);
                                UseUserControl = UserControlFucEnum.DIELEFTCORNER;
                                ModifyCondition = WABoundarySetupFunction.TEACHBOTTOMLEFT;
                            }
                        }
                        else
                        {
                            // LeftCorner 를 먼저 찍지않았을때
                            //var mtask = Task.Run(async () =>
                            //{
                            //    await this.MetroDialogManager().ShowMessageDialog(
                            //        "Error Message",
                            //        "Please register the left corner first.", 
                            //        EnumMessageStyle.Affirmative);

                            //});
                            //mtask.Wait();
                            await this.MetroDialogManager().ShowMessageDialog(
                                    "Error Message",
                                    "Please register the left corner first.",
                                    EnumMessageStyle.Affirmative);
                        }
                        break;
                    case WABoundarySetupFunction.TEACHBOUNDARY:
                        {
                            WaferCoordinate lccoordinate = GetPosition();
                            bottomleftx = lccoordinate.X.Value;
                            bottomlefty = lccoordinate.Y.Value;

                            this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX = 0;
                            this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY = 0;

                            Point originpos = this.WaferAligner().GetLeftCornerPosition(lccoordinate);
                            this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX = bottomleftx -
                               (originpos.X + (this.Wafer.GetSubsInfo().DieXClearance.Value / 2));
                            this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY = bottomlefty -
                               (originpos.Y + (this.Wafer.GetSubsInfo().DieYClearance.Value / 2));

                            //this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX = 
                            //    this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgLCPoint.X - bottomleftx;
                            //this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY =
                            //   this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgLCPoint.Y - bottomlefty;

                            CurCam.UpdateOverlayFlag = true;
                            //Wafer.StopDrawDieOberlay(CurCam);
                            //Wafer.DrawDieOverlay(CurCam);
                            this.WaferAligner().WaferAlignInfo.RecoveryParam.SetBoundaryFalg = true;
                            SubModuleState = new SubModuleDoneState(this);
                            SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                        }
                        break;
                    case WABoundarySetupFunction.PREVSTEP:
                        break;

                }

                IsParamChanged = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }




        private WaferCoordinate GetPosition()
        {
            try
            {
                if (this.VisionManager().DigitizerService[CurCam.GetDigitizerIndex()].CurCamera.GetChannelType() != EnumProberCam.WAFER_HIGH_CAM)
                {
                    return null;
                }
                else
                {
                    return this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }






        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Wafer.GetPhysInfo().LowLeftCorner.X.DoneState != ElementStateEnum.NEEDSETUP
                     && Wafer.GetPhysInfo().LowLeftCorner.Y.DoneState != ElementStateEnum.NEEDSETUP
                     && Wafer.GetPhysInfo().LowLeftCorner.Z.DoneState != ElementStateEnum.NEEDSETUP
                     && Wafer.GetPhysInfo().LowLeftCorner.T.DoneState != ElementStateEnum.NEEDSETUP
                     && Wafer.GetSubsInfo().DieXClearance.DoneState != ElementStateEnum.NEEDSETUP
                     && Wafer.GetSubsInfo().DieYClearance.DoneState != ElementStateEnum.NEEDSETUP)
                {
                    retVal = EventCodeEnum.NONE;
                }

                else if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.ALIGN)
                {
                    retVal = Extensions_IParam.ElementStateNeedSetupValidation(BoundaryParam_IParam);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public override EventCodeEnum ClearSettingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ParamValidation();
                if (this.WaferAligner().IsNewSetup)
                {
                    WaferObject.GetPhysInfo().LowLeftCorner.X.Value = 0;
                    WaferObject.GetPhysInfo().LowLeftCorner.Y.Value = 0;
                    Wafer.GetPhysInfo().LowLeftCorner.X.DoneState = ElementStateEnum.NEEDSETUP;
                    Wafer.GetPhysInfo().LowLeftCorner.Y.DoneState = ElementStateEnum.NEEDSETUP;
                    WaferObject.GetSubsInfo().DieXClearance.Value = 0;
                    WaferObject.GetSubsInfo().DieYClearance.Value = 0;
                    WaferObject.GetSubsInfo().DieXClearance.DoneState = ElementStateEnum.NEEDSETUP;
                    WaferObject.GetSubsInfo().DieYClearance.DoneState = ElementStateEnum.NEEDSETUP;
                }

                SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum LoadParameter()
        {
            try
            {
                return LoadDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum SaveParameter()
        {
            try
            {
                return SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public SubModuleStateEnum GetState()
        {
            try
            {
                return SubModuleState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum Execute()
        {
            try
            {
                return SubModuleState.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum DoExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
                if (retVal == EventCodeEnum.NONE)
                    SubModuleState = new SubModuleDoneState(this);
                else
                    SubModuleState = new SubModuleErrorState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ClearData()
        {
            try
            {
                return SubModuleState.ClearData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ParamValidation();
                //Test Code
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum Recovery()
        {
            try
            {
                return SubModuleState.Recovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum DoRecovery()
        {
            try
            {
                if (this.GetState() == SubModuleStateEnum.IDLE | this.GetState() == SubModuleStateEnum.SKIP)
                    SubModuleState = new SubModuleRecoveryState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ExitRecovery()
        {
            try
            {
                return SubModuleState.ExitRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Wafer.GetPhysInfo().LowLeftCorner.X.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX;
               // Wafer.GetPhysInfo().LowLeftCorner.Y.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY;

               // this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX = 0;
               // this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY = 0;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }
        public bool IsExecute()
        {
            bool retVal = true;
            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {

                if (Wafer.GetPhysInfo().LowLeftCorner.X.DoneState != ElementStateEnum.NEEDSETUP
                       && Wafer.GetPhysInfo().LowLeftCorner.Y.DoneState != ElementStateEnum.NEEDSETUP
                       && Wafer.GetPhysInfo().LowLeftCorner.Z.DoneState != ElementStateEnum.NEEDSETUP
                       && Wafer.GetPhysInfo().LowLeftCorner.T.DoneState != ElementStateEnum.NEEDSETUP
                       && Wafer.GetSubsInfo().DieXClearance.DoneState != ElementStateEnum.NEEDSETUP
                       && Wafer.GetSubsInfo().DieYClearance.DoneState != ElementStateEnum.NEEDSETUP)
                {
                    if (Extensions_IParam.ElementStateNeedSetupValidation(BoundaryParam_IParam) == EventCodeEnum.NONE)
                    {
                        retVal = false;
                    }
                    else
                    {
                        retVal = true;
                    }
                }

                retVal = IsParamChanged | retVal;
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
                    //if(this.WaferAligner().ModuleState.GetState() == ModuleStateEnum.IDLE
                    //    & this.WaferAligner().GetPreModuleState() == ModuleStateEnum.RECOVERY)
                    if (this.WaferAligner().GetIsModify())
                    {
                        SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    }
                    else
                    {
                        if (ParamValidation() == EventCodeEnum.NONE)
                            SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                        else
                            SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    }

                }
                else
                {
                    if (this.GetState() != SubModuleStateEnum.DONE)
                        SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    else
                        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }

                if (!this.WaferAligner().IsNewSetup)
                {
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
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
    }
}