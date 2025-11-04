using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LightJog;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AdjustLightDialog
{
    public class AdjustLightVM : IMainScreenViewModel, INotifyPropertyChanged, IUseLightJog
    {
        readonly Guid _ViewModelGUID = new Guid("D032042C-CE89-4179-9168-74FB2BA3E93E");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;
        public AdjustLightVM()
        {
            VisionManager = this.VisionManager();
            foreach (var camera in VisionManager.GetCameras())
            {
                this.CamList.Add(camera);

            }
            LightJogVM = new LightJogViewModel(
                maxLightValue: 255,
                minLightValue: 0);
        }
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region ==> Property
        //public LightJogViewModel LightJogVM { get; set; }
        private LightJogViewModel _LightJogVM;
        public LightJogViewModel LightJogVM
        {
            get { return _LightJogVM; }
            set
            {
                if (_LightJogVM != value)
                {
                    _LightJogVM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IVisionManager VisionManager { get; set; }

        private bool _GridEnabled = true;
        public bool GridEnabled
        {
            get { return _GridEnabled; }
            set
            {
                if (_GridEnabled != value)
                {
                    _GridEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _GrayLevel;
        public int GrayLevel
        {
            get { return _GrayLevel; }
            set
            {
                if (_GrayLevel != value)
                {
                    _GrayLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ICamera> _CamList
                = new ObservableCollection<ICamera>();

        public ObservableCollection<ICamera> CamList
        {
            get { return _CamList; }
            set
            {
                if (value != _CamList)
                {
                    _CamList = value;
                    RaisePropertyChanged(nameof(_CamList));
                }
            }
        }
        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                if (_CurCam != value)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _LightValue;
        public int LightValue
        {
            get { return _LightValue; }
            set
            {
                if (_LightValue != value)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _LightMaxIntensity = 32767;
        public int LightMaxIntensity
        {
            get { return _LightMaxIntensity; }
            set
            {
                if (_LightMaxIntensity != value)
                {
                    _LightMaxIntensity = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _IOValue;
        public int IOValue
        {
            get { return _IOValue; }
            set
            {
                if (_IOValue != value)
                {
                    _IOValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LightChannelType> _LightChannelTypeList
                = new ObservableCollection<LightChannelType>();
        public ObservableCollection<LightChannelType> LightChannelTypeList
        {
            get { return _LightChannelTypeList; }
            set
            {
                if (_LightChannelTypeList != value)
                {
                    _LightChannelTypeList = value;
                    RaisePropertyChanged(nameof(_LightChannelTypeList));
                }
            }
        }
        private ObservableCollection<ILightChannel> _LightModules
        = new ObservableCollection<ILightChannel>();
        public ObservableCollection<ILightChannel> LightModules
        {
            get { return _LightModules; }
            set
            {
                if (_LightModules != value)
                {
                    _LightModules = value;
                    RaisePropertyChanged(nameof(_LightModules));
                }
            }
        }
        private LightChannelType _CurLightType;
        public LightChannelType CurLightType
        {
            get { return _CurLightType; }
            set
            {
                if (_CurLightType != value)
                {
                    _CurLightType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILightChannel _LightChannel;
        public ILightChannel LightChannel
        {
            get { return _LightChannel; }
            set
            {
                if (_LightChannel != value)
                {
                    _LightChannel = value;
                    RaisePropertyChanged();
                }
            }
        }
        

        #endregion

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public void DeInitModule()
        {

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
                    retval = EventCodeEnum.NONE;
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

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
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

        private AsyncCommand _ParameterChangeCommand;
        public IAsyncCommand ParameterChangeCommand
        {
            get
            {
                if (null == _ParameterChangeCommand) _ParameterChangeCommand = new AsyncCommand(ParameterChangeCommandFunc);
                return _ParameterChangeCommand;
            }
        }

        public async Task ParameterChangeCommandFunc()
        {
            try
            {
                if (CurCam != null)
                {
                    GridEnabled = false;
                    EventCodeEnum saveResult = EventCodeEnum.NONE;                    

                    EnumLightType c = EnumLightType.COAXIAL;
                    EnumLightType o = EnumLightType.OBLIQUE;

                    List<int> grayLevelList = new List<int>();
                    int minimum = 0;
                    int index = 1;
                    LightValue = CurCam.GetLight(CurLightType.Type.Value);
                    int preLight = LightValue;
                    CurCam.SetLight(o, 0);


                    for (int i = 0; i < 256; i++)
                    {
                        CurCam.SetLight(c, (ushort)i);

                        await Task.Delay(100);

                        this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                        await Task.Delay(100);

                        ImageBuffer curimg = null;
                        CurCam.GetCurImage(out curimg);
                        curimg.Band = 1;
                        curimg.ColorDept = 8;
                        this.VisionManager().VisionProcessing.GetGrayLevel(ref curimg);

                        grayLevelList.Add(GrayLevel - curimg.GrayLevelValue);

                        if (minimum >= grayLevelList[i])
                        {
                            index = i;
                            break;
                        }
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                    }
                    int channelMapIdx = CurLightType.ChannelMapIdx.Value;
                    int chIndex = CurCam.LightsChannels.FindIndex(light => light.ChannelMapIdx.Value == channelMapIdx);
                    string lutPath = CurCam.LightModules[chIndex].LightLookUpFilePath;
                    IntListParam lut = (IntListParam)CurCam.LightModules[chIndex].LUT;
                    int changeLut = lut[index - 1];

                    IntListParam newLUT = new IntListParam();
                    int lowerInterval = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        if (i == (LightValue - 1))
                        {
                            newLUT.Add(changeLut);
                            lowerInterval = changeLut + Convert.ToInt32(changeLut / (LightValue - 1));
                        }
                        else
                        {
                            if (lowerInterval >= LightMaxIntensity)
                            {
                                lowerInterval -= Convert.ToInt32(changeLut / (LightValue - 1));
                                newLUT.Add(lowerInterval);
                            }
                            else
                            {
                                newLUT.Add(lowerInterval);
                                lowerInterval += Convert.ToInt32(changeLut / (LightValue - 1));
                            }
                        }
                    }
                    saveResult = Extensions_IParam.SaveParameter(null, newLUT, null, lutPath);
                    if (saveResult == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Success to save a new LUT Parameter file { lutPath}");
                    }
                    else
                    {
                        LoggerManager.Debug($"Failed to save { saveResult}");
                    }
                    await LoadLUTCommandFunc();

                    CurCam.SetLight(c, (ushort)preLight);

                    this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);
                    
                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                GridEnabled = true;
            }
        }

        private AsyncCommand _SelectedCameraCommand;
        public ICommand SelectedCameraCommand
        {
            get
            {
                if (null == _SelectedCameraCommand) _SelectedCameraCommand = new AsyncCommand(SelectedCameraCommandFunc);
                return _SelectedCameraCommand;
            }
        }
        private Task SelectedCameraCommandFunc()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LightValue = 0;
                    IOValue = 0;
                    GrayLevel = 0;
                    LightChannelTypeList.Clear();
                    foreach (var lightChannel in CurCam.LightsChannels)
                    {
                        LightChannelTypeList.Add(lightChannel);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }


        private AsyncCommand _SelectedLightTypeCommand;
        public ICommand SelectedLightTypeCommand
        {
            get
            {
                if (null == _SelectedLightTypeCommand) _SelectedLightTypeCommand = new AsyncCommand(SelectedLightTypeCommandFunc);
                return _SelectedLightTypeCommand;
            }
        }
        private async Task SelectedLightTypeCommandFunc()
        {
            try
            {
                if (CurLightType != null)
                {
                    ImageBuffer curimg = null;
                    EnumLightType c = EnumLightType.COAXIAL;

                    LightValue = CurCam.GetLight(CurLightType.Type.Value); // graylevel == LightValue ?????
                    CurCam.SetLight(c, (ushort)LightValue);
                    
                    this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                    await Task.Delay(100);
                    CurCam.GetCurImage(out curimg);
                    curimg.Band = 1;
                    curimg.ColorDept = 8;
                    this.VisionManager().VisionProcessing.GetGrayLevel(ref curimg);
                    GrayLevel = curimg.GrayLevelValue;

                    LightJogVM.InitCameraJog(this, CurCam.GetChannelType());

                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _LoadLUTCommand;
        public ICommand LoadLUTCommand
        {
            get
            {
                if (null == _LoadLUTCommand) _LoadLUTCommand = new AsyncCommand(LoadLUTCommandFunc);
                return _LoadLUTCommand;
            }
        }

        private Task LoadLUTCommandFunc()
        {
            try
            {
                if (CurCam != null)
                {
                    int channelMapIdx = CurLightType.ChannelMapIdx.Value;
                    int chIndex = CurCam.LightsChannels.FindIndex(light => light.ChannelMapIdx.Value == channelMapIdx);
                    CurCam.LightModules[chIndex].LoadLUT();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _DefaultLUTCommand;
        public ICommand DefaultLUTCommand
        {
            get
            {
                if (null == _DefaultLUTCommand) _DefaultLUTCommand = new AsyncCommand(DefaultLUTCommandFunc);
                return _DefaultLUTCommand;
            }
        }

        private async Task DefaultLUTCommandFunc()
        {
            try
            {
                if(CurCam != null) 
                {
                    EventCodeEnum saveResult = EventCodeEnum.NONE;
                    IntListParam devconfig = new IntListParam();

                    int channelMapIdx = CurLightType.ChannelMapIdx.Value;
                    int chIndex = CurCam.LightsChannels.FindIndex(light => light.ChannelMapIdx.Value == channelMapIdx);
                    string lutPath = CurCam.LightModules[chIndex].LightLookUpFilePath;

                    for (int i = 0; i < 256; i++)
                        devconfig.Add(i * (LightMaxIntensity / 255));

                    saveResult = Extensions_IParam.SaveParameter(null, devconfig, null, lutPath);
                    if (saveResult == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Success to save the Default LUT Parameter file { lutPath}");
                    }
                    else
                    {
                        LoggerManager.Debug($"Failed to save { saveResult}");
                    }
                    await LoadLUTCommandFunc();
                    EnumLightType c = EnumLightType.COAXIAL;

                    LightValue = CurCam.GetLight(CurLightType.Type.Value); // graylevel == LightValue ?????
                    CurCam.SetLight(c, (ushort)LightValue);
                    this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);
                    await Task.Delay(100);

                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _RefrashGrayLevelCommand;
        public ICommand RefrashGrayLevelCommand
        {
            get
            {
                if (null == _RefrashGrayLevelCommand) _RefrashGrayLevelCommand = new AsyncCommand(RefrashGrayLevelCommandFunc);
                return _RefrashGrayLevelCommand;
            }
        }
        private async Task RefrashGrayLevelCommandFunc()
        {
            try
            {
                if (CurLightType != null)
                {
                    ImageBuffer curimg = null;
                    EnumLightType c = EnumLightType.COAXIAL;

                    LightValue = CurCam.GetLight(CurLightType.Type.Value);
                    CurCam.SetLight(c, (ushort)LightValue);
                    
                    this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                    await Task.Delay(100);
                    CurCam.GetCurImage(out curimg);
                    curimg.Band = 1;
                    curimg.ColorDept = 8;
                    this.VisionManager().VisionProcessing.GetGrayLevel(ref curimg);
                    GrayLevel = curimg.GrayLevelValue;

                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
