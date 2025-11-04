using LogModule;
using Newtonsoft.Json;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.State;
using ProberInterfaces.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;
using static ProberInterfaces.Param.PinSearchParameter;

namespace AlignKeyBlob
{
    public class SinglePinAlignKeyBlobModule : IProcessingModule
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
            EventCodeEnum errcode = EventCodeEnum.UNDEFINED;

            var singlepinalign = this.PinAligner().SinglePinAligner;

            ICamera cam = this.VisionManager().GetCam(singlepinalign.FocusingParam.FocusingCam.Value);

            int tmpThreshVal = 0;
            double PosX = 0;
            double PosY = 0;

            try
            {
                IPinData pindata = this.PinAligner().SinglePinAligner.AlignPin;
                bool isDilation = this.PinAlignParam.EnableDilation.Value;
                if (isDilation)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] DoExecute(), isDilation = {isDilation}");
                }
                AlignKeyInfo alignkeyinfo = pindata.PinSearchParam.AlignKeyHigh[singlepinalign.AlignKeyIndex];

                if (alignkeyinfo.PatternIfo.LightParams != null)
                {
                    foreach (var light in alignkeyinfo.PatternIfo.LightParams)
                    {
                        cam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }

                double ratioX = cam.GetRatioX();
                double ratioY = cam.GetRatioY();


                bool BlobExtensionFlag = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtension.Value;

                int RealSearchAreaX = 0;
                int RealSearchAreaY = 0;

                int ROISizeX = 0;
                int ROISizeY = 0;

                int OffsetX = 0;
                int OffsetY = 0;

                if (BlobExtensionFlag == true)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] DoExecute() : Used Focusing Extension Area.");

                    double ExtensionWidth = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionWidth.Value;
                    double ExtensionHeight = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionHeight.Value;

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

                    RealSearchAreaX = ROISizeX + ExtensionWidthPixel;
                    RealSearchAreaY = ROISizeY + ExtensionHeightPixel;
                }
                else
                {
                    ROISizeX = Convert.ToInt32(alignkeyinfo.BlobRoiSizeX.Value / ratioX);
                    ROISizeY = Convert.ToInt32(alignkeyinfo.BlobRoiSizeY.Value / ratioY);

                    OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                    OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);

                    RealSearchAreaX = ROISizeX;
                    RealSearchAreaY = ROISizeY;
                }

                if (alignkeyinfo.ImageBlobType == EnumThresholdType.AUTO)
                {
                    tmpThreshVal = singlepinalign.getOtsuThreshold(EnumProberCam.PIN_HIGH_CAM, OffsetX, OffsetY, RealSearchAreaX, RealSearchAreaY);
                }
                else
                {
                    tmpThreshVal = Convert.ToInt32(alignkeyinfo.BlobThreshold.Value);
                }

                double keysizex = alignkeyinfo.BlobSizeX.Value / ratioX;
                double keysizey = alignkeyinfo.BlobSizeY.Value / ratioY;

                //singlepinalign.BlobMinSize = Convert.ToInt32((keysizex * keysizey) * 0.5);
                //singlepinalign.BlobMaxSize = Convert.ToInt32((keysizex * keysizey) * 1.5);

                singlepinalign.BlobMinSize = Convert.ToInt32(alignkeyinfo.BlobSizeMin.Value / ratioX / ratioY);
                singlepinalign.BlobMaxSize = Convert.ToInt32(alignkeyinfo.BlobSizeMax.Value / ratioY / ratioX);

                //double marginsize = 1.0;

                //int inputSizeX = Convert.ToInt32(keysizex * (1.0 + marginsize));
                //int inputSizeY = Convert.ToInt32(keysizey * (1.0 + marginsize));

                LoggerManager.Debug($"[SinglePinAlignKeyBlobModule] DoExecute() : [Blob Information Start - (Unit : Pixel)]");

                LoggerManager.Debug($"[SinglePinAlignKeyBlobModule] DoExecute() : ROI => Offset(X : {OffsetX}, Y : {OffsetY}), Size(X : {RealSearchAreaX}, Y : {RealSearchAreaY})");
                LoggerManager.Debug($"[SinglePinAlignKeyBlobModule] DoExecute() : SIZE => X : {keysizex}, Y : {keysizey}");
                LoggerManager.Debug($"[SinglePinAlignKeyBlobModule] DoExecute() : FILTER => Min Area : {singlepinalign.BlobMinSize}, Max Area : {singlepinalign.BlobMaxSize}");
                LoggerManager.Debug($"[SinglePinAlignKeyBlobModule] DoExecute() : [Blob Information End]");

                // TODO : 함수 내에서, 인자로 받은 sizex와 sizey를 이용하여 해당 크기보다 큰 블랍을 필터링한다.
                // 따라서, 약간 넉넉한 사이즈를 넘겨야 찾고자 하는 블랍의 데이터를 얻을 수 있다.

                bool SaveOrginalFlag = this.FileManager().FileManagerParam.PinAlignHighKeySaveOriginalImage.Value;
                bool SavePassFlag = this.FileManager().FileManagerParam.PinAlignHighKeySavePassImage.Value;
                bool SaveFailFlag = this.FileManager().FileManagerParam.PinAlignHighKeySaveFailImage.Value;

                string SaveOriginalImageFullPath = string.Empty;
                string SavePassImageFullPath = string.Empty;
                string SaveFailImageFullPath = string.Empty;

                bool AutoLightFlag = false;
                int Foundgraylevel = alignkeyinfo.FoundGrayLevelForBlob.Value;

                // 20% >> 0.2로 매개변수 전달 해야 됨.
                double BlobSizeXMinMargin = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeXMinimumMargin.Value * 0.01;
                double BlobSizeXMaxMargin = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeXMaximumMargin.Value * 0.01;
                double BlobSizeYMinMargin = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeYMinimumMargin.Value * 0.01;
                double BlobSizeYMaxMargin = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobSizeYMaximumMargin.Value * 0.01;

                double minRectangularity = PinAlignParam.PinHighAlignParam.HighAlignKeyParameter.KeyRectangularity.Value;
                bool processing_result = false;

                BlobResult blobresult = this.VisionManager().FindBlobWithRectangularity(EnumProberCam.PIN_HIGH_CAM, ref PosX, ref PosY,
                                                        ref Foundgraylevel, tmpThreshVal, singlepinalign.BlobMinSize, singlepinalign.BlobMaxSize,
                                                        OffsetX, OffsetY,
                                                        RealSearchAreaX, RealSearchAreaY,
                                                        keysizex, keysizey,
                                                        isDilation,
                                                        BlobSizeXMinMargin, BlobSizeXMaxMargin,
                                                        BlobSizeYMinMargin, BlobSizeYMaxMargin,
                                                        AutoLightFlag,
                                                        minRectangularity);
                if (SaveOrginalFlag == true)
                {
                    SaveOriginalImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "HIGHKEYBLOB", $"PinNo#_{pindata.PinNum}_ORIGINAL");
                    this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveOriginalImageFullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                }
                if (blobresult?.DevicePositions?.Count == 1)
                {
                    processing_result = true;

                    if (SavePassFlag == true || this.WaferAligner().IsOnDubugMode)
                    {
                        SavePassImageFullPath = this.FileManager().GetImageSaveFullPath(
                            EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP,
                            true,
                            "SINGLEPIN", "HIGHKEYBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_PASS");
                        this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SavePassImageFullPath, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);
                    }

                    LoggerManager.Debug($"[{this.GetType().Name}] Seach Align Key: Align key blob find Ok, PinNo#_{pindata.PinNum}, Count =  {blobresult?.DevicePositions?.Count}, Threshold_1st_{tmpThreshVal}_PASS");
                }
                else
                {
                    if (SaveFailFlag == true)
                    {
                        SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(
                            EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP,
                            true,
                            "SINGLEPIN", "HIGHKEYBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_FAIL");
                        this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                        LoggerManager.Debug($"[{this.GetType().Name}] Seach Align Key: Align key blob find fail, PinNo#_{pindata.PinNum}, Count =  {blobresult?.DevicePositions?.Count}, Threshold_1st_{tmpThreshVal}_FAIL");
                    }

                    tmpThreshVal = 0;       //Threshold를 다르게 설정하여 다시 Blob 시도 (0으로 주면 안쪽에서 다른 처리가 됨.)
                    blobresult = this.VisionManager().FindBlobWithRectangularity(EnumProberCam.PIN_HIGH_CAM, ref PosX, ref PosY,
                                        ref Foundgraylevel, tmpThreshVal, singlepinalign.BlobMinSize, singlepinalign.BlobMaxSize,
                                        OffsetX, OffsetY,
                                        RealSearchAreaX, RealSearchAreaY,
                                        keysizex, keysizey,
                                        isDilation,
                                        BlobSizeXMinMargin, BlobSizeXMaxMargin,
                                        BlobSizeYMinMargin, BlobSizeYMaxMargin,
                                        AutoLightFlag,
                                        minRectangularity);
                    if (blobresult?.DevicePositions?.Count == 1)
                    {
                        processing_result = true;

                        if (SavePassFlag == true || this.WaferAligner().IsOnDubugMode)
                        {
                            SavePassImageFullPath = this.FileManager().GetImageSaveFullPath(
                                EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP,
                                true,
                                "SINGLEPIN", "HIGHKEYBLOB", $"PinNo#_{pindata.PinNum}_Adaptive_PASS");
                            this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SavePassImageFullPath, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);
                        }
                        LoggerManager.Debug($"[{this.GetType().Name}] Seach Align Key: Align key blob find Ok, PinNo#_{pindata.PinNum}, Count =  {blobresult?.DevicePositions?.Count}, Adaptive_PASS");
                    }
                    else
                    {
                        if (SaveFailFlag == true)
                        {
                            SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(
                                EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP,
                                true,
                                "SINGLEPIN", "HIGHKEYBLOB", $"PinNo#_{pindata.PinNum}_Adaptive_FAIL");
                            this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }
                        LoggerManager.Debug($"[{this.GetType().Name}] Seach Align Key: Align key blob find fail. PinNo#_{pindata.PinNum}, Count =  {blobresult?.DevicePositions?.Count}, Adaptive_FAIL");
                        
                        //Model Find
                        int blobSizeWidthinPixel = (int)(alignkeyinfo.BlobSizeX.Value / cam.GetRatioX());   // Reatio = μm/pixel
                        int blobSizeHeightinPixel = (int)(alignkeyinfo.BlobSizeY.Value / cam.GetRatioY());

                        ObservableCollection<LightValueParam> lightParamLists = alignkeyinfo.PatternIfo.LightParams;

                        MFParameter mFParameters = new MFParameter(blobSizeWidthinPixel, blobSizeHeightinPixel, lightParamLists.ToList());
                        ImageBuffer grabBuffer = null;

                        if (mFParameters != null)
                        {
                            foreach (var light in alignkeyinfo.PatternIfo.LightParams)
                            {
                                cam.SetLight(light.Type.Value, light.Value.Value);
                                LoggerManager.Debug($"ModelFinder SetLight {light.Type.Value}: {light.Value.Value}");
                            }

                            grabBuffer = this.VisionManager().SingleGrab(cam.GetChannelType(), this);

                            var baseResults = this.VisionManager().VisionProcessing.ModelFind_For_Key(
                                    targetimg: grabBuffer,
                                    targettype: mFParameters.ModelTargetType.Value,
                                    foreground: mFParameters.ForegroundType.Value,
                                    size: new Size(mFParameters.ModelWidth.Value, mFParameters.ModelHeight.Value),
                                    acceptance: mFParameters.Acceptance.Value,
                                    roiwidth:RealSearchAreaX,
                                    roiheight:RealSearchAreaY,
                                    scale_min: mFParameters.ScaleMin.Value,
                                    scale_max: mFParameters.ScaleMax.Value,
                                    smoothness: mFParameters.Smoothness.Value);

                            if (baseResults.Count == 1)
                            {
                                processing_result = true;
                                PosX = baseResults.FirstOrDefault().Position.X.Value;
                                PosY = baseResults.FirstOrDefault().Position.Y.Value;
                                LoggerManager.Debug($"[{this.GetType().Name}] Seach Align Key: Align key model find Ok, PinNo#_{pindata.PinNum}, Count =  {baseResults.Count}, Position = ({PosX:0.0}, {PosY:0.0}), ModelTargetType = {mFParameters.ModelTargetType.Value}_PASS");

                                if (SavePassFlag == true || this.WaferAligner().IsOnDubugMode)
                                {
                                    SavePassImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "HIGHKEYMODELFINDER", $"PinNo#_{pindata.PinNum}_ModelTargetType_{mFParameters.ModelTargetType.Value}_PASS");
                                    this.VisionManager().SaveImageBuffer(baseResults.FirstOrDefault().ResultBuffer, SavePassImageFullPath, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[{this.GetType().Name}] Seach Align Key: Align key model find fail, PinNo#_{pindata.PinNum}, Count =  {baseResults.Count}, ModelTargetType = {mFParameters.ModelTargetType.Value}_FAIL");
                                errcode = EventCodeEnum.PIN_ALIGN_FAILED;
                            }
                        }
                    }
                }

                if(processing_result == true)
                {
                    this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                    if (AutoLightFlag == true)
                    {
                        // When pin source is registraion, the value can be changed.
                        if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                        {
                            alignkeyinfo.FoundGrayLevelForBlob.Value = Foundgraylevel;
                        }
                    }

                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                    {
                        if (alignkeyinfo.AlignKeyPos.GetZ() < PinAlignParam.PinHighAlignParam.PinKeyMinDistance.Value)
                        {
                            errcode = EventCodeEnum.PIN_SW_LIMIT;
                            return errcode;
                        }

                        singlepinalign.NewPinPos = new PinCoordinate(singlepinalign.ConvertPosPixelToPin(cam, singlepinalign.AlignPin.AbsPos, PosX, PosY));

                        singlepinalign.NewPinPos.X.Value = singlepinalign.NewPinPos.X.Value - alignkeyinfo.AlignKeyPos.GetX();
                        singlepinalign.NewPinPos.Y.Value = singlepinalign.NewPinPos.Y.Value - alignkeyinfo.AlignKeyPos.GetY();
                        singlepinalign.NewPinPos.Z.Value = singlepinalign.NewPinPos.Z.Value - alignkeyinfo.AlignKeyPos.GetZ();

                        errcode = EventCodeEnum.NONE;

                        // Key를 측정하고 옵션에 따라서 위치 업데이트 없이 Tip만 확인 하여 성공률에 따라 Align 성공 여부 판단. (+ 이 옵션이 켜져 있는 한에서만 Tip Monitoring 가능)
                        if (PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value)
                        {
                            this.StageSupervisor().StageModuleState.PinHighViewMove(singlepinalign.NewPinPos.X.Value, singlepinalign.NewPinPos.Y.Value, singlepinalign.NewPinPos.Z.Value);

                            EventCodeEnum pintiprst = TipFocusModule(singlepinalign);

                            if (pintiprst != EventCodeEnum.NONE)
                            {
                                singlepinalign.AlignPin.PinTipResult.Value = PINALIGNRESULT.PIN_FOCUS_FAILED;
                            }
                            else
                            {
                                PinCoordinate tiploc = null;
                                pintiprst = TipBlobModule(singlepinalign, ref tiploc);

                                if (pintiprst != EventCodeEnum.NONE)
                                {
                                    singlepinalign.AlignPin.PinTipResult.Value = PINALIGNRESULT.PIN_BLOB_FAILED;
                                }
                                else
                                {
                                    if (tiploc != null)
                                    {
                                        singlepinalign.AlignPin.PinTipResult.Value = PINALIGNRESULT.PIN_PASSED;
                                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign(): Monitor Needle Focusing (X:{tiploc.GetX()}, {tiploc.GetY()}, {tiploc.GetT()}, {tiploc.GetZ()})");
                                    }
                                }
                            }
                        }
                    }
                    else if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        return EventCodeEnum.NONE;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Method(): Error occurred. Err = {err.Message}");
                throw err;
            }
            return errcode;

        }

        public EventCodeEnum TipBlobModule(ISinglePinAlign singlepinalign, ref PinCoordinate NewPinPos)
        {
            bool ParamValid = true;
            EventCodeEnum errcode = EventCodeEnum.UNDEFINED;
            int tmpThreshVal = 0;
            double PosX = 0;
            double PosY = 0;
            NewPinPos = null;

            try
            {
                ICamera cam = this.VisionManager().GetCam(singlepinalign.FocusingParam.FocusingCam.Value);

                double ratioX = cam.GetRatioX();
                double ratioY = cam.GetRatioY();

                bool isDilation = this.PinAlignParam.EnableDilation.Value;

                if (isDilation)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] DoExecute(), isDilation = {isDilation}");
                }

                if (singlepinalign.AlignPin.PinSearchParam.LightForTip.Count <= 0)
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule(), LightForTip parameter is invalid. LightForTip Count = {singlepinalign.AlignPin.PinSearchParam.LightForTip.Count}");
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
                    LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule(), MinBlobSizeX parameter is invalid. MinBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value}");
                }

                if ((singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule(), MinBlobSizeY parameter is invalid. MinBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value}");
                }

                if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value >= cam.Param.GrabSizeX.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule(), MaxBlobSizeX parameter is invalid. MaxBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value}");
                }

                if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule(), MaxBlobSizeY parameter is invalid. MaxBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value}");
                }

                if (ParamValid == true)
                {
                    singlepinalign.BlobMinSize = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value / ratioX * singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value / ratioY);
                    singlepinalign.BlobMaxSize = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value / ratioX * singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value / ratioY);

                    // Auto threshold 적용
                    if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.AUTO)
                    {
                        int SizeX = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width / ratioX);
                        int SizeY = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height / ratioY);

                        int OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(SizeX / 2);
                        int OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(SizeY / 2);

                        tmpThreshVal = singlepinalign.getOtsuThreshold(cam.GetChannelType(), OffsetX, OffsetY, SizeX, SizeY);
                    }
                    else
                    {
                        tmpThreshVal = singlepinalign.AlignPin.PinSearchParam.BlobThreshold.Value;
                    }

                    int foundgraylevel = 0;

                    int BlobSizeX = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width / ratioX);
                    int BlobSizeY = Convert.ToInt32(singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height / ratioY);

                    int BlobOffsetX = (int)cam.GetGrabSizeWidth() / 2 - Convert.ToInt32(BlobSizeX) / 2;
                    int BlobOffsetY = (int)cam.GetGrabSizeHeight() / 2 - Convert.ToInt32(BlobSizeY) / 2;

                    bool SaveOrginalFlag = this.FileManager().FileManagerParam.PinAlignHighKeySaveOriginalImage.Value;
                    bool SavePassFlag = this.FileManager().FileManagerParam.PinAlignHighKeySavePassImage.Value;
                    bool SaveFailFlag = this.FileManager().FileManagerParam.PinAlignHighKeySaveFailImage.Value;

                    string SaveOriginalImageFullPath = string.Empty;
                    string SavePassImageFullPath = string.Empty;
                    string SaveFailImageFullPath = string.Empty;

                    string SaveOriginalImageFullPath_For_SizeValidation = string.Empty;
                    string SaveSizeInRangeImageFullPath_For_SizeValidation = string.Empty;
                    string SaveOutOfRangeImageFullPath_For_SizeValidation = string.Empty;

                    string probe_card_Id = string.Empty;

                    BlobResult blobresult = this.VisionManager().FindBlob(cam.GetChannelType(), ref PosX, ref PosY, ref foundgraylevel, tmpThreshVal, singlepinalign.BlobMinSize, singlepinalign.BlobMaxSize, BlobOffsetX, BlobOffsetY, BlobSizeX, BlobSizeY, isDilation);

                    IPinData pindata = this.PinAligner().SinglePinAligner.AlignPin;
                    
                    if (SaveOrginalFlag == true)
                    {
                        SaveOriginalImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "HIGHKEYBLOB", "TIPBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_ORIGINAL");
                        this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveOriginalImageFullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                    }

                    if (blobresult?.DevicePositions?.Count == 1)
                    {
                        if (SavePassFlag == true || this.WaferAligner().IsOnDubugMode)
                        {
                            SavePassImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "HIGHKEYBLOB", "TIPBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_PASS");
                            this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SavePassImageFullPath, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule() : Needle Blob Search Ok");

                        this.VisionManager().StartGrab(cam.GetChannelType(), this);

                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                        {
                            NewPinPos = new PinCoordinate(singlepinalign.ConvertPosPixelToPin(cam, singlepinalign.AlignPin.AbsPos, PosX, PosY));
                            this.StageSupervisor().StageModuleState.PinHighViewMove(NewPinPos.GetX(), NewPinPos.GetY(), NewPinPos.GetZ());
                        }

                        if (PinAlignParam.PinHighAlignParam.PinTipSizeValidationEnable.Value)
                        {
                            bool NeedValidation = false;

                            if(PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Targeting_Option.Value == SizeValidationOption.ALL)
                            {
                                NeedValidation = true;
                            }
                            else if(PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Targeting_Option.Value == SizeValidationOption.INDIVIDUAL)
                            {
                                if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Count > 0)
                                {
                                    var matchingItem = PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.FirstOrDefault(item => item.PinNum.Value == singlepinalign.AlignPin.PinNum.Value);

                                    if (matchingItem != null && matchingItem.IsSelected.Value)
                                    {
                                        NeedValidation = true;
                                    }
                                    else
                                    {
                                        NeedValidation = false;
                                    }
                                }
                            }

                            if(NeedValidation)
                            {
                                PinSizeValidateResult pinResult = new PinSizeValidateResult(singlepinalign.AlignPin.PinNum.Value);

                                if (this.CardChangeModule().GetProbeCardID() != null)
                                {
                                    probe_card_Id = $"{this.CardChangeModule().GetProbeCardID()}";
                                }
                                else
                                {
                                    probe_card_Id = "";
                                }

                                string filename = $"CardID({probe_card_Id})_DutNo({pindata.DutNumber.Value})PinNo({pindata.PinNum.Value})";
                                SaveOriginalImageFullPath_For_SizeValidation = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SIZE_VALIDATION", "ORIGINAL", filename);
                                
                                this.PinAligner().ValidPinTipSize_Original_List.Add(pinResult);
                                EventCodeEnum saveret = this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveOriginalImageFullPath_For_SizeValidation, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                if (saveret == EventCodeEnum.NONE)
                                {
                                    this.PinAligner().ValidPinTipSize_Original_List.Last().filePath = SaveOriginalImageFullPath_For_SizeValidation;
                                }

                                TIP_SIZE_VALIDATION_RESULT monitoring_result = TIP_SIZE_VALIDATION_RESULT.UNDEFINED;
                                
                                monitoring_result = this.PinAligner().Validation_Pin_Tip_Size(blobresult, singlepinalign, ratioX, ratioY);

                                if (monitoring_result == TIP_SIZE_VALIDATION_RESULT.OUT_OF_RANGE)
                                {
                                    PinSizeValidateResult outResult = new PinSizeValidateResult(singlepinalign.AlignPin.PinNum.Value);
                                    this.PinAligner().ValidPinTipSize_OutOfSize_List.Add(outResult);

                                    if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_OutOfSize_Image.Value)
                                    {
                                        SaveOutOfRangeImageFullPath_For_SizeValidation = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SIZE_VALIDATION", "OUT_OF_RANGE", filename);
                                        saveret = this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveOutOfRangeImageFullPath_For_SizeValidation, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                                        if (saveret == EventCodeEnum.NONE)
                                        {
                                            this.PinAligner().ValidPinTipSize_OutOfSize_List.Last().filePath = SaveOutOfRangeImageFullPath_For_SizeValidation;
                                        }
                                    }
                                }
                                else if (monitoring_result == TIP_SIZE_VALIDATION_RESULT.IN_RANGE)
                                {
                                    PinSizeValidateResult inResult = new PinSizeValidateResult(singlepinalign.AlignPin.PinNum.Value);
                                    this.PinAligner().ValidPinTipSize_SizeInRange_List.Add(inResult);

                                    if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_SizeInRange_Image.Value)
                                    {
                                        SaveSizeInRangeImageFullPath_For_SizeValidation = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SIZE_VALIDATION", "SIZE_IN_RANGE", filename);
                                        saveret = this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveSizeInRangeImageFullPath_For_SizeValidation, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);
                                        if (saveret == EventCodeEnum.NONE)
                                        {
                                            this.PinAligner().ValidPinTipSize_SizeInRange_List.Last().filePath = SaveSizeInRangeImageFullPath_For_SizeValidation;
                                        }
                                    }
                                }
                                else
                                {
                                    LoggerManager.PinLog($"[TIP SIZE VALIDATION], failed. return value: {monitoring_result}");
                                }
                            }
                        }
                        else
                        {
                            //Monitoring 기능은 꺼져있고 Tip Focusing Enable이 True로 설정되어 있을 때 타는 곳.
                        }

                        errcode = EventCodeEnum.NONE;
                    }
                    else if (blobresult?.DevicePositions?.Count == 0)
                    {
                        if (SaveFailFlag == true)
                        {
                            if (PinAlignParam.PinHighAlignParam.PinTipSizeValidationEnable.Value)
                            {
                                //PIN에다가 이미지 넣어야 함. 저장옵션도 같이 체크 필요
                            }
                            else
                            {
                                SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "HIGHKEYBLOB", "TIPBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_FAIL");
                                this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                                SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "SINGLEPIN", "HIGHKEYBLOB", "TIPBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_ORI");
                                this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                            }
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule() : Needle Blob Search Fail");
                        errcode = EventCodeEnum.VISION_BLOB_NOT_FOUND;
                        this.NotifyManager().Notify(errcode);
                    }
                    else
                    {
                        if (SaveFailFlag == true)
                        {
                            SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SINGLEPIN", "HIGHKEYBLOB", "TIPBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_FAIL");
                            this.VisionManager().SaveImageBuffer(blobresult.ResultBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                            SaveFailImageFullPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.JPEG, true, "SINGLEPIN", "HIGHKEYBLOB", "TIPBLOB", $"PinNo#_{pindata.PinNum}_Threshold_{tmpThreshVal}_ORI");
                            this.VisionManager().SaveImageBuffer(blobresult.OriginalBuffer, SaveFailImageFullPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}] TipBlobModule() : Needle Blob Search Fail");
                        errcode = EventCodeEnum.VISION_BLOB_AREA_MULTIPLE_OCCURENCES;
                        this.NotifyManager().Notify(errcode);
                    }
                }
                else
                {
                    errcode = EventCodeEnum.PARAM_ERROR;
                }


            }
            catch (Exception err)
            {
                errcode = EventCodeEnum.VISION_BLOB_EXCEPTION;
                LoggerManager.Exception(err);
            }



            return errcode;
        }

        private EventCodeEnum TipFocusModule(ISinglePinAlign singlepinalign)
        {
            bool ParamValid = true;
            EventCodeEnum errcode = EventCodeEnum.UNDEFINED;
            //double PosX = 0;
            //double PosY = 0;
            try
            {
                ICamera cam = this.VisionManager().GetCam(singlepinalign.FocusingParam.FocusingCam.Value);
                if (singlepinalign.AlignPin.PinSearchParam.LightForTip.Count <= 0)
                {
                    ParamValid = false;
                    LoggerManager.Debug($"[{this.GetType().Name}] TipFocusModule(), LightForTip parameter is invalid. LightForTip Count = {singlepinalign.AlignPin.PinSearchParam.LightForTip.Count}");
                }
                else
                {
                    foreach (var light in singlepinalign.AlignPin.PinSearchParam.LightForTip)
                    {
                        cam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }


                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.PROCESSING)
                {
                    // MinBlobSize & MaxBlobSize 값의 유효한 범위 : 0보다 크고, GrabSize 보다 작아야 한다.
                    if ((singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value >= cam.Param.GrabSizeX.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[{this.GetType().Name}] TipFocusModule(), MinBlobSizeX parameter is invalid. MinBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value}");
                    }

                    if ((singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[{this.GetType().Name}] TipFocusModule(), MinBlobSizeY parameter is invalid. MinBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value}");
                    }

                    if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value >= cam.Param.GrabSizeX.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[{this.GetType().Name}] TipFocusModule(), MaxBlobSizeX parameter is invalid. MaxBlobSizeX = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value}");
                    }

                    if ((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value <= 0) || (singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value >= cam.Param.GrabSizeY.Value))
                    {
                        ParamValid = false;
                        LoggerManager.Debug($"[{this.GetType().Name}] DoExecute(), MaxBlobSizeY parameter is invalid. MaxBlobSizeY = {singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value}");
                    }

                    if (ParamValid == true)
                    {
                        //singlepinalign.FocusingParam.FocusingROI.Value = singlepinalign.AlignPin.PinSearchParam.SearchArea.Value;

                        var tipsizeX = Convert.ToInt32(((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeX.Value + singlepinalign.AlignPin.PinSearchParam.MinBlobSizeX.Value) / cam.GetRatioX()) / 2);
                        var tipsizeY = Convert.ToInt32(((singlepinalign.AlignPin.PinSearchParam.MaxBlobSizeY.Value + singlepinalign.AlignPin.PinSearchParam.MinBlobSizeY.Value) / cam.GetRatioY()) / 2);

                        singlepinalign.OffsetX = cam.Param.GrabSizeX.Value / 2 - (tipsizeX / 2);
                        singlepinalign.OffsetY = cam.Param.GrabSizeY.Value / 2 - (tipsizeY / 2);

                        singlepinalign.FocusingParam.FocusingROI.Value = new System.Windows.Rect(singlepinalign.OffsetX, singlepinalign.OffsetY, tipsizeX, tipsizeY);
                        singlepinalign.FocusingParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinHighAlignParam.PinTipFocusRange.Value);

                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Tip Focusing Parameter ROI X = {singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.X}, Y = {singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Y}, Width = {singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Width}, Height = {singlepinalign.AlignPin.PinSearchParam.SearchArea.Value.Height}");
                        LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : Tip Focusing Parameter Focusing Range = " + singlepinalign.FocusingParam.FocusRange.Value.ToString());

                        this.VisionManager().StartGrab(cam.GetChannelType(), this);

                        // TODO: 하드코딩..누군가 언제간 바꾸게 될 것이다. 2020-02-26 Alvin 예언
                        singlepinalign.FocusingParam.CheckDualPeak.Value = false;
                        singlepinalign.FocusingParam.FocusThreshold.Value = 5000;
                        singlepinalign.FocusingParam.PeakRangeThreshold.Value = 200;
                        singlepinalign.FocusingParam.DepthOfField.Value = 1;
                        singlepinalign.FocusingParam.OutFocusLimit.Value = 40;

                        //bool SavePassFlag_For_SizeValidation = false;
                        //bool SaveFailFlag_For_SizeValidation = false;

                        //if (PinAlignParam.PinHighAlignParam.PinTipSizeValidationEnable.Value)
                        //{
                        //    if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Value.Count > 0)
                        //    {
                        //        if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Value.Contains(singlepinalign.AlignPin.PinNum.Value))
                        //        {
                        //            SavePassFlag_For_SizeValidation = PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Focusing_Pass_Image.Value;
                        //            SaveFailFlag_For_SizeValidation = PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Focusing_Fail_Image.Value;
                        //        }
                        //    }
                        //}

                        LoggerManager.Debug($"[{this.GetType().Name}] TipFocusModule() : Tip Focusing Start");

                        //string Tip_SavePassPath = "";
                        //string Tip_SaveFailPath = "";

                        //if (SavePassFlag_For_SizeValidation)
                        //{
                        //    Tip_SavePassPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SIZE_VALIDATION", "FOCUSING_RESULT");
                        //}
                        //else if (SaveFailFlag_For_SizeValidation)
                        //{
                        //    Tip_SaveFailPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "SIZE_VALIDATION", "FOCUSING_RESULT");
                        //}

                        //errcode = singlepinalign.Focusing.Focusing_Retry(singlepinalign.FocusingParam, false, false, false, this, SavePassPath: Tip_SavePassPath, SaveFailPath: Tip_SaveFailPath);

                        errcode = singlepinalign.Focusing.Focusing_Retry(singlepinalign.FocusingParam, false, false, false, this);

                        if (errcode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}] TipFocusModule() : Tip Focusing Fail Error Code = " + errcode.ToString());
                          
                            errcode = EventCodeEnum.PIN_FOCUS_FAILED;
                        }
                        else
                        {
                            errcode = EventCodeEnum.NONE;
                            LoggerManager.Debug($"[{this.GetType().Name}] TipFocusModule() : Tip Focusing Ok");
                        }
                    }
                    else
                    {
                        errcode = EventCodeEnum.PARAM_ERROR;
                    }
                }
                else
                {
                    //new PinCoordinate(singlepinalign.ConvertPosPixelToPin(cam, singlepinalign.AlignPin.AbsPos, PosX, PosY));

                    errcode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                errcode = EventCodeEnum.EXCEPTION;
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
    }
}
