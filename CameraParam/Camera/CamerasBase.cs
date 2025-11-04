//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ProberInterfaces.Enum;
//using ProberInterfaces.Error;

//namespace VisionParams.Camera
//{
//    using System.Windows;
//    using System.ComponentModel;
//    using System.Xml.Serialization;
//    using System.Windows;
//    using ProberInterfaces.Param;
//    using ProberInterfaces.Vision;
//    using ProberErrorCode;
//    using ProberInterfaces;
//    using System.Collections.ObjectModel;
//    using Newtonsoft.Json;
//    using System.Runtime.CompilerServices;
//    using LogModule;

//    [Serializable]
//    public abstract class CameraBase : ICamera
//    {
//        #region ==> PropertyChanged
//        public event PropertyChangedEventHandler PropertyChanged;

//        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
//        {
//            if (PropertyChanged != null)
//                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
//        }
//        #endregion


//        [XmlIgnore, JsonIgnore]
//        public abstract bool Initialized { get; set; }

//        [XmlIgnore, JsonIgnore]
//        public abstract ImageBuffer.DrawDisplay DrawDisplayDelegate { get; set; }


//        public abstract ICameraParameter Param { get; set; }
//        public abstract List<LightChannelType> LightsChannels { get; set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract IDisplay DisplayService { get; set; }
//        public abstract CameraChannelType CameraChannel { get; set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract CatCoordinates CamSystemPos { get; protected set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract UserIndex CamSystemUI { get; protected set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract MachineIndex CamSystemMI { get; protected set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract NCCoordinate CamSystemNC { get; protected set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract ImageBuffer CamCurImage { get; protected set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract bool UpdateOverlayFlag { get; set; }
//        [XmlIgnore, JsonIgnore]
//        public abstract bool RemoveOverlayContextFlag { get; set; }

//        [XmlIgnore, JsonIgnore]
//        public abstract Visibility EnableGetFocusValueFlag { get; set; }

//        public abstract ImageBuffer Grab_SingleShot();

//        public ImageBuffer curImage;
//        public object _ImageGrabLock = new object();

//        private MachineCoordinate _GrabCoord = new MachineCoordinate();
//        [XmlIgnore, JsonIgnore]
//        public MachineCoordinate GrabCoord
//        {
//            get { return _GrabCoord; }
//            set
//            {
//                if (value != _GrabCoord)
//                {
//                    _PreGrabCoord = _GrabCoord;
//                    _GrabCoord = value;

//                    RaisePropertyChanged();
//                }
//            }
//        }
//        private MachineCoordinate _PreGrabCoord = new MachineCoordinate();
//        [XmlIgnore, JsonIgnore]
//        public MachineCoordinate PreGrabCoord
//        {
//            get { return _PreGrabCoord; }
//            set
//            {
//                if (value != _PreGrabCoord)
//                {
//                    _PreGrabCoord = value;

//                    RaisePropertyChanged();
//                }
//            }
//        }

//        public async void ImageGrabbed(ImageBuffer img)
//        {
//            double xpos = 0.0;
//            double ypos = 0.0;
//            double zpos = 0.0;
//            double tpos = 0.0;

//            try
//            {
//                if (img != null)
//                {
//                    this.MotionManager()?.GetRefPos(ref xpos, ref ypos, ref zpos, ref tpos);

//                    MachineCoordinate machinecoord = new MachineCoordinate(xpos, ypos, zpos, tpos);
//                    CatCoordinates camcoord = new CatCoordinates();

//                    camcoord = this.CoordinateManager()?.StageCoordConvertToUserCoord(Param.ChannelType.Value);

//                    lock (_ImageGrabLock)
//                    {

//                        img.CamType = Param.ChannelType.Value;
//                        img.MachineCoordinates = machinecoord;
//                        img.CatCoordinates = camcoord;
//                        img.RatioX.Value = Param.RatioX.Value;
//                        img.RatioY.Value = Param.RatioY.Value;
//                        img.Band = Param.Band.Value;

//                        img.CopyTo(curImage);

