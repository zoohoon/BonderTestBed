using System;
using System.Collections.Generic;
using System.Linq;

namespace PinAlignParam
{
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Template;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using ProberInterfaces.State;
    using LogModule;
    using Newtonsoft.Json;
    using System.Xml.Serialization;

    [Serializable]
    public class PinAlignTemplateFile : INotifyPropertyChanged, ITemplateFileParam, IDeviceParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
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
                //PROBECARD_TYPE
                // Template 변경시, 자동으로 최신 데이터를 유지하기 위해 호출 함.
                
                foreach (PROBECARD_TYPE source in (PROBECARD_TYPE[])System.Enum.GetValues(typeof(PROBECARD_TYPE))) 
                {
                    if (PinAlignTemplateDictionary.ContainsKey(source) == false)
                    {
                        switch (source)
                        {
                            case PROBECARD_TYPE.Cantilever_Standard:
                                PinAlignTemplateDictionary.Add(PROBECARD_TYPE.Cantilever_Standard, "Standard");
                                break;

                            case PROBECARD_TYPE.MEMS_Dual_AlignKey:
                                PinAlignTemplateDictionary.Add(PROBECARD_TYPE.MEMS_Dual_AlignKey, "MEMSType");
                                break;
                            case PROBECARD_TYPE.VerticalType:
                                PinAlignTemplateDictionary.Add(PROBECARD_TYPE.VerticalType, "Vertical");
                                break;
                        }
                    }
                }

                SetDefaultTemplates();

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

        public string FilePath { get; } = "PinAlignParam";

        public string FileName { get; } = "PinAlignTemplateFile.json";

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
        [field: NonSerialized, JsonIgnore]
        private Dictionary<PROBECARD_TYPE, string> PinAlignTemplateDictionary
            = new Dictionary<PROBECARD_TYPE, string>();

        public PinAlignTemplateFile()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //string path = @"C:\ProberSystem\Default\Parameters\DeviceParam\DEFAULTDEVNAME\PinAlignParam\Template\";
                Param.BasePath = @"PinAlignParam\Template\";
                Param.TemplateInfos.Add(new TemplateInfo("Standard", Param.BasePath + "StandardTemplate.json"));
                Param.TemplateInfos.Add(new TemplateInfo("MEMSType", Param.BasePath + "MEMSTemplate.json"));
                Param.TemplateInfos.Add(new TemplateInfo("Vertical", Param.BasePath + "VerticalTemplate.json"));
                
