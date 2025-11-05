using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PopupControlViewModel
{
    public class PopupPanelBase : IPopupPanelBase, INotifyPropertyChanged
    {
        private Autofac.IContainer Container { get; set; }

        readonly Guid _ViewModelGUID = new Guid("8F916576-D51D-409C-BFE7-EAF7FF011C68");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {
            
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ModuleInfoCollection.Clear();

                    ModuleInfoElement tmp = new GPIBModuleInfoElement();
                    ModuleInfoCollection.Add(tmp);

                    tmp = new SONETModuleInfoElement();
                    ModuleInfoCollection.Add(tmp);

                    tmp = new ChillerModuleInfoElement();
                    ModuleInfoCollection.Add(tmp);

                    tmp = new GEMModuleInfoElement();
                    //tmp.InfoCollection.Add(new InfoElement("State", "State : "));
                    ModuleInfoCollection.Add(tmp);

                    foreach (var v in ModuleInfoCollection)
                    {
                        v.SetContainer(this.GetContainer());
                        v.InitModule();
                    }

                    SelectModuleInfo = ModuleInfoCollection[0];

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        private ObservableCollection<ModuleInfoElement> _ModuleInfoCollection;
        public ObservableCollection<ModuleInfoElement> ModuleInfoCollection
        {
            get { return _ModuleInfoCollection; }
            set
            {
                if (value != _ModuleInfoCollection)
                {
                    _ModuleInfoCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleInfoElement _SelectModuleInfo;
        public ModuleInfoElement SelectModuleInfo
        {
            get { return _SelectModuleInfo; }
            set
            {
                if (value != _SelectModuleInfo)
                {
                    _SelectModuleInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsPopupOpen;
        public bool IsPopupOpen
        {
            get { return _IsPopupOpen; }
            set
            {
                if (value != _IsPopupOpen)
                {
                    _IsPopupOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PopupPanelBase()
        {
            try
            {
                ModuleInfoCollection = new ObservableCollection<ModuleInfoElement>();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand<object> _TochUpCommand;
        public ICommand TochUpCommand
        {
            get
            {
                if (null == _TochUpCommand) _TochUpCommand = new RelayCommand<object>(TochUp);
                return _TochUpCommand;
            }
        }

        private void TochUp(object noparam)
        {
            try
            {
                IsPopupOpen = true;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand<object> _ItemSelectCommand;
        public ICommand ItemSelectCommand
        {
            get
            {
                if (null == _ItemSelectCommand) _ItemSelectCommand = new RelayCommand<object>(ItemSelect);
                return _ItemSelectCommand;
            }
        }

        private void ItemSelect(object noparam)
        {
            try
            {
                FrameworkElement control = noparam as FrameworkElement;

                if (control != null)
                {
                    SelectModuleInfo = control.DataContext as ModuleInfoElement;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand<object> _TochLeftCommand;
        public ICommand TochLeftCommand
        {
            get
            {
                if (null == _TochLeftCommand) _TochLeftCommand = new RelayCommand<object>(TochLeft);
                return _TochLeftCommand;
            }
        }

        private void TochLeft(object noparam)
        {
            try
            {
                if (IsPopupOpen == true)
                {
                    IsPopupOpen = false;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
}
