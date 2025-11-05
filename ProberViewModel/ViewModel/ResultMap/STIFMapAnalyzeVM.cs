using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ProberInterfaces.ResultMap;
using MetroDialogInterfaces;
using System.IO;
using MaterialDesignExtensions.Controls;
using ResultMapParamObject;
using ProberInterfaces.ResultMap.Script;

namespace ProberViewModel.ViewModel
{
    public class STIFMapAnalyzeVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("1f9ba088-6f87-4be9-8762-c653169debb7");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                IsDummy = false;
                DisplayBinCode = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private ObservableCollection<double> _STIFVersionlist;
        public ObservableCollection<double> STIFVersionlist
        {
            get { return _STIFVersionlist; }
            set
            {
                if (value != _STIFVersionlist)
                {
                    _STIFVersionlist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MapVersion;
        public double MapVersion
        {
            get { return _MapVersion; }
            set
            {
                if (value != _MapVersion)
                {
                    _MapVersion = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsDummy;
        public bool IsDummy
        {
            get { return _IsDummy; }
            set
            {
                if (value != _IsDummy)
                {
                    _IsDummy = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<MapScriptElement> _STIFResultMap;
        public ObservableCollection<MapScriptElement> STIFResultMap
        {
            get { return _STIFResultMap; }
            set
            {
                if (value != _STIFResultMap)
                {
                    _STIFResultMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IProbingSequenceModule _ProbingSeqModule;
        public IProbingSequenceModule ProbingSeqModule
        {
            get { return _ProbingSeqModule; }
            set
            {
                if (value != _ProbingSeqModule)
                {
                    _ProbingSeqModule = value;
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

        private char[,] _ASCIIMap;
        public char[,] ASCIIMap
        {
            get { return _ASCIIMap; }
            set
            {
                if (value != _ASCIIMap)
                {
                    _ASCIIMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DisplayBinCode;
        public bool DisplayBinCode
        {
            get { return _DisplayBinCode; }
            set
            {
                if (value != _DisplayBinCode)
                {
                    _DisplayBinCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CursorXIndex;
        public int CursorXIndex
        {
            get { return _CursorXIndex; }
            set
            {
                if (value != _CursorXIndex)
                {
                    _CursorXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CursorYIndex;
        public int CursorYIndex
        {
            get { return _CursorYIndex; }
            set
            {
                if (value != _CursorYIndex)
                {
                    _CursorYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CurrentBINData = string.Empty;
        public string CurrentBINData
        {
            get { return _CurrentBINData; }
            set
            {
                if (value != _CurrentBINData)
                {
                    _CurrentBINData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICoordinateManager _CoordinateManager;
        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
            set
            {
                if (value != _CoordinateManager)
                {
                    _CoordinateManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ITCPIP _TCPIPModule;
        public ITCPIP TCPIPModule
        {
            get { return _TCPIPModule; }
            set
            {
                if (value != _TCPIPModule)
                {
                    _TCPIPModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EventCodeEnum SetSTIFVersion()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (STIFVersionlist == null)
                {
                    STIFVersionlist = new ObservableCollection<double>();
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    STIFVersionlist.Clear();

                    STIFVersionlist.Add(1.1);
                    STIFVersionlist.Add(1.2);
                    STIFVersionlist.Add(1.3);
                });

                //ResultMapSysParameter sysparam = this.ResultMapManager().ResultMapSysIParameter as ResultMapSysParameter;
                ResultMapConverterParameter convparam = this.ResultMapManager().GetResultMapConvIParam() as ResultMapConverterParameter;

                if (convparam != null)
                {
                    if (this.ResultMapManager().GetUploadConverterType() == ResultMapConvertType.STIF)
                    {
                        // Get version 
                        MapVersion = convparam.STIFParam.Version.Value;
                        
                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        // TODO : ???
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SetSTIFVersion();

                if (STIFResultMap == null)
                {
                    STIFResultMap = new ObservableCollection<MapScriptElement>();
                }

                CursorXIndex = (int)this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value;
                CursorYIndex = (int)this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
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
                    ProbingSeqModule = this.ProbingSequenceModule();
                    CoordinateManager = this.CoordinateManager();
                    StageSupervisor = this.StageSupervisor();
                    TCPIPModule = this.TCPIPModule();

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
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ExportSTIFMapCommand;
        public ICommand ExportSTIFMapCommand
        {
            get
            {
                if (null == _ExportSTIFMapCommand) _ExportSTIFMapCommand = new AsyncCommand(ExportSTIFMapCommandFunc);
                return _ExportSTIFMapCommand;
            }
        }
        private EnumMessageDialogResult MessageDialogResult = EnumMessageDialogResult.UNDEFIND;
        private async Task ExportSTIFMapCommandFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                int fileCount = 0;
                bool bExist = true;

                MessageDialogResult = await this.MetroDialogManager().ShowMessageDialog("Do you want to export the STIF Map?", "Click OK to export", EnumMessageStyle.AffirmativeAndNegative);

                if (MessageDialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    string rootdir = @"C:\ProberSystem";

                    //string rootpath = Path.Combine(rootdir, "ResultMap", "STIF", DateTime.Now.ToString("yyyy-MM-dd") + "_" + this.FileManager().GetDeviceName() + ".stif");
                    string rootpath = Path.Combine(rootdir, "ResultMap", "STIF");

                    if (Directory.Exists(Path.GetDirectoryName(rootpath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(rootpath));
                    }

                    try
                    {
                        object mapfilebase = null;

                        if (this.ResultMapManager().GetOrgResultMap() == null)
                        {
                            object rm = null;

                            EventCodeEnum retval = this.ResultMapManager().MakeResultMap(ref rm);
                        }

                        mapfilebase = this.ResultMapManager().GetOrgResultMap();

                        bool IsSuccess = false;

                        if (mapfilebase != null)
                        {
                            string mapstream = string.Empty;

                            List<MapScriptElement> se = mapfilebase as List<MapScriptElement>;

                            if (se != null)
                            {
                                foreach (var line in se)
                                {
                                    mapstream += line.Text + Environment.NewLine;
                                }

                                var retval = this.ResultMapManager().ManualUpload(mapstream, FileManagerType.HDD, rootpath, FileReaderType.STREAM);

                                if(retval == EventCodeEnum.NONE)
                                {
                                    IsSuccess = true;
                                }
                                else
                                {
                                    IsSuccess = false;
                                }
                                
                            }
                            else
                            {
                                IsSuccess = false;
                            }
                        }
                        else
                        {
                            IsSuccess = false;

                        }

                        if (IsSuccess == true)
                        {
                            var ret = await this.MetroDialogManager().ShowMessageDialog("Export Success", "", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            var ret = await this.MetroDialogManager().ShowMessageDialog("Export Fail.", "", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                    }
                    catch (IOException err)
                    {
                        LoggerManager.Exception(err);
                        this.MetroDialogManager().ShowMessageDialog("Export Failed", $"Error = {err}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
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



        private AsyncCommand _ImportSTIFMapCommand;
        public ICommand ImportSTIFMapCommand
        {
            get
            {
                if (null == _ImportSTIFMapCommand) _ImportSTIFMapCommand = new AsyncCommand(ImportSTIFMapCommandFunc);
                return _ImportSTIFMapCommand;
            }
        }

        public async Task ImportSTIFMapCommandFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            OpenFileDialogResult result = null;

            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                string filePath = "";
                Stream importFile = null;

                // TODO : extension

                string rootdir = @"C:\ProberSystem\ResultMap\";

                OpenFileDialogArguments dialogArgs = new OpenFileDialogArguments()
                {
                    Width = 900,
                    Height = 700,
                    //CurrentDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory,
                    CurrentDirectory = rootdir,
                    Filters = "All files (*.*)|*.*"
                    //Filters = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
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
                    //importFile = result.FileInfo.OpenRead();
                    filePath = result.FileInfo.FullName;
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    retVal = this.ResultMapManager().ManualConvertResultMapToProberData(filePath, FileManagerType.HDD, FileReaderType.STREAM);

                    if(retVal == EventCodeEnum.NONE)
                    {
                        var resmap = this.ResultMapManager().GetOrgResultMap();

                        if(resmap != null)
                        {
                            STIFResultMap = new ObservableCollection<MapScriptElement>(resmap as List<MapScriptElement>);
                        }
                        
                        SetASCIIMap();
                    }

                    if (retVal != EventCodeEnum.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "This file is wrong format.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                }
                //   return await Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Import Failed", "This file is being used by another process", MetroDialogInterfaces.EnumMessageStyle.Affirmative);

                //return EventCodeEnum.EXCEPTION;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //return EventCodeEnum.EXCEPTION;
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private RelayCommand<Object> _STIFGenerateCommand;
        public ICommand STIFGenerateCommand
        {
            get
            {
                if (null == _STIFGenerateCommand) _STIFGenerateCommand = new RelayCommand<object>(STIFGenerateCommandFunc);
                return _STIFGenerateCommand;
            }
        }

        public EventCodeEnum SetASCIIMap()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ASCIIMap = this.ResultMapManager().GetASCIIMap();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        private void STIFGenerateCommandFunc(object obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            object resmap = null;
            bool IsSuccess = false;

            try
            {
                ResultMapConvertType converterType = this.ResultMapManager().GetUploadConverterType();

                if (converterType == ResultMapConvertType.STIF)
                {
                    object rm = null;

                    retval = this.ResultMapManager().MakeResultMap(ref rm, IsDummy);

                    if (retval == EventCodeEnum.NONE)
                    {
                        if (retval == EventCodeEnum.NONE)
                        {
                            resmap = this.ResultMapManager().GetOrgResultMap();

                            STIFResultMap = new ObservableCollection<MapScriptElement>(resmap as List<MapScriptElement>);

                            if (STIFResultMap != null)
                            {
                                IsSuccess = true;

                                SetASCIIMap();
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[ResultMapAnalyzeVM], STIFGenerateCommandFunc() : Faild to get result map.");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[ResultMapAnalyzeVM], STIFGenerateCommandFunc() : Faild to generate result map.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[ResultMapAnalyzeVM], STIFGenerateCommandFunc() : Converter type is not matched.");
                }

                //ResultMapSysParameter sysparam = this.ResultMapManager().ResultMapSysIParameter as ResultMapSysParameter;

                //if (sysparam != null)
                //{
                //    if (sysparam.UploadComponent.ConverterType.Value == ResultMapConvertType.STIF)
                //    {
                //        retval = this.ResultMapManager().MakeResultMap(IsDummy);

                //        if (retval == EventCodeEnum.NONE)
                //        {
                //            if (retval == EventCodeEnum.NONE && this.ResultMapManager().ConvertedResultMap != null)
                //            {
                //                resmap = this.ResultMapManager().ConvertedResultMap;

                //                STIFResultMap = new ObservableCollection<MapScriptElement>(resmap as List<MapScriptElement>);

                //                if (STIFResultMap != null)
                //                {
                //                    IsSuccess = true;

                //                    SetASCIIMap();
                //                }
                //            }
                //            else
                //            {
                //                LoggerManager.Debug($"[ResultMapAnalyzeVM], STIFGenerateCommandFunc() : Faild to get result map.");
                //            }
                //        }
                //        else
                //        {
                //            LoggerManager.Debug($"[ResultMapAnalyzeVM], STIFGenerateCommandFunc() : Faild to generate result map.");
                //        }
                //    }
                //    else
                //    {
                //        LoggerManager.Debug($"[ResultMapAnalyzeVM], STIFGenerateCommandFunc() : Converter type is not matched.");
                //    }
                //}
                //else
                //{
                //    LoggerManager.Debug($"[ResultMapAnalyzeVM], STIFGenerateCommandFunc() : Parameter is wrong.");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (IsSuccess != true)
                {
                    STIFResultMap = null;
                }
            }
        }

        private RelayCommand<Object> _ChangedSTIFVersionCommand;
        public ICommand ChangedSTIFVersionCommand
        {
            get
            {
                if (null == _ChangedSTIFVersionCommand) _ChangedSTIFVersionCommand = new RelayCommand<object>(ChangedSTIFVersionCommandFunc);
                return _ChangedSTIFVersionCommand;
            }
        }

        private void ChangedSTIFVersionCommandFunc(object obj)
        {
            try
            {
                ResultMapConverterParameter convparam = this.ResultMapManager().GetResultMapConvIParam() as ResultMapConverterParameter;

                if (convparam != null)
                {
                    convparam.STIFParam.Version.Value = MapVersion;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}
