using System;

namespace LoaderCore
{
    using LoaderParameters;
    using LogModule;
    using LoaderBase;
    using LoaderCore.GP_OCRToPreAlignStates;
    using ProberInterfaces.Enum;

    public partial class GP_OCRToPreAlign : ILoaderProcessModule
    {
        public GP_OCRToPreAlignState StateObj { get; set; }

        public TransferProcParam Param { get; private set; }

        public Autofac.IContainer Container { get; private set; }

        public void Init(Autofac.IContainer container, ILoaderProcessParam param)
        {
            try
            {
                this.Container = container;

                this.Param = param as TransferProcParam;

                StateObj = new IdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanExecute(ILoaderProcessParam param)
        {
            bool retval = false;

            try
            {
                var mypa = param as TransferProcParam;

                retval =
                    mypa != null &&
                    mypa.Curr is IOCRReadable &&
                    mypa.Next is IPreAlignModule &&
                    mypa.OCRState == OCRReadStateEnum.DONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
