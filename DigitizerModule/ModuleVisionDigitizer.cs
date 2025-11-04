
namespace DigitizerModule
{
    using System;
    using ProberInterfaces;
    using Matrox.MatroxImagingLibrary;
    using System.Diagnostics;
    using Vision.GrabberModule;
    using System.ComponentModel;
    using ProberInterfaces.Vision;
    using ProberErrorCode;
    using SystemExceptions.VisionException;
    using LogModule;

    public class ModuleVisionDigitizer : INotifyPropertyChanged, IDigitizer, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #region // Grab continous delegate
        public ImageBuffer.ImageReadyDelegate ImageReady { get; set; }
        #endregion

        bool disposed = false;
        private MIL_ID _MilDigitizer = MIL.M_NULL;

        public MIL_ID MilDigitizer
        {
            get { return _MilDigitizer; }
            set { _MilDigitizer = value; }
        }

        private IGrabber _GrabberService;
        public IGrabber GrabberService
        {
            get
            {
                return _GrabberService;
            }

            set
            {
                _GrabberService = value;
            }
        }

        private ICamera _CurCamera = null;
        public ICamera CurCamera
        {
            get { return _CurCamera; }
            set
            {
                if (value != _CurCamera)
                {
                    _CurCamera = value;
                    NotifyPropertyChanged("CurCamera");
                }
            }
        }

        private EnumGrabberRaft _GrabberRaft;

        public EnumGrabberRaft GrabberRaft
        {
            get { return _GrabberRaft; }
            set { _GrabberRaft = value; }
        }

        private string _DigitizerName;

        public string DigitizerName
        {
            get { return _DigitizerName; }
            set { _DigitizerName = value; }
        }

        private int _GrabTimeOut;

        public int GrabTimeOut
        {
            get { return _GrabTimeOut; }
            set { _GrabTimeOut = value; }
        }

        public ModuleVisionDigitizer()
        {

        }