//                        if (DrawDisplayDelegate != null)
//                        {
//                            GrabCoord = new MachineCoordinate(
//                                Math.Round(machinecoord.GetX(), 0),
//                                Math.Round(machinecoord.GetY(), 0),
//                                Math.Round(machinecoord.GetZ(), 0),
//                                Math.Round(machinecoord.GetT(), 0));

//                            //ChangedOverlayParamFlag
//                            if (GrabCoord.GetX() != PreGrabCoord.GetX() ||
//                                GrabCoord.GetY() != PreGrabCoord.GetY() ||
//                                GrabCoord.GetZ() != PreGrabCoord.GetZ() ||
//                                GrabCoord.GetT() != PreGrabCoord.GetT() || UpdateOverlayFlag
//                                    )
//                            {

//                                if (DrawDisplayDelegate != null)
//                                {
//                                    DrawDisplayDelegate(img, this);
//                                    if (UpdateOverlayFlag)
//                                    {
//                                        Application.Current.Dispatcher.Invoke(() =>
//                                        {
//                                            if (PresentationSource.FromVisual(DisplayService.OverlayCanvas) == null)
//                                            {
//                                                if (DrawDisplayDelegate != null)
//                                                    DrawDisplayDelegate(img, this);
//                                                UpdateOverlayFlag = false;
//                                            }
//                                        });
//                                    }
//                                }

//                                UpdateOverlayFlag = false;
//                            }
//                        }

//                        if (EnableGetFocusValueFlag == Visibility.Visible)
//                        {
//                            img.FocusLevelValue = this.VisionManager().GetFocusValue(img);
//                            img.CopyTo(CamCurImage);
//                        }

//                        DisplayService.SetImage(this, img);
//                    }
//                }
//                else
//                {

//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//        }
//        public abstract void ImageProcessed(ImageBuffer img);


//        public abstract void InitGrab();

//        public abstract EventCodeEnum InitLights();

//        public abstract EventCodeEnum InitModule();

//        public abstract void SetDefault(EnumProberCam chntype, EnumGrabberRaft grabberType = EnumGrabberRaft.UNDIFIND, int diginum = 0, int channelnum = 0);

//        public abstract EventCodeEnum SetLight(EnumLightType type, ushort intensity);
//        public abstract int GetLight(EnumLightType type);

//        public abstract int SetLightNoLUT(EnumLightType type, UInt16 intensity);
//        public abstract int SetupLightLookUpTable(EnumLightType type, IntListParam lightLUT);
//        public void DeInitModule()
//        {

//        }
//        public abstract void StartGrab();

//        public abstract void StopGrab();

//        public abstract int SwitchCamera();
//        public abstract void GetCurImage(out ImageBuffer curimg);

//        /// <summary>
//        /// In WaferCamera, the concept of parameters means to Index, 
//        /// and PinCamera means to coordinates.
//        /// </summary>
//        /// <param name="MoveX"></param>
//        /// <param name="MoveY"></param>
//        /// <returns></returns>

//        public async Task<MachineIndex> SeletedMoveToPos(int CurrXIndex, int CurrYIndex, int MoveX, int MoveY)
//        {
//            MachineIndex mindex = new MachineIndex(-99,-99);
//            try
//            {
//                await Task.Run(() =>
//                {
//                    var axisx = this.MotionManager().GetAxis(EnumAxisConstants.X);
//                    var axisy = this.MotionManager().GetAxis(EnumAxisConstants.Y);
//                    int offsetx = CurrXIndex - MoveX;
//                    int offsety = CurrYIndex - MoveY;
//                    double xvel = axisx.Param.Speed.Value;
//                    double xacc = axisx.Param.Acceleration.Value;
//                    double yvel = axisy.Param.Speed.Value;
//                    double yacc = axisy.Param.Acceleration.Value;

//                    MachineCoordinate mcoord = GetMoveEncoderpos(MoveX, MoveY);

//                    if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, mcoord.GetX()) != ProberErrorCode.EventCodeEnum.NONE) return;
//                    if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, mcoord.GetT()) != ProberErrorCode.EventCodeEnum.NONE) return;


