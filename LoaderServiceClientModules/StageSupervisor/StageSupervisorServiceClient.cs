using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderServiceClientModules
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderMapView;
    using LogModule;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Data;
    using ProberInterfaces.Device;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.Loader;
    using ProberInterfaces.NeedleClean;
    using RelayCommandBase;
    using SerializerUtil;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using SystemExceptions;


    public class StageSupervisorServiceClient : IStageSupervisorServiceClient, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
#pragma warning disable 0067
        public event EventHandler ChangedWaferObjectEvent;
        public event EventHandler ChangedProbeCardObjectEvent;
#pragma warning restore 0067
        #region //..Property

        //private Autofac.IContainer _Container;

        //public Autofac.IContainer Container
        //{
        //    //get { return this.GetContainer(); }
        //    get { return _Container; }
        //    set { _Container = value; }
        //}
        public Autofac.IContainer _Container
        {
            get { return this.GetContainer(); }
        }

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        object lockObject = new object();
        private StageObject Stage
        {
            get { return (StageObject)LoaderCommunicationManager.SelectedStage; }
        }

        public bool StageMoveFlag_Display { get; set; }


        private double _MoveTargetPosX;
        public double MoveTargetPosX
        {
            get { return _MoveTargetPosX; }
            set
            {
                if (value != _MoveTargetPosX)
                {
                    _MoveTargetPosX = value;
                    RaisePropertyChanged();
                    //var stage = LoaderCommunicationManager.GetStageSupervisorClient();
                    //if(stage != null)
                    //    stage.SetStageClickMoveTarget(_MoveTargetPosX, _MoveTargetPosY);
                    ////Stage.StageInfo.StageProxy.SetStageClickMoveTarget(_MoveTargetPosX, _MoveTargetPosY);
                }
            }
        }

        private double _MoveTargetPosY;
        public double MoveTargetPosY
        {
            get { return _MoveTargetPosY; }
            set
            {
                if (value != _MoveTargetPosY)
                {
                    lock (lockObject)
                    {
                        _MoveTargetPosY = value;
                        RaisePropertyChanged();
                        //var stage = LoaderCommunicationManager.GetStageSupervisorClient();
                        //if (stage != null)
                        //    stage.SetStageClickMoveTarget(_MoveTargetPosX, _MoveTargetPosY);
                        ////Stage.StageInfo.StageProxy.SetStageClickMoveTarget(_MoveTargetPosX, _MoveTargetPosY);
                    }
                }
            }
        }

        public double PinZClearance { get; set; }
        public double PinMaxRegRange { get; set; }
        public double PinMinRegRange { get; set; }
        public double WaferRegRange { get; set; }
        public double WaferMaxThickness { get; set; }

        public IWaferObject WaferObject
        {
            get
            {
                return Wafer as IWaferObject;
            }
            set
            {
                //minskim// 이전 Wafer object memory 정리 추가(Hard reference)
                ReleaseOldWaferObject();
                Wafer = value as WaferObject;

                ///Stage WaferObject MapIndex 와의 동기를 맞추기 위해 
                ///(Loader 측 MapView에서 Map 클릭시 Stage 에 Index를 전달하기 위해)
                this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += LoaderCommunicationManager.UpdateMapIndex;
                RaisePropertyChanged();
            }
        }
        public WaferObject Wafer { get; set; } = new WaferObject();

        private void ReleaseOldWaferObject()
        {
            //minskim// 이전 WaferObject에 추가된 Delegate 함수를 제거 해야 한다.
            if (Wafer.ChangeMapIndexDelegate != null)
            {
                Wafer.ChangeMapIndexDelegate -= LoaderCommunicationManager.UpdateMapIndex;
            }
            ISubstrateInfo subsinfo = Wafer.GetSubsInfo();
            if (subsinfo != null)
                subsinfo.DIEs = null; //Reserved GC
            Wafer = null; //Reserved GC
            GC.Collect();
        }

        public IMarkObject MarkObject { get; set; }

        private IStageMove _StageModuleState;

        public IStageMove StageModuleState
        {
            get
            {
                if (_StageModuleState == null)
                    StageModuleState = new StageMoveServiceClient();
                return _StageModuleState;
            }
            set { _StageModuleState = value; }
        }

        private IProbeCard _ProbeCardInfo;
        public IProbeCard ProbeCardInfo
        {
            get { return _ProbeCardInfo; }
            set
            {
                if (value != _ProbeCardInfo)
                {
                    _ProbeCardInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ProbingInfo ProbingProcessStatus { get; set; }


        public double UserCoordXPos { get; set; }
        public double UserCoordYPos { get; set; }
        public double UserCoordZPos { get; set; }
        public double UserWaferIndexX { get; set; }
        public double UserWaferIndexY { get; set; }
        public LightJogViewModel PnpLightJog { get; set; }
        public IHexagonJogViewModel PnpMotionJog { get; set; }

        public double WaferINCH6Size { get; set; }

        public double WaferINCH8Size { get; set; }

        public double WaferINCH12Size { get; set; }

        public ICylinderManager IStageCylinderManager { get; set; }

        private INeedleCleanObject _NCObject
             = new NeedleCleanObject();
        public INeedleCleanObject NCObject
        {
            get { return _NCObject; }
            set
            {
                if (value != _NCObject)
                {
                    _NCObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public INeedleCleanObject NCObject { get; set; }

        public ITouchSensorObject TouchSensorObject { get; set; }

        public EventHandler MachineInitEvent { get; set; }
        public EventHandler MachineInitEndEvent { get; set; }
        public bool AcceptUpdateDisp { get; set; }
        ProberInterfaces.StageStateEnum IStageSupervisor.StageMoveState { get; }

        #endregion

        #region //..MotionUpdate
        private ObservableCollection<ProbeAxisObject> _Axes = new ObservableCollection<ProbeAxisObject>();
        public ObservableCollection<ProbeAxisObject> Axes
        {
            get { return _Axes; }
            set
            {
                if (value != _Axes)
                {
                    _Axes = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void OnAxisStateUpdated(ProbeAxisObject axis)
        {
            ProbeAxisObject updateAxis = Axes.FirstOrDefault(ax => ax.AxisType == axis.AxisType);
            if (updateAxis != null)
            {
                updateAxis.Status = axis.Status;
                LoggerManager.Debug($"StateUpdated({axis.Label}): Axis state  has been updated.");
            }
            else
            {
                LoggerManager.Debug($"StateUpdated({axis.Label}): Axis does not exist on list.");
            }
        }


        #endregion

        Task _ClickToMoveTask;
        private AsyncCommand<object> _ClickToMoveLButtonDownCommand;
        public ICommand ClickToMoveLButtonDownCommand
        {
            get
            {
                if (null == _ClickToMoveLButtonDownCommand)
                {
                    _ClickToMoveLButtonDownCommand = new AsyncCommand<object>(ClickToMoveLButtonDown, showWaitMessage:"Move to clicked position");
                }

                return _ClickToMoveLButtonDownCommand;
            }
        }

        private GPCellModeEnum _StageMode;

        public GPCellModeEnum StageMode
        {
            get { return _StageMode; }
            set { _StageMode = value; }
        }
        private int _SlotIndex;

        public int SlotIndex
        {
            get { return _SlotIndex; }
            set { _SlotIndex = value; }
        }

        public InitPriorityEnum InitPriority { get; set; }

        public bool Initialized { get; set; }

        public int AbsoluteIndex => throw new NotImplementedException();


        public StreamingModeEnum StreamingMode { get; set; }
        private bool _IsRecipeDownloadEnable = true;

        public bool IsRecipeDownloadEnable
        {
            get { return _IsRecipeDownloadEnable; }
            set { _IsRecipeDownloadEnable = value; }
        }
        private bool _IsRecoveryMode;
        //리커버리가 필요한 모드일때는 Loader쪽에서 Cell을 함부로 제어해선 안된다.
        public bool IsRecoveryMode
        {
            get { return _IsRecoveryMode; }
            set { _IsRecoveryMode = value; }
        }

        public IStageMoveLockStatus IStageMoveLockStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IStageMoveLockParameter IStageMoveLockParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsModeChanging { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        object clickLockObject = new object();


        public async Task ClickToMoveLButtonDown(object enableClickToMove)
        {
            try
            {
                bool IsEnable = (bool)enableClickToMove;

                if (IsEnable)
                {

                    _ClickToMoveTask = Task.Run(() =>
                    {
                        lock (clickLockObject)
                        {
                            var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();

                            if (stage != null)
                            {
                                stage.SetStageClickMoveTarget(_MoveTargetPosX, _MoveTargetPosY);
                            }

                            if (stage != null)
                            {
                                var t = stage.StageClickMove(enableClickToMove);
                                t.Wait();
                            }
                        }
                    });

                    await _ClickToMoveTask;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        //public void SetContainer(Autofac.IContainer container)
        //{
        //    _Container = container;
        //}

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            StageModuleState = new StageMoveServiceClient();
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            StageModuleState = new StageMoveServiceClient();
            return EventCodeEnum.NONE;
        }

        public bool IsAvailableLoaderRemoteMediator()
        {
            return false;
        }

        public void BindDelegateEventService(string uri)
        {
            throw new NotImplementedException();
        }

        public void BindDispService(string uri)
        {
            throw new NotImplementedException();
        }

        public void CallWaferobjectChangedEvent()
        {
            //throw new NotImplementedException();
        }

        public Task ChangeDeviceFuncUsingName(string devName)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CheckAvailableStageAbsMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CheckAvailableStageRelMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy)
        {
            throw new NotImplementedException();
        }

        public bool CheckAxisBusy()
        {
            throw new NotImplementedException();
        }

        public bool CheckAxisIdle()
        {
            throw new NotImplementedException();
        }

        public ExceptionReturnData ConvertToExceptionErrorCode(Exception err)
        {
            throw new NotImplementedException();
        }

        public void DoLot()
        {
            throw new NotImplementedException();
        }

        public Task DoSystemInit(bool showMessageDialogFlag = true)
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> DoWaferAlign()
        {
            throw new NotImplementedException();
        }

        public string GetDeviceName()
        {
            throw new NotImplementedException();
        }

        public List<DeviceObject> GetDevices()
        {
            throw new NotImplementedException();
        }

        public CellInitModeEnum GetStageInitState()
        {
            throw new NotImplementedException();
        }

        public byte[] GetWaferObject()
        {
            throw new NotImplementedException();
        }

        public void UpdateProbeCardObject()
        {
            byte[] bytes = null;
            IProbeCard card = null;

            try
            {
                bytes = GetProbeCardObject();

                card = ByteToProbeCard(bytes);

                SetProbeCardObject(card);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public byte[] GetProbeCardObject()
        {
            byte[] bytes = null;

            bytes = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetProbeCardObject();

            return bytes;
        }


        public byte[] GetMarkObject()
        {
            byte[] bytes = null;

            bytes = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetMarkObject();

            return bytes;
        }

        public IProbeCard ByteToProbeCard(byte[] bytes)
        {
            IProbeCard retval = null;

            try
            {
                object target = null;

                var uncompBuff = bytes;
                if (uncompBuff != null)
                {
                    var result = SerializeManager.DeserializeFromByte(uncompBuff, out target, typeof(ProbeCard));

                    if (result == true)
                    {
                        retval = target as ProbeCard;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetProbeCardObject(IProbeCard param)
        {
            ProbeCardInfo = param;
        }

        public EnumSubsStatus GetWaferStatus()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitDevParameter()
        {
            throw new NotImplementedException();
        }

        public void InitService()
        {
            throw new NotImplementedException();
        }

        public bool IsExistParamFile(string paramPath)
        {
            throw new NotImplementedException();
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> LoaderInit()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadNCSysObject()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadProberCard()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadWaferObject()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //RetVal = Wafer.LoadDevParameter();

                IParam tmpparam = Wafer.WaferDevObjectRef;
                var devmanger = this.GetLoaderContainer().Resolve<IDeviceManager>();
                string fullpath = devmanger.GetFullPath(this.Wafer.WaferDevObjectRef, LoaderCommunicationManager.SelectedStage.Index);
                this.LoadParameter(ref tmpparam, typeof(WaferDevObject), null, fullpath);
                Wafer.WaferDevObject = (WaferDevObject)tmpparam;
                //Wafer.WaferDevObject.Init();
                if (RetVal == EventCodeEnum.NONE)
                {
                    //CallWaferobjectChangedEvent();
                    //Extensions_IParam.CollectCommonElement(Wafer, this.GetType().Name);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum MOVETONEXTDIE()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveNCSysObject()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveProberCard()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveWaferObject()
        {
            return EventCodeEnum.NONE;
        }

        public void SetAcceptUpdateDisp(bool flag)
        {
            if (LoaderCommunicationManager.SelectedStage != null)
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                if (stage != null)
                {
                    stage.SetAcceptUpdateDisp(flag);
                }
            }
        }

        public bool IsHaveReservationRecipe(int foupnumber)
        {
            return false;
        }
        public void SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1, bool showprogress = true, bool checkreserve = true)
        {
            return;
        }

        public void LoadDevice(string devicename, int foupnumber = -1, bool isreserve = false)
        {

        }

        public void SetStageInitState(CellInitModeEnum e)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetWaferObjectStatus()
        {
            return EventCodeEnum.NONE;
        }

        public void StageSupervisorStateTransition(StageState state)
        {
            throw new NotImplementedException();
        }

        public Task<ErrorCodeResult> SystemInit()
        {
            throw new NotImplementedException();
        }

        public void SetMoveTargetPos(double xpos, double ypos)
        {

        }
        public void SetWaferMapCam(EnumProberCam cam)
        { return; }

        public void SetErrorCodeAlarm(EventCodeEnum errorcode)
        { return; }


        public byte[] GetNCObject()
        {
            return null;
        }
        public void SetWaitCancelDialogHashCode(string hashCode)
        {
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                    if (stage != null)
                    {
                        stage.SetWaitCancelDialogHashCode(hashCode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LotPause()
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> DoPinAlign()
        {
            throw new NotImplementedException();
        }

        //public void DoPMI()
        //{
        //    throw new NotImplementedException();
        //}

        public void SetEMG(EventCodeEnum errorCode)
        {

        }

        public void InitStageService(int stageAbsIndex = 0)
        {
            return;
        }

        public void DeInitService()
        {
            return;
        }

        public void SetVacuum(bool ison)
        {
            return;
        }

        public Task InitLoaderClient()
        {
            return Task.CompletedTask;
        }

        public void SetDynamicMode(DynamicModeEnum dynamicModeEnum)
        {
            try
            {
                this.LotOPModule().LotInfo.DynamicMode.Value = dynamicModeEnum;
                LoggerManager.Debug($"Set to DynamicMode : {dynamicModeEnum}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MoveStageToTargetPos(object enableClickToMove)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                if (stage != null)
                {
                    //stage.StageClickMove(enableClickToMove);
                    stage.MoveStageToTargetPos(enableClickToMove);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetDevice()
        {
            return null;
        }
        public byte[] GetLog(string date)
        {
            return null;
        }
        public List<string> GetStageDebugDates()
        {
            return null;
        }

        public List<string> GetStageRMDates(string filename)
        {
            return null;
        }
        public EventCodeEnum WaferHighViewIndexCoordMove(long mix, long miy)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                if (stage != null)
                {
                    return stage.WaferHighViewIndexCoordMove(mix, miy);
                }
                return EventCodeEnum.STAGEMOVE_WAFER_HIGH_VIEW_MOVE_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.STAGEMOVE_WAFER_HIGH_VIEW_MOVE_ERROR;
            }
        }

        public EventCodeEnum WaferLowViewIndexCoordMove(long mix, long miy)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                if (stage != null)
                {
                    return stage.WaferLowViewIndexCoordMove(mix, miy);
                }
                return EventCodeEnum.STAGEMOVE_WAFER_HIGH_VIEW_MOVE_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.STAGEMOVE_WAFER_LOW_VIEW_MOVE_ERROR;
            }
        }

        public IStagePIV GetStagePIV()
        {
            return null;
        }


        public bool ReadyRecipe()
        {
            return false;
        }

        public Element<AlignStateEnum> GetAlignState(AlignTypeEnum AlignType)
        {
            throw new NotImplementedException();
        }

        public SubstrateInfoNonSerialized GetSubstrateInfoNonSerialized()
        {
            SubstrateInfoNonSerialized retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetSubstrateInfoNonSerialized();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public byte[] GetDIEs()
        {
            throw new NotImplementedException();
        }

        public EnumWaferType GetWaferType()
        {
            throw new NotImplementedException();
        }

        public byte[] GetLogFromFilename(List<string> debug, List<string> temp, List<string> pin, List<string> pmi, List<string> lot)
        {
            return null;
        }
        public byte[] GetPinImageFromStage(List<string> pinImage)
        {
            return null;
        }
        public byte[] GetLogFromFileName(EnumUploadLogType logtype, List<string> data)
        {
            return null;
        }
        public byte[] GetRMdataFromFileName(string filename)
        {
            return null;
        }
        public byte[] GetODTPdataFromFileName(string filename)
        {
            return null;
        }

        public List<string> GetStageTempDates()
        {
            return null;

        }

        public List<string> GetStagePinDates()
        {
            return null;

        }

        public List<string> GetStagePMIDates()
        {
            return null;
        }
        public List<string> GetStageLotDates()
        {
            return null;
        }
        public WaferObjectInfoNonSerialized GetWaferObjectInfoNonSerialize()
        {
            WaferObjectInfoNonSerialized retVal = null;
            try
            {
                retVal = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetWaferObjectInfoNonSerialize();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void WaferIndexUpdated(long xindex, long yindex)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>()?.WaferIndexUpdated(xindex, yindex);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
        }

        public EventCodeEnum CheckPinPadParameterValidity()
        {
            return LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().CheckPinPadParameterValidity();
        }

        public EventCodeEnum GetPinDataFromPads()
        {
            return LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetPinDataFromPads();
        }

        public PROBECARD_TYPE GetProbeCardType()
        {
            return LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetProbeCardType();
        }

        public int DutPadInfosCount()
        {
            return LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DutPadInfosCount();
        }

        public EventCodeEnum InitGemConnectService()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum DeInitGemConnectService()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetStageMode(GPCellModeEnum cellmodeenum)
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            StageMode = cellmodeenum;
            return ret;
        }
        public (GPCellModeEnum, StreamingModeEnum) GetStageMode()
        {
            return LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetStageMode();
        }
        public EventCodeEnum NotifySystemErrorToConnectedCells(EnumLoaderEmergency emgtype)
        {
            return EventCodeEnum.NONE;
        }

        public string[] LoadEventLog(string lFileName)
        {
            return null;
        }

        public List<List<string>> UpdateLogFile()
        {
            return null;
        }

        public byte[] OpenLogFile(string selectedFilePath)
        {
            return null;
        }

        public string GetLotErrorMessage()
        {
            return null;
        }
        public EventCodeEnum HandlerVacOnOff(bool val, int stageindex)
        {
            return LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stageindex).HandlerVacOnOff(val);
        }

        public bool CheckUsingHandler(int stageindex)
        {
            return LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stageindex).CheckUsingHandler();
        }

        public EventCodeEnum CheckWaferStatus(bool isExist)
        {
            return EventCodeEnum.NONE;
        }

        public void StopBeforeProbingCmd(bool stopBeforeProbing)
        {
            throw new NotImplementedException();
        }

        public void StopAfterProbingCmd(bool stopAfterProbing)
        {
            throw new NotImplementedException();
        }

        public void OnceStopBeforeProbingCmd(bool stopBeforeProbing)
        {
            throw new NotImplementedException();
        }

        public void OnceStopAfterProbingCmd(bool stopAfterProbing)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoPinPadMatch_FirstSequence()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum CheckManualZUpState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public EventCodeEnum DO_ManualZUP()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DO_ManualZDown()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoManualSoaking()
        {
            throw new NotImplementedException();
        }

        public void LoadLUT()
        {
            throw new NotImplementedException();
        }
        public IStageSlotInformation GetSlotInfo()
        {
            return null;
        }
        public EventCodeEnum SaveSlotInfo()
        {
            return EventCodeEnum.NONE;
        }

        public int GetWaferObjHashCode()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum SetStageLock(ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetStageUnlock(ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public StageLockMode GetStageLockMode()
        {
            return StageLockMode.UNLOCK;
        }

        public void ChangeLotMode(LotModeEnum mode)
        {
            throw new NotImplementedException();
        }
        public void SetManualTeachPinMode(bool ManualTeachPinMode)
        {
            throw new NotImplementedException();
        }
        public bool IsForcedDoneMode()
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
        public bool IsMovingState()
        {
            bool retVal = false;
            try
            {
                retVal = this.SequenceEngineManager().isMovingState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public (DispFlipEnum disphorflip, DispFlipEnum dispverflip) GetDisplayFlipInfo()
        {
            return (DispFlipEnum.NONE, DispFlipEnum.NONE);
        }

        public (bool reverseX, bool reverseY) GetReverseMoveInfo()
        {
            return (false, false);
        }
        public void Set_TCW_Mode(bool isOn)
        {
           
        }
        public TCW_Mode Get_TCW_Mode()
        {
            return TCW_Mode.OFF;
        }
        public void SetLotModeByForcedLotMode()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveTouchSensorObject()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadTouchSensorObject()
        {
            return EventCodeEnum.NONE;
        }
        public void LoaderConnected()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum ClearWaferStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string GetWaitCancelDialogHashCode()
        {
            throw new NotImplementedException();
        }

        public void SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter)
        {
            return;
        }

        public EventCodeEnum DoManualPinAlign(bool CheckStageMode = true)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoManualWaferAlign(bool CheckStageMode = true)
        {
            throw new NotImplementedException();
        }

        public void UpdateWaferStatusAndState()
        {
            throw new NotImplementedException();
        }

        public void BindDataGatewayService(string uri)
        {
            throw new NotImplementedException();
        }
    }
}

