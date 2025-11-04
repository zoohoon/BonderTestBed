using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberInterfaces.State;
    using ProberErrorCode;


    //public interface IPnpModule : IPnp
    //{
    //    //ObservableCollection<IPnpModule> Categories { get; set; }
    //    //string Parent { get; set; }
    //}

    //public interface ICategoryStepModule : ICategoryModule
    //{
    //    ObservableCollection<CategoryModuleBase> Categories { get; set; }
    //}

    [Serializable]
    public abstract class CategoryModuleBase :  IParamNode, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public  string Genealogy { get; set; }
        public  Object Owner { get; set; }

        public List<object> Nodes { get; set; }

        private string _Header;
        [XmlAttribute("Header")]
        public string Header
        {
            get { return _Header; }
            set
            {
                if (value != _Header)
                {
                    _Header = value;
                    NotifyPropertyChanged("Header");
                }
            }
        }

        private ObservableCollection<CategoryModuleBase> _Categories;
        public ObservableCollection<CategoryModuleBase> Categories
        {
            get { return _Categories; }
            set
            {
                if (value != _Categories)
                {
                    _Categories = value;
                    NotifyPropertyChanged("Categories");
                }
            }
        }

     

        public CategoryModuleBase()
        {

        }

        public CategoryModuleBase(ObservableCollection<CategoryModuleBase> categories)
        {
            Categories = categories;
        }
    }


    //public interface ICategoryNodeItem : IParamNode , ITemplateModule
    //{
    //    ICategoryNodeItem Parent { get; set; }
    //    ObservableCollection<ICategoryNodeItem> Categories { get; set; }
    //    string Header { get; set; }
    //    Task<EventCodeEnum> Cleanup(object parameter = null);

    //    //Task Cleanup(object parameter = null);    //}


    public interface IParamValidation
    {
        EventCodeEnum ParamValidation();
        bool IsParameterChanged(bool issave = false);

    }

    public interface ICategoryNodeItem : IParamNode, ITemplateModule ,IMainScreenViewModel/* : ICategoryNodeItem*//*, IParamValidation*/
    {
        ICategoryNodeItem Parent { get; set; }
        ObservableCollection<ITemplateModule> Categories { get; set; }
        string Header { get; set; }
        string RecoveryHeader { get; set; }
        //Task<EventCodeEnum> Cleanup(object parameter = null);
        EventCodeEnum ClearSettingData();
        Guid PageGUID { get; set; }
        EnumEnableState StateEnable { get; }
        EnumMoudleSetupState StateSetup { get; }
        EnumMoudleSetupState StateRecoverySetup { get; }
        //Wizard 사용안함. Remove
        void SetEnableState(EnumEnableState state);
        void ChangeSetupState(IMoudleSetupState state);
        void SetNodeSetupState(EnumMoudleSetupState state, bool isparent = false);
        void SetNodeSetupRecoveryState(EnumMoudleSetupState state, bool isparent = false);
        EnumMoudleSetupState GetModuleSetupState();
        void SetStepSetupState(string header = null);
        bool NoneCleanUp { get; set; }
    }

    public class CategoryNameItems
    {
        private string _ParentName;
        public string ParentName
        {
            get { return _ParentName; }
            set { _ParentName = value; }
        }

        private string _Header;

        public string Header
        {
            get { return _Header; }
            set { _Header = value; }
        }

        private string _RecoveryHeader;

        public string RecoveryHeader
        {
            get { return _RecoveryHeader; }
            set { _RecoveryHeader = value; }
        }
        private ObservableCollection<CategoryNameItems> _Categories
             = new ObservableCollection<CategoryNameItems>();

        public ObservableCollection<CategoryNameItems> Categories
        {
            get { return _Categories; }
            set { _Categories = value; }
        }

        private bool _IsCategoryForm;

        public bool IsCategoryForm
        {
            get { return _IsCategoryForm; }
            set { _IsCategoryForm = value; }
        }

        public CategoryNameItems()
        {

        }
        public CategoryNameItems(string header,string recoveryHeader)
        {
            Header = header;
            RecoveryHeader = recoveryHeader;
        }
        public CategoryNameItems(string parentName, string header, string recoveryHeader)
        {
            ParentName = parentName;
            Header = header;
            RecoveryHeader = recoveryHeader;
        }
        

    }

}
