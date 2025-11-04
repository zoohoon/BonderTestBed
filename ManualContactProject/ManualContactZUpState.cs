using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ManualContact
{
    public abstract class ManualContactStateBase : INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        protected ManualContactModule ManualContactModule { get; set; }
        protected IMotionManager MotionManager { get; set; }
        //protected IProberStation        ProberStation       { get; set; }
        protected IWaferAligner WaferAligner { get; set; }
        protected ICoordinateManager CoordinateManager { get; set; }
        protected IStageSupervisor StageSupervisor { get; set; }

        private bool _IsZUpState;
        public bool IsZUpState
        {
            get { return _IsZUpState; }
            set
            {
                if (value != _IsZUpState)
                {
                    _IsZUpState = value;

                    ManualContactModule.IsZUpState = _IsZUpState;

                    NotifyPropertyChanged();
                }
            }
        }

        private string _StateString;
        public string StateString
        {
            get { return _StateString; }
            set
            {
                _StateString = value;
                NotifyPropertyChanged();
            }
        }

        internal abstract bool IsCanMoveStage();

        public ManualContactStateBase(ManualContactModule module)
        {
            try
            {
                ManualContactModule = module;
                MotionManager = this.MotionManager();
                //ProberStation = this.ProberStation();
                WaferAligner = this.WaferAligner();
                CoordinateManager = this.CoordinateManager();
                StageSupervisor = this.StageSupervisor();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public virtual EventCodeEnum ZUp(System.Windows.Point MXYIndex, double OverDrive)
            => EventCodeEnum.UNDEFINED;

        public virtual EventCodeEnum ZDown(System.Windows.Point MXYIndex, double OverDrive)
            => EventCodeEnum.UNDEFINED;

        public abstract EventCodeEnum MovePadToPin(WaferCoordinate waferCoordinate, PinCoordinate pinCoordinate, double zc);
    }

    public class ManualContactZUp : ManualContactStateBase
    {
        public ManualContactZUp(ManualContactModule module) : base(module)
        {
            try
            {
                IsZUpState = true;
                StateString = "ZUP";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        internal override bool IsCanMoveStage()
            => false;

        public override EventCodeEnum ZDown(System.Windows.Point MXYIndex, double OverDrive)
        {
            EventCodeEnum stageErrorCode = EventCodeEnum.UNDEFINED;
            WaferCoordinate waferCoordinate = null;
            PinCoordinate pinCoordinate = new PinCoordinate();
            ProbeAxisObject axisZ = this.MotionManager.GetAxis(EnumAxisConstants.Z);
            //MachineCoordinate moveCoordinate = null;
            IMotionManager MotionManager = this.MotionManager();
            double od = this.ProbingModule().OverDrive;
            double zc = this.ProbingModule().ZClearence;
            zc = this.ProbingModule().CalculateZClearenceUsingOD(od, zc);


            //waferCoordinate = this.WaferAligner.MIndexToWPos((int)MXIndex, (int)MYIndex, true);
            waferCoordinate = this.WaferAligner.MachineIndexConvertToProbingCoord((int)MXYIndex.X, (int)MXYIndex.Y);
            
            

            pinCoordinate.X.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
            pinCoordinate.Y.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
            pinCoordinate.Z.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

            //waferCoordinate.X.Value = -15018.2;
            //waferCoordinate.Y.Value = -17304.6;
            //waferCoordinate.Z.Value = 1578.0;
            //pinCoordinate.X.Value = -3863.3;
            //pinCoordinate.Y.Value = -11945.4;
            //pinCoordinate.Z.Value = -9399.0;
            try
            {
                //LoggerManager.Exception(new Exception("당황하지마 Jake. 장비에서 짚고 넘어가라고 일부러 걸어 놓은거야."));

                //var axispz = MotionManager.GetAxis(EnumAxisConstants.PZ);
                //moveCoordinate = new MachineCoordinate(0, 0, axispz.Param.ClearedPosition.Value);
                //var coord = this.CoordinateManager().PinHighPinConvert.Convert(moveCoordinate);
                //pinCoordinate.Z.Value = coord.Z.Value;

                //var axisz = MotionManager.GetAxis(EnumAxisConstants.Z);
                //moveCoordinate = new MachineCoordinate(0, 0, axisz.Param.ClearedPosition.Value);
                //var wcoord = CoordinateManager.WaferHighChuckConvert.Convert(moveCoordinate);
                //waferCoordinate.Z.Value = wcoord.Z.Value;

                this.GetForceMeasure().ResetMeasurement();

                stageErrorCode = StageSupervisor.StageModuleState.ProbingZDOWN(
                    waferCoordinate,
                    pinCoordinate,
                    OverDrive, zc);

                if (stageErrorCode != EventCodeEnum.NONE)
                {
                    throw new Exception(stageErrorCode.ToString());
                }
                else
                {
                    //this.GetForceMeasure().ResetMeasurement();
                    this.GetForceMeasure().MeasureProbingForce();
                    //one more
                    this.GetForceMeasure().ResetMeasurement();
                    this.GetForceMeasure().MeasureProbingForce();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                stageErrorCode = EventCodeEnum.UNDEFINED;
                //throw new Exception(e.Message);
            }

            return stageErrorCode;
        }

        public override EventCodeEnum MovePadToPin(WaferCoordinate waferCoordinate, PinCoordinate pinCoordinate, double zc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = StageSupervisor.StageModuleState.ProbingZDOWN(waferCoordinate, pinCoordinate, this.ManualContactModule().OverDrive, zc);

                if (retVal == EventCodeEnum.NONE)
                {
                    ManualContactModule.ManualContactZAxisStateTransition(new ManualContactZDown(ManualContactModule));
                    retVal = StageSupervisor.StageModuleState.MovePadToPin(waferCoordinate, pinCoordinate, zc);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class ManualContactZDown : ManualContactStateBase
    {
        public ManualContactZDown(ManualContactModule module) : base(module)
        {
            try
            {
                IsZUpState = false;
                StateString = "ZDN";

                MotionManager = this.MotionManager();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        internal override bool IsCanMoveStage()
            => true;

        public override EventCodeEnum ZUp(System.Windows.Point MXYIndex, double OverDrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            WaferCoordinate waferCoordinate = null;
            PinCoordinate pinCoordinate = new PinCoordinate();
            ProbeAxisObject axisZ = MotionManager.GetAxis(EnumAxisConstants.Z);
            MachineCoordinate moveCoordinate = null;

            //waferCoordinate = WaferAligner.MachineIndexConvertToProbingCoord((int)MXYIndex.X, (int)MXYIndex.Y);
            waferCoordinate = WaferAligner.MachineIndexConvertToProbingCoord((int)MXYIndex.X, (int)MXYIndex.Y);
            pinCoordinate.X.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
            pinCoordinate.Y.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
            pinCoordinate.Z.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

            try
            {
                AlignStateEnum pinAlignState = this.GetParam_ProbeCard().GetAlignState();
                AlignStateEnum waferAlignState = StageSupervisor.WaferObject.GetAlignState();
                AlignStateEnum pinPadMatchAlignState = this.GetParam_ProbeCard().GetPinPadAlignState();

                if (!(pinAlignState == AlignStateEnum.DONE
                    && waferAlignState == AlignStateEnum.DONE
                    && pinPadMatchAlignState == AlignStateEnum.DONE))
                {
                    var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                    moveCoordinate = new MachineCoordinate(0, 0, axispz.Param.ClearedPosition.Value);
                    var coord = this.CoordinateManager().PinHighPinConvert.Convert(moveCoordinate);
                    pinCoordinate.Z.Value = coord.Z.Value;

                    var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    moveCoordinate = new MachineCoordinate(0, 0, axisz.Param.ClearedPosition.Value);
                    var wcoord = this.CoordinateManager().WaferHighChuckConvert.Convert(moveCoordinate);
                    waferCoordinate.Z.Value = wcoord.Z.Value;
                }

                this.GetForceMeasure().ResetMeasurement();

                retVal = StageSupervisor.StageModuleState.ProbingZUP(
                            waferCoordinate,
                            pinCoordinate,
                            OverDrive);

                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception(retVal.ToString());
                }
                else
                {
                    //this.GetForceMeasure().ResetMeasurement();
                    this.GetForceMeasure().MeasureProbingForce();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
                //throw new Exception(e.Message);
            }

            return retVal;
        }

        public override EventCodeEnum MovePadToPin(WaferCoordinate waferCoordinate, PinCoordinate pinCoordinate, double zc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = StageSupervisor.StageModuleState.MovePadToPin(waferCoordinate, pinCoordinate, zc);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
