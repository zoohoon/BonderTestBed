using LogModule;
using Newtonsoft.Json;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static ProberInterfaces.Param.PinSearchParameter;

namespace AlignKeyFocusing
{
    public class SinglePinAlignKeyFocusingModule : IProcessingModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        public SubModuleMovingStateBase MovingState { get; set; }

        private SubModuleStateBase _AlignModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _AlignModuleState; }
            set
            {
                if (value != _AlignModuleState)
                {
                    _AlignModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
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

        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "PreRun() : Error occured.");
                LoggerManager.Exception(err);

            }

            return retVal;
        }

        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }

        public void DeInitModule()
        {
        }

        public EventCodeEnum DoClearData()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoExecute()
        {
            EventCodeEnum errcode = EventCodeEnum.UNDEFINED;

            try
            {
                var singlepinalign = this.PinAligner().SinglePinAligner;
                ICamera cam = this.VisionManager().GetCam(singlepinalign.FocusingParam.FocusingCam.Value);

                singlepinalign.FocusingParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;

                IPinData pindata = this.PinAligner().SinglePinAligner.AlignPin;

                AlignKeyInfo alignkeyinfo = pindata.PinSearchParam.AlignKeyHigh[singlepinalign.AlignKeyIndex];

                if(alignkeyinfo.PatternIfo.LightParams != null)
                {
                    foreach (var light in alignkeyinfo.PatternIfo.LightParams)
                    {
                        cam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }

                double ratioX = cam.GetRatioX();
                double ratioY = cam.GetRatioY();

                double focusingROIX = alignkeyinfo.FocusingAreaSizeX.Value / ratioX;
                double focusingROIY = alignkeyinfo.FocusingAreaSizeY.Value/ ratioY;

                double blobsizeX = alignkeyinfo.BlobSizeX.Value / ratioX;
                double blobsizeY = alignkeyinfo.BlobSizeY.Value / ratioY;

                double focuisngrange = alignkeyinfo.FocusingRange.Value;

                // Set FocusingROI

                bool BlobExtensionFlag = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyFocusingExtension.Value;

                int RealFocusingAreaX = 0;
                int RealFocusingAreaY = 0;

                int ROISizeX = 0;
                int ROISizeY = 0;

                int OffsetX = 0;
                int OffsetY = 0;

                if (BlobExtensionFlag == true)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] DoExecute() : Used Focusing Extension Area.");

                    double ExtensionWidth = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyFocusingExtensionWidth.Value;
                    double ExtensionHeight = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyFocusingExtensionHeight.Value;

                    int ExtensionWidthPixel = 0;
                    int ExtensionHeightPixel = 0;

                    if (ExtensionWidth > 0)
                    {
                        ExtensionWidthPixel = Convert.ToInt32(ExtensionWidth / ratioX);
                    }

                    if (ExtensionHeight > 0)
                    {
                        ExtensionHeightPixel = Convert.ToInt32(ExtensionHeight / ratioY);
                    }

                    int SignX = alignkeyinfo.AlignKeyPos.X.Value < 0 ? -1 : 1;
                    int SignY = alignkeyinfo.AlignKeyPos.Y.Value > 0 ? -1 : 1;

                    ROISizeX = Convert.ToInt32(alignkeyinfo.BlobRoiSizeX.Value / ratioX);
                    ROISizeY = Convert.ToInt32(alignkeyinfo.BlobRoiSizeY.Value / ratioY);

                    if (SignX == -1)
                    {
                        OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2) + (SignX * ExtensionWidthPixel);
                    }
                    else
                    {
                        OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                    }

                    if (SignY == -1)
                    {
                        OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2) + (SignY * ExtensionHeightPixel);
                    }
                    else
                    {
                        OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);
                    }

                    RealFocusingAreaX = ROISizeX + ExtensionWidthPixel;
                    RealFocusingAreaY = ROISizeY + ExtensionHeightPixel;
                }
                else
                {
                    ROISizeX = Convert.ToInt32(alignkeyinfo.BlobRoiSizeX.Value / ratioX);
                    ROISizeY = Convert.ToInt32(alignkeyinfo.BlobRoiSizeY.Value / ratioY);

                    OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                    OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);

                    RealFocusingAreaX = ROISizeX;
                    RealFocusingAreaY = ROISizeY;
                }

                //singlepinalign.OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(focusingROIX)) / 2;
                //singlepinalign.OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(focusingROIY)) / 2;

                singlepinalign.FocusingParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, RealFocusingAreaX, RealFocusingAreaY);
                singlepinalign.FocusingParam.FocusRange.Value = focuisngrange;
                singlepinalign.FocusingParam.CheckDualPeak.Value = false;

                LoggerManager.Debug($"[{this.GetType().Name}] DoExecute() : [Focusing Information Start]");
                LoggerManager.Debug($"[{this.GetType().Name}] DoExecute() : ROI (Pixel) => Offset(X : {OffsetX}, Y : {OffsetY}), Size(X : {RealFocusingAreaX}, Y : {RealFocusingAreaY})");
                LoggerManager.Debug($"[{this.GetType().Name}] DoExecute() : Range => {singlepinalign.FocusingParam.FocusRange.Value} um");
                LoggerManager.Debug($"[{this.GetType().Name}] DoExecute() : [Focusing Information End]");

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(singlepinalign.AlignPin.PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                }
                else if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                {
                    int Foundgraylevel = alignkeyinfo.FoundGrayLevelForFocusing.Value;

                    string SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PINALIGNER, true, "SINGLEPIN", "HIGHKEYFOCUSING");

                    errcode = singlepinalign.Focusing.Focusing_Retry(singlepinalign.FocusingParam, false, false, false, this, TargetGrayLevel: Foundgraylevel, ForcedApplyAutolight: false, SaveFailPath: SaveBasePath);

                    if (errcode == EventCodeEnum.NONE)
                    {
                        // When pin source is registraion, the value can be changed.
                        if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                        {
                            ImageBuffer grabbuffer = this.VisionManager().SingleGrab(EnumProberCam.PIN_HIGH_CAM, this);
                            this.VisionManager().VisionProcessing.GetGrayLevel(ref grabbuffer);

                            alignkeyinfo.FoundGrayLevelForFocusing.Value = grabbuffer.GrayLevelValue;
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Align key Focusing Ok");
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Align key Focusing Failed");
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return EventCodeEnum.NONE;

        }

        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();

        }

        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[{this.GetType().Name}] InitModule() : Init Module Start");
            try
            {
                if (Initialized == false)
                {
                    //AlignInfo = (PinAlignInfo)this.PinAligner().AlignInfo;

                    SubModuleState = new SubModuleIdleState(this);

                    MovingState = new SubModuleStopState(this);

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
                LoggerManager.Debug($"[{this.GetType().Name}] InitModule() : Init Module Error");
                LoggerManager.Exception(err);

                throw err;
            }
            LoggerManager.Debug($"[{this.GetType().Name}] InitModule() : Init Module Done");
            return retval;
        }

        public bool IsExecute()
        {
            return true;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                DoRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum UpdateState()
        {
            return EventCodeEnum.NONE;
        }
    }
}
