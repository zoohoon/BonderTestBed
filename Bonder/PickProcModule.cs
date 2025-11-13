using ProberInterfaces.Bonder;
using System;
using LogModule;

namespace Bonder
{
    using PickProcStates;
    public class PickProcModule : IBonderProcModule
    {
        public BonderModule BonderModule { get; set; }
        public PickProcModule(BonderModule module)
        {
            this.BonderModule = module;
        }
        public PickProcState StateObj { get; set; }
        public BonderTransferTypeEnum TransferType => BonderTransferTypeEnum.PICKING;

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
                LoggerManager.Debug("[Bonder] PickProcModule InitState Error");
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
                LoggerManager.Debug("[Bonder] PickProcModule Execute Error");
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
                LoggerManager.Debug("[Bonder] PickProcModule SelfRecovery Error");
                throw;
            }
        }
    }
}
