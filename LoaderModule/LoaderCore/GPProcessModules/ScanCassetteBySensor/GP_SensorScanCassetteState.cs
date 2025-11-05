using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LoaderCore.GP_SensorScanCassetteStates
{
    public abstract class GP_SensorScanCassetteState : LoaderProcStateBase
    {
        public GP_SensorScanCassette Module { get; set; }

        public GP_SensorScanCassetteState(GP_SensorScanCassette module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_SensorScanCassetteState stateObj)
        {
            try
            {

                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state transition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected ICassetteModule Cassette => Module.Param.Cassette;

        protected IScanSensorModule ScanSensor => Module.Param.UseScanable as IScanSensorModule;

        protected SensorScanCassetteData Data => Module.Data;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_SensorScanCassetteState
    {
        public IdleState(GP_SensorScanCassette module) : base(module)
        {
            //Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.SENSORSCANCASSETTE;
            //Loader.ProcModuleInfo.Source = Cassette.ID;

            // loadermap이 있는지 판단하는 로직중 Scan이 끼어들 경우
            // ProcModule이 SENSORSCANCASSETTE 으로 업데이트 되어
            // 현재 실행중인 것을 판단하는 ProcModuleInfo가 오염되는 현상 고침.
            // 현재 scan은 plc에서 제어하기때문에 Scan에 대한 ProcModuleInfo 업데이트 안하도록함. 
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            LoggerManager.ActionLog(ModuleLogType.SCAN_CASSETTE, StateLogType.START, $"Foup:{Cassette.ID.Index}", isLoaderMap: true);
            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : GP_SensorScanCassetteState
    {
        public RunningState(GP_SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {

            var result = this.GetLoaderCommands().CassetteScan(Cassette);
            //  EventCodeEnum result = EventCodeEnum.NONE;
            if (result == EventCodeEnum.NONE)
            {

                System.Threading.Thread.Sleep(1000);
                StateTransition(new AnalyzeState(Module));
            }
            else
            {
                Cassette.SetIllegalScanState();

                Loader.NotifyManager.Notify(EventCodeEnum.FOUP_SCAN_FAILED, Module.Param.Cassette.ID.Index);

                LoggerManager.Error($"GP_SensorScanCassetteState(): Transfer failed. Job result = {result}");
                StateTransition(new SystemErrorState(Module));
            }
            //StateTransition(new AnalyzeState(Module));
        }
    }


    public class AnalyzeState : GP_SensorScanCassetteState
    {
        public AnalyzeState(GP_SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                Dictionary<int, SlotScanStateEnum> scanMap = new Dictionary<int, SlotScanStateEnum>();

                uint ScanCount = 0;
                short[] ScanStates = null;
                int existcount = 0;
                var cdx = this.GetGPLoader().CDXIn;

                CassetteTypeEnum CassetteType = Loader.GetCassetteType(Cassette.ID.Index);

                if (this.GetGPLoader().CDXIn.nCSTWafer_Cnt != null && this.GetGPLoader().CDXIn.nCSTWafer_State != null)
                {
                    ScanCount = this.GetGPLoader().GetScanCount(Cassette.ID.Index - 1);
                    ScanStates = this.GetGPLoader().GetWaferInfos(Cassette.ID.Index - 1);

                    if (ProberInterfaces.SystemManager.SystemType == ProberInterfaces.SystemTypeEnum.DRAX)
                    {
                        for (int i = 0; i < Cassette.Definition.SlotModules.Count; i++)
                        {
                            int index = (Cassette.ID.Index - 1) * Cassette.Definition.SlotModules.Count + i;

                            if (ScanStates[index] == 1)
                            {
                                scanMap.Add(i, SlotScanStateEnum.DETECTED);
                                existcount++;
                            }
                            else if (ScanStates[index] == 0)
                            {
                                scanMap.Add(i, SlotScanStateEnum.NOT_DETECTED);
                            }
                            else if (ScanStates[index] == -1)
                            {
                                scanMap.Add(i, SlotScanStateEnum.UNKNOWN);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Cassette.Definition.SlotModules.Count; i++)
                        {
                            if (ScanStates[i] == 1)
                            {
                                scanMap.Add(i, SlotScanStateEnum.DETECTED);
                                existcount++;
                            }
                            else if (ScanStates[i] == 0)
                            {
                                scanMap.Add(i, SlotScanStateEnum.NOT_DETECTED);
                            }
                            else if (ScanStates[i] == -1)
                            {
                                scanMap.Add(i, SlotScanStateEnum.UNKNOWN);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Cassette.Definition.SlotModules.Count; i++)
                    {
                        if (i < Loader.ScanCount)
                        {
                            scanMap.Add(i, SlotScanStateEnum.DETECTED);
                        }
                        else
                        {
                            scanMap.Add(i, SlotScanStateEnum.NOT_DETECTED);
                        }
                    }
                    existcount = scanMap.ToList().FindAll(x => x.Value == SlotScanStateEnum.DETECTED).Count;
                }

                if (Loader.EmulScanCount > 0 && ProberInterfaces.Extensions_IParam.ProberRunMode == ProberInterfaces.RunMode.EMUL)
                {
                    if (CassetteType == CassetteTypeEnum.FOUP_13)
                    {
                        scanMap = new Dictionary<int, SlotScanStateEnum>();

                        int existcount_13 = 0;
                        for (int k = 0; k < Cassette.Definition.SlotModules.Count; k++)
                        {
                            if (existcount_13 < Loader.EmulScanCount && k % 2 == 0)
                            {
                                scanMap.Add(k, SlotScanStateEnum.DETECTED);
                                existcount_13++;
                            }
                            else
                            {
                                scanMap.Add(k, SlotScanStateEnum.NOT_DETECTED); 
                            }
                        }
                    }

                    existcount = scanMap.ToList().FindAll(x => x.Value == SlotScanStateEnum.DETECTED).Count;
                }


                Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic = new Dictionary<ISlotModule, SlotScanStateEnum>();
                var slots = Loader.ModuleManager.FindSlots(Cassette).OrderBy(item => item.ID.Index);

                int idx = 0;
                string scanResult = "";

                foreach (var slot in slots)
                {
                    if (scanMap[idx] == SlotScanStateEnum.DETECTED)
                    {
                        scanResult += "1";
                    }
                    else
                    {
                        scanResult += "0";
                    }

                    if (idx % 2 == 1 && CassetteType == CassetteTypeEnum.FOUP_13)
                    {
                        if (scanMap[idx] == SlotScanStateEnum.DETECTED && idx > 0)
                        {
                            if (scanMap.ContainsKey(idx - 1))
                            {
                                scanMap[idx - 1] = SlotScanStateEnum.UNKNOWN;
                            }

                            if (scanMap.ContainsKey(idx))
                            {
                                scanMap[idx] = SlotScanStateEnum.UNKNOWN;
                            }

                            if (scanMap.ContainsKey(idx + 1))
                            {
                                scanMap[idx + 1] = SlotScanStateEnum.UNKNOWN;
                            }
                        }
                    }

                    idx++;
                }
                idx = 0;
                foreach (var slot in slots)
                {
                    scanRelDic.Add(slot, scanMap[idx]);
                    idx++;
                }

                LoggerManager.ActionLog(ModuleLogType.SCAN_CASSETTE, StateLogType.DONE, $"Scan Result: WaferCount = {existcount}, SLOT(1~{scanMap.Count}) = [{scanResult}], Foup: {Cassette.ID.Index}", isLoaderMap: true);

                //Update ScanResult
                Cassette.SetScanResult(scanRelDic);
                var scanwafersize = this.GetGPLoader().GetDeviceSize(Cassette.ID.Index - 1);

                foreach (var transferobj in scanRelDic.Keys)
                {
                    try
                    {
                        if (transferobj.Holder.TransferObject != null)
                        {
                            transferobj.Holder.TransferObject.Size.Value = scanwafersize;
                            transferobj.Holder.TransferObject.CST_HashCode = Cassette.HashCode;
                            transferobj.Holder.TransferObject.WaferType.Value = ProberInterfaces.EnumWaferType.STANDARD;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"Set CST_HashCode Error: ({transferobj.ID})");
                        LoggerManager.Exception(err);
                    }
                }

                LoggerManager.Debug($"[GP_SensorScanCassetteState] Execute(): Cassette.HashCode:{Cassette.HashCode}");
                Loader.LoaderMaster.SetCaseetteHashCodeToStage(Cassette.ID.Index, Cassette.HashCode);
                Loader.BroadcastLoaderInfo();

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                StateTransition(new SystemErrorState(Module));
                LoggerManager.Exception(err);
            }
        }
    }

    public class DoneState : GP_SensorScanCassetteState
    {
        public DoneState(GP_SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_SensorScanCassetteState
    {
        public SystemErrorState(GP_SensorScanCassette module) : base(module) 
        {
            Loader.BroadcastLoaderInfo();
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
