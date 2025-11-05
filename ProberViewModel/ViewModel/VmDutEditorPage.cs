using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DutEditorPageViewModel
{
    using LogModule;
    using Microsoft.VisualBasic.FileIO;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.ControlClass.ViewModel.DutEditor;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using ProberInterfaces.State;
    using SerializerUtil;
    using MetroDialogInterfaces;
    using System.IO;
    using MaterialDesignExtensions.Controls;

    public class VmDutEditorPage : IMainScreenViewModel, INotifyPropertyChanged, IDutEditorVM
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("5899DCFE-3032-5360-03D7-1F356B7A0800");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        //public DutEditorViewModel DutEditor { get; set; }

        public bool Initialized { get; set; } = false;

        public IStageSupervisor StageSupervisor { get; internal set; }
        public IMotionManager MotionManager { get; internal set; }

        public IVisionManager VisionManager { get; internal set; }
        private IProbeCard _ProbeCard;
        public IProbeCard ProbeCard
        {
            get { return _ProbeCard; }
            set
            {
                if (value != _ProbeCard)
                {
                    _ProbeCard = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityZoomIn;
        public Visibility VisibilityZoomIn
        {
            get { return _VisibilityZoomIn; }
            set
            {
                if (value != _VisibilityZoomIn)
                {
                    _VisibilityZoomIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityZoomOut;
        public Visibility VisibilityZoomOut
        {
            get { return _VisibilityZoomOut; }
            set
            {
                if (value != _VisibilityZoomOut)
                {
                    _VisibilityZoomOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityMoveToCenter;
        public Visibility VisibilityMoveToCenter
        {
            get { return _VisibilityMoveToCenter; }
            set
            {
                if (value != _VisibilityMoveToCenter)
                {
                    _VisibilityMoveToCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IWaferObject _WaferObject;
        public IWaferObject WaferObject
        {
            get { return _WaferObject; }
            set
            {
                if (value != _WaferObject)
                {
                    _WaferObject = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EnumProberCam CamType { get; internal set; }

        private int _CurrentDutCount;
        public int CurrentDutCount
        {
            get { return _CurrentDutCount; }
            set
            {
                if (value != _CurrentDutCount)
                {
                    _CurrentDutCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ZoomLevel;
        public double ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _AddCheckBoxIsChecked;
        public bool? AddCheckBoxIsChecked
        {
            get { return _AddCheckBoxIsChecked; }
            set
            {
                if (value != _AddCheckBoxIsChecked)
                {
                    _AddCheckBoxIsChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPad;
        public bool? ShowPad
        {
            get { return _ShowPad; }
            set
            {
                if (value != _ShowPad)
                {
                    _ShowPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPin;
        public bool? ShowPin
        {
            get { return _ShowPin; }
            set
            {
                if (value != _ShowPin)
                {
                    _ShowPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowCurrentPos;
        public bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set
            {
                if (value != _ShowCurrentPos)
                {
                    _ShowCurrentPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _EnableDragMap;
        public bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set
            {
                if (value != _EnableDragMap)
                {
                    _EnableDragMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowSelectedDut;
        public bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set
            {
                if (value != _ShowSelectedDut)
                {
                    _ShowSelectedDut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowGrid;
        public bool? ShowGrid
        {
            get { return _ShowGrid; }
            set
            {
                if (value != _ShowGrid)
                {
                    _ShowGrid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CSVFilePath;
        public string CSVFilePath
        {
            get { return _CSVFilePath; }
            set
            {
                if (value != _CSVFilePath)
                {
                    _CSVFilePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Stream _CSVFileStream;
        public Stream CSVFileStream
        {
            get { return _CSVFileStream; }
            set
            {
                if (value != _CSVFileStream)
                {
                    _CSVFileStream = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CurXPos;
        public double CurXPos
        {
            get { return _CurXPos; }
            set
            {
                if (value != _CurXPos)
                {
                    _CurXPos = value;
                    RaisePropertyChanged(nameof(CurXPos));
                }
            }
        }

        private double _CurYPos;
        public double CurYPos
        {
            get { return _CurYPos; }
            set
            {
                if (value != _CurYPos)
                {
                    _CurYPos = value;
                    RaisePropertyChanged(nameof(CurYPos));
                }
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    VisibilityZoomIn = Visibility.Hidden;
                    VisibilityZoomOut = Visibility.Hidden;
                    VisibilityMoveToCenter = Visibility.Hidden;

                    this.StageSupervisor = this.StageSupervisor();
                    this.MotionManager = this.MotionManager();
                    this.VisionManager = this.VisionManager();

                    AddCheckBoxIsChecked = false;

                    //ProbeCardData = new ProbeCard();
                    //ProbeCardData = this.GetParam_ProbeCard();

                    if (this.StageSupervisor().ProbeCardInfo != null && this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef != null)
                    {
                        CurrentDutCount = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count;
                    }

                    //ProbeCardData.SelectDutIndex.XIndex.Value = 0;
                    //ProbeCardData.SelectDutIndex.YIndex.Value = 0;

                    ShowPad = false;
                    ShowPin = false;
                    EnableDragMap = false;
                    ShowSelectedDut = true;
                    ShowGrid = true;
                    ShowCurrentPos = false;

                    ZoomLevel = 2;
                    //DutEditor = new DutEditorViewModel(new Size(892, 911));

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

        #region ==> ZoomInCommand
        private RelayCommand _ZoomInCommand;
        public ICommand ZoomInCommand
        {
            get
            {
                if (null == _ZoomInCommand) _ZoomInCommand = new RelayCommand(ZoomInCommandFunc);
                return _ZoomInCommand;
            }
        }
        private void ZoomInCommandFunc()
        {
            try
            {
                if (ZoomLevel < 16)
                {
                    //ZoomLevel++;
                    ZoomLevel = ZoomLevel + 0.5;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private RelayCommand _MakeDutMapInformationCommand;
        public ICommand MakeDutMapInformationCommand
        {
            get
            {
                if (null == _MakeDutMapInformationCommand) _MakeDutMapInformationCommand = new RelayCommand(MakeDutMapInformationCommandFunc);
                return _MakeDutMapInformationCommand;
            }
        }

        private void MakeDutMapInformationCommandFunc()
        {
            try
            {
                // TODO :

                var dutlist = ProbeCard.ProbeCardDevObjectRef.DutList.ToList();

                int width = ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX;
                int height = ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY;
                int widthStep;

                List<int> dutMIndexlist = new List<int>();

                string DMICmd = string.Empty;
                string seperator = ",";

                foreach (IDut dut in dutlist)
                {
                    int index = ((int)dut.MacIndex.YIndex * width) + (int)dut.MacIndex.XIndex + 1;

                    DMICmd += index.ToString() + seperator;

                    //dutMIndexlist.Add(index);
                }

                DMICmd = DMICmd.Remove(DMICmd.Length - 1, 1);

                DMICmd = "DMI" + "#" + $"{width}" + "#" + $"{height}" + "#" + DMICmd;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #region ==> ZoomOutCommand
        private RelayCommand _ZoomOutCommand;
        public ICommand ZoomOutCommand
        {
            get
            {
                if (null == _ZoomOutCommand) _ZoomOutCommand = new RelayCommand(ZoomOutCommandFunc);
                return _ZoomOutCommand;
            }
        }
        private void ZoomOutCommandFunc()
        {
            try
            {
                if (ZoomLevel > 2)
                {
                    //ZoomLevel--;
                    ZoomLevel = ZoomLevel - 0.5;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> InitializePalletCommand
        private AsyncCommand _InitializePalletCommand;
        public IAsyncCommand InitializePalletCommand
        {
            get
            {
                if (null == _InitializePalletCommand) _InitializePalletCommand = new AsyncCommand(FuncInitializePalletCommand);
                return _InitializePalletCommand;
            }
        }

        public async Task FuncInitializePalletCommand()
        {
            try
            {
                bool retBool = await ConfirmToClear();
                if (retBool == false) return;

                // 현재 더트 정보를 클리어 한다.

                Task task = new Task(() =>
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Clear();
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX = 1;
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY = 1;
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private AsyncCommand<EnumArrowDirection> _DutEditerMoveCommand;
        public ICommand DutEditerMoveCommand
        {
            get
            {
                if (null == _DutEditerMoveCommand) _DutEditerMoveCommand = new AsyncCommand<EnumArrowDirection>(DutEditerMoveCommandFunc);
                return _DutEditerMoveCommand;
            }
        }

        public async Task DutEditerMoveCommandFunc(EnumArrowDirection obj)
        {
            try
            {
                int addIndexX = this.StageSupervisor.CoordinateManager().GetReverseManualMoveX() ? -1 : 1;
                int addIndexY = this.StageSupervisor.CoordinateManager().GetReverseManualMoveY() ? -1 : 1;

                switch (obj)
                {
                    case EnumArrowDirection.LEFTUP:

                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex -= addIndexX;
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex += addIndexY;
                        break;
                    case EnumArrowDirection.UP:

                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex += addIndexY;
                        break;
                    case EnumArrowDirection.RIGHTUP:
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex += addIndexX;
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex += addIndexY;
                        break;
                    case EnumArrowDirection.LEFT:
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex -= addIndexX;
                        break;
                    case EnumArrowDirection.RIGHT:
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex += addIndexX;
                        break;
                    case EnumArrowDirection.LEFTDOWN:
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex -= addIndexX;
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex -= addIndexY;
                        break;
                    case EnumArrowDirection.DOWN:
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex -= addIndexY;
                        break;
                    case EnumArrowDirection.RIGHTDOWN:
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex += addIndexX;
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex -= addIndexY;
                        break;
                    default:
                        break;
                }

                if (AddCheckBoxIsChecked == true)
                {
                    await DutAdd();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ==> CmdDutEditerZoomIn
        private RelayCommand<object> _CmdDutEditerZoomIn;
        public ICommand CmdDutEditerZoomIn
        {
            get
            {
                if (null == _CmdDutEditerZoomIn) _CmdDutEditerZoomIn = new RelayCommand<object>(DutEditerZoomIn);
                return _CmdDutEditerZoomIn;
            }
        }
        private void DutEditerZoomIn(object noparam)
        {
            try
            {
                if (ZoomLevel >= 50)
                {
                    ZoomLevel = 50;
                }
                else
                {
                    ZoomLevel++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CmdDutEditerZoomOut
        private RelayCommand<object> _CmdDutEditerZoomOut;
        public ICommand CmdDutEditerZoomOut
        {
            get
            {
                if (null == _CmdDutEditerZoomOut) _CmdDutEditerZoomOut = new RelayCommand<object>(DutEditerZoomOut);
                return _CmdDutEditerZoomOut;
            }
        }
        private void DutEditerZoomOut(object noparam)
        {
            try
            {
                if (ZoomLevel <= 1)
                {
                    ZoomLevel = 1;
                }
                else
                {
                    ZoomLevel--;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> CmdExportCardData
        private AsyncCommand _CmdExportCardData;
        public ICommand CmdExportCardData
        {
            get
            {
                if (null == _CmdExportCardData) _CmdExportCardData = new AsyncCommand(ExportCardData);
                return _CmdExportCardData;
            }
        }
        private EnumMessageDialogResult MessageDialogResult = EnumMessageDialogResult.UNDEFIND;
        private async Task ExportCardData()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                int fileCount = 0;
                bool bExist = true;
                MessageDialogResult = await this.MetroDialogManager().ShowMessageDialog("Do you want to export the ProbeCard data?",
                                    "Click OK to export",
                                    EnumMessageStyle.AffirmativeAndNegative);
                if (MessageDialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    
                    string DeviceParamRootDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory;
                    string filePath = Path.Combine(DeviceParamRootDirectory, "ProbeCard", "ProbeCard_" + this.FileManager().GetDeviceName() + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
                    if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    while (bExist)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            fileCount++;
                            string[] splitFilePath = filePath.Split('.');
                            if (splitFilePath[0].Contains('('))
                            {
                                string[] splitFilePath2 = splitFilePath[0].Split('(');
                                splitFilePath2[0] += "(" + fileCount + ")" + ".csv";
                                filePath = splitFilePath2[0];
                            }
                            else
                            {
                                splitFilePath[0].Split('(');
                                splitFilePath[0] += "(" + fileCount + ")" + ".csv";
                                filePath = splitFilePath[0];
                            }
                        }
                        else
                        {
                            bExist = false;
                        }
                    }
                    SetProberCardDataToCSVFile(filePath);
                    var ret = await this.MetroDialogManager().ShowMessageDialog("Export Success", filePath,
                    MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        private void SetProberCardDataToCSVFile(string csv_file_path)
        {
            try
            {
                AsyncObservableCollection<IDut> tmpList = new AsyncObservableCollection<IDut>();
                AsyncObservableCollection<IDut> DutList = new AsyncObservableCollection<IDut>();
                Dut tmpDut = new Dut();
                tmpDut.DutNumber = 0;
                bool check = false;

                bool flip = false;
                if (this.CoordinateManager().GetReverseManualMoveX() &&
                    this.CoordinateManager().GetReverseManualMoveY())
                {
                    flip = true;
                }


                Func<IDut, int, int, bool> matchCondition = (IDut dut, int i, int j) =>
                {
                    bool ismatched = false;
                    int invertRowRatio = this.GetParam_Wafer().GetPhysInfo().MapCountX.Value - 1;
                    int colMaxIndex = this.GetParam_Wafer().GetPhysInfo().MapCountY.Value - 1;

                    if (flip)
                    {
                        ismatched = (invertRowRatio - i == dut.MacIndex.XIndex) &&
                                        (colMaxIndex - j == dut.MacIndex.YIndex);
                    }
                    else
                    {
                        ismatched = dut.MacIndex.XIndex == i && dut.MacIndex.YIndex == j;
                    }
                    return ismatched;
                };



                using (StreamWriter sw = new StreamWriter(csv_file_path))
                {
                    String columnLine = String.Empty;

                    DutList = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;
                    int col = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX;
                    int row = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY;

                    for (int i = 0; i < col; i++)
                    {
                        for (int j = 0; j < row; j++)
                        {
                            var vList = DutList.Where(item => matchCondition(item, i, j));
                            foreach (var n in vList)
                            {
                                tmpList.Add(n);
                                check = true;
                            }
                            if (!check)
                            {
                                tmpList.Add(tmpDut);
                            }
                            check = false;
                        }
                    }

                    int count = row;
                    for (int i = 0; i < row; i++)
                    {
                        count = row - i;
                        for (int j = 0; j < col; j++)
                        {
                            sw.Write(tmpList[count-1].DutNumber);
                            if (j < col - 1)
                            {
                                count = count + row;
                                sw.Write(",");
                            }
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                }
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Export Failed", "This file is being used by another process",
                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CmdImportCardData
        private AsyncCommand _CmdImportCardData;
        public ICommand CmdImportCardData
        {
            get
            {
                if (null == _CmdImportCardData) _CmdImportCardData = new AsyncCommand(ImportCardData);
                return _CmdImportCardData;
            }
        }
        OpenFileDialogResult result = null;
        public async Task<EventCodeEnum> ImportCardData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                string filePath = "";
                Stream importFile = null;
                
                if (CSVFileStream != null)
                {
                    importFile = CSVFileStream;
                    filePath = CSVFilePath;
                }
                else
                {
                    //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    OpenFileDialogArguments dialogArgs = new OpenFileDialogArguments()
                    {
                        Width = 900,
                        Height = 700,
                        CurrentDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory,
                        Filters = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
                    };

                    result = await Application.Current.Dispatcher.Invoke<Task<OpenFileDialogResult>>(() =>
                    {
                        Task<OpenFileDialogResult> dialogResult = null;
                        try
                        {
                            dialogResult = MaterialDesignExtensions.Controls.OpenFileDialog.ShowDialogAsync("dialogHost", dialogArgs);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                        return dialogResult;
                    });

                    if (result.Canceled != true)
                    {
                        importFile = result.FileInfo.OpenRead();
                        filePath = result.FileInfo.FullName;
                    }
                }
                if (importFile != null)
                {
                    AsyncObservableCollection<IDut> tmp = GetProberCardDataTableFromCSVFile(importFile);

                    if (tmp.Count < 1)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Csv Import", "The ProberCard Info of the imported CSV file is invalid. Check import file", EnumMessageStyle.Affirmative);
                        return retVal;
                    }

                    if (tmp != null)
                    {
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList = tmp;
                        if (CSVFileStream == null)
                        {
                            LoggerManager.Debug($"Success to update ProberCard from csv file { filePath}");
                            await this.MetroDialogManager().ShowMessageDialog("Import Success", filePath,
                            MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                        retVal = EventCodeEnum.NONE;
                    }
                }
                return retVal;
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Import Failed", "This file is being used by another process",
                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                return EventCodeEnum.EXCEPTION;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
            finally
            {
                CSVFileStream = null;
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private bool CheckValidDUTdata(ObservableCollection<IDut> curDut)
        {
            try
            {
                int curcnt;

                if (curDut.Count <= 0)
                    return false;

                // Sort dut list
                List<IDut> tmpList = new List<IDut>(curDut);

                tmpList.Sort(delegate (IDut dut1, IDut dut2)
                {
                    if (dut1.DutNumber > dut2.DutNumber) return 1;
                    if (dut1.DutNumber <= dut2.DutNumber) return -1;
                    return 0;
                });

                curDut = new ObservableCollection<IDut>(tmpList);

                // Check empty number
                curcnt = 1;
                foreach (Dut chkList in curDut)
                {
                    if (curcnt != chkList.DutNumber)
                    {
                        return false;
                    }

                    curcnt += 1;
                }

                return true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }

            return false;
        }


        private AsyncObservableCollection<IDut> GetProberCardDataTableFromCSVFile(Stream csv_file_path)
        {
            AsyncObservableCollection<IDut> dutList = new AsyncObservableCollection<IDut>();
            int curcnt = 0;
            int zeroCntRow = 0;
            int zeroCntFirstCol = 0;
            int zeroCntLastCol = 0;
            var fieldLength = 0;
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(",");
                    csvReader.HasFieldsEnclosedInQuotes = true;

                    int row = 0;

                    while (csvReader.EndOfData == false)
                    {
                        String[] fieldData = csvReader.ReadFields();
                        fieldLength = fieldData.Length;
                        for (int col = 0; col < fieldData.Length; col++)
                        {
                            ////==> Empty Dut일 경우 Skim
                            //if (fieldData[col] == "X")
                            //    continue;

                            //String[] split = fieldData[col].Split(':');
                            //if (split.Length != 2)
                            //    continue;

                            int number = 0;
                            //String numberStr = split[0];
                            if (int.TryParse(fieldData[col], out number) == false)
                            {
                                LoggerManager.Debug($"The ProberCard Info of the imported CSV file is invalid. DutList[{col}][{row}] = { fieldData[col].ToString()}");
                                dutList.Clear();
                                return dutList;
                            }
                            if (number == 0)
                            {
                                if (row == 0 || csvReader.EndOfData)
                                {
                                    zeroCntRow++;
                                    if (zeroCntRow == fieldData.Length)
                                    {
                                        dutList.Clear();
                                        return dutList;
                                    }
                                }
                                if (col == 0)
                                {
                                    zeroCntFirstCol++;
                                    if (csvReader.EndOfData)
                                    {
                                        if (zeroCntFirstCol-1 == row)
                                        {
                                            dutList.Clear();
                                            return dutList;
                                        }
                                    }
                                }
                                else if (col == fieldData.Length - 1)
                                {
                                    zeroCntLastCol++;
                                    if (csvReader.EndOfData)
                                    {
                                        if (zeroCntLastCol-1 == row)
                                        {
                                            dutList.Clear();
                                            return dutList;
                                        }
                                    }
                                }
                                continue;
                            }

                            Dut newDut = new Dut();
                            newDut.DutNumber = number;
                            newDut.DutEnable = true;
                            newDut.MacIndex.XIndex = col;
                            newDut.MacIndex.YIndex = row;
                            dutList.Add(newDut);
                        }
                        row++;
                        zeroCntRow = 0;
                    }

                    //==> User Index : Left, Down Basepoint
                    int invertRowRatio = row - 1;
                    int colMaxIndex = fieldLength - 1;

                    foreach (IDut dit in dutList)
                    {
                        if(this.CoordinateManager().GetReverseManualMoveX())
                        {
                            dit.MacIndex.XIndex = colMaxIndex - dit.MacIndex.XIndex;
                        }
                        else
                        {
                            dit.MacIndex.XIndex = dit.MacIndex.XIndex;
                        }

                        if (this.CoordinateManager().GetReverseManualMoveY())
                        {
                            dit.MacIndex.YIndex = dit.MacIndex.YIndex;
                        }
                        else
                        {
                            dit.MacIndex.YIndex = invertRowRatio - dit.MacIndex.YIndex;
                        }
                    }
                }

                // Sort dut list
                List<IDut> tmpList = new List<IDut>(dutList);

                tmpList.Sort(delegate (IDut dut1, IDut dut2)
                {
                    if (dut1.DutNumber > dut2.DutNumber) return 1;
                    if (dut1.DutNumber < dut2.DutNumber) return -1;
                    return 0;
                });

                dutList = new AsyncObservableCollection<IDut>(tmpList);

                // 제일 작은 머신 인덱스를 찾아 0이 아닌 경우 시프트 한다.
                // (엑셀파일에서 만들 때 좌측/상단에 빈 공간을 남겨둔 경우
                long minX = tmpList.Min(item => item.MacIndex.XIndex);
                long minY = tmpList.Min(item => item.MacIndex.YIndex);

                if (minX > 0 || minY > 0)
                {
                    foreach (IDut sortList in tmpList)
                    {
                        sortList.MacIndex.XIndex += minX;
                        sortList.MacIndex.YIndex += minY;
                    }
                }

                // Check empty number
                curcnt = 1;
                foreach (Dut chkList in dutList)
                {
                    if (curcnt != chkList.DutNumber)
                    {
                        LoggerManager.Debug($"Failed to import csv file.  #Found missed dut number : {curcnt},  in VmDutEditorPage.cs");
                        dutList.Clear();
                        return dutList;
                    }

                    curcnt += 1;
                }

                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX = (int)dutList.Max(item => item.MacIndex.XIndex) + 1;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY = (int)dutList.Max(item => item.MacIndex.YIndex) + 1;

                // Change MachineIndex Y Value
                //int minDutNumber = dutList.Min(item => item.DutNumber);
                //Dut userIndexBaseDut = dutList.FirstOrDefault(item => item.DutNumber == minDutNumber);
                //userIndexBaseDut.UserIndex.XIndex = 0;
                //userIndexBaseDut.UserIndex.YIndex = 0;

                // Cacluate relative index from first dut
                foreach (Dut dut in dutList)
                {
                    // Modifed by Randy 2020-08-17

                    //UserIndex ui = this.CoordinateManager().MachineIndexConvertToUserIndex(dut.MacIndex);

                    //dut.UserIndex.XIndex = ui.XIndex;
                    //dut.UserIndex.YIndex = ui.YIndex;

                    
                    // refdut 아님 진짜 index 0 으로 가지고 와야함 
                    dut.UserIndex.XIndex = dut.MacIndex.XIndex - dutList[0].MacIndex.XIndex;
                    dut.UserIndex.YIndex = dut.MacIndex.YIndex - dutList[0].MacIndex.YIndex;
                }

                return dutList;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
                dutList.Clear();
                return dutList;
            }

        }
        #endregion

        #region ==> cmdSaveProbeCardData
        private RelayCommand<object> _cmdSaveProbeCardData;
        public ICommand cmdSaveProbeCardData
        {
            get
            {
                if (null == _cmdSaveProbeCardData) _cmdSaveProbeCardData = new RelayCommand<object>(FunccmdSaveProbeCardData);
                return _cmdSaveProbeCardData;
            }
        }
        private void FunccmdSaveProbeCardData(object noparam)
        {
            try
            {
                if (CheckValidDUTdata(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList) == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Warning", "Current DUT has not enough data, " + Environment.NewLine + "Please finish setup first!", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //IStageSupervisor supervisor = this.StageSupervisor();
                    //var prepinstate = this.StageSupervisor().ProbeCardInfo.AlignState.SetupState;
                    //var prewaferstate = (this.StageSupervisor().WaferObject.GetSubsInfo() as IAlignModule).AlignState.SetupState;

                    //this.StageSupervisor().ProbeCardInfo = ProbeCardData.Copy() as ProbeCard;
                    //this.StageSupervisor().ProbeCardInfo.AlignState = ProbeCardData.AlignState;

                    this.StageSupervisor().SaveProberCard();

                    //this.StageSupervisor().LoadProberCard();

                    var prepinstate = this.StageSupervisor().ProbeCardInfo.AlignState.DoneState;
                    var prewaferstate = (this.StageSupervisor().WaferObject.GetSubsInfo() as IAlignModule).AlignState.DoneState;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> DutDeleteCommand
        private AsyncCommand _DutDeleteCommand;
        public IAsyncCommand DutDeleteCommand
        {
            get
            {
                if (null == _DutDeleteCommand) _DutDeleteCommand = new AsyncCommand(DutDelete);
                return _DutDeleteCommand;
            }
        }
        public async Task DutDelete()
        {
            try
            {
                long SelectedDutX = 0;
                long SelectedDutY = 0;
                Dut tmpDut = new Dut();
                long dist = 0;
                int dut_number = 0;
                long MinMX = 0;
                long MinMY = 0;

                List<Dut> imsiList = new List<Dut>();

                var dutList = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.ToList();

                if (dutList.Count <= 0) return;

                bool retBool = await ConfirmToChange();
                if (retBool == false) return;

                // 현재 1번 더트가 있는 위치로부터 선택된 더트까지의 상대거리가 유저 좌표가 된다.
                tmpDut.UserIndex.XIndex = (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.XIndex);
                tmpDut.UserIndex.YIndex = (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.YIndex);
                // 선택된 더트가 이미 찍었던 곳이 아닌 경우 무시한다.
                //var dutList = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.ToList();
                var matchedDut = dutList.Find(d => d.UserIndex.XIndex == tmpDut.UserIndex.XIndex & d.UserIndex.YIndex == tmpDut.UserIndex.YIndex);
                if (matchedDut != null)
                {
                    LoggerManager.Debug($"[DutEditor] Deleted dut information. Dut Number = {matchedDut.DutNumber}, UI = {matchedDut.UserIndex.XIndex}, {matchedDut.UserIndex.YIndex}, MI = {matchedDut.MacIndex.XIndex}, {matchedDut.MacIndex.YIndex}");
                }
                Task task = new Task(() =>
                {

                    if (matchedDut != null)
                    {
                        long shiftX = 0;
                        long shiftY = 0;

                        dut_number = 1;
                        foreach (Dut curList in dutList)
                        {
                            if (matchedDut.DutNumber == 1 && curList.DutNumber == 2)
                            {
                                shiftX = curList.UserIndex.XIndex;
                                shiftY = curList.UserIndex.YIndex;
                            }
                            if (curList.DutNumber != matchedDut.DutNumber)
                            {
                                curList.DutNumber = dut_number;
                                curList.UserIndex.XIndex -= shiftX;     // 지우는 더트가 1번 더트일 경우 2번째 더트를 기준으로 모든 더트의 상대 좌표가 전부 시프트되어야한다.
                                curList.UserIndex.YIndex -= shiftY;
                                imsiList.Add(curList);
                                dut_number += 1;
                            }
                        }

                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Clear();
                        foreach (Dut curList2 in imsiList)
                        {
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Add(curList2);
                        }

                        // 지우고 보니 지운 더트가 최후에 남은 가장 좌측 더트이거나 가장 하단 더트인경우, 즉 이로 인하여 머신 좌표가 전부 다 시프트 되어야 하는 경우 처리
                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count != 0)
                        {
                            MinMX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Min(item => item.MacIndex.XIndex);
                            MinMY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Min(item => item.MacIndex.YIndex);
                        }

                        if (MinMX > 0 || MinMY > 0)
                        {
                            foreach (Dut curList2 in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                curList2.MacIndex.XIndex -= MinMX;
                                curList2.MacIndex.YIndex -= MinMY;
                            }
                        }

                        var prevDut = dutList.Find(x => x.DutNumber == matchedDut.DutNumber - 1);

                        if (prevDut != null)
                        {
                            this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex = this.StageSupervisor().ProbeCardInfo.FirstDutM.XIndex + prevDut.UserIndex.XIndex;
                            this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex = this.StageSupervisor().ProbeCardInfo.FirstDutM.YIndex + prevDut.UserIndex.YIndex;
                        }

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count != 0)
                        {
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX = (int)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Max(item => item.MacIndex.XIndex) + 1;
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY = (int)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Max(item => item.MacIndex.YIndex) + 1;
                        }
                        
                    }
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //if (this.StageSupervisor().ProbeCardInfo.TempDutList.Count > 0)
            //{
            //    long MaxNumber = this.StageSupervisor().ProbeCardInfo.TempDutList.Max(x => x.DutNumber.Value);

            //    this.StageSupervisor().ProbeCardInfo.TempDutList.Remove(this.StageSupervisor().ProbeCardInfo.TempDutList.Find(x => x.DutNumber.Value == MaxNumber));

            //    this.StageSupervisor().ProbeCardInfo.SelectDutIndex.XIndex.Value = this.StageSupervisor().ProbeCardInfo.TempDutList.Find(x => x.DutNumber.Value == (MaxNumber - 1)).MacIndex.XIndex.Value;
            //    this.StageSupervisor().ProbeCardInfo.SelectDutIndex.YIndex.Value = this.StageSupervisor().ProbeCardInfo.TempDutList.Find(x => x.DutNumber.Value == (MaxNumber - 1)).MacIndex.YIndex.Value;
            //}

            //if (this.StageSupervisor().ProbeCardInfo.IsDutEditAble 
            //    && this.StageSupervisor().ProbeCardInfo.SelectDutIndex != null 
            //    && this.StageSupervisor().ProbeCardInfo.TempDutList.Find
            //    (dut => dut.MacIndex.XIndex == this.StageSupervisor().ProbeCardInfo.SelectDutIndex.XIndex && dut.MacIndex.YIndex == this.StageSupervisor().ProbeCardInfo.SelectDutIndex.YIndex) != null)
            //{
            //    this.StageSupervisor().ProbeCardInfo.TempDutList.Remove(this.StageSupervisor().ProbeCardInfo.TempDutList.Find(dut => dut.MacIndex.XIndex == this.StageSupervisor().ProbeCardInfo.SelectDutIndex.XIndex && dut.MacIndex.YIndex == this.StageSupervisor().ProbeCardInfo.SelectDutIndex.YIndex));
            //}
        }
        #endregion

        #region ==> DutAddCommandByMouseDown;
        private AsyncCommand _DutAddMouseDownCommand;
        public IAsyncCommand DutAddMouseDownCommand
        {
            get
            {
                if (null == _DutAddMouseDownCommand) _DutAddMouseDownCommand = new AsyncCommand(DutAddbyMouseDown);
                return _DutAddMouseDownCommand;
            }
        }

        public async Task DutAddbyMouseDown()
        {
            try
            {
                if (AddCheckBoxIsChecked == true)
                {
                    await DutAdd();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. DutAddbyMouseDown() : Error occured.");
                LoggerManager.Exception(err);
            }

        }
        #endregion 

        #region ==> DutAddCommand
        private AsyncCommand _DutAddCommand;
        public IAsyncCommand DutAddCommand
        {
            get
            {
                if (null == _DutAddCommand) _DutAddCommand = new AsyncCommand(DutAdd);
                return _DutAddCommand;
            }
        }

        public bool IsEnableMoving => throw new NotImplementedException();

        private void SetAssociateParam()
        {
            try
            {
                this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.DoneState = ElementStateEnum.NEEDSETUP;
                this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value.Clear();
                this.GetParam_Wafer().PadSetupChangedToggle.DoneState = ElementStateEnum.NEEDSETUP;
                this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Clear();
                this.GetParam_ProbeCard().PinSetupChangedToggle.DoneState = ElementStateEnum.NEEDSETUP;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task<bool> ConfirmToChange()
        {
            try
            {
                bool bRet = false;

                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                {
                    const string message = "Previous aligned result will be cleared,\nDo you want to continue?";
                    const string caption = "Warning";

                    EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);

                    Task task = new Task(() =>
                    {
                        if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            SetAssociateParam();
                            this.StageSupervisor().ProbeCardInfo.ProbeCardChangedToggle.Value = true;
                            this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                            bRet = true;
                        }
                        else
                        {
                            bRet = false;
                        }
                    });
                    task.Start();
                    await task;
                }
                else
                {

                    Task task = new Task(() =>
                    {
                        SetAssociateParam();
                        this.StageSupervisor().ProbeCardInfo.ProbeCardChangedToggle.Value = true;
                        this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                        bRet = true;
                    });
                    task.Start();
                    await task;
                }

                return bRet;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        private async Task<bool> ConfirmToClear()
        {
            try
            {
                bool bRet = false;

                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardChangedToggle.Value == false ||
                //    this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                //{
                const string message = "Previous probe card data & aligned result will be cleared,\nDo you want to continue?";
                const string caption = "Warning";
                EnumMessageDialogResult answer = await this.MetroDialogManager()?.ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);

                Task task = new Task(() =>
                {

                    if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        SetAssociateParam();
                        this.StageSupervisor().ProbeCardInfo.ProbeCardChangedToggle.Value = true;
                        this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
                    }
                });
                task.Start();
                await task;
                //}
                //else
                //{
                //    bRet = true;
                //}

                return bRet;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public async Task DutAdd()
        {
            try
            {
                Dut tmpDut = new Dut();
                long dist = 0;

                bool retBool = await ConfirmToChange();
                if (retBool == false) return;

                Task task = new Task(() =>
                {

                    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count <= 0)
                    {
                        // 첫번째 기준 더트
                        tmpDut.MacIndex.XIndex = 0;
                        tmpDut.MacIndex.YIndex = 0;
                        tmpDut.UserIndex.XIndex = 0;
                        tmpDut.UserIndex.YIndex = 0;
                        tmpDut.DutNumber = 1;
                        tmpDut.DutEnable = true;

                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Add(tmpDut);
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX = 1;
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY = 1;
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex = 0;
                        this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex = 0;
                    }
                    else
                    {
                        // 현재 1번 더트가 있는 위치로부터 선택된 더트까지의 상대거리가 유저 좌표가 된다.
                        tmpDut.UserIndex.XIndex = (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.XIndex);
                        tmpDut.UserIndex.YIndex = (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.YIndex);

                        // 맨 처음 DUT를 생성했을 당시 첫번째 더트 의 머신 좌표는 항상 (1, 1)로 고정했으니 거기에서부터 현재 찍은 위치의 상대거리가 더트의 머신 좌표가 된다.                
                        // 현재 찍은 위치에서 1번 더트까지 거리가 0보다 작을 경우, 그 거리만큼 이전 좌표들을 전부 시프트 시켜주어야 한다.
                        dist = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex + (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.XIndex);
                        if (dist < 0)
                        {
                            tmpDut.MacIndex.XIndex = 0; // 이 더트가 가장 좌측이 된다.
                            foreach (Dut lDut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                lDut.MacIndex.XIndex += Math.Abs(dist);
                            }
                        }
                        else
                        {
                            tmpDut.MacIndex.XIndex = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex + (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.XIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.XIndex);
                        }

                        dist = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex + (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.YIndex);
                        if (dist < 0)
                        {
                            tmpDut.MacIndex.YIndex = 0; // 이 더트가 가장 좌측이 된다.
                            foreach (Dut lDut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                lDut.MacIndex.YIndex += Math.Abs(dist);
                            }
                        }
                        else
                        {
                            tmpDut.MacIndex.YIndex = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex + (this.StageSupervisor().ProbeCardInfo.SelectedCoordM.YIndex - this.StageSupervisor().ProbeCardInfo.FirstDutM.YIndex);
                        }

                        // 추가한 더트가 이미 찍었던 곳에 다시 찍은 경우 무시한다.
                        var dutList = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.ToList();
                        var matchedDut = dutList.Find(d => d.MacIndex.XIndex == tmpDut.MacIndex.XIndex & d.MacIndex.YIndex == tmpDut.MacIndex.YIndex);

                        if (matchedDut == null)
                        {
                            tmpDut.DutNumber = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count + 1;
                            tmpDut.DutEnable = true;

                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Add(tmpDut);
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX = (int)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Max(item => item.MacIndex.XIndex) + 1;
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY = (int)this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Max(item => item.MacIndex.YIndex) + 1;

                            LoggerManager.Debug($"[DutEditor] Added dut information. Dut Number = {tmpDut.DutNumber}, UI = {tmpDut.UserIndex.XIndex}, {tmpDut.UserIndex.YIndex}, MI = {tmpDut.MacIndex.XIndex}, {tmpDut.MacIndex.YIndex}");
                        }
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //public EventCodeEnum InitPage(object parameter = null)
        //{
        //    return EventCodeEnum.NONE;
        //}

        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                Task task = new Task(() =>
                {
                    retVal = CopyingProbeCardData();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                Task task = new Task(() =>
                {
                    this.WaferObject = this.StageSupervisor().WaferObject;
                    this.ProbeCard = this.StageSupervisor().ProbeCardInfo;
                    CamType = EnumProberCam.UNDEFINED;

                    ShowPad = false;
                    ShowPin = false;
                    EnableDragMap = false;
                    ShowSelectedDut = true;
                    ShowGrid = true;
                    ShowCurrentPos = false;

                    if (this.StageSupervisor().ProbeCardInfo != null)
                    {
                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef != null)
                        {
                            CurrentDutCount = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count;
                        }
                    }

                    retVal = CopyingProbeCardData();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum CopyingProbeCardData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //ProbeCardData = this.StageSupervisor().ProbeCardInfo as ProbeCard;
                //this.StageSupervisor().ProbeCardInfo = this.GetParam_ProbeCard();

                if (this.StageSupervisor().ProbeCardInfo != null)
                {
                    if (this.StageSupervisor().ProbeCardInfo.FirstDutM == null)
                    {
                        this.StageSupervisor().ProbeCardInfo.FirstDutM = new MachineIndex();
                        this.StageSupervisor().ProbeCardInfo.FirstDutM.XIndex = 0;
                        this.StageSupervisor().ProbeCardInfo.FirstDutM.YIndex = 0;
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Error($"[VmDutEditorPage - CopyToProbeCardData()] Error during Copying ProbeCardData.");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                this.StageSupervisor().SaveProberCard();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

        public DutEditorDataDescription GetDutEditorInfo()
        {
            DutEditorDataDescription info = new DutEditorDataDescription();

            try
            {
                info.ZoomLevel = ZoomLevel;
                info.AddCheckBoxIsChecked = AddCheckBoxIsChecked;
                info.ShowPad = ShowPad;
                info.ShowPin = ShowPin;
                info.EnableDragMap = EnableDragMap;
                info.ShowSelectedDut = ShowSelectedDut;
                info.ShowGrid = ShowGrid;
                info.ShowCurrentPos = ShowCurrentPos;

                info.SelectedCoordM = this.ProbeCard.SelectedCoordM;
                info.FirstDutM = this.ProbeCard.FirstDutM;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return info;
        }

        public void ChangedSelectedCoordM(MachineIndex param)
        {
            try
            {
                this.StageSupervisor().ProbeCardInfo.SelectedCoordM = param;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangedChangedFirstDutM(MachineIndex param)
        {
            try
            {
                this.StageSupervisor().ProbeCardInfo.FirstDutM = param;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        public void ChangedAddCheckBoxIsChecked(bool? param)
        {
            try
            {
                this.AddCheckBoxIsChecked = param;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] GetDutlist()
        {
            byte[] compressedData = null;
            //var dutlist = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;

            WrapperDutlist wrapper = new WrapperDutlist();
            //wrapper.DutList = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;

            List<IDut> tmpdutlist = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.ToList();

            wrapper.DutList = new AsyncObservableCollection<IDut>(tmpdutlist);

            //foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
            //{
            //    Dut tmpdut = new Dut();

            //    (dut as Dut).Copy(ref tmpdut);

            //    wrapper.DutList.Add(tmpdut);
            //}

            try
            {
                var bytes = SerializeManager.SerializeToByte(wrapper, typeof(WrapperDutlist));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return compressedData;
        }
    }
}
