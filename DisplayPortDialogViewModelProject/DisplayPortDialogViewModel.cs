using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.DialogControl;
using ProberInterfaces.Vision;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UcDisplayPort;
using UcDisplayPortDialogView;


namespace DisplayPortDialogVM
{
    public class DisplayPortDialogViewModel : INotifyPropertyChanged, IDisplayPortDialog
    {
        enum StageCam
        {
            WAFER_HIGH_CAM,
            WAFER_LOW_CAM,
            PIN_HIGH_CAM,
            PIN_LOW_CAM,
        }

        public bool Initialized { get; set; } = false;
        private DisplayPortDialogView DisplayPortDialogView { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _WaferAlignOnToggle = false;
        public bool WaferAlignOnToggle
        {
            get { return _WaferAlignOnToggle; }
            set
            {
                if (value != _WaferAlignOnToggle)
                {
                    _WaferAlignOnToggle = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _PinAlignOnToggle = false;
        public bool PinAlignOnToggle
        {
            get { return _PinAlignOnToggle; }
            set
            {
                if (value != _PinAlignOnToggle)
                {
                    _PinAlignOnToggle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DisplayPortDialogDone = false;
        public bool DisplayPortDialogDone
        {
            get { return _DisplayPortDialogDone; }
            set
            {
                if (value != _DisplayPortDialogDone)
                {
                    _DisplayPortDialogDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _NameTag;
        public string NameTag
        {
            get { return _NameTag; }
            set
            {
                if (value != _NameTag)
                {
                    _NameTag = value;
                    RaisePropertyChanged();
                }
            }
        }
        public IStageSupervisor StageSupervisor { get; private set; }
        protected IVisionManager VisionManager { get; private set; }

        public DisplayPortDialogViewModel()
        {
            try
            {
                VisionManager = Extensions_IModule.VisionManager(null);
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    DisplayPort = new DisplayPort() { GUID = new Guid("8E1A4A49-B8A3-4A3C-96AF-915CFBB0F8AD") };

                    Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                    if (Extensions_IModule.VisionManager(null).CameraDescriptor != null)
                    {
                        foreach (var cam in Extensions_IModule.VisionManager(null).CameraDescriptor.Cams)
                        {
                            for (int index = 0; index < stagecamvalues.Length; index++)
                            {
                                if (((StageCam)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                                {
                                    Extensions_IModule.VisionManager(null).SetDisplayChannel(cam, DisplayPort);
                                    break;
                                }
                            }
                        }
                    }
                    if (WaferAlignOnToggle == true)
                    {
                        CurCam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.WAFER_HIGH_CAM);
                    }
                    else if (PinAlignOnToggle == true)
                    {
                        CurCam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.PIN_HIGH_CAM);
                        //Extensions_IModule.VisionManager(null).StartGrab(CurCam.GetChannelType());
                    }
                    ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

                    Binding bindCamera = new Binding
                    {
                        Path = new System.Windows.PropertyPath("CurCam"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);
                }));
                //if (WaferAlignOnToggle == true)
                //{
                //    CurCam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.WAFER_HIGH_CAM);
                //}
                //else if (PinAlignOnToggle == true)
                //{
                //    CurCam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.PIN_HIGH_CAM);
                //    //Extensions_IModule.VisionManager(null).StartGrab(CurCam.GetChannelType());
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _StartBlobCommand;
        public ICommand StartBlobCommand
        {
            get
            {
                if (null == _StartBlobCommand) _StartBlobCommand = new RelayCommand(StartBlobFunc);
                return _StartBlobCommand;
            }
        }

        private void StartBlobFunc()
        {
            try
            {
                //double imgPosX = 0.0;
                //double imgPosY = 0.0;
                //VisionManager.FindBlob(EnumProberCam.WAFER_HIGH_CAM,
                //                        ref imgPosX, ref imgPosY,
                //                        127, 2500, 25000);
                ImageBuffer curImage = null;
                BlobParameter blobParam = new BlobParameter();
                ROIParameter roiParam = new ROIParameter();

                blobParam.BlobMinRadius.Value = 50;
                blobParam.BlobThreshHold.Value = 127;
                blobParam.MinBlobArea.Value = 2500;
                blobParam.MaxBlobArea.Value = 25000;
                blobParam.MAX_FERET_X.Value = 0;
                blobParam.MAX_FERET_Y.Value = 0;
                blobParam.MIN_FERET_X.Value = 0;
                blobParam.MIN_FERET_Y.Value = 0;
                CurCam.GetCurImage(out curImage);

                roiParam.OffsetX.Value = 0;
                roiParam.OffsetY.Value = 0;
                roiParam.Width.Value = 0;
                roiParam.OffsetY.Value = 0;

                //VisionManager.VisionProcessing.Algorithmes.FindBlobObject(
                //    curImage, blobParam, )
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    StageSupervisor = this.StageSupervisor();
                    DisplayPortDialogView = new DisplayPortDialogView();
                    DisplayPortDialogView.DataContext = this;

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
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Error(err + "InitModule() : Error occured");
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private bool IsNomalEnd = false;
        private bool _IsShowing = false;
        public bool IsShowing
        {
            get { return _IsShowing; }
            set
            {
                _IsShowing = value;
            }
        }

        public async Task<bool> ShowDialog()
        {

            EnumMessageDialogResult ret = EnumMessageDialogResult.UNDEFIND;

            try
            {
                if (WaferAlignOnToggle == true)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Alignment", "Are you sure you want to Wafer alignment?", EnumMessageStyle.AffirmativeAndNegative);
                }
                else if (PinAlignOnToggle == true)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("Pin Alignment", "Are you sure you want to Pin alignment?", EnumMessageStyle.AffirmativeAndNegative);
                }
                else
                {
                    LoggerManager.Error("Unknown process");
                }

                DisplayPortDialogDone = false;

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (!IsShowing)
                    {
                        IsNomalEnd = true;

                        EventCodeEnum result = EventCodeEnum.UNDEFINED;

                        IsShowing = true;

                        if (WaferAlignOnToggle == true)
                        {
                            CurCam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        }
                        else if (PinAlignOnToggle == true)
                        {
                            CurCam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.PIN_HIGH_CAM);
                        }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Binding bindCamera = new Binding
                            {
                                Path = new System.Windows.PropertyPath("CurCam"),
                                Mode = BindingMode.TwoWay,
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                            };
                            BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);
                            DisplayPort.EnalbeClickToMove = false;
                        });
                        this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                        //웨이퍼 얼라인
                        if (WaferAlignOnToggle == true)
                        {
                            this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                            NameTag = "Wafer";
                            this.StageSupervisor().StageModuleState.ZCLEARED();

                            await this.MetroDialogManager().ShowWindow(DisplayPortDialogView, false);
                            ICamera curcam = CurCam;
                            List<LightValueParam> lights = new List<LightValueParam>();
                            foreach (var light in curcam.LightsChannels)
                            {
                                lights.Add(new LightValueParam(light.Type.Value, (ushort)curcam.GetLight(light.Type.Value)));
                            }
                            
                            this.WaferAligner().ClearState();
                            this.WaferAligner().DoManualOperation();

                            this.VisionManager().StartGrab(curcam.GetChannelType(), this);

                            result = this.WaferAligner().SetTeachDevice(isMoving:false);

                            foreach (var light in lights)
                            {
                                curcam.SetLight(light.Type.Value, light.Value.Value);

                            }

                            var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                            this.StageSupervisor().StageModuleState.ZCLEARED();
                            this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                            WaferAlignOnToggle = false;

                            //if(this.WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                            //{
                            //    this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.DONE);
                            //}

                            await CloseDialog();
                        }
                        //
                        // 핀얼라인
                        else if (PinAlignOnToggle == true)
                        {
                            NameTag = "Pin";
                            
                            await this.MetroDialogManager().ShowWindow(DisplayPortDialogView, false);

                            ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Are you sure you want to pin alignment?", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                this.StageSupervisor().DoManualPinAlign(false);
                            }

                            //this.StageSupervisor().StageModuleState.ZCLEARED();
                            //ICamera curcam = CurCam;
                            //List<LightValueParam> lights = new List<LightValueParam>();
                            //foreach (var light in curcam.LightsChannels)
                            //{
                            //    lights.Add(new LightValueParam(light.Type.Value, (ushort)curcam.GetLight(light.Type.Value)));
                            //}
                            //this.PinAligner().ClearState();
                            //this.PinAligner().DoPinAlignProcess();
                            //this.VisionManager().StartGrab(curcam.GetChannelType());
                            //foreach (var light in lights)
                            //{
                            //    curcam.SetLight(light.Type.Value, light.Value.Value);
                            //}
                            //this.StageSupervisor().StageModuleState.ZCLEARED();

                            PinAlignOnToggle = false;

                            await CloseDialog();
                        }
                        //
                        else
                        {

                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "ShowDialog() : Error occured.");
            }
            finally
            {
                await CloseDialog();
            }

            return IsNomalEnd;
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
        public Task CloseDialog()
        {
            try
            {
                 System.Threading.Thread.Sleep(1000);

                //if (IsShowing)
                {
                    //var ii = DisplayPortDialogView.Resources.MergedDictionaries.AsEnumerable();
                    //System.Windows.Media.Animation.DoubleAnimation animation = 
                    //    new System.Windows.Media.Animation.DoubleAnimation(newWidth, duration);


                    //System.Windows.Media.Animation.DoubleAnimation animation = 
                    //    new System.Windows.Media.Animation.DoubleAnimation(150, TimeSpan.FromSeconds(1));

                    //DisplayPortDialogView.Resources["DialogCloseStoryboard"] = animation;

                    //var item = DisplayPortDialogView.Resources["DialogCloseStoryboard"];

                    //await DisplayPortDialogView._WaitForCloseAsync();
                    var mainScreenView = this.ViewModelManager().MainScreenView;
                    var mainScreenVM = this.ViewModelManager().FindViewModelObject(mainScreenView.ScreenGUID);

                    this.MetroDialogManager().CloseWindow(DisplayPortDialogView);

                    mainScreenVM?.PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "CloseDialog() : Error occured.");
            }
            IsShowing = false;

            return Task.CompletedTask;
        }

    }
}
