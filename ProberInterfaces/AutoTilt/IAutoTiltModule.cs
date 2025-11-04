using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;

namespace ProberInterfaces.AutoTilt
{
    public interface IAutoTiltModule : IStateModule
    {
        //double CheckPinPlanePlanarity_By_Pin_HPT(out double pindif);
        //int GetPinPlaneCalVal_By_Pin_HPT();
        //void inputTestPinData();
        AutoTiltDeviceFile ATDeviceFile { get; set; }
        AutoTiltSystemFile ATSysFile { get; set; }

        EventCodeEnum SaveSysFileRefPos();
        EventCodeEnum SaveSysFileLastPos();
    }

    [Serializable]
    public class AutoTiltDeviceFile : INotifyPropertyChanged, IDeviceParameterizable,IParamNode
    {

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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
        public string FilePath { get; } = "";

        public string FileName { get; } = "AutoTiltDeviceFile.json";

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        

        private Element<double> _TiltSkipAreaRatioX = new Element<double>();
        public Element<double> TiltSkipAreaRatioX
        {
            get { return _TiltSkipAreaRatioX; }
            set
            {
                if (value != _TiltSkipAreaRatioX)
                {
                    _TiltSkipAreaRatioX = value;
                    NotifyPropertyChanged("TiltSkipAreaRatioX");
                }
            }
        }

        private Element<double> _TiltSkipAreaRatioY = new Element<double>();
        public Element<double> TiltSkipAreaRatioY
        {
            get { return _TiltSkipAreaRatioY; }
            set
            {
                if (value != _TiltSkipAreaRatioY)
                {
                    _TiltSkipAreaRatioY = value;
                    NotifyPropertyChanged("TiltSkipAreaRatioY");
                }
            }
        }

        private Element<int> _PinAdjustEnable = new Element<int>();
        public Element<int> PinAdjustEnable
        {
            get { return _PinAdjustEnable; }
            set
            {
                if (value != _PinAdjustEnable)
                {
                    _PinAdjustEnable = value;
                    NotifyPropertyChanged("PinAdjustEnable");
                }
            }
        }

        private Element<int> _TiltIntPlaneMinMaxtol = new Element<int>();
        public Element<int> TiltIntPlaneMinMaxtol
        {
            get { return _TiltIntPlaneMinMaxtol; }
            set
            {
                if (value != _TiltIntPlaneMinMaxtol)
                {
                    _TiltIntPlaneMinMaxtol = value;
                    NotifyPropertyChanged("TiltIntPlaneMinMaxtol");
                }
            }
        }

        public string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
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

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }




    }

    [Serializable]
    public class AutoTiltSystemFile : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[AutoTiltSystemFile] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public string FilePath { get; } = "";

        public string FileName { get; } = "AutoTiltSystemFile.json";
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetDefaultParam();
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }

        private Element<double> _TZ1RefPos = new Element<double>();
        public Element<double> TZ1RefPos
        {
            get { return _TZ1RefPos; }
            set
            {
                if (value != _TZ1RefPos)
                {
                    _TZ1RefPos = value;
                    NotifyPropertyChanged("TZ1RefPos");
                }
            }
        }

        private Element<double> _TZ2RefPos =  new Element<double>();
        public Element<double> TZ2RefPos
        {
            get { return _TZ2RefPos; }
            set
            {
                if (value != _TZ2RefPos)
                {
                    _TZ2RefPos = value;
                    NotifyPropertyChanged("TZ2RefPos");
                }
            }
        }

        private Element<double> _TZ3RefPos = new Element<double>();
        public Element<double> TZ3RefPos
        {
            get { return _TZ3RefPos; }
            set
            {
                if (value != _TZ3RefPos)
                {
                    _TZ3RefPos = value;
                    NotifyPropertyChanged("TZ3RefPos");
                }
            }
        }

        private Element<double> _TZ1LastPos =  new Element<double>();
        public Element<double> TZ1LastPos
        {
            get { return _TZ1LastPos; }
            set
            {
                if (value != _TZ1LastPos)
                {
                    _TZ1LastPos = value;
                    NotifyPropertyChanged("TZ1LastPos");
                }
            }
        }

        private Element<double> _TZ2LastPos = new Element<double>();
        public Element<double> TZ2LastPos
        {
            get { return _TZ2LastPos; }
            set
            {
                if (value != _TZ2LastPos)
                {
                    _TZ2LastPos = value;
                    NotifyPropertyChanged("TZ2LastPos");
                }
            }
        }

        private Element<double> _TZ3LastPos =  new Element<double>();
        public Element<double> TZ3LastPos
        {
            get { return _TZ3LastPos; }
            set
            {
                if (value != _TZ3LastPos)
                {
                    _TZ3LastPos = value;
                    NotifyPropertyChanged("TZ3LastPos");
                }
            }
        }

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

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
    }
}
