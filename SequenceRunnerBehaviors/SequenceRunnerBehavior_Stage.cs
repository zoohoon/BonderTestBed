using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.SequenceRunner;
using SequenceRunner;
using System;
using System.Threading.Tasks;

namespace SequenceRunnerBehaviors
{
    [Serializable]
    public class ZDownToClearence : SequenceBehavior
    {
        public ZDownToClearence()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.ZDownToClearence;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IStageSupervisor stageSupervisor = this.StageSupervisor();
                stageSupervisor.StageModuleState.ZCLEARED();

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class StageMoveToBackPosition : SequenceBehavior
    {
        public StageMoveToBackPosition()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.StageMoveToBackPosition;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IStageSupervisor stageSupervisor = this.StageSupervisor();
                EventCodeEnum moveResult = stageSupervisor.StageModuleState.MoveToBackPosition();
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class StageMoveToNcPadChangePosition : SequenceBehavior
    {
        public StageMoveToNcPadChangePosition()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.StageMoveToNcPadChangePosition;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IStageSupervisor stageSupervisor = this.StageSupervisor();
                EventCodeEnum moveResult = stageSupervisor.StageModuleState.MoveToNcPadChangePosition();
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class StageMoveToCenterPosition : SequenceBehavior
    {
        public StageMoveToCenterPosition()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.StageMoveToCenterPosition;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IStageSupervisor stageSupervisor = this.StageSupervisor();

                AxisObject zAxis = this.MotionManager().GetAxis(stageSupervisor.StageModuleState.PinViewAxis);
                MachineCoordinate mc = new MachineCoordinate(0, 0, zAxis.Param.HomeOffset.Value);
                var pincoord = this.CoordinateManager().PinHighPinConvert.Convert(mc);

                retVal.ErrorCode = stageSupervisor.StageModuleState.PinHighViewMove(pincoord.X.Value, pincoord.Y.Value, pincoord.Z.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
}
