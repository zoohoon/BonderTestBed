using Autofac;
using LoaderBase.LoaderLog;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoaderLogSettingViewModelModule
{
    public class LoaderLogSettingViewModel : INotifyPropertyChanged, IFactoryModule, IDisposable, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public InitPriorityEnum InitPriority { get; set; }

        private Autofac.IContainer _Container;

        public ILoaderLogManagerModule LoaderLogModule;

        public LoaderLogSettingViewModel()
        {
            _Container = this.GetLoaderContainer();
            if (_Container != null)
            {
                LoaderLogModule = this.LoaderLogModule = this.GetLoaderContainer().Resolve<ILoaderLogManagerModule>();
                UploadEnable = LoaderLogModule.LoaderLogParam.UploadEnable.Value;
                CanUseStageLogParam = LoaderLogModule.LoaderLogParam.CanUseStageLogParam.Value;
                UserName = LoaderLogModule.LoaderLogParam.UserName.Value;
                Password = LoaderLogModule.LoaderLogParam.Password.Value;
                AutoLogUploadTimeInterval = LoaderLogModule.LoaderLogParam.AutoLogUploadIntervalMinutes.Value;

                StageSystem = LoaderLogModule.LoaderLogParam.StageSystemUpDownLoadPath.Value;
                StageTemp = LoaderLogModule.LoaderLogParam.StageTempUpDownLoadPath.Value;
                StagePin = LoaderLogModule.LoaderLogParam.StagePinUpDownLoadPath.Value;
                StagePMI = LoaderLogModule.LoaderLogParam.StagePMIUpDownLoadPath.Value;
                StageLOT = LoaderLogModule.LoaderLogParam.StageLOTUpDownLoadPath.Value;

                LoaderSystem = LoaderLogModule.LoaderLogParam.LoaderSystemUpDownLoadPath.Value;
                LoaderOCR = LoaderLogModule.LoaderLogParam.LoaderOCRUpDownLoadPath.Value;

                LoaderUploadDevice = LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value;
                LoaderDownloadDevice = LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value;
                LoaderUploadResultMap = LoaderLogModule.LoaderLogParam.ResultMapUpLoadPath.Value;
                LoaderDownloadResultMap = LoaderLogModule.LoaderLogParam.ResultMapDownLoadPath.Value;
                LoaderUploadODTP = LoaderLogModule.LoaderLogParam.ODTPUpLoadPath.Value;
                LoaderUploadResultMapRetryCount = LoaderLogModule.LoaderLogParam.ResultMapUploadRetryCount.Value;
                SpoolingBasePath = LoaderLogModule.LoaderLogParam.SpoolingBasePath.Value;
                LoaderUploadResultMapRetryDelayTime = LoaderLogModule.LoaderLogParam.ResultMapUploadDelayTime.Value;
                LoaderUploadPMIImage = LoaderLogModule.LoaderLogParam.StagePMIImageUploadPath.Value;
                StagePinTipValidationResultImage = LoaderLogModule.LoaderLogParam.StagePinTipValidationResultUploadPath.Value;

                StageLogParamList = new List<StageLogParameter>();

                for (int i = 0; i < LoaderLogModule.LoaderLogParam.StageLogParams.Count; i++)
                {
                    StageLogParamList.Add(LoaderLogModule.LoaderLogParam.StageLogParams[i]);
                }
            }
            
        }
        private bool _UploadEnable;
        public bool UploadEnable
        {
            get { return _UploadEnable; }
            set
            {
                if (value != _UploadEnable)
                {
                    _UploadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CanUseStageLogParam;
        public bool CanUseStageLogParam
        {
            get { return _CanUseStageLogParam; }
            set
            {
                if (value != _CanUseStageLogParam)
                {
                    _CanUseStageLogParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (value != _UserName)
                {
                    _UserName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _AutoLogUploadTimeInterval;
        public int AutoLogUploadTimeInterval
        {
            get { return _AutoLogUploadTimeInterval; }
            set
            {
                if (value != _AutoLogUploadTimeInterval)
                {
                    _AutoLogUploadTimeInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                if (value != _Password)
                {
                    _Password = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StageSystem;
        public string StageSystem
        {
            get { return _StageSystem; }
            set
            {
                if (value != _StageSystem)
                {
                    _StageSystem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StagePin;
        public string StagePin
        {
            get { return _StagePin; }
            set
            {
                if (value != _StagePin)
                {
                    _StagePin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StagePMI;
        public string StagePMI
        {
            get { return _StagePMI; }
            set
            {
                if (value != _StagePMI)
                {
                    _StagePMI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StageTemp;
        public string StageTemp
        {
            get { return _StageTemp; }
            set
            {
                if (value != _StageTemp)
                {
                    _StageTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StageLOT;
        public string StageLOT
        {
            get { return _StageLOT; }
            set
            {
                if (value != _StageLOT)
                {
                    _StageLOT = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _LoaderUploadDevice;
        public string LoaderUploadDevice
        {
            get { return _LoaderUploadDevice; }
            set
            {
                if (value != _LoaderUploadDevice)
                {
                    _LoaderUploadDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LoaderDownloadDevice;
        public string LoaderDownloadDevice
        {
            get { return _LoaderDownloadDevice; }
            set
            {
                if (value != _LoaderDownloadDevice)
                {
                    _LoaderDownloadDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LoaderDownloadResultMap;
        public string LoaderDownloadResultMap
        {
            get { return _LoaderDownloadResultMap; }
            set
            {
                if (value != _LoaderDownloadResultMap)
                {
                    _LoaderDownloadResultMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LoaderUploadResultMap;
        public string LoaderUploadResultMap
        {
            get { return _LoaderUploadResultMap; }
            set
            {
                if (value != _LoaderUploadResultMap)
                {
                    _LoaderUploadResultMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LoaderUploadODTP;
        public string LoaderUploadODTP
        {
            get { return _LoaderUploadODTP; }
            set
            {
                if (value != _LoaderUploadODTP)
                {
                    _LoaderUploadODTP = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private int _LoaderUploadResultMapRetryCount;
        public int LoaderUploadResultMapRetryCount
        {
            get { return _LoaderUploadResultMapRetryCount; }
            set
            {
                if (value != _LoaderUploadResultMapRetryCount)
                {
                    _LoaderUploadResultMapRetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _LoaderUploadResultMapRetryDelayTime;
        public int LoaderUploadResultMapRetryDelayTime
        {
            get { return _LoaderUploadResultMapRetryDelayTime; }
            set
            {
                if (value != _LoaderUploadResultMapRetryDelayTime)
                {
                    if (value < LoaderLogModule.LoaderLogParam.minSpoolingDelayTimeSec)
                        value = LoaderLogModule.LoaderLogParam.defaultSpoolingDelayTimeSec;

                    _LoaderUploadResultMapRetryDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SpoolingBasePath;
        public string SpoolingBasePath
        {
            get { return _SpoolingBasePath; }
            set
            {
                if (value != _SpoolingBasePath)
                {
                    _SpoolingBasePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LoaderOCR;
        public string LoaderOCR
        {
            get { return _LoaderOCR; }
            set
            {
                if (value != _LoaderOCR)
                {
                    _LoaderOCR = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LoaderSystem;
        public string LoaderSystem
        {
            get { return _LoaderSystem; }
            set
            {
                if (value != _LoaderSystem)
                {
                    _LoaderSystem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LoaderUploadPMIImage;
        public string LoaderUploadPMIImage
        {
            get { return _LoaderUploadPMIImage; }
            set
            {
                if (value != _LoaderUploadPMIImage)
                {
                    _LoaderUploadPMIImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StagePinTipValidationResultImage;
        public string StagePinTipValidationResultImage
        {
            get { return _StagePinTipValidationResultImage; }
            set
            {
                if (value != _StagePinTipValidationResultImage)
                {
                    _StagePinTipValidationResultImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumSaveImageFormat _SaveImageFormat;
        public EnumSaveImageFormat SaveImageFormat
        {
            get { return _SaveImageFormat; }
            set
            {
                if (value != _SaveImageFormat)
                {
                    _SaveImageFormat = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<StageLogParameter> _StageLogParamList;
        public List<StageLogParameter> StageLogParamList
        {
            get { return _StageLogParamList; }
            set
            {
                if (value != _StageLogParamList)
                {
                    _StageLogParamList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedType;
        public int SelectedType
        {
            get { return _SelectedType; }
            set
            {
                if (value != _SelectedType)
                {
                    _SelectedType = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region //..Command & Method

        //public void SettingData(PinAlignDevParameters pin_DevParam)
        //{
        //    try
        //    {
        //        this.pinParam = pin_DevParam;
        //        ThreshTypeEnum = pin_DevParam?.EnableAutoThreshold.Value ?? EnumThresholdType.AUTO;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        private AsyncCommand<object> _OKCommand;
        public ICommand OKCommand
        {
            get
            {
                if (null == _OKCommand) _OKCommand
                        = new AsyncCommand<object>(OkComandFunc);
                return _OKCommand;
            }
        }

        private async Task OkComandFunc(object obj)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((() =>
                {
                    _Container = this.GetLoaderContainer();
                    if (_Container != null)
                    {
                        LoaderLogModule = this.LoaderLogModule = this.GetLoaderContainer().Resolve<ILoaderLogManagerModule>();
                        LoaderLogModule.LoaderLogParam.UploadEnable.Value = UploadEnable;
                        LoaderLogModule.LoaderLogParam.CanUseStageLogParam.Value = CanUseStageLogParam;
                        LoaderLogModule.LoaderLogParam.UserName.Value = UserName;
                        LoaderLogModule.LoaderLogParam.Password.Value = Password;

                        LoaderLogModule.LoaderLogParam.StageSystemUpDownLoadPath.Value = StageSystem;
                        LoaderLogModule.LoaderLogParam.StageTempUpDownLoadPath.Value = StageTemp;
                        LoaderLogModule.LoaderLogParam.StagePinUpDownLoadPath.Value = StagePin;
                        LoaderLogModule.LoaderLogParam.StagePMIUpDownLoadPath.Value = StagePMI;
                        LoaderLogModule.LoaderLogParam.StageLOTUpDownLoadPath.Value = StageLOT;

                        LoaderLogModule.LoaderLogParam.LoaderSystemUpDownLoadPath.Value = LoaderSystem;
                        LoaderLogModule.LoaderLogParam.LoaderOCRUpDownLoadPath.Value = LoaderOCR;
                        LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value = LoaderUploadDevice;
                        LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value = LoaderDownloadDevice;
                        LoaderLogModule.LoaderLogParam.ResultMapUpLoadPath.Value = LoaderUploadResultMap;
                        LoaderLogModule.LoaderLogParam.ResultMapDownLoadPath.Value = LoaderDownloadResultMap;
                        LoaderLogModule.LoaderLogParam.ODTPUpLoadPath.Value = LoaderUploadODTP;

                        LoaderLogModule.LoaderLogParam.ResultMapUploadRetryCount.Value = LoaderUploadResultMapRetryCount;
                        LoaderLogModule.LoaderLogParam.ResultMapUploadDelayTime.Value = LoaderUploadResultMapRetryDelayTime;
                        LoaderLogModule.LoaderLogParam.StagePMIImageUploadPath.Value = LoaderUploadPMIImage;
                        LoaderLogModule.LoaderLogParam.StagePinTipValidationResultUploadPath.Value = StagePinTipValidationResultImage;

                        for (int i = 0; i < StageLogParamList.Count; i++)
                        {
                            LoaderLogModule.LoaderLogParam.StageLogParams[i].StageSystemUpDownLoadPath.Value = StageLogParamList[i].StageSystemUpDownLoadPath.Value;
                            LoaderLogModule.LoaderLogParam.StageLogParams[i].StageTempUpDownLoadPath.Value = StageLogParamList[i].StageTempUpDownLoadPath.Value;
                            LoaderLogModule.LoaderLogParam.StageLogParams[i].StagePinUpDownLoadPath.Value = StageLogParamList[i].StagePinUpDownLoadPath.Value;
                            LoaderLogModule.LoaderLogParam.StageLogParams[i].StagePMIUpDownLoadPath.Value = StageLogParamList[i].StagePMIUpDownLoadPath.Value;
                            LoaderLogModule.LoaderLogParam.StageLogParams[i].StageLOTUpDownLoadPath.Value = StageLogParamList[i].StageLOTUpDownLoadPath.Value;
                            LoaderLogModule.LoaderLogParam.StageLogParams[i].StagePinTipValidationResultUploadPath.Value = StageLogParamList[i].StagePinTipValidationResultUploadPath.Value;
                        }

                        LoaderLogModule.LoaderLogParam.AutoLogUploadIntervalMinutes.Value = AutoLogUploadTimeInterval;
                        LoaderLogModule.SetIntervalForLogUpload(LoaderLogModule.LoaderLogParam.AutoLogUploadIntervalMinutes.Value);

                        LoaderLogModule.SaveParameter();
                    }
                    Window window = obj as Window;
                    if (window != null)
                    {
                        window.Close();
                    }
                }));

                //this.pinParam.EnableAutoThreshold.Value = ThreshTypeEnum;
                //await this.PnPManager().ClosePnpAdavanceSetupWindow().ConfigureAwait(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<object> _CancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (null == _CancelCommand) _CancelCommand
                        = new AsyncCommand<object>(CancleClickFunc);
                return _CancelCommand;
            }
        }


        private async Task CancleClickFunc(object obj)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Window window = obj as Window;
                    if (window != null)
                    {
                        window.Close();
                    }
                }));

                //await this.PnPManager().ClosePnpAdavanceSetupWindow().ConfigureAwait(false);
                //ThreshTypeEnum = this.pinParam?.EnableAutoThreshold.Value ?? EnumThresholdType.AUTO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _UpLoadEnableCommand;
        public ICommand UpLoadEnableCommand
        {
            get
            {
                if (null == _UpLoadEnableCommand) _UpLoadEnableCommand = new RelayCommand<object>(UpLoadEnableCommandFunc);
                return _UpLoadEnableCommand;
            }
        }
        private void UpLoadEnableCommandFunc(object noparam)
        {
            try
            {
                UploadEnable = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _UpLoadDisableCommand;
        public ICommand UpLoadDisableCommand
        {
            get
            {
                if (null == _UpLoadDisableCommand) _UpLoadDisableCommand = new RelayCommand<object>(UpLoadDisableCommandFunc);
                return _UpLoadDisableCommand;
            }
        }
        private void UpLoadDisableCommandFunc(object noparam)
        {
            try
            {
                UploadEnable = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _CanUseStageLogParamEnable;
        public ICommand CanUseStageLogParamEnable
        {
            get
            {
                if (null == _CanUseStageLogParamEnable) _CanUseStageLogParamEnable = new RelayCommand<object>(CanUseStageLogParamEnableFunc);
                return _CanUseStageLogParamEnable;
            }
        }
        private void CanUseStageLogParamEnableFunc(object noparam)
        {
            try
            {
                CanUseStageLogParam = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _CanUseStageLogParamDisable;
        public ICommand CanUseStageLogParamDisable
        {
            get
            {
                if (null == _CanUseStageLogParamDisable) _CanUseStageLogParamDisable = new RelayCommand<object>(CanUseStageLogParamDisableFunc);
                return _CanUseStageLogParamDisable;
            }
        }
        private void CanUseStageLogParamDisableFunc(object noparam)
        {
            try
            {
                CanUseStageLogParam = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void Dispose()
        {
            try
            {

            }
            catch (Exception err)
            {

            }
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
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

        #endregion



    }
}
