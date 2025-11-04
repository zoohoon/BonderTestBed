using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeedleCleanerModuleParameter.Template
{
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberInterfaces.Template;
    using System.IO;
    using ProberInterfaces.PnpSetup;
    using System.Collections.ObjectModel;
    using ProberInterfaces.State;
    using ProberInterfaces.AlignEX;
    using Newtonsoft.Json;
    using LogModule;

    [Serializable()]
    public class NeedleCleanTemplateFile : INotifyPropertyChanged, ITemplateFileParam, ISystemParameterizable
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
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


        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "NeedleCleanModule\\";
        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "NeedleCleanTemplateFile.json";

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

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                Param.BasePath = @"NeedleCleanModule\Template\";
                Param.TemplateInfos.Add(new TemplateInfo("Standard", Param.BasePath + "StandardTemplate.json"));
                Param.SeletedTemplate = Param.TemplateInfos[0];
                RetVal = SetDefaultTemplates();
            }
            catch (Exception err)
            {
                throw err;
            }

            return RetVal;
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public void SetElementMetaData()
        {

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
                    this.FileManager().GetSystemParamFullPath(Param.TemplateInfos[0].Path.Value, ""));
                }
                //retVal = CreateStandardDllList(Param.TemplateInfos[0].Path.Value);
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
                if (!File.Exists(path))
                {
                    TemplateFileParam file = new TemplateFileParam();
                    ModuleInfo moduleinfo;

                    file.SubRoutineModule = new ModuleInfo(
                        new ModuleDllInfo(
                           @"NeeldleCleanerSubRutineStandardModule.dll",
                           @"NeeldleCleanerSubRutineStandard",
                             1000,
                             true));
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                             @"NeedleCleanerScheduler.dll",
                             @"NeedleCleanScheduler",
                             1000,
                             true));
                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"NeedleCleanPadModule.dll",
                                     @"CleanPadSetup",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"CleanPadSequenceSetupModule.dll",
                                 @"CleanPadSequenceSetup",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                @"TouchSensorRegisterModule.dll",
                                @"TouchSensorSetup",
                                1000,
                                true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"TouchSensorBaseRegisterModule.dll",
                                 @"TouchSensorBaseSetup",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"TouchSensorPadRefRegisterModule.dll",
                                 @"TouchSensorPadRefSetup",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"TouchSensorOffsetRegisterModule.dll",
                                 @"TouchSensorOffsetSetup",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"RequestPinAlignBeforeCleaning.dll",
                                 @"RequestPinAlignBeforeNC",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"NeedleCleanFocus.dll",
                                 @"CleanPadFocusBySensor",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"NeedleCleanerProcesser.dll",
                                 @"NeedleCleanProcessor",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"RequestPinAlignAfterCleaning.dll",
                                 @"RequestPinAlignAfterNC",
                                 1000,
                                 true));
                    file.AddTemplateModules(moduleinfo);




                    //=======Control==========
                    ControlTemplateParameter cparam = new ControlTemplateParameter();
                    List<Guid> steps = new List<Guid>();

                    //Clean Pad Setup
                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("8d2cd7a9-caf5-4bb0-89f5-3b6642d26fb6");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    steps = new List<Guid>();
                    steps.Add(new Guid("8F2CD01F-2548-C143-95DD-1A2713570B3B")); //CleanPadSetup
                                                                                 //steps.Add(new Guid("52AB4222-912E-43B4-E9DC-B553F7ECCC35")); //CleanPadSequenceSetup

                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                    //Clean Pad Sequence Setup            
                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("2d37a087-fad5-4c3a-ba72-30759c929a57");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    steps = new List<Guid>();
                    //steps.Add(new Guid("8F2CD01F-2548-C143-95DD-1A2713570B3B")); //CleanPadSetup
                    steps.Add(new Guid("52AB4222-912E-43B4-E9DC-B553F7ECCC35")); //CleanPadSequenceSetup

                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;

                    //Touch
                    steps = new List<Guid>();

                    cparam.ControlPNPButtons.Add(new ControlPNPButton());
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].CuiBtnGUID =
                        new Guid("26168D6E-408A-8D81-28BF-4A77F4A3599E");
                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].ViewGuid =
                        new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");

                    steps = new List<Guid>();
                    steps.Add(new Guid("36972B01-CC9A-6E37-10F4-73228D50521C")); //TouchSensorSetup
                    steps.Add(new Guid("72AC2B4C-2CF9-6D37-D8E7-F91C8D87CAF7")); //TouchSensorBaseSetup
                    steps.Add(new Guid("D1205560-3025-DD6C-C29C-A5C158F8BF80")); //TouchSensorPadRefSetup
                    steps.Add(new Guid("2AA2C7F4-8171-1A9D-0EBD-C8ED49A3E6A0")); //TouchSensorOffsetSetup

                    cparam.ControlPNPButtons[cparam.ControlPNPButtons.Count - 1].StepGUID = steps;



                    file.ControlTemplates.Add(cparam);
                    //========================

                    retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                }

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
