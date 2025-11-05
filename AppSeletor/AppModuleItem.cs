using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace AppSelector
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Xml.Serialization;

    [Serializable]
    public class AppModuleItems : ISystemParameterizable, INotifyPropertyChanged
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        [field: NonSerialized, JsonIgnore]
        public List<object> Nodes { get; set; }

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        [field: NonSerialized, JsonIgnore]
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
        public string FilePath { get; } = "";

        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "AppItems.json";

        public AppModuleItems()
        {
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
                throw;
            }
            return retval;
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

                Items = new ObservableCollection<AppModuleItem>();

                //Items.Add(MakeAppItem("System", "S", "System Setting", this.ProberStation().GetPageFromViewModel(new Guid("A43A956D-B46E-61B6-5A61-31766111750D")), "settings-outline"));
                //Items.Add(MakeAppItem("System", "S", "System Setting", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), "Settings"));
                //Items.Add(MakeAppItem("Account", "A", "Account Setting", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), "AccountCardDetails"));
                //Items.Add(MakeAppItem("Recipe", "R", "Show Recipe", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), "ClipboardCheck"));
                //Items.Add(MakeAppItem("Cassette", "C", "Show Cassette", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), "Database"));
                //Items.Add(MakeAppItem("Language", "L", "Language Setting", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), "LanguagePythonText"));
                //Items.Add(MakeAppItem("Network", "N", "Network Setting", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), "LanConnect"));
                //Items.Add(MakeAppItem("Alarm", "A", "Alarm Setting", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), "BellRing"));
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///
                Items.Add(MakeAppItem("OCR", "O", "OCR Setting", new Guid("8aa6dac9-5c54-43e3-8802-0a5dd69e25c8"), "BarcodeScan"));
                Items.Add(MakeAppItem("Card Change", "C", "Card Change", new Guid("0922A4D5-D811-425D-A68F-6CE5A210102B"), "SwapHorizontal"));

                string pathdata = "M14.22,19L17.05,16.17L14.93,14H22V21.07L19.88,19L17.05,21.83L14.22,19M11.39,19L12.39,18H12C7.58,18 4,16.21 4,14V17C4,19.21 7.58,21 12,21C12.47,21 12.93,21 13.38,20.94L11.39,19M17.29,12H20V9C20,10.2 19,11.27 17.29,12M4,9V12C4,14.21 7.58,16 12,16C12.67,16 13.34,15.96 14,15.87L11.07,13C7.09,12.74 4,11.05 4,9M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3Z";
                Items.Add(MakeAppItem("Cassette Port", "CP", "Cassette Port", new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53"), null, null, pathdata));

                pathdata = "M10,9A1,1 0 0,1 11,8A1,1 0 0,1 12,9V13.47L13.21,13.6L18.15,15.79C18.68,16.03 19,16.56 19,17.14V21.5C18.97,22.32 18.32,22.97 17.5,23H11C10.62,23 10.26,22.85 10,22.57L5.1,18.37L5.84,17.6C6.03,17.39 6.3,17.28 6.58,17.28H6.8L10,19V9M11,5A4,4 0 0,1 15,9C15,10.5 14.2,11.77 13,12.46V11.24C13.61,10.69 14,9.89 14,9A3,3 0 0,0 11,6A3,3 0 0,0 8,9C8,9.89 8.39,10.69 9,11.24V12.46C7.8,11.77 7,10.5 7,9A4,4 0 0,1 11,5Z";
                Items.Add(MakeAppItem("Manual Contact", "M", "Manual Contact", new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"), null, null, pathdata));

                pathdata = "M4,6H20V16H4M20,18A2,2 0 0,0 22,16V6C22,4.89 21.1,4 20,4H4C2.89,4 2,4.89 2,6V16A2,2 0 0,0 4,18H0V20H24V18H20Z";
                Items.Add(MakeAppItem("Sys Setting", "S", "System Setting", new Guid("A43A956D-B46E-61B6-5A61-31766111750D"), null, null, pathdata));

                pathdata = "M19,10H17V8H19M19,13H17V11H19M16,10H14V8H16M16,13H14V11H16M16,17H8V15H16M7,10H5V8H7M7,13H5V11H7M8,11H10V13H8M8,8H10V10H8M11,11H13V13H11M11,8H13V10H11M20,5H4C2.89,5 2,5.89 2,7V17A2,2 0 0,0 4,19H20A2,2 0 0,0 22,17V7C22,5.89 21.1,5 20,5Z";
                Items.Add(MakeAppItem("Dev Setting", "D", "Device Setting", new Guid("C059599B-BEDF-2137-859B-47C15E433E4D"), null, null, pathdata));

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public void SetElementMetaData()
        {

        }
        private AppModuleItem MakeAppItem(string name, string code, string desc, object content, string icon = null, string badge = null, string pathdata = "")
        {
            AppModuleItem tmp = new AppModuleItem(name, code, desc, content);
            try
            {

                tmp.Icon = icon;
                tmp.Badge = badge;

                if (string.IsNullOrEmpty(pathdata))
                {
                    tmp.IsPathData = false;
                    tmp.PathData = pathdata;
                }
                else
                {
                    tmp.IsPathData = true;
                    tmp.PathData = pathdata;
                }

                if (icon == null)
                {
                    tmp.Icon = "Close";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return tmp;

        }

        private ObservableCollection<AppModuleItem> _Items;
        public ObservableCollection<AppModuleItem> Items
        {
            get { return _Items; }
            set
            {
                if (value != _Items)
                {
                    _Items = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class AppModuleItem : INotifyPropertyChanged, IFactoryModule
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }

        public AppModuleItem()
        {

        }

        public delegate void SwitchPageDelegate(object obj);
        [field: NonSerialized, JsonIgnore]
        [JsonIgnore]
        public SwitchPageDelegate SwitchTo { get; set; }

        private string _name;
        private string _code;
        private string _badge;
        private string _description;
        private bool _isSelected;
        private object _content;
        private Visibility _visibility;
        private ScrollBarVisibility _horizontalScrollBarVisibilityRequirement;
        private ScrollBarVisibility _verticalScrollBarVisibilityRequirement;

        public AppModuleItem(string name, string code, object content)
        {
            try
            {
                _name = name;
                _code = code;
                Content = content;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public AppModuleItem(string name, string code, string desc, object content)
        {
            try
            {
                _name = name;
                _code = code;
                _description = desc;
                Content = content;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public AppModuleItem(string name, string code, string desc, object content, SwitchPageDelegate switchto)
        {
            try
            {
                _name = name;
                _code = code;
                _description = desc;
                Content = content;
                SwitchTo += switchto;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public AppModuleItem(string name, string code, string desc,
            object content, SwitchPageDelegate switchto, string icon)
        {
            try
            {
                _name = name;
                _code = code;
                _description = desc;
                Content = content;
                SwitchTo += switchto;
                _Icon = icon;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private bool _IsPathData;
        public bool IsPathData
        {
            get { return _IsPathData; }
            set { this.MutateVerbose(ref _IsPathData, value, RaisePropertyChanged()); }
        }

        private string _PathData;
        public string PathData
        {
            get { return _PathData; }
            set
            {
                _PathData = value;
                RaisePropertyChanged();
                //this.MutateVerbose(ref _PathData, value, RaisePropertyChanged());
            }
        }

        private string _Icon;
        public string Icon
        {
            get { return _Icon; }
            set
            {
                _Icon = value;
                RaisePropertyChanged();
                //this.MutateVerbose(ref _Icon, value, RaisePropertyChanged());
            }
        }
        public string Badge
        {
            get { return _badge; }
            set
            {
                _badge = value;
                RaisePropertyChanged();
                //this.MutateVerbose(ref _badge, value, RaisePropertyChanged());
            }
        }
        public string Code
        {
            get { return _code; }
            set { this.MutateVerbose(ref _code, value, RaisePropertyChanged()); }
        }
        public string Name
        {
            get { return _name; }
            set { this.MutateVerbose(ref _name, value, RaisePropertyChanged()); }
        }
        public string Description
        {
            get { return _description; }
            set { this.MutateVerbose(ref _description, value, RaisePropertyChanged()); }
        }
        public Visibility Visibility
        {
            get { return _visibility; }
            set { this.MutateVerbose(ref _visibility, value, RaisePropertyChanged()); }
        }
        public object Content
        {
            get { return _content; }
            set { this.MutateVerbose(ref _content, value, RaisePropertyChanged()); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                this.MutateVerbose(ref _isSelected, value, RaisePropertyChanged());
                this.LotOPModule().SaveAppItems();
            }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibilityRequirement
        {
            get { return _horizontalScrollBarVisibilityRequirement; }
            set { this.MutateVerbose(ref _horizontalScrollBarVisibilityRequirement, value, RaisePropertyChanged()); }
        }
        public ScrollBarVisibility VerticalScrollBarVisibilityRequirement
        {
            get { return _verticalScrollBarVisibilityRequirement; }
            set { this.MutateVerbose(ref _verticalScrollBarVisibilityRequirement, value, RaisePropertyChanged()); }
        }

        [field: NonSerialized, JsonIgnore]
        private Thickness _marginRequirement = new Thickness(16);
        [XmlIgnore, JsonIgnore]
        public Thickness MarginRequirement
        {
            get { return _marginRequirement; }
            set { this.MutateVerbose(ref _marginRequirement, value, RaisePropertyChanged()); }
        }

        [field: NonSerialized, JsonIgnore]
        private RelayCommand<object> _SwitchPage;
        [XmlIgnore, JsonIgnore]
        public ICommand SwitchPage
        {
            get
            {
                if (null == _SwitchPage) _SwitchPage = new RelayCommand<object>(PageSwitching);
                return _SwitchPage;
            }
        }

        private void PageSwitching(object obj)
        {
            if (SwitchTo != null) SwitchTo(obj);
        }

        [field: NonSerialized]
        private RelayCommand<object> _RaisePropertyCommand;
        [XmlIgnore, JsonIgnore]
        public ICommand RaisePropertyCommand
        {
            get
            {
                if (null == _RaisePropertyCommand) _RaisePropertyCommand = new RelayCommand<object>(RaiseProperty);
                return _RaisePropertyCommand;
            }
        }

        private void RaiseProperty(object obj)
        {
            try
            {
                IsSelected = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
