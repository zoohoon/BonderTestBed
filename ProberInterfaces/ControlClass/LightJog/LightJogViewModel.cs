using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProberInterfaces.LightJog
{
    using MetroDialogInterfaces;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PnpSetup;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Windows.Media;

    public class LightJogViewModel : INotifyPropertyChanged, IFactoryModule , ILightJobViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region ==> Container's
        private IStageSupervisor StageSupervisor { get; set; }
        public IPnpManager PnpManager { get; set; }
        public IVisionManager VisionManager { get; set; }
        #endregion


        public CameraBtnType CurSelectedMag { get; set; }

        delegate void CamBtnSettingDelegate();

        private IUseLightJog UserLightJogModule;

        public bool IsUseNC { get; set; } = false;

        private bool _LightJogEnable;
        public bool LightJogEnable
        {
            get { return _LightJogEnable; }
            set
            {
                if (value != _LightJogEnable)
                {
                    _LightJogEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        List<LightValueParam> _LightParam = new List<LightValueParam>();
        ICamera _PreCam = null;
        public LightJogViewModel(
            int maxLightValue,
            int minLightValue)
        {
            //==> Init Container
            StageSupervisor = this.StageSupervisor();
            PnpManager = this.PnPManager();
            VisionManager = this.VisionManager();
            ////==> Init Button UI Color Brush & Text [Binding]
            //_CamBtnUiActionHandlerDic = new Dictionary<CameraBtnType, CamBtnSettingDelegate>();
            //_CamBtnUiActionHandlerDic.Add(CameraBtnType.High,
            //() =>
            //{
            //    //==> High Btn UI & Command Setting
            //    HighBtnText = _BtnNolmalHighText;//==> Set Btn Text
            //    HighBtnBrush = _BtnHighlightBrush1;//==> Set Btn Brush
            //    _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High2X];//==> Set Btn Action(Hihg2X Action 수행)

            //    //==> Low Btn UI & Command Setting
            //    LowBtnText = _BtnNolmalLowText;//==> Set Btn Text
            //    LowBtnBrush = _BtnNolmalBrush;//==> Set Btn Brush
            //    _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];//==> Set Btn Action(Low Action 수행)

            //    //==> Update High and Low Cam
            //    UpdateCamera();
            //    //==> Set Cur Camera Type
            //    AssignedCamera = _HighCam;
            //    //==> Change Cur Cam
            //    //((ISetupPageViewModel)(_Prober.ProberViewModel.ProberMainScreens)).PnpSetupStep.CurCam = AssignedCamera;
            //    UserLightJogModule.CurCam = AssignedCamera;
            //    //==> Change Cam Position
            //    ChangeCamPosition(AssignedCamera.CameraChannel.Type);

            //    //==> Set Cur Camera Light Index
            //    _CurCameraLigntTypeIndex = 0;
            //    ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
            //});
            //_CamBtnUiActionHandlerDic.Add(CameraBtnType.High2X,
            //() =>
            //{
            //    HighBtnText = _BtnNolmalHighText + " 2x";
            //    HighBtnBrush = _BtnHighlightBrush2;
            //    _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

            //    LowBtnText = _BtnNolmalLowText;
            //    LowBtnBrush = _BtnNolmalBrush;
            //    _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];

            //    UpdateCamera();
            //    AssignedCamera = _HighCam;
            //    //((ISetupPageViewModel)(_Prober.ProberViewModel.ProberMainScreens)).PnpSetupStep.CurCam = AssignedCamera;
            //    UserLightJogModule.CurCam = AssignedCamera;
            //    ChangeCamPosition(AssignedCamera.CameraChannel.Type);

            //    _CurCameraLigntTypeIndex = 0;
            //    ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
            //});
            //_CamBtnUiActionHandlerDic.Add(CameraBtnType.Low,
            //() =>
            //{
            //    LowBtnText = _BtnNolmalLowText;
            //    LowBtnBrush = _BtnHighlightBrush1;
            //    _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low2X];

            //    HighBtnText = _BtnNolmalHighText;
            //    HighBtnBrush = _BtnNolmalBrush;
            //    _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

            //    UpdateCamera();
            //    AssignedCamera = _LowCam;
            //    UserLightJogModule.CurCam = AssignedCamera;
            //    ChangeCamPosition(AssignedCamera.CameraChannel.Type);

            //    _CurCameraLigntTypeIndex = 0;
            //    ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
            //});
            //_CamBtnUiActionHandlerDic.Add(CameraBtnType.Low2X,
            //() =>
            //{
            //    LowBtnText = _BtnNolmalLowText + " 2x";
            //    LowBtnBrush = _BtnHighlightBrush2;
            //    _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];

            //    HighBtnText = _BtnNolmalHighText;
            //    HighBtnBrush = _BtnNolmalBrush;
            //    _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

            //    UpdateCamera();
            //    AssignedCamera = _LowCam;
            //    //((ISetupPageViewModel)(_Prober.ProberViewModel.ProberMainScreens)).PnpSetupStep.CurCam = AssignedCamera;
            //    UserLightJogModule.CurCam = AssignedCamera;
            //    ChangeCamPosition(AssignedCamera.CameraChannel.Type);

            //    _CurCameraLigntTypeIndex = 0;
            //    ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
            //});
            //==> Init Button UI Color Brush & Text [Binding]
            _CamBtnUiActionHandlerDic = new Dictionary<CameraBtnType, CamBtnSettingDelegate>();
            _CamBtnUiActionHandlerDic.Add(CameraBtnType.High,
            async () =>
            {
                if (CheckSWLimit(_HighCam.CameraChannel.Type) == false)
                {
                    if (AssignedCamera != null)
                    {
                        if (AssignedCamera.GetChannelType() != _HighCam.GetChannelType())
                        {
                            //await this.WaitCancelDialogService().ShowDialog("Wait");
                            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                            await Task.Run(() =>
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
                            AssignedCamera = _HighCam;
                            //==> Change Cur Cam
                            UserLightJogModule.CurCam = AssignedCamera;
                            //==> Change Cam Position
                            ChangeCamPosition(AssignedCamera.CameraChannel.Type);

                            //==> Set Cur Camera Light Index
                            _CurCameraLigntTypeIndex = 0;
                                ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
                            });

                            await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                        }
                    }
                    else
                    {
#pragma warning disable 4014
                        //brett// cell이 loader로 message dialog를 실행 후 대기 하지 않고 cell 자체 작업을 수행하기 위해 await 하지 않음, ex)init or lot 중 pause 이후 동작 등을 위해 
                        this.MetroDialogManager().ShowMessageDialog("AssignedCamera is null", "Please select a camera type and Click on 'StartGrab'", EnumMessageStyle.Affirmative, "Ok");
#pragma warning restore 4014
                    }
                }
                else
                {
                    const string message = "Operation Failure : S/W Limit";
                    const string caption = "Warning";
#pragma warning disable 4014
                    //brett// cell이 loader로 message dialog를 실행 후 대기 하지 않고 cell 자체 작업을 수행하기 위해 await 하지 않음, ex)init or lot 중 pause 이후 동작 등을 위해 
                    this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative, "Ok", "Cancel");
#pragma warning restore 4014
                }
            });

            _CamBtnUiActionHandlerDic.Add(CameraBtnType.Low,
            async () =>
            {
                if (CheckSWLimit(_LowCam.CameraChannel.Type) == false)
                {
                    if(AssignedCamera != null)
                    {
                        if (AssignedCamera.GetChannelType() != _LowCam.GetChannelType())
                        {

                            //await this.WaitCancelDialogService().ShowDialog("Wait");
                            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                            await Task.Run(() =>
                            {
                                LowBtnText = _BtnNolmalLowText;
                                LowBtnBrush = _BtnHighlightBrush1;

                                HighBtnText = _BtnNolmalHighText;
                                HighBtnBrush = _BtnNolmalBrush;
                                _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

                                UpdateCamera();
                                AssignedCamera = _LowCam;
                                UserLightJogModule.CurCam = AssignedCamera;
                                ChangeCamPosition(AssignedCamera.CameraChannel.Type);

                                _CurCameraLigntTypeIndex = 0;
                                ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
                            });

                            await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                        }
                    }
                    else
                    {
#pragma warning disable 4014
                        //brett// cell이 loader로 message dialog를 실행 후 대기 하지 않고 cell 자체 작업을 수행하기 위해 await 하지 않음, ex)init or lot 중 pause 이후 동작 등을 위해 
                        this.MetroDialogManager().ShowMessageDialog("AssignedCamera is null", "Please select a camera type and Click on 'StartGrab'", EnumMessageStyle.Affirmative, "Ok");
#pragma warning restore 4014
                    }

                }
                else
                {
                    const string message = "Operation Failure : S/W Limit";
                    const string caption = "Warning";
#pragma warning disable 4014
                    //brett// cell이 loader로 message dialog를 실행 후 대기 하지 않고 cell 자체 작업을 수행하기 위해 await 하지 않음, ex)init or lot 중 pause 이후 동작 등을 위해 
                    this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative, "Ok", "Cancel");
#pragma warning restore 4014
                }
            });

            //==> Init Max, Min, Init Light Value
            _MaxLightValue = maxLightValue;
            _MinLightValue = minLightValue;
        }

        //==> 현재 카메라
        public ICamera AssignedCamera { get; set; }

        //==> 현재 선택된 카메라 조명 타입들
        private List<EnumLightType> AssignedCameraLightTypes
        {
            get
            {
                return AssignedCamera.LightsChannels.Select(light => light.Type.Value).ToList();
            }
        }

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

        private ICamera _HighCam;//==> Light Jog의 High Btn 누를시 변경할 Camera
        private ICamera _LowCam;//==> Light Jog의 Low Btn 누를시 변경할 Camera

        //==> Camera Button 이 눌렸을 때 수행 해야할 Action들을 저장
        private Dictionary<CameraBtnType, CamBtnSettingDelegate> _CamBtnUiActionHandlerDic;

        //==> 현재 선택된 카메라 조명 인덱스
        #region ==> CurCameraLigntTypeIndex
        private int _CurCameraLigntTypeIndex;
        private int CurCameraLigntTypeIndex
        {
            get { return _CurCameraLigntTypeIndex; }
            set
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
            get { return AssignedCamera?.GetLight(CurCameraLightType)??0; }
            set
            {
                if (value == _CurCameraLightValue)
                {
                    return;
                }
                else if (value < _MinLightValue)
                    _CurCameraLightValue = _MinLightValue;
                else if (value > _MaxLightValue)
                    _CurCameraLightValue = _MaxLightValue;
                else
                    _CurCameraLightValue = value;

                RaisePropertyChanged();

                if (this.LoaderRemoteMediator() != null)
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPLightJogUpdated(CurCameraLightType, _CurCameraLightValue);
                }
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

        //==> High Camera 변경 버튼과 바인딩
        #region ==> HighBtnCommand
        CamBtnSettingDelegate _HighBtnHandler;
        private RelayCommand<object> _HighBtnCommand;
        public ICommand HighBtnCommand
        {
            get
            {
                if (null == _HighBtnCommand) _HighBtnCommand = new RelayCommand<object>(HighBtnFunc);
                return _HighBtnCommand;
            }
        }
        private void HighBtnFunc(object parameter)
        {
            _HighBtnHandler.Invoke();
            HighBtnEventHandler?.Execute(null);
        }
        public ICommand HighBtnEventHandler { get; set; }
        #endregion

        //==> Low Camera 변경 버튼과 바인딩
        #region ==> LowBtnCommand
        CamBtnSettingDelegate _LowBtnHandler;
        private RelayCommand<object> _LowBtnCommand;
        public ICommand LowBtnCommand
        {
            get
            {
                if (null == _LowBtnCommand) _LowBtnCommand = new RelayCommand<object>(LowBtnFunc);
                return _LowBtnCommand;
            }
        }
        private void LowBtnFunc(object parameter)
        {
            _LowBtnHandler.Invoke();
            LowBtnEventHandler?.Execute(null);
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
            if(AssignedCamera != null)
            {
                if (CurCameraLigntTypeIndex != 0)
                    ChangeCurCameraLightType(CurCameraLigntTypeIndex - 1);
                else
                    ChangeCurCameraLightType(AssignedCameraLightTypes.Count);
            }
            else
            {
                this.MetroDialogManager().ShowMessageDialog("AssignedCamera is null", "Please select a camera type and Click on 'StartGrab'", EnumMessageStyle.Affirmative, "Ok");
            }
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
            if (AssignedCamera != null)
            {
                if (CurCameraLigntTypeIndex + 1 != AssignedCameraLightTypes.Count)
                    ChangeCurCameraLightType(CurCameraLigntTypeIndex + 1);
                else
                    ChangeCurCameraLightType(0);
            }
            else
            {
                this.MetroDialogManager().ShowMessageDialog("AssignedCamera is null", "Please select a camera type and Click on 'StartGrab'", EnumMessageStyle.Affirmative, "Ok");
            }
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
                }

                RaisePropertyChanged();
            }
        }

        private void SetLightChannel()
        {
            try
            {
                var channel = AssignedCamera.LightsChannels.Find(light => light.Type.Value.ToString().Equals(CurLightTypeStr));

                if (channel != null)
                {
                    if(this.LightAdmin() != null)
                    {
                        CurLightChannel = this.LightAdmin().GetLightChannel(channel.ChannelMapIdx.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
        #endregion

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



        private bool CheckSWLimit(EnumProberCam cam)
        {
            try
            {
                double xAxisRelPos = 0.0;
                double yAxisRelPos = 0.0;
                double zAxisRelPos = 0.0;

                if (!IsUseNC)
                {
                    switch (cam)
                    {
                        case EnumProberCam.WAFER_LOW_CAM://==> 현재 Camera가 WH로 가정하고 WL로 이동
                            xAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value;
                            yAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value;
                            zAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Z, this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            break;

                        case EnumProberCam.WAFER_HIGH_CAM://==> 현재 Camera가 WL로 가정하고 WH로 이동
                            xAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value * -1.0;
                            yAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value * -1.0;
                            zAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value * -1.0;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Z, this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            break;

                        case EnumProberCam.PIN_LOW_CAM://==> 현재 Camera가 PH로 가정하고 PL로 이동
                            xAxisRelPos = -this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value;
                            yAxisRelPos = -this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value;
                            zAxisRelPos = -this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(StageSupervisor.StageModuleState.PinViewAxis, this.MotionManager().GetAxis(StageSupervisor.StageModuleState.PinViewAxis).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            break;

                        case EnumProberCam.PIN_HIGH_CAM://==> 현재 Camera가 PL로 가정하고 PH로 이동
                            xAxisRelPos = this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value;
                            yAxisRelPos = this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value;
                            zAxisRelPos = this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(StageSupervisor.StageModuleState.PinViewAxis, this.MotionManager().GetAxis(StageSupervisor.StageModuleState.PinViewAxis).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;

                            break;
                    }
                }
                else
                {
                    switch (cam)
                    {
                        case EnumProberCam.WAFER_LOW_CAM://==> 현재 Camera가 WH로 가정하고 WL로 이동
                            xAxisRelPos = -this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value;
                            yAxisRelPos = -this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value;
                            zAxisRelPos = -this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.PZ, this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            break;

                        case EnumProberCam.WAFER_HIGH_CAM://==> 현재 Camera가 WL로 가정하고 WH로 이동
                            xAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value * -1.0;
                            yAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value * -1.0;
                            zAxisRelPos = this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value * -1.0;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.PZ, this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            break;

                        case EnumProberCam.PIN_LOW_CAM://==> 현재 Camera가 PH로 가정하고 PL로 이동
                            xAxisRelPos = -this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value;
                            yAxisRelPos = -this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value;
                            zAxisRelPos = -this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(StageSupervisor.StageModuleState.PinViewAxis, this.MotionManager().GetAxis(StageSupervisor.StageModuleState.PinViewAxis).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            break;

                        case EnumProberCam.PIN_HIGH_CAM://==> 현재 Camera가 PL로 가정하고 PH로 이동
                            xAxisRelPos = this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value;
                            yAxisRelPos = this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value;
                            zAxisRelPos = this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value;

                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            if (this.MotionManager().CheckSWLimit(StageSupervisor.StageModuleState.PinViewAxis, this.MotionManager().GetAxis(StageSupervisor.StageModuleState.PinViewAxis).Status.Position.Ref + zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return true;
                            break;
                    }
                }

                return false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Exception error in CheckSWLimit()");
                LoggerManager.Exception(err);
                return true;
            }

        }


        public bool UpdateCamera(EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            if (camtype != EnumProberCam.UNDEFINED)
            {
                AssignedCamera = VisionManager.GetCam(camtype);
                if(UserLightJogModule != null)
                {
                    if(UserLightJogModule.CurCam != null)
                    {
                        if (UserLightJogModule.CurCam.GetChannelType() != AssignedCamera.GetChannelType())
                            UserLightJogModule.CurCam = AssignedCamera;
                    }
                }

            }
            else
            {

                //ISetupPageViewModel setupScreen = _Prober.ProberViewModel.ProberMainScreens as ISetupPageViewModel;
                //ISetupPageViewModel setupScreen = _Prober.ViewModelManager.MainScreenViewModel as ISetupPageViewModel;
                //if (setupScreen == null)
                //{
                //    AssignedCamera = null;
                //    return false;
                //}

                //AssignedCamera = ((IPnpSetup)setupScreen.PnpStep).CurCam;


                AssignedCamera = UserLightJogModule.CurCam;
            }


            if (AssignedCamera == null)
                return false;

            //==> Set High Camera, Low Camera
            switch (AssignedCamera.CameraChannel.Type)
            {
                case EnumProberCam.WAFER_HIGH_CAM:
                    _HighCam = AssignedCamera;
                    _LowCam = VisionManager.GetCam(EnumProberCam.WAFER_LOW_CAM);
                    CurSelectedMag = CameraBtnType.High;
                    break;
                case EnumProberCam.PIN_HIGH_CAM:
                    _HighCam = AssignedCamera;
                    _LowCam = VisionManager.GetCam(EnumProberCam.PIN_LOW_CAM);
                    CurSelectedMag = CameraBtnType.High;
                    break;

                case EnumProberCam.WAFER_LOW_CAM:
                    _HighCam = VisionManager.GetCam(EnumProberCam.WAFER_HIGH_CAM);
                    _LowCam = AssignedCamera;
                    CurSelectedMag = CameraBtnType.Low;
                    break;
                case EnumProberCam.PIN_LOW_CAM:
                    _HighCam = VisionManager.GetCam(EnumProberCam.PIN_HIGH_CAM);
                    _LowCam = AssignedCamera;
                    CurSelectedMag = CameraBtnType.Low;
                    break;
                default:
                    _HighCam = null;
                    _LowCam = null;
                    CurSelectedMag = CameraBtnType.UNDEFINED;
                    break;
            }
            return true;
        }


        public void InitCameraJog(IUseLightJog module, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {

            try
            {
                if (module == null)
                    return;
                else
                    UserLightJogModule = module;
                //if (_Prober.ProberViewModel == null)
                if (this.ViewModelManager() == null)
                    return;

                if (UpdateCamera(camtype) == false)
                    return;

                if (AssignedCameraLightTypes.Count < 1)
                    return;

                //==> Init Light Type Index
                CurCameraLigntTypeIndex = 0;
                //==> Init Light Type
                _CurCameraLightType = AssignedCameraLightTypes[CurCameraLigntTypeIndex];
                //==> Init Cur Camera Light Value
                CurCameraLightValue = AssignedCamera.GetLight(_CurCameraLightType);

                //==> Init Light Type & Light Value UI [Binding]
                CurLightTypeStr = CurCameraLightType.ToString();
                CurLightValueStr = CurCameraLightValue.ToString();

                //==> Init Camera Btn UI
                if (AssignedCamera.CameraChannel.Type == _HighCam.CameraChannel.Type)
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
                _PreCam = null;
                _LightParam = new List<LightValueParam>();
            }
            catch (Exception err)
            {
                throw err;
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
                throw;
            }
        }
        
        private void ChangeCurCameraLightType(int lightTypeIndex)
        {
            try
            {
                if (AssignedCamera == null)
                    return;
                CurCameraLigntTypeIndex = lightTypeIndex;
                _CurCameraLightType = AssignedCameraLightTypes[CurCameraLigntTypeIndex];
                CurLightTypeStr = CurCameraLightType.ToString();
                CurCameraLightValue = AssignedCamera.GetLight(CurCameraLightType);
                SetCurCameraLightValue(CurCameraLightValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetCurCameraLightValue(int intensity)
        {
            if (AssignedCamera == null)
                return;
            if (intensity > 255) intensity = 255;
            else if (intensity < 0) intensity = 0;
            AssignedCamera.SetLight(CurCameraLightType, (ushort)intensity);
            //CurCameraLightValue = intensity;
            //AssignedCamera.SetLight(CurCameraLightType, (ushort)CurCameraLightValue);
            CurLightValueStr = CurCameraLightValue.ToString();
        }

        /*
         * High, High 2x 등으로 배율 개념이 추가된 Camera 변경이 가능하려면
         * 현재 Camera가 어떤 상태이건 High, Low, High 2x 카메라로 바꿀 수 있어야 하지 않을까???
         * 
         * 문제 : High -> High 2x, High 2x -> High 로 바꿀때 상대 좌표로 계산한다면,,,
         */
        //==> Parameter 로 들어온 cam으로 바꾼다.
        public void ChangeCamPosition(EnumProberCam cam)
        {
            try
            {
                if (cam == EnumProberCam.INVALID ||
              cam == EnumProberCam.UNDEFINED ||
              AssignedCamera == null)
                    return;

                if (_PreCam != null)
                {
                    ICamera lcam = VisionManager.GetCam(cam);
                    foreach (var light in _LightParam)
                    {
                        lcam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }

                if (!IsUseNC)
                {
                    switch (cam)
                    {
                        case EnumProberCam.WAFER_LOW_CAM://==> 현재 Camera가 WH로 가정하고 WL로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                            WaferCoordinate lwcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.WaferLowViewMove(lwcoord.GetX(), lwcoord.GetY(), lwcoord.GetZ());
                            break;
                        case EnumProberCam.WAFER_HIGH_CAM://==> 현재 Camera가 WL로 가정하고 WH로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                            WaferCoordinate hwcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.WaferHighViewMove(hwcoord.GetX(), hwcoord.GetY(), hwcoord.GetZ());
                            break;
                        case EnumProberCam.PIN_LOW_CAM://==> 현재 Camera가 PH로 가정하고 PL로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                            PinCoordinate lpcoord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.PinLowViewMove(lpcoord.GetX(), lpcoord.GetY(), lpcoord.GetZ());
                            break;
                        case EnumProberCam.PIN_HIGH_CAM://==> 현재 Camera가 PL로 가정하고 PH로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                            PinCoordinate hpcoord = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.PinHighViewMove(hpcoord.GetX(), hpcoord.GetY(), hpcoord.GetZ());
                            break;
                    }
                }
                else
                {
                    //StageSupervisor.StageModuleState.ZCLEARED();
                    switch (cam)
                    {
                        case EnumProberCam.WAFER_LOW_CAM://==> 현재 Camera가 WH로 가정하고 WL로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                            NCCoordinate lwcoord = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.WaferLowCamCoordMoveNCpad(lwcoord, 0);
                            break;
                        case EnumProberCam.WAFER_HIGH_CAM://==> 현재 Camera가 WL로 가정하고 WH로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                            NCCoordinate hwcoord = this.CoordinateManager().WaferLowNCPadConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.WaferHighCamCoordMoveNCpad(hwcoord, 0);
                            break;
                        case EnumProberCam.PIN_LOW_CAM://==> 현재 Camera가 PH로 가정하고 PL로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                            PinCoordinate lpcoord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.TouchSensorLowViewMove(lpcoord.GetX(), lpcoord.GetY(), lpcoord.GetZ());
                            break;
                        case EnumProberCam.PIN_HIGH_CAM://==> 현재 Camera가 PL로 가정하고 PH로 이동
                            _PreCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                            PinCoordinate hpcoord = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                            StageSupervisor.StageModuleState.TouchSensorHighViewMove(hpcoord.GetX(), hpcoord.GetY(), hpcoord.GetZ());
                            break;
                    }
                }

                _LightParam.Clear();
                foreach (var light in _PreCam.LightsChannels)
                {
                    _LightParam.Add(
                        new LightValueParam(light.Type.Value, (ushort)_PreCam.GetLight(light.Type.Value)));
                }

                VisionManager.StartGrab(cam, this);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public List<EnumLightType> GetLightTypes()
        {
            return AssignedCameraLightTypes;
        }

        public bool SetLightType(EnumLightType lighttype)
        {
            _CurCameraLightType = lighttype;
            return true;
        }

        public void SetLightValue(int intensity)
        {
            AssignedCamera.SetLight(CurCameraLightType, (ushort)intensity);
        }

        public List<EnumLightType> SetAssignedCameara(EnumProberCam camtype)
        {
            try
            {
                AssignedCamera = this.VisionManager().GetCam(camtype);
                UserLightJogModule.CurCam = AssignedCamera;
                return AssignedCameraLightTypes;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }
    }
}

#region ==> OLD
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProberInterfaces.LightJog
//{
//    using Autofac;
//    using ProberInterfaces;
//    using ProberInterfaces.Param;
//    using RelayCommandBase;
//    using System.ComponentModel;
//    using System.Windows.Input;
//    using System.Windows.Media;

//    public class LightJogViewModel : INotifyPropertyChanged
//    {
//        #region ==> Notify Property Changed
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged(String info)
//        {
//            if (PropertyChanged != null)
//            {
//                PropertyChanged(this, new PropertyChangedEventArgs(info));
//            }
//        }
//        #endregion

//        #region ==> Container's
//        private IStageSupervisor StageSupervisor
//        {
//            get
//            {
//                return _Container.Resolve<IStageSupervisor>();
//            }
//        }
//        private IVisionManager VisionManager
//        {
//            get
//            {
//                return _Container.Resolve<IVisionManager>();
//            }
//        }
//        #endregion

//        private enum CameraBtnType { UNDEFINED, High, Low, High2X, Low2X }

//        delegate void CamBtnSettingDelegate();

//        public LightJogViewModel(
//            Autofac.IContainer container,
//            int maxLightValue,
//            int minLightValue,
//            int initLightValue,
//            EnumProberCam initCameraType)
//        {
//            //==> Init Container
//            _Container = container;

//            ////==> Init Button UI Color Brush & Text [Binding]
//            _CamBtnUiActionHandlerDic = new Dictionary<CameraBtnType, CamBtnSettingDelegate>();
//            _CamBtnUiActionHandlerDic.Add(CameraBtnType.High,
//            () =>
//            {
//                //==> High Btn
//                HighBtnText = _BtnNolmalHighText;//==> Set Btn Text
//                HighBtnBrush = _BtnHighlightBrush1;//==> Set Btn Brush
//                _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High2X];//==> Set Btn Action(Hihg2X Action 수행)

//                //==> Low Btn
//                LowBtnText = _BtnNolmalLowText;//==> Set Btn Text
//                LowBtnBrush = _BtnNolmalBrush;//==> Set Btn Brush
//                _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];//==> Set Btn Action(Low Action 수행)

//                //==> Set Cur Camera Type
//                _CurCameraType = _HighCameraType;

//                //==> Set Cur Camera Light Index
//                _CurCameraLigntTypeIndex = 0;
//                ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
//            });
//            _CamBtnUiActionHandlerDic.Add(CameraBtnType.High2X,
//            () =>
//            {
//                HighBtnText = _BtnNolmalHighText + " 2x";
//                HighBtnBrush = _BtnHighlightBrush2;
//                _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

//                LowBtnText = _BtnNolmalLowText;
//                LowBtnBrush = _BtnNolmalBrush;
//                _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];

//                _CurCameraType = _HighCameraType;

//                _CurCameraLigntTypeIndex = 0;
//                ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
//            });
//            _CamBtnUiActionHandlerDic.Add(CameraBtnType.Low,
//            () =>
//            {
//                LowBtnText = _BtnNolmalLowText;
//                LowBtnBrush = _BtnHighlightBrush1;
//                _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low2X];

//                HighBtnText = _BtnNolmalHighText;
//                HighBtnBrush = _BtnNolmalBrush;
//                _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

//                _CurCameraType = _LowCameraType;

//                _CurCameraLigntTypeIndex = 0;
//                ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
//            });
//            _CamBtnUiActionHandlerDic.Add(CameraBtnType.Low2X,
//            () =>
//            {
//                LowBtnText = _BtnNolmalLowText + " 2x";
//                LowBtnBrush = _BtnHighlightBrush2;
//                _LowBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.Low];

//                HighBtnText = _BtnNolmalHighText;
//                HighBtnBrush = _BtnNolmalBrush;
//                _HighBtnHandler = _CamBtnUiActionHandlerDic[CameraBtnType.High];

//                _CurCameraType = _LowCameraType;

//                _CurCameraLigntTypeIndex = 0;
//                ChangeCurCameraLightType(_CurCameraLigntTypeIndex);
//            });

//            //==> Init Max, Min, Init Light Value
//            _MaxLightValue = maxLightValue;
//            _MinLightValue = minLightValue;
//            _InitLightValue = initLightValue;
//            if (_InitLightValue < _MinLightValue || _InitLightValue > _MaxLightValue)
//                _InitLightValue = (_MinLightValue + _MaxLightValue / 2);

//            //==> Init Camera Type
//            ChangeInitCamera(initCameraType);
//            ResetCameraJog();
//        }

//        //==> 현재 카메라
//        private ICamera CurCamera
//        {
//            get
//            {
//                return VisionManager.GetCam(_CurCameraType);
//            }
//        }

//        //==> 현재 선택된 카메라 조명 타입들
//        private List<EnumLightType> CurCameraLightTypes
//        {
//            get
//            {
//                return CurCamera.LightsChannels.Select(light => light.Type).ToList();
//            }
//        }

//        private Autofac.IContainer _Container;
//        private readonly int _MaxLightValue;
//        private readonly int _MinLightValue;
//        private readonly int _InitLightValue;
//        private const int _DefaultRegulateValue = 10;//==> Camera 조명값 변화량

//        //==> Button Brush
//        private readonly Brush _BtnNolmalBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x77, 0x77, 0x77));
//        private readonly Brush _BtnHighlightBrush1 = new SolidColorBrush(Colors.Orange);
//        private readonly Brush _BtnHighlightBrush2 = new SolidColorBrush(Colors.Red);

//        //==> Button Text
//        private const String _BtnNolmalHighText = "High";
//        private const String _BtnNolmalLowText = "Low";

//        private EnumProberCam _InitCameraType;//==> Light Jog 초기화시 변경할 카메라 타입
//        private EnumProberCam _HighCameraType;//==> Light Jog의 High Btn 누를시 변경할 Camera
//        private EnumProberCam _LowCameraType;//==> Light Jog의 Low Btn 누를시 변경할 Camera

//        //==> Camera Button 이 눌렸을 때 수행 해야할 Action들을 저장
//        private Dictionary<CameraBtnType, CamBtnSettingDelegate> _CamBtnUiActionHandlerDic;

//        //==> 현재 선택된 카메라 타입
//        #region ==> CurCameraType
//        public EnumProberCam _CurCameraType;
//        public EnumProberCam CurCameraType
//        {
//            get { return _CurCameraType; }
//        }
//        #endregion

//        //==> 현재 선택된 카메라 조명 인덱스
//        #region ==> CurCameraLigntTypeIndex
//        private int _CurCameraLigntTypeIndex;
//        private int CurCameraLigntTypeIndex
//        {
//            get { return _CurCameraLigntTypeIndex; }
//            set
//            {
//                if (value == _CurCameraLigntTypeIndex)
//                    return;
//                else if (value < 0)//==> Under Limit Check
//                    _CurCameraLigntTypeIndex = 0;
//                else if (value >= CurCameraLightTypes.Count)//==> Upper Limit Check
//                    _CurCameraLigntTypeIndex = CurCameraLightTypes.Count - 1;
//                else
//                    _CurCameraLigntTypeIndex = value;
//            }
//        }
//        #endregion

//        //==> 현재 선택된 조명 타입
//        #region ==> CurCameraLightType
//        private EnumLightType _CurCameraLightType;
//        public EnumLightType CurCameraLightType
//        {
//            get
//            {
//                return _CurCameraLightType;
//            }
//        }
//        #endregion

//        //==> 현재 Camera 조명의 밝기
//        #region ==> CurCameraLightValue 
//        private int _CurCameraLightValue;
//        public int CurCameraLightValue
//        {
//            get { return _CurCameraLightValue; }
//            set
//            {
//                if (value == _CurCameraLightValue)
//                    return;
//                else if (value < _MinLightValue)
//                    _CurCameraLightValue = _MinLightValue;
//                else if (value > _MaxLightValue)
//                    _CurCameraLightValue = _MaxLightValue;
//                else
//                    _CurCameraLightValue = value;
//            }
//        }
//        #endregion

//        //==> Hight Camera 변경 버튼 색깔과 바인딩
//        #region ==> HighBtnBrush
//        private Brush _HighBtnBrush;
//        public Brush HighBtnBrush
//        {
//            get { return _HighBtnBrush; }
//            set
//            {
//                if (value != _HighBtnBrush)
//                {
//                    _HighBtnBrush = value;
//                    NotifyPropertyChanged("HighBtnBrush");
//                }
//            }
//        }
//        #endregion

//        //==> Low Camera 변경 버튼 색깔과 바인딩
//        #region ==> LowBtnBrush
//        private Brush _LowBtnBrush;
//        public Brush LowBtnBrush
//        {
//            get { return _LowBtnBrush; }
//            set
//            {
//                if (value != _LowBtnBrush)
//                {
//                    _LowBtnBrush = value;
//                    NotifyPropertyChanged("LowBtnBrush");
//                }
//            }
//        }
//        //==> Hight Camera 변경 버튼 색깔과 바인딩
//        //==> Hight Camera 변경 버튼 색깔과 바인딩
//        #endregion

//        //==> High Camera 변경 버튼 Text와 바인딩
//        #region ==> HighBtnText
//        private String _HighBtnText;
//        public String HighBtnText
//        {
//            get { return _HighBtnText; }
//            set
//            {
//                if (value != _HighBtnText)
//                {
//                    _HighBtnText = value;
//                    NotifyPropertyChanged("HighBtnText");
//                }
//            }
//        }
//        #endregion

//        //==> Low Camera 변경 버튼 Text와 바인딩
//        #region ==> LowBtnText
//        private String _LowBtnText;
//        public String LowBtnText
//        {
//            get { return _LowBtnText; }
//            set
//            {
//                if (value != _LowBtnText)
//                {
//                    _LowBtnText = value;
//                    NotifyPropertyChanged("LowBtnText");
//                }
//            }
//        }
//        #endregion

//        //==> High Camera 변경 버튼과 바인딩
//        #region ==> HighBtnCommand
//        CamBtnSettingDelegate _HighBtnHandler;
//        private RelayCommand<object> _HighBtnCommand;
//        public ICommand HighBtnCommand
//        {
//            get
//            {
//                if (null == _HighBtnCommand) _HighBtnCommand = new RelayCommand<object>(HighBtnFunc);
//                return _HighBtnCommand;
//            }
//        }
//        private void HighBtnFunc(object parameter)
//        {
//            _HighBtnHandler.Invoke();
//            ChangeCam(_CurCameraType);
//        }
//        #endregion

//        //==> Low Camera 변경 버튼과 바인딩
//        #region ==> LowBtnCommand
//        CamBtnSettingDelegate _LowBtnHandler;
//        private RelayCommand<object> _LowBtnCommand;
//        public ICommand LowBtnCommand
//        {
//            get
//            {
//                if (null == _LowBtnCommand) _LowBtnCommand = new RelayCommand<object>(LowBtnFunc);
//                return _LowBtnCommand;
//            }
//        }
//        private void LowBtnFunc(object parameter)
//        {
//            _LowBtnHandler.Invoke();
//            ChangeCam(_CurCameraType);
//        }
//        #endregion

//        #region ==> 
//        private RelayCommand<object> _CamSwitchBtnCommand;
//        public ICommand CamSwitchBtnCommand
//        {
//            get
//            {
//                if (null == _CamSwitchBtnCommand) _CamSwitchBtnCommand = new RelayCommand<object>(CamSwitchBtnFunc);
//                return _CamSwitchBtnCommand;
//            }
//        }
//        private void CamSwitchBtnFunc(object parameter)
//        {
//            _LowBtnHandler.Invoke();
//            ChangeCam(_CurCameraType);
//        }
//        #endregion


//        //==> 이전 조명 타입으로 변경하는 버튼과 바인딩
//        #region ==> LightTypeUpCommand
//        private RelayCommand<object> _LightTypeUpCommand;
//        public ICommand LightTypeUpCommand
//        {
//            get
//            {
//                if (null == _LightTypeUpCommand) _LightTypeUpCommand = new RelayCommand<object>(LightTypeUpFunc);
//                return _LightTypeUpCommand;
//            }
//        }
//        private void LightTypeUpFunc(object parameter)
//        {
//            ChangeCurCameraLightType(CurCameraLigntTypeIndex - 1);
//        }
//        #endregion

//        //==> 다음 조명 타입으로 변경하는 버튼과 바인딩
//        #region ==> LightTypeDownCommand
//        private RelayCommand<object> _LightTypeDownCommand;
//        public ICommand LightTypeDownCommand
//        {
//            get
//            {
//                if (null == _LightTypeDownCommand) _LightTypeDownCommand = new RelayCommand<object>(LightTypeDownFunc);
//                return _LightTypeDownCommand;
//            }
//        }
//        private void LightTypeDownFunc(object parameter)
//        {
//            ChangeCurCameraLightType(CurCameraLigntTypeIndex + 1);
//        }
//        #endregion

//        //==> 조명 값을 올리는 버튼과 바인딩
//        #region ==> LightValueUpCommand
//        private RelayCommand<object> _LightValueUpCommand;
//        public ICommand LightValueUpCommand
//        {
//            get
//            {
//                if (null == _LightValueUpCommand) _LightValueUpCommand = new RelayCommand<object>(LightValueUpFunc);
//                return _LightValueUpCommand;
//            }
//        }
//        private void LightValueUpFunc(object parameter)
//        {
//            SetCurCameraLightValue(CurCameraLightValue + _DefaultRegulateValue);
//        }
//        #endregion

//        //==> 조명 값을 내리는 버튼과 바인딩
//        #region ==> LightValueDownCommand
//        private RelayCommand<object> _LightValueDownCommand;
//        public ICommand LightValueDownCommand
//        {
//            get
//            {
//                if (null == _LightValueDownCommand) _LightValueDownCommand = new RelayCommand<object>(LightValueDownFunc);
//                return _LightValueDownCommand;
//            }
//        }
//        private void LightValueDownFunc(object parameter)
//        {
//            SetCurCameraLightValue(CurCameraLightValue - _DefaultRegulateValue);
//        }
//        #endregion

//        //==> 현재 조명 값을 표시 할 수 있는 문자열 바인딩
//        #region ==> CurLightValueStr
//        private String _CurLightValueStr;
//        public String CurLightValueStr
//        {
//            get { return _CurLightValueStr; }
//            set
//            {
//                if (value != _CurLightValueStr)
//                {
//                    _CurLightValueStr = value;
//                    NotifyPropertyChanged("CurLightValueStr");
//                }
//            }
//        }
//        #endregion

//        //==> 현재 조명 타입을 표시 할 수 있는 문자열 바인딩
//        #region ==> CurLightTypeStr
//        private String _CurLightTypeStr;
//        public String CurLightTypeStr
//        {
//            get { return _CurLightTypeStr; }
//            set
//            {
//                if (value != _CurLightTypeStr)
//                {
//                    _CurLightTypeStr = value;
//                    NotifyPropertyChanged("CurLightTypeStr");
//                }
//            }
//        }
//        #endregion

//        public void ChangeInitCamera(EnumProberCam initCameraType)
//        {
//            _InitCameraType = initCameraType;
//            switch (_InitCameraType)
//            {
//                case EnumProberCam.WAFER_HIGH_CAM:
//                    _HighCameraType = _InitCameraType;
//                    _LowCameraType = EnumProberCam.WAFER_LOW_CAM;
//                    break;
//                case EnumProberCam.WAFER_LOW_CAM:
//                    _HighCameraType = EnumProberCam.WAFER_HIGH_CAM;
//                    _LowCameraType = _InitCameraType;
//                    break;
//                case EnumProberCam.PIN_HIGH_CAM:
//                    _HighCameraType = _InitCameraType;
//                    _LowCameraType = EnumProberCam.PIN_LOW_CAM;
//                    break;
//                case EnumProberCam.PIN_LOW_CAM:
//                    _HighCameraType = EnumProberCam.PIN_HIGH_CAM;
//                    _LowCameraType = _InitCameraType;
//                    break;
//            }
//        }

//        public void ResetCameraJog()
//        {
//            //==> Init Camera Type
//            _CurCameraType = _InitCameraType;
//            if (CurCamera == null)
//                return;
//            if (CurCameraLightTypes.Count < 1)
//                return;

//            //==> Set Light Intensity by Init Light Value
//            foreach (LightChannelType channel in CurCamera.LightsChannels)
//                CurCamera.SetLight(channel.Type, (ushort)_InitLightValue);

//            //==> Init Cur Camera Light Value
//            CurCameraLightValue = _InitLightValue;

//            //==> Init Light Type Index
//            CurCameraLigntTypeIndex = 0;

//            //==> Init Light Type
//            _CurCameraLightType = CurCameraLightTypes[CurCameraLigntTypeIndex];

//            //==> Init Light Type & Light Value UI [Binding]
//            CurLightTypeStr = CurCameraLightType.ToString();
//            CurLightValueStr = CurCameraLightValue.ToString();

//            //==> Init Camera Btn UI
//            if (_InitCameraType == _HighCameraType)
//                _CamBtnUiActionHandlerDic[CameraBtnType.High].Invoke();
//            else
//                _CamBtnUiActionHandlerDic[CameraBtnType.Low].Invoke();
//        }

//        private void ChangeCurCameraLightType(int lightTypeIndex)
//        {
//            CurCameraLigntTypeIndex = lightTypeIndex;
//            _CurCameraLightType = CurCameraLightTypes[CurCameraLigntTypeIndex];
//            CurLightTypeStr = CurCameraLightType.ToString();
//            CurCameraLightValue = CurCamera.GetLight(CurCameraLightType);
//            SetCurCameraLightValue(CurCameraLightValue);
//        }

//        private void SetCurCameraLightValue(int intensity)
//        {
//            CurCameraLightValue = intensity;
//            CurCamera.SetLight(CurCameraLightType, (ushort)CurCameraLightValue);
//            CurLightValueStr = CurCameraLightValue.ToString();
//        }

//        /*
//         * High, High 2x 등으로 배율 개념이 추가된 Camera 변경이 가능하려면
//         * 현재 Camera가 어떤 상태이건 High, Low, High 2x 카메라로 바꿀 수 있어야 하지 않을까???
//         * 
//         * 문제 : High -> High 2x, High 2x -> High 로 바꿀때 상대 좌표로 계산한다면,,,
//         */
//        //==> Parameter 로 들어온 cam으로 바꾼다.
//        private void ChangeCam(EnumProberCam cam)
//        {
//            if (cam != EnumProberCam.INVALID || cam != EnumProberCam.UNDEFINED)
//            {
//                switch (cam)
//                {
//                    case EnumProberCam.WAFER_LOW_CAM:
//                        WaferCoordinate lwcoord = StageSupervisor.CoordinateManager.WaferHighChuckConvert.CurrentPosConvert();
//                        StageSupervisor.StageModuleState.WaferLowViewMove(lwcoord.GetX(), lwcoord.GetY(), lwcoord.GetZ());
//                        break;
//                    case EnumProberCam.WAFER_HIGH_CAM:
//                        WaferCoordinate hwcoord = StageSupervisor.CoordinateManager.WaferLowChuckConvert.CurrentPosConvert();
//                        StageSupervisor.StageModuleState.WaferHighViewMove(hwcoord.GetX(), hwcoord.GetY(), hwcoord.GetZ());
//                        break;
//                    case EnumProberCam.PIN_LOW_CAM:
//                        PinCoordinate lpcoord = StageSupervisor.CoordinateManager.PinHighPinConvert.CurrentPosConvert();
//                        StageSupervisor.StageModuleState.PinLowViewMove(lpcoord.GetX(), lpcoord.GetY(), lpcoord.GetZ());
//                        break;
//                    case EnumProberCam.PIN_HIGH_CAM:
//                        PinCoordinate hpcoord = StageSupervisor.CoordinateManager.PinLowPinConvert.CurrentPosConvert();
//                        StageSupervisor.StageModuleState.PinHighViewMove(hpcoord.GetX(), hpcoord.GetY(), hpcoord.GetZ());
//                        break;
//                }
//                VisionManager.StartGrab(cam);
//            }
//        }
//    }
//}
#endregion
