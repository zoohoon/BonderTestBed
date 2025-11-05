

namespace WaferAlignParam
{
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberInterfaces.Template;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Collections.ObjectModel;
    using ProberInterfaces.State;
    using LogModule;
    using Newtonsoft.Json;

    [Serializable]
    public class WaferAlignTemplateFile : INotifyPropertyChanged, ITemplateFileParam, IDeviceParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [ParamIgnore]
        public List<object> Nodes { get; set; }
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



        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //SetDefaultTemplates();
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
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "WaferAlignParam\\";
        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "WaferAlignTemplateFile.Json";


        [field: NonSerialized, JsonIgnore]
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

        public WaferAlignTemplateFile()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                Param.BasePath = @"WaferAlignParam\Template\";
                //Param.BasePath = @"C:\ProberSystem\Default\Parameters\DeviceParam\DEFAULTDEVNAME\WaferAlignParam\Template\";
                Param.TemplateInfos.Add(new TemplateInfo("Standard", Param.BasePath + "StandardTemplate.json"));
                //Param.TemplateInfos.Add(new TemplateInfo("Development", Param.BasePath + "DevelopmentTemplate.json"));
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
        public string GetFilePath()
        {
            return FilePath;
        }

        public string GetFileName()
        {

            return FileName;
        }

