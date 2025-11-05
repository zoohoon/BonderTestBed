using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.SequenceRunner;
using SequenceRunner;
using System;
using System.Threading.Tasks;

namespace SequenceRunnerBehaviors
{
    [Serializable]
    public class WaitForUserWorking : SequenceBehavior
    {
        public WaitForUserWorking()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.WaitForUserWorking;
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

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", "When you are finished working, click the bottom right button.", EnumMessageStyle.Affirmative);

                retVal.ErrorCode = EventCodeEnum.NONE;
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
