using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderHandlingViewModelModule
{
    using LogModule;
    using ProberInterfaces;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase;
    using LoaderCore;
    using LoaderMapView;
    using LoaderParameters;
    using LoaderParameters.Data;
    using ProberInterfaces.Loader;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using StageStateEnum = LoaderBase.Communication.StageStateEnum;
    using Autofac;
    using ProberViewModel;
    using ProberInterfaces.Foup;
    using LoaderBase.Communication;
    using MetroDialogInterfaces;
    using LoaderRecoveryControl;
    using System.Windows.Controls.Primitives;
    using System.Threading;
    using System.Diagnostics;
    using ProberViewModel.ViewModel;
    using ProberViewModel.View.Handling;
    using WinAPIWrapper;
    using ProberInterfaces.ControlClass.ViewModel;
    using ProberInterfaces.Enum;
    using ProberInterfaces.E84;
    using ProberInterfaces.CardChange;

    public class LoaderHandlingViewModel : INotifyPropertyChanged, ILoaderMapConvert, IMainScreenViewModel, ILoaderHandlingViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        public void Initialize()
        {
            LoaderModule = this.loaderModule;
            loaderModule.SetLoaderMapConvert(this);
            loaderModule.SetLoaderMapConvertHandling(this);
            LoaderMapConvert(loaderModule.GetLoaderInfo().StateMap);


            GPIOs = new ObservableCollection<IOPortDescripter<bool>>();

            var inputs = loaderModule.IOManager.IOMappings.RemoteInputs;
            var inputType = inputs.GetType();
            var props = inputType.GetProperties();

            foreach (var item in props)
            {
                if (item.PropertyType == typeof(List<IOPortDescripter<bool>>))
                {
                    var ios = item.GetValue(inputs) as List<IOPortDescripter<bool>>;

                    if (ios != null)
                    {
                        foreach (var port in ios)
                        {
                            if (port is IOPortDescripter<bool>)
                            {
                                GPIOs.Add(port);
                            }
                        }
                    }
                }
                else if (item.PropertyType == typeof(IOPortDescripter<bool>))
                {
                    var iodesc = item.GetValue(inputs) as IOPortDescripter<bool>;
                    GPIOs.Add(iodesc);
                }

            }
        }


        public IDeviceManager devicemanager => _Container.Resolve<IDeviceManager>();


        public ILoaderModule loaderModule => _Container.Resolve<ILoaderModule>();
        private ILoaderCommunicationManager loaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();
        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();
        private void InitData()
        {
            stagecnt = SystemModuleCount.ModuleCnt.StageCount;
            paCount = SystemModuleCount.ModuleCnt.PACount;
            armcnt = SystemModuleCount.ModuleCnt.ArmCount;
            buffercnt = SystemModuleCount.ModuleCnt.BufferCount;
            foupCount = SystemModuleCount.ModuleCnt.FoupCount;
            cardArmCnt = SystemModuleCount.ModuleCnt.CardArmCount;
            cardTrayCnt = SystemModuleCount.ModuleCnt.CardTrayCount;
            cardBufferCnt = SystemModuleCount.ModuleCnt.CardBufferCount;
            iNSPCnt = SystemModuleCount.ModuleCnt.INSPCount;
            FixedTrayCnt = SystemModuleCount.ModuleCnt.FixedTrayCount;
            slotCount = SystemModuleCount.ModuleCnt.SlotCount;

            for (int i = 0; i < stagecnt; i++)
            {
                //int idx = 0;
                //if (i < 4)
                //{
                //    idx = i + 8;
                //}
                //else if (i < 8)
                //{
                //    idx = i;
                //}
                //else
                //{
                //    idx = i - 8;
                //}
                Cells.Add(new StageObject(i + 1));
            }
            for (int i = 0; i < paCount; i++)
            {
                PAs.Add(new PAObject(i));
            }
            for (int i = 0; i < armcnt; i++)
            {
                Arms.Add(new ArmObject(i));
            }

            for (int i = 0; i < buffercnt; i++)
            {
                Buffers.Add(new BufferObject(i));
            }
            for (int i = 0; i < foupCount; i++)
            {
                Foups.Add(new FoupObject(i));
                Foups[i].Slots = new ObservableCollection<SlotObject>();
                for (int j = 0; j < slotCount; j++)
                {
                    Foups[i].Slots.Add(new SlotObject(slotCount - j));
                }
            }

            for (int i = 0; i < cardArmCnt; i++)
            {
                CardArms.Add(new CardArmObject(i));
            }
            for (int i = 0; i < cardTrayCnt; i++)
            {
                CardTrays.Add(new CardTrayObject(i));
            }
            for (int i = 0; i < cardBufferCnt; i++)
            {
                CardBuffers.Add(new CardBufferObject(i));
            }
            for (int i = 0; i < FixedTrayCnt; i++)
            {
                FTs.Add(new FixedTrayInfoObject(i));
            }
            for (int i = 0; i < iNSPCnt; i++)
            {
                ITs.Add(new InspectionTrayInfoObject(i));
            }


        }


        #region //..Property
        private bool IsTransferSorceWafersizeisundefined = false;
        private bool IsTransferSorceUseOthertypesofcassettes = false;

        private ObservableCollection<IOPortDescripter<bool>> _GPIOs;
        public ObservableCollection<IOPortDescripter<bool>> GPIOs
        {
            get { return _GPIOs; }
            set
            {
                if (value != _GPIOs)
                {
                    _GPIOs = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ILoaderModule _LoaderModule;
        public ILoaderModule LoaderModule
        {
            get { return _LoaderModule; }
            set
            {
                if (value != _LoaderModule)
                {
                    _LoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SearchKeyword;
        public string SearchKeyword
        {
            get { return _SearchKeyword; }
            set
            {
                if (value != _SearchKeyword)
                {
                    _SearchKeyword = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _ModuleInfoEnable = true;
        public bool ModuleInfoEnable
        {
            get { return _ModuleInfoEnable; }
            set
            {
                if (value != _ModuleInfoEnable)
                {
                    _ModuleInfoEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _SkipDocking = false;
        public bool SkipDocking
        {
            get { return _SkipDocking; }
            set
            {
                if (value != _SkipDocking)
                {
                    _SkipDocking = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumRepeatedTransferMode _RepeatedTransferMode;
        public EnumRepeatedTransferMode RepeatedTransferMode
        {
            get { return _RepeatedTransferMode; }
            set
            {
                if (value != _RepeatedTransferMode)
                {
                    _RepeatedTransferMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsFosbActive = false;
        public bool IsFosbActive
        {
            get { return _IsFosbActive; }
            set
            {
                if (value != _IsFosbActive)
                {
                    _IsFosbActive = value;
                    UpdateCSTBtnEnable();
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _FilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> FilteredPorts
        {
            get { return _FilteredPorts; }
            set
            {
                if (value != _FilteredPorts)
                {
                    _FilteredPorts = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _StartTime;
        public string StartTime
        {
            get { return _StartTime; }
            set
            {
                if (value != _StartTime)
                {
                    _StartTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _EndTime;
        public string EndTime
        {
            get { return _EndTime; }
            set
            {
                if (value != _EndTime)
                {
                    _EndTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _ElapsedTime = new long();
        public long ElapsedTime
        {
            get { return _ElapsedTime; }
            set
            {
                if (value != _ElapsedTime)
                {
                    _ElapsedTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<StageObject> _Cells = new ObservableCollection<StageObject>();
        public ObservableCollection<StageObject> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PAObject> _PAs = new ObservableCollection<PAObject>();
        public ObservableCollection<PAObject> PAs
        {
            get { return _PAs; }
            set
            {
                if (value != _PAs)
                {
                    _PAs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<FixedTrayInfoObject> _FTs
            = new ObservableCollection<FixedTrayInfoObject>();
        public ObservableCollection<FixedTrayInfoObject> FTs
        {
            get { return _FTs; }
            set
            {
                if (value != _FTs)
                {
                    _FTs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<InspectionTrayInfoObject> _ITs
           = new ObservableCollection<InspectionTrayInfoObject>();
        public ObservableCollection<InspectionTrayInfoObject> ITs
        {
            get { return _ITs; }
            set
            {
                if (value != _ITs)
                {
                    _ITs = value;
                    RaisePropertyChanged();
                }
            }
        }
        readonly Guid _ViewModelGUID = new Guid("AB9BDCA2-B69A-E42D-C6C5-5E4E84ADF759");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;


        private ObservableCollection<ArmObject> _Arms = new ObservableCollection<ArmObject>();
        public ObservableCollection<ArmObject> Arms
        {
            get { return _Arms; }
            set
            {
                if (value != _Arms)
                {
                    _Arms = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<BufferObject> _Buffers = new ObservableCollection<BufferObject>();
        public ObservableCollection<BufferObject> Buffers
        {
            get { return _Buffers; }
            set
            {
                if (value != _Buffers)
                {
                    _Buffers = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<CardArmObject> _CardArms = new ObservableCollection<CardArmObject>();
        public ObservableCollection<CardArmObject> CardArms
        {
            get { return _CardArms; }
            set
            {
                if (value != _CardArms)
                {
                    _CardArms = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<CardBufferObject> _CardBuffers = new ObservableCollection<CardBufferObject>();
        public ObservableCollection<CardBufferObject> CardBuffers
        {
            get { return _CardBuffers; }
            set
            {
                if (value != _CardBuffers)
                {
                    _CardBuffers = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<CardTrayObject> _CardTrays = new ObservableCollection<CardTrayObject>();
        public ObservableCollection<CardTrayObject> CardTrays
        {
            get { return _CardTrays; }
            set
            {
                if (value != _CardTrays)
                {
                    _CardTrays = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<FoupObject> _Foups = new ObservableCollection<FoupObject>();
        public ObservableCollection<FoupObject> Foups
        {
            get { return _Foups; }
            set
            {
                if (value != _Foups)
                {
                    _Foups = value;
                    RaisePropertyChanged();
                }
            }
        }
        private LoaderMap _LoaderMap = new LoaderMap();
        public LoaderMap LoaderMap
        {
            get { return _LoaderMap; }
            set
            {
                if (value != _LoaderMap)
                {
                    _LoaderMap = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _SourceEnable = true;
        public bool SourceEnable
        {
            get { return _SourceEnable; }
            set
            {
                if (value != _SourceEnable)
                {
                    _SourceEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TargetEnable = false;
        public bool TargetEnable
        {
            get { return _TargetEnable; }
            set
            {
                if (value != _TargetEnable)
                {
                    _TargetEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CancelEnable = true;
        public bool CancelEnable
        {
            get { return _CancelEnable; }
            set
            {
                if (value != _CancelEnable)
                {
                    _CancelEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _TransferEnable = false;
        public bool TransferEnable
        {
            get { return _TransferEnable; }
            set
            {
                if (value != _TransferEnable)
                {
                    _TransferEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _FoupLoadEnable = false;
        public bool FoupLoadEnable
        {
            get { return _FoupLoadEnable; }
            set
            {
                if (value != _FoupLoadEnable)
                {
                    _FoupLoadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _FoupUnloadEnable = false;
        public bool FoupUnloadEnable
        {
            get { return _FoupUnloadEnable; }
            set
            {
                if (value != _FoupUnloadEnable)
                {
                    _FoupUnloadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _FosBLoadEnable = false;
        public bool FosBLoadEnable
        {
            get { return _FosBLoadEnable; }
            set
            {
                if (value != _FosBLoadEnable)
                {
                    _FosBLoadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _FosBUnloadEnable = false;
        public bool FosBUnloadEnable
        {
            get { return _FosBUnloadEnable; }
            set
            {
                if (value != _FosBUnloadEnable)
                {
                    _FosBUnloadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _FoupScanEnable = false;
        public bool FoupScanEnable
        {
            get { return _FoupScanEnable; }
            set
            {
                if (value != _FoupScanEnable)
                {
                    _FoupScanEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _FoupEventEnable = false;
        public bool FoupEventEnable
        {
            get { return _FoupEventEnable; }
            set
            {
                if (value != _FoupEventEnable)
                {
                    _FoupEventEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CollectEnable = true;
        public bool CollectEnable
        {
            get { return _CollectEnable; }
            set
            {
                if (value != _CollectEnable)
                {
                    _CollectEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84Parameters _E84SysParam;
        public IE84Parameters E84SysParam
        {
            get { return _E84SysParam; }
            set
            {
                if (value != _E84SysParam)
                {
                    _E84SysParam = (E84Parameters)value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                InitData();
                Initialize();
                //(CardLoadingTestCommand as AsyncCommand).SetCancelTokenPack(CancelTransferTokenPack);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                this.TransferSource = null;
                this.TransferTarget = null;

                loaderModule.SetLoaderMapConvert(this);
                loaderModule.BroadcastLoaderInfo();
                if (Extensions_IParam.ProberRunMode == RunMode.DEFAULT)
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.CardBufferCount; i++)
                    {
                        var carriervac = loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1).GetDICARRIERVAC();
                        if (carriervac != null)
                        {
                            loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1).RecoveryCardStatus();
                        }
                    }
                }

                foreach (var foup in Foups)
                {
                    foup.ModuleInfo = this.FoupOpModule().GetFoupController(foup.Index + 1)?.FoupModuleInfo;
                }

                string fiexdTrayIndexString = "";
                foreach (var item in FTs)
                {
                    if(item.WaferObj != null)
                    {
                        if (item.CanUseBuffer
                        && item.WaferObj.WaferType.Value == EnumWaferType.POLISH
                        && item.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            fiexdTrayIndexString += item.Index.ToString() + ", ";
                        }
                    }
                }
                if(fiexdTrayIndexString != "")
                {
                    int index = fiexdTrayIndexString.LastIndexOf(',');
                    string msg = fiexdTrayIndexString.Remove(index, 1);
                    this.MetroDialogManager().ShowMessageDialog("Warning", $"Fixed Tray No. {msg} is for Buffer. \nPlease move the object another location.", EnumMessageStyle.Affirmative);
                }
                LoaderMapConvert(loaderModule.GetLoaderInfo().StateMap);

                E84SysParam = this.E84Module().E84SysParam;

                SetAllObjectEnable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.DEFAULT)
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.CardBufferCount; i++)
                    {
                        var carriervac = loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1).GetDICARRIERVAC();
                        if (carriervac != null)
                        {
                            LoggerManager.Debug($"[LoaderHandlingViewModel] Cleanup(): SetCardTrayVac({carriervac.Value})");
                            if (carriervac.Value == false)
                            {
                                loaderModule.GetLoaderCommands().SetCardTrayVac(false);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {

            get { return _IsSelected; }
            set
            {
                //if (value != _IsSelected)
                //{
                _IsSelected = value;
                if (_IsSelected == false)
                {
                    RaisePropertyChanged();
                }
                //}
            }
        }
        #endregion


        public async Task LoaderMapConvert(LoaderMap map)
        {
            try
            {
                Task task = new Task(() =>
                {
                    for (int i = 0; i < map.ChuckModules.Count(); i++)
                    {
                        if (Cells.Count > i)
                        {
                            if (LoaderMaster.StageStates.Count > i)
                            {
                                Cells[i].StageState = this.LoaderMaster.StageStates[i];
                                //cell.Name = map.ChuckModules[i].ID.ToString();
                                if (map.ChuckModules[i].Substrate != null)
                                {
                                    Cells[i].State = StageStateEnum.Requested;
                                    Cells[i].TargetName = map.ChuckModules[i].Substrate.PrevPos.ToString();
                                    Cells[i].Progress = map.ChuckModules[i].Substrate.OriginHolder.Index;
                                    Cells[i].WaferObj = map.ChuckModules[i].Substrate;
                                }
                                else
                                {
                                    Cells[i].State = StageStateEnum.Not_Request;
                                }
                                Cells[i].WaferStatus = map.ChuckModules[i].WaferStatus;
                                if (map.CCModules[i].Substrate != null)
                                {
                                    Cells[i].CardObj = map.CCModules[i].Substrate;
                                    Cells[i].CardStatus = map.CCModules[i].WaferStatus;
                                    Cells[i].Progress = map.CCModules[i].Substrate.OriginHolder.Index;
                                }
                                Cells[i].CardStatus = map.CCModules[i].WaferStatus;
                                Cells[i].StageInfo.IsConnected = loaderCommunicationManager.Cells[i].StageInfo.IsConnected;
                                IChuckModule Chuck = (IChuckModule)this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CHUCK, i + 1);
                                if (Chuck != null)
                                {
                                    Cells[i].IsWaferOnHandler = Chuck.Holder.IsWaferOnHandler;
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] StageCount : {Cells.Count}, [LoaderSystem.json] ChuckModules : {map.ChuckModules.Count()}");
                            break;
                        }
                    }

                    for (int i = 0; i < PAs.Count; i++)
                    {
                        var Pa = map.PreAlignModules.Where(pa => pa.ID.Index == PAs[i].Index).Count();
                        if (Pa > 0)
                        {
                            var PA = map.PreAlignModules.Where(pa => pa.ID.Index == PAs[i].Index).FirstOrDefault();
                            if (PA.Substrate != null)
                            {
                                PAs[i].WaferObj = PA.Substrate;
                            }
                            PAs[i].WaferStatus = PA.WaferStatus;
                            PAs[i].Enable = PA.Enable;
                            IPreAlignModule pA = (IPreAlignModule)this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.PA, i + 1);
                            if (pA != null)
                            {
                                PAs[i].PAStatus = pA.PAStatus;
                            }
                        }
                        else
                        {
                            PAs[i].Enable = false;
                        }
                    }
                    
                    for (int i = 0; i < map.FixedTrayModules.Count(); i++)
                    {
                        if (FTs.Count > i)
                        {
                            if (FTs[i].Index == map.FixedTrayModules[i].ID.Index)
                            {
                                if (map.FixedTrayModules[i].Substrate != null)
                                {
                                    FTs[i].WaferObj = map.FixedTrayModules[i].Substrate;
                                }
                                FTs[i].WaferStatus = map.FixedTrayModules[i].WaferStatus;
                                FTs[i].CanUseBuffer = map.FixedTrayModules[i].CanUseBuffer;
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] FixedTrayCount : {FTs.Count}, [LoaderSystem.json] FixedTrayModules : {map.FixedTrayModules.Count()}");
                            break;
                        }
                    }


                    for (int i = 0; i < map.InspectionTrayModules.Count(); i++)
                    {
                        if (ITs.Count > i)
                        {
                            if (map.InspectionTrayModules[i].Substrate != null)
                            {
                                ITs[i].WaferObj = map.InspectionTrayModules[i].Substrate;
                            }
                            ITs[i].WaferStatus = map.InspectionTrayModules[i].WaferStatus;
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] INSPCount : {ITs.Count}, [LoaderSystem.json] InspectionTrayModules : {map.InspectionTrayModules.Count()}");
                            break;
                        }
                    }


                    for (int i = 0; i < map.ARMModules.Count(); i++)
                    {
                        if (Arms.Count > i)
                        {
                            if (map.ARMModules[i].Substrate != null)
                            {
                                Arms[i].WaferObj = map.ARMModules[i].Substrate;
                            }
                            Arms[i].WaferStatus = map.ARMModules[i].WaferStatus;
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] ArmCount : {Arms.Count}, [LoaderSystem.json] ARMModules : {map.ARMModules.Count()}");
                            break;
                        }
                    }

                    for (int i = 0; i < Buffers.Count; i++)
                    {
                        var Buffer = map.BufferModules.Where(buffer => buffer.ID.Index == Buffers[i].Index).Count();
                        if (Buffer > 0)
                        {
                            var BUFFER = map.BufferModules.Where(pa => pa.ID.Index == Buffers[i].Index).FirstOrDefault();
                            if (BUFFER.Substrate != null)
                            {
                                Buffers[i].WaferObj = BUFFER.Substrate;
                            }
                            Buffers[i].WaferStatus = BUFFER.WaferStatus;
                            Buffers[i].Enable = BUFFER.Enable;
                        }
                        else
                        {
                            Buffers[i].Enable = false;
                        }
                    }

                    for (int i = 0; i < map.CardArmModule.Count(); i++)
                    {
                        if (CardArms.Count > i)
                        {
                            if (map.CardArmModule[i].Substrate != null)
                            {
                                CardArms[i].CardObj = map.CardArmModule[i].Substrate;
                            }
                            CardArms[i].WaferStatus = map.CardArmModule[i].WaferStatus;
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] CardArmCount : {CardArms.Count}, [LoaderSystem.json] CardArmModule : {map.CardArmModule.Count()}");
                            break;
                        }
                    }


                    for (int i = 0; i < map.CardBufferModules.Count(); i++)
                    {
                        if (CardBuffers.Count > i)
                        {
                            if (map.CardBufferModules[i].Substrate != null)
                            {
                                CardBuffers[i].CardObj = map.CardBufferModules[i].Substrate;
                            }
                            var cardbuffer = loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1);
                            CardBuffers[i].CardPRESENCEState = cardbuffer.CardPRESENCEState;

                            CardBuffers[i].WaferStatus = map.CardBufferModules[i].WaferStatus;
                            CardBuffers[i].Enable = map.CardBufferModules[i].Enable;
                            if (cardbuffer != null)
                            {
                                CardBuffers[i].IsCardAttachHolder = cardbuffer.Holder.isCardAttachHolder;
                            }
                        }
                        else
                        {
                            //LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] CardBufferCount : {CardBuffers.Count}, [LoaderSystem.json] CardBufferModules : {map.CardBufferModules.Count()}");
                            break;
                        }
                    }


                    for (int i = 0; i < map.CardTrayModules.Count(); i++)
                    {
                        if (CardTrays.Count > i)
                        {
                            if (map.CardTrayModules[i].Substrate != null)
                            {
                                CardTrays[i].CardObj = map.CardTrayModules[i].Substrate;
                            }
                            CardTrays[i].WaferStatus = map.CardTrayModules[i].WaferStatus;
                            ICardBufferTrayModule cardtray = (ICardBufferTrayModule)this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CARDTRAY, i + 1);
                            if (cardtray != null)
                            {
                                CardTrays[i].IsCardAttachHolder = cardtray.Holder.isCardAttachHolder;
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] CardTrayCount : {CardTrays.Count}, [LoaderSystem.json] CardTrayModules : {map.CardTrayModules.Count()}");
                            break;
                        }
                    }

                    int selectedFoupNum = -1;
                    bool isExternalLotStart = false;
                    if (LoaderMaster.Mode == LoaderMasterMode.External)
                    {
                        isExternalLotStart = true;
                    }

                    for (int i = 0; i < map.CassetteModules.Count(); i++)
                    {
                        if (Foups.Count > i)
                        {
                            Foups[i].State = map.CassetteModules[i].FoupState;
                            Foups[i].ScanState = map.CassetteModules[i].ScanState;
                            Foups[i].CassetteID = map.CassetteModules[i].FoupID;                            
                            Foups[i].CassetteType = loaderModule.Foups[i].CassetteType;

                            bool isChangedEnable = false;
                            if(Foups[i].Enable != map.CassetteModules[i].Enable)
                            {
                                isChangedEnable = true;
                            }
                            Foups[i].Enable = map.CassetteModules[i].Enable;

                            if(isChangedEnable)
                            {
                                UpdateCSTBtnEnable();
                            }

                            for (int j = 0; j < map.CassetteModules[i].SlotModules.Count(); j++)
                            {
                                //var slot = new SlotObject(map.CassetteModules[i].SlotModules.Count() - j);
                                if (map.CassetteModules[i].SlotModules[j].Substrate != null)
                                {
                                    if (isExternalLotStart && LoaderMaster.ActiveLotInfos[i].State == LotStateEnum.Running) //foupNumber가 같을때
                                    {
                                        if (!(LoaderMaster.ActiveLotInfos[i].UsingSlotList.Where(idx => idx == Foups[i].Slots[j].Index).FirstOrDefault() > 0))
                                        {
                                            Foups[i].Slots[j].WaferState = EnumWaferState.SKIPPED;
                                            LoaderModule.Foups[i].Slots[j].WaferState = EnumWaferState.SKIPPED;
                                            var waferObj = LoaderModule.ModuleManager.FindTransferObject(map.CassetteModules[i].SlotModules[j].Substrate.ID.Value);

                                            if (waferObj != null)
                                            {
                                                waferObj.WaferState = EnumWaferState.SKIPPED;
                                            }
                                        }
                                        else
                                        {
                                            Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                            LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                        }

                                        if (LoaderMaster.ActiveLotInfos[i].UsingPMIList.Where(idx => idx == Foups[i].Slots[j].Index).FirstOrDefault() > 0)
                                        {
                                            if (LoaderModule.Foups[i].Slots[j].WaferObj != null)
                                            {
                                                //LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMITriggerEnum.WAFER_INTERVAL;
                                                LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMIRemoteTriggerEnum.UNDIFINED;
                                                LoaderModule.Foups[i].Slots[j].WaferObj.DoPMIFlag = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                        LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                    }
                                    Foups[i].Slots[j].WaferStatus = map.CassetteModules[i].SlotModules[j].WaferStatus;
                                    Foups[i].Slots[j].WaferObj = map.CassetteModules[i].SlotModules[j].Substrate;
                                }
                                else if (map.CassetteModules[i].SlotModules[j].WaferStatus == EnumSubsStatus.UNDEFINED)
                                {
                                    Foups[i].Slots[j].WaferStatus = EnumSubsStatus.NOT_EXIST;
                                }
                                else
                                {
                                    Foups[i].Slots[j].WaferStatus = map.CassetteModules[i].SlotModules[j].WaferStatus;
                                }
                                Foups[i].Slots[j].FoupNumber = i;
                                //foup.Slots.Add(slot);
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] FoupCount : {Foups.Count}, [LoaderSystem.json] CassetteModules : {map.CassetteModules.Count()}");
                            break;
                        }
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"LoaderMapConvert(): Error occurred. Err = {err.Message}");
            }
            //System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{

            //}));

        }

        DateTime StartTimeDate;
        public void SetStartTime(DateTime startTime)
        {
            StartTimeDate = startTime;
            StartTime = startTime.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
        }
        public void SetEndTime(DateTime endTime)
        {
            var elaspsedTime = endTime - StartTimeDate;
            EndTime = endTime.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
            ElapsedTime = (long)elaspsedTime.TotalMilliseconds;
        }
        public void SetElapsedTime(TimeSpan time)
        {
            try
            {
                EndTime = time.ToString(@"hh\:mm\:ss\.fff");
                //EndTime = string.Format("{0:mm\\:ss.fff} days", time);
            }
            catch (Exception err)
            {

            }
        }
        int stagecnt = 12;
        int armcnt = 2;
        int buffercnt = 5;
        int paCount = 3;
        int foupCount = 3;
        int slotCount = 25;
        int cardArmCnt = 1;
        int cardBufferCnt = 4;
        int cardTrayCnt = 9;
        int FixedTrayCnt = 9;
        int iNSPCnt = 3;
        #region //..Transfer
        private object _TransferSource;
        public object TransferSource
        {
            get { return _TransferSource; }
            set
            {
                if (value != _TransferSource)
                {
                    if (value is true || value is null)
                    {
                        TargetEnable = false;
                    }
                    else
                    {
                        TargetEnable = true;
                    }
                    _TransferSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _TransferTarget;
        public object TransferTarget
        {
            get { return _TransferTarget; }
            set
            {
                if (value != _TransferTarget)
                {
                    if (value is true || value is null)
                    {
                        TransferEnable = false;
                    }
                    else
                    {
                        TransferEnable = true;
                    }
                    _TransferTarget = value;
                    RaisePropertyChanged();
                }
            }
        }


        private object GetAttachObject(ModuleTypeEnum module, int index)
        {
            object retVal = null;
            try
            {
                switch (module)
                {
                    case ModuleTypeEnum.SLOT:
                        int slotNum = index % 25;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = 25;
                            offset = -1;
                        }
                        int foupNum = ((index + offset) / 25);
                        retVal = Foups[foupNum].Slots.Where(i => i.Index == slotNum).FirstOrDefault();
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        retVal = FTs.Where(i => i.Index == index).FirstOrDefault();
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        retVal = ITs.Where(i => i.Index == index).FirstOrDefault();
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private bool isClickedTransferSource { get; set; } = false;
        private bool isClickedTransferTarget { get; set; } = false;
        private bool isSourceCard { get; set; } = false;

        private AsyncCommand _TransferSourceClickCommand;
        public ICommand TransferSourceClickCommand
        {
            get
            {
                if (null == _TransferSourceClickCommand) _TransferSourceClickCommand = new AsyncCommand(TransferSourceClickCommandFunc);
                return _TransferSourceClickCommand;
            }
        }
        bool sourceToggle = true;
        private async Task TransferSourceClickCommandFunc()
        {
            try
            {
                if (!targetToggle)
                {
                    TransferTarget = null;
                    isClickedTransferTarget = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    targetToggle = true;
                }

                if (sourceToggle)
                {
                    if (!isClickedTransferTarget)
                    {
                        IsSelected = false;
                        TransferSource = true;
                        isClickedTransferSource = true;
                        SetAllObjectEnable();
                        SetNotExistObjectDisable();


                        if (TransferTarget != null)
                        {
                            TransferTarget = null;
                            isClickedTransferTarget = false;
                            //if (IsCardObject(TransferTarget))
                            //{
                            //    isSourceCard = true;
                            //    SetDisalbeDontMoveCard();
                            //}
                            //else
                            //    SetDisableDontMoveWafer();
                        }

                    }
                    sourceToggle = false;
                }
                else
                {
                    SourceObj = null;
                    TransferSource = null;
                    isClickedTransferSource = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    sourceToggle = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TransferTargetClickCommand;
        public ICommand TransferTargetClickCommand
        {
            get
            {
                if (null == _TransferTargetClickCommand) _TransferTargetClickCommand = new AsyncCommand(TransferTargetClickCommandFunc);
                return _TransferTargetClickCommand;
            }
        }
        bool targetToggle = true;
        private async Task TransferTargetClickCommandFunc()
        {
            try
            {
                if (!sourceToggle)
                {
                    if (TransferSource is bool)
                    {
                        return;
                    }
                }

                Func<ModuleID, int> GetFoupInfo = (ModuleID moduleID) =>
                {
                    ICassetteModule cstModule = loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CST, 1) as ICassetteModule;
                    var slotCount = loaderModule.ModuleManager.FindSlots(cstModule).Count();

                    var slot = moduleID.Index % slotCount;
                    int oldoffset = 0;
                    if (slot == 0)
                    {
                        slot = slotCount;
                        oldoffset = -1;
                    }
                    var foup = ((moduleID.Index + oldoffset) / slotCount) + 1;

                    return foup;
                };

                if (targetToggle)
                {
                    if (!isClickedTransferSource)
                    {
                        IsSelected = false;
                        isClickedTransferTarget = true;
                        TransferTarget = true;

                        SetAllObjectEnable();
                        SetExistObjectDisable();

                        if (TransferSource != null)
                        {
                            if (TransferSource is StageObject)
                            {
                                foreach (var cell in Cells)
                                {
                                    cell.IsEnableTransfer = false;
                                }                                
                            }

                            if (TransferSource is StageObject && (TransferSource as StageObject).CardStatus == EnumSubsStatus.EXIST && (TransferSource as StageObject).WaferStatus == EnumSubsStatus.EXIST)
                            {

                            }
                            else if (IsSourceCardObject(TransferSource))
                            {
                                isSourceCard = true;
                                SetDisalbeDontMoveCard();
                            }
                            else
                            {
                                SetDisableDontMoveWafer();
                            }


                            if (IsTransferSorceWafersizeisundefined)
                            {
                                SetAllObjectDisable();
                                foreach (var it in ITs)
                                {
                                    IsEnableTransferINSP(it.Index, out object transfertaget);
                                    if (transfertaget != null)
                                    {
                                        it.IsEnableTransfer = true;
                                    }
                                }
                            }
                            else
                            {
                                // check cassette N type
                                if (IsTransferSorceUseOthertypesofcassettes)
                                {
                                    // case2. source: slot target: fixed tray => don't move
                                    if (TransferSource is SlotObject)
                                    {
                                        var subObj = TransferSource as SlotObject;
                                        int foup = GetFoupInfo(subObj.WaferObj.OriginHolder);
                                        CassetteTypeEnum curcassetteType = LoaderMaster.Loader.GetCassetteType(foup);
                                        if (curcassetteType != CassetteTypeEnum.FOUP_25)
                                        {
                                            foreach (var ft in FTs)
                                            {
                                                ft.IsEnableTransfer = false;
                                            }

                                            // disable foup_25 
                                            foreach (var fo in Foups)
                                            {
                                                if (fo.State == FoupStateEnum.LOAD && fo.ScanState == CassetteScanStateEnum.READ)
                                                {
                                                    CassetteTypeEnum cassetteType = LoaderMaster.Loader.GetCassetteType(fo.Index + 1);
                                                    if(cassetteType == CassetteTypeEnum.FOUP_25)
                                                    {
                                                        foreach (var slot in fo.Slots)
                                                        {
                                                            slot.IsEnableTransfer = false;
                                                        }
                                                    }                                                    
                                                }
                                            }
                                        }
                                    }

                                    ModuleID moduleID = new ModuleID();
                                    // case3. source: arm, pa, buffer / unknown -> exist
                                    if (TransferSource is ArmObject)
                                    {
                                        var subObj = TransferSource as ArmObject;
                                        moduleID = subObj.WaferObj.OriginHolder;
                                    }                                    
                                    if (TransferSource is PAObject)
                                    {
                                        var subObj = TransferSource as PAObject;
                                        moduleID = subObj.WaferObj.OriginHolder;
                                    }
                                    if (TransferSource is BufferObject)
                                    {
                                        var subObj = TransferSource as BufferObject;
                                        moduleID = subObj.WaferObj.OriginHolder;
                                    }
                                    if (TransferSource is StageObject)
                                    {
                                        var subObj = TransferSource as StageObject;
                                        moduleID = subObj.WaferObj.OriginHolder;
                                    }
                                    if(moduleID != null)
                                    {
                                        // case1. source: inspection,arm,pa,buffer,cell / target: cassete,fixed tray, polish wafer assign 안된 insp
                                        if (moduleID.ModuleType == ModuleTypeEnum.INSPECTIONTRAY || 
                                            TransferSource is InspectionTrayInfoObject ||
                                            TransferSource is ArmObject ||
                                            TransferSource is PAObject ||
                                            TransferSource is BufferObject ||
                                            TransferSource is StageObject)
                                        {
                                            SetAllObjectDisable();
                                            foreach (var it in Foups)
                                            {
                                                if (it.State == FoupStateEnum.LOAD && it.ScanState == CassetteScanStateEnum.READ)
                                                {
                                                    foreach (var slot in it.Slots)
                                                    {
                                                        var retval = IsEnableTransferSlot(it, slot);
                                                        if (retval == EventCodeEnum.NONE)
                                                        {
                                                            slot.IsEnableTransfer = true;
                                                        }
                                                    }
                                                }
                                            }
                                            foreach (var ft in FTs)
                                            {                                                
                                                if (ft.WaferStatus != EnumSubsStatus.EXIST)
                                                {                                                    
                                                    var retval = IsEnableTransferFixedTray(ft);
                                                    if (retval == EventCodeEnum.NONE)
                                                    {
                                                        ft.IsEnableTransfer = true;
                                                    }
                                                }
                                            }
                                            foreach (var it in ITs)
                                            {
                                                if (it.WaferStatus != EnumSubsStatus.EXIST)
                                                {                                                    
                                                    var retval = IsEnableTransferInspectionTrayNotAssignPW(it);
                                                    if (retval == EventCodeEnum.NONE)
                                                    {
                                                        it.IsEnableTransfer = true;
                                                    }
                                                }                                                    
                                            }
                                        }
                                    }

                                    foreach (var fo in Foups)       // foup_13 짝수 slot 선택 X
                                    {
                                        if (fo.State == FoupStateEnum.LOAD && fo.ScanState == CassetteScanStateEnum.READ)
                                        {
                                            CassetteTypeEnum cassetteType = LoaderMaster.Loader.GetCassetteType(fo.Index + 1);
                                            if (cassetteType == CassetteTypeEnum.FOUP_13)
                                            {
                                                foreach (var slot in fo.Slots)
                                                {
                                                    var retval = IsEnableTransferSlot(fo, slot);
                                                    if (retval == EventCodeEnum.NONE)
                                                    {
                                                        slot.IsEnableTransfer = true;
                                                    }
                                                    else
                                                    {
                                                        slot.IsEnableTransfer = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    targetToggle = false;
                }
                else
                {
                    TargetObj = null;
                    TransferTarget = null;
                    isClickedTransferTarget = false;

                    SetAllObjectEnable();

                    IsSelected = false;
                    targetToggle = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private object _TargetObj;
        public object TargetObj
        {
            get { return _TargetObj; }
            set
            {
                if (value != _TargetObj)
                {
                    _TargetObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private object _SourceObj;
        public object SourceObj
        {
            get { return _SourceObj; }
            set
            {
                if (value != _SourceObj)
                {
                    _SourceObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CancellationTokenSourcePack transferCancelTransferTokenSource = new CancellationTokenSourcePack();
        private bool transferAbort { get; set; } = false;
        public void TransferAbortAction(object cardChangeObj)
        {
            try
            {
                transferAbort = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TransferCommand;
        public ICommand TransferCommand
        {
            get
            {
                if (null == _TransferCommand) _TransferCommand = new AsyncCommand(TransferObjectFunc);
                return _TransferCommand;
            }
        }

        LoaderMap Map;
        private async Task<EventCodeEnum> TransferObjectTestFunc(object source, object target)
        {
            EventCodeEnum retCode = EventCodeEnum.UNDEFINED;
            try
            {

                Task task = new Task(() =>
                {
                    transferAbort = false;
                    var loader = loaderModule;
                    Map = loader.GetLoaderInfo().StateMap;

                    string sourceId = null;
                    ModuleID targetId = new ModuleID();
                    object transfermodule = null;
                    //Source Info
                    SetTransferInfoToModule(source, ref transfermodule, true);
                    if (transfermodule == null)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Source Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    sourceId = (string)transfermodule;
                    //Target Info
                    SetTransferInfoToModule(target, ref transfermodule, false);
                    if (transfermodule == null)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Destination Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    targetId = (ModuleID)transfermodule;

                    if (sourceId == null | targetId == null)
                        return;

                    TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == sourceId).FirstOrDefault();
                    ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == targetId).FirstOrDefault();

                    // Fixed Tray 또는 Inspection Tray 에서 Chuck으로 보내는 경우, Polish Wafer 설정이 되어 있을 때
                    if ((subObj.CurrPos.ModuleType == ModuleTypeEnum.FIXEDTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK) ||
                         (subObj.CurrPos.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK))
                    {
                        // DeviceManager로부터 Source의 데이터가 Polish Wafer로 설정되어 있는 Holder인지 알아온다.

                        DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                        TransferObject deviceInfo = null;
                        WaferSupplyMappingInfo supplyInfo = null;

                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == subObj.CurrPos.ModuleType && i.WaferSupplyInfo.ID.Index == subObj.CurrPos.Index);

                        if (supplyInfo != null)
                        {
                            deviceInfo = supplyInfo.DeviceInfo;
                        }

                        // Polish Wafer 사용되는 Holder
                        if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                        {
                            if (subObj.WaferType.Value != EnumWaferType.POLISH)
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer", $"Fixed tray's wafer is not a polish wafer. \nPlease proceed to the polish wafer setup.", EnumMessageStyle.Affirmative).Result;

                                return;

                                //if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                                //{

                                //}
                            }
                            else
                            {
                                // Polish Wafer로 설정은 되어 있음, 파라미터 유효성 검사

                                bool paramvalid = false;

                                if (subObj.PolishWaferInfo.DefineName == null || subObj.PolishWaferInfo.DefineName.Value == string.Empty || subObj.PolishWaferInfo.DefineName.Value == null)
                                {
                                    paramvalid = false;
                                }
                                else
                                {
                                    paramvalid = true;
                                }

                                // 유효성 검사 실패 시
                                if (paramvalid == false)
                                {
                                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer", $"Polish Wafer Information is not enough\nPlease check to the polish wafer parameters.", EnumMessageStyle.Affirmative).Result;

                                    return;
                                }
                            }
                        }
                    }

                    // CC인경우 (Loading prober card, Unloading prober card )저온인지 온도 체크
                    var ccUnload = subObj.CurrPos.ModuleType == ModuleTypeEnum.CC &
                    (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER
                        | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDTRAY);
                    var ccLoad = dstLoc.ID.ModuleType == ModuleTypeEnum.CC &
                    (subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDARM | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDBUFFER
                       | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDTRAY);
                    if (ccUnload | ccLoad)
                    {
                        int stageidx = -1;
                        if (ccUnload)
                        {
                            if (TransferSource != null)
                            {
                                stageidx = (TransferSource as StageObject).Index;
                            }
                            else
                            {
                                stageidx = subObj.CurrPos.Index;
                            }

                        }
                        else if (ccLoad)
                        {
                            //stageidx = (TransferTarget as StageObject).Index;
                            if (TransferTarget != null)
                            {
                                stageidx = (TransferTarget as StageObject).Index;
                            }
                            else
                            {
                                stageidx = dstLoc.ID.Index;
                            }
                        }

                        // CC Temp - 추후 파라미터 처리 필요 
                        //if (stageidx != -1)
                        //{
                        //    var tempClient = loaderCommunicationManager.GetTempControllerClient(stageidx);
                        //    if (tempClient != null)
                        //    {
                        //        //stage 온도 확인 : 저온이면 Ambiment 온도로 올린뒤 다시 동작하도록 한다.
                        //        var temp = tempClient.GetTemperature();
                        //        if (temp < 0)
                        //        {
                        //            var msgRet = await this.MetroDialogManager().ShowMessageDialog("Error Message",
                        //                $"Card Change operation is possible only at room temperature." +
                        //                $" A temperature of the selected stage is {temp}℃." +
                        //                $"Press OK to automatically set the temperature to a safe temperature and you can try again when it reaches normal temperature." +
                        //                $"Or direct fixes can be made in the Device Temperature set." +
                        //                $" After the card change, you must change the temperature to its original temperature.",
                        //                EnumMessageStyle.AffirmativeAndNegative);
                        //            if(msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                        //            {
                        //                tempClient.SetAmbientTemp();
                        //            }
                        //            return;
                        //        }
                        //    }
                        //}
                    }
                    //=========================================================

                    EventCodeEnum Dret = EventCodeEnum.UNDEFINED;
                    SetTransfer(subObj, dstLoc, ref Dret);

                    var mapSlicer = new LoaderMapSlicer();
                    var slicedMap = mapSlicer.ManualSlicing(Map);

                    if (slicedMap != null)
                    {
                        AllCstBtnDisable();

                        bool isError = false;

                        for (int i = 0; i < slicedMap.Count; i++)
                        {
                            loaderModule.SetRequest(slicedMap[i]);

                            while (true)
                            {
                                if (loader.ModuleState == ModuleStateEnum.DONE)
                                {
                                    loader.ClearRequestData();

                                    LoaderMapConvert(loader.GetLoaderInfo().StateMap);

                                    retCode = EventCodeEnum.NONE;

                                    Thread.Sleep(100);

                                    break;
                                }
                                else if (loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    LoaderRecoveryControlVM.Show(_Container, loader.ResonOfError, loader.ErrorDetails);
                                    loader.ResonOfError = "";
                                    loader.ErrorDetails = "";
                                    retCode = EventCodeEnum.UNDEFINED;
                                    isError = true;
                                    break;
                                }

                                Thread.Sleep(100);
                            }
                            if (isError)
                            {
                                break;
                            }

                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(33);
                    }
                    else
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"This is an incorrect operation.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    }
                });
                task.Start();
                await task;

                UpdateCSTBtnEnable();
                UpdateCSTBtnEnable();
                TransferTarget = null;
                isClickedTransferTarget = false;
                SetAllObjectEnable();
                IsSelected = false;
                targetToggle = true;
                TransferSource = null;
                isClickedTransferSource = false;
                sourceToggle = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return retCode;
        }
        private async Task TransferObjectFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                bool isExecute = true;

                Task task = new Task(() =>
                {
                    var loader = loaderModule;
                    Map = loader.GetLoaderInfo().StateMap;

                    string sourceId = null;
                    ModuleID targetId = new ModuleID();
                    ModuleID preOrigin = new ModuleID();
                    object transfermodule = null;

                    //Source Info
                    SetTransferInfoToModule(TransferSource, ref transfermodule, true);

                    if (transfermodule == null)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Source Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }

                    sourceId = (string)transfermodule;

                    //Target Info
                    SetTransferInfoToModule(TransferTarget, ref transfermodule, false);

                    if (transfermodule == null)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Destination Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }

                    targetId = (ModuleID)transfermodule;

                    if (sourceId == null || targetId == null)
                    {
                        return;
                    }

                    TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == sourceId).FirstOrDefault();
                    ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == targetId).FirstOrDefault();

                    bool IsCardhandling = subObj.CurrPos.ModuleType == ModuleTypeEnum.CC || subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDARM || subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDBUFFER || subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDTRAY;

                    if (IsCardhandling == false)
                    {
                        bool canusearm = false;
                        for (int i = 0; i < Map.ARMModules.Count(); i++)
                        {
                            if (Map.ARMModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST ||
                                subObj.CurrHolder.ModuleType == ModuleTypeEnum.ARM)
                            {
                                canusearm = true;
                                break;
                            }
                        }

                        if (canusearm == false)
                        {
                            var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the ARM.\n Please Check the ARM.", EnumMessageStyle.Affirmative).Result;
                            return;
                        }


                        bool canusePA = false;
                        for (int i = 0; i < Map.PreAlignModules.Count(); i++)
                        {
                            if (Map.PreAlignModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST ||
                                subObj.CurrHolder.ModuleType == ModuleTypeEnum.PA)
                            {
                                canusePA = true;
                                break;
                            }
                        }

                        if (canusePA == false)
                        {
                            var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a wafer on the PreAlign.\n Please Check the PreAlign.", EnumMessageStyle.Affirmative).Result;
                            return;
                        }

                        if(subObj.CurrHolder.ModuleType==ModuleTypeEnum.CHUCK || dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                        {
                            int stIdx = -1;
                            if(subObj.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                            {
                                stIdx = subObj.CurrHolder.Index;
                            }
                            else
                            {
                                stIdx = dstLoc.ID.Index;
                            }
                            var cardData = loaderCommunicationManager.GetProxy<IRemoteMediumProxy>(stIdx).GPCC_OP_GetCCVacuumStatus();
                            //=================

                            //1 카드팟이 올라가있고 카드팟위에 캐리어가 있을경우
                            if (cardData.IsLeftUpModuleUp == true || cardData.IsRightUpModuleUp == true )
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Cell{stIdx} is in CardPod Up status.\n Please try again after CardPod Down.", EnumMessageStyle.Affirmative).Result;
                                return;
                            }
                        }
                    }
                    else // Card Handling
                    {
                        bool causeCardARM = false;
                        string existobj = string.Empty;

                        for (int i = 0; i < Map.CardArmModule.Count(); i++)
                        {
                            // CardARM에 존재하지 않거나 Source가 CARDARM인 경우 허용
                            if (Map.CardArmModule[i].WaferStatus == EnumSubsStatus.NOT_EXIST || subObj.CurrHolder.ModuleType == ModuleTypeEnum.CARDARM)
                            {
                                causeCardARM = true;
                                break;
                            }
                            else
                            {
                                existobj = Map.CardArmModule[i].WaferStatus.ToString();
                            }
                        }

                        if (causeCardARM == false)
                        {
                            var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"There is already a {existobj} on the CardARM.\nPlease Check the CardARM.", EnumMessageStyle.Affirmative).Result;
                            return;
                        }
                    }

                    /* [ISSD-3418]
                    Origin : 원래 위치 
                    Source: 현재 위치
                    Target : 이동하려는 위치 
                    이렇게 세개의 위치 개념이 있는데, 여기서 

                    1) Origin 과 Source 가 다르고
                    2) Origin 과 Target 이 다른 경우 
                    Pop up을 띄워서 알려주도록 한다.

                    Foup에서 Cell 로 이동할때는 1)조건이 만족되지 않기에 ( Origin 과 Source 랑 같아서) popup 이 안뜰 것이고 
                    Origin 과 다른 위치에서 출발하는 모든 Object 들에 대해서 확인 가능하다.
                     */

                    // Source 의 원래 위치와 현재 위치의 ModuleType 다르거나, Index 가 다를경우 
                    //((subObj.OriginHolder.ModuleType != subObj.CurrHolder.ModuleType) || (subObj.OriginHolder.Index != subObj.CurrHolder.Index)) &&

                    // Target 으로 이동했을 때 Origin Position 이 바뀌는 경우에 대해서 확인. ( Slot, InspectionTray, FixedTray ) 

                    int TrayIdx = 0;
                    bool showMessage = true;

                    Func<ModuleID, (int, int)> GetSlotInfo = (ModuleID moduleID) =>
                    {
                        ICassetteModule cstModule = loader.ModuleManager.FindModule(ModuleTypeEnum.CST, 1) as ICassetteModule;
                        var slotCount = loader.ModuleManager.FindSlots(cstModule).Count();

                        var slot = moduleID.Index % slotCount;
                        int oldoffset = 0;
                        if (slot == 0)
                        {
                            slot = slotCount;
                            oldoffset = -1;
                        }
                        var foup = ((moduleID.Index + oldoffset) / slotCount) + 1;

                        return (foup, slot);
                    };


                    if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT || dstLoc.ID.ModuleType == ModuleTypeEnum.FIXEDTRAY || dstLoc.ID.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                    {
                        if (dstLoc.ID.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                        {
                            // CanUseBuffer 가 True인 FixedTray인 경우는 Origin이 변경되지 않기 때문에 메시지박스를 띄울 필요 없음. (버퍼와 동일하게 동작해야 함)
                            TrayIdx = dstLoc.ID.Index;
                            if (TrayIdx != 0)
                            {
                                showMessage = Map.FixedTrayModules[TrayIdx - 1].CanUseBuffer ? false : true;
                            }
                        }

                        if (showMessage)
                        {
                            if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                (int foup, int slot) = GetSlotInfo(dstLoc.ID);
                                if (subObj.Size.Value != GPLoader.GetDeviceSize(foup - 1)) 
                                { 
                                    this.MetroDialogManager().ShowMessageDialog("Transfer Error",
                                       $"The wafer sizes for the {subObj.CurrHolder.Label} and Foup {foup} Slot {slot} are different." +
                                       $"\n[Source Size] {subObj.Size.Value}\n[Target Size] {GPLoader.GetDeviceSize(foup - 1)}", EnumMessageStyle.Affirmative);
                                    return;
                                }
                            }

                            // Source 와 Target 의 ModuleType 이 다르거나, Index 가 다를 경우
                            if (((subObj.OriginHolder.ModuleType != dstLoc.ID.ModuleType) || (subObj.OriginHolder.Index != dstLoc.ID.Index)))
                            {
                                (int, int) originFoupSlotPosition = (0, 0);
                                (int, int) targetFoupSlotPosition = (0, 0);

                                // Origin
                                string originPosition;
                                if (subObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                {
                                    (int foup, int slot) = GetSlotInfo(subObj.OriginHolder);
                                    originFoupSlotPosition = (foup, slot);
                                    originPosition = $"FOUP : {foup}, SLOT : {slot}";
                                }
                                else
                                {
                                    originPosition = $"{subObj.OriginHolder.ModuleType}, INDEX : {subObj.OriginHolder.Index}";
                                }

                                // Target
                                string targetPosition;
                                if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                                {
                                    (int foup, int slot) = GetSlotInfo(dstLoc.ID);
                                    targetFoupSlotPosition = (foup, slot);
                                    targetPosition = $"FOUP : {foup}, SLOT : {slot}";
                                }
                                else
                                {
                                    targetPosition = $"{dstLoc.ID.ModuleType}, INDEX : {dstLoc.ID.Index}";
                                }

                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning",
                                       $"Wafer's Origin position and Target position are different. Do you want Continue? " +
                                       $"\n[Origin Position] {originPosition}" +
                                       $"\n[Target Position] {targetPosition}"
                                       , EnumMessageStyle.AffirmativeAndNegative).Result;

                                if (retVal == EnumMessageDialogResult.NEGATIVE)
                                {
                                    return;
                                }
                                else
                                {
                                    //Target Foup 의 LOT 상태가 Running 중이 아니거나, Dy mode인 경우에만 허용함.
                                    if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                                    {
                                        if (LoaderMaster.ModuleState.GetState() != ModuleStateEnum.IDLE)
                                        {
                                            if (LoaderMaster.DynamicMode == DynamicModeEnum.DYNAMIC)
                                            {
                                                // Origin과 다른 위치여도 Transfer 허용.
                                                LoggerManager.Debug($"TransferObjectFunc() : Loader Module State : {LoaderMaster.ModuleState.GetState()}, DY Mode :{LoaderMaster.DynamicMode}");
                                            }
                                            else
                                            {
                                                var transferObjs = Map.GetHolderModuleAll();
                                                transferObjs = transferObjs.FindAll(obj => obj.ID.ModuleType == ModuleTypeEnum.CHUCK
                                                  || obj.ID.ModuleType == ModuleTypeEnum.BUFFER
                                                  || obj.ID.ModuleType == ModuleTypeEnum.PA
                                                  || obj.ID.ModuleType == ModuleTypeEnum.ARM
                                                  || obj.ID.ModuleType == ModuleTypeEnum.INSPECTIONTRAY
                                                  || obj.ID.ModuleType == ModuleTypeEnum.FIXEDTRAY);

                                                if (transferObjs.Count(obj => obj.Substrate != null && obj.Substrate.OriginHolder == dstLoc.ID) != 0)
                                                {
                                                    if (subObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                                    {
                                                        if (originFoupSlotPosition.Item1 != 0 && targetFoupSlotPosition.Item1 != 0)
                                                        {
                                                            if (LoaderMaster.ActiveLotInfos[originFoupSlotPosition.Item1 - 1].State != LotStateEnum.Idle
                                                                || LoaderMaster.ActiveLotInfos[targetFoupSlotPosition.Item1 - 1].State != LotStateEnum.Idle)
                                                            {
                                                                this.MetroDialogManager().ShowMessageDialog("Transfer Error",
                                                                    "Cannot transfer wafer from a different origin position to a foup where LOT is running.", EnumMessageStyle.Affirmative);

                                                                LoggerManager.Debug($"TransferObjectFunc() : Loader Module State : {LoaderMaster.ModuleState.GetState()}, DY Mode :{LoaderMaster.DynamicMode}," +
                                                                    $"Origin Foup Index : {originFoupSlotPosition.Item1}, Foup Lot State : {LoaderMaster.ActiveLotInfos[originFoupSlotPosition.Item1 - 1].State}" +
                                                                    $"Target Foup Index : {targetFoupSlotPosition.Item1}, Foup Lot State : {LoaderMaster.ActiveLotInfos[targetFoupSlotPosition.Item1 - 1].State}");
                                                                return;
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (LoaderMaster.ActiveLotInfos[targetFoupSlotPosition.Item1 - 1].State != LotStateEnum.Idle)
                                                        {
                                                            this.MetroDialogManager().ShowMessageDialog("Transfer Error",
                                                                "Cannot transfer wafer from a different origin position to a foup where LOT is running.", EnumMessageStyle.Affirmative);

                                                            LoggerManager.Debug($"TransferObjectFunc() : Loader Module State : {LoaderMaster.ModuleState.GetState()}, DY Mode :{LoaderMaster.DynamicMode}," +
                                                                $"Target Foup Index : {targetFoupSlotPosition.Item1}, Foup Lot State : {LoaderMaster.ActiveLotInfos[targetFoupSlotPosition.Item1 - 1].State}");
                                                            return;
                                                        }
                                                    }
                                                }
                                            }                                                                                                                                   
                                        }
                                    }

                                    if (IsTransferSorceUseOthertypesofcassettes)
                                    {
                                        // INSPECTIONTRAY -> SLOT
                                        if (subObj.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                                        {
                                            subObj.PreAlignState = PreAlignStateEnum.NONE;
                                            var currHolder = Map.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                                            if (currHolder != null)
                                            {
                                                if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                                                {
                                                    retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning",
                                                        $"Mixed Cassette Mode.\nThe target cassette information is as follows.\nDo you want Continue?\n" +
                                                        $"\nTarget Cassette Infos." +
                                                        $"\n  Index: {targetFoupSlotPosition.Item1}" +
                                                        $"\n  Type: {LoaderMaster.Loader.GetCassetteType(targetFoupSlotPosition.Item1)}" +
                                                        $"\n  ID: {Foups[targetFoupSlotPosition.Item1 - 1].CassetteID}" +
                                                        $"\n  Lot State: {LoaderMaster.ActiveLotInfos[targetFoupSlotPosition.Item1 - 1].State}"
                                                        , EnumMessageStyle.AffirmativeAndNegative).Result;

                                                    if (retVal == EnumMessageDialogResult.NEGATIVE)
                                                    {
                                                        return;
                                                    }
                                                    // Holder 안에 NotchType을 바꿔주기.
                                                    var notch = currHolder.Substrate.NotchType;
                                                    subObj.NotchType = notch;

                                                    // Holder 안에 slot 정보 채우기. // OriginHolder 을 바꿔주는 이유: 돌아가려는 cassette type 에 맞게 이동하기 위해서.
                                                    preOrigin = new ModuleID(subObj.OriginHolder.ModuleType, subObj.OriginHolder.Index, subObj.OriginHolder.Label);
                                                    var Origin = new ModuleID(dstLoc.ID.ModuleType, dstLoc.ID.Index, dstLoc.ID.Label);
                                                    subObj.OriginHolder = Origin;

                                                    TransferObject currSubObj = loaderModule.ModuleManager.FindTransferObject(subObj.ID.Value);
                                                    if (currSubObj != null)
                                                    {
                                                        currSubObj.OriginHolder = Origin;
                                                        currSubObj.NotchType = notch;
                                                    }

                                                }
                                                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                                                {
                                                    var polishwaferinfo = this.DeviceManager().GetPolishWaferInformation(dstLoc.ID) as PolishWaferInformation;
                                                    if (polishwaferinfo != null)
                                                    {
                                                        retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning",
                                                         $"Mixed Cassette Mode.\nThe target fixedtray information is as follows.\nDo you want Continue?\n" +
                                                         $"\nTarget Fixedtray Infos.\n" +
                                                         $"  {dstLoc.ID.Label}, Polish Wafer Type: {polishwaferinfo.DefineName}"
                                                         , EnumMessageStyle.AffirmativeAndNegative).Result;

                                                        if (retVal == EnumMessageDialogResult.NEGATIVE)
                                                        {
                                                            return;
                                                        }

                                                        var notch = polishwaferinfo.NotchType.Value;
                                                        subObj.NotchType = notch;
                                                        TransferObject currSubObj = loaderModule.ModuleManager.FindTransferObject(subObj.ID.Value);
                                                        if (currSubObj != null)
                                                        {
                                                            currSubObj.NotchType = notch;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var ret = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Please make sure that {subObj.CurrPos.Label} is set to pw properly", EnumMessageStyle.Affirmative).Result;
                                                        LoggerManager.Debug($"TransferObjectFunc() WaferType : {subObj.CurrPos.Label} DeviceSize is PolishWaferInfo null");
                                                        return;
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ((subObj.CurrPos.ModuleType == ModuleTypeEnum.FIXEDTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK) ||
                         (subObj.CurrPos.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK))
                    {
                        var currHolder = Map.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                        if (loaderModule.LoaderMaster.ClientList.ContainsKey(dstLoc.ID.ToString()) && currHolder != null)
                        {
                            var CellInfo = loaderModule.LoaderMaster.GetDeviceInfoClient(loaderModule.LoaderMaster.ClientList[dstLoc.ID.ToString()]);
                            if (CellInfo.Size.Value != currHolder.Substrate.Size.Value)
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"{subObj.CurrPos.Label} DeviceSize is {currHolder.Substrate.Size.Value}.\n{dstLoc.ID.Label} DeviceSize is {CellInfo.Size.Value}.\nPlease check the DeviceSize", EnumMessageStyle.Affirmative).Result;
                                LoggerManager.Debug($"TransferObjectFunc() WaferType : {subObj.CurrPos.Label} DeviceSize is {currHolder.Substrate.Size.Value}, {dstLoc.ID.Label} DeviceSize is {CellInfo.Size.Value}");
                                return;
                            }
                        }
                    }
                    else if (subObj.CurrPos.ModuleType == ModuleTypeEnum.SLOT && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                    {
                        (int foup, int slot) = GetSlotInfo(subObj.CurrHolder);
                        if (loaderModule.LoaderMaster.ClientList.ContainsKey(dstLoc.ID.ToString()))
                        {
                            var CellInfo = loaderModule.LoaderMaster.GetDeviceInfoClient(loaderModule.LoaderMaster.ClientList[dstLoc.ID.ToString()]);
                            if (CellInfo.Size.Value != GPLoader.GetDeviceSize(foup - 1))
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"{subObj.CurrPos.Label} DeviceSize is {GPLoader.GetDeviceSize(foup - 1)}.\n{dstLoc.ID.Label} DeviceSize is {CellInfo.Size.Value}.\nPlease check the DeviceSize", EnumMessageStyle.Affirmative).Result;
                                LoggerManager.Debug($"TransferObjectFunc() WaferType : {subObj.CurrPos.Label} DeviceSize is {GPLoader.GetDeviceSize(foup - 1)}, {dstLoc.ID.Label} DeviceSize is {CellInfo.Size.Value}");
                                return;
                            }
                        }
                    }


                    if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                    {
                        if (subObj.WaferType.Value == EnumWaferType.STANDARD)
                        {
                            LoaderMaster.SetAngleInfo(dstLoc.ID.Index, subObj);
                            LoaderMaster.SetNotchType(dstLoc.ID.Index, subObj);
                        }
                    }
                    if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDTRAY)
                    {
                        ICardBufferTrayModule cardTrayModule = loader.ModuleManager.FindModule(dstLoc.ID) as ICardBufferTrayModule;
                        if (!cardTrayModule.IsDrawerSensorOn())
                        {
                            var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Card Tray{cardTrayModule.ID.Index} Open Sensor On.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                            return;
                        }
                    }
                    // Fixed Tray 또는 Inspection Tray 에서 Chuck으로 보내는 경우, Polish Wafer 설정이 되어 있을 때
                    if ((subObj.CurrPos.ModuleType == ModuleTypeEnum.FIXEDTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK) ||
                         (subObj.CurrPos.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK))
                    {
                        // DeviceManager로부터 Source의 데이터가 Polish Wafer로 설정되어 있는 Holder인지 알아온다.

                        DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                        TransferObject deviceInfo = null;
                        WaferSupplyMappingInfo supplyInfo = null;

                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == subObj.CurrPos.ModuleType && i.WaferSupplyInfo.ID.Index == subObj.CurrPos.Index);

                        if (supplyInfo != null)
                        {
                            deviceInfo = supplyInfo.DeviceInfo;
                        }

                        // Polish Wafer 사용되는 Holder
                        if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                        {
                            if (subObj.WaferType.Value != EnumWaferType.POLISH)
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer", $"Fixed tray's wafer is not a polish wafer. \nPlease proceed to the polish wafer setup.", EnumMessageStyle.Affirmative).Result;

                                return;
                            }
                            else
                            {
                                // Polish Wafer로 설정은 되어 있음, 파라미터 유효성 검사

                                bool paramvalid = false;

                                if (subObj.PolishWaferInfo.DefineName == null || subObj.PolishWaferInfo.DefineName.Value == string.Empty || subObj.PolishWaferInfo.DefineName.Value == null)
                                {
                                    paramvalid = false;
                                }
                                else
                                {
                                    paramvalid = true;
                                }

                                // 유효성 검사 실패 시
                                if (paramvalid == false)
                                {
                                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer", $"Polish Wafer Information is not enough\nPlease check to the polish wafer parameters.", EnumMessageStyle.Affirmative).Result;

                                    return;
                                }
                            }
                        }

                        
                    }

                    if ((loaderModule as LoaderModule).GP_ManualOCREnable)
                    {
                        IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(subObj.CurrPos.ModuleType, subObj.CurrPos.Index);
                        IWaferOwnable ownable = attachedmodule as IWaferOwnable;
                        var sourceobj = ownable.Holder.TransferObject;

                        //아래의 로직은 OCR Enable일 때 Config 를 Set 해주기 위한 로직
                        var dstIsPolishSource = this.DeviceManager().GetPolishSourceModules().Where(m => m.ModuleType == dstLoc.ID.ModuleType
                                                                                && m.Index == dstLoc.ID.Index).FirstOrDefault();
                        if (dstIsPolishSource != null &&  (dstIsPolishSource.ModuleType == ModuleTypeEnum.FIXEDTRAY || dstIsPolishSource.ModuleType == ModuleTypeEnum.INSPECTIONTRAY))
                        {
                            var polishwaferinfo = this.DeviceManager().GetPolishWaferInformation(dstIsPolishSource) as PolishWaferInformation;
                            if (polishwaferinfo != null) //getpolishwaferinformation 함수에서 getpolishwafersources가 null인 경우 예외 상황이 발생 할 수 있음.
                            {
                                var prev_WaferType = sourceobj.WaferType.Value;
                                sourceobj.WaferType.Value = EnumWaferType.POLISH; //transfer obj clone으로 set 될 수 있는 값.
                                sourceobj.PolishWaferInfo = polishwaferinfo;
                                sourceobj.OCRDevParam = polishwaferinfo.OCRConfigParam;
                                sourceobj.OCR.Value = string.Empty;
                                //정의된 PW Type이 있음. 
                                LoggerManager.Debug($"[LoaderHandlingViewModel] TransferObjectFunc(): defined polish wafer infomation is exist. Set PolishWaferInfo" +
                                                                        $" definename:{sourceobj.PolishWaferInfo.DefineName.Value}," +
                                                                        $" wafertype: {prev_WaferType} => {sourceobj.WaferType.Value}," +
                                                                        $" source:{subObj.CurrPos.ModuleType}.{subObj.CurrPos.Index}" +
                                                                        $" destination:{subObj.DstPos.ModuleType}.{subObj.DstPos.Index}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[LoaderHandlingViewModel] TransferObjectFunc(): defined polish wafer information is null.");
                        }


                        if (sourceobj.WaferType.Value == EnumWaferType.POLISH)
                        {
                            //PolishWaferInfo 가 변경 되었을 때 업데이트 하고 있기 때문
                            sourceobj.OCRAngle.Value = sourceobj.PolishWaferInfo.OCRConfigParam.OCRAngle;
                        }
                    }
                    
                    // CC인경우 (Loading prober card, Unloading prober card )저온인지 온도 체크
                    var ccUnload = subObj.CurrPos.ModuleType == ModuleTypeEnum.CC &
                    (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER
                        | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDTRAY);
                    var ccLoad = dstLoc.ID.ModuleType == ModuleTypeEnum.CC &
                    (subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDARM | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDBUFFER
                       | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDTRAY);
                    if (ccUnload | ccLoad)
                    {
                        int stageidx = -1;
                        if (ccUnload)
                        {
                            stageidx = (TransferSource as StageObject).Index;
                            //#Hynix_Merge: GetRemoteMediumClient -> GetClient로 바꿔야함.
                            var data = new ProberInterfaces.ViewModel.CardChangeVacuumAndIOStatus();// 임시코드
                            data = loaderCommunicationManager.GetProxy<IRemoteMediumProxy>(stageidx).GPCC_OP_GetCCVacuumStatus();
                            //=================

                            //1 카드팟이 올라가있고 카드팟위에 캐리어가 있을경우
                            if (data.IsLeftUpModuleUp == true && data.IsRightUpModuleUp == true && data.IsCardExistOnCardPod == true)
                            {
                                if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM)
                                {
                                    var cardArm = Map.CardArmModule.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                    if (cardArm != null)
                                    {
                                        if (cardArm.WaferStatus == EnumSubsStatus.NOT_EXIST)
                                        {
                                            //수행 가능
                                            isExecute = true;
                                        }
                                        else
                                        {
                                            var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardArm WaferStatus : {cardArm.WaferStatus}", EnumMessageStyle.Affirmative);
                                            isExecute = false;
                                            //수행 못함
                                        }
                                    }
                                    else
                                    {
                                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"Card Arm  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                        //못한다.
                                    }
                                }
                                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER)
                                {
                                    var cardBuffer = Map.CardBufferModules.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                    if (cardBuffer != null)
                                    {
                                        if (cardBuffer.WaferStatus == EnumSubsStatus.NOT_EXIST)
                                        {
                                            //수행 가능
                                            isExecute = true;
                                        }
                                        else
                                        {
                                            //못한다.
                                            isExecute = false;
                                            var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer WaferStatus : {cardBuffer.WaferStatus}", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                    else
                                    {
                                        //못한다.
                                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                    }
                                }

                            }
                            else if (data.IsLeftUpModuleUp == false && data.IsRightUpModuleUp == false && data.IsCardExistOnCardPod == false)
                            {
                                if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM)
                                {
                                    var cardArm = Map.CardArmModule.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                    if (cardArm != null)
                                    {
                                        if (cardArm.WaferStatus == EnumSubsStatus.CARRIER)
                                        {
                                            isExecute = true;
                                        }
                                        else if (cardArm.WaferStatus == EnumSubsStatus.NOT_EXIST)//버퍼에 캐리어 있는데 카드 암 선택해서 이동시키려 할때
                                        {
                                            //if(버퍼 어딘가에 캐리어가 있을경우에는 가능하다)
                                            // else 나머지는 불가능.
                                        }
                                        else
                                        {
                                            var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"card Arm  info Null", EnumMessageStyle.Affirmative);
                                            isExecute = false;
                                        }
                                    }
                                    else
                                    {
                                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"card Arm  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                    }
                                }
                                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER)
                                {
                                    var cardBuffer = Map.CardBufferModules.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                    if (cardBuffer != null)
                                    {
                                        EnumCardChangeType clientCardType = EnumCardChangeType.UNDEFINED;
                                        clientCardType = loaderModule.LoaderMaster.GetClient(stageidx).GetCardChangeType();

                                        if (clientCardType == EnumCardChangeType.CARRIER)
                                        {
                                            if (cardBuffer.WaferStatus == EnumSubsStatus.CARRIER)
                                            {
                                                isExecute = true;
                                            }
                                            else
                                            {
                                                isExecute = false;
                                                var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer WaferStatus : {cardBuffer.WaferStatus}", EnumMessageStyle.Affirmative);
                                            }
                                        }
                                        else if (clientCardType == EnumCardChangeType.DIRECT_CARD)
                                        {
                                            if (cardBuffer.WaferStatus == EnumSubsStatus.NOT_EXIST)
                                            {
                                                isExecute = true;
                                            }
                                            else
                                            {
                                                isExecute = false;
                                                var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer WaferStatus : {cardBuffer.WaferStatus}", EnumMessageStyle.Affirmative);
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"TransferObjectFunc() Error dstLoc = {dstLoc.ID.ModuleType}, CardChangeModuleType = {this.CardChangeModule().GetCCType()}");
                                        }
                                    }
                                    else
                                    {
                                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                        else if (ccLoad)
                        {
                            stageidx = (TransferTarget as StageObject).Index;
                        }

                        // CC Temp - 추후 파라미터 처리 필요 
                        if(isExecute && stageidx != -1)
                        {
                            var tempClient = loaderCommunicationManager.GetProxy<ITempControllerProxy>(stageidx);
                            if (tempClient != null)
                            {
                                //stage 온도 확인 : 현재 온도가 CCActivatableTemp 보다 낮으면 메시지 출력.
                                transferAbort = false;
                                var temp = tempClient.GetTemperature();
                                var ccActemp = tempClient.GetCCActivatableTemp();
                                var client = loaderModule.LoaderMaster.GetClient(stageidx);
                                if (client != null)
                                {
                                    bool needToSetSV = client.NeedToSetCCActivatableTemp();

                                    var checkCCActiveTemp = loaderModule.LoaderMaster.GetClient(stageidx)?.CardChangeIsConditionSatisfied(needToSetTempToken: false);
                                    if (checkCCActiveTemp != EventCodeEnum.NONE || needToSetSV)
                                    {
                                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Error Message",
                                           $"Card Change operation is possible only above then CC Activatable Temperature : {ccActemp}.\n" +
                                           $" A temperature of the selected stage is {Math.Round(temp, 2)}℃.\n" +
                                           $"Do you want to set the operating conditions and wait?",
                                           EnumMessageStyle.AffirmativeAndNegative).Result;

                                        if (msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                                        {
                                            this.MetroDialogManager().SetMessageToWaitCancelDialog($"Waiting for temperature change to {ccActemp}℃.");
                                            this.MetroDialogManager().ShowCancelButtonOfWaitCancelDiaglog(transferCancelTransferTokenSource.TokenSource, "Abort", TransferAbortAction, this, true);

                                            if (needToSetSV)
                                            {
                                                client.SetCCActiveTemp();
                                                needToSetSV = false;
                                            }

                                            bool canDoAction = false;
                                            while (canDoAction == false)
                                            {
                                                if(transferAbort)
                                                {
                                                    client.RecoveryCCBeforeTemp();
                                                    this.MetroDialogManager().HiddenCancelButtonOfWaitCancelDiaglog();
                                                    return;
                                                }
                                                else
                                                {
                                                    var cardChangeIsConditionSatisfied = loaderModule.LoaderMaster.GetClient(stageidx)?.CardChangeIsConditionSatisfied(needToSetTempToken: needToSetSV) ?? EventCodeEnum.UNDEFINED;
                                                    if (cardChangeIsConditionSatisfied == EventCodeEnum.NONE)
                                                    {
                                                        canDoAction = true;
                                                        this.MetroDialogManager().SetMessageToWaitCancelDialog($"Waiting for card change");
                                                        this.MetroDialogManager().HiddenCancelButtonOfWaitCancelDiaglog();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    string transferToTrayFailReason = "";
                    var retval = this.TranferManager().ValidationTransferToTray(TransferSource, TransferTarget, ref transferToTrayFailReason);
                    if (retval != EventCodeEnum.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Transfer Error", $"{transferToTrayFailReason}", EnumMessageStyle.Affirmative);
                        return;
                    }

                    bool canExcuteMap = false;
                    var remainJob = loaderModule.LoaderJobViewList.Count(x => x.JobDone == false);
                    if (remainJob == 0)
                    {
                        canExcuteMap = true;
                    }
                    else
                    {
                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Error Message",
                                        $"Loader is Busy\n LoaderJob already exist." ,                                        
                                        EnumMessageStyle.Affirmative);
                        return;
                    }

                    //=========================================================

                    if (isExecute)
                    {
                        EventCodeEnum Dret = EventCodeEnum.UNDEFINED;
                        SetTransfer(subObj, dstLoc, ref Dret);

                        var mapSlicer = new LoaderMapSlicer();
                        var slicedMap = mapSlicer.ManualSlicing(Map);
                        bool isError = false;

                        if (slicedMap?.Count > 0)
                        {
                            AllCstBtnDisable();
                            isError = false;

                            for (int i = 0; i < slicedMap.Count; i++)
                            {
                                var request_retVal = loaderModule.SetRequest(slicedMap[i]);

                                if (request_retVal.IsSucceed == false)
                                {
                                    loader.ResonOfError = $"Loader module state is {loader.ModuleState}";
                                    loader.ErrorDetails = "Loader operation request failure. Please try again.";
                                    LoaderRecoveryControlVM.Show(_Container, loader.ResonOfError, loader.ErrorDetails);
                                    loader.ResonOfError = "";
                                    loader.ErrorDetails = "";
                                    loader.RecoveryBehavior = "";
                                    loader.RecoveryCellIdx = -1;
                                    isError = true;
                                    break;
                                }
                                while (true)
                                {
                                    if (loader.ExceedRunningStateDuration())
                                    {
                                        loader.ResonOfError = $"An error occurred that caused the loader to stop{Environment.NewLine}for 5 minutes.";
                                        loader.SetEMGSTOP(); //loader modulestate가 error가 되어 next tick에서 external error가 발생한다.
                                    }

                                    if (loader.ModuleState == ModuleStateEnum.DONE)
                                    {
                                        loader.ClearRequestData();

                                        LoaderMapConvert(loader.GetLoaderInfo().StateMap);

                                        Thread.Sleep(100);

                                        break;
                                    }
                                    else if (loader.ModuleState == ModuleStateEnum.ERROR)
                                    {
                                        if (IsTransferSorceUseOthertypesofcassettes)
                                        {
                                            // INSPECTIONTRAY -> SLOT
                                            if (preOrigin.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                                            {
                                                // Holder 안에 slot 정보 채우기. // OriginHolder 을 바꿔주는 이유: 돌아가려는 cassette type 에 맞게 이동하기 위해서.
                                                subObj.OriginHolder = preOrigin;

                                                TransferObject currSubObj = loaderModule.ModuleManager.FindTransferObject(subObj.ID.Value);
                                                if (currSubObj != null)
                                                {
                                                    // Exist
                                                    currSubObj.OriginHolder = preOrigin;
                                                    LoggerManager.Debug($"TransferObjectFunc(): Restores Origin from {subObj.CurrHolder.ModuleType} to {preOrigin.ModuleType} again.");
                                                }
                                                else
                                                {
                                                    // unknown
                                                    var attachedModules = this.loaderModule.ModuleManager.FindModules<IWaferOwnable>().Where(item => item.Holder.Status == EnumSubsStatus.UNKNOWN);
                                                    foreach (var item in attachedModules)
                                                    {
                                                        if (item.Holder.TransferObject.ID.Value == subObj.ID.Value)
                                                        {
                                                            item.Holder.TransferObject.OriginHolder = preOrigin;
                                                            item.Holder.TransferObject.DstPos = preOrigin;
                                                            LoggerManager.Debug($"TransferObjectFunc(): Restores Origin from {item} () to {preOrigin.Label} again. (TransferObject)");
                                                        }

                                                        if (item.Holder.BackupTransferObject.ID.Value == subObj.ID.Value)
                                                        {
                                                            item.Holder.BackupTransferObject.OriginHolder = preOrigin;
                                                            item.Holder.BackupTransferObject.DstPos = preOrigin;
                                                            LoggerManager.Debug($"TransferObjectFunc(): Restores Origin from {item} () to {preOrigin.Label} again. (BackupTransferObject)");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        LoaderRecoveryControlVM.Show(_Container, loader.ResonOfError, loader.ErrorDetails, loader.RecoveryBehavior, loader.RecoveryCellIdx);
                                        loader.ResonOfError = "";
                                        loader.ErrorDetails = "";
                                        loader.RecoveryBehavior = "";
                                        loader.RecoveryCellIdx = -1;
                                        isError = true;
                                        break;
                                    }

                                    Thread.Sleep(100);
                                }
                                if (isError)
                                {
                                    break;
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(33);
                        }
                        else
                        {
                            var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"This is an incorrect operation.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                            if (IsTransferSorceUseOthertypesofcassettes)
                            {
                                // INSPECTIONTRAY -> SLOT
                                if (preOrigin.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                                {
                                    // Holder 안에 slot 정보 채우기. // OriginHolder 을 바꿔주는 이유: 돌아가려는 cassette type 에 맞게 이동하기 위해서.
                                    subObj.OriginHolder = preOrigin;

                                    TransferObject currSubObj = loaderModule.ModuleManager.FindTransferObject(subObj.ID.Value);
                                    if (currSubObj != null)
                                    {
                                        // Exist
                                        currSubObj.OriginHolder = preOrigin;
                                        LoggerManager.Debug($"TransferObjectFunc(): Restores Origin from {subObj.CurrHolder.ModuleType} to {preOrigin.ModuleType} again.");
                                    }
                                    else
                                    {
                                        // unknown
                                        var attachedModules = this.loaderModule.ModuleManager.FindModules<IWaferOwnable>().Where(item => item.Holder.Status == EnumSubsStatus.UNKNOWN);
                                        foreach (var item in attachedModules)
                                        {
                                            if (item.Holder.TransferObject.ID.Value == subObj.ID.Value)
                                            {
                                                item.Holder.TransferObject.OriginHolder = preOrigin;
                                                item.Holder.TransferObject.DstPos = preOrigin;
                                                LoggerManager.Debug($"TransferObjectFunc(): Restores Origin from {item} () to {preOrigin.Label} again. (TransferObject)");
                                            }

                                            if (item.Holder.BackupTransferObject.ID.Value == subObj.ID.Value)
                                            {
                                                item.Holder.BackupTransferObject.OriginHolder = preOrigin;
                                                item.Holder.BackupTransferObject.DstPos = preOrigin;
                                                LoggerManager.Debug($"TransferObjectFunc(): Restores Origin from {item} () to {preOrigin.Label} again. (BackupTransferObject)");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    else
                    {

                    }
                });
                task.Start();
                await task;

                UpdateCSTBtnEnable();
                UpdateCSTBtnEnable();
                TransferTarget = null;
                isClickedTransferTarget = false;
                SetAllObjectEnable();
                IsSelected = false;
                targetToggle = true;
                TransferSource = null;
                isClickedTransferSource = false;
                sourceToggle = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public async Task RepeatTransfer(string cardID, int pinAlignInterval, int repeatCnt, bool skipDock, EnumRepeatedTransferMode mode, int delayTime)
        {
            try
            {
                loaderModule.LoaderMaster.CardIDFullWord = cardID;
                PinAlignIntervalCnt = pinAlignInterval;
                TestRepeatCnt = repeatCnt;
                SkipDocking = skipDock;
                RepeatedTransferMode = mode;
                if (delayTime < 1000)
                {
                    RepeatDelayTime = 1000;
                }
                else
                {
                    RepeatDelayTime = delayTime;
                }

                LoggerManager.Debug($"LoaderHandlingViewModel.RepeatTransfer() " +
                    $"CardID : {cardID}, " +
                    $"PinAlignIntervalCnt : {pinAlignInterval}, " +
                    $"TestRepeatCnt : {repeatCnt}, " +
                    $"TestRepeatDelayTime : {delayTime}, " +
                    $"SkipDocking : {skipDock}, " +
                    $"RepeatedMode : {mode.ToString()}");

                CancelRepeat = false;
                TransferEnable = false;
                TransferDone = false;

                Task transferTestTask = new Task(async () =>
                {
                    await RepeatedTransferFunc();
                });
                transferTestTask.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private async Task RepeatedTransferTestCommandFunc()
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    if(false)
        //    {
        //        stopwatch.Start();
        //        CancelRepeat = true;

        //        await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), $"Wait for transfer test cancelation.");
        //        await transferTestTask;
        //        CancelRepeat = true;
        //        while (CancelRepeat)
        //        {
        //            if (stopwatch.Elapsed.TotalSeconds > 120)
        //            {
        //                break;
        //            }
        //            Thread.Sleep(100);
        //        }
        //        await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
        //    }
        //    else
        //    {
        //        CancelRepeat = false;
        //        TransferEnable = false;
        //        TransferDone = false;
        //        transferTestTask = new Task(async () =>
        //        {
        //            await RepeatedTransferFunc();
        //        });
        //        transferTestTask.Start();
        //    }
        //}
        private bool finishStopwatch = false;

        public async Task RunStopwatchTotal()
        {
            try
            {
                Task task = new Task(async () =>
                {
                    Stopwatch stopwatchTotal = new Stopwatch();
                    stopwatchTotal.Start();
                    finishStopwatch = true;
                    while (finishStopwatch)
                    {
                        RTElapsedTimeTotal = stopwatchTotal.Elapsed;
                        Thread.Sleep(100);
                    }
                });
                task.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task RepeatedTransferFunc()
        {
            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), $"Repeating Transfer...");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            RTElapsedTimeTotal = TimeSpan.Zero;
            RTElapsedTimeLasted = TimeSpan.Zero;
            CurrentCnt = 1;
            int repeatCount = TestRepeatCnt;
            var loader = loaderModule;
            try
            {
                RunStopwatchTotal();
                Stopwatch stopwatchLasted = new Stopwatch();


                LoaderServiceBase.ILoaderServiceCallback callback = null;
                if (TargetObj is StageObject)
                {
                    int chuckindex = Convert.ToInt32(((TargetObj as StageObject).Name.Split('#'))[1]);

                    callback = LoaderMaster.GetClient(chuckindex);
                }

                while (!CancelRepeat)
                {
                    if (RepeatedTransferMode == EnumRepeatedTransferMode.OneCellMode)
                    {
                        stopwatchLasted.Start();
                        var result = await TransferObjectTestFunc(SourceObj, TargetObj);  // Source -> Target
                        Thread.Sleep(RepeatDelayTime); // 바로 Transfer 시도 시 Transfer Module State가 아직 idle로 변경되지 않아 Busy 에러 발생.

                        if (CancelRepeat) { break; }

                        if (result != EventCodeEnum.NONE)
                        {
                            break;
                        }

                        if (SourceObj is CardBufferObject || SourceObj is CardTrayObject && TargetObj is StageObject)
                        {
                            if (PinAlignIntervalCnt != 0 && SkipDocking == false)
                            {
                                if ((CurrentCnt % PinAlignIntervalCnt) == 0 || CurrentCnt == 1)
                                {
                                    if (callback != null)
                                    {
                                        retVal = callback.DoPinAlign(); //PinAlign
                                        LoggerManager.Debug($"[Repeat Test] PinAlign succeded.");
                                    }
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[Repeat Test] PinAlign failed. {retVal}");
                                        break;
                                    }
                                }
                            }
                        }
                        result = await TransferObjectTestFunc(TargetObj, SourceObj); // Target -> Source
                        Thread.Sleep(RepeatDelayTime); // 바로 Transfer 시도 시 Transfer Module State가 아직 idle로 변경되지 않아 Busy 에러 발생.

                        if (CancelRepeat) { break; }

                        if (result != EventCodeEnum.NONE)
                        {
                            break;
                        }

                        LoggerManager.Debug($"[Repeat Test] Transfer Done. Progressing...({CurrentCnt}/{repeatCount})");
                        CurrentCnt++;
                        if (CurrentCnt > repeatCount & repeatCount > 0)
                        {
                            break;
                        }
                        if (result != EventCodeEnum.NONE)
                        {
                            CancelRepeat = false;
                            LoggerManager.Debug($"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).");
                            await this.MetroDialogManager().ShowMessageDialog(
                                "Test error",
                                $"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).",
                                EnumMessageStyle.Affirmative);
                            break;
                        }

                        if (loader.ModuleState == ModuleStateEnum.ERROR)
                        {
                            LoggerManager.Debug($"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).");
                            await this.MetroDialogManager().ShowMessageDialog(
                                "Test error",
                                $"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).",
                                EnumMessageStyle.Affirmative);
                            break;
                        }
                        RTElapsedTimeLasted = stopwatchLasted.Elapsed;
                        stopwatchLasted.Restart();
                    }
                    else if (RepeatedTransferMode == EnumRepeatedTransferMode.MultipleCellMode)
                    {
                        stopwatchLasted.Start();

                        foreach (var item in loaderCommunicationManager.Cells)
                        {
                            if (item.StageMode == GPCellModeEnum.MAINTENANCE)
                            {
                                TargetObj = (object)item;

                                var result = await TransferObjectTestFunc(SourceObj, TargetObj);  // Source -> Target
                                Thread.Sleep(RepeatDelayTime); // 바로 Transfer 시도 시 Transfer Module State가 아직 idle로 변경되지 않아 Busy 에러 발생.

                                if (CancelRepeat) { break; }

                                if (result != EventCodeEnum.NONE)
                                {
                                    CancelRepeat = true;
                                    break;
                                }

                                if (SourceObj is CardBufferObject || SourceObj is CardTrayObject && TargetObj is StageObject)
                                {
                                    if (PinAlignIntervalCnt != 0 && SkipDocking == false)
                                    {
                                        if ((CurrentCnt % PinAlignIntervalCnt) == 0 || CurrentCnt == 1)
                                        {
                                            if (callback != null)
                                            {
                                                retVal = callback.DoPinAlign(); //PinAlign
                                                LoggerManager.Debug($"[Repeat Test] PinAlign succeded.");
                                            }
                                            if (retVal != EventCodeEnum.NONE)
                                            {
                                                CancelRepeat = true;
                                                LoggerManager.Debug($"[Repeat Test] PinAlign failed. {retVal}");
                                                break;
                                            }
                                        }
                                    }
                                }
                                result = await TransferObjectTestFunc(TargetObj, SourceObj); // Target -> Source
                                Thread.Sleep(RepeatDelayTime); // 바로 Transfer 시도 시 Transfer Module State가 아직 idle로 변경되지 않아 Busy 에러 발생.

                                if (CancelRepeat) { break; }

                                if (result != EventCodeEnum.NONE)
                                {
                                    CancelRepeat = true;
                                    break;
                                }


                                if (result != EventCodeEnum.NONE)
                                {
                                    CancelRepeat = false;
                                    LoggerManager.Debug($"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).");
                                    await this.MetroDialogManager().ShowMessageDialog(
                                        "Test error",
                                        $"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).",
                                        EnumMessageStyle.Affirmative);
                                    CancelRepeat = true;
                                    break;
                                }

                                if (loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    LoggerManager.Debug($"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).");
                                    await this.MetroDialogManager().ShowMessageDialog(
                                        "Test error",
                                        $"[Repeat Test] Transfer error({CurrentCnt}/{repeatCount}).",
                                        EnumMessageStyle.Affirmative);
                                    CancelRepeat = true;
                                    break;
                                }
                                RTElapsedTimeLasted = stopwatchLasted.Elapsed;
                                stopwatchLasted.Restart();
                            }
                        }
                        LoggerManager.Debug($"[Repeat Test] Transfer Done. Progressing...({CurrentCnt}/{repeatCount})");
                        CurrentCnt++;
                        if (CurrentCnt > repeatCount & repeatCount > 0)
                        {
                            CancelRepeat = true;
                            break;
                        }
                    }
                }

                finishStopwatch = false;
                CurrentCnt--;
                CancelRepeat = false;
                TransferDone = true;
                SourceObj = null;
                TargetObj = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                TransferDone = true;
                TransferEnable = true;
                PinAlignIntervalCnt = 1;
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private bool _TransferDone = false;
        public bool TransferDone
        {

            get { return _TransferDone; }
            set
            {
                if (value != _TransferDone)
                {
                    _TransferDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CancellationTokenSourcePack _CancelTransferTokenPack = new CancellationTokenSourcePack();
        public CancellationTokenSourcePack CancelTransferTokenPack
        {
            get { return _CancelTransferTokenPack; }
            set
            {
                if (value != _CancelTransferTokenPack)
                {
                    _CancelTransferTokenPack = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private AsyncCommand _CardLoadingTestCommand;
        //public ICommand CardLoadingTestCommand
        //{
        //    get
        //    {
        //        if (null == _CardLoadingTestCommand) _CardLoadingTestCommand = new AsyncCommand(CardLoadingTestCommandFunc, false);
        //        return _CardLoadingTestCommand;
        //    }
        //}

        private AsyncCommand _CancelTansfer;
        public ICommand CancelTansfer
        {
            get
            {
                if (null == _CancelTansfer) _CancelTansfer = new AsyncCommand(CancelTansferFunc);
                return _CancelTansfer;
            }
        }
        public async Task CancelTansferFunc()
        {
            try
            {
                Task task = new Task(async () =>
                {
                    CancelRepeat = true;
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), $"Wait for transfer test cancelation.");
                    while (CancelRepeat)
                    {
                        Thread.Sleep(100);
                    }
                    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetTransferDoneState()
        {
            bool retVal = false;
            try
            {
                retVal = TransferDone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public TimeSpan GetRTElapsedTimeTotal()
        {
            TimeSpan retVal = TimeSpan.Zero;
            try
            {
                retVal = RTElapsedTimeTotal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public TimeSpan GetRTElapsedTimeLasted()
        {
            TimeSpan retVal = TimeSpan.Zero;
            try
            {
                retVal = RTElapsedTimeLasted;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetTransferCurrentCount()
        {
            int retVal = 0;
            try
            {
                retVal = CurrentCnt;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object getSourceObject()
        {
            object retVal = 0;
            try
            {
                retVal = SourceObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object getTargetObject()
        {
            object retVal = 0;
            try
            {
                retVal = TargetObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private bool _CancelRepeat = true;
        public bool CancelRepeat
        {

            get { return _CancelRepeat; }
            set
            {
                if (value != _CancelRepeat)
                {
                    _CancelRepeat = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TestRepeatCnt = 0;
        public int TestRepeatCnt
        {

            get { return _TestRepeatCnt; }
            set
            {
                if (value != _TestRepeatCnt)
                {
                    _TestRepeatCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PinAlignIntervalCnt = 1;
        public int PinAlignIntervalCnt
        {

            get { return _PinAlignIntervalCnt; }
            set
            {
                if (value != _PinAlignIntervalCnt)
                {
                    _PinAlignIntervalCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TimeSpan _RTElapsedTimeTotal;
        public TimeSpan RTElapsedTimeTotal
        {

            get { return _RTElapsedTimeTotal; }
            set
            {
                if (value != _RTElapsedTimeTotal)
                {
                    _RTElapsedTimeTotal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TimeSpan _RTElapsedTimeLasted;
        public TimeSpan RTElapsedTimeLasted
        {

            get { return _RTElapsedTimeLasted; }
            set
            {
                if (value != _RTElapsedTimeLasted)
                {
                    _RTElapsedTimeLasted = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurrentCnt;
        public int CurrentCnt
        {

            get { return _CurrentCnt; }
            set
            {
                if (value != _CurrentCnt)
                {
                    _CurrentCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RepeatDelayTime = 1000;
        public int RepeatDelayTime
        {

            get { return _RepeatDelayTime; }
            set
            {
                if (value != _RepeatDelayTime)
                {
                    _RepeatDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CardID;
        public string CardID
        {

            get { return _CardID; }
            set
            {
                if (value != _CardID)
                {
                    _CardID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _SetCardID;
        public ICommand SetCardID
        {
            get
            {
                if (null == _SetCardID) _SetCardID = new AsyncCommand(SetCardIDFunc);
                return _SetCardID;
            }
        }

        private async Task SetCardIDFunc()
        {
            try
            {
                loaderModule.LoaderMaster.CardIDFullWord = CardID;
                LoggerManager.Debug($"LoaderHandlingViewModel.SetCardIDFunc() CardID is {CardID}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void SetTransferInfoToModule(object param, ref object Id, bool issource)
        {
            try
            {
                var loader = loaderModule;
                Map = loader.GetLoaderInfo().StateMap;

                if (param is StageObject)
                {

                    if (!isSourceCard)
                    {
                        //Move Wafer

                        int chuckindex = Convert.ToInt32(((param as StageObject).Name.Split('#'))[1]);
                        HolderModuleInfo chuckModule = null;
                        if (issource)
                            chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == chuckindex);
                        else
                            chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == chuckindex);

                        if (chuckModule == null)
                        {
                            Id = null;
                            return;
                        }
                        if (issource)
                            Id = chuckModule.Substrate.ID.Value;
                        else
                            Id = chuckModule.ID;
                    }
                    else
                    {
                        //Move Card
                        int stageindex = Convert.ToInt32(((param as StageObject).Name.Split('#'))[1]);
                        HolderModuleInfo chuckModule = null;
                        if (issource)
                            chuckModule = Map.CCModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == stageindex);
                        else
                            chuckModule = Map.CCModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == stageindex);

                        if (chuckModule == null)
                        {
                            Id = null;
                            return;
                        }
                        if (issource)
                            Id = chuckModule.Substrate.ID.Value;
                        else
                            Id = chuckModule.ID;
                    }
                }
                else if (param is SlotObject)
                {
                    int slotindex = Convert.ToInt32(((param as SlotObject).Name.Split('#'))[1]);
                    var waferIdx = (((param as SlotObject).FoupNumber) * 25) + slotindex;
                    if (Map.CassetteModules[(param as SlotObject).FoupNumber].FoupState == FoupStateEnum.LOAD && Map.CassetteModules[(param as SlotObject).FoupNumber].ScanState == CassetteScanStateEnum.READ)
                    {
                        var slotModule = Map.CassetteModules[(param as SlotObject).FoupNumber].SlotModules.FirstOrDefault(i => i.ID.Index == waferIdx);
                        if (slotModule == null)
                            return;
                        if (issource)
                            Id = slotModule.Substrate.ID.Value;
                        else
                            Id = slotModule.ID;
                    }
                    else
                    {
                        Id = null;
                        return;
                    }

                }
                else if (param is ArmObject)
                {
                    int armindex = Convert.ToInt32(((param as ArmObject).Name.Split('#'))[1]);
                    HolderModuleInfo armmodule = null;
                    if (issource)
                        armmodule = Map.ARMModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == armindex);
                    else
                        armmodule = Map.ARMModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == armindex);

                    if (armmodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource)
                        Id = armmodule.Substrate.ID.Value;
                    else
                        Id = armmodule.ID;
                }
                else if (param is PAObject)
                {
                    int paindex = Convert.ToInt32(((param as PAObject).Name.Split('#'))[1]);
                    HolderModuleInfo pamodule = null;
                    if (issource)
                        pamodule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == paindex);
                    else
                        pamodule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == paindex);

                    if (pamodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource)
                        Id = pamodule.Substrate.ID.Value;
                    else
                        Id = pamodule.ID;
                }
                else if (param is BufferObject)
                {
                    int bufferindex = Convert.ToInt32(((param as BufferObject).Name.Split('#'))[1]);
                    HolderModuleInfo buffermodule = null;
                    if (issource)
                        buffermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == bufferindex);
                    else
                        buffermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == bufferindex);

                    if (buffermodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource)
                        Id = buffermodule.Substrate.ID.Value;
                    else
                        Id = buffermodule.ID;
                }
                else if (param is CardTrayObject)
                {
                    int cardtaryindex = Convert.ToInt32(((param as CardTrayObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardtraymodule = null;
                    if (issource)
                        cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardtaryindex);
                    else
                    {
                        cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardtaryindex);
                        if (cardtraymodule == null)
                        {
                            cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.CARRIER && i.ID.Index == cardtaryindex);
                        }
                    }
                    if (cardtraymodule == null)
                    {
                        Id = null;
                        return;
                    }

                    if (issource)
                        Id = cardtraymodule.Substrate.ID.Value;
                    else
                        Id = cardtraymodule.ID;
                }
                else if (param is CardBufferObject)
                {
                    int cardbufindex = Convert.ToInt32(((param as CardBufferObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardtraymodule = null;
                    if (issource)
                        cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardbufindex);
                    else
                    {
                        cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardbufindex);
                        if (cardtraymodule == null)
                        {
                            cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.CARRIER && i.ID.Index == cardbufindex);
                        }
                    }

                    //if(cardtraymodule != null)
                    //{
                    //    var cardbuffermodule = this.LoaderModule.ModuleManager.FindModule<ICardBufferModule>(cardtraymodule.ID);
                    //    if (cardbuffermodule.CanDistinguishCard())
                    //    {
                    //        if (cardtraymodule.WaferStatus == EnumSubsStatus.EXIST)
                    //        {
                    //            if (cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_ATTACH &&
                    //                cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_DETACH)
                    //            {
                    //                cardtraymodule = null;
                    //                LoggerManager.Debug($"SetTransferInfoToModule(): waferstatus and cardpresencestate is not same. CardPRESENCEState:{cardbuffermodule.CardPRESENCEState}, WaferStus:{cardtraymodule.WaferStatus}");
                    //            }                                
                    //        }
                    //        else if (cardtraymodule.WaferStatus == EnumSubsStatus.CARRIER)
                    //        {
                    //            if (cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                    //            {
                    //                cardtraymodule = null;
                    //                LoggerManager.Debug($"SetTransferInfoToModule(): waferstatus and cardpresencestate is not same. CardPRESENCEState:{cardbuffermodule.CardPRESENCEState}, WaferStus:{cardtraymodule.WaferStatus}");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                    //            {
                    //                cardtraymodule = null;
                    //                LoggerManager.Debug($"SetTransferInfoToModule(): waferstatus and cardpresencestate is not same. CardPRESENCEState:{cardbuffermodule.CardPRESENCEState}, WaferStus:{cardtraymodule.WaferStatus}");
                    //            }
                    //        }
                    //    }
                       
                    //}
                   

                    if (cardtraymodule == null)
                        return;
                    if (issource)
                        Id = cardtraymodule.Substrate.ID.Value;
                    else
                        Id = cardtraymodule.ID;
                }
                else if (param is CardArmObject)
                {
                    int cardarmindex = Convert.ToInt32(((param as CardArmObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardarmmodule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource)
                        cardarmmodule = Map.CardArmModule.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardarmindex);
                    else
                        cardarmmodule = Map.CardArmModule.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardarmindex);

                    if (cardarmmodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource)
                        Id = cardarmmodule.Substrate.ID.Value;
                    else
                        Id = cardarmmodule.ID;
                }
                else if (param is FixedTrayInfoObject)
                {
                    int fixedtrayindex = Convert.ToInt32(((param as FixedTrayInfoObject).Name.Split('#'))[1]);
                    HolderModuleInfo fixedTrayModule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource)
                        fixedTrayModule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == fixedtrayindex);
                    else
                        fixedTrayModule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == fixedtrayindex);

                    if (fixedTrayModule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource)
                        Id = fixedTrayModule.Substrate.ID.Value;
                    else
                        Id = fixedTrayModule.ID;
                }
                else if (param is InspectionTrayInfoObject)
                {
                    int iNSPtrayindex = Convert.ToInt32(((param as InspectionTrayInfoObject).Name.Split('#'))[1]);
                    HolderModuleInfo iNSPTrayModule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource)
                        iNSPTrayModule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == iNSPtrayindex);
                    else
                        iNSPTrayModule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == iNSPtrayindex);

                    if (iNSPTrayModule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource)
                        Id = iNSPTrayModule.Substrate.ID.Value;
                    else
                        Id = iNSPTrayModule.ID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetTransfer(TransferObject subObj, ModuleInfoBase dstLoc, ref EventCodeEnum Dret)
        {
            Dret = EventCodeEnum.NONE;
            string id = subObj.ID.Value;
            ModuleID destinationID = (ModuleID)dstLoc.ID;
            bool stageBusy = true;
            if (subObj == null)
            {
                Map = null;
            }

            if (dstLoc == null)
            {
                Map = null;
            }

            if (subObj.CurrPos == destinationID)
            {
                Map = null;
            }

            if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CHUCK ||
                subObj.CurrPos.ModuleType == ModuleTypeEnum.CC)

            {
                stageBusy = LoaderMaster.GetClient(subObj.CurrPos.Index).GetRunState(true);
                if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CHUCK && stageBusy == false)
                {
                    bool needtorecovery = false;
                    ModuleStateEnum wafertransferstate = ModuleStateEnum.ABORT;
                    var ret = LoaderMaster.GetClient(subObj.CurrPos.Index).CanWaferUnloadRecovery(ref needtorecovery,
                        ref wafertransferstate);
                    if (ret == EventCodeEnum.NONE)
                    {
                        if (needtorecovery && wafertransferstate == ModuleStateEnum.ERROR)
                        {
                            stageBusy = true;
                        }
                    }
                }
            }
            else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK ||
                dstLoc.ID.ModuleType == ModuleTypeEnum.CC)
            {
                LoaderServiceBase.ILoaderServiceCallback callback = LoaderMaster.GetClient(dstLoc.ID.Index);

                if (callback != null)
                {
                    stageBusy = callback.GetRunState(true);
                }
                else
                {
                    LoggerManager.Debug($"[LoaderHandlingViewModel], SetTransfer() : COM ERROR");
                    stageBusy = false;
                }
            }

            if (stageBusy == false)
            {
                var retVal = (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this.loaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    return;
                }
            }

            var card = this.LoaderModule.ModuleManager.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();

            if (card != null)
            {
                if (SkipDocking)
                {
                    card.CardSkip = ProberInterfaces.Enum.CardSkipEnum.SKIP;
                }
                else
                {
                    card.CardSkip = ProberInterfaces.Enum.CardSkipEnum.NONE;
                }

            }

            if (dstLoc is HolderModuleInfo)
            {
                var currHolder = Map.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                var dstHolder = dstLoc as HolderModuleInfo;
                
                if (subObj.Size.Value == SubstrateSizeEnum.UNDEFINED || subObj.Size.Value == SubstrateSizeEnum.INVALID ||
                    (currHolder.ID.ModuleType == ModuleTypeEnum.PA && LoaderMaster.Loader.PAManager.PAModules[currHolder.ID.Index - 1].State.PAAlignAbort)) 
                {
                    subObj.PreAlignState = PreAlignStateEnum.SKIP;
                }

                if (!(subObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT || subObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || subObj.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY) && dstLoc.ID.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && IsTransferSorceWafersizeisundefined)
                {
                    subObj.PreAlignState = PreAlignStateEnum.SKIP;
                }

                subObj.PrevHolder = subObj.CurrHolder;
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrHolder = destinationID;
                subObj.CurrPos = destinationID;

                currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                currHolder.Substrate = null;

                if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                {
                    subObj.NotchAngle.Value = (this.loaderModule.ModuleManager.FindModule(dstLoc.ID) as ISlotModule).Cassette.Device.LoadingNotchAngle.Value;
                }

                currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                currHolder.Substrate = null;

                dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                dstHolder.Substrate = subObj;
                TransferObject currSubObj = loaderModule.ModuleManager.FindTransferObject(subObj.ID.Value);
                if (currSubObj != null)
                {
                    currSubObj.DstPos = dstHolder.ID;
                }
            }
            else
            {
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = destinationID;
            }
        }

        private void SetNotExistObjectDisable()
        {
            int cellidx = 0;
            foreach (var cell in Cells)
            {
                int idx = cellidx;
                if (this.loaderCommunicationManager.Cells[idx].LockMode == StageLockMode.LOCK)
                {
                    cell.IsEnableTransfer = false;
                }
                else if (!(this.loaderCommunicationManager.Cells[idx].StageMode == GPCellModeEnum.ONLINE) && (this.LoaderMaster.StageStates[idx] == ModuleStateEnum.IDLE || this.LoaderMaster.StageStates[idx] == ModuleStateEnum.PAUSED))
                {
                    if (cell.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST
                          | cell.CardStatus == EnumSubsStatus.EXIST)
                        cell.IsEnableTransfer = true;
                    else
                        cell.IsEnableTransfer = false;
                }
                else
                {
                    cell.IsEnableTransfer = false;
                }
                if ((this.loaderCommunicationManager.Cells[idx] as StageObject).ManualZUPState == ManualZUPStateEnum.Z_UP)
                {
                    cell.IsEnableTransfer = false;
                }
                cellidx++;
            }
            foreach (var foup in Foups)
            {

                foreach (var slot in foup.Slots)
                {
                    if (foup.ScanState == CassetteScanStateEnum.ILLEGAL)
                    {
                        slot.IsEnableTransfer = false;
                    }
                    else if (slot.WaferStatus != EnumSubsStatus.EXIST)
                        slot.IsEnableTransfer = false;
                }
            }
            foreach (var buffer in Buffers)
            {
                if (buffer.WaferStatus != EnumSubsStatus.EXIST)
                    buffer.IsEnableTransfer = false;
            }
            foreach (var arm in Arms)
            {
                if (arm.WaferStatus != EnumSubsStatus.EXIST)
                    arm.IsEnableTransfer = false;
            }
            foreach (var pa in PAs)
            {
                if (pa.WaferStatus != EnumSubsStatus.EXIST)
                    pa.IsEnableTransfer = false;
            }
            foreach (var cardbuf in CardBuffers)
            {
                var cardbuffermodule = this.loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, CardBuffers.IndexOf(cardbuf) + 1);
                if (cardbuf.WaferStatus != EnumSubsStatus.EXIST
                    && cardbuf.WaferStatus != EnumSubsStatus.CARRIER)
                {
                    cardbuf.IsEnableTransfer = false;
                }
                else if (cardbuffermodule.CardPRESENCEState == ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                {
                    cardbuf.IsEnableTransfer = false;
                }
                    
            }
            foreach (var cardtary in CardTrays)
            {
                if (cardtary.WaferStatus != EnumSubsStatus.EXIST)
                    cardtary.IsEnableTransfer = false;
            }
            foreach (var cardarm in CardArms)
            {
                if (cardarm.WaferStatus != EnumSubsStatus.EXIST && cardarm.WaferStatus != EnumSubsStatus.CARRIER)
                    cardarm.IsEnableTransfer = false;
            }
            foreach (var ft in FTs)
            {
                if (ft.WaferStatus != EnumSubsStatus.EXIST)
                    ft.IsEnableTransfer = false;
            }
            foreach (var it in ITs)
            {
                if (it.WaferStatus != EnumSubsStatus.EXIST)
                    it.IsEnableTransfer = false;
            }
        }
        private void SetExistObjectDisable()
        {
            try
            {
                foreach (var cell in Cells)
                {
                    int idx = cell.Index - 1;

                    if (this.loaderCommunicationManager.Cells[idx].LockMode == StageLockMode.LOCK)
                    {
                        cell.IsEnableTransfer = false;
                    }
                    else if (!(this.loaderCommunicationManager.Cells[idx].StageMode == GPCellModeEnum.ONLINE) && this.LoaderMaster.StageStates[idx] == ModuleStateEnum.IDLE || this.LoaderMaster.StageStates[idx] == ModuleStateEnum.PAUSED)
                    {
                        if (cell.WaferStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST || cell.CardStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST)
                        {
                            //Disable 함수에선 Disable 만 해야함.
                            //cell.IsEnableTransfer = true;
                        }
                        else
                        {
                            cell.IsEnableTransfer = false;
                        }
                    }
                    else
                    {
                        cell.IsEnableTransfer = false;
                    }

                    if ((this.loaderCommunicationManager.Cells[idx] as StageObject).ManualZUPState == ManualZUPStateEnum.Z_UP)
                    {
                        cell.IsEnableTransfer = false;
                    }

                    if (TransferSource is CardArmObject)
                    {
                        var CardArmObj = TransferSource as CardArmObject;

                        if (CardArmObj.WaferStatus == EnumSubsStatus.CARRIER)
                        {
                            cell.IsEnableTransfer = false;
                        }
                    }
                }

                foreach (var foup in Foups)
                {
                    bool disableFlag = false;

                    if (!(foup.ScanState == CassetteScanStateEnum.READ))
                    {
                        disableFlag = true;
                    }

                    foreach (var slot in foup.Slots)
                    {

                        if (disableFlag)
                        {
                            slot.IsEnableTransfer = false;
                        }
                        else
                        {
                            if (slot.WaferStatus != EnumSubsStatus.NOT_EXIST)
                            {
                                slot.IsEnableTransfer = false;
                            }
                        }
                    }
                }

                foreach (var buffer in Buffers)
                {
                    if (buffer.WaferStatus != EnumSubsStatus.NOT_EXIST)
                    {
                        buffer.IsEnableTransfer = false;
                    }
                }

                foreach (var arm in Arms)
                {
                    arm.IsEnableTransfer = false;
                }

                foreach (var pa in PAs)
                {
                    if (pa.WaferStatus != EnumSubsStatus.NOT_EXIST)
                    {
                        pa.IsEnableTransfer = false;
                    }
                }

                foreach (var cardbuf in CardBuffers)
                {
                    var cardbuffermodule = this.loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, CardBuffers.IndexOf(cardbuf) + 1);

                    if (cardbuf.WaferStatus != EnumSubsStatus.NOT_EXIST
                        && cardbuf.WaferStatus != EnumSubsStatus.CARRIER)
                    {
                        cardbuf.IsEnableTransfer = false;
                    }
                    else if (cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                    {
                        cardbuf.IsEnableTransfer = false;
                    }
                }

                foreach (var cardtary in CardTrays)
                {
                    if (cardtary.WaferStatus != EnumSubsStatus.NOT_EXIST && cardtary.WaferStatus != EnumSubsStatus.CARRIER)
                    {
                        cardtary.IsEnableTransfer = false;
                    }
                }

                foreach (var cardarm in CardArms)
                {
                    if (cardarm.WaferStatus != EnumSubsStatus.NOT_EXIST)
                    {
                        cardarm.IsEnableTransfer = false;
                    }
                }

                foreach (var ft in FTs)
                {
                    if (ft.WaferStatus != EnumSubsStatus.NOT_EXIST)
                    {
                        ft.IsEnableTransfer = false;
                    }
                }

                foreach (var it in ITs)
                {
                    if (it.WaferStatus != EnumSubsStatus.NOT_EXIST)
                    {
                        it.IsEnableTransfer = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetAllObjectEnable()
        {
            int cellidx = 0;
            foreach (var cell in Cells)
            {
                int idx = cellidx;
                if (this.loaderCommunicationManager.Cells[idx].LockMode == StageLockMode.LOCK)
                {
                    cell.IsEnableTransfer = false;
                }
                else if (!(this.loaderCommunicationManager.Cells[idx].StageMode == GPCellModeEnum.ONLINE) && (this.LoaderMaster.StageStates[idx] == ModuleStateEnum.IDLE || this.LoaderMaster.StageStates[idx] == ModuleStateEnum.PAUSED || this.LoaderMaster.StageStates[idx] == ModuleStateEnum.ERROR))
                {
                    // Cell이 IDLE, PAUSED, ERROR 상태 및 웨이퍼 Unknown 상태일 때 웨이퍼 오브젝트 상태를 Exist, Not_Exist 상태로 변경해주기 위해서는 Object가 Click 가능해야 한다.
                    // 다만 Error 상태인 셀을 source나 target으로 설정하여 Transfer는 불가능 해야 하고 이는 다른 함수에서 막혀 있다. SetNotExistObjectDisable(), SetExistObjectDisable()
                    cell.IsEnableTransfer = true;
                }
                else
                {
                    cell.IsEnableTransfer = false;
                }
                cellidx++;
            }
            foreach (var foup in Foups)
            {
                foreach (var slot in foup.Slots)
                {
                    slot.IsEnableTransfer = true;
                }
            }
            foreach (var buffer in Buffers)
            {
                if (buffer.Enable)
                {
                    buffer.IsEnableTransfer = true;
                }
                else
                {
                    buffer.IsEnableTransfer = false;
                }
            }
            foreach (var arm in Arms)
            {
                arm.IsEnableTransfer = true;
            }
            foreach (var pa in PAs)
            {
                if (pa.Enable)
                {
                    pa.IsEnableTransfer = true;
                }
                else
                {
                    pa.IsEnableTransfer = false;
                }
            }
            foreach (var cardbuf in CardBuffers)
            {
                if (cardbuf.Enable)
                {
                    cardbuf.IsEnableTransfer = true;
                }
                else
                {
                    cardbuf.IsEnableTransfer = false;
                }
            }
            foreach (var cardtary in CardTrays)
            {
                cardtary.IsEnableTransfer = true;
            }
            foreach (var cardarm in CardArms)
            {
                cardarm.IsEnableTransfer = true;
            }
            foreach (var ft in FTs)
            {
                ft.IsEnableTransfer = true;
            }
            foreach (var it in ITs)
            {
                it.IsEnableTransfer = true;
            }
        }
        private void SetAllObjectDisable()
        {
            int cellidx = 0;
            foreach (var cell in Cells)
            {
                cell.IsEnableTransfer = false;
            }
            foreach (var foup in Foups)
            {
                foreach (var slot in foup.Slots)
                {
                    slot.IsEnableTransfer = false;
                }
            }
            foreach (var buffer in Buffers)
            {
                buffer.IsEnableTransfer = false;
            }
            foreach (var arm in Arms)
            {
                arm.IsEnableTransfer = false;
            }
            foreach (var pa in PAs)
            {
                pa.IsEnableTransfer = false;
            }
            foreach (var cardbuf in CardBuffers)
            {
                cardbuf.IsEnableTransfer = false;
            }
            foreach (var cardtary in CardTrays)
            {
                cardtary.IsEnableTransfer = false;
            }
            foreach (var cardarm in CardArms)
            {
                cardarm.IsEnableTransfer = false;
            }
            foreach (var ft in FTs)
            {
                ft.IsEnableTransfer = false;
            }
            foreach (var it in ITs)
            {
                it.IsEnableTransfer = false;
            }
        }

        private void SetDisalbeDontMoveCard()
        {
            foreach (var foup in Foups)
            {
                foreach (var slot in foup.Slots)
                {
                    slot.IsEnableTransfer = false;
                }
            }
            foreach (var buffer in Buffers)
            {
                buffer.IsEnableTransfer = false;
            }
            foreach (var arm in Arms)
            {
                arm.IsEnableTransfer = false;
            }
            foreach (var pa in PAs)
            {
                pa.IsEnableTransfer = false;
            }
            foreach (var ft in FTs)
            {
                ft.IsEnableTransfer = false;
            }
            foreach (var it in ITs)
            {
                it.IsEnableTransfer = false;
            }

            foreach (var cell in Cells)
            {
                if (cell.CardStatus == EnumSubsStatus.EXIST)
                    cell.IsEnableTransfer = false;
            }

            foreach (var cardbuf in CardBuffers)
            {
                var cardbuffermodule = this.loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, CardBuffers.IndexOf(cardbuf) + 1);
                if (cardbuf.WaferStatus == EnumSubsStatus.EXIST)
                {
                    cardbuf.IsEnableTransfer = false;
                }
                else if (cardbuf.WaferStatus == EnumSubsStatus.CARRIER
                    && (TransferSource is CardBufferObject || TransferSource is CardTrayObject))
                {
                    cardbuf.IsEnableTransfer = false;
                }

                if (cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                {
                    cardbuf.IsEnableTransfer = false;
                }

                if (TransferSource is StageObject)
                {
                    cardbuf.IsEnableTransfer = IsEnableTransferCCtype(cardbuf.WaferStatus);
                }
            }
            foreach (var cardtary in CardTrays)
            {
                if (cardtary.WaferStatus == EnumSubsStatus.EXIST)
                {
                    cardtary.IsEnableTransfer = false;
                }
                else if (cardtary.WaferStatus == EnumSubsStatus.CARRIER &&
                  (TransferSource is CardBufferObject || TransferSource is CardTrayObject || TransferSource is CardArmObject))
                {
                    cardtary.IsEnableTransfer = false;
                }

                if (TransferSource is StageObject)
                {
                    cardtary.IsEnableTransfer = IsEnableTransferCCtype(cardtary.WaferStatus);
                }
            }
            foreach (var cardarm in CardArms)
            {
                if (cardarm.WaferStatus == EnumSubsStatus.EXIST)
                {
                    cardarm.IsEnableTransfer = false;
                }
                else if (cardarm.WaferStatus == EnumSubsStatus.CARRIER &&
                    (TransferSource is CardBufferObject || TransferSource is CardTrayObject || TransferSource is CardArmObject))
                {
                    cardarm.IsEnableTransfer = false;
                }

                if (TransferSource is StageObject)
                {
                    cardarm.IsEnableTransfer = IsEnableTransferCCtype(cardarm.WaferStatus);
                }
            }

        }
        private EventCodeEnum IsEnableTransferINSP(int index, out object transferTarget)
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            transferTarget = null;
            try
            {
                DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                TransferObject deviceInfo = null;
                var insps = DMParam.DeviceMappingInfos.Where(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY);
                WaferSupplyMappingInfo[] supplyInfo = insps.Where(i =>  i.DeviceInfo != null && i.DeviceInfo.PolishWaferInfo == null).ToArray();
                if (supplyInfo != null)
                {
                    for (int i = 0; i < supplyInfo.Count(); i++)
                    {
                        if (ITs[index - 1].WaferStatus == EnumSubsStatus.NOT_EXIST && ITs[index - 1].Index == supplyInfo[i].WaferSupplyInfo.ID.Index)
                        {
                            transferTarget = GetAttachObject(ModuleTypeEnum.INSPECTIONTRAY, index);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }

        private EventCodeEnum IsEnableTransferSlot(FoupObject foup, SlotObject slot)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            
            try
            {
                int slotindex = slot.Index;
                int foupindex = foup.Index;
                var loaderInfo = LoaderMaster.Loader.GetLoaderInfo();
                int slotCnt = this.LoaderModule.Foups[foupindex].Slots.Count();                
                var slotholder = loaderInfo.StateMap.CassetteModules[foupindex].SlotModules[slotCnt - slotindex];
                if (slotholder.WaferStatus != EnumSubsStatus.EXIST)
                {
                    bool waferexist = false;
                    var transferObjects = loaderInfo.StateMap.GetTransferObjectAll();
                    foreach (var w in transferObjects)
                    {
                        var condition1 = ((w.OriginHolder.Index - 1) / 25) == foupindex;
                        var condition2 = w.OriginHolder.Index % 25 == slotindex;
                        var condition3 = w.CurrHolder.ModuleType != ModuleTypeEnum.SLOT;
                        var condition4 = w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT;
                        var condition5 = w.CST_HashCode == loaderInfo.StateMap.CassetteModules[foupindex].CST_HashCode;

                        if (condition1 && condition2 && condition3 && condition4 && condition5)
                        {
                            waferexist = true;
                            break;
                        }
                    }

                    if (waferexist == false)
                    {
                        // 카세트 타입이 13slot && slot 짝수 => disable
                        if (slotindex % 2 == 0 && foup.CassetteType == CassetteTypeEnum.FOUP_13)
                        {
                            ret = EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            ret = EventCodeEnum.NONE;
                        }
                    }
                    else 
                    {
                        ret = EventCodeEnum.UNDEFINED;
                    }
                }


            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }

        private EventCodeEnum IsEnableTransferFixedTray(FixedTrayInfoObject ft)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;            
            try
            {                
                var loaderInfo = LoaderMaster.Loader.GetLoaderInfo();
                var transferObjects = loaderInfo.StateMap.GetTransferObjectAll();
                bool waferexist = false;                

                foreach (var w in transferObjects)
                {
                    var condition1 = w.CurrHolder.Index == ft.Index;
                    var condition2 = w.CurrHolder.ModuleType != ModuleTypeEnum.FIXEDTRAY;
                    var condition3 = w.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY;
                    if (condition1 && condition2 && condition3)
                    {
                        waferexist = true;
                        break;
                    }
                }        
                
                if(waferexist == false)
                {
                    DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                    WaferSupplyMappingInfo supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY && i.WaferSupplyInfo.ID.Index == ft.Index);                    
                    TransferObject deviceInfo = null; 

                    if (supplyInfo != null)
                    {
                        deviceInfo = supplyInfo.DeviceInfo;
                        if (deviceInfo.PolishWaferInfo == null)
                        {
                            // 해당 FIXEDTRAY 에 polish wafer 정보 assign 되어 있지 않다
                            ret = EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            // Polish Wafer 사용되는 Holder
                            if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                            {
                                // Polish Wafer로 설정은 되어 있음, 파라미터 유효성 검사                            
                                if (deviceInfo.PolishWaferInfo.DefineName == null || deviceInfo.PolishWaferInfo.DefineName.Value == string.Empty || deviceInfo.PolishWaferInfo.DefineName.Value == null ||
                                    deviceInfo.PolishWaferInfo.Size.Value == SubstrateSizeEnum.INVALID || deviceInfo.PolishWaferInfo.Size.Value == SubstrateSizeEnum.UNDEFINED ||
                                    deviceInfo.PolishWaferInfo.NotchType.Value == WaferNotchTypeEnum.UNKNOWN)
                                {
                                    ret = EventCodeEnum.UNDEFINED;
                                }
                                else
                                {
                                    ret = EventCodeEnum.NONE;
                                }
                            }
                            else
                            {
                                ret = EventCodeEnum.UNDEFINED;
                            }
                        }                        
                    }
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }

        private EventCodeEnum IsEnableTransferInspectionTrayNotAssignPW(InspectionTrayInfoObject it)
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {
                var loaderInfo = LoaderMaster.Loader.GetLoaderInfo();
                var transferObjects = loaderInfo.StateMap.GetTransferObjectAll();
                bool waferexist = false;

                foreach (var w in transferObjects)
                {
                    var condition1 = w.CurrHolder.Index == it.Index;
                    var condition2 = w.CurrHolder.ModuleType != ModuleTypeEnum.INSPECTIONTRAY;
                    var condition3 = w.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY;
                    var condition4 = w.OriginHolder.Index == it.Index;
                    if (condition1 && condition2 && condition3 && condition4)
                    {
                        waferexist = true;
                        break;
                    }
                }

                if (waferexist == false)
                {
                    DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                    WaferSupplyMappingInfo supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && i.WaferSupplyInfo.ID.Index == it.Index);
                    TransferObject deviceInfo = null;

                    if (supplyInfo != null)
                    {
                        deviceInfo = supplyInfo.DeviceInfo;
                        if (deviceInfo.PolishWaferInfo == null)
                        {
                            // 해당 INSPECTIONTRAY 에 polish wafer 정보 assign 되어 있지 않다
                            ret = EventCodeEnum.NONE;
                        }
                        else
                        {
                            // Polish Wafer 사용되는 Holder
                            if (supplyInfo.DeviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                            {
                                // Polish Wafer로 설정은 되어 있음, 파라미터 유효성 검사                            
                                if (supplyInfo.DeviceInfo.PolishWaferInfo.DefineName == null || supplyInfo.DeviceInfo.PolishWaferInfo.DefineName.Value == string.Empty || supplyInfo.DeviceInfo.PolishWaferInfo.DefineName.Value == null ||
                                    supplyInfo.DeviceInfo.PolishWaferInfo.Size.Value == SubstrateSizeEnum.INVALID || supplyInfo.DeviceInfo.PolishWaferInfo.Size.Value == SubstrateSizeEnum.UNDEFINED ||
                                    supplyInfo.DeviceInfo.PolishWaferInfo.NotchType.Value == WaferNotchTypeEnum.UNKNOWN)
                                {
                                    ret = EventCodeEnum.NONE;
                                }
                                else
                                {
                                    ret = EventCodeEnum.UNDEFINED;
                                }
                            }
                            else
                            {
                                ret = EventCodeEnum.NONE;
                            }
                        }                        
                    }                    
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }
        
        private bool IsEnableTransferCCtype(EnumSubsStatus ModuleStatus)
        {
            bool IsEnableTransfer = true;
            try
            {
                var stage = TransferSource as StageObject;
                int chuckindex = Convert.ToInt32(((stage as StageObject).Name.Split('#'))[1]);
                var cell = loaderModule.LoaderMaster.GetClient(chuckindex);
                if (cell != null)
                {
                    switch (cell.GetCardChangeType())
                    {
                        case ProberInterfaces.CardChange.EnumCardChangeType.NONE:
                            break;
                        case ProberInterfaces.CardChange.EnumCardChangeType.DIRECT_CARD:
                            break;
                        case ProberInterfaces.CardChange.EnumCardChangeType.CARRIER:
                            if (ModuleStatus != EnumSubsStatus.CARRIER)
                            {
                                IsEnableTransfer = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    LoggerManager.Debug($"IsEnableTransferCCtype() Stage [{chuckindex}] is disconnected");
                    IsEnableTransfer = false;
                }
            }
            catch (Exception err)
            {
                IsEnableTransfer = false;
                LoggerManager.Exception(err);
            }
            return IsEnableTransfer;
        }
        private void SetDisableDontMoveWafer()
        {
            foreach (var cardbuf in CardBuffers)
            {
                cardbuf.IsEnableTransfer = false;
            }
            foreach (var cardtary in CardTrays)
            {
                cardtary.IsEnableTransfer = false;
            }
            foreach (var cardarm in CardArms)
            {
                cardarm.IsEnableTransfer = false;
            }
        }

        private bool IsSourceCardObject(object obj)
        {
            bool retVal = false;
            try
            {
                if (obj is CardArmObject | obj is CardBufferObject | obj is CardTrayObject)
                    retVal = true;
                else
                {
                    if (obj is StageObject)
                    {

                        if (TransferSource != null && (TransferSource is CardArmObject | TransferSource is CardBufferObject | TransferSource is CardTrayObject))
                        {
                            retVal = true;
                        }
                        if ((obj as StageObject).CardStatus == EnumSubsStatus.EXIST)
                        {
                            retVal = true;
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
        private bool IsTargetCardObject(object obj)
        {
            bool retVal = false;
            try
            {
                if (obj is CardArmObject | obj is CardBufferObject | obj is CardTrayObject)
                    retVal = true;
                else
                {
                    if (obj is StageObject)
                    {
                        if (TransferSource != null && (TransferSource is CardArmObject | TransferSource is CardBufferObject | TransferSource is CardTrayObject))
                        {
                            retVal = true;
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

        private void InitCell()
        {
            try
            {
                var loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();
                foreach (var client in loaderSupervisor.ClientList)
                {
                    //client.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        private RelayCommand<object> _ObjectClickCommand;
        public ICommand ObjectClickCommand
        {
            get
            {
                if (null == _ObjectClickCommand) _ObjectClickCommand = new RelayCommand<object>(ObjectClickCommandFunc);
                return _ObjectClickCommand;
            }
        }

        private void ObjectClickCommandFunc(object param)
        {
            try
            {
                object[] _param = param as object[];
                // 0 : ListView
                // 1 : SelectedIndex
                // 2 : SelectedItem
                ListView listView = null;
                int index = -1;
                ListViewItem listViewItem = null;
                ListView foupListView = null;


                listView = (ListView)_param[0];
                index = (int)_param[1];
                if (index == -1)
                    return;
                listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(index);
                object item = _param[2];
                listViewItem.IsSelected = false;
                if (isClickedTransferSource && (!(item is StageObject) || (item is StageObject && Cells[index].IsEnableTransfer)))
                {
                    IsTransferSorceWafersizeisundefined = false;
                    IsTransferSorceUseOthertypesofcassettes = false;
                    Action<ModuleTypeEnum, int> CheckNSetTarget = (ModuleTypeEnum moduleType, int idx) =>
                    {
                        if ((moduleType == ModuleTypeEnum.SLOT ||
                            moduleType == ModuleTypeEnum.FIXEDTRAY ||
                            moduleType == ModuleTypeEnum.INSPECTIONTRAY) && idx != -1)
                        {
                            TransferTarget = GetAttachObject(moduleType, idx);
                            isSourceCard = false;
                        }
                    };

                    SourceObj = item;
                    TransferObject transferObject = null;
                    if (item is StageObject)
                    {
                        var stage = item as StageObject;
                        if (stage.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            //OriginHolder.ModuleType 이 자기 자신이라고 
                            transferObject = stage.WaferObj;
                            if (stage.WaferObj.Size.Value == SubstrateSizeEnum.UNDEFINED ||
                                !(stage.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT || stage.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || stage.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY))
                            {
                                IsTransferSorceWafersizeisundefined = true;
                            }
                            else
                            {
                                CheckNSetTarget(stage.WaferObj.OriginHolder.ModuleType, stage.WaferObj.OriginHolder.Index);
                            }
                        }
                        //else if (stage.CardStatus == EnumSubsStatus.EXIST)
                        //{
                        //    TransferTarget = GetAttachObject(stage.CardObj.OriginHolder.ModuleType, stage.CardObj.OriginHolder.Index);
                        //}
                    }
                    else if (item is ArmObject)
                    {
                        var arm = item as ArmObject;
                        if (arm.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            transferObject = arm.WaferObj;
                            if (arm.WaferObj.Size.Value == SubstrateSizeEnum.UNDEFINED ||
                                !(arm.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT || arm.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || arm.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY))
                            {
                                IsTransferSorceWafersizeisundefined = true;
                            }
                            else
                            {
                                CheckNSetTarget(arm.WaferObj.OriginHolder.ModuleType, arm.WaferObj.OriginHolder.Index);
                            }
                        }
                    }
                    else if (item is BufferObject)
                    {
                        var buffer = item as BufferObject;
                        if (buffer.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            transferObject = buffer.WaferObj;
                            if (buffer.WaferObj.Size.Value == SubstrateSizeEnum.UNDEFINED ||
                                !(buffer.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT || buffer.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || buffer.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY))
                            {
                                IsTransferSorceWafersizeisundefined = true;
                            }
                            else
                            {
                                CheckNSetTarget(buffer.WaferObj.OriginHolder.ModuleType, buffer.WaferObj.OriginHolder.Index);
                            }
                        }
                    }
                    else if (item is FixedTrayInfoObject)
                    {
                        var fixedTray = item as FixedTrayInfoObject;
                        if (fixedTray.WaferStatus == EnumSubsStatus.EXIST && fixedTray.CanUseBuffer == true)
                        {
                            transferObject = fixedTray.WaferObj;
                            if (fixedTray.WaferObj.Size.Value == SubstrateSizeEnum.UNDEFINED)
                            {
                                IsTransferSorceWafersizeisundefined = true;
                            }
                            else
                            {
                                CheckNSetTarget(fixedTray.WaferObj.OriginHolder.ModuleType, fixedTray.WaferObj.OriginHolder.Index);
                            }
                        }
                    }
                    else if (item is PAObject)
                    {
                        var pa = item as PAObject;
                        if (pa.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            transferObject = pa.WaferObj;
                            if (pa.WaferObj.Size.Value == SubstrateSizeEnum.UNDEFINED || LoaderMaster.Loader.PAManager.PAModules[pa.Index - 1].State.PAAlignAbort ||
                                !(pa.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT || pa.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || pa.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY))
                            {
                                IsTransferSorceWafersizeisundefined = true;
                            }
                            else
                            {
                                CheckNSetTarget(pa.WaferObj.OriginHolder.ModuleType, pa.WaferObj.OriginHolder.Index);
                            }
                        }
                    }
                    else if (item is InspectionTrayInfoObject)
                    {
                        var insp = item as InspectionTrayInfoObject;
                        if (insp.WaferStatus == EnumSubsStatus.EXIST && insp.WaferObj.WaferType.Value == EnumWaferType.STANDARD)
                        {
                            transferObject = insp.WaferObj;
                        }
                    }
                    else if (item is InspectionTrayObject)
                    {
                        var insp = item as InspectionTrayObject;
                        if (insp.WaferStatus == EnumSubsStatus.EXIST && insp.WaferObj.WaferType.Value == EnumWaferType.STANDARD)
                        {
                            transferObject = insp.WaferObj;
                        }
                    }

                    for (int i = 0; i < foupCount; i++)
                    {
                        //condition1
                        CassetteTypeEnum CassetteType = LoaderMaster.Loader.GetCassetteType(i + 1);
                        if (CassetteType != CassetteTypeEnum.FOUP_25)
                        {
                            IsTransferSorceUseOthertypesofcassettes = true;
                            break;
                        }
                    }

                    if (IsTransferSorceWafersizeisundefined)
                    {
                        object transfertaget = null;
                        foreach (var it in ITs)
                        {
                            IsEnableTransferINSP(it.Index, out transfertaget);
                            if (transfertaget != null)
                            {
                                break;
                            }
                        }

                        if (transfertaget != null)
                        {
                            (this).MetroDialogManager().ShowMessageDialog("Notify", "Untraceable wafer was detected.\nAvailable destination: Inspection tray.", EnumMessageStyle.Affirmative);
                            TransferTarget = transfertaget;
                        }
                        else 
                        {
                            (this).MetroDialogManager().ShowMessageDialog("Error", "Unable to use inspection tray. Please empty the inspection tray before continue.", EnumMessageStyle.Affirmative);
                            targetToggle = true;
                            item = null;
                        }
                    }
                    else if (IsTransferSorceUseOthertypesofcassettes)
                    {
                        if (transferObject != null && transferObject.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY) 
                        {
                            (this).MetroDialogManager().ShowMessageDialog("Notify", "Mixed Cassette Mode.\n" +
                                                                                "Available destination: Cassette, Fixed tray.", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            (this).MetroDialogManager().ShowMessageDialog("Notify", "Mixed Cassette Mode.\n" +
                                                                                "Please double-check the wafer type before processing.", EnumMessageStyle.Affirmative);                                                        
                        }                        
                    }

                    TransferSource = item;
                    isClickedTransferTarget = false;
                    isClickedTransferSource = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    sourceToggle = true;
                }
                else if (isClickedTransferTarget && (!(item is StageObject) || (item is StageObject && Cells[index].IsEnableTransfer)))
                {
                    TargetObj = item;
                    bool isSlot = false;
                    int FoupIdx = 0;
                    if (TransferSource is SlotObject)
                    {
                        var slot = TransferSource as SlotObject;
                        if (slot.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (slot.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = slot.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((slot.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }
                        }

                    }
                    else if (TransferSource is ArmObject)
                    {
                        var arm = TransferSource as ArmObject;
                        if (arm.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (arm.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = arm.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((arm.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }
                        }
                    }
                    else if (TransferSource is PAObject)
                    {
                        var PAObj = TransferSource as PAObject;
                        if (PAObj.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (PAObj.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = PAObj.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((PAObj.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }
                        }
                    }
                    else if (TransferSource is BufferObject)
                    {
                        var BufferObj = TransferSource as BufferObject;
                        if (BufferObj.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (BufferObj.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = BufferObj.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((BufferObj.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }
                        }
                    }
                    else if (TransferSource is StageObject)
                    {
                        var Chuck = TransferSource as StageObject;
                        if (Chuck.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            if (Chuck.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = Chuck.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((Chuck.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }
                        }
                    }
                    else if (TransferSource is FixedTrayInfoObject)
                    {
                        var FixedTrayObj = TransferSource as FixedTrayInfoObject;
                        if (FixedTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            if (FixedTrayObj.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = FixedTrayObj.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((FixedTrayObj.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }


                        }
                    }
                    else if (TransferSource is InspectionTrayObject)
                    {
                        var INSPTrayObj = TransferSource as InspectionTrayObject;
                        if (INSPTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            if (INSPTrayObj.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = INSPTrayObj.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((INSPTrayObj.WaferObj.OriginHolder.Index + offset) / 25) + 1;

                            }
                        }
                    }
                    else if (TransferSource is InspectionTrayInfoObject)
                    {
                        var INSPObj = TransferSource as InspectionTrayInfoObject;
                        if (INSPObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            if (INSPObj.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                isSlot = true;
                                int slotNum = INSPObj.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((INSPObj.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }
                        }
                    }

                    TransferTarget = item;
                    isClickedTransferTarget = false;
                    isClickedTransferSource = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    targetToggle = true;


                    if (IsTargetCardObject(TransferTarget))
                    {
                        isSourceCard = true;
                    }
                    else
                    {
                        isSourceCard = false;
                    }
                }
                else
                {
                    if (ModuleInfoEnable && !isClickedTransferSource && !isClickedTransferTarget)
                    {
                        if (item is SlotObject)
                        {
                            var slot = item as SlotObject;

                            if (slot.WaferStatus == EnumSubsStatus.EXIST || slot.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.SLOT, item, this.LoaderMaster);
                            }

                        }
                        else if (item is ArmObject)
                        {
                            var arm = item as ArmObject;
                            if (arm.WaferStatus == EnumSubsStatus.EXIST || arm.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.ARM, item, this.LoaderMaster);
                            }
                        }
                        else if (item is PAObject)
                        {
                            var PAObj = item as PAObject;
                            if (PAObj.WaferStatus == EnumSubsStatus.EXIST || PAObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.PA, item, this.LoaderMaster);
                            }
                        }
                        else if (item is BufferObject)
                        {
                            var BufferObj = item as BufferObject;
                            if (BufferObj.WaferStatus == EnumSubsStatus.EXIST || BufferObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.BUFFER, item, this.LoaderMaster);
                            }
                        }
                        else if (item is StageObject)
                        {
                            var Chuck = item as StageObject;
                            if (Chuck.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || Chuck.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CHUCK, item, this.LoaderMaster);
                            }
                            else if (Chuck.CardStatus == ProberInterfaces.EnumSubsStatus.EXIST || Chuck.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CHUCK, item, this.LoaderMaster);
                            }
                        }
                        else if (item is FixedTrayInfoObject)
                        {
                            var FixedTrayObj = item as FixedTrayInfoObject;
                            if (FixedTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || FixedTrayObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.FIXEDTRAY, item, this.LoaderMaster);
                            }
                        }
                        else if (item is InspectionTrayObject)
                        {
                            var INSPTrayObj = item as InspectionTrayObject;
                            if (INSPTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || INSPTrayObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.INSPECTIONTRAY, item, this.LoaderMaster);
                            }
                        }
                        else if (item is CardTrayObject)
                        {
                            var CardTrayObj = item as CardTrayObject;
                            if (CardTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || CardTrayObj.WaferStatus == EnumSubsStatus.UNKNOWN || CardTrayObj.WaferStatus == EnumSubsStatus.CARRIER)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CARDTRAY, item, this.LoaderMaster);
                            }
                        }
                        else if (item is CardBufferObject)
                        {
                            var CardBufferObj = item as CardBufferObject;
                            if (CardBufferObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || CardBufferObj.WaferStatus == EnumSubsStatus.UNKNOWN || CardBufferObj.WaferStatus == EnumSubsStatus.CARRIER)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CARDBUFFER, item, this.LoaderMaster);
                            }
                        }
                        else if (item is CardArmObject)
                        {
                            var CardArmObj = item as CardArmObject;
                            if (CardArmObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || CardArmObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CARDARM, item, this.LoaderMaster);
                            }
                        }
                        else if (item is InspectionTrayInfoObject)
                        {
                            var INSPObj = item as InspectionTrayInfoObject;
                            if (INSPObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST || INSPObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.INSPECTIONTRAY, item, this.LoaderMaster);
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

        private RelayCommand<object> _mouseUpCommand;
        public ICommand MouseUpCommand
        {
            get
            {
                if (_mouseUpCommand == null) return _mouseUpCommand = new RelayCommand<object>(ExecuteMouseUp);
                return _mouseUpCommand;
            }
        }

        public void ExecuteMouseUp(object e)
        {
            //if(SystemManager.SysExcuteMode)
            //ModuleInfoEnable = false;
            try
            {

                MouseEventArgs mouseparam = e as MouseEventArgs;

                //LastMouseRightUpPos = mouseparam.GetPosition((IInputElement)mouseparam.Source);

                FrameworkElement element = (FrameworkElement)mouseparam.OriginalSource;

                ListView testlist = (ListView)mouseparam.Source;

                ListViewItem lvi = (ListViewItem)testlist.ItemContainerGenerator.ContainerFromItem(element.DataContext);

                if (lvi != null)
                {
                    TemplateTransferObjectVM TOVM = Application.Current.Resources["TransferObjectVM"] as TemplateTransferObjectVM;

                    if (TOVM.EmulChecked == true)
                    {
                        // Slot
                        //if(TOVM.OriginModuleType==)
                        if (lvi.DataContext is SlotObject)
                        {
                            var slotobj = lvi.DataContext as SlotObject;

                            int slotindex = Convert.ToInt32(((slotobj as SlotObject).Name.Split('#'))[1]);
                            var waferIdx = (((slotobj as SlotObject).FoupNumber) * 25) + slotindex;

                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.SLOT, waferIdx);
                            IWaferOwnable ownable = attachedmodule as IWaferOwnable;
                            if (this.Foups[(slotobj as SlotObject).FoupNumber].ScanState == CassetteScanStateEnum.READ)
                            {
                                if (slotobj.WaferObj == null)
                                {
                                    if (TOVM != null && attachedmodule != null)
                                    {
                                        // Make clone data(TransferObject) from Template
                                        TransferObject Template = new TransferObject();
                                        Template.Copy(TOVM.TransferObj);
                                        if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                        {
                                            Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, waferIdx, "");

                                        }
                                        else
                                        {
                                            if (TOVM.OriginModuleType == ModuleTypeEnum.SLOT || TOVM.OriginModuleType == ModuleTypeEnum.CST)
                                            {
                                                int idx = (TOVM.FoupIndex * 25) + TOVM.OriginIndex;
                                                Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, idx, "");

                                            }
                                            else
                                            {
                                                Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                            }
                                        }
                                        Template.CurrPos = Template.OriginHolder;
                                        Template.CurrHolder = Template.OriginHolder;

                                        Template.Type.Value = SubstrateTypeEnum.Wafer;
                                        Template.Size.Value = loaderModule.GetDefaultWaferSize();
                                        Template.NotchType = WaferNotchTypeEnum.NOTCH;
                                        if (Template.Size.Value == SubstrateSizeEnum.INCH6)
                                        {
                                            Template.NotchType = WaferNotchTypeEnum.FLAT;
                                        }


                                        Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "SlotObject"));

                                        // Allocate
                                        ownable.Holder.SetLoad(Template);
                                        slotobj.WaferObj = Template;
                                        int slotCnt = this.LoaderModule.Foups[slotobj.FoupNumber].Slots.Count();
                                        this.LoaderModule.Foups[slotobj.FoupNumber].Slots[slotCnt - slotobj.Index].WaferObj = Template;
                                        this.LoaderModule.Foups[slotobj.FoupNumber].Slots[slotCnt - slotobj.Index].WaferStatus = EnumSubsStatus.EXIST;
                                        slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                    }
                                }
                                else
                                {
                                    ownable.Holder.SetUnload();
                                    slotobj.WaferObj = null;
                                    slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                                    int slotCnt = this.LoaderModule.Foups[slotobj.FoupNumber].Slots.Count();
                                    this.LoaderModule.Foups[slotobj.FoupNumber].Slots[slotCnt - slotobj.Index].WaferObj = null;
                                    this.LoaderModule.Foups[slotobj.FoupNumber].Slots[slotCnt - slotobj.Index].WaferStatus = EnumSubsStatus.NOT_EXIST;
                                }
                            }
                            else
                            {
                                // var retVal = (this).MetroDialogManager().ShowMessageDialog("Error", "Scan State is not Read State.", EnumMessageStyle.Affirmative).Result;
                            }
                        }
                        else if (lvi.DataContext is InspectionTrayInfoObject)
                        {
                            var slotobj = lvi.DataContext as InspectionTrayInfoObject;

                            int iNSPtrayindex = Convert.ToInt32((slotobj.Name.Split('#'))[1]);
                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.INSPECTIONTRAY, iNSPtrayindex);

                            IWaferOwnable ownable = attachedmodule as IWaferOwnable;

                            if (slotobj.WaferObj == null)
                            {
                                if (TOVM != null && attachedmodule != null)
                                {
                                    // Make clone data(TransferObject) from Template
                                    TransferObject Template = new TransferObject();
                                    Template.Copy(TOVM.TransferObj);

                                    if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                    {
                                        Template.OriginHolder = new ModuleID(ModuleTypeEnum.INSPECTIONTRAY, iNSPtrayindex, "");


                                    }
                                    else
                                    {
                                        if (TOVM.OriginModuleType == ModuleTypeEnum.SLOT || TOVM.OriginModuleType == ModuleTypeEnum.CST)
                                        {
                                            int idx = (TOVM.FoupIndex * 25) + TOVM.OriginIndex;
                                            Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, idx, "");

                                        }
                                        else
                                        {
                                            Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                        }
                                    }
                                    Template.CurrPos = Template.OriginHolder;
                                    Template.CurrHolder = Template.OriginHolder;

                                    Template.Type.Value = SubstrateTypeEnum.Wafer;
                                    Template.Size.Value = loaderModule.GetDefaultWaferSize();
                                    Template.NotchType = WaferNotchTypeEnum.NOTCH;
                                    if (Template.Size.Value == SubstrateSizeEnum.INCH6)
                                    {
                                        Template.NotchType = WaferNotchTypeEnum.FLAT;
                                    }

                                    Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "InspectionTrayInfoObject"));

                                    //to.ID.Value = Guid.NewGuid().ToString("N");
                                    //to.OriginHolder = holderID;
                                    //to.CurrPos = holderID;
                                    //to.CurrHolder = holderID;
                                    //to.Type.Value = SubstrateTypeEnum.UNDEFINED;
                                    //to.Size.Value = SubstrateSizeEnum.UNDEFINED;
                                    //to.WaferType.Value = EnumWaferType.UNDEFINED;
                                    //to.PreAlignState = PreAlignStateEnum.NONE;
                                    //to.OCRReadState = OCRReadStateEnum.NONE;
                                    //to.WaferState = EnumWaferState.UNPROCESSED;

                                    DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                    TransferObject deviceInfo = null;
                                    WaferSupplyMappingInfo supplyInfo = null;

                                    supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && i.WaferSupplyInfo.ID.Index == iNSPtrayindex);

                                    if (supplyInfo != null)
                                    {
                                        deviceInfo = supplyInfo.DeviceInfo;
                                    }

                                    // Polish Wafer 사용되는 Holder
                                    if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                                    {
                                        Template.PolishWaferInfo.DefineName.Value = deviceInfo.PolishWaferInfo.DefineName.Value;
                                        Template.PolishWaferInfo.Size.Value = deviceInfo.PolishWaferInfo.Size.Value;
                                        Template.PolishWaferInfo.NotchType.Value = deviceInfo.PolishWaferInfo.NotchType.Value;
                                        
                                        Template.PolishWaferInfo.CurrentAngle.Value = deviceInfo.PolishWaferInfo.CurrentAngle.Value;
                                        Template.PolishWaferInfo.NotchAngle.Value = deviceInfo.PolishWaferInfo.NotchAngle.Value;
                                        Template.PolishWaferInfo.RotateAngle.Value = deviceInfo.PolishWaferInfo.RotateAngle.Value;
                                        Template.PolishWaferInfo.Priorty.Value = deviceInfo.PolishWaferInfo.Priorty.Value;

                                        Template.PolishWaferInfo.Margin.Value = deviceInfo.PolishWaferInfo.Margin.Value;
                                        Template.PolishWaferInfo.Thickness.Value = deviceInfo.PolishWaferInfo.Thickness.Value;
                                    }

                                    // Allocate
                                    ownable.Holder.SetLoad(Template);
                                    slotobj.WaferObj = Template;
                                    slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                }
                            }
                            else
                            {
                                ownable.Holder.SetUnload();
                                slotobj.WaferObj = null;
                                slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }
                        else if (lvi.DataContext is FixedTrayInfoObject)
                        {
                            var slotobj = lvi.DataContext as FixedTrayInfoObject;

                            int fixedtrayindex = Convert.ToInt32((slotobj.Name.Split('#'))[1]);

                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.FIXEDTRAY, fixedtrayindex);
                            IWaferOwnable ownable = attachedmodule as IWaferOwnable;

                            if (slotobj.WaferObj == null)
                            {
                                if (TOVM != null && attachedmodule != null)
                                {
                                    // Make clone data(TransferObject) from Template
                                    //TransferObject Template = TOVM.TransferObj.Clone() as TransferObject;
                                    TransferObject Template = new TransferObject();
                                    Template.Copy(TOVM.TransferObj);


                                    if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                    {
                                        Template.OriginHolder = new ModuleID(ModuleTypeEnum.FIXEDTRAY, fixedtrayindex, "");


                                    }
                                    else
                                    {
                                        if (TOVM.OriginModuleType == ModuleTypeEnum.SLOT || TOVM.OriginModuleType == ModuleTypeEnum.CST)
                                        {
                                            int idx = (TOVM.FoupIndex * 25) + TOVM.OriginIndex;
                                            Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, idx, "");

                                        }
                                        else
                                        {
                                            Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                        }
                                    }
                                    Template.CurrPos = Template.OriginHolder;
                                    Template.CurrHolder = Template.OriginHolder;

                                    Template.Type.Value = SubstrateTypeEnum.Wafer;
                                    Template.Size.Value = loaderModule.GetDefaultWaferSize();
                                    Template.NotchType = WaferNotchTypeEnum.NOTCH;
                                    if (Template.Size.Value == SubstrateSizeEnum.INCH6)
                                    {
                                        Template.NotchType = WaferNotchTypeEnum.FLAT;
                                    }

                                    Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "FixedTrayInfoObject"));

                                    DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                    TransferObject deviceInfo = null;
                                    WaferSupplyMappingInfo supplyInfo = null;
                                    if (DMParam != null)
                                    {
                                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY && i.WaferSupplyInfo.ID.Index == fixedtrayindex);

                                        if (supplyInfo != null)
                                        {
                                            deviceInfo = supplyInfo.DeviceInfo;
                                        }
                                    }
                                    // Polish Wafer 사용되는 Holder
                                    if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                                    {
                                        Template.PolishWaferInfo.DefineName.Value = deviceInfo.PolishWaferInfo.DefineName.Value;
                                        Template.PolishWaferInfo.Size.Value = deviceInfo.PolishWaferInfo.Size.Value;
                                        Template.PolishWaferInfo.NotchType.Value = deviceInfo.PolishWaferInfo.NotchType.Value;

                                        Template.PolishWaferInfo.CurrentAngle.Value = deviceInfo.PolishWaferInfo.CurrentAngle.Value;
                                        Template.PolishWaferInfo.NotchAngle.Value = deviceInfo.PolishWaferInfo.NotchAngle.Value;
                                        Template.PolishWaferInfo.RotateAngle.Value = deviceInfo.PolishWaferInfo.RotateAngle.Value;
                                        Template.PolishWaferInfo.Priorty.Value = deviceInfo.PolishWaferInfo.Priorty.Value;

                                        Template.PolishWaferInfo.Margin.Value = deviceInfo.PolishWaferInfo.Margin.Value;
                                        Template.PolishWaferInfo.Thickness.Value = deviceInfo.PolishWaferInfo.Thickness.Value;
                                    }

                                    // Allocate
                                    ownable.Holder.SetLoad(Template);
                                    slotobj.WaferObj = Template;
                                    slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                    if (TOVM.SelectedSubsStatus == EnumSubsStatus.UNKNOWN)
                                    {
                                        ownable.Holder.SetUnknown();
                                        slotobj.WaferStatus = EnumSubsStatus.UNKNOWN;
                                    }
                                }
                            }
                            else
                            {
                                ownable.Holder.SetUnload();
                                slotobj.WaferObj = null;
                                slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }
                        else if (lvi.DataContext is CardBufferObject)
                        {
                            var slotobj = lvi.DataContext as CardBufferObject;

                            int cardbufindex = Convert.ToInt32(((slotobj as CardBufferObject).Name.Split('#'))[1]);

                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CARDBUFFER, cardbufindex);
                            ICardOwnable ownable = attachedmodule as ICardOwnable;

                            if (slotobj.WaferStatus == EnumSubsStatus.NOT_EXIST)
                            {
                                if (TOVM != null && attachedmodule != null)
                                {
                                    TransferObject Template = new TransferObject();
                                    Template.Copy(TOVM.TransferObj);

                                    if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                    {
                                        Template.OriginHolder = new ModuleID(ModuleTypeEnum.CARDBUFFER, cardbufindex, "");
                                    }
                                    else
                                    {
                                        Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                    }
                                    Template.CurrPos = Template.OriginHolder;
                                    Template.CurrHolder = Template.OriginHolder;

                                    Template.Type.Value = SubstrateTypeEnum.Card;
                                    //Template.Size.Value = SubstrateSizeEnum.INCH12;

                                    Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "CardBufferObject"));

                                    // Allocate

                                    if (TOVM.SelectedSubsStatus == EnumSubsStatus.CARRIER)
                                    {
                                        ownable.Holder.SetLoad(Template);
                                        ownable.Holder.SetAllocateCarrier();
                                    }
                                    else
                                    {
                                        ownable.Holder.SetLoad(Template);
                                        slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                    }
                                }
                            }
                            else
                            {
                                ownable.Holder.SetUnload();
                                slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }
                        else if (lvi.DataContext is CardTrayObject)
                        {
                            var slotobj = lvi.DataContext as CardTrayObject;

                            int cardbufindex = Convert.ToInt32(((slotobj as CardTrayObject).Name.Split('#'))[1]);

                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CARDTRAY, cardbufindex);
                            ICardOwnable ownable = attachedmodule as ICardOwnable;

                            if (slotobj.WaferStatus == EnumSubsStatus.NOT_EXIST)
                            {
                                if (TOVM != null && attachedmodule != null)
                                {
                                    TransferObject Template = new TransferObject();
                                    Template.Copy(TOVM.TransferObj);

                                    if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                    {
                                        Template.OriginHolder = new ModuleID(ModuleTypeEnum.CARDBUFFER, cardbufindex, "");


                                    }
                                    else
                                    {
                                        Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");

                                    }
                                    Template.CurrPos = Template.OriginHolder;
                                    Template.CurrHolder = Template.OriginHolder;

                                    Template.Type.Value = SubstrateTypeEnum.Card;
                                    //Template.Size.Value = SubstrateSizeEnum.INCH12;

                                    Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "CardTrayObject"));

                                    //DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                    //TransferObject deviceInfo = null;
                                    //WaferSupplyMappingInfo supplyInfo = null;

                                    //supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.CARDTRAY && i.WaferSupplyInfo.ID.Index == cardbufindex);

                                    //if (supplyInfo != null)
                                    //{
                                    //    deviceInfo = supplyInfo.DeviceInfo;
                                    //}

                                    // Allocate
                                    if (TOVM.SelectedSubsStatus == EnumSubsStatus.CARRIER)
                                    {
                                        ownable.Holder.SetLoad(Template);
                                        ownable.Holder.SetAllocateCarrier();
                                    }
                                    else
                                    {
                                        ownable.Holder.SetLoad(Template);
                                        slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                    }
                                }
                            }
                            else
                            {
                                ownable.Holder.SetUnload();
                                slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }
                        else if (lvi.DataContext is ArmObject)
                        {
                            var slotobj = lvi.DataContext as ArmObject;

                            int armindex = Convert.ToInt32((slotobj.Name.Split('#'))[1]);

                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.ARM, armindex);
                            IWaferOwnable ownable = attachedmodule as IWaferOwnable;

                            if (slotobj.WaferObj == null)
                            {
                                if (TOVM != null && attachedmodule != null)
                                {
                                    // Make clone data(TransferObject) from Template
                                    //TransferObject Template = TOVM.TransferObj.Clone() as TransferObject;
                                    TransferObject Template = new TransferObject();
                                    Template.Copy(TOVM.TransferObj);


                                    if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                    {
                                        Template.OriginHolder = new ModuleID(ModuleTypeEnum.ARM, armindex, "");


                                    }
                                    else
                                    {
                                        if (TOVM.OriginModuleType == ModuleTypeEnum.SLOT || TOVM.OriginModuleType == ModuleTypeEnum.CST)
                                        {
                                            int idx = (TOVM.FoupIndex * 25) + TOVM.OriginIndex;
                                            Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, idx, "");

                                        }
                                        else
                                        {
                                            Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                        }
                                    }
                                    Template.CurrPos = Template.OriginHolder;
                                    Template.CurrHolder = Template.OriginHolder;

                                    Template.Type.Value = SubstrateTypeEnum.Wafer;
                                    Template.Size.Value = loaderModule.GetDefaultWaferSize();
                                    Template.NotchType = WaferNotchTypeEnum.NOTCH;
                                    if (Template.Size.Value == SubstrateSizeEnum.INCH6)
                                    {
                                        Template.NotchType = WaferNotchTypeEnum.FLAT;
                                    }

                                    Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "ArmObject"));

                                    DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                    TransferObject deviceInfo = null;
                                    WaferSupplyMappingInfo supplyInfo = null;
                                    if (DMParam != null)
                                    {
                                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.ARM && i.WaferSupplyInfo.ID.Index == armindex);
                                    }
                                    if (supplyInfo != null)
                                    {
                                        deviceInfo = supplyInfo.DeviceInfo;
                                    }

                                    // Polish Wafer 사용되는 Holder
                                    if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                                    {
                                        Template.PolishWaferInfo.DefineName.Value = deviceInfo.PolishWaferInfo.DefineName.Value;
                                        Template.PolishWaferInfo.Size.Value = deviceInfo.PolishWaferInfo.Size.Value;
                                        Template.PolishWaferInfo.NotchType.Value = deviceInfo.PolishWaferInfo.NotchType.Value;

                                        Template.PolishWaferInfo.CurrentAngle.Value = deviceInfo.PolishWaferInfo.CurrentAngle.Value;
                                        Template.PolishWaferInfo.NotchAngle.Value = deviceInfo.PolishWaferInfo.NotchAngle.Value;
                                        Template.PolishWaferInfo.RotateAngle.Value = deviceInfo.PolishWaferInfo.RotateAngle.Value;
                                        Template.PolishWaferInfo.Priorty.Value = deviceInfo.PolishWaferInfo.Priorty.Value;

                                        Template.PolishWaferInfo.Margin.Value = deviceInfo.PolishWaferInfo.Margin.Value;
                                        Template.PolishWaferInfo.Thickness.Value = deviceInfo.PolishWaferInfo.Thickness.Value;
                                    }

                                    // Allocate
                                    ownable.Holder.SetLoad(Template);
                                    slotobj.WaferObj = Template;
                                    slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                    if (TOVM.SelectedSubsStatus == EnumSubsStatus.UNKNOWN)
                                    {
                                        ownable.Holder.SetUnknown();
                                        slotobj.WaferStatus = EnumSubsStatus.UNKNOWN;
                                    }
                                }
                            }
                            else
                            {
                                ownable.Holder.SetUnload();
                                slotobj.WaferObj = null;
                                slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }
                        else if (lvi.DataContext is BufferObject)
                        {
                            var slotobj = lvi.DataContext as BufferObject;

                            int bufferindex = Convert.ToInt32((slotobj.Name.Split('#'))[1]);

                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.BUFFER, bufferindex);
                            IWaferOwnable ownable = attachedmodule as IWaferOwnable;

                            if (slotobj.WaferObj == null)
                            {
                                if (TOVM != null && attachedmodule != null)
                                {
                                    // Make clone data(TransferObject) from Template
                                    //TransferObject Template = TOVM.TransferObj.Clone() as TransferObject;
                                    TransferObject Template = new TransferObject();
                                    Template.Copy(TOVM.TransferObj);

                                    //Template.OriginHolder = new ModuleID(ModuleTypeEnum.BUFFER, bufferindex, "");
                                    if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                    {
                                        Template.OriginHolder = new ModuleID(ModuleTypeEnum.BUFFER, bufferindex, "");
                                    }
                                    else
                                    {
                                        if (TOVM.OriginModuleType == ModuleTypeEnum.SLOT || TOVM.OriginModuleType == ModuleTypeEnum.CST)
                                        {
                                            int idx = (TOVM.FoupIndex * 25) + TOVM.OriginIndex;
                                            Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, idx, "");

                                        }
                                        else
                                        {
                                            Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                        }
                                    }
                                    Template.CurrPos = Template.OriginHolder;
                                    Template.CurrHolder = Template.OriginHolder;

                                    Template.Type.Value = SubstrateTypeEnum.Wafer;
                                    Template.Size.Value = loaderModule.GetDefaultWaferSize();
                                    Template.NotchType = WaferNotchTypeEnum.NOTCH;
                                    if (Template.Size.Value == SubstrateSizeEnum.INCH6)
                                    {
                                        Template.NotchType = WaferNotchTypeEnum.FLAT;
                                    }

                                    Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "BufferObject"));

                                    DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                    TransferObject deviceInfo = null;
                                    WaferSupplyMappingInfo supplyInfo = null;
                                    if (DMParam != null)
                                    {
                                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.BUFFER && i.WaferSupplyInfo.ID.Index == bufferindex);
                                    }
                                    if (supplyInfo != null)
                                    {
                                        deviceInfo = supplyInfo.DeviceInfo;
                                    }

                                    // Polish Wafer 사용되는 Holder
                                    if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                                    {
                                        Template.PolishWaferInfo.DefineName.Value = deviceInfo.PolishWaferInfo.DefineName.Value;
                                        Template.PolishWaferInfo.Size.Value = deviceInfo.PolishWaferInfo.Size.Value;
                                        Template.PolishWaferInfo.NotchType.Value = deviceInfo.PolishWaferInfo.NotchType.Value;

                                        Template.PolishWaferInfo.CurrentAngle.Value = deviceInfo.PolishWaferInfo.CurrentAngle.Value;
                                        Template.PolishWaferInfo.NotchAngle.Value = deviceInfo.PolishWaferInfo.NotchAngle.Value;
                                        Template.PolishWaferInfo.RotateAngle.Value = deviceInfo.PolishWaferInfo.RotateAngle.Value;
                                        Template.PolishWaferInfo.Priorty.Value = deviceInfo.PolishWaferInfo.Priorty.Value;

                                        Template.PolishWaferInfo.Margin.Value = deviceInfo.PolishWaferInfo.Margin.Value;
                                        Template.PolishWaferInfo.Thickness.Value = deviceInfo.PolishWaferInfo.Thickness.Value;
                                    }

                                    // Allocate
                                    ownable.Holder.SetLoad(Template);
                                    slotobj.WaferObj = Template;
                                    slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                    if (TOVM.SelectedSubsStatus == EnumSubsStatus.UNKNOWN) 
                                    {
                                        ownable.Holder.SetUnknown();
                                        slotobj.WaferStatus = EnumSubsStatus.UNKNOWN;
                                    }
                                }
                            }
                            else
                            {
                                ownable.Holder.SetUnload();
                                slotobj.WaferObj = null;
                                slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }
                        else if (lvi.DataContext is PAObject)
                        {
                            var slotobj = lvi.DataContext as PAObject;

                            int paindex = Convert.ToInt32((slotobj.Name.Split('#'))[1]);

                            IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.PA, paindex);
                            IWaferOwnable ownable = attachedmodule as IWaferOwnable;

                            if (slotobj.WaferObj == null)
                            {
                                if (TOVM != null && attachedmodule != null)
                                {
                                    // Make clone data(TransferObject) from Template
                                    //TransferObject Template = TOVM.TransferObj.Clone() as TransferObject;
                                    TransferObject Template = new TransferObject();
                                    Template.Copy(TOVM.TransferObj);

                                    if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                    {
                                        Template.OriginHolder = new ModuleID(ModuleTypeEnum.PA, paindex, "");
                                    }
                                    else
                                    {
                                        if (TOVM.OriginModuleType == ModuleTypeEnum.SLOT || TOVM.OriginModuleType == ModuleTypeEnum.CST)
                                        {
                                            int idx = (TOVM.FoupIndex * 25) + TOVM.OriginIndex;
                                            Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, idx, "");

                                        }
                                        else
                                        {
                                            Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                        }
                                    }
                                    Template.CurrPos = Template.OriginHolder;
                                    Template.CurrHolder = Template.OriginHolder;

                                    Template.Type.Value = SubstrateTypeEnum.Wafer;
                                    Template.Size.Value = loaderModule.GetDefaultWaferSize();
                                    Template.NotchType = WaferNotchTypeEnum.NOTCH;
                                    if (Template.Size.Value == SubstrateSizeEnum.INCH6)
                                    {
                                        Template.NotchType = WaferNotchTypeEnum.FLAT;
                                    }

                                    Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "PAObject"));

                                    DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                    TransferObject deviceInfo = null;
                                    WaferSupplyMappingInfo supplyInfo = null;
                                    if (DMParam != null)
                                    {
                                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.PA && i.WaferSupplyInfo.ID.Index == paindex);

                                        if (supplyInfo != null)
                                        {
                                            deviceInfo = supplyInfo.DeviceInfo;
                                        }

                                        // Polish Wafer 사용되는 Holder
                                        if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                                        {
                                            Template.PolishWaferInfo.DefineName.Value = deviceInfo.PolishWaferInfo.DefineName.Value;
                                            Template.PolishWaferInfo.Size.Value = deviceInfo.PolishWaferInfo.Size.Value;
                                            Template.PolishWaferInfo.NotchType.Value = deviceInfo.PolishWaferInfo.NotchType.Value;

                                            Template.PolishWaferInfo.CurrentAngle.Value = deviceInfo.PolishWaferInfo.CurrentAngle.Value;
                                            Template.PolishWaferInfo.NotchAngle.Value = deviceInfo.PolishWaferInfo.NotchAngle.Value;
                                            Template.PolishWaferInfo.RotateAngle.Value = deviceInfo.PolishWaferInfo.RotateAngle.Value;
                                            Template.PolishWaferInfo.Priorty.Value = deviceInfo.PolishWaferInfo.Priorty.Value;

                                            Template.PolishWaferInfo.Margin.Value = deviceInfo.PolishWaferInfo.Margin.Value;
                                            Template.PolishWaferInfo.Thickness.Value = deviceInfo.PolishWaferInfo.Thickness.Value;
                                        }
                                    }

                                    // Allocate
                                    ownable.Holder.SetLoad(Template);
                                    slotobj.WaferObj = Template;
                                    slotobj.WaferStatus = EnumSubsStatus.EXIST;
                                    if (TOVM.SelectedSubsStatus == EnumSubsStatus.UNKNOWN)
                                    {
                                        ownable.Holder.SetUnknown();
                                        slotobj.WaferStatus = EnumSubsStatus.UNKNOWN;
                                    }
                                }
                            }
                            else
                            {
                                ownable.Holder.SetUnload();
                                slotobj.WaferObj = null;
                                slotobj.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }
                        else if (lvi.DataContext is StageObject)
                        {
                            var stage = lvi.DataContext as StageObject;

                            int stageidx = Convert.ToInt32(((stage as StageObject).Name.Split('#'))[1]);
                            EnumWaferType type = EnumWaferType.UNDEFINED;
                            if (TOVM != null)
                            {
                                type = TOVM.SelectedType;
                            }
                            if (this.loaderCommunicationManager.Cells[stageidx - 1].StageMode == GPCellModeEnum.ONLINE || this.loaderCommunicationManager.Cells[stageidx - 1].StageMode == GPCellModeEnum.MAINTENANCE)
                            {
                                if (type == EnumWaferType.CARD)
                                {
                                    IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CC, stageidx);
                                    ICardOwnable ownable = attachedmodule as ICardOwnable;

                                    if (stage.CardStatus == EnumSubsStatus.NOT_EXIST)
                                    {
                                        if (TOVM != null && attachedmodule != null)
                                        {
                                            TransferObject Template = new TransferObject();
                                            Template.Copy(TOVM.TransferObj);

                                            if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                            {
                                                Template.OriginHolder = new ModuleID(ModuleTypeEnum.CC, stageidx, "");


                                            }
                                            else
                                            {
                                                Template.OriginHolder = new ModuleID(ModuleTypeEnum.CC, stageidx, "");
                                            }
                                            Template.CurrPos = Template.OriginHolder;
                                            Template.CurrHolder = Template.OriginHolder;

                                            Template.Type.Value = SubstrateTypeEnum.Card;
                                            //Template.Size.Value = SubstrateSizeEnum.INCH12;

                                            Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "CardTrayObject"));

                                            //DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                            //TransferObject deviceInfo = null;
                                            //WaferSupplyMappingInfo supplyInfo = null;

                                            //supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.CARDTRAY && i.WaferSupplyInfo.ID.Index == cardbufindex);

                                            //if (supplyInfo != null)
                                            //{
                                            //    deviceInfo = supplyInfo.DeviceInfo;
                                            //}

                                            // Allocate

                                            Template.WaferState = EnumWaferState.READY;
                                            Template.ProbeCardID.Value = "PC" + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute;
                                            
                                            if(loaderModule.LoaderMaster.GetClient(stageidx) != null)
                                            {
                                                ownable.Holder.SetLoad(Template);
                                                stage.CardStatus = EnumSubsStatus.EXIST;
                                                loaderModule.LoaderMaster.GetClient(stageidx).SetCardStatus(true, Template.ProbeCardID.Value, true);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (loaderModule.LoaderMaster.GetClient(stageidx) != null)
                                        {
                                            ownable.Holder.SetUnload();
                                            stage.CardStatus = EnumSubsStatus.NOT_EXIST;
                                            loaderModule.LoaderMaster.GetClient(stageidx).SetCardStatus(false, "", false);
                                        }
                                    }
                                }
                                else if (type == EnumWaferType.STANDARD || type == EnumWaferType.POLISH)
                                {

                                    int armindex = Convert.ToInt32((stage.Name.Split('#'))[1]);

                                    IAttachedModule attachedmodule = this.loaderModule.ModuleManager.FindModule(ModuleTypeEnum.CHUCK, armindex);
                                    IWaferOwnable ownable = attachedmodule as IWaferOwnable;

                                    if (stage.WaferObj == null)
                                    {
                                        if (TOVM != null && attachedmodule != null)
                                        {
                                            // Make clone data(TransferObject) from Template
                                            //TransferObject Template = TOVM.TransferObj.Clone() as TransferObject;
                                            TransferObject Template = new TransferObject();
                                            Template.Copy(TOVM.TransferObj);


                                            if (TOVM.OriginModuleType == ModuleTypeEnum.UNDEFINED)
                                            {
                                                Template.OriginHolder = new ModuleID(ModuleTypeEnum.ARM, armindex, "");


                                            }
                                            else
                                            {
                                                if (TOVM.OriginModuleType == ModuleTypeEnum.SLOT || TOVM.OriginModuleType == ModuleTypeEnum.CST)
                                                {
                                                    int idx = (TOVM.FoupIndex * 25) + TOVM.OriginIndex;
                                                    Template.OriginHolder = new ModuleID(ModuleTypeEnum.SLOT, idx, "");

                                                }
                                                else
                                                {
                                                    Template.OriginHolder = new ModuleID(TOVM.OriginModuleType, TOVM.OriginIndex, "");
                                                }
                                            }
                                            Template.CurrPos = Template.OriginHolder;
                                            Template.CurrHolder = Template.OriginHolder;

                                            Template.Type.Value = SubstrateTypeEnum.Wafer;
                                            Template.Size.Value = loaderModule.GetDefaultWaferSize();
                                            Template.NotchType = WaferNotchTypeEnum.NOTCH;
                                            if (Template.Size.Value == SubstrateSizeEnum.INCH6)
                                            {
                                                Template.NotchType = WaferNotchTypeEnum.FLAT;
                                            }
                                            Template.ID.Value = SecuritySystem.SecurityUtil.GetHashCode_SHA256((DateTime.Now.Ticks + "ChuckObject"));

                                            DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                                            TransferObject deviceInfo = null;
                                            WaferSupplyMappingInfo supplyInfo = null;

                                            if (DMParam != null)
                                            {
                                                supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == ModuleTypeEnum.ARM && i.WaferSupplyInfo.ID.Index == armindex);

                                                if (supplyInfo != null)
                                                {
                                                    deviceInfo = supplyInfo.DeviceInfo;
                                                }

                                                // Polish Wafer 사용되는 Holder
                                                if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                                                {
                                                    Template.PolishWaferInfo.DefineName.Value = deviceInfo.PolishWaferInfo.DefineName.Value;
                                                    Template.PolishWaferInfo.Size.Value = deviceInfo.PolishWaferInfo.Size.Value;
                                                    Template.PolishWaferInfo.NotchType.Value = deviceInfo.PolishWaferInfo.NotchType.Value;

                                                    Template.PolishWaferInfo.CurrentAngle.Value = deviceInfo.PolishWaferInfo.CurrentAngle.Value;
                                                    Template.PolishWaferInfo.NotchAngle.Value = deviceInfo.PolishWaferInfo.NotchAngle.Value;
                                                    Template.PolishWaferInfo.RotateAngle.Value = deviceInfo.PolishWaferInfo.RotateAngle.Value;
                                                    Template.PolishWaferInfo.Priorty.Value = deviceInfo.PolishWaferInfo.Priorty.Value;

                                                    Template.PolishWaferInfo.Margin.Value = deviceInfo.PolishWaferInfo.Margin.Value;
                                                    Template.PolishWaferInfo.Thickness.Value = deviceInfo.PolishWaferInfo.Thickness.Value;
                                                }
                                            }
                                            // Allocate
                                            ownable.Holder.SetLoad(Template);
                                            stage.WaferObj = Template;
                                            stage.WaferStatus = EnumSubsStatus.EXIST;
                                        }
                                    }
                                    else
                                    {
                                        ownable.Holder.SetUnload();
                                        stage.WaferObj = null;
                                        stage.WaferStatus = EnumSubsStatus.NOT_EXIST;
                                    }
                                }

                            }

                        }
                    }

                    LoaderModule.BroadcastLoaderInfo(false);
                    //Console.WriteLine("Mouse Up : " + mouseparam.GetPosition((IInputElement)mouseparam.Source));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ExecuteMouseUp(): Error occurred. Err = {err.Message}");
            }
        }

        //public Point LastMouseRightUpPos = default(Point);

        private int GetCurrentIndex(ListView listview)
        {
            int index = -1;
            for (int i = 0; i < listview.Items.Count; ++i)
            {
                ListViewItem item = null;

                if (listview.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                {
                    item = null;
                }
                else
                {
                    item = listview.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                }

                if (item == null)
                {
                    continue;
                }

                //if (this.IsMouseOverTarget(item, LastMouseRightUpPos))
                //{
                //    index = i;
                //    break;
                //}
            }

            return index;
        }

        private bool IsMouseOverTarget(Visual target, Point mousePos)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            //Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        private FoupObject _SelectedFoup;
        public FoupObject SelectedFoup
        {
            get { return _SelectedFoup; }
            set
            {
                if (value != _SelectedFoup)
                {
                    if (value is null)
                    {
                        FoupScanEnable = false;
                        FoupLoadEnable = false;
                        FosBLoadEnable = false;
                        FoupUnloadEnable = false;
                        FosBUnloadEnable = false;
                    }
                    else if (value.Enable == false)
                    {
                        FoupScanEnable = false;
                        FoupLoadEnable = false;
                        FoupUnloadEnable = false;
                        FosBLoadEnable = false;
                        FosBUnloadEnable = false;
                        FoupEventEnable = false;
                    }
                    else if (value.State == FoupStateEnum.LOAD)
                    {
                        FoupScanEnable = true;
                        FoupLoadEnable = false;
                        FosBLoadEnable = false;
                        FoupUnloadEnable = true;
                        FosBUnloadEnable = true;
                        FoupEventEnable = false;
                    }
                    else if (value.State == FoupStateEnum.UNLOAD)
                    {
                        FoupScanEnable = false;
                        FoupLoadEnable = true;
                        FosBLoadEnable = true;
                        FoupUnloadEnable = false;
                        FosBUnloadEnable = false;
                        FoupEventEnable = true;
                    }
                    else if (value.State == FoupStateEnum.ERROR || value.State == FoupStateEnum.LOADING || value.State == FoupStateEnum.UNLOADING)
                    {
                        FoupScanEnable = false;
                        FoupLoadEnable = true;
                        FosBLoadEnable = true;
                        FoupUnloadEnable = true;
                        FosBUnloadEnable = true;
                        FoupEventEnable = false;
                    }

                    if (IsFosbActive)
                    {
                        FoupLoadEnable = false;
                        FoupUnloadEnable = false;
                    }
                    else
                    {
                        FosBLoadEnable = false;
                        FosBUnloadEnable = false;
                    }
                    _SelectedFoup = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void AllCstBtnDisable()
        {
            FoupScanEnable = false;
            FoupLoadEnable = false;
            FoupUnloadEnable = false;
            FosBLoadEnable = false;
            FosBUnloadEnable = false;
            FoupEventEnable = false;
        }
        public void UpdateCSTBtnEnable()
        {
            Thread.Sleep(100);
            if (SelectedFoup is null)
            {
                FoupScanEnable = false;
                FoupLoadEnable = false;
                FoupUnloadEnable = false;
                FosBLoadEnable = false;
                FosBUnloadEnable = false;
                FoupEventEnable = false;
            }
            else if(SelectedFoup.Enable == false)
            {
                FoupScanEnable = false;
                FoupLoadEnable = false;
                FoupUnloadEnable = false;
                FosBLoadEnable = false;
                FosBUnloadEnable = false;
            }
            else if (SelectedFoup.State == FoupStateEnum.LOAD)
            {
                FoupScanEnable = true;
                FoupLoadEnable = false;
                FoupUnloadEnable = true;
                FosBLoadEnable = false;
                FosBUnloadEnable = true;
                FoupEventEnable = false;
            }
            else if (SelectedFoup.State == FoupStateEnum.UNLOAD)
            {
                FoupScanEnable = false;
                FoupLoadEnable = true;
                FoupUnloadEnable = false;
                FosBLoadEnable = true;
                FosBUnloadEnable = false;
                FoupEventEnable = true;
            }
            else if (SelectedFoup.State == FoupStateEnum.ERROR)
            {
                FoupScanEnable = false;
                FoupLoadEnable = true;
                FoupUnloadEnable = true;
                FosBLoadEnable = true;
                FosBUnloadEnable = true;
                FoupEventEnable = false;
            }

            if (IsFosbActive)
            {
                FoupLoadEnable = false;
                FoupUnloadEnable = false;
            }
            else
            {
                FosBLoadEnable = false;
                FosBUnloadEnable = false;
            }
        }
        private AsyncCommand _CSTLoadCommand;
        public ICommand CSTLoadCommand
        {
            get
            {
                if (null == _CSTLoadCommand) _CSTLoadCommand = new AsyncCommand(CSTLoadFunc);
                return _CSTLoadCommand;
            }
        }
        private async Task CSTLoadFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    try
                    {
                        if (SelectedFoup == null)
                            return;

                        AllCstBtnDisable();
                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);

                        // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                        var e84controller = this.E84Module().GetE84Controller(foupindex, E84OPModuleTypeEnum.FOUP);
                        if (e84controller != null)
                        {
                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                            {
                                if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);
                                    UpdateCSTBtnEnable();
                                    return;
                                }
                                else
                                {
                                    LoggerManager.Debug($"CSTLoadFunc() Foup#{foupindex}. e84controller runmode is {e84controller.CommModule.RunMode}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"LoadCSTLoadFunc Foup#{foupindex}. e84controller connection state is {e84controller.CommModule.Connection}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"CSTLoadFunc() Foup#{foupindex}. e84controller is null.");
                        }

                        var modules = this.loaderModule.ModuleManager;
                        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        //  this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                        this.LoaderMaster.ActiveLotInfos[Cassette.ID.Index - 1].FoupLoad();
                        UpdateCSTBtnEnable();

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //return Task.Run(() =>
            //{
            //    try
            //    {
            //        if (SelectedFoup == null)
            //            return;
            //        AllCstBtnDisable();
            //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
            //        var modules = this.loaderModule.ModuleManager;
            //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
            //        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
            //        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
            //        UpdateCSTBtnEnable();
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
            //    }
            //});
        }
        private AsyncCommand _FosBLoadCommand;
        public ICommand FosBLoadCommand
        {
            get
            {
                if (null == _FosBLoadCommand) _FosBLoadCommand = new AsyncCommand(FosBLoadFunc);
                return _FosBLoadCommand;
            }
        }
        private async Task FosBLoadFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                Task task = new Task(() =>
                {
                    try
                    {
                        if (SelectedFoup == null)
                            return;
                        AllCstBtnDisable();
                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
                        var modules = this.loaderModule.ModuleManager;
                        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FosB_LoadCommand());
                        UpdateCSTBtnEnable();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            //return Task.Run(() =>
            //{
            //    try
            //    {
            //        if (SelectedFoup == null)
            //            return;
            //        AllCstBtnDisable();
            //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
            //        var modules = this.loaderModule.ModuleManager;
            //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
            //        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
            //        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
            //        UpdateCSTBtnEnable();
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
            //    }
            //});
        }
        //return Task.Run(() =>
        //{
        //    try
        //    {
        //        if (SelectedFoup == null)
        //            return;
        //        AllCstBtnDisable();
        //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
        //        var modules = this.loaderModule.ModuleManager;
        //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
        //        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
        //        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
        //        UpdateCSTBtnEnable();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
        //    }
        //});


        public void UpdateChanged()
        {
            try
            {
                UpdateCSTBtnEnable();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"UpdateChanged(): Exception occurred. Err = {err.Message}");
            }
        }

        private AsyncCommand _CSTUnloadCommand;
        public ICommand CSTUnloadCommand
        {
            get
            {
                if (null == _CSTUnloadCommand) _CSTUnloadCommand = new AsyncCommand(CSTUnloadFunc);
                return _CSTUnloadCommand;
            }
        }

        private AsyncCommand _RisingFoupStateEventCommand;
        public ICommand RisingFoupStateEventCommand
        {
            get
            {
                if (null == _RisingFoupStateEventCommand) _RisingFoupStateEventCommand = new AsyncCommand(RisingFoupStateEventFunc);
                return _RisingFoupStateEventCommand;
            }
        }

        private async Task RisingFoupStateEventFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    try
                    {
                        if (SelectedFoup == null)
                            return;

                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);

                        var e84controller = this.E84Module().GetE84Controller(foupindex, E84OPModuleTypeEnum.FOUP);
                        if (e84controller != null)
                        {
                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                            {
                                if (e84controller.CommModule.RunMode != E84Mode.AUTO)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Error Message",
                                        $"Manual Rising Event cannot be operated when E84 is in {e84controller.CommModule.RunMode} Mode." +
                                        "\nOperate after changing to Auto Mode.", EnumMessageStyle.Affirmative);
                                    UpdateCSTBtnEnable();
                                    return;
                                }
                                else
                                {
                                    LoggerManager.Debug($"RisingFoupStateEventFunc(): Foup#{foupindex}. e84controller runmode is {e84controller.CommModule.RunMode}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"RisingFoupStateEventFunc(): Foup#{foupindex}. e84controller connection state is {e84controller.CommModule.Connection}");
                                return;
                            }

                        }

                        //UpdateFoupState, rising forced event 
                        var foupcontroller = this.FoupOpModule().GetFoupController(foupindex);
                        foupcontroller.Service.FoupModule.UpdateFoupState(forced_event: true);
                        UpdateCSTBtnEnable();

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task CSTUnloadFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (SelectedFoup == null)
                        return;
                    try
                    {
                        AllCstBtnDisable();
                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);

                        // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                        var e84controller = this.E84Module().GetE84Controller(foupindex, E84OPModuleTypeEnum.FOUP);
                        if (e84controller != null)
                        {
                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                            {
                                if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);
                                    UpdateCSTBtnEnable();
                                    return;
                                }
                                else
                                {
                                    LoggerManager.Debug($"CSTUnloadFunc() Foup#{foupindex}. e84controller runmode is {e84controller.CommModule.RunMode}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"CSTUnloadFunc() Foup#{foupindex}. e84controller connection state is {e84controller.CommModule.Connection}");                                
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"CSTUnloadFunc() Foup#{foupindex}. e84controller is null.");
                        }

                        var modules = this.loaderModule.ModuleManager;
                        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        this.LoaderMaster.ActiveLotInfos[Cassette.ID.Index - 1].FoupUnLoad();
                        UpdateCSTBtnEnable();

                        if (LoaderMaster.ActiveLotInfos[Cassette.ID.Index - 1] != null)
                        {
                            if (LoaderMaster.ActiveLotInfos[Cassette.ID.Index - 1].IsActiveFromHost)
                            {
                                LoaderMaster.ActiveLotInfos[Cassette.ID.Index - 1].IsActiveFromHost = false;
                                LoggerManager.Debug($"CassetteNormalUnload Foup#{Cassette.ID.Index} IsActiveFormHost change to false");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private AsyncCommand _FOSBUnloadCommand;
        public ICommand FOSBUnloadCommand
        {
            get
            {
                if (null == _FOSBUnloadCommand) _FOSBUnloadCommand = new AsyncCommand(FOSBUnloadFunc);
                return _FOSBUnloadCommand;
            }
        }

        private async Task FOSBUnloadFunc()
        {
            try
            {
                //FosBLoadFunc();
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                Task task = new Task(() =>
                {
                    if (SelectedFoup == null)
                        return;
                    try
                    {
                        AllCstBtnDisable();
                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);

                        // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                        var e84controller = this.E84Module().GetE84Controller(foupindex, E84OPModuleTypeEnum.FOUP);
                        if (e84controller != null)
                        {
                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                            {
                                if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);
                                    UpdateCSTBtnEnable();
                                    return;
                                }
                                else
                                {
                                    LoggerManager.Debug($"CSTUnloadFunc() Foup#{foupindex}. e84controller runmode is {e84controller.CommModule.RunMode}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"CSTUnloadFunc() Foup#{foupindex}. e84controller connection state is {e84controller.CommModule.Connection}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"CSTUnloadFunc() Foup#{foupindex}. e84controller is null.");
                        }

                        var modules = this.loaderModule.ModuleManager;
                        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FosB_UnloadCommand());
                        UpdateCSTBtnEnable();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            //return Task.Run(() =>
            //{
            //    if (SelectedFoup == null)
            //        return;
            //    try
            //    {
            //        AllCstBtnDisable();
            //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
            //        var modules = this.loaderModule.ModuleManager;
            //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
            //        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
            //        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
            //        UpdateCSTBtnEnable();
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
            //    }
            //});
        }
        //return Task.Run(() =>
        //{
        //    if (SelectedFoup == null)
        //        return;
        //    try
        //    {
        //        AllCstBtnDisable();
        //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
        //        var modules = this.loaderModule.ModuleManager;
        //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
        //        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
        //        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
        //        UpdateCSTBtnEnable();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
        //    }
        //});


        private AsyncCommand _CSTScanCommand;
        public ICommand CSTScanCommand
        {
            get
            {
                if (null == _CSTScanCommand) _CSTScanCommand = new AsyncCommand(CSTScanFunc);
                return _CSTScanCommand;
            }
        }

        private async Task CSTScanFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    try
                    {
                        if (SelectedFoup == null)
                            return;

                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
                        // Check Foup Mode
                        var foupcontroller = this.FoupOpModule().GetFoupController(foupindex);
                        if (foupcontroller.FoupModuleInfo.Enable)
                        {
                            var modules = this.loaderModule.ModuleManager;
                            var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);

                            loaderModule.ScanCount = 25;
                            bool isEnableScan = true;
                            if (LoaderMaster.ActiveLotInfos[foupindex - 1].State == LotStateEnum.Running)
                            {
                                var ret = this.MetroDialogManager().ShowMessageDialog("Scan Confirm", $"Current Foup{foupindex} is LOT Running, Are you sure you want to Scan?", EnumMessageStyle.AffirmativeAndNegative);
                                if (ret.Result == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    isEnableScan = true;
                                    LoggerManager.Debug($"Current Foup{foupindex} is LOT Running Scan. Affirmative");
                                }
                                else
                                {
                                    isEnableScan = false;
                                    LoggerManager.Debug($"Current Foup{foupindex} is LOT Running Scan. Cancle");
                                }
                            }
                            if (isEnableScan)
                            {
                                AllCstBtnDisable();

                                Cassette.SetNoReadScanState();
                                bool scanWaitFlag = false;

                                for (int i = 0; i < loaderModule.ScanCount; i++)
                                {
                                    // Manual Lot 의 선택된 slot 값 초기화 (IsPreSelected)
                                    LoaderModule.Foups[foupindex - 1].Slots[i].IsPreSelected = false;
                                }

                                if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.NONE)
                                {
                                    Cassette.Device.AllocateDeviceInfo.OCRDevParam = new OCRDevParameter();
                                    var retVal = loaderModule.DoScanJob(foupindex);
                                    if (retVal.Result == EventCodeEnum.NONE)
                                    {
                                        scanWaitFlag = true;
                                    }
                                }
                                while (scanWaitFlag)
                                {

                                    if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.READ)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(10);
                                }
                                UpdateCSTBtnEnable();
                                // close foup cover func 
                                if (loaderModule.LoaderMaster.GetIsAlwaysCloseFoupCover())
                                {
                                    LoaderModule.CloseFoupCoverFunc(Cassette, true);
                                }                                
                            }                            
                        }                        
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
                

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //return Task.Run(() =>
            //{
            //    try
            //    {
            //        if (SelectedFoup == null)
            //            return;
            //        AllCstBtnDisable();
            //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);

            //        var modules = this.loaderModule.ModuleManager;
            //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
            //        loaderModule.ScanCount = 25;

            //        Cassette.SetNoReadScanState();
            //        bool scanWaitFlag = false;

            //        if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.NONE)
            //        {
            //            var retVal = loaderModule.DoScanJob(foupindex);
            //            if (retVal.Result == EventCodeEnum.NONE)
            //            {
            //                scanWaitFlag = true;
            //            }
            //        }
            //        while (scanWaitFlag)
            //        {

            //            if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.READ)
            //            {
            //                break;
            //            }
            //            Thread.Sleep(10);
            //        }
            //        UpdateCSTBtnEnable();
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
            //    }
            //});

        }

        private IGPLoader GPLoader
        {
            get { return _Container.Resolve<IGPLoader>(); }
        }
        //private AsyncCommand _InitSysCommand;
        //public ICommand InitSysCommand
        //{
        //    get
        //    {
        //        if (null == _InitSysCommand) _InitSysCommand = new AsyncCommand(InitSysMethod);
        //        return _InitSysCommand;
        //    }
        //}


        //private Task InitSysMethod()
        //{
        //    return Task.Run(() =>
        //    {
        //        try
        //        {
        //            loaderModule.LoaderService.UpdateLoaderSystem();
        //            ((IGPLoaderCommands)GPLoader).InitRobot();
        //        }
        //        catch (Exception err)
        //        {
        //            LoggerManager.Error($"InitSysMethod(): Error occurred. Err = {err.Message}");
        //        }

        //    });
        //}

        //private Task InitSysMethod()
        //{
        //    return Task.Run(() =>
        //    {
        //        try
        //        {
        //            loaderModule.LoaderService.UpdateLoaderSystem();
        //            ((IGPLoaderCommands)GPLoader).InitRobot();
        //        }
        //        catch (Exception err)
        //        {
        //            LoggerManager.Error($"InitSysMethod(): Error occurred. Err = {err.Message}");
        //        }

        //    });
        //}

        private AsyncCommand _CollectWaferCommand;
        public ICommand CollectWaferCommand
        {
            get
            {
                if (null == _CollectWaferCommand) _CollectWaferCommand = new AsyncCommand(CollectWaferMethod);
                return _CollectWaferCommand;
            }
        }


        private async Task CollectWaferMethod()
        {
            try
            {
                EventCodeEnum errorCode = EventCodeEnum.NONE;
                var retVal = await (this).MetroDialogManager().ShowMessageDialog("Collect All Wafer", "Do you Want Collect All Wafer?", EnumMessageStyle.AffirmativeAndNegative);
                string msg = "";
                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    (errorCode, msg) = await this.LoaderMaster.CollectAllWafer();
                    if (!(errorCode == EventCodeEnum.NONE))
                    {
                        retVal = await (this).MetroDialogManager().ShowMessageDialog("Collect All Wafer Error", msg, EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //return Task.Run(() =>
            //{
            //    try
            //    {
            //        EventCodeEnum errorCode = EventCodeEnum.NONE;
            //        var retVal = (this).MetroDialogManager().ShowMessageDialog("Collect All Wafer", "Do you Want Collect All Wafer?", EnumMessageStyle.AffirmativeAndNegative).Result;
            //        string msg = "";
            //        if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
            //        {
            //            errorCode = this.LoaderMaster.CollectAllWafer(out msg);
            //            if (!(errorCode == EventCodeEnum.NONE))
            //            {
            //                retVal = (this).MetroDialogManager().ShowMessageDialog("Collect All Wafer Error", msg, EnumMessageStyle.Affirmative).Result;
            //            }
            //        }
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Exception(err);
            //    }
            //});
        }

        private AsyncCommand _WaferRefreshCommand;
        public ICommand WaferRefreshCommand
        {
            get
            {
                if (null == _WaferRefreshCommand) _WaferRefreshCommand = new AsyncCommand(WaferRefreshMethod);
                return _WaferRefreshCommand;
            }
        }

        private async Task WaferRefreshMethod()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                Task task = new Task(() =>
                {
                    try
                    {
                        EventCodeEnum errorCode = EventCodeEnum.NONE;
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Wafer Refresh", "Do you Want Wafer Refresh?", EnumMessageStyle.AffirmativeAndNegative).Result;
                        string msg = "";
                        if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            if (LoaderMaster.ModuleState.State == ModuleStateEnum.IDLE || LoaderMaster.ModuleState.State == ModuleStateEnum.DONE)
                            {
                                errorCode = this.LoaderMaster.Loader.ModuleInit(true);
                                if (!(errorCode == EventCodeEnum.NONE))
                                {
                                    retVal = (this).MetroDialogManager().ShowMessageDialog("Wafer Refresh Error", msg, EnumMessageStyle.Affirmative).Result;
                                }

                                var ListOfUnknownWafers = loaderModule.GetLoaderInfo().StateMap.GetUnknownTransferObjectAll().Where(item => item != null && item.Type.Value == SubstrateTypeEnum.Wafer).ToList();
                                if (ListOfUnknownWafers != null && ListOfUnknownWafers.Count > 0) 
                                {
                                    string UnknownWafers = string.Join(", ", ListOfUnknownWafers.Select(item =>
                                    {
                                        string moduleType = item.PrevHolder != null ? item.PrevHolder.ModuleType.ToString() : "Unknown";
                                        string index = item.PrevHolder != null ? item.PrevHolder.Index.ToString() : null ;
                                        if (moduleType == "SLOT")
                                        {
                                            int slotNum = item.PrevHolder.Index % 25;
                                            int offset = 0;
                                            if (slotNum == 0)
                                            {
                                                slotNum = 25;
                                                offset = -1;
                                            }
                                            index = "Foup" + ((item.PrevHolder.Index + offset) / 25) + 1 + slotNum.ToString();
                                        }
                                        return $"{moduleType}{index}";
                                    }));

                                    msg = $"There exists a module with an unknown status for the wafer. Please check the status.\n{UnknownWafers}";
                                    retVal = (this).MetroDialogManager().ShowMessageDialog("Wafer Refresh Warning", msg, EnumMessageStyle.Affirmative).Result;
                                }
                            }
                            else
                            {
                                retVal = (this).MetroDialogManager().ShowMessageDialog("Wafer Refresh Error", $"Can`t not Wafer Refresh. Loader is {LoaderMaster.ModuleState.State} State", EnumMessageStyle.Affirmative).Result;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            //return Task.Run(() =>
            //{
            //    ((IGPLoaderCommands)GPLoader).HomingRobot();
            //});
        }

        //private static Mutex mtx = null;

        private AsyncCommand<object> _ShowFoupStateSettingViewCommand;
        public ICommand ShowFoupStateSettingViewCommand
        {
            get
            {
                if (null == _ShowFoupStateSettingViewCommand) _ShowFoupStateSettingViewCommand = new AsyncCommand<object>(ShowFoupStateSettingViewCommandFunc);
                return _ShowFoupStateSettingViewCommand;
            }
        }

        private async Task ShowFoupStateSettingViewCommandFunc(object param)
        {
            try
            {
                var foupcontroller = this.FoupOpModule().GetFoupController((int)param + 1);
                var foupobj = LoaderModule.Foups[(int)param];
                string windowName = $"Foup #{((int)param + 1)} Setting View";
                bool bFindWinow = Win32APIWrapper.CheckWindowExists(windowName);
                if (!bFindWinow && foupcontroller != null)
                {
                    FoupModuleInfo moduleinfo = foupcontroller.FoupModuleInfo;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Window container = new Window();
                        FoupSubSettingView foupSubSettingView = new FoupSubSettingView();
                        foupSubSettingView.DataContext = new FoupSubSettingViewModel(LoaderMaster, foupcontroller, container);
                        container.Content = foupSubSettingView;
                        container.Width = 360;
                        container.Height = 440;
                        container.WindowStyle = WindowStyle.None;
                        container.Title = windowName;
                        container.Topmost = true;
                        container.VerticalAlignment = VerticalAlignment.Center;
                        container.HorizontalAlignment = HorizontalAlignment.Center;
                        container.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        container.Show();
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private AsyncCommand _PARepeatTestCommand;
        public ICommand PARepeatTestCommand
        {
            get
            {
                if (null == _PARepeatTestCommand) _PARepeatTestCommand = new AsyncCommand(PARepeatTestCommandFunc, false);
                return _PARepeatTestCommand;
            }
        }

        private async Task PARepeatTestCommandFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            bool stageBusy = true;
            try
            {


                for (int i = 0; i < 100000; i++)
                {
                    if (CancelRepeat == true)
                    {
                        await TransferObjectTestFunc_OPRT(TransferSource, TransferTarget);  // Source -> Target
                        if (isRepeatError)
                        {
                            LoggerManager.Debug($"[Repeat Test] Transfer Done. Progressing...({i})");
                            break;
                        }

                        while (true && TransferTarget is StageObject)
                        {
                            stageBusy = LoaderMaster.GetClient((TransferTarget as StageObject).Index).GetRunState(true);

                            if (stageBusy)
                            {
                                break;
                            }

                            Thread.Sleep(100);
                        }
                        await TransferObjectTestFunc_OPRT(TransferTarget, TransferSource); // Target -> Source
                        if (isRepeatError)
                        {
                            LoggerManager.Debug($"[Repeat Test] Transfer Done. Progressing...({i})");
                            break;
                        }
                        //if (CancelTransferTokenPack.TokenSource != null)
                        //{
                        //    if (CancelTransferTokenPack.TokenSource.IsCancellationRequested == true)
                        //    {
                        //        CancelTransferTokenPack.TokenSource.Dispose();
                        //        CancelTransferTokenPack.TokenSource = null;
                        //        break;
                        //    }
                        //}
                        LoggerManager.Debug($"[Repeat Test] Transfer Done. Progressing...({i})");
                    }
                    else
                    {
                        break;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                UpdateCSTBtnEnable();
                UpdateCSTBtnEnable();
                TransferTarget = null;
                isClickedTransferTarget = false;
                SetAllObjectEnable();
                IsSelected = false;
                targetToggle = true;
                isClickedTransferSource = false;
                sourceToggle = true;
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        bool isRepeatError = false;

        //TODO Remove
        private async Task TransferObjectTestFunc_OPRT(object source, object target)
        {
            try
            {

                Task task = new Task(() =>
                {
                    var loader = loaderModule;
                    Map = loader.GetLoaderInfo().StateMap;

                    string sourceId = null;
                    ModuleID targetId = new ModuleID();
                    object transfermodule = null;
                    //Source Info
                    SetTransferInfoToModule(source, ref transfermodule, true);
                    if (transfermodule == null)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Source Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    sourceId = (string)transfermodule;
                    //Target Info
                    SetTransferInfoToModule(target, ref transfermodule, false);
                    if (transfermodule == null)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Destination Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    targetId = (ModuleID)transfermodule;

                    if (sourceId == null | targetId == null)
                        return;

                    TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == sourceId).FirstOrDefault();
                    ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == targetId).FirstOrDefault();

                    // Fixed Tray 또는 Inspection Tray 에서 Chuck으로 보내는 경우, Polish Wafer 설정이 되어 있을 때
                    if ((subObj.CurrPos.ModuleType == ModuleTypeEnum.FIXEDTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK) ||
                         (subObj.CurrPos.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK))
                    {
                        // DeviceManager로부터 Source의 데이터가 Polish Wafer로 설정되어 있는 Holder인지 알아온다.

                        DeviceManagerParameter DMParam = this.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                        TransferObject deviceInfo = null;
                        WaferSupplyMappingInfo supplyInfo = null;

                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == subObj.CurrPos.ModuleType && i.WaferSupplyInfo.ID.Index == subObj.CurrPos.Index);

                        if (supplyInfo != null)
                        {
                            deviceInfo = supplyInfo.DeviceInfo;
                        }

                        // Polish Wafer 사용되는 Holder
                        if (deviceInfo?.WaferType.Value == EnumWaferType.POLISH)
                        {
                            if (subObj.WaferType.Value != EnumWaferType.POLISH)
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer", $"Fixed tray's wafer is not a polish wafer. \nPlease proceed to the polish wafer setup.", EnumMessageStyle.Affirmative).Result;

                                return;

                                //if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                                //{

                                //}
                            }
                            else
                            {
                                // Polish Wafer로 설정은 되어 있음, 파라미터 유효성 검사

                                bool paramvalid = false;

                                if (subObj.PolishWaferInfo.DefineName == null || subObj.PolishWaferInfo.DefineName.Value == string.Empty || subObj.PolishWaferInfo.DefineName.Value == null)
                                {
                                    paramvalid = false;
                                }
                                else
                                {
                                    paramvalid = true;
                                }

                                // 유효성 검사 실패 시
                                if (paramvalid == false)
                                {
                                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer", $"Polish Wafer Information is not enough\nPlease check to the polish wafer parameters.", EnumMessageStyle.Affirmative).Result;

                                    return;
                                }
                            }
                        }
                    }

                    // CC인경우 (Loading prober card, Unloading prober card )저온인지 온도 체크
                    var ccUnload = subObj.CurrPos.ModuleType == ModuleTypeEnum.CC &
                    (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER
                        | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDTRAY);
                    var ccLoad = dstLoc.ID.ModuleType == ModuleTypeEnum.CC &
                    (subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDARM | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDBUFFER
                       | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDTRAY);
                    if (ccUnload | ccLoad)
                    {
                        int stageidx = -1;
                        if (ccUnload)
                        {
                            stageidx = (source as StageObject).Index;
                        }
                        else if (ccLoad)
                        {
                            stageidx = (target as StageObject).Index;
                        }

                        // CC Temp - 추후 파라미터 처리 필요 
                        //if (stageidx != -1)
                        //{
                        //    var tempClient = loaderCommunicationManager.GetTempControllerClient(stageidx);
                        //    if (tempClient != null)
                        //    {
                        //        //stage 온도 확인 : 저온이면 Ambiment 온도로 올린뒤 다시 동작하도록 한다.
                        //        var temp = tempClient.GetTemperature();
                        //        if (temp < 0)
                        //        {
                        //            var msgRet = await this.MetroDialogManager().ShowMessageDialog("Error Message",
                        //                $"Card Change operation is possible only at room temperature." +
                        //                $" A temperature of the selected stage is {temp}℃." +
                        //                $"Press OK to automatically set the temperature to a safe temperature and you can try again when it reaches normal temperature." +
                        //                $"Or direct fixes can be made in the Device Temperature set." +
                        //                $" After the card change, you must change the temperature to its original temperature.",
                        //                EnumMessageStyle.AffirmativeAndNegative);
                        //            if(msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                        //            {
                        //                tempClient.SetAmbientTemp();
                        //            }
                        //            return;
                        //        }
                        //    }
                        //}
                    }
                    //=========================================================

                    EventCodeEnum Dret = EventCodeEnum.UNDEFINED;
                    SetTransfer(subObj, dstLoc, ref Dret);

                    var mapSlicer = new LoaderMapSlicer();
                    var slicedMap = mapSlicer.ManualSlicing(Map);

                    if (slicedMap != null)
                    {
                        AllCstBtnDisable();

                        isRepeatError = false;
                        for (int i = 0; i < slicedMap.Count; i++)
                        {
                            loaderModule.SetRequest(slicedMap[i]);

                            while (true)
                            {
                                if (loader.ModuleState == ModuleStateEnum.DONE)
                                {
                                    loader.ClearRequestData();

                                    LoaderMapConvert(loader.GetLoaderInfo().StateMap);

                                    Thread.Sleep(100);

                                    break;
                                }
                                else if (loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    LoaderRecoveryControlVM.Show(_Container, loader.ResonOfError, loader.ErrorDetails);
                                    loader.ResonOfError = "";
                                    loader.ErrorDetails = "";
                                    isRepeatError = true;
                                    break;
                                }

                                Thread.Sleep(100);
                            }
                            if (isRepeatError)
                            {
                                break;
                            }

                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(33);
                    }
                    else
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"This is an incorrect operation.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    }
                });
                task.Start();
                await task;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

}
