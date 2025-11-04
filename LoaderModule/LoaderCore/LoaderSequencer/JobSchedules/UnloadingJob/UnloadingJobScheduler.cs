using System;

using LoaderBase;
namespace LoaderCore
{
    using LogModule;
    using ProberInterfaces;
    using UnloadingJobStates;

    public class UnloadingJob : ILoaderJob
    {
        public int Priority => 50;

        public UnloadingJobSchedulerStateBase StateObj { get; set; }

        public ILoaderModule Loader { get; private set; }

        public TransferObject TransferObject { get; set; }

        public IWaferOwnable CurrHolder => Loader.ModuleManager.FindModule<IWaferOwnable>(TransferObject.CurrHolder);

        public IWaferLocatable CurrPos => Loader.ModuleManager.FindModule<IWaferLocatable>(TransferObject.CurrPos);

        public IWaferSupplyModule Destination { get; set; }

        public void Init(ILoaderModule loader, TransferObject transferObject, IWaferSupplyModule destination)
        {
            this.Loader = loader;
            this.TransferObject = transferObject;
            this.Destination = destination;
        }

        public JobValidateResult Validate()
        {
            JobValidateResult result = new JobValidateResult();
            try
            {
                InitState();
                result = StateObj.Validate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }

        public JobScheduleResult DoSchedule()
        {
            JobScheduleResult result = new JobScheduleResult();
            try
            {
                InitState();
                result = StateObj.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }

        private void InitState()
        {
            try
            {
                switch (TransferObject.CurrPos.ModuleType)
                {
                    case ModuleTypeEnum.CHUCK:
                        StateObj = new OnCHUCK(this);
                        break;
                    case ModuleTypeEnum.ARM:
                        StateObj = new OnARM(this);
                        break;
                    case ModuleTypeEnum.PA:
                        StateObj = new OnPreAlign(this);
                        break;
                    case ModuleTypeEnum.SEMICSOCR:
                    case ModuleTypeEnum.COGNEXOCR:
                        if (Loader.Move.State == LoaderMoveStateEnum.OCR)
                        {
                            StateObj = new OnOCR(this);
                        }
                        else
                        {
                            StateObj = new OnARM(this);
                        }
                        break;
                    case ModuleTypeEnum.SLOT:
                        StateObj = new OnSLOT(this);
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        StateObj = new OnFixedTray(this);
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        StateObj = new OnInspectionTray(this);
                        break;

                    default:
                        throw new AccessViolationException();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
