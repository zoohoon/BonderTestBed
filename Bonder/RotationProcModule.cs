using LogModule;
using ProberInterfaces.Bonder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder
{
    using RotationProcStates;
    public class RotationProcModule : IBonderProcModule
    {
        public BonderModule BonderModule { get; set; }
        public RotationProcModule(BonderModule module)
        {
            this.BonderModule = module;
        }
        public RotationProcState StateObj { get; set; }
        public BonderTransferTypeEnum TransferType => BonderTransferTypeEnum.ROTATING;
        public BonderModeEnum TransferMode => BonderModeEnum.BONDER;
        public BonderTransferProcStateEnum State => StateObj.State;
        public void InitState()
        {
            try
            {
                this.StateObj = new IdleState(this);
            }
            catch (Exception)
            {
                LoggerManager.Debug("[Bonder] RotationProcModule InitState Error");
                throw;
            }
        }
        public void Execute()
        {
            try
            {
                this.StateObj.Execute();
            }
            catch (Exception)
            {
                LoggerManager.Debug("[Bonder] RotationProcModule Execute Error");
                throw;
            }
        }
        public void SelfRecovery()
        {
            try
            {

            }
            catch (Exception)
            {
                LoggerManager.Debug("[Bonder] RotationProcModule SelfRecovery Error");
                throw;
            }
        }
    }
}
