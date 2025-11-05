using System;
using System.Collections.Generic;

namespace NeedleCleanFocus
{
    using ProberInterfaces;
    using ProberInterfaces.NeedleClean;
    using SubstrateObjects;
    using System.ComponentModel;
    using ProberErrorCode;
    using LogModule;
    using NeedleCleanerModuleParameter;
    using ProberInterfaces.State;
    using ProberInterfaces.Param;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Threading.Tasks;

    public class CleanPadFocusBySensor : IProcessingModule, INotifyPropertyChanged, ITemplateModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region IParamNode
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion
        public bool Initialized { get; set; } = false;

        private IStateModule _NeedleCleanModule;
        public IStateModule NeedleCleanModule
        {
            get { return _NeedleCleanModule; }
            set
            {
                if (value != _NeedleCleanModule)
                {
                    _NeedleCleanModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NeedleCleanObject NC { get { return this.StageSupervisor().NCObject as NeedleCleanObject; } }

        private NeedleCleanDeviceParameter _NeedleCleanerParam;
        public NeedleCleanDeviceParameter NeedleCleanerParam
        {
            get { return (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam; }
            set
            {
                if (value != _NeedleCleanerParam)
                {
                    _NeedleCleanerParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SubModuleMovingStateBase MovingState { get; set; }

        public CleanPadFocusBySensor()
        {

        }
        public CleanPadFocusBySensor(IStateModule Module)
        {
            _NeedleCleanModule = Module;
        }

        private IFocusing _TouchSensorBaseFocusModel;

        public IFocusing TouchSensorBaseFocusModel
        {
            get
            {
                if (_TouchSensorBaseFocusModel == null)
                    _TouchSensorBaseFocusModel = this.FocusManager().GetFocusingModel((this.StageSupervisor().NCObject.NCSysParam_IParam as NeedleCleanSystemParameter).FocusingModuleDllInfo);

                return _TouchSensorBaseFocusModel;
            }
            set { _TouchSensorBaseFocusModel = value; }
        }
        private IFocusParameter TouchSensorBaseFocusModelParam => (this.StageSupervisor().NCObject.NCSysParam_IParam as NeedleCleanSystemParameter).FocusParam;


        private AsyncCommand _FocusingCommand;
        public ICommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(CmdFocusing, new Func<bool>(() => !FocusingCommandCanExecute));
                return _FocusingCommand;
            }
        }

        private bool _FocusingCommandCanExecute;
        public bool FocusingCommandCanExecute
        {
            get { return _FocusingCommandCanExecute; }
            set
            {
                if (value != _FocusingCommandCanExecute)
                {
                    _FocusingCommandCanExecute = value;
                    RaisePropertyChanged();
                }
            }
        }

        private async Task<EventCodeEnum> CmdFocusing()
        {
            EventCodeEnum ret = await Task.Run(() => FocusingFunc());
            return ret;
        }

        public EventCodeEnum FocusingFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                FocusingCommandCanExecute = false;

                RetVal = TouchSensorBaseFocusModel.Focusing_Retry(TouchSensorBaseFocusModelParam, false, false, false, this);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            finally
            {
                FocusingCommandCanExecute = true;

            }
            return RetVal;
        }

        #region ISubModule
        private SubModuleStateBase _SubModuleState;
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

        private NeedleCleanSystemParameter _NCSysParam;
        public NeedleCleanSystemParameter NCSysParam
        {
            get { return (NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam; }
            set
            {
                if (value != _NCSysParam)
                {
                    _NCSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private NeedleCleanSystemCommonParameter NCSysCommonParam;

        public EventCodeEnum StartJob()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum Execute()       // 외부에서(ex,lot) Needle clean module 의 
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        public INeedleCleanDeviceObject NeedleCleanDeviceObject { get; set; }
        //private IMotionManager Motion;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //_NeedleCleanModule = this.NeedleCleaner();
                    MovingState = new SubModuleStopState(this);
                    SubModuleState = new SubModuleIdleState(this);
                    //NeedleCleanDeviceObject = this.StageSupervisor().NeedleCleanObject.NeedleCleanDeviceObject;

                    //NC = (NeedleCleanObject)this.StageSupervisor().NeedleCleanObject;
                    var stage = this.StageSupervisor();
                    var ncModule = this.NeedleCleaner();
                    NeedleCleanModule = ncModule;
                    NeedleCleanerParam = (NeedleCleanDeviceParameter)ncModule.NeedleCleanDeviceParameter_IParam;

                    //Motion = this.MotionManager();
                    this.NeedleCleaner().DelFocusing = new CleanUnitFocusing5pt(DoCleanUnitFocusing5pt);

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
            }

            return retval;
        }

        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                foreach (var list in NC.NCSheetVMDefs)
                {
                    list.FlagFocusDone = false;
                    list.FlagRequiredFocus = 0;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - PreRun() : Error occured.");
                throw err;
            }
            return retVal;
        }
        #endregion

        //#region IHasDevParameterizable
        ////private IParam _DevParam;
        ////[ParamIgnore]
        ////public IParam DevParam
        ////{
        ////    get { return _DevParam; }
        ////    set
        ////    {
        ////        if (value != _DevParam)
        ////        {
        ////            _DevParam = value;
        ////            RaisePropertyChanged();
        ////        }
        ////    }
        ////}

        //public EventCodeEnum LoadDevParameter()
        //{


        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    //IParam tmpParam = null;
        //    //tmpParam = new WA_EdgeParam_Standard();
        //    //tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
        //    //RetVal = Extensions_IParam.LoadParameter(ref tmpParam, typeof(WA_EdgeParam_Standard));

        //    //if (RetVal == EventCodeEnum.NONE)
        //    //{
        //    //    Param = tmpParam as WA_EdgeParam_Standard;
        //    //    EdgeStandardParam = tmpParam as WA_EdgeParam_Standard;
        //    //    //TempEdgeStandardParam;
        //    //}


        //    return RetVal;
        //}

        //public EventCodeEnum SaveDevParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    //try
        //    //{
        //    //    if (Param != null)
        //    //    {
        //    //        Param = EdgeStandardParam;
        //    //        RetVal = Extensions_IParam.SaveParameter(Param);
        //    //        RetVal = Extensions_IParam.DeleteTempFile(EdgeStandardParam, typeof(WA_EdgeParam_Standard));
        //    //    }
        //    //}
        //    //catch (Exception err)
        //    //{
        //    //    LoggerManager.Debug($"{err.ToString()}. EdgeStndard - SaveDevParameter() : Error occured.");
        //    //}

        //    return RetVal;
        //}
        //#endregion

        public void DeInitModule()
        {
        }

        enum FCS_List
        {
            FCS_TOPLEFT = 0,
            FCS_BOTTOMLEFT = 1,
            FCS_CENTER = 2,
            FCS_BOTTOMRIGHT = 3,
            FCS_TOPRIGHT = 4
        }

        enum FCS_ReadyState
        {
            FCS_NOT_NEED = 0,
            FCS_READY = 1,
            FCS_NOT_INITIALIZE_RANGE_ERROR = 2
        }

        public EventCodeEnum DoExecute()         //실제 동작하는 logic
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            bool bStart = false;

            //MovingState.Moving();
            try
            {
                if (IsExecute() == true)
                {
                    for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                    {
                        if (IsTimeToFocus(i) == true)
                        {
                            RetVal = BeginFocusTask(i);

                            if (RetVal == EventCodeEnum.NONE)
                            { bStart = true; }
                            else
                            {
                                bStart = false;
                                break;
                            }

                        }
                    }

                    if (bStart == true)
                    { RetVal = DoCleanUnitFocusing(); }

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        SubModuleState = new SubModuleDoneState(this);
                    }
                    else
                    {
                        SubModuleState = new SubModuleErrorState(this);
                    }

                    return RetVal;
                }

                SubModuleState = new SubModuleDoneState(this);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - ReadNcCurPosForPin() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return RetVal;
        }
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                foreach (var list in NC.NCSheetVMDefs)
                {
                    list.FlagFocusDone = false;
                    list.FlagRequiredFocus = 0;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanFocusBySensor - DoClearData() : Error occured.");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }

        public EventCodeEnum DoRecovery()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoExitRecovery()
        {
            throw new NotImplementedException();
        }

        public bool IsExecute()
        {
            bool bRet = false;
            try
            {
                for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                {
                    if (IsTimeToFocus(i) == true)
                    {
                        bRet = true;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - IsExecute() : Error occured.");
                throw err;
            }
            return bRet;
        }

        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }

        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }

        public EventCodeEnum ExitRecovery()
        {
            return SubModuleState.ExitRecovery();
        }

        #region NcMotionFunctions
        private EventCodeEnum GetNcHeight(int ncNum, out double posVal)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            NCCoordinate mccoord_cur = new NCCoordinate();

            try
            {
                if (NC.NCSysParam.TouchSensorAttached.Value == true)
                {
                    mccoord_cur = this.NeedleCleaner().ReadNcCurPosForSensor(ncNum);

                    posVal = mccoord_cur.Z.Value;
                }
                else
                {
                    mccoord_cur = this.NeedleCleaner().ReadNcCurPosForWaferCam(ncNum);

                    posVal = mccoord_cur.Z.Value;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - GetNcHeight() : Error occured.");
                posVal = 0;
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return RetVal;
        }

        private EventCodeEnum MoveCleanPadPosForFocusing(int ncNum, double nPosX, double nPosY, double overdrive)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (NC.NCSysParam.TouchSensorAttached.Value == true)
                { RetVal = MoveCleanPadPosXYForSensor(ncNum, nPosX, nPosY, overdrive); }
                else
                { RetVal = MoveCleanPadPosXYForWaferHigh(ncNum, nPosX, nPosY, overdrive); }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - MoveCleanPadPosForFocusing() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return RetVal;
        }

        private EventCodeEnum MoveCleanPadPosXYForWaferHigh(int ncNum, double nPosX, double nPosY, double overdrive)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                NCCoordinate mccoord = new NCCoordinate();

                mccoord.X.Value = nPosX + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value;
                mccoord.Y.Value = nPosY + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                mccoord.Z.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Z.Value;

                RetVal = this.StageSupervisor().StageModuleState.WaferHighCamCoordMoveNCpad(mccoord, overdrive);
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - MoveCleanPadPosXYForWaferHigh() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return RetVal;
        }

        private EventCodeEnum MoveCleanPadPosXYForWaferLow(int ncNum, double nPosX, double nPosY, double overdrive)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                NCCoordinate mccoord = new NCCoordinate();

                mccoord.X.Value = nPosX + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value;
                mccoord.Y.Value = nPosY + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                mccoord.Z.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Z.Value;

                RetVal = this.StageSupervisor().StageModuleState.WaferLowCamCoordMoveNCpad(mccoord, overdrive);
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - MoveCleanPadPosXYForWaferLow() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return RetVal;
        }

        private EventCodeEnum MoveCleanPadPosXYForSensor(int ncNum, double nPosX, double nPosY, double overdrive)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            PinCoordinate pincoord = new PinCoordinate();
            NCCoordinate mccoord = new NCCoordinate();

            try
            {
                var ncHeightmodule = this.NeedleCleaner().NCHeightProfilingModule;
                double OffsetZ = 0;

                mccoord.X.Value = nPosX + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value;
                mccoord.Y.Value = nPosY + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                mccoord.Z.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Z.Value;

                pincoord.X.Value = NC.NCSysParam.SensorPos.Value.X.Value;
                pincoord.Y.Value = NC.NCSysParam.SensorPos.Value.Y.Value;
                pincoord.Z.Value = NC.NCSysParam.SensorPos.Value.Z.Value;

                OffsetZ = ncHeightmodule.GetPZErrorComp(mccoord.X.Value, mccoord.Y.Value, pincoord.Z.Value);

                pincoord.Z.Value = pincoord.Z.Value + OffsetZ;

                RetVal = this.StageSupervisor().StageModuleState.TouchSensorSensingMoveNCPad(mccoord, pincoord, overdrive);

            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - MoveCleanPadPosForSensor() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }

            return RetVal;
        }

        private EventCodeEnum NcRelMoveZ(double inc)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = this.MotionManager().RelMove(this.NeedleCleaner().NCAxis.AxisType.Value, inc);

                System.Threading.Thread.Sleep(100);

                //retVal = this.MotionManager().AbsMove(this.NeedleCleanModule().NCAxis.AxisType.Value, pos);
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - NcRelMoveZ() : Error occured.");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return retVal;
        }

        #endregion

        #region NcFocusing

        private bool IsReadyToCleaning()
        {
            try
            {
                if (NC.NCSysParam.TouchSensorRegistered.Value != true || NC.NCSysParam.TouchSensorBaseRegistered.Value != true ||
                    NC.NCSysParam.TouchSensorPadBaseRegistered.Value != true || NC.NCSysParam.TouchSensorOffsetRegistered.Value != true)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - IsReadyToCleaning() : Error occured.");

                return false;
            }
        }

        public EventCodeEnum DoAutoDetect(int ncNum)
        {
            // This focusing function must be used whenb nc disk position is valid
            // In case nc disk position is not set yet, use rough focusing

            LoggerManager.Debug($"Begin DoAutoDetect()");

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            if (this.MotionManager().IsEmulMode(this.MotionManager().GetAxis(EnumAxisConstants.PZ)))
            {
                return EventCodeEnum.NONE;
            }

            try
            {
                double orgHeight;
                double curZ;
                bool bFound;

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return EventCodeEnum.NONE;
                }


                RetVal = GetNcHeight(ncNum, out orgHeight);

                if (RetVal == EventCodeEnum.NONE)
                {
                    if (this.NeedleCleaner().IsNCSensorON() == true)
                    {
                        // Already touch sensor is ON, move down firstly
                        for (int m = 1; m <= 50; m++)
                        {
                            RetVal = NcRelMoveZ(-100);
                            if (RetVal != EventCodeEnum.NONE) { break; }

                            if (this.NeedleCleaner().IsNCSensorON() == false)
                            {
                                // For clearance, z down one more time
                                RetVal = NcRelMoveZ(-100);
                                break;
                            }
                        }
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    bFound = false;
                    for (int m = 1; m <= 20; m++)
                    {
                        RetVal = NcRelMoveZ(100);
                        if (RetVal != EventCodeEnum.NONE) { break; }

                        if (this.NeedleCleaner().IsNCSensorON() == true)
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (bFound == false)
                    {
                        // Error exception
                        // No response from sensor, break operation
                        LoggerManager.Debug($"Could not find sensed height in step 1");
                        RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;

                        RetVal = GetNcHeight(ncNum, out curZ);
                        if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                        RetVal = NcRelMoveZ(orgHeight - curZ);

                        RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;
                        return RetVal;
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    bFound = false;
                    for (int m = 1; m <= 50; m++)
                    {
                        RetVal = NcRelMoveZ(-10);
                        if (RetVal != EventCodeEnum.NONE) { break; }

                        if (this.NeedleCleaner().IsNCSensorON() == false)
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (bFound == false)
                    {
                        // Error exception
                        // No response from sensor, break operation
                        LoggerManager.Debug($"Could not find sensed height in step 2");
                        RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;

                        RetVal = GetNcHeight(ncNum, out curZ);
                        if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                        RetVal = NcRelMoveZ(orgHeight - curZ);

                        RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;
                        return RetVal;
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    bFound = false;
                    for (int m = 1; m <= 100; m++)
                    {
                        RetVal = NcRelMoveZ(1);
                        if (RetVal != EventCodeEnum.NONE) { break; }

                        if (this.NeedleCleaner().IsNCSensorON() == true)
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (bFound == false)
                    {
                        // Error exception
                        // No response from sensor, break operation
                        LoggerManager.Debug($"Could not find sensed height in step 3");
                        RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;

                        RetVal = GetNcHeight(ncNum, out curZ);
                        if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                        RetVal = NcRelMoveZ(orgHeight - curZ);

                        RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;
                        return RetVal;
                    }
                }

                return RetVal;

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - DoAutoDetect() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return RetVal;
        }
        #endregion

        #region NcCleanUnitFocusing
        public EventCodeEnum DoCleanUnitFocusing1pt(int ncNum)
        {
            // Measure clean pad height at 1 point
            // (*)기본적으로 이 함수는 적어도 1회 이상 전체 영역에 대한 포커싱이 완료된 이후에 불려야 함.

            // (*) 주의 : 이 함수에서는 클린 유닛 DOWN을 호출하지 않는다. 반드시 이 함수 밖에서 내려주어야 한다. 잊지말것!!!

            // 클린 시트가 일단 등록이 완료된 후에는, 그리고 머신 이닛 후 최초 1회는 적어도 한 번 5포인트 높이를 확인해서 평탄 확인을 해야 한다.
            // 평탄 확인이 완료된 후 부터는 1포인트만 포커싱하여 이전에 알고 있던 높이와 비교하여 변화폭을 확인하는데
            // 이 과정에서 발생하는 오차는 기계적 반복정밀도 + 포커싱 반복정밀도 이므로 보상을 해 봐야 의미가 없다.
            // 따라서 1포인트 포커싱 결과는 실제 클리닝 높이에 반영하지 않으며 단지 안전을 위한 확인 절차로서 사용한다.

            //MovingState.Moving();
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            EventCodeEnum tmpRetVal = EventCodeEnum.UNDEFINED;
            double nPosX;
            double nPosY;
            double nPosZ;
            double prvZ;

            try
            {
                //WriteProLog(EventCodeEnum.CleanPad_Focusing_1Point_Start);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.CleanPad_Focusing_1Point_Start);


                if (NC.NCSheetVMDef.FlagTouchSensorBaseConfirmed == false)
                {
                    tmpRetVal = DoTouchSensorBaseFocusing();
                    if (tmpRetVal != EventCodeEnum.NONE) return tmpRetVal;
                }


                nPosX = 0;
                nPosY = 0;
                prvZ = this.NeedleCleaner().GetMeasuredNcPadHeight(ncNum, nPosX, nPosY);

                if (NC.NCSysParam.TouchSensorAttached.Value == true)
                {
                    // 터치 센서를 사용하는 경우 시작 위치를 약간 내려서 시작한다.
                    tmpRetVal = MoveCleanPadPosForFocusing(ncNum, nPosX, nPosY, prvZ - 100);
                }
                else
                {
                    tmpRetVal = MoveCleanPadPosForFocusing(ncNum, nPosX, nPosY, prvZ);
                }

                if (tmpRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"Failed to move focusing position");
                    RetVal = tmpRetVal;
                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure);

                    return RetVal;
                }

                if (this.StageSupervisor().MarkObject.AlignState.Value == AlignStateEnum.IDLE)
                {
                    LoggerManager.Debug($"Focusing operation is aborted because mark alignment was failed");
                    RetVal = EventCodeEnum.NEEDLE_CLEANING_MARK_ALIGN_NOT_READY;
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure);

                    return RetVal;
                }
                
                if (this.NeedleCleaner().IsCleanPadUP() == false)
                {
                    if (NC.NCSysParam.NC_TYPE.Value == NC_MachineType.AIR_NC)
                    {
                        tmpRetVal = this.NeedleCleaner().CleanPadUP(false);

                        if (tmpRetVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"Failed to up clean pad");
                            RetVal = tmpRetVal;
                            //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);

                            return RetVal;
                        }
                    }
                }

                //if (NC.NCSysParam.SheetDefs[ncNum].NC_TYPE.Value == NeedleCleanSystemParameter.NC_MachineType.AIR_NC)
                //{
                //    tmpRetVal = MoveCleanPadZupForFocusing(ncNum, -100);
                //    if (tmpRetVal != EventCodeEnum.NONE)
                //    {
                //        LoggerManager.Debug($"Failed to move focusing position");
                //        RetVal = tmpRetVal;
                //        return RetVal;
                //    }
                //}

                tmpRetVal = this.NeedleCleaner().WaitForCleanPadUp();
                if (tmpRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"Failed to move up clean pad");
                    RetVal = tmpRetVal;
                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);

                    return RetVal;
                }

                tmpRetVal = DoAutoDetect(ncNum);
                if (tmpRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"Failed to detect focused height");
                    RetVal = tmpRetVal;
                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);

                    return RetVal;
                }
                else
                {
                    RetVal = GetNcHeight(ncNum, out nPosZ);
                    if (RetVal != EventCodeEnum.NONE)
                    {
                        //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);

                        return RetVal;
                    }
                }

                //var offsetZ = this.NeedleCleaner().NCHeightProfilingModule.GetPZErrorComp(nPosX, nPosY, NC.NCSysParam.SensorPos.Value.Z.Value);
                var offsetZ = this.NeedleCleaner().NCHeightProfilingModule.GetPZErrorComp(nPosX + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value,
                                                                                          nPosY + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value,
                                                                                          NC.NCSysParam.SensorPos.Value.Z.Value);

                nPosZ += offsetZ;

                LoggerManager.Debug($"Offset (Center) Z value : {offsetZ}");

                if (Math.Abs(prvZ - nPosZ) > NC.NCSysParam.CleanSheetFocusThreshold.Value)
                {
                    LoggerManager.Debug($"Focusing result is over tolerance");
                    RetVal = EventCodeEnum.NEEDLE_CLEANING_HEIGHT_OVER_TOLERANCE;
                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);

                    return RetVal;
                }
                //MovingState.Stop();

