using Autofac;
using ControlzEx.Standard;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LogModule;
using LogModule.LoggerParam;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Utility;
using RelayCommandBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UcAnimationScrollViewer;

namespace AlarmViewDialog
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(CellInfo.Index);
        public MainWindow()
        {
            InitializeComponent();
        }

        private ICellInfo _CellInfo;
        public ICellInfo CellInfo
        {
            get { return _CellInfo; }
            set
            {
                if (value != _CellInfo)
                {
                    _CellInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ICellInfo> _CellInfos = new ObservableCollection<ICellInfo>();
        public ObservableCollection<ICellInfo> CellInfos
        {
            get { return _CellInfos; }
            set
            {
                if (value != _CellInfos)
                {
                    _CellInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private static bool _CheckAtCloseToggle;
        public bool CheckAtCloseToggle
        {
            get { return _CheckAtCloseToggle; }
            set
            {
                if (value != _CheckAtCloseToggle)
                {
                    _CheckAtCloseToggle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICellInfo _ToViewHistory;
        public ICellInfo ToViewHistory
        {
            get { return _ToViewHistory; }
            set
            {
                if (value != _ToViewHistory)
                {
                    _ToViewHistory = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LogPathInfo> _LogPathInfos;
        public ObservableCollection<LogPathInfo> LogPathInfos
        {
            get { return _LogPathInfos; }
            set
            {
                if (value != _LogPathInfos)
                {
                    _LogPathInfos = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableDictionary<string, string> _ImageLogPathInfos;
        public ObservableDictionary<string, string> ImageLogPathInfos
        {
            get { return _ImageLogPathInfos; }
            set
            {
                if (value != _ImageLogPathInfos)
                {
                    _ImageLogPathInfos = value;
                    RaisePropertyChanged();
                }
            }
        }



        public void ShowAlarmsView()
        {
            for (int i = CellInfo.ErrorCodeAlarams.Count; i > 0; i--)
            {
                if (CellInfo.ErrorCodeAlarams[i - 1].IsChecked == true)
                {
                    CellInfo.ErrorCodeAlarams.Remove(CellInfo.ErrorCodeAlarams[i - 1]);
                }
            }
        }

        public MainWindow(ICellInfo cellinfo) : this()
        {
            try
            {
                //CellInfo.Add(cellinfo.ErrorCodeAlarams);
                CellInfo = cellinfo;
                ShowAlarmsView();

                DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public MainWindow(ObservableCollection<ICellInfo> cellinfos) : this()
        {
            try
            {
                CellInfos = cellinfos;
                DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Command
        private RelayCommand _CloseWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                if (null == _CloseWindowCommand) _CloseWindowCommand = new RelayCommand(CloseWindowCommandFunc);
                return _CloseWindowCommand;
            }
        }

        private void CloseWindowCommandFunc()
        {
            try
            {
                if (CheckAtCloseToggle == true)
                {
                    AllAlramCheckedCommandFunc();
                }

                LoaderVMManager.UpdateLoaderAlarmCount();
                this.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _AllAlramCheckedCommand;
        public ICommand AllAlramCheckedCommand
        {
            get
            {
                if (null == _AllAlramCheckedCommand) _AllAlramCheckedCommand = new RelayCommand(AllAlramCheckedCommandFunc);
                return _AllAlramCheckedCommand;
            }
        }

        private void AllAlramCheckedCommandFunc()
        {
            try
            {
                foreach (var alram in CellInfo.ErrorCodeAlarams)
                {
                    if (!alram.IsChecked)
                    {
                        alram.IsChecked = true;
                    }
                }

                CellInfo.AlarmMessageNotNotifiedCount = 0;

                LoaderVMManager.UpdateLoaderAlarmCount();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int _SelectedAlaramIndex = -1;
        public int SelectedAlaramIndex
        {
            get { return _SelectedAlaramIndex; }
            set
            {
                if (value != _SelectedAlaramIndex)
                {
                    _SelectedAlaramIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UiListView_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CellInfo.ErrorCodeAlarams.Count == 0 | CellInfo.ErrorCodeAlarams.Count < SelectedAlaramIndex | SelectedAlaramIndex == -1)
                    return;

                if (!CellInfo.ErrorCodeAlarams[SelectedAlaramIndex].IsChecked)
                {
                    CellInfo.ErrorCodeAlarams[SelectedAlaramIndex].IsChecked = true;
                    CellInfo.AlarmMessageNotNotifiedCount--;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _HistoryAlaramCommand;
        public ICommand HistoryAlaramCommand
        {
            get
            {
                if (null == _HistoryAlaramCommand) _HistoryAlaramCommand = new RelayCommand(HistoryAlaramCommandFunc);
                return _HistoryAlaramCommand;
            }
        }

        private AsyncCommand _UpdateLogCommand;
        public ICommand UpdateLogCommand
        {
            get
            {
                if (null == _UpdateLogCommand) _UpdateLogCommand = new AsyncCommand(UpdateLogCommandFunc);
                return _UpdateLogCommand;
            }
        }

        private AsyncCommand _OpenFileCommand;
        public ICommand OpenFileCommand
        {
            get
            {
                if (null == _OpenFileCommand) _OpenFileCommand = new AsyncCommand(OpenFileCommandFunc);
                return _OpenFileCommand;
            }
        }

        private AsyncCommand _RefrashFileCommand;
        public ICommand RefrashFileCommand
        {
            get
            {
                if (null == _RefrashFileCommand) _RefrashFileCommand = new AsyncCommand(RefrashFileCommandFunc);
                return _RefrashFileCommand;
            }
        }

        private RelayCommand _SaveLogPathSettingCommand;
        public ICommand SaveLogPathSettingCommand
        {
            get
            {
                if (null == _SaveLogPathSettingCommand) _SaveLogPathSettingCommand = new RelayCommand(SaveLogPathSettingCommandFunc);
                return _SaveLogPathSettingCommand;
            }
        }

        private bool _HistorySwitch;

        public bool HistorySwitch
        {
            get { return _HistorySwitch; }
            set
            {
                _HistorySwitch = value;
                RaisePropertyChanged();
            }
        }

        private bool _LogSwitch;

        public bool LogSwitch
        {
            get { return _LogSwitch; }
            set
            {
                _LogSwitch = value;
                RaisePropertyChanged();
            }
        }

        private List<string> _ComboItemSource = new List<string>();

        public List<string> ComboItemSource
        {
            get { return _ComboItemSource; }
            set
            {
                _ComboItemSource = value;
                RaisePropertyChanged();
            }
        }

        private List<List<string>> _LogFileList = new List<List<string>>();
        public List<List<string>> LogFileList
        {
            get { return _LogFileList; }
            set
            {
                if (value != _LogFileList)
                {
                    _LogFileList = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _CurrentSelection;

        public string CurrentSelection
        {
            get { return _CurrentSelection; }
            set
            {
                _CurrentSelection = value;
                RaisePropertyChanged();
            }
        }
        private string _SelectedLogFilePath;
        public string SelectedLogFilePath
        {
            get { return _SelectedLogFilePath; }
            set
            {
                if (value != _SelectedLogFilePath)
                {
                    _SelectedLogFilePath = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _SelectedLogFileName;
        public string SelectedLogFileName
        {
            get { return _SelectedLogFileName; }
            set
            {
                if (value != _SelectedLogFileName)
                {
                    _SelectedLogFileName = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand<object> _SelectedLogChangedCommand;
        public ICommand SelectedLogChangedCommand
        {
            get
            {
                if (null == _SelectedLogChangedCommand) _SelectedLogChangedCommand = new AsyncCommand<object>(SelectedLogChangedCommandFunc);
                return _SelectedLogChangedCommand;
            }
        }
        private DateTime _StartDate = DateTime.Today.AddDays(-7);
        public DateTime StartDate
        {
            get { return _StartDate; }
            set
            {
                if (value != _StartDate)
                {
                    _StartDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _EndDate = DateTime.Today;
        public DateTime EndDate
        {
            get { return _EndDate; }
            set
            {
                if (value != _EndDate)
                {
                    _EndDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _PeriodFilterCommand;
        public ICommand PeriodFilterCommand
        {
            get
            {
                if (null == _PeriodFilterCommand) _PeriodFilterCommand = new AsyncCommand(PeriodFilterFunc);
                return _PeriodFilterCommand;
            }
        }

        private Task SelectedLogChangedCommandFunc(object obj)
        {
            IList items = obj as IList;

            try
            {
                if (items != null)
                {
                    if (items.Count == 0)
                    {
                        return Task.CompletedTask;
                    }

                    LogTransferList logTransfer = items[items.Count - 1] as LogTransferList;

                    SelectedLogFilePath = logTransfer.LogFilePath;
                    SelectedLogFileName = logTransfer.LogFileName;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }
        public Task PeriodFilterFunc()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DateTime LogDateTime;

                    if (StartDatePicker.SelectedDate.HasValue)
                    {
                        EndDatePicker.DisplayDateStart = StartDatePicker.SelectedDate.Value;
                    }

                    logTransferLists.Clear();

                    for (int i = 0; i < LogFileList.Count; i++)
                    {
                        if (LogFileList[i][0] == CurrentSelection)
                        {
                            for (int j = 1; j < LogFileList[i].Count; j++)
                            {
                                string[] nameSplit = LogFileList[i][j].Split(new string[] { "\\" }, StringSplitOptions.None);
                                string[] dateSplit = nameSplit[nameSplit.Length - 1].Split(new string[] { "_" }, StringSplitOptions.None);
                                Match match = Regex.Match(nameSplit[nameSplit.Length - 1], @"\d{4}\-\d{2}\-\d{2}");
                                string date = match.Value;
                                if (!string.IsNullOrEmpty(date))
                                {
                                    LogDateTime = DateTime.Parse(date);
                                    if (LogDateTime >= StartDate && LogDateTime <= EndDate)
                                    {
                                        logTransferLists.Add(new LogTransferList()
                                        {
                                            LogFileName = nameSplit[nameSplit.Length - 1],
                                            LogFilePath = LogFileList[i][j]
                                        });
                                    }
                                }
                            }
                        }
                    }
                    uiLogListView.ItemsSource = logTransferLists;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        ObservableCollection<LogTransferList> logTransferLists = new ObservableCollection<LogTransferList>();
        private async Task UpdateLogCommandFunc()
        {
            try
            {
                LogSwitch = !LogSwitch;

                if (LogSwitch == true)
                {
                    Task task = new Task(() =>
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                 {
                                     #region UI Visible Hidden
                                     DismissBtn.Visibility = Visibility.Hidden;
                                     HistoryListviewBtn.Visibility = Visibility.Hidden;
                                     svAlarmView.Visibility = Visibility.Hidden;
                                     DismissTb.Visibility = Visibility.Hidden;
                                     HistoryTb.Visibility = Visibility.Hidden;
                                     CheckCloseToggle.Visibility = Visibility.Hidden;
                                     CheckCloseToggleTb.Visibility = Visibility.Hidden;
                                     svLogView.Visibility = Visibility.Visible;
                                     OpenFileBtn.Visibility = Visibility.Visible;
                                     OpenFileTb.Visibility = Visibility.Visible;
                                     RefrashFileBtn.Visibility = Visibility.Visible;
                                     RefrashFileTb.Visibility = Visibility.Visible;
                                     LogTypeCb.Visibility = Visibility.Visible;
                                     LogTypeCb.Text = "Select Log Type";
                                     StartDatePicker.Visibility = Visibility.Visible;
                                     EndDatePicker.Visibility = Visibility.Visible;
                                     #endregion

                                 });
                    });
                    task.Start();
                    await task;

                    LogFileList = new List<List<string>>();
                    LogFileList = await _RemoteMediumProxy.LogTransfer_UpdateLogFile();

                    Task t1 = new Task(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            for (int i = 0; i < LogFileList.Count; i++)
                            {
                                ComboItemSource.Add(LogFileList[i][0]);
                            }
                        });
                    });
                    t1.Start();
                    await t1;
                }
                else
                {
                    Task t1 = new Task(() =>
                    {
                        Application.Current.Dispatcher.Invoke(
                        () =>
                        {
                            #region UI Visible Visible
                            DismissBtn.Visibility = Visibility.Visible;
                            HistoryListviewBtn.Visibility = Visibility.Visible;
                            svAlarmView.Visibility = Visibility.Visible;
                            DismissTb.Visibility = Visibility.Visible;
                            HistoryTb.Visibility = Visibility.Visible;
                            CheckCloseToggle.Visibility = Visibility.Visible;
                            CheckCloseToggleTb.Visibility = Visibility.Visible;
                            svLogView.Visibility = Visibility.Hidden;
                            OpenFileBtn.Visibility = Visibility.Hidden;
                            OpenFileTb.Visibility = Visibility.Hidden;
                            RefrashFileBtn.Visibility = Visibility.Hidden;
                            RefrashFileTb.Visibility = Visibility.Hidden;
                            LogTypeCb.Visibility = Visibility.Hidden;
                            StartDatePicker.Visibility = Visibility.Hidden;
                            EndDatePicker.Visibility = Visibility.Hidden;
                            #endregion

                            ComboItemSource.Clear();
                            logTransferLists.Clear();
                        });
                    });
                    t1.Start();
                    await t1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetLogPathSetting(string header)
        {
            try
            {
                if (header.Equals("SETTING"))
                {
                    ///LogPathInfos ( 로그 파일 경로 정보 ) 
                    if (LogPathInfos == null)
                    {
                        LogPathInfos = new ObservableCollection<LogPathInfo>();
                    }
                    else
                    {
                        LogPathInfos.Clear();
                    }

                    var logPathInfos = _RemoteMediumProxy.GetLogPathInfos();

                    if (logPathInfos != null)
                    {
                        foreach (var info in logPathInfos)
                        {
                            LogPathInfos.Add(new LogPathInfo(info.Key, info.Value));
                        }
                    }
                    else
                    {
                        LoggerManager.Debug("RemoteMediumProxy.GetLogPathInfos() return null");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task OpenFileCommandFunc()
        {
            try
            {
                var fileInfo = await RetreiveFile();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task<FileInfo> RetreiveFile()
        {
            FileInfo fileInfo = null;

            try
            {
                byte[] compressedFile = null;
                var savefilepath = LoggerManager.LoggerManagerParam.FilePath + @"\LogTransfer\CELL" + CellInfo.Index + @"\" + CurrentSelection;

                if (!Directory.Exists(savefilepath))
                {
                    Directory.CreateDirectory(savefilepath);
                }

                compressedFile = await _RemoteMediumProxy.LogTransfer_OpenLogFile(SelectedLogFilePath);
                savefilepath += @"\" + SelectedLogFileName + ".txt";
                fileInfo = new FileInfo(savefilepath);

                DecompressFilesFromByteArray(compressedFile, savefilepath);

                Process.Start(savefilepath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return fileInfo;
        }

        public byte[] Decompress(byte[] data)
        {
            byte[] retVal = null;

            try
            {
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                        {
                            dstream.CopyTo(output);
                        }
                        retVal = output.ToArray();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void DecompressFilesFromByteArray(byte[] param, string filepath)
        {
            try
            {
                byte[] retbytes = null;
                retbytes = Decompress(param);

                using (Stream stream = new MemoryStream(retbytes))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var content = reader.ReadToEnd();
                        File.WriteAllText(filepath, content);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Task Refrash()
        {
            DateTime LogDateTime;
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    logTransferLists.Clear();
                });

                for (int i = 0; i < LogFileList.Count; i++)
                {
                    if (LogFileList[i][0] == CurrentSelection)
                    {
                        for (int j = 1; j < LogFileList[i].Count; j++)
                        {
                            string[] nameSplit = LogFileList[i][j].Split(new string[] { "\\" }, StringSplitOptions.None);
                            Match match = Regex.Match(nameSplit[nameSplit.Length - 1], @"\d{4}\-\d{2}\-\d{2}");

                            string date = match.Value;

                            if (!string.IsNullOrEmpty(date))
                            {
                                LogDateTime = DateTime.Parse(date);
                                if (LogDateTime >= StartDate && LogDateTime <= EndDate)
                                {
                                    logTransferLists.Add(new LogTransferList()
                                    {
                                        LogFileName = nameSplit[nameSplit.Length - 1],
                                        LogFilePath = LogFileList[i][j]
                                    });
                                }
                            }
                        }
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    uiLogListView.ItemsSource = logTransferLists;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"AlarmViewDialog.Refrash(): Error occurred. Err = {err.Message}");
            }
            return Task.CompletedTask;

        }
        private async Task RefrashFileCommandFunc()
        {
            try
            {
                LogFileList = new List<List<string>>();
                LogFileList = await _RemoteMediumProxy.LogTransfer_UpdateLogFile();

                await Refrash().ConfigureAwait(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SaveLogPathSettingCommandFunc()
        {
            try
            {
                ObservableDictionary<string, string> infos = new ObservableDictionary<string, string>();

                if (LogPathInfos != null)
                {
                    foreach (var info in LogPathInfos)
                    {
                        infos.Add(info.LogType, info.LogPath);
                    }
                }
                else
                {
                    LoggerManager.Debug("SaveLogPathSettingCommandFunc() LogPathInfos or EditLogPathInfos is null.");
                }
                EventCodeEnum retVal = _RemoteMediumProxy.SetLogPathInfos(infos);
                if (retVal == EventCodeEnum.NONE)
                {
                    MessageBox.Show("Success Save Parameter.", "Success");
                }
                else
                {
                    MessageBox.Show("The Root Directory of the Path for each log type must match 'FilePath'", "Fail");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void LogTypeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LogFileList = new List<List<string>>();
                LogFileList = await _RemoteMediumProxy.LogTransfer_UpdateLogFile();

                await Refrash().ConfigureAwait(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventLogManager eventLogManager = LoggerManager.EventLogMg;

        private void HistoryAlaramCommandFunc()
        {
            try
            {
                HistorySwitch = !HistorySwitch;

                if (HistorySwitch == true)
                {
                    CellInfo.ErrorCodeAlarams.Clear();
                    CellInfo.ErrorCodeAlarams = new ObservableCollection<AlarmLogData>(eventLogManager.OriginEventLogList.Where(alram => alram.OccurEquipment == CellInfo.Index));

                    ICollectionView ErrorCodeAlaramsView = CollectionViewSource.GetDefaultView(CellInfo.ErrorCodeAlarams);
                    ErrorCodeAlaramsView.SortDescriptions.Add(new SortDescription(nameof(AlarmLogData.ErrorOccurTime), ListSortDirection.Descending));

                    CellInfo.ErrorCodeAlarams.CollectionChanged += (s, e) =>
                    {
                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                        {
                            ErrorCodeAlaramsView.Refresh();
                        }
                    };
                }
                else
                {
                    ShowAlarmsView();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand) _SelectedItemChangedCommand = new AsyncCommand<object>(SelectedItemChangedCommandFunc);
                return _SelectedItemChangedCommand;
            }
        }

        public ILoaderViewModelManager LoaderVMManager => this.GetLoaderContainer().Resolve<ILoaderViewModelManager>();
        private Task SelectedItemChangedCommandFunc(object obj)
        {
            IList items = obj as IList;
            try
            {
                if (items != null)
                {
                    if (items.Count == 0)
                        return Task.CompletedTask;

                    AlarmLogData alaramParam = items[items.Count - 1] as AlarmLogData;
                    if (!alaramParam.IsChecked)
                        alaramParam.IsChecked = true;
                    CellInfo.AlarmMessageNotNotifiedCount = CellInfo.ErrorCodeAlarams.Where(alram => alram.IsChecked == false).Count();
                    LoaderVMManager.UpdateLoaderAlarmCount();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private RelayCommand<AlarmLogData> _buttonClickCommand;
        public ICommand ButtonClickCommand
        {
            get
            {
                if (_buttonClickCommand == null) _buttonClickCommand = new RelayCommand<AlarmLogData>(ButtonClickCommandFunc);
                return _buttonClickCommand;
            }
        }

        private Window GetActiveWindow()
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                return Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            });
        }

        private void ButtonClickCommandFunc(AlarmLogData alarmLogData)
        {
            try
            {
                if (alarmLogData != null)
                {
                    // 현재 활성 창을 부모로 설정
                    var currentWindow = GetActiveWindow();

                    if (currentWindow != null)
                    {
                        currentWindow.Dispatcher.Invoke(() =>
                        {
                            var result = MessageBox.Show(currentWindow, "Are you sure you want to load images?", "Load Image", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                var dataSet = _RemoteMediumProxy.GetImageDataSet(alarmLogData.ModuleType, alarmLogData.ModuleStartTime, alarmLogData.ImageDatasHashCode);

                                if (dataSet?.ImageDataCollection.Count > 0)
                                {
                                    var view = new ImageViewerWindow();
                                    var vm = new ImageViewerViewModel(view, dataSet);

                                    view.DataContext = vm;
                                    view.ShowDialog();
                                    //view.Show();
                                }
                                else
                                {
                                    MessageBox.Show("No images found to load.", "Load Image", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region Scroll

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        private void OnScrollUp(object sender, RoutedEventArgs e)
        {
            var scrollViwer = GetScrollViewer(uiListView) as ScrollViewer;

            if (scrollViwer != null)
            {
                // Logical Scrolling by Item
                // scrollViwer.LineUp();
                // Physical Scrolling by Offset
                scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + 3);
            }
        }

        private void OnScrollDown(object sender, RoutedEventArgs e)
        {
            var scrollViwer = GetScrollViewer(uiListView) as ScrollViewer;

            if (scrollViwer != null)
            {
                // Logical Scrolling by Item
                // scrollViwer.LineDown();
                // Physical Scrolling by Offset
                scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + 3);
            }
        }
        AnimationScrollViewer scrollViewer;
        private void StageListUpBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (svAlarmView.Visibility == Visibility.Visible)
                {
                    scrollViewer = svAlarmView;
                }
                else
                {
                    scrollViewer = svLogView;
                }

                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = scrollViewer.VerticalOffset;
                verticalAnimation.To = scrollViewer.VerticalOffset - ((scrollViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private void StageListDownBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (svAlarmView.Visibility == Visibility.Visible)
                {
                    scrollViewer = svAlarmView;
                }
                else
                {
                    scrollViewer = svLogView;
                }
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = scrollViewer.VerticalOffset;
                verticalAnimation.To = scrollViewer.VerticalOffset + ((scrollViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        #endregion

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] != null)
            {
                TabItem tabItem = e.AddedItems[0] as TabItem;
                if (tabItem != null)
                {
                    SetLogPathSetting(tabItem.Header.ToString());
                }
            }

        }
    }

    public class ForeGroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color fontcolor = Colors.WhiteSmoke;
            if (value is bool)
            {
                if ((bool)value)
                {
                    fontcolor = Colors.Gray;
                }
            }
            return fontcolor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsCheckedVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is bool)
            {
                if ((bool)value)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ViewHistoryBtnColorConverter : IValueConverter
    {
        static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush DrakOrangeBrush = new SolidColorBrush(Colors.DarkOrange);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is bool)
            {
                if ((bool)value)
                {
                    return DrakOrangeBrush;
                }
                else
                {
                    return WhiteBrush;
                }

            }
            return WhiteBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ViewLogBtnColorConverter : IValueConverter
    {
        static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush DrakOrangeBrush = new SolidColorBrush(Colors.DarkOrange);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is bool)
            {
                if ((bool)value)
                {
                    return DrakOrangeBrush;
                }
                else
                {
                    return WhiteBrush;
                }

            }
            return WhiteBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class LogPathInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _LogType;
        public string LogType
        {
            get { return _LogType; }
            set
            {
                if (value != _LogType)
                {
                    _LogType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LogPath;
        public string LogPath
        {
            get { return _LogPath; }
            set
            {
                if (value != _LogPath)
                {
                    _LogPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LogPathInfo(string logtype, string logpath)
        {
            LogType = logtype;
            LogPath = logpath;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = (string)value;

            if (string.IsNullOrEmpty(val))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
