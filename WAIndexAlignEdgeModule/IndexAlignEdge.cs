using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WAIndexAlignEdgeModule
{
    using LogModule;
    using MetroDialogInterfaces;
    using Newtonsoft.Json;
    using PnPControl;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using ProberInterfaces.Vision;
    using ProberInterfaces.WaferAlignEX.Enum;
    using RelayCommandBase;
    using SerializerUtil;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;
    using WA_IndexAlignParameter_Edge;

    [Serializable]
    public class IndexAlignEdge : PNPSetupBase, IProcessingModule, INotifyPropertyChanged, ISetup, IRecovery, IHasDevParameterizable, ILotReadyAble, IPackagable, IHasAdvancedSetup
    {
        public override bool Initialized { get; set; } = false;
        public override Guid ScreenGUID { get; } = new Guid("CF1E43F7-AFEA-C748-B274-0307A4AE40E6");
        //public IParam DevParam { get; set; }
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

        public new List<object> Nodes { get; set; }
        public SubModuleStateBase SubModuleState { get; set; }
        public SubModuleMovingStateBase MovingState { get; set; }
        private IParam _IndexAlignEdgeParam_IParam;
        public IParam IndexAlignEdgeParam_IParam
        {
            get { return _IndexAlignEdgeParam_IParam; }
            set
            {
                if (value != _IndexAlignEdgeParam_IParam)
                {
                    _IndexAlignEdgeParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public WA_IndexAlignParam_Edge IndexAlignEdgeParam_Clone;

        private List<WaferCoordinate> EdgePos = new List<WaferCoordinate>();
        private int _CurEdgePosIndex = 0;
        private bool Indexalignskip = false;
        public IndexAlignEdge()
        {

        }
        public IndexAlignEdge(IStateModule Module)
        {

        }

        public EventCodeEnum DoExecute() //실제 프로세싱 하는 코드
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                MovingState.Moving();

                WA_IndexAlignParam_Edge procparam = IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge;

                if (procparam.AlignEnable == EnumWASubModuleEnable.DISABLE)
                {
                    RetVal = EventCodeEnum.NONE;

                    LoggerManager.Debug($"[Index Align Edge] - Align Enable option :{procparam.AlignEnable}");
                    SubModuleState = new SubModuleDoneState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Edge_Skip);

                    return RetVal;
                }

                if (procparam.AlignEnable == EnumWASubModuleEnable.ENABLE && Indexalignskip == true)
                {
                    RetVal = EventCodeEnum.NONE;

                    LoggerManager.Debug($"[Index Align Edge] - Align Enable option :{procparam.AlignEnable} Align setup state ({this.WaferAligner().IsNewSetup}), ({this.WaferAligner().GetIsModifySetup()})");
                    SubModuleState = new SubModuleDoneState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Edge_Skip);

                    return RetVal;
                }

                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Edge_Start);

                if (CurCam == null)
                {
                    CurCam = this.VisionManager().GetCam((IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge).CamType);
                }

                foreach (var light in procparam.LightParams)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                this.VisionManager().StopGrab(CurCam.GetChannelType());
                this.VisionManager().StopGrab(EnumProberCam.WAFER_HIGH_CAM);               

                for (int index = 0; index < procparam.EdgeParams.Value.Count; index++)
                {
                    RetVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(procparam.EdgeParams.Value[index].Position.GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                                                                      procparam.EdgeParams.Value[index].Position.GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                                                                      WaferObject.GetSubsInfo().ActualThickness);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        //Thread.Sleep(500);
                        RetVal = Processing(procparam.EdgeParams.Value[index].Direction, procparam.AllowableRange.Value, procparam.AlignThreshold.Value);

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            this.WaferAligner().ManuallAlignmentErrTxt = $"Tolerance Error.\nEdge Not Found(IndexAlign) (position : [{procparam.EdgeParams.Value[index].Direction}])";
                            this.WaferAligner().ReasonOfError.AddEventCodeInfo(EventCodeEnum.Wafer_Index_Align_Edge_Failure, this.WaferAligner().ManuallAlignmentErrTxt, this.GetType().Name);

                            #region [Autolight] 추후 알고리즘 개선을 위한 분석용 데이터 수집용으로 이미지를 저장하는 코드가 들어감. 추후 삭제될 수 있음.
                            // 1. SetLight (이때 light template 사용) -> 임시 코드일 수 있음.
                            /// <summary>
                            /// light template
                            /// 1. oblique : 255, coaxial : 0
                            /// 2. oblique: 0, coaxial: 255
                            /// 3. oblique: 128, coaxial: 0
                            /// 4. oblique: 0, coaxial: 128
                            /// </summary>
                            List<Tuple<ushort, ushort>> lightTemplate = new List<Tuple<ushort, ushort>>();
                            lightTemplate.Add(new Tuple<ushort, ushort>(255, 0));
                            lightTemplate.Add(new Tuple<ushort, ushort>(0, 255));
                            lightTemplate.Add(new Tuple<ushort, ushort>(128, 0));
                            lightTemplate.Add(new Tuple<ushort, ushort>(0, 128));

                            for (int idx = 0; idx < lightTemplate.Count; idx++)
                            {
                                foreach (var light in procparam.LightParams)
                                {
                                    if (light.Type.Value == EnumLightType.OBLIQUE)
                                    {
                                        CurCam.SetLight(light.Type.Value, lightTemplate[idx].Item1);
                                    }
                                    else if (light.Type.Value == EnumLightType.COAXIAL)
                                    {
                                        CurCam.SetLight(light.Type.Value, lightTemplate[idx].Item2);
                                    }
                                    else
                                    {
                                        //NONE.
                                    }
                                }

                                // 2. Processing
                                Processing(procparam.EdgeParams.Value[index].Direction, procparam.AllowableRange.Value, procparam.AlignThreshold.Value, true);
                            }
                            #endregion

                            break;
                        }
                    }
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                if (RetVal == EventCodeEnum.NONE &&
                    (procparam.PositionToleranceWCtoTargetdie.Value > 0 && procparam.PositionToleranceWCtoTargetdie.Value < 100) &&
                    procparam.WaferCentertoTargetX.Value > 0 &&
                    procparam.WaferCentertoTargetY.Value > 0)
                {
                    int threshold = procparam.PositionToleranceWCtoTargetdie.Value;

                    double diesize_X = WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    double diesize_Y = WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                    MachineIndex DieMI = this.CoordinateManager().UserIndexConvertToMachineIndex(new UserIndex(WaferObject.GetPhysInfo().CenU.XIndex.Value, WaferObject.GetPhysInfo().CenU.YIndex.Value));
                    var CenterPos = this.WaferAligner().MachineIndexConvertToDieCenter(DieMI.XIndex, DieMI.YIndex);

                    if (procparam.CenMregist_X.Value == DieMI.XIndex && procparam.CenMregist_Y.Value == DieMI.YIndex)
                    {
                        double cenx = Math.Abs(Math.Abs(CenterPos.GetX() - WaferObject.GetSubsInfo().WaferCenterOriginatEdge.GetX()) - Math.Abs(procparam.WaferCentertoTargetX.Value)) / diesize_X * 100;
                        double ceny = Math.Abs(Math.Abs(CenterPos.GetY() - WaferObject.GetSubsInfo().WaferCenterOriginatEdge.GetY()) - Math.Abs(procparam.WaferCentertoTargetY.Value)) / diesize_Y * 100;

                        if (cenx > threshold || ceny > threshold)
                        {
                            RetVal = EventCodeEnum.Wafer_Index_Align_Edge_Failure;

                            var x = Math.Truncate(cenx * 1000) / 1000;
                            var y = Math.Truncate(ceny * 1000) / 1000;

                            this.WaferAligner().ManuallAlignmentErrTxt = $"Tolerance Error.\nPosition Tolerance Wafer Center to Target die: {threshold}%\n Current Position Tolerance Wafer Center to Target die (x,y) : {x}% , {y}%";
                            this.WaferAligner().ReasonOfError.AddEventCodeInfo(EventCodeEnum.Wafer_Index_Align_Edge_Failure, this.WaferAligner().ManuallAlignmentErrTxt, this.GetType().Name);
                        }
                    }
                    else
                    {
                        RetVal = EventCodeEnum.Wafer_Index_Align_Edge_Failure;

                        this.WaferAligner().ManuallAlignmentErrTxt = $"Index Align Error.\nWafer Center die at registration (x,y) : {DieMI.XIndex},{DieMI.YIndex}\n Current Wafer Center die (x,y) : {procparam.CenMregist_X.Value}, {procparam.CenMregist_Y.Value}";
                        this.WaferAligner().ReasonOfError.AddEventCodeInfo(EventCodeEnum.Wafer_Index_Align_Edge_Failure, this.WaferAligner().ManuallAlignmentErrTxt, this.GetType().Name);
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    SubModuleState = new SubModuleDoneState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Edge_OK);
                }
                else if (RetVal == EventCodeEnum.SUB_RECOVERY)
                {
                    SubModuleState = new SubModuleRecoveryState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Index_Align_Edge_Failure, RetVal);
                    this.NotifyManager().Notify(EventCodeEnum.WAFER_INDEX_ALIGN_FAIL);
                }
                else if (RetVal == EventCodeEnum.SUB_SKIP)
                {
                    SubModuleState = new SubModuleSkipState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Edge_OK);
                }
                else
                {
                    RetVal = EventCodeEnum.Wafer_Index_Align_Edge_Failure;
                    SubModuleState = new SubModuleErrorState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Index_Align_Edge_Failure);
                    this.NotifyManager().Notify(EventCodeEnum.WAFER_INDEX_ALIGN_FAIL);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Index_Align_Edge_Failure);
                this.NotifyManager().Notify(EventCodeEnum.WAFER_INDEX_ALIGN_FAIL);
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                throw err;
            }
            finally
            {
                MovingState.Stop();
            }

            return RetVal;
        }
        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public EventCodeEnum DoClearData() //현재 Parameter Check 및 Init하는 코드
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

        public bool IsLotReady(out string msg)
        {
            bool retVal = false;
            msg = null;

            try
            {
                if ((IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge).AlignEnable == EnumWASubModuleEnable.DISABLE ||
                        this.WaferAligner().IsNewSetup || this.WaferAligner().GetIsModifySetup() ||
                        ((IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge).AlignEnable == EnumWASubModuleEnable.ENABLE &&
                        (IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge).EdgeParams.Value.Count == 4))
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                    msg = "Not Exist Index Align Info(Wafer Index Edge Align).";
                }
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
                WA_IndexAlignParam_Edge param = (IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge);

                if (param.AlignEnable == EnumWASubModuleEnable.DISABLE)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    if (param.EdgeParams.Value.Count >= 4)
                    {
                        retVal = Extensions_IParam.ElementStateNeedSetupValidation(IndexAlignEdgeParam_IParam);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.GetState() == SubModuleStateEnum.IDLE | this.GetState() == SubModuleStateEnum.SKIP)
                {
                    SubModuleState = new SubModuleRecoveryState(this);
                }
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
            }

            return RetVal;
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
        public bool IsExecute() //SubModule이 Processing 가능한지 판단하는 조건 
        {
            return true;
        }

        #region Don`t Touch Code
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }
        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }
        public EventCodeEnum ExitRecovery()
        {

            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        #endregion
        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SubModuleState = new SubModuleIdleState(this);
                    MovingState = new SubModuleStopState(this);
                    SetupState = new NotCompletedState(this);

                    if (this.ProberStation() != null)
                    {
                        CurrMaskingLevel = this.ProberStation().MaskingLevel;
                    }

                    if (IndexAlignEdgeParam_Clone?.LightParams == null)
                    {
                        IndexAlignEdgeParam_Clone.LightParams = new ObservableCollection<LightValueParam>();
                        var cam = this.VisionManager().GetCam(IndexAlignEdgeParam_Clone.CamType);
                        for (int index = 0; index < cam.LightsChannels.Count; index++)
                        {
                            ushort lightvalue = 0;
                            if (cam.LightsChannels[index].Type.Value == EnumLightType.OBLIQUE)
                            {
                                lightvalue = 255;
                            }
                            else if (cam.LightsChannels[index].Type.Value == EnumLightType.COAXIAL)
                            {
                                lightvalue = 0;
                            }
                            else
                            {
                                //lightschannel이 추가되면 고려되어야 함 아래 값
                                lightvalue = 110;
                            }
                            IndexAlignEdgeParam_Clone.LightParams.Add(new LightValueParam(cam.LightsChannels[index].Type.Value, lightvalue));
                        }
                    }

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
                LoggerManager.Error($"InitModule(): Error occurred. Err = {err.Message}");

                LoggerManager.Exception(err);
            }

            return retval;
        }

        public new void DeInitModule()
        {
        }

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(WA_IndexAlignParam_Edge));

                    if (target != null)
                    {
                        WA_IndexAlignParam_Edge paramobj = (WA_IndexAlignParam_Edge)target;

                        IndexAlignEdgeParam_Clone.AlignEnable = paramobj.AlignEnable;
                        IndexAlignEdgeParam_Clone.AllowableRange = paramobj.AllowableRange;
                        IndexAlignEdgeParam_Clone.AlignThreshold = paramobj.AlignThreshold;
                        IndexAlignEdgeParam_Clone.PositionToleranceWCtoTargetdie = paramobj.PositionToleranceWCtoTargetdie;
                        IndexAlignEdgeParam_Clone.WaferCentertoTargetX = paramobj.WaferCentertoTargetX;
                        IndexAlignEdgeParam_Clone.WaferCentertoTargetY = paramobj.WaferCentertoTargetY;
                        IndexAlignEdgeParam_IParam = IndexAlignEdgeParam_Clone;

                        break;
                    }
                }

                UpdatePnpUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new WA_IndexAlignParam_Edge();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(WA_IndexAlignParam_Edge));

                if (RetVal == EventCodeEnum.NONE)
                {
                    IndexAlignEdgeParam_IParam = tmpParam;
                    IndexAlignEdgeParam_Clone = IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IndexAlignEdgeParam_IParam != null)
                {
                    IndexAlignEdgeParam_IParam = IndexAlignEdgeParam_Clone;
                    retVal = this.SaveParameter(IndexAlignEdgeParam_IParam);

                    IsParamChanged = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }


        /// <summary>
        /// 현재 모듈의 PNP가 화면에 뜰때마다 호출되는 함수
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = Properties.Resources.Header;

                retVal = InitPnpModuleStage_AdvenceSetting();
                retVal = InitLightJog(this);

                AdvanceSetupView = new EdgeIndexAlignAdvanceSetup.View.EdgeIndexAlignAdvanceSetupView();
                AdvanceSetupViewModel = new EdgeIndexAlignAdvanceSetup.ViewModel.EdgeIndexAlignAdvanceSetupViewModel();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (parameter is EventCodeEnum)
                {
                    if ((EventCodeEnum)parameter == EventCodeEnum.NONE)
                    {
                        await base.Cleanup(parameter);
                    }
                }

                this.WaferAligner().SetIsModifySetup(false);
                this.StageSupervisor().WaferObject.ChangeAlignSetupControlFlag(DrawDieOverlayEnum.Edge_Center, false);
                this.StageSupervisor().WaferObject.StopDrawDieOberlay(CurCam);
                if (GetModuleSetupState() == EnumMoudleSetupState.NOTCOMPLETED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Ararm Message", "The Wafer state was reset to IDLE because the index alignment setup was not complete.", EnumMessageStyle.Affirmative);
                    this.WaferObject.SetAlignState(AlignStateEnum.IDLE);
                }

                retVal = await base.Cleanup(parameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                UpdateGuideLine(true, true);
            }

            return retVal;
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(IndexAlignEdgeParam_IParam));

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                if (this.WaferAligner().IsNewSetup & (this.WaferAligner().GetIsModify() == false))
                {
                    await InitSetup();
                }
                else
                {
                    if (this.GetState() == SubModuleStateEnum.RECOVERY | this.GetState() == SubModuleStateEnum.ERROR)
                    {
                        retVal = await InitRecovery();
                    }
                    else
                    {
                        retVal = await InitSetup();
                    }
                }

                UpdatePnpUI();

                InitLightJog(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION; ;
                throw err;
            }
            finally
            {
                UpdateGuideLine();
            }

            return retVal;
        }

        public new void CloseAdvanceSetupView()
        {
            try
            {
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdateGuideLine(bool isDisable = false, bool isCleanup = false)
        {
            try
            {
                JogMode changejogMode;

                if (isDisable)
                {
                    this.StageSupervisor().WaferObject.TopLeftToBottomRightLineVisible = Visibility.Collapsed;
                    this.StageSupervisor().WaferObject.BottomLeftToTopRightLineVisible = Visibility.Collapsed;

                    if (isCleanup)
                    {
                        // 화면을 나갈 때, true로 변경
                        DisplayClickToMoveEnalbe = true;
                    }

                    this.StageSupervisor().WaferObject.MapViewStageSyncEnable = true;

                    changejogMode = JogMode.Normal;
                }
                else
                {
                    DisplayClickToMoveEnalbe = false;
                    this.StageSupervisor().WaferObject.MapViewStageSyncEnable = false;

                    if (_CurEdgePosIndex == 0 || _CurEdgePosIndex == 2)
                    {
                        changejogMode = JogMode.DiagonalRightUpLeftDown;

                        this.StageSupervisor().WaferObject.TopLeftToBottomRightLineVisible = Visibility.Visible;
                        this.StageSupervisor().WaferObject.BottomLeftToTopRightLineVisible = Visibility.Collapsed;
                    }
                    else
                    {
                        changejogMode = JogMode.DiagonalLeftUpRightDown;

                        this.StageSupervisor().WaferObject.TopLeftToBottomRightLineVisible = Visibility.Collapsed;
                        this.StageSupervisor().WaferObject.BottomLeftToTopRightLineVisible = Visibility.Visible;
                    }
                }

                this.JogType = changejogMode;

                if (!this.PnPManager().IsActivePageSwithcing())
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.WaferObjectInfoNonSerializeUpdated(this.StageSupervisor().GetWaferObjectInfoNonSerialize());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
                retVal = InitPnpUI();
                _CurEdgePosIndex = 0;

                CurCam = this.VisionManager().GetCam(IndexAlignEdgeParam_Clone.CamType);

                if ((IndexAlignEdgeParam_Clone.LightParams?.Count ?? 0) != 0)
                {
                    for (int index = 0; index < IndexAlignEdgeParam_Clone.LightParams.Count; index++)
                    {
                        CurCam.SetLight(IndexAlignEdgeParam_Clone.LightParams[index].Type.Value,
                            IndexAlignEdgeParam_Clone.LightParams[index].Value.Value);
                    }
                }
                else
                {
                    IndexAlignEdgeParam_Clone.LightParams = new ObservableCollection<LightValueParam>();
                    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                    {
                        if (CurCam.LightsChannels[index].Type.Value == EnumLightType.OBLIQUE)
                        {
                            CurCam.SetLight(CurCam.LightsChannels[index].Type.Value, 255);
                        }
                        else if (CurCam.LightsChannels[index].Type.Value == EnumLightType.COAXIAL)
                        {
                            CurCam.SetLight(CurCam.LightsChannels[index].Type.Value, 0);
                        }
                        else
                        {
                            //lightschannel이 추가되면 고려되어야 함 아래 값
                            CurCam.SetLight(CurCam.LightsChannels[index].Type.Value, 110);
                        }

                        IndexAlignEdgeParam_Clone.LightParams.Add(new LightValueParam(CurCam.LightsChannels[index].Type.Value,
                        (ushort)CurCam.GetLight(CurCam.LightsChannels[index].Type.Value)));
                    }
                }

                InitEdgeBasePosition();

                int registerededges = IndexAlignEdgeParam_Clone?.EdgeParams?.Value?.Count ?? 0;

                if (registerededges != 0)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(
                       (IndexAlignEdgeParam_Clone.EdgeParams.Value[_CurEdgePosIndex].Position.GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX()),
                       (IndexAlignEdgeParam_Clone.EdgeParams.Value[_CurEdgePosIndex].Position.GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY()),
                       WaferObject.GetSubsInfo().ActualThickness);

                    for (int index = 0; index < IndexAlignEdgeParam_Clone.EdgeParams.Value.Count; index++)
                    {
                        EnumIndexAlignDirection direction = IndexAlignEdgeParam_Clone.EdgeParams.Value[index].Direction;
                        double x = IndexAlignEdgeParam_Clone.EdgeParams.Value[index].Position.GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX();
                        double y = IndexAlignEdgeParam_Clone.EdgeParams.Value[index].Position.GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY();
                        EdgePos[index] = new WaferCoordinate(x, y);
                    }
                }
                else
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                }

                MainViewTarget = DisplayPort;
                MiniViewTarget = WaferObject;

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                StepLabel = String.Format("Edge  {0}/{1}", registerededges, EdgePos.Count);
                StepSecondLabel = IndexAlignDirection(_CurEdgePosIndex).ToString();

                PadJogRightDown.IsEnabled = false;

                if (IndexAlignEdgeParam_Clone.EdgeParams.Value.Count >= 4)
                {
                    PadJogRightDown.IsEnabled = true;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        /// <summary>
        /// Recovery시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public async Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = await InitSetup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void InitEdgeBasePosition()
        {
            try
            {
                double edgepos = 0.0;
                edgepos = ((WaferObject.GetPhysInfo().WaferSize_um.Value / 2) / Math.Sqrt(2));

                double waferCenterX = WaferObject.GetSubsInfo().WaferCenter.GetX();
                double waferCenterY = WaferObject.GetSubsInfo().WaferCenter.GetY();

                EdgePos.Clear();
                EdgePos.Add(new WaferCoordinate(waferCenterX + edgepos, waferCenterY + edgepos));
                EdgePos.Add(new WaferCoordinate(waferCenterX + (-edgepos), waferCenterY + edgepos));
                EdgePos.Add(new WaferCoordinate(waferCenterX + (-edgepos), waferCenterY + (-edgepos)));
                EdgePos.Add(new WaferCoordinate(waferCenterX + (edgepos), waferCenterY + (-edgepos)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        internal static class ResourceAccessor
        {
            public static ImageSource Get(System.Drawing.Bitmap bitmap)
            {
                BitmapImage image = new BitmapImage();

                using (MemoryStream ms = new MemoryStream())
                {
                    (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    image.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.StreamSource = null;
                }
                return image;
            }
        }
        private EventCodeEnum InitPnpUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    OneButton.SetIconSoruceBitmap(Properties.Resources.Edge_LeftBottom);
                    TwoButton.SetIconSoruceBitmap(Properties.Resources.Edge_RightBottom);
                    ThreeButton.SetIconSoruceBitmap(Properties.Resources.Edge_RightUpper);
                    FourButton.SetIconSoruceBitmap(Properties.Resources.Edge_LeftUpper);
                }
                else
                {
                    OneButton.SetIconSoruceBitmap(Properties.Resources.Edge_RightUpper);
                    TwoButton.SetIconSoruceBitmap(Properties.Resources.Edge_LeftUpper);
                    ThreeButton.SetIconSoruceBitmap(Properties.Resources.Edge_LeftBottom);
                    FourButton.SetIconSoruceBitmap(Properties.Resources.Edge_RightBottom);
                }

                OneButton.IconCaption = "RU";
                TwoButton.IconCaption = "LU";
                ThreeButton.IconCaption = "LL";
                FourButton.IconCaption = "RL";

                OneButton.Command = new AsyncCommand(RegisteRightUpperEdgeCommand);
                TwoButton.Command = new AsyncCommand(RegisteLeftUpperEdgeCommand);
                ThreeButton.Command = new AsyncCommand(RegisteLeftBottomEdgeCommand);
                FourButton.Command = new AsyncCommand(RegisteRightBottomEdgeCommand);

                PadJogLeftUp.IconCaption = "EDGE";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogLeftUp.Command = new AsyncCommand(PrevEdge);

                PadJogRightUp.IconCaption = "EDGE";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                PadJogRightUp.Command = new AsyncCommand(NextEdge);

                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Wafer.png");
                PadJogSelect.IconCaption = "ALIGN";
                PadJogSelect.Command = new AsyncCommand(WaferAlignExecute);

                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Apply.png");
                PadJogRightDown.IconCaption = "APPLY";
                PadJogRightDown.Command = new AsyncCommand(Apply);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        private EventCodeEnum UpdatePnpUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool flag;

                if (this.WaferObject.GetAlignState() == AlignStateEnum.DONE)
                {
                    flag = true;
                    this.StageSupervisor().WaferObject.DrawDieOverlay(CurCam);
                }
                else
                {
                    flag = false;
                    this.StageSupervisor().WaferObject.StopDrawDieOberlay(CurCam);
                }

                OneButton.IsEnabled = flag;
                TwoButton.IsEnabled = flag;
                ThreeButton.IsEnabled = flag;
                FourButton.IsEnabled = flag;

                PadJogRightDown.IsEnabled = false;

                if (flag == true && IndexAlignEdgeParam_Clone.EdgeParams.Value.Count >= 4)
                {
                    PadJogRightDown.IsEnabled = flag;
                }

                this.StageSupervisor().WaferObject.ChangeAlignSetupControlFlag(DrawDieOverlayEnum.Edge_Center, flag, IndexAlignEdgeParam_Clone.PositionToleranceWCtoTargetdie.Value);

                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }
        #region //..Command Method
        private Task PrevEdge()
        {
            try
            {
                // 업데이트가 될 수 있도록 잠시 켜준다.
                this.StageSupervisor().WaferObject.MapViewStageSyncEnable = true;

                if (!this.PnPManager().IsActivePageSwithcing())
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.WaferObjectInfoNonSerializeUpdated(this.StageSupervisor().GetWaferObjectInfoNonSerialize());
                }

                if (_CurEdgePosIndex - 1 >= 0)
                {
                    _CurEdgePosIndex--;
                }
                else if (_CurEdgePosIndex == 0)
                {
                    _CurEdgePosIndex = EdgePos.Count - 1;
                }

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                }
                else
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                }

                StepSecondLabel = IndexAlignDirection(_CurEdgePosIndex).ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                UpdateGuideLine();
            }

            return Task.CompletedTask;
        }

        private Task NextEdge()
        {
            try
            {
                // 업데이트가 될 수 있도록 잠시 켜준다.
                this.StageSupervisor().WaferObject.MapViewStageSyncEnable = true;

                if (!this.PnPManager().IsActivePageSwithcing())
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.WaferObjectInfoNonSerializeUpdated(this.StageSupervisor().GetWaferObjectInfoNonSerialize());
                }

                if (_CurEdgePosIndex + 1 < EdgePos.Count)
                {
                    _CurEdgePosIndex++;
                }
                else if (_CurEdgePosIndex + 1 == EdgePos.Count)
                {
                    _CurEdgePosIndex = 0;
                }

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                }
                else
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                }

                StepSecondLabel = IndexAlignDirection(_CurEdgePosIndex).ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                UpdateGuideLine();
            }

            return Task.CompletedTask;
        }

        private EnumIndexAlignDirection IndexAlignDirection(int _CurEdgePosIndex)
        {
            EnumIndexAlignDirection direction = EnumIndexAlignDirection.UNDIFIND;

            try
            {
                switch (_CurEdgePosIndex % 4)
                {
                    case 0:
                        direction = EnumIndexAlignDirection.RIGHTUPPER;
                        break;
                    case 1:
                        direction = EnumIndexAlignDirection.LEFTUPPER;
                        break;
                    case 2:
                        direction = EnumIndexAlignDirection.LEFTLOWER;
                        break;
                    case 3:
                        direction = EnumIndexAlignDirection.RIGHTLOWER;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return direction;
        }

        private async Task RegisteLeftUpperEdgeCommand()
        {
            await RegisteEdgePos(EnumIndexAlignDirection.LEFTUPPER);
        }
        private async Task RegisteRightUpperEdgeCommand()
        {
            await RegisteEdgePos(EnumIndexAlignDirection.RIGHTUPPER);
        }
        private async Task RegisteRightBottomEdgeCommand()
        {
            await RegisteEdgePos(EnumIndexAlignDirection.RIGHTLOWER);
        }
        private async Task RegisteLeftBottomEdgeCommand()
        {
            await RegisteEdgePos(EnumIndexAlignDirection.LEFTLOWER);
        }

        public async Task Apply()
        {
            try
            {
                UpdateGuideLine(true);

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                UpdateGuideLine();
            }
        }
        public void SetLightParams()
        {
            try
            {
                if (IndexAlignEdgeParam_Clone?.LightParams == null)
                {
                    var cam = this.VisionManager().GetCam(IndexAlignEdgeParam_Clone.CamType);
                    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                    {
                        IndexAlignEdgeParam_Clone.LightParams.Add(new LightValueParam(CurCam.LightsChannels[index].Type.Value,
                        (ushort)CurCam.GetLight(CurCam.LightsChannels[index].Type.Value)));
                    }
                }
                else
                {
                    IndexAlignEdgeParam_Clone?.LightParams?.Clear();
                    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                    {
                        IndexAlignEdgeParam_Clone?.LightParams?.Add(
                            new LightValueParam(CurCam.LightsChannels[index].Type.Value,
                            (ushort)CurCam.GetLight(CurCam.LightsChannels[index].Type.Value)));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task<EventCodeEnum> Modify()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ClearData();
                SetLightParams();

                retVal = Execute();

                WA_IndexAlignParam_Edge procparam = IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge;

                if (procparam.AlignEnable == EnumWASubModuleEnable.ENABLE)
                {
                    _CurEdgePosIndex = EdgePos.Count - 1;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    var ret = await this.MetroDialogManager().ShowMessageDialog("Infomation Message", Properties.Resources.EdgeSuccessMessage, EnumMessageStyle.Affirmative);
                }
                else
                {
                    if (retVal == EventCodeEnum.Wafer_Index_Align_Edge_Failure)
                    {
                        EventCodeInfo eventCodeInfo = this.WaferAligner().ReasonOfError.GetLastEventCode();

                        if (eventCodeInfo != null)
                        {
                            await this.MetroDialogManager().ShowMessageDialog($"[Loader Error]", $"Code = {eventCodeInfo.EventCode}\n" +
                                                                                             $"Occurred time : {eventCodeInfo.OccurredTime}\n" +
                                                                                             $"Occurred location : {eventCodeInfo.ModuleType}\n" +
                                                                                             $"Reason : {eventCodeInfo.Message}",
                                                                                             EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            LoggerManager.Debug($"[IndexAlignEdgePosition] eventCodeInfo is null");

                            await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.EdgeFindFailTitle, Properties.Resources.EdgeFindFailMessage + "/n", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.EdgeFindFailTitle, Properties.Resources.EdgeFindFailMessage + "/n", EnumMessageStyle.Affirmative);
                    }
                }

                SaveDevParameter();
                UpdatePnpUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public async Task WaferAlignExecute()
        {
            ICamera curcam = CurCam;

            List<LightValueParam> lights = new List<LightValueParam>();

            try
            {
                UpdateGuideLine(true);

                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                try
                {
                    UseUserControl = UserControlFucEnum.DEFAULT;

                    if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Error Message", "Markalign can not behave as a failure. Please check Mark .", EnumMessageStyle.Affirmative);
                        return;
                    }

                    foreach (var light in curcam.LightsChannels)
                    {
                        lights.Add(new LightValueParam(light.Type.Value, (ushort)curcam.GetLight(light.Type.Value)));
                    }

                    this.StageSupervisor().WaferObject.StopDrawDieOberlay(CurCam);
                    Indexalignskip = true;
                    retVal = this.WaferAligner().DoManualOperation();

                    int registerededges = IndexAlignEdgeParam_Clone?.EdgeParams?.Value?.Count ?? 0;
                    if (registerededges == 0)
                    {
                        InitEdgeBasePosition();
                    }

                    if (retVal == EventCodeEnum.WAFER_NOT_EXIST_EROOR)
                    {
                        _CurEdgePosIndex = 0;
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                        var ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Fail Wafer alignment - Not Exist Wafer.", EnumMessageStyle.Affirmative);

                        return;
                    }
                    else if (retVal == EventCodeEnum.MARK_Move_Failure)
                    {
                        _CurEdgePosIndex = 0;
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                        var ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Fail Wafer alignment - Mark Align Fail.", EnumMessageStyle.Affirmative);

                        return;
                    }
                    else
                    {
                        if (this.GetParam_Wafer().GetAlignState() != AlignStateEnum.DONE)
                        {
                            _CurEdgePosIndex = 0;
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);

                            this.WaferAligner().SetSetupState();
                            await this.MetroDialogManager().ShowMessageDialog("WaferAlign Fail", "Please check WaferAlign Setup.", EnumMessageStyle.Affirmative);

                            return;
                        }
                        else
                        {
                            _CurEdgePosIndex = 0;
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[_CurEdgePosIndex].GetX(), EdgePos[_CurEdgePosIndex].GetY(), WaferObject.GetSubsInfo().ActualThickness);
                        }
                    }

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    UpdateGuideLine();

                    Indexalignskip = false;
                    UpdatePnpUI();
                    this.VisionManager().StartGrab(curcam.GetChannelType(), this);

                    foreach (var light in lights)
                    {
                        curcam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        private async Task RegisteEdgePos(EnumIndexAlignDirection direction)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    WaferCoordinate coordinate = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                    MachineIndex curIndex = new MachineIndex(CurCam.CamSystemMI.XIndex, CurCam.CamSystemMI.YIndex);
                    int indexpos = 0;

                    switch (direction)
                    {
                        //현재 위치와 선택한 버튼의 프로세싱 위치가 일치 하지 않을 경우.
                        case EnumIndexAlignDirection.LEFTUPPER:
                            if (coordinate.GetX() >= WaferObject.GetSubsInfo().WaferCenter.GetX()
                                || coordinate.GetY() <= WaferObject.GetSubsInfo().WaferCenter.GetY())
                            {
                                await this.MetroDialogManager().ShowMessageDialog("", Properties.Resources.RegisterUpperLeftMessage, EnumMessageStyle.Affirmative);
                                return;
                            }
                            indexpos = 1;
                            break;
                        case EnumIndexAlignDirection.RIGHTUPPER:
                            if (coordinate.GetX() <= WaferObject.GetSubsInfo().WaferCenter.GetX()
                                || coordinate.GetY() <= WaferObject.GetSubsInfo().WaferCenter.GetY())
                            {
                                await this.MetroDialogManager().ShowMessageDialog("", Properties.Resources.RegisterUpperRightMessage, EnumMessageStyle.Affirmative);
                                return;
                            }
                            indexpos = 0;
                            break;
                        case EnumIndexAlignDirection.RIGHTLOWER:
                            if (coordinate.GetX() <= WaferObject.GetSubsInfo().WaferCenter.GetX()
                                || coordinate.GetY() >= WaferObject.GetSubsInfo().WaferCenter.GetY())
                            {
                                await this.MetroDialogManager().ShowMessageDialog("", Properties.Resources.RegisterLowerRightMessage, EnumMessageStyle.Affirmative);
                                return;
                            }
                            indexpos = 3;
                            break;
                        case EnumIndexAlignDirection.LEFTLOWER:
                            if (coordinate.GetX() >= WaferObject.GetSubsInfo().WaferCenter.GetX()
                                || coordinate.GetY() >= WaferObject.GetSubsInfo().WaferCenter.GetY())
                            {
                                await this.MetroDialogManager().ShowMessageDialog("", Properties.Resources.RegisterLowerLeftMessage, EnumMessageStyle.Affirmative);
                                return;
                            }
                            indexpos = 2;
                            break;
                    }
                    this.VisionManager().StopGrab(CurCam.GetChannelType());

                    retVal = Processing(direction, IndexAlignEdgeParam_Clone.AllowableRange.Value, IndexAlignEdgeParam_Clone.AlignThreshold.Value);

                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                    if (retVal == EventCodeEnum.NONE)
                    {
                        int index = IndexAlignEdgeParam_Clone.EdgeParams.Value.ToList<EdgeIndexAlignParam>().FindIndex(info => info.Direction == direction);

                        WaferCoordinate wcd = new WaferCoordinate();

                        wcd.X.Value = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetX() - WaferObject.GetSubsInfo().WaferCenter.GetX();
                        wcd.Y.Value = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetY() - WaferObject.GetSubsInfo().WaferCenter.GetY();
                        wcd.Z.Value = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetZ() - WaferObject.GetSubsInfo().WaferCenter.GetZ();

                        LoggerManager.Debug($"[IndexAlign Registe Edge Position] X : {wcd.X.Value},Y : {wcd.Y.Value},Z : { wcd.Z.Value}");
                        LoggerManager.Debug($"[IndexAlign Registe Edge Position] Center  X : {WaferObject.GetSubsInfo().WaferCenter.GetX()},Y : {WaferObject.GetSubsInfo().WaferCenter.GetY()},Z : {WaferObject.GetSubsInfo().WaferCenter.GetZ()}");

                        double x = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetX();
                        double y = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert().GetY();

                        EdgePos[indexpos] = new WaferCoordinate(x, y);

                        if (index == -1)
                        {
                            IndexAlignEdgeParam_Clone.EdgeParams.Value.Add(new EdgeIndexAlignParam(wcd, direction));
                        }
                        else
                        {
                            MachineIndex _idx = this.WaferAligner().WPosToMIndex(IndexAlignEdgeParam_Clone.EdgeParams.Value[index].Position);
                            IndexAlignEdgeParam_Clone.EdgeParams.Value[index] = new EdgeIndexAlignParam(wcd, direction);
                        }

                        int registerededges = IndexAlignEdgeParam_Clone?.EdgeParams?.Value?.Count ?? 0;

                        if (registerededges > 0)
                        {
                            SaveDevParameter();
                        }

                        StepLabel = String.Format("Edge  {0}/{1}", registerededges, EdgePos.Count);

                        if (IndexAlignEdgeParam_Clone.EdgeParams.Value.Count >= 4)
                        {
                            Validation();
                        }

                        await NextEdge();
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.EdgeFindFailTitle, Properties.Resources.EdgeFindFailMessage, EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.EdgeFindFailTitle, Properties.Resources.CameraErrorMessage, EnumMessageStyle.Affirmative);
                }

                UpdatePnpUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private EventCodeEnum Processing(EnumIndexAlignDirection direction, int AllowableRange, int Threshold, bool isSaveAnalysisImage = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.VisionManager().StopGrab(CurCam.GetChannelType());

                double retval = 0.0;

                int? quadrant_Pos = null;

                switch (direction)
                {
                    case EnumIndexAlignDirection.RIGHTUPPER:
                        quadrant_Pos = 0;
                        break;
                    case EnumIndexAlignDirection.LEFTUPPER:
                        quadrant_Pos = 1;
                        break;
                    case EnumIndexAlignDirection.LEFTLOWER:
                        quadrant_Pos = 2;
                        break;
                    case EnumIndexAlignDirection.RIGHTLOWER:
                        quadrant_Pos = 3;
                        break;
                    default:

                        break;
                }

                ImageBuffer image = null;

                if (quadrant_Pos != null)
                {
                    image = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);
                    retval = this.VisionManager().EdgeFind_IndexAlign(image, (int)quadrant_Pos, AllowableRange, Threshold);

                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                }
                else
                {
                    // ERROR
                }

                // 이미지 저장 시, 현재 조명값 정보를 넣기 위해서 사용
                string curLightInfo = "";
                foreach (var light in CurCam.LightsChannels)
                {
                    curLightInfo += string.IsNullOrEmpty(curLightInfo)
                        ? $"{light.Type.Value}_{CurCam.GetLight(light.Type.Value)}"
                        : $"_{light.Type.Value}_{CurCam.GetLight(light.Type.Value)}";
                }

                // Emul일때, EdgeFind_IndexAlign() retrun 값이 무조건 0으로 나오기 때문에 Emul에서는 조건을 보지않고 NONE 처리하여 넘어갈 수 있도록 처리함.
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    if (!isSaveAnalysisImage) //이값이 false 라는 건 device에 설정된 조명 value로 검사한 것임, 실패시 failimage 폴더에 저장해야함
                    {
                        if (retval > Threshold)
                        {
                            LoggerManager.Debug($"IndexAlign_FindEdge(Success) : Result = {retval}, wafer center X={WaferObject.GetSubsInfo().WaferCenter.GetX()}, Y={WaferObject.GetSubsInfo().WaferCenter.GetY()}", isInfo: true);
                            retVal = EventCodeEnum.NONE;
                            //string SaveBasePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\IndexEdgeImage", "SuccesImage", $"\\EdgeImage_[{quadrant_Pos}]_{curLightInfo}_{retval}");
                            //this.VisionManager().SaveImageBuffer(image, SaveBasePath, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);
                        }
                        else
                        {
                            LoggerManager.Error($"IndexAlign_FindEdge(Fail) : Result = {retval}, wafer center X={WaferObject.GetSubsInfo().WaferCenter.GetX()}, Y={WaferObject.GetSubsInfo().WaferCenter.GetY()}");
                            LoggerManager.Debug($"Edge Not Found(IndexAlign) (position : [{quadrant_Pos}])");
                            string SaveBasePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\IndexEdgeImage", "FailImage", $"\\EdgeFailImage_[{quadrant_Pos}]_{curLightInfo}_{retval}");
                            this.VisionManager().SaveImageBuffer(image, SaveBasePath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }
                    }
                    else
                    {
                        //이값이 true라는건 에러발생한 edge에 대해 분석 용으로 추가적인 조명값으로 processing 한것임, 무조건 이미지 저장해야 함
                        #region [Autolight] 추후 알고리즘 개선을 위한 분석용 데이터 수집용으로 이미지를 저장하는 코드가 들어감. 추후 삭제될 수 있음.
                        retVal = EventCodeEnum.NONE;
                        LoggerManager.Debug($"Save images for analysis(IndexAlign) (position : [{quadrant_Pos}])");
                        string SaveBasePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\IndexEdgeImage", "AnalysisImage", $"\\EdgeAnalysisImage_[{quadrant_Pos}]_{curLightInfo}_{retval}");
                        this.VisionManager().SaveImageBuffer(image, SaveBasePath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        #endregion
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


        private EventCodeEnum Validation()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                List<WaferProcResult> procresults = new List<WaferProcResult>();

                for (int index = 0; index < IndexAlignEdgeParam_Clone.EdgeParams.Value.Count; index++)
                {

                    procresults.Add(new WaferProcResult(
                        new WaferCoordinate(
                        (IndexAlignEdgeParam_Clone.EdgeParams.Value[index].Position.GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX()),
                        (IndexAlignEdgeParam_Clone.EdgeParams.Value[index].Position.GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY()),
                        (IndexAlignEdgeParam_Clone.EdgeParams.Value[index].Position.GetZ() + WaferObject.GetSubsInfo().WaferCenter.GetZ()))));
                }

                double a = 0.0;
                double b = 0.0;
                double c = 0.0;
                double d = 0.0;
                double e = 0.0;
                double f = 0.0;

                double chuckzeroAveXpos = 0.0;
                double chuckzeroAveYpos = 0.0;

                Point[] tmpGCPWaferCen = new Point[4];

                LoggerManager.Debug($"Q1 xpos:{procresults[0].ResultPos.X.Value} ypos{procresults[0].ResultPos.Y.Value}", isInfo: true);
                LoggerManager.Debug($"Q2 xpos:{procresults[1].ResultPos.X.Value} ypos{procresults[1].ResultPos.Y.Value}", isInfo: true);
                LoggerManager.Debug($"Q3 xpos:{procresults[2].ResultPos.X.Value} ypos{procresults[2].ResultPos.Y.Value}", isInfo: true);
                LoggerManager.Debug($"Q4 xpos:{procresults[3].ResultPos.X.Value} ypos{procresults[3].ResultPos.Y.Value}", isInfo: true);

                double distancex = 2 * (procresults[1].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                double distancey = 2 * (procresults[1].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);

                //case1

                a = 2 * (procresults[1].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                b = 2 * (procresults[1].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);
                c = Math.Pow(procresults[1].ResultPos.X.Value, 2.0) - Math.Pow(procresults[0].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[1].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[0].ResultPos.Y.Value, 2.0);
                d = 2 * (procresults[2].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                e = 2 * (procresults[2].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);
                f = Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[0].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[2].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[0].ResultPos.Y.Value, 2.0);

                tmpGCPWaferCen[0].X = ((c * e - f * b) / (e * a - b * d));
                tmpGCPWaferCen[0].Y = ((c * d - a * f) / (d * b - a * e));

                //case2
                a = 2 * (procresults[2].ResultPos.X.Value - procresults[1].ResultPos.X.Value);
                b = 2 * (procresults[2].ResultPos.Y.Value - procresults[1].ResultPos.Y.Value);
                c = Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.Y.Value, 2.0);
                d = 2 * (procresults[3].ResultPos.X.Value - procresults[1].ResultPos.X.Value);
                e = 2 * (procresults[3].ResultPos.Y.Value - procresults[1].ResultPos.Y.Value);
                f = Math.Pow(procresults[3].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[3].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[1].ResultPos.Y.Value, 2.0);

                tmpGCPWaferCen[1].X = ((c * e - f * b) / (e * a - b * d));
                tmpGCPWaferCen[1].Y = ((c * d - a * f) / (d * b - a * e));

                //case3
                a = 2 * (procresults[3].ResultPos.X.Value - procresults[2].ResultPos.X.Value);
                b = 2 * (procresults[3].ResultPos.Y.Value - procresults[2].ResultPos.Y.Value);
                c = Math.Pow(procresults[3].ResultPos.X.Value, 2.0) - Math.Pow(procresults[2].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[3].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[2].ResultPos.Y.Value, 2.0);
                d = 2 * (procresults[0].ResultPos.X.Value - procresults[2].ResultPos.X.Value);
                e = 2 * (procresults[0].ResultPos.Y.Value - procresults[2].ResultPos.Y.Value);
                f = Math.Pow(procresults[0].ResultPos.X.Value, 2.0) - Math.Pow(procresults[2].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[0].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[2].ResultPos.Y.Value, 2.0);

                tmpGCPWaferCen[2].X = ((c * e - f * b) / (e * a - b * d));
                tmpGCPWaferCen[2].Y = ((c * d - a * f) / (d * b - a * e));

                //case4
                a = 2 * (procresults[0].ResultPos.X.Value - procresults[3].ResultPos.X.Value);
                b = 2 * (procresults[0].ResultPos.Y.Value - procresults[3].ResultPos.Y.Value);
                c = Math.Pow(procresults[0].ResultPos.X.Value, 2.0) - Math.Pow(procresults[3].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[0].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[3].ResultPos.Y.Value, 2.0);
                d = 2 * (procresults[1].ResultPos.X.Value - procresults[3].ResultPos.X.Value);
                e = 2 * (procresults[1].ResultPos.Y.Value - procresults[3].ResultPos.Y.Value);
                f = Math.Pow(procresults[1].ResultPos.X.Value, 2.0) - Math.Pow(procresults[3].ResultPos.X.Value, 2.0) +
                    Math.Pow(procresults[1].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[3].ResultPos.Y.Value, 2.0);

                tmpGCPWaferCen[3].X = ((c * e - f * b) / (e * a - b * d));
                tmpGCPWaferCen[3].Y = ((c * d - a * f) / (d * b - a * e));

                bool[] CEN_CHECK_FLAG = new bool[2];

                CEN_CHECK_FLAG[0] = true;
                CEN_CHECK_FLAG[1] = true;

                //if ((Math.Abs(tmpGCPWaferCen[0].X - tmpGCPWaferCen[1].X) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[0].X - tmpGCPWaferCen[2].X) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[0].X - tmpGCPWaferCen[3].X) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[1].X - tmpGCPWaferCen[2].X) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[1].X - tmpGCPWaferCen[3].X) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[2].X - tmpGCPWaferCen[3].X) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value)
                //    )
                //{
                //    CEN_CHECK_FLAG[0] = false;
                //}

                //if ((Math.Abs(tmpGCPWaferCen[0].Y - tmpGCPWaferCen[1].Y) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[0].Y - tmpGCPWaferCen[2].Y) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[0].Y - tmpGCPWaferCen[3].Y) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[1].Y - tmpGCPWaferCen[2].Y) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[1].Y - tmpGCPWaferCen[3].Y) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value) ||
                //    (Math.Abs(tmpGCPWaferCen[2].Y - tmpGCPWaferCen[3].Y) > EdgeStandardParam.gIntEdgeDetectProcTolerance.Value)
                //    )
                //{
                //    CEN_CHECK_FLAG[1] = false;
                //}

                if ((CEN_CHECK_FLAG[0] == true) && (CEN_CHECK_FLAG[1] == true))
                {
                    chuckzeroAveXpos = (tmpGCPWaferCen[0].X + tmpGCPWaferCen[1].X + tmpGCPWaferCen[2].X + tmpGCPWaferCen[3].X) / 4;
                    chuckzeroAveYpos = (tmpGCPWaferCen[0].Y + tmpGCPWaferCen[1].Y + tmpGCPWaferCen[2].Y + tmpGCPWaferCen[3].Y) / 4;

                    LoggerManager.Debug($"Wafer Center xpos:{chuckzeroAveXpos} ypos{chuckzeroAveYpos}", isInfo: true);

                    MachineIndex DieMI = this.CoordinateManager().UserIndexConvertToMachineIndex(new UserIndex(WaferObject.GetPhysInfo().CenU.XIndex.Value, WaferObject.GetPhysInfo().CenU.YIndex.Value));
                    var CenterPos = this.WaferAligner().MachineIndexConvertToDieCenter(DieMI.XIndex, DieMI.YIndex);
                    IndexAlignEdgeParam_Clone.WaferCentertoTargetX.Value = Math.Abs(CenterPos.GetX() - chuckzeroAveXpos);
                    IndexAlignEdgeParam_Clone.WaferCentertoTargetY.Value = Math.Abs(CenterPos.GetY() - chuckzeroAveYpos);
                    IndexAlignEdgeParam_Clone.CenMregist_X.Value = DieMI.XIndex;
                    IndexAlignEdgeParam_Clone.CenMregist_Y.Value = DieMI.YIndex;
                    LoggerManager.Debug(string.Format("Index Align Edge - Wafer Center to TargetX xpos:{0} ypos{1}", IndexAlignEdgeParam_Clone.WaferCentertoTargetX.Value, IndexAlignEdgeParam_Clone.WaferCentertoTargetY.Value));
                    LoggerManager.Debug(string.Format("Index Align Edge - Wafer Center die X :{0} Y: {1}", IndexAlignEdgeParam_Clone.CenMregist_X.Value, IndexAlignEdgeParam_Clone.CenMregist_Y.Value));

                    double[] CLength = new double[procresults.Count()];
                    double[] CLengthRDiff = new double[procresults.Count()];
                    double lRadius;

                    lRadius = Math.Sqrt(Math.Pow((EdgePos[2].X.Value - EdgePos[0].X.Value), 2) + Math.Pow((EdgePos[2].Y.Value - EdgePos[0].Y.Value), 2)) / 2.0;

                    for (int i = 0; i < procresults.Count(); i++)
                    {
                        CLength[i] = Math.Sqrt(Math.Pow((procresults[i].ResultPos.X.Value - chuckzeroAveXpos), 2) + Math.Pow((procresults[i].ResultPos.Y.Value - chuckzeroAveYpos), 2));
                        CLengthRDiff[i] = Math.Abs(lRadius - CLength[i]);
                    }

                    LoggerManager.Debug($"Distance Q1 : {CLength[0]}", isInfo: true);
                    LoggerManager.Debug($"Distance Q2 : {CLength[1]}", isInfo: true);
                    LoggerManager.Debug($"Distance Q3 : {CLength[2]}", isInfo: true);
                    LoggerManager.Debug($"Distance Q4 : {CLength[3]}", isInfo: true);

                    if ((CLengthRDiff[0] < IndexAlignEdgeParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        (CLengthRDiff[1] < IndexAlignEdgeParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        (CLengthRDiff[2] < IndexAlignEdgeParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        (CLengthRDiff[3] < IndexAlignEdgeParam_Clone.gIntEdgeDetectProcToleranceRad.Value))
                    {

                        RetVal = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - Calculation() : Error occured.");
                throw err;
            }
            finally
            {
                //ProcessDialog.CloseDialg(this);
            }

            return RetVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;

            try
            {
                IsParamChanged = true;

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
                if (IndexAlignEdgeParam_Clone.AlignEnable == EnumWASubModuleEnable.DISABLE)
                {
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                    return;
                }

                if (IndexAlignEdgeParam_Clone.EdgeParams.Value.Count >= 4)
                {
                    EventCodeEnum retVal = Extensions_IParam.ElementStateNeedSetupValidation(IndexAlignEdgeParam_Clone as WA_IndexAlignParam_Edge);

                    if (GetState() != SubModuleStateEnum.DONE)
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }

                    if (retVal == EventCodeEnum.NONE)
                    {
                        SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                    }
                    else
                    {
                        SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                    }
                }
                else
                {
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum ClearSettingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IndexAlignEdgeParam_Clone = IndexAlignEdgeParam_IParam as WA_IndexAlignParam_Edge;

                if ((this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING
                      & this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                      | this.WaferAligner().IsNewSetup)
                {
                    SubModuleState = new SubModuleIdleState(this);
                }

                if (IndexAlignEdgeParam_Clone != null)
                {
                    if (this.WaferAligner().IsNewSetup)
                    {
                        LoggerManager.Debug($"IndexAlignEdge ClearSettingData.", isInfo: true);
                        IndexAlignEdgeParam_Clone.AlignEnable = EnumWASubModuleEnable.DISABLE;
                        IndexAlignEdgeParam_Clone.AllowableRange.Value = 64;
                        IndexAlignEdgeParam_Clone.AlignThreshold.Value = 20;
                        IndexAlignEdgeParam_Clone.PositionToleranceWCtoTargetdie.Value = 0;
                        IndexAlignEdgeParam_Clone.WaferCentertoTargetX.Value = 0;
                        IndexAlignEdgeParam_Clone.WaferCentertoTargetY.Value = 0;
                        IndexAlignEdgeParam_Clone.CenMregist_X.Value = 0;
                        IndexAlignEdgeParam_Clone.CenMregist_Y.Value = 0;
                        IndexAlignEdgeParam_Clone.EdgeParams.Value.Clear();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void UpdateLabel()
        {
            return;
        }

        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(IndexAlignEdgeParam_Clone));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
