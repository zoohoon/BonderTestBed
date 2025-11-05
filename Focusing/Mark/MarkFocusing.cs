using System;
using System.Collections.Generic;
using System.Linq;

namespace Focusing.Mark
{
    using FocusGraphControl;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Focus;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using SciChart.Charting.Model.DataSeries;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    [Serializable]
    public class MarkFocusing : FocusingBase, INotifyPropertyChanged
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

        //private LockKey lockObject = new LockKey("Mark Focusing");
        private object lockObject = new object();

        public MarkFocusing()
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
                throw;
            }

        }


        public Rect GetMarkFocusROI(ImageBuffer peakImage, BlobParameter blobParam, ROIParameter roiParam)
        {
            GrabDevPosition roiPos = null;
            try
            {
                blobParam = new BlobParameter();
                roiParam = new ROIParameter();

                blobParam.BlobMinRadius.Value = 150;
                blobParam.BlobThreshHold.Value = 120;
                blobParam.MinBlobArea.Value = 500;
                blobParam.MaxBlobArea.Value = 90000;
                blobParam.MAX_FERET_X.Value = 300;
                blobParam.MAX_FERET_Y.Value = 300;
                blobParam.MIN_FERET_X.Value = 50;
                blobParam.MIN_FERET_Y.Value = 50;

                roiParam.OffsetX.Value = 0;
                roiParam.OffsetY.Value = 0;
                roiParam.Width.Value = 5000;
                roiParam.Height.Value = 5000;

                //double imgPosX = 0.0;
                //double imgPosY = 0.0;
                //this.VisionManager().FindBlob(focusparam.FocusingCam.Value, ref imgPosX, ref imgPosY,127,250,25000,960,960,1);

                ImageBuffer binaryBuf = this.VisionManager().VisionProcessing.Algorithmes.MilDefaultBinarize(peakImage);

                var result = this.VisionManager().VisionProcessing.Algorithmes.FindBlobObject(binaryBuf, blobParam, roiParam, false, true, false);

                roiPos = result.DevicePositions[0];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return new Rect(roiPos.PosX - (roiPos.SizeX / 2), roiPos.PosY + 20 - (roiPos.SizeY / 2), roiPos.SizeX, roiPos.SizeY + 20);
        }

        public override EventCodeEnum Focusing(IFocusParameter focusparam, object callerAssembly, bool isOutRangeFind = false, string SavePassPath = "", string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            double prePosition = 0.0;

            EventCodeEnum focusResult = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.VisionManager().GetVisionProcRaft() != EnumVisionProcRaft.MIL)
                {
                    focusResult = EventCodeEnum.NONE;
                    return focusResult;
                }

                Stopwatch stw = new Stopwatch();
                List<KeyValuePair<string, long>> timeStamp;
                timeStamp = new List<KeyValuePair<string, long>>();
                stw.Start();
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Focusing Start"), stw.ElapsedMilliseconds));
                //atasericesList.Clear();
                XyDataSeries<double, double> dataSeries = null;
                bool continusgrab = false;
                double OrgPos = 0;
                double focusVel = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value).Param.Speed.Value;
                double focusAcc = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value).Param.Acceleration.Value;
                int focusStep;
                Rect focusROI = focusparam.FocusingROI.Value;
                ProbeAxisObject axis = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value);
                double focusResolution = 0;
                try
                {
                    //OrgPos = axis.Status.Position.Actual;
                    double curRefPos = 0.0;
                    this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);
                    OrgPos = curRefPos;

                    if (focusGraph != null)
                        focusGraph.ClearData();
                    //VisionManager.StopGrab(focusParam.FocusingCam);

                    #region // Debug image
#if DEBUG
                    //System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\Logs\images\Focus\");

                    //foreach (FileInfo file in di.GetFiles())
                    //{
                    //    file.Delete();
                    //}
                    //foreach (DirectoryInfo dir in di.GetDirectories())
                    //{
                    //    dir.Delete(true);
                    //}
