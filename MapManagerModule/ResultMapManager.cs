using ProberInterfaces.ResultMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using LogModule;
using ProberErrorCode;
using System.ComponentModel;
using System.IO;
using ProberInterfaces;
using ProberInterfaces.Temperature;
using ProbingDataInterface;
using MetroDialogInterfaces;
using MapConverterModule;
using MapUpDownloadModule;
using ResultMapParamObject;
using ProberInterfaces.ResultMap.BaseMapFormat.Header.Extensions.STIF;
using SerializerUtil;
using System.Runtime.Serialization.Formatters.Binary;
using LoaderControllerBase;
using LoaderController.GPController;
using ResultMapParamObject.STIF;

namespace ResultMapModule
{
    public class ResultMapManager : IResultMapManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged Function
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region ==> Properties & Fields

        // Upload를 위해 ResultMap을 Local에 저장하기 위한 경로
        public string LocalUploadPath { get => @"C:\ProberSystem\ResultMap\Temp\Upload\LOT\"; }
        // Download 받은 ResultMap을 Local에 저장하기 위한 경로
        public string LocalDownloadPath { get => @"C:\ProberSystem\ResultMap\Temp\Download\LOT\"; }
        public string LocalManualUploadPath { get => @"C:\ProberSystem\ResultMap\Temp\Upload\MANUAL\"; }
        // Download 받은 ResultMap을 Local에 저장하기 위한 경로
        public string LocalManualDownloadPath { get => @"C:\ProberSystem\ResultMap\Temp\Download\MANUAL\"; }

        public bool Initialized { get; set; } = false;
        public MapHeaderObject ResultMapHeader { get; set; }
        public ResultMapData ResultMapData { get; set; }
        //public object ConvertedResultMap { get; private set; }

        //private ResultMapManager mResultMapManager = null;
        //public ResultMapDevParameter ResultMapDevParam = null;

        private ResultMapManagerSysParameter _resultMapManagerSysParam;
        public ResultMapManagerSysParameter ResultMapManagerSysParam
        {
            get { return _resultMapManagerSysParam; }
            set
            {
                if (value != _resultMapManagerSysParam)
                {
                    _resultMapManagerSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        public IParam ResultMapManagerSysIParam { get; private set; }

        public ResultMapConverter ResultMapConverter { get; set; }
        public ResultMapUpDownloader ResultMapUpDownloader { get; set; }

        #endregion

        #region ==> Constructor & Init & DeInit Function

        public ResultMapManager() { }



        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    // CONVERTER
                    ResultMapConverter = new ResultMapConverter();
                    ResultMapConverter.LoadDevParameter();

                    // UP/DOWNLOADER
                    ResultMapUpDownloader = new ResultMapUpDownloader();
                    //ResultMapUpDownloader.InitModule(GetUploadHandlerInfo(), GetDownloadHandlerInfo());

                    ResultMapUpDownloader.InitModule();

                    //ResultMapUpDownloader.LoadSysParameter();

                    // TODO : NEED?
                    //this.LotOPModule().LotInfo.LotModeChanged += LotName_ValueChangedEvent;

                    // The process When the initialize process is successful.
                    Initialized = true;
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    Initialized = false;
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public EventCodeEnum CheckAndDownload(bool IsLotNameChangedTriggered)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CanDownload() == true)
                {
                    retval = Download(IsLotNameChangedTriggered);

                    if (retval != EventCodeEnum.NONE)
                    {
                        string msg = $"[{this.GetType().Name}] Fail Download the ResultMap.";
                        this.MetroDialogManager().ShowMessageDialog("Error", msg, EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool CanDownload()
        {
            bool retval = false;

            try
            {
                if (ResultMapManagerSysParam.DownloadType.Value != FileManagerType.UNDEFINED)
                {
                    retval = true;
                }
                else
                {
                    LoggerManager.Error($"[ResultMapManager], CanDownload() : Download type = {ResultMapManagerSysParam.DownloadType.Value}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool NeedDownload()
        {
            bool retval = false;

            try
            {
                LotModeEnum lotModeEnum = this.LotOPModule()?.LotInfo?.LotMode.Value ?? LotModeEnum.UNDEFINED;

                retval = (lotModeEnum == LotModeEnum.MPP) || (lotModeEnum == LotModeEnum.CONTINUEPROBING);

                if (retval == false)
                {
                    LoggerManager.Debug($"[ResultMapManager], NeedDownload() : No need.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {
                Initialized = false;
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        /// <summary>
        /// Argument로 넘어온 Directory 들을 생성합니다.
        /// </summary>
        /// <param name="directoryArray"> 만들고자하는 Directory 배열입니다. </param>
        private bool MakeDirectories(string[] directoryArray)
        {
            bool retVal = true;

            if (directoryArray == null)
                return false;

            try
            {

                foreach (var drtr in directoryArray)
                {
                    retVal &= MakeDirectory(drtr);
                    if (retVal == false)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] MakeDirectories() : Fails during to make '{drtr}' Directory.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// <1> Directory가 없고,
        /// <2> Drive가 존재할 때
        /// <3> Directory를 생성합니다.
        /// </summary>
        /// <param name="drtr"> 만들고자 하는 Directory 입니다. </param>
        private bool MakeDirectory(string drtr)
        {
            bool retVal = false;
            try
            {
                if (!Directory.Exists(drtr)) // <1>
                {
                    var pathRoot = Path.GetPathRoot(drtr);
                    if (Directory.Exists(pathRoot)) // <2>
                    {
                        Directory.CreateDirectory(drtr); // <3>
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }
            return retVal;
        }

        #region ==> Load/Save Function
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam iparam = null;
                retVal = this.LoadParameter(ref iparam, typeof(ResultMapManagerSysParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    ResultMapManagerSysIParam = iparam;
                    ResultMapManagerSysParam = ResultMapManagerSysIParam as ResultMapManagerSysParameter;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(ResultMapManagerSysIParam);

                retVal = this.ResultMapUpDownloader.SaveSysParameter();

                retVal = ResultMapUpDownloader.CreateFileHandlers();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == true)
                {
                    // result data 디바이스 변경 시에 LoadDevParameter 안해줘서 이전 데이터 들고 오는 상황 생김 
                    if (ResultMapConverter == null)
                    {
                        ResultMapConverter = new ResultMapConverter();
                    }
                    ResultMapConverter.LoadDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.ResultMapConverter.SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            retVal = EventCodeEnum.NONE;

            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;

            return retVal;
        }
        #endregion

        #region ==> Function & Command

        private EventCodeEnum MakeResultMapBaseData(bool IsDummy = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            MapHeaderObject mapHeader = null;
            ResultMapData resultMap = null;

            try
            {
                retVal = GetMapHeaderBase(out mapHeader);

                if (retVal == EventCodeEnum.NONE)
                {
                    if (IsDummy == false)
                    {
                        retVal = GetResultMapBase(out resultMap);
                    }
                    else
                    {
                        retVal = GetResultMapBaseForDummy(out resultMap);
                    }
                }

                if (retVal == EventCodeEnum.NONE && mapHeader != null && resultMap != null)
                {
                    retVal = UpdateResultMapData(mapHeader, resultMap);
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }



        private EventCodeEnum GetMapHeaderBase(out MapHeaderObject header)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            ILotInfo lotInfo = null;
            IProbeCard probeCard = null;
            IPhysicalInfo waferPhysicalInfo = null;
            ISubstrateInfo waferSubstrateInfo = null;
            IList<IDeviceObject> deviceList = null;
            IDeviceObject deviceInfo = null;
            ITempController tempController = null;
            IProbingModule probingModule = null;
            IProbingSequenceModule probingSequenceModule = null;
            ICoordinateManager CoordinateManager = null;
            header = null;

            try
            {
                header = new MapHeaderObject();

                lotInfo = this.LotOPModule()?.LotInfo;
                probeCard = this.GetParam_ProbeCard();
                waferPhysicalInfo = this.GetParam_Wafer()?.GetPhysInfo();
                waferSubstrateInfo = this.GetParam_Wafer()?.GetSubsInfo();
                deviceList = this.GetParam_Wafer()?.GetDevices();
                deviceInfo = deviceList?[0];
                tempController = this.TempController();
                probingModule = this.ProbingModule();
                probingSequenceModule = this.ProbingSequenceModule();
                CoordinateManager = this.CoordinateManager();
                
                #region BasicData

                header.BasicData.CardName = this.CardChangeModule().GetProbeCardID();

                header.BasicData.CardSite = 0; // TEL 확인!!
                header.BasicData.DeviceName = lotInfo.DeviceName.Value;
                header.BasicData.MapfileVersion = 0;// 확인하기.
                header.BasicData.OPName = lotInfo.OperatorID.Value;
                header.BasicData.ProberID = this.FileManager().FileManagerParam.ProberID.Value;

                #endregion

                #region LotData

                header.LotData.ChuckTemp = (int)tempController.TempInfo.CurTemp.Value; //온도값 체크
                header.LotData.LotName = lotInfo?.LotName?.Value ?? string.Empty;
                header.LotData.OCRtype = 0;

                #endregion

                #region CassetteDataObj

                //header.CassetteDataObj.CassetteID.Value = //waferPhysicalInfo.CassetteI;
                //header.CassetteDataObj.CassetteNO.Value = //waferPhysicalInfo.CassetteNo;

                #endregion

                #region WaferAlignData

                //header.WaferAlignDataObj.AlignAxis = this.WaferAligner().WaferAlignInfo;
                header.WaferAlignData.AlignedIndexSizeX = waferSubstrateInfo.ActualDieSize.Width.Value;
                header.WaferAlignData.AlignedIndexSizeY = waferSubstrateInfo.ActualDieSize.Height.Value;
                header.WaferAlignData.DieStreetSizeX = waferSubstrateInfo.DieXClearance.Value;
                header.WaferAlignData.DieStreetSizeY = waferSubstrateInfo.DieYClearance.Value;
                //header.WaferAlignDataObj.EdgeDiffX = waferSubstrateInfo.edge.Value;
                //header.WaferAlignDataObj.EdgeDiffY = waferPhysicalInfo.inde
                //header.WaferAlignDataObj.IndexAlignType = "";

                #endregion

                #region MapConfigData

                header.MapConfigData.CenterDieCoordX = (int)waferPhysicalInfo.CenU.XIndex.Value;
                header.MapConfigData.CenterDieCoordY = (int)waferPhysicalInfo.CenU.YIndex.Value;

                header.MapConfigData.RefDieCoordX = (int)waferPhysicalInfo.RefU.XIndex.Value;
                header.MapConfigData.RefDieCoordY = (int)waferPhysicalInfo.RefU.YIndex.Value;

                MachineIndex RefDieMI = CoordinateManager.UserIndexConvertToMachineIndex(new UserIndex(waferPhysicalInfo.RefU.XIndex.Value, waferPhysicalInfo.RefU.YIndex.Value));
                var RefCenterPos = this.WaferAligner().MachineIndexConvertToDieCenter(RefDieMI.XIndex, RefDieMI.YIndex);

                //Brett// origin wafer center 정보를 빼야 actual ref center pos 값이 됨 
                RefCenterPos.X.Value -= waferSubstrateInfo.WaferCenter.X.Value;
                RefCenterPos.Y.Value -= waferSubstrateInfo.WaferCenter.Y.Value;

                if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.LEFT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.UP))
                {
                    header.MapConfigData.AxisDirection = AxisDirectionEnum.UpLeft;
                }
                else if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.RIGHT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.UP))
                {
                    header.MapConfigData.AxisDirection = AxisDirectionEnum.UpRight;
                }
                else if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.RIGHT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.DOWN))
                {
                    header.MapConfigData.AxisDirection = AxisDirectionEnum.DownRight;
                }
                else if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.LEFT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.DOWN))
                {
                    header.MapConfigData.AxisDirection = AxisDirectionEnum.DownLeft;
                }
                else
                {
                    LoggerManager.Error($"[ResultMapManager], GetMapHeaderBase() : MapDirX = {waferPhysicalInfo.MapDirX.Value}, MapDirY = {waferPhysicalInfo.MapDirY.Value}, Undefined.");

                    header.MapConfigData.AxisDirection = AxisDirectionEnum.UpRight;
                }

                MachineIndex FirstDieMI = null;
                UserIndex FirstDieUI = null;

                FirstDieMI = new MachineIndex(0, waferPhysicalInfo.MapCountY.Value - 1);
                FirstDieUI = CoordinateManager.MachineIndexConvertToUserIndex(FirstDieMI);

                header.MapConfigData.FirstDieCoordX = (int)FirstDieUI.XIndex;
                header.MapConfigData.FirstDieCoordY = (int)FirstDieUI.YIndex;

                header.MapConfigData.MapSizeX = waferPhysicalInfo.MapCountX.Value;
                header.MapConfigData.MapSizeY = waferPhysicalInfo.MapCountY.Value;

                //header.MapConfigDataObj.CenterOffsetX = 0;//(int)waferPhysicalInfo.RefDieOffset.XIndex.Value;
                //header.MapConfigDataObj.CenterOffsetY = 0;//(int)waferPhysicalInfo.RefDieOffset.Yndex.Value;

                ResultMapConverterParameter convparam = this.ResultMapManager().GetResultMapConvIParam() as ResultMapConverterParameter;

                var refcalcMode = convparam.STIFParam.RefCalcMode.Value;

                //MachineIndexConvertToDieCenter


                //#region GetWaferCenterToRefDieCenterDistance  user 좌표 값 끼리의 사이 거리 계산

                //var distancetuple = convparam.STIFParam.GetWaferCenterToRefDieCenterDistance(header.MapConfigData.RefDieCoordX,
                //                                                             header.MapConfigData.RefDieCoordY,
                //                                                             header.MapConfigData.CenterDieCoordX,
                //                                                             header.MapConfigData.CenterDieCoordY,
                //                                                             header.WaferAlignData.AlignedIndexSizeX,
                //                                                             header.WaferAlignData.AlignedIndexSizeY,
                //                                                             header.MapConfigData.AxisDirection
                //                                                             );
               

                //#endregion

                if (refcalcMode == RefCalcEnum.MANUAL)
                {
                    header.MapConfigData.WaferCenterToRefDieCenterDistanceX = convparam.STIFParam.XRefManualValue.Value;
                    header.MapConfigData.WaferCenterToRefDieCenterDistanceY = convparam.STIFParam.YRefManualValue.Value;
                }
                else
                {
                    //if (distancetuple != null)
                    //{
                    //    header.MapConfigData.WaferCenterToRefDieCenterDistanceX = distancetuple.Item1;
                    //    header.MapConfigData.WaferCenterToRefDieCenterDistanceY = distancetuple.Item2;
                    //}
                    header.MapConfigData.WaferCenterToRefDieCenterDistanceX = RefCenterPos.X.Value;
                    header.MapConfigData.WaferCenterToRefDieCenterDistanceY = RefCenterPos.Y.Value * (-1);
                }

                LoggerManager.Debug($"[ResultMapManager], GetMapHeaderBase() : refcalcMode = {refcalcMode}, Auto X = {RefCenterPos.X.Value}, Auto Y = {RefCenterPos.Y.Value * (-1)}, Manual X = {convparam.STIFParam.XRefManualValue.Value}, Manual Y = {convparam.STIFParam.YRefManualValue.Value}");


                //header.MapConfigDataObj.ProbingDirection = 0; //??

                //header.MapConfigData.RefDieCoordX = (int)waferPhysicalInfo.OrgU.XIndex.Value;
                //header.MapConfigData.RefDieCoordY = (int)waferPhysicalInfo.OrgU.YIndex.Value;

                //header.MapConfigDataObj.RefDieSet.Value = ;

                #endregion

                #region TestResultData

                waferSubstrateInfo.UpdateCurrentDieCount();

                header.TestResultData.CPCnt = 0; //TEL에서 사용함.
                header.TestResultData.PassDieCnt = waferSubstrateInfo.CurPassedDieCount.Value;
                header.TestResultData.FailDieCnt = waferSubstrateInfo.CurFailedDieCount.Value;
                //header.TestResultDataObj.LastProbingSequence = this.ProbingSequenceModule().ProbingSequenceCount;
                //header.TestResultDataObj.ProbeCardContactCnt = probeCard.ContactCnt;
                //header.TestResultDataObj.ProbingDirection = 1; // 계산이 필요함.
                //header.TestResultDataObj.ProbingMode = 0;
                //header.TestResultDataObj.ProbingStartPos = 0; // 계산이 필요함.
                header.TestResultData.RetestCnt = 0; // TEL
                header.TestResultData.RetestYield = waferSubstrateInfo.RetestYield;
                //header.TestResultDataObj.TestCnt = 1; // 추후에 체크해봐야함.
                header.TestResultData.TestDieCnt = waferSubstrateInfo.CurTestedDieCount.Value;
                //header.TestResultDataObj.TestDieInformAddress = -335544320;

                //0:Normal End
                //1: Yield NG
                //2: Continuous Failng
                //3: Manual Unload
                //4. Ohter Reject
                header.TestResultData.TestEndInformation = (byte)probingModule.ProbingEndReason;
                header.TestResultData.TotalFailDieCnt = lotInfo.AccumulateInfo.TotalFailedDieCount; // TotalFailedDieCount에 아무것도 넣질않음..ㅠㅠ
                header.TestResultData.TotalPassDieCnt = lotInfo.AccumulateInfo.TotalPassedDieCount; // TotalPassedDieCount에 아무것도 넣질않음..ㅠㅠ
                header.TestResultData.TotalTestDieCnt = lotInfo.AccumulateInfo.TotalTestedDieCount; // TotalTestedDieCount에 아무것도 넣질않음..ㅠㅠ
                header.TestResultData.TotalProbingSequence = probingSequenceModule.ProbingSeqParameter.ProbingSeq.Value.Count;
                header.TestResultData.TouchDownCnt = 0; // TEL
                header.TestResultData.Yield = waferSubstrateInfo.Yield;

                #endregion

                #region TimeData

                string timeFormat = "yyMMddHHmmss";
                //header.TimeDataObj.LotEndTime.Value = lotInfo.LotEndTime.ToString(timeFormat);
                header.TimeData.LotStartTime = lotInfo.LotStartTime.ToString(timeFormat);
                header.TimeData.LotEndTime = lotInfo.LotEndTime.ToString(timeFormat);
                header.TimeData.ProbingStartTime = waferSubstrateInfo.ProbingStartTime.ToString(timeFormat);
                header.TimeData.ProbingFinishTime = waferSubstrateInfo.ProbingEndTime.ToString(timeFormat);
                header.TimeData.WaferLoadingTime = waferSubstrateInfo.LoadingTime.ToString(timeFormat);
                header.TimeData.WaferUnloadingTime = DateTime.Now.ToString(timeFormat);

                #endregion

                #region WaferData

                header.WaferData.FlatNotchDir = (int)waferPhysicalInfo.NotchAngle.Value + (int)waferPhysicalInfo.NotchAngleOffset.Value;
                header.WaferData.FlatNotchType = 0; //(int)waferPhysicalInfo.NotchType.Value;
                header.WaferData.FrmdFlatNotchDir = 0;

                header.WaferData.WaferID = waferSubstrateInfo.WaferID.Value;

                //header.WaferDataObj.WaferInsertedSlotNum = (byte)waferPhysicalInfo.SlotIndex.Value;
                //header.WaferDataObj.WaferInsertedSlotNum = (byte)waferSubstrateInfo.SlotIndex.Value;

                header.WaferData.WaferSize = waferPhysicalInfo.WaferSize_um.Value;
                header.WaferData.WaferSlotNum = (byte)waferSubstrateInfo.SlotIndex.Value;

                #endregion

                retVal = SetMapHeaderExtensionBase(header, ResultMapConverter.GetUploadConverterType());

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = header.MakeProberMapPropertyDictionay();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum SetMapHeaderExtensionBase(MapHeaderObject header, ResultMapConvertType convertType)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                switch (convertType)
                {
                    case ResultMapConvertType.OFF:
                        break;
                    case ResultMapConvertType.EG:
                        break;
                    case ResultMapConvertType.STIF:
                        header.ExtensionData = new STIFMapHeaderExtension();
                        break;
                    case ResultMapConvertType.E142:
                        header.ExtensionData = new E142MapHeaderExtension();
                        break;
                    default:
                        break;
                }

                if (header.ExtensionData != null)
                {
                    retval = header.ExtensionData.AssignProperties();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private BinDataMappingBase GetBinDataMappingSelector(ResultMapConvertType converttype)
        {
            BinDataMappingBase retval = null;

            try
            {
                switch (converttype)
                {
                    case ResultMapConvertType.OFF:
                        break;
                    case ResultMapConvertType.EG:
                        break;
                    case ResultMapConvertType.STIF:
                        retval = new STIFBinDataMapping();
                        break;
                    case ResultMapConvertType.E142:
                        retval = new E142BinDataMapping();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum AssignDummyBinData(ResultMapConvertType converttype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                BinDataMappingBase binDataMappingBase = null;
                binDataMappingBase = GetBinDataMappingSelector(converttype);

                if (binDataMappingBase != null)
                {
                    var dies = this.GetParam_Wafer().GetSubsInfo().DIEs;

                    Random random = new Random(DateTime.Now.Millisecond);

                    int? bincode = null;

                    foreach (var dev in dies)
                    {
                        if (dev.DieType.Value == DieTypeEnum.TEST_DIE)
                        {
                            // 0 ~ 1
                            int resultrnd = random.Next(0, 2);

                            if (dev.CurTestHistory != null)
                            {
                                dev.CurTestHistory.TestResult.Value = (resultrnd == 0) ? TestState.MAP_STS_PASS : TestState.MAP_STS_FAIL;
                            }
                            else
                            {

                            }

                            bincode = binDataMappingBase.GetTestBinCode(dev.CurTestHistory.TestResult.Value);

                            if (bincode != null)
                            {
                                if (dev.CurTestHistory != null)
                                {
                                    dev.CurTestHistory.BinCode.Value = (int)bincode;

                                    if (dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_PASS ||
                                        dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_FAIL)
                                    {
                                        dev.State.Value = DieStateEnum.TESTED;
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                // TODO : 
                            }
                        }
                        else
                        {
                            bincode = binDataMappingBase.GetNoneTestBinCode(dev.DieType.Value);

                            if (dev.CurTestHistory != null)
                            {
                                dev.CurTestHistory.BinCode.Value = (int)bincode;
                            }
                            else
                            {

                            }

                            if (dev.DieType.Value == DieTypeEnum.NOT_EXIST)
                            {
                                dev.State.Value = DieStateEnum.NOT_EXIST;
                            }

                            if (dev.DieType.Value == DieTypeEnum.MARK_DIE)
                            {
                                dev.State.Value = DieStateEnum.MARK;
                            }

                        }
                    }

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    // TODO : 
                }

                ISubstrateInfo waferSubstracteInfo = this.GetParam_Wafer()?.GetSubsInfo();
                waferSubstracteInfo.UpdateCurrentDieCount();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum AssignNoneTestDieBinData(ResultMapConvertType converttype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                BinDataMappingBase binDataMappingBase = null;
                binDataMappingBase = GetBinDataMappingSelector(converttype);

                if (binDataMappingBase != null)
                {
                    var dies = this.GetParam_Wafer().GetSubsInfo().DIEs;

                    foreach (var dev in dies)
                    {
                        int? bincode = null;

                        if (dev.DieType.Value != DieTypeEnum.TEST_DIE)
                        {
                            bincode = binDataMappingBase.GetNoneTestBinCode(dev.DieType.Value);

                            if (bincode != null)
                            {
                                dev.CurTestHistory.BinCode.Value = (int)bincode;
                            }
                            else
                            {
                                // TODO : 
                            }
                        }
                        else
                        {

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

        private bool IsTargetDie(int mx, int my)
        {
            bool retval = false;

            try
            {
                UserIndex ui = this.CoordinateManager().WMIndexConvertWUIndex(mx, my);

                var targetdie = this.GetParam_Wafer().GetPhysInfo().TargetUs.FirstOrDefault(x => x.XIndex.Value == ui.XIndex && x.YIndex.Value == ui.YIndex);

                if (targetdie != null)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum GetResultMapBase(out ResultMapData resultMap)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IList<IDeviceObject> deviceList = null;
            ILotInfo lotInfo = null;
            IPhysicalInfo waferPhysicalInfo = null;

            resultMap = null;

            try
            {
                deviceList = this.GetParam_Wafer()?.GetDevices();
                lotInfo = this.LotOPModule()?.LotInfo;
                waferPhysicalInfo = this.GetParam_Wafer()?.GetPhysInfo();
                resultMap = new ResultMapData();

                ResultMapConvertType convtype = ResultMapConverter.GetUploadConverterType();
                BinDataMappingBase binDataMappingBase = null;
                binDataMappingBase = GetBinDataMappingSelector(convtype);

                if (deviceList != null)
                {
                    int waferWidthCount = waferPhysicalInfo.MapCountX.Value;
                    int waferHeightCount = waferPhysicalInfo.MapCountY.Value;

                    var dies = this.GetParam_Wafer().GetSubsInfo().DIEs;

                    for (int j = 0; j < waferHeightCount; j++)
                    {
                        for (int i = 0; i < waferWidthCount; i++)
                        {
                            //LoggerManager.Debug($"X = {j}, Y = {(waferHeightCount - 1) - i}");

                            IDeviceObject dev = dies[i, (waferHeightCount - 1) - j];

                            TestHistory lastTestHistory = null;

                            if (dev.TestHistory != null && dev.TestHistory.Count > 0)
                            {
                                lastTestHistory = dev.TestHistory.Last();
                            }
                            //else
                            //{
                            //    LoggerManager.Error($"[ResultMapManager], GetResultMapBase() : xindex = {i}, yindex = {j}, TestHistory is null.");
                            //}

                            DieTypeEnum dieTypeEnum = dev.DieType.Value;
                            byte? mapResult = null;

                            int? bincode = null;

                            if (dieTypeEnum != DieTypeEnum.TEST_DIE)
                            {
                                if (dieTypeEnum == DieTypeEnum.MARK_DIE) // MARK
                                {
                                    mapResult = (byte)TestState.MAP_STS_MARK;
                                }
                                else if (dieTypeEnum == DieTypeEnum.SKIP_DIE)
                                {
                                    mapResult = (byte)TestState.MAP_STS_SKIP;
                                }
                                else
                                {
                                    mapResult = (byte)TestState.MAP_STS_NOT_EXIST;
                                }

                                // 실제로 MARK_DIE의 값을 갖고 있지만, TARGET DIE에 해당하는 값을 얻어오기 위함.

                                if (IsTargetDie(i, waferHeightCount - j - 1) == true)
                                {
                                    bincode = binDataMappingBase.GetNoneTestBinCode(DieTypeEnum.TARGET_DIE);
                                }
                                else
                                {
                                    bincode = binDataMappingBase.GetNoneTestBinCode(dieTypeEnum);
                                }
                            }
                            else
                            {
                                // TEST_DIE

                                if (lastTestHistory != null)
                                {
                                    if (lastTestHistory.TestResult.Value == TestState.MAP_STS_PASS)
                                    {
                                        mapResult = (byte)TestState.MAP_STS_PASS;
                                    }
                                    else if (lastTestHistory.TestResult.Value == TestState.MAP_STS_FAIL)
                                    {
                                        mapResult = (byte)TestState.MAP_STS_FAIL;
                                    }
                                    else
                                    {
                                        mapResult = (byte)TestState.MAP_STS_TEST;
                                    }

                                    bincode = lastTestHistory.BinCode.Value;
                                }
                                else
                                {
                                    mapResult = (byte)TestState.MAP_STS_TEST;
                                    // TODO : 
                                    bincode = 0;
                                }
                            }

                            if (mapResult != null)
                            {
                                resultMap.mStatusMap.Add((byte)mapResult);
                            }
                            else
                            {
                                LoggerManager.Error($"[ResultMapManager], GetResultMapBase() : dieTypeEnum = {dieTypeEnum}, xindex = {i}, yindex = {j}, Map status is unknown.");
                            }

                            if (bincode != null)
                            {
                                resultMap.mBINMap.Add((long)bincode);
                            }
                            else
                            {
                                LoggerManager.Error($"[ResultMapManager], GetResultMapBase() : dieTypeEnum = {dieTypeEnum}, xindex = {i}, yindex = {j}, Bin code is unknown.");
                            }

                            resultMap.mMapCoordX.Add((int)dev.DieIndex.XIndex);
                            resultMap.mMapCoordY.Add((int)dev.DieIndex.YIndex);

                            // mRePrbMap
                            // 0: Not Re-Probed,
                            // 1: Passed at re-probing 
                            // 2: Failed at re-probing
                            // 3: Perform fail (for special user)

                            // TODO : 

                            resultMap.mRePrbMap.Add(0);

                            resultMap.mDUTMap.Add(dev.DutNumber);

                            // TODO : 
                            resultMap.mTestCntMap.Add(0);

                            // TODO : 
                            resultMap.mInked.Add(false);
                            resultMap.mXWaferCenterDistance.Add(0);
                            resultMap.mYWaferCenterDistance.Add(0);
                            resultMap.mOverdrive.Add(0);
                            resultMap.mTestStartTime.Add(new DateTime());
                            resultMap.mFailMarkInspection.Add(false);
                            resultMap.mNeedleMarkInspection.Add(false);
                            resultMap.mNeedleCleaning.Add(false);
                            resultMap.mNeedleAlign.Add(false);
                        }
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum GetResultMapBaseForDummy(out ResultMapData resultMap)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IList<IDeviceObject> deviceList = null;
            ILotInfo lotInfo = null;
            IPhysicalInfo waferPhysicalInfo = null;

            resultMap = null;

            try
            {
                deviceList = this.GetParam_Wafer()?.GetDevices();
                lotInfo = this.LotOPModule()?.LotInfo;
                waferPhysicalInfo = this.GetParam_Wafer()?.GetPhysInfo();
                resultMap = new ResultMapData();

                ResultMapConvertType convtype = ResultMapConverter.GetUploadConverterType();

                //AssignDummyBinData(convtype);

                

                BinDataMappingBase binDataMappingBase = null;
                binDataMappingBase = GetBinDataMappingSelector(convtype);

                if (deviceList != null)
                {
                    int waferWidthCount = waferPhysicalInfo.MapCountX.Value;
                    int waferHeightCount = waferPhysicalInfo.MapCountY.Value;

                    var dies = this.GetParam_Wafer().GetSubsInfo().DIEs;

                    Random random = new Random(DateTime.Now.Millisecond);

                    for (int j = 0; j < waferHeightCount; j++)
                    {
                        for (int i = 0; i < waferWidthCount; i++)
                        {
                            IDeviceObject dev = dies[i, (waferHeightCount - 1) - j];

                            DieTypeEnum dieTypeEnum = dev.DieType.Value;
                            byte? mapResult = null;

                            int? bincode = null;

                            if (dieTypeEnum != DieTypeEnum.TEST_DIE)
                            {
                                if (dieTypeEnum == DieTypeEnum.MARK_DIE) // MARK
                                {
                                    mapResult = (byte)TestState.MAP_STS_MARK;
                                }
                                else if (dieTypeEnum == DieTypeEnum.SKIP_DIE)
                                {
                                    mapResult = (byte)TestState.MAP_STS_SKIP;
                                }
                                else
                                {
                                    mapResult = (byte)TestState.MAP_STS_NOT_EXIST;
                                }

                                if (IsTargetDie(i, waferHeightCount - j - 1) == true)
                                {
                                    bincode = binDataMappingBase.GetNoneTestBinCode(DieTypeEnum.TARGET_DIE);
                                }
                                else
                                {
                                    bincode = binDataMappingBase.GetNoneTestBinCode(dieTypeEnum);
                                }

                                if (dev.CurTestHistory != null)
                                {
                                    dev.CurTestHistory.BinCode.Value = (int)bincode;
                                }
                                else
                                {

                                }

                                if (dev.DieType.Value == DieTypeEnum.NOT_EXIST)
                                {
                                    dev.State.Value = DieStateEnum.NOT_EXIST;
                                }

                                if (dev.DieType.Value == DieTypeEnum.MARK_DIE)
                                {
                                    dev.State.Value = DieStateEnum.MARK;
                                }

                                bincode = dev.CurTestHistory.BinCode.Value;
                            }
                            else
                            {
                                // TEST_DIE

                                // 0 ~ 1
                                int resultrnd = random.Next(0, 2);

                                if (dev.CurTestHistory != null)
                                {
                                    dev.CurTestHistory.TestResult.Value = (resultrnd == 0) ? TestState.MAP_STS_PASS : TestState.MAP_STS_FAIL;
                                }
                                else
                                {

                                }

                                bincode = binDataMappingBase.GetTestBinCode(dev.CurTestHistory.TestResult.Value);

                                if (bincode != null)
                                {
                                    if (dev.CurTestHistory != null)
                                    {
                                        dev.CurTestHistory.BinCode.Value = (int)bincode;

                                        if (dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_PASS ||
                                            dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_FAIL)
                                        {
                                            dev.State.Value = DieStateEnum.TESTED;
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    // TODO : 
                                }

                                mapResult = (byte)dev.CurTestHistory.TestResult.Value;
                                bincode = dev.CurTestHistory.BinCode.Value;
                            }

                            if (mapResult != null)
                            {
                                resultMap.mStatusMap.Add((byte)mapResult);
                            }
                            else
                            {
                                LoggerManager.Error($"[ResultMapManager], GetResultMapBase() : dieTypeEnum = {dieTypeEnum}, xindex = {i}, yindex = {j}, Map status is unknown.");
                            }

                            if (bincode != null)
                            {
                                resultMap.mBINMap.Add((long)bincode);
                            }
                            else
                            {
                                LoggerManager.Error($"[ResultMapManager], GetResultMapBase() : dieTypeEnum = {dieTypeEnum}, xindex = {i}, yindex = {j}, Bin code is unknown.");
                            }

                            resultMap.mMapCoordX.Add((int)dev.DieIndex.XIndex);
                            resultMap.mMapCoordY.Add((int)dev.DieIndex.YIndex);

                            // mRePrbMap
                            // 0: Not Re-Probed,
                            // 1: Passed at re-probing 
                            // 2: Failed at re-probing
                            // 3: Perform fail (for special user)

                            // TODO : 

                            resultMap.mRePrbMap.Add(0);

                            resultMap.mDUTMap.Add(dev.DutNumber);

                            // TODO : 
                            resultMap.mTestCntMap.Add(0);

                            // TODO : 
                            resultMap.mInked.Add(false);
                            resultMap.mXWaferCenterDistance.Add(0);
                            resultMap.mYWaferCenterDistance.Add(0);
                            resultMap.mOverdrive.Add(0);
                            resultMap.mTestStartTime.Add(new DateTime());
                            resultMap.mFailMarkInspection.Add(false);
                            resultMap.mNeedleMarkInspection.Add(false);
                            resultMap.mNeedleCleaning.Add(false);
                            resultMap.mNeedleAlign.Add(false);
                        }
                    }

                    ISubstrateInfo waferSubstracteInfo = this.GetParam_Wafer()?.GetSubsInfo();
                    waferSubstracteInfo.UpdateCurrentDieCount();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MakeResultMap(ref object resultmap, bool IsDummy = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ResultMapConvertType convtype = ResultMapConverter.GetUploadConverterType();

                //if (IsDummy == true)
                //{
                //    AssignDummyBinData(convtype);
                //}
                //else
                //{
                //    AssignNoneTestDieBinData(convtype);
                //}

                // ResultMap을 만들기에 앞서, Base data를 생성해야 됨.
                retval = MakeResultMapBaseData(IsDummy);

                if (retval == EventCodeEnum.NONE)
                {
                    if (ResultMapHeader != null && ResultMapData != null)
                    {
                        retval = ResultMapConverter.Convert(ResultMapHeader, ResultMapData, convtype, ref resultmap);

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[ResultMapModule], MakeResultMap() : Convert() is failed.");
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapModule], MakeResultMap() : Base data is null.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[ResultMapModule], MakeResultMap() : Base data is not created.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum SaveResultMapToLocal(object resultmap, FileReaderType readertype, string destPath, Type serializeObjType = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SerializeResultMap(destPath, resultmap, readertype, serializeObjType: serializeObjType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum Upload()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                object rm = null;

                retVal = MakeResultMap(ref rm);

                if (retVal == EventCodeEnum.NONE && rm != null)
                {
                    string destPath = string.Empty;

                    Type serializeobjecttype = ResultMapConverter.GetResultMapObjectType();

                    string namerAlias = ResultMapConverter.GetNamerAlias(FileTrasnferType.UPLOAD);
                    string filename = string.Empty;

                    Namer namer = ResultMapUpDownloader.MapNamer.GetNamer(namerAlias);

                    if (namer != null)
                    {
                        retVal = namer.Run(out filename);
                        bool isExist = false;
                        object obj = null;
                        isExist = namer.ProberMapDictionary.TryGetValue(EnumProberMapProperty.LOTID, out obj);

                        if (isExist == true && obj != null)
                        {
                            destPath = Path.Combine(LocalUploadPath, obj.ToString(), filename);
                            retVal = SaveResultMapToLocal(rm, ResultMapConverter.GetUploadReaderType(), destPath, serializeobjecttype);
                        }
                        else
                        {
                            // TODO : 
                            retVal = EventCodeEnum.UNDEFINED;
                        }

                        if (retVal == EventCodeEnum.NONE)
                        {
                            /////Ann TODO : 
                            if (this.LoaderController() is LoaderController.LoaderController)
                            {
                                var service = (this.LoaderController() as LoaderController.LoaderController).LoaderService;
                                retVal = service.ResultMapUpload(this.LoaderController().GetChuckIndex());
                            }
                            else if (this.LoaderController() is GP_LoaderController)
                            {
                                // TODO : FOUP N개일 때, 2번 풉의 Label의 시작은 26번, 고려되어야 함.
                                var service = (this.LoaderController() as GP_LoaderController).GPLoaderService;
                                retVal = service.ResultMapUpload(this.LoaderController().GetChuckIndex(), filename);
                            }
                            else
                            {
                                // TODO : ERROR
                            }


                            // 초기화
                            this.ResultMapHeader = null;
                            this.ResultMapData = null;
                        }
                        else
                        {
                            LoggerManager.Error($"[ResultMapManager], ResultMapUpDownloader.Upload() faild. retval = {retVal}");
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapManager], Upload() : namer is null.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[ResultMapManager], Upload() : MakeResultMap is failed. retval = {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum ManualUpload(object param, FileManagerType managertype, string destPath, FileReaderType readertype, Type serializeObjtype = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string localdestPath = string.Empty;

                string namerAlias = ResultMapConverter.GetNamerAlias(FileTrasnferType.UPLOAD);
                string filename = string.Empty;

                Namer namer = ResultMapUpDownloader.MapNamer.GetNamer(namerAlias);

                if (namer != null)
                {
                    retval = namer.Run(out filename);
                    localdestPath = Path.Combine(LocalManualUploadPath, filename);
                }

                retval = SaveResultMapToLocal(param, readertype, localdestPath, serializeObjType: serializeObjtype);

                if (retval == EventCodeEnum.NONE)
                {
                    retval = ResultMapUpDownloader.ManualUpload(localdestPath, destPath, managertype);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }

        public EventCodeEnum Download(bool IsLotNameChangedTriggered)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string namerAlias = ResultMapConverter.GetNamerAlias(FileTrasnferType.DOWNLOAD);

                if (string.IsNullOrEmpty(namerAlias) == false)
                {
                    retval = ResultMapUpDownloader.Download(namerAlias, IsLotNameChangedTriggered);
                }
                else
                {
                    LoggerManager.Error($"[ResultMapManager], Download() : GetNamerAlias failed.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }

        public bool IsMapDownloadDone()
        {
            bool retVal = false;
            ILotOPModule lotOpModule = this.LotOPModule();
            string mppPath = string.Empty;

            try
            {
                // TEST CODE 
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// Wafer가 Load된 후, ResultMap을 이용하여, Prober Data를 생성
        /// 이 때, ResultMap을 다운로드해야되는 경우가 존재 함.
        /// </summary>
        /// <param name="lotID"></param>
        /// <param name="waferID"></param>
        /// <returns></returns>
        public EventCodeEnum ConvertResultMapToProberDataAfterReadyToLoadWafer()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.ResultMapHeader == null && this.ResultMapData == null)
                {
                    TransferObject loadobj = null;

                    if (this.LoaderController() is LoaderController.LoaderController)
                    {
                        var service = (this.LoaderController() as LoaderController.LoaderController).LoaderService;
                        retval = service.GetWaferLoadObject(out loadobj);
                    }
                    else if (this.LoaderController() is GP_LoaderController)
                    {
                        // TODO : FOUP N개일 때, 2번 풉의 Label의 시작은 26번, 고려되어야 함.

                        var service = (this.LoaderController() as GP_LoaderController).GPLoaderService;
                        retval = service.GetWaferLoadObject(out loadobj);
                    }
                    else
                    {
                        // TODO : ERROR
                    }
                    LoggerManager.Debug($"[ResultMapManager], ConvertResultMapToProberData() : Wafer Type is {loadobj.WaferType.Value}.");
                    if (loadobj.WaferType.Value != EnumWaferType.POLISH) 
                    {
                        string lotID = string.Empty;
                        string waferID = string.Empty;
                        string slotNum = string.Empty;

                        lotID = this.LotOPModule().LotInfo.LotName.Value;

                        if (loadobj != null)
                        {
                            waferID = loadobj.OCR.Value;
                            slotNum = loadobj.OriginHolder.Index.ToString();
                        }

                        MapHeaderObject header = new MapHeaderObject();
                        ResultMapData map = new ResultMapData();

                        object mapfile = null;

                        // 결과맵이 Download 되어 있는 경우
                        // Trigger의 값이 LOT_NAME_INPUT_TRIGGER인 경우,
                        // Lot name 입력 또는 Lot 시작 시, Download 되어 있을 수 있다.

                        string namerAlias = ResultMapConverter.GetNamerAlias(FileTrasnferType.DOWNLOAD);
                        string filename = string.Empty;

                        Namer namer = ResultMapUpDownloader.MapNamer.GetNamer(namerAlias);

                        if (namer != null)
                        {
                            // 결과맵 데이터의 일부를 얻어오기 위한 임시 코드 
                            TransferObject to = null;

                            if (SystemManager.SysteMode == SystemModeEnum.Single)
                            {
                                (this.LoaderController() as ILoaderControllerExtension)?.LoaderService?.GetWaferLoadObject(out to);
                            }
                            else
                            {
                                (this.LoaderController() as GP_LoaderController).GPLoaderService.GetWaferLoadObject(out to);
                            }

                            if (to != null)
                            {
                                if (to.Type.Value == SubstrateTypeEnum.Wafer)
                                {
                                    int sn = to.OriginHolder.Index;

                                    namer.SetValue(EnumProberMapProperty.LOTID, lotID);
                                    namer.SetValue(EnumProberMapProperty.WAFERID, waferID);
                                    namer.SetValue(EnumProberMapProperty.SLOTNO, sn);
                                }
                            }
                        }
                        else
                        {
                            // TODO : 
                            LoggerManager.Error($"[ResultMapManager], ConvertResultMapToProberData() : namer is null.");
                        }

                        ResultMapDownloadTriggerEnum triggerEnum = this.LotOPModule().LotInfo.ResultMapDownloadTrigger;

                        if (triggerEnum == ResultMapDownloadTriggerEnum.LOT_NAME_INPUT_TRIGGER)
                        {
                            // Ann : 서버에서 지정해서 로드 전에 다운 받도록 수정함 
                            // 안되어 있는 경우, 에러

                            /////Ann TODO : 
                            if (this.LoaderController() is LoaderController.LoaderController)
                            {
                                var service = (this.LoaderController() as LoaderController.LoaderController).LoaderService;
                                retval = service.ResultMapDownload(this.LoaderController().GetChuckIndex(), filename);
                            }
                            else if (this.LoaderController() is GP_LoaderController)
                            {
                                retval = namer.Run(out filename);
                                var service = (this.LoaderController() as GP_LoaderController).GPLoaderService;
                                service.ResultMapDownload(this.LoaderController().GetChuckIndex(), filename);
                            }
                            else
                            {
                                // TODO : ERROR
                            }

                            //if (IsMapDownloadDone() == false)
                            //{
                            //    // TODO : ERROR
                            //}
                            //else
                            //{
                            //    retval = EventCodeEnum.NONE;
                            //}

                        }
                        else if (triggerEnum == ResultMapDownloadTriggerEnum.READ_WAFERID_TRIGGER)
                        {
                            // TODO : 이 타이밍에 이미 받았을 경우가 있는가?
                            // 있는 경우 확인하여, 다시 안받아도 되는가?

                            // TODO : 현재 WaferID에 해당하는 파일만 가져오면 되는가?

                            if (namer != null)
                            {
                                // TODO : 이 때, Dictionary에 오브젝트가 없는 경우 확인 필요.
                                // 프로그램 최초 기동 후, MPP 돌리면 어떻게 되는지 확인할 것.

                                //retval = ResultMapUpDownloader.Download(namerAlias, false);
                            }
                        }
                        else
                        {
                            // TODO : 
                            retval = EventCodeEnum.UNDEFINED;
                        }

                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = namer.Run(out filename);
                            bool isExist = false;
                            object obj = null;

                            isExist = namer.ProberMapDictionary.TryGetValue(EnumProberMapProperty.LOTID, out obj);

                            if (isExist == true && obj != null)
                            {
                                string filepath = Path.Combine(LocalDownloadPath, obj.ToString(), filename);

                                retval = DeserializeResultMap(filepath,
                                                          ResultMapManagerSysParam.DownloadType.Value,
                                                          ResultMapConverter.GetDownloadReaderType(),
                                                          ref mapfile,
                                                          ResultMapConverter.GetResultMapObjectType());


                                isExist = namer.ProberMapDictionary.TryGetValue(EnumProberMapProperty.SLOTNO, out obj);

                                if (isExist == true)
                                {
                                    LoggerManager.Debug($"[ResultMapManager], ConvertResultMapToProberData() : DeserializeResultMap is success. LOTID = {lotID}, WAFERID = {waferID}, SLOTNO = {obj.ToString()}");
                                }
                                else
                                {
                                    LoggerManager.Debug($"[ResultMapManager], ConvertResultMapToProberData() : DeserializeResultMap is success. LOTID = {lotID}, WAFERID = {waferID}, SLOTNO = UNDEFINED");
                                }

                                //destPath = Path.Combine(LocalUploadPath, obj.ToString(), filename);
                                //retVal = SaveResultMapToLocal(rm, ResultMapConverter.GetUploadReaderType(), destPath, serializeobjecttype);
                            }
                            else
                            {
                                // TODO : 
                                retval = EventCodeEnum.UNDEFINED;
                            }

                            if (retval == EventCodeEnum.NONE)
                            {
                                ResultMapConvertType converterType = GetDownloadConverterType();

                                retval = SetMapHeaderExtensionBase(header, converterType);
                                retval = header.MakeProberMapPropertyDictionay();

                                retval = ResultMapConverter.ConvertBack(mapfile, converterType, ref header, ref map);

                                if (retval == EventCodeEnum.NONE)
                                {
                                    UpdateResultMapData(header, map);
                                }
                                else
                                {
                                    LoggerManager.Error($"[ResultMapManager], ConvertResultMapToProberData() : ConvertBack() Failed. retval = {retval}");
                                }
                            }
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.NONE;
                        LoggerManager.Debug($"[ResultMapManager], ConvertResultMapToProberData() : Wafer Type is POLISH.");
                    }
                }
                else
                {
                    // TODO : ERROR?
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// destPath에 파일 저장
        /// </summary>
        /// <param name="destPath"></param>
        /// <param name="resultmap"></param>
        /// <param name="readertype"></param>
        /// <param name="serializeObjType"></param>
        /// <returns></returns>
        private EventCodeEnum SerializeResultMap(string destPath, object resultmap, FileReaderType readertype, Type serializeObjType = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (string.IsNullOrEmpty(destPath) == false)
                {
                    DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(destPath));

                    if (dir.Exists == false)
                    {
                        dir.Create();
                    }
                }
                else
                {
                    LoggerManager.Error($"[ResultMapManager], SerializeResultMap() : Path is empty.");
                }

                bool isSuccess = false;

                if (readertype == FileReaderType.BINARY)
                {
                    isSuccess = SaveBinaryType(resultmap, destPath);
                }
                else if (readertype == FileReaderType.STREAM)
                {
                    isSuccess = SaveStreamType(resultmap, destPath);
                }
                else if (readertype == FileReaderType.XML)
                {
                    isSuccess = SerializeManager.Serialize(destPath, resultmap, serializeObjType: serializeObjType, serializerType: SerializerType.XML);
                }
                else if (readertype == FileReaderType.EXTENDEDXML)
                {
                    isSuccess = SerializeManager.Serialize(destPath, resultmap, serializeObjType: serializeObjType, serializerType: SerializerType.EXTENDEDXML);
                }

                if (isSuccess == true)
                {
                    LoggerManager.Debug($"[ResultMapManager], SaveResultMapToLocal() : Success. Path = {destPath}");

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// sourcePath에 존재하는 파일을 Deserialize
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="managerType"></param>
        /// <param name="readertype"></param>
        /// <param name="_param"></param>
        /// <param name="deserializeObjtype"></param>
        /// <returns></returns>
        private EventCodeEnum DeserializeResultMap(string sourcePath, FileManagerType managerType, FileReaderType readertype, ref object _param, Type deserializeObjtype = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                bool isSuccess = false;

                if (File.Exists(sourcePath))
                {
                    if (readertype == FileReaderType.BINARY)
                    {
                        retval = LoadBinaryType(sourcePath, ref _param);

                    }
                    else if (readertype == FileReaderType.STREAM)
                    {
                        retval = LoadStreamType(sourcePath, ref _param);
                    }
                    else if (readertype == FileReaderType.XML)
                    {
                        isSuccess = SerializeManager.Deserialize(sourcePath, out _param, deserializeObjType: deserializeObjtype, deserializerType: SerializerType.XML);
                    }
                    else if (readertype == FileReaderType.EXTENDEDXML)
                    {
                        isSuccess = SerializeManager.Deserialize(sourcePath, out _param, deserializeObjType: deserializeObjtype, deserializerType: SerializerType.EXTENDEDXML);
                    }

                    if (isSuccess == true)
                    {
                        retval = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"[ResultMapManager], DeserializeResultMap() : Not exist file. Path = {sourcePath}, FileName = {Path.GetFileName(sourcePath)}");

                    retval = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// Deserialize 된 오브젝트를 이용하여, ProberMapData를 생성
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="managerType"></param>
        /// <param name="readertype"></param>
        /// <param name="deserializeObjtype"></param>
        /// <returns></returns>
        public EventCodeEnum ManualConvertResultMapToProberData(string filepath, FileManagerType managerType, FileReaderType readertype, Type deserializeObjtype = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            MapHeaderObject header = new MapHeaderObject();
            ResultMapData map = new ResultMapData();

            try
            {
                object mapfile = null;

                retval = DeserializeResultMap(filepath, managerType, readertype, ref mapfile, deserializeObjtype);

                if (retval == EventCodeEnum.NONE)
                {
                    ResultMapConvertType converterType = this.ResultMapManager().GetDownloadConverterType();

                    retval = SetMapHeaderExtensionBase(header, converterType);
                    retval = header.MakeProberMapPropertyDictionay();

                    retval = ResultMapConverter.ConvertBack(mapfile, converterType, ref header, ref map);

                    if (retval == EventCodeEnum.NONE)
                    {
                        UpdateResultMapData(header, map);

                        ApplyBaseMap(header, map);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public EventCodeEnum UpdateResultMapData(MapHeaderObject _headerinfo, ResultMapData _resultmapinfo)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.ResultMapHeader = _headerinfo;
                this.ResultMapData = _resultmapinfo;

                ResultMapUpDownloader.SetHeaderData(this.ResultMapHeader);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public IParam GetResultMapConvIParam()
        {
            return ResultMapConverter.resultMapConverterIParameter;
        }

        public byte[] GetResultMapConvParam()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(ResultMapConverter.resultMapConverterIParameter, typeof(ResultMapConverterParameter));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetResultMapConvParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        public void SetResultMapConvIParam(byte[] param)
        {
            try
            {
                this.ResultMapConverter.SetResultMapConvIParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public bool SetResultMapByFileName(byte[] device, string resultmapname)
        {
            bool ret = false;
            try
            {
                ret = this.ResultMapUpDownloader.SetResultMapByFileName(device, resultmapname);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum ApplyBaseMap(MapHeaderObject mapHeader, ResultMapData resultMap)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            ILotInfo lotInfo = this.LotOPModule()?.LotInfo;
            IDeviceObject[,] dies = null;
            int widthSize = 0;
            int heightSize = 0;

            try
            {
                if (mapHeader != null && resultMap != null)
                {
                    widthSize = mapHeader.MapConfigData.MapSizeX;
                    heightSize = mapHeader.MapConfigData.MapSizeY;

                    ISubstrateInfo substrateInfo = this.GetParam_Wafer().GetSubsInfo();
                    dies = substrateInfo.DIEs;

                    if (widthSize != dies.GetLength(0) || heightSize != dies.GetLength(1)
                        )
                    {
                        throw new Exception($"[{this.GetType().Name}] ApplyBaseMap() : width or height size is not matched with Dies length.");
                    }

                    for (int i = 0; i < widthSize; i++)
                    {
                        for (int j = 0; j < heightSize; j++)
                        {
                            IDeviceObject dev = dies[i, (heightSize - 1) - j];

                            int idx = GetStatusMapIndex(i, j);
                            byte bTestState = resultMap.mStatusMap[idx];

                            if (dev.DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                if (IsTestDie(bTestState))
                                {
                                    if (dev.TestHistory == null)
                                    {
                                        dev.TestHistory = new List<TestHistory>();
                                    }

                                    TestHistory testHistory = new TestHistory();

                                    testHistory.TestResult.Value = (TestState)bTestState;
                                    testHistory.BinCode.Value = (int)resultMap.mBINMap[idx];

                                    dev.TestHistory.Add(testHistory);
                                    dev.CurTestHistory = dev.TestHistory.Last();

                                    dev.State.Value = GetDieState(bTestState);

                                    //if(dev.State.Value == DieStateEnum.TESTED)
                                    //{
                                    //    LoggerManager.Debug($"[TEST] X = {dev.DieIndex.XIndex}, Y = {dev.DieIndex.YIndex}");
                                    //}
                                }
                                else
                                {
                                    // TEST_DIE인데, 
                                    // TestState.MAP_STS_TEST
                                    // TestState.MAP_STS_PASS
                                    // TestState.MAP_STS_FAIL 가 아니다?

                                }
                            }
                        }
                    }

                    int TestedCnt = substrateInfo.Devices.Count(x => x.State.Value == DieStateEnum.TESTED);

                    //foreach (var item in substrateInfo.Devices)
                    //{
                    //    if(item.State.Value == DieStateEnum.TESTED)
                    //    {
                    //        LoggerManager.Debug($"[TEST] X = {item.DieIndex.XIndex}, Y = {item.DieIndex.YIndex}");
                    //    }
                    //}

                    LoggerManager.Debug($"[{this.GetType().Name}] ApplyBaseMap() : Success. TESTED count = {TestedCnt}");

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] ApplyBaseMap() : mapHeader is null. resultMap is null.");

                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;

            int GetStatusMapIndex(int x, int y)
            {
                return (y * widthSize) + x;
            }

            //bool IsMeaningfulDie(byte dieState)
            //{
            //    bool isTestDie = false;

            //    if (dieState == (byte)TestState.MAP_STS_MARK ||
            //        dieState == (byte)TestState.MAP_STS_SKIP ||
            //        IsTestDie(dieState)
            //        )
            //    {
            //        isTestDie = true;
            //    }

            //    return isTestDie;
            //}

            bool IsTestDie(byte dieState)
            {
                bool isTestDie = false;

                if (dieState == (byte)TestState.MAP_STS_TEST ||
                    dieState == (byte)TestState.MAP_STS_PASS ||
                    dieState == (byte)TestState.MAP_STS_FAIL
                    )
                {
                    isTestDie = true;
                }

                return isTestDie;
            }

            DieStateEnum GetDieState(byte bTestState)
            {
                DieStateEnum dieState = DieStateEnum.UNKNOWN;

                if (IsTestDie(bTestState))
                {
                    if (bTestState == (byte)TestState.MAP_STS_TEST)
                    {
                        dieState = DieStateEnum.NORMAL;
                    }
                    else
                    {
                        // PASS 또는 FAIL이면
                        dieState = DieStateEnum.TESTED;
                    }
                }
                else if (bTestState == (byte)TestState.MAP_STS_MARK)
                {
                    dieState = DieStateEnum.MARK;
                }

                return dieState;
            }
        }

        public bool NeedUpload()
        {
            bool retval = false;

            try
            {
                if (ResultMapManagerSysParam.UploadHDDEnable.Value == true ||
                    ResultMapManagerSysParam.UploadFTPEnable.Value == true)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }



        //public bool IsUsingResultMap()
        //{
        //    bool retVal = false;

        //    try
        //    {
        //        // TODO : 
        //        //retVal = this.ResultMapSysParam?.IsEnable?.Value ?? false;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}



        public char[,] GetASCIIMap()
        {
            char[,] retval = null;

            try
            {
                retval = ResultMapConverter.GetASCIIMap();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public object GetResultMap()
        //{
        //    object retval = null;

        //    try
        //    {
        //        if(ResultMapConverter != null)
        //        {
        //            retval = ResultMapConverter.GetResultMap();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum SaveRealTimeProbingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                string savePath = string.Empty;

                // TODO : TEST CODE
                //savePath = @"C:\ProberSystem\ResultMap";

                //retVal = mResultMapManager.SaveRealTimeProbingData(savePath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum LoadRealTimeProbingData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //MapHeaderObject mapHeaderObj = null;
            //ResultMapData resultMap = null;
            //string fileAdditionalStr = string.Empty;
            //string loadPath = string.Empty;

            try
            {
                // TODO : 
                //retVal = mResultMapManager.LoadRealTimeProbingData(loadPath, out mapHeaderObj, out resultMap, fileAdditionalStr);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ResultMapConvertType GetUploadConverterType()
        {
            ResultMapConvertType retval = ResultMapConvertType.OFF;

            try
            {
                retval = this.ResultMapConverter.GetUploadConverterType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ResultMapConvertType GetDownloadConverterType()
        {
            ResultMapConvertType retval = ResultMapConvertType.OFF;

            try
            {
                retval = this.ResultMapConverter.GetDownloadConverterType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string[] GetNamerAliaslist()
        {
            string[] retval = null;

            try
            {
                retval = ResultMapUpDownloader.GetNamerAliaslist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object GetOrgResultMap()
        {
            object retval = null;

            try
            {
                retval = ResultMapConverter.GetOrgResultMap();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        #endregion

        // TODO : 파라미터화?
        public Encoding encoding = Encoding.GetEncoding(1252);

        public static byte[] SerializeToBytes<T>(T item)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        public static object DeserializeFromBytes(byte[] bytes)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                return formatter.Deserialize(stream);
            }
        }

        // TODO : SerializeManager 사용하는 컨셉으로 변경 가능 확인 및 제거
        #region BINARY
        public EventCodeEnum LoadBinaryType(string _filepath, ref object _param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // TODO : 테스트 필요.
                using (BinaryReader reader = new BinaryReader(File.Open(_filepath, FileMode.Open), encoding))
                {
                    //// Pre .Net version 4.0
                    //const int bufferSize = 4096;
                    //using (var ms = new MemoryStream())
                    //{
                    //    byte[] buffer = new byte[bufferSize];
                    //    int count;

                    //    while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    //        ms.Write(buffer, 0, count);
                    //    return ms.ToArray();
                    //}

                    // .Net 4.0 or Newer
                    using (var ms = new MemoryStream())
                    {
                        reader.BaseStream.CopyTo(ms);
                        _param = ms.ToArray();
                    }

                    //while (reader.BaseStream.Position < reader.BaseStream.Length)
                    //{
                    //    char readchar = reader.ReadChar();

                    //    castedmapfiles.Add(readchar);
                    //}
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public bool SaveBinaryType(object _param, string _filepath)
        {
            bool retval = false;

            try
            {
                byte[] bytes = SerializeToBytes(_param);

                using (BinaryWriter writer = new BinaryWriter(File.Open(_filepath, FileMode.Create), encoding))
                {
                    writer.Write(bytes);
                }

                retval = true;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion

        #region STREAM
        public EventCodeEnum LoadStreamType(string _filepath, ref object _param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                using (StreamReader sr = new StreamReader(_filepath, encoding))
                {
                    _param = sr.ReadToEnd();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool SaveStreamType(object _param, string _filepath)
        {
            bool retval = false;

            try
            {
                using (StreamWriter sw = new StreamWriter(_filepath, false, encoding))
                {
                    sw.Write(_param);
                    sw.Close();
                }

                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion

    }
}
