using Command.TCPIP;
using DBManagerModule;
using DutEditorPageViewModel;
using FileSystem;
using LoaderParameters.Data;
using LogModule;
using LotOP;
using NotifyEventModule;
using ProbeEvent;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event.EventProcess;
using ProberInterfaces.Foup;
using ProberInterfaces.Temperature;
using ProberViewModel;
using ProbingModule;
using RelayCommandBase;
using RequestCore.ActionPack.TCPIP;
using RequestCore.Query.TCPIP;
using RequestCore.QueryPack;
using RequestInterface;
using ResultMapModule;
using SequenceEngine;
using StageModule;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TCPIP;
using TesterCommunicationModule;
using TesterDriverModule.TCPIP;
using EventManager = ProbeEvent.EventManager;


namespace ProberEmulator.ViewModel
{
    public enum ProberEmulCommunicationMode
    {
        MANUAL = 0,
        FLOW
    }

    public class VMProberEmulator : INotifyPropertyChanged, IFactoryModule, IMainScreenViewModel
    {
        readonly Guid _ViewModelGUID = new Guid("3a57b612-5b66-43b9-9217-136e59e36cab");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private Autofac.IContainer Container;

        private string Terminator = "\r\n";

        public string SendCommand { get; set; }

        public SimpleInterruptCommandRecipe SelectedInterruptCommand { get; set; }

        public FileManager FileManager { get; set; }
        public EventManager EventManager { get; set; }
        public EventExecutor EventExecutor { get; set; }
        public TCPIPModule TCPIPModule { get; set; }
        public ResultMapManager ResultMapManager { get; set; }

        public TesterCommunicationManager TesterCommunicationManager { get; set; }

        public StageSupervisor StageSupervisorModule { get; set; }

        public Command.CommandManager CommandManager { get; set; }

        public SequenceEngineManager SequenceEngineManager { get; set; }


        public ObservableCollection<CollectionComponent> CommandCollection { get; set; }

        //public ObservableCollection<string> InterruptCommandlist { get; set; }
        public ObservableCollection<SimpleInterruptCommandRecipe> InterruptCommandlist { get; set; }

        public DummyDataModule DummyDataModule { get; set; }

        //private List<MachineIndex> ProbingSequence { get; set; }

        private MachineIndex LastMI { get; set; }

        private LotScenario lotScenario { get; set; }

