using ProberInterfaces.SequenceRunner;
using System.Threading.Tasks;

namespace SequenceRunnerModule
{
    public abstract class SequenceRunnerExecuteController
    {
        public abstract void Run(SequenceRunner module, SequenceRunnerState srState);
    }

    public class SequenceRunnerExecuteController_Idle : SequenceRunnerExecuteController
    {
        public override void Run(SequenceRunner module, SequenceRunnerState srState)
        {
            module.IsExecutting = true;
            module.SequenceRunnerExecuteCtrl = module.executeRunning;
            Task.Run(async () => await srState.TaskExecute()).ContinueWith((tast) => {
                module.IsExecutting = false;
            });
        }
    }

    public class SequenceRunnerExecuteController_Running : SequenceRunnerExecuteController
    {
        public override void Run(SequenceRunner module, SequenceRunnerState srState)
        {
            if (module.IsExecutting == false)
            {
                module.SequenceRunnerExecuteCtrl = module.executeIdle;
            }
        }
    }
}
