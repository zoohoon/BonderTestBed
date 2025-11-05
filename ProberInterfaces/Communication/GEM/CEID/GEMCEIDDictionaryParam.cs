using System;
using System.Collections.Generic;

namespace ProberInterfaces.GEM
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class GEMCEIDDictionaryParam : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #endregion

        #region ==> IParam Implement
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        [JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore, ParamIgnore]
        public object Owner { get; set; }
        #endregion

        public string FilePath { get; } = "GEM";
        public string FileName { get; } = "GEMCEIDDictionaryParam.Json";

        private Dictionary<long,List<long>> _CEIDDic
             = new Dictionary<long, List<long>>();
        public Dictionary<long,List<long>> CEIDDic
        {
            get { return _CEIDDic; }
            set
            {
                if (value != _CEIDDic)
                {
                    _CEIDDic = value;
                    RaisePropertyChanged();
                }
            }
        }



        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            #region ==> Micron

            #endregion
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void SetElementMetaData()
        {
        }
    }
}
