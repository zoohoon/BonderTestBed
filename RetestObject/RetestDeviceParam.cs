using LogModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Retest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace RetestObject
{
    /// <summary>
    /// <author>minskim</author>
    /// Retest 관련 parameter중 Enable와 InstantRetest value 값에 대한 Converter
    /// parameter type을 EnableEnum에서 Element<bool> type으로 변경하게 되면서 기존 device parameter를 deserialize할때 exception이 발생하게됨
    /// 이전 device parameter 호환 및 다음번 설정 변경시 정상적으로 migration 되기 위해 추가된 converter class
    /// </summary> 
    public class RetestParamMigrationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value, serializer);
            t.WriteTo(writer);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Element<bool> ret = new Element<bool>();
            ret.Value = false;

            Element<string> retTemp = serializer.Deserialize<Element<string>>(reader);
            object value = retTemp.Value;

            if (value == null || String.IsNullOrWhiteSpace(value.ToString()))
            {
                return ret;
            }
            value = value.ToString().ToLower().Trim();
            if(value.Equals("enable") || value.Equals("true"))
            {
                ret.Value = true;
            }
            return ret;
        }
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Element<bool>))
            {
                return true;
            }
            return false;
        }
    }

    public class BinSettingComponent : IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        //private RelayCommand<Object> _NCEnableClickCommand;

        //public ICommand NCEnableClickCommand
        //{
        //    get
        //    {
        //        if (null == _NCEnableClickCommand) _NCEnableClickCommand = new RelayCommand<Object>(NCEnableClickCommandFunc);
        //        return _NCEnableClickCommand;
        //    }
        //}
        //private void NCEnableClickCommandFunc(Object param)
        //{
        //    SaveDevParameter();
        //}
    }

    public class RetestComponent : IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        private Element<RetestCategoryEnum> _Category = new Element<RetestCategoryEnum>();
        public Element<RetestCategoryEnum> Category
        {
            get { return _Category; }
            set
            {
                if (value != _Category)
                {
                    _Category = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _Enable = new Element<bool>();
        [JsonConverter(typeof(RetestParamMigrationConverter))]
        public Element<bool> Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _InstantRetest = new Element<bool>();
        [JsonConverter(typeof(RetestParamMigrationConverter))]
        public Element<bool> InstantRetest
        {
            get { return _InstantRetest; }
            set
            {
                if (value != _InstantRetest)
                {
                    _InstantRetest = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ReTestMode> _Mode = new Element<ReTestMode>();
        public Element<ReTestMode> Mode
        {
            get { return _Mode; }
            set
            {
                if (value != _Mode)
                {
                    _Mode = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<int> _OnlineRetestIndex = new Element<int>();
        //public Element<int> OnlineRetestIndex
        //{
        //    get { return _OnlineRetestIndex; }
        //    set
        //    {
        //        if (value != _OnlineRetestIndex)
        //        {
        //            _OnlineRetestIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<int> _MaxTestCount = new Element<int>();
        public Element<int> MaxTestCount
        {
            get { return _MaxTestCount; }
            set
            {
                if (value != _MaxTestCount)
                {
                    _MaxTestCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public RetestComponent(RetestCategoryEnum category, EnableEnum enable, ReTestMode mode, int onlineretestindex = -1, int maxtestcount = 0)
        //{
        //    try
        //    {
        //        this.Category.Value = category;
        //        this.Enable.Value = enable;
        //        this.Mode.Value = mode;

        //        this.OnlineRetestIndex.Value = onlineretestindex;
        //        this.MaxTestCount.Value = maxtestcount;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}
    }

    public class RetestDeviceParam : IRetestDeviceParam, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "";

        public string FileName { get; } = "RetestDevice.json";

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
        
        private Element<ForcedLotModeEnum> _forcedLotMode = new Element<ForcedLotModeEnum>(ForcedLotModeEnum.UNDEFINED);
        public Element<ForcedLotModeEnum> ForcedLotMode
        {
            get { return _forcedLotMode; }
            set
            {
                if (value != _forcedLotMode)
                {
                    _forcedLotMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RetestComponent _Retest_CP1 = new RetestComponent();
        public RetestComponent Retest_CP1
        {
            get { return _Retest_CP1; }
            set
            {
                if (value != _Retest_CP1)
                {
                    _Retest_CP1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RetestComponent _Retest_MPP = new RetestComponent();
        public RetestComponent Retest_MPP
        {
            get { return _Retest_MPP; }
            set
            {
                if (value != _Retest_MPP)
                {
                    _Retest_MPP = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private RetestComponent _Retest_Online = new RetestComponent();
        //public RetestComponent Retest_Online
        //{
        //    get { return _Retest_Online; }
        //    set
        //    {
        //        if (_Retest_Online != value)
        //        {
        //            _Retest_Online = value;
        //        }
        //    }
        //}

        private List<RetestComponent> _Retest_Online_List = new List<RetestComponent>();
        public List<RetestComponent> Retest_Online_List
        {
            get { return _Retest_Online_List; }
            set
            {
                if (_Retest_Online_List != value)
                {
                    _Retest_Online_List = value;
                }
            }
        }

        private Element<int> _OnlineRetestCount = new Element<int>();
        public Element<int> OnlineRetestCount
        {
            get { return _OnlineRetestCount; }
            set
            {
                if (value != _OnlineRetestCount)
                {
                    _OnlineRetestCount = value;
                    RaisePropertyChanged();
                }
            }
        }

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
            }

            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ForcedLotMode = new Element<ForcedLotModeEnum>();
                ForcedLotMode.Value = ForcedLotModeEnum.UNDEFINED;

                Retest_CP1 = new RetestComponent();
                Retest_CP1.Enable.Value = false;
                Retest_CP1.InstantRetest.Value = false;

                Retest_Online_List = new List<RetestComponent>();
                RetestComponent retestOnline1 = new RetestComponent();
                retestOnline1.Enable.Value = false;
                retestOnline1.InstantRetest.Value = false;
                retestOnline1.Mode.Value = ReTestMode.ALL;

                RetestComponent retestOnline2 = new RetestComponent();
                retestOnline2.Enable.Value = false;
                retestOnline2.InstantRetest.Value = false;
                retestOnline2.Mode.Value = ReTestMode.ALL;

                RetestComponent retestOnline3 = new RetestComponent();
                retestOnline3.Enable.Value = false;
                retestOnline3.InstantRetest.Value = false;
                retestOnline3.Mode.Value = ReTestMode.ALL;

                Retest_Online_List.Add(retestOnline1);
                Retest_Online_List.Add(retestOnline2);
                Retest_Online_List.Add(retestOnline3);

                Retest_MPP = new RetestComponent();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SetDefaultParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetElementMetaData()
        {

        }

    }
}