using System;
using System.Collections.Generic;
using System.Linq;

namespace ThetaAlignStandatdModule
{
    using LogModule;
    using Newtonsoft.Json;
    using PnPControl;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using ProberInterfaces.WaferAlignEX;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Xml.Serialization;
    using ProberInterfaces.WaferAlignEX.Enum;
    using Focusing;

    public enum EnumDirection
    {
        VER,
        HOR
    }

    enum ThetaAlignEventCodeEnum
    {
        INVALID,
        UNDEFIND,
        MINLIMIT_OVERFLOW,
        MAXLIMIT_OVERFOLW,
        NOT_FOUND_JUMPINDEX,
        NONE,
        PATTERN_NOT_FOUND,
        REGISTE_PATTERN_USER_CANCEL,
        EXCEPTION
    }

    public abstract class ThetaAlignStandard : PNPSetupBase
    {
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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
        [ParamIgnore]
        public new List<object> Nodes { get; set; }

        public override bool Initialized { get; set; } = false;

        private double DeadZone = 10000; // 1cm

        private double _PostMiniumumLength = 0.0;
        private double _PostOptimumLength = 0.0;
        private double _PostMaximumLength = 0.0;

        private double _MinimumLength = 0.0;
        private double _OptimumLength = 0.0;
        private double _MaximumLength = 0.0;

        private double _RetryFocusingROIMargin_X = 40;
        private double _RetryFocusingROIMargin_Y = 40;

        private bool IsInfo = true;

        protected IFocusing FocusingModule { get; set; }
        protected IFocusParameter FocusingParam { get; set; }

        private double settling = 0.25;

        private ProbeAxisObject axisX;
        private ProbeAxisObject axisY;
        private ProbeAxisObject axisZ;

        private CancellationTokenSource cancellationToken;
        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }
        public abstract SubModuleStateBase SubModuleState { get; set; }

        ObservableCollection<WaferCoordinate> HeightProgiling = new ObservableCollection<WaferCoordinate>();

        public override EventCodeEnum InitModule()
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
                LoggerManager.Error(err + "InitModule() : Error occured.");
                LoggerManager.Exception(err);

