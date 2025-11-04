using System;

namespace LoaderCore.GPProcessModules.CardChangeToCardARM
{
    using ProberErrorCode;
    using ProberInterfaces;
    using LoaderParameters;
    using LogModule;
    using LoaderBase;
    using LoaderCore.GP_CardChangeToCardARMStates;

    public partial class GP_CardChangeToCardARM : ILoaderProcessModule, ICardTransferRemotableProcessModule
    {
        public GP_CardChangeToCardARMState StateObj { get; set; }

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
                mypa.Curr is ICCModule &&
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

        public EventCodeEnum CardTransferEnd(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.NotifyWaferTransferResult(isSucceed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public EventCodeEnum CardChangePick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.CardChangePick();

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
                retVal = StateObj.Card_LoadingPosition();
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public EventCodeEnum CardDockingDone()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public string GetProbeCardID()
        {
            return "";
        }

        public EventCodeEnum CardChangeCarrierPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.CardChangeCarrierPick();
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
                retVal = StateObj.CardChangeCarrierPut();

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
                retVal = StateObj.OriginCarrierPick();

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
                retVal = StateObj.CardTransferDone(isSucceed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
        public EventCodeEnum GetUserCardIDInput(out string userCardIdInput)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            
            userCardIdInput = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
