using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ErrorParam
{
    [Serializable]
    public class ErrorParameter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public ErrorParameter()
        {

        }
        public ErrorParameter(double position, double errorValue)
        {
            try
            {
                Position = position;
                ErrorValue = errorValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public double ErrorValue;
        public double Position;
        public override bool Equals(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return false;
                }

                ErrorParameter errorParam = obj as ErrorParameter;

                if (errorParam == null)
                {
                    return false;
                }

                return this.Position == errorParam.Position;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override int GetHashCode()
        {
            try
            {
                return this.Position.GetHashCode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    [Serializable]
    public class ErrorParameter2D : INotifyPropertyChanged, IParam
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }        
        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public ErrorParameter2D(double PositionY)
        {
            try
            {
                this.PositionY = PositionY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ErrorParameter2D()
        {
            try
            {
                this.PositionY = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public List<ErrorParameter2D_X> ListY;
        public double PositionY { set; get; }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public string FilePath { get; set; }
        [XmlIgnore, JsonIgnore]
        public string FileName { get; set; }
        [XmlIgnore, JsonIgnore]
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
        = new List<object>();
    }
    [Serializable]
    public class ErrorParameter2D_X : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ErrorParameter2D_X(double PositionX, double ErrorValue)
        {
            this.PositionX = PositionX;
            this.ErrorValue = ErrorValue;
        }
        public ErrorParameter2D_X()
        {
            try
            {
                this.PositionX = 0;
                this.ErrorValue = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public double ErrorValue { set; get; }
        public double PositionX { set; get; }
    }
}
