using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using ProberInterfaces;

namespace PopupControlViewModel
{
    public abstract class ModuleInfoElement : INotifyPropertyChanged, IFactoryModule, IModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        protected Autofac.IContainer Container { get; set; }

        private string _ModuleName;
        public string ModuleName
        {
            get { return _ModuleName; }
            set
            {
                _ModuleName = value;
                RaisePropertyChanged();
            }
        }

        private object _Content;
        public object Content
        {
            get { return _Content; }
            set
            {
                _Content = value;
                RaisePropertyChanged();
            }
        }

        private ICommunicationable _CommunicationModule;
        public ICommunicationable CommunicationModule
        {
            get { return _CommunicationModule; }
            set
            {
                if (value != _CommunicationModule)
                {
                    _CommunicationModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        public virtual void SetContainer(Autofac.IContainer container)
        {
            this.Container = container;
        }

        public abstract EventCodeEnum InitModule();
        public abstract void DeInitModule();
        public abstract bool Initialized { get; set; }
    }

    //public class InfoElement : INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void NotifyPropertyChanged(string propName)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    //    }

    //    public object Content { get; set; }

    //    private string _PrefixString;
    //    public string PreFixString
    //    {
    //        get { return _PrefixString; }
    //        set
    //        {
    //            _PrefixString = value;
    //            NotifyPropertyChanged(nameof(PreFixString));
    //        }
    //    }

    //    private string _PostfixString;
    //    public string PostFixString
    //    {
    //        get { return _PostfixString; }
    //        set
    //        {
    //            _PostfixString = value;
    //            NotifyPropertyChanged(nameof(PostFixString));
    //        }
    //    }

    //    public InfoElement(object Content, string PreFixString = "", string PostFixString = "")
    //    {
    //        this.Content = Content;
    //        this.PreFixString = PreFixString;
    //        this.PostFixString = PostFixString;
    //    }
    //}
}
