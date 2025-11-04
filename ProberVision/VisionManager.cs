using System;
using System.Collections.Generic;
using System.Linq;
using ProberInterfaces.Param;

namespace ProberVision
{
    using DigitizerModule;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.IO;
    using CameraModule;
    using System.Collections.ObjectModel;
    using System.Windows.Media.Imaging;
    using System.Windows;
    using ProberInterfaces.Vision;
    using Vision.ProcessingModule;
    using ProcessingModule;
    using Utils.MilSystem;
    using Vision.GrabberModule;
    using RelayCommandBase;
    using System.Windows.Input;
    using ProberErrorCode;
    using VisionParams;
    using VisionParams.Camera;
    using SystemExceptions.VisionException;
    using LogModule;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using System.Diagnostics;
    using Newtonsoft.Json;
    //using ProberInterfaces.ThreadSync;
    using ProberInterfaces.Command.Internal;
    using System.Reflection;
    using System.Security.AccessControl;
    using System.Windows.Shapes;
    using System.Threading;
    using Autofac.Features.Indexed;
    using System.Threading.Tasks;
    using System.IO.Compression;
    using Path = System.IO.Path;

    public class VisionManager : INotifyPropertyChanged, IVisionManager, IParamNode
    {
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public List<object> Nodes { get; set; }

        public bool Initialized { get; set; } = false;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private WriteableBitmap _WrbDispImage;
        public WriteableBitmap WrbDispImage
        {
            get { return _WrbDispImage; }
            set
            {
                if (value != _WrbDispImage)
                {
                    _WrbDispImage = value;
                    // Call OnPropertyChanged whenever the property is updated
                    RaisePropertyChanged();

                }
            }
        }

        #region //..property
        //private Autofac.IContainer Container { get; set; }

