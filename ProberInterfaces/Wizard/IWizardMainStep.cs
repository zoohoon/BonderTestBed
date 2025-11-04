using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces.Wizard
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public interface IWizardMainStep : IWizardStep/*, IHasSubModules*/
    {

    }

    public interface ISubStepModules : IParamNode
    {
        ISchedulingModule SchedulingModule { get; set; }

        /// <summary>
        /// 계층 구조(폴더구조)를 나타내기위한 다차원적인 리스트
        /// </summary>
        //ObservableCollection<ICategoryNodeItem> TemplateModules { get; set; }
        ObservableCollection<ITemplateModule> TemplateModules { get; set; }

        /// <summary>
        /// 모듈만(폴더구조 제외) 1차원적(1자)로 들어있는 리스트
        /// </summary>
        ObservableCollection<ITemplateModule> EntryTemplateModules { get; set; }
        ObservableCollection<IWizardPostTemplateModule> PostProcModules { get; set; }
        /// <summary>
        /// 실제 Process할 모듈 리스트
        /// </summary>
        ObservableCollection<ISubModule> SubModules { get; set; }


    }

    //public interface ISubStepModules : IParamNode
    //{
    //    ISchedulingModule SchedulingModule { get; set; }

    //    ObservableCollection<ITemplateModule> TemplateModules { get; set; }
    //    void GetSubModules();
    //    void GetSubRutineModule();
    //}



    public class SubStepModules : ISubStepModules,  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        [ParamIgnore]
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
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        private ISchedulingModule _SchedulingModule;
        public ISchedulingModule SchedulingModule
        {
            get { return _SchedulingModule; }
            set
            {
                if (value != _SchedulingModule)
                {
                    _SchedulingModule = value;
                    NotifyPropertyChanged("SchedulingModule");
                }
            }
        }


        //private ObservableCollection<ICategoryNodeItem> _TemplateModules
        //    = new ObservableCollection<ICategoryNodeItem>();
        //public ObservableCollection<ICategoryNodeItem> TemplateModules
        //{
        //    get { return _TemplateModules; }
        //    set
        //    {
        //        if (value != _TemplateModules)
        //        {
        //            _TemplateModules = value;
        //            NotifyPropertyChanged("TemplateModules");
        //        }
        //    }
        //}

        private ObservableCollection<ITemplateModule> _TemplateModules
            = new ObservableCollection<ITemplateModule>();
        public ObservableCollection<ITemplateModule> TemplateModules
        {
            get { return _TemplateModules; }
            set
            {
                if (value != _TemplateModules)
                {
                    _TemplateModules = value;
                    NotifyPropertyChanged("TemplateModules");
                }
            }
        }

        private ObservableCollection<ITemplateModule> _EntryTemplateModules
            = new ObservableCollection<ITemplateModule>();
        public ObservableCollection<ITemplateModule> EntryTemplateModules
        {
            get { return _EntryTemplateModules; }
            set
            {
                if (value != _EntryTemplateModules)
                {
                    _EntryTemplateModules = value;
                    NotifyPropertyChanged("EntryTemplateModules");
                }
            }
        }


        private ObservableCollection<IWizardPostTemplateModule> _PostProcModules
            = new ObservableCollection<IWizardPostTemplateModule>();
        [ParamIgnore]
        public ObservableCollection<IWizardPostTemplateModule> PostProcModules
        {
            get { return _PostProcModules; }
            set
            {
                if (value != _PostProcModules)
                {
                    _PostProcModules = value;
                    NotifyPropertyChanged("PostProcModules");
                }
            }
        }



        private ObservableCollection<ISubModule> _SubModules
            = new ObservableCollection<ISubModule>();
        [ParamIgnore]
        public ObservableCollection<ISubModule> SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = value;
                    NotifyPropertyChanged("SubModules");
                }
            }
        }

      
        public SubStepModules()
        {

        }
    }

    //public class CategoriesModule : INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void NotifyPropertyChanged(String info)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(info));
    //        }
    //    }

    //    private ObservableCollection<ICategoryModule> _Modules
    //        = new ObservableCollection<ICategoryModule>();
    //    public ObservableCollection<ICategoryModule> Modules
    //    {
    //        get { return _Modules; }
    //        set
    //        {
    //            if (value != _Modules)
    //            {
    //                _Modules = value;
    //                NotifyPropertyChanged("Modules");
    //            }
    //        }
    //    }



    //}

    public class CategoriesModule : IEnumerable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ObservableCollection<ICategoryNodeItem> _Modules
            = new ObservableCollection<ICategoryNodeItem>();
        public ObservableCollection<ICategoryNodeItem> Modules
        {
            get { return _Modules; }
            set
            {
                if (value != _Modules)
                {
                    _Modules = value;
                    NotifyPropertyChanged("Modules");
                }
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public CategoriesModuleEnum GetEnumerator()
        {
            return new CategoriesModuleEnum(Modules);
        }
        public void Add(object obj)
        {
            try
            {
            //if (obj is IWizardStep)
            //{
            //    Modules.Add((IWizardStep)obj);
            //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }

    public class CategoriesModuleEnum : INotifyPropertyChanged, IEnumerator
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        public object Current { get; }
        //private ObservableCollection<IAlignModule> _AlignModules
        //     = new ObservableCollection<IAlignModule>();
        //public ObservableCollection<IAlignModule> AlignModules
        //{
        //    get { return _AlignModules; }
        //    set
        //    {
        //        if (value != _AlignModules)
        //        {
        //            _AlignModules = value;
        //            NotifyPropertyChanged("AlignModules ");
        //        }
        //    }
        //}

        private ObservableCollection<ICategoryNodeItem> _CategoryModule
            = new ObservableCollection<ICategoryNodeItem>();
        public ObservableCollection<ICategoryNodeItem> CategoryModule
        {
            get { return _CategoryModule; }
            set
            {
                if (value != _CategoryModule)
                {
                    _CategoryModule = value;
                    NotifyPropertyChanged("CategoryModule");
                }
            }
        }


        private int index = -1;
        private ObservableCollection<ICategoryNodeItem> Modules;

        public CategoriesModuleEnum()
        {

        }
        public CategoriesModuleEnum(ObservableCollection<ICategoryNodeItem> modules)
        {
            try
            {
            Modules = modules;
            foreach(var module in Modules)
            {
                LoadCategories(module);
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public bool MoveNext()
        {
            try
            {
                if (index < CategoryModule.Count - 1)
                {
                    index++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Reset()
        {
            try
            {
                index = -1;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void LoadCategories(ICategoryNodeItem category)
        {
            try
            {

            CategoryModule.Add(category);
            //if (category is IModuleInfo)
            //{
            //    if (((IModuleInfo)category).AlignModule != null)
            //    {
            //        LoadAlignMdoule(((IModuleInfo)category).AlignModule);
            //    }
            //}
            //else if (category is ICategoryInfo)
            //{
            //    if (category.Categories != null)
            //    {
            //        foreach (var ccategory in category.Categories)
            //        {
            //            LoadCategories(category);

            //        }
            //    }
            //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private void LoadAlignMdoule(ObservableCollection<IProcessingModule> alignmodules)
        {
            try
            {
            //foreach (var module in alignmodules)
            //{
            //    AlignModules.Add(module);
            //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public void Add(object obj)
        {

        }

    }


    


}
