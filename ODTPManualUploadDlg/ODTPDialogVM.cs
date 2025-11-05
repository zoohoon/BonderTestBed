namespace ODTPManualUploadDlg
{
    using LogModule;
    using MvvmHelpers.Commands;
    using SpoolingUtil;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public class ODTPDialogVM : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public ODTPDialogVM(Window myWnd, SpoolingManager spoolingMng)
        {
            parentWnd = myWnd;
            spoolingMngRef = spoolingMng;
            spoolingMngRef.ManualUploadResultCallBackFunc = ManualUploadResult;
        }

        private Window parentWnd;
        private SpoolingManager spoolingMngRef;
        ObservableCollection<SpoolingListItem> FailedToUploadListItems = new ObservableCollection<SpoolingListItem>();
        private ObservableCollection<ManualUploadItem> _ODTPList = new ObservableCollection<ManualUploadItem>();
        public bool UploadBtnEnable { get; set; } = true;

        private string _Detail_Info;
        public string Detail_Info
        {
            get => _Detail_Info;
            set
            {
                if (value != _Detail_Info)
                {
                    _Detail_Info = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<ManualUploadItem> ManualUploadItemList
        {
            get { return _ODTPList; }
            set
            {
                if (value != _ODTPList)
                {
                    _ODTPList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ManualUploadItem _CurrentSelectedItem;
        public ManualUploadItem CurrentSelectedItem
        {
            get => _CurrentSelectedItem;
            set
            {
                if (value != _CurrentSelectedItem)
                {
                    _CurrentSelectedItem = value;
                    Detail_Info = _CurrentSelectedItem?.failDetailInfo;
                    RaisePropertyChanged();
                }
            }
        }

        public float progressIncreasePos { get; set; } = 1f;

        private float _progressPos = 0f;
        public float progressPos
        {
            get => _progressPos;
            set
            {
                if (value != _progressPos)
                {
                    _progressPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Manual upload dialog 출력
        /// </summary>
        public void ShowMaualUploadDlg()
        {
            try
            {
                ODTPManualUploadDlg.MainWindow wnd = parentWnd as ODTPManualUploadDlg.MainWindow;
                if (false == spoolingMngRef.ManualUploadEndFlag)  //이전 manual upload 처리가 완료되지 않은 상태로 창을 닫아 아직 동작중인 상태라면
                {
                    spoolingMngRef.PreviousTaskDoing = true;
                    if (null != wnd)
                        wnd.PreviousTaskNotice.Visibility = Visibility.Visible;
                }
                else
                    wnd.PreviousTaskNotice.Visibility = Visibility.Hidden;

                ODTPItemListUp();
                if (null != parentWnd)
                    parentWnd.Show();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Manual로 전송할 Item gathering
        /// </summary>
        public void ODTPItemListUp()
        {
            _ODTPList.Clear();
            progressPos = 0;
            progressIncreasePos = 0;
            if (null != spoolingMngRef)
            {
                spoolingMngRef.ManualUploadSpooling_Init(ref FailedToUploadListItems); //< spooling thread pause로 인한 delay 생길 수 있음.
                foreach (var Item in FailedToUploadListItems)
                {
                    string fileNm = Path.GetFileName(Item.TargetItemPath);
                    ManualUploadItemList.Add(new ManualUploadItem(Convert.ToString(Item.Cell), fileNm, Item.Date, "X"));
                }

                progressIncreasePos = 100f / (float)(FailedToUploadListItems.Count);
            }
        }

        /// <summary>
        /// callback으로 호출될 Upload result 처리
        /// </summary>
        /// <param name="fileName"> 처리된 file name</param>
        /// <param name="dataTime"> 처리된 item의 생성된 시간</param>
        /// <param name="sucess"> 성공여부 </param>
        /// <param name="detail"> 실패 시 출력될 사유</param>
        private void ManualUploadResult(string fileName, string dataTime, bool sucess, string detail)
        {
            string ResultMark = "Fail";
            if (sucess)
                ResultMark = "Success";
            var Item = ManualUploadItemList.Where(x => x.resultFileNm == fileName && x.dateInfo == dataTime).FirstOrDefault();
            if (null != Item)
            {
                Item.uploadResult = ResultMark;
                Item.failDetailInfo = detail;
            }

            progressPos += progressIncreasePos;
            if (progressPos >= 100f)
                progressPos = 100f;
        }

        private AsyncCommand<object> _BtnCloseCmd;
        public ICommand ManualUploadDlgClose
        {
            get
            {
                if (null == _BtnCloseCmd) _BtnCloseCmd
                        = new AsyncCommand<object>(CloseCmdProc);
                return _BtnCloseCmd;
            }
        }

        private Task CloseCmdProc(object obj)
        {
            try
            {
                spoolingMngRef.ManualUploadSpooling_Fin();
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ODTPManualUploadDlg.MainWindow window = obj as ODTPManualUploadDlg.MainWindow;
                    if (window != null)
                    {
                        window.Hide();
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private DelegateCommand _UploadCmd; //별도 delegatCommand사용, 기존 AsyncCommand로 사용하면 원형 progressbar 뒤에서 동작되어 별도 처리
        public ICommand ManualUploadStart
        {
            get
            {
                if (null == _UploadCmd)
                    _UploadCmd = new DelegateCommand(ODTPUploadStart, CanExecuteMethod);
                return _UploadCmd;
            }
        }

        /// <summary>
        /// Upload start 처리
        /// </summary>
        /// <param name="obj"></param>
        private void ODTPUploadStart(object obj)
        {
            LoggerManager.Debug($" Execute [{MethodBase.GetCurrentMethod().Name}] async generic command.");
            ODTPManualUploadDlg.MainWindow wnd = obj as ODTPManualUploadDlg.MainWindow;

            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (wnd != null)
                    wnd.UploadBtnInXaml.IsEnabled = false;
            }));
#pragma warning disable 4014
            //brett// task(odtp upload) 실행 후 대기 하지 않고 command 종료 하기 위해 await 하지 않음
            ODTPUploadProc(obj);
#pragma warning restore 4014
            LoggerManager.Debug($" Execute [{MethodBase.GetCurrentMethod().Name}] async command DONE.");
        }
        private bool CanExecuteMethod(object arg)
        {
            return UploadBtnEnable;
        }

        private async Task ODTPUploadProc(object obj)
        {
            try
            {
                ODTPManualUploadDlg.MainWindow wnd = obj as ODTPManualUploadDlg.MainWindow;
                await Task.Run(() =>
                {
                    UploadBtnEnable = false;
                    spoolingMngRef.StartManualUploadProc(ref FailedToUploadListItems);
                    UploadBtnEnable = true;

                });

                if (null != CurrentSelectedItem)
                    Detail_Info = CurrentSelectedItem.failDetailInfo;

                progressPos = 100f;
                await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (wnd != null)
                        wnd.UploadBtnInXaml.IsEnabled = true;

                    //이전 작업이 남아 있다는 문구가 출력중이 었다면 숨김처리
                    if (wnd != null)
                        wnd.PreviousTaskNotice.Visibility = Visibility.Hidden;
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
    public class ManualUploadItem : INotifyPropertyChanged
    {

        public string stageNum { get; set; }
        public string resultFileNm { get; set; }
        public string dateInfo { get; set; }
        private string _uploadResult;
        public string failDetailInfo { get; set; }
        public string uploadResult
        {
            get => _uploadResult;
            set
            {
                _uploadResult = value;
                OnPropertyChanged(nameof(uploadResult));
            }
        }

        public ManualUploadItem(string stageNum, string resultFieNm, string date, string uploadRet)
        {
            this.stageNum = stageNum;
            this.resultFileNm = resultFieNm;
            this.dateInfo = date;
            this.uploadResult = uploadRet;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        Action<object> _executeMethod;
        Func<object, bool> _canexecuteMethod;

        public DelegateCommand(Action<object> executeMethod, Func<object, bool> canexecuteMethod)
        {
            this._executeMethod = executeMethod;
            this._canexecuteMethod = canexecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _executeMethod(parameter);
        }
    }
}
