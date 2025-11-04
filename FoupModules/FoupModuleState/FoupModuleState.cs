using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using ProberErrorCode;
using ProberInterfaces.Event;
using NotifyEventModule;
using LogModule;
using MetroDialogInterfaces;
using System.Threading;
namespace FoupModules.FoupModuleState
{
    public class FoupLoadState : FoupModuleStateBase
    {
        public FoupLoadState(IFoupModule module) : base(module) { }

        public override FoupStateEnum State => FoupStateEnum.LOAD;

        public override EventCodeEnum Load()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum UnLoad()
        {

            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {                                       

                    retVal = Module.ProcManager.UnloadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_UNLOAD_ERROR, Module.FoupNumber);
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        semaphore.Wait();

                        Module.ShowFoupErrorDialogMessage();

                        return EventCodeEnum.FOUP_ERROR;

                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupUnLoadState(Module));
                        //this.EventManager().RaisingEvent(typeof(FoupReadyToUnloadEvent).FullName);

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();


                    }

                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Continue()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.ContinueRun(); //UNLOAD

                    if (retVal == EventCodeEnum.NONE)
                    {
                        Module.FoupModuleStateTransition(new FoupUnLoadingState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        return retVal;
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }

        public override EventCodeEnum PrevRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.PreviousRun(); //UNLOAD

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //Module.FoupModuleStateTransition(new FoupUnLoadingState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        return retVal;
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }
    }

    public class FoupUnLoadState : FoupModuleStateBase
    {
        public FoupUnLoadState(IFoupModule module) : base(module) { }

        public override FoupStateEnum State => FoupStateEnum.UNLOAD;
        public override EventCodeEnum Load()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {                    
                    retVal = Module.ProcManager.LoadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_LOAD_ERROR,Module.FoupNumber);
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        Module.ShowFoupErrorDialogMessage();
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CassetteLoadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupLoadState(Module));

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteLoadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        semaphore.Wait();
                    }


                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum UnLoad()
        {
            try
            {
                EventCodeEnum retVal = 0;


                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    FoupTypeEnum fouptype = this.Module.SystemParam.FoupType.Value;

                    retVal = Module.ProcManager.UnloadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_UNLOAD_ERROR,Module.FoupNumber);

                        Module.ShowFoupErrorDialogMessage();
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        return EventCodeEnum.FOUP_ERROR;

                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupUnLoadState(Module));

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupReadyToUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        //this.CommandManager().SetCommand<ILotOpEnd>(this);
                    }

                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum Continue()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.ContinueRun();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        Module.FoupModuleStateTransition(new FoupLoadingState(Module));
                        return retVal;
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));

                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }

        public override EventCodeEnum PrevRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.PreviousRun(); //UNLOAD

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //Module.FoupModuleStateTransition(new FoupUnLoadingState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        return retVal;
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }
    }

    public class FoupErrorState : FoupModuleStateBase
    {

        public FoupErrorState(IFoupModule module) : base(module) { }

        public override FoupStateEnum State => FoupStateEnum.ERROR;

        public override EventCodeEnum Load()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {

                    retVal = Module.ProcManager.LoadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_LOAD_ERROR,Module.FoupNumber);

                        Module.ShowFoupErrorDialogMessage();
                        this.EventManager().RaisingEvent(typeof(CassetteLoadFailEvent).FullName);
                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupLoadState(Module));

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex+1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteLoadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        semaphore.Wait();
                    }


                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum UnLoad()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.UnloadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_UNLOAD_ERROR,Module.FoupNumber);

                        Module.ShowFoupErrorDialogMessage();
                        return EventCodeEnum.FOUP_ERROR;
                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupUnLoadState(Module));
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                    }

                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum Continue()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    FoupStateEnum state = Module.ProcManager.FindFoupState();

                    if(state != FoupStateEnum.UNDEFIND)
                    {
                        retVal = Module.ProcManager.ContinueRun();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            if(state == FoupStateEnum.LOAD) //loadsequence
                            {
                                Module.FoupModuleStateTransition(new FoupLoadingState(Module));
                                return retVal;
                            }
                            else if(state == FoupStateEnum.UNLOAD)
                            {

                                Module.FoupModuleStateTransition(new FoupUnLoadingState(Module));
                                return retVal;
                            }
                        }
                        else if (retVal == EventCodeEnum.FOUP_SEQUENCE_END)
                        {
                            if (state == FoupStateEnum.LOAD)
                            {
                                Module.FoupModuleStateTransition(new FoupLoadState(Module));
                                return retVal;
                            }
                            else if (state == FoupStateEnum.UNLOAD)
                            {

                                Module.FoupModuleStateTransition(new FoupUnLoadState(Module));
                                PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                                return retVal;
                            }

                            return retVal;
                        }
                        else
                        {
                            ////FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                            //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                            //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            Module.FoupModuleStateTransition(new FoupErrorState(Module));
                            //semaphore.Wait();

                            Module.NotifyManager().Notify(EventCodeEnum.FOUP_ERROR, Module.FoupNumber);
                            LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);

                            return retVal;
                        }
                    }

                }

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }


        public override EventCodeEnum PrevRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.PreviousRun(); //UNLOAD

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //Module.FoupModuleStateTransition(new FoupUnLoadingState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        return retVal;
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }
    }
    public class FoupEMPTY_CASSETTEState : FoupModuleStateBase
    {

        public FoupEMPTY_CASSETTEState(IFoupModule module) : base(module) { }

        public override FoupStateEnum State => FoupStateEnum.EMPTY_CASSETTE;

        public override EventCodeEnum Load()
        {
            try
            {
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum UnLoad()
        {
            try
            {

                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum Continue()
        {
            try
            {
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum PrevRun()
        {
            try
            {
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class FoupLoadingState : FoupModuleStateBase
    {
        public FoupLoadingState(IFoupModule module) : base(module) { }

        public override FoupStateEnum State => FoupStateEnum.LOADING;

        public override EventCodeEnum Load()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.LoadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_LOAD_ERROR, Module.FoupNumber);
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));

                        Module.ShowFoupErrorDialogMessage();
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CassetteLoadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupLoadState(Module));

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteLoadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        semaphore.Wait();
                    }


                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum UnLoad()
        {

            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {

                    retVal = Module.ProcManager.UnloadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_UNLOAD_ERROR, Module.FoupNumber);
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        semaphore.Wait();



                        Module.ShowFoupErrorDialogMessage();
                        return EventCodeEnum.FOUP_ERROR;

                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupUnLoadState(Module));
                        //this.EventManager().RaisingEvent(typeof(FoupReadyToUnloadEvent).FullName);

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();


                    }

                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Continue()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.ContinueRun();

                    if (retVal == EventCodeEnum.NONE)
                    {

                    }
                    else if (retVal == EventCodeEnum.FOUP_SEQUENCE_END)
                    {
                        Module.FoupModuleStateTransition(new FoupLoadState(Module));

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        ////Module.EventManager().RaisingEvent(typeof(CassetteLoadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        //semaphore.Wait();
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);

                        return retVal;
                    }
                }
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }
        public override EventCodeEnum PrevRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.PreviousRun(); //UNLOAD

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //Module.FoupModuleStateTransition(new FoupUnLoadingState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        return retVal;
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }

    }
    public class FoupUnLoadingState : FoupModuleStateBase
    {
        public FoupUnLoadingState(IFoupModule module) : base(module) { }

        public override FoupStateEnum State => FoupStateEnum.UNLOADING;

        public override EventCodeEnum Load()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.LoadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_LOAD_ERROR, Module.FoupNumber);
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));

                        Module.ShowFoupErrorDialogMessage();
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CassetteLoadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupLoadState(Module));

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteLoadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        semaphore.Wait();
                    }


                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum UnLoad()
        {

            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {

                    retVal = Module.ProcManager.UnloadRun();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        Module.NotifyManager().Notify(EventCodeEnum.FOUP_UNLOAD_ERROR, Module.FoupNumber);
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                        semaphore.Wait();



                        Module.ShowFoupErrorDialogMessage();
                        return EventCodeEnum.FOUP_ERROR;

                    }
                    else
                    {
                        Module.FoupModuleStateTransition(new FoupUnLoadState(Module));
                        //this.EventManager().RaisingEvent(typeof(FoupReadyToUnloadEvent).FullName);

                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();


                    }

                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Continue()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.ContinueRun();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        
                    }
                    else if(retVal == EventCodeEnum.FOUP_SEQUENCE_END)
                    {
                        Module.FoupModuleStateTransition(new FoupUnLoadState(Module));
                        PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);

                        return retVal;
                    }
                }
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }

        public override EventCodeEnum PrevRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.Permission != null && Module.Permission.GetState() == FoupPermissionStateEnum.BUSY)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.ProcManager.PreviousRun(); //UNLOAD

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //Module.FoupModuleStateTransition(new FoupUnLoadingState(Module));
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        return retVal;
                    }
                    else
                    {
                        //FOUP_SEQUENCE_NULL, UNEDFINED, FOUP_ERROR
                        Module.FoupModuleStateTransition(new FoupErrorState(Module));
                        LoggerManager.RecoveryLog($"Reason : {retVal}, Please check for cassette position.", true);
                        //PIVInfo pivinfo = new PIVInfo(foupnumber: Module.FoupIndex + 1);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //Module.EventManager().RaisingEvent(typeof(CassetteUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        //semaphore.Wait();
                        Module.NotifyManager().Notify(retVal, Module.FoupNumber);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.RecoveryLog($"foup module state has been transferred, {Module.ModuleState.State}");
            }
        }
    }
}
