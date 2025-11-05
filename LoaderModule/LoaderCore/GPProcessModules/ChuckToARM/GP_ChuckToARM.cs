using System;

namespace LoaderCore
{
    using ProberErrorCode;
    using ProberInterfaces;
    using LoaderParameters;
    using LogModule;
    using LoaderBase;
    using LoaderCore.GP_ChuckToARMStates;

    public partial class GP_ChuckToARM : ILoaderProcessModule, IWaferTransferRemotableProcessModule
    {
        public GP_ChuckToARMState StateObj { get; set; }

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
            var mypa = param as TransferProcParam;

            return
                mypa != null &&
                mypa.Curr is IChuckModule &&
                mypa.Next is IARMModule;
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
            throw new NotImplementedException();
        }

        public EventCodeEnum ChuckDownMove(int option = 0)
        {
            return StateObj.ChuckDownMove(option);
        }

        public EventCodeEnum WriteVacuum(bool value)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MonitorForVacuum(bool value)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaitForVacuum(bool value)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum Wafer_MoveLoadingPosition()
        {
            return StateObj.Wafer_MoveLoadingPosition();
        }
        public EventCodeEnum RetractARM()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SafePosW()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            return StateObj.WTR_SetWaferUnknownStatus(isARMUnknown, isChuckUnknown);
        }
        public EventCodeEnum Notifyhandlerholdwafer(bool ishandlerhold)
        {
            return StateObj.WTR_Notifyhandlerholdwafer(ishandlerhold);
        }
        public EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState,int cellIdx, bool isWaferStateChange = true)
        {
            return StateObj.NotifyUnloadedFromThreeLeg(waferState,cellIdx, isWaferStateChange);
        }

        public EventCodeEnum PickUpMove()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum PlaceDownMove()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaferTransferEnd(bool isSucceed)
        {
            return StateObj.WaferTransferEnd(isSucceed);
        }

        public EventCodeEnum SelfRecoveryRetractARM()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SelfRecoveryTransferToPreAlign()
        {
            throw new NotImplementedException();
        }

        public double GetCurrArmUpOffset()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetWaferLoadedObject(out TransferObject loadedobj)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum NotifyTransferObject(TransferObject transferobj)
        {
            return StateObj.NotifyTransferObject(transferobj);
        }
        //public EventCodeEnum SetResonOfError(string errorMsg)
        //{
        //    return StateObj.SetResonOfError(errorMsg);
        //}
    }
}
