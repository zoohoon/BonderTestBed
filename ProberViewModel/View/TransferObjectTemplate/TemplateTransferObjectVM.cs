using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Enum;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProberViewModel
{
    public class TransferObjectTemplateComponent : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private EnumWaferType _SelectedType;
        public EnumWaferType SelectedType
        {
            get { return _SelectedType; }
            set
            {
                if (value != _SelectedType)
                {
                    _SelectedType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TransferObject _TransferObj = null;
        public TransferObject TransferObj
        {
            get { return _TransferObj; }
            set
            {
                if (value != _TransferObj)
                {
                    _TransferObj = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class TemplateTransferObjectVM : IMainScreenViewModel, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private TemplateTransferObjectWindow window = null;

        private bool? _EmulChecked = false;
        public bool? EmulChecked
        {
            get { return _EmulChecked; }
            set
            {
                if (value != _EmulChecked)
                {
                    _EmulChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumWaferType _SelectedType = EnumWaferType.STANDARD;
        public EnumWaferType SelectedType
        {
            get { return _SelectedType; }
            set
            {
                if (value != _SelectedType)
                {
                    _SelectedType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _SelectedSubsStatus = EnumSubsStatus.UNKNOWN;
        public EnumSubsStatus SelectedSubsStatus
        {
            get { return _SelectedSubsStatus; }
            set
            {
                if (value != _SelectedSubsStatus)
                {
                    _SelectedSubsStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CardPRESENCEStateEnum _SelectedCardPresenceStatus = CardPRESENCEStateEnum.UNDEFINED;
        public CardPRESENCEStateEnum SelectedCardPresenceStatus
        {
            get { return _SelectedCardPresenceStatus; }
            set
            {
                if (value != _SelectedCardPresenceStatus)
                {
                    _SelectedCardPresenceStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<CardPRESENCEStateEnum> _CardPresenceStatusInfos = 
            new List<CardPRESENCEStateEnum>() { CardPRESENCEStateEnum.CARD_ATTACH, CardPRESENCEStateEnum.CARD_DETACH, CardPRESENCEStateEnum.EMPTY};
        public List<CardPRESENCEStateEnum> CardPresenceStatusInfos
        {
            get { return _CardPresenceStatusInfos; }
            set
            {
                if (value != _CardPresenceStatusInfos)
                {
                    _CardPresenceStatusInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _SetCardPresenceCommand;
        public ICommand SetCardPresenceCommand
        {
            get
            {
                if (null == _SetCardPresenceCommand) _SetCardPresenceCommand = new AsyncCommand(SetCardPresence, showWaitCancel:false);
                return _SetCardPresenceCommand;
            }
        }
        public Autofac.IContainer Container { get; set; }
        
        private async Task SetCardPresence()
        {
            try
            {
                
               Task task = new Task(() =>
                {
                    if (EmulChecked == true)
                    {
                        Container = this.GetLoaderContainer();
                        var cardbuffer = Container.Resolve<ILoaderSupervisor>().Loader.ModuleManager
                            .FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, 1);

                        cardbuffer.UpdateCardBufferState(forced_presence: SelectedCardPresenceStatus);
                    }
                });
                task.Start();
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private OCRReadStateEnum _OCRType = OCRReadStateEnum.NONE;
        public OCRReadStateEnum OCRType
        {
            get { return _OCRType; }
            set
            {
                if (value != _OCRType)
                {
                    _OCRType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleTypeEnum _ModuleType = ModuleTypeEnum.SLOT;
        public ModuleTypeEnum ModuleType
        {
            get { return _ModuleType; }
            set
            {
                if (value != _ModuleType)
                {
                    _ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumWaferState _WaferState = EnumWaferState.UNPROCESSED;
        public EnumWaferState WaferState
        {
            get { return _WaferState; }
            set
            {
                if (value != _WaferState)
                {
                    _WaferState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ModuleTypeEnum _OriginModuleType = ModuleTypeEnum.UNDEFINED;
        public ModuleTypeEnum OriginModuleType
        {
            get { return _OriginModuleType; }
            set
            {
                if (value != _OriginModuleType)
                {
                    _OriginModuleType = value;
                    if(value == ModuleTypeEnum.SLOT|| value == ModuleTypeEnum.CST)
                    {
                        IsVisible = Visibility.Visible;
                    }
                    else
                    {
                        IsVisible = Visibility.Collapsed;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private int _OriginIndex;
        public int OriginIndex
        {
            get { return _OriginIndex; }
            set
            {
                if (value != _OriginIndex)
                {
                    _OriginIndex = value+1;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _IsVisible=Visibility.Collapsed;
        public Visibility IsVisible
        {
            get { return _IsVisible; }
            set
            {
                if (value != _IsVisible)
                {
                    _IsVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private TransferObject _TransferObj = null;
        public TransferObject TransferObj
        {
            get { return _TransferObj; }
            set
            {
                if (value != _TransferObj)
                {
                    _TransferObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TemplateTransferObjectVM()
        {
            TransferObj = new TransferObject();
            TransferObj.WaferType.Value = this.SelectedType;

            if (TransferObj.WaferType.Value == EnumWaferType.POLISH)
            {
                TransferObj.PolishWaferInfo = new PolishWaferInformation();
            }
            //TransferObjComponent = new TransferObjectTemplateComponent();
            //TransferObjComponent.TransferObj = new TransferObject();
            //TransferObjComponent.TransferObj.WaferType.Value = EnumWaferType.STANDARD;
        }

        public bool Show()
        {
            bool isCheck = false;

            try
            {
                if (window != null && window.Visibility == Visibility.Visible)
                {
                    window.Close();
                }

                String retValue = String.Empty;

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    window = new TemplateTransferObjectWindow();
                    window.DataContext = this;
                    window.Show();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return isCheck;
        }

        private RelayCommand<Object> _ChangedTypeCommand;
        public ICommand ChangedTypeCommand
        {
            get
            {
                if (null == _ChangedTypeCommand) _ChangedTypeCommand = new RelayCommand<Object>(ChangedTypeCommandFunc);
                return _ChangedTypeCommand;
            }
        }

        private RelayCommand<Object> _ChangedSubsStatusCommand;
        public ICommand ChangedSubsStatusCommand
        {
            get
            {
                if (null == _ChangedSubsStatusCommand) _ChangedSubsStatusCommand = new RelayCommand<Object>(ChangedSubsStatusCommandFunc);
                return _ChangedSubsStatusCommand;
            }
        }

        public bool Initialized => throw new NotImplementedException();

        public Guid ScreenGUID => throw new NotImplementedException();

        private void ChangedSubsStatusCommandFunc(object obj)
        {
            try
            {
                // TODO : ?
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void ChangedTypeCommandFunc(object obj)
        {
            try
            {
                TransferObj = new TransferObject();
                TransferObj.WaferType.Value = this.SelectedType;
                if (TransferObj.WaferType.Value == EnumWaferType.POLISH)
                {
                    TransferObj.PolishWaferInfo = new PolishWaferInformation();
                }

                //RaisePropertyChanged("TransferObj");

                //switch (Type)
                //{
                //    case EnumWaferType.INVALID:
                //        break;
                //    case EnumWaferType.UNDEFINED:
                //        break;
                //    case EnumWaferType.STANDARD:
                //        TypeTemplate = 
                //        break;
                //    case EnumWaferType.POLISH:
                //        break;
                //    case EnumWaferType.CARD:
                //        break;
                //    default:
                //        break;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void DeInitModule()
        {
            //throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {            
            return EventCodeEnum.NONE;
        }
    }
}
