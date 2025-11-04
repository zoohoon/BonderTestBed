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

namespace TipBlob
{
    public class SinglePinAlignTipBlobModule : IProcessingModule
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

            int tmpThreshVal = 0;

            double PosX = 0;
            double PosY = 0;

            try
            {
                bool ParamValid = true;
                bool isDilation = this.PinAlignParam.EnableDilation.Value;

                if (isDilation)
                {
                    LoggerManager.Debug($"[SinglePinAlignTipBlobModule] DoExecute(), isDilation = {isDilation}");
                }

                if (singlepinalign.AlignPin.PinSearchParam.LightForTip.Count <= 0)
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[SinglePinAlignTipBlobModule] DoExecute(), LightForTip parameter is invalid. LightForTip Count = {singlepinalign.AlignPin.PinSearchParam.LightForTip.Count}");
                }
                else
                {
                    foreach (var light in singlepinalign.AlignPin.PinSearchParam.LightForTip)
                    {
                        cam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }

                // MinBlobSize & MaxBlobSize 값의 유효한 범위 : 0보다 크고, GrabSize 보다 작아야 한다.
                if ((singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value >= cam.Param.GrabSizeX.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[SinglePinAlignTipBlobModule] DoExecute(), MinBlobSizeX parameter is invalid. MinBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value}");
                }

                if ((singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[SinglePinAlignTipBlobModule] DoExecute(), MinBlobSizeY parameter is invalid. MinBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value}");
                }

                if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value >= cam.Param.GrabSizeX.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[SinglePinAlignTipBlobModule] DoExecute(), MaxBlobSizeX parameter is invalid. MaxBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value}");
                }

                if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[SinglePinAlignTipBlobModule] DoExecute(), MaxBlobSizeY parameter is invalid. MaxBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value}");
                }

                if (ParamValid == true)
                {
                    singlepinalign.BlobMinSize = singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value * singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value;
                    singlepinalign.BlobMaxSize = singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value * singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value;

                    if (isDilation)
                    {
                        if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                        {
                            tmpThreshVal = -1;
                        }
                        else
                        {
                            tmpThreshVal = singlepinalign.AlignPin.PinSearchParam.BlobThreshold.Value;
                        }
                    }
                    else
                    {
                        // Auto threshold 적용
                        if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                        {
                            int sx = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width);
                            int sy = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height);

                            int OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(sx / 2);
                            int OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(sy / 2);

                            tmpThreshVal = singlepinalign.getOtsuThreshold(cam.GetChannelType(), OffsetX, OffsetY, sx, sy);
                        }
                        else
                        {
                            tmpThreshVal = singlepinalign.AlignPin.PinSearchParam.BlobThreshold.Value;
                        }
                    }

                    bool SaveOrginalFlag = this.FileManager().FileManagerParam.PinAlignHighKeySaveOriginalImage.Value;
                    bool SavePassFlag = this.FileManager().FileManagerParam.PinAlignHighKeySavePassImage.Value;
                    bool SaveFailFlag = this.FileManager().FileManagerParam.PinAlignHighKeySaveFailImage.Value;

                    string SaveOriginalImageFullPath = string.Empty;
                    string SavePassImageFullPath = string.Empty;
                    string SaveFailImageFullPath = string.Empty;

                    int foundgraylevel = 0;

                    int SizeX = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width);
                    int SizeY = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height);

                    int BlobOffsetX = (int)cam.GetGrabSizeWidth() / 2 - Convert.ToInt32(SizeX) / 2;
                    int BlobOffsetY = (int)cam.GetGrabSizeHeight() / 2 - Convert.ToInt32(SizeY) / 2;

                    BlobResult blobresult = this.VisionManager().FindBlob(cam.GetChannelType(), ref PosX, ref PosY, ref foundgraylevel, tmpThreshVal, singlepinalign.BlobMinSize, singlepinalign.BlobMaxSize, BlobOffsetX, BlobOffsetY, SizeX, SizeY, isDilation);

                    if (SaveOrginalFlag == true)
                    {
                        SaveOriginalImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "TIPBLOB", $"PinNo#_{singlepinalign.AlignPin.PinNum}_Threshold_{tmpThreshVal}_ORIGINAL");
                        this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveOriginalImageFullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                    }

                    if (blobresult?.DevicePositions?.Count == 1)
                    {
                        if (SavePassFlag == true)
                        {
                            SavePassImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "TIPBLOB", $"PinNo#_{singlepinalign.AlignPin.PinNum}_Threshold_{tmpThreshVal}_PASS");
                            this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SavePassImageFullPath, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Blob Search Ok");

                        this.VisionManager().StartGrab(cam.GetChannelType(), this);

                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                        {
                            singlepinalign.NewPinPos = new PinCoordinate(singlepinalign.ConvertPosPixelToPin(cam, singlepinalign.AlignPin.AbsPos, PosX, PosY));
                            this.StageSupervisor().StageModuleState.PinHighViewMove(singlepinalign.NewPinPos.GetX(), singlepinalign.NewPinPos.GetY(), singlepinalign.NewPinPos.GetZ());
                        }

                        retVal = EventCodeEnum.NONE;
                    }
                    else if (blobresult?.DevicePositions?.Count == 0)
                    {
                        if (SaveFailFlag == true)
                        {
                            SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "TIPBLOB", $"PinNo#_{singlepinalign.AlignPin.PinNum}_Threshold_{tmpThreshVal}_FAIL");
                            this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                            SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "SINGLEPIN", "TIPBLOB", $"PinNo#_{singlepinalign.AlignPin.PinNum}_Threshold_{tmpThreshVal}_ORI");
                            this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule() : Needle Blob Search Fail");
                        retVal = EventCodeEnum.VISION_BLOB_NOT_FOUND;
                        this.NotifyManager().Notify(retVal);
                    }
                    else
                    {
                        if (SaveFailFlag == true)
                        {
                            SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "TIPBLOB", $"PinNo#_{singlepinalign.AlignPin.PinNum}_Threshold_{tmpThreshVal}_FAIL");
                            this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                            SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "SINGLEPIN", "TIPBLOB", $"PinNo#_{singlepinalign.AlignPin.PinNum}_Threshold_{tmpThreshVal}_ORI");
                            this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Needle Blob Search Fail");
                        this.NotifyManager().Notify(EventCodeEnum.PIN_ALIGN_FAILED);

                        retVal = EventCodeEnum.PIN_ALIGN_FAILED;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.PARAM_ERROR;
                }

                singlepinalign.NewPinPos = new PinCoordinate(cam.GetCurCoordPos());
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
