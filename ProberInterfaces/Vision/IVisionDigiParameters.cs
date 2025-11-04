using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces.Vision
{
    using Newtonsoft.Json;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;

    public interface IVisionDigiParameters : IParamNode
    {
        ObservableCollection<DigitizerGroup> DigitizerGroups { get; set; }
        ObservableCollection<DigitizerParameter> ParamList { get; set; }
    }

    [Serializable]
    public class DigitizerGroup : IParamNode
    {
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


        private Element<ObservableCollection<int>> _DigiGroup
            = new Element<ObservableCollection<int>>();

        public Element<ObservableCollection<int>> DigiGroup
        {
            get { return _DigiGroup; }
            set { _DigiGroup = value; }
        }

        public DigitizerGroup()
        {

        }
        public void DefaultSetting()
        {
            try
            {
                DigiGroup.Value.Add(0);

                //DigiGroup.Add(1);
                //DigiGroup.Add(2);
                //DigiGroup.Add(3);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable]
    public class DigitizerParameter : IParamNode
    {

        public DigitizerParameter()
        {
            try
            {
                _DigitizerName.Value = "-1";
                _GrabRaft.Value = EnumGrabberRaft.MILMORPHIS;
                //_ChannelCount.Value = -1;
                //_ColorDept.Value = 0;
                _DCF.Value = ".dcf";
                _DigiGroup.Value = 0;
                _GrabTimeOut.Value = 2000;
                _EmulGrabMode.Value = EnumGrabberMode.AUTO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public DigitizerParameter(string digiName, EnumGrabberRaft grabType, int channelCount,
            int colordept, string dcf)
        {
            try
            {
                _DigitizerName.Value = digiName;
                _GrabRaft.Value = grabType;
                //_ChannelCount.Value = channelCount;
                //_ColorDept.Value = colordept;
                _DCF.Value = dcf;
                _GrabTimeOut.Value = 2000;
                _EmulGrabMode.Value = EnumGrabberMode.AUTO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public DigitizerParameter(string digiName, EnumGrabberRaft grabType, int channelCount,
    int colordept, string dcf, int digigroup)
        {
            try
            {
                _DigitizerName.Value = digiName;
                _GrabRaft.Value = grabType;
                //_ChannelCount.Value = channelCount;
                //_ColorDept.Value = colordept;
                _DCF.Value = dcf;
                _DigiGroup.Value = digigroup;
                _EmulGrabMode.Value = EnumGrabberMode.AUTO;
                _GrabTimeOut.Value = 2000;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
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

        private Element<string> _DigitizerName
             = new Element<string>();
        // [XmlAttribute("CameraDigitizerName")]
        public Element<string> DigitizerName
        {
            get { return _DigitizerName; }
            set
            {
                if (value == _DigitizerName)
                    return;
                _DigitizerName = value;
            }
        }

        private Element<EnumGrabberRaft> _GrabRaft
             = new Element<EnumGrabberRaft>();
        // [XmlAttribute("GrabType")]
        public Element<EnumGrabberRaft> GrabRaft
        {
            get { return _GrabRaft; }
            set
            {
                if (value == _GrabRaft)
                    return;
                _GrabRaft = value;
            }
        }

        private Element<EnumGrabberMode> _EmulGrabMode
            = new Element<EnumGrabberMode>();
        public Element<EnumGrabberMode> EmulGrabMode
        {
            get { return _EmulGrabMode; }
            set
            {
                if (value == _EmulGrabMode)
                    return;
                _EmulGrabMode = value;
            }
        }



        // private Element<int> _ChannelCount
        //      = new Element<int>();
        //// [XmlAttribute("ChannelCount")]
        // public Element<int> ChannelCount
        // {
        //     get { return _ChannelCount; }
        //     set
        //     {
        //         if (value == _ChannelCount)
        //             return;
        //         _ChannelCount = value;
        //     }
        // }

        // private Element<int> _ColorDept
        //      = new Element<int>();
        //// [XmlAttribute("ColorDept")]
        // public Element<int> ColorDept
        // {
        //     get { return _ColorDept; }
        //     set
        //     {
        //         if (value == _ColorDept)
        //             return;
        //         _ColorDept = value;
        //     }
        // }

        private Element<string> _DCF
             = new Element<string>();
        //  [XmlAttribute("DCF")]
        public Element<string> DCF
        {
            get { return _DCF; }
            set { _DCF = value; }
        }

        private Element<int> _DigiGroup
             = new Element<int>();
        //  [XmlAttribute("DigiGroup")]
        public Element<int> DigiGroup
        {
            get { return _DigiGroup; }
            set { _DigiGroup = value; }
        }

        private Element<int> _GrabTimeOut
           = new Element<int>();
        //  [XmlAttribute("DigiGroup")]
        public Element<int> GrabTimeOut
        {
            get { return _GrabTimeOut; }
            set { _GrabTimeOut = value; }
        }
    }

}
