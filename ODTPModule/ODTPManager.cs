namespace ODTPModule
{
    using ProberInterfaces;
    using ProberInterfaces.ODTP;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces.ODTP.ODTPFormat;
    using ProberInterfaces.Temperature;
    using ResultMapParamObject;
    using ResultMapParamObject.STIF;
    using ProberInterfaces.Event;
    using NotifyEventModule;
    using System.IO;
    using System.Xml.Serialization;
    using LoaderController.GPController;
    using StageModule;

    public class ODTPManager : IODTPManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged Function
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region ==> Properties & Fields
        public bool Initialized { get; set; } = false;

        public ODTPHeader ODTPHeader { get; set; }
        public List<ODTPPCContact> ODTPPCContact { get; set; }

        private int ContactNumber;
        public string LocalUploadPath { get => @"C:\ProberSystem\ResultMap\ODTP\Upload\"; }

        ILotInfo lotInfo = null;
        IProbingModule probingModule = null;
        IPhysicalInfo waferPhysicalInfo = null;
        ISubstrateInfo waferSubstracteInfo = null;
        ITempController tempController = null;
        IProbingSequenceModule probingSequenceModule = null;
        ICoordinateManager CoordinateManager = null;
        IProbeCardDevObject probeCardDevObject = null;
        IProbingSequenceModule ProbingSeqModule = null;
        #endregion

        #region ==> Init & DeInit Function

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


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retval = this.EventManager().RegisterEvent(typeof(CardChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(StageMachineInitCompletedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LoaderConnectedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    //this.EventManager().RegisterEvent(typeof(CardDockEvent).FullName, "ProbeEventSubscibers", EventFired);
                    //this.EventManager().RegisterEvent(typeof(DeviceChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(WaferLoadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(ManualProbingEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(ProbingZUpProcessEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);

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

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (ODTPEnable() == true)
                {
                    lotInfo = this.LotOPModule()?.LotInfo;
                    waferPhysicalInfo = this.GetParam_Wafer()?.GetPhysInfo();
                    waferPhysicalInfo = this.GetParam_Wafer()?.GetPhysInfo();
                    waferSubstracteInfo = this.GetParam_Wafer()?.GetSubsInfo();
                    tempController = this.TempController();
                    probingSequenceModule = this.ProbingSequenceModule();
                    CoordinateManager = this.CoordinateManager();
                    probeCardDevObject = this.GetParam_ProbeCard().ProbeCardDevObjectRef;
                    probingModule = this.ProbingModule();
                    ProbingSeqModule = this.ProbingSequenceModule();
                    if (sender is WaferLoadedEvent)
                    {
                        LoggerManager.Debug($"[ODTPPManager], EventFired(): WaferLoadedEvent.");
                        this.ODTPHeader = null;
                        this.ODTPPCContact = null;
                        ContactNumber = 0;
                    }
                    else if (sender is ManualProbingEvent)
                    {
                        if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            ContactNumber++;

                            ManualProbingEventArg arg = e.Parameter as ManualProbingEventArg;

                            if(arg != null)
                            {
                                LoggerManager.Debug($"[ODTPPManager], EventFired(): ManualProbingEvent. ContactNumber = {ContactNumber}, (X,Y) = {arg.MXYIndex.X} ,{arg.MXYIndex.Y}, od = {arg.OverDrive}");

                                var mi = new MachineIndex((long)arg.MXYIndex.X, (long)arg.MXYIndex.Y);
                                MakeODTPData(mi, (int)arg.OverDrive);
                            }
                            else
                            {
                                LoggerManager.Debug($"[ODTPPManager], EventFired(): ManualProbingEvent. arg is null.");
                            }
                        }
                    }
                    else if (sender is ProbingZUpProcessEvent)
                    {
                        ContactNumber++;
                        LoggerManager.Debug($"[ODTPPManager], EventFired(): ProbingZUpProcessEvent. ContactNumber = {ContactNumber}, (X,Y) = {probingModule.ProbingLastMIndex.XIndex}, {probingModule.ProbingLastMIndex.YIndex}");
                        MakeODTPData(probingModule.ProbingLastMIndex, (int)probingModule.OverDrive);
                    }
                    else if (sender is WaferUnloadedEvent)
                    {
                        LoggerManager.Debug($"[ODTPPManager], EventFired(): WaferUnloadedEvent.");
                        Upload();

                    }
                    else if (sender is CardChangedEvent)
                    {
                        if (this.ODTPHeader != null && ContactNumber > 0)
                        {
                            LoggerManager.Debug($"[ODTPPManager], EventFired(): CardChangedEvent.");
                            // Lot 중 Card Change 한 경우에 카드 이름이 odtp 에 있던 정보와 불일치 할 경우 업로드 하고 새로 만듦
                            if (this.ODTPHeader.PCName != this.CardChangeModule().GetProbeCardID())
                            {
                                Upload();
                                this.ODTPHeader = null;
                                this.ODTPPCContact = null;
                                ContactNumber = 0;
                            }
                            else //Lot 중 Card Change 한 경우에 카드 이름이 odtp 에 있던 정보와 동일하면 이어서 진행 
                            {
                                LoggerManager.Debug($"[ODTPPManager], EventFired(): The card name is the same. Therefore, it is created after the previous ODTP file.");
                            }
                        }
                    }
                    else if (sender is StageMachineInitCompletedEvent)
                    {
                        LoggerManager.Debug($"[ODTPPManager], EventFired(): StageMachineInitCompletedEvent.");
                        // Lot 중 문제가 생겨 프로그램을 재 시작했거나 Error 나 MachineInit 울 헀을 경우 파일 업로드를 위함 ( 로더랑 연결 되어 있을 경우 )
                        Upload();

                    }
                    else if (sender is LoaderConnectedEvent) 
                    {
                        LoggerManager.Debug($"[ODTPPManager], EventFired(): LoaderConnectedEvent.");
                        // 어떠한 문제로 인해 로더랑 연결이 안되어 있어 파일 업로드를 못한 경우에 ODTP 파일을 서버에 업로드 하기위함
                        Upload();
                    }
                    else
                    {

                    }
                    //if (sender is CardLoadingEvent || sender is CardDockEvent)
                    //else if (sender is LotStartEvent)
                    //else if (sender is LotResumeEvent)
                    //else if (sender is DeviceChangedEvent)
                    //else if (sender is WaferLoadedEvent)
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> Upload Function
        public EventCodeEnum Upload()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.ODTPHeader == null)
                {
                    if (!CheckbackupSlotInfo()) 
                    {
                        return retval;
                    }
                }
                
                DateTime closetime = Convert.ToDateTime(this.ODTPHeader.DateTime);
                string time = closetime.ToString("yyyyMMdd-HHmmss");
                
                string sourcePath = Path.Combine(LocalUploadPath, this.ODTPHeader.Reader + ".ODTP");
                string filename = this.ODTPHeader.Reader + "_" + time + ".ODTP";
                LoggerManager.Debug($"[ODTPPManager], Upload() : make path {sourcePath}.");

                if (File.Exists(sourcePath))
                {
                    var service = (this.LoaderController() as GP_LoaderController).GPLoaderService;
                    if (service != null) // Loader 와 연결아 안되어 있는경우 ( Loader 랑 연결 될때 다시 시도 함 ) 
                    {
                        string destPath = Path.Combine(LocalUploadPath, filename.ToLower());
                        File.Move(sourcePath, destPath);
                        LoggerManager.Debug($"[ODTPPManager], Upload() try.[{filename}]");
                        retval = service.ODTPUpload(this.LoaderController().GetChuckIndex(), filename);
                    }
                    else 
                    {
                        LoggerManager.Debug($"[ODTPPManager], Upload() GPLoaderService is null");
                    }
                }
                else if (sourcePath == null) 
                {
                    LoggerManager.Debug($"[ODTPPManager], Upload() : source path is null.");
                }
                else
                {
                    LoggerManager.Debug($"[ODTPPManager], Upload() : source file not exist.");
                }
            }
            catch (Exception err)

            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }
        public bool ODTPEnable()
        {
            bool retval = false;

            try
            {
                var resultdata = this.ResultMapManager()?.ResultMapManagerSysIParam as ResultMapManagerSysParameter;
                retval = resultdata.ODTPEnable.Value;
                LoggerManager.Debug($"[ODTPPManager], ODTPEnable() [{retval}]");
            }
            catch (Exception err)
            {
                retval = true;
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion
        #region ==> MakeODTP
        public EventCodeEnum MakeODTP()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string filename = this.ODTPHeader.Reader + ".ODTP";
                string destPath = Path.Combine(LocalUploadPath, filename);

                retval = SerializeODTP(destPath);
                LoggerManager.Debug($"[ODTPPManager], MakeODTP().");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum MakeODTPData(MachineIndex Mi, int overDrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[ODTPPManager], MakeODTPData(). ContactNumber{ContactNumber}");

                ODTPPCContact PCContact = new ODTPPCContact();
                retVal = GetODTPPCContactData(out PCContact, Mi, overDrive);
                if (retVal == EventCodeEnum.NONE)
                {
                    if (this.ODTPPCContact == null)
                    {
                        this.ODTPPCContact = new List<ODTPPCContact>();
                    }
                    this.ODTPPCContact.Add(PCContact);
                    LoggerManager.Debug($"[ODTPPManager], MakeODTPData(). ODTP PC Contact Data.");
                }
                else 
                {
                    ContactNumber--;
                    LoggerManager.Debug($"[ODTPPManager], MakeODTPData(). ODTP PC Contact Data Invaild.");
                }

                if (retVal == EventCodeEnum.NONE && ContactNumber > 0) 
                {
                    ODTPHeader Header = new ODTPHeader();
                    retVal = GetODTPHeaderData(out Header);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        if (this.ODTPHeader == null)
                        {
                            this.ODTPHeader = new ODTPHeader();
                        }
                        this.ODTPHeader = Header;
                    }
                    LoggerManager.Debug($"[ODTPPManager], MakeODTPData(). ODTP Header Data.");

                    retVal = MakeODTP();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        private EventCodeEnum GetODTPHeaderData(out ODTPHeader oDTPHeader)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            oDTPHeader = null;

            try
            {
                oDTPHeader = new ODTPHeader();


                ///////////////////////////////////////////////////////////////////////////////////////////////////

                #region DateTime
                oDTPHeader.DateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                #endregion

                #region WaferData - waferPhysicalInfo
                oDTPHeader.Reader = waferSubstracteInfo.WaferID.Value;

                oDTPHeader.XStep = waferSubstracteInfo.ActualDieSize.Width.Value;
                oDTPHeader.YStep = waferSubstracteInfo.ActualDieSize.Height.Value;
                #endregion

                #region WaferData - waferPhysicalInfo
                int flat = (int)waferPhysicalInfo.NotchAngle.Value;
                // oDTPHeader.Flat = (int)waferPhysicalInfo.NotchAngle.Value + (int)waferPhysicalInfo.NotchAngleOffset.Value;
                switch (flat)
                {
                    case 0:
                        oDTPHeader.Flat = 270;
                        break;
                    case 90:
                        oDTPHeader.Flat = 180;
                        break;
                    case 180:
                        oDTPHeader.Flat = 90;
                        break;
                    case 270:
                        oDTPHeader.Flat = 0;
                        break;
                    default:
                        break;
                }


                if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.LEFT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.UP))
                {
                    oDTPHeader.CoQuad = (int)AxisDirectionEnum.UpLeft;
                    oDTPHeader.PrQuad = (int)AxisDirectionEnum.UpLeft;
                }
                else if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.RIGHT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.UP))
                {
                    oDTPHeader.CoQuad = (int)AxisDirectionEnum.UpRight;
                    oDTPHeader.PrQuad = (int)AxisDirectionEnum.UpRight;
                }
                else if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.RIGHT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.DOWN))
                {
                    oDTPHeader.CoQuad = (int)AxisDirectionEnum.DownRight;
                    oDTPHeader.PrQuad = (int)AxisDirectionEnum.DownRight;
                }
                else if ((waferPhysicalInfo.MapDirX.Value == MapHorDirectionEnum.LEFT)
                    && (waferPhysicalInfo.MapDirY.Value == MapVertDirectionEnum.DOWN))
                {
                    oDTPHeader.CoQuad = (int)AxisDirectionEnum.DownLeft;
                    oDTPHeader.PrQuad = (int)AxisDirectionEnum.DownLeft;
                }
                else
                {
                    LoggerManager.Error($"[ResultMapManager], GetMapHeaderBase() : MapDirX = {waferPhysicalInfo.MapDirX.Value}, MapDirY = {waferPhysicalInfo.MapDirY.Value}, Undefined.");
                    oDTPHeader.CoQuad = (int)AxisDirectionEnum.UpRight;
                    oDTPHeader.PrQuad = (int)AxisDirectionEnum.UpRight;
                }
                #endregion

                #region BasicData
                oDTPHeader.PCName = this.CardChangeModule().GetProbeCardID();
                oDTPHeader.PCParallelism = probeCardDevObject.DutList.Count();
                oDTPHeader.SetupFile = lotInfo.DeviceName.Value;
                oDTPHeader.SetupTemp = (int)tempController.TempInfo.SetTemp.Value; //온도값 체크
                oDTPHeader.Prober = this.FileManager().FileManagerParam.ProberID.Value;
                oDTPHeader.ODUnit = "micron";
                oDTPHeader.TouchNum = probingSequenceModule.ProbingSequenceCount;
                #endregion

                #region XRef,YRef,XRefDie,YRefDie


                MachineIndex FirstDieMI = null;
                UserIndex FirstDieUI = null;

                FirstDieMI = new MachineIndex(0, waferPhysicalInfo.MapCountY.Value - 1);
                FirstDieUI = CoordinateManager.MachineIndexConvertToUserIndex(FirstDieMI);

                oDTPHeader.XStrp = (int)FirstDieUI.XIndex;
                oDTPHeader.YStrp = (int)FirstDieUI.YIndex;

                ResultMapConverterParameter convparam = this.ResultMapManager().GetResultMapConvIParam() as ResultMapConverterParameter;
                var refcalcMode = convparam.STIFParam.RefCalcMode.Value;

                if (refcalcMode == RefCalcEnum.MANUAL)
                {
                    oDTPHeader.XRef = convparam.STIFParam.XRefManualValue.Value;
                    oDTPHeader.YRef = convparam.STIFParam.YRefManualValue.Value;
                }
                else
                {
                    MachineIndex RefDieMI = CoordinateManager.UserIndexConvertToMachineIndex(new UserIndex(waferPhysicalInfo.RefU.XIndex.Value, waferPhysicalInfo.RefU.YIndex.Value));
                    var RefCenterPos = this.WaferAligner().MachineIndexConvertToDieCenter(RefDieMI.XIndex, RefDieMI.YIndex);
                    RefCenterPos.X.Value -= waferSubstracteInfo.WaferCenter.X.Value;
                    RefCenterPos.Y.Value -= waferSubstracteInfo.WaferCenter.Y.Value;

                    oDTPHeader.XRef = RefCenterPos.X.Value;
                    oDTPHeader.YRef = RefCenterPos.Y.Value * (-1);
                }

                oDTPHeader.XRefDie = (int)waferPhysicalInfo.RefU.XIndex.Value;
                oDTPHeader.YRefDie = (int)waferPhysicalInfo.RefU.YIndex.Value;
                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        private EventCodeEnum GetODTPPCContactData(out ODTPPCContact oDTPPCContact, MachineIndex MI, int overDrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            oDTPPCContact = null;
            ODTPPCContactDie Dies = null;
            try
            {

                oDTPPCContact = new ODTPPCContact();
                oDTPPCContact.Die = new List<ODTPPCContactDie>();
                var UnderDutDevs = GetUnderDutDices(MI);
                if (UnderDutDevs.Count() < 0) 
                {
                    return retVal;
                }
                
                for (int i = 0; i < UnderDutDevs.Count; i++)
                {
                    Dies = new ODTPPCContactDie();
                    Dies.DUTNumber = UnderDutDevs[i].DutNumber;
                    UserIndex UI = CoordinateManager.MachineIndexConvertToUserIndex(UnderDutDevs[i].DieIndexM);
                    Dies.XCoord = UI.XIndex;
                    Dies.YCoord = UI.YIndex;
                    
                    oDTPPCContact.Die.Add(Dies);
                }

                oDTPPCContact.DateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                oDTPPCContact.OD = overDrive;
                oDTPPCContact.ContactNumber = ContactNumber;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }
        #endregion
        #region ==> MakeODTP
        private EventCodeEnum SerializeODTP(string destPath, Type serializeObjType = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            ODTP oDTP = new ODTP();
            try
            {
                oDTP.Header = ODTPHeader;
                oDTP.PCContact = ODTPPCContact;

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
                    LoggerManager.Error($"[ODTPPManager], SerializeODTPP() : Path is empty.");
                }

                // const string DEFAULT_NAMESPACE = "urn: stm:xsd.ODTP.V6 - 0";
                const string DEFAULT_NAMESPACE = "urn:stm:xsd.ODTP.V6-0";
                // XmlSerializer serializer = new XmlSerializer(typeof(ODTP), DEFAULT_NAMESPACE);
                XmlSerializer serializer = new XmlSerializer(typeof(ODTP));
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", DEFAULT_NAMESPACE);
                using (TextWriter writer = new StreamWriter(destPath))
                {
                    serializer.Serialize(writer, oDTP, namespaces);
                }

                LoggerManager.Debug($"[ODTPPManager], SaveODTPPToLocal() : Success. Path = {destPath}");
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum DeserializeODTP(string sourcePath, out ODTP oDTP)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            oDTP = null;

            try
            {
                if (string.IsNullOrEmpty(sourcePath) == true)
                {
                    LoggerManager.Error($"[ODTPPManager], DeserializeODTP() : Path is empty.");
                    return retval;
                }
                else if (!File.Exists(sourcePath))
                {
                    LoggerManager.Error($"[ODTPPManager], DeserializeODTP() : source file not exist.");
                    return retval;
                }

                using (var reader = new StreamReader(sourcePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ODTP));
                    oDTP = (ODTP)serializer.Deserialize(reader);
                }

                if (oDTP != null)
                {
                    LoggerManager.Debug($"[ODTPPManager], DeserializeODTP() : Success. Path = {sourcePath}");
                    retval = EventCodeEnum.NONE;
                }
                else 
                {
                    LoggerManager.Debug($"[ODTPPManager], DeserializeODTP() : Fail. Path = {sourcePath}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        #endregion
        
        #region checkfunc()
        public bool CheckbackupSlotInfo()
        {
            bool result = false;
            try
            {
                var backupSlotInfo = (this.StageSupervisor() as StageSupervisor).SlotInformation;
                if (backupSlotInfo != null)
                {
                    if (backupSlotInfo.WaferStatus == EnumSubsStatus.EXIST &&
                        !string.IsNullOrEmpty(backupSlotInfo.WaferID))
                    {
                        if (backupSlotInfo.WaferType == EnumWaferType.STANDARD)
                        {
                            var sourcePath = Path.Combine(LocalUploadPath, backupSlotInfo.WaferID + ".ODTP");
                            ODTP oDTP = null;
                            var retval = DeserializeODTP(sourcePath, out oDTP);
                            if (retval == EventCodeEnum.NONE)
                            {
                                if (oDTP.Header.Reader == backupSlotInfo.WaferID)
                                {
                                    result = true;
                                    LoggerManager.Debug($"[ODTPPManager], CheckbackupSlotInfo() : Slot Info Wafer ID {backupSlotInfo.WaferID}.");
                                    this.ODTPHeader = oDTP.Header;
                                    this.ODTPPCContact = oDTP.PCContact;
                                }
                                else
                                {
                                    result = false;
                                    LoggerManager.Debug($"[ODTPPManager], CheckbackupSlotInfo() : Slot Info Wafer ID ({backupSlotInfo.WaferID}) , ODTP File Wafer ID {oDTP.Header.Reader}.");
                                }
                            }
                            else
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            result = false;
                            LoggerManager.Debug($"[ODTPPManager], CheckbackupSlotInfo() : Invaild (WaferType : [{backupSlotInfo.WaferType}]).");
                        }
                    }
                    else
                    {
                        result = false;
                        LoggerManager.Debug($"[ODTPPManager], CheckbackupSlotInfo() : Invaild (WaferStatus : [{backupSlotInfo.WaferStatus}] , WaferID : [{backupSlotInfo.WaferID}]).");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
        #endregion
        #region 삭제 해야 함 GetUnderDutDices()
        public List<IDeviceObject> GetUnderDutDices(MachineIndex mCoord)
        {

            List<IDeviceObject> dev = new List<IDeviceObject>();

            var cardinfo = this.GetParam_ProbeCard();
            var Wafer = this.GetParam_Wafer();
            try
            {
                // ODTP 에서는 Mark die / test die 전부 Dut 에 닿았음을 기록해야 하고 Wafer map 영역에서 벗어난 경우에는 기록하면 안된다. ->  추후 옵션이 있어야 할것 같다.
                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                    {
                        IndexCoord retindex = mCoord.Add(cardinfo.GetRefOffset(dutIndex));
                        IDeviceObject devobj = Wafer.GetDevices().Find(x => x.DieIndexM.Equals(retindex));
                        if (devobj != null)
                        {
                            if (devobj.DieType.Value == DieTypeEnum.MARK_DIE)
                            {
                                dev.Add(devobj);
                                dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                            }
                            else if (devobj.DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                dev.Add(devobj);
                                dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                            }
                            else
                            {
                                // 있나?
                            }
                        }
                        else
                        {
                            // Out of wafer map area
                            //devobj = new DeviceObject();
                            //devobj.DieIndexM.XIndex = retindex.XIndex;
                            //devobj.DieIndexM.YIndex = retindex.YIndex;
                            //dev.Add(devobj);
                            //dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                        }
                    }

                    if (dev.Count() > 0)
                    {
                        List<IDeviceObject> dutdevs = new List<IDeviceObject>();
                        for (int devIndex = 0; devIndex < dev.Count; devIndex++)
                        {
                            if (dev[devIndex] != null)
                                dutdevs.Add(dev[devIndex]);
                        }
                        dev = dutdevs;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return dev;
        }
        #endregion
    }
}
