using LogModule;
using Newtonsoft.Json;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static ProberInterfaces.Param.PinSearchParameter;

namespace BaseFocusing
{
    public class SignlePinAlignBaseFocusingModule : IProcessingModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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
                if (PinAlignParam.PinHighAlignBaseFocusingParam.BaseFocsuingEnable.Value == BaseFocusingEnable.DISABLE)
                {
                    errcode = EventCodeEnum.NONE;
                    LoggerManager.Debug($"[SignlePinAlignBaseFocusingModule]DoExecute(): BaseFocsuingEnable is DISABLE");
                }
                else
                {
                    var singlepinalign = this.PinAligner().SinglePinAligner;
                    ICamera cam = this.VisionManager().GetCam(singlepinalign.FocusingParam.FocusingCam.Value);

                    singlepinalign.FocusingParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;

                    IPinData pindata = this.PinAligner().SinglePinAligner.AlignPin;

                    BaseInfo baseinfo = pindata.PinSearchParam.BaseParam;

                    IFocusParameter baseFocusParam = PinAlignParam.PinHighAlignBaseFocusingParam.FocusingParam;

                    if ((baseFocusParam as FocusParameter).LightParams != null)
                    {
                        foreach (var light in (baseFocusParam as FocusParameter).LightParams)
                        {
                            cam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }

                    LoggerManager.Debug($"[SinglePinAlignBaseFocusingModule] DoExecute() : [Focusing Information Start]");
                    LoggerManager.Debug($"[SinglePinAlignBaseFocusingModule] DoExecute() : ROI (Pixel) => " +
                        $"Offset(X : {baseFocusParam.FocusingROI.Value.X}, " +
                        $"Y : {baseFocusParam.FocusingROI.Value.Y}), " +
                        $"Size(X : {baseFocusParam.FocusingROI.Value.Width}, " +
                        $"Y : {baseFocusParam.FocusingROI.Value.Height})");
                    LoggerManager.Debug($"[SinglePinAlignBaseFocusingModule] DoExecute() : Range => {baseFocusParam.FocusRange.Value} um");
                    LoggerManager.Debug($"[SinglePinAlignBaseFocusingModule] DoExecute() : [Focusing Information End]");

                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(singlepinalign.AlignPin.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        errcode = EventCodeEnum.NONE;
                    }
                    else if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                    {
                        int Foundgraylevel = baseinfo.FoundGrayLevelForFocusing.Value;

                        string SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PINALIGNER, true, "SINGLEPIN", "BASEFOCUSING");

                        PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE;

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.VerticalType)
                        {
                            peakSelectionStrategy = PeakSelectionStrategy.HIGHEST;
                        }

                        errcode = singlepinalign.Focusing.Focusing_Retry(baseFocusParam, false, false, false, this, TargetGrayLevel: Foundgraylevel, ForcedApplyAutolight: false, SaveFailPath: SaveBasePath, peakSelectionStrategy: peakSelectionStrategy);

                        if (errcode == EventCodeEnum.NONE)
                        {
                            // When pin source is registraion, the value can be changed.
                            if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                            {
                                ImageBuffer grabbuffer = this.VisionManager().SingleGrab(EnumProberCam.PIN_HIGH_CAM, this);
                                this.VisionManager().VisionProcessing.GetGrayLevel(ref grabbuffer);

                                baseinfo.FoundGrayLevelForFocusing.Value = grabbuffer.GrayLevelValue;
                            }

                            PinCoordinate curpos = new PinCoordinate(cam.GetCurCoordPos());

                            // Base Focusing 후 Tip위치로 이동
                            this.StageSupervisor().StageModuleState.PinHighViewMove(curpos.X.Value - baseinfo.BaseOffsetX, curpos.Y.Value - baseinfo.BaseOffsetY, curpos.Z.Value - baseinfo.DistanceBaseAndTip);

                            baseinfo.BasePos.Z.Value = curpos.Z.Value;

                            LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Base Focusing Ok");
                        }
                        else
                        {
                            LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Base Focusing Failed");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return errcode;
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[SinglePinALignBaseFocusingModule] InitModule() : Init Module Start");

                if (Initialized == false)
                {
                    SubModuleState = new SubModuleIdleState(this);

                    MovingState = new SubModuleStopState(this);

                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retVal = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {

                LoggerManager.Debug($"[SinglePinALignBaseFocusingModule] InitModule() : Init Module Error");
                LoggerManager.Exception(err);

                throw err;
            }
            LoggerManager.Debug($"[SinglePinALignKeyFocusingModule] InitModule() : Init Module Done");
            return retVal;
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
    }
}
