using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CameraModule
{
    using Autofac;
    using System.Windows;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using System.Runtime.CompilerServices;
    using LogModule;
    using SubstrateObjects;
    using VisionParams.Camera;
    using System.Threading;
    using System.Diagnostics;
    using SystemExceptions.VisionException;

    [Serializable]
    public abstract class CameraBase : ICamera
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ..//Property
        [XmlIgnore, JsonIgnore]
        public bool Initialized { get; set; } = false;

        private ImageBuffer.DrawDisplay _DrawDisplayDelegate;
        public ImageBuffer.DrawDisplay DrawDisplayDelegate
        {
            get { return _DrawDisplayDelegate; }
            set
            {
                _DrawDisplayDelegate = value;
                ClearDisplay();
            }
        }

        private ICameraParameter _Param = new CameraParameter();
        public ICameraParameter Param
        {
            get { return _Param; }
            set { _Param = value; }
        }
        private List<LightChannelType> _LightChannels;
        public List<LightChannelType> LightsChannels
        {
            get { return _LightChannels; }
            set { _LightChannels = value; }
        }
        // Autofac ?????? Light assign ???
        private IDisplay _DisplayService;
        [XmlIgnore, JsonIgnore]
        public IDisplay DisplayService
        {
            get
            {
                return _DisplayService;
            }

            set
            {
                if (_DisplayService == value) return;
                _DisplayService = value;
                RaisePropertyChanged();
            }
        }


        private CameraChannelType _CameraChannel;
        public CameraChannelType CameraChannel
        {
            get { return _CameraChannel; }
            set { _CameraChannel = value; }
        }

        private CatCoordinates _CamSystemUpdatePos;
        /// <summary>
        /// CamSystemPos ?? ???????? ????????? Update ??? ???????
        /// </summary>
        public CatCoordinates CamSystemUpdatePos
        {
            get { return _CamSystemUpdatePos; }
            set
            {
                if (value != _CamSystemUpdatePos)
                {
                    _CamSystemUpdatePos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CamSystemPos;
        [XmlIgnore, JsonIgnore]
        public CatCoordinates CamSystemPos
        {
            get { return _CamSystemPos; }
            protected set
            {
                if (value != _CamSystemPos)
                {
                    _CamSystemPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserIndex _CamSystemUI;
        [XmlIgnore, JsonIgnore]
        public UserIndex CamSystemUI
        {
            get { return _CamSystemUI; }
            protected set
            {
                if (value != _CamSystemUI)
                {
                    _CamSystemUI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _CamSystemMI;
        [XmlIgnore, JsonIgnore]
        public MachineIndex CamSystemMI
        {
            get { return _CamSystemMI; }
            protected set
            {
                if (value != _CamSystemMI)
                {
                    _CamSystemMI = value;
                    RaisePropertyChanged();
                }
            }
        }
        private NCCoordinate _CamSystemNC;
        [XmlIgnore, JsonIgnore]
        public NCCoordinate CamSystemNC
        {
            get { return _CamSystemNC; }
            protected set
            {
                if (value != _CamSystemNC)
                {
                    _CamSystemNC = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ImageBuffer _CamCurImage = new ImageBuffer();
        [XmlIgnore, JsonIgnore]
        public ImageBuffer CamCurImage
        {
            get { return _CamCurImage; }
            protected set
            {
                if (value != _CamCurImage)
                {
                    _CamCurImage = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _UpdateOverlayFlag = false;
        [XmlIgnore, JsonIgnore]
        public bool UpdateOverlayFlag
        {
            get { return _UpdateOverlayFlag; }
            set
            {
                if (value != _UpdateOverlayFlag)
                {
                    _UpdateOverlayFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _RemoveOverlayContextFlag = false;
        [XmlIgnore, JsonIgnore]
        public bool RemoveOverlayContextFlag
        {
            get { return _RemoveOverlayContextFlag; }
            set
            {
                if (value != _RemoveOverlayContextFlag)
                {
                    _RemoveOverlayContextFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _EnableGetFocusValueFlag = Visibility.Hidden;
        [XmlIgnore, JsonIgnore]
        public Visibility EnableGetFocusValueFlag
        {
            get { return _EnableGetFocusValueFlag; }
            set
            {
                if (value != _EnableGetFocusValueFlag)
                {
                    _EnableGetFocusValueFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ILightChannel> _LightModules;
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<ILightChannel> LightModules
        {
            get { return _LightModules; }
            private set
            {
                if (value != _LightModules)
                {
                    _LightModules = value;
                    RaisePropertyChanged();
                }
            }
        }

        public WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor()?.WaferObject;
            }
        }


        public ImageBuffer curImage;
        public readonly object _ImageGrabLock = new object();
        private readonly object _CurImageLock = new object();

        private MachineCoordinate _GrabCoord = new MachineCoordinate();
        [XmlIgnore, JsonIgnore]
        public MachineCoordinate GrabCoord
        {
            get { return _GrabCoord; }
            set
            {
                if (value != _GrabCoord)
                {
                    _PreGrabCoord = _GrabCoord;
                    _GrabCoord = value;

                    RaisePropertyChanged();
                }
            }
        }
        private MachineCoordinate _PreGrabCoord = new MachineCoordinate();
        [XmlIgnore, JsonIgnore]
        public MachineCoordinate PreGrabCoord
        {
            get { return _PreGrabCoord; }
            set
            {
                if (value != _PreGrabCoord)
                {
                    _PreGrabCoord = value;

                    RaisePropertyChanged();
                }
            }
        }
        private bool _LightValueChanged = false;
        public bool LightValueChanged
        {
            get { return _LightValueChanged; }
            set
            {
                if (value != _LightValueChanged)
                {
                    _LightValueChanged = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsMovingPos = false;

        public bool IsMovingPos
        {
            get { return _IsMovingPos; }
            set { _IsMovingPos = value; }
        }

        private List<LightValueParam> _LightBackupValues
             = new List<LightValueParam>();

        public List<LightValueParam> LightBackupValues
        {
            get { return _LightBackupValues; }
            set { _LightBackupValues = value; }
        }


        #endregion

        #region //..abstract Function
        public abstract EventCodeEnum InitModule();
        public abstract void InitGrab();
        public abstract EventCodeEnum SetLight(EnumLightType type, ushort intensity);
        public abstract int GetLight(EnumLightType type);

        public abstract int SetLightNoLUT(EnumLightType type, UInt16 intensity);
        public abstract int SetupLightLookUpTable(EnumLightType type, IntListParam lightLUT);
        public abstract int SwitchCamera();
        public abstract ImageBuffer WaitSingleShot();
        #endregion

        public void DeInitModule()
        {

        }
        public void SetDefault(EnumProberCam chntype, EnumGrabberRaft grabberType = EnumGrabberRaft.UNDIFIND, int diginum = 0, int channelnum = 0)
        {
            try
            {
                Param = new CameraParameter();
                Param.DigiNumber.Value = diginum;
                Param.ChannelNumber.Value = channelnum;
                Param.ChannelDesc.Value = "Unknown channel";
                Param.ChannelType.Value = chntype;

                Param.VerticalFlip.Value = FlipEnum.NONE;
                Param.HorizontalFlip.Value = FlipEnum.NONE;

                switch (chntype)
                {
                    case EnumProberCam.WAFER_HIGH_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.WAFER_HIGH_CAM, 0);
                        break;
                    case EnumProberCam.WAFER_LOW_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.WAFER_LOW_CAM, 1);
                        break;
                    case EnumProberCam.PIN_HIGH_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.PIN_HIGH_CAM, 2);
                        break;
                    case EnumProberCam.PIN_LOW_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.PIN_LOW_CAM, 3);
                        break;
                    case EnumProberCam.PACL6_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.PACL6_CAM, 4);
                        break;
                    case EnumProberCam.PACL8_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.PACL8_CAM, 5);
                        break;
                    case EnumProberCam.PACL12_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.PACL12_CAM, 6);
                        break;
                    case EnumProberCam.ARM_6_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.ARM_6_CAM, 7);
                        break;
                    case EnumProberCam.ARM_8_12_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.ARM_8_12_CAM, 8);
                        break;
                    case EnumProberCam.OCR1_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.OCR1_CAM, 9);
                        break;
                    case EnumProberCam.OCR2_CAM:
                        CameraChannel = new CameraChannelType(EnumProberCam.OCR2_CAM, 10);
                        break;
                    case EnumProberCam.INVALID:
                    case EnumProberCam.UNDEFINED:
                    default:
                        break;
                }

                if (grabberType == EnumGrabberRaft.MILMORPHIS || grabberType == EnumGrabberRaft.EMULGRABBER)
                {
                    Param.ChannelType.Value = chntype;
                    Param.GrabSizeX.Value = 480;
                    Param.GrabSizeY.Value = 480;
                    Param.Band.Value = 1;
                    Param.ColorDept.Value = 8;
                    switch (chntype)
                    {
                        case EnumProberCam.WAFER_HIGH_CAM:
                            Param.RatioX.Value = 0.75;
                            Param.RatioY.Value = 0.75;
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                            Param.RatioX.Value = 7.85;
                            Param.RatioY.Value = 7.85;
                            break;
                        case EnumProberCam.PIN_HIGH_CAM:
                            Param.RatioX.Value = 0.585;
                            Param.RatioY.Value = 0.585;
                            break;
                        case EnumProberCam.PIN_LOW_CAM:
                            Param.RatioX.Value = 4.1;
                            Param.RatioY.Value = 4.1;
                            break;
                    }


                }
                else if (grabberType == EnumGrabberRaft.MILGIGE)
                {
                    Param.GrabSizeX.Value = 960;
                    Param.GrabSizeY.Value = 960; //960
                    Param.Band.Value = 1;
                    Param.ColorDept.Value = 8;

                    switch (chntype)
                    {
                        case EnumProberCam.WAFER_HIGH_CAM:
                            Param.VerticalFlip.Value = FlipEnum.FLIP;
                            Param.RatioX.Value = 0.56;
                            Param.RatioY.Value = 0.56;
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                            Param.VerticalFlip.Value = FlipEnum.FLIP;
                            Param.RatioX.Value = 0.78;
                            Param.RatioY.Value = 0.78;
                            break;
                        case EnumProberCam.PIN_HIGH_CAM:
                            Param.RatioX.Value = 0.585;
                            Param.RatioY.Value = 0.585;
                            break;
                        case EnumProberCam.PIN_LOW_CAM:
                            Param.RatioX.Value = 4.1;
                            Param.RatioY.Value = 4.1;
                            break;
                    }
                }


                LightsChannels = new List<LightChannelType>
            {
                new LightChannelType(EnumLightType.COAXIAL, 0),
                new LightChannelType(EnumLightType.OBLIQUE, 1),
                new LightChannelType(EnumLightType.AUX, 2),
                new LightChannelType(EnumLightType.EXTERNAL1, 3),
                new LightChannelType(EnumLightType.EXTERNAL2, 4),
                new LightChannelType(EnumLightType.EXTERNAL3, 5),
                new LightChannelType(EnumLightType.EXTERNAL4, 6),
                new LightChannelType(EnumLightType.EXTERNAL5, 7)
            };
                InitLights();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum InitCameraChannels()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CameraChannel.SetCameraChannelDevOutput = () =>
                {
                    this.CameraChannel().SwitchCamera(Param.DigiNumber.Value, CameraChannel.Channel);
                };

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum InitLights()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LightModules = new ObservableCollection<ILightChannel>();
            try
            {
                if (this.GetContainer().IsRegistered<ILightAdmin>())
                {
                    foreach (LightChannelType light in LightsChannels)
                    {
                        light.SetLightDevOutput = (UInt16 intensity) =>
                        {
                            this.LightAdmin().SetLight(light.ChannelMapIdx.Value, intensity, CameraChannel.Type, light.Type.Value);
                        };

                        light.SetLightDevOutputNoLUT = (UInt16 grayLevel) =>
                        {
                            this.LightAdmin().SetLightNoLUT(light.ChannelMapIdx.Value, grayLevel);
                        };

                        light.SetupLUT = (IntListParam lightLUT) =>
                        {
                            this.LightAdmin().SetupLightLookUpTable(light.ChannelMapIdx.Value, lightLUT);
                        };
                        light.GetLightVal = () =>
                        {
                            return this.LightAdmin().GetLight(light.ChannelMapIdx.Value);
                        };
                        LightModules.Add(this.LightAdmin().GetLightChannel(light.ChannelMapIdx.Value));
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
        readonly Stopwatch stw = new Stopwatch();
        public ImageBuffer Grab_SingleShot()
        {
            stw.Reset();

            try
            {
                IVisionManager vm = this.VisionManager();

                if (vm.DigitizerService[Param.DigiNumber.Value].GrabberService.bContinousGrab != false)
                {
                    vm.DigitizerService[Param.DigiNumber.Value].GrabberService.StopContinuousGrab();
                }

                lock (_ImageGrabLock)
                {
                    stw.Start();
                    CamCurImage = vm.DigitizerService[Param.DigiNumber.Value].GrabberService.SingleGrab();
                    stw.Stop();
                    ImageGrabbed(CamCurImage);
                }

                long timeout = GetGrabTimeOut();

                if (stw.ElapsedMilliseconds > timeout)
                {
                    LoggerManager.Debug($"Camera Type : [{this.GetChannelType()}], Single Grab Time Out. Time:{stw.ElapsedMilliseconds} , TimeOutValue:{timeout}");
                    VisionException err = new VisionException("SingleGrab TimeOut Exception");
                    throw err;
                }

                return curImage;
            }
            catch (VisionException err)
            {
                throw err;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                stw.Stop();
            }
        }
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private bool _IsAssignGrabbedSemaphore = false;
        public virtual async void ImageGrabbed(ImageBuffer img)
        {
            double xpos = 0.0;
            double ypos = 0.0;
            double zpos = 0.0;
            double tpos = 0.0;

            try
            {
                if (img != null)
                {
                    this.MotionManager()?.GetRefPos(ref xpos, ref ypos, ref zpos, ref tpos);

                    MachineCoordinate machinecoord = new MachineCoordinate(xpos, ypos, zpos, tpos);
                    CatCoordinates camcoord = new CatCoordinates();

                    camcoord = this.CoordinateManager()?.StageCoordConvertToUserCoord(Param.ChannelType.Value);

                    _IsAssignGrabbedSemaphore = semaphore.Wait(5000);
                    if (_IsAssignGrabbedSemaphore == false)
                    {
                        LoggerManager.Debug($"[ImageGrabbed] semaphoreWaitResult value is {_IsAssignGrabbedSemaphore}, time(5 seconds) out.");
                        return;
                    }

                    img.CamType = Param.ChannelType.Value;
                    img.MachineCoordinates = machinecoord;
                    img.CatCoordinates = camcoord;
                    img.RatioX.Value = Param.RatioX.Value;
                    img.RatioY.Value = Param.RatioY.Value;
                    img.Band = Param.Band.Value;
                    img.MachineIdx = GetCurCoordMachineIndex();
                    img.UserIdx = GetCurCoordIndex();
                    img.UpdateOverlayFlag = UpdateOverlayFlag;

                    //lock (_ImageGrabLock)
                    //{

                    //}
                    img.CopyTo(curImage);

                    if (EnableGetFocusValueFlag == Visibility.Visible)
                    {
                        img.FocusLevelValue = this.VisionManager().GetFocusValue(img);
                        img.CopyTo(CamCurImage);
                    }
                    //curImage.CopyTo(CamCurImage);
                    //bool drawflag = false; 
                    if (DrawDisplayDelegate != null)
                    {
                        GrabCoord = new MachineCoordinate(
                            Math.Round(machinecoord.GetX(), 0),
                            Math.Round(machinecoord.GetY(), 0),
                            Math.Round(machinecoord.GetZ(), 0),
                            Math.Round(machinecoord.GetT(), 0));

                        //ChangedOverlayParamFlag
                        if (GrabCoord.GetX() != PreGrabCoord.GetX() ||
                            GrabCoord.GetY() != PreGrabCoord.GetY() ||
                            GrabCoord.GetZ() != PreGrabCoord.GetZ() ||
                            GrabCoord.GetT() != PreGrabCoord.GetT() || UpdateOverlayFlag || DisplayService.needUpdateOverlayCanvasSize
                                )
                        {

                            if (DrawDisplayDelegate != null)
                            {
                                await Application.Current.Dispatcher.BeginInvoke((Action) delegate
                                {
                                    try
                                    {
                                        if (DrawDisplayDelegate != null)
                                        {
                                            img.DrawOverlayContexts.Clear();

                                            DrawDisplayDelegate(img, this);
                                            DisplayService.Draw(img);
                                            UpdateOverlayFlag = true;
                                        }

                                        if (DisplayService.needUpdateOverlayCanvasSize)
                                            DisplayService.needUpdateOverlayCanvasSize = false;
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                });

                                //img.DrawOverlayContexts = new ObservableCollection<IDrawable>(DisplayService.DrawOverlayContexts);

                                img.DrawOverlayContexts = DisplayService.DrawOverlayContexts;
                            }

                            UpdateOverlayFlag = false;
                        }
                        else
                        {
                            //img.DrawOverlayContexts.Clear();
                        }
                    }
                    else
                    {
                        if (img.DrawOverlayContexts.Count != 0)
                            img.DrawOverlayContexts.Clear();
                        UpdateOverlayFlag = false;
                    }

                    //if(!drawflag)
                    DisplayService.SetImage(this, img);
                    //DisplayService.ClearOverlayCanvas();
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                try
                {
                    semaphore.Release();
                }
                catch (SemaphoreFullException ex)
                {
                    LoggerManager.Debug($"SemaphoreFullException: {ex.Message}");
                }
            }
        }
        public void ImageProcessed(ImageBuffer img)
        {

            try
            {
                if (img.Band == 3)
                    DisplayService.SetImage(this, img);
                //DisplayService.ConvArrTOWRB_BAndW_Overlay(img.Buffer, img.SizeX, img.SizeY, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void StartGrab()
        {
            try
            {
                this.VisionManager().DigitizerService[Param.DigiNumber.Value].GrabberService.ContinuousGrab();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void StopGrab()
        {
            try
            {
                if (this.VisionManager().DigitizerService.Count > _Param.DigiNumber.Value)
                {
                    this.VisionManager().DigitizerService[_Param.DigiNumber.Value]
                        .GrabberService.StopContinuousGrab();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void GetCurImage(out ImageBuffer curimg)
        {
            try
            {
                lock (_CurImageLock)
                {
                    curimg = new ImageBuffer();
                    curImage.CopyTo(curimg);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// In WaferCamera, the concept of parameters means to Index, 
        /// and PinCamera means to coordinates.
        /// </summary>
        /// <param name="MoveX"></param>
        /// <param name="MoveY"></param>
        /// <returns></returns>
        public async Task<MachineIndex> SeletedMoveToPos(int MoveX, int MoveY)
        {
            MachineIndex mindex = new MachineIndex(-99, -99);
            try
            {
                Task task = new Task(() =>
                {
                    IsMovingPos = true;

                    if (this.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        this.StageSupervisor().WaferHighViewIndexCoordMove(MoveX, MoveY);
                    }
                    else if (this.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    {
                        this.StageSupervisor().WaferLowViewIndexCoordMove(MoveX, MoveY);
                    }

                    mindex.XIndex = MoveX;
                    mindex.YIndex = MoveY;
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                IsMovingPos = false;
            }
            return mindex;
        }
    
        public CatCoordinates GetCurCoordPos()
        {
            try
            {
                CamSystemPos = this.CoordinateManager()?.StageCoordConvertToUserCoord(Param.ChannelType.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CameraBase - GetCurCoordPos() Error occured : {err.Message}");
            }
            return CamSystemPos;
        }
        public void SetCamSystemPos(CatCoordinates coord)
        {
            CamSystemPos = coord;
        }
        public void SetCamSystemUI(UserIndex uindex)
        {
            CamSystemUI = uindex;
        }
        public void SetCamSystemMI(MachineIndex mindex)
        {
            CamSystemMI = mindex;
        }
        public UserIndex GetCurCoordIndex()
        {
            try
            {
                CamSystemUI = this.CoordinateManager()?.GetCurUserIndex(GetCurCoordPos());
                return CamSystemUI;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public MachineIndex GetCurCoordMachineIndex()
        {
            try
            {
                CamSystemMI = this.CoordinateManager()?.GetCurMachineIndex(new WaferCoordinate(GetCurCoordPos().GetX(), GetCurCoordPos().GetY()));

                if (Wafer != null && Wafer.MapViewStageSyncEnable && CamSystemMI != null)
                {
                    if (GetChannelType() == Wafer.MapViewAssignCamType.Value & (IsMovingPos == false))
                    {

                        Wafer.CurrentMXIndex = CamSystemMI.XIndex;
                        Wafer.CurrentMYIndex = CamSystemMI.YIndex;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return CamSystemMI;
        }
        public NCCoordinate GetCurCoorNCCoord()
        {
            try
            {
                CamSystemNC = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CamSystemNC;
        }
        public EventCodeEnum DrawOverlayDisplay()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (DrawDisplayDelegate != null)
                {
                    DrawDisplayDelegate = null;
                }

                DrawDisplayDelegate = (ImageBuffer img, ICamera cam) =>
                {
                    DisplayService.Draw(img);
                };

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum InDrawOverlayDisplay()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (DrawDisplayDelegate != null)
                {
                    DrawDisplayDelegate = null;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DisplayService.DrawOverlayContexts.Clear();
                    DisplayService.OverlayCanvas.Children.Clear();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private void ClearDisplay()
        {
            try
            {
                if (DrawDisplayDelegate == null)
                {
                    Application.Current.Dispatcher.InvokeAsync((Action)(() =>
                    {
                        DisplayService.DrawOverlayContexts.Clear();
                        DisplayService.OverlayCanvas.Children.Clear();
                    }));
                }

                UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int GetDigitizerIndex()
        {
            return Param.DigiNumber.Value;
        }
        public EnumProberCam GetChannelType()
        {
            if (Param == null)
                return EnumProberCam.UNDEFINED;
            else
                return Param.ChannelType.Value;
        }
        public void SetVerticalFlip(FlipEnum fliptype)
        {
            Param.VerticalFlip.Value = fliptype;
        }
        public void SetHorizontalFlip(FlipEnum fliptype)
        {
            Param.HorizontalFlip.Value = fliptype;
        }
        public double GetRatioX()
        {
            return Param.RatioX.Value;
        }
        public double GetRatioY()
        {
            return Param.RatioY.Value;
        }
        public double GetGrabSizeWidth()
        {
            return Param.GrabSizeX.Value;
        }
        public double GetGrabSizeHeight()
        {
            return Param.GrabSizeY.Value;
        }
        public FlipEnum GetVerticalFlip()
        {
            return Param.VerticalFlip.Value;
        }
        public FlipEnum GetHorizontalFlip()
        {
            return Param.HorizontalFlip.Value;
        }
        public int GetGrabTimeOut()
        {
            int retVal = 0;
            try
            {
                retVal = this.VisionManager().DigitizerService[Param.DigiNumber.Value].GrabTimeOut;
                if (retVal == 0)
                {
                    retVal = 2000;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void BackupLights(bool turnoff = false)
        {
            try
            {
                if(LightsChannels != null)
                {
                    if(LightBackupValues == null)
                    {
                        LightBackupValues = new List<LightValueParam>();
                    }

                    foreach (var light in LightsChannels)
                    {
                        if (LightBackupValues.Find(value => value.Type.Value == light.Type.Value) == null)
                        {
                            ushort lightValue = (ushort)GetLight(light.Type.Value);
                            LightBackupValues.Add(new LightValueParam(light.Type.Value, lightValue));

                            LoggerManager.Debug($"BackupLights() : ChannelType : {GetChannelType()}, LightType : {light.Type.Value}, Value : {lightValue}");
                        }

                        if (turnoff)
                        {
                            SetLight(light.Type.Value, 0);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void RestoreLights()
        {
            try
            {
                if(LightBackupValues != null)
                {
                    foreach (var light in LightBackupValues)
                    {
                        SetLight(light.Type.Value, light.Value.Value);
                        
                        LoggerManager.Debug($"RestoreLights() : ChannelType : {GetChannelType()}, LightType : {light.Type.Value}, Value : {light.Value.Value}");
                    }

                    LightBackupValues.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
