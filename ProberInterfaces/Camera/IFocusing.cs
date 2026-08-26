using ProberInterfaces.Param;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using Autofac;
using System.Windows;
using System.Xml.Serialization;
using ProberErrorCode;
using System.ComponentModel;
using LogModule;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public class SelectMetadata : IPointMetadata
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public bool IsSelected { get; set; }
    }

    public interface IFocusing : IFactoryModule
    {
        XyDataSeries<double, double> Dataserices { get; }
        List<XyDataSeries<double, double>> DatasericesList { get; }
        void ShowFocusGraph();
        Type ParamType { get; set; }
        EventCodeEnum Focusing(IFocusParameter focusparam, object callerAssembly, bool isOutRangeFind = false, string SavePassPath = "", string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE);
        EventCodeEnum Focusing_Retry(IFocusParameter focusparam, bool lightChange_retry, bool bruteForce_retry, bool outRangeFind_retry, object callerassembly, int TargetGrayLevel = 0, bool ForcedApplyAutolight = false, string SavePassPath = "", string SaveFailPath = "", PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE);
    }

    [Serializable]
    public abstract class FocusingBase : Autofac.Module, IFocusing
    {
        public IParam DevParam { get; set; }

        private XyDataSeries<double, double> _Dataserices = new XyDataSeries<double, double>();
        public XyDataSeries<double, double> Dataserices
        {
            get { return _Dataserices; }
        }

        private List<XyDataSeries<double, double>> _DatasericesList = new List<XyDataSeries<double, double>>();
        [XmlIgnore, JsonIgnore]
        public List<XyDataSeries<double, double>> DatasericesList
        {
            get { return _DatasericesList; }
        }
        private List<ImageBuffer> _ImageBuffers = new List<ImageBuffer>();
        [XmlIgnore, JsonIgnore]
        public List<ImageBuffer> ImageBuffers
        {
            get { return _ImageBuffers; }
        }

        protected Type ModuleType;

        private bool IsParamSet { get; set; }

        public void DeInitModule()
        {
        }
        //public abstract IFocusParameter FocusParameter { get; set; }

        public abstract Type ParamType { get; set; }

        public FocusingBase()
        {

        }
        public FocusingBase(Type moduleType)
        {

        }

        //public abstract EventCodeEnum Focusing(IFocusParameter focusparam, object callerAssembly = null, bool isOutRangeFind = false);
        //public abstract EventCodeEnum Focusing_Retry(IFocusParameter focusparam, bool lightChange_retry, bool bruteForce_retry, bool outRangeFind_retry, object callerassembly = null);

        //public abstract EventCodeEnum Focusing_Retry(IFocusParameter focusparam, bool lightChange_retry, bool bruteForce_retry, bool outRangeFind_retry, object callerassembly = null, int TargetGrayLevel = 0);

        public abstract EventCodeEnum Focusing(IFocusParameter focusparam,
                                   object callerAssembly = null,
                                   bool isOutRangeFind = false,
                                   string SavePassPath = "",
                                   string SaveFailPath = "",
                                   PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE);

        public abstract EventCodeEnum Focusing_Retry(IFocusParameter focusparam,
                                                     bool lightChange_retry,
                                                     bool bruteForce_retry,
                                                     bool outRangeFind_retry,
                                                     object callerassembly = null,
                                                     int TargetGrayLevel = 0,
                                                     bool ForcedApplyAutolight = false,
                                                     string SavePassPath = "",
                                                     string SaveFailPath = "",
                                                     PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE);

        public ImageBuffer WaitGrab(IFocusParameter focusparam, Rect roi, object callerassembly)
        {
            ICamera cam = this.VisionManager().GetCam(focusparam.FocusingCam.Value);

            ImageBuffer buf = new ImageBuffer();

            try
            {
                buf = this.VisionManager().SingleGrab(cam.GetChannelType(), callerassembly);    // 이미지 그랩 코드

                var signaled = this.VisionManager().DigitizerService[cam.GetDigitizerIndex()].GrabberService.WaitOne(60000);

                int focusval = this.VisionManager().GetFocusValue(buf, roi);    // 이미지 그랩 데이터와 ROI를 가지고 포커스 점수 만들기
                buf.FocusLevelValue = focusval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return buf;
        }
        public ImageBuffer WaitGrab_eGrabber(IFocusParameter focusparam, Rect roi, object callerassembly)
        {
            ICamera cam = this.VisionManager().GetCam(focusparam.FocusingCam.Value);

            ImageBuffer buf = new ImageBuffer();

            try
            {
                buf = this.VisionManager().SingleGrab_egrabber(cam.GetChannelType(), callerassembly);    // 이미지 그랩 코드
                int focusval = this.VisionManager().GetFocusValue(buf, roi);    // 이미지 그랩 데이터와 ROI를 가지고 포커스 점수 만들기
                buf.FocusLevelValue = focusval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return buf;
        }
        //260811 sebas : CoaxLink 카메라용 추가
        protected ImageBuffer WaitGrab_CoaxLinkEx(ICoaxLinkExFocusFrameProvider frameProvider, Rect roi, int timeoutMilliseconds = 3000)
        {
            ImageBuffer imageBuffer = null;

            try
            {
                if (frameProvider == null)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] FrameProvider is null.");

                    return null;
                }

                if (!frameProvider.IsReady)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] Camera is not ready.");

                    return null;
                }

                imageBuffer =
                    frameProvider.WaitNextImage(
                        timeoutMilliseconds);

                if (imageBuffer == null ||
                    imageBuffer.Buffer == null ||
                    imageBuffer.Buffer.Length == 0)
                {
                    LoggerManager.Error(
                        "[CoaxLinkExFocusing] Image buffer is empty.");

                    return null;
                }

                if (imageBuffer.SizeX <= 0 ||
                    imageBuffer.SizeY <= 0)
                {
                    LoggerManager.Error(
                        $"[CoaxLinkExFocusing] Invalid image size. " +
                        $"Width={imageBuffer.SizeX}, " +
                        $"Height={imageBuffer.SizeY}");

                    return null;
                }

                const int focusRoiSize = 960;

                double roiX = (imageBuffer.SizeX - focusRoiSize) / 2.0;
                double roiY = (imageBuffer.SizeY - focusRoiSize) / 2.0;

                roi = new Rect(roiX, roiY, focusRoiSize, focusRoiSize);

                //int focusValue = this.VisionManager().GetFocusValue(imageBuffer, roi);
                int focusValue = this.VisionManager().GetFocusValue_CoaxLinkEx(imageBuffer, roi);

                imageBuffer.FocusLevelValue =
                    focusValue;

                imageBuffer.CapturedTime =
                    DateTime.Now;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                imageBuffer = null;
            }

            return imageBuffer;
        }
        protected void GetFocusResolution(IFocusParameter focusparam, double focusRange, out int focusStep, out double focusResolution, bool NextResolution = false)
        {
            try
            {
                int step = 0;
                double tmpStep = 0.0;
                double minResolution = 40.0; // Focus Depth에 따라 다르게 설정. 카메라 스펙 상 이 범위를 초과하면 각 포커싱 스텝 간격 내에 상이 안 맺힐 수 있다.

                if (focusparam.OutFocusLimit != null)
                {
                    if (NextResolution == true)
                    {
                        minResolution = 40;
                    }
                    else
                    {
                        minResolution = focusparam.OutFocusLimit.Value;
                    }
                }
                else
                {
                    if (focusparam.FocusingCam.Value == EnumProberCam.WAFER_HIGH_CAM)
                        minResolution = 1;

                }

                LoggerManager.Debug($"[FocusingBase] GetFocusResolution()  focusRange: [{focusRange}], OutFocusLimit: [{focusparam.OutFocusLimit.Value}], minResolution [{minResolution}]");

                if (minResolution <= 0)
                {
                    minResolution = 30;
                }

                if (focusparam.DepthOfField == null)
                {
                    focusparam.DepthOfField = new Element<double>();
                    if (focusparam.FocusingCam.Value == EnumProberCam.WAFER_HIGH_CAM)
                        focusparam.DepthOfField.Value = 100;

                }
                if (focusparam.DepthOfField.Value > focusRange)
                {
                    focusStep = 0;
                    focusResolution = 0.0;
                }
                else
                {
                    tmpStep = focusRange / minResolution;       // 가능한 최장 Resolution 기반으로 스텝을 구해본다.

                    if (tmpStep > 10.0)
                    {
                        // 200 마이크론 이상 거리. 그냥 고한다.
                        step = (int)tmpStep; // 스텝 간격에서 소숫점 제거
                        focusStep = step;
                        focusResolution = focusRange / (double)step;    // 수정된 스텝 기반 레졸루션 다시 계산
                    }
                    else if (tmpStep <= 10.0 && tmpStep > 5.0)
                    {
                        // 100 ~ 200 마이크론 
                        tmpStep = focusRange / 15.0;
                        step = (int)tmpStep; // 스텝 간격에서 소숫점 제거
                        focusStep = step;
                        focusResolution = focusRange / (double)step;    // 수정된 스텝 기반 레졸루션 다시 계산
                    }
                    else if (tmpStep <= 5.0 && tmpStep > 2.5)
                    {
                        // 50 ~ 100 마이크론 
                        tmpStep = focusRange / 10.0;
                        step = (int)tmpStep; // 스텝 간격에서 소숫점 제거
                        focusStep = step;
                        focusResolution = focusRange / (double)step;    // 수정된 스텝 기반 레졸루션 다시 계산
                    }
                    else if (tmpStep <= 2.5 && tmpStep > 1.0)
                    {
                        // 20 ~ 50 마이크론 
                        tmpStep = focusRange / 5.0;
                        step = (int)tmpStep; // 스텝 간격에서 소숫점 제거
                        focusStep = step;
                        focusResolution = focusRange / (double)step;    // 수정된 스텝 기반 레졸루션 다시 계산
                    }
                    else if (tmpStep <= 1.0 && tmpStep > 0.5)
                    {
                        // 10 ~ 20 마이크론 
                        tmpStep = focusRange / 3;
                        step = (int)tmpStep; // 스텝 간격에서 소숫점 제거
                        focusStep = step;
                        focusResolution = focusRange / (double)step;    // 수정된 스텝 기반 레졸루션 다시 계산
                    }
                    else
                    {
                        // ~ 10 마이크론 
                        step = (int)focusRange;
                        focusStep = step;
                        focusResolution = 1.0;
                    }

                    if (focusResolution < focusparam.DepthOfField.Value)
                    {
                        step = (int)(focusRange / focusparam.DepthOfField.Value);
                        focusStep = step;
                        focusResolution = focusRange / (double)step;
                    }

                    if (focusStep <= 2)
                    {
                        focusStep = 0;
                        focusResolution = 0;
                    }
                }
            }
            catch (Exception err)
            {
                focusStep = 0;
                focusResolution = 0;
                LoggerManager.Debug($"Failed to intialize focusing resolution and step!");
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected void GetNextResolution(IFocusParameter focusparam, double focusResolution, out int focusStep, out double nextFocusResolution)
        {
            int nextstep;
            double nextRes;

            try
            {
                if (focusResolution > focusparam.DepthOfField.Value)
                {
                    GetFocusResolution(focusparam, focusResolution * 2.0, out nextstep, out nextRes, true);
                    focusStep = nextstep;
                    nextFocusResolution = nextRes;
                }
                else
                {
                    focusStep = 0;
                    nextFocusResolution = 0.0;
                }
            }
            catch (Exception err)
            {
                focusStep = 0;
                focusResolution = 0;
                LoggerManager.Debug($"Failed to get next focusing resolution and step!");
                LoggerManager.Exception(err);
                throw;
            }
        }
        // <-- 260813 sebas : CoaxLink 카메라용 추가
        protected void GetFocusResolution_CoaxLinkEx(IFocusParameter focusparam, double focusRange, out int focusStep, out double focusResolution, bool nextResolution = false)
        {
            try
            {
                int step = 0;
                double tmpStep = 0.0;
                double minResolution = 40.0;

                if (focusparam.OutFocusLimit != null)
                {
                    if (nextResolution)
                    {
                        minResolution = 40.0;
                    }
                    else
                    {
                        minResolution = focusparam.OutFocusLimit.Value;
                    }
                }

                if (minResolution <= 0)
                {
                    minResolution = 30.0;
                }

                /*
                 * CX3에서도 기존 설정된 DepthOfField를 그대로 사용한다.
                 * 값이 없는 경우에만 기본값 생성.
                 */
                if (focusparam.DepthOfField == null)
                {
                    focusparam.DepthOfField = new Element<double>();
                    focusparam.DepthOfField.Value = 1.0;
                }

                if (focusparam.DepthOfField.Value <= 0)
                {
                    focusparam.DepthOfField.Value = 1.0;
                }

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] GetFocusResolution - Range={focusRange:F6}, " +
                    $"OutFocusLimit={focusparam.OutFocusLimit?.Value}, MinResolution={minResolution:F6}, " +
                    $"DOF={focusparam.DepthOfField.Value:F6}, Next={nextResolution}");

                if (focusparam.DepthOfField.Value > focusRange)
                {
                    focusStep = 0;
                    focusResolution = 0.0;
                    return;
                }

                tmpStep = focusRange / minResolution;

                if (tmpStep > 10.0)
                {
                    step = (int)tmpStep;
                    focusStep = step;
                    focusResolution = focusRange / step;
                }
                else if (tmpStep <= 10.0 && tmpStep > 5.0)
                {
                    tmpStep = focusRange / 15.0;
                    step = (int)tmpStep;

                    focusStep = step;
                    focusResolution = focusRange / step;
                }
                else if (tmpStep <= 5.0 && tmpStep > 2.5)
                {
                    tmpStep = focusRange / 10.0;
                    step = (int)tmpStep;

                    focusStep = step;
                    focusResolution = focusRange / step;
                }
                else if (tmpStep <= 2.5 && tmpStep > 1.0)
                {
                    tmpStep = focusRange / 5.0;
                    step = (int)tmpStep;

                    focusStep = step;
                    focusResolution = focusRange / step;
                }
                else if (tmpStep <= 1.0 && tmpStep > 0.5)
                {
                    tmpStep = focusRange / 3.0;
                    step = (int)tmpStep;

                    focusStep = step;
                    focusResolution = focusRange / step;
                }
                else
                {
                    step = (int)focusRange;
                    focusStep = step;
                    focusResolution = 1.0;
                }

                /*
                 * Resolution이 DOF보다 더 작아지지 않도록
                 * 기존 Focusing 로직 유지.
                 */
                if (focusResolution < focusparam.DepthOfField.Value)
                {
                    step = (int)(focusRange / focusparam.DepthOfField.Value);

                    if (step > 0)
                    {
                        focusStep = step;
                        focusResolution = focusRange / step;
                    }
                }

                if (focusStep <= 2)
                {
                    focusStep = 0;
                    focusResolution = 0.0;
                }

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] GetFocusResolution Result - " +
                    $"Range={focusRange:F6}, Step={focusStep}, " +
                    $"Resolution={focusResolution:F6}, DOF={focusparam.DepthOfField.Value:F6}");
            }
            catch (Exception err)
            {
                focusStep = 0;
                focusResolution = 0.0;

                LoggerManager.Debug(
                    "[CoaxLinkExFocusing] Failed to initialize focusing resolution and step.");

                LoggerManager.Exception(err);
                throw;
            }
        }
        protected void GetNextResolution_CoaxLinkEx(IFocusParameter focusparam, double focusResolution, out int focusStep, out double nextFocusResolution)
        {
            try
            {
                focusStep = 0;
                nextFocusResolution = 0.0;

                LoggerManager.Debug(
                    $"[CoaxLinkExFocusing] GetNextResolution CHECK - " +
                    $"CurrentResolution={focusResolution:F6}, " +
                    $"DOF={focusparam.DepthOfField.Value:F6}");

                if (focusResolution > focusparam.DepthOfField.Value)
                {
                    GetFocusResolution_CoaxLinkEx(
                        focusparam,
                        focusResolution * 2.0,
                        out focusStep,
                        out nextFocusResolution,
                        true);

                    LoggerManager.Debug(
                        $"[CoaxLinkExFocusing] GetNextResolution - " +
                        $"CurrentResolution={focusResolution:F6}, " +
                        $"NextRange={(focusResolution * 2.0):F6}, " +
                        $"NextStep={focusStep}, NextResolution={nextFocusResolution:F6}, " +
                        $"DOF={focusparam.DepthOfField.Value:F6}");
                }
                else
                {
                    LoggerManager.Debug(
                        $"[CoaxLinkExFocusing] GetNextResolution END - " +
                        $"Resolution={focusResolution:F6} <= DOF={focusparam.DepthOfField.Value:F6}");

                    focusStep = 0;
                    nextFocusResolution = 0.0;
                }
            }
            catch (Exception err)
            {
                focusStep = 0;
                nextFocusResolution = 0.0;

                LoggerManager.Debug(
                    "[CoaxLinkExFocusing] Failed to get next focusing resolution and step.");

                LoggerManager.Exception(err);
                throw;
            }
        }
        // -->
        public virtual void ShowFocusGraph()
        {
        }

        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                var type = this.GetType();

                var Nullconstructor = type.GetConstructor(Type.EmptyTypes);

                builder.Register(x => Nullconstructor.Invoke(new object[] { })).Named<FocusingBase>(type.FullName);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    #region // 260811 sebas : CoaxLink 카메라용
    public interface ICoaxLinkExFocusFrameProvider
    {
        bool IsReady { get; }

        ImageBuffer WaitNextImage(
            int timeoutMilliseconds = 3000);
    }

    public interface ICoaxLinkExFocusing
    {
        EventCodeEnum Focusing_CoaxLinkEx(
            IFocusParameter focusparam,
            ICoaxLinkExFocusFrameProvider frameProvider,
            object callerAssembly,
            bool isOutRangeFind = false,
            string SavePassPath = "",
            string SaveFailPath = "",
            PeakSelectionStrategy peakSelectionStrategy =
                PeakSelectionStrategy.NONE);

        EventCodeEnum Focusing_Retry_CoaxLinkEx(
            IFocusParameter focusparam,
            ICoaxLinkExFocusFrameProvider frameProvider,
            bool lightChange_retry,
            bool bruteForce_retry,
            bool outRangeFind_retry,
            object callerassembly,
            int TargetGrayLevel = 0,
            bool ForcedApplyAutolight = false,
            string SavePassPath = "",
            string SaveFailPath = "",
            PeakSelectionStrategy peakSelectionStrategy =
                PeakSelectionStrategy.NONE);
    }
    #endregion
}