//                    if (this.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
//                        this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(MoveX, MoveY);
//                    else if (this.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
//                        this.StageSupervisor().StageModuleState.WaferLowViewIndexMove(MoveX, MoveY);

//                    mindex.XIndex = MoveX;
//                    mindex.YIndex = MoveY;
//                });
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return mindex;
//        }

//        protected MachineCoordinate GetMoveEncoderpos(long machx, long machy)
//        {
//            MachineCoordinate mccoord = new MachineCoordinate();
//            WaferCoordinate wfcoord = new WaferCoordinate();
//            WaferCoordinate wfcoord_next = new WaferCoordinate();
//            WaferCoordinate wfcoord_offset = new WaferCoordinate();
//            WaferCoordinate wfcoord_LL = new WaferCoordinate();

//            double curX = 0;
//            double curY = 0;
//            double curZ = 0;
//            double curPZ = 0;
//            double sqr = 0;

//            try
//            {
//                // 현재 좌표
//                MachineIndex mcoord = this.GetCurCoordMachineIndex();
//                long x = mcoord.XIndex;
//                long y = mcoord.YIndex;

//                // 현재 위치
//                wfcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

//                // 현재 다이의 LL 위치
//                wfcoord_LL = this.WaferAligner().MachineIndexConvertToDieLeftCorenr((int)x, (int)y);

//                // 현재 다이에서 LL 까지의 거리
//                wfcoord_offset.X.Value = wfcoord.X.Value - wfcoord_LL.X.Value;
//                wfcoord_offset.Y.Value = wfcoord.Y.Value - wfcoord_LL.Y.Value;


//                // 이동할 위치
//                wfcoord_next = this.WaferAligner().MachineIndexConvertToDieLeftCorenr((int)machx, (int)machy);
//                wfcoord_next.X.Value = wfcoord_next.X.Value + wfcoord_offset.X.Value;
//                wfcoord_next.Y.Value = wfcoord_next.Y.Value + wfcoord_offset.Y.Value;

//                LoggerManager.Debug($"IndexModeTargetPos X : [{wfcoord_next.X.Value}], Y : [{wfcoord_next.Y.Value}]");

//                mccoord = this.CoordinateManager().WaferHighChuckConvert.ConvertBack(wfcoord_next);

//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return mccoord;
//        }

//        public abstract EventCodeEnum MoveToCoord(int xMove, int yMove);

//        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
//        {
//            throw new NotImplementedException();
//        }

//        public abstract CatCoordinates GetCurCoordPos();

//        public abstract UserIndex GetCurCoordIndex();
//        public abstract MachineIndex GetCurCoordMachineIndex();
//        public abstract NCCoordinate GetCurCoorNCCoord();

//        public abstract EventCodeEnum DrawOverlayDisplay();
//        public abstract EventCodeEnum InDrawOverlayDisplay();

//        public abstract ImageBuffer WaitSingleShot();

//        public int GetDigitizerIndex()
//        {
//            return Param.DigiNumber.Value;
//        }

//        public EnumProberCam GetChannelType()
//        {
//            return Param.ChannelType.Value;
//        }

//        public void SetVerticalFlip(FlipEnum fliptype)
//        {
//            Param.VerticalFlip.Value = fliptype;
//        }
//        public double GetRatioX()
//        {
//            return Param.RatioX.Value;
//        }

//        public double GetRatioY()
//        {
//            return Param.RatioY.Value;
//        }
//        public double GetGrabSizeWidth()
//        {
//            return Param.GrabSizeX.Value;
//        }
//        public double GetGrabSizeHeight()
//        {
//            return Param.GrabSizeY.Value;
//        }

//        public FlipEnum GetVerticalFlip()
//        {
//            return Param.VerticalFlip.Value;
//        }

//        public FlipEnum GetHorizontalFlip()
//        {
//            return Param.HorizontalFlip.Value;
//        }


//        private bool _LightValueChanged = false;
//        public bool LightValueChanged
//        {
//            get { return _LightValueChanged; }
//            set
//            {
//                if (value != _LightValueChanged)
//                {
//                    _LightValueChanged = value;
//                    RaisePropertyChanged();
//                }
//            }
//        }


//    }
//}
