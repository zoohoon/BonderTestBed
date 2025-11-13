using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.InternalArgs;
using Autofac;

namespace Motion
{
    using Configurator;
    using ProberInterfaces;
    using System.Diagnostics;
    using System.Threading;
    using System.Collections;
    using ProberErrorCode;
    using SystemExceptions.MotionException;
    using LogModule;
    using System.Reflection;
    using System.Timers;

    ////using ProberInterfaces.ThreadSync;
    public class SlaveAxesInfo
    {
        private int _NodeCount;

        public int NodeCount
        {
            get { return _NodeCount; }
            set { _NodeCount = value; }
        }

        private ushort _AxisRef;

        public ushort AxisRef
        {
            get { return _AxisRef; }
            set { _AxisRef = value; }
        }


        public SlaveAxesInfo(int nodecnt, ushort axisref)
        {
            NodeCount = nodecnt;
            AxisRef = axisref;
        }
    }
    public class ElmoMotionBase : IMotionBase, IHasSysParameterizable, IFactoryModule
    {
        private static int MonitoringInterValInms = 4;
        private static int syncTimeOut_ms = 1000;
        private int previewSyncTimeOut = 100;
        AutoResetEvent ResetEvent = new AutoResetEvent(false);
        AutoResetEvent SDOResetEvent = new AutoResetEvent(false);
        //ManualResetEvent mreUpdateEvent = new ManualResetEvent(false);
        ManualResetEvent mreSyncEvent = new ManualResetEvent(false);
        //bool setState = false;

        System.Timers.Timer _monitoringTimer;
        System.Timers.Timer _sdoMonitoringTimer;
        bool bStopUpdateThread;
        Thread UpdateThread;
        Thread UpdateSDOThread;
        private string FileFolder;
        private string FileName;
        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        //private IParam _SysParam;
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //        }
        //    }
        //}

        private long _UpdateProcTime;

        public long UpdateProcTime
        {
            get { return _UpdateProcTime; }
            set { _UpdateProcTime = value; }
        }


        private List<AxisStatus> _AxisStatusList;
        public List<AxisStatus> AxisStatusList
        {
            get
            {
                return _AxisStatusList;
            }
        }

        public int PortNo => throw new NotImplementedException();


        public bool IsMotionReady => throw new NotImplementedException();
        public const int VALID_STAND_STILL_MASK = 0x40000080;
        //Elmo basic functions
        List<String> elmoNodeName = new List<string>();
        List<String> elmoNodeConfig1 = new List<string>();
        List<String> elmoNodeConfig2 = new List<string>();


        private int ConnHndl;

        MMCComStatisticsEx NetworkCommStat = new MMCComStatisticsEx();

        //MMCSingleAxis[] m_Singleaxis;

        private List<MMCAxis> _MMCAxes;

        public List<MMCAxis> MMCAxes
        {
            get { return _MMCAxes; }
            set {
                _MMCAxes = value;
            }
        }
        ECATAxisDescripter EcatAxisDesc;

        //IPMASManager _PMASManager;

        MMCBulkRead m_BulkRead;
        //NC_BULKREAD_PRESET_5[] stReadBulkReadData;
        NC_BULKREAD_PRESET_4[] stReadBulkReadData;
        //private LockKey bulkReadBufferLock = new LockKey("Elmo - Bulk Read buffer");

        class ELMO_Global_Constants
        {
            public const int MAX_AXIS = 8;
            public const int MAX_IONode = 50;
            public const int MAX_GROUP = 0;

            public const int SERVO_ON = 1;
            public const int SERVO_OFF = 0;

            public const int DIGITAL_INPUT_IDX = 0;
            public const int DIGITAL_OUTPUT_IDX = 0;

            public const int DIGITAL_INPUT_NUM = 6;

            public const int DIGITAL_INPUT_1 = 16;
            public const int DIGITAL_INPUT_2 = 17;
            public const int DIGITAL_INPUT_3 = 18;
            public const int DIGITAL_INPUT_4 = 19;
            public const int DIGITAL_INPUT_5 = 20;
            public const int DIGITAL_INPUT_6 = 21;

            public const int DIGITAL_OUTPUT_NUM = 4;

            public const int DIGITAL_OUTPUT_1 = 16;
            public const int DIGITAL_OUTPUT_2 = 17;
            public const int DIGITAL_OUTPUT_3 = 18;
            public const int DIGITAL_OUTPUT_4 = 19;

            public const int TRD_DIGITAL_OUT_NUM = 8;

            public const int TRD_DIGITAL_OUTPUT_1 = 0;
            public const int TRD_DIGITAL_OUTPUT_2 = 1;
            public const int TRD_DIGITAL_OUTPUT_3 = 2;
            public const int TRD_DIGITAL_OUTPUT_4 = 3;
            public const int TRD_DIGITAL_OUTPUT_5 = 4;
            public const int TRD_DIGITAL_OUTPUT_6 = 5;
            public const int TRD_DIGITAL_OUTPUT_7 = 6;
            public const int TRD_DIGITAL_OUTPUT_8 = 7;

            public const int TRD_DIGITAL_IN_NUM = 8;

            public const int TRD_DIGITAL_INPUT_1 = 0;
            public const int TRD_DIGITAL_INPUT_2 = 1;
            public const int TRD_DIGITAL_INPUT_3 = 2;
            public const int TRD_DIGITAL_INPUT_4 = 3;
            public const int TRD_DIGITAL_INPUT_5 = 4;
            public const int TRD_DIGITAL_INPUT_6 = 5;
            public const int TRD_DIGITAL_INPUT_7 = 6;
            public const int TRD_DIGITAL_INPUT_8 = 7;

            //public const string MDS_PROGRAM = "ProtocolConverter.pexe";
            //public const string MDS_PROGRAM = "ProtocolConverter.gexe";

            public const int STATUS_MONITOR_TIME = 100;

            public const int VALID_STAND_STILL_MASK = 0x40000080;
            public const int VALID_DISABLE_MASK = 0x40000200;
            public const int VALID_MOTION_MASK = 0x40000024;
            public const int VALID_ERROR_MASK = 0x60000480;

            public const uint VALID_AXIS_DONE_MASK = 0x80000000;


            public const int VALID_GROUP_STANDBY_MASK = 0x40020000;
            public const int VALID_GROUP_DISABLE_MASK = 0x40010000;

            public const int COMMUNICATION_TCP_PORT = 4000;
            public const int COMMUNICATION_UDP_PORT = 5000;

            public const int CALLBACK_EVENT_MASK = 0x0FFFFFFF;

            public const int DS402_HOMING_METHOD_NHID = 5;
            public const int DS402_HOMING_METHOD_PHID = 3;
            public const int DS402_HOMING_METHOD_SETPOS = 35;

            public const double XY_DTOP = 51.2;
            public const double Z_DTOP = 77.19657032;
            public const double C_DTOP = 2.268928028;

        }

        //global variables for Elmo
        public class ElmoParams
        {
            public ElmoParams()
            {

            }

            public int AxisOrder { get; set; }
            //public ushort DriverRefID = 0;

            public double _velocity { get; set; }
            public double _accel { get; set; }
            public double _Jerk { get; set; }
            public double _TPOS { get; set; }
        }

