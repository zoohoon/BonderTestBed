using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCodeEditor
{
    using Autofac;
    using EventCodeEditor.GEM;
    using GEMModule;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderMaster;
    using LogModule;
    using NotifyModule.Parameter;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public class MainWindowViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Property
        private Autofac.IContainer Container;
        private IFoupOpModule Foup => Container.Resolve<IFoupOpModule>();
        private IGEMModule GemModule => Container.Resolve<IGEMModule>();
        private ILoaderCommunicationManager LoaderCommunicationManager => Container.Resolve<ILoaderCommunicationManager>();
        private LoaderSupervisor Master => (LoaderSupervisor)Container.Resolve<ILoaderSupervisor>();

        private List<string> _StageList = new List<string>(12)
        { "CELL1", "CELL2" , "CELL3" , "CELL4" , "CELL5" , "CELL6" , "CELL7" , "CELL8" , "CELL9" , "CELL10" , "CELL11" , "CELL12" };
        public List<string> StageList
        {
            get { return _StageList; }
            set
            {
                if (value != _StageList)
                {
                    _StageList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedStageIndex;
        public int SelectedStageIndex
        {
            get { return _SelectedStageIndex; }
            set
            {
                if (value != _SelectedStageIndex)
                {
                    _SelectedStageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<EventCodeEnum> _EventCodeEnumList;
        public ObservableCollection<EventCodeEnum> EventCodeEnumList
        {
            get { return _EventCodeEnumList; }
            set
            {
                if (value != _EventCodeEnumList)
                {
                    _EventCodeEnumList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Array _NotifyCodeEnumList;
        public Array NotifyCodeEnumList
        {
            get { return _NotifyCodeEnumList; }
            set
            {
                if (value != _NotifyCodeEnumList)
                {
                    _NotifyCodeEnumList = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Array _PoberErrorKindList;
        public Array PoberErrorKindList
        {
            get { return _PoberErrorKindList; }
            set
            {
                if (value != _PoberErrorKindList)
                {
                    _PoberErrorKindList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProberErrorKind _SelectedProberErrorKind;
        public EnumProberErrorKind SelectedProberErrorKind
        {
            get { return _SelectedProberErrorKind; }
            set
            {
                if (value != _SelectedProberErrorKind)
                {
                    _SelectedProberErrorKind = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Array _PrologTypeList;
        public Array PrologTypeList
        {
            get { return _PrologTypeList; }
            set
            {
                if (value != _PrologTypeList)
                {
                    _PrologTypeList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PrologType _SelectedPrologType;
        public PrologType SelectedPrologType
        {
            get { return _SelectedPrologType; }
            set
            {
                if (value != _SelectedPrologType)
                {
                    _SelectedPrologType = value;
                    RaisePropertyChanged();
                }
            }
        }



        private EventCodeEnum _SelectedEventCodeEnum;
        public EventCodeEnum SelectedEventCodeEnum
        {
            get { return _SelectedEventCodeEnum; }
            set
            {
                if (value != _SelectedEventCodeEnum)
                {
                    _SelectedEventCodeEnum = value;
                    ClearParamInfo();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// UI 의 정보 초기화
        /// </summary>
        private void ClearParamInfo()
        {
            SelectedProberErrorKind = EnumProberErrorKind.INVALID;
            SelectedPrologType = PrologType.UNDEFINED;
            AlarmMessageTitle = "";
            AlarmMessageContent = "";
            GemAlarmID = "";
            NotifyMessageDialogEnableFlag = false;
            NotifyEventLogEnableFlag = false;
            NotifyProLogEnableFlag = false;
        }


        private ObservableCollection<EventCodeParam> _NotifyEventCodeParam;
        public ObservableCollection<EventCodeParam> NotifyEventCodeParam
        {
            get { return _NotifyEventCodeParam; }
            set
            {
                if (value != _NotifyEventCodeParam)
                {
                    _NotifyEventCodeParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EventCodeParam _SelectedEventCodeParam;
        public EventCodeParam SelectedEventCodeParam
        {
            get { return _SelectedEventCodeParam; }
            set
            {
                if (value != _SelectedEventCodeParam)
                {
                    _SelectedEventCodeParam = value;
                    ChangeGemAlarmInfo();
                    RaisePropertyChanged();
                }
            }
        }

        private void ChangeGemAlarmInfo()
        {
            try
            {
                if (SelectedEventCodeParam != null)
                {
                    SelectedProberErrorKind = SelectedEventCodeParam.ProberErrorKind;
                    SelectedPrologType = SelectedEventCodeParam.ProLogType;
                    AlarmMessageTitle = SelectedEventCodeParam.Title;
                    AlarmMessageContent = SelectedEventCodeParam.Message;
                    GemAlarmID = SelectedEventCodeParam.GemAlaramNumber.ToString();
                    NotifyMessageDialogEnableFlag = SelectedEventCodeParam.EnableNotifyMessageDialog;
                    NotifyEventLogEnableFlag = SelectedEventCodeParam.EnableNotifyEventlog;
                    NotifyProLogEnableFlag = SelectedEventCodeParam.EnableNotifyProlog;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum _SelectedFromErrorCodeParamList;
        public EventCodeEnum SelectedFromErrorCodeParamList
        {
            get { return _SelectedFromErrorCodeParamList; }
            set
            {
                if (value != _SelectedFromErrorCodeParamList)
                {
                    _SelectedFromErrorCodeParamList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EventCodeEnum _SelectedGemAlarmParam;
        public EventCodeEnum SelectedGemAlarmParam
        {
            get { return _SelectedGemAlarmParam; }
            set
            {
                if (value != _SelectedGemAlarmParam)
                {
                    _SelectedGemAlarmParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _AlarmMessageTitle;
        public string AlarmMessageTitle
        {
            get { return _AlarmMessageTitle; }
            set
            {
                if (value != _AlarmMessageTitle)
                {
                    _AlarmMessageTitle = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _AlarmMessageContent;
        public string AlarmMessageContent
        {
            get { return _AlarmMessageContent; }
            set
            {
                if (value != _AlarmMessageContent)
                {
                    _AlarmMessageContent = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _GemAlarmID;
        public string GemAlarmID
        {
            get { return _GemAlarmID; }
            set
            {
                if (value != _GemAlarmID)
                {
                    _GemAlarmID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _NotifyMessageDialogEnableFlag;
        public bool NotifyMessageDialogEnableFlag
        {
            get { return _NotifyMessageDialogEnableFlag; }
            set
            {
                if (value != _NotifyMessageDialogEnableFlag)
                {
                    _NotifyMessageDialogEnableFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _NotifyEventLogEnableFlag;
        public bool NotifyEventLogEnableFlag
        {
            get { return _NotifyEventLogEnableFlag; }
            set
            {
                if (value != _NotifyEventLogEnableFlag)
                {
                    _NotifyEventLogEnableFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _NotifyProLogEnableFlag;
        public bool NotifyProLogEnableFlag
        {
            get { return _NotifyProLogEnableFlag; }
            set
            {
                if (value != _NotifyProLogEnableFlag)
                {
                    _NotifyProLogEnableFlag = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _ErrorCodeSelectedIndex;
        public int ErrorCodeSelectedIndex
        {
            get { return _ErrorCodeSelectedIndex; }
            set
            {
                if (value != _ErrorCodeSelectedIndex)
                {
                    _ErrorCodeSelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _NotifyCodeSelectedIndex;
        public int NotifyCodeSelectedIndex
        {
            get { return _NotifyCodeSelectedIndex; }
            set
            {
                if (value != _NotifyCodeSelectedIndex)
                {
                    _NotifyCodeSelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StageListEnabled;
        public bool StageListEnabled
        {
            get { return _StageListEnabled; }
            set
            {
                if (value != _StageListEnabled)
                {
                    _StageListEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Creator & Init
        public MainWindowViewModel()
        {
            Container = this.GetContainer();

            var array = Enum.GetValues(typeof(EventCodeEnum));

            if (EventCodeEnumList == null)
            {
                EventCodeEnumList = new ObservableCollection<EventCodeEnum>();
            }

            foreach (var item in array)
            {
                EventCodeEnumList.Add((EventCodeEnum)item);
            }

            PoberErrorKindList = Enum.GetValues(typeof(EnumProberErrorKind));
            PrologTypeList = Enum.GetValues(typeof(PrologType));
        }

        public void PageSwitched()
        {
            if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                StageListEnabled = false;
            else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                StageListEnabled = true;

            NotifyEventCodeParam = (this.NotifyManager().NotifySysParam as NotifySystemParameter).NoticeEventCodeParam;
        }
        #endregion

        #region Command & Method

        private void UpdateSelectedInfo(int errorcodeIndex = -1, int notifycodeIndex = -1)
        {
            if (errorcodeIndex != -1)
            {
                var errorCodeInfo = NotifyEventCodeParam[errorcodeIndex];
                GemAlarmID = errorCodeInfo.Message;
            }
            else if (notifycodeIndex != -1)
            {

            }
        }

        #region Delete Event Code Param Command

        private RelayCommand _AddEventrCodeParamCommand;
        public ICommand AddEventCodeParamCommand
        {
            get
            {
                if (null == _AddEventrCodeParamCommand) _AddEventrCodeParamCommand = new RelayCommand(AddEventCodeParamCommandFunc);
                return _AddEventrCodeParamCommand;
            }
        }
        EventCodeParam newParam = new EventCodeParam();
        private void AddEventCodeParamCommandFunc()
        {
            var eventCodeParam = NotifyEventCodeParam.Where(param => param.EventCode == SelectedEventCodeEnum).FirstOrDefault();
            if (eventCodeParam == null)
            {
                newParam = new EventCodeParam();
                newParam.EventCode = SelectedEventCodeEnum;
                newParam.Title = AlarmMessageTitle;
                newParam.Message = AlarmMessageContent;
                newParam.GemAlaramNumber = Convert.ToInt64(GemAlarmID);
                newParam.EnableNotifyMessageDialog = NotifyMessageDialogEnableFlag;
                newParam.EnableNotifyEventlog = NotifyEventLogEnableFlag;
                newParam.EnableNotifyProlog = NotifyProLogEnableFlag;
                newParam.ProberErrorKind = SelectedProberErrorKind;
                newParam.ProLogType = SelectedPrologType;
                NotifyEventCodeParam.Add(newParam);
            }
        }


        private RelayCommand _ModifyEventCodeParamCommand;
        public ICommand ModifyEventCodeParamCommand
        {
            get
            {
                if (null == _ModifyEventCodeParamCommand) _ModifyEventCodeParamCommand = new RelayCommand(ModifyEventCodeParamCommandFunc);
                return _ModifyEventCodeParamCommand;
            }
        }
        private void ModifyEventCodeParamCommandFunc()
        {
            var eventCodeParam = NotifyEventCodeParam.Where(param => param.EventCode == SelectedEventCodeParam.EventCode).FirstOrDefault();
            if (eventCodeParam != null)
            {
                eventCodeParam.Title = AlarmMessageTitle;
                eventCodeParam.Message = AlarmMessageContent;
                eventCodeParam.GemAlaramNumber = Convert.ToInt64(GemAlarmID);
                eventCodeParam.EnableNotifyMessageDialog = NotifyMessageDialogEnableFlag;
                eventCodeParam.EnableNotifyEventlog = NotifyEventLogEnableFlag;
                eventCodeParam.EnableNotifyProlog = NotifyProLogEnableFlag;
                eventCodeParam.ProberErrorKind = SelectedProberErrorKind;
                eventCodeParam.ProLogType = SelectedPrologType;
            }
        }


        #endregion

        #region Delete Event Code Param Command

        private RelayCommand _DeleteEventCodeParamCommand;
        public ICommand DeleteEventCodeParamCommand
        {
            get
            {
                if (null == _DeleteEventCodeParamCommand) _DeleteEventCodeParamCommand = new RelayCommand(DeleteEventCodeParamCommandFunc);
                return _DeleteEventCodeParamCommand;
            }
        }

        private void DeleteEventCodeParamCommandFunc()
        {
            if (ErrorCodeSelectedIndex != -1)
            {
                NotifyEventCodeParam.RemoveAt(ErrorCodeSelectedIndex);
            }
        }

        public void DeleteGemAlarmParameter()
        {
            var gemModule = Container.Resolve<IGEMModule>();
            if (NotifyCodeSelectedIndex != -1)
            {
                //GemAlarmParam.RemoveAt(NotifyCodeSelectedIndex);
                gemModule.SaveSysParameter();
            }
        }

        public void DeleteErrorCodeParameter()
        {
            if (ErrorCodeSelectedIndex != -1)
            {
                NotifyEventCodeParam.RemoveAt(ErrorCodeSelectedIndex);
            }
        }

        #endregion

        #region Stage Raise Event Code Command 

        private AsyncCommand _StageRaiseEventCodeCommand;
        public ICommand StageRaiseEventCodeCommand
        {
            get
            {
                if (null == _StageRaiseEventCodeCommand) _StageRaiseEventCodeCommand = new AsyncCommand(StageRaiseEventCodeCommandFunc);
                return _StageRaiseEventCodeCommand;
            }
        }

        private Task StageRaiseEventCodeCommandFunc()
        {
            if (ErrorCodeSelectedIndex != -1)
            {
                //var client = Master.GetClient(SelectedStageIndex + 1);
                var client = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(SelectedStageIndex + 1);
                if(client != null)
                {
                    client.SetErrorCodeAlarm(SelectedEventCodeEnum);
                }
                
            }
            return Task.CompletedTask;
        }


        #endregion

        #region Loader Raise Event Code Command
        private AsyncCommand _LoaderRaiseEventCodeCommand;
        public ICommand LoaderRaiseEventCodeCommand
        {
            get
            {
                if (null == _LoaderRaiseEventCodeCommand) _LoaderRaiseEventCodeCommand = new AsyncCommand(LoaderRaiseEventCodeCommandFunc);
                return _LoaderRaiseEventCodeCommand;
            }
        }

        private Task LoaderRaiseEventCodeCommandFunc()
        {
            this.NotifyManager().Notify(SelectedEventCodeEnum);
            return Task.CompletedTask;
        }
        #endregion

        #region Download Param To Stage Command
        private RelayCommand _DownloadParamToStageCommand;
        public ICommand DownloadParamToStageCommand
        {
            get
            {
                if (null == _DownloadParamToStageCommand) _DownloadParamToStageCommand = new RelayCommand(DownloadParamToStageCommandFunc);
                return _DownloadParamToStageCommand;
            }
        }

        private void DownloadParamToStageCommandFunc()
        {
            
        }
        #endregion

        #region Save Param Command

        private RelayCommand _SaveParamCommand;
        public ICommand SaveParamCommand
        {
            get
            {
                if (null == _SaveParamCommand) _SaveParamCommand = new RelayCommand(SaveParamCommandFunc);
                return _SaveParamCommand;
            }
        }

        private void SaveParamCommandFunc()
        {
            (this.NotifyManager().NotifySysParam as NotifySystemParameter).NoticeEventCodeParam = NotifyEventCodeParam;
            this.NotifyManager().SaveSysParameter();
            this.NotifyManager().LoadSysParameter();

            GEMAlarmParameter gEMAlarmParam = this.GemModule.GemAlarmSysParam as GEMAlarmParameter;

            foreach (var param in NotifyEventCodeParam)
            {
                if(param.GemAlaramNumber != 0 & (gEMAlarmParam.GemAlramInfos.Where(info => info.AlaramID == param.GemAlaramNumber).FirstOrDefault() == null))
                {
                    gEMAlarmParam.GemAlramInfos.Add(new GemAlarmDiscriptionParam() { AlaramID = param.GemAlaramNumber });
                }
            }

            this.GemModule.SaveSysParameter();
        }

        #endregion

        #region Show Gem Alaram Window

        private RelayCommand _ShowGemAlarmSetupWindowCommand;
        public ICommand ShowGemAlarmSetupWindowCommand
        {
            get
            {
                if (null == _ShowGemAlarmSetupWindowCommand) _ShowGemAlarmSetupWindowCommand = new RelayCommand(ShowGemAlarmSetypWindowFunc);
                return _ShowGemAlarmSetupWindowCommand;
            }
        }
        private GemAlarmSetupViewModel GemAlarmSetupViewModel = new GemAlarmSetupViewModel();
        private void ShowGemAlarmSetypWindowFunc()
        {
            GemAlarmSetupViewModel.PageSwitched();
            Window window = new Window
            {
                Title = "Gem Alarm Setup Window",
                Content = new GemAlarmSetupView() { DataContext = GemAlarmSetupViewModel},
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };
            window.ShowDialog();

            //return Task.FromResult(0);
        }
        #endregion

        //private void ErrorCodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    TBAlarmID = "";
        //    TBErrCodeMSG = "";
        //    TBAlarmID = "";
        //}

        #endregion

    }
}
