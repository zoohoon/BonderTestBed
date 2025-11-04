using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderDutEditorViewModelModule
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LogModule;
    using MaterialDesignExtensions.Controls;
    using MetroDialogInterfaces;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.ControlClass.ViewModel.DutEditor;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using RelayCommandBase;
    using SerializerUtil;
    using ServiceProxy;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;

    public class LoaderDutEditorViewModel : IMainScreenViewModel, IDutEditorVM
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("f4f93721-ecce-4436-bb6b-cafeb5f3ca69");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        private IVisionManager _VisionManager;
        public IVisionManager VisionManager
        {
            get { return _VisionManager; }
            set
            {
                if (value != _VisionManager)
                {
                    _VisionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMotionManager _MotionManager;
        public IMotionManager MotionManager
        {
            get { return _MotionManager; }
            set
            {
                if (value != _MotionManager)
                {
                    _MotionManager = value;
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

        // Not Use
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

        public EnumProberCam CamType { get; internal set; }
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

                    ChangedAddCheckBoxIsChecked(_AddCheckBoxIsChecked);

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

        public void ChangedSelectedCoordM(MachineIndex param)
        {
            try
            {
                _RemoteMediumProxy.DutEditor_ChangedSelectedCoordM(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangedFirstDutM(MachineIndex param)
        {
            try
            {
                _RemoteMediumProxy.DutEditor_ChangedSelectedCoordM(param);
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
                _RemoteMediumProxy.DutEditor_ChangedFirstDutM(param);
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
                _RemoteMediumProxy.DutEditor_ChangedAddCheckBoxIsChecked(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsEnableMoving => throw new NotImplementedException();

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
                var stage = (StageSupervisorProxy)_LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                int fileCount = 0;
                bool bExist = true;

                MessageDialogResult = await this.MetroDialogManager().ShowMessageDialog("Do you want to export the ProberCard data?",
                               "Click OK to export",
                               EnumMessageStyle.AffirmativeAndNegative);
                if (MessageDialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    string DeviceParamRootDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory;
                    string filePath = Path.Combine(DeviceParamRootDirectory, "ProbeCard", "ProbeCard_" + stage.GetDeviceName() + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
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
                    if (SetWaferMapDataToCSVFile(filePath) == EventCodeEnum.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Export Success", filePath,
                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        LoggerManager.Debug($"Success to export ProberCard to csv file { filePath}");
                    }
                    
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
        private EventCodeEnum SetWaferMapDataToCSVFile(string csv_file_path)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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
                            sw.Write(tmpList[count - 1].DutNumber);
                            if (j < col - 1)
                            {
                                count = count + row;
                                sw.Write(",");
                            }
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    retVal = EventCodeEnum.NONE;
                }
                return retVal;
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Export Failed", "This file is being used by another process",
                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                return EventCodeEnum.EXCEPTION;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
        }

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
        private async Task ImportCardData()
        {
            try
            {
                string filePath = null;
                Stream fileStream = null;
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
                    filePath = result.FileInfo.FullName;
                    fileStream = result.FileInfo.OpenRead();
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    _RemoteMediumProxy.DutEditor_ImportFilePath(filePath);
                    EventCodeEnum retVal = await _RemoteMediumProxy.DutEditor_CmdImportCardDataCommand(fileStream);

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //Task.Run(() =>
                        //{
                        //    UpdateDutlist();
                        //    UpdateDutEditorInfo();
                        //});
                        await UpdateDutlist();
                        Task task = new Task(() =>
                        {
                            
                            UpdateDutEditorInfo();
                        });
                        task.Start();
                        await task;

                        LoggerManager.Debug($"Success to update ProberCard from csv file { filePath}");
                        this.MetroDialogManager().ShowMessageDialog("Import Success", filePath,
                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
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

        private AsyncCommand _InitializePalletCommand;
        public IAsyncCommand InitializePalletCommand
        {
            get
            {
                if (null == _InitializePalletCommand) _InitializePalletCommand = new AsyncCommand(FuncInitializePalletCommand);
                return _InitializePalletCommand;
            }
        }

        private async Task FuncInitializePalletCommand()
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                await _RemoteMediumProxy.DutEditor_InitializePalletCommand();

                (this.StageSupervisor() as IStageSupervisorServiceClient).UpdateProbeCardObject();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //finally
            //{
            //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
        }

        private AsyncCommand _DutAddCommand;
        public IAsyncCommand DutAddCommand
        {
            get
            {
                if (null == _DutAddCommand) _DutAddCommand = new AsyncCommand(DutAdd);
                return _DutAddCommand;
            }
        }

        public async Task DutAdd()
        {
            try
            {

                Task changetask = new Task(() =>
                {
                    _RemoteMediumProxy.DutEditor_ChangedSelectedCoordM(this.StageSupervisor().ProbeCardInfo.SelectedCoordM);
                    _RemoteMediumProxy.DutEditor_ChangedFirstDutM(this.StageSupervisor().ProbeCardInfo.FirstDutM);

                });
                changetask.Start();
                await changetask;
                //AsyncHelpers.RunSync(() => _RemoteMediumProxy.DutEditor_DutAddCommand());

                await _RemoteMediumProxy.DutEditor_DutAddCommand();

                //Task.Run(() =>
                //{
                //    UpdateDutlist();
                //    UpdateDutEditorInfo();
                //});
                await UpdateDutlist();
                Task task = new Task(() =>
                {
                    //UpdateDutlist();
                    UpdateDutEditorInfo();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DutDeleteCommand;
        public IAsyncCommand DutDeleteCommand
        {
            get
            {
                if (null == _DutDeleteCommand) _DutDeleteCommand = new AsyncCommand(DutDelete);
                return _DutDeleteCommand;
            }
        }
        private async Task DutDelete()
        {
            try
            {
                Task changetask = new Task(() =>
                {
                    _RemoteMediumProxy.DutEditor_ChangedSelectedCoordM(this.StageSupervisor().ProbeCardInfo.SelectedCoordM);
                    _RemoteMediumProxy.DutEditor_ChangedFirstDutM(this.StageSupervisor().ProbeCardInfo.FirstDutM);
                });
                changetask.Start();
                await changetask;

                //AsyncHelpers.RunSync(() => _RemoteMediumProxy.DutEditor_DutDeleteCommand());

                await _RemoteMediumProxy.DutEditor_DutDeleteCommand();

                await UpdateDutlist();
                Task task = new Task(() =>
                {
                    //UpdateDutlist();
                    UpdateDutEditorInfo();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


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
                _RemoteMediumProxy.DutEditor_ZoomInCommand();

                UpdateDutEditorInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

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
                _RemoteMediumProxy.DutEditor_ZoomOutCommand();

                UpdateDutEditorInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<EnumArrowDirection> _DutEditerMoveCommand;
        public ICommand DutEditerMoveCommand
        {
            get
            {
                if (null == _DutEditerMoveCommand) _DutEditerMoveCommand = new AsyncCommand<EnumArrowDirection>(DutEditerMoveCommandFunc);
                return _DutEditerMoveCommand;
            }
        }

        public async Task DutEditerMoveCommandFunc(EnumArrowDirection param)
        {
            try
            {
                await _RemoteMediumProxy.DutEditor_DutEditerMoveCommand(param);

                await UpdateDutlist();

                Task task = new Task(() =>
                {
                    UpdateDutEditorInfo();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DutAddMouseDownCommand;
        public IAsyncCommand DutAddMouseDownCommand
        {
            get
            {
                if (null == _DutAddMouseDownCommand) _DutAddMouseDownCommand = new AsyncCommand(DutAddbyMouseDown);
                return _DutAddMouseDownCommand;
            }
        }

        public Stream CSVFileStream { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string CSVFilePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private async Task DutAddbyMouseDown()
        {
            try
            {

                Task changetask = new Task(() =>
                {
                    _RemoteMediumProxy.DutEditor_ChangedSelectedCoordM(this.StageSupervisor().ProbeCardInfo.SelectedCoordM);
                    _RemoteMediumProxy.DutEditor_ChangedFirstDutM(this.StageSupervisor().ProbeCardInfo.FirstDutM);
                });
                changetask.Start();
                await changetask;

                //AsyncHelpers.RunSync(() => _RemoteMediumProxy.DutEditor_DutAddMouseDownCommand());
                await _RemoteMediumProxy.DutEditor_DutAddMouseDownCommand();
                await UpdateDutlist();
                Task task = new Task(() =>
                {

                    //UpdateDutlist();
                    UpdateDutEditorInfo();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. DutAddbyMouseDown() : Error occured.");
                LoggerManager.Exception(err);
            }
        }

        public LoaderDutEditorViewModel()
        {

        }
        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
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

                    //AddCheckBoxIsChecked = false;

                    //ShowPad = false;
                    //ShowPin = false;
                    //EnableDragMap = false;
                    //ShowSelectedDut = true;
                    //ShowGrid = true;
                    //ShowCurrentPos = false;

                    //ZoomLevel = 2;
                    
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

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //AsyncHelpers.RunSync(() => _RemoteMediumProxy.DutEditor_PageSwitched());

                await _RemoteMediumProxy.DutEditor_PageSwitched();

                VisionManager = this.VisionManager();
                StageSupervisor = this.StageSupervisor();
                MotionManager = this.MotionManager();

                WaferObject = this.StageSupervisor().WaferObject;
                //ProbeCard = this.StageSupervisor().ProbeCardInfo;

                CamType = EnumProberCam.UNDEFINED;

                (this.StageSupervisor() as IStageSupervisorServiceClient).UpdateProbeCardObject();

                await UpdateDutlist();
                Task task = new Task(() =>
                {
                    //UpdateDutlist();

                    UpdateDutEditorInfo();

                    ChangedSelectedCoordM(this.StageSupervisor().ProbeCardInfo.SelectedCoordM);

                    ChangedFirstDutM(this.StageSupervisor().ProbeCardInfo.FirstDutM);
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

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                await _RemoteMediumProxy.DutEditor_Cleanup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        public DutEditorDataDescription GetDutEditorInfo()
        {
            return null;
        }

        private void UpdateDutEditorInfo()
        {
            try
            {
                var info = _RemoteMediumProxy.GetDutEditorInfo();

                if (info != null)
                {
                    ZoomLevel = info.ZoomLevel;
                    AddCheckBoxIsChecked = info.AddCheckBoxIsChecked;
                    ShowPad = info.ShowPad;
                    ShowPin = info.ShowPin;
                    EnableDragMap = info.EnableDragMap;
                    ShowSelectedDut = info.ShowSelectedDut;
                    ShowGrid = info.ShowGrid;
                    ShowCurrentPos = info.ShowCurrentPos;

                    this.StageSupervisor().ProbeCardInfo.SelectedCoordM = info.SelectedCoordM;
                    this.StageSupervisor().ProbeCardInfo.FirstDutM = info.FirstDutM;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        private async Task UpdateDutlist()
        {
            try
            {
                object target = null;
                WrapperDutlist retval = null;
                Task task = new Task(() =>
                {
                    byte[] obj = _RemoteMediumProxy.DutEditor_GetDutlist();
                    if (obj != null)
                    {
                        var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(WrapperDutlist));

                        if (result == true)
                        {
                            retval = target as WrapperDutlist;

                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList = retval.DutList;
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

        public byte[] GetDutlist()
        {
            throw new NotImplementedException();
        }

        Task<EventCodeEnum> IDutEditorVM.ImportCardData()
        {
            throw new NotImplementedException();
        }

        Task IDutViewControlVM.DutAddbyMouseDown()
        {
            throw new NotImplementedException();
        }

        Task IDutEditorVM.FuncInitializePalletCommand()
        {
            throw new NotImplementedException();
        }

        Task IDutEditorVM.DutAddbyMouseDown()
        {
            throw new NotImplementedException();
        }

        Task IDutEditorVM.DutDelete()
        {
            throw new NotImplementedException();
        }
    }
}
