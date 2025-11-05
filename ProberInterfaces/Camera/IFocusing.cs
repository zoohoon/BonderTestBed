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
                buf = this.VisionManager().SingleGrab(cam.GetChannelType(), callerassembly);

                var signaled = this.VisionManager().DigitizerService[cam.GetDigitizerIndex()].GrabberService.WaitOne(60000);

                int focusval = this.VisionManager().GetFocusValue(buf, roi);
                buf.FocusLevelValue = focusval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return buf;
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
}
