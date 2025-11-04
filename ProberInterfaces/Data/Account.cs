using Newtonsoft.Json;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ProberInterfaces
{
    [Serializable]
    public class Account : INotifyPropertyChanged, ISystemParameterizable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #region <remarks> ISystemParameterizable Property </remarks>

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; set; } = "Account";
        public string FileName { get; set; } = "Account.json";
        public string Genealogy { get; set; }
        public List<object> Nodes { get; set; } = new List<object>();
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

        #endregion

        #region <remarks> Property </remarks>

        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (value != _UserName)
                {
                    _UserName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                if (value != _Password)
                {
                    _Password = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _UserLevel;
        public int UserLevel
        {
            get { return _UserLevel; }
            set
            {
                if (value != _UserLevel)
                {
                    _UserLevel = value;
                    RaisePropertyChanged();
                }
            }
        }
        [NonSerialized]
        private string _ImageSource;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string ImageSource
        {
            get { return _ImageSource; }
            set
            {
                if (value != _ImageSource)
                {
                    _ImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region <ramarks> ISystemParameterizable Method </ramarks>

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
            UserName = "SUPERUSER";
            Password = "SUPERUSER";
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }

        #endregion

        public void ChangeAccount(Account dest)
        {
            this.UserName = dest.UserName;
            this.Password = dest.Password;
            this.UserLevel = dest.UserLevel;
            this.ImageSource = dest.ImageSource;
        }

    }
}
