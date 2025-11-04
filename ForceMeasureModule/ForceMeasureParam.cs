using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ForceMeasureModule
{
    [Serializable]
    public class ForceMeasureParam : INotifyPropertyChanged, IParamNode, ISystemParameterizable
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region // IParamNode implementation
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
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
        public string FilePath { get; } = "ForceMeasure";
        public string FileName { get; } = "ForceMeasureParam.Json";
        #endregion
        #region // Force measure parameters

        private Element<double> _AuxEncDtoPRatio
            = new Element<double>(); 

        public Element<double> AuxEncDtoPRatio
        {
            get { return _AuxEncDtoPRatio; }
            set
            {
                if (value != _AuxEncDtoPRatio)
                {
                    _AuxEncDtoPRatio = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Z0RigidityCoeff
            = new Element<double>(); 

        public Element<double> Z0RigidityCoeff
        {
            get { return _Z0RigidityCoeff; }
            set
            {
                if (value != _Z0RigidityCoeff)
                {
                    _Z0RigidityCoeff = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _Z1RigidityCoeff
            = new Element<double>(); 

        public Element<double> Z1RigidityCoeff
        {
            get { return _Z1RigidityCoeff; }
            set
            {
                if (value != _Z1RigidityCoeff)
                {
                    _Z1RigidityCoeff = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Z2RigidityCoeff
            = new Element<double>(); 

        public Element<double> Z2RigidityCoeff
        {
            get { return _Z2RigidityCoeff; }
            set
            {
                if (value != _Z2RigidityCoeff)
                {
                    _Z2RigidityCoeff = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<bool> _EnableForceMeasure
            = new Element<bool>();

        public Element<bool> EnableForceMeasure
        {
            get { return _EnableForceMeasure; }
            set
            {
                if (value != _EnableForceMeasure)
                {
                    _EnableForceMeasure = value;
                    RaisePropertyChanged();
                }
            }
        }

        
        #endregion
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            retval = EventCodeEnum.NONE;
            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            AuxEncDtoPRatio.Value = 524.288;
            Z0RigidityCoeff.Value = 0.004220851000;
            Z1RigidityCoeff.Value = 0.004228364000;
            Z2RigidityCoeff.Value = 0.003734001000;
            EnableForceMeasure.Value = false;
            retval = EventCodeEnum.NONE;
            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            AuxEncDtoPRatio.Value = 524.288;
            Z0RigidityCoeff.Value = 0.00422;
            Z1RigidityCoeff.Value = 0.00422;
            Z2RigidityCoeff.Value = 0.00422;
            EnableForceMeasure.Value = false;
            retval = EventCodeEnum.NONE;
            return retval;
        }

        public void SetElementMetaData()
        {
            AuxEncDtoPRatio.ElementName = "AuxEncDtoPRatio";
            AuxEncDtoPRatio.CategoryID = "10007";
            AuxEncDtoPRatio.Description = "Aux. Encoder Distance to Pulse Ratio";
            AuxEncDtoPRatio.Unit = "Pulse/um";
            AuxEncDtoPRatio.LowerLimit = 0.0;
            AuxEncDtoPRatio.UpperLimit = 1000000.0;

            Z0RigidityCoeff.ElementName = "Z0RigidityCoeff";
            Z0RigidityCoeff.CategoryID = "10007";
            Z0RigidityCoeff.Description = "Z0 Actuator Rigidity Coeff.";
            Z0RigidityCoeff.Unit = "Kg/um";
            Z0RigidityCoeff.LowerLimit = 0.0;
            Z0RigidityCoeff.UpperLimit = 1000000.0;

            Z1RigidityCoeff.ElementName = "Z1RigidityCoeff";
            Z1RigidityCoeff.CategoryID = "10007";
            Z1RigidityCoeff.Description = "Z1 Actuator Rigidity Coeff.";
            Z1RigidityCoeff.Unit = "Kg/um";
            Z1RigidityCoeff.LowerLimit = 0.0;
            Z1RigidityCoeff.UpperLimit = 1000000.0;

            Z2RigidityCoeff.ElementName = "Z2RigidityCoeff";
            Z2RigidityCoeff.CategoryID = "10007";
            Z2RigidityCoeff.Description = "Z1 Actuator Rigidity Coeff.";
            Z2RigidityCoeff.Unit = "Kg/um";
            Z2RigidityCoeff.LowerLimit = 0.0;
            Z2RigidityCoeff.UpperLimit = 1000000.0;

            EnableForceMeasure.ElementName = "EnableForceMeasure";
            EnableForceMeasure.CategoryID = "10007";
            EnableForceMeasure.Description = "Enable Force Measurement";
            EnableForceMeasure.Unit = "";
            EnableForceMeasure.LowerLimit = 0;
            EnableForceMeasure.UpperLimit = 1;

                     
        }
    }
}
