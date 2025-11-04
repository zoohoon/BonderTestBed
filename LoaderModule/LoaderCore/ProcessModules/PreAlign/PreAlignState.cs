using System;
using System.Collections.Generic;

using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using static ProberInterfaces.ModelFinderResult;

namespace LoaderCore.PreAlignStates
{
    public abstract class PreAlignState : LoaderProcStateBase
    {
        public PreAlign Module { get; set; }

        public PreAlignState(PreAlign module)
        {
            this.Module = module;
        }

        protected void StateTransition(PreAlignState stateObj)
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

        protected IPreAlignModule PA => Module.Param.UsePA as IPreAlignModule;

        protected IARMModule ARM => Module.Param.UseARM;

        protected PreAlignData Data => Module.Data;
    }

    public class IdleState : PreAlignState
    {
        public IdleState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
#if DEBUG
                Loader.ServiceCallback.UI_ShowLoaderCam();
#endif
                //=> Init internal process variable.
                Module.Data = new PreAlignData()
                {
                    Cam = GetCam(Module.Param.TransferObject.Size.Value),
                    //NotchHomeInput = GetNotchHomeInput(Module.Param.TransferObject.Size),
                    NotchHomeInput = PA.GetProcessingParam(Module.Param.TransferObject.Type.Value, Module.Param.TransferObject.Size.Value).SensorInputPort.Value,
                    AccessParam = PA.GetAccessParam(Module.Param.TransferObject.Type.Value, Module.Param.TransferObject.Size.Value),
                    ProcessingParam = PA.GetProcessingParam(Module.Param.TransferObject.Type.Value, Module.Param.TransferObject.Size.Value),
                    Radius = GetRaidus(Module.Param.TransferObject.Size.Value),
                    CenteringTriedCount = 0,
                    CenOffsetAngle = 0,
                    CenOffsetDist = 0,
                };

                //=> Light On
                Loader.Light.SetLight(Data.ProcessingParam.LightChannel.Value, Data.ProcessingParam.LightIntensity.Value);

                StateTransition(new PreAlignDownMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //StateTransition(new SystemErrorState(Module));
            }
        }

