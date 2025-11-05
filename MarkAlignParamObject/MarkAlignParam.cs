using Focusing;

using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.Vision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberInterfaces.Template;
using System.IO;
using LogModule;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace MarkAlignParamObject
{
    [Serializable]
    [XmlInclude(typeof(MarkAlignParam))]
    public class MarkAlignParam : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [ParamIgnore]
        public List<object> Nodes { get; set; }


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
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.PZ, FocusParam, 80);
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
        public void SetElementMetaData()
        {

        }
        [ParamIgnore]
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

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public string FilePath { get; } = "Mark";

        public string FileName { get; } = "MarkAlignerParameter.Json";
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {            // Temporary 20180117
                         //MarkFocusParam.FocusingModel = new NomalFocusing();//==> FocusParam 클래스 생성자에서 초기화 해주어야 함

                //FocusParameter focusParam = new FocusParameter();

                //focusParam.FocusMaxStep = 15;
                //focusParam.FocusRange = 200;
                //focusParam.DepthOfField = 1;
                //focusParam.FocusThreshold = 10000;
                //focusParam.FlatnessThreshold = 90;
                //focusParam.PeakRangeThreshold = 30;
                //focusParam.PotentialThreshold = 20;
                //focusParam.CheckPotential = true;
                //focusParam.CheckThresholdFocusValue = true;
                //focusParam.CheckFlatness = true;
                //focusParam.CheckDualPeak = true;
                //focusParam.FocusingCam = EnumProberCam.WAFER_HIGH_CAM;
                //focusParam.FocusingROI = new Rect(0, 0, 480, 480);
                //focusParam.FocusingAxis = EnumAxisConstants.Z;
                ////focusParam.FocusingModel = new NomalFocusing();

                //MarkFocusParam = new MarkFocusParameter(focusParam);

                MarkPatMatParam.CamType.Value = EnumProberCam.WAFER_HIGH_CAM;
                MarkPatMatParam.PMParameter = new PMParameter();
                MarkPatMatParam.PMParameter.ModelFilePath.Value = @"C:\ProberSystem\Parameters\SystemParam\Vision\PMImage\Mark";

                if(FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }


                MarkPatMatParam.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();
                var lightParam = new LightValueParam();
                lightParam.Value.Value = 255;
                lightParam.Type.Value = EnumLightType.AUX;
                MarkPatMatParam.LightParams.Add(lightParam);
                //MarkPatMatParam.PMParameter.mPMAcceptance
                //MarkPatMatParam.PMParameter.PMCertainty
                //MarkPatMatParam.PMParameter.PMOccurrence

                //MarkPatMatParam.PMParameter.PatternSize
                //MarkPatMatParam.PMParameter.PattWidth
                //MarkPatMatParam.PMParameter.PattHeight
                //MarkPatMatParam.PMParameter.PMModelPath

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return RetVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // Temporary 20180117
                //MarkFocusParam.FocusingModel = new NomalFocusing();//==> FocusParam 클래스 생성자에서 초기화 해주어야 함

                //FocusParameter focusParam = new FocusParameter();

                //focusParam.FocusMaxStep = 15;
                //focusParam.FocusRange = 200;
                //focusParam.DepthOfField = 1;
                //focusParam.FocusThreshold = 10000;
                //focusParam.FlatnessThreshold = 90;
                //focusParam.PeakRangeThreshold = 30;
                //focusParam.PotentialThreshold = 20;
                //focusParam.CheckPotential = true;
                //focusParam.CheckThresholdFocusValue = true;
                //focusParam.CheckFlatness = true;
                //focusParam.CheckDualPeak = true;
                //focusParam.FocusingCam = EnumProberCam.WAFER_HIGH_CAM;
                //focusParam.FocusingROI = new Rect(0, 0, 480, 480);
                //focusParam.FocusingAxis = EnumAxisConstants.PZ;
                ////focusParam.FocusingModel = new NomalFocusing();

                //MarkFocusParam = new MarkFocusParameter(focusParam);

                MarkPatMatParam.CamType.Value = EnumProberCam.WAFER_HIGH_CAM;
                MarkPatMatParam.PMParameter = new PMParameter();
                MarkPatMatParam.PMParameter.ModelFilePath.Value = @"C:\ProberSystem\Parameters\SystemParam\Vision\PMImage\Mark";
                MarkPatMatParam.PMParameter.PatternFileExtension.Value = ".bmp";
                MarkPatMatParam.Enable = true;

                MarkPatMatParam.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();
                var lightParam = new LightValueParam();
                lightParam.Value.Value = 128;
                lightParam.Type.Value = EnumLightType.AUX;
                MarkPatMatParam.LightParams.Add(lightParam);
                //MarkPatMatParam.PMParameter.mPMAcceptance
                //MarkPatMatParam.PMParameter.PMCertainty
                //MarkPatMatParam.PMParameter.PMOccurrence

                //MarkPatMatParam.PMParameter.PatternSize
                //MarkPatMatParam.PMParameter.PattWidth
                //MarkPatMatParam.PMParameter.PattHeight
                //MarkPatMatParam.PMParameter.PMModelPath

                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return RetVal;
        }

        public MarkAlignParam()
        {

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

        private FocusParameter _RetryFocusParam;
        public FocusParameter RetryFocusParam
        {
            get { return _RetryFocusParam; }
            set
            {
                if (value != _RetryFocusParam)
                {
                    _RetryFocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MAStandardPatternInfomation _MarkPatMatParam = new MAStandardPatternInfomation();
        public MAStandardPatternInfomation MarkPatMatParam
        {
            get { return _MarkPatMatParam; }
            set
            {
                if (value != _MarkPatMatParam)
                {
                    _MarkPatMatParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _MarkCompensationEnable = true;
        public bool MarkCompensationEnable
        {
            get { return _MarkCompensationEnable; }
            set
            {
                if (value != _MarkCompensationEnable)
                {
                    _MarkCompensationEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _MarkDiffTolerance_X = new Element<double>() {Value = 100};
        public Element<double> MarkDiffTolerance_X
        {
            get { return _MarkDiffTolerance_X; }
            set
            {
                if (value != _MarkDiffTolerance_X)
                {
                    _MarkDiffTolerance_X = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _MarkDiffTolerance_Y = new Element<double>() { Value = 100};
        public Element<double> MarkDiffTolerance_Y
        {
            get { return _MarkDiffTolerance_Y; }
            set
            {
                if (value != _MarkDiffTolerance_Y)
                {
                    _MarkDiffTolerance_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkDiffToleranceOfWA_X = new Element<double>() { Value = 5 };
        public Element<double> MarkDiffToleranceOfWA_X
        {
            get { return _MarkDiffToleranceOfWA_X; }
            set
            {
                if (value != _MarkDiffToleranceOfWA_X)
                {
                    _MarkDiffToleranceOfWA_X = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _MarkDiffToleranceOfWA_Y = new Element<double>() { Value = 5 };
        public Element<double> MarkDiffToleranceOfWA_Y
        {
            get { return _MarkDiffToleranceOfWA_Y; }
            set
            {
                if (value != _MarkDiffToleranceOfWA_Y)
                {
                    _MarkDiffToleranceOfWA_Y = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<bool> _MarkVerificationAfterWaferAlign = new Element<bool>() { Value = true };
        public Element<bool> MarkVerificationAfterWaferAlign
        {
            get { return _MarkVerificationAfterWaferAlign; }
            set
            {
                if (value != _MarkVerificationAfterWaferAlign)
                {
                    _MarkVerificationAfterWaferAlign = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _DelaywaferCamCylinderExtendedBeforeFocusing;
        /// <summary>
        /// unit : sec
        /// </summary>
        public int DelaywaferCamCylinderExtendedBeforeFocusing
        {
            get { return _DelaywaferCamCylinderExtendedBeforeFocusing; }
            set
            {
                if (value != _DelaywaferCamCylinderExtendedBeforeFocusing)
                {
                    _DelaywaferCamCylinderExtendedBeforeFocusing = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    [Serializable]
    public abstract class MAPatternInfomation : PatternInfomation, INotifyPropertyChanged
    {
        public MAPatternInfomation()
        {

        }
    }

    [Serializable]
    public class MAStandardPatternInfomation : MAPatternInfomation, INotifyPropertyChanged
    {
        public MAStandardPatternInfomation()
        {

        }
    }

    [Serializable]
    public class MarkAlignTemplateFile : INotifyPropertyChanged, ITemplateFileParam, IDeviceParameterizable
    {
        private ITemplateParameter _Param
            = new TemplateParameter();

        public ITemplateParameter Param
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    NotifyPropertyChanged("Param");
                }
            }
        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [field: NonSerialized, JsonIgnore]
        public string FilePath { get; } = "MarkAlign\\";

        public string FileName { get; } = "MarkAlignTemplateFile.Json";

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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public EventCodeEnum Init()
        {

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferAlignTemplateFile] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Param.BasePath = @"C:\ProberSystem\Default\Parameters\DeviceParam\DEFAULTDEVNAME\MarkAlign\Template\";
                Param.BasePath = @"MarkAlign\Template\";

                Param.TemplateInfos.Clear();

                Param.TemplateInfos.Add(new TemplateInfo("MarkOPUSV", Param.BasePath + "MarkOpusVTemplate.json"));
                Param.SeletedTemplate = Param.TemplateInfos[0];
                
                RetVal = SetDefaultTemplates();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
            Param.BasePath = @"MarkAlign\Template\";
            Param.TemplateInfos.Add(new TemplateInfo("MarkOPUSV", Param.BasePath + "MarkOpusVTemplate.json"));
            Param.SeletedTemplate = Param.TemplateInfos[0];
            RetVal = SetDefaultTemplates();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum SetDefaultTemplates()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this is IDeviceParameterizable)
                {
                    retVal = CreateMarkOpusVDllList(
                        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[0].Path.Value));
                }
                else if (this is ISystemParameterizable)
                {
                    retVal = CreateMarkOpusVDllList(
                    this.FileManager().GetSystemParamFullPath(Param.TemplateInfos[0].Path.Value, null));
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        private EventCodeEnum CreateMarkOpusVDllList(string path)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                if (!File.Exists(path))
                {

                    TemplateFileParam file = new TemplateFileParam();
                    ModuleInfo moduleinfo;
                    //CategoryInfo categoryinfo;
                    moduleinfo = new ModuleInfo(
                   new ModuleDllInfo(
                            @"MarkAlignerScheduler.dll",
                            @"MarkAlignScheduler",
                            1000,
                            true));
                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"MarkAlignOpusVProcess.dll",
                                     @"MKOpusVProcess",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);

                    SetDefaultControlTemplate(file);

                     retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void SetDefaultControlTemplate(TemplateFileParam file)
        {
            try
            {
                //=======Control==========
                ControlTemplateParameter cparam = new ControlTemplateParameter();
                cparam.ControlPNPButtons.Add(new ControlPNPButton());
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "Mark Setup";
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                   new Guid("66ae4fca-caf5-42b9-a4ba-bd22d026e65a");
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                    new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                List<Guid> steps = new List<Guid>();
                steps.Add(new Guid("D81C5A65-044E-22BD-897F-75AC68BA4EFD"));
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;
                file.ControlTemplates.Add(cparam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
      
    }

    [Serializable]
    public class TestFile : INotifyPropertyChanged, IDeviceParameterizable

    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [field: NonSerialized, JsonIgnore]
        public string FilePath { get; } = "Test\\";

        public string FileName { get; } = "TestFile.Json";

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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<String> _Name = new Element<string>();
        public Element<String> Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _Age = new Element<int>();
        public Element<int> Age
        {
            get { return _Age; }
            set
            {
                if (value != _Age)
                {
                    _Age = value;
                    RaisePropertyChanged();
                }
            }
        }
   
        private CatCoordinates _Pos2;
        [XmlIgnore, JsonIgnore]
        public CatCoordinates Pos2
        {
            get { return _Pos2; }
            set
            {
                if (value != _Pos2)
                {
                    _Pos2 = value;
                    RaisePropertyChanged();
                }
            }
        }


        private CatCoordinates _Pos1;
        public CatCoordinates Pos1
        {
            get { return _Pos1; }
            set
            {
                if (value != _Pos1)
                {
                    _Pos1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _Pos;
        public CatCoordinates Pos
        {
            get { return _Pos; }
            set
            {
                if (value != _Pos)
                {
                    _Pos = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferAlignTemplateFile] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
            _Age.Value = 10;
            Name.Value = "abc";
            _Pos = new CatCoordinates();
            _Pos.X.Value = 10;
            _Pos.Y.Value = 20;
            _Pos.Z.Value = 40;

            _Pos1 = new CatCoordinates();
            _Pos1.X.Value = 100;
            _Pos1.Y.Value = 200;
            _Pos1.Z.Value = 400;


            _Pos2 = new CatCoordinates();
            _Pos2.X.Value = 1300;
            _Pos2.Y.Value = 2300;
            _Pos2.Z.Value = 4300;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }
        public void SetElementMetaData()
        {

        }
    }
}