                //WriteProLog(EventCodeEnum.CleanPad_Focusing_OK);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.CleanPad_Focusing_OK);

                RetVal = EventCodeEnum.NONE;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - DoCleanUnitFocusing1pt() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);

                //MovingState.Stop();
            }

            finally
            {
                // Zdown to safe height
                tmpRetVal = this.NeedleCleaner().CleanPadDown(true);
                if (tmpRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"Failed to move down clean pad");
                    RetVal = tmpRetVal;
                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                }
            }

            return RetVal;
        }

        // Test Code

        //private IFocusing _TouchSensorFocusModel;

        //public IFocusing TouchSensorFocusModel
        //{
        //    get { return _TouchSensorFocusModel; }
        //    set { _TouchSensorFocusModel = value; }
        //}

        //private IFocusParameter _TouchSensorFocusParam;

        //public IFocusParameter TouchSensorFocusParam
        //{
        //    get { return _TouchSensorFocusParam; }
        //    set { _TouchSensorFocusParam = value; }
        //}

        //private AsyncCommand _FocusingCommand;
        //public ICommand FocusingCommand
        //{
        //    get
        //    {
        //        if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(CmdFocusing, new Func<bool>(() => !FocusingCommandCanExecute));
        //        return _FocusingCommand;
        //    }
        //}

        //private bool _FocusingCommandCanExecute;
        //public bool FocusingCommandCanExecute
        //{
        //    get { return _FocusingCommandCanExecute; }
        //    set
        //    {
        //        if (value != _FocusingCommandCanExecute)
        //        {
        //            _FocusingCommandCanExecute = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private async Task<EventCodeEnum> CmdFocusing()
        //{
        //    EventCodeEnum ret = await Task.Run(() => FocusingFunc());
        //    return ret;
        //}

        //public EventCodeEnum FocusingFunc()
        //{
        //    // Test Code

        //    EventCodeEnum RetVal = EventCodeEnum.NONE;

        //    try
        //    {
        //        FocusingCommandCanExecute = false;

        //        RetVal = TouchSensorFocusModel.Focusing_Retry(TouchSensorFocusParam,
        //                        false, //==> Light Change
        //                        false, //==> brute force retry
        //                        false); //==> find potential 

        //    }
        //    catch (Exception err)
        //    {
        //        RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
        //    }
        //    finally
        //    {
        //        FocusingCommandCanExecute = true;

        //    }
        //    return RetVal;
        //}

        public EventCodeEnum DoCleanUnitFocusing5pt(int ncNum)
        {
            // (*) 주의 : 이 함수에서는 클린 유닛 DOWN을 호출하지 않는다. 반드시 이 함수 밖에서 내려주어야 한다. 잊지말것!!!

            // Measure clean pad height at 1 point
            // nPos : position on clean pad (top & center of nc pad is 0, 0)

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            EventCodeEnum tmpRetVal = EventCodeEnum.UNDEFINED;

            double nPosX = 0;
            double nPosY = 0;
            double nPosZ = 0;
            double curZ = 0;
            bool bFirst = true;
            double minZ;
            double maxZ;
            double focus_margin = 5000;

            // Test Code
            //double SensorTipPos = 0;
            //double SensorBasePos = 0;
            //double FocusedResult1 = 0;
            //double FocusedResult2 = 0;
            //double FocusedResult3 = 0;
            //double FocusedResult4 = 0;
            //double FocusedResult5 = 0;
            //string line;
            //string dirPath = @"C:\ProberSystem\NcTest.txt";

            try
            {
                //WriteProLog(EventCodeEnum.CleanPad_Focusing_5Point_Start);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.CleanPad_Focusing_5Point_Start);


                // Test Code                
                //this.VisionManager().StartGrab(this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).GetChannelType());

                //foreach (var light in NC.NCSysParam.LightForFocusSensor)
                //{
                //    this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(light.Type.Value, light.Value.Value);

                //}
                //this.StageSupervisor().StageModuleState.ZCLEARED();
                //this.StageSupervisor().StageModuleState.TouchSensorHighViewMove(NC.NCSysParam.SensorFocusedPos.Value.X.Value,
                //                                                        NC.NCSysParam.SensorFocusedPos.Value.Y.Value,
                //                                                        NC.NCSysParam.SensorFocusedPos.Value.Z.Value);
                //RetVal = FocusingFunc();
                //PinCoordinate cur_ccord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();                
                //SensorTipPos = cur_ccord.GetZ();



                //foreach (var light in NC.NCSysParam.LightForBaseFocusSensor)
                //{
                //    this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(light.Type.Value, light.Value.Value);
                //}
                //this.StageSupervisor().StageModuleState.ZCLEARED();
                //this.StageSupervisor().StageModuleState.TouchSensorHighViewMove(NC.NCSysParam.SensorBasePos.Value.X.Value,
                //                                                         NC.NCSysParam.SensorBasePos.Value.Y.Value,
                //                                                         NC.NCSysParam.SensorBasePos.Value.Z.Value);

                //RetVal = FocusingFunc();
                //PinCoordinate cur_ccord2 = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                //SensorBasePos = cur_ccord2.GetZ();

                //this.StageSupervisor().StageModuleState.ZCLEARED();


                if (NC.NCSheetVMDef.FlagTouchSensorBaseConfirmed == false)
                {
                    tmpRetVal = DoTouchSensorBaseFocusing();
                    if (tmpRetVal != EventCodeEnum.NONE) return tmpRetVal;                   
                }


                // Zdown to safe height
                // this.MotionManager().StageRelMove  ??????
                List<NCCoordinate> tmpFoucsResult = new List<NCCoordinate>();
                NCCoordinate curPos = new NCCoordinate(0, 0, 0);

                tmpFoucsResult.Clear();
                foreach (FCS_List cur_pos in (FCS_List[])Enum.GetValues(typeof(FCS_List)))
                {
                    // 5mm margin
                    if (cur_pos == FCS_List.FCS_TOPLEFT)
                    {
                        nPosX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + focus_margin;
                        nPosY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - focus_margin;
                    }
                    else if (cur_pos == FCS_List.FCS_CENTER)
                    {
                        nPosX = 0;
                        nPosY = 0;
                    }
                    else if (cur_pos == FCS_List.FCS_TOPRIGHT)
                    {
                        nPosX = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - focus_margin;
                        nPosY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - focus_margin;
                    }
                    else if (cur_pos == FCS_List.FCS_BOTTOMRIGHT)
                    {
                        nPosX = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - focus_margin;
                        nPosY = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + focus_margin;
                    }
                    else if (cur_pos == FCS_List.FCS_BOTTOMLEFT)
                    {
                        nPosX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + focus_margin;
                        nPosY = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + focus_margin;
                    }
                    else
                    {
                        LoggerManager.Debug($"Undefined focusing position");
                        RetVal = EventCodeEnum.PARAM_ERROR;
                        //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                        return RetVal;
                    }

                    nPosZ = 0 - NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Z.Value;

                    if (NC.NCSysParam.TouchSensorAttached.Value == true)
                    {
                        // 터치 센서를 사용하는 경우 시작 위치를 약간 내려서 시작한다.
                        tmpRetVal = MoveCleanPadPosForFocusing(ncNum, nPosX, nPosY, nPosZ - 1000);
                    }
                    else
                    {
                        tmpRetVal = MoveCleanPadPosForFocusing(ncNum, nPosX, nPosY, nPosZ);
                    }

                    if (tmpRetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Failed to move focusing position");
                        RetVal = tmpRetVal;
                        //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                        return RetVal;
                    }

                    if (this.StageSupervisor().MarkObject.AlignState.Value == AlignStateEnum.IDLE)
                    {
                        LoggerManager.Debug($"Focusing operation is aborted because mark alignment was failed");
                        RetVal = EventCodeEnum.NEEDLE_CLEANING_MARK_ALIGN_NOT_READY;
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure);

                        return RetVal;
                    }

                    if (bFirst == true)
                    {
                        if (NC.NCSysParam.NC_TYPE.Value == NC_MachineType.AIR_NC)
                        {
                            if (this.NeedleCleaner().IsCleanPadUP() == false)
                            {
                                tmpRetVal = this.NeedleCleaner().CleanPadUP(false);
                                if (tmpRetVal != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"Failed to up clean pad");
                                    RetVal = tmpRetVal;
                                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                                    return RetVal;
                                }
                            }
                        }

                        tmpRetVal = this.NeedleCleaner().WaitForCleanPadUp();
                        if (tmpRetVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"Failed to move up clean pad");
                            RetVal = tmpRetVal;
                            //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                            return RetVal;
                        }

                        bFirst = false;
                    }

                    tmpRetVal = DoAutoDetect(ncNum);

                    if (tmpRetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Failed to detect focused height");
                        RetVal = tmpRetVal;
                        //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                        return RetVal;
                    }
                    else
                    {
                        // Set NC height                        
                        RetVal = GetNcHeight(ncNum, out curZ);

                        //TO DO: Remove this line
                        //RetVal = EventCodeEnum.NONE;

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                            return RetVal;
                        }

                        curPos = new NCCoordinate();

                        curPos.X.Value = nPosX;
                        curPos.Y.Value = nPosY;
                        curPos.Z.Value = curZ * -1;     // 센서 높이를 기준으로 -10에서 센싱이 되었다는 뜻은 거꾸로 클린 시트가 10 두꺼워졌다는 의미가 되므로 부호를 뒤집어 주어야 한다.

                        var offsetZ = this.NeedleCleaner().NCHeightProfilingModule.GetPZErrorComp(curPos.X.Value + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value, 
                                                                                                  curPos.Y.Value + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value,
                                                                                                  NC.NCSysParam.SensorPos.Value.Z.Value);

                        curPos.Z.Value += offsetZ;

                        tmpFoucsResult.Add(curPos);

                        LoggerManager.Debug($"Clean sheet focusing result. x = {curPos.X.Value}, y = { curPos.Y.Value}, z = {curPos.Z.Value}, OffsetZ = {offsetZ}", isInfo:true);
                    }

                    // Z down toward clearance
                    //RetVal = NcRelMoveZ(-100);
                    //if (RetVal != EventCodeEnum.NONE)
                    //{
                    //    WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    //    return RetVal;
                    //}
                }

                // Check planarity and update
                minZ = 99999999;
                maxZ = -99999999;
                foreach (NCCoordinate cur_pos in tmpFoucsResult)
                {
                    if (cur_pos.Z.Value <= minZ)
                    { minZ = cur_pos.Z.Value; }
                    if (cur_pos.Z.Value >= maxZ)
                    { maxZ = cur_pos.Z.Value; }
                }

                if (Math.Abs(minZ - maxZ) >= NC.NCSysParam.CleanSheetPlanarityLimit.Value)
                {
                    LoggerManager.Debug($"Clean sheet planarity is over a tolerance. Min = {minZ}, Max = {maxZ}, Tolerance = {NC.NCSysParam.CleanSheetPlanarityLimit.Value}", isInfo: true);
                    RetVal = EventCodeEnum.NEEDLE_CLEANING_PLANARITY_OVER_TOLERANCE;
                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    return RetVal;
                }

                LoggerManager.Debug($"Focusing result Min = {minZ}, Max = {maxZ}, Var = {Math.Abs(minZ) + Math.Abs(maxZ)}", isInfo: true);


                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    tmpFoucsResult[0].Z.Value = tmpFoucsResult[0].GetZ() + (1 * (ncNum + 1));
                    tmpFoucsResult[1].Z.Value = tmpFoucsResult[1].GetZ() + (2 * (ncNum + 1));
                    tmpFoucsResult[2].Z.Value = tmpFoucsResult[2].GetZ() + 66;
                    tmpFoucsResult[3].Z.Value = tmpFoucsResult[3].GetZ() - (3 * (ncNum + 1));
                    tmpFoucsResult[4].Z.Value = tmpFoucsResult[4].GetZ() - (4 * (ncNum + 1));
                }


                // Test Code 
                //if (!File.Exists(dirPath))
                //{
                //    // Create a file to write to.
                //    using (StreamWriter sw = File.CreateText(dirPath))
                //    {
                //        sw.WriteLine("NC test start");
                //    }
                //}
                //FocusedResult1 = tmpFoucsResult[0].GetZ() - 1311;
                //FocusedResult2 = tmpFoucsResult[1].GetZ() - 1311;
                //FocusedResult3 = tmpFoucsResult[2].GetZ() - 1311;
                //FocusedResult4 = tmpFoucsResult[3].GetZ() - 1311;
                //FocusedResult5 = tmpFoucsResult[4].GetZ() - 1311;

                //line = $"{FocusedResult1}, {FocusedResult2}, {FocusedResult3}, {FocusedResult4}, {FocusedResult5}, {(long)SensorTipPos}, {(long)SensorBasePos}";

                //using (StreamWriter sw = File.AppendText(dirPath))
                //{
                //    sw.WriteLine(line);
                //}

                NC.NCSheetVMDefs[ncNum].Heights.Clear();

                foreach (NCCoordinate cur_pos in tmpFoucsResult)
                {
                    NC.NCSheetVMDefs[ncNum].Heights.Add(cur_pos);
                }

                // 측정한 높이 바깥 영역에도 안전을 위해 초기값으로 높이 설정을 해 둔다.
                NCCoordinate BasePlane = new NCCoordinate();
                BasePlane.X.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value - 300000;
                BasePlane.Y.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value + 300000;
                BasePlane.Z.Value = tmpFoucsResult[2].Z.Value;  // 가운데 높이
                NC.NCSheetVMDefs[ncNum].Heights.Add(BasePlane);

                BasePlane.X.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value + 300000;
                BasePlane.Y.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value + 300000;
                BasePlane.Z.Value = tmpFoucsResult[2].Z.Value;  // 가운데 높이
                NC.NCSheetVMDefs[ncNum].Heights.Add(BasePlane);

                BasePlane.X.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value - 300000;
                BasePlane.Y.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value - 300000;
                BasePlane.Z.Value = tmpFoucsResult[2].Z.Value;  // 가운데 높이
                NC.NCSheetVMDefs[ncNum].Heights.Add(BasePlane);

                BasePlane.X.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value + 300000;
                BasePlane.Y.Value = NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value - 300000;
                BasePlane.Z.Value = tmpFoucsResult[2].Z.Value;  // 가운데 높이
                NC.NCSheetVMDefs[ncNum].Heights.Add(BasePlane);
                
                // Save to file
                NC.SaveSysParameter();
                //(this.NeedleCleaner() as IHasSysParameterizable).SaveSysParameter();

                //WriteProLog(EventCodeEnum.CleanPad_Focusing_OK);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.CleanPad_Focusing_OK);

                RetVal = EventCodeEnum.NONE;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - DoCleanUnitFocusing5pt() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
            }

            finally
            {
                // Zdown to safe height
                tmpRetVal = this.NeedleCleaner().CleanPadDown(true);
                if (tmpRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"Failed to move down clean pad");
                    //WriteProLog(EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Focusing_Failure, RetVal);
                    RetVal = tmpRetVal;
                }

                //// Test Code
                //this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);

            }
            return RetVal;
        }
        //bool pingflag = false;
        public EventCodeEnum DoCleanUnitFocusing()
        {
            // 포커싱 메인 함수. 여러 개의 포커싱이 한꺼번에 동작할 경우 맨 처음 위치에서만 Clean unit up을 호출하고
            // 포커싱 완료 후에 down한다. (시간절약)

            EventCodeEnum RetVal = EventCodeEnum.NONE;
            EventCodeEnum tmpRetVal = EventCodeEnum.NONE;

            int ncNum = 1;

            try
            {
                if (IsReadyToCleaning() == false)
                {
                    return EventCodeEnum.NEEDLE_CLEANING_NOT_READY;
                }

                this.StageSupervisor().StageModuleState.ZCLEARED();

                for (ncNum = 0; ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1; ncNum++)
                {
                    if (NC.NCSheetVMDefs[ncNum].FlagRequiredFocus != 0 && NC.NCSheetVMDefs[ncNum].FlagFocusDone == false)
                    {
                        if (NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value == 0 ||
                                NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value == 0)
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                            
                            LoggerManager.Debug($"TimeToFocus() : Range parameter is not set. nc num = {ncNum}");
                        }

                        if (RetVal != EventCodeEnum.NONE)
                        { break; }

                        if (NC.NCSheetVMDefs[ncNum].FlagPlanarityConfirmed == true)
                        {
                            // 이미 5점 포인트 확인을 완료했음. 1포인트(영역의 중심)만 확인하고 높이 보상은 하지 않는다.
                            RetVal = DoCleanUnitFocusing1pt(ncNum);
                        }
                        else
                        {
                            // 머신 이닛 후 아직 한 번도 전체 평탄 확인을 안 했음. 5포인트 포커싱하고 평탄 데이터를 재설정한다.
                            RetVal = DoCleanUnitFocusing5pt(ncNum);
                            if (RetVal == EventCodeEnum.NONE) { NC.NCSheetVMDefs[ncNum].FlagPlanarityConfirmed = true; }
                        }

                        if (this.NeedleCleaner().IsCleanPadDown() == false)
                        {
                            // Zdown to safe height
                            tmpRetVal = this.NeedleCleaner().CleanPadDown(true);
                            if (tmpRetVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"Failed to move down clean pad");
                                RetVal = tmpRetVal;
                                return RetVal;
                            }
                        }

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            //this.NeedleCleanModule.ReasonOfError.Reason = "Clean unit focusing failure";
                            this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Clean unit focusing failure", this.GetType().Name);

                            break;
                        }
                        else
                        {
                            NC.NCSheetVMDefs[ncNum].FlagFocusDone = true;

                            if (this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                NC.NCSheetVMDefs[ncNum].FlagFocusedForCurrentLot = true;
                                NC.NCSheetVMDefs[ncNum].FlagFocusedForCurrentWafer = true;
                            }
                        }
                    }

                }

                //this.StageSupervisor().StageModuleState.ZCLEARED();
                //this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - DoCleanUnitFocusing() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            finally
            {
                tmpRetVal = this.NeedleCleaner().CleanPadDown(true);
                if (tmpRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"Failed to down clean pad");
                    // 시스템 에러 발생시켜야 함. 아직 시스템에서 지원 안 해서 처리 못함.
                    RetVal = tmpRetVal;
                }

                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
            }
            return RetVal;
        }

        #endregion

        public bool IsTimeToFocus(int ncNum)
        {
            // (*) 주의!!!!!
            // 포커싱과 클리닝은 서로 물리적으로 분리되어 서로의 상태를 알 수가 없다.
            // 하지만 이런 상황에서 포커싱 입장에서는 포커싱 인터벌이 '클리닝 할 때마다' 라고 설정되어 있으면
            // 클리닝을 할 때에만 포커싱을 해야 하므로 포커싱보다 시간적으로 뒤에 진행될 클리닝이 수행 될 지 말 지에 대해서
            // 미리 알고 있어야 한다.
            // 따라서 포커싱에서는 클리닝이 이번에 수행 될 지 말지에 대해서 알고 있어야 하며, 이를 결정하는 조건 또한 동일하게
            // 만들어져 있어야 한다. 이점에 반드시 유의할 것!!!!

            try
            {
                // 여기 이 조건들은 클리닝 함수에도 동일하게 들어가 있어야 한다.    
                if (NC.NCSysParam.CleanUnitAttached.Value == true)
                {
                    if (ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1)
                    {
                        if (NeedleCleanerParam.SheetDevs[ncNum].Enabled.Value == true)
                        {
                            if (this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {                                
                                // 우선 클리닝과 상관 없이 포커싱을 해야 할 상황인지 확인한다.
                                if (NeedleCleanerParam.SheetDevs[ncNum].FocusInterval.Value == NC_FocusingInterval.EVERY_LOT_START &&
                                NC.NCSheetVMDefs[ncNum].FlagFocusedForCurrentLot == false)
                                {
                                    return true;
                                }
                                else if (NeedleCleanerParam.SheetDevs[ncNum].FocusInterval.Value == NC_FocusingInterval.EVERY_WAFER_START &&
                                    NC.NCSheetVMDefs[ncNum].FlagFocusedForCurrentWafer == false)
                                {
                                    return true;
                                }
                                else if (NeedleCleanerParam.SheetDevs[ncNum].FocusInterval.Value == NC_FocusingInterval.EVERY_CLEANING)
                                {
                                    // 클리닝 할 때마다 포커싱을 해야 하므로 클리닝 조건을 확인한다.
                                    // 여기 조건은 반드시 클리닝 모듈에 있는 조건과 동일해야 한다. 주의할 것!!!!!
                                    if (this.NeedleCleaner().IsTimeToCleaning(ncNum) == true)
                                    {
                                        return true;
                                    }
                                }                               
                            }
                            else
                            {
                                if (NC.NCSysParam.ManualNC.EnableCleaning.Value[ncNum] == true && NC.NCSysParam.ManualNC.EnableFocus.Value == true)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - TimeToFocus() : Error occured.");
                throw err;

            }
        }

        public EventCodeEnum BeginFocusTask(int ncNum)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                // 여기 이 조건들은 클리닝 함수에도 동일하게 들어가 있어야 한다.    
                if (NC.NCSysParam.CleanUnitAttached.Value == true)
                {
                    if (ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1)
                    {
                        if (NeedleCleanerParam.SheetDevs[ncNum].Enabled.Value == true)
                        {
                            if (NC.NCSheetVMDefs[ncNum].FlagPlanarityConfirmed == true)
                            {
                                NC.NCSheetVMDefs[ncNum].FlagRequiredFocus = 1;
                            }
                            else
                            {
                                NC.NCSheetVMDefs[ncNum].FlagRequiredFocus = 5;
                            }
                        }
                    }
                }
                return RetVal;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - BeginFocusTask() : Error occured.");
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }
        }
        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsParameterChanged(bool issave = false)
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

        public EventCodeEnum DoTouchSensorBaseFocusing()
        {
            // 안전을 위해 머신 이닛 후 1회에 한해 처음 클리닝을 하는 경우 터치 센서가 제자리에 있는지 높이를 확인한다.

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            ICamera icam;
            double offsetZ = 0.0;

            try
            {
                //TO DO: 센서 위치로 이동하기
                LoggerManager.Debug("Begin touch sensor base focusing...");

                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.StageSupervisor().StageModuleState.TouchSensorHighViewMove(NC.NCSysParam.SensorBasePos.Value.X.Value,
                                                                         NC.NCSysParam.SensorBasePos.Value.Y.Value,
                                                                         NC.NCSysParam.SensorBasePos.Value.Z.Value);

                icam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                this.VisionManager().StartGrab(icam.GetChannelType(), this);

                foreach (var light in NC.NCSysParam.LightForBaseFocusSensor)
                {
                    icam.SetLight(light.Type.Value, light.Value.Value);
                }

                TouchSensorBaseFocusModelParam.FocusRange.Value = 150;

                retVal = FocusingFunc();

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("Focusing failed for touch sensor base");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Confirm_Touch_Sensor_Base_Position_Failure, retVal);

                    NC.NCSheetVMDef.FlagTouchSensorBaseConfirmed = false;
                }
                else
                {
                    // 위치 읽기
                    PinCoordinate cur_ccord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();

                    offsetZ = cur_ccord.GetZ() - NC.NCSysParam.SensorBasePos.Value.Z.Value;
                                        
                    NC.NCSysParam.SensorBasePos.Value.Z.Value += offsetZ;
                    NC.NCSysParam.SensorPos.Value.Z.Value += offsetZ;

                    NC.NCSheetVMDef.FlagTouchSensorBaseConfirmed = true;

                    LoggerManager.Debug($"Touch sensor height is updated : Base({NC.NCSysParam.SensorBasePos.Value.Z.Value}), Sensor({NC.NCSysParam.SensorPos.Value.Z.Value}), Offset({offsetZ})", isInfo: true);
                }
                
                this.StageSupervisor().StageModuleState.ZCLEARED();

            }
            catch (Exception err)
            {
                NC.NCSheetVMDef.FlagTouchSensorBaseConfirmed = false;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
