using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberViewModel.View.ResultMap;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProberViewModel.ViewModel.ResultMap
{
    public class ResultMapAnalyzeViewerVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private readonly Guid _ViewModelGUID = new Guid("4443dcea-9e4a-4ada-9fae-b3580f613559");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        private int _resultMapTabSelectedIndex;
        public int ResultMapTabSelectedIndex
        {
            get { return _resultMapTabSelectedIndex; }
            set
            {
                if (value != _resultMapTabSelectedIndex)
                {
                    _resultMapTabSelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _innerScreenView;
        public IMainScreenView InnerScreenView
        {
            get { return _innerScreenView; }
            set
            {
                if (value != _innerScreenView)
                {
                    _innerScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _preinnerScreenView;
        public IMainScreenView PreinnerScreenView
        {
            get { return _preinnerScreenView; }
            set
            {
                if (value != _preinnerScreenView)
                {
                    _preinnerScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<int, IMainScreenView> _innserScreenViewDictionary = new Dictionary<int, IMainScreenView>();
        public Dictionary<int, IMainScreenView> InnserScreenViewDictionary
        {
            get { return _innserScreenViewDictionary; }
            set
            {
                if (value != _innserScreenViewDictionary)
                {
                    _innserScreenViewDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

        public void DeInitModule()
        {
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = InitializeInnerScreen();

                ResultMapTabSelectedIndex = -1;
                InnerScreenView = null;
                PreinnerScreenView = null;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }


        private EventCodeEnum InitializeInnerScreen()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (InnserScreenViewDictionary == null)
                {
                    InnserScreenViewDictionary = new Dictionary<int, IMainScreenView>();
                }

                // (1) STIF
                var guid = GetGuid(typeof(STIFMapAnalyzeView));

                IMainScreenView viewobj = this.ViewModelManager().GetViewObj(guid).Result;

                if (viewobj != null)
                {
                    InnserScreenViewDictionary.Add(0, viewobj);
                }

                // (2) E142
                guid = GetGuid(typeof(E142MapAnalyzeView));

                viewobj = this.ViewModelManager().GetViewObj(guid).Result;

                if (viewobj != null)
                {
                    InnserScreenViewDictionary.Add(1, viewobj);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private RelayCommand<object> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand) _SelectedItemChangedCommand = new RelayCommand<object>(SelectedIndexChangedFunc);
                return _SelectedItemChangedCommand;
            }
        }

        public void SelectedIndexChangedFunc(object param)
        {
            try
            {
                if (InnserScreenViewDictionary != null && InnserScreenViewDictionary.Count > 0)
                {
                    bool isExist = false;

                    isExist = InnserScreenViewDictionary.TryGetValue(ResultMapTabSelectedIndex, out var tmpInnerScreen);

                    if(isExist == true)
                    {
                        if(tmpInnerScreen != null)
                        {
                            // TODO : 페이지 변경 시 필요한 것?

                            // CurrentView : Cleanup()

                            if(InnerScreenView != null)
                            {
                                var prevm = this.ViewModelManager().GetViewModelFromViewGuid(InnerScreenView.ScreenGUID);

                                if(prevm != null)
                                {
                                    prevm.Cleanup();
                                }
                                else
                                {
                                    LoggerManager.Error($"[ResultMapAnalyzeViewerVM], SelectedIndexChangedFunc() : prevm is null.");
                                }
                            }

                            var changevm = this.ViewModelManager().GetViewModelFromViewGuid(tmpInnerScreen.ScreenGUID);

                            if(changevm != null)
                            {
                                changevm.PageSwitched();
                            }
                            else
                            {
                                LoggerManager.Error($"[ResultMapAnalyzeViewerVM], SelectedIndexChangedFunc() : changevm is null.");
                            }

                            InnerScreenView = tmpInnerScreen;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Guid GetGuid(Type type)
        {
            Guid retval = default(Guid);

            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var tmpobj = Activator.CreateInstance(type);
                    retval = (tmpobj as IScreenGUID).ScreenGUID;
                });
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
