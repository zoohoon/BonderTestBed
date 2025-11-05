using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using ProberInterfaces.Template;
    [Serializable]
    public class TemplateParameter : INotifyPropertyChanged, ITemplateParameter
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

        private string _BasePath;
        public string BasePath
        {
            get { return _BasePath; }
            set
            {
                if (value != _BasePath)
                {
                    _BasePath = value;
                    NotifyPropertyChanged("BasePath");
                }
            }
        }


        private ObservableCollection<TemplateInfo> _TemplateInfos
            = new ObservableCollection<TemplateInfo>();
        public ObservableCollection<TemplateInfo> TemplateInfos
        {
            get { return _TemplateInfos; }
            set
            {
                if (value != _TemplateInfos)
                {
                    _TemplateInfos = value;
                    NotifyPropertyChanged("TemplateInfos");
                }
            }
        }

        private TemplateInfo _SeletedTemplate;
        [ParamIgnore]
        public TemplateInfo SeletedTemplate
        {
            get { return _SeletedTemplate; }
            set
            {
                if (value != _SeletedTemplate)
                {
                    _SeletedTemplate = value;
                    NotifyPropertyChanged("SeletedTemplate");
                }
            }
        }



        public TemplateParameter()
        {

        }

    }
}
