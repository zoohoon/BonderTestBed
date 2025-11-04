using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProberErrorCode;
using LogModule;

namespace LoaderParameters.Data
{
    /// <summary>
    /// LoaderTest Option의 정보를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoaderTestOption : INotifyPropertyChanged, ICloneable, IParamNode, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; set; } = @"C:\Logs\ProberSystem\";

        public string FileName { get; set; } = @"LoaderTestOption.xml";

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public LoaderTestOption()
        {
            try
            {
                _OptionFlag = false;
                _ScanSlotNum = -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

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

        private bool _OptionFlag;
        [DataMember]
        public bool OptionFlag
        {
            get { return _OptionFlag; }
            set { _OptionFlag = value; RaisePropertyChanged(); }
        }

        private ScanFailOptionEnum _ScanFailOption;
        [DataMember]
        public ScanFailOptionEnum ScanFailOption
        {
            get { return _ScanFailOption; }
            set { _ScanFailOption = value; RaisePropertyChanged(); }
        }
        private int _ScanSlotNum;
        [DataMember]
        public int ScanSlotNum
        {
            get { return _ScanSlotNum; }
            set { _ScanSlotNum = value; RaisePropertyChanged(); }
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
        public object Clone()
        {
            LoaderTestOption info = new LoaderTestOption();
            return info;
        }
        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsScanValidate(int SlotNumber)
        {
            try
            {
                if (OptionFlag)
                {
                    if (this.ScanSlotNum == SlotNumber)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    [Serializable]
    [DataContract]
    public enum ScanFailOptionEnum
    {
        /// <summary>
        /// NONE
        /// </summary>
        [EnumMember]
        NONE,
        [EnumMember]
        FORCE_DONE
    }

}
