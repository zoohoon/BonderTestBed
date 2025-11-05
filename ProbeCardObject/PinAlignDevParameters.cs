using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ProbeCardObject
{
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;

    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using LogModule;
    using Focusing;
    using System.Runtime.Serialization;
    using ProberInterfaces;
    using ProberInterfaces.Param;

    public enum PinAlignIntervalIdx
    {
        Registration = 0,
        DieInterval,
        WaferInterval,
        TimeInterval,
        NeedleCleaning,
        Soaking,
        PolishWafer,
        Manual,
    }
    public enum WAFERINTERVALMODE
    {
        ALWAYS = 0,
        INTERVALMODE = 1,
        STATICMODE = 2
    }
    public enum TIMEINTERVALMODE
    {
        DISABLE = 0,
        ENABLE = 1
    }
    public enum DIEINTERVALMODE
    {
        DISABLE = 0,
        ENABLE = 1
    }

    public enum FOCUSINGRAGE
    {
        RANGE_25 = 25,
        RANGE_50 = 50,
        RANGE_100 = 100,
        RANGE_200 = 200,
        RANGE_300 = 300,
        RANGE_400 = 400,
        RANGE_500 = 500,
        RANGE_600 = 600,
        RANGE_700 = 700,
        RANGE_800 = 800,
        RANGE_900 = 900,
        RANGE_1000 = 1000,
    }
    public enum PINALIGNONOFF
    {
        DISABLE = 0,
        ENABLE = 1,
    }
    public enum PINALIGNMODE
    {
        EMUL = 0,
        PROCESSING = 1,
    }

    public enum USERPINHEIGHT
    {
        LOWEST = 0,
        AVERAGE = 1,
        HIGHEST = 2
    }
    public enum BaseFocusingEnable
    {
        DISABLE = 0,
        ENABLE = 1
    }
    public enum BaseFocusingFirstPin
    {
        FirstPin = 0,
        AllPin = 1
    }

    public enum PinLowAlignTypeEnum
    {
        MANUAL = 0,
        IMAGE,
        DEFAULT,
    }

    public enum SizeValidationOption
    {
        ALL = 0,
        INDIVIDUAL
    }

    [Serializable]
    [XmlInclude(typeof(PinAlignDevParameters))]
    public class PinAlignDevParameters : INotifyPropertyChanged, IDeviceParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private PinAlignSettignParameter MakePinAlignSettingParam(PINALIGNSOURCE source)
        {
            PinAlignSettignParameter retval = null;

            try
            {
                retval = new PinAlignSettignParameter();
                retval.SourceName.Value = source;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum Init()
        {
            try
            {
                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (PinLowAlignParam == null)
                {
                    PinLowAlignParam = new PinLowAlignParameter();
                }

                if (PinHighAlignParam == null)
                {
                    PinHighAlignParam = new PinHighAlignParameter();
                }

                if (PinHighAlignParam.HighAlignKeyParameter == null)
                {
                    PinHighAlignParam.HighAlignKeyParameter = new PinHighAlignKeyParameter();
                }

                // 초기값 설정, 디폴트 값 설정 (20%)
                if (PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeXMinimumMargin.Value == 0)
                {
                    PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeXMinimumMargin.Value = 20;
                }

                if (PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeXMaximumMargin.Value == 0)
                {
                    PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeXMaximumMargin.Value = 20;
                }

                if (PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeYMinimumMargin.Value == 0)
                {
                    PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeYMinimumMargin.Value = 20;
                }

                if (PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeYMaximumMargin.Value == 0)
                {
                    PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeYMaximumMargin.Value = 20;
                }


                // Modified by UNDEFINED - 200407
                // SettingParameter에 소스를 추가하면서 정리 함.
                // PinAlignSettignParam은 UNDEFINED를 제외하고 모든 타입에 대한 파라미터 세트가 구성되어 있어야 한다.
                //int SourceCount = System.Enum.GetNames(typeof(PINALIGNSOURCE)).Length;

                if (PinAlignSettignParam == null)
                {
                    PinAlignSettignParam = new ObservableCollection<PinAlignSettignParameter>();

                    foreach (PINALIGNSOURCE source in (PINALIGNSOURCE[])System.Enum.GetValues(typeof(PINALIGNSOURCE)))
                    {
                        if (source != PINALIGNSOURCE.UNDEFINED)
                        {
                            PinAlignSettignParam.Add(MakePinAlignSettingParam(source));
                        }
                    }
                }
                else
                {
                    // SourceName 프로퍼티가 존재하지 않던 시절 만들어진 파라미터 세트에 대한 예외 처리를 해줘야 한다.

                    // WAFER_INTERVAL => 0
                    // DIE_INTERVAL => 1
                    // TIME_INTERVAL => 2
                    // NEEDLE_CLEANING => 3
                    // POLISH_WAFER => 4
                    // SOAKING => 5
                    // PIN_REGISTRATION => 6

                    if (PinAlignSettignParam.Count == 7)
                    {
                        PinAlignSettignParam[0].SourceName.Value = PINALIGNSOURCE.WAFER_INTERVAL;
                        PinAlignSettignParam[1].SourceName.Value = PINALIGNSOURCE.DIE_INTERVAL;
                        PinAlignSettignParam[2].SourceName.Value = PINALIGNSOURCE.TIME_INTERVAL;
                        PinAlignSettignParam[3].SourceName.Value = PINALIGNSOURCE.NEEDLE_CLEANING;
                        PinAlignSettignParam[4].SourceName.Value = PINALIGNSOURCE.POLISH_WAFER;
                        PinAlignSettignParam[5].SourceName.Value = PINALIGNSOURCE.SOAKING;
                        PinAlignSettignParam[6].SourceName.Value = PINALIGNSOURCE.PIN_REGISTRATION;
                    }
                    else
                    {
                        // 파라미터 유효성 확인

                        int SourceLength = Enum.GetNames(typeof(PINALIGNSOURCE)).Length;

                        // UNDEFINED를 제외한 총 9개가 존재해야 한다.
                        if (PinAlignSettignParam.Count == SourceLength - 1)
                        {
                            LoggerManager.Debug($"[PinAlignDevParameters] Init() : PinAlignSettingParam Count is OK.");
                        }
                        else
                        {
                            LoggerManager.Debug($"[PinAlignDevParameters] Init() : PinAlignSettingParam Count is abnormal. {PinAlignSettignParam.Count}");

                            // 모든 파라미터 세트가 구성되어 있는지 확인.
                            // 없는 세트의 경우 추가 됨.

                            foreach (PINALIGNSOURCE source in (PINALIGNSOURCE[])System.Enum.GetValues(typeof(PINALIGNSOURCE)))
                            {
                                PinAlignSettignParameter settingparam = PinAlignSettignParam.FirstOrDefault(s => s.SourceName.Value == source);

                                if (source != PINALIGNSOURCE.UNDEFINED)
                                {
                                    if (settingparam == null)
                                    {
                                        PinAlignSettignParam.Add(MakePinAlignSettingParam(source));
                                    }
                                }
                            }
                        }
                    }
                }

                if (PinPadMatchParam == null)
                {
                    PinPadMatchParam = new PinPadMatchParameter();
                }

                if (PinAlignInterval == null)
                {
                    PinAlignInterval = new PinAlignIntervalParameter();
                }

                // 핀 센터 업데이트
                double tmpcenx;
                double tmpceny;
                double tmpdutcenx;
                double tmpdutceny;

                IProbeCard probeCard = this.GetParam_ProbeCard();

                probeCard.CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);

                probeCard.ProbeCardDevObjectRef.PinCenX = tmpcenx;
                probeCard.ProbeCardDevObjectRef.PinCenY = tmpceny;
                probeCard.ProbeCardDevObjectRef.DutCenX = tmpdutcenx;
                probeCard.ProbeCardDevObjectRef.DutCenY = tmpdutceny;

                LoggerManager.Debug($"UpdataPinData() : pin cen = ({probeCard.ProbeCardDevObjectRef.PinCenX}, {probeCard.ProbeCardDevObjectRef.PinCenY}), " +
                                    $"dut cen = ({probeCard.ProbeCardDevObjectRef.DutCenX}, {probeCard.ProbeCardDevObjectRef.DutCenY})");


                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //for (int i = 0; i < 2; i++)
                //{
                //    PinLowAlignParam.Patterns.Add(new PinLowAlignPatternInfo());
                //    PinLowAlignParam.Patterns[i].PMParameter.ModelFilePath.Value
                //        = this.FileManager().GetDeviceParamFullPath(@"PinAlignParam\PatternImages\", string.Format("Emul_Pattern{0:D2}", i));
                //    PinLowAlignParam.Patterns[i].PMParameter.PatternFileExtension.Value
                //        = ".bmp";

                //    string FullPath = PinLowAlignParam.Patterns[i].PMParameter.ModelFilePath.Value
                //        + PinLowAlignParam.Patterns[i].PMParameter.PatternFileExtension.Value;

                //    if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                //    {
                //        Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                //    }

                //    if (File.Exists(FullPath) == false)
                //    {
                //        using (FileStream fs = File.Create(FullPath))
                //        {
                //        }
                //    }

                //    PinLowAlignParam.Patterns[i].PatternState.Value = PatternStateEnum.READY;
                //    PinLowAlignParam.Patterns[i].CamType.Value = EnumProberCam.PIN_LOW_CAM;
                //    PinLowAlignParam.Patterns[i].X.Value = -10000 + i;
                //    PinLowAlignParam.Patterns[i].Y.Value = -10000 + i;
                //    PinLowAlignParam.Patterns[i].Z.Value = -10000 + i;
                //}

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                UserPinHeight.Value = USERPINHEIGHT.AVERAGE;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            //PinAlignParam.PinLowFocusParam.FocusingAxis = EnumAxisConstants.Z;
            //PinAlignParam.PinHighFocusParam.FocusingAxis = EnumAxisConstants.Z;

            return retVal;
        }


        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                if (PinAlignSettignParam == null)
                    PinAlignSettignParam = new ObservableCollection<PinAlignSettignParameter>();

                PinAlignInterval = new PinAlignIntervalParameter();
                EnableDilation.Value = false;
                RefPinRegoffsetX.Value = 0;
                RefPinRegoffsetY.Value = 0;
                TargetPinRegoffsetX.Value = 0;
                TargetPinRegoffsetY.Value = 0;

                UserPinHeight.Value = USERPINHEIGHT.AVERAGE;
                PinLowAlignEnable.Value = false;
                //SetEmulParam();

                retVal = EventCodeEnum.NONE;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        #endregion

        #region PinLowAlignEnable
        private Element<bool> _PinLowAlignEnable = new Element<bool>();
        //public double PinHeight
        public Element<bool> PinLowAlignEnable
        {
            get { return _PinLowAlignEnable; }
            set
            {
                if (value != _PinLowAlignEnable)
                {
                    _PinLowAlignEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region PinLowAlignRetryAfterPinHighEnable
        //Pin High Align 실패 시, Low Align 로 retry 를 할지 말지에 대한 설정.

        private Element<bool> _PinLowAlignRetryAfterPinHighEnable
             = new Element<bool>() { Value = false };
        public Element<bool> PinLowAlignRetryAfterPinHighEnable
        {
            get { return _PinLowAlignRetryAfterPinHighEnable; }
            set
            {
                if (value != _PinLowAlignRetryAfterPinHighEnable)
                {
                    _PinLowAlignRetryAfterPinHighEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion



        #region ==> PinAlignMode
        //private double _PinHeight;
        private Element<PINALIGNMODE> _PinAlignMode = new Element<PINALIGNMODE>();
        //public double PinHeight
        public Element<PINALIGNMODE> PinAlignMode
        {
            get { return _PinAlignMode; }
            set
            {
                if (value != _PinAlignMode)
                {
                    _PinAlignMode = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion               

        //public PinAlignLastAlignResult PinAlignLastAlignResult { get; set; }
        //= new PinAlignLastAlignResult();

        private Element<EnumThresholdType> _EnableAutoThreshold = new Element<EnumThresholdType>();
        public Element<EnumThresholdType> EnableAutoThreshold
        {
            get { return _EnableAutoThreshold; }
            set
            {
                if (value != _EnableAutoThreshold)
                {
                    _EnableAutoThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _EnableDilation = new Element<bool>();
        public Element<bool> EnableDilation
        {
            get { return _EnableDilation; }
            set
            {
                if (value != _EnableDilation)
                {
                    _EnableDilation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _RefPinRegoffsetX = new Element<double>();
        public Element<double> RefPinRegoffsetX
        {
            get { return _RefPinRegoffsetX; }
            set
            {
                if (value != _RefPinRegoffsetX)
                {
                    _RefPinRegoffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _RefPinRegoffsetY = new Element<double>();
        public Element<double> RefPinRegoffsetY
        {
            get { return _RefPinRegoffsetY; }
            set
            {
                if (value != _RefPinRegoffsetY)
                {
                    _RefPinRegoffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TargetPinRegoffsetX = new Element<double>();
        public Element<double> TargetPinRegoffsetX
        {
            get { return _TargetPinRegoffsetX; }
            set
            {
                if (value != _TargetPinRegoffsetX)
                {
                    _TargetPinRegoffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _TargetPinRegoffsetY = new Element<double>();
        public Element<double> TargetPinRegoffsetY
        {
            get { return _TargetPinRegoffsetY; }
            set
            {
                if (value != _TargetPinRegoffsetY)
                {
                    _TargetPinRegoffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }


        private FocusParameter _FocusParam;
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set
            {
                if (value != _FocusParam)
                {
                    _FocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region ==> PinAlignMode
        //private double _PinHeight;
        private Element<USERPINHEIGHT> _UserPinHeight = new Element<USERPINHEIGHT>();
        //public double PinHeight
        public Element<USERPINHEIGHT> UserPinHeight
        {
            get { return _UserPinHeight; }
            set
            {
                if (value != _UserPinHeight)
                {
                    _UserPinHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinHeight
        private Element<double> _PinHeight = new Element<double>();
        public Element<double> PinHeight
        {
            get { return _PinHeight; }
            set
            {
                if (value != _PinHeight)
                {
                    _PinHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion               

        #region ==> PinFocusingRange
        //private double _PinHeight;
        private Element<FOCUSINGRAGE> _PinFocusingRange = new Element<FOCUSINGRAGE>();
        //public double PinHeight
        public Element<FOCUSINGRAGE> PinFocusingRange
        {
            get { return _PinFocusingRange; }
            set
            {
                if (value != _PinFocusingRange)
                {
                    _PinFocusingRange = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion               

        #region ==> Low Align Type
        private Element<PinLowAlignTypeEnum> _Type = new Element<PinLowAlignTypeEnum>();
        public Element<PinLowAlignTypeEnum> Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion    

        #region ==> PinAlignSettignParameter
        private ObservableCollection<PinAlignSettignParameter> _PinAlignSettignParam = new ObservableCollection<PinAlignSettignParameter>();
        public ObservableCollection<PinAlignSettignParameter> PinAlignSettignParam
        {
            get { return _PinAlignSettignParam; }
            set
            {
                if (value != _PinAlignSettignParam)
                {
                    _PinAlignSettignParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> PinAlignIntervalParameter
        private PinAlignIntervalParameter _PinAlignInterval;
        public PinAlignIntervalParameter PinAlignInterval
        {
            get { return _PinAlignInterval; }
            set
            {
                if (value != _PinAlignInterval)
                {
                    _PinAlignInterval = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinLowAlignParameter(TeachPin)      
        private PinTeachParameter _TeachPinParam = new PinTeachParameter();
        public PinTeachParameter TeachPinParam
        {
            get { return _TeachPinParam; }
            set
            {
                if (value != _TeachPinParam)
                {
                    _TeachPinParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion         

        #region ==> PinLowAlignParameter

        private PinLowAlignParameter _PinLowAlignParam = new PinLowAlignParameter();
        public PinLowAlignParameter PinLowAlignParam
        {
            get { return _PinLowAlignParam; }
            set
            {
                if (value != _PinLowAlignParam)
                {
                    _PinLowAlignParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinHighAlignParameter

        private PinHighAlignParameter _PinHighAlignParam = new PinHighAlignParameter();
        public PinHighAlignParameter PinHighAlignParam
        {
            get { return _PinHighAlignParam; }
            set
            {
                if (value != _PinHighAlignParam)
                {
                    _PinHighAlignParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinPadMatchParameter

        private PinPadMatchParameter _PinPadMatchParam = new PinPadMatchParameter();
        public PinPadMatchParameter PinPadMatchParam
        {
            get { return _PinPadMatchParam; }
            set
            {
                if (value != _PinPadMatchParam)
                {
                    _PinPadMatchParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region // PinPlaneAdjustParameter
        private PinPlaneAdjustParameter _PinPlaneAdjustParam = new PinPlaneAdjustParameter();
        public PinPlaneAdjustParameter PinPlaneAdjustParam
        {
            get { return _PinPlaneAdjustParam; }
            set
            {
                if (value != _PinPlaneAdjustParam)
                {
                    _PinPlaneAdjustParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinHighAlignBaseFocusingParameter
        private PinHighAlignBaseFocusingParameter _PinHighAlignBaseFocusingParam = new PinHighAlignBaseFocusingParameter();
        public PinHighAlignBaseFocusingParameter PinHighAlignBaseFocusingParam
        {
            get { return _PinHighAlignBaseFocusingParam; }
            set
            {
                if (value != _PinHighAlignBaseFocusingParam)
                {
                    _PinHighAlignBaseFocusingParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "PinAlignParam";
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "PinAlignParam.json";

        public PinAlignDevParameters()
        {
            //_PinAlignEnable.Value = PINALIGNONOFF.DISABLE;
            _PinAlignMode.Value = PINALIGNMODE.PROCESSING;
            _PinHeight.Value = -9000;
            _PinFocusingRange.Value = FOCUSINGRAGE.RANGE_300;
            _PinLowAlignEnable.Value = false;
            _PinPadMatchParam = new PinPadMatchParameter();
            _TeachPinParam = new PinTeachParameter();
        }
        public void SetElementMetaData()
        {
            PinPlaneAdjustParam.EnablePinPlaneCompensation.CategoryID = "10021001";
            PinPlaneAdjustParam.EnablePinPlaneCompensation.ElementName = "EnablePinPlaneCompensation";
            PinPlaneAdjustParam.EnablePinPlaneCompensation.Description = "Enable / Disable Pin Plane Adjust By Chuck Tilt";

            PinPlaneAdjustParam.MaxCompHeight.CategoryID = "10021001";
            PinPlaneAdjustParam.MaxCompHeight.ElementName = "MaxCompHeight";
            PinPlaneAdjustParam.MaxCompHeight.Description = "Maximum Pin Plane Adjust Height";
            PinPlaneAdjustParam.MaxCompHeight.Unit = "um";
            PinPlaneAdjustParam.MaxCompHeight.LowerLimit = 0;
            PinPlaneAdjustParam.MaxCompHeight.UpperLimit = 60;

            PinPlaneAdjustParam.CompRatio.CategoryID = "10021001";
            PinPlaneAdjustParam.CompRatio.ElementName = "Comp. Ratio";
            PinPlaneAdjustParam.CompRatio.Description = "Compensation Ratio. (Actual compensation / Measured Height Diffs.)";
            PinPlaneAdjustParam.CompRatio.Unit = "um";
            PinPlaneAdjustParam.CompRatio.LowerLimit = 0;
            PinPlaneAdjustParam.CompRatio.UpperLimit = 1.5;

            PinLowAlignEnable.CategoryID = "10021001";
            PinLowAlignEnable.ElementName = "EnablePinLowAlign";
            PinLowAlignEnable.Description = "Enable / Disable Pin low alignment";

            UserPinHeight.CategoryID = "10021001";
            UserPinHeight.ElementName = "Pin Height Mode";
            UserPinHeight.Description = "Mode used to calculate pin height (Lowest / Average / Highest)";

            EnableDilation.CategoryID = "10021001";
            EnableDilation.ElementName = "Enable Dilation pin tip size";
            EnableDilation.Description = "Enable / Disable Dilation pin tip size";

            PinLowAlignRetryAfterPinHighEnable.CategoryID = "10021001";
            PinLowAlignRetryAfterPinHighEnable.ElementName = "EnablePinLowRetryAfterPinHigh";
            PinLowAlignRetryAfterPinHighEnable.Description = "Enable / Disable retry pin low alignment after pin high alignment fail";


            RefPinRegoffsetX.CategoryID = "10025001";
            RefPinRegoffsetX.ElementName = "Ref Pin Registration Offset X";
            RefPinRegoffsetX.Description = "To check the difference value from the value of the previous data when re-registering the ref pin position";
            RefPinRegoffsetX.Unit = "um";
            RefPinRegoffsetX.LowerLimit = 0;
            RefPinRegoffsetX.UpperLimit = 1000000;

            RefPinRegoffsetY.CategoryID = "10025001";
            RefPinRegoffsetY.ElementName = "Ref Pin Registration Offset Y";
            RefPinRegoffsetY.Description = "To check the difference value from the value of the previous data when re-registering the ref pin position";
            RefPinRegoffsetY.Unit = "um";
            RefPinRegoffsetY.LowerLimit = 0;
            RefPinRegoffsetY.UpperLimit = 1000000;
        }
    }
    [Serializable]
    public class PinAlignSettignParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public List<object> Nodes { get; set; }

        private Element<PINALIGNSOURCE> _SourceName = new Element<PINALIGNSOURCE>();
        public Element<PINALIGNSOURCE> SourceName
        {
            get { return _SourceName; }
            set
            {
                if (value != _SourceName)
                {
                    _SourceName = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _PinCount;
        private Element<int> _PinCount = new Element<int>();
        //[XmlAttribute("PinCount")]
        //public int PinCount
        public Element<int> PinCount
        {
            get { return _PinCount; }
            set
            {
                if (value != _PinCount)
                {
                    _PinCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private bool _UpdateAlignResultX;
        //private Element<bool> _UpdateAlignResultX = new Element<bool>();
        ////[XmlAttribute("UpdateAlignResultX")]
        ////public bool UpdateAlignResultX
        //public Element<bool> UpdateAlignResultX
        //{
        //    get { return _UpdateAlignResultX; }
        //    set
        //    {
        //        if (value != _UpdateAlignResultX)
        //        {
        //            _UpdateAlignResultX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        ////private bool _UpdateAlignResultY;
        //private Element<bool> _UpdateAlignResultY = new Element<bool>();
        ////[XmlAttribute("UpdateAlignResultY")]
        ////public bool UpdateAlignResultY
        //public Element<bool> UpdateAlignResultY
        //{
        //    get { return _UpdateAlignResultY; }
        //    set
        //    {
        //        if (value != _UpdateAlignResultY)
        //        {
        //            _UpdateAlignResultY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        ////private bool _UpdateAlignResultZ;
        //private Element<bool> _UpdateAlignResultZ = new Element<bool>();
        ////[XmlAttribute("UpdateAlignResultZ")]
        ////public bool UpdateAlignResultZ
        //public Element<bool> UpdateAlignResultZ
        //{
        //    get { return _UpdateAlignResultZ; }
        //    set
        //    {
        //        if (value != _UpdateAlignResultZ)
        //        {
        //            _UpdateAlignResultZ = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private PinCoordinate _EachPinPosition = new PinCoordinate();
        //public PinCoordinate EachPinPosition
        //{
        //    get { return _EachPinPosition; }
        //    set
        //    {
        //        if (value != _EachPinPosition)
        //        {
        //            _EachPinPosition = value;
        //            NotifyPropertyChanged("EachPinPosition");
        //        }
        //    }
        //}

        private Element<double> _EachPinToleranceX = new Element<double>();
        public Element<double> EachPinToleranceX
        {
            get
            {
                return _EachPinToleranceX;
            }
            set
            {
                if (value != _EachPinToleranceX)
                {
                    _EachPinToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _EachPinToleranceY = new Element<double>();
        public Element<double> EachPinToleranceY
        {
            get
            {
                return _EachPinToleranceY;
            }
            set
            {
                if (value != _EachPinToleranceY)
                {
                    _EachPinToleranceY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _EachPinToleranceZ = new Element<double>();
        public Element<double> EachPinToleranceZ
        {
            get
            {
                return _EachPinToleranceZ;
            }
            set
            {
                if (value != _EachPinToleranceZ)
                {
                    _EachPinToleranceZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private PinCoordinate _CardPosition = new PinCoordinate();
        //public PinCoordinate CardPosition
        //{
        //    get { return _CardPosition; }
        //    set
        //    {
        //        if (value != _CardPosition)
        //        {
        //            _CardPosition = value;
        //            NotifyPropertyChanged("CardPosition");
        //        }
        //    }
        //}

        private Element<double> _CardCenterToleranceX = new Element<double>();
        public Element<double> CardCenterToleranceX
        {
            get
            {
                return _CardCenterToleranceX;
            }
            set
            {
                if (value != _CardCenterToleranceX)
                {
                    _CardCenterToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardCenterToleranceY = new Element<double>();
        public Element<double> CardCenterToleranceY
        {
            get
            {
                return _CardCenterToleranceY;
            }
            set
            {
                if (value != _CardCenterToleranceY)
                {
                    _CardCenterToleranceY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CardCenterToleranceZ = new Element<double>();
        public Element<double> CardCenterToleranceZ
        {
            get
            {
                return _CardCenterToleranceZ;
            }
            set
            {
                if (value != _CardCenterToleranceZ)
                {
                    _CardCenterToleranceZ = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _MinMaxZDiffLimit = new Element<int>();
        public Element<int> MinMaxZDiffLimit
        {
            get { return _MinMaxZDiffLimit; }
            set
            {
                if (value != _MinMaxZDiffLimit)
                {
                    _MinMaxZDiffLimit = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private int _SamplePinAlignmentPinCount;
        private Element<int> _SamplePinAlignmentPinCount = new Element<int>();
        //[XmlAttribute("SamplePinAlignmentPinCount")]
        //public int SamplePinAlignmentPinCount
        public Element<int> SamplePinAlignmentPinCount
        {
            get { return _SamplePinAlignmentPinCount; }
            set
            {
                if (value != _SamplePinAlignmentPinCount)
                {
                    _SamplePinAlignmentPinCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private PinCoordinate _SamplePinAlignmentTolerance = new PinCoordinate();
        //public PinCoordinate SamplePinAlignmentTolerance
        //{
        //    get { return _SamplePinAlignmentTolerance; }
        //    set
        //    {
        //        if (value != _SamplePinAlignmentTolerance)
        //        {
        //            _SamplePinAlignmentTolerance = value;
        //            NotifyPropertyChanged("SamplePinAlignmentTolerance");
        //        }
        //    }
        //}
        private Element<double> _SamplePinToleranceX = new Element<double>() { UpperLimit = 10, LowerLimit = 1 };
        public Element<double> SamplePinToleranceX
        {
            get
            {
                return _SamplePinToleranceX;
            }
            set
            {
                if (value != _SamplePinToleranceX)
                {
                    _SamplePinToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SamplePinToleranceY = new Element<double>() { UpperLimit = 10, LowerLimit = 1 };
        public Element<double> SamplePinToleranceY
        {
            get
            {
                return _SamplePinToleranceY;
            }
            set
            {
                if (value != _SamplePinToleranceY)
                {
                    _SamplePinToleranceY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SamplePinToleranceZ = new Element<double>() { UpperLimit = 10, LowerLimit = 1 };
        public Element<double> SamplePinToleranceZ
        {
            get
            {
                return _SamplePinToleranceZ;
            }
            set
            {
                if (value != _SamplePinToleranceZ)
                {
                    _SamplePinToleranceZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private int _FailureDetermineThreshold;
        //private Element<int> _FailureDetermineThreshold = new Element<int>();
        ////[XmlAttribute("FailureDeterminethreshold")]
        ////public int FailureDeterminethreshold
        //public Element<int> FailureDetermineThreshold
        //{
        //    get { return _FailureDetermineThreshold; }
        //    set
        //    {
        //        if (value != _FailureDetermineThreshold)
        //        {
        //            _FailureDetermineThreshold = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        public PinAlignSettignParameter()
        {
            _PinCount.Value = 4;
            //_UpdateAlignResultX.Value = true;
            //_UpdateAlignResultY.Value = true;
            //_UpdateAlignResultZ.Value = true;
            _EachPinToleranceX.Value = 50;
            _EachPinToleranceY.Value = 50;
            _EachPinToleranceZ.Value = 100;
            _CardCenterToleranceX.Value = 50;
            _CardCenterToleranceY.Value = 50;
            _CardCenterToleranceZ.Value = 20;

            _MinMaxZDiffLimit.Value = 150;
            _SamplePinAlignmentPinCount.Value = 0;

            _SamplePinToleranceX.Value = 10;
            _SamplePinToleranceY.Value = 10;
            _SamplePinToleranceZ.Value = 10;

            //_FailureDetermineThreshold.Value = 50;
        }
        public PinAlignSettignParameter(PinAlignSettignParameter param)
        {
            _PinCount = param.PinCount;
            //_UpdateAlignResultX = param.UpdateAlignResultX;
            //_UpdateAlignResultY = param.UpdateAlignResultY;
            //_UpdateAlignResultZ = param.UpdateAlignResultZ;
            //_EachPinPosition = new PinCoordinate(param.EachPinPosition);
            //_CardPosition = new PinCoordinate(param.CardPosition);
            _EachPinToleranceX = param.EachPinToleranceX;
            _EachPinToleranceY = param.EachPinToleranceY;
            _EachPinToleranceZ = param.EachPinToleranceZ;
            _CardCenterToleranceX.Value = param.CardCenterToleranceX.Value;
            _CardCenterToleranceY.Value = param.CardCenterToleranceY.Value;
            _CardCenterToleranceZ.Value = param.CardCenterToleranceZ.Value;


            _MinMaxZDiffLimit = param.MinMaxZDiffLimit;
            _SamplePinAlignmentPinCount = param.SamplePinAlignmentPinCount;
            //_SamplePinAlignmentTolerance = new PinCoordinate(param.SamplePinAlignmentTolerance);
            _SamplePinToleranceX = param.SamplePinToleranceX;
            _SamplePinToleranceY = param.SamplePinToleranceY;
            _SamplePinToleranceZ = param.SamplePinToleranceZ;

            //_FailureDetermineThreshold = param.FailureDetermineThreshold;
        }
    }
    [Serializable]
    public class PinAlignIntervalParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }


        // 마지막 얼라인 된 순간부터 진행된 총 다이 갯수를 계산하기 위해서 사용. 다이인터벌 용
        // 프로빙이 될 때마다 무한히 증가하는 다이 카운트를 읽어서 클리닝이 끝날 때마다 값을 저장해 둔다.
        // 디바이스 변경 후 클리어할 것
        [XmlIgnore, JsonIgnore]
        private long _MarkedDieCountVal = new long();
        public long MarkedDieCountVal
        {
            get { return _MarkedDieCountVal; }
            set
            {
                if (value != _MarkedDieCountVal)
                {
                    _MarkedDieCountVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 웨이퍼 인터벌 용 카운트.
        // 웨이퍼 한 장이 끝날 때마다 무한히 증가하는 웨이퍼 카운트를 읽어서 클리닝이 끝날 때마다 값을 저장해 둔다.
        // 디바이스 변경 후 클리어 할 것
        [XmlIgnore, JsonIgnore]
        private long _MarkedWaferCountVal = new long();
        public long MarkedWaferCountVal
        {
            get { return _MarkedWaferCountVal; }
            set
            {
                if (value != _MarkedWaferCountVal)
                {
                    _MarkedWaferCountVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 몇 장마다 얼라인을 할 지 지정하는 카운트
        private Element<int> _WaferIntervalCount = new Element<int>();
        public Element<int> WaferIntervalCount
        {
            get { return _WaferIntervalCount; }
            set
            {
                if (value != _WaferIntervalCount)
                {
                    _WaferIntervalCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 몇 번째 웨이퍼를 할 것인지 사용자가 지정할 때 사용되는 리스트
        //private int _PinCount;
        private List<Element<bool>> _WaferInterval = new List<Element<bool>>();
        //[XmlAttribute("PinCount")]
        //public int PinCount
        [SharePropPath]
        public List<Element<bool>> WaferInterval
        {
            get { return _WaferInterval; }
            set
            {
                if (value != _WaferInterval)
                {
                    _WaferInterval = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private bool _UpdateAlignResultX;
        private Element<int> _DieInterval = new Element<int>();
        //[XmlAttribute("UpdateAlignResultX")]
        //public bool UpdateAlignResultX
        public Element<int> DieInterval
        {
            get { return _DieInterval; }
            set
            {
                if (value != _DieInterval)
                {
                    _DieInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 단위가 '분' 임. 주의
        private Element<int> _TimeInterval = new Element<int>();
        //[XmlAttribute("UpdateAlignResultX")]
        //public bool UpdateAlignResultX
        public Element<int> TimeInterval
        {
            get { return _TimeInterval; }
            set
            {
                if (value != _TimeInterval)
                {
                    _TimeInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 랏드런 도중에 로드된 웨이퍼에 대해서 얼라인을 했는지 안 했는지 기억하는 플래그.
        // 얼라인하면 True되고, 웨이퍼 언로드 되면 False가 되어야 한다.
        // 전역 변수 개념으로 사용되며, 파일로 저장될 필요 없다. 시리얼라이즈 안 하면 디바이스 로드될때마다 초기화 될까봐 일단 Ignore 옵션 안 넣음.
        [XmlIgnore, JsonIgnore]
        private bool _FlagAlignProcessedForThisWafer = new bool();

        public bool FlagAlignProcessedForThisWafer
        {
            get { return _FlagAlignProcessedForThisWafer; }
            set
            {
                if (value != _FlagAlignProcessedForThisWafer)
                {
                    _FlagAlignProcessedForThisWafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 카드 체인지 후 얼라인을 했는지 안 했는지 기억하는 플래그.
        // 얼라인하면 True되고, 카드 체인지 되면 False가 되어야 한다.
        // 이 플래그가 true이면 핀얼라인이 수행될 때 강제로 Registration 모드로 동작한다. (전체 핀 얼라인)
        // 전역 변수 개념으로 사용되며, 파일로 저장될 필요 없다. 시리얼라이즈 안 하면 디바이스 로드될때마다 초기화 될까봐 일단 Ignore 옵션 안 넣음.
        [XmlIgnore, JsonIgnore]
        private bool _FlagAlignProcessedAfterCardChange = new bool();

        public bool FlagAlignProcessedAfterCardChange
        {
            get { return _FlagAlignProcessedAfterCardChange; }
            set
            {
                if (value != _FlagAlignProcessedAfterCardChange)
                {
                    _FlagAlignProcessedAfterCardChange = value;
                    RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        private bool _FlagAlignProcessedAfterDeviceChange = new bool();

        public bool FlagAlignProcessedAfterDeviceChange
        {
            get { return _FlagAlignProcessedAfterDeviceChange; }
            set
            {
                if (value != _FlagAlignProcessedAfterDeviceChange)
                {
                    _FlagAlignProcessedAfterDeviceChange = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<WAFERINTERVALMODE> _UseWaferInterval = new Element<WAFERINTERVALMODE>();
        //[XmlAttribute("UpdateAlignResultX")]
        //public bool UpdateAlignResultX
        public Element<WAFERINTERVALMODE> UseWaferInterval
        {
            get { return _UseWaferInterval; }
            set
            {
                if (value != _UseWaferInterval)
                {
                    _UseWaferInterval = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<DIEINTERVALMODE> _UseDieInterval = new Element<DIEINTERVALMODE>();
        //[XmlAttribute("UpdateAlignResultX")]
        //public bool UpdateAlignResultX
        public Element<DIEINTERVALMODE> UseDieInterval
        {
            get { return _UseDieInterval; }
            set
            {
                if (value != _UseDieInterval)
                {
                    _UseDieInterval = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<TIMEINTERVALMODE> _UseTimeInterval = new Element<TIMEINTERVALMODE>();
        //[XmlAttribute("UpdateAlignResultX")]
        //public bool UpdateAlignResultX
        public Element<TIMEINTERVALMODE> UseTimeInterval
        {
            get { return _UseTimeInterval; }
            set
            {
                if (value != _UseTimeInterval)
                {
                    _UseTimeInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PinAlignIntervalParameter()
        {
            Element<bool> tmpBool = new Element<bool>();

            _UseWaferInterval.Value = WAFERINTERVALMODE.ALWAYS;
            _UseDieInterval.Value = DIEINTERVALMODE.DISABLE;
            _UseTimeInterval.Value = TIMEINTERVALMODE.DISABLE;

            tmpBool.Value = true;
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);
            _WaferInterval.Add(tmpBool);

            _DieInterval.Value = 0;
            _TimeInterval.Value = 0;

        }
        public PinAlignIntervalParameter(PinAlignIntervalParameter param)
        {
            _UseWaferInterval = param.UseWaferInterval;
            _UseDieInterval = param.UseDieInterval;
            _UseTimeInterval = param.UseTimeInterval;

            _WaferInterval = new List<Element<bool>>(param.WaferInterval);

            _DieInterval = param.DieInterval;
            _TimeInterval = param.TimeInterval;
        }
    }
    [Serializable]
    public class PinTeachParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        //private Element<int> _PositionTolerance = new Element<int>();
        //public Element<int> PositionTolerance
        //{
        //    get { return _PositionTolerance; }
        //    set
        //    {
        //        if (value != _PositionTolerance)
        //        {
        //            _PositionTolerance = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<int> _CardDegreeTolerance = new Element<int>();
        //public Element<int> CardDegreeTolerance
        //{
        //    get { return _CardDegreeTolerance; }
        //    set
        //    {
        //        if (value != _CardDegreeTolerance)
        //        {
        //            _CardDegreeTolerance = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public PinTeachParameter()
        {
            //_PositionTolerance.Value = 30;
            //_CardDegreeTolerance.Value = 20;
        }
        public PinTeachParameter(PinTeachParameter param)
        {
            //_PositionTolerance = param.PositionTolerance;
            //_CardDegreeTolerance = param.CardDegreeTolerance;
        }
    }
    [Serializable]
    public class PinLowAlignParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        private string _PatternPath = @"\PinAlignParameter\PinLowPattern\";
        [DataMember]
        public string PatternPath
        {
            get { return _PatternPath; }
            set
            {
                if (value != _PatternPath)
                {
                    _PatternPath = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private int _MaximumEachPatternCount;
        private Element<int> _MaximumEachPatternCount = new Element<int>();
        [DataMember]
        //public int MaximumEachPatternCount
        public Element<int> MaximumEachPatternCount
        {
            get { return _MaximumEachPatternCount; }
            set
            {
                if (value != _MaximumEachPatternCount)
                {
                    _MaximumEachPatternCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _RetryStepSize = new Element<int>();
        [DataMember]
        //public int MaximumEachPatternCount
        public Element<int> RetryStepSize
        {
            get { return _RetryStepSize; }
            set
            {
                if (value != _RetryStepSize)
                {
                    _RetryStepSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PinLowAlignPatternInfo> _Patterns = new ObservableCollection<PinLowAlignPatternInfo>();
        [DataMember]
        public ObservableCollection<PinLowAlignPatternInfo> Patterns
        {
            get { return _Patterns; }
            set
            {
                if (value != _Patterns)
                {
                    _Patterns = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<PinLowAlignPatternInfo> _SecondPatterns = new ObservableCollection<PinLowAlignPatternInfo>();
        //public ObservableCollection<PinLowAlignPatternInfo> SecondPatterns
        //{
        //    get { return _SecondPatterns; }
        //    set
        //    {
        //        if (value != _SecondPatterns)
        //        {
        //            _SecondPatterns = value;
        //            NotifyPropertyChanged("SecondPatterns");
        //        }
        //    }
        //}     

        //private double _PatternDistanceTolerance;

        private Element<double> _PatternDistanceTolerance = new Element<double>();
        [DataMember]
        //public double PatternDistanceTolerance
        public Element<double> PatternDistanceTolerance
        {
            get { return _PatternDistanceTolerance; }
            set
            {
                if (value != _PatternDistanceTolerance)
                {
                    _PatternDistanceTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private double _EachPatternToleranceX;
        private Element<double> _EachPatternToleranceX = new Element<double>();
        [DataMember]
        //public double EachPatternToleranceX
        public Element<double> EachPatternToleranceX
        {
            get { return _EachPatternToleranceX; }
            set
            {
                if (value != _EachPatternToleranceX)
                {
                    _EachPatternToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _EachPatternToleranceY = new Element<double>();
        [DataMember]
        public Element<double> EachPatternToleranceY
        {
            get { return _EachPatternToleranceY; }
            set
            {
                if (value != _EachPatternToleranceY)
                {
                    _EachPatternToleranceY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _EachPatternToleranceZ = new Element<double>();
        [DataMember]
        public Element<double> EachPatternToleranceZ
        {
            get { return _EachPatternToleranceZ; }
            set
            {
                if (value != _EachPatternToleranceZ)
                {
                    _EachPatternToleranceZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<int> _MinMatchingScore = new Element<int>();
        //[DataMember]
        //public Element<int> MinMatchingScore
        //{
        //    get { return _MinMatchingScore; }
        //    set
        //    {
        //        if (value != _MinMatchingScore)
        //        {
        //            _MinMatchingScore = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private Element<int> _MaxMatchingScore = new Element<int>();
        //[DataMember]
        //public Element<int> MaxMatchingScore
        //{
        //    get { return _MaxMatchingScore; }
        //    set
        //    {
        //        if (value != _MaxMatchingScore)
        //        {
        //            _MaxMatchingScore = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<PinLowAlignTypeEnum> _Result = new Element<PinLowAlignTypeEnum>();
        [DataMember]
        public Element<PinLowAlignTypeEnum> Result
        {
            get { return _Result; }
            set
            {
                if (value != _Result)
                {
                    _Result = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PinLowAlignParameter()
        {
            //_MaximumEachPatternCount.Value = 1;
            //_EachPatternToleranceX.Value = 100;
            //_EachPatternToleranceY.Value = 100;
            //_EachPatternToleranceZ.Value = 100;
            //_PatternDistanceTolerance.Value = 1000;
            //_Result = PINALIGNRESULT.PIN_BLOB_FAILED;
            //_Type = PinLowAlignTypeEnum.MANUAL;

        }
        public PinLowAlignParameter(PinLowAlignParameter param)
        {

        }
    }
    [Serializable]
    public class PinHighAlignParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public List<object> Nodes { get; set; }


        private Element<bool> _PinTipFocusEnable = new Element<bool> { Value = false };
        [DataMember]
        public Element<bool> PinTipFocusEnable
        {
            get { return _PinTipFocusEnable; }
            set
            {
                if (value != _PinTipFocusEnable)
                {
                    _PinTipFocusEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<FOCUSINGRAGE> _PinTipFocusRange = new Element<FOCUSINGRAGE> { Value = FOCUSINGRAGE.RANGE_25 };
        [DataMember]
        public Element<FOCUSINGRAGE> PinTipFocusRange
        {
            get { return _PinTipFocusRange; }
            set
            {
                if (value != _PinTipFocusRange)
                {
                    _PinTipFocusRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PinKeyMinDistance = new Element<int> { Value = 10 };
        [DataMember]
        public Element<int> PinKeyMinDistance
        {
            get { return _PinKeyMinDistance; }
            set
            {
                if (value != _PinKeyMinDistance)
                {
                    _PinKeyMinDistance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PinTipFocusPassPercentage = new Element<int> { Value = 80 };
        [DataMember]
        public Element<int> PinTipFocusPassPercentage
        {
            get { return _PinTipFocusPassPercentage; }
            set
            {
                if (value != _PinTipFocusPassPercentage)
                {
                    _PinTipFocusPassPercentage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PinTipSizeValidationEnable = new Element<bool> { Value = false };
        [DataMember]
        public Element<bool> PinTipSizeValidationEnable
        {
            get { return _PinTipSizeValidationEnable; }
            set
            {
                if (value != _PinTipSizeValidationEnable)
                {
                    _PinTipSizeValidationEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Tip을 Monitoring 할 때 Monitoring 할 대상 옵션
        /// 
        /// 
        /// </summary>
        private Element<SizeValidationOption> _PinTipSizeValidation_Targeting_Option = new Element<SizeValidationOption>();
        [DataMember]
        public Element<SizeValidationOption> PinTipSizeValidation_Targeting_Option
        {
            get { return _PinTipSizeValidation_Targeting_Option; }
            set
            {
                if (value != _PinTipSizeValidation_Targeting_Option)
                {
                    _PinTipSizeValidation_Targeting_Option = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Monitoring할 대상 Pin Number List
        /// </summary>
        private ObservableCollection<ValidationPinItem> _PinTipSizeValidation_Pin_List = new ObservableCollection<ValidationPinItem>();
        [DataMember]
        public ObservableCollection<ValidationPinItem> PinTipSizeValidation_Pin_List
        {
            get
            {
                return _PinTipSizeValidation_Pin_List;
            }
            set
            {
                if (value != _PinTipSizeValidation_Pin_List)
                {
                    _PinTipSizeValidation_Pin_List = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Monitoring에 검출되었을 경우 Lot Pause옵션
        /// True: "Alarm" - Lot 중단, Message Box, 옵션에 따라 Host에 알람
        /// False: "Warning" - Message Box, 옵션에 따라 Host에 알람
        /// </summary>
        private Element<bool> _PinTipSizeValidation_Lot_Pause_Option = new Element<bool> { Value = false };
        [DataMember]
        public Element<bool> PinTipSizeValidation_Lot_Pause_Option
        {
            get { return _PinTipSizeValidation_Lot_Pause_Option; }
            set
            {
                if (value != _PinTipSizeValidation_Lot_Pause_Option)
                {
                    _PinTipSizeValidation_Lot_Pause_Option = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region Tip Monitoring에 대한 이미지 저장 여부 옵션
        /// <summary>
        /// Blob후 사이즈가 적정 범위 내에 들어왔을 때 Blob 이미지 저장 여부
        /// True: 저장
        /// False: 저장하지 않음
        /// </summary>
        private Element<bool> _PinTipSizeValidation_SizeInRange_Image = new Element<bool> { Value = false };
        [DataMember]
        public Element<bool> PinTipSizeValidation_SizeInRange_Image
        {
            get { return _PinTipSizeValidation_SizeInRange_Image; }
            set
            {
                if (value != _PinTipSizeValidation_SizeInRange_Image)
                {
                    _PinTipSizeValidation_SizeInRange_Image = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Blob후 사이즈가 적정 범위 내에 들어오지 않았을 때 Blob 이미지 저장 여부
        /// True: 저장
        /// False: 저장하지 않음
        /// </summary>
        private Element<bool> _PinTipSizeValidation_OutOfSize_Image = new Element<bool> { Value = true };
        [DataMember]
        public Element<bool> PinTipSizeValidation_OutOfSize_Image
        {
            get { return _PinTipSizeValidation_OutOfSize_Image; }
            set
            {
                if (value != _PinTipSizeValidation_OutOfSize_Image)
                {
                    _PinTipSizeValidation_OutOfSize_Image = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 저장된 이미지와 로그를 FTP 서버로 올릴 지 말 지 여부
        /// True: 업로드 O
        /// False: 업로드 X
        /// </summary>
        private Element<bool> _PinTipSizeValidation_UploadToServer = new Element<bool> { Value = false };
        [DataMember]
        public Element<bool> PinTipSizeValidation_UploadToServer
        {
            get { return _PinTipSizeValidation_UploadToServer; }
            set
            {
                if (value != _PinTipSizeValidation_UploadToServer)
                {
                    _PinTipSizeValidation_UploadToServer = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region Tip을 Monitoring 한다고 했을 때 Blob결과와 비교할 범위에 대한 파라미터
        private Element<double> _PinTipSizeValidation_Min_X = new Element<double> { Value = 4 };
        [DataMember]
        public Element<double> PinTipSizeValidation_Min_X
        {
            get { return _PinTipSizeValidation_Min_X; }
            set
            {
                if (value != _PinTipSizeValidation_Min_X)
                {
                    _PinTipSizeValidation_Min_X = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PinTipSizeValidation_Max_X = new Element<double> { Value = 14 };
        [DataMember]
        public Element<double> PinTipSizeValidation_Max_X
        {
            get { return _PinTipSizeValidation_Max_X; }
            set
            {
                if (value != _PinTipSizeValidation_Max_X)
                {
                    _PinTipSizeValidation_Max_X = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PinTipSizeValidation_Min_Y = new Element<double> { Value = 4 };
        [DataMember]
        public Element<double> PinTipSizeValidation_Min_Y
        {
            get { return _PinTipSizeValidation_Min_Y; }
            set
            {
                if (value != _PinTipSizeValidation_Min_Y)
                {
                    _PinTipSizeValidation_Min_Y = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PinTipSizeValidation_Max_Y = new Element<double> { Value = 14 };
        [DataMember]
        public Element<double> PinTipSizeValidation_Max_Y
        {
            get { return _PinTipSizeValidation_Max_Y; }
            set
            {
                if (value != _PinTipSizeValidation_Max_Y)
                {
                    _PinTipSizeValidation_Max_Y = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private PinHighAlignKeyParameter _HighAlignKeyParameter = new PinHighAlignKeyParameter();
        [DataMember]
        public PinHighAlignKeyParameter HighAlignKeyParameter
        {
            get { return _HighAlignKeyParameter; }
            set
            {
                if (value != _HighAlignKeyParameter)
                {
                    _HighAlignKeyParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PinHighAlignParameter()
        {

        }
    }
    [Serializable]
    public class PinHighAlignKeyParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public List<object> Nodes { get; set; }

        private Element<bool> _KeyFocusingExtension = new Element<bool>();
        [DataMember]
        public Element<bool> KeyFocusingExtension
        {
            get { return _KeyFocusingExtension; }
            set
            {
                if (value != _KeyFocusingExtension)
                {
                    _KeyFocusingExtension = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyFocusingExtensionWidth = new Element<double>();
        [DataMember]
        public Element<double> KeyFocusingExtensionWidth
        {
            get { return _KeyFocusingExtensionWidth; }
            set
            {
                if (value != _KeyFocusingExtensionWidth)
                {
                    _KeyFocusingExtensionWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyFocusingExtensionHeight = new Element<double>();
        [DataMember]
        public Element<double> KeyFocusingExtensionHeight
        {
            get { return _KeyFocusingExtensionHeight; }
            set
            {
                if (value != _KeyFocusingExtensionHeight)
                {
                    _KeyFocusingExtensionHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _KeyBlobExtension = new Element<bool>();
        [DataMember]
        public Element<bool> KeyBlobExtension
        {
            get { return _KeyBlobExtension; }
            set
            {
                if (value != _KeyBlobExtension)
                {
                    _KeyBlobExtension = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyBlobExtensionWidth = new Element<double>();
        [DataMember]
        public Element<double> KeyBlobExtensionWidth
        {
            get { return _KeyBlobExtensionWidth; }
            set
            {
                if (value != _KeyBlobExtensionWidth)
                {
                    _KeyBlobExtensionWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyBlobExtensionHeight = new Element<double>();
        [DataMember]
        public Element<double> KeyBlobExtensionHeight
        {
            get { return _KeyBlobExtensionHeight; }
            set
            {
                if (value != _KeyBlobExtensionHeight)
                {
                    _KeyBlobExtensionHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyBlobSizeXMinimumMargin = new Element<double>();
        [DataMember]
        public Element<double> KeyBlobSizeXMinimumMargin
        {
            get { return _KeyBlobSizeXMinimumMargin; }
            set
            {
                if (value != _KeyBlobSizeXMinimumMargin)
                {
                    _KeyBlobSizeXMinimumMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyBlobSizeXMaximumMargin = new Element<double>();
        [DataMember]
        public Element<double> KeyBlobSizeXMaximumMargin
        {
            get { return _KeyBlobSizeXMaximumMargin; }
            set
            {
                if (value != _KeyBlobSizeXMaximumMargin)
                {
                    _KeyBlobSizeXMaximumMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyBlobSizeYMinimumMargin = new Element<double>();
        [DataMember]
        public Element<double> KeyBlobSizeYMinimumMargin
        {
            get { return _KeyBlobSizeYMinimumMargin; }
            set
            {
                if (value != _KeyBlobSizeYMinimumMargin)
                {
                    _KeyBlobSizeYMinimumMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _KeyBlobSizeYMaximumMargin = new Element<double>();
        [DataMember]
        public Element<double> KeyBlobSizeYMaximumMargin
        {
            get { return _KeyBlobSizeYMaximumMargin; }
            set
            {
                if (value != _KeyBlobSizeYMaximumMargin)
                {
                    _KeyBlobSizeYMaximumMargin = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _KeyRectangularity = new Element<double>();
        [DataMember]
        public Element<double> KeyRectangularity
        {
            get { return _KeyRectangularity; }
            set
            {
                if (value != _KeyRectangularity)
                {
                    _KeyRectangularity = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PinHighAlignKeyParameter()
        {
            KeyRectangularity.LowerLimit = 0.1;
            KeyRectangularity.UpperLimit = 1.0;
            KeyRectangularity.Value = 0.8;
        }
    }
    [Serializable]
    public class PinHighAlignBaseFocusingParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public List<object> Nodes { get; set; }

        private Element<BaseFocusingEnable> _BaseFocsuingEnable = new Element<BaseFocusingEnable>();
        [DataMember]
        public Element<BaseFocusingEnable> BaseFocsuingEnable
        {
            get { return _BaseFocsuingEnable; }
            set
            {
                if (value != _BaseFocsuingEnable)
                {
                    _BaseFocsuingEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<BaseFocusingFirstPin> _DoFirstPin = new Element<BaseFocusingFirstPin>();
        [DataMember]
        public Element<BaseFocusingFirstPin> DoFirstPin
        {
            get { return _DoFirstPin; }
            set
            {
                if (value != _DoFirstPin)
                {
                    _DoFirstPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _FocusingTolerance = new Element<double>();
        [DataMember]
        public Element<double> FocusingTolerance
        {
            get { return _FocusingTolerance; }
            set
            {
                if (value != _FocusingTolerance)
                {
                    _FocusingTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFocusParameter _FocusingParam;
        public IFocusParameter FocusingParam
        {
            get { return _FocusingParam; }
            set { _FocusingParam = value; }
        }

        public PinHighAlignBaseFocusingParameter()
        {
            BaseFocsuingEnable.Value = BaseFocusingEnable.DISABLE;
            DoFirstPin.Value = BaseFocusingFirstPin.FirstPin;
            FocusingTolerance.Value = 50;
        }
    }
    [Serializable]
    public class PinPadMatchParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public PinPadMatchParameter()
        {
            _PinPadMatchToleranceX.Value = 25;
            _PinPadMatchToleranceY.Value = 25;
            _PinPadMatchToleranceZ.Value = 25;
            _PinPadMatchToleranceAngle.Value = 1.5;
        }
        public PinPadMatchParameter(PinPadMatchParameter pram)
        {
            _PinPadMatchToleranceX.Value = (int)pram.PinPadMatchToleranceX.GetValue();
            _PinPadMatchToleranceY.Value = (int)pram.PinPadMatchToleranceY.GetValue();
            _PinPadMatchToleranceZ.Value = (int)pram.PinPadMatchToleranceZ.GetValue();
            _PinPadMatchToleranceAngle.Value = (int)pram.PinPadMatchToleranceAngle.GetValue();
        }
        private Element<double> _PinPadMatchToleranceX = new Element<double>();
        public Element<double> PinPadMatchToleranceX
        {
            get
            {
                return _PinPadMatchToleranceX;
            }
            set
            {
                if (value != _PinPadMatchToleranceX)
                {
                    _PinPadMatchToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PinPadMatchToleranceY = new Element<double>();
        public Element<double> PinPadMatchToleranceY
        {
            get
            {
                return _PinPadMatchToleranceY;
            }
            set
            {
                if (value != _PinPadMatchToleranceY)
                {
                    _PinPadMatchToleranceY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PinPadMatchToleranceZ = new Element<double>();
        public Element<double> PinPadMatchToleranceZ
        {
            get
            {
                return _PinPadMatchToleranceZ;
            }
            set
            {
                if (value != _PinPadMatchToleranceZ)
                {
                    _PinPadMatchToleranceZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PinPadMatchToleranceAngle = new Element<double>();
        public Element<double> PinPadMatchToleranceAngle
        {
            get
            {
                return _PinPadMatchToleranceAngle;
            }
            set
            {
                if (value != _PinPadMatchToleranceAngle)
                {
                    _PinPadMatchToleranceAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
    [Serializable]
    public class PinPlaneAdjustParameter : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public PinPlaneAdjustParameter()
        {
            EnablePinPlaneCompensation.Value = true;
            MaxCompHeight.Value = 50;
            CompRatio.Value = 0.95;
        }
        public PinPlaneAdjustParameter(PinPlaneAdjustParameter param)
        {
            EnablePinPlaneCompensation.Value = param.EnablePinPlaneCompensation.Value;
            MaxCompHeight.Value = param.MaxCompHeight.Value;
            CompRatio.Value = param.CompRatio.Value;
        }
        private Element<bool> _EnablePinPlaneCompensation = new Element<bool>() { GEMImmediatelyUpdate = true };
        public Element<bool> EnablePinPlaneCompensation
        {
            get
            {
                return _EnablePinPlaneCompensation;
            }
            set
            {
                if (value != _EnablePinPlaneCompensation)
                {
                    _EnablePinPlaneCompensation = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _MaxCompHeight = new Element<double>();
        public Element<double> MaxCompHeight
        {
            get
            {
                return _MaxCompHeight;
            }
            set
            {
                if (value != _MaxCompHeight)
                {
                    _MaxCompHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _CompRatio = new Element<double>();
        public Element<double> CompRatio
        {
            get
            {
                return _CompRatio;
            }
            set
            {
                if (value != _CompRatio)
                {
                    _CompRatio = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class ValidationPinItem : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string Genealogy { get; set; }

        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        private Element<int> _PinNum = new Element<int>();
        [DataMember]
        public Element<int> PinNum
        {
            get { return _PinNum; }
            set
            {
                if (value != _PinNum)
                {
                    _PinNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsSelected = new Element<bool>();
        [DataMember]
        public Element<bool> IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ValidationPinItem(int pinNo, bool isSelected)
        {
            this.PinNum.Value = pinNo;
            this.IsSelected.Value = isSelected;
        }

        public ValidationPinItem()
        {

        }
    }
}
