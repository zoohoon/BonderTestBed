using System;
using System.Linq;

namespace SecsGemSettingDialogVM
{
    using EventProcessModule.GEM;
    using GEMModule;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Event.EventProcess;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class SecsGemEventSettingDialogViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region <PropertyChanged>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region <Property>


        public GEM GemModule { get; set; }

        private ObservableCollection<EventProcessBase> _GemEventProcessList = new ObservableCollection<EventProcessBase>();
        public ObservableCollection<EventProcessBase> GemEventProcessList
        {
            get { return _GemEventProcessList; }
            set
            {
                if (value != _GemEventProcessList)
                {
                    _GemEventProcessList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EventProcessBase _SelectedGemEventProcess;
        public EventProcessBase SelectedGemEventProcess
        {
            get { return _SelectedGemEventProcess; }
            set
            {
                if (value != _SelectedGemEventProcess)
                {
                    _SelectedGemEventProcess = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<NotifyEventInfo> _OriginNotifyEventInfos;
        public ObservableCollection<NotifyEventInfo> OriginNotifyEventInfos
        {
            get { return _OriginNotifyEventInfos; }
            set
            {
                if (value != _OriginNotifyEventInfos)
                {
                    _OriginNotifyEventInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NotifyEventInfo _SelectedNotifyEventInfo;
        public NotifyEventInfo SelectedNotifyEventInfo
        {
            get { return _SelectedNotifyEventInfo; }
            set
            {
                if (value != _SelectedNotifyEventInfo)
                {
                    _SelectedNotifyEventInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        public SecsGemEventSettingDialogViewModel()
        {
            try
            {
                InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitViewModel()
        {
            try
            {
                GemModule = this.GEMModule() as GEM;
                GemEventProcessList.Clear();
                foreach (var param in GemModule.SubscribeRecipeParam)
                {
                    GemEventProcessList.Add(new GemEventProc_EventMessageSet() { EventFullName = param.EventFullName, Parameter = param.Parameter });
                }

                OriginNotifyEventInfos = new ObservableCollection<NotifyEventInfo>();
                //String strFolder = System.IO.Directory.GetCurrentDirectory();
                String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                //string notifyEventDllPath = strFolder + "\\NotifyEvent.dll";
                string notifyEventDllPath = Path.Combine(strFolder, "NotifyEvent.dll");

                var assembly = Assembly.LoadFrom(notifyEventDllPath);

                if (!assembly.IsDynamic)
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        var ret = GemEventProcessList.Where(processinfo => processinfo.EventFullName.Equals(type.FullName));

                        if (ret.Count() != 0)
                        {
                            OriginNotifyEventInfos.Add(new NotifyEventInfo(type.FullName, false));
                        }
                        else
                        {
                            OriginNotifyEventInfos.Add(new NotifyEventInfo(type.FullName, true));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region < Add $ Delete Event Command >
        private RelayCommand _AddEventCommanad;
        public ICommand AddEventCommanad
        {
            get
            {
                if (null == _AddEventCommanad) _AddEventCommanad = new RelayCommand(AddEventCommanadFunc);
                return _AddEventCommanad;
            }
        }

        public void AddEventCommanadFunc()
        {
            try
            {
                if (SelectedNotifyEventInfo != null)
                {
                    var eventInfo = GemEventProcessList.Where(infos => infos.EventFullName.Equals(SelectedNotifyEventInfo.EventName));
                    if (eventInfo.Count() == 0)
                    {
                        GemEventProcessList.Add(new GemEventProc_EventMessageSet() { EventFullName = SelectedNotifyEventInfo.EventName, Parameter = -1, Enable = true });
                        SelectedNotifyEventInfo.Enable = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _DeleteEventCommanad;
        public ICommand DeleteEventCommanad
        {
            get
            {
                if (null == _DeleteEventCommanad) _DeleteEventCommanad = new RelayCommand(DeleteEventCommanadFunc);
                return _DeleteEventCommanad;
            }
        }

        public void DeleteEventCommanadFunc()
        {
            try
            {
                if (SelectedGemEventProcess != null)
                {
                    var eventInfo = OriginNotifyEventInfos.Where(infos => infos.EventName.Equals(SelectedGemEventProcess.EventFullName));
                    if (eventInfo.Count() == 1)
                    {
                        eventInfo.First().Enable = true;
                        GemEventProcessList.Remove(SelectedGemEventProcess);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion


        #region < Save Parameter Command >

        private RelayCommand _SaveParameterCommand;
        public ICommand SaveParameterCommand
        {
            get
            {
                if (null == _SaveParameterCommand) _SaveParameterCommand = new RelayCommand(SaveParameterCommandFunc);
                return _SaveParameterCommand;
            }
        }

        public void SaveParameterCommandFunc()
        {
            try
            {
                EventProcessList newGemEventProcessList = new EventProcessList();
                foreach (var item in GemEventProcessList)
                {
                    newGemEventProcessList.Add(item);
                }

                GemModule.SubscribeRecipeParam = newGemEventProcessList;
                GemModule.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region < Load Parameter Command >

        private RelayCommand _LoadParameterCommand;
        public ICommand LoadParameterCommand
        {
            get
            {
                if (null == _LoadParameterCommand) _LoadParameterCommand = new RelayCommand(LoadParameterCommandFunc);
                return _LoadParameterCommand;
            }
        }

        public void LoadParameterCommandFunc()
        {
            try
            {
                InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

    }

    public class NotifyEventInfo : INotifyPropertyChanged
    {
        #region <PropertyChanged>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private string _EventName;
        public string EventName
        {
            get { return _EventName; }
            set
            {
                if (value != _EventName)
                {
                    _EventName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NotifyEventInfo(string eventname, bool enable)
        {
            this.EventName = eventname;
            this.Enable = enable;
        }
    }
}
