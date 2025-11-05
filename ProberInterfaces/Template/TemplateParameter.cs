using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces.Template
{
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    public interface ITemplateParameter
    {
        string BasePath { get; set; }
        ObservableCollection<TemplateInfo> TemplateInfos { get; }
        TemplateInfo SeletedTemplate { get; set; }
    }


    [Serializable]
    public class TemplateInfo : INotifyPropertyChanged,  IParamNode
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
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        private Element<string> _Name
             = new Element<string>();
        [XmlAttribute("Name")]
        public Element<string> Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private Element<string> _Path
             = new Element<string>();
        [XmlAttribute("Path")]
        public Element<string> Path
        {
            get { return _Path; }
            set
            {
                if (value != _Path)
                {
                    _Path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }


        public TemplateInfo()
        {

        }

        public TemplateInfo(string name)
        {
            Name.Value = name;
        }
        public TemplateInfo(string name, string path)
        {
            try
            {
            Name.Value = name;
            Path.Value = path;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

}
