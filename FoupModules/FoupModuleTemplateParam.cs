using System;
using System.Collections.Generic;

namespace FoupModules
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Template;
    using System.ComponentModel;
    using System.IO;
    using System.Xml.Serialization;

    [Serializable()]
    public class FoupModuleTemplateParam : INotifyPropertyChanged, ITemplateFileParam, ISystemParameterizable
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
        public string FilePath { get; } = "Foup\\";
        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "FoupModuleTemplate.json";

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

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                Param.BasePath = @"Foup\Template\";
                // 250911 LJH 수정
                //Param.TemplateInfos.Add(new TemplateInfo("GPMultiFoup", Param.BasePath + "GPFoupTemplate.json"));
                Param.TemplateInfos.Add(new TemplateInfo("Foup", Param.BasePath + "FoupTemplate.json"));
                
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
                //if (!File.Exists(path))
                //{
                    TemplateFileParam file = new TemplateFileParam();
                    ModuleInfo moduleinfo;
                    //file.SubRoutineModule = new ModuleInfo(
                    //    new ModuleDllInfo(
                    //       @"FoupModules.dll",
                    //       @"GPFoupCover12Inch",
                    //         1000,
                    //         true));
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                             @"FoupModules.dll",
                             @"FLATFoupCover",
                             1000,
                             true));
                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                            @"FoupModules.dll",
                            @"FLATFoupDockingPlate",
                            1000,
                            true));
                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                            @"FoupModules.dll",
                            @"FLATFoupDockingPort",
                            1000,
                            true));
                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                            @"FoupModules.dll",
                            @"FLATFoupDockingPort40",
                            1000,
                            true));
                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                            @"FoupModules.dll",
                            @"FLATFoupCover",
                            1000,
                            true));
                    file.AddTemplateModules(moduleinfo);
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                            @"FoupModules.dll",
                            @"FLATFoupTilt",
                            1000,
                            true));
                    file.AddTemplateModules(moduleinfo);
                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                            @"FoupModules.dll",
                            @"FLATFoupOpener",
                            1000,
                            true));
                    file.AddTemplateModules(moduleinfo);
                moduleinfo = new ModuleInfo(
                    new ModuleDllInfo(
                            @"FoupModules.dll",
                            @"DockingPortDoorNomal",
                            1000,
                            true));
                file.AddTemplateModules(moduleinfo);
                retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                //}
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