#endif
                    #endregion

                    double focusRange = focusparam.FocusRange.Value;

                    GetFocusResolution(focusparam, focusRange, out focusStep, out focusResolution);

                    if (focusStep == 0 || focusResolution == 0.0)
                    {
                        focusResult = EventCodeEnum.NONE;
                        return focusResult;
                    }

                    //using (Locker locker = new Locker(lockObject))
                    //{
                    lock (lockObject)
                    {
                        continusgrab = this.VisionManager().ConfirmContinusGrab(focusparam.FocusingCam.Value);
                        this.VisionManager().StopGrab(focusparam.FocusingCam.Value);

                        //if (continusgrab == false)
                        //{
                        //    this.VisionManager().StartGrab(focusparam.FocusingCam.Value);
                        //}

                        Dataserices.AcceptsUnsortedData = true;

                        //focusStep = focusparam.FocusMaxStep.Value;


                        //focusRange = 200;
                        //focusStep = 20;

                        //int focusStep = 15;
                        //focusResolution = focusRange / (double)focusStep;

                        double settling = 0;

                        //==> Limit Range - z 축이 움직일 영역 범위 지정
                        this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);
                        
                        double zAxisLimitCeiling = curRefPos + (focusRange / 2);//==> 상단 영역 좌표
                        double zAxisLimitFloor = curRefPos - (focusRange / 2);//==> 하단 영역 좌표
                        
                        prePosition = curRefPos;

                        //==> THRESHOLD
                        double focusThreshold = focusparam.FocusThreshold.Value;
                        double flatnessThreshold = 70;//focusparam.FlatnessThreshold.Value;
                        double peakRangeThreshold = focusparam.PeakRangeThreshold.Value;
                        double step_dir = 1;
                        bool bChecked = false;      // 포커싱 맨 처음에만 편평도 에러 체크하기 위해서

                        //axisZ.Param.Speed.Value, axisZ.Param.Acceleration.Value
                        ImageBuffers.Clear();
                        //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(@"C:\ProberSystem\Parameters\Vision\Focusing\");
                        //foreach (System.IO.FileInfo file in di.GetFiles())
                        //{
                        //    file.Delete();
                        //}

                        List<ImageBuffer> imageBuffers = new List<ImageBuffer>();
                        ICamera cam = this.VisionManager().GetCam(focusparam.FocusingCam.Value);
                        //WaitGrab(cam, focusParam);      // Clear image buffer.

                        //if (callerAssembly == null)
                        //{
                        //    callerAssembly = this;
                        //}

                        this.VisionManager().SetCaller(focusparam.FocusingCam.Value, callerAssembly);

                        // Modify (->Remove)
                        //this.VisionManager().StartGrab(focusparam.FocusingCam.Value, callerAssembly);
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
                                    ImageBuffer newImageBuffer = WaitGrab(focusparam, focusROI, callerAssembly);
                                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("WaitGrab End"), stw.ElapsedMilliseconds));
                                    
                                    lock (newImageBuffer)
                                    {
                                        double actPos = 0.0;

                                        this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);

                                        actPos = curRefPos;
                                        newImageBuffer.ZHeight = actPos;
                                        imageBuffers.Add(newImageBuffer);
                                    }

                                    this.MotionManager().GetRefPos(axis.AxisType.Value, ref curRefPos);
                                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("ZMove Start"), stw.ElapsedMilliseconds));

                                    this.MotionManager().RelMove(axis, focusResolution * step_dir, focusVel, focusAcc);
                                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("ZMove End"), stw.ElapsedMilliseconds));
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }

                            timeStamp.Add(new KeyValuePair<string, long>(string.Format("Move End"), stw.ElapsedMilliseconds));

                            try
                            {
                                //Save Debug Image
                                if (FocusingStaticParam.SaveImageFlag == true)
                                {
                                    if (Directory.Exists(FocusingStaticParam.SaveDebugImagePath) == false)
                                    {
                                        Directory.CreateDirectory(FocusingStaticParam.SaveDebugImagePath);
                                    }

                                    foreach (var image in imageBuffers)
                                    {
                                        string saveFullPath = $"{FocusingStaticParam.SaveDebugImagePath}\\MK_FV({image.FocusLevelValue})_Z({image.ZHeight}).bmp";

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

                            int minFocusValue = imageBuffers.Min(image => image.FocusLevelValue);

                            foreach (var image in imageBuffers)
                            {
                                image.FiliterdFocusValue = image.FocusLevelValue - minFocusValue;
                            }

                            minFocusValue = imageBuffers.Min(image => image.FiliterdFocusValue);
                            int maxFocusValue = imageBuffers.Max(image => image.FiliterdFocusValue);

                            ImageBuffer maxFocusValueImage = imageBuffers.First(image => image.FiliterdFocusValue == maxFocusValue);

                            foreach (var image in imageBuffers)
                            {
                                LoggerManager.Debug($"Focus Val. @{image.ZHeight}um = {image.FiliterdFocusValue}");
                            }

                            //==>[1] CHECK THRESHOLD FOCUS VALUE

                            imageBuffers = imageBuffers.Where(image => image.FiliterdFocusValue > focusThreshold).ToList();

                            if (imageBuffers.Count == 0)
                            {
                                imageBuffers.Add(maxFocusValueImage);

                                focusResult = EventCodeEnum.FOCUS_VALUE_THRESHOLD;
                            }

                            //==>[2] CHECK FLATNESS

                            double focusMaxTotal = maxFocusValue * focusStep;
                            double focusValueTotal = imageBuffers.Sum(image => image.FiliterdFocusValue);
                            double focusFlatness = focusValueTotal / focusMaxTotal * 100;

                            if (bChecked == false)
                            {
                                if (focusFlatness > flatnessThreshold)
                                {
                                    focusResult = EventCodeEnum.FOCUS_VALUE_FLAT;
                                }

                                bChecked = true;
                            }

                            //==>[3] CHECK DUAL PEAK
                            double focusValueAvgRatio = 1.2;
                            double focusValueAvg = imageBuffers.Average(image => image.FiliterdFocusValue);

                            List<ImageBuffer> peakImageBuffer = new List<ImageBuffer>();

                            for (int i = 1; i < imageBuffers.Count - 1; i++)
                            {
                                if (imageBuffers[i].FiliterdFocusValue > (focusValueAvg * focusValueAvgRatio) && //==> Compare Average Focus Level Value
                                    imageBuffers[i].FiliterdFocusValue > imageBuffers[i - 1].FiliterdFocusValue &&//==> Compare Prev Focus Level Value
                                    imageBuffers[i].FiliterdFocusValue > imageBuffers[i + 1].FiliterdFocusValue)//==> Compare Next Focus Level Value
                                {
                                    peakImageBuffer.Add(imageBuffers[i]);
                                }
                            }

                            if (peakImageBuffer.Count == 0)
                            {
                                peakImageBuffer.Add(maxFocusValueImage);
                            }

                            double maxPeakFocusValue = peakImageBuffer.Max(image => image.FiliterdFocusValue);
                            ImageBuffer firstPeakImageBuffer = imageBuffers.First(image => image.FiliterdFocusValue == maxPeakFocusValue);

                            foreach (ImageBuffer peakImage in peakImageBuffer)
                            {
                                if (Math.Abs(peakImage.ZHeight - firstPeakImageBuffer.ZHeight) > peakRangeThreshold)
                                {
                                    focusResult = EventCodeEnum.FOCUS_VALUE_DUALPEAK;
                                    break;
                                }
                            }

                            //==> Analisys Focusing Status
                            if (focusResult == EventCodeEnum.FOCUS_VALUE_THRESHOLD ||
                                focusResult == EventCodeEnum.FOCUS_VALUE_FLAT ||
                                focusResult == EventCodeEnum.FOCUS_VALUE_DUALPEAK)
                            {
                                //==> Error
                                break;
                            }

                            int targetFocusValue = imageBuffers.Max(image => image.FiliterdFocusValue);

                            int maxFocusValIdx = imageBuffers.FindIndex(image => image.FiliterdFocusValue == maxFocusValue);
                            double tagetZHeight = 0;
                            ImageBuffer targetFocusValueImage = imageBuffers.First(image => image.FiliterdFocusValue == maxFocusValue);

                            tagetZHeight = imageBuffers[maxFocusValIdx].ZHeight;

                            GetNextResolution(focusparam, focusResolution, out focusStep, out focusResolution);

                            step_dir = step_dir * -1;

                            focusRange = focusResolution * (double)focusStep;

                            //==> Focusing Value가 가장 높은 지정으로 척 이동
                            //this.MotionManager().AbsMove(axis, tagetZHeight, focusVel, focusAcc);
                            if (focusStep == 0 || focusResolution == 0.0)
                            {

                                this.MotionManager().AbsMove(axis, tagetZHeight, focusVel, focusAcc);

                                focusResult = EventCodeEnum.NONE;
                                break;
                            }
                            else
                            {
                                this.MotionManager().AbsMove(axis, tagetZHeight - (focusRange / 2) * step_dir, focusVel, focusAcc);
                            }

                            focusparam.FocusResultPos = tagetZHeight;
                            focusparam.FocusValue = imageBuffers[maxFocusValIdx].FocusLevelValue;

                            Thread.Sleep(1);
                        }

                        foreach (var item in imageBuffers)
                        {
                            ImageBuffers.Add(item);
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

                if (focusResult != EventCodeEnum.NONE)
                {
                    this.VisionManager().SetImages(ImageBuffers, IMAGE_LOG_TYPE.FAIL, IMAGE_SAVE_TYPE.BMP, IMAGE_PROCESSING_TYPE.FOCUSING, focusResult);

                    // Go back to previous height
                    this.MotionManager().AbsMove(axis, OrgPos, focusVel, focusAcc);
                }
                else
                {
                    this.VisionManager().SetImages(ImageBuffers, IMAGE_LOG_TYPE.PASS, IMAGE_SAVE_TYPE.BMP, IMAGE_PROCESSING_TYPE.FOCUSING, focusResult);
                }

                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Focusing End"), stw.ElapsedMilliseconds));

                if(stw.ElapsedMilliseconds > 5000)
                {
                    foreach (var item in timeStamp)
                    {
                        LoggerManager.Debug($"MarkFocusing TimeStamp - Desc: {item.Key}, Time: {item.Value}");
                    }
                }
                stw.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return focusResult;
        }

        public override EventCodeEnum Focusing_Retry(IFocusParameter focusparam, bool lightChange_retry, bool bruteForce_retry, bool outRangeFind_retry, object callerassembly, int TargetGrayLevel = 0, bool ForcedApplyAutolight = false, string SavePassPath = "", string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            stw.Start();
            ProbeAxisObject axis = this.MotionManager().GetAxis(focusparam.FocusingAxis.Value);
            double curAxis = axis.Status.Position.Actual;

            //int grayLevel = AutoLightManager.GetGrayLevel(focusParam.FocusingCam);
            EventCodeEnum focusingResult = EventCodeEnum.UNDEFINED;

            try
            {
                timeStamp.Add(new KeyValuePair<string, long>("Focusing First start", stw.ElapsedMilliseconds));

                focusingResult = Focusing(focusparam, callerassembly);
                LoggerManager.Debug($"[MarkFocusing] Focusing_Retry(): {focusingResult}");

                timeStamp.Add(new KeyValuePair<string, long>("Focusing First End", stw.ElapsedMilliseconds));
                stw.Stop();

                LoggerManager.Debug($"Focusing Time = {stw.ElapsedMilliseconds}ms");

                focusparam.FocusTime = stw.ElapsedMilliseconds;

                if (focusingResult == EventCodeEnum.NONE)
                    return EventCodeEnum.NONE;

                stw.Restart();
                if (lightChange_retry)
                {
                    this.MotionManager().AbsMove(axis, curAxis, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                    const int setGralLevelValue = 186;
                    this.AutoLightAdvisor().SetGrayLevel(focusparam.FocusingCam.Value, setGralLevelValue);

                    focusingResult = Focusing(focusparam, callerassembly);

                    LoggerManager.Debug($"[MarkFocusing] Focusing_Retry(): {focusingResult} - lightChange_retry");
                    if (focusingResult == EventCodeEnum.NONE)
                        return EventCodeEnum.NONE;
                }

                if (bruteForce_retry)
                {
                    this.MotionManager().AbsMove(axis, curAxis, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                    //focusparam bruteForceParam = new focusparam(focusParam);
                    //bruteForceParam.DepthOfField = 1;
                    //bruteForceParam.FocusMaxStep = (int)bruteForceParam.FocusRange;
                    //focusingResult = Focusing(bruteForceParam, callerassembly);
                    focusingResult = Focusing(focusparam, callerassembly);
                    LoggerManager.Debug($"[MarkFocusing] Focusing_Retry(): {focusingResult} - bruteForce_retry");
                    if (focusingResult == EventCodeEnum.NONE)
                        return EventCodeEnum.NONE;
                }

                if (outRangeFind_retry)
                {
                    focusingResult = Focusing(focusparam, callerassembly, true);
                    LoggerManager.Debug($"[MarkFocusing] Focusing_Retry(): {focusingResult} - outRangeFind_retry");
                    if (focusingResult == EventCodeEnum.NONE)
                        return EventCodeEnum.NONE;
                }
                timeStamp.Add(new KeyValuePair<string, long>("Focusing Data ADD End", stw.ElapsedMilliseconds));
                stw.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return focusingResult;
        }
    }
}
