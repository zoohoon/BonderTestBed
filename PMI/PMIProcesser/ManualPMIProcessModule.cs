using LogModule;
using PMIModuleParameter;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PMIProcesser
{
    public class ManualPMIProcessModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup
    {
        public override Guid ScreenGUID { get; } = new Guid("08C25DD4-C7BB-35CE-DBE5-44571E2C2068");
        #region ==> PropertyChanged
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //}
        #endregion

        public override bool Initialized { get; set; } = false;

        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

         public PadGroup PadInfos
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().Pads; }

        }

        #region Manual All PMI Properties
        private bool IsAllPMIMode
        {
            get; set;
        } = false;
        #endregion

        public ManualPMIProcessModule()
        {

        }

        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }
        }


        private ManualPMISetupControlService ManualPMISetupControl;

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

        private LOGGING_MODE _LoggingMode = LOGGING_MODE.All;
        public LOGGING_MODE LoggingMode
        {
            get { return _LoggingMode; }
            set
            {
                if (value != _LoggingMode)
                {
                    _LoggingMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _SelectedPMIPadIndex;
        //public int SelectedPMIPadIndex
        //{
        //    get { return _SelectedPMIPadIndex; }
        //    set
        //    {
        //        if (value != _SelectedPMIPadIndex)
        //        {
        //            _SelectedPMIPadIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _isExistPMIResult = false;
        public bool isExistPMIResult
        {
            get { return _isExistPMIResult; }
            set
            {
                if (value != _isExistPMIResult)
                {
                    _isExistPMIResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedMarkIndex = -1;
        public int SelectedMarkIndex
        {
            get { return _SelectedMarkIndex; }
            set
            {
                if (value != _SelectedMarkIndex)
                {
                    _SelectedMarkIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _OnDelegated;
        public bool OnDelegated
        {
            get { return _OnDelegated; }
            set
            {
                if (value != _OnDelegated)
                {
                    _OnDelegated = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedPadTableTemplateIndex;
        public int SelectedPadTableTemplateIndex
        {
            get { return _SelectedPadTableTemplateIndex; }
            set
            {
                if (value != _SelectedPadTableTemplateIndex)
                {
                    _SelectedPadTableTemplateIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private PMIManualMode _CurSelectMode;
        public PMIManualMode CurSelectMode
        {
            get { return _CurSelectMode; }
            set
            {
                if (value != _CurSelectMode)
                {
                    _CurSelectMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand<SETUP_DIRECTION> _ChangePadIndexCommand;
        private AsyncCommand<LOGGING_MODE> _ChangeLoggingModeCommand;
        private AsyncCommand<SETUP_DIRECTION> _ChangeMarkIndexCommand;
        private AsyncCommand _ChangePMIManualModeCommand;
        private AsyncCommand _ChangeSideViewerVisibleCommand;
        private AsyncCommand _DoAllPMI;

        private bool RememberMapViewStageSyncEnable { get; set; }
        private bool RememberMapViewCurIndexVisiablity { get; set; }

        /// <summary>
        /// 현재 단계의 Parameter Setting 이 다 되었는지 확인하는 함수.
        /// </summary>
        /// <returns></returns>
        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Parameter 확인한다.

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Modify 상태가 있는지 없는지를 확인해준다.
                retVal = Extensions_IParam.ElementStateDefaultValidation(this.PMIModule().PMIModuleDevParam_IParam);

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
                LoggerManager.Exception(err);
            }
            return retVal;
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

        private void UpdateOverlayDelegate(DELEGATEONOFF flag)
        {
            try
            {
                if ((flag == DELEGATEONOFF.ON) && (OnDelegated == false))
                {
                    this.CoordinateManager().OverlayUpdateDelegate += UpdateOverlay;

                    this.PMIModule().UpdateRenderLayer();

                    OnDelegated = true;
                }
                else if (OnDelegated == true)
                {
                    this.CoordinateManager().OverlayUpdateDelegate -= UpdateOverlay;

                    this.PMIModule().ClearRenderObjects();

                    OnDelegated = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private void UpdateOverlay()
        {
            try
            {
                this.PMIModule().UpdateDisplayedDevices(this.CurCam, false);

                this.PMIModule().UpdateCurrentPadIndex();
                this.PMIModule().UpdateRenderLayer();
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
                LoggerManager.Debug($"[ManualPMIProcessModule] [InitModule()] : {err}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public new void DeInitModule()
        {
        }

        public void ChangeButtonSideViewerVisible()
        {
            if (isExistPMIResult == true)
            {
                TwoButton.IsEnabled = true;
            }
            else
            {
                TwoButton.IsEnabled = false;
            }
        }

        public void ChangeButtonForMarkResult()
        {
            try
            {
                bool isActivated = false;

                if (isExistPMIResult == true)
                {
                    if (CurrnetPad != null)
                    {
                        // Result가 존재하면
                        if (CurrnetPad.PMIResults.Count > 0)
                        {
                            PMIPadResult result = CurrnetPad.PMIResults.Last();

                            if (result.MarkResults.Count > 0)
                            {
                                isActivated = true;
                            }
                        }
                    }

                    //if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                    //{
                    //    MachineIndex mi = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Last();
                    //    DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;

                    //    CurrnetPad = curdie.Pads.PMIPadInfos[CurrentPadIndex];

                    //    if (CurrnetPad != null)
                    //    {
                    //        // Result가 존재하면
                    //        if (CurrnetPad.PMIResults.Count > 0)
                    //        {
                    //            PMIPadResult result = CurrnetPad.PMIResults[CurrnetPad.SelectedPMIPadResultIndex];

                    //            if(result.MarkResults.Count > 0)
                    //            {
                    //                isActivated = true;
                    //            }
                    //        }
                    //    }
                    //}
                }

                if (isActivated == true)
                {
                    PadJogLeft.IsEnabled = true;
                    PadJogRight.IsEnabled = true;
                }
                else
                {
                    PadJogLeft.IsEnabled = false;
                    PadJogRight.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ChangeButtonForPMIResult()
        {
            try
            {
                if (isExistPMIResult == true)
                {
                    PadJogLeftUp.IsEnabled = true;
                    PadJogRightUp.IsEnabled = true;

                    // Mode에 따라
                    if (LoggingMode == LOGGING_MODE.All)
                    {
                        PadJogLeftDown.IsEnabled = false;
                        PadJogRightDown.IsEnabled = true;
                    }
                    else
                    {
                        PadJogLeftDown.IsEnabled = true;
                        PadJogRightDown.IsEnabled = false;
                    }

                }
                else

                {
                    PadJogLeftUp.IsEnabled = false;
                    PadJogRightUp.IsEnabled = false;
                    PadJogLeftDown.IsEnabled = false;
                    PadJogRightDown.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private Task ChangeMarkIndexCommand(SETUP_DIRECTION direction)
        {
            try
            {
                if (CurrnetPad != null)
                {
                    MachineIndex currentMI = CurCam.GetCurCoordMachineIndex();

                    if (CurrnetPad.MachineIndex.XIndex != currentMI.XIndex ||
                        CurrnetPad.MachineIndex.YIndex != currentMI.YIndex)
                    {
                        DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[currentMI.XIndex, currentMI.YIndex] as DeviceObject;
                        CurrnetPad = curdie.Pads.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];
                    }

                    if (CurrnetPad.PMIResults.Count > 0)
                    {
                        PMIPadResult result = CurrnetPad.PMIResults.Last();
                        bool IsFindIndex = false;
                        int StartIndex = SelectedMarkIndex;

                        int allmarkcount = result.MarkResults.Count;

                        if (allmarkcount > 0)
                        {
                            bool hasOnlyFailMarks = result.MarkResults.All(m => m.Status.ToList().All(x => x != MarkStatusCodeEnum.PASS));
                            bool hasOnlyPassMarks = result.MarkResults.All(m => m.Status.ToList().Any(x => x == MarkStatusCodeEnum.PASS));

                            // 만약 FAIL 마크만 있는 경우
                            if (hasOnlyFailMarks && LoggingMode == LOGGING_MODE.OnlyFail)
                            {
                                // 모든 Mark를 순회할 필요 없다.
                                if (direction == SETUP_DIRECTION.PREV)
                                {
                                    SelectedMarkIndex = (SelectedMarkIndex == 0) ? allmarkcount - 1 : SelectedMarkIndex - 1;
                                }
                                else
                                {
                                    SelectedMarkIndex = (SelectedMarkIndex == allmarkcount - 1) ? 0 : SelectedMarkIndex + 1;
                                }

                                IsFindIndex = true; // FAIL 마크만 있는 경우 무조건 인덱스를 이동하여 찾음
                            }
                            else if (hasOnlyPassMarks && LoggingMode == LOGGING_MODE.OnlyFail)
                            {
                                // 만약 PASS 마크만 있는 경우, 검색을 하지 않음
                                IsFindIndex = false;
                            }
                            else
                            {
                                do
                                {
                                    if (direction == SETUP_DIRECTION.PREV)
                                    {
                                        SelectedMarkIndex = (SelectedMarkIndex == 0) ? allmarkcount - 1 : SelectedMarkIndex - 1;
                                    }
                                    else
                                    {
                                        SelectedMarkIndex = (SelectedMarkIndex == allmarkcount - 1) ? 0 : SelectedMarkIndex + 1;
                                    }

                                    if (LoggingMode != LOGGING_MODE.All)
                                    {
                                        if (LoggingMode == LOGGING_MODE.OnlyFail)
                                        {
                                            bool isPass = result.MarkResults[SelectedMarkIndex].Status.ToList().Any(x => x == MarkStatusCodeEnum.PASS);

                                            if (!isPass)
                                            {
                                                IsFindIndex = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        IsFindIndex = true;
                                    }

                                } while (!IsFindIndex && StartIndex != SelectedMarkIndex);
                            }
                        }

                        MachineIndex mi = CurCam.GetCurCoordMachineIndex();

                        EventCodeEnum retval = this.PMIModule().MoveToMark(CurCam, mi, DrawingGroup.SelectedPMIPadIndex, SelectedMarkIndex);

                        MakeSideViewData();
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

        /// <summary>
        /// PMI 검사 완료한 Die들을 순회한다. 
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private Task ChangePadIndexCommand(SETUP_DIRECTION direction)
        {
            try
            {
                var curmi = CurCam.GetCurCoordMachineIndex();
                DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[curmi.XIndex, curmi.YIndex] as DeviceObject;
               
                var allPadCount = curdie.Pads.PMIPadInfos.Count;
                var isProcessedPMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Contains(curmi);

                if (allPadCount > 0 && isProcessedPMI)
                {
                    int? enablecount = PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex].PadEnable.FindAll(x => x.Value == true)?.Count;

                    var movetoPrevDie = false;
                    var movetoNextDie = false;
                    var startIndex = DrawingGroup.SelectedPMIPadIndex;
                    var foundIndex = false;

                    if (enablecount != null && enablecount > 0)
                    {
                        while (foundIndex == false)
                        {
                            if (direction == SETUP_DIRECTION.PREV)
                            {
                                if (DrawingGroup.SelectedPMIPadIndex == 0)
                                {
                                    movetoPrevDie = true;
                                    DrawingGroup.SelectedPMIPadIndex = allPadCount - 1;
                                }
                                else
                                {
                                    if (DrawingGroup.SelectedPMIPadIndex < 0)
                                    {
                                        DrawingGroup.SelectedPMIPadIndex = 0;
                                        movetoPrevDie = true;
                                    }
                                    else
                                    {
                                        DrawingGroup.SelectedPMIPadIndex--;
                                    }
                                }
                            }
                            else
                            {
                                // 현재 Index가 마지막인 경우
                                if (DrawingGroup.SelectedPMIPadIndex == allPadCount - 1)
                                {
                                    DrawingGroup.SelectedPMIPadIndex = 0;
                                    movetoNextDie = true;
                                }
                                else
                                {
                                    if (DrawingGroup.SelectedPMIPadIndex > allPadCount - 1)
                                    {
                                        DrawingGroup.SelectedPMIPadIndex = 0;
                                        movetoNextDie = true;
                                    }
                                    else
                                    {
                                        DrawingGroup.SelectedPMIPadIndex++;
                                    }
                                }
                            }

                            if (PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex].PadEnable[DrawingGroup.SelectedPMIPadIndex].Value == true)
                            {
                                if (LoggingMode != LOGGING_MODE.All)
                                {
                                    PMIPadObject tmpPad = curdie.Pads.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];

                                    if (LoggingMode == LOGGING_MODE.OnlyFail)
                                    {
                                        if (tmpPad.PMIResults.Count > 0)
                                        {
                                            bool isPass = tmpPad.PMIResults.Last().PadStatus.ToList().Any(x => x == PadStatusCodeEnum.PASS);
                                            
                                            if (isPass == false)
                                            {
                                                foundIndex = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foundIndex = true;
                                }
                            }

                            if (startIndex == DrawingGroup.SelectedPMIPadIndex)
                            {
                                break;
                            }
                        }

                        if((movetoNextDie || movetoPrevDie) && IsAllPMIMode)
                        {
                            curmi = GetNextProcessedPmiMI(direction, curmi);
                        }

                        MoveToPadIndex(curmi);
                    }
                }
                else if (allPadCount > 0 && !isProcessedPMI)
                {
                    curmi = GetNextProcessedPmiMI(direction, curmi);
                    if (direction == SETUP_DIRECTION.PREV)
                    {
                        DrawingGroup.SelectedPMIPadIndex = allPadCount - 1;
                    }
                    else
                    {
                        DrawingGroup.SelectedPMIPadIndex = 0;
                    }

                    MoveToPadIndex(curmi);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }

            void MoveToPadIndex(MachineIndex mi)
            {
                EventCodeEnum retval = this.PMIModule().MoveToPad(CurCam, mi, DrawingGroup.SelectedPMIPadIndex);

                if (retval == EventCodeEnum.NONE)
                {
                    if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                    {
                        DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;
                        CurrnetPad = curdie.Pads.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];
                    }

                    SelectedMarkIndex = -1;

                    ChangeButtonForPMIResult();
                    ChangeButtonForMarkResult();
                    ChangeButtonSideViewerVisible();

                    MakeSideViewData();

                    ChangeButtonForMarkResult();
                }

                this.PMIModule().UpdateRenderLayer();

                UpdateLabel();
            }

            MachineIndex GetNextProcessedPmiMI(SETUP_DIRECTION dir, MachineIndex cur_mi)
            {
                var processedMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex;

                var nextMI = new MachineIndex(0, 0);

                var dirdiff = (dir == SETUP_DIRECTION.PREV) ? -1 : 1;

                var ei = processedMI.IndexOf(cur_mi);
                if (ei > -1)
                {
                    nextMI = processedMI[validIndex(ei + dirdiff)];
                }
                else
                {
                    nextMI = GetNearestMI(dir, cur_mi);
                }

                #region to delete
                //if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.WAFERHIGHVIEW)
                //{
                //    ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                //    //this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(nextMI.XIndex, nextMI.YIndex);
                //    //이동한 다이의 가장 마지막 패드로 이동
                //    UpdateLabel();
                //}
                //else if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.WAFERLOWVIEW)
                //{
                //    ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                //    //this.StageSupervisor().StageModuleState.WaferLowViewIndexMove(nextMI.XIndex, nextMI.YIndex);
                //    UpdateLabel();
                //}
                //else
                //{
                //    //dieSizeX = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                //    //dieSizeY = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                //    //_StageSupervisor.StageModuleState.StageRelMove(xInc * dieSizeX, yInc * dieSizeY);
                //}
                #endregion

                //UpdateLabel();
                return nextMI;
            }

            MachineIndex GetNearestMI(SETUP_DIRECTION dir, MachineIndex cur_mi)
            {
                var processedMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex;

                var cur_x = cur_mi.XIndex;
                var cur_y = cur_mi.YIndex;

                var dirOff = (dir == SETUP_DIRECTION.PREV) ? -1 : 0;

                var searchIdx = processedMI.IndexOf(new MachineIndex(cur_x, cur_y));
                var idx = 0;
                do
                {
                    var mi = processedMI[idx];

                    // case 1.
                    if (cur_x < mi.XIndex)
                    {
                        var ret = processedMI[validIndex(idx + dirOff)];
                        return processedMI[validIndex(idx + dirOff)];
                    }
                    // case 2. 
                    if (cur_x == mi.XIndex)
                    {
                        do
                        {
                            if (cur_y < mi.YIndex)
                            {
                                var ret = processedMI[validIndex(idx + dirOff)];
                                return processedMI[validIndex(idx + dirOff)];
                            }
                            if (cur_y == mi.YIndex)
                            {
                                if (dir == SETUP_DIRECTION.PREV)
                                {
                                    mi = processedMI[validIndex(idx - 1)];
                                    return processedMI[validIndex(idx - 1)];
                                }
                                else
                                {
                                    mi = processedMI[validIndex(idx + 1)];
                                    return processedMI[validIndex(idx + 1)];
                                }
                            }
                            mi = processedMI[validIndex(++idx)];
                        } while (cur_x == mi.XIndex);
                    }
                    // case 3.
                    if (cur_x > mi.XIndex)
                    {
                        idx++;
                    }

                } while (idx < processedMI.Count);

                if (dir == SETUP_DIRECTION.PREV)
                {
                    var ret = processedMI[processedMI.Count - 1];
                    return processedMI[processedMI.Count - 1];
                }
                else
                {
                    var ret = processedMI[0];
                    return processedMI[0];
                }
            }

            int validIndex(int idx_)
            {
                var processedMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex;

                if (idx_ < 0) { return processedMI.Count - 1; }
                if (idx_ > processedMI.Count - 1) { return 0; }
                return idx_;
            }

            return Task.CompletedTask;
        }

        private Task ChangeLoggingModeCommand(LOGGING_MODE mode)
        {
            try
            {
                LoggingMode = mode;

                ChangeButtonForPMIResult();

                UpdateLabel();
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

        private Task ChangeSideViewerVisibleCommand()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (SideViewTargetVisibility == Visibility.Visible)
                    {
                        SideViewTargetVisibility = Visibility.Hidden;
                    }
                    else
                    {
                        SideViewTargetVisibility = Visibility.Visible;
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private Task ChangePMIManualModeCommand()
        {
            try
            {
                if (CurSelectMode == PMIManualMode.DIE)
                {
                    CurSelectMode = PMIManualMode.DUT;
                }
                else
                {
                    CurSelectMode = PMIManualMode.DIE;
                }

                UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [ChangeMapSelectModeCommand()]: {err}");
            }

            return Task.CompletedTask;
        }

        public EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "Manual PMI";

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/grid.png");
                OneButton.IconCaption = "DIE/DUT";
                OneButton.Command = _ChangePMIManualModeCommand;

                // TODO : 
                OneButton.IsEnabled = false;

                //OneButton.Caption = null;
                //OneButton.Command = null;
                //OneButton.IsEnabled = false;

                //TwoButton.Caption = null;
                //TwoButton.Command = null;
                //TwoButton.IsEnabled = false;

                TwoButton.Caption = "Result\nView";
                TwoButton.Command = _ChangeSideViewerVisibleCommand;

                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/feature-search-outline_W.png");
                ThreeButton.IconCaption = "All PMI";
                ThreeButton.Command = _DoAllPMI;
                ThreeButton.IsEnabled = true;

                //ThreeButton.Caption = null;
                //ThreeButton.Command = null;
                //ThreeButton.IsEnabled = false;

                FourButton.Caption = null;
                FourButton.Command = null;
                FourButton.IsEnabled = false;
                FiveButton.Caption = null;
                FiveButton.Command = null;
                FiveButton.IsEnabled = false;

                PadJogLeftUp.Caption = "PREV";
                PadJogLeftUp.Command = _ChangePadIndexCommand;
                PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogLeftUp.IsEnabled = false;

                PadJogRightUp.Caption = "NEXT";
                PadJogRightUp.Command = _ChangePadIndexCommand;
                PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;
                PadJogRightUp.IsEnabled = false;

                PadJogLeftDown.Caption = "ALL";
                PadJogLeftDown.Command = _ChangeLoggingModeCommand;
                PadJogLeftDown.CommandParameter = LOGGING_MODE.All;
                PadJogLeftDown.IsEnabled = false;

                PadJogRightDown.Caption = "FAIL";
                PadJogRightDown.Command = _ChangeLoggingModeCommand;
                PadJogRightDown.CommandParameter = LOGGING_MODE.OnlyFail;
                PadJogRightDown.IsEnabled = false;

                PadJogSelect.IconCaption = "PMI";
                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/feature-search-outline_W.png");
                PadJogSelect.Command = new AsyncCommand(DoPMI);

                //PadJogLeft.IconSource = null;
                //PadJogRight.IconSource = null;

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;

                PadJogLeft.Command = _ChangeMarkIndexCommand;
                PadJogLeft.CommandParameter = SETUP_DIRECTION.PREV;

                PadJogRight.Command = _ChangeMarkIndexCommand;
                PadJogRight.CommandParameter = SETUP_DIRECTION.NEXT;

                PadJogUp.IconSource = null;
                PadJogDown.IconSource = null;
                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;

                ChangeButtonForPMIResult();
                ChangeButtonForMarkResult();
                ChangeButtonSideViewerVisible();

                SetNodeSetupState(EnumMoudleSetupState.NONE);

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
                Header = "Manual PMI";

                retVal = InitPnpModuleStage();
                retVal = InitLightJog(this);

                _ChangePadIndexCommand = new AsyncCommand<SETUP_DIRECTION>(ChangePadIndexCommand);
                _ChangeLoggingModeCommand = new AsyncCommand<LOGGING_MODE>(ChangeLoggingModeCommand);
                _ChangeMarkIndexCommand = new AsyncCommand<SETUP_DIRECTION>(ChangeMarkIndexCommand);
                _ChangePMIManualModeCommand = new AsyncCommand(ChangePMIManualModeCommand);

                _ChangeSideViewerVisibleCommand = new AsyncCommand(ChangeSideViewerVisibleCommand);
                _DoAllPMI = new AsyncCommand(DoAllPMI);

                //Wafer = this.StageSupervisor().WaferObject as WaferObject;

                ManualPMISetupControl = new ManualPMISetupControlService(this);

                //PMIDevParam = ((PMIModule as IHasDevParameterizable).DevParam) as PMIModuleDevParam;

                DrawingGroup = new PMIDrawingGroup();

                SharpDXLayer = this.PMIModule().InitPMIRenderLayer(this.PMIModule().GetLayerSize(), 0, 0, 0, 0);
                SharpDXLayer?.Init();

                Initialized = true;

                OnDelegated = false;

                SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;

                LoggerManager.Debug($"[ManualPMIProcessModule] [InitViewModel()] : {err}");
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (PMIInfo.PadTableTemplateInfo.Count > 0)
                {
                    SelectedPadTableTemplateIndex = 0;

                    DrawingGroup.PadTableTemplate = PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex];
                }

                DrawingGroup.SetupMode = PMI_SETUP_MODE.NONE;

                DrawingGroup.DetectedMark = true;
                DrawingGroup.RegisterdPad = true;

                DrawingGroup.SelectedPMIPadIndex = 0;
                CurSelectMode = PMIManualMode.DIE;

                //SelectedPMIPadIndex = 0;

                isExistPMIResult = false;
                CurrnetPad = null;
                //CurrentPadIndex = -1;
                SelectedMarkIndex = -1;

                UpdateLabel();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                {
                    this.PMIModule().InitPMIResult();
                    this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Clear();
                    this.PMIModule().DoPMIInfo.ReservedPMIMIndex.Clear();
                }
                else
                {
                    // 결과가 존재하면

                    if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                    {
                        isExistPMIResult = true;
                    }
                }

                retVal = await InitSetup();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;

                LoggerManager.Debug($"[ManualPMIProcessModule] [PageSwitched()] : {err}");
            }
            finally
            {
                RememberMapViewStageSyncEnable = Wafer.MapViewStageSyncEnable;
                RememberMapViewCurIndexVisiablity = Wafer.MapViewCurIndexVisiablity;

                Wafer.MapViewStageSyncEnable = true;
                Wafer.MapViewCurIndexVisiablity = true;
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
                //this.StageSupervisor().StageModuleState

                //retVal = this.MarkAligner().DoMarkAlign();

                this.PMIModule().SetSubModule(this);

                PMIPadObject movedpadinfo = null;

                this.PMIModule().EnterMovePadPosition(ref movedpadinfo);

                if (movedpadinfo != null)
                {
                    CurrnetPad = movedpadinfo;
                }

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);

                MainViewTarget = DisplayPort;

                //MiniViewTarget = this.GetParam_Wafer();
                MiniViewTarget = Wafer;

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                UpdateOverlayDelegate(DELEGATEONOFF.ON);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    SideViewDisplayMode = SideViewMode.TEXTBLOCK_ONLY;

                    if (isExistPMIResult == true)
                    {
                        SideViewTargetVisibility = Visibility.Visible;
                    }
                    else
                    {
                        SideViewTargetVisibility = Visibility.Hidden;
                    }

                    SideViewSwitchVisibility = Visibility.Hidden;
                    SideViewExpanderVisibility = false;
                    SideViewTextVisibility = true;

                    SideViewVerticalAlignment = VerticalAlignment.Center;
                    SideViewHorizontalAlignment = HorizontalAlignment.Left;

                    SideViewWidth = 250;
                    SideViewHeight = 600;
                    SideViewMargin = new Thickness(5, 0, 0, 0);

                    double fs = 12;
                    Brush fc = Brushes.White;
                    Brush bc = Brushes.Transparent;

                    SideViewTitle = "";
                    SideViewTitleFontSize = 14;
                    SideViewTitleFontColor = fc;
                    SideViewTitleBackground = bc;

                    SideViewTextBlocks.Clear();

                    MakeSideTextblock("[Pad Status]", fs, fc, bc);
                    MakeSideTextblock("PASS", fs, fc, bc);
                    MakeSideTextblock("FAIL", fs, fc, bc);
                    MakeSideTextblock("NO PROBE MARK", fs, fc, bc);
                    MakeSideTextblock("TOO MANY PROBE MARK", fs, fc, bc);
                    MakeSideTextblock("[Mark Status]", fs, fc, bc);
                    MakeSideTextblock("[SIZE X, SIZE Y, AREA] : X", fs, fc, bc);
                    MakeSideTextblock("[Proximity, T, B, L, R] : X", fs, fc, bc);
                    MakeSideTextblock("PASS", fs, fc, bc);
                    MakeSideTextblock("TOO CLOSE TO EDGE", fs, fc, bc);
                    MakeSideTextblock("MARK AREA TOO SMALL", fs, fc, bc);
                    MakeSideTextblock("MARK AREA TOO BIG", fs, fc, bc);
                    MakeSideTextblock("MARK SIZE TOO SMALL", fs, fc, bc);
                    MakeSideTextblock("MARK SIZE TOO BIG", fs, fc, bc);
                    MakeSideTextblock("Please register a reference pad image.", fs, fc, bc);//14
                });

                //this.PMIModule().UpdateRenderLayer();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ManualPMIProcessModule] [InitSetup()] : {err}");
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        private void MakeSideViewData()
        {
            try
            {
                bool[] isFinds = new bool[15];

                PMIPadResult lastpadresult = null;
                PMIMarkResult markresult = null;

                if (CurrnetPad != null)
                {
                    if (CurrnetPad.PMIResults.Count > 0)
                    {
                        lastpadresult = CurrnetPad.PMIResults.LastOrDefault();

                        // [0] = [Pad Status]
                        // [1] = PASS
                        // [2] = FAIL
                        // [3] = NO PROBE MARK
                        // [4] = TOO MANY PROBE MARK
                        // [5] = [Mark Status]
                        // [6] = [SIZE X, SIZE Y, AREA] : 
                        // [7] = [Proximity, T, B, L, R] : 
                        // [8] = [PASS]
                        // [9] = TOO CLOSE TO EDGE
                        // [10] = MARK AREA TOO SMALL
                        // [11] = MARK AREA TOO BIG
                        // [12] = MARK SIZE TOO SMALL
                        // [13] = MARK SIZE TOO BIG
                        // [14] = Please register a reference image for the pad.
                        if (lastpadresult.PadStatus.Any(x => x == PadStatusCodeEnum.PASS) == true)
                        {
                            isFinds[1] = true;
                        }

                        if (lastpadresult.PadStatus.Any(x => x == PadStatusCodeEnum.FAIL) == true)
                        {
                            isFinds[2] = true;
                        }

                        if (lastpadresult.PadStatus.Any(x => x == PadStatusCodeEnum.NO_PROBE_MARK) == true)
                        {
                            isFinds[3] = true;
                        }

                        if (lastpadresult.PadStatus.Any(x => x == PadStatusCodeEnum.TOO_MANY_PROBE_MARK) == true)
                        {
                            isFinds[4] = true;
                        }

                        if(lastpadresult.PadStatus.Any(x => x == PadStatusCodeEnum.NEED_REFERENCE_IMAGE) == true)
                        {
                            isFinds[14] = true;
                        }

                        if (lastpadresult.MarkResults.Count > 0 && SelectedMarkIndex >= 0)
                        {
                            markresult = lastpadresult.MarkResults[SelectedMarkIndex];

                            if (lastpadresult.MarkResults[SelectedMarkIndex].Status.Any(m => m == MarkStatusCodeEnum.PASS) == true)
                            {
                                isFinds[8] = true;
                            }

                            if (lastpadresult.MarkResults[SelectedMarkIndex].Status.Any(m => m == MarkStatusCodeEnum.TOO_CLOSE_TO_EDGE) == true)
                            {
                                isFinds[9] = true;
                            }

                            if (lastpadresult.MarkResults[SelectedMarkIndex].Status.Any(m => m == MarkStatusCodeEnum.MARK_AREA_TOO_SMALL) == true)
                            {
                                isFinds[10] = true;
                            }

                            if (lastpadresult.MarkResults[SelectedMarkIndex].Status.Any(m => m == MarkStatusCodeEnum.MARK_AREA_TOO_BIG) == true)
                            {
                                isFinds[11] = true;
                            }

                            if (lastpadresult.MarkResults[SelectedMarkIndex].Status.Any(m => m == MarkStatusCodeEnum.MARK_SIZE_TOO_SMALL) == true)
                            {
                                isFinds[12] = true;
                            }

                            if (lastpadresult.MarkResults[SelectedMarkIndex].Status.Any(m => m == MarkStatusCodeEnum.MARK_SIZE_TOO_BIG) == true)
                            {
                                isFinds[13] = true;
                            }
                        }
                    }

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        int count = isFinds.Count();

                        for (int i = 0; i < count; i++)
                        {
                            if (isFinds[i] == true)
                            {
                                if ((i == 1) || (i == 8))
                                {
                                    SideViewTextBlocks[i].SideTextBackground = Brushes.Green;
                                }
                                else
                                {
                                    SideViewTextBlocks[i].SideTextBackground = Brushes.Red;
                                }
                            }
                            else
                            {
                                SideViewTextBlocks[i].SideTextBackground = Brushes.Transparent;
                            }

                            if (i == 6)
                            {
                                if (markresult != null)
                                {
                                    SideViewTextBlocks[i].SideTextContents = $"[SIZE X, SIZE Y, AREA]\n{markresult.MarkPosUmFromLT.Width:F1}, {markresult.MarkPosUmFromLT.Height:F1}, {markresult.ScrubArea:F1}";
                                }
                                else
                                {
                                    SideViewTextBlocks[i].SideTextContents = $"[SIZE X, SIZE Y, AREA] : X";
                                }
                            }

                            if (i == 7)
                            {
                                if (markresult != null)
                                {
                                    SideViewTextBlocks[i].SideTextContents = $"[Proximity, T, B, L, R]\n{markresult.MarkProximity.Top:F1}, {markresult.MarkProximity.Bottom:F1}, {markresult.MarkProximity.Left:F1}, {markresult.MarkProximity.Right:F1}";
                                }
                                else
                                {
                                    SideViewTextBlocks[i].SideTextContents = $"[Proximity, T, B, L, R] : X";
                                }
                            }

                            if (i == 14)
                            {
                                SideViewTextBlocks[i].SideTextFontColor = isFinds[i] ? Brushes.White : Brushes.Transparent;
                            }
                        }
                    });

                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        public List<IDeviceObject> GetUnderDutDices(MachineIndex mCoord)
        {
            List<IDeviceObject> dev = new List<IDeviceObject>();

            try
            {
                var cardinfo = this.GetParam_ProbeCard();

                if (CurSelectMode == PMIManualMode.DUT)
                {
                    if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                    {
                        for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                        {
                            //object tmp = cardinfo.ProbeCardDevObjectRef.GetRefOffset(dutIndex);

                            IndexCoord retindex = mCoord.Add(cardinfo.GetRefOffset(dutIndex));

                            IDeviceObject devobj = StageSupervisor.WaferObject.GetDevices().Find(x => x.DieIndexM.Equals(retindex));

                            if (devobj != null)
                            {
                                dev.Add(devobj);
                                dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                            }
                            else
                            {
                                devobj = new DeviceObject();
                                devobj.DieIndexM.XIndex = retindex.XIndex;
                                devobj.DieIndexM.YIndex = retindex.YIndex;

                                dev.Add(devobj);

                                dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return dev;
        }

        //private async Task RunPMI(AsyncObservableCollection<MachineIndex> machineIndices)
        // 함수명바꾸기 ExecutePMI...???
        private async Task RunPMI(List<MachineIndex> targetPMIs)
        {
            var PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

            #region 수행 가능 조건 확인
            List<string> conditions = new List<string>();
            if (PMIInfo.NormalPMIMapTemplateInfo.Count <= 0)
            {
                conditions.Add($"Map Information is wrong.");
            }
            PMIInfo.UpdatePadTableTemplateInfo();

            if (PMIInfo.PadTableTemplateInfo.Count <= 0)
            {
                conditions.Add($"Table Information is wrong.");
            }
            if (PMIInfo.WaferTemplateInfo.Count <= 0)
            {
                conditions.Add($"Wafer Setting Information is wrong.");
            }

            if (conditions.Any())
            {
                LoggerManager.Error($"Manual PMI Process is error.");
                return;
            }
            #endregion

            this.VisionManager().VisionLib.RecipeInit();
            //var uilist = PMIInfo.GetNormalPMIMapTemplate(PMIInfo.SelectedNormalPMIMapTemplateIndex).GetAllEnables();
            //var sortedTest = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.OrderBy(mi => mi.XIndex).ThenBy(mi => mi.YIndex);
            //this.PMIModule().DoPMIInfo.ProcessedPMIMIndex = new AsyncObservableCollection<MachineIndex>(sortedTest);
            await Task.Run(() =>
            {
                targetPMIs.ForEach(ui =>
                {
                    var retval = EventCodeEnum.NONE;

                    UpdateOverlayDelegate(DELEGATEONOFF.OFF);

                    var param = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;
                    var DoPMIInfo = this.PMIModule().DoPMIInfo;

                    // Light
                    if (param.AutoLightEnable.Value == true)
                    {
                        DoPMIInfo.IsTurnAutoLight = true;
                    }
                    else
                    {
                        DoPMIInfo.IsTurnAutoLight = false;
                    }

                    // Focusing

                    if (param.FocusingEachGroupEnable.Value == true)
                    {
                        this.PMIModule().DoPMIInfo.IsTurnFocusing = true;
                        this.PMIModule().DoPMIInfo.RememberFocusingZValue = 0;
                    }
                    else
                    {
                        if (param.FocusingEnable.Value == true)
                        {
                            this.PMIModule().DoPMIInfo.IsTurnFocusing = true;
                            this.PMIModule().DoPMIInfo.RememberFocusingZValue = 0;
                        }
                        else
                        {
                            this.PMIModule().DoPMIInfo.IsTurnFocusing = false;
                        }
                    }

                    // 파라미터의 값이 0 이상이라는 것은, Offset이 발생할 수 있는 것으로 간주.
                    // 메뉴얼 동작 시, 마크 얼라인을 진행하고, 수행할 수 있도록 해보자.
                    if (param.MarkAlignTriggerToleranceUsingPadOffsetX.Value > 0 ||
                       param.MarkAlignTriggerToleranceUsingPadOffsetY.Value > 0)
                    {
                        this.PMIModule().DoPMIInfo.IsTurnOnMarkAlign = true;
                    }
                    else
                    {
                        this.PMIModule().DoPMIInfo.IsTurnOnMarkAlign = false;
                    }

                    //if (param.FocusingEnable.Value == true)
                    //{
                    //    //Module.DoPMIInfo.IsTurnFocusing = Module.CheckFocusingInterval(index);
                    //    DoPMIInfo.IsTurnFocusing = true;
                    //}
                    //else
                    //{
                    //    DoPMIInfo.IsTurnFocusing = false;
                    //}

                    // TODO : 어떤 PMIMap을 쓸건지 고르는 Ui 필요?
                    DoPMIInfo.WaferMapIndex = 0;

                    DoPMIInfo.WorkMode = PMIWORKMODE.MANUAL;

                    List<IDeviceObject> dutlist = new List<IDeviceObject>();
                    //현재 Mode가 Die: 카메라가 현재 보고 있는 Die Index를 얻어 옴
                    if (CurSelectMode == PMIManualMode.DIE)
                    {
                        DeviceObject devobj = new DeviceObject();
                        devobj.DieIndexM.XIndex = ui.XIndex;
                        devobj.DieIndexM.YIndex = ui.YIndex;

                        dutlist.Add(devobj);
                    }
                    else
                    {
                        dutlist = GetUnderDutDices(ui);
                    }

                    foreach (var dut in dutlist)
                    {
                        var pmiindex = DoPMIInfo.ProcessedPMIMIndex.Where(x => (x.XIndex == dut.DieIndexM.XIndex) && x.YIndex == dut.DieIndexM.YIndex);

                        if (pmiindex != null)
                        {
                            foreach (var item in pmiindex.ToList())
                            {
                                DoPMIInfo.ProcessedPMIMIndex.Remove(item);
                            }
                        }

                        DoPMIInfo.ProcessedPMIMIndex.Add(dut.DieIndexM);

                        this.PMIModule().DoPMIInfo.LastPMIDieResult.PassPadCount = 0;
                        this.PMIModule().DoPMIInfo.LastPMIDieResult.FailPadCount = 0;
                        this.PMIModule().DoPMIInfo.LastPMIDieResult.MI = dut.DieIndexM;
                        this.PMIModule().DoPMIInfo.LastPMIDieResult.UI = this.CoordinateManager().MachineIndexConvertToUserIndex(ui);

                        retval = this.PMIModule().DoPMI();
                        if (retval != EventCodeEnum.NONE)
                        {
                            // TODO : 
                            LoggerManager.Error($"Manual PMI Process is error.");
                            break;
                        }
                    }

                    if (retval == EventCodeEnum.NONE)
                    {
                        // First Pad
                        //MovetoFirstPad(this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Last());

                        isExistPMIResult = true;

                        //ChangeButtonForPMIResult();
                        //ChangeButtonForMarkResult();
                        //ChangeButtonSideViewerVisible();

                        //SideViewTargetVisibility = Visibility.Visible;

                        //MakeSideViewData();
                    }
                    else
                    {
                        DrawingGroup.SelectedPMIPadIndex = 0;
                        CurrnetPad = null;
                    }

                    UpdateLabel();

                    UpdateOverlayDelegate(DELEGATEONOFF.ON);
                });

                if (IsAllPMIMode)
                {
                    var sortedTest = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.OrderBy(mi => mi.XIndex).ThenBy(mi => mi.YIndex);
                    this.PMIModule().DoPMIInfo.ProcessedPMIMIndex = new AsyncObservableCollection<MachineIndex>(sortedTest);
                }

                MovetoFirstPad(this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.First());

                ChangeButtonForPMIResult();
                ChangeButtonForMarkResult();
                ChangeButtonSideViewerVisible();

                SideViewTargetVisibility = Visibility.Visible;

                MakeSideViewData();
            });

            void MovetoFirstPad(MachineIndex mi)
            {
                // First Pad
                if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                {
                    this.PMIModule().UpdateDisplayedDevices(this.CurCam, false);

                    DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;

                    DrawingGroup.SelectedPMIPadIndex = 0;
                    SelectedMarkIndex = -1;

                    foreach (var padenable in PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex].PadEnable)
                    {
                        if (padenable.Value == false)
                        {
                            DrawingGroup.SelectedPMIPadIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (curdie.Pads.PMIPadInfos.Count > 0)
                    {
                        CurrnetPad = curdie.Pads.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];
                    }

                    var retval = this.PMIModule().MoveToPad(CurCam, mi, DrawingGroup.SelectedPMIPadIndex);
                    UpdateOverlay();
                }
            }
        }

        public async Task DoPMI()
        {
            ClearPMIResult();
            IsAllPMIMode = false;
            
            await RunPMI(new List<MachineIndex>() { CurCam.GetCurCoordMachineIndex() });
        }

        private async Task DoAllPMI()
        {
            try
            {
                ClearPMIResult();
                IsAllPMIMode = true;
                ThreeButton.IsEnabled = false;

                var param = new List<MachineIndex>(PMIInfo.GetNormalPMIMapTemplate(PMIInfo.SelectedNormalPMIMapTemplateIndex).GetAllEnables());
                await RunPMI(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                ThreeButton.IsEnabled = true;
            }
        }

        private void ClearPMIResult()
        {
            isExistPMIResult = false;
            ChangeButtonForPMIResult();
            ChangeButtonForMarkResult();
            ChangeButtonSideViewerVisible();
            SideViewTargetVisibility = Visibility.Hidden;
        }

        public async Task DoPMI_old()
        {
            try
            {
                //foreach (var d_ in pmiMap_.Value)
                //{
                //                    }

                //var erw = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo().GetNormalPMIMapTemplate(selectedindex_).GetEnable(22,22);//1:enabled , 0:disabled


                List<string> condlst = new List<string>();

                // 조건 확인, PMI를 수행할 수 있는지?

                if (PMIInfo.NormalPMIMapTemplateInfo.Count <= 0)
                {
                    condlst.Add($"Map Information is wrong.");
                }

                // TODO : Check
                this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo().UpdatePadTableTemplateInfo();

                if (PMIInfo.PadTableTemplateInfo.Count <= 0)
                {
                    condlst.Add($"Table Information is wrong.");
                }

                if (PMIInfo.WaferTemplateInfo.Count <= 0)
                {
                    condlst.Add($"Wafer Setting Information is wrong.");
                }

                if (condlst.Count == 0)
                {
                    //this.PMIModule().UpdateGroupingInformation();

                    UpdateOverlayDelegate(DELEGATEONOFF.OFF);

                    await Task.Run(() =>
                    {
                        EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                        // 메뉴얼로 PMI를 동작하면, 그전에 동작했던 결과들을 초기화 한다. 
                        // TODO : 랏드 모드에서 포즈하고, 메뉴얼로 동작시키는 부분에 대한 생각
                        //this.PMIModule().InitPMIResult();

                        // 현재 Mode가 Die : 카메라가 현재 보고 있는 Die Index를 얻어 옴
                        // 현재 Mode가 Dut : 카메라가 현재 보고 있는 Die Index가 First Dut로 인식하고 전체 Dut 정보를 가져온다.

                        var param = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;
                        var DoPMIInfo = this.PMIModule().DoPMIInfo;

                        // Light
                        if (param.AutoLightEnable.Value == true)
                        {
                            DoPMIInfo.IsTurnAutoLight = true;
                        }
                        else
                        {
                            DoPMIInfo.IsTurnAutoLight = false;
                        }

                        // Focusing

                        if (param.FocusingEachGroupEnable.Value == true)
                        {
                            this.PMIModule().DoPMIInfo.IsTurnFocusing = true;
                            this.PMIModule().DoPMIInfo.RememberFocusingZValue = 0;
                        }
                        else
                        {
                            if (param.FocusingEnable.Value == true)
                            {
                                this.PMIModule().DoPMIInfo.IsTurnFocusing = true;
                                this.PMIModule().DoPMIInfo.RememberFocusingZValue = 0;
                            }
                            else
                            {
                                this.PMIModule().DoPMIInfo.IsTurnFocusing = false;
                            }
                        }

                        // 파라미터의 값이 0 이상이라는 것은, Offset이 발생할 수 있는 것으로 간주.
                        // 메뉴얼 동작 시, 마크 얼라인을 진행하고, 수행할 수 있도록 해보자.
                        if (param.MarkAlignTriggerToleranceUsingPadOffsetX.Value > 0 ||
                           param.MarkAlignTriggerToleranceUsingPadOffsetY.Value > 0)
                        {
                            this.PMIModule().DoPMIInfo.IsTurnOnMarkAlign = true;
                        }
                        else
                        {
                            this.PMIModule().DoPMIInfo.IsTurnOnMarkAlign = false;
                        }

                        //if (param.FocusingEnable.Value == true)
                        //{
                        //    //Module.DoPMIInfo.IsTurnFocusing = Module.CheckFocusingInterval(index);
                        //    DoPMIInfo.IsTurnFocusing = true;
                        //}
                        //else
                        //{
                        //    DoPMIInfo.IsTurnFocusing = false;
                        //}

                        // TODO : 어떤 PMIMap을 쓸건지 고르는 Ui 필요?
                        DoPMIInfo.WaferMapIndex = 0;

                        DoPMIInfo.WorkMode = PMIWORKMODE.MANUAL;

                        //DoPMIInfo.ProcessedPMIMIndex.Clear();

                        MachineIndex MI = CurCam.CamSystemMI;

                        List<IDeviceObject> dutlist = new List<IDeviceObject>();
                        //현재 Mode가 Die: 카메라가 현재 보고 있는 Die Index를 얻어 옴
                        if (CurSelectMode == PMIManualMode.DIE)
                        {
                            DeviceObject devobj = new DeviceObject();
                            devobj.DieIndexM.XIndex = MI.XIndex;
                            devobj.DieIndexM.YIndex = MI.YIndex;

                            dutlist.Add(devobj);
                        }
                        else
                        {
                            dutlist = GetUnderDutDices(MI);
                        }

                        foreach (var dut in dutlist)
                        {
                            var pmiindex = DoPMIInfo.ProcessedPMIMIndex.Where(x => (x.XIndex == dut.DieIndexM.XIndex) && x.YIndex == dut.DieIndexM.YIndex);

                            if (pmiindex != null)
                            {
                                foreach (var item in pmiindex.ToList())
                                {
                                    DoPMIInfo.ProcessedPMIMIndex.Remove(item);
                                }
                            }

                            DoPMIInfo.ProcessedPMIMIndex.Add(dut.DieIndexM);

                            this.PMIModule().DoPMIInfo.LastPMIDieResult.PassPadCount = 0;
                            this.PMIModule().DoPMIInfo.LastPMIDieResult.FailPadCount = 0;
                            this.PMIModule().DoPMIInfo.LastPMIDieResult.MI = dut.DieIndexM;
                            this.PMIModule().DoPMIInfo.LastPMIDieResult.UI = this.CoordinateManager().MachineIndexConvertToUserIndex(MI);

                            retval = this.PMIModule().DoPMI();

                            if (retval != EventCodeEnum.NONE)
                            {
                                // TODO : 
                                LoggerManager.Error($"Manual PMI Process is error.");
                                break;
                            }
                        }

                        if (retval == EventCodeEnum.NONE)
                        {
                            // First Pad
                            if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                            {
                                this.PMIModule().UpdateDisplayedDevices(this.CurCam, false);

                                MachineIndex mi = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Last();
                                DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;

                                DrawingGroup.SelectedPMIPadIndex = 0;
                                SelectedMarkIndex = -1;

                                foreach (var padenable in PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex].PadEnable)
                                {
                                    if (padenable.Value == false)
                                    {
                                        DrawingGroup.SelectedPMIPadIndex++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (curdie.Pads.PMIPadInfos.Count > 0)
                                {
                                    CurrnetPad = curdie.Pads.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];
                                }

                                retval = this.PMIModule().MoveToPad(CurCam, mi, DrawingGroup.SelectedPMIPadIndex);
                            }

                            isExistPMIResult = true;

                            ChangeButtonForPMIResult();
                            ChangeButtonForMarkResult();
                            ChangeButtonSideViewerVisible();

                            SideViewTargetVisibility = Visibility.Visible;

                            MakeSideViewData();
                        }
                        else
                        {
                            DrawingGroup.SelectedPMIPadIndex = 0;
                            CurrnetPad = null;
                        }
                    });

                    //CurrentPadIndex = 0;

                    UpdateLabel();

                    UpdateOverlayDelegate(DELEGATEONOFF.ON);
                }
                else
                {
                    // TODO : Pop up the Dialog

                    //EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Information",
                    //                            "Parameter has changed. Do you want to save it?"
                    //                            + Environment.NewLine + "Ok         : Save & Exit"
                    //                            + Environment.NewLine + "Cancel     : Cancel Exit"
                    //                            + Environment.NewLine + "Just Exit  : Exit without Save",
                    //                            EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary
                    //                            , "Just Exit");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ManualPMIProcessModule] [DoPMI()] : {err}");
            }
        }

        //public EventCodeEnum Grouping()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        //List<PMIGroupData> PMIPadGroups = new List<PMIGroupData>();

        //        retval = this.PMIModule().PadGroupingMethod(SelectedPadTableTemplateIndex);

        //        if (retval == EventCodeEnum.NONE)
        //        {
        //            retval = this.PMIModule().MakeGroupSequence(SelectedPadTableTemplateIndex);
        //        }

        //        return retval;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[ManualPMIProcessModule] [Grouping()] : {err}");
        //    }
        //    return retval;
        //}

        private EventCodeEnum InitPnpUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ManualPMIProcessModule] [InitPnpUI()] : {err}");
            }
            return retVal;
        }


        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            //EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                isExistPMIResult = false;

                UpdateOverlayDelegate(DELEGATEONOFF.OFF);

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[ManualPMIProcessModule] [Cleanup()] : Error");
                }
                //retVal = ParamValidation();
                //retVal = await base.Cleanup(parameter);

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ManualPMIProcessModule] [Cleanup()] : {err}");

            }
            finally
            {
                Wafer.MapViewStageSyncEnable = RememberMapViewStageSyncEnable;
                Wafer.MapViewCurIndexVisiablity = RememberMapViewCurIndexVisiablity;
            }

            return retVal;
        }

        /// <summary>
        /// Wafer에 PMI map을 설정하는 창을 호출하는 함수
        /// </summary>
        public async Task NormalPMISetupCommand()
        {
            try
            {
                //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                

                try
                {
                    await ManualPMISetupControl.ShowDialogControl();
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[NormalPMIMapSetupModule] [WaferMapSetupCommand()] : {err}");
                }

                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private int _CurrentPadIndex = -1;
        //public int CurrentPadIndex
        //{
        //    get { return _CurrentPadIndex; }
        //    set
        //    {
        //        if (value != _CurrentPadIndex)
        //        {
        //            _CurrentPadIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private PMIPadObject _CurrnetPad;
        public PMIPadObject CurrnetPad
        {
            get { return _CurrnetPad; }
            set
            {
                if (value != _CurrnetPad)
                {
                    _CurrnetPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override void UpdateLabel()
        {
            MachineIndex curmi = new MachineIndex(-1, -1);
            if (CurCam != null)
            {
                curmi = CurCam.GetCurCoordMachineIndex();
            }

            if (CurrnetPad != null)
            {
                // Result가 존재하면
                if (CurrnetPad.PMIResults.Count > 0)
                {
                    PMIPadResult result = CurrnetPad.PMIResults[CurrnetPad.SelectedPMIPadResultIndex];

                    ObservableCollection<PadStatusCodeEnum> status = result.PadStatus;

                    if ((status != null) && (status.Count > 0))
                    {
                        MachineIndex mi = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Last();
                        DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;

                        if (SelectedMarkIndex >= 0)
                        {
                            int allmarkcount = result.MarkResults.Count;

                            //StepLabel = $"{curmi.XIndex},{curmi.YIndex} Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode} \nPad Index : {DrawingGroup.SelectedPMIPadIndex + 1}/{curdie.Pads.PMIPadInfos.Count}\nMark Index : {SelectedMarkIndex + 1}/{allmarkcount}";
                            StepLabel = $"Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode} \nPad Index : {DrawingGroup.SelectedPMIPadIndex + 1}/{curdie.Pads.PMIPadInfos.Count}\nMark Index : {SelectedMarkIndex + 1}/{allmarkcount}";
                        }
                        else
                        {
                            //StepLabel = $"{curmi.XIndex},{curmi.YIndex} Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode} \nPad Index : {DrawingGroup.SelectedPMIPadIndex + 1}/{curdie.Pads.PMIPadInfos.Count}";
                            StepLabel = $"Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode} \nPad Index : {DrawingGroup.SelectedPMIPadIndex + 1}/{curdie.Pads.PMIPadInfos.Count}";
                        }
                    }
                }
                else
                {
                    //StepLabel = $"{curmi.XIndex},{curmi.YIndex} Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode}";
                    StepLabel = $"Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode}";
                }
            }
            else
            {
                //StepLabel = $"{curmi.XIndex},{curmi.YIndex} Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode}";
                StepLabel = $"Operation Mode : {CurSelectMode}\nView Mode : {LoggingMode}";
            }
        }
    }
}
