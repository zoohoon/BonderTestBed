using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class FFUSysParameter : INotifyPropertyChanged , IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region .. IParam Property
        [JsonIgnore]
        public string FilePath { get; }
        [JsonIgnore]
        public string FileName { get; }
        [JsonIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore]
        public object Owner { get; set; }
        [JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        private ushort _StartAddress;
        public ushort StartAddress
        {
            get { return _StartAddress; }
            set
            {
                if (value != _StartAddress)
                {
                    _StartAddress = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ushort _NumRegisters;
        public ushort NumRegisters
        {
            get { return _NumRegisters; }
            set
            {
                if (value != _NumRegisters)
                {
                    _NumRegisters = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Address;
        public string Address
        {
            get { return _Address; }
            set
            {
                if (value != _Address)
                {
                    _Address = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<int> _NodeNum;
        public ObservableCollection<int> NodeNum
        {
            get { return _NodeNum; }
            set
            {
                if (value != _NodeNum)
                {
                    _NodeNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumFFUModuleMode _FFUModuleType;
        public EnumFFUModuleMode FFUModuleType
        {
            get { return _FFUModuleType; }
            set
            {
                if (value != _FFUModuleType)
                {
                    _FFUModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                StartAddress = 0;
                NumRegisters = 4;
                Address = "COM5";
                NodeNum = new ObservableCollection<int>();
                NodeNum.Add(1);
                NodeNum.Add(2);
                NodeNum.Add(3);
                FFUModuleType = EnumFFUModuleMode.EMUL;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
    }

    public interface IFFUParameter
    {
        string IP { get; set; }
        int Port { get; set; }
        Element<EnumFFUModuleMode> FFUModuleMode { get; set; }

    }

}
