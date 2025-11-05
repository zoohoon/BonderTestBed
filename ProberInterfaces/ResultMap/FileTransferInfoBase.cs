using Newtonsoft.Json;
using ProberInterfaces.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ProberInterfaces.ResultMap
{
    [Serializable]
    public abstract class FileTransferInfoBase : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string Genealogy { get; set; }
        public List<object> Nodes { get; set; }
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

        private Element<string> _UploadPath = new Element<string>();
        public Element<string> UploadPath
        {
            get { return _UploadPath; }
            set
            {
                if (value != _UploadPath)
                {
                    _UploadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _DownloadPath = new Element<string>();
        public Element<string> DownloadPath
        {
            get { return _DownloadPath; }
            set
            {
                if (value != _DownloadPath)
                {
                    _DownloadPath = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class HDDTransferInfo : FileTransferInfoBase, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    [Serializable]
    public class FTPTransferInfo : FileTransferInfoBase, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<IPAddressVer4> _ip = new Element<IPAddressVer4>();
        public Element<IPAddressVer4> IP
        {
            get { return _ip; }
            set
            {
                if (value != _ip)
                {
                    _ip = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _port = new Element<int>();
        public Element<int> Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<String> _userName = new Element<string>();
        public Element<String> UserName
        {
            get { return _userName; }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<String> _userPassword = new Element<string>();
        public Element<String> UserPassword
        {
            get { return _userPassword; }
            set
            {
                if (value != _userPassword)
                {
                    _userPassword = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