        private bool _CanLotStart = false;
        public bool CanLotStart
        {
            get { return _CanLotStart; }
            set
            {
                if (value != _CanLotStart)
                {
                    _CanLotStart = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ProberEmulCommunicationMode _ComType;
        public ProberEmulCommunicationMode ComType
        {
            get { return _ComType; }
            set
            {
                if (value != _ComType)
                {
                    _ComType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private string _SelectedSlotIndex;
        //public string SelectedSlotIndex
        //{
        //    get { return _SelectedSlotIndex; }
        //    set
        //    {
        //        if (value != _SelectedSlotIndex)
        //        {
        //            _SelectedSlotIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<SlotObject> _SelectedSlotObject;
        public ObservableCollection<SlotObject> SelectedSlotObject
        {
            get { return _SelectedSlotObject; }
            set
            {
                if (value != _SelectedSlotObject)
                {
                    _SelectedSlotObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedSlotCount;
        public int SelectedSlotCount
        {
            get { return _SelectedSlotCount; }
            set
            {
                if (value != _SelectedSlotCount)
                {
                    _SelectedSlotCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _MainTabScreenView;
        public IMainScreenView MainTabScreenView
        {
            get { return _MainTabScreenView; }
            set
            {
                if (value != _MainTabScreenView)
                {
                    _MainTabScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _DataTabScreenView;
        public IMainScreenView DataTabScreenView
        {
            get { return _DataTabScreenView; }
            set
            {
                if (value != _DataTabScreenView)
                {
                    _DataTabScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _ResultMapTabScreenView;
        public IMainScreenView ResultMapTabScreenView
        {
            get { return _ResultMapTabScreenView; }
            set
            {
                if (value != _ResultMapTabScreenView)
                {
                    _ResultMapTabScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _PreMainScreenView;
        public IMainScreenView PreMainScreenView
        {
            get { return _PreMainScreenView; }
            set
            {
                if (value != _PreMainScreenView)
                {
                    _PreMainScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _PreDataMainScreenView;
        public IMainScreenView PreDataMainScreenView
        {
            get { return _PreDataMainScreenView; }
            set
            {
                if (value != _PreDataMainScreenView)
                {
                    _PreDataMainScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _PreResultMapMainScreenView;
        public IMainScreenView PreResultMapMainScreenView
        {
            get { return _PreResultMapMainScreenView; }
            set
            {
                if (value != _PreResultMapMainScreenView)
                {
                    _PreResultMapMainScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _MainTabSelectedIndex;
        public int MainTabSelectedIndex
        {
            get { return _MainTabSelectedIndex; }
            set
            {
                if (value != _MainTabSelectedIndex)
                {
                    _MainTabSelectedIndex = value;
                    RaisePropertyChanged();

                    ChangedMainTabIndex();
                }
            }
        }

        public bool UsedDataTab { get; set; }

        private int _DataTabSelectedIndex;
        public int DataTabSelectedIndex
        {
            get { return _DataTabSelectedIndex; }
            set
            {
                if (value != _DataTabSelectedIndex)
                {
                    _DataTabSelectedIndex = value;
                    RaisePropertyChanged();

                    ChangedDataTabIndex();
                }
            }
        }

        public bool UsedResultMapTab { get; set; }
        private int _ResultMapTabSelectedIndex;
        public int ResultMapTabSelectedIndex
        {
            get { return _ResultMapTabSelectedIndex; }
            set
            {
                if (value != _ResultMapTabSelectedIndex)
                {
                    _ResultMapTabSelectedIndex = value;
                    RaisePropertyChanged();

                    ResultMapTabIndex();
                }
            }
        }



        private Dictionary<int, Guid> _MainTabIndexGuidDictionary = new Dictionary<int, Guid>();
        public Dictionary<int, Guid> MainTabIndexGuidDictionary
        {
            get { return _MainTabIndexGuidDictionary; }
            set
            {
                if (value != _MainTabIndexGuidDictionary)
                {
                    _MainTabIndexGuidDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Dictionary<int, Guid> _DataTabIndexGuidDictionary = new Dictionary<int, Guid>();
        public Dictionary<int, Guid> DataTabIndexGuidDictionary
        {
            get { return _DataTabIndexGuidDictionary; }
            set
            {
                if (value != _DataTabIndexGuidDictionary)
                {
                    _DataTabIndexGuidDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<int, Guid> _ResultMapTabIndexGuidDictionary = new Dictionary<int, Guid>();
        public Dictionary<int, Guid> ResultMapTabIndexGuidDictionary
        {
            get { return _ResultMapTabIndexGuidDictionary; }
            set
            {
                if (value != _ResultMapTabIndexGuidDictionary)
                {
                    _ResultMapTabIndexGuidDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ViewModel
        public VmDutEditorPage VmDutEditorPage { get; set; }
        public SequenceMakerVM SequenceMakerVM { get; set; }

        public IMainScreenViewModel PnpControlVM { get; set; }
        public ITempController TempController { get; set; }
        public IParamManager ParamManager { get; set; }

        public IProbingModule ProbingModule { get; set; }

        private EnumProbingState _ProbingStateEnum;
        public EnumProbingState ProbingStateEnum
        {
            get { return _ProbingStateEnum; }
            set
            {
                if (value != _ProbingStateEnum)
                {
                    _ProbingStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ILotOPModule LotOPModule { get; set; }

        private LotOPStateEnum _LotStateEnum;
        public LotOPStateEnum LotStateEnum
        {
            get { return _LotStateEnum; }
            set
            {
                if (value != _LotStateEnum)
                {
                    _LotStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ISubstrateInfo SubstrateInfo { get; set; }

        public IPhysicalInfo PhysicalInfo { get; set; }

        public Dictionary<int, IMainScreenViewModel> VMDictionary { get; set; }
        #endregion

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Init();

                LoggerManager.Debug($"[VMProberEmulator], InitViewModel() : Start");

                ComType = ProberEmulCommunicationMode.MANUAL;

                Container = ModuleResolver.ConfigureDependencies();
                Extensions_IModule.SetContainer(null, Container);
                Extensions_ICommand.SetStageContainer(null, Container);

                ParamManager = this.ParamManager();

                FileManager = this.FileManager() as FileManager;

                EventManager = this.EventManager() as EventManager;
                EventExecutor = this.EventExecutor() as EventExecutor;

                TesterCommunicationManager = this.TesterCommunicationManager() as TesterCommunicationManager;

                TCPIPModule = this.TCPIPModule() as TCPIPModule;

                CommandCollection = TCPIPModule.TesterComDriver.CommandCollection;

                CommandManager = this.CommandManager() as Command.CommandManager;

                StageSupervisorModule = this.StageSupervisor() as StageSupervisor;

                ResultMapManager = this.ResultMapManager() as ResultMapManager;

                SubstrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();
                PhysicalInfo = this.StageSupervisor().WaferObject.GetPhysInfo();

                #region Module

                TempController = this.TempController();
                ProbingModule = this.ProbingModule();
                LotOPModule = this.LotOPModule();

                #endregion

                DummyDataModule = new DummyDataModule();

                DummyDataModule.InitModule();

                this.StageSupervisor().WaferObject.GetSubsInfo().SlotIndex.Value = (DummyDataModule.dummyTCPIPQueries.SelectedSlot.Index + 1);

                //this.LotOPModule().LotInfo.HardwareAsseblyVerifed.ValueChangedEvent += HardwareAsseblyVerifed_ValueChangedEvent;
                //LotOPModule.LotInfo.LotProcessingVerifed.ValueChangedEvent += LotProcessingVerifed_ValueChangedEvent;

                LotStateEnum = LotOPModule.LotStateEnum;

                CheckCanLotStart();

                //// SET FOUP DATA

                //byte[] bytefoupinfos = this.LoaderController().GetBytesFoupObjects();

                //if (bytefoupinfos != null)
                //{
                //    // Deserialize
                //    object target;

                //    WrappedCasseteInfos foupinfos = null;

                //    SerializeManager.DeserializeFromByte(bytefoupinfos, out target, typeof(WrappedCasseteInfos));

                //    if (target != null)
                //    {
                //        foupinfos = target as WrappedCasseteInfos;

                //        DummyDataModule.dummyTCPIPQueries.Foups = foupinfos.Foups;

                //        if (DummyDataModule.dummyTCPIPQueries.Foups != null)
                //        {
                //            if (DummyDataModule.dummyTCPIPQueries.Foups.Count > 0)
                //            {
                //                DummyDataModule.dummyTCPIPQueries.SelectedFoup = DummyDataModule.dummyTCPIPQueries.Foups.First();

                //                if (DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots != null)
                //                {
                //                    DummyDataModule.dummyTCPIPQueries.SelectedSlot = DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots.First();
                //                    DummyDataModule.dummyTCPIPQueries.SelectedSlot.IsSelected = true;

                //                    //SelectedSlot.WaferState = EnumWaferState.UNPROCESSED;

                //                    foreach (var slot in DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots)
                //                    {
                //                        slot.WaferState = EnumWaferState.UNPROCESSED;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                //SetDummyRealAssign();

                AssignDummyQueryData();
                AssignDuumyInterruptData();

                //AssignElementID();

                MakeInterruputCommandlist();

                ConnectEventConditionFunc();

                MakeLotFlowScenario();

                UpdateSelectedSlotIndex();

                SequenceEngineManager = this.SequenceEngineManager() as SequenceEngineManager;

                //SequenceEngineManager.AddSpecificSequence(TCPIPModule, "TCPIP_THREAD");
                //SequenceEngineManager.AddSpecificSequence(EventExecutor, "EventExecutor_THREAD");

                SequenceEngineManager.RunSequences();

                SetDummyProbingSequecne();

                MakeIndexGuidDict();

                DBManager.Open();

                // DB 관련
                ParamManager.RegistElementToDB();

                //==> CSV 파일(DevParameterData, SysParameterData, CommonParameterData)을 읽어서 DB에 Update
                // EnableUpdateDBFile = True일 때 동작.
                ParamManager.SyncDBTableByCSV();

                //==> DB의 Meta data를 읽어 들여서 Element의 Meta Data 초기화
                ParamManager.LoadElementInfoFromDB();

                UsedDataTab = false;
                UsedResultMapTab = false;

                LoggerManager.Debug($"[VMProberEmulator], InitViewModel() : End");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        //private void SetDummyRealAssign()
        //{
        //    try
        //    {
        //        DummyDataModule.dummyTCPIPQueries.BinType.Value = TCPIPModule.TCPIPSysParam.EnumBinType.Value;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        private void MakeIndexGuidDict()
        {
            try
            {

                // MainTabIndexGuidDictionary
                if (MainTabIndexGuidDictionary == null)
                {
                    MainTabIndexGuidDictionary = new Dictionary<int, Guid>();
                }

                // BIN ANALYZE
                MainTabIndexGuidDictionary.Add(2, new Guid("4aa7dc92-1191-4c21-b3fa-ef9f78a2ce7a"));

                // RESULT MAP ANALYZE
                MainTabIndexGuidDictionary.Add(3, new Guid("2c30a55f-2c44-4451-9f54-3623edf02231"));


                // DataTabIndexGuidDictionary
                if (DataTabIndexGuidDictionary == null)
                {
                    DataTabIndexGuidDictionary = new Dictionary<int, Guid>();
                }

                // Dut Editor
                DataTabIndexGuidDictionary.Add(1, new Guid("78744426-ef1d-4624-a961-4a756669a9b7"));

                //// User Coordinate
                //DataTabIndexGuidDictionary.Add(2, new Guid("1b96aa21-1613-108a-71d6-9bce684a4dd0"));

                // Probing Seq
                DataTabIndexGuidDictionary.Add(3, new Guid("EC8FB998-222F-1E88-2C18-6DF6A742B3E9"));

                #region ResultMap

                if (ResultMapTabIndexGuidDictionary == null)
                {
                    ResultMapTabIndexGuidDictionary = new Dictionary<int, Guid>();
                }

                // STIF
                ResultMapTabIndexGuidDictionary.Add(0, new Guid("2c30a55f-2c44-4451-9f54-3623edf02231"));

                // E142
                ResultMapTabIndexGuidDictionary.Add(1, new Guid("96cbd1e2-869d-4dfa-be86-46ead2c635e3"));

                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private Guid GetUserCoordinateUI()
        {
            Guid retval = new Guid();

            try
            {
                Guid guid = new Guid("9b0d3a32-c44c-d98f-210e-20e65a1a3900");

                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();

                this.WaferAligner().IsNewSetup = true;
                this.PnPManager().GetCuiBtnParam(this.WaferAligner(), guid, out viewguid, out pnpsteps);

                if (pnpsteps.Count != 0)
                {
                    this.WaferAligner().ClearState();
                    this.WaferAligner().SetSetupState();
                    this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                }

                retval = viewguid;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private List<TCPIPDummyQueryData> GetDummyQueries(TCPIPQueryData reqeust)
        {
            List<TCPIPDummyQueryData> retval = new List<TCPIPDummyQueryData>();

            try
            {
                //RequestBase extensionquery = null;

                if (reqeust.ACKExtensionQueries != null && reqeust.ACKExtensionQueries.Querys.Count > 0)
                {
                    foreach (var item in reqeust.ACKExtensionQueries.Querys)
                    {
                        retval.Add((item as QueryHasAffix)?.Data as TCPIPDummyQueryData);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private List<object> GetDummyValues(string key)
        {
            List<object> retval = new List<object>();

            try
            {
                //List<string> keypropnames = null;
                List<DummyDataAssignBaseComonent> keypropnames = null;

                bool IsExist = DummyDataModule.DicDummyDatas.TryGetValue(key, out keypropnames);

                if (IsExist == true)
                {
                    if (DummyDataModule.dummyTCPIPQueries != null)
                    {
                        Type t = DummyDataModule.dummyTCPIPQueries.GetType();

                        PropertyInfo[] props = t.GetProperties();

                        foreach (var assigncomponent in keypropnames)
                        {
                            if (assigncomponent.AssignType == DummyDataAssignType.PROPERTY)
                            {
                                DummyDataAssignPropertyType propetytype = assigncomponent as DummyDataAssignPropertyType;

                                string[] propnamesplit = propetytype.Propetyname.Split('.');

                                if (propnamesplit.Length == 1)
                                {
                                    var prop = props.FirstOrDefault(p => p.Name == propetytype.Propetyname);

                                    if (prop != null)
                                    {
                                        retval.Add(prop.GetValue(DummyDataModule.dummyTCPIPQueries));
                                    }
                                }
                                else
                                {
                                    string last = propnamesplit.Last();

                                    PropertyInfo prop = null;
                                    PropertyInfo[] newprops = props;

                                    object tmpobj = null;
                                    object curobj = null;

                                    curobj = DummyDataModule.dummyTCPIPQueries;

                                    foreach (var ps in propnamesplit)
                                    {
                                        prop = newprops.FirstOrDefault(p => p.Name == ps);

                                        if (ps.Equals(last))
                                        {
                                            retval.Add(prop.GetValue(tmpobj));
                                        }
                                        else
                                        {
                                            tmpobj = prop.GetValue(curobj);

                                            if (tmpobj == null)
                                            {
                                                break;
                                            }

                                            newprops = tmpobj.GetType().GetProperties();
                                        }
                                    }
                                }
                            }
                            else if (assigncomponent.AssignType == DummyDataAssignType.FUNC)
                            {
                                DummyDataAssignFuncType dummyDataAssignPropertyType = assigncomponent as DummyDataAssignFuncType;

                                if (dummyDataAssignPropertyType.Func == null)
                                {
                                    retval.Add(null);
                                }
                                else
                                {
                                    retval.Add(dummyDataAssignPropertyType.Func());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //private List<object> GetDummyValues(string key)
        //{
        //    List<object> retval = new List<object>();

        //    try
        //    {
        //        //List<string> keypropnames = null;
        //        List<DummyDataAssignBaseComonent> keypropnames = null;

        //        bool IsExist = DummyDataModule.DicDummyDatas.TryGetValue(key, out keypropnames);

        //        if (IsExist == true)
        //        {
        //            Type t = DummyDataModule.dummyTCPIPQueries.GetType();

        //            PropertyInfo[] props = t.GetProperties();

        //            foreach (var assigncomponent in keypropnames)
        //            {
        //                if (assigncomponent.AssignType == DummyDataAssignType.PROPERTY)
        //                {
        //                    DummyDataAssignPropertyType propetytype = assigncomponent as DummyDataAssignPropertyType;

        //                    foreach (var item in propetytype)
        //                    {
        //                        string[] propnamesplit = item.Split('.');

        //                        if (propnamesplit.Length == 1)
        //                        {
        //                            var prop = props.FirstOrDefault(p => p.Name == item);

        //                            if (prop != null)
        //                            {
        //                                retval.Add(prop.GetValue(DummyDataModule.dummyTCPIPQueries));
        //                            }
        //                        }
        //                        else
        //                        {
        //                            string last = propnamesplit.Last();

        //                            PropertyInfo prop = null;
        //                            PropertyInfo[] newprops = props;

        //                            object tmpobj = null;
        //                            object curobj = null;
        //                            object lastobj = null;

        //                            curobj = DummyDataModule.dummyTCPIPQueries;

        //                            foreach (var ps in propnamesplit)
        //                            {
        //                                prop = newprops.FirstOrDefault(p => p.Name == ps);

        //                                if (ps.Equals(last))
        //                                {
        //                                    retval.Add(prop.GetValue(tmpobj));
        //                                }
        //                                else
        //                                {
        //                                    tmpobj = prop.GetValue(curobj);

        //                                    if (tmpobj == null)
        //                                    {
        //                                        break;
        //                                    }

        //                                    newprops = tmpobj.GetType().GetProperties();
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else if (keypropnames.AssignType == DummyDataAssignType.FUNC)
        //                {
        //                    DummyDataAssignFuncType dummyDataAssignPropertyType = keypropnames as DummyDataAssignFuncType;

        //                    foreach (var func in dummyDataAssignPropertyType.AssignFuncs)
        //                    {
        //                        retval.Add(func());
        //                    }
        //                }

        //            }



















        //            if (keypropnames.AssignType == DummyDataAssignType.PROPERTY)
        //            {
        //                DummyDataAssignPropertyType dummyDataAssignPropertyType = keypropnames as DummyDataAssignPropertyType;

        //                foreach (var item in dummyDataAssignPropertyType.AssignProperties)
        //                {
        //                    string[] propnamesplit = item.Split('.');

        //                    if (propnamesplit.Length == 1)
        //                    {
        //                        var prop = props.FirstOrDefault(p => p.Name == item);

        //                        if (prop != null)
        //                        {
        //                            retval.Add(prop.GetValue(DummyDataModule.dummyTCPIPQueries));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        string last = propnamesplit.Last();

        //                        PropertyInfo prop = null;
        //                        PropertyInfo[] newprops = props;

        //                        object tmpobj = null;
        //                        object curobj = null;
        //                        object lastobj = null;

        //                        curobj = DummyDataModule.dummyTCPIPQueries;

        //                        foreach (var ps in propnamesplit)
        //                        {
        //                            prop = newprops.FirstOrDefault(p => p.Name == ps);

        //                            if (ps.Equals(last))
        //                            {
        //                                retval.Add(prop.GetValue(tmpobj));
        //                            }
        //                            else
        //                            {
        //                                tmpobj = prop.GetValue(curobj);

        //                                if (tmpobj == null)
        //                                {
        //                                    break;
        //                                }

        //                                newprops = tmpobj.GetType().GetProperties();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else if (keypropnames.AssignType == DummyDataAssignType.FUNC)
        //            {
        //                DummyDataAssignFuncType dummyDataAssignPropertyType = keypropnames as DummyDataAssignFuncType;

        //                foreach (var func in dummyDataAssignPropertyType.AssignFuncs)
        //                {
        //                    retval.Add(func());
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //private void AssignElementID()
        //{
        //    try
        //    {
        //        // DB가 갖고 있는 ElementID와 동일한 값을 할당하여 테스트 진행.

        //        //DummyDataModule.dummyTCPIPQueries.SetTemp.ElementID = 1015;
        //        //DummyDataModule.dummyTCPIPQueries.OverDrive.ElementID = 893;
        //        //DummyDataModule.dummyTCPIPQueries.BinType.ElementID = 10000161;
        //        //DummyDataModule.dummyTCPIPQueries.CurTemp.ElementID = 30000001;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private void AssignDuumyInterruptData()
        {
            try
            {
                foreach (var item in TCPIPModule.SubscribeRecipeParam)
                {
                    TCPIPEventParam eventparam = item.Parameter as TCPIPEventParam;
                    TCPIPQueryData tCPIPQueryData = eventparam.Response as TCPIPQueryData;

                    List<TCPIPDummyQueryData> dummyQueries = null;
                    List<object> dummyvalues = null;

                    if (tCPIPQueryData != null)
                    {
                        string queryname = tCPIPQueryData.GetType().FullName;

                        dummyQueries = GetDummyQueries(tCPIPQueryData);
                        dummyvalues = GetDummyValues(queryname);

                        AssignDummyData(dummyQueries, dummyvalues);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AssignDummyData(List<TCPIPDummyQueryData> dummyQueries, List<object> dummyValues)
        {
            try
            {
                if (dummyQueries != null && dummyValues != null)
                {
                    if (dummyQueries.Count == dummyValues.Count)
                    {
                        for (int i = 0; i < dummyQueries.Count; i++)
                        {
                            if (dummyQueries[i] != null)
                            {
                                if (dummyValues[i] != null)
                                {
                                    dummyQueries[i].DummyData = dummyValues[i].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void AssignDummyQueryData()
        {
            try
            {
                foreach (var requestset in TCPIPModule.RequestSetList)
                {
                    TCPIPQueryData tCPIPQueryData = requestset.Request as TCPIPQueryData;

                    if (tCPIPQueryData != null)
                    {
                        List<TCPIPDummyQueryData> dummyqueries = null;
                        List<object> duumyvalues = null;

                        string queryname = requestset.Name;

                        dummyqueries = GetDummyQueries(tCPIPQueryData);
                        duumyvalues = GetDummyValues(queryname);

                        AssignDummyData(dummyqueries, duumyvalues);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void MakeInterruputCommandlist()
        {
            try
            {

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (InterruptCommandlist == null)
                    {
                        //InterruptCommandlist = new ObservableCollection<string>();
                        InterruptCommandlist = new ObservableCollection<SimpleInterruptCommandRecipe>();
                    }
                    else
                    {
                        InterruptCommandlist.Clear();
                    }
                });

                foreach (var item in TCPIPModule.SubscribeRecipeParam)
                {
                    TCPIPEventParam eventparam = item.Parameter as TCPIPEventParam;
                    TCPIPQueryData querydata = eventparam.Response as TCPIPQueryData;

                    if (querydata.CommandType == EnumTCPIPCommandType.INTERRUPT)
                    {
                        string response = eventparam.Response.GetRequestResult()?.ToString();

                        if (string.IsNullOrEmpty(response) == false)
                        {
                            SimpleInterruptCommandRecipe tmp = new SimpleInterruptCommandRecipe();

                            tmp.EventFullName = item.EventFullName;
                            tmp.Response = response;

                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                InterruptCommandlist.Add(tmp);
                            });
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private string Response_MoveToNextIndexDone()
        {
            string retval = string.Empty;
            bool NeedAssign = false;

            try
            {
                //int probingseqindex = DummyDataModule.dummyTCPIPQueries.ProbingSequence.IndexOf(DummyDataModule.dummyTCPIPQueries.SelectedCoordiante);
                int probingseqindex = DummyDataModule.dummyTCPIPQueries.ProbingSequence.IndexOf(ProbingModule.ProbingLastMIndex);

                if (probingseqindex >= 0)
                {
                    // 남아 있는 시퀀스 확인
                    if (DummyDataModule.dummyTCPIPQueries.ProbingSequence.Count - 1 > probingseqindex)
                    {
                        //DummyDataModule.dummyTCPIPQueries.SelectedCoordiante = DummyDataModule.dummyTCPIPQueries.ProbingSequence[probingseqindex + 1];
                        ProbingModule.SetLastProbingMIndex(DummyDataModule.dummyTCPIPQueries.ProbingSequence[probingseqindex + 1]);

                        NeedAssign = true;
                    }
                    else if (DummyDataModule.dummyTCPIPQueries.ProbingSequence.Count - 1 == probingseqindex) // 마지막 인덱스
                    {
                        NeedAssign = false;
                    }
                }

                //if (comtype == ProberEmulCommunicationMode.MANUAL)
                //{

                //}
                //else if (comtype == ProberEmulCommunicationMode.FLOW)
                //{

                //}

                // 실제 동작을 수행하지 않기 때문에, 동작 수행의 분기별로, 이벤트를 달리 만들어줘야 한다.
                // Case 1) 시퀀스 남아 있는 경우, ProbingZUpProcessEvent 이벤트
                // Case 2) 마지막 시퀀스인 경우, DontRemainSequenceEvent 이벤트

                //(ProbingModule as Probing).StateTransition(EnumProbingState.ZDN);
                //ProbingStateEnum = ProbingModule.ProbingStateEnum;

                if (NeedAssign == true)
                {
                    AssignDuumyInterruptData();

                    retval = typeof(ProbingZUpProcessEvent).FullName;
                }
                else
                {
                    retval = typeof(DontRemainSequenceEvent).FullName;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string Response_UnloadWaferDone()
        {
            string retval = string.Empty;

            try
            {
                ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                // 프로빙 시퀀스를 처음으로 변경.
                //DummyDataModule.dummyTCPIPQueries.SelectedCoordiante = DummyDataModule.dummyTCPIPQueries.ProbingSequence.FirstOrDefault();
                ProbingModule.SetLastProbingMIndex(DummyDataModule.dummyTCPIPQueries.ProbingSequence.FirstOrDefault());

                // 현재 로드 된 웨이퍼의 상태를 Processed로 변경
                DummyDataModule.dummyTCPIPQueries.SelectedSlot.WaferState = EnumWaferState.PROCESSED;
                substrateInfo.ProbingEndTime = DateTime.Now.ToLocalTime();

                Thread.Sleep(100);

                substrateInfo.UnloadingTime = DateTime.Now.ToLocalTime();

                //// DIE의 상태를 원상태로 변경.
                //var dies = this.GetParam_Wafer().GetSubsInfo().DIEs;

                //foreach (var dev in dies)
                //{
                //    if (dev.DieType.Value == DieTypeEnum.TEST_DIE)
                //    {
                //        int rndbin = 0;

                //        //dev.State.Value = DieStateEnum.TESTED;

                //        // TODO : Random result

                //        // 0 ~ 1
                //        int resultrnd = random.Next(0, 2);

                //        dev.CurTestHistory.TestResult.Value = (resultrnd == 0) ? TestState.MAP_STS_PASS : TestState.MAP_STS_FAIL;

                //        if (dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_PASS)
                //        {
                //            resultrnd = random.Next(Passminval, Passmaxval + 1);

                //            dev.CurTestHistory.BinCode.Value = resultrnd;

                //            dev.State.Value = DieStateEnum.TESTED;
                //        }
                //        else if (dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_FAIL)
                //        {
                //            resultrnd = random.Next(Failminval, Failmaxval + 1);

                //            dev.CurTestHistory.BinCode.Value = resultrnd;

                //            dev.State.Value = DieStateEnum.TESTED;
                //        }
                //        else
                //        {
                //            dev.CurTestHistory.BinCode.Value = rndbin;
                //        }
                //    }
                //    else if (dev.DieType.Value == DieTypeEnum.MARK_DIE)
                //    {
                //        dev.CurTestHistory.BinCode.Value = edgeorcornerbincode;
                //    }
                //    else if (dev.DieType.Value == DieTypeEnum.NOT_EXIST)
                //    {
                //    }
                //}

                // 남은 웨이퍼가 존재하는 경우, WaferStart 순서로 변경하기 위함.
                var nextwafer = GetNextWafer();

                if (nextwafer != null)
                {
                    ChangeOrderNo = 1;
                }
                else
                {
                    substrateInfo.CurPassedDieCount.Value = 0;
                    substrateInfo.CurFailedDieCount.Value = 0;
                    substrateInfo.CurTestedDieCount.Value = 0;

                    substrateInfo.Yield = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string Response_ZupDone()
        {
            string retval = string.Empty;

            try
            {
                (ProbingModule as Probing).StateTransition(EnumProbingState.ZUP);
                ProbingStateEnum = ProbingModule.ProbingStateEnum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string RemoveTerminator(string recv_data)
        {
            string terminator = Terminator;
            string retStr = recv_data;

            try
            {
                if (terminator != null && terminator != string.Empty)
                {
                    retStr = retStr.Replace(terminator, "");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retStr;
        }

        private string GetLastCommand(string key)
        {
            string retval = string.Empty;

            try
            {
                CommandCollection = TCPIPModule.TesterComDriver.CommandCollection;

                // 마지막 받은 커맨드로부터 거꾸로 탐색
                foreach (var item in CommandCollection.Reverse())
                {
                    if (item.Message.Contains(key))
                    {
                        int startindex = item.Message.IndexOf(key);

                        retval = item.Message.Substring(startindex, item.Message.Length - startindex);
                        retval = RemoveTerminator(retval);

                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string Response_BINCodeDone()
        {
            string retval = string.Empty;

            try
            {
                string BinInfo = string.Empty;

                BinInfo = GetLastCommand("BN");

                // TODO : BIN CODE 분석 테스트

                if (string.IsNullOrEmpty(BinInfo) == false)
                {
                    GetCalcualtePassFailYield action = new GetCalcualtePassFailYield();

                    action.Argument = BinInfo;

                    //MachineIndex CurrentMI = this.DummyDataModule.dummyTCPIPQueries.SelectedCoordiante;
                    //this.ProbingModule().SetLastProbingMIndex(CurrentMI);

                    action.Run();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //private string Response_HardwareAssemblyVerifyDone()
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        string cmdName = "HWAV";

        //        string HardwareAsseblyVerifyInfo = string.Empty;

        //        HardwareAsseblyVerifyInfo = GetLastCommand(cmdName);

        //        // Seperate : Command & Value

        //        string seperatestr = HardwareAsseblyVerifyInfo.Replace(cmdName, string.Empty);

        //        var isNumeric = int.TryParse(seperatestr, out int n);

        //        if(isNumeric == true)
        //        {
        //            if(n == 0)
        //            {
        //                this.LotOPModule.LotInfo.HardwareAsseblyVerifed.Value = false;
        //            }
        //            else if(n == 1)
        //            {
        //                this.LotOPModule.LotInfo.HardwareAsseblyVerifed.Value = true;
        //            }
        //            else
        //            {
        //                this.LotOPModule.LotInfo.HardwareAsseblyVerifed.Value = false;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private string Response_LotProcessingVerifyDone()
        {
            string retval = string.Empty;

            try
            {
                string cmdName = "LPV";

                string LotProcessingVerifyInfo = string.Empty;

                LotProcessingVerifyInfo = GetLastCommand(cmdName);

                // Seperate : Command & Value

                string seperatestr = LotProcessingVerifyInfo.Replace(cmdName, string.Empty);

                var isNumeric = int.TryParse(seperatestr, out int n);

                if (isNumeric == true)
                {
                    //if (n == 0)
                    //{
                    //    this.LotOPModule.LotInfo.LotProcessingVerifed.Value = false;
                    //}
                    //else if (n == 1)
                    //{
                    //    this.LotOPModule.LotInfo.LotProcessingVerifed.Value = true;
                    //}
                    //else
                    //{
                    //    this.LotOPModule.LotInfo.LotProcessingVerifed.Value = false;
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //private string Response_OnWaferInfoDone()
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        string BinInfo = string.Empty;

        //        BinInfo = GetLastCommand("O");
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;

        //}

        private IElement GetDummyElement(int ID)
        {
            IElement element = null;

            try
            {
                Type t = DummyDataModule.dummyTCPIPQueries.GetType();

                PropertyInfo[] props = t.GetProperties();

                bool IsFound = false;

                foreach (var prop in props)
                {
                    if (prop.PropertyType.Name.Contains("Element"))
                    {
                        object o = prop.GetValue(DummyDataModule.dummyTCPIPQueries);

                        element = o as IElement;

                        if (element != null)
                        {
                            int elementID = element.ElementID;

                            if (elementID == ID)
                            {
                                IsFound = true;
                                break;
                            }
                        }
                    }
                }

                if (IsFound == false)
                {
                    element = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return element;
        }
        //private string Response_DWDone()
        //{
        //    string retval = string.Empty;
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        string dwstr = string.Empty;

        //        dwstr = GetLastCommand("DW");

        //        DRDWValueValidation dRDWValueValidation = new DRDWValueValidation();
        //        dRDWValueValidation.Argument = dwstr;

        //        if (dRDWValueValidation.Run() == EventCodeEnum.NONE)
        //        {
        //            ITCPIP tcpip = this.TCPIPModule();

        //            DataIDValue dataIDValue = tcpip.GetIDandValue(dwstr);

        //            DRDWConnectorBase dRDWConnectorBase = tcpip.GetDRDWConnector(dataIDValue.ID);

        //            IElement element = null;

        //            element = GetDummyElement(dRDWConnectorBase.ID);

        //            if (element != null)
        //            {
        //                if (dRDWConnectorBase.ChangeFormatRequest != null)
        //                {
        //                    // CONVERT FORMAT
        //                    dRDWConnectorBase.ChangeFormatRequest.Argument = dataIDValue.value;

        //                    retVal = dRDWConnectorBase.ChangeFormatRequest.Run();

        //                    if (retVal == EventCodeEnum.NONE)
        //                    {
        //                        object setvalue = dRDWConnectorBase.ChangeFormatRequest.Result;

        //                        retVal = element.SetValue(setvalue);

        //                        element.SetOriginValue();
        //                    }
        //                }
        //                else
        //                {
        //                    retVal = element.SetValue(dataIDValue.value);
        //                }

        //                if (retVal == EventCodeEnum.NONE || retVal == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
        //                {
        //                    this.EventManager().RaisingEvent(typeof(PassDWCommandEvent).FullName);
        //                }
        //                else
        //                {
        //                    this.EventManager().RaisingEvent(typeof(FailDWCommandEvent).FullName);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public void ConnectEventConditionFunc()
        {
            // EventFullName과 원하는 Func 연결

            try
            {
                EventProcessBase subscriberecipe = null;

                // Response_ZupDone

                subscriberecipe = TCPIPModule.SubscribeRecipeParam.FirstOrDefault(x => x.EventFullName == typeof(Response_ZupDone).FullName);

                if (subscriberecipe != null)
                {
                    TCPIPEventParam tcpipevent = (subscriberecipe.Parameter as TCPIPEventParam);

                    if (tcpipevent != null)
                    {
                        TCPIPDummyActionData duumyaction = (tcpipevent.Response as TCPIPDummyActionData);

                        if (duumyaction != null)
                        {
                            duumyaction.EventConditionFunc = Response_ZupDone;
                        }
                    }
                }

                // Response_MoveToNextIndexDone

                subscriberecipe = TCPIPModule.SubscribeRecipeParam.FirstOrDefault(x => x.EventFullName == typeof(Response_MoveToNextIndexDone).FullName);

                if (subscriberecipe != null)
                {
                    TCPIPEventParam tcpipevent = (subscriberecipe.Parameter as TCPIPEventParam);

                    if (tcpipevent != null)
                    {
                        TCPIPDummyActionData duumyaction = (tcpipevent.Response as TCPIPDummyActionData);

                        if (duumyaction != null)
                        {
                            duumyaction.EventConditionFunc = Response_MoveToNextIndexDone;
                        }
                    }
                }

                // Response_UnloadWaferDone

                subscriberecipe = TCPIPModule.SubscribeRecipeParam.FirstOrDefault(x => x.EventFullName == typeof(Response_UnloadWaferDone).FullName);

                if (subscriberecipe != null)
                {
                    TCPIPEventParam tcpipevent = (subscriberecipe.Parameter as TCPIPEventParam);

                    if (tcpipevent != null)
                    {
                        TCPIPDummyActionData duumyaction = (tcpipevent.Response as TCPIPDummyActionData);

                        if (duumyaction != null)
                        {
                            duumyaction.EventConditionFunc = Response_UnloadWaferDone;
                        }
                    }
                }

                // Response_BINCodeDone

                subscriberecipe = TCPIPModule.SubscribeRecipeParam.FirstOrDefault(x => x.EventFullName == typeof(Response_BINCodeDone).FullName);

                if (subscriberecipe != null)
                {
                    TCPIPEventParam tcpipevent = (subscriberecipe.Parameter as TCPIPEventParam);

                    if (tcpipevent != null)
                    {
                        TCPIPDummyActionData duumyaction = (tcpipevent.Response as TCPIPDummyActionData);

                        if (duumyaction != null)
                        {
                            duumyaction.EventConditionFunc = Response_BINCodeDone;
                        }
                    }
                }

                //// Response_HardwareAssemblyVerifyDone

                //subscriberecipe = TCPIPModule.SubscribeRecipeParam.FirstOrDefault(x => x.EventFullName == typeof(Response_HardwareAssemblyVerifyDone).FullName);

                //if (subscriberecipe != null)
                //{
                //    TCPIPEventParam tcpipevent = (subscriberecipe.Parameter as TCPIPEventParam);

                //    if (tcpipevent != null)
                //    {
                //        TCPIPDummyActionData duumyaction = (tcpipevent.Response as TCPIPDummyActionData);

                //        if (duumyaction != null)
                //        {
                //            duumyaction.EventConditionFunc = Response_HardwareAssemblyVerifyDone;
                //        }
                //    }
                //}

                // Response_LotProcessingVerifyDone

                subscriberecipe = TCPIPModule.SubscribeRecipeParam.FirstOrDefault(x => x.EventFullName == typeof(Response_LotProcessingVerifyDone).FullName);

                if (subscriberecipe != null)
                {
                    TCPIPEventParam tcpipevent = (subscriberecipe.Parameter as TCPIPEventParam);

                    if (tcpipevent != null)
                    {
                        TCPIPDummyActionData duumyaction = (tcpipevent.Response as TCPIPDummyActionData);

                        if (duumyaction != null)
                        {
                            duumyaction.EventConditionFunc = Response_LotProcessingVerifyDone;
                        }
                    }
                }

                //// Response_DWDone

                //subscriberecipe = TCPIPModule.SubscribeRecipeParam.FirstOrDefault(x => x.EventFullName == typeof(Response_DWDone).FullName);

                //if (subscriberecipe != null)
                //{
                //    TCPIPEventParam tcpipevent = (subscriberecipe.Parameter as TCPIPEventParam);

                //    if (tcpipevent != null)
                //    {
                //        TCPIPDummyActionData duumyaction = (tcpipevent.Response as TCPIPDummyActionData);

                //        if (duumyaction != null)
                //        {
                //            duumyaction.EventConditionFunc = Response_DWDone;
                //        }
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #region Command



        private RelayCommand<object> _LotStateChangedCommand;
        public ICommand LotStateChangedCommand
        {
            get
            {
                if (null == _LotStateChangedCommand) _LotStateChangedCommand = new RelayCommand<object>(LotStateChangedCommandFunc);
                return _LotStateChangedCommand;
            }
        }

        private void LotStateChangedCommandFunc(object obj)
        {
            try
            {
                if ((LotOPModule as LotOPModule).LotStateEnum != LotStateEnum)
                {
                    (LotOPModule as LotOPModule).StateTransition(LotStateEnum);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _WaferMapApplyCommand;
        public ICommand WaferMapApplyCommand
        {
            get
            {
                if (null == _WaferMapApplyCommand) _WaferMapApplyCommand = new RelayCommand<object>(WaferMapApplyCommandFunc);
                return _WaferMapApplyCommand;
            }
        }

        private void WaferMapApplyCommandFunc(object obj)
        {
            try
            {
                // TODO : 변경 된 데이터 기반으로 맵 데이터 생성 필요


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private Guid GetViewGuid(Dictionary<int, Guid> dict, int key)
        {
            Guid retval = default(Guid);

            try
            {
                dict.TryGetValue(key, out retval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task ChangedMainTabIndex()
        {
            try
            {
                // Data Tab에서 옮겨온 경우
                // Data Tab이 갖고 있던 View의 Cleanup 호출 및 

                IMainScreenView preView = null;
                IMainScreenView targetView = null;

                if (UsedResultMapTab == true)
                {
                    preView = _PreResultMapMainScreenView;
                    targetView = this.ResultMapTabScreenView;
                    preView = this.ResultMapTabScreenView;
                }

                if (UsedDataTab == true)
                {
                    preView = _PreDataMainScreenView;
                    targetView = this.DataTabScreenView;
                    preView = this.DataTabScreenView;
                }

                if (preView != null)
                {
                    IMainScreenViewModel PrevVm = null;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if ((preView as UserControl).DataContext is IMainScreenViewModel)
                        {
                            PrevVm = (preView as UserControl).DataContext as IMainScreenViewModel;
                        }
                    });

                    if (PrevVm != null)
                    {
                        await PrevVm.Cleanup(EventCodeEnum.NONE);
                    }

                    targetView = null;
                    this.ViewModelManager().SetMainScreenView(null);
                }

                if (UsedDataTab)
                {
                    UsedDataTab = false;
                }

                if (UsedResultMapTab)
                {
                    UsedResultMapTab = false;
                }

                // Data Tab으로 변경 시
                if (MainTabSelectedIndex == 1)
                {
                    ChangedDataTabIndex();

                    //// 만약, Data Tab이 PreView를 갖고 있다면, 변경해줘야 됨.

                    //if (_PreDataMainScreenView != null)
                    //{
                    //    await this.ViewModelManager().ViewTransitionAsync(_PreDataMainScreenView.ViewGUID);
                    //    this.DataTabScreenView = this.ViewModelManager().MainScreenView;
                    //}
                }
                // Result Map Tab으로 변경 시
                else if (MainTabSelectedIndex == 3)
                {
                    ResultMapTabIndex();
                }
                else
                {
                    Guid guid = default(Guid);
                    guid = GetViewGuid(MainTabIndexGuidDictionary, MainTabSelectedIndex);

                    if (this.MainTabScreenView != null)
                    {
                        _PreMainScreenView = this.MainTabScreenView;

                        IMainScreenViewModel PrevVm = null;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if ((_PreMainScreenView as UserControl).DataContext is IMainScreenViewModel)
                            {
                                PrevVm = (_PreMainScreenView as UserControl).DataContext as IMainScreenViewModel;
                            }
                        });

                        if (PrevVm != null)
                        {
                            await PrevVm.Cleanup(EventCodeEnum.NONE);
                        }

                        this.MainTabScreenView = null;
                        this.ViewModelManager().SetMainScreenView(null);
                    }

                    if (guid != default(Guid))
                    {
                        // 등록 되어 있는 경우
                        await this.ViewModelManager().ViewTransitionAsync(guid);
                        this.MainTabScreenView = this.ViewModelManager().MainScreenView;
                    }
                    else
                    {
                        // 등록 되어 있지 않은 경우
                    }
                }

                if (MainTabSelectedIndex == 0)
                {
                    PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ChangedDataTabIndex()
        {
            try
            {
                Guid guid = default(Guid);

                if (DataTabSelectedIndex == 2)
                {
                    guid = GetUserCoordinateUI();
                }
                else
                {
                    guid = GetViewGuid(DataTabIndexGuidDictionary, DataTabSelectedIndex);
                }

                if (this.DataTabScreenView != null)
                {
                    _PreDataMainScreenView = this.DataTabScreenView;

                    IMainScreenViewModel PrevVm = null;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if ((_PreDataMainScreenView as UserControl).DataContext is IMainScreenViewModel)
                        {
                            PrevVm = (_PreDataMainScreenView as UserControl).DataContext as IMainScreenViewModel;
                        }
                    });

                    if (PrevVm != null)
                    {
                        await PrevVm.Cleanup(EventCodeEnum.NONE);
                    }

                    this.DataTabScreenView = null;
                    this.ViewModelManager().SetMainScreenView(null);
                }

                if (guid != default(Guid))
                {
                    // 등록 되어 있는 경우
                    await this.ViewModelManager().ViewTransitionAsync(guid);
                    this.DataTabScreenView = this.ViewModelManager().MainScreenView;
                }
                else
                {
                    // 등록 되어 있지 않은 경우
                }

                UsedDataTab = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ResultMapTabIndex()
        {
            try
            {
                Guid guid = default(Guid);

                guid = GetViewGuid(ResultMapTabIndexGuidDictionary, ResultMapTabSelectedIndex);

                if (this.ResultMapTabScreenView != null)
                {
                    _PreResultMapMainScreenView = this.ResultMapTabScreenView;

                    IMainScreenViewModel PrevVm = null;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if ((_PreResultMapMainScreenView as UserControl).DataContext is IMainScreenViewModel)
                        {
                            PrevVm = (_PreResultMapMainScreenView as UserControl).DataContext as IMainScreenViewModel;
                        }
                    });

                    if (PrevVm != null)
                    {
                        await PrevVm.Cleanup(EventCodeEnum.NONE);
                    }

                    this.ResultMapTabScreenView = null;
                    this.ViewModelManager().SetMainScreenView(null);
                }

                if (guid != default(Guid))
                {
                    // 등록 되어 있는 경우
                    await this.ViewModelManager().ViewTransitionAsync(guid);
                    this.ResultMapTabScreenView = this.ViewModelManager().MainScreenView;
                }
                else
                {
                    // 등록 되어 있지 않은 경우
                }

                UsedResultMapTab = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private AsyncCommand<object> _TabControlChangedCommand;
        //public IAsyncCommand TabControlChangedCommand
        //{
        //    get
        //    {
        //        if (null == _TabControlChangedCommand) _TabControlChangedCommand = new AsyncCommand<object>(TabControlChangedCommandFunc);
        //        return _TabControlChangedCommand;
        //    }
        //}


        //private async Task TabControlChangedCommandFunc(object arg)
        //{
        //    try
        //    {
        //        if (arg is SelectionChangedEventArgs)
        //        {
        //            SelectionChangedEventArgs e = (arg as SelectionChangedEventArgs);

        //            if (e.Source is TabControl)
        //            {
        //                string TabControlName = string.Empty;

        //                Application.Current.Dispatcher.Invoke((Action)delegate
        //                {
        //                    TabControlName = (e.Source as TabControl).Name;
        //                });

        //                Guid guid = default(Guid);

        //                if (TabControlName == "MainTabControl")
        //                {
        //                    guid = GetViewGuid(MainTabIndexGuidDictionary, MainTabSelectedIndex);
        //                }
        //                else if (TabControlName == "DataTabControl")
        //                {
        //                    if (DataTabSelectedIndex == 2)
        //                    {
        //                        guid = GetUserCoordinateUI();
        //                    }
        //                    else
        //                    {
        //                        guid = GetViewGuid(DataTabIndexGuidDictionary, DataTabSelectedIndex);
        //                    }
        //                }

        //                if (this.MainTabScreenView != null)
        //                {
        //                    _PreMainScreenView = this.MainTabScreenView;
        //                }

        //                if (this.DataTabScreenView != null)
        //                {
        //                    _PreDataMainScreenView = this.DataTabScreenView;
        //                }

        //                if (guid != default(Guid))
        //                {
        //                    await this.ViewModelManager().ViewTransitionAsync(guid);

        //                    if (TabControlName == "MainTabControl")
        //                    {
        //                        this.MainTabScreenView = this.ViewModelManager().MainScreenView;
        //                    }
        //                    else if (TabControlName == "DataTabControl")
        //                    {
        //                        this.DataTabScreenView = this.ViewModelManager().MainScreenView;
        //                    }
        //                }
        //                else
        //                {
        //                    // 등록되지 않은 View로 변경되는 경우
        //                    PageSwitching(TabControlName);
        //                }

        //                if (TabControlName == "MainTabControl")
        //                {
        //                    if(MainTabSelectedIndex == 1)
        //                    {
        //                        LocatedDataTab = true;
        //                    }
        //                    else
        //                    {
        //                        LocatedDataTab = false;
        //                    }

        //                    if (MainTabSelectedIndex == 0)
        //                    {
        //                        this.PageSwitched();
        //                    }
        //                }
        //                else if (TabControlName == "DataTabControl")
        //                {
        //                    LocatedDataTab = true;
        //                }

        //                if(this.DummyDataModule.dummyTCPIPQueries.SelectedCoordiante == null)
        //                {
        //                    SetDummyProbingSequecne();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private async Task PageSwitching(string controlName)
        //{
        //    try
        //    {
        //        IMainScreenView view = null;
        //        Guid ViewGuid = default(Guid);

        //        if (controlName == "MainTabControl")
        //        {
        //            if(LocatedDataTab == true)
        //            {
        //                // DATA -> MAIN
        //                view = DataTabScreenView;
        //            }
        //            else
        //            {
        //                // MAIN -> MAIN
        //                view = MainTabScreenView;
        //            }

        //            if (MainTabSelectedIndex == 1)
        //            {
        //                if (PreDataMainScreenView != null)
        //                {
        //                    ViewGuid = PreDataMainScreenView.ViewGUID;
        //                }
        //            }
        //            else
        //            {
        //                if (PreMainScreenView != null)
        //                {
        //                    ViewGuid = PreMainScreenView.ViewGUID;
        //                }
        //            }
        //        }
        //        else if (controlName == "DataTabControl")
        //        {
        //            view = DataTabScreenView;

        //            if (PreDataMainScreenView != null)
        //            {
        //                ViewGuid = PreDataMainScreenView.ViewGUID;
        //            }
        //        }

        //        if (view != null)
        //        {
        //            IMainScreenViewModel PrevVm = null;

        //            Application.Current.Dispatcher.Invoke(() =>
        //            {
        //                if ((view as UserControl).DataContext is IMainScreenViewModel)
        //                {
        //                    PrevVm = (view as UserControl).DataContext as IMainScreenViewModel;
        //                }
        //            });

        //            if (PrevVm != null)
        //            {
        //                await PrevVm.Cleanup(EventCodeEnum.NONE);
        //            }

        //            view = null;
        //            this.ViewModelManager().SetMainScreenView(null);
        //        }

        //        if (ViewGuid != default(Guid))
        //        {
        //            await this.ViewModelManager().ViewTransitionAsync(ViewGuid);
        //            view = this.ViewModelManager().MainScreenView;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        //private void MainTabChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    int tabItem = ((sender as TabControl)).SelectedIndex;
        //    if (e.Source is TabControl) // This is a soultion of those problem.
        //    {
        //        switch (tabItem)
        //        {
        //            //case 0:    // Chatting
        //            //    Debug.WriteLine("Tab: Chatting");
        //            //    if (MainChatList.Items.Count > 0)
        //            //    {
        //            //        MainChatList.SelectedIndex = MainChatList.Items.Count - 1;
        //            //        MainChatList.ScrollIntoView(MainChatList.Items[MainChatList.Items.Count - 1]);
        //            //    }
        //            //    break;
        //            //case 1:    // Users
        //            //    Debug.WriteLine("Tab: Users");
        //            //    break;
        //            //case 2:    // Friends
        //            //    Debug.WriteLine("Tab: Friends");
        //            //    this.OnFriendTabActive();
        //            //    break;
        //            //default:
        //            //    Debug.WriteLine("Tab: " + tabItem);
        //            //    break;
        //        }
        //    }
        //}

        //private RelayCommand<object> _BinTypeSelectionChangedCommand;
        //public ICommand BinTypeSelectionChangedCommand
        //{
        //    get
        //    {
        //        if (null == _BinTypeSelectionChangedCommand) _BinTypeSelectionChangedCommand = new RelayCommand<object>(BinTypeSelectionChangedCommandFunc);
        //        return _BinTypeSelectionChangedCommand;
        //    }
        //}

        //private void BinTypeSelectionChangedCommandFunc(object param)
        //{
        //    try
        //    {
        //        //string selectedtypeStr = string.Empty;

        //        //ComboBox combobox = (param as ComboBox);

        //        //Type EnumType = null;

        //        //if ((combobox).ItemsSource != null)
        //        //{
        //        //    EnumType = combobox.SelectedItem.GetType();

        //        //    selectedtypeStr = EnumType.ToString();
        //        //}

        //        TCPIPModule.TCPIPSysParam.EnumBinType.Value = DummyDataModule.dummyTCPIPQueries.BinType.Value;
        //        //string changedvalStr = (combobox).SelectedItem.ToString();

        //        //LoggerManager.Debug($"[PMIParameterSetupControlService] SelectionChangedCommandFunc() : Enum Name : {selectedtypeStr}, Changed Value : {changedvalStr}");
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private RelayCommand<Object> _DataResetComand;
        public ICommand DataResetComand
        {
            get
            {
                if (null == _DataResetComand) _DataResetComand = new RelayCommand<object>(DataResetComandFunc);
                return _DataResetComand;
            }
        }

        private void DataResetComandFunc(object obj)
        {
            try
            {
                string rootpath = @"C:\ProberSystem\Default";

                DeleteDirectory(rootpath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        public void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }


        private RelayCommand<Object> _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (null == _SaveCommand) _SaveCommand = new RelayCommand<object>(SaveCommandFunc);
                return _SaveCommand;
            }
        }

        private void SaveCommandFunc(object obj)
        {
            try
            {
                this.TCPIPModule().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }


        private RelayCommand<object> _SelectAllSlotCommand;
        public ICommand SelectAllSlotCommand
        {
            get
            {
                if (null == _SelectAllSlotCommand) _SelectAllSlotCommand = new RelayCommand<object>(SelectAllSlotCommandFunc);
                return _SelectAllSlotCommand;
            }
        }

        private void SelectAllSlotCommandFunc(object obj)
        {
            try
            {
                if (DummyDataModule.dummyTCPIPQueries.SelectedFoup != null)
                {
                    foreach (var slot in DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots)
                    {
                        slot.IsSelected = true;
                    }
                }

                UpdateSelectedSlotIndex();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        private RelayCommand<object> _ClearAllSlotCommand;
        public ICommand ClearAllSlotCommand
        {
            get
            {
                if (null == _ClearAllSlotCommand) _ClearAllSlotCommand = new RelayCommand<object>(ClearAllSlotCommandFunc);
                return _ClearAllSlotCommand;
            }
        }

        private void ClearAllSlotCommandFunc(object obj)
        {
            try
            {
                if (DummyDataModule.dummyTCPIPQueries.SelectedFoup != null)
                {
                    foreach (var slot in DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots)
                    {
                        slot.IsSelected = false;
                    }
                }

                UpdateSelectedSlotIndex();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }


        private RelayCommand<Object> _SelectedCoordinateChangedCommand;
        public ICommand SelectedCoordinateChangedCommand
        {
            get
            {
                if (null == _SelectedCoordinateChangedCommand) _SelectedCoordinateChangedCommand = new RelayCommand<object>(SelectedCoordinateChangedCommandFunc);
                return _SelectedCoordinateChangedCommand;
            }
        }

        private void SelectedCoordinateChangedCommandFunc(object obj)
        {
            try
            {

                //if (DummyDataModule.dummyTCPIPQueries.SelectedCoordiante != null)
                if (ProbingModule.ProbingLastMIndex != null)
                {
                    AssignDummyQueryData();
                    AssignDuumyInterruptData();
                    MakeInterruputCommandlist();
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

        private RelayCommand<Object> _SelectedSlotChangedCommand;
        public ICommand SelectedSlotChangedCommand
        {
            get
            {
                if (null == _SelectedSlotChangedCommand) _SelectedSlotChangedCommand = new RelayCommand<object>(SelectedSlotChangedCommandFunc);
                return _SelectedSlotChangedCommand;
            }
        }

        private void SelectedSlotChangedCommandFunc(object obj)
        {
            try
            {
                if (DummyDataModule.dummyTCPIPQueries.SelectedSlot != null)
                {
                    if (ComType != ProberEmulCommunicationMode.FLOW)
                    {
                        DummyDataModule.dummyTCPIPQueries.SelectedSlot.IsSelected = !DummyDataModule.dummyTCPIPQueries.SelectedSlot.IsSelected;
                    }

                    AssignDummyQueryData();
                    AssignDuumyInterruptData();
                    MakeInterruputCommandlist();
                }

                UpdateSelectedSlotIndex();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        private void UpdateSelectedSlotIndex()
        {
            try
            {
                if (SelectedSlotObject == null)
                {
                    SelectedSlotObject = new ObservableCollection<SlotObject>();
                }
                else
                {
                    SelectedSlotObject.Clear();
                }

                //SelectedSlotIndex = string.Empty;

                if (DummyDataModule.dummyTCPIPQueries != null)
                {
                    foreach (var slot in DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots)
                    {
                        if (slot.IsSelected == true)
                        {
                            SelectedSlotObject.Add(slot);
                        }
                    }

                    SelectedSlotCount = SelectedSlotObject.Count;

                    //ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                    //substrateInfo.SlotIndex.Value = (DummyDataModule.dummyTCPIPQueries.SelectedSlot.Index + 1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _SelectedFoupChangedCommand;
        public ICommand SelectedFoupChangedCommand
        {
            get
            {
                if (null == _SelectedFoupChangedCommand) _SelectedFoupChangedCommand = new RelayCommand<object>(SelectedFoupChangedCommandFunc);
                return _SelectedFoupChangedCommand;
            }
        }

        private void SelectedFoupChangedCommandFunc(object obj)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        //private RelayCommand<Object> _ChangedInterruptCommand;
        //public ICommand ChangedInterruptCommand
        //{
        //    get
        //    {
        //        if (null == _ChangedInterruptCommand) _ChangedInterruptCommand = new RelayCommand<object>(ChangedInterruptCommandFunc);
        //        return _ChangedInterruptCommand;
        //    }
        //}

        //private void ChangedInterruptCommandFunc(object obj)
        //{
        //    try
        //    {
        //        if(SelectedInterruptCommand != null)
        //        {
        //            if (string.IsNullOrEmpty(SelectedInterruptCommand) == false)
        //            {
        //                SendCommand = SelectedInterruptCommand;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //    }
        //}

        private RelayCommand<object> _ConnectExecuteCommand;
        public ICommand ConnectExecuteCommand
        {
            get
            {
                if (null == _ConnectExecuteCommand) _ConnectExecuteCommand = new RelayCommand<object>(ConnectExecuteCommandFunc);
                return _ConnectExecuteCommand;
            }
        }

        private void HardwareAsseblyVerifed_ValueChangedEvent(object oldValue, object newValue)
        {
            try
            {
                CheckCanLotStart();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LotProcessingVerifed_ValueChangedEvent(object oldValue, object newValue)
        {
            try
            {
                CheckCanLotStart();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CheckCanLotStart()
        {
            try
            {
                //if (this.LotOPModule().LotInfo.HardwareAsseblyVerifed.Value == true &&
                //   this.LotOPModule().LotInfo.LotProcessingVerifed.Value == true &&
                
                //if (this.LotOPModule().LotInfo.LotProcessingVerifed.Value == true && IsConnected == true)
                //{
                //    CanLotStart = true;
                //}
                //else
                //{
                //    CanLotStart = false;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void ConnectExecuteCommandFunc(object obj)
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                retval = TCPIPModule.Connect();

                if (retval == EventCodeEnum.NONE)
                {
                    IsConnected = true;

                    CheckCanLotStart();
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        private RelayCommand<object> _DisconnectExecuteCommand;
        public ICommand DisconnectExecuteCommand
        {
            get
            {
                if (null == _DisconnectExecuteCommand) _DisconnectExecuteCommand = new RelayCommand<object>(DisconnectExecuteCommandFunc);
                return _DisconnectExecuteCommand;
            }
        }

        public void DisconnectExecuteCommandFunc(object obj)
        {

            try
            {
                TCPIPModule.DisConnect();

                IsConnected = false;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        private RelayCommand<object> _SendSelectedInterruptCommand;
        public ICommand SendSelectedInterruptCommand
        {
            get
            {
                if (null == _SendSelectedInterruptCommand) _SendSelectedInterruptCommand = new RelayCommand<object>(SendSelectedInterruptCommandFunc);
                return _SendSelectedInterruptCommand;
            }
        }

        public void SendSelectedInterruptCommandFunc(object obj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SelectedInterruptCommand != null)
                {
                    // 선택 된 인터럽트 커맨드가 존재할 때
                    if (string.IsNullOrEmpty(SelectedInterruptCommand.EventFullName) == false &&
                        string.IsNullOrEmpty(SelectedInterruptCommand.Response) == false)
                    {
                        // TODO : 이벤트 발생기를 통해, 이벤트 발생

                        // 구독중인 이벤트들...

                        // TCPIPConnectionStart
                        // LotStartEvent
                        // GoToStartDieEvent
                        // DontRemainSequenceEvent
                        // ProbingZUpProcessEvent
                        // LotEndEvent

                        // Response_ZupDone (ZUp Action 수행 후 발생)

                        // Response_StopAndAlarmDone
                        // Response_MoveToNextIndexDone (MoveToNextIndex Action 수행 후 발생)
                        // Response_UnloadWaferDone (UnloadWafer Action 수행 후 발생)
                        // ProberErrorEvent

                        retVal = this.EventManager().RaisingEvent(SelectedInterruptCommand.EventFullName);
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        private RelayCommand<EnumProbingState> _ChangeZStateCommand;
        public ICommand ChangeZStateCommand
        {
            get
            {
                if (null == _ChangeZStateCommand) _ChangeZStateCommand = new RelayCommand<EnumProbingState>(ChangeZStateCommandFunc);
                return _ChangeZStateCommand;
            }
        }

        public void ChangeZStateCommandFunc(EnumProbingState param)
        {
            try
            {
                bool IsChanged = false;

                if (param == EnumProbingState.ZUP || param == EnumProbingState.ZUPDWELL)
                {
                    (ProbingModule as Probing).StateTransition(EnumProbingState.ZUP);
                    //DummyDataModule.dummyTCPIPQueries.ChuckZState = "Zup";

                    IsChanged = true;
                }
                else if (param == EnumProbingState.ZDN || param == EnumProbingState.ZDOWNDELL)
                {
                    (ProbingModule as Probing).StateTransition(EnumProbingState.ZDN);
                    //DummyDataModule.dummyTCPIPQueries.ChuckZState = "Zdn";
                    IsChanged = true;
                }
                else
                {
                    // TODO : EXCEPTION
                }

                if (IsChanged == true)
                {
                    ProbingStateEnum = ProbingModule.ProbingStateEnum;

                    AssignDuumyInterruptData();
                    MakeInterruputCommandlist();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        //private RelayCommand<object> _AddSequenceCommand;
        //public ICommand AddSequenceCommand
        //{
        //    get
        //    {
        //        if (null == _AddSequenceCommand) _AddSequenceCommand = new RelayCommand<object>(AddSequenceCommandFunc);
        //        return _AddSequenceCommand;
        //    }
        //}

        //public void AddSequenceCommandFunc(object obj)
        //{

        //    try
        //    {
        //        bool CanAdd = false;

        //        MachineIndex tmp = new MachineIndex();

        //        tmp.XIndex = DummyDataModule.dummyTCPIPQueries.XIndexForAddseq;
        //        tmp.YIndex = DummyDataModule.dummyTCPIPQueries.YIndexForAddseq;

        //        DummyDataModule.dummyTCPIPQueries.ProbingSequence.Add(tmp);

        //        MachineIndex lastindex = ProbingModule.ProbingLastMIndex;

        //        if (lastindex == null)
        //        {
        //            if (DummyDataModule.dummyTCPIPQueries.ProbingSequence != null && DummyDataModule.dummyTCPIPQueries.ProbingSequence.Count > 0)
        //            {
        //                lastindex = DummyDataModule.dummyTCPIPQueries.ProbingSequence.FirstOrDefault();
        //            }
        //        }

        //        //if (DummyDataModule.dummyTCPIPQueries.SelectedCoordiante == null)
        //        //{
        //        //    if (DummyDataModule.dummyTCPIPQueries.ProbingSequence != null && DummyDataModule.dummyTCPIPQueries.ProbingSequence.Count > 0)
        //        //    {
        //        //        DummyDataModule.dummyTCPIPQueries.SelectedCoordiante = DummyDataModule.dummyTCPIPQueries.ProbingSequence.FirstOrDefault();
        //        //    }
        //        //}

        //    }
        //    catch (Exception err)
        //    {

        //    }
        //}

        //private RelayCommand<object> _RemoveSequenceCommand;
        //public ICommand RemoveSequenceCommand
        //{
        //    get
        //    {
        //        if (null == _RemoveSequenceCommand) _RemoveSequenceCommand = new RelayCommand<object>(RemoveSequenceCommandFunc);
        //        return _RemoveSequenceCommand;
        //    }
        //}

        //public void RemoveSequenceCommandFunc(object obj)
        //{

        //    try
        //    {
        //        if (DummyDataModule.dummyTCPIPQueries.SelectedCoordiante != null)
        //        {
        //            DummyDataModule.dummyTCPIPQueries.ProbingSequence.Remove(DummyDataModule.dummyTCPIPQueries.SelectedCoordiante);
        //        }
        //    }
        //    catch (Exception err)
        //    {

        //    }
        //}

        private RelayCommand<object> _SendToInterruptPortCommand;
        public ICommand SendToInterruptPortCommand
        {
            get
            {
                if (null == _SendToInterruptPortCommand) _SendToInterruptPortCommand = new RelayCommand<object>(SendToInterruptPortCommandFunc);
                return _SendToInterruptPortCommand;
            }
        }

        public void SendToInterruptPortCommandFunc(object obj)
        {

            try
            {
                if (string.IsNullOrEmpty(SendCommand) == false)
                {
                    TCPIPModule.WriteSTB(SendCommand);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }



        private RelayCommand<object> _SendToCommandPortCommand;
        public ICommand SendToCommandPortCommand
        {
            get
            {
                if (null == _SendToCommandPortCommand) _SendToCommandPortCommand = new RelayCommand<object>(SendToCommandPortCommandFunc);
                return _SendToCommandPortCommand;
            }
        }

        public void SendToCommandPortCommandFunc(object obj)
        {

            try
            {
                TCPIPModule.WriteString(SendCommand);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        private RelayCommand<object> _ClearCommandCollectionListCommand;
        public ICommand ClearCommandCollectionListCommand
        {
            get
            {
                if (null == _ClearCommandCollectionListCommand) _ClearCommandCollectionListCommand = new RelayCommand<object>(ClearCommandCollectionListCommandFunc);
                return _ClearCommandCollectionListCommand;
            }
        }

        public void ClearCommandCollectionListCommandFunc(object obj)
        {

            try
            {
                CommandCollection = TCPIPModule.TesterComDriver.CommandCollection;
                CommandCollection.Clear();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        private RelayCommand<object> _DummyDataSetCommand;
        public ICommand DummyDataSetCommand
        {
            get
            {
                if (null == _DummyDataSetCommand) _DummyDataSetCommand = new RelayCommand<object>(DummyDataSetCommandFunc);
                return _DummyDataSetCommand;
            }
        }

        public void DummyDataSetCommandFunc(object obj)
        {
            try
            {
                DummyDataModule.SaveDummyQueryData();

                AssignDummyQueryData();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        private TCPIPTesterScenario Scenario = new TCPIPTesterScenario();

        Thread LotFlowThread;

        private bool _ContinueLot;
        public bool ContinueLot
        {
            get { return _ContinueLot; }
            set
            {
                if (value != _ContinueLot)
                {
                    _ContinueLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _LotForcedActivationCommand;
        public ICommand LotForcedActivationCommand
        {
            get
            {
                if (null == _LotForcedActivationCommand) _LotForcedActivationCommand = new RelayCommand<object>(LotForcedActivationCommandFunc);
                return _LotForcedActivationCommand;
            }
        }

        private void LotForcedActivationCommandFunc(object obj)
        {
            try
            {
                if (IsConnected == true)
                {
                    CanLotStart = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _LotFlowTestCommand;
        public ICommand LotFlowTestCommand
        {
            get
            {
                if (null == _LotFlowTestCommand) _LotFlowTestCommand = new RelayCommand<object>(LotFlowTestCommandFunc);
                return _LotFlowTestCommand;
            }
        }

        public void LotFlowTestCommandFunc(object obj)
        {

            try
            {
                if (ContinueLot == false)
                {
                    // TODO : 데이터 초기화
                    ComType = ProberEmulCommunicationMode.FLOW;

                    // TODO : ProbingSequecne Module로부터 얻어오자.
                    LastMI = DummyDataModule.dummyTCPIPQueries.ProbingSequence.ToList().FirstOrDefault();

                    foreach (var Component in lotScenario.Components)
                    {
                        Component.IsChecked = false;
                    }

                    foreach (var slot in DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots)
                    {
                        // WaferState로 컨트롤
                        slot.WaferState = EnumWaferState.UNPROCESSED;
                    }

                    LotFlowThread = new Thread(new ThreadStart(SequenceJob));
                    LotFlowThread.Start();

                    LotOPModule.LotInfo.LotStartTime = DateTime.Now.ToLocalTime();
                    (LotOPModule as LotOPModule).StateTransition(LotOPStateEnum.RUNNING);
                    LotStateEnum = LotOPModule.LotStateEnum;
                }
                else
                {
                    ContinueLot = false;

                    StopSequencer();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        public void StopSequencer()
        {
            try
            {
                if (LotFlowThread?.IsAlive == true)
                    LotFlowThread?.Join();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private int ChangeOrderNo = -1;

        private void SequenceJob()
        {
            try
            {
                int index = 0;

                ContinueLot = true;
                ChangeOrderNo = -1;

                string LastSendMessage = string.Empty;

                int ScenarioComponentsCount = lotScenario.Components.Count;

                ScenarioComponent s = null;

                while (ContinueLot)
                {
                    if (ScenarioComponentsCount - 1 >= index && s == null)
                    {
                        s = lotScenario.Components[index];
                    }
                    else if (s != null)
                    {
                        bool CanRaising = false;

                        if (s.type == EnumTCPIPCommandType.INTERRUPT)
                        {
                            if (s.RaisingCondition != null)
                            {
                                CanRaising = s.RaisingCondition();
                            }
                            else
                            {
                                CanRaising = true;
                            }

                            if (CanRaising == true)
                            {
                                if (s.PreAction != null)
                                {
                                    s.PreAction();
                                }

                                SimpleInterruptCommandRecipe recipe = InterruptCommandlist.FirstOrDefault(x => x.EventFullName == s.CommandName);

                                if (recipe != null)
                                {
                                    TCPIPModule.WriteSTB(recipe.Response);

                                    LastSendMessage = recipe.Response;
                                }

                                if (s.AfterAction != null)
                                {
                                    s.AfterAction();
                                }
                            }
                        }

                        if (CanRaising == true && s.CheckResponse == true)
                        {
                            while (s.IsChecked == false)
                            {
                                Thread.Sleep(5);

                                CommandCollection = TCPIPModule.TesterComDriver.CommandCollection;

                                if (CommandCollection.FirstOrDefault(x => x.Message.Contains("ACK " + LastSendMessage) == true) != null)
                                {
                                    s.IsChecked = true;
                                }

                                if (ContinueLot == false)
                                {
                                    break;
                                }
                            }

                            s = null;
                            index++;
                        }
                    }
                    else
                    {
                        ContinueLot = false;
                    }

                    if (ChangeOrderNo != -1)
                    {
                        s = null;
                        index = ChangeOrderNo;
                        ChangeOrderNo = -1;
                    }

                    Thread.Sleep(5);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                // TODO : LOT END
                ChangeOrderNo = -1;
                ComType = ProberEmulCommunicationMode.MANUAL;
                LotOPModule.LotInfo.LotEndTime = DateTime.Now.ToLocalTime();
                (LotOPModule as LotOPModule).StateTransition(LotOPStateEnum.IDLE);
                LotStateEnum = LotOPModule.LotStateEnum;

                ClearLotData();
            }
        }

        private void ClearLotData()
        {
            try
            {
                foreach (var slot in DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots)
                {
                    // WaferState로 컨트롤
                    slot.WaferState = EnumWaferState.UNPROCESSED;
                }

                DummyDataModule.dummyTCPIPQueries.SelectedFoup.LotState = ProberInterfaces.LotStateEnum.Idle;

                ContinueLot = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<FoupStateEnum> _FoupStateChangeCommand;
        public ICommand FoupStateChangeCommand
        {
            get
            {
                if (null == _FoupStateChangeCommand) _FoupStateChangeCommand = new RelayCommand<FoupStateEnum>(FoupStateChangeCommandFunc);
                return _FoupStateChangeCommand;
            }
        }


        public void FoupStateChangeCommandFunc(FoupStateEnum param)
        {

            try
            {
                bool IsChanged = false;

                if (DummyDataModule.dummyTCPIPQueries.Foups != null)
                {
                    FoupObject foup = DummyDataModule.dummyTCPIPQueries.Foups.FirstOrDefault();

                    if (foup != null)
                    {
                        FoupStateEnum currentstate = FoupStateEnum.EMPTY_CASSETTE;

                        currentstate = foup.State;

                        if (currentstate != param)
                        {
                            DummyDataModule.dummyTCPIPQueries.Foups.First().State = param;
                            IsChanged = true;
                        }
                    }

                    if (IsChanged == true)
                    {

                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        #endregion

        private void MakeLotFlowScenario()
        {
            try
            {
                if (this.lotScenario == null)
                {
                    this.lotScenario = new LotScenario();
                }

                if (this.lotScenario.Components != null)
                {
                    this.lotScenario.Components.Clear();
                }

                ScenarioComponent s = null;

                // LotStart#P

                s = new ScenarioComponent();
                s.CommandName = "NotifyEventModule.LotStartEvent";
                s.CheckResponse = true;
                s.IsChecked = false;
                s.type = EnumTCPIPCommandType.INTERRUPT;
                s.AfterAction = LotStart;

                lotScenario.Components.Add(s);

                // WaferStart#P#S

                s = new ScenarioComponent();
                s.CommandName = "NotifyEventModule.GoToStartDieEvent";
                s.CheckResponse = true;
                s.IsChecked = false;
                s.type = EnumTCPIPCommandType.INTERRUPT;
                s.PreAction = LoadWafer;

                lotScenario.Components.Add(s);

                // LotEnd#P

                s = new ScenarioComponent();
                s.CommandName = "NotifyEventModule.LotEndEvent";
                s.CheckResponse = true;
                s.IsChecked = false;
                s.type = EnumTCPIPCommandType.INTERRUPT;
                s.RaisingCondition = CanLotEnd;

                lotScenario.Components.Add(s);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private SlotObject GetNextWafer()
        {
            SlotObject retval = null;

            try
            {
                retval = DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots.FirstOrDefault(x => x.WaferState == EnumWaferState.UNPROCESSED && x.IsSelected == true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void LotStart()
        {
            try
            {
                DummyDataModule.dummyTCPIPQueries.SelectedFoup.LotState = ProberInterfaces.LotStateEnum.Running;

                ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                substrateInfo.FailedDieCount.Value = 0;
                substrateInfo.TestedDieCount.Value = 0;
                substrateInfo.PassedDieCount.Value = 0;

                substrateInfo.CurFailedDieCount.Value = 0;
                substrateInfo.CurTestedDieCount.Value = 0;
                substrateInfo.CurPassedDieCount.Value = 0;

                substrateInfo.Yield = 0;

                AssignDummyQueryData();
                AssignDuumyInterruptData();
                MakeInterruputCommandlist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LoadWafer()
        {
            try
            {
                SlotObject nextwafer = GetNextWafer();

                if (nextwafer != null)
                {
                    ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                    DummyDataModule.dummyTCPIPQueries.SelectedSlot = nextwafer;

                    substrateInfo.LoadingTime = DateTime.Now.ToLocalTime();

                    Thread.Sleep(100);

                    substrateInfo.ProbingStartTime = DateTime.Now.ToLocalTime();

                    substrateInfo.SlotIndex.Value = (DummyDataModule.dummyTCPIPQueries.SelectedSlot.Index + 1);

                    string lotname = this.LotOPModule().LotInfo.LotName.Value;
                    int waferno = substrateInfo.SlotIndex.Value;

                    // TODO : ST의 경우, 유효한 LOTID의 길이 = 7, 8, 9

                    string readerpattern = string.Empty;

                    readerpattern = ReaderMaker.GetReaderPattern(FABTYPE.ST, lotname);

                    if (!string.IsNullOrEmpty(readerpattern))
                    {
                        this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value = ReaderMaker.MakeWaferID(readerpattern, lotname, waferno);
                    }
                    else
                    {
                        this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value = string.Empty;
                    }

                    AssignDummyQueryData();
                    AssignDuumyInterruptData();
                    MakeInterruputCommandlist();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private bool CanLotEnd()
        {
            bool retval = false;

            try
            {
                int selectedcount = DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots.Count(x => x.IsSelected == true);
                int processedcount = DummyDataModule.dummyTCPIPQueries.SelectedFoup.Slots.Count(x => x.WaferState == EnumWaferState.PROCESSED);

                if (selectedcount == processedcount)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void Loaded()
        {
            this.MetroDialogManager().SetMetroWindowLoaded(true);
            //Extensions_IParam.LoadProgramFlag = true;
        }

        private void SetDummyProbingSequecne()
        {
            try
            {
                DummyDataModule.dummyTCPIPQueries.ProbingSequence = new ObservableCollection<MachineIndex>(this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value);

                //DummyDataModule.dummyTCPIPQueries.SelectedCoordiante = DummyDataModule.dummyTCPIPQueries.ProbingSequence.FirstOrDefault();
                ProbingModule.SetLastProbingMIndex(DummyDataModule.dummyTCPIPQueries.ProbingSequence.FirstOrDefault());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SetDummyProbingSequecne();
                DummyDataSetCommandFunc(null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
        }
    }

    public class ScenarioComponent
    {
        public string CommandName { get; set; }
        public bool CheckResponse { get; set; }

        public bool IsChecked { get; set; }
        public int interval { get; set; }
        public EnumTCPIPCommandType type { get; set; }

        public Action PreAction { get; set; }
        public Action AfterAction { get; set; }
        public Func<bool> RaisingCondition { get; set; }
    }

    public class LotScenario
    {
        public LotScenario()
        {
            if (Components == null)
            {
                Components = new List<ScenarioComponent>();
            }
        }

        public List<ScenarioComponent> Components { get; set; }
    }

    public class SimpleInterruptCommandRecipe : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _EventFullName;
        public string EventFullName
        {
            get { return _EventFullName; }
            set
            {
                if (value != _EventFullName)
                {
                    _EventFullName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Response;
        public string Response
        {
            get { return _Response; }
            set
            {
                if (value != _Response)
                {
                    _Response = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
