using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision.GrabberModule
{
    using System.Threading;
    using System.Timers;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.IO;
    using System.Reflection;
    using ProberInterfaces.WaferAlignEX.ModuleInterface;
    using System.ComponentModel;
    using ProberErrorCode;
    using VisionParams.Camera;
    using System.Diagnostics;
    using System.Windows.Input;
    using System.Windows;
    using LogModule;
    ////using ProberInterfaces.ThreadSync;

    public class VisionGrabberEmul : IGrabber, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        int ColorDept = 0;
        public int iGrabSizeX;
        public int iGrabSizeY;


        private int UserGrabSizeX = 0;
        private int UserGrabSizeY = 0;

        private AutoResetEvent _AreUpdateEvent = new AutoResetEvent(false);
        public AutoResetEvent AreUpdateEvent
        {
            get { return _AreUpdateEvent; }
            set
            {
                if (value != _AreUpdateEvent)
                {
                    _AreUpdateEvent = value;
                    NotifyPropertyChanged("AreUpdateEvent");
                }
            }
        }
        public CameraParameter CamParam;
        public ImageBuffer.ImageReadyDelegate ImageGrabbed { get; set; }
        public ImageBuffer CurImageBuffer { get; set; }

        public byte[] ArrImageBuff { get; set; }

        public bool bContinousGrab { get; set; }


        //private LockKey lockObject = new LockKey("EMUL Module Vision Grabber");
        //private LockKey ImageLockObject = new LockKey("EMUL Module Vision Grabber Image");

        private object lockObject = new object();
        private object ImageLockObject = new object();

        private System.Timers.Timer imggrabtimer;
        private static double GrabbingInterValInms = 33.3333;
        private bool LoadImageFilesFlag;
        private List<ImageBuffer> Images { get; set; } = new List<ImageBuffer>();
        private ImageBuffer CopyImage = new ImageBuffer();
        private ImageBuffer Image = new ImageBuffer();
        private int ContinusGrabImageIndex = -1;
        private int SingleGrabImageIndex = -1;
        private object Assembly;
        private EmulImageParameter EmulImageParam;

        private bool IsFocusing = false;

        private ActionModule GrabActionModule = null;
        private FocusingGrabImage ContinusFocusingModule = null;
        private ContinousGrabImage ContinusGrabModule = null;

        private EnumGrabberMode _GrabMode = EnumGrabberMode.DEFAULT;
        public EnumGrabberMode GrabMode
        {
            get { return _GrabMode; }
            set
            {
                if (value != _GrabMode)
                {
                    if (value == _GrabMode)
                        return;
                    _GrabMode = value;
                }
            }
        }

        private bool EnableVisionProc = false;

        public VisionGrabberEmul()
        {

        }

        ~VisionGrabberEmul()
        {
            imggrabtimer?.Stop();
        }

        public void InitGrabber(int milSystem, int digitizer, int camchn)
        {
            try
            {
                if (digitizer == -1)
                    return;
                LoadEmulImageParam();

                if (this.VisionManager().GetVisionProcRaft() == EnumVisionProcRaft.EMUL)
                    EnableVisionProc = false;
                else
                    EnableVisionProc = true;
                return;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private EventCodeEnum LoadEmulImageParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            EmulImageParam = new EmulImageParameter();

            string FullPath = this.FileManager().GetDeviceParamFullPath(EmulImageParam.FilePath, EmulImageParam.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(EmulImageParameter));
                if (RetVal == EventCodeEnum.NONE)
                {
                    EmulImageParam = tmpParam as EmulImageParameter;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($"[VisionManager] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;
        }

        public void SetParameter(string modulename, string path)
        {
            try
            {
                if (EmulImageParam != null)
                {
                    ActionModule module = new ActionModule(modulename);
                    module.SingleGrabImage.GrabInfos.Add(new GrabInfo(path));

                    EmulImageParam.ActionModules.Add(module);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetParameters(string modulename, string[] paths)
        {
            try
            {
                ActionModule module = new ActionModule(modulename);

                foreach (var path in paths)
                {
                    module.SingleGrabImage.GrabInfos.Add(new GrabInfo(path));
                }

                EmulImageParam.ActionModules.Add(module);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CreateEmulGrabWaferAlign()
        {
            try
            {
                string rootPath = Path.Combine(this.FileManager().FileManagerParam.DeviceParamRootDirectory,
                                    this.FileManager().FileManagerParam.DeviceName);
                rootPath = Path.Combine(rootPath, @"Vision\EmulImage\WaferAlign");

                ActionModule EdgeActioModule = new ActionModule(typeof(IWAEdgeModule).ToString());
                EdgeActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "Edge", "Edgeimg0.bmp")));
                EdgeActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "Edge", "Edgeimg1.bmp")));
                EdgeActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "Edge", "Edgeimg2.bmp")));
                EdgeActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "Edge", "Edgeimg3.bmp")));

                ActionModule LowActioModule = new ActionModule(typeof(IWALowModule).ToString());
                LowActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "LowThetaAlign1.mmo")));
                LowActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "LowThetaAlign2.mmo")));

                LowActioModule.ContinousGrabImages.Add(new ContinousGrabImage(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "LowThetaAlign1.mmo"))));

                ActionModule HighActioModule = new ActionModule(typeof(IWAHighModule).ToString());
                HighActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "HighThetaAlign1.mmo")));
                HighActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "HighThetaAlign2.mmo")));
                HighActioModule.SingleGrabImage.GrabInfos.Add(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "HighThetaAlign3.mmo")));

                HighActioModule.ContinousGrabImages.Add(new ContinousGrabImage(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "HighFocusing.mmo"))));

                HighActioModule.FocusingGrabImages.Add(
                    new FocusingGrabImage(new GrabInfo(
                    Path.Combine(rootPath, "ThetaAlign", "HighFocusing.mmo"))));

                EmulImageParam.ActionModules.Add(EdgeActioModule);
                EmulImageParam.ActionModules.Add(LowActioModule);
                EmulImageParam.ActionModules.Add(HighActioModule);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SettingGrabber(EnumGrabberRaft grabberType, ICameraParameter camparam)
        {
            //if (CamParam.ColorDept.Value == (int)ProberInterfaces.ColorDept.BlackAndWhite)
            //{
            try
            {
                CamParam = camparam as CameraParameter;
                if (CamParam != null)
                {

                    if (ArrImageBuff != null)
                        ArrImageBuff = null;

                    if (ColorDept != CamParam.ColorDept.Value)
                    {
                        ColorDept = CamParam.ColorDept.Value;
                    }

                    UserGrabSizeX = CamParam.GrabSizeX.Value;
                    UserGrabSizeY = CamParam.GrabSizeY.Value;

                    iGrabSizeX = UserGrabSizeX;
                    iGrabSizeY = UserGrabSizeY;

                    ArrImageBuff = new byte[UserGrabSizeX * UserGrabSizeX];

                    Image.SizeX = iGrabSizeX;
                    Image.SizeY = iGrabSizeY;
                    Image.ColorDept = CamParam.ColorDept.Value;
                    Image.Band = CamParam.Band.Value;
                    Image.CamType = CamParam.ChannelType.Value;
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public void SetCaller(object assembly)
        {
            try
            {
                this.Assembly = assembly;

                // TODO : SetCallerFocusingAssembly 역할 추가
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private ActionModule ExistEmulGrabInterface()
        {
            ActionModule retVal = null;

            try
            {
                if (GrabMode == EnumGrabberMode.AUTO ||
                    GrabMode == EnumGrabberMode.DEFAULT)
                {
                    foreach (var module in EmulImageParam.ActionModules)
                    {
                        // TODO : 
                        if (module.ModuleName == this.Assembly.GetType().Name)
                        {
                            bool isFound = false;

                            foreach (var info in module.SingleGrabImage.GrabInfos)
                            {
                                if (info.GrabFlag == false)
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            if (isFound)
                            {
                                retVal = module;
                                break;
                            }
                        }
                    }

                    //if (Assembly != null)
                    //{
                    //    if (this.Assembly is Assembly)
                    //    {
                    //        Assembly assembly = (Assembly)this.Assembly;
                    //        foreach (var name in assembly.GetTypes())
                    //        {
                    //            Type[] t = name.GetInterfaces();
                    //            foreach (var type in t)
                    //            {
                    //                foreach (var module in EmulImageParam.ActionModules)
                    //                {
                    //                    if (module.ModuleName == type.ToString())
                    //                    {
                    //                        retVal = module;
                    //                        return retVal;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void ContinuousGrab()
        {
            try
            {
                bContinousGrab = true;

                if (imggrabtimer != null)
                {
                    imggrabtimer.Dispose();
                }

                if (GrabMode == EnumGrabberMode.AUTO || GrabMode == EnumGrabberMode.DEFAULT)
                {
                    if (EnableVisionProc)
                    {
                        GrabActionModule = ExistEmulGrabInterface();

                        if (GrabActionModule != null)
                        {
                            if (!IsFocusing)
                            {
                                for (int index = 0; index < GrabActionModule.ContinousGrabImages.Count; index++)
                                {
                                    if (GrabActionModule.ContinousGrabImages[index].GrabFlag != true)
                                    {
                                        GrabActionModule.ContinousGrabImages[index].GrabFlag = true;
                                        ContinusGrabModule = GrabActionModule.ContinousGrabImages[index];
                                        break;
                                    }
                                }
                            }

                            else
                            {
                                for (int index = 0; index < GrabActionModule.FocusingGrabImages.Count; index++)
                                {
                                    if (GrabActionModule.FocusingGrabImages[index].GrabFlag != true)
                                    {
                                        GrabActionModule.FocusingGrabImages[index].GrabFlag = true;
                                        ContinusFocusingModule = GrabActionModule.FocusingGrabImages[index];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (GrabMode == EnumGrabberMode.MANUAL)
                {
                    if (!LoadImageFilesFlag)
                    {
                        LoadImageFilesFlag = true;
                        LoadImageFilesFlag = false;
                    }

                    if (IsFocusing)
                    {
                        clearUserimageFiles();
                        ImageBuffer img = WaitSingleShot().Result;
                    }
                }

                imggrabtimer = new System.Timers.Timer(GrabbingInterValInms);
                imggrabtimer.Elapsed += ImageGrabbedThread;
                imggrabtimer.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. VisionGrabberEmul - ContinousGrab () : Error occurred.");
                throw err;
            }
        }

        public void DigitizerHalt()
        {
            try
            {
                bContinousGrab = false;
                if (imggrabtimer != null)
                {
                    imggrabtimer?.Stop();
                    imggrabtimer.Close();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public void StopContinuousGrab()
        {
            try
            {
                if (imggrabtimer != null)
                {
                    imggrabtimer.Enabled = false;
                    imggrabtimer.Elapsed -= ImageGrabbedThread;
                    imggrabtimer.Stop();
                    imggrabtimer.Close();
                    imggrabtimer = null;
                }

                ActionModule actionModule = null;

                if (GrabActionModule != null)
                {
                    actionModule = EmulImageParam.ActionModules.ToList<ActionModule>().Find(item => item.GetHashCode() == GrabActionModule.GetHashCode());
                }

                if (ContinusGrabModule != null)
                {
                    if (actionModule.ContinousGrabImages[actionModule.ContinousGrabImages.Count() - 1].GrabFlag == true)
                    {
                        foreach (var module in actionModule.ContinousGrabImages)
                        {
                            foreach (var grabinfo in module.GrabInfos)
                            {
                                grabinfo.GrabFlag = false;
                            }
                        }
                    }
                }
                else if (ContinusFocusingModule != null)
                {

                    if (actionModule.FocusingGrabImages[actionModule.FocusingGrabImages.Count() - 1].GrabFlag == true)
                    {
                        foreach (var module in actionModule.FocusingGrabImages)
                        {
                            foreach (var grabinfo in module.GrabInfos)
                            {
                                grabinfo.GrabFlag = false;
                            }
                        }
                    }
                }


                ContinusGrabModule = null;
                ContinusFocusingModule = null;
                bContinousGrab = false;

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. VisionGrabberEmul - StopContinousGrab () : Error occurred.");
                LoggerManager.Exception(err);
                throw err;
            }
        }
        bool disposed = false;
        public void Dispose()
        {
            try
            {
                Dispose(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    try
                    {
                        // Code to dispose the managed resources of the class
                        if (bContinousGrab | ImageGrabbed != null)
                            StopContinuousGrab();
                        disposed = true;
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

        }
        public ImageBuffer GetCurGrabImage()
        {
            return CopyImage;
        }

        private void ImageGrabbedThread(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!LoadImageFilesFlag)
                {
                    lock (lockObject)
                    {
                        if (EnableVisionProc)
                        {
                            if (ContinusGrabModule != null)
                            {
                                for (int index = 0; index < ContinusGrabModule.GrabInfos.Count; index++)
                                {
                                    if (ContinusGrabModule.GrabInfos[index].GrabFlag != true)
                                    {
                                        ContinusGrabModule.GrabInfos[index].GrabFlag = true;

                                        if (index == ContinusGrabModule.GrabInfos.Count() - 1)
                                        {
                                            foreach (var grabinfo in ContinusGrabModule.GrabInfos)
                                            {
                                                grabinfo.GrabFlag = false;
                                            }
                                        }

                                        Image.Buffer = this.VisionManager().LoadImageFile(ContinusGrabModule.GrabInfos[index].Path).Buffer;
                                        Image = this.VisionManager().LoadImageFile(ContinusGrabModule.GrabInfos[index].Path);
                                    }
                                }
                            }
                            else if (ContinusFocusingModule != null)
                            {
                                for (int index = 0; index < ContinusFocusingModule.GrabInfos.Count; index++)
                                {
                                    if (ContinusFocusingModule.GrabInfos[index].GrabFlag != true)
                                    {
                                        ContinusFocusingModule.GrabInfos[index].GrabFlag = true;

                                        if (index == ContinusFocusingModule.GrabInfos.Count() - 1)
                                        {
                                            foreach (var grabinfo in ContinusFocusingModule.GrabInfos)
                                            {
                                                grabinfo.GrabFlag = false;
                                            }
                                        }

                                        Image.Buffer = this.VisionManager().LoadImageFile(ContinusFocusingModule.GrabInfos[index].Path).Buffer;
                                    }
                                }
                            }
                            else
                            {
                                if (Image.Buffer == null)
                                {
                                    Image.Buffer = new byte[iGrabSizeY * iGrabSizeY];
                                }
                            }
                        }
                        else
                        {
                            if (Image.Buffer == null)
                            {
                                Image.Buffer = new byte[iGrabSizeY * iGrabSizeY];
                            }
                        }

                        Image = GrabSizeConvert(Image);

                        if (Image != null)
                        {
                            ImageGrabbed(Image);
                        }
                        else
                        {
                            Debugger.Break();
                        }
                    }
                }
                else
                {
                    lock (ImageLockObject)
                    {
                        if (!IsFocusing)
                        {

                            if (Images.Count != 0)
                            {
                                if (++ContinusGrabImageIndex >= Images.Count)
                                {
                                    ContinusGrabImageIndex = 0;
                                }
                                Images[ContinusGrabImageIndex] =
                                    GrabSizeConvert(Images[ContinusGrabImageIndex]);
                                ImageGrabbed(Images[ContinusGrabImageIndex]);
                            }
                        }

                        else
                        {
                            if (Images.Count != 0)
                            {
                                if (SingleGrabImageIndex == -1)
                                    SingleGrabImageIndex = 0;
                                CopyImage = Images[SingleGrabImageIndex];
                                CopyImage = GrabSizeConvert(CopyImage);
                                ImageGrabbed(Images[ContinusGrabImageIndex]);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. VisionGrabberEmul - ImageGrabbedThread () : Error occurred.");
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                if (bContinousGrab != false)
                {
                    AreUpdateEvent.Set();
                }
            }
        }

        public ImageBuffer SingleGrab()
        {
            try
            {
                if (!bContinousGrab)
                {
                    ActionModule module = ExistEmulGrabInterface();

                    if (module != null)
                    {
                        if (!IsFocusing)
                        {
                            for (int index = 0; index < module.SingleGrabImage.GrabInfos.Count; index++)
                            {
                                if (module.SingleGrabImage.GrabInfos[index].GrabFlag != true)
                                {
                                    module.SingleGrabImage.GrabInfos[index].GrabFlag = true;

                                    // TODO : CHECK
                                    //if (index == module.SingleGrabImage.GrabInfos.Count() - 1)
                                    //{
                                    //    foreach (var grabinfo in module.SingleGrabImage.GrabInfos)
                                    //    {
                                    //        grabinfo.GrabFlag = false;
                                    //    }
                                    //}

                                    return this.VisionManager().LoadImageFile(module.SingleGrabImage.GrabInfos[index].Path);
                                }
                            }
                        }
                        else
                        {
                            for (int index = 0; index < module.FocusingGrabImages.Count; index++)
                            {
                                for (int jndex = 0; jndex < module.FocusingGrabImages[index].GrabInfos.Count; jndex++)
                                {
                                    if (module.FocusingGrabImages[index].GrabInfos[jndex].GrabFlag != true)
                                    {
                                        module.FocusingGrabImages[index].GrabInfos[jndex].GrabFlag = true;

                                        if (jndex == module.FocusingGrabImages[index].GrabInfos.Count() - 1)
                                        {
                                            foreach (var grabinfo in module.FocusingGrabImages[index].GrabInfos)
                                            {
                                                grabinfo.GrabFlag = false;
                                            }
                                        }

                                        IsFocusing = false;

                                        return this.VisionManager().LoadImageFile(module.FocusingGrabImages[index].GrabInfos[jndex].Path);
                                    }
                                }
                            }
                        }

                        IsFocusing = false;

                        return null;
                    }
                    else
                    {
                        if (GrabMode == EnumGrabberMode.AUTO || GrabMode == EnumGrabberMode.DEFAULT)
                        {
                            if (Images.Count != 0)
                            {
                                if (IsFocusing)
                                {
                                    if (SingleGrabImageIndex == -1)
                                    {
                                        SingleGrabImageIndex = 0;
                                    }

                                    CopyImage = Images[SingleGrabImageIndex];
                                    CopyImage = GrabSizeConvert(CopyImage);
                                }
                                else
                                {
                                    if (++SingleGrabImageIndex >= Images.Count)
                                    {
                                        SingleGrabImageIndex = 0;
                                    }

                                    CopyImage = Images[SingleGrabImageIndex];
                                    CopyImage = GrabSizeConvert(CopyImage);
                                }
                            }
                            else
                            {
                                if (Image.Buffer == null || Image.Buffer.Count<byte>() != (iGrabSizeY * iGrabSizeY))
                                {
                                    Image.Buffer = new byte[iGrabSizeY * iGrabSizeY];
                                }

                                Image = GrabSizeConvert(Image);
                                Image.CopyTo(CopyImage);
                            }

                            return new ImageBuffer(CopyImage);
                        }
                        else if (GrabMode == EnumGrabberMode.MANUAL)
                        {
                            clearUserimageFiles();
                            ImageBuffer img = null;

                            img = WaitSingleShot().Result;
                            img = GrabSizeConvert(img);
                            img.CopyTo(CopyImage);

                            return new ImageBuffer(CopyImage);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. VisionGrabberEmul - SingleGrab () : Error occurred.");
                LoggerManager.Exception(err);
                throw err;
            }

        }

        public EventCodeEnum LoadUserImageFiles(List<ImageBuffer> imgs)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Images.Clear();

                foreach (var img in imgs)
                {
                    img.Band = 1;
                }

                Images = imgs;
                LoadImageFilesFlag = true;
                SingleGrabImageIndex = 0;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum clearUserimageFiles()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoadImageFilesFlag = false;
                Images.Clear();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public delegate object MyDelegate();
        private bool WaitGrabFlag;
        private bool StopWaitGrabFlag;
        public async Task<ImageBuffer> WaitSingleShot()
        {

            try
            {
                StackTrace stackTrace = new StackTrace(true);

                StringBuilder sb = new StringBuilder();
                List<string> sbs = new List<string>();
                foreach (StackFrame frame in stackTrace.GetFrames())
                {
                    string str
                         = " Method Name: " + frame.GetMethod().Name + " || File Name:" + frame.GetMethod().Module.Name + " Line No: " +
                                       frame.GetFileLineNumber();
                    sb.AppendLine(str);
                    sbs.Add(str);
                }


                string dirname = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

                ImageBuffer img = new ImageBuffer();
                Image.CopyTo(img);
                img.Buffer = null;
                this.VisionManager().WirteTextToBuffer(sbs, img);

                WaitGrabFlag = true;
                StopWaitGrabFlag = false;
#pragma warning disable 4014
                //brett// esc key 입력을 받기 위한 task 에서 stopwaitgrabflag를 설정함, 그러므로 await 하면 안됨
                WaitEscapeKey();
#pragma warning restore 4014
                await Task.Run(async () =>
                {
                    while (WaitGrabFlag)
                    {
                        if (LoadImageFilesFlag)
                        {
                            WaitGrabFlag = false;

                            if (StopWaitGrabFlag)
                            {
                                Image.Buffer = new byte[iGrabSizeY * iGrabSizeY];
                                Image.ErrorCode = EventCodeEnum.GRAB_USER_CANCEL;
                                Images.Add(Image);
                            }
                            break;
                        }
                        await Task.Delay(0);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Images[0];
        }

        public async Task WaitEscapeKey()
        {
            try
            {
                await Task.Run(async () =>
                {
                    while (WaitGrabFlag)
                    {
                        await Application.Current.Dispatcher.Invoke(async () =>
                        {
                            if (Keyboard.IsKeyDown(Key.Escape))
                            {
                                WaitGrabFlag = false;
                                Image.Buffer = new byte[iGrabSizeY * iGrabSizeY];
                                Images.Add(Image);
                                LoadImageFilesFlag = true;
                            }
                            await Task.Delay(0);
                        });
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ImageBuffer GrabSizeConvert(ImageBuffer img)
        {
            ImageBuffer retval = null;

            try
            {
                ImageBuffer tmpbuf = this.VisionManager()?.ReduceImageSize(img, 0, 0, iGrabSizeX, iGrabSizeY);

                if (tmpbuf != null)
                {
                    img = tmpbuf;
                    img.SizeX = iGrabSizeX;
                    img.SizeY = iGrabSizeY;
                }

                retval = img;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void StopWaitGrab()
        {
            try
            {
                LoadImageFilesFlag = true;
                StopWaitGrabFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            bool retval = false;

            try
            {
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
