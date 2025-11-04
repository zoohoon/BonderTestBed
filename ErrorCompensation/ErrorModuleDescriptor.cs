using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ErrorCompensation
{
    public enum ErrorModuleType
    {
        Undefined,
        First,
        Second,
        SecondPin
    }

    [Serializable]
    public class ErrorCompensationDescriptorParam : ISystemParameterizable, INotifyPropertyChanged, IParamNode
    {


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
        public string Genealogy { get; set; }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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


        public List<object> Nodes { get; set; }
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ObservableCollection<ErrorModuleDescriptor> _ErrorModuleDescriptors = new ObservableCollection<ErrorModuleDescriptor>();

        public ObservableCollection<ErrorModuleDescriptor> ErrorModuleDescriptors
        {
            get { return _ErrorModuleDescriptors; }
            set { _ErrorModuleDescriptors = value; }
        }
        private Element<ObservableCollection<EnumAxisConstants>> _AssociatedAxisTypeList = new Element<ObservableCollection<EnumAxisConstants>>();
        public Element<ObservableCollection<EnumAxisConstants>> AssociatedAxisTypeList
        {
            get { return _AssociatedAxisTypeList; }
            set
            {
                if (value != _AssociatedAxisTypeList)
                {
                    _AssociatedAxisTypeList = value;
                    NotifyPropertyChanged("AssociatedAxisTypeList");
                }
            }
        }

        private Element<bool> _EnableLinear = new Element<bool>() { Value = true };
        public Element<bool> EnableLinear
        {
            get { return _EnableLinear; }
            set
            {
                if (value != _EnableLinear)
                {
                    _EnableLinear = value;
                    NotifyPropertyChanged("EnableLinear");
                }
            }
        }
        private Element<bool> _EnableAngular = new Element<bool>() { Value = true };
        public Element<bool> EnableAngular
        {
            get { return _EnableAngular; }
            set
            {
                if (value != _EnableAngular)
                {
                    _EnableAngular = value;
                    NotifyPropertyChanged("EnableAngular");
                }
            }
        }
        private Element<bool> _EnableStraightness = new Element<bool>() { Value = true };
        public Element<bool> EnableStraightness
        {
            get { return _EnableStraightness; }
            set
            {
                if (value != _EnableStraightness)
                {
                    _EnableStraightness = value;
                    NotifyPropertyChanged("EnableStraightness");
                }
            }
        }
        public string FilePath { get; } = "";
        public string FileName { get; } = "errormoduledesc.json";
        public void SetElementMetaData()
        {

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
                AssociatedAxisTypeList.Value = new ObservableCollection<EnumAxisConstants>();
                AssociatedAxisTypeList.Value.Add(EnumAxisConstants.X);
                AssociatedAxisTypeList.Value.Add(EnumAxisConstants.Y);
                AssociatedAxisTypeList.Value.Add(EnumAxisConstants.Z);
                AssociatedAxisTypeList.Value.Add(EnumAxisConstants.C);


                _ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.First, this.FileManager().GetSystemRootPath() + @"\ErrorTable1D.json", true, true));
                _ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.Second, this.FileManager().GetSystemRootPath() + @"\ErrorTable2D.json", false, false));

                //_ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.First,  @"C:\ProberSystem\Parameters\ErrorTable1D.XML", true, false));
                //_ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.Second, @"C:\ProberSystem\Parameters\ErrorTable2D.XML", false, false));
                EnableAngular.Value = true;
                EnableLinear.Value = true;
                EnableStraightness.Value = true;
                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        //public ErrorCompensationDescriptor()
        //{
        //    //  _ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.First, @"C:\ProberSystem\Parameters\ErrorTable1D.XML", true,true));
        //    // _ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.Second, @"C:\ProberSystem\Parameters\ErrorTable2D.XML", false,true));
        //}
        //public void DefaultSetting()
        //{
        //    AssociatedAxisTypeList.Add(EnumAxisConstants.X);
        //    AssociatedAxisTypeList.Add(EnumAxisConstants.Y);
        //    AssociatedAxisTypeList.Add(EnumAxisConstants.Z);
        //    AssociatedAxisTypeList.Add(EnumAxisConstants.C);
        //    _ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.First, @"C:\ProberSystem\Parameters\ErrorTable1D.XML", true, false));
        //    _ErrorModuleDescriptors.Add(new ErrorModuleDescriptor(ErrorModuleType.Second, @"C:\ProberSystem\Parameters\ErrorTable2D.XML", false, false));

        //}
    }


    [Serializable]
    public class ErrorModuleDescriptor : INotifyPropertyChanged, IParamNode
    {
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

        public List<object> Nodes { get; set; }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private Element<ErrorModuleType> _Type = new Element<ErrorModuleType>();
        //[XmlAttribute("ErrorModuleType")]
        public Element<ErrorModuleType> Type
        {
            get { return _Type; }
            set
            {
                if (value != this._Type)
                {
                    _Type = value;
                    NotifyPropertyChanged("Type");
                }
            }
        }
        private Element<string> _Path = new Element<string>();
        // [XmlAttribute("Path")]
        public Element<string> Path
        {
            get { return _Path; }
            set
            {
                if (value != this._Path)
                {
                    _Path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }
        private Element<bool> _IsEssential = new Element<bool>();
        //  [XmlAttribute("IsEssential")]
        public Element<bool> IsEssential
        {
            get { return _IsEssential; }
            set
            {
                if (value != this._IsEssential)
                {
                    _IsEssential = value;
                    NotifyPropertyChanged("IsEssential");
                }
            }
        }
        private Element<bool> _Enable = new Element<bool>();
        // [XmlAttribute("Enable")]
        public Element<bool> Enable
        {
            get { return _Enable; }
            set
            {
                if (value != this._Enable)
                {
                    _Enable = value;
                    NotifyPropertyChanged("Enable");
                }
            }
        }
        public ErrorModuleDescriptor()
        {
            try
            {
                Type.Value = ErrorModuleType.Undefined;
                Path.Value = "";
                IsEssential.Value = false;
                Enable.Value = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ErrorModuleDescriptor(ErrorModuleType type, string path, bool isessential, bool enable)
        {
            try
            {
                Type.Value = type;
                Path.Value = path;
                IsEssential.Value = isessential;
                Enable.Value = enable;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
