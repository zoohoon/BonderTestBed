using LogModule;
using ProberInterfaces.Bonder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder
{
    using PlaceProcStates;
    public class PlaceProcModule : IBonderProcModule
    {
        public BonderModule BonderModule { get; set; }
        public PlaceProcModule(BonderModule module)
        {
            this.BonderModule = module;
        }
        public PlaceProcState StateObj { get; set; }
        public BonderTransferTypeEnum TransferType => BonderTransferTypeEnum.PLACING;

        public BonderModeEnum TransferMode => BonderModeEnum.BONDER;

        public BonderTransferProcStateEnum State => StateObj.State;
        public void InitState()
        {
            try
            {
                this.StateObj = new IdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug("[Bonder] PlaceProcModule InitState Error");
                throw;
            }
        }
        public void Execute()
        {
            try
            {
                this.StateObj.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug("[Bonder] PlaceProcModule Execute Error");
                throw;
            }
        }
        public void SelfRecovery()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug("[Bonder] PlaceProcModule SelfRecovery Error");
                throw;
            }
        }
    }
}
