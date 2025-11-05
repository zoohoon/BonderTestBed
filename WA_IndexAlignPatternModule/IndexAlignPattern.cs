using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAIndexAlignPatternModule
{
    using LogModule;
    using PnPControl;
    using PnPontrol.UserModelBases;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Align;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using ProberInterfaces.Vision;
    using ProberInterfaces.WaferAlignEX;
    using RelayCommandBase;
    using WA_IndexAlignParameter_Pattern;
    using System.Collections.ObjectModel;
    using ProberInterfaces.WaferAlignEX.Enum;
    using System.IO;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using MetroDialogInterfaces;
    using System.Threading;

    [Serializable]
    public class IndexAlignPattern : PNPSetupBase, IProcessingModule, INotifyPropertyChanged, ISetup, IRecovery, IHasAdvancedSetup
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("35FA3C7E-9397-EBE3-645D-5D3303CFB3B2");
        //public IParam DevParam { get; set; }

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

        public new List<object> Nodes { get; set; }
        public SubModuleStateBase SubModuleState { get; set; }
        public SubModuleMovingStateBase MovingState { get; set; }
        public IParam IndexAlignPatternParam_IParam { get; set; }
        public WA_IndexAlignParam_Pattern IndexAlignPatternParam_Clone;

        private List<WaferCoordinate> EdgePos = new List<WaferCoordinate>();
        private int _CurEdgePosIndex = 0;

        private int _CurPatternIndex = 0;
        private int _PatternCount = 0;

        public PMParameter SettingPMParam = new PMParameter();

        public IndexAlignPattern()
        {

        }
        public IndexAlignPattern(IStateModule Module)
        {

        }

        public bool IsExecute() //SubModule이 Processing 가능한지 판단하는 조건 
        {
            return true;
        }

        #region Don`t Touch Code
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }
        public EventCodeEnum ExitRecovery()
        {

            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        #endregion

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ProberStation = this.ProberStation();

                    SubModuleState = new SubModuleIdleState(this);
                    MovingState = new SubModuleStopState(this);
                    SetupState = new NotCompletedState(this);

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

        public new void DeInitModule()
        {
        }

        public new void CloseAdvanceSetupView()
        {
            try
            {
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            IParam tmpParam = null;
            tmpParam = new WA_IndexAlignParam_Pattern();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            RetVal = this.LoadParameter(ref tmpParam, typeof(WA_IndexAlignParam_Pattern));

            if (RetVal == EventCodeEnum.NONE)
            {
                IndexAlignPatternParam_IParam = tmpParam;
                IndexAlignPatternParam_Clone = IndexAlignPatternParam_IParam as WA_IndexAlignParam_Pattern;
            }

            return RetVal;
        }
        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IndexAlignPatternParam_Clone != null)
                {
                    if (PnpManager.SeletedStep.Header == this.Header & CurCam != null)
                    {
                        foreach (var pattern in IndexAlignPatternParam_Clone.Patterns.Value)
                        {
                            if (pattern.Imagebuffer != null)
                            {
                                var filePathPrefix = this.FileManager().GetDeviceParamFullPath();
                                this.VisionManager().SavePattern(pattern, filePathPrefix);
                            }
                        }
                    }
                }

                if (IndexAlignPatternParam_IParam != null)
                {
                    if (ParamValidation() == EventCodeEnum.NONE)
                    {
                        IndexAlignPatternParam_IParam = IndexAlignPatternParam_Clone;
                        retVal = this.SaveParameter(IndexAlignPatternParam_IParam);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }



        /// <summary>
        /// 현재 모듈의 PNP가 화면에 뜰때마다 호출되는 함수
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "IndexAlign_Pattern";


                retVal = InitLightJog(this, IndexAlignPatternParam_Clone.CamType);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }


            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                InitPnpModuleStage_AdvenceSetting();
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                if (this.WaferAligner().IsNewSetup & (this.WaferAligner().GetIsModify() == false))
                    return await InitSetup();
                else
                    return await InitRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }



        /// <summary>
        /// Setup시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitPnpUI();

                CurCam = this.VisionManager().GetCam(IndexAlignPatternParam_Clone.CamType);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                MainViewTarget = DisplayPort;
                MiniViewTarget = WaferObject;

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                double edgepos = 0.0;
                edgepos = ((WaferObject.GetPhysInfo().WaferSize_um.Value / 2) / Math.Sqrt(2));

                EdgePos.Clear();
                EdgePos.Add(new WaferCoordinate(edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, -edgepos));
                EdgePos.Add(new WaferCoordinate(edgepos, -edgepos));

                _CurEdgePosIndex = 0;
                _CurPatternIndex = 0;
                _PatternCount = IndexAlignPatternParam_Clone.Patterns.Value.Count;


                if (IndexAlignPatternParam_Clone.Patterns.Value.Count != 0)
                {
                    _CurPatternIndex = 1;
                    if (IndexAlignPatternParam_Clone.Patterns.Value[0].CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetZ() + WaferObject.GetSubsInfo().WaferCenter.GetZ()));
                    else if (IndexAlignPatternParam_Clone.Patterns.Value[0].CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetZ() + WaferObject.GetSubsInfo().WaferCenter.GetZ()));
                }
                else
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[0].GetX(), EdgePos[1].GetY(), WaferObject.GetSubsInfo().AveWaferThick);

                StepLabel = String.Format("PATTERN  {0}/{1}", _CurPatternIndex, _PatternCount);

                ParamValidation();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        /// <summary>
        /// Recovery시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IndexAlignPatternParam_Clone = new WA_IndexAlignParam_Pattern(IndexAlignPatternParam_IParam as WA_IndexAlignParam_Pattern);

                InitPnpUI();

                PadJogSelect.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PnpResume.png");

                PadJogSelect.Command = new AsyncCommand(Resume);


                CurCam = this.VisionManager().GetCam(IndexAlignPatternParam_Clone.CamType);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                MainViewTarget = DisplayPort;
                MiniViewTarget = WaferObject;

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                double edgepos = 0.0;
                edgepos = ((WaferObject.GetPhysInfo().WaferSize_um.Value / 2) / Math.Sqrt(2));

                EdgePos.Clear();
                EdgePos.Add(new WaferCoordinate(edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, -edgepos));
                EdgePos.Add(new WaferCoordinate(edgepos, -edgepos));

                _CurEdgePosIndex = 0;
                _CurPatternIndex = 0;
                _PatternCount = IndexAlignPatternParam_Clone.Patterns.Value.Count;


                if (IndexAlignPatternParam_Clone.Patterns.Value.Count != 0)
                {
                    _CurPatternIndex = 1;
                    if (IndexAlignPatternParam_Clone.Patterns.Value[0].CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetZ() + WaferObject.GetSubsInfo().WaferCenter.GetZ()));
                    else if (IndexAlignPatternParam_Clone.Patterns.Value[0].CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY()),
                            (IndexAlignPatternParam_Clone.Patterns.Value[0].GetZ() + WaferObject.GetSubsInfo().WaferCenter.GetZ()));
                }
                else
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[0].GetX(), EdgePos[1].GetY(), WaferObject.GetSubsInfo().AveWaferThick);

                StepLabel = String.Format("PATTERN  {0}/{1}", _CurPatternIndex, _PatternCount);

                ParamValidation();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        internal static class ResourceAccessor
        {
            public static ImageSource Get(System.Drawing.Bitmap bitmap)
            {
                BitmapImage image = new BitmapImage();

                using (MemoryStream ms = new MemoryStream())
                {
                    (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    image.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.StreamSource = null;
                    image.Freeze();
                }
                return image;
            }
        }

        private EventCodeEnum InitPnpUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {


                OneButton.SetIconSoruceBitmap(Properties.Resources.Dirc_RightUpper);
                OneButton.Command = new AsyncCommand(RegisteRightUpperPTCommand);

                TwoButton.SetIconSoruceBitmap(Properties.Resources.Dirc_LeftUpper);
                TwoButton.Command = new AsyncCommand(RegisteLeftUpperPTCommand);

                ThreeButton.SetIconSoruceBitmap(Properties.Resources.Dirc_LeftBottom);
                ThreeButton.Command = new AsyncCommand(RegisteLeftBottomPTCommand);

                FourButton.SetIconSoruceBitmap(Properties.Resources.Dirc_RightBottom);
                FourButton.Command = new AsyncCommand(RegisteRightBottomPTCommand);


                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");

                PadJogLeft.Command = new RelayCommand(UCDisplayRectWidthMinus);
                PadJogRight.Command = new RelayCommand(UCDisplayRectWidthPlus);
                PadJogUp.Command = new RelayCommand(UCDisplayRectHeightPlus);
                PadJogDown.Command = new RelayCommand(UCDisplayRectHeightMinus);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;


                PadJogLeftUp.SetIconSoruceBitmap(Properties.Resources.ArrowLeft_PT);
                PadJogLeftUp.Command = new RelayCommand(PrevPattern);

                PadJogRightUp.SetIconSoruceBitmap(Properties.Resources.ArrowRight_PT);
                PadJogRightUp.Command = new RelayCommand(NextPattern);

                PadJogLeftDown.SetIconSoruceBitmap(Properties.Resources.ArrowLeft_Edge);
                PadJogLeftDown.Command = new RelayCommand(PrevEdge);

                PadJogRightDown.SetIconSoruceBitmap(Properties.Resources.ArrowRight_Edge);
                PadJogRightDown.Command = new RelayCommand(NextEdge);

                AdvanceSetupView = new UC.UC_IndexAlign_Pattern(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        #region //..Command Method

        private void PrevPattern()
        {
            try
            {
                if (_CurPatternIndex - 1 >= 0)
                {
                    _CurPatternIndex--;
                    StepLabel = String.Format("PATTERN  {0}/{1}", _CurPatternIndex + 1, _PatternCount);
                    this.StageSupervisor().StageModuleState.WaferLowViewMove
                        (IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].GetX()
                        + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].GetY()
                        + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                        IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].GetZ()
                        + WaferObject.GetSubsInfo().WaferCenter.GetZ());

                    foreach (var light in IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].LightParams)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private void NextPattern()
        {
            try
            {
                if (_CurPatternIndex + 1 < IndexAlignPatternParam_Clone.Patterns.Value.Count)
                {
                    _CurPatternIndex++;
                    StepLabel = String.Format("PATTERN  {0}/{1}", _CurPatternIndex + 1, _PatternCount);
                    this.StageSupervisor().StageModuleState.WaferLowViewMove
                        (IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].GetX()
                        + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].GetY()
                        + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                        IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].GetZ()
                        + WaferObject.GetSubsInfo().WaferCenter.GetZ());

                    foreach (var light in IndexAlignPatternParam_Clone.Patterns.Value[_CurPatternIndex].LightParams)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private void PrevEdge()
        {
            try
            {
                if (_CurEdgePosIndex - 1 >= 0)
                {
                    _CurEdgePosIndex--;
                    this.StageSupervisor().StageModuleState.WaferLowViewMove
                        (EdgePos[_CurEdgePosIndex].GetX()
                        + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        EdgePos[_CurEdgePosIndex].GetY()
                        + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                        EdgePos[_CurEdgePosIndex].GetZ()
                        + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private void NextEdge()
        {
            try
            {
                if (_CurEdgePosIndex + 1 < EdgePos.Count)
                {
                    _CurEdgePosIndex++;
                    this.StageSupervisor().StageModuleState.WaferLowViewMove
                        (EdgePos[_CurEdgePosIndex].GetX()
                        + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        EdgePos[_CurEdgePosIndex].GetY()
                        + WaferObject.GetSubsInfo().WaferCenter.GetY(),
                        EdgePos[_CurEdgePosIndex].GetZ()
                        + WaferObject.GetSubsInfo().WaferCenter.GetZ());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private async Task RegisteLeftUpperPTCommand()
        {
            await RegistePattern(EnumIndexAlignDirection.LEFTUPPER);
        }


        private async Task RegisteRightUpperPTCommand()
        {
            await RegistePattern(EnumIndexAlignDirection.RIGHTUPPER);
        }


        private async Task RegisteRightBottomPTCommand()
        {
            await RegistePattern(EnumIndexAlignDirection.RIGHTLOWER);
        }

        private async Task RegisteLeftBottomPTCommand()
        {
            //await Task.Run(() =>
            //{
            //    RegistePattern(EnumIndexAlignDirection.LEFTLOWER);
            //});
            await RegistePattern(EnumIndexAlignDirection.LEFTLOWER);
        }


        public async Task Resume()
        {
            if (IndexAlignPatternParam_Clone.Patterns.Value.Count == 4)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //retVal = await Task.Run(() => SubModuleState.Execute());

                Task task = new Task(() =>
                {
                    retVal = SubModuleState.Execute();
                });
                task.Start();
                await task;
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = ParamValidation();
                }
            }

        }


        #endregion


        public EventCodeEnum DoExecute() //실제 프로세싱 하는 코드
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                MovingState.Moving();

                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Pattern_Start);

                WA_IndexAlignParam_Pattern procparam = IndexAlignPatternParam_IParam as WA_IndexAlignParam_Pattern;

                for (int index = 0; index < procparam.Patterns.Value.Count; index++)
                {
                    RetVal = Processing(procparam.Patterns.Value[index]);
                    if (RetVal != EventCodeEnum.NONE)
                        break;
                }


                if (RetVal == EventCodeEnum.NONE)
                {
                    SubModuleState = new SubModuleDoneState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Pattern_OK);
                }
                else if (RetVal == EventCodeEnum.SUB_RECOVERY)
                {
                    SubModuleState = new SubModuleRecoveryState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Index_Align_Pattern_Failure, RetVal);
                    this.NotifyManager().Notify(EventCodeEnum.WAFER_INDEX_ALIGN_FAIL);
                }
                else if (RetVal == EventCodeEnum.SUB_SKIP)
                {
                    SubModuleState = new SubModuleSkipState(this);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Wafer_Index_Align_Pattern_OK);
                }
                else
                {
                    SubModuleState = new SubModuleErrorState(this);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Index_Align_Pattern_Failure, RetVal);
                    this.NotifyManager().Notify(EventCodeEnum.WAFER_INDEX_ALIGN_FAIL);
                }

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Wafer_Index_Align_Pattern_Failure, RetVal);
                this.NotifyManager().Notify(EventCodeEnum.WAFER_INDEX_ALIGN_FAIL);

                throw err;
            }
            finally
            {
                MovingState.Stop();
            }

            return RetVal;
        }
        public EventCodeEnum DoClearData() //현재 Parameter Check 및 Init하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ParamValidation();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
            //  return ParamValidation();
        }
        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP)
                {

                    if (IndexAlignPatternParam_Clone.Patterns.Value.Count == 4
                        && IndexAlignPatternParam_Clone.Patterns.Value.ToList<WA_IAStnadrdPTInfomation>().FindAll
                        (info => info.PatternState.Value == PatternStateEnum.FAILED).Count == 0)
                        retVal = EventCodeEnum.NONE;
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Extensions_IParam.ElementStateNeedSetupValidation(IndexAlignPatternParam_Clone as WA_IndexAlignParam_Pattern);
                    }
                }
                else
                {
                    SetDefaultLotParam();

                    if ((IndexAlignPatternParam_IParam as WA_IndexAlignParam_Pattern).Patterns.Value.Count == 4
                       && ((IndexAlignPatternParam_IParam as WA_IndexAlignParam_Pattern)).Patterns.Value.ToList<WA_IAStnadrdPTInfomation>().FindAll
                       (info => info.PatternState.Value == PatternStateEnum.FAILED).Count == 0)
                        retVal = EventCodeEnum.NONE;
                    if (retVal == EventCodeEnum.NONE)
                    {


                    }
                }


                if (retVal == EventCodeEnum.NONE)
                {
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
                else
                {
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void SetDefaultLotParam()
        {
            try
            {
                WA_IndexAlignParam_Pattern param = IndexAlignPatternParam_IParam as WA_IndexAlignParam_Pattern;
                param.Patterns.Value.Clear();

                double wSize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value;

                ObservableCollection<EnumIndexAlignDirection> directions = new ObservableCollection<EnumIndexAlignDirection>();
                directions.Add(EnumIndexAlignDirection.UPPER);
                directions.Add(EnumIndexAlignDirection.RIGHT);
                directions.Add(EnumIndexAlignDirection.LOWER);
                param.Patterns.Value.Add(new WA_IAStnadrdPTInfomation(EnumIndexAlignDirection.RIGHTUPPER, directions));

                directions = new ObservableCollection<EnumIndexAlignDirection>();
                directions.Add(EnumIndexAlignDirection.UPPER);
                directions.Add(EnumIndexAlignDirection.LEFT);
                directions.Add(EnumIndexAlignDirection.LOWER);
                param.Patterns.Value.Add(new WA_IAStnadrdPTInfomation(EnumIndexAlignDirection.LEFTUPPER, directions));

                directions = new ObservableCollection<EnumIndexAlignDirection>();
                directions.Add(EnumIndexAlignDirection.LOWER);
                directions.Add(EnumIndexAlignDirection.LEFT);
                directions.Add(EnumIndexAlignDirection.UPPER);
                param.Patterns.Value.Add(new WA_IAStnadrdPTInfomation(EnumIndexAlignDirection.LEFTLOWER, directions));

                directions = new ObservableCollection<EnumIndexAlignDirection>();
                directions.Add(EnumIndexAlignDirection.LOWER);
                directions.Add(EnumIndexAlignDirection.RIGHT);
                directions.Add(EnumIndexAlignDirection.UPPER);
                param.Patterns.Value.Add(new WA_IAStnadrdPTInfomation(EnumIndexAlignDirection.RIGHTLOWER, directions));


                for (int index = 0; index < param.Patterns.Value.Count; index++)
                {
                    long xindex = 0;
                    long yindex = 0;

                    bool flag = true;

                    switch (param.Patterns.Value[index].Direction)
                    {
                        case EnumIndexAlignDirection.RIGHTUPPER:

                            for (long indexY = WaferObject.GetPhysInfo().MapCountY.Value; indexY > (WaferObject.GetPhysInfo().MapCountY.Value / 2); indexY--)
                            {
                                xindex = FindEdgeTestDie(indexY, param.Patterns.Value[index].Direction);
                                if (xindex != -1)
                                {
                                    yindex = indexY;
                                    break;
                                }
                            }

                            break;
                        case EnumIndexAlignDirection.LEFTUPPER:
                            for (long indexY = WaferObject.GetPhysInfo().MapCountY.Value; indexY > (WaferObject.GetPhysInfo().MapCountY.Value / 2); indexY--)
                            {
                                xindex = FindEdgeTestDie(indexY, param.Patterns.Value[index].Direction);
                                if (xindex != -1)
                                {
                                    yindex = indexY;
                                    break;
                                }
                            }

                            break;
                        case EnumIndexAlignDirection.LEFTLOWER:

                            for (long indexY = 0; indexY < (WaferObject.GetPhysInfo().MapCountY.Value / 2); indexY++)
                            {
                                xindex = FindEdgeTestDie(indexY, param.Patterns.Value[index].Direction);
                                if (xindex != -1)
                                {
                                    yindex = indexY;
                                    break;
                                }
                            }

                            break;
                        case EnumIndexAlignDirection.RIGHTLOWER:

                            for (long indexY = 0; indexY < (WaferObject.GetPhysInfo().MapCountY.Value / 2); indexY++)
                            {
                                xindex = FindEdgeTestDie(indexY, param.Patterns.Value[index].Direction);
                                if (xindex != -1)
                                {
                                    yindex = indexY;
                                    break;
                                }
                            }

                            break;
                    }
                    while (flag)
                    {

                        IDeviceObject device = WaferObject.GetDevices().Find(info => info.DieIndexM.XIndex == xindex && info.DieIndexM.YIndex == yindex);
                        if (device != null)
                        {
                            EventCodeEnum ret = ValidationEdgePosition(device, param.Patterns.Value[index].Directions.ToList<EnumIndexAlignDirection>());
                            if (ret == EventCodeEnum.NONE)
                            {
                                flag = false;
                                //WaferCoordinate coordinate = this.WaferAligner().MIndexToWPos(Convert.ToInt32(xindex), Convert.ToInt32(yindex), true);
                                WaferCoordinate coordinate = this.WaferAligner().MachineIndexConvertToDieCenter(Convert.ToInt32(xindex), Convert.ToInt32(yindex));
                                param.Patterns.Value[index].X.Value = coordinate.GetX();
                                param.Patterns.Value[index].Y.Value = coordinate.GetY();
                                param.Patterns.Value[index].Z.Value = WaferObject.GetPhysInfo().Thickness.Value;
                                param.Patterns.Value[index].CamType.Value = EnumProberCam.WAFER_HIGH_CAM;
                            }
                        }

                        if (flag)
                        {
                            switch (param.Patterns.Value[index].Direction)
                            {
                                case EnumIndexAlignDirection.RIGHTUPPER:
                                    xindex = FindEdgeTestDie(yindex--, param.Patterns.Value[index].Direction);
                                    break;
                                case EnumIndexAlignDirection.LEFTUPPER:
                                    xindex = FindEdgeTestDie(yindex++, param.Patterns.Value[index].Direction);
                                    break;
                                case EnumIndexAlignDirection.LEFTLOWER:
                                    xindex = FindEdgeTestDie(yindex--, param.Patterns.Value[index].Direction);
                                    break;
                                case EnumIndexAlignDirection.RIGHTLOWER:
                                    xindex = FindEdgeTestDie(yindex++, param.Patterns.Value[index].Direction);
                                    break;
                            }
                        }

                    }

                }
            }
            catch (Exception err)
            {

                throw err;
            }

        }

        private long FindEdgeTestDie(long yindex, EnumIndexAlignDirection direction)
        {
            long xindex = -1;
            try
            {
                switch (direction)
                {
                    case EnumIndexAlignDirection.RIGHTUPPER:
                        xindex = WaferObject.GetPhysInfo().MapCountX.Value;
                        break;
                    case EnumIndexAlignDirection.LEFTUPPER:
                        xindex = 0;
                        break;
                    case EnumIndexAlignDirection.LEFTLOWER:
                        xindex = 0;
                        break;
                    case EnumIndexAlignDirection.RIGHTLOWER:
                        xindex = WaferObject.GetPhysInfo().MapCountX.Value;
                        break;
                }

                while (true)
                {
                    IDeviceObject dev = WaferObject.GetDevices().Find(info => info.DieIndexM.XIndex == xindex && info.DieIndexM.YIndex == yindex);
                    if (dev != null)
                    {
                        if (dev.State.Value == DieStateEnum.NORMAL)
                            break;
                        else
                        {
                            switch (direction)
                            {
                                case EnumIndexAlignDirection.RIGHTUPPER:
                                    xindex--;
                                    if (xindex == 0)
                                        return -1;
                                    break;
                                case EnumIndexAlignDirection.LEFTUPPER:
                                    xindex++;
                                    if (xindex == WaferObject.GetPhysInfo().MapCountX.Value)
                                        return -1;
                                    break;
                                case EnumIndexAlignDirection.LEFTLOWER:
                                    xindex++;
                                    if (xindex == WaferObject.GetPhysInfo().MapCountX.Value)
                                        return -1;
                                    break;
                                case EnumIndexAlignDirection.RIGHTLOWER:
                                    xindex--;
                                    if (xindex == 0)
                                        return -1;
                                    break;
                            }

                        }
                    }
                    else
                    {
                        switch (direction)
                        {
                            case EnumIndexAlignDirection.RIGHTUPPER:
                                xindex--;
                                if (xindex == 0)
                                    return -1;
                                break;
                            case EnumIndexAlignDirection.LEFTUPPER:
                                xindex++;
                                if (xindex == WaferObject.GetPhysInfo().MapCountX.Value)
                                    return -1;
                                break;
                            case EnumIndexAlignDirection.LEFTLOWER:
                                xindex++;
                                if (xindex == WaferObject.GetPhysInfo().MapCountX.Value)
                                    return -1;
                                break;
                            case EnumIndexAlignDirection.RIGHTLOWER:
                                xindex--;
                                if (xindex == 0)
                                    return -1;
                                break;
                        }

                    }

                    //_delays.DelayFor(1);
                    Thread.Sleep(1);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return xindex;
        }

        private IProberStation ProberStation { get; set; }
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    this.ProberStation.ChangeScreenToPnp(new PnpMultiTuple(this.WaferAligner(), (IPnpSetup)this));
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
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

        private async Task RegistePattern(EnumIndexAlignDirection direction)
        {
            try
            {
                WaferCoordinate coordinate = new WaferCoordinate();
                MachineIndex curIndex = new MachineIndex(CurCam.CamSystemMI.XIndex, CurCam.CamSystemMI.YIndex);
                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    coordinate = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    coordinate = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                List<EnumIndexAlignDirection> directions = new List<EnumIndexAlignDirection>();
                WA_IAStnadrdPTInfomation ptinfo = null;

                IDeviceObject device = WaferObject.GetDevices().Find(index => index.DieIndexM.XIndex == curIndex.XIndex
                 && index.DieIndexM.YIndex == curIndex.YIndex);
                if (device != null)
                {
                    if (device.State.Value != DieStateEnum.NORMAL)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("", "Please register your pattern with TestDie.", EnumMessageStyle.Affirmative);

                        return;
                    }

                }
                else //device == null
                {
                    await this.MetroDialogManager().ShowMessageDialog("", "Device does not exist.", EnumMessageStyle.Affirmative);

                    return;
                }

                switch (direction)
                {
                    // TODO : ImageMessageDialog 챙기기
                    case EnumIndexAlignDirection.RIGHTUPPER:

                        if (coordinate.GetX() <= WaferObject.GetSubsInfo().WaferCenter.GetX()
                            || coordinate.GetY() <= WaferObject.GetSubsInfo().WaferCenter.GetY())
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please set pattern of upper right edge position.", EnumMessageStyle.Affirmative);

                            return;
                        }

                        directions.Add(EnumIndexAlignDirection.UPPER);
                        directions.Add(EnumIndexAlignDirection.RIGHT);
                        directions.Add(EnumIndexAlignDirection.LOWER);

                        if (ValidationEdgePosition(device, directions) != EventCodeEnum.NONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please register the pattern in TestDie located in the same place as the image.", EnumMessageStyle.Affirmative);

                            return;
                        }

                        ptinfo = await RegistePattern("RightUpper");
                        break;
                    case EnumIndexAlignDirection.LEFTUPPER:
                        if (coordinate.GetX() >= WaferObject.GetSubsInfo().WaferCenter.GetX()
                            || coordinate.GetY() <= WaferObject.GetSubsInfo().WaferCenter.GetY())
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please set pattern of upper left edge position.", EnumMessageStyle.Affirmative);

                            return;
                        }

                        directions.Add(EnumIndexAlignDirection.UPPER);
                        directions.Add(EnumIndexAlignDirection.LEFT);
                        directions.Add(EnumIndexAlignDirection.LOWER);
                        if (ValidationEdgePosition(device, directions) != EventCodeEnum.NONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please register the pattern in TestDie located in the same place as the image.", EnumMessageStyle.Affirmative);

                            return;
                        }

                        ptinfo = await RegistePattern("LeftUpper");
                        break;
                    case EnumIndexAlignDirection.LEFTLOWER:
                        if (coordinate.GetX() >= WaferObject.GetSubsInfo().WaferCenter.GetX()
                            || coordinate.GetY() >= WaferObject.GetSubsInfo().WaferCenter.GetY())
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please set the pattern of the lower left edge position.", EnumMessageStyle.Affirmative);

                            return;
                        }
                        directions.Add(EnumIndexAlignDirection.LOWER);
                        directions.Add(EnumIndexAlignDirection.LEFT);
                        directions.Add(EnumIndexAlignDirection.UPPER);

                        if (ValidationEdgePosition(device, directions) != EventCodeEnum.NONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please register the pattern in TestDie located in the same place as the image.", EnumMessageStyle.Affirmative);

                            return;
                        }

                        ptinfo = await RegistePattern("LeftLower");
                        break;
                    case EnumIndexAlignDirection.RIGHTLOWER:
                        if (coordinate.GetX() <= WaferObject.GetSubsInfo().WaferCenter.GetX()
                           || coordinate.GetY() >= WaferObject.GetSubsInfo().WaferCenter.GetY())
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please set the pattern of the lower right edge position.", EnumMessageStyle.Affirmative);

                            return;
                        }
                        directions.Add(EnumIndexAlignDirection.LOWER);
                        directions.Add(EnumIndexAlignDirection.RIGHT);
                        directions.Add(EnumIndexAlignDirection.UPPER);
                        if (ValidationEdgePosition(device, directions) != EventCodeEnum.NONE)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("", "Please register the pattern in TestDie located in the same place as the image.", EnumMessageStyle.Affirmative);

                            return;
                        }
                        ptinfo = await RegistePattern("RightLower");
                        break;

                }

                if (ptinfo != null)
                {
                    ptinfo.Direction = direction;
                    ptinfo.Directions = new ObservableCollection<EnumIndexAlignDirection>(directions);

                    EventCodeEnum retVal = Processing(ptinfo);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        //int ret = IndexAlignPatternParam.Patterns.Value.ToList<WA_IAStnadrdPTInfomation>
                        //    ().FindIndex(info => info.PMParameter.ModelFilePath.Value.Equals(ptinfo.PMParameter.ModelFilePath.Value));

                        int index = IndexAlignPatternParam_Clone.Patterns.Value.ToList<WA_IAStnadrdPTInfomation>
                            ().FindIndex(info => info.Direction == ptinfo.Direction);
                        if (index == -1)
                            IndexAlignPatternParam_Clone.Patterns.Value.Add(ptinfo);
                        else
                            IndexAlignPatternParam_Clone.Patterns.Value[index] = ptinfo;

                        _PatternCount = IndexAlignPatternParam_Clone.Patterns.Value.Count;
                        _CurPatternIndex = IndexAlignPatternParam_Clone.Patterns.Value.Count;
                        StepLabel = String.Format("PATTERN  {0}/{1}", _CurPatternIndex, _PatternCount);

                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Fail Registe Pattern", " Please try again with a different Index pattern. ", EnumMessageStyle.Affirmative);
                    }
                }

                ParamValidation();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum ValidationEdgePosition(IDeviceObject device, List<EnumIndexAlignDirection> directions)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MachineIndex mindex = new MachineIndex(device.DieIndexM.XIndex, device.DieIndexM.YIndex);
                for (int index = 0; index < directions.Count; index++)
                {
                    IDeviceObject dev = null;
                    switch (directions[index])
                    {
                        case EnumIndexAlignDirection.LEFT:
                            mindex.XIndex -= 1;
                            dev = WaferObject.GetDevices().Find(d => d.DieIndexM.XIndex == (mindex.XIndex)
                                     && d.DieIndexM.YIndex == (mindex.YIndex));
                            if (dev != null)
                            {
                                if (dev.State.Value == DieStateEnum.MARK)
                                    retVal = EventCodeEnum.NONE;
                                else
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return retVal;
                                }

                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return retVal;
                            }
                            break;
                        case EnumIndexAlignDirection.RIGHT:
                            mindex.XIndex += 1;
                            dev = WaferObject.GetDevices().Find(d => d.DieIndexM.XIndex == (mindex.XIndex)
                                     && d.DieIndexM.YIndex == (mindex.YIndex));
                            if (dev != null)
                            {
                                if (dev.State.Value == DieStateEnum.MARK)
                                    retVal = EventCodeEnum.NONE;
                                else
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return retVal;
                                }

                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return retVal;
                            }
                            break;
                        case EnumIndexAlignDirection.UPPER:
                            mindex.YIndex += 1;
                            dev = WaferObject.GetDevices().Find(d => d.DieIndexM.XIndex == (mindex.XIndex)
                                     && d.DieIndexM.YIndex == (mindex.YIndex));
                            if (dev != null)
                            {
                                if (dev.State.Value == DieStateEnum.MARK)
                                    retVal = EventCodeEnum.NONE;
                                else
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return retVal;
                                }

                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return retVal;
                            }
                            break;
                        case EnumIndexAlignDirection.LOWER:
                            mindex.YIndex -= 1;
                            dev = WaferObject.GetDevices().Find(d => d.DieIndexM.XIndex == (mindex.XIndex)
                                     && d.DieIndexM.YIndex == (mindex.YIndex));
                            if (dev != null)
                            {
                                if (dev.State.Value == DieStateEnum.MARK)
                                    retVal = EventCodeEnum.NONE;
                                else
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return retVal;
                                }

                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return retVal;
                            }
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        private async Task<WA_IAStnadrdPTInfomation> RegistePattern(string ptname)
        {
            WA_IAStnadrdPTInfomation patterninfo = null;

            try
            {
                RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();

                if (patternparam.Width == 0 || patternparam.Height == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Inappropriate Pattern.", "Check the pattern size.", EnumMessageStyle.Affirmative);

                    return null;
                }

                WaferCoordinate wcd = new WaferCoordinate();
                patterninfo = new WA_IAStnadrdPTInfomation();

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    wcd = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                }
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    wcd = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }

                patterninfo.X.Value = (wcd.X.Value - WaferObject.GetSubsInfo().WaferCenter.GetX());
                patterninfo.Y.Value = (wcd.Y.Value - WaferObject.GetSubsInfo().WaferCenter.GetY());
                patterninfo.Z.Value = (wcd.Z.Value - WaferObject.GetSubsInfo().WaferCenter.GetZ());

                patterninfo.MIndex = new MachineIndex(CurCam.CamSystemMI.XIndex, CurCam.CamSystemMI.YIndex);

                patterninfo.CamType.Value = CurCam.GetChannelType();
                patterninfo.LightParams = new System.Collections.ObjectModel.ObservableCollection<LightValueParam>();

                string RootPath = this.FileManager().FileManagerParam.DeviceParamRootDirectory + "\\" + this.FileManager().FileManagerParam.DeviceName;

                patterninfo.PMParameter = SettingPMParam;
                patterninfo.PMParameter.ModelFilePath.Value = RootPath + IndexAlignPatternParam_Clone.PatternbasePath + IndexAlignPatternParam_Clone.PatternName + ptname;
                patterninfo.PMParameter.PatternFileExtension.Value = ".mmo";

                patterninfo.Imagebuffer = this.VisionManager().ReduceImageSize(this.VisionManager().SingleGrab(patterninfo.CamType.Value, this), patternparam.LocationX, patternparam.LocationY, patternparam.Width, patternparam.Height);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                patterninfo = null;
                throw err;
            }

            return patterninfo;
        }

        private EventCodeEnum Processing(WA_IAStnadrdPTInfomation ptinfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //원점으로 이동.
                if (ptinfo.CamType.Value == EnumProberCam.WAFER_LOW_CAM)
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(
                        ptinfo.GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        ptinfo.GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY());
                else if (ptinfo.CamType.Value == EnumProberCam.WAFER_HIGH_CAM)
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        ptinfo.GetX() + WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        ptinfo.GetY() + WaferObject.GetSubsInfo().WaferCenter.GetY());

                //원점 패턴매칭
                PMResult pmresult = this.VisionManager().PatternMatching(ptinfo, this);

                if (pmresult.RetValue == EventCodeEnum.NONE)
                {
                    for (int index = 0; index < ptinfo.Directions.Count; index++)
                    {
                        switch (ptinfo.Directions[index])
                        {
                            case EnumIndexAlignDirection.LEFT:
                                this.StageSupervisor().StageModuleState.StageRelMove(-(WaferObject.GetSubsInfo().ActualDieSize.Width.Value), 0);
                                break;
                            case EnumIndexAlignDirection.RIGHT:
                                this.StageSupervisor().StageModuleState.StageRelMove(WaferObject.GetSubsInfo().ActualDieSize.Width.Value, 0);
                                break;
                            case EnumIndexAlignDirection.UPPER:
                                this.StageSupervisor().StageModuleState.StageRelMove(0, WaferObject.GetSubsInfo().ActualDieSize.Height.Value);
                                break;
                            case EnumIndexAlignDirection.LOWER:
                                this.StageSupervisor().StageModuleState.StageRelMove(0, -(WaferObject.GetSubsInfo().ActualDieSize.Height.Value));
                                break;
                        }

                        pmresult = this.VisionManager().PatternMatching(ptinfo, this);

                        retVal = pmresult.RetValue;
                        if (retVal != EventCodeEnum.NONE)
                        {
                            ptinfo.PatternState.Value = PatternStateEnum.FAILED;
                            break;
                        }

                    }

                    if (pmresult.RetValue != EventCodeEnum.NONE)
                    {
                        //원점 패턴매칭 실패시
                        retVal = pmresult.RetValue;
                        ptinfo.PatternState.Value = PatternStateEnum.FAILED;
                        return retVal;
                    }
                }
                else
                {
                    //원점 패턴매칭 실패시
                    retVal = pmresult.RetValue;
                    ptinfo.PatternState.Value = PatternStateEnum.FAILED;
                    this.NotifyManager().Notify(EventCodeEnum.WAFER_INDEX_ALIGN_PATTERN_NOT_FOUND);
                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
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

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
    }
}
