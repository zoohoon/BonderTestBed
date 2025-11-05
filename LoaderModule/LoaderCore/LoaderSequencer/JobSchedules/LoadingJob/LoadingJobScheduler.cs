using System;

using LoaderBase;
using ProberInterfaces;

namespace LoaderCore
{
    using LoadingJobStates;
    using LogModule;

    public class LoadingJob : ILoaderJob
    {
        public int Priority => 75;

        public LoadingJobStateBase StateObj { get; set; }

        public ILoaderModule Loader { get; private set; }

        public TransferObject TransferObject { get; set; }

        public IWaferOwnable CurrHolder => Loader.ModuleManager.FindModule<IWaferOwnable>(TransferObject.CurrHolder);

        public IWaferLocatable CurrPos => Loader.ModuleManager.FindModule<IWaferLocatable>(TransferObject.CurrPos);

        public IChuckModule Destination { get; set; }

        public void Init(ILoaderModule loader, TransferObject transferObject, IChuckModule destination)
        {
            try
            {
                this.Loader = loader;
                this.TransferObject = transferObject;
                this.Destination = destination;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
                    case ModuleTypeEnum.SLOT:
                        StateObj = new OnSLOT(this);
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        StateObj = new OnFixedTray(this);
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        StateObj = new OnInspectionTray(this);
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
                    case ModuleTypeEnum.CHUCK:
                        StateObj = new OnChuck(this);
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
