using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JogViewModelModule
{
    using Autofac;
    using LightManager;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.PnpSetup;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Windows.Media;

    public class LoaderLightJogViewModel : INotifyPropertyChanged , ILightJobViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        private Autofac.IContainer _Container;
        private IVisionManagerProxy VisionManager
        {
            get
            {
                return _Container.Resolve<IVisionManagerProxy>();
            }
        }
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return _Container.Resolve<ILoaderCommunicationManager>();
            }
        }


        private IMetroDialogManager MetroDialogManager
        {
            get
            {
                return _Container.Resolve<IMetroDialogManager>();
            }
        }

        public IPnpManager PnpManage => _Container.Resolve<IPnpManager>();

        public bool IsUseNC { get; set; } = false;

        List<LightValueParam> _LightParam = new List<LightValueParam>();

        delegate void CamBtnSettingDelegate();
        private readonly int _MaxLightValue;
        private readonly int _MinLightValue;
        private const int _DefaultRegulateValue = 5;//==> Camera 조명값 변화량

        //==> Button Brush
        private readonly Brush _BtnNolmalBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x77, 0x77, 0x77));
        private readonly Brush _BtnHighlightBrush1 = new SolidColorBrush(Colors.Orange);
        private readonly Brush _BtnHighlightBrush2 = new SolidColorBrush(Colors.Red);

        //==> Button Text
        private const String _BtnNolmalHighText = "High";
        private const String _BtnNolmalLowText = "Low";

        private EnumProberCam _HighCamType { get; set; }
        private EnumProberCam _LowCamType { get; set; }


        public CameraBtnType CurSelectedMag { get; set; }
        //==> Camera Button 이 눌렸을 때 수행 해야할 Action들을 저장
        private Dictionary<CameraBtnType, CamBtnSettingDelegate> _CamBtnUiActionHandlerDic;

        public ICamera AssignedCamera { get; set; }
        public EnumProberCam AssignedCameraType { get; set; }
        //private List<EnumLightType> AssignedCameraLightTypes
        //{
        //    get
        //    {
        //        return AssignedCamera.LightsChannels.Select(light => light.Type.Value).ToList();
        //    }

        //}

        private List<EnumLightType> _AssignedCameraLightTypes;
        public List<EnumLightType> AssignedCameraLightTypes
        {
            get { return _AssignedCameraLightTypes; }
            set
            {
                if (value != _AssignedCameraLightTypes)
                {
                    _AssignedCameraLightTypes = value;
                    RaisePropertyChanged();
                }
            }
        }



        private ILightChannel _CurLightChannel;
        public ILightChannel CurLightChannel
        {
            get { return _CurLightChannel; }
            set
            {
                if (value != _CurLightChannel)
                {
                    _CurLightChannel = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> 현재 선택된 카메라 조명 인덱스
        #region ==> CurCameraLigntTypeIndex
        private int _CurCameraLigntTypeIndex;
        private int CurCameraLigntTypeIndex
        {
            get { return _CurCameraLigntTypeIndex; }
            set
            {
                try
                {
                    if (value == _CurCameraLigntTypeIndex)
                        return;
                    else if (value < 0)//==> Under Limit Check
                        _CurCameraLigntTypeIndex = 0;
                    else if (value >= AssignedCameraLightTypes.Count)//==> Upper Limit Check
                        _CurCameraLigntTypeIndex = AssignedCameraLightTypes.Count - 1;
                    else
                        _CurCameraLigntTypeIndex = value;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
        }
        #endregion

        //==> 현재 선택된 조명 타입
        #region ==> CurCameraLightType
        private EnumLightType _CurCameraLightType;
        public EnumLightType CurCameraLightType
        {
            get
            {
                return _CurCameraLightType;
            }
        }
        #endregion

        //==> 현재 Camera 조명의 밝기
        #region ==> CurCameraLightValue 
        private int _CurCameraLightValue;
        public int CurCameraLightValue
        {
            //get { return AssignedCamera.GetLight(CurCameraLightType); }
            get { return _CurCameraLightValue; }
            set
            {
                if (value == _CurCameraLightValue)
                    RaisePropertyChanged();
                else if (value < _MinLightValue)
                    _CurCameraLightValue = _MinLightValue;
                else if (value > _MaxLightValue)
                    _CurCameraLightValue = _MaxLightValue;
                else
                    _CurCameraLightValue = value;
                ((LightChannel)CurLightChannel).CurLightValue = _CurCameraLightValue;
            }
        }
        #endregion

        //==> Hight Camera 변경 버튼 색깔과 바인딩
        #region ==> HighBtnBrush
        private Brush _HighBtnBrush;
        public Brush HighBtnBrush
        {
            get { return _HighBtnBrush; }
            set
            {
                if (value != _HighBtnBrush)
                {
                    _HighBtnBrush = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        //==> Low Camera 변경 버튼 색깔과 바인딩
        #region ==> LowBtnBrush
        private Brush _LowBtnBrush;
        public Brush LowBtnBrush
        {
            get { return _LowBtnBrush; }
            set
            {
                if (value != _LowBtnBrush)
                {
                    _LowBtnBrush = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Hight Camera 변경 버튼 색깔과 바인딩
        //==> Hight Camera 변경 버튼 색깔과 바인딩
        #endregion

        //==> High Camera 변경 버튼 Text와 바인딩
        #region ==> HighBtnText
        private String _HighBtnText;
        public String HighBtnText
        {
            get { return _HighBtnText; }
            set
            {
                if (value != _HighBtnText)
                {
                    _HighBtnText = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        //==> Low Camera 변경 버튼 Text와 바인딩
        #region ==> LowBtnText
        private String _LowBtnText;
        public String LowBtnText
        {
            get { return _LowBtnText; }
            set
            {
                if (value != _LowBtnText)
                {
                    _LowBtnText = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        private IRemoteMediumProxy CurStageObj => LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();


        #endregion

        #region //..Init

        public LoaderLightJogViewModel(
            int maxLightValue,
            int minLightValue)
        {

            _CamBtnUiActionHandlerDic = new Dictionary<CameraBtnType, CamBtnSettingDelegate>();
            _CamBtnUiActionHandlerDic.Add(CameraBtnType.High,
            () =>
            {
                if (CheckSWLimit(_HighCamType) == false)
                {
                    if (AssignedCameraType != EnumProberCam.WAFER_HIGH_CAM & AssignedCameraType != EnumProberCam.PIN_HIGH_CAM)
                    {
                        //==> High Btn UI & Command Setting
                        HighBtnText = _BtnNolmalHighText;//==> Set Btn Text
                        HighBtnBrush = _BtnHighlightBrush1;//==> Set Btn Brush

                        //==> Low Btn UI & Command Setting
                        LowBtnText = _BtnNolmalLowText;//==> Set Btn Text
                        LowBtnBrush = _BtnNolmalBrush;//==> Set Btn Brush
                        _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];//==> Set Btn Action(Low Action 수행)

    
                        //==> Set Cur Camera Type
                        AssignedCameraType = _HighCamType;

                        //==> Update High and Low Cam
                        UpdateCamera(AssignedCameraType);

                        //==> Change Cur Cam
                        //UserLightJogModule.CurCam = AssignedCamera;
                        //CurStageObj.ChangeCamPosition(AssignedCameraType);
                        //==> Change Cam Position
                        ChangeCamPosition(AssignedCameraType);

                        //==> Set Cur Camera Light Index
                        _CurCameraLigntTypeIndex = 0;
                        ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
                    }
                }
                else
                {
                    const string message = "Operation Failure : S/W Limit";
                    const string caption = "Warning";

                    MetroDialogManager.ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);
                }
            });
            _CamBtnUiActionHandlerDic.Add(CameraBtnType.Low,
            () =>
            {
                if (CheckSWLimit(_LowCamType) == false)
                {
                    //if (AssignedCamera.GetChannelType() != _LowCam.GetChannelType())
                    if (AssignedCameraType != EnumProberCam.WAFER_LOW_CAM & AssignedCameraType != EnumProberCam.PIN_LOW_CAM)
                    {
                        LowBtnText = _BtnNolmalLowText;
                        LowBtnBrush = _BtnHighlightBrush1;

                        HighBtnText = _BtnNolmalHighText;
                        HighBtnBrush = _BtnNolmalBrush;
                        _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];


                        //AssignedCamera = _LowCam;
                        //UserLightJogModule.CurCam = AssignedCamera;
                        AssignedCameraType = _LowCamType;

                        UpdateCamera(AssignedCameraType);

                        ChangeCamPosition(AssignedCameraType);

                        _CurCameraLigntTypeIndex = 0;
                        ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
                    }
                }
                else
                {
                    const string message = "Operation Failure : S/W Limit";
                    const string caption = "Warning";

                    MetroDialogManager.ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);
                }
            });

            //==> Init Max, Min, Init Light Value
            _MaxLightValue = maxLightValue;
            _MinLightValue = minLightValue;
        }

        /// <summary>
        /// Stage쪽에 어떤 viewmodel에 업데이트 해줘야 할지 판단하지 위해.
        /// </summary>
        private string _UsingInterfaceType = null;


        public LoaderLightJogViewModel(
            int maxLightValue,
            int minLightValue, string interfaceType)
        {
            _UsingInterfaceType = interfaceType;
            _CamBtnUiActionHandlerDic = new Dictionary<CameraBtnType, CamBtnSettingDelegate>();
            _CamBtnUiActionHandlerDic.Add(CameraBtnType.High,
            () =>
            {
                if (CheckSWLimit(_HighCamType) == false)
                {
                    if (AssignedCameraType != EnumProberCam.WAFER_HIGH_CAM & AssignedCameraType != EnumProberCam.PIN_HIGH_CAM)
                    {
                        //==> High Btn UI & Command Setting
                        HighBtnText = _BtnNolmalHighText;//==> Set Btn Text
                        HighBtnBrush = _BtnHighlightBrush1;//==> Set Btn Brush

                        //==> Low Btn UI & Command Setting
                        LowBtnText = _BtnNolmalLowText;//==> Set Btn Text
                        LowBtnBrush = _BtnNolmalBrush;//==> Set Btn Brush
                        _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];//==> Set Btn Action(Low Action 수행)

                        //==> Update High and Low Cam
                        UpdateCamera();
                        //==> Set Cur Camera Type
                        AssignedCameraType = _HighCamType;
                        //==> Change Cur Cam
                        //UserLightJogModule.CurCam = AssignedCamera;
                        //CurStageObj.ChangeCamPosition(AssignedCameraType);
                        //==> Change Cam Position
                        ChangeCamPosition(AssignedCameraType);

                        //==> Set Cur Camera Light Index
                        _CurCameraLigntTypeIndex = 0;
                        ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
                    }
                }
                else
                {
                    const string message = "Operation Failure : S/W Limit";
                    const string caption = "Warning";

                    MetroDialogManager.ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);
                }
            });
            _CamBtnUiActionHandlerDic.Add(CameraBtnType.Low,
            () =>
            {
                if (CheckSWLimit(_LowCamType) == false)
                {
                    //if (AssignedCamera.GetChannelType() != _LowCam.GetChannelType())
                    if (AssignedCameraType != EnumProberCam.WAFER_LOW_CAM & AssignedCameraType != EnumProberCam.PIN_LOW_CAM)
                    {
                        LowBtnText = _BtnNolmalLowText;
                        LowBtnBrush = _BtnHighlightBrush1;

                        HighBtnText = _BtnNolmalHighText;
                        HighBtnBrush = _BtnNolmalBrush;
                        _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

                        UpdateCamera();
                        //AssignedCamera = _LowCam;
                        //UserLightJogModule.CurCam = AssignedCamera;
                        AssignedCameraType = _LowCamType;
                        ChangeCamPosition(AssignedCameraType);

                        _CurCameraLigntTypeIndex = 0;
                        ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
                    }
                }
                else
                {
                    const string message = "Operation Failure : S/W Limit";
                    const string caption = "Warning";

                    MetroDialogManager.ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);
                }
            });

            //==> Init Max, Min, Init Light Value
            _MaxLightValue = maxLightValue;
            _MinLightValue = minLightValue;
        }

        public void SetContainer(Autofac.IContainer container)
        {
            _Container = container;
        }
        private bool CheckSWLimit(EnumProberCam cam)
        {
            bool retVal = false;

            try
            {
                retVal = CurStageObj.CheckSWLimit(cam);
                return false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Exception error in CheckSWLimit()");
                LoggerManager.Exception(err);
                return true;
            }
        }

        #endregion 

        #region //..Command & Command Method

        public bool UpdateCamera(EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            if (camtype != EnumProberCam.UNDEFINED)
            {
                AssignedCameraType = camtype;
                //AssignedCamera = VisionManager.GetCam(camtype);
            }
            else
            {
                return false;
                //AssignedCamera = UserLightJogModule.CurCam;
            }

            //if (AssignedCamera == null)
            //    return false;

            //==> Set High Camera, Low Camera
            //switch (AssignedCamera.CameraChannel.Type)
            switch (camtype)
            {
                case EnumProberCam.WAFER_HIGH_CAM:
                    _HighCamType = EnumProberCam.WAFER_HIGH_CAM;
                    _LowCamType = EnumProberCam.WAFER_LOW_CAM;
                    CurSelectedMag = CameraBtnType.High;
                    break;
                case EnumProberCam.PIN_HIGH_CAM:
                    _HighCamType = EnumProberCam.PIN_HIGH_CAM;
                    _LowCamType = EnumProberCam.PIN_LOW_CAM;
                    CurSelectedMag = CameraBtnType.High;
                    break;

                case EnumProberCam.WAFER_LOW_CAM:
                    _HighCamType = EnumProberCam.WAFER_HIGH_CAM;
                    _LowCamType = EnumProberCam.WAFER_LOW_CAM;
                    CurSelectedMag = CameraBtnType.Low;
                    break;
                case EnumProberCam.PIN_LOW_CAM:
                    _HighCamType = EnumProberCam.PIN_HIGH_CAM;
                    _LowCamType = EnumProberCam.PIN_LOW_CAM;
                    CurSelectedMag = CameraBtnType.Low;
                    break;
                default:
                    CurSelectedMag = CameraBtnType.UNDEFINED;
                    break;
            }
            return true;
        }


        public void InitCameraJog(IUseLightJog module, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {

            try
            {
                //if (module == null)
                //   return;
                //else
                //    UserLightJogModule = module;
                //if (_Prober.ProberViewModel == null)
                //if (this.ViewModelManager() == null)
                //    return;

                if (UpdateCamera(camtype) == false)
                    return;

                CurLightChannel = new LightChannel();
                //if (AssignedCameraLightTypes.Count < 1)
                //    return;

                //==> Init Light Type Index
                CurCameraLigntTypeIndex = 0;

                //==> Init Cur Camera Light Value
                //CurCameraLightValue = AssignedCamera.GetLight(_CurCameraLightType);


                // Init Light Channel
                AssignedCameraLightTypes = GetLightTypes();
                //==> Init Light Type UI [Binding]
                //_CurCameraLightType = AssignedCameraLightTypes[CurCameraLigntTypeIndex];
                if(AssignedCameraLightTypes != null)
                    _CurCameraLightType = AssignedCameraLightTypes[0];
                CurLightTypeStr = CurCameraLightType.ToString();

                //==> Init Light Value UI [Binding]

                //CurLightValueStr = CurCameraLightValue.ToString();




                //==> Init Camera Btn UI
                if (camtype == EnumProberCam.WAFER_HIGH_CAM | camtype == EnumProberCam.PIN_HIGH_CAM)
                {
                    //==> High Btn UI & Command Setting
                    HighBtnText = _BtnNolmalHighText;//==> Set Btn Text
                    HighBtnBrush = _BtnHighlightBrush1;//==> Set Btn Brush
                    //_HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High2X];//==> Set Btn Action(Hihg2X Action 수행)

                    //==> Low Btn UI & Command Setting
                    LowBtnText = _BtnNolmalLowText;//==> Set Btn Text
                    LowBtnBrush = _BtnNolmalBrush;//==> Set Btn Brush
                    _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];//==> Set Btn Action(Low Action 수행)
                }
                else
                {
                    LowBtnText = _BtnNolmalLowText;
                    LowBtnBrush = _BtnHighlightBrush1;
                    //_LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low2X];

                    HighBtnText = _BtnNolmalHighText;
                    HighBtnBrush = _BtnNolmalBrush;
                    _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];
                }
                IsUseNC = false;

                _LightParam = new List<LightValueParam>();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw err;
            }
        }

        public void UpdateCameraLightValue()
        {
            try
            {
                if (AssignedCamera == null)
                    return;

                _CurCameraLightType = AssignedCameraLightTypes[CurCameraLigntTypeIndex];
                CurLightTypeStr = CurCameraLightType.ToString();
                CurCameraLightValue = AssignedCamera.GetLight(CurCameraLightType);
                SetCurCameraLightValue(CurCameraLightValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
        }

        //==> High Camera 변경 버튼과 바인딩
        #region ==> HighBtnCommand
        CamBtnSettingDelegate _HighBtnHandler;
        private AsyncCommand<object> _HighBtnCommand;
        public ICommand HighBtnCommand
        {
            get
            {
                if (null == _HighBtnCommand) _HighBtnCommand = new AsyncCommand<object>(HighBtnFunc);
                return _HighBtnCommand;
            }
        }
        private Task HighBtnFunc(object parameter)
        {
            _HighBtnHandler.Invoke();
            HighBtnEventHandler?.Execute(null);
            return Task.CompletedTask;
        }
        public ICommand HighBtnEventHandler { get; set; }
        #endregion

        //==> Low Camera 변경 버튼과 바인딩
        #region ==> LowBtnCommand
        CamBtnSettingDelegate _LowBtnHandler;
        private AsyncCommand<object> _LowBtnCommand;
        public ICommand LowBtnCommand
        {
            get
            {
                if (null == _LowBtnCommand) _LowBtnCommand = new AsyncCommand<object>(LowBtnFunc);
                return _LowBtnCommand;
            }
        }
        private Task LowBtnFunc(object parameter)
        {
            _LowBtnHandler.Invoke();
            LowBtnEventHandler?.Execute(null);
            return Task.CompletedTask;
        }
        public ICommand LowBtnEventHandler { get; set; }
        #endregion

        //==> 이전 조명 타입으로 변경하는 버튼과 바인딩
        #region ==> LightTypeUpCommand
        private RelayCommand<object> _LightTypeUpCommand;
        public ICommand LightTypeUpCommand
        {
            get
            {
                if (null == _LightTypeUpCommand) _LightTypeUpCommand = new RelayCommand<object>(LightTypeUpFunc);
                return _LightTypeUpCommand;
            }
        }
        private void LightTypeUpFunc(object parameter)
        {
            if (CurCameraLigntTypeIndex != 0)
                ChangeCurCameraLightType(CurCameraLigntTypeIndex - 1);
            else
                ChangeCurCameraLightType(AssignedCameraLightTypes.Count);
        }
        #endregion

        //==> 다음 조명 타입으로 변경하는 버튼과 바인딩
        #region ==> LightTypeDownCommand
        private RelayCommand<object> _LightTypeDownCommand;
        public ICommand LightTypeDownCommand
        {
            get
            {
                if (null == _LightTypeDownCommand) _LightTypeDownCommand = new RelayCommand<object>(LightTypeDownFunc);
                return _LightTypeDownCommand;
            }
        }
        private void LightTypeDownFunc(object parameter)
        {
            try
            {
                if (CurCameraLigntTypeIndex + 1 != AssignedCameraLightTypes.Count)
                    ChangeCurCameraLightType(CurCameraLigntTypeIndex + 1);
                else
                    ChangeCurCameraLightType(0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void ChangeCurCameraLightType(int lightTypeIndex)
        {
            try
            {

                if (CurCameraLightType == EnumLightType.UNDEFINED)
                    return;
                CurCameraLigntTypeIndex = lightTypeIndex;
                AssignedCameraLightTypes = GetLightTypes();
                _CurCameraLightType = AssignedCameraLightTypes[CurCameraLigntTypeIndex];
                CurLightTypeStr = CurCameraLightType.ToString();
                //CurCameraLightValue = AssignedCamera.GetLight(CurCameraLightType);
                SetLightChannel();
                GetLightValue();
                SetCurCameraLightValue(CurCameraLightValue);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
        }

        private void SetCurCameraLightValue(int intensity)
        {
            if (AssignedCameraType == EnumProberCam.UNDEFINED)
                return;
            if (intensity > 255) intensity = 255;
            else if (intensity < 0) intensity = 0;
            //AssignedCamera.SetLight(CurCameraLightType, (ushort)intensity);
            //CurLightValueStr = CurCameraLightValue.ToString();
            SetLightValue(intensity);
            GetLightValue().ToString();
        }

        #endregion

        //==> 조명 값을 올리는 버튼과 바인딩
        #region ==> LightValueUpCommand
        private RelayCommand<object> _LightValueUpCommand;
        public ICommand LightValueUpCommand
        {
            get
            {
                if (null == _LightValueUpCommand) _LightValueUpCommand = new RelayCommand<object>(LightValueUpFunc);
                return _LightValueUpCommand;
            }
        }
        private void LightValueUpFunc(object parameter)
        {
            SetCurCameraLightValue(CurCameraLightValue + _DefaultRegulateValue);

        }
        #endregion

        //==> 조명 값을 내리는 버튼과 바인딩
        #region ==> LightValueDownCommand
        private RelayCommand<object> _LightValueDownCommand;
        public ICommand LightValueDownCommand
        {
            get
            {
                if (null == _LightValueDownCommand) _LightValueDownCommand = new RelayCommand<object>(LightValueDownFunc);
                return _LightValueDownCommand;
            }
        }
        private void LightValueDownFunc(object parameter)
        {
            SetCurCameraLightValue(CurCameraLightValue - _DefaultRegulateValue);
        }
        #endregion

        //==> 현재 조명 값을 표시 할 수 있는 문자열 바인딩
        #region ==> CurLightValueStr
        private String _CurLightValueStr;
        public String CurLightValueStr
        {
            get { return _CurCameraLightValue.ToString(); }
            set
            {
                if (value != _CurLightValueStr)
                {
                    _CurLightValueStr = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        //==> 현재 조명 타입을 표시 할 수 있는 문자열 바인딩
        #region ==> CurLightTypeStr
        private String _CurLightTypeStr;
        public String CurLightTypeStr
        {
            get { return _CurLightTypeStr; }
            set
            {
                if (value != _CurLightTypeStr)
                {
                    _CurLightTypeStr = value;
                    RaisePropertyChanged();
                }
                SetLightChannel();
            }
        }

        private void SetLightChannel()
        {
            CurStageObj.SetLightChannel(CurCameraLightType);
            GetLightValue();

            //var channel = AssignedCamera.LightsChannels.Find(light => light.Type.Value.ToString().Equals(CurLightTypeStr));
            //if (channel != null)
            //    CurLightChannel
        }
        #endregion

        public void ChangeCamPosition(EnumProberCam cam)
        {
            try
            {
                (this.ViewModelManager() as ILoaderViewModelManager).Camera.IsMovingPos = true;
                CurStageObj.ChangeCamPosition(cam);
                SetCurCam(cam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                (this.ViewModelManager() as ILoaderViewModelManager).Camera.IsMovingPos = false;
            }
        }

        public void SetCurCam(EnumProberCam cam)
        {
            try
            {
                CurStageObj.UpdateCamera(cam, _UsingInterfaceType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetLightValue(int intensity)
        {
            try
            {
                CurStageObj.SetLightValue(intensity);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public int GetLightValue()
        {
            int retVal = -1;
            try
            {
                CurCameraLightValue = CurStageObj.GetLightValue(CurCameraLightType);
                retVal = CurCameraLightValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<EnumLightType> GetLightTypes()
        {
            List<EnumLightType> retVal = new List<EnumLightType>();
            try
            {
                retVal = CurStageObj.GetLightTypes();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        #endregion
    }
}
