using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceProxy
{
    using LogModule;
    using MarkObjects;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Device;
    using SerializerUtil;
    using SubstrateObjects;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StageSupervisorProxy : ClientBase<IStageSupervisor>, IStageSupervisorProxy, IFactoryModule
    {
        public StageSupervisorProxy(int port, string ip = null, bool bOnlyCheck = false) :
               base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IStageSupervisor)),
                   new NetTcpBinding()
                   {
                       ReceiveTimeout = TimeSpan.MaxValue,
                       SendTimeout = bOnlyCheck ? TimeSpan.FromSeconds(5) : TimeSpan.FromMinutes(1),
                       MaxBufferPoolSize = 2147483647,
                       MaxBufferSize = 2147483647,
                       MaxReceivedMessageSize = 2147483647,
                       Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                       ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                   }, new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.StageSupervisorService}")))

        {
            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }

        private object chnLockObj = new object();

        public bool IsServiceAvailable()
        {
            bool retVal = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                        retVal = Channel.IsServiceAvailable();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"StageSupervisorProxy IsServiceAvailable timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
                else
                {
                    LoggerManager.Error($"StageSupervisor Service service error.");
                    retVal = false;
                }
            }
            return retVal;
        }

        public CellInitModeEnum GetStageInitState()
        {
            CellInitModeEnum e = CellInitModeEnum.BeforeInit;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        e = Channel.GetStageInitState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return e;
        }

        public IWaferObject GetWaferObject(IDeviceObject[,] preDies = null)
        {
            try
            {
                object target = null;

                if (IsOpened())
                {
                    byte[] bytes;

                    lock (chnLockObj)
                    {
                        bytes = Channel.GetWaferObject();

                    }

                    var uncompBuff = bytes;

                    if (uncompBuff != null)
                    {
                        var result = SerializeManager.DeserializeFromByte(uncompBuff, out target, typeof(WaferObject));

                        if (preDies != null)
                        {
                            ((WaferObject)target).GetSubsInfo().DIEs = preDies;
                        }
                    }

                    (((WaferObject)target).DispHorFlip, ((WaferObject)target).DispVerFlip) = Channel.GetDisplayFlipInfo();

                    bytes = null;
                }
                return (WaferObject)target;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }

        /// <summary>
        /// Stage에서 PrboeCard 데이터를 가져오는 함수
        /// </summary>
        /// <returns></returns>
        public byte[] GetProbeCardObject()
        {
            byte[] bytes = null;

            try
            {

                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        bytes = Channel.GetProbeCardObject();

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bytes;
        }

        public void SetProbeCardObject(IProbeCard param)
        {

        }

        public IProbeCard GetProbeCardConcreteObject()
        {
            try
            {
                object target = null;

                if (IsOpened())
                {
                    byte[] bytes = GetProbeCardObject();

                    var uncompBuff = bytes;
                    if (uncompBuff != null)
                    {
                        var result = SerializeManager.DeserializeFromByte(uncompBuff, out target, typeof(ProbeCard));
                    }
                }
                return (ProbeCard)target;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }

        public byte[] GetMarkObject()
        {
            byte[] bytes = null;

            try
            {

                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        bytes = Channel.GetMarkObject();

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bytes;
        }

        public Element<AlignStateEnum> GetAlignState(AlignTypeEnum AlignType)
        {
            Element<AlignStateEnum> retval = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetAlignState(AlignType);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public IMarkObject GetMarkConcreteObject()
        {
            try
            {
                object target = null;

                if (IsOpened())
                {
                    byte[] bytes = GetMarkObject();

                    var uncompBuff = bytes;
                    if (uncompBuff != null)
                    {
                        var result = SerializeManager.DeserializeFromByte(uncompBuff, out target, typeof(MarkObject));
                    }
                }
                return (MarkObject)target;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }
        public IDeviceObject[,] GetDies(List<DeviceObject> devicelist)
        {
            long maxX, minX, maxY, minY;
            long xNum = 0, yNum = 0;

            IDeviceObject[,] retval = null;

            try
            {
                ConcurrentBag<DeviceObject> devices = new ConcurrentBag<DeviceObject>(devicelist);

                if (devices.Count != 0)
                {
                    maxX = devices.Max(d => d.DieIndexM.XIndex);
                    minX = devices.Min(d => d.DieIndexM.XIndex);
                    maxY = devices.Max(d => d.DieIndexM.YIndex);
                    minY = devices.Min(d => d.DieIndexM.YIndex);

                    xNum = maxX - minX + 1;
                    yNum = maxY - minY + 1;

                    retval = new IDeviceObject[xNum, yNum];

                    Parallel.ForEach(devices, device =>
                    {
                        var exist = devices.TryTake(out var d);

                        if (exist)
                        {
                            retval[d.DieIndexM.XIndex, d.DieIndexM.YIndex] = d;
                        }
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<IDeviceObject[,]> GetConcreteDIEs()
        {
            IDeviceObject[,] retval = null;

            try
            {
                if (IsOpened())
                {
                    Stopwatch stw = new Stopwatch();
                    stw.Start();

                    var devices = GetDevices();

                    retval = GetDies(devices);

                    stw.Stop();
                    LoggerManager.Debug($"[{this.GetType().Name}] GetConcreteDIEs() : {stw.ElapsedMilliseconds} ms");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<IDeviceObject[,]>(retval);
        }


        public WaferObjectInfoNonSerialized GetWaferObjectInfoNonSerialize()
        {
            WaferObjectInfoNonSerialized retVal = null;
            try
            {
                if (IsOpened())
                {
                    retVal = Channel.GetWaferObjectInfoNonSerialize();
                }
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
                if (IsOpened())
                {
                    Channel.WaferIndexUpdated(xindex, yindex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void StageLotPause()
        {
            if (IsOpened())
            {
                lock (chnLockObj)
                {
                    Channel.LotPause();
                }
            }
        }
        public List<DeviceObject> GetDevices()
        {
            List<DeviceObject> devs = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Stopwatch stw = new Stopwatch();
                        stw.Start();
                        devs = Channel.GetDevices();
                        stw.Stop();
                        LoggerManager.Debug($"[{this.GetType().Name}] (WCF Only) GetDevices() : {stw.ElapsedMilliseconds} ms");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return devs;
        }

        public void CheckService()
        {
            if (IsOpened())
            {
                lock (chnLockObj)
                {
                    Channel.InitStageService(-1);
                }
            }
        }

        public void InitService()
        {
            InitService(-1);
        }
        public void DeInitService()
        {
            //Dispose
        }
        public void InitService(int stageAbsIndex = 0)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        LoggerManager.Debug($"StageSupervisorProxy State is [{this.State}]");
                        Channel.InitStageService(stageAbsIndex);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task InitLoaderClient()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.InitLoaderClient();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetDynamicMode(DynamicModeEnum dynamicModeEnum)
        {
            try
            {
                if (IsOpened())
                {
                    Channel.SetDynamicMode(dynamicModeEnum);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetLog(string date)
        {
            byte[] log = new byte[0];
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        log = Channel.GetLog(date);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                return null;
            }
            return log;
        }

        public byte[] GetLogFromFilename(List<string> debug, List<string> temp, List<string> pin, List<string> pmi, List<string> lot)
        {
            byte[] log = new byte[0];
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        // 1.4GB 1분 소요
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                            log = Channel.GetLogFromFilename(debug, temp, pin, pmi, lot);
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"StageSupervisorProxy GetLogFromFilename timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }

            return log;
        }

        public byte[] GetPinImageFromStage(List<string> pinImage)
        {
            byte[] log = new byte[0];
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 10, 0);
                            log = Channel.GetPinImageFromStage(pinImage);
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"StageSupervisorProxy GetPinImageFromStage timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }

            return log;
        }

        public byte[] GetLogFromFileName(EnumUploadLogType logtype, List<string> data)
        {
            byte[] log = new byte[0];
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        log = Channel.GetLogFromFileName(logtype, data);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return log;
        }
        public byte[] GetRMdataFromFileName(string filename)
        {
            byte[] log = new byte[0];
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        log = Channel.GetRMdataFromFileName(filename);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return log;
        }

        public byte[] GetODTPdataFromFileName(string filename)
        {
            byte[] log = new byte[0];
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        log = Channel.GetODTPdataFromFileName(filename);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return log;
        }

        public List<string> GetStageDebugDates()
        {
            List<string> dates = new List<string>();
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        dates = Channel.GetStageDebugDates();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return dates;
        }

        public List<string> GetStageTempDates()
        {
            List<string> dates = new List<string>();
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        dates = Channel.GetStageTempDates();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return dates;
        }
        public List<string> GetStagePinDates()
        {
            List<string> dates = new List<string>();
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        dates = Channel.GetStagePinDates();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return dates;
        }
        public List<string> GetStagePMIDates()
        {
            List<string> dates = new List<string>();
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        dates = Channel.GetStagePMIDates();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return dates;
        }

        public List<string> GetStageLotDates()
        {
            List<string> dates = new List<string>();
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        dates = Channel.GetStageLotDates();
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return null;
            }
            return dates;
        }
        public byte[] GetDevice()
        {
            byte[] device = new byte[0];
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        device = Channel.GetDevice();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return device;
        }
        public EnumSubsStatus GetWaferStatus()
        {
            EnumSubsStatus status = EnumSubsStatus.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        status = Channel.GetWaferStatus();
                    }
                }

            }
            catch (CommunicationException err)
            {
                LoggerManager.Error($"CommunicationException occurred. Err = {err.Message}");
            }
            catch (TimeoutException err)
            {
                LoggerManager.Error($"TimeoutException occurred. Err = {err.Message}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error occurred. Err = {err.Message}");
            }
            return status;

        }

        public EnumWaferType GetWaferType()
        {
            EnumWaferType wafertype = EnumWaferType.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        wafertype = Channel.GetWaferType();
                    }
                }
            }
            catch (CommunicationException err)
            {
                LoggerManager.Error($"CommunicationException occurred. Err = {err.Message}");
            }
            catch (TimeoutException err)
            {
                LoggerManager.Error($"TimeoutException occurred. Err = {err.Message}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error occurred. Err = {err.Message}");
            }

            return wafertype;
        }

        public IStageSupervisor GetChannel()
        {
            return (IStageSupervisor)this.Channel;
        }

        public void SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1, bool showprogress = true, bool manualDownload = false)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetDevice(device, devicename, lotid, lotCstHashCode, loaddev, foupnumber, showprogress, manualDownload);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetNeedChangeParaemterInDeviceInfo(needChangeParameter);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void BindDispService(string uri)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.BindDispService(uri);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void BindEventEelegateService(string uri)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.BindDelegateEventService(uri);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void BindDataGatewayService(string uri)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.BindDataGatewayService(uri);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void DoWaferAlign()
        //{
        //    if (IsOpened())
        //        Channel.DoWaferAlign();
        //}



        public void DoLot()
        {
            if (IsOpened())
                Channel.DoLot();
        }

        public async Task DoSystemInit(bool showMessageDialogFlag = true)
        {
            try
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.DoSystemInit(showMessageDialogFlag);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"StageSupervisorProxy DoSystemInit timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task DoWaferAlign()
        {
            try
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.DoWaferAlign();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"StageSupervisorProxy DoWaferAlign timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ChangeDeviceFuncUsingName(string devName)
        {
            if (IsOpened())
            {
                try
                {
                    await Channel.ChangeDeviceFuncUsingName(devName);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        public void SetAcceptUpdateDisp(bool flag)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetAcceptUpdateDisp(flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetWaitCancelDialogHashCode(string hashCode)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetWaitCancelDialogHashCode(hashCode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool InitServiceHost()
        {
            if (IsOpened())
                return Channel.StageCommunicationManager().InitServiceHosts();
            return false;
        }

        public string GetDeviceName()
        {
            string retVal = null;
            try
            {
                if (IsOpened())
                    retVal = Channel.GetDeviceName();
                else
                    retVal = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetEMG(EventCodeEnum errorCode)
        {
            if (IsOpened())
                Channel.SetEMG(errorCode);
        }

        public NeedleCleanObject GetNCObject()
        {
            NeedleCleanObject ncobj = null;
            try
            {
                if (IsOpened())
                {
                    object target = null;
                    SerializeManager.DeserializeFromByte(Channel.GetNCObject(), out target, typeof(NeedleCleanObject));
                    if (target != null)
                        ncobj = (NeedleCleanObject)target;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ncobj;
        }
        public EventCodeEnum HandlerVacOnOff(bool val, int stageindex = -1)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    eventCodeEnum = Channel.HandlerVacOnOff(val);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }

        public bool CheckUsingHandler(int stageindex = -1)
        {
            bool checkUsingHandler = false;
            try
            {
                if (IsOpened())
                {
                    checkUsingHandler = Channel.CheckUsingHandler();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return checkUsingHandler;
        }

        public void SetStageClickMoveTarget(double xpos, double ypos)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetMoveTargetPos(xpos, ypos);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task StageClickMove(object enableClickToMove)
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.ClickToMoveLButtonDown(enableClickToMove);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetWaferMapCam(EnumProberCam cam)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetWaferMapCam(cam);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsOpened()
        {
            bool retVal = false;
            try
            {
                if (State == CommunicationState.Opened || State == CommunicationState.Created)
                    retVal = true;
                else
                    retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public CommunicationState GetCommunicationState()
        {
            return this.State;
        }
        public void SetVacuum(bool ison)
        {
            try
            {
                Channel.SetVacuum(ison);
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
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.MoveStageToTargetPos(enableClickToMove);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum WaferHighViewIndexCoordMove(long mix, long miy)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.WaferHighViewIndexCoordMove(mix, miy);
                    }
                }
                retVal = EventCodeEnum.STAGEMOVE_WAFER_HIGH_VIEW_MOVE_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewIndexCoordMove(long mix, long miy)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.WaferLowViewIndexCoordMove(mix, miy);
                    }
                }
                retVal = EventCodeEnum.STAGEMOVE_WAFER_LOW_VIEW_MOVE_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public SubstrateInfoNonSerialized GetSubstrateInfoNonSerialized()
        {
            SubstrateInfoNonSerialized retval = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetSubstrateInfoNonSerialized();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public EventCodeEnum CheckPinPadParameterValidity()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.CheckPinPadParameterValidity();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum GetPinDataFromPads()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetPinDataFromPads();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PROBECARD_TYPE GetProbeCardType()
        {
            PROBECARD_TYPE retval = PROBECARD_TYPE.Cantilever_Standard;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetProbeCardType();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public int DutPadInfosCount()
        {
            int retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.DutPadInfosCount();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum InitGemConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.InitGemConnectService();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DeInitGemConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.DeInitGemConnectService();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum NotifySystemErrorToConnectedCells(EnumLoaderEmergency emgtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.NotifySystemErrorToConnectedCells(emgtype);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }

        public void SetErrorCodeAlarm(EventCodeEnum errorcode)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetErrorCodeAlarm(errorcode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string[] LoadStageEventLog(string fileFath)
        {

            string[] retVal = null;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.LoadEventLog(fileFath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public string GetLotErrorMessage()
        {
            string retVal = null;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.GetLotErrorMessage();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public (GPCellModeEnum, StreamingModeEnum) GetStageMode()
        {
            (GPCellModeEnum, StreamingModeEnum) retVal = (GPCellModeEnum.OFFLINE, StreamingModeEnum.UNDEFINED);
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.GetStageMode();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public (DispFlipEnum disphorflip, DispFlipEnum dispverflip) GetDisplayFlipInfo()
        {
            (DispFlipEnum disphorflip, DispFlipEnum dispverflip) ret = (DispFlipEnum.NONE, DispFlipEnum.NONE);
            try
            {

                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        ret = Channel.GetDisplayFlipInfo();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return ret;
            }
            return ret;
        }

        public (bool reverseX, bool reverseY) GetReverseMoveInfo()
        {
            (bool reverseX, bool reverseY) ret = (false, false);
            try
            {

                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        ret = Channel.GetReverseMoveInfo();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return ret;
            }
            return ret;
        }


        public void StopBeforeProbingCmd(bool stopBeforeProbing)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.StopBeforeProbingCmd(stopBeforeProbing);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StopAfterProbingCmd(bool stopAfterProbing)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.StopAfterProbingCmd(stopAfterProbing);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void OnceStopBeforeProbingCmd(bool stopBeforeProbing)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.OnceStopBeforeProbingCmd(stopBeforeProbing);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void OnceStopAfterProbingCmd(bool stopAfterProbing)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.OnceStopAfterProbingCmd(stopAfterProbing);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum CheckManualZUpState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.CheckManualZUpState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum DoPinPadMatch_FirstSequence()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.DoPinPadMatch_FirstSequence();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DO_ManualZUP()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.DO_ManualZUP();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DO_ManualZDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.DO_ManualZDown();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DO_ManualSoaking()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.DoManualSoaking();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum DO_ManualWaferAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 15, 0);
                            retVal = Channel.DoManualWaferAlign();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"StageSupervisorProxy DO_ManualWaferAlign timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
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
        public EventCodeEnum DO_ManualPinAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 15, 0);
                            retVal = Channel.DoManualPinAlign();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"StageSupervisorProxy DO_ManualPinAlign timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
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

        public int GetWaferObjHashCode()
        {
            int hashCode = -1;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        hashCode = Channel.GetWaferObjHashCode();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return hashCode;
        }

        public void ChangeLotMode(LotModeEnum mode)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.ChangeLotMode(mode);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetLotModeByForcedLotMode()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetLotModeByForcedLotMode();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsForcedDoneMode()
        {
            bool retVal = false;

            try
            {


                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.IsForcedDoneMode();

                    }
                }
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
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.IsMovingState();

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void LoaderConnected()
        {
            try
            {
                if (IsOpened())
                {
                    Channel.LoaderConnected();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}
