using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MultiExecuter.Model
{
    [Serializable]
    public class DiskItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<DiskItem> _DiskItemCollection;
        public ObservableCollection<DiskItem> DiskItemCollection
        {
            get { return _DiskItemCollection; }
            set
            {
                _DiskItemCollection = value;
                NotifyPropertyChanged(nameof(DiskItemCollection));
            }
        }

        public DiskItem(string name = null, string label = null,int uspace=0, int aspace =0, int tspace=0, int percent =0)
        {

            this.DriveName = name;
            this.DriveLabel = label;
            this.UsageSpace = uspace;
            this.AvailableSpace = aspace;
            this.TotalSpace = tspace;
            this.Percent = percent;

           
        }

        //public DiskItem(int type)
        //{

        //}
        
        [XmlElement]
        private string _DriveName;
        public string DriveName
        {
            get { return _DriveName; }
            set
            {
                if (value != _DriveName)
                {
                    _DriveName = value;
                    NotifyPropertyChanged(nameof(DriveName));
                }
            }
        }

        [XmlElement]
        private string _DriveType;
        public string DriveType
        {
            get { return _DriveType; }
            set
            {
                if (value != _DriveType)
                {
                    _DriveType = value;
                    NotifyPropertyChanged(nameof(DriveType));
                }
            }
        }

        [XmlElement]
        private string _DriveLabel;
        public string DriveLabel
        {
            get { return _DriveLabel; }
            set
            {
                if (value != _DriveLabel)
                {
                    _DriveLabel = value;
                    NotifyPropertyChanged(nameof(DriveLabel));
                }
            }
        }

        [XmlElement]
        private int _AvailableSpace;
        public int AvailableSpace
        {
            get { return _AvailableSpace; }
            set
            {
                if (value != _AvailableSpace)
                {
                    _AvailableSpace = value;
                    NotifyPropertyChanged(nameof(AvailableSpace));
                }

                
            }
        }


        [XmlElement]
        private int _TotalSpace;
        public int TotalSpace
        {
            get { return _TotalSpace; }
            set
            {
                if (value != _TotalSpace)
                {
                    _TotalSpace = value;
                    NotifyPropertyChanged(nameof(TotalSpace));
                }
            }
        }
        [XmlElement]
        private int _UsageSpace;
        public int UsageSpace
        {
            get { return _UsageSpace; }
            set
            {
                if (value != _UsageSpace)
                {
                    _UsageSpace = value;
                    NotifyPropertyChanged(nameof(UsageSpace));
                }
            }
        }

        [XmlElement]
        private int _Percent;
        public int Percent
        {
            get { return _Percent; }
            set
            {
                if (value != _Percent)
                {
                    _Percent = value;
                    NotifyPropertyChanged(nameof(Percent));
                }
            }
        }
    }
}
