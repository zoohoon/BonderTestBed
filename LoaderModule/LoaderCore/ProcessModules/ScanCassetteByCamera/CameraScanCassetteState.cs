using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LoaderParameters;
using LogModule;
using System.Threading;

namespace LoaderCore.CameraScanCassetteStates
{
    public abstract class CameraScanCassetteState : LoaderProcStateBase
    {
        public CameraScanCassette Module { get; set; }

        public CameraScanCassetteState(CameraScanCassette module)
        {
            this.Module = module;
        }
        protected void StateTransition(CameraScanCassetteState stateObj)
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

        protected IScanCameraModule ScanCamera => Module.Param.UseScanable as IScanCameraModule;

        protected CameraScanCassetteData Data => Module.Data;
    }

    public class IdleState : CameraScanCassetteState
    {
        public IdleState(CameraScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"Scan Cassette START");

                var scanparam = GetScanCameraParam();
                var slot1AccessParam = GetCurrSlot1AccessParam();
                Module.Data = new CameraScanCassetteData();

                Module.Data.Slot1AccessParam = slot1AccessParam;
                Module.Data.ScanParam = scanparam;
                Module.Data.GrabSize = Loader.VisionManager.GetGrabSize(GetCam());


                double imgWidth = Module.Data.GrabSize.Width;
                double imgHeight = Module.Data.GrabSize.Height;
                double slotSizePixel = Cassette.Device.SlotSize.Value / scanparam.CameraRatio.Value;

                double ROIWidth = imgWidth * scanparam.ROIWidthRatio.Value;
                double ROIHeight = slotSizePixel * scanparam.ROIHeightRatio.Value;
                double ROI_X = (imgWidth - ROIWidth) * 0.5;
                double ROI_Y = (imgHeight - ROIHeight) * 0.5;

                Module.Data.Cam = GetCam();
                Module.Data.SlotsOrderByBottom =
                    Loader.ModuleManager.FindSlots(Cassette).OrderBy(item => item.ID.Index).ToList();
                Module.Data.SlotsOrderByTop =
                    Loader.ModuleManager.FindSlots(Cassette).OrderByDescending(item => item.ID.Index).ToList();
                Module.Data.GrabSize = Loader.VisionManager.GetGrabSize(Data.Cam);
                Module.Data.ROI = new System.Windows.Rect()
                {
                    X = ROI_X,
                    Y = ROI_Y,
                    Width = ROIWidth,
                    Height = ROIHeight,
                };

               
                
                

                

