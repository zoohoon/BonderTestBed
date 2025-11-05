using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.State;
using ProberInterfaces.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace PinAlignParam
{
    public class SinglePinAlignTemplateFile : INotifyPropertyChanged, ITemplateFileParam, IDeviceParameterizable
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
        public string FilePath { get; } = "PinAlignParam";
        public string FileName { get; } = "SinglePinAlignTemplateFile.json";
        public bool IsParamChanged { get; set; }
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
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //string path = @"C:\ProberSystem\Default\Parameters\DeviceParam\DEFAULTDEVNAME\PinAlignParam\Template\";
                Param.BasePath = @"PinAlignParam\SinglePinAlignTemplate\";
                Param.TemplateInfos.Add(new TemplateInfo("SinglePinAlignStandard", Param.BasePath + "SinglePinAlignStandardTemplate.json"));
                Param.TemplateInfos.Add(new TemplateInfo("SinglePinAlignMEMSType", Param.BasePath + "SinglePinAlignMEMSTemplate.json"));
                Param.TemplateInfos.Add(new TemplateInfo("SinglePinAlignVertical", Param.BasePath + "SinglePinAlignVerticalTemplate.json"));
                Param.SeletedTemplate = Param.TemplateInfos[0];
                SetDefaultTemplates();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }

        public EventCodeEnum SetDefaultTemplates()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = CreateStandardDllList(
                        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[0].Path.Value));

                retVal = CreateMEMSDllList(
                        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[1].Path.Value));

                retVal = CreateVerticalDllList(
                        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[2].Path.Value));
                //if (this is IDeviceParameterizable)
                //{
                //    retVal = CreateStandardDllList(
                //        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[0].Path.Value));

                //    retVal = CreateMEMSDllList(
                //        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[1].Path.Value));
                //}
                //else if (this is ISystemParameterizable)
                //{
                //    retVal = CreateStandardDllList(
                //    this.FileManager().GetSystemParamFullPath(Param.TemplateInfos[0].Path.Value, null));

                //    retVal = CreateMEMSDllList(
                //        this.FileManager().GetSystemParamFullPath(Param.TemplateInfos[1].Path.Value, null));
                //}
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

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"TipFocusRough.dll",
                                 @"SinglePinAlignTipFocusRoughModule",
                                 1000,
                                 true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                      new ModuleDllInfo(
                               @"TipBlob.dll",
                               @"SinglePinAlignTipBlobModule",
                               1000,
                               true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                      new ModuleDllInfo(
                               @"TipFocusing.dll",
                               @"SinglePinAlignTipFocusingModule",
                               1000,
                               true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

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
        private EventCodeEnum CreateMEMSDllList(string path)
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

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"AlignKeyFocusing.dll",
                                 @"SinglePinAlignKeyFocusingModule",
                                 1000,
                                 true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                      new ModuleDllInfo(
                               @"AlignKeyBlob.dll",
                               @"SinglePinAlignKeyBlobModule",
                               1000,
                               true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

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

        private EventCodeEnum CreateVerticalDllList(string path)
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

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"BaseFocusing.dll",
                                 @"SignlePinAlignBaseFocusingModule",
                                 1000,
                                 true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                        new ModuleDllInfo(
                                 @"TipFocusRough.dll",
                                 @"SinglePinAlignTipFocusRoughModule",
                                 1000,
                                 true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                      new ModuleDllInfo(
                               @"TipBlob.dll",
                               @"SinglePinAlignTipBlobModule",
                               1000,
                               true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                      new ModuleDllInfo(
                               @"TipFocusing.dll",
                               @"SinglePinAlignTipFocusingModule",
                               1000,
                               true), EnumEnableState.ENABLE);
                    file.AddTemplateModules(moduleinfo);

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

        public void SetElementMetaData()
        {
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }




    }
}