        public ModuleVisionDigitizer(EnumGrabberRaft grabRaft, string name)
        {
            try
            {
                switch (grabRaft)
                {
                    case EnumGrabberRaft.MILGIGE:
                        GrabberService = new ModuleVisionGrabber();
                        break;
                    case EnumGrabberRaft.MILMORPHIS:
                        GrabberService = new ModuleVisionGrabber();
                        break;
                    case EnumGrabberRaft.USB:
                        GrabberService = new USBVisionGrabber();
                        break;
                    case EnumGrabberRaft.EMULGRABBER:
                        GrabberService = new VisionGrabberEmul();
                        break;
                    case EnumGrabberRaft.GIGE_EMULGRABBER:
                        GrabberService = new VisionGrabberEmul();
                        break;
                    case EnumGrabberRaft.SIMUL_GRABBER:
                        GrabberService = new VisionGrabberSimul();
                        break;
                    case EnumGrabberRaft.TIS:
                        GrabberService = new TISGrabber();
                        break;
                }

                GrabberRaft = grabRaft;
                DigitizerName = name;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        ~ModuleVisionDigitizer()
        {
            try
            {
                Dispose(false);
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
                Dispose(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ServiceDispose()
        {
            try
            {
                GrabberService.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                try
                {

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            // Code to dispose the un-managed resources of the class

            try
            {
                DeInitDigitizer();
            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            disposed = true;
        }

        public void DeInitDigitizer()
        {
            try
            {
                if (MilDigitizer != MIL.M_NULL)
                {
                    MIL.MdigFree(MilDigitizer);
                    MilDigitizer = MIL.M_NULL;
                    GrabberService.Dispose();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public int InitDigitizer(int milSystem, int camchn, string path)
        {
            try
            {
                if (GrabberService is USBVisionGrabber
                    || GrabberService is VisionGrabberEmul
                    || GrabberService is VisionGrabberSimul 
                    || GrabberService is TISGrabber
                    )
                {
                    return camchn;
                }
                else
                {
                    string mDCFPath;
                    mDCFPath = path;

                    if (System.IO.File.Exists(mDCFPath) == true)
                    {
                        Debug.Print("File found.");
                    }
                    else
                    {
                        Debug.Print("DCF file not found.");
                        return -1;
                    }

                    try
                    {
                        if (milSystem != MIL.M_NULL)
                        {
                            MilDigitizer = MIL.MdigAlloc(milSystem, camchn, mDCFPath, MIL.M_DEFAULT, ref _MilDigitizer);
                        }
                        if (camchn == 0)
                            MIL.MdigControl(MilDigitizer, MIL.M_CHANNEL, MIL.M_CH0);
                        else if (camchn == 1)
                            MIL.MdigControl(MilDigitizer, MIL.M_CHANNEL, MIL.M_CH1);
                    }
                    catch (MILException err)
                    {
                        LoggerManager.Exception(err);
                        throw new VisionException("InitDigitizer Error", err, EventCodeEnum.VISION_EXCEPTION, this);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw new Exception(err.Message, err);
                    }
                    return (int)MilDigitizer;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }

        private int milSystem;
        private string camUserName;
        private string path;
        public int InitDigitizer(int milSystem, string camUserName, string path)
        {
            try
            {
                int DigNum = 0;
                if (GrabberService is USBVisionGrabber)
                {
                    return DigNum;
                }
                else
                {
                    string mDCFPath;
                    mDCFPath = path;

                    if (System.IO.File.Exists(mDCFPath) == true)
                    {
                        //Debug.Print("File found.");
                        LoggerManager.Debug($"{mDCFPath}: DCF Available.");
                    }
                    else
                    {
                        LoggerManager.Debug($"{mDCFPath}: DCF Unavailable.");
                        return -1;
                    }

                    try
                    {
                        if (milSystem != MIL.M_NULL)
                        {
                            MilDigitizer = MIL.MdigAlloc(milSystem, MIL.M_GC_CAMERA_ID(camUserName), mDCFPath, MIL.M_GC_DEVICE_USER_NAME, ref _MilDigitizer);
                            this.milSystem = milSystem;
                            this.camUserName = camUserName;
                            this.path = path;
                        }

                    }
                    catch (MILException err)
                    {
                        LoggerManager.Exception(err);
                        throw new VisionException("InitDigitizer Error", err, EventCodeEnum.VISION_EXCEPTION, this);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw new Exception(err.Message, err);
                    }

                    return (int)MilDigitizer;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int InitDigitizer()
        {
            try
            {
                int DigNum = 0;
                if (GrabberService is USBVisionGrabber)
                {
                    return DigNum;
                }
                else
                {
                    string mDCFPath;
                    mDCFPath = path;

                    if (System.IO.File.Exists(mDCFPath) == true)
                    {
                        //Debug.Print("File found.");
                        LoggerManager.Debug($"{mDCFPath}: DCF Available.");
                    }
                    else
                    {
                        LoggerManager.Debug($"{mDCFPath}: DCF Unavailable.");
                        return -1;
                    }

                    try
                    {
                        if (milSystem != MIL.M_NULL)
                        {
                            MIL_ID digi_ID = MIL.M_NULL;
                            var digitizer = MIL.MdigAlloc(milSystem, MIL.M_GC_CAMERA_ID(camUserName), mDCFPath, MIL.M_GC_DEVICE_USER_NAME, ref digi_ID);
                            MilDigitizer = digitizer;
                        }

                    }
                    catch (MILException err)
                    {
                        LoggerManager.Exception(err);
                        throw new VisionException("InitDigitizer Error", err, EventCodeEnum.VISION_EXCEPTION, this);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw new Exception(err.Message, err);
                    }

                    return (int)MilDigitizer;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void DeInitDigitize()
        {
            try
            {
                try
                {
                    GrabberService.Dispose();
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    LoggerManager.Debug($"GrabberService[{this.camUserName }] Dispose Fail");
                }

                if (MilDigitizer != MIL.M_NULL)
                {
                    MIL.MdigFree(MilDigitizer);
                    MilDigitizer = MIL.M_NULL;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public void PassageGrab()
        {
            try
            {
                GrabberService.ImageGrabbed = (ImageBuffer image) =>
                {
                    if (ImageReady != null)
                    {
                        ImageReady(image);
                    }
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public void BlockGrabEvent()
        {
            try
            {
                GrabberService.ImageGrabbed = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EnumGrabberRaft GetGrabberRaft()
        {
            try
            {
                return GrabberRaft;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum CheckDigitizer()
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

        public void Setparameter(string modulename, string path)
        {
            try
            {
                GrabberService.SetParameter(modulename, path);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void Setparameters(string modulename, string[] paths)
        {
            try
            {
                GrabberService.SetParameters(modulename, paths);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
