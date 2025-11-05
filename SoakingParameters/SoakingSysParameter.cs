using Focusing;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SoakingParameters
{
    [Serializable]
    public class SoakingSysParameter: IParam, INotifyPropertyChanged,ISystemParameterizable,ICloneable,IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
        public string FilePath { get; } = "";

        public string FileName { get; } = "SoakingSystemFile.Json";


        private Element<double> _NoWaferOnChuckClearance = new Element<double>();
        public Element<double> NoWaferOnChuckClearance
        {
            get { return _NoWaferOnChuckClearance; }
            set
            {
                if (value != _NoWaferOnChuckClearance)
                {
                    _NoWaferOnChuckClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _NoWaferAlignedClearance = new Element<double>();
        public Element<double> NoWaferAlignedClearance
        {
            get { return _NoWaferAlignedClearance; }
            set
            {
                if (value != _NoWaferAlignedClearance)
                {
                    _NoWaferAlignedClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Status Soaking Use Polish Wafer (False 인 경우 UI에서 PW 설정 화면을 보여주지 않는다.)
        /// </summary>
        private Element<bool> _UsePolishWafer = new Element<bool> { Value = false };
        public Element<bool> UsePolishWafer
        {
            get { return _UsePolishWafer; }
            set
            {
                if (value != _UsePolishWafer)
                {
                    _UsePolishWafer = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// 해당 파라미터가 True이면 Device Change후 Preheating Soak을 트리거 시킨다. 해당 파라미터는 SoakingSysParam에 있는 Use Polish wafer파라미터가 True일 때만 UI에 보이게 된다.
        /// </summary>
        private Element<bool> _PreheatSoak_after_DeviceChange = new Element<bool>() { Value = false };
        public Element<bool> PreheatSoak_after_DeviceChange
        {
            get { return _PreheatSoak_after_DeviceChange; }
            set
            {
                if (value != _PreheatSoak_after_DeviceChange)
                {
                    _PreheatSoak_after_DeviceChange = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Card없을 시 Soaking위치로 이동 하느냐 마느냐
        /// POGO Type일 때 True권장
        /// ZIF Type일 때 False권장
        /// true : 저장된 Soaking위치로 이동
        /// false : ZCleared로 이동
        /// </summary>
        private Element<bool> _SoakWithoutCard = new Element<bool> { Value = true };
        public Element<bool> SoakWithoutCard
        {
            get { return _SoakWithoutCard; }
            set
            {
                if (value != _SoakWithoutCard)
                {
                    _SoakWithoutCard = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _WithoutCardAndNotSoaking_ChuckPosX = new Element<int> { Value = 0 };
        public Element<int> WithoutCardAndNotSoaking_ChuckPosX
        {
            get { return _WithoutCardAndNotSoaking_ChuckPosX; }
            set
            {
                if (value != _WithoutCardAndNotSoaking_ChuckPosX)
                {
                    _WithoutCardAndNotSoaking_ChuckPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _WithoutCardAndNotSoaking_ChuckPosY = new Element<int> { Value = 0 };
        public Element<int> WithoutCardAndNotSoaking_ChuckPosY
        {
            get { return _WithoutCardAndNotSoaking_ChuckPosY; }
            set
            {
                if (value != _WithoutCardAndNotSoaking_ChuckPosY)
                {
                    _WithoutCardAndNotSoaking_ChuckPosY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _ChuckAwayToleranceLimitX = new Element<int> { Value = 200 };
        public Element<int> ChuckAwayToleranceLimitX
        {
            get { return _ChuckAwayToleranceLimitX; }
            set
            {
                if (value != _ChuckAwayToleranceLimitX)
                {
                    _ChuckAwayToleranceLimitX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ChuckAwayToleranceLimitY = new Element<int> { Value = 200 };
        public Element<int> ChuckAwayToleranceLimitY
        {
            get { return _ChuckAwayToleranceLimitY; }
            set
            {
                if (value != _ChuckAwayToleranceLimitY)
                {
                    _ChuckAwayToleranceLimitY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ChuckAwayToleranceLimitZ = new Element<int> { Value = 200 };
        public Element<int> ChuckAwayToleranceLimitZ
        {
            get { return _ChuckAwayToleranceLimitZ; }
            set
            {
                if (value != _ChuckAwayToleranceLimitZ)
                {
                    _ChuckAwayToleranceLimitZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LightValueParam> _LightParams;
        [SharePropPath]
        public ObservableCollection<LightValueParam> LightParams
        {
            get { return _LightParams; }
            set
            {
                if (value != _LightParams)
                {
                    _LightParams = value;
                    RaisePropertyChanged();
                }
            }
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
        private Element<int> _ChuckRefHight = new Element<int>();
        public Element<int> ChuckRefHight
        {
            get { return _ChuckRefHight; }
            set
            {
                if (value != _ChuckRefHight)
                {
                    _ChuckRefHight = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// todo ann 
        /// </summary>
        private Element<bool> _ChuckFocusingSkip = new Element<bool>();
        public Element<bool> ChuckFocusingSkip
        {
            get { return _ChuckFocusingSkip; }
            set
            {
                if (value != _ChuckFocusingSkip)
                {
                    _ChuckFocusingSkip = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// todo ann 
        /// </summary>
        private Element<double> _ChuckFocusingClearanceThd = new Element<double>() { Value = -350 };
        public Element<double> ChuckFocusingClearanceThd
        {
            get { return _ChuckFocusingClearanceThd; }
            set
            {
                if (value != _ChuckFocusingClearanceThd)
                {
                    _ChuckFocusingClearanceThd = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _ChuckFocusingFlatnessThd = new Element<double>() { Value = 70 };
        public Element<double> ChuckFocusingFlatnessThd
        {
            get { return _ChuckFocusingFlatnessThd; }
            set
            {
                if (value != _ChuckFocusingFlatnessThd)
                {
                    _ChuckFocusingFlatnessThd = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _ProcessingFocusingFlatnessThd = new Element<double>() { Value = 70 };
        public Element<double> ProcessingFocusingFlatnessThd
        {
            get { return _ProcessingFocusingFlatnessThd; }
            set
            {
                if (value != _ProcessingFocusingFlatnessThd)
                {
                    _ProcessingFocusingFlatnessThd = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PolishFocusingFlatnessThd = new Element<double>() { Value = 95 };
        public Element<double> PolishFocusingFlatnessThd
        {
            get { return _PolishFocusingFlatnessThd; }
            set
            {
                if (value != _PolishFocusingFlatnessThd)
                {
                    _PolishFocusingFlatnessThd = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<SoakingTestParameter> _SoakingTestList = new ObservableCollection<SoakingTestParameter>();
        public ObservableCollection<SoakingTestParameter> SoakingTestList
        {
            get { return _SoakingTestList; }
            set
            {
                if (value != _SoakingTestList)
                {
                    _SoakingTestList = value;
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
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                NoWaferAlignedClearance.Value = -500;
                NoWaferOnChuckClearance.Value = -700;

                ChuckFocusingFlatnessThd.Value = 70;

                ChuckFocusingSkip.Value = false;
                ChuckFocusingClearanceThd.Value = -350;

                ProcessingFocusingFlatnessThd.Value = 70;
                PolishFocusingFlatnessThd.Value = 95;

                SoakingTestParameter cycle1 = new SoakingTestParameter();
                cycle1.SoakingTestOverDrive = -80;
                cycle1.SoakingTestTime = 900;
                SoakingTestList.Add(cycle1);
                SoakingTestParameter cycle2 = new SoakingTestParameter();
                cycle2.SoakingTestOverDrive = -80;
                cycle2.SoakingTestTime = 900;
                SoakingTestList.Add(cycle2);
                SoakingTestParameter cycle3 = new SoakingTestParameter();
                cycle3.SoakingTestOverDrive = -80;
                cycle3.SoakingTestTime = 900;
                SoakingTestList.Add(cycle3);
                SoakingTestParameter cycle4 = new SoakingTestParameter();
                cycle4.SoakingTestOverDrive = -80;
                cycle4.SoakingTestTime = 900;
                SoakingTestList.Add(cycle4);

                SoakingTestParameter cycle5 = new SoakingTestParameter();
                cycle5.SoakingTestOverDrive = 80;
                cycle5.SoakingTestTime = 600;
                SoakingTestList.Add(cycle5);
                SoakingTestParameter cycle6 = new SoakingTestParameter();
                cycle6.SoakingTestOverDrive = 80;
                cycle6.SoakingTestTime = 600;
                SoakingTestList.Add(cycle6);
                SoakingTestParameter cycle7 = new SoakingTestParameter();
                cycle7.SoakingTestOverDrive = 80;
                cycle7.SoakingTestTime = 600;
                SoakingTestList.Add(cycle7);
                SoakingTestParameter cycle8 = new SoakingTestParameter();
                cycle8.SoakingTestOverDrive = 80;
                cycle8.SoakingTestTime = 600;
                SoakingTestList.Add(cycle8);
                SoakingTestParameter cycle9 = new SoakingTestParameter();
                cycle9.SoakingTestOverDrive = 80;
                cycle9.SoakingTestTime = 600;
                SoakingTestList.Add(cycle9);
                SoakingTestParameter cycle10 = new SoakingTestParameter();
                cycle10.SoakingTestOverDrive = 80;
                cycle10.SoakingTestTime = 600;
                SoakingTestList.Add(cycle10);

                SoakingTestParameter cycle11 = new SoakingTestParameter();
                cycle11.SoakingTestOverDrive = 80;
                cycle11.SoakingTestTime = 1200;
                SoakingTestList.Add(cycle11);
                SoakingTestParameter cycle12 = new SoakingTestParameter();
                cycle12.SoakingTestOverDrive = 80;
                cycle12.SoakingTestTime = 1200;
                SoakingTestList.Add(cycle12);

                SoakingTestParameter cycle13 = new SoakingTestParameter();
                cycle13.SoakingTestOverDrive = 80;
                cycle13.SoakingTestTime = 3600;
                SoakingTestList.Add(cycle13);

                SoakingTestParameter cycle14 = new SoakingTestParameter();
                cycle14.SoakingTestOverDrive = -500;
                cycle14.SoakingTestTime = 600;
                SoakingTestList.Add(cycle14);
                SoakingTestParameter cycle15 = new SoakingTestParameter();
                cycle15.SoakingTestOverDrive = -500;
                cycle15.SoakingTestTime = 600;
                SoakingTestList.Add(cycle15);
                SoakingTestParameter cycle16 = new SoakingTestParameter();
                cycle16.SoakingTestOverDrive = -500;
                cycle16.SoakingTestTime = 600;
                SoakingTestList.Add(cycle16);

                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                    FocusParam.FlatnessThreshold.Value = 99.0;
                }
                ChuckRefHight.Value = 0;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
      
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                retval = this.FocusManager().ValidationFocusParam(FocusParam);

                if (retval != EventCodeEnum.NONE)
                {
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, FocusParam, 200);
                    FocusParam.FlatnessThreshold.Value = 70;
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public object Clone()
        {
            SoakingSysParameter sysfile = new SoakingSysParameter();

            try
            {
                //devfile.AutoSoaking = this.AutoSoaking;

                //devfile.EventSoakingList = this.EventSoakingList;

                //devfile.EveryWaferSoaking = this.EveryWaferSoaking;
                //devfile.FirstWaferSoaking = this.FirstWaferSoaking;
                //devfile.LotResumeSoaking = this.LotResumeSoaking;
                //devfile.PolishWaferSoaking = this.PolishWaferSoaking;
                //devfile.PreHeatSoaking1 = this.PreHeatSoaking1;
                //devfile.PreHeatSoaking2 = this.PreHeatSoaking2;

                //devfile.WhiteList = this.WhiteList;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return sysfile;
        }
    }

    [Serializable]
    public class SoakingTestParameter: INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
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

        private double _SoakingTestOverDrive;
        public double SoakingTestOverDrive
        {
            get { return _SoakingTestOverDrive; }
            set
            {
                if (value != _SoakingTestOverDrive)
                {
                    _SoakingTestOverDrive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SoakingTestTime;
        public int SoakingTestTime
        {
            get { return _SoakingTestTime; }
            set
            {
                if (value != _SoakingTestTime)
                {
                    _SoakingTestTime = value;
                    RaisePropertyChanged();
                }
            }
        }


    }
}
