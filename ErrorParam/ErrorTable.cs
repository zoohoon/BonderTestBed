using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ProberInterfaces;
using System.Xml.Serialization;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;

namespace ErrorParam
{

    [Serializable]
    public class FirstErrorTable : INotifyPropertyChanged, ISystemParameterizable
    {
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
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public ErrorParamList TBL_X_LINEAR = new ErrorParamList();

        public ErrorParamList TBL_X_STRAIGHT = new ErrorParamList();

        public ErrorParamList TBL_X_ANGULAR = new ErrorParamList();

        public ErrorParamList TBL_Y_LINEAR = new ErrorParamList();

        public ErrorParamList TBL_Y_STRAIGHT = new ErrorParamList();

        public ErrorParamList TBL_Y_ANGULAR = new ErrorParamList();
        public string FilePath { get; } = "ErrorTable";

        public string FileName { get; } = "ErrorTable1D.json";
    }
    public class ErrorParamList:ObservableCollection<ErrorParameter>
    {
        public List<double> GetPosList()
        {
            return this.Select(item => item.Position).ToList();
        }
        public List<double> GetValueList()
        {
            return this.Select(item => item.ErrorValue).ToList();
        }
    }
    [Serializable]
    public class MakingErrorTable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public List<double> LINEAR_X_LOW = new List<double>();

        public List<double> LINEAR_X_HIGH = new List<double>();

        public List<double> POS_ERROR_X_Low = new List<double>();

        public List<double> POS_ERROR_X_HIGH = new List<double>();


        public List<double> STRAIGHT_X_LOW = new List<double>();


        public List<double> STRAIGHT_X_HIGH = new List<double>();

        public List<double> ANGULAR_X_LOW = new List<double>();


        public List<double> ANGULAR_X_HIGH = new List<double>();


        public List<double> LINEAR_Y_LOW = new List<double>();

        public List<double> LINEAR_Y_MID = new List<double>();

        public List<double> LINEAR_Y_MID2 = new List<double>();

        public List<double> LINEAR_Y_HIGH = new List<double>();

        public List<double> POS_ERROR_Y_Low = new List<double>();
        public List<double> POS_ERROR_Y_Mid = new List<double>();

        public List<double> POS_ERROR_Y_HIGH = new List<double>();


        public List<double> STRAIGHT_Y_LOW = new List<double>();

        public List<double> STRAIGHT_Y_MID = new List<double>();

        public List<double> STRAIGHT_Y_MID2 = new List<double>();


        public List<double> STRAIGHT_Y_HIGH = new List<double>();

        public List<double> ANGULAR_Y_LOW = new List<double>();

        public List<double> ANGULAR_Y_MID = new List<double>();
        public List<double> ANGULAR_Y_MID2 = new List<double>();

        public List<double> ANGULAR_Y_HIGH = new List<double>();

        //public List<double> ZangleYLow = new List<double>();

        //public List<double> ZheightYLow = new List<double>();

        //public List<double> ZangleYMid = new List<double>();

        //public List<double> ZangleYMid2 = new List<double>();

        //public List<double> ZangleYHigh = new List<double>();

        //public List<double> ZheightYMid = new List<double>();

        //public List<double> ZheightYMid2 = new List<double>();

        //public List<double> ZheightYHigh = new List<double>();


        //public List<double> Zangle1 = new List<double>();

        //public List<double> Zangle1pos = new List<double>();

        //public List<double> Zheight1 = new List<double>();

        //public List<double> Zheight1pos = new List<double>();
    }

    [Serializable]
    public class SecondErrorTable : INotifyPropertyChanged, ISystemParameterizable
    {
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

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[SecondErrorTable] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<ErrorParameter2D> TBL_OX_LINEAR = new ObservableCollection<ErrorParameter2D>();

        public ObservableCollection<ErrorParameter2D> TBL_OX_STRAIGHT = new ObservableCollection<ErrorParameter2D>();

        public string FilePath { get; } = "ErrorTable";

        public string FileName { get; } = "ErrorTable2D.json";
    }
}
