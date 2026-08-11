using System;
using System.Collections.Generic;
using System.Linq;

namespace Focusing
{
    using FocusGraphControl;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Focus;
    using ProberInterfaces.Param;
    using SciChart.Charting.Model.DataSeries;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    [Serializable]
    public class NormalFocusing : FocusingBase, INotifyPropertyChanged, ICoaxLinkExFocusing
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        ManualResetEvent mreUpdateEvent = new ManualResetEvent(false);

        public override Type ParamType { get; set; } = typeof(NormalFocusParameter);

        //private LockKey lockObject = new LockKey("Normal Focusing");
        private object lockObject = new object();

        private bool IsInfo = false;

        public NormalFocusing()
        {

        }

        FocusGraph focusGraph = null;

        public override void ShowFocusGraph()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (focusGraph != null)
                    {
                        focusGraph.Activate();
                        return;
                    }

                    focusGraph = new FocusGraph();
                    // graphX.Owner = Model.ProberMain;
                    focusGraph.Closed += (o, args) => focusGraph = null;
                    focusGraph.Show();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum WriteFocusingInfo(List<ImageBuffer> images, double range, double resolution, int step)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (images != null && images.Count > 0)
                {
                    LoggerManager.Debug($"[NormalFocusing], WriteFocuisngInfo() : Range = {range} | Resolution = {resolution} | step = {step}", isInfo: IsInfo);

                    foreach (var item in images.Select((value, i) => new { i, value }))
                    {
                        var focusImg = item.value;
                        var index = item.i;

                        LoggerManager.Debug($"[NormalFocusing], WriteFocuisngInfo() : Index = {index} | Z Height = {focusImg.ZHeight} | Focusing value = {focusImg.FocusLevelValue}", isInfo: IsInfo);
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum SaveFailImage(List<ImageBuffer> images, string SaveFailPath = "")
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (images != null && images.Count > 0)
                {
                    foreach (var item in images.Select((value, i) => new { i, value }))
                    {
                        var focusImg = item.value;
                        var index = item.i;

                        // Save
                        if (SaveFailPath != string.Empty)
                        {
                            string SaveFullPath = string.Empty;

                            SaveFullPath = $"{SaveFailPath}{focusImg.CapturedTime.ToString("yyMMddHHmmss")}_Focusing#_{index}_Height_{focusImg.ZHeight:F2}_Value_{focusImg.FocusLevelValue}.jpeg";

                            this.VisionManager().SaveImageBuffer(focusImg, SaveFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum Focusing(IFocusParameter focusparam, object callerAssembly, bool isOutRangeFind = false, string SavePassPath = "", string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            EventCodeEnum focusResult = EventCodeEnum.UNDEFINED;

            double prePosition = 0.0;
            double LastZHeightPos = 0;

            bool AssignedLastZHeightPos = false;

            try
            {
                if (this.VisionManager().GetVisionProcRaft() != ProberInterfaces.Vision.EnumVisionProcRaft.MIL)
                {
                    if (FocusingStaticParam.ErrorEventCodeEnum != EventCodeEnum.UNDEFINED &&
                        focusparam.FocusingAxis.Value != EnumAxisConstants.PZ)
                    {
                        focusResult = FocusingStaticParam.ErrorEventCodeEnum;
                    }
                    else
                    {
                        focusResult = EventCodeEnum.NONE;
                    }

                    return focusResult;
                }

                Stopwatch stw = new Stopwatch();

                List<KeyValuePair<string, long>> timeStamp;
                timeStamp = new List<KeyValuePair<string, long>>();
                stw.Start();
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Focusing Start"), stw.ElapsedMilliseconds));

                XyDataSeries<double, double> dataSeries = null;

                bool continusgrab = false;
                double OrgPos = 0;

                double focusVel = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value).Param.Speed.Value;
                double focusAcc = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value).Param.Acceleration.Value;

                int focusStep;

                // 260225 Nick Test Code
                Rect focusROI = focusparam.FocusingROI.Value;

                ProbeAxisObject axis = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value);
                double focusResolution = 0;

                List<ImageBuffer> ImageBuffersForDebug = null;

                double curRefPos = 0.0;

                try
                {
                    this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);

                    OrgPos = curRefPos;

                    if (focusGraph != null)
                    {
                        focusGraph.ClearData();
                    }

                    double focusRange = focusparam.FocusRange.Value;

                    GetFocusResolution(focusparam, focusRange, out focusStep, out focusResolution);

                    if (focusStep == 0 || focusResolution == 0.0)
                    {
                        focusResult = EventCodeEnum.NONE;

                        return focusResult;
                    }

                    lock (lockObject)
                    {
                        continusgrab = this.VisionManager().ConfirmContinusGrab(focusparam.FocusingCam.Value);
                        this.VisionManager().StopGrab(focusparam.FocusingCam.Value);

                        Dataserices.AcceptsUnsortedData = true;

                        double settling = 0;

                        //==> Limit Range - z 축이 움직일 영역 범위 지정
                        this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);

                        double zAxisLimitCeiling = curRefPos + (focusRange / 2);//==> 상단 영역 좌표
                        double zAxisLimitFloor = curRefPos - (focusRange / 2);//==> 하단 영역 좌표
                        prePosition = curRefPos;

                        LoggerManager.Debug($"[NormalFocusing], Focusing() : curRefPos : {curRefPos}, zAxisLimitCeiling : {zAxisLimitCeiling}, zAxisLimitFloor : {zAxisLimitFloor}", isInfo: IsInfo);

                        double step_dir = 1;
                        bool bChecked = false;                  // 포커싱 맨 처음에만 편평도 에러 체크하기 위해서
                        bool bPeakSelectionStrategy = false;    // 포커싱 맨 처음에만 peakSelectionStrategy를 적용하기 위해서

                        List<ImageBuffer> imageBuffers = new List<ImageBuffer>();
                        ICamera cam = this.VisionManager().GetCam(focusparam.FocusingCam.Value);

                        this.VisionManager().SetCaller(focusparam.FocusingCam.Value, callerAssembly);

                        timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start position move Start"), stw.ElapsedMilliseconds));

                        this.MotionManager().RelMove(axis, (-focusRange) / 2, focusVel, focusAcc);
                        this.MotionManager().WaitForAxisMotionDone(axis);

                        this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);
                        VirtualStageConnector.VirtualStageConnector.Instance.SetFocusingStartPos(curRefPos);

                        step_dir = 1;
                        bChecked = false;

                        while (true)
                        {
                            Dataserices.Clear();
                            imageBuffers.Clear();

                            settling = focusResolution / 1000.0 * 8.0;
                            settling = 0.001;

                            this.MotionManager().SetSettlingTime(axis, settling);

                            timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move Start"), stw.ElapsedMilliseconds));
                            dataSeries = new XyDataSeries<double, double>();

                            try
                            {
                                for (int count = 0; count < FocusingStaticParam.SetIdleGrabCount; count++)
                                {
                                    this.VisionManager().SingleGrab(cam.GetChannelType(), callerAssembly);
                                    Thread.Sleep(FocusingStaticParam.FocusDelayTime);
                                }

                                for (int i = 0; i < focusStep; i++)
                                {
                                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("WaitGrab Start"), stw.ElapsedMilliseconds));
                                    ImageBuffer newImageBuffer = WaitGrab_eGrabber(focusparam, focusROI, callerAssembly);
                                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("WaitGrab End"), stw.ElapsedMilliseconds));

                                    lock (newImageBuffer)
                                    {
                                        double actPos = 0.0;
                                        this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);
                                        actPos = curRefPos;

                                        newImageBuffer.ZHeight = actPos;

                                        LoggerManager.Debug($"Focus Val. @{newImageBuffer.ZHeight}um = {newImageBuffer.FocusLevelValue}");

                                        imageBuffers.Add(newImageBuffer);
                                    }

                                    this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);

                                    if (curRefPos + focusResolution * step_dir > zAxisLimitCeiling)
                                    {
                                        break;
                                    }

                                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("ZMove Start"), stw.ElapsedMilliseconds));

                                    this.MotionManager().RelMove(axis, focusResolution * step_dir, focusVel, focusAcc);
                                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("ZMove End"), stw.ElapsedMilliseconds));

                                    Thread.Sleep(FocusingStaticParam.FocusDelayTime);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }

                            timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move End"), stw.ElapsedMilliseconds));

                            #region Save Debug Image
                            try
                            {
                                if (FocusingStaticParam.SaveImageFlag == true)
                                {
                                    if (Directory.Exists(FocusingStaticParam.SaveDebugImagePath) == false)
                                    {
                                        Directory.CreateDirectory(FocusingStaticParam.SaveDebugImagePath);
                                    }

                                    foreach (var item in imageBuffers.Select((value, index) => new { value, index }))
                                    {
                                        var image = item.value;
                                        var index = item.index;

                                        string saveFullPath = $"{FocusingStaticParam.SaveDebugImagePath}\\{image.CapturedTime.ToString("yyyy-MM-dd-HH-mm-ss-fff")}_Focusing#_{index + 1}_Height_{image.ZHeight:F2}_Value_{image.FocusLevelValue}.bmp";

                                        if (FocusingStaticParam.OverlayFocusROIFlag)
                                        {
                                            this.VisionManager().SaveImageBufferWithRectnagle(image, saveFullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE, focusROI);
                                        }
                                        else
                                        {
                                            this.VisionManager().SaveImageBuffer(image, saveFullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                        }
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                            #endregion

                            foreach (var image in imageBuffers)
                            {
                                image.FiliterdFocusValue = image.FocusLevelValue;
                            }

                            int maxFocusValue = imageBuffers.Max(image => image.FiliterdFocusValue);

                            ImageBuffer maxFocusValueImage = imageBuffers.First(image => image.FiliterdFocusValue == maxFocusValue);

                            int MaxFocusValueIndex = imageBuffers.FindIndex(image => image.FiliterdFocusValue == maxFocusValue);

                            LoggerManager.Debug($"[NormalFocusing], Focusing() : Max Index {MaxFocusValueIndex}. Focus Value : {maxFocusValueImage.FiliterdFocusValue}, Range = {focusRange}, Resolution = {focusResolution}, step = {focusStep}", isInfo: IsInfo);

                            #region [1] CHECK THRESHOLD FOCUS VALUE

                            //==> THRESHOLD
                            double focusThreshold = focusparam.FocusThreshold.Value;

                            if (focusThreshold <= 0)
                            {
                                focusThreshold = 70;
                            }

                            imageBuffers = imageBuffers.Where(image => image.FiliterdFocusValue > focusThreshold).ToList();

                            if (imageBuffers.Count == 0)
                            {
                                imageBuffers.Add(maxFocusValueImage);

                                focusResult = EventCodeEnum.FOCUS_VALUE_THRESHOLD;
                            }
                            #endregion

                            #region [2] CHECK FLATNESS
                            double focusMaxTotal = 0;
                            double filiterdfocusvalue = 0;
                            double focusValueTotal = 0;
                            double focusFlatness = 0;

                            if (maxFocusValue > 100000)
                            {
                                double maxValue = maxFocusValue * 0.00001;
                                focusMaxTotal = maxValue * focusStep;

                                foreach (var image in imageBuffers)
                                {
                                    filiterdfocusvalue = Convert.ToDouble(image.FiliterdFocusValue);

                                    focusValueTotal += filiterdfocusvalue;
                                }

                                focusValueTotal = focusValueTotal / 100000;
                                focusFlatness = focusValueTotal / focusMaxTotal * 100;
                            }
                            else
                            {
                                focusMaxTotal = maxFocusValue * focusStep;

                                foreach (var image in imageBuffers)
                                {
                                    filiterdfocusvalue = Convert.ToDouble(image.FiliterdFocusValue);

                                    focusValueTotal += filiterdfocusvalue;
                                }

                                focusFlatness = focusValueTotal / focusMaxTotal * 100;
                            }

                            if (bChecked == false)
                            {
                                double flatnessThreshold = focusparam.FlatnessThreshold.Value;

                                if (flatnessThreshold <= 0)
                                {
                                    flatnessThreshold = 50;
                                }

                                flatnessThreshold = this.VisionManager().GetMaxFocusFlatnessValue();

                                if (focusparam.FlatnessThreshold.Value > 0)
                                {
                                    flatnessThreshold = focusparam.FlatnessThreshold.Value;

                                    if (flatnessThreshold < this.VisionManager().GetMaxFocusFlatnessValue())
                                    {
                                        flatnessThreshold = this.VisionManager().GetMaxFocusFlatnessValue();

                                        LoggerManager.Debug($"Focusing(): Apply max. focus flatness threshold. Threshold = {flatnessThreshold}", isInfo: IsInfo);
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"Focusing(): Apply focus flatness threshold. Threshold = {flatnessThreshold}", isInfo: IsInfo);
                                    }
                                }

                                var focusCam = this.VisionManager().GetCam(focusparam.FocusingCam.Value);
                                var FocusFlatnessTriggerValue = this.VisionManager().GetFocusFlatnessTriggerValue();

                                if (focusCam.Param.RatioX.Value > FocusFlatnessTriggerValue ||
                                    focusCam.Param.RatioY.Value > FocusFlatnessTriggerValue)
                                {
                                    flatnessThreshold = 99.9;

                                    LoggerManager.Debug($"Focusing(): Apply max. flatness for low resolution camera. RatioX = {focusCam.Param.RatioX.Value}, RatioY = {focusCam.Param.RatioY.Value}", isInfo: IsInfo);
                                }
                                if (focusResolution < 10)        // Restrict flattness limit for fine focusing steps.
                                {
                                    flatnessThreshold = 99.9;

                                    LoggerManager.Debug($"Focusing(): Apply max. flatness for high resolution step. Focus resolution = {focusResolution}", isInfo: IsInfo);
                                }

                                LoggerManager.Debug($"Focusing(): Flatness = {focusFlatness:0.00}, Threshold = {flatnessThreshold}");

                                if (focusFlatness > flatnessThreshold)
                                {
                                    focusResult = EventCodeEnum.FOCUS_VALUE_FLAT;

                                    LoggerManager.Debug($"[NormalFocusing], Focusing() : FOCUS_VALUE_FLAT", isInfo: IsInfo);
                                    LoggerManager.Debug($"[NormalFocusing], Focusing() : focus Flatness : {focusFlatness}", isInfo: IsInfo);
                                    LoggerManager.Debug($"[NormalFocusing], Focusing() : maxFocusValue : {maxFocusValue}, focusStep : {focusStep}, focusMaxTotal : {focusMaxTotal}, focusValueTotal : {focusValueTotal}", isInfo: IsInfo);

                                    
                                }

                                bChecked = true;
                            }
                            #endregion

                            #region [3] CHECK DUAL PEAK

                            List<ImageBuffer> peakImageBuffer = new List<ImageBuffer>();

                            double mean = imageBuffers.Average(p => p.FiliterdFocusValue);
                            double variance = imageBuffers.Sum(p => Math.Pow(p.FiliterdFocusValue - mean, 2)) / imageBuffers.Count;
                            double standardDeviation = Math.Sqrt(variance);

                            double k = 1.0;
                            double threshold = mean + (k * standardDeviation);

                            for (int i = 1; i < imageBuffers.Count - 1; i++)
                            {
                                if (imageBuffers[i].FiliterdFocusValue > threshold && //==> Compare Threshold
                                    imageBuffers[i].FiliterdFocusValue > imageBuffers[i - 1].FiliterdFocusValue &&//==> Compare Prev Focus Level Value
                                    imageBuffers[i].FiliterdFocusValue > imageBuffers[i + 1].FiliterdFocusValue)//==> Compare Next Focus Level Value
                                {
                                    LoggerManager.Debug($"[NormalFocusing], Focusing() : A candidate image for dual peak inspection has been added. Index = {i}, First threshold : {threshold}, " +
                                        $"Prev < Current < Next (Focusing value) : {imageBuffers[i - 1].FiliterdFocusValue} < {imageBuffers[i].FiliterdFocusValue} < {imageBuffers[i + 1].FiliterdFocusValue}", isInfo: IsInfo);

                                    LoggerManager.Debug($"[NormalFocusing], Focusing() : Diffrence focusing value information", isInfo: IsInfo);
                                    LoggerManager.Debug($"[NormalFocusing], Focusing() : Current - Prev : {imageBuffers[i].FiliterdFocusValue - imageBuffers[i - 1].FiliterdFocusValue}", isInfo: IsInfo);
                                    LoggerManager.Debug($"[NormalFocusing], Focusing() : Next - Current : {imageBuffers[i + 1].FiliterdFocusValue - imageBuffers[i].FiliterdFocusValue}", isInfo: IsInfo);

                                    peakImageBuffer.Add(imageBuffers[i]);
                                }
                            }

                            if (peakImageBuffer.Count == 0)
                            {
                                peakImageBuffer.Add(maxFocusValueImage);
                            }

                            double maxPeakFocusValue = peakImageBuffer.Max(image => image.FiliterdFocusValue);
                            ImageBuffer MaxPeakImageBuffer = imageBuffers.First(image => image.FiliterdFocusValue == maxPeakFocusValue);
                            int maxPeakImageIndex = imageBuffers.IndexOf(MaxPeakImageBuffer);
                            
                            // 260226 Nick 단위변환 문제로 X*1000
                            double peakRangeThreshold = 200.0 * 1000.0;
                            //double peakRangeThreshold = focusparam.PeakRangeThreshold.Value;

                            // 최솟값을 200으로 설정
                            if (peakRangeThreshold < 200)
                            {
                                peakRangeThreshold = 200;
                            }

                            // 260226 Nick 테스트 중이므로 듀얼 피크 일단 제거
                            //if (peakSelectionStrategy == PeakSelectionStrategy.NONE)
                            //{
                            //    foreach (var item in peakImageBuffer.Select((value, i) => new { i, value }))
                            //    {
                            //        var peakImage = item.value;
                            //        var index = item.i;

                            //        if (Math.Abs(peakImage.ZHeight - MaxPeakImageBuffer.ZHeight) > peakRangeThreshold)
                            //        {
                            //            int CurrentPeakImageIndex = imageBuffers.IndexOf(peakImage);

                            //            LoggerManager.Debug($"[NormalFocusing], Focusing() : FOCUS_VALUE_DUALPEAK ERROR, Threshold value : {peakRangeThreshold}", isInfo: IsInfo);
                            //            LoggerManager.Debug($"[NormalFocusing], Focusing() : Number of images with peak information = {peakImageBuffer.Count}", isInfo: IsInfo);

                            //            LoggerManager.Debug($"[NormalFocusing], Focusing() : Current Index in peak image buffers = {index}", isInfo: IsInfo);
                            //            LoggerManager.Debug($"[NormalFocusing], Focusing() : Current Index = {CurrentPeakImageIndex} | Max peak image's index= {maxPeakImageIndex} (In Whole image buffers)", isInfo: IsInfo);

                            //            focusResult = EventCodeEnum.FOCUS_VALUE_DUALPEAK;

                            //            break;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    // NOTHING
                            //}
                            // 260226 end Nick 테스트 중이므로 듀얼 피크 일단 주석
                            #endregion

                            ImageBuffersForDebug = new List<ImageBuffer>(imageBuffers);

                            WriteFocusingInfo(ImageBuffersForDebug, focusRange, focusResolution, focusStep);

                            // 260226 end Nick 테스트 중이므로 이미지 저장 일단 주석
                            //==> Analisys Focusing Status
                            //if (focusResult == EventCodeEnum.FOCUS_VALUE_THRESHOLD ||
                            //    focusResult == EventCodeEnum.FOCUS_VALUE_FLAT ||
                            //    focusResult == EventCodeEnum.FOCUS_VALUE_DUALPEAK)
                            //{
                            //    //==> Error
                            //    if (ImageBuffersForDebug != null && ImageBuffersForDebug.Count > 0)
                            //    {
                            //        SaveFailImage(ImageBuffersForDebug, SaveFailPath);
                            //    }

                            //    break;
                            //}
                            // 260226 end Nick

                            var maxZHeight = peakImageBuffer.Max(image => image.ZHeight);
                            ImageBuffer HighestValueImage = peakImageBuffer.FirstOrDefault(image => image.ZHeight == maxZHeight);

                            var minZHeight = peakImageBuffer.Min(image => image.ZHeight);
                            ImageBuffer LowestValueImage = peakImageBuffer.FirstOrDefault(image => image.ZHeight == minZHeight);

                            ImageBuffer targetFocusValueImage = imageBuffers.FirstOrDefault(image => image.FiliterdFocusValue == maxPeakFocusValue);

                            if (!bPeakSelectionStrategy)
                            {
                                bPeakSelectionStrategy = true;

                                switch (peakSelectionStrategy)
                                {
                                    case PeakSelectionStrategy.NONE:
                                        break;
                                    case PeakSelectionStrategy.HIGHEST:

                                        if (LowestValueImage.FiliterdFocusValue * 2.0 < HighestValueImage.FiliterdFocusValue)
                                        {
                                            targetFocusValueImage = HighestValueImage;
                                        }
                                        break;
                                    case PeakSelectionStrategy.LOWEST:

                                        if (LowestValueImage.FiliterdFocusValue * 2.0 < HighestValueImage.FiliterdFocusValue)
                                        {
                                            targetFocusValueImage = LowestValueImage;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                targetFocusValueImage = imageBuffers.FirstOrDefault(image => image.FiliterdFocusValue == maxFocusValue);
                            }

                            GetNextResolution(focusparam, focusResolution, out focusStep, out focusResolution);

                            step_dir = step_dir * -1;

                            focusRange = focusResolution * (double)focusStep;

                            //==> Focusing Value가 가장 높은 지정으로 척 이동
                            if (focusStep == 0 || focusResolution == 0.0)
                            {
                                this.MotionManager().AbsMove(axis, targetFocusValueImage.ZHeight, focusVel, focusAcc);

                                focusResult = EventCodeEnum.NONE;

                                LastZHeightPos = targetFocusValueImage.ZHeight;
                                AssignedLastZHeightPos = true;

                                break;
                            }
                            else
                            {
                                this.MotionManager().AbsMove(axis, targetFocusValueImage.ZHeight - (focusRange / 2) * step_dir, focusVel, focusAcc);

                                LastZHeightPos = targetFocusValueImage.ZHeight - (focusRange / 2) * step_dir;
                                AssignedLastZHeightPos = true;
                            }

                            focusparam.FocusResultPos = targetFocusValueImage.ZHeight;
                            focusparam.FocusValue = targetFocusValueImage.FocusLevelValue;

                            Thread.Sleep(1);
                        }
                    }

                    this.VisionManager().SetCaller(focusparam.FocusingCam.Value, callerAssembly);

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    this.MotionManager().SetSettlingTime(this.MotionManager().GetAxis(focusparam.FocusingAxis.Value), this.MotionManager().GetAxis(focusparam.FocusingAxis.Value).SettlingTime);

                    if (continusgrab)
                    {
                        if (!this.VisionManager().ConfirmDigitizerEmulMode(focusparam.FocusingCam.Value))
                        {
                            this.VisionManager().StartGrab(focusparam.FocusingCam.Value, this);
                        }
                    }
                }

                if (AssignedLastZHeightPos == true)
                {
                    LoggerManager.Debug($"[NormarlFocusing], Focusing() : Last Z Height position = {LastZHeightPos}, Start Position = {OrgPos}, Difference position = {LastZHeightPos - OrgPos}", isInfo: IsInfo);
                }
                else
                {
                    LoggerManager.Debug($"[NormarlFocusing], Focusing() : Last Z Height position is not assigned", isInfo: IsInfo);
                }

                if (focusResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[NormarlFocusing], Focusing() : Focusing Failed. Result = {focusResult}", isInfo: IsInfo);

                    LoggerManager.Debug($"[NormarlFocusing], Focusing() : OrgPos : {OrgPos}", isInfo: IsInfo);

                    // Go back to previous height
                    this.MotionManager().AbsMove(axis, OrgPos, focusVel, focusAcc);
                }
                else
                {
                    if (Math.Abs(OrgPos - LastZHeightPos) > (focusparam.FocusRange.Value / 2) * 0.8)
                    {
                        var focusCam = this.VisionManager().GetCam(focusparam.FocusingCam.Value);
                        var FocusFlatnessTriggerValue = this.VisionManager().GetFocusFlatnessTriggerValue();

                        if (focusCam.Param.RatioX.Value < FocusFlatnessTriggerValue || focusCam.Param.RatioY.Value < FocusFlatnessTriggerValue)
                        {
                            focusResult = EventCodeEnum.FOCUS_POS_NEAREDGE;

                            LoggerManager.Debug($"[NormarlFocusing] Focusing() : Focused on near edge. Origin = {OrgPos:0.00}, Focused @{LastZHeightPos:0.00}, Range = {focusparam.FocusRange.Value:0.0}", isInfo: IsInfo);
                        }
                        else
                        {
                            LoggerManager.Debug($"[NormarlFocusing] Focusing() : Focusing Done but Focused on near edge. Origin = {OrgPos:0.00}, Focused @{LastZHeightPos:0.00}, Range = {focusparam.FocusRange.Value:0.0}", isInfo: IsInfo);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[NormarlFocusing] Focusing() : Focusing Done.", isInfo: IsInfo);
                    }
                }

                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Focusing End"), stw.ElapsedMilliseconds));

                if (stw.ElapsedMilliseconds > 10000)
                {
                    foreach (var item in timeStamp)
                    {
                        LoggerManager.Debug($"NormalFocusing TimeStamp - Desc: {item.Key}, Time: {item.Value}");
                    }
                }
                stw.Stop();

                if (FocusingStaticParam.ErrorEventCodeEnum != EventCodeEnum.UNDEFINED)
                {
                    focusResult = FocusingStaticParam.ErrorEventCodeEnum;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return focusResult;
        }

        private EventCodeEnum ParamVaildation(IFocusParameter focusparam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (focusparam != null)
                {
                    if (focusparam.FocusMaxStep.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FocusMaxStep Value is {focusparam.FocusMaxStep.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.FocusRange.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FocusRange Value is {focusparam.FocusRange.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.DepthOfField.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : DepthOfField Value is {focusparam.DepthOfField.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.FocusThreshold.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FocusThreshold Value is {focusparam.FocusThreshold.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.FlatnessThreshold.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FlatnessThreshold Value is {focusparam.FlatnessThreshold.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.PotentialThreshold.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : PotentialThreshold Value is {focusparam.PotentialThreshold.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.PeakRangeThreshold.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : PeakRangeThreshold Value is {focusparam.PeakRangeThreshold.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.FocusingCam.Value == EnumProberCam.INVALID || focusparam.FocusingCam.Value == EnumProberCam.UNDEFINED)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FocusingCam Value is {focusparam.FocusingCam.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if ((focusparam.FocusingROI.Value.Width <= 0) || (focusparam.FocusingROI.Value.Height <= 0))
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FocusingROI Value is Width = {focusparam.FocusingROI.Value.Width}, Height = {focusparam.FocusingROI.Value.Height}, Left = {focusparam.FocusingROI.Value.Left}, Top = {focusparam.FocusingROI.Value.Top}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.FocusingAxis.Value == EnumAxisConstants.Undefined)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FocusingAxis Value is {focusparam.FocusingAxis.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.FocusingType.Value == EnumFocusingType.UNDEFINED)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : FocusingType Value is {focusparam.FocusingType.Value}");

                        // TODO : Check
                        // retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (focusparam.OutFocusLimit.Value < 0)
                    {
                        LoggerManager.Error($"[NormalFocusing], ParamVaildation() : OutFocusLimit Value is {focusparam.OutFocusLimit.Value}");

                        retval = EventCodeEnum.FOCUS_PARAMETER_INVALID;
                    }

                    if (retval == EventCodeEnum.UNDEFINED)
                    {
                        retval = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region // 260811 sebas : CoaxLink카메라용
        public EventCodeEnum Focusing_CoaxLinkEx(IFocusParameter focusparam, ICoaxLinkExFocusFrameProvider frameProvider, object callerAssembly, bool isOutRangeFind = false,
                                                    string SavePassPath = "", string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            EventCodeEnum focusResult = EventCodeEnum.UNDEFINED;

            double prePosition = 0.0;
            double lastZHeightPos = 0.0;

            bool assignedLastZHeightPos = false;

            ProbeAxisObject axis = null;

            double originalPosition = 0.0;
            double focusVelocity = 0.0;
            double focusAcceleration = 0.0;

            Stopwatch stopwatch =
                new Stopwatch();

            List<KeyValuePair<string, long>> timeStamp =
                new List<KeyValuePair<string, long>>();

            try
            {
                LoggerManager.PinLog(
                    "Focusing_CoaxLinkEx..");

                if (focusparam == null)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] focusparam is null.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (frameProvider == null)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] frameProvider is null.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (!frameProvider.IsReady)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] CX3 camera is not ready.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (focusparam.FocusingAxis == null ||
                    focusparam.FocusingAxis.Value ==
                        EnumAxisConstants.Undefined)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] " +
                        "Focusing axis is invalid.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (focusparam.FocusRange == null ||
                    focusparam.FocusRange.Value <= 0)
                {
                    LoggerManager.Error(
                        $"[CoaxLinkExFocusing] " +
                        $"FocusRange is invalid. " +
                        $"Value={focusparam.FocusRange?.Value}");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                Rect focusROI =
                    focusparam.FocusingROI.Value;

                if (focusROI.Width <= 0 ||
                    focusROI.Height <= 0)
                {
                    LoggerManager.Error(
                        $"[CoaxLinkExFocusing] " +
                        $"Focusing ROI is invalid. ROI={focusROI}");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (this.VisionManager().GetVisionProcRaft() !=
                    ProberInterfaces.Vision.EnumVisionProcRaft.MIL)
                {
                    if (FocusingStaticParam.ErrorEventCodeEnum !=
                            EventCodeEnum.UNDEFINED &&
                        focusparam.FocusingAxis.Value !=
                            EnumAxisConstants.PZ)
                    {
                        return
                            FocusingStaticParam.ErrorEventCodeEnum;
                    }

                    return EventCodeEnum.NONE;
                }

                stopwatch.Start();

                timeStamp.Add(
                    new KeyValuePair<string, long>(
                        "CoaxLinkEx Focusing Start",
                        stopwatch.ElapsedMilliseconds));

                XyDataSeries<double, double> dataSeries =
                    null;

                axis =
                    this.MotionManager().GetAxis(
                        focusparam.FocusingAxis.Value);

                if (axis == null)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] " +
                        "Failed to get focusing axis.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                focusVelocity =
                    axis.Param.Speed.Value;

                focusAcceleration =
                    axis.Param.Acceleration.Value;

                int focusStep = 0;
                double focusResolution = 0.0;
                double currentReferencePosition = 0.0;

                List<ImageBuffer> imageBuffersForDebug =
                    null;

                this.MotionManager().GetRefPos(
                    axis.AxisType.Value,
                    ref currentReferencePosition);

                originalPosition =
                    currentReferencePosition;

                if (focusGraph != null)
                {
                    focusGraph.ClearData();
                }

                double focusRange =
                    focusparam.FocusRange.Value;

                GetFocusResolution(
                    focusparam,
                    focusRange,
                    out focusStep,
                    out focusResolution);

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] " +
                    $"Range={focusRange}, " +
                    $"Step={focusStep}, " +
                    $"Resolution={focusResolution}");

                if (focusStep == 0 ||
                    focusResolution == 0.0)
                {
                    LoggerManager.Debug(
                        "[CoaxLinkExFocusing] " +
                        "Focusing is not required because " +
                        "focus step or resolution is zero.");

                    return EventCodeEnum.NONE;
                }

                lock (lockObject)
                {
                    Dataserices.AcceptsUnsortedData =
                        true;

                    double settling = 0.0;

                    this.MotionManager().GetRefPos(
                        axis.AxisType.Value,
                        ref currentReferencePosition);

                    double zAxisLimitCeiling =
                        currentReferencePosition +
                        (focusRange / 2.0);

                    double zAxisLimitFloor =
                        currentReferencePosition -
                        (focusRange / 2.0);

                    prePosition =
                        currentReferencePosition;

                    LoggerManager.Debug(
                        $"[CoaxLinkExFocusing] " +
                        $"Current={currentReferencePosition}, " +
                        $"Ceiling={zAxisLimitCeiling}, " +
                        $"Floor={zAxisLimitFloor}");

                    double stepDirection = 1.0;

                    bool checkedFlatness = false;
                    bool appliedPeakSelectionStrategy = false;

                    List<ImageBuffer> imageBuffers =
                        new List<ImageBuffer>();

                    timeStamp.Add(
                        new KeyValuePair<string, long>(
                            "Start Position Move Start",
                            stopwatch.ElapsedMilliseconds));

                    /*
                     * 최초 위치에서 FocusRange 절반만큼 아래로 이동한다.
                     */
                    this.MotionManager().RelMove(
                        axis,
                        (-focusRange) / 2.0,
                        focusVelocity,
                        focusAcceleration);

                    this.MotionManager()
                        .WaitForAxisMotionDone(axis);

                    this.MotionManager().GetRefPos(
                        axis.AxisType.Value,
                        ref currentReferencePosition);

                    VirtualStageConnector
                        .VirtualStageConnector
                        .Instance
                        .SetFocusingStartPos(
                            currentReferencePosition);

                    stepDirection = 1.0;
                    checkedFlatness = false;

                    while (true)
                    {
                        Dataserices.Clear();
                        imageBuffers.Clear();

                        settling =
                            focusResolution / 1000.0 * 8.0;

                        // 기존 Focusing과 동일하게 0.001 적용
                        settling = 0.001;

                        this.MotionManager().SetSettlingTime(
                            axis,
                            settling);

                        timeStamp.Add(
                            new KeyValuePair<string, long>(
                                "Move Start",
                                stopwatch.ElapsedMilliseconds));

                        dataSeries =
                            new XyDataSeries<double, double>();

                        try
                        {
                            /*
                             * CX3 카메라는 계속 Grab 중이므로
                             * 기존 SingleGrab 대신 새 프레임만 버린다.
                             */
                            for (int count = 0;
                                 count <
                                    FocusingStaticParam.SetIdleGrabCount;
                                 count++)
                            {
                                ImageBuffer idleImage =
                                    frameProvider.WaitNextImage(3000);

                                Thread.Sleep(
                                    FocusingStaticParam.FocusDelayTime);
                            }

                            for (int i = 0;
                                 i < focusStep;
                                 i++)
                            {
                                timeStamp.Add(
                                    new KeyValuePair<string, long>(
                                        "WaitGrab Start",
                                        stopwatch.ElapsedMilliseconds));

                                ImageBuffer newImageBuffer =
                                    WaitGrab_CoaxLinkEx(
                                        frameProvider,
                                        focusROI,
                                        3000);

                                timeStamp.Add(
                                    new KeyValuePair<string, long>(
                                        "WaitGrab End",
                                        stopwatch.ElapsedMilliseconds));

                                if (newImageBuffer == null ||
                                    newImageBuffer.Buffer == null ||
                                    newImageBuffer.Buffer.Length == 0)
                                {
                                    LoggerManager.Error(
                                        "[CoaxLinkExFocusing] " +
                                        "Failed to acquire CX3 image.");

                                    focusResult =
                                        EventCodeEnum.UNDEFINED;

                                    break;
                                }

                                lock (newImageBuffer)
                                {
                                    double actualPosition = 0.0;

                                    this.MotionManager().GetRefPos(
                                        axis.AxisType.Value,
                                        ref actualPosition);

                                    newImageBuffer.ZHeight =
                                        actualPosition;

                                    LoggerManager.Debug(
                                        $"[CoaxLinkExFocusing] " +
                                        $"Focus Value " +
                                        $"@{newImageBuffer.ZHeight:0.000} " +
                                        $"= {newImageBuffer.FocusLevelValue}");

                                    imageBuffers.Add(
                                        newImageBuffer);
                                }

                                this.MotionManager().GetRefPos(
                                    axis.AxisType.Value,
                                    ref currentReferencePosition);

                                if (currentReferencePosition +
                                        focusResolution *
                                        stepDirection >
                                    zAxisLimitCeiling)
                                {
                                    break;
                                }

                                if (currentReferencePosition +
                                        focusResolution *
                                        stepDirection <
                                    zAxisLimitFloor)
                                {
                                    break;
                                }

                                timeStamp.Add(
                                    new KeyValuePair<string, long>(
                                        "ZMove Start",
                                        stopwatch.ElapsedMilliseconds));

                                this.MotionManager().RelMove(
                                    axis,
                                    focusResolution *
                                        stepDirection,
                                    focusVelocity,
                                    focusAcceleration);

                                this.MotionManager()
                                    .WaitForAxisMotionDone(axis);

                                timeStamp.Add(
                                    new KeyValuePair<string, long>(
                                        "ZMove End",
                                        stopwatch.ElapsedMilliseconds));

                                Thread.Sleep(
                                    FocusingStaticParam.FocusDelayTime);
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            focusResult =
                                EventCodeEnum.UNDEFINED;
                        }

                        timeStamp.Add(
                            new KeyValuePair<string, long>(
                                "Move End",
                                stopwatch.ElapsedMilliseconds));

                        if (imageBuffers.Count == 0)
                        {
                            LoggerManager.Error(
                                "[CoaxLinkExFocusing] " +
                                "No focusing images were acquired.");

                            focusResult =
                                EventCodeEnum.UNDEFINED;

                            break;
                        }

                        #region Save Debug Image

                        try
                        {
                            if (FocusingStaticParam.SaveImageFlag)
                            {
                                if (!Directory.Exists(
                                        FocusingStaticParam
                                            .SaveDebugImagePath))
                                {
                                    Directory.CreateDirectory(
                                        FocusingStaticParam
                                            .SaveDebugImagePath);
                                }

                                foreach (var item in
                                    imageBuffers.Select(
                                        (value, index) =>
                                            new
                                            {
                                                value,
                                                index
                                            }))
                                {
                                    ImageBuffer image =
                                        item.value;

                                    int index =
                                        item.index;

                                    string saveFullPath =
                                        $"{FocusingStaticParam.SaveDebugImagePath}\\" +
                                        $"{image.CapturedTime:yyyy-MM-dd-HH-mm-ss-fff}" +
                                        $"_CoaxLinkExFocusing#_{index + 1}" +
                                        $"_Height_{image.ZHeight:F2}" +
                                        $"_Value_{image.FocusLevelValue}.bmp";

                                    if (FocusingStaticParam
                                        .OverlayFocusROIFlag)
                                    {
                                        this.VisionManager()
                                            .SaveImageBufferWithRectnagle(
                                                image,
                                                saveFullPath,
                                                IMAGE_LOG_TYPE.NORMAL,
                                                EventCodeEnum.NONE,
                                                focusROI);
                                    }
                                    else
                                    {
                                        this.VisionManager()
                                            .SaveImageBuffer(
                                                image,
                                                saveFullPath,
                                                IMAGE_LOG_TYPE.NORMAL,
                                                EventCodeEnum.NONE);
                                    }
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }

                        #endregion

                        foreach (ImageBuffer image in imageBuffers)
                        {
                            image.FiliterdFocusValue =
                                image.FocusLevelValue;
                        }

                        int maxFocusValue =
                            imageBuffers.Max(
                                image =>
                                    image.FiliterdFocusValue);

                        ImageBuffer maxFocusValueImage =
                            imageBuffers.First(
                                image =>
                                    image.FiliterdFocusValue ==
                                    maxFocusValue);

                        int maxFocusValueIndex =
                            imageBuffers.FindIndex(
                                image =>
                                    image.FiliterdFocusValue ==
                                    maxFocusValue);

                        LoggerManager.Debug(
                            $"[CoaxLinkExFocusing] " +
                            $"MaxIndex={maxFocusValueIndex}, " +
                            $"MaxFocusValue={maxFocusValue}, " +
                            $"Range={focusRange}, " +
                            $"Resolution={focusResolution}, " +
                            $"Step={focusStep}");

                        #region Check focus threshold

                        double focusThreshold =
                            focusparam.FocusThreshold.Value;

                        if (focusThreshold <= 0)
                        {
                            focusThreshold = 70;
                        }

                        List<ImageBuffer> thresholdPassedImages =
                            imageBuffers
                                .Where(
                                    image =>
                                        image.FiliterdFocusValue >
                                        focusThreshold)
                                .ToList();

                        if (thresholdPassedImages.Count == 0)
                        {
                            LoggerManager.Debug(
                                $"[CoaxLinkExFocusing] " +
                                $"FOCUS_VALUE_THRESHOLD. " +
                                $"Max={maxFocusValue}, " +
                                $"Threshold={focusThreshold}");

                            /*
                             * 기존 Focusing과 동일하게 최대값 이미지는
                             * 유지하여 위치 계산을 계속 수행한다.
                             */
                            thresholdPassedImages.Add(
                                maxFocusValueImage);

                            focusResult =
                                EventCodeEnum.FOCUS_VALUE_THRESHOLD;
                        }

                        imageBuffers =
                            thresholdPassedImages;

                        #endregion

                        #region Check flatness

                        double focusMaxTotal = 0.0;
                        double filteredFocusValue = 0.0;
                        double focusValueTotal = 0.0;
                        double focusFlatness = 0.0;

                        if (maxFocusValue > 100000)
                        {
                            double convertedMaxValue =
                                maxFocusValue * 0.00001;

                            focusMaxTotal =
                                convertedMaxValue * focusStep;

                            foreach (ImageBuffer image in
                                imageBuffers)
                            {
                                filteredFocusValue =
                                    Convert.ToDouble(
                                        image.FiliterdFocusValue);

                                focusValueTotal +=
                                    filteredFocusValue;
                            }

                            focusValueTotal =
                                focusValueTotal / 100000.0;

                            focusFlatness =
                                focusValueTotal /
                                focusMaxTotal *
                                100.0;
                        }
                        else
                        {
                            focusMaxTotal =
                                maxFocusValue * focusStep;

                            foreach (ImageBuffer image in
                                imageBuffers)
                            {
                                filteredFocusValue =
                                    Convert.ToDouble(
                                        image.FiliterdFocusValue);

                                focusValueTotal +=
                                    filteredFocusValue;
                            }

                            if (focusMaxTotal != 0.0)
                            {
                                focusFlatness =
                                    focusValueTotal /
                                    focusMaxTotal *
                                    100.0;
                            }
                        }

                        if (!checkedFlatness)
                        {
                            double flatnessThreshold =
                                focusparam.FlatnessThreshold.Value;

                            if (flatnessThreshold <= 0)
                            {
                                flatnessThreshold = 50;
                            }

                            /*
                             * CX3는 VisionManager.GetCam()에 등록되어 있지
                             * 않으므로 RatioX/RatioY 판정은 하지 않는다.
                             */
                            if (focusResolution < 10)
                            {
                                flatnessThreshold = 99.9;

                                LoggerManager.Debug(
                                    "[CoaxLinkExFocusing] " +
                                    "Apply maximum flatness for " +
                                    $"fine resolution. " +
                                    $"Resolution={focusResolution}");
                            }

                            LoggerManager.Debug(
                                $"[CoaxLinkExFocusing] " +
                                $"Flatness={focusFlatness:0.00}, " +
                                $"Threshold={flatnessThreshold}");

                            if (focusFlatness >
                                flatnessThreshold)
                            {
                                focusResult =
                                    EventCodeEnum.FOCUS_VALUE_FLAT;

                                LoggerManager.Debug(
                                    "[CoaxLinkExFocusing] " +
                                    "FOCUS_VALUE_FLAT");
                            }

                            checkedFlatness = true;
                        }

                        #endregion

                        #region Check peak

                        List<ImageBuffer> peakImageBuffers =
                            new List<ImageBuffer>();

                        double mean =
                            imageBuffers.Average(
                                image =>
                                    image.FiliterdFocusValue);

                        double variance =
                            imageBuffers.Sum(
                                image =>
                                    Math.Pow(
                                        image.FiliterdFocusValue -
                                        mean,
                                        2)) /
                            imageBuffers.Count;

                        double standardDeviation =
                            Math.Sqrt(variance);

                        const double k = 1.0;

                        double peakThreshold =
                            mean +
                            (k * standardDeviation);

                        for (int i = 1;
                             i < imageBuffers.Count - 1;
                             i++)
                        {
                            if (imageBuffers[i]
                                    .FiliterdFocusValue >
                                    peakThreshold &&
                                imageBuffers[i]
                                    .FiliterdFocusValue >
                                    imageBuffers[i - 1]
                                        .FiliterdFocusValue &&
                                imageBuffers[i]
                                    .FiliterdFocusValue >
                                    imageBuffers[i + 1]
                                        .FiliterdFocusValue)
                            {
                                peakImageBuffers.Add(
                                    imageBuffers[i]);

                                LoggerManager.Debug(
                                    $"[CoaxLinkExFocusing] " +
                                    $"Peak candidate added. " +
                                    $"Index={i}, " +
                                    $"Value=" +
                                    $"{imageBuffers[i].FiliterdFocusValue}");
                            }
                        }

                        if (peakImageBuffers.Count == 0)
                        {
                            peakImageBuffers.Add(
                                maxFocusValueImage);
                        }

                        double maxPeakFocusValue =
                            peakImageBuffers.Max(
                                image =>
                                    image.FiliterdFocusValue);

                        ImageBuffer maxPeakImageBuffer =
                            peakImageBuffers.First(
                                image =>
                                    image.FiliterdFocusValue ==
                                    maxPeakFocusValue);

                        #endregion

                        imageBuffersForDebug =
                            new List<ImageBuffer>(
                                imageBuffers);

                        WriteFocusingInfo(
                            imageBuffersForDebug,
                            focusRange,
                            focusResolution,
                            focusStep);

                        ImageBuffer highestPositionImage =
                            peakImageBuffers
                                .OrderByDescending(
                                    image =>
                                        image.ZHeight)
                                .First();

                        ImageBuffer lowestPositionImage =
                            peakImageBuffers
                                .OrderBy(
                                    image =>
                                        image.ZHeight)
                                .First();

                        ImageBuffer targetFocusValueImage =
                            maxPeakImageBuffer;

                        if (!appliedPeakSelectionStrategy)
                        {
                            appliedPeakSelectionStrategy =
                                true;

                            switch (peakSelectionStrategy)
                            {
                                case PeakSelectionStrategy.NONE:
                                    break;

                                case PeakSelectionStrategy.HIGHEST:

                                    if (lowestPositionImage
                                            .FiliterdFocusValue *
                                            2.0 <
                                        highestPositionImage
                                            .FiliterdFocusValue)
                                    {
                                        targetFocusValueImage =
                                            highestPositionImage;
                                    }

                                    break;

                                case PeakSelectionStrategy.LOWEST:

                                    if (lowestPositionImage
                                            .FiliterdFocusValue *
                                            2.0 <
                                        highestPositionImage
                                            .FiliterdFocusValue)
                                    {
                                        targetFocusValueImage =
                                            lowestPositionImage;
                                    }

                                    break;

                                default:
                                    break;
                            }
                        }
                        else
                        {
                            targetFocusValueImage =
                                imageBuffers
                                    .OrderByDescending(
                                        image =>
                                            image.FiliterdFocusValue)
                                    .First();
                        }

                        int nextFocusStep = 0;
                        double nextFocusResolution = 0.0;

                        GetNextResolution(
                            focusparam,
                            focusResolution,
                            out nextFocusStep,
                            out nextFocusResolution);

                        focusStep =
                            nextFocusStep;

                        focusResolution =
                            nextFocusResolution;

                        stepDirection *= -1.0;

                        focusRange =
                            focusResolution *
                            focusStep;

                        /*
                         * 최종 정밀도까지 도달한 경우 최대 초점값 위치로 이동
                         */
                        if (focusStep == 0 ||
                            focusResolution == 0.0)
                        {
                            this.MotionManager().AbsMove(
                                axis,
                                targetFocusValueImage.ZHeight,
                                focusVelocity,
                                focusAcceleration);

                            this.MotionManager()
                                .WaitForAxisMotionDone(axis);

                            lastZHeightPos =
                                targetFocusValueImage.ZHeight;

                            assignedLastZHeightPos =
                                true;

                            focusparam.FocusResultPos =
                                targetFocusValueImage.ZHeight;

                            focusparam.FocusValue =
                                targetFocusValueImage
                                    .FocusLevelValue;

                            focusResult =
                                EventCodeEnum.NONE;

                            break;
                        }

                        /*
                         * 다음 정밀 포커싱 시작 위치로 이동
                         */
                        double nextStartPosition =
                            targetFocusValueImage.ZHeight -
                            (focusRange / 2.0) *
                            stepDirection;

                        this.MotionManager().AbsMove(
                            axis,
                            nextStartPosition,
                            focusVelocity,
                            focusAcceleration);

                        this.MotionManager()
                            .WaitForAxisMotionDone(axis);

                        lastZHeightPos =
                            nextStartPosition;

                        assignedLastZHeightPos =
                            true;

                        focusparam.FocusResultPos =
                            targetFocusValueImage.ZHeight;

                        focusparam.FocusValue =
                            targetFocusValueImage
                                .FocusLevelValue;

                        Thread.Sleep(1);
                    }
                }

                if (assignedLastZHeightPos)
                {
                    LoggerManager.Debug(
                        $"[CoaxLinkExFocusing] " +
                        $"Last Z={lastZHeightPos}, " +
                        $"Start Z={originalPosition}, " +
                        $"Difference=" +
                        $"{lastZHeightPos - originalPosition}");
                }
                else
                {
                    LoggerManager.Debug(
                        "[CoaxLinkExFocusing] " +
                        "Last Z height was not assigned.");
                }

                if (focusResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug(
                        $"[CoaxLinkExFocusing] " +
                        $"Focusing failed. Result={focusResult}");

                    LoggerManager.Debug(
                        $"[CoaxLinkExFocusing] " +
                        $"Move back to original position: " +
                        $"{originalPosition}");

                    this.MotionManager().AbsMove(
                        axis,
                        originalPosition,
                        focusVelocity,
                        focusAcceleration);

                    this.MotionManager()
                        .WaitForAxisMotionDone(axis);
                }
                else
                {
                    double nearEdgeThreshold =
                        (focusparam.FocusRange.Value / 2.0) *
                        0.8;

                    if (Math.Abs(
                            originalPosition -
                            lastZHeightPos) >
                        nearEdgeThreshold)
                    {
                        focusResult =
                            EventCodeEnum.FOCUS_POS_NEAREDGE;

                        LoggerManager.Debug(
                            $"[CoaxLinkExFocusing] " +
                            $"Focused near edge. " +
                            $"Origin={originalPosition:0.00}, " +
                            $"Focused={lastZHeightPos:0.00}, " +
                            $"Range=" +
                            $"{focusparam.FocusRange.Value:0.0}");
                    }
                    else
                    {
                        LoggerManager.Debug(
                            "[CoaxLinkExFocusing] " +
                            "Focusing completed.");
                    }
                }

                timeStamp.Add(
                    new KeyValuePair<string, long>(
                        "CoaxLinkEx Focusing End",
                        stopwatch.ElapsedMilliseconds));

                if (stopwatch.ElapsedMilliseconds > 10000)
                {
                    foreach (KeyValuePair<string, long> item in
                        timeStamp)
                    {
                        LoggerManager.Debug(
                            $"CoaxLinkEx Focusing TimeStamp - " +
                            $"Description={item.Key}, " +
                            $"Time={item.Value}");
                    }
                }

                if (FocusingStaticParam.ErrorEventCodeEnum !=
                    EventCodeEnum.UNDEFINED)
                {
                    focusResult =
                        FocusingStaticParam.ErrorEventCodeEnum;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                focusResult =
                    EventCodeEnum.UNDEFINED;

                /*
                 * 예외 발생 시에도 원래 위치로 복귀한다.
                 */
                try
                {
                    if (axis != null)
                    {
                        this.MotionManager().AbsMove(
                            axis,
                            originalPosition,
                            focusVelocity,
                            focusAcceleration);

                        this.MotionManager()
                            .WaitForAxisMotionDone(axis);
                    }
                }
                catch (Exception moveException)
                {
                    LoggerManager.Exception(
                        moveException);
                }
            }
            finally
            {
                stopwatch.Stop();

                /*
                 * CX3 Grab은 ViewModel Display Thread가 계속 담당하므로
                 * 여기서 StartGrab/StopGrab하지 않는다.
                 */
                try
                {
                    if (axis != null)
                    {
                        this.MotionManager().SetSettlingTime(
                            axis,
                            axis.SettlingTime);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] " +
                    $"Result={focusResult}, " +
                    $"Time={stopwatch.ElapsedMilliseconds}ms");
            }

            return focusResult;
        }
        private EventCodeEnum Validation_CoaxLinkEx(IFocusParameter focusparam, ICoaxLinkExFocusFrameProvider frameProvider)
        {
            try
            {
                if (focusparam == null)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] FocusParameter is null.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (frameProvider == null ||
                    !frameProvider.IsReady)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] Camera is not ready.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (focusparam.FocusRange == null ||
                    focusparam.FocusRange.Value <= 0)
                {
                    LoggerManager.Error(
                        $"[CoaxLinkExFocusing] Invalid FocusRange: " +
                        $"{focusparam.FocusRange?.Value}");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (focusparam.FocusingAxis == null ||
                    focusparam.FocusingAxis.Value ==
                        EnumAxisConstants.Undefined)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] Invalid focusing axis.");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (focusparam.FocusingROI == null ||
                    focusparam.FocusingROI.Value.Width <= 0 ||
                    focusparam.FocusingROI.Value.Height <= 0)
                {
                    LoggerManager.Error(
                        $"[CoaxLinkExFocusing] Invalid ROI: " +
                        $"{focusparam.FocusingROI?.Value}");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                if (focusparam.DepthOfField == null ||
                    focusparam.DepthOfField.Value <= 0)
                {
                    LoggerManager.Error(
                        $"[CoaxLinkExFocusing] Invalid DepthOfField: " +
                        $"{focusparam.DepthOfField?.Value}");

                    return EventCodeEnum.FOCUS_PARAMETER_INVALID;
                }

                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                return EventCodeEnum.FOCUS_PARAMETER_INVALID;
            }
        }
        public EventCodeEnum Focusing_Retry_CoaxLinkEx(IFocusParameter focusparam, ICoaxLinkExFocusFrameProvider frameProvider, bool lightChange_retry, bool bruteForce_retry, 
                                                        bool outRangeFind_retry, object callerassembly, int TargetGrayLevel = 0, bool ForcedApplyAutolight = false, string SavePassPath = "", 
                                                        string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            Stopwatch stopwatch =
                Stopwatch.StartNew();

            EventCodeEnum focusingResult =
                EventCodeEnum.UNDEFINED;

            LoggerManager.PinLog(
                "Focusing_Retry_CoaxLinkEx..");

            try
            {
                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] Caller: " +
                    $"{callerassembly?.GetType()?.FullName}");

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] ROI: " +
                    $"X={focusparam?.FocusingROI?.Value.X}, " +
                    $"Y={focusparam?.FocusingROI?.Value.Y}, " +
                    $"Width={focusparam?.FocusingROI?.Value.Width}, " +
                    $"Height={focusparam?.FocusingROI?.Value.Height}");

                // 기존 ParamVaildation()은 호출하지 않음
                focusingResult =
                    Validation_CoaxLinkEx(
                        focusparam,
                        frameProvider);

                if (focusingResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] " +
                        "Validation failed.");

                    return focusingResult;
                }

                focusingResult =
                    Focusing_CoaxLinkEx(
                        focusparam,
                        frameProvider,
                        callerassembly,
                        false,
                        SavePassPath,
                        SaveFailPath,
                        peakSelectionStrategy);

                if (focusingResult == EventCodeEnum.NONE)
                {
                    return EventCodeEnum.NONE;
                }

                ProbeAxisObject axis =
                    this.MotionManager().GetAxis(
                        focusparam.FocusingAxis.Value);

                double retryPosition = 0;

                this.MotionManager().GetRefPos(
                    axis.AxisType.Value,
                    ref retryPosition);

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] " +
                    $"Current retry position: " +
                    $"{retryPosition:0.00}");

                /*
                 * CX3가 AutoLightAdvisor의 EnumProberCam에 등록되지 않았으므로
                 * 현재는 lightChange_retry를 수행하지 않습니다.
                 */
                if (lightChange_retry)
                {
                    LoggerManager.Debug(
                        "[CoaxLinkExFocusing] " +
                        "lightChange_retry skipped. " +
                        "CX3 light control is not connected.");
                }

                if (bruteForce_retry)
                {
                    LoggerManager.Debug(
                        "[CoaxLinkExFocusing] " +
                        "bruteForce_retry start.");

                    focusingResult =
                        Focusing_CoaxLinkEx(
                            focusparam,
                            frameProvider,
                            callerassembly,
                            false,
                            SavePassPath,
                            SaveFailPath,
                            peakSelectionStrategy);

                    if (focusingResult ==
                        EventCodeEnum.FOCUS_POS_NEAREDGE)
                    {
                        focusingResult =
                            Focusing_CoaxLinkEx(
                                focusparam,
                                frameProvider,
                                callerassembly,
                                false,
                                SavePassPath,
                                SaveFailPath,
                                peakSelectionStrategy);
                    }

                    LoggerManager.Debug(
                        "[CoaxLinkExFocusing] " +
                        "bruteForce_retry end.");

                    if (focusingResult == EventCodeEnum.NONE)
                    {
                        return EventCodeEnum.NONE;
                    }
                }

                if (outRangeFind_retry)
                {
                    LoggerManager.Debug(
                        "[CoaxLinkExFocusing] " +
                        "outRangeFind_retry start.");

                    focusingResult =
                        Focusing_CoaxLinkEx(
                            focusparam,
                            frameProvider,
                            callerassembly,
                            true,
                            SavePassPath,
                            SaveFailPath,
                            peakSelectionStrategy);

                    LoggerManager.Debug(
                        "[CoaxLinkExFocusing] " +
                        "outRangeFind_retry end.");

                    if (focusingResult == EventCodeEnum.NONE)
                    {
                        return EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                stopwatch.Stop();

                if (focusparam != null)
                {
                    focusparam.FocusTime =
                        stopwatch.ElapsedMilliseconds;
                }

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] " +
                    $"Retry result={focusingResult}, " +
                    $"Time={stopwatch.ElapsedMilliseconds}ms");
            }

            return focusingResult;
        }
        #endregion
        public override EventCodeEnum Focusing_Retry(IFocusParameter focusparam, bool lightChange_retry, bool bruteForce_retry, bool outRangeFind_retry, object callerassembly, int TargetGrayLevel = 0, bool ForcedApplyAutolight = false, string SavePassPath = "", string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            stw.Start();
            ProbeAxisObject axis = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value);

            EventCodeEnum focusingResult = EventCodeEnum.UNDEFINED;

            LoggerManager.PinLog("Focusing_Retry..");

            int setGralLevelValue = TargetGrayLevel;

            try
            {
                LoggerManager.Debug($"[NormalFocusing], Focusing_Retry() : Caller is {callerassembly?.GetType()?.FullName}");
                LoggerManager.Debug($"[NormalFocusing], Focusing_Retry() : Focusing ROI : (X:{focusparam.FocusingROI.Value.X}, Y:{focusparam.FocusingROI.Value.Y}, Width:{focusparam.FocusingROI.Value.Width}, Height:{focusparam.FocusingROI.Value.Height})");

                focusingResult = ParamVaildation(focusparam);

                if (focusingResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[NormalFocusing], Focusing_Retry() : ParamVaildation() is failed.");

                    return focusingResult;
                }

                timeStamp.Add(new KeyValuePair<string, long>("Focusing First start", stw.ElapsedMilliseconds));

                if (ForcedApplyAutolight == true && setGralLevelValue != 0)
                {
                    LoggerManager.Error($"[NormalFocusing], Focusing_Retry() : SetGraylLevel fucntion is called. Set value = {setGralLevelValue}");

                    this.AutoLightAdvisor().SetGrayLevel(focusparam.FocusingCam.Value, setGralLevelValue);
                }

                focusingResult = Focusing(focusparam, callerassembly, SavePassPath: SavePassPath, SaveFailPath: SaveFailPath, peakSelectionStrategy: peakSelectionStrategy);

                // 260226 Nick Setting 중 이므로 일단 이부분 주석 추후 주석 해제 필요
                //if (focusingResult == EventCodeEnum.FOCUS_POS_NEAREDGE)
                //{
                //    focusingResult = Focusing(focusparam, callerassembly, SavePassPath: SavePassPath, SaveFailPath: SaveFailPath, peakSelectionStrategy: peakSelectionStrategy);
                //}

                timeStamp.Add(new KeyValuePair<string, long>("Focusing First End", stw.ElapsedMilliseconds));
                stw.Stop();

                LoggerManager.Debug($"[NormalFocusing], Focusing_Retry() : Focusing Time = {stw.ElapsedMilliseconds}ms");

                focusparam.FocusTime = stw.ElapsedMilliseconds;

                if (focusingResult == EventCodeEnum.NONE)
                {
                    return EventCodeEnum.NONE;
                }

                stw.Restart();

                double retrypos = 0;

                // 위에서 실패했을 때, 시작점으로 이동했기 때문에 추가로 움직이지 않아도 될 것으로 보임. 디버깅용으로 위치만 찍어보자.
                this.MotionManager().GetRefPos(axis.AxisType.Value, ref retrypos);

                LoggerManager.Debug($"[NormalFocusing], Cur. position for retry: {retrypos:0.00}");

                if (lightChange_retry)
                {
                    LoggerManager.Debug($"[NormalFocusing], lightChange_retry Start");

                    if (setGralLevelValue != 0 && ForcedApplyAutolight == false)
                    {
                        this.AutoLightAdvisor().SetGrayLevel(focusparam.FocusingCam.Value, setGralLevelValue);

                        focusingResult = Focusing(focusparam, callerassembly, peakSelectionStrategy: peakSelectionStrategy);
                    }
                    else
                    {
                        LoggerManager.Error($"[NormalFocusing], Target Gray Level is 0. Skip retry function.");

                        focusingResult = EventCodeEnum.UNDEFINED;
                    }

                    LoggerManager.Debug($"[NormalFocusing], lightChange_retry End");

                    if (focusingResult == EventCodeEnum.NONE)
                    {
                        return EventCodeEnum.NONE;
                    }
                }

                if (bruteForce_retry)
                {
                    LoggerManager.Debug($"[NormalFocusing], bruteForce_retry Start");

                    focusingResult = Focusing(focusparam, callerassembly, peakSelectionStrategy: peakSelectionStrategy);

                    if (focusingResult == EventCodeEnum.FOCUS_POS_NEAREDGE)
                    {
                        focusingResult = Focusing(focusparam, callerassembly, peakSelectionStrategy: peakSelectionStrategy);
                    }

                    LoggerManager.Debug($"[NormalFocusing], bruteForce_retry End");

                    if (focusingResult == EventCodeEnum.NONE)
                    {
                        return EventCodeEnum.NONE;
                    }
                }

                if (outRangeFind_retry)
                {
                    LoggerManager.Debug($"[NormalFocusing], outRangeFind_retry Start");

                    focusingResult = Focusing(focusparam, callerassembly, true, peakSelectionStrategy: peakSelectionStrategy);

                    LoggerManager.Debug($"[NormalFocusing], outRangeFind_retry End");

                    if (focusingResult == EventCodeEnum.NONE)
                    {
                        return EventCodeEnum.NONE;
                    }
                }

                timeStamp.Add(new KeyValuePair<string, long>("Focusing Data ADD End", stw.ElapsedMilliseconds));
                stw.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return focusingResult;
        }
    }
}
