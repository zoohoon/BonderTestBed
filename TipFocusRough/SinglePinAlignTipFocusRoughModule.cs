using LogModule;
using Newtonsoft.Json;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace TipFocusRough
{
    public class SinglePinAlignTipFocusRoughModule : IProcessingModule
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var singlepinalign = this.PinAligner().SinglePinAligner;
            ICamera cam = this.VisionManager().GetCam(singlepinalign.FocusingParam.FocusingCam.Value);

            try
            {
                bool ParamValid = true;

                if (singlepinalign.AlignPin.PinSearchParam.LightForTip.Count <= 0)
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] DoExecute(), LightForTip parameter is invalid. LightForTip Count = {singlepinalign.AlignPin.PinSearchParam.LightForTip.Count}");
                }
                else
                {
                    foreach (var light in singlepinalign.AlignPin.PinSearchParam.LightForTip)
                    {
                        cam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
                //IFocusParameter tipRoughFocusParam = PinAlignParam.PinHighAlignTipRoughParam.FocusingParam;

                //if ((tipRoughFocusParam as FocusParameter).LightParams != null)
                //{
                //    foreach (var light in (tipRoughFocusParam as FocusParameter).LightParams)
                //    {
                //        cam.SetLight(light.Type.Value, light.Value.Value);
                //    }
                //}

                //singlepinalign.NewPinPos = new PinCoordinate(cam.GetCurCoordPos());
                //IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                //var posX = pininfo.AbsPos.X.Value;
                //var posY = pininfo.AbsPos.Y.Value;
                //var posZ = pininfo.AbsPos.Z.Value;

                //if (posZ > this.StageSupervisor().PinMaxRegRange)
                //{
                //    posZ = this.StageSupervisor().PinMaxRegRange;
                //}

                // SearchArea 값의 유효한 범위 : 0보다 크고, GrabSize 보다 작아야 한다.
                if ((singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width <= 0) || (singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width >= cam.Param.GrabSizeX.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] DoExecute(), SearchArea parameter is invalid. Width = {singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width}");
                }

                if ((singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height <= 0) || (singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height >= cam.Param.GrabSizeY.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] DoExecute(), SearchArea parameter is invalid. Height = {singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height}");
                }

                if (ParamValid == true)
                {
                    singlepinalign.OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width)) / 2;
                    singlepinalign.OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height)) / 2;

                    singlepinalign.FocusingParam.FocusingROI.Value = new System.Windows.Rect(singlepinalign.OffsetX, singlepinalign.OffsetY, singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width, singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height);
                    singlepinalign.FocusingParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);
                    //singlepinalign.FocusingParam.FocusRange.Value = tipRoughFocusParam.FocusRange.Value;


                    LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Rough Focusing Parameter ROI X = " + singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width.ToString() + " Y = " + singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height.ToString());
                    LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Rough Focusing Parameter Focusing Range = " + singlepinalign.FocusingParam.FocusRange.Value.ToString());

                    this.VisionManager().StartGrab(cam.GetChannelType(), this);

                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(singlepinalign.AlignPin.PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                        retVal = EventCodeEnum.NONE;
                    }
                    else if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Rough Focusing Start");

                        string SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PINALIGNER, true, "SINGLEPIN", "HIGHPINTIPFOCUSING_ROUGH");

                        PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE;

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.VerticalType)
                        {
                            peakSelectionStrategy = PeakSelectionStrategy.LOWEST;
                        }

                        retVal = singlepinalign.Focusing.Focusing_Retry(singlepinalign.FocusingParam, false, false, false, this, SaveFailPath: SaveBasePath, peakSelectionStrategy: peakSelectionStrategy);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Rough Focusing Fail Error Code = " + retVal.ToString());

                            retVal = EventCodeEnum.PIN_FOCUS_FAILED;
                        }
                        else
                        {
                            singlepinalign.NewPinPos = new PinCoordinate(cam.GetCurCoordPos());

                            LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Rough Focusing Ok");
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Rough Focusing Start");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;

                LoggerManager.Exception(err);
            }

            return retVal;
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
            LoggerManager.Debug($"[PinHighAli{this.GetType().Name}gnModule] InitModule() : Init Module Done");
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
