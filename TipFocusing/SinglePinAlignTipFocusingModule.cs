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

namespace TipFocusing
{
    public class SinglePinAlignTipFocusingModule : IProcessingModule
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var singlepinalign = this.PinAligner().SinglePinAligner;
            ICamera cam = this.VisionManager().GetCam(singlepinalign.FocusingParam.FocusingCam.Value);

            IPinData pindata = this.PinAligner().SinglePinAligner.AlignPin;
            BaseInfo baseinfo = pindata.PinSearchParam.BaseParam;

            try
            {
                bool ParamValid = true;

                if (singlepinalign.AlignPin.PinSearchParam.LightForTip.Count <= 0)
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[SinglePinAlignTipFocusingModule] DoExecute(), LightForTip parameter is invalid. LightForTip Count = {singlepinalign.AlignPin.PinSearchParam.LightForTip.Count}");
                }
                else
                {
                    foreach (var light in singlepinalign.AlignPin.PinSearchParam.LightForTip)
                    {
                        cam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
                //IFocusParameter tipFocusParam = PinAlignParam.PinHighAlignTipFocusingParam.FocusingParam;

                //if ((tipFocusParam as FocusParameter).LightParams != null)
                //{
                //    foreach (var light in (tipFocusParam as FocusParameter).LightParams)
                //    {
                //        cam.SetLight(light.Type.Value, light.Value.Value);
                //    }
                //}

                //this.VisionManager().StartGrab(cam.GetChannelType());

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                {
                    // MinBlobSize & MaxBlobSize 값의 유효한 범위 : 0보다 크고, GrabSize 보다 작아야 한다.
                    if ((singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value >= cam.Param.GrabSizeX.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[SinglePinAlignTipFocusingModule] DoExecute(), MinBlobSizeX parameter is invalid. MinBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value}");
                    }

                    if ((singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[SinglePinAlignTipFocusingModule] DoExecute(), MinBlobSizeY parameter is invalid. MinBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value}");
                    }

                    if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value >= cam.Param.GrabSizeX.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[SinglePinAlignTipFocusingModule] DoExecute(), MaxBlobSizeX parameter is invalid. MaxBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value}");
                    }

                    if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[SinglePinAlignTipFocusingModule] DoExecute(), MaxBlobSizeY parameter is invalid. MaxBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value}");
                    }

                    if(ParamValid == true)
                    {
                        var tipsizeX = Convert.ToInt32(Convert.ToInt32(((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value + singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value) / 2)));
                        var tipsizeY = Convert.ToInt32(Convert.ToInt32(((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value + singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value) / 2)));

                        singlepinalign.OffsetX = cam.Param.GrabSizeX.Value / 2 - (tipsizeX / 2);
                        singlepinalign.OffsetY = cam.Param.GrabSizeY.Value / 2 - (tipsizeY / 2);

                        //if (singlepinalign.AlignPin.PinSearchParam.TipSizeX.Value <= 0) singlepinalign.AlignPin.PinSearchParam.TipSizeX.Value = 20;
                        //if (singlepinalign.AlignPin.PinSearchParam.TipSizeY.Value <= 0) singlepinalign.AlignPin.PinSearchParam.TipSizeY.Value = 20;

                        //focusingParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, AlignPin.PinSearchParam.TipSizeX.Value, AlignPin.PinSearchParam.TipSizeY.Value);

                        

                        singlepinalign.FocusingParam.FocusingROI.Value = new System.Windows.Rect(singlepinalign.OffsetX, singlepinalign.OffsetY, tipsizeX, tipsizeY);
                        singlepinalign.FocusingParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);
                        //singlepinalign.FocusingParam.FocusRange.Value = tipFocusParam.FocusRange.Value;

                        //LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Tip Focusing Parameter ROI X = " + singlepinalign.AlignPin.PinSearchParam.TipSizeX.Value.ToString() + " Y = " + singlepinalign.AlignPin.PinSearchParam.TipSizeY.Value.ToString());
                        LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Tip Focusing Parameter ROI X = {tipsizeX}, Y = {tipsizeY}");
                        LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Tip Focusing Parameter Focusing Range = " + singlepinalign.FocusingParam.FocusRange.Value.ToString());

                        this.VisionManager().StartGrab(cam.GetChannelType(), this);

                        // TODO: 하드코딩..누군가 언제간 바꾸게 될 것이다. 2020-02-26 Alvin 예언
                        singlepinalign.FocusingParam.CheckDualPeak.Value = false;
                        singlepinalign.FocusingParam.FocusThreshold.Value = 5000;
                        singlepinalign.FocusingParam.PeakRangeThreshold.Value = 200;
                        singlepinalign.FocusingParam.DepthOfField.Value = 1;
                        // ann todo -> OutFocusLimit 주석
                        //singlepinalign.FocusingParam.OutFocusLimit.Value = 40;

                        LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Tip Focusing Start");

                        string SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PINALIGNER, true, "SINGLEPIN", "HIGHPINTIPFOCUSING");

                        PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE;

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.VerticalType)
                        {
                            peakSelectionStrategy = PeakSelectionStrategy.LOWEST;
                        }

                        retVal = singlepinalign.Focusing.Focusing_Retry(singlepinalign.FocusingParam, false, false, false, this, SaveFailPath: SaveBasePath, peakSelectionStrategy: peakSelectionStrategy);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Tip Focusing Fail Error Code = " + retVal.ToString());
                            retVal = EventCodeEnum.PIN_FOCUS_FAILED;
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                            LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Tip Focusing Ok");
                        }

                        //retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

                //newpinpos = new PinCoordinate(cam.GetCurCoordPos());
                singlepinalign.NewPinPos = new PinCoordinate(cam.GetCurCoordPos());

                if (PinAlignParam.PinHighAlignBaseFocusingParam.BaseFocsuingEnable.Value == BaseFocusingEnable.ENABLE)
                {
                    double BasePosZ = baseinfo.BasePos.Z.Value;
                    double NewDistance = (BasePosZ - singlepinalign.NewPinPos.Z.Value);

                    if (Math.Abs(NewDistance - baseinfo.DistanceBaseAndTip) > PinAlignParam.PinHighAlignBaseFocusingParam.FocusingTolerance.Value)
                    {
                        LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : BasePosZ [{BasePosZ}], NewPinPos.Z: [{singlepinalign.NewPinPos.Z.Value}]");
                        LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : [{Math.Abs(NewDistance - baseinfo.DistanceBaseAndTip)}], Base Tolerance: [{PinAlignParam.PinHighAlignBaseFocusingParam.FocusingTolerance.Value}]");
                        LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Needle Rough Focusing Fail, Base Tolerance");
                        retVal = EventCodeEnum.PIN_TIP_BASE_DISTANCE_TOLERANCE;
                    }
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
            LoggerManager.Debug($"[SinglePinALignKeyFocusingModule] InitModule() : Init Module Start");
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
                LoggerManager.Debug($"[SinglePinALignKeyFocusingModule] InitModule() : Init Module Error");
                LoggerManager.Exception(err);

                throw err;
            }
            LoggerManager.Debug($"[SinglePinALignKeyFocusingModule] InitModule() : Init Module Done");
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