        private double GetRaidus(SubstrateSizeEnum size)
        {
            double INCH_TO_UM = 25400.0;

            try
            {
                switch (size)
                {
                    case SubstrateSizeEnum.INCH6: return 6.0 * INCH_TO_UM * 0.5;
                    case SubstrateSizeEnum.INCH8: return 200000 * 0.5;//8.0 * INCH_TO_UM;
                    case SubstrateSizeEnum.INCH12: return 12.0 * INCH_TO_UM * 0.5;
                }
                throw new NotImplementedException($"GetRaidus() : Undefined wafer size={size}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }
        }

        private EnumMotorDedicatedIn GetNotchHomeInput(SubstrateSizeEnum size)
        {
            try
            {
                switch (size)
                {
                    case SubstrateSizeEnum.INCH6:
                        return EnumMotorDedicatedIn.MotorDedicatedIn_1R;
                    case SubstrateSizeEnum.INCH8:
                        return EnumMotorDedicatedIn.MotorDedicatedIn_2R;
                    case SubstrateSizeEnum.INCH12:
                        return EnumMotorDedicatedIn.MotorDedicatedIn_3R;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        private EnumProberCam GetCam(SubstrateSizeEnum size)
        {
            try
            {
                switch (size)
                {
                    case SubstrateSizeEnum.INCH6:
                        return EnumProberCam.PACL6_CAM;
                    case SubstrateSizeEnum.INCH8:
                        return EnumProberCam.PACL8_CAM;
                    case SubstrateSizeEnum.INCH12:
                        return EnumProberCam.PACL12_CAM;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            throw new Exception("PA Cam is not defined");
        }
    }

    public class PreAlignDownMoveState : PreAlignState
    {
        public PreAlignDownMoveState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                PA.ValidateWaferStatus();
                if (PA.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the PreAlign.");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the PreAlign.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                ARM.ValidateWaferStatus();
                if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the ARM.");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = Loader.Move.PreAlignDownMove(ARM, PA);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignDownMove() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignDown Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                StateTransition(new CenteringState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class CenteringState : PreAlignState
    {
        public CenteringState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                ILoaderModule Loader = Module.Container.Resolve<ILoaderModule>();

                ILotOPModule lotmodule = Loader.StageContainer.Resolve<ILotOPModule>();

                lotmodule.LoaderScreenToLotScreen();

                Data.CenteringTriedCount += 1;
                //  string dirPath = @"C:\ProberSystem\PREALIGNTEST.txt";
                //if (Directory.Exists(Path.GetDirectoryName(dirPath)) == false)
                //{
                //    Directory.CreateDirectory(Path.GetDirectoryName(dirPath));
                //}
                //if (!File.Exists(dirPath))
                //{
                //    // Create a file to write to.
                //    using (StreamWriter sw = File.CreateText(dirPath))
                //    {
                //        sw.WriteLine("----PREALIGNTEST----");
                //    }
                //}

                //using (StreamWriter sw = File.AppendText(dirPath))
                //{
                //    sw.WriteLine($"Centering() : Start #{Data.CenteringTriedCount} Time:[" + DateTime.Now + "]");
                //}
                LoggerManager.Debug($"Centering() : Start #{Data.CenteringTriedCount}");

                Degree step_ang = -60.0;
                double step_dist = ConstantValues.V_DEGREE_TO_DIST * step_ang.Value;
                Degree remainAngle = -360;
                Degree tmp_ang = Data.ProcessingParam.CameraAngle.Value;
                Vector2[] foundPosArr = new Vector2[6];

                Vector2 detectedPos;
                bool isEdgeFound;
                EventCodeEnum retVal;

                //Emul Image
                if (Loader.VisionManager.ConfirmDigitizerEmulMode(Data.Cam))
                {
                    Loader.VisionManager.LoadImageFromFileToGrabber(@"C:\ProberSystem\EmulImages\PA\PAResult.bmp", Data.Cam);
                }

                debugImgs.Clear();
                for (int i = 0; i < foundPosArr.Length; i++)
                {
                    if (i > 0)
                    {
                        retVal = Loader.Move.PreAlignRelMove(PA, step_dist);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove() ReturnValue={retVal}");
                            Loader.ResonOfError = $"{Module.GetType().Name} PreAlignRelMove Motion Error";
                            StateTransition(new SystemErrorState(Module));
                            lotmodule.HiddenLoaderScreenToLotScreen();
                            return;
                        }

                        tmp_ang = tmp_ang - step_ang;
                        remainAngle = remainAngle - step_ang;
                    }

                    isEdgeFound = CalcPACenToDetectedPointVec(out detectedPos);
                    if (isEdgeFound == false)
                    {
                        for (int j = 0; j < debugImgs.Count; j++)
                        {
                            Loader.VisionManager.SaveImageBuffer(debugImgs[j], @"C:\ProberSystem\Default\Parameters\Loader\PreAlignImg" + j + ".bmp", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                        }
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} CalcPACenToDetectedPointVec() isEdgeFound={isEdgeFound}");
                        Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Edge Not Found";
                        StateTransition(new SystemErrorState(Module));
                        lotmodule.HiddenLoaderScreenToLotScreen();
                        return;
                    }
                    Loader.VisionManager.SaveImageBuffer(debugImgs[i], @"C:\ProberSystem\Default\Parameters\Loader\PreAlignImg" + i + ".bmp", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                    foundPosArr[i] = detectedPos.Rotated(tmp_ang);
                }

                retVal = Loader.Move.PreAlignRelMove(PA, remainAngle.Value * ConstantValues.V_DEGREE_TO_DIST);

                lotmodule.HiddenLoaderScreenToLotScreen();

                if (retVal != EventCodeEnum.NONE)
                {
                    for (int j = 0; j < debugImgs.Count; j++)
                    {
                        Loader.VisionManager.SaveImageBuffer(debugImgs[j], @"C:\ProberSystem\Default\Parameters\Loader\PreAlignImg" + j + ".bmp", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                    }
                    debugImgs.Clear();
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignRelMove Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                Vector2[] cenPosArr = new Vector2[2];
                double[] radArr = new double[2];
                for (int i = 0; i < cenPosArr.Length; i++)
                {
                    var p1 = foundPosArr[i + 0];
                    var p2 = foundPosArr[i + 2];
                    var p3 = foundPosArr[i + 4];

                    cenPosArr[i] = CalcCenter(p1, p2, p3);
                    radArr[i] = (cenPosArr[i] - p1).Length;
                    radArr[i] += (cenPosArr[i] - p2).Length;
                    radArr[i] += (cenPosArr[i] - p3).Length;
                    radArr[i] = radArr[i] / 3.0;
                }

                double tmpDist1 = Math.Abs(Data.Radius - radArr[0]);
                double tmpDist2 = Math.Abs(Data.Radius - radArr[1]);
                int calibIndex = tmpDist1 < tmpDist2 ? 0 : 1;

                double MAX_DIST = 15000.0;
                Vector2 foundPos = cenPosArr[calibIndex];
                double cenOffDist = foundPos.Length;//- Vector3.Zero).Length;(WaferCenter == zero)

                if (Math.Abs(foundPos.x) > MAX_DIST || Math.Abs(foundPos.y) > MAX_DIST)
                {
                    //using (StreamWriter sw = File.AppendText(dirPath))
                    //{
                    //    sw.WriteLine($"Centering() : CalcCenter failed. X={foundPos.x}, Y={foundPos.y} Time:[" + DateTime.Now + "]");
                    //}
                    LoggerManager.Debug($"Centering() : CalcCenter failed. X={foundPos.x}, Y={foundPos.y}");

                    if (Module.Data.CenteringTriedCount < PA.Definition.RetryCount.Value)
                    {
                        //=> Retry
                        StateTransition(new CenteringState(Module));
                    }
                    else
                    {
                        for (int j = 0; j < debugImgs.Count; j++)
                        {
                            Loader.VisionManager.SaveImageBuffer(debugImgs[j], @"C:\ProberSystem\Default\Parameters\Loader\PreAlignImg" + j + ".bmp", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                        }
                        debugImgs.Clear();
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} CenteringTriedCount={Module.Data.CenteringTriedCount} >= RetryCount={PA.Definition.RetryCount.Value}");
                        Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Centering Not Found";
                        StateTransition(new SystemErrorState(Module));
                    }
                }
                else
                {
                    LoggerManager.Debug($"Centering() : Distance = {cenOffDist}, Tolerance = {Data.ProcessingParam.CenteringTolerance.Value}");
                    if (cenOffDist > Data.ProcessingParam.CenteringTolerance.Value)
                    {
                        LoggerManager.Debug($"Centering() : Inaccurate.");
                        LoggerManager.Debug($"Centering() : Vector = ({foundPos.x:0.00}, {foundPos.y:0.00})");
                        Degree cenOffAng = new Radian(Math.Atan2(foundPos.y, foundPos.x));
                        Degree RotAngle = 180.0 - (cenOffAng.Value);  //(90.00) - cenOffAng;

                        if (cenOffDist > 15000)
                            cenOffDist = 15000;


                        Data.CenOffsetAngle = RotAngle.Value * ConstantValues.V_DEGREE_TO_DIST;
                        Data.CenOffsetDist = cenOffDist;
                        LoggerManager.Debug($"Centering() : CenOffAngle = {Data.CenOffsetAngle}");
                        LoggerManager.Debug($"Centering() : CenOffsetDist = {cenOffDist}");
                        //using (StreamWriter sw = File.AppendText(dirPath))
                        //{
                        //    sw.WriteLine($"Centering() : CenOffAngle = {Data.CenOffsetAngle} Time:[" + DateTime.Now + "]");
                        //    sw.WriteLine($"Centering() : CenOffsetDist = {cenOffDist} Time:[" + DateTime.Now + "]");
                        //}
                        StateTransition(new CalibrationMoveState(Module));
                    }
                    else
                    {
                        LoggerManager.Debug($"Centering() : Found Center.");
                        //using (StreamWriter sw = File.AppendText(dirPath))
                        //{
                        //    sw.WriteLine($"Centering() : Found Center. Time:["+DateTime.Now+"]");
                        //}
                        Loader.Light.SetLight(Data.ProcessingParam.LightChannel.Value, 0);
                        debugImgs.Clear();
                        if ((Loader as LoaderModule).CenteringTestFlag == true)
                        {
                            StateTransition(new IdleState(Module));
                        }
                        else
                        {
                            StateTransition(new NotchFindingState(Module));
                        }

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Vector2 CalcCenter(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            double xc = 0;

            double yc = 0;

            try
            {
                double d1 = (p2.x - p1.x) / (p2.y - p1.y);

                double d2 = (p3.x - p2.x) / (p3.y - p2.y);

                xc = ((p3.y - p1.y) + (p2.x + p3.x) * d2 - (p1.x + p2.x) * d1) / (2.0 * (d2 - d1));

                yc = -d1 * (xc - (p1.x + p2.x) / 2.0) + (p1.y + p2.y) / 2.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return new Vector2(xc, yc);
        }
        private List<ImageBuffer> debugImgs = new List<ImageBuffer>();
        private bool CalcPACenToDetectedPointVec(out Vector2 relVec)
        {
            bool isDetected = false;
            relVec = new Vector2();
            ImageBuffer imgBuf = new ImageBuffer();
            try
            {
                //imgBuf = Loader.VisionManager.SingleGrab(Data.Cam).DeepClone();
                Loader.VisionManager.SingleGrab(Data.Cam).CopyTo(imgBuf);
                debugImgs.Add(imgBuf);
                var edgeRel = Loader.VisionManager.FindPreAlignCenteringEdge(imgBuf, false);
                if (edgeRel.Edges.Count > 0)
                {
                    Vector2 imgCenPos = new Vector2();
                    imgCenPos.X = edgeRel.ImageSize.X * 0.5;
                    imgCenPos.Y = edgeRel.ImageSize.Y * 0.5;
                    if (edgeRel.ImageSize.X % 2 == 0)
                        imgCenPos.X -= 0.5;
                    if (edgeRel.ImageSize.Y % 2 == 0)
                        imgCenPos.Y -= 0.5;

                    double minDist = double.MaxValue;
                    Vector2 minDistDetectedPos = Vector2.Zero;
                    foreach (var pos in edgeRel.Edges)
                    {
                        var foundPos = new Vector2(pos.X, pos.Y);
                        double dist = (imgCenPos - foundPos).Length;

                        if (dist < minDist)
                        {
                            minDist = dist;
                            minDistDetectedPos = foundPos;
                        }
                    }

                    LoggerManager.Debug($"Centering() : Edge found. X={minDistDetectedPos.X}, Y={minDistDetectedPos.Y}");

                    Vector2 imgCenToDetectedPosVec = minDistDetectedPos - imgCenPos;

                    relVec.x = imgCenToDetectedPosVec.X * Data.ProcessingParam.CameraRatio.Value + Data.Radius;
                    relVec.y = 0;//** 

                    isDetected = true;
                }
                else
                {
                    isDetected = false;
                    relVec = Vector2.Zero;
                    LoggerManager.Error($"Centering() : Edge not found.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isDetected;
        }
    }

    public class CalibrationMoveState : PreAlignState
    {
        public CalibrationMoveState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //Move Offset Angle(V axis)
                retval = Loader.Move.PreAlignRelMove(PA, Data.CenOffsetAngle);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove(PA,Data.CenOffsetAngle) ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //Move Offset Dist(U axis)
                double adjustDistGain = 1.0;
                retval = Loader.Move.PreAlignRelMove(ARM, Data.CenOffsetDist * -1d * adjustDistGain);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove(ARM, Data.CenOffsetDist * -1d) ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                StateTransition(new PickUpState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PickUpState : PreAlignState
    {
        public PickUpState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = PA.WriteVacuum(false);
                retval = PA.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = ARM.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WriteVacuum() ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = Loader.Move.PickUp(ARM, PA);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PickUp(ARM, PA) ReturnValue={retval}");
                    PA.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PickUpErrorState(Module));
                    return;
                }

                retval = ARM.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum ReturnValue={retval}");
                    PA.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new ARMVacuumErrorState(Module));
                }

                PA.Holder.SetTransfered(ARM);
                Loader.BroadcastLoaderInfo();

                StateTransition(new CalibrationBackMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class CalibrationBackMoveState : PreAlignState
    {
        public CalibrationBackMoveState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // 신규 장비 추가(EnumLoaderMovingMethodType 추가)시 구분해서 처리해 주어야 함
                if (Loader.SystemParameter.LoaderMovingMethodType == LoaderParameters.EnumLoaderMovingMethodType.OPUSV_MINI)
                {
                    retval = Loader.Move.PreAlignRelMove(ARM, Data.CenOffsetDist * 1d);
                }

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove(ARM, Data.CenOffsetDist * 1d) ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                StateTransition(new PlaceDownState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PlaceDownState : PreAlignState
    {
        public PlaceDownState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = ARM.WriteVacuum(false);
                retval = ARM.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum(flase) ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = PA.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WriteVacuum(true) ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = Loader.Move.PlaceDown(ARM, PA);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PlaceDown(ARM, PA) ReturnValue={retval}");
                    ARM.Holder.SetUnknown();
                    PA.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PlaceDownErrorState(Module));
                    return;
                }

                retval = PA.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum(true) ReturnValue={retval}");
                    ARM.Holder.SetUnknown();
                    PA.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PreAlginVacuumErrorState(Module));
                    return;
                }

                ARM.Holder.SetTransfered(PA);
                Loader.BroadcastLoaderInfo();

                StateTransition(new CenteringState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class NotchFindingState : PreAlignState
    {
        public NotchFindingState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var VaxisObj = Loader.MotionManager.GetAxis(PA.Definition.AxisType.Value);
                double prevVpos = VaxisObj.Status.Position.Actual;

                retval = Loader.Move.FindNotchMove(PA, Data.NotchHomeInput);

                Module.Param.UsePA.Holder.TransferObject.NotchAngle.Value = 
                    PA.GetProcessingParam(
                        Module.Param.UsePA.Holder.TransferObject.Type.Value, 
                        Module.Param.UsePA.Holder.TransferObject.Size.Value).NotchSensorAngle.Value;
                LoggerManager.Debug($"After PA TO Notch Angle = {Module.Param.UsePA.Holder.TransferObject.NotchAngle.Value}");
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FindNotchMove() ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                PA.Holder.TransferObject.SetPreAlignDone(PA.ID);

#if DEBUG
                Loader.ServiceCallback.UI_HideLoaderCam();
#endif
                if (Module.Param.DestPos == this.PA)
                {
                    retval = Loader.Move.RetractAll();
                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retval}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DoneState : PreAlignState
    {
        public DoneState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : PreAlignState
    {
        public SystemErrorState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery()
        {
            try
            {
                StateTransition(new IdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PickUpErrorState : PreAlignState
    {
        public PickUpErrorState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PickUpErrorState");
                //=> Check Wafer Location On ARM
                bool isWaferOnARM;
                retval = ARM.WriteVacuum(true);
                retval = ARM.MonitorForVacuum(true);
                if (retval == EventCodeEnum.NONE)
                {
                    isWaferOnARM = true;
                }
                else
                {
                    isWaferOnARM = false;
                    retval = ARM.WriteVacuum(false);
                }

                //=> Check Wafer Location On PreAlign
                bool isWaferOnPA;
                retval = PA.WriteVacuum(true);
                retval = PA.MonitorForVacuum(true);
                if (retval == EventCodeEnum.NONE)
                {
                    isWaferOnPA = true;
                }
                else
                {
                    isWaferOnPA = false;
                    retval = PA.WriteVacuum(false);
                }

                if (isWaferOnARM == true && isWaferOnPA == false)
                {
                    //=> Recovered wafer location on ARM.
                    ARM.Holder.SetLoad(Module.Param.TransferObject);
                    PA.Holder.SetUnload();
                    Loader.BroadcastLoaderInfo();

                    return;
                }
                else if (isWaferOnARM == false && isWaferOnPA == true)
                {
                    //=> Recovered wafer location on PA.
                    ARM.Holder.SetUnload();
                    PA.Holder.SetLoad(Module.Param.TransferObject);
                    Loader.BroadcastLoaderInfo();

                    return;
                }
                else
                {
                    //=> failed.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class ARMVacuumErrorState : PreAlignState
    {
        public ARMVacuumErrorState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} ARMVacuumErrorState");
                //=> Place down move
                retval = ARM.WriteVacuum(false);
                retval = ARM.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = PA.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = Loader.Move.PlaceDown(ARM, PA, LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = PA.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovered wafer location.
                PA.Holder.SetLoad(Module.Param.TransferObject);
                ARM.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();

                //=> Retract ARM
                retval = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PlaceDownErrorState : PreAlignState
    {
        public PlaceDownErrorState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PlaceDownErrorState");
                //=> Check Wafer Location On ARM
                bool isWaferOnARM;
                retval = ARM.WriteVacuum(true);
                retval = ARM.MonitorForVacuum(true);
                if (retval == EventCodeEnum.NONE)
                {
                    isWaferOnARM = true;
                }
                else
                {
                    isWaferOnARM = false;
                    retval = ARM.WriteVacuum(false);
                }

                //=> Check Wafer Location On PreAlign
                bool isWaferOnPA;
                retval = PA.WriteVacuum(true);
                retval = PA.MonitorForVacuum(true);
                if (retval == EventCodeEnum.NONE)
                {
                    isWaferOnPA = true;
                }
                else
                {
                    isWaferOnPA = false;
                    retval = PA.WriteVacuum(false);
                }

                if (isWaferOnARM == true && isWaferOnPA == false)
                {
                    //=> Recovered wafer location on ARM.
                    ARM.Holder.SetLoad(Module.Param.TransferObject);
                    PA.Holder.SetUnload();
                    Loader.BroadcastLoaderInfo();

                    return;
                }
                else if (isWaferOnARM == false && isWaferOnPA == true)
                {
                    //=> Recovered wafer location on PA.
                    ARM.Holder.SetUnload();
                    PA.Holder.SetLoad(Module.Param.TransferObject);
                    Loader.BroadcastLoaderInfo();

                    return;
                }
                else
                {
                    //=> failed.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PreAlginVacuumErrorState : PreAlignState
    {
        public PreAlginVacuumErrorState(PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PreAlginVacuumErrorState");
                //=> Pick up move
                retval = PA.WriteVacuum(false);
                retval = PA.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = Loader.Move.PickUp(ARM, PA, LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovered wafer location.
                ARM.Holder.SetLoad(Module.Param.TransferObject);
                PA.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();

                //=> Retract ARM
                retval = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
