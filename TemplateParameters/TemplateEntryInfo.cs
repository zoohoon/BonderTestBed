using System;

namespace TemplateParameters
{
    using LogModule;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public class TemplateEntryInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ObservableCollection<EntryInfo> _EntryInfos
            = new ObservableCollection<EntryInfo>();
        public ObservableCollection<EntryInfo> EntryInfos
        {
            get { return _EntryInfos; }
            set
            {
                if (value != _EntryInfos)
                {
                    _EntryInfos = value;
                    NotifyPropertyChanged("EntryInfos");
                }
            }
        }

        public TemplateEntryInfo()
        {

        }
    }

    public class EntryInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string _Name;
        [XmlAttribute("Name")]
        public string Name
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

        private string _Path;
        [XmlAttribute("Path")]
        public string Path
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

        public EntryInfo()
        {

        }

        public EntryInfo(string name, string path)
        {
            try
            {
                Name = name;
                Path = path;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
