using System;
using System.Collections.Generic;
using LogModule;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace SinglePinAlign
{
    public class BlobSinglePinAlign : ISinglePinAlign, IModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public SinglePinAlignTestMock TestMock { get; set; }

        private PinCoordinate _NewPinPos;
        public PinCoordinate NewPinPos
        {
            get { return _NewPinPos; }
            set { _NewPinPos = value; }
        }
        private IPinData _AlignPin;
        public IPinData AlignPin
        {
            get { return _AlignPin; }
            set { _AlignPin = value; }
        }
        private IFocusing _Focusing;
        public IFocusing Focusing
        {
            get { return _Focusing; }
            set { _Focusing = value; }
        }
        private IFocusParameter _FocusingParam;
        public IFocusParameter FocusingParam
        {
            get { return _FocusingParam; }
            set { _FocusingParam = value; }
        }
        private int _OffsetX;

        public int OffsetX
        {
            get { return _OffsetX; }
            set { _OffsetX = value; }
        }
        private int _OffsetY;
        public int OffsetY
        {
            get { return _OffsetY; }
            set { _OffsetY = value; }
        }
        private int _BlobMinSize;
        public int BlobMinSize
        {
            get { return _BlobMinSize; }
            set { _BlobMinSize = value; }
        }
        private int _BlobMaxSize;
        public int BlobMaxSize
        {
            get { return _BlobMaxSize; }
            set { _BlobMaxSize = value; }
        }
        private int _BlobResult;
        public int BlobResult
        {
            get { return _BlobResult; }
            set { _BlobResult = value; }
        }

        private int _AlignKeyIndex;

        public int AlignKeyIndex
        {
            get { return _AlignKeyIndex; }
            set { _AlignKeyIndex = value; }
        }

        public bool Initialized { get; set; } = false;

        public BlobSinglePinAlign()
        {

        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

        public int getOtsuThreshold(EnumProberCam camtype, int OffsetX, int OffsetY, int SizeX, int SizeY)
        {
            byte t = 0;
            float[] vet = new float[256];
            int[] hist = new int[256];
            try
            {
                vet.Initialize();
                hist.Initialize();

                float p1, p2, p12;
                int k;
                //int OffsetX = 0, OffsetY = 0;

                ImageBuffer curImg = null;
                curImg = this.VisionManager().SingleGrab(camtype, this);

                //OffsetX = (curImg.SizeX / 2) - (int)(SizeX / 2);
                //OffsetY = (curImg.SizeY / 2) - (int)(SizeY / 2);

                for (int i = 0; i < curImg.SizeX; i++)
                {
                    for (int j = 0; j < curImg.SizeY; j++)
                    {
                        if ((i >= OffsetX && i <= (OffsetX + SizeX)) && (j >= OffsetY && j <= (OffsetY + SizeY)))
                        {
                            hist[curImg.Buffer[(curImg.SizeX * j) + i]]++;
                        }
                    }
                }

                int minindex = 0, maxindex = 0;

                for (int i = 0; i < 256; i++)
                {
                    if (hist[i] != 0)
                    {
                        minindex = i;
                        break;
                    }
                }

                for (int i = 255; i >= 0; i--)
                {
                    if (hist[i] != 0)
                    {
                        maxindex = i;
                        break;
                    }
                }

                if (maxindex != 0)
                {
                    for (k = minindex + 1; k != maxindex; k++)
                    {
                        try
                        {
                            if (k > 255) break;
                            p1 = Px(minindex, k, hist);             // 제일 어두운 색부터 제일 밝은 색까지 모두 더한 값
                            p2 = Px(k + 1, maxindex, hist);
                            p12 = p1 * p2;
                            if (p12 == 0)
                                p12 = 1;
                            float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                            //if(vet.Length > k)
                            vet[k] = (float)diff * diff / p12;
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                        }

                    }
                }


                t = (byte)findMax(vet, 256);
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return t;
        }
        public float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            try
            {
                for (i = init; i <= end; i++)
                {
                    //if(hist.Length > i)
                    //{
                    sum += hist[i];
                    //}
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return (float)sum;
        }

        // function is used to compute the mean values in the equation (mu)
        public float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            try
            {
                for (i = init; i <= end; i++)
                {
                    //if (hist.Length > i)
                    //{
                    sum += i * hist[i];
                    //}
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return (float)sum;
        }

        // finds the maximum element in a vector
        public int findMax(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            try
            {
                for (i = 1; i < n - 1; i++)
                {
                    if (vec[i] > maxVec)
                    {
                        maxVec = vec[i];
                        idx = i;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "getOtsuThreshold() : Error occured.");
            }
            return idx;
        }

        public PINALIGNRESULT SinglePinalign(out PinCoordinate newpinpos, IPinData alignPin, IFocusing focusing, IFocusParameter focusingParam)
        {
            PINALIGNRESULT ret = PINALIGNRESULT.PIN_BLOB_FAILED;
            AlignPin = alignPin;
            Focusing = focusing;
            FocusingParam = focusingParam;
            OffsetX = 0;
            OffsetY = 0;
            BlobMinSize = 0;
            BlobMaxSize = 0;
            BlobResult = 0;
            AlignKeyIndex = AlignPin.PinSearchParam.AlignKeyIndex.Value;
            List<ISubModule> modules = this.PinAligner().SinglePinTemplate.GetProcessingModule();
            EventCodeEnum alignResult = EventCodeEnum.NONE;

            newpinpos = null;

            MachineCoordinate machine = new MachineCoordinate();

            try
            {
                if (focusingParam.FocusingCam.Value != EnumProberCam.PIN_HIGH_CAM)
                {
                    LoggerManager.Error($"[BlobSinglePinAlign] SinglePinaling() Focusing parameter is wrong. Value is {focusingParam.FocusingCam.Value}");

                    ret = PINALIGNRESULT.PIN_FOCUS_FAILED;
                    newpinpos = null;

                    return ret;
                }

                NewPinPos = new PinCoordinate(this.VisionManager().GetCam(focusingParam.FocusingCam.Value).GetCurCoordPos());
                newpinpos = new PinCoordinate(this.VisionManager().GetCam(focusingParam.FocusingCam.Value).GetCurCoordPos());

                try
                {
                    LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() : Single Pin Align Start");

                    ICamera cam = this.VisionManager().GetCam(focusingParam.FocusingCam.Value);

                    // 핀 얼라인 OFF 면 그냥 현재 위치 반환
                    // 에뮬 모드면 그냥 현재 위치 반환
                    if (this.PinAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                    {
                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.Cantilever_Standard)
                        {
                            newpinpos = new PinCoordinate(cam.GetCurCoordPos());
                        }
                        else if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                        {
                            if (AlignPin.PinSearchParam.AlignKeyHigh.Count > 0)
                            {
                                ProberInterfaces.Param.PinSearchParameter.AlignKeyInfo alignkeyinfo = AlignPin.PinSearchParam.AlignKeyHigh[AlignKeyIndex];

                                newpinpos = new PinCoordinate(cam.GetCurCoordPos());

                                newpinpos.X.Value = newpinpos.X.Value - alignkeyinfo.AlignKeyPos.GetX();
                                newpinpos.Y.Value = newpinpos.Y.Value - alignkeyinfo.AlignKeyPos.GetY();
                                newpinpos.Z.Value = newpinpos.Z.Value - alignkeyinfo.AlignKeyPos.GetZ();

                            }
                            else
                            {
                                newpinpos = new PinCoordinate(cam.GetCurCoordPos());
                            }
                        }

                        alignPin.PinTipResult.Value = PINALIGNRESULT.PIN_PASSED;
                        ret = PINALIGNRESULT.PIN_PASSED;

                        return ret;
                    }

                    if (modules != null)
                    {
                        foreach (IProcessingModule module in modules)
                        {
                            if (module.SubModuleState == null)
                                module.SubModuleState = new SubModuleIdleState(module);
                        }
                        if (this.PinAligner().Each_Pin_Failure)
                        {
                            LoggerManager.Error($"SinglePinalign(),Pin # {AlignPin.PinNum} Each_Pin_Failure = {this.PinAligner().Each_Pin_Failure}");
                            return PINALIGNRESULT.PIN_BLOB_FAILED;
                        }
                        foreach (IProcessingModule module in modules)
                        {
                            // Module list : 2020-05-06

                            // (1) AlignkeyBlob
                            // (2) AlignkeyFocusing
                            // (3) TipBlob
                            // (4) TipFocusing
                            // (5) TipFocusRough

                            // TODO : 각 서브모듈의 리턴값 확인 및 처리

                            alignResult = module.SubModuleState.Execute();

                            if (alignResult != EventCodeEnum.NONE)
                            {
                                if ((alignResult == EventCodeEnum.FOCUS_VALUE_THRESHOLD) ||
                                     (alignResult == EventCodeEnum.FOCUS_VALUE_FLAT) ||
                                     (alignResult == EventCodeEnum.FOCUS_VALUE_DUALPEAK) ||
                                     (alignResult == EventCodeEnum.PIN_FOCUS_FAILED)
                                     )
                                {
                                    return PINALIGNRESULT.PIN_FOCUS_FAILED;
                                }

                                if (alignResult == EventCodeEnum.PIN_ALIGN_FAILED)
                                {
                                    return PINALIGNRESULT.PIN_BLOB_FAILED;
                                }

                                if (alignResult == EventCodeEnum.PIN_TIP_BASE_DISTANCE_TOLERANCE)
                                {
                                    return PINALIGNRESULT.PIN_FOCUS_FAILED;
                                }

                                break;
                            }
                        }

                        if(alignResult == EventCodeEnum.NONE)
                        {
                            newpinpos = NewPinPos;

                            ret = PINALIGNRESULT.PIN_PASSED;
                            try
                            {
                                var mcPos = this.CoordinateManager().PinHighPinConvert.ConvertBack(newpinpos);
                                AlignPin.MachineCoordPos = mcPos;
                                LoggerManager.Debug($"[{this.GetType().Name}] SinglePinalign() : position is updated ({NewPinPos.X.Value:0.00}, {NewPinPos.Y.Value:0.00}, {NewPinPos.Z.Value:0.00})," +
                                            $"machine position is updated ({AlignPin.MachineCoordPos.X.Value:0.00}, {AlignPin.MachineCoordPos.Y.Value:0.00}, {AlignPin.MachineCoordPos.Z.Value:0.00})");
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"AlignResult = {alignResult}");
                            ret = PINALIGNRESULT.PIN_UNKNOWN_FAILED;
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[BlobSinglePinAlign] SinglePinalign() :Exception error is occurred");
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public PinCoordinate ConvertPosPixelToPin(ICamera CurCam, PinCoordinate OldPos, double PosX, double PosY)
        {
            PinCoordinate pcd = new PinCoordinate(CurCam.GetCurCoordPos());

            double offsetx = 0.0;
            double offsety = 0.0;

            try
            {
                offsetx = (CurCam.GetGrabSizeWidth() / 2) - PosX;
                offsety = (CurCam.GetGrabSizeHeight() / 2) - PosY;

                offsetx *= CurCam.GetRatioX();
                offsety *= CurCam.GetRatioY();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return new PinCoordinate(pcd.GetX() - offsetx, pcd.GetY() + offsety, pcd.GetZ());
        }

        public void DeInitModule()
        {
        }
    }
}
