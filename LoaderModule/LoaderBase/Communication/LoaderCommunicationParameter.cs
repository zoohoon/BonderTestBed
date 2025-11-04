using System;
using System.Collections.Generic;

namespace LoaderBase.Communication{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    [Serializable]
    public class LoaderCommunicationParameter : INotifyPropertyChanged, ISystemParameterizable , IParam
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region //.. IParam
        public string FilePath { get; set; } = "";

        public string FileName { get; set; } = "LoaderServiceParameter.Json";

        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }


        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }
        
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                LauncherParams.Add(new LauncherParameter(1, 5000, "192.168.4.30"));
                LauncherParams[0].StageParams.Add(new StageCommiuncationParameter(1, 9000, "192.168.3.14"));
                LauncherParams[0].StageParams.Add(new StageCommiuncationParameter(2, 9000, "192.168.3.20"));
                LauncherParams[0].StageParams.Add(new StageCommiuncationParameter(3, 9000, "192.168.3.12"));
                LauncherParams[0].StageParams.Add(new StageCommiuncationParameter(4, 9000, "192.168.3.13"));

                LauncherParams.Add(new LauncherParameter(2, 5000, "192.168.4.30"));
                LauncherParams[1].StageParams.Add(new StageCommiuncationParameter(5, 8000, "192.168.3.29"));
                LauncherParams[1].StageParams.Add(new StageCommiuncationParameter(6, 8200, "192.168.3.29"));
                LauncherParams[1].StageParams.Add(new StageCommiuncationParameter(7, 8400, "192.168.3.29"));
                LauncherParams[1].StageParams.Add(new StageCommiuncationParameter(8, 8600, "192.168.3.29"));

                LauncherParams.Add(new LauncherParameter(3, 5000, "192.168.4.30"));
                LauncherParams[2].StageParams.Add(new StageCommiuncationParameter(9, 7000, "192.168.3.21"));
                LauncherParams[2].StageParams.Add(new StageCommiuncationParameter(10, 7200, "192.168.3.21"));
                LauncherParams[2].StageParams.Add(new StageCommiuncationParameter(11, 7400, "192.168.3.21"));
                LauncherParams[2].StageParams.Add(new StageCommiuncationParameter(12, 7600, "192.168.3.21"));

                retval = EventCodeEnum.NONE; 
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetElementMetaData()
        {
            return;
        }

        #endregion


        private ObservableCollection<LauncherParameter> _LauncherParams
             = new ObservableCollection<LauncherParameter>();
        public ObservableCollection<LauncherParameter> LauncherParams
        {
            get { return _LauncherParams; }
            set
            {
                if (value != _LauncherParams)
                {
                    _LauncherParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LIP = "192.168.8.101";
        public string LIP
        {
            get { return _LIP; }
            set
            {
                if (value != _LIP)
                {
                    _LIP = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _DispProxyPort;
        public int DispProxyPort
        {
            get { return _DispProxyPort; }
            set
            {
                if (value != _DispProxyPort)
                {
                    _DispProxyPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LoaderCommunicationParameter()
        {

        }
    }

    [Serializable]
    public class LauncherParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _LauncherIndex;
        public int LauncherIndex
        {
            get { return _LauncherIndex; }
            set
            {
                if (value != _LauncherIndex)
                {
                    _LauncherIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _Port;
        public int Port
        {
            get { return _Port; }
            set
            {
                if (value != _Port)
                {
                    _Port = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _IP;
        public string IP
        {
            get { return _IP; }
            set
            {
                if (value != _IP)
                {
                    _IP = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<StageCommiuncationParameter> _StageParams
             = new ObservableCollection<StageCommiuncationParameter>();
        public ObservableCollection<StageCommiuncationParameter> StageParams
        {
            get { return _StageParams; }
            set
            {
                if (value != _StageParams)
                {
                    _StageParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LauncherParameter()
        {

        }

        public LauncherParameter(int index , int port, string ip)
        {
            LauncherIndex = index;
            Port = port;
            IP = ip;
        }

    }

    [Serializable]
    public class StageCommiuncationParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
            set
            {
                if (value != _Port)
                {
                    _Port = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _IP;
        public string IP
        {
            get { return _IP; }
            set
            {
                if (value != _IP)
                {
                    _IP = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnableAutoConnect = false;
        public bool EnableAutoConnect
        {
            get { return _EnableAutoConnect; }
            set
            {
                if (value != _EnableAutoConnect)
                {
                    _EnableAutoConnect = value;
                    RaisePropertyChanged();
                }
            }
        }

        public StageCommiuncationParameter()
        {

        }

        public StageCommiuncationParameter(int index, int port, string ip)
        {
            Index = index;
            Port = port;
            IP = ip;
        }
    }

}