                throw;
            }

            return retval;
        }

        public void SettingLimit(double minium, double optinum, double maxinum)
        {
            try
            {
                _MinimumLength = minium;
                _OptimumLength = optinum;
                _MaximumLength = maxinum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SettingLimit(double postminium, double postoptinum, double postmaxinum, double minium, double optinum, double maxinum)
        {
            try
            {
                _PostMiniumumLength = postminium;
                _PostOptimumLength = postoptinum;
                _PostMaximumLength = postmaxinum;
                _MinimumLength = minium;
                _OptimumLength = optinum;
                _MaximumLength = maxinum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SettingFocusingROIWithPatternSizeParam(double RetryFocusingROIMargin_X, double RetryFocusingROIMargin_Y)
        {
            try
            {
                _RetryFocusingROIMargin_X = RetryFocusingROIMargin_X;
                _RetryFocusingROIMargin_Y = RetryFocusingROIMargin_Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _IsPostAlign = false;

        /// <summary>
        /// 실제 ThetaAlign
        /// </summary>
        /// 
        public EventCodeEnum ThetaAlign(ref List<WAStandardPTInfomation> ptinfos, ref List<WaferProcResult> procresults, ref double rotateangle, CancellationTokenSource cancelToken, bool doAlign = true, bool revisionwc = true)

        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                _IsPostAlign = true;
                cancellationToken = cancelToken;
                HeightProfiling = new List<Tuple<HeightProfilignPosEnum, WaferCoordinate>>();

                if (this.WaferAligner().WaferAlignInfo.AlignProcResult != null)
                {
                    this.WaferAligner().WaferAlignInfo.AlignProcResult.Clear();
                }

                if (procresults != null)
                {
                    procresults.Clear();
                }

                for (int ptinfoindex = 0; ptinfoindex < ptinfos.Count; ptinfoindex++)
                {
                    procresults.Clear();

                    LoggerManager.Debug($"[ThetaAling] Processing() " + $"Pattern Index : {ptinfoindex + 1}", isInfo: IsInfo);

                    if (ptinfos[ptinfoindex].PostJumpIndex != null)
                    {
                        if (ptinfos[ptinfoindex].PostJumpIndex.Count != 0)
                        {
                            _IsPostAlign = true;

                            ret = Processing(ptinfos[ptinfoindex], ptinfos[ptinfoindex].PostJumpIndex, ref procresults, doAlign, null, false, revisionwc);

                            if (ret == EventCodeEnum.NONE)
                            {
                                if (ptinfos[ptinfoindex].PostProcDirection.Value == EnumWAProcDirection.VERTICAL ||
                                    ptinfos[ptinfoindex].PostProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                                {
                                    ret = RevisionVertical(ptinfos[ptinfoindex], procresults, rotateangle);
                                }

                                if (HeightProfiling.Count > 0)
                                {
                                    this.WaferAligner().ResetHeightPlanePoint();
                                    foreach (var height in HeightProfiling)
                                    {
                                        this.WaferAligner().AddHeighPlanePoint(new WAHeightPositionParam(height.Item2), true);
                                    }
                                }
                            }
                            else
                            {
                                if (ptinfos.Count == ptinfoindex)
                                {
                                    ptinfos[ptinfoindex].ErrorCode = ret;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ret = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (ret == EventCodeEnum.NONE)
                    {
                        if (ptinfos[ptinfoindex].JumpIndexs.Count != 0)
                        {
                            _IsPostAlign = false;

                            ret = Processing(ptinfos[ptinfoindex], ptinfos[ptinfoindex].JumpIndexs, ref procresults, doAlign, null, false, revisionwc);

                            if (ret == EventCodeEnum.NONE)
                            {
                                if (ptinfos[ptinfoindex].ProcDirection.Value == EnumWAProcDirection.VERTICAL ||
                                    ptinfos[ptinfoindex].ProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                                {
                                    ret = RevisionVertical(ptinfos[ptinfoindex], procresults, rotateangle);
                                }
                                break;
                            }
                            else
                            {
                                if (ptinfos.Count == ptinfoindex)
                                {
                                    ptinfos[ptinfoindex].ErrorCode = ret;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ptinfos.Count == ptinfoindex)
                        {
                            break;
                        }
                    }
                }

                PatternIndexSort(ptinfos);

                if (ret == EventCodeEnum.NONE)
                {
                    List<WaferCoordinate> planepoint = this.WaferAligner().GetHieghtPlanePoint();
                    int index = 0;

                    foreach (var plane in planepoint)
                    {
                        LoggerManager.Debug($"PlanePoint Total Count : {planepoint.Count}, Index : {index} X: {plane.GetX():0.00}, Y: {plane.GetY():0.00}, Z: {plane.GetZ():0.00}", isInfo: IsInfo);
                        index++;
                    }

                    foreach (var result in procresults)
                    {
                        this.WaferAligner().WaferAlignInfo.AlignProcResult.Add(result);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.ToString()} ThetaAlignStadnard - ThetaAlign () : Error occurred.");
                throw err;
            }
            finally
            {

                if (!this.VisionManager().ConfirmDigitizerEmulMode(ptinfos[0].CamType.Value))
                {
                    this.VisionManager().StartGrab(ptinfos[0].CamType.Value, this);
                }
                else
                {
                    this.VisionManager().ClearGrabberUserImage(ptinfos[0].CamType.Value);
                    this.VisionManager().StopGrab(ptinfos[0].CamType.Value);
                }
            }

            return ret;
        }

        /// <summary>
        /// 패턴저장시 JumpIndex 찾아주고 Prociessing
        /// </summary>
        public WAStandardPTInfomation ThetaAlign(WAStandardPTInfomation ptinfo, ref double angle, CancellationTokenSource cancelToken,
            double minimumlength, double optimumlength, double maximumlength, bool originproc = true, List<WaferProcResult> procresult = null, bool ispost = false, bool revisionwc = true)
        {
            try
            {
                cancellationToken = cancelToken;

                if (this.WaferAligner().WaferAlignInfo.AlignProcResult != null)
                {
                    this.WaferAligner().WaferAlignInfo.AlignProcResult.Clear();
                }

                if (procresult != null)
                {
                    procresult.Clear();
                }

                if (ptinfo != null)
                {
                    try
                    {
                        if (!ispost)
                        {
                            if (ptinfo.ProcDirection.Value == EnumWAProcDirection.HORIZONTAL ||
                                ptinfo.ProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                double orgAcceptance = ptinfo.PMParameter.PMAcceptance.Value;

                                if (!SearchInvailedJumpIndex(ptinfo.ProcDirection.Value, ptinfo))
                                {
                                    if (ptinfo.JumpIndexs == null)
                                    {
                                        ptinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                                    }

                                    ptinfo = SearchHorJumpIndex(minimumlength, optimumlength, maximumlength, ptinfo, originproc, true, ispost);
                                }
                            }

                            if (ptinfo.ProcDirection.Value == EnumWAProcDirection.VERTICAL ||
                                ptinfo.ProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                if (ptinfo.ProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                                {
                                    if (ptinfo.ErrorCode != EventCodeEnum.NONE)
                                    {
                                        return ptinfo;
                                    }
                                }

                                if (!SearchInvailedJumpIndex(ptinfo.ProcDirection.Value, ptinfo))
                                {
                                    if (ptinfo.JumpIndexs == null)
                                    {
                                        ptinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                                    }

                                    ptinfo = SearchVertJumpIndex(minimumlength, optimumlength, maximumlength, ptinfo, originproc, true, ispost);
                                }
                            }
                        }
                        else
                        {
                            if (ptinfo.PostProcDirection.Value == EnumWAProcDirection.HORIZONTAL ||
                                ptinfo.PostProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                double orgAcceptance = ptinfo.PMParameter.PMAcceptance.Value;

                                if (!SearchInvailedJumpIndex(ptinfo.PostProcDirection.Value, ptinfo))
                                {
                                    if (ptinfo.JumpIndexs == null)
                                    {
                                        ptinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                                    }

                                    ptinfo = SearchHorJumpIndex(minimumlength, optimumlength, maximumlength, ptinfo, originproc, true, ispost);
                                }
                            }

                            if (ptinfo.PostProcDirection.Value == EnumWAProcDirection.VERTICAL ||
                                ptinfo.PostProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                if (!SearchInvailedJumpIndex(ptinfo.PostProcDirection.Value, ptinfo))
                                {
                                    if (ptinfo.JumpIndexs == null)
                                    {
                                        ptinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                                    }

                                    ptinfo = SearchVertJumpIndex(minimumlength, optimumlength, maximumlength, ptinfo, originproc, true, ispost);
                                }
                            }
                        }

                        if (ptinfo.ErrorCode == EventCodeEnum.NONE)
                        {
                            List<WAStandardPTInfomation> ptinfos = new List<WAStandardPTInfomation>();

                            if (procresult == null)
                            {
                                procresult = new List<WaferProcResult>();
                            }

                            ptinfos.Add(ptinfo);

                            if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                            {
                                ptinfo = SetHighSetupHeightProfiling(ptinfo);
                            }

                            EventCodeEnum retVal = ThetaAlign(ref ptinfos, ref procresult, ref angle, cancelToken, false, false);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"{err.ToString()} ThetaAlignStadnard - ThetaAlign () : Error occurred.");
                        throw err;
                    }
                    finally
                    {
                        this.VisionManager().StartGrab(ptinfo.CamType.Value, this);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ptinfo;
        }

        private WAStandardPTInfomation SetHighSetupHeightProfiling(WAStandardPTInfomation mptinfo)
        {
            ObservableCollection<StandardJumpIndexParam> highjumpparam = new ObservableCollection<StandardJumpIndexParam>();
            List<StandardJumpIndexParam> tmpjumpparam = new List<StandardJumpIndexParam>();

            try
            {
                foreach (var info in mptinfo.PostJumpIndex)
                {
                    tmpjumpparam.Add(info);
                }

                foreach (var info in mptinfo.JumpIndexs)
                {
                    tmpjumpparam.Add(info);
                }

                StandardJumpIndexParam jiparam = tmpjumpparam.Find(jumpindex => jumpindex.Index.XIndex == 0 && jumpindex.Index.YIndex == 0);

                if (jiparam == null)
                {
                    jiparam = mptinfo.PostJumpIndex.ToList<StandardJumpIndexParam>().Find(jumpindex => jumpindex.Index.XIndex == 0 && jumpindex.Index.YIndex == 0);
                }

                highjumpparam.Add(jiparam);

                tmpjumpparam.Sort(delegate (StandardJumpIndexParam A, StandardJumpIndexParam B)
                {
                    if (A.Index.XIndex > B.Index.XIndex)
                    {
                        return 1;
                    }
                    else if (A.Index.XIndex < B.Index.XIndex)
                    {
                        return -1;
                    }

                    return 0;
                });

                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find(jumpindex => jumpindex.Index.XIndex == tmpjumpparam[0].Index.XIndex && jumpindex.Index.YIndex == tmpjumpparam[0].Index.YIndex));
                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find(jumpindex => jumpindex.Index.XIndex == tmpjumpparam[tmpjumpparam.Count() - 1].Index.XIndex && jumpindex.Index.YIndex == tmpjumpparam[tmpjumpparam.Count() - 1].Index.YIndex));

                tmpjumpparam.Sort(delegate (StandardJumpIndexParam A, StandardJumpIndexParam B)
                {
                    if (A.Index.YIndex > B.Index.YIndex)
                    {
                        return 1;
                    }
                    else if (A.Index.YIndex < B.Index.YIndex)
                    {
                        return -1;
                    }

                    return 0;
                });

                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find(jumpindex => jumpindex.Index.XIndex == tmpjumpparam[0].Index.XIndex && jumpindex.Index.YIndex == tmpjumpparam[0].Index.YIndex));
                highjumpparam.Add(mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find(jumpindex => jumpindex.Index.XIndex == tmpjumpparam[tmpjumpparam.Count() - 1].Index.XIndex && jumpindex.Index.YIndex == tmpjumpparam[tmpjumpparam.Count() - 1].Index.YIndex));

                foreach (var param in highjumpparam)
                {
                    if (param != null)
                    {
                        StandardJumpIndexParam jparam = mptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().Find(jindex => jindex.Index.XIndex == param.Index.XIndex && jindex.Index.YIndex == param.Index.YIndex);

                        if (jparam != null)
                        {
                            jparam.AcceptFocusing.Value = true;
                        }
                        else
                        {
                            jparam = mptinfo.PostJumpIndex.ToList<StandardJumpIndexParam>().Find(jindex => jindex.Index.XIndex == param.Index.XIndex && jindex.Index.YIndex == param.Index.YIndex);

                            if (jparam != null)
                            {
                                jparam.AcceptFocusing.Value = true;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, err.Message);
                throw err;
            }

            return mptinfo;
        }
        //Dictionary<HeightProfilignPosEnum, List<WaferCoordinate>> HeightProgiling = new Dictionary<HeightProfilignPosEnum, List<WaferCoordinate>>(); //dictionary 중복 key 허용
        List<Tuple<HeightProfilignPosEnum, WaferCoordinate>> HeightProfiling = new List<Tuple<HeightProfilignPosEnum, WaferCoordinate>>();
        public EventCodeEnum ThetaAlign(ref WAStandardPTInfomation ptinfo, CancellationTokenSource cancelToken, ref double rotateangle, bool revisionwc = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                cancellationToken = cancelToken;

                this.WaferAligner().WaferAlignInfo.AlignProcResult.Clear();

                List<WAStandardPTInfomation> ptinfos = new List<WAStandardPTInfomation>();
                List<WaferProcResult> procresults = new List<WaferProcResult>();

                ptinfos.Add(ptinfo);

                if (ptinfo.PostJumpIndex != null)
                {
                    if (ptinfo.PostJumpIndex.Count != 0)
                    {
                        retVal = Processing(ptinfo, ptinfo.PostJumpIndex, ref procresults, false, null, false, revisionwc);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            if (ptinfo.PostProcDirection.Value == EnumWAProcDirection.HORIZONTAL ||
                                ptinfo.PostProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                retVal = RevisionHorizontal(ptinfo, procresults, ref rotateangle, revisionwc);
                            }
                            if (ptinfo.PostProcDirection.Value == EnumWAProcDirection.VERTICAL ||
                                ptinfo.PostProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                retVal = RevisionVertical(ptinfo, procresults, rotateangle);
                            }
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    if (ptinfo.JumpIndexs.Count != 0)
                    {
                        retVal = Processing(ptinfo, ptinfo.JumpIndexs, ref procresults, false, null, false, revisionwc);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            if (ptinfo.ProcDirection.Value == EnumWAProcDirection.VERTICAL ||
                                ptinfo.ProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                retVal = RevisionVertical(ptinfo, procresults, rotateangle);
                            }
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    foreach (var result in procresults)
                    {
                        this.WaferAligner().WaferAlignInfo.AlignProcResult.Add(result);
                    }
                }

                ptinfo.ErrorCode = retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        /// <summary>
        /// Theta Alignment 후에 Wafer Center 를 다시 검증하기 위한 함수.
        /// Theta 를 돌리기전에 Processing 한 원본 데이터 (jump index 0,0 pattern)로 Wafer Center 를 보정하는데, 
        /// 보정시에 프로세싱 결과데이터인 Angle사용하여 원본데이터 프로세싱 결과 데이터를 계산하여 보정한다.
        /// 이때, 오차가 있을 수 있으므로 모든 얼라인이 끝난 후에, 원본 데이터 위치에 가서 다시 확인한다. 
        /// 이 과정은 High Mag 과정에서만 수행되면 된다.
        /// </summary>
        protected EventCodeEnum VerifyRevisionWaferCenter(WAStandardPTInfomation ptinfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string tmpfilpath = "";
            try
            {
                if (ptinfo != null)
                {
                    if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        LoggerManager.Debug("VerifyRevisionWaferCenter() Start.");

                        double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
                        double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

                        double movex = 0.0;
                        double movey = 0.0;
                        double movez = 0.0;

                        tmpfilpath = ptinfo.PMParameter.ModelFilePath.Value;

                        axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                        axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                        axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                        this.MotionManager().SetSettlingTime(axisX, settling);
                        this.MotionManager().SetSettlingTime(axisY, settling);
                        this.MotionManager().SetSettlingTime(axisZ, settling);

                        ICamera cam = this.VisionManager().GetCam(ptinfo.CamType.Value);

                        movex = (ptinfo.GetX() + wafercenterx);
                        movey = (ptinfo.GetY() + wafercentery);
                        movez = this.WaferObject.GetSubsInfo().ActualThickness;

                        retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(movex, movey, movez);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            return retVal;
                        }

                        foreach (var light in ptinfo.LightParams)
                        {
                            cam.SetLight(light.Type.Value, light.Value.Value);
                        }

                        ptinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(ptinfo.PMParameter.ModelFilePath.Value);

                        PMResult pmresult = this.VisionManager().PatternMatching(ptinfo, this, true, retryautolight: true, retrySuccessedLight: true);

                        if (pmresult != null)
                        {
                            retVal = pmresult.RetValue;
                        }
                        else
                        {
                            return EventCodeEnum.VISION_PM_NOT_DEFINED_ERROR;
                        }

                        if(retVal != EventCodeEnum.NONE)
                        {
                            retVal = RetryFocusingAndPMWithPatternSize(pmresult, ptinfo);
                        }

                        if (retVal == EventCodeEnum.NONE)
                        {
                            WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);
                            double wcoffsetX = wcoord.GetX() - movex;
                            double wcoffsetY = wcoord.GetY() - movey;

                            var verifyWaferXYLimit = this.WaferAligner().GetVerifyCenterLimitXYValue();
                            double limitX = verifyWaferXYLimit.Item1;
                            double limitY = verifyWaferXYLimit.Item2;

                            if ((limitX != 0 && wcoffsetX > limitX) || (limitY != 0 && wcoffsetY > limitY))
                            {
                                double curThetaRef = 0.0;
                                double curThetaActual = 0.0;
                                this.MotionManager().GetRefPos(EnumAxisConstants.C, ref curThetaRef);
                                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curThetaActual);

                                LoggerManager.Debug($"[WA]CenterOffsetX : {wcoffsetX}. CenterOffsetY : {wcoffsetY}", isInfo: IsInfo);
                                LoggerManager.Debug($"VerifyRevisionWaferCenter() fail. Limit X : {limitX}, Limit Y : {limitY}", isInfo: IsInfo);
                                LoggerManager.Debug($"WaferAlign Angle : {this.WaferAligner().WaferAlignInfo.AlignAngle}," +
                                    $"Theta RefPos : {curThetaRef}, Theta Actual Pos: {curThetaActual}, " +
                                    $"Rotation Center X : {this.CoordinateManager().StageCoord.MarkPosInChuckCoord.GetX()}," +
                                    $"Rotation Center Y : {this.CoordinateManager().StageCoord.MarkPosInChuckCoord.GetY()}.", isInfo: IsInfo);

                                retVal = EventCodeEnum.WAFER_ALIGN_THETA_COMPENSATION_FAIL;
                            }
                            else
                            {
                                //Wafer.GetSubsInfo().WaferCenter.X.Value += wcoffsetX;
                                //Wafer.GetSubsInfo().WaferCenter.Y.Value += wcoffsetY;
                                Wafer.GetSubsInfo().WaferCenter.X.Value = wcoord.GetX() - ptinfo.GetX();
                                Wafer.GetSubsInfo().WaferCenter.Y.Value = wcoord.GetY() - ptinfo.GetY();

                                Wafer.GetSubsInfo().RefDieLeftCorner.X.Value = this.WaferAligner().GetLeftCornerPosition(Wafer.GetSubsInfo().WaferCenter.X.Value, Wafer.GetSubsInfo().WaferCenter.Y.Value).X;
                                Wafer.GetSubsInfo().RefDieLeftCorner.Y.Value = this.WaferAligner().GetLeftCornerPosition(Wafer.GetSubsInfo().WaferCenter.X.Value, Wafer.GetSubsInfo().WaferCenter.Y.Value).Y;

                                LoggerManager.Debug($"[WA]WaferCenter X : {Wafer.GetSubsInfo().WaferCenter.X.Value}, " +
                                    $"WaferCetner Y : {Wafer.GetSubsInfo().WaferCenter.Y.Value}," +
                                    $"Result X Position : {wcoord.GetX()}, Result Y Position : {wcoord.GetY()}," +
                                    $" Pattern OffsetX : {ptinfo.GetX()}, Pattern OffsetY : {ptinfo.GetY()}", isInfo: IsInfo);
                                //LoggerManager.Debug($"[WA]CenterOffsetX : {wcoffsetX}. CenterOffsetY : {wcoffsetY}", isInfo: IsInfo);
                            }
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (tmpfilpath != "") 
                {//patternmathcing exception 발생하면 backup안될 수 있기 때문에 위치 변경
                    ptinfo.PMParameter.ModelFilePath.Value = tmpfilpath;
                }
            }

            return retVal;
        }

        private bool SearchInvailedJumpIndex(EnumWAProcDirection direction, WAStandardPTInfomation ptinfos)
        {
            bool retVal = true;

            try
            {
                if (direction == EnumWAProcDirection.VERTICAL ||
                    direction == EnumWAProcDirection.BIDIRECTIONAL)
                {
                    if (ptinfos.JumpIndexs != null)
                    {
                        List<StandardJumpIndexParam> jumpindexparams = ptinfos.JumpIndexs.ToList<StandardJumpIndexParam>();

                        if (jumpindexparams.FindAll(item => item.Index.XIndex != 0).Count == 0)
                        {
                            retVal = false;
                            return retVal;
                        }
                    }
                    else
                    {
                        retVal = false;
                        return retVal;
                    }

                }
                if (direction == EnumWAProcDirection.HORIZONTAL ||
                    direction == EnumWAProcDirection.BIDIRECTIONAL)
                {

                    if (ptinfos.JumpIndexs != null)
                    {
                        List<StandardJumpIndexParam> jumpindexparams = ptinfos.JumpIndexs.ToList<StandardJumpIndexParam>();

                        if (jumpindexparams.FindAll(item => item.Index.YIndex != 0).Count == 0)
                        {
                            retVal = false;
                            return retVal;
                        }
                    }
                    else
                    {
                        retVal = false;
                        return retVal;
                    }
                }

                ptinfos.ErrorCode = EventCodeEnum.NONE;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private WAStandardPTInfomation SearchHorJumpIndex(double minimumlength, double optimumlength, double maximumlength,
            WAStandardPTInfomation ptinfo, bool originproc, bool addindex = true, bool ispost = false)
        {
            // direction : 이동 방향
            // -1 : 왼쪽
            //  1 : 오른쪽
            int direction = 0;

            double indexwidth = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
            double indexheight = Wafer.GetSubsInfo().ActualDieSize.Height.Value;
            double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
            double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

            double acminimumlength = Math.Ceiling(minimumlength / indexwidth) * indexwidth;
            double acoptimumlength = Math.Ceiling(optimumlength / indexwidth) * indexwidth;
            double acmaximumlength = Math.Ceiling(maximumlength / indexwidth) * indexwidth;

            double movex = ptinfo.GetX() + wafercenterx;
            double movey = ptinfo.GetY() + wafercentery;

            double edgeXlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value / 2), 2) - Math.Pow(movey, 2)) - DeadZone;

            int leftmoveindex = 0;
            int rightmoveindex = 0;

            double leftJumpIndexMaxlimit = 0.0;
            double leftJumpIndexMinlimit = 0.0;
            double rightJumpIndexMinlimit = 0.0;
            double rightJumpIndexMaxlimit = 0.0;

            ThetaAlignEventCodeEnum ret = ThetaAlignEventCodeEnum.UNDEFIND;

            try
            {
                //WaferCenter보다 패턴위치 -  : -1 |  + : 1
                direction = ptinfo.GetX() >= wafercenterx ? -1 : 1;

                if (direction == -1)
                {
                    double remainlength = edgeXlimit - (Math.Abs(movex));

                    if (remainlength >= acminimumlength / 2)
                    {
                        if (remainlength >= acoptimumlength / 2)
                        {
                            leftmoveindex = Calc(1, indexwidth, -((acoptimumlength / 2) + Math.Abs(movex)), movex);
                        }
                        else
                        {
                            leftmoveindex = Calc(1, indexwidth, -((acminimumlength / 2) + Math.Abs(movex)), movex);
                        }

                        rightmoveindex = leftmoveindex;

                        leftJumpIndexMaxlimit = movex + (-acmaximumlength / 2);
                        leftJumpIndexMinlimit = movex + (-acminimumlength / 2);

                        rightJumpIndexMinlimit = movex + (acminimumlength / 2);
                        rightJumpIndexMaxlimit = movex + (acmaximumlength / 2);
                    }
                    else
                    {
                        remainlength = edgeXlimit + (Math.Abs(movex));

                        if (remainlength >= acminimumlength)
                        {
                            leftmoveindex = 0;

                            if (remainlength >= acoptimumlength)
                            {
                                rightmoveindex = Calc(1, indexwidth, (movex + acoptimumlength), movex);
                            }
                            else
                            {
                                rightmoveindex = Calc(1, indexwidth, (movex + acminimumlength), movex);
                            }

                            leftJumpIndexMaxlimit = movex;
                            leftJumpIndexMinlimit = movex;

                            rightJumpIndexMinlimit = movex + acmaximumlength;
                            rightJumpIndexMaxlimit = movex + remainlength;
                        }
                        else
                        {
                            leftmoveindex = -1;
                            rightmoveindex = -1;
                        }
                    }
                }
                else
                {
                    double remainlength = edgeXlimit + (Math.Abs(movex));

                    if (remainlength >= acminimumlength / 2)
                    {
                        if (remainlength >= acoptimumlength / 2)
                        {
                            rightmoveindex = Calc(1, indexwidth, (movex + (acoptimumlength / 2)), movex);
                        }
                        else
                        {
                            rightmoveindex = Calc(1, indexwidth, (movex + (acminimumlength / 2)), movex);
                        }

                        leftmoveindex = rightmoveindex;

                        rightJumpIndexMinlimit = movex + (acminimumlength / 2);
                        rightJumpIndexMaxlimit = movex + (acmaximumlength / 2);

                        leftJumpIndexMaxlimit = movex + (-acmaximumlength / 2);
                        leftJumpIndexMinlimit = movex + (-acminimumlength / 2);
                    }
                    else
                    {
                        remainlength = edgeXlimit - (Math.Abs(movex));

                        if (remainlength >= acminimumlength)
                        {
                            if (remainlength >= acoptimumlength)
                            {
                                leftmoveindex = Calc(1, indexwidth, -((acoptimumlength / 2) + Math.Abs(movex)), movex);
                            }
                            else
                            {
                                leftmoveindex = Calc(1, indexwidth, -((acminimumlength / 2) + Math.Abs(movex)), movex);
                            }

                            rightmoveindex = 0;

                            rightJumpIndexMinlimit = movex;
                            rightJumpIndexMaxlimit = movex;

                            leftJumpIndexMaxlimit = movex + (-remainlength);
                            leftJumpIndexMinlimit = movex + (-acminimumlength);
                        }
                        else
                        {
                            leftmoveindex = -1;
                            rightmoveindex = -1;
                        }
                    }
                }

                if (leftmoveindex != -1)
                {
                    int movecount = leftmoveindex;
                    int trycount = 0;

                    if (leftmoveindex != 0)
                    {
                        while (true)
                        {
                            if ((trycount + 1) == 3)
                            {
                                FocusingModule.Focusing_Retry(FocusingParam, false, false, true, this);
                                trycount = 0;
                            }

                            ret = FindJumpIndex(ptinfo, EnumDirection.VER, movecount, 0, -1, leftJumpIndexMinlimit, leftJumpIndexMaxlimit);

                            if (ret == ThetaAlignEventCodeEnum.NONE)
                            {
                                leftmoveindex = -movecount;
                                break;
                            }
                            else if (ret == ThetaAlignEventCodeEnum.NOT_FOUND_JUMPINDEX ||
                                     ret == ThetaAlignEventCodeEnum.MINLIMIT_OVERFLOW)
                            {
                                leftmoveindex = -1;
                                break;
                            }
                            else if (ret == ThetaAlignEventCodeEnum.PATTERN_NOT_FOUND)
                            {
                                movecount--;
                                trycount++;

                                if (movecount == 0)
                                {
                                    leftmoveindex = -1;
                                    break;
                                }
                            }
                            else
                            {
                                //Exception
                                leftmoveindex = -1;
                                break;
                            }

                            Thread.Sleep(1);
                        }
                    }
                }

                if (leftmoveindex != -1)
                {
                    if (rightmoveindex != -1)
                    {
                        int movecount = rightmoveindex;

                        if (rightmoveindex != 0)
                        {
                            int trycount = 0;

                            while (true)
                            {
                                if ((trycount + 1) == 3)
                                {
                                    FocusingModule.Focusing_Retry(FocusingParam, false, false, true, this);
                                    trycount = 0;
                                }

                                ret = FindJumpIndex(ptinfo, EnumDirection.VER, movecount, 0, 1, rightJumpIndexMinlimit, rightJumpIndexMaxlimit);

                                if (ret == ThetaAlignEventCodeEnum.NONE)
                                {
                                    rightmoveindex = movecount;
                                    break;
                                }
                                else if (ret == ThetaAlignEventCodeEnum.NOT_FOUND_JUMPINDEX ||
                                         ret == ThetaAlignEventCodeEnum.MINLIMIT_OVERFLOW)
                                {
                                    rightmoveindex = -1;
                                    break;
                                }
                                else if (ret == ThetaAlignEventCodeEnum.PATTERN_NOT_FOUND)
                                {
                                    movecount--;
                                    trycount++;

                                    if (movecount == 0)
                                    {
                                        rightmoveindex = -1;
                                        break;
                                    }
                                }
                                else
                                {
                                    rightmoveindex = -1;
                                    break;
                                }

                                Thread.Sleep(1);
                            }
                        }
                    }
                }

                if (leftmoveindex != -1 && rightmoveindex != -1)
                {
                    double retdistance = (Math.Abs(leftmoveindex) + Math.Abs(rightmoveindex)) * indexwidth;

                    if (addindex)
                    {
                        if (originproc)
                        {
                            if (ptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().FindAll(index => index.Index.XIndex == 0 && index.Index.YIndex == 0).Count == 0)
                            {
                                if (!ispost)
                                {
                                    ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, 0, false));
                                }
                                else
                                {
                                    ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(0, 0, false));
                                }
                            }
                        }

                        if (!ispost)
                        {
                            if (ptinfo.HorDirection.Value == EnumHorDirection.LEFTRIGHT)
                            {
                                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(leftmoveindex, 0, false));
                                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(rightmoveindex, 0, false));
                            }
                            else if (ptinfo.HorDirection.Value == EnumHorDirection.RIGHTLEFT)
                            {
                                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(rightmoveindex, 0, false));
                                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(leftmoveindex, 0, false));
                            }
                        }
                        else
                        {
                            if (ptinfo.PostHorDirection.Value == EnumHorDirection.LEFTRIGHT)
                            {
                                ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(leftmoveindex, 0, false));
                                ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(rightmoveindex, 0, false));
                            }
                            else if (ptinfo.PostHorDirection.Value == EnumHorDirection.RIGHTLEFT)
                            {
                                ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(rightmoveindex, 0, false));
                                ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(leftmoveindex, 0, false));
                            }
                        }
                    }
                    else
                    {
                        WAStandardPTInfomation tmpptinfo = new WAStandardPTInfomation();
                        tmpptinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                        tmpptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, 0, ptinfo.JumpIndexs[0].AcceptFocusing.Value));

                        if (!ispost)
                        {
                            if (ptinfo.HorDirection.Value == EnumHorDirection.LEFTRIGHT)
                            {

                                tmpptinfo.JumpIndexs.Add(new StandardJumpIndexParam((leftmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                                tmpptinfo.JumpIndexs.Add(new StandardJumpIndexParam((rightmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                            }
                            else if (ptinfo.HorDirection.Value == EnumHorDirection.RIGHTLEFT)
                            {
                                tmpptinfo.JumpIndexs.Add(new StandardJumpIndexParam((rightmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                                tmpptinfo.JumpIndexs.Add(new StandardJumpIndexParam((leftmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                            }
                        }
                        else
                        {
                            if (ptinfo.PostHorDirection.Value == EnumHorDirection.LEFTRIGHT)
                            {
                                tmpptinfo.PostJumpIndex.Add(new StandardJumpIndexParam((leftmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                                tmpptinfo.PostJumpIndex.Add(new StandardJumpIndexParam((rightmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                            }
                            else if (ptinfo.PostHorDirection.Value == EnumHorDirection.RIGHTLEFT)
                            {
                                tmpptinfo.PostJumpIndex.Add(new StandardJumpIndexParam((rightmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                                tmpptinfo.PostJumpIndex.Add(new StandardJumpIndexParam((leftmoveindex * direction), 0, ptinfo.JumpIndexs[1].AcceptFocusing.Value));
                            }
                        }

                        return tmpptinfo;
                    }

                    ptinfo.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    if (ret == ThetaAlignEventCodeEnum.REGISTE_PATTERN_USER_CANCEL)
                    {
                        ptinfo.ErrorCode = EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }
                    else
                    {
                        ptinfo.ErrorCode = EventCodeEnum.WAFER_JUMPINDEX_NOT_FOUND;
                        LoggerManager.Error($"WAFER_JUMPINDEX_NOT_FOUND : [LEFTINDEX] : {leftmoveindex}, [RIGHTINDEX] : {rightmoveindex}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.ToString()}.SearchHorJumpIndex() : Error occurred.");
                throw err;
            }

            return ptinfo;
        }

        private WAStandardPTInfomation SearchVertJumpIndex(double minimumlength, double optimumlength,
            double maximumlength, WAStandardPTInfomation ptinfo, bool originproc, bool addindex = true, bool ispost = false)
        {

            // direction : 이동 방향
            // -1 : 아래쪽
            //  1 : 위쪽
            int direction = 0;

            double indexwidth = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
            double indexheight = Wafer.GetSubsInfo().ActualDieSize.Height.Value;
            double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
            double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

            double acminimumlength = Math.Ceiling(minimumlength / indexwidth) * indexwidth;
            double acoptimumlength = Math.Ceiling(optimumlength / indexwidth) * indexwidth;
            double acmaximumlength = Math.Ceiling(maximumlength / indexwidth) * indexwidth;

            double movex = ptinfo.GetX() + wafercenterx;
            double movey = ptinfo.GetY() + wafercentery;

            double edgeYlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value / 2), 2) - Math.Pow(movex, 2)) - DeadZone;

            int uppermoveindex = 0;
            int bottommoveindex = 0;

            double upperJumpIndexMaxlimit = 0.0;
            double upperJumpIndexMinlimit = 0.0;
            double bottomJumpIndexMaxlimit = 0.0;
            double bottomJumpIndexMinlimit = 0.0;

            ThetaAlignEventCodeEnum ret = ThetaAlignEventCodeEnum.UNDEFIND;

            try
            {
                direction = ptinfo.GetX() >= wafercentery ? 1 : -1;

                if (direction == -1) //아래쪽이 남은 길이가 더 짧다.
                {
                    double remainlength = edgeYlimit - (Math.Abs(movey));

                    if (remainlength >= acminimumlength / 2)
                    {
                        if (remainlength >= acoptimumlength / 2)
                        {
                            bottommoveindex = Calc(1, indexheight, -((acoptimumlength / 2) + Math.Abs(movey)), movey);
                        }
                        else
                        {
                            bottommoveindex = Calc(1, indexheight, -((acminimumlength / 2) + Math.Abs(movey)), movey);
                        }

                        uppermoveindex = bottommoveindex;

                        bottomJumpIndexMaxlimit = movey + (-acmaximumlength / 2);
                        bottomJumpIndexMinlimit = movey + (-acminimumlength / 2);

                        upperJumpIndexMaxlimit = movey + (acmaximumlength / 2);
                        upperJumpIndexMinlimit = movey + (acminimumlength / 2);
                    }
                    else
                    {
                        remainlength = edgeYlimit + Math.Abs(movey);

                        if (remainlength >= acminimumlength)
                        {
                            bottommoveindex = 0;

                            if (remainlength >= acoptimumlength)
                            {
                                uppermoveindex = Calc(1, indexheight, (movey + acminimumlength), movey);
                            }
                            else
                            {
                                uppermoveindex = Calc(1, indexheight, (movey + acoptimumlength), movey);
                            }

                            bottomJumpIndexMaxlimit = movey;
                            bottomJumpIndexMinlimit = movey;

                            upperJumpIndexMaxlimit = movey + acmaximumlength;
                            upperJumpIndexMinlimit = movey + acminimumlength;
                        }
                        else
                        {
                            bottommoveindex = -1;
                            uppermoveindex = -1;
                        }
                    }
                }
                else
                {
                    double remainlength = edgeYlimit + Math.Abs(movey);

                    if (remainlength >= acminimumlength / 2)
                    {
                        if (remainlength >= acoptimumlength / 2)
                        {
                            uppermoveindex = Calc(1, indexheight, (movey + (acoptimumlength / 2)), movey);
                        }
                        else
                        {
                            uppermoveindex = Calc(1, indexheight, (movey + (acminimumlength / 2)), movey);
                        }

                        bottommoveindex = uppermoveindex;

                        upperJumpIndexMaxlimit = movey + (acmaximumlength / 2);
                        upperJumpIndexMinlimit = movey + (acminimumlength / 2);
                        bottomJumpIndexMaxlimit = movey + (-acmaximumlength / 2);
                        bottomJumpIndexMinlimit = movey + (-acminimumlength / 2);
                    }
                    else
                    {
                        remainlength = edgeYlimit - (Math.Abs(movey));

                        if (remainlength >= acminimumlength)
                        {
                            if (remainlength >= acoptimumlength)
                            {
                                bottommoveindex = Calc(1, indexheight, (-acoptimumlength + Math.Abs(movey)), movey);
                            }
                            else
                            {
                                bottommoveindex = Calc(1, indexheight, (-acminimumlength + Math.Abs(movey)), movey);
                            }

                            uppermoveindex = 0;

                            upperJumpIndexMaxlimit = movey;
                            upperJumpIndexMinlimit = movey;
                            bottomJumpIndexMaxlimit = movey + -acmaximumlength;
                            bottomJumpIndexMinlimit = movey + -acminimumlength;
                        }
                        else
                        {
                            uppermoveindex = -1;
                            bottommoveindex = -1;
                        }
                    }
                }

                if (bottommoveindex != -1)
                {
                    int movecount = bottommoveindex;
                    int trycount = 0;

                    if (movecount != 0)
                    {
                        while (true)
                        {
                            if ((trycount + 1) == 3)
                            {
                                FocusingModule.Focusing_Retry(FocusingParam, false, false, true, this);
                                trycount = 0;
                            }

                            ret = FindJumpIndex(ptinfo, EnumDirection.HOR, 0, movecount, -1, bottomJumpIndexMinlimit, bottomJumpIndexMaxlimit);

                            if (ret == ThetaAlignEventCodeEnum.NONE)
                            {
                                bottommoveindex = -movecount;

                                break;
                            }
                            else if (ret == ThetaAlignEventCodeEnum.NOT_FOUND_JUMPINDEX ||
                                ret == ThetaAlignEventCodeEnum.MINLIMIT_OVERFLOW)
                            {
                                bottommoveindex = -1;
                                break;
                            }
                            else if (ret == ThetaAlignEventCodeEnum.PATTERN_NOT_FOUND)
                            {
                                movecount--;
                                trycount++;

                                if (movecount == 0)
                                {
                                    movecount = -1;
                                    break;
                                }
                            }
                            else
                            {
                                //Exception
                                bottommoveindex = -1;
                                break;
                            }

                            Thread.Sleep(1);
                        }
                    }
                }

                if (bottommoveindex != -1)
                {
                    if (uppermoveindex != 0)
                    {
                        int movecount = uppermoveindex;
                        int trycount = 0;

                        while (true)
                        {
                            if ((trycount + 1) == 3)
                            {
                                FocusingModule.Focusing_Retry(FocusingParam, false, false, true, this);
                                trycount = 0;
                            }

                            ret = FindJumpIndex(ptinfo, EnumDirection.HOR, 0, movecount, 1, upperJumpIndexMinlimit, upperJumpIndexMaxlimit);

                            if (ret == ThetaAlignEventCodeEnum.NONE)
                            {
                                uppermoveindex = movecount;
                                break;
                            }
                            else if (ret == ThetaAlignEventCodeEnum.NOT_FOUND_JUMPINDEX ||
                                     ret == ThetaAlignEventCodeEnum.MINLIMIT_OVERFLOW)
                            {
                                uppermoveindex = -1;
                                break;
                            }
                            else if (ret == ThetaAlignEventCodeEnum.PATTERN_NOT_FOUND)
                            {
                                movecount--;
                                trycount++;

                                if (movecount == 0)
                                {
                                    movecount = -1;
                                    break;
                                }
                            }
                            else
                            {
                                uppermoveindex = -1;
                                break;
                            }

                            Thread.Sleep(1);
                        }
                    }
                }

                if (bottommoveindex != -1 && uppermoveindex != -1)
                {
                    double retdistance = (Math.Abs(uppermoveindex) + Math.Abs(bottommoveindex)) * indexwidth;

                    if (originproc)
                    {
                        if (ptinfo.JumpIndexs.ToList<StandardJumpIndexParam>().FindAll(index => index.Index.XIndex == 0 && index.Index.YIndex == 0).Count == 0)
                        {
                            ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, 0, false));
                        }
                    }

                    if (!ispost)
                    {
                        if (ptinfo.VerDirection.Value == EnumVerDirection.UPPERBOTTOM)
                        {
                            ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, uppermoveindex, false));
                            ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, bottommoveindex, false));
                        }
                        else if (ptinfo.VerDirection.Value == EnumVerDirection.BOTTOMUPPER)
                        {
                            ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, bottommoveindex, false));
                            ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, uppermoveindex, false));
                        }
                    }
                    else
                    {
                        if (ptinfo.PostVerDirection.Value == EnumVerDirection.UPPERBOTTOM)
                        {
                            ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(0, uppermoveindex, false));
                            ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(0, bottommoveindex, false));
                        }
                        else if (ptinfo.PostVerDirection.Value == EnumVerDirection.BOTTOMUPPER)
                        {
                            ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(0, bottommoveindex, false));
                            ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(0, uppermoveindex, false));
                        }
                    }
                }
                else
                {
                    if (ret == ThetaAlignEventCodeEnum.REGISTE_PATTERN_USER_CANCEL)
                    {
                        ptinfo.ErrorCode = EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }
                    else
                    {
                        ptinfo.ErrorCode = EventCodeEnum.WAFER_JUMPINDEX_NOT_FOUND;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.ToString()}.SearchHorJumpIndex() Error occurred.");
                throw err;
            }

            return ptinfo;
        }

        private ThetaAlignEventCodeEnum FindJumpIndex(WAStandardPTInfomation ptinfo, EnumDirection jumpdirecion,
            int movecountx, int movecounty, int direction, double minlimit, double maxlimit, bool isfocusing = false)
        {
            ThetaAlignEventCodeEnum retval = ThetaAlignEventCodeEnum.UNDEFIND;

            try
            {
                axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                this.MotionManager().SetSettlingTime(axisX, settling);
                this.MotionManager().SetSettlingTime(axisY, settling);
                this.MotionManager().SetSettlingTime(axisZ, settling);

                double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
                double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

                double movex = (ptinfo.GetX() + wafercenterx) + ((movecountx * Wafer.GetSubsInfo().ActualDieSize.Width.Value) * direction);
                double movey = ptinfo.GetY() + wafercentery + ((movecounty * Wafer.GetSubsInfo().ActualDieSize.Height.Value) * direction);

                if (direction == 1)
                {
                    if (movecountx != 0)
                    {
                        if (movex < minlimit | this.MotionManager().CheckSWLimit(EnumAxisConstants.X, movex) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FindJumpindex - [MAXLIMIT_OVERFOLW] | Ver,Hor :{jumpdirecion} , Direciont : {direction}, MoveX : {movecountx}, MoveY : {movecounty}", isInfo: IsInfo);
                            LoggerManager.Debug($"FindJumpindex - [MAXLIMIT_OVERFOLW] | MovePosX : {movex} , MovePosY : {movey}, MinLimit : {minlimit}", isInfo: IsInfo);

                            return ThetaAlignEventCodeEnum.MAXLIMIT_OVERFOLW;
                        }
                    }
                    else if (movecounty != 0)
                    {
                        if (movey < minlimit | this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, movey) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FindJumpindex - [MAXLIMIT_OVERFOLW] | Ver,Hor :{jumpdirecion} , Direciont : {direction}, MoveX : {movecountx}, MoveY : {movecounty}", isInfo: IsInfo);
                            LoggerManager.Debug($"FindJumpindex - [MAXLIMIT_OVERFOLW] | MovePosX : {movex} , MovePosY : {movey}, MinLimit : {minlimit}", isInfo: IsInfo);

                            return ThetaAlignEventCodeEnum.MAXLIMIT_OVERFOLW;
                        }
                    }
                }
                else
                {
                    if (movecountx != 0)
                    {
                        if (movex > minlimit | this.MotionManager().CheckSWLimit(EnumAxisConstants.X, movex) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FindJumpindex - [MINLIMIT_OVERFLOW] | Ver,Hor :{jumpdirecion} , Direciont : {direction}, MoveX : {movecountx}, MoveY : {movecounty}", isInfo: IsInfo);
                            LoggerManager.Debug($"FindJumpindex - [MINLIMIT_OVERFLOW] | MovePosX : {movex} , MovePosY : {movey}, MinLimit : {minlimit}", isInfo: IsInfo);

                            return ThetaAlignEventCodeEnum.MINLIMIT_OVERFLOW;
                        }
                    }
                    else if (movecounty != 0)
                    {
                        if (movey > minlimit | this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, movey) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FindJumpindex - [MINLIMIT_OVERFLOW] | Ver,Hor :{jumpdirecion} , Direciont : {direction}, MoveX : {movecountx}, MoveY : {movecounty}", isInfo: IsInfo);
                            LoggerManager.Debug($"FindJumpindex - [MINLIMIT_OVERFLOW] | MovePosX : {movex} , MovePosY : {movey}, MinLimit : {minlimit}", isInfo: IsInfo);

                            return ThetaAlignEventCodeEnum.MINLIMIT_OVERFLOW;
                        }
                    }
                }

                if (ptinfo.CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(movex, movey);
                }
                else if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(movex, movey);
                }

                Thread.Sleep(250);

                if (isfocusing != false)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return ThetaAlignEventCodeEnum.REGISTE_PATTERN_USER_CANCEL;
                    }

                    FocusingModule.Focusing_Retry(FocusingParam, false, true, false, this);

                    if (cancellationToken.Token.IsCancellationRequested)
                    {
                        return ThetaAlignEventCodeEnum.REGISTE_PATTERN_USER_CANCEL;
                    }
                }

                ICamera cam = this.VisionManager().GetCam(ptinfo.CamType.Value);

                foreach (var light in ptinfo.LightParams)
                {
                    cam.SetLight(light.Type.Value, light.Value.Value);
                }

                PMResult pmresult = this.VisionManager().PatternMatching(ptinfo, this);

                if (cancellationToken.Token.IsCancellationRequested)
                {
                    return ThetaAlignEventCodeEnum.REGISTE_PATTERN_USER_CANCEL;
                }

                this.VisionManager().StartGrab(ptinfo.CamType.Value, this);

                if (pmresult.RetValue == EventCodeEnum.NONE)
                {
                    WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);
                    retval = ThetaAlignEventCodeEnum.NONE;
                }
                else
                {
                    FocusingModule.Focusing_Retry(FocusingParam, false, true, false, this);

                    pmresult = this.VisionManager().PatternMatching(ptinfo, this);

                    this.VisionManager().StartGrab(ptinfo.CamType.Value, this);

                    if (pmresult.RetValue == EventCodeEnum.NONE)
                    {
                        WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);
                        retval = ThetaAlignEventCodeEnum.NONE;
                    }
                    else
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return ThetaAlignEventCodeEnum.REGISTE_PATTERN_USER_CANCEL;
                        }

                        retval = ThetaAlignEventCodeEnum.PATTERN_NOT_FOUND;
                        LoggerManager.Error($"FindJumpIndex : PATTERN_NOT_FOUND");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.ToString()}.FindJumpIndex() : Error occurred.");
                return ThetaAlignEventCodeEnum.EXCEPTION;
            }

            return retval;
        }

        private EventCodeEnum Processing(WAStandardPTInfomation ptinfo, ObservableCollection<StandardJumpIndexParam> indexparam, ref List<WaferProcResult> procresults, bool doAlign, object assembly = null, bool retry = true, bool revisionwc = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                double zoffset = 0;

                double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
                double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

                double indexwidth = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                double indexheight = Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                double movex = 0.0;
                double movey = 0.0;
                double movez = 0.0;

                PMResult pmresult = null;
                string tmpfilpath = "";

                try
                {
                    tmpfilpath = ptinfo.PMParameter.ModelFilePath.Value;

                    axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().SetSettlingTime(axisX, settling);
                    this.MotionManager().SetSettlingTime(axisY, settling);
                    this.MotionManager().SetSettlingTime(axisZ, settling);

                    ICamera cam = this.VisionManager().GetCam(ptinfo.CamType.Value);

                    bool revisionhor = false;

                    for (int jumpindex = 0; jumpindex < indexparam.Count; jumpindex++)
                    {
                        movex = (ptinfo.GetX() + wafercenterx) + (indexparam[jumpindex].Index.XIndex * indexwidth);
                        movey = (ptinfo.GetY() + wafercentery) + (indexparam[jumpindex].Index.YIndex * indexheight);
                        movez = this.WaferObject.GetSubsInfo().ActualThickness + zoffset;

                        WaferCoordinate wcd = new WaferCoordinate(movex, movey, movez);

                        var idx = this.WaferAligner().WPosToMIndex(wcd);

                        LoggerManager.Debug($"[ThetaAlign] Processing() " +
                            $"Pattern WaferCoordinate X, Y ({ptinfo.GetX() + wafercenterx} ,{ptinfo.GetY() + wafercentery})" +
                            $"jumpindex  X, Y ({indexparam[jumpindex].Index.XIndex}, {indexparam[jumpindex].Index.YIndex})" +
                            $" MachineIndex X : {idx.XIndex} ,MachineIndex Y : {idx.YIndex}, AcceptProcessing : {indexparam[jumpindex].AcceptProcessing.Value}", isInfo: IsInfo);

                        if (indexparam[jumpindex].AcceptProcessing.Value == false)
                        {
                            LoggerManager.Debug($"Index is skipped.");
                            retVal = EventCodeEnum.NONE;
                            continue;
                        }

                        if (ptinfo.CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                        {
                            retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(movex, movey, movez);
                        }
                        else if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                        {
                            retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(movex, movey, movez);
                        }

                        if (retVal != EventCodeEnum.NONE)
                        {
                            return retVal;
                        }

                        if (movex < 0)
                        {
                            if (movex < Wafer.GetSubsInfo().WaferCenter.GetX() - (Wafer.GetPhysInfo().WaferSize_um.Value / 2))
                            {
                                LoggerManager.Debug($"[WAFER_COMPARE_JUMPINDEX_FAILED] MOVEX : {movex} , WaferXEdgePos : {Wafer.GetSubsInfo().WaferCenter.GetX() - (Wafer.GetPhysInfo().WaferSize_um.Value / 2)}", isInfo: IsInfo);
                                return EventCodeEnum.WAFER_COMPARE_JUMPINDEX_FAILED;
                            }
                        }
                        else  //movex >=0
                        {
                            if (movex > Wafer.GetSubsInfo().WaferCenter.GetX() + (Wafer.GetPhysInfo().WaferSize_um.Value / 2))
                            {
                                LoggerManager.Debug($"[WAFER_COMPARE_JUMPINDEX_FAILED] MOVEX : {movex} , WaferXEdgePos : {Wafer.GetSubsInfo().WaferCenter.GetX() - (Wafer.GetPhysInfo().WaferSize_um.Value / 2)}", isInfo: IsInfo);
                                return EventCodeEnum.WAFER_COMPARE_JUMPINDEX_FAILED;
                            }
                        }

                        if (movey < 0)
                        {
                            if (movey < Wafer.GetSubsInfo().WaferCenter.GetY() - (Wafer.GetPhysInfo().WaferSize_um.Value / 2))
                            {
                                LoggerManager.Debug($"[WAFER_COMPARE_JUMPINDEX_FAILED] MOVEY: {movey} , WaferYEdgePos : {Wafer.GetSubsInfo().WaferCenter.GetY() - (Wafer.GetPhysInfo().WaferSize_um.Value / 2)}", isInfo: IsInfo);

                                return EventCodeEnum.WAFER_COMPARE_JUMPINDEX_FAILED;
                            }
                        }
                        else  //movey >=0
                        {
                            if (movey > Wafer.GetSubsInfo().WaferCenter.GetY() + (Wafer.GetPhysInfo().WaferSize_um.Value / 2))
                            {
                                LoggerManager.Debug($"[WAFER_COMPARE_JUMPINDEX_FAILED] MOVEY : {movey} , WaferYEdgePos : {Wafer.GetSubsInfo().WaferCenter.GetY() + (Wafer.GetPhysInfo().WaferSize_um.Value / 2)}", isInfo: IsInfo);

                                return EventCodeEnum.WAFER_COMPARE_JUMPINDEX_FAILED;
                            }
                        }

                        foreach (var light in ptinfo.LightParams)
                        {
                            cam.SetLight(light.Type.Value, light.Value.Value);
                        }

                        if (indexparam[jumpindex].AcceptFocusing.Value != false)
                        {
                            LoggerManager.Debug($"ThetaAling Focusing Start");

                            retVal = FocusingModule.Focusing_Retry(FocusingParam, false, true, false, this);

                            if (cancellationToken != null)
                            {
                                if (cancellationToken.Token.IsCancellationRequested)
                                {
                                    return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                                }
                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                WaferCoordinate wcoord = null;

                                if (ptinfo.CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                                {
                                    wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                                }
                                else if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                                {
                                    wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                                }

                                if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                                {
                                    double distance = 500000;
                                    double edgepos = 0.0;

                                    edgepos = ((distance / 2) / Math.Sqrt(2));

                                    if (indexparam[jumpindex].Index.XIndex == 0 & indexparam[jumpindex].Index.YIndex == 0)
                                    {
                                        if(HeightProfiling != null)
                                        {
                                            HeightProfiling.Clear();
                                        }

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.CENTER, new WaferCoordinate(wcoord.GetX(), wcoord.GetY(), wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.LEFT, new WaferCoordinate(wcoord.GetX() - (distance / 2), wcoord.GetY(), wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.RIGHT, new WaferCoordinate(wcoord.GetX() + (distance / 2), wcoord.GetY(), wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.UPPER, new WaferCoordinate(wcoord.GetX(), wcoord.GetY() + (distance / 2), wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.BOTTOM, new WaferCoordinate(wcoord.GetX(), wcoord.GetY() - (distance / 2), wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.LEFTUPPER, new WaferCoordinate(wcoord.GetX() - edgepos, wcoord.GetY() + edgepos, wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.RIGHTUPPER, new WaferCoordinate(wcoord.GetX() + edgepos, wcoord.GetY() + edgepos, wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.LEFTBOTTOM, new WaferCoordinate(wcoord.GetX() - edgepos, wcoord.GetY() - edgepos, wcoord.GetZ())));

                                        HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.RIGHTBOTTOM, new WaferCoordinate(wcoord.GetX() + edgepos, wcoord.GetY() - edgepos, wcoord.GetZ())));

                                        this.WaferAligner().PlanePointCenter.X.Value = wcoord.GetX();
                                        this.WaferAligner().PlanePointCenter.Y.Value = wcoord.GetY();
                                        this.WaferAligner().PlanePointCenter.Z.Value = wcoord.GetZ();

                                        Wafer.GetSubsInfo().ActualThickness = wcoord.GetZ() - this.WaferAligner().CalcThreePodTiltedPlane(wcoord.GetX(), wcoord.GetY(), true);

                                        foreach (var height in HeightProfiling)
                                        {
                                            this.WaferAligner().AddHeighPlanePoint(new WAHeightPositionParam(height.Item2), true);
                                        }
                                    }
                                    else
                                    {
                                        this.WaferAligner().AddHeighPlanePoint(new WAHeightPositionParam(wcoord));
                                        HeightProfilignPosEnum posenum = FindHeightProfilingPos(indexparam[jumpindex].Index.XIndex, indexparam[jumpindex].Index.YIndex);
                                        if (posenum != HeightProfilignPosEnum.UNDEFINED)
                                        {
                                            var findprofiling = HeightProfiling.Where(x => x.Item1 == posenum).FirstOrDefault();
                                            List<WaferCoordinate> planepoint = this.WaferAligner().GetHieghtPlanePoint();
                                            var findpoint = planepoint.Where(x => x.GetX() == findprofiling.Item2.GetX() && x.GetY() == findprofiling.Item2.GetY()).FirstOrDefault();
                                            if (findpoint != null)
                                            {
                                                LoggerManager.Debug($"JumpIndexs[{jumpindex}] XIndex : {indexparam[jumpindex].Index.XIndex}, YIndex : {indexparam[jumpindex].Index.YIndex}" +
                                                    $", Base PlanePoint {posenum} Pos Z : {findpoint.Z.Value:0.00} Change to Focused PlanePoint Z : {planepoint.Last().Z.Value:0.00}", isInfo: IsInfo);
                                                findpoint.Z.Value = planepoint.Last().Z.Value;
                                            }
                                        }
                                    }

                                    indexparam[jumpindex].HeightProfilingVal = wcoord.GetZ() - this.WaferAligner().CalcThreePodTiltedPlane(wcoord.GetX(), wcoord.GetY(), true);
                                }

                                LoggerManager.Debug($"ThetaAling Focusing End (Succes) : [{wcoord.GetZ()}]");
                            }
                            else
                            {
                                LoggerManager.Error($"ThetaAling Focusing End (Fail) : [{retVal.ToString()}]");
                                return retVal;
                            }
                        }

                        ptinfo.PMParameter.ModelFilePath.Value = this.FileManager().GetDeviceParamFullPath(ptinfo.PMParameter.ModelFilePath.Value);

                        pmresult = this.VisionManager().PatternMatching(ptinfo, this, true, retryautolight: true, retrySuccessedLight: true);

                        // Check PatternMatching Result
                        if (pmresult != null)
                        {
                            retVal = pmresult.RetValue;
                        }

                        if (retVal != EventCodeEnum.NONE)
                        {
                            if (this.WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                            {
                                retVal = EventCodeEnum.NONE;

                                pmresult.ResultParam.Add(new ProberInterfaces.Vision.PMResultParameter());
                                pmresult.ResultParam[0].XPoss = CurCam.GetGrabSizeWidth() / 2;
                                pmresult.ResultParam[0].YPoss = CurCam.GetGrabSizeHeight() / 2;
                            }

                            LoggerManager.Debug($"PatternMaching Fail_1 (jumpindex : [{jumpindex}])");

                            SaveProcessingImage(pmresult.FailOriginImageBuffer, false, indexparam[jumpindex].Index.XIndex, indexparam[jumpindex].Index.YIndex);
                            SaveProcessingImage(pmresult.FailPatternImageBuffer, false, indexparam[jumpindex].Index.XIndex, indexparam[jumpindex].Index.YIndex, true);
                        }

                        if (cancellationToken != null)
                        {
                            if (cancellationToken.Token.IsCancellationRequested)
                            {
                                return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                            }
                        }

                        if (retVal != EventCodeEnum.NONE)
                        {
                            // TODO: Check
                            FocusingParam.FocusingCam.Value = ptinfo.CamType.Value;
                            LoggerManager.Debug("PatternMachingFail Focusing");
                            retVal = FocusingModule.Focusing_Retry(FocusingParam, false, true, false, this);

                            if (cancellationToken != null)
                            {
                                if (cancellationToken.Token.IsCancellationRequested)
                                {
                                    return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                                }
                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                pmresult = this.VisionManager().PatternMatching(ptinfo, this, true, retryautolight: true, retrySuccessedLight: true);
                                retVal = pmresult.RetValue;

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    if(ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                                    {
                                        retVal = RetryFocusingAndPMWithPatternSize(pmresult, ptinfo);
                                    }
                                }
                            }
                            else
                            {
                                retry = true;
                            }
                        }

                        WAStandardPTInfomation tempptinfo = null;

                        if (retVal != EventCodeEnum.NONE && retry)
                        {
                            if (doAlign)
                            {
                                SaveProcessingImage(pmresult.FailOriginImageBuffer, false, indexparam[jumpindex].Index.XIndex, indexparam[jumpindex].Index.YIndex);
                                SaveProcessingImage(pmresult.FailPatternImageBuffer, false, indexparam[jumpindex].Index.XIndex, indexparam[jumpindex].Index.YIndex, true);

                                tempptinfo = ptinfo;

                                this.VisionManager().StartGrab(ptinfo.CamType.Value, this);

                                retVal = RetryJumpIndex(ptinfo, indexparam[jumpindex], ref pmresult);
                                retVal = pmresult.RetValue;

                                if (cancellationToken != null)
                                {
                                    if (cancellationToken.Token.IsCancellationRequested)
                                    {
                                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                                    }
                                }
                            }
                        }

                        if (!this.VisionManager().ConfirmDigitizerEmulMode(ptinfo.CamType.Value))
                        {
                            this.VisionManager().StartGrab(ptinfo.CamType.Value, this);
                        }

                        #region  //..Test 일부러 실패하게 할때 
                        if (ptinfo.CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                        {
                            if (this.WaferAligner().WaferAlignControItems.LowFailPos != EnumLowStandardPosition.UNDIFIND)
                            {
                                if (jumpindex == (int)this.WaferAligner().WaferAlignControItems.LowFailPos)
                                {
                                    retVal = EventCodeEnum.WA_CONTROL_FAIL;
                                }
                            }
                        }
                        else if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                        {
                            if (this.WaferAligner().WaferAlignControItems.HighFailPos != EnumHighStandardPosition.UNDIFIND)
                            {
                                if (_IsPostAlign)
                                {
                                    if (jumpindex == (int)this.WaferAligner().WaferAlignControItems.HighFailPos)
                                    {
                                        retVal = EventCodeEnum.WA_CONTROL_FAIL;
                                    }
                                }
                                else
                                {
                                    if (jumpindex + ptinfo.PostJumpIndex.Count == (int)this.WaferAligner().WaferAlignControItems.HighFailPos)
                                    {
                                        retVal = EventCodeEnum.WA_CONTROL_FAIL;
                                    }
                                }
                            }
                        }
                        #endregion

                        if (retVal == EventCodeEnum.NONE)
                        {
                            WaferCoordinate wcoord = ChangedLocationFormPT(pmresult);
                            double t = 0.0;
                            this.MotionManager().GetRefPos(EnumAxisConstants.C, ref t);
                            wcoord.T.Value = t;

                            if (indexparam[jumpindex].RetryIndex == null)
                            {
                                procresults.Add(new WaferProcResult(wcoord, indexparam[jumpindex].Index, ptinfo, new RectSize(Wafer.GetSubsInfo().ActualDieSize.Width.Value, Wafer.GetSubsInfo().ActualDieSize.Height.Value), pmresult));
                            }
                            else
                            {
                                procresults.Add(new WaferProcResult(wcoord, indexparam[jumpindex].RetryIndex, ptinfo, new RectSize(Wafer.GetSubsInfo().ActualDieSize.Width.Value, Wafer.GetSubsInfo().ActualDieSize.Height.Value), pmresult));
                                indexparam[jumpindex].RetryIndex = null;
                            }

                            LoggerManager.Debug($"ThetaAlignRelPos => X:{wcoord.GetX()} , Y:{wcoord.GetY()} , Z:{wcoord.GetZ()}");

                            if (jumpindex == 0)
                            {
                                zoffset = wcoord.GetZ() - movez;
                            }

                            if (tempptinfo != null)
                            {
                                ptinfo = tempptinfo;
                            }

                            try
                            {
                                if (this.WaferAligner().IsOnDubugMode)
                                {
                                    string pathbase = this.WaferAligner().IsOnDebugImagePathBase;
                                    var resultImageBuf = pmresult.ResultBuffer.Buffer;
                                    if (resultImageBuf != null)
                                    {
                                        double posX = ptinfo.GetX() + wafercenterx;
                                        double posY = ptinfo.GetY() + wafercentery;

                                        LoggerManager.Debug($"[Wafer Align][PM] {ptinfo.CamType}" +
                                            $"\nWC X : {wafercenterx}, Y : {wafercentery}" +
                                            $"\nPTinfo X : {ptinfo.GetX()}, Y : {ptinfo.GetY()}" +
                                            $"\nProcess X : {posX}, Y : {posY}", isInfo: IsInfo);


                                        SaveProcessingImage(pmresult.ResultBuffer, true, indexparam[jumpindex].Index.XIndex, indexparam[jumpindex].Index.YIndex);
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"PatternMaching Fail_2 (jumpindex : [{jumpindex}])");
                            ptinfo.PatternState.Value = PatternStateEnum.FAILED;
                            retVal = EventCodeEnum.SUB_RECOVERY;
                            break;
                        }
                        //skip 함수는 지정한 인덱스까지 건너뛰고. 나머지 요소를 검사하는 함수.
                        //jumpindex = 0 인 경우, 1부터 ~ 끝까지 확인하는
                        // jumpIndex보다 큰 값들에 대해 AcceptProcessing이 모두 false인지 확인
                        //마지막 인덱스에 대해서 빈 컬렉션을 반환하고 ALL 메서드가 빈 컬렉션에 대해서 호출되면 true 반환 함.
                        bool lastindex = indexparam.Skip(jumpindex + 1).All(param => param.AcceptProcessing.Value == false);
                        //if (jumpindex == indexparam.Count - 1)
                        if (lastindex == true)
                        {
                            if (ptinfo.ProcDirection.Value == EnumWAProcDirection.HORIZONTAL ||
                                ptinfo.ProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                if (!revisionhor)
                                {
                                    double rotateangle = 0.0;
                                    retVal = RevisionHorizontal(ptinfo, procresults, ref rotateangle, revisionwc);
                                    revisionhor = true;
                                }
                            }
                        }
                        else if (indexparam[jumpindex + 1].Index.YIndex != 0)
                        {
                            if (ptinfo.ProcDirection.Value == EnumWAProcDirection.HORIZONTAL ||
                                ptinfo.ProcDirection.Value == EnumWAProcDirection.BIDIRECTIONAL)
                            {
                                if (!revisionhor)
                                {
                                    double rotateangle = 0.0;
                                    retVal = RevisionHorizontal(ptinfo, procresults, ref rotateangle, revisionwc);
                                    revisionhor = true;
                                }
                            }
                        }
                    }

                    //Procsssing 성공했으면 READY 상태로 변경.
                    if (retVal == EventCodeEnum.NONE)
                    {
                        if (ptinfo.PatternState.Value == PatternStateEnum.FAILED)
                        {
                            ptinfo.PatternState.Value = PatternStateEnum.READY;
                        }
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Error($"{err.ToString()} ThetaAlignStadnard - Processing () : Error occurred.");
                    throw err;
                }
                finally
                {
                    if (tmpfilpath != "")
                    {
                        //excetpion 발생하더라도 ModelFilePath 돌려놓기 위한 코드
                        ptinfo.PMParameter.ModelFilePath.Value = tmpfilpath;
                    }
                    ptinfo.ErrorCode = retVal;
                    this.MotionManager().SetSettlingTime(axisX, 0.00000001);
                    this.MotionManager().SetSettlingTime(axisY, 0.00000001);
                    this.MotionManager().SetSettlingTime(axisZ, 0.00000001);
                }
                return retVal;
            }
            catch (Exception err)
            {
                if (this.WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
            return retVal;
        }

        /// <summary>
        /// 패턴 사이즈 크기 + Margin 의 영역으로 포커싱 및 패턴매칭 재시도
        /// </summary>
        /// <param name="pmresult"></param>
        /// <param name="ptinfo"></param>
        /// <returns></returns>

        private EventCodeEnum RetryFocusingAndPMWithPatternSize(PMResult pmresult, WAStandardPTInfomation ptinfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            
            try
            {
                ICamera cam = this.VisionManager().GetCam(ptinfo.CamType.Value);

                double margin_pixel_X = _RetryFocusingROIMargin_X / cam.GetRatioX();
                double margin_pixel_Y = _RetryFocusingROIMargin_Y / cam.GetRatioY();

                double width = ptinfo.PMParameter.PattWidth.Value + (margin_pixel_X * 2);
                double height = ptinfo.PMParameter.PattHeight.Value + (margin_pixel_Y * 2);

                if (width > cam.GetGrabSizeWidth() || height > cam.GetGrabSizeHeight())
                {
                    LoggerManager.Debug($"RetryFocusingAndPMWithPatternSize() ROI width:{width}, height:{height} is greater than 960");
                    return retVal;
                }

                double x = (cam.GetGrabSizeWidth() / 2) - (ptinfo.PMParameter.PattWidth.Value / 2) - margin_pixel_X;
                double y = (cam.GetGrabSizeHeight() / 2) - (ptinfo.PMParameter.PattHeight.Value / 2) - margin_pixel_Y;
                LoggerManager.Debug($"RetryFocusingAndPMWithPatternSize() Start : retryFocusingROIMargin_X:{_RetryFocusingROIMargin_X}, retryFocusingROIMargin_Y:{_RetryFocusingROIMargin_Y}");

                FocusParameter copyFocusingParam = new NormalFocusParameter();
                (FocusingParam as FocusParameter).CopyTo(copyFocusingParam);

                // 해당 패턴 위치에서 포커싱 ROI를 설정하여 포커싱 재시도
                copyFocusingParam.FocusingROI.Value = new System.Windows.Rect(x, y, width, height);

                retVal = FocusingModule.Focusing_Retry(copyFocusingParam, false, true, false, this);

                if (retVal == EventCodeEnum.NONE)
                {
                    pmresult = this.VisionManager().PatternMatching(ptinfo, this, true, retryautolight: true, retrySuccessedLight: true, offsetx: (int)x, offsety: (int)y, sizex: (int)width, sizey: (int)height);

                    retVal = pmresult.RetValue;
                }
                else
                {
                    LoggerManager.Debug($"RetryFocusingAndPMWithPatternSize() Focusing_Retry Fail {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private HeightProfilignPosEnum FindHeightProfilingPos(long xindex, long yindex)
        {
            HeightProfilignPosEnum posenum = HeightProfilignPosEnum.UNDEFINED;

            try
            {
                if (xindex == 0)
                {
                    posenum = (yindex > 0) ? HeightProfilignPosEnum.UPPER : HeightProfilignPosEnum.BOTTOM;
                }
                else if (yindex == 0)
                {
                    posenum = (xindex > 0) ? HeightProfilignPosEnum.RIGHT : HeightProfilignPosEnum.LEFT;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return posenum;

        }
        private bool PatternIndexSort(List<WAStandardPTInfomation> ptinfos)
        {
            bool retVal = false;

            try
            {
                int failcount = ptinfos.FindAll(info => info.PatternState.Value == PatternStateEnum.FAILED).Count;

                if (failcount != 0)
                {
                    if (failcount == ptinfos.Count)
                    {
                        retVal = false;
                    }
                    else
                    {
                        for (int findex = 0; findex < failcount; findex++)
                        {
                            int failindex = ptinfos.FindIndex(info => info.PatternState.Value == PatternStateEnum.FAILED);

                            WAStandardPTInfomation tmp = new WAStandardPTInfomation();
                            ptinfos[failindex].CopyTo(tmp);

                            for (int index = failindex; index < ptinfos.Count - 1; index++)
                            {
                                ptinfos[index] = ptinfos[index + 1];
                            }

                            ptinfos[ptinfos.Count - 1] = tmp;
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

        private EventCodeEnum RetryJumpIndex(WAStandardPTInfomation ptinfo, StandardJumpIndexParam jumpparam, ref PMResult pmresult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (jumpparam.Index.XIndex == 0 && jumpparam.Index.YIndex == 0)
                {
                    return EventCodeEnum.VISION_PM_NOT_FOUND;
                }

                double mininum = 0.0;
                double optinum = 0.0;
                double maxinum = 0.0;

                if (_IsPostAlign)
                {
                    switch (this.GetParam_Wafer().GetPhysInfo().WaferSizeEnum)
                    {
                        case EnumWaferSize.INCH6:
                            mininum = _PostMiniumumLength * (1.0 / 2.0);
                            optinum = _PostOptimumLength * (1.0 / 2.0);
                            maxinum = _PostMaximumLength * (1.0 / 2.0);
                            break;
                        case EnumWaferSize.INCH8:
                            mininum = _PostMiniumumLength * (2.0 / 3.0);
                            optinum = _PostOptimumLength * (2.0 / 3.0);
                            maxinum = _PostMaximumLength * (2.0 / 3.0);
                            break;
                        case EnumWaferSize.INCH12:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (this.GetParam_Wafer().GetPhysInfo().WaferSizeEnum)
                    {
                        case EnumWaferSize.INCH6:
                            mininum = _MinimumLength * (1.0 / 2.0);
                            optinum = _OptimumLength * (1.0 / 2.0);
                            maxinum = _MaximumLength * (1.0 / 2.0);
                            break;
                        case EnumWaferSize.INCH8:
                            mininum = _MinimumLength * (2.0 / 3.0);
                            optinum = _OptimumLength * (2.0 / 3.0);
                            maxinum = _MaximumLength * (2.0 / 3.0);
                            break;
                        case EnumWaferSize.INCH12:
                            break;
                        default:
                            break;
                    }
                }

                IWaferObject waferobj = this.GetParam_Wafer();

                double movex = ptinfo.GetX() + waferobj.GetSubsInfo().WaferCenter.GetX();
                double movey = ptinfo.GetY() + waferobj.GetSubsInfo().WaferCenter.GetY();

                ICamera cam = this.VisionManager().GetCam(ptinfo.CamType.Value);

                foreach (var light in ptinfo.LightParams)
                {
                    cam.SetLight(light.Type.Value, light.Value.Value);
                }

                if (jumpparam.Index.XIndex != 0)
                {
                    retVal = RetryHorJumpIndex(ptinfo, jumpparam, maxinum, mininum, movex, movey, jumpparam.Index.XIndex, ref pmresult);
                }
                else //jumpparam.Index.YIndex != 0
                {
                    retVal = RetryVertJumpIndex(ptinfo, jumpparam, maxinum, mininum, movex, movey, jumpparam.Index.YIndex, ref pmresult);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void MoveStage(EnumProberCam camtype, double xpos, double ypos, double zpos)
        {
            try
            {
                if (camtype == EnumProberCam.WAFER_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(xpos, ypos, zpos);
                }
                else if (camtype == EnumProberCam.WAFER_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(xpos, ypos, zpos);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum ExecutePatternMatchingAndFocus(EnumDirection direction, PatternInfomation ptinfo, StandardJumpIndexParam jumpparam, int jumpindex, ref PMResult pmresult, bool isRetryAttempt)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                pmresult = this.VisionManager().PatternMatching(ptinfo, this);

                this.VisionManager().StartGrab(ptinfo.CamType.Value, this);

                retval = pmresult.RetValue;

                if (retval != EventCodeEnum.NONE)
                {
                    if (direction == EnumDirection.VER)
                    {
                        SaveProcessingImage(pmresult.FailOriginImageBuffer, false, jumpparam.Index.XIndex, jumpindex);
                        SaveProcessingImage(pmresult.FailPatternImageBuffer, false, jumpparam.Index.XIndex, jumpindex, true);
                    }
                    else if (direction == EnumDirection.HOR)
                    {
                        SaveProcessingImage(pmresult.FailOriginImageBuffer, false, jumpindex, jumpparam.Index.YIndex);
                        SaveProcessingImage(pmresult.FailPatternImageBuffer, false, jumpindex, jumpparam.Index.YIndex, true);
                    }

                    if (!isRetryAttempt)
                    {
                        return retval;
                    }

                    retval = FocusingModule.Focusing_Retry(FocusingParam, false, true, false, this);

                    if (cancellationToken != null && cancellationToken.Token.IsCancellationRequested)
                    {
                        return EventCodeEnum.THETA_ALIGN_USER_CANCEL;
                    }

                    if (retval == EventCodeEnum.NONE) // 포커싱 성공
                    {
                        if (isRetryAttempt)
                        {
                            return ExecutePatternMatchingAndFocus(direction, ptinfo, jumpparam, jumpindex, ref pmresult, false);
                        }
                    }
                    else // 포커싱 실패
                    {
                        return retval;
                    }
                }
                else
                {
                    if (direction == EnumDirection.VER)
                    {
                        jumpparam.RetryIndex = new MachineIndex(jumpparam.Index.XIndex, jumpindex);
                        SaveProcessingImage(pmresult.ResultBuffer, true, jumpparam.Index.XIndex, jumpindex);
                    }
                    else if (direction == EnumDirection.HOR)
                    {
                        jumpparam.RetryIndex = new MachineIndex(jumpindex, jumpparam.Index.YIndex);
                        SaveProcessingImage(pmresult.ResultBuffer, true, jumpindex, jumpparam.Index.YIndex);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum RetryHorJumpIndex(WAStandardPTInfomation ptinfo, StandardJumpIndexParam jumpparam, double maxinum, double mininum, double movex, double movey, long oriindex, ref PMResult pmresult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IWaferObject waferobj = this.GetParam_Wafer();
                ISubstrateInfo substrateInfo = waferobj.GetSubsInfo();
                IPhysicalInfo physicalInfo = waferobj.GetPhysInfo();

                double ActualDieSizeWidth = substrateInfo.ActualDieSize.Width.Value;
                double WaferCenterX = Wafer.GetSubsInfo().WaferCenter.GetX();
                double WaferSize_um = physicalInfo.WaferSize_um.Value;

                mininum = Math.Ceiling(mininum / ActualDieSizeWidth) * ActualDieSizeWidth;
                maxinum = Math.Ceiling(maxinum / ActualDieSizeWidth) * ActualDieSizeWidth;

                int minindex = Calc(1, ActualDieSizeWidth, -((mininum / 2) + Math.Abs(movex)), movex);
                int maxindex = Calc(1, ActualDieSizeWidth, -((maxinum / 2) + Math.Abs(movex)), movex);

                int jumpindex;
                bool limitflag;

                for (int index = Math.Abs(maxindex); Math.Abs(index) > Math.Abs(oriindex); index--)
                {
                    jumpindex = index;
                    limitflag = false;

                    if (oriindex < 0)
                    {
                        jumpindex *= -1;

                        if (movex + (jumpindex * ActualDieSizeWidth) > WaferCenterX - (WaferSize_um / 2))
                        {
                            limitflag = true;
                        }
                        else
                        {
                            limitflag = false;
                        }
                    }
                    else
                    {
                        if (movex + (jumpindex * ActualDieSizeWidth) < WaferCenterX + (WaferSize_um / 2))
                        {
                            limitflag = true;
                        }
                        else
                        {
                            limitflag = false;
                        }
                    }

                    if (limitflag)
                    {
                        MoveStage(ptinfo.CamType.Value, movex + (jumpindex * ActualDieSizeWidth), movey, substrateInfo.ActualThickness);

                        retVal = ExecutePatternMatchingAndFocus(EnumDirection.HOR, ptinfo, jumpparam, jumpindex, ref pmresult, true);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            break;
                        }

                        if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                        {
                            return retVal;
                        }
                    }
                }

                if (retVal != EventCodeEnum.NONE)
                {
                    for (int index = Math.Abs(minindex); Math.Abs(index) < Math.Abs(oriindex); index++)
                    {
                        jumpindex = index;
                        limitflag = false;

                        if (oriindex < 0)
                        {
                            jumpindex *= -1;

                            if (movex + (jumpindex * ActualDieSizeWidth) > WaferCenterX - (WaferSize_um / 2))
                            {
                                limitflag = true;
                            }
                            else
                            {
                                limitflag = false;
                            }
                        }
                        else
                        {
                            if (movex + (jumpindex * ActualDieSizeWidth) < WaferCenterX + (WaferSize_um / 2))
                            {
                                limitflag = true;
                            }
                            else
                            {
                                limitflag = false;
                            }
                        }

                        if (limitflag)
                        {
                            MoveStage(ptinfo.CamType.Value, movex + (jumpindex * ActualDieSizeWidth), movey, substrateInfo.ActualThickness);

                            retVal = ExecutePatternMatchingAndFocus(EnumDirection.HOR, ptinfo, jumpparam, jumpindex, ref pmresult, true);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                break;
                            }

                            if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                            {
                                return retVal;
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
        private EventCodeEnum RetryVertJumpIndex(WAStandardPTInfomation ptinfo, StandardJumpIndexParam jumpparam, double maxinum, double mininum, double movex, double movey, long oriindex, ref PMResult pmresult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IWaferObject waferobj = this.GetParam_Wafer();
                ISubstrateInfo substrateInfo = waferobj.GetSubsInfo();
                IPhysicalInfo physicalInfo = waferobj.GetPhysInfo();

                double ActualDieSizeHeight = substrateInfo.ActualDieSize.Height.Value;
                double WaferCenterY = Wafer.GetSubsInfo().WaferCenter.GetY();
                double WaferSize_um = physicalInfo.WaferSize_um.Value;

                mininum = Math.Ceiling(mininum / ActualDieSizeHeight) * ActualDieSizeHeight;
                maxinum = Math.Ceiling(maxinum / ActualDieSizeHeight) * ActualDieSizeHeight;

                int minindex = Calc(1, ActualDieSizeHeight, -((mininum / 2) + Math.Abs(movey)), movey);
                int maxindex = Calc(1, ActualDieSizeHeight, -((maxinum / 2) + Math.Abs(movey)), movey);

                int jumpindex;
                bool limitflag;

                for (int index = Math.Abs(maxindex); Math.Abs(index) > Math.Abs(oriindex); index--)
                {
                    jumpindex = index;
                    limitflag = false;

                    if (oriindex < 0)
                    {
                        jumpindex *= -1;

                        if (movey + (jumpindex * ActualDieSizeHeight) > WaferCenterY - (WaferSize_um / 2))
                        {
                            limitflag = true;
                        }
                        else
                        {
                            limitflag = false;
                        }
                    }
                    else
                    {
                        if (movey + (jumpindex * ActualDieSizeHeight) < WaferCenterY + (WaferSize_um / 2))
                        {
                            limitflag = true;
                        }
                        else
                        {
                            limitflag = false;
                        }
                    }

                    if (limitflag)
                    {
                        MoveStage(ptinfo.CamType.Value, movex, movey + (jumpindex * ActualDieSizeHeight), substrateInfo.ActualThickness);

                        retVal = ExecutePatternMatchingAndFocus(EnumDirection.VER, ptinfo, jumpparam, jumpindex, ref pmresult, true);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            break;
                        }

                        if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                        {
                            return retVal;
                        }
                    }
                }

                if (retVal != EventCodeEnum.NONE)
                {
                    for (int index = Math.Abs(minindex); Math.Abs(index) < Math.Abs(oriindex); index++)
                    {
                        jumpindex = index;
                        limitflag = false;

                        if (oriindex < 0)
                        {
                            jumpindex *= -1;

                            if (movey + (jumpindex * ActualDieSizeHeight) > WaferCenterY - (WaferSize_um / 2))
                            {
                                limitflag = true;
                            }
                            else
                            {
                                limitflag = false;
                            }
                        }
                        else
                        {
                            if (movey + (jumpindex * ActualDieSizeHeight) < WaferCenterY + (WaferSize_um / 2))
                            {
                                limitflag = true;
                            }
                            else
                            {
                                limitflag = false;
                            }
                        }

                        if (limitflag)
                        {
                            MoveStage(ptinfo.CamType.Value, movex, movey + (jumpindex * ActualDieSizeHeight), substrateInfo.ActualThickness);

                            retVal = ExecutePatternMatchingAndFocus(EnumDirection.VER, ptinfo, jumpparam, jumpindex, ref pmresult, true);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                break;
                            }

                            if (retVal == EventCodeEnum.THETA_ALIGN_USER_CANCEL)
                            {
                                return retVal;
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

        private void SaveProcessingImage(ImageBuffer imageBuffer, bool isSucess, long xIndex, long yIndex, bool isPatternImage = false)
        {
            try
            {
                string waferId = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;

                if (waferId == "")
                {
                    waferId = "Default";
                }

                string path = string.Empty;

                IMAGE_LOG_TYPE logtype = IMAGE_LOG_TYPE.NORMAL;

                if (isSucess)
                {
                    logtype = IMAGE_LOG_TYPE.PASS;

                    path = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\", $"{waferId}\\Sucess\\[{imageBuffer.CamType}]_X#{xIndex}_Y#{yIndex}");
                }
                else
                {
                    logtype = IMAGE_LOG_TYPE.FAIL;

                    if (isPatternImage)
                    {
                        path = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\", $"{waferId}\\Fail\\[{imageBuffer.CamType}]_X#{xIndex}_Y#{yIndex}_Pattern");
                    }
                    else
                    {
                        path = this.FileManager().GetImageSaveFullPath(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\", $"{waferId}\\Fail\\[{imageBuffer.CamType}]_X#{xIndex}_Y#{yIndex}");
                    }
                }

                this.VisionManager().SaveImageBuffer(imageBuffer, path, logtype, EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum RevisionHorizontal(WAStandardPTInfomation ptinfo, List<WaferProcResult> procresults, ref double rotateangle, bool revisionwc = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (procresults != null && procresults.Count > 0)
                {
                    List<WaferProcResult> tmpresults = new List<WaferProcResult>();

                    foreach (var result in procresults)
                    {
                        if (result.Index.YIndex == 0)
                        {
                            tmpresults.Add(result);
                        }
                    }

                    for (int index = 0; index < procresults.Count; index++)
                    {
                        LoggerManager.Debug($"WaferAlignProcResult [{index }] => XPixel : {procresults[index].PmResult.ResultParam[0].XPoss} , YPixel : {procresults[index].PmResult.ResultParam[0].YPoss} , Angle : {procresults[index].PmResult.ResultParam[0].Angle}", isInfo: false);
                        LoggerManager.Debug($"WaferAlignProcResult [{index }] => ResultXPos : {procresults[index].ResultPos.GetX()} , ResultYPos : {procresults[index].ResultPos.GetY()}", isInfo: false);
                    }

                    tmpresults.Sort();

                    for (int index = 0; index < tmpresults.Count; index++)
                    {
                        LoggerManager.Debug($"WaferAlignHorSortResult [{index }] => ResultXPos : {tmpresults[index].ResultPos.GetX()} , ResultYPos : {tmpresults[index].ResultPos.GetY()}", isInfo: IsInfo);
                    }

                    rotateangle = 0.0;

                    double angle = this.CoordinateManager().CalcP2PAngle(tmpresults[tmpresults.Count() - 1].ResultPos.GetX(), tmpresults[tmpresults.Count() - 1].ResultPos.GetY(), tmpresults[0].ResultPos.GetX(), tmpresults[0].ResultPos.GetY());

                    angle = Math.Round(angle, 6);

                    if (angle <= -1 && angle >= -89)
                    {

                    }
                    else if (angle <= -90 & angle > -180)
                    {
                        rotateangle = (180 + (angle)) * -1;
                    }
                    else if (angle <= 1 && angle >= 90)
                    {

                    }
                    else if (angle >= 90 && angle < 180)
                    {
                        rotateangle = 180 + (-angle);
                    }

                    rotateangle = Math.Round(rotateangle, 6);

                    var axisC = this.MotionManager().GetAxis(EnumAxisConstants.C);

                    LoggerManager.Debug($"RevisionAngle : {rotateangle}°.", isInfo: IsInfo);

                    double curTheta = 0.0;
                    this.MotionManager().GetRefPos(EnumAxisConstants.C, ref curTheta);
                    double prevTheta = curTheta;

                    this.StageSupervisor().StageModuleState.StageRelMove(axisC, (rotateangle * 10000d));

                    this.MotionManager().GetRefPos(EnumAxisConstants.C, ref curTheta);
                    this.WaferAligner().WaferAlignInfo.AlignAngle = curTheta / 10000d;

                    LoggerManager.Debug($"WaferAlign Angle : {this.WaferAligner().WaferAlignInfo.AlignAngle}°.", isInfo: IsInfo);
                    LoggerManager.Debug($"WaferAlign Theta beforeMove : {prevTheta}, afterMove : {curTheta}, offset : {curTheta - prevTheta}", isInfo: IsInfo);

                    long moveindex = Math.Abs(tmpresults[0].Index.XIndex) + Math.Abs(tmpresults[tmpresults.Count() - 1].Index.XIndex);

                    double movelength = moveindex * Wafer.GetSubsInfo().ActualDieSize.Width.Value;

                    MachineCoordinate pivotPoint = new MachineCoordinate(0, 0);

                    MachineCoordinate MinPoint = new MachineCoordinate(tmpresults[0].ResultPos.GetX(), tmpresults[0].ResultPos.GetY());
                    MachineCoordinate MaxPoint = new MachineCoordinate(tmpresults[tmpresults.Count() - 1].ResultPos.GetX(), tmpresults[tmpresults.Count() - 1].ResultPos.GetY());

                    MachineCoordinate minretcoord = this.CoordinateManager().GetRotatedPoint(MinPoint, pivotPoint, rotateangle);
                    MachineCoordinate maxretcoord = this.CoordinateManager().GetRotatedPoint(MaxPoint, pivotPoint, rotateangle);

                    double proclength = maxretcoord.GetX() - minretcoord.GetX();

                    if (proclength != 0)
                    {
                        double distance2d = Math.Sqrt(Math.Pow((tmpresults[tmpresults.Count - 1].ResultPos.GetX() - tmpresults[0].ResultPos.GetX()), 2) + Math.Pow((tmpresults[tmpresults.Count - 1].ResultPos.GetY() - tmpresults[0].ResultPos.GetY()), 2));

                        double tscoffsetx = Math.Abs(distance2d / moveindex) - Wafer.GetSubsInfo().ActualDieSize.Width.Value;

                        tscoffsetx = Math.Round(tscoffsetx, 6);

                        Wafer.GetSubsInfo().ActualDieSize.Width.Value += tscoffsetx;

                        LoggerManager.Debug($"RevisionIndexWidth: {Wafer.GetSubsInfo().ActualDieSize.Width.Value}(um).", isInfo: IsInfo);
                        LoggerManager.Debug($"IndexWidthOffset(TSCOffsetX): {tscoffsetx}(um).", isInfo: IsInfo);

                        Wafer.GetSubsInfo().ActualDeviceSize.Width.Value = Wafer.GetSubsInfo().ActualDieSize.Width.Value - Wafer.GetSubsInfo().DieXClearance.Value;

                        double waferwidth = Wafer.GetSubsInfo().ActualDieSize.Width.Value;

                        foreach (var result in tmpresults)
                        {
                            if (result.Index.XIndex == 0 && result.PmResult.RetValue == EventCodeEnum.NONE)
                            {
                                MachineCoordinate resultcoord = new MachineCoordinate(result.ResultPos.GetX(), result.ResultPos.GetY());
                                MachineCoordinate coord = new MachineCoordinate(result.PatternInfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), result.PatternInfo.GetY() + Wafer.GetSubsInfo().WaferCenter.GetY());

                                resultcoord = this.CoordinateManager().GetRotatedPoint(resultcoord, pivotPoint, rotateangle);
                                //brett// wafer align fail 및 PTMA의 원인이 될 수 있는 코드로 삭제함(wafer Center와 PadCenter에 영향을 줄 수 있음.)
                                //coord = this.CoordinateManager().GetRotatedPoint(coord, pivotPoint, rotateangle);

                                double wcoffsetX = resultcoord.GetX() - coord.GetX();
                                double wcoffsetY = resultcoord.GetY() - coord.GetY();

                                if (revisionwc)
                                {
                                    Wafer.GetSubsInfo().WaferCenter.X.Value += wcoffsetX;
                                    Wafer.GetSubsInfo().WaferCenter.Y.Value += wcoffsetY;

                                    Wafer.GetSubsInfo().RefDieLeftCorner.X.Value = this.WaferAligner().GetLeftCornerPosition(Wafer.GetSubsInfo().WaferCenter.X.Value, Wafer.GetSubsInfo().WaferCenter.Y.Value).X;
                                    Wafer.GetSubsInfo().RefDieLeftCorner.Y.Value = this.WaferAligner().GetLeftCornerPosition(Wafer.GetSubsInfo().WaferCenter.X.Value, Wafer.GetSubsInfo().WaferCenter.Y.Value).Y;

                                    LoggerManager.Debug($"WaferCenter X : {Wafer.GetSubsInfo().WaferCenter.X.Value}. WaferCetner Y : {Wafer.GetSubsInfo().WaferCenter.Y.Value}", isInfo: IsInfo);
                                    LoggerManager.Debug($"CenterOffsetX : {wcoffsetX}. CenterOffsetY : {wcoffsetY}", isInfo: IsInfo);
                                }

                                ptinfo.ProcWaferCenter = new WaferCoordinate();
                                Wafer.GetSubsInfo().WaferCenter.CopyTo(ptinfo.ProcWaferCenter);

                                break;
                            }
                        }
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                this.NotifyManager().Notify(EventCodeEnum.WAFER_ALIGN_THETA_COMPENSATION_FAIL);
                LoggerManager.Error(err.ToString() + "RevisionHorizontal() : Error occurred.");
            }

            return retVal;
        }

        private EventCodeEnum RevisionVertical(WAStandardPTInfomation ptinfo, List<WaferProcResult> procresults, double rotateangle)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                try
                {
                    List<WaferProcResult> tmpresults = new List<WaferProcResult>();

                    foreach (var result in procresults)
                    {
                        if (result.Index.XIndex == 0)
                        {
                            tmpresults.Add(result);
                        }
                    }

                    for (int index = 0; index < procresults.Count; index++)
                    {
                        LoggerManager.Debug($"WaferAlignProcResult [{index }] => XPixel : {procresults[index].PmResult.ResultParam[0].XPoss} , YPixel : {procresults[index].PmResult.ResultParam[0].YPoss} , Angle : {procresults[index].PmResult.ResultParam[0].Angle}", isInfo: false);
                        LoggerManager.Debug($"WaferAlignProcResult [{index }] => ResultXPos : {procresults[index].ResultPos.GetX()} , ResultYPos : {procresults[index].ResultPos.GetY()}", isInfo: false);
                    }

                    if (tmpresults != null && tmpresults.Count > 0)
                    {
                        tmpresults.Sort(delegate (WaferProcResult A, WaferProcResult B)
                        {
                            if (A.ResultPos.Y.Value > B.ResultPos.Y.Value)
                            {
                                return 1;
                            }
                            else if (A.ResultPos.Y.Value < B.ResultPos.Y.Value)
                            {
                                return -1;
                            }
                            return 0;
                        });

                        for (int index = 0; index < tmpresults.Count; index++)
                        {
                            LoggerManager.Debug($"WaferAlignHorSortResult [{index }] => ResultXPos : {tmpresults[index].ResultPos.GetX()} , ResultYPos : {tmpresults[index].ResultPos.GetY()}", isInfo: IsInfo);
                        }

                        long moveindex = Math.Abs(tmpresults[0].Index.YIndex) + Math.Abs(tmpresults[tmpresults.Count() - 1].Index.YIndex);

                        //VB
                        double squarness = -(tmpresults[tmpresults.Count() - 1].ResultPos.X.Value - tmpresults[0].ResultPos.X.Value) / moveindex;    //음수나옴
                        squarness = -(tmpresults[0].ResultPos.X.Value - tmpresults[tmpresults.Count() - 1].ResultPos.X.Value) / moveindex; //VB와 동일 부호나옴

                        if (Wafer.GetSubsInfo().WaferSequareness == null)
                        {
                            Wafer.GetSubsInfo().WaferSequareness = new Element<double>();
                        }

                        squarness = Math.Round(squarness, 6);
                        Wafer.GetSubsInfo().WaferSequareness.Value = squarness;

                        LoggerManager.Debug($"WaferSquareness: {Wafer.GetSubsInfo().WaferSequareness.Value}(°).", isInfo: IsInfo);


                        double distance2d = Math.Sqrt(Math.Pow((tmpresults[0].ResultPos.GetX() - tmpresults[tmpresults.Count - 1].ResultPos.GetX()), 2) + Math.Pow((tmpresults[0].ResultPos.GetY() - tmpresults[tmpresults.Count - 1].ResultPos.GetY()), 2));
                        distance2d = tmpresults[tmpresults.Count - 1].ResultPos.GetY() - tmpresults[0].ResultPos.GetY();

                        double tscoffsetx = Math.Abs(distance2d / moveindex) - Wafer.GetSubsInfo().ActualDieSize.Height.Value;
                        tscoffsetx = Math.Round(tscoffsetx, 6);

                        Wafer.GetSubsInfo().ActualDieSize.Height.Value += tscoffsetx;
                        Wafer.GetSubsInfo().ActualDeviceSize.Height.Value = Wafer.GetSubsInfo().ActualDieSize.Height.Value - Wafer.GetSubsInfo().DieYClearance.Value;

                        LoggerManager.Debug($"RevisionIndexHeight: {Wafer.GetSubsInfo().ActualDieSize.Height.Value}(um).", isInfo: IsInfo);
                        LoggerManager.Debug($"IndexHeightOffset: {tscoffsetx}(um).", isInfo: IsInfo);
                    }

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Error(err.ToString() + "RevisionVertical() : Error occurred.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        protected WaferCoordinate ChangedLocationFormPT(PMResult pmresult)
        {
            try
            {
                WaferCoordinate procwcd = new WaferCoordinate();
                MachineCoordinate mcPos = null;

                try
                {
                    WaferCoordinate wcd = null;

                    if (pmresult.ResultBuffer.CamType == EnumProberCam.WAFER_LOW_CAM)
                    {
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                    }
                    else if (pmresult.ResultBuffer.CamType == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        wcd = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    }

                    double ptxpos = pmresult.ResultParam[0].XPoss;
                    double ptypos = pmresult.ResultParam[0].YPoss;

                    double offsetx = 0.0;
                    double offsety = 0.0;


                    offsetx = ptxpos - (pmresult.ResultBuffer.SizeX / 2);
                    offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;

                    offsetx *= pmresult.ResultBuffer.RatioX.Value;
                    offsety *= pmresult.ResultBuffer.RatioY.Value;

                    if (wcd != null)
                    {
                        procwcd.X.Value = offsetx + wcd.GetX();
                        procwcd.Y.Value = offsety + wcd.GetY();
                        procwcd.Z.Value = wcd.GetZ();

                        LoggerManager.Debug($"[{this.GetType().Name}], ChangedLocationFormPT() : PixelX = {pmresult.ResultParam[0].XPoss}, PixelY = {pmresult.ResultParam[0].YPoss}, PixelAngle = {pmresult.ResultParam[0].Angle}, OffsetX = {offsetx}, OffsetY = {offsety}, OriginX = {wcd.GetX()}, OriginY = {wcd.GetY()}, ProcX = {procwcd.X.Value}, ProcY = {procwcd.Y.Value}", isInfo: true);

                        var axisx = this.MotionManager().GetAxis(EnumAxisConstants.X);
                        var axisy = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                        double curEnX = axisx?.Status?.Position?.Ref ?? -99999;
                        double curEnY = axisy?.Status?.Position?.Ref ?? -99999;

                        if (pmresult.ResultBuffer.CamType == EnumProberCam.WAFER_LOW_CAM)
                        {
                            mcPos = this.CoordinateManager().WaferLowChuckConvert.ConvertBack(procwcd);
                        }
                        else if (pmresult.ResultBuffer.CamType == EnumProberCam.WAFER_HIGH_CAM)
                        {
                            mcPos = this.CoordinateManager().WaferHighChuckConvert.ConvertBack(procwcd);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}], ChangedLocationFormPT() : Machine Coord X : {curEnX}, Y : {curEnY}", isInfo: true);
                        LoggerManager.Debug($"[{this.GetType().Name}], ChangedLocationFormPT() : Pattern Pos Machine Coord X : {mcPos.X.Value:0.00}, Y : {mcPos.Y.Value:0.00}", isInfo: true);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                return procwcd;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int Calc(int jumpindex, double unitdistance, double targetdistance, double initialpos)
        {
            int curindex = 0;
            double preVal = 0;
            double calcVal = 0.0;
            double curVal = 0.0;

            unitdistance = Math.Abs(unitdistance);

            if (initialpos <= targetdistance)
            {
                while (true)
                {
                    calcVal += jumpindex * unitdistance;
                    curVal = calcVal + initialpos;
                    curindex++;

                    if (curVal < targetdistance)
                    {
                        preVal = curVal;
                    }
                    else if (curVal == targetdistance)
                    {
                        return curindex;
                    }
                    else
                    {
                        double preoffset = Math.Abs(targetdistance - preVal);
                        double curoffset = Math.Abs(curVal - targetdistance);

                        curindex = preoffset < curoffset ? curindex-- : curindex;

                        while ((curindex * unitdistance) + initialpos >= initialpos + (Wafer.GetPhysInfo().WaferSize_um.Value / 2) - Wafer.GetPhysInfo().WaferMargin_um.Value)
                        {
                            curindex--;
                        }

                        return curindex;
                    }

                }
            }
            else
            {
                while (true)
                {
                    calcVal -= jumpindex * unitdistance;
                    curVal = calcVal + initialpos;
                    curindex++;

                    if (curVal > targetdistance)
                    {
                        preVal = curVal;
                    }
                    else if (curVal == targetdistance)
                    {
                        return curindex;
                    }
                    else
                    {
                        double preoffset = Math.Abs(targetdistance - preVal);
                        double curoffset = Math.Abs(curVal - targetdistance);
                        curindex = preoffset < curoffset ? curindex-- : curindex;

                        while ((curindex * unitdistance) + initialpos >= initialpos + (Wafer.GetPhysInfo().WaferSize_um.Value / 2) - Wafer.GetPhysInfo().WaferMargin_um.Value)
                        {
                            curindex--;
                        }
                        return curindex;
                    }
                }
            }
        }

        public EventCodeEnum ValidationTesting(PatternInfomation ptinfo, IFocusing focusmodel = null, IFocusParameter parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY() + ptinfo.GetY());

                }
                else if (ptinfo.CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(ptinfo.GetX() + Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY() + ptinfo.GetY());
                }

                retVal = this.VisionManager().PatternMatching(ptinfo, this).RetValue;

                if (retVal != EventCodeEnum.NONE)
                {
                    retVal = EventCodeEnum.THETA_ALIGN_PATTERN_VALIDATION_TESTING_FAILED;
                }

                this.VisionManager().StartGrab(ptinfo.CamType.Value, this);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.ToString()} ThetaAlignStadnard - VaildationTesting () : Error occurred.");
            }
            return retVal;
        }

        public abstract EventCodeEnum LoadDevParameter();
        public abstract EventCodeEnum SaveDevParameter();
        public new void DeInitModule()
        {
            return;
        }
        public bool IsExecute()
        {
            bool retVal = true;
            return retVal;
        }
    }
}
