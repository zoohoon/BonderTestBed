using System;
using LoaderBase;

namespace LoaderCore
{
    using ARMToChuckStates;
    using ProberErrorCode;
    using ProberInterfaces;
    using LoaderParameters;
    using LogModule;

    public partial class ARMToChuck : ILoaderProcessModule, IWaferTransferRemotableProcessModule
    {
        public ARMToChuckState StateObj { get; set; }
        
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
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanExecute(ILoaderProcessParam param)
        {
            var mypa = param as TransferProcParam;

            return
                mypa != null &&
                mypa.Curr is IARMModule &&
                mypa.Next is IChuckModule &&
                mypa.UseARM is IARMModule;
        }

        public LoaderProcStateEnum State => StateObj.State;

        public ReasonOfSuspendedEnum ReasonOfSuspended => StateObj.ReasonOfSuspended;
        
        public void Execute()
        {
            try
            {

            StateObj.Execute();
            }
            catch(Exception err)
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

        public EventCodeEnum ChuckUpMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            retVal = StateObj.ChuckUpMove(option);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ChuckDownMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.ChuckDownMove(option);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum Wafer_MoveLoadingPosition()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.WriteVacuum(value);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.MonitorForVacuum(value);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.WaitForVacuum(value);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum RetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.RetractARM();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.SafePosW();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.WTR_SetWaferUnknownStatus(isARMUnknown, isChuckUnknown);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            return StateObj.NotifyLoadedToThreeLeg(out loadedObject);
        }

        public EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStateChange = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.NotifyUnloadedFromThreeLeg(waferState, cellIdx);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PickUpMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.PickUpMove();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PlaceDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.PlaceDownMove();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SelfRecoveryRetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.SelfRecoveryRetractARM();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SelfRecoveryTransferToPreAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.SelfRecoveryTransferToPreAlign();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WaferTransferEnd(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StateObj.NotifyWaferTransferResult(isSucceed);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public double GetCurrArmUpOffset()
        {
            double retVal = 0;
            try
            {
                retVal = StateObj.GetCurrArmUpOffset();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum GetWaferLoadedObject(out TransferObject loadobj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadobj = null;
            try
            {
                retVal = StateObj.GetLoadedObject(out loadobj);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum NotifyTransferObject(TransferObject transferobj)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Notifyhandlerholdwafer(bool ishandlerhold)
        {
            throw new NotImplementedException();
        }

        //public EventCodeEnum SetResonOfError(string errorMsg)
        //{
        //    throw new NotImplementedException();
        //}
    }

}