                StateTransition(new UpScanState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EnumProberCam GetCam()
        {
            EnumProberCam camtype = EnumProberCam.INVALID;
            try
            {
                switch (Cassette.Device.AllocateDeviceInfo.Size.Value)
                {
                    case SubstrateSizeEnum.INCH6:
                        camtype = EnumProberCam.ARM_6_CAM;
                        break;
                    case SubstrateSizeEnum.INCH8:
                    case SubstrateSizeEnum.INCH12:
                        camtype = EnumProberCam.ARM_8_12_CAM;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }

            return camtype;
        }

        private ScanCameraParam GetScanCameraParam()
        {
            return ScanCamera.GetScanCameraParam(Cassette);
        }

        private CassetteSlot1AccessParam GetCurrSlot1AccessParam()
        {
            return Cassette.GetSlot1AccessParam(Cassette.Device.AllocateDeviceInfo.Type.Value, Cassette.Device.AllocateDeviceInfo.Size.Value);
        }
    }

    public class UpScanState : CameraScanCassetteState
    {
        public UpScanState(CameraScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                int cassetteNum = Cassette.ID.Index;
                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);

                if (foupInfo.State != ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} foupInfo.State={foupInfo.State}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (foupInfo.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.OPEN)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} foupInfo.FoupCoverState={foupInfo.FoupCoverState}");
                    retVal = Loader.ServiceCallback.FOUP_MonitorForWaferOutSensor(cassetteNum, false);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                var scanparam = Data.ScanParam;
                var slot1Accpos = Data.Slot1AccessParam;

                //light on
                Loader.Light.SetLight(scanparam.LightChannel.Value, scanparam.LightIntensity.Value);

                Thread.Sleep(3000);

                int slotCount = Cassette.Device.SlotModules.Count;
                var bottomSlot = Data.SlotsOrderByBottom.First();

                //=> SLOT1 Position (CENTER)
                retVal = Loader.Move.ScanCameraStartPosMove(ScanCamera, Cassette);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ScanCameraStartPosMove() ReturnValue={retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //Up scan offset move
                retVal = Loader.Move.ScanCameraRelMove(EnumAxisConstants.W, scanparam.UpScanWOffset.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ScanCameraRelMove(EnumAxisConstants.W, scanparam.UpScanWOffset.Value) ReturnValue={retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //LoggerManager.Debug("Loader : Scan position moved.");
                LoggerManager.Debug($"Scan position moved");

                //=> 1슬롯씩 스캔
                var firstSlot = Data.SlotsOrderByBottom.First();
                Data.UpScanDataDic = new Dictionary<ISlotModule, ImageBuffer>();
                foreach (var SLOT in Data.SlotsOrderByBottom)
                {
                    if (firstSlot != SLOT)
                    {
                        double Aoffset = Cassette.Device.SlotSize.Value;
                        retVal = Loader.Move.ScanCameraRelMove(EnumAxisConstants.A, Aoffset);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ScanCameraRelMove(EnumAxisConstants.A, Aoffset) ReturnValue={retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }

                    //CassetteScanProcessingParam pa = new CassetteScanProcessingParam();
                    //pa.SlotLabel = SLOT.ID.Label;
                    //pa.ROI = ProcVar.ROI;
                    //pa.DumpPrefix = ProcVar.DumpPrefixStr + "UP";

                    ImageBuffer ib = Loader.VisionManager.SingleGrab(Data.Cam);

                    Data.UpScanDataDic.Add(SLOT, ib);

                    //var slotRels = Loader.Vision.CassetteScanProcessing(ib, paList, true);
                    //foreach (var rel in slotRels)
                    //{
                    //    ProcVar.UpScanResults.Add(rel.SlotLabel, rel);
                    //}
                }

                StateTransition(new DownScanState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DownScanState : CameraScanCassetteState
    {
        public DownScanState(CameraScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                var scanparam = Data.ScanParam;

                int slotCount = Cassette.Device.SlotModules.Count;

                var topSlot = Data.SlotsOrderByTop.First();

                //=>현재 SlotN 의 A 위치에 있음.
                //DownOffset으로 이동
                double Woffset = scanparam.DownScanWOffset.Value - scanparam.UpScanWOffset.Value;
                retVal = Loader.Move.ScanCameraRelMove(EnumAxisConstants.W, Woffset);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ScanCameraRelMove(EnumAxisConstants.W, Woffset) ReturnValue={retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> 1슬롯씩 스캔
                var firstSlot = Data.SlotsOrderByTop.First();
                Data.DownScanDataDic = new Dictionary<ISlotModule, ImageBuffer>();
                foreach (var SLOT in Data.SlotsOrderByTop)
                {
                    if (firstSlot != SLOT)
                    {
                        double Aoffset = Cassette.Device.SlotSize.Value * -1.0;
                        retVal = Loader.Move.ScanCameraRelMove(EnumAxisConstants.A, Aoffset);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ScanCameraRelMove(EnumAxisConstants.A, Aoffset) ReturnValue={retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }

                    //CassetteScanProcessingParam pa = new CassetteScanProcessingParam();
                    //pa.SlotLabel = SLOT.ID.Label;
                    //pa.ROI = ProcVar.ROI;
                    //pa.DumpPrefix = ProcVar.DumpPrefixStr + "DN";

                    //var paList = new List<CassetteScanProcessingParam>() { pa };

                    ImageBuffer ib = Loader.VisionManager.SingleGrab(Data.Cam);

                    Data.DownScanDataDic.Add(SLOT, ib);

                    //var slotRels = Loader.Vision.CassetteScanProcessing(ib, paList, true);
                    //foreach (var rel in slotRels)
                    //{
                    //    ProcVar.DownScanResults.Add(rel.SlotLabel, rel);
                    //}


                }

                StateTransition(new AnalyzeState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class AnalyzeState : CameraScanCassetteState
    {
        public AnalyzeState(CameraScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                //Key : SlotLabel, Value : ScanRel
                Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic = new Dictionary<ISlotModule, SlotScanStateEnum>();

                var scanparam = Data.ScanParam;

                double imgWidth = Module.Data.GrabSize.Width;

                double scoreThreshold = scanparam.ROEWidthScoreRatio.Value;

                double pixelPerCamRatio = Cassette.Device.WaferThickness.Value / scanparam.CameraRatio.Value;

                double minThickPixel = pixelPerCamRatio * scanparam.MinThicknessRatio.Value;
                double maxThickPixel = pixelPerCamRatio * scanparam.MaxThicknessRatio.Value;

                Dictionary<ISlotModule, CassetteScanSlotResult> UpScanResults = new Dictionary<ISlotModule, CassetteScanSlotResult>();
                foreach (var scanData in Data.UpScanDataDic)
                {
                    var SLOT = scanData.Key;
                    ImageBuffer ib = scanData.Value;

                    CassetteScanSlotParam slotParam = new CassetteScanSlotParam();
                    slotParam.SlotLabel = SLOT.ID.Label;
                    slotParam.ROI = Data.ROI;
                    slotParam.DumpPrefix = Data.DumpPrefixStr + "UP";

                    var rel = Loader.VisionManager.CassetteScanProcessing(ib, slotParam, true);

                    UpScanResults.Add(SLOT, rel);
                }

                Dictionary<ISlotModule, CassetteScanSlotResult> DownScanResults = new Dictionary<ISlotModule, CassetteScanSlotResult>();
                foreach (var scanData in Data.DownScanDataDic)
                {
                    var SLOT = scanData.Key;
                    ImageBuffer ib = scanData.Value;

                    CassetteScanSlotParam slotParam = new CassetteScanSlotParam();
                    slotParam.SlotLabel = SLOT.ID.Label;
                    slotParam.ROI = Data.ROI;
                    slotParam.DumpPrefix = Data.DumpPrefixStr + "DN";

                    var rel = Loader.VisionManager.CassetteScanProcessing(ib, slotParam, true);

                    DownScanResults.Add(SLOT, rel);
                }


                //for (int i = 0; i < slotCount; i++)
                foreach (var item in UpScanResults)
                {
                    var SLOT = item.Key;
                    var upRel = item.Value;

                    //string slotLabel = ProcVar.UpScanResults
                    //var upRel = ProcVar.UpScanResults[i];
                    var downRel = DownScanResults[SLOT];

                    double LTh = 0, LCen = 0, LScore = 0;
                    double RTh = 0, RCen = 0, RScore = 0;
                    int LRel, RRel; // -1 : ERROR, 0 : N, 1 : Y
                    SlotScanStateEnum scanRel = SlotScanStateEnum.UNKNOWN;

                    if (upRel.HasEdges)
                    {
                        RTh = upRel.ROE.Height;
                        RCen = (int)((upRel.ROE.Bottom - upRel.ROE.Top) * 0.5 + upRel.ROE.Y);
                        RScore = upRel.ROE.Width / Data.ROI.Width;

                        if (RScore >= scoreThreshold && RTh >= minThickPixel && RTh <= maxThickPixel)
                            RRel = 1;
                        else
                            RRel = -1;
                    }
                    else
                    {
                        RRel = 0;
                    }

                    if (downRel.HasEdges)
                    {
                        LTh = downRel.ROE.Height;
                        LCen = (int)((downRel.ROE.Bottom - downRel.ROE.Top) * 0.5 + downRel.ROE.Y);
                        LScore = downRel.ROE.Width / Data.ROI.Width;

                        if (LScore >= scoreThreshold && LTh >= minThickPixel && LTh <= maxThickPixel)
                            LRel = 1;
                        else
                            LRel = -1;
                    }
                    else
                    {
                        LRel = 0;
                    }

                    double centerDiffPixel = Math.Abs(LCen - RCen);

                    if (LRel == 1 && RRel == 1)
                    {
                        //if (centerDiffPixel <= elevationDiffPixelTol)
                        //{
                        scanRel = SlotScanStateEnum.DETECTED;
                        //}
                        //else
                        //{
                        //    scanRel = ScanResultEnum.ERROR;
                        //}
                    }
                    else if (LRel == 0 && RRel == 0)
                    {
                        scanRel = SlotScanStateEnum.NOT_DETECTED;
                    }
                    else
                    {
                        scanRel = SlotScanStateEnum.UNKNOWN;
                    }

                    LoggerManager.Debug($"{upRel.SlotLabel} : RESULT = {scanRel}, LTh = {LTh:f1}, LCen = {LCen:f1}, LScore = {(int)(LScore * 100)}%, RTh = {RTh:f1}, RCen = {RCen:f1}, RScore = {(int)(RScore * 100)}%, Center Diff = {centerDiffPixel:f1}");

                    scanRelDic.Add(SLOT, scanRel);
                }//end of for

                LoggerManager.Debug($"Threshold Information : Score : {scoreThreshold * 100}%, Min Thickness = {minThickPixel}, Max Thickness = {maxThickPixel}");

                //light off
                Loader.Light.SetLight(scanparam.LightChannel.Value, 0);

                //Update ScanResult
                Cassette.SetScanResult(scanRelDic);
                Loader.BroadcastLoaderInfo();

                if (Cassette.ScanState != CassetteScanStateEnum.READ)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Cassette.ScanState != CassetteScanStateEnum.READ State={Cassette.ScanState}");
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

    public class DoneState : CameraScanCassetteState
    {
        public DoneState(CameraScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
    }

    public class SystemErrorState : CameraScanCassetteState
    {
        public SystemErrorState(CameraScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class ReadScanFailState : CameraScanCassetteState
    {
        public ReadScanFailState(CameraScanCassette module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.SCAN_FAILED;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

}
