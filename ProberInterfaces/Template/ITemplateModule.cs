using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.Template;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    public interface ITemplateModule : IModule, IFactoryModule, IParamValidation
    {
    }
    public interface ISubRoutine : ITemplateModule
    {

    }

    public interface ISubControlModule
    {

    }

    //public interface ITemplateManager
    //{
    //    EventCodeEnum LoadTemplate();
    //    EventCodeEnum SaveTemplate();
    //    ObservableCollection<ITemplateModule> GetTemplate();
    //    EventCodeEnum CheckTemplate(bool applyload = true, int index = -1);
    //}

    public interface ITemplate
    {
        ITemplateFileParam TemplateParameter { get; }
        ITemplateParam LoadTemplateParam { get; set; }
        ISubRoutine SubRoutine { get; set; }
    }
    public interface ITemplateExtension
    {
        void InjectTemplate(ITemplateParam param);
    }


    public interface IConTemplateModule : ITemplate
    {
        TemplateCollection Template { get; set; }
    }
    public interface ITemplateStateModule : ITemplate
    {
        TemplateStateCollection Template { get; set; }
    }

    public interface ITemplateCollection
    {

    }

    public class TemplateModuleObservableCollection : ObservableCollection<ITemplateModule>, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; set; }

        public string FileName { get; set; }

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
        = new List<object>();

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }

    public class TemplateCollection :  INotifyPropertyChanged, ITemplateCollection, IEnumerable, IEnumerator
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //private ITemplateParam _LoadTemplate;
        //public ITemplateParam LoadTemplate
        //{
        //    get { return _LoadTemplate; }
        //    set
        //    {
        //        if (value != _LoadTemplate)
        //        {
        //            _LoadTemplate = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private TemplateModuleObservableCollection _TemplateModules;
        public TemplateModuleObservableCollection TemplateModules
        {
            get { return _TemplateModules; }
            set
            {
                if (value != _TemplateModules)
                {
                    _TemplateModules = value;
                    ConverterEntryModules();
                    RaisePropertyChanged();
                }
            }
        }


        public ITemplateModule GetSubRutineModule()
        {
            ITemplateModule module = null;
            try
            {
                foreach(var tmodule in TemplateModules)
                {
                    if(tmodule is ISubRoutine)
                    {
                        module = tmodule;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return module;
        }

        public ObservableCollection<ISubControlModule> GetControlModule()
        {
            ObservableCollection<ISubControlModule> module = null;
            try
            {
                foreach (var tmodule in TemplateModules)
                {
                    if (tmodule is ISubControlModule)
                    {
                        module.Add(tmodule as ISubControlModule);
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return module;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this;
        }

        public object Current
        {
            get
            {
                return TemplateModules[index];
            }
        }
        private int index = -1;
        public bool MoveNext()
        {
            if (index < TemplateModules.Count - 1)
            {
                index++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            index = -1;
        }
        public void Add(object obj)
        {

        }

        private ObservableCollection<ITemplateModule> _EntryModules
             = new ObservableCollection<ITemplateModule>();

        public ObservableCollection<ITemplateModule> EntryModules
        {
            get { return _EntryModules; }
            set { _EntryModules = value; }
        }

        private void ConverterEntryModules()
        {
            try
            {
            foreach(var module in TemplateModules)
            {
                if (module is IPnpCategoryForm)
                {
                    InnerForm(module as IPnpCategoryForm);
                }
                else if(module is ITemplateModule)
                {
                    EntryModules.Add(module);
                }

            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private void InnerForm(IPnpCategoryForm form)
        {
            try
            {
            foreach(var module in form.Categories)
            {
                if (module is IPnpCategoryForm)
                {
                    InnerForm(module as IPnpCategoryForm);
                }
                else if (module is ITemplateModule)
                {
                    EntryModules.Add(module);
                }
            }

            EntryModules.Add(form as ITemplateModule);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }


    public class TemplateStateCollection : TemplateCollection
    {
        public TemplateStateCollection()
        {

        }

        public ISchedulingModule SchedulingModule { get; set; }

        public List<ISubModule> GetProcessingModule()
        {
            List<ISubModule> module = null;
            try
            {
                //if (EntryModules.Count == 0)
                //{
                //    LoggerManager.Error($"[{this.ToString()}] The number of EntryModules is {EntryModules.Count}.");
                //}

                foreach (var tmodule in EntryModules)
                {
                    if (tmodule is ISubModule)
                    {
                        if (module == null)
                            module = new List<ISubModule>();
                        module.Add(tmodule as ISubModule);
                    }
                }

            }
            catch (Exception err)
            {
                throw err;
            }
            return module;
        }
    }

}
