using System;

namespace LoaderCore
{
    using LoaderParameters;
    using LogModule;
    using LoaderBase;
    using LoaderCore.GP_CardBufferToCardARMStates;

    public partial class GP_CardBufferToCardARM : ILoaderProcessModule
    {
        public GP_CardBufferToCardARMState StateObj { get; set; }

        public CardTransferProcParam Param { get; private set; }

        public Autofac.IContainer Container { get; private set; }

        public void Init(Autofac.IContainer container, ILoaderProcessParam param)
        {
            try
            {
                this.Container = container;

                this.Param = param as CardTransferProcParam;

                StateObj = new IdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanExecute(ILoaderProcessParam param)
        {
            var mypa = param as CardTransferProcParam;

            return
                mypa != null &&
                mypa.Curr is ICardBufferModule &&
                mypa.Next is ICardARMModule &&
                mypa.UseARM is ICardARMModule;
        }

        public LoaderProcStateEnum State => StateObj.State;

        public ReasonOfSuspendedEnum ReasonOfSuspended => StateObj.ReasonOfSuspended;

        public void Execute()
        {
            try
            {

                StateObj.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Awake()
        {
            try
            {
                StateObj.Resume();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SelfRecovery()
        {
            try
            {
                StateObj.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


    }
}
