using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WAEdgeStadnardModule
{
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using System.Windows;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Collections.ObjectModel;
    using ProberInterfaces.PnpSetup;
    using System.Threading;
    using SubstrateObjects;
    using PnPControl;
    using ProberInterfaces.Align;
    using ProberErrorCode;
    using WA_EdgeParameter_Standard;
    using ProberInterfaces.WaferAlignEX.Enum;
    using LogModule;
    using ProberInterfaces.State;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;
    using SerializerUtil;
    using MetroDialogInterfaces;
    using ProberInterfaces.WaferAligner;
    using ProberInterfaces.Vision;
    using ProberInterfaces.WaferAlignEX;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using Autofac.Features.Indexed;
    using System.IO.Compression;
    using System.Reflection;

    [Serializable, DataContract]
    public class EdgeStandard : PNPSetupBase, ISetup, IRecovery, IProcessingModule, IParamNode, INotifyPropertyChanged, IHasDevParameterizable, ILotReadyAble, IPackagable, IWaferEdgeProcModule, IHasAdvancedSetup
    {
        public override bool Initialized { get; set; } = false;

        [DataMember]
        public override Guid ScreenGUID { get; } = new Guid("5B98472E-6F6D-CDA3-20E1-53EF541856F4");
        enum WAEdgeSetupFunction
        {
            UNDIFINE = -1,
            MANAUALEDGE,
            APPLY
        }

        enum EnumEdgeDirection
        {
            UNDIFIND = -1,
            RIGHTUPPER = UNDIFIND + 1,
            LEFTUPPER,
            LEFTLOWER,
            RIGHTLOWER,
        }
        [DataMember]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        [DataMember]
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
        [DataMember]
        public new List<object> Nodes { get; set; }

        private WAEdgeSetupFunction ModifyCondition;

        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }

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

        private IParam _Edge_IParam;
        [ParamIgnore]
        [DataMember]
        public IParam Edge_IParam
        {
            get { return _Edge_IParam; }
            set
            {
                if (value != _Edge_IParam)
                {
                    _Edge_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        [DataMember]
        public SubModuleMovingStateBase MovingState { get; set; }

        private SubModuleStateBase _SubModuleState;
        [DataMember]
        public SubModuleStateBase SubModuleState
        {
            get { return _SubModuleState; }
            set
            {
                if (value != _SubModuleState)
                {
                    _SubModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IFocusing _WaferEdgeFocusModel;
        [DataMember]
        public IFocusing WaferEdgeFocusModel
        {
            get
            {
                if (_WaferEdgeFocusModel == null)
                    _WaferEdgeFocusModel = this.FocusManager().GetFocusingModel(EdgeStandardParam_Clone.FocusingModuleDllInfo);

                return _WaferEdgeFocusModel;
            }
        }

        private IFocusParameter FocusParam => EdgeStandardParam_Clone.FocusParam;


        [DataMember]
        public bool IsSetAlign = false;
        private WA_EdgeParam_Standard EdgeStandardParam_Clone;

        private List<WaferProcResult> procresults = new List<WaferProcResult>();

        private int _CurEdgeIndex = 0;

        private bool IsManualMode { get; set; } = false;

        private List<EdgeMapParam> _ManualEdgePoss
             = new List<EdgeMapParam>();
        [DataMember]
        public List<EdgeMapParam> ManualEdgePoss
        {
            get { return _ManualEdgePoss; }
            set { _ManualEdgePoss = value; }
        }

        //private List<WaferCoordinate> ManualEdgePoss = new List<WaferCoordinate>();
        private List<EdgeMapParam> EdgeMapParams = new List<EdgeMapParam>();


        private void SetWaferMap()
        {
            try
            {
                //if (!IsManualMode)
                //{
                foreach (var edgeparam in EdgeMapParams)
                {
                    if (WaferObject.GetSubsInfo().DIEs[edgeparam.Index.XIndex, edgeparam.Index.YIndex] != null)
                        WaferObject.GetSubsInfo().DIEs[edgeparam.Index.XIndex, edgeparam.Index.YIndex].DieType.Value = edgeparam.DieType;
                }
                foreach (var edgeparam in ManualEdgePoss)
                {
                    var existedge = EdgeMapParams.Find(
                        index => index.Index.XIndex == edgeparam.Index.XIndex & index.Index.YIndex == edgeparam.Index.YIndex);
                    if (existedge != null)
                    {
                        if (WaferObject.GetSubsInfo().DIEs[edgeparam.Index.XIndex, edgeparam.Index.YIndex] != null)
                            WaferObject.GetSubsInfo().DIEs[edgeparam.Index.XIndex, edgeparam.Index.YIndex].DieType.Value = existedge.DieType;
                    }
                    else
                    {
                        if (WaferObject.GetSubsInfo().DIEs[edgeparam.Index.XIndex, edgeparam.Index.YIndex] != null)
                            WaferObject.GetSubsInfo().DIEs[edgeparam.Index.XIndex, edgeparam.Index.YIndex].DieType.Value = edgeparam.DieType;
                    }
                }

                //var devices = WaferObject.GetDevices().FindAll(dev => dev.DieType.Value == DieTypeEnum.CHANGEMARK_DIE
                //| dev.DieType.Value == DieTypeEnum.MODIFY_DIE);
                //foreach (var dev in devices)
                //{
                //    dev.DieType.Value = DieTypeEnum.MARK_DIE;
                //}
                //}
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        private ObservableCollection<WaferCoordinate> _EdgePos
             = new ObservableCollection<WaferCoordinate>();
        [ParamIgnore]
        [DataMember]
        public ObservableCollection<WaferCoordinate> EdgePos
        {
            get { return _EdgePos; }
            set { _EdgePos = value; }
        }


        public EdgeStandard()
        {

        }


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
                LoggerManager.Error($"{err.ToString() }.EdgeStandard- InitModule() : Error occurred.");

                LoggerManager.Exception(err);

                throw err;
            }

            return retval;
        }


        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }


        #region ..//Command & CommandMethod



        private AsyncCommand _ManualEdgeCommand;
        public ICommand ManualEdgeCommand
        {
            get
            {
                if (null == _ManualEdgeCommand) _ManualEdgeCommand = new AsyncCommand(ManualEdge);
                return _ManualEdgeCommand;
            }
        }
        private async Task ManualEdge()
        {
            try
            {
                ModifyCondition = WAEdgeSetupFunction.MANAUALEDGE;
                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //SetWaferMap();

                if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.DISABLE)
                    WaferObject.GetSubsInfo().WaferCenter = new WaferCoordinate();
                if (parameter is EventCodeEnum)
                {
                    if ((EventCodeEnum)parameter == EventCodeEnum.NONE)
                        await base.Cleanup(parameter);
                    return retVal;
                }
                retVal = await base.Cleanup(parameter);

                //if (retVal == EventCodeEnum.NONE)
                //{
                //    //if (this.WaferAligner().GetWAInnerStateEnum() == ProberInterfaces.Align.WaferAlignInnerStateEnum.RECOVERY)
                //    if(IsManualMode)
                //    {
                //        foreach (var device in EdgeMapParams)
                //        {
                //            Wafer.GetSubsInfo().Devices.ToList<DeviceObject>().Find(item => item.DieIndexM.XIndex == device.Index.XIndex
                //                && item.DieIndexM.YIndex == device.Index.YIndex)
                //                             .State.Value = device.DeviceState;
                //        }
                //    }

                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }


        public async Task Apply()
        {
            try
            {
                ModifyCondition = WAEdgeSetupFunction.APPLY;
                SetNextStepsNotCompleteState((PnpManager.SelectedPnpStep as ICategoryNodeItem).Header);
                if (this.WaferAligner().IsNewSetup)
                {
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Clear();
                    this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Clear();
                }

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async Task RegisteLeftUpperEdgeCommand()
        {
            try
            {
                ModifyCondition = WAEdgeSetupFunction.MANAUALEDGE;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private async Task RegisteRightUpperEdgeCommand()
        {
            try
            {
                ModifyCondition = WAEdgeSetupFunction.MANAUALEDGE;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private async Task RegisteRightLowerEdgeCommand()
        {
            try
            {
                ModifyCondition = WAEdgeSetupFunction.MANAUALEDGE;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private async Task RegisteLeftLowerEdgeCommand()
        {
            try
            {
                ModifyCondition = WAEdgeSetupFunction.MANAUALEDGE;

                await Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion


        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }

        private bool ExistManualEdge(int index)
        {
            bool retVal = false;
            try
            {
                switch (index)
                {
                    case (int)EnumEdgeDirection.RIGHTUPPER:
                        if (ManualEdgePoss.Count != 0)
                        {
                            foreach (var pos in ManualEdgePoss)
                            {
                                if (pos.Coordinate.GetX() > 0 & pos.Coordinate.GetY() > 0)
                                    retVal = true;
                            }
                        }
                        break;
                    case (int)EnumEdgeDirection.LEFTUPPER:
                        if (ManualEdgePoss.Count != 0)
                        {
                            foreach (var pos in ManualEdgePoss)
                            {
                                if (pos.Coordinate.GetX() < 0 & pos.Coordinate.GetY() > 0)
                                    retVal = true;
                            }
                        }
                        break;
                    case (int)EnumEdgeDirection.LEFTLOWER:
                        if (ManualEdgePoss.Count != 0)
                        {
                            foreach (var pos in ManualEdgePoss)
                            {
                                if (pos.Coordinate.GetX() < 0 & pos.Coordinate.GetY() < 0)
                                    retVal = true;
                            }
                        }
                        break;
                    case (int)EnumEdgeDirection.RIGHTLOWER:
                        if (ManualEdgePoss.Count != 0)
                        {
                            foreach (var pos in ManualEdgePoss)
                            {
                                if (pos.Coordinate.GetX() > 0 & pos.Coordinate.GetY() < 0)
                                    retVal = true;
                            }
                        }
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private Task PrevEdge()
        {
            try
            {

                if (_CurEdgeIndex - 1 >= 0)
                {
                    _CurEdgeIndex--;
                    if (ExistManualEdge(_CurEdgeIndex))
                    {
                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove
                            (EdgePos[_CurEdgeIndex].GetX(),
                            EdgePos[_CurEdgeIndex].GetY(),
                            EdgePos[_CurEdgeIndex].GetZ());
                    }
                }
                else if (_CurEdgeIndex == 0)
                {
                    _CurEdgeIndex = EdgePos.Count - 1;
                    if (ExistManualEdge(_CurEdgeIndex))
                    {
                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove
                            (EdgePos[_CurEdgeIndex].GetX(),
                            EdgePos[_CurEdgeIndex].GetY(),
                            EdgePos[_CurEdgeIndex].GetZ());
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
        private Task NextEdge()
        {
            try
            {

                if (_CurEdgeIndex + 1 < EdgePos.Count)
                {
                    _CurEdgeIndex++;
                    if (ExistManualEdge(_CurEdgeIndex))
                    {
                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove
                            (EdgePos[_CurEdgeIndex].GetX(),
                            EdgePos[_CurEdgeIndex].GetY(),
                            EdgePos[_CurEdgeIndex].GetZ());
                    }
                }
                else if (_CurEdgeIndex + 1 == EdgePos.Count)
                {
                    _CurEdgeIndex = 0;
                    if (ExistManualEdge(_CurEdgeIndex))
                    {
                        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove
                                (ManualEdgePoss[_CurEdgeIndex].Coordinate.GetX()
                                + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetY()
                                + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                                ManualEdgePoss[_CurEdgeIndex].Coordinate.GetZ()
                                + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                        }
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove
                            (EdgePos[_CurEdgeIndex].GetX(),
                            EdgePos[_CurEdgeIndex].GetY(),
                            EdgePos[_CurEdgeIndex].GetZ());
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


        private Task SetLight()
        {
            try
            {
                EdgeStandardParam_Clone.LightParams = new ObservableCollection<LightValueParam>();
                for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                {
                    EdgeStandardParam_Clone.LightParams.Add(
                        new LightValueParam(CurCam.LightsChannels[index].Type.Value,
                        (ushort)CurCam.GetLight(CurCam.LightsChannels[index].Type.Value)));
                }

                Edge_IParam = EdgeStandardParam_Clone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        public EventCodeEnum DoExecute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Wafer.GetSubsInfo().WaferCenter == null)
                {
                    Wafer.GetSubsInfo().WaferCenter = new WaferCoordinate();
                }

                if (Wafer.GetSubsInfo().WaferCenterOriginatEdge == null) 
                {
                    Wafer.GetSubsInfo().WaferCenterOriginatEdge = new WaferCoordinate();
                }
                Wafer.GetSubsInfo().WaferCenter.Z.Value = Wafer.GetPhysInfo().Thickness.Value;
                Wafer.GetSubsInfo().WaferCenterOriginatEdge.Z.Value = Wafer.GetPhysInfo().Thickness.Value;

                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Edge_Start);

                if (this.WaferAligner().GetWAInnerStateEnum() == ProberInterfaces.Align.WaferAlignInnerStateEnum.ALIGN)
                {
                    if (GetState() == SubModuleStateEnum.IDLE)
                    {
                        if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.ENABLE | this.WaferAligner().WaferAlignInfo.LotLoadingPosCheckMode)
                            RetVal = Processing();
                        else if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.THETA_RETRY)
                            RetVal = EventCodeEnum.SUB_SKIP;
                        else if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.DISABLE)
                            RetVal = EventCodeEnum.NONE;
                    }
                    else if (GetState() == SubModuleStateEnum.SKIP)
                    {
                        if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.ENABLE)
                            RetVal = Processing();
                        else if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.THETA_RETRY)
                            RetVal = Processing();
                        else if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.DISABLE)
                            RetVal = EventCodeEnum.NONE;
                    }
                    else if (GetState() == SubModuleStateEnum.DONE)
                    {
                        RetVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        if (!IsManualMode)
                        {
                            RetVal = Processing();
                        }

                        else
                        {
                            procresults.Clear();
                            foreach (var manuledgepos in ManualEdgePoss)
                            {
                                procresults.Add(new WaferProcResult(manuledgepos.Coordinate));
                            }
                            RetVal = Calculation();
                        }
                    }

                }
                else
                { 
                    if (this.WaferAligner().GetWAInnerStateEnum() == ProberInterfaces.Align.WaferAlignInnerStateEnum.SETUP && this.WaferAligner().IsNewSetup == true)
                    {
                        if (!IsManualMode)
                        {
                            RetVal = Processing();
                        }
                        else
                        {
                            if (ManualEdgePoss.Count != 4)
                            {
                                var mret = this.MetroDialogManager().ShowMessageDialog
                                    (Properties.Resources.ErrorMessageTitle, Properties.Resources.NotEnoughManualEdgeCountMessage, EnumMessageStyle.Affirmative).Result;
                            }
                            else
                            {
                                procresults.Clear();
                                foreach (var manuledgepos in ManualEdgePoss)
                                {
                                    procresults.Add(new WaferProcResult(manuledgepos.Coordinate));
                                }
                                RetVal = Calculation();
                            }
                        }
                    }
                    else
                    {
                        //this.WaferAligner().IsNewSetup == false -> recovery.
                        if ((!IsManualMode) || ManualEdgePoss.Count < 4)
                        {
                            //if (GetState() == SubModuleStateEnum.DONE
                            //    || GetState() == SubModuleStateEnum.IDLE)
                            //{
                            RetVal = Processing();
                            //}
                        }
                        else
                        {
                            procresults.Clear();
                            foreach (var manuledgepos in ManualEdgePoss)
                            {
                                procresults.Add(new WaferProcResult(manuledgepos.Coordinate));
                            }
                            RetVal = Calculation();
                        }
                    }
                }

                CreateHeightProfilingStandard();

                if (this.WaferAligner().WaferAlignControItems.EdgeFail)
                    RetVal = EventCodeEnum.SUB_RECOVERY;

                if (RetVal == EventCodeEnum.NONE)
                {
                    //======================= Test Code
                    (this).StageSupervisor().StageModuleState.WaferLowViewMove(
                            Wafer.GetSubsInfo().WaferCenter.X.Value,
                            Wafer.GetSubsInfo().WaferCenter.Y.Value,
                            Wafer.GetSubsInfo().WaferCenter.Z.Value);

                    //======================
                    ProcessingType = EnumSetupProgressState.DONE;
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    SubModuleState = new SubModuleDoneState(this);
                    if (EdgeStandardParam_Clone.EdgeMovement.Value == EnumWASubModuleEnable.DISABLE)
                    {
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Edge_Disable);
                    }
                    else
                    {
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Edge_OK);
                    }
                }
                else if (RetVal == EventCodeEnum.SUB_RECOVERY)
                {
                    //this.WaferAligner().ReasonOfError.Reason = "Edge Not Found";
                    this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "Edge Not Found", this.GetType().Name);

                    SubModuleState = new SubModuleRecoveryState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Edge_Failure, RetVal);
                }
                else if (RetVal == EventCodeEnum.SUB_SKIP)
                {
                    SubModuleState = new SubModuleSkipState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_EdgeDection_Skip);
                }
                else
                {
                    if (RetVal == EventCodeEnum.WAFER_EDGE_NOT_FOUND)
                    {
                        //this.WaferAligner().ReasonOfError.Reason = "Edge Not Found";
                        this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "Edge Not Found", this.GetType().Name);
                    }
                    else
                    {
                        //this.WaferAligner().ReasonOfError.Reason = "Edge Error occured" + RetVal;
                        this.WaferAligner().ReasonOfError.AddEventCodeInfo(RetVal, "Edge Error occured", this.GetType().Name);
                    }

                    this.NotifyManager().Notify(EventCodeEnum.WAFER_EDGE_NOT_FOUND);
                    SubModuleState = new SubModuleErrorState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Edge_Failure, RetVal);
                }

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Edge_Failure);

                throw err;
            }
            finally
            {
                //this.VisionManager().ClearGrabberUserImage(CurCam.GetChannelType());
            }
            return RetVal;
        }


        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = Properties.Resources.Header;
                InitPnpModuleStage_AdvenceSetting();

                //AdvanceSetupView = new EdgeStandardAdSetting(Edge_IParam as WA_EdgeParam_Standard);
                //AdvanceSetupViewModel = (IPnpAdvanceSetupViewModel)AdvanceSetupView;
                AdvanceSetupView = new EdgeAdvanceSetup.View.EdgeStandardAdSetting();
                AdvanceSetupViewModel = new EdgeAdvanceSetup.ViewModel.EdgeStandardAdvanceSetupViewModel();

                InitPNPSetupUI();

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

        public override EventCodeEnum ClearSettingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IsManualMode = false;
                //ManualEdgePoss.Clear(); ISSD-4260 clearsettingdata 호출 시점 확인 필요 clear 해주는게 맞는 것인가? wafer obj idle 되는 경우 초기화
                EdgeMapParams.Clear();
                EdgeStandardParam_Clone = Edge_IParam as WA_EdgeParam_Standard;
                SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);

                if (this.WaferAligner().IsNewSetup)
                {
                    SubModuleState = new SubModuleIdleState(this);
                }

                if (this.WaferAligner().IsNewSetup)
                    EdgeStandardParam_Clone.LightParams.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(Edge_IParam));
                MiniViewTarget = this.StageSupervisor().WaferObject;
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                InitStateUI();

                IsManualMode = false;
                ManualEdgePoss.Clear();

                if ((Edge_IParam as WA_EdgeParam_Standard).EdgeMovement.Value == EnumWASubModuleEnable.DISABLE)
                {
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                }
                else
                {
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }

                SetEdgePosition();

                this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[0].GetX(), EdgePos[1].GetY(), WaferObject.GetSubsInfo().AveWaferThick);

                if (this.WaferAligner().IsNewSetup)
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
                CurCam = this.VisionManager().GetCam(EdgeStandardParam_Clone.CamType);
                if (EdgeStandardParam_Clone.LightParams.Count != 0)
                {
                    for (int index = 0; index < EdgeStandardParam_Clone.LightParams.Count; index++)
                    {
                        CurCam.SetLight(EdgeStandardParam_Clone.LightParams[index].Type.Value,
                            EdgeStandardParam_Clone.LightParams[index].Value.Value);

                    }
                }
                else
                {
                    EdgeStandardParam_Clone.DefaultLightValue = 200;
                    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                    {
                        CurCam.SetLight(CurCam.LightsChannels[index].Type.Value,
                            EdgeStandardParam_Clone.DefaultLightValue);
                    }
                }

                procresults = new List<WaferProcResult>(EdgePos.Count());

                this.VisionManager().StartGrab(EdgeStandardParam_Clone.CamType, this);

                UseUserControl = UserControlFucEnum.DEFAULT;

                MainViewTarget = DisplayPort;
                MiniViewTarget = this.StageSupervisor().WaferObject;

                InitPNPSetupUI();

                Wafer.MapViewStageSyncEnable = true;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.WAFER_SETUP_PROCEDURE_EROOR;
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void InitPNPSetupUI()
        {
            try
            {

                PadJogSelect.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Apply.png");
                PadJogSelect.IconCaption = "APPLY";
                PadJogSelect.Command = new AsyncCommand(Apply);

                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                PadJogLeftUp.IconCaption = "EDGE";
                PadJogLeftUp.Command = new AsyncCommand(PrevEdge);
                PadJogRightUp.IconCaption = "EDGE";
                PadJogRightUp.Command = new AsyncCommand(NextEdge);

                PadJogRightDown.Caption = "SETLIGHT";
                PadJogRightDown.Command = new AsyncCommand(SetLight);

                //PadJogLeftDown.Caption = "SETCENTER";
                //PadJogLeftDown.Command = new AsyncCommand(SetCenter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void InitPnpEdgeManualUI()
        {
            try
            {
                IsManualMode = true;

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
                ThreeButton.Command = new AsyncCommand(RegisteLeftLowerEdgeCommand);
                FourButton.Command = new AsyncCommand(RegisteRightLowerEdgeCommand);

                _CurEdgeIndex = 0;

                PadJogSelect.IsEnabled = true;

                this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[0].GetX(), EdgePos[1].GetY(), WaferObject.GetSubsInfo().AveWaferThick);

                foreach (var pos in EdgePos)
                {
                    MachineIndex edgeindex = this.CoordinateManager().GetCurMachineIndex(pos);

                    EdgeMapParams.Add(new EdgeMapParam(edgeindex, Wafer.GetSubsInfo().DIEs[edgeindex.XIndex, edgeindex.YIndex].DieType.Value));
                    //Wafer.GetSubsInfo().DIEs[edgeindex.XIndex, edgeindex.YIndex].DieType.Value = DieTypeEnum.CHANGEMARK_DIE;
                    //Wafer.GetSubsInfo().Devices.ToList<DeviceObject>().Find(item => item.DieIndexM.XIndex == edgeindex.XIndex
                    //     && item.DieIndexM.YIndex == edgeindex.YIndex)
                    //     .DieType.Value = DieTypeEnum.CHANGEMARK_DIE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CurCam = this.VisionManager().GetCam(EdgeStandardParam_Clone.CamType);

                if (EdgeStandardParam_Clone.LightParams.Count != 0)
                {
                    for (int index = 0; index < EdgeStandardParam_Clone.LightParams.Count; index++)
                    {
                        CurCam.SetLight(EdgeStandardParam_Clone.LightParams[index].Type.Value,
                            EdgeStandardParam_Clone.LightParams[index].Value.Value);
                    }
                }
                else
                {
                    EdgeStandardParam_Clone.DefaultLightValue = 200;
                    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                    {
                        CurCam.SetLight(CurCam.LightsChannels[index].Type.Value,
                            EdgeStandardParam_Clone.DefaultLightValue);
                    }
                }

                this.VisionManager().StartGrab(EdgeStandardParam_Clone.CamType, this);

                MainViewTarget = DisplayPort;
                MiniViewTarget = this.StageSupervisor().WaferObject;

                InitPNPSetupUI();
                InitPnpEdgeManualUI();

                Wafer.MapViewStageSyncEnable = true;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.ToString() }.EdgeStandard- InitRecovery() : Error occurred.");
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void SetEdgePosition()
        {
            try
            {
                EdgePos.Clear();

                if (ManualEdgePoss.Count != 4)
                {
                    //new setup 인 경우, new setup 에서 maual 해주지 않고 실패 나서 recovery 들어 온 경우

                    IPhysicalInfo physicalInfo = Wafer.GetPhysInfo();
                    double wSizeoffset = physicalInfo.WaferSize_Offset_um.Value;
                    double wSize = physicalInfo.WaferSize_um.Value + (wSizeoffset * 2);
                    LoggerManager.Debug($"SetEdgePosition(): Wafer Size = {wSize}, Waferoffset.Value = {wSizeoffset}");
                    double edgepos = 0.0;
                    edgepos = ((wSize / 2) / Math.Sqrt(2));

                    double chuck_center_Xoffset = this.CoordinateManager().StageCoord.ChuckCenterX.Value;
                    double chuck_center_Yoffset = this.CoordinateManager().StageCoord.ChuckCenterY.Value;

                    if (chuck_center_Xoffset < this.CoordinateManager().StageCoord.ChuckCenterX.LowerLimit || chuck_center_Xoffset > this.CoordinateManager().StageCoord.ChuckCenterX.UpperLimit
                        || chuck_center_Yoffset < this.CoordinateManager().StageCoord.ChuckCenterY.LowerLimit || chuck_center_Yoffset > this.CoordinateManager().StageCoord.ChuckCenterY.UpperLimit)
                    {
                        LoggerManager.Debug($"SetEdgePosition(): Center offset is out of range. X = {chuck_center_Xoffset:0.00}, y = {chuck_center_Yoffset:0.00}");
                        chuck_center_Xoffset = 0;
                        chuck_center_Yoffset = 0;
                    }
                    EdgePos.Add(new WaferCoordinate(edgepos + chuck_center_Xoffset, edgepos + chuck_center_Yoffset));
                    EdgePos.Add(new WaferCoordinate(-edgepos + chuck_center_Xoffset, edgepos + chuck_center_Yoffset));
                    EdgePos.Add(new WaferCoordinate(-edgepos + chuck_center_Xoffset, -edgepos + chuck_center_Yoffset));
                    EdgePos.Add(new WaferCoordinate(edgepos + chuck_center_Xoffset, -edgepos + chuck_center_Yoffset));
                }
                else
                {
                    //new setup 에서 manual 해준 경우, recovery 한 경우
                    LoggerManager.Debug($"SetEdgePosition(): EXIST ManualEdgePos, Count :{ManualEdgePoss.Count}");
                    foreach (var item in ManualEdgePoss)
                    {
                        EdgePos.Add(new WaferCoordinate(item.Coordinate));
                    }
                }

                LoggerManager.Debug($"SetEdgePosition Result: EdgePos[0]:({EdgePos[0].GetX():0.00}, {EdgePos[0].GetY():0.00}), EdgePos[1]:({EdgePos[1].GetX():0.00}, {EdgePos[1].GetY():0.00}), EdgePos[2]:({EdgePos[2].GetX():0.00}, {EdgePos[2].GetY():0.00}), EdgePos[3]:({EdgePos[3].GetX():0.00}, {EdgePos[3].GetY():0.00}), ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EventCodeEnum Processing()
        {

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;


            ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);

            ImageBuffer[] EdgeBuffer = null;
            ImageBuffer[] EdgeLineBuffer = null;
            try
            {
                //..[Trun Theta 0 ] 
                double curtpos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                curtpos = Math.Abs(curtpos);

                int converttpos = Convert.ToInt32(curtpos);

                if (converttpos != 0)
                {
                    //this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Wafer.GetSubsInfo().ActualThickness);
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                }
                //========================

                procresults.Clear();
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);

                this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM, this);


                SetEdgePosition();

                var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);


                this.MotionManager().SetSettlingTime(axisX, 0.001);
                this.MotionManager().SetSettlingTime(axisY, 0.001);

                if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP
                    || IsManualMode == true)
                {
                    EdgeStandardParam_Clone.LightParams.Clear();
                    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                    {

                        EdgeStandardParam_Clone.LightParams.Add(
                            new LightValueParam(CurCam.LightsChannels[index].Type.Value,
                            (ushort)CurCam.GetLight(CurCam.LightsChannels[index].Type.Value)));
                    }

                }

                foreach (var light in EdgeStandardParam_Clone.LightParams)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                EdgeBuffer = new ImageBuffer[EdgePos.Count];
                EdgeLineBuffer = new ImageBuffer[EdgePos.Count];

                this.VisionManager().StopGrab(CurCam.GetChannelType());

                for (int index = 0; index < EdgePos.Count; index++)
                {
                    RetVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[index].X.Value, EdgePos[index].Y.Value, Wafer.GetPhysInfo().Thickness.Value);
                    if (RetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug("WaferLowViewMove fail");
                        return RetVal;
                    }

                    if (this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                    {
                        this.VisionManager().LoadImageFromFileToGrabber(@"C:\ProberSystem\EmulImages\WaferAlign\Edge\Edge" + index + ".bmp", CurCam.GetChannelType());
                    }

                    EdgeBuffer[index] = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                    if (!this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                    {
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                    }

                    string SaveBasePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, false, "\\EDGE", $"\\Edge + {index}");
                    this.VisionManager().SaveImageBuffer(EdgeBuffer[index], SaveBasePath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                    EdgeLineBuffer[index] = this.VisionManager().Line_Equalization(EdgeBuffer[index], index);
                }
                RetVal = Edgedetection(EdgeBuffer, EdgeLineBuffer, axisX, axisY);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - Processing() : Error occured.");
                throw err;
            }
            finally
            {
                if (this.WaferAligner().WaferAlignControItems.IsDebugEdgeProcessing)
                {
                    string outputPath = this.FileManager().GetImageSavePath(EnumProberModule.WAFERALIGNER, true, "\\WaferEdge\\Debug");

                    for (int index = 0; index < EdgePos.Count; index++)
                    {
                        string dt = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var fullpath = Path.Combine(outputPath, $"{dt}_" + index.ToString()) + ".bmp";

                        this.VisionManager().SaveImageBuffer(EdgeBuffer[index], fullpath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                    }
                }

                if (this.VisionManager().GetVisionProcRaft() == EnumVisionProcRaft.EMUL)
                {
                    try
                    {
                        RetVal = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }

            return RetVal;
        }

        
        public EventCodeEnum Edgedetection(ImageBuffer[] EdgeBuffer, ImageBuffer[] EdgeLineBuffer, ProbeAxisObject axisX = null, ProbeAxisObject axisY = null) 
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            WaferCoordinate wafercoord = null;
            try
            {
                if (EdgeLineBuffer.ToList<ImageBuffer>().FindAll(img => img != null).Count() != 0)
                {
                    int Except_Pixel = this.CoordinateManager().StageCoord.EdgeProcessClampWidth.Value;


                    int Width = EdgeLineBuffer[0].SizeX;
                    int Heigh = EdgeLineBuffer[0].SizeY;

                    int RWidth = EdgeLineBuffer[0].SizeX - 1;
                    int RHeight = EdgeLineBuffer[0].SizeY - 1;

                    int i, ii, k, kk;

                    List<(int, int[], Point[])> edgeCandidatesResult = EdgeCandidatesResult(Except_Pixel, EdgeLineBuffer);

                    int ret_Edge0_count = edgeCandidatesResult[0].Item1;
                    int ret_Edge1_count = edgeCandidatesResult[1].Item1;
                    int ret_Edge2_count = edgeCandidatesResult[2].Item1;
                    int ret_Edge3_count = edgeCandidatesResult[3].Item1;

                    int[] ret_PMEdge0 = edgeCandidatesResult[0].Item2;
                    int[] ret_PMEdge1 = edgeCandidatesResult[1].Item2;
                    int[] ret_PMEdge2 = edgeCandidatesResult[2].Item2;
                    int[] ret_PMEdge3 = edgeCandidatesResult[3].Item2;

                    Point[] ret_Edge0 = edgeCandidatesResult[0].Item3;
                    Point[] ret_Edge1 = edgeCandidatesResult[1].Item3;
                    Point[] ret_Edge2 = edgeCandidatesResult[2].Item3;
                    Point[] ret_Edge3 = edgeCandidatesResult[3].Item3;

                    LoggerManager.Debug($"ret_Edge0_count: {ret_Edge0_count}, ret_Edge1_count:{ret_Edge1_count}, ret_Edge2_count:{ret_Edge2_count}, ret_Edge3_count:{ret_Edge3_count}", isInfo: true);

                    if (((ret_Edge0_count <= 0) || (ret_Edge1_count <= 0) || (ret_Edge2_count <= 0) || (ret_Edge3_count <= 0)) && axisX != null && axisY != null)
                    {
                        SaveEdgeFailImage(EdgeBuffer, EdgeLineBuffer);
                    }

                    int p0, p1, p2, p3;

                    int p_0_2;
                    int p_1_3;

                    double Len1, Len2;

                    bool DIAGFLAG0to2 = false;
                    bool DIAGFLAG1to3 = false;

                    int Interval_DiagPoint = 80;

                    int All_Count;

                    All_Count = ret_Edge0_count * ret_Edge1_count * ret_Edge2_count * ret_Edge3_count;

                    Point[,] Real_Pos = new Point[EdgePos.Count, All_Count];

                    //Point[] Real_Pos0 = new Point[All_Count];
                    //Point[] Real_Pos1 = new Point[All_Count];
                    //Point[] Real_Pos2 = new Point[All_Count];
                    //Point[] Real_Pos3 = new Point[All_Count];

                    double[] Real_Score = new double[All_Count];

                    int Real_Count = 0;

                    double Symmetry_Score;
                    //[EDGE 후보들에서 Edge 조합 찾아내기]
                    for (p0 = 0; p0 < ret_Edge0_count; p0++)
                    {
                        for (p1 = 0; p1 < ret_Edge1_count; p1++)
                        {
                            for (p2 = 0; p2 < ret_Edge2_count; p2++)
                            {
                                for (p3 = 0; p3 < ret_Edge3_count; p3++)
                                {
                                    if (((ret_PMEdge0[p0] > 0) && (ret_PMEdge1[p1] > 0) && (ret_PMEdge2[p2] > 0) && (ret_PMEdge3[p3] > 0)) ||
                                        ((ret_PMEdge0[p0] < 0) && (ret_PMEdge1[p1] < 0) && (ret_PMEdge2[p2] < 0) && (ret_PMEdge3[p3] < 0)))
                                    {
                                        Len1 = Math.Abs((ret_Edge0[p0].X + Width) - ret_Edge2[p2].X);
                                        Len2 = Math.Abs(ret_Edge1[p1].X - (ret_Edge3[p3].X + Width));

                                        if (Math.Abs(Len1 - Len2) < 50)
                                        {
                                            DIAGFLAG0to2 = false;
                                            DIAGFLAG1to3 = false;

                                            // Check  Diag Point (0) and (2), (1) and (3)

                                            for (p_0_2 = 0; p_0_2 < ret_Edge2_count; p_0_2++)
                                            {
                                                if ((((Width + ret_Edge0[p0].X) - (ret_Edge2[p_0_2].X)) < Width + Interval_DiagPoint))
                                                {
                                                    DIAGFLAG0to2 = true;
                                                }
                                            }

                                            for (p_1_3 = 0; p_1_3 < ret_Edge3_count; p_1_3++)
                                            {
                                                if ((((Width + ret_Edge3[p_1_3].X) - (ret_Edge1[p1].X)) < Width + Interval_DiagPoint))
                                                {
                                                    DIAGFLAG1to3 = true;
                                                }
                                            }

                                            if ((DIAGFLAG0to2 == true) && (DIAGFLAG1to3 == true))
                                            {
                                                Symmetry_Score = 0;

                                                for (int mm = 0; mm < 25; mm++)
                                                {
                                                    if ((ret_Edge0[p0].X > Except_Pixel) && (ret_Edge0[p0].X < Width - Except_Pixel) &&
                                                    (ret_Edge1[p1].X > Except_Pixel) && (ret_Edge1[p1].X < Width - Except_Pixel) &&
                                                    (ret_Edge2[p2].X > Except_Pixel) && (ret_Edge2[p2].X < Width - Except_Pixel) &&
                                                    (ret_Edge3[p3].X > Except_Pixel) && (ret_Edge3[p3].X < Width - Except_Pixel) &&
                                                    (ret_Edge0[p0].Y > Except_Pixel) && (ret_Edge0[p0].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge1[p1].Y > Except_Pixel) && (ret_Edge1[p1].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge2[p2].Y > Except_Pixel) && (ret_Edge2[p2].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge3[p3].Y > Except_Pixel) && (ret_Edge3[p3].Y < Heigh - Except_Pixel))
                                                    {
                                                        Symmetry_Score += Math.Abs(EdgeLineBuffer[0].Buffer[((int)ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[0].Buffer[(int)(ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[0].Buffer[(int)(ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)] - EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y - mm) * Width + ((int)ret_Edge3[p3].X - mm)]);
                                                    }
                                                    else
                                                    {
                                                        Symmetry_Score = 99999;
                                                    }
                                                }

                                                Real_Pos[0, Real_Count].X = ret_Edge0[p0].X;
                                                Real_Pos[0, Real_Count].Y = ret_Edge0[p0].Y;

                                                Real_Pos[1, Real_Count].X = ret_Edge1[p1].X;
                                                Real_Pos[1, Real_Count].Y = ret_Edge1[p1].Y;

                                                Real_Pos[2, Real_Count].X = ret_Edge2[p2].X;
                                                Real_Pos[2, Real_Count].Y = ret_Edge2[p2].Y;

                                                Real_Pos[3, Real_Count].X = ret_Edge3[p3].X;
                                                Real_Pos[3, Real_Count].Y = ret_Edge3[p3].Y;

                                                Real_Score[Real_Count] = Symmetry_Score;

                                                Real_Count++;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //[EDGE 후보들에서 점수 높은 순으로 SWAP 정렬]
                    for (i = 0; i < Real_Count - 1; i++)
                    {
                        for (ii = 1; ii < Real_Count - i; ii++)
                        {
                            if (Real_Score[ii - 1] > Real_Score[ii])
                            {
                                // 스코어 값 스왑
                                double swapScore = Real_Score[ii - 1];
                                Real_Score[ii - 1] = Real_Score[ii];
                                Real_Score[ii] = swapScore;

                                for (k = 0; k < EdgePos.Count; k++)
                                {
                                    // X 값 스왑
                                    double swapX = Real_Pos[k, ii].X;
                                    Real_Pos[k, ii].X = Real_Pos[k, ii - 1].X;
                                    Real_Pos[k, ii - 1].X = swapX;

                                    // Y 값 스왑
                                    double swapY = Real_Pos[k, ii].Y;
                                    Real_Pos[k, ii].Y = Real_Pos[k, ii - 1].Y;
                                    Real_Pos[k, ii - 1].Y = swapY;
                                }
                            }
                        }
                    }
                    // 성공할때까지 Edge 조합들 Calculation
                    IPhysicalInfo physicalInfo = Wafer.GetPhysInfo();
                    double wSizeoffset = physicalInfo.WaferSize_Offset_um.Value;

                    if (Real_Count == 0 &&
                        ret_Edge0_count > 0 && ret_Edge1_count > 0 && ret_Edge2_count > 0 && ret_Edge3_count > 0
                        && Math.Abs(wSizeoffset) > 0)
                    {
                        LoggerManager.Debug("The first index candidate has been used.");

                        Real_Pos[0, Real_Count].X = ret_Edge0[0].X;
                        Real_Pos[0, Real_Count].Y = ret_Edge0[0].Y;

                        Real_Pos[1, Real_Count].X = ret_Edge1[0].X;
                        Real_Pos[1, Real_Count].Y = ret_Edge1[0].Y;

                        Real_Pos[2, Real_Count].X = ret_Edge2[0].X;
                        Real_Pos[2, Real_Count].Y = ret_Edge2[0].Y;

                        Real_Pos[3, Real_Count].X = ret_Edge3[0].X;
                        Real_Pos[3, Real_Count].Y = ret_Edge3[0].Y;

                        Real_Score[Real_Count] = 0;

                        Real_Count++;
                    }

                    if (Real_Count > 0)
                    {
                        for (kk = 0; kk < Real_Count; kk++)
                        {
                            procresults.Clear(); //  procresults[0~4] Update
                            for (k = 0; k < EdgePos.Count; k++)
                            {
                                double offsetx = 0;
                                double offsety = 0;

                                //offsetx = (CurCam.GetGrabSizeWidth() / 2) - Math.Abs(Real_Pos[k, kk].X);
                                //offsety = (CurCam.GetGrabSizeHeight() / 2) - Math.Abs(Real_Pos[k, kk].Y);
                                offsetx = Real_Pos[k, kk].X - (CurCam.GetGrabSizeWidth() / 2);
                                offsety = (CurCam.GetGrabSizeHeight() / 2) - Real_Pos[k, kk].Y;

                                if (Real_Pos[k, kk].X != 0 && Real_Pos[k, kk].Y != 0)
                                {
                                    LoggerManager.Debug($"Pixel X : {Real_Pos[k, kk].X} , Pixel Y :{Real_Pos[k, kk].Y}", isInfo: true);
                                }

                                offsetx *= CurCam.GetRatioX();
                                offsety *= CurCam.GetRatioY();

                                if (axisX != null && axisY != null) 
                                {
                                    this.MotionManager().SetSettlingTime(axisX, 0.00001);
                                    this.MotionManager().SetSettlingTime(axisY, 0.00001);
                                }

                                wafercoord = new WaferCoordinate();

                                wafercoord.X.Value = (EdgePos[k].X.Value + offsetx);
                                wafercoord.Y.Value = (EdgePos[k].Y.Value + offsety);

                                RetVal = EventCodeEnum.NONE;

                                procresults.Add(new WaferProcResult(wafercoord, RetVal));
                            }

                            RetVal = Calculation();

                            if (RetVal == EventCodeEnum.NONE)
                            {
                                for (k = 0; k < EdgePos.Count; k++)
                                {
                                    string edgepath = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, false, "WaferEdge", $"{kk}_Edge{k}");

                                    this.VisionManager().AddEdgePosBuffer(EdgeLineBuffer[k], Real_Pos[k, kk].X, Real_Pos[k, kk].Y);
                                    this.VisionManager().SaveImageBuffer(EdgeLineBuffer[k], edgepath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                }

                                if (this.WaferAligner().WaferAlignControItems.IsDebugEdgeProcessing)
                                {
                                    ImageBuffer[] copyBuffers = new ImageBuffer[4];
                                    ImageBuffer[] drawBuffers = new ImageBuffer[4];

                                    for (k = 0; k < EdgePos.Count; k++)
                                    {
                                        copyBuffers[k] = new ImageBuffer();
                                        EdgeBuffer[k].CopyTo(copyBuffers[k]);

                                        // 인덱스 매핑
                                        int drawBufferIndex;
                                        switch (k)
                                        {
                                            case 0:
                                                drawBufferIndex = 1;
                                                break;
                                            case 1:
                                                drawBufferIndex = 0;
                                                break;
                                            case 2:
                                                drawBufferIndex = 2;
                                                break;
                                            case 3:
                                                drawBufferIndex = 3;
                                                break;
                                            default:
                                                drawBufferIndex = k; // 기본 매핑, 예외 처리 필요 시 수정 가능
                                                break;
                                        }

                                        drawBuffers[drawBufferIndex] = this.VisionManager().DrawCrosshair(copyBuffers[k], Real_Pos[k, kk], 20, 12);
                                    }

                                    string resultPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "WaferEdge", $"CombinedResult");

                                    var combindeimg = this.VisionManager().CombineImages(drawBuffers, Width, Heigh, 2, 2);
                                    this.VisionManager().SaveImageBuffer(combindeimg, resultPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                }

                                break;
                            }
                            else
                            {
                                SaveEdgeFailImage(EdgeBuffer, EdgeLineBuffer);
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Failed to find candidate for edge. Real_Count: 0");
                        RetVal = EventCodeEnum.WAFER_EDGE_NOT_FOUND;
                        procresults.Add(new WaferProcResult(wafercoord, RetVal));
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.WAFER_EDGE_NOT_FOUND;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }
        private List<(int, int[], Point[])> EdgeCandidatesResult(int Except_Pixel, ImageBuffer[] EdgeLineBuffer)
        {
            List<(int, int[], Point[])> EdgeCandidates = new List<(int, int[], Point[])>();
            try
            {
                int ret_Edge0_count = 0;
                int ret_Edge1_count = 0;
                int ret_Edge2_count = 0;
                int ret_Edge3_count = 0;

                int rsize = 32;
                int[] ret_PMEdge0 = new int[rsize];
                int[] ret_PMEdge1 = new int[rsize];
                int[] ret_PMEdge2 = new int[rsize];
                int[] ret_PMEdge3 = new int[rsize];

                Point[] ret_Edge0 = new Point[rsize];
                Point[] ret_Edge1 = new Point[rsize];
                Point[] ret_Edge2 = new Point[rsize];
                Point[] ret_Edge3 = new Point[rsize];

                 
                double P_RSum = 0;
                double P_LSum = 0;
                double P_RAvg = 0;
                double P_LAvg = 0;

                int MaxDiffValue = 0;

                Point TempPos = new Point();

                int Acc_Pixel = 20;
                int Cmp_Pixel = 10;

                double Temp_Sum = 0;
                double Temp_Avg = 0;

                int Width = EdgeLineBuffer[0].SizeX;
                int Heigh = EdgeLineBuffer[0].SizeY;

                int RWidth = EdgeLineBuffer[0].SizeX - 1;
                int RHeight = EdgeLineBuffer[0].SizeY - 1;

                double[,] EdgeOVal = new double[EdgePos.Count, Width];

                double TempOval = 0;
                int TempOvalPos = 0;

                double Threshold = 20;

                int i, j, k, kk, m;

                int sx, sy;
                int alphaS, BetaS;
                int alphaE, BetaE;

                int PMFLAG;

                sx = Width;
                sy = Heigh;
                for (k = 0; k < EdgePos.Count; k++)
                {
                    if ((k == 1) || (k == 2))
                    {
                        alphaS = 0;
                        alphaE = RWidth;
                    }
                    else
                    {
                        alphaS = RWidth;
                        alphaE = 0;
                    }

                    if ((k == 0) || (k == 1))
                    {
                        BetaS = 0;
                        BetaE = RHeight;

                        PMFLAG = 1;
                    }
                    else
                    {
                        BetaS = RHeight;
                        BetaE = 0;

                        PMFLAG = -1;
                    }

                    //data_VBcvEdgeFindFEX_Line_Avg = (uchar*)VBcvEdgeFindFEX_Line_Avg[k]->imageData;

                    j = BetaS + (Except_Pixel * PMFLAG);

                    // k = 0, 3
                    if (alphaS > alphaE)
                    {
                        for (i = (alphaS - Except_Pixel); i >= (alphaE + 1); i--)
                        {
                            Temp_Sum = 0;

                            for (kk = 1; kk <= Acc_Pixel; kk++)
                            {
                                Temp_Sum += EdgeLineBuffer[k].Buffer[(j - (kk * PMFLAG)) * Width + (i + kk)];
                            }

                            Temp_Avg = Temp_Sum / Acc_Pixel;

                            if ((i > Except_Pixel) && (i < Width - Except_Pixel))
                            {
                                EdgeOVal[k, RWidth - i] = Temp_Avg - EdgeLineBuffer[k].Buffer[j * Width + i];
                            }

                            j = j + PMFLAG;
                        }
                    }

                    // k = 1, 2
                    else
                    {
                        for (i = (alphaS + Except_Pixel); i <= (alphaE - 1); i++)
                        {
                            Temp_Sum = 0;

                            for (kk = 1; kk <= Acc_Pixel; kk++)
                            {
                                Temp_Sum += EdgeLineBuffer[k].Buffer[(j - (kk * PMFLAG)) * Width + (i - kk)];
                            }

                            Temp_Avg = Temp_Sum / Acc_Pixel;

                            if ((i > Except_Pixel) && (i < Width - Except_Pixel))
                            {
                                EdgeOVal[k, i] = Temp_Avg - EdgeLineBuffer[k].Buffer[j * Width + i];
                            }

                            j = j + PMFLAG;
                        }
                    }
                }


                //[EDGE 후보 구하기]
                for (k = 0; k < EdgePos.Count; k++)
                {
                    for (i = Except_Pixel; i < RWidth - Except_Pixel; i++)
                    {
                        if (Math.Abs(EdgeOVal[k, i]) > Threshold)
                        {
                            TempOval = 0;

                            for (j = -Cmp_Pixel; j < Cmp_Pixel; j++)
                            {
                                if (Math.Abs(EdgeOVal[k, i + j]) > TempOval)
                                {
                                    TempOval = Math.Abs(EdgeOVal[k, i + j]);
                                    TempOvalPos = i + j;
                                }
                            }

                            if (Math.Abs(EdgeOVal[k, TempOvalPos - 1]) > (Threshold * 0.8) &&
                                Math.Abs(EdgeOVal[k, TempOvalPos + 1]) > (Threshold * 0.8))
                            {

                                if (TempOvalPos == i)
                                {
                                    if (k == 0)
                                    {
                                        P_RSum = 0;
                                        P_LSum = 0;

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_RSum += EdgeLineBuffer[k].Buffer[(i - kk) * Width + (RWidth - i + kk)];
                                        }

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_LSum += EdgeLineBuffer[k].Buffer[(i + kk) * Width + (RWidth - i - kk)];
                                        }

                                        P_RAvg = P_RSum / Cmp_Pixel;
                                        P_LAvg = P_LSum / Cmp_Pixel;
                                    }

                                    if (k == 1)
                                    {
                                        P_RSum = 0;
                                        P_LSum = 0;

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_RSum += EdgeLineBuffer[k].Buffer[(i + kk) * Width + (i + kk)];
                                        }

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_LSum += EdgeLineBuffer[k].Buffer[(i - kk) * Width + (i - kk)];
                                        }

                                        P_RAvg = P_RSum / Cmp_Pixel;
                                        P_LAvg = P_LSum / Cmp_Pixel;
                                    }

                                    if (k == 2)
                                    {
                                        P_RSum = 0;
                                        P_LSum = 0;

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_RSum += EdgeLineBuffer[k].Buffer[(RHeight - i - kk) * Width + (i + kk)];
                                        }

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_LSum += EdgeLineBuffer[k].Buffer[(RHeight - i + kk) * Width + (i - kk)];
                                        }

                                        P_RAvg = P_RSum / Cmp_Pixel;
                                        P_LAvg = P_LSum / Cmp_Pixel;
                                    }

                                    if (k == 3)
                                    {
                                        P_RSum = 0;
                                        P_LSum = 0;

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_RSum += EdgeLineBuffer[k].Buffer[(RHeight - i + kk) * Width + (RWidth - i + kk)];
                                        }

                                        for (kk = 1; kk <= Cmp_Pixel; kk++)
                                        {
                                            P_LSum += EdgeLineBuffer[k].Buffer[(RHeight - i - kk) * Width + (RWidth - i - kk)];
                                        }

                                        P_RAvg = P_RSum / Cmp_Pixel;
                                        P_LAvg = P_LSum / Cmp_Pixel;
                                    }

                                    if (Math.Abs(P_RAvg - P_LAvg) > 10)
                                    {
                                        if ((i > (Width / 2 - 1) - (sx / 2 - 1)) && (i < (Width / 2 - 1) - (sx / 2 - 1) + sx))
                                        {
                                            MaxDiffValue = 0;

                                            if (k == 0)
                                            {
                                                for (m = -5; m < 5; m++)
                                                {
                                                    if (Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + ((RWidth) - (i + m + 1))]) > MaxDiffValue)
                                                    {
                                                        MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + ((RWidth) - (i + m + 1))]);

                                                        TempPos.X = (RWidth) - (i + m + 1);
                                                        TempPos.Y = (i + m + 1);
                                                    }
                                                }

                                                if (ret_Edge0_count == rsize)
                                                {
                                                    rsize *= 2;

                                                    //ret_Edge0 = (CvPoint*)realloc(ret_Edge0, sizeof(CvPoint) * rsize);
                                                }

                                                ret_Edge0[ret_Edge0_count] = TempPos;
                                                ret_PMEdge0[ret_Edge0_count] = (int)EdgeOVal[k, (int)(RWidth - TempPos.X)];
                                                ret_Edge0_count++;
                                            }
                                            else if (k == 1)
                                            {
                                                for (m = -5; m <= 5; m++)
                                                {
                                                    if (Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + (i + m + 1)]) > MaxDiffValue)
                                                    {
                                                        MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + (i + m + 1)]);

                                                        TempPos.X = (i + m + 1);
                                                        TempPos.Y = (i + m + 1);
                                                    }
                                                }

                                                if (ret_Edge1_count == rsize)
                                                {
                                                    rsize *= 2;

                                                    //ret_Edge1 = (CvPoint*)realloc(ret_Edge1, sizeof(CvPoint) * rsize);
                                                }

                                                ret_Edge1[ret_Edge1_count] = TempPos;
                                                ret_PMEdge1[ret_Edge1_count] = (int)EdgeOVal[k, (int)TempPos.X];
                                                ret_Edge1_count++;
                                            }
                                            else if (k == 2)
                                            {
                                                for (m = -5; m <= 5; m++)
                                                {
                                                    if (Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + (i + m + 1)]) > MaxDiffValue)
                                                    {
                                                        MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + (i + m + 1)]);

                                                        TempPos.X = (i + m + 1);
                                                        TempPos.Y = (RHeight) - (i + m + 1);
                                                    }

                                                }

                                                if (ret_Edge2_count == rsize)
                                                {
                                                    rsize *= 2;

                                                    //ret_Edge2 = (CvPoint*)realloc(ret_Edge2, sizeof(CvPoint) * rsize);
                                                }

                                                ret_Edge2[ret_Edge2_count] = TempPos;
                                                ret_PMEdge2[ret_Edge2_count] = (int)EdgeOVal[k, (int)TempPos.X];
                                                ret_Edge2_count++;
                                            }
                                            else if (k == 3)
                                            {
                                                for (m = -5; m <= 5; m++)
                                                {
                                                    if (Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + ((RWidth) - (i + m + 1))]) > MaxDiffValue)
                                                    {
                                                        MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + ((RWidth) - (i + m + 1))]);

                                                        TempPos.X = (RWidth) - (i + m + 1);
                                                        TempPos.Y = (RHeight) - (i + m + 1);
                                                    }
                                                }

                                                if (ret_Edge3_count == rsize)
                                                {
                                                    rsize *= 3;

                                                    //ret_Edge3 = (CvPoint*)realloc(ret_Edge3, sizeof(CvPoint) * rsize);
                                                }

                                                ret_Edge3[ret_Edge3_count] = TempPos;
                                                ret_PMEdge3[ret_Edge3_count] = (int)EdgeOVal[k, (int)(RWidth - TempPos.X)];
                                                ret_Edge3_count++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                EdgeCandidates = new List<(int, int[], Point[])>()
                {
                    (ret_Edge0_count, ret_PMEdge0, ret_Edge0),
                    (ret_Edge1_count, ret_PMEdge1, ret_Edge1),
                    (ret_Edge2_count, ret_PMEdge2, ret_Edge2),
                    (ret_Edge3_count, ret_PMEdge3, ret_Edge3),
                };

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EdgeCandidates;
        }

        private void SaveEdgeFailImage(ImageBuffer[] EdgeBuffer, ImageBuffer[] EdgeLineBuffer)
        {
            try
            {
                for (int index = 0; index < EdgeBuffer.Length; index++)
                {
                    string path = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "WaferEdge", "FailImage", $"Edge_[{index}]");
                    this.VisionManager().SaveImageBuffer(EdgeBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                }

                for (int index = 0; index < EdgeLineBuffer.Length; index++)
                {
                    string path = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "WaferEdge", "FailImage", $"EdgeLine_[{index}]");
                    this.VisionManager().SaveImageBuffer(EdgeLineBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CreateHeightProfilingStandard()
        {
            try
            {
                this.WaferAligner().ResetHeightPlanePoint();

                Wafer.GetSubsInfo().ActualThickness = Wafer.GetPhysInfo().Thickness.Value;

                this.WaferAligner().CreateBaseHeightProfiling(new WaferCoordinate(
                    WaferObject.GetSubsInfo().WaferCenter.GetX(),
                    WaferObject.GetSubsInfo().WaferCenter.GetY(),
                    WaferObject.GetSubsInfo().ActualThickness));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private EventCodeEnum Calculation()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
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

                        if ((CLengthRDiff[0] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                            (CLengthRDiff[1] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                            (CLengthRDiff[2] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                            (CLengthRDiff[3] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value))
                        {
                            Wafer.GetSubsInfo().WaferCenter.X.Value = chuckzeroAveXpos;
                            Wafer.GetSubsInfo().WaferCenter.Y.Value = chuckzeroAveYpos;

                            Wafer.GetSubsInfo().WaferCenterOriginatEdge.X.Value = chuckzeroAveXpos;
                            Wafer.GetSubsInfo().WaferCenterOriginatEdge.Y.Value = chuckzeroAveYpos;

                            WaferCoordinate coordinate =
                                this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
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

        private void SWAP(double a, double b)
        {
            try
            {
                double temp = a;
                a = b;
                b = temp;
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


                if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "Markalign can not behave as a failure. Please check Mark .", EnumMessageStyle.Affirmative);
                    return retVal;
                }

                switch (ModifyCondition)
                {
                    case WAEdgeSetupFunction.MANAUALEDGE:
                        {
                            MovingState.Moving();
                            if (await RegisteManualEdgePos())
                                await NextEdge();
                            MovingState.Stop();
                        }
                        break;
                    case WAEdgeSetupFunction.APPLY:
                        {

                            Wafer.GetSubsInfo().WaferCenter.Z.Value = Wafer.GetPhysInfo().Thickness.Value;
                            //ClearData();
                            retVal = Execute();
                            if (retVal != EventCodeEnum.NONE)
                            {
                                var ret = await this.MetroDialogManager().ShowMessageDialog(
                                    Properties.Resources.ErrorMessageTitle, Properties.Resources.EdgeFailMessage, EnumMessageStyle.Affirmative);
                                //ManualEdge
                                InitPnpEdgeManualUI();
                            }
                            else
                            {
                                var ret = await this.MetroDialogManager().ShowMessageDialog(
                                    Properties.Resources.InfoMessageTitle, Properties.Resources.EdgeSuccessMessage, EnumMessageStyle.Affirmative);
                            }
                        }
                        break;
                }

                if (IsParameterChanged())
                    SaveDevParameter();
                SetStepSetupState();

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - Modify() : Error occured.");
                throw err;
            }
            finally
            {

            }
            return retVal;
        }

        private Task<bool> RegisteManualEdgePos()
        {
            bool retVal = false;
            try
            {
                if (CurCam.GetChannelType() != EnumProberCam.WAFER_LOW_CAM)
                    Task.FromResult<bool>(retVal);
                WaferCoordinate coordinate = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                MachineIndex curidx = CurCam.GetCurCoordMachineIndex();
                //if (!(WaferObject.GetSubsInfo().DIEs[curidx.XIndex,curidx.YIndex].DieType.Value == DieTypeEnum.MARK_DIE
                //     | WaferObject.GetSubsInfo().DIEs[curidx.XIndex, curidx.YIndex].DieType.Value == DieTypeEnum.CHANGEMARK_DIE
                //     | WaferObject.GetSubsInfo().DIEs[curidx.XIndex, curidx.YIndex].DieType.Value == DieTypeEnum.MODIFY_DIE))
                //{
                //    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle,
                //        Properties.Resources.RegisterPosNotMarkDieErrorMessage, EnumMessageStyle.Affirmative);
                //    return retVal;
                //}

                int index = -1;
                int mapparamindex = -1;
                MachineIndex idx = new MachineIndex();
                if (coordinate.GetX() > 0 & coordinate.GetY() > 0)
                {   //right upper
                    //idx = EdgeMapParams.Find(dev => dev.Index.XIndex > 0 & dev.Index.YIndex > 0).Index;
                    mapparamindex = 0;
                    idx = EdgeMapParams[mapparamindex].Index;
                    index = ManualEdgePoss.FindIndex(pos => pos.Coordinate.GetX() > 0 & pos.Coordinate.GetY() > 0);
                }
                else if (coordinate.GetX() < 0 & coordinate.GetY() > 0)
                {   //left upper
                    //idx = EdgeMapParams.Find(dev => dev.Index.XIndex < 0 & dev.Index.YIndex > 0).Index;
                    mapparamindex = 1;
                    idx = EdgeMapParams[mapparamindex].Index;
                    index = ManualEdgePoss.FindIndex(pos => pos.Coordinate.GetX() < 0 & pos.Coordinate.GetY() > 0);
                }
                else if (coordinate.GetX() < 0 & coordinate.GetY() < 0)
                {   //left lower
                    //idx = EdgeMapParams.Find(dev => dev.Index.XIndex < 0 & dev.Index.YIndex < 0).Index;
                    mapparamindex = 2;
                    idx = EdgeMapParams[mapparamindex].Index;
                    index = ManualEdgePoss.FindIndex(pos => pos.Coordinate.GetX() < 0 & pos.Coordinate.GetY() < 0);
                }
                else if (coordinate.GetX() > 0 & coordinate.GetY() < 0)
                {   //right lower
                    //idx = EdgeMapParams.Find(dev => dev.Index.XIndex > 0 & dev.Index.YIndex < 0).Index;
                    mapparamindex = 3;
                    idx = EdgeMapParams[mapparamindex].Index;
                    index = ManualEdgePoss.FindIndex(pos => pos.Coordinate.GetX() > 0 & pos.Coordinate.GetY() < 0);
                }

                if (idx != null)
                {
                    if (index != -1)
                    {
                        ManualEdgePoss[index].Coordinate = coordinate;
                        ManualEdgePoss[index].DieType = WaferObject.GetSubsInfo().DIEs[curidx.XIndex, curidx.YIndex].DieType.Value;
                        ManualEdgePoss[index].DieType = DieTypeEnum.UNKNOWN;
                    }
                    //ManualEdgePoss[index] = coordinate;
                    else
                    {
                        ManualEdgePoss.Add(new EdgeMapParam(coordinate, curidx, WaferObject.GetSubsInfo().DIEs[curidx.XIndex, curidx.YIndex].DieType.Value));
                    }
                    //ManualEdgePoss.Add(coordinate);

                    //WaferObject.GetSubsInfo().Devices.ToList<DeviceObject>().Find(item => item.DieIndexM.XIndex == idx.XIndex
                    //     && item.DieIndexM.YIndex == idx.YIndex)
                    //     .DieType.Value = DieTypeEnum.MODIFY_DIE;

                    // 아래가 결과맵에 영향을 줌.
                    //if (curidx.XIndex == idx.XIndex & curidx.YIndex == idx.YIndex)
                    //{//기존의 EdgePos 와 같은 Index면
                    //    WaferObject.GetSubsInfo().DIEs[idx.XIndex, idx.YIndex].DieType.Value = DieTypeEnum.MODIFY_DIE;
                    //}
                    //else
                    //{
                    //    WaferObject.GetSubsInfo().DIEs[idx.XIndex, idx.YIndex].DieType.Value = EdgeMapParams[mapparamindex].DieType;
                    //    WaferObject.GetSubsInfo().DIEs[curidx.XIndex, curidx.YIndex].DieType.Value = DieTypeEnum.MODIFY_DIE;
                    //}



                }

                if (index == -1)
                {
                    if (ManualEdgePoss.Count == 4)
                    {
                        PadJogSelect.IsEnabled = true;
                    }
                }
                else if (index == 3)
                {
                    PadJogSelect.IsEnabled = true;
                }
                //DoExecute();

                retVal = true;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Register_Manual_Edge_OK);

            }
            catch (Exception err)
            {
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Register_Manual_Edge_Failure);
                LoggerManager.Exception(err);
            }
            return Task.FromResult<bool>(retVal);
        }

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(WA_EdgeParam_Standard));
                    if (target != null)
                    {
                        WA_EdgeParam_Standard paramobj = (WA_EdgeParam_Standard)target;

                        paramobj.EdgeMovement.CopyTo(EdgeStandardParam_Clone.EdgeMovement);
                        Edge_IParam = EdgeStandardParam_Clone;
                        break;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadDevParameter()
        {

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new WA_EdgeParam_Standard();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(WA_EdgeParam_Standard));

                if (retval == EventCodeEnum.NONE)
                {
                    Edge_IParam = tmpParam;
                    EdgeStandardParam_Clone = Edge_IParam as WA_EdgeParam_Standard;
                    //Params.Add(this.GetParam_Wafer());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retval;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }


        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Edge_IParam != null)
                {
                    //if(PnpManager.SeletedStep?.Header == this.Header & CurCam != null)
                    //{
                    //    EdgeStandardParam_Clone.LightParams = new ObservableCollection<LightValueParam>();
                    //    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                    //    {
                    //        EdgeStandardParam_Clone.LightParams.Add(
                    //            new LightValueParam(CurCam.LightsChannels[index].Type.Value,
                    //            (ushort)CurCam.GetLight(CurCam.LightsChannels[index].Type.Value)));
                    //    }

                    //    Edge_IParam = EdgeStandardParam_Clone;
                    //}
                    Edge_IParam = EdgeStandardParam_Clone;
                    RetVal = this.SaveParameter(Edge_IParam);
                    IsParamChanged = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - SaveDevParameter() : Error occured.");
            }

            return RetVal;
        }
        public bool IsLotReady(out string msg)
        {
            bool retVal = false;
            msg = null;
            try
            {
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SubModuleState.ClearData();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - PreRun() : Error occured.");
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.WaferAligner().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    IsManualMode = false;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public void DoClearRecoveryData()
        {
            try
            {
                this.ManualEdgePoss.Clear();

                LoggerManager.Debug($"[{this.GetType().Name}] DoClearRecoveryData() : Clear Data Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Element 안붙어있는 Parameter 는 확인해야한다.
                //Ex) EdgeMovement, BlobParam ....

                retVal = Extensions_IParam.ElementStateNeedSetupValidation(Edge_IParam);

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SubModuleState.Recovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    List<Guid> pnpsteps = new List<Guid>();
                    pnpsteps.Add(this.ScreenGUID);
                    Guid viewguid = new Guid("1b96aa21-1613-108a-71d6-9bce684a4dd0");
                    this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                    this.ViewModelManager().ViewTransitionAsync(viewguid);
                }
                //this.PnPManager().GetPnpSteps(this.WaferAligner());
                //this.ViewModelManager().ViewTransitionType(this.PnPManager());
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;
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
                if (PnpManager.SelectedPnpStep != null)
                {
                    if (!(PnpManager.SelectedPnpStep as ICategoryNodeItem).Header.Equals(Header))
                        return retVal;
                }

                //if (EdgeStandardParam_Clone.LightParams != null & EdgeStandardParam_Clone.LightParams.Count != 0)
                //{
                //    foreach (var light in EdgeStandardParam_Clone.LightParams)
                //    {
                //        if (IsParamChanged)
                //            break;
                //        foreach (var camlight in CurCam.LightsChannels)
                //        {
                //            if (light.Type.Value == camlight.Type.Value)
                //            {
                //                if (light.Value.Value != CurCam.GetLight(camlight.Type.Value))
                //                    IsParamChanged = true;
                //                else
                //                    IsParamChanged = false;
                //                break;
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    IsParamChanged = true;
                //}

                IsParamChanged = true;
                EventCodeEnum ret = Extensions_IParam.ElementStateDefaultValidation(Edge_IParam);
                if (ret == EventCodeEnum.NONE)
                    retVal = false;
                else
                    retVal = true;

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
                if ((Edge_IParam as WA_EdgeParam_Standard).EdgeMovement.Value == EnumWASubModuleEnable.DISABLE)
                {
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                    return;
                }

                EventCodeEnum retVal = Extensions_IParam.ElementStateNeedSetupValidation(Edge_IParam as WA_EdgeParam_Standard);
                if (GetState() != SubModuleStateEnum.DONE)
                    retVal = EventCodeEnum.UNDEFINED;
                if (retVal == EventCodeEnum.NONE)
                {
                    if (GetState() == SubModuleStateEnum.DONE)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
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

                PackagableParams.Add(SerializeManager.SerializeToByte(EdgeStandardParam_Clone));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //internal static class ResourceAccessor
        //{
        //    public static ImageSource Get(System.Drawing.Bitmap bitmap)
        //    {
        //        MemoryStream ms = new MemoryStream();
        //        (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //        BitmapImage image = new BitmapImage();
        //        image.BeginInit();
        //        ms.Seek(0, SeekOrigin.Begin);
        //        image.StreamSource = ms;
        //        image.EndInit();

        //        return image;
        //    }
        //}
    }

    [Serializable]
    public class EdgeMapParam
    {
        private WaferCoordinate _Coordinate;

        public WaferCoordinate Coordinate
        {
            get { return _Coordinate; }
            set { _Coordinate = value; }
        }


        private MachineIndex _Index;

        public MachineIndex Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        private DieTypeEnum _DieType;

        public DieTypeEnum DieType
        {
            get { return _DieType; }
            set { _DieType = value; }
        }


        public EdgeMapParam()
        {

        }
        public EdgeMapParam(MachineIndex index, DieTypeEnum dieType)
        {
            try
            {
                Index = index;
                DieType = dieType;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EdgeMapParam(WaferCoordinate coordinate, MachineIndex index, DieTypeEnum dieType)
        {
            try
            {
                Coordinate = coordinate;
                Index = index;
                DieType = dieType;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
