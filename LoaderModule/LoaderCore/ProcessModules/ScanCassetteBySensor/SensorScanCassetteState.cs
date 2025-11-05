using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LoaderParameters;
using LogModule;

namespace LoaderCore.SensorScanCassetteStates
{
    public abstract class SensorScanCassetteState : LoaderProcStateBase
    {
        public SensorScanCassette Module { get; set; }

        public SensorScanCassetteState(SensorScanCassette module)
        {
            this.Module = module;
        }

        protected void StateTransition(SensorScanCassetteState stateObj)
        {
            try
            {
                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state tranition : {State}");
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
    }

    public class IdleState : SensorScanCassetteState
    {
        public IdleState(SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                Module.Data = new SensorScanCassetteData();
                //if (cstDev.CassetteSlotOrder == CassetteSlotOrderEnum.ByBOTTOM)
                {
                    Module.Data.SlotsOrderByBottom =
                        Loader.ModuleManager.FindSlots(Cassette).OrderBy(item => item.ID.Index).ToList();

                    Module.Data.SlotsOrderByTop =
                        Loader.ModuleManager.FindSlots(Cassette).OrderByDescending(item => item.ID.Index).ToList();
                }
                //else
                //{
                //    module.ProcVar.SlotsOrderByBottom =
                //        Loader.Modules.FindSlots(Cassette).OrderByDescending(item => item.ID.Index).ToList();

                //    module.ProcVar.SlotsOrderByTop =
                //        Loader.Modules.FindSlots(Cassette).OrderBy(item => item.ID.Index).ToList();
                //}

                StateTransition(new UpScanState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class UpScanState : SensorScanCassetteState
    {
        public UpScanState(SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                int cassetteNum = Cassette.ID.Index;

                retVal = Loader.ServiceCallback.FOUP_FoupTiltDown(cassetteNum);

                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);

                if (foupInfo.State != ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} foupInfo.State={foupInfo.State}");
                    Loader.ResonOfError = $"{Module.GetType().Name} Foup State Not a Load State. State={foupInfo.State}";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (foupInfo.TiltState != ProberInterfaces.Foup.TiltStateEnum.DOWN)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Tilt.State={foupInfo.TiltState}");
                    Loader.ResonOfError = $"{Module.GetType().Name} Tilt State Not a DOWN State. State={foupInfo.TiltState}";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                if (foupInfo.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.OPEN)
                {
                    retVal = Loader.ServiceCallback.FOUP_MonitorForWaferOutSensor(cassetteNum, false);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FOUP_MonitorForWaferOutSensore() retVal={retVal}");
                        Loader.ResonOfError = $"{Module.GetType().Name} Wafer Out Sensor Error";
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                var scanParam = ScanSensor.GetScanParam(Cassette);
                retVal = Loader.MotionManager.StartScanPosCapt(scanParam.ScanAxis.Value, scanParam.SensorInputPort.Value);

                if (retVal != EventCodeEnum.NONE)
                {
                    // TODO : 

                    Loader.MotionManager.StopScanPosCapt(scanParam.ScanAxis.Value);

                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} StartScanPosCapt() retVal={retVal}");

                    // TODO : 
                    Loader.ResonOfError = $"{Module.GetType().Name} StartScanPosCapt Error";
                    StateTransition(new SystemErrorState(Module));

                    return;
                }

                //=> move to sensor start position (CENTER)
                retVal = Loader.Move.ScanSensorStartPosMove(ScanSensor, Cassette);

                if (retVal == EventCodeEnum.FOUP_SCAN_WAFEROUT)
                {
                    LoggerManager.Debug($"[LOADER WAFEROUT SENSOR] {Module.GetType().Name} ScanSensorStartPosMove() retVal={retVal}");

                    StateTransition(new ReadScanFailState(Module));
                    return;
                }
                else if (retVal != EventCodeEnum.NONE)
                {
                    /// TODO : 
                    Loader.MotionManager.StopScanPosCapt(scanParam.ScanAxis.Value);

                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ScanSensorStartPosMove() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ScanSensorMove Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.ExtendScanSensor(ScanSensor);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ExtendScanSensor() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ScanSensorMove Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Up scan Move
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} StartScanPosCapt() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ScanSensor Postion Capture Start Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.ScanSensorUpMove(ScanSensor, Cassette);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ScanSensorUpMove() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ScanSensor Up Move Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                System.Threading.Thread.Sleep(150);
                object captureLock = new object();
                lock (captureLock)
                {
                    retVal = Loader.MotionManager.StopScanPosCapt(scanParam.ScanAxis.Value);
                }
                lock (captureLock)
                {
                    //write upscan postions
                    Module.Data.UPCapPositions =
                        Loader.MotionManager.GetAxis(scanParam.ScanAxis.Value).Status.CapturePositions.ToList();
                    LoggerManager.Debug($"Updated capture position count(UP) = {Module.Data.UPCapPositions.Count}, hashcode : {Loader.MotionManager.GetAxis(scanParam.ScanAxis.Value).Status.CapturePositions.GetHashCode()}");
                }

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} StopScanPosCapt() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ScanSensor Postion Capture Stop Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                System.Threading.Thread.Sleep(200);
                StateTransition(new DownScanState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DownScanState : SensorScanCassetteState
    {
        public DownScanState(SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                var scanParam = ScanSensor.GetScanParam(Cassette);

                retVal = Loader.MotionManager.StartScanPosCapt(scanParam.ScanAxis.Value, scanParam.SensorDownInputPort.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} DownScanState StartScanPosCapt() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ScanSensor Postion Capture Start Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.ScanSensorDownMove(ScanSensor, Cassette);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} DownScanState ScanSensorDownMove() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ScanSensor Down Move Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                object captureLock = new object();

                lock (captureLock)
                {
                    retVal = Loader.MotionManager.StopScanPosCapt(scanParam.ScanAxis.Value);
                }
                lock (captureLock)
                {
                    //write upscan postions
                    Module.Data.DownCapPositions = Loader.MotionManager.GetAxis(scanParam.ScanAxis.Value).Status.CapturePositions.ToList();
                    LoggerManager.Debug($"Updated capture position count(DN) = {Module.Data.DownCapPositions.Count}");
                }


                Loader.Move.RetractAll(LoaderMovingTypeEnum.NORMAL);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} DownScanState RetractAll() retVal={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                int cassetteNum = Cassette.ID.Index;

                if (!Loader.ServiceCallback.FoupTiltIgoreFlag)
                {

                    retVal = Loader.ServiceCallback.FOUP_FoupTiltUp(cassetteNum);

                    var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);


                    if (foupInfo.TiltState != ProberInterfaces.Foup.TiltStateEnum.UP)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Tilt.State={foupInfo.TiltState}");
                        Loader.ResonOfError = $"{Module.GetType().Name} Tilit State Not a Up State. Tilt.State={foupInfo.TiltState}";
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                }

                StateTransition(new AnalyzeState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class AnalyzeState : SensorScanCassetteState
    {
        public AnalyzeState(SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        private List<double> GetDummyScanData(ScanSensorParam scanparam, double startpos)
        {
            List<double> retval = new List<double>();

            // Output : 

            try
            {
                var slotsize = Cassette.Device.SlotSize.Value;
                var lower_thre = slotsize * scanparam.InSlotLowerRatio.Value;
                var upper_thre = slotsize * scanparam.InSlotUpperRatio.Value;

                var dummy_ratio = 0.5;

                var dummy1_thre = lower_thre * dummy_ratio;
                var dummy2_thre = upper_thre * dummy_ratio;

                for (int i = 0; i < Cassette.Device.SlotModules.Count; i++)
                {
                    var slotsize_n = slotsize * i;
                    var start_interval = startpos + slotsize_n;

                    //var lower = start_interval - lower_thre;
                    //var upper = start_interval + upper_thre;

                    var dummy1 = (start_interval - scanparam.DownOffset.Value)- dummy1_thre;
                    var dummy2 = (start_interval - scanparam.DownOffset.Value) + dummy2_thre;

                    retval.Add(dummy1);
                    retval.Add(dummy2);

                    LoggerManager.Debug($"[{this.GetType().Name}], GetDummyScanData() : [{i+1}] lower = {dummy1}, upper = {dummy2}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug("[SCAN Position Count:" + Module.Data.UPCapPositions.Count + "]");
                var cstDev = Cassette.Device;
                var slot1Param = Cassette.GetSlot1AccessParam(cstDev.AllocateDeviceInfo.Type.Value, cstDev.AllocateDeviceInfo.Size.Value);
                var scanParam = ScanSensor.GetScanParam(Cassette);

                int slotCount = cstDev.SlotModules.Count;
                double StartPos = 0;
                if (scanParam.ScanAxis.Value == EnumAxisConstants.SC)
                {
                    StartPos = slot1Param.Position.SC.Value +
                    scanParam.SensorOffset.Value;
                }
                else if (scanParam.ScanAxis.Value == EnumAxisConstants.A)
                {
                    StartPos = slot1Param.Position.A.Value +
                    scanParam.SensorOffset.Value;
                }
                else
                {
                    // Error
                    StateTransition(new SystemErrorState(Module));

                    LoggerManager.Debug("[LOADER ERROR] Error occurred while AnalyzeState. ScanAxis not defined.");

                    return;
                }

                List<double> detected_wafer_Pos = null;
                List<double> filteredCapturePoss = new List<double>();

                detected_wafer_Pos = Module.Data.UPCapPositions;

                filteredCapturePoss.Clear();

                foreach (var waferpos in detected_wafer_Pos)
                {
                    if (waferpos > StartPos + scanParam.DownOffset.Value)
                    {
                        filteredCapturePoss.Add(waferpos);
                    }
                }

                double SlotSize = cstDev.SlotSize.Value;
                double inSlotLower_Ratio = scanParam.InSlotLowerRatio.Value;
                double inSlotUpper_Ratio = scanParam.InSlotUpperRatio.Value;
                double WaferThickness = cstDev.WaferThickness.Value;
                double Tolerance = scanParam.ThicknessTol.Value;

                Dictionary<int, SlotScanStateEnum> scanMap = new Dictionary<int, SlotScanStateEnum>();
                bool[] isPass = new bool[slotCount];
                //bool first_flag = false;
                if (filteredCapturePoss.Count > 0)
                {
                    //[SCAN SENS] Wafer thickness tolerance Condition

                    double[][] cassttleData = new double[slotCount][];
                    int[] inWindowedPosCnt = new int[slotCount];

                    for (int i = 0; i < slotCount; i++)
                    {
                        //[SCAN SENS] Slot Number=>i+1;
                        cassttleData[i] = new double[100];

                        for (int index = 0; index < filteredCapturePoss.Count; index++)
                        {
                            if (filteredCapturePoss[index] >= (StartPos + (SlotSize * i) - SlotSize * inSlotLower_Ratio) &&
                               filteredCapturePoss[index] <= (StartPos + (SlotSize * i) + SlotSize * inSlotUpper_Ratio))
                            {
                                cassttleData[i][inWindowedPosCnt[i]] = filteredCapturePoss[index];
                                inWindowedPosCnt[i] += 1;
                            }
                        }
                    }

                    bool sigFlag = false;
                    int selectIdx = 0;
                    double firstWaferSize = 0;
                    double avg_value = 0;
                    for (int i = 0; i < slotCount; i++)
                    {
                        if (cassttleData[i] != null)
                        {
                            if (inWindowedPosCnt[i] >= 2)
                            {
                                double wSize = Math.Abs(cassttleData[i][inWindowedPosCnt[i] - 1] - cassttleData[i][0]);
                                if (wSize <= Cassette.Device.WaferThickness.Value * Tolerance)
                                {
                                    if (sigFlag == false)
                                    {
                                        selectIdx = i;
                                        avg_value = (cassttleData[i][inWindowedPosCnt[i] - 1] + cassttleData[i][0]) / 2;
                                        firstWaferSize = wSize;
                                        sigFlag = true;
                                    }
                                    isPass[i] = true;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < slotCount; i++)
                    {
                        //   string dirPath = null;
                        //if (Loader.ServiceCallback.FoupTiltIgoreFlag)
                        //{
                        //     dirPath = @"C:\ProberSystem\SCAN\TiltIgnore_SCANRESULT" + (i + 1) + ".txt";
                        //}
                        //else
                        //{
                        //     dirPath = @"C:\ProberSystem\SCAN\SCANRESULT" + (i + 1) + ".txt";
                        //}
                        //if (Directory.Exists(Path.GetDirectoryName(dirPath)) == false)
                        //{
                        //    Directory.CreateDirectory(Path.GetDirectoryName(dirPath));
                        //}
                        //if (!File.Exists(dirPath))
                        //{
                        //    // Create a file to write to.
                        //    using (StreamWriter sw = File.CreateText(dirPath))
                        //    {
                        //        sw.WriteLine("----SCANRESULT"+(i+1)+"----");
                        //    }
                        //}

                        SlotScanStateEnum slotRel;

                        if (cassttleData[i] != null)
                        {
                            if (inWindowedPosCnt[i] == 1)
                            {
                                //using (StreamWriter sw = File.AppendText(dirPath))
                                //{
                                //    sw.WriteLine((int)cassttleData[i][0] + " Single edge   Slot RESULT = Wafer edge lost Error");
                                //}
                                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                {
                                    LoggerManager.Debug($"{i + 1} :Slot RESULT = PASS  [Position:({cassttleData[i][0]} , {cassttleData[i][1]}) WaferSize:{cassttleData[i][1] - cassttleData[i][0]}]]");
                                    slotRel = SlotScanStateEnum.DETECTED;
                                }
                                else
                                {
                                    LoggerManager.Debug($"{i + 1} : Single edge Slot RESULT = Wafer edge lost Error [Position:({cassttleData[i][0]}) [" + DateTime.Now + "]");
                                    slotRel = SlotScanStateEnum.UNKNOWN;
                                }
                                //if (sigFlag == true)
                                //{
                                //    double slotAvgSize = avg_value + SlotSize * (i - selectIdx);
                                //    if (slotAvgSize - (firstWaferSize * 1.5) <= cassttleData[i][0] && cassttleData[i][0] <= slotAvgSize + (firstWaferSize * 1.5))
                                //    {
                                //        using (StreamWriter sw = File.AppendText(dirPath))
                                //        {
                                //            sw.WriteLine((int)cassttleData[i][0] + "    Slot RESULT =Single edge, PASS ["+DateTime.Now+"]");
                                //        }
                                //        LoggerManager.Debug($"{i + 1} :Slot RESULT =Single edge, PASS [Position:({cassttleData[i][0]})]");
                                //        //tw.WriteLine("FileName= " + FilePath + "[Count 1 =>Position Pass]  Position: " + cassttleData[i][0]);
                                //        slotRel = SlotScanStateEnum.DETECTED;
                                //    }
                                //    else
                                //    {
                                //        using (StreamWriter sw = File.AppendText(dirPath))
                                //        {
                                //            sw.WriteLine((int)cassttleData[i][0] + "    Slot RESULT = Single edge, Out wafer range Error [" + DateTime.Now + "]");
                                //        }
                                //        LoggerManager.Debug($"{i + 1} :Slot RESULT = Single edge, Out wafer range Error [Position:({cassttleData[i][0]})]");
                                //        //error
                                //        // tw.WriteLine("FileName= " + FilePath + "[Count 1 =>Position Error]  Position: " + cassttleData[i][0]);
                                //        slotRel = SlotScanStateEnum.UNKNOWN;
                                //    }
                                //}
                                //else
                                //{
                                //    using (StreamWriter sw = File.AppendText(dirPath))
                                //    {
                                //        sw.WriteLine((int)cassttleData[i][0] + "    Slot RESULT = Wafer edge lost Error");
                                //    }
                                //    LoggerManager.Debug($"{i + 1} :Slot RESULT = Wafer edge lost Error [Position:({cassttleData[i][0]}) [" + DateTime.Now + "]");
                                //    slotRel = SlotScanStateEnum.UNKNOWN;
                                //    //All Position 1
                                //    // tw.WriteLine("FileName= " + FilePath + "[All Detect Pos Count =>1]");
                                //}
                            }
                            else if (inWindowedPosCnt[i] > 1)
                            {
                                if (isPass[i] == true)
                                {
                                    //using (StreamWriter sw = File.AppendText(dirPath))
                                    //{
                                    //    sw.WriteLine(""+ (int)cassttleData[i][0]+" , "+ (int)cassttleData[i][1]+ " [WaferSize:" + (cassttleData[i][1] - cassttleData[i][0]) + "] Slot RESULT = PASS [" + DateTime.Now + "]");
                                    //}
                                    LoggerManager.Debug($"{i + 1} :Slot RESULT = PASS  [Position:({cassttleData[i][0]} , {cassttleData[i][1]}) WaferSize:{cassttleData[i][1] - cassttleData[i][0]}]]");
                                    slotRel = SlotScanStateEnum.DETECTED;
                                }
                                else
                                {
                                    if (sigFlag) /// 기준이 있을경우
                                    {
                                        double slotAvgSize = avg_value + SlotSize * (i - selectIdx);
                                        if (slotAvgSize - (firstWaferSize * 1.8) <= cassttleData[i][0] && cassttleData[i][0] <= slotAvgSize + (firstWaferSize * 1.8))
                                        {
                                            //using (StreamWriter sw = File.AppendText(dirPath))
                                            //{
                                            //    sw.WriteLine("" + (int)cassttleData[i][0] + " , " + (int)cassttleData[i][1] + " [WaferSize:" + (cassttleData[i][1] - cassttleData[i][0]) + "] Slot RESULT = Wafer thickness tolerance error [" + DateTime.Now + "]");
                                            //}
                                            LoggerManager.Debug($"{i + 1} :Slot RESULT = Wafer thickness tolerance error  [Position:({cassttleData[i][0]} , {cassttleData[i][1]}) WaferSize:{cassttleData[i][1] - cassttleData[i][0]}]]");
                                            slotRel = SlotScanStateEnum.UNKNOWN;
                                            //wafer tolerence error
                                        }
                                        else
                                        {
                                            //using (StreamWriter sw = File.AppendText(dirPath))
                                            //{
                                            //    sw.WriteLine("" + (int)cassttleData[i][0] + " , " + (int)cassttleData[i][1] + " [WaferSize:" + (cassttleData[i][1] - cassttleData[i][0]) + "] Slot RESULT = Wafer incline error [" + DateTime.Now + "]");
                                            //}
                                            LoggerManager.Debug($"{i + 1} :Slot RESULT = Wafer incline error  [Position:({cassttleData[i][0]} , {cassttleData[i][1]})] WaferSize:{cassttleData[i][1] - cassttleData[i][0]}]");
                                            slotRel = SlotScanStateEnum.UNKNOWN;
                                            // wafer INClen
                                        }
                                    }
                                    else
                                    {
                                        double slotAvgSize = StartPos + SlotSize * (i);
                                        if (slotAvgSize - (firstWaferSize * 1.8) <= cassttleData[i][0] && cassttleData[i][0] <= slotAvgSize + (firstWaferSize * 1.8))
                                        {
                                            //using (StreamWriter sw = File.AppendText(dirPath))
                                            //{
                                            //    sw.WriteLine("" + (int)cassttleData[i][0] + " , " +(int)cassttleData[i][1] + "  [WaferSize:" + (cassttleData[i][1] - cassttleData[i][0]) + "] Slot RESULT = Wafer thickness tolerance error [" + DateTime.Now + "]");
                                            //}
                                            LoggerManager.Debug($"{i + 1} :Slot RESULT = Wafer thickness tolerance error  [Position:({cassttleData[i][0]} , {cassttleData[i][1]}) WaferSize:{cassttleData[i][1] - cassttleData[i][0]}]]");
                                            slotRel = SlotScanStateEnum.UNKNOWN;
                                            //wafer tolerence error
                                        }
                                        else
                                        {
                                            //using (StreamWriter sw = File.AppendText(dirPath))
                                            //{
                                            //    sw.WriteLine("" + (int)cassttleData[i][0] + " , " + (int)cassttleData[i][1] + " [WaferSize:"+(cassttleData[i][1] -cassttleData[i][0])+"] Slot RESULT = Wafer incline error [" + DateTime.Now + "]");
                                            //}
                                            LoggerManager.Debug($"{i + 1} :Slot RESULT = Wafer incline error  [Position:({cassttleData[i][0]} , {cassttleData[i][1]}) WaferSize:{cassttleData[i][1] - cassttleData[i][0]}]");
                                            slotRel = SlotScanStateEnum.UNKNOWN;
                                            // wafer INClen
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"{i + 1} :Slot RESULT = NOWAFER");

                                slotRel = SlotScanStateEnum.NOT_DETECTED;
                            }
                            scanMap.Add(i, slotRel);
                        }

                    }//end of for

                }
                else
                {
                    for (int i = 0; i < slotCount; i++)
                    {
                        scanMap.Add(i, SlotScanStateEnum.NOT_DETECTED);
                    }
                }

                var existcount = scanMap.ToList().FindAll(x => x.Value == SlotScanStateEnum.DETECTED);

                LoggerManager.Debug($"SCAN RESULT : Detected Wafer Count = {existcount.Count}");

                Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic = new Dictionary<ISlotModule, SlotScanStateEnum>();
                var slots = Loader.ModuleManager.FindSlots(Cassette).OrderBy(item => item.ID.Index);

                int idx = 0;
                foreach (var slot in slots)
                {
                    scanRelDic.Add(slot, scanMap[idx]);
                    idx++;
                }

                //Update ScanResult
                Cassette.SetScanResult(scanRelDic);
                Loader.BroadcastLoaderInfo();

                if (Cassette.ScanState != CassetteScanStateEnum.READ)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Cassette.ScanState != CassetteScanStateEnum.READ [Cassette.ScanState={Cassette.ScanState}]");
                    StateTransition(new ReadScanFailState(Module));
                    return;
                }

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DoneState : SensorScanCassetteState
    {
        public DoneState(SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : SensorScanCassetteState
    {
        public SystemErrorState(SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class ReadScanFailState : SensorScanCassetteState
    {
        public ReadScanFailState(SensorScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.SCAN_FAILED;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/ }
    }


}
