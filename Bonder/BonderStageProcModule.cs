using LogModule;
using ProberInterfaces.Bonder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder
{
    using BonderStageProcStates;
    public class BonderStageProcModule : IBonderProcModule
    {
        public BonderModule BonderModule { get; set; }
        public BonderStageProcState StateObj { get; set; }
        public BonderTransferTypeEnum TransferType => BonderTransferTypeEnum.STAGE;

        public BonderModeEnum TransferMode => BonderModeEnum.BONDER;

        public BonderTransferProcStateEnum State => StateObj.State;
        public BonderStageProcModule(BonderModule module)
        {
            this.BonderModule = module;
        }
        public void InitState()
        {
            try
            {
                this.StateObj = new IdleState(this);
            }
            catch (Exception)
            {
                LoggerManager.Debug("[Bonder] BonderStageProcModule InitState Error");
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
                LoggerManager.Debug("[Bonder] BonderStageProcModule Execute Error");
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
                LoggerManager.Debug("[Bonder] BonderStageProcModule SelfRecovery Error");
                throw;
            }
        }
    }
}
