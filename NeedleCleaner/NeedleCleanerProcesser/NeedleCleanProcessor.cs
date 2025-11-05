using LogModule;
using NeedleCleanerModuleParameter;
using Newtonsoft.Json;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using ProbingModule;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NeedleCleanerProcesser
{
    public class NeedleCleanProcessor : PNPSetupBase, IProcessingModule, INotifyPropertyChanged, ISetup, ITemplateModule
    {
        public override Guid ScreenGUID { get; } = new Guid("DCB30931-9DC1-1D31-3941-10228E6B540B");

        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public override bool Initialized { get; set; } = false;

        private IStateModule _NeedleCleanModule;
        public IStateModule NeedleCleanModule
        {
            get { return _NeedleCleanModule; }
            set
            {
                if (value != _NeedleCleanModule)
                {
                    _NeedleCleanModule = value;
                    RaisePropertyChanged();
                }
            }
        }

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
        public NeedleCleanProcessor()
        {
        }
        public NeedleCleanProcessor(IStateModule Module)
        {
            _NeedleCleanModule = Module;
        }


        private IWaferObject Wafer
        {
            get { return this.StageSupervisor().WaferObject; }
        }
        private new IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }



        public new NeedleCleanObject NC
        {
            get { return this.StageSupervisor().NCObject as NeedleCleanObject; }
        }

        private NeedleCleanDeviceParameter _NeedleCleanerParam;
        public NeedleCleanDeviceParameter NeedleCleanerParam
        {
            get { return (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam; }
            set
            {
                if (value != _NeedleCleanerParam)
                {
                    _NeedleCleanerParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum DoExecute() //실제 프로세싱 하는 코드
        {
            try
            {
                EventCodeEnum RetVal = EventCodeEnum.NONE;
                bool bStart = false;

                //MovingState.Moving();

                for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                {
                    if (this.NeedleCleaner().IsTimeToCleaning(i) == true)
                    {
                        RetVal = BeginCleaningTask(i);
                        bStart = true;
                    }
                }

                if (bStart == true)
                {
                    RetVal = DoNeedleCleaning();
                    this.StageSupervisor().NCObject.NeedleCleaningProcessed = true;
                }

                if (RetVal == EventCodeEnum.NONE)
                {                    
                    SubModuleState = new SubModuleDoneState(this);
                }
                else
                {
                    SubModuleState = new SubModuleErrorState(this);
                }
                return RetVal;

                //MovingState.Stop();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum DoClearData() //현재 Parameter Check 및 Init하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                foreach (var list in NC.NCSheetVMDefs)
                {
                    list.FlagCleaningDone = false;
                    list.FlagRequiredCleaning = false;

                    //TO DO: Remove this code                    
                    //list.FlagCleaningForCurrentLot = false;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - DoClearData() : Error occured.");
                throw err;
            }
            return retVal;
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //모듈의  Setup상태를 변경해준다.

                for (int ncNum = 0; ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1; ncNum++)
                {
                    //NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value == 0
                    if (NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value == 0 ||
                                NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value == 0)
                    {
                        retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                System.Diagnostics.Debug.Assert(true);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //LoggerManager.Debug(err);
            }
            return retVal;
        }



        public EventCodeEnum DoRecovery() // Recovery때 하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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
                System.Diagnostics.Debug.Assert(true);
                throw err;
            }
            return retVal;
        }
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public bool IsExecute() //SubModule이 Processing 가능한지 판단하는 조건 
        {
            try
            {
                for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                {
                    if (this.NeedleCleaner().IsTimeToCleaning(i) == true)
                    {
                        return true;
                    }
                }

                return false;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - IsExecute() : Error occured.");
                throw err;
            }
        }

        #region ==>Debug Code
        private EventCodeEnum DoProcessing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
        #endregion

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
                    _NeedleCleanModule = this.NeedleCleaner();

                    //init 하는 코드 하고 나서 Idle 상태로 초기화
                    SubModuleState = new SubModuleIdleState(this);
                    MovingState = new SubModuleStopState(this);


                    NeedleCleanerParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;


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

        enum FCS_List
        {
            FCS_CENTER = 0,
            FCS_TOPLEFT = 1,
            FCS_TOPRIGHT = 2,
            FCS_BOTTOMRIGHT = 3,
            FCS_BOTTOMLEFT = 4
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
                //Pnp를 사용한다면 ListView에 띄워줄 이름을 기재해 주어야한다.
                Header = "NeedleCleanModule";

                //DisplayPort (Vision 화면)와 관련된 작업. 
                //InitPnpModule() : Stage, Loader 카메라 모두 화면과 연결.
                //InitPnpModuleStage () :  Stage 카메라만 화면과 연결.
                retVal = InitPnpModuleStage();
                //Light Jog를 초기화한다. 
                //CurCam Property를 초기화 하지 않았으면 Camera Type을 같이 넘겨주어야한다.
                //CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM); 이와 같은 고르고 CurCam 을 할당해주었다면 InitLightJog에 this 만 넘겨주어도 된다.
                InitLightJog(this, EnumProberCam.WAFER_HIGH_CAM);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitLightJog(this);
                retVal = await InitSetup();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                System.Diagnostics.Debug.Assert(true);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return retVal;
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
                retVal = InitPnpUI();

                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                InitLightJog(this, EnumProberCam.WAFER_HIGH_CAM);

                // MainView 화면에 Camera(Vision) 화면이 나온다.
                MainViewTarget = CurCam;
                // MiniView 화면에 WaferMap 화면이 나온다.
                //MiniViewTarget = Wafer;
                // MiniView 화면에 Dut 화면이 나온다.
                MiniViewTarget = ProbeCard;

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                System.Diagnostics.Debug.Assert(true);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //LoggerManager.Debug(err);
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
                retVal = InitPnpUI();
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug(err.Message);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPnpUI()
        {
            // Button 들에서 설정할수 있는것들.
            // Caption : 버튼에 보여줄 문자
            // CaptionSize : 문자 크기
            // IconSource : 버튼에 보여줄 이미지
            // RepeatEnable : true - 버튼에 Repeat 기능 활성화 / false - 버튼에 Repeat 기능 비활성화
            // Visibility : Visibility.Visible - 버튼 보임 / Visibility.Hidden - 버튼 안보임
            // IsEnabled : true - 버튼 활성화 / false - 버튼 비활성화

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //StepLabel = "";

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                OneButton.Command = new RelayCommand(RelayCommand_Example1_Method);

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                TwoButton.Command = new AsyncCommand(AsyncCommand_Example1_Method);

                ThreeButton.Caption = "";
                ThreeButton.Command = new RelayCommand<object>(RealyCommand_Example2_Method);

                FourButton.Caption = "";
                FourButton.Command = new AsyncCommand<object>(AsyncCommand_Example2_Method);

                PadJogLeftUp.Caption = null;
                PadJogLeftUp.Command = null;

                PadJogRightUp.Caption = null;
                PadJogRightUp.Command = null;

                PadJogLeftDown.Caption = null;
                PadJogLeftDown.Command = null;

                PadJogRightDown.Caption = null;
                PadJogRightDown.Command = null;

                PadJogSelect.Caption = null;
                PadJogSelect.Command = null;

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

                //ChangeWidthValue , ChangeHeightValue 값 변경 으로 Rect 사이즈 얼마씩 조절할기 설정할수 있다.
                //Ex ) ChangeWidthValue = 8;

                retVal = EventCodeEnum.NONE;
            }

            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //MovingState.Stop();
            }

            return retVal;
        }

        #region //..Command Method
        /// <summary>
        /// 매개변수 받을수 없는 동기 Command
        /// </summary>
        private void RelayCommand_Example1_Method()
        {
            DoExecute();
        }

        /// <summary>
        /// 매개변수 받을수 있는 동기 Command
        /// </summary>
        /// <param name="param"></param>
        private void RealyCommand_Example2_Method(object param)
        {

        }

        /// <summary>
        /// 매개변수 받을수 없는 비동기 command
        /// </summary>
        /// <returns></returns>

        private Task<EventCodeEnum> AsyncCommand_Example1_Method()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        /// <summary>
        /// 매개변수 받을수 있는 비동기 command
        /// </summary>
        /// <returns></returns>
        private Task AsyncCommand_Example2_Method(object param)
        {
            return Task.CompletedTask;
        }
        #endregion                

        //private List<NCCoordinate> _CleaningLoc = new List<NCCoordinate>();
        //public List<NCCoordinate> CleaningLoc
        //{
        //    get { return _CleaningLoc; }
        //    set
        //    {
        //        if (value != _CleaningLoc)
        //        {
        //            _CleaningLoc = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        #region NcSequenceGenerate

        private double DUT_HalfSizeX()
        {
            double tmpVal = 0;

            //TO DO : remove this code
            //this.StageSupervisor().ProbeCardInfo.XSize.Value = 4;

            try
            {
                tmpVal = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX * this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value / 2;
                return tmpVal;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - DUT_HalfSizeX() : Error occured.");
                throw err;
            }
        }

        private double DUT_HalfSizeY()
        {
            double tmpVal = 0;

            //TO DO : remove this code
            //this.StageSupervisor().ProbeCardInfo.YSize.Value = 4;

            try
            {
                tmpVal = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY * this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value / 2;
                return tmpVal;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - DUT_HalfSizeY() : Error occured.");
                throw err;
            }
        }

        private EventCodeEnum GetIncrementValue(int ncNum, out double incX, out double incY)
        {
            EventCodeEnum RetVal = EventCodeEnum.NEEDLE_CLEANING_UNKNOWN_EXCEPTION;

            incX = 0;
            incY = 0;

            try
            {
                if (NeedleCleanerParam.SheetDevs[ncNum].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    switch (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value)
                    {
                        case NC_CleaningDirection.HOLD:
                            {
                                incX = 0;
                                incY = 0;
                                break;
                            }
                        case NC_CleaningDirection.TOP:
                            {
                                incX = 0;
                                incY = NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value;
                                break;
                            }
                        case NC_CleaningDirection.TOP_RIGHT:
                            {
                                incX = NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                incY = NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                break;
                            }
                        case NC_CleaningDirection.RIGHT:
                            {
                                incX = NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value;
                                incY = 0;
                                break;
                            }
                        case NC_CleaningDirection.BOTTOM_RIGHT:
                            {
                                incX = NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                incY = -NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                break;
                            }
                        case NC_CleaningDirection.BOTTOM:
                            {
                                incX = 0;
                                incY = -NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value;
                                break;
                            }
                        case NC_CleaningDirection.BOTTOM_LEFT:
                            {
                                incX = -NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                incY = -NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                break;
                            }
                        case NC_CleaningDirection.LEFT:
                            {
                                incX = -NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value;
                                incY = 0;
                                break;
                            }
                        case NC_CleaningDirection.TOP_LEFT:
                            {
                                incX = -NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                incY = NeedleCleanerParam.SheetDevs[ncNum].CleaningDistance.Value / Math.Sqrt(2);
                                break;
                            }
                    }
                }
                else
                {
                    // User defined mode (multiple direction)
                    incX = 0;
                    incY = 0;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - GetIncrementValue() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                incX = 0;
                incY = 0;
            }

            return RetVal;
        }

        private EventCodeEnum CalcRangeLimit(int ncNum, out double Range_L, out double Range_R, out double Range_T, out double Range_B)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            MachineCoordinate mccoord = new MachineCoordinate();
            NCCoordinate nccoord = new NCCoordinate();
            double probecard_cen_x = 0;
            double probecard_cen_y = 0;
            double sizeX = 0; double sizeY = 0;

            try
            {
                sizeX = this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;
                sizeY = this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeY.Value;

                // SW LIMIT 을 고려하여 Range를 바탕으로 총 사용할 수 있는 영역의 크기를 구한다.
                // TODO: 프로브 카드의 중심이 치우친 만큼 고려해 줄것!!
                probecard_cen_x = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                probecard_cen_y = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;

                // 클리닝 가능한 제일 왼쪽 모서리 위치
                nccoord.X.Value = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + (sizeX / 2);
                mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                if (mccoord.X.Value + probecard_cen_x > this.MotionManager().GetAxis(EnumAxisConstants.X).Param.PosSWLimit.Value)
                {
                    // Range를 벗어남
                    Range_L = (mccoord.X.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.X).Param.PosSWLimit.Value;
                }
                else
                {
                    Range_L = 0;
                }
                // 클리닝 가능한 제일 오른쪽 모서리 위치
                nccoord.X.Value = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value - (sizeX / 2);
                mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                if (mccoord.X.Value + probecard_cen_x < this.MotionManager().GetAxis(EnumAxisConstants.X).Param.NegSWLimit.Value)
                {
                    // Range를 벗어남
                    Range_R = (mccoord.X.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.X).Param.NegSWLimit.Value;
                }
                else
                {
                    Range_R = 0;
                }
                // 클리닝 가능한 제일 위쪽 모서리 위치
                nccoord.Y.Value = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value - (sizeY / 2);
                mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                if (mccoord.Y.Value + probecard_cen_y < this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.NegSWLimit.Value)
                {
                    // Range를 벗어남
                    Range_T = (mccoord.Y.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.NegSWLimit.Value;
                }
                else
                {
                    Range_T = 0;
                }
                // 클리닝 가능한 제일 아래쪽 모서리 위치
                nccoord.Y.Value = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + (sizeY / 2);
                mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                //LoggerManager.Debug($"cleaning pos = {mccoord.Y.Value + probecard_cen_y}");

                if (mccoord.Y.Value + probecard_cen_y > this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.PosSWLimit.Value)
                {
                    // Range를 벗어남
                    Range_B = (mccoord.Y.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.PosSWLimit.Value;
                }
                else
                {
                    Range_B = 0;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - CalcRangeLimit() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

                Range_L = 0;
                Range_R = 0;
                Range_T = 0;
                Range_B = 0;
            }

            return RetVal;
        }



        private EventCodeEnum GetInitialPos(int ncNum, out double posX, out double posY)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            double tmpPosX = 0;
            double tmpPosY = 0;
            double shiftX = 0;
            double shiftY = 0;
            double b = 0;
            double range_L = 0;
            double range_R = 0;
            double range_T = 0;
            double range_B = 0;

            try
            {
                RetVal = CalcRangeLimit(ncNum, out range_L, out range_R, out range_T, out range_B);

                if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.HOLD)
                {
                    // center
                    posX = (range_L + range_R) / 2;
                    posY = (range_T + range_B) / 2;
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.TOP)
                {
                    // from lower left
                    posX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L;
                    posY = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B;
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.TOP_RIGHT)
                {
                    // from lower left
                    posX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L;
                    posY = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B;

                    // 좌측 하단에서 우측 상단으로 빗겨 올려가면 좌측 상단이 사용되지 못한채 남는다.
                    // 이를 방지하기 위하여 초기 Y 위치를 시프트 시켜 시작해야 하는데 적절한 Y 높이를 계산하여 찾는다.
                    CalculateBestShiftForDUT(3, out shiftX, out shiftY);        // 좌측 시프트 양

                    tmpPosX = posX;
                    tmpPosY = posY;
                    while (tmpPosY <= NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T)
                    {
                        tmpPosX = tmpPosX + shiftX;    // 좌측으로 시프트
                        b = posY - tmpPosX;        // 시프트된 그래프의 1차방정식을 구한다  y = ax + b,  a = 1

                        tmpPosY = posX + b;      // 시프트한 그래프 방정식에 처음 X위치를 대입하여 Y 위치를 구하고 클린시트 가장 좌측 위치에서 안쪽으로 들어오는지 확인
                        if (tmpPosY > NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T)
                        {
                            // 범위초과. 더 계산해 볼 필요 없음
                            break;
                        }
                        else
                        {
                            // 사용가능. 일단 이 위치 저장해 두고 한 번 더 시프트 해본다.
                            posY = tmpPosY;
                        }
                    }
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.BOTTOM_RIGHT)
                {
                    // from top left
                    posX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L;
                    posY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T;

                    // 좌측 상단에서 우측 하단으로 빗겨 내려가면 좌측 하단이 사용되지 못한채 남는다.
                    // 이를 방지하기 위하여 초기 Y 위치를 시프트 시켜 시작해야 하는데 적절한 Y 높이를 계산하여 찾는다.
                    CalculateBestShiftForDUT(3, out shiftX, out shiftY);        // 좌측 시프트 양

                    tmpPosY = posY;
                    tmpPosX = posX;
                    while (tmpPosY >= -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B)
                    {
                        tmpPosX = tmpPosX + shiftX;    // 좌측으로 시프트
                        b = posY + tmpPosX;        // 시프트된 그래프의 1차방정식을 구한다  y = ax + b,  a = -1

                        tmpPosY = (-1) * posX + b;      // 시프트한 그래프 방정식에 처음 X위치를 대입하여 Y 위치를 구하고 클린시트 가장 좌측 위치에서 안쪽으로 들어오는지 확인
                        if (tmpPosY < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B)
                        {
                            // 범위초과. 더 계산해 볼 필요 없음
                            break;
                        }
                        else
                        {
                            // 사용가능. 일단 이 위치 저장해 두고 한 번 더 시프트 해본다.
                            posY = tmpPosY;
                        }
                    }
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.BOTTOM_LEFT)
                {
                    // from top right
                    posX = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - DUT_HalfSizeX() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R;
                    posY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T;

                    // 우측 상단에서 좌측 하단으로 빗겨 내려가면 우측 하단이 사용되지 못한채 남는다.
                    // 이를 방지하기 위하여 초기 Y 위치를 시프트 시켜 시작해야 하는데 적절한 Y 높이를 계산하여 찾는다.
                    CalculateBestShiftForDUT(1, out shiftX, out shiftY);        // 우측 시프트 양

                    tmpPosX = posX;
                    tmpPosY = posY;
                    while (tmpPosY >= -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B)
                    {
                        tmpPosX = tmpPosX + shiftX;    // 우측으로 시프트
                        b = posY - tmpPosX;        // 시프트된 그래프의 1차방정식을 구한다  y = ax + b,  a = 1

                        tmpPosY = posX + b;      // 시프트한 그래프 방정식에 처음 X위치를 대입하여 Y 위치를 구하고 클린시트 가장 좌측 위치에서 안쪽으로 들어오는지 확인
                        if (tmpPosY < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B)
                        {
                            // 범위초과. 더 계산해 볼 필요 없음
                            break;
                        }
                        else
                        {
                            // 사용가능. 일단 이 위치 저장해 두고 한 번 더 시프트 해본다.
                            posY = tmpPosY;
                        }
                    }
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.TOP_LEFT)
                {
                    // from bottom right
                    posX = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - DUT_HalfSizeX() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R;
                    posY = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B;

                    // 우측 하단에서 좌측 상단으로 빗겨 올라가면 우측 상단이 사용되지 못한채 남는다.
                    // 이를 방지하기 위하여 초기 Y 위치를 시프트 시켜 시작해야 하는데 적절한 Y 높이를 계산하여 찾는다.
                    CalculateBestShiftForDUT(1, out shiftX, out shiftY);        // 우측 시프트 양

                    tmpPosX = posX;
                    tmpPosY = posY;
                    while (tmpPosY <= NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T)
                    {
                        tmpPosX = tmpPosX + shiftX;    // 우측으로 시프트
                        b = posY + tmpPosX;        // 시프트된 그래프의 1차방정식을 구한다  y = ax + b,  a = -1

                        tmpPosY = (-1) * posX + b;      // 시프트한 그래프 방정식에 처음 X위치를 대입하여 Y 위치를 구하고 클린시트 가장 좌측 위치에서 안쪽으로 들어오는지 확인
                        if (tmpPosY > NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T)
                        {
                            // 범위초과. 더 계산해 볼 필요 없음
                            break;
                        }
                        else
                        {
                            // 사용가능. 일단 이 위치 저장해 두고 한 번 더 시프트 해본다.
                            posY = tmpPosY;
                        }
                    }
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.RIGHT ||
                            NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.BOTTOM)
                {
                    // from top left
                    posX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L;
                    posY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T;
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.LEFT)
                {
                    // from top right
                    posX = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - DUT_HalfSizeX() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R;
                    posY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T;
                }
                else
                {
                    posX = (range_L + range_R) / 2;
                    posY = (range_T + range_B) / 2;
                    RetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                    LoggerManager.Debug($"NeedleCleaningProcessor - GetInitialPos() : Unexpected case");
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - GetInitialPos() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                posX = 0;
                posY = 0;
            }

            return RetVal;
        }

        private EventCodeEnum GetNextPos(int ncNum, double prevX, double prevY, out double posX, out double posY)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            double incX = 0; double incY = 0;

            try
            {
                if (NeedleCleanerParam.SheetDevs[ncNum].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    // Standard mode. single direction.
                    if (prevX == 3233 && prevY == 3233)
                    {
                        // Never cleaninged yet
                        RetVal = GetInitialPos(ncNum, out posX, out posY);
                    }
                    else
                    {
                        // Continue from previous position
                        GetIncrementValue(ncNum, out incX, out incY);
                        if (IsInsideCleanSheet(ncNum, prevX + incX, prevY + incY) == false)
                        {
                            // Out of area, shift to next line
                            RetVal = ShiftNextLine(ncNum, prevX, prevY, out posX, out posY);
                            if (RetVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"NeedleCleaningProcessor - Over cycle count : Error occured.");
                                return RetVal;
                            }
                            else
                            {
                                // Successful                                
                            }
                        }
                        else
                        {
                            posX = prevX + incX;
                            posY = prevY + incY;
                        }
                    }
                }
                else
                {
                    // User defined mode (multiple direction)  Can not use for this mode
                    posX = 0;
                    posY = 0;
                    RetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                    return RetVal;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - GetNextPos() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                posX = 0;
                posY = 0;
            }
            return RetVal;
        }

        private EventCodeEnum CalculateBestShiftForDUT(int iDirection, out double ShiftX, out double ShiftY)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            bool bValidation = false;

            ShiftX = 0;
            ShiftY = 0;

            try
            {
                if (iDirection == 0)
                {
                    // To Top
                    //ShiftX = 0;
                    //ShiftY = (double)(this.StageSupervisor.ProbeCardInfo.YSize.Value) * this.StageSupervisor.WaferObject.PhysInfoGetter.DieSizeY.Value;

                    for (int i = 1; i <= this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY; i++)
                    {
                        bValidation = true;
                        foreach (var OrgList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                        {
                            foreach (var ShiftList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                if (OrgList.MacIndex.XIndex == ShiftList.MacIndex.XIndex &&
                                   OrgList.MacIndex.YIndex == ShiftList.MacIndex.YIndex + i)
                                {
                                    bValidation = false;
                                    break;
                                }
                            }
                            if (bValidation == false) { break; }
                        }
                        if (bValidation == true)
                        {
                            ShiftX = 0;
                            ShiftY = (double)i * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeY.Value;
                            break;
                        }
                    }
                }
                else if (iDirection == 1)
                {
                    // To Right
                    //ShiftX = (double)(this.StageSupervisor.ProbeCardInfo.XSize.Value) * this.StageSupervisor.WaferObject.PhysInfoGetter.DieSizeX.Value;
                    //ShiftY = 0;

                    for (int i = 1; i <= this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX; i++)
                    {
                        bValidation = true;
                        foreach (var OrgList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                        {
                            foreach (var ShiftList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                if (OrgList.MacIndex.XIndex == ShiftList.MacIndex.XIndex + i &&
                                   OrgList.MacIndex.YIndex == ShiftList.MacIndex.YIndex)
                                {
                                    bValidation = false;
                                    break;
                                }
                            }
                            if (bValidation == false) { break; }
                        }
                        if (bValidation == true)
                        {
                            ShiftX = (double)(i) * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;
                            ShiftY = 0;
                            break;
                        }
                    }
                }
                else if (iDirection == 2)
                {
                    // To Bottom
                    //ShiftX = 0;
                    //ShiftY = -(double)(this.StageSupervisor.ProbeCardInfo.YSize.Value) * this.StageSupervisor.WaferObject.PhysInfoGetter.DieSizeY.Value;

                    for (int i = -1; i <= this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY; i--)
                    {
                        bValidation = true;
                        foreach (var OrgList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                        {
                            foreach (var ShiftList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                if (OrgList.MacIndex.XIndex == ShiftList.MacIndex.XIndex &&
                                   OrgList.MacIndex.YIndex == ShiftList.MacIndex.YIndex + i)
                                {
                                    bValidation = false;
                                    break;
                                }
                            }
                            if (bValidation == false) { break; }
                        }
                        if (bValidation == true)
                        {
                            ShiftX = 0;
                            ShiftY = (double)(i) * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeY.Value;
                            break;
                        }
                    }
                }
                else
                {
                    // To Left
                    //ShiftX = -(double)(this.StageSupervisor.ProbeCardInfo.XSize.Value) * this.StageSupervisor.WaferObject.PhysInfoGetter.DieSizeX.Value;
                    //ShiftY = 0;

                    for (int i = -1; i <= this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX; i--)
                    {
                        bValidation = true;

                        foreach (var OrgList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                        {
                            foreach (var ShiftList in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                if (OrgList.MacIndex.XIndex == ShiftList.MacIndex.XIndex + i &&
                                   OrgList.MacIndex.YIndex == ShiftList.MacIndex.YIndex)
                                {
                                    bValidation = false;
                                    break;
                                }
                            }
                            if (bValidation == false) { break; }
                        }
                        if (bValidation == true)
                        {
                            ShiftX = (double)(i) * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;
                            ShiftY = 0;
                            break;
                        }
                    }
                }

                RetVal = EventCodeEnum.NONE;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - CalculateBestShiftForDUT() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }

            return RetVal;
        }

        private EventCodeEnum ShiftNextLine(int ncNum, double posX, double posY, out double nPosX, out double nPosY)
        {
            double shiftX = 0;
            double shiftY = 0;
            double tmpValX = 0;
            double tmpValY = 0;

            double tmpValX2 = 0;
            double tmpValY2 = 0;
            nPosX = 0;
            nPosY = 0;
            double b = 0;
            double range_L = 0;
            double range_R = 0;
            double range_T = 0;
            double range_B = 0;
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = CalcRangeLimit(ncNum, out range_L, out range_R, out range_T, out range_B);

                if (NeedleCleanerParam.SheetDevs[ncNum].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.HOLD)
                    {
                        tmpValX = posX;
                        tmpValY = posY;
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.TOP ||
                            NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.BOTTOM)
                    {
                        // Shift to right

                        // horizontal shift
                        RetVal = CalculateBestShiftForDUT(1, out shiftX, out shiftY);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValX = posX + shiftX;

                        //vertical shift
                        RetVal = GetInitialPos(ncNum, out tmpValX2, out tmpValY2);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValY = tmpValY2;
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.TOP_RIGHT)
                    {
                        // 우측 상단을 향해서 올라가므로 방향의 기울기는 1이다. (45도 방향 고정이므로)
                        // 따라서 직선의 방정식은 y = x + b. b = y - x
                        // 마지막으로 클리닝했던 위치에서 우측으로 한 번 시프트 시켜 X와 Y 위치를 구한 뒤, 그 위치를 공식에 넣어 b를 구한다.
                        // 직선의 방정식이 구해지면 좌상단 대각선 위치의 클리닝 시작 위치를 계산해야 하는데 제일 상단 처음 클리닝 위치의 Y값을 구하고
                        // 앞서 계산 했던 공식에 Y값을 대입하여 X값을 계산해 낸다.
                        // 시프트된 X,Y 초기 위치가 범위를 벗어나 있을 수 있는 경우가 발생하는데, 그렇더라도 대각선 이동 경로 상에 클리닝 가능한 공간이 존재한다면
                        // 그 위치를 계산하여 사용한다.

                        // horizontal shift
                        RetVal = CalculateBestShiftForDUT(1, out shiftX, out shiftY);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValX2 = posX + shiftX + this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;  // 45도 각도로 내려가기 때문에 이전 위치랑 닿지 않기 위해서는 다이 사이즈만큼 더 시프트가 필요하다.
                        tmpValY2 = posY + shiftY;

                        b = tmpValY2 - tmpValX2;

                        tmpValY = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B;  // 클리닝 가능한 가장 최하단 위치

                        tmpValX = tmpValY - b;                              // 앞서 구한 b와 Y값을 가지고 X위치를 구한다.

                        if (tmpValX < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L)
                        {
                            tmpValX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L;   // X위치는 클린시트 좌측 끝면에 맞닿는 지점 고정
                            tmpValY = tmpValX + b;  // Y위치는 X위치를 대입하여 계산한다.
                        }
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.RIGHT ||
                             NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.LEFT)
                    {
                        // Shift to bottom       

                        // vertical shift
                        RetVal = CalculateBestShiftForDUT(2, out shiftX, out shiftY);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValY = posY + shiftY;

                        // horizontal shift
                        RetVal = GetInitialPos(ncNum, out tmpValX2, out tmpValY2);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValX = tmpValX2;
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.BOTTOM_RIGHT)
                    {
                        // 우측 하단을 향해서 내려가므로 방향의 기울기는 -1이다. (45도 방향 고정이므로)
                        // 따라서 직선의 방정식은 y = -x + b. b = x + y
                        // 마지막으로 클리닝했던 위치에서 우측으로 한 번 시프트 시켜 X와 Y 위치를 구한 뒤, 그 위치를 공식에 넣어 b를 구한다.
                        // 직선의 방정식이 구해지면 좌상단 대각선 위치의 클리닝 시작 위치를 계산해야 하는데 제일 상단 처음 클리닝 위치의 Y값을 구하고
                        // 앞서 계산 했던 공식에 Y값을 대입하여 X값을 계산해 낸다.
                        // 시프트된 X,Y 초기 위치가 범위를 벗어나 있을 수 있는 경우가 발생하는데, 그렇더라도 대각선 이동 경로 상에 클리닝 가능한 공간이 존재한다면
                        // 그 위치를 계산하여 사용한다.

                        // horizontal shift
                        RetVal = CalculateBestShiftForDUT(1, out shiftX, out shiftY);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValX2 = posX + shiftX + this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;  // 45도 각도로 내려가기 때문에 이전 위치랑 닿지 않기 위해서는 다이 사이즈만큼 더 시프트가 필요하다.
                        tmpValY2 = posY + shiftY;

                        b = tmpValX2 + tmpValY2;

                        tmpValY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T;  // 클리닝 가능한 가장 최상단 위치

                        tmpValX = b - tmpValY;                              // 앞서 구한 b와 Y값을 가지고 X위치를 구한다.          

                        if (tmpValX < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L)
                        {
                            tmpValX = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + DUT_HalfSizeX() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L;   // X위치는 클린시트 좌측 끝면에 맞닿는 지점 고정
                            tmpValY = -tmpValX + b;  // Y위치는 X위치를 대입하여 계산한다.
                        }
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.BOTTOM_LEFT)
                    {
                        // 좌측 하단을 향해서 내려가므로 방향의 기울기는 1이다. (45도 방향 고정이므로)
                        // 따라서 직선의 방정식은 y = x + b. b = y - x
                        // 마지막으로 클리닝했던 위치에서 좌측으로 한 번 시프트 시켜 X와 Y 위치를 구한 뒤, 그 위치를 공식에 넣어 b를 구한다.
                        // 직선의 방정식이 구해지면 좌상단 대각선 위치의 클리닝 시작 위치를 계산해야 하는데 제일 상단 처음 클리닝 위치의 Y값을 구하고
                        // 앞서 계산 했던 공식에 Y값을 대입하여 X값을 계산해 낸다.
                        // 시프트된 X,Y 초기 위치가 범위를 벗어나 있을 수 있는 경우가 발생하는데, 그렇더라도 대각선 이동 경로 상에 클리닝 가능한 공간이 존재한다면
                        // 그 위치를 계산하여 사용한다.

                        // horizontal shift
                        RetVal = CalculateBestShiftForDUT(3, out shiftX, out shiftY);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValX2 = posX + shiftX - this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;  // 45도 각도로 내려가기 때문에 이전 위치랑 닿지 않기 위해서는 다이 사이즈만큼 더 시프트가 필요하다.
                        tmpValY2 = posY + shiftY;

                        b = tmpValY2 - tmpValX2;

                        tmpValY = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - DUT_HalfSizeY() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T;  // 클리닝 가능한 가장 최상단 위치

                        tmpValX = tmpValY - b;                              // 앞서 구한 b와 Y값을 가지고 X위치를 구한다.        

                        if (tmpValX > NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - DUT_HalfSizeX() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R)
                        {
                            tmpValX = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - DUT_HalfSizeX() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R;   // X위치는 클린시트 좌측 끝면에 맞닿는 지점 고정
                            tmpValY = tmpValX + b;  // Y위치는 X위치를 대입하여 계산한다.
                        }
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDirection.Value == NC_CleaningDirection.TOP_LEFT)
                    {
                        // 좌측 상단을 향해서 올라가므로 방향의 기울기는 -1이다. (45도 방향 고정이므로)
                        // 따라서 직선의 방정식은 y = -x + b. b = x + y
                        // 마지막으로 클리닝했던 위치에서 좌측으로 한 번 시프트 시켜 X와 Y 위치를 구한 뒤, 그 위치를 공식에 넣어 b를 구한다.
                        // 직선의 방정식이 구해지면 좌상단 대각선 위치의 클리닝 시작 위치를 계산해야 하는데 제일 상단 처음 클리닝 위치의 Y값을 구하고
                        // 앞서 계산 했던 공식에 Y값을 대입하여 X값을 계산해 낸다.
                        // 시프트된 X,Y 초기 위치가 범위를 벗어나 있을 수 있는 경우가 발생하는데, 그렇더라도 대각선 이동 경로 상에 클리닝 가능한 공간이 존재한다면
                        // 그 위치를 계산하여 사용한다.

                        // horizontal shift
                        RetVal = CalculateBestShiftForDUT(3, out shiftX, out shiftY);
                        if (RetVal != EventCodeEnum.NONE) return RetVal;
                        tmpValX2 = posX + shiftX - this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;  // 45도 각도로 내려가기 때문에 이전 위치랑 닿지 않기 위해서는 다이 사이즈만큼 더 시프트가 필요하다.
                        tmpValY2 = posY + shiftY;

                        b = tmpValX2 + tmpValY2;

                        tmpValY = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + DUT_HalfSizeY() + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B;  // 클리닝 가능한 가장 최하단 위치

                        tmpValX = b - tmpValY;                              // 앞서 구한 b와 Y값을 가지고 X위치를 구한다.       

                        if (tmpValX > NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - DUT_HalfSizeX() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R)
                        {
                            tmpValX = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - DUT_HalfSizeX() - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R;   // X위치는 클린시트 좌측 끝면에 맞닿는 지점 고정
                            tmpValY = -tmpValX + b;  // Y위치는 X위치를 대입하여 계산한다.
                        }
                    }

                    if (IsInsideCleanSheet(ncNum, tmpValX, tmpValY) == false)
                    {
                        // No more space on right side, begin from first again.
                        if (NeedleCleanerParam.SheetDevs[ncNum].CycleCount.Value + 1 > NeedleCleanerParam.SheetDevs[ncNum].CycleLimit.Value)
                        {
                            RetVal = EventCodeEnum.NEEDLE_CLEANING_OVER_CYCLE_LIMIT;
                            return RetVal;
                        }
                        else
                        {
                            NeedleCleanerParam.SheetDevs[ncNum].CycleCount.Value = NeedleCleanerParam.SheetDevs[ncNum].CycleCount.Value + 1;
                            RetVal = GetInitialPos(ncNum, out tmpValX, out tmpValY);
                            nPosX = tmpValX;
                            nPosY = tmpValY;
                            RetVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        // ok
                        nPosX = tmpValX;
                        nPosY = tmpValY;
                        RetVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    // User defined mode (wrong usage)
                    nPosX = 0;
                    nPosY = 0;
                    RetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - ShiftNextLine() : Error occured.");
                nPosX = 0;
                nPosY = 0;
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return RetVal;
        }

        private bool IsInsideCleanSheet(int ncNum, double posX, double posY)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            double cenoffsetX = 0;
            double cenoffsetY = 0;
            int cenIndexX = 0;
            int cenIndexY = 0;
            double dist = 0;
            double imsi = 0;
            double imsi2 = 0;
            double range_L = 0;
            double range_R = 0;
            double range_T = 0;
            double range_B = 0;

            try
            {
                RetVal = CalcRangeLimit(ncNum, out range_L, out range_R, out range_T, out range_B);

                // TODO: 프로브 카드의 중심이 치우친 만큼 고려해 줄것!!
                
                // distance offset for odd/even numbers 
                imsi = (double)(this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX / 2.0);
                imsi2 = Math.Truncate(imsi);
                if (imsi != imsi2) cenoffsetX = this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value / 2.0;

                imsi = (double)(this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY / 2.0);
                imsi2 = Math.Truncate(imsi);
                if (imsi != imsi2) cenoffsetY = this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeY.Value / 2.0;

                // center coordinate 
                cenIndexX = (int)Math.Truncate((double)StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX / 2.0);
                cenIndexY = (int)Math.Truncate((double)StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY / 2.0);

                // confirm whether each edge position of dut are over clean sheet or not
                foreach (var list in this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    // calculate dut center to each edge position of dut

                    // left side
                    dist = ((list.MacIndex.XIndex - cenIndexX) * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value) - cenoffsetX;
                    if ((dist + posX < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L) || 
                        (dist + posX > NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R))
                    { return false; }

                    // right side
                    dist = ((list.MacIndex.XIndex - cenIndexX) * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value) + cenoffsetX;
                    if ((dist + posX < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_L) ||
                        (dist + posX > NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_R))
                    { return false; }

                    // upper side
                    dist = ((list.MacIndex.YIndex - cenIndexY) * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeY.Value) + cenoffsetY;
                    if ((dist + posY < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B) ||
                        (dist + posY > NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T))
                    { return false; }

                    // bottom side
                    dist = ((list.MacIndex.YIndex - cenIndexY) * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeY.Value) - cenoffsetY;
                    if ((dist + posY < -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_B) || 
                        (dist + posY > NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value + range_T))
                    { return false; }

                }

                return true;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - IsInsideCleanSheet() : Error occured.");
                throw err;
            }

        }

        private EventCodeEnum MakeCleaningJobList(int ncNum)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                int count;
                NCCoordinate ncPos = new NCCoordinate();
                double xPos;
                double yPos;
                double prePosX;
                double prePosY;
                double sizeX = 0; double sizeY = 0;
                double CenX = 0; double CenY = 0;
                double minX = 0; double maxX = 0;
                double minY = 0; double maxY = 0;
                double curX = 0; double curY = 0;
                double marginX = 0; double marginY = 0;
                double machX = 0; double machY = 0;
                int NumX = 0; int NumY = 0;
                int curIndexX = 0; int curIndexY = 0;
                double posX = 0; double posY = 0;
                int cleaning_count = 0;
                MachineCoordinate mccoord = new MachineCoordinate();
                NCCoordinate nccoord = new NCCoordinate();
                double Range_L = 0;
                double Range_R = 0;
                double Range_T = 0;
                double Range_B = 0;
                double Range_Offset_X = 0;
                double Range_Offset_Y = 0;

                if (NeedleCleanerParam.SheetDevs[ncNum].ContactLimit.Value != 0 && (NeedleCleanerParam.SheetDevs[ncNum].ContactCount.Value + NeedleCleanerParam.SheetDevs[ncNum].CleaningCount.Value >
                    NeedleCleanerParam.SheetDevs[ncNum].ContactLimit.Value))
                {
                    RetVal = EventCodeEnum.NEEDLE_CLEANING_OVER_CONTACT_COUNT_LIMIT;
                    LoggerManager.Debug($"NeedleCleaningProcessor - Over contact count limit : Error occured.");
                    
                    //this.NeedleCleanModule.ReasonOfError.Reason = "Over contact count limit";
                    this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Over contact count limit", this.GetType().Name);

                    return RetVal;
                }

                prePosX = NC.NCSysParam.SheetDefs[ncNum].LastCleaningPos.Value.X.Value;
                prePosY = NC.NCSysParam.SheetDefs[ncNum].LastCleaningPos.Value.Y.Value;

                if (NeedleCleanerParam.SheetDevs[ncNum].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    NC.NCSheetVMDefs[ncNum].CleaningSeq.Clear();

                    if (NeedleCleanModule.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        cleaning_count = NeedleCleanerParam.SheetDevs[ncNum].CleaningCount.Value;
                    }
                    else
                    {
                        cleaning_count = NC.NCSysParam.ManualNC.ContactCount.Value[ncNum];
                    }

                    if (cleaning_count <= 0)
                    {
                        RetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                        LoggerManager.Debug($"NeedleCleaningProcessor - Contact count is zero");
                        
                        //this.NeedleCleanModule.ReasonOfError.Reason = "Contact count is zero";
                        this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Contact count is zero", this.GetType().Name);

                        return RetVal;
                    }

                    for (count = 1; count <= cleaning_count; count++)
                    {
                        RetVal = GetNextPos(ncNum, prePosX, prePosY,
                                   out xPos, out yPos);

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            return RetVal;
                        }

                        ncPos = new NCCoordinate();
                        ncPos.X.Value = xPos;
                        ncPos.Y.Value = yPos;
                        prePosX = xPos;
                        prePosY = yPos;
                        NC.NCSheetVMDefs[ncNum].CleaningSeq.Add(ncPos);
                    }
                }
                else
                {
                    // User defined mode (multiple direction)

                    // TODO : Need to remove. This is temporary code in order to prevent out of range error!
                    if (this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX <= 0) this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX = 1;
                    if (this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY <= 0) this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY = 1;

                    // 1회 클리닝 할 때 사용되는 전체 영역의 크기를 계산한다.
                    sizeX = this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeX.Value;
                    sizeY = this.StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY * this.StageSupervisor.WaferObject.GetPhysInfo().DieSizeY.Value;

                    if (NeedleCleanerParam.SheetDevs[ncNum].UserDefinedSeq.Count <= 0)
                    {
                        RetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                        LoggerManager.Debug($"NeedleCleaningProcessor - Contact count is zero");

                        //this.NeedleCleanModule.ReasonOfError.Reason = "Contact count is zero";
                        this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Contact count is zero", this.GetType().Name);

                        return RetVal;
                    }

                    curX = 0;
                    curY = 0;
                    for (int i = 0; i <= NeedleCleanerParam.SheetDevs[ncNum].UserDefinedSeq.Count - 1; i++)
                    {
                        curX = curX + NeedleCleanerParam.SheetDevs[ncNum].UserDefinedSeq[i].Value.X.Value;
                        curY = curY + NeedleCleanerParam.SheetDevs[ncNum].UserDefinedSeq[i].Value.Y.Value;

                        if (curX > maxX) { maxX = curX; }
                        if (curX < minX) { minX = curX; }
                        if (curY > maxY) { maxY = curY; }
                        if (curY < minY) { minY = curY; }
                    }

                    CenX = (minX + maxX) / 2;       // 총 이동 영역에서의 중심점 옵셋. 클린 영역이 결정된 후 이 값을 빼주면 첫 클리닝 시작지점이 된다.
                    CenY = (minY + maxY) / 2;

                    sizeX = sizeX + Math.Abs(minX) + Math.Abs(maxX);
                    sizeY = sizeY + Math.Abs(minY) + Math.Abs(maxY);

                    RetVal = CalcRangeLimit(ncNum, out Range_L, out Range_R, out Range_T, out Range_B);


                    // 사용 불가능한 영역을 제외한 나머지 영역을 바탕으로 Range와 위치 옵셋을 새로 정한다.
                    Range_Offset_X = (Range_L + Range_R) / 2;
                    Range_Offset_Y = (Range_T - Range_B) / 2;


                    // 사용되는 영역을 바탕으로 전체 클린시트의 영역을 나누고 마진을 구한다.
                    NumX = (int)Math.Truncate((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_L) - Math.Abs(Range_R)) / sizeX);
                    NumY = (int)Math.Truncate((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_T) - Math.Abs(Range_B)) / sizeY);

                    marginX = ((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_L) - Math.Abs(Range_R)) - (float)(sizeX * NumX)) / 2.0;
                    marginY = ((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_T) - Math.Abs(Range_B)) - (sizeY * NumY)) / 2.0;

                    if (NumX <= 0 || NumY <= 0)
                    {
                        RetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                        LoggerManager.Debug($"NeedleCleaningProcessor - User defined sequence is over cleaning pad range, required X = {sizeX}, required Y = {sizeY}");

                        //this.NeedleCleanModule.ReasonOfError.Reason = "Not enough clean pad range";
                        this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Not enough clean pad range", this.GetType().Name);

                        return RetVal;
                    }

                    // 현재 위치가 어느 영역에 가장 가까운지 고르고, 거기서부터 아래로 한 칸 시프트한다. 아래쪽에 공간이 부족하면 오른쪽 최 상단으로 간다.

                    machX = -(NumX / 2) * sizeX;  // 좌측 하단 좌표의 왼쪽 아래 위치를 구한다.
                    machY = -(NumY / 2) * sizeY;
                    if (((double)NumX / 2.0) - (int)(NumX / 2) != 0) { machX = machX - (sizeX / 2); }
                    if (((double)NumY / 2.0) - (int)(NumY / 2) != 0) { machY = machY - (sizeY / 2); }

                    // 마지막으로 찍었던 위치가 어느 좌표에 있었는지
                    curIndexX = (int)Math.Truncate(Math.Abs((prePosX - machX) / sizeX));
                    curIndexY = (int)Math.Truncate(Math.Abs((prePosY - machY) / sizeY));

                    if (curIndexX < 0) curIndexX = 0;
                    if (curIndexX > NumX - 1) curIndexX = NumX - 1;
                    if (curIndexY < 0) curIndexY = 0;
                    if (curIndexY > NumY - 1) curIndexY = NumY - 1;

                    // 이전 찍었던 위치에서 일단 -1, 안 그러면 입력된 시퀀스가 돌아서 원위치로 돌아오게 설정되어 있을 경우 도르마무 됨.
                    curIndexY = curIndexY - 1;

                    if (curIndexY < 0)
                    {
                        curIndexY = NumY - 1;

                        // 제일 아래 라인, 더 이상 내려갈 곳이 없으니 X로 시프트 한다.
                        if (curIndexX + 1 > NumX - 1)
                        {
                            // 제일 우측 라인, 한 사이클 끝난 상태. 제일 처음으로 돌아간다.
                            if (NeedleCleanerParam.SheetDevs[ncNum].CycleCount.Value + 1 <= NeedleCleanerParam.SheetDevs[ncNum].CycleLimit.Value)
                            {
                                curIndexX = 0;
                                curIndexY = NumY - 1;
                                NeedleCleanerParam.SheetDevs[ncNum].CycleCount.Value = NeedleCleanerParam.SheetDevs[ncNum].CycleCount.Value + 1;

                                // 한 사이클이 끝났으니 시작 위치를 시프트 한다. (0은 센터, 1은 좌상단, 2는 상단... 시계방향으로 돌린다)
                                if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value >= 8)
                                { NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value = 0; }
                                else
                                { NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value += 1; }
                            }
                            else
                            {
                                // 리미트 초과
                                posX = 0;
                                posY = 0;
                                RetVal = EventCodeEnum.NEEDLE_CLEANING_OVER_CYCLE_LIMIT;
                                LoggerManager.Debug($"NeedleCleaningProcessor - Over cycle count : Error occured.");

                                //this.NeedleCleanModule.ReasonOfError.Reason = "Over cycle count limit";
                                this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Over cycle count limit", this.GetType().Name);

                                return RetVal;
                            }
                        }
                        else
                        {
                            curIndexX = curIndexX + 1;
                        }
                    }
                    else
                    {
                        // 사용 가능. 고한다.
                    }

                    // 첫 번째 샷 위치. 여기부터는 클리닝 시퀀스에 따라 상대적으로 더한다.
                    posX = machX + (curIndexX * sizeX) + (sizeX / 2) - CenX + Range_Offset_X;
                    posY = machY + (curIndexY * sizeY) + (sizeY / 2) - CenY + Range_Offset_Y;

                    //NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value = 5;

                    // 시작 지점의 옵셋을 더한다.
                    marginX = marginX * 0.9;    // 안전을 고려하여 외곽 마진의 10%는 안 찍고 버린다.
                    marginY = marginY * 0.9;
                    if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 1)
                    {
                        posX -= marginX;
                        posY += marginY;
                    }
                    else if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 2)
                    {
                        posY += marginY;
                    }
                    else if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 3)
                    {
                        posX += marginX;
                        posY += marginY;
                    }
                    else if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 4)
                    {
                        posX += marginX;
                    }
                    else if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 5)
                    {
                        posX += marginX;
                        posY -= marginY;
                    }
                    else if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 6)
                    {
                        posY -= marginY;
                    }
                    else if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 7)
                    {
                        posX -= marginX;
                        posY -= marginY;
                    }
                    else if (NC.NCSysParam.SheetDefs[ncNum].LastStartingPos.Value == 8)
                    {
                        posX -= marginX;
                    }

                    // 시작 지점을 기준으로 리스트를 작성한다.
                    NC.NCSheetVMDefs[ncNum].CleaningSeq.Clear();
                    for (int i = 0; i <= NeedleCleanerParam.SheetDevs[ncNum].UserDefinedSeq.Count - 1; i++)
                    {

                        posX = posX + NeedleCleanerParam.SheetDevs[ncNum].UserDefinedSeq[i].Value.X.Value;
                        posY = posY + NeedleCleanerParam.SheetDevs[ncNum].UserDefinedSeq[i].Value.Y.Value;

                        ncPos = new NCCoordinate();
                        ncPos.X.Value = posX;
                        ncPos.Y.Value = posY;
                        NC.NCSheetVMDefs[ncNum].CleaningSeq.Add(ncPos);
                    }

                    RetVal = EventCodeEnum.NONE;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - MakeCleaningJobList() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return RetVal;
        }

        #endregion

        #region NcCleaningMain


        private bool IsReadyToCleaning()
        {
            try
            {
                if (NC.NCSysParam.TouchSensorRegistered.Value != true || NC.NCSysParam.TouchSensorBaseRegistered.Value != true ||
                    NC.NCSysParam.TouchSensorPadBaseRegistered.Value != true || NC.NCSysParam.TouchSensorOffsetRegistered.Value != true)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - IsReadyToCleaning() : Error occured.");
                return false;
            }
        }

        private EventCodeEnum MoveCleanPadPosForCleaning(int ncNum, double nPosX, double nPosY, double overdrive)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            PinCoordinate pincoord = new PinCoordinate();
            NCCoordinate mccoord = new NCCoordinate();

            try
            {
                //TO DO: Remove this
                //return EventCodeEnum.NONE;


                mccoord.X.Value = nPosX + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value;
                mccoord.Y.Value = nPosY + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                mccoord.Z.Value = this.NeedleCleaner().GetMeasuredNcPadHeight(ncNum, nPosX, nPosY);

                //TO DO: 핀 얼라인 동작 시 주석 풀어야 함
                pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX; //this.StageSupervisor().ProbeCardInfo.ProbeCardCenterPos.X.Value;
                pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY; //this.StageSupervisor().ProbeCardInfo.ProbeCardCenterPos.Y.Value;
                pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight; //this.StageSupervisor().ProbeCardInfo.ProbeCardCenterPos.Z.Value;

                RetVal = this.StageSupervisor().StageModuleState.ProbingCoordMoveNCPad(mccoord, pincoord, overdrive);

            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - MoveCleanPadPosForCleaning() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return RetVal;
        }

        private EventCodeEnum SoftMoveCleanPadPosForCleaning(int ncNum, double nPosX, double nPosY, double overdrive, double zSpeed, double zAcc)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            PinCoordinate pincoord = new PinCoordinate();
            NCCoordinate mccoord = new NCCoordinate();

            try
            {
                mccoord.X.Value = nPosX + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.X.Value;
                mccoord.Y.Value = nPosY + NC.NCSysParam.SheetDefs[ncNum].Offset.Value.Y.Value;
                mccoord.Z.Value = this.NeedleCleaner().GetMeasuredNcPadHeight(ncNum, nPosX, nPosY);

                pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                if (zSpeed > 0 && zAcc > 0)
                {
                    RetVal = this.StageSupervisor().StageModuleState.ProbingCoordMoveNCPad(mccoord, pincoord, overdrive, zSpeed, zAcc);
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                    return RetVal;
                }

            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - SoftMoveCleanPadPosForCleaning() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return RetVal;
        }



        private EventCodeEnum DoNeedleCleaning()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            EventCodeEnum tmpRetVal = EventCodeEnum.NONE;
            NCCoordinate nccoord = new NCCoordinate();
            double curX = 0;
            double curY = 0;
            double overdrive = 0;
            double downpos = 0;
            bool bEngrModeOn = false;

            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Restart();
            //stw.Start();

            //NCCoordinate cur_pos;

            // NC1과 NC2가 동시에 실행될 경우 NC1과 NC2 사이에 Clean unit down을 해서는 안 된다. (시간 절약)
            // 따라서 이 함수에서 동시에 NC1 / NC2 / NC3를 동시에 수행시키고 실제 클리닝 동작에서는 Clean unit up/down 동작은 시키지 않는다.
            //timeStamp.Add(new KeyValuePair<string, long>("NC START", stw.ElapsedMilliseconds));
            try
            {

                NeedleCleanerParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;// 포인터 갱신. 안 해주면 밖에서 바뀐 파라미터를 인식 못한다.

                if (IsReadyToCleaning() == false)
                {
                    return EventCodeEnum.NEEDLE_CLEANING_NOT_READY;
                }

                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.CleanPad_Cleaning_Start);

                this.StageSupervisor().StageModuleState.ZCLEARED();


                for (int ncNum = 0; ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1; ncNum++)
                {
                    if (NC.NCSheetVMDefs[ncNum].FlagRequiredCleaning == true && NC.NCSheetVMDefs[ncNum].FlagCleaningDone == false)
                    {
                        // Confirm parameter state (최후의 안전장치. 여기 도달하기 전에 확인되어야 함)
                        if (NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value == 0 ||
                                NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value == 0)
                        {
                            tmpRetVal = EventCodeEnum.PARAM_INSUFFICIENT;
                            LoggerManager.Debug($"TimeToFocus() : Range parameter is not set. nc num = {ncNum}");
                        }
                        if (tmpRetVal != EventCodeEnum.NONE)
                        {
                            RetVal = tmpRetVal;
                            break;
                        }

                        NC.NCSheetVMDef.Index = ncNum;  // 진행중인 NC번호 업데이트

                        //timeStamp.Add(new KeyValuePair<string, long>("NC START 1", stw.ElapsedMilliseconds));

                        // Get cleaning sequence
                        tmpRetVal = MakeCleaningJobList(ncNum);

                        if (tmpRetVal != EventCodeEnum.NONE)
                        {
                            RetVal = tmpRetVal;
                            break;
                        }

                        bEngrModeOn = NC.NCSheetVMDefs[0].FlagHoldCleaningUpState;

                        foreach (NCCoordinate cur_pos in NC.NCSheetVMDefs[ncNum].CleaningSeq)
                        {
                            if (NeedleCleanModule.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                if (NeedleCleanerParam.SheetDevs[ncNum].OverdriveType.Value == NC_OverdriveType.NC_OD)
                                {
                                    overdrive = NeedleCleanerParam.SheetDevs[ncNum].Overdrive.Value;
                                    LoggerManager.Debug($"Cleaning overdrive is set by cleaning overdrive : {overdrive}");
                                }
                                else
                                {

                                    var od = (this.WaferObject.ProbingModule().ProbingModuleDevParam_IParam as ProbingModuleDevParam).OverDrive.Value;

                                    overdrive = od * NeedleCleanerParam.SheetDevs[ncNum].RelativeOdRatio.Value;
                                    LoggerManager.Debug($"Cleaning overdrive is set by probing overdrive : {overdrive}");
                                }
                            }
                            else
                            {
                                if (NC.NCSheetVMDefs[0].FlagHoldCleaningUpState == true)
                                {
                                    overdrive = NC.NCSheetVMDefs[ncNum].EngrOverdrive;
                                    LoggerManager.Debug($"Cleaning overdrive is set by engineering NC mode : {overdrive}");
                                }
                                else
                                {
                                    overdrive = NC.NCSysParam.ManualNC.Overdrive.Value[ncNum];
                                    LoggerManager.Debug($"Cleaning overdrive is set by manual NC : {overdrive}");
                                }
                            }

                            if (NeedleCleanerParam.SheetDevs[ncNum].OverdriveLimit.Value < overdrive && NC.NCSheetVMDefs[0].FlagHoldCleaningUpState == false)
                            {
                                overdrive = NeedleCleanerParam.SheetDevs[ncNum].OverdriveLimit.Value;
                                LoggerManager.Debug($"Cleaning overdrive is adjsuted automatically by overdrive limit : " + NeedleCleanerParam.SheetDevs[ncNum].OverdriveLimit.Value);
                            }

                            // 클리닝 OD 옵셋 적용
                            if (NC.NCSysParam.CleaningOverdriveOffset.Value != 0)
                            {
                                overdrive += NC.NCSysParam.CleaningOverdriveOffset.Value;
                                LoggerManager.Debug($"Cleaning overdrive offset is applied : {NC.NCSysParam.CleaningOverdriveOffset.Value}, final od = {overdrive}");
                            }

                            if (Math.Abs(NeedleCleanerParam.SheetDevs[ncNum].Clearance.Value) == 0)
                            {
                                LoggerManager.Debug($"Clearance for needle cleaning is zero, forcedly set by -500");
                                NeedleCleanerParam.SheetDevs[ncNum].Clearance.Value = -500;
                            }

                            if (NeedleCleanerParam.SheetDevs[ncNum].Overdrive.Value <= 0)
                            {
                                // NC OD가 음수일 경우에는 OD-Z clearance 높이로 간다.
                                downpos = overdrive - Math.Abs(NeedleCleanerParam.SheetDevs[ncNum].Clearance.Value);
                            }
                            else
                                                                       {
                                // NC OD가 양수일 경우에는 -Z clearance 높이로 간다.
                                downpos = -Math.Abs(NeedleCleanerParam.SheetDevs[ncNum].Clearance.Value);
                            }

                            //timeStamp.Add(new KeyValuePair<string, long>("NC START 2", stw.ElapsedMilliseconds));

                            // Move to cleaning position  
                            tmpRetVal = MoveCleanPadPosForCleaning(ncNum, cur_pos.X.Value, cur_pos.Y.Value, downpos);
                            if (tmpRetVal != EventCodeEnum.NONE)
                            {
                                RetVal = tmpRetVal;
                                break;
                            }

                            //LoggerManager.Debug($"NC pos = {cur_pos.X.Value}, {cur_pos.Y.Value}");

                            //timeStamp.Add(new KeyValuePair<string, long>("NC START 3", stw.ElapsedMilliseconds));

                            // Clean unit up                            
                            if (this.NeedleCleaner().IsCleanPadUP() == false)
                            {
                                if (NC.NCSysParam.NC_TYPE.Value == NC_MachineType.AIR_NC)
                                {
                                    tmpRetVal = this.NeedleCleaner().CleanPadUP(false);

                                    if (tmpRetVal != EventCodeEnum.NONE)
                                    {
                                        RetVal = tmpRetVal;
                                        //this.NeedleCleanModule.ReasonOfError.Reason = "Failed to up clean pad";
                                        this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Failed to up clean pad", this.GetType().Name);

                                        LoggerManager.Debug($"Failed to up clean pad");
                                        break;
                                    }
                                }
                            }

                            tmpRetVal = this.NeedleCleaner().WaitForCleanPadUp();
                            if (tmpRetVal != EventCodeEnum.NONE)
                            {
                                RetVal = tmpRetVal;
                                //this.NeedleCleanModule.ReasonOfError.Reason = "Failed to move up clean pad";
                                this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Failed to move up clean pad", this.GetType().Name);

                                LoggerManager.Debug($"Failed to move up clean pad");
                                break;
                            }

                            if (NC.NCSheetVMDefs[0].FlagHoldCleaningUpState == true)
                            {
                                if (NC.NCSheetVMDefs[ncNum].EngrCleaningSpeed <= 0) NC.NCSheetVMDefs[ncNum].EngrCleaningSpeed = 10000;
                                if (NC.NCSheetVMDefs[ncNum].EngrCleaningAccel <= 0) NC.NCSheetVMDefs[ncNum].EngrCleaningAccel = 100000;

                                LoggerManager.Debug($"NC slow contact : speed = {NC.NCSheetVMDefs[ncNum].EngrCleaningSpeed}, accel = {NC.NCSheetVMDefs[ncNum].EngrCleaningAccel}");
                                tmpRetVal = SoftMoveCleanPadPosForCleaning(ncNum, cur_pos.X.Value, cur_pos.Y.Value,
                                                                        overdrive,
                                                                        NC.NCSheetVMDefs[ncNum].EngrCleaningSpeed,
                                                                        NC.NCSheetVMDefs[ncNum].EngrCleaningAccel);
                            }
                            else
                            {
                                if (NeedleCleanerParam.SheetDevs[ncNum].EnableNcSoftTouch.Value == true)
                                {
                                    // Z up 
                                    if (NeedleCleanerParam.SheetDevs[ncNum].InclineOrigin.Value < overdrive)
                                    {
                                        // 일반 속도 UP
                                        tmpRetVal = MoveCleanPadPosForCleaning(ncNum, cur_pos.X.Value, cur_pos.Y.Value, NeedleCleanerParam.SheetDevs[ncNum].InclineOrigin.Value);

                                        if (tmpRetVal != EventCodeEnum.NONE)
                                        {
                                            RetVal = tmpRetVal;
                                            break;
                                        }

                                        // 감속 UP
                                        LoggerManager.Debug($"NC soft touch : speed = {NeedleCleanerParam.SheetDevs[ncNum].InclineSpeed.Value}, accel = {NeedleCleanerParam.SheetDevs[ncNum].InclineAccel.Value}");
                                        tmpRetVal = SoftMoveCleanPadPosForCleaning(ncNum, cur_pos.X.Value, cur_pos.Y.Value,
                                                                                overdrive,
                                                                                NeedleCleanerParam.SheetDevs[ncNum].InclineSpeed.Value,
                                                                                NeedleCleanerParam.SheetDevs[ncNum].InclineAccel.Value);
                                    }
                                    else
                                    {
                                        // 꺾이는 높이가 OD보다 위에 있으므로 소프트 컨텍을 할 수 없다. 일반 속도로 한 번에 올린다.
                                        tmpRetVal = MoveCleanPadPosForCleaning(ncNum, cur_pos.X.Value, cur_pos.Y.Value, overdrive);
                                    }
                                    if (tmpRetVal != EventCodeEnum.NONE)
                                    {
                                        RetVal = tmpRetVal;
                                        break;
                                    }
                                }
                                else
                                {
                                    // Z up (normal contact)
                                    tmpRetVal = MoveCleanPadPosForCleaning(ncNum, cur_pos.X.Value, cur_pos.Y.Value, overdrive);
                                    if (tmpRetVal != EventCodeEnum.NONE)
                                    {
                                        RetVal = tmpRetVal;
                                        break;
                                    }
                                }
                            }

                            double curPZpos = 0.0;
                            this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref curPZpos);

                            LoggerManager.Debug($"NC up = ({cur_pos.X.Value}, {cur_pos.Y.Value}, {curPZpos}), overdrive = {overdrive}");

                            //timeStamp.Add(new KeyValuePair<string, long>("NC START 4", stw.ElapsedMilliseconds));

                            if (NC.NCSheetVMDefs[0].FlagHoldCleaningUpState == true)
                            {
                                // 엔지니어링 중. 클리닝 옵셋 구하는 중이므로 스크러브는 하지 않는다. 그리고 여기서 플래그를 풀 때까지 무한 대기한다.
                                while (NC.NCSheetVMDefs[0].FlagHoldCleaningUpState == true)
                                {
                                    //delay.DelayFor(500);
                                    Thread.Sleep(500);
                                }
                            }
                            else
                            {
                                // Scrub
                                tmpRetVal = DoScrubing(ncNum, overdrive);
                                if (tmpRetVal != EventCodeEnum.NONE)
                                {
                                    RetVal = tmpRetVal;
                                    break;
                                }
                            }

                            // Z down
                            tmpRetVal = MoveCleanPadPosForCleaning(ncNum, cur_pos.X.Value, cur_pos.Y.Value, downpos);
                            if (tmpRetVal != EventCodeEnum.NONE)
                            {
                                RetVal = tmpRetVal;
                                break;
                            }

                            LoggerManager.Debug($"NC down = {downpos}");

                            //timeStamp.Add(new KeyValuePair<string, long>("NC START 5", stw.ElapsedMilliseconds));

                            if (bEngrModeOn == true)
                            {
                                // 엔지니어링 중이면 한번만 UP하고 끝낸다.
                                break;
                            }

                            NC.NCSheetVMDefs[ncNum].CurCleaningLoc.X.Value = cur_pos.X.Value;
                            NC.NCSheetVMDefs[ncNum].CurCleaningLoc.Y.Value = cur_pos.Y.Value;

                            //Thread.Sleep(200);

                            curX = cur_pos.X.Value;
                            curY = cur_pos.Y.Value;

                        }

                        if (tmpRetVal != EventCodeEnum.NONE)
                        {
                            RetVal = tmpRetVal;
                            break;
                        }

                        //nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                        NC.NCSysParam.SheetDefs[ncNum].LastCleaningPos.Value.X.Value = curX; //nccoord.X.Value;
                        NC.NCSysParam.SheetDefs[ncNum].LastCleaningPos.Value.Y.Value = curY; // nccoord.Y.Value;

                        if (this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                        {
                            NC.NCSheetVMDefs[ncNum].FlagCleaningForCurrentLot = true;
                            if (NeedleCleanerParam.SheetDevs[ncNum].CleaningWaferInterval.Value <= ((long)this.LotOPModule().SystemInfo.WaferCount - NC.NCSysParam.SheetDefs[ncNum].MarkedWaferCountVal))
                            { NC.NCSysParam.SheetDefs[ncNum].MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount; }

                            if (NeedleCleanerParam.SheetDevs[ncNum].CleaningDieInterval.Value <= ((long)this.LotOPModule().SystemInfo.DieCount - NC.NCSysParam.SheetDefs[ncNum].MarkedDieCountVal))
                            { NC.NCSysParam.SheetDefs[ncNum].MarkedDieCountVal = (long)this.LotOPModule().SystemInfo.DieCount; }
                        }

                        NC.NCSheetVMDefs[ncNum].FlagCleaningDone = true;
                    }
                }

                // 중복 에러 코드 발생 시 처리 루틴 넣어야 함. 아직 시스템에서 지원 안 해서 처리 못함.
                tmpRetVal = this.NeedleCleaner().CleanPadDown(true);
                
                if (tmpRetVal != EventCodeEnum.NONE)
                {
                    //this.NeedleCleanModule.ReasonOfError.Reason = "Failed to down clean pad";
                    LoggerManager.Debug($"Failed to down clean pad");
                    // 시스템 에러 발생시켜야 함. 아직 시스템에서 지원 안 해서 처리 못함.
                    RetVal = tmpRetVal;
                }

                this.StageSupervisor().StageModuleState.ZCLEARED();

                if (RetVal == EventCodeEnum.NONE)
                {
                    NC.SaveSysParameter();
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.CleanPad_Cleaning_OK);
                }
                else
                {
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Cleaning_Failure);
                }

                //stw.Stop();

                return RetVal;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - DoNeedleCleaning() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.CleanPad_Cleaning_Failure, RetVal);

                return RetVal;
            }

        }

        private EventCodeEnum DoScrubing(int ncNum, double ovd)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            double incX = 0;
            double incY = 0;
            NCCoordinate nccoord = new NCCoordinate();

            try
            {
                if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.SQUARE)
                {
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    LoggerManager.Debug($"Scrubing() start = (" + nccoord.X.Value + ", " + nccoord.Y.Value + "," + (ovd) + ")", isInfo: true);
                    incX = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;
                    incY = 0;

                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);
                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = 0;
                    incY = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;

                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);
                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;
                    incY = 0;

                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);
                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = 0;
                    incY = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;

                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);
                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.OCTAGONAL)
                {
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);
                    LoggerManager.Debug($"Scrubing() start = (" + nccoord.X.Value + ", " + nccoord.Y.Value + ")", isInfo: true);

                    incX = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;
                    incY = 0;
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    incY = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = 0;
                    incY = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    incY = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;
                    incY = 0;
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    incY = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = 0;
                    incY = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value;
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);

                    incX = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    incY = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);
                }
                else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.NO_SCRUB)
                {

                }
                else
                {
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);
                    LoggerManager.Debug($"Scrubing() start = (" + nccoord.X.Value + ", " + nccoord.Y.Value + ")", isInfo: true);

                    if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.TOP)
                    { incX = 0; incY = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value; }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.BOTTOM)
                    { incX = 0; incY = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value; }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.RIGHT)
                    { incX = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value; incY = 0; }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.LEFT)
                    { incX = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value; incY = 0; }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.TOP_RIGHT)
                    {
                        incX = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2); incY = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.BOTTOM_RIGHT)
                    {
                        incX = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2); incY = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.BOTTOM_LEFT)
                    {
                        incX = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2); incY = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    }
                    else if (NeedleCleanerParam.SheetDevs[ncNum].ScrubDirection.Value == NC_SCRUB_Direction.TOP_LEFT)
                    {
                        incX = -NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2); incY = NeedleCleanerParam.SheetDevs[ncNum].ScrubLength.Value / Math.Sqrt(2);
                    }

                    // Abs 무브 X/Y
                    nccoord = this.NeedleCleaner().ReadNcCurPosForPin(ncNum);

                    RetVal = MoveCleanPadPosForCleaning(ncNum, nccoord.X.Value + incX, nccoord.Y.Value + incY, ovd);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                    LoggerManager.Debug($"Scrubing() curpos = (" + (nccoord.X.Value + incX) + ", " + (nccoord.Y.Value + incY) + "," + (ovd) + ")", isInfo: true);
                }

                return RetVal;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleaningProcessor - DoScrubing() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                return RetVal;
            }

        }

        #endregion

        public EventCodeEnum BeginCleaningTask(int ncNum)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                // 여기 이 조건들은 포커싱 함수에도 동일하게 들어가 있어야 한다.    
                if (NC.NCSysParam.CleanUnitAttached.Value == true)
                {
                    if (ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1)
                    {
                        if (NeedleCleanerParam.SheetDevs[ncNum].Enabled.Value == true)
                        {
                            NC.NCSheetVMDefs[ncNum].FlagRequiredCleaning = true;
                        }
                    }
                }
                return RetVal;

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - TimeToFocus() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                return RetVal;
            }

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
