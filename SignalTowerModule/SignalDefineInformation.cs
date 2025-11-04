using LogModule;
using Newtonsoft.Json;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.SignalTower;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SignalTowerModule
{
    public enum EnumSignalTowerState
    {
        UNDEFINED,
        ON,
        OFF,
        BLINK,
        IGNORE,
    }

    [Serializable]
    public class SignalDefineInformation : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion        

        private string _SignalType;
        public string SignalType
        {
            get { return _SignalType; }
            set
            {
                if (_SignalType != value)
                {
                    _SignalType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumSignalTowerState _SignalState;
        public EnumSignalTowerState SignalState
        {
            get { return _SignalState; }
            set
            {
                if (_SignalState != value)
                {
                    _SignalState = value;
                    RaisePropertyChanged();
                }
            }
        }        

        public SignalDefineInformation()
        {

        }    

        public SignalDefineInformation(
            string signalTowerColor, 
            EnumSignalTowerState signalState )
        {
            SignalType = signalTowerColor;
            SignalState = signalState;
        }
    }    
}
