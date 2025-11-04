using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces.Template;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [Serializable]
    [XmlInclude(typeof(CategoryModuleBase))]
    [XmlInclude(typeof(CategoryInfo))]
    [XmlInclude(typeof(ModuleInfo))]
    [XmlInclude(typeof(ModuleDllInfo))]
    public class TemplateFileParam : ITemplateParam, INotifyPropertyChanged, IParam
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        private CategoryModuleBase _SubRoutineModule;
        public CategoryModuleBase SubRoutineModule
        {
            get { return _SubRoutineModule; }
            set
            {
                if (value != _SubRoutineModule)
                {
                    _SubRoutineModule = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<CategoryModuleBase> _TemlateModules
            = new ObservableCollection<CategoryModuleBase>();
        public ObservableCollection<CategoryModuleBase> TemlateModules
        {
            get { return _TemlateModules; }
            set
            {
                if (value != _TemlateModules)
                {
                    _TemlateModules = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ControlTemplateParameter> _ControlTemplates
             = new ObservableCollection<ControlTemplateParameter>();
        public ObservableCollection<ControlTemplateParameter> ControlTemplates
        {
            get { return _ControlTemplates; }
            set
            {
                if (value != _ControlTemplates)
                {
                    _ControlTemplates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<RecoveryTemplateParameter> _RecoveryTemplates
             = new ObservableCollection<RecoveryTemplateParameter>();
        public ObservableCollection<RecoveryTemplateParameter> RecoveryTemplates
        {
            get { return _RecoveryTemplates; }
            set
            {
                if (value != _RecoveryTemplates)
                {
                    _RecoveryTemplates = value;
                    RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public string FilePath { get; set; }

        [XmlIgnore, JsonIgnore]
        public string FileName { get; set; }

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
        = new List<object>();

        public TemplateFileParam()
        {

        }
        public TemplateFileParam(ObservableCollection<CategoryModuleBase> modules)
        {
            TemlateModules = modules;
        }

        public void AddTemplateModules(CategoryModuleBase module)
        {
            TemlateModules.Add(module);
        }

        public void AddControlTemplate(ControlTemplateParameter controltemplate)
        {
            ControlTemplates.Add(controltemplate);
        }

        public void AddRecoveryTempplate(RecoveryTemplateParameter recoverytemplateparam)
        {
            RecoveryTemplates.Add(recoverytemplateparam);
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }


    [Serializable()]
    public class ControlTemplateParameter
    {
        private UCControlInfo _ViewControlModuleInfo;

        public UCControlInfo ViewControlModuleInfo
        {
            get { return _ViewControlModuleInfo; }
            set { _ViewControlModuleInfo = value; }
        }

        private UCControlInfo _ViewModelControlModuleInfo;

        public UCControlInfo ViewModelControlModuleInfo
        {
            get { return _ViewModelControlModuleInfo; }
            set { _ViewModelControlModuleInfo = value; }
        }

        private ObservableCollection<ControlPNPButton> _ControlPNPButtons
             = new ObservableCollection<ControlPNPButton>();

        public ObservableCollection<ControlPNPButton> ControlPNPButtons
        {
            get { return _ControlPNPButtons; }
            set { _ControlPNPButtons = value; }
        }


        public ControlTemplateParameter()
        {

        }
    }

    [Serializable()]
    public class RecoveryTemplateParameter
    {
        private string _ErrorModuleName;

        public string ErrorModuleName
        {
            get { return _ErrorModuleName; }
            set { _ErrorModuleName = value; }
        }

        private List<Guid> _RecoveryStepGUID
            = new List<Guid>();

        public List<Guid> RecoveryStepGUID
        {
            get { return _RecoveryStepGUID; }
            set { _RecoveryStepGUID = value; }
        }

        public RecoveryTemplateParameter()
        {

        }

        public List<Guid> GetRecoveryStepGUID()
        {
            return this.RecoveryStepGUID;
        }
    }

    [Serializable()]
    public class UCControlInfo
    {
        private ModuleDllInfo _DllInfo;

        public ModuleDllInfo DllInfo
        {
            get { return _DllInfo; }
            set { _DllInfo = value; }
        }

        private Guid _DllGuid;

        public Guid DllGuid
        {
            get { return _DllGuid; }
            set { _DllGuid = value; }
        }
        public UCControlInfo()
        {

        }

        public UCControlInfo(ModuleDllInfo dllinfo, Guid dllguid)
        {
            try
            {
            DllInfo = dllinfo;
            DllGuid = dllguid;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

    [Serializable()]
    public class ControlPNPButton
    {
        private string _Caption;

        public string Caption
        {
            get { return _Caption; }
            set { _Caption = value; }
        }

        private Guid _CuiBtnGUID;

        public Guid CuiBtnGUID
        {
            get { return _CuiBtnGUID; }
            set { _CuiBtnGUID = value; }
        }

        private Guid _ViewGuid;

        public Guid ViewGuid
        {
            get { return _ViewGuid; }
            set { _ViewGuid = value; }
        }


        private List<Guid> _StepGUID
             = new List<Guid>();

        public List<Guid> StepGUID
        {
            get { return _StepGUID; }
            set { _StepGUID = value; }
        }


        public ControlPNPButton()
        {

        }
    }
}
