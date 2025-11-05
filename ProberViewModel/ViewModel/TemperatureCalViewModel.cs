using CsvHelper;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using ProberInterfaces.Temperature;
using RecipeEditorControl.RecipeEditorParamEdit;
using RelayCommandBase;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using VirtualKeyboardControl;

namespace TemperatureCalViewModelProject
{
    public class TemperatureCalibrationInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _StandardTemp;
        public double StandardTemp
        {
            get { return _StandardTemp; }
            set
            {
                if (value != _StandardTemp)
                {
                    _StandardTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CorrectTemp;
        public double CorrectTemp
        {
            get { return _CorrectTemp; }
            set
            {
                if (value != _CorrectTemp)
                {
                    _CorrectTemp = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class TemperatureCalViewModel : IMainScreenViewModel, IUpDownBtnNoneVisible
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
        #region ==> RecipeEditorParamEdit
        private RecipeEditorParamEditViewModel _RecipeEditorParamEdit;
        public RecipeEditorParamEditViewModel RecipeEditorParamEdit
        {
            get { return _RecipeEditorParamEdit; }
            set
            {
                if (value != _RecipeEditorParamEdit)
                {
                    _RecipeEditorParamEdit = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        private readonly Guid _ViewModelGUID = new Guid("9a7d8e76-e26c-47bc-bc95-4976c87d314b");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;
        //public ObservableCollection<IRenderableSeriesViewModel> RenderableSeriesViewModels
        //{
        //    get; set;
        //}
        private ObservableCollection<IRenderableSeriesViewModel> _RenderableSeriesViewModels;
        public ObservableCollection<IRenderableSeriesViewModel> RenderableSeriesViewModels
        {
            get { return _RenderableSeriesViewModels; }
            set
            {
                if (value != _RenderableSeriesViewModels)
                {
                    _RenderableSeriesViewModels = value;
                    RaisePropertyChanged();
            }
                }
        }

        private double _MonitoringMVTimeInSec;
        public double MonitoringMVTimeInSec
        {
            get { return _MonitoringMVTimeInSec; }
            set
            {
                if (value != _MonitoringMVTimeInSec)
                {
                    _MonitoringMVTimeInSec = value;
                    RaisePropertyChanged();
                }
            }
        }


        // 1 Means 0.1 degree
        private decimal CorrectTempControlUnit = 0.1m;
        private int _CategoryID = 10021;
        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            
            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                if (this.TempController() != null && this.TempController().Initialized == true)
                {
                    int offsetcount = this.TempController().GetHeaterOffsetCount();
                    foreach (var info in this.TempController().GetHeaterOffsets())
                    {
                        TemperatureCalibrationInfo tmp = new TemperatureCalibrationInfo();
                        tmp.StandardTemp = info.Key;
                        tmp.CorrectTemp = info.Value;
                        TempCalInfos.Add(tmp);
                    }
                    retval = InitCallInfo();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitViewModel(): Error occurred. Err = {err.Message}");
            }
            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(_CategoryID);

                this.TempController().EnableRemoteUpdate();
                TempControlEnable = true;

                ChangeTemp = TempController.TempInfo.SetTemp.Value;
                CorrectTemp = TempController.TempInfo.SetTemp.Value;
                TempController.GetTempMonitorInfo();
                TempMonitorRange = TempController.TempInfo.MonitoringInfo.TempMonitorRange;
                WaitMonitorTimeSec = TempController.TempInfo.MonitoringInfo.WaitMonitorTimeSec;
                MonitoringEnable = TempController.TempInfo.MonitoringInfo.MonitoringEnable;

                InitCallInfo();
                //if (RenderableSeriesViewModels == null)
                //{
                RenderableSeriesViewModels = new ObservableCollection<IRenderableSeriesViewModel>()
                            {
                                new LineRenderableSeriesViewModel{
                                    DataSeries = TempController.dataSeries_CurTemp,
                                    StyleKey = "LineSeriesStyle",
                                    Stroke = Colors.Red},
                                new LineRenderableSeriesViewModel{
                                    DataSeries = TempController.dataSeries_SetTemp,
                                    StyleKey = "LineSeriesStyle",
                                    Stroke = Colors.Green},
                            };
                //}
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private ObservableCollection<TemperatureCalibrationInfo> _TempCalInfos = new ObservableCollection<TemperatureCalibrationInfo>();
        public ObservableCollection<TemperatureCalibrationInfo> TempCalInfos
        {
            get { return _TempCalInfos; }
            set
            {
                if (value != _TempCalInfos)
                {
                    _TempCalInfos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _TempControlEnable;
        public bool TempControlEnable
        {
            get { return _TempControlEnable; }
            set
            {
                if (value != _TempControlEnable)
                {
                    _TempControlEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ChuckVac;
        public bool ChuckVac
        {
            get { return _ChuckVac; }
            set
            {
                if (value != _ChuckVac)
                {
                    _ChuckVac = value;
                    this.TempController().SetVacuum(ChuckVac);
                    RaisePropertyChanged();
                }
            }
        }

        private int RowMaxCount = 6;
        private int _CurrentPage = 0;
        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                if (value != _CurrentPage)
                {
                    _CurrentPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int TableNum = 2;

        private int _PageCount;
        public int PageCount
        {
            get { return _PageCount; }
            set
            {
                if (value != _PageCount)
                {
                    _PageCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SavePath = "";
        public string SavePath
        {
            get { return _SavePath; }
            set
            {
                if (value != _SavePath)
                {
                    _SavePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Init_WriteEnable;
        public bool Init_WriteEnable
        {
            get { return _Init_WriteEnable; }
            set
            {
                if (value != _Init_WriteEnable)
                {
                    _Init_WriteEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private AsyncObservableCollection<TemperatureCalibrationInfo> _First_DG_TempCalInfos = new AsyncObservableCollection<TemperatureCalibrationInfo>();
        public AsyncObservableCollection<TemperatureCalibrationInfo> First_DG_TempCalInfos
        {
            get { return _First_DG_TempCalInfos; }
            set
            {
                if (value != _First_DG_TempCalInfos)
                {
                    _First_DG_TempCalInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<TemperatureCalibrationInfo> _Second_DG_TempCalInfos = new AsyncObservableCollection<TemperatureCalibrationInfo>();
        public AsyncObservableCollection<TemperatureCalibrationInfo> Second_DG_TempCalInfos
        {
            get { return _Second_DG_TempCalInfos; }
            set
            {
                if (value != _Second_DG_TempCalInfos)
                {
                    _Second_DG_TempCalInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TemperatureCalibrationInfo _First_SelectedItem;
        public TemperatureCalibrationInfo First_SelectedItem
        {
            get { return _First_SelectedItem; }
            set
            {
                if (value != _First_SelectedItem)
                {
                    _First_SelectedItem = value;

                    //if (_Second_SelectedItem != null)
                    //{
                    //    _Second_SelectedItem = null;
                    //}

                    RaisePropertyChanged();
                }
            }
        }

        private TemperatureCalibrationInfo _Second_SelectedItem;
        public TemperatureCalibrationInfo Second_SelectedItem
        {
            get { return _Second_SelectedItem; }
            set
            {
                if (value != _Second_SelectedItem)
                {
                    _Second_SelectedItem = value;

                    //if (_First_SelectedItem != null)
                    //{
                    //    _First_SelectedItem = null;
                    //}

                    RaisePropertyChanged();
                }
            }
        }

        private double _ChangeTemp;
        public double ChangeTemp
        {
            get { return _ChangeTemp; }
            set
            {
                if (value != _ChangeTemp)
                {
                    _ChangeTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CorrectTemp;
        public double CorrectTemp
        {
            get { return _CorrectTemp; }
            set
            {
                if (value != _CorrectTemp)
                {
                    _CorrectTemp = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _LoggingInterval;
        public long LoggingInterval
        {
            get { return _LoggingInterval; }
            set
            {
                if (value != _LoggingInterval)
                {
                    _LoggingInterval = value;
                    
                    this.TempController()?.SetLoggingInterval(LoggingInterval);

                    RaisePropertyChanged();
                }
            }
        }

        #region Monitoring
        private double _TempMonitorRange;
        public double TempMonitorRange
        {
            get { return _TempMonitorRange; }
            set
            {
                if (value != _TempMonitorRange)
                {
                    _TempMonitorRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _WaitMonitorTimeSec;
        public double WaitMonitorTimeSec
        {
            get { return _WaitMonitorTimeSec; }
            set
            {
                if (value != _WaitMonitorTimeSec)
                {
                    _WaitMonitorTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MonitoringEnable;
        public bool MonitoringEnable
        {
            get { return _MonitoringEnable; }
            set
            {
                if (value != _MonitoringEnable)
                {
                    _MonitoringEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        public ITempController TempController
        {
            get
            {
                return this.TempController();
            }
            set
            {

            }
        }

        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tmp = param as System.Windows.Controls.TextBox;

                //tmp.Text = tmp.Text.Replace("℃", "");
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetTempCommand;
        public ICommand SetTempCommand
        {
            get
            {
                if (null == _SetTempCommand)
                    _SetTempCommand = new RelayCommand<object>(SetTempCommandFunc);
                return _SetTempCommand;
            }
        }

        private void SetTempCommandFunc(object obj)
        {
            try
            {
                this.TempController().SetSV(TemperatureChangeSource.TEMP_EXTERNAL, ChangeTemp, willYouSaveSetValue: false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private RelayCommand _SetMonitoringTempCommand;
        public ICommand SetMonitoringTempCommand
        {
            get
            {
                if (null == _SetMonitoringTempCommand)
                    _SetMonitoringTempCommand = new RelayCommand(SetMonitoringTempCommandFunc);
                return _SetMonitoringTempCommand;
            }
        }

        private void SetMonitoringTempCommandFunc()
        {
            try
            {
                if (TempController.TempInfo.MonitoringInfo == null)
                    TempController.TempInfo.MonitoringInfo = new TempMonitoringInfo();
                TempController.TempInfo.MonitoringInfo.TempMonitorRange = TempMonitorRange;
                TempController.TempInfo.MonitoringInfo.WaitMonitorTimeSec = WaitMonitorTimeSec;
                TempController.TempInfo.MonitoringInfo.MonitoringEnable = MonitoringEnable;
                TempController.SetTempMonitorInfo(TempController.TempInfo.MonitoringInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }



        private RelayCommand<object> _SaveTempInfoCommand;
        public ICommand SaveTempInfoCommand
        {
            get
            {
                if (null == _SaveTempInfoCommand)
                    _SaveTempInfoCommand = new RelayCommand<object>(SaveTempInfoCommandFunc);
                return _SaveTempInfoCommand;
            }
        }

        private void SaveTempInfoCommandFunc(object obj)
        {
            try
            {
                this.TempController().ClearHeaterOffset();
                foreach (var info in this.TempCalInfos)
                {
                    this.TempController().AddHeaterOffset(info.StandardTemp, info.CorrectTemp);
                }
                TempController.SaveOffsetParameter();
                TempController.SetTemperature();

                if(MonitoringMVTimeInSec != this.TempController().GetMonitoringMVTimeInSec())
                {
                    this.TempController().SetMonitoringMVTimeInSec(MonitoringMVTimeInSec);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

      

        private AsyncCommand _AddOffsetInfoCommand;
        public ICommand AddOffsetInfoCommand
        {
            get
            {
                if (null == _AddOffsetInfoCommand)
                    _AddOffsetInfoCommand = new AsyncCommand(AddOffsetInfoCommandFunc);
                return _AddOffsetInfoCommand;
            }
        }

        private async Task AddOffsetInfoCommandFunc()
        {
            try
            {
                TemperatureCalibrationInfo tmp = new TemperatureCalibrationInfo();

                tmp.StandardTemp = ChangeTemp;
                tmp.CorrectTemp = CorrectTemp;

                EnumMessageDialogResult ret = EnumMessageDialogResult.UNDEFIND;

                // Check, Already exist the standard data.
                var exist = TempCalInfos.FirstOrDefault(i => i.StandardTemp == ChangeTemp);
                bool modify = false;

                // 이미 테이블안에 데이터가 있음.
                if (exist != null)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("WARNING", "Already there is the same temperature data. \nDo you want to overwrite it?", EnumMessageStyle.AffirmativeAndNegative);
                }
                else
                {
                    modify = true;
                }

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    modify = true;
                    TempCalInfos.Remove(exist);
                }

                if (modify == true)
                {
                    TempCalInfos.Add(tmp);

                    // Sort
                    TempCalInfos = new ObservableCollection<TemperatureCalibrationInfo>(TempCalInfos.OrderBy(i => i.StandardTemp));
                }

                UpdateTable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _SavePathSetCommand;
        public ICommand SavePathSetCommand
        {
            get
            {
                if (null == _SavePathSetCommand)
                    _SavePathSetCommand = new RelayCommand<object>(SavePathSetCommandFunc);
                return _SavePathSetCommand;
            }
        }

        private void SavePathSetCommandFunc(object obj)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        SavePath = fbd.SelectedPath;

                        //string[] files = Directory.GetFiles(fbd.SelectedPath);

                        //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                    }
                    else
                    {
                        SavePath = string.Empty;
                    }
                }

                ////파일오픈창 생성 및 설정
                //System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                //ofd.Title = "파일 오픈 예제창";
                //ofd.FileName = "test";
                //ofd.Filter = "그림 파일 (*.jpg, *.gif, *.bmp) | *.jpg; *.gif; *.bmp; | 모든 파일 (*.*) | *.*";

                ////파일 오픈창 로드
                //DialogResult dr = ofd.ShowDialog();

                ////OK버튼 클릭시
                //if (dr == DialogResult.OK)
                //{
                //    //File명과 확장자를 가지고 온다.
                //    string fileName = ofd.SafeFileName;
                //    //File경로와 File명을 모두 가지고 온다.
                //    string fileFullName = ofd.FileName;
                //    //File경로만 가지고 온다.
                //    string filePath = fileFullName.Replace(fileName, "");

                //    ////출력 예제용 로직
                //    //label1.Text = "File Name  : " + fileName;
                //    //label2.Text = "Full Name  : " + fileFullName;
                //    //label3.Text = "File Path  : " + filePath;
                //    ////File경로 + 파일명 리턴
                //    //return fileFullName;
                //}
                ////취소버튼 클릭시 또는 ESC키로 파일창을 종료 했을경우
                //else if (dr == DialogResult.Cancel)
                //{
                //    //SavePath = "";
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _TempDataStoreCommand;
        public ICommand TempDataStoreCommand
        {
            get
            {
                if (null == _TempDataStoreCommand)
                    _TempDataStoreCommand = new RelayCommand<object>(TempDataStoreCommandFunc);
                return _TempDataStoreCommand;
            }
        }

        private void TempDataStoreCommandFunc(object obj)
        {
            try
            {
                //var records = new List<TemperatureCalibrationInfo>();

                //foreach (var item in )
                //{
                //    records.Add()
                //}

                if (SavePath != null)
                {
                    using (var writer = new StreamWriter(SavePath))
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.WriteRecords(TempCalInfos);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _TempDataReStoreCommand;
        public ICommand TempDataReStoreCommand
        {
            get
            {
                if (null == _TempDataReStoreCommand)
                    _TempDataReStoreCommand = new RelayCommand<object>(TempDataReStoreCommandFunc);
                return _TempDataReStoreCommand;
            }
        }

        private void TempDataReStoreCommandFunc(object obj)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";

                DialogResult dr = openFileDialog.ShowDialog();

                // TODO :
                if (openFileDialog.FileName != null)
                {
                    using (var reader = new StreamReader(openFileDialog.FileName))
                    using (var csv = new CsvReader(reader))
                    {
                        var records = csv.GetRecords<TemperatureCalibrationInfo>();
                    }
                }


                //using (TextFieldParser csvReader = new TextFieldParser(openFileDialog.FileName))
                //{
                //    csvReader.SetDelimiters(",");
                //    csvReader.HasFieldsEnclosedInQuotes = true;

                //    int row = 0;

                //    while (csvReader.EndOfData == false)
                //    {
                //        String[] fieldData = csvReader.ReadFields();
                //        for (int col = 0; col < fieldData.Length; col++)
                //        {
                //            int number = 0;
                //            //String numberStr = split[0];
                //            if (int.TryParse(fieldData[col], out number) == false)
                //                continue;
                //        }
                //        row++;
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _CorrectTempChangeUPCommand;
        public ICommand CorrectTempChangeUPCommand
        {
            get
            {
                if (null == _CorrectTempChangeUPCommand)
                    _CorrectTempChangeUPCommand = new RelayCommand<object>(CorrectTempChangeUPCommandFunc);
                return _CorrectTempChangeUPCommand;
            }
        }
        public void CorrectTempChangeUPCommandFunc(object obj)
        {
            try
            {
                if (obj is TemperatureCalibrationInfo)
                {
                    decimal correcttmp = (decimal)(obj as TemperatureCalibrationInfo).CorrectTemp;

                    (obj as TemperatureCalibrationInfo).CorrectTemp = (double)(correcttmp + CorrectTempControlUnit);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _RemoveAllTempInfoCommand;
        public ICommand RemoveAllTempInfoCommand
        {
            get
            {
                if (null == _RemoveAllTempInfoCommand)
                    _RemoveAllTempInfoCommand = new RelayCommand<object>(RemoveAllTempInfoCommandFunc);
                return _RemoveAllTempInfoCommand;
            }
        }

        private void RemoveAllTempInfoCommandFunc(object obj)
        {
            try
            {
                TempCalInfos.Clear();

                CurrentPage = 0;

                UpdateTable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _RemoveTempInfoCommand;
        public ICommand RemoveTempInfoCommand
        {
            get
            {
                if (null == _RemoveTempInfoCommand)
                    _RemoveTempInfoCommand = new RelayCommand<object>(RemoveTempInfoCommandFunc);
                return _RemoveTempInfoCommand;
            }
        }

        private void RemoveTempInfoCommandFunc(object obj)
        {
            try
            {
                if (obj is TemperatureCalibrationInfo)
                {
                    var item = TempCalInfos.FirstOrDefault(i => i == obj);

                    if (item != null)
                    {
                        TempCalInfos.Remove(item);
                    }

                    //    //do whatever you want to do

                    //TempCalInfos.FirstOrDefault(obj);

                    //if( (First_SelectedItem != null) && (obj as TemperatureCalibrationInfo).GetHashCode() == First_SelectedItem.GetHashCode())
                    //{

                    //}
                    //else if ((Second_SelectedItem != null) && (obj as TemperatureCalibrationInfo).GetHashCode() == Second_SelectedItem.GetHashCode())
                    //{
                    //    second
                    //}

                    UpdateTable();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _CorrectTempChangeDOWNCommand;
        public ICommand CorrectTempChangeDOWNCommand
        {
            get
            {
                if (null == _CorrectTempChangeDOWNCommand)
                    _CorrectTempChangeDOWNCommand = new RelayCommand<object>(CorrectTempChangeDOWNCommandFunc);
                return _CorrectTempChangeDOWNCommand;
            }
        }
        public void CorrectTempChangeDOWNCommandFunc(object obj)
        {
            try
            {
                if (obj is TemperatureCalibrationInfo)
                {
                    decimal correcttmp = (decimal)(obj as TemperatureCalibrationInfo).CorrectTemp;

                    (obj as TemperatureCalibrationInfo).CorrectTemp = (double)(correcttmp - CorrectTempControlUnit);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<SETUP_DIRECTION> _ChangePageCommand;
        public ICommand ChangePageCommand
        {
            get
            {
                if (null == _ChangePageCommand)
                    _ChangePageCommand = new RelayCommand<SETUP_DIRECTION>(ChangePageCommandFunc);
                return _ChangePageCommand;
            }
        }

        public void ChangePageCommandFunc(SETUP_DIRECTION obj)
        {
            try
            {
                if (PageCount > 0)
                {
                    if (obj == SETUP_DIRECTION.PREV)
                    {
                        if (CurrentPage == 0)
                        {
                            CurrentPage = PageCount - 1;
                        }
                        else
                        {
                            CurrentPage = CurrentPage - 1;
                        }
                    }
                    else
                    {
                        if (CurrentPage == PageCount - 1)
                        {
                            CurrentPage = 0;
                        }
                        else
                        {
                            CurrentPage = CurrentPage + 1;
                        }
                    }
                }

                UpdateTable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum UpdateTable()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                int FirstStartIndex;
                int FirstEndIndex;

                int SecondStartIndex;
                int SecondEndIndex;

                int index;
                bool NeedSecond = false;

                int tempinfocount = this.TempCalInfos.Count;
                if ((tempinfocount % RowMaxCount) == 0)
                {
                    PageCount = (int)Math.Round((double)tempinfocount / (double)(RowMaxCount * 2), MidpointRounding.AwayFromZero);
                }
                else
                {
                    PageCount = (tempinfocount / (RowMaxCount * 2)) + 1;
                }

                First_DG_TempCalInfos.Clear();
                Second_DG_TempCalInfos.Clear();

                if (PageCount == 0)
                {
                    CurrentPage = -1;
                }

                if (PageCount > 0 && CurrentPage < 0)
                {
                    CurrentPage = 0;
                }


                if (CurrentPage >= 0)
                {
                    FirstStartIndex = (CurrentPage * TableNum) * RowMaxCount;

                    int remaincount;

                    remaincount = tempinfocount - FirstStartIndex;

                    if (remaincount >= RowMaxCount)
                    {
                        FirstEndIndex = FirstStartIndex + RowMaxCount;
                        NeedSecond = true;
                    }
                    else
                    {
                        FirstEndIndex = FirstStartIndex + remaincount;
                    }

                    index = 0;
                    for (int i = FirstStartIndex; i < FirstEndIndex; i++)
                    {
                        First_DG_TempCalInfos.Add(TempCalInfos[i]);

                        index++;
                    }

                    if (NeedSecond == true)
                    {
                        SecondStartIndex = FirstEndIndex;

                        remaincount = tempinfocount - SecondStartIndex;

                        if (remaincount >= RowMaxCount)
                        {
                            SecondEndIndex = SecondStartIndex + RowMaxCount;
                        }
                        else
                        {
                            SecondEndIndex = SecondStartIndex + remaincount;
                        }

                        index = 0;
                        for (int i = SecondStartIndex; i < SecondEndIndex; i++)
                        {
                            Second_DG_TempCalInfos.Add(TempCalInfos[i]);

                            index++;
                        }
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum InitCallInfo()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurrentPage = 0;

                TempCalInfos.Clear();
                var tempOffsets = this.TempController().GetHeaterOffsets();
                foreach (var info in tempOffsets)
                {
                    TemperatureCalibrationInfo tmp = new TemperatureCalibrationInfo();

                    tmp.StandardTemp = info.Key;
                    tmp.CorrectTemp = info.Value;

                    TempCalInfos.Add(tmp);
                }

                if (TempCalInfos.Count > 0)
                {
                    UpdateTable();
                }
                else
                {
                    CurrentPage = -1;
                }

                //if ((this.TempController().TempManager.Dic_HeaterOffset.Count % RowMaxCount) == 0)
                //{
                //    PageCount = this.TempController().TempManager.Dic_HeaterOffset.Count / (RowMaxCount * 2);
                //}
                //else
                //{
                //    PageCount = (this.TempController().TempManager.Dic_HeaterOffset.Count / (RowMaxCount * 2)) + 1;
                //}

                //ChangePageCommandFunc(SETUP_DIRECTION.NEXT);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized == false)
            {
                //TempController = this.TempController();

                //int offsetcount = this.TempController().TempManager.Dic_HeaterOffset.Count;

                //foreach (var info in this.TempController().TempManager.Dic_HeaterOffset)
                //{
                //    TemperatureCalibrationInfo tmp = new TemperatureCalibrationInfo();

                //    tmp.StandardTemp = info.Key;
                //    tmp.CorrectTemp = info.Value;

                //    TempCalInfos.Add(tmp);
                //}

                //retval = InitCallInfo();
                LoggingInterval = 15;
                Initialized = true;
                retval = EventCodeEnum.NONE;
            }
            else
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                retval = EventCodeEnum.DUPLICATE_INVOCATION;
            }

            return retval;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            this.TempController().DisableRemoteUpdate();
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {GetType().Name}");
        }
    }
}
