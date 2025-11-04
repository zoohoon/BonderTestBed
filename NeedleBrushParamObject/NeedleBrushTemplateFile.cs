using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace NeedleBrushParamObject
{
    public class NeedleBrushTemplateFile : ITemplateFileParam, IDeviceParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "NeedleBrushModule";
        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "NeedleBrushTemplateFile.json";

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

        private ITemplateParameter _Param = new TemplateParameter();

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

        public NeedleBrushTemplateFile()
        {

        }

        public void SetElementMetaData()
        {

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
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = SetDefaultParam();
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

                Param.BasePath = @"NeedleBrushModule\Template\";

                Param.TemplateInfos.Add(new TemplateInfo("Standard", Param.BasePath + "StandardTemplate.json"));

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
                if (!File.Exists(path))
                {
                    TemplateFileParam file = new TemplateFileParam();
                    ModuleInfo moduleinfo;
                    //CategoryInfo categoryinfo;

                    moduleinfo = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"NeedleBrushProcesser.dll",
                                     @"NormalNeedleBrushProcessModule",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"NeedleBrushScheduler.dll",
                                     @"NeedleBrushScheduleModule",
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
                throw;
            }
            return retVal;
        }
    }
}
