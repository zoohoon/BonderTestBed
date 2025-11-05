using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Foup;
using System;
using System.Threading;

namespace FoupModules.FoupCover
{
    public class GPFoupCover12Inch : FoupCoverBase, ITemplateModule
    {
        private FCStateBase _StateObj;
        public FCStateBase StateObj
        {
            get { return _StateObj; }
            set { _StateObj = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }
        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if(ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"GPFoupCover12Inch.InitModule(): Init. error. Ret = {ret}");
            //}
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion
        public GPFoupCover12Inch(IFoupModule module) : base(module)
        {
        }
        public GPFoupCover12Inch() : base()
        {
        }

        public override FoupCoverStateEnum GetState()
        {
            return StateObj.GetState();
        }

        public void StateTransition(FCStateBase state)
        {
            try
            {
                StateObj = state;

                this.EnumState = StateObj.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum EventCodeEnum = EventCodeEnum.UNDEFINED;

            if ((Module.IOManager.IOMap.Inputs.DI_COVER_UPs.Count > 0) && (Module.IOManager.IOMap.Inputs.DI_COVER_DOWNs.Count > 0))
            {
                int monRetUP = Module.IOManager.MonitorForIO(
                    Module.IOManager.IOMap.Inputs.DI_COVER_UPs[Module.FoupIndex], true, 100, 200);
                int monRetDN = Module.IOManager.MonitorForIO(
                        Module.IOManager.IOMap.Inputs.DI_COVER_DOWNs[Module.FoupIndex], true, 100, 200);

                if (monRetUP != 1 & monRetDN == 1)
                {
                    StateTransition(new FCStateClose(this));
                    EventCodeEnum = EventCodeEnum.NONE;
                }
                else if (monRetUP == 1 & monRetDN != 1)
                {
                    StateTransition(new FCStateOpen(this));
                    EventCodeEnum = EventCodeEnum.NONE;
                }
                else
                {
                    StateTransition(new FCStateErr(this));
                    EventCodeEnum = EventCodeEnum.FOUP_ERROR;
                }
            }
            else
            {
                LoggerManager.Error($"IOMap is wroing");

                StateObj = new FCStateErr(this);
                EventCodeEnum = EventCodeEnum.FOUP_ERROR;
            }

            LoggerManager.Debug($"GPFoupCover12Inch.StateInit(): Initial state is {StateObj.GetState()}");

            return EventCodeEnum;
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum EventCodeEnum = EventCodeEnum.UNDEFINED;

            EventCodeEnum = CheckState();

            Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_UPs[Module.FoupIndex]);
            Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_DOWNs[Module.FoupIndex]);

            if ((Module.IOManager.IOMap.Outputs.DO_COVER_OPENs.Count > 0) && (Module.IOManager.IOMap.Outputs.DO_COVER_Closes.Count > 0))
            {
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_OPENs[Module.FoupIndex]);
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_Closes[Module.FoupIndex]);
            }

            LoggerManager.Debug($"GPFoupCover12Inch.StateInit(): Initial state is {StateObj.GetState()}");

            return EventCodeEnum;
        }
        public override EventCodeEnum Open()
        {
            return StateObj.Up();
        }

        public override EventCodeEnum Close()
        {
            return StateObj.Down();
        }
    }
    public abstract class FCStateBase
    {
        public FCStateBase(FoupCoverBase owner)
        {
            Owner = (GPFoupCover12Inch)owner;
        }
        private GPFoupCover12Inch _Owner;

        public GPFoupCover12Inch Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }
        protected IFoupIOStates IO => Owner.Module.IOManager;

        private FoupCoverStateEnum _EnumState;
        public FoupCoverStateEnum EnumState
        {
            get { return _EnumState; }
            set
            {
                _EnumState = value;
            }
        }

        protected IFoupModule GetModule()
        {
            return Owner.Module;
        }

        public abstract FoupCoverStateEnum GetState();

        public virtual EventCodeEnum Up()
        {
            return CoverOpenMethod();
        }

        public virtual EventCodeEnum Down()
        {
            return CoverCloseMethod();
        }

        public virtual EventCodeEnum Stop()
        {
            return EventCodeEnum.NONE;      // ToDo: Needs support for cylinder stop command.
        }
        protected EventCodeEnum CoverCloseMethod()
        {
            EventCodeEnum retVal;
            try
            {
                int retValue = -1;
                retValue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUTs[GetModule().FoupIndex], false, 100, 200);
                var cvrClosed = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER_CLOSEs[GetModule().FoupIndex], true, 100, 200);
                if (cvrClosed == 1)
                {
                    // In cover closed state, ignore wafer out sensor
                }
                else
                {
                    if (retValue != 1)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: GetModule().FoupIndex);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        GetModule().EventManager().RaisingEvent(typeof(FoupCloseErrorEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        //error Check Wafer Out
                        Owner.StateTransition(new FCStateErr(Owner));
                        return EventCodeEnum.FOUP_ERROR;
                    }
                }

                try
                {
                    retVal = GetModule().GPCommand.CoverClose(GetModule().FoupIndex);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        Owner.StateTransition(new FCStateClose(Owner));
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        throw new Exception($"FCStateBase().CoverDnMethod(): Error state.");
                    }
                }
                catch (Exception err)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                    Owner.StateTransition(new FCStateErr(Owner));
                    LoggerManager.Debug($"{err.Message.ToString()}. CoverUpMethod() : Error occured.");
                    LoggerManager.Exception(err);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupCoverNormal12Inch_Failure, retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        protected EventCodeEnum CoverOpenMethod()
        {
            EventCodeEnum retVal;
            try
            {
                int retValue = -1;
                retValue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUTs[GetModule().FoupIndex], false, 500, 10000);
                if (retValue != 1)
                {
                    PIVInfo pivinfo = new PIVInfo(foupnumber: GetModule().FoupIndex);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    GetModule().EventManager().RaisingEvent(typeof(FoupOpenErrorEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                    Owner.StateTransition(new FCStateErr(Owner));
                    //error Check Wafer Out
                    return EventCodeEnum.FOUP_ERROR;
                }

                try
                {
                    retVal = GetModule().GPCommand.CoverOpen(GetModule().FoupIndex);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        Owner.StateTransition(new FCStateOpen(Owner));
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        throw new Exception($"FCStateBase().CoverDnMethod(): Error state.");
                    }
                }
                catch (Exception err)
                {
                    PIVInfo pivinfo = new PIVInfo(foupnumber: GetModule().FoupIndex);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    GetModule().EventManager().RaisingEvent(typeof(FoupOpenErrorEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    retVal = EventCodeEnum.FOUP_ERROR;
                    Owner.StateTransition(new FCStateErr(Owner));
                    LoggerManager.Debug($"{err.Message.ToString()}. CoverDnMethod() : Error occured.");
                    LoggerManager.Exception(err);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupCoverNormal12Inch_Failure, retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    public class FCStateOpen : FCStateBase
    {
        public FCStateOpen(FoupCoverBase owner) : base(owner)
        {

        }


        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.OPEN;
        }

        public override EventCodeEnum Stop()
        {
            throw new NotImplementedException();
        }

    }
    public class FCStateClose : FCStateBase
    {
        public FCStateClose(FoupCoverBase owner) : base(owner)
        {

        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.CLOSE;
        }

    }
    public class FCStateErr : FCStateBase
    {
        public FCStateErr(FoupCoverBase owner) : base(owner)
        {

        }


        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.ERROR;
        }

    }
}
