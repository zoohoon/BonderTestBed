using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberDevelopPackWindow.Tab
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using CameraModule;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using ProberVision;
    using RelayCommandBase;
    using VisionParams;
    using VisionParams.Camera;

    public class VisionCameraSettingViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private VisionManager _VisionManager;

        #region Digitizer Tab Property
        private VisionDigiParameters _DigiParameter;

        private ObservableCollection<string> _DigitizerList
             = new ObservableCollection<string>();
        public ObservableCollection<string> DigitizerList
        {
            get { return _DigitizerList; }
            set
            {
                if (value != _DigitizerList)
                {
                    _DigitizerList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SelectedDigitizer;
        public string SelectedDigitizer
        {
            get { return _SelectedDigitizer; }
            set
            {
                if (value != _SelectedDigitizer)
                {
                    _SelectedDigitizer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedDigitizerIndex = -1;
        public int SelectedDigitizerIndex
        {
            get { return _SelectedDigitizerIndex; }
            set
            {
                if (value != _SelectedDigitizerIndex)
                {
                    _SelectedDigitizerIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DigitizerParameter _SelectedDigiParam;
        public DigitizerParameter SelectedDigiParam
        {
            get { return _SelectedDigiParam; }
            set
            {
                if (value != _SelectedDigiParam)
                {
                    _SelectedDigiParam = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion

        #region Camera Tab Property
        private CameraDescriptor _CameraDescriptor;
        private ObservableCollection<EnumProberCam> _ProberCamList
             = new ObservableCollection<EnumProberCam>();
        public ObservableCollection<EnumProberCam> ProberCamList
        {
            get { return _ProberCamList; }
            set
            {
                if (value != _ProberCamList)
                {
                    _ProberCamList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProberCam _SelectedCamera;
        public EnumProberCam SelectedCamera
        {
            get { return _SelectedCamera; }
            set
            {
                if (value != _SelectedCamera)
                {
                    _SelectedCamera = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CameraParameter _SelectedCameraParam;
        public CameraParameter SelectedCameraParam
        {
            get { return _SelectedCameraParam; }
            set
            {
                if (value != _SelectedCameraParam)
                {
                    _SelectedCameraParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IList<FlipEnum> FlipEnums
        {
            get
            {
                // Will result in a list like {"Tester", "Engineer"}
                return Enum.GetValues(typeof(FlipEnum)).Cast<FlipEnum>().ToList<FlipEnum>();
            }
        }

        #endregion

        public VisionCameraSettingViewModel()
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

        public void InitViewModel()
        {
            try
            {
                _VisionManager = this.VisionManager() as VisionManager;
                if (_VisionManager == null)
                    return;
                else
                {
                    _DigiParameter = _VisionManager.Digiparams;
                    if (_DigiParameter.ParamList != null)
                    {
                        foreach (var digiparam in _DigiParameter.ParamList)
                        {
                            DigitizerList.Add(digiparam.DigitizerName.Value);
                        }
                    }

                    _CameraDescriptor = _VisionManager.CamDescriptor;
                    if(_CameraDescriptor.CameraParams != null)
                    {
                        foreach (var camparam in _CameraDescriptor.CameraParams)
                        {
                            ProberCamList.Add(camparam.ChannelType.Value);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region <!-- Digitizer Command -->


        private RelayCommand _SelectedDigitizerChangedCommand;
        public ICommand SelectedDigitizerChangedCommand
        {
            get
            {
                if (null == _SelectedDigitizerChangedCommand) _SelectedDigitizerChangedCommand = new RelayCommand(SelectedDigitizerChangedCommandFunc);
                return _SelectedDigitizerChangedCommand;
            }
        }
        private void SelectedDigitizerChangedCommandFunc()
        {
            try
            {
                var retParam = _DigiParameter.ParamList.SingleOrDefault(param => param.DigitizerName.Value.Equals(SelectedDigitizer));
                if(retParam != null)
                {
                    SelectedDigiParam = retParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private RelayCommand _DigitizerConnectCommand;
        public ICommand DigitizerConnectCommand
        {
            get
            {
                if (null == _DigitizerConnectCommand) _DigitizerConnectCommand = new RelayCommand(DigitizerConnectCommandFunc);
                return _DigitizerConnectCommand;
            }
        }
        private void DigitizerConnectCommandFunc()
        {
            try
            {
                var digiServiceParam = _DigiParameter.ParamList.SingleOrDefault(param => param.DigitizerName.Value.Equals(SelectedDigitizer));
                if(digiServiceParam != null)
                {
                    string RootPath = this.FileManager().FileManagerParam.SystemParamRootDirectory;
                    string FilePath = "\\" + _DigiParameter.FilePath;
                    RootPath = RootPath + FilePath;

                    var retVal =_VisionManager.InitDigitizer(digiServiceParam.GrabRaft.Value, digiServiceParam.DigitizerName.Value, RootPath + digiServiceParam.DCF.Value);
                    if(retVal != ProberErrorCode.EventCodeEnum.NONE)
                    {
                        MessageBox.Show("Fail to InitDigitizer");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _DigitizerDisConnectCommand;
        public ICommand DigitizerDisConnectCommand
        {
            get
            {
                if (null == _DigitizerDisConnectCommand) _DigitizerDisConnectCommand = new RelayCommand(DigitizerConnectCommandFunc);
                return _DigitizerDisConnectCommand;
            }
        }
        private void DigitizerDisConnectCommandFunc()
        {
            try
            {
                var digitizerService = _VisionManager.DigitizerService.SingleOrDefault(service => service.DigitizerName.Equals(SelectedDigitizer));
                if(digitizerService != null)
                {
                    digitizerService.DeInitDigitizer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _LoadDigiParamCommand;
        public ICommand LoadDigiParamCommand
        {
            get
            {
                if (null == _LoadDigiParamCommand) _LoadDigiParamCommand = new RelayCommand(LoadDigiParamCommandFunc);
                return _LoadDigiParamCommand;
            }
        }
        private void LoadDigiParamCommandFunc()
        {
            try
            {
                var ret =  MessageBox.Show("파라미터 파일을 다시 로드하고 Digitizer 와 Camera를 할당합니다.","Warning Message"
                    , MessageBoxButton.OKCancel);
                if(ret == MessageBoxResult.OK)
                {
                    _VisionManager.LoadSysParameter();
                    _VisionManager.LoadDigitizer();
                    _VisionManager.InitCameraModules();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SaveDigiParamCommand;
        public ICommand SaveDigiParamCommand
        {
            get
            {
                if (null == _SaveDigiParamCommand) _SaveDigiParamCommand = new RelayCommand(SaveDigiParamCommandFunc);
                return _SaveDigiParamCommand;
            }
        }
        private void SaveDigiParamCommandFunc()
        {
            try
            {
                this.SaveParameter(_DigiParameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <!-- Camera Command -->

        private bool isExchangingParam = false;
        private RelayCommand _SelectedCameraChangedCommand;
        public ICommand SelectedCameraChangedCommand
        {
            get
            {
                if (null == _SelectedCameraChangedCommand) _SelectedCameraChangedCommand = new RelayCommand(SelectedCameraChangedCommandFunc);
                return _SelectedCameraChangedCommand;
            }
        }
        private void SelectedCameraChangedCommandFunc()
        {
            try
            {
                var retParam = _CameraDescriptor.CameraParams.SingleOrDefault(param => param.ChannelType.Value.Equals(SelectedCamera));
                if (retParam != null)
                {
                    isExchangingParam = true;
                    SelectedCameraParam = retParam;
                    isExchangingParam = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _CameraFilpChangedCommand;
        public ICommand CameraFilpChangedCommand
        {
            get
            {
                if (null == _CameraFilpChangedCommand) _CameraFilpChangedCommand = new RelayCommand(CameraFilpChangedCommandFunc);
                return _CameraFilpChangedCommand;
            }
        }
        private void CameraFilpChangedCommandFunc()
        {
            try
            {
                if (isExchangingParam == true)
                    return;

                ICamera cam = _VisionManager.GetCam(SelectedCamera);
                bool continusgrabflag = false;
                
                if (this.VisionManager().DigitizerService[cam.GetDigitizerIndex()].GrabberService.bContinousGrab)
                {
                    _VisionManager.StopGrab(SelectedCamera);
                    continusgrabflag = true;
                }
                this.VisionManager().SettingGrab(SelectedCamera);

                if (continusgrabflag)
                {
                    _VisionManager.StartGrab(SelectedCamera, this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _StartGrabCommand;
        public ICommand StartGrabCommand
        {
            get
            {
                if (null == _StartGrabCommand) _StartGrabCommand = new RelayCommand(StartGrabCommandFunc);
                return _StartGrabCommand;
            }
        }
        private void StartGrabCommandFunc()
        {
            try
            {
                ICamera cam = _VisionManager.GetCam(SelectedCamera);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _StopGrabCommand;
        public ICommand StopGrabCommand
        {
            get
            {
                if (null == _StopGrabCommand) _StopGrabCommand = new RelayCommand(StopGrabCommandFunc);
                return _StopGrabCommand;
            }
        }
        private void StopGrabCommandFunc()
        {
            try
            {
                ICamera cam = _VisionManager.GetCam(SelectedCamera);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _LoadCamParamCommand;
        public ICommand LoadCamParamCommand
        {
            get
            {
                if (null == _LoadCamParamCommand) _LoadCamParamCommand = new RelayCommand(LoadCamParamCommandFunc);
                return _LoadCamParamCommand;
            }
        }
        private void LoadCamParamCommandFunc()
        {
            try
            { 

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SaveCamParamCommand;
        public ICommand SaveCamParamCommand
        {
            get
            {
                if (null == _SaveCamParamCommand) _SaveCamParamCommand = new RelayCommand(SaveCamParamCommandFunc);
                return _SaveCamParamCommand;
            }
        }
        private void SaveCamParamCommandFunc()
        {
            try
            {
                this.SaveParameter(_CameraDescriptor);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

    }
}
