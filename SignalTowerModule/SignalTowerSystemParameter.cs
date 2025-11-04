using LogModule;
using Newtonsoft.Json;
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
using System.Windows.Media;
using System.Xml.Serialization;

namespace SignalTowerModule
{
    public class SignalTowerSystemParameter : ISystemParameterizable, IParamNode, IParam
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {                
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        public void SetElementMetaData()
        {

        }

        [XmlIgnore, JsonIgnore]
        public String Genealogy { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner { get; set; }

        [XmlIgnore, JsonIgnore]
        public String FilePath { get; } = "";
        [XmlIgnore, JsonIgnore]
        public String FileName { get; } = "SignalTowerSystemParameter.json";

        private List<SignalTowerUnitBase> _SignalTowerUnitBase = new List<SignalTowerUnitBase>();
        public List<SignalTowerUnitBase> SignalTowerUnitBase
        {
            get { return _SignalTowerUnitBase; }
            set { _SignalTowerUnitBase = value;}
        }

        public SignalTowerSystemParameter() 
        {
            
        }
        
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            try
            {
                //=====> RED
                _SignalTowerUnitBase.Add(new SignalTowerUnitBase(
                                        "RED",
                                        "Lamp",
                                        "DOFrontSigRed",                               
                                        Colors.Red,
                                        false));
                //=====> GREEN
                _SignalTowerUnitBase.Add(new SignalTowerUnitBase(
                                       "GREEN",
                                       "Lamp",
                                       "DOFrontSigGrn",
                                       Colors.Green,
                                       false));
                //=====> YELLOW
                _SignalTowerUnitBase.Add(new SignalTowerUnitBase(
                                       "YELLOW",
                                       "Lamp",
                                       "DOFrontSigYl",
                                       Colors.Yellow,
                                       false));
                //=====> BUZZER
                _SignalTowerUnitBase.Add(new SignalTowerUnitBase(
                                       "BUZZER",
                                       "Buzzer",
                                       "DOFrontSigBuz",
                                       Colors.Gray,
                                       true));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }
}