                Param.SeletedTemplate = Param.TemplateInfos[1];
                SetDefaultTemplates();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
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
                    if (Param.TemplateInfos.Count > 0)
                    {
                        string val = null;
                        
                        foreach (PROBECARD_TYPE source in (PROBECARD_TYPE[])System.Enum.GetValues(typeof(PROBECARD_TYPE)))
                        {
                            val = null;
                            
                            bool isExist = PinAlignTemplateDictionary.TryGetValue(source, out val);
                            if (isExist == true)
                            {
                                var retval = Param.TemplateInfos.FirstOrDefault(s => s.Name.Value == val);
                                val = val.Replace("Type", "");
                                if (retval == null)
                                {
                                    Param.TemplateInfos.Add(new TemplateInfo(val, Param.BasePath + val + "Template.json"));
                                }
                            }
                            else 
                            {
                                //??
                            }
                        }
                        retVal = CreateStandardDllList(this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[0].Path.Value));
                        retVal = CreateMEMSDllList(this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[1].Path.Value));
                        retVal = CreateVericalDllList(this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[2].Path.Value));
                    }
                }
                else if (this is ISystemParameterizable)
                {
                    if (Param.TemplateInfos.Count > 0)
                    {
                        retVal = CreateStandardDllList(this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[0].Path.Value, null));

                        if (Param.TemplateInfos.Count >= 2)
                        {
                            retVal = CreateMEMSDllList(this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[1].Path.Value, null));
                            retVal = CreateVericalDllList(this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[2].Path.Value, null));
                        }

                    }
                }
                //CreateStandardDllList(Param.TemplateInfos[0].Path.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"TeachPinModule.dll",
                             @"TeachPinModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobSearchAreaSetupModule.dll",
                             @"BlobSearchAreaSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobThresholdSetupModule.dll",
                             @"BlobThresholdSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobMinSizeSetupModule.dll",
                             @"BlobMinSizeSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobMaxSizeSetupModule.dll",
                             @"BlobMaxSizeSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"PinHighAlignModule.dll",
                             @"PinHighAlignModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                //moduleinfo = new ModuleInfo(
                //    new ModuleDllInfo(
                //             @"PinPadMatchModule.dll",
                //             @"PinPadMatchModule",
                //             1000,
                //             true), EnumEnableState.ENABLE);
                //file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"PinAlignScheduler.dll",
                             @"PinAlignScheduler",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);



                ControlTemplateParameter cparam = new ControlTemplateParameter();

                cparam.ControlPNPButtons.Add(new ControlPNPButton());
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                    new Guid("29cc6805-9418-4fb5-813c-fa1101820b3c");
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                    new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                List<Guid> steps = new List<Guid>();
                steps.Add(new Guid("6B6A825B-A349-464F-37A5-B89B33660F23")); //Pin Group Setting
                steps.Add(new Guid("EA95E665-70D6-BDE9-C512-6F81A4621CC8")); //Teach Pin
                steps.Add(new Guid("36CA2A9B-BB3E-49AE-DEA5-CCD2DD7FEEC3")); //Blob Search Area Setup
                steps.Add(new Guid("EAFB86C4-D46C-D156-CF7F-7AB335BDF82E")); //Blob Threshold Setup
                steps.Add(new Guid("4DA84CB6-311F-469D-8AEC-B2899A854DF9")); //Blob Minimum Size Setup
                steps.Add(new Guid("757A1338-B56F-8962-F40C-BB09316ACD1C")); //Blob Maxinum Size Setup
                steps.Add(new Guid("CF4DA650-9016-278A-3E19-4ACEE77A01B2")); //Pin High Alignment


                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                file.ControlTemplates.Add(cparam);


                retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private EventCodeEnum CreateMEMSDllList(string path)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                // TODO : 
                //if (!File.Exists(path))
                //{
                TemplateFileParam file = new TemplateFileParam();
                ModuleInfo moduleinfo;
                CategoryInfo categoryinfo;

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"TeachPinModule.dll",
                             @"TeachPinModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                #region ==> High Align Key Setup
                categoryinfo = new CategoryInfo("High Align Key Setup");
                categoryinfo.Categories = new ObservableCollection<CategoryModuleBase>();

                categoryinfo.Categories.Add(new ModuleInfo(
                    new ModuleDllInfo(
                             @"HighAlignKeyAngleSetupModule.dll",
                             @"HighAlignKeyAngleSetupModule",
                             1000,
                             true)));

                categoryinfo.Categories.Add(new ModuleInfo(
                    new ModuleDllInfo(
                             @"HighAlignKeyPositionSetupModule.dll",
                             @"HighAlignKeyPositionSetupModule",
                             1000,
                             true)));

                categoryinfo.Categories.Add(new ModuleInfo(
                    new ModuleDllInfo(
                             @"HighAlignkeyBlobMinMaxSizeSetupModule.dll",
                             @"HighAlignkeyBlobMinMaxSizeSetupModule",
                             1000,
                             true)));

                categoryinfo.Categories.Add(new ModuleInfo(
                    new ModuleDllInfo(
                             @"HighAlignKeyBrightnessSetupModule.dll",
                             @"HighAlignKeyBrightnessSetupModule",
                             1000,
                             true)));

                categoryinfo.Categories.Add(new ModuleInfo(
                    new ModuleDllInfo(
                             @"HighAlignKeyAdvancedSetupModule.dll",
                             @"HighAlignKeyAdvancedSetupModule",
                             1000,
                             true)));

                file.AddTemplateModules(categoryinfo);
                #endregion

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                              @"ImageLowAlignModule.dll",
                              @"ImageLowAlignStandardModule",
                              1000,
                              true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"PinHighAlignModule.dll",
                             @"PinHighAlignModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                //moduleinfo = new ModuleInfo(
                //    new ModuleDllInfo(
                //             @"PinPadMatchModule.dll",
                //             @"PinPadMatchModule",
                //             1000,
                //             true), EnumEnableState.ENABLE);
                //file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"PinAlignScheduler.dll",
                             @"PinAlignScheduler",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                ControlTemplateParameter cparam = new ControlTemplateParameter();

                cparam.ControlPNPButtons.Add(new ControlPNPButton());
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                    new Guid("29cc6805-9418-4fb5-813c-fa1101820b3c");
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                    new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                List<Guid> steps = new List<Guid>();
                steps.Add(new Guid("EA95E665-70D6-BDE9-C512-6F81A4621CC8")); //Teach Pin
                steps.Add(new Guid("B8B468A5-EE7F-4B15-B46F-A5C6652B2A6B")); //HighAlignKeySetup
                steps.Add(new Guid("8BB3E2C6-F680-E713-8D89-BA1D768F66B8")); //HighAlignKeyPositionSetup
                steps.Add(new Guid("AA2D6613-33F2-46DB-9EC4-2FD1D483B207")); //HighAlignKeyBrightnessSetup
                steps.Add(new Guid("F7B25A63-15B0-2D41-7A2F-51300EA9532F")); //HighAlignkeyBlobMinMaxSizeSetupModule
                steps.Add(new Guid("728D75F2-B66F-4FC1-90E2-DD174FF9452B")); //HighAlignKeyAdvancedSetupModule
                steps.Add(new Guid("CF4DA650-9016-278A-3E19-4ACEE77A01B2")); //Pin High Alignment
                steps.Add(new Guid("BB5EEA0F-AF78-B573-5908-3864AB536C25")); //LowAlign Setup

                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                file.ControlTemplates.Add(cparam);

                retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private EventCodeEnum CreateVericalDllList(string path)
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

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"TeachPinModule.dll",
                             @"TeachPinModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BaseFocusingModule.dll",
                             @"BaseFocusingModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobSearchAreaSetupModule.dll",
                             @"BlobSearchAreaSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobThresholdSetupModule.dll",
                             @"BlobThresholdSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobMinSizeSetupModule.dll",
                             @"BlobMinSizeSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"BlobMaxSizeSetupModule.dll",
                             @"BlobMaxSizeSetupModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"PinHighAlignModule.dll",
                             @"PinHighAlignModule",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);

                //moduleinfo = new ModuleInfo(
                //    new ModuleDllInfo(
                //             @"PinPadMatchModule.dll",
                //             @"PinPadMatchModule",
                //             1000,
                //             true), EnumEnableState.ENABLE);
                //file.AddTemplateModules(moduleinfo);

                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                             @"PinAlignScheduler.dll",
                             @"PinAlignScheduler",
                             1000,
                             true), EnumEnableState.ENABLE);
                file.AddTemplateModules(moduleinfo);



                ControlTemplateParameter cparam = new ControlTemplateParameter();

                cparam.ControlPNPButtons.Add(new ControlPNPButton());
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "Pin Align Setup";
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                    new Guid("29cc6805-9418-4fb5-813c-fa1101820b3c");
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                    new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                List<Guid> steps = new List<Guid>();
                steps.Add(new Guid("6B6A825B-A349-464F-37A5-B89B33660F23")); //Pin Group Setting
                steps.Add(new Guid("EA95E665-70D6-BDE9-C512-6F81A4621CC8")); //Teach Pin
                steps.Add(new Guid("8E781BDD-ACBD-48D1-80D4-31322749150B")); //Base Focusing
                steps.Add(new Guid("36CA2A9B-BB3E-49AE-DEA5-CCD2DD7FEEC3")); //Blob Search Area Setup
                steps.Add(new Guid("EAFB86C4-D46C-D156-CF7F-7AB335BDF82E")); //Blob Threshold Setup
                steps.Add(new Guid("4DA84CB6-311F-469D-8AEC-B2899A854DF9")); //Blob Minimum Size Setup
                steps.Add(new Guid("757A1338-B56F-8962-F40C-BB09316ACD1C")); //Blob Maxinum Size Setup
                steps.Add(new Guid("CF4DA650-9016-278A-3E19-4ACEE77A01B2")); //Pin High Alignment


                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                cparam.ControlPNPButtons.Add(new ControlPNPButton());
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].Caption = "Update Teach Pin";
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                    new Guid("29D16B55-50AF-43BC-847C-C1DEB1F22008");
                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                    new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                steps = new List<Guid>();
                steps.Add(new Guid("6B6A825B-A349-464F-37A5-B89B33660F23")); //Pin Group Setting
                steps.Add(new Guid("EA95E665-70D6-BDE9-C512-6F81A4621CC8")); //Teach Pin
                steps.Add(new Guid("CF4DA650-9016-278A-3E19-4ACEE77A01B2")); //Pin High Alignment


                cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;
                file.ControlTemplates.Add(cparam);

                retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
