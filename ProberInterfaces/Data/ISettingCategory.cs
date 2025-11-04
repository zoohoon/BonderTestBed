using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;

namespace ProberInterfaces
{
    public enum EnableType
    {
        ENABLE,
        DISABLE
    }

    [Serializable]
    public class SettingCategoryInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public string Name { get; set; }
        public string Icon { get; set; }

        private string _ID = "00000000";
        public string ID
        {
            get { return _ID; }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get { return _Visibility; }
            set
            {
                if (value != _Visibility)
                {
                    _Visibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (value != _IsEnabled)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MaskingLevel = 0;
        public int MaskingLevel
        {
            get { return _MaskingLevel; }
            set
            {
                if (value != _MaskingLevel)
                {
                    _MaskingLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<SettingInfo> SettingInfos { get; set; }
        = new List<SettingInfo>();
    }

    [Serializable]
    public class SettingInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        private Guid _ViewGUID;
        public Guid ViewGUID
        {
            get { return _ViewGUID; }
            set
            {
                if (value != _ViewGUID)
                {
                    _ViewGUID = value;
                    RaisePropertyChanged(nameof(ViewGUID));
                }
            }
        }

        // 1번째 1글자 : System 0 /Device 1
        // 2번째 3글자 : 1단계 카테고리
        // 3번째 4글자 : 2단계 카테고리
        private string _CategoryID = "00000000";
        public string CategoryID
        {
            get { return _CategoryID; }
            set
            {
                if (value != _CategoryID)
                {
                    _CategoryID = value;
                    RaisePropertyChanged(nameof(CategoryID));
                }
            }
        }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get { return _Visibility; }
            set
            {
                if (value != _Visibility)
                {
                    _Visibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (value != _IsEnabled)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MaskingLevel = 0;
        public int MaskingLevel
        {
            get { return _MaskingLevel; }
            set
            {
                if (value != _MaskingLevel)
                {
                    _MaskingLevel = value;
                    RaisePropertyChanged(nameof(MaskingLevel));
                }
            }
        }

        private string _Icon;
        [XmlIgnore, JsonIgnore]
        public string Icon
        {
            get { return _Icon; }
            set
            {
                if (value != _Icon)
                {
                    _Icon = value;
                    RaisePropertyChanged(nameof(Icon));
                }
            }
        }

        private bool _IsSavedCategory;
        [XmlIgnore, JsonIgnore]
        public bool IsSavedCategory
        {
            get { return _IsSavedCategory; }
            set
            {
                if (value != _IsSavedCategory)
                {
                    _IsSavedCategory = value;
                }
            }
        }
    }
}