        private ObservableCollection<IDigitizer> _DigitizerService = new ObservableCollection<IDigitizer>();
        public ObservableCollection<IDigitizer> DigitizerService
        {
            get { return _DigitizerService; }
            set
            {
                if (value != _DigitizerService)
                {
                    _DigitizerService = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMil _Mil;

        public IMil Mil
        {
            get { return _Mil; }
            set { _Mil = value; }
        }

        private int _MilSystemNumber;

        public int MilSystemNumber
        {
            get { return _MilSystemNumber; }
            set { _MilSystemNumber = value; }
        }

        private IVisionProcessing _VisionProcessing;

        public IVisionProcessing VisionProcessing
        {
            get { return _VisionProcessing; }
            set { _VisionProcessing = value; }
        }

        private bool enableImageBufferToTextFile;

        public bool EnableImageBufferToTextFile
        {
            get { return enableImageBufferToTextFile; }
            set { enableImageBufferToTextFile = value; }
        }

        public ProberInterfaces.VisionFramework.IVisionLib VisionLib { get; set; }

        //public ImageBuffer DisplayImage;

        private Dictionary<EnumProberCam, CameraBase> _DictCamera
            = new Dictionary<EnumProberCam, CameraBase>();

        public Dictionary<EnumProberCam, CameraBase> DictCamera
        {
            get { return _DictCamera; }
            private set { _DictCamera = value; }
        }

        private CameraDescriptor _CamDescriptor;

        public CameraDescriptor CamDescriptor
        {
            get { return _CamDescriptor; }
            set
            {
                if (_CamDescriptor == value) return;
                _CamDescriptor = value;
                RaisePropertyChanged();
            }
        }


        public ICameraDescriptor CameraDescriptor { get; set; }


        private VisionProcessingParameter _visionProcParam;
        [ParamIgnore]
        public VisionProcessingParameter visionProcParam
        {
            get { return _visionProcParam; }
            set
            {
                if (value != _visionProcParam)
                {
                    _visionProcParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private VisionProcessingParameter visionProcParam;

        private VisionDigiParameters _Digiparams;
        [ParamIgnore]
        public VisionDigiParameters Digiparams
        {
            get { return _Digiparams; }
            set
            {
                if (value != _Digiparams)
                {
                    _Digiparams = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private VisionDigiParameters Digiparams;

        private PatternRegisterStates _PRState;

        public PatternRegisterStates PRState
        {
            get { return _PRState; }
            set { _PRState = value; }
        }



        private ICamera _CurCamera = null;

        public ICamera CurCamera
        {
            get { return _CurCamera; }
            private set { _CurCamera = value; }
        }

        private List<CameraBase> _Camaraes = new List<CameraBase>();
        public List<CameraBase> Cameraes
        {
            get { return _Camaraes; }
        }

        private Camera preCam = new Camera();


        private int _CurDigNum = -1;


        private ProberImageController proberImageController { get; set; }

        public ImageSaveFilter imageSaveFilter { get; set; }

        #endregion


        #region //..Command

        //private RelayCommand<object> _DragDropImageFileCommand;
        //public ICommand DragDropImageFileCommand
        //{
        //    get
        //    {
        //        if (null == _DragDropImageFileCommand) _DragDropImageFileCommand
        //                = new RelayCommand<object>(DragDropImageFile);
        //        return _DragDropImageFileCommand;
        //    }
        //}


        //public UIElementDropBehavior uIElementDropBehavior;

        #endregion


        public DispFlipEnum GetDispHorFlip()
        {
            return visionProcParam.DisplayHorFlip.Value;
        }
        public DispFlipEnum GetDispVerFlip()
        {
            return visionProcParam.DisplayVerFlip.Value;
        }

        public void SetDispHorFlip(DispFlipEnum value)
        {
            if (visionProcParam.DisplayHorFlip.Value != value)
            {
                LoggerManager.Debug($"DisplayHorFlip: {visionProcParam.DisplayHorFlip.Value} => {value}");
                visionProcParam.DisplayHorFlip.Value = value;
                this.SaveParameter(visionProcParam);
            }
        }

        public void SetDispVerFlip(DispFlipEnum value)
        {
            if (visionProcParam.DisplayVerFlip.Value != value)
            {
                LoggerManager.Debug($"DisplayVerFlip: {visionProcParam.DisplayVerFlip.Value} => {value}");
                visionProcParam.DisplayVerFlip.Value = value;
                this.SaveParameter(visionProcParam);
            }
        }


        public EventCodeEnum LoadVisionDigiParameters()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(VisionDigiParameters));
                if (RetVal == EventCodeEnum.NONE)
                {
                    Digiparams = tmpParam as VisionDigiParameters;
                }

                //DigiParameter = Digiparams;
                LoadDigitizer();

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($"[VisionManager] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadVisionProcessingParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                visionProcParam = new VisionProcessingParameter();
                string FullPath = this.FileManager().GetSystemParamFullPath(visionProcParam.FilePath, visionProcParam.FileName);

                try
                {
                    IParam tmpParam = null;
                    RetVal = this.LoadParameter(ref tmpParam, typeof(VisionProcessingParameter));
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        visionProcParam = tmpParam as VisionProcessingParameter;
                    }

                    FullPath = this.FileManager().GetSystemParamFullPath(visionProcParam.FilePath, visionProcParam.PMRecultPath + "\\");

                    if (Directory.Exists(System.IO.Path.GetDirectoryName(FullPath)) == false)
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath));
                    }

                    InitProcessing();

                    RetVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[VisionManager] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadDigitizer()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                string RootPath = this.FileManager().FileManagerParam.SystemParamRootDirectory;
                string FilePath = "\\" + Digiparams.FilePath;

                RootPath = RootPath + FilePath;

                for (int index = 0; index < Digiparams.ParamList.Count; index++)
                {
                    DigitizerParameter digitizerParameter = Digiparams.ParamList[index];

                    ModuleVisionDigitizer moduleVisionDigitizer = new ModuleVisionDigitizer(digitizerParameter.GrabRaft.Value, digitizerParameter.DigitizerName.Value);

                    DigitizerService.Add(moduleVisionDigitizer);

                    InitDigitizer(digitizerParameter.GrabRaft.Value, digitizerParameter.DigitizerName.Value, RootPath + digitizerParameter.DCF.Value);

                    DigitizerService[index].GrabberService.GrabMode = digitizerParameter.EmulGrabMode.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum InitDigitizer(EnumGrabberRaft graberRaft, string digitizername, string dcfFullpath)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                int digitizer = 0;

                IDigitizer digitizerService = DigitizerService.SingleOrDefault(service => service.DigitizerName.Equals(digitizername));
                int index = Digiparams.ParamList.ToList<DigitizerParameter>().FindIndex(param => param.DigitizerName.Value.Equals(digitizername));

                if (digitizerService != null & index != -1)
                {
                    switch (graberRaft)
                    {
                        case EnumGrabberRaft.MILMORPHIS:
                            digitizer = digitizerService.InitDigitizer(Mil.GetMilSystem(graberRaft), int.Parse(digitizername), dcfFullpath);
                            digitizerService.GrabberService.InitGrabber(Mil.GetMilSystem(graberRaft), digitizer, index);
                            break;
                        case EnumGrabberRaft.MILGIGE:
                            digitizer = digitizerService.InitDigitizer(Mil.GetMilSystem(graberRaft), digitizername, dcfFullpath);
                            digitizerService.GrabberService.InitGrabber(Mil.GetMilSystem(graberRaft), digitizer, index);
                            break;
                        case EnumGrabberRaft.USB:
                            digitizer = int.Parse(digitizername);
                            digitizerService.GrabberService.InitGrabber(Mil.GetMilSystem(graberRaft), digitizer, index);
                            break;
                        case EnumGrabberRaft.EMULGRABBER:
                            digitizer = digitizerService.InitDigitizer(Mil.GetMilSystem(graberRaft), index, dcfFullpath);
                            digitizerService.GrabberService.InitGrabber(Mil.GetMilSystem(graberRaft), digitizer, index);
                            break;
                        case EnumGrabberRaft.GIGE_EMULGRABBER:
                            digitizer = digitizerService.InitDigitizer(Mil.GetMilSystem(graberRaft), index, dcfFullpath);
                            digitizerService.GrabberService.InitGrabber(Mil.GetMilSystem(graberRaft), digitizer, index);
                            break;
                        case EnumGrabberRaft.SIMUL_GRABBER:
                            digitizer = digitizerService.InitDigitizer(Mil.GetMilSystem(graberRaft), index, dcfFullpath);
                            digitizerService.GrabberService.InitGrabber(Mil.GetMilSystem(graberRaft), digitizer, index);
                            break;
                        case EnumGrabberRaft.TIS:
                            int camSer;
                            if (int.TryParse(digitizername, out camSer))
                            {
                                digitizer = digitizerService.InitDigitizer(Mil.GetMilSystem(graberRaft), index, null);
                                DigitizerService[DigitizerService.Count - 1].GrabberService.InitGrabber(index, camSer, index);
                            }

                            break;

                    }
                    DigitizerService[index].GrabberService.GrabMode = Digiparams.ParamList[index].EmulGrabMode.Value;
                    DigitizerService[index].GrabTimeOut = Digiparams.ParamList[index].GrabTimeOut.Value;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitCameraModules()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                _Camaraes.Clear();
                CamDescriptor.Cams = null;
                _DictCamera.Clear();

                for (int index = 0; index < CamDescriptor.CameraParams.Count; index++)
                {
                    CameraParameter cameraParameter = CamDescriptor.CameraParams[index];

                    if (Digiparams.ParamList.Count > cameraParameter.DigiNumber.Value)
                    {
                        CameraBase camera = null;

                        if (Digiparams.ParamList[cameraParameter.DigiNumber.Value].GrabRaft.Value != EnumGrabberRaft.EMULGRABBER &&
                            Digiparams.ParamList[cameraParameter.DigiNumber.Value].GrabRaft.Value != EnumGrabberRaft.GIGE_EMULGRABBER &&
                            Digiparams.ParamList[cameraParameter.DigiNumber.Value].GrabRaft.Value != EnumGrabberRaft.SIMUL_GRABBER)
                        {
                            camera = new Camera(cameraParameter);
                        }
                        else if (Digiparams.ParamList[cameraParameter.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.SIMUL_GRABBER)
                        {
                            camera = new CameraSimul(cameraParameter);
                        }
                        else
                        {
                            camera = new CameraEmul(cameraParameter);
                        }

                        if (camera != null)
                        {
                            _Camaraes.Add(camera);

                            camera.InitModule();

                            if (CamDescriptor.Cams == null)
                            {
                                CamDescriptor.Cams = new ObservableCollection<ICamera>();
                            }

                            CamDescriptor.Cams.Add(camera);
                            _DictCamera.Add(camera.Param.ChannelType.Value, camera);
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[VisionManager] InitCameraModules() : Digiparams.ParamList count = {Digiparams.ParamList.Count}, DigiNumber of camera value is {cameraParameter.DigiNumber.Value}, Index in range = [{0}-{Digiparams.ParamList.Count - 1}]");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum AllocateCamera(EnumProberCam camtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                int digiIndex = GetCam(camtype).GetDigitizerIndex();

                if (DigitizerService.Count > digiIndex)
                {
                    var dService = DigitizerService[digiIndex];

                    if (dService != null)
                    {
                        int digitizer = dService.InitDigitizer();
                        dService.GrabberService.InitGrabber(Mil.GetMilSystem(Digiparams.ParamList[digiIndex].GrabRaft.Value), digitizer, digiIndex);
                        SettingGrab(GetCam(camtype).Param);
                    }
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum DeAllocateCamera(EnumProberCam camtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int digiIndex = GetCam(camtype).GetDigitizerIndex();
                if (DigitizerService.Count > digiIndex)
                {
                    var dService = DigitizerService[digiIndex];
                    if (dService != null)
                    {
                        dService.DeInitDigitizer();
                    }
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                // TODO : 
                IParam tmpParam = null;
                tmpParam = new VisionProcessingParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(VisionProcessingParameter));


                if (RetVal == EventCodeEnum.NONE)
                {
                    visionProcParam = tmpParam as VisionProcessingParameter;
                }
                tmpParam = new VisionDigiParameters();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(VisionDigiParameters));

                if (RetVal == EventCodeEnum.NONE)
                {
                    Digiparams = tmpParam as VisionDigiParameters;
                }

                tmpParam = new CameraDescriptor();

                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CameraDescriptor));

                if (RetVal == EventCodeEnum.NONE)
                {
                    CamDescriptor = tmpParam as CameraDescriptor;
                }

                //RetVal = LoadVisionProcessingParameter();
                //RetVal = LoadVisionDigiParameters();
                //RetVal = LoadCameraDescriptor();


                //..Camera
                _DictCamera = new Dictionary<EnumProberCam, CameraBase>();

                CameraDescriptor = CamDescriptor;


            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"LoadSysParameter() : Error occurred.");
            }
            return RetVal;
        }



        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = this.SaveParameter(visionProcParam);
                RetVal = this.SaveParameter(Digiparams);
                RetVal = this.SaveParameter(CamDescriptor);
                //RetVal = this.CameraChannel().LoadSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {

                    // Processing
                    InitProcessing();
                    //===========         
                    // Digitizer
                    LoadDigitizer();

                    // Camera
                    InitCameraModules();

                    if (proberImageController == null)
                    {
                        proberImageController = new ProberImageController();
                    }

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

                foreach (IDigitizer item in DigitizerService)
                {
                    item.ServiceDispose();
                }

                foreach (IDigitizer item in DigitizerService)
                {
                    item.Dispose();
                }

                Mil.Dispose();

                VisionProcessing.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CheckDigitizer()
        {
            try
            {
                foreach (var digitizer in DigitizerService)
                {
                    //digitizer.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ICamera GetCam(EnumProberCam type)
        {
            CameraBase cam = null;
            try
            {

                if (DictCamera.Count <= 0)
                {
                    LoggerManager.Debug($"Exist Cam . Get Camera");
                    return cam;
                }

                if (DictCamera.TryGetValue(type, out cam) == false)
                {
                    if (cam != null)
                    {
                        return cam;
                    }
                    else
                    {
                        throw new Exception(string.Format("Such camera type not found."));
                    }

                    //throw new Exception(string.Format("Such camera type not found."));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return cam;
        }

        public List<ICamera> GetCameras()
        {
            if (Cameraes != null)
                return Cameraes.ToList<ICamera>();
            else
                return new List<ICamera>();
        }

        private void InitDIgiService()
        {
            try
            {
                //GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.OBLIQUE, 0);
                ((Camera)GetCam(EnumProberCam.WAFER_HIGH_CAM)).GrabCont();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void InitProcessing()
        {
            try
            {
                if (Mil == null)
                    Mil = new Mil();

                if (visionProcParam.ProcRaft.Value == EnumVisionProcRaft.MIL)
                {
                    VisionProcessing = new ModuleVisionProcessing();
                    ((ModuleVisionProcessing)VisionProcessing).VisionProcParam = visionProcParam;
                    VisionProcessing.InitProcessing(Mil.GetMilSystem(visionProcParam.ProcRaft.Value));
                }
                else if (visionProcParam.ProcRaft.Value == EnumVisionProcRaft.EMUL)
                {
                    VisionProcessing = new VisionProcessingEmul();
                    ((VisionProcessingEmul)VisionProcessing).VisionProcParam = visionProcParam;
                    VisionProcessing.InitProcessing(Mil.GetMilSystem(visionProcParam.ProcRaft.Value));
                }

                VisionLib = new VisionFrameworkLib();
                //VisionProcessing.SetContainer(Container);
                VisionProcessing.InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EnumVisionProcRaft GetVisionProcRaft()
        {
            return visionProcParam.ProcRaft.Value;
        }


        /// <summary>
        /// Viewmodel 에서 쓸 disport 와 camtype 을 미리 셋팅.
        /// Displayport 와 camtype 이 여러개인 경우 같은 인덱스 셋팅은 맞춰줘야한다.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum InitDisplayPort(IMainScreenViewModel viewmodel, IDisplayPort[] port, EnumProberCamType[] camtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetDisplayChannelStageCameras(IDisplayPort port)
        {
            try
            {
                Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                for (int index = 0; index < stagecamvalues.Length; index++)
                {
                    ICamera camera =
                         GetCameras().Find(cam => cam.GetChannelType().ToString() == stagecamvalues.GetValue(index).ToString());

                    if (camera != null)
                        SetDisplayChannel(camera, port);


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetDisplyChannelLoaderCameras(IDisplayPort port)
        {
            try
            {
                Array stagecamvalues = Enum.GetValues(typeof(LoaderCam));
                for (int index = 0; index < stagecamvalues.Length; index++)
                {
                    ICamera camera =
                         GetCameras().Find(cam => cam.GetChannelType().ToString() == stagecamvalues.GetValue(index).ToString());
                    if (camera != null)
                        SetDisplayChannel(camera, port);


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// 현재 페이지에서 사용될 displayport 만 등록.
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="port"></param>
        public void SetDisplayChannel(ICamera cam, IDisplayPort port)
        {
            try
            {
                cam.DisplayService.DispPorts.Clear();
                cam.DisplayService.DispPorts.Add(port);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "SetDisplayChannel() : Error occurred.");
                LoggerManager.Exception(err);
            }
        }

        public void SettingGrab(ICameraParameter camparam)
        {
            try
            {
                DigitizerService[camparam.DigiNumber.Value].GrabberService.SettingGrabber(DigitizerService[camparam.DigiNumber.Value].GetGrabberRaft(), camparam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void SettingGrab(EnumProberCam camtype)
        {
            try
            {
                ICamera cam = GetCam(camtype);

                EnumGrabberRaft grabberRaft = DigitizerService[cam.GetDigitizerIndex()].GetGrabberRaft();
                switch (grabberRaft)
                {
                    case EnumGrabberRaft.MILMORPHIS:
                        ((ModuleVisionGrabber)DigitizerService[cam.GetDigitizerIndex()].GrabberService).CamParam = (CameraParameter)cam.Param;
                        break;
                    case EnumGrabberRaft.MILGIGE:
                        ((ModuleVisionGrabber)DigitizerService[cam.GetDigitizerIndex()].GrabberService).CamParam = (CameraParameter)cam.Param;
                        break;
                    case EnumGrabberRaft.USB:
                        ((USBVisionGrabber)DigitizerService[cam.GetDigitizerIndex()].GrabberService).CamParam = (CameraParameter)cam.Param;
                        break;
                    case EnumGrabberRaft.TIS:
                        ((TISGrabber)DigitizerService[cam.GetDigitizerIndex()].GrabberService).CamParam = (CameraParameter)cam.Param;
                        break;
                    case EnumGrabberRaft.SIMUL_GRABBER:
                        ((VisionGrabberSimul)DigitizerService[cam.GetDigitizerIndex()].GrabberService).CamParam = (CameraParameter)cam.Param;
                        break;
                    default:
                        LoggerManager.Error($"Undefined grabber: {grabberRaft}");
                        break;
                }

                DigitizerService[cam.GetDigitizerIndex()].GrabberService.SettingGrabber(
                    Digiparams.ParamList[cam.GetDigitizerIndex()].GrabRaft.Value,
                    GetCam(camtype).Param);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

        }
        public void SetCallerbySwitchCamera(ICameraParameter camparam, object caller)
        {
            try
            {
                DigitizerService[camparam.DigiNumber.Value].GrabberService.SetCaller(caller);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void SetCaller(EnumProberCam cam, object caller)
        {
            try
            {
                ICamera camera = GetCam(cam);
                CameraParameter camparam = null;

                if (Digiparams.ParamList[camera.GetDigitizerIndex()].GrabRaft.Value == EnumGrabberRaft.EMULGRABBER ||
                    Digiparams.ParamList[camera.GetDigitizerIndex()].GrabRaft.Value == EnumGrabberRaft.GIGE_EMULGRABBER ||
                    Digiparams.ParamList[camera.GetDigitizerIndex()].GrabRaft.Value == EnumGrabberRaft.SIMUL_GRABBER)
                {
                    camparam = (CameraParameter)camera.Param;
                }
                else
                {
                    camparam = (CameraParameter)((Camera)camera).Param;
                }

                DigitizerService[camparam.DigiNumber.Value].GrabberService.SetCaller(caller);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public EventCodeEnum StartGrab(EnumProberCam type, object caller)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                CameraBase camera;
                camera = (CameraBase)GetCam(type);

                LoggerManager.Debug($"[{this.GetType().Name}], StartGrab() : Caller = {caller}");

                lock (camera)
                {
                    SwitchCamera(camera.Param, caller);

                    if (DigitizerService[camera.Param.DigiNumber.Value].GrabberService.bContinousGrab == false)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], StartGrab() : DigitizerName = {DigitizerService[camera.Param.DigiNumber.Value].DigitizerName}");

                        DigitizerService[camera.Param.DigiNumber.Value].PassageGrab();

                        DigitizerService[camera.Param.DigiNumber.Value].ImageReady =
                            (ImageBuffer image) =>
                            {
                                camera.ImageGrabbed(image);
                            };

                        camera.StartGrab();
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum StopGrab(EnumProberCam type)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //LoggerManager.Debug($"[{this.GetType().Name}], StopGrab() : CamType = {type}");

                (GetCam(type) as CameraBase).StopGrab();
                ClearGrabberUserImage(type);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Crop(ImageBuffer SourceImg, ref ImageBuffer ResultImg, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                VisionProcessing.Mil_Crop(SourceImg, ref ResultImg, OffsetX, OffsetY, SizeX, SizeY);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum Binarize(EnumProberCam camtype, ref ImageBuffer RelsultImg, int Threshold, int OffsetX, int OffsetY, int SizeX, int SizeY)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (DigitizerService[0].GrabberService.GrabMode == EnumGrabberMode.AUTO)
                {
                    RelsultImg = new ImageBuffer(SingleGrab(camtype, this));
                }
                else
                {
                    RelsultImg = new ImageBuffer(SingleGrab(camtype, this));
                }

                //ProcessImg                
                VisionProcessing.Mil_Binarize(RelsultImg, ref RelsultImg, Threshold, OffsetX, OffsetY, SizeX, SizeY);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "StopGrab() Error occurred.");
                LoggerManager.Exception(err);

                throw err;
            }
            return retVal;
        }
        public ImageBuffer SingleGrab(EnumProberCam type, object assembly)
        {
            CameraBase camera;
            ImageBuffer grabbedImage = null;

            try
            {
                camera = (CameraBase)GetCam(type);

                if (camera != null)
                {
                    lock (camera)
                    {
                        SwitchCamera(camera.Param, assembly);

                        grabbedImage = new ImageBuffer();

                        try
                        {
                            camera.Grab_SingleShot().CopyTo(grabbedImage);
                        }
                        catch (VisionException visionerr)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, visionerr);
                            long timeout = camera.GetGrabTimeOut();
                            Exception err = new Exception("SingleGrab TimeOut Exception");
                            this.MetroDialogManager().ShowMessageDialog("TimeOut Message",
                               $"Camera Type : [{type}], Single Grab Time Out. TimeOutValue:{timeout}",
                               MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                this.LotOPModule().ModuleStopFlag = true;
                                this.LotOPModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.VISION_EXCEPTION,
                                        "Pause by camera error.", this.GetType().Name);
                                this.LotOPModule().PauseSourceEvent = this.LotOPModule().ReasonOfError.GetLastEventCode();
                                this.CommandManager().SetCommand<ILotOpPause>(this);
                            }
                            throw err;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            this.MetroDialogManager().ShowMessageDialog("Error Message",
                                 $"Camera Type : [{type}], Camera error has occurred. \r Please check the camera connection status.",
                                 MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                this.LotOPModule().ModuleStopFlag = true;
                                this.LotOPModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.VISION_EXCEPTION,
                                        "Pause by camera error.", this.GetType().Name);
                                this.LotOPModule().PauseSourceEvent = this.LotOPModule().ReasonOfError.GetLastEventCode();
                                this.CommandManager().SetCommand<ILotOpPause>(this);
                            }
                            throw err;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return grabbedImage;
        }
        public ImageBuffer Line_Equalization(ImageBuffer img, int Cpos)
        {
            ImageBuffer retImg = new ImageBuffer();
            try
            {
                retImg = VisionProcessing.Line_Equalization_Processing(img, Cpos);
                if (retImg == null)
                {
                    LoggerManager.Debug($"Line_Equalization(): retImg is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retImg;
        }
        public EventCodeEnum SwitchCamera(ICameraParameter camparam, object caller, Type declaringtype = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            CameraBase camera = null;

            try
            {
                lock (this)
                {
                    if (DigitizerService[camparam.DigiNumber.Value].CurCamera == null)
                    {
                        DigitizerService[camparam.DigiNumber.Value].CurCamera = new Camera();
                    }

                    if (Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.MILMORPHIS ||
                        Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.EMULGRABBER
                        )
                    {
                        for (int index = 0; index < Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value.Count; index++)
                        {
                            if (Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index] != camparam.DigiNumber.Value
                                && DigitizerService[Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index]].GrabberService.bContinousGrab != false)
                            {
                                DigitizerService[Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index]].GrabberService.StopContinuousGrab();
                            }
                        }

                        if (DigitizerService[camparam.DigiNumber.Value].CurCamera.GetChannelType() != camparam.ChannelType.Value)
                        {
                            if (DigitizerService[camparam.DigiNumber.Value].CurCamera.GetChannelType() != EnumProberCam.UNDEFINED)
                            {
                                camera = (CameraBase)GetCam(DigitizerService[camparam.DigiNumber.Value].CurCamera.GetChannelType());
                                camera.StopGrab();
                            }

                            camera = _Camaraes.Find(cam => cam.Param.ChannelType == camparam.ChannelType);
                        }

                    }
                    else if (Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.MILGIGE ||
                             Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.GIGE_EMULGRABBER ||
                             Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.SIMUL_GRABBER)
                    {
                        if (_CurDigNum == camparam.DigiNumber.Value)
                        {
                            return EventCodeEnum.UNDEFINED;
                        }

                        for (int index = 0; index < Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value.Count; index++)
                        {
                            if (Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index] != camparam.DigiNumber.Value &&
                                DigitizerService[Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index]].GrabberService.bContinousGrab != false)
                            {
                                DigitizerService[Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index]].GrabberService.StopContinuousGrab();
                            }
                        }

                        camera = _Camaraes.Find(cam => cam.Param.ChannelType == camparam.ChannelType);
                    }
                    else if (Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.USB)
                    {
                        for (int i = 0; i < Digiparams.ParamList.Count; i++)
                        {

                            if (i != camparam.DigiNumber.Value && DigitizerService[i].CurCamera.GetChannelType() != EnumProberCam.UNDEFINED
                                && Digiparams.ParamList[i].GrabRaft.Value == EnumGrabberRaft.USB)
                            {
                                camera = (CameraBase)GetCam(DigitizerService[i].CurCamera.GetChannelType());
                                camera.StopGrab();
                            }
                        }

                        //USB는 필요없음.
                        camera = _Camaraes.Find(cam => cam.Param.ChannelType == camparam.ChannelType);
                    }
                    else if (Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.TIS)
                    {
                        for (int index = 0; index < Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value.Count; index++)
                        {
                            if (Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index] != camparam.DigiNumber.Value
                                && DigitizerService[Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index]].GrabberService.bContinousGrab != false)
                            {
                                DigitizerService[Digiparams.DigitizerGroups[Digiparams.ParamList[camparam.DigiNumber.Value].DigiGroup.Value].DigiGroup.Value[index]].GrabberService.StopContinuousGrab();
                            }
                        }

                        camera = _Camaraes.Find(cam => cam.Param.ChannelType == camparam.ChannelType);
                    }

                    if (camera != null)
                    {
                        if (Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.SIMUL_GRABBER)
                        {
                            if (DigitizerService[camparam.DigiNumber.Value].CurCamera.GetChannelType() != camera.GetChannelType())
                            {
                                SettingGrab(camera.Param);
                                DigitizerService[camparam.DigiNumber.Value].CurCamera = camera;
                            }

                            // 접속이 안되었을 때, 동작이 이루어지지 않았을 수 있다.
                            camera.SwitchCamera();
                        }
                        else
                        {
                            if (DigitizerService[camparam.DigiNumber.Value].CurCamera.GetChannelType() != camera.GetChannelType())
                            {
                                if (Digiparams.ParamList[camparam.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.MILMORPHIS)
                                {
                                    camera.SwitchCamera();
                                }

                                SettingGrab(camera.Param);
                                DigitizerService[camparam.DigiNumber.Value].CurCamera = camera;
                            }
                        }
                    }

                    SetCallerbySwitchCamera(camparam, caller);
                }
            }
            catch (Exception err)
            {
                this.NotifyManager().Notify(EventCodeEnum.VISION_CAMERA_CHANGE_ERROR);
                LoggerManager.Exception(err);
            }
            return retVal;

        }

        /// <summary>
        /// Blob 기능 실행
        /// 1)-Blob기능에 필요한 버퍼를 할당하는 등 초기화
        /// 2)-Grab Thread가 실행되고 있다면 Thread Halt
        /// 3)-Grab을 통해 한 프로임(Grab의 결과)을 return 받아 Blob실행함수로 넘겨준다. 
        /// </summary>
        public BlobResult Blob(EnumProberCam camtype, BlobParameter blobpatam, ROIParameter roiparam = null, bool fillholes = true, bool invert = false, bool runsort = true)
        {

            try
            {
                BlobResult blobResult = null;
                CameraBase Cam = (CameraBase)GetCam(camtype);
                VisionProcessing.ImageProcessing =
                (ImageBuffer image) =>
                {
                    Cam.ImageProcessed(image);
                };

                if (Cam.Param.ColorDept.Value == (int)ColorDept.BlackAndWhite)
                {
                    ImageBuffer grabbuffer = SingleGrab(camtype, this);

                    if (grabbuffer != null)
                    {
                        blobResult = VisionProcessing.BlobObject(grabbuffer, blobpatam, roiparam, fillholes, invert, runsort);
                        //VisionProcessing.ViewResult(blobResult.ResultBuffer);

                        blobResult.ResultBuffer.Band = 3;
                        blobResult.ResultBuffer.ColorDept = 24;

                        DisplayProcessing(camtype, blobResult.ResultBuffer);
                    }
                    else
                    {
                        LoggerManager.Error($"Please check the camera connection.");
                    }

                }
                else if (Cam.Param.ColorDept.Value == (int)ColorDept.Color24)
                {
                    ImageBuffer grabbuffer = SingleGrab(camtype, this);

                    if (grabbuffer != null)
                    {
                        blobResult = VisionProcessing.BlobColorObject(grabbuffer, blobpatam, true, false, true);
                        DisplayProcessing(camtype, blobResult.ResultBuffer);
                    }
                    else
                    {
                        LoggerManager.Error($"Please check the camera connection.");
                    }

                }
                return blobResult;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Patten Matching Error : " + err);
                throw;
            }
            finally
            {
                VisionProcessing.ImageProcessing = null;
                DigitizerService[GetCam(camtype).GetDigitizerIndex()].ImageReady = null;
            }
        }

        public BlobResult FindBlobWithRectangularity(EnumProberCam camtype,
                            ref double ImgPosX,
                            ref double ImgPosY,
                            ref int Foundgraylevel,
                            int Threshold,
                            int BlobAreaLow,
                            int BlobAreaHigh,
                            int OffsetX,
                            int OffsetY,
                            int SizeX,
                            int SizeY,
                            double BlobSizeX,
                            double BlobSizeY,
                            bool isDilation = false,
                            double BlobSizeXMinimumMargin = 0.5,
                            double BlobSizeXMaximumMargin = 0.5,
                            double BlobSizeYMinimumMargin = 0.5,
                            double BlobSizeYMaximumMargin = 0.5,
                            bool AutolightFlag = false,
                            double minRectangularity = 0.0)
        {
            double PosX = 0;
            double PosY = 0;

            BlobResult blobResult = null;

            try
            {
                ImageBuffer grabbuffer = SingleGrab(camtype, this);

                if (grabbuffer != null)
                {
                    blobResult = VisionProcessing.FindBlobWithRectangularity(grabbuffer,
                                                        ref PosX,
                                                        ref PosY,
                                                        Threshold,
                                                        BlobAreaLow,
                                                        BlobAreaHigh,
                                                        OffsetX,
                                                        OffsetY,
                                                        SizeX,
                                                        SizeY,
                                                        BlobSizeX,
                                                        BlobSizeY,
                                                        isDilation,
                                                        BlobSizeXMinimumMargin,
                                                        BlobSizeXMaximumMargin,
                                                        BlobSizeYMinimumMargin,
                                                        BlobSizeYMaximumMargin,
                                                        minRectangularity);

                    if (blobResult?.DevicePositions?.Count != 1)
                    {
                        LoggerManager.Debug($"[VisionManager] FindBlob() Auto light flag is {AutolightFlag}, Gray level is {Foundgraylevel}");

                        if (AutolightFlag == true && Foundgraylevel != 0)
                        {
                            if (this.AutoLightAdvisor().SetGrayLevel(camtype, Foundgraylevel) == true)
                            {
                                grabbuffer = SingleGrab(camtype, this);

                                blobResult = VisionProcessing.FindBlobWithRectangularity(grabbuffer,
                                                    ref PosX,
                                                    ref PosY,
                                                    Threshold,
                                                    BlobAreaLow,
                                                    BlobAreaHigh,
                                                    OffsetX,
                                                    OffsetY,
                                                    SizeX,
                                                    SizeY,
                                                    BlobSizeX,
                                                    BlobSizeY,
                                                    isDilation,
                                                    BlobSizeXMinimumMargin,
                                                    BlobSizeXMaximumMargin,
                                                    BlobSizeYMinimumMargin,
                                                    BlobSizeYMaximumMargin,
                                                    minRectangularity);
                            }
                        }
                    }
                    else
                    {
                        // Success
                        if (AutolightFlag == true)
                        {
                            VisionProcessing.GetGrayLevel(ref grabbuffer);

                            Foundgraylevel = grabbuffer.GrayLevelValue;
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"Please check the camera connection.");
                }

                ImgPosX = PosX;
                ImgPosY = PosY;

                return blobResult;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Patten Matching Error : " + err);
                throw;
            }
        }
        public BlobResult FindBlob(EnumProberCam camtype,
                            ref double ImgPosX,
                            ref double ImgPosY,
                            ref int Foundgraylevel,
                            int Threshold,
                            int BlobAreaLow,
                            int BlobAreaHigh,
                            int OffsetX,
                            int OffsetY,
                            int SizeX,
                            int SizeY,
                            bool isDilation = false,
                            double BlobSizeX = 0,
                            double BlobSizeY = 0,
                            double BlobSizeXMinimumMargin = 0.5,
                            double BlobSizeXMaximumMargin = 0.5,
                            double BlobSizeYMinimumMargin = 0.5,
                            double BlobSizeYMaximumMargin = 0.5,
                            bool AutolightFlag = false)
        {
            double PosX = 0;
            double PosY = 0;

            BlobResult blobResult = null;

            try
            {
                CameraBase Cam = (CameraBase)GetCam(camtype);

                ImageBuffer grabbuffer = SingleGrab(camtype, this);

                if (grabbuffer != null)
                {
                    blobResult = VisionProcessing.FindBlob(grabbuffer,
                                                        ref PosX,
                                                        ref PosY,
                                                        Threshold,
                                                        BlobAreaLow,
                                                        BlobAreaHigh,
                                                        OffsetX,
                                                        OffsetY,
                                                        SizeX,
                                                        SizeY,
                                                        BlobSizeX,
                                                        BlobSizeY,
                                                        isDilation,
                                                        BlobSizeXMinimumMargin,
                                                        BlobSizeXMaximumMargin,
                                                        BlobSizeYMinimumMargin,
                                                        BlobSizeYMaximumMargin);

                    if (blobResult?.DevicePositions?.Count != 1)
                    {
                        LoggerManager.Debug($"[VisionManager] FindBlob() Auto light flag is {AutolightFlag}, Gray level is {Foundgraylevel}");

                        if (AutolightFlag == true && Foundgraylevel != 0)
                        {
                            if (this.AutoLightAdvisor().SetGrayLevel(camtype, Foundgraylevel) == true)
                            {
                                grabbuffer = SingleGrab(camtype, this);

                                blobResult = VisionProcessing.FindBlob(grabbuffer,
                                                    ref PosX,
                                                    ref PosY,
                                                    Threshold,
                                                    BlobAreaLow,
                                                    BlobAreaHigh,
                                                    OffsetX,
                                                    OffsetY,
                                                    SizeX,
                                                    SizeY,
                                                    BlobSizeX,
                                                    BlobSizeY,
                                                    isDilation,
                                                    BlobSizeXMinimumMargin,
                                                    BlobSizeXMaximumMargin,
                                                    BlobSizeYMinimumMargin,
                                                    BlobSizeYMaximumMargin);
                            }
                        }
                    }
                    else
                    {
                        // Success
                        if (AutolightFlag == true)
                        {
                            VisionProcessing.GetGrayLevel(ref grabbuffer);

                            Foundgraylevel = grabbuffer.GrayLevelValue;
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"Please check the camera connection.");
                }

                ImgPosX = PosX;
                ImgPosY = PosY;

                return blobResult;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Patten Matching Error : " + err);
                throw;
            }
        }

        public EventCodeEnum SavePattern(RegisteImageBufferParam prparam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(prparam.PatternPath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prparam.PatternPath));
                }

                ImageBuffer grabbuffer = null;

                if (prparam.ImageBuffer == null)
                {
                    grabbuffer = SingleGrab(prparam.CamType, this);
                }
                else
                {
                    grabbuffer = prparam.ImageBuffer;
                }

                VisionProcessing.RegistModel(grabbuffer, prparam.LocationX, prparam.LocationY, prparam.Width, prparam.Height, prparam.Rotangle, prparam.PatternPath, prparam.IsregistMask);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum SavePattern(PatternInfomation prparam, string filePathPrefix = "")
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (prparam.PMParameter.ModelFilePath.Value != null && prparam.PMParameter.PatternFileExtension.Value != null)
                {
                    string modelFilePath = prparam.PMParameter.ModelFilePath.Value;
                    string patternFileExtension = prparam.PMParameter.PatternFileExtension.Value;

                    if (modelFilePath.StartsWith("\\"))
                    {
                        modelFilePath = modelFilePath.TrimStart('\\');
                    }

                    string patternFilePath = System.IO.Path.Combine(filePathPrefix, System.IO.Path.ChangeExtension(modelFilePath, patternFileExtension));

                    VisionProcessing.RegistModel(prparam.Imagebuffer, 0, 0, prparam.Imagebuffer.SizeX, prparam.Imagebuffer.SizeY, 0, patternFilePath);

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"SavePattern(): RegistModel Failed, PatternInformation - ModelFilePath, PatternFileExtension is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum ImageBufferToToTextFile(byte[] imageData, string outputPath)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // 유효성 검사: 이미지 데이터가 null이 아니고 길이가 0보다 큰지 확인
                if (imageData == null || imageData.Length == 0)
                {
                    LoggerManager.Debug("Invalid image data. Encoding aborted.");
                    return retval;
                }

                // 출력 경로의 폴더가 존재하는지 확인하고, 존재하지 않으면 폴더 생성
                string directoryPath = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // 이미지 데이터를 압축
                byte[] compressedData;
                using (var ms = new MemoryStream())
                {
                    using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzip.Write(imageData, 0, imageData.Length);
                    }
                    compressedData = ms.ToArray();
                }

                // 압축된 데이터를 Base64로 인코딩
                string encodedData = Convert.ToBase64String(compressedData);

                string output = Path.ChangeExtension(outputPath, "txt");

                // 인코딩된 데이터를 텍스트 파일에 저장
                File.WriteAllText(output, encodedData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void ImageBufferFromTextFiles(string inputFolder, string outputFolder = "")
        {
            try
            {
                if (string.IsNullOrEmpty(outputFolder))
                {
                    outputFolder = Path.Combine(inputFolder, "Decoded");
                }

                // 출력 경로의 폴더가 존재하는지 확인하고, 존재하지 않으면 폴더 생성
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                string[] files = Directory.GetFiles(inputFolder, "*.txt");

                foreach (var file in files)
                {
                    // outputFolder\file.bmp의 형태로 outputFile을 할당
                    string outputFileName = Path.GetFileNameWithoutExtension(file) + ".bmp";
                    string outputFile = Path.Combine(outputFolder, outputFileName);

                    ImageBufferFromTextFile(file, outputFile);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ImageBufferFromTextFile(string inputFile, string outputFile)
        {
            try
            {
                // 유효성 검사: 파일 경로가 null이 아니고 파일이 존재하는지 확인
                if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                {
                    LoggerManager.Debug("Invalid file path. Decoding aborted.");
                    return;
                }

                // 텍스트 파일에서 인코딩된 데이터 읽기
                string encodedData = File.ReadAllText(inputFile);

                // Base64 문자열을 압축된 바이트 배열로 디코딩
                byte[] compressedData = Convert.FromBase64String(encodedData);

                // 압축된 데이터를 원래의 바이트 배열로 디코딩
                byte[] imageData;
                using (var ms = new MemoryStream(compressedData))
                {
                    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        using (var decompressedMs = new MemoryStream())
                        {
                            gzip.CopyTo(decompressedMs);
                            imageData = decompressedMs.ToArray();
                        }
                    }
                }

                int sizeX = 960;
                int sizeY = 960;

                var RetImg = new ImageBuffer(imageData, sizeX, sizeY, 1, (int)ColorDept.BlackAndWhite);

                this.VisionManager().SaveImageBuffer(RetImg, outputFile, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SaveImageBuffer(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!EnableImageBufferToTextFile)
                {
                    retVal = VisionProcessing.SaveImageBuffer(image, path, logtype, eventcode, offsetx, offsety, width, height, rotangle);

                    if (this.FileManager().FileManagerParam.SaveClipImage.Value == true)
                    {
                        // [현재 클립 이미지 저장 조건]
                        // - path에 ModuleType 지정을 통한 경로가 포함 됨.

                        bool ShouldSaveClipImage = false;

                        if (this.FileManager().FileManagerParam.AlwaysSaveClipImage.Value == true || logtype == IMAGE_LOG_TYPE.FAIL)
                        {
                            string clip_path = string.Empty;

                            // 클립 이미지 저장 경로 획득
                            bool valid_path = this.FileManager().GetClipImagePath(path, out clip_path);

                            if (valid_path)
                            {
                                ShouldSaveClipImage = true;
                            }

                            if (ShouldSaveClipImage)
                            {
                                ResizeRatioEnum resizeRatio = this.FileManager().FileManagerParam.ClipImageResizeRatio.Value;

                                if ((resizeRatio == ResizeRatioEnum.OriginalSize) || (image.SizeX <= (960 / (int)resizeRatio) && image.SizeY <= (960 / (int)resizeRatio)))
                                {
                                    // Save image without resize
                                    retVal = VisionProcessing.SaveImageBuffer(image, clip_path, logtype, eventcode, offsetx, offsety, image.SizeX, image.SizeY, rotangle);
                                }
                                else
                                {
                                    // Resize image
                                    var clip_img = ResizeImageBuffer(image, image.SizeX / (int)resizeRatio, image.SizeY / (int)resizeRatio);

                                    retVal = VisionProcessing.SaveImageBuffer(clip_img, clip_path, logtype, eventcode, offsetx, offsety, clip_img.SizeX, clip_img.SizeY, rotangle);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // TODO : 
                    retVal = ImageBufferToToTextFile(image.Buffer, path);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveImageBufferWithRectnagle(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, Rect focusROI, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = VisionProcessing.SaveImageBufferWithRectnagle(image, path, logtype, eventcode, focusROI, offsetx, offsety, width, height, rotangle);

                if (this.FileManager().FileManagerParam.SaveClipImage.Value == true)
                {
                    // [현재 클립 이미지 저장 조건]
                    // - path에 ModuleType 지정을 통한 경로가 포함 됨.

                    bool ShouldSaveClipImage = false;

                    if (this.FileManager().FileManagerParam.AlwaysSaveClipImage.Value == true || logtype == IMAGE_LOG_TYPE.FAIL)
                    {
                        string clip_path = string.Empty;

                        // 클립 이미지 저장 경로 획득
                        bool valid_path = this.FileManager().GetClipImagePath(path, out clip_path);

                        if (valid_path)
                        {
                            ShouldSaveClipImage = true;
                        }

                        if (ShouldSaveClipImage)
                        {
                            ResizeRatioEnum resizeRatio = this.FileManager().FileManagerParam.ClipImageResizeRatio.Value;

                            if ((resizeRatio == ResizeRatioEnum.OriginalSize) || (image.SizeX <= (960 / (int)resizeRatio) && image.SizeY <= (960 / (int)resizeRatio)))
                            {
                                // Save image without resize
                                retVal = VisionProcessing.SaveImageBufferWithRectnagle(image, clip_path, logtype, eventcode, focusROI, offsetx, offsety, image.SizeX, image.SizeY, rotangle);
                            }
                            else
                            {
                                // Resize image
                                var clip_img = ResizeImageBuffer(image, image.SizeX / (int)resizeRatio, image.SizeY / (int)resizeRatio);

                                retVal = VisionProcessing.SaveImageBufferWithRectnagle(clip_img, clip_path, logtype, eventcode, focusROI, offsetx, offsety, clip_img.SizeX, clip_img.SizeY, rotangle);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AddEdgePosBuffer(ImageBuffer image, double x = 0, double y = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = VisionProcessing.AddEdgePosBuffer(
                    image, x, y);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public ImageBuffer LoadImageFile(string filepath)
        {
            return VisionProcessing.LoadImageFile(filepath);
        }
        public ObservableCollection<GrabDevPosition> Detect_Pad(ImageBuffer _grabedImage, double UserPadSizeX, double UserPadSizeY, ROIParameter roiparam, bool display = true)
        {
            try
            {
                if (display == true)
                {
                    VisionProcessing.ImageProcessing =
                        (ImageBuffer image) =>
                        {
                            CameraBase cam = (CameraBase)GetCam(_grabedImage.CamType);
                            cam.ImageProcessed(image);
                        };
                }

                if (_grabedImage.Buffer == null)
                {
                    _grabedImage = SingleGrab(_grabedImage.CamType, this);
                }

                return VisionProcessing.Detect_Pad(_grabedImage, UserPadSizeX, UserPadSizeY, roiparam);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "Detect_Pad() Error occurred.");
                LoggerManager.Exception(err);

                return null;
            }
        }

        #region # Loader PreAlign & Cassette Scan
        public EdgeProcResult FindPreAlignCenteringEdge(ImageBuffer ib, bool saveDump = false)
        {
            try
            {
                return VisionProcessing.FindEdgeProcessing(ib, saveDump);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Not Find Edge");
                throw err;
            }
        }
        public EdgeProcResult FindEdgeProcessor(EnumProberCam camtype, bool saveDump = false)
        {
            try
            {
                ImageBuffer ib = SingleGrab(camtype, this);
                return VisionProcessing.FindEdgeProcessing(ib, saveDump);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Not Find Edge");
                throw err;
            }
        }
        public CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParams, bool saveDump = false)
        {
            try
            {
                return VisionProcessing.CassetteScanProcessing(ib, slotParams, saveDump);
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        #endregion
        public int GetFocusValue(ImageBuffer img, Rect roi = new Rect())
        {
            try
            {
                return VisionProcessing.GetFocusValue(img, roi);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw err;
            }
        }

        public EventCodeEnum GetGrayValue(ref ImageBuffer img)
        {
            try
            {
                return VisionProcessing.GetGrayLevel(ref img);
            }
            catch (VisionException err)
            {
                throw new VisionException("GetGrayValue" + err.Message, err, EventCodeEnum.VISION_SAVEIMAGE_ERROR, this);
            }
            catch (Exception err)
            {
                //throw new VisionException("GetGrayValue in VisionJ" + err.Message, err, EventCodeEnum.VISION_SAVEIMAGE_ERROR, this);
                //LoggerManager.Error($err, "GetGrayValue() Error occurred.");
                LoggerManager.Exception(err);

                return EventCodeEnum.UNDEFINED;
            }
        }

        public ImageBuffer GetPatternImageInfo(PatternInfomation ptinfo)
        {
            ImageBuffer retval = null;

            try
            {
                retval = VisionProcessing.LoadImageFile(ptinfo.PMParameter.ModelFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value);

                VisionProcessing.GetGrayLevel(ref retval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 조명을 변경하여 재시도 하는 로직은 
        /// 1) 성공했던 때의 조명 정보가 있다면 조명 설정을 변경하고 재시도 해본다. ( ptinfo. SuccessVisionLightParams )
        /// 2) 패턴 저장했을 당시 화면의 GrayLevel 정보가 있다면 해당 GrayLevel 로 AutoLight 로직을 돌리고 재시도 해본다. (ptinfo.GrayLevel)
        /// 3) 성공했던 때의 화면의 GrayLevel 정보가 있다면 해당 GrayLevel 로 AutoLight 로직을 돌리고 재시도 해본다. (ptinfo.SuccessVisionGrayLevel)
        /// </summary>
        /// <param name="ptinfo"></param> : PatternInfomation 정보
        /// <param name="callassembly"></param>
        /// <param name="angleretry"></param> : PatternMatching 실패 시, 각도를 보정하여 Retry 를 시도해 볼건지에 대한 설정
        /// <param name="img"></param> : TargetImage (해당 이미지의 데이터가 null 인 경우 PatternInfo 의 Camera 정보로 TargetImage 를 자동 획득하여 Processing 함)
        /// <param name="display"></param>
        /// <param name="offsetx"></param>
        /// <param name="offsety"></param>
        /// <param name="sizex"></param>
        /// <param name="sizey"></param>
        /// <param name="retryautolight"></param> : PatternMatching 실패 시, Auto Light 를 시도할 것인지에 대한 설정 
        /// <param name="retrySuccessedLight"></param> : PatternMatching 실패 시, PatternInfomation 에 해당 패턴으로 Processing 성공 데이터가 있다면 조명 설정 값과 GrayLevel 을 사용 하여 재시도 할 것인지에 대한 설정
        /// <returns></returns>
        public PMResult PatternMatching(PatternInfomation ptinfo, object callassembly, bool angleretry = false, ImageBuffer img = null, bool display = true,
            int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0, bool retryautolight = false, bool retrySuccessedLight = false)
        {
            int originPMAcceptance = ptinfo.PMParameter.PMAcceptance.Value;

            try
            {
                ImageBuffer grabbuffer;
                ImageBuffer grabbuffer_original = new ImageBuffer();
                PMResult pmResult = null;
                CameraBase Cam = (CameraBase)GetCam(ptinfo.CamType.Value);

                if (img == null)
                {
                    grabbuffer = SingleGrab(ptinfo.CamType.Value, callassembly);
                }
                else
                {
                    grabbuffer = img;
                }

                if (grabbuffer != null)
                {
                    if (grabbuffer.ErrorCode != EventCodeEnum.GRAB_USER_CANCEL)
                    {
                        grabbuffer.CopyTo(grabbuffer_original);

                        pmResult = VisionProcessing.PatternMatching(grabbuffer, ptinfo.Imagebuffer, ptinfo.PMParameter,
                                  ptinfo.PMParameter.ModelFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value,
                                  ptinfo.PMParameter.MaskFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value,
                                  angleretry, offsetx, offsety, sizex, sizey);

                        if (pmResult != null)
                        {
                            // Processing 성공 Camera Light 정보로 Retry
                            if (pmResult.RetValue != EventCodeEnum.NONE && retrySuccessedLight)
                            {
                                if (ptinfo.SuccessVisionLightParams != null && ptinfo.SuccessVisionLightParams.Count != 0)
                                {
                                    LoggerManager.Debug($"[Vision Pattern Matching Retry Set Successed Light] callr : {callassembly.ToString()}");

                                    foreach (var light in ptinfo.SuccessVisionLightParams)
                                    {
                                        Cam.SetLight(light.Type.Value, light.Value.Value);
                                    }

                                    SingleGrayAndPatternMaching();
                                }
                                else
                                {
                                    LoggerManager.Debug($"[Vision Pattern Matching Retry Set Successed Light] data is null");
                                }

                                // AutoLight 기능을 통한 Retry
                                if (pmResult.RetValue != EventCodeEnum.NONE && retryautolight)
                                {
                                    /// AutoLight Target Data 
                                    /// 1) Device 정보 ( 패턴 등록 당시 화면의 GrayLevel )
                                    /// 2) Processing 성공 정보 ( 패턴 정보에 이전에 성공했을 당시 화면의 전체 GrayLevel )
                                    /// 3) Pattern 의 GrayLevel
                                    LoggerManager.Debug($"[Vision Pattern Matching Retry AutoLight ] callr : {callassembly.ToString()}");

                                    int autoLightTargetGrayLevel = 0;
                                    bool autoLightRetVal = false;

                                    /// 1) Device 정보 ( 패턴 등록 당시 화면의 GrayLevel )
                                    if (ptinfo.GrayLevel != 0)
                                    {
                                        autoLightTargetGrayLevel = ptinfo.GrayLevel;

                                        LoggerManager.Debug($"[Vision Pattern Matching Retry AutoLight] Device target light : {autoLightTargetGrayLevel}");

                                        autoLightRetVal = this.AutoLightAdvisor().SetGrayLevel(ptinfo.CamType.Value, autoLightTargetGrayLevel);

                                        if (autoLightRetVal != false)
                                        {
                                            SingleGrayAndPatternMaching();
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[Vision Pattern Matching Retry AutoLight] Device target light data is not exist");
                                    }

                                    if (pmResult.RetValue != EventCodeEnum.NONE)
                                    {
                                        if (ptinfo.SuccessVisionGrayLevel != 0)
                                        {
                                            autoLightTargetGrayLevel = ptinfo.SuccessVisionGrayLevel;

                                            /// 2) Processing 성공 정보 ( 패턴 정보에 이전에 성공했을 당시 화면의 전체 GrayLevel )
                                            LoggerManager.Debug($"[Vision Pattern Matching Retry AutoLight] Successed image target light : {autoLightTargetGrayLevel}");

                                            autoLightRetVal = this.AutoLightAdvisor().SetGrayLevel(ptinfo.CamType.Value, autoLightTargetGrayLevel);

                                            if (autoLightRetVal != false)
                                            {
                                                SingleGrayAndPatternMaching();
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"[Vision Pattern Matching Retry AutoLight] Successed image target light data is not exist");
                                        }
                                    }

                                    if (pmResult.RetValue != EventCodeEnum.NONE)
                                    {

                                        if (File.Exists(ptinfo.PMParameter.ModelFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value))
                                        {
                                            ImageBuffer patternbuffer = VisionProcessing.LoadImageFile(ptinfo.PMParameter.ModelFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value);
                                            VisionProcessing.GetGrayLevel(ref patternbuffer);

                                            /// 3) Pattern 의 GrayLevel
                                            autoLightTargetGrayLevel = patternbuffer.GrayLevelValue;

                                            LoggerManager.Debug($"[Vision Pattern Matching Retry AutoLight] Pattern image target light : {autoLightTargetGrayLevel}.");
                                            autoLightRetVal = this.AutoLightAdvisor().SetGrayLevel(ptinfo.CamType.Value, autoLightTargetGrayLevel);
                                            if (autoLightRetVal != false)
                                            {
                                                SingleGrayAndPatternMaching();
                                            }
                                        }
                                    }
                                }

                                // Wafer Low Camera 인 경우 Acceptance 를 낮춰서 재시도
                                if (pmResult.RetValue != EventCodeEnum.NONE)
                                {
                                    if (ptinfo.CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                                    {
                                        ptinfo.PMParameter.PMAcceptance.Value = this.GetWaferLowPMDownAcceptance();
                                    }

                                    LoggerManager.Debug($"[Vision Pattern Matching Retry Down Acceptance] callr : {callassembly.ToString()} ,  acceptance {originPMAcceptance} to {ptinfo.PMParameter.PMAcceptance.Value}");

                                    SingleGrayAndPatternMaching();
                                }

                                if (pmResult.RetValue == EventCodeEnum.VISION_PM_NOT_FOUND)
                                {
                                    if (ptinfo.PMParameter.ModelFilePath != null)
                                    {
                                        LoggerManager.Debug($"PatternMatching({ptinfo.PMParameter.ModelFilePath}): Result = {pmResult.RetValue}");
                                    }
                                }
                            }

                            if (pmResult != null && pmResult.RetValue == EventCodeEnum.NONE)
                            {
                                this.VisionManager().SetImage(grabbuffer_original, IMAGE_LOG_TYPE.PASS, IMAGE_SAVE_TYPE.BMP, IMAGE_PROCESSING_TYPE.PATTERNMATCHING, EventCodeEnum.NONE);

                                //Processing 시에 성공한 TargetImage 의 GrayLevel 을 기억
                                ptinfo.SuccessVisionGrayLevel = pmResult.ResultBuffer.GrayLevelValue;

                                //Processing 완료(성공) 후 조명 값을 기억
                                if (Cam != null && ptinfo.LightParams != null)
                                {
                                    if (ptinfo.SuccessVisionLightParams == null)
                                    {
                                        ptinfo.SuccessVisionLightParams = new ObservableCollection<LightValueParam>();
                                    }
                                    else
                                    {
                                        ptinfo.SuccessVisionLightParams.Clear();
                                    }

                                    foreach (var light in ptinfo.LightParams)
                                    {
                                        int lightVal = Cam.GetLight(light.Type.Value);
                                        ptinfo.SuccessVisionLightParams.Add(new LightValueParam(light.Type.Value, (ushort)lightVal));
                                    }
                                }

                                PatternMatchingResultLogging(ptinfo.PMParameter);
                            }
                            else
                            {
                                this.VisionManager().SetImage(grabbuffer_original, IMAGE_LOG_TYPE.FAIL, IMAGE_SAVE_TYPE.BMP, IMAGE_PROCESSING_TYPE.PATTERNMATCHING, pmResult.RetValue);
                            }

                            //Lloyd =>pmResult 결과가 null인데 null 체크 안해서 nullPointException 남.
                            if (display == true)
                            {
                                if (pmResult != null && pmResult.RetValue == EventCodeEnum.NONE && visionProcParam.ProcRaft.Value != EnumVisionProcRaft.EMUL)
                                {
                                    DisplayProcessing(ptinfo.CamType.Value, pmResult.ResultBuffer);

                                    return pmResult;
                                }
                            }

                        }
                    }
                }
                return pmResult;

                PMResult SingleGrayAndPatternMaching()
                {
                    try
                    {
                        if (callassembly == null)
                            grabbuffer = SingleGrab(ptinfo.CamType.Value, System.Reflection.Assembly.GetCallingAssembly());
                        else
                            grabbuffer = SingleGrab(ptinfo.CamType.Value, callassembly);

                        if (grabbuffer != null)
                        {
                            pmResult = VisionProcessing.PatternMatching(grabbuffer, ptinfo.Imagebuffer, ptinfo.PMParameter,
                                ptinfo.PMParameter.ModelFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value,
                                ptinfo.PMParameter.MaskFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value,
                                angleretry, offsetx, offsety, sizex, sizey);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return pmResult;
                }
            }
            catch (VisionException err)
            {
                throw new VisionException("PatternMatching" + err.Message, err, EventCodeEnum.VISION_PM_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("PatternMatching in Vision Manager" + err.Message, err);
            }
            finally
            {
                if (ptinfo != null)
                {
                    ptinfo.PMParameter.PMAcceptance.Value = originPMAcceptance;
                }
            }
        }
        public PMResult PatternMatchingRetry(PatternInfomation ptinfo, bool angleretry = false, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0)
        {
            PMResult pmresult = null;
            try
            {
                ImageBuffer patternbuffer;
                List<LightValueParam> lightvals = new List<LightValueParam>();
                //..Regist Current Light
                ICamera cam = GetCam(ptinfo.CamType.Value);

                foreach (var light in cam.LightsChannels)
                {
                    lightvals.Add(new LightValueParam(light.Type.Value, (ushort)cam.GetLight(light.Type.Value)));
                }


                //====> 1st Search Algorithm : Change to pattern Gray Level

                //==> Get Pattern Image Buffer]
                //patternbuffer = VisionProcessing.LoadImageFile(ptinfo.PMParameter.PMModelPath + "_RefModel_BAndW.mmo");
                patternbuffer = VisionProcessing.LoadImageFile(ptinfo.PMParameter.ModelFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value);

                VisionProcessing.GetGrayLevel(ref patternbuffer);

                //==> Get Pattern Gray Level
                int patternImageGrayLevel = patternbuffer.GrayLevelValue;

                //==> Set cur camera gray level set to pattern gray level
                if (this.AutoLightAdvisor().SetGrayLevel(ptinfo.CamType.Value, patternImageGrayLevel) == false)
                {
                    return null;
                }

                //==> Pattern Mathcing 
                pmresult = PatternMatching(ptinfo, this);

                if (pmresult == null)
                {
                    return null;
                }

                //==> Pattern Matching Success
                if (pmresult.ResultParam.Count > 0)
                {
                    return pmresult;
                }

                //====> 2nd Search Algorithm : Divide Search
                int addValue = 10;
                int i = 1;
                bool endUp = false;
                bool endDonw = false;

                while (true)
                {
                    if (endUp && endDonw)
                    {
                        break;
                    }

                    foreach (LightChannelType channel in cam.LightsChannels)
                    {
                        int intensity = cam.GetLight(channel.Type.Value);
                        cam.SetLight(channel.Type.Value, (ushort)(intensity + addValue));

                        if (intensity < 0)
                        {
                            endDonw = true;
                        }
                        else if (intensity > 255)
                        {
                            endUp = true;
                        }
                    }

                    //==> Pattern Mathcing 
                    pmresult = PatternMatching(ptinfo, this);

                    if (pmresult == null)
                    {
                        break;
                    }

                    //==> Pattern Matching Success
                    if (pmresult.ResultParam.Count > 0)
                    {
                        break;
                    }

                    if (i % 2 == 0)
                    {
                        addValue += addValue > 0 ? 10 : -10;
                    }

                    addValue = addValue * -1;
                    i++;

                    //_delays.DelayFor(1);
                    System.Threading.Thread.Sleep(1);
                }

                //Restore Light Value
                foreach (var light in lightvals)
                {
                    cam.SetLight(light.Type.Value, light.Value.Value);
                }
            }
            catch (VisionException err)
            {
                throw new VisionException("PatternMatchingRetry Error" + err.Message, err, EventCodeEnum.VISION_PM_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("PatternMatchingRetry Error in VisionManager" + err.Message, err);
            }
            return pmresult;
        }
        public PMResult SearchPatternMatching(PatternInfomation ptinfo, bool findone = true, bool display = true)
        {
            try
            {
                ImageBuffer grabbuffer;
                PMResult pmResult = null;
                CameraBase Cam = (CameraBase)GetCam(ptinfo.CamType.Value);

                grabbuffer = SingleGrab(ptinfo.CamType.Value, System.Reflection.Assembly.GetCallingAssembly());

                if (grabbuffer != null)
                {
                    if (grabbuffer.ErrorCode != EventCodeEnum.GRAB_USER_CANCEL)
                    {

                        pmResult = VisionProcessing.SearchPatternMatching(grabbuffer, ptinfo, findone);

                        if (display == true)
                        {
                            if (pmResult.RetValue == EventCodeEnum.NONE && visionProcParam.ProcRaft.Value != EnumVisionProcRaft.EMUL)
                            {
                                DisplayProcessing(ptinfo.CamType.Value, pmResult.ResultBuffer);

                                return pmResult;
                            }
                        }
                    }
                }

                return pmResult;
            }
            catch (VisionException err)
            {
                throw new VisionException("PatternMatching" + err.Message, err, EventCodeEnum.VISION_PM_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("PatternMatching in Vision Manager" + err.Message, err);
            }
        }
        public ImageBuffer ResizeImageBuffer(ImageBuffer ib, int ScaleFactorX, int ScaleFactorY)
        {
            try
            {
                lock (ib.Buffer)
                {
                    return VisionProcessing.ResizeImageBuffer(ib, ScaleFactorX, ScaleFactorY);
                }
            }
            catch (VisionException err)
            {
                throw new VisionException("ResizeImageBuffer" + err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("ResizeImageBuffer in Vision Manager" + err.Message, err);
            }
        }

        public ImageBuffer ReduceImageSize(ImageBuffer ib, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0)
        {
            try
            {
                lock (ib.Buffer)
                {
                    if (ib.Buffer != null)
                    {
                        return VisionProcessing.ReduceImageSize(ib, offsetx, offsety, sizex, sizey);
                    }
                    else
                    {
                        return VisionProcessing.ReduceImageSize(SingleGrab(ib.CamType, this), offsetx, offsety, sizex, sizey);
                    }
                }
            }
            catch (VisionException err)
            {
                throw new VisionException("ReduceImageSize" + err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("ReduceImageSize in Vision Manager" + err.Message, err);
            }
        }

        public EventCodeEnum ImageGrabbed(EnumProberCam camtype, ImageBuffer img)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CameraBase camera = (CameraBase)GetCam(camtype);
                if (DigitizerService[camera.Param.DigiNumber.Value].GrabberService.bContinousGrab == true)
                {
                    StopGrab(camtype);
                }

                camera.ImageGrabbed(img);
            }
            catch (VisionException err)
            {
                throw new VisionException("ImageGrabbed" + err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("ImageGrabbed in Vision Manager" + err.Message, err);
                //return null;
            }
            return retVal;
        }

        public EventCodeEnum DisplayProcessing(EnumProberCam camtype, ImageBuffer img)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CameraBase camera = (CameraBase)GetCam(camtype);
                if (DigitizerService[camera.Param.DigiNumber.Value].GrabberService.bContinousGrab == true)
                {
                    DigitizerService[camera.Param.DigiNumber.Value].ImageReady -=
                            (ImageBuffer image) =>
                            {
                                camera.ImageGrabbed(image);
                            };
                }

                VisionProcessing.ImageProcessing =
                (ImageBuffer image) =>
                {
                    image.Band = 3;
                    camera.ImageProcessed(image);
                };

                VisionProcessing.ViewResult(img);

                VisionProcessing.ImageProcessing -=
                (ImageBuffer image) =>
                {
                    camera.ImageProcessed(image);
                };

                if (DigitizerService[camera.Param.DigiNumber.Value].GrabberService.bContinousGrab == false)
                {
                    DigitizerService[camera.Param.DigiNumber.Value].ImageReady +=
                            (ImageBuffer image) =>
                            {
                                camera.ImageGrabbed(image);
                            };
                }
            }
            catch (VisionException err)
            {
                throw new VisionException("DisplayProcessing" + err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("DisplayProcessing in Vision Manager" + err.Message, err);
            }
            return retVal;
        }

        public EventCodeEnum GetPatternSize(string path, out Size size)
        {
            try
            {
                return VisionProcessing.GetPatternSize(path, out size);
            }
            catch (VisionException err)
            {
                throw new VisionException("GetPatternSize" + err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception("GetPatternSize in VisionManager" + err.Message, err);
            }

        }







        public void Dispose()
        {
            try
            {
                VisionProcessing.Dispose();
                for (int digiIndex = 0; digiIndex < DigitizerService.Count; digiIndex++)
                {
                    DigitizerService[digiIndex].GrabberService.Dispose();
                    DigitizerService[digiIndex].Dispose();
                }
                Mil.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        #region //..DragAndDrop

        private RelayCommand<object> _DragDropVisionFilesCommand;
        public ICommand DragDropVisionFilesCommand
        {
            get
            {
                if (null == _DragDropVisionFilesCommand) _DragDropVisionFilesCommand
                        = new RelayCommand<object>(DragDropVisionFiles);
                return _DragDropVisionFilesCommand;
            }
        }


        private void DragDropVisionFiles(object fileinfos)
        {
            try
            {

                if (fileinfos is VisionFileInfos)
                {
                    VisionFileInfos files = (VisionFileInfos)fileinfos;
                    ICamera cam = GetCam(files.CamType);

                    List<ImageBuffer> imgs = new List<ImageBuffer>();

                    foreach (var filepath in files.FilePaths)
                    {
                        imgs.Add(LoadImageFile(filepath));
                    }
                    DigitizerService[cam.GetDigitizerIndex()].GrabberService.LoadUserImageFiles(imgs);
                }

            }
            catch (VisionException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }


        public void LoadImageFromFloder(string folderpath, EnumProberCam camtype)
        {
            try
            {
                if (Directory.Exists(folderpath))
                {
                    ICamera cam = GetCam(camtype);
                    List<ImageBuffer> imgs = new List<ImageBuffer>();
                    string[] fileEntries = Directory.GetFiles(folderpath);
                    foreach (var filepath in fileEntries)
                    {
                        imgs.Add(LoadImageFile(filepath));
                    }
                    DigitizerService[cam.GetDigitizerIndex()].GrabberService.LoadUserImageFiles(imgs);
                }
                //if (File.Exists(folderpath))
                //{
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadImageFromFileToGrabber(string filepath, EnumProberCam camtype)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    ICamera cam = GetCam(camtype);
                    List<ImageBuffer> imgs = new List<ImageBuffer>();
                    //ImageBuffer resizeGrabbuffer = ReduceImageSize(LoadImageFile(filepath), 0, 0, 480, 480);

                    ImageBuffer resizeGrabbuffer = LoadImageFile(filepath);
                    imgs.Add(resizeGrabbuffer);

                    DigitizerService[cam.GetDigitizerIndex()].GrabberService.LoadUserImageFiles(imgs);
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool ConfirmDigitizerEmulMode(EnumProberCam camtype)
        {
            try
            {
                ICamera cam = GetCam(camtype);
                if (cam != null)
                {
                    //Digiparams.ParamList[cam.Param.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.SIMUL_GRABBER
                    if (Digiparams.ParamList[cam.Param.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.EMULGRABBER ||
                        Digiparams.ParamList[cam.Param.DigiNumber.Value].GrabRaft.Value == EnumGrabberRaft.GIGE_EMULGRABBER)
                    {
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
            return false;
        }

        private RelayCommand<object> _DragOverVisionFilesCommand;
        public ICommand DragOverVisionFilesCommand
        {
            get
            {
                if (null == _DragOverVisionFilesCommand) _DragOverVisionFilesCommand
                        = new RelayCommand<object>(DragOverVisionFiles);
                return _DragOverVisionFilesCommand;
            }
        }

        private void DragOverVisionFiles(object param)
        {
            try
            {
                if (param is ICamera)
                {
                    if (!(DigitizerService[((ICamera)param).GetDigitizerIndex()].GetGrabberRaft() == EnumGrabberRaft.EMULGRABBER
                        || DigitizerService[((ICamera)param).GetDigitizerIndex()].GetGrabberRaft() == EnumGrabberRaft.GIGE_EMULGRABBER))
                    {
                        Mouse.OverrideCursor = Cursors.No;
                    }
                }


            }
            catch (VisionException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        public void ChangeGrabMode(EnumGrabberMode grabmode)
        {
            try
            {
                foreach (var digi in DigitizerService)
                {
                    digi.GrabberService.GrabMode = grabmode;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion


        public ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_get_path, bool saveDump = false)
        {
            try
            {
                return VisionProcessing.ReadOCRProcessing(ocrImage, ocrParams, font_get_path, saveDump);
            }
            catch (VisionException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($$"ReadOCRProcessing() : err={err.Message}");
                LoggerManager.Exception(err);

                throw new Exception(err.Message, err);
            }
        }

        public ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
        {
            try
            {
                return VisionProcessing.OcrCalibrateFontProcessing(ocrImage, ocrParams, font_output_path, saveDump);
            }
            catch (VisionException err)
            {
                LoggerManager.Exception(err);

                throw new VisionException(err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($$"OcrCalibrateFontProcessing() : err={err.Message}");
                LoggerManager.Exception(err);

                throw new Exception(err.Message, err);
            }
        }

        public ImageBuffer WirteTextToBuffer(List<string> sb, ImageBuffer ib)
        {
            ImageBuffer imb = VisionProcessing.WirteTextToBuffer(sb, ib);

            try
            {
                (GetCam(ib.CamType) as CameraBase).ImageProcessed(ib);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return imb;
        }

        public void StopWaitGrab(ICamera cam)
        {
            try
            {
                DigitizerService[cam.Param.DigiNumber.Value].GrabberService.StopWaitGrab();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public double EdgeFind_IndexAlign(ImageBuffer SrcBuf, int Cpos, int RWidth, int Threshold)
        {
            return VisionProcessing.EdgeFind_IndexAlign(SrcBuf, Cpos, RWidth, Threshold);
        }

        //임시 지연 함수
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            try
            {
                TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
                DateTime AfterWards = ThisMoment.Add(duration);

                while (AfterWards >= ThisMoment)
                {
                    ThisMoment = DateTime.Now;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return DateTime.Now;
        }

        public bool ConfirmContinusGrab(EnumProberCam camtype)
        {
            try
            {
                ICamera cam = GetCam(camtype);
                return DigitizerService[cam.Param.DigiNumber.Value].GrabberService.bContinousGrab;
            }
            catch (Exception err)
            {

                throw err;
            }

        }

        public bool ClearGrabberUserImage(EnumProberCam camtype)
        {
            try
            {
                ICamera cam = GetCam(camtype);
                DigitizerService[cam.Param.DigiNumber.Value].GrabberService.clearUserimageFiles();
                return true;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public void AllStageCameraStopGrab()
        {
            try
            {
                Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                foreach (var camera in GetCameras())
                {
                    for (int index = 0; index < stagecamvalues.Length; index++)
                    {
                        if (((StageCam)stagecamvalues.GetValue(index)).ToString() == camera.GetChannelType().ToString())
                        {
                            StopGrab(camera.GetChannelType());
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public double GetMaxFocusFlatnessValue()
        {
            double retVal = 85;
            try
            {
                var value = visionProcParam.MaxFocusFlatness.Value;
                retVal = value;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetMaxFocusFlatnessValue(): Error occurred. Err = {err.Message}");
            }

            return retVal;
        }
        public double GetFocusFlatnessTriggerValue()
        {
            double retVal = 1.5;
            try
            {
                var value = visionProcParam.FocusFlatnessTriggerValue.Value;
                retVal = value;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetFocusFlatnessTriggerValue(): Error occurred. Err = {err.Message}");
            }

            return retVal;
        }

        public int GetWaferLowPMDownAcceptance()
        {
            int retVal = 0;
            try
            {
                retVal = visionProcParam.WaferLowPMDownAcceptance.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetWaferLowPMDownAcceptance(int value)
        {
            try
            {
                visionProcParam.WaferLowPMDownAcceptance.Value = value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void PatternMatchingResultLogging(PMParameter pmparam)
        {
            MachineCoordinate machine = new MachineCoordinate();
            try
            {
                double encoderxpos = 0.0;
                double encoderypos = 0.0;
                double encoderzpos = 0.0;

                this.MotionManager().GetRefPos(EnumAxisConstants.X, ref encoderxpos);
                this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref encoderypos);
                this.MotionManager().GetRefPos(EnumAxisConstants.Z, ref encoderzpos);

                pmparam.MachineCoordPos = new MachineCoordinate(encoderxpos, encoderypos, encoderzpos);

                LoggerManager.Debug($"{this.GetType().Name}, PatternMatching() : X [{pmparam.MachineCoordPos.X.Value:0.00}] Y [{pmparam.MachineCoordPos.Y.Value:0.00}] Z [{pmparam.MachineCoordPos.Z.Value:0.00}]");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private IDigitizer GetDigitizer(EnumProberCam camtype)
        {
            IDigitizer retval = null;

            try
            {
                int digiIndex = GetCam(camtype).GetDigitizerIndex();

                retval = DigitizerService[digiIndex];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetParameter(EnumProberCam camtype, string modulename, string path)
        {
            try
            {
                GetDigitizer(camtype).Setparameter(modulename, path);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void SetParameters(EnumProberCam camtype, string modulename, string[] paths)
        {
            try
            {
                GetDigitizer(camtype).Setparameters(modulename, paths);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void StartImageCollection(EnumProberModule module, EventCodeEnum targetCode)
        {
            try
            {
                if (proberImageController != null)
                {
                    proberImageController.StartImageCollection(module, targetCode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void EndImageCollection()
        {
            try
            {
                if (proberImageController != null)
                {
                    proberImageController.EndImageCollection();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum GetConnectedNotifyCode()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = proberImageController.ConnectedNotifyCode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public (EnumProberModule? currentModuleType, string lastModuleStartTime, string lastHashCode) GetImageDataSetIdentifiers()
        {
            EnumProberModule? currentModuleType = null;
            string lastModuleStartTime = string.Empty;
            string lastHashCode = string.Empty;

            try
            {
                if (proberImageController != null)
                {
                    currentModuleType = proberImageController.CurrentProberModule;
                    lastModuleStartTime = proberImageController.LastModuleStartTime;
                    lastHashCode = proberImageController.LastHashCode;

                    proberImageController.CurrentProberModule = null;
                    proberImageController.ConnectedNotifyCode = EventCodeEnum.UNDEFINED;

                    proberImageController.LastModuleStartTime = string.Empty;
                    proberImageController.LastHashCode = string.Empty;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return (currentModuleType, lastModuleStartTime, lastHashCode);
        }

        public ImageDataSet GetImageDataSet(EnumProberModule moduletype, string moduleStartTime, string hashCode)
        {
            ImageDataSet retval = null;

            try
            {
                if (proberImageController != null)
                {

                    retval = proberImageController.GetImageDataSet(moduletype, moduleStartTime, hashCode);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public void SetImage(ImageBuffer imageBuffer, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum)
        {
            try
            {
                if (proberImageController != null)
                {
                    if (imageSaveFilter != ImageSaveFilter.NotUse)
                    {
                        proberImageController.SetImage(imageBuffer, iMAGE_LOG_TYPE, iMAGE_SAVE_TYPE, iMAGE_PROCESSING_TYPE, eventCodeEnum);
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], SetImage() : imageSaveFilter is {imageSaveFilter}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetImages(List<ImageBuffer> imageBuffers, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum)
        {
            try
            {
                if (proberImageController != null)
                {
                    if (imageSaveFilter != ImageSaveFilter.NotUse)
                    {
                        proberImageController.SetImages(imageBuffers, iMAGE_LOG_TYPE, iMAGE_SAVE_TYPE, iMAGE_PROCESSING_TYPE, eventCodeEnum);
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], SetImages() : imageSaveFilter is {imageSaveFilter}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task SaveModuleImagesAsync(EnumProberModule? enumProberModule)
        {
            return proberImageController.SaveModuleImagesAsync(enumProberModule, imageSaveFilter);
        }

        public ImageBuffer DrawCrosshair(ImageBuffer grabbedImage, Point pt, int length, int thickness = 1)
        {
            ImageBuffer retImg = new ImageBuffer();
            try
            {
                retImg = VisionProcessing.DrawCrosshair(grabbedImage, pt, length, thickness);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retImg;
        }

        public ImageBuffer CombineImages(ImageBuffer[] images, int width, int height, int rows, int columns)
        {
            ImageBuffer retImg = new ImageBuffer();
            try
            {
                retImg = VisionProcessing.CombineImages(images, width, height, rows, columns);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retImg;
        }
    }
}
