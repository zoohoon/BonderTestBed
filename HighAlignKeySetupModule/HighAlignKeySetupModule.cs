using LogModule;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using PinAlignUserControl;
using PnPControl;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using static ProberInterfaces.Param.PinSearchParameter;

namespace HighAlignKeyAngleSetupModule
{
    public class HighAlignKeyAngleSetupModule : PNPSetupBase, ISetup, ITemplateModule, IParamNode, IRecovery, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("B8B468A5-EE7F-4B15-B46F-A5C6652B2A6B");

        private ICamera Cam;

        private SubModuleStateBase _ModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int CurPinIndex = 0;
        private int CurDutIndex = 0;
        private int CurPinArrayIndex = 0;
        private int CurLibPackIndex = 0;

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

        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }

        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        private bool bToogleStateAlignKey = false;  // 현재 PREV/NEXT로 이동할 때 핀 팁을 볼 것인지 얼라인 키를 볼 것인지 토글하는 역할을 한다.

        public EventCodeEnum ClearData()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Execute()
        {
            return EventCodeEnum.NONE;
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.IDLE;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[HighAlignKeySetupModule] InitModule() : InitModule Stert");
            try
            {
                if (Initialized == false)
                {
                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    LoggerManager.Debug($"[HighAlignKeySetupModule] InitModule() : InitModule Done");

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
                //LoggerManager.Error($err + "InitModule() : Error occured.");
            }

            return retval;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Angle Setup";

                retVal = InitPnpModuleStage();
                retVal = InitLightJog(this);

                SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                retVal = await InitSetup();
                InitLightJog(this);

                // Page가 바뀌면 Pin High를 가리키고 있어서 조명값도 그에 맞게 업데이트 해준다.
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                foreach (var light in pininfo.PinSearchParam.LightForTip)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }
                UpdateCameraLightValue();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return retVal;
        }


        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //InitBackupData();

            LoggerManager.Debug($"[HighAlignKeySetupModule] InitSetup() : InitSetup Stert");
            try
            {

                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count < 1)
                {
                    retVal = EventCodeEnum.NODATA;
                    throw new Exception();
                }

                this.StageSupervisor().ProbeCardInfo.CheckValidPinParameters();

                CurPinIndex = 0;
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);
                IPinData tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                if (tmpPinData != null)
                    CurDutIndex = tmpPinData.DutNumber.Value - 1;
                else
                    CurDutIndex = 0;

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                }

                this.StageSupervisor().StageModuleState.PinHighViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);

                bToogleStateAlignKey = false;  // 초기 이동은 핀 위치로 간다.

                Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                CurCam = Cam;

                //foreach (var light in pininfo.PinSearchParam.LightForTip)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}

                this.PnPManager().RestoreLastLightSetting(CurCam);

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);

                UseUserControl = UserControlFucEnum.DEFAULT;

                MainViewTarget = DisplayPort;

                MiniViewTarget = null;


                LoadDevParameter();

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                CurCam.UpdateOverlayFlag = true;

                UpdateLabel();

                LoggerManager.Debug($"[HighAlignKeySetupModule] InitSetup() : InitSetup Done");

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }


        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MainViewTarget = DisplayPort;

                Header = "Angle Setup";

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";
                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                PadJogSelect.Caption = "Toggle";

                // TODO : 키를 여러개 사용하는 모드가 개발되면 추가할 것!

                //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib.Count > 1)
                //{
                //    PadJogRightDown.Caption = "=>";     // Todo for pinalignment : 아이콘으로 변경할 것
                //    PadJogLeftDown.Caption = "<=";


                //    PadJogRightDown.IsEnabled = true;
                //    PadJogLeftDown.IsEnabled = true;
                //}
                //else
                //{
                //    PadJogRightDown.Caption = "";
                //    PadJogLeftDown.Caption = "";
                //    PadJogRightDown.IsEnabled = false;
                //    PadJogLeftDown.IsEnabled = false;
                //}

                PadJogRightDown.IsEnabled = false;
                PadJogLeftDown.IsEnabled = false;

                //PadJogLeftDown.Command = new AsyncCommand(DoPrevLibPack);
                //PadJogRightDown.Command = new AsyncCommand(DoNextLibPack);
                //PadJogLeftDown.Caption = "Prev. Key";
                //PadJogRightDown.Caption = "Next. Key";

                PadJogSelect.Command = new AsyncCommand(DoToggle);
                
                PadJogUp.Caption = "TOP";
                PadJogDown.Caption = "DOWN";
                if (this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    PadJogUp.Command = new AsyncCommand(DoSetAlignKeyPosDown);
                    PadJogDown.Command = new AsyncCommand(DoSetAlignKeyPosUp);
                }
                else
                {
                    PadJogUp.Command = new AsyncCommand(DoSetAlignKeyPosUp);
                    PadJogDown.Command = new AsyncCommand(DoSetAlignKeyPosDown);
                }                
                PadJogUp.RepeatEnable = false;
                PadJogDown.RepeatEnable = false;

                PadJogLeft.Caption = "LEFT";
                PadJogRight.Caption = "RIGHT";
                if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP)
                {
                    PadJogLeft.Command = new AsyncCommand(DoSetAlignKeyPosRight);
                    PadJogRight.Command = new AsyncCommand(DoSetAlignKeyPosLeft);
                }
                else
                {
                    PadJogLeft.Command = new AsyncCommand(DoSetAlignKeyPosLeft);
                    PadJogRight.Command = new AsyncCommand(DoSetAlignKeyPosRight);
                }                    
                PadJogLeft.RepeatEnable = false;
                PadJogRight.RepeatEnable = false;


                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;

                PadJogSelect.IsEnabled = true;
                PadJogUp.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                OneButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Visibility = System.Windows.Visibility.Hidden;

                ThreeButton.Visibility = System.Windows.Visibility.Visible;
                FourButton.Visibility = System.Windows.Visibility.Visible;

                FiveButton.Visibility = System.Windows.Visibility.Hidden;

                OneButton.Caption = "Set All";
                OneButton.Command = Button1Command;

                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                ThreeButton.IconCaption = "ADD";
                ThreeButton.Command = Button3Command;

                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Delete.png");
                FourButton.IconCaption = "DELETE";
                FourButton.Command = Button4Command;

                UseUserControl = UserControlFucEnum.DEFAULT;

                if (this.PinAligner().IsRecoveryStarted == true)
                {
                    FiveButton.Caption = "Show\nResult";
                    FiveButton.Command = Button5Command;
                    FiveButton.Visibility = System.Windows.Visibility.Visible;
                }

                TargetRectangleHeight = 0;
                TargetRectangleWidth = 0;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
        }


        public void UpdateOverlay()
        {
            this.PinAligner().StopDrawDutOverlay(CurCam);
            this.PinAligner().DrawDutOverlay(CurCam); ;
        }
        private Task DoSetAlignKeyPosRight()
        {
            try
            {
                
                SetAlignKeyPosRight();
                //UpdateOverlay();

                this.StageSupervisor().ProbeCardInfo.SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.PinAligner().DrawDutOverlay(CurCam);               
            }
            return Task.CompletedTask;
        }
        private double DegreeToRadian(double angle)
        {
            try
            {
                return Math.PI * angle / 180.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return 0;
            }
        }

        private void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            try
            {
                newx = OriPos.X.Value - RefPos.X.Value;
                newy = OriPos.Y.Value - RefPos.Y.Value;

                NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
                NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Task DoToggle()
        {
            try
            {
                TogglePosition();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }
        private void TogglePosition()
        {
            // 팁과 얼라인 마크의 위치를 서로 토글하여 위치를 전환한다.
            double posX = 0;
            double posY = 0;
            double posZ = 0;

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (bToogleStateAlignKey == true)
                {
                    // 지금 얼라인 키를 보고 있다면 핀 위치로 이동한다. 
                    posX = pininfo.AbsPos.X.Value;
                    posY = pininfo.AbsPos.Y.Value;
                    posZ = pininfo.AbsPos.Z.Value;

                    if (posZ > this.StageSupervisor().PinMaxRegRange)
                    {
                        posZ = this.StageSupervisor().PinMaxRegRange;
                    }

                    this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);

                    foreach (var light in pininfo.PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    bToogleStateAlignKey = false;
                }
                else
                {
                    if (pininfo.PinSearchParam.AlignKeyHigh.Count > 0)
                    {
                        // 지금 핀을 보고 있다면 이 핀의 첫번째 얼라인 마크 위치로 이동한다.
                        // 선택된 얼라인 마크로 이동 하도록 수정됨. 
                        var currKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;
                        posX = pininfo.AbsPos.X.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].AlignKeyPos.X.Value;
                        posY = pininfo.AbsPos.Y.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].AlignKeyPos.Y.Value;
                        posZ = pininfo.AbsPos.Z.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].AlignKeyPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);

                        if (pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].PatternIfo.LightParams != null)
                        {
                            foreach (var light in pininfo.PinSearchParam.AlignKeyHigh[currKeyIndex].PatternIfo.LightParams)
                            {
                                CurCam.SetLight(light.Type.Value, light.Value.Value);
                            }
                        }
                    }

                    bToogleStateAlignKey = true;
                }

                UpdateCameraLightValue();

                UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Task DoPrevLibPack()
        {
            try
            {                
                PrevLibPack();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }

        private void PrevLibPack()
        {
            // 원래 각 핀별로 다양한 형태의 얼라인 키를 가질 수 있다면 여기서 라이브러리에 등록된 리스트 들 중에서 고를 수 있어야 한다.
            // 폼펙터 카드 용 얼라인을 만들게 될 때 필요하겠지만 마이크론에서는 필요없을 듯 하여 일단 스킵.

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                PinSearchParameter CurrentPinSearchParam = pininfo.PinSearchParam;

                var currIndex = CurrentPinSearchParam.AlignKeyIndex.Value;

                if (CurrentPinSearchParam.AlignKeyHigh.Count > 0)
                {
                    if (currIndex >= 1)
                    {
                        currIndex--;
                    }
                    else
                    {
                        currIndex = CurrentPinSearchParam.AlignKeyHigh.Count - 1;
                    }


                    //if (currIndex >= 1)
                    //{
                    //    currIndex--;
                    //}
                    //else
                    //{
                    //    currIndex = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib.Count - 1;
                    //}
                    //if (currIndex < 0)
                    //{
                    //    currIndex = 0;
                    //}

                    CurLibPackIndex = currIndex;

                    CurrentPinSearchParam.AlignKeyIndex.Value = CurLibPackIndex;
                    this.PinAligner().StopDrawDutOverlay(CurCam);
                    this.PinAligner().DrawDutOverlay(CurCam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Task DoNextLibPack()
        {
            try
            {
                NextLibPack();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }
        private void NextLibPack()
        {
            // 원래 각 핀별로 다양한 형태의 얼라인 키를 가질 수 있다면 여기서 라이브러리에 등록된 리스트 들 중에서 고를 수 있어야 한다.
            // 폼펙터 카드 용 얼라인을 만들게 될 때 필요하겠지만 마이크론에서는 필요없을 듯 하여 일단 스킵.

            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                PinSearchParameter CurrentPinSearchParam = pininfo.PinSearchParam;

                var currIndex = CurrentPinSearchParam.AlignKeyIndex.Value;

                if (CurrentPinSearchParam.AlignKeyHigh.Count > 0)
                {
                    if (currIndex < CurrentPinSearchParam.AlignKeyHigh.Count - 1)
                    {
                        currIndex++;
                    }
                    else
                    {
                        currIndex = 0;
                    }

                    //if (currIndex < this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib.Count - 1)
                    //{
                    //    currIndex++;
                    //}
                    //else
                    //{
                    //    currIndex = 0;
                    //}

                    //if(currIndex >= this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib.Count)
                    //{
                    //    currIndex = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib.Count - 1;
                    //}

                    CurLibPackIndex = currIndex;

                    CurrentPinSearchParam.AlignKeyIndex.Value = CurLibPackIndex;
                    this.PinAligner().StopDrawDutOverlay(CurCam);
                    this.PinAligner().DrawDutOverlay(CurCam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void RotateAlignKey(IPinData curPin, double angle)
        {
            //double tmpValX = 0.0;
            //double tmpValY = 0.0;

            // 주의 : 각 돌리는 것은 90도 간격밖에 고려 안 되어 있음. 혹시나 45도 돌리려면 다시 만들어야 함!!!!!

            try
            {
                // 현재 각도와 입력 각도가 다른 경우에만 동작.


                // 현재 선택된 Pin과 해당 핀의 KeyIndex를 얻고, 해당 키의 데이터를 변경한다.

                int currentKeyIndex = curPin.PinSearchParam.AlignKeyIndex.Value;

                if (curPin.PinSearchParam.AlignKeyHigh.Count > 0 && curPin.PinSearchParam.AlignKeyHigh[currentKeyIndex].AlignKeyAngle.Value != angle)
                {
                    // 팁을 기준으로 하여 현재 Angle 위치로 얼라인키를 돌린다. 라이브러리에 저장된 대표핀은 0도를 기준으로 한다 (팁을 기준으로 했을 때 우측 방향에 존재)
                    PinCoordinate NewPos = new PinCoordinate();
                    PinCoordinate OldPos = new PinCoordinate();

                    // Key의 기존 위치
                    OldPos.X.Value = curPin.AbsPos.X.Value + curPin.PinSearchParam.AlignKeyHigh[currentKeyIndex].AlignKeyPos.X.Value;
                    OldPos.Y.Value = curPin.AbsPos.Y.Value + curPin.PinSearchParam.AlignKeyHigh[currentKeyIndex].AlignKeyPos.Y.Value;

                    //OldPos.X.Value = curPin.AbsPos.X.Value + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib[i].AlignKeyPosition.X.Value;
                    //OldPos.Y.Value = curPin.AbsPos.Y.Value + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib[i].AlignKeyPosition.Y.Value;

                    // TODO : 변경되는 각도를 넘겨야 됨.

                    double currentangle = curPin.PinSearchParam.AlignKeyHigh[currentKeyIndex].AlignKeyAngle.Value;
                    double DiffAngle = angle - currentangle;

                    GetRotCoordEx(ref NewPos, OldPos, curPin.AbsPos, DiffAngle);

                    //curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyPos.X.Value = NewPos.X.Value - curPin.AbsPos.X.Value;
                    //curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyPos.Y.Value = NewPos.Y.Value - curPin.AbsPos.Y.Value;

                    // Angle 별 부호로 확인?

                    double DiffX = NewPos.X.Value - curPin.AbsPos.X.Value;
                    double DiffY = NewPos.Y.Value - curPin.AbsPos.Y.Value;

                    curPin.PinSearchParam.AlignKeyHigh[currentKeyIndex].AlignKeyPos.X.Value = DiffX;
                    curPin.PinSearchParam.AlignKeyHigh[currentKeyIndex].AlignKeyPos.Y.Value = DiffY;

                    curPin.PinSearchParam.AlignKeyHigh[currentKeyIndex].AlignKeyAngle.Value = angle;
                }
                else
                {
                    // Key가 존재하지 않는 경우
                }

                // TODO : 각도 변경 시, 자동으로 Blob Size를 변경할 것인지에 따라, 추가 및 변경할 것.

                //if ((Math.Abs(curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyAngle.Value - angle) == 90) ||
                //    (Math.Abs(curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyAngle.Value - angle) == 270))
                //{
                //    // X/Y 사이즈가 서로 바뀐다.
                //    tmpValX = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolX.Value;
                //    tmpValY = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolY.Value;
                //    curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolX.Value = tmpValY;
                //    curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolY.Value = tmpValX;

                //    tmpValX = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeX.Value;
                //    tmpValY = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeY.Value;
                //    curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeX.Value = tmpValY;
                //    curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeY.Value = tmpValX;

                //    tmpValX = curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeX.Value;
                //    tmpValY = curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeY.Value;
                //    curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeX.Value = tmpValY;
                //    curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeY.Value = tmpValX;
                //}

                //curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyAngle.Value = angle;

                //for (int i = 0; i < this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib.Count; i++)
                //{
                //    // 팁을 기준으로 하여 현재 Angle 위치로 얼라인키를 돌린다. 라이브러리에 저장된 대표핀은 0도를 기준으로 한다 (팁을 기준으로 했을 때 우측 방향에 존재)
                //    PinCoordinate NewPos = new PinCoordinate();
                //    PinCoordinate OldPos = new PinCoordinate();
                //    OldPos.X.Value = curPin.AbsPos.X.Value
                //                    + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib[i].AlignKeyPosition.X.Value;
                //    OldPos.Y.Value = curPin.AbsPos.Y.Value
                //                    + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.AlignKeyHighLib[0].AlignKeyLib[i].AlignKeyPosition.Y.Value;

                //    GetRotCoordEx(ref NewPos, OldPos, curPin.AbsPos, angle);

                //    curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyPos.X.Value
                //        = NewPos.X.Value - curPin.AbsPos.X.Value;

                //    curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyPos.Y.Value
                //        = NewPos.Y.Value - curPin.AbsPos.Y.Value;

                //    if ((Math.Abs(curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyAngle.Value - angle) == 90) ||
                //        (Math.Abs(curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyAngle.Value - angle) == 270))
                //    {
                //        // X/Y 사이즈가 서로 바뀐다.
                //        tmpValX = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolX.Value;
                //        tmpValY = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolY.Value;
                //        curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolX.Value = tmpValY;
                //        curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeTolY.Value = tmpValX;

                //        tmpValX = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeX.Value;
                //        tmpValY = curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeY.Value;
                //        curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeX.Value = tmpValY;
                //        curPin.PinSearchParam.AlignKeyHigh[i].BlobSizeY.Value = tmpValX;

                //        tmpValX = curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeX.Value;
                //        tmpValY = curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeY.Value;
                //        curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeX.Value = tmpValY;
                //        curPin.PinSearchParam.AlignKeyHigh[i].FocusingAreaSizeY.Value = tmpValX;
                //    }

                //    curPin.PinSearchParam.AlignKeyHigh[i].AlignKeyAngle.Value = angle;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void SetAlignKeyPosRight()
        {
            double posX = 0.0;
            double posY = 0.0;
            double posZ = 0.0;

            // 얼라인 키를 팁을 기준으로 오른쪽으로 돌린다. 즉, 각도를 0으로 한다.
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (pininfo.PinSearchParam.AlignKeyHigh.Count > 0)
                {
                    RotateAlignKey(pininfo, 0);

                    var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                    if (bToogleStateAlignKey == true)
                    {
                        // 지금 얼라인 키를 보고 있다면 돌린 위치로 이동한다. 첫번째 얼라인 키 위치로 간다
                        posX = pininfo.AbsPos.X.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                        posY = pininfo.AbsPos.Y.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                        posZ = pininfo.AbsPos.Z.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                        this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                    }

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private Task DoSetAlignKeyPosLeft()
        {
            try
            {

                
                SetAlignKeyPosLeft();

                this.StageSupervisor().ProbeCardInfo.SaveDevParameter();

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
                this.PinAligner().DrawDutOverlay(CurCam);
            }
            return Task.CompletedTask;
        }
        private void SetAlignKeyPosLeft()
        {
            double posX = 0.0;
            double posY = 0.0;
            double posZ = 0.0;

            // 얼라인 키를 팁을 기준으로 왼쪽으로 돌린다. 즉, 각도를 180으로 한다.
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (pininfo.PinSearchParam.AlignKeyHigh.Count > 0)
                {
                    RotateAlignKey(pininfo, 180);

                    var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                    if (bToogleStateAlignKey == true)
                    {
                        // 지금 얼라인 키를 보고 있다면 돌린 위치로 이동한다. 첫번째 얼라인 키 위치로 간다
                        posX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.
                            DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.
                               DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                        posY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.
                            DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.
                               DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                        posZ = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.
                            DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value
                               + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.
                               DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                        this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                    }

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Task DoSetAlignKeyPosUp()
        {
            try
            {
                
                SetAlignKeyPosUp();
                this.StageSupervisor().ProbeCardInfo.SaveDevParameter();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
                this.PinAligner().DrawDutOverlay(CurCam);
            }
            return Task.CompletedTask;
        }
        private void SetAlignKeyPosUp()
        {
            double posX = 0.0;
            double posY = 0.0;
            double posZ = 0.0;

            // 얼라인 키를 팁을 기준으로 위쪽으로 돌린다. 즉, 각도를 90으로 한다.
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (pininfo.PinSearchParam.AlignKeyHigh.Count > 0)
                {

                    RotateAlignKey(pininfo, 90);
                    var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                    if (bToogleStateAlignKey == true)
                    {
                        // 지금 얼라인 키를 보고 있다면 돌린 위치로 이동한다. 첫번째 얼라인 키 위치로 간다
                        posX = pininfo.AbsPos.X.Value + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                        posY = pininfo.AbsPos.Y.Value + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                        posZ = pininfo.AbsPos.Z.Value + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                        this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                    }

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Task DoSetAlignKeyPosDown()
        {
            try
            {
                
                SetAlignKeyPosDown();
                this.StageSupervisor().ProbeCardInfo.SaveDevParameter();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
                this.PinAligner().DrawDutOverlay(CurCam);
            }
            return Task.CompletedTask;
        }
        private void SetAlignKeyPosDown()
        {
            double posX = 0.0;
            double posY = 0.0;
            double posZ = 0.0;

            // 얼라인 키를 팁을 기준으로 아래쪽으로 돌린다. 즉, 각도를 270으로 한다.
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (pininfo.PinSearchParam.AlignKeyHigh.Count > 0)
                {
                    RotateAlignKey(pininfo, 270);


                    var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                    if (bToogleStateAlignKey == true)
                    {
                        // 지금 얼라인 키를 보고 있다면 돌린 위치로 이동한다. 첫번째 얼라인 키 위치로 간다
                        posX = pininfo.AbsPos.X.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                        posY = pininfo.AbsPos.Y.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                        posZ = pininfo.AbsPos.Z.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                        this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                    }

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            return retval;
        }

        public override EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public override bool IsParameterChanged(bool issave = false)
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

        public override void SetStepSetupState(string header = null)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public EventCodeEnum SaveDevParameter()
        {
            return SaveProbeCardData();
        }

        public EventCodeEnum StartJob()
        {
            return EventCodeEnum.NONE;
        }

        private async Task DoNext()
        {
            try
            {
                
                await Next();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }
        public async Task Next()
        {
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;
                double posX = 0.0;
                double posY = 0.0;
                double posZ = 0.0;


                Task task = new Task(() =>
                {
                    TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                    if (TotalPinNum <= 0) return;

                    if (CurPinIndex >= TotalPinNum - 1)
                    {
                        CurPinIndex = 0;
                    }
                    else
                    {
                        CurPinIndex++;
                    }
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                    CurDutIndex = tmpPinData.DutNumber.Value - 1;

                    // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                    // 따라서 배열상의 어레이로 변환해 주어야 한다.
                    CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                    IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                    var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                    if (bToogleStateAlignKey == false)
                    {
                        // 핀 위치로 이동
                        posX = pininfo.AbsPos.X.Value;
                        posY = pininfo.AbsPos.Y.Value;
                        posZ = pininfo.AbsPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        foreach (var light in pininfo.PinSearchParam.LightForTip)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }
                    else
                    {
                        // 얼라인 키 위치로 이동
                        posX = pininfo.AbsPos.X.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                        posY = pininfo.AbsPos.Y.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                        posZ = pininfo.AbsPos.Z.Value
                               + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        if (pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams != null)
                        {
                            foreach (var light in pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams)
                            {
                                CurCam.SetLight(light.Type.Value, light.Value.Value);
                            }
                        }
                    }

                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        if (posZ == this.StageSupervisor().PinMaxRegRange)
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, posZ);
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        }
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                });
                task.Start();
                await task;

                UpdateLabel();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err + "Next() : Error occured.");
            }
            //this.StageSupervisor().StageModuleState.PinHighViewMove(ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, ProbeCard.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
        }

        private async Task DoPrev()
        {
            try
            {
                
                await Prev();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }
        public async Task Prev()
        {
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;
                double posX = 0.0;
                double posY = 0.0;
                double posZ = 0.0;

                Task task = new Task(() =>
                {

                    TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                    if (TotalPinNum <= 0) return;

                    if (CurPinIndex <= 0)
                    {
                        CurPinIndex = TotalPinNum - 1;
                    }
                    else
                    {
                        CurPinIndex--;
                    }
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                    CurDutIndex = tmpPinData.DutNumber.Value - 1;

                    // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                    // 따라서 배열상의 어레이로 변환해 주어야 한다.
                    CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                    IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                    if (bToogleStateAlignKey == false)
                    {
                        // 핀 위치로 이동
                        posX = pininfo.AbsPos.X.Value;
                        posY = pininfo.AbsPos.Y.Value;
                        posZ = pininfo.AbsPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        foreach (var light in pininfo.PinSearchParam.LightForTip)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }
                    }
                    else
                    {
                        var curKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;

                            // 얼라인 키 위치로 이동
                            posX = pininfo.AbsPos.X.Value
                                   + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.X.Value;
                            posY = pininfo.AbsPos.Y.Value
                                   + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Y.Value;
                            posZ = pininfo.AbsPos.Z.Value
                                   + pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].AlignKeyPos.Z.Value;

                        if (posZ > this.StageSupervisor().PinMaxRegRange)
                        {
                            posZ = this.StageSupervisor().PinMaxRegRange;
                        }

                        if (pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams != null)
                        {
                            foreach (var light in pininfo.PinSearchParam.AlignKeyHigh[curKeyIndex].PatternIfo.LightParams)
                            {
                                CurCam.SetLight(light.Type.Value, light.Value.Value);
                            }
                        }
                    }

                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(posX, posY, posZ);
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        if (posZ == this.StageSupervisor().PinMaxRegRange)
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, posZ);
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.PinLowViewMove(pininfo.AbsPos.X.Value, pininfo.AbsPos.Y.Value, pininfo.AbsPos.Z.Value);
                        }
                        if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                        {
                            string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(pininfo.PinNum.Value) % 5).ToString() + ".bmp";
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                            this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_LOW_CAM);
                        }
                    }
                });
                task.Start();
                await task;
                UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "Prev() : Error occured.");
            }
        }


        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command)
                    _Button1Command = new AsyncCommand(DoConfirmToSetAll);

                return _Button1Command;
            }
        }

        //private async Task DoConfirmToSetAll()
        //{
        //    try
        //    {
        //        

        //        Task t = Task.Run(async () =>
        //        {
        //            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
        //                "Align mark for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);
        //            if (result == EnumMessageDialogResult.AFFIRMATIVE)
        //            {
        //                SetAll();
        //                CurCam.UpdateOverlayFlag = true;
        //            }
        //        });
        //    }
        //    catch (Exception err)
        //    {

        //    }
        //    finally
        //    {
        //        
        //    }
        //}

        private async Task DoConfirmToSetAll()
        {
            try
            {
                
                //EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                //    "Align mark for all of pins will be updated at once, \nDo you want to proceed?", EnumMessageStyle.AffirmativeAndNegative);

                IPinData SelectedPinInfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];
                int KeyNum = SelectedPinInfo.PinSearchParam.AlignKeyHigh.Count;

                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Confirm To Change All",
                    $"Selected pin {SelectedPinInfo.PinNum}\n" +
                    $"Number of key information : {KeyNum}\n" +
                    $"Key data of all pins is changed to be the same as the information of the selected pin."
                    , EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    SetAll();
                    CurCam.UpdateOverlayFlag = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }

        public void SetAll()
        {
            LoggerManager.Debug($"[HighAlignKeySetupModule] SetAll() : Set All Alignmark Start");

            try
            {
                IPinData SelectedPinInfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                //var curPinIndex = SelectedPinInfo.PinSearchParam.AlignKeyIndex.Value;

                bool ExistKeyData = false;
                int KeyNum = -1;

                KeyNum = SelectedPinInfo.PinSearchParam.AlignKeyHigh.Count;

                if (KeyNum > 0)
                {
                    ExistKeyData = true;
                    
                }
                else
                {
                    ExistKeyData = false;
                }

                // 모든 핀의 Key 데이터를 삭제.

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData pin in dut.PinList)
                    {
                        if (SelectedPinInfo.PinNum.Value != pin.PinNum.Value)
                        {
                            pin.PinSearchParam.AlignKeyHigh.Clear();

                            if (ExistKeyData == true)
                            {
                                for (int k = 0; k < KeyNum; k++)
                                {
                                    AlignKeyInfo tmp = new AlignKeyInfo();

                                    tmp.AlignKeyAngle.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].AlignKeyAngle.Value;

                                    tmp.AlignKeyPos.X.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].AlignKeyPos.X.Value;
                                    tmp.AlignKeyPos.Y.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].AlignKeyPos.Y.Value;
                                    tmp.AlignKeyPos.Z.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].AlignKeyPos.Z.Value;

                                    tmp.BlobSizeX.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeX.Value;
                                    tmp.BlobSizeY.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeY.Value;

                                    tmp.BlobRoiSizeX.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobRoiSizeX.Value;
                                    tmp.BlobRoiSizeY.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobRoiSizeY.Value;

                                    tmp.BlobSizeMinX.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeMinX.Value;
                                    tmp.BlobSizeMinY.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeMinY.Value;

                                    tmp.BlobSizeMaxX.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeMaxX.Value;
                                    tmp.BlobSizeMaxY.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeMaxY.Value;

                                    tmp.BlobSizeMax.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeMax.Value;
                                    tmp.BlobSizeMin.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeMin.Value;

                                    tmp.BlobSizeTolX.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeTolX.Value;
                                    tmp.BlobSizeTolY.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].BlobSizeTolY.Value;

                                    tmp.FocusingAreaSizeX.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].FocusingAreaSizeX.Value;
                                    tmp.FocusingAreaSizeY.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].FocusingAreaSizeY.Value;
                                    tmp.FocusingRange.Value = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].FocusingRange.Value;
                                    tmp.ImageBlobType = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].ImageBlobType;
                                    tmp.ImageObjectColor = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].ImageObjectColor;
                                    tmp.ImageProcType = SelectedPinInfo.PinSearchParam.AlignKeyHigh[k].ImageProcType;

                                    pin.PinSearchParam.AlignKeyHigh.Add(tmp);
                                }
                            }
                        }
                    }
                }

                SaveProbeCardData();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
            LoggerManager.Debug($"[HighAlignKeySetupModule] SetAll() : Set All Search Area Done");
        }

        #endregion

        #region pnp button 2
        #endregion

        #region pnp button 3

        private RelayCommand _Button3Command;
        public ICommand Button3Command
        {
            get
            {
                if (null == _Button3Command)
                    _Button3Command = new RelayCommand(AddKey);
                return _Button3Command;
            }
        }

        public void AddKey()
        {
            try
            {
                double DefaultAngle = 0;
                double DefaultPosX = 50;
                double DefaultPosY = 0;
                double DefaultSizeX = 50;
                double DefaultSizeY = 50;

                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                if (pininfo.PinSearchParam.AlignKeyHigh.Count >= 1)
                {
                    this.MetroDialogManager().ShowMessageDialog("Align Key Add", "Already exist align key. \nOnly one align key can be registered.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    var currentAlignKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;
                    PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                    AlignKeyInfo alignkeyinfo = new AlignKeyInfo();

                    alignkeyinfo.ImageProcType = IMAGE_PROC_TYPE.PROC_BLOB;

                    if (PinAlignParam.EnableAutoThreshold.Value == EnumThresholdType.MANUAL)
                    {
                        alignkeyinfo.ImageBlobType = EnumThresholdType.MANUAL;
                    }
                    else
                    {
                        alignkeyinfo.ImageBlobType = EnumThresholdType.AUTO;
                    }

                    alignkeyinfo.ImageObjectColor = PIN_OBJECT_COLOR.WHITE;
                    alignkeyinfo.AlignKeyAngle.Value = DefaultAngle;
                    alignkeyinfo.AlignKeyPos = new PinCoordinate(DefaultPosX, DefaultPosY);

                    alignkeyinfo.FocusingAreaSizeX.Value = DefaultPosX * 2;
                    alignkeyinfo.FocusingAreaSizeY.Value = DefaultPosY * 2;

                    alignkeyinfo.FocusingRange.Value = 200;

                    alignkeyinfo.BlobSizeX.Value = DefaultSizeX;
                    alignkeyinfo.BlobSizeY.Value = DefaultSizeY;

                    alignkeyinfo.BlobSizeMinX.Value = alignkeyinfo.BlobSizeX.Value / 2.0;
                    alignkeyinfo.BlobSizeMinY.Value = alignkeyinfo.BlobSizeY.Value / 2.0;

                    alignkeyinfo.BlobSizeMaxX.Value = alignkeyinfo.BlobSizeX.Value;
                    alignkeyinfo.BlobSizeMaxY.Value = alignkeyinfo.BlobSizeY.Value;

                    alignkeyinfo.BlobSizeMin.Value = alignkeyinfo.BlobSizeMinX.Value * alignkeyinfo.BlobSizeMinY.Value;
                    alignkeyinfo.BlobSizeMax.Value = alignkeyinfo.BlobSizeMaxX.Value * alignkeyinfo.BlobSizeMaxY.Value;

                    alignkeyinfo.BlobSizeTolX.Value = 3;
                    alignkeyinfo.BlobSizeTolY.Value = 3;

                    alignkeyinfo.BlobRoiSizeX.Value = alignkeyinfo.BlobSizeX.Value * 2;
                    alignkeyinfo.BlobRoiSizeY.Value = alignkeyinfo.BlobSizeY.Value * 2;

                    alignkeyinfo.BlobThreshold.Value = 127;
                    //alignkeyinfo.PatternIfo

                    CurrentPinSerachParam.AlignKeyHigh.Add(alignkeyinfo);

                    // 추가 된 마지막 인덱스로 이동
                    CurrentPinSerachParam.AlignKeyIndex.Value = CurrentPinSerachParam.AlignKeyHigh.Count - 1;

                    SaveProbeCardData();

                    CurCam.UpdateOverlayFlag = true;

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
        }

        #endregion

        #region pnp button 4

        private RelayCommand _Button4Command;
        public ICommand Button4Command
        {
            get
            {
                if (null == _Button4Command)
                    _Button4Command = new RelayCommand(DeleteKey);
                return _Button4Command;
            }
        }

        public void DeleteKey()
        {
            try
            {
                IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

                var currentAlignKeyIndex = pininfo.PinSearchParam.AlignKeyIndex.Value;
                PinSearchParameter CurrentPinSerachParam = pininfo.PinSearchParam;

                if (CurrentPinSerachParam.AlignKeyHigh.Count > 0)
                {
                    CurrentPinSerachParam.AlignKeyHigh.RemoveAt(CurrentPinSerachParam.AlignKeyIndex.Value);

                    // 키가 남아 있다면, 그 이전 인덱스로 변경할 것.

                    if (CurrentPinSerachParam.AlignKeyHigh.Count > 0)
                    {
                        if (CurrentPinSerachParam.AlignKeyIndex.Value == 0)
                        {
                            // 마지막 인덱스로 이동

                            CurrentPinSerachParam.AlignKeyIndex.Value = CurrentPinSerachParam.AlignKeyHigh.Count - 1;
                        }
                        else
                        {
                            CurrentPinSerachParam.AlignKeyIndex.Value = CurrentPinSerachParam.AlignKeyIndex.Value - 1;
                        }
                    }
                }

                SaveProbeCardData();

                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "SetAll() : Error occured.");
            }
        }

        #endregion

        #region pnp button 5
        private RelayCommand _Button5Command;
        public ICommand Button5Command
        {
            get
            {
                if (null == _Button5Command)
                    _Button5Command = new RelayCommand(DoShowResult);
                return _Button5Command;
            }
        }

        private void DoShowResult()
        {
            try
            {
                //
                ShowResult();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                //
            }
        }

        private void ShowResult()
        {
            UcPinAlignResult ucPinResult = new UcPinAlignResult();

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #endregion

        public override async Task Save()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Vision Setup Save",
                    "Do you want to save the contents of the Vision Setup ?", EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    foreach (var module in this.PinAligner().Template)
                    {
                        if (module.GetType().GetInterfaces().Contains(typeof(IHasDevParameterizable)))
                        {
                            ((IHasDevParameterizable)module).SaveDevParameter();
                        }
                    }

                    this.SaveParameter(((IParam)this.StageSupervisor().WaferObject));

                    await this.ViewModelManager().BackPreScreenTransition();
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Debug($err);
            }
        }

        public override async Task Exit()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Vision Setup Exit",
                   "If you exit now, Setup will exit without saving.", EnumMessageStyle.AffirmativeAndNegative);
                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    await this.ViewModelManager().BackPreScreenTransition();
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Debug($err);
            }
        }

        public EventCodeEnum SaveProbeCardData()
        {
            EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;

            LoggerManager.Debug($"[HighAlignKeySetupModule] SaveProbeCardData() : Save Start");
            //UpdateData();

            try
            {
                serialRes = this.StageSupervisor().SaveProberCard();

                if (serialRes == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Fail.");
                }
                LoggerManager.Debug($"[HighAlignKeySetupModule] SaveProbeCardData() : Save Done");
            }
            catch (Exception err)
            {
                throw err;
            }
            return serialRes;
        }


        public bool IsExecute()
        {
            return true;
        }

        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = DoRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public MovingStateEnum GetMovingState()
        {
            return MovingStateEnum.STOP;
        }

        public EventCodeEnum ExitRecovery()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoExecute()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoClearData()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoRecovery()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoExitRecovery()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetNodeSetupState(EnumMoudleSetupState.NONE);
                //this.PinAligner().StopDrawPinOverlay(CurCam);
                this.PinAligner().StopDrawDutOverlay(CurCam);
                retVal = await base.Cleanup(parameter);

                if (retVal == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                }

                this.PnPManager().RememberLastLightSetting(CurCam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            throw new NotImplementedException();
        }

        public override void UpdateLabel()
        {
            IPinData pininfo = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex];

            //// 핀을 보고 있을 때 
            //if (bToogleStateAlignKey == true)
            //{

            //}
            //else // 키를 보고 있을 때 
            //{

            //}

            if (pininfo.PinSearchParam.AlignKeyHigh.Count > 0)
            {
                int index = pininfo.PinSearchParam.AlignKeyIndex.Value;
                double angle = pininfo.PinSearchParam.AlignKeyHigh[index].AlignKeyAngle.Value;
                int flippedangle = int.Parse(((int)angle).ToString());
                string angleStr = string.Empty;

                if (this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP &&
                    this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP)
                {
                    flippedangle += 180;
                    if (flippedangle >= 360)
                    {
                        flippedangle -= 360;
                    }
                }

                if (flippedangle == 0)
                {
                    angleStr = "RIGHT";
                }
                else if (flippedangle == 90)
                {
                    angleStr = "TOP";
                }
                else if (flippedangle == 180)
                {
                    angleStr = "LEFT";
                }
                else if (flippedangle == 270)
                {
                    angleStr = "DOWN";
                }
                else
                {
                    angleStr = string.Empty;
                    LoggerManager.Debug($"Angle is wrong.");
                }

                StepLabel = $"Registered key, Angle({angleStr})";
            }
            else
            {
                StepLabel = $"Not registered key";
            }
        }
    }
}
