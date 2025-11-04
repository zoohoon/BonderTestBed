using LogModule;
using PolishWaferParameters;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PolishWafer;
using ProberInterfaces.WaferAlign;
using ProberInterfaces.WaferAlignEX;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TouchSensorSystemParameter;

namespace PolishWaferFocusingBySensorModule
{
    public class PolishWaferFocusingBySensor_Standard : IPolishWaferFocusingBySensor
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool IsInfo = false;

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

        private TouchSensorSysParameter _TouchSensorParam;
        public TouchSensorSysParameter TouchSensorParam
        {
            get { return _TouchSensorParam; }
            set
            {
                if (value != _TouchSensorParam)
                {
                    _TouchSensorParam = value;
                    RaisePropertyChanged();
                }
            }
        }
         
        public EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param, bool manualmode = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            EventCodeEnum tmpRetval = EventCodeEnum.UNDEFINED;
            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Focusing_Start);

            try
            {
                WaferCoordinate tmpresult = new WaferCoordinate();

                TouchSensorParam = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;

                if (TouchSensorParam.TouchSensorRegistered.Value != true || TouchSensorParam.TouchSensorBaseRegistered.Value != true ||
                    TouchSensorParam.TouchSensorPadBaseRegistered.Value != true || TouchSensorParam.TouchSensorOffsetRegistered.Value != true)
                {
                    LoggerManager.Debug("Touch Sensor is not registered yet for Polish Wafer");
                    //LoggerManager.PWFocusinglog("Touch Sensor is not registered yet for Polish Wafer");
                    return EventCodeEnum.TOUCH_SENSOR_NOT_READY;
                }

                PolishWaferCleaningParameter _param = param as PolishWaferCleaningParameter;
                IPolishWaferSourceInformation pwinfo = this.StageSupervisor().WaferObject.GetPolishInfo();

                if (manualmode == true)
                {
                    if (_param.Thickness.Value == 0)
                    {
                        pwinfo.Thickness.Value = this.StageSupervisor().WaferMaxThickness;
                    }
                    else
                    {
                        pwinfo.Thickness.Value = _param.Thickness.Value;
                    }
                }

                //touch sensor base 보상.  
                double diffX = 0.0;
                double diffY = 0.0;
                double diffZ = 0.0;
                if (this.TouchSensorBaseSetupModule().TouchSensorBaseSetupSystemInit(out diffX, out diffY, out diffZ) == EventCodeEnum.NONE)
                {

                    double touchsensordifflimit = 100; //touch sensor offset diff 160um
                    if (diffZ > touchsensordifflimit)
                    {
                        LoggerManager.Debug($"[TouchSensorBaseSetupModule] Touch Sensor Base Pos Tolerance Error :  DiffZ  = {diffZ}um, Tolerance = {touchsensordifflimit}um");
                        return retVal;
                    }
                    TouchSensorParam.SensorPos.Value.X.Value += diffX;
                    TouchSensorParam.SensorPos.Value.Y.Value += diffY;
                    TouchSensorParam.SensorPos.Value.Z.Value += diffZ;

                }
                else
                {
                    TouchSensorParam.TouchSensorOffsetRegistered.Value = false;
                    retVal = EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure;
                    return retVal;
                }

                if (((_param != null) && (pwinfo != null) && (_param.WaferDefineType.Value == pwinfo.DefineName.Value)) || manualmode == true)
                {
                    if (_param.FocusingModuleDllInfo != null)
                    {
                        var fm = this.FocusManager().GetFocusingModel(_param.FocusingModuleDllInfo);
                        if (fm != null)
                        {
                            if (_param.FocusingPos == null)
                                _param.FocusingPos = new ObservableCollection<FocusingPosition>();

                            retVal = this.FocusManager().ValidationFocusParam(_param.FocusParam);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"[PolishWaferFocusing_Standard], DoFocusing() : Focusing parameter is wrong.");
                                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, _param.FocusParam, 300);
                            }
                            
                            retVal = MakeFocusingPosition(_param.FocusingPos, _param.FocusingPointMode.Value, pwinfo.Thickness.Value, pwinfo.Margin.Value, param.FocusParam.FocusingCam.Value);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                ObservableCollection<WaferCoordinate> HeightProfiling = new ObservableCollection<WaferCoordinate>();

                                ICamera CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                                foreach (var light in _param.FocusParam.LightParams)
                                {
                                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                                }

                                foreach (var item in _param.FocusingPos.Select((value, i) => new { i, value }))
                                {
                                    var focuspos = item.value;
                                    var index = item.i;

                                    PinCoordinate pincoord = new PinCoordinate();
                                    WaferCoordinate wcoord = new WaferCoordinate();

                                    wcoord.X.Value = focuspos.Position.X.Value;
                                    wcoord.Y.Value = focuspos.Position.Y.Value;
                                    wcoord.Z.Value = focuspos.Position.Z.Value;

                                    pincoord.X.Value = TouchSensorParam.SensorPos.Value.X.Value;
                                    pincoord.Y.Value = TouchSensorParam.SensorPos.Value.Y.Value;
                                    pincoord.Z.Value = TouchSensorParam.SensorPos.Value.Z.Value;

                                    //move to pad pin 
                                    //if(TouchSensorParam.SensorAutoDetectZCleareance.Value < this.StageSupervisor().WaferMaxThickness)
                                    //{
                                    //    TouchSensorParam.SensorAutoDetectZCleareance.Value = this.StageSupervisor().WaferMaxThickness + 500;
                                    //}

                                    double zcleareance = this.StageSupervisor().WaferMaxThickness;

                                    if (zcleareance < pwinfo.Thickness.Value && pwinfo.Thickness.Value > 0)
                                    {
                                        zcleareance = pwinfo.Thickness.Value;
                                    }

                                    tmpRetval = this.StageSupervisor().StageModuleState.TouchSensorSensingMoveStage(wcoord, pincoord, zcleareance + Math.Abs(TouchSensorParam.SensorAutoDetectZCleareance.Value)); //안전한 높이로 Move
                                    tmpRetval = StageMoveZ(zcleareance - Math.Abs(TouchSensorParam.SensorAutoDetectZCleareance.Value)); //Zcleareance 값 적용해서 최종 Stage Move
                                    LoggerManager.Debug($"[Focusing By Touch Sensor] Sensor AutoDetect Z-Cleraeance Value : {Math.Abs(TouchSensorParam.SensorAutoDetectZCleareance.Value)}", isInfo: IsInfo);

                                    if (tmpRetval == EventCodeEnum.NONE)
                                    {
                                        //expected = tmpresult.GetZ() - (pwinfo.Thickness.Value + 4000);

                                        tmpRetval = DoAutoDetect(index);
                                        if (tmpRetval != EventCodeEnum.NONE)
                                        {
                                            this.StageSupervisor().StageModuleState.ZCLEARED();
                                            retVal = tmpRetval;
                                            LoggerManager.Error($"[Focusing By Touch Sensor] Failed to detect focused height");
                                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED, retVal);
                                            return retVal;
                                        }
                                        else
                                        {
                                            //Read Sensor Z Pos.
                                            tmpRetval = GetStageHeight(index, out tmpresult);
                                            if (tmpRetval == EventCodeEnum.NONE)
                                            {
                                                if (index == 0)
                                                {
                                                    double distance = 500000;
                                                    double edgepos = 0.0;

                                                    edgepos = ((distance / 2) / Math.Sqrt(2));

                                                    //Left
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                        wcoord.GetX() - (distance / 2), wcoord.GetY(), wcoord.GetZ()));
                                                    //Right
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                        wcoord.GetX() + (distance / 2), wcoord.GetY(), wcoord.GetZ()));
                                                    //Upper
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                        wcoord.GetX(), wcoord.GetY() + (distance / 2), wcoord.GetZ()));
                                                    //Lower
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                        wcoord.GetX(), wcoord.GetY() - (distance / 2), wcoord.GetZ()));
                                                    //LeftUpper
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                        wcoord.GetX() - edgepos, wcoord.GetY() + edgepos, wcoord.GetZ()));
                                                    //RightUpper
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                        wcoord.GetX() + edgepos, wcoord.GetY() + edgepos, wcoord.GetZ()));
                                                    //LeftLower
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                        wcoord.GetX() - edgepos, wcoord.GetY() - edgepos, wcoord.GetZ()));
                                                    //RightLower
                                                    HeightProfiling.Add(new WaferCoordinate(
                                                     wcoord.GetX() + edgepos, wcoord.GetY() - edgepos, wcoord.GetZ()));
                                                }
                                                //actual = tmpresult.GetZ();
                                                HeightProfiling.Add(tmpresult);
                                                focuspos.Position.Z.Value = tmpresult.Z.Value;
                                                //focuspos.Position.Z.Value = actual - expected;
                                            }
                                            else
                                            {
                                                LoggerManager.Error($"[Focusing By Touch Sensor] Faild Focusing By Touch Sensor.");
                                                retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                                                return retVal;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"[Focusing By Touch Sensor] Wafer High View Move Error");
                                        LoggerManager.Error($"[{this.GetType().Name}] DoFocusing() : Wafer High View Move Error");
                                        retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                                        return retVal;
                                    }

                                    this.StageSupervisor().StageModuleState.ZCLEARED();
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
                                string focretStr = string.Empty;

                                if (tmpRetval == EventCodeEnum.NONE)
                                {

                                    foreach (var item in _param.FocusingPos.Select((value, i) => new { i, value }))
                                    {
                                        var focuspos = item.value;
                                        var index = item.i;

                                        LoggerManager.Debug($"PolishWaferFocusingBySensor_Standard, DoFocusing() : [{index + 1}] X : {focuspos.Position.X.Value}, Y : {focuspos.Position.Y.Value}, Z : {focuspos.Position.Z.Value}", isInfo: IsInfo);
                                    }

                                    focusMin = _param.FocusingPos.Min(x => x.Position.Z.Value);
                                    focusMax = _param.FocusingPos.Max(x => x.Position.Z.Value);
                                    focusAvg = _param.FocusingPos.Average(x => x.Position.Z.Value);
                                    focusMaxMinDiff = focusMax - focusMin;

                                    LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : [Focusing summary]: Thickness = {pwinfo.Thickness.Value}, Min = {focusMin:F2}, Max = {focusMax:F2}, Avg = {focusAvg:F2}, Max - Min = {focusMaxMinDiff:F2}", isInfo: IsInfo);
                                    
                                }

                                if (_param.FocusingHeightTolerance != null && _param.FocusingHeightTolerance.Value > 0) 
                                {
                                    double diffabs = Math.Abs(pwinfo.Thickness.Value - focusAvg);

                                    if (diffabs > _param.FocusingHeightTolerance.Value)
                                    {
                                        focretStr = "FAIL";
                                        retVal = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;

                                        LoggerManager.Error($"PolishWaferFocusing_Standard, DoFocusing() : {retVal.ToString()}, Abs(Thickness - average) = {diffabs} > Tolerance = {_param.FocusingHeightTolerance.Value} ");

                                    }
                                    else
                                    {
                                        focretStr = "OK";
                                    }

                                    LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : Thickness = {pwinfo.Thickness.Value} , Avg = {focusAvg:F2} , Abs(Thickness - average) = {diffabs} , Tolerance = {_param.FocusingHeightTolerance.Value}, Result : {focretStr}.", isInfo: IsInfo);
                                }
                                else
                                {
                                    if (_param.FocusingHeightTolerance == null)
                                    {

                                        LoggerManager.Error($"PolishWaferFocusing_Standard, DoFocusing() : FocusingRangeTolerance is null.");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : FocusingRangeTolerance value is {_param.FocusingHeightTolerance.Value}", isInfo: IsInfo);
                                    }
                                }

                                if(retVal != EventCodeEnum.POLISHWAFER_FOCUSING_ERROR)
                                {
                                    if (HeightProfiling != null && HeightProfiling.Count > 0)
                                    {
                                        LoggerManager.Error($"PolishWaferFocusing_Standard, DoFocusing() : TotalHeightPoint to PolishWafer : {HeightProfiling.Count}");

                                        foreach (var height in HeightProfiling)
                                        {
                                            this.AddHeighPlanePoint(pwinfo.WaferHeightMapping, new WAHeightPositionParam(height));
                                            LoggerManager.Debug($"PolishWaferFocusing_Standard, DoFocusing() : AddHeightPlanePoint to PolishWafer. X : " + $"{height.GetX()}, Y : {height.GetY()}, Z : {height.GetZ()}", isInfo: IsInfo);
                                            //LoggerManager.PWFocusinglog($"PolishWaferFocusing_Standard, DoFocusing() : AddHeightPlanePoint to PolishWafer. X : " + $"{height.GetX()}, Y : {height.GetY()}, Z : {height.GetZ()}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"Define Name is not matched.");
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
                this.StageSupervisor().TouchSensorObject.SaveSysParameter();

                this.StageSupervisor().StageModuleState.ZCLEARED();
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    try
                    {
                        LoggerManager.Debug($"DoFocusing() : Forced Done. ProberRunMode = {Extensions_IParam.ProberRunMode}");

                        retVal = EventCodeEnum.NONE;

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
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

            return retVal;
        }

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

                if ((pointmode == PWFocusingPointMode.POINT1) || (pointmode == PWFocusingPointMode.POINT5) ||
                    (pointmode == PWFocusingPointMode.POINT9))
                {
                    // Add Center Position : Offset is zero.
                    focusingpos.Add(new FocusingPosition(centerpos, HeightProfilignPosEnum.CENTER));

                    if(pointmode == PWFocusingPointMode.POINT5 || pointmode == PWFocusingPointMode.POINT9)
                    {
                        double WaferSize_um = Wafer.GetPhysInfo().WaferSize_um.Value;

                        double edgepos = 0;

                        edgepos = ((WaferSize_um / 2) / Math.Sqrt(2));

                        double ActualDieSize_X = this.GetParam_Wafer().GetSubsInfo().ActualDieSize.Width.Value;
                        double ActualDieSize_Y = this.GetParam_Wafer().GetSubsInfo().ActualDieSize.Height.Value;

                        ICamera cam = this.VisionManager().GetCam(focusingCam);

                        int grabsize_X = cam.Param.GrabSizeX.Value;
                        int grabsize_Y = cam.Param.GrabSizeY.Value;

                        double margin_X = PW_margin + (grabsize_X / 2) + ActualDieSize_X;
                        double margin_Y = PW_margin + (grabsize_Y / 2) + ActualDieSize_Y;

                        FocusingPosition left = new FocusingPosition(new WaferCoordinate(centerpos.X.Value - edgepos + margin_X, centerpos.Y.Value, centerpos.Z.Value), HeightProfilignPosEnum.LEFT);
                        FocusingPosition right = new FocusingPosition(new WaferCoordinate(centerpos.X.Value + edgepos - margin_X, centerpos.Y.Value, centerpos.Z.Value), HeightProfilignPosEnum.RIGHT);
                        FocusingPosition up = new FocusingPosition(new WaferCoordinate(centerpos.X.Value, centerpos.Y.Value + edgepos - margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.UPPER);
                        FocusingPosition down = new FocusingPosition(new WaferCoordinate(centerpos.X.Value, centerpos.Y.Value - edgepos + margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.BOTTOM);

                        focusingpos.Add(left);
                        focusingpos.Add(right);
                        focusingpos.Add(up);
                        focusingpos.Add(down);

                        if (pointmode == PWFocusingPointMode.POINT9)
                        {
                            edgepos = ((WaferSize_um / 2) / 2); //((chucksize / 2) / Math.Sqrt(2)) / Math.Sqrt(2);
                            FocusingPosition leftup = new FocusingPosition(new WaferCoordinate(centerpos.X.Value - edgepos + margin_X, centerpos.Y.Value + edgepos - margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.LEFTUPPER);
                            FocusingPosition rightup = new FocusingPosition(new WaferCoordinate(centerpos.X.Value + edgepos - margin_X, centerpos.Y.Value + edgepos - margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.RIGHTUPPER);
                            FocusingPosition leftdown = new FocusingPosition(new WaferCoordinate(centerpos.X.Value - edgepos + margin_X, centerpos.Y.Value - edgepos + margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.LEFTBOTTOM);
                            FocusingPosition rightdown = new FocusingPosition(new WaferCoordinate(centerpos.X.Value + edgepos - margin_X, centerpos.Y.Value - edgepos + margin_Y, centerpos.Z.Value), HeightProfilignPosEnum.RIGHTBOTTOM);

                            focusingpos.Add(leftup);
                            focusingpos.Add(rightup);
                            focusingpos.Add(leftdown);
                            focusingpos.Add(rightdown);
                        }
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum DoAutoDetect(int index)
        {
            LoggerManager.Debug($"Begin DoAutoDetect()");

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            if (this.MotionManager().IsEmulMode(this.MotionManager().GetAxis(EnumAxisConstants.Z)))
            {
                return EventCodeEnum.NONE;
            }

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return EventCodeEnum.NONE;
                }

                //조금씩 올려서 Z축 Move.
                WaferCoordinate tmpwaferpos_org = new WaferCoordinate();
                WaferCoordinate tmpwaferpos_cur = new WaferCoordinate();
                double orgHeight;
                double curZ;
                bool bFound;
                int detectrange;
                int detectstep;
                double stagemove = 100; // 터치센서 스펙 160um
                //double detectrange = (touchsensorsensing / 10);  //100, 10, 1 (10배)

                RetVal = GetStageHeight(index, out tmpwaferpos_org);
                orgHeight = tmpwaferpos_org.Z.Value;
                LoggerManager.Debug($"DoAutoDetect() : start orgheight Z : {orgHeight}");

                detectstep = (int)(orgHeight / stagemove); //step1. 100um 단위로 이동.
                detectrange = (int)(TouchSensorParam.TouchSensorOffset.Value / stagemove); 
                LoggerManager.Debug($"DoAutoDetect() : Dtect Step : {detectstep}, Detect Range : {detectrange}");

                if (RetVal == EventCodeEnum.NONE)
                {
                    if (this.NeedleCleaner().IsNCSensorON() == true)
                    {
                        bFound = false;

                        // Already touch sensor is ON, move down firstly
                        bFound = StageMove(detectstep + detectrange, -stagemove); //stagedown 100um

                        if (bFound == false)
                        {
                            // Error exception
                            // No response from sensor, break operation
                            LoggerManager.Debug($"touch sensor not false(off)");
                            RetVal = FailureAutoDetect(index, orgHeight);
                            return RetVal;
                        }
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    bFound = false;

                    RetVal = GetStageHeight(index, out tmpwaferpos_cur);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    curZ = tmpwaferpos_cur.Z.Value;

                    detectstep = (int)(curZ / stagemove); //step1. 100um 단위로 이동.
                    detectrange = (int)(TouchSensorParam.TouchSensorOffset.Value / stagemove);
                    LoggerManager.Debug($"DoAutoDetect() : Dtect Step : {detectstep}, Detect Range : {detectrange}");

                    // Already touch sensor is ON, move down firstly
                    bFound = StageMove(detectstep + detectrange, stagemove); //stageup 100um

                    if (bFound == false)
                    {
                        // Error exception
                        // No response from sensor, break operation
                        LoggerManager.Debug($"Could not find sensed height in step 1");
                        RetVal = FailureAutoDetect(index, orgHeight);
                        return RetVal;
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    bFound = false;

                    RetVal = GetStageHeight(index, out tmpwaferpos_cur);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    curZ = tmpwaferpos_cur.Z.Value;

                    detectstep = (int)(curZ / (stagemove/10)); //step1. 10um 단위로 이동.
                    detectrange = (int)(TouchSensorParam.TouchSensorOffset.Value / (stagemove / 10));
                    LoggerManager.Debug($"DoAutoDetect() : Dtect Step : {detectstep}, Detect Range : {detectrange}");

                    bFound = StageMove(detectstep + detectrange, -(stagemove / 10)); //stagedown

                    if (bFound == false)
                    {
                        // Error exception
                        // No response from sensor, break operation
                        LoggerManager.Debug($"Could not find sensed height in step 2");
                        RetVal = FailureAutoDetect(index, orgHeight);
                        return RetVal;
                    }
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    bFound = false;

                    RetVal = GetStageHeight(index, out tmpwaferpos_cur);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    curZ = tmpwaferpos_cur.Z.Value;

                    detectstep = (int)(curZ / (stagemove / 10)); //step1. 1um 단위로 이동.
                    detectrange = (int)(TouchSensorParam.TouchSensorOffset.Value / (stagemove / 10));
                    LoggerManager.Debug($"DoAutoDetect() : Dtect Step : {detectstep}, Detect Range : {detectrange}");

                    bFound = StageMove(detectstep + detectrange, (stagemove / 100));//stageup

                    if (bFound == false)
                    {
                        // Error exception
                        // No response from sensor, break operation
                        LoggerManager.Debug($"Could not find sensed height in step 3");
                        RetVal = FailureAutoDetect(index, orgHeight);
                        return RetVal;
                    }
                    else
                    {
                        var axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                        LoggerManager.Debug($"DoAutoDetect({index}): Position detected. @{axis.Status.Position.Actual:0.00}");
                    }
                }
            }
            catch (Exception err)
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. {this.GetType().Name} DoAutoDetect() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return RetVal;
        }

        public bool StageMove(double detectcount, double movedistance)
        {
            try
            {
                LoggerManager.Debug($"StageDown() : Detect Count : {detectcount}");

                if (detectcount <= 0)
                {
                    // detectcount가 0 이하이면 검출이 실패한 것으로 간주
                    return false;
                }
                else
                {
                    bool retval = false;
                    EventCodeEnum result = StageMoveZ(movedistance); //zdown동작.

                    if (result != EventCodeEnum.NONE)
                    {
                        // 이동 중 오류가 발생한 경우 검출이 실패한 것으로 간주
                        return retval;
                    }

                        
                    if(movedistance < 0)
                    {
                        if (!this.NeedleCleaner().IsNCSensorON()) //sensor OFF
                        {
                            LoggerManager.Debug($"StageDown() : Done");
                            // 센서가 검출되지 않으면 검출이 완료된 것으로 간주
                            retval = true;
                            return retval;
                        }
                        else
                        {
                            return StageMove(detectcount - 1, movedistance);
                        }
                    }
                    else if(movedistance > 0)
                    {
                        if (this.NeedleCleaner().IsNCSensorON()) //sensor ON
                        {
                            LoggerManager.Debug($"StageUp() : Done");
                            // 센서가 검출되면 검출이 성공한 것으로 간주
                            retval = true;
                            return retval;
                        }
                        else
                        { 
                            return StageMove(detectcount - 1, movedistance);
                        }
                    }
                    return retval;
                }
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. NcRelMoveZ() : Error occured.");
                return false;
            }
        }

        public EventCodeEnum FailureAutoDetect(int index, double orgHeight)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                double curZ = 0.0;
                WaferCoordinate tmpwaferpos_cur = new WaferCoordinate();

                EventCodeEnum result = GetStageHeight(index, out tmpwaferpos_cur);
                curZ = tmpwaferpos_cur.Z.Value;
                result = StageMoveZ(orgHeight - curZ);

                retval = EventCodeEnum.TOUCH_SENSOR_DETECTED_ERROR;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FailureAutoDetect() : Error occured.");
                retval = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retval;
        }

        public EventCodeEnum StageMoveZ(double inc)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = this.MotionManager().RelMove(EnumAxisConstants.Z, inc);
                var axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                this.MotionManager().WaitForAxisMotionDone(axis, 30000);
                LoggerManager.Debug($"NcRelMoveZ(Inc = {inc}): Curr. Axis position = {axis.Status.Position.Actual:0.00}");
                System.Threading.Thread.Sleep(100);
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. StageMoveZ() : Error occured.");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return retVal;
        }

        private EventCodeEnum GetStageHeight(int index, out WaferCoordinate wafercoord_cur)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            PinCoordinate pincoord = new PinCoordinate();
            MachineCoordinate mccoord = new MachineCoordinate();

            try
            {
                pincoord.X.Value = TouchSensorParam.SensorPos.Value.X.Value;
                pincoord.Y.Value = TouchSensorParam.SensorPos.Value.Y.Value;
                pincoord.Z.Value = TouchSensorParam.SensorPos.Value.Z.Value;

                mccoord.X.Value = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                mccoord.Y.Value = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                mccoord.Z.Value = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref;

                wafercoord_cur = this.CoordinateManager().WaferHighChuckConvert.GetWaferPosFromAlignedPos(mccoord, pincoord); //주어진 pin 높이 기준 wafercoord 계산.
                //posVal = wafercoord_cur.Z.Value;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                wafercoord_cur = new WaferCoordinate(0,0,0);
                LoggerManager.Debug($"{err.ToString()}.  GetStageHeight() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }
            return RetVal;
        }

        public void AddHeighPlanePoint(WaferHeightMapping heightmapping, WAHeightPositionParam param = null)
        {
            try
            {
                //double radius = 0.0;

                WaferCoordinate PlanePoint = new WaferCoordinate();

                if (param == null)
                {
                    PlanePoint = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
                else
                {
                    param.Position.CopyTo(PlanePoint);
                }

                // TODO : Check logic

                //radius = Wafer.GetSubsInfo().ActualDieSize.Width.Value < Wafer.GetSubsInfo().ActualDieSize.Height.Value ?
                //    Wafer.GetSubsInfo().ActualDieSize.Width.Value : Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                //radius *= 0.8;

                //if (heightmapping.PlanPoints != null)
                //{
                //    for (int i = 0; i < heightmapping.PlanPoints.Count; i++)
                //    {
                //        double pointdist = Point.Subtract(new Point(heightmapping.PlanPoints[i].X.Value, heightmapping.PlanPoints[i].Y.Value), new Point(PlanePoint.X.Value, PlanePoint.Y.Value)).Length;

                //        if (pointdist < radius)
                //        {
                //            heightmapping.PlanPoints.RemoveAt(i);
                //        }
                //    }
                //}

                if (PlanePoint != null)
                {
                    heightmapping.PlanPoints.Add(PlanePoint);
                    //TotalHeightPoint++;
                    //SaveWaferAveThick();

                    //LoggerManager.Debug($"AddHeightPlanePoint to WaferAlign. X : " + $"{PlanePoint.GetX()}, Y : {PlanePoint.GetY()}, Z : {PlanePoint.GetZ()}");
                    //LoggerManager.Debug($"TotalHeightPoint to WaferAlign : {heightmapping.PlanPoints.Count}");
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

    }
}
