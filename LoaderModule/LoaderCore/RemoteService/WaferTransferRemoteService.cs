using System;

using ProberErrorCode;
using LoaderBase;
using LoaderParameters;
using ProberInterfaces;
using Autofac;
using LogModule;

namespace LoaderCore
{
    public class WaferTransferRemoteService : IWaferTransferRemoteService
    {
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL1;

        public IContainer Container { get; set; }

        public IWaferTransferRemotableProcessModule ProcModule { get; set; }

        public EventCodeEnum InitModule(IContainer container)
        {
            try
            {
                this.Container = container;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void Activate(IWaferTransferRemotableProcessModule procModule)
        {
            try
            {
                this.ProcModule = procModule;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void Deactivate()
        {
            try
            {
                this.ProcModule = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ChuckUpMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.ChuckUpMove(option);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ChuckDownMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.ChuckDownMove(option);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Wafer_MoveLoadingPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.Wafer_MoveLoadingPosition();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.WriteVacuum(value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.MonitorForVacuum(value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.WaitForVacuum(value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.RetractARM();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.SafePosW();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        protected ILoaderModule Loader => Container.Resolve<ILoaderModule>();
        public bool IsLoadWafer()
        {
            bool retVal = false;
            try
            {
                IARMModule loadARM = Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                if(loadARM==null)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        

        public EventCodeEnum SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.SetWaferUnknownStatus(isARMUnknown, isChuckUnknown);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Notifyhandlerholdwafer(bool ishandlerhold)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.Notifyhandlerholdwafer(ishandlerhold);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum GetWaferLoadObject(out TransferObject loadedObject)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadedObject = new TransferObject();
            try
            {
                retVal = ProcModule.GetWaferLoadedObject(out loadedObject);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum NotifyTransferObject(TransferObject transferobj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = ProcModule.NotifyTransferObject(transferobj);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadedObject = null;
            try
            {
                retVal = ProcModule.NotifyLoadedToThreeLeg(out loadedObject);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

       

        public EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStateChange = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.NotifyUnloadedFromThreeLeg(waferState,cellIdx, isWaferStateChange);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum PickUpMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.PickUpMove();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum PlaceDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.PlaceDownMove();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum NotifyWaferTransferResult(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ProcModule != null)
                {
                    retVal = ProcModule.WaferTransferEnd(isSucceed);
                }
                else
                {
                    LoggerManager.Debug($"[NotifyWaferTransferResult{isSucceed}] ProcModule is Null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SelfRecoveryRetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.SelfRecoveryRetractARM();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SelfRecoveryTransferToPreAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.SelfRecoveryTransferToPreAlign();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public double GetCurrArmUpOffset()
        {
            double retVal = 0;
            try
            {
                retVal = ProcModule.GetCurrArmUpOffset();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public SubstrateSizeEnum GetTransferWaferSize()
        {
            SubstrateSizeEnum retVal = SubstrateSizeEnum.UNDEFINED;
            try
            {
                ProcModule.GetWaferLoadedObject(out TransferObject loadedObject);
                if (loadedObject != null) 
                {
                    retVal = loadedObject.Size.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        //public EventCodeEnum SetResonOfError(string errorMsg)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        retVal = ProcModule.SetResonOfError(errorMsg);

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}
    }
}
