using System;
using System.Collections.Generic;

namespace ControlModules.Chiller
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ChillerErrorParameter : IParam, ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region < ISystemParameterizable Property >
        public string FilePath { get; } = "EnvControl";
        public string FileName { get; } = "ChillerErrorParam.json";
        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }
        #endregion

        private Dictionary<double,string> _ChillerErrorMessageDic
             = new Dictionary<double, string>();
        public Dictionary<double,string> ChillerErrorMessageDic
        {
            get { return _ChillerErrorMessageDic; }
            set
            {
                if (value != _ChillerErrorMessageDic)
                {
                    _ChillerErrorMessageDic = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region < IParam Method>
        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ChillerErrorMessageDic.Add(-2108, "");
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


        public void SetElementMetaData()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

    }

}
