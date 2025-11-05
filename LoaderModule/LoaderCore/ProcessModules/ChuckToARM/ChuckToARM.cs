using System;
using LoaderBase;
using LoaderParameters;
using ProberInterfaces;
using ProberErrorCode;

namespace LoaderCore
{
    using ChuckToARMStates;
    using LogModule;

    public partial class ChuckToARM : ILoaderProcessModule, IWaferTransferRemotableProcessModule
    {
        public ChuckToARMState StateObj { get; set; }
                
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

                retval = mypa != null &&
                mypa.Curr is IChuckModule &&
                mypa.Next is IARMModule &&
                mypa.UseARM is IARMModule;
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

        public EventCodeEnum ChuckUpMove(int option=0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.ChuckUpMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ChuckDownMove(int option=0)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.ChuckDownMove(option);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum Wafer_MoveLoadingPosition()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.WriteVacuum(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.MonitorForVacuum(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.WaitForVacuum(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum RetractARM()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.RetractARM();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SafePosW()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.SafePosW();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.WTR_SetWaferUnknownStatus(isARMUnknown, isChuckUnknown);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            loadedObject = null;

            try
            {
                retval = StateObj.NotifyLoadedToThreeLeg(out loadedObject);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStateChange = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.NotifyUnloadedFromThreeLeg(waferState, cellIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum PickUpMove()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.PickUpMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum PlaceDownMove()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.PlaceDownMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SelfRecoveryRetractARM()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.SelfRecoveryRetractARM();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SelfRecoveryTransferToPreAlign()
        {
            return StateObj.SelfRecoveryTransferToPreAlign();
        }

        public EventCodeEnum WaferTransferEnd(bool isSucceed)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.WaferTransferEnd(isSucceed);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public double GetCurrArmUpOffset()
        {
            double retVal = 0;
            try
            {
                retVal=StateObj.GetCurrArmUpOffset();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum GetWaferLoadedObject(out TransferObject loadedobj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            loadedobj = null;

            try
            {
                retval = StateObj.GetWaferLoadObject(out loadedobj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
