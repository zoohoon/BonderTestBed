using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPCardAlign
{
    using CardChange;
    using CylType;
    using GeometryHelp;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using System.IO;
    using System.Windows;

    public class GPCardAligner : IGPCardAligner, IFactoryModule
    {
        private ICardChangeSysParam CardChangeSysParam => this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
        private ICardChangeDevParam CardChangeDevParam => this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
        private IFocusing _CardFocusModel;
        public IFocusing CardFocusModel
        {
            get
            {
                if (_CardFocusModel == null)
                    _CardFocusModel = this.FocusManager().GetFocusingModel((CardChangeSysParam as CardChangeSysParam).FocusingModuleDllInfo);

                return _CardFocusModel;
            }
        }
        private IFocusParameter FocusParam => (CardChangeSysParam as CardChangeSysParam).FocusParam;

        public IFocusParameter PLFocusParam
        {
            get { return (CardChangeSysParam as CardChangeSysParam).PLFocusParam; }
        }

        private IGPCCObservationTempParam GPCCObservationTempParam => this.CardChangeModule().GPCCObservationTempParams_IParam as IGPCCObservationTempParam;
        private String PatternPath { get; set; }
        private String FMPatternPath { get; set; }
        private String PogoPatternPath { get; set; }
        private String PogoPatternPath3P { get; set; }
        private string PodPattPath = "";
        private String _GPCardAlignLogPath;
        private List<PatternRelPoint> _PatternRelPointList9;//==> Pattern Match를 실패할 경우 탐색하는 상대 위치를 모아 두었다.(3X3)
        private List<PatternRelPoint> _PatternRelPointList25;//==> Pattern Match를 실패할 경우 탐색하는 상대 위치를 모아 두었다.(5X5)

        public GPCardAligner()
        {
            _PatternRelPointList9 = new List<PatternRelPoint>();
            List<PatternRelPoint> oddSearchPosList3 = GetSnailPointsOdd(3);
            _PatternRelPointList9.AddRange(oddSearchPosList3);

            _PatternRelPointList25 = new List<PatternRelPoint>();
            List<PatternRelPoint> oddSearchPosList5 = GetSnailPointsOdd(5);
            _PatternRelPointList25.AddRange(oddSearchPosList5);

            FMPatternPath = Path.Combine(this.FileManager().GetSystemRootPath(), @"CardChange\Pattern\FMPattern");//==> Pattern Image 저장 위치
            PatternPath = Path.Combine(this.FileManager().GetDeviceRootPath(), this.FileManager().GetDeviceName()
                + "\\CardChange\\Pattern");
            PogoPatternPath = Path.Combine(this.FileManager().GetSystemRootPath(), @"CardChange\Pattern\GPCCPattern");//==> Pattern Image 저장 위치
            PogoPatternPath3P = Path.Combine(this.FileManager().GetSystemRootPath(), @"CardChange\Pattern\GPCCPattern3P");//==> Pattern Image 저장 위치
            _GPCardAlignLogPath = Path.Combine(this.FileManager().GetSystemRootPath(), @"CardChange\Pattern\\GPCC_AlignDiff.txt");//==> For Test : Card Docking 위치 차이를 기록하기 위한 로그 파일 위치.
            PodPattPath = Path.Combine(this.FileManager().GetSystemRootPath(), @"CardChange\Pattern\GPCCPattern");
        }

        //==> Card Align 수행
        public EventCodeEnum Align()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal = EventCodeEnum.INVALID_PARAMETER_FIND;
                    return retVal;
                }

                //==> Card를 안전한 위치로 이동 시킴, (Stage Module 상태를 ZCLEARED 상태로 만들지는 않음.)
                var result = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (result != EventCodeEnum.NONE)
                {
                    retVal = EventCodeEnum.GP_CardChange_MOVE_ERROR;
                    return retVal;
                }
                //==> 각도를 0도로 맞춤.
                //result = this.MotionManager().AbsMove(EnumAxisConstants.C, 0.0 * 10000);
                var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);

                result = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(axist, 0.0 * 10000, axist.Param.Speed.Value, axist.Param.Acceleration.Value);
                if (result != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : ZSWNet Down");
                    retVal = EventCodeEnum.GP_CardChange_MOVE_ERROR;
                    return retVal;
                }

                //==> Card Align
                double cardCenterDiffX;
                double cardCenterDiffY;
                double cardDegreeDiff;
                double cardAvgZ = 0;

                // For direct docking type, card align should be done before align.
                if (CardChangeSysParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                {
                    var handoffPos = CardChangeSysParam.CardTransferPos.Value;
                    var offsetX = CardChangeSysParam.CardTransferOffsetX.Value;
                    var offsetY = CardChangeSysParam.CardTransferOffsetY.Value;
                    var offsetZ = CardChangeSysParam.CardTransferOffsetZ.Value;
                    var offsetT = CardChangeSysParam.CardTransferOffsetT.Value;

                    var loadPosX = this.CoordinateManager().StageCoord.ChuckLoadingPosition.X.Value;
                    var loadPosY = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Y.Value;
                    var loadPosZ = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Z.Value;
                    //var loadPosT = 0d;
                    cardCenterDiffX = -offsetX;
                    cardCenterDiffY = -offsetY;
                    cardDegreeDiff = -offsetT;
                    //cardAvgZ = CardChangeSysParam.CardPodCenterZ.Value;             // Expected value: ~3000um
                    if (CardChangeSysParam.CardTopFromChuckPlane == null)
                    {
                        CardChangeSysParam.CardTopFromChuckPlane = new Element<double>();
                        CardChangeSysParam.CardTopFromChuckPlane.Value = 30000;
                    }
                    if (CardChangeSysParam.CardTopFromChuckPlane.Value == 0)
                    {
                        CardChangeSysParam.CardTopFromChuckPlane.Value = 30000;
                    }
                    cardAvgZ = CardChangeSysParam.CardTopFromChuckPlane.Value;
                }
                else
                {
                    if (AlignCard(out cardCenterDiffX, out cardCenterDiffY, out cardDegreeDiff, out cardAvgZ) == false)
                    {
                        LoggerManager.Error($"CardAligner Fail : Card Algin");
                        retVal = EventCodeEnum.GP_CardChange_CARD_ALIGN_FAIL;
                        return retVal;
                    }
                }

                //==> Wafer Cam 접는다.
                StageCylinderType.MoveWaferCam.Retract();

                double pogoCenterDiffX = 0;
                double pogoCenterDiffY = 0;
                double pogoDegreeDiff = 0;
                double pogoAvgZ = 0;

                if (CardChangeSysParam.GP_ManualPogoAlignMode)
                {
                    // Door를 닫을 때 Arm이 Stage 안쪽으로 들어와 있나 확인하기 위해 만들어 놓은 체크 함수.
                    // 로더 암에 포지션을 체크함. <-- 이 함수를 이용하여 포고 얼라인 시 로더 암이 들어와 있을 경우 align을 진행하지 않게 처리 함.
                    EventCodeEnum isShutterCloseRetVal = EventCodeEnum.UNDEFINED;
                    isShutterCloseRetVal = this.LoaderController().IsShutterClose();
                    if (isShutterCloseRetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GPCardAligner.AlignPogo() Fail. The stage door is open and the loader arm is inside the stage, so the alignment cannot proceed.");
                        retVal = EventCodeEnum.GP_CardChange_POGO_ALIGN_FAIL;
                        return retVal;
                    }

                    //==> Pogo Align Manual 모드일 경우 그냥 넘어감
                    pogoCenterDiffX = cardChangeDevParam.GP_PogoCenterDiffX;
                    pogoCenterDiffY = cardChangeDevParam.GP_PogoCenterDiffY;
                    pogoDegreeDiff = cardChangeDevParam.GP_PogoDegreeDiff;

                    pogoCenterDiffX = CardChangeSysParam.GP_PogoCenter.X.Value;
                    pogoCenterDiffY = CardChangeSysParam.GP_PogoCenter.Y.Value;
                    pogoDegreeDiff = CardChangeSysParam.GP_PogoCenter.T.Value;

                    if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                    {
                        for (int i = 0; i < CardChangeSysParam.GP_SearchedPogoMarkPosList.Count; ++i)
                        {
                            //==> Pattern을 찾은 위치를 저장
                            PinCoordinate searchedPogoMarkPos = CardChangeSysParam.GP_SearchedPogoMarkPosList[i];

                            pogoAvgZ += searchedPogoMarkPos.Z.Value;
                        }
                        pogoAvgZ = pogoAvgZ / CardChangeSysParam.GP_SearchedPogoMarkPosList.Count;
                    }
                    else if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                    {
                        for (int i = 0; i < CardChangeSysParam.GP_SearchedPogoMarkPosList3P.Count; ++i)
                        {
                            //==> Pattern을 찾은 위치를 저장
                            PinCoordinate searchedPogoMarkPos = CardChangeSysParam.GP_SearchedPogoMarkPosList3P[i];

                            pogoAvgZ += searchedPogoMarkPos.Z.Value;
                        }
                        pogoAvgZ = pogoAvgZ / CardChangeSysParam.GP_SearchedPogoMarkPosList3P.Count;
                    }
                    else
                    {
                        LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                    }


                    LoggerManager.Debug($"CardChangeSysParam.GP_PogoCenter X = {pogoCenterDiffX:0.000}, Y = {pogoCenterDiffY:0.000}, T = {pogoDegreeDiff:0.000}, Z = {pogoAvgZ:0.000}");
                }
                else//==> POGO Align
                {
                    bool pogoAlignResult = false;
                    if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                    {
                        pogoAlignResult = AlignPogo(out pogoCenterDiffX, out pogoCenterDiffY, out pogoDegreeDiff, out pogoAvgZ);
                    }
                    else if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                    {
                        pogoAlignResult = AlignPogo3P(out pogoCenterDiffX, out pogoCenterDiffY, out pogoDegreeDiff, out pogoAvgZ);
                    }
                    else
                    {
                        LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                        pogoAlignResult = false;
                    }

                    if (pogoAlignResult == false)
                    {
                        LoggerManager.Error($"CardAligner Fail : Pogo Algin");
                        retVal = EventCodeEnum.GP_CardChange_POGO_ALIGN_FAIL;
                        return retVal;
                    }
                }

                ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                ProbeAxisObject tAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);

                double curXPos = 0;
                double curYPos = 0;
                double curTPos = 0;
                WaferCoordinate wafer = new WaferCoordinate(cardCenterDiffX, cardCenterDiffY, cardAvgZ, cardDegreeDiff);

                PinCoordinate pin = new PinCoordinate(pogoCenterDiffX, pogoCenterDiffY, pogoAvgZ, pogoDegreeDiff);

                var matchedPos = this.CoordinateManager().WaferHighChuckConvert.GetWaferPinAlignedPosition(wafer, pin);

                //ISSD-3188 Docking X, Y Offset 추가
                matchedPos.X.Value = matchedPos.GetX() + CardChangeSysParam.GP_ContactCorrectionX;
                matchedPos.Y.Value = matchedPos.GetY() + CardChangeSysParam.GP_ContactCorrectionY;

                double radiusDist = Math.Sqrt(Math.Pow(matchedPos.GetX(), 2) + Math.Pow(matchedPos.GetY(), 2));
                double CardContactRadiusLimit = cardChangeSysParam.CardContactRadiusLimit.Value;
                if (radiusDist > CardContactRadiusLimit)
                {
                    LoggerManager.Error($"CardContact Move X,Y Failed. CardContactRadiusLimit: {CardContactRadiusLimit}, Radius:{radiusDist:0.00}");
                    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retVal = EventCodeEnum.GP_CardChange_CARD_CONTACT_LIMIT_ERROR;
                    return retVal;
                }
                else
                {
                    this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(xAxis, matchedPos.GetX(), xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                    this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(yAxis, matchedPos.GetY(), yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                    this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(tAxis, matchedPos.GetT() * 10000, tAxis.Param.Speed.Value, tAxis.Param.Acceleration.Value);



                    ////==> Card 오차 보정 1, WL 좌표와 Machin 좌표의 X,Y가 반대라서 '-'로 보정함.(계산적으로 보았을때는 X,Y에 +를 해야 하는데...
                    //this.MotionManager().AbsMove(xAxis, curXPos - cardCenterDiffX, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                    //this.MotionManager().AbsMove(yAxis, curYPos - cardCenterDiffY, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                    //this.MotionManager().AbsMove(EnumAxisConstants.C, curTPos + (cardDegreeDiff * 10000));

                    ////==> Pogo 오차 보정
                    //this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                    //this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                    //this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);

                    //this.MotionManager().AbsMove(xAxis, curXPos + pogoCenterDiffX, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                    //this.MotionManager().AbsMove(yAxis, curYPos + pogoCenterDiffY, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                    //this.MotionManager().AbsMove(EnumAxisConstants.C, curTPos + (pogoDegreeDiff * 10000 * -1));

                    ////==> Pin Pad Match 오차 보정(파라미터에 저장된 값 만큼 보정 시킴
                    //double correctionX = cardChangeParam.GP_ContactCorrectionX;
                    //double correctionY = cardChangeParam.GP_ContactCorrectionY;
                    //this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                    //this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);

                    //this.MotionManager().AbsMove(xAxis, curXPos + correctionX, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                    //this.MotionManager().AbsMove(yAxis, curYPos + correctionY, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);


                    //==> Card를 POGO에 Docking하는 좌표를 System Parameter에 저장함(X,Y,T)
                    this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                    this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                    this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);

                    cardChangeDevParam.GP_CardContactPosX = curXPos;
                    cardChangeDevParam.GP_CardContactPosY = curYPos;
                    cardChangeDevParam.GP_CardContactPosT = curTPos;
                    cardChangeDevParam.GP_CardContactPosZ = matchedPos.GetZ();

                    cardChangeSysParam.GP_Undock_CardContactPosX = curXPos;
                    cardChangeSysParam.GP_Undock_CardContactPosY = curYPos;
                    cardChangeSysParam.GP_Undock_CardContactPosZ = matchedPos.GetZ();
                    cardChangeSysParam.GP_Undock_CardContactPosT = curTPos;
                    LoggerManager.Debug($"CardAlign Matched Position X = {matchedPos.GetX():0.000}, Y = {matchedPos.GetY():0.000}, T = {matchedPos.GetT():0.000}, Z = {matchedPos.GetZ():0.000}");
                    result = this.CardChangeModule().SaveSysParameter();
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Save Align Contact Position fail");
                        retVal = EventCodeEnum.GP_CardChange_CARD_CHANGE_SAVE_PARAM_ERROR;
                        return retVal;
                    }
                    result = this.CardChangeModule().SaveDevParameter();
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Save Align Contact Position fail");
                        retVal = EventCodeEnum.GP_CardChange_CARD_CHANGE_SAVE_PARAM_ERROR;
                        return retVal;
                    }

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool AlignDDCard(out PinCoordinate cardcenterpos, ProberCardListParameter proberCard)
        {
            bool retval = false;
            EventCodeEnum eventCodeRet = EventCodeEnum.UNDEFINED;
            cardcenterpos = new PinCoordinate();
            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return true;
                }

                string alignPath;
                alignPath = Path.Combine(this.FileManager().GetSystemRootPath(), @"CardChange\Pattern\FMPattern\" + proberCard.CardID);

                try
                {
                    if (Directory.Exists(alignPath) == false)
                    {
                        Directory.CreateDirectory(alignPath);
                    }
                    else
                    {
                        var filelist = Directory.GetFiles(alignPath);
                        if (filelist.Count() > 0)
                        {
                            LoggerManager.Debug($"AlignDDCard() The cell already has a card pattern image file.");
                        }
                        else
                        {
                            var ret = DownLoadCardImageFromLoader(true, proberCard.CardID); // Cell에 패턴이미지가 없을 경우 로더에서 다운받음.
                            if (ret != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"Error occurd while Downlaod card image from loader in GetPatternPosList func");
                                retval = false;
                                return retval;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerManager.Exception(ex);
                }

                bool dileft_up_module;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out dileft_up_module);
                bool diright_up_module;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diright_up_module);

                if (dileft_up_module == true || diright_up_module == true)//Card Pod Left, Right 둘중 하나라도 True(Up)이라면 Align 하면 안됨.
                {
                    LoggerManager.Debug($"Card Pod State is Up, You can't alignment.");
                    retval = false;
                    return retval;
                }
                if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                {
                    eventCodeRet = this.StageSupervisor().StageModuleState.CCZCLEARED();

                    if (eventCodeRet != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    eventCodeRet = this.MarkAligner().DoMarkAlign();
                    if (eventCodeRet != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    StageCylinderType.MoveWaferCam.Retract();
                }
                else
                {
                    eventCodeRet = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (eventCodeRet != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    StageCylinderType.MoveWaferCam.Retract();
                }

                List<CatCoordinates> regFiducialPoss = new List<CatCoordinates>();

                foreach (var fid in proberCard.FiducialMarInfos)
                {
                    regFiducialPoss.Add(fid.FiducialMarkPos);
                }

                this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);
                System.Threading.Thread.Sleep(100);
                
                this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM, this);
                System.Threading.Thread.Sleep(100);

                List<CatCoordinates> patternPosList = SearchCardPattPositions(regFiducialPoss, alignPath);

                if (patternPosList.Count >= 2)
                {
                    var firstPoint = new PinCoordinate(
                        patternPosList.FirstOrDefault().X.Value,
                        patternPosList.FirstOrDefault().Y.Value,
                        patternPosList.FirstOrDefault().Z.Value);
                    var secPoint = new PinCoordinate(
                        patternPosList.LastOrDefault().X.Value,
                        patternPosList.LastOrDefault().Y.Value,
                        patternPosList.LastOrDefault().Z.Value);

                    //피디셜마크 얼라인 성공 이후 해당 값으로 파라미터 저장
                    proberCard.FiducialMarInfos.FirstOrDefault().FiducialMarkPos = firstPoint;
                    proberCard.FiducialMarInfos.LastOrDefault().FiducialMarkPos = secPoint;

                    var card = CardChangeSysParam.ProberCardList.Where(x => x.CardID == proberCard.CardID).FirstOrDefault();
                    if (card == null)
                    {
                        CardChangeSysParam.ProberCardList.Add(proberCard);
                    }

                    this.CardChangeModule().SaveSysParameter();
                    
                    var pattDist = GeometryHelper.GetDistance2D(firstPoint, secPoint);
                    var CardCenterOffsetDist = GeometryHelper.GetDistance2D(proberCard.FiducialMarInfos.First().CardCenterOffset, proberCard.FiducialMarInfos.LastOrDefault().CardCenterOffset);

                    if (CardChangeSysParam.PatternDistMargin.Value > Math.Abs(pattDist - CardCenterOffsetDist))
                    {
                        var cardAngle = GeometryHelper.GetAngle(firstPoint, secPoint);
                        var centerOffset = proberCard.FiducialMarInfos.First().CardCenterOffset;

                        PinCoordinate rotatedCenterPoint = new PinCoordinate();
                        GeometryHelper.GetRotCoordEx(
                            ref rotatedCenterPoint,
                            centerOffset + proberCard.FiducialMarInfos.First().FiducialMarkPos,
                            proberCard.FiducialMarInfos.First().FiducialMarkPos,
                            cardAngle);
                        rotatedCenterPoint.Z.Value = firstPoint.Z.Value;
                        rotatedCenterPoint.T.Value = cardAngle * 10000;
                        rotatedCenterPoint.CopyTo(cardcenterpos);
                        LoggerManager.Debug($"CardAlign Successed. Measured Distance first point and second point: Distance = {pattDist:0.00}um, CardCenterOffsetDist. Dist = {CardCenterOffsetDist}um, " +
                                            $"Diff = {Math.Abs(pattDist - CardCenterOffsetDist)}, PatternDistMargin = {CardChangeSysParam.PatternDistMargin.Value}");
                        LoggerManager.Debug($"AlignDDCard(): Measured card center ({cardcenterpos.X.Value:0.00},{cardcenterpos.Y.Value:0.00},{cardcenterpos.Z.Value:0.00}), Angle = {cardAngle}");
                        retval = true;
                    }
                    else
                    {
                        LoggerManager.Error($"CardAlign Failed. Acquired positions are invalid. Distance = {pattDist:0.00}um, CardCenterOffsetDist. Dist = {CardCenterOffsetDist}um, " +
                                            $"Diff = {Math.Abs(pattDist - CardCenterOffsetDist)}, PatternDistMargin = {CardChangeSysParam.PatternDistMargin.Value}");
                        eventCodeRet = this.StageSupervisor().StageModuleState.CCZCLEARED();
                        retval = false;
                    }
                }
                else
                {
                    LoggerManager.Error($"CardAlign Failed. Acquired position is invalid. Not enough pattern points({patternPosList.Count}).");
                    eventCodeRet = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retval = false;
                }


                ////==> 설계상의 Card Pattern 위치
                //List<CatCoordinates> designPosList = new List<CatCoordinates>(CardChangeSysParam.GP_CardMarkPosList);
                ////==> 실제 Card Pattern 위치
                //List<CatCoordinates> searchedPosList = new List<CatCoordinates>(CardChangeSysParam.GP_SearchedCardMarkPosList);

                ////==> desing pos로 이동한 다음 Pattern Match를 하고 실제 위치를 searched pos에 저장을 한다.
                //this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);
                //System.Threading.Thread.Sleep(100);
                //this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM);
                //System.Threading.Thread.Sleep(100);
                //List<CatCoordinates> patternPosList = SearchCardPattPositions(regFiducialPoss);

                //if (designPosList.Count != patternPosList.Count)
                //{
                //    LoggerManager.Error($"Designed position and pattern location counts is differ. designPosListCount = {designPosList.Count}, PatternPosListCount = {patternPosList.Count}");
                //    retval = false;
                //    return retval;
                //}
                ////==> 패턴 매칭 해서 찾은 포지션을 저장해서 다음  Align 에서 사용
                //for (int i = 0; i < searchedPosList.Count; ++i)
                //{
                //    //==> Pattern을 찾은 위치를 저장
                //    WaferCoordinate searchedCardMarkPos = CardChangeSysParam.GP_SearchedCardMarkPosList[i];
                //    CatCoordinates patternPos = patternPosList[i];
                //    searchedCardMarkPos.X.Value = patternPos.X.Value;
                //    searchedCardMarkPos.Y.Value = patternPos.Y.Value;
                //    searchedCardMarkPos.Z.Value = patternPos.Z.Value;
                //    cardAvgZ += searchedCardMarkPos.Z.Value;
                //}
                //cardAvgZ = cardAvgZ / searchedPosList.Count;

                //EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();
                ////this.CardChangeModule().SaveDevParameter();
                //if (saveResult != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error("Save Card Align Result Failed");
                //}


                ////==> design pos 와 pattern match pos를 이용하여 X, Y, Theta 편차를 얻어옴
                //GetCorrectionCoordinate(designPosList, patternPosList, out centerDiffX, out centerDiffY, out degreeDiff);

                String cardAlignResultStr = $"GPCC Card Diff ... : X,Y,Z Pos. : ({cardcenterpos.X.Value:0.00}, {cardcenterpos.Y.Value:0.00}, {cardcenterpos.Z.Value:0.00}), Angle = {cardcenterpos.T.Value:0.00}";

                //double radiusDist = Math.Sqrt(Math.Pow(cardcenterpos.X.Value, 2) + Math.Pow(cardcenterpos.Y.Value, 2));
                //if (radiusDist > this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value)
                //{
                //    LoggerManager.Error($"CardAlign Failed. ProbingRadiusLimit: {this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value}, Radius:{radiusDist:0.00}");
                //    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();
                //    retval = false;
                //}
                //else
                //{
                LoggerManager.Debug(cardAlignResultStr);
                using (StreamWriter sw = new StreamWriter(_GPCardAlignLogPath, append: true))
                {
                    sw.WriteLine(cardAlignResultStr);
                }
                eventCodeRet = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (eventCodeRet != EventCodeEnum.NONE)
                {
                    retval = false;
                    return retval;
                }

                UpdateCardContactPosZ();
                //retval = true;
                //}
            }
            catch (Exception er)
            {
                LoggerManager.Exception(er);
            }
            return retval;

        }
        public bool AlignCard(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double cardAvgZ, ProberCardListParameter proberCard = null)
        {
            ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
            bool retVal = false;

            List<CatCoordinates> designPosList = new List<CatCoordinates>(CardChangeSysParam.GP_CardMarkPosList);
            PinCoordinate cardCenterInPinCoord = new PinCoordinate();
            centerDiffX = centerDiffY = degreeDiff = 0d;
            cardAvgZ = designPosList.FirstOrDefault().Z.Value;
            try
            {
                CardChangeSysParam.CardAlignState = AlignStateEnum.IDLE;
                switch (cardChangeSysParam.CardDockType.Value)
                {
                    case EnumCardDockType.DIRECTDOCK:
                        if (proberCard != null)
                        {
                            if (AlignDDCard(out cardCenterInPinCoord, proberCard))
                            {
                                double xpos = this.CoordinateManager().StageCoord.ChuckLoadingPosition.X.Value;
                                double ypos = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Y.Value;
                                double zpos = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Z.Value;
                                if (cardChangeSysParam.CardPodCenterX == null
                                    | cardChangeSysParam.CardPodCenterY == null
                                    | cardChangeSysParam.CardPodCenterZ == null)
                                {
                                    cardChangeSysParam.CardPodCenterX = new Element<double>();
                                    cardChangeSysParam.CardPodCenterY = new Element<double>();
                                    cardChangeSysParam.CardPodCenterZ = new Element<double>();
                                }
                                WaferCoordinate cardPodPosInChuckCoord = new WaferCoordinate(
                                    cardChangeSysParam.CardPodCenterX.Value,
                                    cardChangeSysParam.CardPodCenterY.Value,
                                    cardChangeSysParam.CardPodCenterZ.Value);

                                var matchedPos = this.CoordinateManager().WaferHighChuckConvert.GetWaferPinAlignedPosition(
                                    cardPodPosInChuckCoord, cardCenterInPinCoord);

                                centerDiffX = matchedPos.X.Value - xpos;
                                centerDiffY = matchedPos.Y.Value - ypos;
                                degreeDiff = cardCenterInPinCoord.T.Value;
                                cardAvgZ = matchedPos.Z.Value;
                                LoggerManager.Debug($"AlignCard(): Loading Pos. X:{xpos:0.00} Y:{ypos:0.00} Z:{zpos:0.00}");
                                LoggerManager.Debug($"AlignCard(): Succeed. X:{centerDiffX:0.00} Y:{centerDiffY:0.00} Z:{cardAvgZ:0.00}, Angle = {degreeDiff:0.0000}");

                                if (cardChangeSysParam.CardTransferPos.Value == null)
                                {
                                    cardChangeSysParam.CardTransferPos.Value = new CatCoordinates();
                                }
                                cardChangeSysParam.CardTransferPos.Value = new CatCoordinates(
                                    matchedPos.X.Value, matchedPos.Y.Value, matchedPos.Z.Value, cardCenterInPinCoord.T.Value);
                                var tfPos = cardChangeSysParam.CardTransferPos.Value;
                                LoggerManager.Debug($"AlignCard(): CardTransferPos =  X:{tfPos.X.Value:0.00} Y:{tfPos.Y.Value:0.00} Z:{tfPos.Z.Value:0.00}, Angle = {tfPos.T.Value:0.0000}");

                                retVal = true;
                            }
                        }
                        break;
                    case EnumCardDockType.INVALID:
                    case EnumCardDockType.NORMAL:
                    default:
                        retVal = AlignPGCard(out centerDiffX, out centerDiffY, out degreeDiff, out cardAvgZ);
                        break;
                }

                if (retVal == false)
                {
                    LoggerManager.Error($"CardAligner Fail : Card Algin");
                    CardChangeSysParam.CardAlignState = AlignStateEnum.FAIL;
                }
                else
                {
                    LoggerManager.Debug($"CardAlign Success : Align State is Done");
                    CardChangeSysParam.CardAlignState = AlignStateEnum.DONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"");
            }

            return retVal;
        }


        public bool AlignPGCard(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double cardAvgZ)
        {
            bool retval = false;
            EventCodeEnum err = EventCodeEnum.UNDEFINED;
            centerDiffX = 0;
            centerDiffY = 0;
            degreeDiff = 0;
            cardAvgZ = 0;
            try
            {
                PatternPath = Path.Combine(this.FileManager().GetDeviceRootPath(), this.FileManager().GetDeviceName()
                        + "\\CardChange\\Pattern");
                try
                {
                    if (Directory.Exists(PatternPath) == false)
                    {
                        Directory.CreateDirectory(PatternPath);
                    }
                }
                catch (Exception ex)
                {
                    LoggerManager.Exception(ex);
                }

                if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                {
                    err = this.StageSupervisor().StageModuleState.CCZCLEARED();

                    if (err != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    err = this.MarkAligner().DoMarkAlign();
                    if (err != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                }
                else
                {
                    err = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (err != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                }

                //var result = this.StageSupervisor().StageModuleState.ZCLEARED();
                //if (result != EventCodeEnum.NONE)
                //{
                //    return false;
                //}

                //==> 설계상의 Card Pattern 위치
                List<CatCoordinates> designPosList = new List<CatCoordinates>(CardChangeSysParam.GP_CardMarkPosList);
                //==> 실제 Card Pattern 위치
                List<CatCoordinates> searchedPosList = new List<CatCoordinates>(CardChangeSysParam.GP_SearchedCardMarkPosList);

                //==> desing pos로 이동한 다음 Pattern Match를 하고 실제 위치를 searched pos에 저장을 한다.
                this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);
                System.Threading.Thread.Sleep(100);

                this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM, this);
                System.Threading.Thread.Sleep(100);

                List<CatCoordinates> patternPosList = GetPatternPosList(EnumProberCam.WAFER_LOW_CAM, searchedPosList);

                if (designPosList.Count != 4 || searchedPosList.Count != 4 || designPosList.Count != patternPosList.Count)
                {
                    LoggerManager.Error($"Designed position and pattern location counts is differ. designPosListCount = {designPosList.Count}, PatternPosListCount = {patternPosList.Count}");
                    retval = false;
                    return retval;
                }

                //==> 패턴 매칭 해서 찾은 포지션을 저장해서 다음  Alignn 에서 사용

                for (int i = 0; i < searchedPosList.Count; ++i)
                {
                    //==> Pattern을 찾은 위치를 저장
                    WaferCoordinate searchedCardMarkPos = CardChangeSysParam.GP_SearchedCardMarkPosList[i];
                    CatCoordinates patternPos = patternPosList[i];

                    searchedCardMarkPos.X.Value = patternPos.X.Value;
                    searchedCardMarkPos.Y.Value = patternPos.Y.Value;
                    searchedCardMarkPos.Z.Value = patternPos.Z.Value;

                    cardAvgZ += searchedCardMarkPos.Z.Value;
                }

                cardAvgZ = cardAvgZ / searchedPosList.Count;

                EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();
                this.CardChangeModule().SaveDevParameter();
                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save Pogo Align Result Fail");
                }


                //==> design pos 와 pattern match pos를 이용하여 X, Y, Theta 편차를 얻어옴
                GetCorrectionCoordinate(designPosList, patternPosList, out centerDiffX, out centerDiffY, out degreeDiff);
                String cardAlignResultStr = $"GPCC Card Diff ... : X,Y,T Pos : {centerDiffX}, {centerDiffY}, {degreeDiff}";

                double radiusDist = Math.Sqrt(Math.Pow(centerDiffX, 2) + Math.Pow(centerDiffY, 2));
                if (radiusDist > this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value)
                {
                    LoggerManager.Error($"CardAlign Failed. ProbingRadiusLimit: {this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value}, Radius:{radiusDist:0.00}");
                    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retval = false;
                }
                else
                {
                    LoggerManager.Debug(cardAlignResultStr);

                    using (StreamWriter sw = new StreamWriter(_GPCardAlignLogPath, append: true))
                    {
                        sw.WriteLine(cardAlignResultStr);
                    }

                    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();

                    if (ret != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }

                    UpdateCardContactPosZ();

                    retval = true;
                }
            }
            catch (Exception er)
            {
                LoggerManager.Exception(er);
            }
            return retval;

        }
        public bool AlignCardPod(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double cardAvgZ)
        {
            bool retval = false;
            EventCodeEnum err = EventCodeEnum.UNDEFINED;
            centerDiffX = 0;
            centerDiffY = 0;
            degreeDiff = 0;
            cardAvgZ = 0;
            String cardAlignResultStr = "Not defined.";
            try
            {
                if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                {
                    err = this.StageSupervisor().StageModuleState.CCZCLEARED();

                    if (err != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    err = this.MarkAligner().DoMarkAlign();
                    if (err != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    err = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (err != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    var ret = StageCylinderType.MoveWaferCam.Retract();
                    if (ret != 0)
                    {
                        retval = false;
                        return retval;
                    }
                }
                else
                {
                    err = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (err != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                }
                //==> 설계상의 Card Pattern 위치
                List<CatCoordinates> designPosList = new List<CatCoordinates>(CardChangeSysParam.CardPodMarkPosList);
                //==> 실제 Card Pattern 위치
                List<CatCoordinates> searchedPosList = new List<CatCoordinates>(CardChangeSysParam.CardPodMarkPosList);
                //==> desing pos로 이동한 다음 Pattern Match를 하고 실제 위치를 searched pos에 저장을 한다.
                this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);
                System.Threading.Thread.Sleep(100);
                
                this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM, this);
                System.Threading.Thread.Sleep(100);

                List<CatCoordinates> patternPosList = GetPatternPosList(EnumProberCam.WAFER_LOW_CAM, searchedPosList);
                if (designPosList.Count != patternPosList.Count)
                {
                    LoggerManager.Error($"Designed position and pattern location counts is differ. designPosListCount = {designPosList.Count}, PatternPosListCount = {patternPosList.Count}");
                    retval = false;
                    return retval;
                }
                //==> 패턴 매칭 해서 찾은 포지션을 저장해서 다음  Alignn 에서 사용
                for (int i = 0; i < searchedPosList.Count; ++i)
                {
                    //==> Pattern을 찾은 위치를 저장
                    WaferCoordinate searchedCardMarkPos = CardChangeSysParam.CardPodMarkPosList[i];
                    CatCoordinates patternPos = patternPosList[i];
                    searchedCardMarkPos.X.Value = patternPos.X.Value;
                    searchedCardMarkPos.Y.Value = patternPos.Y.Value;
                    searchedCardMarkPos.Z.Value = patternPos.Z.Value;
                    cardAvgZ += searchedCardMarkPos.Z.Value;
                }
                cardAvgZ = cardAvgZ / searchedPosList.Count;
                EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();
                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save POD Align Result Fail");
                }
                // 세점을 이용하여 중심점을 계산 한다. 결과는 웨이퍼 좌표계로 생성 되며
                // CardChangeSysParam.CardPodCenter...로 저장되어 도킹 시 핸드오프 시작점에 적용 되어 진다.
                double podRadius = 0;
                if (CardChangeSysParam.CardPodMarkPosList != null)
                {
                    if (CardChangeSysParam.CardPodMarkPosList.Count > 3)
                    {
                        var circle = GeometryHelper.CircleFromPoints(
                            CardChangeSysParam.CardPodMarkPosList[0],
                            CardChangeSysParam.CardPodMarkPosList[1],
                            CardChangeSysParam.CardPodMarkPosList[2]);
                        CardChangeSysParam.CardPodCenterX.Value = circle.Center.X;
                        CardChangeSysParam.CardPodCenterY.Value = circle.Center.Y;
                        podRadius = circle.Radius;
                        CardChangeSysParam.CardPodCenterZ.Value = cardAvgZ;
                    }
                }
                cardAlignResultStr = $"Card POD Center ... : (X:{CardChangeSysParam.CardPodCenterX.Value:0.00}, Y:{CardChangeSysParam.CardPodCenterY.Value:0.00}, Z:{cardAvgZ})";
                double maxCardPodRadius = CardChangeSysParam.CardPodRadiusMax.Value; // 300mm의 1.5배
                double minCardPodRadius = CardChangeSysParam.CardPodRadiusMin.Value;
                double minCardPodHeight = CardChangeSysParam.CardPodMinHeight.Value;
                if (podRadius > maxCardPodRadius | podRadius < minCardPodRadius |
                    CardChangeSysParam.CardPodCenterZ.Value < minCardPodHeight)
                {
                    LoggerManager.Error($"Card POD Alignment Failed. Pod radius error, Radius:{podRadius:0.00}");
                    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retval = false;
                }
                else
                {
                    LoggerManager.Debug(cardAlignResultStr);
                    using (StreamWriter sw = new StreamWriter(_GPCardAlignLogPath, append: true))
                    {
                        sw.WriteLine(cardAlignResultStr);
                    }
                    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (ret != EventCodeEnum.NONE)
                    {
                        retval = false;
                        return retval;
                    }
                    retval = true;
                }
            }
            catch (Exception er)
            {
                LoggerManager.Exception(er);
            }
            return retval;
        }
        public bool AlignPogo(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double pogoAvgZ)
        {
            centerDiffX = 0;
            centerDiffY = 0;
            degreeDiff = 0;
            pogoAvgZ = 0;

            bool retVal = false;
            try
            {
                // Door를 닫을 때 Arm이 Stage 안쪽으로 들어와 있나 확인하기 위해 만들어 놓은 체크 함수.
                // 로더 암에 포지션을 체크함. <-- 이 함수를 이용하여 포고 얼라인 시 로더 암이 들어와 있을 경우 align을 진행하지 않게 처리 함.
                EventCodeEnum isShutterCloseRetVal = EventCodeEnum.UNDEFINED;
                isShutterCloseRetVal = this.LoaderController().IsShutterClose();
                if (isShutterCloseRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"GPCardAligner.AlignPogo() The stage door is open and the loader arm is inside the stage, so the alignment cannot proceed.");
                    retVal = false;
                    return retVal;
                }

                this.StageSupervisor().StageModuleState.CCZCLEARED();

                //==> 설계상의 Pogo Pattern 위치
                List<CatCoordinates> designPosList = new List<CatCoordinates>(CardChangeSysParam.GP_PogoMarkPosList);
                //==> 설계상의 Pogo Pattern 위치
                List<CatCoordinates> searchedPosList = new List<CatCoordinates>(CardChangeSysParam.GP_SearchedPogoMarkPosList);

                //==> desing pos로 이동한 다음 Pattern Match를 하고 실제 위치를 searched pos에 저장을 한다.
                this.VisionManager().StopGrab(EnumProberCam.WAFER_LOW_CAM);
                System.Threading.Thread.Sleep(100);
                
                this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);
                System.Threading.Thread.Sleep(100);

                List<CatCoordinates> patternPosList = GetPatternPosList(EnumProberCam.PIN_LOW_CAM, searchedPosList);
                if (designPosList.Count != 4 || patternPosList.Count != 4 || designPosList.Count != patternPosList.Count)
                {
                    LoggerManager.Debug($"GPCardAligner.AlignPogo() designPosList Count:{designPosList.Count}, searchedPosList Count:{searchedPosList.Count}, Mode {CardChangeSysParam.PogoAlignPoint.Value}");
                    retVal = false;
                    return retVal;
                }

                //==> 패턴 매칭 해서 찾은 포지션을 저장해서 다음  Alignn 에서 사용
                for (int i = 0; i < designPosList.Count; ++i)
                {
                    //==> Pattern을 찾은 위치를 저장
                    PinCoordinate searchedPogoMarkPos = CardChangeSysParam.GP_SearchedPogoMarkPosList[i];
                    CatCoordinates patternPos = patternPosList[i];

                    searchedPogoMarkPos.X.Value = patternPos.X.Value;
                    searchedPogoMarkPos.Y.Value = patternPos.Y.Value;
                    searchedPogoMarkPos.Z.Value = patternPos.Z.Value;
                    pogoAvgZ += searchedPogoMarkPos.Z.Value;
                }
                pogoAvgZ = pogoAvgZ / designPosList.Count;
                //==> design pos 와 pattern match pos를 이용하여 X, Y, Theta 편차를 얻어옴

                LoggerManager.Debug($"PatternPosList Count: {patternPosList.Count}");

                // 4점의 center 찾기 (4POINT)
                GetCorrectionCoordinate(designPosList, patternPosList, out centerDiffX, out centerDiffY, out degreeDiff);

                CardChangeDevParam.GP_PogoCenterDiffX = centerDiffX;
                CardChangeDevParam.GP_PogoCenterDiffY = centerDiffY;
                CardChangeDevParam.GP_PogoDegreeDiff = degreeDiff;

                CardChangeSysParam.GP_PogoCenter.X.Value = centerDiffX;
                CardChangeSysParam.GP_PogoCenter.Y.Value = centerDiffY;
                CardChangeSysParam.GP_PogoCenter.T.Value = degreeDiff;
                LoggerManager.Debug($"CardChangeSysParam.GP_PogoCenter X = {CardChangeSysParam.GP_PogoCenter.X.Value:0.000}, Y = {CardChangeSysParam.GP_PogoCenter.Y.Value:0.000}, T = {CardChangeSysParam.GP_PogoCenter.T.Value:0.000}");

                EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();
                this.CardChangeModule().SaveDevParameter();
                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save Pogo Align Result Fail");
                }

                String pogoAlignResultStr = $"GPCC Pogo Diff ... : X,Y,T Pos : {centerDiffX}, {centerDiffY}, {degreeDiff}";

                LoggerManager.Debug(pogoAlignResultStr);

                double radiusDist = Math.Sqrt(Math.Pow(centerDiffX, 2) + Math.Pow(centerDiffY, 2));
                if (radiusDist > this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value)
                {
                    LoggerManager.Error($"PogoAlign Failed. PogoAlignRadiusLimit: {this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value}, Radius:{radiusDist:0.00}");
                    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retVal = false;
                    return retVal;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(_GPCardAlignLogPath, append: true))
                    {
                        sw.WriteLine(pogoAlignResultStr);
                    }

                    var result = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (result != EventCodeEnum.NONE)
                    {
                        retVal = false;
                        return retVal;
                    }
                }

                UpdateCardContactPosZ();

                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool AlignPogo3P(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double pogoAvgZ)
        {
            centerDiffX = 0;
            centerDiffY = 0;
            degreeDiff = 0;
            pogoAvgZ = 0;

            bool retVal = false;
            try
            {
                EventCodeEnum isShutterCloseRetVal = EventCodeEnum.UNDEFINED;
                isShutterCloseRetVal = this.LoaderController().IsShutterClose();
                if (isShutterCloseRetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"GPCardAligner.AlignPogo() The stage door is open and the loader arm is inside the stage, so the alignment cannot proceed.");
                    retVal = false;
                    return retVal;
                }

                this.StageSupervisor().StageModuleState.CCZCLEARED();

                //==> 설계상의 Pogo Pattern 위치
                List<CatCoordinates> designPosList = new List<CatCoordinates>(CardChangeSysParam.GP_PogoMarkPosList3P);
                //==> 설계상의 Pogo Pattern 위치
                List<CatCoordinates> searchedPosList = new List<CatCoordinates>(CardChangeSysParam.GP_SearchedPogoMarkPosList3P);

                //==> desing pos로 이동한 다음 Pattern Match를 하고 실제 위치를 searched pos에 저장을 한다.
                this.VisionManager().StopGrab(EnumProberCam.WAFER_LOW_CAM);
                System.Threading.Thread.Sleep(100);

                this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);
                System.Threading.Thread.Sleep(100);

                List<CatCoordinates> patternPosList = GetPatternPosList(EnumProberCam.PIN_LOW_CAM, searchedPosList);
                if (designPosList.Count != 3 || patternPosList.Count != 3 || designPosList.Count != patternPosList.Count)
                {
                    LoggerManager.Debug($"GPCardAligner.AlignPogo3P() designPosList Count:{designPosList.Count}, searchedPosList Count:{searchedPosList.Count}, Mode {CardChangeSysParam.PogoAlignPoint.Value}");
                    retVal = false;
                    return retVal;
                }

                //==> 패턴 매칭 해서 찾은 포지션을 저장해서 다음  Alignn 에서 사용
                for (int i = 0; i < designPosList.Count; ++i)
                {
                    //==> Pattern을 찾은 위치를 저장
                    PinCoordinate searchedPogoMarkPos = CardChangeSysParam.GP_SearchedPogoMarkPosList3P[i];
                    CatCoordinates patternPos = patternPosList[i];

                    searchedPogoMarkPos.X.Value = patternPos.X.Value;
                    searchedPogoMarkPos.Y.Value = patternPos.Y.Value;
                    searchedPogoMarkPos.Z.Value = patternPos.Z.Value;
                    pogoAvgZ += searchedPogoMarkPos.Z.Value;
                }
                pogoAvgZ = pogoAvgZ / designPosList.Count;
                //==> design pos 와 pattern match pos를 이용하여 X, Y, Theta 편차를 얻어옴

                LoggerManager.Debug($"PatternPosList Count: {patternPosList.Count}");

                // 원의 3점으로 센터찾기 (3POINT)
                CalPogoCenter(patternPosList, out centerDiffX, out centerDiffY);

                CardChangeDevParam.GP_PogoCenterDiffX = centerDiffX;
                CardChangeDevParam.GP_PogoCenterDiffY = centerDiffY;
                CardChangeDevParam.GP_PogoDegreeDiff = degreeDiff;

                CardChangeSysParam.GP_PogoCenter.X.Value = centerDiffX;
                CardChangeSysParam.GP_PogoCenter.Y.Value = centerDiffY;
                CardChangeSysParam.GP_PogoCenter.T.Value = degreeDiff;
                LoggerManager.Debug($"CardChangeSysParam.GP_PogoCenter X = {CardChangeSysParam.GP_PogoCenter.X.Value:0.000}, Y = {CardChangeSysParam.GP_PogoCenter.Y.Value:0.000}, T = {CardChangeSysParam.GP_PogoCenter.T.Value:0.000}", isInfo:true);

                EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();
                this.CardChangeModule().SaveDevParameter();
                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save Pogo Align Result Fail");
                }

                String pogoAlignResultStr = $"GPCC Pogo Diff ... : X,Y,T Pos : {centerDiffX}, {centerDiffY}, {degreeDiff}";

                LoggerManager.Debug(pogoAlignResultStr, isInfo: true);

                double radiusDist = Math.Sqrt(Math.Pow(centerDiffX, 2) + Math.Pow(centerDiffY, 2));
                if (radiusDist > this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value)
                {
                    LoggerManager.Error($"PogoAlign Failed. PogoAlignRadiusLimit: {this.CoordinateManager().StageCoord.ProbingSWRadiusLimit.Value}, Radius:{radiusDist:0.00}");
                    var ret = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retVal = false;
                    return retVal;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(_GPCardAlignLogPath, append: true))
                    {
                        sw.WriteLine(pogoAlignResultStr);
                    }

                    var result = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (result != EventCodeEnum.NONE)
                    {
                        retVal = false;
                        return retVal;
                    }
                }

                UpdateCardContactPosZ();

                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum UpdateCardContactPosZ()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            double cardAvgZ = 0;
            double pogoAvgZ = 0;

            List<CatCoordinates> searchedCardMarkPosList = new List<CatCoordinates>(CardChangeSysParam.GP_SearchedCardMarkPosList);
            List<CatCoordinates> searchedPogoMarkPosList = new List<CatCoordinates>(CardChangeSysParam.GP_SearchedPogoMarkPosList);
            List<CatCoordinates> searchedPogoMarkPosList3P = new List<CatCoordinates>(CardChangeSysParam.GP_SearchedPogoMarkPosList3P);

            try
            {
                // CardAlign Z value Result
                if (CardChangeSysParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                {
                    if (CardChangeSysParam.CardTopFromChuckPlane == null)
                    {
                        CardChangeSysParam.CardTopFromChuckPlane = new Element<double>();
                        CardChangeSysParam.CardTopFromChuckPlane.Value = 30000;
                    }
                    if (CardChangeSysParam.CardTopFromChuckPlane.Value == 0)
                    {
                        CardChangeSysParam.CardTopFromChuckPlane.Value = 30000;
                    }
                    cardAvgZ = CardChangeSysParam.CardTopFromChuckPlane.Value;
                }
                else
                {
                    for (int i = 0; i < searchedCardMarkPosList.Count; ++i)
                    {
                        cardAvgZ += searchedCardMarkPosList[i].Z.Value;
                    }
                    cardAvgZ = cardAvgZ / searchedCardMarkPosList.Count;
                }

                // PogoAlign Z value Result
                if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                {
                    for (int i = 0; i < searchedPogoMarkPosList.Count; ++i)
                    {
                        pogoAvgZ += searchedPogoMarkPosList[i].Z.Value;
                    }
                    pogoAvgZ = pogoAvgZ / CardChangeSysParam.GP_SearchedPogoMarkPosList.Count;
                }
                else if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                {
                    for (int i = 0; i < searchedPogoMarkPosList3P.Count; ++i)
                    {
                        pogoAvgZ += searchedPogoMarkPosList3P[i].Z.Value;
                    }
                    pogoAvgZ = pogoAvgZ / CardChangeSysParam.GP_SearchedPogoMarkPosList3P.Count;
                }

                LoggerManager.Debug($"UpdateCardContactPosZ() cardAvgZ : {cardAvgZ}, pogoAvgZ : {pogoAvgZ}");

                if (cardAvgZ == 0 || pogoAvgZ == 0)
                {
                    return retVal;
                }

                WaferCoordinate wafer = new WaferCoordinate(0, 0, cardAvgZ, 0);
                PinCoordinate pin = new PinCoordinate(0, 0, pogoAvgZ, 0);

                var matchedPos = this.CoordinateManager().WaferHighChuckConvert.GetWaferPinAlignedPosition(wafer, pin);
                CardChangeDevParam.GP_CardContactPosZ = matchedPos.GetZ();
                CardChangeSysParam.GP_Undock_CardContactPosZ = matchedPos.GetZ();

                retVal = this.CardChangeModule().SaveSysParameter();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Save Align Contact Position fail");
                    retVal = EventCodeEnum.GP_CardChange_CARD_CHANGE_SAVE_PARAM_ERROR;
                    return retVal;
                }
                retVal = this.CardChangeModule().SaveDevParameter();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Save Align Contact Position fail");
                    retVal = EventCodeEnum.GP_CardChange_CARD_CHANGE_SAVE_PARAM_ERROR;
                    return retVal;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private List<CatCoordinates> GetPattPositionsWithPLCam(List<CatCoordinates> searchPosList)
        {
            List<CatCoordinates> patternPosList = new List<CatCoordinates>();
            CatCoordinates patternPos = null;
            var proberCam = EnumProberCam.PIN_LOW_CAM;
            try
            {
                var filelist = Directory.GetFiles(PatternPath);
                for (int i = 0; i < searchPosList.Count; i++)
                {
                    CatCoordinates searchPos = searchPosList[i];
                    List<string> pathlist = new List<string>();
                    pathlist.Clear();
                    foreach (var file in filelist)
                    {
                        if (file.Contains($"PT{i}_"))
                        {
                            pathlist.Add(file);
                        }
                    }
                    if (FindPatternPosition(searchPos, proberCam, pathlist, out patternPos) == false)
                    {
                        LoggerManager.Debug($"Pattern mathcing failed");
                        break;
                    }
                    else
                    {
                        patternPosList.Add(patternPos);
                        LoggerManager.Debug($"Find pattern sucess. pattern Index:{i} pattern pos X:{patternPos.X.Value} Y:{patternPos.Y.Value} Z:{patternPos.Z.Value}");
                        //break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return patternPosList;
        }

        //==> searchPosList 에 있는 위치를 하나씩 이동 하면서 Pattern Match를 하고 실제 위치를 얻어온다.
        private List<CatCoordinates> SearchCardPattPositions(List<CatCoordinates> searchPosList, string alignPath)
        {
            List<CatCoordinates> patternPosList = new List<CatCoordinates>();
            EnumProberCam proberCam = EnumProberCam.PIN_LOW_CAM;
            try
            {
                //var ret = DownLoadCardImageFromLoader();
                //if (ret != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"Error occurd while Downlaod card image from loader in GetPatternPosList func");
                //}
                
                //var filelist = Directory.GetFiles(alignPath);

                /// selly 상위에서 Directory.CreateDirectory(alignPath);해준다. 
                DirectoryInfo directoryInfo = new DirectoryInfo(alignPath); 
                FileInfo[] files = directoryInfo.GetFiles();

                // 파일들을 수정한 날짜 기준으로 내림차순 정렬
                files = files.OrderByDescending(f => f.LastWriteTime).ToArray();
                string[] itemlist = files.Select(f => f.FullName).ToArray();
               
                for (int i = 0; i < searchPosList.Count; i++)
                {
                    CatCoordinates searchPos = searchPosList[i];
                    CatCoordinates patternPos;
                    List<string> pathlist = new List<string>();
                    //pathlist.Clear();
                    foreach (var file in itemlist)
                    {
                        if (file.Contains($"PT{i}_"))
                        {
                            pathlist.Add(file);
                            LoggerManager.Debug($"Add Pattern Path List: " + file);
                        }
                    }

                    if (FindPatternPosition(searchPos, proberCam, pathlist, out patternPos) == false)
                    {
                        LoggerManager.Debug($"Pattern matching failed");
                        break;
                    }
                    else
                    {
                        patternPosList.Add(patternPos);
                        LoggerManager.Debug($"Find pattern sucess. pattern Index:{i} pattern pos X:{patternPos.X.Value} Y:{patternPos.Y.Value} Z:{patternPos.Z.Value}");
                        //break;
                    }
                }

                // 피디셜마크 얼라인 후 핀로우 조명 끄기!
                var cam = this.VisionManager().GetCam(proberCam);
                cam.SetLight(EnumLightType.COAXIAL, 0);
                cam.SetLight(EnumLightType.OBLIQUE, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return patternPosList;
        }
        //==> searchPosList 에 있는 위치를 하나씩 이동 하면서 Pattern Match를 하고 실제 위치를 얻어온다.
        private List<CatCoordinates> GetPatternPosList(EnumProberCam proberCam, List<CatCoordinates> searchPosList)
        {
            List<CatCoordinates> patternPosList = new List<CatCoordinates>();
            string patternPath = null;
            try
            {
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {

                    //Download Device card image from loader
                    var ret = DownLoadCardImageFromLoader(false, null);
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"Error occurd while Downlaod card image from loader in GetPatternPosList func");
                    }

                    var filelist = Directory.GetFiles(PatternPath);

                    for (int i = 0; i < searchPosList.Count; i++)
                    {
                        CatCoordinates searchPos = searchPosList[i];
                        CatCoordinates patternPos;
                        List<string> pathlist = new List<string>();
                        pathlist.Clear();
                        foreach (var file in filelist)
                        {
                            if (file.Contains($"PT{i}_"))
                            {
                                pathlist.Add(file);
                            }
                        }

                        if (FindPatternPosition(searchPos, proberCam, pathlist, out patternPos) == false)
                        {
                            LoggerManager.Debug($"Pattern mathcing fail");
                            break;
                        }
                        else
                        {
                            patternPosList.Add(patternPos);
                            LoggerManager.Debug($"Find pattern sucess. pattern Index:{i} pattern pos X:{patternPos.X.Value} Y:{patternPos.Y.Value} Z:{patternPos.Z.Value}");
                            //break;
                        }

                    }
                }
                else // pogo
                {
                    if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                    {
                        patternPath = $"{PogoPatternPath}";
                    }
                    else if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                    {
                        patternPath = $"{PogoPatternPath3P}";
                    }
                    else
                    {
                        LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                    }
                    for (int i = 0; i < searchPosList.Count; i++)
                    {
                        CatCoordinates searchPos = searchPosList[i];
                        CatCoordinates patternPos;
                        if (FindPatternPosition(searchPos, proberCam, $"{patternPath}_{proberCam}_{i}", out patternPos)
                            == false)
                        {
                            break;
                        }

                        patternPosList.Add(patternPos);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return patternPosList;
        }
        //==> Pattern Match를 이용하여 Pattern을 찾고 위치를 얻어온다. Pattern Match를 실패하면 달팽이 수열 검색 방식으로 Pattern Match를 수행하여 찾으려고 노력함.
        private bool FindPatternPosition(CatCoordinates searchBasePos, EnumProberCam proberCam, String patternPath, out CatCoordinates patternPos)
        {
            patternPos = null;
            bool isFind = false;
            try
            {
                float xJumDistance = 0;
                float yJumDistance = 0;
                EnumCCAlignModule targetModule = EnumCCAlignModule.CARD;
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    xJumDistance = 2500.0f;
                    yJumDistance = 2500.0f;
                    targetModule = EnumCCAlignModule.CARD;
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    xJumDistance = 2000.0f;
                    yJumDistance = 2000.0f;
                    targetModule = EnumCCAlignModule.POGO;
                }


                foreach (PatternRelPoint relPos in _PatternRelPointList9)
                {
                    CatCoordinates searchPos = new CatCoordinates(searchBasePos.X.Value, searchBasePos.Y.Value, searchBasePos.Z.Value);
                    searchPos.X.Value += (xJumDistance * relPos.X);
                    searchPos.Y.Value += (yJumDistance * relPos.Y);

                    if (CamAbsMove(proberCam, searchPos) == false)
                    {
                        break;
                    }


                    CatCoordinates patternMatchPos;
                    if (GetPatternMatchingPosition(proberCam, patternPath, targetModule, out patternMatchPos))
                    {
                        patternPos = new CatCoordinates();
                        patternPos.X.Value = patternMatchPos.X.Value;
                        patternPos.Y.Value = patternMatchPos.Y.Value;
                        patternPos.Z.Value = patternMatchPos.Z.Value;
                        isFind = true;
                        LoggerManager.Debug($"Success pattern matching. " +
                            $"Pattern:{patternPath} " +
                            $"Pattern Pos X:{patternMatchPos.X.Value}" +
                            $"Pattern Pos Y:{patternMatchPos.Y.Value}" +
                            $"Pattern Pos Y:{patternMatchPos.Z.Value}");
                        CamAbsMove(proberCam, patternMatchPos);
                        break;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isFind;
        }

        private bool FindPatternPosition(CatCoordinates searchBasePos, EnumProberCam proberCam, List<string> patternPaths, out CatCoordinates patternPos)
        {
            patternPos = null;
            bool isFind = false;

            float xJumDistance = 0;
            float yJumDistance = 0;
            try
            {
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    xJumDistance = 2100.0f;
                    yJumDistance = 2100.0f;
                }
                else
                {
                    xJumDistance = 2100.0f;
                    yJumDistance = 2100.0f;
                }
                bool focused = false;


                List<PatternRelPoint> PatternRelPoint = new List<PatternRelPoint>();
                int oddNum = CardChangeSysParam.SnailPointsOdd;
                if (oddNum == 3)
                {
                    PatternRelPoint = _PatternRelPointList9;
                }
                else if (oddNum == 5)
                {
                    PatternRelPoint = _PatternRelPointList25;
                }


                foreach (PatternRelPoint relPos in PatternRelPoint)
                {
                    CatCoordinates searchPos = new CatCoordinates(searchBasePos.X.Value, searchBasePos.Y.Value, searchBasePos.Z.Value);
                    searchPos.X.Value += (xJumDistance * relPos.X);
                    searchPos.Y.Value += (yJumDistance * relPos.Y);
                    if (CamAbsMove(proberCam, searchPos) == false)
                    {
                        break;
                    }

                    if (proberCam == EnumProberCam.PIN_LOW_CAM & focused == false)
                    {
                        LoggerManager.Debug($"before focusing in CardAlign curpos X:{this.MotionManager().GetAxis(EnumAxisConstants.X).Status.RawPosition.Actual}" +
                            $" curpos Y:{this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.RawPosition.Actual} " +
                            $" curpos Z:{this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.RawPosition.Actual}");
                        FocusParam.DepthOfField.Value = 5.0;
                        FocusParam.FocusMaxStep.Value = 10;
                        //PLFocusParam.FocusRange.Value = 1500;
                        var ret = CardFocusModel.Focusing_Retry(PLFocusParam, false, false, false, this);

                        //첫번째 포인트 외 나머지 포인트에서도 해당 Z값에서 패턴매칭할 수 있게 포커싱 되있는 Z값으로 변경
                        PinCoordinate CurPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                        searchBasePos.Z.Value = CurPos.Z.Value;

                        focused = true;
                    }

                    CatCoordinates patternMatchPos;
                    string sucessfilename = string.Empty;
                    bool success = false;
                    if (this.VisionManager().VisionProcessing.IsValidModLicense() == false)
                    {
                        LoggerManager.Debug($"FindPatternPosition(): Model Finder License Is Invalid.");
                        success = GetPatternMatchingPosition(proberCam, patternPaths, out patternMatchPos, out string sucessfile);
                        if (success)
                            sucessfilename = sucessfile;
                    }
                    else
                    {
                        success = GetModelPosition(proberCam, out patternMatchPos);
                        if (!success)
                        {
                            success = GetPatternMatchingPosition(proberCam, patternPaths, out patternMatchPos, out string sucessfile);
                            if (success)
                                sucessfilename = sucessfile;
                        }
                    }
                    if (success)
                    {
                        patternPos = new CatCoordinates();
                        patternPos.X.Value = patternMatchPos.X.Value;
                        patternPos.Y.Value = patternMatchPos.Y.Value;
                        patternPos.Z.Value = patternMatchPos.Z.Value;
                        isFind = true;
                        LoggerManager.Debug($"Success pattern matching. " +
                            $" Pattern file name:{sucessfilename}" +
                            $" Pattern Pos X:{patternMatchPos.X.Value}" +
                            $" Pattern Pos Y:{patternMatchPos.Y.Value}" +
                            $" Pattern Pos Y:{patternMatchPos.Z.Value}");
                        CamAbsMove(proberCam, patternMatchPos);
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return isFind;
        }
        private EventCodeEnum DownLoadCardImageFromLoader(bool FMPattern, string cardid)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var cardimages = this.LoaderController().DownloadCardPatternImages(this.FileManager().GetDeviceName(), CardChangeSysParam.PatternMatchingRetryCount, cardid);

                for (int i = 0; i < cardimages.Count; i++)
                {
                    if (FMPattern)
                    {
                        File.WriteAllBytes(FMPatternPath + $"//{cardid}" + $"//{cardimages[i].ImgFileName}", cardimages[i].ImageByte);
                    }
                    else
                    {
                        File.WriteAllBytes(PatternPath + $"//{cardimages[i].ImgFileName}", cardimages[i].ImageByte);
                    }

                    LoggerManager.Debug($"DowonLoadCardImageFromLoader func filename {cardimages[i].ImgFileName}");
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Exception occured while DownLoadCardImageFromLoader func {err.Message}");
            }
            return ret;
        }

        private ProberCardListParameter DownLoadProbeCardInfoFromLoader(string CardID)
        {
            ProberCardListParameter ret = null;
            try
            {
                ret = this.LoaderController().DownloadProbeCardInfo(CardID);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Exception occured while DownLoadProbeCardInfoFromLoader func {err.Message}");
            }
            return ret;
        }

        //==> 홀수 방식의 달팽이 수열 순서로 Pattern 상대 위치를 얻어옴
        /* 
         * 7(-1,1)   8(0,1)   9(1,1)
         * 6(-1,0)   1(0,0)   2(1,0)
         * 5(-1,-1)  4(0,-1)  3(1,-1)
         */
        private List<PatternRelPoint> GetSnailPointsOdd(int snailWidht)
        {
            List<PatternRelPoint> patternRelPointList = new List<PatternRelPoint>();

            try
            {
                if (snailWidht % 2 == 0)
                {
                    return patternRelPointList;
                }

                int maxLayer = Convert.ToInt32(snailWidht / 2);

                int curLayer = 0;//==> Layer는 0 부터 시작 한다, (0,0)
                int xWalker = 0;
                int yWalker = 0;
                int xDirection = 1;
                int yDirection = -1;
                bool xyDirectionFlag = true;//==> true : x, false : y, 처음엔 X 방향으로 이동 한다.

                patternRelPointList.Add(new PatternRelPoint(xWalker, yWalker));
                while (true)
                {
                    if (xWalker == curLayer && yWalker == curLayer)
                    {
                        if (curLayer == maxLayer)
                        {
                            break;
                        }
                        else
                        {
                            //==> 다시 오른쪽으로 가기 위한 Reset
                            xyDirectionFlag = true;
                            xDirection = 1;
                            yDirection = -1;

                            curLayer++;
                        }
                    }

                    if (xyDirectionFlag)
                    {
                        xWalker += xDirection;

                        if (Math.Abs(xWalker) >= curLayer)
                        {
                            xyDirectionFlag = false;
                            xDirection *= -1;
                        }
                    }
                    else
                    {
                        yWalker += yDirection;

                        if (Math.Abs(yWalker) >= curLayer)
                        {
                            xyDirectionFlag = true;
                            yDirection *= -1;
                        }
                    }

                    patternRelPointList.Add(new PatternRelPoint(xWalker, yWalker));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return patternRelPointList;
        }
        //==> 짝수 방식의 달팽이 수열 순서로 Pattern 상대 위치를 얻어옴
        /* 
         * 7(-1,1)     8(0,1)     9(1,1)    9(2,1)
         * 6(-1,0)     1(0,0)     2(1,0)    10(2,0)
         * 5(-1,-1)    4(0,-1)    3(1,-1)   11(2,-1)
         * 15(-1,-2)   14(0,-2)   13(1,-2)  12(2,-2)
         */
        private List<PatternRelPoint> GetSnailPointsEven(int snailWidht)
        {
            List<PatternRelPoint> patternRelPointList = new List<PatternRelPoint>();

            try
            {
                if (snailWidht % 1 == 1)
                {
                    return patternRelPointList;
                }

                int maxLayer = Convert.ToInt32(snailWidht / 2);
                int curLayer = 1;//==> Layer는 0 부터 시작 한다, (0,0)
                int xWalker = 0;
                int yWalker = 0;
                int xDirection = -1;
                int yDirection = 1;
                bool xyDirectionFlag = true;//==> true : x, false : y, 처음엔 X 방향으로 이동 한다.

                patternRelPointList.Add(new PatternRelPoint(xWalker, yWalker));
                while (true)
                {
                    if (xWalker == curLayer - 1 && yWalker == curLayer)
                    {
                        if (curLayer == maxLayer)
                        {
                            break;
                        }
                        else
                        {
                            //==> 다시 오른쪽으로 가기 위한 Reset
                            xyDirectionFlag = true;
                            xDirection = 1;
                            yDirection = -1;

                            curLayer++;
                        }
                    }

                    if (xyDirectionFlag)
                    {
                        xWalker += xDirection;

                        if (xWalker >= curLayer - 1 || xWalker <= (curLayer * -1))
                        {
                            xyDirectionFlag = false;
                            xDirection *= -1;
                        }
                    }
                    else
                    {
                        yWalker += yDirection;

                        if (yWalker <= ((curLayer - 1) * -1) || yWalker >= curLayer)
                        {
                            xyDirectionFlag = true;
                            yDirection *= -1;
                        }
                    }

                    patternRelPointList.Add(new PatternRelPoint(xWalker + 0.5f, yWalker + 0.5f));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return patternRelPointList;
        }
        private ushort GetLightValueStr(string fileName, string lightName)
        {
            ushort value = 0;
            string LightStr;
            try
            {
                if (fileName.Contains(lightName))
                {
                    var temp = fileName.Substring(fileName.IndexOf(lightName) + lightName.Length);
                    if (temp.Contains("_"))
                    {
                        LightStr = temp.Substring(0, temp.IndexOf("_"));
                    }
                    else
                    {
                        LightStr = temp.ToString();
                    }
                    ushort.TryParse(LightStr, out value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return value;
        }
        #region ==> %%% Pattern %%%
        //==> Pattern 등록
        public EventCodeEnum RegisterPattern(EnumProberCam proberCam, int index, EnumCCAlignModule module, ProberCardListParameter probeCard = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double patternWidth = 0;
                double patternHeight = 0;
                string patternPath = null;
                string filenamefromhash = null;
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    if (module == EnumCCAlignModule.CARD)
                    {
                        patternWidth = CardChangeDevParam.GP_CardPatternWidth;
                        patternHeight = CardChangeDevParam.GP_CardPatternHeight;
                    }
                    else if (module == EnumCCAlignModule.POD)
                    {
                        patternWidth = CardChangeDevParam.CardPodPatternWidth;
                        patternHeight = CardChangeDevParam.CardPodPatternHeight;
                    }
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    if (module == EnumCCAlignModule.CARD)
                    {
                        patternWidth = CardChangeDevParam.GP_CardPatternWidth;
                        patternHeight = CardChangeDevParam.GP_CardPatternHeight;
                    }
                    else if (module == EnumCCAlignModule.POGO)
                    {
                        patternWidth = CardChangeDevParam.GP_PogoPatternWidth;
                        patternHeight = CardChangeDevParam.GP_PogoPatternHeight;
                    }
                }

                this.VisionManager().StopGrab(proberCam);

                ImageBuffer img = null;
                ICamera cam = this.VisionManager().GetCam(proberCam);
                cam.GetCurImage(out img);

                //==> Pattern 등록
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    if (module == EnumCCAlignModule.CARD)
                    {
                        filenamefromhash = $"PT{index}_ID{this.CardChangeModule().GetProbeCardID()}_{img.GetHashCode()}_COA{cam.GetLight(EnumLightType.COAXIAL)}_OBL{cam.GetLight(EnumLightType.OBLIQUE)}_";
                        patternPath = $"{PatternPath}\\{filenamefromhash}";
                    }
                    else if (module == EnumCCAlignModule.POD)
                    {
                        filenamefromhash = $"PT{index}_POD_COA{cam.GetLight(EnumLightType.COAXIAL)}_OBL{cam.GetLight(EnumLightType.OBLIQUE)}_";
                        patternPath = $"{PodPattPath}\\{filenamefromhash}";
                    }
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    if (module == EnumCCAlignModule.CARD)
                    {
                        filenamefromhash = $"PT{index}_ID{this.CardChangeModule().GetProbeCardID()}_{img.GetHashCode()}_COA{cam.GetLight(EnumLightType.COAXIAL)}_OBL{cam.GetLight(EnumLightType.OBLIQUE)}_";
                        patternPath = $"{FMPatternPath}\\{probeCard.CardID}\\PT{index}_{proberCam}_COA{cam.GetLight(EnumLightType.COAXIAL)}_OBL{cam.GetLight(EnumLightType.OBLIQUE)}";
                    }
                    else
                    {
                        if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                        {
                            patternPath = $"{PogoPatternPath}_{proberCam}_{index}";
                        }
                        else if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                        {
                            patternPath = $"{PogoPatternPath3P}_{proberCam}_{index}";
                        }
                        else
                        {
                            LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                        }
                    }

                }
                else
                {
                    //Error
                }

                RegisteImageBufferParam rparam = new RegisteImageBufferParam(
                  proberCam,
                  (int)((cam.GetGrabSizeWidth() / 2) - (patternWidth / 2)),
                  (int)((cam.GetGrabSizeHeight() / 2) - (patternHeight / 2)),
                  (int)patternWidth,
                  (int)patternHeight,
                  patternPath);

                rparam.ImageBuffer = img;
                retVal = this.VisionManager().SavePattern(rparam);

                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    if (module == EnumCCAlignModule.CARD)
                    {
                        byte[] cardimges = new byte[0];
                        if (File.Exists(patternPath + ".bmp"))
                        {
                            cardimges = File.ReadAllBytes(patternPath + ".bmp");
                            this.LoaderController().UploadCardPatternImages(cardimges, filenamefromhash, this.FileManager().GetDeviceName(), null);
                        }
                        else
                        {
                            LoggerManager.Error($"Not Exist Pattern CardPattern filename:{filenamefromhash}");
                        }
                    }
                    else if (module == EnumCCAlignModule.POD)
                    {
                        // Do nothing. 카드 팟은 시스템 파라미터 이므로 로더로 올릴 필요 없음.
                    }
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM) // MD Fiducial Mark Align
                {
                    if (module == EnumCCAlignModule.CARD)
                    {
                        byte[] cardimges = new byte[0];
                        if (File.Exists(patternPath + ".bmp"))
                        {
                            cardimges = File.ReadAllBytes(patternPath + ".bmp");
                            this.LoaderController().UploadCardPatternImages(cardimges, filenamefromhash, this.FileManager().GetDeviceName(), probeCard.CardID);
                        }
                        else
                        {
                            LoggerManager.Error($"Not Exist Pattern CardPattern filename:{filenamefromhash}");
                        }
                    }
                }

                this.VisionManager().StartGrab(proberCam, this);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        //==> Pattern Match를 수행하여 Pattern 위치를 얻어옴.
        private bool GetPatternMatchingPosition(EnumProberCam proberCam, String patternPath, EnumCCAlignModule module, out CatCoordinates position)
        {
            position = null;
            double x = 0;
            double y = 0;
            double z = 0;


            //focusing 
            if (proberCam == EnumProberCam.WAFER_LOW_CAM)
            {
                LoggerManager.Debug($"before focusing in CardAlign curpos X:{this.MotionManager().GetAxis(EnumAxisConstants.X).Status.RawPosition.Actual}" +
                    $" curpos Y:{this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.RawPosition.Actual} " +
                    $" curpos Z:{this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.RawPosition.Actual}");

                var ret = CardFocusModel.Focusing_Retry(FocusParam, false, false, false, this);
            }

            this.VisionManager().StopGrab(proberCam);
            LoggerManager.Debug($"PatternMatching () Cam: {proberCam} , Path: {patternPath}");
            
            PMResult pmresult = this.VisionManager().PatternMatching(new PatternInfomation(proberCam, new PMParameter(75, 98), patternPath), this);

            System.Threading.Thread.Sleep(100);
            #region ==> Version 2
            //PatternInfomation patterninfo = new PatternInfomation();
            //patterninfo.CamType.Value = camType;
            //patterninfo.PMParameter.ModelFilePath.Value = filePath;
            //patterninfo.PMParameter.PatternFileExtension.Value = ".mmo";
            //PMResult pmresult = this.VisionManager().PatternMatching(patterninfo);
            #endregion

            this.VisionManager().StartGrab(proberCam, this);
            if (pmresult.ResultParam == null || pmresult.ResultParam.Count != 1)
            {
                //==> TODO : FOCUSING을 해서 다시 Pattern Matching을 시도 할 수 있다,
                //==> 이 과정에서 Z가 바뀔 수 있다.
                LoggerManager.Debug($"Not found pattern pmresult.ResultParam:{pmresult.ResultParam} pmresult.RetValue:{pmresult.RetValue}");
                return false;
            }

            double ptxpos = pmresult.ResultParam[0].XPoss;
            double ptypos = pmresult.ResultParam[0].YPoss;
            double offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
            double offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
            offsetx *= pmresult.ResultBuffer.RatioX.Value;
            offsety *= pmresult.ResultBuffer.RatioY.Value;

            if (proberCam == EnumProberCam.WAFER_LOW_CAM)
            {
                WaferCoordinate waferPos = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                x = waferPos.X.Value - offsetx;
                y = waferPos.Y.Value + offsety;
                z = waferPos.Z.Value;
            }
            else if (proberCam == EnumProberCam.PIN_LOW_CAM)
            {
                PinCoordinate pinPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                x = pinPos.X.Value - offsetx;
                y = pinPos.Y.Value + offsety;
                z = pinPos.Z.Value;
            }

            position = new CatCoordinates(x, y, z);

            return true;
        }

        private bool GetPatternMatchingPosition(EnumProberCam proberCam, List<string> patternPaths, out CatCoordinates position, out string sucessfile)
        {
            position = null;
            sucessfile = null;
            double x = 0;
            double y = 0;
            double z = 0;

            try
            {

                //focusing \
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    LoggerManager.Debug($"before focusing in CardAlign curpos X:{this.MotionManager().GetAxis(EnumAxisConstants.X).Status.RawPosition.Actual}" +
                        $" curpos Y:{this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.RawPosition.Actual} " +
                        $" curpos Z:{this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.RawPosition.Actual}");

                    FocusParam.DepthOfField.Value = 5.0;
                    FocusParam.FocusMaxStep.Value = 10;

                    var ret = CardFocusModel.Focusing_Retry(FocusParam, false, false, false, this);
                }

                this.VisionManager().StopGrab(proberCam);
                var cam = this.VisionManager().GetCam(proberCam);
                PMResult pmresult = null;

                if (patternPaths.Count == 0)
                {
                    LoggerManager.Debug($"Fail pattern matching. There is no pattern path.");
                    return false;
                }

                for (int i = 0; i < patternPaths.Count; i++)
                {
                    LoggerManager.Debug($"CardAlgin_SetLight Camtype:{proberCam} , FilePath:{patternPaths[i]}");

                    cam.SetLight(EnumLightType.COAXIAL, GetLightValueStr(patternPaths[i], "_COA"));
                    cam.SetLight(EnumLightType.OBLIQUE, GetLightValueStr(patternPaths[i], "_OBL"));
                    Task.Delay(300).Wait();

                    pmresult = this.VisionManager().PatternMatching(new PatternInfomation(proberCam, new PMParameter(75, 98), patternPaths[i]), this);

                    if (pmresult.RetValue == EventCodeEnum.NONE)
                    {
                        sucessfile = patternPaths[i];
                        bool zeroidx = sucessfile.Contains("0_");
                        bool oneidx = sucessfile.Contains("1_");
                        bool twoidx = sucessfile.Contains("2_");
                        bool threeidx = sucessfile.Contains("3_");
                        try
                        {
                            if (zeroidx)
                            {
                                if (File.Exists($"{PatternPath}\\0_.bmp") == false)
                                {
                                    File.Copy(sucessfile, $"{PatternPath}\\0_.bmp", true);
                                }
                            }
                            else if (oneidx)
                            {
                                if (File.Exists($"{PatternPath}\\1_.bmp") == false)
                                {
                                    File.Copy(sucessfile, $"{PatternPath}\\1_.bmp", true);
                                }
                            }
                            else if (twoidx)
                            {
                                if (File.Exists($"{PatternPath}\\2_.bmp") == false)
                                {
                                    File.Copy(sucessfile, $"{PatternPath}\\2_.bmp", true);
                                }
                            }
                            else if (threeidx)
                            {
                                if (File.Exists($"{PatternPath}\\3_.bmp") == false)
                                {
                                    File.Copy(sucessfile, $"{PatternPath}\\3_.bmp", true);
                                }
                            }

                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug($"{err.Message}");
                        }

                        break;
                    }
                    else
                    {
                        LoggerManager.Debug($"Fail pattern matching file {patternPaths[i]}");
                    }
                    LoggerManager.Debug($"CardAlign pattern matching retry count{i}. Pattern matching retry value  : {CardChangeSysParam.PatternMatchingRetryCount}");
                    if (i + 1 > CardChangeSysParam.PatternMatchingRetryCount)
                    {
                        break;
                    }
                }

                this.VisionManager().StartGrab(proberCam, this);
                Task.Delay(300).Wait();
                if (pmresult.ResultParam == null || pmresult.ResultParam.Count != 1)
                {
                    //==> TODO : FOCUSING을 해서 다시 Pattern Matching을 시도 할 수 있다,
                    //==> 이 과정에서 Z가 바뀔 수 있다.
                    LoggerManager.Debug($"Not found pattern pmresult.ResultParam:{pmresult.ResultParam} pmresult.RetValue:{pmresult.RetValue}");
                    return false;
                }

                double ptxpos = pmresult.ResultParam[0].XPoss;
                double ptypos = pmresult.ResultParam[0].YPoss;
                double offsetx = (pmresult.ResultBuffer.SizeX / 2) - ptxpos;
                double offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;
                offsetx *= pmresult.ResultBuffer.RatioX.Value;
                offsety *= pmresult.ResultBuffer.RatioY.Value;

                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    WaferCoordinate waferPos = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                    x = waferPos.X.Value - offsetx;
                    y = waferPos.Y.Value + offsety;
                    z = waferPos.Z.Value;
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    PinCoordinate pinPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                    x = pinPos.X.Value - offsetx;
                    y = pinPos.Y.Value + offsety;
                    z = pinPos.Z.Value;
                }

                position = new CatCoordinates(x, y, z);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }


            return true;
        }
        private bool GetModelPosition(EnumProberCam proberCam, out CatCoordinates position)
        {
            position = null;
            double x = 0;
            double y = 0;
            double z = 0;
            double ptxpos = 0d;
            double ptypos = 0d;
            double offsetx = 0d;
            double offsety = 0d;

            try
            {
                position = new CatCoordinates(this.MotionManager().GetAxis(EnumAxisConstants.X).Status.RawPosition.Actual, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.RawPosition.Actual, this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.RawPosition.Actual);

                // Focusing
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    LoggerManager.Debug($"before focusing in CardAlign curpos X:{this.MotionManager().GetAxis(EnumAxisConstants.X).Status.RawPosition.Actual}" +
                        $" curpos Y:{this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.RawPosition.Actual} " +
                        $" curpos Z:{this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.RawPosition.Actual}");

                    FocusParam.DepthOfField.Value = 5.0;
                    FocusParam.FocusMaxStep.Value = 10;

                    var ret = CardFocusModel.Focusing_Retry(FocusParam, false, false, false, this);
                }
                else
                {
                    // PinLowCam은 바깥에서 포커싱 하고 들어옴.
                }
                this.VisionManager().StopGrab(proberCam);
                var cam = this.VisionManager().GetCam(proberCam);

                List<ModelFinderResult> mfresult = null;
                List<MFParameter> mFParameters = CardChangeDevParam.ModelInfos;
                ImageBuffer grabBuffer = null;

                if (mFParameters != null)
                {
                    LoggerManager.Debug($"GetModelPosition(): get model params. Count:{mFParameters.Count}");

                    foreach (MFParameter mf in mFParameters)
                    {
                        if (mf.CamType.Value == proberCam)
                        {
                            LoggerManager.Debug($"GetModelPosition(): get model param. index:{mFParameters.IndexOf(mf)} ");

                            foreach (var light in mf.Lights)
                            {
                                cam.GetLight(light.Type.Value);
                                cam.SetLight(light.Type.Value, (ushort)light.ChannelMapIdx.Value);
                                LoggerManager.Debug($"ModelFinder SetLight {light.Type.Value}: {light.ChannelMapIdx.Value}");
                            }

                            if (grabBuffer == null)
                            {
                                grabBuffer = this.VisionManager().SingleGrab(cam.GetChannelType(), this);
                            }

                            var baseResults = this.VisionManager().VisionProcessing.ModelFind(
                                targetimg: grabBuffer,
                                targettype: mf.ModelTargetType.Value,
                                foreground: mf.ForegroundType.Value,
                                size: new Size(mf.ModelWidth.Value / cam.GetRatioX(), mf.ModelHeight.Value / cam.GetRatioY()),
                                acceptance: mf.Acceptance.Value,
                                scale_min: mf.ScaleMin.Value,
                                scale_max: mf.ScaleMax.Value,
                                smoothness: mf.Smoothness.Value,
                                number: mf.ExpectedOccurrence.Value);

                            if (baseResults.Count > 0)
                            {
                                LoggerManager.Debug($"GetModelPosition(): baseResults find {baseResults.Count} models. ModelTargetType:{mf.ModelTargetType.Value}");
                                if (mf.Child != null)
                                {
                                    List<ModelFinderResult> childResults = new List<ModelFinderResult>();
                                    for (int i = 0; i < baseResults.Count; i++)
                                    {
                                        foreach (var light in mf.Child.Lights)
                                        {
                                            cam.GetLight(light.Type.Value);
                                            cam.SetLight(light.Type.Value, (ushort)light.ChannelMapIdx.Value);
                                            LoggerManager.Debug($"ModelFinder_Child SetLight {light.Type.Value}: {light.ChannelMapIdx.Value}");
                                        }

                                        if (grabBuffer == null)
                                        {
                                            grabBuffer = this.VisionManager().SingleGrab(cam.GetChannelType(), this);
                                        }

                                        LoggerManager.Debug($"GetModelPosition(): baseResults({i}) - childResults Set roi x:{baseResults[i].Position.X.Value}, y:{baseResults[i].Position.Y.Value}, w:{mf.ModelWidth.Value}, h:{mf.ModelHeight.Value}");

                                        childResults = this.VisionManager().VisionProcessing.ModelFind(
                                        targetimg: grabBuffer,
                                        targettype: mf.Child.ModelTargetType.Value,
                                        foreground: mf.Child.ForegroundType.Value,
                                        size: new Size(mf.Child.ModelWidth.Value / cam.GetRatioX(), mf.Child.ModelHeight.Value / cam.GetRatioY()),
                                        acceptance: mf.Child.Acceptance.Value,
                                        posx: baseResults[i].Position.X.Value,
                                        posy: baseResults[i].Position.Y.Value,
                                        roiwidth: mf.ModelWidth.Value / cam.GetRatioX(),
                                        roiheight: mf.ModelHeight.Value / cam.GetRatioY(),
                                        scale_min: mf.Child.ScaleMin.Value,
                                        scale_max: mf.Child.ScaleMax.Value,
                                        smoothness: mf.Child.Smoothness.Value,
                                        number: mf.Child.ExpectedOccurrence.Value);

                                        if (childResults.Count > 0)
                                        {
                                            LoggerManager.Debug($"GetModelPosition(): childResults find {childResults.Count} models. ModelTargetType:{mf.Child.ModelTargetType.Value}");
                                            for (int j = 0; j < childResults.Count; j++)
                                            {
                                                double BaseCenX = baseResults[i].Position.X.Value; //+ (mf.ModelWidth.Value / Cam.GetRatioX()) / 2;
                                                double BaseCenY = baseResults[i].Position.Y.Value; //+ (mf.ModelHeight.Value / Cam.GetRatioY()) / 2;

                                                double ChildCenX = childResults.First().Position.X.Value;
                                                double ChildCenY = childResults.First().Position.Y.Value;
                                                double x2 = (BaseCenX - ChildCenX) * (BaseCenX - ChildCenX);
                                                double y2 = (BaseCenY - ChildCenY) * (BaseCenY - ChildCenY);
                                                double distance = Math.Sqrt(x2 + y2); // 중심 거리가 중앙에 있어야 잘 찾았다고 인식 

                                                if (distance < 5)
                                                {
                                                    mfresult = childResults;
                                                    ptxpos = childResults.First().Position.X.Value;
                                                    ptypos = childResults.First().Position.Y.Value;
                                                    LoggerManager.Debug($"GetModelPosition(): {mf.Child.ModelTargetType.Value} type model found @({childResults[j].Position.X.Value:0.00},{childResults[j].Position.Y.Value:0.00})on Base model #{i}");

                                                    offsetx = (grabBuffer.SizeX / 2) - ptxpos;
                                                    offsety = (grabBuffer.SizeY / 2) - ptypos;
                                                    LoggerManager.Debug($"GetModelPosition(): offsetx :{offsetx:0.00} pixel , offsety:{offsety:0.00} pixel on Base model #{i}");
                                                    this.VisionManager().SaveImageBuffer(mfresult.FirstOrDefault().ResultBuffer, this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, false, "\\MFImage\\ResultImage"), IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);

                                                    var mf0 = mf;
                                                    mFParameters.Remove(mf);
                                                    mFParameters.Insert(0, mf0);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // model 종류별로 이미지를 저장할 필요 없음. 그랩된 이미지기 때문에 모델 1,2,3에서 저장하는 이미지는 다 같은 이미지임.
                                            // 그렇기 때문에 첫번쨰 모델에서만 실패했을 경우 이미지를 저장한다.
                                            // fail 시 실패 이미지 무조건 저장하도록 수정.
                                            //if (mFParameters.IndexOf(mf) == 0)
                                            //{
                                                var path = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage");
                                                this.VisionManager().SaveImageBuffer(grabBuffer, path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                                                LoggerManager.Debug($"ModelFind({mf.Child.ModelTargetType}): Fail Model finder find 0 models. Saved Image Path: " + $"{path}");
                                            //}

                                            LoggerManager.Debug($"GetModelPosition(): Child Count {childResults.Count}. baseresults Index:{i}");
                                        }
                                    }
                                }
                                else
                                {
                                    mfresult = baseResults;
                                    ptxpos = baseResults.First().Position.X.Value;
                                    ptypos = baseResults.First().Position.Y.Value;

                                    offsetx = (grabBuffer.SizeX / 2) - ptxpos;
                                    offsety = (grabBuffer.SizeY / 2) - ptypos;
                                    this.VisionManager().SaveImageBuffer(mfresult.FirstOrDefault().ResultBuffer, this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, false, "\\MFImage\\ResultImage"), IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);

                                    // 여러 모델 정보들이 리스트의 혼재되어 있기 때문에 모델파인더로 성공한 현재 모델을 리스트 중 가장 첫번째로 옮겨 이후 부터는 바로 성공할 수 있게 한다.
                                    var mf0 = mf;
                                    mFParameters.Remove(mf);
                                    mFParameters.Insert(0, mf0);

                                    break;
                                }

                                if (mfresult == null)
                                {
                                    LoggerManager.Debug($"ModelFind() : Distance Fail Modelfinder find 0 models");
                                }
                            }

                            else
                            {
                                // model 종류별로 이미지를 저장할 필요 없음. 그랩된 이미지기 때문에 모델 1,2,3에서 저장하는 이미지는 다 같은 이미지임.
                                // 그렇기 때문에 첫번쨰 모델에서만 실패했을 경우 이미지를 저장한다.
                                // fail 시 실패 이미지 무조건 저장하도록 수정.
                                //if (mFParameters.IndexOf(mf) == 0)
                                //{
                                    var path = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage");
                                    this.VisionManager().SaveImageBuffer(grabBuffer, path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                                    LoggerManager.Debug($"ModelFind({mf.ModelTargetType}): Fail Model finder find 0 models. Saved Image Path: " + $"{path}");
                                //}

                                LoggerManager.Debug($"GetModelPosition(): Base Count {baseResults.Count}. ");
                            }

                            if (mfresult != null)
                            {
                                // Base찾고, Child에서도 찾았으면 나머지 Model로 돌릴 필요 없음.
                                break;
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"GetModelPosition(): Cam Type difference. Cur Cam: {proberCam.ToString()}, Parameter: {mf.CamType.Value.ToString()}");
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"GetModelPosition(): mFParameters is null");
                }

                this.VisionManager().StartGrab(proberCam, this);

                if (mfresult == null)
                {
                    //==> TODO : FOCUSING을 해서 다시 Pattern Matching을 시도 할 수 있다,
                    //==> 이 과정에서 Z가 바뀔 수 있다.                    

                    LoggerManager.Debug($"GetModelPosition(): Model Not Found.");
                    return false;
                }

                offsetx *= mfresult.FirstOrDefault().ResultBuffer.RatioX.Value;
                offsety *= mfresult.FirstOrDefault().ResultBuffer.RatioY.Value;
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    WaferCoordinate waferPos = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                    x = waferPos.X.Value - offsetx;
                    y = waferPos.Y.Value + offsety;
                    z = waferPos.Z.Value;
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    PinCoordinate pinPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                    x = pinPos.X.Value - offsetx;
                    y = pinPos.Y.Value + offsety;
                    z = pinPos.Z.Value;
                }

                position = new CatCoordinates(x, y, z);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }



            return true;
        }

        #endregion

        #region ==> %%% MATH %%%

        private void CalPogoCenter(List<CatCoordinates> searchedPositionList,
            out double centerX, out double centerY)
        {
            Point tmpPogoCen = new Point();

            double a = 0.0;
            double b = 0.0;
            double c = 0.0;
            double d = 0.0;
            double e = 0.0;
            double f = 0.0;

            try
            {
                a = 2 * (searchedPositionList[1].X.Value - searchedPositionList[0].X.Value);
                b = 2 * (searchedPositionList[1].Y.Value - searchedPositionList[0].Y.Value);
                c = Math.Pow(searchedPositionList[1].X.Value, 2.0) - Math.Pow(searchedPositionList[0].X.Value, 2.0) +
                    Math.Pow(searchedPositionList[1].Y.Value, 2.0) - Math.Pow(searchedPositionList[0].Y.Value, 2.0);
                d = 2 * (searchedPositionList[2].X.Value - searchedPositionList[0].X.Value);
                e = 2 * (searchedPositionList[2].Y.Value - searchedPositionList[0].Y.Value);
                f = Math.Pow(searchedPositionList[2].X.Value, 2.0) - Math.Pow(searchedPositionList[0].X.Value, 2.0) +
                    Math.Pow(searchedPositionList[2].Y.Value, 2.0) - Math.Pow(searchedPositionList[0].Y.Value, 2.0);

                tmpPogoCen.X = ((c * e - f * b) / (e * a - b * d));
                tmpPogoCen.Y = ((c * d - a * f) / (d * b - a * e));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            centerX = tmpPogoCen.X;
            centerY = tmpPogoCen.Y;
        }

        //==> 두 도형의 꼭지점들을 리스트로 받아와서 편차(X,Y,Theta)를 구함
        private void GetCorrectionCoordinate(
            List<CatCoordinates> basePositionList, List<CatCoordinates> movedPositionList,
            out double centerDiffX, out double centerDiffY, out double degreeDiff)
        {
            //==> 설계상 Pattern 좌표들로 도형의 중심을 얻어옴.
            CatCoordinates designCenterCoordinate = new CatCoordinates();
            foreach (CatCoordinates coord in basePositionList)
            {
                designCenterCoordinate.X.Value += coord.X.Value;
                designCenterCoordinate.Y.Value += coord.Y.Value;
            }
            designCenterCoordinate.X.Value /= basePositionList.Count;
            designCenterCoordinate.Y.Value /= basePositionList.Count;

            //==> Pattern Match를 하여 얻은 좌표들로 도형의 중심을 얻어옴.
            CatCoordinates patternCenterCoordinate = new CatCoordinates();
            foreach (CatCoordinates coord in movedPositionList)
            {
                patternCenterCoordinate.X.Value += coord.X.Value;
                patternCenterCoordinate.Y.Value += coord.Y.Value;
            }
            patternCenterCoordinate.X.Value /= movedPositionList.Count;
            patternCenterCoordinate.Y.Value /= movedPositionList.Count;

            //==> 두 도형의 X,Y 편차를 얻어옴
            centerDiffX = patternCenterCoordinate.X.Value - designCenterCoordinate.X.Value;
            centerDiffY = patternCenterCoordinate.Y.Value - designCenterCoordinate.Y.Value;

            double degreeSum = 0;

            #region ==> [NOT USE] Version 1
            //for (int i = 0; i < DesignPositionList.Count; ++i)
            //{
            //    CatCoordinates patternPosition = PatternPositionList[i];
            //    CatCoordinates centerMovedPattern =
            //        new CatCoordinates(patternPosition.X.Value - centerDiffX, patternPosition.Y.Value - centerDiffY);
            //    CatCoordinates designPosition = DesignPositionList[i];

            //    double degree = GetDegreeeByThreePoint(designCenterCoordinate, designPosition, centerMovedPattern);
            //    degreeSum += degree;
            //}
            #endregion

            //==> Version 2
            int minPositionLen = Math.Min(basePositionList.Count, movedPositionList.Count);
            for (int i = 0; i < minPositionLen; ++i)
            {
                CatCoordinates designPosition = basePositionList[i];
                CatCoordinates patternPosition = movedPositionList[i];
                //==> (1)설계상 도형의 중심과 꼭지점을 이은 선
                //==> (2)Pattern Match 도형의 중심과 꼭지점을 이은 선

                //==> (1), (2) 두 직선 사이의 각을 얻어온다.
                double degree = GetDegreeeByTwoLine(designCenterCoordinate, designPosition, patternCenterCoordinate, patternPosition);
                degreeSum += degree;
            }

            degreeDiff = degreeSum / basePositionList.Count;//==> degree 편차의 평균값을 얻어와 최종 편차 Theta를 얻어옴.
            degreeDiff = Math.Round(degreeDiff, 5);
        }

        #region ==> [NOT USE] Version 1
        //private double GetDegreeeByThreePoint(CatCoordinates pivot, CatCoordinates point1, CatCoordinates point2)
        //{
        //    double arg1 = Math.Atan2(point1.Y.Value - pivot.Y.Value, point1.X.Value - pivot.X.Value);
        //    double arg2 = Math.Atan2(point2.Y.Value - pivot.Y.Value, point2.X.Value - pivot.X.Value);
        //    double degree = RadianToDegree(arg1 - arg2);
        //    if(degree > 180)
        //    {
        //        degree = degree - 360.0;
        //    }
        //    else if(degree <= -180)
        //    {
        //        degree = degree + 360.0;
        //    }
        //    return degree;
        //}
        #endregion

        //==> Version 2
        //==> 두 직선사이의 각을 구한다.
        private double GetDegreeeByTwoLine(CatCoordinates pointStart1, CatCoordinates pointEnd1, CatCoordinates pointStart2, CatCoordinates pointEnd2)
        {
            double x1 = pointStart1.X.Value;
            double y1 = pointStart1.Y.Value;

            double x2 = pointEnd1.X.Value;
            double y2 = pointEnd1.Y.Value;

            double x3 = pointStart2.X.Value;
            double y3 = pointStart2.Y.Value;

            double x4 = pointEnd2.X.Value;
            double y4 = pointEnd2.Y.Value;

            Double angle = Math.Atan2(y2 - y1, x2 - x1) - Math.Atan2(y4 - y3, x4 - x3);
            double degree = RadianToDegree(angle);

            if (degree > 180)
            {
                degree = degree - 360.0;
            }
            else if (degree <= -180)
            {
                degree = degree + 360.0;
            }

            return degree;
        }
        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
        private System.Windows.Point GetCrossPoint(
            System.Windows.Point a1,
            System.Windows.Point a2,
            System.Windows.Point b1,
            System.Windows.Point b2)
        {
            double x1 = a1.X;
            double y1 = a1.Y;
            double x2 = a2.X;
            double y2 = a2.Y;
            double x3 = b1.X;
            double y3 = b1.Y;
            double x4 = b2.X;
            double y4 = b2.Y;

            double part1 = (x1 * y2) - (y1 * x2);
            double part2 = (x3 * y4) - (y3 * x4);
            double under = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));

            double x = ((part1 * (x3 - x4)) - ((x1 - x2) * part2)) /
                       under;

            double y = ((part1 * (y3 - y4)) - ((y1 - y2) * part2)) /
                       under;

            return new System.Windows.Point(x, y);
        }
        #endregion

        //==> GP Card Change 과정 중에 사용하는 Motion 함수이다.
        public bool CamAbsMove(EnumProberCam proberCam, CatCoordinates position)
        {
            EventCodeEnum result = EventCodeEnum.NONE;

            double x = position.X.Value;
            double y = position.Y.Value;
            double z = position.Z.Value;
            double focuslimitvalue = ((CardChangeSysParam as CardChangeSysParam).FocusParam.FocusRange.Value / 2);
            double marginvalue = 10;

            bool retVal = false;
            try
            {
                ICamera cam = this.VisionManager().GetCam(proberCam);
                //==> Wafer Cam과 Card의 상판은 너무 가깝다, 그래서 안전하게 움직이기 위해 다음처럼 Z를 안전하게 내린다.
                //result = this.StageSupervisor().StageModuleState.ZCLEARED();
                if (result != EventCodeEnum.NONE)
                {
                    retVal = false;
                    return retVal;
                }
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    WaferCoordinate wfcoord = new WaferCoordinate();
                    wfcoord.Z.Value = position.Z.Value;
                    double zNEGLimitValue = zAxis.Param.NegSWLimit.Value;
                    double machineZ = this.CoordinateManager().WaferLowChuckConvert.ConvertBack(wfcoord).Z.Value;
                    if (zNEGLimitValue + focuslimitvalue + marginvalue < machineZ)
                    {
                        z = position.Z.Value;
                    }
                    else
                    {
                        MachineCoordinate mcoord = new MachineCoordinate();
                        mcoord.Z.Value = zNEGLimitValue + focuslimitvalue + marginvalue;
                        z = this.CoordinateManager().WaferLowChuckConvert.Convert(mcoord).Z.Value;
                    }

                    LoggerManager.Debug($"GPCardAlign CamAbsMove func. ProberCam:{proberCam} position X:{x} position Y:{y} position Z:{z}");
                    LoggerManager.Debug($"[SetLight] Cam:{proberCam.ToString()}, Type: {EnumLightType.COAXIAL},Value: {CardChangeSysParam.GP_WL_COAXIAL}");
                    LoggerManager.Debug($"[SetLight] Cam:{proberCam.ToString()}, Type: {EnumLightType.OBLIQUE},Value: {CardChangeSysParam.GP_WL_OBLIQUE}");
                    cam.SetLight(EnumLightType.COAXIAL, CardChangeSysParam.GP_WL_COAXIAL);
                    cam.SetLight(EnumLightType.OBLIQUE, CardChangeSysParam.GP_WL_OBLIQUE);
                    result = this.StageSupervisor().StageModuleState.CardViewMove(x, y);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GPCardAlign CamAbsMove func. CardViewMove(X,Y) fail return code:{result}");
                        retVal = false;
                        return retVal;
                    }

                    result = this.StageSupervisor().StageModuleState.CardViewMove(zAxis, z);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GPCardAlign CamAbsMove func. CardViewMove(Z) fail return code:{result}");
                        retVal = false;
                        return retVal;
                    }

                    //StageCylinderType.MoveWaferCam.Extend();
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    StageCylinderType.MoveWaferCam.Retract();
                    LoggerManager.Debug($"[SetLight] Cam:{proberCam.ToString()}, Type: {EnumLightType.COAXIAL},Value: {CardChangeSysParam.GP_PL_COAXIAL}");
                    result = cam.SetLight(EnumLightType.COAXIAL, CardChangeSysParam.GP_PL_COAXIAL);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[SetLight Error] Cam:{proberCam.ToString()}, Type: {EnumLightType.COAXIAL},Value: {CardChangeSysParam.GP_PL_COAXIAL}");
                    }
                    LoggerManager.Debug($"[SetLight] Cam:{proberCam.ToString()}, Type: {EnumLightType.OBLIQUE},Value: {CardChangeSysParam.GP_PL_OBLIQUE}");
                    result = cam.SetLight(EnumLightType.OBLIQUE, CardChangeSysParam.GP_PL_OBLIQUE);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[SetLight Error] Cam:{proberCam.ToString()}, Type: {EnumLightType.OBLIQUE},Value: {CardChangeSysParam.GP_PL_OBLIQUE}");
                    }
                    LoggerManager.Debug($"GPCardAlign CamAbsMove func. ProberCam:{proberCam} position X:{x} position Y:{y} position Z:{z}");
                    result = this.StageSupervisor().StageModuleState.PogoViewMove(x, y, z);
                    if (result != EventCodeEnum.NONE)
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
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        //==> GP Card Change 과정 중에 사용하는 RelMoveZ 함수 이다.
        public void RelMoveZ(EnumProberCam proberCam, double val)
        {
            try
            {
                double apos = 0;
                double absPos = 0;
                if (proberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref apos);

                    absPos = apos + val;

                    if ((this.IOManager().IO.Inputs.DIWAFERCAMMIDDLE.Value == false) &&
                      (this.IOManager().IO.Inputs.DIWAFERCAMREAR.Value == true))
                    {
                        //카메라가 접혀있음. 움직일수 있다.
                    }
                    else if (absPos > -68000.0)//==> Wafer Cam 높이 이상으로는 못 올라 간다.
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Up, Wafer Cam Limit, {absPos}");
                        return;
                    }


                    if (absPos > zaxis.Param.PosSWLimit.Value)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Up, SW Limit, {absPos}");
                        return;
                    }
                    if (absPos < zaxis.Param.NegSWLimit.Value)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Down, SW Limit, {absPos}");
                    }

                    this.MotionManager().AbsMove(zaxis, absPos, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                }
                else if (proberCam == EnumProberCam.PIN_LOW_CAM && StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                {
                    ProbeAxisObject pzaxis = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                    this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref apos);

                    absPos = apos + val;
                    //if (absPos > -7500.0)//==> Pin Camera의 상판에 대한 초점 거리
                    //{
                    //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : PZ Up, Pogo  Limit, {absPos}");
                    //    return;
                    //}
                    if (absPos > pzaxis.Param.PosSWLimit.Value)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : PZ Up, SW Limit, {absPos}");
                        return;
                    }
                    if (absPos < pzaxis.Param.NegSWLimit.Value)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : PZ Down, SW Limit, {absPos}");
                    }

                    this.MotionManager().AbsMove(pzaxis, absPos, pzaxis.Param.Speed.Value, pzaxis.Param.Acceleration.Value);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        #region ==> [For Test] Observation : Card가 Pogo에 Docking된 정밀도를 측정하기 위한 코드 들이다. 실제 GP Card Change 과정에서는 사용되지 않는다.
        public bool ObservationCard()
        {
            double centerDiffX = 0;
            double centerDiffY = 0;
            double degreeDiff = 0;
            bool retVal = false;

            try
            {
                List<CatCoordinates> designPosList = new List<CatCoordinates>(GPCCObservationTempParam.ObservationMarkPosList);
                List<CatCoordinates> registeredMarkPosList = new List<CatCoordinates>(GPCCObservationTempParam.RegisteredMarkPosList);
                List<CatCoordinates> patternPosList = new List<CatCoordinates>();
                for (int i = 0; i < registeredMarkPosList.Count; i++)
                {
                    CatCoordinates registeredMarkPos = registeredMarkPosList[i];
                    CatCoordinates patternPos;
                    if (FindPatternPosition_Observe(registeredMarkPos, $"{PatternPath}_CONTACT_CARD_{i}", out patternPos)
                        == false)
                    {
                        break;
                    }

                    patternPosList.Add(patternPos);
                }

                if (designPosList.Count != patternPosList.Count)
                {
                    LoggerManager.Error("Logging Contact card position Fail");
                    retVal = false;
                    return retVal;
                }

                GetCorrectionCoordinate(designPosList, patternPosList, out centerDiffX, out centerDiffY, out degreeDiff);
                LoggerManager.Debug($"GPCC Contact Card Diff ... : X,Y,T Pos : {centerDiffX}, {centerDiffY}, {degreeDiff}");
                using (StreamWriter sw = new StreamWriter(_GPCardAlignLogPath, append: true))
                {
                    sw.WriteLine($"GPCC Contact Card Diff ... : X,Y,T Pos : {centerDiffX}, {centerDiffY}, {degreeDiff}");
                }

                var result = this.StageSupervisor().StageModuleState.ZCLEARED();
                if (result != EventCodeEnum.NONE)
                {
                    retVal = false;
                    return retVal;
                }
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void ObservationRegisterPattern(int index)
        {
            try
            {
                this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);

                ImageBuffer img = null;
                ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                cam.GetCurImage(out img);

                RegisteImageBufferParam rparam = new RegisteImageBufferParam(
                  EnumProberCam.PIN_LOW_CAM,
                  (int)((cam.GetGrabSizeWidth() / 2) - (GPCCObservationTempParam.ObservationPatternWidth / 2)),
                  (int)((cam.GetGrabSizeHeight() / 2) - (GPCCObservationTempParam.ObservationPatternHeight / 2)),
                  (int)GPCCObservationTempParam.ObservationPatternWidth,
                  (int)GPCCObservationTempParam.ObservationPatternHeight,
                  $"{PatternPath}_CONTACT_CARD_{index}");

                rparam.ImageBuffer = img;
                
                this.VisionManager().SavePattern(rparam);

                this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool FindPatternPosition_Observe(CatCoordinates searchBasePos, String patternPath, out CatCoordinates patternPos)
        {
            patternPos = null;

            float xJumDistance = 0;
            float yJumDistance = 0;

            xJumDistance = 2000.0f;
            yJumDistance = 2000.0f;

            bool isFind = false;
            try
            {
                foreach (PatternRelPoint relPos in _PatternRelPointList9)
                {
                    CatCoordinates searchPos = new CatCoordinates(searchBasePos.X.Value, searchBasePos.Y.Value, searchBasePos.Z.Value);
                    searchPos.X.Value += (xJumDistance * relPos.X);
                    searchPos.Y.Value += (yJumDistance * relPos.Y);

                    if (CamAbsMove_Observe(searchPos) == false)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(300);

                    CatCoordinates patternMatchPos;
                    if (GetPatternMatchingPosition(EnumProberCam.PIN_LOW_CAM, patternPath, EnumCCAlignModule.POGO, out patternMatchPos))
                    {
                        patternPos = new CatCoordinates();
                        patternPos.X.Value = patternMatchPos.X.Value;
                        patternPos.Y.Value = patternMatchPos.Y.Value;
                        patternPos.Z.Value = patternMatchPos.Z.Value;
                        isFind = true;
                        CamAbsMove_Observe(patternMatchPos);
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isFind;
        }
        public bool CamAbsMove_Observe(CatCoordinates position)
        {
            bool retVal = false;
            try
            {
                StageCylinderType.MoveWaferCam.Retract();

                EventCodeEnum result = EventCodeEnum.NONE;

                double x = position.X.Value;
                double y = position.Y.Value;
                double z = position.Z.Value;

                //result = this.MotionManager().AbsMove(EnumAxisConstants.C, 0);
                var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                result = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(axist, 0, axist.Param.Speed.Value, axist.Param.Acceleration.Value);
                if (result != EventCodeEnum.NONE)
                {
                    retVal = false;
                    return retVal;
                }

                ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);

                cam.SetLight(EnumLightType.COAXIAL, GPCCObservationTempParam.ObservationCOAXIAL);
                cam.SetLight(EnumLightType.OBLIQUE, GPCCObservationTempParam.ObservationOBLIQUE);

                result = this.StageSupervisor().StageModuleState.PogoViewMove(x, y, z);
                if (result != EventCodeEnum.NONE)
                {
                    retVal = false;
                    return retVal;
                }
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion
    }
}
