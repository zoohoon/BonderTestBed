using Bonder;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BonderModuleMove
{
    public class BonderMove : IBonderMove
    {
        private IBonderState _BonderStageMove;
        public IBonderState BonderStageMove
        {
            get { return _BonderStageMove; }
            set { _BonderStageMove = value; }
        }
        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    BonderStageMove = new StageIDLEState(this);

                    //this.CoordinateManager().SetPinAxisAs(this.PinViewAxis);
                    // Z축 safe position 이동 추가
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
                throw;
            }

            return retval;
        }
        public void BonderModuleStateTransition(IBonderState state)
        {
            try
            {
                BonderStageMove = state;
            }
            catch
            {
                LoggerManager.Debug("[Bonder] BonderMove. BonderModuleStateTransition() Error");
                throw;
            }
        }
        public BonderStateEnum GetState()
        {
            BonderStateEnum retval = BonderStateEnum.UNKNOWN;

            try
            {
                retval = BonderStageMove.GetState();
            }
            catch
            {
                retval = BonderStateEnum.UNKNOWN;
                throw;
            }
            return retval;
        }
        public EventCodeEnum FDStage_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.FDStage_VacuumOnOff(val, extraVacReady, extraVacOn, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MovePickPosition(double offsetvalue)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MovePickPosition(offsetvalue);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MoveEjPickPosition(double offsetvalue)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MoveEjPickPosition(offsetvalue);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MovePickZMove(bool updown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MovePickZMove(updown);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MoveEjPickZMove(bool updown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MoveEjPickZMove(updown);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MovePinZMove(bool updown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MovePinZMove(updown);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum EjPin_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.EjPin_VacuumOnOff(val, extraVacReady, extraVacOn, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum Arm1_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.Arm1_VacuumOnOff(val, extraVacReady, extraVacOn, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum Arm2_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.Arm2_VacuumOnOff(val, extraVacReady, extraVacOn, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum Arm1_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.Arm1_BlowOnOff(val, extraVacReady, extraVacOn, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum Arm2_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.Arm2_BlowOnOff(val, extraVacReady, extraVacOn, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MoveRotation()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MoveRotation();

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MovePlacePosition(double offsetvalue)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MovePlacePosition(offsetvalue);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MovePlaceZMove(bool updown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MovePlaceZMove(updown);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MagneticOnOff(bool val, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MagneticOnOff(val, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum MoveNanoZMove(bool updown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.MoveNanoZMove(updown);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum Arm1_AirOnOff(bool val, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.Arm1_AirOnOff(val, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
        public EventCodeEnum Arm2_AirOnOff(bool val, long timeout = 0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = BonderStageMove.Arm2_AirOnOff(val, timeout);

                retval = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return retval;
        }
    }

    public abstract class BonderState : IBonderState
    {
        private BonderMove _Module;

        public BonderMove Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }
        public BonderState(BonderMove module)
        {
            Module = module;
        }

        #region => Interface
        public override BonderStateEnum GetState()
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : GetState Error.");
            return BonderStateEnum.UNKNOWN;
        }
        public override EventCodeEnum MovePickPosition(double offsetvalue)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MovePickPosition Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MoveEjPickPosition(double offsetvalue)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MoveEjPickPosition Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MovePickZMove(bool updown)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MovePickZMove Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MoveEjPickZMove(bool updown)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MoveEjPickZMove Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MovePinZMove(bool updown)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MovePinZMove Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum EjPin_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : EjPin_VacuumOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum FDStage_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : FDStage_VacuumOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum Arm1_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : Arm1_VacuumOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum Arm2_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : Arm2_VacuumOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum Arm1_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : Arm1_BlowOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum Arm2_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : Arm2_BlowOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MoveRotation()
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MoveRotation Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MovePlacePosition(double offsetvalue)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MovePlacePosition Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MagneticOnOff(bool val, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MagneticOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MoveNanoZMove(bool updown)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MoveNanoZMove Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum Arm1_AirOnOff(bool val, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : Arm1_AirOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum Arm2_AirOnOff(bool val, long timeout = 0)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : Arm2_AirOnOff Error.");
            return EventCodeEnum.BonderError;
        }
        public override EventCodeEnum MovePlaceZMove(bool updown)
        {
            LoggerManager.Error("[Bonder] BonderMove.cs : MovePlaceZMove Error.");
            return EventCodeEnum.BonderError;
        }
        #endregion

        protected EventCodeEnum CheckStage()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                // check stage code 추가
                //ret = CheckTriLeg();
                //ret = CheckRotateAxis();

                // 본더의 경우 Z 높이만 주의하면 될듯
                LoggerManager.Debug("[Bonder] (BonderStage) check = Ejection Z Safe position?");

                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum ResultValidate(object funcname, EventCodeEnum retcode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            ret = retcode;

            if (retcode != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"ResultValidate Fail :  Error code = {retcode.ToString()}, fucntion name = {funcname.ToString()}");

                throw new Exception($"FunctionName: {funcname.ToString()} Returncode: {retcode.ToString()} Error occurred");
            }

            return ret;
        }
        protected EventCodeEnum MoveStageSafePos(bool zval, bool pzval, bool wafercamval, bool isDoMark = true)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            int retVal = -1;

            try
            {
                // 삼발이체크 , Z 위치 체크
                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum MovePickPositionFunc(double offsetvalue)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var xaxis = Module.MotionManager().GetAxis(EnumAxisConstants.X);
                var yaxis = Module.MotionManager().GetAxis(EnumAxisConstants.Y);
                var zaxis = Module.MotionManager().GetAxis(EnumAxisConstants.Z);
                var taxis = Module.MotionManager().GetAxis(EnumAxisConstants.C);
                var axisFDz = Module.MotionManager().GetAxis(EnumAxisConstants.NZD1);  // = FD Stage Z

                double curZpos = 0.0;
                double curPZ = 0;

                double zoffset = offsetvalue;
                double xpos = Module.CoordinateManager().StageCoord.ChuckLoadingPosition.X.Value;
                double ypos = Module.CoordinateManager().StageCoord.ChuckLoadingPosition.Y.Value;
                double zpos = Module.CoordinateManager().StageCoord.ChuckLoadingPosition.Z.Value;


                LoggerManager.Debug("[Bonder] (BonderStage) move = FD Stage X,Y,T");
                LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = FD Stage");

                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum MoveEjPickPositionFunc(double offsetvalue)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug("[Bonder] (BonderStage) move = Ejection XY");
                LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Ejection");

                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum MovePickZMoveFunc(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (updown)  // up
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = FD Stage Z Up");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = FD Stage");
                }
                else // down
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = FD Stage Z Down");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = FD Stage");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum MoveEjPickZMoveFunc(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (updown)  // up
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = Ejection Z up");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Ejection");
                }
                else // down
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = Ejection Z down");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Ejection");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum MovePinZMoveFunc(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if(updown)  // up
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = Ejection Pin Z up");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Ejection");
                }
                else  // down
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = Ejection Pin Z down");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Ejection");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum EjPin_VacuumOnOffFunc(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // vacuum on
                {
                    LoggerManager.Debug("[Bonder] (Pick) Ejection Pin = Vacuum On");
                    LoggerManager.Debug("[Bonder] (Pick) check = Vacuum pressure OK?");
                }
                else  // vacuum off
                {
                    LoggerManager.Debug("[Bonder] (Stage) Ejection Pin = Vacuum Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum Arm1_AirOnOffFunc(bool val, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // air on
                {
                    LoggerManager.Debug("[Bonder] (Pick) or (RotationOnly) Arm1 = Air On");
                }
                else  // air off
                {
                    LoggerManager.Debug("[Bonder] (Place) or (PickOnly) Arm1 = Air Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum Arm2_AirOnOffFunc(bool val, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // air on
                {
                    LoggerManager.Debug("[Bonder] (Pick) or (RotationOnly) Arm2 = Air On");
                }
                else  // air off
                {
                    LoggerManager.Debug("[Bonder] (Place) or (PickOnly) Arm2 = Air Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum Arm1_VacuumOnOffFunc(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // vacuum on
                {
                    LoggerManager.Debug("[Bonder] (Pick) ARM1 = Vacuum On");
                    LoggerManager.Debug("[Bonder] (Pick) check = Vacuum pressure OK?");
                }
                else  // vacuum off
                {
                    LoggerManager.Debug("[Bonder] (Place) Arm1 = Vacuum Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum Arm2_VacuumOnOffFunc(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // vacuum on
                {
                    LoggerManager.Debug("[Bonder] (Pick) ARM2 = Vacuum On");
                    LoggerManager.Debug("[Bonder] (Pick) check = Vacuum pressure OK?");
                }
                else  // vacuum off
                {
                    LoggerManager.Debug("[Bonder] Picker2 = Vacuum Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum MoveRotationFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug("[Bonder] (Rotation) move = Rotation Move");
                LoggerManager.Debug("[Bonder] (Rotation) MotionDone = Rotation");

                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum MovePlacePositionFunc(double offsetvalue)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug("[Bonder] (BonderStage) move = Wafer Stage X, Y, T");
                LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Wafer Stage");

                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum MovePlaceZMoveFunc(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if(updown)
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = Wafer Stage Z Up");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Wafer Stage");
                }
                else
                {
                    LoggerManager.Debug("[Bonder] (BonderStage) move = Wafer Stage Z Down");
                    LoggerManager.Debug("[Bonder] (BonderStage) MotionDone = Wafer Stage");
                }

                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum MagneticOnOffFunc(bool val, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // Magnetic on
                {
                    LoggerManager.Debug("[Bonder] (Place) Bearing = Magnetic On");
                }
                else  // Magnetic off
                {
                    LoggerManager.Debug("[Bonder] (Place) Bearing = Magnetic Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum MoveNanoZMoveFunc(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (updown)  // up
                {
                    LoggerManager.Debug("[Bonder] (Place) move = Nano Stage Z Up");
                    LoggerManager.Debug("[Bonder] (Place) MotionDone = Nano Stage");
                }
                else  // down
                {
                    LoggerManager.Debug("[Bonder] (Place) move = Nano Stage Z Down");
                    LoggerManager.Debug("[Bonder] (Place) MotionDone = Nano Stage");
                }

                ret = EventCodeEnum.NONE;
            }
            catch
            {

            }
            return ret;
        }
        protected EventCodeEnum Arm1_BlowOnOffFunc(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // vacuum on
                {
                    LoggerManager.Debug("[Bonder] (Place) ARM1 Blow On");
                }
                else  // vacuum off
                {
                    LoggerManager.Debug("[Bonder] (Place) ARM1 Blow Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum Arm2_BlowOnOffFunc(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // vacuum on
                {
                    LoggerManager.Debug("[Bonder] Placer2 Vacuum On");
                }
                else  // vacuum off
                {
                    LoggerManager.Debug("[Bonder] (Place) ARM1 Vacuum Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
        protected EventCodeEnum BlowOnOffFunc(bool val, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (val == true)    // Blow on
                {
                    LoggerManager.Debug("[Bonder] (Place) Bearing = Blow On");
                }
                else  // Blow off
                {
                    LoggerManager.Debug("[Bonder] (Place) Bearing = Blow Off");
                }
                ret = EventCodeEnum.NONE;
            }
            catch
            {
                throw;
            }
            return ret;
        }
    }

    public class StageIDLEState : BonderState
    {
        // Ejection Z up 제외
        BonderModule bonderModule = ShareInstance.bonderModule;

        public StageIDLEState(BonderMove module) : base(module) 
        {
            LoggerManager.Debug("[Bonder] Bonder Move State = IDLE");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.IDLE;
        }

        // FDStage_VacuumOnOff 추가 필요

        public override EventCodeEnum MovePickPosition(double offsetvalue)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = CheckStage();
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = MoveStageSafePos(true, true, true, false);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = MovePickPositionFunc(offsetvalue);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new StageMoveToPickPosition(Module));
            }
            catch
            {

            }
            return ret;
        }
    }

    public class StageMoveToPickPosition : BonderState
    {
        // FD Stage와 Ejection의 XY 이동에 대한 것
        // X , Y 이동 시 충돌 주의
        // Z up은 제외

        public StageMoveToPickPosition(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = StageMoveToPickPosition");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.MOVETOPICKPOS;
        }

        //  MovePickRotation 추가 필요

        public override EventCodeEnum MoveEjPickPosition(double offsetvalue)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = CheckStage();
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = MoveStageSafePos(true, true, true, false);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = MoveEjPickPositionFunc(offsetvalue);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new StageMoveToZUp(Module));
            }
            catch
            {
                throw;
            }
            return ret;
        }
    }

    public class StageMoveToZUp : BonderState
    {
        // FD Stage 및 Ejection Z up만 해당
        public StageMoveToZUp(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = StageMoveToZUp");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.PICKZUP;
        }
        public override EventCodeEnum MovePickZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MovePickZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum MoveEjPickZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MoveEjPickZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new StageMoveToPinZUp(Module));
            }
            catch
            {
                throw;
            }
            return ret;
        }
    }

    public class StageMoveToPinZUp : BonderState
    {
        // Pick 직전단계로 Ejection Pin Z up과 Pin Vacuum만 해당
        public StageMoveToPinZUp(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = StageMoveToPinZUp");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.PICKZUP;
        }
        public override EventCodeEnum MovePinZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MovePinZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum EjPin_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Pin Vacuum On
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = EjPin_VacuumOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new PickingState(Module));
            }
            catch
            {

            }
            return ret;
        }
    }
    public class PickingState : BonderState
    {
        // Pick , Air On , Vac On 하는 것만 해당
        BonderModule bonderModule = ShareInstance.bonderModule;

        public PickingState(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = Picking");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.PICKING;
        }
        public override EventCodeEnum Arm1_AirOnOff(bool val, long timeout = 0)
        {
            // Arm1 Air On
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm1_AirOnOffFunc(val, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm2_AirOnOff(bool val, long timeout = 0)
        {
            // Arm Air On
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm2_AirOnOffFunc(val, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm1_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Vacuum On
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm1_VacuumOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new StageMoveToPinZDown(Module));
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm2_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Vacuum On
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm2_VacuumOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new StageMoveToPinZDown(Module));
            }
            catch
            {

            }
            return ret;
        }
    }

    public class StageMoveToPinZDown : BonderState
    {
        // Ejection Pin Z down , Pin Vacuum Off 만 해당
        BonderModule bonderModule = ShareInstance.bonderModule;

        public StageMoveToPinZDown(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = StageMoveToPinZDown");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.PICKZDOWN;
        }
        public override EventCodeEnum EjPin_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Pin Vacuum Off
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = EjPin_VacuumOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum MovePinZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MovePinZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new StageMoveToZDown(Module));
            }
            catch
            {

            }
            return ret;
        }
    }

    public class StageMoveToZDown : BonderState
    {
        // FD Stage 및 Ejection Z up만 해당
        BonderModule bonderModule = ShareInstance.bonderModule;

        public StageMoveToZDown(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = StageMoveToZDown");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.PICKZDOWN;
        }
        public override EventCodeEnum MoveEjPickZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MoveEjPickZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public override EventCodeEnum MovePickZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MovePickZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new RotatingState(Module));
            }
            catch
            {

            }
            return ret;
        }        
    }

    public class RotatingState : BonderState
    {
        // Rotation 하는 것만 해당
        BonderModule bonderModule = ShareInstance.bonderModule;

        public RotatingState(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = Rotating");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.ROTATING;
        }
        public override EventCodeEnum MoveRotation()
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MoveRotationFunc();
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm1_AirOnOff(bool val, long timeout = 0)
        {
            // Air On
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm1_AirOnOffFunc(val, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                if (val == false)
                {
                    Module.BonderModuleStateTransition(new StageMoveToPlacePosition(Module));
                }
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm2_AirOnOff(bool val, long timeout = 0)
        {
            // Air On
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm2_AirOnOffFunc(val, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                if(val == false)
                {
                    Module.BonderModuleStateTransition(new StageMoveToPlacePosition(Module));
                }
            }
            catch
            {

            }
            return ret;
        }
    }

    public class StageMoveToPlacePosition : BonderState
    {
        // Rotation 후 Wafer Stage가 XY 이동하는 것만 해당

        public StageMoveToPlacePosition(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = StageMoveToPlacePosition");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.MOVETOPLACEPOS;
        }
        public override EventCodeEnum MovePlacePosition(double offsetvalue)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = CheckStage();
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = MoveStageSafePos(true, true, true, false);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = MovePlacePositionFunc(offsetvalue);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum MovePlaceZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MovePlaceZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Module.BonderModuleStateTransition(new PlacingState(Module));
            }
            catch
            {

            }
            return ret;
        }
    }

    public class PlacingState : BonderState
    {
        // Place 하는 것만 해당
        // Die Align을 포함할지 별도로할지 미정
        BonderModule bonderModule = ShareInstance.bonderModule;

        public PlacingState(BonderMove module) : base(module)
        {
            LoggerManager.Debug(" ");
            LoggerManager.Debug("[Bonder] Bonder Move State = Placing");
        }
        public override BonderStateEnum GetState()
        {
            return BonderStateEnum.PLACING;
        }
        public override EventCodeEnum MagneticOnOff(bool val, long timeout = 0)
        {
            // FD Stage Vacuum
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MagneticOnOffFunc(val, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                // 마그네틱 OFF가 Placing 가장 마지막 단계. 아닐경우 아래 상태변경 내용을 옮겨야 함
                if (val == false)
                {
                    // Module.BonderModuleStateTransition(new StageMoveToPickPosition(Module)); // Place 후 다이가 더 있어서 Pick하는 경우
                    Module.BonderModuleStateTransition(new StageIDLEState(Module));
                }
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm1_AirOnOff(bool val, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm1_AirOnOffFunc(val, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm2_AirOnOff(bool val, long timeout = 0)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm2_AirOnOffFunc(val, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum MoveNanoZMove(bool updown)
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = MoveNanoZMoveFunc(updown);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }

        public override EventCodeEnum Arm1_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Vacuum Off
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm1_VacuumOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm2_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Vacuum Off
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm2_VacuumOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);


                Module.BonderModuleStateTransition(new StageMoveToPinZDown(Module));
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm1_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Place Blow On / Off
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm1_BlowOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
        public override EventCodeEnum Arm2_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            // Place Blow On / Off
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                ret = Arm2_BlowOnOffFunc(val, extraVacReady, extraVacOn, timeout);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch
            {

            }
            return ret;
        }
    }

    public class StageErrorState
    {

    }

    public class FDHighViewState
    {
        // FD Align할 때 하이 카메라로 이동 및 그 외 움직임 관련
    }

    public class FDLowViewState
    {
        // FD Align할 때 로우 카메라로 이동 및 그 외 움직임 관련
    }

    public class StageManualState
    {

    }

    public class StageLockState
    {

    }
}
