using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Linq;

namespace PolishWaferFocusingModule
{
    using LogModule;
    using PolishWaferParameters;
    using ProberInterfaces.PolishWafer;
    using SubstrateObjects;
    using ProberInterfaces.WaferAlignEX;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using ProberInterfaces.WaferAlign;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ComponentModel;

    public class PolishWaferFocusing_Standard : IPolishWaferFocusing
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///Focusing Parameter Wafer -> Interval Parameter 로 변경필요.
        /// </summary>
        #region //..Focusing

        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }

        private PolishWaferParameter PWParam
        {
            get
            {
                return (PolishWaferParameter)this.PolishWaferModule().PolishWaferParameter;
            }
        }

        private List<Tuple<HeightProfilignPosEnum, WaferCoordinate>> _HeightProfiling 
            = new List<Tuple<HeightProfilignPosEnum, WaferCoordinate>>();
        public List<Tuple<HeightProfilignPosEnum, WaferCoordinate>> HeightProfiling
        {
            get { return _HeightProfiling; }
            set
            {
                if (value != _HeightProfiling)
                {
                    _HeightProfiling = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double Distance2D(double X1, double Y1, double X2, double Y2)
        {
            return Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
        }



        //위치를 찾아서 PlanePoint값을 변경하자.
        public void PlanePointChangetoFocusing(HeightProfilignPosEnum posenum, WaferHeightMapping heightmapping)
        {
            try
            {
                LoggerManager.Debug($"PlanePointChangetoFocusing() : [POINT5,POINT9] Change the value to the Z Value of the PlanePointby focusing the PlanePoint with the Center Z value ");
                if (posenum != HeightProfilignPosEnum.UNDEFINED && HeightProfiling.Count > 0 && heightmapping != null)
                {
                    var findprofiling = HeightProfiling.Where(x => x.Item1 == posenum).FirstOrDefault();
                    var findpoint = heightmapping.PlanPoints.Where(x => x.GetX() == findprofiling.Item2.GetX() && x.GetY() == findprofiling.Item2.GetY()).FirstOrDefault();
                    LoggerManager.Debug($"Equal Center PlanePoint {posenum} Pos Z : {findpoint.Z.Value:0.00} Change to Focused PlanePoint Z : {heightmapping.PlanPoints.Last().Z.Value:0.00}");
                    findpoint.Z.Value = heightmapping.PlanPoints.Last().Z.Value;
                }
                else
                {
                    LoggerManager.Debug($"PlanePointChangetoFocusing() : Method Parameter null, PosEnum : {posenum}, HeightProfiling : {HeightProfiling.Count}, HeightMapping null");

                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PolishWafer - PlanePointChangetoFocusing() : Error occured. ");
            }
        }


        public void AddHeighPlanePoint(WaferHeightMapping heightmapping, WAHeightPositionParam param = null)
        {
            try
            {
                double TiltedZ = 0.0;
                WaferCoordinate PlanePoint = new WaferCoordinate();

                if (param == null)
                {
                    PlanePoint = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
                else
                {
                    param.Position.CopyTo(PlanePoint);
                }

                if (PlanePoint != null)
                {

                    TiltedZ = this.WaferAligner().CalcThreePodTiltedPlane(PlanePoint.GetX(), PlanePoint.GetY(), true);
                    LoggerManager.Debug($"[{this.GetType().Name}], AddHeighPlanePoint() : PlanePoint Z = {PlanePoint.GetZ():0.00} - TiltedPlane Z = {TiltedZ:0.00} for (X,Y) = ({PlanePoint.GetX():0.00},{PlanePoint.GetY():0.00})");
                    PlanePoint.Z.Value = PlanePoint.Z.Value - TiltedZ;
                    LoggerManager.Debug($"[{this.GetType().Name}], AddHeighPlanePoint() : AddHeightPlanePoint Z = {PlanePoint.GetZ():0.00} for (X,Y) = ({PlanePoint.GetX():0.00},{PlanePoint.GetY():0.00})");

                    heightmapping.PlanPoints.Add(PlanePoint);
                }
                else
                {
                    LoggerManager.Debug("AddHeightPlanPoint Failed.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PolishWafer - AddHeighPlanePoint() : Error occured. ");
            }
        }

        public EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param, bool manualmode = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //LoggerManager.Funclog(typeof(PolishWaferFocusing_Standard), EnumFuncCallingTime.START);

            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Focusing_Start);

            try
            {
                PolishWaferCleaningParameter _param = param as PolishWaferCleaningParameter;

                IPolishWaferSourceInformation pwinfo = this.StageSupervisor().WaferObject.GetPolishInfo();

                // 현재 클리닝의 Name과 Stage에 존재하는 Name이 같아야 함.

                //var source = PWParam.SourceParameters.Where(x => x.DefineName.Value == _param.WaferDefineType.Value).FirstOrDefault();


                if ((_param != null) && (pwinfo != null) && (_param.WaferDefineType.Value == pwinfo.DefineName.Value)
                    )
                {
                    if (_param.FocusingModuleDllInfo != null)
                    {
                        // Move to Focusing Position

                        // Get Focusing Module
                        var fm = this.FocusManager().GetFocusingModel(_param.FocusingModuleDllInfo);

                        if (fm != null)
                        {
                            if (_param.FocusingPos == null)
                            {
                                _param.FocusingPos = new ObservableCollection<FocusingPosition>();
                            }

                            retVal = this.FocusManager().ValidationFocusParam(_param.FocusParam);
                            if (retVal != EventCodeEnum.NONE)
                            {
                                if (_param.FocusParam.FocusRange.Value <= 10)
                                {
                                    _param.FocusParam.FocusRange.Value = 300;
                                }
                                
                                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, _param.FocusParam, _param.FocusParam.FocusRange.Value);
                                LoggerManager.Debug($"[PolishWaferFocusing_Standard], DoFocusing() : validation failed, default focus param is created.");
                            }

                            retVal = MakeFocusingPosition(_param.FocusingPos, _param.FocusingPointMode.Value, pwinfo.Thickness.Value, pwinfo.Margin.Value, _param.FocusParam.FocusingCam.Value);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                if (_param.FocusingPos == null)
                                {
                                    retVal = EventCodeEnum.PARAM_ERROR;
                                    return retVal;
                                }

                                retVal = this.FocusManager().ValidationFocusParam(_param.FocusParam);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"[PolishWaferFocusing_Standard], DoFocusing() : Focusing parameter is wrong.");

                                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, _param.FocusParam, 300);
                                }

                                ICamera CurCam = this.VisionManager().GetCam(_param.FocusParam.FocusingCam.Value);

                                if (_param.CenteringLightParams == null || _param.CenteringLightParams.Count <= 0)
                                {
                                    retVal = EventCodeEnum.PARAM_ERROR;

                                    return retVal;
                                }

                                foreach (var light in _param.FocusParam.LightParams)
                                {
                                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                                }

                                if(_param.FocusParam.FlatnessThreshold.Value < this.VisionManager().GetMaxFocusFlatnessValue())
                                {
                                    _param.FocusParam.FlatnessThreshold.Value = 95.0;
                                }

                                foreach (var item in _param.FocusingPos.Select((value, i) => new { i, value }))
                                {
                                    var focuspos = item.value;
                                    var index = item.i;

                                    // Move
                                    if (_param.FocusParam.FocusingCam.Value == EnumProberCam.WAFER_LOW_CAM)
                                    {
                                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(focuspos.Position.X.Value, focuspos.Position.Y.Value, focuspos.Position.Z.Value, true);
                                    }
                                    else if (_param.FocusParam.FocusingCam.Value == EnumProberCam.WAFER_HIGH_CAM)
                                    {
                                        retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(focuspos.Position.X.Value, focuspos.Position.Y.Value, focuspos.Position.Z.Value, true);
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"Camera type is wrong.");

                                        retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;

                                        return retVal;
                                    }

                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        retVal = fm.Focusing_Retry(_param.FocusParam, false, false, false, this);

                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            WaferCoordinate wcoord = null;

                                            if (_param.FocusParam.FocusingCam.Value == EnumProberCam.WAFER_LOW_CAM)
                                            {
                                                wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                                                wcoord.Z.Value = wcoord.GetZ();
                                            }
                                            else if (_param.FocusParam.FocusingCam.Value == EnumProberCam.WAFER_HIGH_CAM)
                                            {
                                                wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                                                wcoord.Z.Value = wcoord.GetZ();
                                            }
                                            else
                                            {
                                                LoggerManager.Error($"Camera type is wrong.");

                                                retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;

                                                return retVal;// ERROR
                                            }

                                            if (index == 0)
                                            {
                                                double distance = 500000;

                                                double wafercenterX = wcoord.GetX();
                                                double wafercenterY = wcoord.GetY();

                                                double left = wafercenterX - (distance / 2);
                                                double right = wafercenterX + (distance / 2);
                                                double upper = wafercenterY + (distance / 2);
                                                double lower = wafercenterY - (distance / 2);

                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.CENTER, new WaferCoordinate(wcoord)));

                                                //Left
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.LEFT, new WaferCoordinate(left, wafercenterY, wcoord.GetZ())));

                                                //Right
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.RIGHT, new WaferCoordinate(right, wafercenterY, wcoord.GetZ())));

                                                //Upper
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.UPPER, new WaferCoordinate(wafercenterX, upper, wcoord.GetZ())));

                                                //Lower
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.BOTTOM, new WaferCoordinate(wafercenterX, lower, wcoord.GetZ())));

                                                //LeftUpper
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.LEFTUPPER, new WaferCoordinate(left, upper, wcoord.GetZ())));

                                                //RightUpper
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.RIGHTUPPER, new WaferCoordinate(right, upper, wcoord.GetZ())));

                                                //LeftLower
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.LEFTBOTTOM, new WaferCoordinate(left, lower, wcoord.GetZ())));

                                                //RightLower
                                                HeightProfiling.Add(new Tuple<HeightProfilignPosEnum, WaferCoordinate>(HeightProfilignPosEnum.RIGHTBOTTOM, new WaferCoordinate(right, lower, wcoord.GetZ())));

                                                //PlanePointCenter.X.Value = wafercenterX;
                                                //PlanePointCenter.X.Value = wafercenterX;

                                                foreach (var height in HeightProfiling)
                                                {
                                                    this.AddHeighPlanePoint(pwinfo.WaferHeightMapping, new WAHeightPositionParam(height.Item2));
                                                    LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : AddHeightPlanePoint to PolishWafer. X : " + $"{height.Item2.GetX()}, Y : {height.Item2.GetY()}, Z : {height.Item2.GetZ()}");
                                                }
                                            }
                                            else
                                            {
                                                this.AddHeighPlanePoint(pwinfo.WaferHeightMapping, new WAHeightPositionParam(wcoord));
                                                if (_param.FocusingPointMode.Value == PWFocusingPointMode.POINT5)
                                                {
                                                    PlanePointChangetoFocusing(focuspos.PosEnum, pwinfo.WaferHeightMapping);
                                                }
                                                LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : AddHeightPlanePoint to PolishWafer. X : " + $"{wcoord.GetX()}, Y : {wcoord.GetY()}, Z : {wcoord.GetZ()}");

                                            }

                                            focuspos.Position.Z.Value = wcoord.GetZ();
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Faild Focusing.");

                                            retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;

                                            return retVal;
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"Faild Move Function.");

                                        retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;

                                        return retVal;
                                    }
                                }

                                if (pwinfo.WaferHeightMapping != null)
                                {
                                    pwinfo.WaferHeightMapping.HeightPlanPoints.Clear();
                                    pwinfo.WaferHeightMapping.PlanPoints.Clear();
                                }

                                double focusAvg = 0;
                                double focusMin = 0;
                                double focusMax = 0;
                                double focusMaxMinDiff = 0;

                                if (_param.FocusingPos.Count > 0)
                                {
                                    foreach (var item in _param.FocusingPos.Select((value, i) => new { i, value }))
                                    {
                                        var focuspos = item.value;
                                        var index = item.i;

                                        LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : [{index + 1}] X : {focuspos.Position.X.Value}, Y : {focuspos.Position.Y.Value}, Z : {focuspos.Position.Z.Value}", isInfo:true);
                                    }

                                    focusMin = _param.FocusingPos.Min(x => x.Position.Z.Value);
                                    focusMax = _param.FocusingPos.Max(x => x.Position.Z.Value);
                                    focusAvg = _param.FocusingPos.Average(x => x.Position.Z.Value);
                                    focusMaxMinDiff = focusMax - focusMin;

                                    LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : [Focusing summary]: Thickness = {pwinfo.Thickness.Value}, Min = {focusMin:F2}, Max = {focusMax:F2}, Avg = {focusAvg:F2}, Max - Min = {focusMaxMinDiff:F2}", isInfo: true);
                                }

                                if (_param.FocusingHeightTolerance != null && _param.FocusingHeightTolerance.Value > 0)
                                {
                                    double diffabs = Math.Abs(pwinfo.Thickness.Value - focusAvg);

                                    string focretStr = string.Empty;

                                    if (diffabs > _param.FocusingHeightTolerance.Value)
                                    {
                                        focretStr = "FAIL";
                                        retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                                    }
                                    else
                                    {
                                        focretStr = "OK";
                                    }

                                    LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : Abs(Thickness - average) = {diffabs:F2}. Tolerance = {_param.FocusingHeightTolerance.Value}, {focretStr}.");
                                }
                                else
                                {
                                    if (_param.FocusingHeightTolerance == null)
                                    {

                                        LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : FocusingRangeTolerance is null.");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : FocusingRangeTolerance value is {_param.FocusingHeightTolerance.Value}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Faild get focusing module.");
                            retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Focusing parameter is not enough.");
                        retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                    }
                }
                else
                {
                    LoggerManager.Debug($"Define Name is not matched.");
                    retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;

                LoggerManager.Exception(err);
            }
            finally
            {
                // POLISHWAFER_FOCUSING_ERROR for Testing
                if ((retVal == EventCodeEnum.NONE || retVal == EventCodeEnum.UNDEFINED) && 
                    this.PolishWaferModule().PolishWaferControlItems.POLISHWAFER_FOCUSING_ERROR == true)
                {
                    retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Focusing_OK);
                }
                else
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Focusing_Failure, EventCodeEnum.NONE, retVal.ToString());
                    this.NotifyManager().Notify(EventCodeEnum.POLISHWAFER_FOCUSING_ERROR);
                }
            }

            //LoggerManager.Funclog(typeof(PolishWaferFocusing_Standard), EnumFuncCallingTime.END);

            return retVal;
        }

        //private EventCodeEnum NeedToRecalcJumpIndex(PolishWaferCleaningParameter pwcleaningparam)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        if (pwcleaningparam.JumpIndexs == null)
        //        {
        //            pwcleaningparam.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
        //        }

        //        // 설정된 포인트와 데이터 개수가 일치 해야 됨.
        //        if (pwcleaningparam.JumpIndexs.Count == (int)pwcleaningparam.FocusingPointMode.Value)
        //        {
        //            // 갖고 있는 데이터의 index가 현재 디바이스에서 유효한 영역에 있는 값인지 확인
        //            foreach (var jumpindex in pwcleaningparam.JumpIndexs)
        //            {
        //                if ((jumpindex.Index.XIndex <= 0) || (jumpindex.Index.XIndex > Wafer.GetPhysInfo().MapCountX.Value - 1) ||
        //                    (jumpindex.Index.YIndex <= 0) || (jumpindex.Index.YIndex > Wafer.GetPhysInfo().MapCountY.Value - 1)
        //                    )
        //                {
        //                    retval = EventCodeEnum.UNDEFINED;
        //                    LoggerManager.Error($"Data is invalid.");
        //                    break;
        //                }
        //                else
        //                {
        //                    retval = EventCodeEnum.NONE;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            LoggerManager.Error($"Data is invalid.");

        //            retval = EventCodeEnum.UNDEFINED;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //private EventCodeEnum NeedToRecalcJumpIndex(PolishWaferInformation pwinfo)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        if (pwinfo.JumpIndexs == null)
        //        {
        //            pwinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
        //        }

        //        // 설정된 포인트와 데이터 개수가 일치 해야 됨.
        //        if (pwinfo.JumpIndexs.Count == (int)pwinfo.FocusingPointMode.Value)
        //        {
        //            // 갖고 있는 데이터의 index가 현재 디바이스에서 유효한 영역에 있는 값인지 확인
        //            foreach (var jumpindex in pwinfo.JumpIndexs)
        //            {
        //                if ((jumpindex.Index.XIndex <= 0) || (jumpindex.Index.XIndex > Wafer.GetPhysInfo().MapCountX.Value - 1) ||
        //                    (jumpindex.Index.YIndex <= 0) || (jumpindex.Index.YIndex > Wafer.GetPhysInfo().MapCountY.Value - 1)
        //                    )
        //                {
        //                    retval = EventCodeEnum.UNDEFINED;
        //                    LoggerManager.Error($"Data is invalid.");
        //                    break;
        //                }
        //                else
        //                {
        //                    retval = EventCodeEnum.NONE;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            LoggerManager.Error($"Data is invalid.");

        //            retval = EventCodeEnum.UNDEFINED;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        #region //..Focusing
        private EventCodeEnum FocusingAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //    IFocusing focusingmodule = this.FocusManager().GetFocusingModel(this.GetParam_PolishWafer().PolishWaferObj.FocusingModuleDllInfo);

                //    IWaferObject waferObject = this.GetParam_Wafer();
                //    IProbeCard probeCardInfo = this.GetParam_ProbeCard();

                //    double centerx = waferObject.GetSubsInfo().WaferCenter.GetX();
                //    double centery = waferObject.GetSubsInfo().WaferCenter.GetY();

                //    //Center Focusing
                //    retVal = MoveFocusing(centerx, centery, waferObject.GetSubsInfo().ActualThickness);
                //    if (retVal != EventCodeEnum.NONE)
                //    {
                //        this.PolishWaferModule().ReasonOfError.Reason = "Focusing Failed.";
                //        return retVal;
                //    }
                //    this.PolishWaferModule().AddOutSideHeightPlanePoint();

                //Edge Focusing

                //if (this.GetParam_PolishWafer().PolishWaferObj.FocusingMode.Value == EnumPolishWaferFocusingMode.POINT5)
                //{
                //    long leftindex = 0;
                //    long rightinde = 0;
                //    long upperindex = 0;
                //    long lowerindex = 0;
                //    List<MachineIndex> machineIndices = new List<MachineIndex>();
                //    MachineIndex centerindex = this.WaferAligner().WPosToMIndex(waferObject.GetSubsInfo().WaferCenter);

                //    machineIndices.Add(waferObject.GetDevices().FindAll(index => index.DieIndexM.YIndex == centerindex.YIndex).Find(die => die.DieType.Value == DieTypeEnum.TEST_DIE).DieIndexM);
                //    machineIndices.Add(waferObject.GetDevices().FindAll(index => index.DieIndexM.YIndex == centerindex.YIndex).FindLast(die => die.DieType.Value == DieTypeEnum.TEST_DIE).DieIndexM);
                //    machineIndices.Add(waferObject.GetDevices().FindAll(index => index.DieIndexM.XIndex == centerindex.XIndex).Find(die => die.DieType.Value == DieTypeEnum.TEST_DIE).DieIndexM);
                //    machineIndices.Add(waferObject.GetDevices().FindAll(index => index.DieIndexM.XIndex == centerindex.XIndex).FindLast(die => die.DieType.Value == DieTypeEnum.TEST_DIE).DieIndexM);

                //    foreach (var index in machineIndices)
                //    {
                //        index.XIndex = index.XIndex - centerindex.XIndex;
                //        index.YIndex = index.YIndex - centerindex.YIndex;

                //        retVal = MoveFocusing(
                //            centerx + (index.XIndex * waferObject.GetSubsInfo().ActualDeviceSize.Width.Value),
                //            centery + (index.XIndex * waferObject.GetSubsInfo().ActualDeviceSize.Height.Value),
                //            waferObject.GetSubsInfo().ActualThickness);
                //        if (retVal != EventCodeEnum.NONE)
                //        {
                //            this.PolishWaferModule().ReasonOfError.Reason = "Focusing Failed.";
                //            return retVal;
                //        }
                //        this.PolishWaferModule().AddHeighPlanePoint();
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private EventCodeEnum MoveFocusing(double xpos, double ypos, double zpos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if (this.GetParam_PolishWafer().PolishWaferObj.FocusParameter.FocusingCam.Value == EnumProberCam.WAFER_LOW_CAM)
                //    this.StageSupervisor().StageModuleState.WaferLowViewMove(xpos, ypos, zpos);
                //else if (this.GetParam_PolishWafer().PolishWaferObj.FocusParameter.FocusingCam.Value == EnumProberCam.WAFER_HIGH_CAM)
                //    this.StageSupervisor().StageModuleState.WaferHighViewMove(xpos, ypos, zpos);

                //IFocusing focusingmodule = this.FocusManager().GetFocusingModel(this.GetParam_PolishWafer().PolishWaferObj.FocusingModuleDllInfo);
                //retVal = focusingmodule.Focusing_Retry(this.GetParam_PolishWafer().PolishWaferObj.FocusParameter, false, false, false, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        #endregion

        #region //..Touch Sensor Focusing
        private EventCodeEnum TouchSensorFoucing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #endregion

        private EventCodeEnum MakeFocusingPosition(ObservableCollection<FocusingPosition> focusingpos, PWFocusingPointMode pointmode, double thickness, double PW_margin, EnumProberCam focusingCam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                focusingpos.Clear();

                WaferCoordinate centerpos = new WaferCoordinate();

                IPolishWaferSourceInformation pwinfo = this.StageSupervisor().WaferObject.GetPolishInfo();

                if (pwinfo.PolishWaferCenter == null)
                {
                    pwinfo.PolishWaferCenter = new WaferCoordinate();
                }

                centerpos.X.Value = pwinfo.PolishWaferCenter.X.Value;
                centerpos.Y.Value = pwinfo.PolishWaferCenter.Y.Value;
                centerpos.Z.Value = thickness;

                if ((pointmode == PWFocusingPointMode.POINT1) ||
                    (pointmode == PWFocusingPointMode.POINT5))
                {
                    // Add Center Position : Offset is zero.
                    focusingpos.Add(new FocusingPosition(centerpos, HeightProfilignPosEnum.CENTER));

                    if (pointmode == PWFocusingPointMode.POINT5)
                    {
                        double WaferSize_um = Wafer.GetPhysInfo().WaferSize_um.Value;

                        double edgepos = 0;
                        
                        edgepos = ((WaferSize_um / 2) / Math.Sqrt(2));
                        
                        ICamera cam = this.VisionManager().GetCam(focusingCam);
                       
                        int grabsize_X = cam.Param.GrabSizeX.Value;
                        int grabsize_Y = cam.Param.GrabSizeY.Value;
                        
                        double ActualDieSize_X = this.GetParam_Wafer().GetSubsInfo().ActualDieSize.Width.Value;
                        double ActualDieSize_Y = this.GetParam_Wafer().GetSubsInfo().ActualDieSize.Height.Value;

                        double margin_X = PW_margin + (grabsize_X / 2) + ActualDieSize_X;
                        double margin_Y = PW_margin + (grabsize_Y / 2) + ActualDieSize_Y;

                        FocusingPosition left = new FocusingPosition(new WaferCoordinate(centerpos.X.Value - edgepos + margin_X, centerpos.Y.Value, centerpos.Z.Value), HeightProfilignPosEnum.LEFT);
                        FocusingPosition right = new FocusingPosition(new WaferCoordinate(centerpos.X.Value + edgepos + margin_X, centerpos.Y.Value, centerpos.Z.Value), HeightProfilignPosEnum.RIGHT);
                        FocusingPosition up = new FocusingPosition(new WaferCoordinate(centerpos.X.Value, centerpos.Y.Value + edgepos - margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.UPPER);
                        FocusingPosition down = new FocusingPosition(new WaferCoordinate(centerpos.X.Value, centerpos.Y.Value - edgepos + margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.BOTTOM);

                        focusingpos.Add(left);
                        focusingpos.Add(right);
                        focusingpos.Add(up);
                        focusingpos.Add(down);

                        retval = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"Data is invalid.");

                    retval = EventCodeEnum.UNDEFINED;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //private EventCodeEnum GetJumpIndex(PolishWaferInformation pwinfo)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        pwinfo.JumpIndexs.Clear();

        //        WaferCoordinate centerpos = new WaferCoordinate();

        //        centerpos.X.Value = Wafer.GetSubsInfo().WaferCenter.GetX();
        //        centerpos.Y.Value = Wafer.GetSubsInfo().WaferCenter.GetY();

        //        if ((pwinfo.FocusingPointMode.Value == PWFocusingPointMode.POINT1) ||
        //            (pwinfo.FocusingPointMode.Value == PWFocusingPointMode.POINT5))
        //        {
        //            // Add Center Position : Offset is zero.
        //            pwinfo.JumpIndexs.Add(MakeJumpIndexParam(0, 0));

        //            if (pwinfo.FocusingPointMode.Value == PWFocusingPointMode.POINT5)
        //            {
        //                double AlignMinimumLength = 0;
        //                double AlignOptimumLength = 0;
        //                double AlignMaximumLength = 0;

        //                double MinimumLength;
        //                double OptimumLength;
        //                double MaximumLength;

        //                switch (Wafer.GetPhysInfo().WaferSize_um.Value)
        //                {
        //                    case 150000:

        //                        // TODO : 6INCH ?
        //                        AlignMinimumLength = AlignMinimumLengthFor12Inch;
        //                        AlignOptimumLength = AlignOptimumLengthFor12Inch;
        //                        AlignMaximumLength = AlignMaximumLengthFor12Inch;

        //                        MinimumLength = AlignMinimumLength * (1.0 / 2.0);
        //                        OptimumLength = AlignOptimumLength * (1.0 / 2.0);
        //                        MaximumLength = AlignMaximumLength * (1.0 / 2.0);

        //                        break;

        //                    case 200000:

        //                        // TODO : 8INCH ?

        //                        AlignMinimumLength = AlignMinimumLengthFor12Inch;
        //                        AlignOptimumLength = AlignOptimumLengthFor12Inch;
        //                        AlignMaximumLength = AlignMaximumLengthFor12Inch;

        //                        MinimumLength = AlignMinimumLength * (2.0 / 3.0);
        //                        OptimumLength = AlignOptimumLength * (2.0 / 3.0);
        //                        MaximumLength = AlignMaximumLength * (2.0 / 3.0);

        //                        break;
        //                    case 300000:


        //                        AlignMinimumLength = AlignMinimumLengthFor12Inch;
        //                        AlignOptimumLength = AlignOptimumLengthFor12Inch;
        //                        AlignMaximumLength = AlignMaximumLengthFor12Inch;

        //                        MinimumLength = AlignMinimumLength;
        //                        OptimumLength = AlignOptimumLength;
        //                        MaximumLength = AlignMaximumLength;

        //                        break;

        //                    default:

        //                        MinimumLength = AlignMinimumLength;
        //                        OptimumLength = AlignOptimumLength;
        //                        MaximumLength = AlignMaximumLength;

        //                        break;

        //                }

        //                int offsexindex;

        //                // LEFT
        //                offsexindex = SearchHorJumpIndex(-1, MinimumLength, OptimumLength, MaximumLength, pwinfo.DeadZone);
        //                pwinfo.JumpIndexs.Add(MakeJumpIndexParam(-offsexindex, 0));

        //                // RIGHT
        //                offsexindex = SearchHorJumpIndex(+1, MinimumLength, OptimumLength, MaximumLength, pwinfo.DeadZone);
        //                pwinfo.JumpIndexs.Add(MakeJumpIndexParam(offsexindex, 0));

        //                // DOWN
        //                offsexindex = SearchVerJumpIndex(-1, MinimumLength, OptimumLength, MaximumLength, pwinfo.DeadZone);
        //                pwinfo.JumpIndexs.Add(MakeJumpIndexParam(0, -offsexindex));

        //                // UP
        //                offsexindex = SearchVerJumpIndex(+1, MinimumLength, OptimumLength, MaximumLength, pwinfo.DeadZone);
        //                pwinfo.JumpIndexs.Add(MakeJumpIndexParam(0, offsexindex));

        //                retval = EventCodeEnum.NONE;
        //            }
        //        }
        //        else
        //        {
        //            LoggerManager.Error($"Data is invalid.");

        //            retval = EventCodeEnum.UNDEFINED;
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private StandardJumpIndexParam MakeJumpIndexParam(long xind, long yind)
        {
            StandardJumpIndexParam retval = null;

            try
            {
                MachineIndex mi = new MachineIndex(xind, yind);

                retval = new StandardJumpIndexParam(mi);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
                        return curindex;
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
                        return curindex;
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

        //private int SearchHorJumpIndex(int direction, double minimumlength, double optimumlength, double maximumlength, double deadzone)
        //{
        //    // direction : 이동 방향
        //    // -1 : 왼쪽
        //    //  1 : 오른쪽

        //    int retval = 0;

        //    double indexwidth = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
        //    double indexheight = Wafer.GetSubsInfo().ActualDieSize.Height.Value;
        //    double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
        //    double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

        //    double acminimumlength = Math.Ceiling(minimumlength / indexwidth) * indexwidth;
        //    double acoptimumlength = Math.Ceiling(optimumlength / indexwidth) * indexwidth;
        //    double acmaximumlength = Math.Ceiling(maximumlength / indexwidth) * indexwidth;

        //    //double movex = ptinfo.GetX() + wafercenterx;
        //    //double movey = ptinfo.GetY() + wafercentery;

        //    double movex = 0 + wafercenterx;
        //    double movey = 0 + wafercentery;

        //    double edgeXlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value / 2), 2) - Math.Pow(movey, 2)) - deadzone;

        //    try
        //    {
        //        //WaferCenter보다 패턴위치 -  : -1 |  + : 1
        //        //direction = ptinfo.GetX() >= wafercenterx ? -1 : 1;

        //        if (direction == -1)
        //        {
        //            double remainlength = edgeXlimit - (Math.Abs(movex));

        //            if (remainlength >= acminimumlength / 2)
        //            {
        //                if (remainlength >= acoptimumlength / 2)
        //                    retval = Calc(1, indexwidth, -((acoptimumlength / 2) + Math.Abs(movex)), movex);
        //                else
        //                    retval = Calc(1, indexwidth, -((acminimumlength / 2) + Math.Abs(movex)), movex);
        //            }
        //            else
        //            {
        //                remainlength = edgeXlimit + (Math.Abs(movex));

        //                if (remainlength >= acminimumlength)
        //                {
        //                    if (remainlength >= acoptimumlength)
        //                        retval = Calc(1, indexwidth, (movex + acoptimumlength), movex);
        //                    else
        //                        retval = Calc(1, indexwidth, (movex + acminimumlength), movex);
        //                }
        //                else
        //                {
        //                    retval = -1;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            double remainlength = edgeXlimit + (Math.Abs(movex));

        //            if (remainlength >= acminimumlength / 2)
        //            {
        //                if (remainlength >= acoptimumlength / 2)
        //                    retval = Calc(1, indexwidth, (movex + (acoptimumlength / 2)), movex);
        //                else
        //                    retval = Calc(1, indexwidth, (movex + (acminimumlength / 2)), movex);
        //            }
        //            else
        //            {
        //                remainlength = edgeXlimit - (Math.Abs(movex));
        //                if (remainlength >= acminimumlength)
        //                {
        //                    if (remainlength >= acoptimumlength)
        //                        retval = Calc(1, indexwidth, -((acoptimumlength / 2) + Math.Abs(movex)), movex);
        //                    else
        //                        retval = Calc(1, indexwidth, -((acminimumlength / 2) + Math.Abs(movex)), movex);
        //                }
        //                else
        //                {
        //                    retval = -1;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"{err.ToString()}.SearchHorJumpIndex() : Error occurred.");
        //        throw err;
        //    }

        //    return retval;
        //}

        //private int SearchVerJumpIndex(int direction, double minimumlength, double optimumlength, double maximumlength, double deadzone)
        //{
        //    int retval = 0;

        //    // direction : 이동 방향
        //    // -1 : 아래쪽
        //    //  1 : 위쪽

        //    double indexwidth = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
        //    double indexheight = Wafer.GetSubsInfo().ActualDieSize.Height.Value;
        //    double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
        //    double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

        //    double acminimumlength = Math.Ceiling(minimumlength / indexwidth) * indexwidth;
        //    double acoptimumlength = Math.Ceiling(optimumlength / indexwidth) * indexwidth;
        //    double acmaximumlength = Math.Ceiling(maximumlength / indexwidth) * indexwidth;

        //    //double movex = ptinfo.GetX() + wafercenterx;
        //    //double movey = ptinfo.GetY() + wafercentery;

        //    double movex = 0 + wafercenterx;
        //    double movey = 0 + wafercentery;

        //    double edgeYlimit = Math.Sqrt(Math.Pow((Wafer.GetPhysInfo().WaferSize_um.Value / 2), 2) - Math.Pow(movex, 2)) - deadzone;

        //    try
        //    {
        //        if (direction == -1) //아래쪽이 남은 길이가 더 짧다.
        //        {
        //            //double remainlenght = Math.Abs(movey - (-edgeYlimit));
        //            double remainlength = edgeYlimit - (Math.Abs(movey));

        //            if (remainlength >= acminimumlength / 2)
        //            {
        //                if (remainlength >= acoptimumlength / 2)
        //                    retval = Calc(1, indexheight, -((acoptimumlength / 2) + Math.Abs(movey)), movey);
        //                else
        //                    retval = Calc(1, indexheight, -((acminimumlength / 2) + Math.Abs(movey)), movey);
        //            }
        //            else
        //            {
        //                //remainlength = Math.Abs(edgeYlimit - movey);
        //                remainlength = edgeYlimit + Math.Abs(movey);

        //                if (remainlength >= acminimumlength)
        //                {
        //                    if (remainlength >= acoptimumlength)
        //                        retval = Calc(1, indexheight, (movey + acminimumlength), movey);
        //                    else
        //                        retval = Calc(1, indexheight, (movey + acoptimumlength), movey);
        //                }
        //                else
        //                {
        //                    retval = -1;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            double remainlength = edgeYlimit + Math.Abs(movey);

        //            if (remainlength >= acminimumlength / 2)
        //            {
        //                if (remainlength >= acoptimumlength / 2)
        //                    retval = Calc(1, indexheight, (movey + (acoptimumlength / 2)), movey);
        //                else
        //                    retval = Calc(1, indexheight, (movey + (acminimumlength / 2)), movey);
        //            }
        //            else
        //            {
        //                remainlength = edgeYlimit - (Math.Abs(movey));

        //                if (remainlength >= acminimumlength)
        //                {
        //                    if (remainlength >= acoptimumlength)
        //                        retval = Calc(1, indexheight, (-acoptimumlength + Math.Abs(movey)), movey);
        //                    else
        //                        retval = Calc(1, indexheight, (-acminimumlength + Math.Abs(movey)), movey);
        //                }
        //                else
        //                {
        //                    retval = -1;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {

        //        LoggerManager.Debug($"{err.ToString()}.SearchHorJumpIndex() Error occurred.");
        //        throw err;
        //    }

        //    return retval;
        //}
    }
}