        public EventCodeEnum SetDefaultTemplates()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this is IDeviceParameterizable)
                {
                    retVal = CreateStandardDllList(
                        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[0].Path.Value));
                }
                else if (this is ISystemParameterizable)
                {
                    retVal = CreateStandardDllList(
                    this.FileManager().GetSystemParamFullPath(Param.TemplateInfos[0].Path.Value, null));
                }

            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }


        private EventCodeEnum CreateStandardDllList(string path)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                //if (!File.Exists(path))
                //{
                    TemplateFileParam file = new TemplateFileParam();
                    ModuleInfo moduleinfo;
                    CategoryInfo categoryinfo;

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                 @"SchedulerStandardModule.dll",
                 @"WaferAlignScheduler",
                 1000,
                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"WAEdgeStadnardModule.dll",
                                     @"EdgeStandard",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);

                    categoryinfo = new CategoryInfo("Theta Alignment");
                    categoryinfo.Categories = new ObservableCollection<CategoryModuleBase>();
                    categoryinfo.Categories.Add(new ModuleInfo(
                        new ModuleDllInfo(
                                 @"WALowStandardModule.dll",
                                 @"LowStandard",
                                 1000,
                                 true)));

                    categoryinfo.Categories.Add(new ModuleInfo(
                        new ModuleDllInfo(
                                 @"WAHighStandardModule.dll",
                                 @"HighStandard",
                                 1000,
                                 true)));
                    file.AddTemplateModules(categoryinfo);


                    moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                         @"WABoundaryStandardModule.dll",
                         @"BoundaryStandard",
                         1000,
                         true));

                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"WAMapStandardModule.dll",
                                 @"MapStandard",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"WAUserCoordStandardModule.dll",
                                 @"UserCoordsStandard",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                @"WAIndexAlignEdgeModule.dll",
                                @"IndexAlignEdge",
                                1000,
                                true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"WAPadStandardModule.dll",
                                 @"PadStandard",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    //=======Control==========
                    ControlTemplateParameter cparam = new ControlTemplateParameter();

                    #region ..Pad
                    cparam.ViewControlModuleInfo = new UCControlInfo(
                        new ModuleDllInfo(
                                @"PadSettingView_Standard.dll",
                                1000,
                                true),
                        new Guid("56AF1E26-1C9F-1FDD-B38B-EA0C7324311F"));

                    cparam.ViewModelControlModuleInfo = new UCControlInfo(
                        new ModuleDllInfo(
                                @"PadSettingViewModel_Standard.dll",
                                1000,
                                true),
                        new Guid("8DC261D5-B1C5-C803-CBB6-C47B0F0650D6"));

                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "PadSetup";
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("21092926-C512-A80F-BFA6-EF25137B51A8");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    List<Guid> steps = new List<Guid>();
                    steps.Add(new Guid("DE441C02-5B97-B9C4-1AD9-26DEEC0B5595"));

                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                    file.ControlTemplates.Add(cparam);
                #endregion



                #region ..Setup

                cparam = new ControlTemplateParameter();
                    cparam.ViewControlModuleInfo = new UCControlInfo(
                        new ModuleDllInfo(
                                @"WASettingView_Standard.dll",
                                1000,
                                true),
                        new Guid("DE32A33D-AC5D-9A21-CFDA-02A032611C11"));
                    cparam.ViewModelControlModuleInfo = new UCControlInfo(
                        new ModuleDllInfo(
                                @"WASettingViewModel_Standard.dll",
                                1000,
                                true),
                        new Guid("F9D0CFDA-0611-822C-6D77-1B5FF69B815A"));

                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "WaferAlignSetup";
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("a05a34bf-e63f-41ee-9819-285274faef1a");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    steps = new List<Guid>();
                    steps.Add(new Guid("5B98472E-6F6D-CDA3-20E1-53EF541856F4")); //Edge
                    steps.Add(new Guid("830A6DE1-956A-54AB-407F-3D3222DFBA41")); //Low
                    steps.Add(new Guid("91AF5353-D69F-CAE4-DA09-7AE6BCF9B789")); //High
                    steps.Add(new Guid("95ADDCB8-5670-582E-7190-AAABEF881152")); //Height Profiling(Development)
                    steps.Add(new Guid("FD761F1E-F14C-FEF6-4EE3-FE3E6ADC5458")); //Boundary
                    steps.Add(new Guid("4203F878-B532-8CCC-2613-D5745D4ED5AE")); //Map
                    steps.Add(new Guid("8DC02412-4BA1-E1F4-E62E-0091081C67A8")); //Coord
                    steps.Add(new Guid("CF1E43F7-AFEA-C748-B274-0307A4AE40E6")); //IndexAlignEdge

                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                    //Recovery
                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "Recovery Setup";
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("D0A33FFE-DD22-4572-5B69-73F66C38CEB4");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    steps = new List<Guid>();
                    steps.Add(new Guid("5B98472E-6F6D-CDA3-20E1-53EF541856F4")); //Edge
                    steps.Add(new Guid("830A6DE1-956A-54AB-407F-3D3222DFBA41")); //Low
                    steps.Add(new Guid("91AF5353-D69F-CAE4-DA09-7AE6BCF9B789")); //High
                    steps.Add(new Guid("FD761F1E-F14C-FEF6-4EE3-FE3E6ADC5458")); //Boundary
                    steps.Add(new Guid("DE441C02-5B97-B9C4-1AD9-26DEEC0B5595")); //Pad 

                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                    //MAP=====================
                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "Wafer Map Setup";
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("0FA740C6-FEC8-1C7F-BAB0-A1725716BD62");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    steps = new List<Guid>();
                    steps.Add(new Guid("4203F878-B532-8CCC-2613-D5745D4ED5AE"));
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                    //========================

                    //USER COORDINATE ========
                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "User Coord Setup";
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("9B0D3A32-C44C-D98F-210E-20E65A1A3900");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    steps = new List<Guid>();
                    steps.Add(new Guid("8DC02412-4BA1-E1F4-E62E-0091081C67A8")); //Coord
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;
                    file.ControlTemplates.Add(cparam);
                //========================

                //Index Align ========
                cparam.ControlPNPButtons.Add(new ControlPNPButton());
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "Index Align Setup";
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                    new Guid("A8A532A5-A961-4391-8B8C-E91D193B18AB");
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                    new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                steps = new List<Guid>();
                steps.Add(new Guid("CF1E43F7-AFEA-C748-B274-0307A4AE40E6"));
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;
                file.ControlTemplates.Add(cparam);
                //========================


                //file.ControlTemplates.Add(cparam);
                #endregion

                #region recovery step 

                //List<string> processingmodules = new List<string>() { "EdgeStandard", "LowStandard", "HighStandard" };
                RecoveryTemplateParameter recoverytemplateparam = new RecoveryTemplateParameter();
                recoverytemplateparam.ErrorModuleName = "EdgeStandard";
                recoverytemplateparam.RecoveryStepGUID.Add(new Guid("5B98472E-6F6D-CDA3-20E1-53EF541856F4")); //Edge
                file.AddRecoveryTempplate(recoverytemplateparam);

                recoverytemplateparam = new RecoveryTemplateParameter();
                recoverytemplateparam.ErrorModuleName = "LowStandard";
                recoverytemplateparam.RecoveryStepGUID.Add(new Guid("830A6DE1-956A-54AB-407F-3D3222DFBA41")); //Low
                recoverytemplateparam.RecoveryStepGUID.Add(new Guid("FD761F1E-F14C-FEF6-4EE3-FE3E6ADC5458")); //Boundary
                file.AddRecoveryTempplate(recoverytemplateparam);

                recoverytemplateparam = new RecoveryTemplateParameter();
                recoverytemplateparam.ErrorModuleName = "HighStandard";
                recoverytemplateparam.RecoveryStepGUID.Add(new Guid("91AF5353-D69F-CAE4-DA09-7AE6BCF9B789")); //High
                recoverytemplateparam.RecoveryStepGUID.Add(new Guid("FD761F1E-F14C-FEF6-4EE3-FE3E6ADC5458")); //Boundary
                recoverytemplateparam.RecoveryStepGUID.Add(new Guid("DE441C02-5B97-B9C4-1AD9-26DEEC0B5595")); //Pad 
                file.AddRecoveryTempplate(recoverytemplateparam);

                recoverytemplateparam = new RecoveryTemplateParameter();
                recoverytemplateparam.ErrorModuleName = "IndexAlignEdge";
                recoverytemplateparam.RecoveryStepGUID.Add(new Guid("CF1E43F7-AFEA-C748-B274-0307A4AE40E6")); //IndexAlign
                file.AddRecoveryTempplate(recoverytemplateparam);
                #endregion
                retVal = Extensions_IParam.SaveParameter(null, file, null, path);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
