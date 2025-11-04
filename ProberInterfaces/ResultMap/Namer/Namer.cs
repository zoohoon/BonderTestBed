using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;

namespace ProberInterfaces.ResultMap
{
    [DataContract]
    public enum NamingComponentType
    {
        [EnumMember]
        VALUE,
        [EnumMember]
        SUFFIX,
        [EnumMember]
        SEPERATOR,
    }

    [DataContract]
    public enum NamingSuffixType
    {
        [EnumMember]
        NUMERICAL,
        [EnumMember]
        LOWERALPHABETICAL,
        [EnumMember]
        UPPERALPHABETICAL,
    }

    [Serializable]
    public class Namer : INamer, IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IParamNode

        public string Genealogy { get; set; }
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

        #endregion


        private Element<string> _alias = new Element<string>();
        public Element<string> Alias
        {
            get { return _alias; }
            set
            {
                if (value != _alias)
                {
                    _alias = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsLotNameChangedTriggered { get; set; }
        public NamingComponentSet ComponentSet { get; set; }

        [JsonIgnore]
        public Dictionary<EnumProberMapProperty, object> ProberMapDictionary { get; private set; }

        public void SetProberMapDictionary(Dictionary<EnumProberMapProperty, object> dictionary)
        {
            this.ProberMapDictionary = dictionary;
        }

        public Namer()
        {
            this.ComponentSet = new NamingComponentSet();
            IsLotNameChangedTriggered = false;
        }
        
        //private void SetNewValue(EnumProberMapProperty key, object value)
        //{
        //    try
        //    {
        //        bool isExist = false;
        //        object old = null;

        //        isExist = ProberMapDictionary.TryGetValue(key, out old);

        //        if (isExist == true)
        //        {
        //            ProberMapDictionary[key] = value;
        //        }
        //        else
        //        {
        //            ProberMapDictionary.Add(key, value);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public void SetValue(EnumProberMapProperty key, object value)
        {
            try
            {
                bool isExist = false;
                object old = null;

                isExist = ProberMapDictionary.TryGetValue(key, out old);

                if (isExist == true)
                {
                    ProberMapDictionary[key] = value;
                }
                else
                {
                    ProberMapDictionary.Add(key, value);
                }

                //SetNewValue(key, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Run(out string name)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            name = string.Empty;

            try
            {
                retval = ComponentSet.Run(this, out name);

                LoggerManager.Debug($"[Namer], Run(), retval = {retval}, name = {name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class NamingComponentSet : IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IParamNode

        public string Genealogy { get; set; }
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

        #endregion

        // TODO : 타겟 위치, 값의 범위, 연속된 값의 관계
        // 관계는 Type에 따라, 정의해놓기.

        public List<NamingComponentBase> namingComponents { get; set; }

        public NamingComponentSet()
        {
            this.namingComponents = new List<NamingComponentBase>();
        }

        public void Add(NamingComponentBase component)
        {
            namingComponents.Add(component);
        }

        public EventCodeEnum Run(INamer namer, out string name)
        {
            // 시작을 NONE으로...
            EventCodeEnum retval = EventCodeEnum.NONE;
            name = string.Empty;

            try
            {
                string prefix = string.Empty;

                foreach (var component in namingComponents)
                {
                    string CurStr = string.Empty;
                    component.namer = namer;
                    CurStr = component.Run();

                    //// VALUE
                    //if (component is NamingValueComponent)
                    //{
                    //    CurStr = (component as NamingValueComponent).Run();
                    //}
                    //// SUFFIX
                    //else if (component is NamingSuffixComponent)
                    //{
                    //    CurStr = (component as NamingSuffixComponent).Run();
                    //}
                    //// SEPERATOR
                    //else if (component is NamingSeperatorComponent)
                    //{
                    //    CurStr = (component as NamingSeperatorComponent).Run();
                    //}
                    //// CONSTANT
                    //else if (component is NamingConstantComponent)
                    //{
                    //    CurStr = (component as NamingConstantComponent).Run();
                    //}

                    if (string.IsNullOrEmpty(CurStr) == true)
                    {
                        // ERROR
                        retval = EventCodeEnum.UNDEFINED;
                        break;
                    }
                    else
                    {
                        name += CurStr;
                    }
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
        
}
