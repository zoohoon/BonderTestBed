using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using ProberErrorCode;

using LogModule;
using Newtonsoft.Json;

namespace ProberInterfaces.Vision
{
    public class EmulImageParameter : IDeviceParameterizable
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
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        private ObservableCollection<ActionModule> _ActionModules = new ObservableCollection<ActionModule>();
        public ObservableCollection<ActionModule> ActionModules
        {
            get { return _ActionModules; }
            set { _ActionModules = value; }
        }

        public string FilePath { get; } = "Vision";

        public string FileName { get; } = "EmulImageParam.json";


        public EmulImageParameter()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }

    public class ActionModule
    {
        private string _ModuleName;
        [XmlAttribute("ModuleName")]
        public string ModuleName
        {
            get { return _ModuleName; }
            set { _ModuleName = value; }
        }

        private ObservableCollection<ContinousGrabImage> _ContinousGrabImages = new ObservableCollection<ContinousGrabImage>();

        public ObservableCollection<ContinousGrabImage> ContinousGrabImages
        {
            get { return _ContinousGrabImages; }
            set { _ContinousGrabImages = value; }
        }

        private SingleGrabImages _SingleGrabImage = new SingleGrabImages();

        public SingleGrabImages SingleGrabImage
        {
            get { return _SingleGrabImage; }
            set { _SingleGrabImage = value; }
        }

        private ObservableCollection<FocusingGrabImage> _FocusingGrabImages = new ObservableCollection<FocusingGrabImage>();

        public ObservableCollection<FocusingGrabImage> FocusingGrabImages
        {
            get { return _FocusingGrabImages; }
            set { _FocusingGrabImages = value; }
        }

        public ActionModule()
        {
        }

        public ActionModule(string modulename)
        {
            ModuleName = modulename;
        }
    }

    public class ContinousGrabImage
    {
        private ObservableCollection<GrabInfo> _GrabInfos = new ObservableCollection<GrabInfo>();

        public ObservableCollection<GrabInfo> GrabInfos
        {
            get { return _GrabInfos; }
            set { _GrabInfos = value; }
        }

        private bool _GrabFlag;

        [XmlIgnore, JsonIgnore]
        public bool GrabFlag
        {
            get { return _GrabFlag; }
            set { _GrabFlag = value; }
        }

        public ContinousGrabImage()
        {

        }
        public ContinousGrabImage(ObservableCollection<GrabInfo> grabinfos)
        {
            GrabInfos = grabinfos;
        }
        public ContinousGrabImage(GrabInfo grabinfo)
        {
            GrabInfos.Add(grabinfo);
        }
    }


    public class SingleGrabImages
    {
        private ObservableCollection<GrabInfo> _GrabInfos = new ObservableCollection<GrabInfo>();

        public ObservableCollection<GrabInfo> GrabInfos
        {
            get { return _GrabInfos; }
            set { _GrabInfos = value; }
        }

        public SingleGrabImages()
        {
        }
    }

    public class FocusingGrabImage
    {
        private ObservableCollection<GrabInfo> _GrabInfos = new ObservableCollection<GrabInfo>();

        public ObservableCollection<GrabInfo> GrabInfos
        {
            get { return _GrabInfos; }
            set { _GrabInfos = value; }
        }

        private bool _GrabFlag;

        [XmlIgnore, JsonIgnore]
        public bool GrabFlag
        {
            get { return _GrabFlag; }
            set { _GrabFlag = value; }
        }

        public FocusingGrabImage()
        {
        }
        public FocusingGrabImage(GrabInfo grabinfo)
        {
            GrabInfos.Add(grabinfo);
        }
        public FocusingGrabImage(ObservableCollection<GrabInfo> grabinfos)
        {
            GrabInfos = grabinfos;
        }
    }

    public class GrabInfo
    {
        private string _Path;

        public string Path
        {
            get { return _Path; }
            set { _Path = value; }
        }

        private bool _GrabFlag;

        [XmlIgnore, JsonIgnore]
        public bool GrabFlag
        {
            get { return _GrabFlag; }
            set { _GrabFlag = value; }
        }

        public GrabInfo()
        {
        }

        public GrabInfo(string path)
        {
            Path = path;
        }

        public GrabInfo(string path, bool grabflag)
        {
            try
            {
                Path = path;
                GrabFlag = grabflag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
