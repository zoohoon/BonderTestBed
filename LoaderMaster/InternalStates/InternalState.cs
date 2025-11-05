using LoaderBase;
using LoaderMaster.LoaderSupervisorStates;
using LoaderParameters;
using LoaderRecoveryControl;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace LoaderMaster.InternalStates
{
    public abstract class LoaderSupervisorInternalStateBase : LoaderSupervisorStateBase
    {
        public LoaderSupervisorInternalStateBase(LoaderSupervisor module) : base(module)
        {
        }
        public override EventCodeEnum SetEMGSTOP()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                StateTransition(new Internal_Error(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class Internal_Idle : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.IDLE;

        public Internal_Idle(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = false;
            }
            Module.InternalLotInit();
        }



        public override void Execute()
        {
            Func<bool> conditionFunc = () =>
            {
                bool canRunning = false;
                canRunning = Module.IsCanLotStart();

                return canRunning;
            };

            Action doAction = () =>
            {
                Module.LotStartTime = DateTime.Now;
                StateTransition(new Internal_ReadyIdleToRunning(Module));
            };
            Action abortAction = () =>
            {

                //Module.MetroDialogManager().ShowMessageDialog("[LOTSTART]", "FAIL=" + Module.LotStartFailReason, EnumMessageStyle.AffirmativeAndNegative);
            };
            bool consumed;

            consumed = Module.CommandManager().ProcessIfRequested<IGPLotOpStart>(
          Module,
          conditionFunc,
          doAction,
          abortAction);


        }

        public override ResponseResult SetRequest(LoaderMap dstMap)
        {
            ResponseResult rr = null;

            try
            {
                //rr = Module.Sequencer.SetRequest(dstMap);

                //if (rr.IsSucceed)
                //{
                //    StateTransition(new SCHEDULING(Module));
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rr;
        }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new Internal_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region => Jog Methods
        public override EventCodeEnum JogRelMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Module.Move.JogRelMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum JogAbsMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // retVal = Module.Move.JogAbsMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion






        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is IGPLotOpStart;

            return isValidCommand;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
    }

    public class Internal_ReadyIdleToRunning : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public Internal_ReadyIdleToRunning(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = true;
            }
        }

        public override void Execute()
        {
            try
            {
                var scanjob = Module.Loader.GetLoaderInfo().StateMap;
                Module.Loader.ScanCount = 25;

                //if (Module.Loader.ScanCount == 0)
                //{
                //}
                //Module.Loader.ScanCount=Module.Loader.ScanCount--;
                bool[] scanflag = new bool[3];

                ///어떤 foup이 로드 됬는지 확인한후에 로딩된 foup scan하기 ~
                ///
                int cassette1_idx = 1;
                int cassette2_idx = 2;
                int cassette3_idx = 3;
                Module.Loader.ScanFlag[0] = false;
                Module.Loader.ScanFlag[1] = false;
                Module.Loader.ScanFlag[2] = false;
                scanflag[0] = Module.Loader.ScanFlag[0]; //testcode
                scanflag[1] = Module.Loader.ScanFlag[1]; //testcode
                scanflag[2] = Module.Loader.ScanFlag[2]; //testcode

                var Cassette1 = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, cassette1_idx);
                if (Cassette1.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    Cassette1.SetNoReadScanState();
                    if (Cassette1.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette1.ScanState == CassetteScanStateEnum.NONE)
                    {
                        scanflag[0] = true;
                        Module.Loader.DoScanJob(cassette1_idx);
                    }
                }

                var Cassette2 = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, cassette2_idx);
                if (Cassette2.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    Cassette2.SetNoReadScanState();
                    if (Cassette2.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette2.ScanState == CassetteScanStateEnum.NONE)
                    {
                        scanflag[1] = true;
                        Module.Loader.DoScanJob(cassette2_idx);
                    }
                }

                var Cassette3 = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, cassette3_idx);
                if (Cassette3.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    Cassette3.SetNoReadScanState();
                    if (Cassette3.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette3.ScanState == CassetteScanStateEnum.NONE)
                    {
                        scanflag[2] = true;
                        Module.Loader.DoScanJob(cassette3_idx);
                    }
                }

                while (true)
                {
                    if (scanflag[0])
                    {
                        if (Cassette1.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette1.ScanState == CassetteScanStateEnum.READ)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    //de.DelayFor(10);
                    Thread.Sleep(10);
                }
                while (true)
                {
                    if (scanflag[1])
                    {
                        if (Cassette2.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette2.ScanState == CassetteScanStateEnum.READ)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    //de.DelayFor(10);
                    Thread.Sleep(10);
                }
                while (true)
                {
                    if (scanflag[2])
                    {
                        if (Cassette3.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette3.ScanState == CassetteScanStateEnum.READ)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    //de.DelayFor(10);
                    Thread.Sleep(10);
                }



                Module.LotOPStart();
                Module.ArrangeCallbackIndex();
                StateTransition(new Internal_Running(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
    }

    public class Internal_Running : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;
        public Internal_Running(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = true;
            }
            Module.GEMModule().ClearAlarmOnly();
        }

        public override void Execute()
        {
            try
            {
                bool isLoaderPauseTrigger = false;
                var allWafer = Module.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll();
                var unProcessWafer = allWafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT);
                var loaderMap = Module.RequestJob();
                if (loaderMap == null && unProcessWafer == null && Module.IsLotEndReady())
                {
                    if (!Module.ContinueLotFlag)
                    {
                        var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 1);
                        if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                        {
                            this.Module.ActiveLotInfos[Cassette.ID.Index - 1].FoupUnLoad();
                           // Module.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                        }
                        Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 2);
                        if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                        {
                            this.Module.ActiveLotInfos[Cassette.ID.Index - 1].FoupUnLoad();
                         //   Module.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                        }
                        Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 3);
                        if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                        {
                            this.Module.ActiveLotInfos[Cassette.ID.Index - 1].FoupUnLoad();
                           // Module.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                        }
                    }
                    Module.LotOPEnd();
                    StateTransition(new Internal_Done(Module));
                    return;
                }
                var slicedMap = Module.MapSlicer.Slicing(loaderMap);
                if (slicedMap != null)
                {
                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        Module.Loader.SetRequest(slicedMap[i]);
                        while (true)
                        {
                            if (Module.Loader.ModuleState == ModuleStateEnum.DONE)
                            {
                                Module.Loader.ClearRequestData();
                                break;
                            }
                            else if (Module.Loader.ModuleState == ModuleStateEnum.ERROR)
                            {

                                //  Module.MetroDialogManager().ShowMessageDialog("LOADER ERROR", Module.Loader.ResonOfError, ProberInterfaces.Enum.EnumMessageStyle.Affirmative);
                                StateTransition(new Internal_Error(Module));

                                return;
                            }
                            //delays.DelayFor(100);
                            Thread.Sleep(100);
                        }
                        Func<bool> conditionFunc = () =>
                        {
                            bool canRunning = true;
                            Module.LotOPPause();
                            return canRunning;
                        };

                        Action doAction = () =>
                        {
                            if (Module.IsSelectedLoader)
                            {
                                isLoaderPauseTrigger = true;
                            }
                            else
                            {
                                isLoaderPauseTrigger = false;
                            }
                        };
                        Action abortAction = () =>
                        {
                        };
                        bool consumed;

                        consumed = Module.CommandManager().ProcessIfRequested<IGPLotOpPause>(
                      Module,
                      conditionFunc,
                      doAction,
                      abortAction);

                        Func<bool> resumeConditionFunc = () =>
                        {
                            bool canRunning = true;
                            Module.LotOPResume();
                            return canRunning;
                        };

                        Action resumeDoAction = () =>
                        {
                        };
                        Action resumeAbortAction = () =>
                        {
                        };
                        bool resumConsumed;

                        resumConsumed = Module.CommandManager().ProcessIfRequested<IGPLotOpResume>(
                      Module,
                      resumeConditionFunc,
                      resumeDoAction,
                      resumeAbortAction);

                    }
                }
                Func<bool> conditionFunc1 = () =>
                {
                    bool canRunning = true;
                    Module.LotOPPause();
                    return canRunning;
                };

                Action doAction1 = () =>
                {
                    if (Module.IsSelectedLoader)
                    {
                        isLoaderPauseTrigger = true;
                    }
                    else
                    {
                        isLoaderPauseTrigger = false;
                    }
                };
                Action abortAction1 = () =>
                 {
                 };
                bool consumed1;

                consumed1 = Module.CommandManager().ProcessIfRequested<IGPLotOpPause>(
              Module,
              conditionFunc1,
              doAction1,
              abortAction1);

                Func<bool> resumeConditionFunc1 = () =>
                {
                    bool canRunning = true;
                    Module.LotOPResume();
                    return canRunning;
                };

                Action resumeDoAction1 = () =>
                {
                };
                Action resumeAbortAction1 = () =>
                {
                };
                bool resumConsumed1;
                resumConsumed1 = Module.CommandManager().ProcessIfRequested<IGPLotOpResume>(
              Module,
              resumeConditionFunc1,
              resumeDoAction1,
              resumeAbortAction1);

                if (isLoaderPauseTrigger)
                {
                    //Module.WaitCancelDialogService().CloseDialog().Wait();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).Wait();

                    StateTransition(new Internal_Paused(Module));
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return;
        }

        public override EventCodeEnum Resume()
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
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new Internal_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is IGPLotOpPause;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
    }

    public class Internal_ReadyPausedToRunning : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public Internal_ReadyPausedToRunning(LoaderSupervisor module) : base(module) { }

        public override void Execute()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum ClearRequestData()
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
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSING;
        }

    }

    public class Internal_Pausing : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.SUSPENDED;

        public Internal_Pausing(LoaderSupervisor module) : base(module) { }

        public override void Execute() { /*No WORKS*/ }
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new Internal_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum AwakeProcessModule()
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
        //public override EventCodeEnum Pause()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    Module.PreStateObj = this;
        //    StateTransition(new PAUSED(Module));
        //    return retVal;
        //}

        public override EventCodeEnum AbortRequest()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum ClearRequestData()
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

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSING;
        }
    }

    public class Internal_Paused : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.PAUSED;

        public Internal_Paused(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = false;
            }
            //Module.ModuleState.State = ModuleStateEnum.PAUSED;
        }

        public override void Execute()
        {

            Func<bool> resumeConditionFunc = () =>
            {
                bool canRunning = true;
                Module.LotOPResume();
                return canRunning;
            };

            Action resumeDoAction = () =>
            {
                if (Module.IsSelectedLoader)
                {
                    StateTransition(new Internal_Running(Module));
                }
            };
            Action resumeAbortAction = () =>
            {

                //Module.MetroDialogManager().ShowMessageDialog("[LOTSTART]", "FAIL=" + Module.LotStartFailReason, EnumMessageStyle.AffirmativeAndNegative);
            };
            bool resumConsumed;

            resumConsumed = Module.CommandManager().ProcessIfRequested<IGPLotOpResume>(
          Module,
          resumeConditionFunc,
          resumeDoAction,
          resumeAbortAction);



            Func<bool> conditionFunc = () =>
            {
                bool canRunning = true;
                Module.LotOPPause();
                return canRunning;
            };

            Action doAction = () =>
            {
            };
            Action abortAction = () =>
            {
            };
            bool consumed;

            consumed = Module.CommandManager().ProcessIfRequested<IGPLotOpPause>(
          Module,
          conditionFunc,
          doAction,
          abortAction);

            Func<bool> EndConditionFunc = () =>
            {
                bool canRunning = true;
                // Module.LotOPEnd();
                return canRunning;
            };

            Action EndDoAction = () =>
            {
                StateTransition(new Internal_Aborted(Module));
            };
            Action EndAbortAction = () =>
            {
            };
            bool endConsumed;

            endConsumed = Module.CommandManager().ProcessIfRequested<IGPLotOpEnd>(
          Module,
          EndConditionFunc,
          EndDoAction,
          EndAbortAction);
        }

        public override EventCodeEnum ClearRequestData()
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
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new Internal_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is IGPLotOpResume ||
                token is IGPLotOpEnd;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }
    }

    public class Internal_Aborted : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;

        public Internal_Aborted(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = true;
            }
            Module.GEMModule().ClearAlarmOnly();
        }

        public override void Execute()
        {
            var allWafer = Module.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll();
            bool isLotEnd = false;
            bool isLotMode = false;
            bool isAvailableModule = true;
            var loaderMap = Module.UnloadRequestJob(out isLotEnd, out isLotMode, out isAvailableModule);
            if (isAvailableModule == false)
            {
                this.Module.GPLoader.LoaderBuzzer(true);
                var retVal = Module.MetroDialogManager().ShowMessageDialog("LOT END Fail", $"There are ARM or PA modules that cannot be used. Please Check it.", EnumMessageStyle.Affirmative).Result;
                StateTransition(new Internal_Paused(Module));
                this.Module.MetroDialogManager().CloseWaitCancelDialaog(string.Empty);
                return;
            }
            if (loaderMap == null&& isLotEnd)
            {
                if (Module.Loader.ScanFlag[0])
                {
                    var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 1);
                    Cassette.SetNoReadScanState();
                }

                if (Module.Loader.ScanFlag[1])
                {
                    var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 2);
                    Cassette.SetNoReadScanState();
                }
                if (Module.Loader.ScanFlag[2])
                {
                    var Cassette = Module.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 3);
                    Cassette.SetNoReadScanState();
                }
                Module.LotOPEnd();
                Module.ContinueLotFlag = false;

                //Module.WaitCancelDialogService().CloseDialog().Wait();
                Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).Wait();
                StateTransition(new Internal_Done(Module));
                return;
            }
            if (loaderMap != null)
            {
                var slicedMap = Module.MapSlicer.Slicing(loaderMap);
                for (int i = 0; i < slicedMap.Count; i++)
                {
                    Module.Loader.SetRequest(slicedMap[i]);
                    while (true)
                    {
                        if (Module.Loader.ModuleState == ModuleStateEnum.DONE)
                        {
                            Module.Loader.ClearRequestData();
                            break;
                        }
                    }
                }
            }

        }

        public override EventCodeEnum ClearRequestData()
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
        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new Internal_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
        //public override EventCodeEnum Pause()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    Module.PreStateObj = this;
        //    StateTransition(new PAUSED(Module));
        //    return retVal;
        //}
    }


    public class Internal_Error : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ERROR;

        public Internal_Error(LoaderSupervisor module) : base(module)
        {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().LoaderLampSetState(ModuleState);
                this.Module.GetGPLoader().IsLoaderBusy = false;
            }
        }
        bool firstFlag = true;
        public override void Execute()
        {
            if (firstFlag)
            {
              //  Module.LotOPPause();
                LoaderRecoveryControlVM.Show(Module.cont, Module.Loader.ResonOfError, Module.Loader.ErrorDetails);
                Module.Loader.ResonOfError = "";
                Module.Loader.ErrorDetails = "";
                firstFlag = false;
            }

        }

        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new Internal_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        public override void SelfRecovery()
        {
            try
            {
                //Module.Sequencer.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                StateTransition(new Internal_Paused(Module));
                Module.ModuleState.StateTransition(Module.StateObj.ModuleState);
                Module.ModuleState.State = Module.StateObj.ModuleState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }
    }

    public class Internal_Done : LoaderSupervisorInternalStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.DONE;

        public Internal_Done(LoaderSupervisor module) : base(module) {
            if (this.Module.GetGPLoader() != null)
            {
                this.Module.GetGPLoader().IsLoaderBusy = false;
            }
        }

        public override void Execute()
        {
            if (Module.ContinueLotFlag)
            {
                StateTransition(new Internal_ReadyIdleToRunning(Module));
            }
            else
            {
                //UploadLog();
                StateTransition(new Internal_Idle(Module));
            }
        }

        public override EventCodeEnum SystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SystemInitFunc();

                if (retVal == EventCodeEnum.NONE)
                    StateTransition(new Internal_Idle(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        public override void SelfRecovery()
        {
            try
            {
                //Module.Sequencer.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //private EventCodeEnum UploadLog()
        //{
        //    EventCodeEnum ret = EventCodeEnum.UNDEFINED;

        //    if (Module.LoaderLogManager.LoaderLogParam.UploadEnable.Value == true)
        //    {
        //        IsChangeLogDate changedate = new IsChangeLogDate();
        //        DateTime endtime = DateTime.Now;
        //        ret = Module.LoaderLogManager.StagesLogUploadServer(Module.LotStartTime, endtime);
        //        if (ret != EventCodeEnum.NONE)
        //        {
        //            LoggerManager.Error($"Error occurd while stage log upload func{MethodBase.GetCurrentMethod().Name} retcode:{ret}");
        //            Module.NotifyManager().Notify(EventCodeEnum.LOGUPLOAD_CONNECT_FAIL);
        //            //Module.MetroDialogManager().ShowMessageDialog("Connected Fail", $"have to check that network connect between server and loader", enummessagesytel: EnumMessageStyle.Affirmative);
        //        }
        //        else
        //        {
        //            ret = Module.LoaderLogManager.LoaderLogUploadServer(Module.LotStartTime, endtime);
        //            if (ret != EventCodeEnum.NONE)
        //            {
        //                LoggerManager.Error($"Error occurd while loader log upload func{MethodBase.GetCurrentMethod().Name} retcode:{ret}");
        //                Module.NotifyManager().Notify(EventCodeEnum.LOGUPLOAD_CONNECT_FAIL);
        //                //Module.MetroDialogManager().ShowMessageDialog("Connected Fail", $"have to check that network connect between server and loader", enummessagesytel: EnumMessageStyle.Affirmative);
        //            }
        //            else
        //            {
        //                ret = EventCodeEnum.NONE;
        //            }
        //        }

        //    }
        //    else
        //    {
        //        ret = EventCodeEnum.NONE;
        //    }
        //    return ret;
        //}
        public override EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Module.Sequencer.Clear();

                //if (Module.Move.State != LoaderMoveStateEnum.ERROR)
                //{
                //    StateTransition(new IDLE(Module));
                //    retVal = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart;

            return isValidCommand;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
    }
}
