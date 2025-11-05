using System;
using System.Collections.Generic;
using System.Linq;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces.State;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using ProberInterfaces.PnpSetup;
using System.Threading.Tasks;
using ProberInterfaces.ParamUtil;

namespace ProberInterfaces
{
    //==> Deserialize : XML 데이터로 부터 데이터 읽어 들여서 초기화
    //==> Serialize : XML에 데이터 Write
    //==> DefaultSetting : Excel로 부터 데이터 읽어 들여서 객체 초기화 및 생성
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Property | System.AttributeTargets.Interface)]
    public class ParamIgnore : System.Attribute
    {
        public ParamIgnore()
        {
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Property | System.AttributeTargets.Interface)]
    public class EditIgnore : System.Attribute
    {
        public EditIgnore()
        {
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Property | System.AttributeTargets.Interface)]
    public class SharePropPath : System.Attribute
    {
        public SharePropPath()
        {
        }
    }

    [System.AttributeUsage(System.AttributeTargets.All,
                    AllowMultiple = false,
                    Inherited = true)]
    public class NoShowingParamEdit : System.Attribute
    {
        public NoShowingParamEdit() { }
    }
    public interface INode
    { }
    public interface IParamNode : INode
    {
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        List<object> Nodes { get; set; }
    }
    public static class GenericCopyMaker<T>
    {
        public static T GetDeepCopy(object objectToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, objectToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }

    public interface IRecipeEditorSelectAble
    {
        Element<String> Label { get; set; }
    }

    public delegate void ValueChangedEventHandler(Object oldValue, Object newValue, object valueChangedParam = null);
    public delegate EventCodeEnum CheckSetValueAvailableEventHandler(string propertypath, IElement element, Object val, out string errorlog);//, IModule source = null);
    public delegate EventCodeEnum BeforeValueChangedEventHandler(string propertypath, IElement element, Object val, out string errorlog, bool isNeedValidation = false);//, IModule source = null);
    public delegate EventCodeEnum AfterValueChangedEventHandler(string propertypath, IElement element, Object oldVal, Object val, out string errorlog, bool ecvchangedevent = true);
    public delegate string ConvertToOriginTypeEventHandler(object val);
    public delegate string ConvertToReportTypeEventHandler(object val);



    public interface IElement : INotifyPropertyChanged, INode
    {
        event CheckSetValueAvailableEventHandler CheckSpecificSetValueAvailableEvent;
        event BeforeValueChangedEventHandler BeforeSpecificValueChangedEvent;
        event AfterValueChangedEventHandler AfterSpecificValueChangedEvent;
        event ConvertToOriginTypeEventHandler ConvertToOriginTypeEvent;
        event ConvertToReportTypeEventHandler ConvertToReportTypeEvent;

        String PropertyPath { get; set; }
        int ElementID { get; set; }
        String ElementName { get; set; }
        String ElementAdmin { get; set; }
        String AssociateElementID { get; set; }
        string ValueTypeDesc { get; set; }
        List<string> AssociateElementIDList { get; set; }
        String CategoryID { get; set; }
        List<int> CategoryIDList { get; set; }
        String Unit { get; set; }
        
        double LowerLimit { get; set; }
        double UpperLimit { get; set; }
        int ReadMaskingLevel { get; set; }
        int WriteMaskingLevel { get; set; }
        String Description { get; set; }
        int VID { get; set; }
        bool IsChanged { get; set; }
        String OriginPropertyPath { get; set; }
        String ReportPropertyPath { get; set; }
      
        Object Owner { get; set; }
        bool ApplyAltValue { get; }
        Object GetValue();
        Object GetOriginValue();
        //bool SetValue(Object val);
        EventCodeEnum SetValue(Object val, bool isNeedValidation = false, bool isEqualsValue = true, object param = null);//, IModule source = null);
        void SetOriginValue();
        void SetAltValue(object altVal);
        Type ValueType { get; set; }
        ElementStateEnum DoneState { get; set; }
        ElementSaveStateEnum SaveState { get; set; }
        bool GEMImmediatelyUpdate { get; set; }
        bool RaisePropertyChangedFalg { get; set; }
        bool GEMEnable { get; set; }
        void SetDefaultModifyState(IElement element);
        void SaveElement();
        void SaveElement(bool callerParameterizable = false, bool callowner = true);
        event ValueChangedEventHandler ValueChangedEvent;
        bool ExistAssociateNotNeedSetupState();
        void Setup();
        void CopyTo(ElementPack pack);
        EventCodeEnum CheckSetValueAvailable(string propertypath, object val, out string errorlog);//, IModule source = null);
        EventCodeEnum BeforeValueChangedBehavior(string propertypath, IElement element, Object val, out string errorlog, bool isNeedValidation = false);//, IModule source = null);


        EventCodeEnum AfterValueChangedBehavior(string propertypath, IElement element, object oldval, object val, out string errorlog, object valueChangedParam = null);
        //EventCodeEnum SetValueAndUpdateAssosiate(string propertypath, object val);

        string ConvertToOriginType(object val);
        string ConvertToReportType(object val);
    }
    [Serializable]
    public class ElementPack : IElement, INotifyPropertyChanged
    {
#pragma warning disable 0067
        public event CheckSetValueAvailableEventHandler CheckSpecificSetValueAvailableEvent;
        public event BeforeValueChangedEventHandler BeforeSpecificValueChangedEvent;
        public event AfterValueChangedEventHandler AfterSpecificValueChangedEvent;
        public event ConvertToOriginTypeEventHandler ConvertToOriginTypeEvent;
        public event ConvertToReportTypeEventHandler ConvertToReportTypeEvent;
#pragma warning restore 0067

        #region ==> PropertyChangedEventHandler
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //[XmlIgnore, JsonIgnore]
        //public string Genealogy { get; set; }

        #region // Supported properties
        #region ==> ElementID
        private int _ElementID;
        public int ElementID
        {
            get { return _ElementID; }
            set { _ElementID = value; }
        }
        #endregion

        #region ==> ElementName
        private String _ElementName;
        public String ElementName
        {
            get { return _ElementName; }
            set { _ElementName = value; }
        }
        #endregion

        #region ==> ElementAdmin
        private String _ElementAdmin;
        public String ElementAdmin
        {
            get { return _ElementAdmin; }
            set { _ElementAdmin = value; }
        }
        #endregion

        #region ==> AssociateElementID
        private String _AssociateElementID;
        public String AssociateElementID
        {
            get { return _AssociateElementID; }
            set { _AssociateElementID = value; }
        }

        public List<string> AssociateElementIDList { get; set; } = new List<string>();
        #endregion

        #region ==> CategoryID
        private String _CategoryID;
        public String CategoryID
        {
            get { return _CategoryID; }
            set { _CategoryID = value; }
        }

        public List<int> CategoryIDList { get; set; }
        #endregion

        #region ==> Unit
        private String _Unit;
        public String Unit
        {
            get { return _Unit; }
            set { _Unit = value; }
        }
        #endregion

        #region ==> LowerLimit
        private double _LowerLimit;
        public double LowerLimit
        {
            get { return _LowerLimit; }
            set { _LowerLimit = value; }
        }
        #endregion

        #region ==> UpperLimit
        private double _UpperLimit;
        public double UpperLimit
        {
            get { return _UpperLimit; }
            set { _UpperLimit = value; }
        }
        #endregion

        #region ==> ReadMaskingLevel
        private int _ReadMaskingLevel;
        public int ReadMaskingLevel
        {
            get { return _ReadMaskingLevel; }
            set { _ReadMaskingLevel = value; }
        }
        #endregion

        #region ==> WriteMaskingLevel
        private int _WriteMaskingLevel;
        public int WriteMaskingLevel
        {
            get { return _WriteMaskingLevel; }
            set { _WriteMaskingLevel = value; }
        }
        #endregion

        #region ==> Description
        private String _Description;
        public String Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        #endregion

        #region ==> VID
        private int _VID;
        public int VID
        {
            get { return _VID; }
            set
            {
                if (value != _VID)
                {
                    _VID = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> DoneState
        private ElementStateEnum _DoneState;
        public ElementStateEnum DoneState
        {
            get { return _DoneState; }
            set
            {
                if (value != _DoneState)
                {
                    _DoneState = value;
                }
            }
        }
        #endregion

        private bool _ApplyAltValue;

        public bool ApplyAltValue
        {
            get { return _ApplyAltValue; }
            private set { _ApplyAltValue = value; }
        }

        //이전 값과 같아도 NotifyChangedEvent 를 날리고싶을때. true
        private bool _RaisePropertyChangedFalg = false;
        [JsonIgnore, ParamIgnore]
        public bool RaisePropertyChangedFalg
        {
            get { return _RaisePropertyChangedFalg; }
            set { _RaisePropertyChangedFalg = value; }
        }

        private ElementSaveStateEnum _SaveState
             = ElementSaveStateEnum.SAVEED;
        public ElementSaveStateEnum SaveState
        {
            get { return _SaveState; }
            set
            {
                if (value != _SaveState)
                {
                    _SaveState = value;
                }
            }
        }

        private List<IElement> _AssociateElements
             = new List<IElement>();
        public List<IElement> AssociateElements
        {
            get
            {
                if (_AssociateElements == null)
                    _AssociateElements = new List<IElement>();
                return _AssociateElements;
            }
            set
            {
                if (value != _AssociateElements)
                {
                    _AssociateElements = value;
                }
            }
        }

        [NonSerialized]
        private bool _GEMImmediatelyUpdate;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public bool GEMImmediatelyUpdate
        {
            get { return _GEMImmediatelyUpdate; }
            set
            {
                if (value != _GEMImmediatelyUpdate)
                {
                    _GEMImmediatelyUpdate = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _GEMEnable;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public bool GEMEnable
        {
            get { return _GEMEnable; }
            set
            {
                if (value != _GEMEnable)
                {
                    _GEMEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Object _Owner;
        [JsonIgnore]
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

        //public Type ValueType { get; set; }
        private Type _ValueType;

        public Type ValueType
        {
            get { return _ValueType; }
            set { _ValueType = value; }
        }
        private string _ValueTypeDesc;

        public string ValueTypeDesc
        {
            get { return _ValueTypeDesc; }
            set { _ValueTypeDesc = value; }
        }


        #region ==> PropertyPath
        private String _PropertyPath;
        public String PropertyPath
        {
            get { return _PropertyPath; }
            set { _PropertyPath = value; }
        }
        #endregion

        [NonSerialized]
        private bool _IsChanged;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public bool IsChanged
        {
            get { return _IsChanged; }
            set
            {
                if (_IsChanged != value)
                {
                    _IsChanged = value;
                }
            }
        }
#pragma warning disable 0067
        public event ValueChangedEventHandler ValueChangedEvent;
#pragma warning restore 0067 
        private object _Value;
        public object Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        string _OriginPropertyPath = "";
        public string OriginPropertyPath
        {
            get { return GetOriginPropertyPath(); }
            set
            {
                if (this.PropertyPath != value)
                {
                    LoggerManager.Debug($"{this.PropertyPath} Set _OriginPropertyPath:{value}");
                }
                
                _OriginPropertyPath = value;
            }
        }


        public string GetOriginPropertyPath()
        {
            string retVal = "";
            if (_OriginPropertyPath is "")
            {
                retVal = this.PropertyPath;
            }
            else
            {
                retVal = _OriginPropertyPath;
            }
            return retVal;
        }


        string _ReportPropertyPath = "";
        public string ReportPropertyPath
        {
            get { return GetReportPropertyPath(); }
            set
            {
                if (_ReportPropertyPath != "" && _ReportPropertyPath != PropertyPath)
                {
                    LoggerManager.Debug($"{this.PropertyPath} Set OriginPropertyPath:{value}");
                }
                _OriginPropertyPath = value;
            }
        }


        public string GetReportPropertyPath()
        {
            string retVal = "";
            if (_ReportPropertyPath is "")
            {
                retVal = this.PropertyPath;
            }
            else
            {
                retVal = _ReportPropertyPath;
            }
            return retVal;
        }

     

        public object GetValue()
        {
            return Value;
        }


        public string ConvertToOriginType(object val)
        {
            string retVal = val.ToString();
            if (ConvertToOriginTypeEvent != null)
            {
                retVal = ConvertToOriginTypeEvent(val);
            }
            return retVal;
        }

        public string ConvertToReportType(object val)
        {
            string retVal = val.ToString();
            if (ConvertToReportTypeEvent != null)
            {
                retVal = ConvertToReportTypeEvent(val);
            }
            return retVal;
        }

        public Object GetOriginValue()
        {
            return Value;
        }

        public EventCodeEnum SetValue(object val, bool isNeedValidation = false, bool isEqualsValue = true, object param = null)//, IModule source = null)
        {
            Value = val;
            return EventCodeEnum.NONE;
        }

        public void SetOriginValue()
        {            
        }
        public void SetAltValue(object altVal)
        {
        }
        public void UpdateValue()
        {
        }
        public void SetDefaultModifyState(IElement element)
        {            
        }

        public void SaveElement()
        {            
        }

        public void SaveElement(bool callerParameterizable = false, bool callowner = true)
        {
        }

        public bool ExistAssociateNotNeedSetupState()
        {
            return false;
        }

        public void Setup()
        {            
        }

        public void CopyTo(ElementPack pack)
        {            
        }

        public EventCodeEnum CheckSetValueAvailable(string propertypath, Object val, out string errorlog)//, IModule source = null)
        {
            errorlog = "";
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum BeforeValueChangedBehavior(string propertypath, IElement element,  Object val, out string errorlog, bool isNeedValidation = false)//, IModule source = null)
        {
            errorlog = "";
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum AfterValueChangedBehavior(string propertypath, IElement element, Object oldVal, Object val, out string errorlog, object valueChangedParam = null)
        {
            errorlog = "";
            return EventCodeEnum.UNDEFINED;
        }

        //public EventCodeEnum SetValueAndUpdateAssosiate(string propertypath, Object val)
        //{            
        //    return EventCodeEnum.UNDEFINED;
        //}

        #endregion
    }
    [Serializable]
    public class ElementPacks
    {
        public ElementPacks()
        {
            Elements = new List<ElementPack>();
        }
        private List<ElementPack> _Elements;

        public List<ElementPack> Elements
        {
            get { return _Elements; }
            set { _Elements = value; }
        }
    }


    [Serializable, DataContract]
    public class Element<T> : IElement, IFactoryModule
    {
        public event CheckSetValueAvailableEventHandler CheckSpecificSetValueAvailableEvent;
#pragma warning disable 0067
        public event BeforeValueChangedEventHandler BeforeSpecificValueChangedEvent;
        public event AfterValueChangedEventHandler AfterSpecificValueChangedEvent;
#pragma warning restore 0067
        public event ConvertToOriginTypeEventHandler ConvertToOriginTypeEvent;
        public event ConvertToReportTypeEventHandler ConvertToReportTypeEvent;

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }

        #region ==> PropertyChangedEventHandler
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private T _Value;
        [DataMember]
        public T Value
        {
            get
            {
                if (ApplyAltValue == true)
                {
                    return _AltValue;
                }

                return _Value;
            }
            set
            {
                if (_Value != null)
                {
                    EventCodeEnum IsChanged = SetValue(value);

                    //if (IsChanged == true | RaisePropertyChangedFalg)
                    if (IsChanged == EventCodeEnum.NONE | RaisePropertyChangedFalg)
                    {
                        //SaveState = ElementSaveStateEnum.MODIFYED;
                        UpdateAssociateElementList();
                        UpdateOwnerIsChanged();
                        RaisePropertyChanged();
                    }

                    //if (!_Value.Equals(value))
                    //{
                    //    _Value = value;

                    //    UpdateAssociateElements();
                    //    UpdateOwnerIsChanged();
                    //    RaisePropertyChanged();
                    //}
                }
                else
                {
                    _Value = value;

                    UpdateAssociateElementList();// property-set에서 하위 Element를 업데이트 해주는 구조가 있음.
                    UpdateOwnerIsChanged();
                    RaisePropertyChanged();
                }
            }
        }

        //이전 값과 같아도 NotifyChangedEvent 를 날리고싶을때. true
        private bool _RaisePropertyChangedFalg = false;
        [JsonIgnore, ParamIgnore]
        public bool RaisePropertyChangedFalg
        {
            get { return _RaisePropertyChangedFalg; }
            set { _RaisePropertyChangedFalg = value; }
        }

        //private T _OriginElement = default(T);
        //[JsonIgnore, ParamIgnore]
        //public T OriginElement
        //{
        //    get { return _OriginElement; }
        //    set
        //    {
        //        _OriginElement = value;
        //        RaisePropertyChanged();
        //    }
        //}


        private T _OriginalValue = default(T);
        [JsonIgnore, ParamIgnore]
        public T OriginalValue
        {
            get { return _OriginalValue; }
            set
            {
                _OriginalValue = value;
                RaisePropertyChanged();
            }
        }

        #region ==> PropertyPath        
        private String _PropertyPath;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String PropertyPath
        {
            get { return _PropertyPath; }
            set { _PropertyPath = value; }
        }
        #endregion

        #region ==> ElementID        
        private int _ElementID;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public int ElementID
        {
            get { return _ElementID; }
            set { _ElementID = value; }
        }
        #endregion

        #region ==> ElementName        
        private String _ElementName;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String ElementName
        {
            get { return _ElementName; }
            set { _ElementName = value; }
        }
        #endregion

        #region ==> ElementAdmin        
        private String _ElementAdmin;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String ElementAdmin
        {
            get { return _ElementAdmin; }
            set { _ElementAdmin = value; }
        }
        #endregion

        #region ==> AssociateElementID       
        private String _AssociateElementID;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String AssociateElementID
        {
            get { return _AssociateElementID; }
            set { _AssociateElementID = value; }
        }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        //public List<int> AssociateElementIDList { get; set; }
        public List<string> AssociateElementIDList { get; set; } = new List<string>();
        #endregion

        #region ==> CategoryID        
        private String _CategoryID;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String CategoryID
        {
            get { return _CategoryID; }
            set { _CategoryID = value; }
        }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<int> CategoryIDList { get; set; }
        #endregion

        #region ==> Unit        
        private String _Unit;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String Unit
        {
            get { return _Unit; }
            set { _Unit = value; }
        }
        #endregion

        #region ==> LowerLimit        
        private double _LowerLimit;
        [XmlIgnore, JsonIgnore]
        public double LowerLimit
        {
            get { return _LowerLimit; }
            set { _LowerLimit = value; }
        }
        #endregion

        #region ==> UpperLimit        
        private double _UpperLimit;
        [XmlIgnore, JsonIgnore]
        public double UpperLimit
        {
            get { return _UpperLimit; }
            set { _UpperLimit = value; }
        }
        #endregion

        #region ==> ReadMaskingLevel        
        private int _ReadMaskingLevel;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public int ReadMaskingLevel
        {
            get { return _ReadMaskingLevel; }
            set { _ReadMaskingLevel = value; }
        }
        #endregion

        #region ==> WriteMaskingLevel        
        private int _WriteMaskingLevel;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public int WriteMaskingLevel
        {
            get { return _WriteMaskingLevel; }
            set { _WriteMaskingLevel = value; }
        }
        #endregion

        #region ==> Description        
        private String _Description;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        #endregion

        #region ==> VID
        private int _VID;
        public int VID
        {
            get { return _VID; }
            set
            {
                if (value != _VID)
                {
                    _VID = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> DoneState
        private ElementStateEnum _DoneState;
        public ElementStateEnum DoneState
        {
            get { return _DoneState; }
            set
            {
                if (value != _DoneState | ExistAssociateNotNeedSetupState())
                {
                    _DoneState = value;
                    RaisePropertyChanged();
                    UpdateNeedSetupState();
                }
            }
        }
        #endregion

        private ElementSaveStateEnum _SaveState
             = ElementSaveStateEnum.SAVEED;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public ElementSaveStateEnum SaveState
        {
            get { return _SaveState; }
            set
            {
                if (value != _SaveState)
                {
                    _SaveState = value;
                    //RaisePropertyChanged();
                }
            }
        }

        private List<IElement> _AssociateElements
             = new List<IElement>();
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<IElement> AssociateElements
        {
            get
            {
                if (_AssociateElements == null)
                    _AssociateElements = new List<IElement>();
                return _AssociateElements;
            }
            set
            {
                if (value != _AssociateElements)
                {
                    _AssociateElements = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _GEMImmediatelyUpdate;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        ///GP GEM 연결으로 Commander 에 각 Stage 별 Buffer 개념이 생겼고 Stage 는 값이 변경되었을때
        ///바로 Gem 동글에 업데이트 하지않고 버퍼에 없데이트한다. 이때 버퍼가 아닌 바로 업데이트가 
        ///필요한 값들에 대해 설정하기위해. 즉 true : element 값 변경시 바로 gem 동글 업데이트 | false : 버퍼에 업데이트
        public bool GEMImmediatelyUpdate
        {
            get { return _GEMImmediatelyUpdate; }
            set
            {
                if (value != _GEMImmediatelyUpdate)
                {
                    _GEMImmediatelyUpdate = value;
                }
            }
        }

        [NonSerialized]
        private bool _GEMEnable;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        ///같은 Element 이지만, 고객사 마다 사용하는 Element Data 일수도 있고, 아닐수도 있음.
        ///불필요한 업데이트를 하지 않기 위해 GimVid Parameter 로부터 값을 얻어와서 사용하기 위한 프로퍼티 
        public bool GEMEnable
        {
            get { return _GEMEnable; }
            set
            {
                if (value != _GEMEnable)
                {
                    _GEMEnable = value;
                }
            }
        }

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

        //[XmlIgnore, JsonIgnore, ParamIgnore]
        //public string Genealogy { get; set; }
        //[XmlIgnore, JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }

        private Type _ValueType;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public Type ValueType
        {
            get
            {
                return _ValueType;
            }
            set
            {
                if (_ValueType != value)
                {
                    _ValueType = value;
                    ValueTypeDesc = _ValueType.ToString();
                }
            }
        }
        private string _ValueTypeDesc;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string ValueTypeDesc
        {
            get { return _ValueTypeDesc; }
            set { _ValueTypeDesc = value; }
        }

        [NonSerialized]
        private bool _IsChanged = false;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public bool IsChanged
        {
            get { return _IsChanged; }
            set
            {
                if (_IsChanged != value)
                {
                    _IsChanged = value;
                }
            }
        }

        #region ==> AltValue  

        private bool _ApplyAltValue;

        public bool ApplyAltValue
        {
            get { return _ApplyAltValue; }
            private set { _ApplyAltValue = value; }
        }


        [NonSerialized]
        private T _AltValue = default(T);
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public T AltValue
        {
            get { return _AltValue; }
            private set { _AltValue = value; }
        }
        #endregion

        public event ValueChangedEventHandler ValueChangedEvent;


        public EventCodeEnum SetValue(Object val, bool isNeedValidation = false, bool isEqualsValue = true, object param = null)//, IModule source = null)
        {
            string errormsg = "";
            if (val == null)
                return EventCodeEnum.UNDEFINED;

            if (isEqualsValue && Object.Equals(GetValue(), val))
                return EventCodeEnum.PARAM_SET_EQUAL_VALUE;

            T localVal = Value;
            bool result = GetGenericValue(val, ref localVal);
            if (result == false)
                return EventCodeEnum.UNDEFINED;

            EventCodeEnum beforeBeh = this.BeforeValueChangedBehavior(this.PropertyPath, this, val, out errormsg, isNeedValidation: isNeedValidation);//, source: source);
            if (beforeBeh != EventCodeEnum.NONE)
            {
                return beforeBeh;//Set이 되면 안됨.
            }

            T oldValue = Value;
            T newValue = localVal;

            _Value = localVal;


            EventCodeEnum afterBeh = this.AfterValueChangedBehavior(this.PropertyPath, this, oldValue, val, out errormsg, param);
            if (afterBeh != EventCodeEnum.NONE)
            {
                return afterBeh;
            }

            IsChanged = true;

            return EventCodeEnum.NONE;
        }
        public void SetOriginValue()
        {
            OriginalValue = Value;
        }


        public void SetAltValue(object altVal)
        {
            try
            {
                string validateErrorLog = "";
                if (ParamBaseValidate(PropertyPath, altVal, out validateErrorLog) == EventCodeEnum.NONE)
                {
                    T value = _Value;
                    bool result = GetGenericValue(altVal, ref value);
                    if (result)
                    {
                        //<!-- 적용 전에 Min/Max 확인 -->

                        _AltValue = value;
                        ApplyAltValue = true;
                        LoggerManager.Debug($"SetAltValue() Success. PropertyPath : {this.PropertyPath}, AltValue : {_AltValue}, Value : {_Value}.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"SetAltValue() Parameter Validation Check Fail. PropertyPath : {this.PropertyPath}, AltValue : {_AltValue}, Value : {_Value}. ErrorLog : {validateErrorLog}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public bool CompareValueChanged()
        {
            try
            {
                if (Object.Equals(OriginalValue, Value))
                    return false;
                else
                    return true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return false;
        }
        private bool GetGenericValue(Object setValue, ref T genericValue)
        {
            //==> List나 객체 타입은 Value를 Reflection으로 따로 처리 해야 할 듯
            bool result = false;
            do
            {
                //==> [1] Casting
                if (setValue is T)
                {
                    genericValue = (T)setValue;
                    result = true;
                    break;
                }

                //==> [2] Enum Convert

                // TODO : 현재 Int 값의 String 형태로 입력이 들어오는 경우, Retrun false

                if(ValueType == null)
                {
                    ValueType = typeof(T);
                }

                if (ValueType.IsEnum)
                {
                    string[] str = System.Enum.GetNames(ValueType);

                    if (str.Contains(setValue.ToString()) == false)
                        break;

                    genericValue = (T)System.Enum.Parse(ValueType, setValue.ToString());
                    result = true;
                    break;
                }
                //==> [3] Convert
                TypeConverter converter = TypeDescriptor.GetConverter(setValue);
                if (converter.CanConvertTo(ValueType))
                {
                    genericValue = (T)converter.ConvertTo(setValue, ValueType);
                    result = true;
                    break;

                }
                //==> [4] Change Type
                try
                {
                    genericValue = (T)Convert.ChangeType(setValue, ValueType);
                    result = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                    break;
                }
            } while (false);

            return result;
        }
        public Object GetValue()
        {
            return Value;
        }

        public string ConvertToOriginType(object val)
        {
            string retVal = val.ToString();
            if (ConvertToOriginTypeEvent != null)
            {
                retVal = ConvertToOriginTypeEvent(val);
            }
            return retVal;
        }

        public string ConvertToReportType(object val)
        {
            string retVal = val.ToString();
            if (ConvertToReportTypeEvent != null)
            {
                retVal = ConvertToReportTypeEvent(val);
            }
            return retVal;
        }

        public Object GetOriginValue()
        {
            return _OriginalValue;
        }

        private void UpdateOwnerIsChanged()
        {
            if (Owner != null)
            {
                bool IsOwnerNull = false;
                Object ParanetOwner = Owner;

                while (!IsOwnerNull)
                {
                    if (ParanetOwner is IParam)
                    {
                        (ParanetOwner as IParam).IsParamChanged = true;
                        break;
                    }
                    else
                    {
                        if (ParanetOwner is IParamNode)
                        {
                            ParanetOwner = (ParanetOwner as IParamNode).Owner;
                        }
                        else
                        {
                            IsOwnerNull = true;
                        }
                    }
                }
            }
        }

        private void UpdateNeedSetupState()
        {
            SaveState = ElementSaveStateEnum.MODIFYED;
            if (!Extensions_IParam.LoadProgramFlag)
                return;
            if (DoneState == ElementStateEnum.NEEDSETUP)
            {
                UpdateAssociateElementList();
                ApplyAssociateElement();
            }
        }

        private void ApplyAssociateElement(bool callerParameterizable = false, bool callowner = true)
        {
            try
            {
                //if (SaveState != ElementSaveStateEnum.SAVEED)
                //    SaveState = ElementSaveStateEnum.MODIFYED;

                if (SaveState != ElementSaveStateEnum.SAVEED)
                {
                    if (callowner)
                    {   //자신의 SaveState변경.
                        //SetupState = ElementStateEnum.SETUPED;
                        SaveState = ElementSaveStateEnum.SAVEED;
                    }

                    if (!callerParameterizable)
                    {

                        //ParamManager - SaveChangedElementList 함수 참고.
                        Object owner = Owner;
                        bool isParamNodeGet = false;
                        IParam param = null;

                        while (owner != null)
                        {
                            if (owner is IParam)
                            {
                                param = owner as IParam;

                                isParamNodeGet = true;
                                break;
                            }
                            else if (owner is IParamNode)
                            {
                                IParamNode node = owner as IParamNode;
                                owner = node.Owner;
                            }
                            else
                            {
                                //LoggerManager.Debug(PropertyPath + "  node path wrong");
                                break;
                            }
                        }

                        if (isParamNodeGet == false)
                        {
                            //LoggerManager.Debug(PropertyPath + " get onwer fail");
                        }

                        //Check caller
                        if (param != null)
                        {
                            EventCodeEnum errrCode = EventCodeEnum.UNDEFINED;
                            errrCode = param.SaveParameterCallElement(param);
                        }
                    }
                }

                if (AssociateElements != null)
                {
                    if (DoneState != ElementStateEnum.NEEDSETUP & ExistAssociateNotNeedSetupState())
                    {
                        List<string> changedparam = new List<string>();

                        foreach (var element in AssociateElements.ToList())
                        {
                            if (element != null)
                            {
                                element.DoneState = ElementStateEnum.NEEDSETUP;
                                element.SaveState = ElementSaveStateEnum.MODIFYED;
                                changedparam.Add(element.ElementName);
                                element.SaveElement(false, false);
                                string description = changedparam.Aggregate((a, b) => a + ',' + b);

                                LoggerManager.Event
                                    ($"Parameter Change [{this.ElementID}]. Please check the changed parameters.",
                                    $"{description}");


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

        private void UpdateAssociateElementList()
        {
            if (AssociateElementIDList != null)
            {
                //==> TODO : Linq 로 바꿀 수 있음.
                //AssociateElements = new List<IElement>();

                AssociateElements.Clear();
                foreach (var id in AssociateElementIDList)
                {
                    IElement element = this.ParamManager().GetAssociateElement(id);
                    if (element != null)
                        AssociateElements.Add(element);
                    else
                    {

                    }
                }
            }
        }

        /// <summary>
        /// AssociateElements 중 DoneState가 Done 인 Element 가있으면 true. 아니면 false.
        /// </summary>
        /// <returns></returns>
        private bool CheckExistAssociateDoneState()
        {
            bool retVal = false;
            try
            {
                if (AssociateElements.FindAll(element => element.DoneState == ElementStateEnum.DONE)
                    .Count > 0)
                    retVal = true;
                else
                    retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        /// <summary>
        /// AssociateElements 중 NeedSetup이 아닌 Element가있다면 true, 없다면 false.
        /// </summary>
        /// <returns></returns>
        public bool ExistAssociateNotNeedSetupState()
        {
            bool retVal = false;
            try
            {
                if (AssociateElements.Count != 0)
                {
                    if (AssociateElements.FindAll(element => element.DoneState == ElementStateEnum.NEEDSETUP)
                        .Count != AssociateElements.Count)
                    {
                        retVal = true;
                        if (retVal)
                            return retVal;

                        foreach (var element in AssociateElements)
                        {
                            retVal = element.ExistAssociateNotNeedSetupState();

                            if (retVal)
                                break;
                        }
                    }
                    else
                    {
                        retVal = false;

                        foreach (var element in AssociateElements)
                        {
                            retVal = element.ExistAssociateNotNeedSetupState();

                            if (retVal)
                                break;
                        }
                    }
                }
                else
                {
                    if ((AssociateElementIDList != null) && (AssociateElementIDList.Count != AssociateElements.Count))
                    {
                        //foreach (var element in AssociateElements)
                        //{
                        List<IElement> associateelements = new List<IElement>();
                        foreach (var id in AssociateElementIDList)
                        {
                            IElement subelement = this.ParamManager().GetAssociateElement(id);
                            if (subelement != null)
                            {
                                associateelements.Add(subelement);
                                retVal = subelement.ExistAssociateNotNeedSetupState();

                                if (retVal)
                                    break;
                            }
                        }

                        if (!retVal)
                        {
                            if (associateelements.FindAll(aelement => aelement.DoneState == ElementStateEnum.NEEDSETUP)
                                   .Count != AssociateElements.Count)
                                retVal = true;
                            else
                                retVal = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        /// <summary>
        /// Element 개별 저장할시 (결국은 File전체 저장)
        /// </summary>
        public void SaveElement()
        {
            try
            {
                Object owner = Owner;
                IParam param = null;

                while (owner != null)
                {
                    if (owner is IParam)
                    {
                        param = owner as IParam;

                        break;
                    }
                    else if (owner is IParamNode)
                    {
                        IParamNode node = owner as IParamNode;
                        owner = node.Owner;
                    }
                    else
                    {
                        //LoggerManager.Debug(PropertyPath + "  node path wrong");
                        break;
                    }
                }

                if (param != null)
                {
                    Extensions_IParam.SaveElementParmeter(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// callerParameterizable 가 true라는 것은 IParameterizable 의 SavePraameter 함수로 부터 호출된 것이므로 SaveParameter 을 호출하지 않아야한다.
        /// 개별 Element 저장을 호출할시 callerParameterizable 가 false로 설정되어있어야 Save를 호출한다.
        /// </summary>
        /// <param name="callerParameterizable"></param>
        public void SaveElement(bool callerParameterizable = false, bool callowner = true)
        {
            try
            {
                //프로그램 뜨기 전
                if (!Extensions_IParam.LoadProgramFlag)
                    return;


                //값이 변경되지 않거나 Done State가 NEEDSETUP이 아닌경우 .
                if (!CompareValueChanged() & DoneState != ElementStateEnum.NEEDSETUP)
                {
                    if (AssociateElements != null)
                    {
                        if (AssociateElements.Count != 0)
                            AssociateElements.Clear();
                    }
                    SaveState = ElementSaveStateEnum.SAVEED;
                }
                else
                { //값이 변경된 경우
                  //if (SaveState != ElementSaveStateEnum.SAVEED)
                    SaveState = ElementSaveStateEnum.MODIFYED;
                    UpdateAssociateElementList();
                }


                if (SaveState == ElementSaveStateEnum.MODIFYED & AssociateElements.Count != 0)
                {
                    //if (AssociateElements.Count != 0)
                    if (DoneState != ElementStateEnum.NEEDSETUP | ExistAssociateNotNeedSetupState())
                    {
                        List<string> changedparam = new List<string>();

                        foreach (var element in AssociateElements.ToList())
                        {
                            if (element != null)
                            {
                                if (element.DoneState != ElementStateEnum.NEEDSETUP)
                                {
                                    element.DoneState = ElementStateEnum.NEEDSETUP;
                                    changedparam.Add(element.ElementName);
                                    string description = changedparam.Aggregate((a, b) => a + ',' + b);
                                    LoggerManager.Event($"Parameter Change [{element.ElementID}]. Please check the changed parameters.",
                                    $"{description}");
                                }
                                if (element.ExistAssociateNotNeedSetupState())
                                    element.SaveElement(false, false);
                            }
                        }
                    }
                }

                if (SaveState != ElementSaveStateEnum.SAVEED)
                {
                    //본인이 포함된 Param이나 Element.Save가 외부로 불렸을시에만 저장하기 위해.
                    //AssociateElement 로 저장된 것이라면 State를 변경하면 안되기 때문에.

                    if (callowner)
                    {   //자신의 SaveState변경.
                        //SetupState = ElementStateEnum.SETUPED;
                        SaveState = ElementSaveStateEnum.SAVEED;
                        //OriginValue = Value
                        SetOriginValue();
                    }

                    //ParamManager - SaveChangedElementList 함수 참고.
                    if (!callerParameterizable)
                    {
                        Object owner = Owner;
                        bool isParamNodeGet = false;
                        IParam param = null;

                        while (owner != null)
                        {
                            if (owner is IParam)
                            {
                                param = owner as IParam;

                                isParamNodeGet = true;
                                break;
                            }
                            else if (owner is IParamNode)
                            {
                                IParamNode node = owner as IParamNode;
                                owner = node.Owner;
                            }
                            else
                            {
                                //LoggerManager.Debug(PropertyPath + "  node path wrong");
                                break;
                            }
                        }

                        if (isParamNodeGet == false)
                        {
                            //LoggerManager.Debug(PropertyPath + " get onwer fail");
                        }

                        //Check caller
                        if (param != null)
                        {
                            EventCodeEnum errrCode = param.SaveParameterCallElement(param);
                        }
                    }
                }

                SetOriginValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EventCodeEnum ElementSave(IParamNode param, bool callerowner = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    var properties = GetPropertyTypes(param, typeof(IParamNode), typeof(IElement), typeof(IList));
                    foreach (var prop in properties)
                    {
                        var paramIgnore = prop.GetCustomAttribute(typeof(ParamIgnore));

                        if (paramIgnore == null)
                        {
                            var nodeInstance = prop.GetValue(param);

                            if (nodeInstance is IList)
                            {
                                var genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();

                                if (prop.GetCustomAttribute(typeof(SharePropPath)) == null)
                                {
                                    if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                                    {
                                        IList list = nodeInstance as IList;
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            var node = list[i] as IParamNode;

                                            retVal = ElementSave(node);
                                            if (retVal != EventCodeEnum.NONE)
                                                return retVal;
                                        }
                                    }
                                }
                                else
                                {
                                    if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                                    {
                                        IList list = nodeInstance as IList;
                                        if (list.Count > 0)
                                        {
                                            var node = list[0] as IParamNode;

                                            retVal = ElementSave(node);
                                            if (retVal != EventCodeEnum.NONE)
                                                return retVal;
                                        }
                                        else
                                            return EventCodeEnum.NONE;

                                    }
                                }
                                //retVal = ElementSave(nodeInstance as IParamNode);
                                //if (retVal != EventCodeEnum.NONE)
                                //return retVal;
                            }
                            else if (nodeInstance is IParamNode)
                            {
                                var node = nodeInstance as IParamNode;
                                retVal = ElementSave(node);
                                if (retVal != EventCodeEnum.NONE)
                                    return retVal;
                            }
                            else if (nodeInstance is IElement)
                            {
                                //if ((nodeInstance as IElement).SetupState == State.ElementStateEnum.DEFAULT)
                                //{
                                //    retVal = EventCodeEnum.PARAM_INSUFFICIENT;

                                //    return retVal;
                                //}

                                IElement element = nodeInstance as IElement;
                                if (element.DoneState == ElementStateEnum.DEFAULT)
                                {
                                    if (SetValue(Value) == EventCodeEnum.NONE)
                                    {
                                        element.DoneState = ElementStateEnum.DONE;
                                    }
                                    //if (element.GetValue() is IComparable)
                                    //{    //원본값과 현재값이 다르면
                                    //    if (((IComparable)(element.GetValue())).CompareTo(element.GetOriginValue()) != 0)
                                    //    {
                                    //        element.SetupState = ElementStateEnum.SETUPED;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    element.SetupState = ElementStateEnum.SETUPED;
                                    //}
                                }
                                else
                                {
                                    if (element.DoneState == ElementStateEnum.NEEDSETUP & callerowner)
                                        element.DoneState = ElementStateEnum.DONE;
                                }
                                element.SaveState = ElementSaveStateEnum.SAVEED;
                                element.SetOriginValue();
                            }
                        }
                    }

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        private static List<PropertyInfo> GetPropertyTypes(object instance, params Type[] interfaceTypes)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            try
            {
                var type = instance.GetType();
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    foreach (var interfaceType in interfaceTypes)
                    {
                        if (interfaceType.IsAssignableFrom(prop.PropertyType))
                        {
                            list.Add(prop);
                            break;
                        }
                        else if (prop.PropertyType.Name == "IList`1")
                        {
                            list.Add(prop);
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return list;
        }
        public void SetNodeSetupStateSetuped()
        {
            DoneState = ElementStateEnum.DONE;
        }
        public void SetDefaultModifyState(IElement element)
        {
            try
            {
                //ElementSetupStateBase state = element.SetupState as ElementSetupStateBase;
                //state = new ElementDefaultState(element);
                DoneState = ElementStateEnum.DEFAULT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static TConvertType DoConvert<TConvertType>(object convertValue, out bool hasConverted)
        {
            hasConverted = false;
            var converted = default(TConvertType);
            try
            {
                converted = (TConvertType)
                    Convert.ChangeType(convertValue, typeof(TConvertType));
                hasConverted = true;
            }
            catch (InvalidCastException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            return converted;
        }

        public Element(object value)
        {
            try
            {
                _PropertyPath = "EMPTY";
                _ElementID = -1;
                _ElementName = "EMPTY";
                _ElementAdmin = "EMPTY";
                _AssociateElementID = "EMPTY";
                _CategoryID = "EMPTY";
                _Unit = "EMPTY";
                _LowerLimit = 0.0;
                _UpperLimit = 0.0;
                _ReadMaskingLevel = 100;
                _WriteMaskingLevel = 100;
                _Description = "EMPTY";

                bool hasConverted;

                _Value = DoConvert<T>(value, out hasConverted);
                
                if(hasConverted == false)
                {
                    
                }

                DoneState = ElementStateEnum.DEFAULT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Element()
        {
            try
            {
                _PropertyPath = "EMPTY";
                _ElementID = -1;
                _ElementName = "EMPTY";
                _ElementAdmin = "EMPTY";
                _AssociateElementID = "EMPTY";
                _CategoryID = "EMPTY";
                _Unit = "EMPTY";
                _LowerLimit = 0.0;
                _UpperLimit = 0.0;
                _ReadMaskingLevel = 100;
                _WriteMaskingLevel = 100;
                _Description = "EMPTY";
                _Value = default(T);

                DoneState = ElementStateEnum.DEFAULT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void CopyTo(Element<T> target)
        {
            try
            {
                if (target == null) target = new Element<T>();
                target.Value = Value;
                target.LowerLimit = LowerLimit;
                target.UpperLimit = UpperLimit;
                if (Description != null)
                    target.Description = (string)Description.Clone();
                target.DoneState = DoneState;
                target.ValueChangedEvent = ValueChangedEvent;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override string ToString()
        {
            if (Value != null)
            {
                return Value.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        public void Setup()
        {
            try
            {
                CategoryIDList = new List<int>();
                AssociateElementIDList = new List<string>();
                //==> Category ID Initilize
                if (String.IsNullOrEmpty(CategoryID) == false)
                {
                    //==> Parsing Check
                    String[] split = CategoryID.Split(',');
                    int intValue = 0;
                    foreach (String categoryID in split)
                    {
                        //==> Parsing Check
                        if (int.TryParse(categoryID, out intValue) == false)
                            break;
                        CategoryIDList.Add(intValue);
                    }
                }

                //==> [Dependency Element ID List]
                if (String.IsNullOrEmpty(AssociateElementID) == false)
                {
                    //==> Parsing Check
                    String[] split = AssociateElementID.Split(',');
                    //int intValue = 0;
                    foreach (string depElementID in split)
                    {
                        //==> Parsing Check
                        //if (int.TryParse(depElementID, out intValue) == false)
                        //    break;
                        //AssociateElementIDList.Add(intValue);
                        if (!depElementID.Equals("EMPTY"))
                            AssociateElementIDList.Add(depElementID);
                    }
                }
                if (Value != null)
                    OriginalValue = Value;
                ValueType = typeof(T);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CopyTo(ElementPack target)
        {
            try
            {
                if (target == null) target = new ElementPack();
                target.Value = Value;
                target.ValueType = ValueType;
                target.ValueTypeDesc = "";
                if (ValueType != null)
                {
                    target.ValueTypeDesc = ValueType.ToString();
                }
                else
                {
                    target.ValueTypeDesc = "";
                }
                target.ElementID = ElementID;
                target.PropertyPath = (string)PropertyPath.Clone();
                target.CategoryID = (string)CategoryID.Clone();
                if (CategoryIDList != null)
                {
                    if (CategoryIDList.Count > 0)
                    {
                        int[] cats = new int[CategoryIDList.Count];
                        CategoryIDList.CopyTo(cats);
                        target.CategoryIDList = new List<int>(cats);
                    }
                    else
                    {
                        target.CategoryIDList = null;
                    }
                }
                else
                {
                    target.CategoryIDList = null;
                }
                target.ElementName = ElementName;
                target.LowerLimit = LowerLimit;
                target.UpperLimit = UpperLimit;
                if (Description != null)
                    target.Description = (string)Description.Clone();
                target.DoneState = DoneState;
                target.Unit = this.Unit;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
        }


        string _OriginPropertyPath = "";
        public string OriginPropertyPath
        {
            get { return GetOriginPropertyPath(); }
            set
            {
                if(_OriginPropertyPath != "" && _OriginPropertyPath != PropertyPath)
                {
                    LoggerManager.Debug($"{this.PropertyPath} Set OriginPropertyPath:{value}");
                }
                _OriginPropertyPath = value;
            }
        }


        public string GetOriginPropertyPath()
        {
            string retVal = "";
            if (_OriginPropertyPath is "")
            {
                retVal = this.PropertyPath;
            }
            else
            {
                retVal = _OriginPropertyPath;
            }
            return retVal;
        }

        string _ReportPropertyPath = "";
        public string ReportPropertyPath
        {
            get { return GetReportPropertyPath(); }
            set
            {
                if (_ReportPropertyPath != "" && _ReportPropertyPath != PropertyPath)
                {
                    LoggerManager.Debug($"{this.PropertyPath} Set ReportPropertyPath:{value}");
                }
                _ReportPropertyPath = value;
            }
        }


        public string GetReportPropertyPath()
        {
            string retVal = "";
            if (_ReportPropertyPath is "")
            {
                retVal = this.PropertyPath;
            }
            else
            {
                retVal = _ReportPropertyPath;
            }
            return retVal;
        }

        public EventCodeEnum ParamBaseValidate(string propertypath, object val, out string errorlog)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errormsg = "";
            try
            {
                //1. Convert 가능한 값인지 확인한다. 
                T localVal = Value;
                bool result = GetGenericValue(val, ref localVal);
                if (result == false)
                    return EventCodeEnum.UNDEFINED;

                //2. Enum 이면 가능하다고하고 정수나 실수형인 경우 Min/Max를 확인한다.
                if (this.IsNumericType())
                {
                    int irst;
                    if (int.TryParse(val.ToString(), out irst))
                    {
                        if (irst <= this.LowerLimit)
                        {
                            retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                            errormsg = $"ParamBaseValidate(): {propertypath} SetValue out of range. SetValue:{irst}, LowerLimit:{this.LowerLimit}, UpperLimit:{this.UpperLimit}";
                        }
                        else if (irst >= this.UpperLimit)
                        {
                            retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                            errormsg = $"ParamBaseValidate(): {propertypath} SetValue out of range. SetValue:{irst}, LowerLimit:{this.LowerLimit}, UpperLimit:{this.UpperLimit}";
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                   
                }              
                else if (this.IsFloatingType())
                {
                    double drst;
                    if (double.TryParse(val.ToString(), out drst))
                    {
                        if (drst < this.LowerLimit)
                        {
                            retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                            errormsg = $"ParamBaseValidate(): {propertypath} SetValue out of range. SetValue:{drst}, LowerLimit:{this.LowerLimit}, UpperLimit:{this.UpperLimit}";
                        }
                        else if (drst > this.UpperLimit)
                        {
                            retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                            errormsg = $"ParamBaseValidate(): {propertypath} SetValue out of range. SetValue:{drst}, LowerLimit:{this.LowerLimit}, UpperLimit:{this.UpperLimit}";
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }
                else
                {
                    // null 또는 정상적인 값이 여기를 탐.
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                errorlog = errormsg;
            }
            return retVal;

        }
        
        
        /// <summary>
        /// isValidParameterBase, isValidSourceBase, isValidCustomBase 의 리턴 타입을 보고 모두 NONE이어야 해당 값으로 set할 수 있는 상태라고 판단한다.
        /// 이 함수를 호출했다는 것은 source가 null이든 아니든 상관없이 Validation을 의도했다는 것을 의미함.
        /// </summary>
        /// <param name="propertypath"></param>
        /// <param name="element"></param>
        /// <param name="source"> param Validation을 해야하는 조건으로 사용. 만약 null이면 프로그램 로드 시로 판단하고 validation을 시행하지 않음. 단, Value = 로 직접 넣을 때도 null 이므로 validation 하지 않음.</param>
        /// <param name="val"></param>
        /// <param name="errorlog"></param>
        /// <returns></returns>
        public EventCodeEnum CheckSetValueAvailable(string propertypath, object val, out string errorlog)//, IModule source = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errormsg = "";
            try
            {
                EventCodeEnum isValidParameterBase = EventCodeEnum.UNDEFINED;
                //EventCodeEnum isValidSourceBase = EventCodeEnum.UNDEFINED;
                EventCodeEnum isValidCustomBase = EventCodeEnum.UNDEFINED;


                // Check Parameter In Range
                isValidParameterBase = ParamBaseValidate(propertypath, val, out errorlog);
                errormsg += errormsg;
      

                //Check Custom Condition                
                if (this.CheckSpecificSetValueAvailableEvent != null)
                {
                    isValidCustomBase = this.CheckSpecificSetValueAvailableEvent(propertypath, null, val, out errormsg);//, source: source);
                    errormsg += errormsg;
                }
                else
                {
                    isValidCustomBase = EventCodeEnum.NONE;
                }


                if (isValidParameterBase != EventCodeEnum.NONE ||                    
                    isValidCustomBase != EventCodeEnum.NONE)
                {
                    if (isValidParameterBase != EventCodeEnum.NONE)
                    {
                        retVal = isValidParameterBase;
                    }            

                    if (isValidCustomBase != EventCodeEnum.NONE)
                    {
                        retVal = isValidCustomBase;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;// Success
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                errorlog = errormsg;
            }

            return retVal;
        }

        

        //delegate로 property path 에 있는 PrevBehavior를 호출한다. 
        public EventCodeEnum BeforeValueChangedBehavior(string propertypath, IElement element, object val, out string errorlog, bool isNeedValidation = false)//, IModule source = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errormsg = "";
            try
            {
                if (this.PropertyPath != null && this.PropertyPath != "" && this.PropertyPath != "EMPTY")
                {
                    if (isNeedValidation == false)
                    {
                        //Validation을 의도적으로 하지 않는 경우.
                        //1.SetValue()로 함수를 호출 하지 않고 Value = value 로 값을 직접 Set한 경우(ex.Program Load)
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        //IModule을 받고 있지 않은 Class에서 호출되었을 경우 Source가 null이 더라도 Validation을 시행함.(ex: ViewModel)
                        retVal = this.CheckSetValueAvailable(propertypath, val, out errormsg);//, source: source);                   
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                errorlog = errormsg;
            }
            return retVal;
        }

 


        //delegate로 property path 에 있는 AfterValueChangedBehavior를 호출한다. 
        public EventCodeEnum AfterValueChangedBehavior(string propertypath, IElement element, Object oldVal, Object val, out string errorlog, object param = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errormsg = "";
            try
            {
                errormsg = "";
                // 1. VID Update
                RaisePropertyChanged("Value");// Raise를 따로 한번 해줘야 됨.

                // 2. Associate Value Update
                if (ValueChangedEvent != null)
                    ValueChangedEvent(oldVal, val, param);//아래 코드로 호환되게 만들기.// TODO: Maintenance 일때는 해당 값 다 나ㄹ라감.이유는?

                // 3. Report Element Value Update

                if (ReportPropertyPath != "" && ReportPropertyPath != null && ReportPropertyPath != propertypath)// 다를때만 
                {
                    object reportVal = val;
                    if(ConvertToReportTypeEvent != null)
                    {
                        // ReportElement -> OriginElement -> AfterBeh: ReportElement 로 오는 경우도 있고
                        // OriginElement -> AfterBeh: ReportElement 로 오는 경우도 있음.
                        reportVal = ConvertToReportTypeEvent(val);
                    }
                    retVal = this.ParamManager().GetElement(ReportPropertyPath).SetValue(reportVal);
                    if(retVal == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

              
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                errorlog = errormsg;
            }

            return retVal;
        }
    }
}
