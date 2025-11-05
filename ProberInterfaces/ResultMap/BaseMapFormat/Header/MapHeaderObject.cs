using LogModule;
using ProberErrorCode;
using ProberInterfaces.ResultMap;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public class ProberMapSection
    {
        private EnumProberMapSection _Section;
        public EnumProberMapSection Section
        {
            get { return _Section; }
            set
            {
                if (value != _Section)
                {
                    _Section = value;
                }
            }
        }

        private object _Value;
        public object Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                }
            }
        }

        public ProberMapSection(EnumProberMapSection section, object val)
        {
            this.Section = section;
            this.Value = val;
        }
    }

    public class ProberMapProperty
    {
        private EnumProberMapProperty _ProberMapPropertyEnum;
        public EnumProberMapProperty ProberMapPropertyEnum
        {
            get { return _ProberMapPropertyEnum; }
            set
            {
                if (value != _ProberMapPropertyEnum)
                {
                    _ProberMapPropertyEnum = value;
                }
            }
        }

        private string _PropertyName;
        public string PropertyName
        {
            get { return _PropertyName; }
            set
            {
                if (value != _PropertyName)
                {
                    _PropertyName = value;
                }
            }
        }

        private object _Value;
        public object Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                }
            }
        }
    }

    [Serializable]
    [ProtoContract]
    public class MapHeaderObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public MapHeaderObject()
        {
            //MakeProberMapPropertyDictionay();
        }

        [ProtoIgnore]
        private bool IsDictionaryAssigned { get; set; }

        [ProtoIgnore]

        private Dictionary<EnumProberMapProperty, object> _PropertyDictionary = new Dictionary<EnumProberMapProperty, object>();
        public Dictionary<EnumProberMapProperty, object> PropertyDictionary
        {
            get
            {
                return _PropertyDictionary;
            }
            set
            {
                if (value != _PropertyDictionary)
                {
                    _PropertyDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ProtoIgnore]

        private List<ProberMapSection> _ProberMapSections = new List<ProberMapSection>();
        public List<ProberMapSection> ProberMapSections
        {
            get
            {
                return _ProberMapSections;
            }
            set
            {
                if (value != _ProberMapSections)
                {
                    _ProberMapSections = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// INPUT  : EnumProberMapProperty
        /// OUTPUT : EnumProberMapSection
        /// 특정 SECTION을 찾기 위함.
        /// </summary>
        [ProtoIgnore]

        private Dictionary<EnumProberMapSection, List<ProberMapProperty>> _SectionDictionary = new Dictionary<EnumProberMapSection, List<ProberMapProperty>>();
        public Dictionary<EnumProberMapSection, List<ProberMapProperty>> SectionDictionary
        {
            get
            {
                return _SectionDictionary;
            }
            set
            {
                if (value != _SectionDictionary)
                {
                    _SectionDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void GetPropertiesToDictionary(EnumProberMapSection section, object obj)
        {
            try
            {
                EnumProberMapProperty key = default(EnumProberMapProperty);
                string propname = string.Empty;

                object value = null;

                if (obj != null)
                {
                    Type type = obj.GetType();

                    System.Reflection.PropertyInfo[] propertyInfos = type.GetProperties();

                    foreach (var prop in propertyInfos)
                    {
                        if (Attribute.IsDefined(prop, typeof(ProberMapPropertyAttribute)))
                        {
                            var att = prop.GetCustomAttributes(typeof(ProberMapPropertyAttribute), false).FirstOrDefault();

                            if (att != null)
                            {
                                key = (att as ProberMapPropertyAttribute).ProberProperty;
                                propname = (att as ProberMapPropertyAttribute).PropertyName;
                            }

                            value = prop.GetValue(obj, null);

                            if (key != default(EnumProberMapProperty) && value != null)
                            {
                                PropertyDictionary.Add(key, value);

                                // TODO : SectionDictionary
                                bool IsContain = SectionDictionary.ContainsKey(section);

                                ProberMapProperty tmp = new ProberMapProperty();

                                tmp.ProberMapPropertyEnum = key;
                                tmp.PropertyName = propname;

                                if (IsContain == false)
                                {
                                    List<ProberMapProperty> newlist = new List<ProberMapProperty>();
                                    newlist.Add(tmp);

                                    SectionDictionary.Add(section, newlist);
                                }
                                else
                                {
                                    List<ProberMapProperty> existinglist;
                                    bool IsValid = SectionDictionary.TryGetValue(section, out existinglist);

                                    if (IsValid == true && existinglist != null)
                                    {
                                        existinglist.Add(tmp);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum MakeProberMapPropertyDictionay()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsDictionaryAssigned == false)
                {
                    if (PropertyDictionary == null)
                    {
                        PropertyDictionary = new Dictionary<EnumProberMapProperty, object>();
                    }
                    else
                    {
                        PropertyDictionary.Clear();
                    }

                    if(ProberMapSections != null)
                    {
                        ProberMapSections.Clear();
                    }

                    Type type = this.GetType();
                    EnumProberMapSection section = default(EnumProberMapSection);
                    object value = null;

                    System.Reflection.PropertyInfo[] propertyInfos = type.GetProperties();

                    foreach (var prop in propertyInfos)
                    {
                        if (Attribute.IsDefined(prop, typeof(ProberMapSectionAttribute)))
                        {
                            var att = prop.GetCustomAttributes(typeof(ProberMapSectionAttribute), false).FirstOrDefault();

                            if (att != null)
                            {
                                section = (att as ProberMapSectionAttribute).Section;
                            }

                            value = prop.GetValue(this, null);

                            ProberMapSections.Add(new ProberMapSection(section, value));
                        }
                    }

                    foreach (var item in ProberMapSections)
                    {
                        GetPropertiesToDictionary(item.Section, item.Value);
                    }

                    IsDictionaryAssigned = true;

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private object GetObject(EnumProberMapSection section)
        {
            object retval = null;

            try
            {
                retval = ProberMapSections.FirstOrDefault(x => x.Section == section).Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private Tuple<object, string> GetTargetSectionAndProertyName(EnumProberMapProperty key)
        {
            Tuple<object, string> retval = null;

            try
            {
                object foundSection = null;
                string propname = string.Empty;

                foreach (KeyValuePair<EnumProberMapSection, List<ProberMapProperty>> item in SectionDictionary)
                {
                    var propobj = item.Value.FirstOrDefault(x => x.ProberMapPropertyEnum == key);

                    if(propobj != null)
                    {
                        foundSection = GetObject(item.Key);
                        propname = propobj.PropertyName;
                        break;
                    }
                }

                if (foundSection != null && string.IsNullOrEmpty(propname) == false)
                {
                    retval = new Tuple<object, string>(foundSection, propname);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum SetProperty(EnumProberMapProperty key, object value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var s = GetTargetSectionAndProertyName(key);

                if (s != null)
                {
                    retval = PropertyExtension.SetChildPropertyValue(s.Item1, s.Item2, value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private MapHeaderExtensionBase _ExtensionData;
        [ProtoIgnore]
        [ProberMapSectionAttribute(EnumProberMapSection.EXTENSION)]
        public MapHeaderExtensionBase ExtensionData
        {
            get { return _ExtensionData; }
            set
            {
                if(_ExtensionData != value)
                {
                    _ExtensionData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BasicData _BasicData = new BasicData();
        [ProtoMember(1)]
        [ProberMapSectionAttribute(EnumProberMapSection.BASICDATA)]
        public BasicData BasicData
        {
            get { return _BasicData; }
            set
            {
                if (_BasicData != value)
                {
                    _BasicData = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CassetteData _CassetteData = new CassetteData();
        [ProtoMember(2)]
        [ProberMapSectionAttribute(EnumProberMapSection.CASSETTEDATA)]
        public CassetteData CassetteData
        {
            get { return _CassetteData; }
            set
            {
                if (_CassetteData != value)
                {
                    _CassetteData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotData _LotData = new LotData();
        [ProtoMember(3)]
        [ProberMapSectionAttribute(EnumProberMapSection.LOTDATA)]
        public LotData LotData
        {
            get { return _LotData; }
            set
            {
                if (_LotData != value)
                {
                    _LotData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferData _WaferData = new WaferData();
        [ProtoMember(4)]
        [ProberMapSectionAttribute(EnumProberMapSection.WAFERDATA)]
        public WaferData WaferData
        {
            get { return _WaferData; }
            set
            {
                if (_WaferData != value)
                {
                    _WaferData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferAlignData _WaferAlignData = new WaferAlignData();
        [ProtoMember(5)]
        [ProberMapSectionAttribute(EnumProberMapSection.WAFERALIGNDATA)]
        public WaferAlignData WaferAlignData
        {
            get { return _WaferAlignData; }
            set
            {
                if (_WaferAlignData != value)
                {
                    _WaferAlignData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MapConfigData _MapConfigData = new MapConfigData();
        [ProtoMember(6)]
        [ProberMapSectionAttribute(EnumProberMapSection.MAPCONFIGDATA)]
        public MapConfigData MapConfigData
        {
            get { return _MapConfigData; }
            set
            {
                if (_MapConfigData != value)
                {
                    _MapConfigData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TestResultData _TestResultData = new TestResultData();
        [ProtoMember(7)]
        [ProberMapSectionAttribute(EnumProberMapSection.TESTRESULTDATA)]
        public TestResultData TestResultData
        {
            get { return _TestResultData; }
            set
            {
                if (_TestResultData != value)
                {
                    _TestResultData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TimeData _TimeData = new TimeData();
        [ProtoMember(8)]
        [ProberMapSectionAttribute(EnumProberMapSection.TIMEDATA)]
        public TimeData TimeData
        {
            get { return _TimeData; }
            set
            {
                if (_TimeData != value)
                {
                    _TimeData = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
