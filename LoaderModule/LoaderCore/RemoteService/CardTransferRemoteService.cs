using System;

using ProberErrorCode;
using LoaderBase;
using ProberInterfaces;
using Autofac;
using LogModule;

namespace LoaderCore
{
    public class CardTransferRemoteService : ICardTransferRemoteService
    {
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL1;

        public IContainer Container { get; set; }

        public ICardTransferRemotableProcessModule ProcModule { get; set; }

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

        public void Activate(ICardTransferRemotableProcessModule procModule)
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

        public EventCodeEnum CardChangePick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.CardChangePick();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Card_MoveLoadingPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.Card_MoveLoadingPosition();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum CardChangePut(out TransferObject transObj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            transObj = null;
            try
            {
                retVal = ProcModule.CardChangePut(out transObj);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState waferState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            transObj = null;
            try
            {
                retVal = ProcModule.SetTransferAfterCardChangePutError(out transObj, waferState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum CardChangeCarrierPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.CardChangeCarrierPick();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OriginCarrierPut()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.OriginCarrierPut();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OriginCardPut()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.OriginCardPut();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        

        public EventCodeEnum CardChangeCarrierPut()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.CardChangeCarrierPut();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OriginCarrierPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.OriginCarrierPick();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum CardTransferDone(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.CardTransferDone(isSucceed);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        protected ILoaderModule Loader => Container.Resolve<ILoaderModule>();
       


      
     

        public EventCodeEnum NotifyCardTransferResult(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ProcModule.CardTransferEnd(isSucceed);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum NotifyCardDocking()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ProcModule != null)
                {
                    retVal = ProcModule.CardDockingDone();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public string GetProbeCardID()
        {
            string retVal = "";
            try
            {
                if (ProcModule != null)
                {
                    retVal = ProcModule.GetProbeCardID();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum GetUserCardIDInput(out string UserCardIDInput)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            
            UserCardIDInput = string.Empty;
            
            try
            {
                if (ProcModule != null)
                {
                    retVal = ProcModule.GetUserCardIDInput(out string NewCardID);
                    UserCardIDInput = NewCardID;
                }
                else
                {
                    retVal = EventCodeEnum.NODATA; 
                    UserCardIDInput = string.Empty; 
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