        public ElmoMotionBase()
        {

        }
        //public ElmoMotionBase(int CtrlNum, String filePath, IPMASManager pmasManager, Autofac.IContainer container)
        public ElmoMotionBase(int CtrlNum)
        {
            try
            {
                ConnHndl = CtrlNum;


                LoadSysParameter();

                InitMotionProvider(CtrlNum.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        public EventCodeEnum LoadECATAxisDescripterParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            EcatAxisDesc = new ECATAxisDescripter();
            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(ECATAxisDescripter.ECATAxisDescripterParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    EcatAxisDesc.ECATAxisDescripterParams = tmpParam as ECATAxisDescripter.ECATAxisDescripterParam;

                    if(EcatAxisDesc.ECATAxisDescripterParams.GroupAxisDeviation<100)
                    {
                        EcatAxisDesc.ECATAxisDescripterParams.GroupAxisDeviation = 2000;
                    }
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[ElmoMotionBase] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = LoadECATAxisDescripterParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            return RetVal;
        }


        private List<int> slaveAxes = new List<int>();
        private List<SlaveAxesInfo> slaveAxesInfo = new List<SlaveAxesInfo>();
        public int InitMotionProvider(string ctrlNo)
        {
            int nRetVal = 0;

            try
            {
                //MmcCommstatistics NetworkCommStat = new MmcCommstatistics();

                //MMCNetwork NetworkInfo = new MMCNetwork(ctrlNo);

                //NetworkInfo.GetNetworkInfo(ref NetworkCommStat);


                MMCAxes = new List<MMCAxis>();
                stReadBulkReadData = new NC_BULKREAD_PRESET_4[EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions.Count];
                int cnt = 0;
                _AxisStatusList = new List<AxisStatus>();
                ushort[] nNodeList = new ushort[EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions.Count];
                foreach (ECATNodeDefinition eCatAxis in EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions)
                {
                    MMCAxis axis = null;
                    if (eCatAxis.GroupType == EnumGroupType.GROUP)
                    {
                        axis = new MMCGroupAxis(eCatAxis.ID, this.ConnHndl);
                        var masteraxis = EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions.Where(item => item.GroupName == eCatAxis.ID && item.GroupType == EnumGroupType.MASTER).FirstOrDefault();
                        var master = new MMCSingleAxis(masteraxis.ID, this.ConnHndl);
                        nNodeList[cnt] = master.AxisReference;
                        stReadBulkReadData[cnt++] = new NC_BULKREAD_PRESET_4();
                    }
                    else
                    {
                        axis = new MMCSingleAxis(eCatAxis.ID, this.ConnHndl);
                        nNodeList[cnt] = axis.AxisReference;

                        if (eCatAxis.GroupType == EnumGroupType.SLAVE
                            | eCatAxis.GroupType == EnumGroupType.MASTER)
                        {
                            SlaveAxesInfo slaveaxes = new SlaveAxesInfo(cnt, axis.AxisReference);
                            slaveAxesInfo.Add(slaveaxes);
                            //slaveAxes.Add(cnt);
                        }

                        stReadBulkReadData[cnt++] = new NC_BULKREAD_PRESET_4();
                    }

                    MMCAxes.Add(axis);

                    _AxisStatusList.Add(new AxisStatus());

                    if (eCatAxis.GroupType != EnumGroupType.UNDEFINED && eCatAxis.GroupName != "")
                    {
                        int nodenum = GetNodeNum(cnt - 1);
                        this.PMASManager().SendMessage(nodenum, "CA[79]", "0");
                        this.PMASManager().SendMessage(nodenum, "CA[79]", "1");
                    }
                    //nNodeList[cnt] = axis.AxisReference;
                    //stReadBulkReadData[cnt++] = new NC_BULKREAD_PRESET_5();
                }

                _monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                _monitoringTimer.Start();

                _sdoMonitoringTimer = new System.Timers.Timer(MonitoringInterValInms * 10);
                _sdoMonitoringTimer.Elapsed += _sdoMonitoringTimer_Elapsed;
                _sdoMonitoringTimer.Start();

                DevConnected = true;

                if (DevConnected)
                {
                    m_BulkRead = new MMCBulkRead(this.ConnHndl);

                    m_BulkRead.Init(
                        NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_4,
                        NC_BULKREAD_CONFIG_ENUM.eBULKREAD_CONFIG_1,
                        nNodeList, (ushort)nNodeList.Length);
                    m_BulkRead.Config();
                    bStopUpdateThread = false;
                    UpdateThread = new Thread(new ThreadStart(UpdateElmoBaseProc));
                    //UpdateThread.Priority = ThreadPriority.Highest;
                    UpdateThread.Start();

                    UpdateSDOThread = new Thread(new ThreadStart(UpdateSDOsProc));
                    UpdateSDOThread.Start();
                }
                FileFolder = @"C:\Logs";
                FileName = @"TriErrorCheck.txt";

                SetOPModeforAllAxis();
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);

                return (int)EnumMotionBaseReturnCode.InitError;
            }
            catch (System.Exception err)
            {
                LoggerManager.Exception(err);
            }

            return nRetVal;
        }

        private void _sdoMonitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            try
            {
                SDOResetEvent.Set();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void UpdateSDOsProc()
        {

            //int auxpos = 0;
            //int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            try
            {
                while (bStopUpdateThread == false)
                {
                    //Thread.Sleep(2);
                    if (DevConnected == true)
                    {
                        //for (int i = 0; i < AxisStatusList.Count; i++)
                        //{
                        //    if (MMCAxes[i] is MMCGroupAxis)
                        //    {

                        //    }
                        //    else
                        //    {
                        //        GetAuxPos(i, ref auxpos);
                        //    }
                        //}
                        for (int i = 0; i < slaveAxesInfo.Count; i++)
                        {
                            //GetAuxPos(slaveAxes[i], ref auxpos);
                            //AxisStatusList[slaveAxes[i]].AuxPosition = auxpos;
                            byte bEntry = 0;
                            double[] dValue = new double[1];
                            ReadGroupParametersArgs[] stInputParam = new ReadGroupParametersArgs[1];
                            stInputParam[0] = new ReadGroupParametersArgs();
                            stInputParam[0].iParameterIndex = 0;
                            stInputParam[0].uiParameterNumber = (uint)MMC_PARAMETER_LIST_ENUM.MMC_AUXILIARY_POSITION;
                            stInputParam[0].usAxisRef = slaveAxesInfo[i].AxisRef;
                            stInputParam[0].usPadding = 0;
                            //m_Singleaxis[0].ReadGroupParameters(stInputParam, (byte)1, ref dValue, ref bEntry);
                            ((MMCSingleAxis)MMCAxes[slaveAxesInfo[i].NodeCount]).ReadGroupParameters(stInputParam, (byte)1, ref dValue, ref bEntry);
                            SDOResetEvent.WaitOne(50);
                            AxisStatusList[slaveAxesInfo[i].NodeCount].AuxPosition = dValue.FirstOrDefault();
                        }
                    }
                    SDOResetEvent.WaitOne(300);
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"UpdateElmoBaseProc MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void _monitoringTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //_monitoringTimer.Stop();

            try
            {
                //if (setState == true)
                //{
                //    mreUpdateEvent.Reset();
                //    setState = false;
                //}
                //else
                //{
                //    mreUpdateEvent.Set();
                //    setState = true;
                //}
                ResetEvent.Set();
                //_monitoringTimer.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int SetBulkReadConfig()
        {
            int nRetVal = 0;

            ushort[] nNodeList = new ushort[MMCAxes.Count];     // + ConstantsList.MAX_GROUP];

            try
            {
                for (int i = 0; i < MMCAxes.Count; i++)
                {
                    nNodeList[i] = MMCAxes[i].AxisReference;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            // nNodeList[ConstantsList.MAX_AXIS] = m_GroupAxis.AxisReference;

            try
            {
                m_BulkRead = new MMCBulkRead(ConnHndl);

                m_BulkRead.Init(
                    NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_4,
                    NC_BULKREAD_CONFIG_ENUM.eBULKREAD_CONFIG_1,
                    nNodeList,
                    ELMO_Global_Constants.MAX_AXIS);

                m_BulkRead.Config();

                LoggerManager.Debug($"Elmo SetBulkReadConfig ok.");
            }
            catch (MMCException ex)
            {
                LoggerManager.Error($"SetBulkReadConfig() Function error: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                return (int)ex.MMCError;
            }

            return nRetVal;
        }

        public int DS402HomingProgress(MMCSingleAxis singleAxis, int nMethodNumber, double pos, float dvelpulse, float daccpulse, float ot, float lowspeed, float highspeed, double dlimitvel)
        {
            int nRetVal = -1;
            bool timeoutcheck = false;
            uint timeoutLimt = 300000; // 5min
            byte[] sparebyte = new byte[16];
            LoggerManager.Debug($"[Motion]DS402HomingProgress start. Axis:{singleAxis.AxisName}");
            Stopwatch stw = new Stopwatch();
            stw.Start();
            try
            {

                SetOperationMode(singleAxis, OPM402.OPM402_HOMING_MODE);
                if (nMethodNumber == 35 || nMethodNumber == 33 || nMethodNumber == 34)
                {
                    var param = new MMC_HOMEDS402Params();
                    param.uiHomingMethod = nMethodNumber;
                    param.dbPosition = pos;
                    param.uiTimeLimit = (uint)Math.Max(1000, timeoutLimt-1000);
                    param.fDistanceLimit = (float)dlimitvel;
                    param.fVelocity = (float)dvelpulse;
                    param.fAcceleration = (float)daccpulse;
                    param.eBufferMode = MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE;
                    singleAxis.HomeDS402(param);

                    while (singleAxis.ReadStatus() != VALID_STAND_STILL_MASK)
                    {
                        if (stw.ElapsedMilliseconds > timeoutLimt)
                        {
                            LoggerManager.Debug("[Motion]Time Out occured while wait for idle in DS402HomingProgress");
                            timeoutcheck = true;
                            break;
                        }
                    }
                }
                else
                {
                    // 251106 sebas add homing delay
                    if(singleAxis.AxisName == "g37")
                    {
                        Thread.Sleep(500);
                    }
                    else if(singleAxis.AxisName == "g33" || singleAxis.AxisName == "g34" || singleAxis.AxisName == "g35" || singleAxis.AxisName == "g36" || singleAxis.AxisName == "g38")
                    {
                        Thread.Sleep(2000);
                    }
                    else if(singleAxis.AxisName == "a16" || singleAxis.AxisName == "a17")
                    {
                        Thread.Sleep(1000);
                    }
                    singleAxis.HomeDS402Ex(pos, dlimitvel, daccpulse, highspeed, lowspeed, ot, 2000, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE,
                        nMethodNumber, timeoutLimt, timeoutLimt, 1, sparebyte);
                    while (singleAxis.ReadStatus() != VALID_STAND_STILL_MASK)
                    {
                        if (stw.ElapsedMilliseconds > timeoutLimt)
                        {
                            LoggerManager.Debug("[Motion]Time Out occured while wait for idle in in DS402HomingProgress");
                            timeoutcheck = true;
                            break;
                        }
                    }
                }
                if (timeoutcheck == true)
                {
                    nRetVal = (int)EnumMotionBaseReturnCode.HomingError;
                }
                else
                {
                    nRetVal = 0;
                }

                SetOperationMode(singleAxis, OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
            }
            catch (MMCException ex)
            {
                nRetVal = (int)EnumMotionBaseReturnCode.HomingError;
                LoggerManager.Debug($"DS402HomingProgress({singleAxis.AxisName}): MMCException occurred. Err. code = {ex.MMCError}");
                LoggerManager.Error($"DS402HomingProgress() Function error axis = {singleAxis.AxisName}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                //LoggerManager.DebugError($"DS402HomingProgress() Function error: {ex.Message}");
                SetOperationMode(singleAxis, OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
            }
            LoggerManager.Debug($"[Motion]DS402HomingProgress End. Axis:{singleAxis.AxisName} return:{nRetVal}");
            return nRetVal;
        }
        public int DS402HomingProgress(MMCSingleAxis singleAxis, int nMethodNumber, double pos, float dvelpulse, float daccpulse, float ot)
        {
            int nRetVal = -1;
            uint timeout = 1000;

            try
            {
                timeout = (uint)(ot / dvelpulse);
                MMC_HOMEDS402Params stHomingParam;
                stHomingParam.dbPosition = pos;     //7289273; //pos;
                stHomingParam.fVelocity = dvelpulse;
                stHomingParam.fAcceleration = daccpulse;
                stHomingParam.fDistanceLimit = ot;
                stHomingParam.fTorqueLimit = 2000;
                stHomingParam.uiHomingMethod = nMethodNumber;
                stHomingParam.uiTimeLimit = 60000;       //'' timeout;
                stHomingParam.eBufferMode = MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE;
                stHomingParam.ucExecute = 1;
                singleAxis.HomeDS402(stHomingParam);
                while (singleAxis.ReadStatus() != VALID_STAND_STILL_MASK) { }
                nRetVal = 0;
            }
            catch (MMCException err)
            {
                //LoggerManager.Error($"DS402HomingProgress() Function error: " + ex.Message);
                LoggerManager.Exception(err);

            }

            return nRetVal;
        }

        public int DS402HomingProgress(MMCSingleAxis singleAxis, int nMethodNumber, float dvelpulse, float daccpulse)
        {
            int nRetVal = -1;

            try
            {
                MMC_HOMEDS402Params stHomingParam;

                stHomingParam.dbPosition = 0;
                stHomingParam.fVelocity = dvelpulse;
                stHomingParam.fAcceleration = daccpulse;
                stHomingParam.fDistanceLimit = 100000;
                stHomingParam.fTorqueLimit = 2000;
                stHomingParam.uiHomingMethod = nMethodNumber;
                stHomingParam.uiTimeLimit = 1000;
                stHomingParam.eBufferMode = MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE;
                stHomingParam.ucExecute = 1;

                singleAxis.HomeDS402(stHomingParam);
                //while (singleAxis.ReadStatus() != ELMO_Global_Constants.VALID_STAND_STILL_MASK) { }
                nRetVal = 0;
            }
            catch (MMCException ex)
            {
                LoggerManager.Error($"DS402HomingProgress() Function error axis = {singleAxis.AxisName}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");

                //LoggerManager.DebugError($"DS402HomingProgress() Function error: " + ex.Message);
            }

            return nRetVal;
        }

        public int SetOPModeforAllAxis()
        {
            int nRetVal = -1;
            //OPM402 tmpval;
            int invalidcnt = 0;

            try
            {

                for (int i = 0; i < MMCAxes.Count; i++)
                {
                    if (EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions[i].GroupType != EnumGroupType.GROUP)
                    {
                        // 20251030 LJH Elmo 메뉴얼 조그 임시 회피 코드
                        //AlarmReset((MMCSingleAxis)MMCAxes[i]);
                        //SetOperationMode((MMCSingleAxis)MMCAxes[i], OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                        // end

                        //tmpval = ((MMCSingleAxis)MMCAxes[i]).GetOpMode();
                        //if (tmpval != OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE)
                        //{
                        //    ((MMCSingleAxis)MMCAxes[i]).SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);

                        //}
                        //else
                        //{
                        //    invalidcnt += 1;
                        //}
                    }
                    else
                    {
                        // Grouptype
                    }
                }


                //for (int i = 0; i < MMCAxes.Count; i++)
                //{
                //    tmpval = ((MMCSingleAxis)MMCAxes[i]).GetOpMode();
                //    if (tmpval != OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE)
                //    {
                //        ((MMCSingleAxis)MMCAxes[i]).SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                //    }
                //    else
                //    {
                //        invalidcnt += 1;
                //    }


                //    //if (tmpval != OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE)
                //    //{
                //    //    MMCAxes[i].SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                //    //}
                //    //else
                //    //{
                //    //    invalidcnt += 1;
                //    //}

                //}

                if (invalidcnt != 0)
                {
                    nRetVal = (int)EnumMotionBaseReturnCode.WaitforFunctionError;
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);
                return (int)EnumMotionBaseReturnCode.WaitforFunctionError;
            }
            catch (System.Exception err)
            {
                LoggerManager.Exception(err);

                nRetVal = (int)EnumMotionBaseReturnCode.WaitforFunctionError;
            }

            return nRetVal;
        }

        public int ElmoSetPostion(AxisObject axis, double setposval)
        {
            int nRetVal = 0;
            double pulsecnt = 0.0;
            double velocity = 0;
            pulsecnt = (double)axis.DtoP(setposval);
            LoggerManager.Debug($"[Motion] ElmoSetPosition Start Axis:{axis.Label.Value} PosVal:{setposval}");
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return nRetVal;
                //    }
                lock (axis)
                {

                    if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                    {
                        DisableAxis(axis);
                        nRetVal = GetVelocity(axis, EnumTrjType.Homming, ref velocity);
                        ResultValidate(nRetVal);
                        for (int i = 0; i < axis.GroupMembers.Count; i++)
                        {
                            Thread.Sleep(500);
                            DS402HomingProgress(((MMCSingleAxis)MMCAxes[axis.GroupMembers[i].AxisIndex.Value]),
                                       35,
                                       pulsecnt * -1d,
                                       (float)axis.DtoP(velocity),
                                       (float)axis.DtoP(GetAccel(axis, EnumTrjType.Homming)),
                                       (float)axis.DtoP((Math.Abs(axis.Param.PosSWLimit.Value) + Math.Abs(axis.Param.NegSWLimit.Value))),
                                       (float)axis.DtoP(axis.Param.IndexSearchingSpeed.Value),
                                       (float)axis.DtoP(axis.Param.HommingSpeed.Value),
                                       (float)axis.DtoP(velocity));
                            AxisStatusList[axis.GroupMembers[i].AxisIndex.Value].Pulse.Command = pulsecnt;
                            axis.GroupMembers[i].Status.Pulse.Ref = pulsecnt;
                            axis.GroupMembers[i].Status.Pulse.Command = pulsecnt;
                            axis.GroupMembers[i].Status.Position.Ref = axis.PtoD(pulsecnt);
                        }
                        AxisStatusList[axis.AxisIndex.Value].Pulse.Command = pulsecnt;
                        axis.Status.Pulse.Ref = pulsecnt;
                        axis.Status.Pulse.Command = pulsecnt;
                        axis.Status.Position.Ref = axis.PtoD(pulsecnt);

                        nRetVal = EnableAxis(axis);
                        ResultValidate(nRetVal);
                    }
                    else
                    {
                        nRetVal = GetVelocity(axis, EnumTrjType.Homming, ref velocity);
                        ResultValidate(nRetVal);

                        nRetVal = DS402HomingProgress(((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]),
                                       35,
                                       pulsecnt * -1d,
                                       (float)axis.DtoP(velocity),
                                       (float)axis.DtoP(GetAccel(axis, EnumTrjType.Homming)),
                                       (float)axis.DtoP((Math.Abs(axis.Param.PosSWLimit.Value) + Math.Abs(axis.Param.NegSWLimit.Value))),
                                       (float)axis.DtoP(axis.Param.IndexSearchingSpeed.Value),
                                       (float)axis.DtoP(axis.Param.HommingSpeed.Value),
                                       (float)axis.DtoP(velocity));
                        ResultValidate(nRetVal);
                        AxisStatusList[axis.AxisIndex.Value].Pulse.Command = pulsecnt;
                        axis.Status.Pulse.Command = pulsecnt;
                    }

                    LoggerManager.Debug($"{axis.Label.Value} Axis Set position as {axis.PtoD(pulsecnt)}(Pulse = {pulsecnt}). ");
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"ElmoSetPostion({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                LoggerManager.Exception(err);

                return (int)EnumMotionBaseReturnCode.SetPositionError;
            }
            LoggerManager.Debug($"[Motion] ElmoSetPosition End Axis:{axis.Label.Value} return:{nRetVal}");
            return nRetVal;
        }

        public int SetAxisConfig_SWlimit(AxisObject axis, double NegLimit, double PosLimit)
        {
            try
            {
                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).SetParameter((double)axis.DtoP(NegLimit), MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_NEGATIVE_PARAM, (int)MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_NEGATIVE_PARAM);
                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).SetParameter((double)axis.DtoP(PosLimit), MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_POSITIVE_PARAM, (int)MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_POSITIVE_PARAM);
                return 0;
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);

                return -1;
            }
        }

        public int SetAxisConfig_Inposition(AxisObject axis, double Inposval)
        {
            double inpostime = 0.0;


            try
            {
                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).SetParameter((double)axis.DtoP(Inposval), MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS, (int)MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS);
                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).SetParameter(inpostime, MMC_PARAMETER_LIST_ENUM.MMC_TARGET_TIME, (int)MMC_PARAMETER_LIST_ENUM.MMC_TARGET_TIME);

                return 0;
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);

                return -1;
            }

        }

        public double GetAxisConfig_Inposition(AxisObject axis)
        {
            double rcvval = 0;

            try
            {
                rcvval = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetParameter(MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS, (int)MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS);

                return (double)axis.PtoD(rcvval);
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);

                return -1;
            }

        }

        public double GetAxisConfig_SWPOSLIMIT(AxisObject axis)
        {
            double rcvval = 0;



            try
            {
                rcvval = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetParameter(MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_POSITIVE_PARAM, (int)MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_POSITIVE_PARAM);

                return (double)axis.PtoD(rcvval);
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);

                return -1;
            }

        }

        public double GetAxisConfig_SWNEGLIMIT(AxisObject axis)
        {
            double rcvval = 0;

            try
            {
                rcvval = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetParameter(MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_NEGATIVE_PARAM, (int)MMC_PARAMETER_LIST_ENUM.MMC_SW_LIMIT_NEGATIVE_PARAM);

                return (double)axis.PtoD(rcvval);
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);

                return -1;
            }

        }

        //--------------------------------------------------
        public int Abort(AxisObject axis)
        {
            //Elmo에서는 사용되지 않는 함수 , MEI에서 사용되는 함수 이기때무에 만들어진 함수 
            //Return Value 0은 정상적으로 반환되는 값 
            return (int)EnumMotionBaseReturnCode.ReturnCodeOK;
        }

        private int WaitForMovingStarted(AxisObject axis, int timeout = 2000)
        {
            timeout = 10000;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int retVal = -1;
            long elapsed = 0;
            bool isMoving = false;
            try
            {
                while (true)
                {
                    retVal = QueryIsMoving(axis, ref isMoving);
                    ResultValidate(retVal);
                    if (isMoving)
                    {
                        elapsed = sw.ElapsedMilliseconds;
                        retVal = 0;
                        break;
                    }

                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        retVal = -1;
                        break;
                    }

                    Thread.Sleep(1);
                }

                sw.Stop();
            }
            catch (MMCException ex)
            {
                LoggerManager.Error($"WaitForMovingStarted() Function error axis = {axis.AxisIndex}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");

                //LoggerManager.DebugError($"WaitForMovingStarted() Function error: " + ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"WaitForMovingStarted() Function error: " + ex.Message);
                return -1;

            }

            return retVal;
        }

        public int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            double velocity = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    retVal = GetVelocity(axis, trjtype, ref velocity, ovrd);
                    ResultValidate(retVal);
                    retVal = AbsMove(axis, abspos, velocity, axis.Param.Acceleration.Value);
                    ResultValidate(retVal);
                }
            }
            catch (MMCException ex)
            {
                LoggerManager.Debug($"AbsMove({axis.Label.Value}): MMCException occurred. Err. code = {ex.MMCError}");

                LoggerManager.Error($"AbsMove() Function error axis = {axis.Label.Value}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                //LoggerManager.DebugError($"AbsMove() Function error axis = {axis.Label.Value}: " + ex.Message);
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"AbsMove() Function error axis = {axis.Label.Value}: " + ex.Message);
            }
            return retVal;
        }
        public int AbsMove(AxisObject axis, double abspos, double finalVel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1.0)
        {
            int retVal = -1;
            double velocity = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    retVal = GetVelocity(axis, trjtype, ref velocity, ovrd);
                    ResultValidate(retVal);
                    AbsMove(axis, abspos, velocity, axis.Param.Acceleration.Value);
                    ResultValidate(retVal);
                }
            }
            catch (MMCException ex)
            {
                LoggerManager.Debug($"AbsMove({axis.Label.Value}): MMCException occurred. Err. code = {ex.MMCError}");
                LoggerManager.Error($"AbsMove() Function error axis = {axis.Label.Value}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                //LoggerManager.DebugError($"AbsMove() Function error axis = {axis.Label.Value}: " + ex.Message);
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"AbsMove() Function error axis = {axis.Label.Value}: " + ex.Message);
            }
            return retVal;
        }


        public int AbsMove(AxisObject axis, double abspos, double vel, double acc)
        {
            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();
            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Setting Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
            int retVal = -1;
            double targetPos;
            //double refPos = 0;
            short axisIndex = (short)axis.AxisIndex.Value;

            //int nodenum = 0;

            double veltopulsecnt = 0.0;
            double acctopulsecnt = 0.0;
            double jerktopulsecnt = 0.0;


            //MotionParams motionparams = new MotionParams();
            //motionparams.position = abspos;
            //motionparams.finalVelocity = axis.Param.FinalVelociy.Value;

            targetPos = (double)axis.DtoP(abspos);
            targetPos = Math.Round(targetPos);
            veltopulsecnt = (double)axis.DtoP(vel);
            acctopulsecnt = (double)axis.DtoP(acc);
            jerktopulsecnt = (double)axis.DtoP(axis.Param.AccelerationJerk.Value);

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (!(axis is AxisObject))
                    {
                        LoggerManager.Error($"Axis is Not AxisObject Axis = {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWPOSLimitError;
                    }

                    if (abspos > axis.Param.PosSWLimit.Value)
                    {
                        targetPos = abspos;
                        LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWPOSLimitError;
                    }
                    else if (abspos < axis.Param.NegSWLimit.Value)
                    {
                        targetPos = abspos;
                        LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWNEGLimitError;
                    }
                    else
                    {
                        int cmdPulse = 0;
                        GetCommandPulse(axis, ref cmdPulse);
                        if (cmdPulse == targetPos)
                        {
                            //LoggerManager.Debug($"AbsMove() :skip the same position. axis = {axis.Label}");
                            retVal = 0;
                        }

                        else
                        {
                            if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                            {

                                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Move Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
                                MC_DIRECTION_ENUM Direction =
                                    targetPos >= 0 ? MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION : MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION;

                                retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveAbsoluteEx(
                                    targetPos, veltopulsecnt, acctopulsecnt, acctopulsecnt, jerktopulsecnt,
                                    Direction, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Move Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
                                if (retVal != 0)
                                {
                                    LoggerManager.Debug($"AbsMove() :AbsMoveFail axis = {axis.Label.Value}");
                                    return (int)EnumMotionBaseReturnCode.AbsMoveError;                                    
                                }
                                else
                                {
                                    axis.Status.Pulse.Command = targetPos;
                                }
                            }
                            else
                            {

                                double[] grouppos = new double[axis.GroupMembers.Count];
                                uint returnValue = 0;
                                bool isCompSampePos = true;



                                for (int i = 0; i < axis.GroupMembers.Count; i++)
                                {
                                    int groupactpulse = 0;

                                    grouppos[i] = targetPos + axis.GroupMembers[i].DtoP(axis.GroupMembers[i].Status.CompValue);
                                    retVal = GetActualPulse(axis.GroupMembers[i], ref groupactpulse);
                                    ResultValidate(retVal);
                                    if (grouppos[i] != groupactpulse)
                                    {
                                        isCompSampePos &= false;
                                    }
                                }

                                if (axis.Status.Pulse.Command == targetPos)
                                {
                                    //LoggerManager.Debug($"AbsMove() :skip the same position. axis = {axis.Label} ");

                                    retVal = 0;
                                }
                                else
                                {
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = acctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = acctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;

                                    returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearAbsoluteEx(veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                    if (axis.GroupMembers.Count == 3)
                                    {
                                        LoggerManager.Debug($"Group axis comp. values = {axis.GroupMembers[0].Status.CompValue}, {axis.GroupMembers[1].Status.CompValue}, {axis.GroupMembers[2].Status.CompValue}");
                                    }
                                    returnValue = 0;
                                    axis.Status.Pulse.Command = targetPos;


                                }
                                retVal = Convert.ToInt32(returnValue);
                            }

                        }
                    }
                }
            }
            catch (MMCException ex)
            {
                LoggerManager.Debug($"AbsMove({axis.Label.Value}): MMCException occurred. Err. code = {ex.MMCError}");

                LoggerManager.Error($"AbsMove() Function error axis = {axis.Label.Value}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                return (int)EnumMotionBaseReturnCode.AbsMoveError;
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"AbsMove() Function error axis = {axis.Label.Value}" + ex.Message);
                return (int)EnumMotionBaseReturnCode.AbsMoveError;

            }
            //stw.Stop();
            return retVal;

        }

        public int AbsMove(AxisObject axis, double abspos, double vel, double acc, double dcc)
        {
            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Setting Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
            int retVal = -1;
            double targetPos;
            //double refPos = 0;
            short axisIndex = (short)axis.AxisIndex.Value;

            //int nodenum = 0;

            double veltopulsecnt = 0.0;
            double acctopulsecnt = 0.0;
            double dcctopulsecnt = 0.0;
            double jerktopulsecnt = 0.0;


            //MotionParams motionparams = new MotionParams();
            //motionparams.position = abspos;
            //motionparams.finalVelocity = axis.Param.FinalVelociy.Value;

            targetPos = (double)axis.DtoP(abspos);
            targetPos = Math.Round(targetPos);
            veltopulsecnt = (double)axis.DtoP(vel);
            acctopulsecnt = (double)axis.DtoP(acc);
            dcctopulsecnt = (double)axis.DtoP(dcc);
            jerktopulsecnt = (double)axis.DtoP(axis.Param.AccelerationJerk.Value);

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (!(axis is AxisObject))
                    {
                        LoggerManager.Error($"Axis is Not AxisObject Axis = {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.FatalError;

                    }

                    if (abspos > axis.Param.PosSWLimit.Value)
                    {
                        targetPos = abspos;
                        LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWPOSLimitError;
                    }
                    else if (abspos < axis.Param.NegSWLimit.Value)
                    {
                        targetPos = abspos;
                        LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWNEGLimitError;
                    }
                    else
                    {
                        int cmdPulse = 0;
                        GetCommandPulse(axis, ref cmdPulse);
                        if (cmdPulse == targetPos)
                        {
                            //LoggerManager.Debug($"AbsMove() :skip the same position. axis = {axis.Label}");
                            retVal = 0;
                        }
                        else
                        {
                            if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                            {
                                //AxisStatusList[axis.AxisIndex.Value].Pulse.Command = targetPos;
                                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Move Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
                                MC_DIRECTION_ENUM Direction =
                                    targetPos >= 0 ? MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION : MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION;

                                retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveAbsoluteEx(
                                    targetPos, veltopulsecnt, acctopulsecnt, dcctopulsecnt, jerktopulsecnt,
                                    Direction, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Move Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
                                if (retVal != 0)
                                {
                                    retVal = (int)EnumMotionBaseReturnCode.AbsMoveError;
                                    LoggerManager.Debug($"AbsMove() :AbsMoveFail axis = {axis.Label.Value}");
                                }
                                else
                                {
                                    axis.Status.Pulse.Command = targetPos;
                                }

                            }
                            else
                            {
                                double[] grouppos = new double[axis.GroupMembers.Count];

                                for (int i = 0; i < axis.GroupMembers.Count; i++)
                                {
                                    grouppos[i] = targetPos + axis.GroupMembers[i].Status.CompValue;
                                }
                               ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = acctopulsecnt;
                                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = acctopulsecnt;
                                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;

                                var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearAbsoluteEx(veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                                retVal = 0;

                                AxisStatusList[axis.AxisIndex.Value].Pulse.Command = targetPos;
                                axis.Status.Pulse.Command = targetPos;
                            }
                        }
                    }
                }
            }
            catch (MMCException ex)
            {
                LoggerManager.Debug($"AbsMove({axis.Label.Value}): MMCException occurred. Err. code = {ex.MMCError}");
                LoggerManager.Error($"AbsMove() Function error axis = {axis.Label.Value}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                retVal = (int)EnumMotionBaseReturnCode.AbsMoveError;
                throw ex;
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"AbsMove() Function error axis= {axis.Label.Value} " + ex.Message);
                retVal = (int)EnumMotionBaseReturnCode.AbsMoveError;
            }
            //stw.Stop();
            return retVal;

        }
        public int ChuckTiltMove(AxisObject axis, double offsetz0, double offsetz1, double offsetz2, double abspos, double vel, double acc)
        {
            int retVal = -1;
            double targetPos;

            double veltopulsecnt = 0.0;
            double acctopulsecnt = 0.0;
            double jerktopulsecnt = 0.0;

            targetPos = (double)axis.DtoP(abspos);
            targetPos = Math.Round(targetPos);
            veltopulsecnt = (double)axis.DtoP(vel);
            acctopulsecnt = (double)axis.DtoP(acc);
            jerktopulsecnt = (double)axis.DtoP(axis.Param.AccelerationJerk.Value);

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (!(axis is AxisObject))
                    {
                        LoggerManager.Error($"Axis is Not AxisObject Axis = {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWPOSLimitError;
                    }

                    if (abspos > axis.Param.PosSWLimit.Value)
                    {
                        targetPos = abspos;
                        LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWPOSLimitError;
                    }
                    else if (abspos < axis.Param.NegSWLimit.Value)
                    {
                        targetPos = abspos;
                        LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWNEGLimitError;
                    }
                    else
                    {
                        if (axis.GroupMembers[0].Status.CompValue == offsetz0 &&
                           axis.GroupMembers[1].Status.CompValue == offsetz1 &&
                           axis.GroupMembers[2].Status.CompValue == offsetz2)
                        {
                            retVal = 0;
                        }
                        else
                        {
                            double[] grouppos = new double[axis.GroupMembers.Count];
                            axis.GroupMembers[0].Status.CompValue = offsetz0;
                            axis.GroupMembers[1].Status.CompValue = offsetz1;
                            axis.GroupMembers[2].Status.CompValue = offsetz2;
                            for (int i = 0; i < axis.GroupMembers.Count; i++)
                            {
                                grouppos[i] = targetPos + axis.GroupMembers[i].DtoP(axis.GroupMembers[i].Status.CompValue);
                            }

                               ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;

                            var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearAbsoluteEx(veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                            retVal = 0;

                            AxisStatusList[axis.AxisIndex.Value].Pulse.Command = targetPos;
                            axis.Status.Pulse.Command = targetPos;
                            for (int i = 0; i < axis.GroupMembers.Count; i++)
                            {
                                AxisStatusList[axis.GroupMembers[i].AxisIndex.Value].Pulse.Command = targetPos;
                                axis.GroupMembers[i].Status.Pulse.Command = grouppos[i];
                            }
                        }
                    }
                }
            }
            catch (MMCException ex)
            {
                LoggerManager.Debug($"ChuckTiltMove({axis.Label.Value}): MMCException occurred. Err. code = {ex.MMCError}");
                LoggerManager.Error($"ChuckTiltMove() Function error axis = {axis.Label.Value}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                return (int)EnumMotionBaseReturnCode.AbsMoveError;
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"ChuckTiltMove() Function error axis = {axis.Label.Value}" + ex.Message);
                return (int)EnumMotionBaseReturnCode.AbsMoveError;

            }
            //stw.Stop();
            return retVal;

        }

        public int AlarmReset(AxisObject axis)
        {
            //axis to node number
            //int nodenum = 0;
            int retVal = -1;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    mreSyncEvent.WaitOne(syncTimeOut_ms);
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {

                        uint statusWord = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();

                        if ((statusWord >> 29 & 0x01) == 0x01)
                        {
                            ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ResetAsync();
                            WaitforStatus(axis, EnumAxisState.IDLE);
                            retVal = 0;
                        }
                    }
                    else
                    {

                        uint statusWord = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                        if ((statusWord >> 29 & 0x01) == 0x01)
                        {
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReset();
                            WaitforStatus(axis, EnumAxisState.IDLE);
                            retVal = 0;
                        }

                    }

                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"AlarmReset({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                LoggerManager.Error($"AlarmReset() Function error axis= {axis.Label.Value} " + err.Message);
                LoggerManager.Exception(err);
                retVal = (int)EnumMotionBaseReturnCode.AmpfaultClearError;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"AlarmReset() Function error axis= {axis.Label.Value} " + err.Message);
                retVal = (int)EnumMotionBaseReturnCode.AmpfaultClearError;
            }
            return retVal;

        }
        public int AlarmReset(MMCSingleAxis axis)
        {
            //axis to node number
            //int nodenum = 0;
            int retVal = -1;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.AxisName}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    mreSyncEvent.WaitOne(syncTimeOut_ms);
                    uint statusWord = axis.ReadStatus();

                    if ((statusWord >> 29 & 0x01) == 0x01)
                    {
                        axis.ResetAsync();
                        WaitforStatus(axis, EnumAxisState.IDLE);
                        retVal = 0;
                    }

                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"AlarmReset({axis.AxisName}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                LoggerManager.Error($"AlarmReset() Function error axis= {axis.AxisName} " + err.Message);
                LoggerManager.Exception(err);
                retVal = (int)EnumMotionBaseReturnCode.AmpfaultClearError;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"AlarmReset() Function error axis= {axis.AxisName} " + err.Message);
                retVal = (int)EnumMotionBaseReturnCode.AmpfaultClearError;
            }
            return retVal;

        }
        public int AmpFaultClear(AxisObject axis)
        {
            int retVal = 0;
            LoggerManager.Debug($"[Motion]AmpFaultClear Start. {axis.Label.Value}");
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    mreSyncEvent.WaitOne(syncTimeOut_ms);
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {

                        uint statusWord = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();

                        if ((statusWord >> 29 & 0x01) == 0x01)
                        {
                            ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ResetAsync();
                            WaitforStatus(axis, EnumAxisState.DISABLED);
                            //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ResetAsync();
                            //WaitforStatus(axis, EnumAxisState.DISABLED);
                            retVal = 0;
                        }
                    }
                    else
                    {

                        uint statusWord = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                        if ((statusWord >> 29 & 0x01) == 0x01 | (statusWord >> 14 & 0x01) == 0x01)
                        {
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReset();
                            WaitforStatus(axis, EnumAxisState.DISABLED);
                            retVal = 0;
                        }

                    }
                }

            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"AlarmReset({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                LoggerManager.Error($"AmpFaultClear() Function error axis= {axis.Label.Value} " + err.Message);
                //LoggerManager.Debug($"Elmo AmpFaultClear failed.", ex.ToString());
                retVal = (int)EnumMotionBaseReturnCode.AmpfaultClearError;
            }
            LoggerManager.Debug($"[Motion]AmpFaultClear End. {axis.Label.Value} return:{retVal}");
            return retVal;
        }

        public int ApplyAxisConfig(AxisObject axis)
        {
            return 0;
        }

        public int ClearUserLimit(AxisObject axis)
        {
            return 0;
        }

        public int ClearUserLimit(int axisNumber)
        {
            return 0;
        }

        public int ConfigCapture(AxisObject axis, EnumMotorDedicatedIn input)
        {

            int nodeNum = GetNodeNum(axis.AxisIndex.Value);

            int inputInt = (int)input;

            //return _PMASManager.SendMessage(nodeNum, "HM[3]", inputInt.ToString());
            return this.PMASManager().SendMessage(nodeNum, "HM[3]", inputInt.ToString());
        }

        private int GetNodeNum(int axisIdx)
        {
            int nodeNum = -1;
            try
            {

                String axisName = MMCAxes[axisIdx].AxisName;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < axisName.Length; i++)
                {
                    if (char.IsDigit(axisName[i]))
                    {
                        sb.Append(axisName[i]);
                    }
                }
                string value = sb.ToString();
                Int32.TryParse(value, out nodeNum);

            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetNodeNum() Function error axisindex= {axisIdx} " + err.Message);
                //LoggerManager.Debug($"Elmo AmpFaultClear failed.", ex.ToString());
                LoggerManager.Exception(err);
            }

            return nodeNum;
        }
        public int ConfigCapture(AxisObject axis, int index)
        {

            throw new NotImplementedException();
        }

        public int DeInitMotionService()
        {
            int retVal = -1;

            try
            {
                bStopUpdateThread = true;
                //if (UpdateThread != null) UpdateThread.Join();
                UpdateThread?.Join();
                UpdateSDOThread?.Join();

                DevConnected = false;

                _monitoringTimer?.Stop();
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.DeInitError;

                LoggerManager.Error($"DeInitMotionService() Function error: " + err.Message);
            }
            return retVal;
        }

        public int DisableAxis(AxisObject axis)
        {
            //axis to node number
            //int nodenum = 0;
            int retVal = -1;
            bool value = false;
            LoggerManager.Debug($"[Motion] DisableAxis Start. Axis:{axis.Label.Value}");
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        retVal = IsMotorEnabled(axis, ref value);
                        ResultValidate(retVal);
                        if (value == true)
                        {
                            ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).PowerOff(MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                        }
                        else
                        {

                        }
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    }
                    else
                    {
                        var status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                        retVal = IsGroupEnabled(status, ref value);
                        ResultValidate(retVal);
                        if (value == true)
                        {
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupDisable();
                        }
                        else
                        {

                        }
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    }
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"DisableAxis({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                retVal = (int)EnumMotionBaseReturnCode.DisbleAxisrError;
                LoggerManager.Exception(err);
            }
            finally
            {
                axis.Status.IsHomeSeted = false;
            }
            LoggerManager.Debug($"[Motion] DisableAxis End. Axis:{axis.Label.Value} return:{retVal}");
            return retVal;
        }

        public int DisableCapture(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int EnableAxis(AxisObject axis)
        {
            //axis to node number
            //int nodenum = 0;
            int retVal = -1;
            bool value = false;
            LoggerManager.Debug($"[Motion]EnableAxis Start. {axis.Label.Value}");

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                    {
                        var status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                        retVal = IsGroupEnabled(status, ref value);
                        ResultValidate(retVal);
                        if (value)
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupEnable();
                        }

                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    }
                    else
                    {
                        retVal = IsMotorEnabled(axis, ref value);
                        ResultValidate(retVal);
                        if (value == false)
                        {
                            if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                            {
                                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).PowerOn(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            }
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                    }
                }
            }
            catch (MMCException err)
            {
                retVal = (int)EnumMotionBaseReturnCode.EnableAxisrError;
                LoggerManager.Debug($"EnableAxis({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[Motion]EnableAxis End. {axis.Label.Value} return:{retVal}");
            return retVal;
        }

        public int GetActualPosition(AxisObject axis, ref double pos)
        {
            //axis to node number
            double actpospulse = 0;
            int retVal = -1;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    mreSyncEvent.WaitOne(syncTimeOut_ms);
                //    if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                //    {
                //        actpospulse = (int)stReadBulkReadData[axis.AxisIndex.Value].aPos;
                //        pos = axis.PtoD(actpospulse);
                //    }
                //    else
                //    {
                //        actpospulse = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetActualPosition();
                //        pos = axis.PtoD(actpospulse);
                //    }
                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //}
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                {
                    actpospulse = (int)stReadBulkReadData[axis.AxisIndex.Value].aPos;
                    pos = axis.PtoD(actpospulse);
                }
                else
                {
                    actpospulse = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetActualPosition();
                    //actpospulse = (int)stReadBulkReadData[axis.AxisIndex.Value].aPos;
                    pos = axis.PtoD(actpospulse);
                }
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetActualPosition({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                retVal = (int)EnumMotionBaseReturnCode.GetActualPositionError;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public int GetActualPulse(AxisObject axis, ref int pos)
        {
            //axis to node number
            int retVal = -1;
            //double pulse = 0d;

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    mreSyncEvent.WaitOne(syncTimeOut_ms);
                //    //pos = (int)stReadBulkReadData[axis.AxisIndex.Value].aPos;

                //    //if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                //    //{
                //    pos = stReadBulkReadData[axis.AxisIndex.Value].aPos;
                //    //}
                //    //else
                //    //{
                //    //    pulse = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetActualPosition();
                //    //    pos = Convert.ToInt32(pulse);
                //    //}

                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //}
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                //pos = (int)stReadBulkReadData[axis.AxisIndex.Value].aPos;

                //if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                //{
                pos = stReadBulkReadData[axis.AxisIndex.Value].aPos;
                //}
                //else
                //{
                //    pulse = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetActualPosition();
                //    pos = Convert.ToInt32(pulse);
                //}

                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetActualPulse({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                retVal = (int)EnumMotionBaseReturnCode.GetActualPulseError;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetAlarmCode(AxisObject axis, ref ushort alarmcode)
        {
            //axis to node number
            short axisIndex = (short)axis.AxisIndex.Value;
            ushort errstatus = 0;
            int retVal = -1;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    mreSyncEvent.WaitOne(syncTimeOut_ms);
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        errstatus = (ushort)((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).LastError;
                        //errstatus = (short)stReadBulkReadData[axisIndex].usLastEmcyErrorCode.iVar;
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    }
                    else if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                    {
                        errstatus = (ushort)((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).LastError;
                        //errstatus = (short)stReadBulkReadData[axisIndex].usLastEmcyErrorCode.iVar;
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    }
                }

                mreSyncEvent.WaitOne(syncTimeOut_ms);
                if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                {
                    errstatus = (ushort)((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).LastError;
                    //errstatus = (short)stReadBulkReadData[axisIndex].usLastEmcyErrorCode.iVar;
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                }
                else if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                {
                    errstatus = (ushort)((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).LastError;
                    //errstatus = (short)stReadBulkReadData[axisIndex].usLastEmcyErrorCode.iVar;
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetAlarmCode({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }
            finally
            {
                alarmcode = errstatus;
            }
            return retVal;
        }

        public int GetAxisInputs(AxisObject axis, ref uint instatus)
        {
            throw new NotImplementedException();
        }

        public int GetAxisState(AxisObject axis, ref int state)
        {
            int retVal = -1;
            int axis_state = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    mreSyncEvent.WaitOne(syncTimeOut_ms);
                //    //axis to node number
                //    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                //    {
                //        var status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                //        //status = stReadBulkReadData[axis.AxisIndex.Value].ulAxisStatus;
                //        var axisstate = GetServoState1(status);
                //        state = (int)axisstate;
                //        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

                //    }
                //    else
                //    {
                //        var status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                //        var axisstate = GetServoState1(status);
                //        state = (int)axisstate;
                //        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //    }
                //}

                mreSyncEvent.WaitOne(syncTimeOut_ms);
                //axis to node number
                if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                {
                    var status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                    //status = stReadBulkReadData[axis.AxisIndex.Value].ulAxisStatus;
                    var axisstate = GetServoState1(status);
                    state = (int)axisstate;
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

                }
                else
                {
                    var status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                    var axisstate = GetServoState1(status);
                    state = (int)axisstate;
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetAxisState({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                LoggerManager.Exception(err);
            }
            finally
            {
                state = axis_state;
            }

            return retVal;

        }
        [Obsolete("Type casting error.")]
        public int GetAxisStatus(AxisObject axis)
        {
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            //axis to node number
            //return (int)stReadBulkReadData[axis.AxisIndex.Value].ulAxisStatus;
            int retVal = -1;
            uint val = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    GetElmoAxisStatus(axis, ref val);
                //    retVal = Convert.ToInt32(val);
                //}

                GetElmoAxisStatus(axis, ref val);
                retVal = Convert.ToInt32(val);
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetAxisStatus({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                retVal = (int)EnumMotionBaseReturnCode.GetAxisStatusError;
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetAxisStatusError;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public int GetAxisStatus(AxisObject axis, ref uint val)
        {
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            //axis to node number
            //return (int)stReadBulkReadData[axis.AxisIndex.Value].ulAxisStatus;
            int retVal = (int)GetElmoAxisStatus(axis, ref val);

            return retVal;
        }
        private int GetElmoAxisStatus(AxisObject axis, ref uint val)
        {
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            //axis to node number
            //return (int)stReadBulkReadData[axis.AxisIndex.Value].ulAxisStatus;
            uint status = 0;
            int retVal = -1;

            try
            {
                //lock (axis)
                //{
                //    if (axis.AxisGroupType.Value == EnumAxisGroupType.GROUPAXIS)
                //    {
                //        status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                //    }
                //    else
                //    {
                //        status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                //    }
                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //}
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                status = stReadBulkReadData[axis.AxisIndex.Value].ulAxisStatus;
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetElmoAxisStatus({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.GetAxisStatusError;
                LoggerManager.Exception(err);
            }
            finally
            {
                val = status;
            }
            return retVal;
        }

        public int GetCaptureStatus(int axisindex, AxisStatus status)
        {
            throw new NotImplementedException();
        }

        public int GetCmdPosition(AxisObject axis, ref double cmdpos)
        {
            int retVal = -1;
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    cmdpos = 0.0;
                //    cmdpos = axis.Status.Position.Command;
                //    cmdpos = axis.PtoD(axis.Status.Pulse.Command);
                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //}
                cmdpos = 0.0;
                cmdpos = axis.Status.Position.Command;
                cmdpos = axis.PtoD(axis.Status.Pulse.Command);
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetCommandPositionError;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public int GetCommandPosition(AxisObject axis, ref double pos)
        {
            int retVal = -1;
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    pos = 0.0;
                //    //pos = axis.Status.Position.Command;
                //    pos = axis.PtoD(axis.Status.Pulse.Command);
                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //}
                pos = 0.0;
                //pos = axis.Status.Position.Command;
                pos = axis.PtoD(axis.Status.Pulse.Command);
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetCommandPositionError;
                LoggerManager.Exception(err);
            }
            finally
            {
                pos = axis.PtoD(axis.Status.Pulse.Command);
            }

            return retVal;
        }
        //public int GetCommandPulse(int axisIndex, ref int pos)
        //{
        //    int targetPulse = 0;
        //    int retVal = -1;
        //    pos = 0;
        //    try
        //    {

        //        //((MMCSingleAxis)MMCAxes[axisIndex]).UploadSDO(0x607A, 0, out actpospulse, 500);
        //        pos = Convert.ToInt32(axis.Status.Pulse.Command);
        //        //pos = actpospulse;
        //        //((MMCSingleAxis)MMCAxes[axisIndex]).UploadSDO(0x607A, 0, out targetPulse, 100);
        //        //pos = targetPulse;
        //        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

        //    }
        //    catch (MMCException err)
        //    {
        //        retVal = (int)EnumMotionBaseReturnCode.GetCommandPulseError;
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

        public int GetCommandPulse(AxisObject axis, ref int pos)
        {
            int retVal = -1;
            try
            {
                pos = Convert.ToInt32(axis.Status.Pulse.Command);
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }

            catch (MMCException err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetCommandPulseError;
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetCommandPulseError;
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public bool GetHomeSensorState(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public bool GetIOAmpFault(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public bool GetIOHome(AxisObject axis)
        {
            int ret = -1;
            bool value = false;
            ret = IsHomeSensor(axis.AxisIndex.Value, ref value);

            return value;
        }

        public bool GetIONegLim(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public bool GetIOPosLim(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int GetMotorAmpEnable(AxisObject axis, ref bool turnAmp)
        {
            throw new NotImplementedException();
        }

        public int GetPosError(AxisObject axis, ref double poserr)
        {
            int retVal = -1;
            try
            {
                retVal = GetErrorPosition(axis, ref poserr);
                ResultValidate(retVal);
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetPosError({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.GetErrorPositionError;
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetErrorPositionError;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetVelocity(AxisObject axis, EnumTrjType trjtype, ref double vel, double ovrd = 1)
        {
            int retVal = -1;
            //double pulse = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    switch (trjtype)
                //    {
                //        case EnumTrjType.Normal:
                //            vel = axis.Param.Speed.Value;
                //            break;
                //        case EnumTrjType.Probing:
                //            vel = axis.Param.SeqSpeed.Value;
                //            break;
                //        case EnumTrjType.Homming:
                //            vel = axis.Param.HommingSpeed.Value;
                //            break;
                //        case EnumTrjType.Emergency:
                //            vel = axis.Param.Speed.Value * 1.5;
                //            break;
                //        default:
                //            break;
                //    }
                //    vel = vel * ovrd;
                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //}

                switch (trjtype)
                {
                    case EnumTrjType.Normal:
                        vel = axis.Param.Speed.Value;
                        break;
                    case EnumTrjType.Probing:
                        vel = axis.Param.SeqSpeed.Value;
                        break;
                    case EnumTrjType.Homming:
                        vel = axis.Param.HommingSpeed.Value;
                        break;
                    case EnumTrjType.Emergency:
                        vel = axis.Param.Speed.Value * 1.5;
                        break;
                    default:
                        break;
                }
                vel = vel * ovrd;
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetVelocityError;
                LoggerManager.Error($"GetAccel() Function error: " + err.Message);
            }

            return retVal;
        }

        public int Halt(AxisObject axis, double value)
        {
            throw new NotImplementedException();
        }

        public bool HasAlarm(AxisObject axis)
        {
            throw new NotImplementedException();
        }
        private int WaitforOPMode(MMCSingleAxis axis, OPM402 opmode, int timeout = 0)
        {
            int retVal = -1;
            //int cnt = 0;
            Stopwatch elapsedStopWatch = new Stopwatch();
            OPM402 tmpop = OPM402.OPM402_NO;
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Restart();
            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.AxisName), stw.ElapsedMilliseconds));
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.AxisName}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    bool runFlag = true;
                //    mreSyncEvent.WaitOne(syncTimeOut_ms);
                //    do
                //    {
                //        tmpop = axis.GetOpMode();
                //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetOpMode {0}axis", axis.AxisName), stw.ElapsedMilliseconds));

                //        if (tmpop != opmode)
                //        {
                //            if (timeout != 0)
                //            {
                //                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                //                {
                //                    LoggerManager.Error($"WaitforOPMode({axis.AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                //                    runFlag = false;
                //                    retVal = -2;
                //                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;

                //                }
                //            }
                //            else
                //            {
                //                runFlag = true;
                //            }
                //        }
                //        else
                //        {
                //            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //            runFlag = false;
                //        }
                //    } while (runFlag == true);
                //    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Settling.... {0}axis", axis.AxisName), stw.ElapsedMilliseconds));
                //}

                bool runFlag = true;
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                do
                {
                    tmpop = axis.GetOpMode();
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetOpMode {0}axis", axis.AxisName), stw.ElapsedMilliseconds));

                    if (tmpop != opmode)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitforOPMode({axis.AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;

                            }
                        }
                        else
                        {
                            runFlag = true;
                        }
                    }
                    else
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        runFlag = false;
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitforOPMode() Function error: {err.Message}");
                retVal = (int)EnumMotionBaseReturnCode.WaitforFunctionError;

            }
            finally
            {
                elapsedStopWatch?.Stop();
                //double pos=0;
                // GetCmdPosition(axis, ref pos);
                // axis.Status.Position.Ref = pos;
            }
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.AxisName), stw.ElapsedMilliseconds));
            //stw.Stop();
            //int step = 0;
            //foreach (var item in timeStamp)
            //{
            //    step++;
            //    Log.Debug(string.Format("Time Stamp [{0}] - Desc: {1}, Time: {2}", step, item.Key, item.Value));
            //}
            return retVal;
        }
        private int WaitforOPMode(AxisObject axis, OPM402 opmode, int timeout = 0)
        {
            LoggerManager.Debug($"[Motion] WaitforOPMode Start. Axis:{axis.Label.Value}");
            int retVal = -1;
            //int cnt = 0;
            Stopwatch elapsedStopWatch = new Stopwatch();
            OPM402 tmpop = OPM402.OPM402_NO;
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Restart();
            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    bool runFlag = true;
                //    mreSyncEvent.WaitOne(syncTimeOut_ms);
                //    do
                //    {
                //        tmpop = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetOpMode();
                //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetOpMode {0}axis", axis.Label), stw.ElapsedMilliseconds));

                //        if (tmpop != opmode)
                //        {
                //            if (timeout != 0)
                //            {
                //                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                //                {
                //                    LoggerManager.Error($"WaitforOPMode({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                //                    runFlag = false;
                //                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;

                //                }
                //            }
                //            else
                //            {
                //                runFlag = true;
                //            }
                //        }
                //        else
                //        {
                //            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //            runFlag = false;
                //        }
                //    } while (runFlag == true);
                //    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Settling.... {0}axis", axis.Label), stw.ElapsedMilliseconds));


                //    if (axis.SettlingTime > 0)
                //    {
                //        Thread.Sleep(axis.SettlingTime / 2);
                //    }
                //    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));
                //}
                bool runFlag = true;
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                do
                {
                    tmpop = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetOpMode();
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetOpMode {0}axis", axis.Label), stw.ElapsedMilliseconds));

                    if (tmpop != opmode)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitforOPMode({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;

                            }
                        }
                        else
                        {
                            runFlag = true;
                        }
                    }
                    else
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        runFlag = false;
                    }
                } while (runFlag == true);
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Settling.... {0}axis", axis.Label), stw.ElapsedMilliseconds));


                if (axis.SettlingTime > 0)
                {
                    Thread.Sleep((int)axis.SettlingTime / 2);
                }
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.WaitforFunctionError;
                LoggerManager.Error($"WaitforOPMode() Function error: {err.Message}");
            }
            finally
            {
                elapsedStopWatch?.Stop();
                //double pos=0;
                // GetCmdPosition(axis, ref pos);
                // axis.Status.Position.Ref = pos;
            }
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //stw.Stop();
            //int step = 0;
            //foreach (var item in timeStamp)
            //{
            //    step++;
            //    Log.Debug(string.Format("Time Stamp [{0}] - Desc: {1}, Time: {2}", step, item.Key, item.Value));
            //}
            LoggerManager.Debug($"[Motion] WaitforOPMode End. Axis:{axis.Label.Value} return:{retVal}");

            return retVal;
        }
        private int WaitforStatus(AxisObject axis, EnumAxisState state, int timeout = 0)
        {
            int retVal = -1;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    bool runFlag = true;
                //    mreSyncEvent.WaitOne(syncTimeOut_ms);
                //    uint status;
                //    EnumAxisState tmpstate = EnumAxisState.ERROR;
                //    do
                //    {
                //        if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                //        {
                //            status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                //            tmpstate = GetServoState1(status);
                //        }
                //        else
                //        {
                //            status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                //            tmpstate = GetServoState1(status);
                //        }
                //        if (state != tmpstate)
                //        {
                //            if (timeout != 0)
                //            {
                //                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                //                {
                //                    LoggerManager.Debug($"WaitforStatus({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Timeout error occurred. Timeout = {timeout}ms");
                //                    runFlag = false;
                //                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                //                }
                //            }
                //            else
                //            {
                //                runFlag = true;
                //            }
                //        }
                //        else
                //        {
                //            runFlag = false;
                //            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //        }
                //    } while (runFlag == true);
                //}
                bool runFlag = true;
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                uint status;
                EnumAxisState tmpstate = EnumAxisState.ERROR;
                do
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                        tmpstate = GetServoState1(status);
                    }
                    else
                    {
                        status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                        tmpstate = GetServoState1(status);
                    }
                    if (state != tmpstate)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Debug($"WaitforStatus({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Timeout error occurred. Timeout = {timeout}ms");
                                runFlag = false;
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                            }
                        }
                        else
                        {
                            runFlag = true;
                        }
                    }
                    else
                    {
                        runFlag = false;
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.WaitforFunctionError;
                LoggerManager.Error($"WaitforStatus() Function error: {err.Message}");
            }
            finally
            {
                elapsedStopWatch?.Stop();
            }

            return retVal;
        }

        private int WaitforStatus(MMCSingleAxis axis, EnumAxisState state, int timeout = 0)
        {
            int retVal = -1;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.AxisName}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                //    bool runFlag = true;
                //    mreSyncEvent.WaitOne(syncTimeOut_ms);
                //    uint status;
                //    EnumAxisState tmpstate = EnumAxisState.ERROR;
                //    do
                //    {
                //        status = ((MMCSingleAxis)axis).ReadStatus();
                //        tmpstate = GetServoState1(status);
                //        if (state != tmpstate)
                //        {
                //            if (timeout != 0)
                //            {
                //                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                //                {
                //                    LoggerManager.Debug($"WaitforStatus({axis.AxisName}) : Timeout error occurred. Timeout = {timeout}ms");
                //                    runFlag = false;
                //                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                //                }
                //            }
                //            else
                //            {
                //                if (tmpstate == EnumAxisState.DISABLED) // If disable, consider as idle state.
                //                {
                //                    runFlag = false;
                //                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //                }
                //                else
                //                {
                //                    runFlag = true;
                //                }
                //            }

                //        }
                //        else
                //        {
                //            runFlag = false;
                //            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //        }
                //    } while (runFlag == true);
                //}
                bool runFlag = true;
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                uint status;
                EnumAxisState tmpstate = EnumAxisState.ERROR;
                do
                {
                    status = ((MMCSingleAxis)axis).ReadStatus();
                    tmpstate = GetServoState1(status);
                    if (state != tmpstate)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Debug($"WaitforStatus({axis.AxisName}) : Timeout error occurred. Timeout = {timeout}ms");
                                runFlag = false;
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                            }
                        }
                        else
                        {
                            if (tmpstate == EnumAxisState.DISABLED) // If disable, consider as idle state.
                            {
                                runFlag = false;
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            }
                            else
                            {
                                runFlag = true;
                            }
                        }

                    }
                    else
                    {
                        runFlag = false;
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.WaitforFunctionError;
                LoggerManager.Error($"WaitforStatus() Function error: {err.Message}");
            }
            finally
            {
                elapsedStopWatch?.Stop();
            }
            return retVal;
        }

        private int SetOperationMode(MMCSingleAxis axis, OPM402 opmode)
        {
            int retVal = -1;
            try
            {
                OPM402 tmpop = axis.GetOpMode();
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.AxisName}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (opmode == OPM402.OPM402_HOMING_MODE)
                    {
                        if (tmpop != OPM402.OPM402_HOMING_MODE)
                        {
                            axis.SetOpMode(OPM402.OPM402_HOMING_MODE);
                            WaitforOPMode(axis, OPM402.OPM402_HOMING_MODE, 5000);
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }

                    }
                    else if (opmode == OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE)
                    {
                        if (tmpop != OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE)
                        {
                            axis.SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                            WaitforOPMode(axis, OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE, 5000);
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }

                    }
                    else
                    {
                        retVal = (int)EnumMotionBaseReturnCode.FatalError;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private int SetOperationMode(AxisObject axis, OPM402 opmode)
        {
            int retVal = -1;
            LoggerManager.Debug($"[Motion] SetOperationMode Start. Axis{axis.Label.Value}");
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    OPM402 tmpop = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetOpMode();

                    if (opmode == OPM402.OPM402_HOMING_MODE)
                    {
                        if (tmpop != OPM402.OPM402_HOMING_MODE)
                        {
                            ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).SetOpMode(OPM402.OPM402_HOMING_MODE);
                            retVal = WaitforOPMode(axis, OPM402.OPM402_HOMING_MODE, 60000);
                            //retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }

                    }
                    else if (opmode == OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE)
                    {
                        if (tmpop != OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE)
                        {
                            ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                            retVal = WaitforOPMode(axis, OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE, 60000);
                            //retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }

                    }
                    else
                    {
                        retVal = (int)EnumMotionBaseReturnCode.FatalError;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            LoggerManager.Debug($"[Motion] SetOperationMode End. Axis{axis.Label.Value} return:{retVal}");
            return retVal;
        }
        public int Homming(AxisObject axis)
        {
            int nRetVal = -1;
            LoggerManager.Debug($"[Motion]Homming Start. Axis:{axis.Label.Value}");
            //OPM402 tmpval;
            double velocity = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return nRetVal;
                //    }
                lock (axis)
                {

                    axis.Status.IsHoming = true;
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        int nodeNum = GetNodeNum(axis.AxisIndex.Value);
                        if (axis.Label.Value == "EJX1" || axis.Label.Value == "EJY1" || axis.Label.Value == "FDT1" || axis.Label.Value == "EJPZ1" || axis.Label.Value == "EJZ1" || axis.Label.Value == "NZD")
                        {

                        }
                        else
                        {
                            this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");
                            SetFeedrate(axis, 1, 1);
                        }
                        //tmpval = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetOpMode();
                        AmpFaultClear(axis);

                        EnableAxis(axis);
                        SetOperationMode(axis, OPM402.OPM402_HOMING_MODE);
                        nRetVal = GetVelocity(axis, EnumTrjType.Homming, ref velocity);
                        ResultValidate(nRetVal);
                        DS402HomingProgress(((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]),
                               (int)axis.HomingType.Value,
                               axis.DtoP(axis.Param.HomeOffset.Value * -1d),
                               (float)axis.DtoP(velocity),
                               (float)axis.DtoP(GetAccel(axis, EnumTrjType.Homming)),
                               (float)axis.DtoP((Math.Abs(axis.Param.PosSWLimit.Value) + Math.Abs(axis.Param.NegSWLimit.Value))),
                               (float)axis.DtoP(axis.Param.IndexSearchingSpeed.Value),
                               (float)axis.DtoP(axis.Param.HommingSpeed.Value),
                               (float)axis.DtoP(velocity));
                        SetOperationMode(axis, OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                        Thread.Sleep(500);
                        double posOffset = 0;
                        if (axis.Param.HomeOffset.Value >= 0)
                        {
                            posOffset = -100;
                        }
                        else
                        {
                            posOffset = 100;
                        }

                        axis.Status.Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                        axis.Status.Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                        axis.Status.Position.Ref = axis.Param.HomeOffset.Value;
                        if (axis.HomingType.Value == HomingMethodType.RLSEDGE)
                        {
                            axis.Status.Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Position.Ref = axis.Param.HomeOffset.Value;

                            //int acutalPluse = 0;
                            //GetActualPulse(axis, ref acutalPluse);
                            //AxisStatusList[axis.AxisIndex.Value].Pulse.Command = acutalPluse;
                            //axis.Status.Pulse.Command = acutalPluse;
                            axis.Status.IsHoming = false;
                            axis.Status.IsHomeSeted = true;
                            nRetVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            //TRI축 같은경우 이미RLS인데 RLS방향으로 무빙을 하면 에러가남 
                        }
                        else if (axis.HomingType.Value == HomingMethodType.FLSEDGE)
                        {
                            axis.Status.Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Position.Ref = axis.Param.HomeOffset.Value;

                            axis.Status.IsHoming = false;
                            axis.Status.IsHomeSeted = true;
                            nRetVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            //TRI축 같은경우 이미RLS인데 RLS방향으로 무빙을 하면 에러가남 
                        }
                        else
                        {
                            // <-- 251106 sebas
                            if (axis.Label.Value == "X" || axis.Label.Value == "Y" || axis.Label.Value == "T" || axis.Label.Value == "TRI" || axis.Label.Value == "Z" || axis.Label.Value == "Z0" || axis.Label.Value == "Z1" || axis.Label.Value == "Z2")
                            {
                                // 251106 sebas 주석처리
                                //nRetVal = RelMove(axis, posOffset,
                                //                            axis.Param.HommingSpeed.Value,
                                //                            axis.Param.HommingAcceleration.Value);
                                //ResultValidate(nRetVal);
                                //nRetVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                                //ResultValidate(nRetVal);

                                //nRetVal = AbsMove(axis, axis.Param.HomeOffset.Value, EnumTrjType.Normal);
                                //ResultValidate(nRetVal);
                                //nRetVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                                //ResultValidate(nRetVal);
                            }
                            else
                            {

                            }
                            // -->
                            axis.Status.Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Position.Ref = axis.Param.HomeOffset.Value;

                            //int acutalPluse = 0;
                            //GetActualPulse(axis, ref acutalPluse);
                            //AxisStatusList[axis.AxisIndex.Value].Pulse.Command = acutalPluse;
                            //axis.Status.Pulse.Command = acutalPluse;
                            axis.Status.IsHoming = false;
                            axis.Status.IsHomeSeted = true;
                            nRetVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        //251103 yb 초기화전 파워 오프
                        ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).PowerOff(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                    }
                    else
                    {
                        foreach (var groupmember in axis.GroupMembers)
                        {
                            SetFeedrate(groupmember, 1, 1);
                            groupmember.Status.IsHoming = true;
                        }
                        if (axis.HomingType.Value == HomingMethodType.SYNC_NHPI)
                        {
                            nRetVal = SyncNHPIHoming(axis);
                            AxisStatusList[axis.AxisIndex.Value].Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Position.Command = axis.Param.HomeOffset.Value;
                            axis.Status.Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                            axis.Status.Position.Ref = axis.Param.HomeOffset.Value;

                            axis.Status.IsHoming = false;
                            axis.Status.IsHomeSeted = true;

                            foreach (var groupmember in axis.GroupMembers)
                            {
                                groupmember.Status.IsHomeSeted = true;
                                groupmember.Status.IsHoming = false;
                            }
                        }
                    }
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"Homming({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                axis.Status.IsHoming = false;
                nRetVal = (int)EnumMotionBaseReturnCode.HomingError;
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[Motion]HommingEnd. Axis:{axis.Label.Value} return:{nRetVal}");
            return nRetVal;

        }
        //public int Homming(AxisObject axis)
        //{

        //    int nRetVal = 0;
        //    OPM402 tmpval;


        //    float ftorqueval = 0;
        //    float fvel = 0;
        //    float facc = 0;
        //    int homingmethodnum = 0;


        //    if ((axis.AxisIndex.Value == 0) || (axis.AxisIndex.Value == 1))
        //    {
        //        ftorqueval = 3000;
        //        fvel = axis.DtoP(2500);
        //        facc = axis.DtoP(25000);
        //    }
        //    else
        //    {
        //        ftorqueval = 1000;
        //        fvel = axis.DtoP(2000);
        //        facc = axis.DtoP(20000);
        //    }

        //    try
        //    {

        //        tmpval = MMCAxes[axis.AxisIndex.Value].GetOpMode();

        //        if (tmpval == OPM402.OPM402_HOMING_MODE)
        //        {
        //            MMCAxes[axis.AxisIndex.Value].SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
        //        }


        //        MMCAxes[axis.AxisIndex.Value].SetOpMode(OPM402.OPM402_HOMING_MODE);
        //        tmpval = MMCAxes[axis.AxisIndex.Value].GetOpMode();
        //        if (tmpval != OPM402.OPM402_HOMING_MODE)
        //        {
        //            LoggerManager.Debug($"Homing mode set failed.");
        //            return (int)EventCodeEnum.MOTION_OPMODE_ERROR;
        //        }
        //        else
        //        {
        //            //nRetVal = DS402HomingProgress(
        //            //    MMCAxes[axis.AxisIndex.Value],
        //            //    (int)axis.HomingType,
        //            //    axis.DtoP(axis.Param.HomeOffset.Value),
        //            //    (float)GetVelocity(axis, EnumTrjType.Homming),
        //            //    (float)GetAccel(axis, EnumTrjType.Homming),
        //            //    axis.DtoP(Math.Abs(axis.Param.PosSWLimit.Value) + Math.Abs(axis.Param.NegSWLimit.Value)));
        //            nRetVal = DS402HomingProgress(MMCAxes[axis.AxisIndex.Value], (int)axis.HomingType);
        //        }

        //        MMCAxes[axis.AxisIndex.Value].SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
        //        return 0;
        //    }
        //    catch (MMCException ex)
        //    {
        //        LoggerManager.Debug($ex.ToString());
        //        return -1;
        //    }

        //}

        public int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset)
        {
            throw new NotImplementedException();
        }

        public int Homming(AxisObject axis, bool reverse, int input, double homeoffset)
        {
            throw new NotImplementedException();
        }

        public int Homming(AxisObject axis, bool reverse, EnumMCInputs input, double homeoffset)
        {
            throw new NotImplementedException();
        }



        //public bool IsHoming(AxisObject axis)
        //{
        //    throw new NotImplementedException();
        //}

        public int IsInposition(AxisObject axis, ref bool val)
        {

            //int nodenum = 0;
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            //uint statusRegister = 0;
            //uint mcsLimitRegister = 0;
            bool isInpos = false;
            try
            {
                //lock (axis)
                //{
                //mreSyncEvent.WaitOne(syncTimeOut_ms);
                //if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                //{
                //    if (((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus() == ELMO_Global_Constants.VALID_STAND_STILL_MASK)
                //    {
                //        val = true;
                //        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //    }
                //    else
                //    {
                //        val = false;
                //        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //    }
                //}
                //else
                //{
                //    if (((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus() == ELMO_Global_Constants.VALID_GROUP_STANDBY_MASK)
                //    {
                //        val = true;
                //        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //    }
                //    else
                //    {
                //        val = false;
                //        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //    }
                //}
                bool synced = false;
                synced = mreSyncEvent.WaitOne(previewSyncTimeOut);
                if (synced == false)
                {
                    // LoggerManager.Debug($"IsInposition({axis.Label.Value}): Update sync timeouted. Enter to sync recovery procedure.");
                    synced = mreSyncEvent.WaitOne(syncTimeOut_ms);
                    if (synced == true)
                    {
                        LoggerManager.Debug($"IsInposition({axis.Label.Value}): Update sync recovered.");
                    }
                    else
                    {
                        LoggerManager.Debug($"IsInposition({axis.Label.Value}): Update sync timeouted. Presume out of position.");
                    }
                }
                if (synced == true)
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetStatusRegister(ref statusRegister, ref mcsLimitRegister); 
                        //if ((((statusRegister >> 12) & 0x01) == 0x01) && (((statusRegister >> 11) & 0x01) == 0x01) && (((statusRegister >> 10) & 0x01) == 0x01))
                        //{
                        //    val = true;
                        //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        //}
                        //else
                        //{
                        //    val = false;
                        //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        //}
                        //using (Locker locker = new Locker(bulkReadBufferLock))
                        //{
                        //    if (locker.AcquiredLock == false)
                        //    {
                        //        System.Diagnostics.Debugger.Break();
                        //        return retVal;
                        //    }

                        //    isInpos = IsInposition(stReadBulkReadData[axis.AxisIndex.Value].uiStatusRegister);
                        //}
                        isInpos = IsInposition(stReadBulkReadData[axis.AxisIndex.Value].uiStatusRegister);
                        if (isInpos)
                        {
                            val = true;
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            val = false;
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                    }
                    else
                    {
                        //m_BulkRead.Perform();
                        bool groupinpos = false;
                        //using (Locker locker = new Locker(bulkReadBufferLock))
                        //{
                        //    if (locker.AcquiredLock == false)
                        //    {
                        //        System.Diagnostics.Debugger.Break();
                        //        return retVal;
                        //    }

                        //    groupinpos = IsInposition(stReadBulkReadData[axis.AxisIndex.Value].uiStatusRegister);
                        //}
                        groupinpos = IsInposition(stReadBulkReadData[axis.AxisIndex.Value].uiStatusRegister);
                        //for (int i = 0; i < axis.GroupMembers.Count; i++)
                        //{
                        //    ((MMCSingleAxis)MMCAxes[axis.GroupMembers[i].AxisIndex.Value]).GetStatusRegister(ref statusRegister, ref mcsLimitRegister);
                        //    if ((((statusRegister >> 12) & 0x01) == 0x01) && (((statusRegister >> 11) & 0x01) == 0x01) && (((statusRegister >> 10) & 0x01) == 0x01))
                        //    {
                        //        groupinpos &= true;
                        //    }
                        //    else
                        //    {
                        //        groupinpos = false;
                        //    }
                        //}
                        //groupinpos = true;
                        for (int i = 0; i < axis.GroupMembers.Count; i++)
                        {
                            bool memberIsInpos = IsInposition(stReadBulkReadData[axis.GroupMembers[i].AxisIndex.Value].uiStatusRegister);
                            if (memberIsInpos)
                            {
                                groupinpos &= true;
                            }
                            else
                            {
                                groupinpos = false;
                            }
                        }


                        if (groupinpos == true)
                        {
                            val = true;
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            val = false;
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }

                    }
                }
                else
                {
                    val = false;
                    //retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                }
                //}
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"IsInposition({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.IsInpositionError;
                LoggerManager.Exception(err);

            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.IsInpositionError;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public int IsMotorEnabled(AxisObject axis, ref bool val)
        {
            mreSyncEvent.WaitOne(syncTimeOut_ms);
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            try
            {
                uint status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                if ((status >> 9 & 0x01) == 0x01)
                {
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    val = false;
                }
                else
                {
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    val = true;
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"IsMotorEnabled({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);

            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private int IsMotorEnabled(int axisIndex, ref bool val)
        {
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            try
            {
                //ushort statusWord = stReadBulkReadData[axisIndex].usStatusWord.usVar;

                //if ((statusWord >> 1 & 0x01) == 0x01)
                //{
                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //    val = true;
                //}
                //else
                //{
                //    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                //    val = false;
                //}


                uint statusWord = 0x0;
                var axis = MMCAxes[axisIndex];

                if (axis is MMCSingleAxis)
                {
                    statusWord = ((MMCSingleAxis)axis).ReadStatus();


                    if ((statusWord >> 9 & 0x01) == 0x01)
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        val = false;
                    }
                    else
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        val = true;
                    }
                }
                else if (axis is MMCGroupAxis)
                {
                    statusWord = ((MMCGroupAxis)axis).GroupReadStatus();


                    if ((statusWord >> 16 & 0x01) == 0x01)
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        val = false;
                    }
                    else
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        val = true;
                    }
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"IsMotorEnabled({axisIndex}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);

            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }


            return retVal;

        }
        private int IsGroupEnabled(uint status, ref bool val)
        {
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            try
            {

                if ((status >> 16 & 0x01) == 0x01)
                {
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    val = false;
                }
                else
                {
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    val = true;
                }
            }

            catch (MMCException err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);

            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }

            return retVal;

        }
        private int IsSingleAxisEnabled(uint status, ref bool val)
        {
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            try
            {

                if ((status >> 9 & 0x01) == 0x01)
                {
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    val = false;
                }
                else
                {
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                    val = true;
                }
            }

            catch (MMCException err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);

            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }

            return retVal;

        }
        public int IsMoving(AxisObject axis, ref bool val)
        {
            //mreSyncEvent.WaitOne(syncTimeOut_ms);
            //int nodenum = 0;
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;

            EnumAxisState tmpstr = EnumAxisState.ERROR;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    uint status;

                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                    }
                    else
                    {
                        status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();

                    }
                    tmpstr = GetServoState1(status);
                    if (tmpstr == EnumAxisState.IDLE)
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

                        val = false;
                    }
                    else
                    {
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        val = true;
                    }
                }
            }
            catch (MMCException err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);

            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private int QueryIsMoving(AxisObject axis, ref bool val)
        {
            //int nodenum = 0;
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;

            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.AxisIndex.Value), stw.ElapsedMilliseconds));
            EnumAxisState axisstate = EnumAxisState.ERROR;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    // Thread.Sleep(1);
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Read status Start {0}axis", axis.AxisIndex.Value), stw.ElapsedMilliseconds));
                    uint status = 0;
                    //mreSyncEvent.WaitOne(syncTimeOut_ms);
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        status = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).ReadStatus();
                        //status = stReadBulkReadData[axis.AxisIndex.Value].ulAxisStatus;
                        axisstate = GetServoState1(status);

                        if (axisstate == EnumAxisState.IDLE)
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            val = false;
                        }
                        else if (axisstate == EnumAxisState.ERROR)
                        {
                            retVal = (int)EnumMotionBaseReturnCode.FatalError;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            val = true;
                        }
                    }
                    else
                    {
                        //uint tmpstatus;
                        //bool isMoving = true;
                        //bool isInIdle = true;
                        //for (int i = 0; i < axis.GroupMembers.Count; i++)
                        //{
                        //    status = stReadBulkReadData[axis.GroupMembers[i].AxisIndex.Value].ulAxisStatus;
                        //    var singleAxisstate = GetServoState1(status);
                        //    if (singleAxisstate == EnumAxisState.IDLE)
                        //    {
                        //        isInIdle = isInIdle & true;
                        //    }
                        //    else if (singleAxisstate == EnumAxisState.ERROR)
                        //    {
                        //        throw new MotionException($"Axis error state while wait for motion done. Axis = {axis.Label.Value}");
                        //    }
                        //    else
                        //    {
                        //        isInIdle = false;
                        //    }
                        //}
                        //isMoving = !isInIdle;
                        //return isMoving;

                        status = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupReadStatus();
                        axisstate = GetServoState1(status);

                        if (axisstate == EnumAxisState.IDLE)
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            val = false;
                        }
                        else if (axisstate == EnumAxisState.ERROR)
                        {
                            retVal = (int)EnumMotionBaseReturnCode.FatalError;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            val = true;
                        }

                    }
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Read status End {0}axis", axis.AxisIndex.Value), stw.ElapsedMilliseconds));
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.AxisIndex.Value), stw.ElapsedMilliseconds));
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"QueryIsMoving({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private int GetMovingState(EnumAxisState state, ref bool val)
        {
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            try
            {

                switch (state)
                {
                    case EnumAxisState.INVALID:
                        val = false;
                        break;
                    case EnumAxisState.IDLE:
                        val = false;
                        break;
                    case EnumAxisState.MOVING:
                        val = true;
                        break;
                    case EnumAxisState.STOPPING:
                        val = true;
                        break;
                    case EnumAxisState.STOPPED:
                        val = false;
                        break;
                    case EnumAxisState.STOPPING_ERROR:
                        val = false;
                        break;
                    case EnumAxisState.ERROR:
                        val = false;
                        break;
                    case EnumAxisState.END:
                        val = false;
                        break;
                    default:
                        val = false;
                        break;
                }
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

            }
            catch (MMCException err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }
            catch (System.Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.FatalError;
                LoggerManager.Exception(err);
            }


            return retVal;
        }
        public int JogMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int JogMove(AxisObject axis, double velocity, double accel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int LinInterpolation(AxisObject[] axes, double[] poss)
        {
            throw new NotImplementedException();
        }

        public int Pause(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            double velocity = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    retVal = GetVelocity(axis, trjtype, ref velocity, ovrd);
                    ResultValidate(retVal);
                    RelMove(axis, pos, velocity, axis.Param.Acceleration.Value);
                    ResultValidate(retVal);
                }
            }
            catch (MMCException ex)
            {
                LoggerManager.Debug($"RelMove({axis.Label.Value}): MMCException occurred. Err. code = {ex.MMCError}");

                LoggerManager.Error($"RelMove() Function error axis = {axis.Label.Value}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                //LoggerManager.DebugError($"RelMove() Function error axis = {axis.Label.Value}: " + ex.Message);
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"RelMove() Function error axis = {axis.Label.Value}: " + ex.Message);
            }
            return retVal;
        }

        public int RelMove(AxisObject axis, double pos, double vel, double acc)
        {
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            if (pos > double.MaxValue)
            {

            }
            else if (pos < double.MaxValue)
            {

            }
            else
            {

            }
            stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Setting Start {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));

            //int pulse = 0;
            int retVal = (int)EnumMotionBaseReturnCode.RelMoveError;
            double targetPos;
            short axisIndex = (short)axis.AxisIndex.Value;
            //int nodenum = 0;

            double veltopulsecnt = 0.0;
            double acctopulsecnt = 0.0;
            double jerktopulsecnt = 0.0;

            //int cmdPos;
            //targetPos = axis.Status.APos + pos;
            //   GetCommandPulse(axis, out cmdPos);
            targetPos = axis.DtoP(pos);
            targetPos = Math.Round(targetPos);
            veltopulsecnt = (double)axis.DtoP(vel);
            acctopulsecnt = (double)axis.DtoP(acc);
            jerktopulsecnt = (double)axis.DtoP(axis.Param.AccelerationJerk.Value);

            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Setting End {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
            //MMCAxes[axis.AxisIndex.Value].MoveRelativeEx(targetPos, veltopulsecnt, acctopulsecnt, acctopulsecnt, jerktopulsecnt,
            //             MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
            //MMCAxes[axis.AxisIndex.Value].SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    // 251106 sebas SWLimit 제거
                    //if (targetPos + axis.Status.Pulse.Command > axis.DtoP(axis.Param.PosSWLimit.Value))
                    if (false)
                    {
                        LoggerManager.Error($"Positive SW Limit occurred while Relative moving for Axis { axis.Label.Value}, Target = { AxisStatusList[axis.AxisIndex.Value].Pulse.Command + targetPos}, Limit = { axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWPOSLimitError;
                    }
                    //else if (targetPos + axis.Status.Pulse.Command < axis.DtoP(axis.Param.NegSWLimit.Value))
                    else if (false)
                    {
                        LoggerManager.Error($"Negative SW Limit occurred while Relative moving for Axis { axis.Label.Value}, Target = { AxisStatusList[axis.AxisIndex.Value].Pulse.Command + targetPos}, Limit = { axis.Param.NegSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWNEGLimitError;
                    }
                    else
                    {
                        if (targetPos == 0)
                        {
                            LoggerManager.Debug($"RelMove() :skip the same position. axis = {axis.Label.Value}");
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Param Setting START {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));

                            MC_DIRECTION_ENUM dir = targetPos >= 0 ?
                                MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION : MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION;
                            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Param Setting START2 {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                            //MMCAxes[axis.AxisIndex.Value].ReadStatus();
                            //if (MMCAxes[axis.AxisIndex.Value].GetOpMode() == OPM402.OPM402_HOMING_MODE)
                            //{
                            //    MMCAxes[axis.AxisIndex.Value].SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                            //    while (MMCAxes[axis.AxisIndex.Value].GetOpMode() != OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE) { }
                            //}
                            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel  Param Setting END {0}axis", axis.Label), stw.ElapsedMilliseconds));
                            //MMCAxes[axis.AxisIndex.Value].MoveRelative(targetPos, (float)veltopulsecnt, (float)acctopulsecnt, (float)acctopulsecnt, (float)jerktopulsecnt, MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);

                            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Move Start {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));

                            if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                            {
                                //20251030 LJH 모터 ON 임시 코드
                                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).PowerOn(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                Thread.Sleep(3000);
                                // end

                                //20251030 LJH 모터 임시 코드
                                //retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelativeEx(targetPos, veltopulsecnt, acctopulsecnt, acctopulsecnt, jerktopulsecnt,
                                //dir, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);

                                // <-- 251106 sebas add
                                if(axis.AxisIndex.Value == 27)  // Ejection Z 축
                                {
                                    retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelativeEx(targetPos, 10000, 10000, 10000, 100000,
                                    dir, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                }
                                else if(axis.AxisIndex.Value == 13) // Nano Z 축
                                {
                                    retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelativeEx(targetPos, 4000000, 4000000, 4000000, 40000000,
                                    dir, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                }
                                else if(axis.AxisIndex.Value == 14) // FD Chuck Z축
                                {
                                    retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelativeEx(targetPos, 10000000, 10000000, 10000000, 100000000,
                                    dir, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                }
                                else if(axis.AxisIndex.Value == 15) // Base X 축
                                {
                                    retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelativeEx(targetPos, 1000000, 1000000, 1000000, 10000000,
                                    dir, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                }
                                else
                                {
                                    retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelativeEx(targetPos, 50000, 50000, 50000, 500000,
                                    dir, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                }
                                // -->

                                //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                                //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                                //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                                //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = acctopulsecnt;
                                //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;
                                //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Direction = targetPos >= 0 ?
                                //MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION : MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION;

                                //retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelative(targetPos, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                axis.Status.Pulse.Command += targetPos;
                                AxisStatusList[axis.AxisIndex.Value].Pulse.Command = axis.Status.Pulse.Command;

                                // 20251030 LJH 모터 OFF 임시 코드
                                Thread.Sleep(3000);
                                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).PowerOff(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                                Thread.Sleep(1500);
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                // end
                            }
                            else
                            {
                                double[] grouppos = new double[axis.GroupMembers.Count];

                                for (int i = 0; i < axis.GroupMembers.Count; i++)
                                {
                                    grouppos[i] = targetPos;
                                }

                                if (targetPos == 0)
                                {
                                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                }
                                else
                                {
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = acctopulsecnt;
                                    //((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = acctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = (double)axis.DtoP(axis.Param.Acceleration.Value) * 1.5;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;
                                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Prepare rel move parameters {0}axis: Command issued.", axis.Label.Value), stw.ElapsedMilliseconds));

                                    var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearRelativeEx(veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                                    //var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearRelative((float)veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                                    retVal = Convert.ToInt32(returnValue);
                                    retVal = 0;
                                    //AxisStatusList[axis.AxisIndex.Value].Pulse.Command = AxisStatusList[axis.AxisIndex.Value].Pulse.Command + targetPos;
                                    axis.Status.Pulse.Command += targetPos;
                                    AxisStatusList[axis.AxisIndex.Value].Pulse.Command = axis.Status.Pulse.Command;
                                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Move Start {0}axis: Command issued.", axis.Label.Value), stw.ElapsedMilliseconds));

                                }

                            }

                            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Move End {0}axis", axis.Label), stw.ElapsedMilliseconds));
                            // MMCAxes[axis.AxisIndex.Value].ReadStatus()
                            if (retVal != 0)
                            {
                                retVal = (int)EnumMotionBaseReturnCode.RelMoveError;
                            }
                            else
                            {
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                            }

                        }
                    }
                }
            }

            catch (MMCException err)
            {
                LoggerManager.Debug($"RelMove({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.RelMoveError;
                LoggerManager.Error($"Motion command failed while Relative moving for Axis =  {axis.Label.Value}, Target = {targetPos}" + err.Message);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.RelMoveError;
                LoggerManager.Error($"Motion command failed while Relative moving for Axis =  {axis.Label.Value}, Target = {targetPos}" + err.Message);
            }
            stw.Stop();
            return retVal;
        }

        public int RelMove(AxisObject axis, double pos, double vel, double acc, double dcc)
        {
            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Setting Start {0}axis", axis.Label), stw.ElapsedMilliseconds));

            //int pulse = 0;
            int retVal = -1;
            double targetPos;
            short axisIndex = (short)axis.AxisIndex.Value;
            //int nodenum = 0;

            double veltopulsecnt = 0.0;
            double acctopulsecnt = 0.0;
            double dcctopulsecnt = 0.0;
            double jerktopulsecnt = 0.0;

            //int cmdPos;
            //targetPos = axis.Status.APos + pos;
            //   GetCommandPulse(axis, out cmdPos);
            targetPos = axis.DtoP(pos);
            targetPos = Math.Round(targetPos);


            veltopulsecnt = (double)axis.DtoP(vel);

            acctopulsecnt = (double)axis.DtoP(acc);
            dcctopulsecnt = (double)axis.DtoP(dcc);
            jerktopulsecnt = (double)axis.DtoP(axis.Param.AccelerationJerk.Value);
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Rel Setting End {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //MMCAxes[axis.AxisIndex.Value].MoveRelativeEx(targetPos, veltopulsecnt, acctopulsecnt, acctopulsecnt, jerktopulsecnt,
            //             MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
            //MMCAxes[axis.AxisIndex.Value].SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (targetPos + axis.Status.Pulse.Command > axis.DtoP(axis.Param.PosSWLimit.Value))
                    {

                        targetPos = axis.Status.Pulse.Command + targetPos;
                        LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWPOSLimitError;
                    }
                    else if (targetPos + axis.Status.Pulse.Command < axis.DtoP(axis.Param.NegSWLimit.Value))
                    {
                        targetPos = axis.Status.Pulse.Command + targetPos;
                        LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label.Value}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                        return (int)EnumMotionBaseReturnCode.SWNEGLimitError;
                    }
                    else
                    {

                        MC_DIRECTION_ENUM dir = targetPos >= 0 ?
                            MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION : MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION;
                        if (targetPos == 0)
                        {
                            LoggerManager.Debug($"RelMove() :skip the same position. axis = {axis.Label.Value}");
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else
                        {
                            if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                            {
                                retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveRelativeEx(targetPos, veltopulsecnt, acctopulsecnt, acctopulsecnt, jerktopulsecnt,
                                dir, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                                axis.Status.Pulse.Command += targetPos;
                            }
                            else
                            {
                                double[] grouppos = new double[axis.GroupMembers.Count];

                                for (int i = 0; i < axis.GroupMembers.Count; i++)
                                {
                                    grouppos[i] = targetPos;
                                }

                                if (targetPos == 0)
                                {
                                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                }
                                else
                                {
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = dcctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = acctopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;

                                    var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearRelativeEx(veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                                    retVal = Convert.ToInt32(returnValue);

                                    //AxisStatusList[axis.AxisIndex.Value].Pulse.Command = AxisStatusList[axis.AxisIndex.Value].Pulse.Command + targetPos;
                                    axis.Status.Pulse.Command += targetPos;
                                    AxisStatusList[axis.AxisIndex.Value].Pulse.Command = axis.Status.Pulse.Command;
                                }

                            }
                            if (retVal != 0)
                            {
                                retVal = (int)EnumMotionBaseReturnCode.RelMoveError;
                            }
                        }
                    }
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"RelMove({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.RelMoveError;
                LoggerManager.Error($"Motion command failed while Relative moving for Axis =  {axis.Label.Value}, Target = {targetPos}" + err.Message);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.RelMoveError;

                LoggerManager.Error($"RelMove() Function error: " + err.Message);
            }
            //stw.Stop();
            return retVal;
        }

        public int ResultValidate(int retcode)
        {

            try
            {
                if ((EnumMotionBaseReturnCode)retcode != EnumMotionBaseReturnCode.ReturnCodeOK)
                {
                    LoggerManager.Error($"ResultValidate(): Err code = {retcode}, Description = {((EnumMotionBaseReturnCode)retcode).ToString()}");

                    throw new MotionException($"ResultValidate(): Err code = {retcode}, Description = {((EnumMotionBaseReturnCode)retcode).ToString()}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retcode));

                    //throw new MotionException($"ResultValidate(): Err code = {retcode}, Description = {((EnumMotionBaseReturnCode)retcode).ToString()}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retcode;
        }

        public int Resume(AxisObject axis)
        {
            return 0;
        }

        public int SetHWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return 0;
        }

        public int SetHWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return 0;
        }

        public int SetLimitSWNegAct(AxisObject axis, EnumEventActionType action)
        {
            return 0;
        }

        public int SetLimitSWPosAct(AxisObject axis, EnumEventActionType action)
        {
            return 0;
        }

        public int SetMotorAmpEnable(AxisObject axis, bool turnAmp)
        {
            return 0;
        }

        public int SetOverride(AxisObject axis, double ovrd)
        {
            return 0;
        }

        public int SetPosition(AxisObject axis, double pos)
        {
            int nRetVal = -1;

            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return nRetVal;
                //    }
                lock (axis)
                {
                    nRetVal = ElmoSetPostion(axis, pos);
                    ResultValidate(nRetVal);
                }
            }
            catch (System.Exception err)
            {
                nRetVal = (int)EnumMotionBaseReturnCode.SetPositionError;
                LoggerManager.Exception(err);
            }
            return nRetVal;
        }

        public int SetPulse(AxisObject axis, double pos)
        {
            return 0;
        }

        public int SetSwitchAction(AxisObject axis, EnumDedicateInputs input, EnumEventActionType action, EnumInputLevel reverse)
        {
            return 0;
        }

        public int SetSWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return 0;
        }

        public int SetSWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return 0;
        }

        public int SetZeroPosition(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    retVal = SetPosition(axis, 0);
                    ResultValidate(retVal);
                }
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.SetPositionError;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int Stop(AxisObject axis)
        {
            int retVal = -1;
            int pos = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {

                        if (EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions[axis.AxisIndex.Value].GroupType == EnumGroupType.MASTER ||
                            EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions[axis.AxisIndex.Value].GroupType == EnumGroupType.SLAVE)
                        {
                            //SyncMotion이기 때문에 MMC prohibit Error 발생 함
                            retVal = 0;
                        }
                        else
                        {
                            ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Stop((float)axis.Param.Decceleration.Value, (float)axis.Param.DeccelerationJerk.Value, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                            retVal = WaitForAxisMotionDone(axis, 6000);
                        }

                        if (retVal == 0)
                        {
                            //System.Threading.Thread.Sleep(1000);
                            GetCommandPulse(axis, ref pos);
                            AxisStatusList[axis.AxisIndex.Value].Pulse.Command = pos;
                        }
                    }
                    else
                    {
                        ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupStop((float)axis.Param.Decceleration.Value, (float)axis.Param.DeccelerationJerk.Value, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                        retVal = WaitForAxisMotionDone(axis, 6000);
                        if (retVal == 0)
                        {
                            //System.Threading.Thread.Sleep(1000);
                            //GetCommandPulse(axis, out pos);
                            //AxisStatusList[axis.AxisIndex.Value].Pulse.Command = pos;
                        }
                    }
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"Stop({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.StopError;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int VMoveStop(AxisObject axis)
        {
            int retVal = -1;
            int pos = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {

                        if (EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions[axis.AxisIndex.Value].GroupType == EnumGroupType.MASTER ||
                            EcatAxisDesc.ECATAxisDescripterParams.ECATNodeDefinitions[axis.AxisIndex.Value].GroupType == EnumGroupType.SLAVE)
                        {
                            //SyncMotion이기 때문에 MMC prohibit Error 발생 함
                            retVal = 0;
                        }
                        else
                        {
                            ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).Stop((float)axis.Param.Decceleration.Value, (float)axis.Param.DeccelerationJerk.Value, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                            //retVal = WaitForVMoveAxisMotionDone(axis, 6000);
                            retVal = 0;
                        }

                        if (retVal == 0)
                        {
                            //System.Threading.Thread.Sleep(1000);
                            GetCommandPulse(axis, ref pos);
                            AxisStatusList[axis.AxisIndex.Value].Pulse.Command = pos;
                        }
                    }
                    else
                    {
                        ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupStop((float)axis.Param.Decceleration.Value, (float)axis.Param.DeccelerationJerk.Value, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                        //retVal = WaitForAxisMotionDone(axis, 6000);
                        retVal = 0;

                        if (retVal == 0)
                        {
                            //System.Threading.Thread.Sleep(1000);
                            //GetCommandPulse(axis, out pos);
                            //AxisStatusList[axis.AxisIndex.Value].Pulse.Command = pos;
                        }
                    }
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"VMoveStop({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.StopError;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int VMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            double veltopulsecnt = 0.0;
            double acctopulsecnt = 0.0;
            double jerktopulsecnt = 0.0;
            double vel = 0.0;

            if (trjtype == EnumTrjType.Normal)
            {
                veltopulsecnt = (double)axis.DtoP(velocity);
            }
            else
            {
                GetVelocity(axis, trjtype, ref vel, ovrd);
                veltopulsecnt = (double)axis.DtoP(vel);
            }

            acctopulsecnt = (double)axis.DtoP(axis.Param.Acceleration.Value);

            jerktopulsecnt = (double)axis.DtoP(axis.Param.AccelerationJerk.Value);
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {


                        if (velocity >= 0)
                        {
                            retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveVelocity((float)veltopulsecnt, (float)acctopulsecnt, (float)acctopulsecnt, (float)jerktopulsecnt, MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                            //  MMCAxes[axis.AxisIndex.Value].Stop(MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                        }
                        else
                        {
                            veltopulsecnt *= -1;
                            retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveVelocity((float)veltopulsecnt, (float)acctopulsecnt, (float)acctopulsecnt, (float)jerktopulsecnt, MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                        }
                    }
                    else
                    {
                        double[] grouppos = new double[axis.GroupMembers.Count];
                        double totaldist = Math.Abs(axis.Param.NegSWLimit.Value) + Math.Abs(axis.Param.PosSWLimit.Value);

                        if (velocity >= 0)
                        {
                            for (int i = 0; i < axis.GroupMembers.Count; i++)
                            {
                                grouppos[i] = axis.DtoP(totaldist);
                            }

                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;

                            var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearRelativeEx(veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                            retVal = 0;

                        }
                        else
                        {
                            for (int i = 0; i < axis.GroupMembers.Count; i++)
                            {
                                grouppos[i] = axis.DtoP(totaldist) * -1d;
                            }

                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;

                            var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearRelativeEx(veltopulsecnt, grouppos, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                            retVal = 0;
                        }
                    }
                }

            }
            catch (MMCException err)
            {
                if ((int)err.MMCError == -218)
                {
                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                }
                else
                {
                    VMoveStop(axis);
                    LoggerManager.Debug($"Vmove({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                    retVal = (int)EnumMotionBaseReturnCode.VelocityMoveError;
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.VelocityMoveError;
                LoggerManager.Exception(err);
            }
            return retVal;

        }
        public int VMove(AxisObject axis, double velocity, double acc, double dcc, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            double veltopulsecnt = 0.0;
            double acctopulsecnt = 0.0;
            double dcctopulsecnt = 0.0;
            double jerktopulsecnt = 0.0;
            double vel = 0.0;
            if (trjtype == EnumTrjType.Normal)
            {
                veltopulsecnt = (double)axis.DtoP(velocity);
            }
            else
            {
                GetVelocity(axis, trjtype, ref vel, ovrd);
                veltopulsecnt = (double)axis.DtoP(vel);
            }
            acctopulsecnt = (double)axis.DtoP(acc);
            dcctopulsecnt = (double)axis.DtoP(dcc);
            jerktopulsecnt = (double)axis.DtoP(dcc);
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }
                lock (axis)
                {
                    if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                    {
                        if (velocity >= 0)
                        {
                            //  MMCAxes[axis.AxisIndex.Value].Stop(MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                            retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveVelocity((float)veltopulsecnt, (float)acctopulsecnt, (float)dcctopulsecnt, (float)jerktopulsecnt, MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                        }
                        else
                        {
                            veltopulsecnt *= -1;
                            retVal = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).MoveVelocity((float)veltopulsecnt, (float)acctopulsecnt, (float)dcctopulsecnt, (float)jerktopulsecnt, MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);

                        }
                    }
                    else
                    {
                        double[] grouppos = new double[axis.GroupMembers.Count];
                        double totaldist = Math.Abs(axis.Param.NegSWLimit.Value) + Math.Abs(axis.Param.PosSWLimit.Value);

                        if (velocity >= 0)
                        {
                            for (int i = 0; i < axis.GroupMembers.Count; i++)
                            {
                                grouppos[i] = axis.DtoP(totaldist);
                            }

                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = dcctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = axis.DtoP(axis.Param.Decceleration.Value) * 1.5;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;



                            var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearRelativeEx(axis.Param.HommingSpeed.Value, grouppos, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                            retVal = 0;

                        }
                        else
                        {
                            for (int i = 0; i < axis.GroupMembers.Count; i++)
                            {
                                grouppos[i] = axis.DtoP(totaldist) * -1d;
                            }

                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Execute = 1;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Acceleration = acctopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Deceleration = dcctopulsecnt;
                            //((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).StopDeceleration = axis.DtoP(axis.Param.Decceleration.Value) * 1.5;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Velocity = veltopulsecnt;
                            ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).Jerk = jerktopulsecnt;



                            var returnValue = ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).MoveLinearRelativeEx(axis.DtoP(axis.Param.HommingSpeed.Value), grouppos, MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);
                            retVal = 0;
                        }
                    }

                }
            }
            catch (MMCException err)
            {
                VMoveStop(axis);
                retVal = (int)EnumMotionBaseReturnCode.VelocityMoveError;
                LoggerManager.Debug($"VMove({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.VelocityMoveError;
                LoggerManager.Exception(err);
            }
            return retVal;

        }
        public int WaitForAxisEvent(AxisObject axis, EnumAxisState waitfor, double distlimit, long timeout = -1)
        {
            return 0;
        }

        private double CalcAbsDistance(double commandpos, double actualpos)
        {
            double retVal = 0.0;
            try
            {
                //커맨드 pause가 양수이면서 액쳘이 양수일때
                if (commandpos > 0 && actualpos > 0)
                {
                    retVal = Math.Abs(Math.Abs(commandpos) - Math.Abs(actualpos));
                }
                //커맨드가 양수이고 액쳘이 음수
                else if (commandpos > 0 && actualpos < 0)
                {
                    retVal = Math.Abs(Math.Abs(commandpos) + Math.Abs(actualpos));
                }
                //커맨드가 음수이고 액쳘이 양수
                else if (commandpos > 0 && actualpos < 0)
                {
                    retVal = Math.Abs(Math.Abs(commandpos) + Math.Abs(actualpos));
                }
                //커맨드가 음수이고 액쳘이 음수일때
                else if (commandpos > 0 && actualpos < 0)
                {
                    retVal = Math.Abs(Math.Abs(commandpos) - Math.Abs(actualpos));
                }
                else
                {
                    retVal = Math.Abs(Math.Abs(commandpos) - Math.Abs(actualpos));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private int GetErrorPosition(AxisObject axis, ref double errorpos)
        {
            int retVal = -1;
            double errPosition = 0.0;
            try
            {

                //m_BulkRead.Perform();
                //stReadBulkReadData = m_BulkRead.Preset_4;
                //var actualPulse = ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).GetActualPosition();
                mreSyncEvent.WaitOne(syncTimeOut_ms);
                errPosition = (int)stReadBulkReadData[axis.AxisIndex.Value].iPosFollowingErr;
                errorpos = axis.PtoD(errPosition);
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"GetErrorPosition({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");

                retVal = (int)EnumMotionBaseReturnCode.GetErrorPositionError;
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.GetErrorPositionError;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public int WaitForVMoveAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            int retVal = -1;

            bool isAxisbusy = true;
            //int cnt = 0;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            bool isinpos = false;
            bool overrideflag = false;
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            uint ustatus = 0;
            stw.Restart();
            stw.Start();
            try
            {
                bool runFlag = true;
                EnumAxisState axisState = EnumAxisState.INVALID;
                do
                {
                    if (axis.Param.FeedOverride.Value == 0)
                    {
                        timeout = 0;
                        overrideflag = true;
                    }
                    else
                    {
                        if (overrideflag == true)
                        {
                            elapsedStopWatch.Reset();
                            elapsedStopWatch.Start();
                        }
                        overrideflag = false;

                    }

                    retVal = IsInposition(axis, ref isinpos);
                    ResultValidate(retVal);
                    isAxisbusy = !isinpos;

                    retVal = GetElmoAxisStatus(axis, ref ustatus);
                    ResultValidate(retVal);
                    axisState = GetServoState1(ustatus);

                    if (isinpos == false || isAxisbusy == true || axisState != EnumAxisState.IDLE)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {

                                LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                DisableAxis(axis);
                            }
                        }
                        else
                        {
                            runFlag = true;
                        }
                        if (axisState == EnumAxisState.ERROR)
                        {
                            runFlag = false;
                            retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                            throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for motion done. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }
                    }
                    else
                    {
                        if (axis.Status.IsHoming == true)
                        {
                            retVal = IsInposition(axis, ref isinpos);
                            ResultValidate(retVal);
                        }
                        if (isinpos == true)
                        {

                            if (axisState == EnumAxisState.IDLE)
                            {
                                if (axis.Status.IsHoming == true || axis.Config.MoterConfig.MotorType.Value == EnumMoterType.STEPPER)
                                {
                                    runFlag = false;
                                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                }
                                else
                                {
                                    runFlag = false;
                                    double errordistance = 0.0;
                                    retVal = GetErrorPosition(axis, ref errordistance);
                                    ResultValidate(retVal);
                                    if (axis.Config.MoterConfig.ErrorLimitTrigger.Value < errordistance)
                                    {
                                        retVal = GetErrorPosition(axis, ref errordistance);
                                        ResultValidate(retVal);
                                        if (axis.Config.MoterConfig.ErrorLimitTrigger.Value < errordistance)
                                        {
                                            retVal = (int)EnumMotionBaseReturnCode.ErrorPositionLimitError;
                                        }
                                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                    }
                                    else
                                    {
                                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                    }
                                }
                            }
                            else
                            {
                                Stop(axis);
                                retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                                throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for motion done. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                            }
                        }
                    }
                } while (runFlag == true);

                if (axis.SettlingTime > 0)
                {
                    Thread.Sleep((int)axis.SettlingTime / 2);
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"WaitforMotiondone({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.WaitforMotionDoneError;
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.WaitforMotionDoneError;
                LoggerManager.Exception(err);
            }
            finally
            {
                elapsedStopWatch?.Stop();
            }
            stw.Stop();
            return retVal;
        }
        public int WaitForAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            int retVal = -1;

            bool isAxisbusy = true;
            //int cnt = 0;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            bool isinpos = false;
            bool overrideflag = false;
            int limitTimeout = 600000;
            int overrideLimitTimeOut = 1200000;
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            uint ustatus = 0;
            stw.Restart();
            stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label.Value}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                bool runFlag = true;
                EnumAxisState axisState = EnumAxisState.INVALID;
                //mreSyncEvent.WaitOne(syncTimeOut_ms);
                //axis.Status.AxisBusy = QueryIsMoving(axis);
                //axis.Status.Inposition = IsInposition(axis);
                //mreUpdateEvent.WaitOne(1);
                //GetAxisStatus(axis);
                //axis.Status.AxisBusy = DirectIsMoving(axis.AxisIndex.Value);
                //axis.Status.AxisEnabled = IsMotorEnabled(axis);
                //axis.Status.Inposition = IsInposition(axis);
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("First End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                //mreSyncEvent.WaitOne(syncTimeOut_ms);
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter status loop for {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                do
                {

                    if (axis.Param.FeedOverride.Value == 0)
                    {
                        timeout = 0;
                        overrideflag = true;
                    }
                    else
                    {
                        //if (overrideflag == true)
                        //{
                        //    elapsedStopWatch.Reset();
                        //    elapsedStopWatch.Start();
                        //}
                        overrideflag = false;
                    }

                    //mreSyncEvent.WaitOne(syncTimeOut_ms);
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Retreive IsInposition {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                    retVal = IsInposition(axis, ref isinpos);
                    ResultValidate(retVal);
                    isAxisbusy = !isinpos;

                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Retreive QueryIsMoving {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                    //retVal = QueryIsMoving(axis, ref isAxisbusy);
                    //ResultValidate(retVal);
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Retreive GetElmoAxisStatus {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                    retVal = GetElmoAxisStatus(axis, ref ustatus);
                    ResultValidate(retVal);
                    axisState = GetServoState1(ustatus);
                    //axis.Status.AxisEnabled = IsMotorEnabled(axis);
                    int actpulse = 0;
                    int cmdpulse = 0;
                    int subpulse = 0;

                    if (isinpos == false || isAxisbusy == true || axisState != EnumAxisState.IDLE)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {

                                LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                if(axis.GroupMembers.Count > 0)
                                {
                                    for (int i = 0; i < axis.GroupMembers.Count; i++)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                        DisableAxis(axis.GroupMembers[i]);
                                    }
                                }
                                DisableAxis(axis);
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                            }
                        }
                        else
                        {
                            runFlag = true;
                            if (timeout == 0 && overrideflag == false)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > limitTimeout)
                                {
                                    LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {limitTimeout}ms");

                                    runFlag = false;
                                    if (axis.GroupMembers.Count > 0)
                                    {
                                        for (int i = 0; i < axis.GroupMembers.Count; i++)
                                        {
                                            System.Threading.Thread.Sleep(500);
                                            DisableAxis(axis.GroupMembers[i]);
                                        }
                                    }
                                    DisableAxis(axis);
                                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                }
                            }

                            if (overrideflag == true)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > overrideLimitTimeOut)
                                {
                                    LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {overrideLimitTimeOut}ms");

                                    runFlag = false;
                                    DisableAxis(axis);
                                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                }
                            }
                        }
                        if (axisState == EnumAxisState.ERROR)
                        {
                            runFlag = false;
                            retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                            throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for motion done. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }
                        if (axisState == EnumAxisState.DISABLED)
                        {
                            runFlag = false;
                            retVal = (int)EnumMotionBaseReturnCode.WaitforMotionDoneError;
                            throw new MotionException($"{axis.Label.Value} Axis disabled while wait for motion done. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }

                    }
                    else
                    {
                        if (axis.Status.IsHoming == true)
                        {
                            retVal = IsInposition(axis, ref isinpos);
                            ResultValidate(retVal);
                        }
                        if (isinpos == true)
                        {
                            GetCommandPulse(axis, ref cmdpulse);
                            GetActualPulse(axis, ref actpulse);
                            if (axis.GroupMembers.Count > 0)
                            {
                                subpulse = cmdpulse - actpulse;
                                if (Math.Abs(subpulse) > axis.DtoP(axis.Config.Inposition.Value))
                                {
                                    subpulse = cmdpulse - actpulse + Convert.ToInt32(axis.DtoP(axis.GroupMembers[0].Status.CompValue));
                                }
                            }
                            else
                            {
                                subpulse = cmdpulse - actpulse;

                            }
                            subpulse = Math.Abs(subpulse);
                            if (subpulse > axis.DtoP(axis.Config.Inposition.Value))
                            {
                                if (axisState == EnumAxisState.IDLE && axis.Status.IsHoming == true
                                    || axis.Config.MoterConfig.MotorType.Value == EnumMoterType.STEPPER)
                                {
                                    runFlag = false;
                                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                }
                                else
                                {
                                    //LoggerManager.Debug($"WaitForAxisMotionDone(Axis:{axis.Label.Value}): Position out of window. Wait for inposition window lock in. CMD = {cmdpulse}, ACT = {actpulse}.");
                                    if (timeout != 0)
                                    {
                                        if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                                        {

                                            LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                            runFlag = false;
                                            if (axis.GroupMembers.Count > 0)
                                            {
                                                for (int i = 0; i < axis.GroupMembers.Count; i++)
                                                {
                                                    System.Threading.Thread.Sleep(500);
                                                    DisableAxis(axis.GroupMembers[i]);
                                                }
                                            }
                                            DisableAxis(axis);
                                            retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                        }
                                    }
                                    else
                                    {
                                        runFlag = true;
                                        if (timeout == 0 && overrideflag == false)
                                        {
                                            if (elapsedStopWatch.ElapsedMilliseconds > limitTimeout)
                                            {
                                                LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {limitTimeout}ms");

                                                runFlag = false;
                                                if (axis.GroupMembers.Count > 0)
                                                {
                                                    for (int i = 0; i < axis.GroupMembers.Count; i++)
                                                    {
                                                        System.Threading.Thread.Sleep(500);
                                                        DisableAxis(axis.GroupMembers[i]);
                                                    }
                                                }
                                                DisableAxis(axis);
                                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                            }
                                        }

                                        if (overrideflag == true)
                                        {
                                            if (elapsedStopWatch.ElapsedMilliseconds > overrideLimitTimeOut)
                                            {
                                                LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {overrideLimitTimeOut}ms");

                                                runFlag = false;
                                                DisableAxis(axis);
                                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (axisState == EnumAxisState.IDLE)
                            {
                                if (axis.Status.IsHoming == true || axis.Config.MoterConfig.MotorType.Value == EnumMoterType.STEPPER)
                                {
                                    runFlag = false;
                                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                }
                                else
                                {
                                    runFlag = false;
                                    double errordistance = 0.0;
                                    retVal = GetErrorPosition(axis, ref errordistance);
                                    ResultValidate(retVal);
                                    if (axis.Config.MoterConfig.ErrorLimitTrigger.Value < errordistance)
                                    {
                                        retVal = GetErrorPosition(axis, ref errordistance);
                                        ResultValidate(retVal);
                                        if (axis.Config.MoterConfig.ErrorLimitTrigger.Value < errordistance)
                                        {
                                            retVal = (int)EnumMotionBaseReturnCode.ErrorPositionLimitError;
                                        }
                                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                    }
                                    else
                                    {
                                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                    }
                                    //LoggerManager.Debug($"WaitForAxisMotionDone({axis.Label.Value}) : Axis motion done. Position is {axis.Status.Position.Actual}");
                                }
                            }
                            else
                            {
                                if (axis.GroupMembers.Count > 0)
                                {
                                    for (int i = 0; i < axis.GroupMembers.Count; i++)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                        DisableAxis(axis.GroupMembers[i]);
                                    }
                                }
                                DisableAxis(axis);
                                retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                                throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for motion done. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                                //LoggerManager.DebugError($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion error occurred. Err code = {axis.Status.ErrCode}");
                            }
                        }
                    }
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetAxisStatus {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                } while (runFlag == true);
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Settling.... {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));

                //Thread.Sleep(20);
                if (axis.SettlingTime > 0)
                {
                    Thread.Sleep((int)axis.SettlingTime / 2);
                }
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                //}
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"WaitforMotiondone({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.WaitforMotionDoneError;
                //LoggerManager.Error($string.Format("WaitForAxisMotionDone() Function error: {0}" + err.Message));
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                retVal = (int)EnumMotionBaseReturnCode.WaitforMotionDoneError;
                //LoggerManager.Error($string.Format("WaitForAxisMotionDone() Function error: {0}" + err.Message));
                LoggerManager.Exception(err);
            }
            finally
            {
                elapsedStopWatch?.Stop();

                //double pos=0;
                // GetCmdPosition(axis, ref pos);
                // axis.Status.Position.Ref = pos;
            }
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
            stw.Stop();
            //int step = 0;
            //foreach (var item in timeStamp)
            //{
            //    step++;
            //    LoggerManager.Debug($"Time Stamp [{step}] - Desc: {item.Key}, Time: {item.Value}");
            //}
            //LoggerManager.Debug($"{axis.Label.Value} Axis motion done. Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
            return retVal;
        }

        public int WaitForHomeSensor(AxisObject axis, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public double GetAccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            double acc = 0.0;
            double pulse = 0;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return 0;
                //    }

                switch (trjtype)
                {
                    case EnumTrjType.Normal:
                        acc = axis.Param.Acceleration.Value;
                        break;
                    case EnumTrjType.Probing:
                        acc = axis.Param.SeqAcc.Value;
                        break;
                    case EnumTrjType.Homming:
                        pulse = axis.Param.HommingAcceleration.Value;
                        acc = pulse;
                        break;
                    case EnumTrjType.Emergency:
                        acc = axis.Param.Acceleration.Value * 1.5;
                        break;
                    default:
                        break;
                }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAccel() Function error: " + err.Message);
            }
            if (ovrd < 0.1)
            {
                ovrd = 0.1;
            }
            return acc * ovrd;
        }

        public double GetDeccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            int retVal = -1;
            Stopwatch elapsedStopWatch = new Stopwatch();
            bool isinpos = false;
            bool isAxisbusy = true;
            EnumAxisState axisState = EnumAxisState.INVALID;
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            bool overrideflag = false;
            int limitTimeout = 600000;
            int overrideLimitTimeOut = 3600000;
            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();

            stw.Restart();
            stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                bool runFlag = true;
                bool state = false;
                bool isEnter = false;
                uint ustatus = 0;
                //mreUpdateEvent.WaitOne(1);
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("First Enter GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                //GetAxisStatus(axis);

                retVal = IsInposition(axis, ref isinpos);
                ResultValidate(retVal);
                isAxisbusy = !isinpos;

                //retVal = QueryIsMoving(axis, ref isAxisbusy);
                //ResultValidate(retVal);
                retVal = GetElmoAxisStatus(axis, ref ustatus);
                ResultValidate(retVal);
                axisState = GetServoState1(ustatus);
                int actpulse = 0;
                int cmdpulse = 0;
                int subpulse = 0;
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("First End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                do
                {
                    state = GetSourceLevel();
                    if (isEnter && (state == resumeLevel))
                    {
                        retVal = SetFeedrate(axis, 1, 1);
                        ResultValidate(retVal);
                        //Resume(axis);
                        isEnter = false;
                    }
                    else if (!isEnter && (!state == resumeLevel))
                    {
                        //Stop(axis);
                        retVal = SetFeedrate(axis, 0, 0);
                        ResultValidate(retVal);
                        isEnter = true;
                        elapsedStopWatch.Reset();
                        elapsedStopWatch.Start();
                    }


                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                    if (axis.Param.FeedOverride.Value == 0)
                    {
                        timeout = 0;
                        overrideflag = true;
                    }
                    else
                    {
                        //if (overrideflag == true)
                        //{
                        //    elapsedStopWatch.Reset();
                        //    elapsedStopWatch.Start();
                        //}
                        overrideflag = false;
                    }

                    retVal = IsInposition(axis, ref isinpos);
                    ResultValidate(retVal);
                    isAxisbusy = !isinpos;

                    //retVal = QueryIsMoving(axis, ref isAxisbusy);
                    //ResultValidate(retVal);
                    retVal = GetElmoAxisStatus(axis, ref ustatus);
                    ResultValidate(retVal);
                    axisState = GetServoState1(ustatus);

                    if (isinpos == false || isAxisbusy == true || axisState != EnumAxisState.IDLE)
                    {

                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                DisableAxis(axis);
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                            }
                        }
                        else
                        {
                            runFlag = true;
                            if (timeout == 0 && overrideflag == false)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > limitTimeout)
                                {
                                    LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {limitTimeout}ms");

                                    runFlag = false;
                                    DisableAxis(axis);
                                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                }
                            }

                            if (overrideflag == true)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > overrideLimitTimeOut)
                                {
                                    LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {overrideLimitTimeOut}ms");

                                    runFlag = false;
                                    DisableAxis(axis);
                                    retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                }
                            }
                        }
                        if (axisState == EnumAxisState.ERROR)
                        {
                            runFlag = false;
                            retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                            throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for motion done. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }
                    }
                    else
                    {
                        retVal = IsInposition(axis, ref isinpos);
                        ResultValidate(retVal);
                        if (isinpos == true)
                        {
                            GetCommandPulse(axis, ref cmdpulse);
                            GetActualPulse(axis, ref actpulse);
                            subpulse = cmdpulse - actpulse;
                            subpulse = Math.Abs(subpulse);

                            if (subpulse > axis.DtoP(axis.Config.Inposition.Value))
                            {
                                if (axisState == EnumAxisState.IDLE && axis.Status.IsHoming == true)
                                {
                                    runFlag = false;
                                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                }
                                else
                                {
                                    LoggerManager.Debug($"WaitForAxisMotionDone(Axis:{axis.Label.Value}): Position out of window. Wait for inposition window lock in. CMD = {cmdpulse}, ACT = {actpulse}.");
                                }
                            }

                            else if (axisState == EnumAxisState.IDLE)
                            {
                                if (axis.Status.IsHoming == true || axis.Config.MoterConfig.MotorType.Value == EnumMoterType.STEPPER)
                                {
                                    runFlag = false;
                                    retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                }
                                else
                                {
                                    runFlag = false;
                                    double errorpos = 0.0;
                                    retVal = GetErrorPosition(axis, ref errorpos);
                                    ResultValidate(retVal);
                                    if (axis.Config.MoterConfig.ErrorLimitTrigger.Value < errorpos)
                                    {
                                        retVal = GetErrorPosition(axis, ref errorpos);
                                        ResultValidate(retVal);
                                        if (axis.Config.MoterConfig.ErrorLimitTrigger.Value < errorpos)
                                        {
                                            retVal = (int)EnumMotionBaseReturnCode.ErrorPositionLimitError;
                                        }
                                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

                                    }
                                    else
                                    {
                                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                    }
                                    //LoggerManager.Debug($"WaitForAxisMotionDone({axis.Label.Value}) : Axis motion done. Position is {axis.Status.Position.Actual}");
                                }
                            }
                            else
                            {
                                DisableAxis(axis);
                                retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                                throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for motion done. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                                //LoggerManager.DebugError($"WaitForAxisMotionDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion error occurred. Err code = {axis.Status.ErrCode}");
                            }
                        }
                    }
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                } while (runFlag == true);

                //}
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"WaitforMotiondone({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                retVal = (int)EnumMotionBaseReturnCode.WaitforMotionDoneError;
                //LoggerManager.Error($string.Format("WaitForAxisMotionDone() Function error: {0}" + err.Message));
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WaitForAxisMotionDone() Function error: {0}" + err.Message));
                retVal = (int)EnumMotionBaseReturnCode.WaitforMotionDoneError;
                LoggerManager.Exception(err);
                //  throw new MotionException(string.Format("WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
            }
            finally
            {
                elapsedStopWatch?.Stop();

                //double pos=0;
                // GetCmdPosition(axis, ref pos);
                // axis.Status.Position.Ref = pos;
            }
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Settling.... {0}axis", axis.Label), stw.ElapsedMilliseconds));

            if (axis.SettlingTime > 0)
            {
                Thread.Sleep((int)axis.SettlingTime);
            }
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //stw.Stop();
            //int step = 0;
            //foreach (var item in timeStamp)
            //{
            //    step++;
            //    LoggerManager.Debug($string.Format("Time Stamp [{0}] - Desc: {1}, Time: {2}", step, item.Key, item.Value));
            //}
            return retVal;
        }
        public int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0, long settlingtime = 0)
        {
            int retVal = -1;
            try
            {
                //using (Locker locker = new Locker(axis, $"Axis Lock - {axis.Label}"))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return retVal;
                //    }

                retVal = WaitForAxisMotionDone(axis, GetSourceLevel, resumeLevel, timeout);

                Thread.Sleep((int)settlingtime);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public int SetSettlingTime(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int SetFeedrate(AxisObject axis, double normfeedrate, double pausefeedrate)
        {
            int retVal = -1;
            try
            {
                float overridefeedrate = (float)normfeedrate;
                ((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).SetOverride(overridefeedrate, overridefeedrate, overridefeedrate, 0);
                axis.Param.FeedOverride.Value = overridefeedrate;
                retVal = 0;
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"SetFeedrate({axis.Label.Value}): MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        class SlidingBuffer<T> : IEnumerable<T>
        {
            private readonly Queue<T> _queue;
            private readonly int _maxCount;

            public SlidingBuffer(int maxCount)
            {
                try
                {
                    _maxCount = maxCount;
                    _queue = new Queue<T>(maxCount);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            public void Add(T item)
            {
                try
                {
                    if (_queue.Count == _maxCount)
                        _queue.Dequeue();
                    _queue.Enqueue(item);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                try
                {
                    return _queue.GetEnumerator();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                try
                {
                    return GetEnumerator();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
        }

        //private double GetCommandPluse(int axisIndex)
        //{
        //    int actpospulse=0;
        //    //MMCAxes[axisIndex].UploadSDO(0x607A, 0, out actpospulse, 500);
        //    return Convert.ToDouble(actpospulse);
        //}
        private int GetAuxPos(int axisindex, ref int auxpos)
        {
            int retVal = -1;

            try
            {
                ((MMCSingleAxis)MMCAxes[axisindex]).UploadSDO(0x20A0, 0, out auxpos, 500);
                retVal = 0;
            }
            catch (MMCException ex)
            {
                LoggerManager.Debug($"Exception GetAuxPos function Axisindex: {axisindex}, msg:{ex.Message}");
            }
            return retVal;
        }
        private void UpdateElmoBaseProc()
        {
            long elapsedTime;
            long loopTime;
            //double meanTime;
            bool ismoving = false;
            bool isenabled = false;
            //bool isinposition = false;
            bool ishSensor = false;
            bool islSensor = false;
            //int auxpos = 0;
            int retVal = (int)EnumMotionBaseReturnCode.FatalError;
            Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            stw.Start();

            SlidingBuffer<long> loopTimeBuff = new SlidingBuffer<long>(10000);
            try
            {
                stw.Start();
                m_BulkRead.Perform();

                stReadBulkReadData = m_BulkRead.Preset_4;
                //for (int i = 0; i < AxisStatusList.Count; i++)
                //{
                //    int actpospulse;
                //    GetCommandPulse(i, out actpospulse);
                //    AxisStatusList[i].Pulse.Command = actpospulse;
                //}

                elapsedTime = stw.ElapsedMilliseconds;

                while (bStopUpdateThread == false)
                {
                    mreSyncEvent.Reset();
                    //loopTime = stw.ElapsedMilliseconds;
                    //if (loopTime > long.MaxValue / 2)
                    //{
                    //    stw.Reset();
                    //    stw.Start();
                    //    loopTime = 0;
                    //}
                    //stw.Reset();
                    //stw.Start();
                    //_monitoringTimer.Interval = 4;
                    Thread.Sleep(2);
                    if (DevConnected == true)
                    {

                        elapsedTime = stw.ElapsedMilliseconds;
                        //using (Locker locker = new Locker(bulkReadBufferLock))
                        //{

                        //    if (locker.AcquiredLock == false)
                        //    {
                        //        System.Diagnostics.Debugger.Break();
                        //        return;
                        //    }
                        //    m_BulkRead.Perform();

                        //    stReadBulkReadData = m_BulkRead.Preset_4;
                        //}
                        m_BulkRead.Perform();
                        lock (stReadBulkReadData)
                        {
                            stReadBulkReadData = m_BulkRead.Preset_4;
                        }
                        loopTime = stw.ElapsedMilliseconds - elapsedTime;
                        UpdateProcTime = loopTime;
                        if (loopTime > previewSyncTimeOut)
                        {
                            //LoggerManager.Debug($"Delayed bulkread time. Elapsed time is {loopTime}ms");
                        }
                        //elapsedTime = stw.ElapsedMilliseconds - loopTime;
                        Parallel.For(0, AxisStatusList.Count, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, i =>
                          {
                              if (MMCAxes[i] is MMCGroupAxis)
                              {
                                  //double groupActulpos = 0;
                                  var status = stReadBulkReadData[i].ulAxisStatus;
                                  var statusRegister = stReadBulkReadData[i].uiStatusRegister;
                                  //GetGroupActualPosition(i, out groupActulpos);
                                  //var status = ((MMCGroupAxis)MMCAxes[i]).GroupReadStatus();
                                  AxisStatusList[i].State = GetServoState1(status);
                                  retVal = GetMovingState(AxisStatusList[i].State, ref ismoving);
                                  ResultValidate(retVal);
                                  AxisStatusList[i].AxisBusy = ismoving;
                                  AxisStatusList[i].Inposition = IsInposition(statusRegister);

                                  retVal = IsGroupEnabled(status, ref isenabled);
                                  ResultValidate(retVal);
                                  AxisStatusList[i].AxisEnabled = isenabled;
                                  //AxisStatusList[i].Pulse.Actual = groupActulpos;
                                  AxisStatusList[i].Pulse.Actual = stReadBulkReadData[i].aPos;
                                  AxisStatusList[i].Torque = stReadBulkReadData[i].aTorque;
                              }
                              else
                              {
                                  AxisStatusList[i].Torque = stReadBulkReadData[i].aTorque;
                                  AxisStatusList[i].Pulse.Actual = stReadBulkReadData[i].aPos;
                                  AxisStatusList[i].Pulse.Error = stReadBulkReadData[i].iPosFollowingErr;
                                  AxisStatusList[i].State = GetServoState1(stReadBulkReadData[i].ulAxisStatus);
                                  AxisStatusList[i].StatusCode = (int)stReadBulkReadData[i].ulAxisStatus;
                                  retVal = GetMovingState(AxisStatusList[i].State, ref ismoving);
                                  ResultValidate(retVal);
                                  AxisStatusList[i].AxisBusy = ismoving;
                                  //retVal = IsMotorEnabled(i, ref isenabled);
                                  retVal = IsSingleAxisEnabled(stReadBulkReadData[i].ulAxisStatus, ref isenabled);
                                  ResultValidate(retVal);

                                  AxisStatusList[i].AxisEnabled = isenabled;
                                  AxisStatusList[i].Inposition = IsInposition(stReadBulkReadData[i].uiStatusRegister);

                                  IsHomeSensor(i, ref ishSensor);
                                  AxisStatusList[i].IsHomeSensor = ishSensor;
                                  IsLimitSensor(i, ref islSensor);
                                  AxisStatusList[i].IsLimitSensor = islSensor;
                                  //GetAuxPos(i, ref auxpos);
                                  //AxisStatusList[i].AuxPosition = auxpos;
                                  //GetAxisStatus(i, AxisStatusList[i]);
                                  //timeStamp.Add(new KeyValuePair<string, long>(string.Format("DirectIsMoving End {0}axis", i), stw.ElapsedMilliseconds));
                                  //GetACEPosition(i, AxisStatusList[i]);
                                  //AxisStatusList[i].NOTSensor = GetIONegLim(i);
                                  //AxisStatusList[i].POTSensor = GetIOPosLim(i);
                              }
                          });
                        //for (int i = 0; i < AxisStatusList.Count; i++)
                        //{
                        //    if (MMCAxes[i] is MMCGroupAxis)
                        //    {
                        //        double groupActulpos = 0;
                        //        var status = stReadBulkReadData[i].ulAxisStatus;
                        //        var statusRegister = stReadBulkReadData[i].uiStatusRegister;
                        //        //GetGroupActualPosition(i, out groupActulpos);
                        //        //var status = ((MMCGroupAxis)MMCAxes[i]).GroupReadStatus();
                        //        AxisStatusList[i].State = GetServoState1(status);
                        //        retVal = GetMovingState(AxisStatusList[i].State, ref ismoving);
                        //        ResultValidate(retVal);
                        //        AxisStatusList[i].AxisBusy = ismoving;
                        //        AxisStatusList[i].Inposition = IsInposition(statusRegister);

                        //        retVal = IsGroupEnabled(status, ref isenabled);
                        //        ResultValidate(retVal);
                        //        AxisStatusList[i].AxisEnabled = isenabled;
                        //        //AxisStatusList[i].Pulse.Actual = groupActulpos;
                        //        AxisStatusList[i].Pulse.Actual = stReadBulkReadData[i].aPos;
                        //    }
                        //    else
                        //    {
                        //        AxisStatusList[i].Pulse.Actual = stReadBulkReadData[i].aPos;
                        //        AxisStatusList[i].Pulse.Error = stReadBulkReadData[i].iPosFollowingErr;
                        //        AxisStatusList[i].State = GetServoState1(stReadBulkReadData[i].ulAxisStatus);
                        //        retVal = GetMovingState(AxisStatusList[i].State, ref ismoving);
                        //        ResultValidate(retVal);
                        //        AxisStatusList[i].AxisBusy = ismoving;
                        //        //retVal = IsMotorEnabled(i, ref isenabled);
                        //        retVal = IsSingleAxisEnabled(stReadBulkReadData[i].ulAxisStatus, ref isenabled);
                        //        ResultValidate(retVal);

                        //        AxisStatusList[i].AxisEnabled = isenabled;
                        //        AxisStatusList[i].Inposition = IsInposition(stReadBulkReadData[i].uiStatusRegister);

                        //        //GetAxisStatus(i, AxisStatusList[i]);
                        //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("DirectIsMoving End {0}axis", i), stw.ElapsedMilliseconds));
                        //        //GetACEPosition(i, AxisStatusList[i]);
                        //        //AxisStatusList[i].NOTSensor = GetIONegLim(i);
                        //        //AxisStatusList[i].POTSensor = GetIOPosLim(i);
                        //    }
                        //}
                    }
                    mreSyncEvent.Set();
                    //System.Threading.Thread.Sleep(1);
                    //elapsedTime = stw.ElapsedMilliseconds - loopTime;
                    //loopTimeBuff.Add(elapsedTime);
                    //meanTime = loopTimeBuff.Average();
                    //stw.Stop();
                    //mreUpdateEvent.WaitOne(4);
                    ResetEvent.WaitOne(33);
                }
            }
            catch (MMCException err)
            {
                LoggerManager.Debug($"UpdateElmoBaseProc MMCException occurred. Command ID = {err.CommandID}, Err. code = {err.MMCError}, {err.What}");
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}, MMCErr={1}, LibErr={2}", ex.Message, ex.MMCError, ex.LibraryError));
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}, MMCErr={1}", err.Message));
                LoggerManager.Exception(err);

            }
            stw.Stop();
        }


        private int GetGroupActualPosition(int axisindex, out double actualpos)
        {
            int retVal = -1;

            double[] position = new double[16];

            try
            {
                retVal = ((MMCGroupAxis)MMCAxes[axisindex]).GroupReadActualPosition(MC_COORD_SYSTEM_ENUM.MC_ACS_COORD, ref position);
                actualpos = position[0];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EnumAxisState GetServoState(uint nStatus)
        {
            EnumAxisState strMotionStatus = EnumAxisState.ERROR;
            Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            try
            {
                stw.Start();
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start GetServoState"), stw.ElapsedMilliseconds));

                if (((nStatus >> 10) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.ERROR;
                }
                else if (((nStatus >> 7) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.IDLE;
                }
                else if (((nStatus >> 8) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.STOPPING;
                }
                else if (((nStatus >> 9) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.IDLE;
                }
                else if (((nStatus >> 11) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetServoState"), stw.ElapsedMilliseconds));
                stw.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return strMotionStatus;
        }
        public EnumAxisState GetServoState1(uint nStatus)
        {
            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start GetServoState"), stw.ElapsedMilliseconds));
            EnumAxisState strMotionStatus = EnumAxisState.ERROR;

            try
            {
                if (((nStatus >> 10) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.ERROR;
                }
                else if (((nStatus >> 8) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.STOPPING;
                }

                else if (((nStatus >> 11) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }

                else if (((nStatus >> 13) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else if (((nStatus >> 14) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.ERROR;
                }
                else if (((nStatus >> 9) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.DISABLED;
                }
                else if (((nStatus >> 16) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.DISABLED;
                }
                else if (((nStatus >> 3) & 0x01) == 0x01)   // 0xXXXXXXX8: Homing state bit mask
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else if (((nStatus >> 2) & 0x01) == 0x01)   // 0xXXXXXXX4: Constant velocity state bit mask
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else if (((nStatus >> 1) & 0x01) == 0x01)   // 0xXXXXXXX2: Accelerating state bit mask
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else if (((nStatus >> 0) & 0x01) == 0x01)   // 0xXXXXXXX1: Decelerating state bit mask
                {
                    strMotionStatus = EnumAxisState.STOPPING;
                }
                else if (((nStatus >> 4) & 0x01) == 0x01)   // 0xXXXXXX1X: Synchronized moving state bit mask
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else if (((nStatus >> 12) & 0x01) == 0x01)   // 0xxxxx1xxx: Synchronized stopping state bit mask
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else if (nStatus == 0)
                {
                    strMotionStatus = EnumAxisState.MOVING; // 0을 일단 무빙 처리
                }
                else if (((nStatus >> 7) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.IDLE;
                }
                else if (((nStatus >> 17) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.IDLE;
                }
                else if (((nStatus >> 6) & 0x01) == 0x01)
                {
                    strMotionStatus = EnumAxisState.MOVING;
                }
                else
                {
                    strMotionStatus = EnumAxisState.ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetServoState"), stw.ElapsedMilliseconds));
            //stw.Stop();
            return strMotionStatus;
        }

        private bool IsInposition(uint nStatus)
        {
            bool isInpos = false;
            try
            {
                //if (((nStatus >> 7) & 0x01) == 0x01)
                //{
                //    isInpos = true;
                //}
                //else
                //{
                //    if (((nStatus >> 31) & 0x01) == 0x01)
                //    {
                //        isInpos = true;
                //    }
                //    else if (((nStatus >> 17) & 0x01) == 0x01)
                //    {
                //        isInpos = true;
                //    }
                //    else
                //    {
                //        isInpos = false;
                //    }

                //}

                if ((((nStatus >> 12) & 0x01) == 0x01) && (((nStatus >> 11) & 0x01) == 0x01) && (((nStatus >> 10) & 0x01) == 0x01))
                {
                    isInpos = true;
                }
                else
                {
                    isInpos = false;
                }


            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsInposition() Function error: " + err.Message);
            }
            return isInpos;
        }

        public int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public int SetSettlingTime(AxisObject axis, double settlingTime)
        {
            //double rcvval;
            //double val;
            //double time;
            try
            {
                //MMCAxes[axis.AxisIndex.Value].SetParameter((double)axis.DtoP(axis.Config.Inposition.Value), MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS, (int)MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS);
                //MMCAxes[axis.AxisIndex.Value].SetParameter(settlingTime, MMC_PARAMETER_LIST_ENUM.MMC_TARGET_TIME, (int)MMC_PARAMETER_LIST_ENUM.MMC_TARGET_TIME);

                //rcvval = MMCAxes[axis.AxisIndex.Value].GetParameter(MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS, (int)MMC_PARAMETER_LIST_ENUM.MMC_TARGET_RADIUS);
                //time = MMCAxes[axis.AxisIndex.Value].GetParameter(MMC_PARAMETER_LIST_ENUM.MMC_TARGET_TIME, (int)MMC_PARAMETER_LIST_ENUM.MMC_TARGET_TIME);
                //val =(double)axis.PtoD(rcvval);

                axis.SettlingTime = (long)(settlingTime);
                return 0;
            }
            catch (MMCException err)
            {
                LoggerManager.Exception(err);

                return -1;
            }
        }

        private double NormalizedDegree(double value, double lowerLimit = -180.0, double upperLimit = 180.0)
        {
            //return new Degree(this.Value);

            double round = upperLimit - lowerLimit;
            Debug.Assert(round == 360.0,
                "The range of angles is 360.",
                "lowerLimit = {0}, upperLimit = {1}, round = {2}",
                lowerLimit, upperLimit, round);

            double degree = value % round;

            try
            {
                if (degree < lowerLimit)
                    return degree + round;

                if (degree < upperLimit)
                    return degree;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return degree - round;
        }
        public int ValidationSendMessage(int nodeNumber, string obj, string param)
        {
            int retVal = -1;
            int receivedata = -1;
            try
            {
                retVal = this.PMASManager().ReceiveMessage(nodeNumber, obj, out receivedata);
                ResultValidate(retVal);

                if (receivedata.ToString() != param)
                {
                    LoggerManager.Error($"ValidationSendMessage(): different of send data:{param} and receivedata:{receivedata}");
                }
                else
                {

                }
            }
            catch (MMCException mcerror)
            {
                LoggerManager.Exception(mcerror);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retVal = -1;
            }
            return retVal;
        }
        public int NotchFinding(AxisObject axis, EnumMotorDedicatedIn input)
        {
            int retVal = -1;

            try
            {
                //----
                ConfigCapture(axis, input);
                int nodeNum = GetNodeNum(axis.AxisIndex.Value);
                //_PMASManager.SendMessage(nodeNum, "HM[1]", "1");
                //_PMASManager.SendMessage(nodeNum, "HM[4]", "0");




                double onstepAngle;

                int first = 0; //, second = 0;

                double firstPos, secondPos;

                //retVal = this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");
                //retVal = this.PMASManager().SendMessage(nodeNum, "HM[4]", "0");
                //retVal = this.PMASManager().SendMessage(nodeNum, "HM[5]", "2");
                //RelMove(axis, 36000, axis.Param.SeqSpeed.Value, axis.Param.SeqAcc.Value);
                //WaitForAxisMotionDone(axis);

                for (int retrycount = 0; retrycount <= 5; retrycount++)
                {
                    try
                    {
                        //AbsMove(axis, 0, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                        //WaitForAxisMotionDone(axis);
                        retVal = this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");
                        retVal = this.PMASManager().SendMessage(nodeNum, "HM[4]", "2");
                        retVal = this.PMASManager().SendMessage(nodeNum, "HM[5]", "2");
                        retVal = this.PMASManager().SendMessage(nodeNum, "HM[1]", "1");
                        Thread.Sleep(200);
                        //AbsMove(axis, 0, axis.Param.SeqSpeed.Value, axis.Param.SeqAcc.Value);
                        //first -> rising edge
                        onstepAngle = 370.0 * 100.0;

                        //WaitForAxisMotionDone(axis);
                        //Thread.Sleep(100);
                        RelMove(axis, onstepAngle, axis.Param.SeqSpeed.Value, axis.Param.SeqAcc.Value);
                        WaitForAxisMotionDone(axis);

                        int triVal;

                        //_PMASManager.ReceiveMessage(nodeNum, "HM[1]", out triVal);
                        retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[1]", out triVal);
                        ResultValidate(retVal);
                        if (triVal != 0)    // Try one more round
                        {
                            onstepAngle = 370.0 * 100.0;
                            RelMove(axis, onstepAngle, axis.Param.SeqSpeed.Value, axis.Param.SeqAcc.Value);
                            WaitForAxisMotionDone(axis);
                            retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[1]", out triVal);
                            ResultValidate(retVal);
                            if (triVal != 0)
                            {
                                throw new Exception("trigger error");
                            }
                            //throw new Exception("trigger error");
                        }
                        else
                        {
                            retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[7]", out first);
                            ResultValidate(retVal);
                            break;
                        }
                        //_PMASManager.ReceiveMessage(nodeNum, "HM[7]", out first);
                        LoggerManager.Debug($"NotchFinding Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum}  RetryCount:{retrycount}");

                        Thread.Sleep(500);
                    }
                    catch (MMCException mcerror)
                    {
                        LoggerManager.Error($"NotchFinding Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum}  RetryCount:{retrycount}");
                        if (retrycount < 5)
                        {
                            continue;
                        }
                        else
                        {
                            retVal = -1;
                            LoggerManager.Exception(mcerror);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"NotchFinding Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum} RetryCount:{retrycount}");
                        if (retrycount < 5)
                        {
                            continue;
                        }
                        else
                        {
                            retVal = -1;
                            LoggerManager.Exception(err);
                        }
                    }

                }

                firstPos = axis.PtoD(first);
                firstPos = NormalizedDegree(firstPos * 0.01) * 100.0;
                LoggerManager.Debug($"NotchFinding(): Triggered position = {firstPos}.");
                //debug : move to first notch pos
                //AbsMove(axis, firstPos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);

                //second -> 

                //_PMASManager.SendMessage(nodeNum, "HM[1]", "1");
                //_PMASManager.SendMessage(nodeNum, "HM[4]", "0");
                double notchOffset = -1000;
                AbsMove(axis, firstPos + notchOffset, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                WaitForAxisMotionDone(axis);

                this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");

                //Capture array selection(ex: ZX,NT,ET,UI,BH)
                this.PMASManager().SendMessage(nodeNum, "HM[11]", 5.ToString());

                //Capture array low index 어레이 시작 인덱스 
                this.PMASManager().SendMessage(nodeNum, "HM[12]", 1.ToString());

                //Capture array High index 어레이 끝 인덱스 
                this.PMASManager().SendMessage(nodeNum, "HM[13]", 999.ToString());

                //Number of events. HM[4] is performed at last event. 포지션 몇개 잡나
                this.PMASManager().SendMessage(nodeNum, "HM[1]", "99");

                onstepAngle = 75.0 * 100.0;
                RelMove(axis, onstepAngle, axis.Param.HommingSpeed.Value, axis.Param.HommingAcceleration.Value);
                WaitForAxisMotionDone(axis);

                this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");

                AxisStatusList[axis.AxisIndex.Value].CapturePositions = new List<double>();
                int capturedCnt = 0;
                retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[9]", out capturedCnt);
                ResultValidate(retVal);

                int capturePos;
                AxisStatusList[axis.AxisIndex.Value].CapturePositions.Clear();

                for (int i = 1; i < capturedCnt; i++)
                {
                    retVal = this.PMASManager().ReceiveMessage(nodeNum, string.Format("GX[{0}]", i), out capturePos);
                    ResultValidate(retVal);

                    AxisStatusList[axis.AxisIndex.Value].CapturePositions.Add(axis.PtoD(capturePos));
                }

                if (AxisStatusList[axis.AxisIndex.Value].CapturePositions.Count > 1)
                {
                    firstPos = AxisStatusList[axis.AxisIndex.Value].CapturePositions[0];
                    secondPos = AxisStatusList[axis.AxisIndex.Value].CapturePositions[capturedCnt - 2];
                    double notchCenPos = (secondPos - firstPos) / 2d + firstPos;

                    LoggerManager.Debug($"NotchFinding(): Concave positions(Total {AxisStatusList[axis.AxisIndex.Value].CapturePositions.Count} position(s)) = ({firstPos:0.00} ~ {secondPos:0.00}), Notch position = {notchCenPos:0.00}(Angle = {(secondPos - firstPos):0.00})");


                    AbsMove(axis, notchCenPos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                    WaitForAxisMotionDone(axis);
                }
                else
                {
                    LoggerManager.Debug($"Position not properly captured. capturedCnt = {capturedCnt}.");
                    retVal = -1;
                }


                retVal = 0;
                //this.PMASManager().SendMessage(nodeNum, "HM[1]", "1");
                //this.PMASManager().SendMessage(nodeNum, "HM[4]", "0");

                //onstepAngle = 360.0 * 100.0 * -1.0;

                //RelMove(axis, onstepAngle, axis.Param.SeqSpeed.Value, axis.Param.SeqAcc.Value);
                //WaitForAxisMotionDone(axis);

                ////_PMASManager.ReceiveMessage(nodeNum, "HM[1]", out triVal);
                //this.PMASManager().ReceiveMessage(nodeNum, "HM[1]", out triVal);

                //if (triVal != 0)
                //    throw new Exception("trigger error");

                ////_PMASManager.ReceiveMessage(nodeNum, "HM[7]", out second);
                //this.PMASManager().ReceiveMessage(nodeNum, "HM[7]", out second);

                //secondPos = axis.PtoD(second);
                //secondPos = NormalizedDegree(secondPos * 0.01) * 100.0;

                ////debug : move to second notch pos
                ////AbsMove(axis, secondPos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);

                ////move to notch center
                //double notchPos = (firstPos + secondPos) * 0.5;
                //AbsMove(axis, notchPos, axis.Param.Speed.Value, axis.Param.Acceleration.Value);
                //WaitForAxisMotionDone(axis);
                ////SetPosition(axis, 0d);
                //retVal = 0;
            }
            catch (Exception err)
            {
                retVal = -1;
                //LoggerManager.Error($ex.Message);
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public int OriginSet(AxisObject axis, double pos)
        {
            throw new NotImplementedException();
        }

        public int StartScanPosCapt(AxisObject axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            int retVal = -1;

            try
            {
                int nodeNum = GetNodeNum(axis.AxisIndex.Value);

                int inputInt = (int)MotorDedicatedIn;

                AxisStatusList[axis.AxisIndex.Value].CapturePositions.Clear();
                //무조건 초기화 
                this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");

                // EventDefinition ( 입력받을 센서 설정)
                this.PMASManager().SendMessage(nodeNum, "HM[3]", inputInt.ToString());
                //this.PMASManager().SendMessage(nodeNum, "HF[3]", (inputInt + 1).ToString());

                //Capture array selection(ex: ZX,NT,ET,UI,BH)
                this.PMASManager().SendMessage(nodeNum, "HM[11]", 5.ToString());
                //Capture array low index 어레이 시작 인덱스 
                this.PMASManager().SendMessage(nodeNum, "HM[12]", 1.ToString());
                //Capture array High index 어레이 끝 인덱스 
                this.PMASManager().SendMessage(nodeNum, "HM[13]", 999.ToString());


                //Number of events. HM[4] is performed at last event. 포지션 몇개 잡나
                this.PMASManager().SendMessage(nodeNum, "HM[1]", "999");
                // 초기화  Stop the homing process. HM[1] is automatically reset to 0 when homing is complete.
                //this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");
                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public int StopScanPosCapt(AxisObject axis)
        {
            int retVal = 0;
            try
            {
                WaitForAxisMotionDone(axis, 30000);

                int nodeNum = GetNodeNum(axis.AxisIndex.Value);
                this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");

                if(AxisStatusList[axis.AxisIndex.Value].CapturePositions == null)
                {
                    AxisStatusList[axis.AxisIndex.Value].CapturePositions = new List<double>();
                    LoggerManager.Debug($"[ElmoMotionBase] StopScanPosCapt() CapturePositions is null, hashcode : {AxisStatusList[axis.AxisIndex.Value].CapturePositions.GetHashCode()}");
                }
                else
                {
                    AxisStatusList[axis.AxisIndex.Value].CapturePositions.Clear();
                    LoggerManager.Debug($"[ElmoMotionBase] StopScanPosCapt() CapturePositions is clear, hashcode : {AxisStatusList[axis.AxisIndex.Value].CapturePositions.GetHashCode()}");
                }
                
                int capturedCnt = 0;
                retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[9]", out capturedCnt);
                ResultValidate(retVal);
                int capturePos;

                for (int i = 0; i < capturedCnt; i++)
                {
                    retVal = this.PMASManager().ReceiveMessage(nodeNum, string.Format("GX[{0}]", i), out capturePos);

                    if (i == 0)
                    {
                        retVal = 0;
                        LoggerManager.Debug($"Flush start position with Index '0'.");
                    }
                    else
                    {
                        LoggerManager.Debug($"Captured Position #[{i}]: {capturePos}");
                        AxisStatusList[axis.AxisIndex.Value].CapturePositions.Add(axis.PtoD(capturePos));
                        ResultValidate(retVal);
                    }
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;

        }

        public int ForcedZDown(AxisObject axis)
        {
            return 0;
        }

        public int SetDualLoop(bool dualloop)
        {
            return 0;
        }

        public int SetLoadCellZero()
        {
            return 0;
        }

        public int SetMotorStopCommand(AxisObject axis, string setevent, EnumMotorDedicatedIn input)
        {
            int retVal = -1;

            int nodeNum = GetNodeNum(axis.AxisIndex.Value);

            int inputInt = (int)input;

            //return _PMASManager.SendMessage(nodeNum, "HM[3]", inputInt.ToString());
            this.PMASManager().SendMessage(nodeNum, setevent, inputInt.ToString());
            retVal = 0;
            return retVal;

        }
        public int IsHomeSensor(int axisindex, ref bool ishomesensor)
        {
            int retVal = -1;
            try
            {
                var inputs = stReadBulkReadData[axisindex].uiInputs;

                var portStatus = inputs >> 16;
                if ((portStatus & 0x01) == 0x01)
                {
                    ishomesensor = true;
                }
                else
                {
                    ishomesensor = false;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return (int)EnumMotionBaseReturnCode.GetAxisStateError;
            }
            return retVal;
        }
        public int IsLimitSensor(int axisindex, ref bool islimitsensor)
        {
            int retVal = -1;
            try
            {
                var inputs = stReadBulkReadData[axisindex].uiInputs;

                var portStatus = inputs >> 16;
                if ((portStatus & 0x00) == 0x00)
                {
                    islimitsensor = true;
                }
                else
                {
                    islimitsensor = false;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return (int)EnumMotionBaseReturnCode.GetAxisStateError;
            }
            return retVal;
        }
        public int IsThreeLegUp(AxisObject axis, ref bool isthreelegup)
        {
            int retVal = -1;
            //byte flsstate;
            StreamWriter sw = null;
            List<string> triErrorList = new List<string>();
            try
            {

                for (int retrycount = 0; retrycount <= 5; retrycount++)
                {
                    try
                    {
                        mreSyncEvent.WaitOne(syncTimeOut_ms);
                        var inputs = stReadBulkReadData[axis.AxisIndex.Value].uiInputs;
                        // Input value validation failed.
                        if (inputs == 0)
                        {
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} ";
                            triErrorList.Add(errormsg);
                            continue;
                        }
                        if (retrycount > 0)
                        {
                            // Sync with update thread.
                            mreSyncEvent.WaitOne(syncTimeOut_ms);
                        }
                        var portStatus = inputs >> 0;
                        if ((portStatus & 0x01) == 0x00 & (portStatus & 0x02) == 0x02)
                        {
                            isthreelegup = true;
                        }
                        else
                        {
                            if ((portStatus & 0x01) == 0x00 & (portStatus & 0x02) == 0x00)
                            {
                                //LoggerManager.Debug($"IsThreeLegUp(): ThreeLeg Invalid State. Retry status scanning...({retrycount} / maxRetryCount)");
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                isthreelegup = false;
                                continue;
                            }
                            if ((portStatus & 0x01) == 0x01 & (portStatus & 0x02) == 0x02)
                            {
                                //LoggerManager.Debug($"IsThreeLegUp(): ThreeLeg Invalid State. Retry status scanning...({retrycount} / maxRetryCount)");
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                isthreelegup = false;
                                continue;
                            }
                            isthreelegup = false;
                        }
                        //0x60FD
                        //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).UploadSDO(0x20FD, 0, out flsstate, 500);
                        //if (((flsstate >> (int)EnumDigitalInputs.FLS) & 0x01) == 0x01)
                        //{
                        //    isthreelegup = true;
                        //}
                        //else
                        //{
                        //    isthreelegup = false;
                        //}
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        break;
                    }
                    catch (MMCException err)
                    {
                        if (retrycount < 6)
                        {
                            Thread.Sleep(300);
                            retVal = (int)EnumMotionBaseReturnCode.ThreeLegUpError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}" +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}");
                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} " +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}";
                            triErrorList.Add(errormsg);
                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ThreeLegUpError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (retrycount < 6)
                        {
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ThreeLegUpError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} Msg:{ex.Message}");
                        }
                    }
                }
            }

            catch (MMCException err)
            {
                LoggerManager.Exception(err);
                return (int)EnumMotionBaseReturnCode.ThreeLegUpError;
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
                return (int)EnumMotionBaseReturnCode.ThreeLegUpError;
            }
            finally
            {
                if (0 < (triErrorList?.Count ?? 0))
                {
                    try
                    {
                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in triErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }
            }
            return retVal;
        }

        public int IsThreeLegDown(AxisObject axis, ref bool isthreelegdn)
        {
            int retVal = -1;
            //byte rlsstate;
            int maxRetryCount = 5;
            StreamWriter sw = null;
            List<string> triErrorList = new List<string>();

            try
            {

                for (int retrycount = 0; retrycount <= maxRetryCount; retrycount++)
                {
                    try
                    {
                        mreSyncEvent.WaitOne(syncTimeOut_ms);
                        var inputs = stReadBulkReadData[axis.AxisIndex.Value].uiInputs;
                        // Input value validation failed.
                        if (inputs == 0)
                        {
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} ";
                            triErrorList.Add(errormsg);
                            continue;
                        }

                        if (retrycount > 0)
                        {
                            // Sync with update thread.
                            mreSyncEvent.WaitOne(syncTimeOut_ms);
                        }
                        var portStatus = inputs >> 0;
                        if ((portStatus & 0x01) == 0x01 & (portStatus & 0x02) == 0x00)
                        {
                            isthreelegdn = true;
                        }
                        else
                        {
                            if ((portStatus & 0x01) == 0x00 & (portStatus & 0x02) == 0x00)
                            {
                                //LoggerManager.Debug($"IsThreeLegDown(): ThreeLeg Invalid State. Retry status scanning...({retrycount} / maxRetryCount)");
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                isthreelegdn = false;
                                continue;
                            }
                            if ((portStatus & 0x01) == 0x01 & (portStatus & 0x02) == 0x02)
                            {
                                //LoggerManager.Debug($"IsThreeLegUp(): ThreeLeg Invalid State. Retry status scanning...({retrycount} / maxRetryCount)");
                                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                                isthreelegdn = false;
                                continue;
                            }
                            isthreelegdn = false;
                        }

                        //((MMCSingleAxis)MMCAxes[axis.AxisIndex.Value]).UploadSDO(0x20FD, 0, out rlsstate, 500);
                        //if (((rlsstate >> (int)EnumDigitalInputs.RLS) & 0x01) == 0x01)
                        //{
                        //    isthreelegdn = true;
                        //}
                        //else
                        //{
                        //    isthreelegdn = false;
                        //}



                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        break;
                    }
                    catch (MMCException err)
                    {
                        if (retrycount < 6)
                        {
                            Thread.Sleep(300);
                            retVal = (int)EnumMotionBaseReturnCode.ThreeLegDnError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} " +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}");

                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} " +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}";
                            triErrorList.Add(errormsg);

                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ThreeLegDnError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (retrycount < 6)
                        {
                            Thread.Sleep(1000);

                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.ThreeLegDnError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} Msg:{ex.Message}");
                        }
                    }
                    finally
                    {
                        if (0 < (triErrorList?.Count ?? 0))
                        {
                            try
                            {
                                sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                                foreach (var errorList in triErrorList)
                                {
                                    sw.WriteLine(errorList);
                                }
                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            }
                            finally
                            {
                                if (sw != null)
                                {
                                    sw.Dispose();
                                    sw = null;
                                }
                            }
                        }
                    }
                }
            }

            catch (MMCException err)
            {
                LoggerManager.Exception(err);
                return (int)EnumMotionBaseReturnCode.ThreeLegDnError;
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
                return (int)EnumMotionBaseReturnCode.ThreeLegDnError;
            }

            return retVal;
        }

        public int IsRls(AxisObject axis, ref bool isrls)
        {
            int retVal = -1;
            StreamWriter sw = null;
            List<string> triErrorList = new List<string>();
            try
            {

                for (int retrycount = 0; retrycount <= 5; retrycount++)
                {
                    try
                    {
                        mreSyncEvent.WaitOne(syncTimeOut_ms);
                        var inputs = stReadBulkReadData[axis.AxisIndex.Value].uiInputs;
                        // Input value validation failed.
                        if (inputs == 0)
                        {
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} ";
                            triErrorList.Add(errormsg);
                            continue;
                        }
                        if (retrycount > 0)
                        {
                            // Sync with update thread.
                            mreSyncEvent.WaitOne(syncTimeOut_ms);
                        }
                        var portStatus = inputs >> 0;
                        if ((portStatus & 0x01) == 0x01 & (portStatus & 0x02) == 0x00)
                        {
                            isrls = true;
                        }
                        else
                        {
                            isrls = false;
                        }
                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        break;
                    }
                    catch (MMCException err)
                    {
                        if (retrycount < 6)
                        {
                            Thread.Sleep(300);
                            retVal = (int)EnumMotionBaseReturnCode.InputRetreiveError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}" +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}");
                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} " +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}";
                            triErrorList.Add(errormsg);
                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.InputRetreiveError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                        if (retrycount < 6)
                        {
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.InputRetreiveError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                        }
                    }
                }
            }

            catch (MMCException err)
            {
                LoggerManager.Exception(err);
                return (int)EnumMotionBaseReturnCode.InputRetreiveError;
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
                return (int)EnumMotionBaseReturnCode.InputRetreiveError;
            }
            finally
            {
                if (0 < (triErrorList?.Count ?? 0))
                {
                    try
                    {
                        sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                        foreach (var errorList in triErrorList)
                        {
                            sw.WriteLine(errorList);
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Dispose();
                            sw = null;
                        }
                    }
                }
            }
            return retVal;
        }

        public int IsFls(AxisObject axis, ref bool isfls)
        {
            int retVal = -1;
            int maxRetryCount = 5;
            StreamWriter sw = null;
            List<string> triErrorList = new List<string>();

            try
            {

                for (int retrycount = 0; retrycount <= maxRetryCount; retrycount++)
                {
                    try
                    {
                        mreSyncEvent.WaitOne(syncTimeOut_ms);
                        var inputs = stReadBulkReadData[axis.AxisIndex.Value].uiInputs;
                        // Input value validation failed.
                        if (inputs == 0)
                        {
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} ";
                            triErrorList.Add(errormsg);
                            continue;
                        }

                        if (retrycount > 0)
                        {
                            // Sync with update thread.
                            mreSyncEvent.WaitOne(syncTimeOut_ms);
                        }
                        var portStatus = inputs >> 0;
                        if ((portStatus & 0x01) == 0x00 & (portStatus & 0x02) == 0x02)
                        {
                            isfls = true;
                        }
                        else
                        {
                            isfls = false;
                        }

                        retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        break;
                    }
                    catch (MMCException err)
                    {
                        if (retrycount < 6)
                        {
                            Thread.Sleep(300);
                            retVal = (int)EnumMotionBaseReturnCode.InputRetreiveError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} " +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}");

                            string errormsg = $"[{DateTime.Now.ToString()}]/// {MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount} " +
                                $"Err. type = {err.What}, MMC Error code = {err.MMCError} MMCStatus = {err.Status} MMCLibError = {err.LibraryError}";
                            triErrorList.Add(errormsg);

                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.InputRetreiveError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                        if (retrycount < 6)
                        {
                            Thread.Sleep(1000);

                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                            continue;
                        }
                        else
                        {
                            retVal = (int)EnumMotionBaseReturnCode.InputRetreiveError;
                            LoggerManager.Error($"{MethodBase.GetCurrentMethod()}, Axis:{axis.Label.Value}  RetryCount:{retrycount}");
                        }
                    }
                    finally
                    {
                        if (0 < (triErrorList?.Count ?? 0))
                        {
                            try
                            {
                                sw = new StreamWriter(Path.Combine(FileFolder, FileName), true);

                                foreach (var errorList in triErrorList)
                                {
                                    sw.WriteLine(errorList);
                                }
                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            }
                            finally
                            {
                                if (sw != null)
                                {
                                    sw.Dispose();
                                    sw = null;
                                }
                            }
                        }
                    }
                }
            }

            catch (MMCException err)
            {
                LoggerManager.Exception(err);
                return (int)EnumMotionBaseReturnCode.InputRetreiveError;
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
                return (int)EnumMotionBaseReturnCode.InputRetreiveError;
            }

            return retVal;
        }


        private int WaitforEventDone(AxisObject axis, EnumDigitalInputs input, EnumEventActionType action, int timeout = 0)
        {
            int retVal = -1;
            //int nodeNum = 0;
            byte rlsstate;
            bool isinRLS = false;
            bool isinpos = false;
            bool isaxisbusy = false;
            uint ustatus = 0;
            //EnumAxisState axisstate;
            Stopwatch elapsedStopWatch = new Stopwatch();

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            stw.Restart();
            stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
            try
            {
                bool runFlag = true;
                EnumAxisState axisState = EnumAxisState.INVALID;
                //mreSyncEvent.WaitOne(syncTimeOut_ms);
                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("First End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                bool[] rlsStates = new bool[axis.GroupMembers.Count];
                do
                {

                    //GetAxisStatus(axis);
                    //foreach (var item in axis.GroupMembers)
                    //{
                    //    ((MMCSingleAxis)MMCAxes[item.AxisIndex.Value]).UploadSDO(0x20FD, 0, out rlsstate, 500);
                    //    if (((rlsstate >> (int)input) & 0x01) == 0x01)
                    //    {

                    //        isinRLS = true;
                    //        Log.Debug($"{item.Label.Value} Axis ");
                    //    }
                    //}
                    isinRLS = true;
                    for (int i = 0; i < axis.GroupMembers.Count; i++)
                    {
                        ((MMCSingleAxis)MMCAxes[axis.GroupMembers[i].AxisIndex.Value]).UploadSDO(0x20FD, 0, out rlsstate, 500);
                        if (((rlsstate >> (int)input) & 0x01) == 0x01)
                        {
                            rlsStates[i] = true;
                        }
                        isinRLS = isinRLS & rlsStates[i];
                    }


                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                    retVal = IsInposition(axis, ref isinpos);
                    ResultValidate(retVal);
                    retVal = QueryIsMoving(axis, ref isaxisbusy);
                    ResultValidate(retVal);
                    retVal = GetElmoAxisStatus(axis, ref ustatus);
                    ResultValidate(retVal);
                    axisState = GetServoState1(ustatus);

                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End ISMoving {0}axis", axis.Label), stw.ElapsedMilliseconds));
                    //axis.Status.AxisEnabled = IsMotorEnabled(axis);
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End ISMoving {0}axis", axis.Label), stw.ElapsedMilliseconds));
                    if (isinpos == false | isaxisbusy == true)
                    {
                        if (isinRLS)
                        {
                            runFlag = false;
                            Stop(axis);
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
                        }
                        else if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {

                                LoggerManager.Debug($"WaitforEventDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = (int)EnumMotionBaseReturnCode.TimeOutError;
                                Stop(axis);
                            }
                        }
                        else if (axisState == EnumAxisState.ERROR)
                        {
                            runFlag = false;
                            retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                            throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for event. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                        }
                        else
                        {
                            runFlag = true;
                        }
                    }
                    else
                    {

                        if (isinRLS)
                        {
                            if (axisState != EnumAxisState.IDLE)
                            {
                                runFlag = false;
                                retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                                throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for event. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                            }
                            Stop(axis);
                            runFlag = false;
                            retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;

                        }

                        if (axisState == EnumAxisState.ERROR)
                        {
                            //if (timeout != 0)
                            //{
                            //    if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            //    {

                            //        LoggerManager.Debug($"WaitforEventDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                            //        runFlag = false;
                            //        retVal = -2;
                            //        Stop(axis);
                            //    }
                            //}
                            runFlag = false;
                            retVal = (int)EnumMotionBaseReturnCode.GetAxisStateError;
                            throw new MotionException($"{axis.Label.Value} Axis error occurred while wait for event. State = {axisState}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                            //retVal = 0;
                            //runFlag = false;
                            //Log.Info(string.Format("WaitForAxisMotionDone({0}) : Axis motion done.", axis.AxisIndex.Value));
                        }
                        else
                        {
                            Stop(axis);
                            retVal = (int)EnumMotionBaseReturnCode.FatalError;
                            runFlag = false;
                            LoggerManager.Debug($"WaitforEventDone({axis.AxisIndex.Value}, {MMCAxes[axis.AxisIndex.Value].AxisName}) : Axis motion error occurred. Err code = {axis.Status.ErrCode}");
                        }
                    }
                    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
                } while (runFlag == true);

                //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));

            }
            catch (Exception err)
            {
                LoggerManager.Error($"{axis.Label.Value} Axis WaitforEventDone() Function error: {err.Message}");

                //retVal = -1;
            }
            finally
            {
                elapsedStopWatch?.Stop();
                //double pos=0;
                // GetCmdPosition(axis, ref pos);
                // axis.Status.Position.Ref = pos;
            }
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));
            stw.Stop();
            //int step = 0;
            //foreach (var item in timeStamp)
            //{
            //    step++;
            //    Log.Debug(string.Format("Time Stamp [{0}] - Desc: {1}, Time: {2}", step, item.Key, item.Value));
            //}


            return retVal;
        }
        private int SyncNHPIHoming(AxisObject axis)
        {
            int retVal = -1;
            bool axisEnabled = false;
            LoggerManager.Debug($"[Motion] SyncNHPIHoming Start Axis:{axis.Label.Value}");

            foreach (var item in axis.GroupMembers)
            {
                AmpFaultClear(item);
            }

            AmpFaultClear(axis);
            try
            {
                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupDisable();
                foreach (var item in axis.GroupMembers)
                {
                    retVal = IsMotorEnabled(item, ref axisEnabled);
                    if(!axisEnabled)
                    {
                        retVal = DisableAxis(item);
                        ResultValidate(retVal);
                    }
                }

                Thread.Sleep(1000);
                foreach (var item in axis.GroupMembers)
                {
                    retVal = IsMotorEnabled(item, ref axisEnabled);
                    if (!axisEnabled)
                    {
                        retVal = EnableAxis(item);
                        ResultValidate(retVal);
                        Thread.Sleep(600);
                    }
                }
                Thread.Sleep(1000);
                retVal = EnableAxis(axis);
                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupEnable();
                ResultValidate(retVal);

                Thread.Sleep(600);

                int nodeNum = 0;
                foreach (var item in axis.GroupMembers)
                {
                    nodeNum = GetNodeNum(item.AxisIndex.Value);
                    this.PMASManager().SendMessage(nodeNum, "HM[1]", 0.ToString());
                    this.PMASManager().SendMessage(nodeNum, "IL[1]", 9.ToString());

                    this.PMASManager().SendMessage(nodeNum, "HM[3]", 0.ToString());
                    this.PMASManager().SendMessage(nodeNum, "HM[5]", 0.ToString());
                    this.PMASManager().SendMessage(nodeNum, "HM[6]", 0.ToString());
                    this.PMASManager().SendMessage(nodeNum, "HM[4]", 0.ToString());
                    //this.PMASManager().SendMessage(nodeNum, "HM[1]", 1.ToString());
                }
                //byte rlsstate;

                bool isinRLS = false;
                for (int i = 0; i < axis.GroupMembers.Count; i++)
                {
                    Thread.Sleep(1000);
                    var inputs = stReadBulkReadData[axis.GroupMembers[i].AxisIndex.Value].uiInputs;
                    var portStatus = inputs >> 0;

                    if ((portStatus & 0x01) == 0x01)
                    {
                        isinRLS = true;
                    }
                }
                Thread.Sleep(1000);
                for (int i = 0; i < axis.GroupMembers.Count; i++)
                {
                    Thread.Sleep(1000);
                    var inputs = stReadBulkReadData[axis.GroupMembers[i].AxisIndex.Value].uiInputs;
                    var portStatus = inputs >> 0;

                    if ((portStatus & 0x01) == 0x01)
                    {
                        isinRLS = true;
                        LoggerManager.Debug($"RLS Triggered on Slave#{i}");
                    }
                }

                //for (int i = 0; i < axis.GroupMembers.Count; i++)
                //{
                //    Thread.Sleep(1000);
                //    ((MMCSingleAxis)MMCAxes[axis.GroupMembers[i].AxisIndex.Value]).UploadSDO(0x20FD, 0, out rlsstate, 3000);
                //    if ((rlsstate & 0x01) == 0x01)
                //    {
                //        isinRLS = true;
                //    }
                //}
                //Thread.Sleep(1000);
                //for (int i = 0; i < axis.GroupMembers.Count; i++)
                //{
                //    Thread.Sleep(1000);
                //    ((MMCSingleAxis)MMCAxes[axis.GroupMembers[i].AxisIndex.Value]).UploadSDO(0x20FD, 0, out rlsstate, 3000);
                //    if ((rlsstate & 0x01) == 0x01)
                //    {
                //        isinRLS = true;
                //    }
                //}

                if (isinRLS)
                {
                    DisableAxis(axis);
                    retVal = SetPosition(axis, axis.Param.HomeOffset.Value);
                    ResultValidate(retVal);

                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupEnable();
                    LoggerManager.Debug($"SyncNHPI(): Relmove to Home distance to escape from RLS");
                    retVal = RelMove(axis, axis.Param.HomeDistLimit.Value, axis.Param.HommingSpeed.Value, axis.Param.HommingAcceleration.Value);
                    ResultValidate(retVal);
                    retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(retVal);
                    LoggerManager.Debug($"SyncNHPI(): VMove to RLS");
                    retVal = VMove(axis, axis.Param.HommingSpeed.Value * -1d,
                        axis.Param.HommingAcceleration.Value,
                        axis.Param.HommingDecceleration.Value,
                        EnumTrjType.Homming);
                    ResultValidate(retVal);
                    retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(retVal);
                }
                else
                {
                    LoggerManager.Debug($"SyncNHPI(): VMove to down position without RLS escape");
                    retVal = VMove(axis, axis.Param.HommingSpeed.Value * -1d,
                        axis.Param.HommingAcceleration.Value,
                        axis.Param.HommingDecceleration.Value,
                        EnumTrjType.Homming);
                    ResultValidate(retVal);

                    //WaitforEventDone(axis, EnumDigitalInputs.RLS, EnumEventActionType.ActionE_STOP, 60000);

                    retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(retVal);

                }

                for (int i = 0; i < axis.GroupMembers.Count; i++)
                {
                    nodeNum = GetNodeNum(axis.GroupMembers[i].AxisIndex.Value);
                    this.PMASManager().SendMessage(nodeNum, "HM[1]", 0.ToString());
                    LoggerManager.Debug($"SyncNHPI(): Send capture clear to Slave#{i}");
                    //this.PMASManager().SendMessage(nodeNum, "IL[1]", 17.ToString());
                }

                retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                ResultValidate(retVal);
                retVal = DisableAxis(axis);
                ResultValidate(retVal);

                //((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupDisable();

                //for (int i = 0; i < axis.GroupMembers.Count; i++)
                //{
                //    DS402HomingProgress(((MMCSingleAxis)MMCAxes[axis.GroupMembers[i].AxisIndex.Value]),
                //               35,
                //               axis.DtoP(axis.Param.HomeOffset.Value * -1d),
                //               (float)GetVelocity(axis, EnumTrjType.Homming),
                //               (float)GetAccel(axis, EnumTrjType.Homming),
                //               (float)axis.DtoP((Math.Abs(axis.Param.PosSWLimit.Value) + Math.Abs(axis.Param.NegSWLimit.Value))),
                //               (float)axis.DtoP(axis.Param.IndexSearchingSpeed.Value),
                //               (float)axis.DtoP(axis.Param.HommingSpeed.Value),
                //               (float)GetVelocity(axis, EnumTrjType.Homming));
                //}
                Thread.Sleep(600);
                retVal = SetPosition(axis, axis.Param.HomeOffset.Value);
                ResultValidate(retVal);
                Thread.Sleep(600);
                ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupEnable();

                LoggerManager.Debug($"SyncNHPI(): Home shift movement. Homeshift is {axis.Param.HomeShift.Value:0.000}");

                retVal = RelMove(axis, axis.Param.HomeShift.Value, axis.Param.HommingSpeed.Value,
                    axis.Param.HommingAcceleration.Value);
                ResultValidate(retVal);

                retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                ResultValidate(retVal);

                // index 찾기 


                ////Capture array selection(ex: ZX,NT,ET,UI,BH)
                //this.PMASManager().SendMessage(nodeNum, "HM[11]", 5.ToString());
                ////Capture array low index 어레이 시작 인덱스 
                //this.PMASManager().SendMessage(nodeNum, "HM[12]", 1.ToString());
                ////Capture array High index 어레이 끝 인덱스 
                //this.PMASManager().SendMessage(nodeNum, "HM[13]", 1.ToString());
                //Number of events. HM[4] is performed at last event. 포지션 몇개 잡나

                //P01.a32 > hm[1] = 0
                //P01.a32 > hm[3] = 3
                //P01.a32 > hm[6] = 1
                //P01.a32 > hm[4] = 1
                //P01.a32 > hm[5] = 2
                //P01.a32 > hm[1] = 1
                //P01.a32 > hm[7]

                foreach (var item in axis.GroupMembers)
                {
                    nodeNum = GetNodeNum(item.AxisIndex.Value);
                    this.PMASManager().SendMessage(nodeNum, "HM[1]", "0");  // Disable capture
                    this.PMASManager().SendMessage(nodeNum, "HM[3]", "3");  // Set strobe input as Index pulse
                    //this.PMASManager().SendMessage(nodeNum, "HM[6]", "1");  // Output value when input triggered
                    //this.PMASManager().SendMessage(nodeNum, "HM[4]", "1");  // Event behavior. 0 = Stop, 1 = Set output 2 = Do Nothing
                    this.PMASManager().SendMessage(nodeNum, "HM[6]", "0");  // Output value when input triggered
                    this.PMASManager().SendMessage(nodeNum, "HM[4]", "2");  // Event behavior. 0 = Stop, 1 = Set output 2 = Do Nothing
                    this.PMASManager().SendMessage(nodeNum, "HM[5]", "2");  // Set value when event triggered. 0 = Position, 1 = X, 2 = Do Nothing, 3 = Gantry related
                    this.PMASManager().SendMessage(nodeNum, "HM[1]", "1");
                    LoggerManager.Debug($"SyncNHPI(): Set capture for Node#{nodeNum}");

                }
                LoggerManager.Debug($"SyncNHPI(): Home dist. Limit Move. Dist = {axis.Param.HomeDistLimit.Value}");

                retVal = RelMove(axis, axis.Param.HomeDistLimit.Value, axis.Param.HommingSpeed.Value, axis.Param.HommingAcceleration.Value);
                ResultValidate(retVal);
                retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                ResultValidate(retVal);
                Thread.Sleep(1000);

                foreach (var item in axis.GroupMembers)
                {
                    AxisStatusList[item.AxisIndex.Value].CapturePositions = new List<double>();
                }
                Thread.Sleep(1000);

                double[] cptpos = new double[axis.GroupMembers.Count];

                for (int i = 0; i < axis.GroupMembers.Count; i++)
                {
                    LoggerManager.Debug($"SyncNHPI(): Retreive captured position from Slave#{i}");

                    nodeNum = GetNodeNum(axis.GroupMembers[i].AxisIndex.Value);
                    int capturepos = 0;
                    double convertpos = 0d;
                    Thread.Sleep(1000);
                    retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[7]", out capturepos);
                    ResultValidate(retVal);

                    convertpos = Convert.ToDouble(capturepos);
                    if (convertpos == 0)
                    {
                        Thread.Sleep(500);
                        retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[7]", out capturepos);
                        ResultValidate(retVal);

                        convertpos = Convert.ToDouble(capturepos);
                        if (convertpos == 0)
                        {
                            Thread.Sleep(500);
                            retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[7]", out capturepos);
                            Thread.Sleep(500);
                            retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[7]", out capturepos);
                            Thread.Sleep(500);
                            retVal = this.PMASManager().ReceiveMessage(nodeNum, "HM[7]", out capturepos);

                            ResultValidate(retVal);
                            convertpos = Convert.ToDouble(capturepos);
                            if (convertpos == 0)
                            {
                                LoggerManager.Debug($"Index searching failed on node a{nodeNum}.");
                                throw new MotionException($"Index searching failed on node a{nodeNum}.", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                                //System.Diagnostics.Debug.Assert(false, $"Index searching failed on {nodeNum} node.");
                            }
                        }
                    }
                    cptpos[i] = axis.GroupMembers[i].PtoD(convertpos);
                    LoggerManager.Debug($"Axis{axis.GroupMembers[i].Label.Value}'s Index = {cptpos[i]}");
                }
                double[] capturePositions = new double[axis.GroupMembers.Count];
                double minMaxParam = EcatAxisDesc.ECATAxisDescripterParams.GroupAxisDeviation;
                if(minMaxParam<=0)
                {
                    minMaxParam = 2000;
                }

                for (int i = 0; i < axis.GroupMembers.Count; i++)
                {
                    LoggerManager.Debug($"Axis{axis.GroupMembers[i].Label.Value}'s 편차는  {cptpos[0] - (cptpos[i])}");
                    capturePositions[i] = cptpos[0] - cptpos[i];
                }
                if (minMaxParam < Math.Abs(capturePositions.Min() - capturePositions.Max()))
                {
                    retVal = -1;
                    LoggerManager.Debug($"[Motion Homming ERROR] SyncNHPIHoming CapturePos Deviation Param={minMaxParam} ,Min{capturePositions.Min()}, Max{capturePositions.Max()} Axis:{axis.Label.Value} return:{retVal}");
                }
                else
                {

                    double cenPos = cptpos[0] + axis.GroupMembers[0].Param.HomeOffset.Value;

                    retVal = AbsMove(axis, cptpos[0], axis.Param.HommingSpeed.Value, axis.Param.HommingAcceleration.Value);
                    ResultValidate(retVal);
                    retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(retVal);

                    //double offset1 = cptpos[0] - cptpos[1];
                    //double offset2 = cptpos[0] - cptpos[2];

                    //double homeoffset1 = cptpos[1] + offset1;
                    //double homeoffset2 = cptpos[2] + offset2;
                    double[] offsets = new double[axis.GroupMembers.Count];
                    double[] homeOffsets = new double[axis.GroupMembers.Count];

                    for (int i = 0; i < axis.GroupMembers.Count; i++)
                    {
                        offsets[i] = cptpos[0] - cptpos[i];
                        homeOffsets[i] =
                            axis.Param.HomeOffset.Value +
                            axis.GroupMembers[i].Param.HomeOffset.Value +
                            offsets[i];
                        LoggerManager.Debug($"Axis{axis.GroupMembers[i].Label.Value}'s Homeoffsets  {homeOffsets[i]}");

                    }
                    Thread.Sleep(1000);
                    retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(retVal);
                    DisableAxis(axis);
                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupDisable();
                    Thread.Sleep(1000);
                    for (int i = 0; i < axis.GroupMembers.Count; i++)
                    {
                        //DS402HomingProgress(((MMCSingleAxis)MMCAxes[axis.GroupMembers[i].AxisIndex.Value]),
                        //           35,
                        //           axis.DtoP(homeOffsets[i] * -1d),
                        //           (float)GetVelocity(axis, EnumTrjType.Homming),
                        //           (float)GetAccel(axis, EnumTrjType.Homming),
                        //           (float)axis.DtoP((Math.Abs(axis.Param.PosSWLimit.Value) + Math.Abs(axis.Param.NegSWLimit.Value))),
                        //           (float)axis.DtoP(axis.Param.IndexSearchingSpeed.Value),
                        //           (float)axis.DtoP(axis.Param.HommingSpeed.Value),
                        //           (float)GetVelocity(axis, EnumTrjType.Homming));
                        retVal = SetPosition(axis.GroupMembers[i], homeOffsets[i]);
                        ResultValidate(retVal);
                    }


                    ////foreach (var item in axis.GroupMembers)
                    ////{
                    ////    AbsMove(item, axis.GroupMembers[0].Param.HomeOffset.Value, axis.Param.HommingSpeed.Value,
                    ////    axis.Param.HommingAcceleration.Value);
                    ////}
                    ////foreach (var item in axis.GroupMembers)
                    ////{
                    ////    WaitForAxisMotionDone(item);
                    ////}
                    Thread.Sleep(1000);
                    ((MMCGroupAxis)MMCAxes[axis.AxisIndex.Value]).GroupEnable();
                    Thread.Sleep(1000);
                    //SetPosition(axis, axis.Param.HomeOffset.Value);

                    double posOffset = 0;
                    if (axis.Param.HomeOffset.Value >= 0)
                    {
                        posOffset = -200;
                    }
                    else
                    {
                        posOffset = 200;
                    }

                    retVal = RelMove(axis, posOffset,
                        axis.Param.HommingSpeed.Value,
                        axis.Param.HommingAcceleration.Value);
                    ResultValidate(retVal);

                    retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(retVal);

                    Thread.Sleep(1000);

                    retVal = AbsMove(axis,
                        axis.Param.HomeOffset.Value,
                        axis.Param.HommingSpeed.Value,
                        axis.Param.HommingAcceleration.Value);
                    ResultValidate(retVal);

                    retVal = WaitForAxisMotionDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(retVal);

                    retVal = 0;
                }
            }
            catch (MMCException ex)
            {
                LoggerManager.Error($"SyncNHPIHoming() Function error axis = {axis.Label.Value}: Err. type = {ex.What}, MMC Error code = {ex.MMCError}, Message{ex.Message}");
                retVal = -1;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error occurred. {err.Message}");
                retVal = -1;

            }
            LoggerManager.Debug($"[Motion] SyncNHPIHoming End Axis:{axis.Label.Value} return:{retVal}");
            return retVal;
        }

        public int GetAuxPulse(AxisObject axis, ref int pos)
        {
            SDOResetEvent.WaitOne(100);
            int retVal = GetAuxPos(axis.AxisIndex.Value, ref pos);
            if (retVal != 0)
            {
                retVal = GetAuxPos(axis.AxisIndex.Value, ref pos);
            }
            return retVal;
        }
    }
}
