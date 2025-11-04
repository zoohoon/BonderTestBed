using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WAPadStandardModule
{
    using ProberInterfaces;
    using ProberInterfaces.WaferAlignEX;
    using System.IO;
    using RelayCommandBase;
    using System.Windows.Input;
    using ProberInterfaces.Param;
    using ProberInterfaces.Pad;
    using ProberInterfaces.PnpSetup;
    using SubstrateObjects;
    using PnPControl;
    using System.Windows;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Align;
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using WA_PadParameter_Standard;
    using ProberInterfaces.State;
    using LogModule;
    //using DutViewer.ViewModel;
    using Vision.GraphicsContext;
    using System.Windows.Media;
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    using MetroDialogInterfaces;
    using GeometryHelp;
    using ProbeCardObject;

    public class PadStandard : PNPSetupBase, ISetup, IRecovery, IProcessingModule, IHasDevParameterizable, ILotReadyAble
    {
        public override Guid ScreenGUID { get; } = new Guid("DE441C02-5B97-B9C4-1AD9-26DEEC0B5595");

        public enum WAPadSetupFunction
        {
            UNDIFINE = -1,
            REGPAD,
            DELETEPAD,
            DELETEALLPAD,
            AUTOSEARCH,
            CHANGEPATTERNWIDTH,
            CHANGEDPATTRNHEIGHT,
            MOVETOPAD,
            PREVSETP,
            COMPAREREFPAD
        }
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
        [ParamIgnore]
        public new List<object> Nodes { get; set; }

        #region ..//Property

        public override bool Initialized { get; set; } = false;

        private WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        private WAPadSetupFunction ModifyCondition;

        private int _ChangeWidthValue;
        private int _ChangeHeightValue;

        private bool IsChanged { get; set; } = false;

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
        public SubModuleMovingStateBase MovingState { get; set; }

        private IPadRegist _PadReg;
        public IPadRegist PadReg
        {
            get { return _PadReg; }
            set
            {
                if (value != _PadReg)
                {
                    _PadReg = value;
                    RaisePropertyChanged();
                }
            }
        }


        private List<DUTPadObject> _DutPadInfos
            = new List<DUTPadObject>();
        public List<DUTPadObject> DutPadInfos
        {
            get { return _DutPadInfos; }
            set
            {
                if (value != _DutPadInfos)
                {
                    _DutPadInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        public WA_PadParam_Standard PadStandardParam_Clone { get; set; }

        private IFocusing _WaferPadFocusModel;
        public IFocusing WaferPadFocusModel
        {
            get
            {
                if (_WaferPadFocusModel == null)
                    _WaferPadFocusModel = this.FocusManager().GetFocusingModel((PadStandardParam_IParam as WA_PadParam_Standard).FocusingModuleDllInfo);

                return _WaferPadFocusModel;
            }
        }

        private ICamera precam;
        private ICamera _CurCam;
        public override ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                precam = _CurCam;
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
                ReSizeingOverlayRect();
                ConfirmDisplay(precam, _CurCam);
            }
        }

        private IFocusParameter FocusParam => (PadStandardParam_IParam as WA_PadParam_Standard)?.FocusParam;
        //PadRegDutViewerViewModel DutViewer;
        private bool _EnablePADCompensation;
        public bool EnablePADCompensation
        {
            get { return _EnablePADCompensation; }
            set
            {
                if (value != _EnablePADCompensation)
                {
                    _EnablePADCompensation = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        private void ReSizeingOverlayRect()
        {
            try
            {
                if (DutPadInfos.Count != 0)
                {
                    if (CurPadIndex != 0)
                    {
                        TargetRectangleWidth =
                             DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex - 1].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                        TargetRectangleHeight =
                            DisplayPort.ConvertDisplayHeight(DutPadInfos[CurPadIndex - 1].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();
                    }
                    else
                    {
                        TargetRectangleWidth =
                             DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                        TargetRectangleHeight =
                            DisplayPort.ConvertDisplayHeight(DutPadInfos[0].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();
                    }
                }
                else
                {
                    if (precam == null)
                        return;
                    if (precam.GetChannelType() == CurCam.GetChannelType())
                        return;

                    //RegisteImageBufferParam rectparam = GetDisplayPortRectInfo();
                    ICamera cam = null;
                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                    else
                        cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                    //Task.Run(() =>
                    //{
                    TargetRectangleWidth = (TargetRectangleWidth * precam.GetRatioX()) / CurCam.GetRatioX();
                    TargetRectangleHeight = (TargetRectangleHeight * precam.GetRatioY()) / CurCam.GetRatioY();
                    //});

                    //TargetRectangleWidth = Math.Ceiling(
                    //    DisplayPort.ConvertDisplayWidth((rectparam.Width * cam.GetRatioX()), CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX());
                    //TargetRectangleHeight = Math.Ceiling(
                    //    DisplayPort.ConvertDisplayHeight((rectparam.Height * cam.GetRatioY()), CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitOverlayRect()
        {
            TargetRectangleWidth = 128;
            TargetRectangleHeight = 128;
        }

        #region ..//Command

        private RelayCommand _UIModeChangeCommand;
        public ICommand UIModeChangeCommand
        {
            get
            {
                if (null == _UIModeChangeCommand)
                    _UIModeChangeCommand = new RelayCommand(
UIModeChange, EvaluationPrivilege.Evaluate(
CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
new Action(() => { ShowMessages("UIModeChange"); }));
                return _UIModeChangeCommand;
            }
        }

        private void UIModeChange()
        {

        }

        private void ClearPinData()
        {
            var cardinfo = this.GetParam_ProbeCard();

            try
            {
                foreach (Dut dut in cardinfo.ProbeCardDevObjectRef.DutList)
                {
                    dut.PinList.Clear();
                }

                LoggerManager.Debug($"[{this.GetType().Name}], ClearPinData() : pin data cleared.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsChanged)
                {
                    ClearPinData();
                }

                this.WaferAligner().SetIsNewSetup(false);

                if (Wafer.GetPhysInfo().Thickness.Value != Wafer.GetSubsInfo().ActualThickness)
                {
                    Wafer.GetPhysInfo().Thickness.Value = Math.Round(Wafer.GetSubsInfo().ActualThickness, 3);
                }

                SaveDevParameter();

                List<IDeviceObject> devices = Wafer.GetDevices();
                IDeviceObject dev = devices.Find(device => device.DieType.Value == DieTypeEnum.TEACH_DIE);

                if (dev != null)
                    dev.DieType.Value = DieTypeEnum.TEST_DIE;

                UseUserControl = UserControlFucEnum.DEFAULT;

                retVal = await base.Cleanup(parameter);

                SetMotionJogMoveZOffsetEnable(false);
                if (retVal == EventCodeEnum.NONE)
                {
                    StopDrawDut();
                }

                if (this.WaferAligner().GetWAInnerStateEnum() != WaferAlignInnerStateEnum.SETUP)
                {
                    //Recovery
                    foreach (var module in this.WaferAligner().Template.GetProcessingModule())
                    {
                        retVal = module.ExitRecovery();
                        if (retVal != EventCodeEnum.NONE)
                            break;
                    }
                    if (retVal == EventCodeEnum.NONE)
                    {
                        //WaferObject.SetAlignState(AlignStateEnum.DONE);
                        //this.WaferAligner().SetModuleDoneState();
                    }
                }

                if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                {
                    await this.LoaderRemoteMediator()?.GetServiceCallBack()?.RequestGetWaferObject();
                }

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.PADCOUNT, this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count.ToString());

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.WaferAligner().InitIdelState();
            }
            return retVal;
        }


        private RelayCommand<object> _VerifyRefPadCommand;
        public ICommand VerifyRefPadCommand
        {
            get
            {
                if (null == _VerifyRefPadCommand)
                    _VerifyRefPadCommand = new RelayCommand<object>(VerifyRefPad);
                return _VerifyRefPadCommand;
            }
        }

        private void VerifyRefPad(object noparam)
        {
            try
            {
                WaferCoordinate padcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                Wafer.GetSubsInfo().WaferCenterOffset.X.Value = padcoord.GetX() - this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgRefPadPoint.X;
                Wafer.GetSubsInfo().WaferCenterOffset.Y.Value = padcoord.GetY() - this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgRefPadPoint.Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #endregion

        #region ..//Command

        private AsyncCommand _AddCommand;
        public ICommand AddCommand
        {
            get
            {
                if (null == _AddCommand)
                    _AddCommand = new AsyncCommand(
    AddPad//, EvaluationPrivilege.Evaluate(
          // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
          // new Action(() => { ShowMessages("UIModeChange"); })
       );
                return _AddCommand;
            }
        }

        private async Task AddPad()
        {
            try
            {
                ModifyCondition = WAPadSetupFunction.REGPAD;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _FindCommand;
        public ICommand FindCommand
        {
            get
            {
                if (null == _FindCommand)
                    _FindCommand = new RelayCommand(
    FindPad//, EvaluationPrivilege.Evaluate(
           // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
           // new Action(() => { ShowMessages("UIModeChange"); })
      );
                return _FindCommand;
            }
        }

        private void FindPad()
        {
            PadReg.FindPad();
        }

        private AsyncCommand _DeleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (null == _DeleteCommand)
                    _DeleteCommand = new AsyncCommand(
    Delete//, EvaluationPrivilege.Evaluate(
          // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
          // new Action(() => { ShowMessages("UIModeChange"); })
    );
                return _DeleteCommand;
            }
        }

        private async Task Delete()
        {
            try
            {
                ModifyCondition = WAPadSetupFunction.DELETEPAD;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
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
        private double _CurXPos;
        public new double CurXPos
        {
            get { return _CurXPos; }
            set
            {
                if (value != _CurXPos)
                {
                    _CurXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CurYPos;
        public new double CurYPos
        {
            get { return _CurYPos; }
            set
            {
                if (value != _CurYPos)
                {
                    _CurYPos = value;
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

        private async Task SetTeachDie()
        {
            try
            {
                //var messageret = await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.WarningMessage, 
                //    Properties.Resources.Change_TeachDie_Message, EnumMessageStyle.AffirmativeAndNegative);

                var messageret = await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.WarningMessage, Properties.Resources.Change_TeachDie_Message, EnumMessageStyle.AffirmativeAndNegative);

                if (messageret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    // TODO: FLIP 검토 할것 : 패드 화면에서의 바운더리 그림을 뒤집어서 그 화면의 DUT:1 에 등록한다면 주석을 풀어야함.

                    //if (this.VisionManager().DispHorFlip == DispFlipEnum.FLIP)
                    //{
                    //    this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex = this.WaferObject.GetPhysInfo().MapCountX.Value - 1 - CurCam.GetCurCoordMachineIndex().XIndex;
                    //}
                    //else
                    {
                        this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex = CurCam.GetCurCoordMachineIndex().XIndex;
                    }

                    //if (this.VisionManager().DispVerFlip == DispFlipEnum.FLIP)
                    //{
                    //    this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex = this.WaferObject.GetPhysInfo().MapCountY.Value - 1 - CurCam.GetCurCoordMachineIndex().YIndex;
                    //}
                    //else
                    {
                        this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex = CurCam.GetCurCoordMachineIndex().YIndex;
                    }

                    LoggerManager.Debug($"SetTeachDie:({this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex},{this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex})", isInfo: true);

                    this.WaferAligner().SetTeachDevice(true, this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex, this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex, CurCam.GetChannelType());

                    DeleteAllPad();
                    DrawDutPad();

                    CurCam.UpdateOverlayFlag = true;

                    CalcDutCen();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private async Task MoveToPad()
        {
            try
            {
                ModifyCondition = WAPadSetupFunction.MOVETOPAD;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void DeleteAllPad()
        {
            try
            {
                if (DutPadInfos != null)
                {
                    IsChanged = true;

                    DutPadInfos.Clear();
                    WaferObject.GetSubsInfo().Pads.DutPadInfos = DutPadInfos;
                    this.WaferAligner().UpdatePadCen();
                    CurCam.UpdateOverlayFlag = true;
                    CurPadIndex = 0;
                    StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";

                    if (this.LoaderRemoteMediator() != null)
                    {
                        byte[] dutpadinfos = null;

                        dutpadinfos = this.ObjectToByteArray(DutPadInfos);

                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateDutPadInfos(dutpadinfos);
                    }

                    WaferObject.PadSetupChangedToggle.Value = !oripadsetuptoggle;

                    UseUserControl = UserControlFucEnum.PTRECT;
                    SetStepSetupState();

                    this.GetParam_ProbeCard().SetAlignState(AlignStateEnum.IDLE);
                    this.GetParam_ProbeCard().SetPinPadAlignState(AlignStateEnum.IDLE);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task PatternWidthPlus()
        {
            try
            {
                ModifyCondition = WAPadSetupFunction.CHANGEPATTERNWIDTH;
                _ChangeWidthValue = 1;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private async Task PatternWidthMinus()
        {
            try
            {
                //OneButton.IsEnabled = true;
                //TwoButton.IsEnabled = true;

                ModifyCondition = WAPadSetupFunction.CHANGEPATTERNWIDTH;
                _ChangeWidthValue = -1;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async Task PatternHeightMinus()
        {
            try
            {
                ModifyCondition = WAPadSetupFunction.CHANGEDPATTRNHEIGHT;
                _ChangeHeightValue = -1;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private async Task PatternHeightPlus()
        {
            try
            {
                //this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.DONE);

                ModifyCondition = WAPadSetupFunction.CHANGEDPATTRNHEIGHT;
                _ChangeHeightValue = 1;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _DoMarkAlignCommand;
        public ICommand DoMarkAlignCommand
        {
            get
            {
                if (null == _DoMarkAlignCommand)
                    _DoMarkAlignCommand = new AsyncCommand(DoMarkAlignCommandFunc);
                return _DoMarkAlignCommand;
            }
        }

        private async Task DoMarkAlignCommandFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                // 현재 위치 기억.

                EnumProberCam prevCam = CurCam.GetChannelType();
                CatCoordinates prevPos = CurCam.CamSystemPos;
                WaferCoordinate wcd = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                // 이미 DONE이어도, 강제로 실행
                this.MarkAligner().DoMarkAlign(true);

                // 기억했던 위치로 이동.
                // TODO : 카메라 다시 변경 시, 동작시켜줘야 되는 것 확인.
                CurCam = this.VisionManager().GetCam(prevCam);

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(prevPos.X.Value, prevPos.Y.Value, prevPos.Z.Value);
                }
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(prevPos.X.Value, prevPos.Y.Value, prevPos.Z.Value);
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                DrawDutPad();
                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }


        private AsyncCommand _TogglePadCompCommand;
        public ICommand TogglePadCompCommand
        {
            get
            {
                if (null == _TogglePadCompCommand)
                    _TogglePadCompCommand = new AsyncCommand(TogglePadCompCommandFunc, false);
                return _TogglePadCompCommand;
            }
        }

        private Task TogglePadCompCommandFunc()
        {
            try
            {
                EnablePADCompensation = !EnablePADCompensation;

                StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private int _CurPadIndex = 0;
        public int CurPadIndex
        {
            get { return _CurPadIndex; }
            set
            {
                if (value != _CurPadIndex)
                {
                    _CurPadIndex = value;
                    RaisePropertyChanged();

                    //if(_CurPadIndex == 0)
                    //{
                    //    TargetRectangleWidth = 128;
                    //    TargetRectangleHeight = 128;
                    //    ReSizeingOverlayRect();
                    //}
                }
            }
        }

        private Task PrevPad()
        {
            try
            {

                if (DutPadInfos.Count == 0)
                    return Task.CompletedTask;
                if (CurPadIndex - 1 > 0)
                {
                    WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)DutPadInfos[CurPadIndex - 2].MachineIndex.XIndex,
                                      (int)DutPadInfos[CurPadIndex - 2].MachineIndex.YIndex);



                    double xpos = wcoord.GetX() + DutPadInfos[CurPadIndex - 2].PadCenter.X.Value;
                    double ypos = wcoord.GetY() + DutPadInfos[CurPadIndex - 2].PadCenter.Y.Value;
                    double zpos = this.WaferAligner().GetHeightValueAddZOffset(xpos, ypos, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            xpos,
                            ypos,
                            zpos, true);
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            xpos,
                            ypos,
                            zpos, true);

                    TargetRectangleWidth =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex - 2].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                    TargetRectangleHeight =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex - 2].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();

                    CurPadIndex--;
                }
                else if (CurPadIndex - 1 == 0)
                {
                    CurPadIndex = DutPadInfos.Count;
                    WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)DutPadInfos[CurPadIndex - 1].MachineIndex.XIndex,
                  (int)DutPadInfos[CurPadIndex - 1].MachineIndex.YIndex);


                    double xpos = wcoord.GetX() + DutPadInfos[CurPadIndex - 1].PadCenter.X.Value;
                    double ypos = wcoord.GetY() + DutPadInfos[CurPadIndex - 1].PadCenter.Y.Value;
                    double zpos = this.WaferAligner().GetHeightValueAddZOffset(xpos, ypos, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            xpos,
                            ypos,
                            zpos, true);
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            xpos,
                            ypos,
                            zpos, true);

                    TargetRectangleWidth =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex - 1].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                    TargetRectangleHeight =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex - 1].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();

                    //  WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorenr((int)DutPadInfos[0].MachineIndex.XIndex,
                    //(int)DutPadInfos[0].MachineIndex.YIndex);

                    //  if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    //      this.StageSupervisor().StageModuleState.WaferHighViewMove(
                    //          wcoord.GetX() + DutPadInfos[0].PadCenter.X.Value,
                    //          wcoord.GetY() + DutPadInfos[0].PadCenter.Y.Value,
                    //          Wafer.GetSubsInfo().ActualThickness);
                    //  else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    //      this.StageSupervisor().StageModuleState.WaferLowViewMove(
                    //          wcoord.GetX() + DutPadInfos[0].PadCenter.X.Value,
                    //          wcoord.GetY() + DutPadInfos[0].PadCenter.Y.Value,
                    //          Wafer.GetSubsInfo().ActualThickness);

                    //  TargetRectangleWidth =
                    //      DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                    //  TargetRectangleHeight =
                    //      DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();
                }

                StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";

                if (!this.WaferAligner().IsNewSetup)
                {
                    if (CurPadIndex == 1)
                    {
                        PadJogSelect.IsEnabled = true;
                    }
                    else
                    {
                        PadJogSelect.IsEnabled = false;
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

        private Task NextPad()
        {
            try
            {

                if (DutPadInfos.Count == 0)
                    return Task.CompletedTask;
                if (CurPadIndex + 1 <= DutPadInfos.Count)
                {
                    WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)DutPadInfos[CurPadIndex].MachineIndex.XIndex,
                                       (int)DutPadInfos[CurPadIndex].MachineIndex.YIndex);


                    double xpos = wcoord.GetX() + DutPadInfos[CurPadIndex].PadCenter.X.Value;
                    double ypos = wcoord.GetY() + DutPadInfos[CurPadIndex].PadCenter.Y.Value;
                    double zpos = this.WaferAligner().GetHeightValueAddZOffset(xpos, ypos, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            xpos,
                            ypos,
                            zpos, true);
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            xpos,
                            ypos,
                            zpos, true);

                    TargetRectangleWidth =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                    TargetRectangleHeight =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();

                    CurPadIndex++;
                }
                else if (CurPadIndex == DutPadInfos.Count)
                {
                    CurPadIndex = 1;

                    WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)DutPadInfos[0].MachineIndex.XIndex,
                                 (int)DutPadInfos[0].MachineIndex.YIndex);


                    double xpos = wcoord.GetX() + DutPadInfos[0].PadCenter.X.Value;
                    double ypos = wcoord.GetY() + DutPadInfos[0].PadCenter.Y.Value;
                    double zpos = this.WaferAligner().GetHeightValueAddZOffset(xpos, ypos, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            xpos,
                            ypos,
                            zpos, true);
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            xpos,
                            ypos,
                            zpos, true);

                    TargetRectangleWidth =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                    TargetRectangleHeight =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();
                }

                StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";

                if (!this.WaferAligner().IsNewSetup)
                {
                    if (CurPadIndex == 1)
                    {
                        PadJogSelect.IsEnabled = true;
                    }
                    else
                    {
                        PadJogSelect.IsEnabled = false;
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

        private AsyncCommand _ReAdjustRefPadCommand;
        public ICommand ReAdjustRefPadCommand
        {
            get
            {
                if (null == _ReAdjustRefPadCommand)
                    _ReAdjustRefPadCommand = new AsyncCommand(ReAdjustRefPad);
                return _ReAdjustRefPadCommand;
            }
        }

        private async Task ReAdjustRefPad()
        {
            try
            {
                WaferCoordinate wcoord = null;

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();


                MachineIndex mindex = this.CoordinateManager().GetCurMachineIndex(wcoord);

                //DutWaferIndex dutinfo = this.WaferAligner().GetDutDieIndexs().FirstOrDefault(dut => dut.WaferIndex.XIndex == mindex.XIndex && dut.WaferIndex.YIndex == mindex.YIndex);

                //long dutnum = -1;

                //if (dutinfo != null)
                //{
                //    dutnum = dutinfo.DutNumber;
                //}

                //if (dutnum == -1)
                //{
                //    await this.MetroDialogManager().ShowMessageDialog("Error Message", "Dut information does not exist.", EnumMessageStyle.Affirmative);
                //    return ;
                //}

                //if (InvalidationMatchDutIdex(mindex))
                //{
                DUTPadObject padparam = new DUTPadObject();
                padparam.PadSizeX.Value = GetDisplayPortRectInfo().Width * CurCam.GetRatioX();
                padparam.PadSizeY.Value = GetDisplayPortRectInfo().Height * CurCam.GetRatioY();
                padparam.PadShape.Value = EnumPadShapeType.SQUARE;
                padparam.MachineIndex = mindex;

                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();

                Point pt = this.WaferAligner().GetLeftCornerPosition(wcoord);

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;
                padparam.PadCenter.X.Value = Math.Round(wcoord.GetX(), 1) - Math.Round(CurDieLeftCorner.GetX(), 1);
                padparam.PadCenter.Y.Value = Math.Round(wcoord.GetY(), 1) - Math.Round(CurDieLeftCorner.GetY(), 1);
                var refpad = DutPadInfos.SingleOrDefault(info => info.PadNumber.Value == 1);

                if (DutPadInfos.Count == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Pad Information Invalid", "Please Check Registered Pad Information", EnumMessageStyle.Affirmative);
                    return;
                }

                this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX = padparam.PadCenter.X.Value - refpad.PadCenter.X.Value;
                this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY = padparam.PadCenter.Y.Value - refpad.PadCenter.Y.Value;

                WaferCoordinate patterncenter = this.WaferAligner().GetPatternWaferCenter();


                //low pattern change

                foreach (var pad in WaferObject.GetSubsInfo().Pads.DutPadInfos)
                {
                    if (CurCam.Param.VerticalFlip.Value == FlipEnum.FLIP)
                    {
                        pad.PadCenter.X.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX;
                        pad.PadCenter.Y.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY;
                    }
                    else
                    {
                        pad.PadCenter.X.Value += this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX;
                        pad.PadCenter.Y.Value -= this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY;
                    }
                }

                this.WaferAligner().SetRefPad(Wafer.GetSubsInfo().Pads.DutPadInfos);
                // }

                CurCam.UpdateOverlayFlag = true;
                Wafer.GetSubsInfo().Pads.DutPadInfos = this.DutPadInfos;
                EventCodeEnum retVal = Execute();
                if (retVal == EventCodeEnum.NONE)
                {
                    WaferObject.SetAlignState(AlignStateEnum.IDLE);
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.SetRefPadFalg = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private AsyncCommand _CompareRefPadCommand;
        public ICommand CompareRefPadCommand
        {
            get
            {
                if (null == _CompareRefPadCommand)
                    _CompareRefPadCommand = new AsyncCommand(
    CompareRefPad, false//, EvaluationPrivilege.Evaluate(
                        // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                        // new Action(() => { ShowMessages("UIModeChange"); })
    );
                return _CompareRefPadCommand;
            }
        }

        private async Task CompareRefPad()
        {
            try
            {
                ModifyCondition = WAPadSetupFunction.COMPAREREFPAD;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private async Task Resume()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                EnumMessageDialogResult ret =
                    await this.MetroDialogManager().ShowMessageDialog("Do you want to update the recovered data?",
                    "Click OK to update, No to not save the recovery data.",
                    EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary);

                List<ISubModule> moduls = this.WaferAligner().Template.GetProcessingModule();

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    foreach (var module in moduls)
                    {
                        if (module is IRecovery)
                        {
                            retVal = module.ExitRecovery();
                            if (retVal != EventCodeEnum.NONE)
                                break;
                        }
                    }
                }
                else if (ret == EnumMessageDialogResult.NO)
                {
                    foreach (var module in moduls)
                    {
                        if (module is IRecovery)
                        {
                            retVal = module.ExitRecovery();
                            if (retVal != EventCodeEnum.NONE)
                                break;
                        }
                    }
                }
                Wafer.StopDrawDieOberlay(CurCam);
                CurCam.DrawDisplayDelegate = null;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public async Task WaferAlignExecute()
        {
            ICamera curcam = CurCam;

            List<LightValueParam> lights = new List<LightValueParam>();

            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                try
                {
                    UseUserControl = UserControlFucEnum.DEFAULT;

                    StopDrawDut();
                    MiniViewTarget = Wafer;
                    ShowPad = true;
                    ShowPin = false;
                    base.EnableDragMap = false;
                    ShowSelectedDut = false;
                    ShowGrid = false;
                    ShowCurrentPos = true;

                    if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Error Message", "Markalign can not behave as a failure. Please check Mark .", EnumMessageStyle.Affirmative);
                        return;
                    }

                    foreach (var light in curcam.LightsChannels)
                    {
                        lights.Add(new LightValueParam(light.Type.Value, (ushort)curcam.GetLight(light.Type.Value)));
                    }

                    retVal = this.WaferAligner().DoManualOperation();

                    if (retVal == EventCodeEnum.WAFER_NOT_EXIST_EROOR)
                    {
                        var ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Fail Wafer alignment - Not Exist Wafer.", EnumMessageStyle.Affirmative);
                        return;
                    }
                    else if (retVal == EventCodeEnum.MARK_Move_Failure)
                    {
                        var ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Fail Wafer alignment - Mark Align Fail.", EnumMessageStyle.Affirmative);
                        return;
                    }
                    //else if (retVal == EventCodeEnum.WAFER_NOT_STANDARD_TYPE)
                    //{
                    //    var ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Fail Wafer alignment - Not Stadndard Type Wafer.", EnumMessageStyle.Affirmative);
                    //}
                    else
                    {
                        if (this.GetParam_Wafer().GetAlignState() != AlignStateEnum.DONE)
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, WaferObject.GetSubsInfo().ActualThickness);

                            this.WaferAligner().SetSetupState();
                            if(retVal == EventCodeEnum.WAFERALIGN_TEMP_DEVIATION_OUTOFRANGE)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("WaferAlign Fail", "The current temperature and the set temperature are not the same.\nPlease check the temperature.", EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("WaferAlign Fail", "Please check WaferAlign Setup.", EnumMessageStyle.Affirmative);
                            }
                            
                            return;
                        }
                    }


                    if (this.GetParam_Wafer().GetAlignState() != AlignStateEnum.DONE | DutPadInfos.Count == 0)
                        retVal = this.WaferAligner().SetTeachDevice(true, -1, -1, curcam.GetChannelType());
                    else
                        retVal = this.WaferAligner().SetTeachDevice(false, -1, -1, curcam.GetChannelType());

                    CurPadIndex = 0;

                    if (DutPadInfos.Count != 0)
                    {
                        WaferCoordinate wcoord = null;

                        //wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorenr((int)this.WaferAligner().GetDutDieIndexs()[0].WaferIndex.XIndex,
                        //(int)this.WaferAligner().GetDutDieIndexs()[0].WaferIndex.YIndex);

                        wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex,
                                    this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex);


                        //Point pt = this.WaferAligner().PositionFromLeftBottomCorner(wcoord.X.Value, wcoord.Y.Value);

                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                wcoord.GetX() + DutPadInfos[0].PadCenter.X.Value,
                                wcoord.GetY() + DutPadInfos[0].PadCenter.Y.Value,
                                Wafer.GetSubsInfo().ActualThickness);
                        else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(
                                wcoord.GetX() + DutPadInfos[0].PadCenter.X.Value,
                                wcoord.GetY() + DutPadInfos[0].PadCenter.Y.Value,
                                Wafer.GetSubsInfo().ActualThickness);

                        TargetRectangleWidth =
                            DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                        TargetRectangleHeight =
                            DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();

                        CurPadIndex = 1;

                    }

                    retVal = EventCodeEnum.NONE;
                    StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";

                    if (this.WaferAligner().GetIsModify() == false)
                    {
                        OneButton.IsEnabled = true;
                        TwoButton.IsEnabled = true;
                        FiveButton.IsEnabled = true;
                    }

                    this.WaferAligner().SetSetupState();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    this.VisionManager().StartGrab(curcam.GetChannelType(), this);

                    CalcDutCen();

                    foreach (var light in lights)
                    {
                        curcam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    MiniViewTarget = this.GetParam_ProbeCard();
                    DrawDutPad();
                    UseUserControl = UserControlFucEnum.PTRECT;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public Task FocusingCmdFunc()
        {
            try
            {

                FocusParam.FocusingCam.Value = CurCam.GetChannelType();

                WaferPadFocusModel.Focusing_Retry(FocusParam, false, true, false, this);

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    Wafer.GetSubsInfo().ActualThickness = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetZ();
                }
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    Wafer.GetSubsInfo().ActualThickness = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert().GetZ();
                }

                Wafer.GetSubsInfo().ActualThickness =
                                       Wafer.GetSubsInfo().ActualThickness - this.WaferAligner().CalcThreePodTiltedPlane(WaferObject.GetSubsInfo().WaferCenter.GetX(), WaferObject.GetSubsInfo().WaferCenter.GetY(), true);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

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
        //==============

        public Task MoveToTeachDie()
        {
            try
            {

                WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner(this.WaferAligner().TeachDieXIndex, this.WaferAligner().TeachDieYIndex);
                coordinate.X.Value += (Wafer.GetSubsInfo().DieXClearance.Value / 2);
                coordinate.Y.Value += (Wafer.GetSubsInfo().DieYClearance.Value / 2);
                this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX(), coordinate.GetY(), Wafer.GetSubsInfo().ActualThickness);
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


        #endregion


        private IParam _PadStandardParam_IParam;
        [ParamIgnore]
        public IParam PadStandardParam_IParam
        {
            get { return _PadStandardParam_IParam; }
            set
            {
                if (value != _PadStandardParam_IParam)
                {
                    _PadStandardParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if (Initialized == false)
                    {
                        PadReg = this.PadRegist();

                        SubModuleState = new SubModuleIdleState(this);
                        MovingState = new SubModuleStopState(this);
                        SetupState = new NotCompletedState(this);

                        //CurCam = this.VisionManager().GetCam(PadStandardParam_Clone.CamType);
                        InitPnpModuleStage();
                        Initialized = true;
                        MotionManager = this.MotionManager();
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retval;
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }



        public EventCodeEnum DoExecute()
        {
            try
            {

                //Calc PadCenter (WaferCoordinate)
                //List<double> padXPos = new List<double>();
                //List<double> padYPos = new List<double>();
                //DutWaferIndex StandradDut = null;

                //if (Wafer.DutDieMatchIndexs == null)
                //    this.WaferAligner().SetTeachDevice();

                //StandradDut = Wafer.DutDieMatchIndexs.Find(dut => dut.DutNumber == 1);

                //if (StandradDut != null)
                //{
                //    WaferCoordinate standarddutleftcornerwcd = this.WaferAligner().MachineIndexConvertToDieLeftCorenr(StandradDut.WaferIndex.XIndex, StandradDut.WaferIndex.YIndex);

                //    if (Wafer.GetSubsInfo().Pads.DutPadInfos != null)
                //    {
                //        foreach (var pad in Wafer.GetSubsInfo().Pads.DutPadInfos)
                //        {
                //            WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorenr(pad.MachineIndex.XIndex, pad.MachineIndex.YIndex);
                //            padXPos.Add((wcoord.GetX() + pad.PadCenter.GetX()) - standarddutleftcornerwcd.GetX());
                //            padYPos.Add((wcoord.GetY() + pad.PadCenter.GetY()) - standarddutleftcornerwcd.GetY());

                //            pad.PadCenterRef.X.Value = padXPos[padXPos.Count() - 1];
                //            pad.PadCenterRef.Y.Value = padYPos[padYPos.Count() - 1];
                //        }

                //        if (padXPos.Count != 0)
                //            Wafer.GetSubsInfo().Pads.PadCenX = padXPos.Min() + ((padXPos.Max() - padXPos.Min()) / 2);
                //        if (padYPos.Count != 0)
                //            Wafer.GetSubsInfo().Pads.PadCenY = padYPos.Min() + ((padYPos.Max() - padYPos.Min()) / 2);
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            SubModuleState = new SubModuleDoneState(this);
            return EventCodeEnum.NONE;
        }



        public bool IsLotReady(out string msg)
        {
            bool retVal = false;
            try
            {
                msg = null;
                try
                {
                    //if (ParamValidation() == EventCodeEnum.NONE)
                    //    retVal = true;
                    //else
                    //{
                    //    msg = "Not Exist Pad.";
                    //}

                    if (this.WaferAligner().IsNewSetup || this.WaferAligner().GetIsModifySetup())
                    {
                        retVal = true;
                    }
                    else
                    {
                        // 최소 등록 개수 
                        if (this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count >= 4)
                        {
                            retVal = true;
                        }
                        else if (this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count > 0)
                        {
                            msg = "Not enough pad.";
                            retVal = false;
                        }
                        else
                        {
                            msg = "Not Exist Pad.";
                            retVal = false;
                        }
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
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }
        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX = 0.0;
                //this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY = 0.0;
                retVal = ParamValidation();
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
            return SubModuleState.Recovery();
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
            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum DoExitRecovery()
        {
            return EventCodeEnum.NONE;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = Properties.Resources.PadRegistrationHeader;
                RecoveryHeader = Properties.Resources.RecoveryHeader;
                //if (AdvenceSettingDialog == null)
                //    AdvenceSettingDialog = new PadAdvenSettingControl(this);
                //retVal = InitPnpModuleStage();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        bool oripadsetuptoggle = false;


        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IsChanged = false;

                //InitPnpModuleStage();
                //Wafer.WaferDevObject.CalcWaferCenterIndex();
                SideViewDisplayMode = SideViewMode.NOUSE;
                SideViewExpanderVisibility = SideViewExpanderVisibility = false;
                SideViewSwitchVisibility = Visibility.Hidden;

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                DutPadInfos = new List<DUTPadObject>(Wafer.GetSubsInfo().Pads.DutPadInfos);
                oripadsetuptoggle = WaferObject.PadSetupChangedToggle.Value;

                CurCam = this.VisionManager().GetCam(PadStandardParam_Clone.CamType);
                //if ((this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING
                //                     & this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.PAUSED) 
                //                     | this.WaferAligner().IsNewSetup)
                InitStateUI();

                if (this.WaferAligner().IsNewSetup)
                {
                    retVal = await InitSetup();
                }
                else
                {
                    retVal = await InitRecovery();
                }


                ushort defaultlightvalue = 85;

                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                InitLightJog(this);
                DisplayClickToMoveEnalbe = true;
                SetStepSetupState();
                CurCam.UpdateOverlayFlag = true;
                //FiveButton.Caption = "SW MV";
                //FiveButton.Command = new RelayCommand(SwitchMiniView);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private void SwitchMiniView()
        {
            if (MiniViewTarget is IWaferObject)
                MiniViewTarget = this.GetParam_ProbeCard();
            else
                MiniViewTarget = this.WaferObject;
        }
        public void CalcDutCen()
        {
            try
            {
                long xMinIndex = Wafer.WaferDevObject.Info.DutDieMatchIndexs.Min(dev => dev.WaferIndex.XIndex);
                long xMaxIndex = Wafer.WaferDevObject.Info.DutDieMatchIndexs.Max(dev => dev.WaferIndex.XIndex);
                long yMinIndex = Wafer.WaferDevObject.Info.DutDieMatchIndexs.Min(dev => dev.WaferIndex.YIndex);
                long yMaxIndex = Wafer.WaferDevObject.Info.DutDieMatchIndexs.Max(dev => dev.WaferIndex.YIndex);

                double xMaxPos = this.WaferAligner().MachineIndexConvertToDieCenter(xMaxIndex, 0).GetX();
                double xMinPos = this.WaferAligner().MachineIndexConvertToDieCenter(xMinIndex, 0).GetX();
                double yMaxPos = this.WaferAligner().MachineIndexConvertToDieCenter(0, yMaxIndex).GetY();
                double yMinPos = this.WaferAligner().MachineIndexConvertToDieCenter(0, yMinIndex).GetY();

                double xOffset = (xMaxPos - xMinPos) / 2;
                double yOffset = (yMaxPos - yMinPos) / 2;

                Wafer.GetSubsInfo().DutCenX = xMaxPos - xOffset;
                Wafer.GetSubsInfo().DutCenY = yMaxPos - yOffset;

                //loader에 dutviewcontrol의 현재 위치 표시를 위해 dut center 정보를 알려주어야 함
                this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedWaferObjectDutCenter(Wafer.GetSubsInfo().DutCenX, Wafer.GetSubsInfo().DutCenY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                UseUserControl = UserControlFucEnum.PTRECT;
                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;


                MainViewTarget = DisplayPort;

                Wafer.MapViewControlMode = MapViewMode.MapMode;

                this.WaferAligner().SetDefaultDutDieIndexs();
                this.WaferAligner().SetTeachDevice();
                CalcDutCen();

                // DutView 오브젝트 
                MiniViewTarget = this.GetParam_ProbeCard();
                ShowPad = true;
                ShowPin = false;
                base.EnableDragMap = false;
                ShowSelectedDut = false;
                ShowGrid = false;
                ShowCurrentPos = true;

                //DutViewer = new PadRegDutViewerViewModel(new Size(MiniViewTargetWidth, MiniViewTargetHeight));
                //MiniViewTarget = DutViewer;
                //DisplayPort.RegistMouseDownEvent(DutViewer.MainScreen_MouseDown);

                DrawDutPad();
                CurCam.UpdateOverlayFlag = true;
                CurPadIndex = 0;


                DutPadInfos = new List<DUTPadObject>(Wafer.GetSubsInfo().Pads.DutPadInfos);
                if (DutPadInfos.Count != 0)
                {
                    WaferCoordinate wcoord = null;

                    wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)this.WaferAligner().GetDutDieIndexs()[0].WaferIndex.XIndex,
                        (int)this.WaferAligner().GetDutDieIndexs()[0].WaferIndex.YIndex);




                    double xpos = wcoord.GetX() + DutPadInfos[0].PadCenter.X.Value;
                    double ypos = wcoord.GetY() + DutPadInfos[0].PadCenter.Y.Value; 
                    double zpos = this.WaferAligner().GetHeightValueAddZOffset(xpos, ypos, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            xpos,
                            ypos,
                            zpos, true);
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            xpos,
                            ypos,
                            zpos, true);

                    TargetRectangleWidth =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                    TargetRectangleHeight =
                        DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();

                    CurPadIndex = 1;

                }
                else
                {
                    this.WaferAligner().SetIsNewSetup(true);
                }

                retVal = InitPNPSetupUI();
                StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";
                if (this.WaferObject.GetAlignState() != AlignStateEnum.DONE)
                {
                    OneButton.IsEnabled = false;
                    TwoButton.IsEnabled = false;
                }
                //MiniViewTarget = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //MiniViewTarget = Wafer;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Delete.png");

                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rect-outline.png");
                FiveButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rectangle-plus.png");

                //FiveButton.Caption = "Toogle";

                PadJogRightDown.Caption = "Set TeachDie";
                PadJogRightDown.CaptionSize = 17;
                PadJogRightDown.Command = new AsyncCommand(SetTeachDie);

                PadJogLeftUp.IconCaption = "PAD";
                PadJogRightUp.IconCaption = "PAD";
                PadJogLeftUp.Command = new AsyncCommand(PrevPad);
                PadJogRightUp.Command = new AsyncCommand(NextPad);
                PadJogLeft.Command = new AsyncCommand(PatternWidthMinus, false);
                PadJogRight.Command = new AsyncCommand(PatternWidthPlus, false);
                PadJogUp.Command = new AsyncCommand(PatternHeightPlus, false);
                PadJogDown.Command = new AsyncCommand(PatternHeightMinus, false);


                PadJogRight.RepeatEnable = true;
                PadJogLeft.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                OneButton.IconCaption = Properties.Resources.Btn_Add;
                OneButton.Command = AddCommand;



                TwoButton.IconCaption = Properties.Resources.Btn_Delete;
                TwoButton.Command = DeleteCommand;

                ThreeButton.IconCaption = Properties.Resources.Btn_Focus;
                ThreeButton.Command = new AsyncCommand(FocusingCmdFunc);


                FourButton.IconCaption = Properties.Resources.Btn_AutoPadSize;
                //FourButton.IconCaption = "AutoPad";
                FourButton.Command = new AsyncCommand(MoveToPad);

                FiveButton.Caption = "";
                FiveButton.IconCaption = "Auto Comp.";
                FiveButton.Command = TogglePadCompCommand;
                EnablePADCompensation = true;
                PadJogLeftDown.Caption = Properties.Resources.Btn_DeleteAll;
                PadJogLeftDown.Command = new RelayCommand(DeleteAllPad);


                PadJogSelect.IconCaption = Properties.Resources.Btn_Align;
                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Wafer.png");
                PadJogSelect.Command = new AsyncCommand(WaferAlignExecute);
                //(PadJogSelect.Command as AsyncCommand).SetCancelTokenPack(PadJogSelectTokenPack);

                SetMotionJogMoveZOffsetEnable(true);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
        }

        private EventCodeEnum InitRecoveryPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //..Recovery UI
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeftUp.IconCaption = "PAD";
                PadJogRightUp.IconCaption = "PAD";

                PadJogLeftUp.Command = new AsyncCommand(PrevPad);
                PadJogRightUp.Command = new AsyncCommand(NextPad);
                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rect-outline.png");
                OneButton.IconCaption = Properties.Resources.Btn_AutoPadSize;
                OneButton.Command = new AsyncCommand(MoveToPad);
                OneButton.IsEnabled = true;

                PadJogSelect.IconCaption = "Set Pad";
                PadJogSelect.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rect-outline-multi.png");
                PadJogSelect.Command = ReAdjustRefPadCommand;
                PadJogSelect.Visibility = Visibility.Visible;
                PadJogSelect.IsEnabled = true;

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new WA_PadParam_Standard();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(WA_PadParam_Standard));

                if (retVal == EventCodeEnum.NONE)
                {
                    PadStandardParam_IParam = tmpParam;
                    PadStandardParam_Clone = PadStandardParam_IParam as WA_PadParam_Standard;
                }

                if (FocusParam != null)
                {
                    FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }


            return retVal;
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {
                    //Data
                    if (this.WaferAligner().WaferAlignInfo.RecoveryParam.BackupDutPadInfos.Count == Wafer.GetSubsInfo().Pads.DutPadInfos.Count)
                    {
                        for (int i = 0; i < this.WaferAligner().WaferAlignInfo.RecoveryParam.BackupDutPadInfos.Count; i++)
                        {
                            Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.X.Value = this.WaferAligner().WaferAlignInfo.RecoveryParam.BackupDutPadInfos[i].PadCenter.X.Value;
                            Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.Y.Value = this.WaferAligner().WaferAlignInfo.RecoveryParam.BackupDutPadInfos[i].PadCenter.Y.Value;
                        }
                        this.WaferAligner().WaferAlignInfo.RecoveryParam.BackupDutPadInfos.Clear();
                    }

                    foreach (var pad in Wafer.GetSubsInfo().Pads.DutPadInfos)
                    {
                        DUTPadObject padBackup = new DUTPadObject();
                        pad.CopyTo(padBackup);
                        this.WaferAligner().WaferAlignInfo.RecoveryParam.BackupDutPadInfos.Add(padBackup);
                    }

                    this.WaferAligner().SetRefPad(Wafer.GetSubsInfo().Pads.DutPadInfos);

                    UseUserControl = UserControlFucEnum.PTRECT;
                    TargetRectangleWidth = 128;
                    TargetRectangleHeight = 128;


                    MainViewTarget = DisplayPort;

                    Wafer.MapViewControlMode = MapViewMode.MapMode;
                    Wafer.DrawDieOverlay(CurCam);

                    this.WaferAligner().SetDefaultDutDieIndexs();
                    this.WaferAligner().SetTeachDevice();
                    CalcDutCen();

                    // DutView 오브젝트 
                    MiniViewTarget = this.GetParam_ProbeCard();
                    ShowPad = true;
                    ShowPin = false;
                    base.EnableDragMap = false;
                    ShowSelectedDut = false;
                    ShowGrid = false;
                    ShowCurrentPos = true;

                    DrawDutPad();
                    CurCam.UpdateOverlayFlag = true;
                    CurPadIndex = 0;

                    DutPadInfos = new List<DUTPadObject>(Wafer.GetSubsInfo().Pads.DutPadInfos);

                    if (DutPadInfos.Count != 0)
                    {
                        WaferCoordinate wcoord = null;

                        wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(
                            (int)Wafer.GetSubsInfo().Pads.RefPad.MachineIndex.XIndex,
                            (int)Wafer.GetSubsInfo().Pads.RefPad.MachineIndex.YIndex);

                        double refpadx = wcoord.GetX() + Wafer.GetSubsInfo().Pads.RefPad.PadCenter.GetX();
                        double refpady = wcoord.GetY() + Wafer.GetSubsInfo().Pads.RefPad.PadCenter.GetY();
                        double refpadz = this.WaferAligner().GetHeightValueAddZOffset(refpadx, refpady, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);


                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                refpadx,
                                refpady,
                                refpadz, true);
                        else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(
                                refpadx,
                                refpady,
                                refpadz, true);

                        this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgRefPadPoint = new Point(refpadx, refpady);

                        TargetRectangleWidth =
                            DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
                        TargetRectangleHeight =
                            DisplayPort.ConvertDisplayWidth(DutPadInfos[0].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();

                        CurPadIndex = 1;

                        StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";
                    }

                    if (SetupState.GetState() == EnumMoudleSetupState.NOTCOMPLETED)
                    {
                        this.WaferAligner().WaferAlignInfo.RecoveryParam.SetRefPadFalg = false;
                    }
                    else
                    {
                        this.WaferAligner().WaferAlignInfo.RecoveryParam.SetRefPadFalg = true;
                    }

                    InitRecoveryPNPSetupUI();

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    //LoggerManager.Error($err + "InitRecovery() : Error occured.");
                }
                return Task.FromResult<EventCodeEnum>(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    DoExecute();
                    SubModuleState = new SubModuleIdleState(this);

                    if (PadStandardParam_IParam != null)
                    {
                        this.SaveParameter(PadStandardParam_IParam);
                    }
                    Wafer.GetSubsInfo().Pads.DutPadInfos = this.DutPadInfos;

                    this.StageSupervisor().SaveWaferObject();
                }
                catch (Exception err)
                {
                    throw err;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                throw err;
            }
            return retVal;
        }

        #region ..//Command & CommandMethod                                 

        private AsyncCommand _AutoPadSearchCommand;
        public ICommand AutoPadSearchCommand
        {
            get
            {
                if (null == _AutoPadSearchCommand)
                    _AutoPadSearchCommand = new AsyncCommand(AutoPadSearch);
                return _AutoPadSearchCommand;
            }
        }

        private Task<EventCodeEnum> AutoPadSearch()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            try
            {

                //PadParameter padparam = new PadParameter();
                //RegistePatternParam param = GetPatternRectInfo();

                ////padparam.PadSizeX = 81;
                ////padparam.PadSizeY = 84;

                //padparam.PadShape = EnumPadShapeType.SQUARE;
                //padparam.PadColor = EnumPadColorType.WHITE;

                ////PadReg.AddPad(padparam);

                //Task<EventCodeEnum> stateTask;
                //stateTask = Task.Run(() => PadReg.AllPadSearch());
                //await stateTask;
                //retval = stateTask.Result;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(retval);
        }

        private RelayCommand<object> _SaveWaferSetupStatusCommand;
        public ICommand SaveWaferSetupStatusCommand
        {
            get
            {
                if (null == _SaveWaferSetupStatusCommand)
                    _SaveWaferSetupStatusCommand = new RelayCommand<object>(SaveWaferSetupStatus);
                return _SaveWaferSetupStatusCommand;
            }
        }

        private void SaveWaferSetupStatus(object parameter)
        {
        }

        private AsyncCommand _DieAllSearchCommand;
        public ICommand DieAllSearchCommand
        {
            get
            {
                if (null == _DieAllSearchCommand)
                    _DieAllSearchCommand = new AsyncCommand(DieAllSearch);
                return _DieAllSearchCommand;
            }
        }
        private async Task<EventCodeEnum> DieAllSearch()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            try
            {
                PadObject padparam = new PadObject();
                //RegistePatternParam param = GetPatternRectInfo();
                //padparam.PadSizeX = 81;
                //padparam.PadSizeY = 84;
                padparam.PadShape.Value = EnumPadShapeType.SQUARE;
                padparam.PadColor.Value = EnumPadColorType.WHITE;

                //PadReg.AddPad(padparam);

                //Task<EventCodeEnum> stateTask;
                //stateTask = Task.Run(() => PadReg.DieAllSearch());
                //await stateTask;
                //retval = stateTask.Result;

                retval = await PadReg.DieAllSearch();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        #endregion

        public PadStandard()
        {

        }

        public async Task<EventCodeEnum> Modify()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {


                PatternSizeWidth = TargetRectangleWidth;
                PatternSizeHeight = TargetRectangleHeight;
                PatternSizeLeft = TargetRectangleLeft;
                PatternSizeTop = TargetRectangleTop;


                switch (ModifyCondition)
                {
                    case WAPadSetupFunction.REGPAD:

                        if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "Markalign can not behave as a failure. Please check Mark .", EnumMessageStyle.Affirmative);
                            return RetVal;
                        }
                        RetVal = await RegistPad();
                        WaferObject.PadSetupChangedToggle.Value = !oripadsetuptoggle;
                        SetStepSetupState();
                        this.GetParam_ProbeCard().SetAlignState(AlignStateEnum.IDLE);
                        this.GetParam_ProbeCard().SetPinPadAlignState(AlignStateEnum.IDLE);
                        //Wafer.DrawPadOverlay(CurCam);
                        break;
                    case WAPadSetupFunction.DELETEPAD:

                        RetVal = await DeletePad();
                        WaferObject.PadSetupChangedToggle.Value = !oripadsetuptoggle;
                        SetStepSetupState();
                        this.GetParam_ProbeCard().SetAlignState(AlignStateEnum.IDLE);
                        this.GetParam_ProbeCard().SetPinPadAlignState(AlignStateEnum.IDLE);
                        break;
                    case WAPadSetupFunction.CHANGEPATTERNWIDTH:
                        PatternSizeWidth += _ChangeWidthValue;
                        PatternSizeLeft -= (_ChangeWidthValue / 2);
                        TargetRectangleWidth = PatternSizeWidth;
                        PatternSizeWidth = TargetRectangleWidth;
                        break;
                    case WAPadSetupFunction.CHANGEDPATTRNHEIGHT:
                        PatternSizeHeight += _ChangeHeightValue;
                        PatternSizeTop -= (_ChangeHeightValue / 2);
                        TargetRectangleHeight = PatternSizeHeight;
                        PatternSizeHeight = TargetRectangleHeight;
                        break;
                    case WAPadSetupFunction.COMPAREREFPAD:

                        RetVal = await ComparedRefPad();
                        break;
                    case WAPadSetupFunction.MOVETOPAD:

                        if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "Mark alignment Failed. Please check Ref. Mark status", EnumMessageStyle.Affirmative);
                            return RetVal;
                        }
                        MoveToPad(null);
                        break;
                }
                WaferObject.GetSubsInfo().Pads.DutPadInfos = DutPadInfos;
                this.WaferAligner().UpdatePadCen();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return RetVal;
        }

        private async Task<EventCodeEnum> RegistPad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //Wafer.SetAlignState(AlignStateEnum.DONE); 

                if (Wafer.GetAlignState() != AlignStateEnum.DONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog
                        (Properties.Resources.DoWaferAlignErrorTitle,
                        Properties.Resources.DoWaferAlignErrorMessage,
                        EnumMessageStyle.Affirmative);
                    return retVal;
                }

                WaferCoordinate wcoord = null;
                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

                #region // Regist PAD pattern and excute find pattern with Mark align.
                var camType = CurCam.GetChannelType();
                WAStandardPTInfomation patterninfo = new WAStandardPTInfomation();
                RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();


                patterninfo.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();
                patterninfo.CamType.Value = CurCam.GetChannelType();

                var logRootPath = this.FileManager().GetLogRootPath();
                var targetPath = string.Format($"{logRootPath}\\ImageLogs\\");
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                if (EnablePADCompensation == true)
                {
                    patterninfo.PMParameter.ModelFilePath.Value = targetPath + "PadPattern";
                    patterninfo.PMParameter.PatternFileExtension.Value = ".mmo";
                    double pattRatio = 1.5;
                    double pattWidth = patternparam.Width * pattRatio;
                    double pattHeight = patternparam.Height * pattRatio;
                    double locX = patternparam.LocationX - (pattWidth - patternparam.Width) / 2;
                    double locY = patternparam.LocationY - (pattHeight - patternparam.Height) / 2;
                    patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(this.VisionManager().SingleGrab(patterninfo.CamType.Value, this), (int)locX, (int)locY, (int)pattWidth, (int)pattHeight);


                    patterninfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;
                    patterninfo.HorDirection.Value = EnumHorDirection.LEFTRIGHT;
                    patterninfo.VerDirection.Value = EnumVerDirection.UPPERBOTTOM;
                    patterninfo.WaferCenter = new WaferCoordinate();
                    wcoord.CopyTo(patterninfo.WaferCenter);
                    for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                    {
                        patterninfo.LightParams.Add(
                            new LightValueParam(CurCam.LightsChannels[lightindex].Type.Value,
                            (ushort)CurCam.GetLight(CurCam.LightsChannels[lightindex].Type.Value)));
                    }
                    retVal = this.StageSupervisor().MarkAligner().DoMarkAlign(true);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"RegistPad(): Mark alignment failed. Result = {retVal}");
                        await this.MetroDialogManager().ShowMessageDialog("Error Message", "PAD position compensation failed - Mark alignment failed.", EnumMessageStyle.Affirmative);

                        return EventCodeEnum.MARK_Focusing_Failure;
                    }

                    var waferThickness = this.WaferAligner().GetHeightValueAddZOffset(wcoord.GetX(), wcoord.GetY(), this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);
                    double ratioX = 1.0;
                    double ratioY = 1.0;
                    var camObject = this.VisionManager().GetCam(camType);
                    ratioX = camObject.GetRatioX();
                    ratioY = camObject.GetRatioY();
                    if (camType == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            wcoord.GetX(), wcoord.GetY(), waferThickness, true);

                    }

                    if (camType == EnumProberCam.WAFER_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            wcoord.GetX(), wcoord.GetY(), waferThickness, true);
                    }

                    this.VisionManager().StartGrab(patterninfo.CamType.Value, this);
                    System.Threading.Thread.Sleep(1000);

                    patterninfo.PMParameter.PMAcceptance.Value = 80;
                    var pmResult = this.VisionManager().PatternMatching(patterninfo, this);

                    if (pmResult.RetValue != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"RegistPad(): Pattern matching failed. Result = {retVal}");
                    }
                    else
                    {
                        if (pmResult.ResultParam != null)
                        {
                            if (pmResult.ResultParam.Count > 0)
                            {
                                var result = pmResult.ResultParam.First();
                                var patPos = new WaferCoordinate(wcoord.GetX(), wcoord.GetY());
                                LoggerManager.Debug($"RegistPad(): Pointed position in wafer coord = ({patPos.X.Value:0.00}, {patPos.Y.Value:0.00})", isInfo: true);
                                patPos.X.Value = patPos.X.Value + ((camObject.GetGrabSizeWidth() / 2) - (result.XPoss)) * ratioX;
                                patPos.Y.Value = patPos.Y.Value + ((camObject.GetGrabSizeHeight() / 2) - (result.YPoss)) * ratioY;
                                LoggerManager.Debug($"RegistPad(): Matched position in wafer coord = ({patPos.X.Value:0.00}, {patPos.Y.Value:0.00})", isInfo: true);

                                var dist = GeometryHelper.GetDistance2d(wcoord.GetX(), wcoord.GetY(), patPos.GetX(), patPos.GetY());
                                if (dist > GeometryHelper.GetDistance2d(0, 0, pattWidth / ratioX, pattHeight / ratioY))
                                {
                                    LoggerManager.Error($"RegistPad(): Invalid pad position. PAD position compensation failed.");
                                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "PAD position compensation failed - Out of Tolerence", EnumMessageStyle.Affirmative);
                                    return EventCodeEnum.PAD_AUTOCOMP_OUT_OF_TOL;
                                }
                                else
                                {
                                    waferThickness = this.WaferAligner().GetHeightValueAddZOffset(wcoord.GetX(), wcoord.GetY(), this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);
                                    if (camType == EnumProberCam.WAFER_HIGH_CAM)
                                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                            patPos.GetX(), patPos.GetY(), waferThickness, true);
                                    if (camType == EnumProberCam.WAFER_LOW_CAM)
                                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                                            patPos.GetX(), patPos.GetY(), waferThickness, true);
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"RegistPad(): Invalid pad position. PAD position compensation failed.");
                                await this.MetroDialogManager().ShowMessageDialog("Error Message", "PAD position compensation failed - Pattern matching Failed!", EnumMessageStyle.Affirmative);
                                return EventCodeEnum.PAD_AUTOCOMP_PM_FAILED;
                            }
                        }
                    }
                    this.VisionManager().StartGrab(patterninfo.CamType.Value, this);
                }
                #endregion

                MachineIndex mindex = this.CoordinateManager().GetCurMachineIndex(wcoord);
                //var dutnum = this.WaferAligner().GetDutDieIndexs().Find(dut => dut.WaferIndex.XIndex == mindex.XIndex
                //   && dut.WaferIndex.YIndex == mindex.YIndex).DutNumber;

                DutWaferIndex dutinfo = this.WaferAligner().GetDutDieIndexs().FirstOrDefault(dut => dut.WaferIndex.XIndex == mindex.XIndex && dut.WaferIndex.YIndex == mindex.YIndex);

                long dutnum = -1;

                if (dutinfo != null)
                {
                    dutnum = dutinfo.DutNumber;
                }

                if (dutnum == -1)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "Dut information does not exist.", EnumMessageStyle.Affirmative);
                    return EventCodeEnum.UNDEFINED;
                }

                if (InvalidationMatchDutIdex(mindex))
                {
                    DUTPadObject padparam = new DUTPadObject();
                    padparam.PadSizeX.Value = GetDisplayPortRectInfo().Width * CurCam.GetRatioX();
                    padparam.PadSizeY.Value = GetDisplayPortRectInfo().Height * CurCam.GetRatioY();
                    padparam.PadShape.Value = EnumPadShapeType.SQUARE;
                    padparam.MachineIndex = mindex;

                    WaferCoordinate CurDieLeftCorner = new WaferCoordinate();

                    Point pt = this.WaferAligner().GetLeftCornerPosition(wcoord);

                    CurDieLeftCorner.X.Value = pt.X;
                    CurDieLeftCorner.Y.Value = pt.Y;

                    padparam.PadCenter.X.Value = Math.Round(wcoord.GetX(), 1) - Math.Round(CurDieLeftCorner.GetX(), 1);
                    padparam.PadCenter.Y.Value = Math.Round(wcoord.GetY(), 1) - Math.Round(CurDieLeftCorner.GetY(), 1);
                    padparam.MIndexLCWaferCoord = CurDieLeftCorner;

                    padparam.DutMIndex = this.WaferAligner().GetDutDieIndexs().Find(dut => dut.WaferIndex.XIndex == mindex.XIndex && dut.WaferIndex.YIndex == mindex.YIndex).DutIndex;
                    padparam.DutNumber = this.WaferAligner().GetDutDieIndexs().Find(dut => dut.WaferIndex.XIndex == mindex.XIndex && dut.WaferIndex.YIndex == mindex.YIndex).DutNumber;

                    padparam.PadNumber.Value = FindMaxPadNumber() + 1;

                    //Invaildation Check
                    IList<DUTPadObject> pads = DutPadInfos.ToList<DUTPadObject>().FindAll(dut => dut.MachineIndex.XIndex == padparam.MachineIndex.XIndex && dut.MachineIndex.YIndex == padparam.MachineIndex.YIndex);
                    int index = InvalidationExistPad(padparam, pads);

                    if (index == -1 || Extensions_IParam.ProberExecuteMode == ExecuteMode.ENGINEER)
                    //if (true) //index == -1)            // 엔지니어링 목적으로만 허용 되어야 함.
                    {
                        IsChanged = true;

                        DutPadInfos.Add(padparam);
                        Wafer.GetSubsInfo().Pads.DutPadInfos = this.DutPadInfos;

                        if (DutPadInfos.Count == 1)
                        {
                            Wafer.GetSubsInfo().Pads.RefPad = new DUTPadObject();
                            Wafer.GetSubsInfo().Pads.RefPad = padparam;
                        }
                        //WaferObject.GetSubsInfo().Pads.DutPadInfos = DutPadInfos;
                        this.WaferAligner().SetRefPad(DutPadInfos);
                        StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {++CurPadIndex}";

                        if (this.LoaderRemoteMediator() != null)
                        {
                            byte[] dutpadinfos = null;

                            dutpadinfos = this.ObjectToByteArray(DutPadInfos);

                            this.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateDutPadInfos(dutpadinfos);
                        }
                    }
                    else
                    {
                        //이미 있는 패드로 간주
                        var retmsg = await this.MetroDialogManager().ShowMessageDialog(
                            "Information Message",
                            "The location of the pad already exists. Do you want to update the existing pad location?",
                            EnumMessageStyle.AffirmativeAndNegative);

                        if (retmsg == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            IsChanged = true;

                            //Pad Position update
                            var originpad = DutPadInfos.Find(padobj => padobj.PadNumber.Value == index);
                            if (originpad != null)
                            {
                                padparam.Index.Value = originpad.Index.Value;
                                padparam.PadNumber.Value = originpad.PadNumber.Value;
                                originpad.PadCenter = padparam.PadCenter;
                                originpad.PadCenterRef = padparam.PadCenterRef;
                                originpad = padparam;
                                this.WaferAligner().SetRefPad(DutPadInfos);
                            }
                        }
                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessage,
                        Properties.Resources.RegistePadRangeErrorMessage, EnumMessageStyle.Affirmative);

                    WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieLeftCorner(
                        this.WaferAligner().TeachDieXIndex, this.WaferAligner().TeachDieYIndex);

                    double zpos = this.WaferAligner().GetHeightValueAddZOffset(coordinate.GetX(), coordinate.GetY(), this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);
                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX(), coordinate.GetY(), zpos, true);
                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(coordinate.GetX(), coordinate.GetY(), zpos, true);
                    StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {DutPadInfos.Count}";
                }


                CurCam.UpdateOverlayFlag = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (DutPadInfos.Count > 0)
                    Wafer.PadSetupChangedToggle.DoneState = ElementStateEnum.DONE;
            }
            return retVal;
        }

        public int FindMaxPadNumber()
        {
            if (DutPadInfos.Count == 0)
            {
                return 0;
            }
            int maxAge = int.MinValue;
            try
            {
                foreach (var type in DutPadInfos)
                {
                    if (type.PadNumber.Value > maxAge)
                    {
                        maxAge = type.PadNumber.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return maxAge;
        }

        private async Task<EventCodeEnum> DeletePad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                WaferCoordinate wcoord = null;

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

                MachineIndex mindex = this.CoordinateManager().GetCurMachineIndex(wcoord);

                if (InvalidationMatchDutIdex(mindex))
                {
                    int index = CurPadIndex - 1;

                    if (index > -1)
                    {
                        IsChanged = true;

                        DutPadInfos.Remove(DutPadInfos[index]);

                        foreach (var pad in DutPadInfos)
                        {
                            if (pad.PadNumber.Value > index)
                            {
                                pad.PadNumber.Value -= 1;
                            }
                        }

                        if (index != 0)
                        {
                            await PrevPad();
                        }
                        else if (DutPadInfos.Count != 0)
                        {
                            await NextPad();
                        }
                        else
                        {
                            StepLabel = $"{GetRegistPadCompType()} Pad Count : {DutPadInfos.Count}. Cur : {--CurPadIndex}";
                        }

                        WaferObject.GetSubsInfo().Pads.DutPadInfos = DutPadInfos;
                        this.WaferAligner().UpdatePadCen();

                        if (this.LoaderRemoteMediator() != null)
                        {
                            byte[] dutpadinfos = null;

                            dutpadinfos = this.ObjectToByteArray(DutPadInfos);

                            this.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateDutPadInfos(dutpadinfos);
                        }
                    }

                    CurCam.UpdateOverlayFlag = true;
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog
                        (Properties.Resources.DeletePadErrorTitile,
                         Properties.Resources.DeletePadErrorMessage,
                        EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;

        }

        private string GetRegistPadCompType()
        {
            string ret = "";
            try
            {
                if (EnablePADCompensation)
                {
                    ret = "       (Auto Comp)\n";
                }
                else
                {
                    ret = "     (Manual Regist)\n";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public async void MoveToPad(PadObject padparam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ROIParameter roiparam = new ROIParameter();

                roiparam.OffsetX.Value = (int)(PatternSizeLeft - (TargetRectangleWidth / 2));
                roiparam.OffsetY.Value = (int)(PatternSizeTop - (TargetRectangleHeight / 2));
                roiparam.Width.Value = (int)(TargetRectangleWidth + roiparam.OffsetX.Value);
                roiparam.Height.Value = (int)(TargetRectangleHeight + roiparam.OffsetY.Value);

                ObservableCollection<GrabDevPosition> devpositions = this.VisionManager().Detect_Pad(new ImageBuffer(CurCam.GetChannelType()), TargetRectangleWidth * CurCam.GetRatioX(), TargetRectangleHeight * CurCam.GetRatioY(), roiparam, false);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                if (devpositions.Count == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessage, Properties.Resources.MoveToPad_Blob_ErrorMessage, EnumMessageStyle.Affirmative);
                    return;
                }
                else if (devpositions.Count == 1)
                {
                    WaferCoordinate wcd = null;
                    double offsetx = devpositions[0].PosX;
                    double offsety = devpositions[0].PosY;

                    double posx = 0.0;
                    double posy = 0.0;

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

                    if (CurCam.GetVerticalFlip() == FlipEnum.NONE)
                    {
                        posx = (CurCam.GetGrabSizeWidth() / 2) - offsetx;
                    }
                    else
                    {
                        posx = offsetx - (CurCam.GetGrabSizeWidth() / 2);
                    }

                    if (CurCam.GetHorizontalFlip() == FlipEnum.NONE)
                    {
                        posy = (CurCam.GetGrabSizeHeight() / 2) - offsety;
                    }
                    else
                    {
                        posy = offsety - (CurCam.GetGrabSizeHeight() / 2);

                    }

                    posx *= CurCam.GetRatioX();
                    posy *= CurCam.GetRatioY();

                    double zpos = this.WaferAligner().GetHeightValueAddZOffset(wcd.GetX() + posx, wcd.GetY() + posy, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            wcd.GetX() + posx,
                            wcd.GetY() + posy,
                            zpos, true);
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            wcd.GetX() + posx,
                            wcd.GetY() + posy,
                            zpos, true);

                    TargetRectangleWidth = devpositions[0].SizeX;
                    TargetRectangleHeight = devpositions[0].SizeY;
                }
                else
                {

                    for (int index = devpositions.Count() - 1; index > 0; index--)
                    {
                        for (int jndex = 0; jndex < index; jndex++)
                        {
                            double Aoffsetx = Math.Abs(CurCam.GetGrabSizeWidth() / 2 - devpositions[jndex].PosX);
                            double Aoffsety = Math.Abs(CurCam.GetGrabSizeHeight() / 2 - devpositions[jndex].PosY);

                            double Boffsetx = Math.Abs(CurCam.GetGrabSizeWidth() / 2 - devpositions[jndex + 1].PosX);
                            double Boffsety = Math.Abs(CurCam.GetGrabSizeHeight() / 2 - devpositions[jndex + 1].PosY);

                            if (Aoffsetx + Aoffsety > Boffsetx + Boffsety)
                            {
                                GrabDevPosition tmempdevpos = devpositions[jndex];
                                devpositions[jndex] = devpositions[jndex + 1];
                                devpositions[jndex + 1] = tmempdevpos;
                            }
                        }
                    }


                    WaferCoordinate wcd = null;
                    double offsetx = devpositions[0].PosX;
                    double offsety = devpositions[0].PosY;

                    double posx = 0.0;
                    double posy = 0.0;

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

                    if (CurCam.GetVerticalFlip() == FlipEnum.NONE)
                    {
                        posx = (CurCam.GetGrabSizeWidth() / 2) - offsetx;
                    }
                    else
                    {
                        posx = offsetx - (CurCam.GetGrabSizeWidth() / 2);
                    }

                    if (CurCam.GetHorizontalFlip() == FlipEnum.NONE)
                    {
                        posy = (CurCam.GetGrabSizeHeight() / 2) - offsety;
                    }
                    else
                    {
                        posy = offsety - (CurCam.GetGrabSizeHeight() / 2);

                    }

                    posx *= CurCam.GetRatioX();
                    posy *= CurCam.GetRatioY();

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            wcd.GetX() + posx,
                            wcd.GetY() + posy);
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            wcd.GetX() + posx,
                            wcd.GetY() + posy);

                    TargetRectangleWidth = devpositions[0].SizeX;
                    TargetRectangleHeight = devpositions[0].SizeY;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool InvalidationMatchDutIdex(MachineIndex index)
        {
            bool retVal = false;
            try
            {

                DutWaferIndex idx = this.WaferAligner().GetDutDieIndexs().Find(
                    dut => dut.WaferIndex.XIndex == index.XIndex && dut.WaferIndex.YIndex == index.YIndex);

                if (idx != null)
                    retVal = true;
                else
                    retVal = false;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        private int InvalidationExistPad(DUTPadObject padObject, IList<DUTPadObject> pads, double distlimit = 0.0)
        {
            int retVal = -1;
            try
            {

                bool isXflag = false;
                bool isYflag = false;

                double distX = 0.0;
                double distY = 0.0;
                double distLimit = 0.0;
                try
                {
                    if (distlimit == 0.0)
                        distLimit = 10.0;
                    else
                        distLimit = distlimit;

                    for (int index = 0; index < pads.Count(); index++)
                    {
                        isXflag = false;
                        isYflag = false;

                        distX = Math.Abs(pads[index].PadCenter.X.Value - padObject.PadCenter.X.Value);
                        distY = Math.Abs(pads[index].PadCenter.Y.Value - padObject.PadCenter.Y.Value);

                        if (distX < padObject.PadSizeX.Value)
                        {
                            isXflag = true;
                        }
                        else
                        {
                            isXflag = false;
                        }

                        if (distY < padObject.PadSizeY.Value)
                        {
                            isYflag = true;
                        }
                        else
                        {
                            isYflag = false;
                        }

                        if (isXflag == true && isYflag == true)
                        {
                            retVal = index;
                            retVal = pads[index].PadNumber.Value;
                            break;
                        }

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
                throw err;
            }
            return retVal;
        }

        private Task<EventCodeEnum> ComparedRefPad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    WaferCoordinate padcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();



                    this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX =
                        padcoord.GetX() - this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgRefPadPoint.X;
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY =
                        padcoord.GetY() - this.WaferAligner().WaferAlignInfo.RecoveryParam.OrgRefPadPoint.Y;



                    EnumMessageDialogResult dialogResult = EnumMessageDialogResult.UNDEFIND;

                    //dialogResult = await MessageDialog.ShowDialog("Low Recovery",
                    //        String.Format("Existing information and new information have errors of X: {0}, Y:{1}." +
                    //        " Do you want to save the error as corrected information?",
                    //        recoveryParam.RefDieOffsetX, recoveryParam.RefDieOffsetY), EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary);

                    dialogResult = EnumMessageDialogResult.AFFIRMATIVE;

                    if (dialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        //foreach (var ptinfo in tempPtInfo)
                        //{
                        //    ptinfo.X.Value += Wafer.SubsInfo.RecoveryParam.RefDieOffsetX;
                        //    ptinfo.Y.Value += Wafer.SubsInfo.RecoveryParam.RefDieOffsetY;
                        //}
                        SubModuleState.Execute();

                        //AlignModuleState.Verify();
                    }

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }


        private void ShiftPads(double offsetx, double offsety)
        {
            try
            {
                foreach (var pad in DutPadInfos)
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }


        public bool IsExecute()
        {
            bool retVal = true;
            return retVal;
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //WaferAlign SubModule 이지만 Setup 을 따로하기 때문에 WAferAlign Setup이 끝나 Align은 돌지만 PadSetup이 안되도 WaferAlign 실패가 되면 안되기 때문에.
                //Pad 만 PadParamValidation() 라는 함수 사용. 

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw err;
            }
            return retVal;
        }


        public EventCodeEnum PadParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Wafer.GetSubsInfo().Pads.DutPadInfos != null)
                {
                    if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count >= 4)
                    {
                        if (Wafer.GetSubsInfo().Pads.RefPad != null)
                        {
                            retVal = EventCodeEnum.NONE;
                        }
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

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                retVal = IsParamChanged;
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
                if (this.WaferAligner().GetIsModify() == false)
                {
                    if (this.WaferObject.GetAlignState() == AlignStateEnum.DONE)
                    {
                        OneButton.IsEnabled = true;
                        TwoButton.IsEnabled = true;
                    }
                    else
                    {
                        OneButton.IsEnabled = false;
                        TwoButton.IsEnabled = false;
                        if (!this.WaferAligner().IsNewSetup)
                        {
                            OneButton.IsEnabled = true;
                        }
                    }
                }

                if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP && this.WaferAligner().IsNewSetup)
                {
                    if (PadParamValidation() == EventCodeEnum.NONE && !IsParameterChanged())
                        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                    else
                        SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
                else
                {
                    //!this.WaferAligner().IsNewSetup
                    if (this.GetState() != SubModuleStateEnum.DONE)
                        SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    else
                        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetAssociateParam()
        {
            this.GetParam_ProbeCard().PinSetupChangedToggle.DoneState = ElementStateEnum.NEEDSETUP;
            this.GetParam_ProbeCard().SetAlignState(AlignStateEnum.IDLE);
        }

        #region //.. OverlayDut

        private void DrawDutPad()
        {
            try
            {
                StopDrawDut();
                //CurCam.DrawDisplayDelegate += async (ImageBuffer img, ICamera camera) =>
                //{
                //    DrawDutPadThread(img);
                //};
                CurCam.DrawDisplayDelegate += DrawDutPadThread;
                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }


        private void StopDrawDut()
        {
            try
            {
                //CurCam.DrawDisplayDelegate -= async (ImageBuffer img, ICamera camera) =>
                //{
                //    DrawDutPadThread(img);
                //};
                CurCam.DrawDisplayDelegate -= DrawDutPadThread;
                CurCam.InDrawOverlayDisplay();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        DrawRectangleModule drawDutRectangle = new DrawRectangleModule();
        DrawRectangleModule drawPadRectangle = new DrawRectangleModule();

        private WaferCoordinate LUcorner = new WaferCoordinate();
        private WaferCoordinate RDcorner = new WaferCoordinate();

        private void DrawDutPadThread(ImageBuffer img, ICamera cam = null)
        {
            try
            {

                double pt1_x;
                double pt1_y;
                double pt2_x;
                double pt2_y;

                double milScreenLeft = img.CatCoordinates.X.Value - (img.SizeX / 2 * img.RatioX.Value);
                double milScreenTop = img.CatCoordinates.Y.Value + (img.SizeY / 2 * img.RatioY.Value);
                double milScreenRight = img.CatCoordinates.X.Value + (img.SizeX / 2 * img.RatioX.Value);
                double milScreenBottom = img.CatCoordinates.Y.Value - (img.SizeY / 2 * img.RatioY.Value);

                double[,] DieCornerPos = new double[2, 2];
                double[,] DisplayCornerPos = new double[2, 2];

                if (Wafer.DispHorFlip == DispFlipEnum.FLIP)
                {
                    DisplayCornerPos[0, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));
                    DisplayCornerPos[1, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));

                }
                else
                {
                    DisplayCornerPos[0, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));
                    DisplayCornerPos[1, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));

                }
                if (Wafer.DispVerFlip == DispFlipEnum.FLIP)
                {
                    DisplayCornerPos[0, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));
                    DisplayCornerPos[1, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));

                }
                else
                {
                    DisplayCornerPos[0, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));
                    DisplayCornerPos[1, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));
                }

                LUcorner.X.Value = DisplayCornerPos[0, 0];
                LUcorner.Y.Value = DisplayCornerPos[0, 1];

                RDcorner.X.Value = DisplayCornerPos[1, 0];
                RDcorner.Y.Value = DisplayCornerPos[1, 1];

                MachineIndex LUcornerUI = this.CoordinateManager().GetCurMachineIndex(LUcorner);
                MachineIndex RDcornerUI = this.CoordinateManager().GetCurMachineIndex(RDcorner);
                //LoggerManager.Debug($"DrawDutPad(): LUcornerUI(M) = ({LUcornerUI.XIndex}, {LUcornerUI.YIndex})");
                //LoggerManager.Debug($"DrawDutPad(): RDcornerUI(M) = ({RDcornerUI.XIndex}, {RDcornerUI.YIndex})");

                //MachineIndex CurDieUI = this.CoordinateManager().GetCurMachineIndex(img.CatCoordinates as WaferCoordinate);
                MachineIndex CurDieUI = CurCam.GetCurCoordMachineIndex();


                double DieStartXIndex = (LUcornerUI.XIndex > RDcornerUI.XIndex)
                 ? RDcornerUI.XIndex : LUcornerUI.XIndex;
                double DieStartYIndex = (LUcornerUI.YIndex > RDcornerUI.YIndex)
                ? RDcornerUI.YIndex : LUcornerUI.YIndex;

                double DieEndXIndex = (LUcornerUI.XIndex > RDcornerUI.XIndex)
                ? LUcornerUI.XIndex : RDcornerUI.XIndex;
                double DieEndYIndex = (LUcornerUI.YIndex > RDcornerUI.YIndex)
                ? LUcornerUI.YIndex : RDcornerUI.YIndex;



                for (int y = (int)DieStartYIndex; y <= (int)DieEndYIndex; y++)
                {
                    for (int x = (int)DieStartXIndex; x <= (int)DieEndXIndex; x++)
                    {

                        if (this.WaferAligner().GetDutDieIndexs().Find(dev => dev.WaferIndex.XIndex == x && dev.WaferIndex.YIndex == y) != null)
                        {

                            DieCornerPos = this.CoordinateManager().GetAnyDieCornerPos(null, img.CamType);

                            Point pt = new Point();
                            pt.X = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(x, y).GetX();
                            pt.Y = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(x, y).GetY();

                            //DieCornerPos[0, 0] = pt.X + (WaferObject.DieXClearance.Value / 2);
                            //DieCornerPos[1, 1] = pt.Y + (WaferObject.DieYClearance.Value / 2);
                            //DieCornerPos[0, 1] = DieCornerPos[1, 1] + (WaferObject.GetSubsInfo().ActualDieSize.Height.Value - (WaferObject.DieYClearance.Value));
                            //DieCornerPos[1, 0] = DieCornerPos[0, 0] + (WaferObject.GetSubsInfo().ActualDieSize.Width.Value - (WaferObject.DieXClearance.Value));

                            DieCornerPos[0, 0] = pt.X;
                            DieCornerPos[1, 1] = pt.Y;

                            DieCornerPos[0, 1] = DieCornerPos[1, 1] + WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                            DieCornerPos[1, 0] = DieCornerPos[0, 0] + WaferObject.GetSubsInfo().ActualDieSize.Width.Value;

                            //if (Wafer.VisionManager().DispHorFlip == DispFlipEnum.FLIP)
                            //{
                            //    DieCornerPos[0, 0] = DieCornerPos[0, 0] + WaferObject.GetSubsInfo().ActualDieSize.Width.Value;  
                            //    DieCornerPos[1, 0] = pt.X;
                            //}
                            //else
                            //{
                            //    DieCornerPos[0, 0] = pt.X;
                            //    DieCornerPos[1, 0] = DieCornerPos[0, 0] + WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                            //}
                            //if (Wafer.VisionManager().DispVerFlip == DispFlipEnum.FLIP)
                            //{
                            //    DieCornerPos[1, 1] = DieCornerPos[1, 1] + WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                            //    DieCornerPos[0, 1] = pt.Y; 

                            //}
                            //else
                            //{
                            //    DieCornerPos[1, 1] = pt.Y;
                            //    DieCornerPos[0, 1] = DieCornerPos[1, 1] + WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                            //}

                            pt1_x = (DieCornerPos[0, 0] - milScreenLeft) / img.RatioX.Value;
                            pt1_y = (milScreenTop - DieCornerPos[0, 1]) / img.RatioY.Value;

                            pt2_x = (DieCornerPos[1, 0] - milScreenLeft) / img.RatioX.Value;
                            pt2_y = (milScreenTop - (DieCornerPos[1, 1])) / img.RatioY.Value;


                            drawDutRectangle = new DrawRectangleModule(
                                Math.Truncate(pt1_x + ((pt2_x - pt1_x) / 2)),
                                Math.Truncate(pt1_y - ((pt1_y - pt2_y) / 2)),
                                Math.Abs(pt2_x - pt1_x),
                                Math.Abs(pt1_y - pt2_y),
                                verflip: this.VisionManager().GetDispVerFlip(),
                                horflip: this.VisionManager().GetDispHorFlip(),
                                left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);


                            drawDutRectangle.Color = Colors.Violet;
                            drawDutRectangle.Thickness = 3;
                            CurCam.DisplayService.DrawOverlayContexts.Add(drawDutRectangle);


                            #region //..Dut
                            var pt1x = (DieCornerPos[0, 0] - milScreenLeft) / img.RatioX.Value;
                            var pt1y = (milScreenTop - DieCornerPos[0, 1]) / img.RatioY.Value;

                            var pt2x = (DieCornerPos[1, 0] - milScreenLeft) / img.RatioX.Value;
                            var pt2y = (milScreenTop - (DieCornerPos[1, 1])) / img.RatioY.Value;

                            if (MainViewTarget == DisplayPort)
                            {
                                MachineIndex CurDieUIFlip = new MachineIndex(CurDieUI);

                                var dutobj = this.WaferAligner().GetDutDieIndexs().Find(
                                    dut => dut.WaferIndex.XIndex == CurDieUIFlip.XIndex
                                        && dut.WaferIndex.YIndex == CurDieUIFlip.YIndex);


                                if (dutobj != null)
                                {
                                    DrawTextModule drawDutText = new DrawTextModule
                                            (440, 54, $"DUT : {dutobj.DutNumber}");
                                    drawDutText.Fontcolor = Colors.OrangeRed;
                                    drawDutText.FontSize = 28;
                                    CurCam.DisplayService.DrawOverlayContexts.Add(drawDutText);
                                    //StepLabel = StepLabel + Environment.NewLine + $"DUT : {dutobj.DutNumber}";
                                }
                                else
                                {
                                    DrawTextModule drawDutText = new DrawTextModule
                                            (480 - 100, 54, $"DUT : Out Index");
                                    drawDutText.Fontcolor = Colors.OrangeRed;
                                    drawDutText.FontSize = 28;
                                    CurCam.DisplayService.DrawOverlayContexts.Add(drawDutText);
                                    //StepLabel = StepLabel + Environment.NewLine + $"DUT : Out Index";
                                }
                            }



                            #endregion


                            //DutViewer.UcDisplayDutSize = new Size(drawDutRectangle.Width, drawDutRectangle.Height);
                            for (int i = 0; i < DutPadInfos.Count(); i++)
                            {


                                if (DutPadInfos[i].MachineIndex.XIndex == x && DutPadInfos[i].MachineIndex.YIndex == y)
                                {


                                    //double padstartX = ((DieCornerPos[0, 0] - WaferObject.DieXClearance.Value/2) + (DutPadInfos[i].PadCenter.X.Value)
                                    //       + (DutPadInfos[i].PadSizeX.Value / 2));
                                    //double padstartY = ((DieCornerPos[1, 1] - WaferObject.DieYClearance.Value / 2) + (DutPadInfos[i].PadCenter.Y.Value)
                                    //    + (DutPadInfos[i].PadSizeY.Value / 2));
                                    double padstartX = ((DieCornerPos[0, 0]) + (DutPadInfos[i].PadCenter.X.Value)
                                              + (DutPadInfos[i].PadSizeX.Value / 2));
                                    double padstartY = ((DieCornerPos[1, 1]) + (DutPadInfos[i].PadCenter.Y.Value)
                                        + (DutPadInfos[i].PadSizeY.Value / 2));
                                    double padendX = padstartX + DutPadInfos[i].PadSizeX.Value;
                                    double padendY = padstartY - DutPadInfos[i].PadSizeY.Value;

                                    if (padstartX > milScreenLeft |
                                          padstartX < milScreenRight |
                                          padstartY < milScreenTop |
                                          padstartY > milScreenBottom)
                                    {
                                        pt1_x = (padstartX - milScreenLeft) / img.RatioX.Value;
                                        pt1_y = (milScreenTop - padstartY) / img.RatioY.Value;

                                        pt2_x = (padendX - milScreenLeft) / img.RatioX.Value;
                                        pt2_y = (milScreenTop - padendY) / img.RatioY.Value;


                                        //double padcetnerx = pt1_x - ((pt2_x - pt1_x) * (-1) / 2);
                                        //double padcentery = pt1_y - ((pt1_y - pt2_y) * (-1) / 2);
                                        double padcetnerx = pt1_x - ((pt2_x - pt1_x) / 2);
                                        double padcentery = pt1_y - ((pt1_y - pt2_y) / 2);

                                        if (!(padcetnerx - (DutPadInfos[i].PadSizeX.Value / 2) > CurCam.GetGrabSizeWidth()
                                        | padcetnerx + (DutPadInfos[i].PadSizeX.Value / 2) < 0
                                        | padcentery - (DutPadInfos[i].PadSizeY.Value / 2) > CurCam.GetGrabSizeHeight()
                                        | padcentery + (DutPadInfos[i].PadSizeY.Value / 2) < 0))
                                        {
                                            drawPadRectangle =
                                       new DrawRectangleModule(
                                           padcetnerx, padcentery, DutPadInfos[i].PadSizeX.Value /
                                           CurCam.GetRatioX(), DutPadInfos[i].PadSizeY.Value /
                                           CurCam.GetRatioY(),
                                           verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                           left: 0, right: img.SizeX, top: 0, bottom: img.SizeY
                                           );

                                            drawPadRectangle.Color = Colors.Blue;

                                            DrawTextModule drawText = new DrawTextModule
                                            (padcetnerx - ((DutPadInfos[i].PadSizeX.Value / 2) /
                                                CurCam.GetRatioX()) + 3,
                                                padcentery - ((DutPadInfos[i].PadSizeY.Value / 2) /
                                                CurCam.GetRatioY()) + 1,
                                                DutPadInfos[i].PadNumber.ToString());
                                            drawText.Fontcolor = Colors.OrangeRed;
                                            drawText.FontSize = 14;
                                            CurCam.DisplayService.DrawOverlayContexts.Add(drawPadRectangle);
                                            CurCam.DisplayService.DrawOverlayContexts.Add(drawText);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //CurCam.DisplayService.Draw(img);
                //if (CurCam.UpdateOverlayFlag)
                //    CurCam.UpdateOverlayFlag = false;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void UpdateLabel()
        {
        }


        #endregion

    }



}
