using LogModule;
using MapConverterModule.E142.Format;
using MaterialDesignExtensions.Controls;
using MetroDialogInterfaces;
using Newtonsoft.Json.Linq;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProberViewModel.ViewModel.ResultMap
{
    public class E142MapAnalyzeVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("15881ff4-1b04-4a6c-8442-563c0d7b0d52");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

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

        private object _ViewerSource;
        public object ViewerSource
        {
            get { return _ViewerSource; }
            set
            {
                if (value != _ViewerSource)
                {
                    _ViewerSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IsDummy = false;

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

        private RelayCommand<Object> _GenerateCommand;
        public ICommand GenerateCommand
        {
            get
            {
                if (null == _GenerateCommand) _GenerateCommand = new RelayCommand<object>(GenerateCommandFunc);
                return _GenerateCommand;
            }
        }

        private void GenerateCommandFunc(object obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            object resmap = null;
            bool IsSuccess = false;

            try
            {
                ResultMapConvertType converterType = this.ResultMapManager().GetUploadConverterType();

                if(converterType == ResultMapConvertType.E142)
                {
                    object rm = null;

                    retval = this.ResultMapManager().MakeResultMap(ref rm, IsDummy);

                    if (retval == EventCodeEnum.NONE)
                    {
                        if (retval == EventCodeEnum.NONE)
                        {
                            resmap = this.ResultMapManager().GetOrgResultMap();

                            JObject jObj = JObject.FromObject(resmap);

                            ViewerSource = jObj.Children().Select(c => JsonHeaderLogic.FromJToken(c));
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
                //    if (sysparam.UploadComponent.ConverterType.Value == ResultMapConvertType.E142)
                //    {
                //        retval = this.ResultMapManager().MakeResultMap(IsDummy);

                //        if (retval == EventCodeEnum.NONE)
                //        {
                //            if (retval == EventCodeEnum.NONE && this.ResultMapManager().ConvertedResultMap != null)
                //            {
                //                resmap = this.ResultMapManager().ConvertedResultMap;

                //                JObject jObj = JObject.FromObject(resmap);

                //                ViewerSource = jObj.Children().Select(c => JsonHeaderLogic.FromJToken(c));
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
                    //STIFResultMap = null;
                }
            }
        }

        private AsyncCommand _ExportMapCommand;
        public ICommand ExportMapCommand
        {
            get
            {
                if (null == _ExportMapCommand) _ExportMapCommand = new AsyncCommand(ExportMapCommandFunc);
                return _ExportMapCommand;
            }
        }
        private EnumMessageDialogResult MessageDialogResult = EnumMessageDialogResult.UNDEFIND;
        private async Task ExportMapCommandFunc()
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                int fileCount = 0;
                bool bExist = true;

                MessageDialogResult = await this.MetroDialogManager().ShowMessageDialog("Do you want to export the STIF Map?", "Click OK to export", EnumMessageStyle.AffirmativeAndNegative);

                if (MessageDialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    string rootdir = @"C:\ProberSystem";
                    string rootpath = Path.Combine(rootdir, "ResultMap", "E142");

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

                            retval = this.ResultMapManager().MakeResultMap(ref rm);
                        }

                        mapfilebase = this.ResultMapManager().GetOrgResultMap();

                        bool IsSuccess = false;

                        if (mapfilebase != null)
                        {
                            retval = this.ResultMapManager().ManualUpload(mapfilebase, FileManagerType.HDD, rootpath, FileReaderType.XML, typeof(MapDataType));

                            if(retval == EventCodeEnum.NONE)
                            {
                                IsSuccess = true;
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


        private AsyncCommand _ImportMapCommand;
        public ICommand ImportMapCommand
        {
            get
            {
                if (null == _ImportMapCommand) _ImportMapCommand = new AsyncCommand(ImportMapCommandFunc);
                return _ImportMapCommand;
            }
        }

        public async Task<EventCodeEnum> ImportMapCommandFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            OpenFileDialogResult result = null;

            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                string filePath = "";
                Stream importFile = null;

                string rootdir = @"C:\ProberSystem";
                string rootpath = Path.Combine(rootdir, "ResultMap", "E142");

                OpenFileDialogArguments dialogArgs = new OpenFileDialogArguments()
                {
                    Width = 900,
                    Height = 700,
                    //CurrentDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory,
                    CurrentDirectory = rootpath,
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
                    filePath = result.FileInfo.FullName;
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    retVal = this.ResultMapManager().ManualConvertResultMapToProberData(filePath, FileManagerType.HDD, FileReaderType.XML, typeof(MapDataType));

                    if (retVal != EventCodeEnum.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "This file is wrong format.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                }

                return retVal;
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Import Failed", "This file is being used by another process", MetroDialogInterfaces.EnumMessageStyle.Affirmative);

                return EventCodeEnum.EXCEPTION;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

    }
}
