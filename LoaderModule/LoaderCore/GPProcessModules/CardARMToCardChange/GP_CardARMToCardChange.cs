using System;

namespace LoaderCore
{
    using ProberErrorCode;
    using ProberInterfaces;
    using LoaderParameters;
    using LogModule;
    using LoaderBase;
    using LoaderCore.GP_CardARMToCardChangeStates;

    public partial class GP_CardARMToCardChange : ILoaderProcessModule, ICardTransferRemotableProcessModule
    {
        public GP_CardARMToCardChangeState StateObj { get; set; }

        public CardTransferProcParam Param { get; private set; }

        public Autofac.IContainer Container { get; private set; }

        public void Init(Autofac.IContainer container, ILoaderProcessParam param)
        {
            try
            {
                this.Container = container;

                this.Param = param as CardTransferProcParam;
                LoggerManager.Debug("GP_CardARMToCardChange Init execute");
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
                mypa.Curr is ICardARMModule &&
                mypa.Next is ICCModule &&
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
            try
            {
                return EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return EventCodeEnum.UNDEFINED;
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
            return EventCodeEnum.UNDEFINED;
        }
        public EventCodeEnum CardChangePut(out TransferObject transObj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            transObj = null;
            try
            {
                retVal = StateObj.CardChangePut(out transObj);
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
                retVal = StateObj.SetTransferAfterCardChangePutError(out transObj, waferState);
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
                retVal = StateObj.CardChangeCarrierPick();
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
                retVal = StateObj.OriginCarrierPut();
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
                retVal = StateObj.OriginCardPut();
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
                retVal = StateObj.CardDockingDone();
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
                retVal = StateObj.GetProbeCardID();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public EventCodeEnum GetUserCardIDInput(out string userCardIdInput)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            userCardIdInput = string.Empty;

            try
            {
                retVal = StateObj.GetUserCardIDInput(out userCardIdInput);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum CardChangeCarrierPut()
        {
            try
            {
                return EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum OriginCarrierPick()
        {
            try
            {
                return EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return EventCodeEnum.UNDEFINED;
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
    }
}
