



using System;
using System.Collections.Generic;
using System.Linq;

namespace PolishWaferProcesserModule
{
    using ProberInterfaces;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.State;
    using LogModule;
    using Newtonsoft.Json;
    using ProberInterfaces.PolishWafer;
    using System.Windows;
    using ProberInterfaces.Param;
    using PolishWaferCenteringModule;
    using PolishWaferFocusingModule;
    using PolishWaferCleaningModule;
    using System.Threading;
    using ProberInterfaces.Event;
    using NotifyEventModule;
    using TouchSensorSystemParameter;
    using PolishWaferFocusingBySensorModule;

    public class PolishWaferProcessor_Standard : IProcessingModule, IPolishWaferProcessor, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        #region //..IProciessingModule Property
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }
        public bool Initialized { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }

        private SubModuleStateBase _SubModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _SubModuleState; }
            set
            {
                if (value != _SubModuleState)
                {
                    _SubModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        private IPolishWaferCentering _CenteringModule;

        public IPolishWaferCentering CenteringModule
        {
            get { return _CenteringModule; }
            set { _CenteringModule = value; }
        }

        private IPolishWaferFocusing _FocusingModule;

        public IPolishWaferFocusing FocusingModule
        {
            get { return _FocusingModule; }
            set { _FocusingModule = value; }
        }

        private IPolishWaferFocusingBySensor _FocusingBySensorModule;

        public IPolishWaferFocusingBySensor FocusingBySensorModule
        {
            get { return _FocusingBySensorModule; }
            set { _FocusingBySensorModule = value; }
        }

        private IPolishWaferCleaning _CleaningModule;

        public IPolishWaferCleaning CleaningModule
        {
            get { return _CleaningModule; }
            set { _CleaningModule = value; }
        }

        private TouchSensorSysParameter _TouchSensorParam;
        public TouchSensorSysParameter TouchSensorParam
        {
            get { return _TouchSensorParam; }
            set
            {
                if (value != _TouchSensorParam)
                {
                    _TouchSensorParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //..Init & DeInit Method
        public PolishWaferProcessor_Standard()
        {

        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SubModuleState = new SubModuleIdleState(this);
                CenteringModule = new PolishWaferCentering_Standard();
                FocusingModule = new PolishWaferFocusing_Standard();
                FocusingBySensorModule = new PolishWaferFocusingBySensor_Standard();
                CleaningModule = new PolishWaferCleaning_Standard();
                this.StageSupervisor().LoadTouchSensorObject();
                TouchSensorParam = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region //..IProcessingModule Method (Don't Touch)
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }
        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }
        public EventCodeEnum ExitRecovery()
        {
            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        #endregion

        #region //..IProcessing Method

        public EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsExecute()
        {
            bool retVal = false;
            try
            {
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IPolishWaferModule pwmoule = this.PolishWaferModule();
                //PolishWaferParameter pwparam = (this.PolishWaferModule().PolishWaferParameter as PolishWaferParameter);
                IPolishWaferCleaningParameter cleaningparam = null;
                PolishWafertCleaningInfo cleaninginfo = null;

                if (pwmoule.IsManualTriggered == false)
                {
                    cleaningparam = pwmoule.GetCurrentCleaningParam();
                    cleaninginfo = pwmoule.ProcessingInfo.GetCurrentCleaningInfo();
                }
                else
                {
                    cleaningparam = pwmoule.ManualCleaningParam;
                    cleaninginfo = pwmoule.ManualCleaningInfo;
                }

                if (cleaningparam == null)
                {
                    LoggerManager.Error($"[{this.GetType().Name}], DoExecute(), curcleaningapram is null.");
                }

                if (cleaninginfo?.PolishWaferCleaningProcessed == false)
                {
                    retVal = Processing(cleaningparam, cleaninginfo);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        this.NotifyManager().Notify(retVal);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {

            }
            return retVal;
        }

        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        #endregion

        public EventCodeEnum Processing(IPolishWaferCleaningParameter cleaningParameter, PolishWafertCleaningInfo cleaningInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //IPolishWaferModule pwmoule = this.PolishWaferModule();
                //IPolishWaferCleaningParameter curcleaningapram = null;

                //if (pwmoule.IsManualTriggered == false)
                //{
                //    curcleaningapram = pwmoule.GetCurrentIntervalParam()?.CurCleaningParameter;
                //}
                //else
                //{
                //    curcleaningapram = pwmoule.ManualCleaningParam;
                //}

                //if (curcleaningapram == null)
                //{
                //    LoggerManager.Error($"Unknwon Error.");
                //}

                this.LotOPModule().VisionScreenToLotScreen();

                if (cleaningInfo.PolishWaferCleaningProcessed == false)
                {
                    this.PolishWaferModule().CommandSendSlot.ClearToken();
                    if ((retVal = CenteringModule.DoCentering(cleaningParameter)) == EventCodeEnum.NONE)
                    {
                        TouchSensorParam = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;

                        if (TouchSensorParam.TouchSensorAttached.Value == true)
                        {
                            LoggerManager.Debug($"PolishWaferProcessorModule, Processing(): Touch Sensor Attached : {TouchSensorParam.TouchSensorAttached.Value}, Start Focusing By Sesnor");
                            retVal = FocusingBySensorModule.DoFocusing(cleaningParameter);
                        }
                        else
                        {
                            LoggerManager.Debug($"PolishWaferProcessorModule, Processing(): Touch Sensor Attached : {TouchSensorParam.TouchSensorAttached.Value}, Start Focusing By Wafer Cam");
                            retVal = FocusingModule.DoFocusing(cleaningParameter);
                        }

                        if (retVal == EventCodeEnum.NONE)
                        {
                            if ((retVal = CleaningModule.DoCleaning(cleaningParameter)) == EventCodeEnum.NONE)
                            {
                                SubModuleState = new SubModuleDoneState(this);
                                cleaningInfo.PolishWaferCleaningProcessed = true;
                                
                                // 성공적으로 수행했기 때문에, Angle을 업데이트 하기위해 플래그를 켜놓는다.
                                // Wafer Unload 시, 사용되는 플래그.
                                // TODO : Single 적용 필요
                                this.PolishWaferModule().NeedAngleUpdate = true;
                            }
                            else
                            {
                                //Cleaning Fail
                                SubModuleState = new SubModuleErrorState(this);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CleaningFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                semaphore.Wait();
                            }
                            this.StageSupervisor().StageModuleState.ZCLEARED();
                        }
                        else
                        {
                            //Focusing Fail
                            SubModuleState = new SubModuleErrorState(this);


                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CleaningFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                            semaphore.Wait();
                        }
                    }
                    else
                    {
                        //Centering Fail
                        SubModuleState = new SubModuleErrorState(this);


                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CleaningFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }
                }
                else
                {
                    if (cleaningInfo.PolishWaferCleaningRetry)
                    {
                        LoggerManager.Debug($"[PolishWaferProcessor_Standard] Processing() : Polish Wafer Cleaning Processed, " +
                            $"PinAlign After Cleaning Trigger = {cleaningParameter.PinAlignAfterCleaning}, PinAlign After Cleaning Processed = {cleaningInfo.PinAlignAfterCleaningProcessed}" +
                            $"PinAlign Before Cleaning Trigger = {cleaningParameter.PinAlignBeforeCleaning} , PinAlign Before Cleaning Processed = {cleaningInfo.PinAlignBeforeCleaningProcessed}");

                        retVal = EventCodeEnum.NONE;

                        SubModuleState = new SubModuleDoneState(this);
                        cleaningInfo.PolishWaferCleaningProcessed = true;

                        // 성공적으로 수행했기 때문에, Angle을 업데이트 하기위해 플래그를 켜놓는다.
                        // Wafer Unload 시, 사용되는 플래그.
                        // TODO : Single 적용 필요
                        this.PolishWaferModule().NeedAngleUpdate = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.LotOPModule().MapScreenToLotScreen();
            }

            return retVal;
        }

        #region //..Edge Detection
        private List<WaferProcResult> procresults = new List<WaferProcResult>();
        private List<WaferCoordinate> _EdgePos = new List<WaferCoordinate>();
        public List<WaferCoordinate> EdgePos
        {
            get { return _EdgePos; }
            set { _EdgePos = value; }
        }


        private EventCodeEnum EdgeDetection()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            WaferCoordinate wafercoord = null;
            ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
            try
            {
                #region //..[Trun Theta 0 ] 
                double curtpos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                curtpos = Math.Abs(curtpos);

                int converttpos = Convert.ToInt32(curtpos);

                if (converttpos != 0)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                }
                #endregion

                procresults.Clear();

                ICamera CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM, this);

                double wSize = this.GetParam_Wafer().GetPhysInfo().WaferSize_um.Value;
                double edgepos = 0.0;
                edgepos = ((wSize / 2) / Math.Sqrt(2));

                EdgePos.Clear();
                EdgePos.Add(new WaferCoordinate(edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, -edgepos));
                EdgePos.Add(new WaferCoordinate(edgepos, -edgepos));

                var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);


                this.MotionManager().SetSettlingTime(axisX, 0.001);
                this.MotionManager().SetSettlingTime(axisY, 0.001);

                //if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP)
                //{
                //    //
                //    EdgeStandardParam_Clone.LightParams.Clear();
                //    for (int index = 0; index < CurCam.LightsChannels.Count; index++)
                //    {

                //        EdgeStandardParam_Clone.LightParams.Add(
                //            new LightValueParam(CurCam.LightsChannels[index].Type.Value,
                //            (ushort)CurCam.GetLight(CurCam.LightsChannels[index].Type.Value)));
                //    }

                //}

                //foreach (var light in EdgeStandardParam_Clone.LightParams)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}

                ImageBuffer[] EdgeBuffer = new ImageBuffer[EdgePos.Count];
                ImageBuffer[] EdgeLineBuffer = new ImageBuffer[EdgePos.Count];

                this.VisionManager().StopGrab(CurCam.GetChannelType());

                for (int index = 0; index < EdgePos.Count; index++)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[index].X.Value, EdgePos[index].Y.Value, this.GetParam_Wafer().GetPhysInfo().Thickness.Value);

                    if (this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                    {
                        this.VisionManager().LoadImageFromFileToGrabber(@"C:\ProberSystem\EmulImages\WaferAlign\Edge\Edge" + index + ".bmp", CurCam.GetChannelType());
                    }

                    EdgeBuffer[index] = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                    if (!this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                    {
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                    }

                    string edgepath = @"C:\Logs\Image\EdgeImage\Edge" + index + ".bmp";
                    this.VisionManager().SaveImageBuffer(EdgeBuffer[index], edgepath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                    EdgeLineBuffer[index] = this.VisionManager().Line_Equalization(EdgeBuffer[index], index);

                }

                if (EdgeLineBuffer.ToList<ImageBuffer>().FindAll(img => img != null).Count() != 0)
                {
                    int Except_Pixel = 30;
                    int Acc_Pixel = 20;
                    int Cmp_Pixel = 10;

                    double Temp_Sum = 0;
                    double Temp_Avg = 0;

                    int Width = EdgeLineBuffer[0].SizeX;
                    int Heigh = EdgeLineBuffer[0].SizeY;

                    int RWidth = EdgeLineBuffer[0].SizeX - 1;
                    int RHeight = EdgeLineBuffer[0].SizeY - 1;

                    double[,] EdgeOVal = new double[EdgePos.Count, Width];

                    double TempOval = 0;
                    int TempOvalPos = 0;

                    double Threshold = 20;

                    int i, ii, j, k, kk, m;

                    int alphaS, BetaS;
                    int alphaE, BetaE;

                    int PMFLAG;

                    for (k = 0; k < EdgePos.Count; k++)
                    {
                        if ((k == 1) || (k == 2))
                        {
                            alphaS = 0;
                            alphaE = RWidth;
                        }
                        else
                        {
                            alphaS = RWidth;
                            alphaE = 0;
                        }

                        if ((k == 0) || (k == 1))
                        {
                            BetaS = 0;
                            BetaE = RHeight;

                            PMFLAG = 1;
                        }
                        else
                        {
                            BetaS = RHeight;
                            BetaE = 0;

                            PMFLAG = -1;
                        }

                        //data_VBcvEdgeFindFEX_Line_Avg = (uchar*)VBcvEdgeFindFEX_Line_Avg[k]->imageData;

                        j = BetaS + (Except_Pixel * PMFLAG);

                        // k = 0, 3
                        if (alphaS > alphaE)
                        {
                            for (i = (alphaS - Except_Pixel); i >= (alphaE + 1); i--)
                            {
                                Temp_Sum = 0;

                                for (kk = 1; kk <= Acc_Pixel; kk++)
                                {
                                    Temp_Sum += EdgeLineBuffer[k].Buffer[(j - (kk * PMFLAG)) * Width + (i + kk)];
                                }

                                Temp_Avg = Temp_Sum / Acc_Pixel;

                                if ((i > Except_Pixel) && (i < Width - Except_Pixel))
                                {
                                    EdgeOVal[k, RWidth - i] = Temp_Avg - EdgeLineBuffer[k].Buffer[j * Width + i];
                                }

                                j = j + PMFLAG;
                            }
                        }

                        // k = 1, 2
                        else
                        {
                            for (i = (alphaS + Except_Pixel); i <= (alphaE - 1); i++)
                            {
                                Temp_Sum = 0;

                                for (kk = 1; kk <= Acc_Pixel; kk++)
                                {
                                    Temp_Sum += EdgeLineBuffer[k].Buffer[(j - (kk * PMFLAG)) * Width + (i - kk)];
                                }

                                Temp_Avg = Temp_Sum / Acc_Pixel;

                                if ((i > Except_Pixel) && (i < Width - Except_Pixel))
                                {
                                    EdgeOVal[k, i] = Temp_Avg - EdgeLineBuffer[k].Buffer[j * Width + i];
                                }

                                j = j + PMFLAG;
                            }
                        }
                    }

                    double P_RSum = 0;
                    double P_LSum = 0;
                    double P_RAvg = 0;
                    double P_LAvg = 0;

                    int sx, sy;

                    int MaxDiffValue = 0;

                    Point TempPos = new Point();

                    int ret_Edge0_count = 0;
                    int ret_Edge1_count = 0;
                    int ret_Edge2_count = 0;
                    int ret_Edge3_count = 0;

                    int rsize = 32;

                    int[] ret_PMEdge0 = new int[rsize];
                    int[] ret_PMEdge1 = new int[rsize];
                    int[] ret_PMEdge2 = new int[rsize];
                    int[] ret_PMEdge3 = new int[rsize];

                    Point[] ret_Edge0 = new Point[rsize];
                    Point[] ret_Edge1 = new Point[rsize];
                    Point[] ret_Edge2 = new Point[rsize];
                    Point[] ret_Edge3 = new Point[rsize];

                    sx = Width;
                    sy = Heigh;

                    for (k = 0; k < EdgePos.Count; k++)
                    {
                        for (i = Except_Pixel; i < RWidth - Except_Pixel; i++)
                        {
                            if (Math.Abs(EdgeOVal[k, i]) > Threshold)
                            {
                                TempOval = 0;

                                for (j = -Cmp_Pixel; j < Cmp_Pixel; j++)
                                {
                                    if (Math.Abs(EdgeOVal[k, i + j]) > TempOval)
                                    {
                                        TempOval = Math.Abs(EdgeOVal[k, i + j]);
                                        TempOvalPos = i + j;
                                    }
                                }

                                if (Math.Abs(EdgeOVal[k, TempOvalPos - 1]) > (Threshold * 0.8) &&
                                    Math.Abs(EdgeOVal[k, TempOvalPos + 1]) > (Threshold * 0.8))
                                {

                                    if (TempOvalPos == i)
                                    {
                                        if (k == 0)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(i - kk) * Width + (RWidth - i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(i + kk) * Width + (RWidth - i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (k == 1)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(i + kk) * Width + (i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(i - kk) * Width + (i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (k == 2)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(RHeight - i - kk) * Width + (i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(RHeight - i + kk) * Width + (i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (k == 3)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(RHeight - i + kk) * Width + (RWidth - i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(RHeight - i - kk) * Width + (RWidth - i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (Math.Abs(P_RAvg - P_LAvg) > 10)
                                        {
                                            if ((i > (Width / 2 - 1) - (sx / 2 - 1)) && (i < (Width / 2 - 1) - (sx / 2 - 1) + sx))
                                            {
                                                MaxDiffValue = 0;

                                                if (k == 0)
                                                {
                                                    for (m = -5; m < 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + ((RWidth) - (i + m + 1))]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + ((RWidth) - (i + m + 1))]);

                                                            TempPos.X = (RWidth) - (i + m + 1);
                                                            TempPos.Y = (i + m + 1);
                                                        }
                                                    }

                                                    if (ret_Edge0_count == rsize)
                                                    {
                                                        rsize *= 2;

                                                        //ret_Edge0 = (CvPoint*)realloc(ret_Edge0, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge0[ret_Edge0_count] = TempPos;
                                                    ret_PMEdge0[ret_Edge0_count] = (int)EdgeOVal[k, (int)(RWidth - TempPos.X)];
                                                    ret_Edge0_count++;
                                                }
                                                else if (k == 1)
                                                {
                                                    for (m = -5; m <= 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + (i + m + 1)]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + (i + m + 1)]);

                                                            TempPos.X = (i + m + 1);
                                                            TempPos.Y = (i + m + 1);
                                                        }
                                                    }

                                                    if (ret_Edge1_count == rsize)
                                                    {
                                                        rsize *= 2;

                                                        //ret_Edge1 = (CvPoint*)realloc(ret_Edge1, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge1[ret_Edge1_count] = TempPos;
                                                    ret_PMEdge1[ret_Edge1_count] = (int)EdgeOVal[k, (int)TempPos.X];
                                                    ret_Edge1_count++;
                                                }
                                                else if (k == 2)
                                                {
                                                    for (m = -5; m <= 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + (i + m + 1)]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + (i + m + 1)]);

                                                            TempPos.X = (i + m + 1);
                                                            TempPos.Y = (RHeight) - (i + m + 1);
                                                        }

                                                    }

                                                    if (ret_Edge2_count == rsize)
                                                    {
                                                        rsize *= 2;

                                                        //ret_Edge2 = (CvPoint*)realloc(ret_Edge2, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge2[ret_Edge2_count] = TempPos;
                                                    ret_PMEdge2[ret_Edge2_count] = (int)EdgeOVal[k, (int)TempPos.X];
                                                    ret_Edge2_count++;
                                                }
                                                else if (k == 3)
                                                {
                                                    for (m = -5; m <= 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + ((RWidth) - (i + m + 1))]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + ((RWidth) - (i + m + 1))]);

                                                            TempPos.X = (RWidth) - (i + m + 1);
                                                            TempPos.Y = (RHeight) - (i + m + 1);
                                                        }
                                                    }

                                                    if (ret_Edge3_count == rsize)
                                                    {
                                                        rsize *= 3;

                                                        //ret_Edge3 = (CvPoint*)realloc(ret_Edge3, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge3[ret_Edge3_count] = TempPos;
                                                    ret_PMEdge3[ret_Edge3_count] = (int)EdgeOVal[k, (int)(RWidth - TempPos.X)];
                                                    ret_Edge3_count++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ((ret_Edge0_count <= 0) || (ret_Edge1_count <= 0) || (ret_Edge2_count <= 0) || (ret_Edge3_count <= 0))
                    {
                        SaveEdgeFailImage(EdgeBuffer, EdgeLineBuffer);
                    }

                    int p0, p1, p2, p3;

                    int p_0_2;
                    int p_1_3;

                    double Len1, Len2;

                    bool DIAGFLAG0to2 = false;
                    bool DIAGFLAG1to3 = false;

                    int Interval_DiagPoint = 80;

                    int All_Count;

                    All_Count = ret_Edge0_count * ret_Edge1_count * ret_Edge2_count * ret_Edge3_count;

                    Point[,] Real_Pos = new Point[EdgePos.Count, All_Count];

                    //Point[] Real_Pos0 = new Point[All_Count];
                    //Point[] Real_Pos1 = new Point[All_Count];
                    //Point[] Real_Pos2 = new Point[All_Count];
                    //Point[] Real_Pos3 = new Point[All_Count];

                    double[] Real_Score = new double[All_Count];

                    int Real_Count = 0;

                    double Symmetry_Score;

                    for (p0 = 0; p0 < ret_Edge0_count; p0++)
                    {
                        for (p1 = 0; p1 < ret_Edge1_count; p1++)
                        {
                            for (p2 = 0; p2 < ret_Edge2_count; p2++)
                            {
                                for (p3 = 0; p3 < ret_Edge3_count; p3++)
                                {
                                    if (((ret_PMEdge0[p0] > 0) && (ret_PMEdge1[p1] > 0) && (ret_PMEdge2[p2] > 0) && (ret_PMEdge3[p3] > 0)) ||
                                        ((ret_PMEdge0[p0] < 0) && (ret_PMEdge1[p1] < 0) && (ret_PMEdge2[p2] < 0) && (ret_PMEdge3[p3] < 0))
                                        )
                                    {
                                        Len1 = Math.Abs((ret_Edge0[p0].X + Width) - ret_Edge2[p2].X);
                                        Len2 = Math.Abs(ret_Edge1[p1].X - (ret_Edge3[p3].X + Width));

                                        if (Math.Abs(Len1 - Len2) < 50)
                                        {
                                            DIAGFLAG0to2 = false;
                                            DIAGFLAG1to3 = false;

                                            // Check  Diag Point (0) and (2), (1) and (3)

                                            for (p_0_2 = 0; p_0_2 < ret_Edge2_count; p_0_2++)
                                            {
                                                if ((((Width + ret_Edge0[p0].X) - (ret_Edge2[p_0_2].X)) < Width + Interval_DiagPoint))
                                                {
                                                    DIAGFLAG0to2 = true;
                                                }
                                            }

                                            for (p_1_3 = 0; p_1_3 < ret_Edge3_count; p_1_3++)
                                            {
                                                if ((((Width + ret_Edge3[p_1_3].X) - (ret_Edge1[p1].X)) < Width + Interval_DiagPoint))
                                                {
                                                    DIAGFLAG1to3 = true;
                                                }
                                            }

                                            if ((DIAGFLAG0to2 == true) && (DIAGFLAG1to3 == true))
                                            {
                                                Symmetry_Score = 0;

                                                for (int mm = 0; mm < 25; mm++)
                                                {
                                                    if ((ret_Edge0[p0].X > Except_Pixel) && (ret_Edge0[p0].X < Width - Except_Pixel) &&
                                                    (ret_Edge1[p1].X > Except_Pixel) && (ret_Edge1[p1].X < Width - Except_Pixel) &&
                                                    (ret_Edge2[p2].X > Except_Pixel) && (ret_Edge2[p2].X < Width - Except_Pixel) &&
                                                    (ret_Edge3[p3].X > Except_Pixel) && (ret_Edge3[p3].X < Width - Except_Pixel) &&
                                                    (ret_Edge0[p0].Y > Except_Pixel) && (ret_Edge0[p0].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge1[p1].Y > Except_Pixel) && (ret_Edge1[p1].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge2[p2].Y > Except_Pixel) && (ret_Edge2[p2].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge3[p3].Y > Except_Pixel) && (ret_Edge3[p3].Y < Heigh - Except_Pixel))
                                                    {
                                                        Symmetry_Score += Math.Abs(EdgeLineBuffer[0].Buffer[((int)ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[0].Buffer[(int)(ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[0].Buffer[(int)(ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)] - EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y - mm) * Width + ((int)ret_Edge3[p3].X - mm)]);
                                                    }
                                                    else
                                                    {
                                                        Symmetry_Score = 99999;
                                                    }
                                                }

                                                Real_Pos[0, Real_Count].X = ret_Edge0[p0].X;
                                                Real_Pos[0, Real_Count].Y = ret_Edge0[p0].Y;

                                                Real_Pos[1, Real_Count].X = ret_Edge1[p1].X;
                                                Real_Pos[1, Real_Count].Y = ret_Edge1[p1].Y;

                                                Real_Pos[2, Real_Count].X = ret_Edge2[p2].X;
                                                Real_Pos[2, Real_Count].Y = ret_Edge2[p2].Y;

                                                Real_Pos[3, Real_Count].X = ret_Edge3[p3].X;
                                                Real_Pos[3, Real_Count].Y = ret_Edge3[p3].Y;

                                                Real_Score[Real_Count] = Symmetry_Score;

                                                Real_Count++;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (i = 0; i < Real_Count - 1; i++)
                    {
                        for (ii = 1; ii < Real_Count - i; ii++)
                        {
                            if (Real_Score[ii - 1] > Real_Score[ii])
                            {
                                SWAP(Real_Pos[0, ii - 1].X, Real_Pos[0, ii].X);
                                SWAP(Real_Pos[0, ii - 1].Y, Real_Pos[0, ii].Y);

                                SWAP(Real_Pos[1, ii - 1].X, Real_Pos[1, ii].X);
                                SWAP(Real_Pos[1, ii - 1].Y, Real_Pos[1, ii].Y);

                                SWAP(Real_Pos[2, ii - 1].X, Real_Pos[2, ii].X);
                                SWAP(Real_Pos[2, ii - 1].Y, Real_Pos[2, ii].Y);

                                SWAP(Real_Pos[3, ii - 1].X, Real_Pos[3, ii].X);
                                SWAP(Real_Pos[3, ii - 1].Y, Real_Pos[3, ii].Y);

                                SWAP(Real_Score[ii - 1], Real_Score[ii]);
                            }
                        }
                    }

                    if (Real_Count > 0)
                    {

                        for (kk = 0; kk < Real_Count; kk++)
                        {
                            for (k = 0; k < EdgePos.Count; k++)
                            {
                                double offsetx = 0;
                                double offsety = 0;

                                //offsetx = (CurCam.GetGrabSizeWidth() / 2) - Math.Abs(Real_Pos[k, kk].X);
                                //offsety = (CurCam.GetGrabSizeHeight() / 2) - Math.Abs(Real_Pos[k, kk].Y);
                                offsetx = Real_Pos[k, kk].X - (CurCam.GetGrabSizeWidth() / 2);
                                offsety = (CurCam.GetGrabSizeHeight() / 2) - Real_Pos[k, kk].Y;

                                if (Real_Pos[k, kk].X != 0 && Real_Pos[k, kk].Y != 0)
                                {
                                    LoggerManager.Debug($"Pixel X : {Real_Pos[k, kk].X} , Pixel Y :{Real_Pos[k, kk].Y}", isInfo: true);
                                }

                                offsetx *= CurCam.GetRatioX();
                                offsety *= CurCam.GetRatioY();


                                this.MotionManager().SetSettlingTime(axisX, 0.00001);
                                this.MotionManager().SetSettlingTime(axisY, 0.00001);

                                wafercoord = new WaferCoordinate();

                                wafercoord.X.Value = (EdgePos[k].X.Value + offsetx);
                                wafercoord.Y.Value = (EdgePos[k].Y.Value + offsety);

                                retVal = EventCodeEnum.NONE;

                                procresults.Add(new WaferProcResult(wafercoord, retVal));
                            }

                            retVal = Calculation();

                            if (retVal == EventCodeEnum.NONE)
                            {
                                break;
                            }
                            else
                            {
                                SaveEdgeFailImage(EdgeBuffer, EdgeLineBuffer);
                            }
                        }
                    }
                    else
                    {

                        retVal = EventCodeEnum.WAFER_EDGE_NOT_FOUND;

                        procresults.Add(new WaferProcResult(wafercoord, retVal));
                    }
                }
                else
                {
                    retVal = EventCodeEnum.WAFER_EDGE_NOT_FOUND;
                }
                //});
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. EdgeStndard - Processing() : Error occured.");
            }
            finally
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    try
                    {
                        retVal = EventCodeEnum.NONE;

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                }
            }

            return retVal;
        }

        private void SaveEdgeFailImage(ImageBuffer[] EdgeBuffer, ImageBuffer[] EdgeLineBuffer)
        {
            try
            {
                for (int index = 0; index < EdgeBuffer.Length; index++)
                {
                    string path = this.FileManager().GetImageSaveFullPath(EnumProberModule.POLISHWAFER, IMAGE_SAVE_TYPE.BMP, true, "WaferEdge", "FailImage", $"Edge_[{index}]");
                    this.VisionManager().SaveImageBuffer(EdgeBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                }

                for (int index = 0; index < EdgeLineBuffer.Length; index++)
                {
                    string path = this.FileManager().GetImageSaveFullPath(EnumProberModule.POLISHWAFER, IMAGE_SAVE_TYPE.BMP, true, "WaferEdge", "FailImage", $"EdgeLine_[{index}]");
                    this.VisionManager().SaveImageBuffer(EdgeLineBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EventCodeEnum Calculation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    double a = 0.0;
                    double b = 0.0;
                    double c = 0.0;
                    double d = 0.0;
                    double e = 0.0;
                    double f = 0.0;

                    double chuckzeroAveXpos = 0.0;
                    double chuckzeroAveYpos = 0.0;

                    Point[] tmpGCPWaferCen = new Point[4];

                    LoggerManager.Debug($"Q1 xpos:{procresults[0].ResultPos.X.Value} ypos{procresults[0].ResultPos.Y.Value}", isInfo: true);
                    LoggerManager.Debug($"Q2 xpos:{procresults[1].ResultPos.X.Value} ypos{procresults[1].ResultPos.Y.Value}", isInfo: true);
                    LoggerManager.Debug($"Q3 xpos:{procresults[2].ResultPos.X.Value} ypos{procresults[2].ResultPos.Y.Value}", isInfo: true);
                    LoggerManager.Debug($"Q4 xpos:{procresults[3].ResultPos.X.Value} ypos{procresults[3].ResultPos.Y.Value}", isInfo: true);

                    double distancex = 2 * (procresults[1].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                    double distancey = 2 * (procresults[1].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);

                    //case1

                    a = 2 * (procresults[1].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                    b = 2 * (procresults[1].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);
                    c = Math.Pow(procresults[1].ResultPos.X.Value, 2.0) - Math.Pow(procresults[0].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[1].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[0].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[2].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                    e = 2 * (procresults[2].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);
                    f = Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[0].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[2].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[0].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[0].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[0].Y = ((c * d - a * f) / (d * b - a * e));

                    //case2
                    a = 2 * (procresults[2].ResultPos.X.Value - procresults[1].ResultPos.X.Value);
                    b = 2 * (procresults[2].ResultPos.Y.Value - procresults[1].ResultPos.Y.Value);
                    c = Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[3].ResultPos.X.Value - procresults[1].ResultPos.X.Value);
                    e = 2 * (procresults[3].ResultPos.Y.Value - procresults[1].ResultPos.Y.Value);
                    f = Math.Pow(procresults[3].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[3].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[1].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[1].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[1].Y = ((c * d - a * f) / (d * b - a * e));

                    //case3
                    a = 2 * (procresults[3].ResultPos.X.Value - procresults[2].ResultPos.X.Value);
                    b = 2 * (procresults[3].ResultPos.Y.Value - procresults[2].ResultPos.Y.Value);
                    c = Math.Pow(procresults[3].ResultPos.X.Value, 2.0) - Math.Pow(procresults[2].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[3].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[2].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[0].ResultPos.X.Value - procresults[2].ResultPos.X.Value);
                    e = 2 * (procresults[0].ResultPos.Y.Value - procresults[2].ResultPos.Y.Value);
                    f = Math.Pow(procresults[0].ResultPos.X.Value, 2.0) - Math.Pow(procresults[2].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[0].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[2].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[2].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[2].Y = ((c * d - a * f) / (d * b - a * e));

                    //case4
                    a = 2 * (procresults[0].ResultPos.X.Value - procresults[3].ResultPos.X.Value);
                    b = 2 * (procresults[0].ResultPos.Y.Value - procresults[3].ResultPos.Y.Value);
                    c = Math.Pow(procresults[0].ResultPos.X.Value, 2.0) - Math.Pow(procresults[3].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[0].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[3].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[1].ResultPos.X.Value - procresults[3].ResultPos.X.Value);
                    e = 2 * (procresults[1].ResultPos.Y.Value - procresults[3].ResultPos.Y.Value);
                    f = Math.Pow(procresults[1].ResultPos.X.Value, 2.0) - Math.Pow(procresults[3].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[1].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[3].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[3].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[3].Y = ((c * d - a * f) / (d * b - a * e));

                    bool[] CEN_CHECK_FLAG = new bool[2];

                    CEN_CHECK_FLAG[0] = true;
                    CEN_CHECK_FLAG[1] = true;


                    if ((CEN_CHECK_FLAG[0] == true) && (CEN_CHECK_FLAG[1] == true))
                    {
                        chuckzeroAveXpos = (tmpGCPWaferCen[0].X + tmpGCPWaferCen[1].X + tmpGCPWaferCen[2].X + tmpGCPWaferCen[3].X) / 4;
                        chuckzeroAveYpos = (tmpGCPWaferCen[0].Y + tmpGCPWaferCen[1].Y + tmpGCPWaferCen[2].Y + tmpGCPWaferCen[3].Y) / 4;

                        LoggerManager.Debug($"Wafer Center xpos:{chuckzeroAveXpos} ypos{chuckzeroAveYpos}", isInfo: true);

                        double[] CLength = new double[procresults.Count()];
                        double[] CLengthRDiff = new double[procresults.Count()];
                        double lRadius;

                        lRadius = Math.Sqrt(Math.Pow((EdgePos[2].X.Value - EdgePos[0].X.Value), 2) + Math.Pow((EdgePos[2].Y.Value - EdgePos[0].Y.Value), 2)) / 2.0;

                        for (int i = 0; i < procresults.Count(); i++)
                        {
                            CLength[i] = Math.Sqrt(Math.Pow((procresults[i].ResultPos.X.Value - chuckzeroAveXpos), 2) + Math.Pow((procresults[i].ResultPos.Y.Value - chuckzeroAveYpos), 2));
                            CLengthRDiff[i] = Math.Abs(lRadius - CLength[i]);
                        }

                        LoggerManager.Debug($"Distance Q1 : {CLength[0]}", isInfo: true);
                        LoggerManager.Debug($"Distance Q2 : {CLength[1]}", isInfo: true);
                        LoggerManager.Debug($"Distance Q3 : {CLength[2]}", isInfo: true);
                        LoggerManager.Debug($"Distance Q4 : {CLength[3]}", isInfo: true);

                        //if ((CLengthRDiff[0] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        //    (CLengthRDiff[1] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        //    (CLengthRDiff[2] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        //    (CLengthRDiff[3] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value))
                        //{
                        this.GetParam_Wafer().GetSubsInfo().WaferCenter.X.Value = chuckzeroAveXpos;
                        this.GetParam_Wafer().GetSubsInfo().WaferCenter.Y.Value = chuckzeroAveYpos;

                        this.GetParam_Wafer().GetSubsInfo().WaferCenterOriginatEdge.X.Value = chuckzeroAveXpos;
                        this.GetParam_Wafer().GetSubsInfo().WaferCenterOriginatEdge.Y.Value = chuckzeroAveYpos;

                        //Wafer.GetSubsInfo().WaferCenter.X.Value = chuckzeroAveXpos;
                        //    Wafer.GetSubsInfo().WaferCenter.Y.Value = chuckzeroAveYpos;
                        WaferCoordinate coordinate =
                            this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();


                        retVal = EventCodeEnum.NONE;
                        //}
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"{err.ToString()}. EdgeStndard - Calculation() : Error occured.");
                    throw err;
                }
                finally
                {
                    //ProcessDialog.CloseDialg(this);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void SWAP(double a, double b)
        {
            try
            {
                double temp = a;
                a = b;
                b = temp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion
        public EventCodeEnum DoCentering(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CenteringModule != null)
                {
                    retval = CenteringModule.DoCentering(param);
                }
                else
                {
                    retval = EventCodeEnum.UNDEFINED;
                    LoggerManager.Error("CenteringModule is not loaded.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                TouchSensorParam = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;

                if (TouchSensorParam.TouchSensorAttached.Value == true)
                {
                    if (FocusingBySensorModule != null)
                    {
                        retval = FocusingBySensorModule.DoFocusing(param);
                    }
                    else
                    {
                        LoggerManager.Error("FocusingUsingTouchSensorModule is not loaded.");
                    }
                }
                else
                {
                    if (FocusingModule != null)
                    {
                        retval = FocusingModule.DoFocusing(param);
                    }
                    else
                    {
                        LoggerManager.Error("FocusingModule is not loaded.");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CleaningModule != null)
                {
                    retval = CleaningModule.DoCleaning(param);
                }
                else
                {
                    LoggerManager.Error("CleaningModule is not loaded.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
