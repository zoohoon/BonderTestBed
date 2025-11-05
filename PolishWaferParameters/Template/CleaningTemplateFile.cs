using System;
using System.Collections.Generic;

namespace PolishWaferModule
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Template;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class CleaningTemplateFile : INotifyPropertyChanged, ITemplateFileParam, IDeviceParameterizable
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

        #region //..IDeviceParameterizable
        [field: NonSerialized, JsonIgnore]
        public string FilePath { get; } = "PolishWaferParam\\";
        [field: NonSerialized, JsonIgnore]
        public string FileName { get; } = "PolishWaferTemplateFile.Json";
        [field: NonSerialized, JsonIgnore]
        public bool IsParamChanged { get; set; }
        [field: NonSerialized, JsonIgnore]
        public string Genealogy { get; set; }
        [field: NonSerialized, JsonIgnore]
        public object Owner { get; set; }
        [field: NonSerialized, JsonIgnore]
        public List<object> Nodes { get; set; }

        public void SetElementMetaData()
        {
            return;
        }
        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
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
                Param.BasePath = @"PolishWaferParam\Template\";
                Param.TemplateInfos.Add(new TemplateInfo("Standard", Param.BasePath + "StandardTemplate.json"));
                Param.SeletedTemplate = Param.TemplateInfos[0];
                RetVal = SetDefaultTemplates();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }



        #endregion

        #region //.. ITemplateFileParam
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
                    RaisePropertyChanged();
                }
            }
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
        #endregion


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
                           @"PolishWaferSubRutine_StandardModule.dll",
                           @"PolishWaferSubRutineStandard",
                             1000,
                             true));
                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                         @"CleaningScheduler.dll",
                         @"CleaningScheduleModule",
                         1000,
                         true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                       new ModuleDllInfo(
                        @"RequestPinAlignBeforePolishWaferCleaning.dll",
                        @"RequestPinAlignBeforePWCleaning",
                        1000,
                        true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                         @"PolishWaferProcesserModule.dll",
                         @"PolishWaferProcessor_Standard",
                         1000,
                         true));
                    file.AddTemplateModules(moduleinfo);


                    moduleinfo = new ModuleInfo(
                       new ModuleDllInfo(
                        @"RequestPinAlignAfterPolishWaferCleaning.dll",
                        @"RequestPinAlignAfterPWCleaning",
                        1000,
                        true));
                    file.AddTemplateModules(moduleinfo);

                    retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
